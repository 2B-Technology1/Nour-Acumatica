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
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.IN
{
	[System.SerializableAttribute()]
	[PXCacheName(Messages.INItemSiteHistByLastDayInPeriod)]
	[PXProjection(typeof(Select5<INItemSiteHistDay,
		InnerJoin<MasterFinPeriod,
			On<INItemSiteHistDay.sDate, Less<MasterFinPeriod.endDate>,
				And<MasterFinPeriod.startDate, NotEqual<MasterFinPeriod.endDate>>>>, // Adjustment
		Aggregate<GroupBy<INItemSiteHistDay.inventoryID,
			GroupBy<INItemSiteHistDay.subItemID,
			GroupBy<INItemSiteHistDay.siteID,
			GroupBy<INItemSiteHistDay.locationID,
			Max<INItemSiteHistDay.sDate,
			GroupBy<MasterFinPeriod.finPeriodID>>>>>>>>))]
	public partial class INItemSiteHistByLastDayInPeriod : PX.Data.IBqlTable
	{
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		[StockItem(IsKey = true, BqlField = typeof(INItemSiteHistDay.inventoryID))]
		[PXDefault()]
		public virtual Int32? InventoryID
		{
			get;
			set;
		}
		#endregion
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		[SubItem(IsKey = true, BqlField = typeof(INItemSiteHistDay.subItemID))]
		[PXDefault()]
		public virtual Int32? SubItemID
		{
			get;
			set;
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		[Site(IsKey = true, BqlField = typeof(INItemSiteHistDay.siteID))]
		[PXDefault()]
		public virtual Int32? SiteID
		{
			get;
			set;
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		[IN.Location(typeof(siteID), IsKey = true, BqlField = typeof(INItemSiteHistDay.locationID))]
		[PXDefault()]
		public virtual Int32? LocationID
		{
			get;
			set;
		}
		#endregion
		#region LastActivityDate
		public abstract class lastActivityDate : PX.Data.BQL.BqlDateTime.Field<lastActivityDate> { }
		[PXDBDate(BqlField = typeof(INItemSiteHistDay.sDate))]
		public virtual DateTime? LastActivityDate
		{
			get;
			set;
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		[PXDBString(6, IsKey = true, IsFixed = true, BqlField = typeof(MasterFinPeriod.finPeriodID))]
		[PXUIField(Visibility = PXUIVisibility.SelectorVisible, Visible = true, Enabled = false, DisplayName = "Financial Period ID")]
		public virtual String FinPeriodID
		{
			get;
			set;
		}
		#endregion
	}
}
