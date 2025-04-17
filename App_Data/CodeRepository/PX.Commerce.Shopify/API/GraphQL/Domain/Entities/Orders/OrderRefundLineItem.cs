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
	/// A line item that's included in a refund.
	/// </summary>
	[JsonObject(Description = "Order Refund Line Item")]
	[CommerceDescription(ShopifyCaptions.RefundItem, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class OrderRefundLineItemGQL: BCAPIEntity, INode
	{
		/// <summary>
		/// The LineItem resource associated to the refunded line item.
		/// </summary>
		[JsonProperty("lineItem", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("lineItem", GraphQLConstants.DataType.Object, typeof(OrderLineItemGQL))]
		[CommerceDescription(ShopifyCaptions.LineItem, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public OrderLineItemGQL LineItem { get; set; }

		/// <summary>
		/// The price of a refunded line item in shop and presentment currencies.
		/// </summary>
		[JsonProperty("priceSet")]
		[GraphQLField("priceSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag PriceSet { get; set; }

		[JsonIgnore]
		public decimal? PricePresentment { get => PriceSet?.PresentmentMoney?.Amount; }

		/// <summary>
		/// The quantity of a refunded line item.
		/// </summary>
		[JsonProperty("quantity")]
		[GraphQLField("quantity", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Int)]
		[CommerceDescription(ShopifyCaptions.Quantity, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public int Quantity { get; set; }

		/// <summary>
		/// The type of restock for the refunded line item.
		/// </summary>
		[JsonProperty("restockType", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("restockType", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.RestockType, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public string RestockType { get; set; }

		/// <summary>
		/// Whether the refunded line item was restocked. Not applicable in the context of a SuggestedRefund.
		/// </summary>
		[JsonProperty("restocked")]
		[GraphQLField("restocked", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool Restocked { get; set; }

		/// <summary>
		/// The subtotal price of a refunded line item in shop and presentment currencies.
		/// </summary>
		[JsonProperty("subtotalSet")]
		[GraphQLField("subtotalSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag SubtotalSet { get; set; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.SubtotalPresentment, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public decimal? SubtotalPresentment { get => SubtotalSet?.PresentmentMoney?.Amount; }

		/// <summary>
		/// The total tax charged on a refunded line item in shop and presentment currencies.
		/// </summary>
		[JsonProperty("totalTaxSet")]
		[GraphQLField("totalTaxSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag TotalTaxSet { get; set; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.TotalTaxPresentment, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public decimal? TotalTaxPresentment { get => TotalTaxSet?.PresentmentMoney?.Amount; }

		/// <inheritdoc/>
		public string Id { get; set; }
	}
}
