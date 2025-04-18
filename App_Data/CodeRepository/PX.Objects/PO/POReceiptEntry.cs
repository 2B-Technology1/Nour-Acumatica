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
using System.Collections;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Objects.CM.Extensions;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.PM;
using PX.Objects.TX;
using PX.Objects.EP;
using SOOrder = PX.Objects.SO.SOOrder;
using SOLine4 = PX.Objects.SO.SOLine4;
using PX.Objects.SO;
using System.Linq;
using CRLocation = PX.Objects.CR.Standalone.Location;
using ItemLotSerial = PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated.ItemLotSerial;
using SiteLotSerial = PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated.SiteLotSerial;
using PX.Objects.AP.MigrationMode;
using PX.Objects.Common;
using PX.CS.Contracts.Interfaces;
using PX.Data.DependencyInjection;
using PX.LicensePolicy;
using PX.TaxProvider;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.GL.FinPeriods;
using PX.Objects.Common.Extensions;
using PX.Objects.PO.LandedCosts;
using PX.Objects.PO.GraphExtensions.POReceiptEntryExt;
using System.Runtime.Serialization;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.Common.Bql;
using PX.Objects.Extensions.CostAccrual;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using PX.Data.WorkflowAPI;
using PX.Objects.IN.Services;
using PX.Objects.Extensions.MultiCurrency;
using PX.Objects.CR.Extensions;
using PX.Objects.IN.InventoryRelease;
using PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated;

namespace PX.Objects.PO
{   
    [Serializable]
	public class POReceiptEntry : PXGraph<POReceiptEntry, POReceipt>, IGraphWithInitialization
	{
		#region Extensions
		public class CostAccrual : NonStockAccrualGraph<POReceiptEntry, POReceipt>
		{
			[PXOverride]
			public virtual void SetExpenseAccount(PXCache sender, PXFieldDefaultingEventArgs e, InventoryItem item, Action<PXCache, PXFieldDefaultingEventArgs, InventoryItem> baseMethod)
			{
				POReceiptLine row = (POReceiptLine)e.Row;

				if (row != null && row.AccrueCost == true)
				{
					SetExpenseAccountSub(sender, e, item, row.SiteID,
					GetAccountSubUsingPostingClass: (InventoryItem inItem, INSite inSite, INPostClass inPostClass) =>
					{
						return INReleaseProcess.GetAcctID<INPostClass.invtAcctID>(Base, inPostClass.InvtAcctDefault, inItem, inSite, inPostClass);
					},
					GetAccountSubFromItem: (InventoryItem inItem) =>
					{
						return inItem.InvtAcctID;
					});
				}
			}

			[PXOverride]
			public virtual object GetExpenseSub(PXCache sender, PXFieldDefaultingEventArgs e, InventoryItem item, Func<PXCache, PXFieldDefaultingEventArgs, InventoryItem, object> baseMethod)
			{
				POReceiptLine row = (POReceiptLine)e.Row;

				object expenseAccountSub = null;

				if (row != null && row.AccrueCost == true)
				{
					expenseAccountSub = GetExpenseAccountSub(sender, e, item, row.SiteID,
					GetAccountSubUsingPostingClass: (InventoryItem inItem, INSite inSite, INPostClass inPostClass) =>
					{
						return INReleaseProcess.GetSubID<INPostClass.invtSubID>(Base, inPostClass.InvtAcctDefault, inPostClass.InvtSubMask, inItem, inSite, inPostClass);
					},
					GetAccountSubFromItem: (InventoryItem inItem) =>
					{
						return inItem.InvtSubID;
					});
				}

				return expenseAccountSub;
			}
		}

		public virtual MultiCurrency MultiCurrencyExt => FindImplementation<MultiCurrency>();
		public class MultiCurrency : MultiCurrencyGraph<POReceiptEntry, POReceipt>
		{
			protected override string Module => BatchModule.PO;

			protected override CurySourceMapping GetCurySourceMapping()
			{
				return new CurySourceMapping(typeof(Vendor));
			}

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(POReceipt))
				{
					DocumentDate = typeof(POReceipt.receiptDate),
					BAccountID = typeof(POReceipt.vendorID)
				};
			}

			protected override PXSelectBase[] GetChildren()
			{
				return new PXSelectBase[]
				{
					Base.Document,
					Base.transactions
				};
			}

			protected override void _(Events.FieldDefaulting<CurrencyInfo, CurrencyInfo.curyID> e)
			{
				if (Base.Document.Current?.ReceiptType == POReceiptType.TransferReceipt)
				{
					e.NewValue = GetBaseCurency();
					e.Cancel = true;
					return;
				}

				base._(e);
			}

			protected override void _(Events.FieldUpdated<Document, Document.documentDate> e)
			{
				POReceipt doc = (POReceipt)e.Cache.GetMain(e.Row);
				if (doc != null && (doc.ReceiptType != POReceiptType.POReturn || doc.ReturnInventoryCostMode != ReturnCostMode.OriginalCost))
				{
					DateFieldUpdated<Document.curyInfoID, Document.documentDate>(e.Cache, e.Row);
				}
			}

			protected override void _(Events.FieldUpdated<Document, Document.branchID> e)
			{
				POReceipt doc = (POReceipt)e.Cache.GetMain(e.Row);
				bool resetCuryID = !Base.IsCopyPasteContext &&
					(doc?.VendorID == null && e.ExternalCall || doc?.ReceiptType == POReceiptType.TransferReceipt);
				SourceFieldUpdated<Document.curyInfoID, Document.curyID, Document.documentDate>(e.Cache, e.Row, resetCuryID);
			}

			protected override void _(Events.FieldUpdated<Document, Document.bAccountID> e)
			{
				if ((e.ExternalCall || e.Row?.CuryID == null) && Base.AllowNonBaseCurrency() && !Base.IsCopyPasteContext)
				{
					SourceFieldUpdated<Document.curyInfoID, Document.curyID, Document.documentDate>(e.Cache, e.Row);
				}
			}

			protected override void _(Events.RowSelected<Document> e)
			{
				if (Base.SkipUIUpdate == false)
				{
					PXUIFieldAttribute.SetVisible<POReceipt.curyID>(e.Cache, e.Row, Base.AllowNonBaseCurrency());
					base._(e);
				}
			}

			protected override bool AllowOverrideCury()
			{
				return !Base.HasTransactions() && Base.AllowNonBaseCurrency()
					&& Base.vendor.Current != null && Base.vendor.Current.AllowOverrideCury == true;
			}

			protected override bool AllowOverrideRate(PXCache sender, CurrencyInfo info, CurySource source)
			{
				POReceipt doc = Base.Document.Current;
				bool changeCuryRateOnReceipt = Base.posetup.Current?.ChangeCuryRateOnReceipt == true;

				return base.AllowOverrideRate(sender, info, source) && doc?.Released != true && changeCuryRateOnReceipt && PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() &&
					(doc?.ReceiptType == POReceiptType.POReceipt || (doc?.ReceiptType == POReceiptType.POReturn && doc?.ReturnInventoryCostMode != ReturnCostMode.OriginalCost));
			}
		}
		#endregion

		public delegate List<PXResult<INItemPlan, INPlanType>> OnBeforeSalesOrderProcessPOReceipt(PXGraph graph, IEnumerable<PXResult<INItemPlan, INPlanType>> list, string POReceiptType, string POReceiptNbr);
        public OnBeforeSalesOrderProcessPOReceipt onBeforeSalesOrderProcessPOReceipt;
		[InjectDependency]
		public IInventoryAccountService InventoryAccountService { get; set; }

		#region DAC Overrides
		[PXCustomizeBaseAttribute(typeof(AccountAttribute), nameof(AccountAttribute.Visible), false)]
		protected virtual void POReceiptLine_POAccrualAcctID_CacheAttached(PXCache sender) { }

		[PXCustomizeBaseAttribute(typeof(SubAccountAttribute), nameof(SubAccountAttribute.Visible), false)]
		protected virtual void POReceiptLine_POAccrualSubID_CacheAttached(PXCache sender) { }

		#region SOOrderShipment
		[PXDBInt()]
		[PXDBDefault(typeof(SOAddress.addressID))]
		protected virtual void SOOrderShipment_ShipAddressID_CacheAttached(PXCache sender) { }
		#endregion

		#region SiteID
		[IN.Site(DisplayName = "From Warehouse", DescriptionField = typeof(INSite.descr))]
        protected virtual void INRegister_SiteID_CacheAttached(PXCache sender) { }
		#endregion
        #region ToSiteID
        [IN.ToSite(DisplayName = "To Warehouse", DescriptionField = typeof(INSite.descr))]
        protected virtual void INRegister_ToSiteID_CacheAttached(PXCache sender) { }
        #endregion
        #endregion

        #region Selects
        [PXViewName(Messages.POReceipt)]
		public PXSelectJoin<POReceipt,
			LeftJoinSingleTable<Vendor, On<Vendor.bAccountID, Equal<POReceipt.vendorID>>>,
			Where<POReceipt.receiptType, Equal<Optional<POReceipt.receiptType>>,
			And<Where<Vendor.bAccountID, IsNull,
			Or<Match<Vendor, Current<AccessInfo.userName>>>>>>> Document;
		public PXSelect<POReceipt, Where<POReceipt.receiptType, Equal<Current<POReceipt.receiptType>>,
				And<POReceipt.receiptNbr, Equal<Current<POReceipt.receiptNbr>>>>> CurrentDocument;
		[PXViewName(Messages.POReceiptLine)]
		public PXOrderedSelect<POReceipt, POReceiptLine, Where<POReceiptLine.receiptType, Equal<Current<POReceipt.receiptType>>, And<POReceiptLine.receiptNbr, Equal<Current<POReceipt.receiptNbr>>>>,
			OrderBy<Asc<POReceiptLine.receiptType, Asc<POReceiptLine.receiptNbr, Asc<POReceiptLine.sortOrder, Asc<POReceiptLine.lineNbr>>>>>> transactions;

		public PXSelectJoin<POReceiptLine,
				LeftJoin<POLine,
					On<POLine.orderType, Equal<POReceiptLine.pOType>,
						And<POLine.orderNbr, Equal<POReceiptLine.pONbr>,
						And<POLine.lineNbr, Equal<POReceiptLine.pOLineNbr>>>>>,
				Where<
					POReceiptLine.receiptType, Equal<Current<POReceipt.receiptType>>,
					And<POReceiptLine.receiptNbr, Equal<Current<POReceipt.receiptNbr>>>>,
				OrderBy<
					Asc<POReceiptLine.receiptType,
					Asc<POReceiptLine.receiptNbr,
					Asc<POReceiptLine.lineNbr>>>>>
		transactionsPOLine;

		[PXCopyPasteHiddenView()]
		public PXSelect<POReceiptLineSplit, Where<POReceiptLineSplit.receiptType, Equal<Current<POReceiptLine.receiptType>>,
			And<POReceiptLineSplit.receiptNbr, Equal<Current<POReceiptLine.receiptNbr>>,
											And<POReceiptLineSplit.lineNbr, Equal<Current<POReceiptLine.lineNbr>>,
											And<Where<POLineType.Goods.Contains<POReceiptLine.lineType.FromCurrent>>>>>>> splits;

		public PXSetup<POSetup> posetup;
		public PXSetup<APSetup> apsetup;
		public PXSetupOptional<INSetup> insetup;
	    public PXSetupOptional<CommonSetup> commonsetup;
		public PXSetup<GL.Company> company;
		public PXSetup<Branch>.Where<Branch.bAccountID.IsEqual<POReceipt.vendorID.AsOptional>> branch;
		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<POReceipt.curyInfoID>>>> Current_currencyinfo;

		[PXViewName(AP.Messages.Vendor)]
		public PXSetup<Vendor>.Where<Vendor.bAccountID.IsEqual<POReceipt.vendorID.AsOptional>> vendor;
		[PXViewName(AP.Messages.VendorClass)]
		public PXSetup<VendorClass>.Where<VendorClass.vendorClassID.IsEqual<Vendor.vendorClassID.FromCurrent>> vendorclass;
		
		[PXViewName(AP.Messages.VendorLocation)]
		public PXSetup<Location>.Where<Location.bAccountID.IsEqual<POReceipt.vendorID.FromCurrent>.And<Location.locationID.IsEqual<POReceipt.vendorLocationID.AsOptional>>> location;

		[PXCopyPasteHiddenView()]
		public PXSelect<POOrderReceipt, Where<POOrderReceipt.receiptType, Equal<Current<POReceipt.receiptType>>, And<POOrderReceipt.receiptNbr, Equal<Current<POReceipt.receiptNbr>>>>> ReceiptOrders;

		[PXCopyPasteHiddenView()]
		public PXSelect<POOrderReceiptLink, Where<POOrderReceiptLink.receiptType, Equal<Current<POReceipt.receiptType>>, And<POOrderReceiptLink.receiptNbr, Equal<Current<POReceipt.receiptNbr>>>>> ReceiptOrdersLink;

		[PXCopyPasteHiddenView]
		public 
			PXSelectReadonly<INRegister,
			Where<INRegister.docType, Equal<INDocType.transfer>,
				And<INRegister.transferType, Equal<INTransferType.oneStep>,
				And<INRegister.pOReceiptType, Equal<Current<POReceipt.receiptType>>,
				And<INRegister.pOReceiptNbr, Equal<Current<POReceipt.receiptNbr>>>>>>,
			OrderBy<Desc<INRegister.createdDateTime>>>
			RelatedTransfers;

		public PXSelect<POLineR> purchaseLinesUPD;
		
		public PXSelect<POOrder, Where<POOrder.orderType, NotEqual<POOrderType.regularSubcontract>>> poOrderUPD;

		public PXSelect<POReceiptLineReturnUpdate> poReceiptReturnUpdate;

		public PXSelect<POItemCostManager.POVendorInventoryPriceUpdate> priceStatus;

		public PX.Objects.PO.GraphExtensions.POReceiptEntryExt.POReceiptLineSplittingExtension LineSplittingExt
			=> FindImplementation<PX.Objects.PO.GraphExtensions.POReceiptEntryExt.POReceiptLineSplittingExtension>();
		
		public PXSelect<SOLine4> solineselect;
        public PXSelect<SOLineSplit> solinesplitselect;
		public PXSelect<SOOrder> soorderselect;
		public PXSelect<SOAddress> soaddressselect;
		public PXSelect<SOOrderShipment> ordershipmentselect;
		public PXSelect<SOOrderSite> orderSite;
        public PXSelect<INItemSite> initemsite;
		
		public PXSelect<POLine> poline;
		public PXSelect<INItemXRef> xrefs;

		public PXSelect<POReceiptLandedCostDetail, Where<POReceiptLandedCostDetail.pOReceiptType, Equal<Current<POReceipt.receiptType>>,
				And<POReceiptLandedCostDetail.pOReceiptNbr, Equal<Current<POReceipt.receiptNbr>>>>,
			OrderBy<Desc<POReceiptLandedCostDetail.docDate, Desc<POReceiptLandedCostDetail.lCRefNbr>>>> landedCosts;

		[Api.Export.PXOptimizationBehavior(IgnoreBqlDelegate = true)]
		protected virtual IEnumerable Transactions()
		{
			PrefetchWithDetails();
			return null;
		}

		public virtual void PrefetchWithDetails()
		{
		}

		public PXSelect<POReceiptAPDoc> apDocs;

		public virtual IEnumerable ApDocs()
		{
			if (Document.Current == null)
				return Enumerable.Empty<POReceiptAPDoc>();

			var rawdata = PXSelectJoin<POReceiptLine,
				InnerJoin<APTran, On<APTran.pOAccrualType, Equal<POReceiptLine.pOAccrualType>,
					And<APTran.pOAccrualRefNoteID, Equal<POReceiptLine.pOAccrualRefNoteID>,
					And<APTran.pOAccrualLineNbr, Equal<POReceiptLine.pOAccrualLineNbr>>>>,
				LeftJoin<POAccrualSplit, On<POAccrualSplit.type, Equal<APTran.pOAccrualType>,
					And<POAccrualSplit.refNoteID, Equal<APTran.pOAccrualRefNoteID>,
					And<POAccrualSplit.lineNbr, Equal<APTran.pOAccrualLineNbr>,
					And<POAccrualSplit.aPDocType, Equal<APTran.tranType>,
					And<POAccrualSplit.aPRefNbr, Equal<APTran.refNbr>,
					And<POAccrualSplit.aPLineNbr, Equal<APTran.lineNbr>,
					And<POAccrualSplit.FK.ReceiptLine>>>>>>>>>,
				Where<
					POReceiptLine.FK.Receipt.SameAsCurrent>>.Select(this).AsEnumerable();

			string poReceiptType = Document.Current?.ReceiptType;
			string poReceiptNbr = Document.Current?.ReceiptNbr;

			var aptrans = rawdata
				 .Select(a => PXResult.Unwrap<APTran>(a))
				 .ToList();

			var filtered = rawdata.AsEnumerable()
				.Where(a => {
					var b = PXResult.Unwrap<POReceiptLine>(a);
					var c = PXResult.Unwrap<APTran>(a);
					var d = PXResult.Unwrap<POAccrualSplit>(a);
					// filter AP Bill lines which are already applied to our Receipt lines
					return b.POAccrualType == POAccrualType.Receipt
						|| d.POReceiptNbr != null && d.APRefNbr != null
						// or potentially might be applied
						|| b.UnbilledQty != 0m && (c.Qty != 0m && c.Released == false || c.UnreceivedQty != 0);
				}).ToList();

			var links = filtered
				.Select(a => PXResult.Unwrap<APTran>(a))
				.GroupBy(a => new { DocType = a.TranType, RefNbr = a.RefNbr })
				.Select(a => {
					APInvoice doc = APInvoice.PK.Find(this, a.Key.DocType, a.Key.RefNbr);

					return new POReceiptAPDoc
					{
						DocType = a.Key.DocType,
						RefNbr = a.Key.RefNbr,
						Status = doc?.Status,
						DocDate = doc?.DocDate,
						AccruedQty = filtered
							.Select(b => PXResult.Unwrap<POAccrualSplit>(b))
							.Where(b => b.APDocType == a.Key.DocType && b.APRefNbr == a.Key.RefNbr)
							.Sum(b => b.BaseAccruedQty) ?? 0m,
						AccruedAmt = filtered
							.Select(b => PXResult.Unwrap<POAccrualSplit>(b))
							.Where(b => b.APDocType == a.Key.DocType && b.APRefNbr == a.Key.RefNbr)
							.Sum(b => b.AccruedCost) ?? 0m,
						TotalPPVAmt = filtered
							.Select(b => PXResult.Unwrap<POAccrualSplit>(b))
							.Where(b => b.APDocType == a.Key.DocType && b.APRefNbr == a.Key.RefNbr)
							.Sum(b => b.PPVAmt) ?? 0m,
						TotalQty = aptrans
							.Where(b => b.TranType == a.Key.DocType && b.RefNbr == a.Key.RefNbr)
							.Sum(b => b.BaseQty * b.Sign),
						TotalAmt = aptrans
							.Where(b => b.TranType == a.Key.DocType && b.RefNbr == a.Key.RefNbr)
							.Sum(b => b.TranAmt * b.Sign)
					};
				}).ToList();

			object returnValue = null;
			Caches[typeof(APTran)].RaiseFieldSelecting<APTran.tranAmt>(aptrans.FirstOrDefault(), ref returnValue, true);
			int currencyPrecision = returnValue is PXDecimalState state ? state.Precision : 0;

			string statusMsg;
			if (PXAccess.FeatureInstalled<FeaturesSet.inventory>())
			{
				statusMsg = PXMessages.LocalizeFormatNoPrefix(Messages.StatusTotalBilledTotalAccruedTotalPPV,
					FormatQty(links.Sum(t => t.TotalQty)), FormatAmt((decimal)links.Sum(t => t.TotalAmt), currencyPrecision), FormatQty(links.Sum(t => t.AccruedQty)),
					FormatAmt((decimal)links.Sum(t => t.AccruedAmt), currencyPrecision), FormatAmt((decimal)links.Sum(t => t.TotalPPVAmt), currencyPrecision));
			}
			else
			{
				statusMsg = PXMessages.LocalizeFormatNoPrefix(Messages.StatusTotalBilledTotalAccrued,
					FormatQty(links.Sum(t => t.TotalQty)), FormatAmt((decimal)links.Sum(t => t.TotalAmt), currencyPrecision), FormatQty(links.Sum(t => t.AccruedQty)),
					FormatAmt((decimal)links.Sum(t => t.AccruedAmt), currencyPrecision));
			}

			foreach (var receiptApDoc in links)
			{
				receiptApDoc.StatusText = statusMsg;
			}

			return links;
		}

		public PXSelect<POReceiptPOOriginal> receiptHistory;

		public IEnumerable ReceiptHistory()
		{
			var query = new PXSelectJoinGroupBy<POReceipt,
				InnerJoin<POReceiptLine,
					On<POReceiptLine.FK.Receipt>>,
				Where<
					POReceiptLine.origReceiptType, Equal<Current<POReceipt.receiptType>>,
					And<POReceiptLine.origReceiptNbr, Equal<Current<POReceipt.receiptNbr>>>>,
				Aggregate<GroupBy<POReceipt.receiptType, GroupBy<POReceipt.receiptNbr, Sum<POReceiptLine.baseReceiptQty>>>>>(this);

			var result = query.Select().Select(t => (PXResult<POReceipt, POReceiptLine>) t).Select(t => new POReceiptPOOriginal
			{
				ReceiptType = ((POReceipt) t).ReceiptType,
				ReceiptNbr = ((POReceipt) t).ReceiptNbr,
				DocDate = ((POReceipt) t).ReceiptDate,
				FinPeriodID = ((POReceipt) t).FinPeriodID,
				Status = ((POReceipt) t).Status,
				InvtDocType = ((POReceipt) t).InvtDocType,
				InvtRefNbr = ((POReceipt) t).InvtRefNbr,
				TotalQty = ((POReceiptLine) t).BaseReceiptQty,
			});

			return result;
		}


		public virtual string FormatQty(decimal? value)
		{
			return (value == null) ? string.Empty : ((decimal)value).ToString("N" + CommonSetupDecPl.Qty.ToString(), System.Globalization.NumberFormatInfo.CurrentInfo);
		}

		public virtual string FormatAmt(decimal? value, int precision)
		{
			return (value == null) ? string.Empty : ((decimal)value).ToString("N" + precision.ToString(), System.Globalization.NumberFormatInfo.CurrentInfo);
		}

		#region Add PO Order sub-form
		public PXFilter<POOrderFilter> filter;
		public PXFilter<POReceiptLineS> addReceipt;

		[PXCopyPasteHiddenView()]
		public PXSelectJoin<
			POLineS,
			InnerJoin<POOrder, 
				On<POOrder.orderType, Equal<POReceiptEntry.POLineS.orderType>, 
					And<POOrder.orderNbr, Equal<POReceiptEntry.POLineS.orderNbr>>>>,
			Where<POLineS.orderType, Equal<Current<POOrderFilter.orderType>>,
				And<POLineS.lineType, NotEqual<POLineType.description>,
				And2<Where<POLineS.orderType, NotEqual<POOrderType.projectDropShip>,
					Or<POLineS.dropshipReceiptProcessing, Equal<DropshipReceiptProcessingOption.generateReceipt>>>,
				And2<Where<Current<POOrderFilter.vendorID>, IsNull,
					Or<POLineS.vendorID, Equal<Current<POOrderFilter.vendorID>>>>,
				And2<Where<Current<POOrderFilter.vendorLocationID>, IsNull,
					Or<POOrder.vendorLocationID, Equal<Current<POOrderFilter.vendorLocationID>>>>,
				And2<Where<Current<POOrderFilter.orderNbr>, IsNull,
					Or<POLineS.orderNbr, Equal<Current<POOrderFilter.orderNbr>>>>,
				And2<Where<Current<POOrderFilter.inventoryID>, IsNull,
					Or<POLineS.inventoryID, Equal<Current<POOrderFilter.inventoryID>>>>,
				And2<Where<Current<POOrderFilter.subItemID>, IsNull,
					Or<POLineS.subItemID, Equal<Current<POOrderFilter.subItemID>>,
					Or<Not<FeatureInstalled<FeaturesSet.subItem>>>>>,
				And2<Where<Current<APSetup.requireSingleProjectPerDocument>, Equal<boolFalse>,
					Or<POLineS.projectID, Equal<Current<POReceipt.projectID>>>>,
				And<POOrder.status, Equal<POOrderStatus.open>,
				And<POLineS.completed, Equal<boolFalse>,
				And<POLineS.cancelled, Equal<boolFalse>,
				And<POOrder.curyID, Equal<Current<POReceipt.curyID>>>>>>>>>>>>>>>,
			OrderBy<Asc<POLineS.sortOrder>>> poLinesSelection;
		
		[PXReadOnlyView]
		[PXCopyPasteHiddenView()]
        public PXSelectJoin<INTran,
            InnerJoin<INRegister, On<INTran.FK.INRegister>,
            InnerJoin<INSite, On<INRegister.FK.ToSite>,
            InnerJoin<INTranInTransit, On<INTranType.transfer, Equal<INTran.tranType>, And<INTranInTransit.refNbr, Equal<INTran.refNbr>, And<INTranInTransit.lineNbr, Equal<INTran.lineNbr>>>>,
            LeftJoin<INTran2, On<INTran2.released, NotEqual<True>, 
                And<INTran2.origLineNbr, Equal<INTranInTransit.lineNbr>,
                And<INTran2.origRefNbr, Equal<INTranInTransit.refNbr>,
                And<INTran2.origTranType, Equal<INTranType.transfer>>>>>,
            LeftJoin<POReceiptLine, On<POReceiptLine.released, NotEqual<True>,
                And<POReceiptLine.origLineNbr, Equal<INTranInTransit.lineNbr>,
                And<POReceiptLine.origRefNbr, Equal<INTranInTransit.refNbr>,
                And<POReceiptLine.origTranType, Equal<INTranType.transfer>>>>>>>>>>,
            Where<INRegister.origModule, Equal<GL.BatchModule.moduleSO>,
                And<POReceiptLine.receiptNbr, IsNull,
                And<INTran2.refNbr, IsNull,
                And<INRegister.docType, Equal<INDocType.transfer>,
                And<INRegister.released, Equal<True>,
                And2<Where<Current<POReceipt.siteID>, IsNull,
                    Or<INRegister.toSiteID, Equal<Current<POReceipt.siteID>>>>,
                And2<Match<INSite, Current<AccessInfo.userName>>,
                And2<Where<Current<POOrderFilter.shipFromSiteID>, IsNull,
                    Or<INRegister.siteID, Equal<Current<POOrderFilter.shipFromSiteID>>>>,
                And2<Where<Current<POOrderFilter.sOOrderNbr>, IsNull,
                                Or<INTran.sOOrderType, Equal<SOOrderTypeConstants.transferOrder>, And<INTran.sOOrderNbr, Equal<Current<POOrderFilter.sOOrderNbr>>>>>,
                And2<Where<Current<POOrderFilter.inventoryID>, IsNull,
                                Or<INTran.inventoryID, Equal<Current<POOrderFilter.inventoryID>>>>,
                And<Where<Current<POOrderFilter.subItemID>, IsNull,
													Or<INTran.subItemID, Equal<Current<POOrderFilter.subItemID>>>>>>>>>>>>>>>> intranSelection;

		[PXCopyPasteHiddenView]
		public PXSelect<
			POOrderS,
			Where<POOrderS.hold, Equal<boolFalse>,
				And<POOrderS.status, NotEqual<POOrderStatus.awaitingLink>,
				And<POOrder.orderType, NotEqual<POOrderType.regularSubcontract>,
				And2<Where<Current<POReceipt.vendorID>, IsNull,
					Or<POOrderS.vendorID, Equal<Current<POReceipt.vendorID>>>>,
				And2<Where<Current<POReceipt.vendorLocationID>, IsNull,
					Or<POOrderS.vendorLocationID, Equal<Current<POReceipt.vendorLocationID>>>>,
				And2<Where<Current<POOrderFilter.vendorID>, IsNull,
					Or<POOrderS.vendorID, Equal<Current<POOrderFilter.vendorID>>>>,
				And2<Where<Current<POOrderFilter.vendorLocationID>, IsNull,
					Or<POOrderS.vendorLocationID, Equal<Current<POOrderFilter.vendorLocationID>>>>,
				And2<Where2<
					Where<Current<POOrderFilter.orderType>, IsNull,
						And<Where<POOrderS.orderType, Equal<POOrderType.regularOrder>,
							Or<POOrderS.orderType, Equal<POOrderType.dropShip>>>>>,
						Or<POOrderS.orderType, Equal<Current<POOrderFilter.orderType>>>>,
				And2<Where<POOrderS.orderType, NotEqual<POOrderType.projectDropShip>, 
					Or<POOrderS.dropshipReceiptProcessing, Equal<DropshipReceiptProcessingOption.generateReceipt>>>,
				And2<Where<POOrderS.shipToBAccountID, Equal<Current<POOrderFilter.shipToBAccountID>>,
					Or<Current<POOrderFilter.shipToBAccountID>, IsNull>>,
				And2<Where<POOrderS.shipToLocationID, Equal<Current<POOrderFilter.shipToLocationID>>,
					Or<Current<POOrderFilter.shipToLocationID>, IsNull>>,
				And2<Where<Current<APSetup.requireSingleProjectPerDocument>, Equal<boolFalse>,
					Or<POOrderS.projectID, Equal<Current<POReceipt.projectID>>>>,
				And<POOrderS.cancelled, Equal<boolFalse>,
				And<POOrderS.status, Equal<POOrderStatus.open>,
				And<POOrderS.curyID, Equal<Current<POReceipt.curyID>>>>>>>>>>>>>>>>>,
			OrderBy<Asc<POOrderS.orderDate>>> openOrders;

		[PXCopyPasteHiddenView()]
		public PXSelectJoin<SOOrderShipment, 
				InnerJoin<INRegister,
					On< INRegister.docType, Equal<SOOrderShipment.invtDocType>,
					And<INRegister.refNbr, Equal<SOOrderShipment.invtRefNbr>>>,
				InnerJoin<INTransferInTransit,
					On< INRegister.docType, Equal<INDocType.transfer>,
					And<INRegister.refNbr, Equal<INTransferInTransit.transferNbr>>>,
				InnerJoin<SOOrder, On<SOOrderShipment.FK.Order>>>>,
				Where<INRegister.origModule, Equal<GL.BatchModule.moduleSO>,
					And<INRegister.docType, Equal<INDocType.transfer>,
					And<INRegister.released, Equal<True>>>>> openTransfers;

		[PXCopyPasteHiddenView()]
		public PXSelect<INRegister> dummyOpenTransfers; //used only for CacheAttached

        public PXSelect<INCostSubItemXRef> costsubitemxref;

		public IEnumerable OpenTransfers()
		{
			Dictionary<POReceiptLine, int> usedTransfer = new Dictionary<POReceiptLine, int>(new SOOrderShipmentComparer());

			int count;
			foreach (POReceiptLine receiptLine in transactions.Cache.Inserted)
			{
				if (receiptLine.OrigRefNbr != null)
				{
					usedTransfer.TryGetValue(receiptLine, out count);
					usedTransfer[receiptLine] = count + 1;
				}
			}

			foreach (POReceiptLine receiptLine in transactions.Cache.Deleted)
			{
				if (receiptLine.OrigRefNbr != null && transactions.Cache.GetStatus(receiptLine) != PXEntryStatus.InsertedDeleted)
				{
					usedTransfer.TryGetValue(receiptLine, out count);
					usedTransfer[receiptLine] = count - 1;
				}
			}

			BqlCommand selectSites = BqlCommand.CreateInstance(typeof(Select<INSite, Where<MatchWithBranch<INSite.branchID>>>));
			PXResultset<SOOrderShipment> results;
			PXCache cache = this.Caches[typeof(INSite)];
			using (new PXReadBranchRestrictedScope())
			{ 
				results = 
					PXSelectJoinGroupBy<SOOrderShipment,
						InnerJoin<INRegister, On<INRegister.docType, Equal<SOOrderShipment.invtDocType>, And<INRegister.refNbr, Equal<SOOrderShipment.invtRefNbr>>>,
						InnerJoin<INTransferInTransitSO, On<INTransferInTransitSO.sOShipmentNbr, Equal<SOOrderShipment.shipmentNbr>>,
						InnerJoin<INSite, On<INSite.siteID, Equal<INTransferInTransitSO.toSiteID>>,
						InnerJoin<SOOrder, On<SOOrderShipment.FK.Order>>>>>,
						Where<INTransferInTransitSO.origModule, Equal<GL.BatchModule.moduleSO>,
						And<INRegister.docType, Equal<INDocType.transfer>,
						And<SOOrderShipment.shipmentType, Equal<SOShipmentType.transfer>,
						And<INRegister.released, Equal<True>,
						And2<Match<INSite, Current<AccessInfo.userName>>,
						And<INTransferInTransitSO.toSiteID, Equal<Current<POReceipt.siteID>>,
						And<Where<Current<POOrderFilter.shipFromSiteID>, IsNull,
							Or<INTransferInTransitSO.fromSiteID, Equal<Current<POOrderFilter.shipFromSiteID>>>>>>>>>>>,
					Aggregate<
						GroupBy<SOOrderShipment.shipmentNbr,
						GroupBy<SOOrderShipment.orderNbr,
						GroupBy<SOOrderShipment.orderType,
						Count>>>>>
					.Select(this);
			}

			foreach (var result in results)
			{
				if (selectSites.Meet(cache, PXResult.Unwrap<INSite>(result)))
				{
					POReceiptLine receiptLine = new POReceiptLine();
					receiptLine.SOOrderType = ((SOOrderShipment)result).OrderType;
					receiptLine.SOOrderNbr = ((SOOrderShipment)result).OrderNbr;
					receiptLine.SOShipmentNbr = ((SOOrderShipment)result).ShipmentNbr;
					if (usedTransfer.TryGetValue(receiptLine, out count))
					{
						usedTransfer.Remove(receiptLine);
						if (count < result.RowCount)
							yield return new PXResult<SOOrderShipment, INRegister, SOOrder>((SOOrderShipment)result, PXResult.Unwrap<INRegister>(result), PXResult.Unwrap<SOOrder>(result));
			}
					else
						yield return new PXResult<SOOrderShipment, INRegister, SOOrder>((SOOrderShipment)result, PXResult.Unwrap<INRegister>(result), PXResult.Unwrap<SOOrder>(result));

				}
			}

			foreach (POReceiptLine deletedTran in usedTransfer.Where(_ => _.Value < 0).Select(_ => _.Key))
			{
				yield return (PXResult<SOOrderShipment, INRegister, SOOrder>)
					PXSelectJoin<SOOrderShipment,
						InnerJoin<INRegister, On<INRegister.docType, Equal<SOOrderShipment.invtDocType>, And<INRegister.refNbr, Equal<SOOrderShipment.invtRefNbr>>>,
						InnerJoin<SOOrder, On<SOOrderShipment.FK.Order>>>,
					Where<SOOrderShipment.invtRefNbr, Equal<Required<SOOrderShipment.invtRefNbr>>,
					And<SOOrderShipment.invtDocType, Equal<INDocType.transfer>, 
					And<SOOrderShipment.orderNbr, Equal<Required<SOOrderShipment.orderNbr>>,
					And<SOOrderShipment.orderType, Equal<Required<SOOrderShipment.orderType>>>>>>>.Select(this, deletedTran.OrigRefNbr, deletedTran.SOOrderNbr, deletedTran.SOOrderType);
			}


		}

		#endregion

		#region Add Return sub-form

		public PXFilter<POReceiptReturnFilter> returnFilter;

		[PXCopyPasteHiddenView]
		public PXSelectOrderBy<POReceiptReturn, OrderBy<Desc<POReceiptReturn.receiptNbr>>> poReceiptReturn;

		protected virtual IEnumerable pOReceiptReturn()
		{
			// delegate is used because of read-only PXSelectJoinGroupBy and the need to set Selected on the lines
			var query = new PXSelectJoinGroupBy<POReceiptReturn,
				LeftJoin<POOrderReceipt,
					On<POOrderReceipt.receiptType, Equal<POReceiptReturn.receiptType>,
					And<POOrderReceipt.receiptNbr, Equal<POReceiptReturn.receiptNbr>>>,
				InnerJoin<POReceiptLineReturn,
					On<POReceiptLineReturn.receiptType, Equal<POReceiptReturn.receiptType>,
					And<POReceiptLineReturn.receiptNbr, Equal<POReceiptReturn.receiptNbr>>>,
				LeftJoin<POReceiptLine,
					On<POReceiptLine.origReceiptType, Equal<POReceiptLineReturn.receiptType>,
					And<POReceiptLine.origReceiptNbr, Equal<POReceiptLineReturn.receiptNbr>,
					And<POReceiptLine.origReceiptLineNbr, Equal<POReceiptLineReturn.lineNbr>,
					And<POReceiptLine.released, Equal<False>>>>>>>>,
				Where<POReceiptReturn.receiptType, Equal<POReceiptType.poreceipt>,
					And<POReceiptReturn.released, Equal<True>,
					And<POReceiptReturn.vendorID, Equal<Current<POReceipt.vendorID>>,
					And<POReceiptReturn.vendorLocationID, Equal<Current<POReceipt.vendorLocationID>>,
					And2<Where<POReceiptLine.receiptNbr, IsNull,
						Or<POReceiptLine.FK.Receipt.SameAsCurrent>>,
					And2<Where<Current<POReceiptReturnFilter.orderType>, IsNull,
						Or<POOrderReceipt.pOType, Equal<Current<POReceiptReturnFilter.orderType>>>>,
					And2<Where<Current<POReceiptReturnFilter.orderNbr>, IsNull,
						Or<POOrderReceipt.pONbr, Equal<Current<POReceiptReturnFilter.orderNbr>>>>,
					And<Where<Current<POReceiptReturnFilter.receiptNbr>, IsNull,
						Or<POReceiptReturn.receiptNbr, Equal<Current<POReceiptReturnFilter.receiptNbr>>>>>>>>>>>>,
				Aggregate<
					GroupBy<POReceiptReturn.receiptNbr,
					GroupBy<POReceiptReturn.released, 
					Count<POReceiptLineReturn.lineNbr>>>>,
				OrderBy<Desc<POReceiptReturn.receiptNbr>>>(this);

			var currentReceiptNbrs = transactions.Select().RowCast<POReceiptLine>().GroupBy(t => new
				{
					ReceiptType = t.OrigReceiptType,
					ReceiptNbr = t.OrigReceiptNbr
				})
				.Select(t => new
				{
					t.Key.ReceiptType,
					t.Key.ReceiptNbr,
					RowCount = t.Count()
				}).ToArray();

			int startRow = PXView.StartRow;
			int totalRows = 0;
			int maximumRows = PXView.MaximumRows;

			if (currentReceiptNbrs.Any())
			{
				maximumRows += currentReceiptNbrs.Length;
			}

			var raw = query.View.Select(
				PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns,
				PXView.Descendings, PXView.Filters, ref startRow, maximumRows, ref totalRows);
			PXView.StartRow = 0;

			var ret = raw
				.Select(o => (PXResult<POReceiptReturn, POOrderReceipt, POReceiptLineReturn>)o)
				.Where(t => t.RowCount > 0)
				.Where(t => !currentReceiptNbrs.Any(m =>
					m.ReceiptType == ((POReceiptReturn)t).ReceiptType
					&& m.ReceiptNbr == ((POReceiptReturn)t).ReceiptNbr
					&& m.RowCount >= t.RowCount.Value))
				.Where(c => ((POReceiptReturn)c).CuryID == Document.Current?.CuryID)
				.Select(o => (POReceiptReturn)o)
				.Select(rct => poReceiptReturn.Cache.Updated.RowCast<POReceiptReturn>().FirstOrDefault(u =>
					u.ReceiptType == rct.ReceiptType
					&& u.ReceiptNbr == rct.ReceiptNbr) ?? rct)
				.ToList();

			return ret;
		}

		[PXCopyPasteHiddenView]
		public PXSelectOrderBy<POReceiptLineReturn, OrderBy<Desc<POReceiptLineReturn.receiptNbr, Asc<POReceiptLineReturn.lineNbr>>>> poReceiptLineReturn;

		protected virtual IEnumerable PoReceiptLineReturn()
		{
			PXSelectBase<POReceiptLineReturn> cmd = new PXSelectJoin<POReceiptLineReturn,
				LeftJoin<POReceiptLine,
					On<POReceiptLine.origReceiptType, Equal<POReceiptLineReturn.receiptType>,
					And<POReceiptLine.origReceiptNbr, Equal<POReceiptLineReturn.receiptNbr>,
					And<POReceiptLine.origReceiptLineNbr, Equal<POReceiptLineReturn.lineNbr>,
					And<POReceiptLine.released, Equal<False>>>>>>,
				Where< POReceiptLineReturn.vendorID, Equal<Current<POReceipt.vendorID>>,
					And<POReceiptLineReturn.vendorLocationID, Equal<Current<POReceipt.vendorLocationID>>,
					And2<Where<POReceiptLine.receiptNbr, IsNull,
						Or<POReceiptLine.FK.Receipt.SameAsCurrent>>,
					And2<Where<Current<POReceiptReturnFilter.orderType>, IsNull, Or<POReceiptLineReturn.pOType, Equal<Current<POReceiptReturnFilter.orderType>>>>,
					And2<Where<Current<POReceiptReturnFilter.orderNbr>, IsNull, Or<POReceiptLineReturn.pONbr, Equal<Current<POReceiptReturnFilter.orderNbr>>>>,
					And2<Where<Current<POReceiptReturnFilter.receiptNbr>, IsNull, Or<POReceiptLineReturn.receiptNbr, Equal<Current<POReceiptReturnFilter.receiptNbr>>>>,
					And<Where<Current<POReceiptReturnFilter.inventoryID>, IsNull, Or<POReceiptLineReturn.inventoryID, Equal<Current<POReceiptReturnFilter.inventoryID>>>>>>>>>>>,
				OrderBy<Desc<POReceiptLineReturn.receiptNbr, Asc<POReceiptLineReturn.lineNbr>>>>(this);

			var currentReceiptLines = transactions.Select().RowCast<POReceiptLine>()
				.Select(t => new
				{
					POReceiptType = t.OrigReceiptType,
					POReceiptNbr = t.OrigReceiptNbr,
					POReceiptLineNbr = t.OrigReceiptLineNbr
				}).ToArray();

			int startRow = PXView.StartRow;
			int totalRows = 0;
			int maximumRows = PXView.MaximumRows;

			if (currentReceiptLines.Any() && maximumRows > 0)
			{
				maximumRows += currentReceiptLines.Length;
			}

			var result = cmd.View.Select(PXView.Currents, PXView.Parameters, new object[PXView.SortColumns.Length],
				PXView.SortColumns,
				PXView.Descendings, PXView.Filters, ref startRow, maximumRows, ref totalRows).RowCast<POReceiptLineReturn>().ToList();
			PXView.StartRow = 0;

			result = result.Where(t =>
				!currentReceiptLines.Contains(new
				{
					POReceiptType = t.ReceiptType,
					POReceiptNbr = t.ReceiptNbr,
					POReceiptLineNbr = t.LineNbr
				}))
				.Where(c => ((POReceiptLineReturn)c).CuryID == Document.Current?.CuryID)
				.ToList();

			return result;
		}

		#endregion
		#endregion

		#region Custom Buttons

		public PXInitializeState<POReceipt> initializeState;

		public PXAction<POReceipt> putOnHold;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Hold")]
		protected virtual IEnumerable PutOnHold(PXAdapter adapter) => adapter.Get<POReceipt>();

		public PXAction<POReceipt> releaseFromHold;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Remove Hold")]
		protected virtual IEnumerable ReleaseFromHold(PXAdapter adapter) => adapter.Get<POReceipt>();

		public new PXAction<POReceipt> Cancel;
		[PXUIField(DisplayName = ActionsMessages.Cancel, MapEnableRights = PXCacheRights.Select)]
		[PXCancelButton]
		protected virtual IEnumerable cancel(PXAdapter adapter)
		{
			//Reports an error if user enters number of existing document with different type
			foreach (object it in (new PXCancel<POReceipt>(this, "Cancel")).Press(adapter))
			{
				POReceipt rct = PXResult.Unwrap<POReceipt>(it);
				if (!String.IsNullOrEmpty(rct.ReceiptNbr))
				{
					POReceipt another = POReceipt.PK.Find(this, rct);
					if (another != null && another.ReceiptType != rct.ReceiptType)
					{
						POReceiptType.ListAttribute namesList = new POReceiptType.ListAttribute();
						string typeName = namesList.ValueLabelDic[rct.ReceiptType];
						this.Document.Cache.RaiseExceptionHandling<POReceipt.receiptNbr>(rct, rct.ReceiptNbr, new PXSetPropertyException(Messages.ReceiptNumberBelongsToDocumentHavingDifferentType, rct.ReceiptNbr, typeName, another.ReceiptType));
						this.Document.Current.ReceiptNbr = null;
					}
				}
				yield return it;
			}
		}

		public PXAction<POReceipt> createReturn;
		[PXUIField(DisplayName = Messages.CreateReturn, MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Insert)]
		[PXButton]
		public virtual IEnumerable CreateReturn(PXAdapter adapter)
		{
			var doc = this.Document.Current;
			if (doc?.Released != true)
			{
				throw new PXException(Messages.Document_Status_Invalid);
			}
			var selectedLines = this.transactions.Cache.Updated.RowCast<POReceiptLine>()
				.Where(l => l.Selected == true)
				.OrderBy(l => l.SortOrder).ThenBy(l => l.LineNbr)
				.ToList();
			if (!selectedLines.Any())
			{
				throw new PXException(Messages.SelectLinesForProcessing);
			}

			POReceiptEntry receiptEntry = PXGraph.CreateInstance<POReceiptEntry>();
			POReceipt poReturn = receiptEntry.Document.Insert(
				new POReceipt() { ReceiptType = POReceiptType.POReturn });
			poReturn.BranchID = doc.BranchID;
			poReturn.VendorID = doc.VendorID;
			poReturn.VendorLocationID = doc.VendorLocationID;
			poReturn.ProjectID = doc.ProjectID;
			poReturn = receiptEntry.Document.Update(poReturn);
			if (selectedLines.Any(x => POLineType.IsProjectDropShip(x.LineType)))
			{
				poReturn.ReturnInventoryCostMode = ReturnCostMode.OriginalCost;
			}
			else if (selectedLines.Any(x => x.IsSpecialOrder == true))
			{
				poReturn.ReturnInventoryCostMode = ReturnCostMode.CostByIssue;
			}

			if (poReturn.CuryID != doc.CuryID || poReturn.BranchID != doc.BranchID)
			{
				POReceipt poReturnCopy = (POReceipt)receiptEntry.Document.Cache.CreateCopy(poReturn);
				poReturnCopy.CuryID = doc.CuryID;
				poReturnCopy.BranchID = doc.BranchID;
				poReturn = receiptEntry.Document.Update(poReturnCopy);
			}

			receiptEntry.CopyReceiptCurrencyInfoToReturn(GetCurrencyInfo(doc.CuryInfoID), receiptEntry.GetCurrencyInfo(poReturn.CuryInfoID), poReturn.ReturnInventoryCostMode == ReturnCostMode.OriginalCost);

			foreach (var origLine in selectedLines)
			{
				POReceiptLine returnLine = receiptEntry.AddReturnLine(origLine);
				returnLine.BranchID = origLine.BranchID;
				returnLine = receiptEntry.transactions.Update(returnLine);
			}
			throw new PXRedirectRequiredException(receiptEntry, Messages.POReceiptRedirection);
		}

		public virtual CurrencyInfo GetCurrencyInfo(long? curyInfoID)
		{
			return MultiCurrencyExt.GetCurrencyInfo(curyInfoID);
		}

		public virtual bool IsSameCurrencyInfo(CurrencyInfo curyA, CurrencyInfo curyB)
		{
			if (curyA == null)
				throw new ArgumentNullException(nameof(curyA));

			if (curyB == null)
				throw new ArgumentNullException(nameof(curyB));

			return curyA.CuryID == curyB.CuryID
				&& curyA.CuryEffDate == curyB.CuryEffDate
				&& curyA.CuryRate == curyB.CuryRate
				&& (curyA.CuryMultDiv == curyB.CuryMultDiv || curyA.CuryRate == 1m && curyB.CuryRate == 1m)
				&& curyA.CuryRateTypeID == curyB.CuryRateTypeID;
		}

		public virtual void CopyReceiptCurrencyInfoToReturn(long? receipt_CuryInfoID, long? return_CuryInfoID, bool copyCuryRate = true) =>
			CopyReceiptCurrencyInfoToReturn(GetCurrencyInfo(receipt_CuryInfoID), GetCurrencyInfo(return_CuryInfoID), copyCuryRate);

		public virtual void CopyReceiptCurrencyInfoToReturn(CurrencyInfo receipt_CuryInfo, CurrencyInfo return_CuryInfo, bool copyCuryRate = true)
		{
			if (receipt_CuryInfo != null && return_CuryInfo != null)
			{
				return_CuryInfo.BaseCuryID = receipt_CuryInfo.BaseCuryID;
				return_CuryInfo.CuryID = receipt_CuryInfo.CuryID;
				return_CuryInfo.CuryRateTypeID = receipt_CuryInfo.CuryRateTypeID;
				if (copyCuryRate)
				{
					return_CuryInfo.CuryEffDate = receipt_CuryInfo.CuryEffDate;
					return_CuryInfo.CuryRate = receipt_CuryInfo.CuryRate;
					return_CuryInfo.RecipRate = receipt_CuryInfo.RecipRate;
					return_CuryInfo.CuryMultDiv = receipt_CuryInfo.CuryMultDiv;
				}

				MultiCurrencyExt.currencyinfo.Update(return_CuryInfo);
			}
		}

		public PXAction<POReceipt> release;
        [PXUIField(DisplayName = Messages.Release, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXProcessButton]
        public virtual IEnumerable Release(PXAdapter adapter)
        {
            var list = new List<POReceipt>();
            foreach (POReceipt indoc in adapter.Get<POReceipt>())
            {
                if (indoc.Hold == false && indoc.Released == false)
                {
                    list.Add(Document.Update(indoc));
                }
            }

            if (list.Count == 0)
            {
                throw new PXException(Messages.Document_Status_Invalid);
            }
            Save.Press();
            PXLongOperation.StartOperation(this, delegate() { POReleaseReceipt.ReleaseDoc(list, false); });
            return list;
        }

        [PXViewName(CR.Messages.MainContact)]
        public PXSelect<Contact> DefaultCompanyContact;
        protected virtual IEnumerable defaultCompanyContact()
        {
	        return OrganizationMaint.GetDefaultContactForCurrentOrganization(this);
        }

        [PXViewName(Messages.VendorContact)]
        public PXSelectJoin<Contact, InnerJoin<Vendor, On<Contact.contactID, Equal<Vendor.defContactID>>>, Where<Vendor.bAccountID, Equal<Current<POReceipt.vendorID>>>> contact;
        
        public PXAction<POReceipt> notification;
        [PXUIField(DisplayName = "Notifications", Visible = false)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntryF)]
        protected virtual IEnumerable Notification(PXAdapter adapter,
        [PXString]
		string notificationCD
        )
        {
			bool massProcess = adapter.MassProcess;
			var receipts = adapter.Get<POReceipt>().ToArray();

			PXLongOperation.StartOperation(this, () =>
			{
				var receiptEntry = Lazy.By(() => PXGraph.CreateInstance<POReceiptEntry>());
				bool anyfailed = false;

				foreach (POReceipt receipt in receipts)
				{
					if (massProcess) PXProcessing<POReceipt>.SetCurrentItem(receipt);

					try
					{
						receiptEntry.Value.Clear();
						receiptEntry.Value.Document.Cache.Current = receipt;
						var parameters = new Dictionary<string, string>();
						parameters["POReceipt.ReceiptType"] = receipt.ReceiptType;
						parameters["POReceipt.ReceiptNbr"] = receipt.ReceiptNbr;

						using (var ts = new PXTransactionScope())
						{
							receiptEntry.Value.GetExtension<POReceiptEntry_ActivityDetailsExt>().SendNotification(APNotificationSource.Vendor, notificationCD, receipt.BranchID, parameters, massProcess);

							ts.Complete();
						}

						if (massProcess) PXProcessing<POReceipt>.SetProcessed();
					}
					catch (Exception exception) when (massProcess)
					{
						PXProcessing<POReceipt>.SetError(exception);
						anyfailed = true;
					}
				}

				if (anyfailed)
				{
					throw new PXOperationCompletedWithErrorException(ErrorMessages.SeveralItemsFailed);
				}
			});

			return receipts;
		}

		public PXAction<POReceipt> emailPurchaseReceipt;
		[PXButton(CommitChanges = true)]
		[PXUIField(DisplayName = "Email Purchase Receipt", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable EmailPurchaseReceipt(
			PXAdapter adapter,
			[PXString]
			string notificationCD = null) => Notification(adapter, notificationCD ?? "PURCHASE RECEIPT");

		#region Print actions

		public PXAction<POReceipt> report;
        [PXUIField(DisplayName = "Reports", MapEnableRights = PXCacheRights.Select)]
        [PXButton(SpecialType = PXSpecialButtonType.ReportsFolder, CommitChanges = true, MenuAutoOpen = true)]
        protected virtual IEnumerable Report(PXAdapter adapter,			
			
			[PXString(8, InputMask = "CC.CC.CC.CC")]
			[POReceiptEntryReports]
			string reportID)
        {
            List<POReceipt> list = adapter.Get<POReceipt>().ToList();
			if (string.IsNullOrEmpty(reportID) == false)
			{
                Save.Press();
				int i = 0;
				Dictionary<string, string> parameters = new Dictionary<string, string>();
                foreach (POReceipt doc in list)
				{

					if (reportID == "PO632000")
					{
						parameters["FinPeriodID"] = (string)Document.GetValueExt<POReceipt.finPeriodID>(doc);
                        parameters["ReceiptNbr"] = doc.ReceiptNbr;
					}
					else
					{
                        parameters["ReceiptType"] = doc.ReceiptType;
						parameters["ReceiptNbr"] = doc.ReceiptNbr;
					}
					i++;

				}
				if (i > 0)
				{
					throw new PXReportRequiredException(parameters, reportID,
						PXBaseRedirectException.WindowMode.New, string.Format("Report {0}", reportID));
				}
			}
            return list;
        }

		public PXAction<POReceipt> printPurchaseReceipt;
		[PXButton(CommitChanges = true)]
		[PXUIField(DisplayName = POReceiptEntryReportsAttribute.DisplayNames.PurchaseReceipt, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable PrintPurchaseReceipt(PXAdapter adapter) 
			=> Report(adapter.Apply(a => a.Menu = POReceiptEntryReportsAttribute.DisplayNames.PurchaseReceipt), POReceiptEntryReportsAttribute.Values.PurchaseReceipt);

		public PXAction<POReceipt> printBillingDetail;
		[PXButton(CommitChanges = true)]
		[PXUIField(DisplayName = POReceiptEntryReportsAttribute.DisplayNames.BillingDetails, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable PrintBillingDetail(PXAdapter adapter)
			=> Report(adapter.Apply(a => a.Menu = POReceiptEntryReportsAttribute.DisplayNames.BillingDetails), POReceiptEntryReportsAttribute.Values.BillingDetails);

		public PXAction<POReceipt> printAllocated;
		[PXButton(CommitChanges = true)]
		[PXUIField(DisplayName = POReceiptEntryReportsAttribute.DisplayNames.Allocated, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable PrintAllocated(PXAdapter adapter)
			=> Report(adapter.Apply(a => a.Menu = POReceiptEntryReportsAttribute.DisplayNames.Allocated), POReceiptEntryReportsAttribute.Values.Allocated);

		#endregion

		public PXAction<POReceipt> assign;
		[PXUIField(DisplayName = Messages.Assign, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Enabled = false)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable Assign(PXAdapter adapter)
		{
			if (posetup.Current.DefaultReceiptAssignmentMapID == null)
			{
				throw new PXSetPropertyException(Messages.AssignNotSetup_Receipt, "PO Setup");
			}

			var processor = new EPAssignmentProcessor<POReceipt>();
			processor.Assign(Document.Current, posetup.Current.DefaultReceiptAssignmentMapID);

			Document.Update(Document.Current);

			return adapter.Get();
		}

		public PXAction<POReceipt> addPOOrder;
		[PXUIField(DisplayName = Messages.AddPOOrder, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXLookupButton]
		public virtual IEnumerable AddPOOrder(PXAdapter adapter)
		{
			if (this.Document.Current != null &&
				 this.Document.Current.Released != true)
			{				
                if (this.filter.Current.ResetFilter == true)
				{
					this.filter.Cache.Remove(this.filter.Current);
					this.filter.Cache.Insert(new POOrderFilter());
				}
				else
					this.filter.Cache.RaiseRowSelected(this.filter.Current);

			    if (this.openOrders.AskExt(true) == WebDialogResult.OK)
					return AddPOOrder2(adapter);
			}
			openOrders.Cache.Clear();
			return adapter.Get();
		}

		public PXAction<POReceipt> addPOOrder2;
		[PXUIField(DisplayName = Messages.AddPOOrder, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable AddPOOrder2(PXAdapter adapter)
		{
			if (Document.Current != null && Document.Current.Released != true)
			{
				bool hasError = false;
				foreach (POOrderS it in openOrders.Cache.Updated)
				{
					//Some orders may be filtered out but still be in the updated Cache
					if (it.Selected == true && IsPassingFilter(filter.Current, it))
					{
						string message;
						if (!CanAddOrder(it, out message))
							throw new PXException(Messages.SomeOrdersMayNotBeAddedTypeOrShippingDestIsDifferent);

						try
						{
							AddPurchaseOrder(it);
						}
						catch (PXException ex)
						{
							PXTrace.WriteError(ex);
							openOrders.Cache.RaiseExceptionHandling<POOrderS.selected>(it, it.Selected,
								new PXSetPropertyException<POLineS.selected>(ex.Message, PXErrorLevel.RowError));
							hasError = true;
						}

						it.Selected = false;
					}
				}

				if (hasError)
					throw new PXException(Messages.FailedToAddPOOrder);
			}
			return adapter.Get();
		}

        public PXAction<POReceipt> addTransfer;
        [PXUIField(DisplayName = Messages.AddTransfer, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
        [PXLookupButton]
        public virtual IEnumerable AddTransfer(PXAdapter adapter)
        {
            if (this.Document.Current != null &&
                 this.Document.Current.Released != true)
            {
                if (this.filter.Current.ResetFilter == true)
                {
                    this.filter.Cache.Remove(this.filter.Current);
                    this.filter.Cache.Insert(new POOrderFilter());
                }
                else
                    this.filter.Cache.RaiseRowSelected(this.filter.Current);

                if (this.openTransfers.AskExt(true) == WebDialogResult.OK)
                    return AddTransfer2(adapter);
            }
            openTransfers.Cache.Clear();
            return adapter.Get();
        }

        public PXAction<POReceipt> addTransfer2;
        [PXUIField(DisplayName = Messages.AddTransfer, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXLookupButton]
        public virtual IEnumerable AddTransfer2(PXAdapter adapter)
        {
            if (this.Document.Current != null &&
                 this.Document.Current.Released != true)
            {
                bool hasError = false;
	            foreach (SOOrderShipment it in this.openTransfers.Cache.Updated)
                {
                    if (it.Selected == true)
                    {
						using (new PXReadBranchRestrictedScope())
	                    {
                        AddTransferDoc(it);
	                    }

                        it.Selected = false;
                    }
                }
                if (hasError)
                    throw new PXException(Messages.SomeOrdersMayNotBeAddedTypeOrShippingDestIsDifferent);
            }
            return adapter.Get();
        }

        public PXAction<POReceipt> addINTran;
        [PXUIField(DisplayName = Messages.AddTransferLine, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXLookupButton]
        public virtual IEnumerable AddINTran(PXAdapter adapter)
        {
            return adapter.Get();
        }

        public PXAction<POReceipt> addINTran2;
        [PXUIField(DisplayName = Messages.AddTransferLine, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXLookupButton]
        public virtual IEnumerable AddINTran2(PXAdapter adapter)
        {
            return adapter.Get();
        }

		public PXAction<POReceipt> addPOOrderLine;
		[PXUIField(DisplayName = Messages.AddPOOrderLine, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXLookupButton]
		public virtual IEnumerable AddPOOrderLine(PXAdapter adapter)
		{
			if (this.Document.Current != null &&
					this.Document.Current.Released != true)
			{
                if (this.filter.Current.ResetFilter == true)
				{
					this.filter.Cache.Remove(this.filter.Current);
					this.filter.Cache.Insert(new POOrderFilter());
				}
				else
					this.filter.Cache.RaiseRowSelected(this.filter.Current);

                if (this.poLinesSelection.AskExt((graph, view) => graph.Views[view].Cache.Clear(), true) == WebDialogResult.OK)
				{
					return AddPOOrderLine2(adapter);
				}
			}
			poLinesSelection.Cache.Clear();
			return adapter.Get();
		}

		private void ClearSelectionCache<TField>(PXCache cache)
			where TField : IBqlField
		{
			if (!this.IsImport)
			{
				cache.Clear();
			}
			else
			{
				foreach (object record in poLinesSelection.Cache.Updated)
				{
					cache.SetValue<TField>(record, false);
				}
			}
		}

		public PXAction<POReceipt> addPOOrderLine2;
		[PXUIField(DisplayName = Messages.AddPOOrderLine, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable AddPOOrderLine2(PXAdapter adapter)
		{
			if (Document.Current != null && Document.Current.Released != true)
			{
				bool failedToAddLine = false;
				foreach (POLine it in poLinesSelection.Cache.Updated)
				{
					if (it.Selected == true)
					{
						try
						{
							_skipUIUpdate = true;
							POReceiptLine addedLine = null;
							addedLine = AddPOLine(it);
							if (addedLine != null)
							{
							AddPOOrderReceipt(it.OrderType, it.OrderNbr);
						}
						}
						catch (PXException ex)
						{
							var processingException = new Common.Exceptions.ErrorProcessingEntityException(poLinesSelection.Cache, it, ex);
							PXTrace.WriteError(processingException);
							poLinesSelection.Cache.RaiseExceptionHandling<POLineS.selected>(it, it.Selected,
								new PXSetPropertyException<POLineS.selected>(ex.Message, PXErrorLevel.RowError));
							failedToAddLine = true;
							if (IsContractBasedAPI)
								throw processingException;
						}
						finally
						{
							_skipUIUpdate = false;
						}

						it.Selected = false;
					}
				}

				if (failedToAddLine)
				{
					throw new PXException(Messages.FailedToAddLine);
				}
			}
			return adapter.Get();
		}

		public PXAction<POReceipt> addPOReceiptLine;
		[PXUIField(DisplayName = Messages.AddLine, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXLookupButton]
		public virtual IEnumerable AddPOReceiptLine(PXAdapter adapter)
		{
			if (this.Document.Current != null &&
					this.Document.Current.Released != true)
			{
				this.filter.Current.AllowAddLine = false;
				if (addReceipt.AskExt(
						(graph, view) => ((POReceiptEntry)graph).ResetReceiptFilter(false)) == WebDialogResult.OK)
				{
					try
					{
						this._skipUIUpdate = IsImport;
						AddReceiptLine();
					}
					finally
					{
						this._skipUIUpdate = false;
					}
				}
			}
			return adapter.Get();
		}
		public PXAction<POReceipt> addPOReceiptLine2;
		[PXUIField(DisplayName = Messages.AddLine, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXLookupButton]
		public virtual IEnumerable AddPOReceiptLine2(PXAdapter adapter)
		{
			if (this.Document.Current != null &&
					this.Document.Current.Released != true &&
					AddReceiptLine())
			{
				ResetReceiptFilter(true);
				this.addReceipt.View.RequestRefresh();	
			}
			return adapter.Get();
		}
		private void ResetReceiptFilter(bool keepDescription)
		{
			POReceiptLineS s = this.addReceipt.Current;
			this.addReceipt.Cache.Remove(s);
			this.addReceipt.Cache.Insert(new POReceiptLineS());
			this.addReceipt.Current.ByOne = s.ByOne;
			this.addReceipt.Current.AutoAddLine = s.AutoAddLine;		
            
            if (keepDescription)
				this.addReceipt.Current.Description = s.Description;		
            
			addReceipt.Current.ReceiptType = (Document.Current != null) ? Document.Current.ReceiptType : s.ReceiptType;
			addReceipt.Current.ReceiptVendorID = (Document.Current != null) ? Document.Current.VendorID : s.ReceiptVendorID;
			addReceipt.Current.ReceiptVendorLocationID = (Document.Current != null) ? Document.Current.VendorLocationID : s.ReceiptVendorLocationID;
		}

		public PXAction<POReceipt> addPOReceiptReturn;
		[PXUIField(DisplayName = Messages.AddReceipt, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXLookupButton]
		public virtual IEnumerable AddPOReceiptReturn(PXAdapter adapter)
		{
			if (this.Document.Current != null &&
				this.Document.Current.Released != true)
			{
				ResetReturnFilter();
				if (this.poReceiptReturn.AskExt((graph, view) => graph.Views[view].Cache.Clear(), true) == WebDialogResult.OK)
				{
					return AddPOReceiptReturn2(adapter);
				}
			}
			return adapter.Get();
		}

		public PXAction<POReceipt> addPOReceiptReturn2;
		[PXUIField(DisplayName = Messages.AddReceipt, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable AddPOReceiptReturn2(PXAdapter adapter)
		{
			if (this.Document.Current != null &&
				this.Document.Current.Released != true)
			{
				var res = poReceiptReturn.Cache.Updated;
				string[] selectedReceiptNbrs = poReceiptReturn.Cache.Updated.RowCast<POReceiptReturn>()
					.Where(r => r.Selected == true)
					.Select(r => r.ReceiptNbr).ToArray();
				if (selectedReceiptNbrs.Any())
				{
					foreach (POReceiptLine origLine in PXSelectReadonly<POReceiptLine,
						Where<POReceiptLine.receiptType, Equal<POReceiptType.poreceipt>,
							And<POReceiptLine.receiptNbr, In<Required<POReceiptLine.receiptNbr>>>>>
						.Select(this, new object[] { selectedReceiptNbrs }))
					{
						try
						{
							AddReturnLine(origLine);
						}
						catch (PXAlreadyCreatedException ex)
						{
							PXTrace.WriteWarning(ex);
						}
					}
				}
			}
			return adapter.Get();
		}

		public PXAction<POReceipt> addPOReceiptLineReturn;
		[PXUIField(DisplayName = Messages.AddReceiptLine, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXLookupButton]
		public virtual IEnumerable AddPOReceiptLineReturn(PXAdapter adapter)
		{
			if (this.Document.Current != null &&
				this.Document.Current.Released != true)
			{
				ResetReturnFilter();
				if (this.poReceiptLineReturn.AskExt((graph, view) =>
					{
						graph.Views[view].Cache.Clear();
						graph.Views[view].Cache.ClearQueryCacheObsolete();
					}, true) == WebDialogResult.OK)
				{
					return AddPOReceiptLineReturn2(adapter);
				}
			}
			return adapter.Get();
		}

		public PXAction<POReceipt> addPOReceiptLineReturn2;
		[PXUIField(DisplayName = Messages.AddReceiptLine, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable AddPOReceiptLineReturn2(PXAdapter adapter)
		{
			if (this.Document.Current != null &&
				this.Document.Current.Released != true)
			{
				foreach (POReceiptLineReturn origLine in this.poReceiptLineReturn.Cache.Updated.RowCast<POReceiptLineReturn>().Where(r => r.Selected == true))
				{
					AddReturnLine(origLine);
					origLine.Selected = false;
				}
			}
			return adapter.Get();
		}

		protected virtual void ResetReturnFilter()
		{
			this.returnFilter.Cache.Remove(this.returnFilter.Current);
			this.returnFilter.Cache.Insert();
			this.returnFilter.Cache.IsDirty = false;
		}

		protected virtual bool AddReceiptLine()
		{
			if (addReceipt.Current.VendorLocationID == null && addReceipt.Current.OrigRefNbr == null ||
					addReceipt.Current.Qty <= 0)
				return false;
			
			POLineS poLine =
				addReceipt.Current.PONbr != null && addReceipt.Current.POLineNbr != null
					? this.poLinesSelection.Search<POLineS.orderType, POLineS.orderNbr, POLineS.lineNbr>(
						addReceipt.Current.POType, addReceipt.Current.PONbr, addReceipt.Current.POLineNbr)
					: null;
			
			int? inventoryID = addReceipt.Current.InventoryID;

			if (inventoryID == null && poLine != null)
				inventoryID = poLine.InventoryID;

			if (inventoryID == null)
				return false;

			InventoryItem item = InventoryItem.PK.Find(this, inventoryID);

			if (item == null)
				return false;

			POReceiptLine line = null;

			if (this.Document.Current.VendorID == null && addReceipt.Current.VendorID != null)
			{
				POReceipt r = PXCache<POReceipt>.CreateCopy(this.Document.Current);
				r.VendorID = poLine != null ? poLine.VendorID : addReceipt.Current.VendorID;
				r.VendorLocationID = poLine != null ? poLine.VendorLocationID : addReceipt.Current.VendorLocationID;
				this.Document.Update(r);
			}

			if (this.Document.Current.SiteID == null && addReceipt.Current.SiteID != null)
			{
				POReceipt r = PXCache<POReceipt>.CreateCopy(this.Document.Current);
				r.SiteID = addReceipt.Current.SiteID;
				this.Document.Update(r);
			}

			if (poLine != null)
			{
				line = PXSelect<POReceiptLine,
					Where<POReceiptLine.receiptType, Equal<Current<POReceipt.receiptType>>,
						And<POReceiptLine.receiptNbr, Equal<Current<POReceipt.receiptNbr>>,
							And<POReceiptLine.pOType, Equal<Current<POLine.orderType>>,
								And<POReceiptLine.pONbr, Equal<Current<POLine.orderNbr>>,
									And<POReceiptLine.pOLineNbr, Equal<Current<POLine.lineNbr>>>>>>>>
					.SelectSingleBound(this, new object[] { poLine });

				if (line != null && addReceipt.Current.SiteID != null && line.SiteID != addReceipt.Current.SiteID)
					line = null;

				if (line == null)
				{
					//Note: AddPOLine() in its implementation always adds all unreceived qty. 
					//In order to add only what is specified in the 'Add Receipt Line' dialog set the order qty to the desired value:
					bool overrideOrderQty = (poLine.BaseOrderQty != addReceipt.Current.BaseReceiptQty || poLine.UOM != addReceipt.Current.UOM);
					if (overrideOrderQty)
					{
						poLine.OrderedQtyAltered = true;
						poLine.OverridenUOM = addReceipt.Current.UOM;
						poLine.OverridenQty = addReceipt.Current.ReceiptQty;
						poLine.BaseOverridenQty = addReceipt.Current.BaseReceiptQty;
					}
					
					line = AddPOLine(poLine, true);//Add Receipt line without adding the split (for performance), it will be added later in this method.
					if (line != null)
					{
					AddPOOrderReceipt(poLine.OrderType, poLine.OrderNbr);
				}
			}
			}

			INTran intran = 
                PXSelect<INTran, 
				Where<INTran.docType, Equal<Required<INTran.docType>>, 
					And<INTran.refNbr, Equal<Required<INTran.refNbr>>, 
					And<INTran.lineNbr, Equal<Required<INTran.lineNbr>>>>>>
				.Select(this, INDocType.Transfer, addReceipt.Current.OrigRefNbr, addReceipt.Current.OrigLineNbr);

			if (intran != null)
			{
				line = PXSelect<POReceiptLine,
							Where<
								POReceiptLine.receiptType, Equal<Required<POReceipt.receiptType>>,
								And<POReceiptLine.receiptNbr, Equal<Required<POReceipt.receiptNbr>>,
								And<POReceiptLine.origDocType, Equal<Required<POReceiptLine.origDocType>>,
								And<POReceiptLine.origRefNbr, Equal<Required<POReceiptLine.origRefNbr>>,
								And<POReceiptLine.origLineNbr, Equal<Required<POReceiptLine.origLineNbr>>>>>>>>
								.Select(this, Document.Current.ReceiptType, Document.Current.ReceiptNbr, intran.DocType, intran.RefNbr, intran.LineNbr);

				if (line == null)
				{
					line = AddTransferLine(intran);

					if (item.StkItem == true && line != null && addReceipt.Current.LocationID != null)
					{
						foreach (POReceiptLineSplit split in splits.Select())
						{
							split.LocationID = addReceipt.Current.LocationID;
							splits.Update(split);
						}
					}
				}
			}

			if (line == null && Document.Current != null && Document.Current.ReceiptType == POReceiptType.TransferReceipt)
			{
				throw new PXException(ErrorMessages.CantInsertRecord);
			}

			if (line == null)
			{
				string lineType = item.StkItem != true 
                    ? POLineType.NonStock 
                    : POLineType.GoodsForInventory;

				line = 
                    PXSelect<POReceiptLine,
				Where<POReceiptLine.receiptType, Equal<Current<POReceipt.receiptType>>,
					And<POReceiptLine.receiptNbr, Equal<Current<POReceipt.receiptNbr>>,
					And<POReceiptLine.lineType, Equal<Required<POReceiptLine.lineType>>,
					And<POReceiptLine.inventoryID, Equal<Required<POReceiptLine.inventoryID>>,
					And<POReceiptLine.subItemID, Equal<Required<POReceiptLine.subItemID>>,
					And<POReceiptLine.pOLineNbr, IsNull,
                    		And2<
                    			Where<Current<POReceiptLineS.siteID>, IsNull, 
                    				Or<POReceiptLine.siteID, Equal<Current<POReceiptLineS.siteID>>>>,
                    			And<Where<Current<POReceiptLineS.unitCost>, Equal<decimal0>, 
                    				Or<POReceiptLine.unitCost, Equal<Current<POReceiptLineS.unitCost>>>>>>>>>>>>>
				.SelectSingleBound(this, null, lineType, addReceipt.Current.InventoryID, addReceipt.Current.SubItemID);

				if (line == null)
				{
					line = PXCache<POReceiptLine>.CreateCopy(this.transactions.Insert(new POReceiptLine()));
					line.LineType = lineType;
					line.InventoryID = addReceipt.Current.InventoryID;
					line.UOM = addReceipt.Current.UOM;

					if (addReceipt.Current.SubItemID != null)
						line.SubItemID = addReceipt.Current.SubItemID;

					if (addReceipt.Current.SiteID != null)
						line.SiteID = addReceipt.Current.SiteID;

					line = PXCache<POReceiptLine>.CreateCopy(this.transactions.Update(line));

					if (addReceipt.Current.UnitCost > 0)
						line.UnitCost = addReceipt.Current.UnitCost;

					line = PXCache<POReceiptLine>.CreateCopy(this.transactions.Update(line));
				}
			}

			string subitem = string.Empty;

			if (line != null && intran == null)
			{
				line = PXCache<POReceiptLine>.CreateCopy(line);
				transactions.Current = line;

				if (item.StkItem == true)
				{
					POReceiptLineSplit split = new POReceiptLineSplit();
					split.Qty = addReceipt.Current.BaseReceiptQty;

					if (addReceipt.Current.LocationID != null)
					{
						split.LocationID = addReceipt.Current.LocationID;
					}

					if (addReceipt.Current.LotSerialNbr != null)
						split.LotSerialNbr = addReceipt.Current.LotSerialNbr;

					if (addReceipt.Current.ExpireDate != null)
						split.ExpireDate = addReceipt.Current.ExpireDate;

					this.splits.Insert(split);
					
					if (!IsImport)
					{
						subitem = insetup.Current.UseInventorySubItem == true && split.SubItemID != null
							? ":" + splits.GetValueExt<POReceiptLineSplit.subItemID>(split)
							: string.Empty;
					}
				}
				else
				{
					line.ReceiptQty += addReceipt.Current.ReceiptQty;
					this.transactions.Update(line);
				}

			}

			if (line != null)
			{
				if (!IsImport)
				{
					addReceipt.Current.Description =
						poLine != null
							? PXMessages.LocalizeFormatNoPrefixNLA(Messages.ReceiptAddedForPO,
								transactions.GetValueExt<POReceiptLine.inventoryID>(line).ToString().Trim(),
								subitem,
								addReceipt.Cache.GetValueExt<POReceiptLineS.receiptQty>(addReceipt.Current).ToString(),
								addReceipt.Current.UOM,
								poLine.OrderNbr)
							: PXMessages.LocalizeFormatNoPrefixNLA(Messages.ReceiptAdded,
								transactions.GetValueExt<POReceiptLine.inventoryID>(line).ToString().Trim(),
								subitem,
								addReceipt.Cache.GetValueExt<POReceiptLineS.receiptQty>(addReceipt.Current).ToString(),
								addReceipt.Current.UOM);
				}
				
				if (addReceipt.Current.BarCode != null && addReceipt.Current.SubItemID != null)
				{
					INItemXRef xref =
						PXSelect<INItemXRef,
							Where<INItemXRef.inventoryID, Equal<Current<POReceiptLineS.inventoryID>>,
								And<INItemXRef.alternateID, Equal<Current<POReceiptLineS.barCode>>,
									And<INItemXRef.alternateType, In3<INAlternateType.barcode, INAlternateType.gIN>>>>>
							.SelectSingleBound(this, null);
					if (xref == null)
					{
                        xref = new INItemXRef
                        {
                            InventoryID = addReceipt.Current.InventoryID,
                            AlternateID = addReceipt.Current.BarCode,
                            AlternateType = INAlternateType.Barcode,
                            SubItemID = addReceipt.Current.SubItemID,
                            BAccountID = 0
                        };

						this.xrefs.Insert(xref);
					}
				}
			}
			
			return line != null;
		}

		public PXAction<POReceipt> viewPOOrder;
		[PXUIField(DisplayName = Messages.ViewPOOrder, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXLookupButton]
		public virtual IEnumerable ViewPOOrder(PXAdapter adapter)
		{
			if (this.transactions.Current != null)
			{
				POReceiptLine row = this.transactions.Current;

				if (string.IsNullOrEmpty(row.POType) || string.IsNullOrEmpty(row.PONbr))
				{
					throw new PXException(Messages.POReceiptLineDoesNotReferencePOOrder);
				}

				POOrderEntry graph = PXGraph.CreateInstance<POOrderEntry>();
				graph.Document.Current = graph.Document.Search<POOrder.orderNbr>(row.PONbr, row.POType);
                throw new PXRedirectRequiredException(graph, true, Messages.Document) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}

			return adapter.Get();
		}

		public PXAction<POReceipt> createAPDocument;
		[PXUIField(DisplayName = Messages.CreateAPInvoice, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXProcessButton]
		public virtual IEnumerable CreateAPDocument(PXAdapter adapter)
		{
			if (this.Document.Current != null &&
				this.Document.Current.Released == true)
			{
				POReceipt doc = this.Document.Current;
				if (doc.UnbilledQty != Decimal.Zero)
				{
					APInvoiceEntry invoiceGraph = PXGraph.CreateInstance<APInvoiceEntry>();
					DocumentList<APInvoice> created = new DocumentList<APInvoice>(invoiceGraph);

					bool hasRetainedTaxes = (apsetup.Current.RetainTaxes == true) && HasOrderWithRetainage(doc);
					invoiceGraph.InvoicePOReceipt(doc, created, keepOrderTaxes: !hasRetainedTaxes);
                    invoiceGraph.AttachPrepayment();
					throw new PXRedirectRequiredException(invoiceGraph, Messages.CreateAPInvoice);
				}
				else
				{
					throw new PXException(Messages.AllTheLinesOfPOReceiptAreAlreadyBilled);
				}

			}
			return adapter.Get();
		}

		public PXAction<POReceipt> createLCDocument;
		[PXUIField(DisplayName = Messages.CreateLandedCostDocument, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXProcessButton]
		public virtual IEnumerable CreateLCDocument(PXAdapter adapter)
		{
			if (this.Document.Current != null &&
			    this.Document.Current.Released == true)
			{
				POReceipt doc = this.Document.Current;
				var lcGraph = PXGraph.CreateInstance<POLandedCostDocEntry>();
				InsertNewLandedCostDoc(lcGraph, Document.Current);
				DocumentList<APInvoice> created = new DocumentList<APInvoice>(lcGraph);
				var receiptsToAdd = new List<POReceipt>() { doc };

				lcGraph.AddPurchaseReceipts(receiptsToAdd);

				throw new PXRedirectRequiredException(lcGraph, Messages.CreateLandedCostDocument);
			}
			return adapter.Get();
		}

		protected virtual void InsertNewLandedCostDoc(POLandedCostDocEntry lcGraph, POReceipt receipt)
			=> lcGraph.Document.Insert(new POLandedCostDoc());

		#endregion

		#region Entity Event Handlers
		public PXWorkflowEventHandler<POReceipt> OnInventoryReceiptCreatedFromPOReceipt;
		public PXWorkflowEventHandler<POReceipt> OnInventoryIssueCreatedFromPOReturn;
		public PXWorkflowEventHandler<POReceipt> OnInventoryReceiptCreatedFromPOTransfer;
		#endregion
		public POReceiptEntry()
		{
			APSetupNoMigrationMode.EnsureMigrationModeDisabled(this);
			POSetup record = posetup.Current;

			this.Actions.Move("Insert", "Cancel");
			RowUpdated.AddHandler<POReceipt>(ParentFieldUpdated);
			this.poLinesSelection.Cache.AllowInsert = false;
			this.poLinesSelection.Cache.AllowDelete = false;
			this.poLinesSelection.Cache.AllowUpdate = true;
			this.openOrders.Cache.AllowInsert = false;
			this.openOrders.Cache.AllowDelete = false;
			this.openOrders.Cache.AllowUpdate = true;
			this.openTransfers.Cache.AllowInsert = false;
			this.openTransfers.Cache.AllowDelete = false;
			this.openTransfers.Cache.AllowUpdate = true;
			this.ReceiptOrdersLink.Cache.AllowInsert = false;
			this.ReceiptOrdersLink.Cache.AllowDelete = false;
			this.ReceiptOrdersLink.Cache.AllowUpdate = true;

			bool isInvoiceNbrRequired = (bool)apsetup.Current.RequireVendorRef;

			OpenPeriodAttribute.SetValidatePeriod<POReceipt.finPeriodID>(this.Document.Cache, null, PeriodValidation.DefaultSelectUpdate);
			PXDefaultAttribute.SetPersistingCheck<POReceipt.invoiceNbr>(this.Document.Cache, null, isInvoiceNbrRequired ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
			PXUIFieldAttribute.SetRequired<POReceipt.invoiceNbr>(this.Document.Cache, isInvoiceNbrRequired);

			PXDefaultAttribute.SetPersistingCheck<POReceipt.invoiceDate>(this.Document.Cache, null, PXPersistingCheck.NullOrBlank);
			PXUIFieldAttribute.SetRequired<POReceipt.invoiceDate>(this.Document.Cache, true);

			PXUIFieldAttribute.SetEnabled(this.openOrders.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<POOrderS.selected>(this.openOrders.Cache, null, true);

			PXUIFieldAttribute.SetEnabled(openTransfers.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<INRegister.selected>(openTransfers.Cache, null, true);

			PXDimensionSelectorAttribute.SetValidCombo<POReceiptLine.subItemID>(transactions.Cache, false);
			PXDimensionSelectorAttribute.SetValidCombo<POReceiptLineSplit.subItemID>(splits.Cache, false);

			bool isPMVisible = ProjectAttribute.IsPMVisible(BatchModule.PO);
			PXUIFieldAttribute.SetVisible<POReceiptLine.projectID>(transactions.Cache, null, isPMVisible);
			PXUIFieldAttribute.SetVisible<POReceiptLine.taskID>(transactions.Cache, null, isPMVisible);

			FieldDefaulting.AddHandler<BAccountR.type>((sender, e) => { if (e.Row != null) e.NewValue = BAccountType.VendorType; });

			this.Views.Caches.Remove(typeof(POLineS));
			this.Views.Caches.Remove(typeof(POOrderS));
			this.Views.Caches.Remove(typeof(POReceiptReturn));
			this.Views.Caches.Remove(typeof(POReceiptLineReturn));

			PXUIFieldAttribute.SetVisible<SOOrderShipment.orderType>(this.Caches[typeof(SOOrderShipment)], null, true);
            PXUIFieldAttribute.SetVisible<SOOrderShipment.orderNbr>(this.Caches[typeof(SOOrderShipment)], null, true);
            PXUIFieldAttribute.SetVisible<SOOrderShipment.shipmentNbr>(this.Caches[typeof(SOOrderShipment)], null, true);

			PXNoteAttribute.ForcePassThrow<POOrderReceiptLink.orderNoteID>(ReceiptOrdersLink.Cache);
		}

		[InjectDependency]
		protected ILicenseLimitsService _licenseLimits { get; set; }

		void IGraphWithInitialization.Initialize()
			{
			if (_licenseLimits != null)
			{
				OnBeforeCommit += _licenseLimits.GetCheckerDelegate<POReceipt>(
					new TableQuery(TransactionTypes.LinesPerMasterRecord, typeof(POReceiptLine), (graph) =>
					{
						return new PXDataFieldValue[]
						{
							new PXDataFieldValue<POReceiptLine.receiptType>(PXDbType.Char, ((POReceiptEntry)graph).Document.Current?.ReceiptType),
							new PXDataFieldValue<POReceiptLine.receiptNbr>(((POReceiptEntry)graph).Document.Current?.ReceiptNbr)
						};
					}),
					new TableQuery(TransactionTypes.SerialsPerDocument, typeof(POReceiptLineSplit), (graph) =>
					{
						return new PXDataFieldValue[]
						{
							new PXDataFieldValue<POReceiptLineSplit.receiptType>(PXDbType.Char, ((POReceiptEntry)graph).Document.Current?.ReceiptType),
							new PXDataFieldValue<POReceiptLineSplit.receiptNbr>(((POReceiptEntry)graph).Document.Current?.ReceiptNbr)
						};
					}));
			}
		}

		#region POReceipt Events
		protected virtual void POReceipt_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			bool requireControlTotal = (bool)posetup.Current.RequireReceiptControlTotal;
			POReceipt row = (POReceipt)e.Row;
			POReceipt oldRow = (POReceipt)e.OldRow;
			if (row == null) return;

			if (requireControlTotal == false)
            {
				if (row.OrderQty != null && row.OrderQty != decimal.Zero)
				{
					sender.SetValue<POReceipt.controlQty>(row, row.OrderQty);
				}
				else
				{
					sender.SetValue<POReceipt.controlQty>(row, 0m);
				}
			}

			bool released = (bool)row.Released;
			if (row.Hold == false && released == false)
			{
                if (requireControlTotal)
                {
					if (row.OrderQty != row.ControlQty)
					{
						sender.RaiseExceptionHandling<POReceipt.controlQty>(row, row.ControlQty, new PXSetPropertyException(Messages.DocumentOutOfBalance));
					}
					else
					{
						sender.RaiseExceptionHandling<POReceipt.controlQty>(row, row.ControlQty, null);
					}
				}
			}
			else 
			{
				sender.RaiseExceptionHandling<POReceipt.controlQty>(row, null, null);
			}

			if (row.ReceiptType == POReceiptType.POReturn && oldRow != null && 
				!sender.ObjectsEqual<POReceipt.returnInventoryCostMode>(row, oldRow))
			{
				if (row.ReturnInventoryCostMode == ReturnCostMode.OriginalCost)
				{
					SetOriginalAmountsAllReturnLines(sender, row, oldRow, true);
				}
				else if (oldRow.ReturnInventoryCostMode == ReturnCostMode.OriginalCost)
				{
					ResetAmountsAllReturnLines(sender, row);
					sender.RaiseFieldUpdated<POReceipt.receiptDate>(row, null);
				}
			}

			if (row.Hold == false && !sender.ObjectsEqual<POReceipt.hold>(e.Row, e.OldRow))
			{
				foreach (POReceiptLine line in transactions.Select())
				{
					if (line.ReceiptQty <= Decimal.Zero && transactions.Cache.GetStatus(line) == PXEntryStatus.Notchanged)
					{
						transactions.Cache.MarkUpdated(line, assertError: true);
					}
				}
			}
		}

		protected virtual void POReceipt_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var row = (POReceipt)e.Row;
			if (row == null || e.Operation.Command() == PXDBOperation.Delete)
				return;

			if (row.ReturnInventoryCostMode == ReturnCostMode.CostByIssue)
			{
				foreach (POReceiptLine line in transactions.Select())
				{
					if (POLineType.IsProjectDropShip(line.LineType))
					{
						Document.Cache.RaiseExceptionHandling<POReceipt.returnInventoryCostMode>(row, row.ReturnInventoryCostMode,
							new PXSetPropertyException(Messages.ProjectDropShipCostByIssueStrategyInvalid, PXErrorLevel.Error));
						break;
					}
				}
			}
		}

		/// <summary>
		/// Updates all the return lines with the original receipt amounts
		/// </summary>
		public virtual void SetOriginalAmountsAllReturnLines(PXCache sender, POReceipt row, POReceipt oldRow, bool updateCurencyInfo)
		{
			bool curyInfoUpdated = false;

			var returnAndReceiptLines = SelectFrom<POReceiptLine>.
				LeftJoin<POReceiptLine2>.
					On<POReceiptLine2.receiptType.IsEqual<POReceiptLine.origReceiptType>.
					And<POReceiptLine2.receiptNbr.IsEqual<POReceiptLine.origReceiptNbr>.
					And<POReceiptLine2.lineNbr.IsEqual<POReceiptLine.origReceiptLineNbr>>>>.
				LeftJoin<POReceipt>.
					On<POReceipt.receiptType.IsEqual<POReceiptLine2.receiptType>.
					And<POReceipt.receiptNbr.IsEqual<POReceiptLine2.receiptNbr>>>.
				LeftJoin<CurrencyInfo>.On<CurrencyInfo.curyInfoID.IsEqual<POReceipt.curyInfoID>>.
			Where<POReceiptLine.receiptType.IsEqual<POReceiptType.poreturn>.
				And<POReceiptLine.receiptNbr.IsEqual<@P.AsString>>.
				And<POReceiptLine.origReceiptNbr.IsNotNull>>.View.Select(this, row.ReceiptNbr);

			if (row.ReturnInventoryCostMode == ReturnCostMode.OriginalCost)
			{
				CurrencyInfo returnCuryInfo = GetCurrencyInfo(Document.Current?.CuryInfoID);
				CurrencyInfo prevCuryInfo = null;

				bool showOrigPOReceiptCannotBeFoundError = false;
				bool showReturnByOrigCostCannotBeSelectedDifferentRatesError = false;
				bool showReturnByIssueStrategyCannotBeSelectedError = false;

				foreach (PXResult<POReceiptLine, POReceiptLine2, POReceipt, CurrencyInfo> line in returnAndReceiptLines)
				{
					POReceiptLine returnLine = line;
					POReceiptLine2 receiptLine = line;
					POReceipt receipt = line;
					CurrencyInfo receiptCurrencyInfo = line;

					if (receiptLine.ReceiptNbr == null || receipt.ReceiptNbr == null || receiptCurrencyInfo.CuryInfoID == null)
					{
						showOrigPOReceiptCannotBeFoundError = true;
					}

					if (returnCuryInfo.CuryID != receiptCurrencyInfo.CuryID ||
						prevCuryInfo != null && !IsSameCurrencyInfo(prevCuryInfo, receiptCurrencyInfo))
					{
						showReturnByOrigCostCannotBeSelectedDifferentRatesError = true;
					}

					if (POLineType.IsProjectDropShip(returnLine.LineType))
					{
						showReturnByIssueStrategyCannotBeSelectedError = true;
					}

					if (showReturnByIssueStrategyCannotBeSelectedError && (showOrigPOReceiptCannotBeFoundError || showReturnByOrigCostCannotBeSelectedDifferentRatesError))
					{
						break;
					}

					prevCuryInfo = receiptCurrencyInfo;
				}

				if (showReturnByIssueStrategyCannotBeSelectedError && (showOrigPOReceiptCannotBeFoundError || showReturnByOrigCostCannotBeSelectedDifferentRatesError))
				{
					Document.Cache.SetValue<POReceipt.returnInventoryCostMode>(row, oldRow.ReturnInventoryCostMode);
					sender.RaiseExceptionHandling<POReceipt.returnInventoryCostMode>(row, oldRow.ReturnInventoryCostMode, new PXSetPropertyException(Messages.ReturnByOrigCostAndByIssueStrategyCannotBeSelected));
					return;
				}
				else if (showOrigPOReceiptCannotBeFoundError)
				{
					Document.Cache.SetValue<POReceipt.returnInventoryCostMode>(row, oldRow.ReturnInventoryCostMode);
					sender.RaiseExceptionHandling<POReceipt.returnInventoryCostMode>(row, oldRow.ReturnInventoryCostMode, new PXSetPropertyException(Messages.OrigPOReceiptCannotBeFound));
					return;
				}
				else if (showReturnByOrigCostCannotBeSelectedDifferentRatesError)
				{
					Document.Cache.SetValue<POReceipt.returnInventoryCostMode>(row, oldRow.ReturnInventoryCostMode);
					sender.RaiseExceptionHandling<POReceipt.returnInventoryCostMode>(row, oldRow.ReturnInventoryCostMode, new PXSetPropertyException(Messages.ReturnByOrigCostCannotBeSelectedDifferentRates));
					return;
				}
			}
			
			foreach (PXResult<POReceiptLine, POReceiptLine2, POReceipt, CurrencyInfo> line in returnAndReceiptLines)
			{
				POReceiptLine returnLine = line;
				POReceiptLine2 receiptLine = line;
				CurrencyInfo receiptCurrencyInfo = line;

				//there should be no situations when receipts with different currencies/currency rates are combined into one return when Original Cost from Receipt is selected
				if (updateCurencyInfo && !curyInfoUpdated)
				{
					CopyReceiptCurrencyInfoToReturn(receiptCurrencyInfo, GetCurrencyInfo(row.CuryInfoID), true);
					curyInfoUpdated = true;
				}

				SetOriginalReceiptAmounts(returnLine, receiptLine);
			}
		}

		public virtual void SetOriginalAmountsReturnLine(POReceiptLine row)
		{
			POReceiptLine2 receiptLine = SelectFrom<POReceiptLine2>.
			Where<POReceiptLine2.receiptType.IsEqual<@P.AsString.ASCII>.
				And<POReceiptLine2.receiptNbr.IsEqual<@P.AsString>>.
				And<POReceiptLine2.lineNbr.IsEqual<@P.AsInt>>>.View.Select(this, row.OrigReceiptType, row.OrigReceiptNbr, row.OrigReceiptLineNbr);

			if (receiptLine != null)
			{
				SetOriginalReceiptAmounts(row, receiptLine);
			}
		}

		public virtual void SetOriginalReceiptAmounts(POReceiptLine returnLine, POReceiptLine receiptLine)
		{
			CurrencyInfo currencyInfo = MultiCurrencyExt.GetDefaultCurrencyInfo();
			POReceiptLine updatedReturnLine = (POReceiptLine)transactions.Cache.CreateCopy(returnLine);

			updatedReturnLine.AllowEditUnitCost = false;

			updatedReturnLine.DiscPct = receiptLine.DiscPct;
			decimal? srcCuryDiscAmt = receiptLine.CuryDiscAmt;
			updatedReturnLine.CuryDiscAmt = (receiptLine.ReceiptQty == returnLine.ReceiptQty) ? srcCuryDiscAmt
				: currencyInfo.RoundCury((srcCuryDiscAmt * returnLine.ReceiptQty / receiptLine.ReceiptQty) ?? 0m);

			updatedReturnLine.CuryUnitCost = receiptLine.CuryUnitCost;

			updatedReturnLine.TranUnitCost = receiptLine.TranCostFinal / receiptLine.ReceiptQty;
			if (updatedReturnLine.TranUnitCost != null)
			{
				updatedReturnLine.CuryTranUnitCost = currencyInfo.CuryConvCuryRaw(updatedReturnLine.TranUnitCost ?? 0m);
				if (updatedReturnLine.CuryTranUnitCost != null)
					updatedReturnLine.CuryTranUnitCost = Math.Round(updatedReturnLine.CuryTranUnitCost.Value, POReceiptLine.curyTranUnitCost.Precision, MidpointRounding.AwayFromZero);
			}
			decimal? srcCuryExtCost = receiptLine.CuryExtCost;
			updatedReturnLine.CuryExtCost = (receiptLine.ReceiptQty == returnLine.ReceiptQty) ? srcCuryExtCost
				: currencyInfo.RoundCury((srcCuryExtCost * returnLine.ReceiptQty / receiptLine.ReceiptQty) ?? 0m);

			decimal? srcTranCost = receiptLine.TranCostFinal ?? receiptLine.TranCost;
			updatedReturnLine.TranCost = (receiptLine.ReceiptQty == returnLine.ReceiptQty) ? srcTranCost
				: currencyInfo.RoundBase((srcTranCost * returnLine.ReceiptQty / receiptLine.ReceiptQty) ?? 0m);
			if (updatedReturnLine.TranCost != null)
			{
				updatedReturnLine.CuryTranCost = currencyInfo.CuryConvCury(updatedReturnLine.TranCost ?? 0m);
			}
			transactions.Cache.Update(updatedReturnLine);
		}

		public virtual void ResetAmountsAllReturnLines(PXCache sender, POReceipt row)
		{
			if (row.ReceiptType != POReceiptType.POReturn)
				return;

			foreach (POReceiptLine returnLine in transactions.Select())
			{
				if (returnLine.OrigReceiptNbr != null)
				{
					returnLine.AllowEditUnitCost = true;
					returnLine.CuryTranCost = ((Decimal?)PXFormulaAttribute.Evaluate<POReceiptLine.curyTranCost>(transactions.Cache, returnLine)) ?? returnLine.CuryTranCost;
					transactions.Cache.Update(returnLine);
				}
			}
		}

		protected virtual void POReceipt_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			POReceipt doc = (POReceipt)e.Row;
			if (doc == null) return;

			if (doc.Released == true)
			{
				if (!String.IsNullOrEmpty(doc.InvtDocType) && !String.IsNullOrEmpty(doc.InvtRefNbr))
				{
					doc.InventoryDocType = doc.InvtDocType;
					doc.InventoryRefNbr = doc.InvtRefNbr;
				}
				else
				{
					// Acuminator disable once PX1042 DatabaseQueriesInRowSelecting [false positive]
					INTran _inventoryDoc = GetInventoryDoc(doc);
					if (_inventoryDoc != null)
					{
						doc.InventoryDocType = _inventoryDoc.DocType;
						doc.InventoryRefNbr = _inventoryDoc.RefNbr;
					}
				}
			}

		}

		protected virtual void POReceipt_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            POReceipt doc = (POReceipt)e.Row;
			if (doc == null) return;

			bool released = doc.Released == true;
			bool requireControlTotal = posetup.Current.RequireReceiptControlTotal == true;
            bool hasTransactions = false;
			bool isReturn = doc.ReceiptType == POReceiptType.POReturn;
			bool isTransferReceipt = doc.ReceiptType == POReceiptType.TransferReceipt;
			bool isReceipt = (doc.ReceiptType == POReceiptType.POReceipt);
            bool isWarehouseSet = doc.SiteID != null;

			bool isPMVisible = ProjectAttribute.IsPMVisible(BatchModule.PO);
			bool requireSingleProject = (apsetup.Current.RequireSingleProjectPerDocument == true);
			bool displayProject = (isPMVisible && requireSingleProject && !isTransferReceipt);

			if (!_skipUIUpdate)
            {
				PXUIFieldAttribute.SetVisible<POReceipt.curyID>(sender, doc, this.AllowNonBaseCurrency());
				PXUIFieldAttribute.SetVisible<POReceipt.projectID>(sender, doc, displayProject);
            }

			filter.Current.VendorID = doc.VendorID;
			filter.Current.VendorLocationID = doc.VendorLocationID;

            PXUIFieldAttribute.SetEnabled(this.poLinesSelection.Cache, null, false);
            if (doc.Released == true)
            {
                PXUIFieldAttribute.SetEnabled(sender, doc, false);
				sender.AllowDelete = false;
				sender.AllowUpdate = true;
                PXDefaultAttribute.SetPersistingCheck<POReceipt.invoiceNbr>(sender, doc, PXPersistingCheck.Nothing);
				PXNoteAttribute.ForcePassThrow<POReceipt.noteID>(sender);
            }
            else
            {
                if (this._skipUIUpdate == false)
                {
                    PXUIFieldAttribute.SetEnabled<POReceipt.autoCreateInvoice>(sender, doc, !isTransferReceipt);
					PXUIFieldAttribute.SetEnabled<POReceipt.projectID>(sender, doc, displayProject && string.IsNullOrEmpty(doc.POType));
					PXDefaultAttribute.SetPersistingCheck<POReceipt.projectID>(sender, doc, displayProject ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

					bool autoCreateInvoice = doc.AutoCreateInvoice ?? false;
                    PXUIFieldAttribute.SetRequired<POReceipt.invoiceNbr>(sender, autoCreateInvoice);

                    PXUIFieldAttribute.SetEnabled<POReceipt.invoiceDate>(sender, doc, autoCreateInvoice);
                    PXDefaultAttribute.SetPersistingCheck<POReceipt.invoiceDate>(sender, doc, autoCreateInvoice ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
                    PXUIFieldAttribute.SetRequired<POReceipt.invoiceDate>(sender, autoCreateInvoice);

                    bool isInvoiceNbrRequired = autoCreateInvoice && (bool)apsetup.Current.RequireVendorRef;
                    PXDefaultAttribute.SetPersistingCheck<POReceipt.invoiceNbr>(sender, doc, isInvoiceNbrRequired ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
                    PXUIFieldAttribute.SetRequired<POReceipt.invoiceNbr>(sender, isInvoiceNbrRequired);

                    PXUIFieldAttribute.SetVisible<POOrderS.receivedQty>(this.openOrders.Cache, null, isReturn);
                    PXUIFieldAttribute.SetVisible<POOrderS.leftToReceiveQty>(this.openOrders.Cache, null, !isReturn);
                    PXUIFieldAttribute.SetVisible<POLineS.receivedQty>(this.poLinesSelection.Cache, null, isReturn);
                    PXUIFieldAttribute.SetVisible<POLineS.leftToReceiveQty>(this.poLinesSelection.Cache, null, !isReturn);

                    hasTransactions = HasTransactions();
                    PXUIFieldAttribute.SetEnabled<POReceipt.receiptType>(sender, doc, !hasTransactions);
                    PXUIFieldAttribute.SetEnabled<POReceipt.vendorID>(sender, doc, !hasTransactions);
                    PXUIFieldAttribute.SetEnabled<POReceipt.siteID>(sender, doc, !hasTransactions);

					sender.AllowDelete = true;
                    sender.AllowUpdate = true;
                    PXUIFieldAttribute.SetEnabled<POLineS.selected>(this.poLinesSelection.Cache, null, true);

                    string theOnlyTaxCalcMode;

                    if (autoCreateInvoice &&
                        (theOnlyTaxCalcMode = ReceiptOrdersLink.SelectWindowed(0, 1).RowCast<POOrderReceiptLink>().FirstOrDefault()?.TaxCalcMode) != null &&
                        ReceiptOrdersLink.Select().RowCast<POOrderReceiptLink>().Any(_ => _.TaxCalcMode != theOnlyTaxCalcMode))
                    {
                        sender.RaiseExceptionHandling<POReceipt.autoCreateInvoice>(doc, autoCreateInvoice,
                            new PXSetPropertyException(
                                AP.Messages.POReceiptContainsSeveralTaxCalcModes,
                                PXErrorLevel.Warning,
                                TX.TaxCalculationMode.ListAttribute.GetLocalizedLabel<Location.vTaxCalcMode>(location.Cache, location.Current)));
                    }
                    else
                    {
                        sender.RaiseExceptionHandling<POReceipt.autoCreateInvoice>(doc, autoCreateInvoice, null);
                    }
				}
            }

			bool hasVendor = (doc.VendorID != null && doc.VendorLocationID != null);
			transactions.Cache.AllowInsert = !released && !isTransferReceipt && hasVendor && doc.WMSSingleOrder == false;
			transactions.Cache.AllowUpdate = hasVendor;
			transactions.Cache.AllowDelete = !released;

            if (this._skipUIUpdate == false)
            {
                PXUIFieldAttribute.SetEnabled<POReceipt.receiptNbr>(sender, doc);
                PXUIFieldAttribute.SetEnabled<POReceipt.receiptType>(sender, doc);

                PXUIFieldAttribute.SetVisible<POReceipt.controlQty>(sender, e.Row, requireControlTotal || released);

                bool allowAddOrder = !isReturn && !released && hasVendor && doc.WMSSingleOrder == false && (!requireSingleProject || doc.ProjectID != null);
				bool allowAddReturn = isReturn && !released && hasVendor;
                bool allowSplits = !released && hasTransactions;
				this.addPOOrder.SetEnabled(allowAddOrder);
                this.addPOOrderLine.SetEnabled(allowAddOrder);
				this.addPOReceiptLine.SetEnabled(allowAddOrder);
				this.addPOReceiptReturn.SetEnabled(allowAddReturn);
				this.addPOReceiptLineReturn.SetEnabled(allowAddReturn);
				bool allowCreateAPDocument = released
                    && (doc.UnbilledQty != Decimal.Zero)
                    && !isTransferReceipt;
                this.createAPDocument.SetEnabled(allowCreateAPDocument);
            }

            addLinePopupHandler = isTransferReceipt ? (PopupHandler)new AddTransferPopupHandler(this) : new AddReceiptPopupHandler(this);
            PXUIFieldAttribute.SetVisible<POReceipt.siteID>(sender, e.Row, isTransferReceipt);
            PXUIFieldAttribute.SetVisible<POReceipt.vendorID>(sender, e.Row, !isTransferReceipt);
            PXUIFieldAttribute.SetVisible<POReceipt.vendorLocationID>(sender, e.Row, !isTransferReceipt);
            PXUIFieldAttribute.SetVisible<POReceipt.autoCreateInvoice>(sender, e.Row, !isTransferReceipt);
            PXUIFieldAttribute.SetVisible<POReceipt.invoiceNbr>(sender, e.Row, !isTransferReceipt);

            addPOOrder.SetVisible(isReceipt);
            addPOOrderLine.SetVisible(isReceipt);
			this.addPOReceiptLine.SetVisible(!isReturn);
			this.addPOReceiptReturn.SetVisible(isReturn);
			this.addPOReceiptLineReturn.SetVisible(isReturn);
            viewPOOrder.SetVisible(!isTransferReceipt);
            addTransfer.SetVisible(isTransferReceipt);
            addTransfer.SetEnabled(isWarehouseSet && isTransferReceipt);
			PXUIFieldAttribute.SetVisible(ReceiptOrdersLink.Cache, null, !isTransferReceipt);

			PXUIFieldAttribute.SetVisible<POReceipt.invoiceDate>(sender, e.Row, !isTransferReceipt);

            PXUIFieldAttribute.SetVisible<POReceipt.unbilledQty>(sender, e.Row, !isTransferReceipt);

			landedCosts.AllowDelete = false;

			if (this.IsImport == false)
			{
				PXUIFieldAttribute.SetVisible<POReceiptLine.pOType>(this.Caches[typeof(POReceiptLine)], null, !isTransferReceipt);
				PXUIFieldAttribute.SetVisible<POReceiptLine.pONbr>(this.Caches[typeof(POReceiptLine)], null, !isTransferReceipt);
				PXUIFieldAttribute.SetVisible<POReceiptLine.pOLineNbr>(this.Caches[typeof(POReceiptLine)], null, !isTransferReceipt);

				PXUIFieldAttribute.SetVisible<POReceiptLine.reasonCode>(this.Caches[typeof(POReceiptLine)], null, isReturn);
				PXUIFieldAttribute.SetVisible<POReceiptLine.allowComplete>(this.Caches[typeof(POReceiptLine)], null, isReceipt);
				PXUIFieldAttribute.SetVisible<POReceiptLine.allowOpen>(this.Caches[typeof(POReceiptLine)], null, isReturn);
				PXUIFieldAttribute.SetVisible<POReceiptLine.sOOrderType>(this.Caches[typeof(POReceiptLine)], null, isTransferReceipt);
				PXUIFieldAttribute.SetVisible<POReceiptLine.sOOrderNbr>(this.Caches[typeof(POReceiptLine)], null, isTransferReceipt);
				PXUIFieldAttribute.SetVisible<POReceiptLine.sOOrderLineNbr>(this.Caches[typeof(POReceiptLine)], null, isTransferReceipt);
				PXUIFieldAttribute.SetVisible<POReceiptLine.sOShipmentNbr>(this.Caches[typeof(POReceiptLine)], null, isTransferReceipt);

				PXUIFieldAttribute.SetVisible<POReceiptLine.selected>(this.Caches[typeof(POReceiptLine)], null, isReceipt && released);
				PXUIFieldAttribute.SetVisible<POReceiptLine.returnedQty>(this.Caches[typeof(POReceiptLine)], null, isReceipt && released);
				PXUIFieldAttribute.SetVisible<POReceiptLine.origReceiptNbr>(this.Caches[typeof(POReceiptLine)], null, isReturn);
			}
			PXUIFieldAttribute.SetVisible<POReceipt.returnInventoryCostMode>(sender, doc, isReturn);

			PXUIFieldAttribute.SetVisible<POReceiptAPDoc.accruedQty>(this.Caches[typeof(POReceiptAPDoc)], null, released);
			PXUIFieldAttribute.SetVisible<POReceiptAPDoc.accruedAmt>(this.Caches[typeof(POReceiptAPDoc)], null, released);
			PXUIFieldAttribute.SetVisible<POReceiptAPDoc.totalPPVAmt>(this.Caches[typeof(POReceiptAPDoc)], null, released);

			receiptHistory.AllowSelect = !isTransferReceipt;
			apDocs.AllowSelect = !isTransferReceipt;
		}

		private bool isDeleting = false;
		protected virtual void POReceipt_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			POReceipt doc = (POReceipt)e.Row;
			isDeleting = true;

		}

		protected virtual void POReceipt_VendorLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<POReceipt.branchID>(e.Row);
		}

		[PopupMessage]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void POReceipt_VendorID_CacheAttached(PXCache sender)
        {
        }


        protected virtual void POReceipt_VendorID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			branch.RaiseFieldUpdated(sender, e.Row);
			vendor.RaiseFieldUpdated(sender, e.Row);

			sender.SetDefaultExt<POReceipt.autoCreateInvoice>(e.Row);
			sender.SetDefaultExt<POReceipt.vendorLocationID>(e.Row);
		}

		protected virtual void POReceipt_InvoiceDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			POReceipt row = (POReceipt)e.Row;
			if (row != null)
			{
				e.NewValue = row.ReceiptDate;
			}
		}
		
		protected virtual void POReceipt_ProjectID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			POReceipt row = e.Row as POReceipt;
			if (row == null) return;

			if (apsetup.Current.RequireSingleProjectPerDocument == true)
			{
				foreach (POReceiptLine line in transactions.Select())
				{
					line.ProjectID = row.ProjectID;
					transactions.Update(line);
				}
				this.poLinesSelection.Cache.Clear();
				this.openOrders.Cache.Clear();
			}
		}

		private INTran GetInventoryDoc(POReceipt doc)
		{
			var inTrans = PXSelectGroupBy<INTran,
						Where<INTran.pOReceiptType, Equal<Required<INTran.pOReceiptType>>,
							And<INTran.pOReceiptNbr, Equal<Required<INTran.pOReceiptNbr>>>>,
						Aggregate<
							GroupBy<INTran.docType,
								GroupBy<INTran.refNbr>>>,
						OrderBy<
							Asc<INTran.refNbr>>>
					.Select(this, doc.ReceiptType, doc.ReceiptNbr)
					.RowCast<INTran>()
					.OrderByDescending(t => t.RefNbr)
					.ThenByDescending(t => t.LastModifiedDateTime)
					.ToList();

				string expectedINDocType = (doc.ReceiptType == POReceiptType.POReturn)
					? (doc.ReturnInventoryCostMode == ReturnCostMode.OriginalCost ? INDocType.Adjustment : INDocType.Issue)
					: INDocType.Receipt;

			INTran docToOpen = inTrans.FirstOrDefault(t => t.DocType == expectedINDocType) ?? inTrans.FirstOrDefault(t => t.DocType == INDocType.Issue);

			return docToOpen;
		}
		#endregion

		#region Receipt Line Events
		protected virtual void POReceiptLine_ReasonCode_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			POReceiptLine row = (POReceiptLine)e.Row;
			if (row != null)
			{
				if (row.ReceiptType == POReceiptType.POReturn)
				{
					e.NewValue = posetup.Current.RCReturnReasonCodeID;
				}
			}
		}

		protected object GetValue<Field>(object data)
			where Field : IBqlField
		{
			if (data == null) return null;
			return this.Caches[BqlCommand.GetItemType(typeof(Field))].GetValue(data, typeof(Field).Name);
		}

		protected virtual void POReceiptLine_POAccrualAcctID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			POReceiptLine row = e.Row as POReceiptLine;
			if (row == null) return;
			if (IsAccrualAccountRequired(row))
			{
				if (!_forceAccrualAcctDefaulting
					&& (row.ReceiptType == POReceiptType.POReceipt && !string.IsNullOrEmpty(row.PONbr) && !string.IsNullOrEmpty(row.POType) && row.POLineNbr != null
					|| row.ReceiptType == POReceiptType.POReturn && !string.IsNullOrEmpty(row.OrigReceiptType) && !string.IsNullOrEmpty(row.OrigReceiptNbr) && row.OrigReceiptLineNbr != null))
				{
					e.Cancel = true;
					return;
				}
						var item = InventoryItem.PK.Find(this, row.InventoryID);
						var postClass = INPostClass.PK.Find(this, item?.PostClassID);

						if (postClass != null)
						{
							Vendor vendorForAccrual = vendor.Current;
							var site = INSite.PK.Find(this, row.SiteID);
							e.NewValue = INReleaseProcess.GetPOAccrualAcctID<INPostClass.pOAccrualAcctID>(this, postClass.POAccrualAcctDefault, item, site, postClass, vendorForAccrual);
							if (e.NewValue != null)
							{
								e.Cancel = true;
							}
						}
					}
					else
					{
					e.NewValue = null;
					e.Cancel = true;
				}
			}

		protected virtual void POReceiptLine_POAccrualSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			POReceiptLine row = e.Row as POReceiptLine;
			if (row == null) return;
			if (IsAccrualAccountRequired(row))
			{
				if (!_forceAccrualAcctDefaulting
					&& (row.ReceiptType == POReceiptType.POReceipt && !string.IsNullOrEmpty(row.PONbr) && !string.IsNullOrEmpty(row.POType) && row.POLineNbr != null
					|| row.ReceiptType == POReceiptType.POReturn && !string.IsNullOrEmpty(row.OrigReceiptType) && !string.IsNullOrEmpty(row.OrigReceiptNbr) && row.OrigReceiptLineNbr != null))
				{
					e.Cancel = true;
					return;
				}
					var item = InventoryItem.PK.Find(this, row.InventoryID);
					var postClass = INPostClass.PK.Find(this, item?.PostClassID);

					if (postClass != null)
					{
						try
						{
							Vendor vendorForAccrual = vendor.Current;
							var site = INSite.PK.Find(this, row.SiteID);
							e.NewValue = INReleaseProcess.GetPOAccrualSubID<INPostClass.pOAccrualSubID>(this, postClass.POAccrualAcctDefault, postClass.POAccrualSubMask, item, site, postClass, vendorForAccrual);
						}
						catch (PXMaskArgumentException) { }
						if (e.NewValue != null)
						{
							e.Cancel = true;
						}
					}
			}
			else
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		/// <summary>
		/// Sets Expense Account for items with Accrue Cost = true. See implementation in CostAccrual extension.
		/// </summary>
		public virtual void SetExpenseAccount(PXCache sender, PXFieldDefaultingEventArgs e, InventoryItem item)
		{
		}

		protected virtual void POReceiptLine_ExpenseAcctID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			POReceiptLine row = (POReceiptLine)e.Row;
			if (row == null || !row.ReceiptType.IsIn(POReceiptType.POReceipt, POReceiptType.POReturn)
				|| row.ReceiptType == POReceiptType.POReceipt && !string.IsNullOrEmpty(row.PONbr) && !string.IsNullOrEmpty(row.POType) && row.POLineNbr != null
				|| row.ReceiptType == POReceiptType.POReturn && !string.IsNullOrEmpty(row.OrigReceiptType) && !string.IsNullOrEmpty(row.OrigReceiptNbr) && row.OrigReceiptLineNbr != null)
			{
				return;
			}
			switch (row.LineType)
			{
				case POLineType.Description:
					e.Cancel = true;
					break;
				case POLineType.Freight:
					Carrier carrier = Carrier.PK.Find(this, location.Current.VCarrierID);
					e.NewValue = GetValue<Carrier.freightExpenseAcctID>(carrier) ?? posetup.Current.FreightExpenseAcctID;
					e.Cancel = true;
					break;
				default:
					var item = InventoryItem.PK.Find(this, row.InventoryID);
					if (item != null)
					{
						if (row != null && item.StkItem != true && row.AccrueCost == true)
						{
							SetExpenseAccount(sender, e, item);
						}
						else
						{
							e.NewValue = GetNonStockExpenseAccount(row, item);
						}
					}
					if (e.NewValue != null)
						e.Cancel = true;
					break;
			}
		}

		public virtual int? GetNonStockExpenseAccount(POReceiptLine row, InventoryItem item)
		{
			INPostClass postClass;
			if (POLineType.IsNonStock(row.LineType) && !POLineType.IsService(row.LineType) && (postClass = INPostClass.PK.Find(this, item.PostClassID)) != null)
			{
				var site = INSite.PK.Find(this, row.SiteID);
				try
				{
					return INReleaseProcess.GetAcctID<INPostClass.cOGSAcctID>(this, postClass.COGSAcctDefault, item, site, postClass);
				}
				catch (PXMaskArgumentException)
				{
				}
			}
			else if (POLineType.IsNonStock(row.LineType))
			{
				return item.COGSAcctID ?? location.Current.VExpenseAcctID;
			}

			return null;
		}

		/// <summary>
		/// Sets Expense Subaccount for items with Accrue Cost = true. See implementation in CostAccrual extension.
		/// </summary>
		public virtual object GetExpenseSub(PXCache sender, PXFieldDefaultingEventArgs e, InventoryItem item)
		{
			return null;
		}

		protected virtual void POReceiptLine_ExpenseSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			POReceiptLine row = (POReceiptLine)e.Row;
			if (row == null || !row.ReceiptType.IsIn(POReceiptType.POReceipt, POReceiptType.POReturn))
			{
				return;
			}
			if (row.ReceiptType == POReceiptType.POReceipt && !string.IsNullOrEmpty(row.PONbr) && !string.IsNullOrEmpty(row.POType) && row.POLineNbr != null
				|| row.ReceiptType == POReceiptType.POReturn && !string.IsNullOrEmpty(row.OrigReceiptType) && !string.IsNullOrEmpty(row.OrigReceiptNbr) && row.OrigReceiptLineNbr != null)
			{
				e.Cancel = true;
				return;
			}

			switch (row.LineType)
			{
				case POLineType.Description:
					break;
				case POLineType.Freight:
					Carrier carrier = Carrier.PK.Find(this, location.Current.VCarrierID);
					e.NewValue = GetValue<Carrier.freightExpenseSubID>(carrier) ?? posetup.Current.FreightExpenseSubID;
					e.Cancel = true;
					break;
				default:
					var item = InventoryItem.PK.Find(this, row.InventoryID);
					if (item != null)
					{
						//It is possible to get there with item.StkItem == true and row.LineType == 'SV' (service) when creating return lines.
						//Return lines have pre-defined LineType, but LineType field comes after the InventoryID field so all the handlers (POReceiptLine_InventoryID_FieldUpdated for example),
						//called before the LineType value is set, are using LineType == 'SV' (dafault value).
						//Normally LineType is defaulted by POLineTypeListAttribute.InventoryIDUpdated handler, before all the orher handlers.  
						if (item.StkItem == true)
						{
							e.NewValue = null;
							break;
						}

						INPostClass postClass;
						if (POLineType.IsNonStock(row.LineType) && !POLineType.IsService(row.LineType) && (postClass = INPostClass.PK.Find(this, item.PostClassID)) != null)
						{
							var site = INSite.PK.Find(this, row.SiteID);
							try
							{
								e.NewValue = INReleaseProcess.GetSubID<INPostClass.cOGSSubID>(
									this,
									postClass.COGSAcctDefault,
									postClass.COGSSubMask,
									item,
									site,
									postClass);
							}
							catch (PXMaskArgumentException)
							{
							}
						}
						else
						{
							e.NewValue = null;
						}

						if (POLineType.IsNonStock(row.LineType))
						{
							EPEmployee employee = PXSelect<EPEmployee, Where<EPEmployee.userID, Equal<Required<EPEmployee.userID>>>>
								.Select(this, PXAccess.GetUserID());

							CRLocation companyloc = PXSelectJoin<CRLocation,
								InnerJoin<BAccountR,
									On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>,
										And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>,
								InnerJoin<Branch, On<BAccountR.bAccountID, Equal<Branch.bAccountID>>>>,
								Where<Branch.branchID, Equal<Required<POLine.branchID>>>>
								.Select(this, row.BranchID);


							int? projectID = row.ProjectID ?? ProjectDefaultAttribute.NonProject();
							PMProject project = PMProject.PK.Find(this, projectID);
							int? projectTask_SubID = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>, And<PMTask.taskID, Equal<Required<PMTask.taskID>>>>>
								.Select(this, project.ContractID, row.TaskID)
								.RowCast<PMTask>()
								.FirstOrDefault()
								?.DefaultExpenseSubID;

							POReceipt receipt = Document.Current;
							Location vendorLocation = location.Current;

							object subCD;

							if (row != null && item.StkItem != true && row.AccrueCost == true)
							{
								subCD = GetExpenseSub(sender, e, item);
							}
							else
							{
								subCD = AP.SubAccountMaskAttribute.MakeSub<APSetup.expenseSubMask>(
									this,
									apsetup.Current.ExpenseSubMask,
									new[]
									{
									GetValue<Location.vExpenseSubID>(vendorLocation),
									e.NewValue ?? GetValue<InventoryItem.cOGSSubID>((InventoryItem)item),
									GetValue<EPEmployee.expenseSubID>(employee),
									GetValue<CRLocation.cMPExpenseSubID>(companyloc),
									project.DefaultExpenseSubID,
									projectTask_SubID
									},
									new[]
									{
									typeof(Location.vExpenseSubID),
									typeof(InventoryItem.cOGSSubID),
									typeof(EPEmployee.expenseSubID),
									typeof(CRLocation.cMPExpenseSubID),
									typeof(PMProject.defaultExpenseSubID),
									typeof(PMTask.defaultExpenseSubID)
									});
							}

							sender.RaiseFieldUpdating<POReceiptLine.expenseSubID>(e.Row, ref subCD);
							e.NewValue = subCD;
						}
						else
						{
							e.NewValue = null;
						}
					}
					e.Cancel = true;
					break;
			}
		}

		protected virtual void POReceiptLine_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			POReceiptLine row = (POReceiptLine)e.Row;
			if (row != null)
			{
				e.NewValue = INTranType.InvtMult(row.TranType);
			}
		}

		protected virtual void POReceiptLine_ReceiptQty_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			POReceiptLine row = (POReceiptLine)e.Row;
			if (row != null)
			{
				e.NewValue = row.LineType == POLineType.Freight ? Decimal.One : Decimal.Zero;
			}
		}

		protected virtual void _(Events.FieldVerifying<POReceiptLine, POReceiptLine.selected> e)
		{
			if (e.Row == null)
				return;

			if (((bool?)e.NewValue) == true && !VerifyStockItem(e.Row, out string inventoryCD))
			{
				e.NewValue = false;
				throw new PXSetPropertyException(Messages.CannotReturnConvertedItem, inventoryCD);
			}
		}

		protected virtual bool VerifyStockItem(IPOReturnLineSource row, out string inventoryCD)
		{
			InventoryItem item = InventoryItem.PK.Find(this, row.InventoryID);
			bool isItemConverted = (item?.IsConverted == true && row.IsStockItem != null && row.IsStockItem != item.StkItem);
			inventoryCD = item?.InventoryCD.Trim();

			return !isItemConverted;
		}

		protected virtual void _(Events.FieldVerifying<POReceiptLineReturn, POReceiptLineReturn.selected> e)
		{
			if (e.Row == null)
				return;

			if (((bool?)e.NewValue) == true && !VerifyStockItem(e.Row, out string inventoryCD))
			{
				e.NewValue = false;
				throw new PXSetPropertyException(Messages.CannotReturnConvertedItem, inventoryCD);
			}
		}

		protected virtual void POReceiptLine_ReceiptQty_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			POReceiptLine row = e.Row as POReceiptLine;
			if (row != null && row.ManualPrice != true)
			{
				sender.SetDefaultExt<POReceiptLine.curyUnitCost>(e.Row);
			}

			if (row.ReceiptType == POReceiptType.POReturn && row.OrigReceiptNbr != null && Document.Current?.ReturnInventoryCostMode == ReturnCostMode.OriginalCost)
			{
				SetOriginalAmountsReturnLine(row);
			}
		}

		protected virtual void POLine_SubItemID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			POReceiptLine row = e.Row as POReceiptLine;
			if (row != null)
			{
				sender.SetDefaultExt<POReceiptLine.curyUnitCost>(e.Row);
			}
		}

        [PopupMessage]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void POReceiptLine_InventoryID_CacheAttached(PXCache sender)
        {
        }

        protected virtual void POReceiptLine_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			this.inventoryIDChanging = true;
			sender.SetDefaultExt<POReceiptLine.accrueCost>(e.Row);
			sender.SetDefaultExt<POReceiptLine.unitVolume>(e.Row);
			sender.SetDefaultExt<POReceiptLine.unitWeight>(e.Row);
			sender.SetDefaultExt<POReceiptLine.uOM>(e.Row);
			sender.SetDefaultExt<POReceiptLine.tranDesc>(e.Row);
			sender.SetDefaultExt<POReceiptLine.siteID>(e.Row);
			sender.SetDefaultExt<POReceiptLine.expenseAcctID>(e.Row);
			sender.SetDefaultExt<POReceiptLine.expenseSubID>(e.Row);
			sender.SetDefaultExt<POReceiptLine.pOAccrualAcctID>(e.Row);
			sender.SetDefaultExt<POReceiptLine.pOAccrualSubID>(e.Row);
			sender.SetDefaultExt<POReceiptLine.curyUnitCost>(e.Row);
		}
		
        protected virtual void POLine_ProjectID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            POLine row = e.Row as POLine;
            if (row == null) return;

            sender.SetDefaultExt<POLine.expenseSubID>(row);
        }

        protected virtual void POLine_TaskID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            POLine row = e.Row as POLine;
            if (row == null) return;

            sender.SetDefaultExt<POLine.expenseSubID>(row);
        }

		protected virtual void POReceiptLine_UOM_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			POReceiptLine row = (POReceiptLine)e.Row;
			if (e.Row != null && !this.inventoryIDChanging)
			{
				row.LastBaseReceivedQty = row.BaseReceiptQty;
				//this code addresses in PO the problem with empty UOM, similar to the problem in SO (AC-76179)
				if (e.NewValue == null && (POLineType.IsStock(row.LineType) || (POLineType.IsNonStock(row.LineType) && row.InventoryID != null)))
                {
                    throw new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(POReceiptLine.uOM)}]");
                }
			}
		}

		protected virtual void POReceiptLine_UOM_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			POReceiptLine row = (POReceiptLine) e.Row;
			string oldUOM = (string) e.OldValue;

			if (!this.inventoryIDChanging)
			{
				decimal qty = decimal.Zero;
				decimal curyUnitCost = row.CuryUnitCost ?? decimal.Zero;
				decimal curyTranUnitCost = row.CuryTranUnitCost ?? 0m;
				if (row != null && (!string.IsNullOrEmpty(oldUOM) && !string.IsNullOrEmpty(row.UOM) && row.UOM != oldUOM))
				{
					if (row.LastBaseReceivedQty.HasValue && row.LastBaseReceivedQty.Value != decimal.Zero)
					{
						qty = INUnitAttribute.ConvertFromBase<POReceiptLine.inventoryID>(sender, e.Row, row.UOM, row.LastBaseReceivedQty.Value, INPrecision.QUANTITY);
					}

					curyUnitCost = INUnitAttribute.ConvertFromBase<POReceiptLine.inventoryID>(sender, e.Row, oldUOM, curyUnitCost, INPrecision.UNITCOST);
					curyUnitCost = INUnitAttribute.ConvertToBase<POReceiptLine.inventoryID>(sender, e.Row, row.UOM, curyUnitCost, INPrecision.UNITCOST);
					if (row.CuryTranUnitCost != null)
					{
						curyTranUnitCost = INUnitAttribute.ConvertFromBase<POReceiptLine.inventoryID>(sender, e.Row, oldUOM, curyTranUnitCost, INPrecision.NOROUND);
						curyTranUnitCost = INUnitAttribute.ConvertToBase<POReceiptLine.inventoryID>(sender, e.Row, row.UOM, curyTranUnitCost, INPrecision.NOROUND);
					}
				}
				sender.SetValueExt<POReceiptLine.receiptQty>(e.Row, qty);
				sender.SetValueExt<POReceiptLine.curyUnitCost>(e.Row, curyUnitCost);
				if (row.CuryTranUnitCost != null)
				{
					sender.SetValueExt<POReceiptLine.curyTranUnitCost>(e.Row, curyTranUnitCost);
					CurrencyInfo currencyInfo = MultiCurrencyExt.GetDefaultCurrencyInfo();
					sender.SetValueExt<POReceiptLine.tranUnitCost>(e.Row, currencyInfo.CuryConvBaseRaw(row.CuryTranUnitCost ?? 0m));
				}
			}
		}

	    protected virtual void POReceiptLine_AlternateID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
	    {
		    var row = (POReceiptLine) e.Row;

		    if (row == null)
                return;

			sender.SetDefaultExt<POReceiptLine.curyUnitCost>(e.Row);
	    }

		protected virtual void POReceiptLine_SiteID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			var newSiteId = e.NewValue as int?;
			var row = (POReceiptLine)e.Row;
			if (newSiteId != null && row.LineType != null)
			{
				if (POLineType.IsDropShip(row.LineType))
				{
					var newSite = INSite.PK.Find(this, newSiteId);
					if (newSite != null && newSite.DropShipLocationID == null)
					{
						throw new PXSetPropertyException(Messages.SiteWithoutDropShipLocation) { ErrorValue = newSite.SiteCD };
					}
				}
				else if (row.LineType.IsIn(POLineType.GoodsForSalesOrder, POLineType.NonStockForSalesOrder)
					&& !string.IsNullOrEmpty(row.PONbr))
				{
					SOLineSplit linkedBlanketSplit = PXSelectReadonly<SOLineSplit,
						Where<SOLineSplit.pOType, Equal<Required<SOLineSplit.pOType>>,
							And<SOLineSplit.pONbr, Equal<Required<SOLineSplit.pONbr>>,
							And<SOLineSplit.pOLineNbr, Equal<Required<SOLineSplit.pOLineNbr>>,
							And<SOLineSplit.behavior, Equal<SOBehavior.bL>,
							And<SOLineSplit.siteID, NotEqual<Required<SOLineSplit.siteID>>>>>>>>
						.Select(this, row.POType, row.PONbr, row.POLineNbr, newSiteId);
					if (linkedBlanketSplit != null)
					{
						var newSite = INSite.PK.Find(this, newSiteId);
						throw new PXSetPropertyException(SO.Messages.WarehouseFixedBecauseLinkedToBlanket, linkedBlanketSplit.OrderNbr)
						{
							ErrorValue = newSite?.SiteCD ?? e.NewValue
						};
					}
				}
			}
		}

	    protected virtual void POReceiptLine_SiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			POReceiptLine row = (POReceiptLine)e.Row;
			if (row?.InventoryID == null
				|| (row.ReceiptType == POReceiptType.POReceipt && !string.IsNullOrEmpty(row.PONbr) && !string.IsNullOrEmpty(row.POType) && row.POLineNbr != null
				|| row.ReceiptType == POReceiptType.POReturn && !string.IsNullOrEmpty(row.OrigReceiptType) && !string.IsNullOrEmpty(row.OrigReceiptNbr) && row.OrigReceiptLineNbr != null))
			{
				return;
			}
			sender.SetDefaultExt<POReceiptLine.expenseAcctID>(e.Row);
			sender.SetDefaultExt<POReceiptLine.expenseSubID>(e.Row);
			sender.SetDefaultExt<POReceiptLine.pOAccrualAcctID>(e.Row);
			sender.SetDefaultExt<POReceiptLine.pOAccrualSubID>(e.Row);

			sender.SetDefaultExt<POReceiptLine.curyUnitCost>(e.Row);
		}

		protected virtual void POReceiptLine_LineType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<POReceiptLine.receiptQty>(e.Row);

			var row = (POReceiptLine)e.Row;
			if (row == null || !e.ExternalCall) return;
			bool isTransferReceipt = (row.ReceiptType == POReceiptType.TransferReceipt);
			bool fromPO = !string.IsNullOrEmpty(row.POType) && !string.IsNullOrEmpty(row.PONbr) && row.POLineNbr != null;
			bool hasOrigReceipt = !string.IsNullOrEmpty(row.OrigReceiptType) && !string.IsNullOrEmpty(row.OrigReceiptNbr) && row.OrigReceiptLineNbr != null;
			if (isTransferReceipt || fromPO || hasOrigReceipt) return;

			if ((row.ExpenseAcctID == null || row.ExpenseSubID == null) && IsExpenseAccountRequired(row))
			{
				sender.SetDefaultExt<POReceiptLine.expenseAcctID>(e.Row);
				sender.SetDefaultExt<POReceiptLine.expenseSubID>(e.Row);
			}
			if ((row.POAccrualAcctID == null || row.POAccrualSubID == null) && IsAccrualAccountRequired(row))
			{
				sender.SetDefaultExt<POReceiptLine.pOAccrualAcctID>(e.Row);
				sender.SetDefaultExt<POReceiptLine.pOAccrualSubID>(e.Row);
			}
		}

		protected virtual void POReceiptLine_CuryUnitCost_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			POReceiptLine row = (POReceiptLine)e.Row;
			if (row == null) return;

			if (row.AllowEditUnitCost != true || row.ManualPrice == true || (row.ReceiptType == POReceiptType.POReturn && row.OrigReceiptNbr != null))
			{
				e.NewValue = row.CuryUnitCost ?? 0m;
				return;
			}
			else
			{
				POLine poOriginLine = null;
				POOrder poOriginOrder = null;
				if (row.PONbr != null && row.POType != null && row.POLineNbr != null)
				{
					PXResult poOrigin = PXSelectReadonly2<POLine, InnerJoin<POOrder, On<POOrder.orderType, Equal<POLine.orderType>, And<POOrder.orderNbr, Equal<POLine.orderNbr>>>>, Where<POLine.orderType, Equal<Required<POLine.orderType>>,
						And<POLine.orderNbr, Equal<Required<POLine.orderNbr>>,
						And<POLine.lineNbr, Equal<Required<POLine.lineNbr>>>>>>.Select(this, row.POType, row.PONbr, row.POLineNbr);

					poOriginOrder = PXResult.Unwrap<POOrder>(poOrigin);
					poOriginLine = PXResult.Unwrap<POLine>(poOrigin);
				}

				if (poOriginLine != null && poOriginLine.UOM == row.UOM)
				{
					if (poOriginOrder?.CuryID == Document.Current.CuryID)
					{
						e.NewValue = poOriginLine.CuryUnitCost ?? 0m;
						e.Cancel = true;
						return;
					}
					else if (AllowNonBaseCurrency()) //UnitCost will be redefaulted later in case AllowNonBaseCurrency() != true
					{
						CurrencyInfo currencyInfo = MultiCurrencyExt.GetDefaultCurrencyInfo();
						e.NewValue = currencyInfo.CuryConvCury(poOriginLine.UnitCost ?? 0m, CommonSetupDecPl.PrcCst);

					e.Cancel = true;
					return;
				}
				}

				e.NewValue = DefaultUnitCost(sender, row, !AllowNonBaseCurrency()) ?? 0m;
			}
		}

		protected virtual void POReceiptLine_ManualPrice_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			POReceiptLine row = e.Row as POReceiptLine;

			if (row != null && row.ManualPrice == false && e.ExternalCall)
				sender.SetDefaultExt<POReceiptLine.curyUnitCost>(e.Row);
		}

		protected virtual void POReceiptLine_CuryUnitCost_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			POReceiptLine row = (POReceiptLine)e.Row;
			Decimal? value = (Decimal?)e.NewValue;
			if (IsNegativeCostStockItem(row, value))
			{
				throw new PXSetPropertyException(Messages.UnitCostMustBeNonNegativeForStockItem);
			}
		}

		protected virtual void POReceiptLine_CuryExtCost_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			POReceiptLine row = (POReceiptLine)e.Row;
			Decimal? value = (Decimal?)e.NewValue;
			if (IsNegativeCostStockItem(row, value))
			{
				throw new PXSetPropertyException(Messages.ExtCostMustBeNonNegativeForStockItem);
			}
		}

		protected virtual bool IsNegativeCostStockItem(POReceiptLine row, Decimal? value)
		{
			return value.HasValue && value < Decimal.Zero &&
					row.LineType.IsNotIn(
						POLineType.NonStock,
						POLineType.Service,
						POLineType.MiscCharges,
						POLineType.Freight,
						POLineType.NonStockForDropShip,
						POLineType.NonStockForSalesOrder,
						POLineType.NonStockForServiceOrder,
						POLineType.NonStockForManufacturing);
		}
		
		protected virtual void POReceiptLine_AllowComplete_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			POReceiptLine row = (POReceiptLine)e.Row;
			if (String.IsNullOrEmpty(row.POType) == false
				&& String.IsNullOrEmpty(row.PONbr) == false
				&& row.POLineNbr.HasValue)
			{

				POLineR source = GetReferencedPOLineR(row.POType, row.PONbr, row.POLineNbr);
				if (source != null && source.AllowComplete != (row.AllowComplete ?? false))
				{
					source.AllowComplete = (row.AllowComplete ?? false);
					this.Caches[typeof(POLineR)].Update(source);
				}
				this.transactions.View.RequestRefresh();
			}
		}
		protected virtual void POReceiptLine_AllowOpen_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			POReceiptLine row = (POReceiptLine)e.Row;
			if (String.IsNullOrEmpty(row.POType) == false
				&& String.IsNullOrEmpty(row.PONbr) == false
				&& row.POLineNbr.HasValue)
			{

				POLineR source = GetReferencedPOLineR(row.POType, row.PONbr, row.POLineNbr);
				if (source != null && source.AllowComplete != (row.AllowOpen ?? false))
				{
					source.AllowComplete = (row.AllowOpen ?? false);
					this.Caches[typeof(POLineR)].Update(source);
				}
				this.transactions.View.RequestRefresh();
			}
		}

		protected virtual void POReceiptLine_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
            POReceiptLine row = e.Row as POReceiptLine;
			if (row == null) return;

			bool isReceipt = row.ReceiptType == POReceiptType.POReceipt;
			bool wmsReceipt = Document.Current?.WMSSingleOrder == true;
			bool returnWithOrigCost = Document.Current?.ReturnInventoryCostMode == ReturnCostMode.OriginalCost;
			bool fromPO = !(string.IsNullOrEmpty(row.POType) || string.IsNullOrEmpty(row.PONbr) || row.POLineNbr == null);
			
			if (row.Released != true)
			{
				bool isStockItem = POLineType.IsStock(row.LineType);
				bool isNonStockItem = POLineType.IsNonStock(row.LineType);
				bool usePOAccrual = POLineType.UsePOAccrual(row.LineType);
				bool isReturn = (row.ReceiptType == POReceiptType.POReturn);
                bool isTransferReceipt = (row.ReceiptType == POReceiptType.TransferReceipt);

				bool isFreight = row.LineType == POLineType.Freight;
				bool isDropShip = (row.LineType == POLineType.GoodsForDropShip);
				bool isNonStockKit = (!row.IsStockItem ?? false) && (row.IsKit ?? false);
				bool receiptBasedBilling = (row.POAccrualType == POAccrualType.Receipt);
				bool hasOrigReceipt = !string.IsNullOrEmpty(row.OrigReceiptType) && !string.IsNullOrEmpty(row.OrigReceiptNbr) && row.OrigReceiptLineNbr != null;

				PXUIFieldAttribute.SetEnabled<POReceiptLine.branchID>(sender, row, !isTransferReceipt && (receiptBasedBilling || !usePOAccrual) && !wmsReceipt);
                PXUIFieldAttribute.SetEnabled<POReceiptLine.uOM>(sender, row, !isTransferReceipt && (isStockItem || row.LineType == POLineType.NonStock));
				PXUIFieldAttribute.SetEnabled<POReceiptLine.inventoryID>(sender, row, !isTransferReceipt && !fromPO && !isFreight && !hasOrigReceipt);
				PXUIFieldAttribute.SetEnabled<POReceiptLine.lineType>(sender, row, !isTransferReceipt && !fromPO && !hasOrigReceipt);
				PXUIFieldAttribute.SetEnabled<POReceiptLine.siteID>(sender, row, !isTransferReceipt && !wmsReceipt);

                PXUIFieldAttribute.SetEnabled<POReceiptLine.subItemID>(sender, row, (isStockItem && !isNonStockKit) && !isTransferReceipt);
				PXUIFieldAttribute.SetEnabled<POReceiptLine.receiptQty>(sender, row, isStockItem || isNonStockItem);
				PXUIFieldAttribute.SetEnabled<POReceiptLine.locationID>(sender, e.Row, isStockItem && !isDropShip);
				PXUIFieldAttribute.SetEnabled<POReceiptLine.allowComplete>(sender, e.Row, fromPO);
				PXUIFieldAttribute.SetEnabled<POReceiptLine.allowOpen>(sender, e.Row, fromPO);

				if (isDropShip)
				{
					PXUIFieldAttribute.SetEnabled<POReceiptLine.lotSerialNbr>(sender, e.Row, false);
					PXUIFieldAttribute.SetEnabled<POReceiptLine.expireDate>(sender, e.Row, false);
				}

				if (isReturn)
				{
					PXUIFieldAttribute.SetEnabled<POReceiptLine.expenseAcctID>(sender, e.Row, isNonStockItem);
					PXUIFieldAttribute.SetEnabled<POReceiptLine.expenseSubID>(sender, e.Row, isNonStockItem);
				}
				else
				{
					PXUIFieldAttribute.SetEnabled<POReceiptLine.expenseAcctID>(sender, e.Row, IsExpenseAccountRequired(row));
					PXUIFieldAttribute.SetEnabled<POReceiptLine.expenseSubID>(sender, e.Row, IsExpenseAccountRequired(row));
				}
				
				PXUIFieldAttribute.SetEnabled<POReceiptLine.pOType>(sender, e.Row, false);
				PXUIFieldAttribute.SetEnabled<POReceiptLine.pONbr>(sender, e.Row, false);
				PXUIFieldAttribute.SetEnabled<POReceiptLine.pOLineNbr>(sender, e.Row, false);
				PXUIFieldAttribute.SetEnabled<POReceiptLine.reasonCode>(sender, e.Row, isReturn);
				PXUIFieldAttribute.SetRequired<POReceiptLine.reasonCode>(sender, isReturn);

				PXUIFieldAttribute.SetEnabled<POReceiptLine.pOAccrualAcctID>(sender, e.Row, receiptBasedBilling && IsAccrualAccountRequired(row));
                PXUIFieldAttribute.SetEnabled<POReceiptLine.pOAccrualSubID>(sender, e.Row, receiptBasedBilling && IsAccrualAccountRequired(row));

                PXUIFieldAttribute.SetEnabled<POReceiptLine.sOOrderType>(sender, e.Row, false);
                PXUIFieldAttribute.SetEnabled<POReceiptLine.sOOrderNbr>(sender, e.Row, false);
                PXUIFieldAttribute.SetEnabled<POReceiptLine.sOOrderLineNbr>(sender, e.Row, false);

				bool allowChangingCost = row.AllowEditUnitCost == true &&
					(isReceipt || (isReturn && (!hasOrigReceipt || !returnWithOrigCost)));
				PXUIFieldAttribute.SetEnabled<POReceiptLine.manualPrice>(sender, e.Row, allowChangingCost);
				PXUIFieldAttribute.SetEnabled<POReceiptLine.curyUnitCost>(sender, e.Row, allowChangingCost && !isFreight);
				PXUIFieldAttribute.SetEnabled<POReceiptLine.curyExtCost>(sender, e.Row, allowChangingCost);
				PXUIFieldAttribute.SetEnabled<POReceiptLine.selected>(sender, e.Row, false);
			}
			else
			{
				PXUIFieldAttribute.SetEnabled(sender, e.Row, false);
				if (isReceipt)
					PXUIFieldAttribute.SetEnabled<POReceiptLine.selected>(sender, e.Row, true);
			}

			if (sender.GetStatus(row) == PXEntryStatus.Inserted && !VerifyStockItem(row, out string inventoryCD))
			{
				sender.RaiseExceptionHandling<POReceiptLine.inventoryID>(row, inventoryCD,
					new PXSetPropertyException(Messages.CannotReturnConvertedItem, inventoryCD));
			}
		}

		[Obsolete(Common.Messages.MethodIsObsoleteRemoveInLaterAcumaticaVersions)]
		protected virtual void POReceiptLineReturn_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
		}

		protected virtual void _(Events.RowSelecting<POReceiptLine> e)
		{
			bool fromPO = !string.IsNullOrEmpty(e.Row?.POType) && !string.IsNullOrEmpty(e.Row?.PONbr) && e.Row?.POLineNbr != null;
			if (fromPO)
			{
				// Acuminator disable once PX1042 DatabaseQueriesInRowSelecting [not needed after AC-201935]
				POLineR src = GetReferencedPOLineR(e.Row.POType, e.Row.PONbr, e.Row.POLineNbr);
				e.Row.AllowComplete = e.Row.AllowOpen = (e.Row.Released == true) ?
					(e.Row.ReceiptType != POReceiptType.POReturn ? src?.Completed == true : src?.Completed != true) :
					(src?.AllowComplete ?? false);
			}
			if (e.Row?.Released == true)
			{
				// Acuminator disable once PX1075 RaiseExceptionHandlingInEventHandlers [exception is highly unlikely]
				// Acuminator disable once PX1042 DatabaseQueriesInRowSelecting [not needed after AC-201935]
				PopulateReturnedQty(e.Row);
			}
		}

		protected virtual void _(Events.RowSelecting<POReceiptLineReturn> e)
		{
			if (e.Row != null)
			{
				// Acuminator disable once PX1075 RaiseExceptionHandlingInEventHandlers [exception is highly unlikely]
				// Acuminator disable once PX1042 DatabaseQueriesInRowSelecting [not needed after AC-201935]
				PopulateReturnedQty(e.Row);
			}
		}

		public virtual void PopulateReturnedQty(IPOReturnLineSource row)
		{
			decimal baseReturnedQty = row.BaseReturnedQty ?? 0m;
			row.ReturnedQty = (baseReturnedQty == 0m) ? 0m
				: INUnitAttribute.ConvertFromBase<POReceiptLine.inventoryID, POReceiptLine.uOM>(this.Caches[row.GetType()], row, baseReturnedQty, INPrecision.QUANTITY);
		}

		public virtual void VerifyTransferLine(PXCache sender, POReceiptLine row)
        {
            if (row.ReceiptType != POReceiptType.TransferReceipt || (Document.Current.Released ?? false))
                return;

            POReceiptLine check1 = PXSelectReadonly2<POReceiptLine, InnerJoin<POReceipt,
					On<POReceiptLine.FK.Receipt>>,
                Where<POReceiptLine.receiptType, Equal<Current<POReceiptLine.receiptType>>,
                And<POReceiptLine.receiptNbr, NotEqual<Current<POReceiptLine.receiptNbr>>,
                And<POReceipt.released, NotEqual<True>,
                And<POReceiptLine.origRefNbr, Equal<Current<POReceiptLine.origRefNbr>>,
                And<POReceiptLine.origLineNbr, Equal<Current<POReceiptLine.origLineNbr>>>>>>>>.SelectSingleBound(this, new object[] { row });

            if (check1 != null)
                throw new PXRowPersistingException(typeof(POReceiptLine.lineNbr).Name, row.LineNbr, Messages.LineCannotBeReceiptedTwicePO, check1.ReceiptNbr, check1.LineNbr);

            INTran check2 = PXSelectReadonly<INTran, 
                Where<INTran.docType, Equal<INDocType.receipt>,
                And2<
                    Where<INTran.pOReceiptType, NotEqual<Current<POReceiptLine.receiptType>>,
                    Or<INTran.pOReceiptNbr, NotEqual<Current<POReceiptLine.receiptNbr>>,
                    Or<INTran.pOReceiptLineNbr, NotEqual<Current<POReceiptLine.lineNbr>>>>>,
                And<INTran.origRefNbr, Equal<Current<POReceiptLine.origRefNbr>>,
                And<INTran.origLineNbr, Equal<Current<POReceiptLine.origLineNbr>>>>>>>.SelectSingleBound(this, new object[] { row });

            if (check2 != null)
                throw new PXRowPersistingException(typeof(POReceiptLine.lineNbr).Name, row.LineNbr, Messages.LineCannotBeReceiptedTwiceIN, check2.RefNbr, check2.LineNbr);
        }

	    protected virtual bool IsStockItem(POReceiptLine row)
	    {
            //Note: POReceiptLine.IsStockItem includes the same conditions except for GoodsForDropShip
	        return row?.LineType != null && (row.LineType == POLineType.GoodsForInventory ||
	                                         row.LineType == POLineType.GoodsForSalesOrder ||
                                             row.LineType == POLineType.GoodsForServiceOrder ||
	                                         row.LineType == POLineType.GoodsForReplenishment ||
	                                         row.LineType == POLineType.GoodsForManufacturing || 
	                                         row.LineType == POLineType.GoodsForDropShip);
	    }

		public virtual bool AllowNonBaseCurrency()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() &&
				(Document.Current?.ReceiptType == POReceiptType.POReceipt || Document.Current?.ReceiptType == POReceiptType.POReturn);
		}

		public virtual bool HasTransactions()
		{
			return transactions.SelectWindowed(0, 1).Count > 0;
		}

		protected virtual void POReceiptLine_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var row = (POReceiptLine)e.Row;
			if (row == null || e.Operation.Command() == PXDBOperation.Delete)
				return;

			bool isReturn = (row.ReceiptType == POReceiptType.POReturn);
			bool isStockItem = IsStockItem(row);
			PXDefaultAttribute.SetPersistingCheck<POReceiptLine.inventoryID>(sender, row, (POLineType.IsStock(row.LineType) || POLineType.IsNonStock(row.LineType) && !POLineType.IsService(row.LineType)) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<POReceiptLine.receiptQty>(sender, row, (isStockItem || row.LineType == POLineType.NonStock) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<POReceiptLine.baseReceiptQty>(sender, row, (isStockItem || row.LineType == POLineType.NonStock) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

			PXDefaultAttribute.SetPersistingCheck<POReceiptLine.expenseAcctID>(sender, e.Row, (!IsExpenseAccountRequired(row) || (row.LineType == POLineType.NonStock && isReturn)) ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);
			PXDefaultAttribute.SetPersistingCheck<POReceiptLine.expenseSubID>(sender, e.Row, (!IsExpenseAccountRequired(row) || (row.LineType == POLineType.NonStock && isReturn)) ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);

			PXDefaultAttribute.SetPersistingCheck<POReceiptLine.pOAccrualAcctID>(sender, e.Row, IsAccrualAccountRequired(row) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<POReceiptLine.pOAccrualSubID>(sender, e.Row, IsAccrualAccountRequired(row) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<POReceiptLine.siteID>(sender, row, (row.LineType == POLineType.Description || row.LineType == POLineType.Freight || POLineType.IsService(row.LineType)) ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);

			PXDefaultAttribute.SetPersistingCheck<POReceiptLine.uOM>(sender, e.Row, (POLineType.IsStock(row.LineType) || (POLineType.IsNonStock(row.LineType) && row.InventoryID != null)) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

			PXDefaultAttribute.SetPersistingCheck<POReceiptLine.reasonCode>(sender, e.Row,
				isReturn ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);

			bool isIntercompanyReceipt = row.IsIntercompany == true && row.ReceiptType == POReceiptType.POReceipt;
			if (Document.Current?.Hold == false
				&& (!isIntercompanyReceipt && row.ReceiptQty <= decimal.Zero || row.ReceiptQty < decimal.Zero))
			{
				sender.RaiseExceptionHandling<POReceiptLine.receiptQty>(row, row.ReceiptQty,
					new PXSetPropertyException(Messages.POLineQuantityMustBeGreaterThanZero, PXErrorLevel.Error));
			}

			if (!(string.IsNullOrEmpty(row.POType) || string.IsNullOrEmpty(row.PONbr) || (row.LineNbr == null)))
			{
				CheckRctForPOQuantityRule(sender, row, false);
			}

			CheckReturnQty(sender, row);
		}

		protected virtual void CheckReturnQty(PXCache sender, POReceiptLine row)
		{
			if (row.ReceiptType == POReceiptType.POReturn && !string.IsNullOrEmpty(row.OrigReceiptType) && !string.IsNullOrEmpty(row.OrigReceiptNbr) && row.OrigReceiptLineNbr != null
				&& row.BaseReceiptQty > row.BaseOrigQty)
			{
				sender.RaiseExceptionHandling<POReceiptLine.receiptQty>(row, row.ReceiptQty,
					new PXSetPropertyException(Messages.ReturnedQtyMoreThanReceivedQtyInvt, PXErrorLevel.Error,
						PXForeignSelectorAttribute.GetValueExt<POReceiptLine.inventoryID>(sender, row)));
			}
		}

		protected virtual bool IsExpenseAccountRequired(POReceiptLine line)
		{
			return line.LineType != POLineType.Description && (!POLineType.IsStock(line.LineType) || POLineType.IsProjectDropShip(line.LineType));
		}

        protected virtual bool IsAccrualAccountRequired(POReceiptLine line)
        {
			return line.ReceiptType != POReceiptType.TransferReceipt && POLineType.UsePOAccrual(line.LineType) && line.DropshipExpenseRecording != DropshipExpenseRecordingOption.OnBillRelease;
        }

		protected virtual void POReceiptLine_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			POReceiptLine row = (POReceiptLine)e.Row;
			POReceiptLine oldRow = (POReceiptLine)e.OldRow;

			if (IsCopyPasteContext && row.LineType == POLineType.GoodsForDropShip)
			{
				row.LineType = POLineType.GoodsForInventory;
			}

			ClearUnused(row);

			POLine poOriginLine = null;
			if (row.PONbr != null && row.POType != null && row.POLineNbr != null)
			{
					poOriginLine = PXSelectReadonly<POLine, 
						Where<POLine.orderType, Equal<Required<POLine.orderType>>,
					And<POLine.orderNbr, Equal<Required<POLine.orderNbr>>,
						And<POLine.lineNbr, Equal<Required<POLine.lineNbr>>>>>>
						.Select(this, row.POType, row.PONbr, row.POLineNbr);
			}

			if (row.BaseReceiptQty != oldRow.BaseReceiptQty)
			{
				this.UpdatePOLineCompleteFlag(row, false, poOriginLine);
			}

			try
			{
				CheckRctForPOQuantityRule(sender, row, true, poOriginLine);
			}
			finally
			{
				this.inventoryIDChanging = false;
			}

			if (e.ExternalCall || sender.Graph.IsImport)
			{
				if (sender.ObjectsEqual<POReceiptLine.inventoryID>(e.Row, e.OldRow) && sender.ObjectsEqual<POReceiptLine.subItemID>(e.Row, e.OldRow)
				&& sender.ObjectsEqual<POReceiptLine.alternateID>(e.Row, e.OldRow) && sender.ObjectsEqual<POReceiptLine.uOM>(e.Row, e.OldRow)
				&& sender.ObjectsEqual<POReceiptLine.receiptQty>(e.Row, e.OldRow) && sender.ObjectsEqual<POReceiptLine.branchID>(e.Row, e.OldRow)
				&& sender.ObjectsEqual<POReceiptLine.siteID>(e.Row, e.OldRow) && sender.ObjectsEqual<POReceiptLine.manualPrice>(e.Row, e.OldRow)
				&& (!sender.ObjectsEqual<POReceiptLine.unitCost>(e.Row, e.OldRow)
				|| !sender.ObjectsEqual<POReceiptLine.curyUnitCost>(e.Row, e.OldRow)
				|| !sender.ObjectsEqual<POReceiptLine.curyExtCost>(e.Row, e.OldRow)))
				{
					sender.SetValueExt<POReceiptLine.manualPrice>(row, true);
				}
			}

		}

		protected virtual void POReceiptLine_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
			=> ChangeCopyPasteLineType(e.Row as POReceiptLine);

		protected virtual void POReceiptLine_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
			=> ChangeCopyPasteLineType(e.NewRow as POReceiptLine);

		protected virtual void ChangeCopyPasteLineType(POReceiptLine line)
		{
			if (IsCopyPasteContext)
			{
				switch (line?.LineType)
				{
					case POLineType.GoodsForSalesOrder:
					case POLineType.GoodsForServiceOrder:
					case POLineType.GoodsForReplenishment:
					case POLineType.GoodsForDropShip:
					case POLineType.GoodsForManufacturing:
					case POLineType.GoodsForProject:
						line.LineType = POLineType.GoodsForInventory;
						break;
					case POLineType.NonStockForSalesOrder:
					case POLineType.NonStockForServiceOrder:
					case POLineType.NonStockForDropShip:
					case POLineType.NonStockForManufacturing:
					case POLineType.NonStockForProject:
						line.LineType = POLineType.NonStock;
						break;
				}
			}
		}

		protected virtual void POReceiptLine_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            POReceiptLine row = (POReceiptLine)e.Row;

			ClearUnused(row);
            POReceipt doc = (POReceipt)this.Document.Current;
            POLine poOriginLine = null;
            if (row.PONbr != null && row.POType != null && row.POLineNbr != null)
			{
				poOriginLine = PXSelectReadonly<POLine, Where<POLine.orderType, Equal<Required<POLine.orderType>>,
					And<POLine.orderNbr, Equal<Required<POLine.orderNbr>>,
						And<POLine.lineNbr, Equal<Required<POLine.lineNbr>>>>>>.Select(this, row.POType, row.PONbr, row.POLineNbr);
				this.UpdatePOLineCompleteFlag(row, false, poOriginLine);
			}
            if (poOriginLine == null && (row.UnitCost == null || row.UnitCost == Decimal.Zero))
            {
				row.CuryUnitCost = DefaultUnitCost(sender, row, !AllowNonBaseCurrency());
            }

            try
            {
                CheckRctForPOQuantityRule(sender, row, true, poOriginLine);
            }
            finally
            {
                this.inventoryIDChanging = false;
            }
        }

        protected virtual void POReceiptLine_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            POReceiptLine row = (POReceiptLine)e.Row;
            if (row.PONbr != null && row.POType != null)
            {
				if (row.POLineNbr != null && row.BaseReceiptQty >= 0)
					this.UpdatePOLineCompleteFlag(row, true, null);
                this.DeleteUnusedReference(row, this.isDeleting);
            }
        }

		protected bool IsRequired(string poLineType)
		{
			switch (poLineType)
			{
				case PX.Objects.PO.POLineType.NonStock:
				case PX.Objects.PO.POLineType.Freight:
				case PX.Objects.PO.POLineType.Service:
					return true;

				default:
					return false;
			}
		}

		protected virtual void POReceiptLine_Selected_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			POReceiptLine row = e.Row as POReceiptLine;
			if (row == null || (bool)e.NewValue != true) return;

			bool completelyReturned = (row.BaseReturnedQty >= row.BaseReceiptQty);
			if (completelyReturned)
			{
				this.transactions.View.RequestRefresh();	// needed for Select All checkbox
				throw new PXSetPropertyException(Messages.ReceiptLineIsCompletelyReturned, PXErrorLevel.Warning);
			}
		}

		#endregion

		#region POLine Events

		[PXStringList(
			new string[] { 
				POLineType.GoodsForInventory, 
				POLineType.GoodsForSalesOrder, 
				POLineType.GoodsForServiceOrder, 
				POLineType.GoodsForReplenishment, 
				POLineType.GoodsForDropShip, 
				POLineType.NonStockForDropShip, 
				POLineType.GoodsForProject, 
				POLineType.NonStockForProject, 
				POLineType.NonStockForSalesOrder, 
				POLineType.NonStockForServiceOrder, 
				POLineType.NonStock, 
				POLineType.Service, 
				POLineType.Freight, 
				POLineType.Description },
			new string[] {
				Messages.GoodsForInventory, 
				Messages.GoodsForSalesOrder, 
				Messages.GoodsForServiceOrder, 
				Messages.GoodsForReplenishment,
				Messages.GoodsForDropShip, 
				Messages.NonStockForDropShip, 
				Messages.GoodsForProject, 
				Messages.NonStockForProject, 
				Messages.NonStockForSalesOrder, 
				Messages.NonStockForServiceOrder, 
				Messages.NonStockItem, 
				Messages.Service, 
				Messages.Freight, 
				Messages.Description }
			)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		public void POLineS_LineType_CacheAttached(PXCache sender) { }


		protected virtual void POLine_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            e.Cancel = true;
        }
        #endregion

        #region POOrderFilter events
        protected virtual void POOrderFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            POOrderFilter row = (POOrderFilter)e.Row;
            POReceipt doc = this.Document.Current;
            if (row != null && doc != null)
            {
                if (!String.IsNullOrEmpty(doc.POType))
                    row.OrderType = doc.POType;
                if (doc.ShipToBAccountID.HasValue)
                    row.ShipToBAccountID = doc.ShipToBAccountID;
                if (doc.ShipToLocationID.HasValue)
                    row.ShipToLocationID = doc.ShipToLocationID;
                PXUIFieldAttribute.SetEnabled<POOrderFilter.orderType>(sender, row, (String.IsNullOrEmpty(doc.POType)));

                {
                    PXStringListAttribute.SetList<POOrderFilter.orderType>(sender, null,
                        new string[] { POOrderType.RegularOrder, POOrderType.DropShip, POOrderType.ProjectDropShip },
                        new string[] { Messages.RegularOrder, Messages.DropShip, Messages.ProjectDropShip });

                    PXUIFieldAttribute.SetEnabled<POOrderFilter.shipToBAccountID>(sender, row, (!doc.ShipToBAccountID.HasValue));
                    PXUIFieldAttribute.SetEnabled<POOrderFilter.shipToLocationID>(sender, row, (!doc.ShipToLocationID.HasValue));
                    PXUIFieldAttribute.SetVisible<POOrderFilter.shipToBAccountID>(sender, row, (row.OrderType == POOrderType.DropShip || string.IsNullOrEmpty(row.OrderType)));
                    PXUIFieldAttribute.SetVisible<POOrderFilter.shipToLocationID>(sender, row, (row.OrderType == POOrderType.DropShip || string.IsNullOrEmpty(row.OrderType)));
                }
                bool vendorVisible = row.VendorID == null && this.Document.Current.VendorID == null;
                PXUIFieldAttribute.SetVisible<POLineS.orderNbr>(this.Caches[typeof(POLineS)], null, row.OrderNbr == null);
                PXUIFieldAttribute.SetVisible<POLineS.vendorID>(this.Caches[typeof(POLineS)], null, vendorVisible);
                PXUIFieldAttribute.SetVisible<POLineS.vendorLocationID>(this.Caches[typeof(POLineS)], null, vendorVisible);
                //	this.addPOOrderLine2.SetVisible(row.AllowAddLine == true);
                this.addPOOrderLine2.SetEnabled(row.AllowAddLine == true);

            }
        }

        protected virtual void POOrderFilter_OrderNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            POOrderFilter row = (POOrderFilter)e.Row;
            this.ClearSelectionCache<POLineS.selected>(this.poLinesSelection.Cache);
        }

        protected virtual void POOrderFilter_OrderType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            POOrderFilter row = (POOrderFilter)e.Row;
            sender.SetDefaultExt<POOrderFilter.shipToBAccountID>(e.Row);
            sender.SetValuePending<POOrderFilter.orderNbr>(e.Row, null);
            this.poLinesSelection.Cache.Clear();
            this.openOrders.Cache.Clear();
        }

        protected virtual void POOrderFilter_ShipToBAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            POOrderFilter row = (POOrderFilter)e.Row;
            sender.SetDefaultExt<POOrderFilter.shipToLocationID>(e.Row);
            this.poLinesSelection.Cache.Clear();
            this.openOrders.Cache.Clear();
        }
        #endregion

        #region AddReceiptLine Events
		protected virtual void POReceiptLineS_BarCode_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (e.ExternalCall)
			{
				var xRef = GetCrossReference((POReceiptLineS)e.Row);

				if (xRef != null)
				{
					sender.SetValue<POReceiptLineS.inventoryID>(e.Row, null);
					sender.SetValuePending<POReceiptLineS.inventoryID>(e.Row, ((InventoryItem)xRef).InventoryCD);
					sender.SetValuePending<POReceiptLineS.subItemID>(e.Row, ((INSubItem)xRef).SubItemCD);
					sender.SetValuePending<POReceiptLineS.uOM>(e.Row, ((INItemXRef)xRef).UOM);
				}
				else
				{
					sender.SetValuePending<POReceiptLineS.inventoryID>(e.Row, null);
					sender.SetValuePending<POReceiptLineS.subItemID>(e.Row, null);
					sender.SetValuePending<POReceiptLineS.uOM>(e.Row, null);
				}
			}
		}
        protected virtual void POReceiptLineS_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            if (e.ExternalCall)
            {
                POReceiptLineS row = e.Row as POReceiptLineS;
                if (e.OldValue != null && row.InventoryID != null)
                    row.BarCode = null;
                sender.SetDefaultExt<POReceiptLineS.subItemID>(e);
            }
        }

        protected virtual void POReceiptLineS_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
        {
            POReceiptLineS row = e.NewRow as POReceiptLineS;
            POReceiptLineS old = e.Row as POReceiptLineS;
            if (!e.ExternalCall || row == null || old == null)
                return;

            if (row.ByOne != old.ByOne && row.ByOne == true)
                row.Qty = 1m;

            if (row.InventoryID != null &&
                    (row.InventoryID != old.InventoryID ||
                     row.SubItemID != old.SubItemID ||
                     row.VendorLocationID != old.VendorLocationID ||
                     row.ShipFromSiteID != old.ShipFromSiteID ||
                     row.FetchMode == true))
            {
                if (addLinePopupHandler.View.Answer == WebDialogResult.None)
                {
                    this.filter.Cache.Remove(this.filter.Current);
                    this.filter.Cache.Insert(new POOrderFilter()
                    {
                        VendorID = row.VendorID,
                        VendorLocationID = row.VendorLocationID,
                        ShipFromSiteID = row.ShipFromSiteID,
                        OrderType = row.POType,
                        OrderNbr = row.PONbr,
						ReceiptType = row.ReceiptType,
                        InventoryID = row.InventoryID,
                        SubItemID = row.SubItemID,
                        ResetFilter = true,
                        AllowAddLine = false
                    });
                }

                addLinePopupHandler.TryGetSourceItem(row);
            }
        }

		protected virtual void POReceiptLineS_UOM_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var xRef = GetCrossReference((POReceiptLineS)e.Row);
			if (xRef != null)
			{
				e.NewValue = ((INItemXRef) xRef).UOM;
			}
		}

		private PXResult<INItemXRef, InventoryItem, INSubItem> GetCrossReference(POReceiptLineS row)
		{
			return (PXResult<INItemXRef, InventoryItem, INSubItem>)
				PXSelectJoin<INItemXRef,
				InnerJoin<InventoryItem,
					On2<INItemXRef.FK.InventoryItem,
					And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
					And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.noPurchases>,
					And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>>>>>,
				InnerJoin<INSubItem,
					On<INItemXRef.FK.SubItem>>>,
				Where<INItemXRef.alternateID, Equal<Current<POReceiptLineS.barCode>>,
					And<INItemXRef.alternateType, In3<INAlternateType.barcode, INAlternateType.gIN>>>>
				.SelectSingleBound(this, new object[] {row});
		}

		public virtual POLineR GetReferencedPOLineR(string poType, string poNbr, int? poLineNbr)
		{
			if (string.IsNullOrEmpty(poType) || string.IsNullOrEmpty(poNbr) || poLineNbr == null)
				return null;
			var poLine = new POLineR
			{
				OrderType = poType,
				OrderNbr = poNbr,
				LineNbr = poLineNbr,
			};
			poLine = purchaseLinesUPD.Locate(poLine);
			if (poLine == null)
			{
				poLine = PXSelect<POLineR,
					Where<POLineR.orderType, Equal<Required<POLineR.orderType>>,
						And<POLineR.orderNbr, Equal<Required<POLineR.orderNbr>>,
						And<POLineR.lineNbr, Equal<Required<POLineR.lineNbr>>>>>>
					.Select(this, poType, poNbr, poLineNbr);
				if (poLine != null)
				{
					purchaseLinesUPD.Cache.Hold(poLine);
				}
			}
			return poLine;
		}

	    public abstract class PopupHandler
        {
            protected POReceiptEntry _graph;
            public PopupHandler(POReceiptEntry graph)
            {
                _graph = graph;
            }

            public abstract PXView View { get; }
            public abstract object GetSourceItem();
            public abstract void TryGetSourceItem(POReceiptLineS filter);
            public abstract void SetFilterToSource(PXCache sender, POReceiptLineS filter, object _source);
            public abstract void SetFilterToError(PXCache sender, POReceiptLineS filter);
        }

        public class AddReceiptPopupHandler : PopupHandler
        {
            public AddReceiptPopupHandler(POReceiptEntry graph)
                : base(graph)
            {
            }

            public override PXView View
            {
                get
                {
                    return _graph.poLinesSelection.View;
                }
            }

            public override void TryGetSourceItem(POReceiptLineS filter)
            {
                PXResultset<POLineS> polineResultSet = _graph.poLinesSelection.Select();
                POLineS source = null;
                int openLineCount = 0;
                foreach (POLineS itemsource in polineResultSet)
                {
                    POLineR polineR = _graph.GetReferencedPOLineR(itemsource.OrderType, itemsource.OrderNbr, itemsource.LineNbr);
                    if (polineR == null || itemsource.OrderQty - polineR.ReceivedQty <= 0)
                    {
                        itemsource.Selected = false;
                        continue;
                    }
                    if (itemsource.Selected == true)
                    {
                        source = itemsource;
                        openLineCount = 1;
                        break;
                    }
                    openLineCount += 1;
                    if (source == null) source = itemsource;
                }

                if (openLineCount > 1)
                {
                    filter.FetchMode = true;
                    source = null;
                    if (_graph.poLinesSelection.AskExt((graph, view) => graph.Views[view].Cache.Clear()) == WebDialogResult.OK)
                    {
                        foreach (POLineS s in _graph.poLinesSelection.Cache.Updated)
                        {
                            if (s.Selected == true)
                                source = s;
                        }
                    }
                    if (source == null && _graph.poLinesSelection.View.Answer == WebDialogResult.OK)
                        _graph.poLinesSelection.AskExt((graph, view) => graph.Views[view].Cache.Clear());

                }
                _graph.poLinesSelection.View.SetAnswer(null, WebDialogResult.None);
                filter.FetchMode = false;
            }

            public override object GetSourceItem()
        {
                PXResultset<POLineS> polineResultSet = _graph.poLinesSelection.Select();
                POLineS source = null;
                int openLineCount = 0;
                foreach (POLineS itemsource in polineResultSet)
                {
                    POLineR polineR = _graph.GetReferencedPOLineR(itemsource.OrderType, itemsource.OrderNbr, itemsource.LineNbr);
                    if (polineR == null ||
                        (_graph.Document.Current.ReceiptType == POReceiptType.POReceipt && itemsource.OrderQty - polineR.ReceivedQty <= 0) ||
                        (_graph.Document.Current.ReceiptType == POReceiptType.POReturn && polineR.ReceivedQty <= 0))
                    {
                        itemsource.Selected = false;
                        continue;
                    }
                    if (itemsource.Selected == true)
                    {
                        source = itemsource;
                        openLineCount = 1;
                        break;
                    }
                    openLineCount += 1;
                    if (source == null) source = itemsource;
                }

                if (openLineCount > 1)
                {
                    source = null;
                    if (_graph.poLinesSelection.View.Answer == WebDialogResult.OK)
                    {
                        foreach (POLineS s in _graph.poLinesSelection.Cache.Updated)
                        {
                            if (s.Selected == true)
                                source = s;
                        }
                    }
                }

                return source;
            }

            public override void SetFilterToSource(PXCache sender, POReceiptLineS filter, object _source)
                {
                POLineS source = _source as POLineS;

                if (filter.VendorID == null)
                    {
                        POOrder order = PXSelect<POOrder,
                            Where<POOrder.orderType, Equal<Required<POOrder.orderType>>,
                                And<POOrder.orderNbr, Equal<Required<POOrder.orderNbr>>>>>
                        .SelectWindowed(_graph, 0, 1, source.OrderType, source.OrderNbr);
                    filter.VendorID = order.VendorID;
                    filter.VendorLocationID = order.VendorLocationID;
                    }
                filter.SubItemID = source.SubItemID;
                filter.UOM = source.UOM;
                filter.SiteID = source.SiteID;
                filter.POType = source.OrderType;
                filter.PONbr = source.OrderNbr;
                filter.POLineNbr = source.LineNbr;
                filter.UnitCost = source.UnitCost;
                filter.POAccrualType = source.POAccrualType;
                sender.SetDefaultExt<POReceiptLineS.locationID>(filter);

                InventoryItem item = InventoryItem.PK.Find(_graph, filter.InventoryID);
				INLotSerClass lotclass = INLotSerClass.PK.Find(_graph, item?.LotSerClassID);

				Decimal? openQty = null;
                Decimal? baseOpenQty = null;
                if (source != null)
                {
                    POLineR polineR = source != null
                                        ? _graph.GetReferencedPOLineR(source.OrderType, source.OrderNbr, source.LineNbr)
                                        : null;

                    if (polineR != null)
                    {
                        openQty = _graph.Document.Current.ReceiptType == POReceiptType.POReceipt
                                    ? source.OrderQty - polineR.ReceivedQty
                                    : polineR.ReceivedQty;
                        baseOpenQty = _graph.Document.Current.ReceiptType == POReceiptType.POReceipt
                                        ? source.BaseOrderQty - polineR.BaseReceivedQty
                                        : polineR.BaseReceivedQty;
                    }
                    if (openQty < 0)
                    {
                        openQty = 0;
                        baseOpenQty = 0;
                    }
                }

                decimal qty = openQty.GetValueOrDefault();

                if (lotclass != null &&
                    lotclass.LotSerTrack == INLotSerTrack.SerialNumbered && lotclass.LotSerAssign == INLotSerAssign.WhenReceived)
                {
                    qty = 1;
                    filter.UOM = item.BaseUnit;
                }
                else if (filter.ByOne == true || source == null)
                    qty = 1;

                sender.SetValueExt<POReceiptLineS.receiptQty>(filter, qty);

                if (baseOpenQty != null && filter.BaseReceiptQty > baseOpenQty)
                    sender.SetValueExt<POReceiptLineS.receiptQty>(filter, 0m);

                if (source != null)
                    sender.SetValueExt<POReceiptLineS.unitCost>(filter, source.UnitCost);
            }

            public override void SetFilterToError(PXCache sender, POReceiptLineS filter)
            {
                filter.PONbr = null;
                filter.POLineNbr = null;
                sender.SetDefaultExt<POReceiptLineS.uOM>(filter);
                sender.SetDefaultExt<POReceiptLineS.siteID>(filter);
                sender.SetValueExt<POReceiptLineS.receiptQty>(filter, 0m);

                if (filter.VendorID != null)
                    sender.SetDefaultExt<POReceiptLineS.unitCost>(filter);
                sender.RaiseExceptionHandling<POReceiptLineS.pONbr>
                    (filter, null, new PXSetPropertyException(Messages.POSourceNotFound, PXErrorLevel.Warning));
            }
        }

        public class AddTransferPopupHandler : PopupHandler
        {
            public AddTransferPopupHandler(POReceiptEntry graph)
                : base(graph)
            {
            }

            public override PXView View
            {
                get
                {
                    return _graph.intranSelection.View;
                }
            }

            public override void TryGetSourceItem(POReceiptLineS filter)
            {
                PXResultset<INTran> intranResultSet = _graph.intranSelection.Select();
                INTran source = null;
                int openLineCount = 0;
                foreach (INTran itemsource in intranResultSet)
                {
                    //if (false)
                    //{
                    //    itemsource.Selected = false;
                    //    continue;
                    //}
                    if (itemsource.Selected == true)
                    {
                        source = itemsource;
                        openLineCount = 1;
                        break;
                    }
                    openLineCount += 1;
                    if (source == null) source = itemsource;
                }

                if (openLineCount > 1)
                {
                    filter.FetchMode = true;
                    source = null;
                    if (_graph.intranSelection.AskExt((graph, view) => graph.Views[view].Cache.Clear()) == WebDialogResult.OK)
                    {
                        foreach (INTran s in _graph.intranSelection.Cache.Updated)
                        {
                            if (s.Selected == true)
                                source = s;
                        }
                    }
                    if (source == null && _graph.intranSelection.View.Answer == WebDialogResult.OK)
                        _graph.intranSelection.AskExt((graph, view) => graph.Views[view].Cache.Clear());

                }
                _graph.intranSelection.View.SetAnswer(null, WebDialogResult.None);
                filter.FetchMode = false;
            }


            public override object GetSourceItem()
            {
                PXResultset<INTran> intranResultSet = _graph.intranSelection.Select();
                INTran source = null;
                int openLineCount = 0;
                foreach (INTran itemsource in intranResultSet)
                {
                    //if (false)
                    //{
                    //    itemsource.Selected = false;
                    //    continue;
                    //}
                    if (itemsource.Selected == true)
                    {
                        source = itemsource;
                        openLineCount = 1;
                        break;
                    }
                    openLineCount += 1;
                    if (source == null) source = itemsource;
                }

                if (openLineCount > 1)
                {
                    source = null;
                    if (_graph.intranSelection.View.Answer == WebDialogResult.OK)
                    {
                        foreach (INTran s in _graph.intranSelection.Cache.Updated)
                        {
                            if (s.Selected == true)
                                source = s;
                        }
                    }
                }

                return source;
                }

            public override void SetFilterToSource(PXCache sender, POReceiptLineS filter, object _source)
            {
                INTran source = _source as INTran;

                //if (filter.VendorID == null)
                //{
                //    POOrder order = PXSelect<POOrder,
                //        Where<POOrder.orderType, Equal<Required<POOrder.orderType>>,
                //            And<POOrder.orderNbr, Equal<Required<POOrder.orderNbr>>>>>
                //        .SelectWindowed(_graph, 0, 1, source.OrderType, source.OrderNbr);
                //    filter.VendorID = order.VendorID;
                //    filter.VendorLocationID = order.VendorLocationID;
                //}
                filter.SubItemID = source.SubItemID;
                filter.UOM = source.UOM;
                filter.ShipFromSiteID = source.SiteID;
                filter.SiteID = source.ToSiteID;
                filter.SOOrderType = source.SOOrderType;
                filter.SOOrderNbr = source.SOOrderNbr;
                filter.SOOrderLineNbr = source.SOOrderLineNbr;
                filter.SOShipmentNbr = source.SOShipmentNbr;
                filter.OrigRefNbr = source.RefNbr;
                filter.OrigLineNbr = source.LineNbr;
                filter.UnitCost = source.UnitCost;
                sender.SetDefaultExt<POReceiptLineS.locationID>(filter);

				InventoryItem item = InventoryItem.PK.Find(_graph, filter.InventoryID);
				INLotSerClass lotclass = INLotSerClass.PK.Find(_graph, item?.LotSerClassID);

				Decimal? openQty = source.Qty;
                Decimal? baseOpenQty = source.BaseQty;

                decimal qty = openQty.GetValueOrDefault();

                if (lotclass != null &&
                    lotclass.LotSerTrack == INLotSerTrack.SerialNumbered && lotclass.LotSerAssign == INLotSerAssign.WhenReceived)
                {
                    qty = 1;
                    filter.UOM = item.BaseUnit;
                }
                else if (filter.ByOne == true || source == null)
                    qty = 1;

                sender.SetValueExt<POReceiptLineS.receiptQty>(filter, qty);

                if (baseOpenQty != null && filter.BaseReceiptQty > baseOpenQty)
                    sender.SetValueExt<POReceiptLineS.receiptQty>(filter, 0m);

                if (source != null)
                    sender.SetValueExt<POReceiptLineS.unitCost>(filter, source.UnitCost);
            }

            public override void SetFilterToError(PXCache sender, POReceiptLineS filter)
            {
                filter.SOOrderType = null;
                filter.SOOrderNbr = null;
                filter.SOOrderLineNbr = null;
                filter.OrigRefNbr = null;
                filter.OrigLineNbr = null;
                sender.SetDefaultExt<POReceiptLineS.uOM>(filter);
                sender.SetDefaultExt<POReceiptLineS.siteID>(filter);
                sender.SetValueExt<POReceiptLineS.receiptQty>(filter, 0m);

                if (filter.VendorID != null)
                    sender.SetDefaultExt<POReceiptLineS.unitCost>(filter);
                sender.RaiseExceptionHandling<POReceiptLineS.sOOrderNbr>
                    (filter, null, new PXSetPropertyException(Messages.INSourceNotFound, PXErrorLevel.Error));
            }
        }

        public PopupHandler addLinePopupHandler;

        protected virtual void POReceiptLineS_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            POReceiptLineS row = e.Row as POReceiptLineS;
            POReceiptLineS old = e.OldRow as POReceiptLineS;
            if (!e.ExternalCall || row == null || old == null)
                return;

            if (row.ByOne != old.ByOne && row.ByOne == true && row.Qty != 1)
            {
                sender.SetValueExt<POReceiptLineS.receiptQty>(row, 1m);
            }

            if (row.InventoryID != null &&
                  (row.InventoryID != old.InventoryID ||
                     row.SubItemID != old.SubItemID ||
                     row.VendorLocationID != old.VendorLocationID ||
                     row.ShipFromSiteID != old.ShipFromSiteID ||
                     row.FetchMode == true))
            {

                object source = addLinePopupHandler.GetSourceItem();
                if (source != null)
                {
                    addLinePopupHandler.SetFilterToSource(sender, row, source);
            }
                else
                {
                    addLinePopupHandler.SetFilterToError(sender, row);
                }
            }
            else if (row.UOM != old.UOM && row.InventoryID == old.InventoryID && row.InventoryID != null)
            {
                if (old.UOM != null && row.UOM != null)
                {
                    Decimal? cost = row.UnitCost;
                    string oldUOM = old.UOM;
                    decimal qty = decimal.Zero;
                    decimal unitCost = row.UnitCost ?? Decimal.Zero;

                    qty = (old.Qty == row.Qty)
                            ? INUnitAttribute.ConvertFromBase<POReceiptLineS.inventoryID>(sender, e.Row, row.UOM,
                                                                                          old.BaseReceiptQty.Value,
                                                                                          INPrecision.QUANTITY)
                            : row.Qty.Value;

                    if (row.VendorID != null && row.InventoryID != null && row.UnitCost == 0m)
                        sender.SetDefaultExt<POReceiptLineS.unitCost>(row);

                    if (old.UnitCost == row.UnitCost)
                    {
                        unitCost = INUnitAttribute.ConvertFromBase<POReceiptLineS.inventoryID>(sender, e.Row, oldUOM, unitCost,
                                                                                                   INPrecision.UNITCOST);
                        unitCost = INUnitAttribute.ConvertToBase<POReceiptLineS.inventoryID>(sender, e.Row, row.UOM, unitCost,
                                                                                                 INPrecision.UNITCOST);
                    }
                    else
                        unitCost = row.UnitCost.Value;

                    sender.SetValueExt<POReceiptLineS.receiptQty>(e.Row, qty);
                    sender.SetValueExt<POReceiptLineS.unitCost>(e.Row, unitCost);
                }
                else
                    sender.SetDefaultExt<POReceiptLineS.unitCost>(row);
            }
            bool complete = false;
            if (row.AutoAddLine == true && row.Qty > 0 && row.VendorID != null && row.InventoryID != null && row.BarCode != null && row.SubItemID != null && row.LocationID != null)
            {
                complete = row.LotSerialNbr != null;
                if (!complete)
                {
                    INLotSerClass lotclass =
                        PXSelectJoin<INLotSerClass,
                            InnerJoin<InventoryItem, On<INLotSerClass.lotSerClassID, Equal<InventoryItem.lotSerClassID>>>,
                            Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                            .SelectWindowed(this, 0, 1, row.InventoryID);
                    complete = lotclass.LotSerTrack == INLotSerTrack.NotNumbered || lotclass.AutoNextNbr == true;
                }
                if (complete)
                {
                    AddReceiptLine();
                    ResetReceiptFilter(true);
                }
            }
            if (!complete)
                row.Description = null;

        }
        protected virtual void POReceiptLineS_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            POReceiptLineS row = e.Row as POReceiptLineS;
            if (row != null)
            {
				InventoryItem item = InventoryItem.PK.Find(this, row.InventoryID);
				INLotSerClass lotclass = INLotSerClass.PK.Find(this, item?.LotSerClassID);

				bool requestLotSer = lotclass != null && lotclass.LotSerTrack != INLotSerTrack.NotNumbered &&
                                     lotclass.LotSerAssign == INLotSerAssign.WhenReceived;
                bool isTransferReceipt = row.ReceiptType == POReceiptType.TransferReceipt;

                PXUIFieldAttribute.SetEnabled<POReceiptLineS.pOType>(sender, row, row.PONbr == null);
                PXUIFieldAttribute.SetEnabled<POReceiptLineS.lotSerialNbr>(sender, row, requestLotSer && lotclass.AutoNextNbr == false);
                PXUIFieldAttribute.SetEnabled<POReceiptLineS.expireDate>(sender, row, requestLotSer && lotclass.LotSerTrackExpiration == true);
                PXUIFieldAttribute.SetEnabled<POReceiptLineS.vendorID>(sender, row, row.PONbr == null && row.ReceiptVendorID == null);
                PXUIFieldAttribute.SetEnabled<POReceiptLineS.vendorLocationID>(sender, row, row.PONbr == null && row.ReceiptVendorLocationID == null);
                PXUIFieldAttribute.SetEnabled<POReceiptLineS.siteID>(sender, row, row.PONbr == null && !isTransferReceipt);
                PXUIFieldAttribute.SetEnabled<POReceiptLineS.uOM>(sender, row, !(requestLotSer && lotclass.LotSerTrack == INLotSerTrack.SerialNumbered) && row.ByOne != true && !isTransferReceipt);
                PXUIFieldAttribute.SetEnabled<POReceiptLineS.receiptQty>(sender, row, !(requestLotSer && lotclass.LotSerTrack == INLotSerTrack.SerialNumbered) && row.ByOne != true && !isTransferReceipt);
                PXUIFieldAttribute.SetEnabled<POReceiptLineS.inventoryID>(sender, row, (string.IsNullOrEmpty(row.BarCode) || row.InventoryID == null));
                PXUIFieldAttribute.SetEnabled<POReceiptLineS.unitCost>(sender, row, row.POAccrualType != POAccrualType.Order);

                PXUIFieldAttribute.SetVisible<POReceiptLineS.vendorID>(sender, row, !isTransferReceipt);
                PXUIFieldAttribute.SetVisible<POReceiptLineS.vendorLocationID>(sender, row, !isTransferReceipt);
                PXUIFieldAttribute.SetVisible<POReceiptLineS.pOType>(sender, row, !isTransferReceipt);
                PXUIFieldAttribute.SetVisible<POReceiptLineS.pONbr>(sender, row, !isTransferReceipt);
                PXUIFieldAttribute.SetVisible<POReceiptLineS.pOLineNbr>(sender, row, !isTransferReceipt);
                PXUIFieldAttribute.SetVisible<POReceiptLineS.unitCost>(sender, row, !isTransferReceipt);
                PXUIFieldAttribute.SetVisible<POReceiptLineS.shipFromSiteID>(sender, row, isTransferReceipt);
                PXUIFieldAttribute.SetVisible<POReceiptLineS.sOOrderType>(sender, row, isTransferReceipt);
                PXUIFieldAttribute.SetVisible<POReceiptLineS.sOOrderNbr>(sender, row, isTransferReceipt);
                PXUIFieldAttribute.SetVisible<POReceiptLineS.sOOrderLineNbr>(sender, row, isTransferReceipt);
                PXUIFieldAttribute.SetVisible<POReceiptLineS.sOShipmentNbr>(sender, row, isTransferReceipt);

                //addPOReceiptLine.SetEnabled(!isTransferReceipt || row.SOOrderLineNbr != null);
                //addPOReceiptLine2.SetEnabled(!isTransferReceipt || row.SOOrderLineNbr != null);

                PXSetPropertyException warning = null;
                if (row.BarCode != null && row.InventoryID != null && row.SubItemID != null)
                {
                    INItemXRef xref =
                    PXSelect<INItemXRef,
                        Where<INItemXRef.inventoryID, Equal<Current<POReceiptLineS.inventoryID>>,
                            And<INItemXRef.alternateID, Equal<Current<POReceiptLineS.barCode>>,
                                And<INItemXRef.alternateType, In3<INAlternateType.barcode, INAlternateType.gIN>>>>>
                        .SelectSingleBound(this, new object[] { e.Row });
                    if (xref == null)
                        warning = new PXSetPropertyException(Messages.BarCodeAddToItem, PXErrorLevel.Warning);
                }
                sender.RaiseExceptionHandling<POReceiptLineS.barCode>(e.Row, ((POReceiptLineS)e.Row).BarCode, warning);
            }
        }
        protected virtual void INItemXRef_BAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            e.Cancel = true;
        }
        #endregion

		#region SOLineSplit Events
		[PXRemoveBaseAttribute(typeof(PXDBDefaultAttribute))]
		[PXMergeAttributes, PXDefault]
		protected virtual void SOLineSplit_OrderNbr_CacheAttached(PXCache sender)
		{
		}

		[PXDBDate()]
		[PXDefault()]
		protected virtual void SOLineSplit_OrderDate_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt()]
		protected virtual void SOLineSplit_SiteID_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt()]
		protected virtual void SOLineSplit_LocationID_CacheAttached(PXCache sender)
		{
		}
		#endregion

		public override IEnumerable ExecuteSelect(string viewName, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
		{
			if (viewName == nameof(poLinesSelection) && (maximumRows == 0 || maximumRows > 201))
			{
				return base.ExecuteSelect(viewName, parameters, searches, sortcolumns, descendings, filters, ref startRow, 201, ref totalRows);
			}
			else
			{
				return base.ExecuteSelect(viewName, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
			}
		}

		public override int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
		{
			if (viewName == nameof(openTransfers))
				using (new PXReadBranchRestrictedScope())
					return base.ExecuteUpdate(viewName, keys, values, parameters);
			else
				return base.ExecuteUpdate(viewName, keys, values, parameters);
		}

        public override void Clear(PXClearOption option)
        {
			base.Clear(option);

            if (IsImport && !IsMobile)
            {
				filter.Cache.Clear();
				filter.Cache.Insert(new POOrderFilter());
			}
        }

		#region SOOrderShipment
		/// <summary><see cref="SOOrderShipment"/> Selected</summary>
		protected virtual void SOOrderShipment_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = (SOOrderShipment)e.Row;
			var orderType = SOOrderType.PK.Find(this, row.OrderType);
			if (orderType?.Behavior == SOBehavior.TR || orderType?.Behavior == SOBehavior.SO && orderType?.INDocType == INDocType.Transfer)
			{
				var view = new PXSelectJoin<POReceipt,
					InnerJoin<POReceiptLine,
						On<POReceiptLine.FK.Receipt>>,
					Where<POReceiptLine.sOOrderType, Equal<Current<SOOrderShipment.orderType>>,
					And<POReceiptLine.sOOrderNbr, Equal<Current<SOOrderShipment.orderNbr>>,
					And<POReceiptLine.sOShipmentNbr, Equal<Current<SOOrderShipment.shipmentNbr>>,
					And<Where<POReceiptLine.released, NotEqual<True>, Or<POReceiptLine.iNReleased, NotEqual<True>
						>>>>>>>(this);
				if (Document.Cache.GetStatus(Document.Current).IsIn(PXEntryStatus.Inserted, PXEntryStatus.InsertedDeleted) == false)
					view.WhereAnd<
						Where<POReceipt.receiptType, Equal<Current<POReceipt.receiptType>>,
						And<POReceipt.receiptNbr, NotEqual<Current<POReceipt.receiptNbr>>>>>();
				POReceipt transferReceipt = view.SelectSingle();
				PXUIFieldAttribute.SetEnabled<SOOrderShipment.selected>(sender, row, transferReceipt == null);
				PXSetPropertyException propertyException = transferReceipt == null
					? null
					: new PXSetPropertyException(Messages.TransferUsedInUnreleasedReceipt, PXErrorLevel.RowWarning, transferReceipt.ReceiptNbr);
				sender.RaiseExceptionHandling<SOOrderShipment.selected>(row, null, propertyException);
			}
		}
		#endregion

		public override bool IsDirty => (Document.Current?.Released != true || IsContractBasedAPI) && base.IsDirty;

		public override void Persist()
        {
			if (Document.Current != null && Document.Current.Hold == false)
			{
				foreach (POReceiptLine poReceiptLine in transactions.Select())
				{
					if (poReceiptLine.ReceiptQty == 0m && Document.Current.ReceiptType == POReceiptType.TransferReceipt)
					{
						transactions.Delete(poReceiptLine);
					}
				}

				ValidateDuplicateSerialsOnDropship();
			}

			SyncUnassigned();

			base.Persist();
			this.ClearSelectionCache<POLineS.selected>(this.poLinesSelection.Cache);
			this.openOrders.Cache.Clear();
		}

		protected virtual void ValidateDuplicateSerialsOnDropship()
		{
			if (Document.Current != null && Document.Current.Hold != true)
			{
				HashSet<string> uniqueSerials = new HashSet<string>();

				bool duplicateFound = false;
				foreach (POReceiptLineSplit split in splits.Select())
				{
					if (split.LineType == POLineType.GoodsForDropShip && LineSplittingExt.IsTrackSerial(split))
					{
						if (!string.IsNullOrEmpty(split.AssignedNbr) && INLotSerialNbrAttribute.StringsEqual(split.AssignedNbr, split.LotSerialNbr))
						{
							continue;
						}

						string key = string.Format("{0}.{1}", split.InventoryID, split.LotSerialNbr);

						if (uniqueSerials.Contains(key))
						{
							duplicateFound = true;

							POReceiptLine detail = (POReceiptLine) PXParentAttribute.SelectParent(splits.Cache, split, typeof (POReceiptLine));

							if (detail != null)
							{
								transactions.Cache.RaiseExceptionHandling<POReceiptLine.inventoryID>(detail, null, new PXSetPropertyException(Messages.ContainsDuplicateSerialNumbers, PXErrorLevel.RowError));
							}
						}
						else
						{
							uniqueSerials.Add(key);
						}
					}
				}

				if (duplicateFound)
				{
					throw new PXException(Messages.DuplicateSerialNumbers);
				}
			}
		}

		public virtual POReceipt CreateEmptyReceiptFrom(POOrder order)
		{
			POReceipt receipt = Document.Insert();

			receipt.ReceiptType = POReceiptType.POReceipt;
			receipt.BranchID = order.BranchID;
			receipt.VendorID = order.VendorID;
			receipt.VendorLocationID = order.VendorLocationID;
			receipt.ProjectID = order.ProjectID;

			receipt = Document.Update(receipt);

			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				CurrencyInfo orderCuryInfo = MultiCurrencyExt.GetCurrencyInfo(order.CuryInfoID);
				CurrencyInfo receiptCuryInfo = MultiCurrencyExt.GetCurrencyInfo(receipt.CuryInfoID);

				receiptCuryInfo.CuryID = orderCuryInfo.CuryID;
				receiptCuryInfo.CuryRateTypeID = orderCuryInfo.CuryRateTypeID;
				receiptCuryInfo = MultiCurrencyExt.currencyinfo.Update(receiptCuryInfo);

				receipt.CuryID = receiptCuryInfo.CuryID;
				receipt = Document.Update(receipt);
			}

			return receipt;
		}

		public virtual POReceipt CreateReceiptFrom(POOrder order, bool redirect = false)
		{
			ValidatePOOrder(order);

			CreateEmptyReceiptFrom(order);

			AddPurchaseOrder(order);
			if (transactions.Cache.IsDirty)
			{
				if (redirect)
					throw new PXRedirectRequiredException(this, Messages.POReceiptRedirection);
				else
					return Document.Current;
			}

			throw new PXException(Messages.POReceiptFromOrderCreation_NoApplicableLinesFound);
		}

		#region Internal Utility functions
		public int? GetReasonCodeSubID(ReasonCode reasoncode, InventoryItem item, INSite site, INPostClass postclass)
        {
            int? reasoncode_SubID = (int?)Caches[typeof(ReasonCode)].GetValue<ReasonCode.subID>(reasoncode);
            int? item_SubID = (int?)Caches[typeof(InventoryItem)].GetValue<InventoryItem.reasonCodeSubID>(item);
            int? site_SubID = (int?)Caches[typeof(INSite)].GetValue<INSite.reasonCodeSubID>(site);
            int? class_SubID = (int?)Caches[typeof(INPostClass)].GetValue<INPostClass.reasonCodeSubID>(postclass);

            object value = IN.ReasonCodeSubAccountMaskAttribute.MakeSub<ReasonCode.subMask>(this, reasoncode.SubMask,
                new object[] { reasoncode_SubID, item_SubID, site_SubID, class_SubID },
                new Type[] { typeof(ReasonCode.subID), typeof(InventoryItem.reasonCodeSubID), typeof(INSite.reasonCodeSubID), typeof(INPostClass.reasonCodeSubID) });

            Caches[typeof(ReasonCode)].RaiseFieldUpdating<ReasonCode.subID>(reasoncode, ref value);
            return (int?)value;
        }

		protected virtual decimal? DefaultUnitCost(PXCache sender, POReceiptLine row, bool alwaysFromBaseCury)
		{
			POReceipt doc = this.Document.Current;

			if (doc?.VendorID == null || (row?.InventoryID == null || string.IsNullOrEmpty(row.UOM)) && row?.LineType != POLineType.Freight)
				return null;

			if (row.LineType == POLineType.Freight)
				return 0m;

			if ((row.ManualPrice == true && row.UnitCost != null) || row.PONbr != null)
				return (alwaysFromBaseCury ? row.UnitCost : row.CuryUnitCost) ?? 0m;

			CurrencyInfo curyInfo = MultiCurrencyExt.GetDefaultCurrencyInfo();
			Decimal? vendorUnitCost = APVendorPriceMaint.CalculateUnitCost(
				sender,
				row.VendorID,
				doc.VendorLocationID,
				row.InventoryID,
				row.SiteID,
				curyInfo.GetCM(),
				row.UOM,
				row.ReceiptQty,
				(DateTime)doc.ReceiptDate,
				row.UnitCost,
				alwaysFromBaseCurrency: alwaysFromBaseCury);

			if (vendorUnitCost == null && row.SubItemID != null)
			{
				vendorUnitCost = POItemCostManager.Fetch<POReceiptLine.inventoryID, POReceiptLine.curyInfoID>(
					sender.Graph, 
					row,
					doc.VendorID, 
					doc.VendorLocationID, 
					doc.ReceiptDate,
					doc.CuryID, 
					row.InventoryID, 
					row.SubItemID, 
					row.SiteID, 
					row.UOM);
			}

			APVendorPriceMaint.CheckNewUnitCost<POReceiptLine, POReceiptLine.curyUnitCost>(sender, row, vendorUnitCost);

			return vendorUnitCost;
		}

        protected virtual void ParentFieldUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            if (!sender.ObjectsEqual<POReceipt.receiptDate, POReceipt.curyID>(e.Row, e.OldRow))
            {
                foreach (POReceiptLine tran in this.transactions.Select())
                {
                    transactions.Cache.MarkUpdated(tran, assertError: true);
                }
            }
        }

		public virtual void AddPurchaseOrder(POOrder aOrder) => AddPurchaseOrder(aOrder, inventoryID: null, subItemID: null);

		public virtual void AddPurchaseOrder(POOrder aOrder, int? inventoryID, int? subItemID = null)
		{
			POOrderFilter orderFilter = new POOrderFilter();
			orderFilter.OrderType = aOrder.OrderType;
			orderFilter.OrderNbr = aOrder.OrderNbr;
			if (inventoryID != null)
				orderFilter.InventoryID = inventoryID;
			if (subItemID != null)
				orderFilter.SubItemID = subItemID;

			bool failedToAddLine = false;
			bool lineAddedSuccessfully = false;
			Exception orderLevelException = null;
			try
			{
				PrefetchDropShipLinks(aOrder);

				_skipUIUpdate = true;
				
				foreach (PXResult<POLineS, POOrder> res in poLinesSelection.View.SelectMultiBound(new[] { orderFilter }))
				{
					try
					{
						AddPOLine(res, false);
						lineAddedSuccessfully = true;
					}
					catch (PXException ex)
					{
						if (ex is FailedToAddPOOrderException)
							orderLevelException = ex;

						PXTrace.WriteError(ex);
						failedToAddLine = true;
					}
				}
				if (lineAddedSuccessfully) AddPOOrderReceipt(aOrder.OrderType, aOrder.OrderNbr);
			}
			finally
			{
				_skipUIUpdate = false;
			}

			if (failedToAddLine)
			{
				throw orderLevelException != null ? orderLevelException : new PXException(Messages.FailedToAddLine);
			}
		}

        public virtual void AddTransferDoc(SOOrderShipment aOrder)
        {
            try
            {
                this._skipUIUpdate = true;
				var tranToTransits = new Dictionary<INTran, List<PXResult<INTransitLine, INLotSerialStatusInTransit, INLocationStatusInTransit, InventoryItem, INTransitLineStatus>>>(intranSelection.Cache.GetComparer());
                //cannot select intran where already exists unreleased receipt poreceiptlines or intrans.
				foreach (PXResult<INTran, INTransitLine, INTransitLineStatus, INLotSerialStatusInTransit, INLocationStatusInTransit, InventoryItem, POReceiptLine> iTran in 
                    PXSelectJoin<INTran, 
					InnerJoin<INTransitLine,
						On<INTransitLine.transferNbr, Equal<INTran.refNbr>,
						And<INTransitLine.transferLineNbr, Equal<INTran.lineNbr>>>,
					InnerJoin<INTransitLineStatus,
						On<INTransitLineStatus.transferNbr, Equal<INTransitLine.transferNbr>,
						And<INTransitLineStatus.transferLineNbr, Equal<INTransitLine.transferLineNbr>>>,
					LeftJoin<INLotSerialStatusInTransit, On<INLotSerialStatusInTransit.locationID, Equal<INTransitLine.costSiteID>>,
					InnerJoin<INLocationStatusInTransit, On<INLocationStatusInTransit.locationID, Equal<INTransitLine.costSiteID>>,
					InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INLocationStatusInTransit.inventoryID>>, 
					LeftJoin<POReceiptLine,
						On2<POReceiptLine.FK.OriginalINTran,
						And<Where<POReceiptLine.released, NotEqual<True>,
							Or<POReceiptLine.iNReleased, NotEqual<True>>>>>>>>>>>,
					Where<POReceiptLine.receiptNbr, IsNull,
                    And<INTransitLineStatus.qtyOnHand, Greater<Zero>,
                    And<INTran.docType, Equal<Required<SOOrderShipment.invtDocType>>, 
                    And<INTran.refNbr, Equal<Required<SOOrderShipment.invtRefNbr>>, 
                    And<INTran.sOOrderType, Equal<Required<SOOrderShipment.orderType>>, 
                    And<INTran.sOOrderNbr, Equal<Required<SOOrderShipment.orderNbr>>, 
                    And<INTran.sOShipmentType, Equal<Required<SOOrderShipment.shipmentType>>, 
						And<INTran.sOShipmentNbr, Equal<Required<SOOrderShipment.shipmentNbr>>>>>>>>>>,
					OrderBy<Asc<INTransitLine.transferNbr, Asc<INTransitLine.transferLineNbr>>>>
                    .Select(this, aOrder.InvtDocType, aOrder.InvtRefNbr, aOrder.OrderType, aOrder.OrderNbr, aOrder.ShipmentType, aOrder.ShipmentNbr))
                {
					tranToTransits
						.GetOrAdd(iTran, () => new List<PXResult<INTransitLine, INLotSerialStatusInTransit, INLocationStatusInTransit, InventoryItem, INTransitLineStatus>>())
						.Add(new PXResult<INTransitLine, INLotSerialStatusInTransit, INLocationStatusInTransit, InventoryItem, INTransitLineStatus>(iTran, iTran, iTran, iTran, iTran));
                }

				foreach (var tranToTransit in tranToTransits.Where(r => r.Value.Any()))
				{
					INTransitLineStatus lineStatus = tranToTransit.Value.First();

					/// <summary><see cref="LSPOReceiptLine.OpenOrderQty_FieldSelecting"/> OpenOrderQty optimization</summary>
					PXSelect<INTransitLineStatus,
						Where<INTransitLineStatus.transferNbr, Equal<Current<POReceiptLine.origRefNbr>>,
							And<INTransitLineStatus.transferLineNbr, Equal<Current<POReceiptLine.origLineNbr>>>>>
						.StoreResult(this, lineStatus);

					/// <summary><see cref="LSPOReceiptLine.OrigOrderQty_FieldSelecting"/> OrigOrderQty optimization</summary>
					PXSelect<INTran,
						Where<INTran.tranType, Equal<INTranType.transfer>,
							And<INTran.refNbr, Equal<Current<POReceiptLine.origRefNbr>>,
							And<INTran.lineNbr, Equal<Current<POReceiptLine.origLineNbr>>,
							And<INTran.docType, Equal<Current<POReceiptLine.origDocType>>>>>>>
						.StoreResult(this, tranToTransit.Key);

					AddTransferLine(tranToTransit.Key, tranToTransit.Value);
				}
            }
            finally
            {
                this._skipUIUpdate = false;
            }
        }

		public virtual POReceiptLine AddPOLine(POLine aLine, bool isLSEntryBlocked = false)
		{
			POReceipt doc = this.Document.Current;
			if (doc != null && doc.ReceiptType != POReceiptType.TransferReceipt)
			{
				ValidatePOLine(aLine, doc);

				decimal baseQtyDelta = Decimal.Zero;
				Dictionary<int, POReceiptLine> currentRows = new Dictionary<int, POReceiptLine>();
				foreach (POReceiptLine iLine in PXSelect<POReceiptLine,
															Where<POReceiptLine.receiptType, Equal<Required<POReceiptLine.receiptType>>,
																And<POReceiptLine.receiptNbr, Equal<Required<POReceiptLine.receiptNbr>>,
																And<POReceiptLine.pOType, Equal<Required<POReceiptLine.pOType>>,
																And<POReceiptLine.pONbr, Equal<Required<POReceiptLine.pONbr>>,
																And<POReceiptLine.pOLineNbr, Equal<Required<POReceiptLine.pOLineNbr>>>>>>>>.Select(this, doc.ReceiptType, doc.ReceiptNbr, aLine.OrderType, aLine.OrderNbr, aLine.LineNbr))
				{
					if (aLine.LineType.IsIn(POLineType.GoodsForSalesOrder, POLineType.NonStockForSalesOrder, POLineType.GoodsForServiceOrder,
						POLineType.NonStockForServiceOrder, POLineType.GoodsForManufacturing, POLineType.NonStockForManufacturing))
					{
						// We can't add POLine linked to Sales Order second time to the same receipt.
						return null;
					}

					POReceiptLine original = POReceiptLine.PK.Find(this, iLine);

					baseQtyDelta += (iLine.BaseReceiptQty ?? Decimal.Zero) - (original != null ? ((decimal)original.BaseReceiptQty) : Decimal.Zero);
					currentRows[iLine.LineNbr.Value] = iLine;
				}
				//Find deleted
				foreach (POReceiptLine iOrig in PXSelectReadonly<POReceiptLine,
															Where<POReceiptLine.receiptType, Equal<Required<POReceiptLine.receiptType>>,
																And<POReceiptLine.receiptNbr, Equal<Required<POReceiptLine.receiptNbr>>,
																And<POReceiptLine.pOType, Equal<Required<POReceiptLine.pOType>>,
																And<POReceiptLine.pONbr, Equal<Required<POReceiptLine.pONbr>>,
																And<POReceiptLine.pOLineNbr, Equal<Required<POReceiptLine.pOLineNbr>>>>>>>>.Select(this, doc.ReceiptType, doc.ReceiptNbr, aLine.OrderType, aLine.OrderNbr, aLine.LineNbr))
				{
					if (!currentRows.ContainsKey(iOrig.LineNbr.Value))
					{
						baseQtyDelta -= (decimal)iOrig.BaseReceiptQty;
					}
				}
				decimal qtyDelta = baseQtyDelta;
				POReceiptLine line = new POReceiptLine
				{
					ReceiptType = doc.ReceiptType,
					ReceiptNbr = doc.ReceiptNbr,
					CuryInfoID = doc.CuryInfoID,
				};
				if (baseQtyDelta != Decimal.Zero)
				{
					if (aLine.InventoryID != null && !String.IsNullOrEmpty(aLine.UOM))
						qtyDelta = INUnitAttribute.ConvertFromBase(this.transactions.Cache, aLine.InventoryID, aLine.UOM, baseQtyDelta, INPrecision.QUANTITY);
				}
				Copy(line, aLine, qtyDelta, baseQtyDelta);
				_forceAccrualAcctDefaulting = (POLineType.UsePOAccrual(aLine.LineType) && aLine.POAccrualAcctID == null && aLine.POAccrualSubID == null);
				if (line.ReceiptQty >= Decimal.Zero)
				{
					try
					{
						line.IsLSEntryBlocked = isLSEntryBlocked;//split line is not created if IsLSEntryBlocked = TRUE
						line = InsertReceiptLine(line, doc, aLine);
					}
					finally
					{
						if (line != null)
						{
							line.IsLSEntryBlocked = false;
						}
					}
				}
				else
				{
					line.ReceiptQty = line.BaseReceiptQty = Decimal.Zero;
					line = InsertReceiptLine(line, doc, aLine);
					transactions.Cache.RaiseExceptionHandling<POReceiptLine.receiptQty>(line, line.ReceiptQty,
						new PXSetPropertyException(Messages.QuantityReceivedForOrderLineExceedsOrdersQuatity, PXErrorLevel.Warning));
				}
				_forceAccrualAcctDefaulting = false;

				if (posetup.Current.CopyLineNotesToReceipt == true || posetup.Current.CopyLineFilesToReceipt == true)
				{
					PXNoteAttribute.CopyNoteAndFiles(poline.Cache, aLine, transactions.Cache, line,
						posetup.Current.CopyLineNotesToReceipt, posetup.Current.CopyLineFilesToReceipt);
				}

				return line;
			}
			return null;
		}

        public virtual POReceiptLine AddTransferLine(INTran aLine)
		{
			return AddTransferLine(aLine, AddTransferLineSelectTransitLines(aLine));
		}

		// TODO: use local function in AddTransferLine(INTran) instead
		private IEnumerable<PXResult<INTransitLine, INLotSerialStatusInTransit, INLocationStatusInTransit, InventoryItem>> AddTransferLineSelectTransitLines(INTran aLine)
			{
			foreach (PXResult<INTransitLine, INLotSerialStatusInTransit, INLocationStatusInTransit, InventoryItem> res in
				PXSelectJoin<INTransitLine,
				LeftJoin<INLotSerialStatusInTransit, On<INLotSerialStatusInTransit.locationID, Equal<INTransitLine.costSiteID>>,
				InnerJoin<INLocationStatusInTransit, On<INLocationStatusInTransit.locationID, Equal<INTransitLine.costSiteID>>,
				InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INLocationStatusInTransit.inventoryID>>
				>>>,
				Where<INTransitLine.transferNbr, Equal<Required<INTransitLine.transferNbr>>,
					And<INTransitLine.transferLineNbr, Equal<Required<INTransitLine.transferLineNbr>>>>,
				OrderBy<Asc<INTransitLine.transferNbr, Asc<INTransitLine.transferLineNbr>>>>
				.Select(this, aLine.RefNbr, aLine.LineNbr))
				{
				yield return res;
				}
			}

		public virtual POReceiptLine AddTransferLine(INTran aLine, IEnumerable<PXResult<INTransitLine, INLotSerialStatusInTransit, INLocationStatusInTransit, InventoryItem>> transitLines)
				{
			POReceipt doc = this.Document.Current;
            if (doc == null || doc.ReceiptType != POReceiptType.TransferReceipt)
                return null;
			foreach (POReceiptLine r in 
				PXSelect<POReceiptLine,
															Where<POReceiptLine.receiptType, Equal<Required<POReceipt.receiptType>>,
																And<POReceiptLine.receiptNbr, Equal<Required<POReceipt.receiptNbr>>,
                                                                And<POReceiptLine.origDocType, Equal<Required<POReceiptLine.origDocType>>,
                                                                And<POReceiptLine.origRefNbr, Equal<Required<POReceiptLine.origRefNbr>>,
                                                                And<POReceiptLine.origLineNbr, Equal<Required<POReceiptLine.origLineNbr>>>>>>>>
                                                                .Select(this, doc.ReceiptType, doc.ReceiptNbr, aLine.DocType, aLine.RefNbr, aLine.LineNbr))
					{
				return null;
			}

			POReceiptLine newtran = null;
            int? prev_linenbr = null;
			INLocationStatusInTransit prev_stat = null;
            decimal newtranqty = 0m;
            decimal newtrancost = 0m;
            string transfernbr = aLine.RefNbr;
            ParseSubItemSegKeys();

			foreach (PXResult<INTransitLine, INLotSerialStatusInTransit, INLocationStatusInTransit, InventoryItem> res in transitLines)
			{
                INTransitLine transitline = res;
				INLotSerialStatusInTransit lotstat = res;
				INLocationStatusInTransit stat = res;
                InventoryItem item = res;

                if (stat.QtyOnHand == 0m || (lotstat != null && lotstat.QtyOnHand == 0m))
                    continue;

                if (prev_linenbr != transitline.TransferLineNbr)
				{
                    UpdateTranCostQty(newtran, newtranqty, newtrancost);
                    newtranqty = 0m;

					newtran = PXCache<POReceiptLine>.CreateCopy(this.transactions.Insert(new POReceiptLine()));
                    Copy(newtran, aLine);

					newtran.InvtMult = 1;
                    newtran.OrigPlanType = transitline.IsFixedInTransit == true ? INPlanConstants.Plan44 : INPlanConstants.Plan42;
					newtran.OrigIsFixedInTransit = transitline.IsFixedInTransit;
					newtran.OrigIsLotSerial = transitline.IsLotSerial;
					newtran.OrigToLocationID = transitline.ToLocationID;
					newtran.OrigNoteID = transitline.NoteID;

					splits.Current = null;

                    newtran.ManualPrice = true;
					newtran = transactions.Update(newtran);

					if (splits.Current != null)
					{
						splits.Delete(splits.Current);
					}

                    prev_linenbr = transitline.TransferLineNbr;
				}

                if (this.Caches[typeof(INLocationStatusInTransit)].ObjectsEqual(prev_stat, stat) == false)
                {
                    newtranqty += stat.QtyOnHand.Value;
                    prev_stat = stat;
                }

                decimal newsplitqty;
                POReceiptLineSplit newsplit;
                if (lotstat.QtyOnHand == null)
				{
                    newsplit = new POReceiptLineSplit();
                    newsplit.InventoryID = stat.InventoryID;
                    newsplit.IsStockItem = true;
                    newsplit.SubItemID = stat.SubItemID;
                    newsplit.LotSerialNbr = null;
                    newsplitqty = stat.QtyOnHand.Value;
                }
                else
                {
                    newsplit = new POReceiptLineSplit();
                    newsplit.InventoryID = lotstat.InventoryID;
                    newsplit.IsStockItem = true;
                    newsplit.SubItemID = lotstat.SubItemID;
                    newsplit.LotSerialNbr = lotstat.LotSerialNbr;
                    newsplitqty = lotstat.QtyOnHand.Value;
                }

                int? costsubitemid = GetCostSubItemID(newsplit, item);
					newsplit.ReceiptType = newtran.ReceiptType;
					newsplit.ReceiptNbr = newtran.ReceiptNbr;
					newsplit.LineNbr = newtran.LineNbr;

					newsplit = PXCache<POReceiptLineSplit>.CreateCopy(splits.Insert(newsplit));
					newsplit.InvtMult = (short)1;
					newsplit.SiteID = transitline.ToSiteID;

                newsplit.MaxTransferBaseQty = newsplitqty;
                newsplit.BaseQty = newsplitqty;
                newsplit.Qty = newsplitqty;
                newtrancost += newsplit.BaseQty.Value * GetTransitSplitUnitCost(newtran, newsplit, item, costsubitemid, transfernbr);
					newsplit = splits.Update(newsplit);
				}
            UpdateTranCostQty(newtran, newtranqty, newtrancost);

            return newtran;
		}


        public virtual void UpdateTranCostQty(POReceiptLine newtran, decimal newtranqty, decimal newtrancost)
        {
            if (newtran != null)
            {
                newtran.BaseQty = newtranqty;
                newtran.MaxTransferBaseQty = newtranqty;
                newtran.Qty = INUnitAttribute.ConvertFromBase(transactions.Cache, newtran.InventoryID, newtran.UOM, newtran.BaseQty.Value, INPrecision.QUANTITY);
                newtran.CuryUnitCost = PXDBPriceCostAttribute.Round(newtrancost / newtran.Qty.Value);

				string BaseCuryID = new PXSetup<Company>(this).Current.BaseCuryID;

                transactions.Update(newtran);

            }
        }

		public virtual POReceiptLine AddReturnLine(IPOReturnLineSource origLine)
		{
			if (origLine.ReceiptQty - origLine.ReturnedQty <= 0)
				return null;

			POReceiptLine existingNotReleased = PXSelect<POReceiptLine,
				Where<
					POReceiptLine.origReceiptType, Equal<Required<POReceiptLine.origReceiptType>>,
					And<POReceiptLine.origReceiptNbr, Equal<Required<POReceiptLine.origReceiptNbr>>,
					And<POReceiptLine.origReceiptLineNbr, Equal<Required<POReceiptLine.origReceiptLineNbr>>,
					And<POReceiptLine.released, Equal<False>>>>>>
				.SelectWindowed(this, 0, 1, origLine.ReceiptType, origLine.ReceiptNbr, origLine.LineNbr);

			if (existingNotReleased != null)
			{
				throw new PXAlreadyCreatedException(Messages.UnreleasedReturnExistsForPOReceipt, existingNotReleased.ReceiptNbr);
			}

			if (Document.Current?.CuryID != Accessinfo.BaseCuryID)
			{
				CurrencyInfo returnCuryInfo = GetCurrencyInfo(Document.Current?.CuryInfoID);
				CurrencyInfo receiptCuryInfo = GetCurrencyInfo(origLine.CuryInfoID);
				if (Document.Current?.ReturnInventoryCostMode == ReturnCostMode.OriginalCost)
				{
					PXResultset<POReceiptLine> returnLines = transactions.Select();
					if (!returnLines.Any())
					{
						CopyReceiptCurrencyInfoToReturn(receiptCuryInfo, returnCuryInfo, true);
					}
					else
					{
						if (!IsSameCurrencyInfo(returnCuryInfo, receiptCuryInfo))
						{
							throw new PXException(Messages.ReceiptCannotBeAddedToReturnByOrigCostDifferentRates, origLine.ReceiptNbr);
						}
					}
				}
				else if (returnCuryInfo.CuryID != receiptCuryInfo.CuryID)
				{
					throw new PXException(Messages.ReceiptCannotBeAddedToReturnDifferentCurrencies, origLine.ReceiptNbr);
				}
			}

			var newLine = transactions.Insert();
			CopyFromOrigReceiptLine(newLine, origLine, preserveLineType: POLineType.IsProjectDropShip(origLine.LineType), returnOrigCost: Document.Current?.ReturnInventoryCostMode == ReturnCostMode.OriginalCost);

			InventoryItem item = InventoryItem.PK.Find(this, origLine.InventoryID);
			bool isConverted = item?.IsConverted == true && origLine.IsStockItem != null && origLine.IsStockItem != item.StkItem;
			if (isConverted)
				transactions.Cache.SetDefaultExt<POReceiptLine.lineType>(newLine);

			bool isLotSerTrack = IsLotSerTracked(origLine);
			if (!isLotSerTrack)
			{
				newLine = transactions.Update(newLine);
			}
			else
			{
				using (LineSplittingExt.SuppressedModeScope(true))
				{
					newLine.UnassignedQty = newLine.BaseReceiptQty;
					newLine = transactions.Update(newLine);
				}

				if (!string.IsNullOrEmpty(origLine.LotSerialNbr))
				{
					newLine.LotSerialNbr = origLine.LotSerialNbr;
					newLine.ExpireDate = origLine.ExpireDate;
					try
					{
					newLine = transactions.Update(newLine);
				}
					catch (Exception ex)
					{
						transactions.Cache.RaiseExceptionHandling<POReceiptLine.lotSerialNbr>(newLine, null,
						new PXSetPropertyException(ex.Message, PXErrorLevel.Error));
					}
				}
			}

			if (isConverted)
				newLine.IsStockItem = !item.StkItem;

			if (!string.IsNullOrEmpty(newLine.POType) && !string.IsNullOrEmpty(newLine.PONbr))
			{
				AddPOOrderReceipt(newLine.POType, newLine.PONbr);
			}

			return newLine;
		}

		public virtual bool IsLotSerTracked(IPOReturnLineSource line)
		{
			if (!string.IsNullOrEmpty(line.LotSerialNbr))
				return true;

			var item = InventoryItem.PK.Find(this, line.InventoryID);
			INLotSerClass lotclass = INLotSerClass.PK.Find(this, item?.LotSerClassID);

			if (Document.Current?.ReceiptType == POReceiptType.POReturn && lotclass?.LotSerAssign == INLotSerAssign.WhenUsed)
				return false;

			return !(lotclass?.LotSerTrack).IsIn(null, INLotSerTrack.NotNumbered);
		}

		public virtual void CopyFromOrigReceiptLine(POReceiptLine destLine, IPOReturnLineSource srcLine, bool preserveLineType, bool? returnOrigCost)
		{
			destLine.OrigReceiptType = srcLine.ReceiptType;
			destLine.OrigReceiptNbr = srcLine.ReceiptNbr;
			destLine.OrigReceiptLineNbr = srcLine.LineNbr;
			destLine.POType = srcLine.POType;
			destLine.PONbr = srcLine.PONbr;
			destLine.POLineNbr = srcLine.POLineNbr;
			destLine.InventoryID = srcLine.InventoryID;
			destLine.AccrueCost = srcLine.AccrueCost;
			destLine.DropshipExpenseRecording = srcLine.DropshipExpenseRecording;
			destLine.SubItemID = srcLine.SubItemID;
			destLine.SiteID = srcLine.SiteID;
			destLine.LocationID = srcLine.LocationID;
			if (preserveLineType)
			{
				destLine.LineType = srcLine.LineType;
			}
			else
			{
				destLine.LineType = POLineType.IsService(srcLine.LineType) ? POLineType.Service
					: POLineType.IsStock(srcLine.LineType) ? POLineType.GoodsForInventory
					: POLineType.IsNonStock(srcLine.LineType) ? POLineType.NonStock
					: POLineType.Freight;
			}
			destLine.OrigReceiptLineType = srcLine.LineType;
			destLine.UOM = srcLine.UOM;
			destLine.ReceiptQty = srcLine.ReceiptQty - srcLine.ReturnedQty;
			destLine.BaseReceiptQty = srcLine.BaseReceiptQty - srcLine.BaseReturnedQty;
			destLine.BaseOrigQty = srcLine.BaseReceiptQty - srcLine.BaseReturnedQty;
			destLine.ExpenseAcctID = srcLine.ExpenseAcctID;
			destLine.ExpenseSubID = srcLine.ExpenseSubID;
			destLine.POAccrualAcctID = srcLine.POAccrualAcctID;
			destLine.POAccrualSubID = srcLine.POAccrualSubID;
			destLine.POAccrualType = POAccrualType.Receipt;
			destLine.TranDesc = srcLine.TranDesc;
			destLine.AllowComplete = false;
			destLine.AllowOpen = false;
			destLine.CostCodeID = srcLine.CostCodeID;
			destLine.ProjectID = srcLine.ProjectID;
			destLine.TaskID = srcLine.TaskID;
			destLine.ManualPrice = srcLine.ManualPrice;
			destLine.AllowEditUnitCost = returnOrigCost != true;
			destLine.DiscPct = srcLine.DiscPct;

			destLine.IsSpecialOrder = srcLine.IsSpecialOrder;
			destLine.CostCenterID = srcLine.CostCenterID;

			CurrencyInfo currencyInfo = MultiCurrencyExt.GetCurrencyInfo(destLine.CuryInfoID);

			decimal? srcCuryDiscAmt = srcLine.CuryDiscAmt;
			destLine.CuryDiscAmt = (srcLine.ReturnedQty == 0m) ? srcCuryDiscAmt
				: currencyInfo.RoundCury((srcCuryDiscAmt * (srcLine.ReceiptQty - srcLine.ReturnedQty) / srcLine.ReceiptQty) ?? 0m);

			destLine.CuryUnitCost = srcLine.CuryUnitCost;

			destLine.TranUnitCost = srcLine.TranCostFinal / srcLine.ReceiptQty;
			if (destLine.TranUnitCost != null)
			{
				destLine.CuryTranUnitCost = currencyInfo.CuryConvCuryRaw(destLine.TranUnitCost ?? 0m);
				if (destLine.CuryTranUnitCost != null)
					destLine.CuryTranUnitCost = Math.Round(destLine.CuryTranUnitCost.Value, POReceiptLine.curyTranUnitCost.Precision, MidpointRounding.AwayFromZero);
			}

			decimal? srcCuryExtCost = srcLine.CuryExtCost;
			destLine.CuryExtCost = (srcLine.ReturnedQty == 0m) ? srcCuryExtCost
				: currencyInfo.RoundCury((srcCuryExtCost * (srcLine.ReceiptQty - srcLine.ReturnedQty) / srcLine.ReceiptQty) ?? 0m);

			decimal? srcTranCost = srcLine.TranCostFinal ?? srcLine.TranCost;
			destLine.TranCost = (srcLine.ReturnedQty == 0m) ? srcTranCost
				: currencyInfo.RoundBase((srcTranCost * (srcLine.ReceiptQty - srcLine.ReturnedQty) / srcLine.ReceiptQty) ?? 0m);
			if (destLine.TranCost != null)
			{
				destLine.CuryTranCost = currencyInfo.CuryConvCury(destLine.TranCost ?? 0m);
			}
		}

        List<Segment> _SubItemSeg = null;
        Dictionary<short?, string> _SubItemSegVal = null;

        public virtual void ParseSubItemSegKeys()
        {
            if (_SubItemSeg == null)
            {
                _SubItemSeg = new List<Segment>();

                foreach (Segment seg in PXSelect<Segment, Where<Segment.dimensionID, Equal<IN.SubItemAttribute.dimensionName>>>.Select(this))
                {
                    _SubItemSeg.Add(seg);
                }

                _SubItemSegVal = new Dictionary<short?, string>();

                foreach (SegmentValue val in PXSelectJoin<SegmentValue, InnerJoin<Segment, On<Segment.dimensionID, Equal<SegmentValue.dimensionID>, And<Segment.segmentID, Equal<SegmentValue.segmentID>>>>, Where<SegmentValue.dimensionID, Equal<IN.SubItemAttribute.dimensionName>, And<Segment.isCosted, Equal<boolFalse>, And<SegmentValue.isConsolidatedValue, Equal<boolTrue>>>>>.Select(this))
                {
                    try
                    {
                        _SubItemSegVal.Add((short)val.SegmentID, val.Value);
                    }
                    catch (Exception excep)
                    {
                        throw new PXException(excep, IN.Messages.MultipleAggregateChecksEncountred, val.SegmentID, val.DimensionID);
                    }
                }
            }
        }

        public virtual string MakeCostSubItemCD(string SubItemCD)
        {
            StringBuilder sb = new StringBuilder();

            int offset = 0;

            foreach (Segment seg in _SubItemSeg)
            {
                string segval = SubItemCD.Substring(offset, (int)seg.Length);
                if (seg.IsCosted == true || segval.TrimEnd() == string.Empty)
                {
                    sb.Append(segval);
                }
                else
                {
                    if (_SubItemSegVal.TryGetValue(seg.SegmentID, out segval))
                    {
						sb.Append(segval.PadRight(seg.Length ?? 0));
                    }
                    else
                    {
                        throw new PXException(IN.Messages.SubItemSeg_Missing_ConsolidatedVal);
                    }
                }
                offset += (int)seg.Length;
            }

            return sb.ToString();
        }

        public object GetValueExt<Field>(PXCache cache, object data)
            where Field : class, IBqlField
        {
            object val = cache.GetValueExt<Field>(data);

            if (val is PXFieldState)
            {
                return ((PXFieldState)val).Value;
            }
            else
            {
                return val;
            }
        }

        public virtual int? GetCostSubItemID(POReceiptLineSplit split, InventoryItem item)
        {
            INCostSubItemXRef xref = new INCostSubItemXRef();

            xref.SubItemID = split.SubItemID;
            xref.CostSubItemID = split.SubItemID;

            string SubItemCD = (string)this.GetValueExt<INCostSubItemXRef.costSubItemID>(costsubitemxref.Cache, xref);

            xref.CostSubItemID = null;

            string CostSubItemCD = PXAccess.FeatureInstalled<FeaturesSet.subItem>() ? MakeCostSubItemCD(SubItemCD) : SubItemCD;

            costsubitemxref.Cache.SetValueExt<INCostSubItemXRef.costSubItemID>(xref, CostSubItemCD);
            xref = costsubitemxref.Update(xref);

            if (costsubitemxref.Cache.GetStatus(xref) == PXEntryStatus.Updated)
            {
                costsubitemxref.Cache.SetStatus(xref, PXEntryStatus.Notchanged);
            }

            return xref.CostSubItemID;
        }

        public Int32? INTransitSiteID
        {
            get
            {
                if (insetup.Current.TransitSiteID == null)
                    throw new PXException("Please fill transite site id in inventory preferences.");
                return insetup.Current.TransitSiteID;
            }
        }

        public virtual PXView GetCostStatusCommand(POReceiptLine line, POReceiptLineSplit split, InventoryItem item, string transferNbr, int? costsubitemid, out object[] parameters)
		{
			BqlCommand cmd = null;

			int? costsiteid;
			costsiteid = GetCostStatusCommandCostSiteID(line);

			switch (item.ValMethod)
			{
				case INValMethod.Average:
				case INValMethod.Standard:
				case INValMethod.FIFO:

					cmd = new Select<INCostStatus,
						Where<INCostStatus.inventoryID, Equal<Required<INCostStatus.inventoryID>>,
							And<INCostStatus.costSiteID, Equal<Required<INCostStatus.costSiteID>>,
							And<INCostStatus.costSubItemID, Equal<Required<INCostStatus.costSubItemID>>,
							And<INCostStatus.layerType, Equal<INLayerType.normal>,
							And<INCostStatus.receiptNbr, Equal<Required<INCostStatus.receiptNbr>>>>>>>,
						OrderBy<Asc<INCostStatus.receiptDate, Asc<INCostStatus.receiptNbr>>>>();

					parameters = new object[] { split.InventoryID, costsiteid, costsubitemid, transferNbr };
					break;
				case INValMethod.Specific:
					cmd = new Select<INCostStatus,
						Where<INCostStatus.inventoryID, Equal<Required<INCostStatus.inventoryID>>,
						And<INCostStatus.costSiteID, Equal<Required<INCostStatus.costSiteID>>,
						And<INCostStatus.costSubItemID, Equal<Required<INCostStatus.costSubItemID>>,
						And<INCostStatus.lotSerialNbr, Equal<Required<INCostStatus.lotSerialNbr>>,
						And<INCostStatus.layerType, Equal<INLayerType.normal>,
						And<INCostStatus.receiptNbr, Equal<Required<INCostStatus.receiptNbr>>>>>>>>>();
					parameters = new object[] { split.InventoryID, costsiteid, costsubitemid, split.LotSerialNbr, transferNbr };
					break;
				default:
					throw new PXException();
			}

			return new PXView(this, false, cmd);
		}

		protected virtual int? GetCostStatusCommandCostSiteID(POReceiptLine line)
			=> INTransitSiteID;

		public virtual decimal GetTransitSplitUnitCost(POReceiptLine line, POReceiptLineSplit split, InventoryItem item, int? costsubitemid, string transferNbr)
        {
            if (split.BaseQty == 0m || split.BaseQty == null)
                return 0m;

            object[] parameters;
            PXView cmd = GetCostStatusCommand(line, split, item, transferNbr, costsubitemid, out parameters);
            INCostStatus layer = (INCostStatus)cmd.SelectSingle(parameters);
            
            return layer.TotalCost.Value / layer.QtyOnHand.Value;
		}

		public virtual void AddPOOrderReceipt(string aOrderType, string aOrderNbr)
		{
			POOrderReceipt receiptOrder = null;
			foreach (POOrderReceipt it in this.ReceiptOrders.Select())
			{
				if (it.POType == aOrderType && it.PONbr == aOrderNbr) receiptOrder = it;
			}
			if (receiptOrder == null)
			{
				POOrder order = PXSelectReadonly<POOrder, Where<POOrder.orderType, Equal<Required<POOrder.orderType>>,
													And<POOrder.orderNbr, Equal<Required<POOrder.orderNbr>>>>>.Select(this, aOrderType, aOrderNbr);
				if (order != null)
				{
					receiptOrder = new POOrderReceipt();
					receiptOrder.POType = aOrderType;
					receiptOrder.PONbr = aOrderNbr;
					receiptOrder.OrderNoteID = order.NoteID;
					receiptOrder = this.ReceiptOrders.Insert(receiptOrder);
					POReceipt doc = this.Document.Current;
					doc.POType = order.OrderType;
					if (order.OrderType == POOrderType.DropShip)
					{
						doc.ShipToBAccountID = order.ShipToBAccountID;
						doc.ShipToLocationID = order.ShipToLocationID;
						this.Document.Update(doc);
					}
				}
			}
		}
		protected virtual void CheckRctForPOQuantityRule(PXCache sender, POReceiptLine row, bool displayAsWarning)
		{
			CheckRctForPOQuantityRule(sender, row, displayAsWarning, null);
		}

		protected virtual void CheckRctForPOQuantityRule(PXCache sender, POReceiptLine row, bool displayAsWarning, POLine aOriginLine)
		{
			if (row.Released == true)
				return;
			POLine poLine = aOriginLine;
			if (poLine == null || (poLine.OrderType != row.POType || poLine.OrderNbr != row.PONbr || poLine.LineNbr != row.POLineNbr))
			{
				poLine = PXSelectReadonly<POLine, Where<POLine.orderType, Equal<Required<POLine.orderType>>,
							And<POLine.orderNbr, Equal<Required<POLine.orderNbr>>,
							And<POLine.lineNbr, Equal<Required<POLine.lineNbr>>>>>>.Select(this, row.POType, row.PONbr, row.POLineNbr);
			}
			if (poLine != null &&
				 (poLine.RcptQtyAction == POReceiptQtyAction.Reject ||
					poLine.RcptQtyAction == POReceiptQtyAction.AcceptButWarn))
			{
				if (poLine.OrderQty != Decimal.Zero)
				{
					decimal qty = (decimal)row.ReceiptQty;
					if (row.InventoryID != null)
					{
						if (!string.IsNullOrEmpty(poLine.UOM) && !string.IsNullOrEmpty(row.UOM) && poLine.UOM != row.UOM)
						{
							qty = INUnitAttribute.ConvertFromTo<POReceiptLine.inventoryID>(sender, row, row.UOM, poLine.UOM, qty, INPrecision.QUANTITY);
						}
						foreach (POReceiptLine iLine in PXSelect<POReceiptLine,
												Where<POReceiptLine.receiptType, Equal<Required<POReceiptLine.receiptType>>,
													And<POReceiptLine.receiptNbr, Equal<Required<POReceiptLine.receiptNbr>>,
													And<POReceiptLine.lineNbr, NotEqual<Required<POReceiptLine.lineNbr>>,
														And<POReceiptLine.pOType, Equal<Required<POReceiptLine.pOType>>,
														And<POReceiptLine.pONbr, Equal<Required<POReceiptLine.pONbr>>,
														And<POReceiptLine.pOLineNbr, Equal<Required<POReceiptLine.pOLineNbr>>>>>>>>>.Select(this, row.ReceiptType, row.ReceiptNbr, row.LineNbr, row.POType, row.PONbr, row.POLineNbr))
						{
							if (!string.IsNullOrEmpty(poLine.UOM) && !string.IsNullOrEmpty(iLine.UOM) && poLine.UOM != iLine.UOM && iLine.Qty.HasValue)
							{
								qty += INUnitAttribute.ConvertFromTo<POReceiptLine.inventoryID>(sender, iLine, iLine.UOM, poLine.UOM, iLine.Qty.Value, INPrecision.QUANTITY);
							}
						}
					}
                    decimal minQty = PXDBQuantityAttribute.Round((poLine.RcptQtyMin.Value / 100.0m) * poLine.OpenQty.Value);
					decimal ratio = (qty / poLine.OrderQty.Value) * 100.0m;
					decimal maxRatio = ratio;
					POLineR poLineUpdated = GetReferencedPOLineR(row.POType, row.PONbr, row.POLineNbr);
					if (poLineUpdated != null)
						maxRatio = (poLineUpdated.ReceivedQty.Value / poLine.OrderQty.Value) * 100.0m;

					if (row.ReceiptType == POReceiptType.POReceipt)
					{
						PXErrorLevel errorLevel = ((!displayAsWarning && (poLine.RcptQtyAction == POReceiptQtyAction.Reject))
						                           	? PXErrorLevel.Error
						                           	: PXErrorLevel.Warning);
                        if (qty < minQty)
					{
							sender.RaiseExceptionHandling<POReceiptLine.receiptQty>(row, row.ReceiptQty,
							                                                        new PXSetPropertyException(
							                                                        	Messages.ReceiptLineQtyDoesNotMatchMinPOQuantityRules,
							                                                        	errorLevel));
					}
					if (maxRatio > poLine.RcptQtyMax)
					{
							sender.RaiseExceptionHandling<POReceiptLine.receiptQty>(row, row.ReceiptQty,
							                                                        new PXSetPropertyException(
							                                                        	Messages.ReceiptLineQtyDoesNotMatchMaxPOQuantityRules,
							                                                        	errorLevel));
						}
					}
					else if (poLineUpdated != null && poLineUpdated.ReceivedQty < 0)
					{
						sender.RaiseExceptionHandling<POReceiptLine.receiptQty>(row, row.ReceiptQty,
																													new PXSetPropertyException(
																														Messages.ReceiptLineQtyGoNegative,
																														PXErrorLevel.Error));
					}

				}
			}
		}

		[Obsolete(Common.Messages.MethodIsObsoleteRemoveInLaterAcumaticaVersions)]
		protected virtual void UpdatePOLineCompleteFlag(POReceiptLine row, bool isDeleted)
		{
			UpdatePOLineCompleteFlag(row, isDeleted, null);
		}

		protected virtual void UpdatePOLineCompleteFlag(POReceiptLine row, bool isDeleted, POLine aOriginLine)
		{
			if (string.IsNullOrEmpty(row.PONbr) || string.IsNullOrEmpty(row.POType) || row.POLineNbr == null)
				return;

			POLineR poLineCurrent = GetReferencedPOLineR(row.POType, row.PONbr, row.POLineNbr);
			POLine poLineOrigin = aOriginLine;
			if (poLineOrigin == null || (poLineOrigin.OrderType != row.POType || poLineOrigin.OrderNbr != row.PONbr || poLineOrigin.LineNbr != row.POLineNbr))
			{
				poLineOrigin = PXSelectReadonly<POLine,
					Where<POLine.orderType, Equal<Required<POLine.orderType>>,
					And<POLine.orderNbr, Equal<Required<POLine.orderNbr>>,
					And<POLine.lineNbr, Equal<Required<POLine.lineNbr>>>>>>
					.Select(this, row.POType, row.PONbr, row.POLineNbr);
			}
			if (poLineCurrent == null || poLineOrigin == null)
				return;

			if (poLineOrigin.CompletePOLine != CompletePOLineTypes.Quantity)
				return;

			if (row.ReceiptType == POReceiptType.POReceipt)
			{
				decimal qtyTreshold = poLineOrigin.OrderQty.Value * poLineOrigin.RcptQtyThreshold.Value / 100.0m;
				bool needToCompletePOByQty = (poLineCurrent.ReceivedQty.Value >= qtyTreshold);

				if (needToCompletePOByQty != poLineCurrent.AllowComplete)
				{
					poLineCurrent.AllowComplete = needToCompletePOByQty;
					this.Caches[typeof(POLineR)].Update(poLineCurrent);
					this.transactions.View.RequestRefresh();
				}
				row.AllowComplete = row.AllowOpen = poLineCurrent.AllowComplete;
			}
			else
			{
				bool needToOpen = poLineCurrent.Completed == true;
				if (needToOpen != poLineCurrent.AllowComplete)
				{
					poLineCurrent.AllowComplete = needToOpen;
					this.Caches[typeof(POLineR)].Update(poLineCurrent);
					this.transactions.View.RequestRefresh();
				}
				row.AllowComplete = row.AllowOpen = poLineCurrent.AllowComplete;
			}
		}

		protected virtual void DeleteUnusedReference(POReceiptLine aLine, bool skipReceiptUpdate)
		{
			if (string.IsNullOrEmpty(aLine.POType) || string.IsNullOrEmpty(aLine.PONbr)) return;

			string aOrderType = aLine.POType;
			string aOrderNbr = aLine.PONbr;
			POReceipt doc = this.Document.Current;
			POOrderReceipt receiptOrder = null;
			foreach (POOrderReceipt it in this.ReceiptOrders.Select())
			{
				if (it.POType == aOrderType && it.PONbr == aOrderNbr) receiptOrder = it;
			}

			foreach (POReceiptLine iLine in PXSelect<POReceiptLine,
										Where<POReceiptLine.receiptType, Equal<Required<POReceipt.receiptType>>,
											And<POReceiptLine.receiptNbr, Equal<Required<POReceipt.receiptNbr>>,
											And<POReceiptLine.pOType, Equal<Required<POReceiptLine.pOType>>,
												And<POReceiptLine.pONbr, Equal<Required<POReceiptLine.pONbr>>>>>>>.Select(this, aLine.ReceiptType, aLine.ReceiptNbr, aOrderType, aOrderNbr))
			{
				if (iLine.LineNbr != aLine.LineNbr)
					return;  //Other reference to the same receipt exists				
			}
			if (receiptOrder != null)
				this.ReceiptOrders.Delete(receiptOrder);

			if (!skipReceiptUpdate)
			{
				POOrder order = PXSelectReadonly<POOrder, Where<POOrder.orderType, Equal<Required<POOrder.orderType>>,
														And<POOrder.orderNbr, Equal<Required<POOrder.orderNbr>>>>>.Select(this, aOrderType, aOrderNbr);
				if (doc.POType == aLine.POType)
				{
					POReceiptLine typeRef = null;
					foreach (PXResult<POReceiptLine, POOrder> it in PXSelectJoin<POReceiptLine, InnerJoin<POOrder, On<POOrder.orderType, Equal<POReceiptLine.pOType>,
																				And<POOrder.orderNbr, Equal<POReceiptLine.pONbr>>>>,
																	Where<POReceiptLine.receiptType, Equal<Required<POReceipt.receiptType>>,
																	And<POReceiptLine.receiptNbr, Equal<Required<POReceipt.receiptNbr>>,
																	And<POReceiptLine.pOType, Equal<Required<POReceiptLine.pOType>>>>>>.Select(this, aLine.ReceiptType, aLine.ReceiptNbr, aOrderType))
					{
						POReceiptLine iLine = (POReceiptLine)it;
						POOrder iOrder = (POOrder)it;
						if (iLine.LineNbr == aLine.LineNbr) continue;
						typeRef = iLine;
						if (!doc.ShipToBAccountID.HasValue || !doc.ShipToLocationID.HasValue
							|| (doc.ShipToBAccountID == iOrder.ShipToBAccountID && doc.ShipToLocationID == iOrder.ShipToLocationID))
							return;	//Another reference to the same receipt type/destination exists 						
					}

					doc.ShipToBAccountID = null;
					doc.ShipToLocationID = null;
					if (typeRef == null)
						doc.POType = null;
					this.Document.Update(doc);
				}
			}
		}

		public virtual void Copy(POReceiptLine aDest, POLine aSrc, decimal aQtyAdj, decimal aBaseQtyAdj)
		{
			aDest.BranchID = aSrc.BranchID;
			aDest.POType = aSrc.OrderType;
			aDest.PONbr = aSrc.OrderNbr;
			aDest.POLineNbr = aSrc.LineNbr;
			aDest.IsStockItem = aSrc.IsStockItem;
			aDest.InventoryID = aSrc.InventoryID;
			aDest.AccrueCost = aSrc.AccrueCost;
			aDest.AlternateID = aSrc.AlternateID;
			aDest.SubItemID = aSrc.SubItemID;
			aDest.SiteID = aSrc.SiteID;
			aDest.LineType = aSrc.LineType;
			aDest.DropshipExpenseRecording = aSrc.DropshipExpenseRecording;
			aDest.POAccrualType = (aDest.ReceiptType == POReceiptType.POReturn) ? POAccrualType.Receipt : aSrc.POAccrualType;
			if (aDest.POAccrualType == POAccrualType.Order)
			{
				aDest.POAccrualRefNoteID = aSrc.OrderNoteID;
				aDest.POAccrualLineNbr = aSrc.LineNbr;
			}
			aDest.POAccrualAcctID = aSrc.POAccrualAcctID;
			aDest.POAccrualSubID = aSrc.POAccrualSubID;
			if (this.Document.Current.ReceiptType == POReceiptType.POReturn)
			{
				aDest.LineType =
						POLineType.IsStock(aSrc.LineType) ? POLineType.GoodsForInventory
					: POLineType.IsNonStock(aSrc.LineType) ? POLineType.NonStock
					: POLineType.Freight;
			}

			if (aDest.LineType == POLineType.GoodsForInventory || aDest.LineType == POLineType.GoodsForReplenishment)
			{
				aDest.OrigPlanType = INPlanConstants.Plan70;
			}
			else if (aDest.LineType == POLineType.GoodsForSalesOrder)
			{
				aDest.OrigPlanType = INPlanConstants.Plan76;
			}
            else if (aDest.LineType == POLineType.GoodsForServiceOrder)
            {
                aDest.OrigPlanType = INPlanConstants.PlanF7;
            }
			else if (aDest.LineType == POLineType.GoodsForDropShip)
			{
				aDest.OrigPlanType = INPlanConstants.Plan74;
			}
			else if (aDest.LineType == POLineType.GoodsForManufacturing)
			{
			    aDest.OrigPlanType = INPlanConstants.PlanM4;
			}

			aDest.TranDesc = aSrc.TranDesc;
            //volume and weight must be defaulted
			aDest.UnitVolume = null;
			aDest.UnitWeight = null;
			aDest.UOM = aSrc.UOM;
			aDest.VendorID = aSrc.VendorID;
			aDest.AlternateID = aSrc.AlternateID;			
            if (posetup.Current.DefaultReceiptQty == DefaultReceiptQuantity.Zero && aSrc.LineType != POLineType.Freight)
            {
                aDest.BaseQty = Decimal.Zero;
                aDest.Qty = Decimal.Zero;
            }
			else if (aSrc.OrderedQtyAltered == true)
			{
				aDest.UOM = aSrc.OverridenUOM;
				aDest.BaseQty = aSrc.BaseOverridenQty;
				aDest.Qty = aSrc.OverridenQty;
			}
            else
            {
                aDest.BaseQty =
                    this.Document.Current.ReceiptType == POReceiptType.POReturn
                    ? aSrc.BaseReceivedQty - aBaseQtyAdj
                    : aSrc.LeftToReceiveBaseQty - aBaseQtyAdj;
                aDest.Qty =
                    this.Document.Current.ReceiptType == POReceiptType.POReturn
                    ? aSrc.ReceivedQty - aQtyAdj
                    : aSrc.LeftToReceiveQty - aQtyAdj;
            }
						
			aDest.ExpenseAcctID = aSrc.ExpenseAcctID;
			aDest.ExpenseSubID = aSrc.ExpenseSubID;

			aDest.AllowComplete = aDest.AllowOpen = (aSrc.CompletePOLine == CompletePOLineTypes.Quantity);

			aDest.ManualPrice = false;

			bool copyAllowEditUnitCostFromPO = (POLineType.UsePOAccrual(aDest.LineType) && (aDest.POAccrualType == POAccrualType.Receipt)) || 
				(!POLineType.UsePOAccrual(aDest.LineType) && !(string.IsNullOrEmpty(aDest.POType) || string.IsNullOrEmpty(aDest.PONbr) || aDest.POLineNbr == null));

			aDest.AllowEditUnitCost = copyAllowEditUnitCostFromPO ? aSrc.AllowEditUnitCostInPR : false;
			aDest.DiscPct = aSrc.DiscPct;

			CalculateAmountFromPOLine(aDest, aSrc);
			aDest.ProjectID = aSrc.ProjectID;
			aDest.TaskID = aSrc.TaskID;
			aDest.CostCodeID = aSrc.CostCodeID;

			aDest.IsSpecialOrder = aSrc.IsSpecialOrder;
			aDest.CostCenterID = aSrc.CostCenterID;
		}

		public virtual void CalculateAmountFromPOLine(POReceiptLine aDest, POLine aSrc)
		{
			var accrualAmt = APReleaseProcess.GetExpensePostingAmount(this, aSrc);

			decimal baseUnitCost = aSrc.UnitCost ?? 0m;
			decimal baseAccrualAmt = accrualAmt.Base ?? 0m;
			decimal curyAccrualAmt = accrualAmt.Cury ?? 0m;
			CurrencyInfo srcCuryInfo = MultiCurrencyExt.GetCurrencyInfo(aSrc.CuryInfoID);
			CurrencyInfo destCuryInfo = MultiCurrencyExt.GetCurrencyInfo(aDest.CuryInfoID);

			if (AllowNonBaseCurrency() && destCuryInfo.CuryID == srcCuryInfo.CuryID)
			{
				aDest.CuryUnitCost = aSrc.CuryUnitCost ?? 0m;
				aDest.CuryExtCost = destCuryInfo.RoundCury(((aSrc.OrderQty != 0m) ? (aSrc.CuryLineAmt ?? 0m) * aDest.Qty / aSrc.OrderQty : (aSrc.CuryLineAmt ?? 0m)) ?? 0m);
				aDest.CuryTranCost = destCuryInfo.RoundCury(((aSrc.OrderQty != 0m) ? curyAccrualAmt * aDest.Qty / aSrc.OrderQty : curyAccrualAmt) ?? 0m);
				aDest.CuryTranUnitCost = (aSrc.OrderQty != 0m) ? curyAccrualAmt / aSrc.OrderQty : curyAccrualAmt;

				if (aDest.CuryTranUnitCost != null)
					aDest.CuryTranUnitCost = Math.Round(aDest.CuryTranUnitCost.Value, POReceiptLine.curyTranUnitCost.Precision, MidpointRounding.AwayFromZero);
			}
			else
			{
				if (destCuryInfo.CuryID == destCuryInfo.BaseCuryID)
				{
					aDest.CuryUnitCost = baseUnitCost;
					aDest.CuryExtCost = destCuryInfo.RoundCury(((aSrc.OrderQty != 0m) ? aSrc.LineAmt * aDest.Qty / aSrc.OrderQty : aSrc.LineAmt) ?? 0m);
					aDest.CuryTranCost = destCuryInfo.RoundCury(((aSrc.OrderQty != 0m) ? baseAccrualAmt * aDest.Qty / aSrc.OrderQty : baseAccrualAmt) ?? 0m);
					aDest.CuryTranUnitCost = (aSrc.OrderQty != 0m) ? baseAccrualAmt / aSrc.OrderQty : baseAccrualAmt;

					if (aDest.CuryTranUnitCost != null)
						aDest.CuryTranUnitCost = Math.Round(aDest.CuryTranUnitCost.Value, POReceiptLine.curyTranUnitCost.Precision, MidpointRounding.AwayFromZero);
				}
				else
				{
					aDest.UnitCost = baseUnitCost;
					aDest.ExtCost = destCuryInfo.RoundCury(((aSrc.OrderQty != 0m) ? aSrc.LineAmt * aDest.Qty / aSrc.OrderQty : aSrc.LineAmt) ?? 0m);
					aDest.TranCost = destCuryInfo.RoundCury(((aSrc.OrderQty != 0m) ? baseAccrualAmt * aDest.Qty / aSrc.OrderQty : baseAccrualAmt) ?? 0m);
					aDest.TranUnitCost = (aSrc.OrderQty != 0m) ? baseAccrualAmt / aSrc.OrderQty : baseAccrualAmt;

					aDest.CuryUnitCost = destCuryInfo.CuryConvCury(aDest.UnitCost ?? 0m, CommonSetupDecPl.PrcCst);
					aDest.CuryExtCost = destCuryInfo.CuryConvCury(aDest.ExtCost ?? 0m);
					aDest.CuryTranCost = destCuryInfo.CuryConvCury(aDest.TranCost ?? 0m);
					aDest.CuryTranUnitCost = destCuryInfo.CuryConvCuryRaw(aDest.TranUnitCost ?? 0m);

					if (aDest.CuryTranUnitCost != null)
						aDest.CuryTranUnitCost = Math.Round(aDest.CuryTranUnitCost.Value, POReceiptLine.curyTranUnitCost.Precision, MidpointRounding.AwayFromZero);
				}
			}
		}

        public virtual void Copy(POReceiptLine aDest, INTran aSrc)
        {
            aDest.POType = null;
            aDest.PONbr = null;
            aDest.POLineNbr = null;
			aDest.IsStockItem = aSrc.IsStockItem;
            aDest.InventoryID = aSrc.InventoryID;
            aDest.SubItemID = aSrc.SubItemID;
            aDest.SiteID = aSrc.ToSiteID;
            aDest.LocationID = aSrc.ToLocationID;
            aDest.LineType = POLineType.GoodsForInventory;
            aDest.UOM = aSrc.UOM;
            aDest.Qty = 0;
            aDest.BaseQty = 0;
            aDest.OrigDocType = aSrc.DocType;
            aDest.OrigTranType = aSrc.TranType;
            aDest.OrigRefNbr = aSrc.RefNbr;
            aDest.OrigLineNbr = aSrc.LineNbr;
            aDest.SOOrderType = aSrc.SOOrderType;
            aDest.SOOrderNbr = aSrc.SOOrderNbr;
            aDest.SOOrderLineNbr = aSrc.SOOrderLineNbr;
			aDest.SOShipmentType = aSrc.SOShipmentType;
			aDest.SOShipmentNbr = aSrc.SOShipmentNbr;
            aDest.ExpenseAcctID = null;
            aDest.ExpenseSubID = null;
            aDest.ReasonCode = null;

			aDest.TranDesc = aSrc.TranDesc;
            aDest.CuryUnitCost = 0m;
            //aDest.UnitVolume = aSrc.UnitVolume;
            //aDest.UnitWeight = aSrc.UnitWeight;
            //aDest.VendorID = aSrc.VendorID;
            //aDest.AlternateID = aSrc.AlternateID;
            //aDest.CuryUnitCost = aSrc.CuryUnitCost;
            aDest.AllowComplete = true;
            aDest.AllowOpen = true;
			aDest.CostCodeID = aSrc.CostCodeID;
			aDest.AllowEditUnitCost = false;

			if (aDest.LineType != POLineType.GoodsForInventory)//for stock items the project and task is taken from the Location via PODefaultProjectAttribute.
            {
                aDest.ProjectID = aSrc.ProjectID;
                aDest.TaskID = aSrc.TaskID;
            }

			aDest.IsSpecialOrder = aSrc.IsSpecialOrder;
			if (aSrc.DocType == INDocType.Transfer)
			{
				aDest.CostCenterID = aSrc.ToCostCenterID;
			}

		}

		protected virtual void ClearUnused(POReceiptLine aLine)
		{
			if (aLine.LineType.IsIn(
					POLineType.Freight,
					POLineType.MiscCharges, 
					POLineType.NonStock,
					POLineType.NonStockForDropShip,
					POLineType.NonStockForSalesOrder,
                    POLineType.NonStockForServiceOrder))
			{
				aLine.SubItemID = null;
				if (aLine.LineType == POLineType.MiscCharges)
				{
					aLine.InventoryID = null;
					aLine.UOM = null;
					aLine.CuryUnitCost = decimal.Zero;
				}
				aLine.LocationID = null;
			}

			if (!IsExpenseAccountRequired(aLine))
			{
				aLine.ExpenseAcctID = null;
				aLine.ExpenseSubID = null;
			}
			if (!IsAccrualAccountRequired(aLine))
			{
				aLine.POAccrualAcctID = null;
				aLine.POAccrualSubID = null;
			}
		}

		protected virtual bool CanAddOrder(POOrder aOrder, out string message)
		{
			POReceipt doc = this.Document.Current;
			if (!String.IsNullOrEmpty(doc.POType))
			{
				if (doc.POType != aOrder.OrderType)
				{
					message = String.Format(PXMessages.LocalizeNoPrefix(Messages.PurchaseOrderHasTypeDifferentFromOthersInPOReceipt), PXMessages.LocalizeNoPrefix(aOrder.OrderType),aOrder.OrderNbr);
					return false;
				}
				if ((doc.ShipToBAccountID.HasValue || doc.ShipToLocationID.HasValue))
				{
					if (doc.ShipToBAccountID != aOrder.ShipToBAccountID || doc.ShipToLocationID != aOrder.ShipToLocationID)
					{
						message = String.Format(PXMessages.LocalizeNoPrefix(Messages.PurchaseOrderHasShipDestinationDifferentFromOthersInPOReceipt), PXMessages.LocalizeNoPrefix(aOrder.OrderType),aOrder.OrderNbr);
						return false;
					}
				}
			}
			message = string.Empty;
			return true;
		}

		protected static bool IsPassingFilter(POOrderFilter aFilter, POOrder aOrder)
		{
			if (!string.IsNullOrEmpty(aFilter.OrderType) && aOrder.OrderType != aFilter.OrderType) return false;
			if (aFilter.ShipToBAccountID != null && aOrder.ShipToBAccountID != aFilter.ShipToBAccountID) return false;
			if (aFilter.ShipToLocationID != null && aOrder.ShipToLocationID != aFilter.ShipToLocationID) return false;
			return true;
		}

		protected static bool IsPassingFilter(POOrderFilter aFilter, POLine aLine)
		{
			if (aLine.OrderType != aFilter.OrderType) return false;
			if (aLine.OrderNbr != aFilter.OrderNbr) return false;
			return true;
		}

		protected virtual bool HasOrderWithRetainage(POReceipt row)
		{
			POOrderReceipt retained = PXSelectJoin<POOrderReceipt,
				InnerJoin<POOrder, On<POOrder.orderType, Equal<POOrderReceipt.pOType>, And<POOrder.orderNbr, Equal<POOrderReceipt.pONbr>>>>,
				Where2<POOrderReceipt.FK.Receipt.SameAsCurrent,
					And<POOrder.retainageApply, Equal<boolTrue>>>>
				.SelectSingleBound(this, new[] { row });

			return retained != null;
		}
        #endregion

        #region Release Functions
        public virtual void ReleaseReceipt(INReceiptEntry docgraph, AP.APInvoiceEntry invoiceGraph, POReceipt aDoc, DocumentList<INRegister> aINCreated, DocumentList<AP.APInvoice> aAPCreated, bool aIsMassProcess)
		{
			var prepareGraph = Func.Memorize(() =>
			{
				if (docgraph == null)
					docgraph = CreateReceiptEntry();

				docgraph.insetup.Current.RequireControlTotal = false;
				docgraph.insetup.Current.HoldEntry = false;
				return docgraph;
			});

			var prepareDocument = Func.Memorize(() =>
			{
				prepareGraph();

				INRegister newdoc = new INRegister
				{
					BranchID = aDoc.BranchID,
					DocType = INDocType.Receipt,
					SiteID = null,
					TranDate = aDoc.ReceiptDate,
					FinPeriodID = aDoc.FinPeriodID,
					OrigModule = BatchModule.PO
				};

				newdoc = docgraph.receipt.Insert(newdoc);
				return newdoc;
			});

			this.Clear();
			docgraph?.Clear();

			invoiceGraph.Clear();

			invoiceGraph.APSetup.Current.RequireControlTotal = false;
			invoiceGraph.APSetup.Current.RequireControlTaxTotal = false;
			if (this.posetup.Current.AutoReleaseAP == true)
			{
				invoiceGraph.APSetup.Current.HoldEntry = false;
			}

			aDoc = ActualizeAndValidatePOReceiptForReleasing(aDoc);

			POReceiptLine prev_line = null;
			INTran newline = null;
			var podemand = new HashSet<PXResult<INItemPlan, INPlanType>>();
			var posupply = new List<INItemPlan>();
			bool hasStockItems = false;
			string origTransferNbr = null;

			POLineUOpen loadPOLine(string poType, string poNbr, int? poLineNbr)
			{
				if (string.IsNullOrEmpty(poType) || string.IsNullOrEmpty(poNbr) || poLineNbr == null)
					return null;
				//Need special select for correct updating data merging in the cache
				return PXSelect<POLineUOpen, Where<POLineUOpen.orderType, Equal<Required<POLineUOpen.orderType>>,
					And<POLineUOpen.orderNbr, Equal<Required<POLineUOpen.orderNbr>>,
						And<POLineUOpen.lineNbr, Equal<Required<POLineUOpen.lineNbr>>>>>>.Select(this, poType, poNbr, poLineNbr);
			};

			foreach (PXResult<POReceiptLine> res in GetLinesToReleaseQuery().Select(aDoc.ReceiptType, aDoc.ReceiptNbr))
			{
				POReceiptLine line = res;
				POReceiptLineSplit split = (POReceiptLineSplit)res.GetItem<POReceiptLineSplit>();
				INItemPlan plan = PXCache<INItemPlan>.CreateCopy(res.GetItem<INItemPlan>());
				INPlanType plantype = INPlanType.PK.Find(this, plan.PlanType) ?? new INPlanType();
				INSite insite = res.GetItem<INSite>();
				POLineUOpen poLine = loadPOLine(line.POType, line.PONbr, line.POLineNbr);
				POOrder poOrder = res.GetItem<POOrder>();
				POAddress poAddress = res.GetItem<POAddress>();

				if (line.BaseReceiptQty == 0 && split.ReceiptNbr == null)
					continue;

				ValidateReceiptLineOnRelease(res);

				bool isStock = POLineType.IsStockNonDropShip(line.LineType);
				bool isNonStock = POLineType.IsNonStockNonServiceNonDropShip(line.LineType);
				bool postToIN = isStock
					|| isNonStock && line.POAccrualAcctID != null
					|| POLineType.IsProjectDropShip(line.LineType) && line.DropshipExpenseRecording == DropshipExpenseRecordingOption.OnReceiptRelease;
				bool reattachExistingPlan = false;
				//Splits actually exist only for the Stock Items and Non-zero Qty
				if (!string.IsNullOrEmpty(split.ReceiptNbr))
				{
					if (!string.IsNullOrEmpty(plantype.PlanType) && (bool)plantype.DeleteOnEvent)
					{
						reattachExistingPlan = postToIN;
						if (!reattachExistingPlan)
						{
							Caches[typeof(INItemPlan)].Delete(plan);
						}
						Caches[typeof(POReceiptLineSplit)].MarkUpdated(split, assertError: true);
						split = (POReceiptLineSplit)Caches[typeof(POReceiptLineSplit)].Locate(split);
						if (split != null) split.PlanID = null;
						Caches[typeof(POReceiptLineSplit)].IsDirty = true;
					}
					else if (!string.IsNullOrEmpty(plantype.PlanType) && string.IsNullOrEmpty(plantype.ReplanOnEvent) == false)
					{
						plan.PlanType = plantype.ReplanOnEvent;
						Caches[typeof(INItemPlan)].Update(plan);
						Caches[typeof(POReceiptLineSplit)].MarkUpdated(split, assertError: true);
						Caches[typeof(POReceiptLineSplit)].IsDirty = true;
					}
				}
				bool rctLineChange = (Caches[typeof(POReceiptLine)].ObjectsEqual(prev_line, line) == false);
				if (rctLineChange)
				{
					ReleaseReceiptLine(line, poLine, poOrder);
				}
				bool isNonStockKit = split.InventoryID != null && line.InventoryID != split.InventoryID;

				if (rctLineChange || isNonStockKit)
				{
					if (postToIN)
					{
						prepareDocument();
						hasStockItems = true;
						newline = new INTran
						{
							BranchID = line.OrigTranType == INTranType.Transfer ? insite.BranchID : line.BranchID,
							TranType = POReceiptType.GetINTranType(aDoc.ReceiptType),
							POReceiptType = line.ReceiptType,
							POReceiptNbr = line.ReceiptNbr,
							POReceiptLineNbr = line.LineNbr,
							POLineType = line.LineType,
							OrigDocType = line.OrigDocType,
							OrigTranType = line.OrigTranType,
							OrigRefNbr = line.OrigRefNbr,
							OrigLineNbr = line.OrigLineNbr
						};

						if (line.OrigTranType == INTranType.Transfer)
						{
							newline.AcctID = null;
							newline.SubID = null;
							newline.InvtAcctID = null;
							newline.InvtSubID = null;
							newline.OrigPlanType = line.OrigPlanType;

							newline.OrigNoteID = line.OrigNoteID;
							newline.OrigToLocationID = line.OrigToLocationID;
							newline.OrigIsLotSerial = line.OrigIsLotSerial;

							if (origTransferNbr == null)
								origTransferNbr = line.OrigRefNbr;
						}
						else
						{
							newline.AcctID = line.POAccrualAcctID;
							newline.SubID = line.POAccrualSubID;
							newline.ReclassificationProhibited = true;

							if (POLineType.IsProjectDropShip(line.LineType))
							{
								newline.COGSAcctID = line.ExpenseAcctID;
								newline.COGSSubID = line.ExpenseSubID;
							}
							else
							{
								newline.InvtAcctID = line.ExpenseAcctID;
								newline.InvtSubID = line.ExpenseSubID;
							}

							newline.OrigPlanType = null;
						}

						newline.IsStockItem = isNonStockKit && POLineType.IsStockNonDropShip(line.LineType) ? null : line.IsStockItem;
						newline.InventoryID = isNonStockKit && POLineType.IsStockNonDropShip(line.LineType) ? split.InventoryID : line.InventoryID;
						newline.SiteID = line.SiteID;
						UpdateSOOrderLink(newline, poLine, line);

						if (isNonStockKit && !POLineType.IsNonStockNonServiceNonDropShip(line.LineType))
						{
							newline.SubItemID = split.SubItemID;
							newline.LocationID = split.LocationID;
							newline.UOM = split.UOM;
							newline.UnitPrice = 0m;
							newline.UnitCost = 0m;
							newline.TranDesc = null;
						}
						else
						{
							newline.SubItemID = line.SubItemID;
							newline.LocationID = line.LocationID;
							newline.UOM = line.UOM;
							newline.TranDesc = line.TranDesc;
							newline.ReasonCode = line.ReasonCode;
							newline.UnitCost = line.UnitCost;
						}

						newline.InvtMult = POLineType.IsProjectDropShip(line.LineType) ? 0 : line.InvtMult;
						newline.Qty = POLineType.IsStockNonDropShip(line.LineType) ? Decimal.Zero : line.Qty;
						newline.ExpireDate = line.ExpireDate;
						newline.AccrueCost = line.AccrueCost;
						newline.ProjectID = line.ProjectID;
						newline.TaskID = line.TaskID;
						newline.CostCodeID = line.CostCodeID;
						newline.IsIntercompany = line.IsIntercompany;
						docgraph.CostCenterDispatcherExt?.SetCostLayerType(newline);

						try
						{
							newline = docgraph.LineSplittingExt.lsselect.Insert(newline);
						}
						catch(PXException exception)
						{
							throw new Common.Exceptions.ErrorProcessingEntityException(
								Caches[line.GetType()], line, exception);
						}
					}
					else
					{
						newline = null;
					}

					UpdateReceiptLineOnRelease(res, poLine);
					poLine = (POLineUOpen)this.Caches<POLineUOpen>().Locate(poLine) ?? poLine;

					CollectDemandOrdersMarkedForPO(line, poLine, posupply, podemand);
					UpdateDemandOrdersMarkedForDropship(line, poLine, poAddress);
					CollectDemandForTransferOrders(line, posupply, podemand);
				}
				prev_line = line;
				if (newline != null && !string.IsNullOrEmpty(split.ReceiptNbr))
				{
					INTranSplit newsplit = (INTranSplit)newline;
					newsplit.SplitLineNbr = null;
					newsplit.SubItemID = split.SubItemID;
					newsplit.LocationID = split.LocationID;
					newsplit.LotSerialNbr = split.LotSerialNbr;
					newsplit.InventoryID = split.InventoryID;
					newsplit.UOM = split.UOM;
					newsplit.Qty = split.Qty;
					newsplit.ExpireDate = split.ExpireDate;
					newsplit.POLineType = poLine != null ? poLine.LineType : line.LineType;
					newsplit.OrigPlanType = line.OrigTranType == INTranType.Transfer ? line.OrigPlanType : null;

					if (reattachExistingPlan)
					{
						newsplit.PlanID = plan.PlanID;
						plan.OrigPlanID = null;
						plan.OrigPlanType = null;
						plan.OrigNoteID = null;
						plan.OrigPlanLevel = null;
						plan.IgnoreOrigPlan = false;
						plan.Reverse = false;
						plan.RefNoteID = docgraph.receipt.Current.NoteID;
						plan.RefEntityType = typeof(INRegister).FullName;
						docgraph.Caches[typeof(INItemPlan)].Update(plan);
					}

					try
					{
						newsplit = docgraph.splits.Insert(newsplit);

						ReattachDemandPlansToIN(docgraph, newsplit, posupply, podemand);
						docgraph.splits.Cache.RaiseRowUpdated(newsplit, newsplit);
					}
					catch (PXException exception)
					{
						throw new Common.Exceptions.ErrorProcessingEntityException(
							Caches[line.GetType()], line, exception);
					}
				}
				else if (reattachExistingPlan)
				{
					ThrowPlanNotReattachedError(line, split);
				}

				if (newline != null && newline.InventoryID == line.InventoryID)
				{
					//This needs to be done after splits insert - to override recalculation made by them
					newline.TranCost = line.TranCostFinal;
				}

				if (TryProcessReceiptLinkedAllocation(newline, aDoc, poLine, podemand, posupply))
				{
					podemand.Clear();
					posupply.Clear();
				}
			}

			if (hasStockItems)
			{
				INRegister copy = PXCache<INRegister>.CreateCopy(docgraph.receipt.Current);
				PXFormulaAttribute.CalcAggregate<INTran.qty>(docgraph.transactions.Cache, copy);
				PXFormulaAttribute.CalcAggregate<INTran.tranCost>(docgraph.transactions.Cache, copy);
				docgraph.receipt.Update(copy);
			}

			if (origTransferNbr != null)
			{
				docgraph.receipt.Cache.SetValue<INRegister.transferNbr>(docgraph.receipt.Current, origTransferNbr);
			}

			List<INRegister> forReleaseIN = new List<INRegister>();
			List<APRegister> forReleaseAP = new List<APRegister>();
			bool alreadyBilledError = false;
			bool isINDocValid = hasStockItems;

				using (PXTransactionScope ts = new PXTransactionScope())
				{
					if (isINDocValid) //Skip saving empty document
					{
						try
						{
						docgraph.Save.Press();
					}
						catch(PXOuterException exception)
						{
							if (exception.Row is INTran tran)
							{
								var pOReceiptLine = new POReceiptLine()
								{
									ReceiptType = tran.POReceiptType,
									ReceiptNbr = tran.POReceiptNbr,
									LineNbr = tran.POReceiptLineNbr
								};
								throw new Common.Exceptions.ErrorProcessingEntityException(
									docgraph.Caches[pOReceiptLine.GetType()], pOReceiptLine, exception);
							}
							else throw;
						}
					}

					var poReceipt = UpdateReceiptReleased(isINDocValid ? docgraph.receipt.Current : null);

					this.Save.Press();

					if (aDoc.AutoCreateInvoice == true)
					{
						if (poReceipt.UnbilledQty > 0m)
						{
							bool hasRetainedTaxes = (apsetup.Current.RetainTaxes == true) && HasOrderWithRetainage(aDoc);
							invoiceGraph.InvoicePOReceipt(aDoc, aAPCreated, keepOrderTaxes: !hasRetainedTaxes);
							invoiceGraph.AttachPrepayment();
							invoiceGraph.Save.Press();
							APInvoice apDoc = PXSelect<APInvoice, Where<APInvoice.docType, Equal<Required<APInvoice.docType>>, And<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>>>>.Select(this, invoiceGraph.Document.Current.DocType, invoiceGraph.Document.Current.RefNbr);
							apDoc.Passed = true;
							if (aAPCreated != null)
							{
								aAPCreated.Add(apDoc);
							}
							if (this.posetup.Current.AutoReleaseAP == true && apDoc.Hold == false)
							{
								forReleaseAP.Add((APRegister)apDoc);
								PXTimeStampScope.DuplicatePersisted(this.Caches[typeof(APRegister)], apDoc, typeof(APInvoice));
							}
						}
						else
						{
							alreadyBilledError = true;
						}
					}

					if (isINDocValid && this.posetup.Current.AutoReleaseIN == true && docgraph.receipt.Current.Hold == false)
					{
						forReleaseIN.Add(docgraph.receipt.Current);
					}

					ts.Complete();
				}

			if (isINDocValid && aINCreated.Find(docgraph.receipt.Current) == null)
			{
				aINCreated.Add(docgraph.receipt.Current);
			}

			if (forReleaseIN.Count > 0)
			{
				try
				{
					INDocumentRelease.ReleaseDoc(forReleaseIN, false);
				}
				catch (PXException ex)
				{
					if (ex.InnerException is PXIntercompanyReceivedNotIssuedException)
					{
						throw new PXIntercompanyReceivedNotIssuedException(ex.InnerException,
							Messages.IntercompanyReceivedNotIssued, forReleaseIN[0].RefNbr);
					}
					else
					{
						throw new PXException(Messages.INDocumentFailedToReleaseDuringPOReceiptRelease, ex.Message);
					}
				}
			}

			if (forReleaseAP.Count > 0)
			{
				try
				{
					APDocumentRelease.ReleaseDoc(forReleaseAP, aIsMassProcess);
				}
				catch (PXException ex)
				{
					throw new PXException(Messages.APDocumentFailedToReleaseDuringPOReceiptRelease, ex.Message);
				}
			}

			if (alreadyBilledError)
			{
				throw new PXException(Messages.APDocumentNotCreatedAllBilled);
			}
		}

		public virtual void PrefetchDropShipLinks(POOrder order)
		{
		}

		public virtual void ValidatePOOrder(POOrder order)
		{
		}

		public virtual void ValidateProjectDropShipLineOnRelease(POReceiptLine line)
		{
			if (line.DropshipExpenseRecording == DropshipExpenseRecordingOption.OnReceiptRelease &&
				PXAccess.FeatureInstalled<FeaturesSet.inventory>() && insetup.Current?.UpdateGL != true)
			{
				throw new PXException(Messages.ProjectDropShipReceiptReleaseUpdateGLNotSet);
			}
		}

		public virtual PXSelectBase<POReceiptLine> GetLinesToReleaseQuery()
		{
			return new PXSelectJoin<POReceiptLine,
				LeftJoin<POReceiptLineSplit, On<POReceiptLineSplit.FK.ReceiptLine>,
				LeftJoin<INTran, On<INTran.FK.POReceiptLine>,
				LeftJoin<INItemPlan, On<INItemPlan.planID, Equal<POReceiptLineSplit.planID>>,
				LeftJoin<INSite, On<POReceiptLine.FK.Site>,
				LeftJoin<POOrder, On<POReceiptLine.FK.Order>,
				LeftJoin<POAddress, On<POOrder.shipAddressID, Equal<POAddress.addressID>>>>>>>>,
				Where<POReceiptLine.receiptType, Equal<Required<POReceipt.receiptType>>,
					And<POReceiptLine.receiptNbr, Equal<Required<POReceipt.receiptNbr>>,
					And<INTran.refNbr, IsNull>>>,
				OrderBy<Asc<POReceiptLine.receiptNbr, Asc<POReceiptLine.lineNbr>>>>(this);
		}

		public virtual void ValidatePOLine(POLine poline, POReceipt receipt)
		{
		}

		public virtual POReceiptLine InsertReceiptLine(POReceiptLine line, POReceipt receipt, POLine poline)
		{
			return transactions.Insert(line);
		}

		protected virtual void ValidateLineOnRelease(PXResult<POReceiptLine> row)
		{
			var receiptLine = (POReceiptLine)row;

			if (LineSplittingExt.IsLSEntryEnabled(receiptLine) && Math.Abs(receiptLine.UnassignedQty ?? 0) >= 0.0000005m)
				throw new PXException(Messages.BinLotSerialNotAssigned);

			ValidateProjectDropShipLineOnRelease(receiptLine);
		}

		public virtual void ValidateReceiptLineOnRelease(PXResult<POReceiptLine> row)
		{
			ValidateLineOnRelease(row);
		}

		public virtual void ValidateReturnLineOnRelease(PXResult<POReceiptLine> row)
		{
			ValidateLineOnRelease(row);
			if (((POReceiptLine)row).POAccrualType == POAccrualType.Order)
			{
				throw new InvalidOperationException("PO Return cannot be Order-based, it must create its own separate PO Accrual.");
			}
		}

		protected virtual void UpdateReceiptLineOnRelease(PXResult<POReceiptLine> row, POLineUOpen poLine)
		{
		}

		protected virtual void UpdateSOOrderLink(INTran newtran, POLineUOpen poLine, POReceiptLine line)
		{
		}

		protected virtual void UpdateReturnLineOnRelease(PXResult<POReceiptLine> row, POLineUOpen poLine)
		{
			UpdateReturnOrdersMarkedForDropship(row, poLine);
		}

		protected virtual void ReduceSOAllocationOnReleaseReturn(PXResult<POReceiptLine> row, POReceiptLineSplit posplit, POLineUOpen poLine)
		{
		}

		public virtual void CollectDemandOrdersMarkedForPO(POReceiptLine line, POLineUOpen poLine,
			List<INItemPlan> posupply, HashSet<PXResult<INItemPlan, INPlanType>> podemand)
		{
			if (poLine == null ||
				poLine.LineType.IsNotIn(POLineType.GoodsForSalesOrder,
										POLineType.NonStockForSalesOrder,
										POLineType.GoodsForManufacturing,
										POLineType.NonStockForManufacturing,
										POLineType.GoodsForServiceOrder,
										POLineType.NonStockForServiceOrder))
			{
				return;
			}

			decimal? SupplyQty = line.BaseReceiptQty;

			foreach (PXResult<INItemPlan, INPlanType> demandres in
				SelectFrom<INItemPlan>
					.InnerJoin<INPlanType>.On<INItemPlan.FK.PlanType>
					.LeftJoin<SOLineSplit>.On<SOLineSplit.planID.IsEqual<INItemPlan.planID>>
					.Where<INItemPlan.planQty.IsGreater<decimal0>
						.And<INPlanType.isDemand.IsEqual<True>>
						.And<INPlanType.isFixed.IsEqual<True>>
						.And<INItemPlan.supplyPlanID.IsEqual<POLine.planID.FromCurrent>
							.And<SOLineSplit.pONbr.IsNull>
							.Or<SOLineSplit.FK.POLine.SameAsCurrent
							.And<SOLineSplit.pOCreate.IsEqual<True>>
							.And<SOLineSplit.completed.IsNotEqual<True>>>>>
				.View.SelectMultiBound(this, new[] { poLine.ToPOLine() }).AsEnumerable()
				.OrderBy(res => PXResult.Unwrap<SOLineSplit>(res).Behavior == SOBehavior.BL))
				// TODO: BlanketSO: revise the sorting - Child Orders should go before parent Blankets
			{
				INItemPlan demand = demandres;
				bool lineIsFromServiceModule = poLine.LineType.IsIn(POLineType.GoodsForServiceOrder, POLineType.NonStockForServiceOrder);
				var supply = new INItemPlan
				{
					InventoryID = line.InventoryID,
					SiteID = line.SiteID,
					SubItemID = line.SubItemID,
					LocationID = lineIsFromServiceModule ? line.LocationID : null,
					BAccountID = !lineIsFromServiceModule ? demand.BAccountID : null,
					DemandPlanID = demand.PlanID,
					RefNoteID = demand.RefNoteID,
					RefEntityType = demand.RefEntityType,
					PlanDate = demand.PlanDate,
					PlanType = lineIsFromServiceModule ? INPlanConstants.PlanF5 : INPlanConstants.Plan64,
					FixedSource = line.SiteID != demand.SiteID ? INReplenishmentSource.Transfer : INReplenishmentSource.None,
					SourceSiteID = line.SiteID,
					Hold = false,
					ProjectID = line.ProjectID,
					TaskID = line.TaskID,
					CostCenterID = line.CostCenterID,
				};

				if (SupplyQty >= demand.PlanQty)
				{
					supply.PlanQty = demand.PlanQty;


					posupply.Add(supply);
					SupplyQty -= demand.PlanQty;
					podemand.Add(demandres);
				}
				else if (SupplyQty > 0m)
				{
					supply.PlanQty = SupplyQty;

					posupply.Add(supply);
					SupplyQty = 0m;
				}

				if (poLine.Completed == true)
				{
					podemand.Add(demandres);
				}
			}
		}

		public virtual void UpdateDemandOrdersMarkedForDropship(POReceiptLine line, POLineUOpen poLine, POAddress poAddress)
		{
			if (poLine == null ||
				poLine.LineType.IsNotIn(POLineType.GoodsForDropShip, POLineType.NonStockForDropShip))
			{
				return;
			}

			var result = PXSelectJoin<SOLine4,
				InnerJoin<SOLineSplit,
					On<SOLineSplit.orderType, Equal<SOLine4.orderType>,
						And<SOLineSplit.orderNbr, Equal<SOLine4.orderNbr>,
						And<SOLineSplit.lineNbr, Equal<SOLine4.lineNbr>>>>>,
				Where<SOLineSplit.pOType, Equal<Current<POReceiptLine.pOType>>,
					And<SOLineSplit.pONbr, Equal<Current<POReceiptLine.pONbr>>,
					And<SOLineSplit.pOLineNbr, Equal<Current<POReceiptLine.pOLineNbr>>>>>,
				OrderBy<Asc<SOLineSplit.splitLineNbr>>>
				.SelectMultiBound(this, new object[] { line }).AsEnumerable()
				.Cast<PXResult<SOLine4, SOLineSplit>>().ToArray();

			if (!result.Any()) return;

			CurrencyInfo currencyInfo = MultiCurrencyExt.GetDefaultCurrencyInfo();

			decimal? baseSplitReceivedQty = null;
			decimal? splitReceivedQty = null;

			decimal? baseReceivedQtyUndistributed = null;
			decimal? receivedQtyUndistributed = null;

			for (int i = 0; i < result.Length; i++)
			{
				SOLine4 soline = result[i];
				SOOrder order = PXParentAttribute.SelectParent<SOOrder>(solineselect.Cache, soline);
				if (order == null)
				{
					throw new Common.Exceptions.RowNotFoundException(this.Caches<SOOrder>(), soline.OrderType, soline.OrderNbr);
				}
				if (soline.Completed == true)
				{
					var item = InventoryItem.PK.Find(this, soline.InventoryID);
					EPEmployee employee = PXSelectReadonly<EPEmployee, Where<EPEmployee.defContactID, Equal<Required<SOOrder.ownerID>>>>.SelectWindowed(this, 0, 1, order.OwnerID);
					if (employee == null)
						throw new PXException(Messages.SalesOrderLineHasAlreadyBeenCompleted, soline.OrderType, soline.OrderNbr, item.InventoryCD);
					throw new PXException(Messages.SalesOrderLineHasAlreadyBeenCompletedContactForDetails,
						soline.OrderType, soline.OrderNbr, item.InventoryCD, employee.AcctName);
				}

				PopulateDropshipFields(order);

				SOLineSplit sosplit = PXCache<SOLineSplit>.CreateCopy(result[i]);
				SOLine4 solineCopy = PXCache<SOLine4>.CreateCopy(soline);

				bool samePOReceiptUOM = line.UOM == solineCopy.UOM;

				//distributing received qty. in situation when multiple SOLineSplits linked with one POLine
				if (baseReceivedQtyUndistributed == null)
				{
					baseReceivedQtyUndistributed = line.BaseReceiptQty;
					receivedQtyUndistributed = samePOReceiptUOM ? line.ReceiptQty : INUnitAttribute.ConvertFromBase<SOLine4.inventoryID, SOLine4.uOM>(solineselect.Cache, solineCopy, baseReceivedQtyUndistributed ?? 0m, INPrecision.QUANTITY);
				}

				if (i == result.Length - 1 || baseReceivedQtyUndistributed < sosplit.BaseOpenQty)
				{
					baseSplitReceivedQty = baseReceivedQtyUndistributed;
					splitReceivedQty = receivedQtyUndistributed;

					baseReceivedQtyUndistributed = 0m;
					receivedQtyUndistributed = 0m;
				}
				else if (baseReceivedQtyUndistributed >= sosplit.BaseOpenQty)
				{
					baseSplitReceivedQty = sosplit.BaseOpenQty;
					splitReceivedQty = sosplit.OpenQty;

					baseReceivedQtyUndistributed -= baseSplitReceivedQty;
					receivedQtyUndistributed -= INUnitAttribute.ConvertFromBase<SOLine4.inventoryID, SOLine4.uOM>(solineselect.Cache, solineCopy, baseSplitReceivedQty ?? 0m, INPrecision.QUANTITY);
				}

					solineCopy.BaseShippedQty += solineCopy.LineSign * baseSplitReceivedQty;
					if (samePOReceiptUOM)
						solineCopy.ShippedQty += solineCopy.LineSign * splitReceivedQty;
					else
						PXDBQuantityAttribute.CalcBaseQty<SOLine4.baseShippedQty>(solineselect.Cache, solineCopy);

					if (poLine.Completed == true)
					{
						//Updating UnbilledQty on original SOLine with unreceived/overreceived quantity
						bool samePOOrderUOM = solineCopy.UOM == poLine.UOM;
						decimal? baseOrderedToReceivedQtyDifference = ((sosplit.BaseQty ?? 0m) - (sosplit.BaseShippedQty ?? 0m)) - (baseSplitReceivedQty ?? 0m);
						decimal? orderedToReceivedQtyDifference = ((sosplit.Qty ?? 0m) - (sosplit.ShippedQty ?? 0m)) - (splitReceivedQty ?? 0m);
						solineCopy.BaseUnbilledQty -= solineCopy.LineSign * baseOrderedToReceivedQtyDifference;
						if (samePOOrderUOM)
							solineCopy.UnbilledQty -= solineCopy.LineSign * orderedToReceivedQtyDifference;
						else
							PXDBQuantityAttribute.CalcTranQty<SOLine4.unbilledQty>(solineselect.Cache, solineCopy);

						//revert back for formulas
						solineCopy.ShippedQty = soline.ShippedQty;
						solineCopy.BaseOpenQty -= solineCopy.LineSign * baseSplitReceivedQty;
						if (samePOReceiptUOM)
							solineCopy.OpenQty -= solineCopy.LineSign * splitReceivedQty;
						else
							PXDBQuantityAttribute.CalcTranQty<SOLine4.openQty>(solineselect.Cache, solineCopy);

					bool closeLine = (solineCopy.OpenLine == true)
						 && PXParentAttribute.SelectSiblings(solinesplitselect.Cache, sosplit, typeof(SOLine))
						.Cast<SOLineSplit>()
						.Where(s => s.SplitLineNbr != sosplit.SplitLineNbr)
						.All(s => s.Completed == true);

					if (closeLine)
					{
						solineCopy.OpenQty = 0m;
						solineCopy.CuryOpenAmt = 0m;
						solineCopy.Completed = true;
						solineCopy.OpenLine = false;
					}
					solineselect.Update(solineCopy);

					sosplit.BaseShippedQty += baseSplitReceivedQty;
					if (samePOReceiptUOM)
						sosplit.ShippedQty += splitReceivedQty;
					else
						PXDBQuantityAttribute.CalcTranQty<SOLineSplit.shippedQty>(solinesplitselect.Cache, sosplit);

					sosplit.Completed = true;
					sosplit.POReceiptType = line.ReceiptType;
					sosplit.POReceiptNbr = line.ReceiptNbr;
					sosplit.POCompleted = true;
					sosplit.PlanID = null;

					solinesplitselect.Update(sosplit);

					if (order != null)
					{
						if (closeLine)
						{
							order.OpenLineCntr--;
						}

						if (order.Approved != true)
						{
							var ownerName = soorderselect.Cache.GetValueExt<SOOrder.ownerID>(order);

							throw new PXException(Messages.SalesOrderRelatedToDropShipReceiptIsNotApproved,
								order.OrderType, order.OrderNbr, ownerName);
						}

						decimal? actualDiscUnitPrice = GetActualDiscUnitPrice(solineCopy);
						decimal? lineAmt = currencyInfo.RoundBase((splitReceivedQty * actualDiscUnitPrice) ?? 0m);
						var oshipment = CreateUpdateOrderShipment(order, line, poAddress, true, baseSplitReceivedQty, lineAmt);

						if (order.OpenShipmentCntr == 0 && order.OpenLineCntr == 0)
						{
							order.MarkCompleted();
						}
					}

					foreach (INItemPlan soplan in PXSelectJoin<INItemPlan,
						InnerJoin<INPlanType,
							On<INItemPlan.FK.PlanType>>,
						Where<INItemPlan.supplyPlanID, Equal<Required<INItemPlan.supplyPlanID>>,
							And<INPlanType.isDemand, Equal<boolTrue>,
							And<INPlanType.isFixed, Equal<boolTrue>>>>>.Select(this, poLine.PlanID))
					{
						this.Caches[typeof(INItemPlan)].Delete(soplan);
					}
				}
				else
				{
					solineselect.Update(solineCopy);

					sosplit.BaseShippedQty += baseSplitReceivedQty;
					if (samePOReceiptUOM)
						sosplit.ShippedQty += splitReceivedQty;
					else
						PXDBQuantityAttribute.CalcTranQty<SOLineSplit.shippedQty>(solinesplitselect.Cache, sosplit);

					sosplit.POReceiptType = line.ReceiptType;
					sosplit.POReceiptNbr = line.ReceiptNbr;

					solinesplitselect.Update(sosplit);

					//Even if the poline is partially complete, the receipt will still be available for Invoicing - thus ShipmentCntr should be increased.
					if (order != null)
					{
						decimal? actualDiscUnitPrice = GetActualDiscUnitPrice(solineCopy);
						decimal? lineAmt = currencyInfo.RoundBase((splitReceivedQty * actualDiscUnitPrice) ?? 0m);
						var oshipment = CreateUpdateOrderShipment(order, line, null, true, baseSplitReceivedQty, lineAmt);
					}
				}
			}
		}

		public virtual void PopulateDropshipFields(SOOrder order)
		{
			if (Document.Current.DropshipFieldsSet == true)
			{
				if (Document.Current.DropshipCustomerID != order.CustomerID)
					Document.Current.DropshipCustomerID = null;
				if (Document.Current.DropshipCustomerLocationID != order.CustomerLocationID)
					Document.Current.DropshipCustomerLocationID = null;
				if (Document.Current.DropshipCustomerOrderNbr != order.CustomerOrderNbr)
					Document.Current.DropshipCustomerOrderNbr = null;
				if (Document.Current.DropshipShipVia != order.ShipVia)
					Document.Current.DropshipShipVia = null;
			}
			else
			{
				Document.Current.DropshipCustomerID = order.CustomerID;
				Document.Current.DropshipCustomerLocationID = order.CustomerLocationID;
				Document.Current.DropshipCustomerOrderNbr = order.CustomerOrderNbr;
				Document.Current.DropshipShipVia = order.ShipVia;
				Document.Current.DropshipFieldsSet = true;
			}
			Document.UpdateCurrent();
		}

		public virtual void CollectDemandForTransferOrders(POReceiptLine line,
			List<INItemPlan> posupply, HashSet<PXResult<INItemPlan, INPlanType>> podemand)
		{
			if (line.OrigTranType != INTranType.Transfer || string.IsNullOrEmpty(line.OrigRefNbr))
			{
				return;
			}
			//all demand will be attached by one plan
			INItemPlan intransit = PXSelectJoin<INItemPlan,
				InnerJoin<INTransitLine,
					On<INTransitLine.noteID, Equal<INItemPlan.refNoteID>>>,
				Where<INTransitLine.transferNbr, Equal<Required<INTransitLine.transferNbr>>,
				And<INTransitLine.transferLineNbr, Equal<Required<INTransitLine.transferLineNbr>>>>>.Select(this, line.OrigRefNbr, line.OrigLineNbr);

			decimal? SupplyQty = line.BaseReceiptQty;

			//TODO: revise constraint And<INPlanType.isDemand, Equal<boolTrue>, And<INPlanType.isFixed, Equal<boolTrue>>>
			foreach (INItemPlan demand in PXSelect<INItemPlan, Where<INItemPlan.supplyPlanID, Equal<Required<INItemPlan.supplyPlanID>>>>.Select(this, intransit.PlanID))
			{
				var planType = INPlanType.PK.Find(this, demand.PlanType);
				var supply = new INItemPlan
				{
					InventoryID = line.InventoryID,
					SiteID = line.SiteID,
					SubItemID = line.SubItemID,
					DemandPlanID = demand.PlanID,
					RefNoteID = demand.RefNoteID,
					RefEntityType = demand.RefEntityType,
					PlanDate = demand.PlanDate,
					PlanType = INPlanConstants.Plan64,
					BAccountID = demand.BAccountID,
					Hold = false,
					CostCenterID = demand.CostCenterID,
				};

				if (SupplyQty >= demand.PlanQty)
				{
					supply.PlanQty = demand.PlanQty;

					posupply.Add(supply);

					SupplyQty -= demand.PlanQty;
					podemand.Add(new PXResult<INItemPlan, INPlanType>(demand, planType));
				}
				else if (SupplyQty > 0m)
				{
					supply.PlanQty = SupplyQty;

					posupply.Add(supply);

					SupplyQty = 0m;
				}

				//TODO: revise this criteria for Intransit
				if (line.AllowComplete == true)
				{
					podemand.Add(new PXResult<INItemPlan, INPlanType>(demand, planType));
				}
			}
		}

		public virtual void ReattachDemandPlansToIN(INReceiptEntry docgraph, INTranSplit newsplit,
			List<INItemPlan> posupply, HashSet<PXResult<INItemPlan, INPlanType>> podemand)
		{
			//TODO: revise constraint 
			foreach (INItemPlan demand in podemand)
			{
				demand.SupplyPlanID = newsplit.PlanID;
				docgraph.Caches[typeof(INItemPlan)].MarkUpdated(demand, assertError: true);
			}

			podemand.Clear();

			decimal? UnassignedQty = newsplit.BaseQty;
			foreach (INItemPlan supply in posupply.ToArray())
			{
				if (UnassignedQty <= 0)
				{
					break;
				}

				if (!string.IsNullOrEmpty(newsplit.LotSerialNbr))
				{
					INItemPlan copy = PXCache<INItemPlan>.CreateCopy(supply);
					copy.PlanQty = Math.Min((decimal)supply.PlanQty, (decimal)UnassignedQty);
					copy.LotSerialNbr = newsplit.LotSerialNbr;
					copy.SupplyPlanID = newsplit.PlanID;
					docgraph.Caches[typeof(INItemPlan)].Insert(copy);

					supply.PlanQty -= copy.PlanQty;
					UnassignedQty -= copy.PlanQty;

					if (supply.PlanQty <= 0m)
					{
						posupply.Remove(supply);
					}
				}
				else
				{
					supply.SupplyPlanID = newsplit.PlanID;
					docgraph.Caches[typeof(INItemPlan)].Insert(supply);

					posupply.Remove(supply);
				}
			}
		}

	    protected virtual bool TryProcessReceiptLinkedAllocation(INTran newline, POReceipt receiptDoc, POLineUOpen poLine, HashSet<PXResult<INItemPlan, INPlanType>> podemand, List<INItemPlan> posupply)
	    {
			//Non-Stock Item
			if (newline == null || poLine != null && (poLine.LineType == POLineType.NonStockForSalesOrder || poLine.LineType == POLineType.NonStockForServiceOrder))
			{
				var planselect = new PXSelect<INItemPlan>(this);
				INPlanType nonstockplantype = new INPlanType { ReplanOnEvent = INPlanConstants.Plan60 };

				var planlist = podemand.ToList();
				planlist.AddRange(posupply.ConvertAll<PXResult<INItemPlan, INPlanType>>(_ => new PXResult<INItemPlan, INPlanType>(planselect.Insert(_), nonstockplantype)));

				if (onBeforeSalesOrderProcessPOReceipt != null)
				{
					planlist = onBeforeSalesOrderProcessPOReceipt(this, planlist, receiptDoc.ReceiptType, receiptDoc.ReceiptNbr);
				}

				SOOrderEntry.ProcessPOReceipt(this, planlist, receiptDoc.ReceiptType, receiptDoc.ReceiptNbr);
				return true;
			}

			return false;
	    }

		protected virtual void ReleaseReceiptLine(POReceiptLine line, POLineUOpen poLine, POOrder poOrder)
		{
		}

		protected virtual void ReleaseReturnLine(POReceiptLine line, POLineUOpen poLine, POReceiptLine2 origLine)
		{
			UpdateReceiptReturn(line, origLine.BaseQty);
		}

		public virtual POReceiptLineReturnUpdate UpdateReceiptReturn(POReceiptLine line, decimal? baseOrigQty)
		{
			if (string.IsNullOrEmpty(line.OrigReceiptType) || string.IsNullOrEmpty(line.OrigReceiptNbr) || line.OrigReceiptLineNbr == null)
				return null;

			var row = new POReceiptLineReturnUpdate()
			{
				ReceiptType = line.OrigReceiptType,
				ReceiptNbr = line.OrigReceiptNbr,
				LineNbr = line.OrigReceiptLineNbr,
			};

			row = poReceiptReturnUpdate.Insert(row);
			row.BaseOrigQty = baseOrigQty;
			row.BaseReturnedQty += line.BaseReceiptQty;

			return poReceiptReturnUpdate.Update(row);
		}

		public virtual INReceiptEntry CreateReceiptEntry()
		{
			INReceiptEntry re = PXGraph.CreateInstance<INReceiptEntry>();

			re.Caches[typeof(SiteStatusByCostCenter)] = this.Caches[typeof(SiteStatusByCostCenter)];
			re.Caches[typeof(LocationStatusByCostCenter)] = this.Caches[typeof(LocationStatusByCostCenter)];
			re.Caches[typeof(LotSerialStatusByCostCenter)] = this.Caches[typeof(LotSerialStatusByCostCenter)];
			re.Caches[typeof(SiteLotSerial)] = this.Caches[typeof(SiteLotSerial)];
			re.Caches[typeof(ItemLotSerial)] = this.Caches[typeof(ItemLotSerial)];

			re.Views.Caches.Remove(typeof(SiteStatusByCostCenter));
			re.Views.Caches.Remove(typeof(LocationStatusByCostCenter));
			re.Views.Caches.Remove(typeof(LotSerialStatusByCostCenter));
			re.Views.Caches.Remove(typeof(SiteLotSerial));
			re.Views.Caches.Remove(typeof(ItemLotSerial));

			// allow non-stock inventory for IN Transactions
			re.FieldVerifying.AddHandler<INTran.inventoryID>((sender, e) => e.Cancel = true);

			return re;
		}

		public virtual INIssueEntry CreateIssueEntry()
		{
			INIssueEntry ie = PXGraph.CreateInstance<INIssueEntry>();

			this.Caches[typeof(SiteStatusByCostCenter)] = ie.Caches[typeof(SiteStatusByCostCenter)];
			this.Caches[typeof(LocationStatusByCostCenter)] = ie.Caches[typeof(LocationStatusByCostCenter)];
			this.Caches[typeof(LotSerialStatusByCostCenter)] = ie.Caches[typeof(LotSerialStatusByCostCenter)];
			this.Caches[typeof(SiteLotSerial)] = ie.Caches[typeof(SiteLotSerial)];
			this.Caches[typeof(ItemLotSerial)] = ie.Caches[typeof(ItemLotSerial)];

			this.Views.Caches.Remove(typeof(SiteStatusByCostCenter));
			this.Views.Caches.Remove(typeof(LocationStatusByCostCenter));
			this.Views.Caches.Remove(typeof(LotSerialStatusByCostCenter));
			this.Views.Caches.Remove(typeof(SiteLotSerial));
			this.Views.Caches.Remove(typeof(ItemLotSerial));

			ie.FieldDefaulting.AddHandler<SiteStatusByCostCenter.negAvailQty>((sender, e) =>
			{
				INItemClass itemclass = INItemClass.PK.Find(sender.Graph, ((SiteStatusByCostCenter)e.Row)?.ItemClassID);
				e.NewValue = itemclass?.NegQty == true;
				e.Cancel = true;
			});

			// allow non-stock inventory for IN Transactions
			ie.FieldVerifying.AddHandler<INTran.inventoryID>((sender, e) => e.Cancel = true);

			return ie;
		}

		public virtual APInvoiceEntry CreateAPInvoiceEntry()
		{
			return PXGraph.CreateInstance<APInvoiceEntry>();
		}

		protected virtual void VerifyOrigINReceipt(POReceiptLine line, POReceiptLineSplit split, POReceiptLine origLine)
		{
			if (string.IsNullOrEmpty(line.OrigReceiptType)
				|| string.IsNullOrEmpty(line.OrigReceiptNbr)
				|| line.OrigReceiptLineNbr == null
				|| POLineType.IsDropShip(line.OrigReceiptLineType)
				|| origLine.INReleased == true
				|| (POLineType.IsProjectDropShip(line.LineType) && line.DropshipExpenseRecording == DropshipExpenseRecordingOption.OnBillRelease))
				return;

			INTran origINTran = SelectFrom<INTran>
						.Where<INTran.docType.IsEqual<INDocType.receipt>
						.And<INTran.pOReceiptType.IsEqual<@P.AsString.ASCII>>
						.And<INTran.pOReceiptNbr.IsEqual<@P.AsString>>
						.And<INTran.pOReceiptLineNbr.IsEqual<@P.AsInt>>>.View.ReadOnly.SelectWindowed(this, 0, 1, origLine.ReceiptType, origLine.ReceiptNbr, origLine.LineNbr);

			bool isStock = POLineType.IsStockNonDropShip(line.LineType) && split.IsStockItem == true;
			bool isNonStock = POLineType.IsNonStockNonServiceNonDropShip(line.LineType);
			bool isProjectDropShipWithIN = POLineType.IsProjectDropShip(line.LineType) && line.DropshipExpenseRecording == DropshipExpenseRecordingOption.OnReceiptRelease;

			if (((isStock || isProjectDropShipWithIN) && origINTran?.Released != true) || ((isNonStock || isProjectDropShipWithIN) && origINTran?.Released == false))
				throw new PXException(Messages.OrigINReceiptMustBeReleased, origINTran?.RefNbr);
		}

		public virtual void ReleaseReturn(INIssueEntry docGraph, APInvoiceEntry invoiceGraph, POReceipt aDoc, DocumentList<INRegister> aCreated, DocumentList<APInvoice> aInvoiceCreated, bool aIsMassProcess)
		{
			var prepareGraph = Func.Memorize(() =>
			{
				if (docGraph == null)
					docGraph = CreateIssueEntry();

				docGraph.insetup.Current.RequireControlTotal = false;
				docGraph.insetup.Current.HoldEntry = false;
				return docGraph;
			});

			var prepareDocument = Func.Memorize(() =>
			{
				prepareGraph();

				INRegister newdoc = new INRegister
				{
					BranchID = aDoc.BranchID,
					DocType = INDocType.Issue,
					SiteID = null,
					TranDate = aDoc.ReceiptDate,
					FinPeriodID = aDoc.FinPeriodID,
					OrigModule = BatchModule.PO,
				};
				newdoc = docGraph.issue.Insert(newdoc);
				return newdoc;
			});

			this.Clear();
			docGraph?.Clear();

			invoiceGraph.Clear();

			invoiceGraph.APSetup.Current.RequireControlTotal = false;
			invoiceGraph.APSetup.Current.RequireControlTaxTotal = false;
			invoiceGraph.APSetup.Current.HoldEntry = false;

			aDoc = ActualizeAndValidatePOReceiptForReleasing(aDoc);

			bool exactCost = aDoc.ReturnInventoryCostMode.IsIn(ReturnCostMode.OriginalCost, ReturnCostMode.ManualCost);
			POReceiptLine prev_line = null;
			INTran newline = null;
			var orderCheckClosed = new HashSet<Tuple<string, string>>();
			bool hasTransactions = false;

			var receiptLineSplits = PXSelectJoin<POReceiptLine,
				LeftJoin<POReceiptLineSplit, On<POReceiptLineSplit.FK.ReceiptLine>,
				LeftJoin<POLineUOpen, On<POLineUOpen.orderType, Equal<POReceiptLine.pOType>, And<POLineUOpen.orderNbr, Equal<POReceiptLine.pONbr>, And<POLineUOpen.lineNbr, Equal<POReceiptLine.pOLineNbr>>>>,
				LeftJoin<INTran, On<INTran.FK.POReceiptLine>,
				LeftJoin<INItemPlan, On<INItemPlan.planID, Equal<POReceiptLineSplit.planID>>,
				LeftJoin<POReceiptLine2,
					On<POReceiptLine2.receiptType, Equal<POReceiptLine.origReceiptType>,
					And<POReceiptLine2.receiptNbr, Equal<POReceiptLine.origReceiptNbr>,
					And<POReceiptLine2.lineNbr, Equal<POReceiptLine.origReceiptLineNbr>>>>>>>>>,
				Where2<POReceiptLine.FK.Receipt.SameAsCurrent,
					And<INTran.refNbr, IsNull>>,
				OrderBy<Asc<POReceiptLine.receiptNbr, Asc<POReceiptLine.lineNbr>>>>
				.Select(this).AsEnumerable()
				.Cast<PXResult<POReceiptLine, POReceiptLineSplit, POLineUOpen, INTran, INItemPlan, POReceiptLine2>>()
				.ToList();

			for (int i = 0; i < receiptLineSplits.Count; i++)
			{
				var res = receiptLineSplits[i];
				POReceiptLine line = (POReceiptLine)res;
				POReceiptLine2 origLine = (POReceiptLine2)res;
				POReceiptLineSplit split = res.GetItem<POReceiptLineSplit>();
				INItemPlan plan = PXCache<INItemPlan>.CreateCopy(res.GetItem<INItemPlan>());
				INPlanType plantype = INPlanType.PK.Find(this, plan.PlanType) ?? new INPlanType();
				POLineUOpen poLine = res.GetItem<POLineUOpen>();
				if (!string.IsNullOrEmpty(poLine?.OrderNbr))
				{
					poLine = (POLineUOpen)this.Caches<POLineUOpen>().Locate(poLine) ?? poLine;
				}

				ValidateReturnLineOnRelease(res);

				InventoryItem item = InventoryItem.PK.Find(this, line.InventoryID);

				bool isStock = POLineType.IsStockNonDropShip(line.LineType) && split.IsStockItem == true;
				bool isNonStock = POLineType.IsNonStockNonServiceNonDropShip(line.LineType);
				if (isStock && (string.IsNullOrEmpty(line.OrigReceiptType) || string.IsNullOrEmpty(line.OrigReceiptNbr) || line.OrigReceiptLineNbr == null))
				{
					if (aDoc.ReturnInventoryCostMode == ReturnCostMode.OriginalCost)
						throw new PXException(Messages.GoodsLineWithoutOrigLinkCantReturnByOrigCost, line.LineNbr);
					if (aDoc.ReturnInventoryCostMode == ReturnCostMode.ManualCost)
						throw new PXException(Messages.GoodsLineWithoutOrigLinkCantReturnByManualCost, line.LineNbr);
				}
				bool postToIN = isStock
					|| isNonStock
					|| POLineType.IsProjectDropShip(line.LineType) && line.DropshipExpenseRecording == DropshipExpenseRecordingOption.OnReceiptRelease && insetup.Current.UpdateGL == true;
				if (postToIN)
				{
					VerifyOrigINReceipt(line, split, origLine);
					prepareDocument();
				}

				bool reattachExistingPlan = false;
				//Splits actually exist only for the Stock Items
				if (!string.IsNullOrEmpty(split.ReceiptNbr))
				{
					if (!string.IsNullOrEmpty(plantype.PlanType) && (bool)plantype.DeleteOnEvent)
					{
						reattachExistingPlan = postToIN;
						if (!reattachExistingPlan)
						{
							Caches[typeof(INItemPlan)].Delete(plan);
						}
						Caches[typeof(POReceiptLineSplit)].MarkUpdated(split, assertError: true);
						split = (POReceiptLineSplit)Caches[typeof(POReceiptLineSplit)].Locate(split);
						if (split != null)
							split.PlanID = null;
						Caches[typeof(POReceiptLineSplit)].IsDirty = true;
					}
					else if (!string.IsNullOrEmpty(plantype.PlanType) && string.IsNullOrEmpty(plantype.ReplanOnEvent) == false)
					{
						plan.PlanType = plantype.ReplanOnEvent;
						Caches[typeof(INItemPlan)].Update(plan);
						Caches[typeof(POReceiptLineSplit)].MarkUpdated(split, assertError: true);
						Caches[typeof(POReceiptLineSplit)].IsDirty = true;
					}
				}
				bool rctLineChange = (Caches[typeof(POReceiptLine)].ObjectsEqual(prev_line, line) == false);
				if (rctLineChange)
				{
					ReleaseReturnLine(line, poLine, origLine);
					UpdateReturnLineOnRelease(res, poLine);
				}
				bool isKit = !split.InventoryID.IsIn(null, line.InventoryID);
				if (rctLineChange || isKit)
				{
					if (postToIN)
					{
						hasTransactions = true;
						newline = new INTran()
						{
							BranchID = line.BranchID,
							TranType = POReceiptType.GetINTranType(aDoc.ReceiptType),
							POReceiptType = line.ReceiptType,
							POReceiptNbr = line.ReceiptNbr,
							POReceiptLineNbr = line.LineNbr,
							POLineType = line.LineType,

							AcctID = line.POAccrualAcctID,
							SubID = line.POAccrualSubID,
							ReclassificationProhibited = true,
							InvtAcctID = isNonStock ? line.ExpenseAcctID : null,
							InvtSubID = isNonStock ? line.ExpenseSubID : null,
							ReasonCode = line.ReasonCode,
							SiteID = line.SiteID,
							InvtMult = line.InvtMult,
							ExpireDate = line.ExpireDate,
							ProjectID = line.ProjectID,
							TaskID = line.TaskID,
							CostCodeID = line.CostCodeID,
							TranDesc = isKit ? null : line.TranDesc,
							UOM = isKit ? split.UOM : line.UOM,
							Qty = isStock ? decimal.Zero : line.Qty,
							IsStockItem = isKit ? null : line.IsStockItem,
							InventoryID = isKit ? split.InventoryID : line.InventoryID,
							SubItemID = isKit ? split.SubItemID : line.SubItemID,
							LocationID = isKit ? split.LocationID : line.LocationID,
							UnitPrice = isKit ? (decimal?)0m : null,
							UnitCost = isKit ? decimal.Zero : (line.UnitCost ?? decimal.Zero),
							IsIntercompany = line.IsIntercompany,
							ExactCost = exactCost,
						};

						UpdateSOOrderLink(newline, poLine, line);
						UpdateINTranForProjectDropShip(newline, line);

						docGraph.CostCenterDispatcherExt?.SetCostLayerType(newline);
						newline = docGraph.LineSplittingExt.lsselect.Insert(newline);
					}
					else
					{
						newline = null;
					}
				}
				prev_line = line;
				if (newline != null && !string.IsNullOrEmpty(split.ReceiptNbr))
				{
					INTranSplit newsplit = (INTranSplit)newline;
					newsplit.SplitLineNbr = null;
					newsplit.InventoryID = split.InventoryID;
					newsplit.SubItemID = split.SubItemID;
					newsplit.LocationID = split.LocationID;
					newsplit.LotSerialNbr = split.LotSerialNbr;
					newsplit.UOM = split.UOM;
					newsplit.Qty = split.Qty;
					newsplit.ExpireDate = split.ExpireDate;
					newsplit.POLineType = poLine.LineType;

					if (reattachExistingPlan)
					{
						newsplit.PlanID = plan.PlanID;
						plan.OrigPlanID = null;
						plan.OrigPlanType = null;
						plan.OrigNoteID = null;
						plan.OrigPlanLevel = null;
						plan.IgnoreOrigPlan = false;
						plan.Reverse = false;
						plan.RefNoteID = docGraph.issue.Current.NoteID;
						plan.RefEntityType = typeof(INRegister).FullName;
						docGraph.Caches[typeof(INItemPlan)].Update(plan);
					}

					newsplit = docGraph.splits.Insert(newsplit);
					docGraph.splits.Cache.RaiseRowUpdated(newsplit, newsplit);
				}
				else if (reattachExistingPlan)
				{
					ThrowPlanNotReattachedError(line, split);
				}
				bool lastLineSplit = (i + 1 >= receiptLineSplits.Count)
					|| !Caches[typeof(POReceiptLine)].ObjectsEqual(line, (POReceiptLine)receiptLineSplits[i + 1]);
				if (lastLineSplit || isKit)
				{
					SetReturnCostFinal(newline, line, item, aDoc.ReturnInventoryCostMode);
				}

				ReduceSOAllocationOnReleaseReturn(res, split, poLine);

			}

			if (hasTransactions)
			{
				INRegister copy = PXCache<INRegister>.CreateCopy(docGraph.issue.Current);
				PXFormulaAttribute.CalcAggregate<INTran.qty>(docGraph.transactions.Cache, copy);
				PXFormulaAttribute.CalcAggregate<INTran.tranAmt>(docGraph.transactions.Cache, copy);
				PXFormulaAttribute.CalcAggregate<INTran.tranCost>(docGraph.transactions.Cache, copy);
				docGraph.issue.Update(copy);
			}
			if ((bool)aDoc.AutoCreateInvoice)
			{
				bool hasRetainedTaxes = (apsetup.Current.RetainTaxes == true) && HasOrderWithRetainage(aDoc);
				invoiceGraph.InvoicePOReceipt(aDoc, aInvoiceCreated, keepOrderTaxes: !hasRetainedTaxes);
			}

			bool isINDocValid = hasTransactions;
			List<APRegister> forReleaseAP = new List<APRegister>();
			using (PXTransactionScope ts = new PXTransactionScope())
			{
				if (isINDocValid) //Skip saving empty document - if receipt contains Non-Stocks only
				{
					docGraph.Save.Press();
				}

				UpdateReturnReleased(isINDocValid ? docGraph.issue.Current : null);

				this.Save.Press();

				if (isINDocValid)
				{
					// the only option to have POReceiptLine.TranCostFinal populated properly is releasing IN Issue
					try
					{
						INDocumentRelease.ReleaseDoc(new List<INRegister>() { docGraph.issue.Current }, false);
					}
					catch (PXException ex)
					{
						throw new PXException(Messages.INDocumentFailedToReleaseDuringPOReceiptReleaseInTransaction, ex.Message);
					}
				}

				if ((bool)aDoc.AutoCreateInvoice)
				{
					invoiceGraph.Save.Press();
					APInvoice apDoc = invoiceGraph.Document.Current;
					if (this.posetup.Current.AutoReleaseAP == true && apDoc.Hold == false)
						forReleaseAP.Add((APRegister)apDoc);
				}

				ts.Complete();
			}

			if (isINDocValid && aCreated.Find(docGraph.issue.Current) == null)
			{
				aCreated.Add(docGraph.issue.Current);
			}

			if (forReleaseAP.Count > 0)
			{
				try
				{
					APDocumentRelease.ReleaseDoc(forReleaseAP, aIsMassProcess);
				}
				catch (PXException ex)
				{
					throw new PXException(Messages.APDocumentFailedToReleaseDuringPOReceiptRelease, ex.Message);
				}
			}
		}

		protected virtual POReceipt ActualizeAndValidatePOReceiptForReleasing(POReceipt aDoc)
		{
			Document.Current = Document.Search<POReceipt.receiptNbr>(aDoc.ReceiptNbr, aDoc.ReceiptType);
			if (PX.SM.WorkflowAction.HasWorkflowActionEnabled(this, g => g.release, Document.Current) == false)
			{
				throw new PXInvalidOperationException(Messages.ActionNotAvailableInCurrentState,
					release.GetCaption(), Document.Cache.GetRowDescription(Document.Current));
			}
			return Document.Current;
		}

		protected virtual POReceipt UpdateReceiptReleased(INRegister inRegister)
		{
			POReceipt receiptCopy = Document.Current;

			POReceipt.Events
				.Select(ev => ev.InventoryReceiptCreated)
				.FireOn(this, receiptCopy);

			receiptCopy.InvtDocType = inRegister?.DocType;
			receiptCopy.InvtRefNbr = inRegister?.RefNbr;

			decimal? unbilledQty = 0m;
			foreach (POReceiptLine poline in transactions.Select())
			{
				unbilledQty += poline.UnbilledQty;
				poline.Released = true;
				Caches[typeof(POReceiptLine)].MarkUpdated(poline, assertError: true);
			}

			receiptCopy.UnbilledQty = unbilledQty;
			receiptCopy = this.Document.Update(receiptCopy);
			return receiptCopy;
		}

		protected virtual POReceipt UpdateReturnReleased(INRegister inRegister)
		{
			POReceipt receiptCopy = Document.Current;

			POReceipt.Events
				.Select(ev => ev.InventoryIssueCreated)
				.FireOn(this, receiptCopy);

			receiptCopy.InvtDocType = inRegister?.DocType;
			receiptCopy.InvtRefNbr = inRegister?.RefNbr;
			receiptCopy = this.Document.Update(receiptCopy);

			foreach (POReceiptLine poline in transactions.Select())
			{
				poline.Released = true;
				Caches[typeof(POReceiptLine)].MarkUpdated(poline, assertError: true);
			}
			return receiptCopy;
		}

		public virtual void SetReturnCostFinal(INTran newline, POReceiptLine rctLine, InventoryItem item, string returnCostMode)
		{
			/// for stock items final cost is set during IN Issue release, <see cref=INReleaseProcess.UpdatePOReceiptCost/>
			if (newline == null || returnCostMode.IsNotIn(ReturnCostMode.OriginalCost, ReturnCostMode.ManualCost))
			{
				if (POLineType.IsNonStock(rctLine.LineType)
					|| POLineType.IsDropShip(rctLine.LineType)
					|| POLineType.IsProjectDropShip(rctLine.LineType))
				{
					if (newline != null)
					{
						newline.TranCost = rctLine.TranCost;
					}
					rctLine.TranCostFinal = rctLine.TranCost;

					this.Caches[typeof(POReceiptLine)].MarkUpdated(rctLine, assertError: true);
				}
				return;
			}

			CurrencyInfo currencyInfo = MultiCurrencyExt.GetDefaultCurrencyInfo();

			decimal unitCost = rctLine.UnitCost ?? decimal.Zero;
			decimal? tranCost = rctLine.TranCost;
			if (item.ValMethod == INValMethod.FIFO && !POLineType.IsProjectDropShip(rctLine.LineType))
			{
				if (rctLine.POType == POOrderType.DropShip)
				{
					if (returnCostMode == ReturnCostMode.ManualCost)
						throw new PXException(Messages.CannotReturnByManualCostDropShipFIFO);
					throw new PXException(Messages.CannotReturnByOrigCostDropShipFIFO);
				}

				var origTran = PXSelectReadonly2<INTran,
					InnerJoin<INTranCost,
						On<INTranCost.FK.Tran>,
					InnerJoin<INCostStatus, On<INTranCost.costID, Equal<INCostStatus.costID>>>>,
					Where<INTran.docType, Equal<INDocType.receipt>,
						And<INTran.pOReceiptType, Equal<Required<INTran.pOReceiptType>>,
						And<INTran.pOReceiptNbr, Equal<Required<INTran.pOReceiptNbr>>,
						And<INTran.pOReceiptLineNbr, Equal<Required<INTran.pOReceiptLineNbr>>>>>>>
					.Select(this, rctLine.OrigReceiptType, rctLine.OrigReceiptNbr, rctLine.OrigReceiptLineNbr);
				if (origTran == null || origTran.Count != 1)
				{
					throw new PXException(AP.Messages.CannotFindINReceipt, rctLine.OrigReceiptNbr);
				}
				var inTran = (PXResult<INTran, INTranCost, INCostStatus>)origTran.First();
				newline.OrigRefNbr = ((INTran)inTran).RefNbr;

				if (returnCostMode != ReturnCostMode.ManualCost)
				{
					var costLayer = (INCostStatus)inTran;
					decimal? baseUnitCost = (costLayer.QtyOnHand <= 0m) ? costLayer.TotalCost
						: costLayer.TotalCost / costLayer.QtyOnHand;
					unitCost = INUnitAttribute.ConvertToBase<POReceiptLine.inventoryID>(transactions.Cache, rctLine,
						rctLine.UOM, baseUnitCost ?? 0m, INPrecision.UNITCOST);
					tranCost = (costLayer.QtyOnHand <= rctLine.BaseReceiptQty) ? costLayer.TotalCost
						: currencyInfo.RoundBase((costLayer.TotalCost * rctLine.BaseReceiptQty / costLayer.QtyOnHand) ?? 0m);
				}
			}
			newline.UnitCost = unitCost;
			newline.TranCost = tranCost;
			rctLine.TranCostFinal = (rctLine.TranCostFinal ?? 0m) + tranCost;

			this.Caches[typeof(POReceiptLine)].MarkUpdated(rctLine, assertError: true);
		}

		protected virtual void UpdateReturnOrdersMarkedForDropship(POReceiptLine line, POLineUOpen poLine)
		{
		}

		public virtual decimal? GetActualDiscUnitPrice(SOLine4 soLine)
			=> (soLine.OrderQty == 0m)
				? soLine.UnitPrice * (1m - soLine.DiscPct / 100m)
				: soLine.LineAmt / soLine.OrderQty;

		public virtual SOOrderShipment CreateUpdateOrderShipment(
			SOOrder order, POReceiptLine line, POAddress poAddress,
			bool confirmed, decimal? qty, decimal? amt)
		{
			var oshipment = SOOrderShipment.FromDropshipPOReceipt(Document.Current, order, line);

			if (poAddress?.IsDefaultAddress == false)
			{
				//Ship-To address was changed in PO order and should be copied to SOOrderShipment.
				SOAddress address = new SOAddress
				{
					CustomerID = poAddress.BAccountID,
					CustomerAddressID = poAddress.BAccountAddressID,
					RevisionID = poAddress.RevisionID,
					IsDefaultAddress = poAddress.IsDefaultAddress,
					AddressLine1 = poAddress.AddressLine1,
					AddressLine2 = poAddress.AddressLine2,
					AddressLine3 = poAddress.AddressLine3,
					City = poAddress.City,
					CountryID = poAddress.CountryID,
					State = poAddress.State,
					PostalCode = poAddress.PostalCode
				};

				address = soaddressselect.Insert(address);
				oshipment.ShipAddressID = address.AddressID;
			}

			oshipment.Confirmed = confirmed;
			oshipment.ShipmentQty = qty;
			oshipment.LineTotal = amt;

			SOOrderShipment existing = PXSelect<SOOrderShipment,
				Where<SOOrderShipment.shipmentType, Equal<SOShipmentType.dropShip>,
					And<SOOrderShipment.shipmentNbr, Equal<Required<SOOrderShipment.shipmentNbr>>,
					And<SOOrderShipment.orderType, Equal<Required<SOOrderShipment.orderType>>,
					And<SOOrderShipment.orderNbr, Equal<Required<SOOrderShipment.orderNbr>>>>>>>
				.Select(this, oshipment.ShipmentNbr, oshipment.OrderType, oshipment.OrderNbr);
			if (existing == null)
			{
				order.ShipmentCntr++;
				return ordershipmentselect.Insert(oshipment);
			}
			else
			{
				if (confirmed && existing.Confirmed == false)
				{
					order.ShipmentCntr++;
					existing.ShipmentQty = 0m;
					existing.LineTotal = 0m;
				}
				existing.Confirmed = confirmed;
				existing.ShipmentQty += qty;
				existing.LineTotal += amt;
				ordershipmentselect.Cache.MarkUpdated(existing, assertError: true);
				return existing;
			}
		}

		protected virtual void UpdateINTranForProjectDropShip(INTran newline, POReceiptLine line)
		{
			if (!POLineType.IsProjectDropShip(line.LineType))
				return;
			
			newline.InvtMult = (short)0;
			newline.AcctID = line.POAccrualAcctID;
			newline.SubID = line.POAccrualSubID;
			newline.COGSAcctID = line.ExpenseAcctID;
			newline.COGSSubID = line.ExpenseSubID;
			newline.InvtAcctID = null;
			newline.InvtSubID = null;
		}

		protected virtual void ThrowPlanNotReattachedError(POReceiptLine line, POReceiptLineSplit split)
			=> throw new PXInvalidOperationException();

		#endregion

		#region Internal variables
		private bool inventoryIDChanging = false;
		private bool _skipUIUpdate = false;
		private bool _forceAccrualAcctDefaulting = false;
		#endregion

		internal bool SkipUIUpdate => _skipUIUpdate;

		#region Internal Member Definitions
		[Serializable]
		public partial class POOrderFilter : IBqlTable
		{
			#region VendorID
			public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }

			protected Int32? _VendorID;
			[Vendor(
				typeof(Search<BAccountR.bAccountID, Where<True, Equal<True>>>), // TODO: remove fake Where after AC-101187
				Visibility = PXUIVisibility.SelectorVisible,
				CacheGlobal = true,
				Filterable = true)]
			[VerndorNonEmployeeOrOrganizationRestrictor(typeof(POReceipt.receiptType))]
			[PXRestrictor(
				typeof(Where<Vendor.vStatus, IsNull,
					Or<Vendor.vStatus, In3<VendorStatus.active, VendorStatus.oneTime, VendorStatus.holdPayments>>>),
				AP.Messages.VendorIsInStatus, typeof(Vendor.vStatus))]
			[PXDefault(typeof(POReceipt.vendorID))]
			public virtual Int32? VendorID
			{
				get
				{
					return this._VendorID;
				}
				set
				{
					this._VendorID = value;
				}
			}
			#endregion
			#region VendorLocationID
			public abstract class vendorLocationID : PX.Data.BQL.BqlInt.Field<vendorLocationID> { }
			protected Int32? _VendorLocationID;
			[PXDefault(typeof(POReceipt.vendorLocationID))]
			[LocationID(typeof(Where<Location.bAccountID, Equal<Current<POOrderFilter.vendorID>>>), DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible)]			
			public virtual Int32? VendorLocationID
			{
				get
				{
					return this._VendorLocationID;
				}
				set
				{
					this._VendorLocationID = value;
				}
			}
			#endregion
			#region ReceiptType
			public abstract class receiptType : IBqlField
			{
			}
			[PXString(2, IsFixed = true)]
			public virtual string ReceiptType
			{
				get;
				set;
			}
			#endregion
			#region InventoryID
			public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
			protected Int32? _InventoryID;
			[POReceiptLineInventory(typeof(receiptType))]
			[PXDefault()]
			public virtual Int32? InventoryID
			{
				get
				{
					return this._InventoryID;
				}
				set
				{
					this._InventoryID = value;
				}
			}
			#endregion
			#region SubItemID
			public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
			protected Int32? _SubItemID;
			[SubItem(typeof(POOrderFilter.inventoryID))]
			[PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
				Where<InventoryItem.inventoryID, Equal<Current2<POOrderFilter.inventoryID>>,
				And<InventoryItem.defaultSubItemOnEntry, Equal<boolTrue>>>>),
				PersistingCheck = PXPersistingCheck.Nothing)]
			[PXFormula(typeof(Default<POOrderFilter.inventoryID>))]
			public virtual Int32? SubItemID
			{
				get
				{
					return this._SubItemID;
				}
				set
				{
					this._SubItemID = value;
				}
			}
			#endregion
			#region OrderType
			public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
			protected String _OrderType;
			[PXDBString(2, IsFixed = true)]
			[PXDefault(POOrderType.RegularOrder)]
			[POOrderType.RegularDropShipList()]
			[PXUIField(DisplayName = "Type")]
			public virtual String OrderType
			{
				get
				{
					return this._OrderType;
				}
				set
				{
					this._OrderType = value;
				}
			}
			#endregion
			#region OrderNbr
			public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
			protected String _OrderNbr;
			[PXDBString(15, IsUnicode = true, InputMask = "")]
			[PXDefault()]
			[PXUIField(DisplayName = "Order Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
			[PO.RefNbr(
				typeof(Search2<POOrderS.orderNbr,
					LeftJoin<Vendor, On<POOrderS.vendorID, Equal<Vendor.bAccountID>>>,
					Where<POOrderS.orderType, Equal<Optional<POOrderFilter.orderType>>,
						And<POOrderS.hold, Equal<boolFalse>,
						And2<Where<POOrderS.orderType, NotEqual<POOrderType.projectDropShip>,
							Or<POOrderS.dropshipReceiptProcessing, Equal<DropshipReceiptProcessingOption.generateReceipt>>>,
						And2<Where<Current<POReceipt.vendorID>, IsNull,
							Or<POOrderS.vendorID, Equal<Current<POReceipt.vendorID>>>>,
						And2<Where<Current<POReceipt.vendorLocationID>, IsNull,
							Or<POOrderS.vendorLocationID, Equal<Current<POReceipt.vendorLocationID>>>>,
						And2<Where<POOrderS.shipToBAccountID, Equal<Current<POOrderFilter.shipToBAccountID>>,
							Or<Current<POOrderFilter.shipToBAccountID>, IsNull>>,
						And2<Where<POOrderS.shipToLocationID, Equal<Current<POOrderFilter.shipToLocationID>>,
							Or<Current<POOrderFilter.shipToLocationID>, IsNull>>,
						And2<Where<Current<APSetup.requireSingleProjectPerDocument>, Equal<boolFalse>,
							Or<POOrderS.projectID, Equal<Current<POReceipt.projectID>>>>,
						And<POOrderS.cancelled, Equal<boolFalse>,
						And<POOrderS.status, Equal<POOrderStatus.open>>>>>>>>>>>>), 
				Filterable = true)]
			public virtual String OrderNbr
			{
				get
				{
					return this._OrderNbr;
				}
				set
				{
					this._OrderNbr = value;
				}
			}
			#endregion
            #region SOOrderNbr
            public abstract class sOOrderNbr : PX.Data.BQL.BqlString.Field<sOOrderNbr> { }
            protected String _SOOrderNbr;
            [PXDBString(15, IsUnicode = true)]
            [PXUIField(DisplayName = "Transfer Nbr.", Visible = false)]
            [PXSelector(typeof(Search<SOOrder.orderNbr, Where<SOOrder.orderType, Equal<SOOrderTypeConstants.transferOrder>>>))]
            public virtual String SOOrderNbr
            {
                get
                {
                    return this._SOOrderNbr;
                }
                set
                {
                    this._SOOrderNbr = value;
                }
            }
            #endregion
			#region ShipToBAccountID
			public abstract class shipToBAccountID : PX.Data.BQL.BqlInt.Field<shipToBAccountID> { }
			protected Int32? _ShipToBAccountID;
			[PXDBInt()]
			[PXSelector(typeof(Search2<BAccount2.bAccountID,
			LeftJoin<AR.Customer, On<AR.Customer.bAccountID, Equal<BAccount2.bAccountID>>>,
			Where<Optional<POOrderFilter.orderType>, Equal<POOrderType.regularOrder>,
			Or<Where<AR.Customer.bAccountID, IsNotNull, And<Optional<POOrderFilter.orderType>, Equal<POOrderType.dropShip>>>>>>),
				typeof(BAccount.acctCD), typeof(BAccount.acctName), typeof(BAccount.type), typeof(BAccount.acctReferenceNbr), typeof(BAccount.parentBAccountID),
			SubstituteKey = typeof(BAccount.acctCD), DescriptionField = typeof(BAccount.acctName))]
			[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Ship To")]
			public virtual Int32? ShipToBAccountID
			{
				get
				{
					return this._ShipToBAccountID;
				}
				set
				{
					this._ShipToBAccountID = value;
				}
			}
			#endregion
			#region ShipToLocationID
			public abstract class shipToLocationID : PX.Data.BQL.BqlInt.Field<shipToLocationID> { }
			protected Int32? _ShipToLocationID;
			[LocationID(typeof(Where<Location.bAccountID, Equal<Optional<POOrderFilter.shipToBAccountID>>>), DescriptionField = typeof(Location.descr))]
			[PXDefault(typeof(Search<BAccount2.defLocationID, Where<BAccount2.bAccountID, Equal<Current<POOrderFilter.shipToBAccountID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Shipping Location")]
			public virtual Int32? ShipToLocationID
			{
				get
				{
					return this._ShipToLocationID;
				}
				set
				{
					this._ShipToLocationID = value;
				}
			}
			#endregion
            #region ShipFromSiteID
            public abstract class shipFromSiteID : PX.Data.BQL.BqlInt.Field<shipFromSiteID> { }
            protected Int32? _ShipFromSiteID;
            [Site(DisplayName = "From Warehouse", DescriptionField = typeof(INSite.descr))]
            public virtual Int32? ShipFromSiteID
            {
                get
                {
                    return this._ShipFromSiteID;
                }
                set
                {
                    this._ShipFromSiteID = value;
                }
            }
            #endregion
			#region ResetFilter
			public abstract class resetFilter : PX.Data.BQL.BqlBool.Field<resetFilter> { }
			protected Boolean? _ResetFilter;
			[PXDBBool()]
			[PXDefault(false)]			
			public virtual Boolean? ResetFilter
			{
				get
				{
					return this._ResetFilter;
				}
				set
				{
					this._ResetFilter = value;
				}
			}
			#endregion
			#region hideAddButton
			public abstract class allowAddLine : PX.Data.BQL.BqlBool.Field<allowAddLine> { }
			protected Boolean? _AllowAddLine;
			[PXDBBool()]
			[PXDefault(true)]
			public virtual Boolean? AllowAddLine
			{
				get
				{
					return this._AllowAddLine;
				}
				set
				{
					this._AllowAddLine = value;
				}
			}
			#endregion
		}

        [POLineForReceivingProjection(Persistent = false)]
        [Serializable]
		public partial class POLineS : POLine
		{			
			#region OrderType
			public new abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
			[PXDBString(2, IsKey = true, IsFixed = true)]
			[PXUIField(DisplayName = "Order Type", Visibility = PXUIVisibility.Visible, Visible = false)]
			public override String OrderType
			{
				get
				{
					return this._OrderType;
				}
				set
				{
					this._OrderType = value;
				}
			}
			#endregion
			#region OrderNbr
			public new abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
			
			[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]			
			[PXUIField(DisplayName = "Order Nbr.", Visibility = PXUIVisibility.Invisible, Visible = true)]
			public override String OrderNbr
			{
				get
				{
					return this._OrderNbr;
				}
				set
				{
					this._OrderNbr = value;
				}
			}
			#endregion
			#region LineNbr
			public new abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
			[PXDBInt(IsKey = true)]
			[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]			
			public override Int32? LineNbr
			{
				get
				{
					return this._LineNbr;
				}
				set
				{
					this._LineNbr = value;
				}
			}
			#endregion
			#region VendorID
			public new abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
			[Vendor(
				typeof(Search<BAccountR.bAccountID, Where<True, Equal<True>>>), // TODO: remove fake Where after AC-101187
				DescriptionField = typeof(BAccount.acctName),
				CacheGlobal = true,
				Filterable = true)]
			[VerndorNonEmployeeOrOrganizationRestrictor]

			public override Int32? VendorID
			{
				get
				{
					return this._VendorID;
				}
				set
				{
					this._VendorID = value;
				}
			}
			#endregion
			#region ReceivedQty
			public new abstract class receivedQty : PX.Data.BQL.BqlDecimal.Field<receivedQty> { }
			#endregion
			#region BaseReceivedQty
			public new abstract class baseReceivedQty : PX.Data.BQL.BqlDecimal.Field<baseReceivedQty> { }

			#endregion
			#region CuryInfoID
			public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
			[PXDBLong()]
			public override Int64? CuryInfoID
			{
				get
				{
					return this._CuryInfoID;
				}
				set
				{
					this._CuryInfoID = value;
				}
			}
			#endregion
			#region Selected
			public new abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
			#endregion
			#region PlanID
			public new abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }

			[PXDBLong()]
			public override Int64? PlanID
			{
				get
				{
					return this._PlanID;
				}
				set
				{
					this._PlanID = value;
				}
			}

			#endregion
			#region LineType
			public new abstract class lineType : PX.Data.BQL.BqlString.Field<lineType> { }
			#endregion
			#region Cancelled
			public new abstract class cancelled : PX.Data.BQL.BqlBool.Field<cancelled> { }
			#endregion
			#region Completed
			public new abstract class completed : PX.Data.BQL.BqlBool.Field<completed> { }
			#endregion
			#region OrderQty
			public new abstract class orderQty : PX.Data.BQL.BqlDecimal.Field<orderQty> { }
			#endregion
			#region TaxCategoryID
			public new abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }

			[PXDBString(TaxCategory.taxCategoryID.Length, IsUnicode = true)]
			[PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
            [PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
            [PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
            public override String TaxCategoryID
			{
				get
				{
					return this._TaxCategoryID;
				}
				set
				{
					this._TaxCategoryID = value;
				}
			}
			#endregion
			#region InventoryID
			public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
			//Cross-Item Attributes causes an unwanted actions on FieldUpdating event
			[Inventory(Filterable = true, DisplayName = "Inventory ID")]
			public override Int32? InventoryID
			{
				get
				{
					return this._InventoryID;
				}
				set
				{
					this._InventoryID = value;
				}
			}
			#endregion
			#region SubItemID
			public new abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
			#endregion
			#region PromisedDate
			public new abstract class promisedDate : PX.Data.BQL.BqlDateTime.Field<promisedDate> { }			
			[PXDBDate()]			
			[PXUIField(DisplayName = "Promised Date")]
			public override DateTime? PromisedDate
			{
				get
				{
					return this._PromisedDate;
				}
				set
				{
					this._PromisedDate = value;
				}
			}
			#endregion
			#region CommitmentID
			public new abstract class commitmentID : PX.Data.BQL.BqlGuid.Field<commitmentID> { }
			[PXDBGuid]
			public override Guid? CommitmentID
			{
				get
				{
					return this._CommitmentID;
				}
				set
				{
					this._CommitmentID = value;
				}
			}
			#endregion
		}

		[PXProjection(typeof(Select5<POOrder,
										InnerJoin<POLineS,
											 On<POLineS.orderType, Equal<POOrder.orderType>,
											And<POLineS.orderNbr, Equal<POOrder.orderNbr>,
											And<POLineS.lineType, NotEqual<POLineType.description>,
											And<POLineS.completed, NotEqual<True>,
											And<POLineS.cancelled, NotEqual<True>,
											And2<Where<CurrentValue<POReceiptLineS.inventoryID>, IsNull,
															Or<POLineS.inventoryID, Equal<CurrentValue<POReceiptLineS.inventoryID>>>>,
                                            And<Where<CurrentValue<POReceiptLineS.subItemID>, IsNull,
												Or<POLineS.subItemID, Equal<CurrentValue<POReceiptLineS.subItemID>>>>>>>>>>>>,
											Aggregate
												<GroupBy<POOrder.orderType,
												GroupBy<POOrder.orderNbr,
												GroupBy<POOrder.orderDate,
												GroupBy<POOrder.curyID,
												GroupBy<POOrder.curyOrderTotal,
												GroupBy<POOrder.hold,
												GroupBy<POOrder.status,
												GroupBy<POOrder.cancelled,
												GroupBy<POOrder.isTaxValid,
												GroupBy<POOrder.isUnbilledTaxValid,
												Sum<POLineS.orderQty,
												Sum<POLineS.receivedQty,
												Sum<POLineS.baseReceivedQty>>>>>>>>>>>>>>>), Persistent = false)]
        [Serializable]
		public partial class POOrderS : POOrder
		{
			#region Selected
			public new abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
			#endregion
			#region OrderType
			public new abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
			#endregion
			#region OrderNbr
			public new abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
			#endregion
			#region ReceivedQty
			public abstract class receivedQty : PX.Data.BQL.BqlDecimal.Field<receivedQty> { }
			protected Decimal? _ReceivedQty;
			[PXDBQuantity(HandleEmptyKey = true, BqlField = typeof(POLineS.receivedQty))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Received Qty.", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public virtual Decimal? ReceivedQty
			{
				get
				{
					return this._ReceivedQty;
				}
				set
				{
					this._ReceivedQty = value;
				}
			}
			#endregion
			#region BaseReceivedQty
			public abstract class baseReceivedQty : PX.Data.BQL.BqlDecimal.Field<baseReceivedQty> { }
			protected Decimal? _BaseReceivedQty;
			[PXDBDecimal(6, BqlField = typeof(POLineS.baseReceivedQty))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Base Received Qty.", Visibility = PXUIVisibility.Visible)]
			public virtual Decimal? BaseReceivedQty
			{
				get
				{
					return this._BaseReceivedQty;
				}
				set
				{
					this._BaseReceivedQty = value;
				}
			}
			#endregion
			#region Hold
			public new abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
			#endregion
			#region Cancelled
			public new abstract class cancelled : PX.Data.BQL.BqlBool.Field<cancelled> { }
			#endregion
			#region IsTaxValid
			public new abstract class isTaxValid : PX.Data.BQL.BqlBool.Field<isTaxValid> { }
			#endregion
			#region IsUnbilledTaxValid
			public new abstract class isUnbilledTaxValid : PX.Data.BQL.BqlBool.Field<isUnbilledTaxValid> { }
			#endregion
			#region LeftToReceiveQty
			public abstract class leftToReceiveQty : PX.Data.BQL.BqlDecimal.Field<leftToReceiveQty> { }
			[PXQuantity()]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Open Qty.", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public virtual Decimal? LeftToReceiveQty
			{
                [PXDependsOnFields(typeof(orderQty), typeof(receivedQty))]
				get
				{
					return (this.OrderQty - this.ReceivedQty);
				}
			}
			#endregion
			#region CuryInfoID
			public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
			[PXDBLong()]
			public override Int64? CuryInfoID
			{
				get
				{
					return this._CuryInfoID;
				}
				set
				{
					this._CuryInfoID = value;
				}
			}
			#endregion
		}

        [Serializable]
		public partial class POReceiptLineS : PX.Data.IBqlTable
		{
			#region BarCode
			public abstract class barCode : PX.Data.BQL.BqlString.Field<barCode> { }
			protected String _BarCode;
			[PXDBString(255, IsUnicode = true)]
			[PXUIField(DisplayName = "Barcode")]
			public virtual String BarCode
			{
				get
				{
					return this._BarCode;
				}
				set
				{
					this._BarCode = value;
				}
			}
			#endregion
			#region InventoryID
			public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
			protected Int32? _InventoryID;
			[POReceiptLineInventory(typeof(receiptType))]
			[PXDefault()]
			public virtual Int32? InventoryID
			{
				get
				{
					return this._InventoryID;
				}
				set
				{
					this._InventoryID = value;
				}
			}
			#endregion
			#region VendorID
			public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
			protected Int32? _VendorID;
			[Vendor(
				typeof(Search<BAccountR.bAccountID, Where<True, Equal<True>>>), // TODO: remove fake Where after AC-101187
				CacheGlobal = true,
				Filterable = true)]
			[VerndorNonEmployeeOrOrganizationRestrictor]
			[PXDBDefault(typeof(POReceipt.vendorID))]
			public virtual Int32? VendorID
			{
				get
				{
					return this._VendorID;
				}
				set
				{
					this._VendorID = value;
				}
			}
			#endregion
			#region VendorLocationID
			public abstract class vendorLocationID : PX.Data.BQL.BqlInt.Field<vendorLocationID> { }
			protected Int32? _VendorLocationID;			
			[PXDefault(typeof(Search<BAccountR.defLocationID, Where<BAccountR.bAccountID, Equal<Current<POReceiptLineS.vendorID>>>>))]		
            [LocationID(typeof(Where<Location.bAccountID, Equal<Current<POReceiptLineS.vendorID>>>), DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Vendor Location")]
			[PXFormula(typeof(Default<POReceiptLineS.vendorID>))]
			public virtual Int32? VendorLocationID
			{
				get
				{
					return this._VendorLocationID;
				}
				set
				{
					this._VendorLocationID = value;
				}
			}
			#endregion
            #region ShipFromSiteID
            public abstract class shipFromSiteID : PX.Data.BQL.BqlInt.Field<shipFromSiteID> { }
            protected Int32? _ShipFromSiteID;
            [Site(DisplayName = "From Warehouse", DescriptionField = typeof(INSite.descr))]
            public virtual Int32? ShipFromSiteID
            {
                get
                {
                    return this._ShipFromSiteID;
                }
                set
                {
                    this._ShipFromSiteID = value;
                }
            }
            #endregion
			#region POType
			public abstract class pOType : PX.Data.BQL.BqlString.Field<pOType> { }
			protected String _POType;
			[PXDBString(2, IsFixed = true)]
			[POOrderType.RegularDropShipList()]
			[PXDefault(POOrderType.RegularOrder)]
			[PXUIField(DisplayName = "Order Type")]
			public virtual String POType
			{
				get
				{
					return this._POType;
				}
				set
				{
					this._POType = value;
				}
			}
			#endregion
			#region PONbr
			public abstract class pONbr : PX.Data.BQL.BqlString.Field<pONbr> { }
			protected String _PONbr;
			[PXDBString(15, IsUnicode = true)]
			[PXUIField(DisplayName = "Order Nbr.")]
			[PO.RefNbr(typeof(Search2<POOrder.orderNbr,
				LeftJoinSingleTable<Vendor, On<POOrder.vendorID, Equal<Vendor.bAccountID>,
				And<Match<Vendor, Current<AccessInfo.userName>>>>>,
				Where<POOrder.orderType, Equal<Optional<POReceiptLineS.pOType>>,
                And<Vendor.bAccountID, IsNotNull>>,
				OrderBy<Desc<POOrder.orderNbr>>>), Filterable = true)]
			public virtual String PONbr
			{
				get
				{
					return this._PONbr;
				}
				set
				{
					this._PONbr = value;
				}
			}
			#endregion
			#region POLineNbr
			public abstract class pOLineNbr : PX.Data.BQL.BqlInt.Field<pOLineNbr> { }
			protected Int32? _POLineNbr;
			[PXDBInt()]
			[PXUIField(DisplayName = "Line Nbr.")]
			public virtual Int32? POLineNbr
			{
				get
				{
					return this._POLineNbr;
				}
				set
				{
					this._POLineNbr = value;
				}
			}
			#endregion
			#region POAccrualType
			public abstract class pOAccrualType : PX.Data.BQL.BqlString.Field<pOAccrualType> { }
			[PXDBString(1, IsFixed = true)]
			[POAccrualType.List]
			[PXUIField(DisplayName = "Billing Based On", Enabled = false)]
			public virtual string POAccrualType
			{
				get;
				set;
			}
			#endregion
			#region SOOrderType
			public abstract class sOOrderType : PX.Data.BQL.BqlString.Field<sOOrderType> { }
            protected String _SOOrderType;
            [PXDBString(15, IsUnicode = true)]
            [PXUIField(DisplayName = "Transfer Type", Enabled = false)]
            [PXSelector(typeof(Search<SOOrder.orderType>))]
            public virtual String SOOrderType
            {
                get
                {
                    return this._SOOrderType;
                }
                set
                {
                    this._SOOrderType = value;
                }
            }
            #endregion
            #region SOOrderNbr
            public abstract class sOOrderNbr : PX.Data.BQL.BqlString.Field<sOOrderNbr> { }
            protected String _SOOrderNbr;
            [PXDBString(15, IsUnicode = true)]
            [PXUIField(DisplayName = "Transfer Nbr.", Enabled = false)]
            [PXSelector(typeof(Search<SOOrder.orderNbr>))]
            public virtual String SOOrderNbr
            {
                get
                {
                    return this._SOOrderNbr;
                }
                set
                {
                    this._SOOrderNbr = value;
                }
            }
            #endregion
            #region SOOrderLineNbr
            public abstract class sOOrderLineNbr : PX.Data.BQL.BqlInt.Field<sOOrderLineNbr> { }
            protected Int32? _SOOrderLineNbr;
            [PXDBInt()]
            [PXUIField(DisplayName = "Line Nbr.", Enabled = false)]
            public virtual Int32? SOOrderLineNbr
            {
                get
                {
                    return this._SOOrderLineNbr;
                }
                set
                {
                    this._SOOrderLineNbr = value;
                }
            }
            #endregion
            #region SOShipmentNbr
            public abstract class sOShipmentNbr : PX.Data.BQL.BqlString.Field<sOShipmentNbr> { }
            protected String _SOShipmentNbr;
            [PXDBString(15, IsUnicode = true)]
            [PXUIField(DisplayName = "Shipment Nbr.", Enabled = false)]
            [PXSelector(typeof(Search<SOShipment.shipmentNbr>))]
            public virtual String SOShipmentNbr
            {
                get
                {
                    return this._SOShipmentNbr;
                }
                set
                {
                    this._SOShipmentNbr = value;
                }
            }
            #endregion
            #region OrigRefNbr
            public abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr> { }
            protected String _OrigRefNbr;
            [PXDBString(15, IsUnicode = true)]
            public virtual String OrigRefNbr
            {
                get
                {
                    return this._OrigRefNbr;
                }
                set
                {
                    this._OrigRefNbr = value;
                }
            }
            #endregion
            #region OrigLineNbr
            public abstract class origLineNbr : PX.Data.BQL.BqlInt.Field<origLineNbr> { }
            protected Int32? _OrigLineNbr;
            [PXDBInt()]
            public virtual Int32? OrigLineNbr
            {
                get
                {
                    return this._OrigLineNbr;
                }
                set
                {
                    this._OrigLineNbr = value;
                }
            }
            #endregion
			#region TranType
			public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }

			public string TranType
			{
				get
				{
					return POReceiptType.GetINTranType(POReceiptType.POReceipt);
				}
			}
			#endregion
			#region InvtMult
			public abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }
			protected Int16? _InvtMult;
			[PXDBShort()]
			[PXDefault()]
			public virtual Int16? InvtMult
			{
				get
				{
					return this._InvtMult;
				}
				set
				{
					this._InvtMult = value;
				}
			}
			#endregion
			#region SubItemID
			public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
			protected Int32? _SubItemID;
			[SubItem(typeof(POReceiptLineS.inventoryID))]			
			public virtual Int32? SubItemID
			{
				get
				{
					return this._SubItemID;
				}
				set
				{
					this._SubItemID = value;
				}
			}
			#endregion
			#region UOM
			public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
			protected String _UOM;

			[PXDefault(typeof(Search<InventoryItem.purchaseUnit, Where<InventoryItem.inventoryID, Equal<Current<POReceiptLineS.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
			[INUnit(typeof(POReceiptLineS.inventoryID))]
			public virtual String UOM
			{
				get
				{
					return this._UOM;
				}
				set
				{
					this._UOM = value;
				}
			}
			#endregion
			#region SiteID
			public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
			protected Int32? _SiteID;
			[IN.POSiteAvail(typeof(POReceiptLineS.inventoryID), typeof(POReceiptLineS.subItemID), typeof(CostCenter.freeStock))]
			[InterBranchRestrictor(typeof(Where<SameOrganizationBranch<INSite.branchID, Current<POReceipt.branchID>>>))]
			[PXDefault(typeof(Coalesce<
				Search2<LocationBranchSettings.vSiteID,
					InnerJoin<INSite, On<INSite.siteID, Equal<LocationBranchSettings.vSiteID>>>,
					Where<LocationBranchSettings.locationID, Equal<Current2<POReceiptLineS.vendorLocationID>>,
						And<LocationBranchSettings.bAccountID, Equal<Current2<POReceiptLineS.vendorID>>,
						And<LocationBranchSettings.branchID, Equal<Current<AccessInfo.branchID>>,
						And<Where2<FeatureInstalled<FeaturesSet.interBranch>,
							Or<SameOrganizationBranch<INSite.branchID, Current<POReceipt.branchID>>>>>>>>>,
				Search2<CR.Location.vSiteID,
					InnerJoin<INSite, On<INSite.siteID, Equal<CR.Location.vSiteID>>>,
				Where<CR.Location.locationID, Equal<Current2<POReceiptLineS.vendorLocationID>>,
						And<CR.Location.bAccountID, Equal<Current2<POReceiptLineS.vendorID>>,
						And<Where2<FeatureInstalled<FeaturesSet.interBranch>,
							Or<SameOrganizationBranch<INSite.branchID, Current<POReceipt.branchID>>>>>>>>,
				Search2<InventoryItemCurySettings.dfltSiteID,
					InnerJoin<INSite, On<INSite.siteID, Equal<InventoryItemCurySettings.dfltSiteID>>>,
					Where<InventoryItemCurySettings.inventoryID, Equal<Current2<POReceiptLineS.inventoryID>>,
						And<InventoryItemCurySettings.curyID, EqualBaseCuryID<Current2<POReceipt.branchID>>,
						And<Where2<FeatureInstalled<FeaturesSet.interBranch>,
							Or<SameOrganizationBranch<INSite.branchID, Current<POReceipt.branchID>>>>>>>>>))]
			[PXFormula(typeof(Default<POReceiptLineS.vendorLocationID>))]
			[PXFormula(typeof(Default<POReceiptLineS.inventoryID>))]
			public virtual Int32? SiteID
			{
				get
				{
					return this._SiteID;
				}
				set
				{
					this._SiteID = value;
				}
			}
			#endregion
			#region LocationID
			public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
			protected Int32? _LocationID;
			[LocationAvail(typeof(POReceiptLineS.inventoryID), typeof(POReceiptLineS.subItemID), typeof(CostCenter.freeStock), typeof(POReceiptLineS.siteID), typeof(POReceiptLineS.tranType), typeof(POReceiptLineS.invtMult), KeepEntry = false)]
			[PXFormula(typeof(Default<POReceiptLineS.siteID>))]
			[PXFormula(typeof(Default<POReceiptLineS.inventoryID>))]			
			public virtual Int32? LocationID
			{
				get
				{
					return this._LocationID;
				}
				set
				{
					this._LocationID = value;
				}
			}
			#endregion
			#region LotSerialNbr
			public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
			protected String _LotSerialNbr;
			[LotSerialNbr]
			public virtual String LotSerialNbr
			{
				get
				{
					return this._LotSerialNbr;
				}
				set
				{
					this._LotSerialNbr = value;
				}
			}
			#endregion
			#region ExpireDate
			public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
			protected DateTime? _ExpireDate;
			[PXDBDate(InputMask = "d", DisplayMask = "d")]
			[PXUIField(DisplayName = "Expiration Date")]
			public virtual DateTime? ExpireDate
			{
				get
				{
					return this._ExpireDate;
				}
				set
				{
					this._ExpireDate = value;
				}
			}
			#endregion

			#region ReceiptQty
			public abstract class receiptQty : PX.Data.BQL.BqlDecimal.Field<receiptQty> { }
			protected Decimal? _ReceiptQty;

			[PXDBQuantity(typeof(POReceiptLineS.uOM), typeof(POReceiptLineS.baseReceiptQty), HandleEmptyKey = true, MinValue = 0)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Receipt Qty.", Visibility = PXUIVisibility.Visible)]
			public virtual Decimal? ReceiptQty
			{
				get
				{
					return this._ReceiptQty;
				}
				set
				{
					this._ReceiptQty = value;
				}
			}

			public virtual Decimal? Qty
			{
				get
				{
					return this._ReceiptQty;
				}
				set
				{
					this._ReceiptQty = value;
				}
			}
			#endregion
			#region BaseReceiptQty
			public abstract class baseReceiptQty : PX.Data.BQL.BqlDecimal.Field<baseReceiptQty> { }
			protected Decimal? _BaseReceiptQty;

			[PXDBDecimal(6)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public virtual Decimal? BaseReceiptQty
			{
				get
				{
					return this._BaseReceiptQty;
				}
				set
				{
					this._BaseReceiptQty = value;
				}
			}			
			#endregion			
			#region CuryInfoID
			public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
			protected Int64? _CuryInfoID;
			[PXDBLong()]
			[CurrencyInfo(typeof(POReceipt.curyInfoID))]
			public virtual Int64? CuryInfoID
			{
				get
				{
					return this._CuryInfoID;
				}
				set
				{
					this._CuryInfoID = value;
				}
			}
			#endregion
			#region UnitCost
			public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }
			protected Decimal? _UnitCost;

			[PXDBDecimal(typeof(Search<CommonSetup.decPlPrcCst>))]
			[PXUIField(DisplayName = "Unit Cost", Visibility = PXUIVisibility.SelectorVisible)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public virtual Decimal? UnitCost
			{
				get
				{
					return this._UnitCost;
				}
				set
				{
					this._UnitCost = value;
				}
			}
			#endregion
			#region FetchMode
			public abstract class fetchMode : PX.Data.BQL.BqlBool.Field<fetchMode> { }
			protected Boolean? _FetchMode;
			[PXDBBool()]			
			public virtual Boolean? FetchMode
			{
				get
				{
					return this._FetchMode;
				}
				set
				{
					this._FetchMode = value;					
				}
			}
			#endregion
			#region ByOne
			public abstract class byOne : PX.Data.BQL.BqlBool.Field<byOne> { }
			protected Boolean? _ByOne;
			[PXDBBool()]
			[PXUIField(DisplayName = "Add One Unit per Barcode")]
			[PXDefault(typeof(POSetup.receiptByOneBarcodeReceiptBarcode), PersistingCheck = PXPersistingCheck.Nothing)]
			public virtual Boolean? ByOne
			{
				get
				{
					return this._ByOne;
				}
				set
				{
					this._ByOne = value;
				}
			}
			#endregion
			#region AutoAddLine
			public abstract class autoAddLine : PX.Data.BQL.BqlBool.Field<autoAddLine> { }
			protected Boolean? _AutoAddLine;
			[PXDBBool()]
			[PXDefault(typeof(POSetup.autoAddLineReceiptBarcode), PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Add Line Automatically")]
			public virtual Boolean? AutoAddLine
			{
				get
				{
					return this._AutoAddLine;
				}
				set
				{
					this._AutoAddLine = value;
				}
			}
			#endregion
			#region Description
			public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
			protected String _Description;
			[PXDBString(255)]
			[PXUIField(DisplayName = "")]
			public virtual String Description
			{
				get
				{
					return this._Description;
				}
				set
				{
					this._Description = value;
				}
			}
			#endregion

			#region ReceiptType
			public abstract class receiptType : PX.Data.BQL.BqlString.Field<receiptType> { }
			[PXDBString(2, IsFixed = true)]
			public virtual string ReceiptType
			{
				get;
				set;
			}
			#endregion
			#region ReceiptVendorID
			public abstract class receiptVendorID : PX.Data.BQL.BqlInt.Field<receiptVendorID> { }
			[PXDBInt]
			public virtual int? ReceiptVendorID
			{
				get;
				set;
			}
			#endregion
			#region ReceiptVendorLocationID
			public abstract class receiptVendorLocationID : PX.Data.BQL.BqlInt.Field<receiptVendorLocationID> { }
			[PXDBInt]
			public virtual int? ReceiptVendorLocationID
			{
				get;
				set;
			}
			#endregion
		}

		#endregion

		protected virtual void SyncUnassigned() { }

		public class SOOrderShipmentComparer : IEqualityComparer<POReceiptLine>
		{
			public SOOrderShipmentComparer()
			{
			}

			#region IEqualityComparer<POReceiptLine> Members

			public bool Equals(POReceiptLine x, POReceiptLine y)
			{
				return x.SOOrderType == y.SOOrderType && x.SOOrderNbr == y.SOOrderNbr && x.SOShipmentNbr == y.SOShipmentNbr;
			}

			public int GetHashCode(POReceiptLine obj)
			{
				unchecked
				{
					return obj.SOShipmentNbr.GetHashCode() + 37 * obj.SOOrderType.GetHashCode() + 109 * obj.SOOrderNbr.GetHashCode();
				}
			}

			#endregion
		}

		/// <exclude/>
		public class ExtensionSort
			: SortExtensionsBy<ExtensionOrderFor<POReceiptEntry>
				.FilledWith<
					MultiCurrency,
					GraphExtensions.POReceiptEntryExt.UpdatePOOnRelease,
					GraphExtensions.POReceiptEntryExt.AffectedPOOrdersByPOReceipt
				>>
		{ }
	}

	public class PXAlreadyCreatedException : PXException
	{
		public PXAlreadyCreatedException() : base() { }

		public PXAlreadyCreatedException(string format, params object[] args) : base(format, args) { }
		
		public PXAlreadyCreatedException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{

		}
	}
}
