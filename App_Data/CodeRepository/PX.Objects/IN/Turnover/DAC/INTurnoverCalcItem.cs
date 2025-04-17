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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.GL;

namespace PX.Objects.IN.Turnover
{
	[PXCacheName(Messages.INTurnoverCalcItem,
		PXDacType.Details)]
	public class INTurnoverCalcItem: IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INTurnoverCalcItem>.By<branchID, fromPeriodID, toPeriodID, inventoryID, siteID>
		{
			public static INTurnoverCalcItem Find(PXGraph graph, int? branchID, string fromPeriodID, string toPeriodID, int? inventoryID, int? siteID, PKFindOptions options = PKFindOptions.None)
				=> FindBy(graph, branchID, fromPeriodID, toPeriodID, inventoryID, siteID, options);
		}
		public static class FK
		{
			public class TurnoverCalc: INTurnoverCalc.PK.ForeignKeyOf<INTurnoverCalcItem>.By<branchID, fromPeriodID, toPeriodID> { }
			public class Branch : GL.Branch.PK.ForeignKeyOf<INTurnoverCalcItem>.By<branchID> { }
			public class Site : INSite.PK.ForeignKeyOf<INTurnoverCalcItem>.By<siteID> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INTurnoverCalcItem>.By<inventoryID> { }
		}
		#endregion

		#region BranchID
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public virtual int? BranchID { get; set; }
		public abstract class branchID : BqlInt.Field<branchID> { }
		#endregion

		#region FromPeriodID
		// Acuminator disable once PX1030 PXDefaultIncorrectUse [The field have PXDBStringAttribute]
		[FinPeriodID(IsKey = true)]
		[PXDefault]
		public virtual string FromPeriodID { get; set; }
		public abstract class fromPeriodID : BqlString.Field<fromPeriodID> { }
		#endregion

		#region ToPeriodID
		// Acuminator disable once PX1030 PXDefaultIncorrectUse [The field have PXDBStringAttribute]
		[FinPeriodID(IsKey = true)]
		[PXDefault]
		[PXParent(typeof(FK.TurnoverCalc))]
		public virtual string ToPeriodID { get; set; }
		public abstract class toPeriodID : BqlString.Field<toPeriodID> { }
		#endregion

		#region InventoryID
		[AnyInventory(typeof(SelectFrom<InventoryItem>
			.Where<Match<AccessInfo.userName.FromCurrent>
				.And<InventoryItem.stkItem.IsEqual<True>>>
			.SearchFor<InventoryItem.inventoryID>),
			typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr), IsKey = true)]
		[PXRestrictor(typeof(Where<InventoryItem.itemStatus.IsNotEqual<InventoryItemStatus.markedForDeletion>>),
			IN.Messages.InventoryItemIsInStatus, typeof(InventoryItem.itemStatus), ShowWarning = true)]
		public virtual int? InventoryID { get; set; }
		public abstract class inventoryID : BqlInt.Field<inventoryID> { }
		#endregion

		#region SiteID
		[Site(IsKey = true)]
		[PXRestrictor(typeof(Where<True,Equal<True>>), "", ReplaceInherited=true)]
		public virtual int? SiteID { get; set; }
		public abstract class siteID : BqlInt.Field<siteID> { }
		#endregion

		#region BegQty
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? BegQty { get; set; }
		public abstract class begQty : BqlDecimal.Field<begQty> { }
		#endregion
		#region BegCost
		[PXDBPriceCost]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? BegCost { get; set; }
		public abstract class begCost : BqlDecimal.Field<begCost> { }
		#endregion

		#region YtdQty
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? YtdQty { get; set; }
		public abstract class ytdQty : BqlDecimal.Field<ytdQty> { }
		#endregion
		#region YtdCost
		[PXDBPriceCost]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? YtdCost { get; set; }
		public abstract class ytdCost : BqlDecimal.Field<ytdCost> { }
		#endregion

		#region AvgQty
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? AvgQty { get; set; }
		public abstract class avgQty : BqlDecimal.Field<avgQty> { }
		#endregion
		#region AvgCost
		[PXDBPriceCost]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? AvgCost { get; set; }
		public abstract class avgCost : BqlDecimal.Field<avgCost> { }
		#endregion

		#region SoldQty
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? SoldQty { get; set; }
		public abstract class soldQty : BqlDecimal.Field<soldQty> { }
		#endregion
		#region SoldCost
		[PXDBPriceCost]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? SoldCost { get; set; }
		public abstract class soldCost : BqlDecimal.Field<soldCost> { }
		#endregion

		#region QtyRatio
		[PXDBQuantity]
		public virtual decimal? QtyRatio { get; set; }
		public abstract class qtyRatio : BqlDecimal.Field<qtyRatio> { }
		#endregion
		#region CostRatio
		[PXDBPriceCost(true)]
		public virtual decimal? CostRatio { get; set; }
		public abstract class costRatio : BqlDecimal.Field<costRatio> { }
		#endregion

		#region QtySellDays
		[PXDBQuantity]
		public virtual decimal? QtySellDays { get; set; }
		public abstract class qtySellDays : BqlDecimal.Field<qtySellDays> { }
		#endregion
		#region CostSellDays
		[PXDBQuantity]
		public virtual decimal? CostSellDays { get; set; }
		public abstract class costSellDays : BqlDecimal.Field<costSellDays> { }
		#endregion

		#region IsVirtual
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? IsVirtual { get; set; }
		public abstract class isVirtual : BqlBool.Field<isVirtual> { }
		#endregion

		#region System fields

		#region CreatedByID
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }
		public abstract class createdByID : BqlGuid.Field<createdByID> { }
		#endregion

		#region CreatedByScreenID
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID { get; set; }
		public abstract class createdByScreenID : BqlString.Field<createdByScreenID> { }
		#endregion

		#region CreatedDateTime
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime { get; set; }
		public abstract class createdDateTime : BqlDateTime.Field<createdDateTime> { }
		#endregion

		#region tstamp
		[PXDBTimestamp]
		public virtual byte[] tstamp { get; set; }
		public abstract class Tstamp : BqlByteArray.Field<Tstamp> { }
		#endregion

		#endregion
	}
}
