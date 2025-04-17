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
	public class INLotSerialStatusProjectionAttribute : PXProjectionAttribute
	{
		public INLotSerialStatusProjectionAttribute() : base(typeof(
			SelectFrom<INLotSerialStatusByCostCenter>
			.InnerJoin<INItemLotSerial>.On<INLotSerialStatusByCostCenter.FK.ItemLotSerial>
			.AggregateTo<
				GroupBy<INLotSerialStatusByCostCenter.inventoryID>, GroupBy<INLotSerialStatusByCostCenter.subItemID>, GroupBy<INLotSerialStatusByCostCenter.siteID>,
				GroupBy<INLotSerialStatusByCostCenter.locationID>, GroupBy<INLotSerialStatusByCostCenter.lotSerialNbr>,
				Sum<INLotSerialStatusByCostCenter.qtyOnHand>, Sum<INLotSerialStatusByCostCenter.qtyAvail>,
				Sum<INLotSerialStatusByCostCenter.qtyHardAvail>, Sum<INLotSerialStatusByCostCenter.qtyActual>, Sum<INLotSerialStatusByCostCenter.qtyInTransit>,
				Sum<INLotSerialStatusByCostCenter.qtyInTransitToSO>, Sum<INLotSerialStatusByCostCenter.qtyPOPrepared>, Sum<INLotSerialStatusByCostCenter.qtyPOOrders>,
				Sum<INLotSerialStatusByCostCenter.qtyPOReceipts>, Sum<INLotSerialStatusByCostCenter.qtyFSSrvOrdBooked>, Sum<INLotSerialStatusByCostCenter.qtyFSSrvOrdAllocated>,
				Sum<INLotSerialStatusByCostCenter.qtyFSSrvOrdPrepared>, Sum<INLotSerialStatusByCostCenter.qtySOBackOrdered>, Sum<INLotSerialStatusByCostCenter.qtySOPrepared>,
				Sum<INLotSerialStatusByCostCenter.qtySOBooked>, Sum<INLotSerialStatusByCostCenter.qtySOShipped>, Sum<INLotSerialStatusByCostCenter.qtySOShipping>,
				Sum<INLotSerialStatusByCostCenter.qtyINIssues>, Sum<INLotSerialStatusByCostCenter.qtyINReceipts>, Sum<INLotSerialStatusByCostCenter.qtyINAssemblyDemand>,
				Sum<INLotSerialStatusByCostCenter.qtyINAssemblySupply>, Sum<INLotSerialStatusByCostCenter.qtyInTransitToProduction>, Sum<INLotSerialStatusByCostCenter.qtyProductionSupplyPrepared>,
				Sum<INLotSerialStatusByCostCenter.qtyProductionSupply>, Sum<INLotSerialStatusByCostCenter.qtyPOFixedProductionPrepared>, Sum<INLotSerialStatusByCostCenter.qtyPOFixedProductionOrders>,
				Sum<INLotSerialStatusByCostCenter.qtyProductionDemandPrepared, Sum<INLotSerialStatusByCostCenter.qtyProductionDemand, Sum<INLotSerialStatusByCostCenter.qtyProductionAllocated,
				Sum<INLotSerialStatusByCostCenter.qtySOFixedProduction, Sum<INLotSerialStatusByCostCenter.qtyProdFixedPurchase, Sum<INLotSerialStatusByCostCenter.qtyProdFixedProduction,
				Sum<INLotSerialStatusByCostCenter.qtyProdFixedSalesOrdersPrepared, Sum<INLotSerialStatusByCostCenter.qtyProdFixedProdOrders, Sum<INLotSerialStatusByCostCenter.qtyProdFixedProdOrdersPrepared,
				Sum<INLotSerialStatusByCostCenter.qtyProdFixedProdOrders, Sum<INLotSerialStatusByCostCenter.qtyProdFixedSalesOrdersPrepared, Sum<INLotSerialStatusByCostCenter.qtyProdFixedSalesOrders,
				Sum<INLotSerialStatusByCostCenter.qtyFixedFSSrvOrd, Sum<INLotSerialStatusByCostCenter.qtyPOFixedFSSrvOrd,
				Sum<INLotSerialStatusByCostCenter.qtyPOFixedFSSrvOrdPrepared, Sum<INLotSerialStatusByCostCenter.qtyPOFixedFSSrvOrdReceipts,
				Sum<INLotSerialStatusByCostCenter.qtySOFixed, Sum<INLotSerialStatusByCostCenter.qtyPOFixedOrders, Sum<INLotSerialStatusByCostCenter.qtyPOFixedPrepared,
				Sum<INLotSerialStatusByCostCenter.qtyPOFixedReceipts, Sum<INLotSerialStatusByCostCenter.qtySODropShip, Sum<INLotSerialStatusByCostCenter.qtyPODropShipOrders,
				Sum<INLotSerialStatusByCostCenter.qtyPODropShipPrepared, Sum<INLotSerialStatusByCostCenter.qtyPODropShipReceipts,
				Min<INLotSerialStatusByCostCenter.receiptDate>>>>>>>>>>>>>>>>>>>>>>>>>>))
		{
			Persistent = false;
		}

		protected override Type GetSelect(PXCache sender)
		{
			if (INSiteStatusProjectionAttribute.NonFreeStockExists())
				return base.GetSelect(sender);
			else
				return typeof(SelectFrom<INLotSerialStatusByCostCenter>
					.InnerJoin<INItemLotSerial>.On<INLotSerialStatusByCostCenter.FK.ItemLotSerial>
					.Where<INLotSerialStatusByCostCenter.costCenterID.IsEqual<CostCenter.freeStock>>);
		}
	}
}
