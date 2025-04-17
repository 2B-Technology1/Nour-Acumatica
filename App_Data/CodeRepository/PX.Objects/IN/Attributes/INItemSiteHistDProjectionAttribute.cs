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

namespace PX.Objects.IN
{
	public class INItemSiteHistDProjectionAttribute : PXProjectionAttribute
	{
		public INItemSiteHistDProjectionAttribute() : base(typeof(
			SelectFrom<INItemSiteHistByCostCenterD>
			.AggregateTo<
				GroupBy<INItemSiteHistByCostCenterD.siteID>, GroupBy<INItemSiteHistByCostCenterD.inventoryID>,
				GroupBy<INItemSiteHistByCostCenterD.subItemID>, GroupBy<INItemSiteHistByCostCenterD.sDate>,
				Sum<INItemSiteHistByCostCenterD.qtyReceived>, Sum<INItemSiteHistByCostCenterD.qtyIssued>,
				Sum<INItemSiteHistByCostCenterD.qtySales>, Sum<INItemSiteHistByCostCenterD.qtyCreditMemos>,
				Sum<INItemSiteHistByCostCenterD.qtyDropShipSales>, Sum<INItemSiteHistByCostCenterD.qtyTransferIn>,
				Sum<INItemSiteHistByCostCenterD.qtyTransferOut>, Sum<INItemSiteHistByCostCenterD.qtyAssemblyIn>,
				Sum<INItemSiteHistByCostCenterD.qtyAssemblyOut>, Sum<INItemSiteHistByCostCenterD.qtyAdjusted>,
				Sum<INItemSiteHistByCostCenterD.begQty>, Sum<INItemSiteHistByCostCenterD.endQty>,
				Sum<INItemSiteHistByCostCenterD.qtyDebit>, Sum<INItemSiteHistByCostCenterD.qtyCredit>,
				Sum<INItemSiteHistByCostCenterD.costDebit>, Sum<INItemSiteHistByCostCenterD.costCredit>>))
		{
			Persistent = false;
		}

		protected override Type GetSelect(PXCache sender)
		{
			if (INSiteStatusProjectionAttribute.NonFreeStockExists())
				return base.GetSelect(sender);
			else
				return typeof(SelectFrom<INItemSiteHistByCostCenterD>.Where<INItemSiteHistByCostCenterD.costCenterID.IsEqual<CostCenter.freeStock>>);
		}
	}
}
