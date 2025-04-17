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
using System;
using System.Collections.Generic;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// The input fields used to create or update a draft order.
	/// </summary>
	public class DraftOrderInput
	{
		/// <summary>
		/// The discount that will be applied to the draft order. A draft order line item can have one discount. A draft order can also have one order-level discount.
		/// </summary>
		[JsonProperty("appliedDiscount", NullValueHandling = NullValueHandling.Ignore)]
		public DraftOrderAppliedDiscountInput AppliedDiscount { get; set; }

		/// <summary>
		/// The mailing address associated with the payment method.
		/// </summary>
		[JsonProperty("billingAddress", NullValueHandling = NullValueHandling.Ignore)]
		public MailingAddressInput BillingAddress { get; set; }

		/// <summary>
		/// Extra information added to the customer.
		/// </summary>
		[JsonProperty("customAttributes", NullValueHandling = NullValueHandling.Ignore)]
		public List<AttributeInput> CustomAttributes { get; set; }

		/// <summary>
		/// The customer's email address.
		/// </summary>
		[JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
		public string Email { get; set; }

		/// <summary>
		/// Product variant line item or custom line item associated to the draft order. Each draft order must include at least one line item.
		/// </summary>
		[JsonProperty("lineItems", NullValueHandling = NullValueHandling.Ignore)]
		public List<OrderLineItemInput> LineItems { get; set; }

		/// <summary>
		/// The code designating a country/region, which generally follows ISO 3166-1 alpha-2 guidelines. 
		/// </summary>
		[JsonProperty("marketRegionCountryCode", NullValueHandling = NullValueHandling.Ignore)]
		public string MarketRegionCountryCode { get; set; }

		/// <summary>
		/// Metafields attached to the draft order.
		/// </summary>
		[JsonProperty("metafields", NullValueHandling = NullValueHandling.Ignore)]
		public List<MetafieldInput> Metafields { get; set; }

		/// <summary>
		/// The text of an optional note that a shop owner can attach to the draft order.
		/// </summary>
		[JsonProperty("note", NullValueHandling = NullValueHandling.Ignore)]
		public string Note { get; set; }

		/// <summary>
		/// The customer's phone number.
		/// </summary>
		[JsonProperty("phone", NullValueHandling = NullValueHandling.Ignore)]
		public string Phone { get; set; }

		/// <summary>
		/// The payment currency of the customer for this draft order, which generally follows ISO 3166-1 alpha-2 guidelines. 
		/// </summary>
		[JsonProperty("presentmentCurrencyCode", NullValueHandling = NullValueHandling.Ignore)]
		public string PresentmentCurrencyCode { get; set; }

		/// <summary>
		/// The private metafields attached to the draft order.
		/// </summary>
		[JsonProperty("privateMetafields", NullValueHandling = NullValueHandling.Ignore)]
		public List<PrivateMetafieldInput> PrivateMetafields { get; set; }

		/// <summary>
		/// The purchasing entity for this draft order.
		/// </summary>
		[JsonProperty("purchasingEntity")]
		public PurchasingEntityInput PurchasingEntity { get; set; }

		/// <summary>
		/// Time after which inventory will automatically be restocked.
		/// </summary>
		[JsonProperty("reserveInventoryUntil", NullValueHandling = NullValueHandling.Ignore)]
		public DateTime? ReserveInventoryUntil { get; set; }

		/// <summary>
		/// The mailing address to where the order will be shipped.
		/// </summary>
		[JsonProperty("shippingAddress", NullValueHandling = NullValueHandling.Ignore)]
		public MailingAddressInput ShippingAddress { get; set; }

		/// <summary>
		/// A shipping line object, which details the shipping method used.
		/// </summary>
		[JsonProperty("shippingLine", NullValueHandling = NullValueHandling.Ignore)]
		public ShippingLineInput ShippingLine { get; set; }

		/// <summary>
		/// The source of the checkout. To use this field for sales attribution, you must register the channels that your app is managing.
		/// </summary>
		[JsonProperty("sourceName", NullValueHandling = NullValueHandling.Ignore)]
		public string SourceName { get; set; }

		/// <summary>
		/// A comma separated list of tags that have been added to the draft order.
		/// </summary>
		[JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
		public List<string> Tags { get; set; }

		/// <summary>
		/// Whether or not taxes are exempt for the draft order. If false, then Shopify will refer to the taxable field for each line item. If a customer is applied to the draft order, then Shopify will use the customer's tax exempt field instead.
		/// </summary>
		[JsonProperty("taxExempt", NullValueHandling = NullValueHandling.Ignore)]
		public bool? TaxExempt { get; set; }

		/// <summary>
		/// Sent as part of a draft order object to load customer shipping information.
		/// </summary>
		[JsonProperty("useCustomerDefaultAddress", NullValueHandling = NullValueHandling.Ignore)]
		public bool? UseCustomerDefaultAddress { get; set; }

		/// <summary>
		/// Whether the draft order will be visible to the customer on the self-serve portal.
		/// </summary>
		[JsonProperty("visibleToCustomer", NullValueHandling = NullValueHandling.Ignore)]
		public bool? VisibleToCustomer { get; set; }

		[JsonIgnore]
		public Guid? LocalID { get; set; }
	}
}
