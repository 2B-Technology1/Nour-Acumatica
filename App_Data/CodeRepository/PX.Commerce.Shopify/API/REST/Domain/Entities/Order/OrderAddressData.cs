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

namespace PX.Commerce.Shopify.API.REST
{

	[JsonObject(Description = "Order Address")]
	[CommerceDescription(ShopifyCaptions.OrderAddress, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class OrderAddressData : BCAPIEntity
	{
		/// <summary>
		/// The first name of the person associated with the payment method.
		/// </summary>
		[PIIData]
		[JsonProperty("first_name", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.FirstName, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public virtual string FirstName { get; set; }

		/// <summary>
		/// The last name of the person associated with the payment method.
		/// </summary>
		[PIIData]
		[JsonProperty("last_name", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.LastName, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public virtual string LastName { get; set; }

		/// <summary>
		/// The full name of the person associated with the payment method.
		/// </summary>
		[PIIData]
		[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Name, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public virtual string Name { get; set; }

		/// <summary>
		/// The company of the person associated with the address.
		/// </summary>
		[PIIData]
		[JsonProperty("company", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.CompanyName, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public virtual string Company { get; set; }

		/// <summary>
		/// The street address of the address.
		/// </summary>
		[PIIData]
		[JsonProperty("address1", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.AddressLine1, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public string Address1 { get; set; }

		/// <summary>
		/// An optional additional field for the street address of the address.
		/// </summary>
		[PIIData]
		[JsonProperty("address2", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.AddressLine2, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		[ValidateRequired(AutoDefault = true)]
		public string Address2 { get; set; }

		/// <summary>
		/// The city, town, or village of the address.
		/// </summary>
		[PIIData]
		[JsonProperty("city", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.City, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		[ValidateRequired(AutoDefault = true)]
		public virtual string City { get; set; }

		/// <summary>
		/// The name of the region (province, state, prefecture, …) of the address.
		/// </summary>
		[JsonProperty("province", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Province, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		[ValidateRequired(AutoDefault = true)]
		public virtual string Province { get; set; }

		/// <summary>
		/// The postal code (zip, postcode, Eircode, …) of the address.
		/// </summary>
		[PIIData]
		[JsonProperty("zip", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.PostalCode, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		[ValidateRequired(AutoDefault = true)]
		public virtual string PostalCode { get; set; }

		/// <summary>
		/// The name of the country of the address.
		/// </summary>
		[JsonProperty("country", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Country, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		[ValidateRequired()]
		public virtual string Country { get; set; }

		/// <summary>
		/// The two-letter code (ISO 3166-1 format) for the country of the address.
		/// </summary>
		[JsonProperty("country_code", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.CountryISOCode, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public string CountryCode { get; set; }

		/// <summary>
		/// The two-letter abbreviation of the region of the address.
		/// </summary>
		[PIIData]
		[JsonProperty("province_code", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.ProvinceCode, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public string ProvinceCode { get; set; }

		/// <summary>
		/// The phone number at the address.
		/// </summary>
		[PIIData]
		[JsonProperty("phone", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.PhoneNumber, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		[ValidateRequired(AutoDefault = true)]
		public virtual string Phone { get; set; }

		/// <summary>
		/// The latitude of the address.
		/// </summary>
		[JsonProperty("latitude", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Latitude, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public virtual string Latitude { get; set; }

		/// <summary>
		/// The longitude of the address.
		/// </summary>
		[JsonProperty("longitude", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Longitude, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public virtual string Longitude { get; set; }

		/// <summary>
		/// The ID for the shipping zone that the address belongs to.
		/// </summary>
		[JsonProperty("shipping_zone_id")]
		[CommerceDescription(ShopifyCaptions.ShippingZoneId, FieldFilterStatus.Skipped, FieldMappingStatus.Skipped)]
		[JsonIgnore]
		public long? ShippingZoneId { get; set; }
	}

}
