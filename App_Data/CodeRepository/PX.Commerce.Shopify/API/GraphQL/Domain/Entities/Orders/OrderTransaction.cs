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
using Newtonsoft.Json.Linq;
using PX.Commerce.Core;
using System;
using System.Collections.Generic;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// A payment transaction in the context of an order.
	/// </summary>
	[JsonObject(Description = "Order Transaction")]
	[CommerceDescription(ShopifyCaptions.OrdersTransaction, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class OrderTransactionGQL: BCAPIEntity, INode
	{
		/// <summary>
		/// The masked account number associated with the payment method.
		/// </summary>
		[JsonProperty("accountNumber")]
		[GraphQLField("accountNumber", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string AccountNumber { get; set; }

		/// <summary>
		/// The amount and currency of the transaction in shop and presentment currencies.
		/// </summary>
		[JsonProperty("amountSet")]
		[GraphQLField("amountSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag AmountSet { get; set; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.Amount, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public decimal? AmountPresentment { get => AmountSet?.PresentmentMoney?.Amount; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.Currency, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		public string CurrencyPresentment { get => AmountSet?.PresentmentMoney?.CurrencyCode; }

		/// <summary>
		/// Authorization code associated with the transaction.
		/// </summary>
		[JsonProperty("authorizationCode")]
		[GraphQLField("authorizationCode", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.Authorization, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public string AuthorizationCode { get; set; }

		/// <summary>
		/// The time when the authorization expires. This field is available only to stores on a Shopify Plus plan and is populated only for Shopify Payments authorizations.
		/// </summary>
		[JsonProperty("authorizationExpiresAt")]
		[GraphQLField("authorizationExpiresAt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.DateTime)]
		public DateTime? AuthorizationExpiresAt { get; set; }

		/// <summary>
		/// Date and time when the transaction was created.
		/// </summary>
		[JsonProperty("createdAt")]
		[GraphQLField("createdAt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.DateTime)]
		[CommerceDescription(ShopifyCaptions.DateCreated, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		public DateTime? DateCreatedAt { get; set; }

		/// <summary>
		/// A standardized error code, independent of the payment provider.
		/// </summary>
		[JsonProperty("errorCode")]
		[GraphQLField("errorCode", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.ErrorCode, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		public string ErrorCode { get; set; }

		/// <summary>
		/// The transaction fees charged on the order transaction. Only present for Shopify Payments transactions.
		/// </summary>
		[JsonProperty("fees")]
		[GraphQLField("fees", GraphQLConstants.DataType.Object, typeof(OrderTransactionFeeGQL))]
		public List<OrderTransactionFeeGQL> Fees { get; set; }

		/// <summary>
		/// The human-readable payment gateway name used to process the transaction.
		/// </summary>
		[JsonProperty("formattedGateway")]
		[GraphQLField("formattedGateway", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string FormattedGateway { get; set; }

		/// <summary>
		/// The payment gateway used to process the transaction.
		/// </summary>
		[JsonProperty("gateway")]
		[GraphQLField("gateway", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.Gateway, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		public string Gateway { get; set; }

		/// <summary>
		/// A globally-unique identifier.
		/// </summary>
		[JsonProperty("id")]
		[GraphQLField("id", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.Id, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		public string Id { get; set; }

		/// <summary>
		/// The kind of transaction.
		/// </summary>
		[JsonProperty("kind")]
		[GraphQLField("kind", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.Kind, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		public string Kind { get; set; }

		/// <summary>
		/// Whether the transaction can be manually captured.
		/// </summary>
		[JsonProperty("manuallyCapturable")]
		[GraphQLField("manuallyCapturable", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool? ManuallyCapturable { get; set; }

		/// <summary>
		/// Specifies the available amount with currency to refund on the gateway. This value is only available for transactions of type SuggestedRefund.
		/// </summary>
		[JsonProperty("maximumRefundableV2")]
		[GraphQLField("maximumRefundableV2", GraphQLConstants.DataType.Object, typeof(Money))]
		public Money MaximumRefundableV2 { get; set; }

		/// <summary>
		/// The associated order.
		/// </summary>
		[JsonProperty("order")]
		[GraphQLField("order", GraphQLConstants.DataType.Object, typeof(OrderDataGQL))]
		public OrderDataGQL Order { get; set; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.OrderId, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public string OrderId { get => Order?.Id; set => Order = new OrderDataGQL { Id = value }; }

		/// <summary>
		/// The associated parent transaction, for example the authorization of a capture.
		/// </summary>
		[JsonProperty("parentTransaction")]
		[GraphQLField("parentTransaction", GraphQLConstants.DataType.Object, typeof(OrderTransactionGQL))]
		public OrderTransactionGQL ParentTransaction { get; set; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.ParentId, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public string ParentId { get => ParentTransaction?.Id; set => ParentTransaction = new OrderTransactionGQL { Id = value }; }

		/// <summary>
		/// The payment details for the transaction.
		/// </summary>
		//[JsonProperty("paymentDetails")]
		//[GraphQLField("paymentDetails", GraphQLConstants.DataType.Object, typeof(object))]
		//public object PaymentDetails { get; set; }

		/// <summary>
		/// The payment ID associated with the transaction.
		/// </summary>
		[JsonProperty("paymentId")]
		[GraphQLField("paymentId", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string PaymentId { get; set; }

		/// <summary>
		/// Date and time when the transaction was processed.
		/// </summary>
		[JsonProperty("processedAt")]
		[GraphQLField("processedAt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.DateTime)]
		[CommerceDescription(ShopifyCaptions.ProcessedAt, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		public DateTime? ProcessedAt { get; set; }

		/// <summary>
		/// The transaction receipt that the payment gateway attaches to the transaction. The value of this field depends on which payment gateway processed the transaction.
		/// </summary>
		[JsonProperty("receiptJson")]
		[GraphQLField("receiptJson", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.Receipt, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public JToken ReceiptJson { get; set; }

		/// <summary>
		/// The settlement currency.
		/// </summary>
		[JsonProperty("settlementCurrency")]
		[GraphQLField("settlementCurrency", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string SettlementCurrency { get; set; }

		/// <summary>
		/// The rate used when converting the transaction amount to settlement currency.
		/// </summary>
		[JsonProperty("settlementCurrencyRate")]
		[GraphQLField("settlementCurrencyRate", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Decimal)]
		public decimal? SettlementCurrencyRate { get; set; }

		/// <summary>
		/// Contains all Shopify Payments information related to an order transaction. This field is available only to stores on a Shopify Plus plan.
		/// </summary>
		//[JsonProperty("shopifyPaymentsSet")]
		//[GraphQLField("shopifyPaymentsSet", GraphQLConstants.DataType.Object, typeof(object))]
		//public object ShopifyPaymentsSet { get; set; }

		/// <summary>
		/// The status of this transaction.
		/// </summary>
		[JsonProperty("status")]
		[GraphQLField("status", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.Status, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		public string Status { get; set; }

		/// <summary>
		/// Whether the transaction is a test transaction.
		/// </summary>
		[JsonProperty("test")]
		[GraphQLField("test", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		[CommerceDescription(ShopifyCaptions.TestTransaction, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		public bool? Test { get; set; }

		/// <summary>
		/// Specifies the available amount with currency to capture on the gateway in shop and presentment currencies. Only available when an amount is capturable or manually mark as paid.
		/// </summary>
		[JsonProperty("totalUnsettledSet")]
		[GraphQLField("totalUnsettledSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag TotalUnsettledSet { get; set; }

		[JsonIgnore]
		public decimal? TotalUnsettledPresentment { get => TotalUnsettledSet?.PresentmentMoney?.Amount; set => TotalUnsettledSet = new MoneyBag { PresentmentMoney = new Money { Amount = value } }; }

		[JsonIgnore]
		public string LastKind { get; set; }

		[JsonIgnore]
		[IncludeInHash]
		public string Action { get; set; }

		[JsonIgnore]
		public DateTime? DateModifiedAt { get => ProcessedAt; }

		/// <summary>
		/// Staff member who was logged into the Shopify POS device when the transaction was processed.
		/// </summary>
		//[JsonProperty("user")]
		//[GraphQLField("user", GraphQLConstants.DataType.Object, typeof(object))]
		//public object User { get; set; }
	}
}
