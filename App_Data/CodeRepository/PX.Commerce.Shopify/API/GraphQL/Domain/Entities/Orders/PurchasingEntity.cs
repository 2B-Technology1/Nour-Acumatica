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
using System;
using static PX.Commerce.Core.BCEntitiesAttribute;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// Represents information about the purchasing entity for the order or draft order.
	/// This is the union object in Shopify, it may be a CustomerData object, maybe a PurchasingCompany object
	/// </summary>
	public class PurchasingEntity
	{
		#region CustomerData
		
		[JsonProperty("email")]
		[GraphQLField("email", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String, "Customer")]
		public string Email { get; set; }

		[JsonProperty("id")]
		[GraphQLField("id", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String, "Customer")]
		public string Id { get; set; }

		[JsonProperty("firstName")]
		[GraphQLField("firstName", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String, "Customer")]
		public string FirstName { get; set; }

		[JsonProperty("lastName")]
		[GraphQLField("lastName", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String, "Customer")]
		public string LastName { get; set; }

		[JsonProperty("legacyResourceId")]
		[GraphQLField("legacyResourceId", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String, "Customer")]
		public string LegacyResourceId { get; set; }

		[JsonProperty("phone")]
		[GraphQLField("phone", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String, "Customer")]
		public string Phone { get; set; }

		[JsonProperty("updatedAt")]
		[GraphQLField("updatedAt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.DateTime, "Customer")]
		public DateTime? UpdatedAt { get; set; }

		#endregion

		#region PurchasingCompanyData

		[JsonProperty("company")]
		[GraphQLField("company", GraphQLConstants.DataType.Object, typeof(CompanyDataGQL), UnionObjectType = "PurchasingCompany")]
		public CompanyDataGQL Company { get; set; }

		[JsonProperty("contact")]
		[GraphQLField("contact", GraphQLConstants.DataType.Object, typeof(CompanyContactDataGQL), UnionObjectType = "PurchasingCompany")]
		public CompanyContactDataGQL CompanyContact { get; set; }

		[JsonProperty("location")]
		[GraphQLField("location", GraphQLConstants.DataType.Object, typeof(CompanyLocationDataGQL), UnionObjectType = "PurchasingCompany")]
		public CompanyLocationDataGQL CompanyLocation { get; set; }
		#endregion
	}

	/// <summary>
	/// Represents information about the purchasing company for the order or draft order.
	/// </summary>
	public class PurchasingCompany
	{
		/// <summary>
		/// The company associated to the order or draft order.
		/// </summary>
		[JsonProperty("company")]
		[GraphQLField("company", GraphQLConstants.DataType.Object, typeof(CompanyDataGQL))]
		public CompanyDataGQL Company { get; set; }

		/// <summary>
		/// The company contact associated to the order or draft order.
		/// </summary>
		[JsonProperty("contact")]
		[GraphQLField("contact", GraphQLConstants.DataType.Object, typeof(CompanyContactDataGQL))]
		public CompanyContactDataGQL CompanyContact { get; set; }

		/// <summary>
		/// The company location associated to the order or draft order.
		/// </summary>
		[JsonProperty("location")]
		[GraphQLField("location", GraphQLConstants.DataType.Object, typeof(CompanyLocationDataGQL))]
		public CompanyLocationDataGQL CompanyLocation { get; set; }
	}
}
