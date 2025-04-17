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
using PX.Commerce.Core;
using System.Collections.Generic;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// Represents individual products and quantities purchased in the associated draft order.
	/// </summary>
	public class DraftOrderLineItemDataGQL : BCAPIEntity, INode
	{
		/// <summary>
		/// A globally-unique identifier.
		/// </summary>
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("id", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.ID)]
		public string Id { get; set; }

		/// <summary>
		/// The line item price after discounts are applied.
		/// </summary>
		[JsonProperty("discountedTotal")]
		[GraphQLField("discountedTotal", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Money)]
		public decimal? DiscountedTotal { get; set; }

		/// <summary>
		/// The discountedTotal divided by quantity, resulting in the value of the discount per unit.
		/// </summary>
		[JsonProperty("discountedUnitPrice")]
		[GraphQLField("discountedUnitPrice", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Money)]
		public decimal? DiscountedUnitPrice { get; set; }

		/// <summary>
		/// The name of the product.
		/// </summary>
		[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("name", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Name { get; set; }

		/// <summary>
		/// The number of products that were purchased.
		/// </summary>
		[JsonProperty("quantity")]
		[GraphQLField("quantity", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Int)]
		public int Quantity { get; set; }

		/// <summary>
		/// The total price (without discounts) of the line item, based on the original unit price of the variant x quantity.
		/// </summary>
		[JsonProperty("originalTotal")]
		[GraphQLField("originalTotal", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Money)]
		public decimal? OriginalTotal { get; set; }

		/// <summary>
		/// The variant price without any discounts applied.
		/// </summary>
		[JsonProperty("originalUnitPrice")]
		[GraphQLField("originalUnitPrice", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Money)]
		public decimal? OriginalUnitPrice { get; set; }

		/// <summary>
		/// The SKU number of the product variant.
		/// </summary>
		[JsonProperty("sku", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("sku", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Sku { get; set; }

		/// <summary>
		/// Title of the item. Ignored when variantId is provided.
		/// </summary>
		[JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("title", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Title { get; set; }

		/// <summary>
		/// Thethe product variant
		/// </summary>
		[JsonProperty("variant", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("variant", GraphQLConstants.DataType.Object, typeof(ProductVariantGQL))]
		public ProductVariantGQL Variant { get; set; }

		/// <summary>
		/// The Product object associated with this line item's variant.
		/// </summary>
		[JsonProperty("product", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("product", GraphQLConstants.DataType.Object, typeof(ProductDataGQL))]
		public ProductDataGQL Product { get; set; }
	}
}
