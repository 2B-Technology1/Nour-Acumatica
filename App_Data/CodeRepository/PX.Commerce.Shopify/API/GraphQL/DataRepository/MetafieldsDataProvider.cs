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

using PX.Data;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PX.Commerce.Shopify.API.GraphQL
{
	public class MetafieldsGQLDataProviderFactory : ISPGraphQLDataProviderFactory<MetaFielsGQLDataProvider>
	{
		public MetaFielsGQLDataProvider GetProvider(IGraphQLAPIClient graphQLAPIClient)
		{
			return new MetaFielsGQLDataProvider(graphQLAPIClient);
		}
	}


	public class MetaFielsGQLDataProvider : SPGraphQLDataProvider, IMetafieldsGQLDataProvider
	{
		private const int DefaultPageSize = 20;

		public MetaFielsGQLDataProvider(IGraphQLAPIClient graphQLAPIClient) : base(graphQLAPIClient)
		{
		}

		public async Task<IEnumerable<MetafieldDefintionGQL>> GetAllForEntityTypeAsync(string entityType, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(QueryArgument.First<MetafieldDefintionGQL>), DefaultPageSize},
				{ typeof(QueryArgument.OwnerType<MetafieldDefintionGQL>), entityType}
			};
			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(MetafieldDefintionGQL), GraphQLQueryType.Connection, variables, true);

			return await GetAllAsync<MetafieldDefintionGQL, MetafieldsDefinitionResponseData, MetafieldsDefinitionResponse>(queryInfo, cancellationToken);
		}
	}
}
