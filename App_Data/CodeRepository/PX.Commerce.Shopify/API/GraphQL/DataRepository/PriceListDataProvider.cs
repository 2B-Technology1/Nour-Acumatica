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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PX.Commerce.Core;
using PX.Data;
using PX.Objects.CR;
using static PX.Commerce.Shopify.API.GraphQL.PriceListFixedPricesUpdatePayload.Arguments;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <inheritdoc />
	public class PriceListGQLDataProviderFactory : ISPGraphQLDataProviderFactory<PriceListGQLDataProvider>
	{
		/// <inheritdoc />
		public PriceListGQLDataProvider GetProvider(IGraphQLAPIClient graphQLAPIService)
		{
			return new PriceListGQLDataProvider(graphQLAPIService);
		}
	}

	/// <summary>
	/// Performs data operations with PriceList through Shopify's GraphQL API
	/// </summary>
	public class PriceListGQLDataProvider : SPGraphQLDataProvider, IPriceListDataGQLProvider
	{

		/// <summary>
		/// Creates a new instance of the PriceListDataGraphQLProvider that uses the specified GraphQLAPIService.
		/// </summary>
		/// <param name="graphQLAPIClient">The GraphQLAPIService to use to make requests.</param>
		public PriceListGQLDataProvider(IGraphQLAPIClient graphQLAPIClient) : base(graphQLAPIClient)
		{
		}


		/// <inheritdoc />
		public async Task<CompanyLocationCatalogGQL> CreateCatalog(CatalogCreateInput catalogInput, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(CatalogCreatePayload.Arguments.Input), catalogInput }
			};

			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(CatalogCreatePayload), GraphQLQueryType.Mutation, variables, true, false);

			var response = await MutationAsync<PriceListMutation>(queryInfo, cancellationToken);
			CheckIfHaveErrors(response?.CatalogCreate?.UserErrors);

			return response?.CatalogCreate?.Catalog;
		}

		/// <inheritdoc />
		public async Task<CompanyLocationCatalogGQL> UpdateCatalog(string catalogId, CatalogUpdateInput catalogInput, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(CatalogUpdatePayload.Arguments.Id), catalogId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.CompanyLocationCatalog)},
				{ typeof(CatalogUpdatePayload.Arguments.Input), catalogInput}
			};
			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(CatalogUpdatePayload), GraphQLQueryType.Mutation, variables, true, false);

			var response = await MutationAsync<PriceListMutation>(queryInfo, cancellationToken);
			CheckIfHaveErrors(response?.CatalogUpdate?.UserErrors);

			return response?.CatalogUpdate?.Catalog;
		}

		/// <inheritdoc />
		public async Task<string> DeleteCatalog(string catalogId, bool deleteDependentResources = true, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(CatalogDeletePayload.Arguments.Id), catalogId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.CompanyLocationCatalog)},
				{ typeof(CatalogDeletePayload.Arguments.DeleteDependentResources), deleteDependentResources}
			};
			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(CatalogDeletePayload), GraphQLQueryType.Mutation, variables, true);

			var response = await MutationAsync<PriceListMutation>(queryInfo, cancellationToken);
			CheckIfHaveErrors(response?.CatalogDelete?.UserErrors);

			return response?.CatalogDelete?.DeletedId;
		}

		/// <inheritdoc />
		public async Task<PublicationGQL> CreatePublication(PublicationCreateInput publicationInput, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(PublicationCreatePayload.Arguments.Input), publicationInput }
			};

			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(PublicationCreatePayload), GraphQLQueryType.Mutation, variables, true, false);

			var response = await MutationAsync<PriceListMutation>(queryInfo, cancellationToken);
			CheckIfHaveErrors(response?.PublicationCreate?.UserErrors);

			return response?.PublicationCreate?.Publication;
		}

		/// <inheritdoc />
		public async Task<PriceListGQL> CreatePriceList(PriceListCreateInput priceListInput, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(PriceListCreatePayload.Arguments.Input), priceListInput }
			};

			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(PriceListCreatePayload), GraphQLQueryType.Mutation, variables, true, false);

			var response = await MutationAsync<PriceListMutation>(queryInfo, cancellationToken);
			CheckIfHaveErrors(response?.PriceListCreate?.UserErrors);

			return response?.PriceListCreate?.PriceList;
		}

		/// <inheritdoc />
		public async Task<PriceListGQL> UpdatePriceList(string priceListId, PriceListUpdateInput priceListInput, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(PriceListUpdatePayload.Arguments.Id), priceListId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.PriceList)},
				{ typeof(PriceListUpdatePayload.Arguments.Input), priceListInput}
			};
			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(PriceListUpdatePayload), GraphQLQueryType.Mutation, variables, true, false);

			var response = await MutationAsync<PriceListMutation>(queryInfo, cancellationToken);
			CheckIfHaveErrors(response?.PriceListUpdate?.UserErrors);

			return response?.PriceListUpdate?.PriceList;
		}

		/// <inheritdoc />
		public async Task<string> DeletePriceList(string priceListId, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(PriceListDeletePayload.Arguments.Id), priceListId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.PriceList)}
			};
			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(PriceListDeletePayload), GraphQLQueryType.Mutation, variables, true);

			var response = await MutationAsync<PriceListMutation>(queryInfo, cancellationToken);
			CheckIfHaveErrors(response?.PriceListDelete?.UserErrors);

			return response?.PriceListDelete?.DeletedId;
		}

		/// <inheritdoc />
		public async Task<IEnumerable<PriceListPriceGQL>> AddPriceListFixedPrices(string priceListId, List<PriceListPriceInput> priceListPriceInputs, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(PriceListFixedPricesAddPayload.Arguments.PriceListId), priceListId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.PriceList)},
				{ typeof(PriceListFixedPricesAddPayload.Arguments.Prices), priceListPriceInputs}
			};
			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(PriceListFixedPricesAddPayload), GraphQLQueryType.Mutation, variables, true, false);

			var response = await MutationAsync<PriceListMutation>(queryInfo, cancellationToken);
			CheckIfHaveErrors(response?.PriceListFixedPricesAdd?.UserErrors);

			return response?.PriceListFixedPricesAdd?.Prices;
		}

		/// <inheritdoc />
		public async Task<PriceListFixedPricesUpdatePayload> UpdatePriceListFixedPrices(string priceListId, List<PriceListPriceInput> priceListPriceInputs, string[] variantIdsToDelete = null, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(PriceListFixedPricesUpdatePayload.Arguments.PriceListId), priceListId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.PriceList)},
				{ typeof(PriceListFixedPricesUpdatePayload.Arguments.VariantIdsToDelete), variantIdsToDelete?? new string[0]},
				{ typeof(PriceListFixedPricesUpdatePayload.Arguments.PricesToAdd), priceListPriceInputs}
			};
			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(PriceListFixedPricesUpdatePayload), GraphQLQueryType.Mutation, variables, true, false);

			var response = await MutationAsync<PriceListMutation>(queryInfo, cancellationToken);
			CheckIfHaveErrors(response?.PriceListFixedPricesUpdate?.UserErrors);

			return response?.PriceListFixedPricesUpdate;
		}

		/// <inheritdoc />
		public async Task<IEnumerable<string>> DeletePriceListFixedPrices(string priceListId, string[] variantIdsToDelete, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(PriceListFixedPricesDeletePayload.Arguments.PriceListId), priceListId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.PriceList)},
				{ typeof(PriceListFixedPricesDeletePayload.Arguments.VariantIds), variantIdsToDelete?? new string[0]}
			};
			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(PriceListFixedPricesDeletePayload), GraphQLQueryType.Mutation, variables, true, false);

			var response = await MutationAsync<PriceListMutation>(queryInfo, cancellationToken);
			CheckIfHaveErrors(response?.PriceListFixedPricesDelete?.UserErrors);

			return response?.PriceListFixedPricesDelete.DeletedFixedPriceVariantIds;
		}

		/// <inheritdoc />
		public async Task<IEnumerable<CompanyLocationCatalogGQL>> GetCatalogs(string filterString = null, bool includedSubFields = false, CancellationToken cancellationToken = default, params string[] specifiedFieldsOnly)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(QueryArgument.First<CompanyLocationCatalogGQL>), includedSubFields ? DefaultFetchBulkSizeWithSubfields : DefaultFetchBulkSize},
				{ typeof(QueryArgument.After<CompanyLocationCatalogGQL>), null}
			};
			if (string.IsNullOrEmpty(filterString) == false)
			{
				variables[typeof(QueryArgument.Query<CompanyLocationCatalogGQL>)] = filterString;
			}

			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(CompanyLocationCatalogGQL), GraphQLQueryType.Connection, variables, includedSubFields, false, specifiedFieldsOnly);

			return await GetAllAsync<CompanyLocationCatalogGQL, CatalogsResponseData, CatalogsResponse>(queryInfo, cancellationToken);
		}

		/// <inheritdoc />
		public async Task<IEnumerable<PriceListGQL>> GetPriceLists(string filterString = null, bool includedSubFields = false, CancellationToken cancellationToken = default, params string[] specifiedFieldsOnly)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(QueryArgument.First<PriceListGQL>), includedSubFields ? DefaultFetchBulkSizeWithSubfields : DefaultFetchBulkSize},
				{ typeof(QueryArgument.After<PriceListGQL>), null}
			};
			if (string.IsNullOrEmpty(filterString) == false)
			{
				variables[typeof(QueryArgument.Query<PriceListGQL>)] = filterString;
			}

			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(PriceListGQL), GraphQLQueryType.Connection, variables, includedSubFields, false, specifiedFieldsOnly);

			return await GetAllAsync<PriceListGQL, PriceListsResponseData, PriceListsResponse>(queryInfo, cancellationToken);
		}

		/// <inheritdoc />
		public async Task<CompanyLocationCatalogGQL> GetCatalogByID(string id, bool withSubFields = true, bool withSubConnections = false, CancellationToken cancellationToken = default, params string[] specifiedFieldsOnly)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(QueryArgument.ID<CompanyLocationCatalogGQL>), id.ConvertIdToGid(ShopifyGraphQLConstants.Objects.CompanyLocationCatalog)}
			};
			if (withSubConnections)
			{
				variables[typeof(QueryArgument.First<CompanyLocationDataGQL>)] = DefaultPageSize;
				variables[typeof(QueryArgument.After<CompanyLocationDataGQL>)] = null;
			}

			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(CompanyLocationCatalogGQL), GraphQLQueryType.Node, variables, withSubFields, withSubConnections, specifiedFieldsOnly);

			return await GetSingleAsync<CompanyLocationCatalogGQL, CompanyLocationDataGQL, CatalogResponse>(queryInfo, nameof(CompanyLocationCatalogGQL.CompanyLocations), cancellationToken);
		}

		/// <inheritdoc />
		public async Task<PriceListGQL> GetPriceListByID(string id, bool withSubFields = true, bool withSubConnections = false, CancellationToken cancellationToken = default, params string[] specifiedFieldsOnly)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(QueryArgument.ID<PriceListGQL>), id.ConvertIdToGid(ShopifyGraphQLConstants.Objects.PriceList)}
			};
			if (withSubConnections)
			{
				variables[typeof(PriceListPriceGQL.Arguments.OriginType)] = PriceListPriceOriginTypeGQL.Fixed;
				variables[typeof(QueryArgument.First<PriceListPriceGQL>)] = DefaultPageSize;
				variables[typeof(QueryArgument.After<PriceListPriceGQL>)] = null;
			}

			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(PriceListGQL), GraphQLQueryType.Node, variables, withSubFields, withSubConnections, specifiedFieldsOnly);

			return await GetSingleAsync<PriceListGQL, PriceListPriceGQL, PriceListResponse>(queryInfo, nameof(PriceListGQL.Prices), cancellationToken);
		}

		/// <inheritdoc />
		public async Task<ShopGQL> GetShop(CancellationToken cancellationToken = default, params string[] specifiedFieldsOnly)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(QueryArgument.First<CurrencySettingGQL>), DefaultPageSize},
				{ typeof(QueryArgument.After<CurrencySettingGQL>), null}
			};

			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(ShopGQL), GraphQLQueryType.Node, variables, true, true, specifiedFieldsOnly);

			return await GetSingleAsync<ShopGQL, CurrencySettingGQL, ShopResponse>(queryInfo, nameof(ShopGQL.CurrencySettings), cancellationToken);
		}

	}
}
