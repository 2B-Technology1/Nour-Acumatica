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

namespace PX.Objects.PO
{
	public interface IAPTranSource
	{
		Int32? BranchID { get; }
		Int32? ExpenseAcctID { get; }
		Int32? ExpenseSubID { get; }
		Int32? POAccrualAcctID { get; }
		Int32? POAccrualSubID { get; }
		String LineType { get; }
		Int32? SiteID { get; }
		bool? IsStockItem { get; }
		Int32? InventoryID { get; }
		bool? AccrueCost { get; }
		string OrigUOM { get; }
		String UOM { get; }
		Int64? CuryInfoID { get; }
		decimal? OrigQty { get; }
		decimal? BaseOrigQty { get; }
		decimal? BillQty { get; }
		decimal? BaseBillQty { get; }
		decimal? CuryBilledAmt { get; }
		decimal? BilledAmt { get; }
		decimal? CuryUnitCost { get; }
		decimal? UnitCost { get; }
		decimal? CuryDiscAmt { get; }
		decimal? DiscAmt { get; }
		decimal? DiscPct { get; }
		decimal? CuryRetainageAmt { get; }
		decimal? RetainageAmt { get; }
		decimal? RetainagePct { get; }
		decimal? CuryLineAmt { get; }
		decimal? LineAmt { get; }
		String TaxCategoryID { get; }
		String TranDesc { get; }
		String TaxID { get; }
		int? ProjectID { get; }
		int? TaskID { get; }
		string POAccrualType { get; }
		Guid? POAccrualRefNoteID { get; }
		int? POAccrualLineNbr { get; }
		string OrderType { get;  }
		string CompletePOLine { get; }
		decimal? CuryUnbilledAmt { get; }
		decimal? UnbilledAmt { get; }
		int? CostCodeID { get; }
		bool IsReturn { get; }
		bool IsPartiallyBilled { get; }
		bool? AllowEditUnitCost { get; }
		bool AggregateWithExistingTran { get; }

		String DiscountID { get; }

		String DiscountSequenceID { get; }

		decimal? GroupDiscountRate { get; }

		decimal? DocumentDiscountRate { get; }

		DateTime? DRTermStartDate { get; }

		DateTime? DRTermEndDate { get; }

		string DropshipExpenseRecording { get; }

		bool CompareReferenceKey(AP.APTran aTran);
		void SetReferenceKeyTo(AP.APTran aTran);

	}
}
