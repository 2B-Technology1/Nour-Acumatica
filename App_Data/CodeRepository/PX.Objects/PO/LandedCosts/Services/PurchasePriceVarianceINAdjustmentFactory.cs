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
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.SO;

namespace PX.Objects.PO.LandedCosts
{
	public class PurchasePriceVarianceINAdjustmentFactory
	{
		private readonly INAdjustmentEntry _inGraph;

		public PurchasePriceVarianceINAdjustmentFactory(INAdjustmentEntry inGraph)
		{
			_inGraph = inGraph;
		}

		public virtual void CreateAdjustmentTran(List<AllocationServiceBase.POReceiptLineAdjustment> pOLinesToProcess, string ReasonCode)
		{
			foreach (AllocationServiceBase.POReceiptLineAdjustment poreceiptline in pOLinesToProcess)
			{
				INTran intran = new INTran();

				INTran origtran = PurchasePriceVarianceAllocationService.Instance.GetOriginalInTran(_inGraph, poreceiptline.ReceiptLine.ReceiptType, poreceiptline.ReceiptLine.ReceiptNbr, poreceiptline.ReceiptLine.LineNbr);
				bool isDropShip = (poreceiptline.ReceiptLine.LineType == POLineType.GoodsForDropShip || poreceiptline.ReceiptLine.LineType == POLineType.NonStockForDropShip);

				if (!isDropShip && origtran == null) throw new PXException(AP.Messages.CannotFindINReceipt, poreceiptline.ReceiptLine.ReceiptNbr);

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
				intran.IsStockItem = poreceiptline.ReceiptLine.IsStockItem;
				intran.InventoryID = poreceiptline.ReceiptLine.InventoryID;
				intran.SubItemID = poreceiptline.ReceiptLine.SubItemID;
				intran.SiteID = poreceiptline.ReceiptLine.SiteID;
				intran.BAccountID = poreceiptline.ReceiptLine.VendorID;
				intran.BranchID = poreceiptline.BranchID;


				if (isDropShip && intran.SiteID != null)
				{
					INSite invSite = INSite.PK.Find(_inGraph, intran.SiteID);
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
				intran.ProjectID = poreceiptline.ReceiptLine.ProjectID;
				intran.TaskID = poreceiptline.ReceiptLine.TaskID;
				intran.CostCodeID = poreceiptline.ReceiptLine.CostCodeID;

				//tran.Qty = poreceiptline.ReceiptQty;
				intran.TranDesc = poreceiptline.ReceiptLine.TranDesc;
				//tran.UnitCost = PXDBPriceCostAttribute.Round(inGraph.transactions.Cache, (decimal)(poreceiptline.ExtCost / poreceiptline.ReceiptQty));
				intran.TranCost = poreceiptline.AllocatedAmt;
				intran.ReasonCode = ReasonCode;
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

				//Set AcctID and SubID = POAccrual Acct/Sub from orig. INTran
				if (origtran != null)
				{
					intran.AcctID = origtran.AcctID;
					intran.SubID = origtran.SubID;
				}
				else if (isDropShip)
				{
					intran.AcctID = poreceiptline.ReceiptLine.POAccrualAcctID;
					intran.SubID = poreceiptline.ReceiptLine.POAccrualSubID;
				}
				ReasonCode reasonCode = CS.ReasonCode.PK.Find(_inGraph, ReasonCode);
				if (reasonCode == null)
					throw new PXException(AP.Messages.ReasonCodeCannotNotFound, ReasonCode);
				AP.APReleaseProcess.GetPPVAccountSub(ref acctID, ref subID, _inGraph, poreceiptline.ReceiptLine, reasonCode);

				intran.COGSAcctID = acctID;
				intran.COGSSubID = subID;

				if (origtran?.IsSpecialOrder == true)
				{ 
					intran.IsSpecialOrder = true;
					intran.SOOrderType = origtran.SOOrderType;
					intran.SOOrderNbr = origtran.SOOrderNbr;
					intran.SOOrderLineNbr = origtran.SOOrderLineNbr;
				}
				_inGraph.CostCenterDispatcherExt?.SetCostLayerType(intran);

				intran = _inGraph.transactions.Insert(intran);
			}
		}
	}
}
