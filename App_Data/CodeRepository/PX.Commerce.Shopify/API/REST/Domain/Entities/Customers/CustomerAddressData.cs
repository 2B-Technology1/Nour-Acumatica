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
using System.ComponentModel;
using System.Collections.Generic;

namespace PX.Commerce.Shopify.API.REST
{
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class CustomerAddressResponse : IEntityResponse<CustomerAddressData>
	{
		[JsonProperty("customer_address")]
		public CustomerAddressData Data { get; set; }
	}
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class CustomerAddressesResponse : IEntitiesResponse<CustomerAddressData>
	{
		[JsonProperty("addresses")]
		public IEnumerable<CustomerAddressData> Data { get; set; }
	}

	[JsonObject(Description = "Customer -> Customer Address")]
	[CommerceDescription(ShopifyCaptions.CustomerAddressData, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]

	public class CustomerAddressData : BCAPIEntity
	{
		/// <summary>
		/// A unique identifier for the address.
		/// </summary>
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.LocationId, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		public virtual long? Id { get; set; }

		/// <summary>
		/// A unique identifier for the customer where the address attaches to
		/// </summary>
		[JsonProperty("customer_id", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.CustomerId, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		public virtual long? CustomerId { get; set; }

		/// <summary>
		/// The customer’s first name.
		/// </summary>
		[PIIData]
		[JsonProperty("first_name", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.FirstName, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public virtual string FirstName { get; set; }

		/// <summary>
		/// The customer’s last name.
		/// </summary>
		[PIIData]
		[JsonProperty("last_name", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.LastName, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public virtual string LastName { get; set; }

		/// <summary>
		/// The customer’s first and last names.
		/// </summary>
		[PIIData]
		[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Name, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public virtual string Name { get; set; }

		/// <summary>
		/// The customer’s company.
		/// </summary>
		[PIIData]
		[JsonProperty("company", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.CompanyName, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public virtual string Company { get; set; }

		/// <summary>
		/// The customer's mailing address
		/// </summary>
		[PIIData]
		[JsonProperty("address1", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.AddressLine1, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public string Address1 { get; set; }

		/// <summary>
		/// An additional field for the customer's mailing address.
		/// </summary>
		[PIIData]
		[JsonProperty("address2", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.AddressLine2, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		[ValidateRequired(AutoDefault = true)]
		public string Address2 { get; set; }

		/// <summary>
		/// The customer's city, town, or village.
		/// </summary>
		[PIIData]
		[JsonProperty("city", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.City, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		[ValidateRequired(AutoDefault = true)]
		public virtual string City { get; set; }

		/// <summary>
		/// The customer’s region name. Typically a province, a state, or a prefecture.
		/// </summary>
		[PIIData]
		[JsonProperty("province", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Province, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		[ValidateRequired(AutoDefault = true)]
		public virtual string Province { get; set; }

		/// <summary>
		/// The customer’s postal code, also known as zip, postcode, Eircode, etc.
		/// </summary>
		[PIIData]
		[JsonProperty("zip", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.PostalCode, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		[ValidateRequired(AutoDefault = true)]
		public virtual string PostalCode { get; set; }

		/// <summary>
		/// The customer's country.
		/// </summary>
		[PIIData]
		[JsonProperty("country", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Country, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		[ValidateRequired()]
		public virtual string Country { get; set; }

		/// <summary>
		/// The customer’s normalized country name.
		/// </summary>
		[PIIData]
		[JsonProperty("country_name", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.CountryName, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		[ValidateRequired()]
		public virtual string CountryName { get; set; }

		/// <summary>
		/// The two-letter country code corresponding to the customer's country.
		/// </summary>
		[JsonProperty("country_code", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.CountryISOCode, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public string CountryCode { get; set; }

		/// <summary>
		/// The two-letter code for the customer’s region.
		/// </summary>
		[JsonProperty("province_code", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.ProvinceCode, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public string ProvinceCode { get; set; }

		/// <summary>
		/// The customer’s phone number at this address.
		/// </summary>
		[PIIData]
		[JsonProperty("phone", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.PhoneNumber, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		[ValidateRequired(AutoDefault = true)]
		public virtual string Phone { get; set; }

		/// <summary>
		/// Whether this address is the default address for the customer.
		/// </summary>
		[JsonProperty("default", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.IsDefault, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public virtual bool? Default { get; set; }
	}
}
