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
	public abstract class BaseAddress: BCAPIEntity
	{
		/// <summary>
		/// The first line of the address. Typically the street address or PO Box number.
		/// </summary>
		[JsonProperty("address1")]
		[GraphQLField("address1", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.AddressLine1, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public string Address1 { get; set; }

		/// <summary>
		/// The second line of the address. Typically the number of the apartment, suite, or unit.
		/// </summary>
		[JsonProperty("address2")]
		[GraphQLField("address2", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.AddressLine2, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public string Address2 { get; set; }

		/// <summary>
		/// The name of the city, district, village, or town.
		/// </summary>
		[JsonProperty("city")]
		[GraphQLField("city", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.City, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public string City { get; set; }

		/// <summary>
		/// A unique phone number for the customer. Formatted using E.164 standard.For example, +16135551111.
		/// </summary>
		[JsonProperty("phone")]
		[GraphQLField("phone", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.PhoneNumber, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public string Phone { get; set; }

		/// <summary>
		/// The zip or postal code of the address.
		/// </summary>
		[JsonProperty("zip")]
		[GraphQLField("zip", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.PostalCode, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public string Zip { get; set; }
	}
}
