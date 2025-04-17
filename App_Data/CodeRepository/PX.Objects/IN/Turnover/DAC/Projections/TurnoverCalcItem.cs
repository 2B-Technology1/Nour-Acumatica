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
using PX.Objects.GL;

namespace PX.Objects.IN.Turnover
{
	[PXCacheName(Messages.INTurnoverCalcItem,
		PXDacType.Details)]
	[PXProjection(typeof(SelectFrom<INTurnoverCalcItem>
						.InnerJoin<InventoryItem>
							.On<INTurnoverCalcItem.FK.InventoryItem>
						.InnerJoin<INSite>
							.On<INTurnoverCalcItem.FK.Site>
						.Where<INTurnoverCalcItem.isVirtual.IsEqual<False>>), Persistent = false)]
	public class TurnoverCalcItem : IBqlTable
	{
		#region BranchID
		[PXDBInt(IsKey = true, BqlField = typeof(INTurnoverCalcItem.branchID))]
		public virtual int? BranchID { get; set; }
		public abstract class branchID : BqlInt.Field<branchID> { }
		#endregion

		#region FromPeriodID
		[FinPeriodID(IsKey = true, BqlField = typeof(INTurnoverCalcItem.fromPeriodID))]
		public virtual string FromPeriodID { get; set; }
		public abstract class fromPeriodID : BqlString.Field<fromPeriodID> { }
		#endregion

		#region ToPeriodID
		[FinPeriodID(IsKey = true, BqlField = typeof(INTurnoverCalcItem.toPeriodID))]
		public virtual string ToPeriodID { get; set; }
		public abstract class toPeriodID : BqlString.Field<toPeriodID> { }
		#endregion

		#region InventoryID
		[StockItem(BqlField = typeof(INTurnoverCalcItem.inventoryID))]
		public virtual int? InventoryID { get; set; }
		public abstract class inventoryID : BqlInt.Field<inventoryID> { }
		#endregion

		#region InventoryCD
		[PXDBString(IsUnicode = true, IsKey = true, BqlField = typeof(InventoryItem.inventoryCD))]
		public virtual string InventoryCD { get; set; }
		public abstract class inventoryCD : BqlString.Field<inventoryCD> { }
		#endregion

		#region ItemClassID
		[PXDBInt(BqlField = typeof(InventoryItem.itemClassID))]
		public virtual int? ItemClassID { get; set; }
		public abstract class itemClassID : BqlInt.Field<itemClassID> { }
		#endregion

		#region Description
		[PXDBLocalizableString(255, IsUnicode = true, BqlField = typeof(InventoryItem.descr), IsProjection = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual string Description { get; set; }
		public abstract class description : BqlString.Field<description> { }
		#endregion

		#region UOM
		[INUnit(BqlField = typeof(InventoryItem.baseUnit))]
		public virtual string UOM { get; set; }
		public abstract class uOM : BqlString.Field<uOM> { }
		#endregion

		#region SiteID
		[Site(BqlField = typeof(INTurnoverCalcItem.siteID))]
		public virtual int? SiteID { get; set; }
		public abstract class siteID : BqlInt.Field<siteID> { }
		#endregion

		#region SiteCD
		[PXDBString(30, IsUnicode = true, IsKey = true, BqlField = typeof(INSite.siteCD))]
		public virtual string SiteCD { get; set; }
		public abstract class siteCD : BqlString.Field<siteCD> { }
		#endregion

		#region BegQty
		[PXDBQuantity(BqlField = typeof(INTurnoverCalcItem.begQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Beginning Inventory (Units)")]
		public virtual decimal? BegQty { get; set; }
		public abstract class begQty : BqlDecimal.Field<begQty> { }
		#endregion
		#region BegCost
		[PXDBPriceCost(BqlField = typeof(INTurnoverCalcItem.begCost))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = Messages.BeginningInventory)]
		public virtual decimal? BegCost { get; set; }
		public abstract class begCost : BqlDecimal.Field<begCost> { }
		#endregion

		#region YtdQty
		[PXDBQuantity(BqlField = typeof(INTurnoverCalcItem.ytdQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Ending Inventory (Units)")]
		public virtual decimal? YtdQty { get; set; }
		public abstract class ytdQty : BqlDecimal.Field<ytdQty> { }
		#endregion
		#region YtdCost
		[PXDBPriceCost(BqlField = typeof(INTurnoverCalcItem.ytdCost))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = Messages.EndingInventory)]
		public virtual decimal? YtdCost { get; set; }
		public abstract class ytdCost : BqlDecimal.Field<ytdCost> { }
		#endregion

		#region AvgQty
		[PXDBQuantity(BqlField = typeof(INTurnoverCalcItem.avgQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Average  Inventory (Units)")]
		public virtual decimal? AvgQty { get; set; }
		public abstract class avgQty : BqlDecimal.Field<avgQty> { }
		#endregion
		#region AvgCost
		[PXDBPriceCost(BqlField = typeof(INTurnoverCalcItem.avgCost))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = Messages.AverageInventory)]
		public virtual decimal? AvgCost { get; set; }
		public abstract class avgCost : BqlDecimal.Field<avgCost> { }
		#endregion

		#region SoldQty
		[PXDBQuantity(BqlField = typeof(INTurnoverCalcItem.soldQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. of Items Sold")]
		public virtual decimal? SoldQty { get; set; }
		public abstract class soldQty : BqlDecimal.Field<soldQty> { }
		#endregion
		#region SoldCost
		[PXDBPriceCost(BqlField = typeof(INTurnoverCalcItem.soldCost))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = Messages.CostOfGoodsSold)]
		public virtual decimal? SoldCost { get; set; }
		public abstract class soldCost : BqlDecimal.Field<soldCost> { }
		#endregion

		#region QtyRatio
		[PXDBQuantity(BqlField = typeof(INTurnoverCalcItem.qtyRatio))]
		[PXUIField(DisplayName = "Turnover Ratio (Units)", Visible = false)]
		public virtual decimal? QtyRatio { get; set; }
		public abstract class qtyRatio : BqlDecimal.Field<qtyRatio> { }
		#endregion
		#region CostRatio
		[PXDBPriceCost(true, BqlField = typeof(INTurnoverCalcItem.costRatio))]
		[PXUIField(DisplayName = Messages.TurnoverRatio)]
		public virtual decimal? CostRatio { get; set; }
		public abstract class costRatio : BqlDecimal.Field<costRatio> { }
		#endregion

		#region QtySellDays
		[PXDBQuantity(BqlField = typeof(INTurnoverCalcItem.qtySellDays))]
		public virtual decimal? QtySellDays { get; set; }
		public abstract class qtySellDays : BqlDecimal.Field<qtySellDays> { }
		#endregion
		#region CostSellDays
		[PXDBQuantity(BqlField = typeof(INTurnoverCalcItem.costSellDays))]
		[PXUIField(DisplayName = "Days Sales of Inventory")]
		public virtual decimal? CostSellDays { get; set; }
		public abstract class costSellDays : BqlDecimal.Field<costSellDays> { }
		#endregion
	}
}
