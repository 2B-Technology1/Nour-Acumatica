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
using System.Collections.Generic;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// The fields used to create or update a mailing address.
	/// </summary>
	public class MailingAddressInput
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
		/// The name of the customer's company or organization.
		/// </summary>
		[JsonProperty("company", NullValueHandling = NullValueHandling.Ignore)]
		public string Company { get; set; }

		/// <summary>
		/// The two-letter code for the country of the address. For example, US.
		/// </summary>
		[JsonProperty("countryCode", NullValueHandling = NullValueHandling.Ignore)]
		public string CountryCode { get; set; }

		/// <summary>
		/// Property used to set value in GraphQLQueryBuilder.
		/// </summary>
		[JsonIgnore]
		[JsonProperty("countryCodeV2", NullValueHandling = NullValueHandling.Ignore)]
		public string CountryCodeV2 { get => CountryCode; set => CountryCode = value; }

		/// <summary>
		/// The first name of the customer.
		/// </summary>
		[JsonProperty("firstName", NullValueHandling = NullValueHandling.Ignore)]
		public string FirstName { get; set; }

		///<inheritdoc/>
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		public string Id { get; set; }

		/// <summary>
		/// The last name of the customer.
		/// </summary>
		[JsonProperty("lastName", NullValueHandling = NullValueHandling.Ignore)]
		public string LastName { get; set; }

		/// <summary>
		/// A unique phone number for the customer. Formatted using E.164 standard.For example, +16135551111.
		/// </summary>
		[JsonProperty("phone", NullValueHandling = NullValueHandling.Ignore)]
		public string Phone { get; set; }

		/// <summary>
		/// The region of the address, such as the province, state, or district.
		/// </summary>
		[JsonProperty("province", NullValueHandling = NullValueHandling.Ignore)]
		public string Province { get; set; }

		/// <summary>
		/// The two-letter code for the region.	For example, ON.
		/// </summary>
		[JsonProperty("provinceCode", NullValueHandling = NullValueHandling.Ignore)]
		public string ProvinceCode { get; set; }

		/// <summary>
		/// The zip or postal code of the address.
		/// </summary>
		[JsonProperty("zip", NullValueHandling = NullValueHandling.Ignore)]
		public string Zip { get; set; }

		[JsonIgnore]
		public bool Default { get; set; }
	}
}
