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
	public interface IPOReturnLineSource
	{
		string ReceiptType { get; }
		string ReceiptNbr { get; }
		int? LineNbr { get; }
		string LineType { get; }
		string POType { get; }
		string PONbr { get; }
		int? POLineNbr { get; }
		bool? IsStockItem { get; }
		int? InventoryID { get; }
		bool? AccrueCost { get; }
		int? SubItemID { get; }
		int? SiteID { get; }
		int? LocationID { get; }
		string LotSerialNbr { get; }
		DateTime? ExpireDate { get; }
		string UOM { get; }
		decimal? ReceiptQty { get; }
		decimal? BaseReceiptQty { get; }
		decimal? ReturnedQty { get; set; }
		decimal? BaseReturnedQty { get; }
		Int64? CuryInfoID { get; }
		int? ExpenseAcctID { get; }
		int? ExpenseSubID { get; }
		int? POAccrualAcctID { get; }
		int? POAccrualSubID { get; }
		string TranDesc { get; }
		int? CostCodeID { get; }
		int? ProjectID { get; }
		int? TaskID { get; }
		bool? AllowEditUnitCost { get; }
		bool? ManualPrice { get; }
		decimal? DiscPct { get; }
		decimal? CuryDiscAmt { get; }
		decimal? DiscAmt { get; }
		decimal? UnitCost { get; }
		decimal? CuryUnitCost { get; }
		decimal? ExtCost { get; }
		decimal? CuryExtCost { get; }
		decimal? TranCostFinal { get; }
		decimal? TranCost { get; }
		decimal? CuryTranCost { get; }
		string DropshipExpenseRecording { get; }
		bool? IsSpecialOrder { get; }
		int? CostCenterID { get; }
	}
}
