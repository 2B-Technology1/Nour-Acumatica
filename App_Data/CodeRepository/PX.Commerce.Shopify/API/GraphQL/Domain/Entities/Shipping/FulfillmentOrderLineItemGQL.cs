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
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using PX.Commerce.Core;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// Associates an order line item with quantities requiring fulfillment from the respective fulfillment order.
	/// </summary>
	public class FulfillmentOrderLineItemGQL: BCAPIEntity, INode
	{
		/// <summary>
		/// A globally-unique identifier.
		/// </summary>
		[JsonProperty("id")]
		[GraphQLField("id", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.ID)]
		public string Id { get; set; }

		/// <summary>
		/// The ID of the inventory item.
		/// </summary>
		[JsonProperty("inventoryItemId")]
		[GraphQLField("inventoryItemId", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.ID)]
		public string InventoryItemId { get; set; }

		/// <summary>
		/// The title of the product.
		/// </summary>
		[JsonProperty("productTitle")]
		[GraphQLField("productTitle", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string ProductTitle { get; set; }

		/// <summary>
		/// The number of units remaining to be fulfilled.
		/// </summary>
		[JsonProperty("remainingQuantity")]
		[GraphQLField("remainingQuantity", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Int)]
		public int RemainingQuantity { get; set; }

		/// <summary>
		/// Whether physical shipping is required for the variant.
		/// </summary>
		[JsonProperty("requiresShipping")]
		[GraphQLField("requiresShipping", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool RequiresShipping { get; set; }

		/// <summary>
		/// The variant SKU number.
		/// </summary>
		[JsonProperty("sku")]
		[GraphQLField("sku", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Sku { get; set; }

		/// <summary>
		/// The total number of units to be fulfilled.
		/// </summary>
		[JsonProperty("totalQuantity")]
		[GraphQLField("totalQuantity", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Int)]
		public int TotalQuantity { get; set; }

		/// <summary>
		/// The name of the variant.
		/// </summary>
		[JsonProperty("variantTitle")]
		[GraphQLField("variantTitle", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string VariantTitle { get; set; }
	}
}
