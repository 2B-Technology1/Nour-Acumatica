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
using System.Collections;
using PX.Objects.AM.Attributes;
using PX.Objects.IN;
using PX.Objects.CS;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing Inventory Planning Detail Inquiry graph
    /// </summary>
    public class MRPDetail : PXGraph<MRPDetail>
    {
        public PXFilter<InvLookup> invlookup;
		public PXCancel<InvLookup> cancel;
        public PXSelectReadonly<
            AMRPPlan,
            Where<AMRPPlan.inventoryID, Equal<Current<InvLookup.inventoryID>>,
                And<AMRPPlan.siteID, Equal<Current<InvLookup.siteID>>,
                And<AMRPPlan.subItemID, Equal<Current<InvLookup.subItemID>>>>>,  
            OrderBy<
                Asc<AMRPPlan.promiseDate, 
                Asc<AMRPPlan.refNoteID>>>> 
            MRPRecs;

        [PXHidden]
        public PXSelect<
            AMRPItemSite,
            Where<AMRPItemSite.inventoryID, Equal<Current<InvLookup.inventoryID>>,
                And2<
                    Where<AMRPItemSite.subItemID, Equal<Current<InvLookup.subItemID>>,
                        Or<Not<FeatureInstalled<FeaturesSet.subItem>>>>,
                    And<AMRPItemSite.siteID, Equal<Current<InvLookup.siteID>>>>>> MRPInventory;

        [PXHidden]
        public PXSetup<AMRPSetup> amrpSetup;

		public MRPDetail()
		{
			var localDisplayName = amrpSetup.Current?.StockingMethod ==
						AMRPSetup.MRPStockingMethod.SafetyStock ? Messages.StockingMethodSafetyStock : Messages.StockingMethodReorderPoint;
			PXUIFieldAttribute.SetDisplayName<InvLookup.safetyStock>(invlookup.Cache, localDisplayName);
		}

		// For cache attached
		[PXHidden]
        public PXSelect<AMProdOper> ProdOper;

        #region CacheAttahed

        //Changing the production order keys for display of related document
        [OperationIDField(IsKey = false, Visible = false, Enabled = false)]
        protected virtual void _(Events.CacheAttached<AMProdOper.operationID> e) { }

        //Changing the production order keys for display of related document
        [OperationCDField(IsKey = true, Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void _(Events.CacheAttached<AMProdOper.operationCD> e) { }

		#endregion

		public PXAction<InvLookup> WarehouseDetails;
		[PXUIField(DisplayName = "Warehouse Details", Enabled = true)]
		[PXButton(CommitChanges = true)]
		public virtual IEnumerable warehouseDetails(PXAdapter adapter)
		{
			if(MRPRecs.Current != null)
			{
				var graph = CreateInstance<INItemSiteMaint>();
				var itemsite = INItemSite.PK.Find(graph, MRPRecs.Current.InventoryID, MRPRecs.Current.SiteID);
				graph.itemsiterecord.Current = itemsite;
				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.New);
			}

			return adapter.Get();
		}

		/// <summary>
		/// Redirect to MRPDetail graph for given filter
		/// </summary>
		/// <param name="filter"></param>
		public static void Redirect(InvLookup filter)
        {
            if (filter?.InventoryID == null || filter.SiteID == null)
            {
                throw new PXArgumentException(nameof(filter));
            }

            var graph = CreateInstance<MRPDetail>();
			graph.FieldVerifying.AddHandler<InvLookup.inventoryID>((sender, e) => { e.Cancel = true; });
			graph.FieldVerifying.AddHandler<InvLookup.siteID>((sender, e) => { e.Cancel = true; });

            graph.invlookup.Cache.SetValueExt<InvLookup.inventoryID>(graph.invlookup.Current, filter.InventoryID);
            graph.invlookup.Cache.SetValueExt<InvLookup.siteID>(graph.invlookup.Current, filter.SiteID);

            if (AM.InventoryHelper.SubItemFeatureEnabled)
            {
                if (filter.SubItemID == null)
                {
                    throw new PXArgumentException(nameof(filter));
                }
                graph.invlookup.Cache.SetValueExt<InvLookup.subItemID>(graph.invlookup.Current, filter.SubItemID);
            }

            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.New);
        }

        /// <summary>
        /// Is the filter incomplete or empty (unable to process detail data)
        /// </summary>
        public bool EmptyFilter => invlookup?.Current?.InventoryID == null || invlookup.Current.SiteID == null || invlookup.Current.SubItemID == null && AM.InventoryHelper.SubItemFeatureEnabled;

        protected virtual void InvLookup_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var invLookup = (InvLookup)e.Row;
            if (invLookup == null)
            {
                return;
            }
            sender.SetDefaultExt<InvLookup.uOM>(e.Row);
            sender.SetDefaultExt<InvLookup.siteID>(e.Row);
            sender.SetDefaultExt<InvLookup.subItemID>(e.Row);
            KeyFilterFieldsChanged();
        }

        protected virtual void InvLookup_SiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            KeyFilterFieldsChanged();
        }

        protected virtual void InvLookup_SubItemID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            KeyFilterFieldsChanged();
        }

		protected virtual void _(Events.RowSelected<InvLookup> e)
		{
			if (e.Row != null)
			{
				PXUIFieldAttribute.SetVisible<InvLookup.aMGroupWindow>(e.Cache, e.Row,
					amrpSetup.Current.UseDaysSupplytoConsolidateOrders ?? false);
			}			
		}

		protected virtual void KeyFilterFieldsChanged()
        {
            MRPRecs.Cache.Clear();
            SetFilterAMRPItemSiteFields();
        }

        protected virtual void SetFilterAMRPItemSiteFields()
        {
            invlookup.Current.QtyOnHand = 0m;
            invlookup.Current.MinOrderQty = 0m;
            invlookup.Current.MaxOrderQty = 0m;
            invlookup.Current.LotQty = 0m;
            invlookup.Current.SafetyStock = 0m;
			invlookup.Current.ReplenishmentSource = null;
			invlookup.Current.ReplenishmentSiteID = null;
			invlookup.Current.TransferLeadTime = 0;
			invlookup.Current.LeadTime = 0;
			invlookup.Current.AMGroupWindow = 0;

			MRPInventory.Current = MRPInventory.Select();

            if (MRPInventory.Current == null)
            {
				return;
            }

            invlookup.Current.QtyOnHand = MRPInventory.Current.QtyOnHand.GetValueOrDefault();
            invlookup.Current.MinOrderQty = MRPInventory.Current.MinOrdQty.GetValueOrDefault();
            invlookup.Current.MaxOrderQty = MRPInventory.Current.MaxOrdQty.GetValueOrDefault();
            invlookup.Current.LotQty = MRPInventory.Current.LotSize.GetValueOrDefault();
            invlookup.Current.SafetyStock = amrpSetup.Current.StockingMethod == AMRPSetup.MRPStockingMethod.SafetyStock
                ? MRPInventory.Current.SafetyStock.GetValueOrDefault() : MRPInventory.Current.ReorderPoint.GetValueOrDefault();
			invlookup.Current.ReplenishmentSource = MRPInventory.Current.ReplenishmentSource;
			invlookup.Current.ReplenishmentSiteID = MRPInventory.Current.ReplenishmentSiteID;
			invlookup.Current.TransferLeadTime = MRPInventory.Current.TransferLeadTime.GetValueOrDefault();
			invlookup.Current.LeadTime = MRPInventory.Current.LeadTime.GetValueOrDefault();
			invlookup.Current.AMGroupWindow = MRPInventory.Current.AMGroupWindow.GetValueOrDefault();
		}

        protected virtual IEnumerable mRPRecs()
        {
            if (invlookup.Current == null)
            {
                yield break;
            }

            if (EmptyFilter)
            {
                yield break;
            }

            var itVar1 = false;
            IEnumerator enumerator = this.MRPRecs.Cache.Inserted.GetEnumerator();
            while (enumerator.MoveNext())
            {
                AMRPPlan itVar2 = (AMRPPlan)enumerator.Current;
                itVar1 = true;
                yield return itVar2;
            }

            if (!itVar1)
            {
                PXSelectBase<AMRPPlan> amrpPlans = new PXSelect<AMRPPlan,
                    Where<AMRPPlan.inventoryID, Equal<Current<InvLookup.inventoryID>>,
                        And<AMRPPlan.siteID, Equal<Current<InvLookup.siteID>>>>,
                    OrderBy<Asc<AMRPPlan.promiseDate,
                        Asc<AMRPPlan.refNoteID>>>>(this);

                if (AM.InventoryHelper.SubItemFeatureEnabled)
                {
                    amrpPlans.WhereAnd<Where<AMRPPlan.subItemID, Equal<Current<InvLookup.subItemID>>>>();
                }

                var qtytot = invlookup.Current.QtyOnHand.GetValueOrDefault();

                foreach (AMRPPlan amrpPlan in amrpPlans.Select())
                {
                    var row = amrpPlan;

                    qtytot += row.BaseQty.GetValueOrDefault();
                    row.QtyOnHand = qtytot;

                    yield return row;
                }
            }
        }
    }

    /// <summary>
    /// Filter dac
    /// </summary>
    [Serializable]
    [PXCacheName(Messages.MRPDetailInventoryFilter)]
    public class InvLookup : IBqlTable
    {
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;

		/// <summary>
		/// Inventory ID
		/// </summary>
		[StockItem]
        [PXDefault]
        public virtual Int32? InventoryID
        {
            get
            {
                return _InventoryID;
            }
            set
            {
                _InventoryID = value;
            }
        }
        #endregion
        #region SubItemID
        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        protected Int32? _SubItemID;

		/// <summary>
		/// Sub item ID
		/// </summary>
		[SubItem(typeof(InvLookup.inventoryID), Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
            Where<InventoryItem.inventoryID, Equal<Current<InvLookup.inventoryID>>,
            And<InventoryItem.defaultSubItemOnEntry, Equal<boolTrue>>>>),
            PersistingCheck = PXPersistingCheck.NullOrBlank)]
        public virtual Int32? SubItemID
        {
            get
            {
                return _SubItemID;
            }
            set
            {
                _SubItemID = value;
            }
        }
        #endregion
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected int? _SiteID;

		/// <summary>
		/// Warehouse
		/// </summary>
		[AMSite]
		[PXDefault(typeof(Search<InventoryItemCurySettings.dfltSiteID, Where<InventoryItemCurySettings.inventoryID, Equal<Current<InvLookup.inventoryID>>,
			And<InventoryItemCurySettings.curyID, Equal<Current<AccessInfo.baseCuryID>>>>>), PersistingCheck = PXPersistingCheck.NullOrBlank)]
		public virtual int? SiteID
        {
            get
            {
                return _SiteID;
            }
            set
            {
                _SiteID = value;
            }
        }
        #endregion
        #region Quantity On Hand
        public abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }

        protected decimal? _QtyOnHand;

		/// <summary>
		/// Qty on hand
		/// </summary>
		[PXQuantity]
        [PXUIField(DisplayName = "Qty On Hand", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual decimal? QtyOnHand
        {
            get
            {
                return _QtyOnHand;
            }
            set
            {
                _QtyOnHand = value;
            }
        }
        #endregion
        #region UOM
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

        protected String _UOM;

		/// <summary>
		/// U o m
		/// </summary>
		[PXDefault(typeof(Search<InventoryItem.baseUnit, Where<InventoryItem.inventoryID, Equal<Current<InvLookup.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXString]
        [PXUIField(DisplayName = "Base Unit", Enabled = false)]
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
        #region Safety Stock
        public abstract class safetyStock : PX.Data.BQL.BqlDecimal.Field<safetyStock> { }

        protected Decimal? _SafetyStock;

		/// <summary>
		/// Safety stock
		/// </summary>
		[PXQuantity]
        [PXUIField(DisplayName = "Safety Stock", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Decimal? SafetyStock
        {
            get
            {
                return this._SafetyStock;
            }
            set
            {
                this._SafetyStock = value;
            }
        }
        #endregion
        #region Min Order Qty
        public abstract class minOrderQty : PX.Data.BQL.BqlDecimal.Field<minOrderQty> { }

        protected Decimal? _MinOrderQty;

		/// <summary>
		/// Min order qty
		/// </summary>
		[PXQuantity]
        [PXUIField(DisplayName = "Min. Order Qty", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Decimal? MinOrderQty
        {
            get
            {
                return this._MinOrderQty;
            }
            set
            {
                this._MinOrderQty = value;
            }
        }
        #endregion
        #region Max Order Qty
        public abstract class maxOrderQty : PX.Data.BQL.BqlDecimal.Field<maxOrderQty> { }

        protected Decimal? _MaxOrderQty;

		/// <summary>
		/// Max order qty
		/// </summary>
		[PXQuantity]
        [PXUIField(DisplayName = "Max. Order Qty", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Decimal? MaxOrderQty
        {
            get
            {
                return this._MaxOrderQty;
            }
            set
            {
                this._MaxOrderQty = value;
            }
        }
        #endregion
        #region Lot Qty
        public abstract class lotQty : PX.Data.BQL.BqlDecimal.Field<lotQty> { }

        protected Decimal? _LotQty;

		/// <summary>
		/// Lot qty
		/// </summary>
		[PXQuantity]
        [PXUIField(DisplayName = "Lot Qty", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Decimal? LotQty
        {
            get
            {
                return this._LotQty;
            }
            set
            {
                this._LotQty = value;
            }
        }
		#endregion
		#region ReplenishmentSource
		public abstract class replenishmentSource : PX.Data.BQL.BqlString.Field<replenishmentSource> { }

		protected string _ReplenishmentSource;

		/// <summary>
		/// Replenishment source
		/// </summary>
		[PXString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Source", Enabled = false, FieldClass = nameof(FeaturesSet.Replenishment))]
		[INReplenishmentSource.List]
		public virtual string ReplenishmentSource
		{
			get
			{
				return _ReplenishmentSource;
			}
			set
			{
				_ReplenishmentSource = value;
			}
		}
		#endregion
		#region ReplenishmentSiteID
		public abstract class replenishmentSiteID : PX.Data.BQL.BqlInt.Field<replenishmentSiteID> { }

		protected Int32? _ReplenishmentSiteID;

		/// <summary>
		/// Source Warehouse
		/// </summary>
		[Site(DisplayName = "Source Warehouse", Enabled = false, FieldClass = nameof(FeaturesSet.Replenishment))]
		public virtual Int32? ReplenishmentSiteID
		{
			get
			{
				return this._ReplenishmentSiteID;
			}
			set
			{
				this._ReplenishmentSiteID = value;
			}
		}
		#endregion
		#region TransferLeadTime
		public abstract class transferLeadTime : PX.Data.BQL.BqlInt.Field<transferLeadTime> { }

		/// <summary>
		/// Transfer lead time
		/// </summary>
		[PXInt]
		[PXUnboundDefault(0)]
		[PXUIField(DisplayName = "Transfer Lead Time", Enabled = false)]
		public Int32? TransferLeadTime { get; set; }
		#endregion
		#region LeadTime
		public abstract class leadTime : PX.Data.BQL.BqlInt.Field<leadTime> { }

		protected Int32? _LeadTime;

		/// <summary>
		/// Lead time
		/// </summary>
		[PXInt]
		[PXUIField(DisplayName = "Lead Time", Enabled = false)]
		[PXUnboundDefault(TypeCode.Int32, "0")]
		public Int32? LeadTime
		{
			get
			{
				return this._LeadTime;
			}
			set
			{
				this._LeadTime = value;
			}
		}
		#endregion
		#region AMGroupWindow
		public abstract class aMGroupWindow : PX.Data.BQL.BqlInt.Field<aMGroupWindow> { }

		protected int? _AMGroupWindow;

		/// <summary>
		/// Days of Supply filter field
		/// </summary>
		[PXUIField(DisplayName = "Days of Supply", Enabled = false, FieldClass = nameof(FeaturesSet.Warehouse))]
		[PXDefault(TypeCode.Int32, "0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? AMGroupWindow
		{
			get
			{
				return this._AMGroupWindow;
			}
			set
			{
				this._AMGroupWindow = value;
			}
		}
		#endregion
	}
}
