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
	public class INLocationStatusByCostLayerTypeProjectionAttribute : PXProjectionAttribute
	{
		public INLocationStatusByCostLayerTypeProjectionAttribute() : base(typeof(
			SelectFrom<INLocationStatusByCostCenter>
			.LeftJoin<INCostCenter>.On<INLocationStatusByCostCenter.costCenterID.IsEqual<INCostCenter.costCenterID>>
			.AggregateTo<GroupBy<INLocationStatusByCostCenter.inventoryID>, GroupBy<INLocationStatusByCostCenter.subItemID>, GroupBy<INLocationStatusByCostCenter.siteID>,
				GroupBy<INLocationStatusByCostCenter.locationID>, GroupBy<INCostCenter.costLayerType>,
				Sum<INLocationStatusByCostCenter.qtyOnHand>, Sum<INLocationStatusByCostCenter.qtyAvail>,
				Sum<INLocationStatusByCostCenter.qtyHardAvail>, Sum<INLocationStatusByCostCenter.qtyActual>,
				Sum<INLocationStatusByCostCenter.qtyInTransit>, Sum<INLocationStatusByCostCenter.qtyInTransitToSO>,
				Sum<INLocationStatusByCostCenter.qtyPOPrepared>, Sum<INLocationStatusByCostCenter.qtyPOOrders>,
				Sum<INLocationStatusByCostCenter.qtyPOReceipts>, Sum<INLocationStatusByCostCenter.qtyFSSrvOrdBooked>,
				Sum<INLocationStatusByCostCenter.qtyFSSrvOrdAllocated>, Sum<INLocationStatusByCostCenter.qtyFSSrvOrdPrepared>,
				Sum<INLocationStatusByCostCenter.qtySOBackOrdered>, Sum<INLocationStatusByCostCenter.qtySOPrepared>,
				Sum<INLocationStatusByCostCenter.qtySOBooked>, Sum<INLocationStatusByCostCenter.qtySOShipped>,
				Sum<INLocationStatusByCostCenter.qtySOShipping>, Sum<INLocationStatusByCostCenter.qtyINIssues>,
				Sum<INLocationStatusByCostCenter.qtyINReceipts>, Sum<INLocationStatusByCostCenter.qtyINAssemblyDemand>,
				Sum<INLocationStatusByCostCenter.qtyINAssemblySupply>>))
		{
		}

		protected override Type GetSelect(PXCache sender)
		{
			if (INSiteStatusProjectionAttribute.NonFreeStockExists())
				return base.GetSelect(sender);
			else
				return typeof(SelectFrom<INLocationStatusByCostCenter>.
					LeftJoin<INCostCenter>.On<int1.IsEqual<int0>>.
					Where<INLocationStatusByCostCenter.costCenterID.IsEqual<CostCenter.freeStock>>);
		}

	}
}
