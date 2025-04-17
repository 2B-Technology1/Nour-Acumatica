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
using PX.SM;

namespace PX.Objects.IN
{
	[System.SerializableAttribute()]
	[PXCacheName(Messages.INItemSiteHistByDay)]
	[PXProjection(typeof(Select5<INItemSiteHistDay,
		InnerJoin<DateInfo, On<DateInfo.date, GreaterEqual<INItemSiteHistDay.sDate>>>,
			Aggregate<GroupBy<INItemSiteHistDay.inventoryID,
				GroupBy<INItemSiteHistDay.subItemID,
				GroupBy<INItemSiteHistDay.siteID,
				GroupBy<INItemSiteHistDay.locationID,
				Max<INItemSiteHistDay.sDate,
				GroupBy<DateInfo.date>>>>>>>>))]
	public partial class INItemSiteHistByDay : PX.Data.IBqlTable
	{
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID>
		{
		}
		protected Int32? _InventoryID;
		[StockItem(IsKey = true, BqlField = typeof(INItemSiteHistDay.inventoryID))]
		[PXDefault()]
		public virtual Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID>
		{
		}
		protected Int32? _SubItemID;
		[SubItem(IsKey = true, BqlField = typeof(INItemSiteHistDay.subItemID))]
		[PXDefault()]
		public virtual Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID>
		{
		}
		protected Int32? _SiteID;
		[Site(IsKey = true, BqlField = typeof(INItemSiteHistDay.siteID))]
		[PXDefault()]
		public virtual Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID>
		{
		}
		protected Int32? _LocationID;
		[IN.Location(typeof(siteID), IsKey = true, BqlField = typeof(INItemSiteHistDay.locationID))]
		[PXDefault()]
		public virtual Int32? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion
		#region LastActivityDate
		public abstract class lastActivityDate : PX.Data.BQL.BqlDateTime.Field<lastActivityDate>
		{
		}
		protected DateTime? _LastActivityDate;
		[PXDBDate(BqlField = typeof(INItemSiteHistDay.sDate))]
		public virtual DateTime? LastActivityDate
		{
			get
			{
				return this._LastActivityDate;
			}
			set
			{
				this._LastActivityDate = value;
			}
		}
		#endregion
		#region Date
		public abstract class date : PX.Data.BQL.BqlDateTime.Field<date>
		{
		}
		protected DateTime? _date;
		[PXDBDate(IsKey = true, BqlField = typeof(DateInfo.date))]
		[PXUIField(DisplayName = "Date")]
		public virtual DateTime? Date
		{
			get
			{
				return this._date;
			}
			set
			{
				this._date = value;
			}
		}
		#endregion
	}
}
