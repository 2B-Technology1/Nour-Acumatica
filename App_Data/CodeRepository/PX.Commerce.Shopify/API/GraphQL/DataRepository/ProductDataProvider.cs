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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <inheritdoc />
	public class ProductGQLDataProviderFactory : ISPGraphQLDataProviderFactory<ProductGQLDataProvider>
	{
		/// <inheritdoc />
		public ProductGQLDataProvider GetProvider(IGraphQLAPIClient graphQLAPIService)
		{
			return new ProductGQLDataProvider(graphQLAPIService);
		}
	}

	/// <summary>
	/// Performs data operations with products through Shopify's GraphQL API
	/// </summary>
	public class ProductGQLDataProvider : SPGraphQLDataProvider, IProductGQLDataProvider
	{
		/// <summary>
		/// Creates a new instance of the ProductGraphQLDataProvider that uses the specified GraphQLAPIService.
		/// </summary>
		/// <param name="graphQLAPIClient">The GraphQLAPIService to use to make requests.</param>
		public ProductGQLDataProvider(IGraphQLAPIClient graphQLAPIClient) : base(graphQLAPIClient)
		{
		}

		/// <inheritdoc />
		public virtual async Task<IEnumerable<ProductVariantGQL>> GetProductVariantsAsync(string filterString = null, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(QueryArgument.First<ProductVariantGQL>), DefaultPageSize},
				{ typeof(QueryArgument.After<ProductVariantGQL>), null},
				{ typeof(QueryArgument.Query<ProductVariantGQL>), filterString}
			};
			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(ProductVariantGQL), GraphQLQueryType.Connection, variables, true);

			return await GetAllAsync<ProductVariantGQL, ProductVariantsResponseData, ProductVariantsResponse>(queryInfo, cancellationToken);
		}
	}
}
