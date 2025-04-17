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
using PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated;
using PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated.Abstraction;
using ItemLotSerial = PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated.ItemLotSerial;
using SiteLotSerial = PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated.SiteLotSerial;

namespace PX.Objects.IN.GraphExtensions
{
	public abstract class ItemPlanHelper<TGraph> : PXGraphExtension<TGraph>
		where TGraph : PXGraph
	{
		public virtual INPlanType GetTargetPlanType<TNode>(INItemPlan plan, INPlanType plantype)
			where TNode : class, IQtyAllocatedBase
		{
			if (plan.ExcludePlanLevel != null &&
				(typeof(TNode) == typeof(SiteStatusByCostCenter) && (plan.ExcludePlanLevel & INPlanLevel.ExcludeSite) == INPlanLevel.ExcludeSite ||
				typeof(TNode) == typeof(LocationStatusByCostCenter) && (plan.ExcludePlanLevel & INPlanLevel.ExcludeLocation) == INPlanLevel.ExcludeLocation ||
				typeof(TNode) == typeof(LotSerialStatusByCostCenter) && (plan.ExcludePlanLevel & INPlanLevel.ExcludeLocationLotSerial) == INPlanLevel.ExcludeLocationLotSerial ||
				typeof(TNode) == typeof(ItemLotSerial) && (plan.ExcludePlanLevel & INPlanLevel.ExcludeLotSerial) == INPlanLevel.ExcludeLotSerial ||
				typeof(TNode) == typeof(SiteLotSerial) && (plan.ExcludePlanLevel & INPlanLevel.ExcludeSiteLotSerial) == INPlanLevel.ExcludeSiteLotSerial))
			{
				return (INPlanType)0;
			}

			if (plan.IgnoreOrigPlan != true && !string.IsNullOrEmpty(plan.OrigPlanType))
			{
				if (typeof(TNode) == typeof(SiteStatusByCostCenter) ||
					typeof(TNode) == typeof(LocationStatusByCostCenter) && (plan.OrigPlanLevel & INPlanLevel.Location) == INPlanLevel.Location ||
					typeof(TNode) == typeof(LotSerialStatusByCostCenter) && (plan.OrigPlanLevel & INPlanLevel.LocationLotSerial) == INPlanLevel.LocationLotSerial ||
					typeof(TNode) == typeof(ItemLotSerial) && (plan.OrigPlanLevel & INPlanLevel.LotSerial) == INPlanLevel.LotSerial ||
					typeof(TNode) == typeof(SiteLotSerial) && (plan.OrigPlanLevel & INPlanLevel.LotSerial) == INPlanLevel.LotSerial)
				{
					INPlanType origPlanType = INPlanType.PK.Find(Base, plan.OrigPlanType);
					return plantype > 0 ? plantype - origPlanType : plantype + origPlanType;
				}
			}

			return plantype;
		}

		public virtual TNode UpdateAllocatedQuantitiesBase<TNode>(INItemPlan plan, INPlanType plantype, bool InclQtyAvail)
			where TNode : class, IQtyAllocatedBase
		{
			bool isDirty = Base.Caches[typeof(TNode)].IsDirty;
			TNode target = (TNode)Base.Caches[typeof(TNode)].Insert(ConvertPlan<TNode>(plan));
			Base.Caches[typeof(TNode)].IsDirty = isDirty;

			return UpdateAllocatedQuantitiesBase<TNode>(target, plan, plantype, InclQtyAvail, plan.Hold, plan.RefEntityType);
		}

		protected TNode ConvertPlan<TNode>(INItemPlan item)
			where TNode : class, IQtyAllocatedBase
		{
			if (typeof(TNode) == typeof(ItemLotSerial))
			{
				return INItemPlan.ToItemLotSerial(item) as TNode;
			}
			if (typeof(TNode) == typeof(SiteLotSerial))
			{
				return INItemPlan.ToSiteLotSerial(item) as TNode;
			}
			if (typeof(TNode) == typeof(SiteStatusByCostCenter))
			{
				return INItemPlan.ToSiteStatusByCostCenter(item) as TNode;
			}
			if (typeof(TNode) == typeof(LocationStatusByCostCenter))
			{
				return INItemPlan.ToLocationStatusByCostCenter(item) as TNode;
			}
			if (typeof(TNode) == typeof(LotSerialStatusByCostCenter))
			{
				return INItemPlan.ToLotSerialStatusByCostCenter(item) as TNode;
			}

			return null;
		}

		public virtual TNode UpdateAllocatedQuantitiesBase<TNode>(TNode target, IQtyPlanned plan, INPlanType plantype, bool? InclQtyAvail, bool? hold)
			where TNode : class, IQtyAllocatedBase
		{
			return UpdateAllocatedQuantitiesBase<TNode>(target, plan, plantype, InclQtyAvail, hold, null);
		}

		public virtual TNode UpdateAllocatedQuantitiesBase<TNode>(TNode target, IQtyPlanned plan, INPlanType plantype, bool? InclQtyAvail, bool? hold, string refEntityType)
			where TNode : class, IQtyAllocatedBase
		{
			decimal qty = plan.PlanQty ?? 0;
			if (plan.Reverse == true)
			{
				if (target.InclQtySOReverse != true && IsSORelated(plantype))
					return target;
				else
					qty = -qty;
			}

			if (hold == true &&
				INPlanConstants.ToModuleField(plantype.PlanType) == GL.BatchModule.IN &&
				!string.Equals(refEntityType, typeof(PO.POReceipt).FullName, StringComparison.OrdinalIgnoreCase) &&
				!GetAllocateDocumentsOnHold())
			{
				qty = 0m;
			}

			if (target is IQtyAllocated exttarget)
			{
				exttarget.QtyINIssues += (plantype.InclQtyINIssues ?? 0) * qty;
				exttarget.QtyINReceipts += (plantype.InclQtyINReceipts ?? 0) * qty;
				exttarget.QtyPOPrepared += (plantype.InclQtyPOPrepared ?? 0) * qty;
				exttarget.QtyPOOrders += (plantype.InclQtyPOOrders ?? 0) * qty;
				exttarget.QtyPOReceipts += (plantype.InclQtyPOReceipts ?? 0) * qty;

				exttarget.QtyFSSrvOrdPrepared += (plantype.InclQtyFSSrvOrdPrepared ?? 0) * qty;
				exttarget.QtyFSSrvOrdBooked += (plantype.InclQtyFSSrvOrdBooked ?? 0) * qty;
				exttarget.QtyFSSrvOrdAllocated += (plantype.InclQtyFSSrvOrdAllocated ?? 0) * qty;

				exttarget.QtySOBackOrdered += (plantype.InclQtySOBackOrdered ?? 0) * qty;
				exttarget.QtySOPrepared += (plantype.InclQtySOPrepared ?? 0) * qty;
				exttarget.QtySOBooked += (plantype.InclQtySOBooked ?? 0) * qty;
				exttarget.QtySOShipped += (plantype.InclQtySOShipped ?? 0) * qty;
				exttarget.QtySOShipping += (plantype.InclQtySOShipping ?? 0) * qty;
				exttarget.QtyINAssemblySupply += (plantype.InclQtyINAssemblySupply ?? 0) * qty;
				exttarget.QtyINAssemblyDemand += (plantype.InclQtyINAssemblyDemand ?? 0) * qty;
				exttarget.QtyInTransitToProduction += (plantype.InclQtyInTransitToProduction ?? 0) * qty;
				exttarget.QtyProductionSupplyPrepared += (plantype.InclQtyProductionSupplyPrepared ?? 0) * qty;
				exttarget.QtyProductionSupply += (plantype.InclQtyProductionSupply ?? 0) * qty;
				exttarget.QtyPOFixedProductionPrepared += (plantype.InclQtyPOFixedProductionPrepared ?? 0) * qty;
				exttarget.QtyPOFixedProductionOrders += (plantype.InclQtyPOFixedProductionOrders ?? 0) * qty;
				exttarget.QtyProductionDemandPrepared += (plantype.InclQtyProductionDemandPrepared ?? 0) * qty;
				exttarget.QtyProductionDemand += (plantype.InclQtyProductionDemand ?? 0) * qty;
				exttarget.QtyProductionAllocated += (plantype.InclQtyProductionAllocated ?? 0) * qty;
				exttarget.QtySOFixedProduction += (plantype.InclQtySOFixedProduction ?? 0) * qty;
				exttarget.QtyProdFixedPurchase += (plantype.InclQtyProdFixedPurchase ?? 0) * qty;
				exttarget.QtyProdFixedProduction += (plantype.InclQtyProdFixedProduction ?? 0) * qty;
				exttarget.QtyProdFixedProdOrdersPrepared += (plantype.InclQtyProdFixedProdOrdersPrepared ?? 0) * qty;
				exttarget.QtyProdFixedProdOrders += (plantype.InclQtyProdFixedProdOrders ?? 0) * qty;
				exttarget.QtyProdFixedSalesOrdersPrepared += (plantype.InclQtyProdFixedSalesOrdersPrepared ?? 0) * qty;
				exttarget.QtyProdFixedSalesOrders += (plantype.InclQtyProdFixedSalesOrders ?? 0) * qty;
				exttarget.QtyINReplaned += (plantype.InclQtyINReplaned ?? 0) * qty;

				exttarget.QtyFixedFSSrvOrd += (plantype.InclQtyFixedFSSrvOrd ?? 0) * qty;
				exttarget.QtyPOFixedFSSrvOrd += (plantype.InclQtyPOFixedFSSrvOrd ?? 0) * qty;
				exttarget.QtyPOFixedFSSrvOrdPrepared += (plantype.InclQtyPOFixedFSSrvOrdPrepared ?? 0) * qty;
				exttarget.QtyPOFixedFSSrvOrdReceipts += (plantype.InclQtyPOFixedFSSrvOrdReceipts ?? 0) * qty;

				exttarget.QtySOFixed += (plantype.InclQtySOFixed ?? 0) * qty;
				exttarget.QtyPOFixedOrders += (plantype.InclQtyPOFixedOrders ?? 0) * qty;
				exttarget.QtyPOFixedPrepared += (plantype.InclQtyPOFixedPrepared ?? 0) * qty;
				exttarget.QtyPOFixedReceipts += (plantype.InclQtyPOFixedReceipts ?? 0) * qty;
				exttarget.QtySODropShip += (plantype.InclQtySODropShip ?? 0) * qty;
				exttarget.QtyPODropShipOrders += (plantype.InclQtyPODropShipOrders ?? 0) * qty;
				exttarget.QtyPODropShipPrepared += (plantype.InclQtyPODropShipPrepared ?? 0) * qty;
				exttarget.QtyPODropShipReceipts += (plantype.InclQtyPODropShipReceipts ?? 0) * qty;
				exttarget.QtyInTransitToSO += (plantype.InclQtyInTransitToSO ?? 0) * qty;
			}
			target.QtyInTransit += (plantype.InclQtyInTransit ?? 0) * qty;

			decimal avail = 0m, hardAvail = 0m, actual = 0m, receipts = 0m;

			avail -= target.InclQtyINIssues == true ? (plantype.InclQtyINIssues ?? 0) * qty : 0m;
			avail += target.InclQtyINReceipts == true ? (plantype.InclQtyINReceipts ?? 0) * qty : 0m;
			avail += target.InclQtyInTransit == true ? (plantype.InclQtyInTransit ?? 0) * qty : 0m;
			avail += target.InclQtyPOPrepared == true ? (plantype.InclQtyPOPrepared ?? 0) * qty : 0m;
			avail += target.InclQtyPOOrders == true ? (plantype.InclQtyPOOrders ?? 0) * qty : 0m;
			avail += target.InclQtyPOReceipts == true ? (plantype.InclQtyPOReceipts ?? 0) * qty : 0m;
			avail += target.InclQtyINAssemblySupply == true ? (plantype.InclQtyINAssemblySupply ?? 0) * qty : 0m;
			avail += target.InclQtyProductionSupplyPrepared == true ? (plantype.InclQtyProductionSupplyPrepared ?? 0) * qty : 0m;
			avail += target.InclQtyProductionSupply == true ? (plantype.InclQtyProductionSupply ?? 0) * qty : 0m;

			avail -= target.InclQtyFSSrvOrdPrepared == true ? (plantype.InclQtyFSSrvOrdPrepared ?? 0) * qty : 0m;
			avail -= target.InclQtyFSSrvOrdBooked == true ? (plantype.InclQtyFSSrvOrdBooked ?? 0) * qty : 0m;
			avail -= target.InclQtyFSSrvOrdAllocated == true ? (plantype.InclQtyFSSrvOrdAllocated ?? 0) * qty : 0m;

			avail -= target.InclQtySOBackOrdered == true ? (plantype.InclQtySOBackOrdered ?? 0) * qty : 0m;
			avail -= target.InclQtySOPrepared == true ? (plantype.InclQtySOPrepared ?? 0) * qty : 0m;
			avail -= target.InclQtySOBooked == true ? (plantype.InclQtySOBooked ?? 0) * qty : 0m;
			avail -= target.InclQtySOShipped == true ? (plantype.InclQtySOShipped ?? 0) * qty : 0m;
			avail -= target.InclQtySOShipping == true ? (plantype.InclQtySOShipping ?? 0) * qty : 0m;
			avail -= target.InclQtyINAssemblyDemand == true ? (plantype.InclQtyINAssemblyDemand ?? 0) * qty : 0m;
			avail -= target.InclQtyProductionDemandPrepared == true ? (plantype.InclQtyProductionDemandPrepared ?? 0) * qty : 0m;
			avail -= target.InclQtyProductionDemand == true ? (plantype.InclQtyProductionDemand ?? 0) * qty : 0m;
			avail -= target.InclQtyProductionAllocated == true ? (plantype.InclQtyProductionAllocated ?? 0) * qty : 0m;
			avail += target.InclQtyPOFixedReceipt == true ? (plantype.InclQtyPOFixedReceipts ?? 0) * qty : 0m;

			if (target.InclQtyFixedSOPO == true)
			{
				avail += target.InclQtyPOOrders == true ? (plantype.InclQtyPOFixedOrders ?? 0) * qty : 0m;
				avail += target.InclQtyPOPrepared == true ? (plantype.InclQtyPOFixedPrepared ?? 0) * qty : 0m;
				avail += target.InclQtyPOReceipts == true ? (plantype.InclQtyPOFixedReceipts ?? 0) * qty : 0m;
				avail -= target.InclQtySOBooked == true ? (plantype.InclQtySOFixed ?? 0) * qty : 0m;
			}

			var receiptTarget = target as IQtyAllocatedSeparateReceipts;
			if (receiptTarget != null)
			{
				if (plan.Reverse != true)
				{
					receipts += target.InclQtyINReceipts == true ? (plantype.InclQtyINReceipts ?? 0) * qty : 0m;
					receipts += target.InclQtyPOPrepared == true ? (plantype.InclQtyPOPrepared ?? 0) * qty : 0m;
					receipts += target.InclQtyPOOrders == true ? (plantype.InclQtyPOOrders ?? 0) * qty : 0m;
					receipts += target.InclQtyPOReceipts == true ? (plantype.InclQtyPOReceipts ?? 0) * qty : 0m;
					receipts += target.InclQtyPOFixedReceipt == true ? (plantype.InclQtyPOFixedReceipts ?? 0) * qty : 0m;
					receipts += target.InclQtyINAssemblySupply == true ? (plantype.InclQtyINAssemblySupply ?? 0) * qty : 0m;
				}
				else
				{
					receipts -= target.InclQtySOBackOrdered == true ? (plantype.InclQtySOBackOrdered ?? 0) * qty : 0m;
					receipts -= target.InclQtySOPrepared == true ? (plantype.InclQtySOPrepared ?? 0) * qty : 0m;
					receipts -= target.InclQtySOBooked == true ? (plantype.InclQtySOBooked ?? 0) * qty : 0m;
					receipts -= target.InclQtySOShipped == true ? (plantype.InclQtySOShipped ?? 0) * qty : 0m;
					receipts -= target.InclQtySOShipping == true ? (plantype.InclQtySOShipping ?? 0) * qty : 0m;

					receipts -= target.InclQtyFSSrvOrdPrepared == true ? (plantype.InclQtyFSSrvOrdPrepared ?? 0) * qty : 0m;
					receipts -= target.InclQtyFSSrvOrdBooked == true ? (plantype.InclQtyFSSrvOrdBooked ?? 0) * qty : 0m;
					receipts -= target.InclQtyFSSrvOrdAllocated == true ? (plantype.InclQtyFSSrvOrdAllocated ?? 0) * qty : 0m;
				}
			}

			if (plan.Reverse != true)
			{
				//Has to be synchronized with InventoryAllocDetEnq.filter(), part "filter.QtyHardAvail =..."
				hardAvail -= (plantype.InclQtySOShipped ?? 0) * qty;
				hardAvail -= (plantype.InclQtySOShipping ?? 0) * qty;
				hardAvail -= (plantype.InclQtyINIssues ?? 0) * qty;
				hardAvail -= (plantype.InclQtyProductionAllocated ?? 0) * qty;

				hardAvail -= (plantype.InclQtyFSSrvOrdAllocated ?? 0) * qty;
				hardAvail -= (plantype.InclQtyINAssemblyDemand ?? 0) * qty;

				actual -= (plantype.InclQtySOShipped ?? 0) * qty;
			}

			if (InclQtyAvail == true)
			{
				target.QtyAvail += avail;
				target.QtyHardAvail += hardAvail;
				target.QtyActual += actual;
				if (receiptTarget != null)
				{
					receiptTarget.QtyOnReceipt += receipts;
				}
			}
			else if (InclQtyAvail == false)
			{
				target.QtyNotAvail += avail;

				// crutch for SO Booked which is shipped from Not Available Locations
				if (typeof(TNode) == typeof(SiteStatusByCostCenter) && ((plantype.InclQtySOShipping ?? 0) != 0))
				{
					target.QtyNotAvail += target.InclQtySOBooked == true ? (plantype.InclQtySOBooked ?? 0) * qty : 0m;
					target.QtyAvail -= target.InclQtySOBooked == true ? (plantype.InclQtySOBooked ?? 0) * qty : 0m;
				}
			}

			// temporary solution for 23R1 update, should be changed in a major release
			if (target is SiteLotSerial siteLotSerial)
			{
				if (InclQtyAvail == true)
				{
					siteLotSerial.QtyAvailOnSite += avail;
				}
				else
				{
					target.QtyAvail += avail;
					target.QtyHardAvail += hardAvail;
					target.QtyActual += actual;
					if (receiptTarget != null)
					{
						receiptTarget.QtyOnReceipt += receipts;
					}
				}
			}

			return target;
		}

		protected bool IsSORelated(INPlanType plantype)
		{
			return plantype.InclQtySOBackOrdered.GetValueOrDefault() != 0
				|| plantype.InclQtySOBooked.GetValueOrDefault() != 0
				|| plantype.InclQtySODropShip.GetValueOrDefault() != 0
				|| plantype.InclQtySOFixed.GetValueOrDefault() != 0
				|| plantype.InclQtySOFixedProduction.GetValueOrDefault() != 0
				|| plantype.InclQtySOPrepared.GetValueOrDefault() != 0
				|| plantype.InclQtySOShipped.GetValueOrDefault() != 0
				|| plantype.InclQtySOShipping.GetValueOrDefault() != 0;
		}

		protected bool GetAllocateDocumentsOnHold()
		{
			var setupSelect = new PXSetup<INSetup>(Base);
			return setupSelect.Current.AllocateDocumentsOnHold != false;
		}

		public static void AddStatusDACsToCacheMapping(PXGraph graph)
		{
			var caches = graph.Caches;
			caches.AddCacheMapping(typeof(INSiteStatusByCostCenter), typeof(INSiteStatusByCostCenter));
			caches.AddCacheMapping(typeof(INLocationStatusByCostCenter), typeof(INLocationStatusByCostCenter));
			caches.AddCacheMapping(typeof(INLotSerialStatusByCostCenter), typeof(INLotSerialStatusByCostCenter));
			caches.AddCacheMapping(typeof(INItemLotSerial), typeof(INItemLotSerial));
			caches.AddCacheMapping(typeof(INSiteLotSerial), typeof(INSiteLotSerial));

			caches.AddCacheMapping(typeof(SiteStatusByCostCenter), typeof(SiteStatusByCostCenter));
			caches.AddCacheMapping(typeof(LocationStatusByCostCenter), typeof(LocationStatusByCostCenter));
			caches.AddCacheMapping(typeof(LotSerialStatusByCostCenter), typeof(LotSerialStatusByCostCenter));
			caches.AddCacheMapping(typeof(ItemLotSerial), typeof(ItemLotSerial));
			caches.AddCacheMapping(typeof(SiteLotSerial), typeof(SiteLotSerial));
		}
	}
}
