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
using System.Collections.Generic;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// The input fields for providing the fields and values to use when creating or updating a fixed price list price.
	/// </summary>
	public class PriceListPriceInput
	{
		/// <summary>
		/// The compare-at price of the product variant on this price list.
		/// </summary>
		[JsonProperty("compareAtPrice", NullValueHandling = NullValueHandling.Ignore)]
		public MoneyInput CompareAtPrice { get; set; }

		/// <summary>
		/// The price of the product variant on this price list.
		/// </summary>
		[JsonProperty("price")]
		public MoneyInput Price { get; set; }

		/// <summary>
		/// The product variant ID associated with the price list price.
		/// </summary>
		[JsonProperty("variantId")]
		public string VariantId { get; set; }

	}
}
