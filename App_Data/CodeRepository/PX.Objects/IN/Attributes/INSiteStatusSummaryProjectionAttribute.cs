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

using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.Common;
using PX.Objects.CS;
using System;

namespace PX.Objects.IN.Attributes
{

	public class INSiteStatusSummaryProjectionAttribute : PXProjectionAttribute
	{
		public INSiteStatusSummaryProjectionAttribute()
			: base(typeof(SelectFrom<INSiteStatusByCostCenter>
				.AggregateTo<
					GroupBy<INSiteStatusByCostCenter.inventoryID>, GroupBy<INSiteStatusByCostCenter.siteID>,
					Sum<INSiteStatusByCostCenter.qtyOnHand>, Sum<INSiteStatusByCostCenter.qtyAvail>, Sum<INSiteStatusByCostCenter.qtyNotAvail>>))
		{
			Persistent = false;
		}

		protected override Type GetSelect(PXCache sender)
		{
			if (SubItemsExist() || INSiteStatusProjectionAttribute.NonFreeStockExists())
				return base.GetSelect(sender);
			else
				return typeof(SelectFrom<INSiteStatusByCostCenter>
					.Where<INSiteStatusByCostCenter.subItemID.IsEqual<SubItemAttribute.defaultSubItemID>
						.And<INSiteStatusByCostCenter.costCenterID.IsEqual<CostCenter.freeStock>>>);
		}

		public static bool SubItemsExist()
			=> PXAccess.FeatureInstalled<FeaturesSet.subItem>()
				&& RecordExistsSlot<INSubItem, INSubItem.subItemID,
					Where<INSubItem.subItemID.IsNotEqual<SubItemAttribute.defaultSubItemID>>>.IsRowsExists();
	}
}
