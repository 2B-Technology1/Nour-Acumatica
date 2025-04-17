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
using PX.Data;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.PO;
using System.Linq;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;


namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing Inventory helper class
    /// Shared method such as get lot size, default bin location, etc.
    /// </summary>
    public static class InventoryHelper
    {
        public static bool IsInvalidItemStatus(InventoryItem inventoryItem)
        {
            if (inventoryItem == null)
            {
                return false;
            }

            return IsInvalidItemStatus(inventoryItem.ItemStatus);
        }

        public static bool IsInvalidItemStatus(string itemStatus)
        {
            return itemStatus == INItemStatus.Inactive ||
                   itemStatus == INItemStatus.ToDelete;
        }

        public static decimal GetMfgReorderQty(PXGraph graph, int? inventoryID, int? siteId = null, decimal? qty = null)
        {
            if (inventoryID == null)
            {
                return 0;
            }

            decimal orderQty = qty ?? 1m;

            if (siteId != null)
            {
                INItemSite inItemSite = PXSelect
                    <INItemSite, Where<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>,
                        And<INItemSite.siteID, Equal<Required<INItemSite.siteID>>>>>.Select(graph, inventoryID,
                            siteId);

                if (inItemSite != null)
                {
                    INItemSiteExt inItemSiteExt =
                        PXCache<INItemSite>.GetExtension<INItemSiteExt>(inItemSite);

                    if (inItemSiteExt != null)
                    {
                        return ReorderQuantity(orderQty, inItemSiteExt.AMMinOrdQty, inItemSiteExt.AMMaxOrdQty, inItemSiteExt.AMLotSize);
                    }
                }
            }

            InventoryItem inventoryItem = PXSelect
                <InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                .Select(graph, inventoryID);

            if (inventoryItem == null)
            {
                return 0;
            }

            InventoryItemExt inventoryItemExt =
                PXCache<InventoryItem>.GetExtension<InventoryItemExt>(inventoryItem);

            if (inventoryItemExt != null)
            {
                return ReorderQuantity(orderQty, inventoryItemExt.AMMinOrdQty, inventoryItemExt.AMMaxOrdQty, inventoryItemExt.AMLotSize);
            }

            return orderQty;
        }

        public static decimal GetMfgReorderQty(PXGraph graph, InventoryItem inventoryItem, INItemSite inItemSite, decimal? qty = null)
        {
            var orderQty = qty ?? 1m;

            if (inItemSite?.SiteID != null)
            {
                var inItemSiteExt =
                        PXCache<INItemSite>.GetExtension<INItemSiteExt>(inItemSite);

                if (inItemSiteExt != null)
                {
                    return ReorderQuantity(orderQty, inItemSiteExt.AMMinOrdQty, inItemSiteExt.AMMaxOrdQty, inItemSiteExt.AMLotSize);
                }
            }

            if (inventoryItem == null)
            {
                return 0;
            }

            var inventoryItemExt =
                PXCache<InventoryItem>.GetExtension<InventoryItemExt>(inventoryItem);

            return inventoryItemExt != null ? ReorderQuantity(orderQty, inventoryItemExt.AMMinOrdQty, inventoryItemExt.AMMaxOrdQty, inventoryItemExt.AMLotSize) : orderQty;
        }

        public static decimal GetPurchaseOrderQtyByVendor(PXGraph graph, int? inventoryID, int? vendorID, decimal? qty)
        {
            if (inventoryID == null || qty == 0 || vendorID == null)
            {
                return 0;
            }

            POVendorInventory vendorInventory = PXSelect<POVendorInventory,
                Where<POVendorInventory.vendorID, Equal<Required<POVendorInventory.vendorID>>,
                    And<POVendorInventory.inventoryID, Equal<Required<POVendorInventory.inventoryID>>>
                >>.SelectWindowed(graph, 0, 1, vendorID, inventoryID);

            if(vendorInventory == null)
            {
                return qty.GetValueOrDefault();
            }

            // Assumes the returning value is the UOM related to the Vendor Inventory...
            return ReorderQuantity(qty.GetValueOrDefault(), vendorInventory.MinOrdQty.GetValueOrDefault(), vendorInventory.MaxOrdQty.GetValueOrDefault(),
                vendorInventory.LotSize.GetValueOrDefault());
        }

		public static int GetPurchaseLeadTime(PXGraph graph, int? inventoryID, int? siteID)
		{
			var result = (PXResult<INItemSite, POVendorInventory>)SelectFrom<INItemSite>
				.LeftJoin<POVendorInventory>
					.On<INItemSite.preferredVendorID.IsEqual<POVendorInventory.vendorID>
					.And<INItemSite.inventoryID.IsEqual<POVendorInventory.inventoryID>>>
				.Where<INItemSite.inventoryID.IsEqual<@P.AsInt>
					.And<INItemSite.siteID.IsEqual<@P.AsInt>>>
				.View.Select(graph, inventoryID, siteID);

			var prefVendor = result.GetItem<POVendorInventory>();
			var itemSite = result.GetItem<INItemSite>();

			if(prefVendor?.VendorID == null)
			{
				return itemSite?.SiteID == null ? 0 : (PXCache<INItemSite>.GetExtension<INItemSiteExt>(itemSite)?.AMMFGLeadTime ?? 0);
			}

			return prefVendor.VLeadTime.GetValueOrDefault() + prefVendor.AddLeadTimeDays.GetValueOrDefault();
		}

        /// <summary>
        /// Retrieves the fixed manufacturing lead time value for an item [and site]
        /// </summary>
        /// <param name="graph">calling graph</param>
        /// <param name="inventoryID">Inventory ID</param>
        /// <param name="siteID">Warehouse ID</param>
        /// <returns></returns>
        public static int GetFixMfgLeadTime(PXGraph graph, int? inventoryID, int? siteID = null)
        {
            if (graph == null)
            {
                throw new ArgumentNullException(nameof(graph));
            }

            if (inventoryID == null)
            {
                return 0;
            }

            if (siteID != null)
            {
                INItemSite inItemSite = PXSelectReadonly
                    <INItemSite, Where<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>,
                        And<INItemSite.siteID, Equal<Required<INItemSite.siteID>>>>>.Select(graph, inventoryID,
                            siteID);

                if (inItemSite != null)
                {
                    INItemSiteExt inItemSiteExt =
                        PXCache<INItemSite>.GetExtension<INItemSiteExt>(inItemSite);

                    if (inItemSiteExt != null && inItemSiteExt.AMMFGLeadTime.GetValueOrDefault() != 0)
                    {
                        return inItemSiteExt.AMMFGLeadTime.GetValueOrDefault();
                    }
                }
            }

            InventoryItem inventoryItem = PXSelectReadonly
                <InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                .Select(graph, inventoryID);

            if (inventoryItem == null)
            {
                return 0;
            }

            InventoryItemExt inventoryItemExt =
                PXCache<InventoryItem>.GetExtension<InventoryItemExt>(inventoryItem);

            return inventoryItemExt.AMMFGLeadTime.GetValueOrDefault();
        }

        public static PXResult<InventoryItem, INLotSerClass> GetItemLotSerClass(PXGraph graph, int? inventoryId)
        {
            if (graph == null || inventoryId.GetValueOrDefault() == 0)
            {
                return null;
            }

            return (PXResult<InventoryItem, INLotSerClass>)PXSelectJoin<InventoryItem,
                LeftJoin<INLotSerClass, On<INLotSerClass.lotSerClassID, Equal<InventoryItem.lotSerClassID>>>,
                Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.SelectWindowed(graph, 0, 1, inventoryId);
        }

        /// <summary>
        /// Get the qty avail for an item/subitem/bin combo. 
        /// </summary>
        /// <param name="graph">Calling PXGraph</param>
        /// <param name="inventoryId">Inventory Item ID</param>
        /// <param name="subItemId">Sub Item</param>
        /// <param name="locationID">Bin Location ID</param>
        /// <returns>Current quantity available</returns>
        public static decimal GetQtyAvail(PXGraph graph, int? inventoryId, int? subItemId, int? siteID, int? locationID)
        {
            return GetQtyAvail(graph, inventoryId, subItemId, siteID, locationID, null);
        }

        /// <summary>
        /// Get the qty avail for an item/subitem/bin combo. 
        /// </summary>
        /// <param name="graph">Calling PXGraph</param>
        /// <param name="inventoryId">Inventory Item ID</param>
        /// <param name="subItemId">Sub Item</param>
        /// <param name="locationID">Bin Location ID</param>
        /// <param name="lotSerialNbr">Lot/Serial Number (leave null/empty to skip check on lot/serial)</param>
        /// <returns>Current quantity available</returns>
        public static decimal GetQtyAvail(PXGraph graph, int? inventoryId, int? subItemId, int? siteID, int? locationID, string lotSerialNbr)
        {
            return GetQtyAvail(graph, inventoryId, subItemId, siteID, locationID, lotSerialNbr, null, null, true);
        }

        /// <summary>
        /// Get the qty avail for an item/subitem/bin combo. 
        /// </summary>
        /// <param name="graph">Calling PXGraph</param>
        /// <param name="inventoryId">Inventory Item ID</param>
        /// <param name="subItemId">Sub Item</param>
        /// <param name="locationID">Bin Location ID</param>
        /// <param name="lotSerialNbr">Lot/Serial Number (leave null/empty to skip check on lot/serial)</param>
        /// <param name="receiptsAllowed">find bin locations with receipts valid matching value (null = any value)</param>
        /// <param name="salesAllowed">find bin locations with sales valid matching value (null = any value)</param>
        /// <param name="productionAllowed">find bin locations with production valid matching value (null = any value)</param>
        /// <returns>Current quantity available</returns>
        public static decimal GetQtyAvail(PXGraph graph, int? inventoryId, int? subItemId, int? siteID, int? locationID, string lotSerialNbr,
            bool? receiptsAllowed, bool? salesAllowed, bool? productionAllowed)
        {
            if (graph == null || inventoryId.GetValueOrDefault() == 0)
            {
                return 0m;
            }

            if (string.IsNullOrWhiteSpace(lotSerialNbr))
            {
                var locactionStatus = GetLocationStatusSum(graph, inventoryId, subItemId, siteID, locationID, receiptsAllowed, salesAllowed, productionAllowed);
                return locactionStatus?.QtyOnHand ?? 0m;
            }

            var lotSerialStatus = GetLotSerialStatusSum(graph, inventoryId, subItemId, siteID, locationID, lotSerialNbr, receiptsAllowed, salesAllowed, productionAllowed);
            return lotSerialStatus?.QtyOnHand ?? 0m;
        }

        /// <summary>
        /// Get the qty hard avail for an item/subitem/bin combo. 
        /// </summary>
        /// <param name="graph">Calling PXGraph</param>
        /// <param name="inventoryId">Inventory Item ID</param>
        /// <param name="subItemId">Sub Item</param>
        /// <param name="locationID">Bin Location ID</param>
        /// <param name="lotSerialNbr">Lot/Serial Number (leave null/empty to skip check on lot/serial)</param>
        /// <param name="receiptsAllowed">find bin locations with receipts valid matching value (null = any value)</param>
        /// <param name="salesAllowed">find bin locations with sales valid matching value (null = any value)</param>
        /// <param name="productionAllowed">find bin locations with production valid matching value (null = any value)</param>
        /// <returns>Current quantity available</returns>
        public static decimal GetQtyHardAvail(PXGraph graph, int? inventoryId, int? subItemId, int? siteID, int? locationID, string lotSerialNbr,
            bool? receiptsAllowed, bool? salesAllowed, bool? productionAllowed)
        {
            if (graph == null || inventoryId.GetValueOrDefault() == 0)
            {
                return 0m;
            }

            var siteHardAvail = GetSiteStatusSum(graph, inventoryId, subItemId, siteID)?.QtyHardAvail;
            var hardAvail = 0m;

            if (string.IsNullOrWhiteSpace(lotSerialNbr))
            {
                var locactionStatus = GetLocationStatusSum(graph, inventoryId, subItemId, siteID, locationID, receiptsAllowed, salesAllowed, productionAllowed);
                hardAvail = locactionStatus?.QtyHardAvail ?? 0m;
            }
            else
            {
                var lotSerialStatus = GetLotSerialStatusSum(graph, inventoryId, subItemId, siteID, locationID, lotSerialNbr, receiptsAllowed, salesAllowed, productionAllowed);
                hardAvail = lotSerialStatus?.QtyHardAvail ?? 0m;
            }
            if (siteHardAvail != null)
            {
                return Math.Min(hardAvail, siteHardAvail.GetValueOrDefault());
            }
            return hardAvail;
        }

        /// <summary>
        /// Query INLocationStatusByCostCenter and return the results
        /// </summary>
        public static PXResultset<INLocationStatusByCostCenter> GetLocationStatus(PXGraph graph, int? inventoryID, int? subitemID, int? siteID, int? locationID)
        {
            return GetLocationStatus(graph, inventoryID, subitemID, siteID, locationID, false, false, true);
        }

        /// <summary>
        /// Query INLocationStatusByCostCenter and return the results based on shipping availabe
        /// </summary>
        public static PXResultset<INLocationStatusByCostCenter> GetLocationStatus(PXGraph graph, int? inventoryID, int? subitemID, int? siteID, int? locationID,
            bool? receiptsAllowed, bool? salesAllowed, bool? productionAllowed)
        {
            return GetLocationStatus<INLocationStatusByCostCenter.qtyHardAvail>(graph, inventoryID, subitemID, siteID, locationID, receiptsAllowed, salesAllowed, productionAllowed);
        }

        /// <summary>
        /// Query INLocationStatusByCostCenter and return the results based on quantity on hand
        /// </summary>
        public static PXResultset<INLocationStatusByCostCenter> GetLocationStatusProductionAllocatedQty(PXGraph graph, int? inventoryID, int? subitemID, int? siteID, int? locationID,
            bool? receiptsAllowed, bool? salesAllowed, bool? productionAllowed)
        {
            return GetLocationStatus<INLocationStatusByCostCenter.qtyOnHand>(graph, inventoryID, subitemID, siteID, locationID, receiptsAllowed, salesAllowed, productionAllowed);
        }

        private static PXResultset<INLocationStatusByCostCenter> GetLocationStatus<TQtyField>(PXGraph graph, int? inventoryID, int? subitemID, int? siteID, int? locationID,
            bool? receiptsAllowed, bool? salesAllowed, bool? productionAllowed)
                where TQtyField : IBqlField
        {
            if (inventoryID.GetValueOrDefault() == 0)
            {
                throw new PXArgumentException(nameof(inventoryID));
            }

            bool multiWarehouseEnabled = PXAccess.FeatureInstalled<FeaturesSet.warehouse>();
            if (siteID.GetValueOrDefault() == 0 && multiWarehouseEnabled)
            {
                throw new PXArgumentException(nameof(siteID));
            }

            PXSelectBase<INLocationStatusByCostCenter> cmd = new PXSelectJoin<INLocationStatusByCostCenter,
                InnerJoin<INLocation, On<INLocationStatusByCostCenter.locationID, Equal<INLocation.locationID>>>,
                    Where<INLocationStatusByCostCenter.inventoryID, Equal<Required<INLocationStatusByCostCenter.inventoryID>>,
                        And<TQtyField, Greater<decimal0>,
                        And<INLocation.active, Equal<True>>>>,
                    OrderBy<Asc<INLocation.pickPriority>>>(graph);

            var cmdParms = new List<object> { inventoryID };

            BuildLocationStatusCommand(ref cmd, ref cmdParms, subitemID, siteID, locationID,
                receiptsAllowed, salesAllowed, productionAllowed);

            return cmd.Select(cmdParms.ToArray());
        }

        /// <summary>
        /// Query INLocationStatusByCostCenter and return the sum of the results
        /// </summary>
        public static INLocationStatus GetLocationStatusSum(PXGraph graph, int? inventoryID, int? subitemID, int? siteID, int? locationID,
            bool? receiptsAllowed, bool? salesAllowed, bool? productionAllowed)
        {
            if (inventoryID.GetValueOrDefault() == 0)
            {
                throw new PXArgumentException(nameof(inventoryID));
            }

            bool multiWarehouseEnabled = PXAccess.FeatureInstalled<FeaturesSet.warehouse>();
            if (siteID.GetValueOrDefault() == 0 && multiWarehouseEnabled)
            {
                throw new PXArgumentException(nameof(siteID));
            }

            PXSelectBase<INLocationStatusByCostCenter> cmd = new PXSelectJoinGroupBy<INLocationStatusByCostCenter,
                InnerJoin<INLocation, On<INLocationStatusByCostCenter.locationID, Equal<INLocation.locationID>>>,
                    Where<INLocationStatusByCostCenter.inventoryID, Equal<Required<INLocationStatusByCostCenter.inventoryID>>>,
                    Aggregate<
                        Sum<INLocationStatusByCostCenter.qtyOnHand,
                        Sum<INLocationStatusByCostCenter.qtyAvail,
                        Sum<INLocationStatusByCostCenter.qtyNotAvail,
                        Sum<INLocationStatusByCostCenter.qtyExpired,
                        Sum<INLocationStatusByCostCenter.qtyHardAvail,
                        Sum<INLocationStatusByCostCenter.qtyActual,
                        Sum<INLocationStatusByCostCenter.qtyFSSrvOrdBooked,
                        Sum<INLocationStatusByCostCenter.qtyFSSrvOrdAllocated,
                        Sum<INLocationStatusByCostCenter.qtyFSSrvOrdPrepared,
                        Sum<INLocationStatusByCostCenter.qtySOBackOrdered,
                        Sum<INLocationStatusByCostCenter.qtySOPrepared,
                        Sum<INLocationStatusByCostCenter.qtySOBooked,
                        Sum<INLocationStatusByCostCenter.qtySOShipped,
                        Sum<INLocationStatusByCostCenter.qtySOShipping,
                        Sum<INLocationStatusByCostCenter.qtyINIssues,
                        Sum<INLocationStatusByCostCenter.qtyINReceipts,
                        Sum<INLocationStatusByCostCenter.qtyInTransit,
                        Sum<INLocationStatusByCostCenter.qtyInTransitToSO,
                        Sum<INLocationStatusByCostCenter.qtyPOReceipts,
                        Sum<INLocationStatusByCostCenter.qtyPOPrepared,
                        Sum<INLocationStatusByCostCenter.qtyPOOrders,
                        Sum<INLocationStatusByCostCenter.qtyFixedFSSrvOrd,
                        Sum<INLocationStatusByCostCenter.qtyPOFixedFSSrvOrd,
                        Sum<INLocationStatusByCostCenter.qtyPOFixedFSSrvOrdPrepared,
                        Sum<INLocationStatusByCostCenter.qtyPOFixedFSSrvOrdReceipts,
                        Sum<INLocationStatusByCostCenter.qtySOFixed,
                        Sum<INLocationStatusByCostCenter.qtyPOFixedOrders,
                        Sum<INLocationStatusByCostCenter.qtyPOFixedPrepared,
                        Sum<INLocationStatusByCostCenter.qtyPOFixedReceipts,
                        Sum<INLocationStatusByCostCenter.qtySODropShip,
                        Sum<INLocationStatusByCostCenter.qtyPODropShipOrders,
                        Sum<INLocationStatusByCostCenter.qtyPODropShipPrepared,
                        Sum<INLocationStatusByCostCenter.qtyPODropShipReceipts,
                        Sum<INLocationStatusByCostCenter.qtyINAssemblySupply,
                        Sum<INLocationStatusByCostCenter.qtyINAssemblyDemand,
                        Sum<INLocationStatusByCostCenter.qtyInTransitToProduction,
                        Sum<INLocationStatusByCostCenter.qtyProductionSupplyPrepared,
                        Sum<INLocationStatusByCostCenter.qtyProductionSupply,
                        Sum<INLocationStatusByCostCenter.qtyPOFixedProductionPrepared,
                        Sum<INLocationStatusByCostCenter.qtyPOFixedProductionOrders,
                        Sum<INLocationStatusByCostCenter.qtyProductionDemandPrepared,
                        Sum<INLocationStatusByCostCenter.qtyProductionDemand,
                        Sum<INLocationStatusByCostCenter.qtyProductionAllocated,
                        Sum<INLocationStatusByCostCenter.qtySOFixedProduction,
                        Sum<INLocationStatusByCostCenter.qtyProdFixedPurchase,
                        Sum<INLocationStatusByCostCenter.qtyProdFixedProduction,
                        Sum<INLocationStatusByCostCenter.qtyProdFixedProdOrdersPrepared,
                        Sum<INLocationStatusByCostCenter.qtyProdFixedProdOrders,
                        Sum<INLocationStatusByCostCenter.qtyProdFixedSalesOrdersPrepared,
                        Sum<INLocationStatusByCostCenter.qtyProdFixedSalesOrders
                        >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>(graph);

            var cmdParms = new List<object> { inventoryID };

            BuildLocationStatusCommand(ref cmd, ref cmdParms, subitemID, siteID, locationID,
                receiptsAllowed, salesAllowed, productionAllowed);

            var locationStatus = new INLocationStatus
            {
                InventoryID = inventoryID,
                SubItemID = subitemID,
                SiteID = siteID,
                LocationID = locationID,
                QtyOnHand = 0m,
                QtyAvail = 0m,
                QtyNotAvail = 0m,
                QtyExpired = 0m,
                QtyHardAvail = 0m,
                QtyActual = 0m,
                QtyFSSrvOrdBooked = 0m,
                QtyFSSrvOrdAllocated = 0m,
                QtyFSSrvOrdPrepared = 0m,
                QtySOBackOrdered = 0m,
                QtySOPrepared = 0m,
                QtySOBooked = 0m,
                QtySOShipped = 0m,
                QtySOShipping = 0m,
                QtyINIssues = 0m,
                QtyINReceipts = 0m,
                QtyInTransit = 0m,
                QtyInTransitToSO = 0m,
                QtyPOReceipts = 0m,
                QtyPOPrepared = 0m,
                QtyPOOrders = 0m,
                QtyFixedFSSrvOrd = 0m,
                QtyPOFixedFSSrvOrd = 0m,
                QtyPOFixedFSSrvOrdPrepared = 0m,
                QtyPOFixedFSSrvOrdReceipts = 0m,
                QtySOFixed = 0m,
                QtyPOFixedOrders = 0m,
                QtyPOFixedPrepared = 0m,
                QtyPOFixedReceipts = 0m,
                QtySODropShip = 0m,
                QtyPODropShipOrders = 0m,
                QtyPODropShipPrepared = 0m,
                QtyPODropShipReceipts = 0m,
                QtyINAssemblySupply = 0m,
                QtyINAssemblyDemand = 0m,
                QtyInTransitToProduction = 0m,
                QtyProductionSupplyPrepared = 0m,
                QtyProductionSupply = 0m,
                QtyPOFixedProductionPrepared = 0m,
                QtyPOFixedProductionOrders = 0m,
                QtyProductionDemandPrepared = 0m,
                QtyProductionDemand = 0m,
                QtyProductionAllocated = 0m,
                QtySOFixedProduction = 0m,
                QtyProdFixedPurchase = 0m,
                QtyProdFixedProduction = 0m,
                QtyProdFixedProdOrdersPrepared = 0m,
                QtyProdFixedProdOrders = 0m,
                QtyProdFixedSalesOrdersPrepared = 0m,
                QtyProdFixedSalesOrders = 0m
            };

            foreach (INLocationStatusByCostCenter result in cmd.Select(cmdParms.ToArray()))
            {
                locationStatus.QtyOnHand += result.QtyOnHand.GetValueOrDefault();
                locationStatus.QtyAvail += result.QtyAvail.GetValueOrDefault();
                locationStatus.QtyNotAvail += result.QtyNotAvail.GetValueOrDefault();
                locationStatus.QtyExpired += result.QtyExpired.GetValueOrDefault();
                locationStatus.QtyHardAvail += result.QtyHardAvail.GetValueOrDefault();
                locationStatus.QtyActual += result.QtyActual.GetValueOrDefault();
                locationStatus.QtyFSSrvOrdBooked += result.QtyFSSrvOrdBooked.GetValueOrDefault();
                locationStatus.QtyFSSrvOrdAllocated += result.QtyFSSrvOrdAllocated.GetValueOrDefault();
                locationStatus.QtyFSSrvOrdPrepared += result.QtyFSSrvOrdPrepared.GetValueOrDefault();
                locationStatus.QtySOBackOrdered += result.QtySOBackOrdered.GetValueOrDefault();
                locationStatus.QtySOPrepared += result.QtySOPrepared.GetValueOrDefault();
                locationStatus.QtySOBooked += result.QtySOBooked.GetValueOrDefault();
                locationStatus.QtySOShipped += result.QtySOShipped.GetValueOrDefault();
                locationStatus.QtySOShipping += result.QtySOShipping.GetValueOrDefault();
                locationStatus.QtyINIssues += result.QtyINIssues.GetValueOrDefault();
                locationStatus.QtyINReceipts += result.QtyINReceipts.GetValueOrDefault();
                locationStatus.QtyInTransit += result.QtyInTransit.GetValueOrDefault();
                locationStatus.QtyInTransitToSO += result.QtyInTransitToSO.GetValueOrDefault();
                locationStatus.QtyPOReceipts += result.QtyPOReceipts.GetValueOrDefault();
                locationStatus.QtyPOPrepared += result.QtyPOPrepared.GetValueOrDefault();
                locationStatus.QtyPOOrders += result.QtyPOOrders.GetValueOrDefault();
                locationStatus.QtyFixedFSSrvOrd += result.QtyFixedFSSrvOrd.GetValueOrDefault();
                locationStatus.QtyPOFixedFSSrvOrd += result.QtyPOFixedFSSrvOrd.GetValueOrDefault();
                locationStatus.QtyPOFixedFSSrvOrdPrepared += result.QtyPOFixedFSSrvOrdPrepared.GetValueOrDefault();
                locationStatus.QtyPOFixedFSSrvOrdReceipts += result.QtyPOFixedFSSrvOrdReceipts.GetValueOrDefault();
                locationStatus.QtySOFixed += result.QtySOFixed.GetValueOrDefault();
                locationStatus.QtyPOFixedOrders += result.QtyPOFixedOrders.GetValueOrDefault();
                locationStatus.QtyPOFixedPrepared += result.QtyPOFixedPrepared.GetValueOrDefault();
                locationStatus.QtyPOFixedReceipts += result.QtyPOFixedReceipts.GetValueOrDefault();
                locationStatus.QtySODropShip += result.QtySODropShip.GetValueOrDefault();
                locationStatus.QtyPODropShipOrders += result.QtyPODropShipOrders.GetValueOrDefault();
                locationStatus.QtyPODropShipPrepared += result.QtyPODropShipPrepared.GetValueOrDefault();
                locationStatus.QtyPODropShipReceipts += result.QtyPODropShipReceipts.GetValueOrDefault();
                locationStatus.QtyINAssemblySupply += result.QtyINAssemblySupply.GetValueOrDefault();
                locationStatus.QtyINAssemblyDemand += result.QtyINAssemblyDemand.GetValueOrDefault();
                locationStatus.QtyInTransitToProduction += result.QtyInTransitToProduction.GetValueOrDefault();
                locationStatus.QtyProductionSupplyPrepared += result.QtyProductionSupplyPrepared.GetValueOrDefault();
                locationStatus.QtyProductionSupply += result.QtyProductionSupply.GetValueOrDefault();
                locationStatus.QtyPOFixedProductionPrepared += result.QtyPOFixedProductionPrepared.GetValueOrDefault();
                locationStatus.QtyPOFixedProductionOrders += result.QtyPOFixedProductionOrders.GetValueOrDefault();
                locationStatus.QtyProductionDemandPrepared += result.QtyProductionDemandPrepared.GetValueOrDefault();
                locationStatus.QtyProductionDemand += result.QtyProductionDemand.GetValueOrDefault();
                locationStatus.QtyProductionAllocated += result.QtyProductionAllocated.GetValueOrDefault();
                locationStatus.QtySOFixedProduction += result.QtySOFixedProduction.GetValueOrDefault();
                locationStatus.QtyProdFixedPurchase += result.QtyProdFixedPurchase.GetValueOrDefault();
                locationStatus.QtyProdFixedProduction += result.QtyProdFixedProduction.GetValueOrDefault();
                locationStatus.QtyProdFixedProdOrdersPrepared += result.QtyProdFixedProdOrdersPrepared.GetValueOrDefault();
                locationStatus.QtyProdFixedProdOrders += result.QtyProdFixedProdOrders.GetValueOrDefault();
                locationStatus.QtyProdFixedSalesOrdersPrepared += result.QtyProdFixedSalesOrdersPrepared.GetValueOrDefault();
                locationStatus.QtyProdFixedSalesOrders += result.QtyProdFixedSalesOrders.GetValueOrDefault();
            }

            return locationStatus;
        }

        /// <summary>
        /// Builds the where statement for the base select for location status records
        /// </summary>
        private static void BuildLocationStatusCommand(ref PXSelectBase<INLocationStatusByCostCenter> cmd, ref List<object> cmdParms,
            int? subitemID, int? siteID, int? locationID,
            bool? receiptsAllowed, bool? salesAllowed, bool? productionAllowed)
        {
            if (siteID != null)
            {
                cmd.WhereAnd<Where<INLocationStatusByCostCenter.siteID, Equal<Required<INLocationStatusByCostCenter.siteID>>>>();
                cmdParms.Add(siteID);
            }

            if (subitemID.GetValueOrDefault() != 0 && PXAccess.FeatureInstalled<FeaturesSet.subItem>())
            {
                cmd.WhereAnd<Where<INLocationStatusByCostCenter.subItemID, Equal<Required<INLocationStatusByCostCenter.subItemID>>,
                    Or<INLocationStatusByCostCenter.subItemID, IsNull>>>();
                cmdParms.Add(subitemID);
            }

            if (locationID.GetValueOrDefault() != 0 && PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>())
            {
                cmd.WhereAnd<Where<INLocationStatusByCostCenter.locationID, Equal<Required<INLocationStatusByCostCenter.locationID>>>>();
                cmdParms.Add(locationID);
            }

            if (receiptsAllowed != null)
            {
                cmd.WhereAnd<Where<INLocation.receiptsValid, Equal<Required<INLocation.receiptsValid>>>>();
                cmdParms.Add(receiptsAllowed.GetValueOrDefault());
            }

            if (salesAllowed != null)
            {
                cmd.WhereAnd<Where<INLocation.salesValid, Equal<Required<INLocation.salesValid>>>>();
                cmdParms.Add(salesAllowed.GetValueOrDefault());
            }

            if (productionAllowed != null)
            {
                cmd.WhereAnd<Where<INLocation.productionValid, Equal<Required<INLocation.productionValid>>>>();
                cmdParms.Add(productionAllowed.GetValueOrDefault());
            }

			cmd.WhereAnd<Where<INLocationStatusByCostCenter.costCenterID, Equal<CostCenter.freeStock>>>();
        }

        public static INSiteStatus GetSiteStatusSum(PXGraph graph, int? inventoryID, int? subitemID, int? siteID)
        {
            if (inventoryID.GetValueOrDefault() == 0)
            {
                throw new PXArgumentException(nameof(inventoryID));
            }

            bool multiWarehouseEnabled = PXAccess.FeatureInstalled<FeaturesSet.warehouse>();
            if (siteID.GetValueOrDefault() == 0 && multiWarehouseEnabled)
            {
                throw new PXArgumentException(nameof(siteID));
            }

            PXSelectBase<INSiteStatusByCostCenter> cmd = new PXSelectGroupBy<INSiteStatusByCostCenter,
                Where<INSiteStatusByCostCenter.inventoryID, Equal<Required<INSiteStatusByCostCenter.inventoryID>>,
					And<INSiteStatusByCostCenter.costCenterID, Equal<CostCenter.freeStock>>>,
                Aggregate<
                    Sum<INSiteStatusByCostCenter.qtyOnHand,
                    Sum<INSiteStatusByCostCenter.qtyAvail,
                    Sum<INSiteStatusByCostCenter.qtyNotAvail,
                    Sum<INSiteStatusByCostCenter.qtyExpired,
                    Sum<INSiteStatusByCostCenter.qtyHardAvail,
                    Sum<INSiteStatusByCostCenter.qtyActual,
                    Sum<INSiteStatusByCostCenter.qtyFSSrvOrdBooked,
                    Sum<INSiteStatusByCostCenter.qtyFSSrvOrdAllocated,
                    Sum<INSiteStatusByCostCenter.qtyFSSrvOrdPrepared,
                    Sum<INSiteStatusByCostCenter.qtySOBackOrdered,
                    Sum<INSiteStatusByCostCenter.qtySOPrepared,
                    Sum<INSiteStatusByCostCenter.qtySOBooked,
                    Sum<INSiteStatusByCostCenter.qtySOShipped,
                    Sum<INSiteStatusByCostCenter.qtySOShipping,
                    Sum<INSiteStatusByCostCenter.qtyINIssues,
                    Sum<INSiteStatusByCostCenter.qtyINReceipts,
                    Sum<INSiteStatusByCostCenter.qtyInTransit,
                    Sum<INSiteStatusByCostCenter.qtyInTransitToSO,
                    Sum<INSiteStatusByCostCenter.qtyPOReceipts,
                    Sum<INSiteStatusByCostCenter.qtyPOPrepared,
                    Sum<INSiteStatusByCostCenter.qtyPOOrders,
                    Sum<INSiteStatusByCostCenter.qtyFixedFSSrvOrd,
                    Sum<INSiteStatusByCostCenter.qtyPOFixedFSSrvOrd,
                    Sum<INSiteStatusByCostCenter.qtyPOFixedFSSrvOrdPrepared,
                    Sum<INSiteStatusByCostCenter.qtyPOFixedFSSrvOrdReceipts,
                    Sum<INSiteStatusByCostCenter.qtySOFixed,
                    Sum<INSiteStatusByCostCenter.qtyPOFixedOrders,
                    Sum<INSiteStatusByCostCenter.qtyPOFixedPrepared,
                    Sum<INSiteStatusByCostCenter.qtyPOFixedReceipts,
                    Sum<INSiteStatusByCostCenter.qtySODropShip,
                    Sum<INSiteStatusByCostCenter.qtyPODropShipOrders,
                    Sum<INSiteStatusByCostCenter.qtyPODropShipPrepared,
                    Sum<INSiteStatusByCostCenter.qtyPODropShipReceipts,
                    Sum<INSiteStatusByCostCenter.qtyINAssemblySupply,
                    Sum<INSiteStatusByCostCenter.qtyINAssemblyDemand,
                    Sum<INSiteStatusByCostCenter.qtyInTransitToProduction,
                    Sum<INSiteStatusByCostCenter.qtyProductionSupplyPrepared,
                    Sum<INSiteStatusByCostCenter.qtyProductionSupply,
                    Sum<INSiteStatusByCostCenter.qtyPOFixedProductionPrepared,
                    Sum<INSiteStatusByCostCenter.qtyPOFixedProductionOrders,
                    Sum<INSiteStatusByCostCenter.qtyProductionDemandPrepared,
                    Sum<INSiteStatusByCostCenter.qtyProductionDemand,
                    Sum<INSiteStatusByCostCenter.qtyProductionAllocated,
                    Sum<INSiteStatusByCostCenter.qtySOFixedProduction,
                    Sum<INSiteStatusByCostCenter.qtyProdFixedPurchase,
                    Sum<INSiteStatusByCostCenter.qtyProdFixedProduction,
                    Sum<INSiteStatusByCostCenter.qtyProdFixedProdOrdersPrepared,
                    Sum<INSiteStatusByCostCenter.qtyProdFixedProdOrders,
                    Sum<INSiteStatusByCostCenter.qtyProdFixedSalesOrdersPrepared,
                    Sum<INSiteStatusByCostCenter.qtyProdFixedSalesOrders
                    >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>(graph);

            var cmdParms = new List<object> { inventoryID };

            if (siteID != null)
            {
                cmd.WhereAnd<Where<INSiteStatusByCostCenter.siteID, Equal<Required<INSiteStatusByCostCenter.siteID>>>>();
                cmdParms.Add(siteID);
            }

            if (subitemID.GetValueOrDefault() != 0 && PXAccess.FeatureInstalled<FeaturesSet.subItem>())
            {
                cmd.WhereAnd<Where<INSiteStatusByCostCenter.subItemID, Equal<Required<INSiteStatusByCostCenter.subItemID>>,
                    Or<INSiteStatusByCostCenter.subItemID, IsNull>>>();
                cmdParms.Add(subitemID);
            }

            var siteStatus = new INSiteStatus
            {
                InventoryID = inventoryID,
                SubItemID = subitemID,
                SiteID = siteID,
                QtyOnHand = 0m,
                QtyAvail = 0m,
                QtyNotAvail = 0m,
                QtyExpired = 0m,
                QtyHardAvail = 0m,
                QtyActual = 0m,
                QtyFSSrvOrdBooked = 0m,
                QtyFSSrvOrdAllocated = 0m,
                QtyFSSrvOrdPrepared = 0m,
                QtySOBackOrdered = 0m,
                QtySOPrepared = 0m,
                QtySOBooked = 0m,
                QtySOShipped = 0m,
                QtySOShipping = 0m,
                QtyINIssues = 0m,
                QtyINReceipts = 0m,
                QtyInTransit = 0m,
                QtyInTransitToSO = 0m,
                QtyPOReceipts = 0m,
                QtyPOPrepared = 0m,
                QtyPOOrders = 0m,
                QtyFixedFSSrvOrd = 0m,
                QtyPOFixedFSSrvOrd = 0m,
                QtyPOFixedFSSrvOrdPrepared = 0m,
                QtyPOFixedFSSrvOrdReceipts = 0m,
                QtySOFixed = 0m,
                QtyPOFixedOrders = 0m,
                QtyPOFixedPrepared = 0m,
                QtyPOFixedReceipts = 0m,
                QtySODropShip = 0m,
                QtyPODropShipOrders = 0m,
                QtyPODropShipPrepared = 0m,
                QtyPODropShipReceipts = 0m,
                QtyINAssemblySupply = 0m,
                QtyINAssemblyDemand = 0m,
                QtyInTransitToProduction = 0m,
                QtyProductionSupplyPrepared = 0m,
                QtyProductionSupply = 0m,
                QtyPOFixedProductionPrepared = 0m,
                QtyPOFixedProductionOrders = 0m,
                QtyProductionDemandPrepared = 0m,
                QtyProductionDemand = 0m,
                QtyProductionAllocated = 0m,
                QtySOFixedProduction = 0m,
                QtyProdFixedPurchase = 0m,
                QtyProdFixedProduction = 0m,
                QtyProdFixedProdOrdersPrepared = 0m,
                QtyProdFixedProdOrders = 0m,
                QtyProdFixedSalesOrdersPrepared = 0m,
                QtyProdFixedSalesOrders = 0m
            };

            foreach (INSiteStatusByCostCenter result in cmd.Select(cmdParms.ToArray()))
            {
                siteStatus.QtyOnHand += result.QtyOnHand.GetValueOrDefault();
                siteStatus.QtyAvail += result.QtyAvail.GetValueOrDefault();
                siteStatus.QtyNotAvail += result.QtyNotAvail.GetValueOrDefault();
                siteStatus.QtyExpired += result.QtyExpired.GetValueOrDefault();
                siteStatus.QtyHardAvail += result.QtyHardAvail.GetValueOrDefault();
                siteStatus.QtyActual += result.QtyActual.GetValueOrDefault();
                siteStatus.QtyFSSrvOrdBooked += result.QtyFSSrvOrdBooked.GetValueOrDefault();
                siteStatus.QtyFSSrvOrdAllocated += result.QtyFSSrvOrdAllocated.GetValueOrDefault();
                siteStatus.QtyFSSrvOrdPrepared += result.QtyFSSrvOrdPrepared.GetValueOrDefault();
                siteStatus.QtySOBackOrdered += result.QtySOBackOrdered.GetValueOrDefault();
                siteStatus.QtySOPrepared += result.QtySOPrepared.GetValueOrDefault();
                siteStatus.QtySOBooked += result.QtySOBooked.GetValueOrDefault();
                siteStatus.QtySOShipped += result.QtySOShipped.GetValueOrDefault();
                siteStatus.QtySOShipping += result.QtySOShipping.GetValueOrDefault();
                siteStatus.QtyINIssues += result.QtyINIssues.GetValueOrDefault();
                siteStatus.QtyINReceipts += result.QtyINReceipts.GetValueOrDefault();
                siteStatus.QtyInTransit += result.QtyInTransit.GetValueOrDefault();
                siteStatus.QtyInTransitToSO += result.QtyInTransitToSO.GetValueOrDefault();
                siteStatus.QtyPOReceipts += result.QtyPOReceipts.GetValueOrDefault();
                siteStatus.QtyPOPrepared += result.QtyPOPrepared.GetValueOrDefault();
                siteStatus.QtyPOOrders += result.QtyPOOrders.GetValueOrDefault();
                siteStatus.QtyFixedFSSrvOrd += result.QtyFixedFSSrvOrd.GetValueOrDefault();
                siteStatus.QtyPOFixedFSSrvOrd += result.QtyPOFixedFSSrvOrd.GetValueOrDefault();
                siteStatus.QtyPOFixedFSSrvOrdPrepared += result.QtyPOFixedFSSrvOrdPrepared.GetValueOrDefault();
                siteStatus.QtyPOFixedFSSrvOrdReceipts += result.QtyPOFixedFSSrvOrdReceipts.GetValueOrDefault();
                siteStatus.QtySOFixed += result.QtySOFixed.GetValueOrDefault();
                siteStatus.QtyPOFixedOrders += result.QtyPOFixedOrders.GetValueOrDefault();
                siteStatus.QtyPOFixedPrepared += result.QtyPOFixedPrepared.GetValueOrDefault();
                siteStatus.QtyPOFixedReceipts += result.QtyPOFixedReceipts.GetValueOrDefault();
                siteStatus.QtySODropShip += result.QtySODropShip.GetValueOrDefault();
                siteStatus.QtyPODropShipOrders += result.QtyPODropShipOrders.GetValueOrDefault();
                siteStatus.QtyPODropShipPrepared += result.QtyPODropShipPrepared.GetValueOrDefault();
                siteStatus.QtyPODropShipReceipts += result.QtyPODropShipReceipts.GetValueOrDefault();
                siteStatus.QtyINAssemblySupply += result.QtyINAssemblySupply.GetValueOrDefault();
                siteStatus.QtyINAssemblyDemand += result.QtyINAssemblyDemand.GetValueOrDefault();
                siteStatus.QtyInTransitToProduction += result.QtyInTransitToProduction.GetValueOrDefault();
                siteStatus.QtyProductionSupplyPrepared += result.QtyProductionSupplyPrepared.GetValueOrDefault();
                siteStatus.QtyProductionSupply += result.QtyProductionSupply.GetValueOrDefault();
                siteStatus.QtyPOFixedProductionPrepared += result.QtyPOFixedProductionPrepared.GetValueOrDefault();
                siteStatus.QtyPOFixedProductionOrders += result.QtyPOFixedProductionOrders.GetValueOrDefault();
                siteStatus.QtyProductionDemandPrepared += result.QtyProductionDemandPrepared.GetValueOrDefault();
                siteStatus.QtyProductionDemand += result.QtyProductionDemand.GetValueOrDefault();
                siteStatus.QtyProductionAllocated += result.QtyProductionAllocated.GetValueOrDefault();
                siteStatus.QtySOFixedProduction += result.QtySOFixedProduction.GetValueOrDefault();
                siteStatus.QtyProdFixedPurchase += result.QtyProdFixedPurchase.GetValueOrDefault();
                siteStatus.QtyProdFixedProduction += result.QtyProdFixedProduction.GetValueOrDefault();
                siteStatus.QtyProdFixedProdOrdersPrepared += result.QtyProdFixedProdOrdersPrepared.GetValueOrDefault();
                siteStatus.QtyProdFixedProdOrders += result.QtyProdFixedProdOrders.GetValueOrDefault();
                siteStatus.QtyProdFixedSalesOrdersPrepared += result.QtyProdFixedSalesOrdersPrepared.GetValueOrDefault();
                siteStatus.QtyProdFixedSalesOrders += result.QtyProdFixedSalesOrders.GetValueOrDefault();
            }

            return siteStatus;
        }

        /// <summary>
        /// Query INLotSerialStatusByCostCenter and return the results
        /// </summary>
        public static PXResultset<INLotSerialStatusByCostCenter> GetLotSerialStatus(PXGraph graph, int? inventoryID, int? subitemID, int? siteID, int? locationID, string lotSerialNbr)
        {
            return GetLotSerialStatus(graph, inventoryID, subitemID, siteID, locationID, lotSerialNbr, null, null, true);
        }

        /// <summary>
        /// Query INLotSerialStatusByCostCenter and return the results
        /// </summary>
        public static PXResultset<INLotSerialStatusByCostCenter> GetLotSerialStatus(PXGraph graph, int? inventoryID, int? subitemID, int? siteID, int? locationID, string lotSerialNbr,
            bool? receiptsAllowed, bool? salesAllowed, bool? productionAllowed)
        {
            var result = GetItemLotSerClass(graph, inventoryID);

            INLotSerClass lsClass = null;
            if (result != null)
            {
                lsClass = result;
            }

            return GetLotSerialStatus(graph, lsClass, inventoryID, subitemID, siteID, locationID, lotSerialNbr, receiptsAllowed, salesAllowed, productionAllowed);
        }

        /// <summary>
        /// Query INLotSerialStatusByCostCenter and return the results
        /// </summary>
        public static PXResultset<INLotSerialStatusByCostCenter> GetLotSerialStatus(PXGraph graph, INLotSerClass lotSerClass, int? inventoryID, int? subitemID, int? siteID, int? locationID)
        {
            return GetLotSerialStatus(graph, lotSerClass, inventoryID, subitemID, siteID, locationID, null);
        }

        /// <summary>
        /// Query INLotSerialStatus and return the results
        /// </summary>
        public static PXResultset<INLotSerialStatusByCostCenter> GetLotSerialStatus(PXGraph graph, INLotSerClass lotSerClass, int? inventoryID, int? subitemID, int? siteID, int? locationID, string lotSerialNbr)
        {
            return GetLotSerialStatus(graph, lotSerClass, inventoryID, subitemID, siteID, locationID, lotSerialNbr, null, null, true);
        }

        public static PXResultset<INLotSerialStatusByCostCenter> GetLotSerialStatusProductionAllocatedQty(PXGraph graph, INLotSerClass lotSerClass, int? inventoryID, int? subitemID, int? siteID, int? locationID, string lotSerialNbr,
            bool? receiptsAllowed, bool? salesAllowed, bool? productionAllowed)
        {
			return GetLotSerialStatus<INLotSerialStatusByCostCenter.qtyOnHand>(graph, lotSerClass, inventoryID, subitemID, siteID, locationID, lotSerialNbr, receiptsAllowed, salesAllowed, productionAllowed);
        }

        /// <summary>
        /// Query INLotSerialStatusByCostCenter and return the results
        /// </summary>
        public static PXResultset<INLotSerialStatusByCostCenter> GetLotSerialStatus(PXGraph graph, INLotSerClass lotSerClass, int? inventoryID, int? subitemID, int? siteID, int? locationID, string lotSerialNbr,
            bool? receiptsAllowed, bool? salesAllowed, bool? productionAllowed)
        {
            return GetLotSerialStatus<INLotSerialStatusByCostCenter.qtyHardAvail>(graph, lotSerClass, inventoryID, subitemID, siteID, locationID, lotSerialNbr, receiptsAllowed, salesAllowed, productionAllowed);
        }

        private static PXResultset<INLotSerialStatusByCostCenter> GetLotSerialStatus<TQtyField>(PXGraph graph, INLotSerClass lotSerClass, int? inventoryID, int? subitemID, int? siteID, int? locationID, string lotSerialNbr,
            bool? receiptsAllowed, bool? salesAllowed, bool? productionAllowed)
                where TQtyField : IBqlField
        {
            if (inventoryID.GetValueOrDefault() == 0)
            {
                throw new PXArgumentException(nameof(inventoryID));
            }

            if (lotSerClass == null
                || string.IsNullOrWhiteSpace(lotSerClass.LotSerClassID))
            {
                throw new PXArgumentException(nameof(lotSerClass));
            }

            if (lotSerClass.LotSerTrack == INLotSerTrack.NotNumbered)
            {
                return null;
            }

            var multiWarehouseEnabled = PXAccess.FeatureInstalled<FeaturesSet.warehouse>();
            if (siteID.GetValueOrDefault() == 0 && multiWarehouseEnabled)
            {
                throw new PXArgumentException(nameof(siteID));
            }

            PXSelectBase<INLotSerialStatusByCostCenter> cmd = new PXSelectJoin<INLotSerialStatusByCostCenter,
                InnerJoin<INLocation, On<INLotSerialStatusByCostCenter.locationID, Equal<INLocation.locationID>>,
                InnerJoin<INItemLotSerial, On<INLotSerialStatusByCostCenter.inventoryID, Equal<INItemLotSerial.inventoryID>,
                    And<INLotSerialStatusByCostCenter.lotSerialNbr, Equal<INItemLotSerial.lotSerialNbr>>>>>,
                    Where<INLotSerialStatusByCostCenter.inventoryID, Equal<Required<INLotSerialStatusByCostCenter.inventoryID>>,
                        And<TQtyField, Greater<decimal0>,
                        And<INLocation.active, Equal<True>>>>,
                    OrderBy<Asc<INLocation.pickPriority>>>(graph);

            var cmdParms = new List<object> { inventoryID };

            BuildLotSerialStatusCommand(ref cmd, ref cmdParms, lotSerClass, subitemID, siteID, locationID, lotSerialNbr,
                receiptsAllowed, salesAllowed, productionAllowed);

            return cmd.Select(cmdParms.ToArray());
        }

        /// <summary>
        /// Query INLotSerialStatus and return the results as a list
        /// </summary>
        public static List<PXResult<INLotSerialStatusByCostCenter, INLocation, INItemLotSerial>> GetLotSerialStatusList(PXGraph graph, INLotSerClass lotSerClass, int? inventoryID, int? subitemID, int? siteID, int? locationID, string lotSerialNbr,
            bool? receiptsAllowed, bool? salesAllowed, bool? productionAllowed)
        {
            return GetLotSerialStatus(graph, lotSerClass, inventoryID, subitemID, siteID, locationID, lotSerialNbr,
                    receiptsAllowed, salesAllowed, productionAllowed)
                ?.ToList<INLotSerialStatusByCostCenter, INLocation, INItemLotSerial>();
        }

        /// <summary>
        /// Query INLotSerialStatusByCostCenter and return the sum of the results
        /// </summary>
        public static INLotSerialStatus GetLotSerialStatusSum(PXGraph graph, int? inventoryID, int? subitemID, int? siteID, int? locationID, string lotSerialNbr,
            bool? receiptsAllowed, bool? salesAllowed, bool? productionAllowed)
        {
            var result = GetItemLotSerClass(graph, inventoryID);
            INLotSerClass lsClass = null;
            if (result != null)
            {
                lsClass = result;
            }

            return GetLotSerialStatusSum(graph, lsClass, inventoryID, subitemID, siteID, locationID, lotSerialNbr, receiptsAllowed, salesAllowed, productionAllowed);
        }

        /// <summary>
        /// Query INLotSerialStatusByCostCenter and return the sum of the results
        /// </summary>
        public static INLotSerialStatus GetLotSerialStatusSum(PXGraph graph, INLotSerClass lotSerClass, int? inventoryID, int? subitemID, int? siteID, int? locationID, string lotSerialNbr,
            bool? receiptsAllowed, bool? salesAllowed, bool? productionAllowed)
        {
            if (inventoryID.GetValueOrDefault() == 0)
            {
                throw new PXArgumentException(nameof(inventoryID));
            }

            if (lotSerClass == null
                || string.IsNullOrWhiteSpace(lotSerClass.LotSerClassID))
            {
                throw new PXArgumentException(nameof(lotSerClass));
            }

            if (lotSerClass.LotSerTrack == INLotSerTrack.NotNumbered)
            {
                return null;
            }

            bool multiWarehouseEnabled = PXAccess.FeatureInstalled<FeaturesSet.warehouse>();
            if (siteID.GetValueOrDefault() == 0 && multiWarehouseEnabled)
            {
                throw new PXArgumentException(nameof(siteID));
            }

            PXSelectBase<INLotSerialStatusByCostCenter> cmd = new PXSelectJoinGroupBy<INLotSerialStatusByCostCenter,
                    InnerJoin<INLocation, On<INLotSerialStatusByCostCenter.locationID, Equal<INLocation.locationID>>,
                    InnerJoin<INItemLotSerial, On<INLotSerialStatusByCostCenter.inventoryID, Equal<INItemLotSerial.inventoryID>, 
                        And<INLotSerialStatusByCostCenter.lotSerialNbr, Equal<INItemLotSerial.lotSerialNbr>>>>>,
                    Where<INLotSerialStatusByCostCenter.inventoryID, Equal<Required<INLotSerialStatusByCostCenter.inventoryID>>>,
                    Aggregate<
                        Sum<INLotSerialStatusByCostCenter.qtyOnHand,
                        Sum<INLotSerialStatusByCostCenter.qtyAvail,
                        Sum<INLotSerialStatusByCostCenter.qtyNotAvail,
                        Sum<INLotSerialStatusByCostCenter.qtyExpired,
                        Sum<INLotSerialStatusByCostCenter.qtyHardAvail,
                        Sum<INLotSerialStatusByCostCenter.qtyActual,
                        Sum<INLotSerialStatusByCostCenter.qtyFSSrvOrdBooked,
                        Sum<INLotSerialStatusByCostCenter.qtyFSSrvOrdAllocated,
                        Sum<INLotSerialStatusByCostCenter.qtyFSSrvOrdPrepared,
                        Sum<INLotSerialStatusByCostCenter.qtySOBackOrdered,
                        Sum<INLotSerialStatusByCostCenter.qtySOPrepared,
                        Sum<INLotSerialStatusByCostCenter.qtySOBooked,
                        Sum<INLotSerialStatusByCostCenter.qtySOShipped,
                        Sum<INLotSerialStatusByCostCenter.qtySOShipping,
                        Sum<INLotSerialStatusByCostCenter.qtyINIssues,
                        Sum<INLotSerialStatusByCostCenter.qtyINReceipts,
                        Sum<INLotSerialStatusByCostCenter.qtyInTransit,
                        Sum<INLotSerialStatusByCostCenter.qtyInTransitToSO,
                        Sum<INLotSerialStatusByCostCenter.qtyPOReceipts,
                        Sum<INLotSerialStatusByCostCenter.qtyPOPrepared,
                        Sum<INLotSerialStatusByCostCenter.qtyPOOrders,
                        Sum<INLotSerialStatusByCostCenter.qtyFixedFSSrvOrd,
                        Sum<INLotSerialStatusByCostCenter.qtyPOFixedFSSrvOrd,
                        Sum<INLotSerialStatusByCostCenter.qtyPOFixedFSSrvOrdPrepared,
                        Sum<INLotSerialStatusByCostCenter.qtyPOFixedFSSrvOrdReceipts,
                        Sum<INLotSerialStatusByCostCenter.qtySOFixed,
                        Sum<INLotSerialStatusByCostCenter.qtyPOFixedOrders,
                        Sum<INLotSerialStatusByCostCenter.qtyPOFixedPrepared,
                        Sum<INLotSerialStatusByCostCenter.qtyPOFixedReceipts,
                        Sum<INLotSerialStatusByCostCenter.qtySODropShip,
                        Sum<INLotSerialStatusByCostCenter.qtyPODropShipOrders,
                        Sum<INLotSerialStatusByCostCenter.qtyPODropShipPrepared,
                        Sum<INLotSerialStatusByCostCenter.qtyPODropShipReceipts,
                        Sum<INLotSerialStatusByCostCenter.qtyINAssemblySupply,
                        Sum<INLotSerialStatusByCostCenter.qtyINAssemblyDemand,
                        Sum<INLotSerialStatusByCostCenter.qtyInTransitToProduction,
                        Sum<INLotSerialStatusByCostCenter.qtyProductionSupplyPrepared,
                        Sum<INLotSerialStatusByCostCenter.qtyProductionSupply,
                        Sum<INLotSerialStatusByCostCenter.qtyPOFixedProductionPrepared,
                        Sum<INLotSerialStatusByCostCenter.qtyPOFixedProductionOrders,
                        Sum<INLotSerialStatusByCostCenter.qtyProductionDemandPrepared,
                        Sum<INLotSerialStatusByCostCenter.qtyProductionDemand,
                        Sum<INLotSerialStatusByCostCenter.qtyProductionAllocated,
                        Sum<INLotSerialStatusByCostCenter.qtySOFixedProduction,
                        Sum<INLotSerialStatusByCostCenter.qtyProdFixedPurchase,
                        Sum<INLotSerialStatusByCostCenter.qtyProdFixedProduction,
                        Sum<INLotSerialStatusByCostCenter.qtyProdFixedProdOrdersPrepared,
                        Sum<INLotSerialStatusByCostCenter.qtyProdFixedProdOrders,
                        Sum<INLotSerialStatusByCostCenter.qtyProdFixedSalesOrdersPrepared,
                        Sum<INLotSerialStatusByCostCenter.qtyProdFixedSalesOrders
                        >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>(graph);

            var cmdParms = new List<object> { inventoryID };

            BuildLotSerialStatusCommand(ref cmd, ref cmdParms, lotSerClass, subitemID, siteID, locationID, lotSerialNbr,
                receiptsAllowed, salesAllowed, productionAllowed);

            var lotSerialStatus = new INLotSerialStatus
            {
                InventoryID = inventoryID,
                SubItemID = subitemID,
                SiteID = siteID,
                LocationID = locationID,
                LotSerialNbr = lotSerialNbr,
                QtyOnHand = 0m,
                QtyAvail = 0m,
                QtyNotAvail = 0m,
                QtyExpired = 0m,
                QtyHardAvail = 0m,
                QtyActual = 0m,
                QtyFSSrvOrdBooked = 0m,
                QtyFSSrvOrdAllocated = 0m,
                QtyFSSrvOrdPrepared = 0m,
                QtySOBackOrdered = 0m,
                QtySOPrepared = 0m,
                QtySOBooked = 0m,
                QtySOShipped = 0m,
                QtySOShipping = 0m,
                QtyINIssues = 0m,
                QtyINReceipts = 0m,
                QtyInTransit = 0m,
                QtyInTransitToSO = 0m,
                QtyPOReceipts = 0m,
                QtyPOPrepared = 0m,
                QtyPOOrders = 0m,
                QtyFixedFSSrvOrd = 0m,
                QtyPOFixedFSSrvOrd = 0m,
                QtyPOFixedFSSrvOrdPrepared = 0m,
                QtyPOFixedFSSrvOrdReceipts = 0m,
                QtySOFixed = 0m,
                QtyPOFixedOrders = 0m,
                QtyPOFixedPrepared = 0m,
                QtyPOFixedReceipts = 0m,
                QtySODropShip = 0m,
                QtyPODropShipOrders = 0m,
                QtyPODropShipPrepared = 0m,
                QtyPODropShipReceipts = 0m,
                QtyINAssemblySupply = 0m,
                QtyINAssemblyDemand = 0m,
                QtyInTransitToProduction = 0m,
                QtyProductionSupplyPrepared = 0m,
                QtyProductionSupply = 0m,
                QtyPOFixedProductionPrepared = 0m,
                QtyPOFixedProductionOrders = 0m,
                QtyProductionDemandPrepared = 0m,
                QtyProductionDemand = 0m,
                QtyProductionAllocated = 0m,
                QtySOFixedProduction = 0m,
                QtyProdFixedPurchase = 0m,
                QtyProdFixedProduction = 0m,
                QtyProdFixedProdOrdersPrepared = 0m,
                QtyProdFixedProdOrders = 0m,
                QtyProdFixedSalesOrdersPrepared = 0m,
                QtyProdFixedSalesOrders = 0m
            };

            foreach (PXResult<INLotSerialStatusByCostCenter, INLocation, INItemLotSerial> result in cmd.Select(cmdParms.ToArray()))
            {
                // we need item lot serial because hard allocations from sales order does not update lotserialstatus (only itemlotserial if allocated to a lot/serial)
                var lotSerialStatusResult = (INLotSerialStatusByCostCenter) result;
                var itemLotSerialResult = (INItemLotSerial) result;

                lotSerialStatus.QtyOnHand += lotSerialStatusResult.QtyOnHand.GetValueOrDefault();
                lotSerialStatus.QtyAvail += lotSerialStatusResult.QtyAvail.GetValueOrDefault();
                lotSerialStatus.QtyNotAvail += lotSerialStatusResult.QtyNotAvail.GetValueOrDefault();
                lotSerialStatus.QtyExpired += lotSerialStatusResult.QtyExpired.GetValueOrDefault();
                lotSerialStatus.QtyHardAvail += Math.Min(lotSerialStatusResult.QtyHardAvail.GetValueOrDefault(), itemLotSerialResult.QtyHardAvail.GetValueOrDefault());
                lotSerialStatus.QtyActual += lotSerialStatusResult.QtyActual.GetValueOrDefault();
                lotSerialStatus.QtyFSSrvOrdBooked += lotSerialStatusResult.QtyFSSrvOrdBooked.GetValueOrDefault();
                lotSerialStatus.QtyFSSrvOrdAllocated += lotSerialStatusResult.QtyFSSrvOrdAllocated.GetValueOrDefault();
                lotSerialStatus.QtyFSSrvOrdPrepared += lotSerialStatusResult.QtyFSSrvOrdPrepared.GetValueOrDefault();
                lotSerialStatus.QtySOBackOrdered += lotSerialStatusResult.QtySOBackOrdered.GetValueOrDefault();
                lotSerialStatus.QtySOPrepared += lotSerialStatusResult.QtySOPrepared.GetValueOrDefault();
                lotSerialStatus.QtySOBooked += lotSerialStatusResult.QtySOBooked.GetValueOrDefault();
                lotSerialStatus.QtySOShipped += lotSerialStatusResult.QtySOShipped.GetValueOrDefault();
                lotSerialStatus.QtySOShipping += lotSerialStatusResult.QtySOShipping.GetValueOrDefault();
                lotSerialStatus.QtyINIssues += lotSerialStatusResult.QtyINIssues.GetValueOrDefault();
                lotSerialStatus.QtyINReceipts += lotSerialStatusResult.QtyINReceipts.GetValueOrDefault();
                lotSerialStatus.QtyInTransit += lotSerialStatusResult.QtyInTransit.GetValueOrDefault();
                lotSerialStatus.QtyInTransitToSO += lotSerialStatusResult.QtyInTransitToSO.GetValueOrDefault();
                lotSerialStatus.QtyPOReceipts += lotSerialStatusResult.QtyPOReceipts.GetValueOrDefault();
                lotSerialStatus.QtyPOPrepared += lotSerialStatusResult.QtyPOPrepared.GetValueOrDefault();
                lotSerialStatus.QtyPOOrders += lotSerialStatusResult.QtyPOOrders.GetValueOrDefault();
                lotSerialStatus.QtyFixedFSSrvOrd += lotSerialStatusResult.QtyFixedFSSrvOrd.GetValueOrDefault();
                lotSerialStatus.QtyPOFixedFSSrvOrd += lotSerialStatusResult.QtyPOFixedFSSrvOrd.GetValueOrDefault();
                lotSerialStatus.QtyPOFixedFSSrvOrdPrepared += lotSerialStatusResult.QtyPOFixedFSSrvOrdPrepared.GetValueOrDefault();
                lotSerialStatus.QtyPOFixedFSSrvOrdReceipts += lotSerialStatusResult.QtyPOFixedFSSrvOrdReceipts.GetValueOrDefault();
                lotSerialStatus.QtySOFixed += lotSerialStatusResult.QtySOFixed.GetValueOrDefault();
                lotSerialStatus.QtyPOFixedOrders += lotSerialStatusResult.QtyPOFixedOrders.GetValueOrDefault();
                lotSerialStatus.QtyPOFixedPrepared += lotSerialStatusResult.QtyPOFixedPrepared.GetValueOrDefault();
                lotSerialStatus.QtyPOFixedReceipts += lotSerialStatusResult.QtyPOFixedReceipts.GetValueOrDefault();
                lotSerialStatus.QtySODropShip += lotSerialStatusResult.QtySODropShip.GetValueOrDefault();
                lotSerialStatus.QtyPODropShipOrders += lotSerialStatusResult.QtyPODropShipOrders.GetValueOrDefault();
                lotSerialStatus.QtyPODropShipPrepared += lotSerialStatusResult.QtyPODropShipPrepared.GetValueOrDefault();
                lotSerialStatus.QtyPODropShipReceipts += lotSerialStatusResult.QtyPODropShipReceipts.GetValueOrDefault();
                lotSerialStatus.QtyINAssemblySupply += lotSerialStatusResult.QtyINAssemblySupply.GetValueOrDefault();
                lotSerialStatus.QtyINAssemblyDemand += lotSerialStatusResult.QtyINAssemblyDemand.GetValueOrDefault();
                lotSerialStatus.QtyInTransitToProduction += lotSerialStatusResult.QtyInTransitToProduction.GetValueOrDefault();
                lotSerialStatus.QtyProductionSupplyPrepared += lotSerialStatusResult.QtyProductionSupplyPrepared.GetValueOrDefault();
                lotSerialStatus.QtyProductionSupply += lotSerialStatusResult.QtyProductionSupply.GetValueOrDefault();
                lotSerialStatus.QtyPOFixedProductionPrepared += lotSerialStatusResult.QtyPOFixedProductionPrepared.GetValueOrDefault();
                lotSerialStatus.QtyPOFixedProductionOrders += lotSerialStatusResult.QtyPOFixedProductionOrders.GetValueOrDefault();
                lotSerialStatus.QtyProductionDemandPrepared += lotSerialStatusResult.QtyProductionDemandPrepared.GetValueOrDefault();
                lotSerialStatus.QtyProductionDemand += lotSerialStatusResult.QtyProductionDemand.GetValueOrDefault();
                lotSerialStatus.QtyProductionAllocated += lotSerialStatusResult.QtyProductionAllocated.GetValueOrDefault();
                lotSerialStatus.QtySOFixedProduction += lotSerialStatusResult.QtySOFixedProduction.GetValueOrDefault();
                lotSerialStatus.QtyProdFixedPurchase += lotSerialStatusResult.QtyProdFixedPurchase.GetValueOrDefault();
                lotSerialStatus.QtyProdFixedProduction += lotSerialStatusResult.QtyProdFixedProduction.GetValueOrDefault();
                lotSerialStatus.QtyProdFixedProdOrdersPrepared += lotSerialStatusResult.QtyProdFixedProdOrdersPrepared.GetValueOrDefault();
                lotSerialStatus.QtyProdFixedProdOrders += lotSerialStatusResult.QtyProdFixedProdOrders.GetValueOrDefault();
                lotSerialStatus.QtyProdFixedSalesOrdersPrepared += lotSerialStatusResult.QtyProdFixedSalesOrdersPrepared.GetValueOrDefault();
                lotSerialStatus.QtyProdFixedSalesOrders += lotSerialStatusResult.QtyProdFixedSalesOrders.GetValueOrDefault();
            }

            return lotSerialStatus;
        }

        /// <summary>
        /// Builds the where statement for the base select for lotserial status records
        /// </summary>
        private static void BuildLotSerialStatusCommand(ref PXSelectBase<INLotSerialStatusByCostCenter> cmd, ref List<object> cmdParms,
            INLotSerClass lotSerClass, int? subitemID, int? siteID, int? locationID, string lotSerialNbr,
            bool? receiptsAllowed, bool? salesAllowed, bool? productionAllowed)
        {
            if (siteID != null)
            {
                cmd.WhereAnd<Where<INLotSerialStatusByCostCenter.siteID, Equal<Required<INLotSerialStatusByCostCenter.siteID>>>>();
                cmdParms.Add(siteID);
            }

            if (subitemID.GetValueOrDefault() != 0 && PXAccess.FeatureInstalled<FeaturesSet.subItem>())
            {
                cmd.WhereAnd<Where<INLotSerialStatusByCostCenter.subItemID, Equal<Required<INLotSerialStatusByCostCenter.subItemID>>,
                    Or<INLotSerialStatusByCostCenter.subItemID, IsNull>>>();
                cmdParms.Add(subitemID);
            }

            if (locationID.GetValueOrDefault() != 0 && PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>())
            {
                cmd.WhereAnd<Where<INLotSerialStatusByCostCenter.locationID, Equal<Required<INLotSerialStatusByCostCenter.locationID>>>>();
                cmdParms.Add(locationID);
            }

            if (!string.IsNullOrWhiteSpace(lotSerialNbr))
            {
                cmd.WhereAnd<Where<INLotSerialStatusByCostCenter.lotSerialNbr, Equal<Required<INLotSerialStatusByCostCenter.lotSerialNbr>>>>();
                cmdParms.Add(lotSerialNbr);
            }

            if (receiptsAllowed != null)
            {
                cmd.WhereAnd<Where<INLocation.receiptsValid, Equal<Required<INLocation.receiptsValid>>>>();
                cmdParms.Add(receiptsAllowed.GetValueOrDefault());
            }

            if (salesAllowed != null)
            {
                cmd.WhereAnd<Where<INLocation.salesValid, Equal<Required<INLocation.salesValid>>>>();
                cmdParms.Add(salesAllowed.GetValueOrDefault());
            }

            if (productionAllowed != null)
            {
                cmd.WhereAnd<Where<INLocation.productionValid, Equal<Required<INLocation.productionValid>>>>();
                cmdParms.Add(productionAllowed.GetValueOrDefault());
            }

			cmd.WhereAnd<Where<INLotSerialStatusByCostCenter.costCenterID, Equal<CostCenter.freeStock>>>();

            switch (lotSerClass.LotSerIssueMethod)
            {
                case INLotSerIssueMethod.FIFO:
                    cmd.OrderByNew<OrderBy<Asc<INLocation.pickPriority, Asc<INLotSerialStatusByCostCenter.receiptDate, Asc<INLotSerialStatusByCostCenter.lotSerialNbr>>>>>();
                    break;
                case INLotSerIssueMethod.LIFO:
                    cmd.OrderByNew<OrderBy<Asc<INLocation.pickPriority, Desc<INLotSerialStatusByCostCenter.receiptDate, Asc<INLotSerialStatusByCostCenter.lotSerialNbr>>>>>();
                    break;
                case INLotSerIssueMethod.Expiration:
                    cmd.OrderByNew<OrderBy<Asc<INLotSerialStatusByCostCenter.expireDate, Asc<INLocation.pickPriority, Asc<INLotSerialStatusByCostCenter.lotSerialNbr>>>>>();
                    break;
                case INLotSerIssueMethod.Sequential:
                    cmd.OrderByNew<OrderBy<Asc<INLocation.pickPriority, Asc<INLotSerialStatusByCostCenter.lotSerialNbr>>>>();
                    break;
                case INLotSerIssueMethod.UserEnterable:
                default:
                    if (string.IsNullOrWhiteSpace(lotSerialNbr))
                    {
                        cmd.WhereAnd<Where<True, Equal<False>>>();
                    }
                    break;
            }
        }

        #region Default Bin Locations
        /// <summary>
        /// Class containing Manufacturing calls for getting an items default bin location information
        /// </summary>
        public static class DfltLocation
        {
            /// <summary>
            /// Bin Default Type
            /// </summary>
            public enum BinType
            {
                /// <summary>
                /// Put Away or Receipt Default Bin Location
                /// </summary>
                Receipt,   // 0
                /// <summary>
                /// Pick or Ship Default Bin Location
                /// </summary>
                Ship    // 1
            };

			internal static PXResultset<InventoryItemCurySettings> InventoryDefaultSelect<TDfltLocationField, locationAllowField>(
				PXGraph graph, int? inventoryID, bool checkProductionValid)
				where TDfltLocationField : IBqlField
				where locationAllowField : IBqlField
			{
				var cmd = new PXSelectJoin<InventoryItemCurySettings,
					InnerJoin<INLocation, On<TDfltLocationField, Equal<INLocation.locationID>>>,
					Where<InventoryItemCurySettings.inventoryID, Equal<Required<InventoryItemCurySettings.inventoryID>>,
						And<InventoryItemCurySettings.curyID, Equal<Required<InventoryItemCurySettings.curyID>>,
						And<INLocation.active, Equal<True>,
						And<locationAllowField, Equal<True>>>>>>(graph);

				if (checkProductionValid)
				{
					cmd.WhereAnd<Where<INLocation.productionValid, Equal<True>>>();
				}

				return cmd.Select(inventoryID, graph.Accessinfo.BaseCuryID);
			}

            /// <summary>
            /// Get the inventory item default location (no match to items default warehouse).
            /// </summary>
            /// <param name="graph">calling graph</param>
            /// <param name="binLocationType">bin location Type to retrieve</param>
            /// <param name="inventoryID">inventory item ID</param>
            /// <param name="checkProductionValid">Restrict the bin location with production allowed option checked</param>
            /// <returns>default bin location ID</returns>
            public static int? GetInventoryDefault(PXGraph graph, BinType binLocationType, int? inventoryID, bool checkProductionValid)
            {
                return GetInventoryDefault(graph, binLocationType, inventoryID, null, checkProductionValid);
            }

            /// <summary>
            /// Get the inventory item default location.
            /// </summary>
            /// <param name="graph">calling graph</param>
            /// <param name="binLocationType">bin location Type to retrieve</param>
            /// <param name="inventoryID">inventory item ID</param>
            /// <param name="siteID">(optional) warehouse ID. When entered - the default is returned only when item has a default warehouse entered</param>
            /// <param name="checkProductionValid">Restrict the bin location with production allowed option checked</param>
            /// <returns>default bin location ID</returns>
            public static int? GetInventoryDefault(PXGraph graph, BinType binLocationType, int? inventoryID, int? siteID, bool checkProductionValid)
            {
                if (graph == null || inventoryID == null)
                {
                    return null;
                }

				var item = binLocationType == BinType.Ship
					? (InventoryItemCurySettings)InventoryDefaultSelect<InventoryItemCurySettings.dfltShipLocationID, INLocation.salesValid>(graph, inventoryID, checkProductionValid)
					: (InventoryItemCurySettings)InventoryDefaultSelect<InventoryItemCurySettings.dfltReceiptLocationID, INLocation.receiptsValid>(graph, inventoryID, checkProductionValid);

                if (item != null && (siteID == null || item.DfltSiteID == siteID))
                {
                    return binLocationType == BinType.Ship
                        ? item.DfltShipLocationID
                        : item.DfltReceiptLocationID;
                }
                return null;
            }

			internal static PXResultset<INItemSite> ItemWarehouseDefaultSelect<TDfltLocationField, locationAllowField>(
				PXGraph graph, int? inventoryID, int? siteID, bool checkProductionValid)
				where TDfltLocationField : IBqlField
				where locationAllowField : IBqlField
			{
				var cmd = new PXSelectJoin<INItemSite,
					InnerJoin<INLocation, On<TDfltLocationField, Equal<INLocation.locationID>>>,
					Where<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>,
						And<INItemSite.siteID, Equal<Required<INItemSite.siteID>>,
						And<INLocation.active, Equal<True>,
						And<locationAllowField, Equal<True>>>>>>(graph);

				if (checkProductionValid)
				{
					cmd.WhereAnd<Where<INLocation.productionValid, Equal<True>>>();
				}

				return cmd.Select(inventoryID, siteID);
			}

            /// <summary>
            /// Get the item warehouse default location
            /// </summary>
            /// <param name="graph">calling graph</param>
            /// <param name="binLocationType">bin location Type to retrieve</param>
            /// <param name="inventoryID">inventory item ID</param>
            /// <param name="siteID">warehouse ID</param>
            /// <param name="checkProductionValid">Restrict the bin location with production allowed option checked</param>
            /// <returns>default bin location ID</returns>
            public static int? GetItemWarehouseDefault(PXGraph graph, BinType binLocationType, int? inventoryID, int? siteID, bool checkProductionValid)
            {
                if (graph == null || inventoryID == null || siteID == null)
                {
                    return null;
                }

                var itemsite = binLocationType == BinType.Ship
					? (INItemSite)ItemWarehouseDefaultSelect<INItemSite.dfltShipLocationID, INLocation.salesValid>(graph, inventoryID, siteID, checkProductionValid)
					: (INItemSite)ItemWarehouseDefaultSelect<INItemSite.dfltReceiptLocationID, INLocation.receiptsValid>(graph, inventoryID, siteID, checkProductionValid);

                if (itemsite != null)
                {
                    return binLocationType == BinType.Ship
                        ? itemsite.DfltShipLocationID
                        : itemsite.DfltReceiptLocationID;
                }
                return null;
            }

			internal static PXResultset<INSite> WarehouseDefaultSelect<TDfltLocationField, locationAllowField>(
				PXGraph graph, int? siteID, bool checkProductionValid)
				where TDfltLocationField : IBqlField
				where locationAllowField : IBqlField
			{
				var cmd = new PXSelectJoin<INSite,
					InnerJoin<INLocation, On<TDfltLocationField, Equal<INLocation.locationID>>>,
					Where<INSite.siteID, Equal<Required<INSite.siteID>>,
						And<INLocation.active, Equal<True>,
						And<locationAllowField, Equal<True>>>>>(graph);

				if (checkProductionValid)
				{
					cmd.WhereAnd<Where<INLocation.productionValid, Equal<True>>>();
				}

				return cmd.Select(siteID);
			}

            /// <summary>
            /// Get a warehouse's default location.
            /// </summary>
            /// <param name="graph">calling graph</param>
            /// <param name="binLocationType">bin location Type to retrieve</param>
            /// <param name="siteID">warehouse ID</param>
            /// <param name="checkProductionValid">Restrict the bin location with production allowed option checked</param>
            /// <returns>default bin location ID</returns>
            public static int? GetWarehouseDefault(PXGraph graph, BinType binLocationType, int? siteID, bool checkProductionValid)
            {
                if (graph == null || siteID == null)
                {
                    return null;
                }

				var warehouse = binLocationType == BinType.Ship
					? (INSite)WarehouseDefaultSelect<INSite.shipLocationID, INLocation.salesValid>(graph, siteID, checkProductionValid)
					: (INSite)WarehouseDefaultSelect<INSite.receiptLocationID, INLocation.receiptsValid>(graph, siteID, checkProductionValid);

                if (warehouse != null)
                {
                    return binLocationType == BinType.Receipt
                        ? warehouse.ReceiptLocationID
                        : warehouse.ShipLocationID;
                }
                return null;
            }

            /// <summary>
            /// Get the first location allowed for a given warehouse.
            /// </summary>
            /// <param name="graph">calling graph</param>
            /// <param name="binLocationType">bin location Type to retrieve</param>
            /// <param name="siteID">warehouse ID</param>
            /// <param name="checkProductionValid">Restrict the bin location with production allowed option checked</param>
            /// <returns>default bin location ID</returns>
            public static int? GetFirstWarehouseLocation(PXGraph graph, BinType binLocationType, int? siteID, bool checkProductionValid)
            {
                if (graph == null || siteID == null)
                {
                    return null;
                }

                PXSelectBase<INLocation> cmd = new PXSelect<INLocation,
                        Where<INLocation.siteID, Equal<Required<INLocation.siteID>>,
                            And<INLocation.active, Equal<True>>>,
                        OrderBy<Asc<INLocation.pickPriority>>>(graph);

                if (binLocationType == BinType.Ship)
                {
                    cmd.WhereAnd<Where<INLocation.salesValid, Equal<True>>>();
                }
                else
                {
                    cmd.WhereAnd<Where<INLocation.receiptsValid, Equal<True>>>();
                }

                if (checkProductionValid)
                {
                    cmd.WhereAnd<Where<INLocation.productionValid, Equal<True>>>();
                }

                var inLocation = (INLocation)cmd.SelectWindowed(0, 1, siteID);

                return inLocation?.LocationID;
            }

            /// <summary>
            /// Get a set of default locations for an item and warehouse.
            /// Defaults searched: item warehouse default, item default (matching default warehouse), warehouse default
            /// </summary>
            /// <param name="graph">calling graph</param>
            /// <param name="binLocationType">bin location Type to retrieve</param>
            /// <param name="inventoryID">inventory item ID</param>
            /// <param name="siteID">warehouse ID</param>
            /// <param name="checkProductionValid">Restrict the bin locations with production allowed option checked</param>
            /// <returns>a set of unique bin location ids</returns>
            public static List<int> GetDefaults(PXGraph graph, BinType binLocationType, int? inventoryID, int? siteID, bool checkProductionValid)
            {
                return GetDefaults(graph, binLocationType, inventoryID, siteID, false, false, checkProductionValid);
            }

            /// <summary>
            /// Get a set of default locations for an item and warehouse.
            /// Defaults searched: item warehouse default, item default (matching default warehouse), warehouse default
            /// </summary>
            /// <param name="graph">calling graph</param>
            /// <param name="binLocationType">bin location Type to retrieve</param>
            /// <param name="inventoryID">inventory item ID</param>
            /// <param name="siteID">warehouse ID</param>
            /// <param name="returnAnyLocation">when true, find at least 1 location regardless of found default locations</param>
            /// <param name="returnFirst">when true, return the first found location (only 1 location needed)</param>
            /// <param name="checkProductionValid">Restrict the bin locations with production allowed option checked</param>
            /// <returns>a set of unique bin location ids</returns>
            public static List<int> GetDefaults(PXGraph graph, BinType binLocationType, int? inventoryID, int? siteID, bool returnAnyLocation, bool returnFirst, bool checkProductionValid)
            {
                var defaults = new List<int>();
                var hashDefaults = new HashSet<int>();

                try
                {
                    if (siteID == null || inventoryID == null)
                    {
                        return defaults;
                    }

                    var itemWarehouseDefault = GetItemWarehouseDefault(graph, binLocationType, inventoryID, siteID, checkProductionValid);
                    if (itemWarehouseDefault.GetValueOrDefault() != 0
                            && hashDefaults.Add(itemWarehouseDefault.GetValueOrDefault()))
                    {
                        defaults.Add(itemWarehouseDefault.GetValueOrDefault());
                        if (returnFirst)
                        {
                            return defaults;
                        }
                    }

                    var itemDefault = GetInventoryDefault(graph, binLocationType, inventoryID, siteID, checkProductionValid);
                    if (itemDefault.GetValueOrDefault() != 0
                            && hashDefaults.Add(itemDefault.GetValueOrDefault()))
                    {
                        defaults.Add(itemDefault.GetValueOrDefault());
                        if (returnFirst)
                        {
                            return defaults;
                        }
                    }

                    var warehouseDefault = GetWarehouseDefault(graph, binLocationType, siteID, checkProductionValid);
                    if (warehouseDefault.GetValueOrDefault() != 0
                            && hashDefaults.Add(warehouseDefault.GetValueOrDefault()))
                    {
                        defaults.Add(warehouseDefault.GetValueOrDefault());
                        if (returnFirst)
                        {
                            return defaults;
                        }
                    }

                    if (returnAnyLocation)
                    {
                        var firstLoc = GetFirstWarehouseLocation(graph, binLocationType, siteID, checkProductionValid);
                        if (firstLoc.GetValueOrDefault() != 0
                            && hashDefaults.Add(firstLoc.GetValueOrDefault()))
                        {
                            defaults.Add(firstLoc.GetValueOrDefault());
                            if (returnFirst)
                            {
                                return defaults;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    PXTraceHelper.PxTraceException(e);
                }

                return defaults;
            }

            /// <summary>
            /// Find an items first default location.
            /// Checks [1] item warehouse [2] inventory item (matching default site) [3] warehouse default
            /// </summary>
            /// <param name="graph">calling graph</param>
            /// <param name="binLocationType">Bin type - Ship or Receipt</param>
            /// <param name="inventoryID">Inventory ID</param>
            /// <param name="siteID">Warehouse ID</param>
            /// <param name="returnAnyLocation">True: A bin location must be returned. First valid bin found is used after all defaults are checked. This helps to prevent a null return providing some type of overall default.
            /// False: Only defaults or null are returned.</param>
            /// <returns>A location ID</returns>
            public static int? GetDefault(PXGraph graph, BinType binLocationType, int? inventoryID, int? siteID, bool returnAnyLocation)
            {
                return GetDefault(graph, binLocationType, inventoryID, siteID, returnAnyLocation, false);
            }

            /// <summary>
            /// Find an items first default location.
            /// Checks [1] item warehouse [2] inventory item (matching default site) [3] warehouse default
            /// </summary>
            /// <param name="graph">calling graph</param>
            /// <param name="binLocationType">Bin type - Ship or Receipt</param>
            /// <param name="inventoryID">Inventory ID</param>
            /// <param name="siteID">Warehouse ID</param>
            /// <param name="returnAnyLocation">True: A bin location must be returned. First valid bin found is used after all defaults are checked. This helps to prevent a null return providing some type of overall default.
            /// False: Only defaults or null are returned.</param>
            /// <param name="checkProductionValid">Check locations only marked as production allowed</param>
            /// <returns>A location ID</returns>
            public static int? GetDefault(PXGraph graph, BinType binLocationType, int? inventoryID, int? siteID, bool returnAnyLocation, bool checkProductionValid)
            {
                var defaults = GetDefaults(graph, binLocationType, inventoryID, siteID, returnAnyLocation, true, checkProductionValid);

                if (defaults == null || defaults.Count == 0)
                {
                    return null;
                }

                return defaults[0];
            }
        }
        #endregion

        /// <summary>
        /// Determines the replenishment source for a specific inventory item using the InventoryID and SiteID
        /// </summary>
        public static string GetReplenishmentSource(PXGraph graph, int? inventoryID, int? siteID)
        {
            //Find the replenishment source int the INItemSite Table
            INItemSite inItemSite = PXSelect<INItemSite, Where<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>,
                    And<INItemSite.siteID, Equal<Required<INItemSite.siteID>>>>>.SelectWindowed(graph, 0, 1, inventoryID, siteID);

            if (inItemSite != null && !string.IsNullOrWhiteSpace(inItemSite.ReplenishmentSource))
            {
                return inItemSite.ReplenishmentSource;
            }

            InventoryItem item = PXSelect<InventoryItem,
                Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(graph, inventoryID);

            var itemExtension = item.GetExtension<InventoryItemExt>();
            if (itemExtension != null && !string.IsNullOrWhiteSpace(item.ReplenishmentSource))
            {
                return item.ReplenishmentSource;
            }

            return INReplenishmentSource.None;
        }

        public static decimal ReorderQuantity(decimal orderQty, decimal? minQty, decimal? maxQty, decimal? lotSize)
        {
            decimal reorderQty = orderQty;

            if (reorderQty < 0)
            {
                return reorderQty;
            }

            decimal minQtyDecimal = (minQty ?? 0) <= 0 ? 0 : minQty ?? 0;
            decimal maxQtyDecimal = (maxQty ?? 0) <= 0 ? 0 : maxQty ?? 0;
            decimal lotSizeDecimal = (lotSize ?? 0) <= 0 ? 0 : lotSize ?? 0;

            if (reorderQty < minQtyDecimal && minQtyDecimal > 0)
            {
                // APPLY MIN First
                reorderQty = minQtyDecimal;
            }

            // APPLY LOT SIZE Second
            reorderQty = lotSizeDecimal <= 0 ? reorderQty : Math.Ceiling(reorderQty / lotSizeDecimal) * lotSizeDecimal;

            decimal maxByLotQtyDecimal = lotSizeDecimal <= 0 ? maxQtyDecimal : Math.Floor(maxQtyDecimal / lotSizeDecimal) * lotSizeDecimal;

            if (reorderQty > maxByLotQtyDecimal && maxByLotQtyDecimal > 0)
            {
                // APPLY MAX Last
                reorderQty = maxByLotQtyDecimal;
            }

            return UomHelper.QuantityRound(reorderQty);
        }

        public static bool MakeItemSiteByItem(PXGraph graph, int? inventoryID, int? siteID, out INItemSite inItemSite)
        {
            if (graph == null || inventoryID == null)
            {
                inItemSite = null;
                return false;
            }

            InventoryItem inventoryItem =
                PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                    .Select(graph, inventoryID);

            return MakeItemSiteByItem(graph, inventoryItem, siteID, out inItemSite);
        }

        public static bool MakeItemSiteByItem(PXGraph graph, InventoryItem inventoryItem, int? siteID, out INItemSite inItemSite)
        {
            if (graph == null
                || inventoryItem == null
                || !inventoryItem.StkItem.GetValueOrDefault()
                || inventoryItem.InventoryID == null
                || siteID == null)
            {
                inItemSite = null;
                return false;
            }

            INSite site = PXSelectReadonly<INSite, Where<INSite.siteID, Equal<Required<INSite.siteID>>>>.Select(graph, siteID);

            if (site == null)
            {
                inItemSite = null;
                return false;
            }

            inItemSite = PXSelectReadonly<INItemSite,
                Where<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>,
                    And<INItemSite.siteID, Equal<Required<INItemSite.siteID>>>>>.Select(graph, inventoryItem.InventoryID, site.SiteID);

            if (inItemSite == null)
            {
                INPostClass postclass =
                    PXSelect<INPostClass, Where<INPostClass.postClassID, Equal<Required<INPostClass.postClassID>>>>
                        .Select(graph, inventoryItem.PostClassID);

                if (postclass == null)
                {
                    return false;
                }

                inItemSite = new INItemSite
                {
                    InventoryID = inventoryItem.InventoryID,
                    SiteID = site.SiteID
                };

                var itemCurySettings = InventoryItemCurySettings.PK.Find(graph, inventoryItem.InventoryID, site.BaseCuryID);
                INItemSiteMaint.DefaultItemSiteByItem(graph, inItemSite, inventoryItem, site, postclass, itemCurySettings);

                DefaultItemSiteManufacturing(graph, inventoryItem, inItemSite);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Default Manufacturing values from InventoryItem to INItemSite
        /// </summary>
        public static bool DefaultItemSiteManufacturing(PXGraph graph, InventoryItem inventoryItem, INItemSite inItemSite)
        {
            if (graph == null || inventoryItem == null || inItemSite == null)
            {
                return false;
            }

            InventoryItemExt inventoryItemExt = PXCache<InventoryItem>.GetExtension<InventoryItemExt>(inventoryItem);
            if (inventoryItemExt == null)
            {
                return false;
            }

            INItemSiteExt inItemSiteExt = PXCache<INItemSite>.GetExtension<INItemSiteExt>(inItemSite);
            if (inItemSiteExt == null)
            {
                return false;
            }

            //ready
            inItemSiteExt.MapAMExtensionFields(inventoryItemExt);
			inItemSite.ReplenishmentSource = inventoryItem.ReplenishmentSource;
			inItemSiteExt.AMReplenishmentSource = inventoryItem.ReplenishmentSource;
			inItemSite.ReplenishmentSourceSiteID = inventoryItemExt.AMSourceSiteID;
			inItemSiteExt.AMSourceSiteID = inventoryItemExt.AMSourceSiteID;
			inItemSite.PlanningMethod = inventoryItem.PlanningMethod;
            
            return true;
        }

        public static string GetbaseUOM(PXGraph graph, int? inventoryid)
        {
            InventoryItem inventoryItem = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                .Select(graph, inventoryid);
            if (inventoryItem != null)
            {
                return inventoryItem.BaseUnit;
            }

            return String.Empty;

        }

        /// <summary>
        /// Get the INSubItem row for a given sub item segment CD value
        /// </summary>
        /// <param name="graph">calling graph</param>
        /// <param name="subitemCD">Sub item CD Segment value</param>
        /// <param name="createIfNotFound">Should the call create an INSubItem record on the fly if not found</param>
        /// <returns>Found (or Created) INSubItem row</returns>
        public static INSubItem GetSubItem(PXGraph graph, string subitemCD, bool createIfNotFound = false)
        {
            if (graph == null || string.IsNullOrWhiteSpace(subitemCD))
            {
                return null;
            }

            INSubItem inSubItem = PXSelect<INSubItem,
                Where<INSubItem.subItemCD, Equal<Required<INSubItem.subItemCD>>>>.Select(graph, subitemCD);

            if (inSubItem != null)
            {
                return inSubItem;
            }

            if (createIfNotFound && TryCreateSubItem(graph, subitemCD, out inSubItem))
            {
                return inSubItem;
            }

            return null;
        }

        /// <summary>
        /// Indicates if the Lot Serial Tracking feature is enabled
        /// </summary>
        public static bool LotSerialTrackingFeatureEnabled => PXAccess.FeatureInstalled<FeaturesSet.lotSerialTracking>();

        /// <summary>
        /// Indicates if the Multiple Warehouse feature is enabled
        /// </summary>
        public static bool MultiWarehousesFeatureEnabled => PXAccess.FeatureInstalled<FeaturesSet.warehouse>();

        /// <summary>
        /// Indicates if the Multiple Warehouse Locations feature is enabled
        /// </summary>
        public static bool MultiWarehouseLocationFeatureEnabled => PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>();

        /// <summary>
        /// Indicates if the sub item feature is turned on (return = True)
        /// </summary>
        public static bool SubItemFeatureEnabled => PXAccess.FeatureInstalled<FeaturesSet.subItem>();

        /// <summary>
        /// MYOB - "Basic Inventory Replenishments" feature.
        /// Indicates is this feature is enabled/turned on
        /// </summary>
        public static bool BasicReplenishmentsEnabled => OEMHelper.FeatureInstalled(OEMHelper.MYOBFeatures.BasicInvReplenish);

        /// <summary>
        /// Indicates if the full replenishment feature is enabled/turned on
        /// </summary>
        public static bool FullReplenishmentsEnabled => PXAccess.FeatureInstalled<FeaturesSet.replenishment>();

        /// <summary>
        /// Try to create a sub item record if one does not exist for the entered sub item CD value
        /// </summary>
        /// <param name="subitemCD">Sub item CD Segment value</param>
        /// <param name="inSubItem">if created the new INSubItem row</param>
        /// <returns>True if a row was created, otherwise false</returns>
        private static bool TryCreateSubItem(PXGraph graph, string subitemCD, out INSubItem inSubItem)
        {
            inSubItem = null;

            if (!SubItemFeatureEnabled || string.IsNullOrWhiteSpace(subitemCD))
            {
                return false;
            }

            try
            {
                graph.Views.Caches.Add(typeof(INSubItem));

                inSubItem = PXSelect<INSubItem,
                    Where<INSubItem.subItemCD, Equal<Required<INSubItem.subItemCD>>>>.Select(graph, subitemCD);

                if (inSubItem != null)
                {
                    return false;
                }

                inSubItem = new INSubItem() { SubItemCD = subitemCD };

                graph.Caches[typeof(INSubItem)].Insert(inSubItem);

                graph.Persist();

                inSubItem = PXSelect<INSubItem,
                    Where<INSubItem.subItemCD, Equal<Required<INSubItem.subItemCD>>>>.Select(graph, subitemCD);

                if (inSubItem != null)
                {
                    PXTrace.WriteInformation(Messages.GetLocal(Messages.CreatedNewINSubItemRecord), subitemCD);
                    return true;
                }
            }
            catch (PXSetPropertyException)
            {
                //typically a bad subitem entered segment so lets toss that one up the chain
                throw;
            }
            catch (Exception e)
            {
                PXTraceHelper.PxTraceException(e);
            }

            return false;
        }

        public static string GetINDocTypeDescription(string inDocType)
        {
            switch (inDocType.TrimIfNotNullEmpty())
            {
                case INDocType.Adjustment:
                    return PX.Objects.IN.Messages.Adjustment;
                case INDocType.Issue:
                    return PX.Objects.IN.Messages.Issue;
                case INDocType.Receipt:
                    return PX.Objects.IN.Messages.Receipt;
                case INDocType.Transfer:
                    return PX.Objects.IN.Messages.Transfer;
                default:
                    return PX.Objects.IN.Messages.Undefined;

            }
        }

        public static string GetPurchaseOrderUnit(PXGraph graph, int? inventoryID, int? vendorID, string uom)
        {
            if (inventoryID == null)
            {
                return string.Empty;
            }

            //PO Unit by vendor
            if ((vendorID ?? 0) != 0)
            {
                POVendorInventory vendorInventory = PXSelect<POVendorInventory,
                    Where<POVendorInventory.vendorID, Equal<Required<POVendorInventory.vendorID>>,
                        And<POVendorInventory.inventoryID, Equal<Required<POVendorInventory.inventoryID>>>
                    >>.Select(graph, vendorID, inventoryID);

                if (vendorInventory != null)
                {
                    if (!string.IsNullOrWhiteSpace(vendorInventory.PurchaseUnit))
                    {
                        return vendorInventory.PurchaseUnit;
                    }
                }
            }

            //PO Unit by item (not vendor specific)
            InventoryItem inventoryItem = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                .Select(graph, inventoryID);

            if (inventoryItem != null)
            {
                if (!string.IsNullOrWhiteSpace(inventoryItem.PurchaseUnit))
                {
                    return inventoryItem.PurchaseUnit;
                }
            }

            return uom;
        }

        public static decimal ConvertToBaseUnits(PXGraph graph, POVendorInventory poVendorInventory, decimal qty)
        {
            if (UomHelper.TryConvertToBaseQty<POVendorInventory.inventoryID>(graph.Caches[typeof(POVendorInventory)],
                poVendorInventory, poVendorInventory.PurchaseUnit, qty, out var convertedUnits))
            {
                return convertedUnits.GetValueOrDefault();
            }

            return qty;
        }

		public static decimal ConvertToBaseUnits(PXGraph graph, POVendorInventory poVendorInventory, string baseUnit, decimal qty)
		{
			if(qty == 0 || poVendorInventory?.PurchaseUnit == null || string.IsNullOrWhiteSpace(baseUnit) || poVendorInventory.PurchaseUnit == baseUnit)
			{
				return qty;
			}

			if (UomHelper.TryConvertFromToQty<POVendorInventory.inventoryID>(graph.Caches[typeof(POVendorInventory)],
				poVendorInventory, poVendorInventory.PurchaseUnit, baseUnit, qty, out var convertedUnits))
			{
				return convertedUnits.GetValueOrDefault();
			}

			return qty;
		}

        public static InventoryAllocDetEnq GetInventoryAllocDetEnq<InventoryIDField, SiteIDField, SubItemIDField>(PXCache cache, object currentRow)
            where InventoryIDField : IBqlField
            where SiteIDField : IBqlField
            where SubItemIDField : IBqlField
        {
            var allocGraph = PXGraph.CreateInstance<InventoryAllocDetEnq>();
            if (currentRow == null)
            {
                return null;
            }

            allocGraph.Filter.Current.InventoryID = (int?)cache.GetValue<InventoryIDField>(currentRow);
            if (allocGraph.Filter.Current.InventoryID == null)
            {
                return null;
            }

            if (InventoryHelper.SubItemFeatureEnabled)
            {
                var subItem = (INSubItem)PXSelectorAttribute.Select<SubItemIDField>(cache, currentRow);
                if (!string.IsNullOrWhiteSpace(subItem?.SubItemCD))
                {
                    allocGraph.Filter.Current.SubItemCD = subItem.SubItemCD;
                }
            }

            allocGraph.Filter.Current.SiteID = (int?)cache.GetValue<SiteIDField>(currentRow);
            allocGraph.RefreshTotal.Press();
            return allocGraph;
        }

        public static int GetTransferLeadTime(PXGraph graph, int? inventoryID, int? fromSiteID)
        {
            INItemClassRep firstItemClassRep = null;

            foreach (PXResult<INItemRep, InventoryItem, INItemClassRep> result in PXSelectJoin<INItemRep,
                InnerJoin<InventoryItem,
                    On<INItemRep.inventoryID, Equal<InventoryItem.inventoryID>>,
                InnerJoin<INItemClassRep,
                    On<INItemClassRep.itemClassID, Equal<InventoryItem.itemClassID>,
                        And<INItemClassRep.replenishmentClassID, Equal<INItemRep.replenishmentClassID>>>>>,
                Where<INItemRep.inventoryID, Equal<Required<INItemRep.inventoryID>>>
                >.Select(graph, inventoryID))
            {
                var itemRep = (INItemRep)result;
                var itemClassRep = (INItemClassRep)result;

                if (itemRep.ReplenishmentSource != INReplenishmentSource.Transfer)
                {
                    continue;
                }

                if (itemRep.ReplenishmentSourceSiteID == fromSiteID)
                {
                    firstItemClassRep = itemClassRep;
                    break;
                }

                firstItemClassRep = itemClassRep;
            }

            return firstItemClassRep?.TransferLeadTime ?? 0;
        }

		public static int GetTransferLeadTime(PXGraph graph, int? inventoryID, int? fromSiteID, int? toSiteID)
		{
			if (fromSiteID == null || toSiteID == null)
			{
				return 0;
			}

			var itemSite = INItemSite.PK.Find(graph, inventoryID, toSiteID);
			if (itemSite != null && itemSite.ReplenishmentSourceSiteID == fromSiteID)
			{
				return PXCache<INItemSite>.GetExtension<INItemSiteExt>(itemSite)?.AMTransferLeadTime ?? 0;
			}

			return AMSiteTransfer.PK.Find(graph, fromSiteID, toSiteID)?.TransferLeadTime ?? 0;
		}

		public static InventoryItem CacheQueryInventoryItem(PXCache cache, int? inventoryId)
        {
            if (cache == null || inventoryId == null)
            {
                return null;
            }

            var row = cache.Cached.RowCast<InventoryItem>().FirstOrDefault(r => r.InventoryID == inventoryId);
            if (row != null)
            {
                return row;
            }

            return InventoryItem.PK.Find(cache.Graph, inventoryId);
        }

        public static INItemSite CacheQueryINItemSite(PXCache cache, int? inventoryId, int? siteId)
        {
            if (cache == null || inventoryId == null || siteId == null)
            {
                return null;
            }

            var row = cache.Cached.RowCast<INItemSite>().FirstOrDefault(r => r.InventoryID == inventoryId && r.SiteID == siteId);
            if (row != null)
            {
                return row;
            }

            return INItemSite.PK.Find(cache.Graph, inventoryId, siteId);
        }

		/// <summary>
		/// Determine if the LotSerialNbr for the split record is a dummy place holder
		/// </summary>
		public static bool IsLotSerialTempAssigned(AMProdItemSplit split)
		{
			return split != null && IsLotSerialTempAssigned(split.AssignedNbr, split.LotSerialNbr);
		}

		/// <summary>
		/// Determine if the LotSerialNbr for the split record is a dummy place holder
		/// </summary>
		public static bool IsLotSerialTempAssigned(string assignedNbr, string lotSerialNbr)
		{
			return !string.IsNullOrEmpty(assignedNbr) && INLotSerialNbrAttribute.StringsEqual(assignedNbr, lotSerialNbr);
		}

		/// <summary>
		/// Has the lot/serial nbr been assigned to another production order
		/// </summary>
		public static bool IsLotSerialPreassigned(PXGraph graph, AMProdItemSplit split, IProdOrder prodOrderExclude, out string foundOrderType, out string foundProdOrdID)
		{
			foundOrderType = null;
			foundProdOrdID = null;
			return !IsLotSerialTempAssigned(split) && IsLotSerialPreassigned(graph, split?.InventoryID, split?.LotSerialNbr, prodOrderExclude, out foundOrderType, out foundProdOrdID);
		}

		/// <summary>
		/// Has the lot/serial nbr been assigned to another production order
		/// </summary>
		public static bool IsLotSerialPreassigned(PXGraph graph, int? inventoryID, string lotSerial, IProdOrder prodOrderExclude, out string foundOrderType, out string foundProdOrdID)
        {
			foundOrderType = null;
			foundProdOrdID = null;

			if(inventoryID == null || string.IsNullOrWhiteSpace(lotSerial))
			{
				return false;
			}

			foreach (AMProdItemSplit split in SelectFrom<AMProdItemSplit>
				.Where<AMProdItemSplit.inventoryID.IsEqual<@P.AsInt>
					.And<AMProdItemSplit.lotSerialNbr.IsEqual<@P.AsString>>>
				.View.Select(graph, inventoryID, lotSerial))
			{
				if(split?.ProdOrdID == null || (prodOrderExclude != null && prodOrderExclude.IsSameOrder(split)))
				{
					continue;
				}

				if(split.Qty != 0m)
				{
					foundOrderType = split.OrderType;
					foundProdOrdID = split.ProdOrdID;
					return true;
				}
			}

            return false;
        }

		/// <summary>
		/// Checks if an Inventory item is lot/serial tracked
		/// </summary>
		/// <returns>True if record is lot/serial tracked</returns>
		public static bool IsItemLotSerialTracked(PXGraph graph, int? inventoryId)
		{
			INLotSerClass item = GetItemLotSerClass(graph, inventoryId);
			if (item == null || item.LotSerTrack == INLotSerTrack.NotNumbered) return false;
			return true;
		}
	}
}
