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
using PX.Commerce.Shopify.API.REST;
using System;
using System.Collections.Generic;

namespace PX.Commerce.Shopify.API.GraphQL
{
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class OrderRefundResponse : IEntityResponse<OrderRefundGQL>
	{
		[JsonProperty("refund")]
		public OrderRefundGQL TEntityData { get; set; }
	}

	[GraphQLObject(NodeName = "refund")]
	[JsonObject(Description = "Order Refund")]
	[CommerceDescription(ShopifyCaptions.Refund, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class OrderRefundGQL : BCAPIEntity, INode
	{
		/// <summary>
		/// The date and time when the draft order was created in Shopify.
		/// </summary>
		[JsonProperty("createdAt")]
		[GraphQLField("createdAt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.DateTime)]
		[CommerceDescription(ShopifyCaptions.DateCreated, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public DateTime? DateCreatedAt { get; set; }

		/// <summary>
		/// A globally-unique identifier.
		/// </summary>
		[JsonProperty("id")]
		[GraphQLField("id", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.ID)]
		public string Id { get; set; }

		/// <summary>
		/// The ID of the corresponding resource in the REST Admin API.
		/// </summary>
		[JsonProperty("legacyResourceId")]
		[GraphQLField("legacyResourceId", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Int)]
		[CommerceDescription(ShopifyCaptions.Id, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public long? LegacyResourceId { get; set; }

		/// <summary>
		/// The order adjustments that are attached with the refund.
		/// </summary>
		[JsonProperty("orderAdjustments")]
		[GraphQLField("orderAdjustments", GraphQLConstants.DataType.Connection, typeof(OrderAdjustmentGQL))]
		public Connection<OrderAdjustmentGQL> OrderAdjustments { get; set; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.OrderAdjustment, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public IEnumerable<OrderAdjustmentGQL> OrderAdjustmentsList { get => OrderAdjustments?.Nodes; }

		/// <summary>
		/// The list of refunded line Items.
		/// </summary>
		[JsonProperty("refundLineItems")]
		[GraphQLField("refundLineItems", GraphQLConstants.DataType.Connection, typeof(OrderRefundLineItemGQL))]
		public Connection<OrderRefundLineItemGQL> RefundLineItems { get; set; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.RefundItem, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public IEnumerable<OrderRefundLineItemGQL> RefundLineItemsList { get => RefundLineItems?.Nodes; }

		/// <summary>
		/// The text of an optional note that a shop owner can attach to the draft order.
		/// </summary>
		[JsonProperty("note", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("note", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.Note, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public string Note { get; set; }

		/// <summary>
		/// The total amount across all transactions for the refund, in shop and presentment currencies.
		/// </summary>
		[JsonProperty("totalRefundedSet", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("totalRefundedSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag TotalRefundedSet { get; set; }

		[JsonIgnore]
		public decimal? TotalRefundedPresentment { get => TotalRefundedSet?.PresentmentMoney?.Amount; }

		/// <summary>
		/// The transactions associated with the refund.
		/// </summary>
		[JsonProperty("transactions")]
		[GraphQLField("transactions", GraphQLConstants.DataType.Connection, typeof(OrderTransactionGQL))]
		public Connection<OrderTransactionGQL> Transactions { get; set; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.Transactions, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public IEnumerable<OrderTransactionGQL> TransactionsList { get => Transactions?.Nodes; }

		/// <summary>
		/// The date and time when the draft order was last changed. The format is YYYY-MM-DD HH:mm:ss. For example, 2016-02-05 17:04:01.
		/// </summary>
		[JsonProperty("updatedAt", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("updatedAt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.DateTime)]
		[CommerceDescription(ShopifyCaptions.DateModified, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public DateTime? UpdatedAt { get; set; }

		[JsonIgnore]
		public Guid? LocalID { get; set; }
	}
}
