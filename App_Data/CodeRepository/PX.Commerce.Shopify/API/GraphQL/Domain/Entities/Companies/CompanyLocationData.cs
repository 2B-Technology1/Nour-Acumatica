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
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class CompanyLocationsResponseData : INodeListResponse<CompanyLocationDataGQL>
	{
		/// <summary>
		/// The response data part
		/// </summary>
		[JsonProperty("nodes")]
		public IEnumerable<CompanyLocationDataGQL> Nodes { get; set; }

		/// <summary>
		/// The response PageInfo part
		/// </summary>
		public PageInfo PageInfo { get; set; }
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class CompanyLocationResponse : IEntityResponse<CompanyLocationDataGQL>
	{
		[JsonProperty("companyLocation")]
		public CompanyLocationDataGQL TEntityData { get; set; }
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class CompanyLocationsResponse : IEntitiesResponses<CompanyLocationDataGQL, CompanyLocationsResponseData>
	{
		[JsonProperty("companyLocations")]
		public CompanyLocationsResponseData TEntitiesData { get; set; }
	}

	/// <summary>
	/// Represents a company's business location.
	/// </summary>
	[GraphQLObject(NodeName = "companyLocation", ConnectionName = "companyLocations")]
	[CommerceDescription(ShopifyCaptions.CompanyLocationData, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
	public class CompanyLocationDataGQL: BCAPIEntity, INode
	{
		/// <summary>
		/// The address used as billing address for the location.
		/// </summary>
		[JsonProperty("billingAddress")]
		[GraphQLField("billingAddress", GraphQLConstants.DataType.Object, typeof(CompanyAddressDataGQL))]
		[CommerceDescription(ShopifyCaptions.BillingAddress, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public CompanyAddressDataGQL BillingAddress { get; set; }

		/// <summary>
		/// The company that the company location belongs to.
		/// </summary>
		[JsonProperty("company")]
		[GraphQLField("company", GraphQLConstants.DataType.Object, typeof(CompanyDataGQL))]
		[CommerceDescription(ShopifyCaptions.Company, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public CompanyDataGQL Company { get; set; }

		/// <summary>
		/// The date and time (ISO 8601 format) at which the company location was created.
		/// </summary>
		[JsonProperty("createdAt")]
		[GraphQLField("createdAt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.DateTime)]
		[CommerceDescription(ShopifyCaptions.DateCreated, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		public DateTime? CreatedAt { get; set; }

		/// <summary>
		/// The location's currency based on the shipping address. If the shipping address is empty,
		/// then the value is the shop's primary market.
		/// </summary>
		[JsonProperty("currency")]
		[GraphQLField("currency", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.Currency, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public string Currency { get; set; }

		///<inheritdoc/>
		[JsonProperty("defaultCursor")]
		[GraphQLField("defaultCursor", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string DefaultCursor { get; set; }

		/// <summary>
		/// The external ID of the company location.
		/// </summary>
		[JsonProperty("externalId")]
		[GraphQLField("externalId", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string ExternalId { get; set; }

		///// <summary>
		///// Whether the company location has a price list with the specified ID.
		///// </summary>
		//[JsonProperty("hasPriceList")]
		//public bool? HasPriceList { get; set; }

		/// <summary>
		/// Whether the merchant added a timeline comment to the company location.
		/// </summary>
		[JsonProperty("hasTimelineComment")]
		[GraphQLField("hasTimelineComment", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool? HasTimelineComment { get; set; }

		///<inheritdoc/>
		[JsonProperty("id")]
		[GraphQLField("id", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.ID)]
		public string Id { get; set; }

		/// <summary>
		/// The preferred locale of the company location.
		/// </summary>
		[JsonProperty("locale")]
		[GraphQLField("locale", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Locale { get; set; }

		/// <summary>
		/// The name of the company location.
		/// </summary>
		[JsonProperty("name")]
		[GraphQLField("name", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.Name, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public string Name { get; set; }

		/// <summary>
		/// A note about the company location.
		/// </summary>
		[JsonProperty("note")]
		[GraphQLField("note", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.Note, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public string Note { get; set; }

		/// <summary>
		/// The total number of orders placed for the location.
		/// </summary>
		[JsonProperty("orderCount", ReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
		[GraphQLField("orderCount", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Int)]
		public int? OrderCount { get; set; }

		/// <summary>
		/// The phone number of the company location.
		/// </summary>
		[JsonProperty("phone")]
		[GraphQLField("phone", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.Phone, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public string Phone { get; set; }

		/// <summary>
		/// Returns the list of roles assigned to the company location.
		/// </summary>
		[JsonProperty("roleAssignments")]
		[GraphQLField("roleAssignments", GraphQLConstants.DataType.Connection, typeof(CompanyContactRoleAssignmentDataGQL))]
		public Connection<CompanyContactRoleAssignmentDataGQL> RoleAssignments { get; set; }

		[JsonIgnore]
		public IEnumerable<CompanyContactRoleAssignmentDataGQL> RoleAssignmentsList { get => RoleAssignments?.Nodes; }

		/// <summary>
		/// The address used as shipping address for the location.
		/// </summary>
		[JsonProperty("shippingAddress")]
		[GraphQLField("shippingAddress", GraphQLConstants.DataType.Object, typeof(CompanyAddressDataGQL))]
		[CommerceDescription(ShopifyCaptions.ShippingAddress, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public CompanyAddressDataGQL ShippingAddress { get; set; }

		/// <summary>
		/// The tax registration ID for the company location.
		/// </summary>
		[JsonProperty("taxRegistrationId")]
		[GraphQLField("taxRegistrationId", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string TaxRegistrationId { get; set; }

		/// <summary>
		/// The total amount spent by the location.
		/// </summary>
		[JsonProperty("totalSpent")]
		[GraphQLField("totalSpent", GraphQLConstants.DataType.Object, typeof(Money))]
		public Money TotalSpent { get; set; }

		/// <summary>
		/// The date and time (ISO 8601 format) at which the company location was last modified.
		/// </summary>
		[JsonProperty("updatedAt")]
		[GraphQLField("updatedAt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.DateTime)]
		public DateTime? UpdatedAt { get; set; }

		[JsonIgnore]
		public Guid? LocalID { get; set; }

		[JsonIgnore]
		public string LocalLocationCD { get; set; }

	}
}
