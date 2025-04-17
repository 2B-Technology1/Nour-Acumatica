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
using PX.Data;
using PX.Data.WorkflowAPI;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.AM.Attributes;
using PX.Data.BQL.Fluent;
using PX.Common;
using PX.Objects.FA;
using PX.Data.BQL;
using static PX.Objects.CS.FeaturesSet;

namespace PX.Objects.AM.GraphExtensions
{
    public class InventoryItemMaintAMExtension : PXGraphExtension<InventoryItemMaint>
    {
        public static bool IsActive()
        {
			return Features.ManufacturingOrDRPOrReplenishmentEnabled();
        }

        public SelectFrom<InventoryItem>.
                            Where<InventoryItem.inventoryID.IsEqual<InventoryItem.inventoryID.FromCurrent>>.View inventoryPlanningSettings;
        public SelectFrom<InventoryItem>.
                            Where<InventoryItem.inventoryID.IsEqual<InventoryItem.inventoryID.FromCurrent>>.View productionOrderDefaultSettings;
        public SelectFrom<InventoryItem>.
                            Where<InventoryItem.inventoryID.IsEqual<InventoryItem.inventoryID.FromCurrent>>.View manufacturingSettings;

        [PXCopyPasteHiddenView]
        public PXSelect<AMSubItemDefault, Where<AMSubItemDefault.inventoryID, Equal<Current<InventoryItem.inventoryID>>>, OrderBy<Asc<AMSubItemDefault.subItemID>>> AMSubItemDefaults;

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

            var subItemEnabled = PXAccess.FeatureInstalled<FeaturesSet.subItem>() && PXAccess.FeatureInstalled<FeaturesSet.manufacturing>();
            // Controls the display of the grid on stock items
            AMSubItemDefaults.AllowSelect = subItemEnabled;
			ViewPlanningBOM.SetVisible(PXAccess.FeatureInstalled<FeaturesSet.manufacturingMRP>());
		}

        public sealed override void Configure(PXScreenConfiguration configuration) =>
            Configure(configuration.GetScreenConfigurationContext<InventoryItemMaint, InventoryItem>());
        protected static void Configure(WorkflowContext<InventoryItemMaint, InventoryItem> context)
        {
            context
                .UpdateScreenConfigurationFor(screen =>
                    screen.WithActions(actions =>
                        actions.Add<InventoryItemMaintAMExtension>(e => e.WhereUsedInq, a => a.WithCategory(PredefinedCategory.Inquiries))));
        }

        [PXOverride]
        public void Persist(Action del)
        {
            var estimateInventoryCdUpdateRequired = !Base.Item.Cache.IsCurrentRowInserted() &&
                                                    EstimateGraphHelper
                                                        .InventoryCDUpdateRequired<InventoryItem.inventoryCD>(
                                                            Base.Item.Cache);

            del();

            if (!FullReplenishmentsEnabled)
            {
                //Issue with the grid rep source not refreshing correctly... force refresh of 'Warehouse Details' tab
                Base.itemsiterecords.Cache.Clear();
                Base.itemsiterecords.Cache.ClearQueryCache();
            }

            if (estimateInventoryCdUpdateRequired)
            {
                EstimateGraphHelper.UpdateEstimateInventoryCD(Base.Item.Current, Base);
            }
        }

        public PXAction<InventoryItem> WhereUsedInq;
        [PXButton]
        [PXUIField(DisplayName = "BOM Where Used", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual System.Collections.IEnumerable whereUsedInq(PXAdapter adapter)
        {
            if (Base.Item.Current == null
                || Base.Item.Current.InventoryID.GetValueOrDefault() <= 0)
            {
                return adapter.Get();
            }

            var inqGraph = PXGraph.CreateInstance<BOMWhereUsedInq>();
            inqGraph.Filter.Current.InventoryID = Base.Item.Current.InventoryID;
            PXRedirectHelper.TryRedirect(inqGraph, PXRedirectHelper.WindowMode.NewWindow);

            return adapter.Get();
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

        protected virtual void InventoryItem_AMScrapSiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (InventoryItem)e.Row;
            var rowExt = row.GetExtension<InventoryItemExt>();

            if (row == null)
            {
                return;
            }

            Base.Item.Cache.SetValueExt<InventoryItemExt.aMScrapLocationID>(Base.Item.Current, null);
        }

        public virtual void AMSubItemDefault_IsItemDefault_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (!PXAccess.FeatureInstalled<FeaturesSet.subItem>())
            {
                return;
            }

            var newValue = (bool?)e.NewValue;

            var row = (AMSubItemDefault)e.Row;

            if (row?.InventoryID == null || row.SubItemID == null || !newValue.GetValueOrDefault())
            {
                return;
            }

            //if new value is true we need to check other subitem defaults to make sure there isn't a default already
            System.Collections.Generic.List<AMSubItemDefault> subItemDefaults = AMSubItemDefaults.Cache.Cached.Cast<AMSubItemDefault>().Where(
                cacheRow => cacheRow.InventoryID == row.InventoryID && cacheRow.SubItemID == row.SubItemID && cacheRow.IsItemDefault.GetValueOrDefault()
                ).ToList();

            if (subItemDefaults.Count > 0)
            {
                e.NewValue = false;
                e.Cancel = true;
                cache.RaiseExceptionHandling<AMSubItemDefault.isItemDefault>(
                    row,
                    row.IsItemDefault,
                    new PXSetPropertyException(AM.Messages.GetLocal(AM.Messages.OneDefaultSubItem), PXErrorLevel.Error)
                    );
            }

        }

        protected virtual void INItemSite_RowSelecting(PXCache cache, PXRowSelectingEventArgs e, PXRowSelecting del)
        {
            if (del != null)
            {
                del(cache, e);
            }

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

            // Make sure these unbound fields have a value (possibly used on Warehouse Details tab)
            extension.AMSafetyStock = row.SafetyStock.GetValueOrDefault();
            extension.AMSafetyStockOverride = row.SafetyStockOverride.GetValueOrDefault();
			extension.AMMinQty = row.MinQty.GetValueOrDefault();
			extension.AMMinQtyOverride = row.MinQtyOverride.GetValueOrDefault();
		}

        protected virtual void InventoryItem_RowSelected(PXCache cache, PXRowSelectedEventArgs e, PXRowSelected del)
		{
            if (del != null)
            {
                del(cache, e);
            }

            var row = (InventoryItem)e.Row;
            if (row == null)
            {
                return;
            }

            var extension = PXCache<InventoryItem>.GetExtension<InventoryItemExt>(row);
            if (extension == null)
            {
                return;
            }

            bool isManufactured = row.ReplenishmentSource == INReplenishmentSource.Manufactured;
			var hasMRPorDRP = PXAccess.FeatureInstalled<FeaturesSet.manufacturingMRP>() || PXAccess.FeatureInstalled<FeaturesSet.distributionReqPlan>();
			var hasManufacturing = PXAccess.FeatureInstalled<FeaturesSet.manufacturing>();
			var isReplenishment = PXAccess.FeatureInstalled<FeaturesSet.replenishment>() && row.PlanningMethod == INPlanningMethod.InventoryReplenishment;

            PXUIFieldAttribute.SetEnabled<InventoryItemExt.aMMakeToOrderItem>(Base.ItemSettings.Cache, row, isManufactured);
            manufacturingSettings.AllowSelect = PXAccess.FeatureInstalled<FeaturesSet.manufacturing>();
			inventoryPlanningSettings.AllowSelect = hasMRPorDRP && row.PlanningMethod.IsIn(INPlanningMethod.MRP, INPlanningMethod.DRP);
            Base.replenishment.AllowSelect = PXAccess.FeatureInstalled<FeaturesSet.replenishment>() && row.PlanningMethod == INPlanningMethod.InventoryReplenishment;
            productionOrderDefaultSettings.AllowSelect = hasManufacturing && (!hasMRPorDRP || row.PlanningMethod.IsNotIn(INPlanningMethod.MRP, INPlanningMethod.DRP));

            PXUIFieldAttribute.SetVisible<InventoryItemExt.aMGroupWindow>(inventoryPlanningSettings.Cache, row, IsConsolidateOrdersVisible);
            PXUIFieldAttribute.SetVisible<InventoryItemExt.aMGroupWindowOverride>(inventoryPlanningSettings.Cache, row, IsConsolidateOrdersVisible);
            PXUIFieldAttribute.SetEnabled<InventoryItemExt.aMMakeToOrderItem>(Base.ItemSettings.Cache, row, isManufactured);
            PXUIFieldAttribute.SetEnabled<InventoryItemExt.aMGroupWindow>(Base.ItemSettings.Cache, row, extension.AMGroupWindowOverride == true);
            ValidateSelectedBOM(cache, row);
			ToggleReplenishmentOnlyFieldsVisibility(isReplenishment);
        }

		private void ToggleReplenishmentOnlyFieldsVisibility(bool isReplenishment)
		{
            PXUIFieldAttribute.SetVisible<INItemSite.replenishmentPolicyID>(Base.itemsiterecords.Cache, null, isReplenishment);
            PXUIFieldAttribute.SetVisible<INItemSite.replenishmentPolicyOverride>(Base.itemsiterecords.Cache, null, isReplenishment);
            PXUIFieldAttribute.SetVisible<INItemSite.serviceLevelPct>(Base.itemsiterecords.Cache, null, isReplenishment);
            PXUIFieldAttribute.SetVisible<INItemSite.demandPerDayAverage>(Base.itemsiterecords.Cache, null, isReplenishment);
            PXUIFieldAttribute.SetVisible<INItemSite.demandPerDaySTDEV>(Base.itemsiterecords.Cache, null, isReplenishment);
            PXUIFieldAttribute.SetVisible<INItemSite.lastForecastDate>(Base.itemsiterecords.Cache, null, isReplenishment);
		}

		protected virtual void ValidateSelectedBOM(PXCache cache, InventoryItem row)
		{
			var itemExtension = row.GetExtension<InventoryItemExt>();
			if (itemExtension.AMBOMID != null)
			{
				var bom = PrimaryBomIDManager.GetActiveRevisionBomItem(Base, itemExtension.AMBOMID);

				if (bom == null)
				{
					cache.RaiseExceptionHandling<InventoryItemExt.aMBOMID>(row, itemExtension.AMBOMID,
						new PXSetPropertyException(Messages.NoActiveRevisionForBom, PXErrorLevel.Warning, itemExtension.AMBOMID));
				}
			}
			if (itemExtension.AMPlanningBOMID != null)
			{
				var planingBom = PrimaryBomIDManager.GetActiveRevisionBomItem(Base, itemExtension.AMPlanningBOMID);

				if (planingBom == null)
				{
					cache.RaiseExceptionHandling<InventoryItemExt.aMPlanningBOMID>(row, itemExtension.AMPlanningBOMID,
						new PXSetPropertyException(Messages.NoActiveRevisionForBom, PXErrorLevel.Warning, itemExtension.AMPlanningBOMID));
				}
			}
		}

		private List<INItemSite> GetCurrentINItemSites()
        {
            var inItemSites = new List<INItemSite>();
            foreach (INItemSite result in Base.itemsiterecords.Select())
            {
                inItemSites.Add((INItemSite)Base.itemsiterecords.Cache.CreateCopy(result));
            }
            return inItemSites;
        }

        protected virtual void SyncReplenishmentSettings(List<INItemSite> inItemSitesToSync)
        {
            if (inItemSitesToSync == null
                || inItemSitesToSync.Count == 0)
            {
                return;
            }

            var itemExtension = Base.Item.Current.GetExtension<InventoryItemExt>();

            foreach (INItemSite inItemSite in inItemSitesToSync)
            {
                bool isUpdated = false;
                INItemSite inItemSiteUpdate = (INItemSite)Base.itemsiterecords.Cache.Locate(inItemSite);
                if (inItemSiteUpdate == null)
                {
                    inItemSiteUpdate = PXSelect<INItemSite,
                    Where<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>,
                        And<INItemSite.siteID, Equal<Required<INItemSite.siteID>>>>
                        >.SelectWindowed(Base, 0, 1, inItemSite.InventoryID, inItemSite.SiteID);
                }
                if (inItemSiteUpdate == null)
                {
                    continue;
                }
                var inItemSiteUpdateExtension = inItemSiteUpdate.GetExtension<INItemSiteExt>();

                // Replenishment Source (and override)
                if (!FullReplenishmentsEnabled
                    && inItemSite.ReplenishmentPolicyOverride.GetValueOrDefault() != inItemSiteUpdate.ReplenishmentPolicyOverride.GetValueOrDefault())
                {
                    inItemSiteUpdate.ReplenishmentPolicyOverride = inItemSite.ReplenishmentPolicyOverride.GetValueOrDefault();
                    isUpdated = true;
                }
                if (!FullReplenishmentsEnabled
                    && inItemSiteUpdate.ReplenishmentPolicyOverride.GetValueOrDefault())
                {
                    inItemSiteUpdate.ReplenishmentSource = inItemSite.ReplenishmentSource;
                    isUpdated = true;
                }
                if (itemExtension != null
                    && !inItemSiteUpdate.ReplenishmentPolicyOverride.GetValueOrDefault()
                    && (!FullReplenishmentsEnabled || string.IsNullOrWhiteSpace(inItemSiteUpdate.ReplenishmentClassID))
                    && !inItemSiteUpdate.ReplenishmentSource.EqualsWithTrim(inItemSite.ReplenishmentSource))
                {
                    inItemSiteUpdate.ReplenishmentSource = inItemSite.ReplenishmentSource ?? INReplenishmentSource.Purchased;
                    isUpdated = true;
                }

                // Safety Stock (and override)
                if (!FullReplenishmentsEnabled
                    && inItemSite.SafetyStockOverride.GetValueOrDefault() != inItemSiteUpdate.SafetyStockOverride.GetValueOrDefault())
                {
                    inItemSiteUpdate.SafetyStockOverride = inItemSite.SafetyStockOverride.GetValueOrDefault();
                    isUpdated = true;
                }
                if (!FullReplenishmentsEnabled
                    && inItemSiteUpdate.SafetyStockOverride.GetValueOrDefault())
                {
                    inItemSiteUpdate.SafetyStock = inItemSite.SafetyStock.GetValueOrDefault();
                    isUpdated = true;
                }
                if (itemExtension != null
                    && !inItemSiteUpdate.SafetyStockOverride.GetValueOrDefault()
                    && (!FullReplenishmentsEnabled || string.IsNullOrWhiteSpace(inItemSiteUpdate.ReplenishmentClassID))
                    && inItemSiteUpdate.SafetyStock.GetValueOrDefault() != itemExtension.AMSafetyStock.GetValueOrDefault())
                {
                    inItemSiteUpdate.SafetyStock = itemExtension.AMSafetyStock.GetValueOrDefault();
                    isUpdated = true;
                }

                // Reorder Point (and override)
                if (!FullReplenishmentsEnabled && !BasicReplenishmentsEnabled
                    && inItemSite.MinQtyOverride.GetValueOrDefault() != inItemSiteUpdate.MinQtyOverride.GetValueOrDefault())
                {
                    inItemSiteUpdate.MinQtyOverride = inItemSite.MinQtyOverride.GetValueOrDefault();
                    isUpdated = true;
                }
                if (!FullReplenishmentsEnabled && !BasicReplenishmentsEnabled
                    && inItemSiteUpdate.MinQtyOverride.GetValueOrDefault())
                {
                    inItemSiteUpdate.MinQty = inItemSite.MinQty.GetValueOrDefault();
                    isUpdated = true;
                }
                if (itemExtension != null
                    && !inItemSiteUpdate.MinQtyOverride.GetValueOrDefault()
                    && (!FullReplenishmentsEnabled || string.IsNullOrWhiteSpace(inItemSiteUpdate.ReplenishmentClassID))
                    && inItemSiteUpdate.MinQty.GetValueOrDefault() != itemExtension.AMMinQty.GetValueOrDefault())
                {
                    inItemSiteUpdate.MinQty = itemExtension.AMMinQty.GetValueOrDefault();
                    isUpdated = true;
                }

                // Min Order Qty
                if (inItemSiteUpdateExtension != null
                    && !inItemSiteUpdateExtension.AMMinOrdQtyOverride.GetValueOrDefault()
                    && inItemSiteUpdateExtension.AMMinOrdQty.GetValueOrDefault() != itemExtension.AMMinOrdQty.GetValueOrDefault())
                {
                    inItemSiteUpdateExtension.AMMinOrdQty = itemExtension.AMMinOrdQty.GetValueOrDefault();
                    isUpdated = true;
                }

                // Max Order Qty
                if (inItemSiteUpdateExtension != null
                    && !inItemSiteUpdateExtension.AMMaxOrdQtyOverride.GetValueOrDefault()
                    && inItemSiteUpdateExtension.AMMaxOrdQty.GetValueOrDefault() != itemExtension.AMMaxOrdQty.GetValueOrDefault())
                {
                    inItemSiteUpdateExtension.AMMaxOrdQty = itemExtension.AMMaxOrdQty.GetValueOrDefault();
                    isUpdated = true;
                }

                // Lot Size
                if (inItemSiteUpdateExtension != null
                    && !inItemSiteUpdateExtension.AMLotSizeOverride.GetValueOrDefault()
                    && inItemSiteUpdateExtension.AMLotSize.GetValueOrDefault() != itemExtension.AMLotSize.GetValueOrDefault())
                {
                    inItemSiteUpdateExtension.AMLotSize = itemExtension.AMLotSize.GetValueOrDefault();
                    isUpdated = true;
                }

                // MFG Lead Time
                if (inItemSiteUpdateExtension != null
                    && !inItemSiteUpdateExtension.AMMFGLeadTimeOverride.GetValueOrDefault()
                    && inItemSiteUpdateExtension.AMMFGLeadTime.GetValueOrDefault() != itemExtension.AMMFGLeadTime.GetValueOrDefault())
                {
                    inItemSiteUpdateExtension.AMMFGLeadTime = itemExtension.AMMFGLeadTime.GetValueOrDefault();
                    isUpdated = true;
                }

                // Group Planning
                if (inItemSiteUpdateExtension != null
                    && !inItemSiteUpdateExtension.AMGroupWindowOverride.GetValueOrDefault()
                    && inItemSiteUpdateExtension.AMGroupWindow.GetValueOrDefault() != itemExtension.AMGroupWindow.GetValueOrDefault())
                {
                    inItemSiteUpdateExtension.AMGroupWindow = itemExtension.AMGroupWindow.GetValueOrDefault();
                    isUpdated = true;
                }

                // Scrap Warehouse and Location
                if (inItemSiteUpdateExtension != null
                    && !inItemSiteUpdateExtension.AMScrapOverride.GetValueOrDefault()
                    && itemExtension.AMScrapSiteID != null
                    && itemExtension.AMScrapLocationID != null)
                {
                    inItemSiteUpdateExtension.AMScrapSiteID = itemExtension.AMScrapSiteID;
                    inItemSiteUpdateExtension.AMScrapLocationID = itemExtension.AMScrapLocationID;
                    isUpdated = true;
                }

                if (isUpdated && Base.itemsiterecords.Cache.GetStatus(inItemSiteUpdate) == PXEntryStatus.Notchanged)
                {
                    Base.itemsiterecords.Cache.SetStatus(inItemSiteUpdate, PXEntryStatus.Updated);
                }
            }
        }

        public virtual void InventoryItem_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e, PXRowUpdated del)
        {
            var inItemSitesBefore = GetCurrentINItemSites();
            del?.Invoke(cache, e);
            SyncReplenishmentSettings(inItemSitesBefore);

            var row = (InventoryItem)e.Row;
            var oldRow = (InventoryItem)e.OldRow;
            if (row == null || oldRow == null)
            {
                return;
            }

                InventoryItemExt itemExt = row.GetExtension<InventoryItemExt>();
                InventoryItemExt oldItemExt = oldRow.GetExtension<InventoryItemExt>();
            if (itemExt == null || oldItemExt == null)
            {
                return;
            }

			if (row.PlanningMethod != oldRow.PlanningMethod || row.ReplenishmentSource != oldRow.ReplenishmentSource)
			{
				if (!(row.ReplenishmentSource == INReplenishmentSource.Transfer || (row.PlanningMethod.IsIn(INPlanningMethod.MRP, INPlanningMethod.DRP) && row.PlanningMethod.IsIn(INReplenishmentSource.Purchased, INReplenishmentSource.PurchaseToOrder))))
				{
					itemExt.AMSourceSiteID = null;
				}
			}

            var itemsiterows = SelectFrom<INItemSite>.
                                Where<INItemSite.FK.InventoryItem.SameAsCurrent>.
                                View.Select(Base);

            foreach (INItemSite itemsite in itemsiterows)
            {
                bool hasChanges = false;
                var itemSiteExt = itemsite.GetExtension<INItemSiteExt>();

				if (row.PlanningMethod != INPlanningMethod.InventoryReplenishment)
				{
					if (itemSiteExt.AMSourceSiteIDOverride.GetValueOrDefault() == false && itemExt.AMSourceSiteID != oldItemExt.AMSourceSiteID || Base.itemsiterecords.Cache.GetStatus(itemsite) == PXEntryStatus.Inserted)
					{
						itemsite.ReplenishmentSourceSiteID = itemExt.AMSourceSiteID;
						hasChanges = true;
					}
					if (itemSiteExt.AMReplenishmentSourceOverride.GetValueOrDefault() == false && row.ReplenishmentSource != oldRow.ReplenishmentSource || Base.itemsiterecords.Cache.GetStatus(itemsite) == PXEntryStatus.Inserted)
					{
						itemsite.ReplenishmentSource = row.ReplenishmentSource;
						hasChanges = true;
					}
					if (itemSiteExt.AMSafetyStockOverride.GetValueOrDefault() == false && itemExt.AMSafetyStock != oldItemExt.AMSafetyStock || Base.itemsiterecords.Cache.GetStatus(itemsite) == PXEntryStatus.Inserted)
					{
						itemsite.SafetyStock = itemExt.AMSafetyStock;
						hasChanges = true;
					}
					if (itemSiteExt.AMMinQtyOverride.GetValueOrDefault() == false && itemExt.AMMinQty != oldItemExt.AMMinQty || Base.itemsiterecords.Cache.GetStatus(itemsite) == PXEntryStatus.Inserted)
					{
						itemsite.MinQty = itemExt.AMMinQty;
						hasChanges = true;
					}
				}
                if (itemSiteExt.AMMinOrdQtyOverride.GetValueOrDefault() == false && itemExt.AMMinOrdQty != oldItemExt.AMMinOrdQty || Base.itemsiterecords.Cache.GetStatus(itemsite) == PXEntryStatus.Inserted)
                {
                    itemSiteExt.AMMinOrdQty = itemExt.AMMinOrdQty;
                    hasChanges = true;
                }
                if (itemSiteExt.AMMaxOrdQtyOverride.GetValueOrDefault() == false && itemExt.AMMaxOrdQty != oldItemExt.AMMaxOrdQty || Base.itemsiterecords.Cache.GetStatus(itemsite) == PXEntryStatus.Inserted)
                {
                    itemSiteExt.AMMaxOrdQty = itemExt.AMMaxOrdQty;
                    hasChanges = true;
                }
                if (itemSiteExt.AMLotSizeOverride.GetValueOrDefault() == false && itemExt.AMLotSize != oldItemExt.AMLotSize || Base.itemsiterecords.Cache.GetStatus(itemsite) == PXEntryStatus.Inserted)
                {
                    itemSiteExt.AMLotSize = itemExt.AMLotSize;
                    hasChanges = true;
                }
                if (itemSiteExt.AMGroupWindowOverride.GetValueOrDefault() == false && !cache.ObjectsEqual<InventoryItemExt.aMGroupWindow>(itemExt, oldItemExt) || Base.itemsiterecords.Cache.GetStatus(itemsite) == PXEntryStatus.Inserted)
                {
                    itemSiteExt.AMGroupWindow = itemExt.AMGroupWindow;
                    hasChanges = true;
                }
                if (itemSiteExt.AMMFGLeadTimeOverride.GetValueOrDefault() == false && itemExt.AMMFGLeadTime != oldItemExt.AMMFGLeadTime || Base.itemsiterecords.Cache.GetStatus(itemsite) == PXEntryStatus.Inserted)
                {
                    itemSiteExt.AMMFGLeadTime = itemExt.AMMFGLeadTime;
                    hasChanges = true;
                }
				if (!itemSiteExt.AMTransferLeadTimeOverride.GetValueOrDefault() && itemsite.ReplenishmentSource == INReplenishmentSource.Transfer)
				{
					AMSiteTransfer leadTime = SelectFrom<AMSiteTransfer>.
						Where<AMSiteTransfer.siteID.IsEqual<@P.AsInt>.
							And<AMSiteTransfer.transferSiteID.IsEqual<@P.AsInt>.
						And<INItemSiteExt.aMTransferLeadTimeOverride.FromCurrent.IsEqual<False>>>>.View.Select(Base, itemsite.SiteID, itemsite.ReplenishmentSourceSiteID);
						itemSiteExt.AMTransferLeadTime = leadTime?.TransferLeadTime;
				}
                if (hasChanges)
                    Base.itemsiterecords.Cache.MarkUpdated(itemsite, assertError: true);
            }
        }

        protected virtual void INItemRep_RowInserted(PXCache cache, PXRowInsertedEventArgs e, PXRowInserted del)
        {
            var inItemSitesBefore = GetCurrentINItemSites();
            if (del != null)
            {
                del(cache, e);
            }
			SyncStockItemReplenishmentSettings((INItemRep)e.Row, e);
			SyncReplenishmentSettings(inItemSitesBefore);
        }
        protected virtual void INItemRep_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e, PXRowUpdated del)
        {
            var inItemSitesBefore = GetCurrentINItemSites();
            if (del != null)
            {
                del(cache, e);
            }
			SyncStockItemReplenishmentSettings((INItemRep)e.Row, e);
			SyncReplenishmentSettings(inItemSitesBefore);
        }

		protected virtual void INItemRep_RowDeleted(PXCache cache, PXRowDeletedEventArgs e, PXRowDeleted del)
        {
            var inItemSitesBefore = GetCurrentINItemSites();
            if (del != null)
            {
                del(cache, e);
            }
			SyncStockItemReplenishmentSettings((INItemRep)e.Row, e);
			SyncReplenishmentSettings(inItemSitesBefore);
        }

        protected virtual void SyncStockItemReplenishmentSettings(INItemRep inItemRep, EventArgs eventArgs)
        {
            if (inItemRep == null || !FullReplenishmentsEnabled
                || string.IsNullOrWhiteSpace(inItemRep.ReplenishmentClassID))
            {
                return;
            }

            var inventoryItemCache = Base.Caches[typeof(InventoryItem)];
            var inventoryItem = (InventoryItem)inventoryItemCache.Current;
            if (inventoryItem == null) return;

            var itemExtension = Base.Item?.Current?.GetExtension<InventoryItemExt>();
            if (itemExtension == null)
            {
                return;
            }

            var inItemRepReferenced = inItemRep.ReplenishmentSource.EqualsWithTrim(inventoryItem.ReplenishmentSource) ? inItemRep : GetINItemRep(inventoryItem.ReplenishmentSource);

            // INSERTED and no other matching reps
            if (!inItemRep.ReplenishmentSource.EqualsWithTrim(inventoryItem.ReplenishmentSource)
                && eventArgs is PXRowInsertedEventArgs
                && (inventoryItem.ReplenishmentSource == null
                || inItemRepReferenced != null))
            {
                SetAMReplenishmentValues(inItemRepReferenced);
                return;
            }

            // UPDATED and source is a match
            if ((inItemRep.ReplenishmentSource.EqualsWithTrim(inventoryItem.ReplenishmentSource) || inItemRepReferenced == null)
                && eventArgs is PXRowUpdatedEventArgs)
            {
                SetAMReplenishmentValues(inItemRepReferenced ?? inItemRep);
                return;
            }

            // DELETED and the source needs to be reset (or pick another existing initemrep record)
            if (inItemRep.ReplenishmentSource.EqualsWithTrim(inventoryItem.ReplenishmentSource)
                && eventArgs is PXRowDeletedEventArgs)
            {
                var newINItemRepRef = GetFirstINItemRepNotMatchingSource(inItemRep.ReplenishmentSource);

                SetAMReplenishmentValues(newINItemRepRef);
                return;
            }
        }

        protected virtual void SetAMReplenishmentValues(INItemRep inItemRep)
        {
            if (inItemRep == null
                || string.IsNullOrWhiteSpace(inItemRep.ReplenishmentClassID))
            {
                SetAMReplenishmentDefaultValues();
                return;
            }

            var itemExtension = Base.Item.Current.GetExtension<InventoryItemExt>();
            if (itemExtension == null)
            {
                return;
            }
        }

        /// <summary>
        /// Reset Manufacturing replenishment settings on stock items to default values
        /// </summary>
        protected virtual void SetAMReplenishmentDefaultValues()
        {
            var itemExtension = Base.Item.Current.GetExtension<InventoryItemExt>();
            if (itemExtension == null)
            {
                return;
            }
        }

        protected virtual INItemRep GetINItemRep(string replenishmentSource)
        {
            if (string.IsNullOrWhiteSpace(replenishmentSource))
            {
                return null;
            }

            //Do not convert to linq - will receive invalid cast in some instances
            foreach (INItemRep inItemRep in Base.replenishment.Select())
            {
                if (inItemRep.ReplenishmentSource.EqualsWithTrim(replenishmentSource))
                {
                    return inItemRep;
                }
            }
            return null;
        }

        protected virtual INItemRep GetFirstINItemRepNotMatchingSource(string replenishmentSource)
        {
            if (string.IsNullOrWhiteSpace(replenishmentSource))
            {
                return null;
            }

            //Do not convert to linq - will receive invalid cast in some instances
            foreach (INItemRep inItemRep in Base.replenishment.Select())
            {
                if (!inItemRep.ReplenishmentSource.EqualsWithTrim(replenishmentSource))
                {
                    return inItemRep;
                }
            }
            return null;
        }

        protected virtual void InventoryItem_AMReplenishmentSource_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (InventoryItem)e.Row;
            var rowExt = row.GetExtension<InventoryItemExt>();

            if (row == null)
            {
                return;
            }

            bool isManufactured = row.ReplenishmentSource == INReplenishmentSource.Manufactured;

            if (!isManufactured)
            {
                Base.Item.Cache.SetValueExt<InventoryItemExt.aMMakeToOrderItem>(Base.Item.Current, false);
            }
        }

        protected virtual void InventoryItem_PostClassID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e, PXFieldUpdated del)
        {
            del?.Invoke(sender, e);
            sender.SetDefaultExt<InventoryItemExt.aMWIPVarianceAcctID>(e.Row);
            sender.SetDefaultExt<InventoryItemExt.aMWIPVarianceSubID>(e.Row);
            sender.SetDefaultExt<InventoryItemExt.aMWIPAcctID>(e.Row);
            sender.SetDefaultExt<InventoryItemExt.aMWIPSubID>(e.Row);
		}

        protected virtual void _(Events.FieldUpdated<InventoryItem, InventoryItem.itemClassID> e, PXFieldUpdated baseMethod)
        {
            baseMethod?.Invoke(e.Cache, e.Args);

            if (Base.doResetDefaultsOnItemClassChange == true)
            {
                InventoryItemExt extension = PXCache<InventoryItem>.GetExtension<InventoryItemExt>(e.Row);

                e.Cache.SetDefaultExt<InventoryItemExt.aMSourceSiteID>(e.Row);
                e.Cache.SetDefaultExt<InventoryItemExt.aMSafetyStock>(e.Row);
                e.Cache.SetDefaultExt<InventoryItemExt.aMMinQty>(e.Row);
                e.Cache.SetDefaultExt<InventoryItemExt.aMMinOrdQty>(e.Row);
                e.Cache.SetDefaultExt<InventoryItemExt.aMMaxOrdQty>(e.Row);
                e.Cache.SetDefaultExt<InventoryItemExt.aMLotSize>(e.Row);
                e.Cache.SetDefaultExt<InventoryItemExt.aMMFGLeadTime>(e.Row);

                if (extension.AMGroupWindowOverride == false && IsConsolidateOrdersVisible == true)
                {
                    e.Cache.SetDefaultExt<InventoryItemExt.aMGroupWindow>(e.Row);
                }
				foreach (INItemSite itemSite in Base.itemsiterecords.Cache.Inserted)
				{
					itemSite.PlanningMethod = e.Row.PlanningMethod;
				}

            }
        }

        protected virtual void _(Events.FieldUpdated<InventoryItem, InventoryItemExt.aMGroupWindowOverride> e, PXFieldUpdated baseMethod)
        {
            baseMethod?.Invoke(e.Cache, e.Args);
            if (IsConsolidateOrdersVisible == true)
            {
                InventoryItemExt extension = PXCache<InventoryItem>.GetExtension<InventoryItemExt>(e.Row);

                if (extension.AMGroupWindowOverride == false)
                {
                    e.Cache.SetDefaultExt<InventoryItemExt.aMGroupWindow>(e.Row);
                }
            }
        }

		public PXAction<InventoryItem> ViewBOM;
		[PXButton]
		[PXUIField(DisplayName = "View BOM", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable viewBOM(PXAdapter adapter)
		{
			var itemExtension = Base.Item.Current.GetExtension<InventoryItemExt>();
			var currentInventoryItem = Base.Item.Current;
			if (itemExtension.AMBOMID == null && currentInventoryItem != null)
			{
				BOMMaint.RedirectNew(currentInventoryItem.InventoryID > 0 ? currentInventoryItem.InventoryID : null, currentInventoryItem.DfltSiteID);
			}
			BOMMaint.RedirectToBOM(itemExtension.AMBOMID);

			return adapter.Get();
		}

		public PXAction<InventoryItem> ViewPlanningBOM;
		[PXButton]
		[PXUIField(DisplayName = "View Planning BOM", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable viewPlanningBOM(PXAdapter adapter)
		{
			var itemExtension = Base.Item.Current.GetExtension<InventoryItemExt>();
			var currentInventoryItem = Base.Item.Current;
			if (itemExtension.AMPlanningBOMID == null && currentInventoryItem != null)
			{
				BOMMaint.RedirectNew(currentInventoryItem.InventoryID > 0 ? currentInventoryItem.InventoryID : null, currentInventoryItem.DfltSiteID);
			}
			BOMMaint.RedirectToBOM(itemExtension.AMPlanningBOMID);

			return adapter.Get();
		}
	}
}
