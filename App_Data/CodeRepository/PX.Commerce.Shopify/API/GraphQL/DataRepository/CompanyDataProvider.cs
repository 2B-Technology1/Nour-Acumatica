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

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <inheritdoc />
	public class CompanyGQLDataProviderFactory : ISPGraphQLDataProviderFactory<CompanyGQLDataProvider>
	{
		/// <inheritdoc />
		public CompanyGQLDataProvider GetProvider(IGraphQLAPIClient graphQLAPIService)
		{
			return new CompanyGQLDataProvider(graphQLAPIService);
		}
	}

	/// <summary>
	/// Performs data operations with companies through Shopify's GraphQL API
	/// </summary>
	public class CompanyGQLDataProvider : SPGraphQLDataProvider, ICompanyGQLDataProvider
	{
		private const int DefaultPageSize = 10;
		private const int MaxPageSize = 250;

		/// <summary>
		/// Creates a new instance of the CompanyDataGraphQLProvider that uses the specified GraphQLAPIService.
		/// </summary>
		/// <param name="graphQLAPIClient">The GraphQLAPIService to use to make requests.</param>
		public CompanyGQLDataProvider(IGraphQLAPIClient graphQLAPIClient) : base(graphQLAPIClient)
		{
		}


		/// <inheritdoc />
		public async Task<CompanyDataGQL> CreateCompanyAsync(CompanyDataGQL company, CancellationToken cancellationToken = default)
		{
			var companyCreateInput = new CompanyCreateInput
			{
				Company = GraphQLHelper.ConvertToMutationObject<CompanyInput, CompanyDataGQL>(company),
				CompanyLocation = GraphQLHelper.ConvertToMutationObject<CompanyLocationInput, CompanyLocationDataGQL>(company.Locations?.Nodes?.FirstOrDefault())
			};
			if(companyCreateInput?.CompanyLocation != null)
				companyCreateInput.CompanyLocation.ShippingAddress = GraphQLHelper.ConvertToMutationObject<CompanyAddressInput, CompanyAddressDataGQL>(company.Locations?.Nodes?.FirstOrDefault()?.ShippingAddress);

			var variables = new Dictionary<Type, object>
			{
				{ typeof(CompanyCreatePayload.Arguments.Input), companyCreateInput },
				{ typeof(QueryArgument.First<CompanyLocationDataGQL>), DefaultPageSize},
				{ typeof(QueryArgument.First<CompanyContactRoleDataGQL>), DefaultPageSize }
			};

			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(CompanyCreatePayload), GraphQLQueryType.Mutation, variables, true, true);

			var response = await MutationAsync<CompanyMutation>(queryInfo, cancellationToken);
			CheckIfHaveErrors(response?.CompanyCreate?.UserErrors);

			return response?.CompanyCreate?.Company;
		}

		/// <inheritdoc />
		public async Task<CompanyDataGQL> UpdateCompanyAsync(string companyId, CompanyDataGQL company, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(CompanyUpdatePayload.Arguments.CompanyId), companyId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.Company)},
				{ typeof(CompanyUpdatePayload.Arguments.Input), GraphQLHelper.ConvertToMutationObject<CompanyInput, CompanyDataGQL>(company)},
				{ typeof(QueryArgument.First<CompanyContactRoleDataGQL>), DefaultPageSize }
			};
			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(CompanyUpdatePayload), GraphQLQueryType.Mutation, variables, true, true);

			var response = await MutationAsync<CompanyMutation>(queryInfo, cancellationToken);
			CheckIfHaveErrors(response?.CompanyUpdate?.UserErrors);

			return response?.CompanyUpdate?.Company;
		}

		/// <inheritdoc />
		public async Task<string> DeleteCompanyAsync(string companyId, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(CompanyDeletePayload.Arguments.Id), companyId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.Company)}
			};
			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(CompanyDeletePayload), GraphQLQueryType.Mutation, variables, true);

			var response = await MutationAsync<CompanyMutation>(queryInfo, cancellationToken);
			CheckIfHaveErrors(response?.CompanyDelete?.UserErrors);

			return response?.CompanyDelete?.DeletedCompanyId;
		}

		/// <inheritdoc />
		public async Task<IEnumerable<CompanyContactRoleAssignmentDataGQL>> AssignCompanyContactRolesAsync(string companyContactId, List<CompanyContactRoleAssign> assignRoles, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(CompanyContactAssignRolesPayload.Arguments.CompanyContactId), companyContactId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.CompanyContact)},
				{ typeof(CompanyContactAssignRolesPayload.Arguments.RolesToAssign), assignRoles}
			};
			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(CompanyContactAssignRolesPayload), GraphQLQueryType.Mutation, variables, true);

			var response = await MutationAsync<CompanyMutation>(queryInfo, cancellationToken);
			CheckIfHaveErrors(response?.CompanyContactAssignRolesCreate?.UserErrors);

			return response?.CompanyContactAssignRolesCreate.RoleAssignments;
		}

		/// <inheritdoc />
		public async Task<IEnumerable<string>> RevokeCompanyContactRolesAsync(string companyContactId, List<string> revokeRoleAssignmentIds, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(CompanyContactRevokeRolesPayload.Arguments.CompanyContactId), companyContactId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.CompanyContact)},
				{ typeof(CompanyContactRevokeRolesPayload.Arguments.RevokeAll), false},
				{ typeof(CompanyContactRevokeRolesPayload.Arguments.RoleAssignmentIds), revokeRoleAssignmentIds}
			};
			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(CompanyContactRevokeRolesPayload), GraphQLQueryType.Mutation, variables, true);

			var response = await MutationAsync<CompanyMutation>(queryInfo, cancellationToken);
			CheckIfHaveErrors(response?.CompanyContactRevokeRoles?.UserErrors);

			return response?.CompanyContactRevokeRoles.RevokedRoleAssignmentIds;
		}

		/// <inheritdoc />
		public async Task<CompanyContactDataGQL> CreateCompanyContactAsync(string companyId, CompanyContactDataGQL contact, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(CompanyContactCreatePayload.Arguments.CompanyId), companyId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.Company)},
				{ typeof(CompanyContactCreatePayload.Arguments.Input), GraphQLHelper.ConvertToMutationObject<CompanyContactInput, CompanyContactDataGQL>(contact)}
			};
			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(CompanyContactCreatePayload), GraphQLQueryType.Mutation, variables, true);

			var response = await MutationAsync<CompanyMutation>(queryInfo, cancellationToken);
			CheckIfHaveErrors(response?.CompanyContactCreate?.UserErrors);

			return response?.CompanyContactCreate?.CompanyContact;
		}

		/// <inheritdoc />
		public async Task<CompanyContactDataGQL> UpdateCompanyContactAsync(string companyContactId, CompanyContactDataGQL contact, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(CompanyContactUpdatePayload.Arguments.CompanyContactId), companyContactId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.CompanyContact)},
				{ typeof(CompanyContactUpdatePayload.Arguments.Input), GraphQLHelper.ConvertToMutationObject<CompanyContactInput, CompanyContactDataGQL>(contact)}
			};
			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(CompanyContactUpdatePayload), GraphQLQueryType.Mutation, variables, true);

			var response = await MutationAsync<CompanyMutation>(queryInfo, cancellationToken);
			CheckIfHaveErrors(response?.CompanyContactUpdate?.UserErrors);

			return response?.CompanyContactUpdate?.CompanyContact;
		}
		
		/// <inheritdoc />
		public async Task<CompanyContactDataGQL> AssignCustomerAsContactAsync(string companyId, string customerId, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(CompanyAssignCustomerAsContactPayload.Arguments.CompanyId), companyId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.Company)},
				{ typeof(CompanyAssignCustomerAsContactPayload.Arguments.CustomerId), customerId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.Customer)}
			};
			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(CompanyAssignCustomerAsContactPayload), GraphQLQueryType.Mutation, variables, true);

			var response = await MutationAsync<CompanyMutation>(queryInfo, cancellationToken);
			CheckIfHaveErrors(response?.CompanyAssignCustomerAsContact?.UserErrors);

			return response?.CompanyAssignCustomerAsContact?.CompanyContact;
		}

		/// <inheritdoc />
		public async Task<CompanyDataGQL> AssignCompanyMainContactAsync(string companyId, string companyContactId, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(CompanyAssignMainContactPayload.Arguments.CompanyId), companyId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.Company)},
				{ typeof(CompanyAssignMainContactPayload.Arguments.CompanyContactId), companyContactId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.CompanyContact)}
			};
			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(CompanyAssignMainContactPayload), GraphQLQueryType.Mutation, variables, true);

			var response = await MutationAsync<CompanyMutation>(queryInfo, cancellationToken);
			CheckIfHaveErrors(response?.CompanyAssignMainContact?.UserErrors);

			return response?.CompanyAssignMainContact?.Company;
		}

		/// <inheritdoc />
		public async Task<CompanyLocationDataGQL> CreateCompanyLocationAsync(string companyId, CompanyLocationDataGQL location, CancellationToken cancellationToken = default)
		{
			var companyLocationInput = GraphQLHelper.ConvertToMutationObject<CompanyLocationInput, CompanyLocationDataGQL>(location);
			companyLocationInput.BillingAddress = location.BillingAddress != null ? GraphQLHelper.ConvertToMutationObject<CompanyAddressInput, CompanyAddressDataGQL>(location.BillingAddress) : null;
			companyLocationInput.ShippingAddress = location.ShippingAddress != null ? GraphQLHelper.ConvertToMutationObject<CompanyAddressInput, CompanyAddressDataGQL>(location.ShippingAddress) : null;

			var variables = new Dictionary<Type, object>
			{
				{ typeof(CompanyLocationCreatePayload.Arguments.CompanyId), companyId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.Company)},
				{ typeof(CompanyLocationCreatePayload.Arguments.Input), companyLocationInput}
			};
			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(CompanyLocationCreatePayload), GraphQLQueryType.Mutation, variables, true);

			var response = await MutationAsync<CompanyMutation>(queryInfo, cancellationToken);
			CheckIfHaveErrors(response?.CompanyLocationCreate?.UserErrors);

			return response?.CompanyLocationCreate?.CompanyLocation;
		}

		/// <inheritdoc />
		public async Task<CompanyLocationDataGQL> UpdateCompanyLocationAsync(string companyLocationId, CompanyLocationDataGQL location, CancellationToken cancellationToken = default)
		{
			var companyLocationInput = GraphQLHelper.ConvertToMutationObject<CompanyLocationUpdateInput, CompanyLocationDataGQL>(location);
			
			var variables = new Dictionary<Type, object>
			{
				{ typeof(CompanyLocationUpdatePayload.Arguments.CompanyLocationId), companyLocationId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.CompanyLocation)},
				{ typeof(CompanyLocationUpdatePayload.Arguments.Input), companyLocationInput}
			};
			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(CompanyLocationUpdatePayload), GraphQLQueryType.Mutation, variables, true);

			var response = await MutationAsync<CompanyMutation>(queryInfo, cancellationToken);
			CheckIfHaveErrors(response?.CompanyLocationUpdate?.UserErrors);

			if(location.ShippingAddress != null)
			{
				var addressResponse = await UpdateCompanyLocationAddressAsync(companyLocationId, location.ShippingAddress, new string[] { "SHIPPING" });
				if(response?.CompanyLocationUpdate?.CompanyLocation != null)
				{
					response.CompanyLocationUpdate.CompanyLocation.ShippingAddress = addressResponse.FirstOrDefault();
				}
			}

			return response?.CompanyLocationUpdate?.CompanyLocation;
		}

		/// <inheritdoc />
		public async Task<IEnumerable<CompanyAddressDataGQL>> UpdateCompanyLocationAddressAsync(string companyLocationId, CompanyAddressDataGQL addressData, string[] addressTypes,CancellationToken cancellationToken = default)
		{
			var companyAddressInput = GraphQLHelper.ConvertToMutationObject<CompanyAddressInput, CompanyAddressDataGQL>(addressData);
			
			var variables = new Dictionary<Type, object>
			{
				{ typeof(CompanyLocationAssignAddressPayload.Arguments.LocationId), companyLocationId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.CompanyLocation)},
				{ typeof(CompanyLocationAssignAddressPayload.Arguments.Address), companyAddressInput},
				{ typeof(CompanyLocationAssignAddressPayload.Arguments.AddressTypes), addressTypes}
			};
			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(CompanyLocationAssignAddressPayload), GraphQLQueryType.Mutation, variables, true);

			var response = await MutationAsync<CompanyMutation>(queryInfo, cancellationToken);
			CheckIfHaveErrors(response?.CompanyLocationAssignAddress?.UserErrors);

			return response?.CompanyLocationAssignAddress?.CompanyLocationAddresses;
		}

		/// <inheritdoc />
		public async Task<IEnumerable<CompanyDataGQL>> GetCompaniesAsync(string filterString = null, bool includedSubFields = false, CancellationToken cancellationToken = default, params string[] specifiedFieldsOnly)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(QueryArgument.First<CompanyDataGQL>), includedSubFields ? DefaultFetchBulkSizeWithSubfields : DefaultFetchBulkSize},
				{ typeof(QueryArgument.After<CompanyDataGQL>), null}
			};
			if (string.IsNullOrEmpty(filterString) == false)
			{
				variables[typeof(QueryArgument.Query<OrderDataGQL>)] = filterString;
			}

			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(CompanyDataGQL), GraphQLQueryType.Connection, variables, includedSubFields, false, specifiedFieldsOnly);

			return await GetAllAsync<CompanyDataGQL, CompaniesResponseData, CompaniesResponse>(queryInfo, cancellationToken);
		}


		/// <inheritdoc />
		public async Task<CompanyDataGQL> GetCompanyByIDAsync(string id, bool withContacts = false, bool withLocations = false, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(QueryArgument.ID<CompanyDataGQL>), id.ConvertIdToGid(ShopifyGraphQLConstants.Objects.Company)}
			};

			List<string> subConnections = new List<string>();
			if (withContacts)
			{
				variables[typeof(QueryArgument.First<CompanyContactDataGQL>)] = DefaultPageSize;
				variables[typeof(QueryArgument.After<CompanyContactDataGQL>)] = null;
				subConnections.Add(nameof(CompanyDataGQL.Contacts));
			}

			if (withLocations)
			{
				variables[typeof(QueryArgument.First<CompanyLocationDataGQL>)] = DefaultPageSize;
				variables[typeof(QueryArgument.After<CompanyLocationDataGQL>)] = null;
				subConnections.Add(nameof(CompanyDataGQL.Locations));
			}

			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(CompanyDataGQL), GraphQLQueryType.Node, variables, true, withContacts || withLocations);
			var response = await GetSingleWithSubConnectionsAsync<CompanyDataGQL,CompanyResponse> (queryInfo, cancellationToken, subConnections.ToArray());

			if(response?.ContactsList?.Any() == true)
			{
				foreach (var item in response?.Contacts?.Nodes ?? Enumerable.Empty<CompanyContactDataGQL>())
				{
					item.RoleAssignments = new Connection<CompanyContactRoleAssignmentDataGQL>
					{
						Nodes = await GetContactRoleAssignmentsByContactIDAsync(item.Id, cancellationToken)
					};
				}
			}

			return response;
		}

		public async Task<IEnumerable<CustomerDataGQL>> GetCompanyCustomerByEmailAsync(string email, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(QueryArgument.First<CustomerDataGQL>), DefaultPageSize},
				{ typeof(QueryArgument.After<CustomerDataGQL>), null},
				{ typeof(QueryArgument.Query<CustomerDataGQL>), string.Format("email:{0}", email)}
			};
			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(CustomerDataGQL), GraphQLQueryType.Connection, variables);

			return await GetAllAsync<CustomerDataGQL, CustomersResponseData, CustomersResponse>(queryInfo, cancellationToken);
		}

		public async Task<IEnumerable<CompanyContactDataGQL>> GetCompanyContactsByCompanyIDAsync(string id, int contactCount, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(QueryArgument.First<CompanyContactDataGQL>), contactCount},
				{ typeof(QueryArgument.After<CompanyContactDataGQL>), null},
				{ typeof(QueryArgument.ID<CompanyDataGQL>), id.ConvertIdToGid(ShopifyGraphQLConstants.Objects.Company)}
			};
			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(CompanyDataGQL), GraphQLQueryType.Node, variables, true, true, nameof(CompanyDataGQL.Contacts));

			var response = await GetSingleAsync<CompanyDataGQL, CompanyContactDataGQL, CompanyResponse>(queryInfo, nameof(CompanyDataGQL.Contacts), cancellationToken);

			foreach(var item in response?.Contacts?.Nodes ?? Enumerable.Empty<CompanyContactDataGQL>())
			{
				item.RoleAssignments = new Connection<CompanyContactRoleAssignmentDataGQL>
				{
					Nodes = await GetContactRoleAssignmentsByContactIDAsync(item.Id, cancellationToken)
				};
			}

			return response?.Contacts?.Nodes ?? Enumerable.Empty<CompanyContactDataGQL>();
		}

		public async Task<IEnumerable<CompanyContactRoleDataGQL>> GetCompanyContactRolesByCompanyIDAsync(string id, int contactCount = DefaultPageSize, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(QueryArgument.First<CompanyContactRoleDataGQL>), contactCount},
				{ typeof(QueryArgument.After<CompanyContactRoleDataGQL>), null},
				{ typeof(QueryArgument.ID<CompanyDataGQL>), id.ConvertIdToGid(ShopifyGraphQLConstants.Objects.Company)}
			};
			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(CompanyDataGQL), GraphQLQueryType.Node, variables, true, true, nameof(CompanyDataGQL.ContactRoles));

			var response = await GetSingleAsync<CompanyDataGQL, CompanyContactRoleDataGQL, CompanyResponse>(queryInfo, nameof(CompanyDataGQL.ContactRoles), cancellationToken);

			return response?.ContactRoles?.Nodes ?? Enumerable.Empty<CompanyContactRoleDataGQL>();
		}

		public async Task<IEnumerable<CompanyContactRoleAssignmentDataGQL>> GetContactRoleAssignmentsByContactIDAsync(string id, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(QueryArgument.First<CompanyContactRoleAssignmentDataGQL>), DefaultPageSize},
				{ typeof(QueryArgument.After<CompanyContactRoleAssignmentDataGQL>), null},
				{ typeof(QueryArgument.ID<CompanyContactDataGQL>), id.ConvertIdToGid(ShopifyGraphQLConstants.Objects.CompanyContact)}
			};
			var querybuilder = new GraphQLQueryBuilder();
			querybuilder.AllowRecursiveObject = true;
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(CompanyContactDataGQL), GraphQLQueryType.Node, variables, true, true, nameof(CompanyContactDataGQL.RoleAssignments));

			var response = await GetSingleAsync<CompanyContactDataGQL, CompanyContactRoleAssignmentDataGQL, CompanyContactResponse>(queryInfo, nameof(CompanyContactDataGQL.RoleAssignments), cancellationToken);

			return response?.RoleAssignments?.Nodes ?? Enumerable.Empty<CompanyContactRoleAssignmentDataGQL>();
		}

		public async Task<IEnumerable<CompanyLocationDataGQL>> GetCompanyLocationsByCompanyIDAsync(string id, int locationCount, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(QueryArgument.First<CompanyLocationDataGQL>), locationCount},
				{ typeof(QueryArgument.After<CompanyLocationDataGQL>), null},
				{ typeof(QueryArgument.ID<CompanyDataGQL>), id.ConvertIdToGid(ShopifyGraphQLConstants.Objects.Company)}
			};
			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(CompanyDataGQL), GraphQLQueryType.Node, variables, true, true, nameof(CompanyDataGQL.Locations));

			var response = await GetSingleAsync<CompanyDataGQL, CompanyLocationDataGQL, CompanyResponse>(queryInfo, nameof(CompanyDataGQL.Locations), cancellationToken);

			return response?.Locations?.Nodes ?? Enumerable.Empty<CompanyLocationDataGQL>();
		}
	}
}
