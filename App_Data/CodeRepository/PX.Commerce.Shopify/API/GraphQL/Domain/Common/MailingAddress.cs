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
using System.Collections.Generic;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// Represents a customer mailing address. For example, a customer's default address
	/// and an order's billing address are both mailing addresses.
	/// </summary>
	[JsonObject(Description = "Customer -> Customer Address")]
	[CommerceDescription(ShopifyCaptions.CustomerAddressData, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class MailingAddress: BaseAddress, INode
	{
		/// <summary>
		/// The name of the customer's company or organization.
		/// </summary>
		[PIIData]
		[JsonProperty("company")]
		[GraphQLField("company", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.CompanyName, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public string Company { get; set; }

		/// <summary>
		/// Whether the address coordinates are valid.
		/// </summary>
		[JsonProperty("coordinatesValidated")]
		[GraphQLField("coordinatesValidated", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool? CoordinatesValidated { get; set; }

		/// <summary>
		/// The name of the country.
		/// </summary>
		[PIIData]
		[JsonProperty("country")]
		[GraphQLField("country", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.Country, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public string Country { get; set; }

		/// <summary>
		/// The two-letter code for the country of the address. For example, US.
		/// </summary>
		[PIIData]
		[JsonProperty("countryCodeV2")]
		[GraphQLField("countryCodeV2", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.CountryISOCode, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public string CountryCode { get; set; }

		/// <summary>
		/// The first name of the customer.
		/// </summary>
		[PIIData]
		[JsonProperty("firstName")]
		[GraphQLField("firstName", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.FirstName, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public string FirstName { get; set; }

		/// <summary>
		/// A formatted version of the address, customized by the provided arguments.
		/// </summary>
		[PIIData]
		[JsonProperty("formatted")]
		[GraphQLField("formatted", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public List<string> Formatted { get; set; }

		/// <summary>
		/// A comma-separated list of the values for city, province, and country.
		/// </summary>
		[PIIData]
		[JsonProperty("formattedArea")]
		[GraphQLField("formattedArea", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string FormattedArea { get; set; }

		///<inheritdoc/>
		[JsonProperty("id")]
		[GraphQLField("id", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.ID)]
		[CommerceDescription(ShopifyCaptions.LocationId, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		public string Id { get; set; }

		/// <summary>
		/// The last name of the customer.
		/// </summary>
		[PIIData]
		[JsonProperty("lastName")]
		[GraphQLField("lastName", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.LastName, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public string LastName { get; set; }

		/// <summary>
		/// The latitude coordinate of the customer address.
		/// </summary>
		[PIIData]
		[JsonProperty("latitude")]
		[GraphQLField("latitude", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Float)]
		public decimal? Latitude { get; set; }

		/// <summary>
		/// The longitude coordinate of the customer address.
		/// </summary>
		[PIIData]
		[JsonProperty("longitude")]
		[GraphQLField("longitude", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Float)]
		public decimal? Longitude { get; set; }

		/// <summary>
		/// The full name of the customer, based on firstName and lastName.
		/// </summary>
		[PIIData]
		[JsonProperty("name")]
		[GraphQLField("name", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.Name, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public string Name { get; set; }

		/// <summary>
		/// The region of the address, such as the province, state, or district.
		/// </summary>
		[PIIData]
		[JsonProperty("province")]
		[GraphQLField("province", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.Province, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public string Province { get; set; }

		/// <summary>
		/// The two-letter code for the region.	For example, ON.
		/// </summary>
		[PIIData]
		[JsonProperty("provinceCode")]
		[GraphQLField("provinceCode", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.ProvinceCode, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public string ProvinceCode { get; set; }

		[JsonProperty("timeZone")]
		[GraphQLField("timeZone", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.TimeZone, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public string TimeZone { get; set; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.IsDefault, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public virtual bool? Default { get; set; }

		[JsonIgnore]
		public virtual string CustomerId { get; set; }

		public override bool Equals(object obj)
		{
			// If the passed object is null or is not MailingAddress, return False
			if (obj == null || !(obj is MailingAddress))
			{
				return false;
			}

			var objMailingAddr = obj as MailingAddress;

			return string.Equals(this.Id, objMailingAddr.Id, System.StringComparison.OrdinalIgnoreCase) ||
				(string.Equals(this.City?.Trim() ?? string.Empty, objMailingAddr.City?.Trim() ?? string.Empty, System.StringComparison.OrdinalIgnoreCase) &&
				 string.Equals(this.CountryCode?.Trim() ?? string.Empty, objMailingAddr.CountryCode?.Trim() ?? string.Empty, System.StringComparison.OrdinalIgnoreCase) &&
				 string.Equals(this.Phone?.Trim() ?? string.Empty, objMailingAddr.Phone?.Trim() ?? string.Empty, System.StringComparison.OrdinalIgnoreCase) &&
				 string.Equals(this.ProvinceCode?.Trim() ?? string.Empty, objMailingAddr.ProvinceCode?.Trim() ?? string.Empty, System.StringComparison.OrdinalIgnoreCase) &&
				 string.Equals(this.Address1?.Trim() ?? string.Empty, objMailingAddr.Address1?.Trim() ?? string.Empty, System.StringComparison.OrdinalIgnoreCase) &&
				 string.Equals(this.Address2?.Trim() ?? string.Empty, objMailingAddr.Address2?.Trim() ?? string.Empty, System.StringComparison.OrdinalIgnoreCase) &&
				 string.Equals(this.Zip?.Trim() ?? string.Empty, objMailingAddr.Zip?.Trim() ?? string.Empty, System.StringComparison.OrdinalIgnoreCase) &&
				 string.Equals(this.Company?.Trim() ?? string.Empty, objMailingAddr.Company?.Trim() ?? string.Empty, System.StringComparison.OrdinalIgnoreCase));
		}
	}
}
