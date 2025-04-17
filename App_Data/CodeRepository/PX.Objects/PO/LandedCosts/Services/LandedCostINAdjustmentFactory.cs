/* ---------------------------------------------------------------------*
*                             Acumatica Inc.                            *

*              Copyright (c) 2005-2023 All rights reserved.             *

*                                                                       *

*                                                                       *

* This file and its contents are protected by United States and         *

* International copyright laws.  Unauthorized reproduction and/or       *

* distribution of all or any portion of the code contained herein       *

* is strictly prohibited and will result in severe civil and criminal   *

* penalties.  Any violations of this copyright will be prosecuted       *

* to the fullest extent possible under law.                             *

*                                                                       *

* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *

* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *

* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY ACUMATICA PRODUCT.       *

*                                                                       *

* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *

* --------------------------------------------------------------------- */

using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.IN.InventoryRelease;

namespace PX.Objects.PO.LandedCosts
{
	public class LandedCostINAdjustmentFactory
	{
		private readonly PXGraph _pxGraph;

		public LandedCostINAdjustmentFactory(PXGraph pxGraph)
		{
			_pxGraph = pxGraph;
		}

		public virtual IDictionary<POLandedCostDetail, INAdjustmentWrapper> CreateLandedCostAdjustments(POLandedCostDoc doc, IEnumerable<LandedCostAllocationService.POLandedCostReceiptLineAdjustment> adjustments)
		{
			var result = new Dictionary<POLandedCostDetail, INAdjustmentWrapper>();

			var adjustmentsGroups = adjustments.Where(t=>t.LandedCostDetail != null).GroupBy(t => t.LandedCostDetail);

			foreach (var adjustmentsGroup in adjustmentsGroups)
			{
				var landedCostCode = GetLandedCostCode(adjustmentsGroup.Key.LandedCostCodeID);

				if (landedCostCode.AllocationMethod == LandedCostAllocationMethod.None)
					continue;

				var inTransctions = CreateTransactions(doc, adjustmentsGroup.Key, adjustmentsGroup.AsEnumerable());

				var newDoc = new INRegister
				{
					DocType = INDocType.Adjustment,
					OrigModule = BatchModule.PO,
					SiteID = null,
					TranDate = doc.DocDate,
					FinPeriodID = doc.FinPeriodID,
					BranchID = doc.BranchID,
					Hold = false
				};

				var resultItem = new INAdjustmentWrapper(newDoc, inTransctions);

				result.Add(adjustmentsGroup.Key, resultItem);
			}

			return result;
		}

		protected virtual INTran[] CreateTransactions(POLandedCostDoc doc, POLandedCostDetail landedCostDetail, IEnumerable<LandedCostAllocationService.POLandedCostReceiptLineAdjustment> pOLinesToProcess)
		{
			var result = new List<INTran>();

			var landedCostCode = GetLandedCostCode(landedCostDetail.LandedCostCodeID);
			var reasonCode = landedCostCode.ReasonCode;

			foreach (LandedCostAllocationService.POLandedCostReceiptLineAdjustment poreceiptline in pOLinesToProcess)
			{
				INTran intran = new INTran();

				INTran origtran = LandedCostAllocationService.Instance.GetOriginalInTran(_pxGraph, poreceiptline.ReceiptLine.ReceiptType, poreceiptline.ReceiptLine.ReceiptNbr, poreceiptline.ReceiptLine.LineNbr);
				bool isDropShip = POLineType.IsDropShip(poreceiptline.ReceiptLine.LineType);

				// There is no INTran for non-stock if 'Update GL' setting is off.
				if (POLineType.IsNonStockNonServiceNonDropShip(poreceiptline.ReceiptLine.LineType) && origtran == null)
				{
					INSetup inSetup = PXSelectReadonly<INSetup>.Select(_pxGraph);
					if (inSetup.UpdateGL.Value == false)
						continue;
				}

				if (!isDropShip && origtran == null)
					throw new PXException(AP.Messages.CannotFindINReceipt, poreceiptline.ReceiptLine.ReceiptNbr);

				//Drop-Ships are considered non-stocks
				if (POLineType.IsStockNonDropShip(poreceiptline.ReceiptLine.LineType))
				{
					intran.TranType = INTranType.ReceiptCostAdjustment;
				}
				else
				{
					//Landed cost and PPV for non-stock items are handled in special way in the inventory.
					//They should create a GL Batch, but for convinience and unforminty this functionality is placed into IN module
					//Review this part when the functionality is implemented in IN module
					intran.IsCostUnmanaged = true;
					intran.TranType = INTranType.Adjustment;
					intran.InvtMult = 0;
				}
				intran.InventoryID = poreceiptline.ReceiptLine.InventoryID;
				intran.SubItemID = poreceiptline.ReceiptLine.SubItemID;
				intran.SiteID = poreceiptline.ReceiptLine.SiteID;
				intran.BAccountID = doc.VendorID;
				intran.BranchID = landedCostDetail.BranchID;


				if (isDropShip && intran.SiteID != null)
				{
					INSite invSite = INSite.PK.Find(_pxGraph, intran.SiteID);
					if (invSite.DropShipLocationID == null)
					{
						throw new PXException(SO.Messages.NoDropShipLocation, invSite.SiteCD);
					}

					intran.LocationID = invSite.DropShipLocationID;
				}
				else
				{
					intran.LocationID = poreceiptline.ReceiptLine.LocationID ?? origtran.LocationID;
				}

				intran.LotSerialNbr = poreceiptline.ReceiptLine.LotSerialNbr;
				intran.POReceiptType = poreceiptline.ReceiptLine.ReceiptType;
				intran.POReceiptNbr = poreceiptline.ReceiptLine.ReceiptNbr;
				intran.POReceiptLineNbr = poreceiptline.ReceiptLine.LineNbr;
				intran.POLineType = poreceiptline.ReceiptLine.LineType;

				//tran.Qty = poreceiptline.ReceiptQty;
				intran.TranDesc = landedCostDetail.Descr;
				//tran.UnitCost = PXDBPriceCostAttribute.Round(inGraph.transactions.Cache, (decimal)(poreceiptline.ExtCost / poreceiptline.ReceiptQty));
				intran.TranCost = poreceiptline.AllocatedAmt;
				intran.ReasonCode = reasonCode;

				if (origtran != null && origtran.CostCodeID != null)
				{
					intran.CostCodeID = origtran.CostCodeID;
				}
				if (origtran != null && origtran.DocType == INDocType.Issue)
				{
					intran.ARDocType = origtran.ARDocType;
					intran.ARRefNbr = origtran.ARRefNbr;
					intran.ARLineNbr = origtran.ARLineNbr;
				}
				if (!isDropShip)
				{
					intran.OrigDocType = origtran.DocType;
					intran.OrigTranType = origtran.TranType;
					intran.OrigRefNbr = origtran.RefNbr;
				}

				int? acctID = null;
				int? subID = null;
				intran.AcctID = landedCostDetail.LCAccrualAcct;
				intran.SubID = landedCostDetail.LCAccrualSub;
				GetLCVarianceAccountSub(ref acctID, ref subID, poreceiptline.ReceiptLine);
				intran.COGSAcctID = acctID;
				intran.COGSSubID = subID;

				if (origtran?.IsSpecialOrder == true)
				{
					intran.IsSpecialOrder = true;
					intran.SOOrderType = origtran.SOOrderType;
					intran.SOOrderNbr = origtran.SOOrderNbr;
					intran.SOOrderLineNbr = origtran.SOOrderLineNbr;
				}

				result.Add(intran);
			}

			return result.ToArray();
		}

		protected virtual LandedCostCode GetLandedCostCode(string landedCostCodeID) => LandedCostCode.PK.Find(_pxGraph, landedCostCodeID);

		protected virtual void GetLCVarianceAccountSub(ref int? aAccountID, ref int? aSubID, POReceiptLine receiptLine)
		{
			if (receiptLine.InventoryID.HasValue)
			{
				var invItem = InventoryItem.PK.Find(_pxGraph, receiptLine.InventoryID);
				if (invItem != null)
				{
					INPostClass postClass = INPostClass.PK.Find(_pxGraph, invItem.PostClassID);

					if ((bool)invItem.StkItem)
					{
						if (postClass == null)
							throw new PXException(Messages.PostingClassIsNotDefinedForTheItemInReceiptRow, invItem.InventoryCD, receiptLine.ReceiptNbr, receiptLine.LineNbr);
						INSite invSite = null;
						if (receiptLine.SiteID != null)
							invSite = INSite.PK.Find(_pxGraph, receiptLine.SiteID);
						if (receiptLine.LineType == POLineType.GoodsForDropShip)
						{
							aAccountID = INReleaseProcess.GetAcctID<INPostClass.cOGSAcctID>(_pxGraph, postClass.COGSAcctDefault, invItem, invSite, postClass);
							if (aAccountID == null)
								throw new PXException(Messages.COGSAccountCanNotBeFoundForItemInReceiptRow, invItem.InventoryCD, receiptLine.ReceiptNbr, receiptLine.LineNbr, postClass.PostClassID, invSite != null ? invSite.SiteCD : String.Empty);
							try
							{
								aSubID = INReleaseProcess.GetSubID<INPostClass.cOGSSubID>(_pxGraph, postClass.COGSAcctDefault, postClass.COGSSubMask, invItem, invSite, postClass);
							}
							catch (PXException ex)
							{
								if (postClass.COGSSubID == null
									|| string.IsNullOrEmpty(postClass.COGSSubMask)
										|| invItem.COGSSubID == null || invSite == null || invSite.COGSSubID == null)
									throw new PXException(Messages.COGSSubAccountCanNotBeFoundForItemInReceiptRow, invItem.InventoryCD, receiptLine.ReceiptNbr, receiptLine.LineNbr, postClass.PostClassID, invSite != null ? invSite.SiteCD : String.Empty);
								else
									throw ex;
							}
							if (aSubID == null)
								throw new PXException(Messages.COGSSubAccountCanNotBeFoundForItemInReceiptRow, invItem.InventoryCD, receiptLine.ReceiptNbr, receiptLine.LineNbr, postClass.PostClassID, invSite != null ? invSite.SiteCD : String.Empty);
						}
						else
						{
							aAccountID = INReleaseProcess.GetAcctID<INPostClass.lCVarianceAcctID>(_pxGraph, postClass.LCVarianceAcctDefault, invItem, invSite, postClass);
							if (aAccountID == null)
							{
								throw new PXException(Messages.LCVarianceAccountCanNotBeFoundForItemInReceiptRow, invItem.InventoryCD, receiptLine.ReceiptNbr, receiptLine.LineNbr, postClass.PostClassID, invSite != null ? invSite.SiteCD : String.Empty);
							}
							try
							{
								aSubID = INReleaseProcess.GetSubID<INPostClass.lCVarianceSubID>(_pxGraph, postClass.LCVarianceAcctDefault, postClass.LCVarianceSubMask, invItem, invSite, postClass);
							}
							catch (PXException ex)
							{
								if (postClass.LCVarianceSubID == null
									|| string.IsNullOrEmpty(postClass.LCVarianceSubMask)
										|| invItem.LCVarianceSubID == null || invSite == null || invSite.LCVarianceSubID == null)
								{
									throw new PXException(Messages.LCVarianceSubAccountCanNotBeFoundForItemInReceiptRow, invItem.InventoryCD, receiptLine.ReceiptNbr, receiptLine.LineNbr, postClass.PostClassID, invSite != null ? invSite.SiteCD : String.Empty);
								}
								else
								{
									throw ex;
								}
							}
							if (aSubID == null)
								throw new PXException(Messages.LCVarianceSubAccountCanNotBeFoundForItemInReceiptRow, invItem.InventoryCD, receiptLine.ReceiptNbr, receiptLine.LineNbr, postClass.PostClassID, invSite != null ? invSite.SiteCD : String.Empty);
						}

					}
					else
					{
						aAccountID = receiptLine.ExpenseAcctID;
						aSubID = receiptLine.ExpenseSubID;
					}
				}
				else
				{
					throw
						new PXException(Messages.LCInventoryItemInReceiptRowIsNotFound, receiptLine.InventoryID, receiptLine.ReceiptNbr, receiptLine.LineNbr);
				}
			}
			else
			{
				aAccountID = receiptLine.ExpenseAcctID;
				aSubID = receiptLine.ExpenseSubID;
			}
		}

	}
}
