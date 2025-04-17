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
using PX.Objects.CS;

namespace PX.Objects.IN
{
	[PXProjection(typeof(Select2<InventoryItem,
		CrossJoin<INSite,
		LeftJoin<InventoryItemCurySettings, On<InventoryItemCurySettings.inventoryID, Equal<InventoryItem.inventoryID>, And<InventoryItemCurySettings.curyID, Equal<INSite.baseCuryID>>>,
		LeftJoin<INItemRep, On<INItemRep.inventoryID, Equal<InventoryItem.inventoryID>, And<INItemRep.curyID, Equal<INSite.baseCuryID>, And<INItemRep.replenishmentClassID, Equal<INSite.replenishmentClassID>>>>,
		LeftJoinSingleTable<INItemSite, On<INItemSite.inventoryID, Equal<InventoryItem.inventoryID>, And<INItemSite.siteID, Equal<INSite.siteID>>>,
		LeftJoin<INItemStats, On<INItemStats.FK.ItemSite>>>>>>>))]
	[Serializable]
	[PXHidden]
	public partial class INItemSiteSettings : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INItemSiteSettings>.By<inventoryID, siteID>
		{
			public static INItemSiteSettings Find(PXGraph graph, int? inventoryID, int? siteID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, inventoryID, siteID, options);
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[PXDBInt(IsKey = true, BqlField = typeof(InventoryItem.inventoryID))]
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
		#region DefaultSubItemID
		public abstract class defaultSubItemID : PX.Data.BQL.BqlInt.Field<defaultSubItemID> { }
		protected Int32? _DefaultSubItemID;
		[PXDBInt(BqlField = typeof(InventoryItem.defaultSubItemID))]
		public virtual Int32? DefaultSubItemID
		{
			get;
			set;
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[PXDBInt(IsKey = true, BqlField = typeof(INSite.siteID))]
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
		#region PreferredVendorID
		public abstract class preferredVendorID : PX.Data.BQL.BqlInt.Field<preferredVendorID> { }
		[PXDBCalced(typeof(IsNull<InventoryItemCurySettings.preferredVendorID, INItemSite.preferredVendorID>), typeof(Int32))]
		public virtual Int32? PreferredVendorID
		{
			get;
			set;
		}
		#endregion
		#region PreferredVendorLocationID
		public abstract class preferredVendorLocationID : PX.Data.BQL.BqlInt.Field<preferredVendorLocationID> { }
		[PXDBCalced(typeof(IsNull<InventoryItemCurySettings.preferredVendorLocationID, INItemSite.preferredVendorLocationID>), typeof(Int32))]
		public virtual Int32? PreferredVendorLocationID
		{
			get;
			set;
		}
		#endregion
		#region ReplenishmentSource
		public abstract class replenishmentSource : PX.Data.BQL.BqlString.Field<replenishmentSource> { }
		[PXDBCalced(typeof(IsNull<INItemSite.replenishmentSource, INItemRep.replenishmentSource>), typeof(string))]
		public virtual string ReplenishmentSource
		{
			get;
			set;
		}
		#endregion
		#region ReplenishmentSourceSiteID
		public abstract class replenishmentSourceSiteID : PX.Data.BQL.BqlInt.Field<replenishmentSourceSiteID> { }
		[PXDBCalced(typeof(IsNull<INItemSite.replenishmentSourceSiteID, INItemRep.replenishmentSourceSiteID>), typeof(Int32))]
		public virtual Int32? ReplenishmentSourceSiteID
		{
			get;
			set;
		}
		#endregion
		#region POCreate
		public abstract class pOCreate : PX.Data.BQL.BqlBool.Field<pOCreate> { }
		[PXDBCalced(
			typeof(Switch<Case<Where<IsNull<INItemSite.replenishmentSource, INItemRep.replenishmentSource>, Equal<INReplenishmentSource.purchaseToOrder>,
					Or<IsNull<INItemSite.replenishmentSource, INItemRep.replenishmentSource>, Equal<INReplenishmentSource.dropShipToOrder>>>, boolTrue>, boolFalse>), typeof(Boolean))]
		public virtual Boolean? POCreate
		{
			get;
			set;
		}
		#endregion
		#region POSource
		public abstract class pOSource : PX.Data.BQL.BqlString.Field<pOSource> { }
		[PXDBCalced(
			typeof(Switch<Case<Where<IsNull<INItemSite.replenishmentSource, INItemRep.replenishmentSource>, Equal<INReplenishmentSource.dropShipToOrder>>, INReplenishmentSource.dropShipToOrder,
						Case<Where<IsNull<INItemSite.replenishmentSource, INItemRep.replenishmentSource>, Equal<INReplenishmentSource.purchaseToOrder>>, INReplenishmentSource.purchaseToOrder>>,
						INReplenishmentSource.none>), typeof(string))]
		public virtual string POSource
		{
			get;
			set;
		}
		#endregion
		#region INItemSiteExists
		public abstract class iNItemSiteExists : PX.Data.BQL.BqlBool.Field<iNItemSiteExists> { }
		[PXDBCalced(
			typeof(Switch<Case<Where<INItemSite.inventoryID, IsNotNull>, True>, False>), typeof(bool))]
		public virtual bool? INItemSiteExists
		{
			get;
			set;
		}
		#endregion
		#region ValMethod
		public abstract class valMethod : Data.BQL.BqlString.Field<valMethod>
		{
		}
		[PXDBString(1, IsFixed = true, BqlField = typeof(InventoryItem.valMethod))]
		public virtual string ValMethod
		{
			get;
			set;
		}
		#endregion
		#region ABCCodeID
		public abstract class aBCCodeID : Data.BQL.BqlString.Field<aBCCodeID>
		{
		}
		[PXDBCalced(typeof(IsNull<INItemSite.aBCCodeID, InventoryItem.aBCCodeID>), typeof(string))]
		public virtual string ABCCodeID
		{
			get;
			set;
		}
		#endregion
		#region MovementClassID
		public abstract class movementClassID : Data.BQL.BqlString.Field<movementClassID>
		{
		}
		[PXDBCalced(typeof(IsNull<INItemSite.movementClassID, InventoryItem.movementClassID>), typeof(string))]
		public virtual string MovementClassID
		{
			get;
			set;
		}
		#endregion
		#region InvtAcctID
		public abstract class invtAcctID : Data.BQL.BqlInt.Field<invtAcctID>
		{
		}
		[PXDBCalced(typeof(IsNull<INItemSite.invtAcctID, InventoryItem.invtAcctID>), typeof(int))]
		public virtual int? InvtAcctID
		{
			get;
			set;
		}
		#endregion
		#region InvtSubID
		public abstract class invtSubID : Data.BQL.BqlInt.Field<invtSubID>
		{
		}
		[PXDBCalced(typeof(IsNull<INItemSite.invtSubID, InventoryItem.invtSubID>), typeof(int))]
		public virtual int? InvtSubID
		{
			get;
			set;
		}
		#endregion
		#region NegativeCost
		public abstract class negativeCost : PX.Data.BQL.BqlDecimal.Field<negativeCost>
		{
		}
		protected Decimal? _NegativeCost;
		[PXDBCalced(typeof(Switch<
				Case<Where<InventoryItem.valMethod, Equal<INValMethod.standard>>,
					IsNull<INItemSite.stdCost, InventoryItemCurySettings.stdCost>,
				Case<Where<InventoryItem.valMethod, Equal<INValMethod.average>,
						And<INItemStats.qtyOnHand, NotEqual<decimal0>,
						And<Div<INItemStats.totalCost, INItemStats.qtyOnHand>, Greater<decimal0>>>>,
					Div<INItemStats.totalCost, INItemStats.qtyOnHand>,
				Case<Where<INItemStats.lastCostDate, GreaterEqual<INItemStats.dateAfterMinDate>>,
					INItemStats.lastCost>>>,
				Null>),
			typeof(Decimal))]
		[PXDecimal]
		public virtual Decimal? NegativeCost
		{
			get
			{
				return this._NegativeCost;
			}
			set
			{
				this._NegativeCost = value;
			}
		}
		#endregion
	}
}
