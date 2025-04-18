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
using System.ComponentModel;

namespace PX.Commerce.Shopify.API.REST
{
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class CustomerResponse : IEntityResponse<CustomerData>
	{
		[JsonProperty("customer")]
		public CustomerData Data { get; set; }
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class CustomersResponse : IEntitiesResponse<CustomerData>
	{
		[JsonProperty("customers")]
		public IEnumerable<CustomerData> Data { get; set; }
	}

	[JsonObject(Description = "Customer")]
	[CommerceDescription(ShopifyCaptions.Customer, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class CustomerData: BCAPIEntity
	{
        public CustomerData()
        {
            Addresses = new List<CustomerAddressData>();
        }

		/// <summary>
		/// A unique identifier for the customer
		/// </summary>
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.CustomerId, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		public virtual long? Id { get; set; }

		/// <summary>
		/// The unique email address of the customer. Attempting to assign the same email address to multiple customers returns an error.
		/// </summary>
		[PIIData]
		[JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.EmailAddress, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		[ValidateRequired()]
		public virtual string Email { get; set; }

		/// <summary>
		/// The marketing consent information when the customer consented to receiving marketing material by email.
		/// The email property is required to create a customer with email consent information and to update a customer
		/// for email consent that doesn't have an email recorded. The customer must have a unique email address associated to the record.
		/// </summary>
		[JsonProperty("email_marketing_consent", NullValueHandling = NullValueHandling.Ignore)]
		public virtual CustomerEmailMarketing CustomerMarketingInfo { get; set; }

		/// <summary>
		/// The date and time (ISO 8601 format) when the customer was created.
		/// </summary>
		[JsonProperty("created_at")]
		[CommerceDescription(ShopifyCaptions.DateCreated, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		[ShouldNotSerialize]
		public virtual string DateCreatedAt { get; set; }

		/// <summary>
		/// The date and time (ISO 8601 format) when the customer was last updated.
		/// </summary>
		[JsonProperty("updated_at")]
		[ShouldNotSerialize]
		[CommerceDescription(ShopifyCaptions.DateModified, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		public virtual string DateModifiedAt { get; set; }

		/// <summary>
		/// The customer's first name.
		/// </summary>
		[PIIData]
		[JsonProperty("first_name", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.FirstName, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		[ValidateRequired]
		public virtual string FirstName { get; set; }

		/// <summary>
		/// The customer's last name.
		/// </summary>
		[PIIData]
		[JsonProperty("last_name", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.LastName, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		[ValidateRequired()]
		public virtual string LastName { get; set; }

		/// <summary>
		/// The number of orders associated with this customer.
		/// </summary>
		[JsonProperty("orders_count")]
		[ShouldNotSerialize]
		public int? OrdersCount { get; private set; }

		/// <summary>
		/// The state of the customer's account with a shop. Default value: disabled. Valid values:
		/// disabled: The customer doesn't have an active account. Customer accounts can be disabled from the Shopify admin at any time.
		/// invited: The customer has received an email invite to create an account.
		/// enabled: The customer has created an account.
		/// declined: The customer declined the email invite to create an account.
		/// </summary>
		[JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.State, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public virtual CustomerState? Status { get; private set; } = CustomerState.Disabled;

		/// <summary>
		/// The total amount of money that the customer has spent across their order history.
		/// </summary>
		[JsonProperty("total_spent")]
		[ShouldNotSerialize]
		public decimal? TotalSpent { get; private set; }

		/// <summary>
		/// The ID of the customer's last order.
		/// </summary>
		[JsonProperty("last_order_id")]
		[ShouldNotSerialize]
		public long? LastOrderId { get; private set; }

		/// <summary>
		/// A note about the customer.
		/// </summary>
		[JsonProperty("note", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Note, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public virtual string Note { get; set; }

		[JsonProperty("verified_email", NullValueHandling = NullValueHandling.Ignore)]
		public bool? VerifiedEmail { get; private set; }

		/// <summary>
		/// Whether the customer is exempt from paying taxes on their order. If true, then taxes won't be applied to an order at checkout. If false, then taxes will be applied at checkout.
		/// </summary>
		[JsonProperty("tax_exempt", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.TaxExempt, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public bool? TaxExempt { get; set; }

		/// <summary>
		/// The unique phone number (E.164 format) for this customer. Attempting to assign the same phone number to multiple customers returns an error. 
		/// The property can be set using different formats, but each format must represent a number that can be dialed from anywhere in the world. The following formats are all valid:
		///6135551212,
		///+16135551212,
		///(613)555-1212,
		///+1 613-555-1212
		/// </summary>
		[PIIData]
		[JsonProperty("phone", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Phone, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public virtual string Phone { get; set; }

		/// <summary>
		/// Tags that the shop owner has attached to the customer, formatted as a string of comma-separated values. A customer can have up to 250 tags. Each tag can have up to 255 characters.
		/// </summary>
		[JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Tags, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public virtual string Tags { get; set; }

		/// <summary>
		/// The name of the customer's last order. This is directly related to the name field on the Order resource.
		/// </summary>
		[JsonProperty("last_order_name")]
		[ShouldNotSerialize]
		public string LastOrderName { get; private set; }

		/// <summary>
		/// The three-letter code (ISO 4217 format) for the currency that the customer used when they paid for their last order. Defaults to the shop currency. Returns the shop currency for test orders.
		/// </summary>
		[JsonProperty("currency")]
		[CommerceDescription(ShopifyCaptions.Currency, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		[ShouldNotSerialize]
		public string Currency { get; private set; }

		/// <summary>
		/// Whether the customer is exempt from paying specific taxes on their order. Canadian taxes only. Valid values:
		/// EXEMPT_ALL: This customer is exempt from all Canadian taxes.
		///CA_STATUS_CARD_EXEMPTION: This customer is exempt from specific taxes for holding a valid STATUS_CARD_EXEMPTION in Canada.
		///CA_DIPLOMAT_EXEMPTION: This customer is exempt from specific taxes for holding a valid DIPLOMAT_EXEMPTION in Canada.
		///CA_BC_RESELLER_EXEMPTION: This customer is exempt from specific taxes for holding a valid RESELLER_EXEMPTION in British Columbia.
		///CA_MB_RESELLER_EXEMPTION: This customer is exempt from specific taxes for holding a valid RESELLER_EXEMPTION in Manitoba.
		///CA_SK_RESELLER_EXEMPTION: This customer is exempt from specific taxes for holding a valid RESELLER_EXEMPTION in Saskatchewan.
		///CA_BC_COMMERCIAL_FISHERY_EXEMPTION: This customer is exempt from specific taxes for holding a valid COMMERCIAL_FISHERY_EXEMPTION in British Columbia.
		///CA_MB_COMMERCIAL_FISHERY_EXEMPTION: This customer is exempt from specific taxes for holding a valid COMMERCIAL_FISHERY_EXEMPTION in Manitoba.
		///CA_NS_COMMERCIAL_FISHERY_EXEMPTION: This customer is exempt from specific taxes for holding a valid COMMERCIAL_FISHERY_EXEMPTION in Nova Scotia.
		///CA_PE_COMMERCIAL_FISHERY_EXEMPTION: This customer is exempt from specific taxes for holding a valid COMMERCIAL_FISHERY_EXEMPTION in Prince Edward Island.
		///CA_SK_COMMERCIAL_FISHERY_EXEMPTION: This customer is exempt from specific taxes for holding a valid COMMERCIAL_FISHERY_EXEMPTION in Saskatchewan.
		///CA_BC_PRODUCTION_AND_MACHINERY_EXEMPTION: This customer is exempt from specific taxes for holding a valid PRODUCTION_AND_MACHINERY_EXEMPTION in British Columbia.
		///CA_SK_PRODUCTION_AND_MACHINERY_EXEMPTION: This customer is exempt from specific taxes for holding a valid PRODUCTION_AND_MACHINERY_EXEMPTION in Saskatchewan.
		///CA_BC_SUB_CONTRACTOR_EXEMPTION: This customer is exempt from specific taxes for holding a valid SUB_CONTRACTOR_EXEMPTION in British Columbia.
		///CA_SK_SUB_CONTRACTOR_EXEMPTION: This customer is exempt from specific taxes for holding a valid SUB_CONTRACTOR_EXEMPTION in Saskatchewan.
		///CA_BC_CONTRACTOR_EXEMPTION: This customer is exempt from specific taxes for holding a valid CONTRACTOR_EXEMPTION in British Columbia.
		///CA_SK_CONTRACTOR_EXEMPTION: This customer is exempt from specific taxes for holding a valid CONTRACTOR_EXEMPTION in Saskatchewan.
		///CA_ON_PURCHASE_EXEMPTION: This customer is exempt from specific taxes for holding a valid PURCHASE_EXEMPTION in Ontario.
		///CA_MB_FARMER_EXEMPTION: This customer is exempt from specific taxes for holding a valid FARMER_EXEMPTION in Manitoba.
		///CA_NS_FARMER_EXEMPTION: This customer is exempt from specific taxes for holding a valid FARMER_EXEMPTION in Nova Scotia.
		///CA_SK_FARMER_EXEMPTION: This customer is exempt from specific taxes for holding a valid FARMER_EXEMPTION in Saskatchewan.
		        /// </summary>
		[JsonProperty("tax_exemptions", NullValueHandling = NullValueHandling.Ignore)]
		//[CommerceDescription(ShopifyCaptions.TaxExemptions)]
		public string[] TaxExemptions { get; set; }

		/// <summary>
		/// A list of the ten most recently updated addresses for the customer.
		/// </summary>
		[JsonProperty("addresses", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.CustomerAddress, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
        public IList<CustomerAddressData> Addresses { get; set; }

		/// <summary>
		/// The default address for the customer.
		/// </summary>
		[JsonProperty("default_address")]
		[ShouldNotSerialize]
		public CustomerAddressData DefaultAddress { get; private set; }

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
        public List<MetafieldData> Metafields { get; set; }

        public void AddAddresses(CustomerAddressData address)
        {
			if(address != null)
				Addresses.Add(address);
        }

        public void AddAddresses(List<CustomerAddressData> addresses)
        {
            if (addresses != null && addresses.Count > 0)
            {
                ((List<CustomerAddressData>)Addresses).AddRange(addresses);
            }
        }

        //Conditional Serialization
        public bool ShouldSerializeId()
        {
            return Id.HasValue && Id > 0;
        }
	}
}
