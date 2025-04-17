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
	/// The input fields used to create a line item for a draft order.
	/// </summary>
	public class OrderLineItemInput
	{
		/// <summary>
		/// Discount which will be applied to the line item.
		/// </summary>
		[JsonProperty("appliedDiscount", NullValueHandling = NullValueHandling.Ignore)]
		public DraftOrderAppliedDiscountInput AppliedDiscount { get; set; }

		/// <summary>
		/// Represents a generic custom attribute using a key value pair.
		/// </summary>
		[JsonProperty("customAttributes", NullValueHandling = NullValueHandling.Ignore)]
		public List<AttributeInput> CustomAttributes { get; set; }

		/// <summary>
		/// The price without any discounts applied. This value is ignored when variantId is provided.
		/// </summary>
		[JsonProperty("originalUnitPrice", NullValueHandling = NullValueHandling.Ignore)]
		public decimal? OriginalUnitPrice { get; set; }

		/// <summary>
		/// The number of products that were purchased.
		/// </summary>
		[JsonProperty("quantity")]
		public int Quantity { get; set; }

		/// <summary>
		/// Whether physical shipping is required. This value is ignored when variantId is provided.
		/// </summary>
		[JsonProperty("requiresShipping", NullValueHandling = NullValueHandling.Ignore)]
		public bool? RequiresShipping { get; set; }

		/// <summary>
		/// The SKU number of the item. This value is ignored when variantId is provided.
		/// </summary>
		[JsonProperty("sku", NullValueHandling = NullValueHandling.Ignore)]
		public string Sku { get; set; }

		/// <summary>
		/// Whether the item is taxable. This value is ignored when variantId is provided.
		/// </summary>
		[JsonProperty("taxable", NullValueHandling = NullValueHandling.Ignore)]
		public bool? Taxable { get; set; }

		/// <summary>
		/// Title of the item. Ignored when variantId is provided.
		/// </summary>
		[JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
		public string Title { get; set; }

		/// <summary>
		/// The ID of the product variant corresponding to the line item. Null if custom line item. Required if product variant line item.
		/// </summary>
		[JsonProperty("variantId", NullValueHandling = NullValueHandling.Ignore)]
		public string VariantId { get; set; }

		/// <summary>
		/// Specifies the weight unit and value inputs. This value is ignored when variantId is provided.
		/// </summary>
		[JsonProperty("weight", NullValueHandling = NullValueHandling.Ignore)]
		public WeightInput Weight { get; set; }
	}
}
