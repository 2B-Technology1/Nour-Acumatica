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
using PX.Objects.GL;
using PX.Data.BQL;

namespace PX.Objects.IN.InventoryRelease.Accumulators.ItemHistory
{
	[PXHidden]
	[Accumulator]
	[PXDisableCloneAttributes]
	public class ItemCostHist : INItemCostHist
	{
		#region InventoryID
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public override int? InventoryID
		{
			get => _InventoryID;
			set => _InventoryID = value;
		}
		public new abstract class inventoryID : BqlInt.Field<inventoryID> { }
		#endregion
		#region CostSubItemID
		[SubItem(IsKey = true)]
		[PXDefault]
		public override int? CostSubItemID
		{
			get => _CostSubItemID;
			set => _CostSubItemID = value;
		}
		public new abstract class costSubItemID : BqlInt.Field<costSubItemID> { }
		#endregion
		#region CostSiteID
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public override int? CostSiteID
		{
			get => _CostSiteID;
			set => _CostSiteID = value;
		}
		public new abstract class costSiteID : BqlInt.Field<costSiteID> { }
		#endregion
		#region AccountID
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public override int? AccountID
		{
			get => _AccountID;
			set => _AccountID = value;
		}
		public new abstract class accountID : BqlInt.Field<accountID> { }
		#endregion
		#region SubID
		[SubAccount(IsKey = true)]
		[PXDefault]
		public override int? SubID
		{
			get => _SubID;
			set => _SubID = value;
		}
		public new abstract class subID : BqlInt.Field<subID> { }
		#endregion
		#region SiteID
		[PXDBInt]
		[PXDefault]
		public override int? SiteID
		{
			get => _SiteID;
			set => _SiteID = value;
		}
		public new abstract class siteID : BqlInt.Field<siteID> { }
		#endregion
		#region FinPeriodID
		[PXDBString(6, IsKey = true, IsFixed = true)]
		[PXDefault]
		public override string FinPeriodID
		{
			get => _FinPeriodID;
			set => _FinPeriodID = value;
		}
		public new abstract class finPeriodID : BqlString.Field<finPeriodID> { }
		#endregion
		#region FinYtdCost
		public new abstract class finYtdCost : BqlDecimal.Field<finYtdCost> { }
		#endregion
		#region FinYtdQty
		public new abstract class finYtdQty : BqlDecimal.Field<finYtdQty> { }
		#endregion

		public class AccumulatorAttribute : PXAccumulatorAttribute
		{
			public AccumulatorAttribute() : base(
				Run<finBegQty>().WithValueOf<finYtdQty>(),
				Run<finYtdQty>().WithOwnValue(),
				Run<finBegCost>().WithValueOf<finYtdCost>(),
				Run<finYtdCost>().WithOwnValue(),

				Run<tranBegQty>().WithValueOf<tranYtdQty>(),
				Run<tranYtdQty>().WithOwnValue(),
				Run<tranBegCost>().WithValueOf<tranYtdCost>(),
				Run<tranYtdCost>().WithOwnValue())
			{ }
		}
	}
}
