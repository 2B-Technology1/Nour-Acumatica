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

using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.CS;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.AM.GraphExtensions
{
	public class INItemClassMaintAMExtension : PXGraphExtension<INItemClassMaint>
	{
		public SelectFrom<INItemClass>.
			Where<INItemClass.itemClassID.IsEqual<INItemClass.itemClassID.FromCurrent>>.View inventoryPlanningSettings;
		public SelectFrom<INItemClass>.
			Where<INItemClass.itemClassID.IsEqual<INItemClass.itemClassID.FromCurrent>>.View productionOrderDefaultSettings;

		public static bool IsActive()
		{
			return Features.ManufacturingOrDRPOrReplenishmentEnabled();
		}


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

			PXUIFieldAttribute.SetVisible<INItemClassExt.aMDaysSupply>(Base.itemclass.Cache, null, IsConsolidateOrdersVisible);
		}

		[PXOverride]
		public void Persist(Action del)
		{

			if (Base.itemclass.Current != null && Base.itemclass.Cache.GetStatus(Base.itemclass.Current) == PXEntryStatus.Updated)
			{
				INItemClass oldrow = (INItemClass)Base.itemclass.Cache.GetOriginal(Base.itemclass.Current);
				INItemClass row = Base.itemclass.Current;

				if (Base.itemclass.Cache.ObjectsEqual<INItemClassExt.aMDaysSupply>(row, oldrow) == false)
				{
					var inventoryItemsBefore = GetCurrentInventoryItems(row);
					SyncDaysSupply(inventoryItemsBefore);
				}
			}

			del.Invoke();
		}


		private List<InventoryItem> GetCurrentInventoryItems(INItemClass e)
		{
			return SelectFrom<InventoryItem>.
				   Where<InventoryItem.itemClassID.IsEqual<@P.AsInt>>
				   .View.Select(Base, e.ItemClassID)?.FirstTableItems.ToList();

		}

		private List<INItemSite> GetCurrentINItemSites(InventoryItem e)
		{
			return SelectFrom<INItemSite>.
				   Where<INItemSite.inventoryID.IsEqual<@P.AsInt>>
				   .View.Select(Base, e.InventoryID)?.FirstTableItems.ToList();

		}

		protected virtual void SyncDaysSupply(List<InventoryItem> inventoryItemsToSync)
		{
			if (inventoryItemsToSync == null || inventoryItemsToSync.Count == 0)
			{
				return;
			}
			Common.Cache.AddCacheView<INItemSite>(Base);

			var itemClassExtension = PXCache<INItemClass>.GetExtension<INItemClassExt>(Base.itemclass.Current);

			foreach (InventoryItem inventoryItem in inventoryItemsToSync)
			{
				bool isUpdated = false;
				InventoryItem inventoryItemUpdate = (InventoryItem)Base.Items.Cache.LocateElse(inventoryItem);

				if (inventoryItemUpdate == null)
				{
					continue;
				}

				var inventoryItemExtension = PXCache<InventoryItem>.GetExtension<InventoryItemExt>(inventoryItemUpdate);

				//Days Supply
				if (inventoryItemExtension != null
					&& inventoryItemExtension.AMGroupWindow.GetValueOrDefault() != itemClassExtension.AMDaysSupply.GetValueOrDefault()
					&& inventoryItemExtension.AMGroupWindowOverride == false)
				{
					inventoryItemExtension.AMGroupWindow = itemClassExtension.AMDaysSupply.GetValueOrDefault();
					isUpdated = true;
				}

				//Check ItemSite records and update them
				var inItemSiteToSync = GetCurrentINItemSites(inventoryItemUpdate);

				foreach (INItemSite inItemSite in inItemSiteToSync)
				{
					bool isItemClassUpdated = false;

					INItemSite inItemSiteUpdate = (INItemSite)Base.Caches[typeof(INItemSite)].LocateElse(inItemSite);

					if (inItemSiteUpdate == null)
					{
						continue;
					}

					var inItemSiteExt = PXCache<INItemSite>.GetExtension<CacheExtensions.INItemSiteExt>(inItemSiteUpdate);

					//Days Supply
					if (inItemSiteExt != null
						&& inItemSiteExt.AMGroupWindow.GetValueOrDefault() != itemClassExtension.AMDaysSupply.GetValueOrDefault()
						&& inItemSiteExt.AMGroupWindowOverride == false)
					{
						inItemSiteExt.AMGroupWindow = itemClassExtension.AMDaysSupply.GetValueOrDefault();
						isItemClassUpdated = true;
					}

					if (isItemClassUpdated)
					{
						Base.Caches[typeof(INItemSite)].Update(inItemSiteUpdate);
					}
				}

				if (isUpdated)
				{
					Base.Items.Update(inventoryItemUpdate);
				}

			}
		}

		protected virtual void _(Events.RowSelected<INItemClass> e)
		{
			if (e.Row == null) return;
			var hasMRPorDRP = PXAccess.FeatureInstalled<FeaturesSet.manufacturingMRP>() || PXAccess.FeatureInstalled<FeaturesSet.distributionReqPlan>();
			var hasManufacturing = PXAccess.FeatureInstalled<FeaturesSet.manufacturing>();
			inventoryPlanningSettings.AllowSelect = e.Row.StkItem.GetValueOrDefault() && e.Row.PlanningMethod.IsIn(INPlanningMethod.MRP, INPlanningMethod.DRP) && hasMRPorDRP;
			Base.replenishment.AllowSelect = e.Row.StkItem.GetValueOrDefault() && e.Row.PlanningMethod == INPlanningMethod.InventoryReplenishment && PXAccess.FeatureInstalled<FeaturesSet.replenishment>();
			productionOrderDefaultSettings.AllowSelect = e.Row.StkItem.GetValueOrDefault() && hasManufacturing && (!hasMRPorDRP || e.Row.PlanningMethod.IsNotIn(INPlanningMethod.MRP, INPlanningMethod.DRP));

		}

		public virtual void _(Events.FieldUpdated<INItemClass.replenishmentSource> e) 
		{
			ResetReplenishmentSite((INItemClass)e.Row, e.NewValue);
		}
		public virtual void _(Events.FieldUpdated<INItemClass.planningMethod> e)
		{
			ResetReplenishmentSite((INItemClass)e.Row, e.NewValue);
		}
		public virtual void ResetReplenishmentSite(INItemClass row, Object newValue)
		{ 
			if (row != null && newValue != null)
			{
				if (!(newValue.ToString() == INReplenishmentSource.Transfer || (row.PlanningMethod.IsIn(INPlanningMethod.MRP, INPlanningMethod.DRP) && newValue.IsIn(INReplenishmentSource.Purchased, INReplenishmentSource.PurchaseToOrder))))
				{
					INItemClassExt itemExt = PXCache<INItemClass>.GetExtension<INItemClassExt>(row);
					itemExt.AMSourceSiteID = null;
				}
			}
		}
	}
}
