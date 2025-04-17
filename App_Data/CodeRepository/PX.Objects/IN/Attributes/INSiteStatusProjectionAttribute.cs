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
using System.Linq;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.Common;
using PX.Objects.CS;

namespace PX.Objects.IN
{
	public class INSiteStatusProjectionAttribute : PXProjectionAttribute
	{
		public INSiteStatusProjectionAttribute() : base(typeof(
			SelectFrom<INSiteStatusByCostCenter>
			.AggregateTo<
				GroupBy<INSiteStatusByCostCenter.inventoryID>, GroupBy<INSiteStatusByCostCenter.subItemID>, GroupBy<INSiteStatusByCostCenter.siteID>,
				Sum<INSiteStatusByCostCenter.qtyOnHand>, Sum<INSiteStatusByCostCenter.qtyNotAvail>, Sum<INSiteStatusByCostCenter.qtyAvail>,
				Sum<INSiteStatusByCostCenter.qtyHardAvail>, Sum<INSiteStatusByCostCenter.qtyActual>, Sum<INSiteStatusByCostCenter.qtyInTransit>,
				Sum<INSiteStatusByCostCenter.qtyInTransitToSO>, Sum<INSiteStatusByCostCenter.qtyPOPrepared>, Sum<INSiteStatusByCostCenter.qtyPOOrders>,
				Sum<INSiteStatusByCostCenter.qtyPOReceipts>, Sum<INSiteStatusByCostCenter.qtyFSSrvOrdBooked>, Sum<INSiteStatusByCostCenter.qtyFSSrvOrdAllocated>,
				Sum<INSiteStatusByCostCenter.qtyFSSrvOrdPrepared>, Sum<INSiteStatusByCostCenter.qtySOBackOrdered>, Sum<INSiteStatusByCostCenter.qtySOPrepared>,
				Sum<INSiteStatusByCostCenter.qtySOBooked>, Sum<INSiteStatusByCostCenter.qtySOShipped>, Sum<INSiteStatusByCostCenter.qtySOShipping>,
				Sum<INSiteStatusByCostCenter.qtyINIssues>, Sum<INSiteStatusByCostCenter.qtyINReceipts>, Sum<INSiteStatusByCostCenter.qtyINAssemblyDemand>,
				Sum<INSiteStatusByCostCenter.qtyINAssemblySupply>, Sum<INSiteStatusByCostCenter.qtyInTransitToProduction>, Sum<INSiteStatusByCostCenter.qtyProductionSupplyPrepared>,
				Sum<INSiteStatusByCostCenter.qtyProductionSupply>, Sum<INSiteStatusByCostCenter.qtyPOFixedProductionPrepared>, Sum<INSiteStatusByCostCenter.qtyPOFixedProductionOrders>,
				Sum<INSiteStatusByCostCenter.qtyProductionDemandPrepared>, Sum<INSiteStatusByCostCenter.qtyProductionDemand, Sum<INSiteStatusByCostCenter.qtyProductionAllocated,
				Sum<INSiteStatusByCostCenter.qtySOFixedProduction, Sum<INSiteStatusByCostCenter.qtyProdFixedPurchase, Sum<INSiteStatusByCostCenter.qtyProdFixedProduction,
				Sum<INSiteStatusByCostCenter.qtyProdFixedSalesOrdersPrepared, Sum<INSiteStatusByCostCenter.qtyProdFixedProdOrders, Sum<INSiteStatusByCostCenter.qtyProdFixedProdOrdersPrepared,
				Sum<INSiteStatusByCostCenter.qtyProdFixedProdOrders, Sum<INSiteStatusByCostCenter.qtyProdFixedSalesOrdersPrepared, Sum<INSiteStatusByCostCenter.qtyProdFixedSalesOrders,
				Sum<INSiteStatusByCostCenter.qtyINReplaned, Sum<INSiteStatusByCostCenter.qtyFixedFSSrvOrd, Sum<INSiteStatusByCostCenter.qtyPOFixedFSSrvOrd,
				Sum<INSiteStatusByCostCenter.qtyPOFixedFSSrvOrdPrepared, Sum<INSiteStatusByCostCenter.qtyPOFixedFSSrvOrdReceipts,
				Sum<INSiteStatusByCostCenter.qtySOFixed, Sum<INSiteStatusByCostCenter.qtyPOFixedOrders, Sum<INSiteStatusByCostCenter.qtyPOFixedPrepared,
				Sum<INSiteStatusByCostCenter.qtyPOFixedReceipts, Sum<INSiteStatusByCostCenter.qtySODropShip, Sum<INSiteStatusByCostCenter.qtyPODropShipOrders,
				Sum<INSiteStatusByCostCenter.qtyPODropShipPrepared, Sum<INSiteStatusByCostCenter.qtyPODropShipReceipts>>>>>>>>>>>>>>>>>>>>>>>>>))
		{
			Persistent = false;
		}

		protected override Type GetSelect(PXCache sender)
		{
			if (NonFreeStockExists())
				return base.GetSelect(sender);
			else
				return typeof(SelectFrom<INSiteStatusByCostCenter>.Where<INSiteStatusByCostCenter.costCenterID.IsEqual<CostCenter.freeStock>>);
		}

		public static bool NonFreeStockExists()
			=> (PXAccess.FeatureInstalled<FeaturesSet.specialOrders>() || PXAccess.FeatureInstalled<FeaturesSet.materialManagement>())
			&& RecordExistsSlot<INCostCenter, INCostCenter.costCenterID>.IsRowsExists();
	}
}
