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
	/// Metafields enable you to attach additional information to a Shopify resource, such as a Product or a Collection. 
	/// </summary>
	public class MetafieldGQL :  BCAPIEntity,INode
	{
		/// <summary>
		/// The date and time when the metafield was created.
		/// </summary>
		[JsonProperty("createdAt", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("createdAt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.DateTime)]
		public DateTime? CreatedAt { get; set; }

		/// <summary>
		/// The date and time when the metafield was updated.
		/// </summary>
		[JsonProperty("updatedAt", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("updatedAt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.DateTime)]
		public DateTime? UpdatedAt { get; set; }

		/// <summary>
		/// A globally-unique identifier.
		/// </summary>
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("id", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.ID)]
		public string Id { get; set; }

		/// <summary>
		/// The ID of the corresponding resource in the REST Admin API.
		/// </summary>
		[JsonProperty("legacyResourceId", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("legacyResourceId", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.UnsignedInt64)]
		public long? LegacyResourceId { get; set; }

		/// <summary>
		/// The description of the metafield.
		/// </summary>
		[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("description", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Description { get; set; }

		/// <summary>
		/// The key name of the metafield. Required when creating but optional when updating.
		/// </summary>
		[JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("key", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Key { get; set; }

		/// <summary>
		/// A container for a group of metafields. Grouping metafields within a namespace prevents your metafields from conflicting with other metafields that have the same key name.
		/// </summary>
		[JsonProperty("namespace", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("namespace", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Namespace { get; set; }

		/// <summary>
		/// The data to store in the metafield. The data is always stored as a string, regardless of the metafield's type.
		/// </summary>
		[JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("value", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Value { get; set; }

		/// <summary>
		/// The type of data that the metafield stores in the value field. See the list of supported types.
		/// </summary>
		[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("type", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Type { get; set; }
	}
}
