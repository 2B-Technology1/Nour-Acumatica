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

namespace PX.Objects.Common.Discount
{
	public interface IDiscountDetail
	{
		int? RecordID { get; set; }
		ushort? LineNbr { get; set; }
		bool? SkipDiscount { get; set; }
		string DiscountID { get; set; }
		string DiscountSequenceID { get; set; }
		string Type { get; set; }
		decimal? CuryDiscountableAmt { get; set; }
		decimal? DiscountableQty { get; set; }
		decimal? CuryDiscountAmt { get; set; }
		decimal? DiscountPct { get; set; }
		int? FreeItemID { get; set; }
		decimal? FreeItemQty { get; set; }
		bool? IsManual { get; set; }
		bool? IsOrigDocDiscount { get; set; }
		string ExtDiscCode { get; set; }
		string Description { get; set; }
	}
}