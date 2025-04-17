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

namespace PX.Objects.Common
{
	public struct FullBalanceDelta
	{
		/// <summary>
		/// The unsigned amount (in adjusted document currency)
		/// on which the adjusted document balance is affected
		/// and which is not induced by a symmetrical change
		/// in the adjusting document balance. For example,
		/// it may include RGOL, write-offs, cash discounts etc.
		/// </summary>
		public decimal CurrencyAdjustedExtraAmount;
		/// <summary>
		/// The unsigned amount (in adjusting document currency)
		/// on which the adjusted document balance is affected
		/// and which is not induced by a symmetrical change
		/// in the adjusting document balance. For example,
		/// it may include RGOL, write-offs, cash discounts etc.
		/// </summary>
		public decimal CurrencyAdjustingExtraAmount;
		/// <summary>
		/// The unsigned amount (in base currency)
		/// on which the adjusted document balance is affected
		/// and which is not induced by a symmetrical change
		/// in the adjusting document balance. For example,
		/// it may include RGOL, write-offs, cash discounts etc.
		/// </summary>
		public decimal BaseAdjustedExtraAmount;
		/// <summary>
		/// The full unsigned amount (in document currency) 
		/// on which the adjusted document balance is affected
		/// by the application.
		/// </summary>
		public decimal CurrencyAdjustedBalanceDelta;
		/// <summary>
		/// The full unsigned amount (in document currency)
		/// on which the adjusting document balance is affected
		/// by the application.
		/// </summary>
		public decimal CurrencyAdjustingBalanceDelta;
		/// <summary>
		/// The full unsigned amount (in base currency)
		/// on which the adjusted document balance is affected
		/// by the application.
		/// </summary>
		public decimal BaseAdjustedBalanceDelta;
		/// <summary>
		/// The full unsigned amount (in base currency)
		/// on which the adjusting document balance is affected
		/// by the application.
		/// </summary>
		public decimal BaseAdjustingBalanceDelta;
	}
}
