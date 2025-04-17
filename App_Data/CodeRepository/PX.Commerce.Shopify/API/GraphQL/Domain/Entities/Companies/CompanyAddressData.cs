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
	/// <summary>
	/// Represents a billing or shipping address for a company location.
	/// </summary>
	public class CompanyAddressDataGQL : BCAPIEntity, INode
	{
		/// <summary>
		/// The first line of the address. Typically the street address or PO Box number.
		/// </summary>
		[JsonProperty("address1")]
		[GraphQLField("address1", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Address1 { get; set; }

		/// <summary>
		/// The second line of the address. Typically the number of the apartment, suite, or unit.
		/// </summary>
		[JsonProperty("address2")]
		[GraphQLField("address2", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Address2 { get; set; }

		/// <summary>
		/// The name of the city, district, village, or town.
		/// </summary>
		[JsonProperty("city")]
		[GraphQLField("city", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string City { get; set; }

		/// <summary>
		/// The name of the country.
		/// </summary>
		[JsonProperty("country")]
		[GraphQLField("country", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Country { get; set; }

		/// <summary>
		/// The two-letter code for the country of the address. For example, US.
		/// </summary>
		[JsonProperty("countryCode")]
		[GraphQLField("countryCode", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string CountryCode { get; set; }

		/// <summary>
		/// The date and time (ISO 8601 format) at which the company address was created.
		/// </summary>
		[JsonProperty("createdAt")]
		[GraphQLField("createdAt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public DateTime? CreatedAt { get; set; }

		/// <summary>
		/// The formatted version of the address.
		/// </summary>
		[JsonProperty("formattedAddress")]
		[GraphQLField("formattedAddress", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public List<string> FormattedAddress { get; set; }

		/// <summary>
		/// A comma-separated list of the values for city, province, and country.
		/// </summary>
		[JsonProperty("formattedArea")]
		[GraphQLField("formattedArea", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string FormattedArea { get; set; }

		///<inheritdoc/>
		[JsonProperty("id")]
		[GraphQLField("id", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.ID)]
		public string Id { get; set; }

		/// <summary>
		/// A unique phone number for the customer. Formatted using E.164 standard. For example, +16135551111.
		/// </summary>
		[JsonProperty("phone")]
		[GraphQLField("phone", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Phone { get; set; }

		/// <summary>
		/// The region of the address, such as the province, state, or district.
		/// </summary>
		[JsonProperty("province")]
		[GraphQLField("province", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Province { get; set; }

		/// <summary>
		/// The identity of the recipient e.g. 'Receiving Department'.
		/// </summary>
		[JsonProperty("recipient")]
		[GraphQLField("recipient", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Recipient { get; set; }

		/// <summary>
		/// The date and time (ISO 8601 format) at which the company address was last updated.
		/// </summary>
		[JsonProperty("updatedAt")]
		[GraphQLField("updatedAt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.DateTime)]
		public DateTime? UpdatedAt { get; set; }

		/// <summary>
		/// The zip or postal code of the address.
		/// </summary>
		[JsonProperty("zip")]
		[GraphQLField("zip", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Zip { get; set; }

		/// <summary>
		/// The two-letter code for the region. For example, ON.
		/// </summary>
		[JsonProperty("zoneCode")]
		[GraphQLField("zoneCode", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string ZoneCode { get; set; }

		[JsonIgnore]
		public Guid? LocalID { get; set; }
	}
}
