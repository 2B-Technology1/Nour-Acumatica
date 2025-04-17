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

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <inheritdoc />
	public class CustomerGQLDataProviderFactory : ISPGraphQLDataProviderFactory<CustomerGQLDataProvider>
	{
		/// <inheritdoc />
		public CustomerGQLDataProvider GetProvider(IGraphQLAPIClient graphQLAPIService)
		{
			return new CustomerGQLDataProvider(graphQLAPIService);
		}
	}

	/// <summary>
	/// Performs data operations with customers through Shopify's GraphQL API
	/// </summary>
	public class CustomerGQLDataProvider : SPGraphQLDataProvider, ICustomerGQLDataProvider
	{
		private const int DefaultPageSize = 10;
		private const int MaxPageSize = 250;

		/// <summary>
		/// Creates a new instance of the CustomerDataGraphQLProvider that uses the specified GraphQLAPIService.
		/// </summary>
		/// <param name="graphQLAPIClient">The GraphQLAPIService to use to make requests.</param>
		public CustomerGQLDataProvider(IGraphQLAPIClient graphQLAPIClient) : base(graphQLAPIClient)
		{
		}


		/// <inheritdoc />
		public async Task<CustomerDataGQL> CreateCustomerAsync(CustomerInput customerInput, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(CustomerCreatePayload.Arguments.Input), customerInput }
			};

			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(CustomerCreatePayload), GraphQLQueryType.Mutation, variables, true, false);

			var response = await MutationAsync<CustomerMutation>(queryInfo, cancellationToken);
			CheckIfHaveErrors(response?.CustomerCreate?.UserErrors);

			return response?.CustomerCreate?.Customer;
		}

		/// <inheritdoc />
		public async Task<CustomerDataGQL> UpdateCustomerAsync(CustomerInput customerInput, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(CustomerUpdatePayload.Arguments.Input), customerInput}
			};
			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(CustomerUpdatePayload), GraphQLQueryType.Mutation, variables, true, false);

			var response = await MutationAsync<CustomerMutation>(queryInfo, cancellationToken);
			CheckIfHaveErrors(response?.CustomerUpdate?.UserErrors);

			return response?.CustomerUpdate?.Customer;
		}

		/// <inheritdoc />
		public async Task<CustomerDataGQL> UpdateCustomerDefaultAddressAsync(string customerId, string addressId, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(CustomerUpdateDefaultAddressPayload.Arguments.AddressId), addressId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.MailingAddress)},
				{ typeof(CustomerUpdateDefaultAddressPayload.Arguments.CustomerId), customerId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.Customer)}
			};
			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(CustomerUpdateDefaultAddressPayload), GraphQLQueryType.Mutation, variables, true, false);

			var response = await MutationAsync<CustomerMutation>(queryInfo, cancellationToken);
			CheckIfHaveErrors(response?.CustomerUpdateDefaultAddress?.UserErrors);

			return response?.CustomerUpdateDefaultAddress?.Customer;
		}

		/// <inheritdoc />
		public async Task<string> DeleteCustomerAsync(string customerId, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(CustomerDeletePayload.Arguments.Input), new CustomerDeleteInput(){ Id = customerId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.Customer)} }
			};
			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(CustomerDeletePayload), GraphQLQueryType.Mutation, variables, true);

			var response = await MutationAsync<CustomerMutation>(queryInfo, cancellationToken);
			CheckIfHaveErrors(response?.CustomerDelete?.UserErrors);

			return response?.CustomerDelete?.DeletedCustomerId;
		}

		/// <inheritdoc />
		public async Task<IEnumerable<CustomerDataGQL>> GetCustomersAsync(string filterString = null, string sortKeyFieldName = OrderSortKeys.UpdatedAt, bool includedSubFields = false, CancellationToken cancellationToken = default, params string[] specifiedFieldsOnly)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(QueryArgument.First<CustomerDataGQL>), includedSubFields ? DefaultFetchBulkSizeWithSubfields : DefaultFetchBulkSize},
				{ typeof(QueryArgument.After<CustomerDataGQL>), null},
				{ typeof(CustomerDataGQL.Arguments.SortKey), sortKeyFieldName}
			};
			if (string.IsNullOrEmpty(filterString) == false)
			{
				variables[typeof(QueryArgument.Query<CustomerDataGQL>)] = filterString;
			}

			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(CustomerDataGQL), GraphQLQueryType.Connection, variables, includedSubFields, false, specifiedFieldsOnly);

			return await GetAllAsync<CustomerDataGQL, CustomersResponseData, CustomersResponse>(queryInfo, cancellationToken);
		}


		/// <inheritdoc />
		public async Task<CustomerDataGQL> GetCustomerByIDAsync(string id, bool includedSubFields = true, bool includedMetaFields = false, CancellationToken cancellationToken = default, params string[] specifiedFieldsOnly)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(QueryArgument.ID<CustomerDataGQL>), id.ConvertIdToGid(ShopifyGraphQLConstants.Objects.Customer)}
			};
			if (includedMetaFields)
			{
				variables[typeof(QueryArgument.First<MetafieldGQL>)] = DefaultPageSize;
				variables[typeof(QueryArgument.After<MetafieldGQL>)] = null;
			}

			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(CustomerDataGQL), GraphQLQueryType.Node, variables, includedSubFields, includedMetaFields, specifiedFieldsOnly);

			return includedMetaFields ? await GetSingleAsync<CustomerDataGQL, MetafieldGQL, CustomerResponse> (queryInfo, nameof(CustomerDataGQL.MetafieldNodes), cancellationToken) :
				await GetSingleAsync<CustomerDataGQL, CustomerResponse>(queryInfo, cancellationToken);
		}

		public async Task<IEnumerable<MailingAddress>> GetCustomerAddressesAsync(string id, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(QueryArgument.ID<CustomerDataGQL>), id.ConvertIdToGid(ShopifyGraphQLConstants.Objects.Customer)}
			};
			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(CustomerDataGQL), GraphQLQueryType.Node, variables, true, false, nameof(CustomerDataGQL.Addresses));

			return (await GetSingleAsync<CustomerDataGQL, CustomerResponse>(queryInfo, cancellationToken))?.Addresses ?? Enumerable.Empty<MailingAddress>();
		}
	}
}
