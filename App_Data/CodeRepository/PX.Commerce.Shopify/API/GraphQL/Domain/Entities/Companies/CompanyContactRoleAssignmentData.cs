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

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// Represents information about a company contact role assignment.
	/// </summary>
	public class CompanyContactRoleAssignmentDataGQL : BCAPIEntity, INode
	{
		/// <summary>
		/// The company this role assignment belongs to.
		/// </summary>
		[JsonProperty("company")]
		[GraphQLField("company", GraphQLConstants.DataType.Object, typeof(CompanyDataGQL))]
		public CompanyDataGQL Company { get; set; }

		/// <summary>
		/// The company contact for whom this role is assigned.
		/// </summary>
		[JsonProperty("companyContact")]
		[GraphQLField("companyContact", GraphQLConstants.DataType.Object, typeof(CompanyContactDataGQL))]
		public CompanyContactDataGQL CompanyContact { get; set; }

		/// <summary>
		/// The company location to which the role is assigned.
		/// </summary>
		[JsonProperty("companyLocation")]
		[GraphQLField("companyLocation", GraphQLConstants.DataType.Object, typeof(CompanyLocationDataGQL))]
		public CompanyLocationDataGQL CompanyLocation { get; set; }

		/// <summary>
		/// The date and time (ISO 8601 format) at which the assignment record was created.
		/// </summary>
		[JsonProperty("createdAt")]
		[GraphQLField("createdAt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.DateTime)]
		public DateTime? CreatedAt { get; set; }

		/// <summary>
		/// A globally-unique identifier.
		/// </summary>
		[JsonProperty("id")]
		[GraphQLField("id", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.ID)]
		public string Id { get; set; }

		/// <summary>
		/// The role that is assigned.
		/// </summary>
		[JsonProperty("role")]
		[GraphQLField("role", GraphQLConstants.DataType.Object, typeof(CompanyContactRoleDataGQL))]
		public CompanyContactRoleDataGQL Role { get; set; }

		/// <summary>
		/// The date and time (ISO 8601 format) at which the assignment record was last updated.
		/// </summary>
		[JsonProperty("updatedAt")]
		[GraphQLField("updatedAt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.DateTime)]
		public DateTime? UpdatedAt { get; set; }

		[JsonIgnore]
		public Guid? LocalID { get; set; }
	}
}
