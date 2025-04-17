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

using System.Linq;
using Newtonsoft.Json;
using PX.Commerce.Core;

namespace PX.Commerce.Shopify.API.GraphQL
{
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class ProductVariantNode : EntityNodeResponse<ProductVariantGQL> {}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class ProductVariantsResponseData : EntitiesResponse<ProductVariantGQL, ProductVariantNode> {}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class ProductVariantsResponse : IEntitiesResponses<ProductVariantGQL, ProductVariantsResponseData>
	{
		[JsonProperty("productVariants")]
		public ProductVariantsResponseData TEntitiesData { get; set; }

	}

	[GraphQLObject(NodeName = "productVariant", ConnectionName = "productVariants")]
	public class ProductVariantGQL : BCAPIEntity, INode
	{
		[JsonProperty("id")]
		[GraphQLField("id", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.ID)]
		public string Id { get; set; }

		[JsonProperty("sku")]
		[GraphQLField("sku", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Sku { get; set; }

		[JsonIgnore]
		public string IdNumber => Id?.Split('/').LastOrDefault();

		[JsonProperty("product")]
		[GraphQLField("product", GraphQLConstants.DataType.Object, typeof(ProductDataGQL))]
		public ProductDataGQL Product { get; set; }
	}
}
