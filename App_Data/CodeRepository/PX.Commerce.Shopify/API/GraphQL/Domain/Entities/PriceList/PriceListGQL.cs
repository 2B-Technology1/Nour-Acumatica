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
using System.ComponentModel;
using Newtonsoft.Json;
using PX.Commerce.Core;

namespace PX.Commerce.Shopify.API.GraphQL
{
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PriceListsResponseData : EntitiesResponse<PriceListGQL, PriceListNode>, INodeListResponse<PriceListGQL>, IEdgesResponse<PriceListGQL, PriceListNode>
	{
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PriceListNode : EntityNodeResponse<PriceListGQL>
	{
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PriceListResponse : IEntityResponse<PriceListGQL>
	{
		[JsonProperty("priceList")]
		public PriceListGQL TEntityData { get; set; }
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PriceListsResponse : IEntitiesResponses<PriceListGQL, PriceListsResponseData>
	{
		[JsonProperty("priceLists")]
		public PriceListsResponseData TEntitiesData { get; set; }
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	[GraphQLObject(NodeName = "priceList", ConnectionName = "priceLists")]
	public class PriceListGQL : BCAPIEntity, INode
	{
		/// <summary>
		/// The currency for fixed prices associated with this price list.
		/// </summary>
		[JsonProperty("currency")]
		[GraphQLField("currency", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Currency { get; set; }

		/// <summary>
		/// The catalog associated with the price list.
		/// </summary>
		[JsonProperty("catalog")]
		[GraphQLField("catalog", GraphQLConstants.DataType.Object, typeof(CompanyLocationCatalogGQL))]
		public CompanyLocationCatalogGQL Catalog { get; set; }

		/// <summary>
		/// A globally-unique identifier.
		/// </summary>
		[JsonProperty("id")]
		[GraphQLField("id", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.ID)]
		public string Id { get; set; }

		/// <summary>
		/// The number of fixed prices on the price list.
		/// </summary>
		[JsonProperty("fixedPricesCount")]
		[GraphQLField("fixedPricesCount", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Int)]
		public int FixedPricesCount { get; set; }

		/// <summary>
		/// The unique name of the price list, used as a human-readable identifier.
		/// </summary>
		[JsonProperty("name")]
		[GraphQLField("name", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Name { get; set; }

		/// <summary>
		/// Relative adjustments to other prices.
		/// </summary>
		[JsonProperty("parent")]
		[GraphQLField("parent", GraphQLConstants.DataType.Object, typeof(PriceListParentGQL))]
		public PriceListParentGQL PriceListParent { get; set; }

		/// <summary>
		/// A list of prices associated with the price list.
		/// </summary>
		[JsonProperty("prices", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("prices", GraphQLConstants.DataType.Connection, typeof(PriceListPriceGQL))]
		public Connection<PriceListPriceGQL> Prices { get; set; }

		public IEnumerable<PriceListPriceGQL> PricesList { get => Prices?.Nodes; }
	}
}
