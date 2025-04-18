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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using PX.Api;
using PX.Common;
using PX.Common.Collection;
using PX.Concurrency;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.WorkflowAPI;

using PX.Objects.AR;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.AR.MigrationMode;
using PX.Objects.CA;
using PX.Objects.CM.Extensions;
using PX.Objects.Common;
using PX.Objects.Common.Bql;
using PX.Objects.Common.Discount;
using PX.Objects.Common.Extensions;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.SO.GraphExtensions.SOInvoiceEntryExt;
using PX.Objects.TX;

using static PX.Objects.AR.ARSalesPriceMaint;

using CRLocation = PX.Objects.CR.Standalone.Location;
using POLineType = PX.Objects.PO.POLineType;
using POReceipt = PX.Objects.PO.POReceipt;
using POReceiptLine = PX.Objects.PO.POReceiptLine;
using POReceiptType = PX.Objects.PO.POReceiptType;

namespace PX.Objects.SO
{
	//Invert sign of BaseQty for BilledQty and UnbilledQty calculation in case AR Invoice type (credit/debit) and SOLine operation (issue/receipt) have opposite signs
	public class BaseBilledQtyFormula :
		IIf<Where<ARTran.sOOrderLineOperation, Equal<SOOperation.receipt>, And<ARTran.drCr, Equal<DrCr.credit>,
			Or<ARTran.sOOrderLineOperation, Equal<SOOperation.issue>, And<ARTran.drCr, Equal<DrCr.debit>>>>>,
		Data.Minus<ARTran.baseQty.Multiply<ARTran.sOOrderLineSign>>,
		ARTran.baseQty.Multiply<ARTran.sOOrderLineSign>>
	{
	}
	public class CuryBilledAmtFormula :
		IIf<Where<ARTran.sOOrderLineOperation, Equal<SOOperation.receipt>, And<ARTran.drCr, Equal<DrCr.credit>,
			Or<ARTran.sOOrderLineOperation, Equal<SOOperation.issue>, And<ARTran.drCr, Equal<DrCr.debit>>>>>,
		Data.Minus<ARTran.curyTranAmt.Multiply<ARTran.sOOrderLineSign>>,
		ARTran.curyTranAmt.Multiply<ARTran.sOOrderLineSign>>
	{
	}

	public partial class SOInvoiceEntry : ARInvoiceEntry
	{
		public PXAction<ARInvoice> selectShipment;
		[PXUIField(DisplayName = "Add Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable SelectShipment(PXAdapter adapter)
		{
			if (Transactions.AllowInsert)
				shipmentlist.AskExt();
			return adapter.Get();
		}

		public PXAction<ARInvoice> addShipment;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable AddShipment(PXAdapter adapter)
		{
			var orders = shipmentlist
				.Cache.Updated.Cast<SOOrderShipment>()
				.Where(sho => sho.Selected == true)
				.SelectMany(sho =>
						PXSelectJoin<SOOrderShipment,
						InnerJoin<SOOrder,
							On<SOOrderShipment.FK.Order>,
						InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<SOOrder.curyInfoID>>,
						InnerJoin<SOAddress, On<SOAddress.addressID, Equal<SOOrder.billAddressID>>,
						InnerJoin<SOContact, On<SOContact.contactID, Equal<SOOrder.billContactID>>>>>>,
					Where<SOOrderShipment.shipmentNbr, Equal<Current<SOOrderShipment.shipmentNbr>>,
						And<SOOrderShipment.shipmentType, Equal<Current<SOOrderShipment.shipmentType>>,
						And<SOOrderShipment.orderType, Equal<Current<SOOrderShipment.orderType>>,
						And<SOOrderShipment.orderNbr, Equal<Current<SOOrderShipment.orderNbr>>>>>>>
					.SelectMultiBound(this, new object[] { sho }).AsEnumerable()
					.Cast<PXResult<SOOrderShipment, SOOrder, CurrencyInfo, SOAddress, SOContact>>()
					.Select(row => new { Shipment = sho, Row = row }))
				.ToArray();

			var linkedOrdersKeys =
				PXSelect<SOOrderShipment,
				Where<SOOrderShipment.invoiceType, Equal<Current<ARInvoice.docType>>,
					And<SOOrderShipment.invoiceNbr, Equal<Current<ARInvoice.refNbr>>>>>
				.Select(this).AsEnumerable()
				.RowCast<SOOrderShipment>()
				.Select(r => new { Type = r.OrderType, Nbr = r.OrderNbr })
				.ToHashSet();
			var linkedOrders = linkedOrdersKeys.Any() // will fall if linked orders count is more than 1000
				? PXSelectReadonly<SOOrder, Where<SOOrder.orderType, In<Required<SOOrder.orderType>>, And<SOOrder.orderNbr, In<Required<SOOrder.orderNbr>>>>>
					.Select(this, linkedOrdersKeys.Select(k => k.Type).ToArray(), linkedOrdersKeys.Select(k => k.Nbr).ToArray()).AsEnumerable()
					.RowCast<SOOrder>()
					.Where(so => linkedOrdersKeys.Contains(new { Type = so.OrderType, Nbr = so.OrderNbr }))
					.ToArray()
				: Enumerable.Empty<SOOrder>();

			var ordersByTaxZone = orders.Select(r => r.Row.GetItem<SOOrder>()).Concat(linkedOrders).ToLookup(s => s.TaxZoneID);
			string theOnlyTaxZone = ordersByTaxZone.Any()
				? Document.Current?.TaxZoneID ?? linkedOrders.FirstOrDefault()?.TaxZoneID ?? ordersByTaxZone.First().Key
				: null;

			var ordersByTaxCalcMode = orders.Select(r => r.Row.GetItem<SOOrder>()).Concat(linkedOrders).ToLookup(s => s.TaxCalcMode);
			string theOnlyTaxCalcMode = ordersByTaxCalcMode.Any()
				? Document.Current?.TaxCalcMode ?? linkedOrders.FirstOrDefault()?.TaxCalcMode ?? ordersByTaxCalcMode.First().Key
				: null;

			var ordersByAutomaticTaxCalculation = orders.Select(r => r.Row.GetItem<SOOrder>()).Concat(linkedOrders).ToLookup(s => s.DisableAutomaticTaxCalculation);
			bool? theOnlyAutomaticTaxCalculation = ordersByAutomaticTaxCalculation.Any()
				? Document.Current?.DisableAutomaticTaxCalculation ?? linkedOrders.FirstOrDefault()?.DisableAutomaticTaxCalculation ?? ordersByAutomaticTaxCalculation.First().Key
				: null;

			bool requireControlTotal = ARSetup.Current.RequireControlTotal == true;
			var excludedOrders = new List<SOOrder>();
			var excludedCalcModes = new List<SOOrder>();
			var excludedAutomaticTaxCalculation = new List<SOOrder>();
			foreach (var order in orders)
			{
				if (order.Row.GetItem<SOOrder>().TaxZoneID == theOnlyTaxZone &&
					order.Row.GetItem<SOOrder>().TaxCalcMode == theOnlyTaxCalcMode &&
					((order.Row.GetItem<SOOrder>().DisableAutomaticTaxCalculation == theOnlyAutomaticTaxCalculation && Document.Current?.DisableAutomaticTaxCalculation != null) ||
					(Document.Current?.DisableAutomaticTaxCalculation == null && !linkedOrders.Any())))
				{
					SOOrderShipment orderShipment = order.Row.GetItem<SOOrderShipment>();
					var details = new PXResultset<SOShipLine, SOLine>();
					details.AddRange(
						PXSelectJoin<POReceiptLine,
						InnerJoin<SOLineSplit, On<SOLineSplit.pOType, Equal<POReceiptLine.pOType>,
							And<SOLineSplit.pONbr, Equal<POReceiptLine.pONbr>,
							And<SOLineSplit.pOLineNbr, Equal<POReceiptLine.pOLineNbr>>>>,
						InnerJoin<SOLine, On<SOLine.orderType, Equal<SOLineSplit.orderType>,
							And<SOLine.orderNbr, Equal<SOLineSplit.orderNbr>,
							And<SOLine.lineNbr, Equal<SOLineSplit.lineNbr>>>>>>,
						Where<POReceiptLine.lineType, In3<POLineType.goodsForDropShip, POLineType.nonStockForDropShip>,
							And<SOShipmentType.dropShip, Equal<Current<SOOrderShipment.shipmentType>>,
							And<POReceiptLine.receiptType, Equal<Required<POReceiptLine.receiptType>>,
							And<POReceiptLine.receiptNbr, Equal<Required<POReceiptLine.receiptNbr>>,
							And<SOLine.orderType, Equal<Current<SOOrderShipment.orderType>>,
							And<SOLine.orderNbr, Equal<Current<SOOrderShipment.orderNbr>>>>>>>>>
						.SelectMultiBound(this,
							new object[] { orderShipment },
							orderShipment.Operation == SOOperation.Receipt ? POReceiptType.POReturn : POReceiptType.POReceipt,
							orderShipment.ShipmentNbr)
						.AsEnumerable()
						.Cast<PXResult<POReceiptLine, SOLineSplit, SOLine>>()
						.Select(line => new PXResult<SOShipLine, SOLine>(SOShipLine.FromDropShip(line, line), line)));

					ARSetup.Current.RequireControlTotal = false;
					try
					{
						using (new SOInvoiceAddOrderPaymentsScope())
						{
							InvoiceOrder(new InvoiceOrderArgs(order.Row)
							{
								InvoiceDate = Accessinfo.BusinessDate.Value,
								Details = details
							});
						}
					}
					catch (Exception)
					{
						orderShipment = shipmentlist.Locate(order.Shipment);
						if (orderShipment?.InvoiceType == Document.Current.DocType && orderShipment.InvoiceNbr == Document.Current.RefNbr)
						{
							orderShipment.HasUnhandledErrors = true;
							shipmentlist.Update(orderShipment);
						}
						throw;
					}
					finally
					{
						ARSetup.Current.RequireControlTotal = requireControlTotal;
					}
					order.Shipment.HasDetailDeleted = false;
					order.Shipment.IsPartialInvoiceConstraintViolated = false;
					shipmentlist.Update(order.Shipment);
				}
				else if (order.Row.GetItem<SOOrder>().TaxZoneID != theOnlyTaxZone)
				{
					excludedOrders.Add(order.Row);
				}
				else if (order.Row.GetItem<SOOrder>().DisableAutomaticTaxCalculation != theOnlyAutomaticTaxCalculation)
				{
					excludedAutomaticTaxCalculation.Add(order.Row);
				}
				else
				{
					excludedCalcModes.Add(order.Row);
				}
			}

			shipmentlist.View.Clear();

			if (excludedOrders.Any())
				throw new PXInvalidOperationException(
					Messages.CannotAddOrderToInvoiceDueToTaxZoneConflict,
					theOnlyTaxZone,
					string.Join(",", excludedOrders.Select(s => s.OrderNbr)));

			if (excludedCalcModes.Any())
				throw new PXInvalidOperationException(
					Messages.CannotAddOrderToInvoiceDueToTaxCalcModeConflict,
					PXStringListAttribute.GetLocalizedLabel<SOOrder.taxCalcMode>(Document.Cache, null, theOnlyTaxCalcMode),
					string.Join(",", excludedCalcModes.Select(s => s.OrderNbr)));

			if (excludedAutomaticTaxCalculation.Any())
				throw new PXInvalidOperationException(
					Messages.CannotAddOrderToInvoiceDueToAutomaticTaxCalculationConflict,
					string.Join(",", excludedAutomaticTaxCalculation.Select(s => s.OrderNbr)));

			return adapter.Get();
		}


		public PXAction<ARInvoice> addShipmentCancel;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable AddShipmentCancel(PXAdapter adapter)
		{
			foreach (SOOrderShipment shipment in shipmentlist.Cache.Updated)
			{
				if (shipment.InvoiceNbr == null)
				{
					shipment.Selected = false;
				}
			}

			shipmentlist.View.Clear();
			//shipmentlist.Cache.Clear();
			return adapter.Get();
		}

		public bool cancelUnitPriceCalculation
		{
			get;
			set;
		}

		private bool forceDiscountCalculation = false;
		public PXSelect<ARTran, Where<ARTran.tranType, Equal<Current<ARInvoice.docType>>, And<ARTran.refNbr, Equal<Current<ARInvoice.refNbr>>, And<ARTran.lineType, Equal<SOLineType.freight>>>>, OrderBy<Asc<ARTran.tranType, Asc<ARTran.refNbr, Asc<ARTran.lineNbr>>>>> Freight;
		public PXSelect<ARTran, Where<ARTran.tranType, Equal<Current<ARInvoice.docType>>, And<ARTran.refNbr, Equal<Current<ARInvoice.refNbr>>, And<ARTran.lineType, Equal<SOLineType.discount>>>>, OrderBy<Asc<ARTran.tranType, Asc<ARTran.refNbr, Asc<ARTran.lineNbr>>>>> Discount;
		public PXSelect<ARSalesPerTran, Where<ARSalesPerTran.docType, Equal<Current<ARInvoice.docType>>, And<ARSalesPerTran.refNbr, Equal<Current<ARInvoice.refNbr>>>>> commisionlist;
		public PXSelect<SOInvoice, Where<SOInvoice.docType, Equal<Optional<ARInvoice.docType>>, And<SOInvoice.refNbr, Equal<Optional<ARInvoice.refNbr>>>>> SODocument;
		[PXCopyPasteHiddenView]
		public PXSelectOrderBy<SOOrderShipment, OrderBy<Asc<SOOrderShipment.orderType, Asc<SOOrderShipment.orderNbr, Asc<SOOrderShipment.shipmentNbr, Asc<SOOrderShipment.shipmentType>>>>>> shipmentlist;
		public PXSelect<SOShipment> shipments;
		[PXCopyPasteHiddenView]
		public PXSelect<SOFreightDetail, Where<SOFreightDetail.docType, Equal<Optional2<ARInvoice.docType>>, And<SOFreightDetail.refNbr, Equal<Optional2<ARInvoice.refNbr>>>>> FreightDetails;

		public PXSelect<SOAdjust> soadjustments;
		public PXSelect<INTran> inTran;
		public PXSetup<SOOrderType, Where<SOOrderType.orderType, Equal<Optional<SOOrder.orderType>>>> soordertype;

		public PXSelect<ARInvoice> invoiceview;

		#region Cache Attached
		#region ARTran
		[PXDBString(2, IsFixed = true)]
		[SOLineType.List()]
		[PXUIField(DisplayName = "Line Type", Visible = false, Enabled = false)]
		[PXDefault]
		[PXFormula(typeof(Switch<
			Case<Where<ARTran.inventoryID, IsNull>, SOLineType.nonInventory,
			Case<Where<ARTran.sOShipmentNbr, IsNull>,
				Selector<ARTran.inventoryID, Switch<
					Case<Where<InventoryItem.stkItem, Equal<True>>, SOLineType.inventory,
					Case<Where<InventoryItem.nonStockShip, Equal<True>>, SOLineType.nonInventory>>,
				SOLineType.miscCharge>>>>,
			ARTran.lineType>))]
		protected virtual void ARTran_LineType_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(TaxCategory.taxCategoryID.Length, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Category")]
		[SOInvoiceTax(Inventory = typeof(ARTran.inventoryID), UOM = typeof(ARTran.uOM), LineQty = typeof(ARTran.qty))] //Per Unit Tax settings
		[PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
        [PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
        [PXDefault(typeof(Selector<ARTran.inventoryID, InventoryItem.taxCategoryID>),
			PersistingCheck = PXPersistingCheck.Nothing, SearchOnDefault = false)]
		protected override void ARTran_TaxCategoryID_CacheAttached(PXCache sender)
		{
		}

		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Manual Price", Visible = true)]
		protected virtual void ARTran_ManualPrice_CacheAttached(PXCache sender)
		{
		}

		[PopupMessage]
		[PXRemoveBaseAttribute(typeof(ARTranInventoryItemAttribute))]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[NonStockNonKitCrossItem(INPrimaryAlternateType.CPN, Messages.CannotAddNonStockKitDirectly, typeof(ARTran.sOOrderNbr),
			typeof(FeaturesSet.advancedSOInvoices), Filterable = true)]
		protected override void ARTran_InventoryID_CacheAttached(PXCache sender) { }

		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[IN.SiteAvail(typeof(ARTran.inventoryID), typeof(ARTran.subItemID), typeof(CostCenter.freeStock), DocumentBranchType = typeof(ARInvoice.branchID))]
		[InterBranchRestrictor(typeof(Where<SameOrganizationBranch<INSite.branchID, Current<ARInvoice.branchID>>>))]
		protected override void ARTran_SiteID_CacheAttached(PXCache sender) { }

		//Returning original attributes from ARTran
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected override void ARTran_LocationID_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDBQuantity(typeof(ARTran.uOM), typeof(ARTran.baseQty), InventoryUnitType.BaseUnit | InventoryUnitType.SalesUnit, HandleEmptyKey = true)]
		protected virtual void ARTran_Qty_CacheAttached(PXCache sender) { }

		[PXRemoveBaseAttribute(typeof(PXUIFieldAttribute))]
		[PXUIField(DisplayName = AP.APTran.sortOrder.DispalyName, Visible = false, Enabled = false)]
		[SOInvoiceLinesSorting]
		protected virtual void ARTran_SortOrder_CacheAttached(PXCache sender) { }

		#endregion
		#region ARInvoice
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault()]
		[ARDocType.SOEntryList()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
		protected virtual void ARInvoice_DocType_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[ARInvoiceType.RefNbr(typeof(Search2<AR.Standalone.ARRegisterAlias.refNbr,
			InnerJoinSingleTable<ARInvoice, On<ARInvoice.docType, Equal<AR.Standalone.ARRegisterAlias.docType>,
				And<ARInvoice.refNbr, Equal<AR.Standalone.ARRegisterAlias.refNbr>>>,
			InnerJoinSingleTable<Customer, On<AR.Standalone.ARRegisterAlias.customerID, Equal<Customer.bAccountID>>>>,
			Where<AR.Standalone.ARRegisterAlias.docType, Equal<Optional<ARInvoice.docType>>,
				And<AR.Standalone.ARRegisterAlias.origModule, Equal<BatchModule.moduleSO>,
				And<Match<Customer, Current<AccessInfo.userName>>>>>,
			OrderBy<Desc<AR.Standalone.ARRegisterAlias.refNbr>>>), Filterable = true, IsPrimaryViewCompatible = true)]
		[ARInvoiceType.Numbering()]
		[ARInvoiceNbr()]
		protected virtual void ARInvoice_RefNbr_CacheAttached(PXCache sender)
		{
		}
		[SOOpenPeriod(
			sourceType: typeof(ARRegister.docDate),
			branchSourceType: typeof(ARRegister.branchID),
			masterFinPeriodIDType: typeof(ARRegister.tranPeriodID),
			IsHeader = true)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void ARInvoice_FinPeriodID_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(10, IsUnicode = true)]
		[PXFormula(typeof(
			IIf<Where<ExternalCall, Equal<True>, Or<PendingValue<ARInvoice.termsID>, IsNull>>,
				IIf<Where<Current<ARInvoice.docType>, NotEqual<ARDocType.creditMemo>>,
					Selector<ARInvoice.customerID, Customer.termsID>,
					Null>,
				ARInvoice.termsID>))]
		[PXUIField(DisplayName = "Terms", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Search<Terms.termsID, Where<Terms.visibleTo, Equal<TermsVisibleTo.all>, Or<Terms.visibleTo, Equal<TermsVisibleTo.customer>>>>), DescriptionField = typeof(Terms.descr), Filterable = true)]
		[SOInvoiceTerms()]
		protected override void ARInvoice_TermsID_CacheAttached(PXCache sender)
		{
		}
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID,
								Where<PaymentMethod.isActive, Equal<boolTrue>,
								And<PaymentMethod.useForAR, Equal<boolTrue>>>>), DescriptionField = typeof(PaymentMethod.descr))]
		protected virtual void _(Events.CacheAttached<ARInvoice.paymentMethodID> e)
		{
		}
		[PXDBDate()]
		[PXUIField(DisplayName = "Due Date", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void ARInvoice_DueDate_CacheAttached(PXCache sender)
		{
		}
		[PXDBDate()]
		[PXUIField(DisplayName = "Cash Discount Date", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void ARInvoice_DiscDate_CacheAttached(PXCache sender)
		{
		}
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.origDocAmt))]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void ARInvoice_CuryOrigDocAmt_CacheAttached(PXCache sender)
		{
		}
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.docBal), BaseCalc = false)]
		[PXUIField(DisplayName = "Balance", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		protected virtual void ARInvoice_CuryDocBal_CacheAttached(PXCache sender)
		{
		}
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.origDiscAmt))]
		[PXUIField(DisplayName = "Cash Discount", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void ARInvoice_CuryOrigDiscAmt_CacheAttached(PXCache sender)
		{
		}
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Line Total")]
		protected virtual void ARInvoice_CuryGoodsTotal_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXParent(typeof(Select<AR.Standalone.ARRegister,
			Where<AR.Standalone.ARRegister.noteID, Equal<Current<ARAdjust.invoiceID>>,
				And<Current<ARAdjust.adjgDocType>, Equal<ARDocType.creditMemo>>>>))]
		protected virtual void _(Events.CacheAttached<ARAdjust.invoiceID> eventArgs)
		{
		}

		//VAT fields names are different in SO and AR
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "VAT Exempt", Visibility = PXUIVisibility.Visible, Enabled = false)]
		protected virtual void _(Events.CacheAttached<ARInvoice.curyVatExemptTotal> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "VAT Taxable", Visibility = PXUIVisibility.Visible, Enabled = false)]
		protected virtual void _(Events.CacheAttached<ARInvoice.curyVatTaxableTotal> e)
		{
		}

		#endregion
		#region SOInvoice
		[CustomerActive(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Customer.acctName), Filterable = true)]
		[PXDBDefault(typeof(ARInvoice.customerID))]
		protected virtual void SOInvoice_CustomerID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region SOAdjust
		[PXDBInt()]
		[PXDefault()]
		protected virtual void SOAdjust_CustomerID_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault()]
		protected virtual void SOAdjust_AdjdOrderType_CacheAttached(PXCache sender)
		{
		}

		[PXRemoveBaseAttribute(typeof(PXSelectorAttribute))]
		[PXRemoveBaseAttribute(typeof(PXRestrictorAttribute))]
		[PXRemoveBaseAttribute(typeof(PXUnboundFormulaAttribute))]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		protected virtual void SOAdjust_AdjdOrderNbr_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(3, IsKey = true, IsFixed = true, InputMask = "")]
		[PXDefault()]
		protected virtual void SOAdjust_AdjgDocType_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXParent(typeof(Select<ARPaymentTotals,
			Where<ARPaymentTotals.docType, Equal<Current<SOAdjust.adjgDocType>>,
				And<ARPaymentTotals.refNbr, Equal<Current<SOAdjust.adjgRefNbr>>>>>), ParentCreate = true)]
		protected virtual void SOAdjust_AdjgRefNbr_CacheAttached(PXCache sender)
		{
		}

		[PXRemoveBaseAttribute(typeof(PXDBDecimalAttribute))]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDBCurrency(typeof(SOAdjust.adjdCuryInfoID), typeof(SOAdjust.adjAmt))]
		[PXUIField(DisplayName = "Applied To Order")]
		protected virtual void SOAdjust_CuryAdjdAmt_CacheAttached(PXCache sender)
		{
		}

		[PXDBDecimal(4)]
		[PXFormula(typeof(Maximum<Sub<SOAdjust.origAdjAmt, SOAdjust.adjBilledAmt>, decimal0>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		protected virtual void SOAdjust_AdjAmt_CacheAttached(PXCache sender)
		{
		}
		[PXDBDecimal(4)]
		[PXFormula(typeof(Maximum<Sub<SOAdjust.curyOrigAdjgAmt, SOAdjust.curyAdjgBilledAmt>, decimal0>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		protected virtual void SOAdjust_CuryAdjgAmt_CacheAttached(PXCache sender)
		{
		}
		[PXDBLong()]
		[PXDefault()]
		[CurrencyInfo]
		protected virtual void SOAdjust_AdjdOrigCuryInfoID_CacheAttached(PXCache sender)
		{
		}
		[PXDBLong()]
		[PXDefault()]
		[CurrencyInfo]
		protected virtual void SOAdjust_AdjgCuryInfoID_CacheAttached(PXCache sender)
		{
		}
		[PXDBLong()]
		[PXDefault()]
		[CurrencyInfo]
		protected virtual void SOAdjust_AdjdCuryInfoID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region ARInvoiceDiscountDetail
		[PXDBString(10, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIEnabled(typeof(Where<ARInvoiceDiscountDetail.type, NotEqual<DiscountType.ExternalDocumentDiscount>, And<ARInvoiceDiscountDetail.orderNbr, IsNull>>))]
		[PXUIField(DisplayName = "Discount Code", Required = false)]
		[PXSelector(typeof(Search<ARDiscount.discountID, Where<ARDiscount.type, NotEqual<DiscountType.LineDiscount>>>))]
		protected virtual void ARInvoiceDiscountDetail_DiscountID_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#region ARAdjust

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDBDecimalAttribute))]
		[PXDBCurrency(typeof(ARAdjust.adjdCuryInfoID), typeof(ARAdjust.adjAmt), BaseCalc = false)]
		[PXUnboundFormula(
			typeof(Switch<Case<Where<ARAdjust.voided, Equal<False>>, ARAdjust.curyAdjdAmt>, decimal0>),
			typeof(SumCalc<ARInvoice.curyPaymentTotal>))]
		[PXUnboundFormula(
			typeof(Switch<Case<Where<ARAdjust.voided, Equal<False>>, ARAdjust.curyAdjdAmt>, decimal0>),
			typeof(SumCalc<AR.Standalone.ARInvoiceAdjusted.curyPaymentTotal>))]
		[PXUnboundFormula(
			typeof(Switch<Case<Where<ARAdjust.voided, Equal<False>, And<ARAdjust.released, Equal<False>,
				And<Where<ARAdjust.paymentID, IsNotNull, And<ARAdjust.paymentReleased, NotEqual<True>,
					Or<ARAdjust.invoiceID, IsNotNull, And<Parent<AR.Standalone.ARRegister.released>, NotEqual<True>>>>>>>>,
				ARAdjust.curyAdjdAmt>, decimal0>),
			typeof(SumCalc<ARInvoice.curyUnreleasedPaymentAmt>))]
		[PXUnboundFormula(
			typeof(Switch<Case<Where<ARAdjust.voided, Equal<False>, And<ARAdjust.released, Equal<False>,
				And<Where<ARAdjust.paymentID, IsNotNull, And<ARAdjust.paymentReleased, Equal<True>,
					Or<ARAdjust.invoiceID, IsNotNull, And<Parent<AR.Standalone.ARRegister.released>, Equal<True>>>>>>>>,
				ARAdjust.curyAdjdAmt>, decimal0>),
			typeof(SumCalc<ARInvoice.curyPaidAmt>))]
		protected override void _(Events.CacheAttached<ARAdjust.curyAdjdAmt> e) { }

		#endregion

		#region ARAdjust2
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDBString(3, IsKey = true, IsFixed = true, InputMask = "")]
		[ARPaymentType.List()]
		[PXDefault()]
		[PXUIField(DisplayName = "Doc. Type")]
		protected virtual void ARAdjust2_AdjgDocType_CacheAttached(PXCache sender) {}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDefault]
		[PXRemoveBaseAttribute(typeof(PXSelectorAttribute))]
		[ARPaymentType.AdjgRefNbr(typeof(Search<ARPayment.refNbr,
			Where<ARPayment.docType, Equal<Optional<ARAdjust2.adjgDocType>>>>),
			Filterable = true)]
		protected override void ARAdjust2_AdjgRefNbr_CacheAttached(PXCache sender) { }

		[PXCustomizeBaseAttribute(typeof(PXDBCurrencyAttribute), nameof(PXDBCurrencyAttribute.MinValue), (double)decimal.MinValue)]
		protected virtual void ARAdjust2_CuryAdjdAmt_CacheAttached(PXCache sender) { }
		#endregion

		#endregion

		public PXSelectOrderBy<ExternalTransaction, OrderBy<Desc<ExternalTransaction.transactionID>>> ExternalTran;
		public virtual IEnumerable externalTran()
		{
			foreach (ExternalTransaction tran in PXSelectReadonly<ExternalTransaction,
				Where<ExternalTransaction.refNbr, Equal<Current<ARInvoice.refNbr>>,
					And<ExternalTransaction.docType, Equal<Current<ARInvoice.docType>>>>,
				OrderBy<Desc<ExternalTransaction.transactionID>>>.SelectMultiBound(this, PXView.Currents))
			{
				yield return tran;
			}

			foreach (ExternalTransaction tran in PXSelectReadonly2<ExternalTransaction,
					InnerJoin<SOOrderShipment, On<SOOrderShipment.orderNbr, Equal<ExternalTransaction.origRefNbr>,
						And<SOOrderShipment.orderType, Equal<ExternalTransaction.origDocType>>>>,
					Where<SOOrderShipment.invoiceNbr, Equal<Current<ARInvoice.refNbr>>,
						And<SOOrderShipment.invoiceType, Equal<Current<ARInvoice.docType>>,
						And<ExternalTransaction.refNbr, IsNull>>>,
					OrderBy<Desc<ExternalTransaction.transactionID>>>.SelectMultiBound(this, PXView.Currents))
			{
				yield return tran;
			}
		}

		public PXSelectOrderBy<CCProcTran, OrderBy<Desc<CCProcTran.tranNbr>>> ccProcTran;

		public virtual IEnumerable ccproctran()
		{
			var externalTrans = ExternalTran.Select();
			var query = new PXSelect<CCProcTran,
				Where<CCProcTran.transactionID, Equal<Required<CCProcTran.transactionID>>>>(this);
			foreach (ExternalTransaction extTran in externalTrans)
			{
				foreach (CCProcTran procTran in query.Select(extTran.TransactionID))
				{
					yield return procTran;
				}
			}
		}

		public PXSetup<SOSetup> sosetup;
		public PXSetup<ARSetup> arsetup;
		public PXSetup<Company> Company;
		public PXSelect<SOOrder,
			Where<SOOrder.orderType, Equal<Required<SOOrder.orderType>>,
				And<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>> soorder;

		public SOInvoiceLineSplittingExtension LineSplittingExt => FindImplementation<SOInvoiceLineSplittingExtension>();
		public SOInvoiceItemAvailabilityExtension ItemAvailabilityExt => FindImplementation<SOInvoiceItemAvailabilityExtension>();

		public PXInitializeState<ARInvoice> initializeState;

		public PXAction<ARInvoice> putOnHold;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Hold")]
		protected virtual IEnumerable PutOnHold(PXAdapter adapter) => adapter.Get();

		public PXAction<ARInvoice> releaseFromHold;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Remove Hold")]
		protected virtual IEnumerable ReleaseFromHold(PXAdapter adapter) => adapter.Get();

		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Release", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public override IEnumerable Release(PXAdapter adapter)
		{
			bool isMassProcess = adapter.MassProcess;
			List<ARRegister> list = new List<ARRegister>();
			foreach (ARInvoice ardoc in adapter.Get<ARInvoice>())
			{
				OnBeforeRelease(ardoc);
				Document.Cache.MarkUpdated(ardoc, assertError: true);
				list.Add(ardoc);
			}

			Save.Press();

			PXLongOperation.StartOperation(this, delegate ()
			{
				SOInvoiceEntry ie = PXGraph.CreateInstance<SOInvoiceEntry>();
				ie.ReleaseInvoiceProc(list, isMassProcess);
			});
			return list;
		}

		public virtual void ReleaseInvoiceProc(List<ARRegister> list, bool isMassProcess)
			=> ReleaseInvoiceProcImpl(list, isMassProcess);

		public void ReleaseInvoiceProcImpl(List<ARRegister> list, bool isMassProcess)
		{
			PXTimeStampScope.SetRecordComesFirst(typeof(ARInvoice), true);
			PXNoteAttribute.ForcePassThrow<ARTran.noteID>(this.Freight.Cache);

			SOOrderShipmentProcess docgraph = PXGraph.CreateInstance<SOOrderShipmentProcess>();
			var invoicePostingContext = this.GetInvoicePostingContext();

			var createdIssuesToRelease = new HashSet<INRegister>();
			var processedInvoices = new HashSet<object>();
			try
			{
				var lastCcTransByInvoices = new Dictionary<ARRegister, ExternalTransactionState>();
					foreach (ARRegister ardoc in list.Where(x => x != null))
				{
					var ccProcInvoiceTrans = ExternalTranHelper.GetSOInvoiceExternalTrans(this, (ARInvoice)ardoc);
					if (ccProcInvoiceTrans != null)
					{
						var state = ExternalTranHelper.GetActiveTransactionState(this, ccProcInvoiceTrans);
						lastCcTransByInvoices.Add(ardoc, state);
					}
				}

				ARDocumentRelease.ReleaseDoc(list, isMassProcess, null, null, delegate (ARRegister ardoc)
				{
					docgraph.Clear();

					var orderShipments = docgraph.UpdateOrderShipments(ardoc, processedInvoices);

					var directShipmentsToCreate = new List<SOOrderShipment>();
					docgraph.CompleteMiscLines(ardoc, directShipmentsToCreate);
					docgraph.UpdateApplications(ardoc, orderShipments.RowCast<SOOrderShipment>().Union(directShipmentsToCreate));

					PX.Objects.SO.SOInvoice.Events
						.Select(e => e.InvoiceReleased)
						.FireOn(docgraph, PX.Objects.SO.SOInvoice.PK.Find(docgraph, ardoc.DocType, ardoc.RefNbr));

					docgraph.Save.Press();

						var issues = new DocumentList<INRegister>(invoicePostingContext.IssueEntry);
						this.PostInvoice(invoicePostingContext, ardoc as ARInvoice, issues, directShipmentsToCreate);

						if (this.sosetup.Current.AutoReleaseIN == true && issues.Count > 0 && issues.All(issue => issue.Hold == false))
						{
							createdIssuesToRelease.AddRange(issues);
						}

					CreateDirectShipments(directShipmentsToCreate, orderShipments.RowCast<SOOrderShipment>().ToList());
					docgraph.OnInvoiceReleased(ardoc, orderShipments);

					this.CopyFreightNotesAndFilesToARTran(ardoc);
				});
			}
			finally
			{
				if (createdIssuesToRelease.Any())
					ReleaseCreatedIssues(invoicePostingContext.IssueEntry, createdIssuesToRelease);

			}
		}

		protected virtual void ReleaseCreatedIssues(INIssueEntry issueEntry, IEnumerable<INRegister> createdIssues)
		{
				issueEntry.Clear();
				issueEntry.release.Press(createdIssues).Consume();
			}

		public PXAction<ARInvoice> post;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Post Invoice to IN", Visible = false)]
		[Obsolete("The action is obsolete as Posting to IN became a part of the Release action.")]
		protected virtual IEnumerable Post(PXAdapter adapter)
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.inventory>())
				return adapter.Get();

			bool isMassProcess = adapter.MassProcess;
			List<ARRegister> list = new List<ARRegister>();
			foreach (ARInvoice order in adapter.Get<ARInvoice>())
			{
				list.Add(order);
			}

			Save.Press();

			PXLongOperation.StartOperation(this, delegate ()
			{
				SOInvoiceEntry ie = PXGraph.CreateInstance<SOInvoiceEntry>();
				ie.PostInvoiceToINProc(list, isMassProcess);
			});

			return adapter.Get();
		}

		private void PostInvoiceToINProc(List<ARRegister> list, bool isMassProcess)
		{
			var invoicePostingContext = this.GetInvoicePostingContext();
			DocumentList<INRegister> inlist = new DocumentList<INRegister>(invoicePostingContext.IssueEntry);

			bool failed = false;

			foreach (ARInvoice ardoc in list)
			{
				try
				{
					using (PXTransactionScope ts = new PXTransactionScope())
					{
						var directShipmentsToCreate = new List<SOOrderShipment>();

						this.PostInvoice(invoicePostingContext, ardoc, inlist, directShipmentsToCreate);
						this.CreateDirectShipments(directShipmentsToCreate, new List<SOOrderShipment>());

						ts.Complete();
					}

					if (isMassProcess)
					{
						PXProcessing<ARInvoice>.SetInfo(list.IndexOf(ardoc), ActionsMessages.RecordProcessed);
					}
				}
				catch (Exception ex)
				{
					if (!isMassProcess)
					{
						throw;
					}
					PXProcessing<ARInvoice>.SetError(list.IndexOf(ardoc), ex);
					failed = true;
				}
			}

			if (this.sosetup.Current.AutoReleaseIN == true && inlist.Count > 0 && inlist[0].Hold == false)
			{
				INDocumentRelease.ReleaseDoc(inlist, false);
			}

			if (failed)
			{
				throw new PXOperationCompletedWithErrorException(ErrorMessages.SeveralItemsFailed);
			}
		}

		[PXUIField(DisplayName = "Reports", MapEnableRights = PXCacheRights.Select)]
		[PXButton(CommitChanges = true, MenuAutoOpen = true, SpecialType = PXSpecialButtonType.ReportsFolder)]
		protected override IEnumerable Report(PXAdapter adapter,
			[PXString(8, InputMask = "CC.CC.CC.CC")]
			string reportID
			)
		{
			List<ARInvoice> list = adapter.Get<ARInvoice>().ToList();
			if (!String.IsNullOrEmpty(reportID))
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				string actualReportID = null;
				PXReportRequiredException ex = null;
				Dictionary<PX.SM.PrintSettings, PXReportRequiredException> reportsToPrint = new Dictionary<PX.SM.PrintSettings, PXReportRequiredException>();

				foreach (ARInvoice doc in list)
				{
					parameters = new Dictionary<string, string>();
					parameters["ARInvoice.DocType"] = doc.DocType;
					parameters["ARInvoice.RefNbr"] = doc.RefNbr;

					actualReportID = new NotificationUtility(this).SearchCustomerReport(reportID, doc.CustomerID, doc.BranchID);
					ex = PXReportRequiredException.CombineReport(ex, actualReportID, parameters, OrganizationLocalizationHelper.GetCurrentLocalization(this));
					ex.Mode = PXBaseRedirectException.WindowMode.New;

					reportsToPrint = PX.SM.SMPrintJobMaint.AssignPrintJobToPrinter(reportsToPrint, parameters, adapter,
						new NotificationUtility(this).SearchPrinter, ARNotificationSource.Customer, reportID, actualReportID,
						doc.BranchID, OrganizationLocalizationHelper.GetCurrentLocalization(this));
				}

				if (ex != null)
				{
					LongOperationManager.StartAsyncOperation(Guid.NewGuid(), async ct =>
					{
						await PX.SM.SMPrintJobMaint.CreatePrintJobGroups(reportsToPrint, ct);
					});
					throw ex;
				}
			}
			return list;
		}

		public PXAction<ARInvoice> printInvoice;
		[PXButton, PXUIField(DisplayName = "Print Invoice", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable PrintInvoice(PXAdapter adapter, string reportID = null) => Report(adapter.Apply(it => it.Menu = "Print Invoice"), reportID ?? "SO643000");

		public PXAction<ARInvoice> arEdit;
		[PXButton, PXUIField(DisplayName = "AR Edit", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable AREdit(PXAdapter adapter, string reportID = null) => Report(adapter.Apply(it => it.Menu = "AR Edit"), reportID ?? "AR610500");

		[PXButton, PXUIField(DisplayName = "Email Invoice", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected override IEnumerable EmailInvoice(
			PXAdapter adapter,
			[PXString]
			string notificationCD = null) => Notification(adapter, notificationCD ?? "SO INVOICE");

		[PXButton, PXUIField(DisplayName = "Reverse", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public override IEnumerable ReverseInvoice(PXAdapter adapter) => throw new PXInvalidOperationException();

		public SOInvoiceEntry()
			: base()
		{
			{
				SOSetup record = sosetup.Current;
			}

			ARSetupNoMigrationMode.EnsureMigrationModeDisabled(this);

			Document.View = new PXView(this, false, new Select2<ARInvoice,
			LeftJoinSingleTable<Customer, On<ARInvoice.customerID, Equal<Customer.bAccountID>>>,
			Where<ARInvoice.docType, Equal<Optional<ARInvoice.docType>>,
			And<ARInvoice.origModule, Equal<BatchModule.moduleSO>,
			And<Where<Customer.bAccountID, IsNull,
			Or<Match<Customer, Current<AccessInfo.userName>>>>>>>>());

			this.Views["Document"] = Document.View;

			BqlCommand cmd = Transactions.View.BqlSelect.WhereNew<
			Where<ARTran.tranType, Equal<Current<ARInvoice.docType>>,
			And<ARTran.refNbr, Equal<Current<ARInvoice.refNbr>>,
			And<ARTran.lineType, NotEqual<SOLineType.discount>,
			And<ARTran.lineType, NotEqual<SOLineType.freight>>>>>>();

			Transactions.View = new PXView(this, false, cmd, new PXSelectDelegate(transactions));

			this.Views["Transactions"] = Transactions.View;

			PXUIFieldAttribute.SetVisible<SOOrderShipment.orderType>(shipmentlist.Cache, null, true);
			PXUIFieldAttribute.SetVisible<SOOrderShipment.orderNbr>(shipmentlist.Cache, null, true);
			PXUIFieldAttribute.SetVisible<SOOrderShipment.shipmentNbr>(shipmentlist.Cache, null, true);

			PXDBLiteDefaultAttribute.SetDefaultForInsert<SOOrderShipment.invoiceNbr>(shipmentlist.Cache, null, true);
			PXDBLiteDefaultAttribute.SetDefaultForUpdate<SOOrderShipment.invoiceNbr>(shipmentlist.Cache, null, true);

			PXUIFieldAttribute.SetEnabled<ARAdjust2.curyAdjgDiscAmt>(Adjustments.Cache, null, false);

			//reverseInvoiceAndApplyToMemo.SetVisible(false);  //A dirty workaround that hides inherited "Reverse and Aplly To Memo" button. Caused by a platform retrieving actions from base graph.

			TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(Transactions.Cache, null, TaxCalc.ManualLineCalc);

			Transactions.CustomComparer = Comparer<PXResult>.Create((a, b) =>
			{
				ARTran aTran = PXResult.Unwrap<ARTran>(a);
				ARTran bTran = PXResult.Unwrap<ARTran>(b);

				return string.Compare(string.Format("{0}.{1}.{2:D7}.{3}", aTran.SOOrderType, aTran.SOOrderNbr, aTran.SOOrderSortOrder, aTran.SOShipmentNbr),
					string.Format("{0}.{1}.{2:D7}.{3}", bTran.SOOrderType, bTran.SOOrderNbr, bTran.SOOrderSortOrder, bTran.SOShipmentNbr));
			});

			var releaseRetainage = Actions[nameof(ARInvoiceEntryRetainage.ReleaseRetainage)];
			if (releaseRetainage != null)
			{
				action.AddMenuAction(releaseRetainage);
				action.SetVisible(nameof(ARInvoiceEntryRetainage.ReleaseRetainage), false);
			}

			Approval.SuppressApproval = true;

			ARDiscountDetails.OrderByNew<
				OrderBy<Asc<ARInvoiceDiscountDetail.orderType,
					Asc<ARInvoiceDiscountDetail.orderNbr,
					Asc<ARInvoiceDiscountDetail.lineNbr>>>>>();

			this.Views.Caches.Add(typeof(NoteDoc));
		}

		protected virtual Services.InvoicePostingContext GetInvoicePostingContext()
		{
			return new Services.InvoicePostingContext(FinPeriodUtils);
		}

		protected override void RecalculateDiscountsProc(bool redirect)
		{
			Document.Current.DeferPriceDiscountRecalculation = false;
			try
			{
				base.RecalculateDiscountsProc(redirect);
			}
			finally
			{
				Document.Current.DeferPriceDiscountRecalculation = soordertype.Current.DeferPriceDiscountRecalculation;
			}
		}

		protected virtual void RecalculatePricesAndDiscountsOnPersist(IEnumerable<ARInvoice> docs)
		{
			foreach (ARInvoice doc in docs.Where(doc => doc.DeferPriceDiscountRecalculation == true && doc.IsPriceAndDiscountsValid == false))
			{
				if (!object.ReferenceEquals(Document.Current, doc))
				{
					Document.Current = doc;
				}

				TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(this.Transactions.Cache, null, TaxCalc.ManualLineCalc | TaxCalc.RecalculateAlways);

				doc.DeferPriceDiscountRecalculation = false;
				try
				{
					ARDiscountEngine.AutoRecalculatePricesAndDiscounts(Transactions.Cache, Transactions, null, ARDiscountDetails,
						Document.Current.CustomerLocationID, Document.Current.DocDate, GetDefaultARDiscountCalculationOptions(doc, true));

					doc.IsPriceAndDiscountsValid = true;
				}
				finally
				{
					doc.DeferPriceDiscountRecalculation = true;
				}
			}
		}

		public override void Persist()
		{
			CopyFreightNotesAndFilesToARTran();

			if (this.Caches[typeof(SOOrderShipment)] != null)
			{
				foreach (SOOrderShipment sos in this.Caches[typeof(SOOrderShipment)].Cached)
				{
					if (sos.HasUnhandledErrors == true)
					{
						throw new PXException(Messages.AddingOrderShipmentErrorsOccured, sos.OrderNbr, sos.ShipmentNbr);
					}
					else if (sos.IsPartialInvoiceConstraintViolated == true)
					{
						throw new PXException(Messages.PartialInvoice);
					}
				}
			}

			var invoices = Document.Cache.Inserted
				.Concat_(Document.Cache.Updated)
				.Cast<ARInvoice>();

			RecalculatePricesAndDiscountsOnPersist(invoices);

			PXCache solinecache = Caches[typeof(SOLine2)];
			foreach (SOLine2 soline in solinecache.Updated)
			{
				PXTimeStampScope.DuplicatePersisted(solinecache, soline, typeof(SOLine));
			}

			DeleteZeroAdjustments();

			VerifyDocumentBalanceAgainstAdjustmentBalance();

			Transactions.RenumberAllBeforePersist = LinesSortingAttribute.AllowSorting(Transactions.Cache, SODocument.Current);

			base.Persist();
		}

		#region Persist-releated methods
		protected virtual void DeleteZeroAdjustments()
		{
			foreach (ARAdjust2 adj in Adjustments.Cache.Inserted)
			{
				if (adj.CuryAdjdAmt == 0m && adj.Recalculatable != true)
				{
					Adjustments.Cache.SetStatus(adj, PXEntryStatus.InsertedDeleted);
				}
			}

			foreach (ARAdjust2 adj in Adjustments.Cache.Updated
				.RowCast<ARAdjust2>()
				.Where(adj => adj.CuryAdjdAmt == 0m && adj.Recalculatable != true))
			{
				Adjustments.Cache.SetStatus(adj, PXEntryStatus.Deleted);
			}
		}

		protected virtual void VerifyDocumentBalanceAgainstAdjustmentBalance()
		{
			foreach (ARInvoice arDoc in Document.Cache.Cached
				.Cast<ARInvoice>()
				.Where(ardoc =>
					Document.Cache.GetStatus(ardoc).IsIn(PXEntryStatus.Inserted, PXEntryStatus.Updated)
					&& ardoc.DocType == ARDocType.Invoice
					&& ardoc.Released == false
					&& ardoc.ApplyPaymentWhenTaxAvailable != true))
			{
				SOInvoice soDoc = SODocument.Select(arDoc.DocType, arDoc.RefNbr);

				if (arDoc.CuryDocBal - arDoc.CuryBalanceWOTotal - arDoc.CuryPaymentTotal < 0m)
				{
					foreach (ARAdjust2 adj in Adjustments_Inv.View
						.SelectMultiBound(new object[] { arDoc })
						.RowCast<ARAdjust2>().Where(adj =>
							Adjustments.Cache.GetStatus(adj).IsIn(PXEntryStatus.Inserted, PXEntryStatus.Updated)
							|| ((decimal?)Document.Cache.GetValueOriginal<ARInvoice.curyDocBal>(arDoc) != arDoc.CuryDocBal)))
					{
						Adjustments.Cache.MarkUpdated(adj, assertError: true);
						Adjustments.Cache.RaiseExceptionHandling<ARAdjust2.curyAdjdAmt>(adj, adj.CuryAdjdAmt, new PXSetPropertyException(AR.Messages.Application_Amount_Cannot_Exceed_Document_Amount));
						throw new PXException(AR.Messages.Application_Amount_Cannot_Exceed_Document_Amount);
					}
				}
			}
		}
		#endregion

		public override ARInvoice InsertReversalARInvoice(ARInvoice arInvoice)
		{
			if (arInvoice.DocType != ARInvoiceType.Invoice && arInvoice.RefNbr == null)
			{
				arInvoice.RefNbr = arInvoice.OrigRefNbr;
			}

			return base.InsertReversalARInvoice(arInvoice);
		}

		public override ARInvoice SetRefNumber(ARInvoice arInvoice, string refNbr)
		{
			SOInvoice soInvoice = SODocument.Current;
			soInvoice.RefNbr = refNbr;
			SODocument.Cache.Normalize();
			SODocument.Update(soInvoice);

			return base.SetRefNumber(arInvoice, refNbr);
		}

		public override void RecalcUnbilledTax()
		{
			var orders = Transactions.SelectMain()
				.Where(o => o.SOOrderType != null && o.SOOrderNbr != null)
				.Select(o => new { o.SOOrderType, o.SOOrderNbr })
				.Distinct();

			SOOrderEntry soOrderEntry = PXGraph.CreateInstance<SOOrderEntry>();
			foreach (var order in orders)
			{
				soOrderEntry.Clear(PXClearOption.ClearAll);
				soOrderEntry.Document.Current = soOrderEntry.Document.Search<SOOrder.orderNbr>(order.SOOrderNbr, order.SOOrderType);
				if (IsExternalTax(soOrderEntry.Document.Current.TaxZoneID))
					soOrderEntry.CalculateExternalTax(soOrderEntry.Document.Current);
				soOrderEntry.Persist();
			}
		}

		protected override void ARInvoice_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var arInvoice = (ARInvoice)e.Row;
			if (e.Operation != PXDBOperation.Delete && (arInvoice.DocType == ARDocType.CashSale || arInvoice.DocType == ARDocType.CashReturn))
			{
				ValidateTaxConfiguration(sender, arInvoice);
			}

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert)
			{
				SOOrderShipment orderShipment = PXSelect<SOOrderShipment,
					Where<SOOrderShipment.invoiceType, Equal<Required<SOOrderShipment.invoiceType>>,
						And<SOOrderShipment.invoiceNbr, Equal<Required<SOOrderShipment.invoiceNbr>>>>>
						.SelectSingleBound(this, null, arInvoice.DocType, arInvoice.RefNbr);

				if (orderShipment != null)
				{
					SOOrderType orderType = SOOrderType.PK.Find(this, orderShipment.OrderType);
					if (orderType != null)
					{
						if (string.IsNullOrEmpty(arInvoice.RefNbr) && orderType.UserInvoiceNumbering == true)
						{
							throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<SOOrder.invoiceNbr>(soorder.Cache));
						}

						if (orderType.MarkInvoicePrinted == true)
						{
							arInvoice.Printed = true;
						}

						if (orderType.MarkInvoiceEmailed == true)
						{
							arInvoice.Emailed = true;
						}

						AutoNumberAttribute.SetNumberingId<ARInvoice.refNbr>(Document.Cache, orderType.ARDocType, orderType.InvoiceNumberingID);
					}
				}
			}

			if (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
			{
				if ((arInvoice.CuryDiscTot ?? 0m) > (Math.Abs((arInvoice.CuryGoodsTotal ?? 0m) + (arInvoice.CuryMiscTot ?? 0m))))
				{
					if (sender.RaiseExceptionHandling<ARInvoice.curyDiscTot>(e.Row, arInvoice.CuryDiscTot,
						new PXSetPropertyException(AR.Messages.DiscountGreaterDetailTotal, PXErrorLevel.Error)))
					{
						throw new PXRowPersistingException(typeof(ARInvoice.curyDiscTot).Name, null,
							AR.Messages.DiscountGreaterDetailTotal);
					}
				}
			}

			base.ARInvoice_RowPersisting(sender, e);
		}

		protected virtual void ARInvoice_OrigModule_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = GL.BatchModule.SO;
			e.Cancel = true;
		}

		protected override void ARInvoice_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			base.ARInvoice_RowInserted(sender, e);

			ARInvoice aRInvoice = (ARInvoice)e.Row;
			SODocument.Cache.Insert();
			SODocument.Cache.IsDirty = false;

			SODocument.Current.AdjDate = aRInvoice.DocDate;
			SODocument.Current.AdjFinPeriodID = aRInvoice.FinPeriodID;
			SODocument.Current.AdjTranPeriodID = aRInvoice.TranPeriodID;
			SODocument.Current.NoteID = aRInvoice.NoteID;
			SODocument.Current.CuryID = aRInvoice.CuryID;
		}

		protected override void ARTran_CuryUnitPrice_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!cancelUnitPriceCalculation)
				base.ARTran_CuryUnitPrice_FieldVerifying(sender, e);
		}

		protected override void ARTran_CuryUnitPrice_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			ARTran row = (ARTran)e.Row;

			if (row?.InventoryID != null && row.UOM != null && row.IsFree != true && row.ManualPrice != true && !cancelUnitPriceCalculation)
			{
				string customerPriceClass = ARPriceClass.EmptyPriceClass;
				Location c = location.Select();

				if (!string.IsNullOrEmpty(c?.CPriceClassID))
					customerPriceClass = c.CPriceClassID;

				DateTime date = Document.Current.DocDate.Value;
				string taxCalcMode = Document.Current.TaxCalcMode;

				if (row.TranType == ARDocType.CreditMemo && row.OrigInvoiceDate != null)
					date = row.OrigInvoiceDate.Value;

				CurrencyInfo currencyInfo = currencyinfo.Select();

				(ARSalesPriceMaint.SalesPriceItem spItem, decimal? price) = ARSalesPriceMaint.SingleARSalesPriceMaint.
					GetSalesPriceItemAndCalculatedPrice(
					cache, customerPriceClass,
					row.CustomerID,
					row.InventoryID,
					row.SiteID,
					currencyInfo.GetCM(),
					row.UOM,
					row.Qty,
					date,
					row.CuryUnitPrice,
					taxCalcMode);
				// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs [Regular C# field used for optimization]
				row.SkipLineDiscountsBuffer = spItem?.SkipLineDiscounts;
				e.NewValue = price;
				ARSalesPriceMaint.CheckNewUnitPrice<ARTran, ARTran.curyUnitPrice>(cache, row, price);
			}
			else
			{
				decimal? curyUnitPrice = row.CuryUnitPrice;
				e.NewValue = curyUnitPrice ?? 0m;
				e.Cancel = curyUnitPrice != null;
			}
		}

		private (ARSalesPriceMaint.SalesPriceItem, decimal?) GetSalesPriceItemAndCalculatedPrice(
			PXCache sender,
			string custPriceClass,
			int? customerID,
			int? inventoryID,
			int? siteID,
			CM.CurrencyInfo currencyinfo,
			string UOM,
			decimal? quantity,
			DateTime date,
			decimal? currentUnitPrice,
			string taxCalcMode)
		{
			var arSalesPriceMaint = ARSalesPriceMaint.SingleARSalesPriceMaint;
			bool alwaysFromBase = arSalesPriceMaint.GetAlwaysFromBaseCurrencySetting(sender);
			SalesPriceItem spItem = arSalesPriceMaint.CalculateSalesPriceItem(sender, custPriceClass, customerID, inventoryID, siteID, currencyinfo, Math.Abs(quantity ?? 0m), UOM, date, alwaysFromBase, false, taxCalcMode);
			decimal? salesPrice = arSalesPriceMaint.AdjustSalesPrice(sender, spItem, inventoryID, currencyinfo, UOM);
			return (spItem, salesPrice ?? 0);
		}

		[PXFormula(typeof(Current<SOSetup.deferPriceDiscountRecalculation>))]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		public void ARInvoice_DeferPriceDiscountRecalculation_CacheAttached(PXCache sender)
		{
		}

		protected override void ARInvoice_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			bool? originalRequireControlTotal = ARSetup.Current.RequireControlTotal;

			ARInvoice doc = e.Row as ARInvoice;
			ARInvoice oldDoc = e.OldRow as ARInvoice;

			if (doc.DocType == ARDocType.CashSale || doc.DocType == ARDocType.CashReturn)
				ARSetup.Current.RequireControlTotal = true;

			bool docDateChanged = !sender.ObjectsEqual<ARInvoice.docDate>(oldDoc, doc);

			try
			{
				if (docDateChanged)
				{
					Document.Current.DisableAutomaticDiscountCalculation = true;
				}

				base.ARInvoice_RowUpdated(sender, e);
			}
			finally
			{
				if (docDateChanged)
					Document.Current.DisableAutomaticDiscountCalculation = false; //For now, AR and SO Invoices don't allow to control this paramenter from the UI, so it should always be reverted to false
				ARSetup.Current.RequireControlTotal = originalRequireControlTotal;
			}

			if (doc != null && doc.RefNbr == null)
				return;

			if (doc.DeferPriceDiscountRecalculation == true && !sender.ObjectsEqual<ARInvoice.taxZoneID>(e.OldRow, doc))
			{
				doc.IsPriceAndDiscountsValid = false;
			}

			if ((doc.DocType == ARDocType.CashSale || doc.DocType == ARDocType.CashReturn) && doc.Released != true)
			{
				if (sender.ObjectsEqual<ARInvoice.curyDocBal, ARInvoice.curyOrigDiscAmt>(e.Row, e.OldRow) == false && doc.CuryDocBal - doc.CuryOrigDiscAmt != doc.CuryOrigDocAmt)
				{
					if (doc.CuryDocBal != null && doc.CuryOrigDiscAmt != null && doc.CuryDocBal != 0)
						sender.SetValueExt<ARInvoice.curyOrigDocAmt>(doc, doc.CuryDocBal - doc.CuryOrigDiscAmt);
					else
						sender.SetValueExt<ARInvoice.curyOrigDocAmt>(doc, 0m);
				}
				else if (sender.ObjectsEqual<ARInvoice.curyOrigDocAmt>(e.Row, e.OldRow) == false)
				{
					if (doc.CuryDocBal != null && doc.CuryOrigDocAmt != null && doc.CuryDocBal != 0)
						sender.SetValueExt<ARInvoice.curyOrigDiscAmt>(doc, doc.CuryDocBal - doc.CuryOrigDocAmt);
					else
						sender.SetValueExt<ARInvoice.curyOrigDiscAmt>(doc, 0m);
				}
			}

			if (doc != null && doc.CuryDocBal != null && doc.Hold != true && doc.CuryDocBal < 0m && SODocument.Current != null && Document.Current.CuryPremiumFreightAmt < 0m && (doc.CuryDocBal - Document.Current.CuryPremiumFreightAmt) >= 0m)
			{
				sender.RaiseExceptionHandling<ARInvoice.curyDocBal>(doc, doc.CuryDocBal,
					new PXSetPropertyException(Messages.DocumentBalanceNegativePremiumFreight));
			}

			if ((doc.DocType == ARDocType.CashSale || doc.DocType == ARDocType.CashReturn) && doc.Released != true && doc.Hold != true)
			{
				if (doc.CuryDocBal < doc.CuryOrigDocAmt)
				{
					sender.RaiseExceptionHandling<ARInvoice.curyOrigDocAmt>(doc, doc.CuryOrigDocAmt, new PXSetPropertyException(AR.Messages.CashSaleOutOfBalance));
				}
				else
				{
					sender.RaiseExceptionHandling<ARInvoice.curyOrigDocAmt>(doc, doc.CuryOrigDocAmt, null);
				}
			}

			if (!sender.ObjectsEqual<ARInvoice.customerID, ARInvoice.docDate, ARInvoice.finPeriodID, ARInvoice.curyTaxTotal, ARInvoice.curyOrigDocAmt, ARInvoice.docDesc, ARInvoice.curyOrigDiscAmt, ARInvoice.hold>(e.Row, e.OldRow))
			{
				SOInvoice invoice = SODocument.Select();
				if (IsImport && invoice == null && SODocument.Current != null && sender.Current is ARInvoice)
				{
					if ((((ARInvoice)sender.Current).DocType != SODocument.Current.DocType
						|| ((ARInvoice)sender.Current).RefNbr != SODocument.Current.RefNbr)
						&& SODocument.Cache.GetStatus(SODocument.Current) == PXEntryStatus.Inserted
						&& sender.Locate(new ARInvoice { DocType = SODocument.Current.DocType, RefNbr = SODocument.Current.RefNbr }) == null)
					{
						SODocument.Cache.Delete(SODocument.Current);
					}
				}

				SODocument.Current = invoice ?? (SOInvoice)SODocument.Cache.Insert();
				SODocument.Current.CustomerID = doc.CustomerID;

				if (doc.DocType.IsIn(ARDocType.CashSale, ARDocType.CashReturn, ARDocType.Invoice, ARDocType.CreditMemo)
					&& !sender.ObjectsEqual<ARInvoice.customerID>(e.Row, e.OldRow))
				{
					SODocument.Cache.SetDefaultExt<SOInvoice.paymentMethodID>(SODocument.Current);
					SODocument.Cache.SetDefaultExt<SOInvoice.pMInstanceID>(SODocument.Current);
				}

				SODocument.Current.AdjDate = doc.DocDate;
				SODocument.Current.DepositAfter = doc.DocDate;
				SODocument.Current.AdjFinPeriodID = doc.FinPeriodID;
				SODocument.Current.AdjTranPeriodID = doc.TranPeriodID;
				SODocument.Current.CuryPaymentAmt = doc.CuryOrigDocAmt - doc.CuryOrigDiscAmt - doc.CuryPaymentTotal;
				SODocument.Current.DocDesc = doc.DocDesc;
				SODocument.Current.CuryID = doc.CuryID;
				SODocument.Current.PaymentProjectID = PM.ProjectDefaultAttribute.NonProject();
				SODocument.Current.Hold = doc.Hold;

				SODocument.Cache.MarkUpdated(SODocument.Current, assertError: true);
			}

			if (!sender.ObjectsEqual<ARInvoice.curyPaymentTotal>(e.OldRow, e.Row))
			{
				SOInvoice invoice = SODocument.Select();
				if (invoice != null)
				{
					SODocument.Current = invoice;
					SODocument.Current.CuryPaymentAmt = doc.CuryOrigDocAmt - doc.CuryOrigDiscAmt - doc.CuryPaymentTotal;
				}
			}

			if (e.ExternalCall && sender.GetStatus(doc) != PXEntryStatus.Deleted && !sender.ObjectsEqual<ARInvoice.curyDiscTot>(e.OldRow, e.Row))
			{
				//Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers 
				ARDiscountEngine.SetTotalDocDiscount(Transactions.Cache, Transactions, ARDiscountDetails,
					Document.Current.CuryDiscTot, DiscountEngine.DiscountCalculationOptions.DisableAPDiscountsCalculation);
				RecalculateTotalDiscount();
			}
		}

		protected override void ARInvoice_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			base.ARInvoice_RowSelected(cache, e);

			if (sosetup.Current.DeferPriceDiscountRecalculation == true)
			{
				TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(this.Transactions.Cache, null, TaxCalc.ManualCalc | TaxCalc.RedefaultAlways);
			}

			PXUIFieldAttribute.SetVisible<ARTran.taskID>(Transactions.Cache, null, PM.ProjectAttribute.IsPMVisible(BatchModule.SO) || PM.ProjectAttribute.IsPMVisible(BatchModule.AR));

			selectShipment.SetEnabled(Transactions.AllowInsert);

			if (e.Row == null)
				return;

			ARInvoice doc = e.Row as ARInvoice;

			bool isCashDocument = doc != null && doc.DocType.IsIn(ARDocType.CashSale, ARDocType.CashReturn);

			cache.Graph.Actions[CommonActionCategories.ProcessingCategoryID]?.SetVisible(nameof(PayInvoice), !isCashDocument);
			cache.Graph.Actions[SOInvoiceEntry_Workflow.ActionCategories.CorrectionsCategoryID]?.SetVisible(nameof(WriteOff), !isCashDocument);

			if (isCashDocument)
			{
				PXUIFieldAttribute.SetVisible<ARInvoice.curyOrigDocAmt>(cache, e.Row);
			}

			cache.Adjust<PXUIFieldAttribute>()
				.For<ARInvoice.curyPaymentTotal>(a => a.Visible = doc.DocType.IsNotIn(ARDocType.CashReturn, ARDocType.CashSale))
				.SameFor<ARInvoice.curyUnreleasedPaymentAmt>()
				.SameFor<ARInvoice.curyPaidAmt>()
				.SameFor<ARInvoice.curyUnpaidBalance>()
				.SameFor<ARInvoice.curyBalanceWOTotal>()
				.For<ARInvoice.curyCCAuthorizedAmt>(a => a.Visible =
					(doc.DocType.IsNotIn(ARDocType.CreditMemo, ARDocType.CashReturn, ARDocType.CashSale)));

			SODocument.Cache.AllowUpdate = Document.Cache.AllowUpdate;
			FreightDetails.Cache.AllowUpdate = Document.Cache.AllowUpdate && Transactions.Cache.AllowUpdate;
			ExternalTransactionState tranState = ExternalTranHelper.GetActiveTransactionState(cache.Graph, ExternalTran);
			bool isCCCaptured = tranState.IsCaptured;
			bool isCCRefunded = tranState.IsRefunded;
			bool isCCPreAuthorized = tranState.IsPreAuthorized;
			bool isAuthorizedCashSale = doc.DocType == ARDocType.CashSale && (isCCPreAuthorized || isCCCaptured);
			bool isRefundedCashReturn = doc.DocType == ARDocType.CashReturn && isCCRefunded;
			Transactions.Cache.AllowDelete = Transactions.Cache.AllowDelete && !isAuthorizedCashSale && !isRefundedCashReturn;
			Transactions.Cache.AllowUpdate = Transactions.Cache.AllowUpdate && !isAuthorizedCashSale && !isRefundedCashReturn;
			Transactions.Cache.AllowInsert = Transactions.Cache.AllowInsert && !isAuthorizedCashSale && !isRefundedCashReturn;
			PXUIFieldAttribute.SetEnabled<ARInvoice.curyOrigDocAmt>(cache, doc, doc.Released == false && !isAuthorizedCashSale && !isRefundedCashReturn);
			PXUIFieldAttribute.SetEnabled<ARInvoice.curyOrigDiscAmt>(cache, doc, doc.Released == false && doc.DocType != ARDocType.CreditMemo && !isAuthorizedCashSale && !isRefundedCashReturn);

			Adjustments.Cache.AllowSelect = !isCashDocument;

			if (doc.DisableAutomaticTaxCalculation == true)
			{
				TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(this.Transactions.Cache, null, TaxCalc.ManualCalc);
			}
		}

		protected override void DisableCreditHoldActions(PXCache cache, ARInvoice doc)
		{
			bool enabled = doc.DocType.IsNotIn(ARDocType.CreditMemo, ARDocType.CashSale, ARDocType.CashReturn);
			putOnCreditHold.SetEnabled(enabled);
		}

		protected override bool IsWarehouseVisible(ARInvoice doc) => true;

		public static bool IsDocTypeSuitableForCC(string docType)
		{
			return (docType == ARDocType.Invoice || docType == ARDocType.CashReturn || docType == ARDocType.CashSale || docType == ARDocType.Refund) ? true : false;
		}

		public override void SetDocTypeList(PXCache cache, PXRowSelectedEventArgs e)
		{
			//doctype list should not be updated in SOInvoiceEntry
		}

		protected override void ARInvoice_CustomerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			string CreditRule = customer.Current?.CreditRule;
			try
			{
				base.ARInvoice_CustomerID_FieldUpdated(sender, e);
			}
			finally
			{
				if (customer.Current != null)
				{
					customer.Current.CreditRule = CreditRule;
				}
			}
		}

		protected virtual void SOInvoice_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;

			SOInvoice doc = (SOInvoice)e.Row;
			ARInvoice arDoc = this.Document.Current;
			doc.PaymentProjectID = PM.ProjectDefaultAttribute.NonProject();

			PXUIFieldAttribute.SetEnabled<ARInvoice.curyDiscTot>(SODocument.Cache, e.Row, Document.Cache.AllowUpdate);

			PXUIFieldAttribute.SetEnabled<SOInvoice.cashAccountID>(SODocument.Cache, e.Row, Document.Cache.AllowUpdate && (arDoc == null || arDoc.IsCancellation != true) && (((SOInvoice)e.Row).PMInstanceID != null || string.IsNullOrEmpty(doc.PaymentMethodID) == false));
			PXUIFieldAttribute.SetEnabled<SOInvoice.extRefNbr>(SODocument.Cache, e.Row, Document.Cache.AllowUpdate && (arDoc == null || arDoc.IsCancellation != true) && (((SOInvoice)e.Row).PMInstanceID != null || string.IsNullOrEmpty(doc.PaymentMethodID) == false));
			PXUIFieldAttribute.SetEnabled<SOInvoice.cleared>(SODocument.Cache, e.Row, Document.Cache.AllowUpdate && (((SOInvoice)e.Row).PMInstanceID != null || string.IsNullOrEmpty(doc.PaymentMethodID) == false) && (((SOInvoice)e.Row).DocType == ARDocType.CashSale || ((SOInvoice)e.Row).DocType == ARDocType.CashReturn));
			PXUIFieldAttribute.SetEnabled<SOInvoice.clearDate>(SODocument.Cache, e.Row, Document.Cache.AllowUpdate && (((SOInvoice)e.Row).PMInstanceID != null || string.IsNullOrEmpty(doc.PaymentMethodID) == false) && (((SOInvoice)e.Row).DocType == ARDocType.CashSale || ((SOInvoice)e.Row).DocType == ARDocType.CashReturn));

			PXUIFieldAttribute.SetVisible<SOInvoice.extRefNbr>(SODocument.Cache, e.Row, doc.DocType == ARDocType.CashSale || doc.DocType == ARDocType.CashReturn);

		}

		protected virtual void SOInvoice_PaymentMethodID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<SOInvoice.pMInstanceID>(e.Row);
			sender.SetDefaultExt<SOInvoice.cashAccountID>(e.Row);
		}

		protected virtual void SOInvoice_PMInstanceID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<SOInvoice.cashAccountID>(e.Row);
		}

		protected virtual void SOInvoice_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
			{
				SOInvoice doc = (SOInvoice)e.Row;

				if ((doc.DocType == ARDocType.CashSale || doc.DocType == ARDocType.CashReturn))
				{
					if (String.IsNullOrEmpty(doc.PaymentMethodID) == true)
					{
						if (sender.RaiseExceptionHandling<SOInvoice.pMInstanceID>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(Objects.SO.SOInvoice.pMInstanceID)}]")))
						{
							throw new PXRowPersistingException(nameof(Objects.SO.SOInvoice.pMInstanceID), null, ErrorMessages.FieldIsEmpty, nameof(Objects.SO.SOInvoice.pMInstanceID));
						}
					}
					else
					{

						CA.PaymentMethod pm = PXSelect<CA.PaymentMethod, Where<CA.PaymentMethod.paymentMethodID, Equal<Required<CA.PaymentMethod.paymentMethodID>>>>.Select(this, doc.PaymentMethodID);
						bool pmInstanceRequired = (pm.IsAccountNumberRequired == true);
						if (pmInstanceRequired && doc.PMInstanceID == null)
						{
							if (sender.RaiseExceptionHandling<SOInvoice.pMInstanceID>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(Objects.SO.SOInvoice.pMInstanceID)}]")))
							{
								throw new PXRowPersistingException(nameof(Objects.SO.SOInvoice.pMInstanceID), null, ErrorMessages.FieldIsEmpty, nameof(Objects.SO.SOInvoice.pMInstanceID));
							}
						}
					}
				}

				bool isCashSale = (doc.DocType == AR.ARDocType.CashSale) || (doc.DocType == AR.ARDocType.CashReturn);
				if (isCashSale && SODocument.GetValueExt<SOInvoice.cashAccountID>((SOInvoice)e.Row) == null)
				{
					if (sender.RaiseExceptionHandling<SOInvoice.cashAccountID>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(Objects.SO.SOInvoice.cashAccountID)}]")))
					{
						throw new PXRowPersistingException(nameof(Objects.SO.SOInvoice.cashAccountID), null, ErrorMessages.FieldIsEmpty, nameof(Objects.SO.SOInvoice.cashAccountID));
					}
				}

				object acctcd;

				if ((acctcd = SODocument.GetValueExt<SOInvoice.cashAccountID>((SOInvoice)e.Row)) != null && sender.GetValue<SOInvoice.cashAccountID>(e.Row) == null)
				{
					sender.RaiseExceptionHandling<SOInvoice.cashAccountID>(e.Row, null, null);
					sender.SetValueExt<SOInvoice.cashAccountID>(e.Row, acctcd is PXFieldState ? ((PXFieldState)acctcd).Value : acctcd);
				}
			}
		}
		private void ValidateTaxConfiguration(PXCache cache, ARInvoice cashSale)
		{
			bool reduceOnEarlyPayments = false;
			bool reduceTaxableAmount = false;
			foreach (PXResult<ARTax, Tax> result in PXSelectJoin<ARTax,
				InnerJoin<Tax, On<Tax.taxID, Equal<ARTax.taxID>>>,
				Where<ARTax.tranType, Equal<Current<ARInvoice.docType>>,
				And<ARTax.refNbr, Equal<Current<ARInvoice.refNbr>>>>>.Select(this))
			{
				Tax tax = (Tax)result;
				if (tax.TaxApplyTermsDisc == CSTaxTermsDiscount.ToPromtPayment)
				{
					reduceOnEarlyPayments = true;
				}
				if (tax.TaxApplyTermsDisc == CSTaxTermsDiscount.ToTaxableAmount)
				{
					reduceTaxableAmount = true;
				}
				if (reduceOnEarlyPayments && reduceTaxableAmount)
				{
					cache.RaiseExceptionHandling<ARInvoice.taxZoneID>(cashSale, cashSale.TaxZoneID, new PXSetPropertyException(TX.Messages.InvalidTaxConfiguration));
				}
			}
		}

		protected virtual void SOInvoice_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (!sender.ObjectsEqual<SOInvoice.pMInstanceID, SOInvoice.paymentMethodID, SOInvoice.cashAccountID>(e.Row, e.OldRow))
			{
				ARInvoice ardoc = Document.Search<ARInvoice.refNbr>(((SOInvoice)e.Row).RefNbr, ((SOInvoice)e.Row).DocType);
				//is null on delete operation
				if (ardoc != null)
				{
					ardoc.PMInstanceID = ((SOInvoice)e.Row).PMInstanceID;
					ardoc.PaymentMethodID = ((SOInvoice)e.Row).PaymentMethodID;
					ardoc.CashAccountID = ((SOInvoice)e.Row).CashAccountID;

					Document.Cache.MarkUpdated(ardoc, assertError: true);
				}
			}
		}

		protected override void ARAdjust2_CuryAdjdAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if ((decimal)e.NewValue < 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GE, ((int)0).ToString());
			}

			ARAdjust2 adj = (ARAdjust2)e.Row;
			Terms terms = PXSelect<Terms, Where<Terms.termsID, Equal<Current<ARInvoice.termsID>>>>.Select(this);

			if (terms != null && terms.InstallmentType != TermsInstallmentType.Single && (decimal)e.NewValue > 0m)
			{
				throw new PXSetPropertyException(AR.Messages.PrepaymentAppliedToMultiplyInstallments);
			}

			if (adj.CuryDocBal == null)
			{
				CalcBalancesFromInvoiceSide(adj, false, false);
			}

			if ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjdAmt - (decimal)e.NewValue < 0)
			{
				throw new PXSetPropertyException(AR.Messages.Entry_LE, ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjdAmt).ToString());
			}
		}

		protected virtual void ARAdjust2_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var row = (ARAdjust2)e.Row;

			if(e.ExternalCall && row.Recalculatable == true && !sender.ObjectsEqual<ARAdjust2.curyAdjdAmt>(e.Row, e.OldRow))
			{
				row.Recalculatable = false;
			}
		}

		protected override void ARTran_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			var row = (ARTran)e.Row;
			if (row == null) return;

			if (row.SortOrder == null)
				row.SortOrder = row.LineNbr;

			if (e.ExternalCall || forceDiscountCalculation)
				//Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers
				RecalculateDiscounts(sender, (ARTran)e.Row);
			TaxAttribute.Calculate<ARTran.taxCategoryID>(sender, e);

			if (SODocument.Current != null)
			{
				SODocument.Current.IsTaxValid = false;
				SODocument.Cache.MarkUpdated(SODocument.Current, assertError: true);
			}

			if (row.LineType == SOLineType.Inventory && (row.InvtMult ?? 0) != 0)
				UpdateCreateINDocValue(true);

			if (Document.Current != null)
			{
				Document.Current.IsTaxValid = false;
				SODocument.Cache.MarkUpdated(SODocument.Current, assertError: true);
			}
		}

		protected override void ARTran_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			//Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers
			base.ARTran_RowDeleted(sender, e);

			var row = (ARTran)e.Row;
			if (row.LineType == SOLineType.Freight)
				return;

			var invoiceDeleted = Document.Current != null && Document.Cache.GetStatus(Document.Current).IsIn(PXEntryStatus.Deleted, PXEntryStatus.InsertedDeleted);

			List<ARTran> siblings;
			if (invoiceDeleted)
				siblings = new List<ARTran>();
			else
				siblings = PXSelect<ARTran,
					Where<ARTran.sOOrderType, Equal<Required<ARTran.sOOrderType>>,
				And<ARTran.sOOrderNbr, Equal<Required<ARTran.sOOrderNbr>>,
				And<ARTran.sOShipmentType, Equal<Required<ARTran.sOShipmentType>>,
				And<ARTran.sOShipmentNbr, Equal<Required<ARTran.sOShipmentNbr>>,
				And<ARTran.tranType, Equal<Required<ARTran.tranType>>,
					And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>>>>>>>>
				.SelectWindowed(this, 0, 2, row.SOOrderType, row.SOOrderNbr, row.SOShipmentType, row.SOShipmentNbr, row.TranType, row.RefNbr)
				.RowCast<ARTran>()
				.ToList();

			if (siblings.Count == 1 && siblings[0].LineType == SOLineType.Freight)
			{
				Freight.Delete(siblings[0]);
				siblings.Clear();
			}

			SOOrderShipment ordershipment =
			 PXSelect<SOOrderShipment,
				Where<SOOrderShipment.orderType, Equal<Required<SOOrderShipment.orderType>>,
					And<SOOrderShipment.orderNbr, Equal<Required<SOOrderShipment.orderNbr>>,
					And<SOOrderShipment.shipmentType, Equal<Required<SOOrderShipment.shipmentType>>,
					And<SOOrderShipment.shipmentNbr, Equal<Required<SOOrderShipment.shipmentNbr>>,
					And<SOOrderShipment.invoiceType, Equal<Required<ARInvoice.docType>>,
					And<SOOrderShipment.invoiceNbr, Equal<Required<ARInvoice.refNbr>>>>>>>>>
				.SelectWindowed(this, 0, 1,
					row.SOOrderType, row.SOOrderNbr,
					row.SOShipmentType, row.SOShipmentNbr,
					row.TranType, row.RefNbr);

			if (siblings.Count == 0)
			{
				if (ordershipment != null)
				{
					ordershipment.HasDetailDeleted = false;
					ordershipment.IsPartialInvoiceConstraintViolated = false;
					if (Document.Current?.DisableAutomaticTaxCalculation == true && ordershipment.OrderTaxAllocated == true)
					{
						DeductTaxAmountsOfDeletedOrdersFromARTaxTranDetails(ordershipment);
					}
					ordershipment = ordershipment.UnlinkInvoice(this);

					if (!invoiceDeleted && ordershipment.CreateINDoc == true && ordershipment.InvtRefNbr == null)
						UpdateCreateINDocValue(false);

					if (!string.Equals(ordershipment.ShipmentNbr, Constants.NoShipmentNbr) && ordershipment.ShipmentNbr != null && ordershipment.ShipmentType != null)
					{
						ordershipment.OrderFreightAllocated = false;
						shipmentlist.Cache.Update(ordershipment);
					}
					else
						shipmentlist.Delete(ordershipment);

					if (!invoiceDeleted)
					{
						SOOrderShipment remainOrderShip =
							PXSelect<SOOrderShipment,
								Where<SOOrderShipment.invoiceType, Equal<Current<ARInvoice.docType>>,
									And<SOOrderShipment.invoiceNbr, Equal<Current<ARInvoice.refNbr>>>>>
								.SelectWindowed(this, 0, 1);
						if (remainOrderShip == null)
						{
							if (SODocument.Current == null)
								SODocument.Current = SODocument.Select();
							if (SODocument.Current == null)
								throw new ArgumentNullException(typeof(SOInvoice).Name);
							SODocument.Current.InitialSOBehavior = null;
							SODocument.Cache.MarkUpdated(SODocument.Current);
						}
					}
				}

				if (!invoiceDeleted)
				{
				SOFreightDetail freightDet = FreightDetails.Select().AsEnumerable()
					.RowCast<SOFreightDetail>()
					.Where(d => d.ShipmentType == row.SOShipmentType && d.ShipmentNbr == row.SOShipmentNbr && d.OrderType == row.SOOrderType && d.OrderNbr == row.SOOrderNbr)
					.FirstOrDefault();
				if (freightDet != null)
				{
					FreightDetails.Delete(freightDet);
				}

				SOOrder order = SOOrder.PK.Find(this, row.SOOrderType, row.SOOrderNbr);
				if (order != null)
				{
					Guid[] orderFileGuids = PXNoteAttribute.GetFileNotes(this.Caches[typeof(SOOrder)], order);

					foreach (NoteDoc file in PXSelect<NoteDoc, Where<NoteDoc.noteID, Equal<Required<NoteDoc.noteID>>>>.Select(this, Document.Current.NoteID))
					{
						if (orderFileGuids.Contains(file.FileID ?? Guid.Empty))
						{
							this.Caches[typeof(NoteDoc)].Delete(file);
						}
					}
				}
			}
			}
			else
			{
				if (ordershipment != null)
				{
					ordershipment.HasDetailDeleted = true;
					ordershipment.IsPartialInvoiceConstraintViolated |= row.LineType != SOLineType.MiscCharge
						|| SOOrderType.PK.Find(this, ordershipment.OrderType).RequireShipping != true;
					shipmentlist.Update(ordershipment);
				}
			}

			if (!invoiceDeleted)
			{
			if (SODocument.Current != null)
			{
				SODocument.Current.IsTaxValid = false;
				SODocument.Cache.MarkUpdated(SODocument.Current, assertError: true);
			}

			if (Document.Current != null)
			{
				Document.Current.IsTaxValid = false;
				Document.Cache.MarkUpdated(Document.Current, assertError: true);
			}

			if (row.LineType == SOLineType.Inventory && row.InvtMult != 0)
			{
				UpdateCreateINDocValue(false);
			}
		}
		}

		public virtual void DeductTaxAmountsOfDeletedOrdersFromARTaxTranDetails(SOOrderShipment ordershipment)
		{
			if (Document.Current?.DisableAutomaticTaxCalculation != true)
				return;

			foreach (PXResult<SOTaxTran, Tax> res in PXSelectJoin<SOTaxTran,
							InnerJoin<Tax, On<SOTaxTran.taxID, Equal<Tax.taxID>>>,
							Where<SOTaxTran.orderType, Equal<Required<SOTaxTran.orderType>>, And<SOTaxTran.orderNbr, Equal<Required<SOTaxTran.orderNbr>>>>>
							.Select(this, ordershipment.OrderType, ordershipment.OrderNbr))
			{
				SOTaxTran tax = (SOTaxTran)res;

				foreach (ARTaxTran existingTaxTran in Taxes.Select().Where(a =>
				((ARTaxTran)a).TaxID == tax.TaxID && ((ARTaxTran)a).JurisType == tax.JurisType && ((ARTaxTran)a).JurisName == tax.JurisName))
				{
					ARTaxTran updatedTaxTran = (ARTaxTran)this.Taxes.Cache.CreateCopy(existingTaxTran);
					updatedTaxTran.CuryTaxableAmt -= tax.CuryTaxableAmt;
					updatedTaxTran.CuryTaxAmt -= tax.CuryTaxAmt;

					if (updatedTaxTran.CuryTaxAmt < 0m)
					{
						updatedTaxTran.CuryTaxableAmt = 0m;
						updatedTaxTran.CuryTaxAmt = 0m;
					}

					if (updatedTaxTran.CuryTaxAmt >= 0)
						updatedTaxTran = this.Taxes.Update(updatedTaxTran);

					if (updatedTaxTran.CuryTaxAmt == 0m)
						this.Taxes.Delete(updatedTaxTran);
				}
			}
		}

		protected override void ARTran_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			ARTran row = (ARTran)e.Row;
			ARTran oldRow = (ARTran)e.OldRow;

			if (row != null)
			{
				if (Document.Current.DeferPriceDiscountRecalculation == true && !sender.ObjectsEqual<ARTran.taxCategoryID>(oldRow, row))
				{
					Document.Current.IsPriceAndDiscountsValid = false;
				}

				if (row.SkipLineDiscountsBuffer != null)
				{
					row.SkipLineDiscounts = row.SkipLineDiscountsBuffer;
					row.SkipLineDiscountsBuffer = null;
				}

				if ((e.ExternalCall || sender.Graph.IsImport)
					&& sender.ObjectsEqual<ARTran.inventoryID>(e.Row, e.OldRow) && sender.ObjectsEqual<ARTran.uOM>(e.Row, e.OldRow)
					&& sender.ObjectsEqual<ARTran.qty>(e.Row, e.OldRow) && sender.ObjectsEqual<ARTran.branchID>(e.Row, e.OldRow)
					&& sender.ObjectsEqual<ARTran.siteID>(e.Row, e.OldRow) && sender.ObjectsEqual<ARTran.manualPrice>(e.Row, e.OldRow)
					&& (!sender.ObjectsEqual<ARTran.curyUnitPrice>(e.Row, e.OldRow) || !sender.ObjectsEqual<ARTran.curyExtPrice>(e.Row, e.OldRow))
					&& row.ManualPrice == oldRow.ManualPrice)
				{
					row.ManualPrice = true;
					row.SkipLineDiscounts = false;
					row.SkipLineDiscountsBuffer = false;
				}

				if (!sender.ObjectsEqual<ARTran.branchID>(e.Row, e.OldRow) || !sender.ObjectsEqual<ARTran.inventoryID>(e.Row, e.OldRow) ||
					!sender.ObjectsEqual<ARTran.qty>(e.Row, e.OldRow) || !sender.ObjectsEqual<ARTran.curyUnitPrice>(e.Row, e.OldRow) || !sender.ObjectsEqual<ARTran.curyTranAmt>(e.Row, e.OldRow) ||
					!sender.ObjectsEqual<ARTran.curyExtPrice>(e.Row, e.OldRow) || !sender.ObjectsEqual<ARTran.curyDiscAmt>(e.Row, e.OldRow) ||
					!sender.ObjectsEqual<ARTran.discPct>(e.Row, e.OldRow) || !sender.ObjectsEqual<ARTran.manualDisc>(e.Row, e.OldRow) ||
					!sender.ObjectsEqual<ARTran.discountID>(e.Row, e.OldRow) || !sender.ObjectsEqual<ARTran.skipLineDiscounts>(e.Row, e.OldRow))
					//Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers
					RecalculateDiscounts(sender, row);

				if (row.ManualDisc != true)
				{
					var discountCode = (ARDiscount)PXSelectorAttribute.Select<ARTran.discountID>(sender, row);
					row.DiscPctDR = (discountCode != null && discountCode.IsAppliedToDR == true) ? row.DiscPct : 0.0m;
				}


				if (row.ManualPrice != true)
				{
					row.CuryUnitPriceDR = row.CuryUnitPrice;
				}

				bool oldCreateINDocValueForDirectStockLine = oldRow.LineType == SOLineType.Inventory && (oldRow.InvtMult ?? 0) != 0;
				bool newCreateINDocValueForDirectStockLine = row.LineType == SOLineType.Inventory && (row.InvtMult ?? 0) != 0;

				if (newCreateINDocValueForDirectStockLine != oldCreateINDocValueForDirectStockLine)
					UpdateCreateINDocValue(newCreateINDocValueForDirectStockLine);

				TaxAttribute.Calculate<ARTran.taxCategoryID>(sender, e);
			}
		}

		protected override void ARTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.ARTran_RowSelected(sender, e);

			ARTran row = e.Row as ARTran;
			if (row == null)
				return;

			bool directlyInvoicedNotMiscLine = row.InvtMult != 0 && row.LineType != SOLineType.MiscCharge;
			PXUIFieldAttribute.SetEnabled<ARTran.inventoryID>(sender, row, row.SOOrderNbr == null);
			PXUIFieldAttribute.SetEnabled<ARTran.qty>(sender, row, row.SOOrderNbr == null || directlyInvoicedNotMiscLine);
			PXUIFieldAttribute.SetEnabled<ARTran.uOM>(sender, row, row.SOOrderNbr == null || directlyInvoicedNotMiscLine);
			PXUIFieldAttribute.SetEnabled<ARTran.skipLineDiscounts>(sender, e.Row, IsCopyPasteContext);
		}

		protected override void ARTran_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = (ARTran)e.Row;
			if (row.SOShipmentNbr == null)
			{
				sender.SetDefaultExt<ARTran.invtMult>(e.Row);
			}

			base.ARTran_InventoryID_FieldUpdated(sender, e);
		}

		protected virtual void ARTran_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			ARTran row = (ARTran)e.Row;
			if (row == null) return;

			e.NewValue = (row.SOShipmentNbr != null || row.LineType == SOLineType.Discount)
				? (short)0
				: INTranType.InvtMultFromInvoiceType(row.TranType);
		}

		protected override void ARTran_UOM_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<ARTran.curyUnitPrice>(e.Row);
		}

		protected virtual void ARTran_SiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARTran tran = (ARTran)e.Row;

			sender.SetDefaultExt<ARTran.curyUnitPrice>(tran);

			if (tran.InventoryID != null && tran.IsStockItem != true && tran.AccrueCost == true)
			{
				//Expense Accrual Account
				sender.SetDefaultExt<ARTran.expenseAccrualAccountID>(e.Row);
				try
				{
					sender.SetDefaultExt<ARTran.expenseAccrualSubID>(e.Row);
				}
				catch (PXSetPropertyException)
				{
					sender.SetValue<ARTran.expenseAccrualSubID>(e.Row, null);
				}

				//Expense Account
				sender.SetDefaultExt<ARTran.expenseAccountID>(e.Row);
				try
				{
					sender.SetDefaultExt<ARTran.expenseSubID>(e.Row);
				}
				catch (PXSetPropertyException)
				{
					sender.SetValue<ARTran.expenseSubID>(e.Row, null);
				}
			}
			else
			{
				tran.ExpenseAccrualAccountID = null;
				tran.ExpenseAccrualSubID = null;
				tran.ExpenseAccountID = null;
				tran.ExpenseSubID = null;
			}
		}

		protected override void ARTran_Qty_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			base.ARTran_Qty_FieldUpdated(sender, e);
			ARTran row = e.Row as ARTran;
			if (row != null)
			{
				sender.SetDefaultExt<ARTran.tranDate>(row);
				sender.SetValueExt<ARTran.manualDisc>(row, false);
				sender.SetDefaultExt<ARTran.curyUnitPrice>(row);
			}
		}

		protected override void ARTran_TaxCategoryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && string.IsNullOrEmpty(((ARTran)e.Row).SOOrderNbr) == false)
			{
				//tax category is taken from invoice lines
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void ARTran_SalesPersonID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && string.IsNullOrEmpty(((ARTran)e.Row).SOOrderNbr) == false)
			{
				//salesperson is taken from invoice lines
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected override void ARTran_AccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && string.IsNullOrEmpty(((ARTran)e.Row).SOOrderType) == false)
			{
                ARTran tran = (ARTran)e.Row;

                if (tran != null)
                {
					InventoryItem item = IN.InventoryItem.PK.Find(this, tran.InventoryID);
					if (item == null)
                        return;

					SOOrderType ordertype = SOOrderType.PK.Find(this, tran.SOOrderType);

					switch (ordertype.SalesAcctDefault)
                    {
                        case SOSalesAcctSubDefault.MaskItem:
                            e.NewValue = GetValue<InventoryItem.salesAcctID>(item);
                            e.Cancel = true;
                            break;
                        case SOSalesAcctSubDefault.MaskSite:
							INSite site = INSite.PK.Find(this, tran.SiteID);
							e.NewValue = GetValue<INSite.salesAcctID>(site);
                            e.Cancel = true;
                            break;
                        case SOSalesAcctSubDefault.MaskClass:
							INPostClass postclass = INPostClass.PK.Find(this, item.PostClassID) ?? new INPostClass();
							e.NewValue = GetValue<INPostClass.salesAcctID>(postclass);
                            e.Cancel = true;
                            break;
                        case SOSalesAcctSubDefault.MaskLocation:
							Location customerloc = location.Current;
							e.NewValue = GetValue<Location.cSalesAcctID>(customerloc);
                            e.Cancel = true;
                            break;
                        case SOSalesAcctSubDefault.MaskReasonCode:
							ReasonCode reasoncode = ReasonCode.PK.Find(this, tran.ReasonCode);
							e.NewValue = GetValue<ReasonCode.salesAcctID>(reasoncode);
                            e.Cancel = true;
                            break;
                    }
                }
			}
			else
			{
				base.ARTran_AccountID_FieldDefaulting(sender, e);
			}
		}

		protected override void ARTran_SubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && string.IsNullOrEmpty(((ARTran)e.Row).SOOrderType) == false)
			{
				ARTran tran = (ARTran)e.Row;

				if (tran != null && tran.AccountID != null)
				{
					InventoryItem item = IN.InventoryItem.PK.Find(this, tran.InventoryID);
					INSite site = INSite.PK.Find(this, tran.SiteID);
					ReasonCode reasoncode = ReasonCode.PK.Find(this, tran.ReasonCode);
					SOOrderType ordertype = SOOrderType.PK.Find(this, tran.SOOrderType);
					SalesPerson salesperson = (SalesPerson)PXSelectorAttribute.Select<ARTran.salesPersonID>(sender, e.Row);
					INPostClass postclass = INPostClass.PK.Find(this, item?.PostClassID) ?? new INPostClass();

					EPEmployee employee = (EPEmployee)PXSelectJoin<EPEmployee, InnerJoin<SOOrder, On<EPEmployee.defContactID, Equal<SOOrder.ownerID>>>, Where<SOOrder.orderType, Equal<Required<ARTran.sOOrderType>>, And<SOOrder.orderNbr, Equal<Required<ARTran.sOOrderNbr>>>>>.Select(this, tran.SOOrderType, tran.SOOrderNbr);
					CRLocation companyloc =
						PXSelectJoin<CRLocation, InnerJoin<BAccountR, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>, And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>, InnerJoin<Branch, On<BAccountR.bAccountID, Equal<Branch.bAccountID>>>>, Where<Branch.branchID, Equal<Required<ARTran.branchID>>>>.Select(this, tran.BranchID);
					Location customerloc = location.Current;

					object item_SubID = GetValue<InventoryItem.salesSubID>(item);
					object site_SubID = GetValue<INSite.salesSubID>(site);
					object postclass_SubID = GetValue<INPostClass.salesSubID>(postclass);
					object customer_SubID = GetValue<Location.cSalesSubID>(customerloc);
					object employee_SubID = GetValue<EPEmployee.salesSubID>(employee);
					object company_SubID = GetValue<CRLocation.cMPSalesSubID>(companyloc);
					object salesperson_SubID = GetValue<SalesPerson.salesSubID>(salesperson);
					object reasoncode_SubID = GetValue<ReasonCode.salesSubID>(reasoncode);

					object value = null;

					try
					{
						value = SOSalesSubAccountMaskAttribute.MakeSub<SOOrderType.salesSubMask>(this, ordertype.SalesSubMask,
																								 new object[]
																							 {
																								 item_SubID,
																								 site_SubID,
																								 postclass_SubID,
																								 customer_SubID,
																								 employee_SubID,
																								 company_SubID,
																								 salesperson_SubID,
																								 reasoncode_SubID
																							 },
																								 new Type[]
																							 {
																								 typeof(InventoryItem.salesSubID),
																								 typeof(INSite.salesSubID),
																								 typeof(INPostClass.salesSubID),
																								 typeof(Location.cSalesSubID),
																								 typeof(EPEmployee.salesSubID),
																								 typeof(Location.cMPSalesSubID),
																								 typeof(SalesPerson.salesSubID),
																								 typeof(ReasonCode.subID)
																							 });

						sender.RaiseFieldUpdating<ARTran.subID>(tran, ref value);
					}
					catch (PXMaskArgumentException ex)
					{
						sender.RaiseExceptionHandling<ARTran.subID>(e.Row, null, new PXSetPropertyException(ex.Message));
						value = null;
					}
					catch (PXSetPropertyException ex)
					{
						sender.RaiseExceptionHandling<ARTran.subID>(e.Row, value, ex);
						value = null;
					}

					e.NewValue = (int?)value;
					e.Cancel = true;
				}
			}
			else
			{
				base.ARTran_SubID_FieldDefaulting(sender, e);
			}
		}

		protected override void ARInvoiceDiscountDetail_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.ARInvoiceDiscountDetail_RowSelected(sender, e);

			ARInvoiceDiscountDetail discountDetail = (ARInvoiceDiscountDetail)e.Row;

			if (discountDetail != null && discountDetail.OrderNbr == null && discountDetail.DiscountID != null)
			{
				bool hasSODiscounts = false;
				foreach (ARTran tran in Transactions.Cache.Cached)
				{
					if (Transactions.Cache.GetStatus(tran).IsNotIn(PXEntryStatus.Deleted, PXEntryStatus.InsertedDeleted) &&
						(tran.OrigGroupDiscountRate != 1 || tran.OrigDocumentDiscountRate != 1))
					{
						hasSODiscounts = true;
						break;
					}
				}

				if (hasSODiscounts)
			{
				sender.RaiseExceptionHandling<ARInvoiceDiscountDetail.discountID>(discountDetail, discountDetail.DiscountID,
					new PXSetPropertyException(Messages.AutomaticDiscountInSOInvoice, PXErrorLevel.Warning));
			}
		}
		}

		protected virtual void SOFreightDetail_AccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOFreightDetail row = e.Row as SOFreightDetail;
			if (row != null && row.TaskID == null)
			{
				sender.SetDefaultExt<SOFreightDetail.taskID>(e.Row);
			}
		}

		protected virtual void SOFreightDetail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var row = (SOFreightDetail)e.Row;
			if (row != null)
			{
				UpdateFreightTransaction(row, false);
			}
		}

		protected virtual void SOFreightDetail_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			var row = (SOFreightDetail)e.Row;
			if (row != null)
			{
				UpdateFreightTransaction(row, true);
			}
		}

		public override int ExecuteInsert(string viewName, IDictionary values, params object[] parameters)
		{
			switch (viewName)
			{
				case "Freight":
					values[PXDataUtils.FieldName<ARTran.lineType>()] = SOLineType.Freight;
					break;
				case "Discount":
					values[PXDataUtils.FieldName<ARTran.lineType>()] = SOLineType.Discount;
					break;
			}
			return base.ExecuteInsert(viewName, values, parameters);
		}

		public virtual IEnumerable sHipmentlist()
		{
			PXSelectBase<ARTran> cmd = new PXSelect<ARTran,
				Where<ARTran.sOShipmentNbr, Equal<Current<SOOrderShipment.shipmentNbr>>,
				And<ARTran.sOShipmentType, Equal<Current<SOOrderShipment.shipmentType>>,
				And<ARTran.sOOrderType, Equal<Current<SOOrderShipment.orderType>>,
				And<ARTran.sOOrderNbr, Equal<Current<SOOrderShipment.orderNbr>>,
				And<ARTran.sOOrderLineNbr, IsNotNull,
				And<ARTran.canceled, NotEqual<True>,
				And<ARTran.isCancellation, NotEqual<True>>>>>>>>>(this);

			var list = new InvoiceList(this);
			CurrencyInfo info = currencyinfo.Select();
			list.Add(Document.Current, SODocument.Select(), info);

			bool newInvoice = (Transactions.SelectSingle() == null);
			bool curyRateNotDefined = (info.CuryRate ?? 0m) == 0m;
			var invoiceSearchValues = new List<FieldLookup>();

			HashSet<SOOrderShipment> selectedShipments = new HashSet<SOOrderShipment>(shipmentlist.Cache.GetComparer());

			foreach (SOOrderShipment shipment in shipmentlist.Cache.Updated)
			{
				selectedShipments.Add(shipment);
			}

			foreach (PXResult<SOOrderShipment, SOOrder, SOOrderType> order in
				PXSelectJoin<SOOrderShipment,
					InnerJoin<SOOrder, On<SOOrderShipment.FK.Order>,
					InnerJoin<SOOrderType, On<SOOrderShipment.FK.OrderType>,
					InnerJoin<SOShipment, On<SOShipment.shipmentNbr, Equal<SOOrderShipment.shipmentNbr>,
						And<SOShipment.shipmentType, Equal<SOOrderShipment.shipmentType>>>>>>,
					Where<SOOrderShipment.customerID, Equal<Current<ARInvoice.customerID>>,
						And<SOOrderShipment.hold, Equal<boolFalse>,
						And<SOOrderShipment.confirmed, Equal<boolTrue>,
						And<SOOrderType.aRDocType, Equal<Current<ARInvoice.docType>>,
						And<Where<SOOrderShipment.invoiceNbr, IsNull,
							Or<SOOrderShipment.invoiceNbr, Equal<Current<ARInvoice.refNbr>>>>>>>>>>
				.Select(this).AsEnumerable()
				.Concat(
				PXSelectJoin<SOOrderShipment,
					InnerJoin<SOOrder, On<SOOrderShipment.FK.Order>,
					InnerJoin<SOOrderType, On<SOOrderShipment.FK.OrderType>,
					InnerJoin<POReceipt, On<POReceipt.receiptNbr, Equal<SOOrderShipment.shipmentNbr>,
						And<POReceipt.receiptType, Equal<POReceiptType.poreceipt>>>>>>,
					Where<SOOrderShipment.shipmentType, Equal<SOShipmentType.dropShip>,
						And<SOOrderShipment.customerID, Equal<Current<ARInvoice.customerID>>,
						And<SOOrderType.aRDocType, Equal<Current<ARInvoice.docType>>,
						And<Where<SOOrderShipment.invoiceNbr, IsNull,
							Or<SOOrderShipment.invoiceNbr, Equal<Current<ARInvoice.refNbr>>>>>>>>>
				.Select(this)))
			{
				SOOrderShipment soOrderShipment = order;
				if (cmd.View.SelectSingleBound(new object[] { soOrderShipment }) != null)
					continue;

				SOOrder soOrder = order;
				SOOrderType soOrderType = order;
				bool copyCuryInfoFromSO = (curyRateNotDefined || newInvoice && soOrderType.UseCuryRateFromSO == true);

				if (!newInvoice)
				{
					invoiceSearchValues.Add(new FieldLookup<ARInvoice.customerID>(soOrder.CustomerID));
					invoiceSearchValues.Add(new FieldLookup<SOInvoice.billAddressID>(soOrder.BillAddressID));
					invoiceSearchValues.Add(new FieldLookup<SOInvoice.billContactID>(soOrder.BillContactID));
					invoiceSearchValues.Add(new FieldLookup<ARInvoice.termsID>(soOrder.TermsID));
					if (Document.Current?.DisableAutomaticDiscountCalculation != null)
						invoiceSearchValues.Add(new FieldLookup<ARInvoice.disableAutomaticTaxCalculation>(soOrder.DisableAutomaticTaxCalculation));
					invoiceSearchValues.Add(new FieldLookup<ARInvoice.hidden>(false));
				}
				if (!copyCuryInfoFromSO)
				{
					invoiceSearchValues.Add(new FieldLookup<ARInvoice.curyID>(soOrder.CuryID));
				}

				if (list.Find(invoiceSearchValues.ToArray()) != null)
				{
					selectedShipments.Remove(soOrderShipment);
					yield return soOrderShipment;
				}

				invoiceSearchValues.Clear();
			}

			foreach (var notListed in selectedShipments)
			{
				notListed.Selected = false;
			}
		}

		protected virtual void SOOrderShipment_ShipmentNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void UpdateCreateINDocValue(bool presumptiveNewValue)
		{
			if (SODocument.Current == null)
				SODocument.Current = SODocument.Select();

			if (SODocument.Current == null)
				throw new ArgumentNullException(typeof(SOInvoice).Name);

			if (SODocument.Current.CreateINDoc == presumptiveNewValue)
				return;

			if (presumptiveNewValue == false)
			{
				bool stockShipmentExists =
					PXSelect<SOOrderShipment,
					Where<SOOrderShipment.invoiceType, Equal<Current<ARInvoice.docType>>,
						And<SOOrderShipment.invoiceNbr, Equal<Current<ARInvoice.refNbr>>,
						And<SOOrderShipment.createINDoc, Equal<boolTrue>,
						And<SOOrderShipment.invtRefNbr, IsNull>>>>>
					.SelectWindowed(this, 0, 2).Count > 0;

				if (stockShipmentExists) return;

				bool directStockTranExists =
					PXSelect<ARTran,
					Where<ARTran.tranType, Equal<Current<ARInvoice.docType>>,
						And<ARTran.refNbr, Equal<Current<ARInvoice.refNbr>>,
						And<ARTran.lineType, Equal<SOLineType.inventory>,
						And<ARTran.invtMult, NotEqual<short0>>>>>>
					.SelectWindowed(this, 0, 1).Count > 0;

				if (directStockTranExists) return;
			}

			SODocument.Current.CreateINDoc = presumptiveNewValue;
			if (SODocument.Cache.GetStatus(SODocument.Current) == PXEntryStatus.Notchanged)
			{
				SODocument.Cache.MarkUpdated(SODocument.Current, assertError: true);
			}
		}

		protected virtual void SOOrderShipment_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			SOOrderShipment row = e.Row as SOOrderShipment;
			if (row != null && string.Equals(row.ShipmentNbr, Constants.NoShipmentNbr))
			{
				SOOrder cached = soorder.Locate(new SOOrder { OrderType = row.OrderType, OrderNbr = row.OrderNbr });
				if (cached != null)
				{
					cached.ShipmentCntr--;
					soorder.Update(cached);

					SOOrderShipment.Events
						.Select(ev => ev.ShipmentUnlinked)
						.FireOn(this, row, new SOShipment { ShipmentNbr = Constants.NoShipmentNbr });
				}
			}
		}

		protected virtual void SOOrderShipment_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXUIFieldAttribute.SetEnabled(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled<SOOrderShipment.selected>(sender, e.Row, true);
		}

		protected virtual void SOOrder_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			SOOrder doc = (SOOrder)e.Row;
			if (e.Operation == PXDBOperation.Update)
			{
				if (doc.ShipmentCntr < 0 || doc.OpenShipmentCntr < 0 || doc.ShipmentCntr < doc.BilledCntr + doc.ReleasedCntr && doc.Behavior == SOBehavior.SO)
				{
					throw new Exceptions.InvalidShipmentCountersException();
				}
			}
		}

		protected virtual void SOOrder_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			SOOrder row = e.Row as SOOrder;
			SOOrder oldRow = e.OldRow as SOOrder;
			if (row != null && oldRow != null && row.UnbilledOrderQty != oldRow.UnbilledOrderQty)
			{
				row.IsUnbilledTaxValid = false;
			}

			if (e.OldRow != null)
			{
				ARReleaseProcess.UpdateARBalances(this, (SOOrder)e.OldRow, -((SOOrder)e.OldRow).UnbilledOrderTotal, -((SOOrder)e.Row).OpenOrderTotal);
			}
			ARReleaseProcess.UpdateARBalances(this, (SOOrder)e.Row, ((SOOrder)e.Row).UnbilledOrderTotal, ((SOOrder)e.Row).OpenOrderTotal);
		}

		public bool TransferApplicationFromSalesOrder;
		public override IEnumerable adjustments()
		{
			Adjustments_Inv.View.Clear();
			int applcount = 0;
			foreach (PXResult<ARAdjust2, ARPayment, AR.Standalone.ARRegisterAlias, CurrencyInfo, ExternalTransaction> res
				in Adjustments_Inv.Select())
			{
				ARPayment payment = PXCache<ARPayment>.CreateCopy(res);
				ARAdjust2 adj = res;
				CurrencyInfo pay_info = res;

				PXCache<ARRegister>.RestoreCopy(payment, (AR.Standalone.ARRegisterAlias)res);
				ARPayment originalPayment = PXCache<ARPayment>.CreateCopy(payment);

				if (adj == null) continue;

				if (adj.CuryDocBal == null)
				{
					CalcBalancesFromInvoiceSide(adj, payment, true, true);
				}

				yield return new PXResult<ARAdjust2, ARPayment, ExternalTransaction>(adj, originalPayment, res);
				applcount++;
			}

			//fix unattended grid load in InvoiceOrder when CurrencyRate is set
			if (this.UnattendedMode && !TransferApplicationFromSalesOrder)
			{
				yield break;
			}

			if (this.IsContractBasedAPI || this.UnattendedMode || SOInvoiceAddOrderPaymentsScope.IsActive)
			{
				foreach (PXResult<ARAdjust2, ARPayment> res in LoadDocumentsProc(applcount))
				{
					yield return res;
				}
			}
		}

		public override IEnumerable adjustments_1()
		{
			foreach (var res in base.adjustments_1())
			{
				yield return res;
			}

			if (TransferApplicationFromSalesOrder && Document.Current?.DocType == ARDocType.CreditMemo)
			{
				using (new ReadOnlyScope(Adjustments.Cache, Adjustments_1.Cache, Document.Cache, arbalances.Cache, SODocument.Cache, PaymentTotalsUpd.Cache))
				{
					foreach (PXResult<ARPayment, CurrencyInfo, ExternalTransaction> res in CollectPaymentsToApply(applcount: 0))
					{
						var adj = CreateApplicationFromPayment(Adjustments_1.Cache, res);
						if (adj != null)
							yield return new PXResult<ARAdjust, ARPayment, ExternalTransaction>(adj, res, res);
					}
				}
			}
		}

		/// <summary>
		/// The method to calculate application
		/// balances in Invoice currency. Only
		/// payment document should be set.
		/// </summary>
		protected override void CalcBalancesFromInvoiceSide(
			ARAdjust2 adj,
			ARPayment payment,
			bool isCalcRGOL,
			bool DiscOnDiscDate)
		{
			ARInvoice invoice = ARInvoice_CustomerID_DocType_RefNbr
				.Select(adj.AdjdCustomerID, adj.AdjdDocType, adj.AdjdRefNbr);
			ARAdjust2 others = GetOtherAppsOfPaymentGrouped(adj);
			new ARInvoiceBalanceCalculator(GetExtension<MultiCurrency>(), this)
				.CalcBalancesFromInvoiceSide(adj, invoice, payment, isCalcRGOL, DiscOnDiscDate, others);
		}

		protected virtual ARAdjust2 GetOtherAppsOfPaymentGrouped(ARAdjust2 adj)
		{
			ARAdjust2 other = PXSelectGroupBy<ARAdjust2,
				Where<ARAdjust2.adjgDocType, Equal<Required<ARAdjust2.adjgDocType>>,
					And<ARAdjust2.adjgRefNbr, Equal<Required<ARAdjust2.adjgRefNbr>>,
					And<ARAdjust2.released, Equal<False>,
					And<Where<ARAdjust2.adjdDocType, NotEqual<Required<ARAdjust2.adjdDocType>>,
						Or<ARAdjust2.adjdRefNbr, NotEqual<Required<ARAdjust2.adjdRefNbr>>>>>>>>,
				Aggregate<GroupBy<ARAdjust2.adjgDocType, GroupBy<ARAdjust2.adjgRefNbr,
					Sum<ARAdjust2.curyAdjgSignedAmt, Sum<ARAdjust2.adjSignedAmt>>>>>>
				.Select(this, adj.AdjgDocType, adj.AdjgRefNbr, adj.AdjdDocType, adj.AdjdRefNbr);

			if (other != null)
			{
				var mult = ARDocType.SignBalance(adj.AdjdDocType);
				other.CuryAdjgAmt = other.CuryAdjgSignedAmt * mult;
				other.AdjAmt = other.AdjSignedAmt * mult;
			}

			return other;
		}

		public override PXResultset<ARAdjust2, ARPayment, ExternalTransaction> LoadDocumentsProc()
		{
			var orderShipments = SelectFrom<SOOrderShipment>
				.Where<SOOrderShipment.invoiceType.IsEqual<ARInvoice.docType.FromCurrent>
					.And<SOOrderShipment.invoiceNbr.IsEqual<ARInvoice.refNbr.FromCurrent>>
					.And<Exists<
						SelectFrom<SOAdjust>
							.Where<SOAdjust.adjdOrderType.IsEqual<SOOrderShipment.orderType>
								.And<SOAdjust.adjdOrderNbr.IsEqual<SOOrderShipment.orderNbr>>>>>>.View.Select(this);

			using (new SOInvoiceAddOrderPaymentsScope())
			{
				foreach (var orderShipment in orderShipments)
					InsertApplications(orderShipment);
			}

			return LoadDocumentsProc(0);
		}

		public virtual void NonTransferApplicationQuery(PXSelectBase<ARPayment> cmd)
        {
			cmd.Join<LeftJoin<ARAdjust2,
							On<ARAdjust2.adjgDocType, Equal<ARPayment.docType>,
								And<ARAdjust2.adjgRefNbr, Equal<ARPayment.refNbr>,
								And<ARAdjust2.adjNbr, Equal<ARPayment.adjCntr>,
								And<ARAdjust2.released, Equal<False>,
								And<ARAdjust2.hold, Equal<True>,
								And<ARAdjust2.voided, Equal<False>,
								And<
									Where<ARAdjust2.adjdDocType, NotEqual<Current<ARInvoice.docType>>,
										Or<ARAdjust2.adjdRefNbr, NotEqual<Current<ARInvoice.refNbr>>>
									>>>>>>>>>>();
			cmd.Join<LeftJoin<SOAdjust,
			On<SOAdjust.adjgDocType, Equal<ARPayment.docType>,
				And<SOAdjust.adjgRefNbr, Equal<ARPayment.refNbr>,
				And<SOAdjust.adjAmt, Greater<decimal0>>>>>>();

			cmd.WhereAnd<Where<ARPayment.finPeriodID, LessEqual<Current<ARInvoice.finPeriodID>>,
				And<ARPayment.released, Equal<True>,
				And<ARAdjust2.adjdRefNbr, IsNull,
				And<SOAdjust.adjgRefNbr, IsNull>>>>>();
		}

		public virtual PXResultset<ARAdjust2, ARPayment, ExternalTransaction> LoadDocumentsProc(int applcount)
		{
			var adjs = new PXResultset<ARAdjust2, ARPayment, ExternalTransaction>();

			if (Document.Current?.Released == false
				&& Document.Current.DocType.IsIn(ARDocType.Invoice, ARDocType.DebitMemo))
			{
				using (new ReadOnlyScope(Adjustments.Cache, Adjustments_1.Cache, Document.Cache, arbalances.Cache, SODocument.Cache, PaymentTotalsUpd.Cache))
				{
					foreach (PXResult<ARPayment, CurrencyInfo, ExternalTransaction> res in CollectPaymentsToApply(applcount))
					{
						var adj = (ARAdjust2)CreateApplicationFromPayment(Adjustments.Cache, res);
						if (adj != null)
							adjs.Add(new PXResult<ARAdjust2, ARPayment, ExternalTransaction>(adj, res, res));
					}
				}
			}
			return adjs;
		}

		protected virtual List<PXResult<ARPayment, CurrencyInfo, ExternalTransaction>> CollectPaymentsToApply(int applcount)
		{
			var list = new List<PXResult<ARPayment, CurrencyInfo, ExternalTransaction>>();
			//same as ARInvoiceEntry but without released constraint and with hold constraint
			PXSelectBase<ARPayment> cmd = new PXSelectReadonly2<ARPayment,
				InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARPayment.curyInfoID>>,
				LeftJoin<ExternalTransaction,
					On<ExternalTransaction.transactionID, Equal<ARPayment.cCActualExternalTransactionID>>>>,
				Where<ARPayment.customerID, In3<Current<ARInvoice.customerID>, Current<Customer.consolidatingBAccountID>>,
					And<ARPayment.openDoc, Equal<True>>
				>,
				OrderBy<
					Asc<ARPayment.docType,
					Asc<ARPayment.refNbr>>>
				>(this);

			if (Document.Current?.DocType == ARDocType.CreditMemo)
			{
				cmd.WhereAnd<Where<ARPayment.docType, Equal<ARDocType.refund>>>();
			}
			else
			{
				cmd.WhereAnd<Where<ARPayment.docType, In3<ARDocType.payment, ARDocType.prepayment, ARDocType.creditMemo, ARDocType.prepaymentInvoice>>>();
			}

			//this delegate is invoked in processing to transfer applications from sales order
			//date and period constraints are not valid in this case
			if (!TransferApplicationFromSalesOrder)
			{
				NonTransferApplicationQuery(cmd);

				int remaining = Constants.MaxNumberOfPaymentsAndMemos - applcount;
				if (remaining > 0)
				{
					foreach (PXResult<AR.ARPayment, CurrencyInfo, ExternalTransaction> res
						in cmd.SelectWindowed(0, remaining))
					{
						list.Add(res);
					}
				}
			}
			else
			{
				cmd.Join<InnerJoin<SOAdjust,
				On<SOAdjust.adjgDocType, Equal<ARPayment.docType>,
					And<SOAdjust.adjgRefNbr, Equal<ARPayment.refNbr>>>>>();
				cmd.WhereAnd<Where<SOAdjust.adjdOrderType, Equal<Required<SOAdjust.adjdOrderType>>,
					And<SOAdjust.adjdOrderNbr, Equal<Required<SOAdjust.adjdOrderNbr>>>>>();

				HashSet<string> orderProcessed = new HashSet<string>();

				foreach (ARTran tran in Transactions.Select())
				{
					if (!string.IsNullOrEmpty(tran.SOOrderType) && !string.IsNullOrEmpty(tran.SOOrderNbr))
					{
						string key = string.Format("{0}.{1}", tran.SOOrderType, tran.SOOrderNbr);

						if (!orderProcessed.Contains(key))
						{
							orderProcessed.Add(key);
							foreach (PXResult<ARPayment, CurrencyInfo, ExternalTransaction, SOAdjust> res
								in cmd.Select(tran.SOOrderType, tran.SOOrderNbr))
							{
								SOAdjust soadjust = res;
								SOAdjust cachedSOAdjust = soadjustments.Locate(soadjust);

								if (cachedSOAdjust != null && soadjustments.Cache.GetStatus(cachedSOAdjust)
									.IsNotIn(PXEntryStatus.Deleted, PXEntryStatus.InsertedDeleted))
								{
									soadjust = cachedSOAdjust; 
								}
								else if (cachedSOAdjust != null)
								{
									soadjust = null;
								}

								if (soadjust?.AdjAmt > 0m)
									list.Add(res);
							}
						}
					}
				}
			}
			return list;
		}

		protected virtual ARAdjust CreateApplicationFromPayment(PXCache adjCache, ARPayment payment)
		{
			// this method works both for Invoices (ARAdjust2) and Credit Memos (ARAdjust)
			bool isCurrentCreditMemo = Document.Current?.DocType == ARDocType.CreditMemo;
			bool isPaymentCreditMemo = payment.DocType == ARDocType.CreditMemo;
			var adj = (ARAdjust)adjCache.CreateInstance();
			adj.CustomerID = payment.CustomerID;
			adj.AdjdCustomerID = Document.Current.CustomerID;
			adj.AdjdDocType = Document.Current.DocType;
			adj.AdjdRefNbr = Document.Current.RefNbr;
			adj.AdjdLineNbr = 0;
			adj.AdjdBranchID = Document.Current.BranchID;
						adj.AdjdFinPeriodID = Document.Current.FinPeriodID;
			adj.AdjgDocType = payment.DocType;
			adj.AdjgRefNbr = payment.RefNbr;
			adj.AdjgBranchID = payment.BranchID;
						adj.AdjgFinPeriodID = payment.FinPeriodID;
			adj.AdjNbr = payment.AdjCntr;
			adj.InvoiceID = isCurrentCreditMemo ? null : Document.Current.NoteID;
			adj.PaymentID = isCurrentCreditMemo || !isPaymentCreditMemo ? payment.NoteID : null;
			adj.MemoID = isCurrentCreditMemo ? Document.Current.NoteID : (isPaymentCreditMemo ? payment.NoteID : null);
			adj.Recalculatable = Document.Current.IsTaxValid != true && IsExternalTax(Document.Current.TaxZoneID);

			var adjFromCache = (ARAdjust)adjCache.Locate(adj);
			if (adjFromCache == null || adjCache.GetStatus(adjFromCache).IsIn(PXEntryStatus.InsertedDeleted, PXEntryStatus.Deleted))
			{
				adj.AdjgCuryInfoID = payment.CuryInfoID;
				adj.AdjdOrigCuryInfoID = Document.Current.CuryInfoID;
				//if LE constraint is removed from payment selection this must be reconsidered
				adj.AdjdCuryInfoID = Document.Current.CuryInfoID;
							PXSelectorAttribute.StoreCached<ARAdjust2.adjgRefNbr>(Adjustments.Cache, adj,
								payment);

				CalcBalancesFromInvoiceSide(adj, payment, false, false);

				try
				{
					return (ARAdjust)adjCache.Insert(adj);
				}
				catch (PXException) when (isCurrentCreditMemo)
				{
					ARAdjust unreleasedApplication = PXSelectReadonly<ARAdjust,
						Where<ARAdjust.adjgDocType, Equal<Current<ARPayment.docType>>,
							And<ARAdjust.adjgRefNbr, Equal<Current<ARPayment.refNbr>>,
							And<ARAdjust.released, NotEqual<True>,
							And<ARAdjust.voided, NotEqual<True>,
							And<Where<ARAdjust.adjdDocType, NotEqual<Current<ARInvoice.docType>>,
							Or<ARAdjust.adjdRefNbr, NotEqual<Current<ARInvoice.refNbr>>>>>>>>>>
						.SelectSingleBound(this, new object[] { payment, Document.Current });
					if (unreleasedApplication != null)
					{
						throw new PXException(Messages.RefundUnreleasedApplicationToCMExists,
							payment.RefNbr, unreleasedApplication.AdjdRefNbr);
					}
					throw;
				}
			}
			else
			{
				return null;
			}
		}

		public delegate void InvoiceCreatedDelegate(ARInvoice invoice, InvoiceOrderArgs args);
		protected virtual void InvoiceCreated(ARInvoice invoice, InvoiceOrderArgs args)
		{

		}

		public virtual string GetInvoiceDocType(SOOrderType soOrderType, SOOrder order, string shipmentOperation)
		{
			switch (soOrderType.ARDocType)
			{
				case ARDocType.InvoiceOrCreditMemo:
					return order.CuryOrderTotal >= 0m && order.DefaultOperation == SOOperation.Issue
						|| order.CuryOrderTotal < 0m && order.DefaultOperation == SOOperation.Receipt
						? ARDocType.Invoice : ARDocType.CreditMemo;
				case ARDocType.CashSaleOrReturn:
					return order.CuryOrderTotal >= 0m && order.DefaultOperation == SOOperation.Issue
						|| order.CuryOrderTotal < 0m && order.DefaultOperation == SOOperation.Receipt
						? ARDocType.CashSale : ARDocType.CashReturn;
				default:
					if (ARInvoiceType.DrCr(soOrderType.ARDocType) == DrCr.Credit && shipmentOperation == SOOperation.Receipt
						|| ARInvoiceType.DrCr(soOrderType.ARDocType) == DrCr.Debit && shipmentOperation == SOOperation.Issue)
					{
						//for RMA switch document type if previous shipment was not invoiced previously in the current run, i.e. list.Find() returned null
						return
							soOrderType.ARDocType.IsIn(ARDocType.Invoice, ARDocType.DebitMemo) ? ARDocType.CreditMemo :
							soOrderType.ARDocType == ARDocType.CreditMemo ? ARDocType.Invoice :
							soOrderType.ARDocType == ARDocType.CashSale ? ARDocType.CashReturn :
							soOrderType.ARDocType == ARDocType.CashReturn ? ARDocType.CashSale :
							null;
					}
					else return soOrderType.ARDocType;
			}
		}

		public virtual bool HasOrderShipmentTransactions(SOOrderShipment orderShipment) => this.Transactions.Search<ARTran.sOOrderType, ARTran.sOOrderNbr, ARTran.sOShipmentType, ARTran.sOShipmentNbr>(
						orderShipment.OrderType, orderShipment.OrderNbr, orderShipment.ShipmentType, orderShipment.ShipmentNbr).Count > 0;

		public virtual PXResultset<SOShipLine> SelectLinesToInvoice(SOOrderShipment orderShipment) =>
			 PXSelectJoin<SOShipLine,
					InnerJoin<SOLine, On<SOLine.orderType, Equal<SOShipLine.origOrderType>,
						And<SOLine.orderNbr, Equal<SOShipLine.origOrderNbr>,
						And<SOLine.lineNbr, Equal<SOShipLine.origLineNbr>>>>,
					LeftJoin<SOSalesPerTran, On<SOLine.orderType, Equal<SOSalesPerTran.orderType>,
						And<SOLine.orderNbr, Equal<SOSalesPerTran.orderNbr>,
						And<SOLine.salesPersonID, Equal<SOSalesPerTran.salespersonID>>>>,
					LeftJoin<ARTran, On<ARTran.sOShipmentNbr, Equal<SOShipLine.shipmentNbr>,
						And<ARTran.sOShipmentType, Equal<SOShipLine.shipmentType>,
						And<ARTran.sOOrderType, Equal<SOShipLine.origOrderType>,
						And<ARTran.sOOrderNbr, Equal<SOShipLine.origOrderNbr>,
						And<ARTran.sOOrderLineNbr, Equal<SOShipLine.origLineNbr>,
						And<ARTran.canceled, NotEqual<True>,
						And<ARTran.isCancellation, NotEqual<True>>>>>>>>,
					LeftJoin<ARTranAccrueCost, On<ARTranAccrueCost.tranType, Equal<SOLine.invoiceType>,
						  And<ARTranAccrueCost.refNbr, Equal<SOLine.invoiceNbr>,
						  And<ARTranAccrueCost.lineNbr, Equal<SOLine.invoiceLineNbr>>>>>>>>,
					Where<SOShipLine.shipmentNbr, Equal<Required<SOShipLine.shipmentNbr>>,
						And<SOShipLine.shipmentType, Equal<Required<SOShipLine.shipmentType>>,
						And<SOShipLine.origOrderType, Equal<Required<SOShipLine.origOrderType>>,
						And<SOShipLine.origOrderNbr, Equal<Required<SOShipLine.origOrderNbr>>>>>>>.Select(this, orderShipment.ShipmentNbr, orderShipment.ShipmentType, orderShipment.OrderType, orderShipment.OrderNbr);

		private void InvoiceOrderImpl(InvoiceOrderArgs args)
		{
			ARInvoice newdoc;

			SOOrderShipment orderShipment = args.OrderShipment;
			SOOrder soOrder = args.SOOrder;
			CurrencyInfo soCuryInfo = args.SoCuryInfo;
			SOAddress soBillAddress = args.SoBillAddress;
			SOContact soBillContact = args.SoBillContact;
			SOOrderType soOrderType = SOOrderType.PK.Find(this, soOrder.OrderType);
			SOShipment soShipment = GetShipment(orderShipment);

			SOOpenPeriodAttribute.SetValidatePeriod<ARInvoice.finPeriodID>(Document.Cache, null, PeriodValidation.Nothing);

			if (args.List != null)
			{
				DateTime orderInvoiceDate = GetOrderInvoiceDate(args.InvoiceDate, soOrder, orderShipment);

				newdoc = FindOrCreateInvoice(orderInvoiceDate, args);

				if (newdoc.RefNbr != null)
				{
					Document.Current = newdoc = this.Document.Search<ARInvoice.refNbr>(newdoc.RefNbr, newdoc.DocType);
				}
				else
				{
					this.Clear();

					newdoc.DocType = GetInvoiceDocType(soOrderType, soOrder, orderShipment.Operation);

					newdoc.DocDate = orderInvoiceDate;
					newdoc.BranchID = soOrder.BranchID;

					if (string.IsNullOrEmpty(soOrder.FinPeriodID) == false)
					{
						newdoc.FinPeriodID = soOrder.FinPeriodID;
					}

					if (soOrder.InvoiceNbr != null)
					{
						newdoc.RefNbr = soOrder.InvoiceNbr;
						newdoc.RefNoteID = soOrder.NoteID;
					}

					if (soOrderType.UserInvoiceNumbering == true && string.IsNullOrEmpty(newdoc.RefNbr))
					{
						throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<SOOrder.invoiceNbr>(soorder.Cache));
					}

					if (soOrderType.UserInvoiceNumbering == false && !string.IsNullOrEmpty(newdoc.RefNbr))
					{
						throw new PXException(Messages.MustBeUserNumbering, soOrderType.InvoiceNumberingID);
					}

					AutoNumberAttribute.SetNumberingId<ARInvoice.refNbr>(Document.Cache, newdoc.DocType, soOrderType.InvoiceNumberingID);

					newdoc = (ARInvoice)Document.Cache.CreateCopy(this.Document.Insert(newdoc));

					newdoc.CustomerID = soOrder.CustomerID;
					newdoc.CustomerLocationID = soOrder.CustomerLocationID;

					if (newdoc.DocType != ARDocType.CreditMemo)
					{
						newdoc.TermsID = soOrder.TermsID;
						newdoc.DiscDate = soOrder.DiscDate;
						newdoc.DueDate = soOrder.DueDate;
					}

					newdoc.TaxZoneID = soOrder.TaxZoneID;
					newdoc.TaxCalcMode = soOrder.TaxCalcMode;
					newdoc.ExternalTaxExemptionNumber = soOrder.ExternalTaxExemptionNumber;
					newdoc.AvalaraCustomerUsageType = soOrder.AvalaraCustomerUsageType;
					newdoc.SalesPersonID = soOrder.SalesPersonID;
					newdoc.DocDesc = soOrder.OrderDesc;
					newdoc.InvoiceNbr = soOrder.CustomerOrderNbr;
					newdoc.CuryID = soOrder.CuryID;
					newdoc.ProjectID = soOrder.ProjectID ?? PM.ProjectDefaultAttribute.NonProject();
					newdoc.Hold = args.QuickProcessFlow != PXQuickProcess.ActionFlow.HasNextInFlow && soOrderType.InvoiceHoldEntry == true;
					newdoc.DisableAutomaticTaxCalculation = soOrder.DisableAutomaticTaxCalculation;

					if (soOrderType.MarkInvoicePrinted == true)
					{
						newdoc.Printed = true;
					}

					if (soOrderType.MarkInvoiceEmailed == true)
					{
						newdoc.Emailed = true;
					}

					if (soOrder.PMInstanceID != null || string.IsNullOrEmpty(soOrder.PaymentMethodID) == false)
					{
						if (CustomerPaymentMethod.PK.Find(this, soOrder.PMInstanceID)?.IsActive == true)
						{
							newdoc.PMInstanceID = soOrder.PMInstanceID;
						}
						newdoc.PaymentMethodID = soOrder.PaymentMethodID;
						newdoc.CashAccountID = soOrder.CashAccountID;
					}

					var cancelDefaulting = new List<(Type itemType, string fieldName, PXFieldDefaulting handler)>()
					{
						CancelDefaulting<ARInvoice.branchID>(),
						CancelDefaulting<ARInvoice.taxZoneID>()
					};

					if (newdoc.DocType != ARDocType.CreditMemo)
					{
						cancelDefaulting.Add(CancelDefaulting<SOInvoice.paymentMethodID>());
						cancelDefaulting.Add(CancelDefaulting<SOInvoice.pMInstanceID>());
					}

					try
					{
						using (new PXReadDeletedScope())
						{
							newdoc = this.Document.Update(newdoc);

							bool isUpdateNeeded = false;
							if (newdoc.AvalaraCustomerUsageType != soOrder.AvalaraCustomerUsageType)
							{
								newdoc.AvalaraCustomerUsageType = soOrder.AvalaraCustomerUsageType;
								isUpdateNeeded = true;
							}
							if (newdoc.TaxCalcMode != soOrder.TaxCalcMode)
							{
								newdoc.TaxCalcMode = soOrder.TaxCalcMode;
								isUpdateNeeded = true;
							}

							if (isUpdateNeeded)
							{
								newdoc = this.Document.Update(newdoc);
							}
						}
					}
					finally
					{
						foreach (var eventHandler in cancelDefaulting)
						{
							FieldDefaulting.RemoveHandler(
								eventHandler.itemType, eventHandler.fieldName, eventHandler.handler);
						}
					}

					if (soOrder.PMInstanceID != null || string.IsNullOrEmpty(soOrder.PaymentMethodID) == false)
					{
						if (CustomerPaymentMethod.PK.Find(this, soOrder.PMInstanceID)?.IsActive == true)
						{
							SODocument.Current.PMInstanceID = soOrder.PMInstanceID;
						}
						else
						{
							newdoc.PMInstanceID = null;
						}
						SODocument.Current.PaymentMethodID = soOrder.PaymentMethodID;
						if (SODocument.Current.CashAccountID != soOrder.CashAccountID)
							SODocument.SetValueExt<SOInvoice.cashAccountID>(SODocument.Current, soOrder.CashAccountID);
						if (SODocument.Current.CashAccountID == null)
							SODocument.Cache.SetDefaultExt<SOInvoice.cashAccountID>(SODocument.Current);
						if (SODocument.Current.ARPaymentDepositAsBatch == true && SODocument.Current.DepositAfter == null)
							SODocument.Current.DepositAfter = SODocument.Current.AdjDate;
						SODocument.Current.ExtRefNbr = soOrder.ExtRefNbr;

						//clear error in case invoice currency different from default cash account for customer
						SODocument.Cache.RaiseExceptionHandling<SOInvoice.cashAccountID>(SODocument.Current, null, null);
					}

					CurrencyInfo info = this.currencyinfo.Select();
					bool curyRateNotDefined = (info.CuryRate ?? 0m) == 0m;
					if (curyRateNotDefined || soOrderType.UseCuryRateFromSO == true)
					{
						PXCache<CurrencyInfo>.RestoreCopy(info, soCuryInfo);
						info.CuryInfoID = newdoc.CuryInfoID;
					}
					else
					{
						info.CuryRateTypeID = soCuryInfo.CuryRateTypeID;
						currencyinfo.Update(info);
					}

					AddressAttribute.CopyRecord<ARInvoice.billAddressID>(this.Document.Cache, newdoc, soBillAddress, true);
					if (soBillAddress?.IsValidated == true && Billing_Address.Current != null)
						Billing_Address.Current.IsValidated = true;
					ContactAttribute.CopyRecord<ARInvoice.billContactID>(this.Document.Cache, newdoc, soBillContact, true);
					var soShipContact = SOContact.PK.Find(this, orderShipment.ShipContactID);
					ARShippingContactAttribute.CopyRecord<ARInvoice.shipContactID>(this.Document.Cache, newdoc, soShipContact, true);
				}
			}
			else
			{
				newdoc = (ARInvoice)Document.Cache.CreateCopy(Document.Current);
				bool newInvoice = (Transactions.SelectSingle() == null);

				if (newInvoice)
				{
					newdoc.CustomerID = soOrder.CustomerID;
					newdoc.ProjectID = soOrder.ProjectID;
					newdoc.CustomerLocationID = soOrder.CustomerLocationID;
					newdoc.SalesPersonID = soOrder.SalesPersonID;
					newdoc.TaxZoneID = soOrder.TaxZoneID;
					newdoc.TaxCalcMode = soOrder.TaxCalcMode;
					newdoc.ExternalTaxExemptionNumber = soOrder.ExternalTaxExemptionNumber;
					newdoc.AvalaraCustomerUsageType = soOrder.AvalaraCustomerUsageType;
					newdoc.DocDesc = soOrder.OrderDesc;
					newdoc.InvoiceNbr = soOrder.CustomerOrderNbr;
					newdoc.TermsID = soOrder.TermsID;
					newdoc.DisableAutomaticTaxCalculation = soOrder.DisableAutomaticTaxCalculation;
				}

				CurrencyInfo info = this.currencyinfo.Select();
				bool curyRateNotDefined = (info.CuryRate ?? 0m) == 0m;
				bool copyCuryInfoFromSO = (curyRateNotDefined || newInvoice && soOrderType.UseCuryRateFromSO == true);
				if (copyCuryInfoFromSO)
				{
					PXCache<CurrencyInfo>.RestoreCopy(info, soCuryInfo);
					info.CuryInfoID = newdoc.CuryInfoID;
					this.currencyinfo.Update(info);
					newdoc.CuryID = info.CuryID;
				}
				else
				{
					if (!this.currencyinfo.Cache.ObjectsEqual<CurrencyInfo.curyID>(info, soCuryInfo)
						|| !this.currencyinfo.Cache.ObjectsEqual<
							CurrencyInfo.curyRateTypeID,
							CurrencyInfo.curyMultDiv,
							CurrencyInfo.curyRate>(info, soCuryInfo)
						&& soOrderType.UseCuryRateFromSO == true)
					{
						throw new PXException(Messages.CurrencyRateDiffersInSO, soOrder.RefNbr);
					}
				}

				newdoc = this.Document.Update(newdoc);

				AddressAttribute.CopyRecord<ARInvoice.billAddressID>(this.Document.Cache, newdoc, soBillAddress, true);
				if (soBillAddress?.IsValidated == true && Billing_Address.Current != null)
					Billing_Address.Current.IsValidated = true;
				ContactAttribute.CopyRecord<ARInvoice.billContactID>(this.Document.Cache, newdoc, soBillContact, true);
				var soShipContact = SOContact.PK.Find(this, orderShipment.ShipContactID);
				ARShippingContactAttribute.CopyRecord<ARInvoice.shipContactID>(this.Document.Cache, newdoc, soShipContact, true);
			}

			if (newdoc.DisableAutomaticTaxCalculation == null)
				newdoc.DisableAutomaticTaxCalculation = soOrder.DisableAutomaticTaxCalculation;

			CopyOrderHeaderNoteAndFiles(soOrder, Document.Current, soOrderType);

			SODocument.Current = (SOInvoice)SODocument.Select() ?? (SOInvoice)SODocument.Cache.Insert();
			if (SODocument.Current.ShipAddressID == null)
			{
				DefaultShippingAddress(newdoc, orderShipment, soShipment);
			}
			else if (SODocument.Current.ShipAddressID != orderShipment.ShipAddressID && newdoc.MultiShipAddress != true)
			{
				newdoc.MultiShipAddress = true;
				ARShippingAddressAttribute.DefaultRecord<ARInvoice.shipAddressID>(this.Document.Cache, newdoc);
			}

			bool prevHoldState = newdoc.Hold == true;
			if (newdoc.Hold != true)
			{
				newdoc.Hold = true;
				newdoc = this.Document.Update(newdoc);
			}
			InvoiceCreated(newdoc, args);

			//Delete all discount details that belong to the order (shipment) being currently inserted (it is possible when two partial shipments created for the same order are invoiced together).
			//Discounts will be recalculated/prorated later.
			PXSelectBase<ARInvoiceDiscountDetail> selectInvoiceDiscounts = new PXSelect<ARInvoiceDiscountDetail,
			Where<ARInvoiceDiscountDetail.docType, Equal<Current<SOInvoice.docType>>,
			And<ARInvoiceDiscountDetail.refNbr, Equal<Current<SOInvoice.refNbr>>,
			And<ARInvoiceDiscountDetail.orderType, Equal<Required<ARInvoiceDiscountDetail.orderType>>,
			And<ARInvoiceDiscountDetail.orderNbr, Equal<Required<ARInvoiceDiscountDetail.orderNbr>>>>>>>(this);

			foreach (ARInvoiceDiscountDetail detail in selectInvoiceDiscounts.Select(orderShipment.OrderType, orderShipment.OrderNbr))
			{
				//Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers 
				ARDiscountEngine.DeleteDiscountDetail(this.ARDiscountDetails.Cache, ARDiscountDetails, detail);
			}

			TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(this.Transactions.Cache, null, TaxCalc.ManualCalc);

			InsertSOShipLines(args);

			//DropShip Receipt/Shipment cannot be invoiced twice thats why we have to be sure that all SOPO links at this point in that Receipt are valid:
			ValidateDropShipSOPOLinks(args, orderShipment);

			//orderShipment is updated in several stages. This part sets values that can be used by the CreateInvoiceDetails() method.
			#region Updating orderShipment. Stage 1
			orderShipment = PXCache<SOOrderShipment>.CreateCopy(orderShipment);
			orderShipment.InvoiceType = Document.Current.DocType;
			orderShipment.InvoiceNbr = Document.Current.RefNbr;
			if (string.Equals(orderShipment.ShipmentNbr, Constants.NoShipmentNbr))
			{
				orderShipment.ShippingRefNoteID = Document.Current.NoteID;
			}
			orderShipment = shipmentlist.Update(orderShipment);
			#endregion

			var detailsProcessingResult = CreateInvoiceDetails(newdoc, args, orderShipment, soOrderType);

			HashSet<ARTran> arTranSet = detailsProcessingResult.arTranSet;
			Dictionary<int, SOSalesPerTran> dctCommissions = detailsProcessingResult.dctCommissions;
			DateTime? origInvoiceDate = detailsProcessingResult.origInvoiceDate;
			bool updateINRequired = detailsProcessingResult.updateINRequired;

			var miscLines = CreateInvoiceDetailsForMiscLines(newdoc, args, orderShipment, soOrderType, dctCommissions);
			arTranSet.AddRange(miscLines);

			ValidateLinesAdded(detailsProcessingResult.lineAdded || miscLines.Any());

			ProcessSalespersonCommissions(newdoc, args, dctCommissions);

			if (LinesSortingAttribute.AllowSorting(Transactions.Cache, SODocument.Current))
				ResortTransactions(arTranSet, this.UnattendedMode);

			SODocument.Current.InitialSOBehavior ??= soOrder.Behavior;
			SODocument.Current.BillAddressID = soOrder.BillAddressID;
			SODocument.Current.BillContactID = soOrder.BillContactID;

			SODocument.Current.ShipAddressID = orderShipment.ShipAddressID;
			SODocument.Current.ShipContactID = orderShipment.ShipContactID;

			SODocument.Current.PaymentProjectID = PM.ProjectDefaultAttribute.NonProject();

			SODocument.Current.CreateINDoc |= (updateINRequired && orderShipment.InvtRefNbr == null);

			SOFreightDetail freightDetail = FillFreightDetails(soOrder, orderShipment);

			//Second stage of the orderShipment update.
			//It is called after the FillFreightDetails() method, as some fields of the orderShipment can potentially be modified inside that method.
			#region Updating orderShipment. Stage 2
			orderShipment = PXCache<SOOrderShipment>.CreateCopy(orderShipment);
			orderShipment.CreateINDoc = updateINRequired;
			orderShipment = shipmentlist.Update(orderShipment);
			orderShipment = orderShipment.LinkInvoice(SODocument.Current, this, false); //taxes will be allocated later in the AddOrderTaxes() method
			#endregion

			if (string.Equals(orderShipment.ShipmentNbr, Constants.NoShipmentNbr))
			{
				SOOrder cached = soorder.Locate(soOrder);
				if (cached != null)
				{
					if (cached.Behavior.IsIn(SOBehavior.SO, SOBehavior.RM) && cached.OpenLineCntr == 0)
						cached.MarkCompleted();

					cached.ShipmentCntr++;
					soorder.Update(cached);
				}
			}

			ProcessGroupAndDocumentDiscounts(newdoc, args, orderShipment, soOrderType);

			newdoc.IsPriceAndDiscountsValid = true;

			AddOrderTaxes(orderShipment);

			if (!IsExternalTax(Document.Current.TaxZoneID))
			{
				if (soShipment != null && soShipment.TaxCategoryID != soOrder.FreightTaxCategoryID)
				{
					// if freight tax category is changed on the shipment we need to recalculate freight tax
					// because the tax added in the shipment may be absent in the sales order
					TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(this.Transactions.Cache, null, TaxCalc.ManualLineCalc);
					try
					{
						freightDetail.TaxCategoryID = soShipment.TaxCategoryID;
						FreightDetails.Update(freightDetail);
					}
					finally
					{
						TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(this.Transactions.Cache, null, TaxCalc.ManualCalc);
					}
				}
			}

			if (!IsExternalTax(Document.Current.TaxZoneID) || SOInvoiceAddOrderPaymentsScope.IsActive)
				InsertApplications(orderShipment);

			newdoc = (ARInvoice)Document.Cache.CreateCopy(Document.Current);
			newdoc.OrigDocDate = origInvoiceDate;

			if (newdoc.DocType == ARDocType.CreditMemo)
			{
				PXFormulaAttribute.CalcAggregate<ARAdjust.curyAdjdAmt>(Adjustments_1.Cache, newdoc, false);
			}
			else
			{
				PXFormulaAttribute.CalcAggregate<ARAdjust2.adjdRefNbr>(Adjustments.Cache, newdoc, false);
				PXFormulaAttribute.CalcAggregate<ARAdjust2.curyAdjdAmt>(Adjustments.Cache, newdoc, false);
				PXFormulaAttribute.CalcAggregate<ARAdjust2.curyAdjdWOAmt>(Adjustments.Cache, newdoc, false);
			}

			PXResultset<SOOrderShipment> invoiceOrderShipments = PXSelect<SOOrderShipment, Where<SOOrderShipment.invoiceType, Equal<Current<ARInvoice.docType>>, And<SOOrderShipment.invoiceNbr, Equal<Current<ARInvoice.refNbr>>>>>.Select(this);

			UpdateInvoiceIfItContainsSeveralShipments(newdoc, args, invoiceOrderShipments);

			ProcessFreeItemDiscountsFromSeveralShipments(newdoc, args, invoiceOrderShipments, true);

			this.Document.Update(newdoc);
			SOOpenPeriodAttribute.SetValidatePeriod<ARInvoice.finPeriodID>(Document.Cache, null, PeriodValidation.DefaultSelectUpdate);

			void RestoreHold(bool toValue)
			{
				var doc = this.Document.Current;
				if (doc.CuryDocBal >= 0 && doc.Hold != toValue)
				{
					doc.Hold = toValue;
					this.Document.Update(doc);
				}
			}

			if (args.List == null)
			{
				RestoreHold(prevHoldState);
			}
			else
			{
				if (HasOrderShipmentTransactions(orderShipment))
				{
					try
					{
						this.Document.Current.ApplyPaymentWhenTaxAvailable = true;

						if (soOrderType.AutoWriteOff == true)
						{
							AutoWriteOffBalance(args.Customer);
						}

						if (!IsExternalTax(Document.Current.TaxZoneID))
							RestoreHold(prevHoldState);

						this.Save.Press();

						if (IsExternalTax(Document.Current.TaxZoneID))
						{
							//clear caches before save in second time
							var ordercache = this.soorder.Cache;
							ordercache.ClearQueryCache();
							ordercache.Clear();

							InsertApplications(orderShipment);
							
							RestoreHold(prevHoldState);
							
							this.Save.Press();
						}
					}
					finally
					{
						this.Document.Current.ApplyPaymentWhenTaxAvailable = false;
					}


					if (args.List.Find(this.Document.Current) == null)
					{
						CurrencyInfo currencyInfo = this.currencyinfo.Select();
						args.List.Add(this.Document.Current, this.SODocument.Current, currencyInfo);
					}
				}
				else
				{
					this.Clear();
				}
			}
		}

		private PXRowUpdated CreateApprovedBalanceCollectorDelegate(InvoiceOrderArgs args)
		{
			SOOrder soOrder = args.SOOrder;

			decimal ApprovedBalance = 0;
			HashSet<SOOrder> accountedForOrders = new HashSet<SOOrder>(soorder.Cache.GetComparer());

			PXRowUpdated ApprovedBalanceCollector = delegate (PXCache sender, PXRowUpdatedEventArgs e)
			{
				ARInvoice ARDoc = (ARInvoice)e.Row;

				//Discounts can reduce the balance - adjust the creditHold if it was wrongly set:
				if ((decimal)ARDoc.DocBal <= ApprovedBalance && ARDoc.CreditHold == true)
				{
					object OldRow = sender.CreateCopy(ARDoc);
					sender.SetValueExt<ARInvoice.creditHold>(ARDoc, false);
					sender.RaiseRowUpdated(ARDoc, OldRow);
				}

				//Maximum approved balance for an invoice is the sum of all approved order amounts:
				if ((bool)soOrder.ApprovedCredit)
				{
					if (!accountedForOrders.Contains(soOrder))
					{
						ApprovedBalance += soOrder.ApprovedCreditAmt.GetValueOrDefault();
						accountedForOrders.Add(soOrder);
					}

					ARDoc.ApprovedCreditAmt = ApprovedBalance;
					ARDoc.ApprovedCredit = true;
				}
			};

			return ApprovedBalanceCollector;
		}

		public virtual void InvoiceOrder(InvoiceOrderArgs args)
		{
			InvoicePreProcessingValidations(args);
			PXRowUpdated ApprovedBalanceCollector = CreateApprovedBalanceCollectorDelegate(args);

			var customerCreditExtension = this.FindImplementation<AR.GraphExtensions.ARInvoiceEntry_ARInvoiceCustomerCreditExtension>();
			if (customerCreditExtension != null)
			{
				customerCreditExtension.AppendPreUpdatedEvent(typeof(ARInvoice), ApprovedBalanceCollector);
			}

			try
			{
				InvoiceOrderImpl(args);
			}
			finally
			{
				customerCreditExtension?.RemovePreUpdatedEvent(typeof(ARInvoice), ApprovedBalanceCollector);
			TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(this.Transactions.Cache, null, TaxCalc.ManualLineCalc);
		}
		}

		/// <summary>
		/// Copies notes and files from Sales Order to SO Invoice
		/// </summary>
		public virtual void CopyOrderHeaderNoteAndFiles(SOOrder srcOrder, ARInvoice dstInvoice, SOOrderType orderType)
		{
			bool copyNote = PXNoteAttribute.GetNote(Document.Cache, dstInvoice) == null ? (orderType.CopyHeaderNotesToInvoice ?? false) : false;
			PXNoteAttribute.CopyNoteAndFiles(Caches[typeof(SOOrder)], srcOrder, Document.Cache, dstInvoice, copyNote, orderType.CopyHeaderFilesToInvoice);
		}

		/// <summary>
		/// This method can be used to add validations that should be performed right before the start of the SO Invoice creation procedure.
		/// </summary>
		/// <param name="args">InvoiceOrderArgs contains original SOOrder, SOLines, SOShipLines, SOOrderShipment, etc. that were passed to the main InvoiceOrder method</param>
		public virtual void InvoicePreProcessingValidations(InvoiceOrderArgs args)
		{
			SOOrderShipment orderShipment = args.OrderShipment;

			//TODO: Temporary solution. Review when AC-80210 is fixed
			if (orderShipment.ShipmentNbr != Constants.NoShipmentNbr && orderShipment.ShipmentType != SOShipmentType.DropShip && orderShipment.Confirmed != true)
			{
				throw new PXException(Messages.UnableToProcessUnconfirmedShipment, orderShipment.ShipmentNbr);
			}

			if (orderShipment.InvoiceNbr != null)
			{
				throw new PXInvalidOperationException(Messages.ActionNotAvailableInCurrentState,
					selectShipment.GetCaption(), shipmentlist.Cache.GetRowDescription(orderShipment));
			}
		}

		public virtual void InsertSOShipLines(InvoiceOrderArgs args)
		{
			if (args.Details != null)
			{
				PXCache cache = this.Caches[typeof(SOShipLine)];
				foreach (PXResult<SOShipLine, SOLine> det in args.Details)
				{
					SOShipLine shipline = det;
					SOLine soline = det;

					if (Math.Abs(soline.BaseQty.Value) >= 0.0000005m && (soline.UnassignedQty >= 0.0000005m || soline.UnassignedQty <= -0.0000005m))
					{
						throw new PXException(Messages.BinLotSerialNotAssigned);
					}

					//there should be no parent record of SOLineSplit2 type.
					var insertedshipline = (SOShipLine)cache.Insert(shipline);

					if (insertedshipline == null)
						continue;

					if (insertedshipline.LineType == SOLineType.Inventory)
					{
						var ii = IN.InventoryItem.PK.Find(this, insertedshipline.InventoryID);
						if (ii.StkItem == false && ii.KitItem == true)
						{
							insertedshipline.RequireINUpdate = ((SOLineSplit)PXSelectJoin<SOLineSplit,
								InnerJoin<IN.InventoryItem,
									On2<SOLineSplit.FK.InventoryItem,
									And<IN.InventoryItem.stkItem, Equal<True>>>>,
								Where<SOLineSplit.orderType, Equal<Current<SOLine.orderType>>, And<SOLineSplit.orderNbr, Equal<Current<SOLine.orderNbr>>, And<SOLineSplit.lineNbr, Equal<Current<SOLine.lineNbr>>, And<SOLineSplit.qty, Greater<Zero>>>>>>.SelectSingleBound(this, new object[] { soline })) != null;
						}
						else
						{
							insertedshipline.RequireINUpdate = ii.StkItem;
						}
					}
					else
					{
						insertedshipline.RequireINUpdate = false;
					}
				}
			}
		}

		/// <summary>
		/// Validates that drop-ship PO has no lines that are not linked with SO
		/// </summary>
		/// <param name="args">InvoiceOrderArgs contains original SOOrder, SOLines, SOShipLines, SOOrderShipment, etc. that were passed to the main InvoiceOrder method</param>
		/// <param name="currentOrderShipment">Current SOOrderShipment. It can potentially be different from its original value that is stored in the args</param>
		public virtual void ValidateDropShipSOPOLinks(InvoiceOrderArgs args, SOOrderShipment currentOrderShipment)
		{
			if (currentOrderShipment.ShipmentType == SOShipmentType.DropShip)
			{
				PXSelectBase<POReceiptLine> selectUnlinkedDropShips = new PXSelectJoin<POReceiptLine,
					InnerJoin<PO.POLine, On<PO.POLine.orderType, Equal<POReceiptLine.pOType>, And<PO.POLine.orderNbr, Equal<POReceiptLine.pONbr>, And<PO.POLine.lineNbr, Equal<POReceiptLine.pOLineNbr>>>>,
					LeftJoin<SOLineSplit, On<SOLineSplit.pOType, Equal<POReceiptLine.pOType>, And<SOLineSplit.pONbr, Equal<POReceiptLine.pONbr>, And<SOLineSplit.pOLineNbr, Equal<POReceiptLine.pOLineNbr>>>>>>,
					Where<POReceiptLine.receiptType, Equal<PO.POReceiptType.poreceipt>,
					And<POReceiptLine.receiptNbr, Equal<Required<POReceiptLine.receiptNbr>>,
					And<SOLineSplit.pOLineNbr, IsNull,
					And<Where<POReceiptLine.lineType, Equal<POLineType.goodsForDropShip>, Or<POReceiptLine.lineType, Equal<POLineType.nonStockForDropShip>>>>>>>>(this);

				var unlinkedDropShipsLines = selectUnlinkedDropShips.Select(currentOrderShipment.ShipmentNbr);
				if (unlinkedDropShipsLines.Count > 0)
				{
					foreach (POReceiptLine line in unlinkedDropShipsLines)
					{
						InventoryItem item = IN.InventoryItem.PK.Find(this, line.InventoryID);
						PXTrace.WriteError(Messages.SOPOLinkIsIvalidInPOOrder, line.PONbr, item?.InventoryCD);
					}

					throw new PXException(Messages.SOPOLinkIsIvalid);
				}
			}
		}

		/// <summary>
		/// this method copies salesperson commissions from SO to AR
		/// </summary>
		/// <param name="args">InvoiceOrderArgs contains original SOOrder, SOLines, SOShipLines, SOOrderShipment, etc. that were passed to the main InvoiceOrder method</param>
		public virtual void ProcessSalespersonCommissions(ARInvoice newdoc, InvoiceOrderArgs args, Dictionary<int, SOSalesPerTran> dctCommissions)
		{
			foreach (SOSalesPerTran sspt in dctCommissions.Values)
			{
				ARSalesPerTran aspt = new ARSalesPerTran();
				aspt.DocType = newdoc.DocType;
				aspt.RefNbr = newdoc.RefNbr;
				aspt.SalespersonID = sspt.SalespersonID;
				commisionlist.Cache.SetDefaultExt<ARSalesPerTran.adjNbr>(aspt);
				commisionlist.Cache.SetDefaultExt<ARSalesPerTran.adjdRefNbr>(aspt);
				commisionlist.Cache.SetDefaultExt<ARSalesPerTran.adjdDocType>(aspt);
				aspt = commisionlist.Locate(aspt);
				if (aspt != null && aspt.CommnPct != sspt.CommnPct)
				{
					aspt.CommnPct = sspt.CommnPct;
					commisionlist.Update(aspt);
				}
			}
		}

		/// <summary>
		/// Creates ARTran (Invoice details) records from sales order and shipment lines
		/// </summary>
		/// <param name="args">InvoiceOrderArgs contains original SOOrder, SOLines, SOShipLines, SOOrderShipment, etc. that were passed to the main InvoiceOrder method</param>
		/// <param name="currentOrderShipment">Current SOOrderShipment. It can potentially be different from its original value that is stored in the args</param>
		public virtual (HashSet<ARTran> arTranSet, Dictionary<int, SOSalesPerTran> dctCommissions, DateTime? origInvoiceDate, bool updateINRequired, bool lineAdded) CreateInvoiceDetails
			(ARInvoice newdoc, InvoiceOrderArgs args, SOOrderShipment currentOrderShipment, SOOrderType soOrderType)
		{
			DateTime? origInvoiceDate = null;
			bool updateINRequired = (currentOrderShipment.ShipmentType == SOShipmentType.DropShip);

			HashSet<ARTran> arTranSet = new HashSet<ARTran>(Transactions.Cache.GetComparer());
			Dictionary<int, SOSalesPerTran> dctCommissions = new Dictionary<int, SOSalesPerTran>();

			var deletedLines = this.Transactions.Cache.Deleted.OfType<ARTran>()
				.Where(e => e.SOShipmentNbr != null && e.SOOrderNbr != null)
				.Select(e => (
					e.SOShipmentNbr,
					e.SOShipmentType,
					e.SOOrderNbr,
					e.SOOrderType,
					e.SOOrderLineNbr
				)).ToHashSet();

			bool lineAdded = false;

			foreach (PXResult<SOShipLine, SOLine, SOSalesPerTran, ARTran, ARTranAccrueCost> res in
				SelectLinesToInvoice(currentOrderShipment))
			{
				ARTran artran = (ARTran)res;
				ARTranAccrueCost artranAccrueCost = (ARTranAccrueCost)res;
				SOSalesPerTran sspt = (SOSalesPerTran)res;
				SOLine orderline = (SOLine)res;
				SOShipLine shipline = (SOShipLine)res;

				if (sspt != null && sspt.SalespersonID != null && !dctCommissions.ContainsKey(sspt.SalespersonID.Value))
				{
					dctCommissions[sspt.SalespersonID.Value] = sspt;
				}

				if (artran.RefNbr == null || (artran.RefNbr != null && deletedLines.Contains((shipline.ShipmentNbr, shipline.ShipmentType, orderline.OrderNbr, orderline.OrderType, orderline.LineNbr))))
				{
					//TODO: Temporary solution. Review when AC-80210 is fixed
					if (shipline.ShipmentNbr != null && currentOrderShipment.ShipmentType != SOShipmentType.DropShip && currentOrderShipment.ShipmentNbr != Constants.NoShipmentNbr && (shipline.Confirmed != true || shipline.UnassignedQty != 0))
					{
						throw new PXException(Messages.UnableToProcessUnconfirmedShipment, shipline.ShipmentNbr);
					}

					if (Math.Abs((decimal)shipline.BaseShippedQty) < 0.0000005m && !string.Equals(shipline.ShipmentNbr, Constants.NoShipmentNbr))
					{
						continue;
					}

					if (origInvoiceDate == null && orderline.InvoiceDate != null)
						origInvoiceDate = orderline.InvoiceDate;

					lineAdded = true;

					bool allowShiplineMerge = IsShiplineMergeAllowed(shipline);
					ARTran newtran = CreateTranFromShipLine(newdoc, soOrderType, orderline.Operation, orderline, ref shipline);
					if (allowShiplineMerge)
					{
						foreach (ARTran existing in Transactions.Cache.Inserted
							.Concat_(Transactions.Cache.Updated))
						{
							if (Transactions.Cache.ObjectsEqual<ARTran.sOShipmentNbr, ARTran.sOShipmentType, ARTran.sOOrderType, ARTran.sOOrderNbr, ARTran.sOOrderLineNbr>(newtran, existing))
							{
								Transactions.Cache.RestoreCopy(newtran, existing);
								break;
							}
						}
					}

					if (artranAccrueCost != null && artranAccrueCost.AccrueCost == true)
					{
						newtran.AccrueCost = artranAccrueCost.AccrueCost;
						newtran.CostBasis = artranAccrueCost.CostBasis;
						newtran.ExpenseAccrualAccountID = artranAccrueCost.ExpenseAccrualAccountID;
						newtran.ExpenseAccrualSubID = artranAccrueCost.ExpenseAccrualSubID;
						newtran.ExpenseAccountID = artranAccrueCost.ExpenseAccountID;
						newtran.ExpenseSubID = artranAccrueCost.ExpenseSubID;

						if (newtran.Qty != 0 && artranAccrueCost.Qty != 0)
							newtran.AccruedCost = PXPriceCostAttribute.Round((decimal)(artranAccrueCost.AccruedCost ?? 0m * (newtran.Qty / artranAccrueCost.Qty)));

					}

					if (newtran.LineNbr == null)
					{
						try
						{
							cancelUnitPriceCalculation = true;
							newtran = this.Transactions.Insert(newtran);
							arTranSet.Add(newtran);
						}
						catch (PXSetPropertyException e)
						{
							var parentOrderLine = new SOLine()
							{
								OrderType = newtran.SOOrderType,
								OrderNbr = newtran.SOOrderNbr,
								LineNbr = newtran.SOOrderLineNbr
							};

							throw new Common.Exceptions.ErrorProcessingEntityException(
								Caches[parentOrderLine.GetType()], parentOrderLine, e);
						}
						finally
						{
							cancelUnitPriceCalculation = false;
						}

						PXNoteAttribute.CopyNoteAndFiles(Caches[typeof(SOLine)], orderline, Caches[typeof(ARTran)], newtran,
							soOrderType.CopyLineNotesToInvoice == true && (soOrderType.CopyLineNotesToInvoiceOnlyNS == false || orderline.LineType == SOLineType.NonInventory),
							soOrderType.CopyLineFilesToInvoice == true && (soOrderType.CopyLineFilesToInvoiceOnlyNS == false || orderline.LineType == SOLineType.NonInventory));
					}
					else
					{
						newtran = this.Transactions.Update(newtran);
						TaxAttribute.Calculate<ARTran.taxCategoryID>(Transactions.Cache, new PXRowUpdatedEventArgs(newtran, null, true));
					}

					if (newtran.RequireINUpdate == true && newtran.Qty != 0m)
					{
						updateINRequired = true;
					}

				}
			}

			return (arTranSet, dctCommissions, origInvoiceDate, updateINRequired, lineAdded);
		}

		/// <summary>
		/// Creates Invoice details for Misc. sales order lines only
		/// </summary>
		/// <param name="args">InvoiceOrderArgs contains original SOOrder, SOLines, SOShipLines, SOOrderShipment, etc. that were passed to the main InvoiceOrder method</param>
		/// <param name="currentOrderShipment">Current SOOrderShipment. It can potentially be different from its original value that is stored in the args</param>
		public virtual HashSet<ARTran> CreateInvoiceDetailsForMiscLines(ARInvoice newdoc, InvoiceOrderArgs args, SOOrderShipment currentOrderShipment, SOOrderType soOrderType, Dictionary<int, SOSalesPerTran> dctCommissions)
		{
			HashSet<ARTran> arTranSet = new HashSet<ARTran>(Transactions.Cache.GetComparer());

			PXSelectBase<ARTran> cmd = new PXSelect<ARTran,
				Where<ARTran.tranType, Equal<Current<ARInvoice.docType>>,
					And<ARTran.refNbr, Equal<Current<ARInvoice.refNbr>>,
					And<ARTran.sOOrderType, Equal<Current<SOMiscLine2.orderType>>,
					And<ARTran.sOOrderNbr, Equal<Current<SOMiscLine2.orderNbr>>,
					And<ARTran.sOOrderLineNbr, Equal<Current<SOMiscLine2.lineNbr>>>>>>>>(this);


			foreach (PXResult<SOMiscLine2, SOSalesPerTran> res in PXSelectJoin<SOMiscLine2,
																LeftJoin<SOSalesPerTran, On<SOMiscLine2.orderType, Equal<SOSalesPerTran.orderType>,
																	And<SOMiscLine2.orderNbr, Equal<SOSalesPerTran.orderNbr>,
																	And<SOMiscLine2.salesPersonID, Equal<SOSalesPerTran.salespersonID>>>>>,
				Where<SOMiscLine2.orderType, Equal<Required<SOMiscLine2.orderType>>,
					And<SOMiscLine2.orderNbr, Equal<Required<SOMiscLine2.orderNbr>>,
																	And<
																		Where2<
																			Where<SOMiscLine2.curyUnbilledAmt, Greater<decimal0>,   //direct billing process with positive amount
																			And<SOMiscLine2.curyLineAmt, Greater<decimal0>>>,
																		Or2<
																			Where<SOMiscLine2.curyUnbilledAmt, Less<decimal0>,      //billing process with negative amount
																			And<SOMiscLine2.curyLineAmt, Less<decimal0>>>,
																		Or2<
																			Where<SOMiscLine2.curyLineAmt, Equal<decimal0>,         //special case with zero line amount, e.g. discount = 100% or unit price=0
																			And<SOMiscLine2.unbilledQty, Greater<decimal0>,
																			And<SOMiscLine2.orderQty, Greater<decimal0>>>>,
																		Or<
																			Where<SOMiscLine2.curyLineAmt, Equal<decimal0>,         //special case, receipt line of RM order
																			And<SOMiscLine2.unbilledQty, Less<decimal0>,
																			And<SOMiscLine2.orderQty, Less<decimal0>>>>>>>>>>>,
				//process all lines with positive CuryUnbilledAmt first and add negative lines later, retaining order in these groups
				OrderBy<Desc<Switch<Case<Where<SOMiscLine2.curyUnbilledAmt, GreaterEqual<decimal0>>, decimal1>, decimal0>, Asc<SOMiscLine2.lineNbr>>>>
																.Select(this, currentOrderShipment.OrderType, currentOrderShipment.OrderNbr))
			{
				SOMiscLine2 orderline = res;
				SOSalesPerTran sspt = res;
				if (sspt != null && sspt.SalespersonID != null && !dctCommissions.ContainsKey(sspt.SalespersonID.Value))
				{
					dctCommissions[sspt.SalespersonID.Value] = sspt;
				}
				if (cmd.View.SelectSingleBound(new object[] { Document.Current, orderline }) == null)
				{
					ARTran newtran = CreateTranFromMiscLine(currentOrderShipment, orderline);
					ChangeBalanceSign(newtran, newdoc, orderline.DefaultOperation);

					newtran = this.Transactions.Insert(newtran);
					arTranSet.Add(newtran);

					PXNoteAttribute.CopyNoteAndFiles(Caches[typeof(SOMiscLine2)], orderline, Caches[typeof(ARTran)], newtran,
						soOrderType.CopyLineNotesToInvoice, soOrderType.CopyLineFilesToInvoice);
				}
			}

			return arTranSet;
		}

		/// <summary>
		/// This method prepares group and document discounts for SO invoice. Discounts are either prorated from the originating SO to the AR document, or recalculated on the AR level.
		/// </summary>
		/// <param name="args">InvoiceOrderArgs contains original SOOrder, SOLines, SOShipLines, SOOrderShipment, etc. that were passed to the main InvoiceOrder method</param>
		/// <param name="currentOrderShipment">Current SOOrderShipment. It can potentially be different from its original value that is stored in the args</param>
		public virtual void ProcessGroupAndDocumentDiscounts(ARInvoice newdoc, InvoiceOrderArgs args, SOOrderShipment currentOrderShipment, SOOrderType soOrderType)
		{
			SOOrder soOrder = args.SOOrder;

			PXSelectBase<SOLine> transactions = new PXSelect<SOLine,
							Where<SOLine.orderType, Equal<Required<SOOrder.orderType>>,
								And<SOLine.orderNbr, Equal<Required<SOOrder.orderNbr>>>>>(this);
			PXSelectBase<SOOrderDiscountDetail> discountdetail = new PXSelect<SOOrderDiscountDetail,
				Where<SOOrderDiscountDetail.orderType, Equal<Required<SOOrder.orderType>>,
					And<SOOrderDiscountDetail.orderNbr, Equal<Required<SOOrder.orderNbr>>>>>(this);

			Lazy<bool> fullOrderInvoicing = new Lazy<bool>(() => IsFullOrderInvoicing(soOrder, soOrderType, transactions));

			Lazy<TwoWayLookup<SOOrderDiscountDetail, SOLine>> discountCodesWithApplicableSOLines =
				new Lazy<TwoWayLookup<SOOrderDiscountDetail, SOLine>>(() => DiscountEngineProvider.GetEngineFor<SOLine, SOOrderDiscountDetail>()
					.GetListOfLinksBetweenDiscountsAndDocumentLines(
						Caches[typeof(SOLine)],
						transactions,
						discountdetail,
						documentDetailsSelectParams: new string[] { soOrder.OrderType, soOrder.OrderNbr },
						discountDetailsSelectParams: new string[] { soOrder.OrderType, soOrder.OrderNbr }));

			Lazy<bool> hasManualDiscounts = new Lazy<bool>(() => discountCodesWithApplicableSOLines.Value.LeftValues.Any(dd => dd.IsManual == true));
			Lazy<bool> hasExternalDiscounts = new Lazy<bool>(() => discountCodesWithApplicableSOLines.Value.LeftValues.Any(dd => dd.Type == DiscountType.ExternalDocument));

			/*In case Discounts were not recalculated add prorated discounts */
			if (soOrderType.RecalculateDiscOnPartialShipment != true || hasExternalDiscounts.Value || fullOrderInvoicing.Value && hasManualDiscounts.Value)
			{
				decimal? defaultRate = 1m;
				if (soOrder.LineTotal > 0m)
					defaultRate = currentOrderShipment.LineTotal / soOrder.LineTotal;

				TwoWayLookup<ARInvoiceDiscountDetail, ARTran> discountCodesWithApplicableARLines = new TwoWayLookup<ARInvoiceDiscountDetail, ARTran>(leftComparer: new ARInvoiceDiscountDetail.ARInvoiceDiscountDetailComparer());

				foreach (SOOrderDiscountDetail docGroupDisc in discountCodesWithApplicableSOLines.Value.LeftValues)
				{
					if ((soOrderType.RecalculateDiscOnPartialShipment == true && docGroupDisc.IsManual != true) ||
						(docGroupDisc.Type == DiscountType.ExternalDocument && docGroupDisc.SkipDiscount == true))
						continue;

					bool customerDiscountsFeatureEnabled = PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>();

					var dd = new ARInvoiceDiscountDetail
					{
						SkipDiscount = docGroupDisc.SkipDiscount,
						Type = docGroupDisc.Type,
						DiscountID = docGroupDisc.DiscountID,
						DiscountSequenceID = docGroupDisc.DiscountSequenceID,
						OrderType = customerDiscountsFeatureEnabled ? docGroupDisc.OrderType : null,
						OrderNbr = customerDiscountsFeatureEnabled ? docGroupDisc.OrderNbr : null,
						DocType = newdoc.DocType,
						RefNbr = newdoc.RefNbr,
						IsManual = docGroupDisc.IsManual,
						DiscountPct = docGroupDisc.DiscountPct,
						FreeItemID = docGroupDisc.FreeItemID,
						FreeItemQty = docGroupDisc.FreeItemQty,
						ExtDiscCode = docGroupDisc.ExtDiscCode,
						Description = docGroupDisc.Description,
						CuryInfoID = Document.Current.CuryInfoID
					};

					decimal? rate = defaultRate;
					decimal invoicedCuryGroupAmt = 0m;
					decimal invoicedCuryDocAmt = 0m;
					foreach (SOLine soLine in discountCodesWithApplicableSOLines.Value.RightsFor(docGroupDisc))
					{
						foreach (ARTran tran in Transactions.Select())
						{
							bool linkedToSOLine = soLine.OrderType == tran.SOOrderType &&
												  soLine.OrderNbr == tran.SOOrderNbr &&
												  soLine.LineNbr == tran.SOOrderLineNbr &&
												  (customerDiscountsFeatureEnabled || currentOrderShipment.ShipmentNbr.Trim() == tran.SOShipmentNbr.Trim()); //discounts cannot be identified by OrderType/OrderNbr when Customer Discounts feature is disabled

							if (linkedToSOLine)
							{
								bool excludeFromDiscountableAmt = false;
								if (tran.DiscountID != null)
								{
									ARDiscount lineDiscount = ARDiscount.PK.Find(this, tran.DiscountID);
									if (lineDiscount?.ExcludeFromDiscountableAmt == true)
										excludeFromDiscountableAmt = true;
								}

								if (!excludeFromDiscountableAmt)
								{
									if (docGroupDisc.Type == DiscountType.Group)
										invoicedCuryGroupAmt += (tran.CuryTranAmt ?? 0m);
									if (docGroupDisc.Type.IsIn(DiscountType.Document, DiscountType.ExternalDocument))
										invoicedCuryDocAmt += (tran.CuryTranAmt ?? 0m) - ((tran.CuryTranAmt ?? 0m) * (1 - (soLine.GroupDiscountRate ?? 1m)));
								}
							}

							//When customer discounts feature is off, SO Order/SO Invoice can only have one external document discount which is applicable to all lines
							if (linkedToSOLine || !customerDiscountsFeatureEnabled)
							{
								discountCodesWithApplicableARLines.Link(dd, tran);
							}
						}
					}

					bool fullOrderDiscAllocation = (fullOrderInvoicing.Value && docGroupDisc.Type == DiscountType.Document);
					if (fullOrderDiscAllocation)
					{
						rate = 1m;
					}
					else if (docGroupDisc.CuryDiscountableAmt > 0m)
					{
						if (docGroupDisc.Type == DiscountType.Group)
							rate = invoicedCuryGroupAmt / docGroupDisc.CuryDiscountableAmt;
						else if (soOrder.LineTotal != 0m || soOrder.MiscTot != 0)
							rate = invoicedCuryDocAmt / docGroupDisc.CuryDiscountableAmt;
					}

					ARInvoiceDiscountDetail located = ARDiscountDetails.Locate(dd);
					//RecordID prevents Locate() from work as intended. To review.
					if (located == null)
					{
						List<ARInvoiceDiscountDetail> discountDetails = new List<ARInvoiceDiscountDetail>();

						//TODO: review this part
						if (PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>())
						{
							foreach (ARInvoiceDiscountDetail detail in ARDiscountDetails.Cache.Cached)
							{
								if (ARDiscountDetails.Cache.GetStatus(detail).IsNotIn(PXEntryStatus.Deleted, PXEntryStatus.InsertedDeleted)) //discount details that belong to the order being currently processed should have been deleted earlier
								discountDetails.Add(detail);
							}
						}
						else
						{
							foreach (ARInvoiceDiscountDetail detail in ARDiscountDetails.Select())
							{
								discountDetails.Add(detail);
							}
						}

						foreach (ARInvoiceDiscountDetail detail in discountDetails)
						{
							if (detail.DiscountID == dd.DiscountID && detail.DiscountSequenceID == dd.DiscountSequenceID && detail.OrderType == dd.OrderType
								&& detail.OrderNbr == dd.OrderNbr && detail.DocType == dd.DocType && detail.RefNbr == dd.RefNbr && detail.Type == dd.Type)
								located = detail;
						}
					}
					if (located != null)
					{
						if (docGroupDisc.Type == DiscountType.Group || fullOrderDiscAllocation)
						{
							UpdateDiscountDetail(located, docGroupDisc, rate);
						}
						else
						{
							ARDiscountDetails.Cache.SetValueExt<ARInvoiceDiscountDetail.discountAmt>(located, located.DiscountAmt + docGroupDisc.DiscountAmt * rate);
							ARDiscountDetails.Cache.SetValueExt<ARInvoiceDiscountDetail.curyDiscountAmt>(located, located.CuryDiscountAmt + docGroupDisc.CuryDiscountAmt * rate);
							ARDiscountDetails.Cache.SetValueExt<ARInvoiceDiscountDetail.discountableAmt>(located, located.DiscountableAmt + docGroupDisc.DiscountableAmt * rate);
							ARDiscountDetails.Cache.SetValueExt<ARInvoiceDiscountDetail.curyDiscountableAmt>(located, located.CuryDiscountableAmt + docGroupDisc.CuryDiscountableAmt * rate);
							ARDiscountDetails.Cache.SetValueExt<ARInvoiceDiscountDetail.discountableQty>(located, located.DiscountableQty + docGroupDisc.DiscountableQty * rate);
						}
						if (ARDiscountDetails.Cache.GetStatus(located) == PXEntryStatus.Deleted)
							located = ARDiscountEngine.InsertDiscountDetail(this.ARDiscountDetails.Cache, ARDiscountDetails, located);
						else
							located = ARDiscountEngine.UpdateDiscountDetail(this.ARDiscountDetails.Cache, ARDiscountDetails, located);
					}
					else
					{
						UpdateDiscountDetail(dd, docGroupDisc, rate);

						located = ARDiscountEngine.InsertDiscountDetail(ARDiscountDetails.Cache, ARDiscountDetails, dd);
					}

					ARInvoiceDiscountDetail.ARInvoiceDiscountDetailComparer discountDetailComparer = new ARInvoiceDiscountDetail.ARInvoiceDiscountDetailComparer();
					foreach (ARInvoiceDiscountDetail newDiscount in discountCodesWithApplicableARLines.LeftValues)
					{
						if (discountDetailComparer.Equals(newDiscount, located))
						{
							newDiscount.DiscountAmt = located.DiscountAmt;
							newDiscount.CuryDiscountableAmt = located.CuryDiscountableAmt;
							newDiscount.CuryDiscountAmt = located.CuryDiscountAmt;
							newDiscount.DiscountableQty = located.DiscountableQty;
							newDiscount.DiscountableAmt = located.DiscountableAmt;
							newDiscount.IsOrigDocDiscount = located.IsOrigDocDiscount;
							newDiscount.LineNbr = located.LineNbr;
						}
					}
				}

				if (PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>())
				{
					RecalculateTotalDiscount();
				}

				PXSelectBase<ARTran> orderLinesSelect = new PXSelectJoin<ARTran, LeftJoin<SOLine,
				On<SOLine.orderType, Equal<ARTran.sOOrderType>,
				And<SOLine.orderNbr, Equal<ARTran.sOOrderNbr>,
				And<SOLine.lineNbr, Equal<ARTran.sOOrderLineNbr>>>>>,
				Where<ARTran.tranType, Equal<Current<ARInvoice.docType>>,
				And<ARTran.refNbr, Equal<Current<ARInvoice.refNbr>>,
				And<ARTran.sOOrderType, Equal<Required<SOOrder.orderType>>,
				And<ARTran.sOOrderNbr, Equal<Required<SOOrder.orderNbr>>>>>>,
				OrderBy<Asc<ARTran.tranType, Asc<ARTran.refNbr, Asc<ARTran.lineNbr>>>>>(this);

				ARDiscountEngine.CalculateGroupDiscountRate(
					cache: Transactions.Cache,
					lines: orderLinesSelect,
					currentLine: null,
					discountCodesWithApplicableLines: discountCodesWithApplicableARLines,
					useLegacyDetailsSelectMethod: true,
					recalculateTaxes: false,
					documentLinesSelectParams: new string[] { soOrder.OrderType, soOrder.OrderNbr },
					recalcAll: true,
					forceFormulaCalculation: true);

				ARDiscountEngine.CalculateDocumentDiscountRate(Transactions.Cache, discountCodesWithApplicableARLines, null, documentDetails: Transactions, forceFormulaCalculation: true);

				if (!PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>())
				{
					RecalculateTotalDiscount();
				}
			}

			if (soOrderType.RecalculateDiscOnPartialShipment == true)
			{
				//Recalculate all discounts
				ARTran firstLine = null;

				//Recalculate line discounts on each line
				foreach (ARTran tran in Transactions.Select())
				{
					if (firstLine == null)
						firstLine = tran;

					RecalculateDiscounts(this.Transactions.Cache, tran, true);
					this.Transactions.Update(tran);
				}

				//Recalculate group and document discounts
				if (firstLine != null)
				{
					RecalculateDiscounts(this.Transactions.Cache, firstLine);
					this.Transactions.Update(firstLine);
				}
			}
		}

		/// <summary>
		/// Copies free item discounts from SO to AR from several shipments
		/// </summary>
		/// <param name="args">InvoiceOrderArgs contains original SOOrder, SOLines, SOShipLines, SOOrderShipment, etc. that were passed to the main InvoiceOrder method</param>
		/// <param name="useLegacyBehavior">When true, ProcessFreeItemDiscounts will be called for the first order in the invoiceOrderShipments list only. This mode is only kept for better performance and can produce inacurate results.</param>
		public virtual void ProcessFreeItemDiscountsFromSeveralShipments(ARInvoice newdoc, InvoiceOrderArgs args, PXResultset<SOOrderShipment> invoiceOrderShipments, bool useLegacyBehavior)
		{
			HashSet<string> ordersDistinct = new HashSet<string>();
			foreach (SOOrderShipment shipment in invoiceOrderShipments)
			{
				if (useLegacyBehavior)
				{
					ordersDistinct.Add(string.Format("{0}|{1}", shipment.OrderType, shipment.OrderNbr));

					if (args.List != null && ordersDistinct.Count > 1)
						break;
				}

				ProcessFreeItemDiscounts(newdoc, args, shipment);
			}
		}

		/// <summary>
		/// Copies free item discounts from SO to AR
		/// </summary>
		/// <param name="args">InvoiceOrderArgs contains original SOOrder, SOLines, SOShipLines, SOOrderShipment, etc. that were passed to the main InvoiceOrder method</param>
		public virtual void ProcessFreeItemDiscounts(ARInvoice newdoc, InvoiceOrderArgs args, SOOrderShipment shipment)
		{
			#region Update FreeItemQty for DiscountDetails based on shipments

			PXSelectBase<SOShipmentDiscountDetail> selectShipmentDiscounts = new PXSelect<SOShipmentDiscountDetail,
					Where<SOShipmentDiscountDetail.orderType, Equal<Required<SOShipmentDiscountDetail.orderType>>,
					And<SOShipmentDiscountDetail.orderNbr, Equal<Required<SOShipmentDiscountDetail.orderNbr>>,
					And<SOShipmentDiscountDetail.shipmentNbr, Equal<Required<SOShipmentDiscountDetail.shipmentNbr>>>>>>(this);

			foreach (SOShipmentDiscountDetail sdd in selectShipmentDiscounts.Select(shipment.OrderType, shipment.OrderNbr, shipment.ShipmentNbr))
			{
				bool discountDetailLineExist = false;

				foreach (ARInvoiceDiscountDetail idd in ARDiscountDetails.Select())
				{
					if (idd.DocType == newdoc.DocType && idd.RefNbr == newdoc.RefNbr
						&& idd.OrderType == shipment.OrderType && idd.OrderNbr == shipment.OrderNbr
						&& idd.DiscountID == sdd.DiscountID && idd.DiscountSequenceID == sdd.DiscountSequenceID)
					{
						discountDetailLineExist = true;
						if (idd.FreeItemID == null)
						{
							idd.FreeItemID = sdd.FreeItemID;
							idd.FreeItemQty = sdd.FreeItemQty;
						}
						else
							idd.FreeItemQty = sdd.FreeItemQty;
					}
				}

				if (!discountDetailLineExist)
				{
					var idd = new ARInvoiceDiscountDetail
					{
						Type = DiscountType.Group,
						DocType = newdoc.DocType,
						RefNbr = newdoc.RefNbr,
						OrderType = sdd.OrderType,
						OrderNbr = sdd.OrderNbr,
						DiscountID = sdd.DiscountID,
						DiscountSequenceID = sdd.DiscountSequenceID,
						FreeItemID = sdd.FreeItemID,
						FreeItemQty = sdd.FreeItemQty
					};

					ARDiscountEngine.InsertDiscountDetail(ARDiscountDetails.Cache, ARDiscountDetails, idd);
				}
			}

			#endregion
		}

		/// <summary>
		/// Resets some field values in resulting Invoice in case several shipments are combined into one invoice
		/// </summary>
		/// <param name="args">InvoiceOrderArgs contains original SOOrder, SOLines, SOShipLines, SOOrderShipment, etc. that were passed to the main InvoiceOrder method</param>
		public virtual void UpdateInvoiceIfItContainsSeveralShipments(ARInvoice newdoc, InvoiceOrderArgs args, PXResultset<SOOrderShipment> invoiceOrderShipments)
		{
			List<string> ordersdistinct = new List<string>();
			foreach (SOOrderShipment shipment in invoiceOrderShipments)
			{
				string key = string.Format("{0}|{1}", shipment.OrderType, shipment.OrderNbr);
				if (!ordersdistinct.Contains(key))
				{
					ordersdistinct.Add(key);
				}

				if (args.List != null && ordersdistinct.Count > 1)
				{
					newdoc.InvoiceNbr = null;
					newdoc.SalesPersonID = null;
					newdoc.DocDesc = null;
					break;
				}
			}
		}

		protected virtual (Type itemType, string fieldName, PXFieldDefaulting handler) CancelDefaulting<TField>()
			where TField : class, IBqlField
		{
			var field = typeof(TField);
			var itemType = BqlCommand.GetItemType(field);
			string fieldName = field.Name;

			var cancelDefaulting = new PXFieldDefaulting((cache, e) =>
			{
				e.NewValue = cache.GetValue(e.Row, fieldName);
				e.Cancel = true;
			});

			FieldDefaulting.AddHandler(itemType, fieldName, cancelDefaulting);

			return (itemType, fieldName, cancelDefaulting);
		}

		protected virtual void ValidateLinesAdded(bool lineAdded)
		{
			if (!lineAdded)
				throw new PXInvalidOperationException(ErrorMessages.ElementDoesntExist, Transactions.Cache.DisplayName);
		}

		private void UpdateDiscountDetail(ARInvoiceDiscountDetail dd, SOOrderDiscountDetail docGroupDisc,  decimal? rate)
		{
			ARDiscountDetails.Cache.SetValueExt<ARInvoiceDiscountDetail.discountAmt>(dd, docGroupDisc.DiscountAmt * rate);
			ARDiscountDetails.Cache.SetValueExt<ARInvoiceDiscountDetail.curyDiscountAmt>(dd, docGroupDisc.CuryDiscountAmt * rate);
			ARDiscountDetails.Cache.SetValueExt<ARInvoiceDiscountDetail.discountableAmt>(dd, docGroupDisc.DiscountableAmt * rate);
			ARDiscountDetails.Cache.SetValueExt<ARInvoiceDiscountDetail.curyDiscountableAmt>(dd, docGroupDisc.CuryDiscountableAmt * rate);
			ARDiscountDetails.Cache.SetValueExt<ARInvoiceDiscountDetail.discountableQty>(dd, docGroupDisc.DiscountableQty * rate);
		}

		protected virtual void ResortTransactions(HashSet<ARTran> set, bool fullResort)
		{
			List<Tuple<string, ARTran>> linesToResort = new List<Tuple<string, ARTran>>();
			int lastSortOrderNbr = 0;
			foreach (PXResult<ARTran> res in Transactions.Select())
			{
				ARTran tran = res;
				if (fullResort || set.Contains(tran))
				{
					string sortkey = string.Format("{0}.{1}.{2:D7}.{3}", tran.SOOrderType, tran.SOOrderNbr, tran.SOOrderSortOrder, tran.SOShipmentNbr);
					linesToResort.Add(new Tuple<string, ARTran>(sortkey, tran));
				}
				else
				{
					lastSortOrderNbr = Math.Max(lastSortOrderNbr, tran.SortOrder.GetValueOrDefault());
				}
			}

			linesToResort.Sort((x, y) => x.Item1.CompareTo(y.Item1));

			for (int i = 0; i < linesToResort.Count; i++)
			{
				lastSortOrderNbr++;
				if (linesToResort[i].Item2.SortOrder != lastSortOrderNbr)
				{
					linesToResort[i].Item2.SortOrder = lastSortOrderNbr;
					Transactions.Cache.MarkUpdated(linesToResort[i].Item2, assertError: true);
				}
			}

			Transactions.Cache.ClearQueryCache();
		}

		public virtual void InsertApplications(SOOrderShipment orderShipment)
		{
			// this method works both for Invoices (ARAdjust2) and Credit Memos (ARAdjust)
			bool isCreditMemo = Document.Current?.DocType == ARDocType.CreditMemo;
			PXCache adjCache = isCreditMemo ? Adjustments_1.Cache : Adjustments.Cache;
			decimal? CuryApplAmt = 0m;
			bool Calculated = false;

			foreach (SOAdjust soadj in PXSelectJoin<SOAdjust,
				InnerJoin<ARPayment, On<ARPayment.docType, Equal<SOAdjust.adjgDocType>,
					And<ARPayment.refNbr, Equal<SOAdjust.adjgRefNbr>>>>,
				Where<SOAdjust.adjdOrderType, Equal<Required<SOAdjust.adjdOrderType>>,
					And<SOAdjust.adjdOrderNbr, Equal<Required<SOAdjust.adjdOrderNbr>>,
					And<ARPayment.openDoc, Equal<True>>>>>
				.Select(this, orderShipment.OrderType, orderShipment.OrderNbr))
			{
				ARAdjust prevAdj = null;
				List<ARAdjust> resultset = null;

				try
				{
					TransferApplicationFromSalesOrder = true;
					resultset = isCreditMemo
						? Adjustments_1.Select().RowCast<ARAdjust>().ToList()
						: Adjustments.Select().RowCast<ARAdjust2>().Cast<ARAdjust>().ToList();
				}
				finally
				{
					TransferApplicationFromSalesOrder = false;
				}

				foreach (ARAdjust adj in resultset)
				{
					if (Calculated)
					{
						CuryApplAmt -= adj.CuryAdjdAmt;
					}

					if (string.Equals(adj.AdjgDocType, soadj.AdjgDocType) && string.Equals(adj.AdjgRefNbr, soadj.AdjgRefNbr))
					{
						if (soadj.CuryAdjdAmt > 0m)
						{
							ARAdjust copy = (ARAdjust)adjCache.CreateCopy(adj);
							copy.CuryAdjdAmt += (soadj.CuryAdjdAmt > adj.CuryDocBal) ? adj.CuryDocBal : soadj.CuryAdjdAmt;
							copy.CuryAdjdOrigAmt = copy.CuryAdjdAmt;
							copy.AdjdOrderType = soadj.AdjdOrderType;
							copy.AdjdOrderNbr = soadj.AdjdOrderNbr;
							prevAdj = (ARAdjust)adjCache.Update(copy);
						}

						if (Calculated)
						{
							CuryApplAmt += adj.CuryAdjdAmt;
							break;
						}
					}

					CuryApplAmt += adj.CuryAdjdAmt;
				}

				Calculated = true;

				if (prevAdj != null)
				{
					prevAdj = (ARAdjust)adjCache.CreateCopy(prevAdj);

					decimal curyDocBalance = (Document.Current.CuryDocBal ?? 0m);
					decimal curyApplDifference = (CuryApplAmt ?? 0m) - curyDocBalance;

					if (CuryApplAmt > curyDocBalance)
					{
						if (prevAdj.CuryAdjdAmt > curyApplDifference)
						{
							prevAdj.CuryAdjdAmt -= curyApplDifference;
							CuryApplAmt = curyDocBalance;
						}
						else
						{
							CuryApplAmt -= prevAdj.CuryAdjdAmt;
							prevAdj.CuryAdjdAmt = 0m;
						}
						prevAdj = (ARAdjust)adjCache.Update(prevAdj);
					}
				}
			}
		}

		protected virtual void DefaultShippingAddress(ARInvoice newdoc, SOOrderShipment orderShipment, SOShipment soShipment)
		{
			var soShipAddress = SOAddress.PK.Find(this, orderShipment.ShipAddressID);
			if (!ExternalTax.IsExternalTax(this, newdoc.TaxZoneID)
				|| !ExternalTax.IsEmptyAddress(soShipAddress))
			{
				ARShippingAddressAttribute.CopyRecord<ARInvoice.shipAddressID>(this.Document.Cache, newdoc, soShipAddress, true);
				if (soShipAddress?.IsValidated == true && Shipping_Address.Current != null)
					Shipping_Address.Current.IsValidated = true;
				return;
			}

			if (soShipment?.WillCall == true)
			{
				var site = INSite.PK.Find(this, soShipment.SiteID);
				var shipToAddress = Address.PK.Find(this, site.AddressID);
				if (!ExternalTax.IsEmptyAddress(shipToAddress))
				{
					ARShippingAddressAttribute.DefaultAddress<ARShippingAddress, ARShippingAddress.addressID>(
						this.Document.Cache,
						nameof(ARInvoice.shipAddressID),
						newdoc,
						null,
						new PXResult<Address, ARShippingAddress>(shipToAddress, new ARShippingAddress()));
					return;
				}
			}

			var order = SOOrderShipment.FK.Order.FindParent(this, orderShipment);
			var soOrderAddress = SOAddress.PK.Find(this, order?.ShipAddressID);
			if (!ExternalTax.IsEmptyAddress(soOrderAddress))
			{
				ARShippingAddressAttribute.CopyRecord<ARInvoice.shipAddressID>(this.Document.Cache, newdoc, soOrderAddress, true);
				if (soOrderAddress?.IsValidated == true && Shipping_Address.Current != null)
					Shipping_Address.Current.IsValidated = true;
			}
		}

		public virtual bool IsFullOrderInvoicing(SOOrder soOrder, SOOrderType soOrderType, PXSelectBase<SOLine> transactions)
		{
			bool fullOrderInvoicing = false;
			if (PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>() && soOrderType.RequireShipping == true && soOrder.OpenLineCntr == 0)
			{
				if (transactions.Select(soOrder.OrderType, soOrder.OrderNbr).AsEnumerable().RowCast<SOLine>()
					.All(l => l.ShippedQty == l.OrderQty || l.LineType == SOLineType.MiscCharge))
				{
					SOOrderShipment notInvoicedOrderShipment = PXSelect<SOOrderShipment,
						Where<SOOrderShipment.orderType, Equal<Required<SOOrder.orderType>>, And<SOOrderShipment.orderNbr, Equal<Required<SOOrder.orderNbr>>,
							And<Where<SOOrderShipment.invoiceNbr, IsNull,
								Or<SOOrderShipment.invoiceType, NotEqual<Current<ARInvoice.docType>>, Or<SOOrderShipment.invoiceNbr, NotEqual<Current<ARInvoice.refNbr>>>>>>>>>
						.SelectWindowed(this, 0, 1, soOrder.OrderType, soOrder.OrderNbr);
					fullOrderInvoicing = (notInvoicedOrderShipment == null);
				}
			}
			return fullOrderInvoicing;
		}

        /// <summary>
        /// Automatically writes-off the difference between original Amount Paid in Sales Order and Amount Paid in SO Invoice
        /// </summary>
        /// <param name="customer"></param>
		protected virtual void AutoWriteOffBalance(Customer customer)
		{
			foreach (ARAdjust2 adjustment in Adjustments_Inv.Select())
			{
				decimal applDifference = (adjustment.CuryAdjdAmt ?? 0m) - (adjustment.CuryAdjdOrigAmt ?? 0m);
				if (customer != null && customer.SmallBalanceAllow == true && applDifference != 0m && Math.Abs(customer.SmallBalanceLimit ?? 0m) >= Math.Abs(applDifference))
				{
					ARAdjust2 upd_adj = PXCache<ARAdjust2>.CreateCopy(adjustment);
					upd_adj.CuryAdjdAmt = upd_adj.CuryAdjdOrigAmt;
					upd_adj.CuryAdjdWOAmt = applDifference;
					upd_adj = Adjustments.Update(upd_adj);
				}
			}

			if (this.Document.Current.CuryApplicationBalance != 0m)
			{
				ARAdjust2 firstAdjustment = Adjustments_Inv.SelectSingle();
				if (firstAdjustment != null)
				{
					ARAdjust2 upd_adj = PXCache<ARAdjust2>.CreateCopy(firstAdjustment);

					decimal applDifference = this.Document.Current.CuryApplicationBalance ?? 0m;

					if (customer != null && customer.SmallBalanceAllow == true && Math.Abs(customer.SmallBalanceLimit ?? 0m) >= Math.Abs(applDifference))
					{
						upd_adj.CuryAdjdWOAmt = -applDifference;
						upd_adj = Adjustments.Update(upd_adj);
					}
				}
			}
		}

		public virtual void AddOrderTaxes(SOOrderShipment orderShipment)
		{
			if (Document.Current == null || (IsExternalTax(Document.Current.TaxZoneID) && Document.Current.DisableAutomaticTaxCalculation != true))
				return;

			if (Document.Current.DisableAutomaticTaxCalculation == true)
			{
				SOOrderShipment allocated = PXSelect<SOOrderShipment,
					Where<SOOrderShipment.orderType, Equal<Current<SOOrderShipment.orderType>>,
						And<SOOrderShipment.orderNbr, Equal<Current<SOOrderShipment.orderNbr>>,
						And<SOOrderShipment.invoiceNbr, IsNotNull,
						And<SOOrderShipment.orderTaxAllocated, Equal<True>>>>>>
						.SelectSingleBound(this, new object[] { orderShipment });
				if (allocated != null)
				{
					return;
				}
				orderShipment.OrderTaxAllocated = true;
				orderShipment = shipmentlist.Update(orderShipment);
			}

			// scope of the taxes recalculation is limited with the current SOOrderShipment
			// necessary to set proper current because the method SOInvoiceTaxAttribute.FilterParent depends on it
			shipmentlist.Current = orderShipment;

			List<SOTaxTran> orderTaxes = PXSelectJoin<SOTaxTran,
				InnerJoin<Tax, On<SOTaxTran.taxID, Equal<Tax.taxID>>>,
				Where<SOTaxTran.orderType, Equal<Required<SOTaxTran.orderType>>, And<SOTaxTran.orderNbr, Equal<Required<SOTaxTran.orderNbr>>>>>
				.Select(this, orderShipment.OrderType, orderShipment.OrderNbr).AsEnumerable().RowCast<SOTaxTran>().ToList();

			foreach (SOTaxTran tax in orderTaxes)
			{
				ARTaxTran newtax = new ARTaxTran { Module = BatchModule.AR };
				Taxes.Cache.SetDefaultExt<ARTaxTran.origTranType>(newtax);
				Taxes.Cache.SetDefaultExt<ARTaxTran.origRefNbr>(newtax);
				Taxes.Cache.SetDefaultExt<ARTaxTran.lineRefNbr>(newtax);
				newtax.TranType = Document.Current.DocType;
				newtax.RefNbr = Document.Current.RefNbr;
				newtax.TaxID = tax.TaxID;
				newtax.TaxRate = 0m;
				newtax.CuryID = Document.Current.CuryID;

				if (Document.Current?.DisableAutomaticTaxCalculation == true)
				{
					newtax.CuryTaxableAmt = tax.CuryTaxableAmt;
					newtax.CuryTaxAmt = tax.CuryTaxAmt;
					newtax.TaxRate = tax.TaxRate;
					newtax.JurisName = tax.JurisName;
					newtax.JurisType = tax.JurisType;
					newtax.TaxBucketID = 0;
				}

				this.Taxes.Select().Consume();

				bool insertNewTax = true;
				if (Document.Current?.DisableAutomaticTaxCalculation == true)
				{
					foreach (ARTaxTran existingTaxTran in this.Taxes.Cache.Cached.RowCast<ARTaxTran>().Where(a =>
						!this.Taxes.Cache.GetStatus(a).IsIn(PXEntryStatus.Deleted, PXEntryStatus.InsertedDeleted)
						&& this.Taxes.Cache.ObjectsEqual<ARTaxTran.module, ARTaxTran.refNbr, ARTaxTran.tranType, ARTaxTran.taxID, ARTaxTran.jurisType, ARTaxTran.jurisName>(newtax, a)))
					{
						ARTaxTran updatedTaxTran = (ARTaxTran)this.Taxes.Cache.CreateCopy(existingTaxTran);
						updatedTaxTran.CuryTaxableAmt += newtax.CuryTaxableAmt;
						updatedTaxTran.CuryTaxAmt += newtax.CuryTaxAmt;
						this.Taxes.Update(updatedTaxTran);
						insertNewTax = false;
						break;
					}
				}
				else
				{
					foreach (ARTaxTran existingTaxTran in this.Taxes.Cache.Cached.RowCast<ARTaxTran>().Where(a =>
						!this.Taxes.Cache.GetStatus(a).IsIn(PXEntryStatus.Deleted, PXEntryStatus.InsertedDeleted)
						&& this.Taxes.Cache.ObjectsEqual<ARTaxTran.module, ARTaxTran.refNbr, ARTaxTran.tranType, ARTaxTran.taxID>(newtax, a)))
					{
						this.Taxes.Delete(existingTaxTran);
					}
				}

				if (insertNewTax)
					newtax = this.Taxes.Insert(newtax);
			}

			if (Document.Current?.DisableAutomaticTaxCalculation == true)
			{
				var order = (SOOrder)this.Caches<SOOrder>().Locate(new SOOrder
				{
					OrderType = orderShipment.OrderType,
					OrderNbr = orderShipment.OrderNbr });

				SOOrder copy = PXCache<SOOrder>.CreateCopy(order);

				foreach (SOTaxTran tax in orderTaxes)
				{
					tax.CuryUnbilledTaxableAmt = 0m;
					tax.CuryUnbilledTaxAmt = 0m;
					tax.CuryUnshippedTaxableAmt = 0m;
					tax.CuryUnshippedTaxAmt = 0m;

					this.Caches<SOTaxTran>().Update(tax);
				}

				order.CuryOpenTaxTotal = 0m;
				order.CuryUnbilledTaxTotal = 0m;
				order.CuryOpenOrderTotal -= copy.CuryOpenTaxTotal;
				order.CuryUnbilledOrderTotal -= copy.CuryUnbilledTaxTotal;

				this.Caches<SOOrder>().Update(order);
			}
		}

		public virtual DateTime GetOrderInvoiceDate(DateTime invoiceDate, SOOrder soOrder, SOOrderShipment orderShipment)
		{
			return (sosetup.Current.UseShipDateForInvoiceDate == true && soOrder.InvoiceDate == null ? orderShipment.ShipDate : soOrder.InvoiceDate) ?? invoiceDate;
		}

		public virtual bool IsCreditCardProcessing(SOOrder soOrder)
		{
			return PXSelectReadonly<ExternalTransaction,
				Where<ExternalTransaction.origDocType, Equal<Required<ExternalTransaction.origDocType>>, And<ExternalTransaction.origRefNbr, Equal<Required<ExternalTransaction.origRefNbr>>,
					And<ExternalTransaction.refNbr, IsNull>>>>
				.SelectWindowed(this, 0, 1, soOrder.OrderType, soOrder.OrderNbr).Count > 0;
		}

		public virtual ARInvoice FindOrCreateInvoice(DateTime orderInvoiceDate, InvoiceOrderArgs args)
		{
			SOOrderShipment orderShipment = args.OrderShipment;
			SOOrder soOrder = args.SOOrder;
			SOOrderType soOrderType = SOOrderType.PK.Find(this, soOrder.OrderType);
			string invoiceDocType = GetInvoiceDocType(soOrderType, soOrder, args.GroupByDefaultOperation ? soOrderType.DefaultOperation : orderShipment.Operation);
			string orderTermsID = invoiceDocType == ARDocType.CreditMemo ? null : soOrder.TermsID;

			if (orderShipment.BillShipmentSeparately == true)
			{
				ARInvoice newdoc = args.List.Find(
					new FieldLookup<ARInvoice.hidden>(false),
					new FieldLookup<ARInvoice.hiddenByShipment>(true),
					new FieldLookup<ARInvoice.hiddenShipmentType>(orderShipment.ShipmentType),
					new FieldLookup<ARInvoice.hiddenShipmentNbr>(orderShipment.ShipmentNbr),
					new FieldLookup<ARInvoice.taxCalcMode>(soOrder.TaxCalcMode));
				return newdoc ?? new ARInvoice()
				{
					HiddenShipmentType = orderShipment.ShipmentType,
					HiddenShipmentNbr = orderShipment.ShipmentNbr,
					HiddenByShipment = true
				};
			}
			else if (soOrder.PaymentCntr != 0 || soOrder.BillSeparately == true || IsCreditCardProcessing(soOrder))
			{
				ARInvoice newdoc = args.List.Find(
					new FieldLookup<ARInvoice.hidden>(true),
					new FieldLookup<ARInvoice.hiddenByShipment>(false),
					new FieldLookup<ARInvoice.hiddenOrderType>(soOrder.OrderType),
					new FieldLookup<ARInvoice.hiddenOrderNbr>(soOrder.OrderNbr));
				return newdoc ?? new ARInvoice()
				{
					HiddenOrderType = soOrder.OrderType,
					HiddenOrderNbr = soOrder.OrderNbr,
					Hidden = true
				};
			}
			else
			{
				var invoiceSearchValues = new List<FieldLookup>()
				{
					new FieldLookup<ARInvoice.hidden>(false),
					new FieldLookup<ARInvoice.hiddenByShipment>(false),
					new FieldLookup<ARInvoice.docType>(invoiceDocType),
					new FieldLookup<ARInvoice.docDate>(orderInvoiceDate),
					new FieldLookup<ARInvoice.branchID>(soOrder.BranchID),
					new FieldLookup<ARInvoice.customerID>(soOrder.CustomerID),
					new FieldLookup<ARInvoice.customerLocationID>(soOrder.CustomerLocationID),
					new FieldLookup<ARInvoice.taxZoneID>(soOrder.TaxZoneID),
					new FieldLookup<ARInvoice.taxCalcMode>(soOrder.TaxCalcMode),
					new FieldLookup<ARInvoice.curyID>(soOrder.CuryID),
					new FieldLookup<ARInvoice.termsID>(orderTermsID),
					new FieldLookup<SOInvoice.billAddressID>(soOrder.BillAddressID),
					new FieldLookup<SOInvoice.billContactID>(soOrder.BillContactID),
					new FieldLookup<ARInvoice.disableAutomaticTaxCalculation>(soOrder.DisableAutomaticTaxCalculation)
				};

				if (args.GroupByCustomerOrderNumber)
					invoiceSearchValues.Add(new FieldLookup<ARInvoice.invoiceNbr>(soOrder.CustomerOrderNbr));

				invoiceSearchValues.Add(new FieldLookup<SOInvoice.extRefNbr>(soOrder.ExtRefNbr));
					invoiceSearchValues.Add(new FieldLookup<SOInvoice.pMInstanceID>(soOrder.PMInstanceID));
				if (soOrder.CashAccountID != null)
				{
					invoiceSearchValues.Add(new FieldLookup<SOInvoice.cashAccountID>(soOrder.CashAccountID));
				}

				CurrencyInfo orderCuryInfo = args.SoCuryInfo;
				invoiceSearchValues.Add(new FieldLookup<CurrencyInfo.curyRateTypeID>(orderCuryInfo.CuryRateTypeID));
				if (soOrderType.UseCuryRateFromSO == true)
				{
					invoiceSearchValues.Add(new FieldLookup<CurrencyInfo.curyMultDiv>(orderCuryInfo.CuryMultDiv));
					invoiceSearchValues.Add(new FieldLookup<CurrencyInfo.curyRate>(orderCuryInfo.CuryRate));
				}

				return args.List.Find(invoiceSearchValues.ToArray()) ?? new ARInvoice();
			}
		}

		public virtual ARTran CreateTranFromMiscLine(SOOrderShipment orderShipment, SOMiscLine2 orderline)
		{
			ARTran newtran = new ARTran();
			newtran.BranchID = orderline.BranchID;
			newtran.AccountID = orderline.SalesAcctID;
			newtran.SubID = orderline.SalesSubID;
			newtran.SOOrderType = orderline.OrderType;
			newtran.SOOrderNbr = orderline.OrderNbr;
			newtran.SOOrderLineNbr = orderline.LineNbr;
			newtran.SOOrderLineOperation = orderline.Operation;
			newtran.SOOrderSortOrder = orderline.SortOrder;
			newtran.SOOrderLineSign = orderline.LineSign;
			newtran.SOShipmentNbr = orderShipment.ShipmentNbr;
			newtran.SOShipmentLineNbr = null;
			newtran.SOShipmentType = orderShipment.ShipmentType;

			newtran.LineType = SOLineType.MiscCharge;
			newtran.InventoryID = orderline.InventoryID;
			newtran.SiteID = orderline.SiteID;
			newtran.ProjectID = orderline.ProjectID;
			newtran.TaskID = orderline.TaskID;
			newtran.SalesPersonID = orderline.SalesPersonID;
			newtran.Commissionable = orderline.Commissionable;
			newtran.UOM = orderline.UOM;
			newtran.Qty = orderline.UnbilledQty;
			newtran.BaseQty = orderline.BaseUnbilledQty;
			newtran.CuryUnitPrice = orderline.CuryUnitPrice;
			newtran.CuryExtPrice = orderline.CuryExtPrice;
			newtran.CuryDiscAmt = orderline.CuryDiscAmt;
			newtran.CuryTranAmt = orderline.CuryUnbilledAmt;
			newtran.TranDesc = orderline.TranDesc;
			newtran.TaxCategoryID = orderline.TaxCategoryID;
			newtran.DiscPct = orderline.DiscPct;

			newtran.IsFree = orderline.IsFree;
			newtran.ManualPrice = true;
			newtran.ManualDisc = orderline.ManualDisc == true || orderline.IsFree == true;
			newtran.FreezeManualDisc = true;

			newtran.DiscountID = orderline.DiscountID;
			newtran.DiscountSequenceID = orderline.DiscountSequenceID;

			newtran.DRTermStartDate = orderline.DRTermStartDate;
			newtran.DRTermEndDate = orderline.DRTermEndDate;
			newtran.CuryUnitPriceDR = orderline.CuryUnitPriceDR;
			newtran.DiscPctDR = orderline.DiscPctDR;
			newtran.DefScheduleID = orderline.DefScheduleID;
			newtran.SortOrder = orderline.SortOrder;
			newtran.OrigInvoiceType = orderline.InvoiceType;
			newtran.OrigInvoiceNbr = orderline.InvoiceNbr;
			newtran.OrigInvoiceLineNbr = orderline.InvoiceLineNbr;
			newtran.OrigInvoiceDate = orderline.InvoiceDate;
			newtran.CostCodeID = orderline.CostCodeID;

			newtran.BlanketType = orderline.BlanketType;
			newtran.BlanketNbr = orderline.BlanketNbr;
			newtran.BlanketLineNbr = orderline.BlanketLineNbr;
			newtran.BlanketSplitLineNbr = orderline.BlanketSplitLineNbr;

			return newtran;
		}

		public virtual ARTran CreateTranFromShipLine(ARInvoice newdoc, SOOrderType ordertype, string operation, SOLine orderline, ref SOShipLine shipline)
		{
			ARTran newtran = new ARTran();
			newtran.SOOrderType = shipline.OrigOrderType;
			newtran.SOOrderNbr = shipline.OrigOrderNbr;
			newtran.SOOrderLineNbr = shipline.OrigLineNbr;
			newtran.SOShipmentNbr = shipline.ShipmentNbr;
			newtran.SOShipmentType = shipline.ShipmentType;
			newtran.SOShipmentLineNbr = shipline.LineNbr;
			newtran.RequireINUpdate = shipline.RequireINUpdate;

			newtran.LineType = orderline.LineType;
			newtran.InventoryID = shipline.InventoryID;
			newtran.SiteID = orderline.SiteID;
			newtran.UOM = shipline.UOM;
			newtran.SubItemID = shipline.SubItemID;
			newtran.LocationID = (shipline.ShipmentType == SOShipmentType.DropShip || shipline.ShipmentNbr == Constants.NoShipmentNbr) ? orderline.LocationID : shipline.LocationID;
			newtran.LotSerialNbr = shipline.LotSerialNbr;
			newtran.ExpireDate = shipline.ExpireDate;
			newtran.CostCodeID = shipline.CostCodeID;

			bool useLineDiscPct;
			CopyTranFieldsFromSOLine(newtran, ordertype, orderline, out useLineDiscPct);

			if (IsShiplineMergeAllowed(shipline))
			{
				bool isMixedUOM = false;
				foreach (SOShipLine other in this.Caches[typeof(SOShipLine)].Cached)
				{
					if (this.Caches[typeof(SOShipLine)].ObjectsEqual<SOShipLine.shipmentNbr, SOShipLine.shipmentType, SOShipLine.origOrderType, SOShipLine.origOrderNbr, SOShipLine.origLineNbr>(shipline, other) && shipline.LineNbr != other.LineNbr)
					{
						shipline = PXCache<SOShipLine>.CreateCopy(shipline);
						shipline.ShippedQty += other.ShippedQty;
						shipline.BaseShippedQty += other.BaseShippedQty;

						isMixedUOM |= other.UOM != newtran.UOM;
						newtran.SOShipmentLineNbr = null;
						if (!string.Equals(newtran.LotSerialNbr, other.LotSerialNbr, StringComparison.OrdinalIgnoreCase))
						{
							newtran.LotSerialNbr = null;
							newtran.ExpireDate = null;
						}
					}
				}
				if (isMixedUOM)
					PXDBQuantityAttribute.CalcTranQty<SOShipLine.shippedQty>(this.Caches[typeof(SOShipLine)], shipline);
			}

			newtran.Qty = shipline.ShippedQty;
			newtran.BaseQty = shipline.BaseShippedQty;

			bool shippedInDifferentUOM = shipline.UOM != orderline.UOM;

			decimal shippedQtyInOrderUnits = 0m;
			if (shippedInDifferentUOM)
			{
				try
				{
					decimal shippedQtyInBaseUnits = INUnitAttribute.ConvertToBase(Transactions.Cache, newtran.InventoryID, shipline.UOM, shipline.ShippedQty.Value, INPrecision.QUANTITY);
					shippedQtyInOrderUnits = INUnitAttribute.ConvertFromBase(Transactions.Cache, newtran.InventoryID, orderline.UOM, shippedQtyInBaseUnits, INPrecision.QUANTITY);
				}
				catch (PXSetPropertyException e)
				{
					throw new Common.Exceptions.ErrorProcessingEntityException(Caches[orderline.GetType()], orderline, e);
				}
			}
			else
			{
				shippedQtyInOrderUnits = shipline.ShippedQty.Value;
			}

			MultiCurrency multiCurrencyExt = GetExtension<MultiCurrency>();

			if (shippedQtyInOrderUnits != orderline.OrderQty || shippedInDifferentUOM)
				{
				//no need to prorate Ext. Price and recalculate Unit Price in case Qty * Unit Price = Ext. Price on SOLine
				bool isOrderUnitPriceRecalculationNeeded =
					multiCurrencyExt.GetCurrencyInfo(orderline.CuryInfoID).RoundCury((orderline.OrderQty ?? 0m) * (orderline.CuryUnitPrice ?? 0m)) -
					(orderline.CuryExtPrice ?? 0m) != 0m;

				//ARTran.CuryUnitPrice
				decimal curyUnitPriceInOrderUnits = 0m;
				if (isOrderUnitPriceRecalculationNeeded)
				{
					if (orderline.CuryExtPrice != 0 && orderline.OrderQty != 0)
					curyUnitPriceInOrderUnits = PXPriceCostAttribute.Round(orderline.CuryExtPrice.Value / orderline.OrderQty.Value);
				}
				else
				{
					curyUnitPriceInOrderUnits = orderline.CuryUnitPrice.Value;
				}

				decimal curyUnitPriceInShippedUnits = 0m;
				if (shippedInDifferentUOM)
				{
				decimal curyUnitPriceInBaseUnits = INUnitAttribute.ConvertFromBase(Transactions.Cache, newtran.InventoryID, orderline.UOM, curyUnitPriceInOrderUnits, INPrecision.UNITCOST);
					curyUnitPriceInShippedUnits = INUnitAttribute.ConvertToBase(Transactions.Cache, newtran.InventoryID, shipline.UOM, curyUnitPriceInBaseUnits, INPrecision.UNITCOST);
				}
				else
				{
					curyUnitPriceInShippedUnits = curyUnitPriceInOrderUnits;
				}
				if (orderline.CuryUnitPrice != 0)
					newtran.CuryUnitPrice = curyUnitPriceInShippedUnits;

				//ARTran.CuryExtPrice
				if (shippedQtyInOrderUnits != orderline.OrderQty && orderline.OrderQty != 0m)
				{
					decimal? curyExtPrice = isOrderUnitPriceRecalculationNeeded ?
						orderline.CuryExtPrice * shippedQtyInOrderUnits / orderline.OrderQty :
						shippedQtyInOrderUnits * orderline.CuryUnitPrice;

					newtran.CuryExtPrice = multiCurrencyExt.GetDefaultCurrencyInfo().RoundCury(curyExtPrice ?? 0m);
				}
				else
				{
					newtran.CuryExtPrice = orderline.CuryExtPrice;
				}

				//all the difference (including accumulated rounding difference) goes to discount amount
				if (orderline.DiscPct != 0 || orderline.CuryDiscAmt != 0)
				{
				decimal? salesPriceAfterDiscount = curyUnitPriceInShippedUnits * (useLineDiscPct ? (1m - orderline.DiscPct / 100m) : 1m);
				if (arsetup.Current.LineDiscountTarget == LineDiscountTargetType.SalesPrice)
				{
					salesPriceAfterDiscount = PXPriceCostAttribute.Round(salesPriceAfterDiscount ?? 0m);
				}

				decimal? curyTranAmt = shipline.ShippedQty * salesPriceAfterDiscount;
				newtran.CuryTranAmt = multiCurrencyExt.GetDefaultCurrencyInfo().RoundCury(curyTranAmt ?? 0m);

					newtran.CuryDiscAmt = newtran.CuryExtPrice - newtran.CuryTranAmt;
				}
				else
				{
					newtran.CuryTranAmt = newtran.CuryExtPrice;
					newtran.CuryDiscAmt = 0m;
			}
			}
			else
			{
				newtran.CuryUnitPrice = orderline.CuryUnitPrice;
				newtran.CuryExtPrice = orderline.CuryExtPrice;
				newtran.CuryTranAmt = orderline.CuryLineAmt;
				newtran.CuryDiscAmt = orderline.CuryDiscAmt;
			}

			ChangeBalanceSign(newtran, newdoc, orderline.Operation);

			return newtran;
		}

		protected virtual bool IsShiplineMergeAllowed(SOShipLine shipline)
		{
			SOShipment shipment = GetShipment(shipline.ShipmentType, shipline.ShipmentNbr);
			return shipment?.IsIntercompany != true;
		}

		protected virtual void ChangeBalanceSign(ARTran tran, ARInvoice newdoc, string soLineOperation)
		{
			if (newdoc.DrCr == DrCr.Credit && soLineOperation == SOOperation.Receipt
				|| newdoc.DrCr == DrCr.Debit && soLineOperation == SOOperation.Issue)
			{
				//keep BaseQty positive for PXFormula
				tran.Qty = -tran.Qty;
				tran.CuryDiscAmt = -tran.CuryDiscAmt;
				tran.CuryTranAmt = -tran.CuryTranAmt;
				tran.CuryExtPrice = -tran.CuryExtPrice;
			}
		}

		protected virtual void CopyTranFieldsFromOrigTran(ARTran newtran, ARTran origTran)
		{
			newtran.IsFree = origTran.IsFree;
			newtran.ManualPrice = true;
			newtran.ManualDisc = (origTran.ManualDisc == true || origTran.IsFree == true);
			if (origTran.ManualDisc == true)
			{
				newtran.DiscPct = origTran.DiscPct;
			}
			newtran.AvalaraCustomerUsageType = origTran.AvalaraCustomerUsageType;
			newtran.TaxCategoryID = origTran.TaxCategoryID;
		}

		protected virtual void CopyTranFieldsFromSOLine(ARTran newtran, SOOrderType ordertype, SOLine orderline, out bool useLineDiscPct)
		{
			useLineDiscPct = ordertype?.RecalculateDiscOnPartialShipment != true || orderline.ManualDisc == true || arsetup.Current.LineDiscountTarget == LineDiscountTargetType.SalesPrice;
			newtran.BranchID = orderline.BranchID;
			newtran.AccountID = orderline.SalesAcctID;
			newtran.SubID = orderline.SalesSubID;
			newtran.ReasonCode = orderline.ReasonCode;

			newtran.DRTermStartDate = orderline.DRTermStartDate;
			newtran.DRTermEndDate = orderline.DRTermEndDate;
			newtran.CuryUnitPriceDR = orderline.CuryUnitPriceDR;
			newtran.DiscPctDR = orderline.DiscPctDR;
			newtran.DefScheduleID = orderline.DefScheduleID;

			newtran.Commissionable = orderline.Commissionable;

			newtran.ProjectID = orderline.ProjectID;
			newtran.TaskID = orderline.TaskID;
			newtran.CostCodeID = orderline.CostCodeID;
			newtran.TranDesc = orderline.TranDesc;
			newtran.SalesPersonID = orderline.SalesPersonID;
			newtran.TaxCategoryID = orderline.TaxCategoryID;
			newtran.DiscPct = (useLineDiscPct ? orderline.DiscPct : 0m);

			newtran.IsFree = orderline.IsFree;
			newtran.ManualPrice = true;
			newtran.ManualDisc = orderline.ManualDisc == true || orderline.IsFree == true;
			newtran.FreezeManualDisc = true;
			newtran.SkipLineDiscounts = orderline.SkipLineDiscounts;

			newtran.DiscountID = orderline.DiscountID;
			newtran.DiscountSequenceID = orderline.DiscountSequenceID;

			newtran.DisableAutomaticTaxCalculation = orderline.DisableAutomaticTaxCalculation;

			newtran.SortOrder = orderline.SortOrder;
			newtran.OrigInvoiceType = orderline.InvoiceType;
			newtran.OrigInvoiceNbr = orderline.InvoiceNbr;
			newtran.OrigInvoiceLineNbr = orderline.InvoiceLineNbr;
			newtran.OrigInvoiceDate = orderline.InvoiceDate;

			newtran.SOOrderLineOperation = orderline.Operation;
			newtran.SOOrderSortOrder = orderline.SortOrder;
			newtran.SOOrderLineSign = orderline.LineSign;
			newtran.AvalaraCustomerUsageType = orderline.AvalaraCustomerUsageType;

			newtran.BlanketType = orderline.BlanketType;
			newtran.BlanketNbr = orderline.BlanketNbr;
			newtran.BlanketLineNbr = orderline.BlanketLineNbr;
			newtran.BlanketSplitLineNbr = orderline.BlanketSplitLineNbr;
		}

		public virtual void PostInvoice(
			Services.InvoicePostingContext context,
			ARInvoice invoice,
			DocumentList<INRegister> list,
			List<SOOrderShipment> directShipmentsToCreate)
		{
			SOInvoice soInvoice = this.SODocument.Select(invoice.DocType, invoice.RefNbr);
			if (soInvoice?.CreateINDoc == true)
			{
			foreach (PXResult<SOOrderShipment, SOOrder> res in PXSelectJoin<SOOrderShipment,
				InnerJoin<SOOrder, On<SOOrder.orderType, Equal<SOOrderShipment.orderType>, And<SOOrder.orderNbr, Equal<SOOrderShipment.orderNbr>>>>,
				Where<SOOrderShipment.invoiceType, Equal<Current<ARInvoice.docType>>, And<SOOrderShipment.invoiceNbr, Equal<Current<ARInvoice.refNbr>>,
				And<SOOrderShipment.createINDoc, Equal<True>, And<SOOrderShipment.invtRefNbr, IsNull>>>>>.SelectMultiBound(this, new object[] { invoice }))
			{
				if (((SOOrderShipment)res).ShipmentType == SOShipmentType.DropShip)
				{
					context.GetClearShipmentEntryDS().PostReceipt(context.IssueEntry, res, invoice, list);
				}
				else if (string.Equals(((SOOrderShipment)res).ShipmentNbr, Constants.NoShipmentNbr))
				{
					context.GetClearOrderEntry().PostOrder(context.IssueEntry, (SOOrder)res, list, (SOOrderShipment)res);
				}
				else
				{
					context.GetClearShipmentEntry().PostShipment(context.IssueEntry, res, list, invoice);
				}
			}
			}

			PostInvoiceDirectLines(context.IssueEntry, invoice, list, directShipmentsToCreate);
		}

		public virtual ARTran InsertInvoiceDirectLine(ARTran tran)
		{
			if (Document.Current == null)
				return null;

			if (tran.SOOrderLineNbr != null)
			{
				SOLine line = SOLine.PK.Find(this, tran.SOOrderType, tran.SOOrderNbr, tran.SOOrderLineNbr);
				if (line != null)
				{
					tran.IsStockItem = tran.InventoryID != null ? tran.IsStockItem : line.IsStockItem;
					tran.InventoryID = tran.InventoryID ?? line.InventoryID;
					tran.SubItemID = tran.SubItemID ?? line.SubItemID;
					tran.SiteID = tran.SiteID ?? line.SiteID;
					tran.LocationID = tran.LocationID ?? line.LocationID;
					tran.UOM = tran.UOM ?? line.UOM;
					tran.LotSerialNbr = tran.LotSerialNbr ?? line.LotSerialNbr;
					tran.ExpireDate = tran.ExpireDate ?? line.ExpireDate;
					tran.CuryUnitPrice = tran.CuryUnitPrice ?? line.CuryUnitPrice;
					if (tran.Qty == null)
					{
						short lineSign = (short)((line.DefaultOperation == SOOperation.Receipt) ? 1 : -1);
						short? tranMult = INTranType.InvtMultFromInvoiceType(Document.Current.DocType);
						tran.Qty = lineSign * tranMult * (line.OrderQty - line.ShippedQty);
					}

					bool useLineDiscPct;
					var orderType = SOOrderType.PK.Find(this, line.OrderType);
					CopyTranFieldsFromSOLine(tran, orderType, line, out useLineDiscPct);
					tran.FreezeManualDisc = false;
				}
			}
			else if (tran.OrigInvoiceNbr != null && tran.OrigInvoiceLineNbr != null)
			{
				ARTran origTran = PXSelectReadonly<ARTran,
					Where<ARTran.tranType, Equal<Required<ARTran.tranType>>,
						And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>, And<ARTran.lineNbr, Equal<Required<ARTran.lineNbr>>>>>>
					.Select(this, tran.OrigInvoiceType, tran.OrigInvoiceNbr, tran.OrigInvoiceLineNbr);
				if (origTran != null)
				{
					tran.IsStockItem = tran.InventoryID != null ? tran.IsStockItem : origTran.IsStockItem;
					tran.InventoryID = tran.InventoryID ?? origTran.InventoryID;
					tran.SubItemID = tran.SubItemID ?? origTran.SubItemID;
					tran.SiteID = tran.SiteID ?? origTran.SiteID;
					tran.LocationID = tran.LocationID ?? origTran.LocationID;
					tran.UOM = tran.UOM ?? origTran.UOM;
					tran.LotSerialNbr = tran.LotSerialNbr ?? origTran.LotSerialNbr;
					tran.ExpireDate = tran.ExpireDate ?? origTran.ExpireDate;
					tran.CuryUnitPrice = tran.CuryUnitPrice ?? origTran.CuryUnitPrice;
					tran.AccountID = tran.AccountID ?? origTran.AccountID;
					tran.SubID = tran.SubID ?? origTran.SubID;
					if (tran.Qty == null)
					{
						short? tranMult = INTranType.InvtMultFromInvoiceType(Document.Current.DocType);
						tran.Qty = tranMult * Math.Abs(origTran.Qty ?? 0m);
					}

					CopyTranFieldsFromOrigTran(tran, origTran);
				}
			}

			if (tran.CuryUnitPrice != null)
			{
				cancelUnitPriceCalculation = true;
			}
			forceDiscountCalculation = true;

			try
			{
				//Non-stock kits cannot be added directly to invoices
				//This verification is needed because ARTran_InventoryID_FieldVerifying suppresses all verifications when ExternalCall is false (including verifications in restrictor attributes)
				InventoryItem item = IN.InventoryItem.PK.Find(this, tran.InventoryID);
				if (tran.SOOrderNbr == null && item?.StkItem != true && item?.KitItem == true)
					throw new PXException(Messages.CannotAddNonStockKitDirectly);

				return Transactions.Insert(tran);
			}
			finally
			{
				cancelUnitPriceCalculation = false;
				forceDiscountCalculation = false;
			}
		}

		protected virtual void PostInvoiceDirectLines(
			INIssueEntry docgraph,
			ARInvoice invoice,
			DocumentList<INRegister> list,
			List<SOOrderShipment> directShipmentsToCreate)
		{
			List<PXResult<ARTran, SOLine, INItemPlan>> directLines = PXSelectJoin<ARTran,
				LeftJoin<SOLine, On<ARTran.FK.SOOrderLine>,
				LeftJoin<INItemPlan, On<INItemPlan.planID, Equal<ARTran.planID>>>>,
				Where2<ARTran.FK.Invoice.SameAsCurrent,
					And<ARTran.lineType, NotEqual<SOLineType.miscCharge>,
					And<ARTran.invtRefNbr, IsNull, And<ARTran.invtMult, NotEqual<short0>>>>>,
				OrderBy<
					Desc<ARTran.sOOrderType, Asc<ARTran.sOOrderNbr, Asc<ARTran.sOOrderLineNbr>>>>>
				.SelectMultiBound(this, new object[] { invoice }).AsEnumerable()
				.Select(r => (PXResult<ARTran, SOLine, INItemPlan>)r).ToList();
			if (!directLines.Any())
				return;

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				Document.Current = Document.Search<ARInvoice.refNbr>(invoice.RefNbr, invoice.DocType);

				var postedARTrans = new List<ARTran>();
				var orderARTrans = new PXResultset<ARTran, SOLine>();
				var orderEntry = new Lazy<SOOrderEntry>(() => PXGraph.CreateInstance<SOOrderEntry>());
				var affectedOrderShipments = new List<SOOrderShipment>();
				for (int i = 0; i < directLines.Count; i++)
				{
					PXResult<ARTran, SOLine, INItemPlan> directLine = directLines[i];
					ARTran tran = directLine;
					INItemPlan plan = directLine;

					//avoid ReadItem()
					if (plan.PlanID != null)
					{
						Caches[typeof(INItemPlan)].SetStatus(plan, PXEntryStatus.Notchanged);
					}

					Transactions.Cache.MarkUpdated(tran, assertError: true);
					tran = (ARTran)Transactions.Cache.Locate(tran);
					tran.PlanID = null;
					Transactions.Cache.IsDirty = true;

					if (tran.Qty == decimal.Zero)
					{
						if (plan.PlanID != null)
						{
							Caches[typeof(INItemPlan)].Delete(plan);
						}
						continue;
					}

					if (tran.LineType == SOLineType.Inventory)
					{
						if (!postedARTrans.Any())
						{
							docgraph.insetup.Current.HoldEntry = false;
							docgraph.insetup.Current.RequireControlTotal = false;

							INRegister newdoc = new INRegister()
							{
								BranchID = invoice.BranchID,
								DocType = INDocType.Issue,
								TranDate = invoice.DocDate,
								OrigModule = BatchModule.SO,
								SrcDocType = invoice.DocType,
								SrcRefNbr = invoice.RefNbr,
							};

							docgraph.issue.Insert(newdoc);
						}

						INTran newline = new INTran()
						{
							BranchID = tran.BranchID,
							DocType = INDocType.Issue,
							TranType = INTranType.TranTypeFromInvoiceType(tran.TranType, tran.Qty),
							SOShipmentNbr = tran.SOShipmentNbr,
							SOShipmentType = tran.SOShipmentType,
							SOShipmentLineNbr = tran.SOShipmentLineNbr,
							SOOrderType = tran.SOOrderType,
							SOOrderNbr = tran.SOOrderNbr,
							SOOrderLineNbr = tran.SOOrderLineNbr,
							SOLineType = SOLineType.Inventory,
							ARDocType = tran.TranType,
							ARRefNbr = tran.RefNbr,
							ARLineNbr = tran.LineNbr,
							BAccountID = tran.CustomerID,
							AcctID = tran.AccountID,
							SubID = tran.SubID,
							ProjectID = tran.ProjectID,
							TaskID = tran.TaskID,
							CostCodeID = tran.CostCodeID,
							IsStockItem = tran.IsStockItem,
							InventoryID = tran.InventoryID,
							SiteID = tran.SiteID,
							Qty = 0m,
							SubItemID = tran.SubItemID,
							UOM = tran.UOM,
							UnitPrice = tran.UnitPrice,
							TranDesc = tran.TranDesc,
							ReasonCode = tran.ReasonCode,
						};
						if (tran.OrigInvoiceNbr != null && tran.InvtMult * tran.Qty > 0m)
						{
							newline.UnitCost = CalculateUnitCostForReturnDirectLine(tran);
						}
						newline.InvtMult = INTranType.InvtMult(newline.TranType);
						docgraph.CostCenterDispatcherExt?.SetCostLayerType(newline);
						newline = docgraph.LineSplittingExt.lsselect.Insert(newline);

						INTranSplit newsplit = (INTranSplit)newline;
						newsplit.SplitLineNbr = null;
						newsplit.SubItemID = tran.SubItemID;
						newsplit.LocationID = tran.LocationID;
						newsplit.LotSerialNbr = tran.LotSerialNbr;
						newsplit.ExpireDate = tran.ExpireDate;
						newsplit.UOM = tran.UOM;
						newsplit.Qty = Math.Abs(tran.Qty ?? 0m);
						newsplit.BaseQty = null;
						newsplit.PlanID = plan.PlanID;
						newsplit = docgraph.splits.Insert(newsplit);
						postedARTrans.Add(tran);
					}
					if (tran.SOOrderNbr != null)
					{
						orderARTrans.Add(directLine);
						if (i + 1 >= directLines.Count
							|| !Transactions.Cache.ObjectsEqual<ARTran.sOOrderType, ARTran.sOOrderNbr>(tran, (ARTran)directLines[i + 1]))
						{
							var orderShipment = UpdateSalesOrderInvoicedDirectly(orderEntry.Value, orderARTrans);
							if (orderShipment != null)
							{
								SOOrderShipment foundOrderShipment = directShipmentsToCreate.FirstOrDefault(s =>
									s.OrderType == orderShipment.OrderType
									&& s.OrderNbr == orderShipment.OrderNbr
									&& s.ShippingRefNoteID == orderShipment.ShippingRefNoteID);

								if (foundOrderShipment != null)
								{
									foundOrderShipment.LineCntr += orderShipment.LineCntr;
									foundOrderShipment.ShipmentQty += orderShipment.ShipmentQty;
									foundOrderShipment.LineTotal += orderShipment.LineTotal;
									foundOrderShipment.CreateINDoc |= orderShipment.CreateINDoc;
								}
								else
								{
									directShipmentsToCreate.Add(orderShipment);
									foundOrderShipment = orderShipment;
								}

								affectedOrderShipments.Add(foundOrderShipment);
							}

							orderARTrans.Clear();
						}
					}
				}

				bool updatedIN = postedARTrans.Any();
				if (updatedIN)
				{
					INRegister copy = PXCache<INRegister>.CreateCopy(docgraph.issue.Current);
					PXFormulaAttribute.CalcAggregate<INTran.qty>(docgraph.transactions.Cache, copy);
					PXFormulaAttribute.CalcAggregate<INTran.tranAmt>(docgraph.transactions.Cache, copy);
					PXFormulaAttribute.CalcAggregate<INTran.tranCost>(docgraph.transactions.Cache, copy);
					docgraph.issue.Update(copy);
				}

				try
				{
					if (updatedIN)
					{
						docgraph.Save.Press();

						foreach (ARTran tran in postedARTrans)
						{
							tran.InvtDocType = docgraph.issue.Current.DocType;
							tran.InvtRefNbr = docgraph.issue.Current.RefNbr;
						}
						foreach (SOOrderShipment orderShip in affectedOrderShipments)
						{
							orderShip.InvtDocType = docgraph.issue.Current.DocType;
							orderShip.InvtRefNbr = docgraph.issue.Current.RefNbr;
							orderShip.InvtNoteID = docgraph.issue.Current.NoteID;
						}
					}
					this.Save.Press();
				}
				catch
				{
					throw;
				}
				if (updatedIN)
				{
					list.Add(docgraph.issue.Current);
				}

				ts.Complete();
			}
		}

		protected virtual SOOrderShipment UpdateSalesOrderInvoicedDirectly(
			SOOrderEntry orderEntry,
			PXResultset<ARTran, SOLine> orderARTranSet)
		{
			var orderARTrans = orderARTranSet.Select(r => (PXResult<ARTran, SOLine>)r).ToList();
			if (orderARTrans.Any(r => ((SOLine)r).LineNbr == null))
				throw new PXException(Messages.SOLineNotFound);
			int orderCount = orderARTrans.GroupBy(r => new { ((SOLine)r).OrderType, ((SOLine)r).OrderNbr }).Count();
			if (orderCount > 1)
				throw new PXArgumentException(nameof(orderARTrans));
			else if (orderCount == 0)
				return null;

			PXCache cache = Transactions.Cache;
			SOLine first = orderARTrans.First();
			orderEntry.Clear();
			orderEntry.Document.Current = orderEntry.Document.Search<SOOrder.orderNbr>(first.OrderNbr, first.OrderType);
			orderEntry.Document.Cache.MarkUpdated(orderEntry.Document.Current, assertError: true);

			orderEntry.soordertype.Current.RequireControlTotal = false;

			decimal orderInvoicedQty = 0m;
			decimal orderInvoicedAmt = 0m;
			bool updateIN = false;
			foreach (var groupBySOLine in orderARTrans.GroupBy(r => ((SOLine)r).LineNbr))
			{
				SOLine line = groupBySOLine.First();

				IEnumerable<ARTran> trans = groupBySOLine.Select(r => (ARTran)r);
				ARTran firstTran = trans.First();
				decimal sumQty = trans.Sum(t => Math.Abs(t.Qty ?? 0m)),
					sumBaseQty = trans.Sum(t => Math.Abs(t.BaseQty ?? 0m));
				orderInvoicedQty += sumQty;
				orderInvoicedAmt += trans.Sum(t => t.TranAmt) ?? 0m;
				updateIN |= trans.Any(t => t.LineType == SOLineType.Inventory);

				foreach (ARTran tran in trans)
				{
					ItemAvailabilityExt.OrderCheck(tran);
				}
				decimal? lineBaseOrderQty = line.LineSign * line.BaseOrderQty;
				decimal? lineBaseShippedQty = line.LineSign * line.BaseShippedQty;
				bool completeLineByQty = (PXDBQuantityAttribute.Round((decimal)(lineBaseOrderQty * line.CompleteQtyMin / 100m - lineBaseShippedQty - sumBaseQty)) <= 0m);
				if (line.ShipComplete == SOShipComplete.ShipComplete && !completeLineByQty)
				{
					throw new PXException(Messages.CannotShipComplete_Line, cache.GetValueExt<ARTran.inventoryID>(firstTran));
				}
				bool completeLine = completeLineByQty || (line.ShipComplete == SOShipComplete.CancelRemainder);
				if (PXDBQuantityAttribute.Round((decimal)(lineBaseOrderQty * line.CompleteQtyMax / 100m - lineBaseShippedQty - sumBaseQty)) < 0m)
				{
					throw new PXException(Messages.OrderCheck_QtyNegative,
						cache.GetValueExt<ARTran.inventoryID>(firstTran), cache.GetValueExt<ARTran.subItemID>(firstTran),
						cache.GetValueExt<ARTran.sOOrderType>(firstTran), cache.GetValueExt<ARTran.sOOrderNbr>(firstTran));
				}

				line = (SOLine)orderEntry.Transactions.Cache.CreateCopy(line);
				orderEntry.Transactions.Current = line;
				var splitsWithPlans = PXSelectJoin<SOLineSplit,
					InnerJoin<INItemPlan, On<INItemPlan.planID, Equal<SOLineSplit.planID>>>,
					Where<SOLineSplit.orderType, Equal<Required<SOLineSplit.orderType>>,
						And<SOLineSplit.orderNbr, Equal<Required<SOLineSplit.orderNbr>>,
						And<SOLineSplit.lineNbr, Equal<Required<SOLineSplit.lineNbr>>,
						And<SOLineSplit.completed, Equal<boolFalse>>>>>>
					.Select(orderEntry, line.OrderType, line.OrderNbr, line.LineNbr)
					.Select(r => (PXResult<SOLineSplit, INItemPlan>)r)
					.ToList();
				var splits = splitsWithPlans.Select(s => (SOLineSplit)s).ToList();
				var updatedSplits = new HashSet<int?>();
				SOLineSplit lastUpdatedSplit = null;

				foreach (ARTran tran in trans)
				{
					var splitsCopy = splits.Where(s => s.Completed != true).ToList();
					// sort SOLineSplits by their proximity to the current ARTran (by Lot/Serial Nbr and Location)
					splitsCopy.Sort((s1, s2) =>
					{
						if (!string.IsNullOrEmpty(tran.LotSerialNbr)
							&& !string.Equals(s1.LotSerialNbr, s2.LotSerialNbr, StringComparison.InvariantCultureIgnoreCase))
						{
							if (string.Equals(s1.LotSerialNbr, tran.LotSerialNbr, StringComparison.InvariantCultureIgnoreCase))
								return -1;
							else if (string.Equals(s2.LotSerialNbr, tran.LotSerialNbr, StringComparison.InvariantCultureIgnoreCase))
								return 1;
						}

						if (s1.LocationID != s2.LocationID)
						{
							if (tran.LocationID == s1.LocationID)
								return -1;
							else if (tran.LocationID == s2.LocationID)
								return 1;
						}

						return s1.SplitLineNbr.GetValueOrDefault().CompareTo(
							s2.SplitLineNbr.GetValueOrDefault());
					});
					decimal qtyToWriteOff = Math.Abs(tran.BaseQty ?? 0m);

					for (int j = 0; j < splits.Count; j++)
					{
						if (qtyToWriteOff <= 0m) break;
						SOLineSplit split = splits[j];
						bool lastSplit = (j + 1 >= splits.Count);

						decimal splitQty = (decimal)(split.BaseQty - split.BaseShippedQty);
						if (lastSplit || splitQty >= qtyToWriteOff)
						{
							split.BaseShippedQty += qtyToWriteOff;
							split.ShippedQty = INUnitAttribute.ConvertFromBase(orderEntry.splits.Cache, split.InventoryID, split.UOM, (decimal)split.BaseShippedQty, INPrecision.QUANTITY);
							qtyToWriteOff = 0m;
						}
						else
						{
							split.BaseShippedQty = split.BaseQty;
							split.ShippedQty = split.Qty;
							qtyToWriteOff -= splitQty;
							split.Completed = true;
						}
						updatedSplits.Add(split.SplitLineNbr);
						lastUpdatedSplit = split;
					}
				}

				PXRowUpdating cancelSOLineUpdatingHandler = new PXRowUpdating((sender, e) => { e.Cancel = true; });
				orderEntry.RowUpdating.AddHandler<SOLine>(cancelSOLineUpdatingHandler);
				foreach (PXResult<SOLineSplit, INItemPlan> splitWithPlan in splitsWithPlans)
				{
					SOLineSplit split = splitWithPlan;
					if (updatedSplits.Contains(split.SplitLineNbr))
					{
						split.Completed = true;
						split.ShipComplete = line.ShipComplete;
						split.PlanID = null;
						split.RefNoteID = Document.Current.NoteID;
						orderEntry.splits.Cache.Update(split);
						orderEntry.Caches[typeof(INItemPlan)].Delete((INItemPlan)splitWithPlan);
					}
				}

				if (!completeLine)
				{
					SOLineSplit split = PXCache<SOLineSplit>.CreateCopy(lastUpdatedSplit);
					split.PlanID = null;
					split.PlanType = split.BackOrderPlanType;
					split.ParentSplitLineNbr = split.SplitLineNbr;
					split.SplitLineNbr = null;
					split.IsAllocated = false;
					split.Completed = false;
					split.ShipmentNbr = null;
					split.LotSerialNbr = null;
					split.VendorID = null;
					split.ClearPOFlags();
					split.ClearPOReferences();
					split.ClearSOReferences();

					split.RefNoteID = null;
					split.BaseReceivedQty = 0m;
					split.ReceivedQty = 0m;
					split.BaseShippedQty = 0m;
					split.ShippedQty = 0m;
					split.BaseQty = line.BaseOrderQty - line.BaseShippedQty - sumBaseQty;
					split.Qty = INUnitAttribute.ConvertFromBase(orderEntry.splits.Cache, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY);

					orderEntry.splits.Insert(split);
				}
				orderEntry.RowUpdating.RemoveHandler<SOLine>(cancelSOLineUpdatingHandler);

				using (orderEntry.LineSplittingExt.SuppressedModeScope(true))
				{
					line.ShippedQty += line.LineSign * sumQty;
					line.BaseShippedQty += line.LineSign * sumBaseQty;
					if (completeLine)
					{
						line.OpenQty = 0m;
						line.ClosedQty = line.OrderQty;
						line.BaseClosedQty = line.BaseOrderQty;
						line.OpenLine = false;
						line.Completed = true;
						line.UnbilledQty -= (line.OrderQty - line.ShippedQty);
					}
					else
					{
						line.OpenQty = line.OrderQty - line.ShippedQty;
						line.BaseOpenQty = line.BaseOrderQty - line.BaseShippedQty;
						line.ClosedQty = line.ShippedQty;
						line.BaseClosedQty = line.BaseShippedQty;
					}
					orderEntry.Transactions.Cache.Update(line);
				}
			}

			var orderShipment = SOOrderShipment.FromDirectInvoice(
				Document.Current,
				orderEntry.Document.Current.OrderType,
				orderEntry.Document.Current.OrderNbr);

			orderShipment.LineCntr = orderARTrans.Count;
			orderShipment.ShipmentQty = orderInvoicedQty;
			orderShipment.LineTotal = orderInvoicedAmt;
			orderShipment.CreateINDoc = updateIN;

			if (orderEntry.Document.Current.OpenLineCntr <= 0)
				orderEntry.Document.Current.MarkCompleted();

			orderEntry.Save.Press();

			return orderShipment;
		}

		protected virtual void CreateDirectShipments(
			List<SOOrderShipment> directShipmentsToCreate,
			List<SOOrderShipment> existingShipments)
		{
			foreach (var orderShipment in directShipmentsToCreate)
			{
				SOOrderShipment existingOrderShipment = existingShipments.FirstOrDefault(s => s.OrderType == orderShipment.OrderType
					&& s.OrderNbr == orderShipment.OrderNbr
					&& s.ShippingRefNoteID == orderShipment.ShippingRefNoteID);

				SOOrder order = soorder.Select(orderShipment.OrderType, orderShipment.OrderNbr);

				if (existingOrderShipment != null)
				{
					existingOrderShipment = SOOrderShipment.PK.Find(this, existingOrderShipment, PKFindOptions.IncludeDirty);

					existingOrderShipment.LineCntr += orderShipment.LineCntr;
					existingOrderShipment.ShipmentQty += orderShipment.ShipmentQty;
					existingOrderShipment.LineTotal += orderShipment.LineTotal;
					existingOrderShipment.CreateINDoc |= orderShipment.CreateINDoc;

					if (existingOrderShipment.InvtRefNbr == null && orderShipment.InvtRefNbr != null)
					{
						existingOrderShipment.InvtDocType = orderShipment.InvtDocType;
						existingOrderShipment.InvtRefNbr = orderShipment.InvtRefNbr;
						existingOrderShipment.InvtNoteID = orderShipment.InvtNoteID;
					}

					shipmentlist.Update(existingOrderShipment);
				}
				else
				{
					orderShipment.Operation = order.DefaultOperation;
					orderShipment.OrderNoteID = order.NoteID;
					shipmentlist.Insert(orderShipment);

					order.ShipmentCntr++;
				}

				if (order.OpenLineCntr == 0 && order.UnbilledMiscTot == 0m && order.UnbilledOrderQty == 0m)
					order.MarkCompleted();

				soorder.Update(order);
			}

			if (shipmentlist.Cache.IsInsertedUpdatedDeleted)
				this.Save.Press();
		}

		protected virtual decimal? CalculateUnitCostForReturnDirectLine(ARTran tran)
		{
			PXSelectBase cmd = new PXSelectReadonly<ARTran,
				Where<ARTran.tranType, Equal<Current<ARTran.origInvoiceType>>, And<ARTran.refNbr, Equal<Current<ARTran.origInvoiceNbr>>,
					And<ARTran.inventoryID, Equal<Current<ARTran.inventoryID>>, And<ARTran.subItemID, Equal<Current<ARTran.subItemID>>,
					And<Mult<ARTran.qty, ARTran.invtMult>, LessEqual<decimal0>>>>>>>(this);

			decimal baseQtySum = 0m, tranCostSum = 0m;
			foreach (ARTran t in cmd.View.SelectMultiBound(new[] { tran }))
			{
				if (INTranType.InvtMultFromInvoiceType(t.TranType) * t.Qty < 0m)
				{
					baseQtySum += Math.Abs(t.BaseQty ?? 0m);
					tranCostSum += Math.Abs(t.TranCost ?? 0m);
				}
			}

			return (baseQtySum == 0m) ? null :
				(decimal?)INUnitAttribute.ConvertToBase(base.Transactions.Cache, tran.InventoryID, tran.UOM, (decimal)(tranCostSum / baseQtySum), INPrecision.UNITCOST);
		}

		public override void DefaultDiscountAccountAndSubAccount(ARTran tran)
		{
			ARTran firstTranWithOrderType = PXSelect<ARTran, Where<ARTran.tranType, Equal<Current<SOInvoice.docType>>,
				And<ARTran.refNbr, Equal<Current<SOInvoice.refNbr>>,
				And<ARTran.sOOrderType, IsNotNull>>>>.Select(this);

			if (firstTranWithOrderType != null)
			{
				SOOrderType type = soordertype.SelectWindowed(0, 1, firstTranWithOrderType.SOOrderType);

				if (type != null)
				{
					Location customerloc = location.Current;
					CRLocation companyloc =
						PXSelectJoin<CRLocation, InnerJoin<BAccountR, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>, And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>, InnerJoin<Branch, On<Branch.bAccountID, Equal<BAccountR.bAccountID>>>>, Where<Branch.branchID, Equal<Current<ARRegister.branchID>>>>.Select(this);

					switch (type.DiscAcctDefault)
					{
						case SODiscAcctSubDefault.OrderType:
							tran.AccountID = (int?)GetValue<SOOrderType.discountAcctID>(type);
							break;
						case SODiscAcctSubDefault.MaskLocation:
							tran.AccountID = (int?)GetValue<Location.cDiscountAcctID>(customerloc);
							break;
					}


					if (tran.AccountID == null)
					{
						tran.AccountID = type.DiscountAcctID;
					}

					Discount.Cache.RaiseFieldUpdated<ARTran.accountID>(tran, null);

					if (tran.AccountID != null)
					{
						object ordertype_SubID = GetValue<SOOrderType.discountSubID>(type);
						object customer_Location = GetValue<Location.cDiscountSubID>(customerloc);
						object company_Location = GetValue<CRLocation.cMPDiscountSubID>(companyloc);

						object value = SODiscSubAccountMaskAttribute.MakeSub<SOOrderType.discSubMask>(this, type.DiscSubMask,
								new object[] { ordertype_SubID, customer_Location, company_Location },
								new Type[] { typeof(SOOrderType.discountSubID), typeof(Location.cDiscountSubID), typeof(Location.cMPDiscountSubID) });

						Discount.Cache.RaiseFieldUpdating<ARTran.subID>(tran, ref value);

						tran.SubID = (int?)value;
					}
				}
			}

		}

		public override ARInvoiceState GetDocumentState(PXCache cache, ARInvoice doc)
		{
			var res = base.GetDocumentState(cache, doc);

			SOInvoice soDoc = SODocument.Select(doc.DocType, doc.RefNbr);
			res.AllowUpdateAdjustments &= soDoc?.InitialSOBehavior != SOBehavior.MO;
			res.AllowDeleteAdjustments &= soDoc?.InitialSOBehavior != SOBehavior.MO;
			res.AllowUpdateCMAdjustments &= soDoc?.InitialSOBehavior != SOBehavior.MO;
			res.LoadDocumentsEnabled &= soDoc?.InitialSOBehavior != SOBehavior.MO;
			res.AutoApplyEnabled &= soDoc?.InitialSOBehavior != SOBehavior.MO;

			return res;
		}

		#region Freight
		public virtual SOFreightDetail FillFreightDetails(SOOrder order, SOOrderShipment ordershipment)
		{
			return string.Equals(ordershipment.ShipmentNbr, Constants.NoShipmentNbr)
				? FillFreightDetailsForNonShipment(order, ordershipment)
				: FillFreightDetailsForShipment(order, ordershipment);
		}

		public virtual SOFreightDetail FillFreightDetailsForNonShipment(SOOrder order, SOOrderShipment orderShipment)
		{
			var freightDet = new SOFreightDetail()
			{
				CuryInfoID = Document.Current.CuryInfoID,
				ShipmentNbr = orderShipment.ShipmentNbr,
				ShipmentType = orderShipment.ShipmentType,
				OrderType = orderShipment.OrderType,
				OrderNbr = orderShipment.OrderNbr,
				ProjectID = order.ProjectID,
				ShipTermsID = order.ShipTermsID,
				ShipVia = order.ShipVia,
				ShipZoneID = order.ShipZoneID,
				TaxCategoryID = order.FreightTaxCategoryID,

				Weight = order.OrderWeight,
				Volume = order.OrderVolume,
				CuryLineTotal = order.CuryLineTotal,
				CuryFreightCost = order.CuryFreightCost,
				CuryFreightAmt = order.CuryFreightAmt,
				CuryPremiumFreightAmt = order.CuryPremiumFreightAmt,
			};

			PopulateFreightAccountAndSubAccount(freightDet, order, orderShipment);

			return FreightDetails.Insert(freightDet);
		}

		public virtual SOShipment GetShipment(SOOrderShipment orderShipment)
			=> GetShipment(orderShipment.ShipmentType, orderShipment.ShipmentNbr);

		protected virtual SOShipment GetShipment(string shipmentType, string shipmentNbr)
		{
			if (string.Equals(shipmentNbr, Constants.NoShipmentNbr) || shipmentType == SOShipmentType.DropShip)
				return null;

			return (SOShipment)PXSelect<SOShipment,
				Where<SOShipment.shipmentType, Equal<Required<SOShipment.shipmentType>>, And<SOShipment.shipmentNbr, Equal<Required<SOShipment.shipmentNbr>>>>>
				.Select(this, shipmentType, shipmentNbr);
		}

		public virtual SOFreightDetail FillFreightDetailsForShipment(SOOrder order, SOOrderShipment orderShipment)
		{
			bool isDropship = (orderShipment.ShipmentType == SOShipmentType.DropShip);
			var shipment = GetShipment(orderShipment);
			if (!isDropship && shipment == null)
				return null;

			bool isOrderBased = ((shipment?.FreightAmountSource ?? order.FreightAmountSource) == FreightAmountSourceAttribute.OrderBased);

			var freightDet = new SOFreightDetail()
			{
				CuryInfoID = Document.Current.CuryInfoID,
				ShipmentNbr = orderShipment.ShipmentNbr,
				ShipmentType = orderShipment.ShipmentType,
				OrderType = orderShipment.OrderType,
				OrderNbr = orderShipment.OrderNbr,
				ProjectID = order.ProjectID,
				ShipTermsID = (isDropship || isOrderBased) ? order.ShipTermsID : shipment.ShipTermsID,
				ShipVia = isDropship ? order.ShipVia : shipment.ShipVia,
				ShipZoneID = isDropship ? order.ShipZoneID : shipment.ShipZoneID,
				// set freight tax category from order unconditionally to update it later from shipment for correct tax calculation
				TaxCategoryID = order.FreightTaxCategoryID,
				Weight = orderShipment.ShipmentWeight,
				Volume = orderShipment.ShipmentVolume,
				LineTotal = orderShipment.LineTotal,
				CuryLineTotal = GetExtension<MultiCurrency>().GetCurrencyInfo(Document.Current.CuryInfoID).CuryConvCury(orderShipment.LineTotal.Value),
				CuryFreightCost = 0m,
				CuryFreightAmt = 0m,
				CuryPremiumFreightAmt = 0m,
			};

			bool fullOrderAllocation = IsFullOrderFreightAmountFirstTime(order);
			CalcOrderBasedFreight(freightDet, order, orderShipment, isOrderBased, fullOrderAllocation, isDropship);
			CalcShipmentBasedFreight(freightDet, orderShipment, shipment, isOrderBased, isDropship);

			PopulateFreightAccountAndSubAccount(freightDet, order, orderShipment);

			freightDet = FreightDetails.Insert(freightDet);

			freightDet = FillFreightDetailRoundingDiffByOrder(freightDet, order, orderShipment, isOrderBased, fullOrderAllocation);
			freightDet = FillFreightDetailRoundingDiffByShipment(freightDet, orderShipment, shipment, isOrderBased, isDropship);

			return freightDet;
		}

		public virtual void CalcOrderBasedFreight(SOFreightDetail freightDet, SOOrder order, SOOrderShipment orderShipment, bool isOrderBased, bool fullOrderAllocation, bool isDropship)
		{
			if (order.DefaultOperation != orderShipment.Operation)
				return;

			var orderRatio = new Lazy<decimal>(() => CalcOrderFreightRatio(order, orderShipment));
			if (fullOrderAllocation)
			{
				SOOrderShipment allocated = PXSelect<SOOrderShipment,
					Where<SOOrderShipment.orderType, Equal<Current<SOOrderShipment.orderType>>,
						And<SOOrderShipment.orderNbr, Equal<Current<SOOrderShipment.orderNbr>>,
						And<SOOrderShipment.invoiceNbr, IsNotNull,
						And<SOOrderShipment.orderFreightAllocated, Equal<True>>>>>>
					.SelectSingleBound(this, new object[] { orderShipment });
				if (allocated == null)
				{
					freightDet.CuryPremiumFreightAmt = order.CuryPremiumFreightAmt;
					if (isOrderBased)
					{
						freightDet.CuryFreightAmt = order.CuryFreightAmt;
					}
					orderShipment.OrderFreightAllocated = true;
				}
			}
			else if (sosetup.Current.FreightAllocation == FreightAllocationList.Prorate)
			{
				CurrencyInfo currencyInfo = GetExtension<MultiCurrency>().GetDefaultCurrencyInfo();
				freightDet.CuryPremiumFreightAmt = currencyInfo.RoundCury(orderRatio.Value * (order.CuryPremiumFreightAmt ?? 0m));
				if (isOrderBased)
				{
					freightDet.CuryFreightAmt = currencyInfo.RoundCury(orderRatio.Value * (order.CuryFreightAmt ?? 0m));
				}
			}

			if (isDropship)
			{
				freightDet.CuryFreightCost = GetExtension<MultiCurrency>()
					.GetDefaultCurrencyInfo()
					.RoundCury(orderRatio.Value * (order.CuryFreightCost ?? 0m));
			}
		}

		public virtual void CalcShipmentBasedFreight(SOFreightDetail freightDet, SOOrderShipment orderShipment, SOShipment shipment, bool isOrderBased, bool isDropship)
		{
			if (isDropship) return;

			decimal shipmentRatio = CalcShipmentFreightRatio(orderShipment, shipment);
			CM.Extensions.CurrencyInfo currencyInfo = GetExtension<MultiCurrency>().GetDefaultCurrencyInfo();

			bool sameCurrency = string.Equals(shipment.CuryID, Document.Current.CuryID, StringComparison.OrdinalIgnoreCase);
			if (sameCurrency)
			{
				freightDet.CuryFreightCost = currencyInfo.RoundCury(shipmentRatio * (shipment.CuryFreightCost ?? 0m));
				if (!isOrderBased)
				{
					freightDet.CuryFreightAmt = currencyInfo.RoundCury(shipmentRatio * (shipment.CuryFreightAmt ?? 0m));
				}
			}
			else
			{
				freightDet.FreightCost = shipmentRatio * (shipment.FreightCost ?? 0m);
				freightDet.CuryFreightCost = currencyInfo.CuryConvCury(freightDet.FreightCost.Value);
				if (!isOrderBased)
				{
					freightDet.FreightAmt = shipmentRatio * (shipment.FreightAmt ?? 0m);
					freightDet.CuryFreightAmt = currencyInfo.CuryConvCury(freightDet.FreightAmt.Value);
				}
			}
		}

		public virtual bool IsFullOrderFreightAmountFirstTime(SOOrder order)
		{
			if (sosetup.Current.FreightAllocation == FreightAllocationList.FullAmount || order.LineTotal <= 0m)
				return true;

			if (order.Behavior != SOBehavior.RM)
				return false;

			SOOrderTypeOperation nonDefaultOperation = PXSelectReadonly<SOOrderTypeOperation,
				Where<SOOrderTypeOperation.orderType, Equal<Required<SOOrderTypeOperation.orderType>>,
					And<SOOrderTypeOperation.operation, NotEqual<Required<SOOrderTypeOperation.operation>>,
					And<SOOrderTypeOperation.active, Equal<True>>>>>
				.SelectWindowed(this, 0, 1, order.OrderType, order.DefaultOperation);
			return nonDefaultOperation != null;
		}

		public virtual decimal CalcOrderFreightRatio(SOOrder order, SOOrderShipment orderShipment)
		{
			if (orderShipment.ShipmentType == SOShipmentType.DropShip && orderShipment.LineTotal == 0m)
			{
				// this block is obsolete and will be used only for drop-shipments created before
				// now SOOrderShipment.LineTotal is properly populated on PO Receipt releasing

				// prorate by base receipted qty and then by amount
				if (order.CuryLineTotal == 0m)
				{
					return 1m;
				}
				decimal curyDropShipLineAmt = 0m;
				foreach (PXResult<SOLine, SOLineSplit, PO.POLine, POReceiptLine> res in PXSelectJoin<SOLine,
					InnerJoin<SOLineSplit, On<SOLineSplit.orderType, Equal<SOLine.orderType>, And<SOLineSplit.orderNbr, Equal<SOLine.orderNbr>, And<SOLineSplit.lineNbr, Equal<SOLine.lineNbr>>>>,
					InnerJoin<PO.POLine, On<PO.POLine.orderType, Equal<SOLineSplit.pOType>, And<PO.POLine.orderNbr, Equal<SOLineSplit.pONbr>, And<PO.POLine.lineNbr, Equal<SOLineSplit.pOLineNbr>>>>,
					InnerJoin<POReceiptLine, On<POReceiptLine.pOLineNbr, Equal<PO.POLine.lineNbr>, And<POReceiptLine.pONbr, Equal<PO.POLine.orderNbr>, And<POReceiptLine.pOType, Equal<PO.POLine.orderType>>>>>>>,
					Where<POReceiptLine.receiptType, Equal<Required<POReceiptLine.receiptType>>,
						And<POReceiptLine.receiptNbr, Equal<Required<POReceiptLine.receiptNbr>>,
						And<SOLine.orderType, Equal<Required<SOLine.orderType>>,
						And<SOLine.orderNbr, Equal<Required<SOLine.orderNbr>>>>>>>
					.Select(this,
						orderShipment.Operation == SOOperation.Receipt ? POReceiptType.POReturn : POReceiptType.POReceipt,
						orderShipment.ShipmentNbr,
						orderShipment.OrderType, orderShipment.OrderNbr))
				{
					SOLine soline = (SOLine)res;
					POReceiptLine pOReceiptline = (POReceiptLine)res;

					decimal baseQtyRcpRate = ((soline.BaseOrderQty ?? 0m) > 0m) ? (decimal)(pOReceiptline.BaseReceiptQty / soline.BaseOrderQty) : 1m;
					curyDropShipLineAmt += (soline.CuryLineAmt ?? 0m) * baseQtyRcpRate;
				}
				return Math.Min(1m, curyDropShipLineAmt / (decimal)order.CuryLineTotal);
			}
			else
			{
				return (order.LineTotal == 0m) ? 1m : Math.Min(1m, (decimal)(orderShipment.LineTotal / order.LineTotal));
			}
		}

		public virtual decimal CalcShipmentFreightRatio(SOOrderShipment orderShipment, SOShipment shipment)
		{
			return (shipment.LineTotal == 0m) ? 1m : Math.Min(1m, (decimal)(orderShipment.LineTotal / shipment.LineTotal));
		}

		public virtual SOFreightDetail FillFreightDetailRoundingDiffByShipment(SOFreightDetail freightDet, SOOrderShipment orderShipment, SOShipment shipment, bool isOrderBased, bool isDropship)
		{
			if (isDropship)
				return freightDet;

			bool sameCurrency = string.Equals(shipment.CuryID, Document.Current.CuryID, StringComparison.OrdinalIgnoreCase);
			if (!sameCurrency) return freightDet;

			PXResultset<SOFreightDetail> freightDetails = PXSelect<SOFreightDetail,
				Where<SOFreightDetail.docType, Equal<Current<ARInvoice.docType>>, And<SOFreightDetail.refNbr, Equal<Current<ARInvoice.refNbr>>,
					And<SOFreightDetail.shipmentType, Equal<Required<SOFreightDetail.shipmentType>>, And<SOFreightDetail.shipmentNbr, Equal<Required<SOFreightDetail.shipmentNbr>>>>>>>
				.Select(this, orderShipment.ShipmentType, orderShipment.ShipmentNbr);

			if (freightDetails.Count <= 1)
				return freightDet;

			PXResultset<SOOrderShipment> orderShipments = PXSelect<SOOrderShipment,
				Where<SOOrderShipment.shipmentType, Equal<Required<SOOrderShipment.shipmentType>>, And<SOOrderShipment.shipmentNbr, Equal<Required<SOOrderShipment.shipmentNbr>>>>>
				.Select(this, orderShipment.ShipmentType, orderShipment.ShipmentNbr);

			if (freightDetails.Count != orderShipments.Count)
				return freightDet;

			decimal totalInvoicedFreightCost = 0m,
				totalInvoicedFreightPrice = 0m;
			foreach (SOFreightDetail freightDetail in freightDetails)
			{
				totalInvoicedFreightCost += freightDetail.CuryFreightCost ?? 0m;
				totalInvoicedFreightPrice += freightDetail.CuryFreightAmt ?? 0m;
			}

			decimal freightCostDiff = (shipment.CuryFreightCost ?? 0m) - totalInvoicedFreightCost,
				freightPriceDiff = (shipment.CuryFreightAmt ?? 0m) - totalInvoicedFreightPrice;
			if (freightCostDiff != 0m || !isOrderBased && freightPriceDiff != 0m)
			{
				freightDet.CuryFreightCost += freightCostDiff;
				if (freightDet.CuryFreightCost < 0m)
				{
					freightDet.CuryFreightCost = 0m;
				}
				if (!isOrderBased)
				{
					freightDet.CuryFreightAmt += freightPriceDiff;
					if (freightDet.CuryFreightAmt < 0m)
					{
						freightDet.CuryFreightAmt = 0m;
					}
				}

				return FreightDetails.Update(freightDet);
			}

			return freightDet;
		}

		public virtual SOFreightDetail FillFreightDetailRoundingDiffByOrder(SOFreightDetail freightDet, SOOrder order, SOOrderShipment orderShipment, bool isOrderBased, bool fullOrderAllocation)
		{
			if (order.OpenLineCntr != 0 || fullOrderAllocation)
				return freightDet;

			PXResultset<SOFreightDetail> freightDetails = PXSelect<SOFreightDetail,
				Where<SOFreightDetail.docType, Equal<Current<ARInvoice.docType>>, And<SOFreightDetail.refNbr, Equal<Current<ARInvoice.refNbr>>,
					And<SOFreightDetail.orderType, Equal<Required<SOFreightDetail.orderType>>, And<SOFreightDetail.orderNbr, Equal<Required<SOFreightDetail.orderNbr>>>>>>>
				.Select(this, order.OrderType, order.OrderNbr);

			if (freightDetails.Count <= 1)
				return freightDet;

			PXResultset<SOOrderShipment> orderShipments = PXSelect<SOOrderShipment,
				Where<SOOrderShipment.orderType, Equal<Required<SOOrderShipment.orderType>>, And<SOOrderShipment.orderNbr, Equal<Required<SOOrderShipment.orderNbr>>>>>
				.Select(this, order.OrderType, order.OrderNbr);

			if (freightDetails.Count != orderShipments.Count)
				return freightDet;

			decimal totalInvoicedFreight = 0m,
				totalInvoicedPremiumFreight = 0m;
			foreach (SOFreightDetail freightDetail in freightDetails)
			{
				totalInvoicedFreight += freightDetail.CuryFreightAmt ?? 0m;
				totalInvoicedPremiumFreight += freightDetail.CuryPremiumFreightAmt ?? 0m;
			}

			decimal freightDiff = (order.CuryFreightAmt ?? 0m) - totalInvoicedFreight,
				premiumFreightDiff = (order.CuryPremiumFreightAmt ?? 0m) - totalInvoicedPremiumFreight;
			if (isOrderBased && freightDiff != 0m || premiumFreightDiff != 0m)
			{
				if (isOrderBased)
				{
					freightDet.CuryFreightAmt += freightDiff;
					if (freightDet.CuryFreightAmt < 0m)
					{
						freightDet.CuryFreightAmt = 0m;
					}
				}
				freightDet.CuryPremiumFreightAmt += premiumFreightDiff;

				return FreightDetails.Update(freightDet);
			}
			return freightDet;
		}

		public virtual ARTran UpdateFreightTransaction(SOFreightDetail fd, bool newFreightDetail)
		{
			ARTran freightTran = null;
			if (!newFreightDetail && Document.Current != null)
			{
				freightTran = GetFreightTran(Document.Current.DocType, Document.Current.RefNbr, fd);
			}

			if (fd.CuryFreightAmt == 0m && fd.CuryPremiumFreightAmt == 0m && fd.CuryFreightCost == 0m)
			{
				if (freightTran != null)
				{
					Freight.Delete(freightTran);
				}
				return null;
			}

			bool newFreightTran = (freightTran == null);
			freightTran = freightTran ?? new ARTran();
			freightTran.SOShipmentNbr = fd.ShipmentNbr;
			freightTran.SOShipmentType = fd.ShipmentType ?? SOShipmentType.Issue;
			freightTran.SOOrderType = fd.OrderType;
			freightTran.SOOrderNbr = fd.OrderNbr;
			freightTran.LineType = SOLineType.Freight;
			freightTran.Qty = 1;
			freightTran.CuryUnitPrice = fd.CuryTotalFreightAmt;
			freightTran.CuryUnitPriceDR = fd.CuryTotalFreightAmt;
			freightTran.CuryTranAmt = fd.CuryTotalFreightAmt;
			freightTran.CuryExtPrice = fd.CuryTotalFreightAmt;
			freightTran.TranCostOrig = fd.FreightCost;
			freightTran.TaxCategoryID = fd.TaxCategoryID;
			freightTran.AccountID = fd.AccountID;
			freightTran.SubID = fd.SubID;
			freightTran.ProjectID = fd.ProjectID;
			freightTran.TaskID = fd.TaskID;
			freightTran.Commissionable = false;
			if (PM.CostCodeAttribute.UseCostCode())
				freightTran.CostCodeID = PM.CostCodeAttribute.DefaultCostCode;
			using (new PXLocaleScope(customer.Current.LocaleName))
			{
				freightTran.TranDesc = PXMessages.LocalizeFormatNoPrefix(Messages.FreightDescr, fd.ShipVia);
			}

			freightTran = newFreightTran
				? Freight.Insert(freightTran)
				: Freight.Update(freightTran);

			if (freightTran.TaskID == null && !PM.ProjectDefaultAttribute.IsNonProject(freightTran.ProjectID))
			{
				Account ac = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, freightTran.AccountID);
				throw new PXException(Messages.TaskWasNotAssigned, ac.AccountCD);
			}
			return freightTran;
		}

		public virtual ARTran GetFreightTran(string docType, string refNbr, SOFreightDetail fd)
		{
			return PXSelect<ARTran,
				Where<ARTran.lineType, Equal<SOLineType.freight>,
					And<ARTran.tranType, Equal<Required<ARInvoice.docType>>, And<ARTran.refNbr, Equal<Required<ARInvoice.refNbr>>,
					And<ARTran.sOShipmentType, Equal<Required<ARTran.sOShipmentType>>, And<ARTran.sOShipmentNbr, Equal<Required<ARTran.sOShipmentNbr>>,
					And<ARTran.sOOrderType, Equal<Required<ARTran.sOOrderType>>, And<ARTran.sOOrderNbr, Equal<Required<ARTran.sOOrderNbr>>>>>>>>>>
				.Select(this, docType, refNbr, fd.ShipmentType, fd.ShipmentNbr, fd.OrderType, fd.OrderNbr);
		}

		public virtual void CopyFreightNotesAndFilesToARTran() => CopyFreightNotesAndFilesToARTran(Document.Current);

		public virtual void CopyFreightNotesAndFilesToARTran(ARRegister doc)
		{
			if (doc == null || doc.Released != true)
				return;
			var freights = FreightDetails.Select(doc.DocType, doc.RefNbr).RowCast<SOFreightDetail>();
			if (!freights.Any())
				return;
			var freightLines = new SelectFrom<ARTran>
					.Where<ARTran.tranType.IsEqual<@P.AsString.ASCII>
					.And<ARTran.refNbr.IsEqual<@P.AsString>
					.And<ARTran.lineType.IsEqual<SOLineType.freight>>>>.View(this)
					.SelectMain(doc.DocType, doc.RefNbr)
					.ToLookup(x => Composite.Create(x.SOOrderType, x.SOOrderNbr, x.SOShipmentType, x.SOShipmentNbr));
			foreach (var freight in freights)
			{
				foreach (var line in freightLines[Composite.Create(freight.OrderType, freight.OrderNbr, freight.ShipmentType, freight.ShipmentNbr)])
					PXNoteAttribute.CopyNoteAndFiles(FreightDetails.Cache, freight, Freight.Cache, line);
			}
		}

		public virtual void PopulateFreightAccountAndSubAccount(SOFreightDetail freightDet, SOOrder order, SOOrderShipment orderShipment)
		{
			int? accountID;
			object subID;
			GetFreightAccountAndSubAccount(order, freightDet.ShipVia, order.OwnerID, out accountID, out subID);
			freightDet.AccountID = accountID;
			FreightDetails.Cache.RaiseFieldUpdating<SOFreightDetail.subID>(freightDet, ref subID);
			freightDet.SubID = (int?)subID;
		}

		public virtual void GetFreightAccountAndSubAccount(SOOrder order, string ShipVia, int? ownerID, out int? accountID, out object subID)
		{
			accountID = null;
			subID = null;
			SOOrderType type = soordertype.SelectWindowed(0, 1, order.OrderType);

			if (type != null)
			{
				Location customerloc = location.Current;
				Carrier carrier = Carrier.PK.Find(this, ShipVia);

				CRLocation companyloc =
						PXSelectJoin<CRLocation, InnerJoin<BAccountR, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>, And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>, InnerJoin<Branch, On<Branch.bAccountID, Equal<BAccountR.bAccountID>>>>, Where<Branch.branchID, Equal<Current<ARRegister.branchID>>>>.Select(this);
				EPEmployee employee = (EPEmployee)PXSelect<EPEmployee, Where<EPEmployee.defContactID, Equal<Required<SOOrder.ownerID>>>>.Select(this, ownerID);

				switch (type.FreightAcctDefault)
				{
					case SOFreightAcctSubDefault.OrderType:
						accountID = (int?)GetValue<SOOrderType.freightAcctID>(type);
						break;
					case SOFreightAcctSubDefault.MaskLocation:
						accountID = (int?)GetValue<Location.cFreightAcctID>(customerloc);
						break;
					case SOFreightAcctSubDefault.MaskShipVia:
						accountID = (int?)GetValue<Carrier.freightSalesAcctID>(carrier);
						break;
				}

				if (accountID == null)
				{
					accountID = type.FreightAcctID;

					if (accountID == null)
					{
						throw new PXException(Messages.FreightAccountIsRequired);
					}

				}

				if (accountID != null)
				{
					object orderType_SubID = GetValue<SOOrderType.freightSubID>(type);
					object customer_Location_SubID = GetValue<Location.cFreightSubID>(customerloc);
					object carrier_SubID = GetValue<Carrier.freightSalesSubID>(carrier);
					object branch_SubID = GetValue<CRLocation.cMPFreightSubID>(companyloc);
					object employee_SubID = GetValue<EPEmployee.salesSubID>(employee);

					if (employee_SubID != null)
						subID = SOFreightSubAccountMaskAttribute.MakeSub<SOOrderType.freightSubMask>(this, type.FreightSubMask,
									new object[] { orderType_SubID, customer_Location_SubID, carrier_SubID, branch_SubID, employee_SubID },
									new Type[] { typeof(SOOrderType.freightSubID), typeof(Location.cFreightSubID), typeof(Carrier.freightSalesSubID), typeof(Location.cMPFreightSubID), typeof(EPEmployee.salesSubID) });
					else
						subID = SOFreightSubAccountMaskAttribute.MakeSub<SOOrderType.freightSubMask>(this, type.FreightSubMask,
							new object[] { orderType_SubID, customer_Location_SubID, carrier_SubID, branch_SubID, customer_Location_SubID },
							new Type[] { typeof(SOOrderType.freightSubID), typeof(Location.cFreightSubID), typeof(Carrier.freightSalesSubID), typeof(Location.cMPFreightSubID), typeof(Location.cFreightSubID) });
				}
			}
		}
		#endregion

		#region Discount

		public override void RecalculateDiscounts(PXCache sender, ARTran line)
		{
			RecalculateDiscounts(sender, line, false);
		}

		public virtual void RecalculateDiscounts(PXCache sender, ARTran line, bool disableGroupAndDocumentDiscountsCalculation)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>() && line.InventoryID != null && line.Qty != null && line.CuryTranAmt != null && line.IsFree != true)
			{
				object origCurrent = sender.CreateCopy(sender.Current);
				DateTime? docDate = Document.Current.DocDate;
				int? customerLocationID = Document.Current.CustomerLocationID;

				//Recalculate discounts on Sales Order date
				/*SOLine soline = PXSelect<SOLine, Where<SOLine.orderType, Equal<Required<SOLine.orderType>>,
				And<SOLine.orderNbr, Equal<Required<SOLine.orderNbr>>,
				And<SOLine.lineNbr, Equal<Required<SOLine.lineNbr>>>>>>.Select(this, line.SOOrderType, line.SOOrderNbr, line.SOOrderLineNbr);
				if (soline != null)
				{
					docDate = soline.OrderDate;
				}*/

				DiscountEngine.DiscountCalculationOptions discountCalculationOptions = GetDefaultARDiscountCalculationOptions(Document.Current) | DiscountEngine.DiscountCalculationOptions.DisableFreeItemDiscountsCalculation;
				if (line.CalculateDiscountsOnImport == true)
					discountCalculationOptions = discountCalculationOptions | DiscountEngine.DiscountCalculationOptions.CalculateDiscountsFromImport;
				if (disableGroupAndDocumentDiscountsCalculation)
					discountCalculationOptions = discountCalculationOptions | DiscountEngine.DiscountCalculationOptions.DisableGroupAndDocumentDiscounts;

				//Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers 
				ARDiscountEngine.SetDiscounts(
					sender,
					Transactions,
					line,
					ARDiscountDetails,
					Document.Current.BranchID,
					customerLocationID,
					Document.Current.CuryID,
					docDate,
					recalcdiscountsfilter.Current,
					discountCalculationOptions);

				RecalculateTotalDiscount();

				if (sender.Graph.IsMobile || sender.Graph.IsContractBasedAPI)
				{
					sender.Current = origCurrent;
				}
			}
			else if (!PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>() && Document.Current != null)
			{
				//Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers 
				ARDiscountEngine.CalculateDocumentDiscountRate(Transactions.Cache, Transactions, line, ARDiscountDetails);
			}
		}

		public bool ProrateDiscount
		{
			get
			{
				SOSetup sosetup = PXSelect<SOSetup>.Select(this);

				if (sosetup == null)
				{
					return true;//default true
				}
				else
				{
					if (sosetup.ProrateDiscounts == null)
						return true;
					else
						return sosetup.ProrateDiscounts == true;
				}

			}
		}
		#endregion

		#region Address Lookup Extension
		/// <exclude/>
		public class SOInvoiceEntryAddressLookupExtension : CR.Extensions.AddressLookupExtension<SOInvoiceEntry, ARInvoice, ARAddress>
		{
			protected override string AddressView => nameof(Base.Billing_Address);
		}

		/// <exclude/>
		public class SOInvoiceEntryShippingAddressLookupExtension : CR.Extensions.AddressLookupExtension<SOInvoiceEntry, ARInvoice, ARShippingAddress>
		{
			protected override string AddressView => nameof(Base.Shipping_Address);
		}
		#endregion

		protected override bool AskUserApprovalIfInvoiceIsLinkedToShipment(ARInvoice origDoc)
		{
			return true;
		}

		public override void PrefetchWithDetails()
		{
			LoadEntityDiscounts();
		}

		#region SelectEntityDiscounts enhancement

		protected string EntityDiscountsLoadedFor;

		protected virtual void LoadEntityDiscounts()
		{
			if (Document.Current?.RefNbr == null
				|| !PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>())
				return;

			var invoice = $"{Document.Current.DocType}-{Document.Current.RefNbr}";

			if (EntityDiscountsLoadedFor == invoice)
				return;

			var query = new
				SelectFrom<ARTran>
					.LeftJoin<DiscountItem>
						.On<DiscountItem.inventoryID.IsEqual<ARTran.inventoryID>>
					.LeftJoin<DiscountSequence>
						.On<DiscountSequence.isActive.IsEqual<True>
						.And<DiscountItem.FK.DiscountSequence>>
					.Where<ARTran.FK.Invoice.SameAsCurrent>
				.View
				.ReadOnly(this);

			var items = new Dictionary<int, HashSet<DiscountSequenceKey>>();

			using (new PXFieldScope(query.View,
					typeof(ARTran.inventoryID),
					typeof(DiscountSequence.discountID),
					typeof(DiscountSequence.discountSequenceID)))
			{
				foreach (PXResult<ARTran, DiscountItem, DiscountSequence> res in query.Select())
				{
					ARTran line = res;
					DiscountSequence seq = res;
					HashSet<DiscountSequenceKey> seqSet;

					if (line.InventoryID != null)
					{
						if (!items.TryGetValue(line.InventoryID.Value, out seqSet))
							items.Add(line.InventoryID.Value, seqSet = new HashSet<DiscountSequenceKey>());

						if (seq.DiscountID != null && seq.DiscountSequenceID != null)
							seqSet.Add(new DiscountSequenceKey(seq.DiscountID, seq.DiscountSequenceID));
					}
				}
			}

			DiscountEngine.UpdateEntityCache();
			DiscountEngine.PutEntityDiscountsToSlot<DiscountItem, int>(items);

			EntityDiscountsLoadedFor = invoice;
		}

		#endregion
	}

	public class SOInvoiceEntryProjectFieldVisibilityGraphExtension : PXGraphExtension<SOInvoiceEntry>
	{
		protected virtual void _(Events.RowSelected<ARInvoice> e)
		{
			if (e.Row == null) return;

			PXUIFieldAttribute.SetVisible<ARInvoice.projectID>(e.Cache, e.Row,
				PXAccess.FeatureInstalled<FeaturesSet.contractManagement>() || PM.ProjectAttribute.IsPMVisible(BatchModule.SO) || PM.ProjectAttribute.IsPMVisible(BatchModule.AR));
			PXUIFieldAttribute.SetDisplayName<ARInvoice.projectID>(e.Cache, GL.Messages.ProjectContract);
		}
	}

	public class SOInvoiceAddOrderPaymentsScope : Common.Scopes.FlaggedModeScopeBase<SOInvoiceAddOrderPaymentsScope> { }
}
