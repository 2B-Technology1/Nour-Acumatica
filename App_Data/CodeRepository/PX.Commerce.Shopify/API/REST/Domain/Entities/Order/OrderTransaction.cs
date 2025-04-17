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
using Newtonsoft.Json;
using PX.Commerce.Core;
using PX.Commerce.Shopify.API.GraphQL;

namespace PX.Commerce.Shopify.API.REST
{
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class OrderTransactionResponse : IEntityResponse<OrderTransaction>
	{
		[JsonProperty("transaction")]
		public OrderTransaction Data { get; set; }
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class OrderTransactionsResponse : IEntitiesResponse<OrderTransaction>
	{
		[JsonProperty("transactions")]
		public IEnumerable<OrderTransaction> Data { get; set; }
	}

	[JsonObject(Description = "Order Transaction")]
	[CommerceDescription(ShopifyCaptions.OrdersTransaction, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class OrderTransaction : BCAPIEntity
	{
        private DateTime? _dateModifiedAt;
        /// <summary>
        /// The amount of money included in the transaction. If you don't provide a value for `amount`, then it defaults to the total cost of the order (even if a previous transaction has been made towards it).
        /// </summary>
        [JsonProperty("amount", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Amount, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public decimal? Amount { get; set; }

		/// <summary>
		/// The authorization code associated with the transaction.
		/// </summary>
		[JsonProperty("authorization", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Authorization, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public String Authorization { get; set; }

		/// <summary>
		/// [READ-ONLY] The date and time (ISO 8601 format) when the transaction was created.
		/// </summary>
		[JsonProperty("created_at")]
		[CommerceDescription(ShopifyCaptions.DateCreated, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		[ShouldNotSerialize]
		public DateTime? DateCreatedAt { get; set; }

        [ShouldNotSerialize]
        public DateTime? DateModifiedAt { get { return _dateModifiedAt == null ? DateCreatedAt : _dateModifiedAt; } set { _dateModifiedAt = value; } }

        /// <summary>
        /// The three-letter code (ISO 4217 format) for the currency used for the payment.
        /// </summary>
        [JsonProperty("currency", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Currency, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		public string Currency { get; set; }

		/// <summary>
		///  [READ-ONLY] The ID for the device.
		/// </summary>
		[JsonProperty("device_id", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.DeviceId, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		[ShouldNotSerialize]
		public long? DeviceId { get; set; }

		/// <summary>
		/// [READ-ONLY] A standardized error code, independent of the payment provider. 
		/// </summary>
		[JsonProperty("error_code")]
		[CommerceDescription(ShopifyCaptions.ErrorCode, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		[ShouldNotSerialize]
		public string ErrorCode { get; set; }

		/// <summary>
		/// The name of the gateway the transaction was issued through. A list of gateways can be found on Shopify's payment gateways page.
		/// </summary>
		[JsonProperty("gateway", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Gateway, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		public string Gateway { get; set; }

		/// <summary>
		///  [READ-ONLY] The ID for the transaction.
		/// </summary>
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Id, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		public long? Id { get; set; }

		/// <summary>
		/// The transaction's type.
		/// Usually a <see cref="TransactionType"/>
		/// </summary>
		[JsonProperty("kind")]
		[CommerceDescription(ShopifyCaptions.Kind, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		[ValidateRequired(AutoDefault = false)]
		public string Kind { get; set; }
		[JsonIgnore]
		public string LastKind { get; set; }

		[IncludeInHash]
		public string Action { get; set; }

		/// <summary>
		///  [READ-ONLY] The ID of the physical location where the transaction was processed.
		/// </summary>
		[JsonProperty("location_id")]
		[CommerceDescription(ShopifyCaptions.LocationId, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public long? LocationId { get; set; }

		/// <summary>
		/// [READ-ONLY] A string generated by the payment provider with additional information about why the transaction succeeded or failed. 
		/// </summary>
		[JsonProperty("message")]
		[CommerceDescription(ShopifyCaptions.Message, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		[ShouldNotSerialize]
		public string Message { get; set; }

		/// <summary>
		///  The ID for the order that the transaction is associated with.
		/// </summary>
		[JsonProperty("order_id", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.OrderId, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public long? OrderId { get; set; }

		/// <summary>
		/// Information about the credit card used for this transaction.
		/// </summary>
		[JsonProperty("payment_details")]
		[CommerceDescription(ShopifyCaptions.PaymentDetail, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		[ShouldNotSerialize]
		public PaymentDetail PaymentDetail { get; set; }
			
		/// <summary>
		///  The ID of an associated transaction.
		///  For capture transactions, the parent needs to be an authorization transaction.
		///  For void transactions, the parent needs to be an authorization transaction.
		///  For refund transactions, the parent needs to be a capture or sale transaction.
		/// </summary>
		[JsonProperty("parent_id", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.ParentId, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public long? ParentId { get; set; }

		/// <summary>
		/// This unique ID is now sent to payment providers when a customer pays at checkout.
		/// This ID can be used to match order information between Shopify and payment providers.
		/// An Order can have more than one Payment ID.
		/// It only includes successful or pending payments.
		/// It does not include captures and refunds.
		/// </summary>
		[JsonProperty("payment_id")]
		public string PaymentID { get; set; }

		[JsonProperty("payments_refund_attributes", NullValueHandling = NullValueHandling.Ignore)]
		public PaymentsRefundAttribute PaymentRefundAttribute { get; set; }

		/// <summary>
		/// The date and time (ISO 8601 format) when a transaction was processed. 
		/// This value is the date that's used in the analytic reports. By default, it matches the created_at value. 
		/// If you're importing transactions from an app or another platform, then you can set processed_at to a date and time in the past to match when the original transaction was processed.
		/// </summary>
		[JsonProperty("processed_at", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.ProcessedAt, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		public DateTime? ProcessedAt { get; set; }

		/// <summary>
		/// A transaction receipt attached to the transaction by the gateway. The value of this field depends on which gateway the shop is using.
		/// </summary>
		[JsonProperty("receipt")]
		[CommerceDescription(ShopifyCaptions.Receipt, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		[ShouldNotSerialize]
		public object Receipt { get; set; }

		/// <summary>
		/// [READ-ONLY] The origin of the transaction. This is set by Shopify and can't be overridden. Example values: web, pos, iphone, and android.
		/// </summary>
		[JsonProperty("source_name")]
		[CommerceDescription(ShopifyCaptions.SourceName, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		[ShouldNotSerialize]
		public string SourceName { get; set; }

		/// <summary>
		/// The status of the transaction. Valid values: pending, failure, success, and error.
		/// </summary>
		[JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Status, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		public TransactionStatus? Status { get; set; }

		/// <summary>
		/// Represents the remaining amount to be captured on the transaction in both shop and presentment money objects with currency.
		/// Specifies the available amount with currency to capture on the gateway in shop and presentment currencies.
		/// Only available when an amount is capturable or manually mark as paid.
		/// </summary>
		///<remarks>It is read-only in Shopify.</remarks>
		[JsonProperty("total_unsettled_set")]
		[ShouldNotSerialize]
		public AmountToCaptureAttribute AmountToCapture { get; set; }

		/// <summary>
		/// Whether the transaction is a test transaction.
		/// </summary>
		[JsonProperty("test", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.TestTransaction, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		public bool? IsTestTransaction { get; set; }

		/// <summary>
		///  The ID for the user who was logged into the Shopify POS device when the order was processed, if applicable.
		/// </summary>
		[JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.UserId, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public long? UserId { get; set; }


		/// <summary>
		/// The transaction fees charged on the order transaction. Only present for Shopify Payments transactions. (Note: this field is only applicable in GraphQL API)
		/// </summary>
		[JsonIgnore]
		public IEnumerable<OrderTransactionFeeGQL> Fees { get; set; }
	}
}
