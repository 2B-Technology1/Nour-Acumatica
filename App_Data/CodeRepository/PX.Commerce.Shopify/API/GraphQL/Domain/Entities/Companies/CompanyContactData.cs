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
	public class CompanyContactResponse : IEntityResponse<CompanyContactDataGQL>
	{
		[JsonProperty("companyContact")]
		public CompanyContactDataGQL TEntityData { get; set; }
	}

	/// <summary>
	/// A person that represents a company.
	/// </summary>
	[GraphQLObject(NodeName = "companyContact")]
	[CommerceDescription(ShopifyCaptions.CompanyContactData, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
	public class CompanyContactDataGQL : BCAPIEntity, INode
	{
		/// <summary>
		/// The company to which the company contact belongs.
		/// </summary>
		[JsonProperty("company")]
		[GraphQLField("company", GraphQLConstants.DataType.Object, typeof(CompanyDataGQL))]
		[CommerceDescription(ShopifyCaptions.Company, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		public CompanyDataGQL Company { get; set; }

		/// <summary>
		/// The date and time (ISO 8601 format) at which the company contact was created.
		/// </summary>
		[JsonProperty("createdAt")]
		[GraphQLField("createdAt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.DateTime)]
		public DateTime? CreatedAt { get; set; }

		/// <summary>
		/// The customer record to which the company contact is associated.
		/// </summary>
		[JsonProperty("customer")]
		[GraphQLField("customer", GraphQLConstants.DataType.Object, typeof(CustomerDataGQL))]
		public CustomerDataGQL Customer { get; set; }

		///<inheritdoc/>
		[JsonProperty("id")]
		[GraphQLField("id", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.ID)]
		public string Id { get; set; }

		/// <summary>
		/// The Email of the company contact
		/// For mutation conversion only
		/// </summary>
		[JsonProperty("email")]
		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.Email, FieldFilterStatus.Filterable, FieldMappingStatus.Export)]
		public string Email { get; set; }

		/// <summary>
		/// The FirstName of the company contact
		/// For mutation conversion only
		/// </summary>
		[JsonProperty("firstName")]
		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.FirstName, FieldFilterStatus.Filterable, FieldMappingStatus.Export)]
		public string FirstName { get; set; }

		/// <summary>
		/// The company contact's last name.
		/// For mutation conversion only
		/// </summary>
		[JsonProperty("lastName")]
		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.LastName, FieldFilterStatus.Filterable, FieldMappingStatus.Export)]
		public string LastName { get; set; }

		/// <summary>
		/// Whether the given company contact is the main contact of the company.
		/// </summary>
		[JsonProperty("isMainContact")]
		[GraphQLField("isMainContact", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		[CommerceDescription(ShopifyCaptions.IsMainContact, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public bool? IsMainContact { get; set; }

		/// <summary>
		/// The lifetime duration of the company contact, since its creation date on Shopify.
		/// </summary>
		[JsonProperty("lifetimeDuration")]
		[GraphQLField("lifetimeDuration", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string LifetimeDuration { get; set; }

		/// <summary>
		/// The locale of the company contact.
		/// </summary>
		[JsonProperty("locale")]
		[GraphQLField("locale", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Locale { get; set; }

		/// <summary>
		/// Returns the list of roles assigned to this company contact.
		/// </summary>
		[JsonProperty("roleAssignments")]
		[GraphQLField("roleAssignments", GraphQLConstants.DataType.Connection, typeof(CompanyContactRoleAssignmentDataGQL))]
		public Connection<CompanyContactRoleAssignmentDataGQL> RoleAssignments { get; set; }

		[JsonIgnore]
		public IEnumerable<CompanyContactRoleAssignmentDataGQL> RoleAssignmentsList { get => RoleAssignments?.Nodes; }

		/// <summary>
		/// The title of the company contact.
		/// </summary>
		[JsonProperty("title")]
		[GraphQLField("title", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.Title, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
		public string Title { get; set; }

		/// <summary>
		/// The date and time (ISO 8601 format) at which the company contact was last updated.
		/// </summary>
		[JsonProperty("updatedAt")]
		[GraphQLField("updatedAt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.DateTime)]
		public DateTime? UpdatedAt { get; set; }

		[JsonIgnore]
		public Guid? LocalID { get; set; }

		[JsonIgnore]
		public int? LocalContactID { get; set; }
	}
}
