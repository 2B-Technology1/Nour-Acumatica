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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CM;
using PX.Objects.CS;

namespace PX.Objects.IN
{
	[System.SerializableAttribute()]
	[PXCacheName(Messages.INItemStats)]
	public partial class INItemStats : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INItemStats>.By<inventoryID, siteID>
		{
			public static INItemStats Find(PXGraph graph, int? inventoryID, int? siteID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, inventoryID, siteID, options);
		}
		public static class FK
		{
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INItemStats>.By<inventoryID> { }
			public class Site : INSite.PK.ForeignKeyOf<INItemStats>.By<siteID> { }
			public class ItemSite : INItemSite.PK.ForeignKeyOf<INItemStats>.By<inventoryID, siteID> { }
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[PXDBInt(IsKey = true)]
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
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[PXDBInt(IsKey = true)]
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
		#region ValMethod
		public abstract class valMethod : PX.Data.BQL.BqlString.Field<valMethod> { }
		protected String _ValMethod;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(typeof(Search<InventoryItem.valMethod, Where<InventoryItem.inventoryID, Equal<Current<INItemStats.inventoryID>>>>))]
		public virtual String ValMethod
		{
			get
			{
				return this._ValMethod;
			}
			set
			{
				this._ValMethod = value;
			}
		}
		#endregion
		#region LastCost
		public abstract class lastCost : PX.Data.BQL.BqlDecimal.Field<lastCost> { }
		protected Decimal? _LastCost;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Last Cost", Enabled = false)]
		public virtual Decimal? LastCost
		{
			get
			{
				return this._LastCost;
			}
			set
			{
				this._LastCost = value;
			}
		}
		#endregion

		public class MinDate : PX.Data.BQL.BqlString.Constant<MinDate>
		{
			public const string VALUE = "01/01/1900";
			public MinDate() : base(VALUE) { }
			public static DateTime? get() { return new DateTime(1900, 1, 1); }
		}
		public class dateAfterMinDate : PX.Data.BQL.BqlDateTime.Constant<dateAfterMinDate>
		{
			public dateAfterMinDate() : base(MinDate.get().Value.AddDays(1)) { }
		}
		#region LastCostDate
		public abstract class lastCostDate : PX.Data.BQL.BqlDateTime.Field<lastCostDate> { }
		protected DateTime? _LastCostDate;
		[PXDBLastChangeDateTime(typeof(INItemStats.lastCost))]
		[PXDefault(TypeCode.DateTime, MinDate.VALUE)]
		public virtual DateTime? LastCostDate
		{
			get
			{
				return this._LastCostDate;
			}
			set
			{
				this._LastCostDate = value;
			}
		}
		#endregion
		#region AvgCost
		public abstract class avgCost : PX.Data.BQL.BqlDecimal.Field<avgCost> { }
		protected Decimal? _AvgCost;
		[PXDBPriceCostCalced(typeof(Switch<Case<Where<INItemStats.qtyOnHand, Equal<decimal0>>, decimal0>, Div<INItemStats.totalCost, INItemStats.qtyOnHand>>), typeof(Decimal), CastToScale = 9, CastToPrecision = 25)]
		[PXPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Average Cost", Enabled = false)]
		public virtual Decimal? AvgCost
		{
			get
			{
				return this._AvgCost;
			}
			set
			{
				this._AvgCost = value;
			}
		}
		#endregion
		#region MinCost
		public abstract class minCost : PX.Data.BQL.BqlDecimal.Field<minCost> { }
		protected Decimal? _MinCost;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Minimal Cost", Enabled = false)]
		public virtual Decimal? MinCost
		{
			get
			{
				return this._MinCost;
			}
			set
			{
				this._MinCost = value;
			}
		}
		#endregion
		#region MaxCost
		public abstract class maxCost : PX.Data.BQL.BqlDecimal.Field<maxCost> { }
		protected Decimal? _MaxCost;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Max. Cost", Enabled = false)]
		public virtual Decimal? MaxCost
		{
			get
			{
				return this._MaxCost;
			}
			set
			{
				this._MaxCost = value;
			}
		}
		#endregion
		#region QtyOnHand
		public abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
		protected Decimal? _QtyOnHand;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? QtyOnHand
		{
			get
			{
				return this._QtyOnHand;
			}
			set
			{
				this._QtyOnHand = value;
			}
		}
		#endregion
		#region TotalCost
		public abstract class totalCost : PX.Data.BQL.BqlDecimal.Field<totalCost> { }
		protected decimal? _TotalCost;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TotalCost
		{
			get
			{
				return this._TotalCost;
			}
			set
			{
				this._TotalCost = value;
			}
		}
		#endregion
		#region QtyReceived
		public abstract class qtyReceived : PX.Data.BQL.BqlDecimal.Field<qtyReceived> { }
		protected Decimal? _QtyReceived;
		[PXDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? QtyReceived
		{
			get
			{
				return this._QtyReceived;
			}
			set
			{
				this._QtyReceived = value;
			}
		}
		#endregion
		#region CostReceived
		public abstract class costReceived : PX.Data.BQL.BqlDecimal.Field<costReceived> { }
		protected decimal? _CostReceived;
		[PXDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? CostReceived
		{
			get
			{
				return this._CostReceived;
			}
			set
			{
				this._CostReceived = value;
			}
		}
		#endregion
		#region LastPurchaseDate
		public abstract class lastPurchaseDate : PX.Data.BQL.BqlDateTime.Field<lastPurchaseDate> { }
		protected DateTime? _LastPurchaseDate;
		[PXDBDate]
		[PXUIField(DisplayName = "Last Purchase Date")]
		public virtual DateTime? LastPurchaseDate
		{
			get
			{
				return this._LastPurchaseDate;
			}
			set
			{
				this._LastPurchaseDate = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
	}

	[Obsolete(Common.Messages.ClassIsObsolete)]
	[Serializable]
	[PXHidden]
	public partial class INItemStatsA : INItemStats
	{
		#region InventoryID
		public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		#endregion
		#region SiteID
		public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		#endregion
		#region QtyOnHand
		public new abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
		#endregion
		#region TotalCost
		public new abstract class totalCost : PX.Data.BQL.BqlDecimal.Field<totalCost> { }
		#endregion
		#region MinCost
		public new abstract class minCost : PX.Data.BQL.BqlDecimal.Field<minCost> { }
		#endregion
		#region MaxCost
		public new abstract class maxCost : PX.Data.BQL.BqlDecimal.Field<maxCost> { }
		#endregion
		#region LastCost
		public new abstract class lastCost : PX.Data.BQL.BqlDecimal.Field<lastCost> { }
		#endregion
		#region LastCostDate
		public new abstract class lastCostDate : PX.Data.BQL.BqlDateTime.Field<lastCostDate> { }
		#endregion
		#region ValMethod
		public new abstract class valMethod : PX.Data.BQL.BqlString.Field<valMethod> { }
		#endregion
		#region tstamp
		public new abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
		#endregion
	}
}
