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
using PX.Commerce.Shopify.API;
using PX.Commerce.Shopify.API.GraphQL;
using PX.Commerce.Shopify.API.REST;
using PX.Data.Api.Models;
using System.Collections.Generic;

namespace PX.Commerce.Shopify.API.GraphQL
{
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class MetafieldsDefinitionResponseData : EntitiesResponse<MetafieldDefintionGQL, MetafieldsDefinitionNode>, INodeListResponse<MetafieldDefintionGQL>
	{
	}
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class MetafieldsDefinitionNode : EntityNodeResponse<MetafieldDefintionGQL>
	{
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class MetafieldsDefinitionResponse : IEntitiesResponses<MetafieldDefintionGQL, MetafieldsDefinitionResponseData>
	{
		[JsonProperty("metafieldDefinitions")]
		public MetafieldsDefinitionResponseData TEntitiesData { get; set; }
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	[GraphQLObject(NodeName = "metafieldDefinition", ConnectionName = "metafieldDefinitions")]
	public class MetafieldDefintionGQL : BCAPIEntity, INode
	{
		[JsonProperty("ownerType")]
		[GraphQLField("ownerType", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string OwnerType { get; set; }

		[JsonProperty("name")]
		[GraphQLField("name", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Name { get; set; }

		[JsonProperty("namespace")]
		[GraphQLField("namespace", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Namespace { get; set; }

		[JsonProperty("key")]
		[GraphQLField("key", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Key { get; set; }

		[JsonProperty("type")]
		[GraphQLField("type", GraphQLConstants.DataType.Object, typeof(MetafieldType))]
		public MetafieldType Type { get; set; }
		public ShopifyMetafieldType InternalType { get; set; }
		public MetafieldCategory TypeCategory { get; set; }

		[JsonProperty("id")]
		[GraphQLField("id", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.ID)]
		public string Id { get; set; }
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class MetafieldValidation
	{
		[JsonProperty("name")]
		[GraphQLField("name", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Name { get; set; }
		[JsonProperty("value")]
		[GraphQLField("value", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Value { get; set; }
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class MetafieldType
	{
		[JsonProperty("name")]
		[GraphQLField("name", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Name { get; set; }

		[JsonProperty("category")]
		[GraphQLField("category", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Category { get; set; }
	}

	/// <summary>
	/// Enumerates all supported types on Shopify
	/// </summary>
	public enum ShopifyMetafieldType
	{
		boolean,
		collection_reference,
		color,
		date,
		date_time,
		dimension,
		file_reference,
		json,
		money,
		multi_line_text_field,
		number_decimal,
		number_integer,
		page_reference,
		product_reference,
		rating,
		single_line_text_field,
		url,
		variant_reference,
		volume,
		weight,
		NotSupportedShopifyType
	}

	public enum MetafieldCategory
	{
		SingleValue,
		List,
		jSon,
		jSonList
	}
}
