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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

namespace PX.Objects.IN
{
	[PXCacheName(Messages.INCartContentByLocation)]
	[PXProjection(typeof(
		SelectFrom<INCartSplit>.
		AggregateTo<
			GroupBy<INCartSplit.siteID>,
			GroupBy<INCartSplit.fromLocationID>,
			GroupBy<INCartSplit.inventoryID>,
			GroupBy<INCartSplit.subItemID>,
			Sum<INCartSplit.baseQty>>
		), Persistent = false)]
	public class INCartContentByLocation : IBqlTable
	{
		#region Keys
		public static class FK
		{
			public class Site : INSite.PK.ForeignKeyOf<INCartContentByLocation>.By<siteID> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INCartContentByLocation>.By<inventoryID> { }
			public class SubItem : INSubItem.PK.ForeignKeyOf<INCartContentByLocation>.By<subItemID> { }
			public class Location : INLocation.PK.ForeignKeyOf<INCartContentByLocation>.By<locationID> { }
			public class LocationStatus : INLocationStatus.PK.ForeignKeyOf<INCartContentByLocation>.By<inventoryID, subItemID, siteID, locationID> { }
		}
		#endregion
		#region SiteID
		[Site(IsKey = true, BqlTable = typeof(INCartSplit))]
		public int? SiteID { get; set; }
		public abstract class siteID : BqlInt.Field<siteID> { }
		#endregion
		#region LocationID
		[Location(typeof(siteID), IsKey = true, BqlField = typeof(INCartSplit.fromLocationID))]
		public virtual Int32? LocationID { get; set; }
		public abstract class locationID : BqlInt.Field<locationID> { }
		#endregion
		#region InventoryID
		[StockItem(IsKey = true, BqlTable = typeof(INCartSplit))]
		public virtual Int32? InventoryID { get; set; }
		public abstract class inventoryID : BqlInt.Field<inventoryID> { }
		#endregion
		#region SubItemID
		[SubItem(typeof(inventoryID), IsKey = true, BqlTable = typeof(INCartSplit))]
		public virtual Int32? SubItemID { get; set; }
		public abstract class subItemID : BqlInt.Field<subItemID> { }
		#endregion
		#region BaseQty
		[PXDBDecimal(6, BqlTable = typeof(INCartSplit))]
		public virtual Decimal? BaseQty { get; set; }
		public abstract class baseQty : BqlDecimal.Field<baseQty> { }
		#endregion
	}

	[PXCacheName(Messages.INCartContentByLotSerial)]
	[PXProjection(typeof(
		SelectFrom<INCartSplit>.
		AggregateTo<
			GroupBy<INCartSplit.siteID>,
			GroupBy<INCartSplit.fromLocationID>,
			GroupBy<INCartSplit.inventoryID>,
			GroupBy<INCartSplit.subItemID>,
			GroupBy<INCartSplit.lotSerialNbr>,
			Sum<INCartSplit.baseQty>>
		), Persistent = false)]
	public class INCartContentByLotSerial : IBqlTable
	{
		#region Keys
		public static class FK
		{
			public class Site : INSite.PK.ForeignKeyOf<INCartContentByLotSerial>.By<siteID> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INCartContentByLotSerial>.By<inventoryID> { }
			public class SubItem : INSubItem.PK.ForeignKeyOf<INCartContentByLotSerial>.By<subItemID> { }
			public class Location : INLocation.PK.ForeignKeyOf<INCartContentByLotSerial>.By<locationID> { }
			public class LocationStatus : INLocationStatus.PK.ForeignKeyOf<INCartContentByLotSerial>.By<inventoryID, subItemID, siteID, locationID> { }
			public class LotSerialStatus : INLotSerialStatus.PK.ForeignKeyOf<INCartContentByLotSerial>.By<inventoryID, subItemID, siteID, locationID, lotSerialNbr> { }
		}
		#endregion
		#region SiteID
		[Site(IsKey = true, BqlTable = typeof(INCartSplit))]
		public int? SiteID { get; set; }
		public abstract class siteID : BqlInt.Field<siteID> { }
		#endregion
		#region LocationID
		[Location(typeof(siteID), IsKey = true, BqlField = typeof(INCartSplit.fromLocationID))]
		public virtual Int32? LocationID { get; set; }
		public abstract class locationID : BqlInt.Field<locationID> { }
		#endregion
		#region InventoryID
		[StockItem(IsKey = true, BqlTable = typeof(INCartSplit))]
		public virtual Int32? InventoryID { get; set; }
		public abstract class inventoryID : BqlInt.Field<inventoryID> { }
		#endregion
		#region SubItemID
		[SubItem(typeof(inventoryID), IsKey = true, BqlTable = typeof(INCartSplit))]
		public virtual Int32? SubItemID { get; set; }
		public abstract class subItemID : BqlInt.Field<subItemID> { }
		#endregion
		#region LotSerialNbr
		[LotSerialNbr(IsKey = true, BqlTable = typeof(INCartSplit))]
		public virtual String LotSerialNbr { get; set; }
		public abstract class lotSerialNbr : BqlString.Field<lotSerialNbr> { }
		#endregion
		#region BaseQty
		[PXDBDecimal(6, BqlTable = typeof(INCartSplit))]
		public virtual Decimal? BaseQty { get; set; }
		public abstract class baseQty : BqlDecimal.Field<baseQty> { }
		#endregion
	}
}
