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
using System.Linq;
using System.Text;

namespace PX.Commerce.Shopify.API.GraphQL
{
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class OrdersResponseData : EntitiesResponse<OrderDataGQL, OrderNode>, INodeListResponse<OrderDataGQL>, IEdgesResponse<OrderDataGQL, OrderNode>
	{
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class OrderNode : EntityNodeResponse<OrderDataGQL>
	{
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class OrderResponse : IEntityResponse<OrderDataGQL>
	{
		[JsonProperty("order")]
		public OrderDataGQL TEntityData { get; set; }
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class OrdersResponse : IEntitiesResponses<OrderDataGQL, OrdersResponseData>
	{
		[JsonProperty("orders")]
		public OrdersResponseData TEntitiesData { get; set; }
	}

	/// <summary>
	/// An order is a customer's request to purchase one or more products from a shop.
	/// </summary>
	[JsonObject(Description = "Order")]
	[GraphQLObject(NodeName = "order", ConnectionName = "orders")]
	[CommerceDescription(ShopifyCaptions.OrderData, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class OrderDataGQL : BCAPIEntity, INode
	{
		public class Arguments
		{
			[GraphQLArgument("sortKey", "OrderSortKeys")]
			public abstract class SortKey { }
		}

		/// <summary>
		/// A list of discounts that are applied to the order, not including order edits and refunds.
		/// </summary>
		[JsonProperty("discountApplications", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("discountApplications", GraphQLConstants.DataType.Connection, typeof(DiscountApplicationGQL))]
		public Connection<DiscountApplicationGQL> DiscountApplications { get; set; }

		public IEnumerable<DiscountApplicationGQL> DiscountApplicationsList { get => DiscountApplications?.Nodes; }

		/// <summary>
		/// The mailing address associated with the payment method.
		/// </summary>
		[JsonProperty("billingAddress")]
		[CommerceDescription(ShopifyCaptions.BillingAddress, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		[GraphQLField("billingAddress", GraphQLConstants.DataType.Object, typeof(MailingAddress))]
		public MailingAddress BillingAddress { get; set; }

		/// <summary>
		/// Whether the billing address matches the shipping address.
		/// </summary>
		[JsonProperty("billingAddressMatchesShippingAddress")]
		[GraphQLField("billingAddressMatchesShippingAddress", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool? BillingAddressMatchesShippingAddress { get; set; }

		/// <summary>
		/// Whether the order can be manually marked as paid.
		/// </summary>
		[JsonProperty("canMarkAsPaid")]
		[GraphQLField("canMarkAsPaid", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool? CanMarkAsPaid { get; set; }

		/// <summary>
		/// Whether a customer email exists for the order.
		/// </summary>
		[JsonProperty("canNotifyCustomer")]
		[GraphQLField("canNotifyCustomer", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool? CanNotifyCustomer { get; set; }

		/// <summary>
		/// The reason provided when the order was canceled. Returns null if the order wasn't canceled.
		/// </summary>
		[JsonProperty("cancelReason")]
		[CommerceDescription(ShopifyCaptions.CancelReason, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		[GraphQLField("cancelReason", GraphQLConstants.DataType.Enum, GraphQLConstants.ScalarType.String)]
		public OrderCancelReason? CancelReason { get; set; }

		/// <summary>
		/// The date and time when the order was canceled. Returns null if the order wasn't canceled.
		/// </summary>
		[JsonProperty("cancelledAt")]
		[CommerceDescription(ShopifyCaptions.DateCanceled, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		[GraphQLField("cancelledAt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.DateTime)]
		public DateTime? CancelledAt { get; set; }

		/// <summary>
		/// Whether the order is closed.
		/// </summary>
		[JsonProperty("closed")]
		[GraphQLField("closed", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool? Closed { get; set; }

		/// <summary>
		/// The date and time when the order was closed. Returns null if the order is not closed.
		/// </summary>
		[JsonProperty("closedAt")]
		[GraphQLField("closedAt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.DateTime)]
		public DateTime? ClosedAt { get; set; }

		/// <summary>
		/// Whether inventory has been reserved for the order.
		/// </summary>
		[JsonProperty("confirmed")]
		[GraphQLField("confirmed", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool? Confirmed { get; set; }

		/// <summary>
		/// The date and time when the order was created in Shopify.
		/// </summary>
		[JsonProperty("createdAt")]
		[CommerceDescription(ShopifyCaptions.DateCreated, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		[GraphQLField("createdAt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.DateTime)]
		public DateTime? DateCreatedAt { get; set; }

		/// <summary>
		///The shop currency when the order was placed. 
		/// </summary>
		[JsonProperty("currencyCode")]
		[CommerceDescription(ShopifyCaptions.Currency, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		[GraphQLField("currencyCode", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Currency { get; set; }

		/// <summary>
		/// The current order-level discount amount after all order updates, in shop and presentment currencies.
		/// </summary>
		[JsonProperty("currentCartDiscountAmountSet")]
		[GraphQLField("currentCartDiscountAmountSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag CurrentCartDiscountAmountSet { get; set; }

		[JsonIgnore]
		public decimal? CurrentCartDiscountAmountPresentment { get => CurrentCartDiscountAmountSet?.PresentmentMoney?.Amount; }

		/// <summary>
		/// The sum of the quantities for all line items that contribute to the order's current subtotal price.
		/// </summary>
		[JsonProperty("currentSubtotalLineItemsQuantity")]
		[GraphQLField("currentSubtotalLineItemsQuantity", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Int)]
		public int? CurrentSubtotalLineItemsQuantity { get; set; }

		/// <summary>
		/// The sum of the prices for all line items after discounts and returns, in shop and presentment currencies. If taxesIncluded is true, then the subtotal also includes tax.
		/// </summary>
		[JsonProperty("currentSubtotalPriceSet")]
		[GraphQLField("currentSubtotalPriceSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag CurrentSubtotalPriceSet { get; set; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.CurrentSubTotalPricePresentment, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public decimal? CurrentSubTotalPricePresentment { get => CurrentSubtotalPriceSet?.PresentmentMoney?.Amount; }

		/// <summary>
		/// A list of all tax lines applied to line items on the order, after returns. Tax line prices represent the total price for all tax lines with the same rate and title.
		/// </summary>
		[JsonProperty("currentTaxLines")]
		[GraphQLField("currentTaxLines", GraphQLConstants.DataType.Object, typeof(OrderTaxLineGQL))]
		public List<OrderTaxLineGQL> CurrentTaxLines { get; set; }

		//This field doesn't exist in API version 2023-01, it will be changed if we change the API version later.
		///// <summary>
		///// The total amount of additional fees after returns, in shop and presentment currencies. Returns null if there are no additional fees for the order.
		///// </summary>
		//[JsonProperty("currentTotalAdditionalFeesSet")]
		//[GraphQLField("currentTotalAdditionalFeesSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		//public MoneyBag CurrentTotalAdditionalFeesSet { get; set; }

		//[CommerceDescription(ShopifyCaptions.CurrentTotalAdditionalFeesPresentment, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		//public decimal? CurrentTotalAdditionalFeesPresentment { get => CurrentTotalAdditionalFeesSet?.PresentmentMoney?.Amount; }

		/// <summary>
		/// The total amount discounted on the order after returns, in shop and presentment currencies. This includes both order and line level discounts.
		/// </summary>
		[JsonProperty("currentTotalDiscountsSet")]
		[GraphQLField("currentTotalDiscountsSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag CurrentTotalDiscountsSet { get; set; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.CurrentTotalDiscountsPresentment, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public decimal? CurrentTotalDiscountsPresentment { get => CurrentTotalDiscountsSet?.PresentmentMoney?.Amount; }

		/// <summary>
		/// The total amount of duties after returns, in shop and presentment currencies. Returns null if duties aren't applicable.
		/// </summary>
		[JsonProperty("currentTotalDutiesSet")]
		[GraphQLField("currentTotalDutiesSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag CurrentTotalDutiesSet { get; set; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.CurrentTotalDutiesPresentment, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public decimal? CurrentTotalDutiesPresentment { get => CurrentTotalDutiesSet?.PresentmentMoney?.Amount; }

		/// <summary>
		/// The total price of the order, after returns, in shop and presentment currencies. This includes taxes and discounts.
		/// </summary>
		[JsonProperty("currentTotalPriceSet")]
		[GraphQLField("currentTotalPriceSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag CurrentTotalPriceSet { get; set; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.CurrentTotalPricePresentment, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public decimal? CurrentTotalPricePresentment { get => CurrentTotalPriceSet?.PresentmentMoney?.Amount; }

		/// <summary>
		/// The sum of the prices of all tax lines applied to line items on the order, after returns, in shop and presentment currencies.
		/// </summary>
		[JsonProperty("currentTotalTaxSet")]
		[GraphQLField("currentTotalTaxSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag CurrentTotalTaxSet { get; set; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.CurrentTotalTaxPresentment, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public decimal? CurrentTotalTaxPresentment { get => CurrentTotalTaxSet?.PresentmentMoney?.Amount; }

		/// <summary>
		/// The total weight of the order after returns, in grams.
		/// </summary>
		[JsonProperty("currentTotalWeight")]
		[GraphQLField("currentTotalWeight", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Decimal)]
		public decimal? CurrentTotalWeight { get; set; }

		/// <summary>
		/// Extra information added to the customer.
		/// </summary>
		[JsonProperty("customAttributes")]
		[GraphQLField("customAttributes", GraphQLConstants.DataType.Object, typeof(Attribute))]
		[CommerceDescription(ShopifyCaptions.NoteAttribute, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public List<Attribute> CustomAttributes { get; set; }

		/// <summary>
		/// The customer that placed the order.
		/// </summary>
		[JsonProperty("customer", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("customer", GraphQLConstants.DataType.Object, typeof(CustomerDataGQL))]
		[CommerceDescription(ShopifyCaptions.Customer, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public CustomerDataGQL Customer { get; set; }

		/// <summary>
		/// The discount code used for the order.
		/// </summary>
		[JsonProperty("discountCode", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("discountCode", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.Discount, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public string DiscountCode { get; set; }

		/// <summary>
		/// This field only use for order export
		/// </summary>
		public DraftOrderAppliedDiscountInput AppliedDiscount { get; set; }

		/// <summary>
		/// The discount code used for the order.
		/// </summary>
		[JsonProperty("discountCodes", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("discountCodes", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public List<string> DiscountCodes { get; set; }

		/// <summary>
		/// The primary address of the customer. Returns null if neither the shipping address nor the billing address was provided.
		/// </summary>
		[JsonProperty("displayAddress", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("displayAddress", GraphQLConstants.DataType.Object, typeof(MailingAddress))]
		public MailingAddress DisplayAddress { get; set; }

		/// <summary>
		/// The financial status of the order that can be shown to the merchant. This field does not capture all the details of an order's financial state. It should only be used for display summary purposes.
		/// </summary>
		[JsonProperty("displayFinancialStatus", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("displayFinancialStatus", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.FinancialStatus, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public string FinancialStatus { get; set; }

		/// <summary>
		/// The fulfillment status for the order that can be shown to the merchant. This field does not capture all the details of an order's fulfillment state.
		/// It should only be used for display summary purposes. For a more granular view of the fulfillment status, refer to the FulfillmentOrder object.
		/// </summary>
		[JsonProperty("displayFulfillmentStatus", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("displayFulfillmentStatus", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.FulfillmentStatus, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public string FulfillmentStatus { get; set; }

		/// <summary>
		/// Whether the order has had any edits applied.
		/// </summary>
		[JsonProperty("edited", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("edited", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool? Edited { get; set; }

		/// <summary>
		/// The customer's email address.
		/// </summary>
		[JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("email", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.Email, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public string Email { get; set; }

		/// <summary>
		/// Whether taxes on the order are estimated. This field returns false when taxes on the order are finalized and aren't subject to any changes.
		/// </summary>
		[JsonProperty("estimatedTaxes", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("estimatedTaxes", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool? EstimatedTaxes { get; set; }

		/// <summary>
		/// Whether there are line items that can be fulfilled. This field returns false when the order has no fulfillable line items. For a more granular view of the fulfillment status, refer to the FulfillmentOrder object.
		/// </summary>
		[JsonProperty("fulfillable", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("fulfillable", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool? Fulfillable { get; set; }

		/// <summary>
		/// A list of fulfillment orders for a specific order.
		/// </summary>
		[JsonProperty("fulfillmentOrders", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("fulfillmentOrders", GraphQLConstants.DataType.Connection, typeof(FulfillmentOrderGQL))]
		public Connection<FulfillmentOrderGQL> FulfillmentOrders { get; set; }

		public IEnumerable<FulfillmentOrderGQL> FulfillmentOrdersList { get => FulfillmentOrders?.Nodes; }

		/// <summary>
		/// List of shipments for the order.
		/// </summary>
		[JsonProperty("fulfillments", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("fulfillments", GraphQLConstants.DataType.Object, typeof(FulfillmentGQL))]
		[CommerceDescription(ShopifyCaptions.Fulfillment, FieldFilterStatus.Skipped, FieldMappingStatus.Skipped)]
		public List<FulfillmentGQL> Fulfillments { get; set; }

		/// <summary>
		/// Whether the order has been paid in full.
		/// </summary>
		[JsonProperty("fullyPaid", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("fullyPaid", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool? FullyPaid { get; set; }

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
		[CommerceDescription(ShopifyCaptions.OrderId, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		public long? LegacyResourceId { get; set; }

		/// <summary>
		/// The list of line Items.
		/// </summary>
		[JsonProperty("lineItems")]
		[GraphQLField("lineItems", GraphQLConstants.DataType.Connection, typeof(OrderLineItemGQL))]
		public Connection<OrderLineItemGQL> LineItems { get; set; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.LineItem, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public IEnumerable<OrderLineItemGQL> LineItemsList { get => LineItems?.Nodes; }

		/// <summary>
		/// Whether the order can be edited by the merchant. For example, canceled orders cannot be edited.
		/// </summary>
		[JsonProperty("merchantEditable", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("merchantEditable", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool? MerchantEditable { get; set; }

		/// <summary>
		/// The identifier for the order, which is unique within the store. For example, #D1223.
		/// </summary>
		[PIIData]
		[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("name", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.Name, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public string Name { get; set; }

		/// <summary>
		/// The net payment for the order, based on the total amount received minus the total amount refunded, in shop and presentment currencies.
		/// </summary>
		[JsonProperty("netPaymentSet")]
		[GraphQLField("netPaymentSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag NetPaymentSet { get; set; }

		/// <summary>
		/// The text of an optional note that a shop owner can attach to the order.
		/// </summary>
		[JsonProperty("note", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("note", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.Note, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public string Note { get; set; }

		///// <summary>
		///// The total amount of additional fees after returns, in shop and presentment currencies. Returns null if there are no additional fees for the order.
		///// </summary>
		//[JsonProperty("originalTotalAdditionalFeesSet")]
		//[GraphQLField("originalTotalAdditionalFeesSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		//public MoneyBag OriginalTotalAdditionalFeesSet { get; set; }

		/// <summary>
		/// The total amount of duties before returns, in shop and presentment currencies. Returns null if duties aren't applicable.
		/// </summary>
		[JsonProperty("originalTotalDutiesSet")]
		[GraphQLField("originalTotalDutiesSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag OriginalTotalDutiesSet { get; set; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.OriginalTotalDutiesPresentment, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public decimal? OriginalTotalDutiesPresentment { get => OriginalTotalDutiesSet?.PresentmentMoney?.Amount; }

		/// <summary>
		/// The total price of the order at the time of order creation, in shop and presentment currencies.
		/// </summary>
		[JsonProperty("originalTotalPriceSet")]
		[GraphQLField("originalTotalPriceSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag OriginalTotalPriceSet { get; set; }

		/// <summary>
		/// Attaches additional metadata to a shop's resources:
		///key(required) : An identifier for the metafield(maximum of 30 characters).
		///namespace(required): A container for a set of metadata(maximum of 20 characters). Namespaces help distinguish between metadata that you created and metadata created by another individual with a similar namespace.
		///value (required): Information to be stored as metadata.
		///value_type(required): The value type.Valid values: string and integer.
		///description(optional): Additional information about the metafield.
		/// </summary>
		[JsonProperty("metafields", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Metafields, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		[BCExternCustomField(BCConstants.ShopifyMetaFields)]
		[GraphQLField("metafields", GraphQLConstants.DataType.Connection, typeof(MetafieldGQL))]
		public Connection<MetafieldGQL> MetafieldNodes { get; set; }

		[JsonIgnore]
		public IEnumerable<MetafieldGQL> MetafieldList { get => MetafieldNodes?.Nodes; set => MetafieldNodes = new Connection<MetafieldGQL> { Nodes = value}; }

		/// <summary>
		/// A list of the names of all payment gateways used for the order. For example, "Shopify Payments" and "Cash on Delivery (COD)".
		/// </summary>
		[JsonProperty("paymentGatewayNames", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("paymentGatewayNames", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public List<string> PaymentGatewayNames { get; set; }

		/// <summary>
		/// The fulfillment location that was assigned when the order was created. Use the FulfillmentOrder object for up to date fulfillment location information.
		/// </summary>
		[JsonProperty("physicalLocation", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("physicalLocation", GraphQLConstants.DataType.Object, typeof(LocationGQL))]
		[CommerceDescription(ShopifyCaptions.InventoryLocation, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		public LocationGQL PhysicalLocation { get; set; }

		/// <summary>
		/// The customer's phone number.
		/// </summary>
		[PIIData]
		[JsonProperty("phone", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("phone", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.Phone, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public string Phone { get; set; }

		/// <summary>
		/// The payment currency of the customer for this order, which generally follows ISO 3166-1 alpha-2 guidelines. 
		/// </summary>
		[JsonProperty("presentmentCurrencyCode", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("presentmentCurrencyCode", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.PresentmentCurrency, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public string PresentmentCurrencyCode { get; set; }

		/// <summary>
		/// The date and time when the order was processed. This date and time might not match the date and time when the order was created.
		/// </summary>
		[JsonProperty("processedAt", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("processedAt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.DateTime)]
		[CommerceDescription(ShopifyCaptions.ProcessedAt, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		public DateTime? ProcessedAt { get; set; }

		/// <summary>
		/// The purchasing entity for this order.
		/// </summary>
		[JsonProperty("purchasingEntity", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("purchasingEntity", GraphQLConstants.DataType.Union, typeof(PurchasingEntity))]
		public PurchasingEntity PurchasingEntity { get; set; }

		/// <summary>
		/// The difference between the suggested and actual refund amount of all refunds that have been applied to the order.
		/// A positive value indicates a difference in the merchant's favor, and a negative value indicates a difference in the customer's favor.
		/// </summary>
		[JsonProperty("refundDiscrepancySet")]
		[GraphQLField("refundDiscrepancySet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag RefundDiscrepancySet { get; set; }

		/// <summary>
		/// Whether the order can be refunded.
		/// </summary>
		[JsonProperty("refundable", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("refundable", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool? Refundable { get; set; }

		/// <summary>
		/// A list of refunds that have been applied to the order.
		/// </summary>
		[JsonProperty("refunds", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("refunds", GraphQLConstants.DataType.Object, typeof(OrderRefundGQL))]
		[CommerceDescription(ShopifyCaptions.Refund, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public List<OrderRefundGQL> Refunds { get; set; }

		/// <summary>
		/// Whether the order has shipping lines or at least one line item on the order that requires shipping.
		/// </summary>
		[JsonProperty("requiresShipping", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("requiresShipping", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool? RequiresShipping { get; set; }

		/// <summary>
		/// Whether any line item on the order can be restocked.
		/// </summary>
		[JsonProperty("restockable", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("restockable", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool? Restockable { get; set; }

		/// <summary>
		/// The fraud risk level of the order.
		/// </summary>
		[JsonProperty("riskLevel", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("riskLevel", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.OrderRisk, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public string RiskLevel { get; set; }

		/// <summary>
		/// A list of risks associated with the order.
		/// </summary>
		//[JsonProperty("risks", NullValueHandling = NullValueHandling.Ignore)]
		//[GraphQLField("risks", GraphQLConstants.DataType.Object, typeof(OrderRiskGL))]
		//public List<OrderRiskGL> OrderRisks { get; set; }

		/// <summary>
		/// The mailing address to where the order will be shipped.
		/// </summary>
		[JsonProperty("shippingAddress", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("shippingAddress", GraphQLConstants.DataType.Object, typeof(MailingAddress))]
		[CommerceDescription(ShopifyCaptions.ShippingAddress, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public MailingAddress ShippingAddress { get; set; }

		/// <summary>
		/// A summary of all shipping costs on the order.
		/// </summary>
		[JsonProperty("shippingLine", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("shippingLine", GraphQLConstants.DataType.Object, typeof(OrderShippingLineGQL))]
		[CommerceDescription(ShopifyCaptions.ShippingLine, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public OrderShippingLineGQL ShippingLine { get; set; }

		/// <summary>
		/// A unique POS or third party order identifier. For example, "1234-12-1000" or "111-98567-54". The receipt_number field is derived from this value for POS orders.
		/// </summary>
		[JsonProperty("sourceIdentifier", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.SourceName, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		[GraphQLField("sourceIdentifier", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string SourceName { get; set; }

		/// <summary>
		/// The sum of the quantities for all line items that contribute to the order's subtotal price.
		/// </summary>
		[JsonProperty("subtotalLineItemsQuantity", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("subtotalLineItemsQuantity", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Int)]
		public int SubtotalLineItemsQuantity { get; set; }

		/// <summary>
		/// A subtotal of the line items and corresponding discounts. The subtotal doesn't include shipping charges, shipping discounts, or taxes.
		/// </summary>
		[JsonProperty("subtotalPriceSet", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("subtotalPriceSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag SubtotalPriceSet { get; set; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.SubtotalPresentment, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public decimal? SubTotalPresentment { get => SubtotalPriceSet?.PresentmentMoney?.Amount; }

		/// <summary>
		/// A comma separated list of tags that have been added to the draft order.
		/// </summary>
		[JsonProperty("tags")]
		[GraphQLField("tags", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public List<string> TagsList { get; set; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.Tags, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public string Tags { get => string.Join(";", TagsList); set => TagsList = value?.Split(',', ';')?.ToList(); }

		/// <summary>
		/// A list of all tax lines applied to line items on the order, before returns. Tax line prices represent the total price for all tax lines with the same rate and title.
		/// </summary>
		[JsonProperty("taxLines")]
		[GraphQLField("taxLines", GraphQLConstants.DataType.Object, typeof(OrderTaxLineGQL))]
		[CommerceDescription(ShopifyCaptions.TaxLine, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public List<OrderTaxLineGQL> TaxLines { get; set; }

		/// <summary>
		/// Whether taxes are included in the subtotal price of the order.
		/// </summary>
		[JsonProperty("taxesIncluded", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("taxesIncluded", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		[CommerceDescription(ShopifyCaptions.TaxesIncluded, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public bool? TaxesIncluded { get; set; }

		/// <summary>
		/// Whether the order is a test. Test orders are made using the Shopify Bogus Gateway or a payment provider with test mode enabled. A test order cannot be converted into a real order and vice versa.
		/// </summary>
		[JsonProperty("test", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("test", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		[CommerceDescription(ShopifyCaptions.TestCase, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public bool? Test { get; set; }

		/// <summary>
		/// The authorized amount that is uncaptured or undercaptured, in shop and presentment currencies. This amount isn't adjusted for returns.
		/// </summary>
		[JsonProperty("totalCapturableSet", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("totalCapturableSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag TotalCapturableSet { get; set; }

		/// <summary>
		/// The total discounts for this order.
		/// </summary>
		[JsonProperty("totalDiscountsSet", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("totalDiscountsSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag TotalDiscountsSet { get; set; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.TotalDiscountPresentment, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public decimal? TotalDiscountPresentment { get => TotalDiscountsSet?.PresentmentMoney?.Amount; set => TotalDiscountsSet = new MoneyBag { PresentmentMoney = new Money { Amount = value } }; }

		/// <summary>
		/// The total amount of the order including taxes, shipping charges, and discounts.
		/// </summary>
		[JsonProperty("totalPriceSet", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("totalPriceSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag TotalPriceSet { get; set; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.OrderTotalPresentment, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public decimal? OrderTotalPresentment { get => TotalPriceSet?.PresentmentMoney?.Amount; set => TotalPriceSet = new MoneyBag { PresentmentMoney = new Money { Amount = value } }; }

		/// <summary>
		/// The total amount that was refunded, in shop and presentment currencies.
		/// </summary>
		[JsonProperty("totalRefundedSet", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("totalRefundedSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag TotalRefundedSet { get; set; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.OrderRefundsTotalPresentment, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public decimal? TotalRefundedPresentment { get => TotalRefundedSet?.PresentmentMoney?.Amount; }

		/// <summary>
		/// The total shipping charge for the order.
		/// </summary>
		[JsonProperty("totalShippingPriceSet", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("totalShippingPriceSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag TotalShippingPriceSet { get; set; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.OrderShippingsTotalPresentment, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public decimal? TotalShippingPricePresentment { get => TotalShippingPriceSet?.PresentmentMoney?.Amount; set => TotalShippingPriceSet = new MoneyBag { PresentmentMoney = new Money { Amount = value } }; }

		/// <summary>
		/// The total amount of shipping that was refunded, in shop and presentment currencies.
		/// </summary>
		[JsonProperty("totalRefundedShippingSet", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("totalRefundedShippingSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag TotalRefundedShippingSet { get; set; }

		[JsonIgnore]
		public decimal? TotalRefundedShippingPresentment { get => TotalRefundedShippingSet?.PresentmentMoney?.Amount; }

		/// <summary>
		/// The total amount of taxes for the order.
		/// </summary>
		[JsonProperty("totalTaxSet", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("totalTaxSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag TotalTaxSet { get; set; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.TotalTaxPresentment, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public decimal? TotalTaxPresentment { get => TotalTaxSet?.PresentmentMoney?.Amount; set => TotalTaxSet = new MoneyBag { PresentmentMoney = new Money { Amount = value } }; }

		/// <summary>
		/// The sum of all tip amounts for the order, in shop and presentment currencies.
		/// </summary>
		[JsonProperty("totalTipReceivedSet", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("totalTipReceivedSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag TotalTipReceivedSet { get; set; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.TotalTips, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		public decimal? TotalTipReceivedPresentment { get => TotalTipReceivedSet?.PresentmentMoney?.Amount; set => TotalTipReceivedSet = new MoneyBag { PresentmentMoney = new Money { Amount = value } }; }

		/// <summary>
		/// The total weight in grams of the order.
		/// </summary>
		[JsonProperty("totalWeight", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("totalWeight", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Float)]
		[CommerceDescription(ShopifyCaptions.TotalWeight, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public decimal? TotalWeight { get; set; }

		/// <summary>
		/// A list of transactions associated with the order.
		/// </summary>
		[JsonProperty("transactions")]
		[GraphQLField("transactions", GraphQLConstants.DataType.Object, typeof(OrderTransactionGQL))]
		[CommerceDescription(ShopifyCaptions.OrdersTransaction, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public List<OrderTransactionGQL> Transactions { get; set; }

		/// <summary>
		/// The date and time when the order was last changed. The format is YYYY-MM-DD HH:mm:ss. For example, 2016-02-05 17:04:01.
		/// </summary>
		[JsonProperty("updatedAt", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("updatedAt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.DateTime)]
		[CommerceDescription(ShopifyCaptions.DateModified, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		public DateTime? DateModifiedAt { get; set; }

		/// <summary>
		/// Whether no payments have been made for the order.
		/// </summary>
		[JsonProperty("unpaid", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("unpaid", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool? Unpaid { get; set; }

		/// <summary>
		/// Purchase Order number associated with the Sales Order
		/// </summary>
		[JsonProperty("poNumber", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("poNumber", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string PONumber { get; set; }

		[JsonIgnore]
		public Guid? LocalID { get; set; }

		public override string CalculateHash()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(this.Id ?? string.Empty);
			sb.Append(this.Email ?? string.Empty);
			sb.Append(this.Phone ?? string.Empty);
			sb.Append(this.OrderTotalPresentment ?? 0.00m);
			sb.Append(this.CurrentTotalTaxPresentment ?? 0.00m);
			foreach (var lineItem in this.LineItemsList ?? Enumerable.Empty<OrderLineItemGQL>())
			{
				sb.Append(lineItem.Id ?? string.Empty);
				sb.Append(lineItem.Quantity);
			}
			sb.Append(this.ShippingAddress?.Name ?? string.Empty);
			sb.Append(this.ShippingAddress?.Address1 ?? string.Empty);
			sb.Append(this.ShippingAddress?.Zip ?? string.Empty);

			if (sb.Length <= 0) return null;
			byte[] hash = PX.Data.Update.PXCriptoHelper.CalculateSHA(sb.ToString());
			String hashcode = String.Concat(hash.Select(b => b.ToString("X2")));
			return hashcode;
		}
	}
}
