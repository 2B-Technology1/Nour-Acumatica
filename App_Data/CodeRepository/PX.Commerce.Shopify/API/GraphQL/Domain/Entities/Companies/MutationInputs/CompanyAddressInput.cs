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

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// Specifies the input fields to create or update the address of a company location.
	/// </summary>
	public class CompanyAddressInput
	{
		/// <summary>
		/// The first line of the address. Typically the street address or PO Box number.
		/// </summary>
		[JsonProperty("address1", NullValueHandling = NullValueHandling.Ignore)]
		public string Address1 { get; set; }

		/// <summary>
		/// The second line of the address. Typically the number of the apartment, suite, or unit.
		/// </summary>
		[JsonProperty("address2", NullValueHandling = NullValueHandling.Ignore)]
		public string Address2 { get; set; }

		/// <summary>
		/// The name of the city, district, village, or town.
		/// </summary>
		[JsonProperty("city", NullValueHandling = NullValueHandling.Ignore)]
		public string City { get; set; }

		/// <summary>
		/// The two-letter code ([ISO 3166-1 alpha-2]](https://en.wikipedia.org/wiki/ISO_3166-1_alpha-2) format)
		/// for the country of the address. For example, `US`` for the United States.
		/// </summary>
		[JsonProperty("countryCode", NullValueHandling = NullValueHandling.Ignore)]
		public string CountryCode { get; set; }

		/// <summary>
		/// A phone number for the recipient. Formatted using E.164 standard. For example, +16135551111.
		/// </summary>
		[JsonProperty("phone", NullValueHandling = NullValueHandling.Ignore)]
		public string Phone { get; set; }

		/// <summary>
		/// The zip or postal code of the address.
		/// </summary>
		[JsonProperty("zip", NullValueHandling = NullValueHandling.Ignore)]
		public string Zip { get; set; }

		/// <summary>
		/// The two-letter code ([ISO 3166-2 alpha-2]](https://en.wikipedia.org/wiki/ISO_3166-2) format)
		/// for the region of the address, such as the province, state, or district. For example, ON for Ontario, Canada.
		/// </summary>
		[JsonProperty("zoneCode", NullValueHandling = NullValueHandling.Ignore)]
		public string ZoneCode { get; set; }
	}
}
