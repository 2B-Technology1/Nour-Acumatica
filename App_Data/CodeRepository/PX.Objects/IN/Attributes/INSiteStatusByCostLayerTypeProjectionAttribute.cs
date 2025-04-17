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
	public class INSiteStatusByCostLayerTypeProjectionAttribute : PXProjectionAttribute
	{
		public INSiteStatusByCostLayerTypeProjectionAttribute() : base(typeof(
			SelectFrom<INSiteStatusByCostCenter>
			.LeftJoin<INCostCenter>.On<INSiteStatusByCostCenter.costCenterID.IsEqual<INCostCenter.costCenterID>>
			.AggregateTo<GroupBy<INSiteStatusByCostCenter.inventoryID>, GroupBy<INSiteStatusByCostCenter.subItemID>, GroupBy<INSiteStatusByCostCenter.siteID>, GroupBy<INCostCenter.costLayerType>,
				Sum<INSiteStatusByCostCenter.qtyOnHand>, Sum<INSiteStatusByCostCenter.qtyNotAvail>,
				Sum<INSiteStatusByCostCenter.qtyAvail>, Sum<INSiteStatusByCostCenter.qtyHardAvail>,
				Sum<INSiteStatusByCostCenter.qtyActual>, Sum<INSiteStatusByCostCenter.qtyInTransit>,
				Sum<INSiteStatusByCostCenter.qtyInTransitToSO>, Sum<INSiteStatusByCostCenter.qtyPOPrepared>,
				Sum<INSiteStatusByCostCenter.qtyPOOrders>, Sum<INSiteStatusByCostCenter.qtyPOReceipts>,
				Sum<INSiteStatusByCostCenter.qtyFSSrvOrdBooked>, Sum<INSiteStatusByCostCenter.qtyFSSrvOrdAllocated>,
				Sum<INSiteStatusByCostCenter.qtyFSSrvOrdPrepared>, Sum<INSiteStatusByCostCenter.qtySOBackOrdered>,
				Sum<INSiteStatusByCostCenter.qtySOPrepared>, Sum<INSiteStatusByCostCenter.qtySOBooked>,
				Sum<INSiteStatusByCostCenter.qtySOShipped>, Sum<INSiteStatusByCostCenter.qtySOShipping>,
				Sum<INSiteStatusByCostCenter.qtyINIssues>, Sum<INSiteStatusByCostCenter.qtyINReceipts>,
				Sum<INSiteStatusByCostCenter.qtyINAssemblyDemand>, Sum<INSiteStatusByCostCenter.qtyINAssemblySupply>>))
		{
		}

		protected override Type GetSelect(PXCache sender)
		{
			if (INSiteStatusProjectionAttribute.NonFreeStockExists())
				return base.GetSelect(sender);
			else
				return typeof(SelectFrom<INSiteStatusByCostCenter>.
					LeftJoin<INCostCenter>.On<int1.IsEqual<int0>>.
					Where<INSiteStatusByCostCenter.costCenterID.IsEqual<CostCenter.freeStock>>);
		}
	}
}
