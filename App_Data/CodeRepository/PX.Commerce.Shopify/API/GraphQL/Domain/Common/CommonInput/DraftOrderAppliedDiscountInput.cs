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

using Newtonsoft.Json;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// The input fields for applying an order-level discount to a draft order.
	/// </summary>
	public class DraftOrderAppliedDiscountInput
	{
		/// <summary>
		/// The applied amount of the discount. If the type of the discount is fixed amount, then this is the fixed dollar amount. If the type is percentage, then this is the subtotal multiplied by the percentage.
		/// </summary>
		[JsonProperty("amount", NullValueHandling = NullValueHandling.Ignore)]
		public decimal? Amount { get; set; }

		/// <summary>
		/// Reason for the discount.
		/// </summary>
		[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
		public string Description { get; set; }

		/// <summary>
		/// Title of the discount.
		/// </summary>
		[JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
		public string Title { get; set; }

		/// <summary>
		/// The value of the discount. If the type of the discount is fixed amount, then this is a fixed dollar amount. If the type is percentage, then this is the percentage.
		/// </summary>
		[JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
		public decimal? Value { get; set; }

		/// <summary>
		/// The type of discount.
		/// </summary>
		[JsonProperty("valueType", NullValueHandling = NullValueHandling.Ignore)]
		public DraftOrderAppliedDiscountType ValueType { get; set; }
	}
}
