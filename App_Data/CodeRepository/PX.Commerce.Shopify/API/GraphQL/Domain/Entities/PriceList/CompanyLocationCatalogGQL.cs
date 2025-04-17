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
	public class CatalogsResponseData : EntitiesResponse<CompanyLocationCatalogGQL, CatalogNode>, INodeListResponse<CompanyLocationCatalogGQL>, IEdgesResponse<CompanyLocationCatalogGQL, CatalogNode>
	{
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class CatalogNode : EntityNodeResponse<CompanyLocationCatalogGQL>
	{
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class CatalogResponse : IEntityResponse<CompanyLocationCatalogGQL>
	{
		[JsonProperty("catalog")]
		public CompanyLocationCatalogGQL TEntityData { get; set; }
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class CatalogsResponse : IEntitiesResponses<CompanyLocationCatalogGQL, CatalogsResponseData>
	{
		[JsonProperty("catalogs")]
		public CatalogsResponseData TEntitiesData { get; set; }
	}

	[CommerceDescription(ShopifyCaptions.Catalog)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	[GraphQLObject(NodeName = "catalog", ConnectionName = "catalogs")]
	public class CompanyLocationCatalogGQL: BCAPIEntity, INode
	{
		/// <summary>
		/// A globally-unique identifier.
		/// </summary>
		[JsonProperty("id")]
		[GraphQLField("id", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.ID)]
		public string Id { get; set; }

		/// <summary>
		/// The price list associated with the catalog.
		/// </summary>
		[JsonProperty("priceList")]
		[GraphQLField("priceList", GraphQLConstants.DataType.Object, typeof(PriceListGQL))]
		public PriceListGQL PriceList { get; set; }

		/// <summary>
		/// A group of products and collections that's published to a catalog.
		/// </summary>
		[JsonProperty("publication", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("publication", GraphQLConstants.DataType.Object, typeof(PublicationGQL))]
		public PublicationGQL Publication { get; set; }

		/// <summary>
		/// The status of the catalog.
		/// </summary>
		[JsonProperty("status")]
		[GraphQLField("status", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Status { get; set; }

		/// <summary>
		/// The name of the catalog.
		/// </summary>
		[JsonProperty("title")]
		[GraphQLField("title", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Title { get; set; }

		/// <summary>
		/// The company locations associated with the catalog.
		/// </summary>
		[JsonProperty("companyLocations", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("companyLocations", GraphQLConstants.DataType.Connection, typeof(CompanyLocationDataGQL))]
		public Connection<CompanyLocationDataGQL> CompanyLocations { get; set; }

		public IEnumerable<CompanyLocationDataGQL> CompanyLocationsList { get => CompanyLocations?.Nodes; }
	}
}
