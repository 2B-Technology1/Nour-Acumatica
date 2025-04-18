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
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CM.Extensions;
using PX.Objects.Common.Discount.Attributes;
using PX.Objects.CS;
using PX.Objects.DR;
using PX.Objects.Extensions.MultiCurrency;
using PX.Objects.GL;
using PX.Objects.TX;
using PX.Objects.IN;

namespace PX.Objects.PO.GraphExtensions.APInvoiceEntryExt
{
	public class Prepayments : PXGraphExtension<APInvoiceEntry.MultiCurrency, APInvoiceEntry>
	{
		public PXSelect<POLine> POLines;
		public PXSelect<POOrder,
			Where<POOrder.orderType, Equal<Optional<POOrderPrepayment.orderType>>, And<POOrder.orderNbr, Equal<Optional<POOrderPrepayment.orderNbr>>>>>
			POOrders;

		public PXSelect<POOrderPrepayment,
			Where<POOrderPrepayment.aPDocType, Equal<Current<APInvoice.docType>>,
				And<POOrderPrepayment.aPRefNbr, Equal<Current<APInvoice.refNbr>>,
				And<Current<APInvoice.docType>, Equal<APDocType.prepayment>>>>>
			PrepaidOrders;

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXParent(typeof(Select<POOrderPrepayment,
			Where<POOrderPrepayment.orderType, Equal<Current<APTran.pOOrderType>>, And<POOrderPrepayment.orderNbr, Equal<Current<APTran.pONbr>>,
				And<POOrderPrepayment.aPDocType, Equal<Current<APTran.tranType>>, And<POOrderPrepayment.aPRefNbr, Equal<Current<APTran.refNbr>>,
				And<Current<APTran.tranType>, Equal<APDocType.prepayment>>>>>>>), ParentCreate = true)]
		protected virtual void _(Events.CacheAttached<APTran.pONbr> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(ManualDiscountMode))]
		[PrepaymentDiscount(typeof(APTran.curyDiscAmt), typeof(APTran.curyTranAmt),
			typeof(APTran.discPct), typeof(APTran.freezeManualDisc), DiscountFeatureType.VendorDiscount)]
		protected virtual void _(Events.CacheAttached<APTran.manualDisc> e)
		{
		}

		#region Multicurrency

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXParent(typeof(POOrderPrepayment.FK.Order))]
		protected virtual void _(Events.CacheAttached<POOrderPrepayment.orderNbr> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[CurrencyInfo(typeof(POOrder.curyInfoID))]
		protected virtual void _(Events.CacheAttached<POOrderPrepayment.curyInfoID> e)
		{
		}

		/// <summary>
		/// Overrides <see cref="APInvoiceEntry.MultiCurrency.GetChildren"/>.
		/// </summary>
		[PXOverride]
		public virtual PXSelectBase[] GetChildren(Func<PXSelectBase[]> baseMethod)
		{
			return baseMethod().Union(new PXSelectBase[] { PrepaidOrders }).ToArray();
		}

		/// <summary>
		/// Overrides <see cref="MultiCurrencyGraph{TGraph, TPrimary}.GetTrackedExceptChildren"/>.
		/// </summary>
		[PXOverride]
		public virtual PXSelectBase[] GetTrackedExceptChildren(Func<PXSelectBase[]> baseMethod)
		{
			return baseMethod().Union(new PXSelectBase[] { POLines }).ToArray();
		}

		#endregion

		protected virtual void _(Events.RowInserted<POOrderPrepayment> e)
		{
			if (Base.Document.Current?.OrigModule == BatchModule.AP)
			{
				Base.Document.Current.OrigModule = BatchModule.PO;
				Base.Document.Cache.MarkUpdated(Base.Document.Current, assertError: true);
			}
		}

		protected virtual void _(Events.RowDeleted<POOrderPrepayment> e)
		{
			if (Base.Document.Current?.OrigModule == BatchModule.PO
				&& !PrepaidOrders.Select().AsEnumerable().Any())
			{
				Base.Document.Current.OrigModule = BatchModule.AP;
				Base.Document.Cache.MarkUpdated(Base.Document.Current, assertError: true);
			}
		}

		protected virtual void _(Events.FieldDefaulting<APTran, APTran.prepaymentPct> e)
		{
			if (e.Row.TranType != APDocType.Prepayment)
			{
				e.NewValue = 0m;
				e.Cancel = true;
				return;
			}

			if (!string.IsNullOrEmpty(e.Row.POOrderType) && !string.IsNullOrEmpty(e.Row.PONbr))
			{
				POOrder order = PXSelectReadonly<POOrder,
					Where<POOrder.orderType, Equal<Required<POOrder.orderType>>,
						And<POOrder.orderNbr, Equal<Required<POOrder.orderNbr>>>>>
					.Select(Base, e.Row.POOrderType, e.Row.PONbr);
				if (order?.PrepaymentPct != null)
				{
					e.NewValue = order.PrepaymentPct;
					e.Cancel = true;
					return;
				}
			}

			if (e.Row.InventoryID != null)
			{
				POVendorInventory vendorInventory = PXSelectReadonly<POVendorInventory,
					Where<POVendorInventory.inventoryID, Equal<Required<POVendorInventory.inventoryID>>,
						And<POVendorInventory.vendorID, Equal<Current<APInvoice.vendorID>>,
						And<POVendorInventory.vendorLocationID, Equal<Current<APInvoice.vendorLocationID>>>>>>
					.Select(Base, e.Row.InventoryID);
				if (vendorInventory?.PrepaymentPct != null)
				{
					e.NewValue = vendorInventory.PrepaymentPct;
					e.Cancel = true;
					return;
				}
			}

			e.NewValue = Base.location.Current?.VPrepaymentPct ?? 0m;
		}

		protected virtual void _(Events.FieldUpdated<APTran, APTran.inventoryID> e)
		{
			if (e.Row.TranType == APDocType.Prepayment)
			{
				e.Cache.SetDefaultExt<APTran.prepaymentPct>(e.Row);
			}
		}

		protected virtual void _(Events.FieldUpdated<APTran, APTran.pONbr> e)
		{
			if (e.Row.TranType == APDocType.Prepayment)
			{
				e.Cache.SetDefaultExt<APTran.prepaymentPct>(e.Row);
			}
		}

		protected virtual void _(Events.RowDeleted<APTran> e)
		{
			if (e.Row.TranType != APDocType.Prepayment || string.IsNullOrEmpty(e.Row.POOrderType) || string.IsNullOrEmpty(e.Row.PONbr))
				return;
			var prepaidOrder = PXParentAttribute.SelectParent<POOrderPrepayment>(Base.Transactions.Cache, e.Row);
			if (prepaidOrder != null
				&& !PXParentAttribute.SelectChildren(Base.Transactions.Cache, prepaidOrder, typeof(POOrderPrepayment)).Any())
			{
				PrepaidOrders.Delete(prepaidOrder);
			}
		}

		public virtual void AddPOOrderProc(POOrder order, bool createNew)
		{
			APInvoice prepayment;
			if (createNew)
			{
				prepayment = Base.Document.Insert(new APInvoice { DocType = APDocType.Prepayment });
				prepayment.BranchID = order.BranchID;
				prepayment.DocDesc = order.OrderDesc;
				if (PXAccess.FeatureInstalled<FeaturesSet.vendorRelations>())
				{
					prepayment.VendorID = order.PayToVendorID;
					prepayment.VendorLocationID = (order.VendorID == order.PayToVendorID) ? order.VendorLocationID : null;
					prepayment.SuppliedByVendorID = order.VendorID;
					prepayment.SuppliedByVendorLocationID = order.VendorLocationID;
				}
				else
				{
					prepayment.VendorID =
					prepayment.SuppliedByVendorID = order.VendorID;
					prepayment.VendorLocationID =
					prepayment.SuppliedByVendorLocationID = order.VendorLocationID;
				}
				prepayment.CuryID = order.CuryID;
				Base.Document.Update(prepayment);
				prepayment.TaxCalcMode = order.TaxCalcMode;
				prepayment.InvoiceNbr = order.OrderNbr;
				prepayment.DueDate = order.OrderDate;
				prepayment.TaxZoneID = order.TaxZoneID;
				Base.Document.Update(prepayment);
			}
			else
			{
				prepayment = Base.Document.Current;
			}

			TaxBaseAttribute.SetTaxCalc<APTran.taxCategoryID, TaxAttribute>(Base.Transactions.Cache, null, TaxCalc.ManualCalc);

			var orderLines = PXSelectReadonly<POLineRS,
				Where<POLineRS.orderType, Equal<Required<POOrder.orderType>>,
					And<POLineRS.orderNbr, Equal<Required<POOrder.orderNbr>>>>,
				OrderBy<Asc<POLineRS.sortOrder, Asc<POLineRS.lineNbr>>>>
				.Select(Base, order.OrderType, order.OrderNbr)
				.RowCast<POLineRS>()
				.ToList();

			bool hasAdded = AddPOOrderLines(orderLines);
			if (!hasAdded)
			{
				throw new PXException(Messages.APInvoicePOOrderCreation_NoApplicableLinesFound);
			}

			Base.AddOrderTaxes(order);

			TaxBaseAttribute.SetTaxCalc<APTran.taxCategoryID, TaxAttribute>(Base.Transactions.Cache, null, TaxCalc.ManualLineCalc);
		}

		public virtual bool AddPOOrderLines(IEnumerable<POLineRS> lines)
		{
			bool hasAdded = false;
			foreach (POLineRS line in lines.Where(l =>
				(l.CuryExtCost + l.CuryRetainageAmt > l.CuryReqPrepaidAmt)
				&& l.Cancelled == false && l.Closed == false
				&& (l.Billed == false || l.LineType == POLineType.Service)))
			{
				var tran = new APTran
				{
					IsStockItem = line.IsStockItem,
					InventoryID = line.InventoryID,
					ProjectID = line.ProjectID,
					TaskID = line.TaskID,
					CostCodeID = line.CostCodeID,
					TaxID = line.TaxID,
					TaxCategoryID = line.TaxCategoryID,
					TranDesc = line.TranDesc,
					UOM = line.UOM,
					CuryUnitCost = line.CuryUnitCost,
					DiscPct = line.DiscPct,
					ManualPrice = true,
					ManualDisc = true,
					FreezeManualDisc = true,
					DiscountID = line.DiscountID,
					DiscountSequenceID = line.DiscountSequenceID,
					POOrderType = line.OrderType,
					PONbr = line.OrderNbr,
					POLineNbr = line.LineNbr,

					// The Retainage Percent and Retainage Amount = 0. Values must not be copied from subcontract. (AC-214055)
					RetainagePct = null,
					CuryRetainageAmt = null
				};

				decimal? billedAndPrepaidQty = line.ReqPrepaidQty + line.OrderBilledQty;
				tran.Qty = (line.OrderQty <= billedAndPrepaidQty) ? line.OrderQty : line.OrderQty - billedAndPrepaidQty;

				decimal? billedAndPrepaidAmt = line.CuryReqPrepaidAmt + line.CuryOrderBilledAmt;
				if (billedAndPrepaidAmt == 0m)
				{
					tran.CuryLineAmt = line.CuryLineAmt;
					tran.CuryDiscAmt = line.CuryDiscAmt;
				}
				else if (line.CuryExtCost + line.CuryRetainageAmt <= billedAndPrepaidAmt)
				{
					tran.CuryLineAmt = 0m;
					tran.CuryDiscAmt = 0m;
					tran.CuryRetainageAmt = 0m;
					tran.CuryTranAmt = 0m;
				}
				else
				{
					CurrencyInfo currencyInfo = Base.FindImplementation<IPXCurrencyHelper>().GetDefaultCurrencyInfo();

					decimal? prepaymentRatio = (line.CuryExtCost + line.CuryRetainageAmt - billedAndPrepaidAmt) / (line.CuryExtCost + line.CuryRetainageAmt);
					tran.CuryLineAmt = currencyInfo.RoundCury((prepaymentRatio * line.CuryLineAmt) ?? 0m);
					tran.CuryDiscAmt = currencyInfo.RoundCury((prepaymentRatio * line.CuryDiscAmt) ?? 0m);
				}

				if (tran.InventoryID.HasValue)
				{
					InventoryItem inventoryItem = InventoryItem.PK.Find(Base, tran.InventoryID);

					if (inventoryItem != null)
					{
						DRDeferredCode deferralCode = DRDeferredCode.PK.Find(Base, inventoryItem.DeferredCode);

						if (deferralCode != null)
						{
							tran.RequiresTerms = Base.DoesRequireTerms(deferralCode);
						}
					}
				}

				tran = Base.Transactions.Insert(tran);

				if (PXParentAttribute.SelectParent<POOrderPrepayment>(Base.Transactions.Cache, tran) == null)
				{
					PXParentAttribute.CreateParent(Base.Transactions.Cache, tran, typeof(POOrderPrepayment));
				}

				hasAdded = true;
			}
			if (!hasAdded && lines.All(l => l.RcptQtyThreshold == 0))
			{
				throw new PXException(Messages.PrepaymentPOCreation_AllLinesCompleteOnZero);
			}
			Base.AutoRecalculateDiscounts();
			return hasAdded;
		}

		[PXOverride]
		public virtual void VoidPrepayment(APRegister doc, Action<APRegister> baseMethod)
		{
			foreach (PXResult<APTran, POLine> poLineRes in Base.TransactionsPOLine.Select())
			{
				POLine line = poLineRes;
				if (line?.OrderNbr == null) continue;

				POLine updLine = PXCache<POLine>.CreateCopy(line);
				APTran tran = poLineRes;
				updLine.ReqPrepaidQty -= tran.Qty;
				updLine.CuryReqPrepaidAmt -= tran.CuryTranAmt + tran.CuryRetainageAmt;
				updLine = this.POLines.Update(updLine);
			}

			POOrderPrepayment prepay = PrepaidOrders.Select();
			if (prepay != null)
			{
				POOrderPrepayment updPrepay = PXCache<POOrderPrepayment>.CreateCopy(prepay);
				updPrepay.CuryAppliedAmt -= doc.CuryOrigDocAmt;
				updPrepay = this.PrepaidOrders.Update(updPrepay);

				POOrder order = POOrders.Select(prepay.OrderType, prepay.OrderNbr);
				POOrder updOrder = PXCache<POOrder>.CreateCopy(order);
				updOrder.CuryPrepaidTotal -= doc.CuryDocBal;
				updOrder = this.POOrders.Update(updOrder);
			}

			baseMethod(doc);
		}

		protected virtual void _(Events.RowPersisting<APTran> e)
		{
			APTran tran = e.Row;
			if (tran == null || e.Operation.Command().IsNotIn(PXDBOperation.Insert, PXDBOperation.Update))
				return;

			if (tran.Released != true && tran.TranType == APDocType.Prepayment && !string.IsNullOrEmpty(tran.PONbr) && tran.POLineNbr != null)
			{
				POLine poLine = PXSelectReadonly<POLine,
					Where<POLine.orderType, Equal<Required<POLine.orderType>>, And<POLine.orderNbr, Equal<Required<POLine.orderNbr>>,
						And<POLine.lineNbr, Equal<Required<POLine.lineNbr>>>>>>
					.SelectWindowed(Base, 0, 1, tran.POOrderType, tran.PONbr, tran.POLineNbr);
				if (tran.Qty > poLine.OrderQty)
				{
					e.Cache.RaiseExceptionHandling<APTran.qty>(tran, tran.Qty, new PXSetPropertyException(Messages.PrepaidQtyCantExceedPOLine));
				}
				if ((poLine.CuryReqPrepaidAmt > poLine.CuryBilledAmt ? poLine.CuryReqPrepaidAmt : poLine.CuryBilledAmt)
					+ tran.CuryTranAmt + tran.CuryRetainageAmt
					> poLine.CuryExtCost + poLine.CuryRetainageAmt)
				{
					e.Cache.RaiseExceptionHandling<APTran.curyTranAmt>(tran, tran.CuryTranAmt,
						new PXSetPropertyException(Messages.PrepaidAmtCantExceedPOLine));
				}
				else if (poLine.CuryReqPrepaidAmt + poLine.CuryBilledAmt
					+ tran.CuryTranAmt + tran.CuryRetainageAmt
					> poLine.CuryExtCost + poLine.CuryRetainageAmt)
				{
					e.Cache.RaiseExceptionHandling<APTran.curyTranAmt>(tran, tran.CuryTranAmt,
						new PXSetPropertyException(Messages.PrepaidAmtMayExceedPOLine, PXErrorLevel.Warning, poLine.OrderNbr));
				}
			}
		}
	}
}
