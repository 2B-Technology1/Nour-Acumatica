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
using System;
using System.Collections.Generic;

namespace PX.Commerce.Shopify.API.GraphQL
{
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class DraftOrdersResponseData : EntitiesResponse<DraftOrderDataGQL, DraftOrderNode>, INodeListResponse<DraftOrderDataGQL>, IEdgesResponse<DraftOrderDataGQL, DraftOrderNode>
	{
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class DraftOrderNode : EntityNodeResponse<DraftOrderDataGQL>
	{
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class DraftOrderResponse : IEntityResponse<DraftOrderDataGQL>
	{
		[JsonProperty("draftOrder")]
		public DraftOrderDataGQL TEntityData { get; set; }
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class DraftOrdersResponse : IEntitiesResponses<DraftOrderDataGQL, DraftOrdersResponseData>
	{
		[JsonProperty("draftOrders ")]
		public DraftOrdersResponseData TEntitiesData { get; set; }
	}

	/// <summary>
	/// An order that a merchant creates on behalf of a customer.
	/// </summary>
	[GraphQLObject(NodeName = "draftOrder", ConnectionName = "draftOrders")]
	public class DraftOrderDataGQL : BCAPIEntity, INode
	{
		/// <summary>
		/// The discount that will be applied to the draft order. A draft order line item can have one discount. A draft order can also have one order-level discount.
		/// </summary>
		[JsonProperty("appliedDiscount")]
		[GraphQLField("appliedDiscount", GraphQLConstants.DataType.Object, typeof(DraftOrderAppliedDiscountGQL))]
		public DraftOrderAppliedDiscountGQL AppliedDiscount { get; set; }

		/// <summary>
		/// The mailing address associated with the payment method.
		/// </summary>
		[JsonProperty("billingAddress")]
		[GraphQLField("billingAddress", GraphQLConstants.DataType.Object, typeof(MailingAddress))]
		public MailingAddress BillingAddress { get; set; }

		/// <summary>
		/// Whether the billing address matches the shipping address.
		/// </summary>
		[JsonProperty("billingAddressMatchesShippingAddress")]
		[GraphQLField("billingAddressMatchesShippingAddress", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool? BillingAddressMatchesShippingAddress { get; set; }

		/// <summary>
		/// The date and time when the draft order converted to a new order, and the draft order's status changed to Completed.
		/// </summary>
		[JsonProperty("completedAt")]
		[GraphQLField("completedAt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.DateTime)]
		public DateTime? CompletedAt { get; set; }

		/// <summary>
		/// The date and time when the draft order was created in Shopify.
		/// </summary>
		[JsonProperty("createdAt")]
		[GraphQLField("createdAt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.DateTime)]
		public DateTime? CreatedAt { get; set; }

		/// <summary>
		/// The payment currency of the customer for this draft order, which generally follows ISO 3166-1 alpha-2 guidelines. 
		/// </summary>
		[JsonProperty("currencyCode")]
		[GraphQLField("currencyCode", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string CurrencyCode { get; set; }

		/// <summary>
		/// Extra information added to the customer.
		/// </summary>
		[JsonProperty("customAttributes")]
		[GraphQLField("customAttributes", GraphQLConstants.DataType.Object, typeof(Attribute))]
		public List<Attribute> CustomAttributes { get; set; }

		/// <summary>
		/// The customer who will be sent an invoice for the draft order, if there is one.
		/// </summary>
		[JsonProperty("customer", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("customer", GraphQLConstants.DataType.Object, typeof(CustomerDataGQL))]
		public CustomerDataGQL Customer { get; set; }

		/// <summary>
		/// A default cursor that returns the single next record, sorted ascending by ID.
		/// </summary>
		[JsonProperty("defaultCursor", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("defaultCursor", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string DefaultCursor { get; set; }

		/// <summary>
		/// The customer's email address.
		/// </summary>
		[JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("email", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Email { get; set; }

		/// <summary>
		/// Whether the merchant has added timeline comments to the draft order.
		/// </summary>
		[JsonProperty("hasTimelineComment", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("hasTimelineComment", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool? HasTimelineComment { get; set; }

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
		public long? LegacyResourceId { get; set; }

		/// <summary>
		/// The list of line Items.
		/// </summary>
		[JsonProperty("lineItems")]
		[GraphQLField("lineItems", GraphQLConstants.DataType.Connection, typeof(DraftOrderLineItemDataGQL))]
		public Connection<DraftOrderLineItemDataGQL> LineItems { get; set; }

		[JsonIgnore]
		public IEnumerable<DraftOrderLineItemDataGQL> LineItemsList { get => LineItems?.Nodes; }

		/// <summary>
		/// The subtotal of the line items and corresponding discounts. The subtotal doesn't include shipping charges, shipping discounts, taxes, or order discounts.
		/// </summary>
		[JsonProperty("lineItemsSubtotalPrice")]
		[GraphQLField("lineItemsSubtotalPrice", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag LineItemsSubtotalPrice { get; set; }

		/// <summary>
		/// The name of the selected market.
		/// </summary>
		[JsonProperty("marketName")]
		[GraphQLField("marketName", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string MarketName { get; set; }

		/// <summary>
		/// The code designating a country/region, which generally follows ISO 3166-1 alpha-2 guidelines. 
		/// </summary>
		[JsonProperty("marketRegionCountryCode", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("marketRegionCountryCode", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string MarketRegionCountryCode { get; set; }

		/// <summary>
		/// The identifier for the draft order, which is unique within the store. For example, #D1223.
		/// </summary>
		[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("name", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Name { get; set; }

		/// <summary>
		/// The text of an optional note that a shop owner can attach to the draft order.
		/// </summary>
		[JsonProperty("note2", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("note2", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Note { get; set; }

		/// <summary>
		/// The order that was created from this draft order.
		/// </summary>
		[JsonProperty("order", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("order", GraphQLConstants.DataType.Object, typeof(OrderDataGQL))]
		public OrderDataGQL Order { get; set; }

		/// <summary>
		/// The customer's phone number.
		/// </summary>
		[JsonProperty("phone", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("phone", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Phone { get; set; }

		/// <summary>
		/// The payment currency of the customer for this draft order, which generally follows ISO 3166-1 alpha-2 guidelines. 
		/// </summary>
		[JsonProperty("presentmentCurrencyCode", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("presentmentCurrencyCode", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string PresentmentCurrencyCode { get; set; }

		/// <summary>
		/// The purchasing entity for this draft order.
		/// </summary>
		[JsonProperty("purchasingEntity")]
		[GraphQLField("purchasingEntity", GraphQLConstants.DataType.Union, typeof(PurchasingEntity))]
		public PurchasingEntity PurchasingEntity { get; set; }

		/// <summary>
		/// Whether the Draft Order is ready and can be completed. Draft Orders might have asynchronous operations that can take time to finish.
		/// </summary>
		[JsonProperty("ready", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("ready", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool? Ready { get; set; }

		/// <summary>
		/// Time after which inventory will automatically be restocked.
		/// </summary>
		[JsonProperty("reserveInventoryUntil", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("reserveInventoryUntil", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.DateTime)]
		public DateTime? ReserveInventoryUntil { get; set; }

		/// <summary>
		/// The mailing address to where the order will be shipped.
		/// </summary>
		[JsonProperty("shippingAddress", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("shippingAddress", GraphQLConstants.DataType.Object, typeof(MailingAddress))]
		public MailingAddress ShippingAddress { get; set; }

		/// <summary>
		/// The source of the checkout. To use this field for sales attribution, you must register the channels that your app is managing.
		/// </summary>
		[JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("status", GraphQLConstants.DataType.Enum, GraphQLConstants.ScalarType.String)]
		public DraftOrderStatus Status { get; set; }

		/// <summary>
		/// The subtotal of the line items and their discounts. The subtotal doesn't include shipping charges, shipping discounts, or taxes.
		/// </summary>
		[JsonProperty("subtotalPrice", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("subtotalPrice", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public decimal SubtotalPrice { get; set; }

		/// <summary>
		/// A subtotal of the line items and corresponding discounts. The subtotal doesn't include shipping charges, shipping discounts, or taxes.
		/// </summary>
		[JsonProperty("subtotalPriceSet", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("subtotalPriceSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag SubtotalPriceSet { get; set; }

		/// <summary>
		/// A comma separated list of tags that have been added to the draft order.
		/// </summary>
		[JsonProperty("tags")]
		[GraphQLField("tags", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public List<string> Tags { get; set; }

		/// <summary>
		/// Whether or not taxes are exempt for the draft order. If false, then Shopify will refer to the taxable field for each line item. If a customer is applied to the draft order, then Shopify will use the customer's tax exempt field instead.
		/// </summary>
		[JsonProperty("taxExempt", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("taxExempt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool? TaxExempt { get; set; }

		/// <summary>
		/// Whether the line item prices include taxes.
		/// </summary>
		[JsonProperty("taxesIncluded", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("taxesIncluded", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool? TaxesIncluded { get; set; }

		/// <summary>
		/// The total discounts for this draft order.
		/// </summary>
		[JsonProperty("totalDiscountsSet", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("totalDiscountsSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag TotalDiscountsSet { get; set; }

		/// <summary>
		/// The total price of line items for this draft order.
		/// </summary>
		[JsonProperty("totalLineItemsPriceSet", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("totalLineItemsPriceSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag TotalLineItemsPriceSet { get; set; }

		/// <summary>
		/// The total amount of the draft order, including taxes, shipping charges, and discounts.
		/// </summary>
		[JsonProperty("totalPrice", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("totalPrice", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Money)]
		public decimal TotalPrice { get; set; }

		/// <summary>
		/// The total amount of the draft order including taxes, shipping charges, and discounts.
		/// </summary>
		[JsonProperty("totalPriceSet", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("totalPriceSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag TotalPriceSet { get; set; }

		/// <summary>
		/// The total shipping charge for the draft order.
		/// </summary>
		[JsonProperty("totalShippingPrice", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("totalShippingPrice", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Money)]
		public decimal TotalShippingPrice { get; set; }

		/// <summary>
		/// The total shipping charge for the draft order.
		/// </summary>
		[JsonProperty("totalShippingPriceSet", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("totalShippingPriceSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag TotalShippingPriceSet { get; set; }

		/// <summary>
		/// The total amount of taxes for the draft order.
		/// </summary>
		[JsonProperty("totalTax", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("totalTax", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Money)]
		public decimal TotalTax { get; set; }

		/// <summary>
		/// The total amount of taxes for the draft order.
		/// </summary>
		[JsonProperty("totalTaxSet", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("totalTaxSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag TotalTaxSet { get; set; }

		/// <summary>
		/// The total weight in grams of the draft order.
		/// </summary>
		[JsonProperty("totalWeight", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("totalWeight", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.UnsignedInt64)]
		public decimal TotalWeight { get; set; }

		/// <summary>
		/// The date and time when the draft order was last changed. The format is YYYY-MM-DD HH:mm:ss. For example, 2016-02-05 17:04:01.
		/// </summary>
		[JsonProperty("updatedAt", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("updatedAt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.DateTime)]
		public DateTime? UpdatedAt { get; set; }

		/// <summary>
		/// Whether the draft order will be visible to the customer on the self-serve portal.
		/// </summary>
		[JsonProperty("visibleToCustomer", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("visibleToCustomer", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool? VisibleToCustomer { get; set; }

		[JsonIgnore]
		public Guid? LocalID { get; set; }
	}
}
