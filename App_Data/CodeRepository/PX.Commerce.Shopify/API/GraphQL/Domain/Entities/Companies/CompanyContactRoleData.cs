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
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class CompanyContactRoleResponse : IEntityResponse<CompanyContactRoleDataGQL>
	{
		[JsonProperty("companyContactRole")]
		public CompanyContactRoleDataGQL TEntityData { get; set; }
	}
	/// <summary>
	/// The role for a company contact.
	/// </summary>
	[GraphQLObject(NodeName = "companyContactRole")]
	public class CompanyContactRoleDataGQL : BCAPIEntity, INode
	{
		///<inheritdoc/>
		[JsonProperty("id")]
		[GraphQLField("id", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.ID)]
		public string Id { get; set; }

		/// <summary>
		/// The name of a role. For example, admin or buyer.
		/// </summary>
		[JsonProperty("name")]
		[GraphQLField("name", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Name { get; set; }

		/// <summary>
		/// A note for the role.
		/// </summary>
		[JsonProperty("note")]
		[GraphQLField("note", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Note { get; set; }

		[JsonIgnore]
		public Guid? LocalID { get; set; }
	}
}
