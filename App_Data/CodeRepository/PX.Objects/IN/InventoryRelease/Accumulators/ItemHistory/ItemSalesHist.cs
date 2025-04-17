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

namespace PX.Objects.IN.InventoryRelease.Accumulators.ItemHistory
{
	[PXHidden]
	[Accumulator]
	[PXDisableCloneAttributes]
	public class ItemSalesHist : INItemSalesHist
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

		public class AccumulatorAttribute : PXAccumulatorAttribute
		{
			public AccumulatorAttribute() : base(
				Run<finYtdSales>().WithOwnValue(),
				Run<finYtdCreditMemos>().WithOwnValue(),
				Run<finYtdDropShipSales>().WithOwnValue(),
				Run<finYtdCOGS>().WithOwnValue(),
				Run<finYtdCOGSCredits>().WithOwnValue(),
				Run<finYtdCOGSDropShips>().WithOwnValue(),
				Run<finYtdQtySales>().WithOwnValue(),
				Run<finYtdQtyCreditMemos>().WithOwnValue(),
				Run<finYtdQtyDropShipSales>().WithOwnValue(),

				Run<tranYtdSales>().WithOwnValue(),
				Run<tranYtdCreditMemos>().WithOwnValue(),
				Run<tranYtdDropShipSales>().WithOwnValue(),
				Run<tranYtdCOGS>().WithOwnValue(),
				Run<tranYtdCOGSCredits>().WithOwnValue(),
				Run<tranYtdCOGSDropShips>().WithOwnValue(),
				Run<tranYtdQtySales>().WithOwnValue(),
				Run<tranYtdQtyCreditMemos>().WithOwnValue(),
				Run<tranYtdQtyDropShipSales>().WithOwnValue())
			{ }

			protected override bool PrepareInsert(PXCache cache, object row, PXAccumulatorCollection columns)
			{
				if (!base.PrepareInsert(cache, row, columns))
					return false;

				ItemSalesHist hist = (ItemSalesHist)row;

				columns.RestrictPast<finPeriodID>(PXComp.GE, hist.FinPeriodID.Substring(0, 4) + "01");
				columns.RestrictFuture<finPeriodID>(PXComp.LE, hist.FinPeriodID.Substring(0, 4) + "99");

				return true;
			}
		}
	}
}
