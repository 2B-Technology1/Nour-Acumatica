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
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using INItemCostHistLast = PX.Objects.IN.InventoryRelease.Accumulators.ItemHistory.ItemCostHist;

namespace PX.Objects.IN.Turnover
{
	[PXHidden]
	[PXProjection(typeof(
		SelectFrom<InventoryItem>
		.CrossJoin<INSite>
		.CrossJoin<MasterFinPeriod>
		.InnerJoin<INItemCostHistRange>
			.On<INItemCostHistRange.inventoryID.IsEqual<InventoryItem.inventoryID>
			.And<INItemCostHistRange.siteID.IsEqual<INSite.siteID>>>
		.LeftJoin<INItemCostHist>
			.On<INItemCostHist.inventoryID.IsEqual<INItemCostHistRange.inventoryID>
			.And<INItemCostHist.costSubItemID.IsEqual<INItemCostHistRange.costSubItemID>>
			.And<INItemCostHist.costSiteID.IsEqual<INItemCostHistRange.costSiteID>>
			.And<INItemCostHist.accountID.IsEqual<INItemCostHistRange.accountID>>
			.And<INItemCostHist.subID.IsEqual<INItemCostHistRange.subID>>
			.And<INItemCostHist.finPeriodID.IsEqual<MasterFinPeriod.finPeriodID>>>
		.LeftJoin<INItemCostHistLastActivePeriod>
			.On<MasterFinPeriod.finPeriodID.IsEqual<INTurnoverCalc.fromPeriodID.FromCurrent.Value>
			.And<INItemCostHist.finPeriodID.IsNull>
			.And<INItemCostHistLastActivePeriod.inventoryID.IsEqual<INItemCostHistRange.inventoryID>>
			.And<INItemCostHistLastActivePeriod.costSubItemID.IsEqual<INItemCostHistRange.costSubItemID>>
			.And<INItemCostHistLastActivePeriod.costSiteID.IsEqual<INItemCostHistRange.costSiteID>>
			.And<INItemCostHistLastActivePeriod.accountID.IsEqual<INItemCostHistRange.accountID>>
			.And<INItemCostHistLastActivePeriod.subID.IsEqual<INItemCostHistRange.subID>>>
		.LeftJoin<INItemCostHistLast>
			.On<INItemCostHistLast.inventoryID.IsEqual<INItemCostHistRange.inventoryID>
			.And<INItemCostHistLast.costSubItemID.IsEqual<INItemCostHistRange.costSubItemID>>
			.And<INItemCostHistLast.costSiteID.IsEqual<INItemCostHistRange.costSiteID>>
			.And<INItemCostHistLast.accountID.IsEqual<INItemCostHistRange.accountID>>
			.And<INItemCostHistLast.subID.IsEqual<INItemCostHistRange.subID>>
			.And<INItemCostHistLast.finPeriodID.IsEqual<INItemCostHistLastActivePeriod.lastActiveFinPeriodID>>>
		), Persistent = false)]
	public class INTurnoverCalcItemHist : IBqlTable
	{
		#region InventoryID
		[PXDBInt(IsKey = true, BqlField = typeof(InventoryItem.inventoryID))]
		public virtual int? InventoryID { get; set; }
		public abstract class inventoryID : BqlInt.Field<inventoryID> { }
		#endregion
		#region CostSubItemID
		[SubItem(IsKey = true, BqlField = typeof(INItemCostHistRange.costSubItemID))]
		public virtual int? CostSubItemID { get; set; }
		public abstract class costSubItemID : BqlInt.Field<costSubItemID> { }
		#endregion
		#region CostSiteID
		[Site(IsKey = true, BqlField = typeof(INItemCostHistRange.costSiteID))]
		public virtual int? CostSiteID { get; set; }
		public abstract class costSiteID : BqlInt.Field<costSiteID> { }
		#endregion
		#region AccountID
		[PXDBInt(IsKey = true, BqlField = typeof(INItemCostHistRange.accountID))]
		public virtual int? AccountID { get; set; }
		public abstract class accountID : BqlInt.Field<accountID> { }
		#endregion
		#region SubID
		[PXDBInt(IsKey = true, BqlField = typeof(INItemCostHistRange.subID))]
		public virtual int? SubID { get; set; }
		public abstract class subID : BqlInt.Field<subID> { }
		#endregion
		#region FinPeriodID
		[FinPeriodID(IsKey = true, BqlField = typeof(MasterFinPeriod.finPeriodID))]
		public virtual string FinPeriodID { get; set; }
		public abstract class finPeriodID : BqlString.Field<finPeriodID> { }
		#endregion

		#region BranchID
		[PXDBInt(BqlField = typeof(INSite.branchID))]
		public virtual int? BranchID { get; set; }
		public abstract class branchID : BqlInt.Field<branchID> { }
		#endregion
		#region SiteID
		[PXDBInt(IsKey = true, BqlField = typeof(INSite.siteID))]
		public virtual int? SiteID { get; set; }
		public abstract class siteID : BqlInt.Field<siteID> { }
		#endregion

		#region INItemCostHist fields

		#region CostHistFinPeriodID
		[FinPeriodID(IsKey = true, BqlField = typeof(INItemCostHist.finPeriodID))]
		public virtual string CostHistFinPeriodID { get; set; }
		public abstract class costHistFinPeriodID : BqlString.Field<costHistFinPeriodID> { }
		#endregion

		#region FinBegQty
		[PXDBQuantity(BqlField = typeof(INItemCostHist.finBegQty))]
		public virtual decimal? FinBegQty { get; set; }
		public abstract class finBegQty : BqlDecimal.Field<finBegQty> { }
		#endregion

		#region FinYtdQty
		[PXDBQuantity(BqlField = typeof(INItemCostHist.finYtdQty))]
		public virtual decimal? FinYtdQty { get; set; }
		public abstract class finYtdQty : BqlDecimal.Field<finYtdQty> { }
		#endregion

		#region FinBegCost
		[PXDBDecimal(4, BqlField = typeof(INItemCostHist.finBegCost))]
		public virtual decimal? FinBegCost { get; set; }
		public abstract class finBegCost : BqlDecimal.Field<finBegCost> { }
		#endregion

		#region FinYtdCost
		[PXDBDecimal(4, BqlField = typeof(INItemCostHist.finYtdCost))]
		public virtual decimal? FinYtdCost { get; set; }
		public abstract class finYtdCost : BqlDecimal.Field<finYtdCost> { }
		#endregion

		#region COGS fields
		#region FinPtdCOGS
		[PXDBDecimal(4, BqlField = typeof(INItemCostHist.finPtdCOGS))]
		public virtual decimal? FinPtdCOGS { get; set; }
		public abstract class finPtdCOGS : BqlDecimal.Field<finPtdCOGS> { }
		#endregion

		#region FinPtdQtySales
		[PXDBQuantity(BqlField = typeof(INItemCostHist.finPtdQtySales))]
		public virtual decimal? FinPtdQtySales { get; set; }
		public abstract class finPtdQtySales : BqlDecimal.Field<finPtdQtySales> { }
		#endregion

		#region FinPtdCOGSCredits
		[PXDBDecimal(4, BqlField = typeof(INItemCostHist.finPtdCOGSCredits))]
		public virtual decimal? FinPtdCOGSCredits { get; set; }
		public abstract class finPtdCOGSCredits : BqlDecimal.Field<finPtdCOGSCredits> { }
		#endregion

		#region FinPtdQtyCreditMemos
		[PXDBQuantity(BqlField = typeof(INItemCostHist.finPtdQtyCreditMemos))]
		public virtual decimal? FinPtdQtyCreditMemos { get; set; }
		public abstract class finPtdQtyCreditMemos : BqlDecimal.Field<finPtdQtyCreditMemos> { }
		#endregion
		#endregion

		#region Production fields
		#region FinPtdCostAMAssemblyOut
		[PXDBDecimal(4, BqlField = typeof(INItemCostHist.finPtdCostAMAssemblyOut))]
		public virtual decimal? FinPtdCostAMAssemblyOut { get; set; }
		public abstract class finPtdCostAMAssemblyOut : BqlDecimal.Field<finPtdCostAMAssemblyOut> { }
		#endregion

		#region FinPtdQtyAMAssemblyOut
		[PXDBQuantity(BqlField = typeof(INItemCostHist.finPtdQtyAMAssemblyOut))]
		public virtual decimal? FinPtdQtyAMAssemblyOut { get; set; }
		public abstract class finPtdQtyAMAssemblyOut : BqlDecimal.Field<finPtdQtyAMAssemblyOut> { }
		#endregion
		#endregion

		#region Assembly fields
		#region FinPtdCostAssemblyOut
		[PXDBDecimal(4, BqlField = typeof(INItemCostHist.finPtdCostAssemblyOut))]
		public virtual decimal? FinPtdCostAssemblyOut { get; set; }
		public abstract class finPtdCostAssemblyOut : BqlDecimal.Field<finPtdCostAssemblyOut> { }
		#endregion

		#region FinPtdQtyAssemblyOut
		[PXDBQuantity(BqlField = typeof(INItemCostHist.finPtdQtyAssemblyOut))]
		public virtual decimal? FinPtdQtyAssemblyOut { get; set; }
		public abstract class finPtdQtyAssemblyOut : BqlDecimal.Field<finPtdQtyAssemblyOut> { }
		#endregion
		#endregion

		#region Issues fields
		#region FinPtdCostIssued
		[PXDBDecimal(4, BqlField = typeof(INItemCostHist.finPtdCostIssued))]
		public virtual decimal? FinPtdCostIssued { get; set; }
		public abstract class finPtdCostIssued : BqlDecimal.Field<finPtdCostIssued> { }
		#endregion

		#region FinPtdQtyIssued
		[PXDBQuantity(BqlField = typeof(INItemCostHist.finPtdQtyIssued))]
		public virtual decimal? FinPtdQtyIssued { get; set; }
		public abstract class finPtdQtyIssued : BqlDecimal.Field<finPtdQtyIssued> { }
		#endregion
		#endregion

		#region Transfers fields
		#region FinPtdCostTransferOut
		[PXDBDecimal(4, BqlField = typeof(INItemCostHist.finPtdCostTransferOut))]
		public virtual decimal? FinPtdCostTransferOut { get; set; }
		public abstract class finPtdCostTransferOut : BqlDecimal.Field<finPtdCostTransferOut> { }
		#endregion

		#region FinPtdQtyTransferOut
		[PXDBQuantity(BqlField = typeof(INItemCostHist.finPtdQtyTransferOut))]
		public virtual decimal? FinPtdQtyTransferOut { get; set; }
		public abstract class finPtdQtyTransferOut : BqlDecimal.Field<finPtdQtyTransferOut> { }
		#endregion
		#endregion

		#endregion

		#region INItemCostHistLast fields

		#region LastActiveFinPeriodID
		[FinPeriodID(BqlField = typeof(INItemCostHistLast.finPeriodID))]
		public virtual string LastActiveFinPeriodID { get; set; }
		public abstract class lastActiveFinPeriodID : BqlString.Field<lastActiveFinPeriodID> { }
		#endregion

		#region LastFinYtdQty
		[PXDBQuantity(BqlField = typeof(INItemCostHistLast.finYtdQty))]
		public virtual decimal? LastFinYtdQty { get; set; }
		public abstract class lastFinYtdQty : BqlDecimal.Field<lastFinYtdQty> { }
		#endregion

		#region LastFinYtdCost
		[PXDBDecimal(4, BqlField = typeof(INItemCostHistLast.finYtdCost))]
		public virtual decimal? LastFinYtdCost { get; set; }
		public abstract class lastFinYtdCost : BqlDecimal.Field<lastFinYtdCost> { }
		#endregion

		#endregion

		public static implicit operator (int? CostSubItemID, int? CostSiteID, int? AccountID, int? SubID)(INTurnoverCalcItemHist h)
			=> (h.CostSubItemID, h.CostSiteID, h.AccountID, h.SubID);
	}
}
