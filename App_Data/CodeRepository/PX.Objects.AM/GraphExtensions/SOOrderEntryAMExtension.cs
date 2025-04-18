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
using PX.Data;
using PX.Objects.SO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.TX;
using PX.Common;
using PX.Objects.CM;
using PX.Objects.AM.Attributes;
using PX.Objects.AM.CacheExtensions;
using PX.Data.WorkflowAPI;
using PX.Objects.Common;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;

namespace PX.Objects.AM.GraphExtensions
{
	using static BoundedTo<SOOrderEntry, SOOrder>;

	public class SOOrderEntryAMExtension : SalesOrderBaseAMExtension<SOOrderEntry, SOOrder,
            Where<AMConfigurationResults.ordTypeRef,
                        Equal<Current<SOLine.orderType>>,
                    And<AMConfigurationResults.ordNbrRef,
                        Equal<Current<SOLine.orderNbr>>,
                    And<AMConfigurationResults.ordLineRef,
                        Equal<Current<SOLine.lineNbr>>>>>>
    {
		public class AMProdItemSplitPlan : AMProdItemSplitPlan<SOOrderEntry>
		{
			public static bool IsActive() => SOOrderEntryAMExtension.IsActive();
		}

		public class WorkflowChanges : PXGraphExtension<SOOrderEntry_Workflow, SOOrderEntry>
        {
            public static bool IsActive() => SOOrderEntryAMExtension.IsActive();

			public sealed override void Configure(PXScreenConfiguration config) =>
				Configure(config.GetScreenConfigurationContext<SOOrderEntry, SOOrder>());
			protected static void Configure(WorkflowContext<SOOrderEntry, SOOrder> context)
			{
				var manufacturingCategory = context.Categories.CreateNew(ActionCategories.ManufacturingCategoryID,
					category => category.DisplayName(ActionCategories.DisplayNames.Manufacturing).
					PlaceAfter(SO.Workflow.SalesOrder.ScreenConfiguration.ActionCategories.ReplenishmentCategoryID));
				#region Conditions
				Condition Bql<T>() where T : IBqlUnary, new() => context.Conditions.FromBql<T>();
				var conditions = new
				{
					HideProductionOrdersActions
						= Bql<SOOrderTypeExt.aMProductionOrderEntry.IsEqual<False>.And<SOOrderTypeExt.aMProductionOrderEntryOnHold.IsEqual<False>>>(),
				}.AutoNameConditions();
				#endregion

				context.UpdateScreenConfigurationFor(screen =>
				{
					return screen
						.WithActions(actions =>
						{
							actions.Add<SOOrderEntryAMExtension>(g => g.CreateProdOrder, a => a.WithCategory(manufacturingCategory)
							.IsHiddenWhen(conditions.HideProductionOrdersActions));
							actions.Add<SOOrderEntryAMExtension>(g => g.CTPButton, a => a.WithCategory(manufacturingCategory)
							.IsHiddenWhen(conditions.HideProductionOrdersActions));
						})
						.WithCategories(categories =>
						{
							categories.Add(manufacturingCategory);
						});
				});
			}

			public static class ActionCategories
            {
                public const string ManufacturingCategoryID = "Manufacturing Category";

                [PXLocalizable]
                public static class DisplayNames
                {
                    public const string Manufacturing = "Manufacturing";
                }

            }
        }


        /// <summary>
        /// Linked to estimates tab --> grid
        /// </summary>
        [PXHidden]
        [PXCopyPasteHiddenView]
        public PXSelectJoin<AMEstimateReference, InnerJoin<AMEstimateItem,
                On<AMEstimateReference.estimateID, Equal<AMEstimateItem.estimateID>,
                    And<AMEstimateReference.revisionID, Equal<AMEstimateItem.revisionID>>>>,
            Where<AMEstimateReference.quoteType, Equal<Current<SOOrder.orderType>>,
                And<AMEstimateReference.quoteNbr, Equal<Current<SOOrder.orderNbr>>,
                    And<AMEstimateItem.quoteSource, Equal<EstimateSource.salesOrder>>>>> OrderEstimateRecords;

        [PXHidden]
        [PXCopyPasteHiddenView]
        public PXSelect<AMEstimateReferenceSO,

            Where<AMEstimateReferenceSO.orderType, Equal<Current<SOOrder.orderType>>,
                And<AMEstimateReferenceSO.orderNbr, Equal<Current<SOOrder.orderNbr>>>>> DocDetailEstimateRecords;

        /// <summary>
        /// Tied to the production order action for listing and creating production orders related to sales order lines
        /// </summary>
        [PXHidden]
        [PXCopyPasteHiddenView]
        public PXSelectJoin<SOLine,
            LeftJoin<AMProdItem,
                 On<SOLineExt.aMOrderType,
                    Equal<AMProdItem.orderType>,
                And<SOLineExt.aMProdOrdID,
                    Equal<AMProdItem.prodOrdID>>>,
            LeftJoin<AMConfigurationResults,
                On<AMConfigurationResults.ordTypeRef,
                    Equal<SOLine.orderType>,
                And<AMConfigurationResults.ordNbrRef,
                    Equal<SOLine.orderNbr>,
                And<AMConfigurationResults.ordLineRef,
                    Equal<SOLine.lineNbr>>>>>>,
            Where<SOLineExt.aMProdCreate, Equal<True>,
                And<SOLine.orderType,
                    Equal<Current<SOOrder.orderType>>,
                    And<SOLine.orderNbr,
                        Equal<Current<SOOrder.orderNbr>>>>>> AMSOLineRecords;

        /// <summary>
        /// Prod. Order Link grid
        /// </summary>
        [PXHidden]
        [PXCopyPasteHiddenView]
        public PXSelect<AMProdItem> AMSOLineLinkRecords;
 
        public PXFilter<LinkProdOrderSelectFilter> linkProdOrderSelectFilter;

        //Required for cache attached label changing
        [PXHidden]
        [PXCopyPasteHiddenView]
        public PXSelect<AMProdItem,
            Where<AMProdItem.orderType,
                Equal<Current<SOLineExt.aMOrderType>>,
                And<AMProdItem.prodOrdID,
                    Equal<Current<SOLineExt.aMProdOrdID>>>>> ProdItemRecords;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelect<AMProdItemSplit,
			Where<AMProdItemSplit.orderType,
				Equal<Current<SOLineExt.aMOrderType>>,
				And<AMProdItemSplit.prodOrdID,
					Equal<Current<SOLineExt.aMProdOrdID>>>>> ProdItemSplitRecords;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelect<AMProdEvnt> ProdEvntRecords;

        /// <summary>
        /// Determines if extension is active
        /// </summary>
        /// <returns></returns>
        public static bool IsActive()
        {
            return !Common.IsPortal && PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        /// <summary>
        /// Are we running the Copy order copy config logic
        /// </summary>
        public bool IsCopyConfig
        {
            get;
            protected set;
        }

        #region Cache Attached

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXParent(typeof(Select<SOOrder, Where<SOOrder.orderType, Equal<Current<AMEstimateReference.quoteType>>,
                And<SOOrder.orderNbr, Equal<Current<AMEstimateReference.quoteNbr>>>>>), LeaveChildren = true)]
        protected virtual void _(Events.CacheAttached<AMEstimateReference.estimateID> e) { }

        [PXDBInt]
        [PXUIField(DisplayName = "Tax Line Nbr.", Visible = false, Enabled = false)]
        [PXLineNbr(typeof(SOOrder.lineCntr))] //consume the standard doc detail line numbers for estimates (shared numbering)
        protected virtual void _(Events.CacheAttached<AMEstimateReference.taxLineNbr> e) { }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [AMSOTax(typeof(SOOrder), typeof(SOTax), typeof(SOTaxTran), TaxCalc = TaxCalc.ManualLineCalc)]
        protected virtual void _(Events.CacheAttached<AMEstimateReference.taxCategoryID> e) { }

        //Using an Append in a table Extension would not work with the taxes. 
        //Confirmed through debugging the code it was not finding a pxparent attribute

        // Do not move this cache attached to an extension
        //   QuoteType and QuoteNbr are correct as these refer to estimates tab (quote references). Document details would be OrderType and OrderNbr references)
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXParent(typeof(Select<AMEstimateReference,
            Where<AMEstimateReference.quoteType, Equal<Current<SOTax.orderType>>,
                And<AMEstimateReference.quoteNbr, Equal<Current<SOTax.orderNbr>>,
                    And<AMEstimateReference.taxLineNbr, Equal<Current<SOTax.lineNbr>>>>>>))]
        protected virtual void _(Events.CacheAttached<SOTax.lineNbr> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(SOTaxAttribute))]
		[SOTaxAMExtension(typeof(SOOrder), typeof(SOTax), typeof(SOTaxTran), typeof(SOOrder.taxCalcMode), TaxCalc = TaxCalc.ManualLineCalc,
			   CuryTaxableAmtField = typeof(SOLine.curyTaxableAmt),
			   Inventory = typeof(SOLine.inventoryID), UOM = typeof(SOLine.uOM), LineQty = typeof(SOLine.orderQty))]
		[SOOpenTax(typeof(SOOrder), typeof(SOTax), typeof(SOTaxTran), TaxCalc = TaxCalc.ManualLineCalc,
			   //Per Unit Tax settings
			   Inventory = typeof(SOLine.inventoryID), UOM = typeof(SOLine.uOM), LineQty = typeof(SOLine.openQty))]
		[SOUnbilledTax(typeof(SOOrder), typeof(SOTax), typeof(SOTaxTran), TaxCalc = TaxCalc.ManualLineCalc,
			   //Per Unit Tax settings
			   Inventory = typeof(SOLine.inventoryID), UOM = typeof(SOLine.uOM), LineQty = typeof(SOLine.unbilledQty))]
        protected virtual void _(Events.CacheAttached<SOLine.taxCategoryID> e) { }

		[SOOrderTaxAMExtension(typeof(SOOrder), typeof(SOTax), typeof(SOTaxTran), typeof(SOOrder.taxCalcMode), TaxCalc = TaxCalc.ManualLineCalc,
			   CuryTaxableAmtField = typeof(SOOrder.curyTaxableFreightAmt))]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(SOOrderTaxAttribute), ExactType = true)]
        protected virtual void _(Events.CacheAttached<SOOrder.freightTaxCategoryID> e) { }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [INUnit(typeof(AMProdItem.inventoryID), DisplayName = "Production UOM")]
        protected virtual void _(Events.CacheAttached<AMProdItem.uOM> e) { }

        //remove customer prospect to prevent invalid error on account id not found
        [PXDBInt]
        [PXUIField(DisplayName = "Customer")]
        protected virtual void _(Data.Events.CacheAttached<AMEstimateReference.bAccountID> e) { }

        [PXDBDefault(typeof(SOOrder.orderNbr), DefaultForInsert = false, DefaultForUpdate = false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXMergeAttributes(Method = MergeMethod.Append)]
        protected virtual void _(Events.CacheAttached<AMEstimateReference.quoteNbr> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[CM.CurrencyInfo(typeof(SOOrder.curyInfoID))]
		protected void _(Events.CacheAttached<AMConfigurationResults.curyInfoID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDBDefault(typeof(Search<SOLineSplit.planID, Where<SOLineSplitExt.aMOrderType,
			Equal<Current<AMProdItem.orderType>>, And<SOLineSplitExt.aMProdOrdID, Equal<Current<AMProdItem.prodOrdID>>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<AMProdItem.demandPlanID> e) { }
        #endregion

        public override void Initialize()
        {
            base.Initialize();

            AMSOLineRecords.AllowDelete = false;
            AMSOLineRecords.AllowInsert = false;

            OrderEstimateRecords.AllowDelete = false;
            OrderEstimateRecords.AllowInsert = false;
            OrderEstimateRecords.AllowUpdate = false;

            SelectedEstimateRecord.AllowDelete = false;

            PXUIFieldAttribute.SetVisible<SOLineExt.isConfigurable>(AMSOLineRecords.Cache, null, AllowConfigurations);
            PXUIFieldAttribute.SetVisible<AMConfigurationResults.completed>(AMSOLineRecords.Cache, null, AllowConfigurations);
            PXUIFieldAttribute.SetEnabled(AMSOLineLinkRecords.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<AMProdItem.selected>(AMSOLineLinkRecords.Cache, null, true);
        }

        public override bool OrderAllowsEdit => Base.Document?.Current != null && Base.Document.AllowUpdate && Base.Transactions.AllowInsert && Base.Transactions.AllowUpdate && !Base.Document.Current.Completed.GetValueOrDefault() && !Base.Document.Current.Cancelled.GetValueOrDefault();

        /// <summary>
        /// If a sales order (or sales lines) are being deleted and contains estimate detail 
        /// we need to make sure the estimate has no reference to the order
        /// </summary>
        protected virtual void RemoveEstimateReferences()
        {
            foreach (SOLine line in Base.Transactions.Cache.Deleted)
            {
                var lineExt = PXCache<SOLine>.GetExtension<SOLineExt>(line);
                if (lineExt?.AMEstimateID == null)
                {
                    continue;
                }

                var estimateRef = DocDetailEstimateRecords.Cache.LocateElse<AMEstimateReferenceSO>(
                    PXSelect<AMEstimateReferenceSO,
                        Where<AMEstimateReferenceSO.estimateID, Equal<Required<AMEstimateReferenceSO.estimateID>>>
                    >.Select(Base, lineExt.AMEstimateID));

                if (estimateRef == null || !estimateRef.OrderType.EqualsWithTrim(line.OrderType) ||
                    !estimateRef.OrderNbr.EqualsWithTrim(line.OrderNbr))
                {
                    continue;
                }

                estimateRef.OrderType = null;
                estimateRef.OrderNbr = null;
                DocDetailEstimateRecords.Update(estimateRef);
            }
        }

        [PXOverride]
        public virtual void Persist(Action del)
        {
            RemoveEstimateReferences();
            UpdateProductionCustomer();
                        
            del?.Invoke();
        }

        /// <summary>
        /// Indicates if the order allows the production order button
        /// </summary>
        public bool AllowProductionOrders
        {
            get
            {
                if (!Features.ManufacturingEnabled())
                {
                    return false;
                }

                if (Base.soordertype.Current != null && Base.CurrentDocument.Current != null)
                {
                    var soOrderTypeExt =
                        PXCache<SOOrderType>.GetExtension<SOOrderTypeExt>(Base.soordertype.Current);

                    return soOrderTypeExt != null
                           && (soOrderTypeExt.AMProductionOrderEntry.GetValueOrDefault()
                               || soOrderTypeExt.AMProductionOrderEntryOnHold.GetValueOrDefault());
                }

                return false;
            }
        }

        //Temporary variable. Please correct the condition or move it into the workflow extension (WorkflowChanges)
        internal bool ShowProductionOrdersActions
        {
            get
            {
                if (Base.soordertype.Current != null && Base.CurrentDocument.Current != null)
                {
                    var soOrderTypeExt =
                        PXCache<SOOrderType>.GetExtension<SOOrderTypeExt>(Base.soordertype.Current);

                    return soOrderTypeExt != null
                           && (soOrderTypeExt.AMProductionOrderEntry.GetValueOrDefault()
                               || soOrderTypeExt.AMProductionOrderEntryOnHold.GetValueOrDefault());
                }

                return false;
            }
        }

        /// <summary>
        /// Indicates if order allows for estimates
        /// </summary>
        public bool AllowEstimates
        {
            get
            {
                var extension = PXCache<SOOrderType>.GetExtension<SOOrderTypeExt>(Base?.soordertype?.Current);
                return AllowEstimatesInt && extension != null && extension.AMEstimateEntry.GetValueOrDefault();
            }
        }

        /// <summary>
        /// Indicates if order allows for configurations
        /// </summary>
        public bool AllowConfigurations => AllowConfigurationsInt && Base?.soordertype?.Current != null
			&& PXCache<SOOrderType>.GetExtension<SOOrderTypeExt>(Base.soordertype.Current)?.AMConfigurationEntry == true;

        public static bool OrderTypeAllowConfigurations(SOOrderType orderType)
        {
            if (orderType == null)
                return false;
            var soOrderTypeExt = PXCache<SOOrderType>.GetExtension<SOOrderTypeExt>(orderType);
            return soOrderTypeExt != null && soOrderTypeExt.AMConfigurationEntry.GetValueOrDefault();
        }

        /// <summary>
        /// Update the estimate reference based on the sales order status. Provides the necessary reference status changes to 
        /// keep in sync with the referenced SO Order.
        /// </summary>
        /// <param name="row">Sales order row of estimates to update</param>
        /// <param name="persist">If true then persist the update call to the DB, otherwise false is just a cache update only</param>
        protected virtual void UpdateEstimateReferenceStatus(SOOrder row, bool persist)
        {
            if (row == null)
            {
                return;
            }

            var estStatus = GetEstRefStatus(row);
            foreach (PXResult<AMEstimateReference, AMEstimateItem> result in PXSelectJoin<AMEstimateReference,
                InnerJoin<AMEstimateItem, On<AMEstimateReference.estimateID, Equal<AMEstimateItem.estimateID>,
                And<AMEstimateReference.revisionID, Equal<AMEstimateItem.revisionID>>>>,
                Where<AMEstimateItem.quoteSource, Equal<EstimateSource.salesOrder>,
                    And<AMEstimateReference.quoteType, Equal<Required<SOOrder.orderType>>,
                    And<AMEstimateReference.quoteNbr, Equal<Required<SOOrder.orderNbr>>>>>
                        >.Select(Base, row.OrderType, row.OrderNbr))
            {
                var estReference = (AMEstimateReference)result;
                var estItem = (AMEstimateItem)result;

                if (string.IsNullOrWhiteSpace(estItem?.EstimateID))
                {
                    continue;
                }

                var isReopen = !EstimateStatus.IsFinished(estStatus) &&
                               EstimateStatus.IsFinished(estItem.EstimateStatus);

                if (EstimateStatus.IsFinished(estStatus) && !EstimateStatus.IsFinished(estItem.EstimateStatus) || isReopen)
                {
                    var estimateItem = PXCache<AMEstimateItem>.CreateCopy(estItem);
                    estimateItem.EstimateStatus = isReopen ? EstimateStatus.Completed : estStatus;
                    var estimateRef = SyncEstimateReference(PXCache<AMEstimateReference>.CreateCopy(estReference), estItem);
					// Change to qty or cost for the reference field will impact taxes. We only want the trigger to row updated when changed.
					var hasCostQtyChanged = estReference?.OrderQty != estimateRef?.OrderQty || estReference?.CuryUnitPrice != estimateRef?.CuryUnitPrice;
                    if (persist)
                    {
                        Base.Caches<AMEstimateItem>().PersistUpdated(estimateItem);
						if (hasCostQtyChanged)
						{
                        OrderEstimateRecords.Cache.PersistUpdated(estimateRef);
						}
                        continue;
                    }
					if (hasCostQtyChanged)
					{
                    OrderEstimateRecords.Update(estimateRef);
					}
                    Base.Caches<AMEstimateItem>().Update(estimateItem);
                }
            }
        }

        public virtual void SOOrder_Completed_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
#pragma warning disable PX1043 // Changes cannot be saved to the database from the event handler
            UpdateEstimateReferenceStatus((SOOrder)e.Row, false);
#pragma warning restore PX1043
        }

        public virtual void SOOrder_Cancelled_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
#pragma warning disable PX1043 // Changes cannot be saved to the database from the event handler
            UpdateEstimateReferenceStatus((SOOrder)e.Row, false);
#pragma warning restore PX1043
        }

        public virtual void SOOrder_RowSelected(PXCache sender, PXRowSelectedEventArgs e, PXRowSelected del)
        {
            del?.Invoke(sender, e);

            var row = (SOOrder)e.Row;
            if (row == null)
            {
                return;
            }

            var isSavedOrder = Base.Document.Cache.GetStatus(Base.Document.Current) != PXEntryStatus.Inserted;

            AddEstimate.SetEnabled(AllowEstimates && isSavedOrder && OrderAllowsEdit);
            quickEstimate.SetEnabled(AllowEstimates && isSavedOrder);

            removeEstimate.SetEnabled(AllowEstimates && isSavedOrder && OrderAllowsEdit);
            CreateProdOrder.SetEnabled(AllowProductionOrders && isSavedOrder);
            ConfigureEntry.SetEnabled(AllowConfigurations && isSavedOrder);

            ConfigureEntry.SetVisible(AllowConfigurations);
            // Make sure estimate tab is controlled by selected order type...
            OrderEstimateRecords.AllowSelect = AllowEstimates;
            CTPButton.SetEnabled(isSavedOrder && OrderAllowsProductionCreation(Base.soordertype.Current, row));

            //Temporary solution. Please correct the visibility condition if needed or move it into the workflow extension (WorkflowChanges)
            Base.Actions[WorkflowChanges.ActionCategories.ManufacturingCategoryID]?.SetVisible(nameof(createProdOrder), ShowProductionOrdersActions);
            Base.Actions[WorkflowChanges.ActionCategories.ManufacturingCategoryID]?.SetVisible(nameof(cTPButton), ShowProductionOrdersActions);

            PXUIFieldAttribute.SetVisible<SOOrderExt.aMCuryEstimateTotal>(sender, row, AllowEstimates);
            PXUIFieldAttribute.SetVisible<SOOrderExt.aMEstimateQty>(sender, row, AllowEstimates);

			PXUIFieldAttribute.SetVisible<SOLineExt.aMProdCreate>(Base.Transactions.Cache, null, AllowProductionOrders);
			PXUIFieldAttribute.SetVisible<SOLineExt.aMOrderType>(Base.Transactions.Cache, null, AllowProductionOrders);
			PXUIFieldAttribute.SetVisible<SOLineExt.aMProdOrdID>(Base.Transactions.Cache, null, AllowProductionOrders);
			PXUIFieldAttribute.SetVisible<SOLineExt.aMConfigKeyID>(Base.Transactions.Cache, null, AllowConfigurations);
			PXUIFieldAttribute.SetVisible<SOLineExt.isConfigurable>(Base.Transactions.Cache, null, AllowConfigurations);
			PXUIFieldAttribute.SetVisible<SOLineExt.aMIsSupplemental>(Base.Transactions.Cache, null, AllowConfigurations);
			PXUIFieldAttribute.SetVisible<SOLineExt.aMParentLineNbr>(Base.Transactions.Cache, null, AllowConfigurations);
			PXUIFieldAttribute.SetVisible<SOLineExt.aMEstimateID>(Base.Transactions.Cache, null, row.Behavior != SOBehavior.BL && !AllowEstimates);
			PXUIFieldAttribute.SetVisible<SOLineExt.aMEstimateRevisionID>(Base.Transactions.Cache, null, row.Behavior != SOBehavior.BL && !AllowEstimates);
        }

		protected virtual void _(Events.FieldUpdated<SOOrder, SOOrder.customerID> e)
		{
			if (e?.Row == null || e.NewValue == e.OldValue || Base.IsCopyOrder) return;
			foreach (AMEstimateReference estimateReference in OrderEstimateRecords.Select())
			{
				if (estimateReference.BAccountID == e.Row.CustomerID) continue;

				estimateReference.BAccountID = e.Row.CustomerID;
				OrderEstimateRecords.Update(estimateReference);
			}
		}

		protected virtual void _(Events.FieldUpdated<SOLine, SOLine.completed> e)
		{
			var soLine = e.Row;
			var soLineExt = soLine.GetExtension<SOLineExt>();
			if (!soLineExt.AMProdCreate.GetValueOrDefault() || string.IsNullOrEmpty(soLineExt.AMProdOrdID)) return;

			var prodOrder = AMProdItem.PK.Find(Base, soLineExt.AMOrderType, soLineExt.AMProdOrdID);
			if (prodOrder == null) return;
			if ((bool)e.NewValue == false)
			{
				ProdItemRecords.Cache.RaiseFieldDefaulting<AMProdItem.demandPlanID>(prodOrder, out var _);
			}
			else
			{
			prodOrder.DemandPlanID = null;
			}
			Base.Caches<AMProdItem>().Update(prodOrder);
		}

        [PXOverride]
		public virtual void SOOrder_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e, PXRowUpdated del)
        {
            SOOrderExt orderExt = null;
            if (!Base.IsCopyOrder && AllowConfigurations && !sender.ObjectsEqual<SOOrder.customerID, SOOrder.customerLocationID, SOOrder.orderDate>(e.OldRow, e.Row))
            {
                UpdateCurrentConfigurations((SOOrder)e.Row);
                orderExt = PXCache<SOOrder>.GetExtension<SOOrderExt>((SOOrder)e.Row);
                if (orderExt != null)
                {
                    orderExt.AMUseConfigPrice = true;
                }
            }

            try
            {
                del?.Invoke(sender, e);
            }
            finally
            {
                if (orderExt != null && orderExt.AMUseConfigPrice.GetValueOrDefault())
                {
                    orderExt.AMUseConfigPrice = false;
                }
            }
        }

        [PXOverride]
        public virtual void RecalculateDiscounts(PXCache sender, SOLine line, Action<PXCache, SOLine> del)
        {
            var lineExt = PXCache<SOLine>.GetExtension<SOLineExt>(line);
            if (lineExt != null && lineExt.AMIsSupplemental.GetValueOrDefault() && Base.IsCopyOrder)
            {
                //When running the copy order process, we insert new sup lines because configurations could change
                //  Depending on setup of discounts there is a problem where Acumatica inserts another Discount Detail record (manual pricing) which is not allowed during save
                //  We assume here we do not need to run anything more related to discounts for the newly inserted supplemental line item
                return;
            }

            del?.Invoke(sender, line);
        }

        protected virtual void UpdateCurrentConfigurations(SOOrder order)
        {
            if (order?.OrderType == null)
            {
                return;
            }
            foreach (AMConfigurationResults configResult in PXSelect<
                AMConfigurationResults, 
                Where <AMConfigurationResults.ordTypeRef, Equal<Required<SOLine.orderType>>,
                    And<AMConfigurationResults.ordNbrRef, Equal<Required<SOLine.orderNbr>>>>>
                .Select(Base, order.OrderType, order.OrderNbr))
            {
                var updated = false;
                if (configResult.CustomerID != order.CustomerID)
                {
                    configResult.CustomerID = order.CustomerID;
                    configResult.CustomerLocationID = order.CustomerLocationID;
                    updated = true;
                }
                if (configResult.CustomerLocationID != order.CustomerLocationID)
                {
                    configResult.CustomerLocationID = order.CustomerLocationID;
                    updated = true;
                }
                if (configResult.DocDate != order.OrderDate)
                {
                    configResult.DocDate = order.OrderDate;
                    updated = true;
                }

                if (!updated)
                {
                    continue;
                }

                ItemConfiguration.Update(configResult);
            }
        }

        public virtual void CopyParamFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e, PXRowSelected del)
        {
            del?.Invoke(sender, e);

            var row = (CopyParamFilter)e.Row;
            if (row == null)

            {
                return;
            }

            PXUIFieldAttribute.SetVisible<SOCopyParamFilterExt.aMIncludeEstimate>(sender, row, AllowEstimates && ContainsEstimates);
            PXUIFieldAttribute.SetEnabled<SOCopyParamFilterExt.aMIncludeEstimate>(sender, row, ContainsEstimates);
            PXUIFieldAttribute.SetVisible<SOCopyParamFilterExt.copyConfigurations>(sender, row,
                Features.ProductConfiguratorEnabled() &&
                OrderTypeAllowConfigurations((SOOrderType)PXSelectorAttribute.Select<CopyParamFilter.orderType>(sender, e.Row)));
        }

        /// <summary>
        /// Does the current estimate contain estimates?
        /// </summary>
        public override bool ContainsEstimates
        {
            get
            {
                if (!OrderEstimateRecords.Cache.Cached.Any_())
                {
                    OrderEstimateRecords.Select();
                }
                return OrderEstimateRecords.Cache.Cached.Any_();
            }
        }

        public virtual void SOLine_RowSelected(PXCache sender, PXRowSelectedEventArgs e, PXRowSelected del)
        {
            del?.Invoke(sender, e);

            var row = (SOLine)e.Row;
            if (row == null)

            {
                return;
            }

            var rowExt = PXCache<SOLine>.GetExtension<SOLineExt>(row);
            bool? hasProductionNbrReference = null;
            if (AllowProductionOrders)
            {
                bool createEnabled = ProductionOrderCreationEnabled(Base.soordertype.Current, Base.Document.Current, row);
                if (createEnabled && !sender.AllowUpdate)
                {
                    sender.AllowUpdate = true;
                    PXUIFieldAttribute.SetEnabled(sender, row, false);
                }

                PXUIFieldAttribute.SetEnabled<SOLineExt.aMSelected>(sender, row, createEnabled);

                if (rowExt != null)
                {
                    PXUIFieldAttribute.SetEnabled<SOLineExt.aMOrderType>(sender, row,

                        rowExt.AMSelected.GetValueOrDefault());

                    bool enableProdOrdID = false;
                    if (rowExt.AMSelected.GetValueOrDefault())

                    {
                        Numbering prodNumbering = PXSelectJoin<Numbering,
                            LeftJoin<AMOrderType, On<AMOrderType.prodNumberingID, Equal<Numbering.numberingID>>>,
                            Where<AMOrderType.orderType, Equal<Required<AMOrderType.orderType>>>
                            >.Select(Base, rowExt.AMOrderType);

                        enableProdOrdID = prodNumbering != null && prodNumbering.UserNumbering.GetValueOrDefault();
                    }
                    else
                    {

                        hasProductionNbrReference = !string.IsNullOrWhiteSpace(rowExt.AMProdOrdID);
                    }
                    PXUIFieldAttribute.SetEnabled<SOLineExt.aMProdOrdID>(sender, row, enableProdOrdID);
                }
            }

            var soOrderTypeExt = PXCache<SOOrderType>.GetExtension<SOOrderTypeExt>(Base.soordertype.Current);

            var isSiteIdEnabledForProduction = Common.Cache.GetEnabled<SOLine.siteID>(sender, row).GetValueOrDefault() &&
                (soOrderTypeExt.AMEnableWarehouseLinkedProduction.GetValueOrDefault() || !hasProductionNbrReference.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<SOLine.siteID>(sender, row, isSiteIdEnabledForProduction);

            OrderLineRowSelected(sender, row, rowExt, GetConfiguration(row), AllowConfigurations);

			var quoteMarkForProdOnly = soOrderTypeExt?.AMMTOOrder == true && row.Behavior == SOBehavior.QT;
			var shipableProduction = AllowProductionOrders && row.Operation == SOOperation.Issue;
            // Allow only POCreate or AMProdCreate to be selected at any time. Cannot select both
			PXUIFieldAttribute.SetEnabled<SOLineExt.aMProdCreate>(sender, e.Row, row.POCreate != true && (shipableProduction || quoteMarkForProdOnly));
            PXUIFieldAttribute.SetEnabled<SOLine.pOCreate>(sender, e.Row, rowExt?.AMProdCreate != true && Common.Cache.GetEnabled<SOLine.pOCreate>(sender, e.Row) == true);
        }

        public virtual void SOLine_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e, PXRowUpdated del)
        {
            var row = (SOLine)e.Row;
            var rowOld = (SOLine)e.OldRow;
            if (row == null || rowOld == null)
            {

                return;
            }
            var rowExt = PXCache<SOLine>.GetExtension<SOLineExt>(row);
            var rowExtOld = PXCache<SOLine>.GetExtension<SOLineExt>(rowOld);
            if (rowExt == null || rowExtOld == null)
            {
                return;
            }

            var keepManualPricingValue = rowExt.IsConfigurable.GetValueOrDefault() && rowExt.AMConfigKeyID != rowExtOld.AMConfigKeyID
                // Logic copied from base RowUpdated excluding change of price...
                && (e.ExternalCall || sender.Graph.IsImport)
                && sender.ObjectsEqual<SOLine.customerID>(e.Row, e.OldRow)
                && sender.ObjectsEqual<SOLine.inventoryID>(e.Row, e.OldRow) &&
                sender.ObjectsEqual<SOLine.uOM>(e.Row, e.OldRow)
                && sender.ObjectsEqual<SOLine.orderQty>(e.Row, e.OldRow) &&
                sender.ObjectsEqual<SOLine.branchID>(e.Row, e.OldRow)
                && sender.ObjectsEqual<SOLine.siteID>(e.Row, e.OldRow)
                && sender.ObjectsEqual<SOLine.manualPrice>(e.Row, e.OldRow);

            bool currentManualPriceValue = row.ManualPrice.GetValueOrDefault();

            del.Invoke(sender, e);

            if (keepManualPricingValue && currentManualPriceValue != row.ManualPrice.GetValueOrDefault())
            {
                // Need to preserve manual pricing when the key is changed by the user (which updates the price if not currently manual price)
                // Base call will update manual price incorrectly for configured items when key is changed... change it back...
                row.ManualPrice = currentManualPriceValue;
            }

            // SOOrder_RowUpdated calls DiscountEngine.RecalculatePricesAndDiscountsOnLine which raises SOLine.RowUpdated. 
        }

		[Obsolete("Logic moved to SOOrderLineSplittingAllocatedExtensionAMExtension")]
        protected virtual bool ProductionExtensionFieldsChanged(PXCache cache, SOLine rowUpdated, SOLine rowOld)
        {
            return !cache.ObjectsEqual<SOLineExt.aMProdCreate>(rowUpdated, rowOld)
                   || !cache.ObjectsEqual<SOLineExt.aMProdStatusID>(rowUpdated, rowOld)
                   || !cache.ObjectsEqual<SOLineExt.aMOrderType>(rowUpdated, rowOld)
                   || !cache.ObjectsEqual<SOLineExt.aMProdOrdID>(rowUpdated, rowOld)
                   || !cache.ObjectsEqual<SOLineExt.aMProdQtyComplete>(rowUpdated, rowOld)
                   || !cache.ObjectsEqual<SOLineExt.aMProdBaseQtyComplete>(rowUpdated, rowOld);
        }

		[Obsolete("Logic moved to SOOrderLineSplittingAllocatedExtensionAMExtension")]
        protected virtual void UpdateSplitExtension(SOLine soLine, PXCache splitCache)
        {
            var soLineExt = PXCache<SOLine>.GetExtension<SOLineExt>(soLine);
            if (soLine == null || soLineExt == null || soLine.Completed.GetValueOrDefault())
            {
                return;
            }

            foreach (SOLineSplit split in PXParentAttribute.SelectChildren(splitCache, soLine, typeof(SOLine)))
            {
                if (split?.Completed == true || split?.IsAllocated == true)
                {
                    continue;
                }

                var splitExt = PXCache<SOLineSplit>.GetExtension<SOLineSplitExt>(split);
                if (splitExt == null)
                {
                    continue;
                }

                split.AMProdCreate = soLineExt.AMProdCreate.GetValueOrDefault();
                splitExt.AMOrderType = soLineExt.AMOrderType;
                splitExt.AMProdOrdID = soLineExt.AMProdOrdID;
                splitExt.AMProdStatusID = soLineExt.AMProdStatusID;
                splitExt.AMProdQtyComplete = soLineExt.AMProdQtyComplete;
                splitExt.AMProdBaseQtyComplete = soLineExt.AMProdBaseQtyComplete;
                splitCache.Update(split);
            }
        }

        public virtual void SOLine_RowDeleting(PXCache sender, PXRowDeletingEventArgs e, PXRowDeleting del)
        {
            del?.Invoke(sender, e);

            var row = (SOLine) e.Row;
            if (row == null)
            {
                return;
            }

            var rowExt = PXCache<SOLine>.GetExtension<SOLineExt>(row);

            if (AllowConfigurations
                && rowExt != null
                && rowExt.AMIsSupplemental.GetValueOrDefault())
            {
                // Get the Parent Line
                SOLine parentRow = PXSelect<
                    SOLine, 
                    Where<SOLine.orderType, Equal<Required<SOLine.orderType>>,
                        And<SOLine.orderNbr, Equal<Required<SOLine.orderNbr>>,
                        And<SOLine.lineNbr, Equal<Required<SOLine.lineNbr>>
                            >>>>
                    .Select(Base, row.OrderType, row.OrderNbr, rowExt.AMParentLineNbr);

                if (parentRow == null || sender.GetStatus(parentRow) == PXEntryStatus.Deleted || Base.IsCopyOrder)
                {
                    return;
                }

                e.Cancel = true;

                var currentInventoryID = sender.GetStateExt<SOLine.inventoryID>((object)row);
                var parentInventoryID = sender.GetStateExt<SOLine.inventoryID>((object)parentRow);

                throw new PXException(Messages.GetLocal(Messages.CannotDeleteSupplementalSalesLine, currentInventoryID, parentInventoryID));
            }
        }

		public virtual void SOLine_RowDeleted(PXCache sender, PXRowDeletedEventArgs e, PXRowDeleted del)
        {
            del?.Invoke(sender, e);

            var row = (SOLine)e.Row;
			RemoveLinkProductionOrders(row);
            var soLineExt = PXCache<SOLine>.GetExtension<SOLineExt>(row);
            if (!AllowConfigurations || row == null || soLineExt == null || !soLineExt.IsConfigurable.GetValueOrDefault())
            {
                return;
            }

            var supsDeleted = false;
            foreach (SOLine supplementalLine in PXSelect<
                SOLine, 
                Where<SOLine.orderType, Equal<Required<SOLine.orderType>>,
                    And<SOLine.orderNbr, Equal<Required<SOLine.orderNbr>>,
                    And<SOLineExt.aMParentLineNbr, Equal<Required<SOLineExt.aMParentLineNbr>>,
                        And<SOLineExt.aMIsSupplemental, Equal<True>
                           >>>>>
                .Select(Base, row.OrderType, row.OrderNbr, soLineExt.AMParentLineNbr))
            {
                Base.Transactions.Delete(supplementalLine);
                supsDeleted = true;
            }

            if (supsDeleted)
            {
                //Deleted sups not correctly shown as deleted in the UI so we need to shake the tree...
                Base.Transactions.View.RequestRefresh();
            }
        }

		protected virtual void RemoveLinkProductionOrders(SOLine soLine)
		{
			if (soLine?.OrderType == null)
			{
				return;
			}

			var soLineStatus = Base.Transactions.Cache.GetStatus(soLine);
			if (soLineStatus == PXEntryStatus.InsertedDeleted)
			{
				return;
			}

			var soLineExtension = PXCache<SOLine>.GetExtension<SOLineExt>(soLine);
			if (string.IsNullOrWhiteSpace(soLineExtension?.AMProdOrdID))
			{
				return;
			}
			var prodItem = AMProdItem.PK.Find(Base, soLineExtension?.AMOrderType, soLineExtension?.AMProdOrdID);

			if (prodItem == null)
			{
				return;
			}

			prodItem.OrdTypeRef = null;
			prodItem.OrdNbr = null;
			prodItem.OrdLineRef = null;
			prodItem.CustomerID = null;
			prodItem.DemandPlanID = null;
			prodItem.SupplyType = ProductionSupplyType.Inventory;

			ProdItemRecords.Update(prodItem);

			// update after Prod Item
			foreach (AMProdItemSplit prodItemSplit in PXSelect<AMProdItemSplit,
				Where<AMProdItemSplit.orderType, Equal<Required<AMProdItemSplit.orderType>>,
					And<AMProdItemSplit.prodOrdID, Equal<Required<AMProdItemSplit.prodOrdID>>>>>.Select(Base, prodItem.OrderType, prodItem.ProdOrdID))
			{
				// Triggers AMProdItemSplitPlanIDAttribute
				ProdItemSplitRecords.Cache.RaiseRowUpdated(prodItemSplit, prodItemSplit);
			}

			//Common.Cache.AddCacheView<AMProdEvnt>(Base);
			var prodEvent = ProductionEventHelper.InsertInformationEvent(Base,
				Messages.GetLocal(Messages.RemovedLinkToSalesOrder, soLine.OrderType, soLine.OrderNbr),
					prodItem.ProdOrdID, prodItem.OrderType, false);

			if (prodEvent != null)
			{
				prodEvent.RefNoteID = Base.Document.Current?.NoteID;
				ProdEvntRecords.Update(prodEvent);
			}
		}

        public virtual void SOLine_InventoryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e, PXFieldVerifying del)
        {
            del?.Invoke(sender, e);

            var row = (SOLine)e.Row;
            var rowExt = PXCache<SOLine>.GetExtension<SOLineExt>(row);
            if (rowExt == null || Base.Document.Cache.GetStatus(Base.Document.Current) == PXEntryStatus.Inserted)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(rowExt.AMOrderType) && !string.IsNullOrWhiteSpace(rowExt.AMProdOrdID))
            {
                e.NewValue = row.InventoryID;
                throw new PXSetPropertyException(Messages.SalesLineIsLInkedToProduction, PXUIFieldAttribute.GetDisplayName<SOLine.inventoryID>(sender), rowExt.AMOrderType, rowExt.AMProdOrdID);
            }

            if (rowExt.IsConfigurable == true
                && TransactionsAskDeleteRelatedConfiguration() == WebDialogResult.No)
            {
                e.NewValue = row.InventoryID;
                e.Cancel = true;
            }
        }

		protected virtual void _(Events.FieldVerifying<SOOrder, SOOrder.disableAutomaticTaxCalculation> e)
		{
			if (e.Row == null)
				return;

			if ((bool?)e.NewValue == true && AllowEstimates)
			{
				throw new PXSetPropertyException(Messages.EstimatesAndDisableTaxCalculationConflictSOOrder);
			}
		}

		protected virtual WebDialogResult TransactionsAskDeleteRelatedConfiguration()
        {
            if (!AllowConfigurations
                || (ItemConfiguration != null && ItemConfiguration.IsCopyMode)
                || Base.IsImport
                || Base.IsCopyPasteContext)
            {
                return WebDialogResult.Yes;
            }

            return Base.Transactions.Ask(Messages.DeletingExistingConfiguration, MessageButtons.YesNo);
        }

		protected virtual void SOLine_RowInserted(PXCache sender, PXRowInsertedEventArgs e, PXRowInserted del)
        {
            del?.Invoke(sender, e);

            var row = (SOLine)e.Row;
            var rowExt = PXCache<SOLine>.GetExtension<SOLineExt>(row);
            if (rowExt == null)
            {
                return;
            }

            rowExt.AMParentLineNbr = row.LineNbr;

            if (!Base.IsCopyOrder || !AllowConfigurations || !IsCopyConfig || row?.OrigLineNbr == null)
            {
                return;
            }

#if DEBUG
            AMDebug.TraceWriteMethodName($"RowInserted: SOLine[{row.OrderType}-{row.OrderNbr}-{row.LineNbr}]");
#endif
            AMConfigurationResults toConfigResult = null;
            if (row.InventoryID != null && row.SiteID != null)
            {
                toConfigResult = PXSelect<AMConfigurationResults,
                    Where<AMConfigurationResults.ordTypeRef, Equal<Required<SOLine.orderType>>,
                        And<AMConfigurationResults.ordNbrRef, Equal<Required<SOLine.orderNbr>>,
                            And<AMConfigurationResults.ordLineRef, Equal<Required<SOLine.lineNbr>>>>>
                >.Select(Base, row.OrderType, row.OrderNbr, row.LineNbr);

                UpdateConfigurationResult(row, toConfigResult);
            }

            if (string.IsNullOrWhiteSpace(rowExt.AMConfigurationID))
            {
                return;
            }

            AMConfigurationResults fromConfigResult = PXSelect<AMConfigurationResults,
                Where<AMConfigurationResults.ordTypeRef, Equal<Required<AMConfigurationResults.ordTypeRef>>,
                    And<AMConfigurationResults.ordNbrRef, Equal<Required<AMConfigurationResults.ordNbrRef>>,
                        And<AMConfigurationResults.ordLineRef, Equal<Required<AMConfigurationResults.ordLineRef>>>>>
            >.Select(Base, row.OrigOrderType, row.OrigOrderNbr, row.OrigLineNbr);

            if (fromConfigResult == null)
            {
                return;
            }

            toConfigResult = PXSelect<AMConfigurationResults,
                Where<AMConfigurationResults.ordTypeRef, Equal<Required<AMConfigurationResults.ordTypeRef>>,
                    And<AMConfigurationResults.ordNbrRef, Equal<Required<AMConfigurationResults.ordNbrRef>>,
                        And<AMConfigurationResults.ordLineRef, Equal<Required<AMConfigurationResults.ordLineRef>>>>>
            >.Select(Base, row.OrderType, row.OrderNbr, row.LineNbr);

            if (toConfigResult == null)
            {
                return;
            }

            //set key fields for pricing
            toConfigResult.Qty = row.OrderQty.GetValueOrDefault();
            toConfigResult.UOM = row.UOM;
            toConfigResult.CustomerID = row.CustomerID;
            toConfigResult.CustomerLocationID = Base.Document?.Current?.CustomerLocationID;
            toConfigResult = ItemConfiguration.Update(toConfigResult);
            if (toConfigResult == null)
            {
                return;
            }

            var graphExt = Base.GetExtension<SOOrderEntryAMExtension>();
#pragma warning disable PX1043 // Changes cannot be saved to the database from the event handler
#pragma warning disable PX1045 // A PXGraph instance cannot be created within an event handler
#if DEBUG
            // Based on logic PX1043 & PX1045 are not an issues because there is not save to the database or graph instance created.
#endif
            graphExt.CopyConfiguration(row, toConfigResult, fromConfigResult);
#pragma warning restore PX1045
#pragma warning restore PX1043
        }

        public virtual void SOLine_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e, PXFieldUpdated del)
        {
            del?.Invoke(sender, e);

            var row = (SOLine)e.Row;
            if (row == null)
            {
                return;
            }

            if (AllowConfigurations && !ItemConfiguration.IsCopyMode && !IsCopyConfig)
            {
                UpdateConfigurationResult(row);
            }

            var rowExt = PXCache<SOLine>.GetExtension<SOLineExt>(row);
            if (rowExt == null)
            {
                return;
            }

            var item = (InventoryItem)PXSelectorAttribute.Select<SOLine.inventoryID>(sender, row) ?? 
                PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(Base, row.InventoryID);

			var invItemExt = item == null ? null : PXCache<InventoryItem>.GetExtension<InventoryItemExt>(item);
			if (invItemExt == null)
            {
				return;
			}

			var orderTypeExt = PXCache<SOOrderType>.GetExtension<SOOrderTypeExt>(Base.soordertype.Current);
			if (orderTypeExt == null)
			{
				return;
			}

			var shipableProduction = (orderTypeExt?.AMProductionOrderEntry == true || orderTypeExt?.AMProductionOrderEntryOnHold == true)
				&& row.Operation == SOOperation.Issue && orderTypeExt?.AMMTOOrder == true;
			var quoteMarkForProdOnly = orderTypeExt?.AMMTOOrder == true && row.Behavior == SOBehavior.QT;
			rowExt.AMProdCreate = invItemExt?.AMMakeToOrderItem == true && (shipableProduction || quoteMarkForProdOnly);
		}

        protected virtual void _(Events.FieldUpdated<SOLine, SOLine.operation> e)
		{
			if (e.Row == null)
			{
				return;
			}

			var rowExt = PXCache<SOLine>.GetExtension<SOLineExt>(e.Row);
			if (rowExt == null)
			{
				return;
			}

			if (e.Row.Operation != SOOperation.Issue)
			{
				rowExt.AMProdCreate = false;
            }
        }

        public virtual void SOLine_SiteID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e, PXFieldVerifying del)
        {
            del?.Invoke(sender, e);

            var row = (SOLine)e.Row;
            var rowExt = PXCache<SOLine>.GetExtension<SOLineExt>(row);
            if (rowExt == null)
            {
                return;
            }

            if (rowExt.IsConfigurable.GetValueOrDefault() 
                && sender.GetStatus(e.Row) != PXEntryStatus.Inserted
                && ConfigurationChangeRequired(row.InventoryID, WarehouseAsID(e.NewValue), GetConfiguration(row))
                && TransactionsAskDeleteRelatedConfiguration() == WebDialogResult.No)
            {
                e.NewValue = row.SiteID;
                e.Cancel = true;
            }
        }

        public virtual void SOLine_SiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e, PXFieldUpdated del)
        {
            del?.Invoke(sender, e);

            var row = (SOLine)e.Row;
            if (row == null)
            {
                return;
            }

            //If UOM defaulting wasn't done already, skip since this will be done later by another event handler
            if (row.UOM != null && AllowConfigurations && !ItemConfiguration.IsCopyMode && !IsCopyConfig)
            {
                UpdateConfigurationResult(row);
            }
        }

        #region Overrides for price change
        // Ref to 059230 for request to simplify price override

        // [1 of 4] Copy of 6.10.0680 to replace price call only...
        protected virtual void SOLine_UOM_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e, PXFieldUpdated del)
        {
            del?.Invoke(sender, e);
            TrySetConfiguredPrice(sender, e);
        }

        // [2 of 4] Copy of 6.10.0680 to replace price call only...
        protected virtual void SOLine_OrderQty_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e, PXFieldUpdated del)
        {
            // If we do not call the delegate, the OpenQty field never gets set and causes some big issues on sales order...
            del?.Invoke(sender, e);
            TrySetConfiguredPrice(sender, e);
        }

        // [3 of 4] Copy of 6.10.0680 to replace price call only...
        protected virtual void SOLine_ManualPrice_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e, PXFieldUpdated del)
        {
            del?.Invoke(sender, e);
            TrySetConfiguredPrice(sender, e);
        }

        // [4 of 4] Copy of 6.10.0680 to replace price call only...
        protected virtual void SOLine_IsFree_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e, PXFieldUpdated del)
        {
            del?.Invoke(sender, e);
            TrySetConfiguredPrice(sender, e);
        }

        protected virtual void TrySetConfiguredPrice(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            TrySetConfiguredPrice(sender, e.Row as SOLine);
        }

        protected virtual void TrySetConfiguredPrice(PXCache sender, SOLine row)
        {
            if (row == null || Base.IsCopyOrder || row.ManualPrice.GetValueOrDefault() || row.IsFree.GetValueOrDefault() || sender.Graph.IsCopyPasteContext)
            {
                return;
            }
            var rowExt = PXCache<SOLine>.GetExtension<SOLineExt>(row);
            if (rowExt != null && rowExt.IsConfigurable.GetValueOrDefault())
            {
                // Overriding price calculation call
                SetConfiguredLineUnitPrice(sender, row, PXSelect<AMConfigurationResults,
                    Where<AMConfigurationResults.ordTypeRef, Equal<Required<AMConfigurationResults.ordTypeRef>>,
                        And<AMConfigurationResults.ordNbrRef, Equal<Required<AMConfigurationResults.ordNbrRef>>,
                            And<AMConfigurationResults.ordLineRef, Equal<Required<AMConfigurationResults.ordLineRef>>>>>
                >.Select(Base, row.OrderType, row.OrderNbr, row.LineNbr));
            }
        }

        #endregion

        protected virtual void SetConfiguredLineUnitPrice(PXCache sender, SOLine row, AMConfigurationResults configResult)
        {
            SetConfiguredLineUnitPrice(sender, row, configResult, true);
        }

        protected virtual void SetConfiguredLineUnitPrice(PXCache sender, SOLine row, AMConfigurationResults configResult, bool setInCache)
        {
            // When calling cache set value if we copy it will not set the value in cache so only copy if row update
            var rowCopy = setInCache ? row : PXCache<SOLine>.CreateCopy(row);
            var rowExt = PXCache<SOLine>.GetExtension<SOLineExt>(rowCopy);
            if (rowCopy == null || rowExt == null)
            {
                return;
            }

            if (!AllowConfigurations || !rowExt.IsConfigurable.GetValueOrDefault() ||
                rowCopy.ManualPrice.GetValueOrDefault())
            {
                return;
            }

            var price = 0m;
            if (configResult != null)
            {
                price = AMConfigurationPriceAttribute.GetPriceExt<AMConfigurationResults.displayPrice>(ItemConfiguration.Cache, configResult, ConfigCuryType.Document).GetValueOrDefault();
            }

            if (rowCopy.CuryUnitPrice.GetValueOrDefault() == price)
            {
                return;
            }

#if DEBUG
            AMDebug.TraceWriteMethodName($"[Sales Line {rowCopy.LineNbr}] updating configured unit price from {rowCopy.CuryUnitPrice.GetValueOrDefault()} to {price} [setInCache={setInCache};IsCopyOrder={Base.IsCopyOrder}]");
#endif
            if (setInCache)
            {
                sender.SetValueExt<SOLine.curyUnitPrice>(rowCopy, price);
                return;
            }
            // Else update the data row...
            rowCopy.CuryUnitPrice = price;
            sender.Update(rowCopy);
        }

        public virtual void SOLine_AMSelected_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e, PXFieldUpdated del)
        {
            del?.Invoke(sender, e);

            var row = (SOLine)e.Row;
            var rowExt = PXCache<SOLine>.GetExtension<SOLineExt>(row);
            if (rowExt == null)
            {
                return;
            }

            if (rowExt.AMSelected.GetValueOrDefault() && ProductionSetup.Current != null)
            {
                rowExt.AMOrderType = ProductionSetup.Current.DefaultOrderType;
                return;
            }

            if (!rowExt.AMSelected.GetValueOrDefault()
                && !string.IsNullOrWhiteSpace(rowExt.AMOrderType)
                && string.IsNullOrWhiteSpace(rowExt.AMProdOrdID))
            {
                rowExt.AMOrderType = null;
            }
        }

        public virtual void SOLine_AMProdOrdID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e, PXFieldVerifying del)
        {
            var row = (SOLine)e.Row;
            var rowExt = PXCache<SOLine>.GetExtension<SOLineExt>(row);
            if (rowExt == null)
            {
                return;
            }

            Numbering prodNumbering = PXSelectJoin<Numbering, LeftJoin<AMOrderType, On<AMOrderType.prodNumberingID,
                Equal<Numbering.numberingID>>>, Where<AMOrderType.orderType, Equal<Required<AMOrderType.orderType>>>
                    >.Select(Base, rowExt.AMOrderType);

            if (prodNumbering == null)
            {
                throw new PXException(Messages.GetLocal(Messages.NumberingMissingExceptionProduction), rowExt.AMOrderType);
            }

            if (prodNumbering.UserNumbering.GetValueOrDefault() && rowExt.AMSelected.GetValueOrDefault())
            {
                AMProdItem amProdItem = PXSelect<AMProdItem, Where<AMProdItem.orderType, Equal<Required<AMProdItem.orderType>>,
                    And<AMProdItem.prodOrdID, Equal<Required<AMProdItem.prodOrdID>>>>>.Select(Base, rowExt.AMOrderType, e.NewValue);
                if (amProdItem != null)
                {
                    string prodID = (string)e.NewValue;
                    e.NewValue = null;
                    throw new PXSetPropertyException(Messages.GetLocal(Messages.ProductionOrderIDIsAlreadyUsed), prodID, rowExt.AMOrderType);
                }
            }

            del?.Invoke(sender, e);
        }

        public virtual void SOLine_CuryUnitPrice_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e, PXFieldDefaulting del)
        {
            var row = (SOLine) e.Row;
            if (row != null && PXCache<SOLine>.GetExtension<SOLineExt>(row)?.IsConfigurable == true
                            && (Base.Transactions.Current == null ||
                                !sender.ObjectsEqual<SOLine.orderType,
                                        SOLine.orderNbr,
                                        SOLine.lineNbr>(row,
                                        Base.Transactions.Current)))
            {
                // All we have to go from in the price calculation for configured items in ARSalesPriceMaintAMExtension is the current sales line (Base.Transactions.Current).
                //  There are cases when the current is not correct for what we need (Ex: updating sales order date and the process for updating price runs for all lines but current remains the same for all lines).
                Base.Transactions.Current = row;
            }

            del?.Invoke(sender, e);
        }

		public virtual void _(Events.RowPersisting<AMConfigurationResults> e, PXRowPersisting del)
		{
			if (e.Row != null && (e.Operation & PXDBOperation.Command) == PXDBOperation.Insert)
			{
				e.Cache.Current = e.Cache.Locate(e.Row);
			}
			del?.Invoke(e.Cache, e.Args);
		}

        /// <summary>
        /// Indicates if the given order and sales line allows for production order creation
        /// </summary>
        public virtual bool ProductionOrderCreationEnabled(SOOrderType orderType, SOOrder order, SOLine line)
        {
            if (order == null || line == null || line.POCreate.GetValueOrDefault() || orderType == null)
            {
                return false;
            }

            var lineExtension = PXCache<SOLine>.GetExtension<SOLineExt>(line);
            if (lineExtension == null)

            {
                return false;
            }

            return string.IsNullOrWhiteSpace(lineExtension.AMProdOrdID) && OrderAllowsProductionCreation(orderType, order);
        }

        public virtual bool OrderAllowsProductionCreation(SOOrderType orderType, SOOrder order)
        {
           return CanCreateProductionOrder(orderType, order);
        }

        public static bool CanCreateProductionOrder(SOOrderType orderType, SOOrder order)
        {
            if (orderType == null || order == null)
            {
                return false;
            }

            var orderTypeExtension = PXCache<SOOrderType>.GetExtension<SOOrderTypeExt>(orderType);
            if (orderTypeExtension == null || order.Cancelled.GetValueOrDefault() || order.Completed.GetValueOrDefault())
            {
                return false;
            }

            var allowApproved = order.Approved.GetValueOrDefault() &&
                                 orderTypeExtension.AMProductionOrderEntry.GetValueOrDefault();

            var allowOnHold = order.Hold.GetValueOrDefault() &&
                              orderTypeExtension.AMProductionOrderEntryOnHold.GetValueOrDefault();

            return allowApproved || allowOnHold;
        }

        public virtual void AMEstimateItem_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            AMEstimateItemRowSelectedInt(cache, e, Base.Transactions.AllowUpdate);
        }

        /// <summary>
        /// Get/calculate the sales lines plan date for production
        /// </summary>
        /// <param name="soLine">sales line</param>
        /// <returns>production plan date</returns>
        protected virtual DateTime? GetPlanDate(SOLine soLine)
        {
            DateTime businessDate = Common.Current.BusinessDate(Base);
            DateTime planDate = soLine.ShipDate ?? soLine.RequestDate.GetValueOrDefault(businessDate);

            if (Common.Dates.IsMinMaxDate(planDate)
                || Common.Dates.IsDefaultDate(planDate))
            {
                planDate = businessDate;
            }

            //  lets remove a day to account for production completion before ship date
            return planDate.AddDays(-1);
        }

        public PXAction<SOOrder> CreateProdOrder;
        [PXButton]
        [PXUIField(DisplayName = Messages.CreateProductionOrdersInq, MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Update)]
        public virtual IEnumerable createProdOrder(PXAdapter adapter)
        {
            if (AMSOLineRecords.AskExt() == WebDialogResult.OK)
            {
                Base.Persist();

                CreateProductionOrders(AMSOLineRecords.Cache.Cached.Cast<SOLine>().ToList());
                //Sales Order Refresh as Create Orders updates SOLine
                Base.Actions.PressCancel();
            }
            
            return adapter.Get();
        }

        /// <summary>
        /// Create production orders based on a set list of sales lines
        /// </summary>
        public virtual void CreateProductionOrders(List<SOLine> list)
        {
            if (list == null
                || list.Count == 0
                || !AllowProductionOrders)
            {
                return;
            }

            var createProdOrdersGraph = PXGraph.CreateInstance<CreateProductionOrdersProcess>();
            createProdOrdersGraph.CreateProductionOrdersFromSalesLines(list);
        }

        public PXAction<SOOrder> linkProdOrder;
        [PXButton]
        [PXUIField(DisplayName = "Link Prod Order", MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Update)]
        public virtual IEnumerable LinkProdOrder(PXAdapter adapter)
        {
            var soLineExt = PXCache<SOLine>.GetExtension<SOLineExt>(Base.Transactions.Current);

            if (soLineExt != null && Base.Transactions.Current != null &&
                soLineExt.AMProdCreate.GetValueOrDefault() &&
                AllowProductionOrders &&
                AMSOLineLinkRecords.AskExt() == WebDialogResult.OK)
            {
                try
                {
                    var selectedProdItem = GetFirstSelectedAMProdItem();
                    AMSOLineLinkRecords.Cache.Clear();
                    Base.Persist();

                    LinkProductionOrders(Base, Base.Transactions.Current, selectedProdItem);

                    Base.Actions.PressSave();
                }
                catch
                {
                    ResetLinkProd();
                    throw;
                }
            }
			if (AMSOLineLinkRecords?.Current != null)
			{
            AMSOLineLinkRecords.Current.Selected = false;
			}
            ResetLinkProd();
            return adapter.Get();
        }

        private AMProdItem GetFirstSelectedAMProdItem()
        {
            return AMSOLineLinkRecords.Cache.Cached.RowCast<AMProdItem>().Where(r => r.Selected == true)?.First();
        }

        public virtual void LinkProductionOrders(PXGraph graph, SOLine soLine, AMProdItem prodItem)
        {
            _LinkProductionOrders(graph, soLine, prodItem);
        }

        internal static void _LinkProductionOrders(PXGraph graph, SOLine soLine, AMProdItem prodItem)
        {
            if (soLine?.OrderNbr == null || prodItem?.OrderType == null || prodItem.ProdOrdID == null)
            {
                return;
            }

            var soLineExt = PXCache<SOLine>.GetExtension<SOLineExt>(soLine);
            if (soLineExt == null || !string.IsNullOrWhiteSpace(soLineExt.AMProdOrdID) 
                || soLineExt.AMProdCreate != true || !string.IsNullOrWhiteSpace(prodItem.OrdNbr))
            {
                return;
            }

            soLineExt.AMOrderType = prodItem.OrderType;
            soLineExt.AMProdOrdID = prodItem.ProdOrdID;

            graph.Caches[typeof(SOLine)].Update(soLine);

            //Need to figure this out because could be multiple lines... but for now
            SOLineSplit soLineSplit = PXSelect<SOLineSplit,
                    Where<SOLineSplit.orderType, Equal<Required<SOLineSplit.orderType>>,
                        And<SOLineSplit.orderNbr, Equal<Required<SOLineSplit.orderNbr>>,
                        And<SOLineSplit.lineNbr, Equal<Required<SOLineSplit.lineNbr>>
                    >>>>.Select(graph, soLine.OrderType, soLine.OrderNbr, soLine.LineNbr);


            if (soLineSplit?.PlanID == null)
            {
                return;
            }

            var soLineSplitExt = PXCache<SOLineSplit>.GetExtension<SOLineSplitExt>(soLineSplit);
            if (soLineSplitExt == null)
            {
                return;
            }

            INItemPlan solineSplitPlan = PXSelect<INItemPlan,
                    Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>
                .Select(graph, soLineSplit?.PlanID);

            AMProdItemSplit prodItemSplit = PXSelect<AMProdItemSplit, 
                Where<AMProdItemSplit.orderType, Equal<Required<AMProdItem.orderType>>, 
                And<AMProdItemSplit.prodOrdID, Equal<Required<AMProdItem.prodOrdID>>>>>.Select(graph, prodItem.OrderType, prodItem.ProdOrdID);

            if (solineSplitPlan == null || prodItemSplit?.PlanID == null)
            {
                return;
            }

            solineSplitPlan.SupplyPlanID = prodItemSplit.PlanID;
            graph.Caches[typeof(INItemPlan)].Update(solineSplitPlan);

            soLineSplit.RefNoteID = prodItem.NoteID;
            soLineSplitExt.AMOrderType = prodItem.OrderType;
            soLineSplitExt.AMProdOrdID = prodItem.ProdOrdID;

            graph.Caches[typeof(SOLineSplit)].Update(soLineSplit);

			prodItem.DemandPlanID = solineSplitPlan.PlanID;
            prodItem.OrdTypeRef = soLine.OrderType;
            prodItem.OrdNbr = soLine.OrderNbr;
            prodItem.OrdLineRef = soLine.LineNbr;
            prodItem.CustomerID = soLine.CustomerID;
            prodItem.SupplyType = ProductionSupplyType.SalesOrder;

            Common.Cache.AddCacheView<AMProdItem>(graph);
            graph.Caches[typeof(AMProdItem)].Update(prodItem);

            INItemPlan prodItemPlan = PXSelect<INItemPlan,
                    Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>
                .Select(graph, prodItemSplit.PlanID);

            if (prodItemPlan?.PlanType == null)
            {
                return;
            }

            prodItemPlan.PlanType = prodItem.Hold == true
                || prodItem.Released == false ? INPlanConstants.PlanMD : INPlanConstants.PlanME;
            prodItemPlan.BAccountID = soLine.CustomerID;
            graph.Caches[typeof(INItemPlan)].Update(prodItemPlan);

            Common.Cache.AddCacheView<AMProdEvnt>(graph);

            var prodEvent = ProductionEventHelper.InsertInformationEvent(graph,
                Messages.GetLocal(Messages.LinkedToProdOrderTypeOrderNbr, soLine.OrderType, soLine.OrderNbr),
                prodItem.ProdOrdID, prodItem.OrderType, false);
            if (prodEvent != null)
            {
                prodEvent.RefNoteID = solineSplitPlan.RefNoteID;
                graph.Caches[typeof(AMProdEvnt)].Update(prodEvent);
            }
        }

        protected static AMEstimateReference MakeEstimateReference(SOOrder order)
        {
            return SetEstimateReference(new AMEstimateReference(), order);
        }

        protected override AMEstimateReference SetEstimateReference(AMEstimateReference estimateReference)
        {
            if (estimateReference == null)
            {
                throw new ArgumentNullException(nameof(estimateReference));
            }

            var order = (SOOrder)Base?.Document?.Current;
            if (order == null)
            {
                return null;
            }

            return SetEstimateReference(estimateReference, order);
        }

        protected static AMEstimateReference SetEstimateReference(AMEstimateReference estimateReference, SOOrder order)
        {
            if (estimateReference == null)
            {
                throw new ArgumentNullException(nameof(estimateReference));
            }

            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            estimateReference.BAccountID = order.CustomerID;
            estimateReference.QuoteType = order.OrderType;
            estimateReference.QuoteNbr = order.OrderNbr;
            estimateReference.CuryInfoID = order.CuryInfoID;
            estimateReference.ExternalRefNbr = string.IsNullOrWhiteSpace(order.CustomerRefNbr)
#pragma warning disable CS0618 // Type or member is obsolete
                ? order.ExtRefNbr
#pragma warning restore CS0618 // Type or member is obsolete
                : order.CustomerRefNbr;

            return estimateReference;
        }

        protected override AMEstimateHistory MakeQuoteEstimateHistory(AMEstimateItem amEstimateItem, bool estimateCreated)
        {
            var currentOrder = (SOOrder)Base?.Document?.Current;
            if (currentOrder == null)
            {
                return null;
            }

            if (estimateCreated)
            {
                return new AMEstimateHistory
                {
                    RevisionID = amEstimateItem.RevisionID.TrimIfNotNullEmpty(),
                    Description = Messages.GetLocal(Messages.EstimateCreatedFromSalesOrder,
                        amEstimateItem.RevisionID.TrimIfNotNullEmpty(),
                        currentOrder.OrderType,
                        currentOrder.OrderNbr)
                };
            }

            return new AMEstimateHistory
            {
                EstimateID = amEstimateItem.EstimateID.TrimIfNotNullEmpty(),
                RevisionID = amEstimateItem.RevisionID.TrimIfNotNullEmpty(),
                Description = Messages.GetLocal(Messages.EstimateAddedToSalesOrder,
                    amEstimateItem.RevisionID.TrimIfNotNullEmpty(),
                    currentOrder.OrderType,
                    currentOrder.OrderNbr)
            };
        }

        protected override void RemoveEstimateFromQuote(EstimateMaint estimateGraph)
        {
            var currentOrder = (SOOrder)Base?.Document?.Current;
            if (currentOrder == null || string.IsNullOrWhiteSpace(estimateGraph?.Documents?.Current?.EstimateID))
            {
                return;
            }

            estimateGraph.EstimateHistoryRecords.Insert(new AMEstimateHistory
            {
                EstimateID = estimateGraph.Documents.Current.EstimateID.TrimIfNotNullEmpty(),
                RevisionID = estimateGraph.Documents.Current.RevisionID.TrimIfNotNullEmpty(),
                Description = Messages.GetLocal(Messages.EstimateRemovedFromSalesOrder,
                    currentOrder.OrderType,
                    currentOrder.OrderNbr)
            });

            if (estimateGraph.IsDirty && estimateGraph.EstimateReferenceRecord?.Current != null)
            {
                var estGraphHelper = new EstimateGraphHelper(estimateGraph);
                estGraphHelper.PersistSOOrderEntryRemove(Base, new List<AMEstimateReference> { estimateGraph.EstimateReferenceRecord.Current });
                //press cancel only for "refresh"
                Base.Actions.PressCancel();
            }
        }

        [PXUIField(DisplayName = Messages.Add, MapEnableRights = PXCacheRights.Insert,
                MapViewRights = PXCacheRights.Insert)]
        [PXButton]
        protected override IEnumerable addEstimate(PXAdapter adapter)
        {
            if (Base.Document?.Current?.CustomerID == null)
            {
                return adapter.Get();
            }

            if (OrderEstimateItemFilter.AskExt() == WebDialogResult.OK)
            {
                var estimateGraph = AddEstimateToQuote(OrderEstimateItemFilter.Current, Base.Document.Current);
                if (estimateGraph?.EstimateReferenceRecord?.Current != null)
                {
                    var estRef = estimateGraph.EstimateReferenceRecord.Current;
                    estRef.QuoteType = Base.Document.Current.OrderType;
                    estRef.QuoteNbr = Base.Document.Current.OrderNbr;
                    estimateGraph.EstimateReferenceRecord.Update(estRef);
                }

                var estGraphHelper = new EstimateGraphHelper(estimateGraph);
                estGraphHelper.PersistSOOrderEntry(Base,
                    OrderEstimateItemFilter.Current.AddExisting.GetValueOrDefault()
                        ? EstimateReferenceOrderAction.Add
                        : EstimateReferenceOrderAction.New);
                //press cancel only for "refresh"
                Base.Actions.PressCancel();
            }

            OrderEstimateItemFilter.Cache.Clear();

            return adapter.Get();
        }

        /// <summary>
        /// Extending the order copy process
        /// </summary>
        [PXOverride]
        public virtual void CopyOrderProc(SOOrder order, CopyParamFilter copyFilter, Action<SOOrder, CopyParamFilter> del)
        {
            if (del == null)
            {
                return;
            }

            SOOrderType fromOrderType = Base.soordertype.Current;
            if (fromOrderType == null)
            {
                fromOrderType = PXSelect<SOOrderType, Where<SOOrderType.orderType, Equal<Required<SOOrderType.orderType>>>>.Select(Base, order.OrderType);
            }

            var filterExtension = PXCache<CopyParamFilter>.GetExtension<SOCopyParamFilterExt>(copyFilter);
            bool toOrderTypeAllowsConfigs = OrderTypeAllowConfigurations((SOOrderType)PXSelectorAttribute.Select<CopyParamFilter.orderType>(Base.copyparamfilter.Cache, copyFilter));
            IsCopyConfig = filterExtension?.CopyConfigurations == true && toOrderTypeAllowsConfigs && AllowConfigurations;

            Base.RowSelecting.AddHandler<SOLine>(
                delegate (PXCache sender, PXRowSelectingEventArgs e)
                {
                    var row = e.Row as SOLine;
                    if (row == null)
                    {
                        return;
                    }

                    var ext = PXCache<SOLine>.GetExtension<SOLineExt>(row);
                    if (ext == null)
                    {
                        return;
                    }
                    ext.AMOrderType = null;
                    ext.AMProdOrdID = null;
                    ext.AMEstimateID = null;
                    ext.AMEstimateRevisionID = null;
                    ext.AMConfigurationID = null;
                    ext.AMOrigParentLineNbr = ext.AMParentLineNbr;
                    if (!IsCopyConfig)
                    {
                        ext.AMParentLineNbr = null;
                        ext.AMIsSupplemental = false;
                        ext.AMConfigurationID = null;
                    }
                });

            if (IsCopyConfig)
            {
                var graphExt = Base.GetExtension<SOOrderEntryAMExtension>();
                graphExt.ItemConfiguration.RemoveSOLineHandlers();

                Base.RowInserting.AddHandler<SOLine>((cache, args) =>
                {
                    var soLine = (SOLine)args.Row;
                    if (soLine == null)
                    {
                        return;
                    }

                    var soLineExt = PXCache<SOLine>.GetExtension<SOLineExt>(soLine);
#if DEBUG
                    AMDebug.TraceWriteMethodName($"RowInserting: SOLine[{soLine.OrderType}-{soLine.OrderNbr}-{soLine.LineNbr}] From Order Line: [{soLine.OrigOrderType}-{soLine.OrigOrderNbr}-{soLine.OrigLineNbr}] IsSup={soLineExt?.AMIsSupplemental}; InventoryID={soLine.InventoryID}");
#endif
                    if (soLineExt == null || !soLineExt.AMIsSupplemental.GetValueOrDefault() || soLine.OrigLineNbr == null || soLineExt.AMOrigParentLineNbr == null)
                    {
                        return;
                    }

                    // ***
                    // *** Continue only for supplemental line items coming from the copied order...
                    // ***

                    var parent = ConfigSupplementalItemsHelper.FindParentConfigLineByOrigParentLineNbr(cache, soLine);
                    if (parent == null)
                    {
                        return;
                    }

                    //Check for inserted supps already and delete those lines that match by parent (no orig parent) and inventory item
                    var sup = ConfigSupplementalItemsHelper.GetInsertedSupplementalLinesByParent((SOOrderEntry)cache.Graph, parent.LineNbr);

                    int? inventoryID = soLine.InventoryID;
                    if (sup != null && inventoryID == null)
                    {
                        // Find from to compare for inventory ID...
                        SOLine fromSoLine = PXSelect<SOLine,
                            Where<SOLine.orderType, Equal<Required<SOLine.orderType>>,
                                And<SOLine.orderNbr, Equal<Required<SOLine.orderNbr>>,
                                    And<SOLine.lineNbr, Equal<Required<SOLine.lineNbr>>>>>
                                    >.SelectWindowed(Base, 0, 1, soLine.OrigOrderType, soLine.OrigOrderNbr, soLine.OrigLineNbr);
                        if (fromSoLine != null)
                        {
                            inventoryID = fromSoLine.InventoryID;
                        }
                    }

                    if (sup != null && inventoryID != null)
                    {
                        var recalcUnitPrices = copyFilter.RecalcUnitPrices.GetValueOrDefault();
                        var overrideManualPrices = copyFilter.OverrideManualPrices.GetValueOrDefault();
                        var recalcDiscounts = copyFilter.RecalcDiscounts.GetValueOrDefault();
                        var overrideManualDiscounts = copyFilter.OverrideManualDiscounts.GetValueOrDefault();

                        foreach (var supSoLine in sup)
                        {
                            var supSoLineExt = PXCache<SOLine>.GetExtension<SOLineExt>(supSoLine);
                            if (supSoLine.InventoryID == inventoryID
                                //Sups inserted from config copy do not have an orig line pointing to a product from opportunity... we want only these sups...
                                && supSoLineExt != null && supSoLineExt.AMOrigParentLineNbr == null)
                            {
                                var copy = (SOLine)cache.CreateCopy(supSoLine);
                                copy.TranDesc = soLine.TranDesc;
                                copy.Qty = soLine.Qty.GetValueOrDefault();
                                copy.UOM = soLine.UOM;
                                copy.CuryUnitPrice = soLine.CuryUnitPrice;
                                copy.TaxCategoryID = soLine.TaxCategoryID;
                                copy.SiteID = soLine.SiteID;
                                copy.IsFree = soLine.IsFree;
                                copy.ProjectID = soLine.ProjectID;
                                copy.TaskID = soLine.TaskID;

								if (copy.SalesAcctID == null && soLine.SalesAcctID != null)
								{
									copy.SalesAcctID = soLine.SalesAcctID;
								}

								if (copy.SalesSubID == null && soLine.SalesSubID != null)
								{
									copy.SalesSubID = soLine.SalesSubID;
								}

                                if (!overrideManualDiscounts)
                                {
                                    copy.ManualDisc = soLine.ManualDisc;
                                    copy.ManualPrice = soLine.ManualPrice;
                                }

                                if (!recalcUnitPrices && copy.ManualPrice.GetValueOrDefault())
                                {
                                    copy.CuryUnitPrice = soLine.CuryUnitPrice;
                                    copy.CuryExtPrice = soLine.CuryExtPrice;
                                }

                                if (!overrideManualPrices)
                                {
                                    copy.ManualPrice = soLine.ManualPrice;
                                }

                                if (!recalcDiscounts)
                                {
                                    copy.ManualDisc = soLine.ManualDisc;
                                }

                                var copyExt = PXCache<SOLine>.GetExtension<SOLineExt>(copy);
                                if (copyExt != null)
                                {
                                    copyExt.AMOrigParentLineNbr = soLineExt.AMOrigParentLineNbr;
                                }
#if DEBUG
                                AMDebug.TraceWriteMethodName($"Copying [{soLine.OrderType}-{soLine.OrderNbr}-{soLine.LineNbr}] to [{supSoLine.OrderType}-{supSoLine.OrderNbr}-{supSoLine.LineNbr}]");
#endif
                                cache.Update(copy);
                            }
                        }
                    }
#if DEBUG
                    AMDebug.TraceWriteMethodName($"Cancel SOLine[{soLine.OrderType}-{soLine.OrderNbr}-{soLine.LineNbr}]");
#endif
                    //CANCEL ALL COPIED SUPS FROM SOURCE LINE AS THE COPY CONFIG WILL RE-ADD THE CORRECT SUPS TO ACCOUNT FOR CONFIG CHANGES...
                    args.Cancel = true;
                });

            }

            try
            {
                del(order, copyFilter);
            }
            finally
            {
                IsCopyConfig = false;
            }

            //Clear extension header fields...
            if (Base.Document.Current != null && SOOrderExt.IsActive())
            {
                var newOrderExt = PXCache<SOOrder>.GetExtension<SOOrderExt>(Base.Document.Current);
                if (newOrderExt != null && (newOrderExt.AMCuryEstimateTotal.GetValueOrDefault() != 0 ||
                                            newOrderExt.AMEstimateTotal.GetValueOrDefault() != 0 ||
                                            newOrderExt.AMEstimateQty.GetValueOrDefault() != 0))
                {
                    newOrderExt.AMCuryEstimateTotal = 0;
                    newOrderExt.AMEstimateTotal = 0;
                    newOrderExt.AMEstimateQty = 0;
                    Base.Document.Update(Base.Document.Current);
                }
            }

            if (Base.Document.Current == null || copyFilter == null || order == null || !Features.EstimatingEnabled())
            {
                return;
            }

            SOOrder oldOrder = PXSelect<SOOrder,
                Where<SOOrder.orderType, Equal<Required<SOOrder.orderType>>,
                    And<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>
                    >.Select(Base, order.OrderType, order.OrderNbr);

            SOOrderType newOrderType = PXSelect<SOOrderType, Where<SOOrderType.orderType, Equal<Required<SOOrderType.orderType>>>>.Select(Base, copyFilter.OrderType);
            if (newOrderType == null)
            {
                return;
            }

            var estimateAction = SOCopyParamFilterExt.DetermineAction(copyFilter, fromOrderType, newOrderType);
            if (estimateAction == SOCopyParamFilterExt.EstimateAction.NoAction)
            {
                return;
            }

            var sourceEstimates = new List<AMEstimateItem>();
            foreach (PXResult<AMEstimateReference, AMEstimateItem> result in PXSelectJoin<AMEstimateReference,
                InnerJoin<AMEstimateItem, On<AMEstimateReference.estimateID, Equal<AMEstimateItem.estimateID>,
                    And<AMEstimateReference.revisionID, Equal<AMEstimateItem.revisionID>>>>,
                Where<AMEstimateItem.quoteSource, Equal<EstimateSource.salesOrder>,
                        And<AMEstimateReference.quoteType, Equal<Required<SOOrder.orderType>>,
                        And<AMEstimateReference.quoteNbr, Equal<Required<SOOrder.orderNbr>>>>>>.Select(Base,
                            order.OrderType, order.OrderNbr))
            {
                var estimateReference = (AMEstimateReference)result;
                var estimateItem = (AMEstimateItem)result;

                if (string.IsNullOrWhiteSpace(estimateReference?.EstimateID) || string.IsNullOrWhiteSpace(estimateItem?.EstimateID))
                {
                    continue;
                }

                if ((estimateItem.IsNonInventory.GetValueOrDefault() || estimateItem.InventoryID == null)
                    && estimateAction == SOCopyParamFilterExt.EstimateAction.ConvertAction)
                {
                    throw new PXException(Messages.UnableToConvertToOrderNonInventory,
                        estimateItem.InventoryCD.TrimIfNotNullEmpty(), estimateItem.EstimateID.TrimIfNotNullEmpty(), estimateItem.RevisionID.TrimIfNotNullEmpty());
                }

                sourceEstimates.Add(estimateItem);
            }

            if (sourceEstimates.Count == 0)
            {
                return;
            }

            if (estimateAction == SOCopyParamFilterExt.EstimateAction.CopyAction)
            {
                UpdateEstimateReferenceStatus(oldOrder, true);
                CopyEstimates(sourceEstimates);

                Base.Persist();

                return;
            }

            //is the from order completed or canceled?
            SOOrder fromOrder = PXSelect<SOOrder,
                Where<SOOrder.orderType, Equal<Required<SOOrder.orderType>>,
                    And<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>>.Select(Base, order.OrderType, order.OrderNbr);

            // at this point assumes convert...
            ConvertEstimates(Base, sourceEstimates, GetEstRefStatus(fromOrder));
        }

        protected virtual int GetEstRefStatus(SOOrder order)
        {
            if (order.Cancelled.GetValueOrDefault())
            {
                return EstimateStatus.Canceled;
            }

            if (order.Completed.GetValueOrDefault())
            {
                return EstimateStatus.Closed;
            }

            return EstimateStatus.NewStatus;
        }

        protected virtual void CopyEstimates(List<AMEstimateItem> sourceEstimates)
        {
            var soOrder = Base?.Document?.Current;
            if (soOrder == null)
            {
                return;
            }

            var graph = PXGraph.CreateInstance<EstimateCopy>();
            AMEstimateSetup estimateSetup = PXSelect<AMEstimateSetup>.Select(graph);

            if (estimateSetup == null)
            {
                throw new EstimatingSetupNotEnteredException();
            }

            Numbering numbering = PXSelect<Numbering,
                Where<Numbering.numberingID, Equal<Required<Numbering.numberingID>>>
                >.Select(graph, estimateSetup.EstimateNumberingID);

            if (numbering == null)
            {
                throw new EstimatingSetupNotEnteredException(Messages.GetLocal(Messages.EstimateNumberingSequenceIncorrect));
            }

            if (numbering.UserNumbering.GetValueOrDefault())
            {
                throw new PXException(Messages.EstimateCopyRequiresAutoNumber, numbering.NumberingID);
            }

            foreach (var sourceEstimate in sourceEstimates)
            {
                var sourceEstReference = AMEstimateReference.PK.FindDirty(Base, sourceEstimate.EstimateID, sourceEstimate.RevisionID);

                graph.Clear();
                graph.CreateNewEstimate(sourceEstimate, SetEstimateReference(new AMEstimateReference { TaxLineNbr = sourceEstReference?.TaxLineNbr }, soOrder));
                var insertedEstReference = (AMEstimateReference)graph.EstimateReferenceRecord.Cache.Inserted.FirstOrDefault_();

                // Must save the estimate reference with the sales order so taxes and totals are calculated correctly
                if (insertedEstReference?.RevisionID != null)
                {
                    graph.EstimateReferenceRecord.Cache.Remove(insertedEstReference);

                    var copyOfInserted = PXCache<AMEstimateReference>.CreateCopy(insertedEstReference);

                    insertedEstReference.OrderQty = 0m;
                    insertedEstReference.CuryExtPrice = 0m;
                    insertedEstReference.ExtPrice = 0m;
                    insertedEstReference.CuryUnitPrice = 0m;
                    insertedEstReference.UnitPrice = 0m;
                    insertedEstReference.TaxCategoryID = null;
                    insertedEstReference = OrderEstimateRecords.Insert(insertedEstReference);
                    insertedEstReference.TaxCategoryID = copyOfInserted.TaxCategoryID;
                    insertedEstReference.OrderQty = copyOfInserted.OrderQty;
                    insertedEstReference.CuryExtPrice = copyOfInserted.CuryExtPrice;
                    insertedEstReference.CuryUnitPrice = copyOfInserted.CuryUnitPrice;
                    OrderEstimateRecords.Update(insertedEstReference);
                }

                // This will create estimates even if the main copied quote does not create estimates. 
                // We need to move the copy logic or at least the cache inserts into the Base graph and not persist in this secondary graph. We also need the EstimateID auto number to work  
                graph.Persist();
            }
        }

        protected static decimal? GetOrderCuryValue(PXGraph graph, SOOrder order, long? sourceCuryInfoID, string sourceCuryID, decimal? sourceAmount)
        {
            if (string.IsNullOrWhiteSpace(sourceCuryID)
                || order.CuryID.EqualsWithTrim(sourceCuryID))
            {
                return sourceAmount;
            }

            return CurrencyHelper.ConvertFromToCury(graph, order.CuryInfoID, sourceCuryInfoID, sourceAmount);
        }

        public static void AddEstimateToOrder(SOOrder order, AMEstimateItem estimateItem)
        {
            AddEstimateToOrder(order.OrderType, order.OrderNbr, estimateItem);
        }

        public static void AddEstimateToOrder(string orderType, string orderNbr, AMEstimateItem estimateItem)
        {
            if (string.IsNullOrWhiteSpace(orderType))
            {
                throw new PXArgumentException(nameof(orderType));
            }

            if (string.IsNullOrWhiteSpace(orderNbr))
            {
                throw new PXArgumentException(nameof(orderNbr));
            }

            if (estimateItem == null)
            {
                throw new PXArgumentException(nameof(estimateItem));
            }

            var graph = PXGraph.CreateInstance<SOOrderEntry>();
            graph.Document.Current = graph.Document.Search<SOOrder.orderNbr>(orderNbr, orderType);

            if (graph.Document.Current == null)
            {
                throw new PXException(Messages.OrderNotFound);
            }

            //make sure we are not adding to a completed/canceled order
            if (graph.Document.Current.Completed.GetValueOrDefault()
                || graph.Document.Current.Cancelled.GetValueOrDefault())
            {
                throw new PXException(Messages.CannotAddToClosedOrder);
            }

            AddEstimateAsSalesLine(graph, estimateItem);

            if (graph.IsDirty)
            {
                graph.Actions.PressSave();
            }
        }

        public static void AddEstimateAsSalesLine(SOOrderEntry orderEntryGraph, AMEstimateItem estimateItem)
        {
            AddEstimateAsSalesLine(orderEntryGraph, estimateItem, estimateItem.EstimateStatus ?? EstimateStatus.NewStatus);
        }

        public static void AddEstimateAsSalesLine(SOOrderEntry orderEntryGraph, AMEstimateItem estimateItem, int newEstimateRefStatus)
        {
            if (orderEntryGraph?.Document.Current == null || estimateItem == null || estimateItem.IsNonInventory.GetValueOrDefault() || estimateItem.InventoryID == null || !Features.EstimatingEnabled())
            {
                return;
            }

            AMEstimateReferenceSO estimateRef = PXSelect<AMEstimateReferenceSO,
                    Where<AMEstimateReferenceSO.estimateID, Equal<Required<AMEstimateReferenceSO.estimateID>>,
                        And<AMEstimateReferenceSO.revisionID, Equal<Required<AMEstimateReferenceSO.revisionID>>>>>
                .Select(orderEntryGraph, estimateItem.EstimateID, estimateItem.RevisionID);

            if (estimateRef == null)
            {
                return;
            }

            var newLine = orderEntryGraph.Transactions.Insert(new SOLine { BranchID = estimateRef.BranchID });

            newLine.InventoryID = estimateItem.InventoryID;
            if (InventoryHelper.SubItemFeatureEnabled)
            {
                newLine.SubItemID = estimateItem.SubItemID;
            }
            newLine.UOM = estimateItem.UOM;
            newLine.SiteID = estimateItem.SiteID;
            newLine.TranDesc = estimateItem.ItemDesc;
            newLine.TaxCategoryID = null;
            newLine.ManualPrice = true;
            newLine.ManualDisc = true;
            newLine.IsFree = false;
            newLine.OrderQty = estimateItem.OrderQty;
            newLine.CuryUnitPrice = estimateItem.CuryUnitPrice;
            newLine.TaxCategoryID = estimateRef.TaxCategoryID;

            //Convert Unit Price...
            var convertedCuryUnitPrice = GetOrderCuryValue(orderEntryGraph, orderEntryGraph.Document.Current, estimateRef.CuryInfoID, estimateItem.CuryID, estimateRef.CuryUnitPrice);
            if (convertedCuryUnitPrice != null)
            {
                newLine.CuryUnitPrice = convertedCuryUnitPrice;
            }

            SOLineExt newLineExtension = PXCache<SOLine>.GetExtension<SOLineExt>(newLine);
            newLineExtension.AMEstimateID = estimateItem.EstimateID;
            newLineExtension.AMEstimateRevisionID = estimateItem.RevisionID;
            
            newLine = orderEntryGraph.Transactions.Update(newLine);

            bool updateRef = false;
            if (string.IsNullOrWhiteSpace(estimateRef.OrderNbr))
            {
                estimateRef.OrderType = orderEntryGraph.Document.Current.OrderType;
                estimateRef.OrderNbr = orderEntryGraph.Document.Current.OrderNbr;
                updateRef = true;
            }

            if (estimateRef.EstimateStatus != newEstimateRefStatus)
            {
                estimateRef.EstimateStatus = newEstimateRefStatus;
                updateRef = true;
            }

            if (estimateRef.BAccountID == null)
            {
                estimateRef.BAccountID = orderEntryGraph.Document.Current.CustomerID;
                updateRef = true;
            }
            string soRef = string.IsNullOrWhiteSpace(orderEntryGraph.Document.Current.CustomerRefNbr)
#pragma warning disable CS0618 // Type or member is obsolete
                ? orderEntryGraph.Document.Current.ExtRefNbr
#pragma warning restore CS0618 // Type or member is obsolete
                : orderEntryGraph.Document.Current.CustomerRefNbr;
            if (string.IsNullOrWhiteSpace(estimateRef.ExternalRefNbr)
                && !string.IsNullOrWhiteSpace(soRef))
            {
                estimateRef.ExternalRefNbr = soRef;
                updateRef = true;
            }

            if (updateRef)
            {
                var graphExtension = orderEntryGraph.GetExtension<SOOrderEntryAMExtension>();
                graphExtension.DocDetailEstimateRecords.Update(estimateRef);
            }
        }

        protected static void ConvertEstimates(SOOrderEntry orderEntryGraph, List<AMEstimateItem> sourceEstimates, int newEstimateRefStatus)
        {
            if (sourceEstimates == null || sourceEstimates.Count == 0)
            {
                return;
            }
#if DEBUG
            AMDebug.TraceWriteMethodName($"Convert {sourceEstimates.Count} estimates to order '{orderEntryGraph.Document.Current.OrderType.TrimIfNotNullEmpty()}' '{orderEntryGraph.Document.Current.OrderNbr.TrimIfNotNullEmpty()}'");
#endif

            //for some silly reason the current changes on us when the insert of the first sales line takes place....
            SOOrder currentSOOrder = orderEntryGraph.Document.Current;
            foreach (var sourceEstimate in sourceEstimates)
            {
                AddEstimateAsSalesLine(orderEntryGraph, sourceEstimate, newEstimateRefStatus);

                if (orderEntryGraph.Document.Current != null
                    && !orderEntryGraph.Document.Current.OrderNbr.EqualsWithTrim(currentSOOrder.OrderNbr))
                {
                    orderEntryGraph.Document.Current = currentSOOrder;
                }
            }
            if (orderEntryGraph.IsDirty)
            {
                try
                {
                    orderEntryGraph.Actions.PressSave();
                }
                catch (Exception e)
                {
                    PXTraceHelper.PxTraceException(e);
                    orderEntryGraph.Actions.PressCancel();
                    throw;
                }
            }
        }

        /// <summary>
        /// Update related configured sales line from configuration data.
        /// Ex: unit price, transaction description, etc.
        /// </summary>
        /// <param name="graph">Sales Order Graph</param>
        /// <param name="line">sales line to update</param>
        /// <param name="configResults">configuration result</param>
        /// <param name="calledFromSalesOrder">Set true when call originated from Sales Order Entry graph/UI</param>
        public static void RefreshConfiguredSalesLine(SOOrderEntry graph, SOLine line, AMConfigurationResults configResults, bool calledFromSalesOrder)
        {
            if (graph.Document.Current == null
                || graph.Document.Current.OrderNbr != line.OrderNbr)
            {
                return;
            }

            if (configResults == null)
            {
                throw new PXArgumentException(nameof(configResults));
            }

            var graphExt = graph.GetExtension<SOOrderEntryAMExtension>();
            graphExt.ItemConfiguration.RemoveSOLineHandlers();

            graph.Transactions.Current = line;
            if (!string.IsNullOrWhiteSpace(configResults.TranDescription))
            {
                graph.Transactions.Cache.SetValueExt<SOLine.tranDesc>(line, configResults.TranDescription);
            }
            var lineExt = PXCache<SOLine>.GetExtension<SOLineExt>(line);
            if (lineExt != null)
            {
                graph.Transactions.Cache.SetValueExt<SOLineExt.aMConfigKeyID>(line, configResults.KeyID);
            }
            if (!calledFromSalesOrder)
            {
                graph.Transactions.Update(line);
            }

            var locatedLine = graph.Transactions.Cache.LocateElse(line);
            graphExt.SetConfiguredLineUnitPrice(graph.Transactions.Cache, locatedLine, graphExt.GetConfiguration(line), calledFromSalesOrder && !graph.IsCopyOrder);

            if (!graph.IsCopyOrder)
            {
                graphExt.ItemConfiguration.AddSOLineHandlers();
            }
        }

        /// <summary>
        /// Hyper-link on EstimateID to open the estimate page
        /// </summary>
        public PXAction<SOOrder> ViewEstimate;
        [PXButton]
        [PXUIField(DisplayName = "View Estimate", Visible = false)]
        protected virtual void viewEstimate()
        {
            if (OrderEstimateRecords.Current?.EstimateID == null)
            {
                return;
            }

            EstimateMaint.Redirect(OrderEstimateRecords.Current.EstimateID, OrderEstimateRecords.Current.RevisionID);
        }

        public PXAction<SOOrder> CTPButton;
        [PXButton(OnClosingPopup = PXSpecialButtonType.Cancel)]
        [PXUIField(DisplayName = "Process CTP", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        protected virtual void cTPButton()
        {
            if (Base.IsDirty)
            {
                Base.Persist();
            }

            var graph = PXGraph.CreateInstance<CTPProcess>();
            graph.Filter.Current.SOOrderType = Base.Document.Current?.OrderType;
            graph.Filter.Current.SOOrderNbr = Base.Document.Current?.OrderNbr;
            graph.Filter.Current.Redirect = true;
            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
        }

        [PXButton(OnClosingPopup = PXSpecialButtonType.Cancel, Tooltip = "Launch configuration entry", DisplayOnMainToolbar = false)]
        [PXUIField(DisplayName = Messages.Configure, MapEnableRights = PXCacheRights.Update,
            MapViewRights = PXCacheRights.Update)]
        protected override void configureEntry()
        {
            if (Base.Transactions.Current == null)
            {
                return;
            }

            var soLineExt = PXCache<SOLine>.GetExtension<SOLineExt>(Base.Transactions.Current);
            if (!AllowConfigurations
                || !soLineExt.IsConfigurable.GetValueOrDefault())
            {
                throw new PXException(Messages.NotConfigurableItem);
            }

            if (Base.IsDirty)
            {
                Base.Persist();
            }

            AMConfigurationResults configuration = ItemConfiguration.Select();
            if (configuration != null)
            {
                var graph = PXGraph.CreateInstance<ConfigurationEntry>();
                graph.Results.Current = graph.Results.Search<AMConfigurationResults.configResultsID>(configuration.ConfigResultsID);
                PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.Popup);
            }
        }

        public virtual AMConfigurationResults GetConfiguration(SOLine soLine)
        {
            if (soLine == null)
            {
                throw new PXArgumentException(nameof(soLine));
            }

            AMConfigurationResults configurationResult = PXSelect<AMConfigurationResults,
                    Where<AMConfigurationResults.ordTypeRef, Equal<Required<AMConfigurationResults.ordTypeRef>>,
                        And<AMConfigurationResults.ordNbrRef, Equal<Required<AMConfigurationResults.ordNbrRef>>,
                            And<AMConfigurationResults.ordLineRef, Equal<Required<AMConfigurationResults.ordLineRef>>>>>
                >.SelectWindowed(Base, 0, 1, soLine.OrderType, soLine.OrderNbr, soLine.LineNbr);

            return configurationResult;
        }

        /// <summary>
        /// Copy configuration results from one sales line to another
        /// </summary>
        public virtual void CopyConfiguration(SOLine toSOLine, SOLine fromSOLine)
        {
            if (fromSOLine == null)
            {
                throw new PXArgumentException(nameof(fromSOLine));
            }

            if (toSOLine == null)
            {
                throw new PXArgumentException(nameof(toSOLine));
            }

            var fromConfig = GetConfiguration(fromSOLine);
            if (fromConfig == null)
            {
                return;
            }

            var toConfig = GetConfiguration(toSOLine);
            if (toConfig == null && Base.Transactions.Cache.GetStatus(toSOLine) == PXEntryStatus.Inserted)
            {
                //Row is currently inserting/ed and new configuration template not yet inserted...
                UpdateConfigurationResult(toSOLine, null);
                toConfig = GetConfiguration(toSOLine);
            }

            CopyConfiguration(toSOLine, toConfig, fromConfig);
        }

        /// <summary>
        /// Copy configuration results from one sales line to another
        /// </summary>
        public virtual void CopyConfiguration(SOLine toSoLine, AMConfigurationResults toConfigResult, AMConfigurationResults fromConfigResult)
        {
            if (toSoLine == null)
            {
                throw new PXArgumentException(nameof(toSoLine));
            }

            if (toConfigResult == null || fromConfigResult == null)
            {
                return;
            }

            ConfigSupplementalItemsHelper.RemoveSOSupplementalLineItems(Base, toConfigResult);
			ItemConfiguration.RemoveRulesHandlers();
            ConfigurationCopyEngine.UpdateConfigurationFromConfiguration(Base, toConfigResult, fromConfigResult);
			ItemConfiguration.AddRulesHandlers();
			ItemConfiguration.RunRuleFormula(toConfigResult.ConfigResultsID < 0, out var _);

            //Try to "Finish" the configuration...
            var locatedConfig = ItemConfiguration.Locate(toConfigResult);
            if (locatedConfig != null)
            {
                string errorMessage;
                bool documentValid = ItemConfiguration.IsDocumentValid(out errorMessage);

                if (documentValid)
                {
                    locatedConfig.Completed = true;
                    ItemConfiguration.Update(locatedConfig);

                    ConfigurationSelect.UpdateSalesOrderWithConfiguredLineChanges(Base, toSoLine, locatedConfig, false);

                    return;
                }

                if (!Base.IsImport && !Base.IsContractBasedAPI)
                {
                    Base.Transactions.Cache.RaiseExceptionHandling<SOLine.inventoryID>(toSoLine, toSoLine.InventoryID,
                        new PXSetPropertyException(Messages.ConfigurationNeedsAttention, PXErrorLevel.Warning, errorMessage));
                }
            }
        }

        private static bool SalesLineMatchesConfiguration(SOLine soLine, AMConfigurationResults configResults)
        {
            return soLine != null && configResults != null &&
                   soLine.OrderType.EqualsWithTrim(configResults.OrdTypeRef) &&
                   soLine.OrderNbr.EqualsWithTrim(configResults.OrdNbrRef) &&
                   soLine.LineNbr == configResults.OrdLineRef;
        }

        private AMConfigurationResults FindConfigurationResults(SOLine row)
        {
            foreach (AMConfigurationResults rowInserted in ItemConfiguration.Cache.Inserted)
            {
                if (SalesLineMatchesConfiguration(row, rowInserted))
                {
#if DEBUG
                    AMDebug.TraceWriteMethodName($"Found Inserted Configuration {rowInserted.ConfigResultsID} for sales line {row.OrderType} - {row.OrderNbr} - {row.LineNbr}");
#endif
                    return ItemConfiguration.Cache.LocateElse(rowInserted);
                }
            }

            return ItemConfiguration.Cache.LocateElse((AMConfigurationResults)PXSelect<AMConfigurationResults,
                Where<AMConfigurationResults.ordTypeRef, Equal<Required<SOLine.orderType>>,
                    And<AMConfigurationResults.ordNbrRef, Equal<Required<SOLine.orderNbr>>,
                        And<AMConfigurationResults.ordLineRef, Equal<Required<SOLine.lineNbr>>>>>
            >.Select(Base, row?.OrderType, row?.OrderNbr, row?.LineNbr));
        }

        /// <summary>
        /// Update a given configuration to match the given sales line information.
        /// </summary>
        /// <param name="row"></param>
        private void UpdateConfigurationResult(SOLine row)
        {
            UpdateConfigurationResult(row, FindConfigurationResults(row));
        }

        /// <summary>
        /// Update a given configuration to match the given sales line information.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="configuration"></param>
        private void UpdateConfigurationResult(SOLine row, AMConfigurationResults configuration)
        {
            UpdateConfigurationResult(row, PXCache<SOLine>.GetExtension<SOLineExt>(row), configuration);
        }

        /// <summary>
        /// Update a given configuration to match the given sales line information.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="rowExt"></param>
        /// <param name="configuration"></param>
        private void UpdateConfigurationResult(SOLine row, SOLineExt rowExt, AMConfigurationResults configuration)
        {
            if (row == null || rowExt == null)
            {
                return;
            }

#if DEBUG
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
#endif
            string configurationID;
            var isSuccessful = ItemConfiguration.TryGetDefaultConfigurationID(row.InventoryID, row.SiteID, out configurationID);
            rowExt.AMConfigurationID = isSuccessful ? configurationID : null;

            var newConfigRequired = ConfigurationChangeRequired(rowExt.AMConfigurationID, configuration);
            if (!newConfigRequired)
            {
                return;
            }

            var isCurrentConfig = configuration != null;
            if (isCurrentConfig)
            {
                isCurrentConfig = ItemConfiguration.Delete(configuration) == null;
            }

            if (!string.IsNullOrEmpty(rowExt.AMConfigurationID) && !isCurrentConfig && AllowConfigurations)
            {
                InsertConfigurationResult(row, rowExt.AMConfigurationID);
            }
#if DEBUG
            }
            finally
            {
                sw.Stop();
                var debugMsg = PXTraceHelper.CreateTimespanMessage(sw.Elapsed, string.Join(":", "UpdateConfigurationResult", row?.OrderType, row?.OrderNbr, row?.LineNbr));
                PXTrace.WriteInformation(debugMsg);
                AMDebug.TraceWriteMethodName(debugMsg);
            }
#endif
        }

        private AMConfigurationResults InsertConfigurationResult(SOLine soLine, string configurationID)
        {
            if (soLine == null)
            {
                throw new PXArgumentException(nameof(soLine));
            }

            var configurationResult = new AMConfigurationResults
            {
                ConfigurationID = configurationID,
                InventoryID = soLine.InventoryID,
                OrdLineRef = soLine.LineNbr,
                Qty = soLine.OrderQty,
                UOM = soLine.UOM,
				SiteID = soLine.SiteID,
                CustomerID = soLine.CustomerID,
                CustomerLocationID = Base.Document?.Current?.CustomerLocationID
            };

            using (new DisableFormulaCalculationScope(ItemConfiguration.Cache))
            {
                return ItemConfiguration.Insert(configurationResult);
            }
        }

        public static void UpdateSupplementalLine(SOOrderEntry orderEntryGraph, SOLine supplementalLine, List<SOLine> soLineList)
        {
            var supplementalLineExt = PXCache<SOLine>.GetExtension<SOLineExt>(supplementalLine);

            foreach (SOLine soLine in soLineList)
            {
                var soLineExt = PXCache<SOLine>.GetExtension<SOLineExt>(soLine);
                if (soLineExt.AMConfigurationID != null && soLineExt.AMOrigParentLineNbr == supplementalLineExt.AMOrigParentLineNbr)
                {
                    supplementalLineExt.AMParentLineNbr = soLine.LineNbr;
                    orderEntryGraph.Transactions.Update(supplementalLine);
                    break;
                }
            }
        }

        protected override void PrimaryRowDeleted(PXCache cache, PXRowDeletedEventArgs e)
        {
            var row = (SOOrder)e.Row;
            if (row == null)
            {
                return;
            }

            RemoveEstimateReference(OrderEstimateRecords.Select(),
                Messages.GetLocal(Messages.EstimateRemovedFromSalesOrder,
                    row.OrderType,
                    row.OrderNbr));
        }

        public virtual void UpdateCustomer(SOOrder soOrder)
        {
            if (soOrder == null)
            {
                 return;
            }
            
            Common.Cache.AddCacheView<AMProdItem>(Base);
            
            foreach (AMProdItem prodItem in PXSelect<AMProdItem,
                                        Where<AMProdItem.ordTypeRef, Equal<Required<AMProdItem.ordTypeRef>>,
                                            And<AMProdItem.ordNbr, Equal<Required<AMProdItem.ordNbr>>
                                            >>>.Select(Base, soOrder.OrderType, soOrder.OrderNbr))
            {
                if (prodItem?.ProdOrdID != null)
                {
                    prodItem.CustomerID = soOrder.CustomerID;
                    Base.Caches[typeof(AMProdItem)].Update(prodItem);
                }
            }
        }

        protected virtual void UpdateProductionCustomer()
        {
            var currentOrder = Base.Document.Current;
            if (!AllowProductionOrders ||
                currentOrder?.CustomerID == null ||
                Base.Document.Cache.GetStatus(currentOrder) != PXEntryStatus.Updated)
            {
                return;
            }

            var origOrder = (SOOrder)Base.Document.Cache.GetOriginal(currentOrder);
            if (origOrder != null && origOrder.CustomerID != currentOrder.CustomerID)
            {
                UpdateCustomer(currentOrder);
            }
        }

        public virtual IEnumerable aMSOLineLinkRecords()
        {
            var lineExt = Base.Transactions.Current.GetExtension<SOLineExt>();

            List<AMProdItem> linkedLines = new List<AMProdItem>();
            
            foreach(AMProdItem prodItem in PXSelect<AMProdItem,
                    Where<AMProdItem.canceled, Equal<False>,                   
                    And<AMProdItem.completed, Equal<False>,
                    And<AMProdItem.closed, Equal<False>,
                    And2<Where<AMProdItem.orderType, Equal<Current<LinkProdOrderSelectFilter.orderType>>,
                        Or<Current<LinkProdOrderSelectFilter.orderType>, IsNull>>,
                    And2<Where<AMProdItem.prodOrdID, Equal<Current<LinkProdOrderSelectFilter.prodOrdID>>,
                        Or<Current<LinkProdOrderSelectFilter.prodOrdID>, IsNull>>,
                    And2<Where<AMProdItem.statusID, Equal<Current<LinkProdOrderSelectFilter.statusID>>,
                        Or<Current<LinkProdOrderSelectFilter.statusID>, IsNull>>,
                    And<AMProdItem.siteID, Equal<Current<SOLine.siteID>>,
                    And<AMProdItem.inventoryID, Equal<Current<SOLine.inventoryID>
                        >>>>>>>>>>.Select(Base))
            {
                if (prodItem.OrdNbr != null && prodItem.ProdOrdID != lineExt.AMProdOrdID)
                {
                    continue;
                }

                if (prodItem.OrderType == lineExt.AMOrderType && prodItem.ProdOrdID == lineExt.AMProdOrdID)
                {
                    prodItem.Selected = true;
                }

                linkedLines.Add(prodItem);
            }
           
            return linkedLines;
        }

        protected virtual void _(Data.Events.FieldUpdating<AMProdItem.selected> e)
        {
            var lineExt = Base.Transactions.Current.GetExtension<SOLineExt>();

            if (lineExt.AMOrderType != null || lineExt.AMProdOrdID != null)
            {
                e.NewValue = e.OldValue;
                return;
            }

            List<AMProdItem> prodItemList = AMSOLineLinkRecords.Cache.Cached.RowCast<AMProdItem>().Where(r => r.Selected == true).ToList();

            e.NewValue = prodItemList.Count == 0 ? e.NewValue : false;
        }

        protected virtual void ResetLinkProd()
        {
            linkProdOrderSelectFilter.Current = null;
            linkProdOrderSelectFilter.Cache.Clear();
            
            Base.Actions.PressSave();
        }

        /// <summary>
        /// Update to estimate reference record for document detail estimates. 
        /// Do not update for estimate tab estimate references.
        /// </summary>
        [PXProjection(typeof(Select2<AMEstimateReference,
			InnerJoin<AM.Standalone.AMEstimateItem,
			On<AM.Standalone.AMEstimateItem.estimateID, Equal<AMEstimateReference.estimateID>,
				And<AM.Standalone.AMEstimateItem.revisionID, Equal<AMEstimateReference.revisionID>>>,
			InnerJoin<AM.Standalone.AMEstimatePrimary, 
            On<AM.Standalone.AMEstimatePrimary.estimateID, Equal<AMEstimateReference.estimateID>>>>>), Persistent = true)]
        [Serializable]
        [PXHidden]
        public class AMEstimateReferenceSO : IBqlTable
        {
            #region Estimate ID
            public abstract class estimateID : PX.Data.BQL.BqlString.Field<estimateID> { }
            protected String _EstimateID;
            [PXDBDefault(typeof(AMEstimateItem.estimateID))]
            [EstimateID(IsKey = true, Enabled = false, BqlField = typeof(AMEstimateReference.estimateID))]
            [PXParent(typeof(Select<SOOrder,
                Where<SOOrder.orderType, Equal<Current<AMEstimateReferenceSO.orderType>>,
                And<SOOrder.orderNbr, Equal<Current<AMEstimateReferenceSO.orderNbr>>>>>), LeaveChildren = true)]
            public virtual String EstimateID
            {
                get { return this._EstimateID; }
                set { this._EstimateID = value; }
            }
            #endregion
            #region Revision ID
            public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }
            protected String _RevisionID;
            [PXDBDefault(typeof(AMEstimateItem.revisionID))]
            [PXDBString(10, IsKey = true, IsUnicode = true, InputMask = ">AAAAAAAAAA", BqlField = typeof(AMEstimateReference.revisionID))]
            [PXUIField(DisplayName = "Revision", Visibility = PXUIVisibility.SelectorVisible)]
            public virtual String RevisionID
            {
                get { return this._RevisionID; }
                set { this._RevisionID = value; }
            }
			#endregion
			#region Branch ID 
			public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
			protected Int32? _BranchID;
			[PXDBInt(BqlField = typeof(AM.Standalone.AMEstimateItem.branchID))]
			public virtual Int32? BranchID
			{
				get
				{
					return this._BranchID;
				}
				set
				{
					this._BranchID = value;
				}
			}
			#endregion
			#region QuoteSource
			public abstract class quoteSource : PX.Data.BQL.BqlInt.Field<quoteSource> { }
            [PXDBInt(BqlField = typeof(AM.Standalone.AMEstimatePrimary.quoteSource))]
            [PXDefault(EstimateSource.Estimate)]
            [PXUIField(DisplayName = "Quote Source", Enabled = false)]
            public virtual int? QuoteSource { get; set; }
            #endregion
            #region CuryInfoID
            public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
            protected Int64? _CuryInfoID;
            [PXDBLong(BqlField = typeof(AMEstimateReference.curyInfoID))]
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
            #region Order Type
            public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
            protected String _OrderType;
            [PXDBString(2, IsUnicode = true, BqlField = typeof(AMEstimateReference.orderType))]
            [PXUIField(DisplayName = "SO Order Type")]
            [PXDBDefault(typeof(SOOrder.orderType), PersistingCheck = PXPersistingCheck.Nothing)]
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
            #region Order Nbr
            public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
            protected String _OrderNbr;
            [PXDBString(15, IsUnicode = true, BqlField = typeof(AMEstimateReference.orderNbr))]
            [PXUIField(DisplayName = "SO Order Nbr")]
            [PXDBDefault(typeof(SOOrder.orderNbr), PersistingCheck = PXPersistingCheck.Nothing)]
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
            #region TaxLineNbr
            public abstract class taxLineNbr : PX.Data.BQL.BqlInt.Field<taxLineNbr> { }
            protected Int32? _TaxLineNbr;
            [PXDBInt(BqlField = typeof(AMEstimateReference.taxLineNbr))]
            [PXUIField(DisplayName = "Tax Line Nbr.", Visible = false, Enabled = false)]
            public virtual Int32? TaxLineNbr
            {
                get
                {
                    return this._TaxLineNbr;
                }
                set
                {
                    this._TaxLineNbr = value;
                }
            }
            #endregion
            #region Tax Category ID
            public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }
            protected String _TaxCategoryID;
            [PXDBString(TaxCategory.taxCategoryID.Length, IsUnicode = true, BqlField = typeof(AMEstimateReference.taxCategoryID))]
            [PXUIField(DisplayName = "Tax Category")]
            public virtual String TaxCategoryID
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
            #region Order Qty
            public abstract class orderQty : PX.Data.BQL.BqlDecimal.Field<orderQty> { }
            protected Decimal? _OrderQty;
            [PXDBQuantity(BqlField = typeof(AMEstimateReference.orderQty))]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Order Qty")]
            public virtual Decimal? OrderQty
            {
                get
                {
                    return this._OrderQty;
                }
                set
                {
                    this._OrderQty = value;
                }
            }
            #endregion
            #region Cury Unit Price
            public abstract class curyUnitPrice : PX.Data.BQL.BqlDecimal.Field<curyUnitPrice> { }
            protected Decimal? _CuryUnitPrice;
            [PXDBCurrency(typeof(Search<PX.Objects.CS.CommonSetup.decPlPrcCst>), typeof(AMEstimateReferenceSO.curyInfoID), typeof(AMEstimateReferenceSO.unitPrice), BqlField = typeof(AMEstimateReference.curyUnitPrice))]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Unit Price", Enabled = false)]
            public virtual Decimal? CuryUnitPrice
            {
                get
                {
                    return this._CuryUnitPrice;
                }
                set
                {
                    this._CuryUnitPrice = value;
                }
            }
            #endregion
            #region Unit Price
            public abstract class unitPrice : PX.Data.BQL.BqlDecimal.Field<unitPrice> { }
            protected Decimal? _UnitPrice;
            [PXDBPriceCost(BqlField = typeof(AMEstimateReference.unitPrice))]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Unit Price", Enabled = false)]
            public virtual Decimal? UnitPrice
            {
                get
                {
                    return this._UnitPrice;
                }
                set
                {
                    this._UnitPrice = value;
                }
            }
            #endregion
            #region Cury Ext Price
            public abstract class curyExtPrice : PX.Data.BQL.BqlDecimal.Field<curyExtPrice> { }
            protected Decimal? _CuryExtPrice;
            [PXDBCurrency(typeof(AMEstimateReferenceSO.curyInfoID), typeof(AMEstimateReferenceSO.extPrice), BqlField = typeof(AMEstimateReference.curyExtPrice))]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Total Price", Enabled = false)]
            [PXFormula(typeof(Mult<AMEstimateReferenceSO.curyUnitPrice, AMEstimateReferenceSO.orderQty>))]
            public virtual Decimal? CuryExtPrice
            {
                get
                {
                    return this._CuryExtPrice;
                }
                set
                {
                    this._CuryExtPrice = value;
                }
            }
            #endregion
            #region Ext Price
            public abstract class extPrice : PX.Data.BQL.BqlDecimal.Field<extPrice> { }
            protected Decimal? _ExtPrice;
            [PXDBBaseCury(BqlField = typeof(AMEstimateReference.extPrice))]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Total Price", Enabled = false)]
            public virtual Decimal? ExtPrice
            {
                get
                {
                    return this._ExtPrice;
                }
                set
                {
                    this._ExtPrice = value;
                }
            }
            #endregion
            #region BAccount ID
            public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
            protected Int32? _BAccountID;
            [PXDBInt(BqlField = typeof(AMEstimateReference.bAccountID))]
            public virtual Int32? BAccountID
            {
                get
                {
                    return this._BAccountID;
                }
                set
                {
                    this._BAccountID = value;
                }
            }
            #endregion
            #region EstimateStatus
            public abstract class estimateStatus : PX.Data.BQL.BqlInt.Field<estimateStatus> { }
            [PXDBInt(BqlField = typeof(AM.Standalone.AMEstimatePrimary.estimateStatus))]
            [PXDefault(AM.Attributes.EstimateStatus.NewStatus)]
            [PXUIField(DisplayName = "Estimate Status", Visibility = PXUIVisibility.SelectorVisible)]
            public virtual int? EstimateStatus { get; set; }
            #endregion
            #region External Ref Nbr
            public abstract class externalRefNbr : PX.Data.BQL.BqlString.Field<externalRefNbr> { }
            protected String _ExternalRefNbr;
            [PXDBString(15, IsUnicode = true, BqlField = typeof(AMEstimateReference.externalRefNbr))]
            [PXUIField(DisplayName = "Ext. Ref. Nbr.")]
            public virtual String ExternalRefNbr
            {
                get
                {
                    return this._ExternalRefNbr;
                }
                set
                {
                    this._ExternalRefNbr = value;
                }
            }
            #endregion
            #region PEstimateID
            /// <summary>
            /// EstimateID for AMEstimatePrimary
            /// </summary>
            public abstract class pEstimateID : PX.Data.BQL.BqlString.Field<pEstimateID> { }
            /// <summary>
            /// EstimateID for AMEstimatePrimary
            /// </summary>
            [PXExtraKey]
            [EstimateID(Visible = false, Enabled = false, Visibility = PXUIVisibility.Invisible, BqlField = typeof(AM.Standalone.AMEstimatePrimary.estimateID))]
            public virtual String PEstimateID
            {
                get { return EstimateID; }
                set { }
            }
            #endregion
        }

        [Serializable]
        [PXCacheName("Link Prod. Orders Filter")]
        public partial class LinkProdOrderSelectFilter : IBqlTable
        {
            #region OrderType
            public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

            [AMOrderTypeField]
            [PXRestrictor(typeof(Where<AMOrderType.active, Equal<True>>), PX.Objects.SO.Messages.OrderTypeInactive)]
            [PXSelector(typeof(Search<AMOrderType.orderType>))]
            public virtual String OrderType { get; set; }
            #endregion
            #region ProdOrdID
            public abstract class prodOrdID : PX.Data.BQL.BqlString.Field<prodOrdID> { }

            [ProductionNbr]
            [ProductionOrderSelector(typeof(LinkProdOrderSelectFilter.orderType), true)]
            public virtual String ProdOrdID { get; set; }

            #endregion
            #region StatusID
            public abstract class statusID : PX.Data.BQL.BqlString.Field<statusID> { }

            [PXDBString(1, IsFixed = true)]
            [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible)]
            [ProductionOrderStatus.List]
            public virtual String StatusID { get; set; }
            #endregion
        }
    }

	public class SoOrderEntryDRPExtension: PXGraphExtension<SOOrderEntry>
	{
		public static bool IsActive()
		{
			return Features.ManufacturingOrDRPOrReplenishmentEnabled();
		}

		protected virtual void _(Events.RowInserted<INItemSite> e)
		{
			if (e.Row.PlanningMethod.IsNotIn(INPlanningMethod.MRP, INPlanningMethod.DRP)) return;

			var row = e.Row;
			var rowExt = row.GetExtension<INItemSiteExt>();
			var invItem = InventoryItem.PK.Find(Base, row.InventoryID);
			var invItemExt = invItem.GetExtension<InventoryItemExt>();
			row.ReplenishmentSource = invItem.ReplenishmentSource;
			row.ReplenishmentSourceSiteID = invItemExt.AMSourceSiteID;
		}
	}
}
