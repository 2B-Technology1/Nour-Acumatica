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

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// Represents the address of a location.
	/// </summary>
	public class LocationAddressGQL : BaseAddress
	{
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
		/// A formatted version of the address, customized by the provided arguments.
		/// </summary>
		[JsonProperty("formatted")]
		[GraphQLField("formatted", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public List<string> Formatted { get; set; }

		/// <summary>
		/// The latitude coordinate of the customer address.
		/// </summary>
		[JsonProperty("latitude")]
		[GraphQLField("latitude", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Float)]
		public decimal? Latitude { get; set; }

		/// <summary>
		/// The longitude coordinate of the customer address.
		/// </summary>
		[JsonProperty("longitude")]
		[GraphQLField("longitude", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Float)]
		public decimal? Longitude { get; set; }

		/// <summary>
		/// The region of the address, such as the province, state, or district.
		/// </summary>
		[JsonProperty("province")]
		[GraphQLField("province", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Province { get; set; }

		/// <summary>
		/// The two-letter code for the region.	For example, ON.
		/// </summary>
		[JsonProperty("provinceCode")]
		[GraphQLField("provinceCode", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string ProvinceCode { get; set; }
	}
}
