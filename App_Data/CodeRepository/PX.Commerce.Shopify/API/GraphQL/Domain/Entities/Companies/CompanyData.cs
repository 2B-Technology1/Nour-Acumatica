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
	public class CompaniesResponseData : EntitiesResponse<CompanyDataGQL, CompanyNode>, INodeListResponse<CompanyDataGQL>, IEdgesResponse<CompanyDataGQL, CompanyNode>
	{
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class CompanyNode : EntityNodeResponse<CompanyDataGQL>
	{
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class CompanyResponse: IEntityResponse<CompanyDataGQL>
	{
		[JsonProperty("company")]
		public CompanyDataGQL TEntityData { get; set; }
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class CompaniesResponse : IEntitiesResponses<CompanyDataGQL, CompaniesResponseData>
	{
		[JsonProperty("companies")]
		public CompaniesResponseData TEntitiesData { get; set; } 
	}

	/// <summary>
	/// Represents information about a company which is also a customer of the shop.
	/// </summary>
	[GraphQLObject(NodeName = "company", ConnectionName = "companies")]
	[CommerceDescription(ShopifyCaptions.Company, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
	public class CompanyDataGQL : BCAPIEntity, INode
	{
		public class Arguments
		{
			[GraphQLArgument("sortKey", "CompanySortKeys")]
			public abstract class SortKey { }
		}

		/// <summary>
		/// The number of contacts that belong to the company.
		/// </summary>
		[JsonProperty("contactCount")]
		[GraphQLField("contactCount", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Int)]
		public int? ContactCount { get; set; }

		/// <summary>
		/// The list of roles for the company contacts.
		/// </summary>
		[JsonProperty("contactRoles")]
		[GraphQLField("contactRoles", GraphQLConstants.DataType.Connection, typeof(CompanyContactRoleDataGQL))]
		public Connection<CompanyContactRoleDataGQL> ContactRoles { get; set; }

		[JsonIgnore]
		public IEnumerable<CompanyContactRoleDataGQL> ContactRolesList { get => ContactRoles?.Nodes; }

		/// <summary>
		/// Returns the list of contacts in the company.
		/// </summary>
		[JsonProperty("contacts")]
		[GraphQLField("contacts", GraphQLConstants.DataType.Connection, typeof(CompanyContactDataGQL))]
		public Connection<CompanyContactDataGQL> Contacts { get; set; }

		[JsonIgnore]
		public IEnumerable<CompanyContactDataGQL> ContactsList { get => Contacts?.Nodes; }

		/// <summary>
		/// The date and time (ISO 8601 format) at which the company was created.
		/// </summary>
		[JsonProperty("createdAt")]
		[GraphQLField("createdAt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.DateTime)]
		public DateTime? CreatedAt { get; set; }

		/// <summary>
		/// The date and time (ISO 8601 format) at which the company became the customer.
		/// </summary>
		[JsonProperty("customerSince")]
		[GraphQLField("customerSince", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.DateTime)]
		public DateTime CustomerSince { get; set; }

		/// <inheritdoc/>
		[JsonProperty("defaultCursor")]
		[GraphQLField("defaultCursor", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string DefaultCursor { get; set; }

		/// <summary>
		/// The default contact role for the company.
		/// </summary>
		[JsonProperty("defaultRole")]
		[GraphQLField("defaultRole", GraphQLConstants.DataType.Object, typeof(CompanyContactRoleDataGQL))]
		public CompanyContactRoleDataGQL DefaultRole { get; set; }

		/// <summary>
		/// The external ID for the company.
		/// </summary>
		[JsonProperty("externalId")]
		[GraphQLField("externalId", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string ExternalId { get; set; }

		/// <summary>
		/// Whether the merchant added a timeline comment to the company.
		/// </summary>
		[JsonProperty("hasTimelineComment")]
		[GraphQLField("hasTimelineComment", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool? HasTimelineComment { get; set; }

		/// <summary>
		/// The lifetime duration of the company, since it became a customer of the shop.
		/// </summary>
		[JsonProperty("lifetimeDuration")]
		[GraphQLField("lifetimeDuration", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string LifetimeDuration { get; set; }

		/// <inheritdoc/>
		[JsonProperty("id")]
		[GraphQLField("id", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.ID)]
		[CommerceDescription(ShopifyCaptions.Id, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		public string Id { get; set; }

		/// <summary>
		/// The number of locations that belong to the company.
		/// </summary>
		[JsonProperty("locationCount")]
		[GraphQLField("locationCount", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Int)]
		public int? LocationCount { get; set; }

		/// <summary>
		/// Returns the list of locations in the company.
		/// </summary>
		[JsonProperty("locations")]
		[GraphQLField("locations", GraphQLConstants.DataType.Connection, typeof(CompanyLocationDataGQL))]
		public Connection<CompanyLocationDataGQL> Locations { get; set; }

		[JsonIgnore]
		public IEnumerable<CompanyLocationDataGQL> LocationsList { get => Locations?.Nodes; }

		/// <summary>
		/// The main contact person for the company.
		/// </summary>
		[JsonProperty("mainContact")]
		[GraphQLField("mainContact", GraphQLConstants.DataType.Object, typeof(CompanyContactDataGQL))]
		public CompanyContactDataGQL MainContact { get; set; }

		/// <summary>
		/// The name of the company.
		/// </summary>
		[JsonProperty("name")]
		[GraphQLField("name", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.Name, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public string Name { get; set; }

		/// <summary>
		/// A note about the company.
		/// </summary>
		[JsonProperty("note")]
		[GraphQLField("note", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.Note, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public string Note { get; set; }

		/// <summary>
		/// The total number of orders placed for this company, across all its locations.
		/// </summary>
		[JsonProperty("orderCount")]
		[GraphQLField("orderCount", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Int)]
		public int? OrderCount { get; set; }

		/// <summary>
		/// The total amount spent by this company, across all its locations.
		/// </summary>
		[JsonProperty("totalSpent")]
		[GraphQLField("totalSpent", GraphQLConstants.DataType.Object, typeof(Money))]
		public Money TotalSpent { get; set; }

		/// <summary>
		/// The date and time (ISO 8601 format) at which the company was last modified.
		/// </summary>
		[JsonProperty("updatedAt")]
		[GraphQLField("updatedAt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.DateTime)]
		public DateTime? UpdatedAt { get; set; }
	}


}
