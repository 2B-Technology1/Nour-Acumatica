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

using PX.Data;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.AM.Attributes;
using System.Collections;
using System;
using PX.Data.BQL.Fluent;
using PX.Common;
using PX.Data.BQL;
using PX.Objects.FA;

namespace PX.Objects.AM.GraphExtensions
{
    public class INItemSiteMaintAMExtension : PXGraphExtension<INItemSiteMaint>
    {
        public static bool IsActive()
        {
			return Features.ManufacturingOrDRPOrReplenishmentEnabled();
		}

        public PXSelect<AMSubItemDefault,
            Where<AMSubItemDefault.inventoryID, Equal<Current<INItemSite.inventoryID>>,
                And<AMSubItemDefault.siteID, Equal<Current<INItemSite.siteID>>>>,
            OrderBy<Asc<AMSubItemDefault.subItemID>>> AMSubItemDefaults;

        public PXSelectJoin<INItemSite,
                InnerJoin<InventoryItem, On<INItemSite.FK.InventoryItem>>,
                Where<INItemSite.inventoryID, Equal<Current<INItemSite.inventoryID>>,
                And<INItemSite.siteID, Equal<Current<INItemSite.siteID>>>>> PlanningReplenishmentSettings;

        public PXSelectJoin<INItemSite,
                InnerJoin<InventoryItem, On<INItemSite.FK.InventoryItem>>,
                Where<INItemSite.inventoryID, Equal<Current<INItemSite.inventoryID>>,
                And<INItemSite.siteID, Equal<Current<INItemSite.siteID>>>>> preferedVendorFields;

        public PXSelectJoin<INItemSite,
                InnerJoin<InventoryItem, On<INItemSite.FK.InventoryItem>>,
                Where<INItemSite.inventoryID, Equal<Current<INItemSite.inventoryID>>,
                And<INItemSite.siteID, Equal<Current<INItemSite.siteID>>>>> inventoryPlanningSettings;

        public PXSelectJoin<INItemSite,
                InnerJoin<InventoryItem, On<INItemSite.FK.InventoryItem>>,
                Where<INItemSite.inventoryID, Equal<Current<INItemSite.inventoryID>>,
                And<INItemSite.siteID, Equal<Current<INItemSite.siteID>>>>> productionOrderDefaultSettings;

        public PXSelectJoin<INItemSite,
                InnerJoin<InventoryItem, On<INItemSite.FK.InventoryItem>>,
                Where<INItemSite.inventoryID, Equal<Current<INItemSite.inventoryID>>,
                And<INItemSite.siteID, Equal<Current<INItemSite.siteID>>>>> manufacturingSettings;

        public PXSetupOptional<AMRPSetup> Setup;

        protected virtual bool IsConsolidateOrdersVisible
        {
            get
            {
                return Setup.Current?.UseDaysSupplytoConsolidateOrders == true;
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            AMSubItemDefaults.AllowDelete = false;
            AMSubItemDefaults.AllowInsert = false;

            var subItemEnabled = PXAccess.FeatureInstalled<FeaturesSet.subItem>();
            // Controls the display of the grid on stock items
            AMSubItemDefaults.AllowSelect = subItemEnabled;

            ViewPlanningBOM.SetVisible(PXAccess.FeatureInstalled<FeaturesSet.manufacturingMRP>());
        }

        /// <summary>
        /// MYOB - "Basic Inventory Replenishments" feature.
        /// Indicates is this feature is enabled/turned on
        /// </summary>
        protected virtual bool BasicReplenishmentsEnabled => OEMHelper.FeatureInstalled(OEMHelper.MYOBFeatures.BasicInvReplenish);

        /// <summary>
        /// Indicates if the full replenishment feature is enabled/turned on
        /// </summary>
        protected virtual bool FullReplenishmentsEnabled => PXAccess.FeatureInstalled<FeaturesSet.replenishment>();

        protected virtual void INItemSite_RowSelecting(PXCache cache, PXRowSelectingEventArgs e, PXRowSelecting del)
        {
            del?.Invoke(cache, e);

            var row = (INItemSite)e.Row;
            if (row == null)
            {
                return;
            }

            var rowExt = PXCache<INItemSite>.GetExtension<INItemSiteExt>(row);
            if (rowExt == null)
            {
                return;
            }
			rowExt.AMReplenishmentSource = row.ReplenishmentSource;
			rowExt.AMSourceSiteID = row.ReplenishmentSourceSiteID;
			rowExt.AMMinQty = row.MinQty;
			rowExt.AMSafetyStock = row.SafetyStock;
			rowExt.AMSafetyStockOverride = row.SafetyStockOverride;
			rowExt.AMMinQtyOverride = row.MinQtyOverride;
		if (rowExt.AMTransferLeadTimeOverride.GetValueOrDefault() == false)
			cache.RaiseFieldDefaulting<INItemSiteExt.aMTransferLeadTime>(row, out object _);

		}

        protected virtual void INItemSite_RowSelected(PXCache cache, PXRowSelectedEventArgs e, PXRowSelected del)
        {
            del?.Invoke(cache, e);

            var row = (INItemSite)e.Row;
            if (row == null)
            {
                return;
            }

            var extension = row.GetExtension<INItemSiteExt>();
            if (extension == null)
            {
                return;
            }
            var hasManufacturing = PXAccess.FeatureInstalled<FeaturesSet.manufacturing>();
            manufacturingSettings.AllowSelect = hasManufacturing;
			var hasMRPorDRP = PXAccess.FeatureInstalled<FeaturesSet.manufacturingMRP>() || PXAccess.FeatureInstalled<FeaturesSet.distributionReqPlan>();

			inventoryPlanningSettings.AllowSelect = hasMRPorDRP && row.PlanningMethod.IsIn(INPlanningMethod.MRP, INPlanningMethod.DRP); 
			PlanningReplenishmentSettings.AllowSelect = PXAccess.FeatureInstalled<FeaturesSet.replenishment>() && row.PlanningMethod == INPlanningMethod.InventoryReplenishment;
			productionOrderDefaultSettings.AllowSelect = hasManufacturing && (row.PlanningMethod.IsNotIn(INPlanningMethod.MRP, INPlanningMethod.DRP));
			preferedVendorFields.AllowSelect = row.PlanningMethod != INPlanningMethod.None;
			Base.PreferredVendorItem.AllowSelect = row.PlanningMethod != INPlanningMethod.None;

			PXUIFieldAttribute.SetVisible<INItemSiteExt.aMGroupWindow>(Base.itemsitesettings.Cache, row, IsConsolidateOrdersVisible);
            PXUIFieldAttribute.SetVisible<INItemSiteExt.aMGroupWindowOverride>(Base.itemsitesettings.Cache, row, IsConsolidateOrdersVisible);
            PXUIFieldAttribute.SetEnabled<INItemSiteExt.aMReplenishmentSource>(Base.itemsitesettings.Cache, row, extension.AMReplenishmentSourceOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<INItemSiteExt.aMSourceSiteID>(Base.itemsitesettings.Cache, row, extension.AMSourceSiteIDOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<INItemSiteExt.aMSafetyStock>(Base.itemsitesettings.Cache, row, extension.AMSafetyStockOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<INItemSiteExt.aMMinQty>(Base.itemsitesettings.Cache, row, extension.AMMinQtyOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<INItemSiteExt.aMMinOrdQty>(Base.itemsitesettings.Cache, row, extension.AMMinOrdQtyOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<INItemSiteExt.aMMaxOrdQty>(Base.itemsitesettings.Cache, row, extension.AMMaxOrdQtyOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<INItemSiteExt.aMLotSize>(Base.itemsitesettings.Cache, row, extension.AMLotSizeOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<INItemSiteExt.aMMFGLeadTime>(Base.itemsitesettings.Cache, row, extension.AMMFGLeadTimeOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<INItemSiteExt.aMScrapSiteID>(Base.itemsitesettings.Cache, row, extension.AMScrapOverride.GetValueOrDefault());
            PXUIFieldAttribute.SetEnabled<INItemSiteExt.aMScrapLocationID>(Base.itemsitesettings.Cache, row, extension.AMScrapOverride.GetValueOrDefault());
			PXUIFieldAttribute.SetVisible<INItemSiteExt.aMReplenishmentSourceOverride>(Base.itemsitesettings.Cache, row, row.PlanningMethod != INPlanningMethod.InventoryReplenishment);
			PXUIFieldAttribute.SetVisible<INItemSiteExt.aMSourceSiteIDOverride>(Base.itemsitesettings.Cache, row, row.PlanningMethod != INPlanningMethod.InventoryReplenishment);
			ValidateSelectedBOM(cache, row);
        }

		protected virtual void ValidateSelectedBOM(PXCache cache, INItemSite row)
		{
			var itemExtension = row.GetExtension<INItemSiteExt>();
			if (itemExtension.AMBOMID != null)
			{
				var bom = PrimaryBomIDManager.GetActiveRevisionBomItem(Base, itemExtension.AMBOMID);

				if (bom == null)
				{
					cache.RaiseExceptionHandling<INItemSiteExt.aMBOMID>(row, itemExtension.AMBOMID,
						new PXSetPropertyException(Messages.NoActiveRevisionForBom, PXErrorLevel.Warning, itemExtension.AMBOMID));
				}
			}
			if (itemExtension.AMPlanningBOMID != null)
			{
				var planingBom = PrimaryBomIDManager.GetActiveRevisionBomItem(Base, itemExtension.AMPlanningBOMID);

				if (planingBom == null)
				{
					cache.RaiseExceptionHandling<INItemSiteExt.aMPlanningBOMID>(row, itemExtension.AMPlanningBOMID,
						new PXSetPropertyException(Messages.NoActiveRevisionForBom, PXErrorLevel.Warning, itemExtension.AMPlanningBOMID));
				}
			}
		}

        public virtual void INItemSite_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e, PXRowUpdated del)
        {
            if (del != null)
            {
                del(cache, e);
            }

            var row = (INItemSite)e.Row;
            var oldRow = (INItemSite)e.OldRow;
            if (row == null)
            {
                return;
            }

            var extension = row.GetExtension<INItemSiteExt>();
            var oldRowExtension = oldRow.GetExtension<INItemSiteExt>();
            var item = Base.itemrecord.Current;
            var itemExt = Base.itemrecord.Current.GetExtension<InventoryItemExt>();
            if (extension == null)
            {
                return;
            }

            // OVERRIDES REMOVED... RESET VALUE BACK FROM THE STOCK ITEM VALUE
            if (itemExt != null && oldRowExtension != null)
            {
                if (oldRowExtension.AMReplenishmentSourceOverride.GetValueOrDefault() && !extension.AMReplenishmentSourceOverride.GetValueOrDefault())
                {
                    cache.SetValue<INItemSite.replenishmentSource>(row, item.ReplenishmentSource);
                    cache.SetValue<INItemSiteExt.aMReplenishmentSource>(row, item.ReplenishmentSource);
                }
                if (oldRowExtension.AMSourceSiteIDOverride.GetValueOrDefault() && !extension.AMSourceSiteIDOverride.GetValueOrDefault())
                {
                    cache.SetValue<INItemSiteExt.aMSourceSiteID>(row, itemExt.AMSourceSiteID.GetValueOrDefault());
                }
                if (oldRowExtension.AMSafetyStockOverride.GetValueOrDefault() && !extension.AMSafetyStockOverride.GetValueOrDefault())
                {
                    cache.SetValue<INItemSiteExt.aMSafetyStock>(row, itemExt.AMSafetyStock.GetValueOrDefault());
                }
				if (oldRowExtension.AMMinQtyOverride.GetValueOrDefault() && !extension.AMMinQtyOverride.GetValueOrDefault())
				{
					cache.SetValue<INItemSiteExt.aMMinQty>(row, itemExt.AMMinQty.GetValueOrDefault());
				}
				if (oldRowExtension.AMMinOrdQtyOverride.GetValueOrDefault() && !extension.AMMinOrdQtyOverride.GetValueOrDefault())
                {
                    cache.SetValue<INItemSiteExt.aMMinOrdQty>(row, itemExt.AMMinOrdQty.GetValueOrDefault());
                }
                if (oldRowExtension.AMMaxOrdQtyOverride.GetValueOrDefault() && !extension.AMMaxOrdQtyOverride.GetValueOrDefault())
                {
                    cache.SetValue<INItemSiteExt.aMMaxOrdQty>(row, itemExt.AMMaxOrdQty.GetValueOrDefault());
                }
                if (oldRowExtension.AMLotSizeOverride.GetValueOrDefault() && !extension.AMLotSizeOverride.GetValueOrDefault())
                {
                    cache.SetValue<INItemSiteExt.aMLotSize>(row, itemExt.AMLotSize.GetValueOrDefault());
                }
                if (oldRowExtension.AMMFGLeadTimeOverride.GetValueOrDefault() && !extension.AMMFGLeadTimeOverride.GetValueOrDefault())
                {
                    cache.SetValue<INItemSiteExt.aMMFGLeadTime>(row, itemExt.AMMFGLeadTime.GetValueOrDefault());
                }
                if (oldRowExtension.AMGroupWindowOverride.GetValueOrDefault() && !extension.AMGroupWindowOverride.GetValueOrDefault())
                {
                    cache.SetValue<INItemSiteExt.aMGroupWindow>(row, itemExt.AMGroupWindow.GetValueOrDefault());
                }
            }

            if (FullReplenishmentsEnabled)
            {
                if (oldRow.ReplenishmentSource != row.ReplenishmentSource)
                    cache.SetValue<INItemSiteExt.aMSafetyStock>(row, row.SafetyStock);
            }
            else
            {
                // Safety Stock (and override)
                if ((oldRowExtension != null
                    && extension.AMSafetyStock.GetValueOrDefault() != oldRowExtension.AMSafetyStock.GetValueOrDefault())
                    || row.SafetyStockOverride.GetValueOrDefault() != extension.AMSafetyStockOverride.GetValueOrDefault())
                {
                    cache.SetValue<INItemSite.safetyStockOverride>(row, extension.AMSafetyStockOverride.GetValueOrDefault());
                }
                if (row.PlanningMethod != INPlanningMethod.InventoryReplenishment && row.SafetyStock.GetValueOrDefault() != extension.AMSafetyStock.GetValueOrDefault())
                {
                    cache.SetValue<INItemSite.safetyStock>(row, extension.AMSafetyStock);
                }
            }

            if (!FullReplenishmentsEnabled && !BasicReplenishmentsEnabled && row.ReplenishmentPolicyOverride == false)
            {
                cache.SetValue<INItemSite.replenishmentSource>(row, row.ReplenishmentSource);
            }

			// Reorder Point (and override)
			if (row.PlanningMethod != INPlanningMethod.InventoryReplenishment && (oldRowExtension != null
				&& extension.AMMinQty.GetValueOrDefault() != oldRowExtension.AMMinQty.GetValueOrDefault())
				|| row.SafetyStockOverride.GetValueOrDefault() != extension.AMSafetyStockOverride.GetValueOrDefault())
			{
				cache.SetValue<INItemSite.minQtyOverride>(row, extension.AMMinQtyOverride.GetValueOrDefault());
			}
			if (row.PlanningMethod != INPlanningMethod.InventoryReplenishment && row.MinQty.GetValueOrDefault() != extension.AMMinQty.GetValueOrDefault())
			{
				cache.SetValue<INItemSite.minQty>(row, extension.AMMinQty);
			}
			if (row.PlanningMethod != INPlanningMethod.InventoryReplenishment && row.ReplenishmentSourceSiteID.GetValueOrDefault() != extension.AMSourceSiteID.GetValueOrDefault())
			{
				extension.AMSourceSiteID = extension.AMSourceSiteID ==0 ? null : extension.AMSourceSiteID;
				cache.SetValue<INItemSite.replenishmentSourceSiteID>(row, extension.AMSourceSiteID);
			}
			else
			{
				cache.SetValue<INItemSiteExt.aMSourceSiteID>(row, row.ReplenishmentSourceSiteID);
			}
			if (row.PlanningMethod != INPlanningMethod.InventoryReplenishment && row.ReplenishmentSource != extension.AMReplenishmentSource)
			{
				cache.SetValue<INItemSite.replenishmentSource>(row, extension.AMReplenishmentSource);
			}
			else
			{
				cache.SetValue<INItemSiteExt.aMReplenishmentSource>(row, row.ReplenishmentSource);
			}
		}

        protected virtual void INItemSite_AMScrapSiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (INItemSite)e.Row;
            var rowExt = row.GetExtension<INItemSiteExt>();

            if (row == null)
            {
                return;
            }

            rowExt.AMScrapLocationID = null;
        }

        //Purpose of override is to copy InventoryItemExt values over to the new INItemSiteExt record as defaults similar to Acumatica's DefaultItemSiteByitem call
        public virtual void INItemSite_RowInserted(PXCache sender, PXRowInsertedEventArgs e, PXRowInserted del)
        {
            if (del != null)
            {
                del(sender, e);
            }

            INItemSite inItemSite = (INItemSite)e.Row;
            if (inItemSite == null || !inItemSite.InventoryID.HasValue || !inItemSite.SiteID.HasValue)
            {
                return;
            }

            InventoryItem currentInventoryItem = Base.itemrecord.Current;
            if (currentInventoryItem != null && Base.itemrecord.Cache.IsDirty)
            {
                AM.InventoryHelper.DefaultItemSiteManufacturing(Base, currentInventoryItem, inItemSite);
                var invItemExt = currentInventoryItem.GetExtension<InventoryItemExt>();

                if (invItemExt != null && !FullReplenishmentsEnabled && !BasicReplenishmentsEnabled)
                {
                    inItemSite.ReplenishmentSource = currentInventoryItem.ReplenishmentSource;
                }
            }

			INItemSiteExt iNItemSiteExt = inItemSite.GetExtension<INItemSiteExt>();
			var rowExt = inItemSite.GetExtension<INItemSiteExt>();
			AMSiteTransfer leadTime = SelectFrom<AMSiteTransfer>.
					Where<AMSiteTransfer.siteID.IsEqual<@P.AsInt>.
						And<AMSiteTransfer.transferSiteID.IsEqual<@P.AsInt>.
						And<INItemSiteExt.aMTransferLeadTimeOverride.FromCurrent.IsEqual<False>>>>.View.Select(Base, inItemSite.SiteID, rowExt.AMSourceSiteID);
			iNItemSiteExt.AMTransferLeadTime = leadTime?.TransferLeadTime;
        }

		protected void _(Events.FieldUpdated<INItemSite, INItemSiteExt.aMReplenishmentSource> e)
		{
			if (e.Row == null) return;
			var row = e.Row;
			var rowExt = row.GetExtension<INItemSiteExt>();
			row.ReplenishmentSource = e.NewValue.ToString();

			if(!(e.NewValue.ToString() == INReplenishmentSource.Transfer || (row.PlanningMethod.IsIn(INPlanningMethod.MRP, INPlanningMethod.DRP) && e.NewValue.IsIn(INReplenishmentSource.Purchased, INReplenishmentSource.PurchaseToOrder))))
			{
				rowExt.AMSourceSiteID = null;
			}
		}

		protected virtual void _(Events.FieldUpdated<INItemSite, INItemSiteExt.aMSourceSiteID> e)
		{
			if (e.Row == null) return;
			var row = e.Row;
			var rowExt = row.GetExtension<INItemSiteExt>();
			row.ReplenishmentSourceSiteID = rowExt.AMSourceSiteID;
		}

		protected virtual void _(Events.FieldUpdated<INItemSite, INItemSiteExt.aMMinQty> e)
		{
			if (e.Row == null) return;
			var row = e.Row;
			var rowExt = row.GetExtension<INItemSiteExt>();
			row.MinQty = rowExt.AMMinQty;
			row.MinQtyOverride = rowExt.AMMinQtyOverride;
}

		protected virtual void _(Events.FieldUpdated<INItemSite, INItemSiteExt.aMSafetyStock> e)
		{
			if (e.Row == null) return;
			var row = e.Row;
			var rowExt = row.GetExtension<INItemSiteExt>();
			row.SafetyStock = rowExt.AMSafetyStock;
			row.SafetyStockOverride = rowExt.AMSafetyStockOverride;
			e.Cache.MarkUpdated(row);
		}


        // Added this cache attached because without it, the page times out when changing the Warehouse
        [PXDBInt]
        [PXUIField(DisplayName = "Scrap Location", FieldClass = "INLOCATION")]
        protected virtual void _(Events.CacheAttached<INSiteExt.aMScrapLocationID> e) { }

        public PXAction<INItemSite> ViewBOM;
        [PXButton]
        [PXUIField(DisplayName = "View BOM", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected virtual IEnumerable viewBOM(PXAdapter adapter)
        {
			var itemExtension = Base.itemsiterecord.Current.GetExtension<INItemSiteExt>();
			var currentSite = Base.itemsiterecord.Current;
			if (itemExtension.AMBOMID == null && currentSite != null)
			{
				BOMMaint.RedirectNew(currentSite.InventoryID, currentSite.SiteID);
			}
			BOMMaint.RedirectToBOM(itemExtension.AMBOMID);

			return adapter.Get();
        }

        public PXAction<INItemSite> ViewPlanningBOM;
        [PXButton]
        [PXUIField(DisplayName = "View Planning BOM", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected virtual IEnumerable viewPlanningBOM(PXAdapter adapter)
        {
            var itemExtension = Base.itemsiterecord.Current.GetExtension<INItemSiteExt>();
            var currentSite = Base.itemsiterecord.Current;
            if (itemExtension.AMPlanningBOMID == null && currentSite != null)
            {
                BOMMaint.RedirectNew(currentSite.InventoryID, currentSite.SiteID);
            }
            BOMMaint.RedirectToBOM(itemExtension.AMPlanningBOMID);

            return adapter.Get();
        }
    }
}
