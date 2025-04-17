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
using PX.Data.BQL.Fluent;
using PX.Objects.CS;

namespace PX.Objects.IN.Attributes
{
	public class INLotSerialStatusByCostLayerTypeProjectionAttribute : PXProjectionAttribute
	{
		public INLotSerialStatusByCostLayerTypeProjectionAttribute() : base(typeof(
			SelectFrom<INLotSerialStatusByCostCenter>
			.LeftJoin<INCostCenter>.On<INLotSerialStatusByCostCenter.costCenterID.IsEqual<INCostCenter.costCenterID>>
			.AggregateTo<GroupBy<INLotSerialStatusByCostCenter.inventoryID>, GroupBy<INLotSerialStatusByCostCenter.subItemID>, GroupBy<INLotSerialStatusByCostCenter.siteID>,
				GroupBy<INLotSerialStatusByCostCenter.locationID>, GroupBy<INLotSerialStatusByCostCenter.lotSerialNbr>, GroupBy<INCostCenter.costLayerType>,
				Sum<INLotSerialStatusByCostCenter.qtyOnHand>, Sum<INLotSerialStatusByCostCenter.qtyAvail>,
				Sum<INLotSerialStatusByCostCenter.qtyHardAvail>, Sum<INLotSerialStatusByCostCenter.qtyActual>,
				Sum<INLotSerialStatusByCostCenter.qtyInTransit>, Sum<INLotSerialStatusByCostCenter.qtyInTransitToSO>,
				Sum<INLotSerialStatusByCostCenter.qtyPOPrepared>, Sum<INLotSerialStatusByCostCenter.qtyPOOrders>,
				Sum<INLotSerialStatusByCostCenter.qtyPOReceipts>, Sum<INLotSerialStatusByCostCenter.qtyFSSrvOrdBooked>,
				Sum<INLotSerialStatusByCostCenter.qtyFSSrvOrdAllocated>, Sum<INLotSerialStatusByCostCenter.qtyFSSrvOrdPrepared>,
				Sum<INLotSerialStatusByCostCenter.qtySOBackOrdered>, Sum<INLotSerialStatusByCostCenter.qtySOPrepared>,
				Sum<INLotSerialStatusByCostCenter.qtySOBooked>, Sum<INLotSerialStatusByCostCenter.qtySOShipped>,
				Sum<INLotSerialStatusByCostCenter.qtySOShipping>, Sum<INLotSerialStatusByCostCenter.qtyINIssues>,
				Sum<INLotSerialStatusByCostCenter.qtyINReceipts>, Sum<INLotSerialStatusByCostCenter.qtyINAssemblyDemand>,
				Sum<INLotSerialStatusByCostCenter.qtyINAssemblySupply>>))
		{
		}

		protected override Type GetSelect(PXCache sender)
		{
			if (INSiteStatusProjectionAttribute.NonFreeStockExists())
				return base.GetSelect(sender);
			else
				return typeof(SelectFrom<INLotSerialStatusByCostCenter>.
					LeftJoin<INCostCenter>.On<int1.IsEqual<int0>>.
					Where<INLotSerialStatusByCostCenter.costCenterID.IsEqual<CostCenter.freeStock>>);
		}
	}
}
