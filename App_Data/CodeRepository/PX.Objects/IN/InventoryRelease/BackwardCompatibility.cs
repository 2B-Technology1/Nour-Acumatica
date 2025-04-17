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
using PX.Common;
using PX.Data;

namespace PX.Objects.IN
{
	namespace Overrides.INDocumentRelease
	{
		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class ItemLotSerial : InventoryRelease.Accumulators.QtyAllocated.ItemLotSerial { }
		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class SiteLotSerial : InventoryRelease.Accumulators.QtyAllocated.SiteLotSerial { }

		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class AverageCostStatus : InventoryRelease.Accumulators.CostStatuses.AverageCostStatus { }
		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class FIFOCostStatus : InventoryRelease.Accumulators.CostStatuses.FIFOCostStatus { }
		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class OversoldCostStatus : InventoryRelease.Accumulators.CostStatuses.OversoldCostStatus { }
		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class SpecificCostStatus : InventoryRelease.Accumulators.CostStatuses.SpecificCostStatus { }
		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class SpecificTransitCostStatus : InventoryRelease.Accumulators.CostStatuses.SpecificTransitCostStatus { }
		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class StandardCostStatus : InventoryRelease.Accumulators.CostStatuses.StandardCostStatus { }
		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class UnmanagedCostStatus : InventoryRelease.Accumulators.CostStatuses.UnmanagedCostStatus { }

		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class ARTranUpdate : InventoryRelease.Accumulators.Documents.ARTranUpdate { }
		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class INTranCostUpdate : InventoryRelease.Accumulators.Documents.INTranCostUpdate { }
		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class INTranSplitAdjustmentUpdate : InventoryRelease.Accumulators.Documents.INTranSplitAdjustmentUpdate { }
		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class INTranSplitUpdate : InventoryRelease.Accumulators.Documents.INTranSplitUpdate { }
		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class INTranUpdate : InventoryRelease.Accumulators.Documents.INTranUpdate { }
		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class POReceiptLineUpdate : InventoryRelease.Accumulators.Documents.POReceiptLineUpdate { }
		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class SOShipLineUpdate : InventoryRelease.Accumulators.Documents.SOShipLineUpdate { }

		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class ItemCostHist : InventoryRelease.Accumulators.ItemHistory.ItemCostHist { }
		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class ItemCustSalesHist : InventoryRelease.Accumulators.ItemHistory.ItemCustSalesHist { }
		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class ItemSalesHist : InventoryRelease.Accumulators.ItemHistory.ItemSalesHist { }
		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class ItemSalesHistD : InventoryRelease.Accumulators.ItemHistory.ItemSalesHistD { }
		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class ItemSiteHist : InventoryRelease.Accumulators.ItemHistory.ItemSiteHist { }
		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class ItemSiteHistD : InventoryRelease.Accumulators.ItemHistory.ItemSiteHistD { }
		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class ItemSiteHistDay : InventoryRelease.Accumulators.ItemHistory.ItemSiteHistDay { }

		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class ItemCost : InventoryRelease.Accumulators.Statistics.Item.ItemCost { }
		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class ItemStats : InventoryRelease.Accumulators.Statistics.Item.ItemStats { }
		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class ItemCustDropShipStats : InventoryRelease.Accumulators.Statistics.ItemCustomer.ItemCustDropShipStats { }
		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class ItemCustSalesStats : InventoryRelease.Accumulators.Statistics.ItemCustomer.ItemCustSalesStats { }

		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class ReceiptStatus : InventoryRelease.Accumulators.ReceiptStatus { }

		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class ReadOnlyCostStatus : InventoryRelease.DAC.ReadOnlyCostStatus { }
		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class ReadOnlyReceiptStatus : InventoryRelease.DAC.ReadOnlyReceiptStatus { }
	}

	namespace DAC.Accumulators
	{
		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class LocationStatusByCostCenter : InventoryRelease.Accumulators.QtyAllocated.LocationStatusByCostCenter { }
		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class LotSerialStatusByCostCenter : InventoryRelease.Accumulators.QtyAllocated.LotSerialStatusByCostCenter { }
		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class SiteStatusByCostCenter : InventoryRelease.Accumulators.QtyAllocated.SiteStatusByCostCenter { }
		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class TransitLocationStatusByCostCenter : InventoryRelease.Accumulators.QtyAllocated.TransitLocationStatusByCostCenter { }
		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class TransitLotSerialStatusByCostCenter : InventoryRelease.Accumulators.QtyAllocated.TransitLotSerialStatusByCostCenter { }
		[Obsolete("Use base class instead."), PXHidden, PXInternalUseOnly] public class TransitSiteStatusByCostCenter : InventoryRelease.Accumulators.QtyAllocated.TransitSiteStatusByCostCenter { }
	}
}
