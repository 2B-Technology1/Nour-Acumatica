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

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonObject(Description = "Order -> Billing Address")]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class OrderAddressData
    {
		/// <summary>
		/// The first name of the addressee
		/// 
		/// [string(255)]
		/// </summary>
		[PIIData]
		[JsonProperty("first_name")]
		[CommerceDescription(BigCommerceCaptions.FirstName, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public virtual string FirstName { get; set; }

		/// <summary>
		/// The last name of the addressee.
		/// 
		/// [string(255)]
		/// </summary>
		[PIIData]
		[JsonProperty("last_name")]
		[CommerceDescription(BigCommerceCaptions.LastName, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public virtual string LastName { get; set; }

		/// <summary>
		/// The company of the addressee.
		/// 
		/// [string(100)]
		/// </summary>
		[PIIData]
		[JsonProperty("company")]
		[CommerceDescription(BigCommerceCaptions.CompanyName, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public virtual string Company { get; set; }

		/// <summary>
		/// The first street line of the address.
		/// 
		/// [string(255)]
		/// </summary>
		[PIIData]
		[JsonProperty("street_1")]
		[CommerceDescription(BigCommerceCaptions.Street1, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public virtual string Street1 { get; set; }

		/// <summary>
		/// The second street line of the address.
		/// 
		/// [string(255)]
		/// </summary>
		[PIIData]
		[JsonProperty("street_2")]
		[CommerceDescription(BigCommerceCaptions.Street2, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public virtual string Street2 { get; set; }

		/// <summary>
		/// The city or suburb of the address.
		/// 
		/// [string(50)]
		/// </summary>
		[PIIData]
		[JsonProperty("city")]
		[CommerceDescription(BigCommerceCaptions.City, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public virtual string City { get; set; }

		/// <summary>
		/// The state of the address.
		/// 
		/// [string(50)]
		/// </summary>
		[JsonProperty("state")]
		[CommerceDescription(BigCommerceCaptions.State, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public virtual string State { get; set; }

		/// <summary>
		/// The zip or postcode of the address.
		/// 
		/// [string(50)]
		/// </summary>
		[PIIData]
		[JsonProperty("zip")]
		[CommerceDescription(BigCommerceCaptions.Zipcode, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public virtual string ZipCode { get; set; }

		/// <summary>
		/// The country of the address.
		/// 
		/// [string(50)]
		/// </summary>
		[JsonProperty("country")]
		[CommerceDescription(BigCommerceCaptions.Country, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public virtual string Country { get; set; }

		/// <summary>
		/// The country code of the country field.
		/// 
		/// [string(50)]
		/// </summary>
		[JsonProperty("country_iso2")]
        [Description(BigCommerceCaptions.CountryISOCode)]
        public virtual string CountryIso2 { get; set; }

		/// <summary>
		/// The phone number of the addressee.
		/// 
		/// [string(50)]
		/// </summary>
		[PIIData]
		[JsonProperty("phone")]
		[CommerceDescription(BigCommerceCaptions.PhoneNumber, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public virtual string Phone { get; set; }

		/// <summary>
		/// The email address of the addressee.
		/// 
		/// [string(255)]
		/// </summary>
		[PIIData]
		[JsonProperty("email")]
		[CommerceDescription(BigCommerceCaptions.EmailAddress, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public virtual string Email { get; set; }        
    }
}
