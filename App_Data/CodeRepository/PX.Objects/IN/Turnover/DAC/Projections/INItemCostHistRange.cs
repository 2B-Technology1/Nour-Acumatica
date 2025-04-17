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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

namespace PX.Objects.IN.Turnover
{
	[PXHidden]
	[PXProjection(typeof(
		SelectFrom<INItemCostHist>
			.Where<INItemCostHist.finPeriodID.IsLessEqual<INTurnoverCalc.toPeriodID.FromCurrent.Value>>
		.AggregateTo<
			GroupBy<INItemCostHist.inventoryID>,
			GroupBy<INItemCostHist.costSubItemID>,
			GroupBy<INItemCostHist.costSiteID>,
			GroupBy<INItemCostHist.accountID>,
			GroupBy<INItemCostHist.subID>,
			Max<INItemCostHist.siteID>>
		), Persistent = false)]
	public class INItemCostHistRange :  IBqlTable
	{
		#region InventoryID
		[StockItem(IsKey = true, BqlField = typeof(INItemCostHist.inventoryID))]
		public virtual int? InventoryID { get; set; }
		public abstract class inventoryID : BqlInt.Field<inventoryID> { }
		#endregion
		#region CostSubItemID
		[SubItem(IsKey = true, BqlField = typeof(INItemCostHist.costSubItemID))]
		public virtual int? CostSubItemID { get; set; }
		public abstract class costSubItemID : BqlInt.Field<costSubItemID> { }
		#endregion
		#region CostSiteID
		[Site(IsKey = true, BqlField = typeof(INItemCostHist.costSiteID))]
		public virtual int? CostSiteID { get; set; }
		public abstract class costSiteID : BqlInt.Field<costSiteID> { }
		#endregion
		#region AccountID
		[PXDBInt(IsKey = true, BqlField = typeof(INItemCostHist.accountID))]
		public virtual int? AccountID { get; set; }
		public abstract class accountID : BqlInt.Field<accountID> { }
		#endregion
		#region SubID
		[PXDBInt(IsKey = true, BqlField = typeof(INItemCostHist.subID))]
		public virtual int? SubID { get; set; }
		public abstract class subID : BqlInt.Field<subID> { }
		#endregion
		#region SiteID
		[Site(true, BqlField = typeof(INItemCostHist.siteID))]
		public virtual int? SiteID { get; set; }
		public abstract class siteID : BqlInt.Field<siteID> { }
		#endregion
	}
}
