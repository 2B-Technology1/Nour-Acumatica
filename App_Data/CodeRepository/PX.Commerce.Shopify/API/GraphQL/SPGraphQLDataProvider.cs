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
using PX.Api;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static PX.Objects.FA.FARecordType;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// A data provider that interacts with a GraphQL API.
	/// </summary>
	public abstract class SPGraphQLDataProvider
	{
		/// <summary>
		/// The API service which performs requests to and from the external GraphQL API.
		/// </summary>
		protected IGraphQLAPIClient GraphQLAPIClient { get; }
		protected const int DefaultPageSize = 10;
		protected const int DefaultFetchBulkSizeWithSubfields = 50;
		protected const int DefaultFetchBulkSize = 100;

		/// <summary>
		/// Creates an instance of the <see cref="SPGraphQLDataProvider"/> with that
		/// uses the provided <see cref="GraphQLAPIClient"/>.
		/// </summary>
		/// <param name="graphQLAPIClient"></param>
		public SPGraphQLDataProvider(IGraphQLAPIClient graphQLAPIClient)
		{
			GraphQLAPIClient = graphQLAPIClient;
		}

		/// <summary>
		/// Get the single entity data
		/// </summary>
		/// <typeparam name="TEntityData">The entity data type</typeparam>
		/// <typeparam name="TEntityResponse">The response container of entity data </typeparam>
		/// <param name="queryInfo">The query info object</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns>The single entity data</returns>
		public async Task<TEntityData> GetSingleAsync<TEntityData, TEntityResponse>(GraphQLQueryInfo queryInfo, CancellationToken cancellationToken = default)
			where TEntityData : BCAPIEntity, new()
			where TEntityResponse : class, IEntityResponse<TEntityData>, new()
		{
			var response = await GraphQLAPIClient.ExecuteAsync<TEntityResponse>(queryInfo.Query, queryInfo.GetCostPoints(), queryInfo.Variables, cancellationToken);
			return response?.TEntityData;
		}

		/// <summary>
		/// Get the single entity data with sub entity data
		/// </summary>
		/// <typeparam name="TEntityData">The entity data type</typeparam>
		/// <typeparam name="TSubEntityData">The sub entity data type</typeparam>
		/// <typeparam name="TEntityResponse">The response container of entity data </typeparam>
		/// <param name="queryInfo">The query info object</param>
		/// <param name="subItemPropertyName">The property name of sub entity data</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns>The single entity data</returns>
		public async Task<TEntityData> GetSingleAsync<TEntityData, TSubEntityData, TEntityResponse>(GraphQLQueryInfo queryInfo, string subItemPropertyName, CancellationToken cancellationToken = default)
			where TEntityData : BCAPIEntity, new()
			where TSubEntityData : BCAPIEntity, new()
			where TEntityResponse : class, IEntityResponse<TEntityData>, new()
		{
			var response = await GraphQLAPIClient.ExecuteAsync<TEntityResponse>(queryInfo.Query, queryInfo.GetCostPoints(), queryInfo.Variables, cancellationToken);

			if(response?.TEntityData != null)
			{
				var matchedSubItem = response.TEntityData.GetType().GetProperty(subItemPropertyName, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
				if(matchedSubItem != null)
				{
					List<TSubEntityData> result = new List<TSubEntityData>();
					bool needGet = false;
					Connection<TSubEntityData> connectionValue = null;
					do
					{
						needGet = false;
						var subItemValue = matchedSubItem.GetValue(response.TEntityData);
						if (subItemValue != null && subItemValue is Connection<TSubEntityData>)
						{
							connectionValue = (Connection<TSubEntityData>)subItemValue;
							result.AddRange(connectionValue.Nodes);
							needGet = HasNextPageData(typeof(TSubEntityData), connectionValue?.PageInfo, queryInfo);
						}

						if (needGet)
						{
							response = await GraphQLAPIClient.ExecuteAsync<TEntityResponse>(queryInfo.Query, queryInfo.GetCostPoints(), queryInfo.Variables, cancellationToken);
						}
						else if(!needGet && result.Any())
						{
							connectionValue.Nodes = result;
							matchedSubItem.SetValue(response.TEntityData, connectionValue);
						}

					} while (needGet);
				}
			}
			return response?.TEntityData;
		}

		/// <summary>
		/// Get the single entity data with sub connections data
		/// </summary>
		/// <typeparam name="TEntityData">The entity data type</typeparam>
		/// <typeparam name="TEntityResponse">The response container of entity data </typeparam>
		/// <param name="queryInfo">The query info object</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <param name="subConnectionFieldNames">The name of sub connection field</param>
		/// <returns>The single entity data</returns>
		public async Task<TEntityData> GetSingleWithSubConnectionsAsync<TEntityData, TEntityResponse>(GraphQLQueryInfo queryInfo, CancellationToken cancellationToken = default, params string[] subConnectionFieldNames)
			where TEntityData : BCAPIEntity, new()
			where TEntityResponse : class, IEntityResponse<TEntityData>, new()
		{
			var response = await GraphQLAPIClient.ExecuteAsync<TEntityResponse>(queryInfo.Query, queryInfo.GetCostPoints(), queryInfo.Variables, cancellationToken);

			if (response?.TEntityData != null)
			{
				if(subConnectionFieldNames.Length > 0)
				{
					var firstResultData = response?.TEntityData;
					Dictionary<Type, (PropertyInfo matchedItem, IList result, bool needGet)> subConnectionDic = new Dictionary<Type, (PropertyInfo, IList, bool)>();
					bool needGet = false;
					dynamic connectionValue = null;
					foreach (string fieldName in subConnectionFieldNames)
					{
						var matchedSubItem = response.TEntityData.GetType().GetProperty(fieldName, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
						if (matchedSubItem != null && matchedSubItem.PropertyType.IsGenericType && matchedSubItem.PropertyType.GetGenericTypeDefinition() == typeof(Connection<>))
						{
							Type fieldType = matchedSubItem.PropertyType.GenericTypeArguments[0];
							var result = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(fieldType));
							var subItemValue = matchedSubItem.GetValue(response.TEntityData);
							if (subItemValue != null)
							{
								connectionValue = subItemValue as dynamic;

								bool needGetSubItem = HasNextPageData(fieldType, connectionValue?.PageInfo, queryInfo);
								if (needGetSubItem)
								{
									foreach(var node in (IList)connectionValue.Nodes)
									{
										result.Add(node);
									}
									
									subConnectionDic[fieldType] = (matchedSubItem, result, needGetSubItem);
								}
								needGet = needGetSubItem || needGet;
							}
						}
					}

					while (needGet)
					{
						response = await GraphQLAPIClient.ExecuteAsync<TEntityResponse>(queryInfo.Query, queryInfo.GetCostPoints(), queryInfo.Variables, cancellationToken);
						needGet = false;
						var copyDict = subConnectionDic.ToDictionary(entry => entry.Key, entry => entry.Value);
						if (response?.TEntityData != null)
						{
							foreach(var subConnection in copyDict)
							{
								if(subConnection.Value.needGet)
								{
									var subItemValue = subConnection.Value.matchedItem.GetValue(response.TEntityData);
									if (subItemValue != null)
									{
										connectionValue = subItemValue as dynamic;
										var result = subConnection.Value.result;
										foreach (var node in (IList)connectionValue.Nodes)
										{
											result.Add(node);
										}
										
										bool needGetSubItem = HasNextPageData(subConnection.Key, connectionValue?.PageInfo, queryInfo);
										subConnectionDic[subConnection.Key] = (subConnection.Value.matchedItem, result, needGetSubItem);
										needGet = needGetSubItem || needGet;
									}
								}
							}
						}
					}

					foreach (var subConnection in subConnectionDic)
					{
						object connectionInstance = Activator.CreateInstance(subConnection.Value.matchedItem.PropertyType);
						subConnection.Value.matchedItem.PropertyType.GetProperty(nameof(Connection<BCAPIEntity>.Nodes)).SetValue(connectionInstance, subConnection.Value.result);
						subConnection.Value.matchedItem.SetValue(firstResultData, connectionInstance);
					}
					response.TEntityData = firstResultData;
				}
			}
			return response?.TEntityData;
		}

		/// <summary>
		/// Get the entity data list
		/// </summary>
		/// <typeparam name="TEntityData">The entity data type</typeparam>
		/// <typeparam name="TNodesResponse">The nodes container of entity data</typeparam>
		/// <typeparam name="TEntitiesResponse">The response container of nodes list</typeparam>
		/// <param name="queryInfo">The query info object</param>
		/// <param name="cancellationToken"></param>
		/// <returns>The list of entity data</returns>
		public async Task<IEnumerable<TEntityData>> GetAllAsync<TEntityData, TNodesResponse, TEntitiesResponse>(GraphQLQueryInfo queryInfo, CancellationToken cancellationToken = default)
			where TEntityData : BCAPIEntity, new()
			where TNodesResponse : class, INodeListResponse<TEntityData>, new()
			where TEntitiesResponse : class, IEntitiesResponses<TEntityData, TNodesResponse>, new()
		{
			List<TEntityData> result = new List<TEntityData>();
			bool needGet = true; 
			while (needGet)
			{
				cancellationToken.ThrowIfCancellationRequested();
				var response = await GraphQLAPIClient.ExecuteAsync<TEntitiesResponse>(queryInfo.Query, queryInfo.GetCostPoints(), queryInfo.Variables, cancellationToken);
				if (response?.TEntitiesData != null)
				{
					var entities = response.TEntitiesData.Nodes;
					if (entities != null)
					{
						result.AddRange(entities);
						needGet = HasNextPageData(typeof(TEntityData), response.TEntitiesData.PageInfo, queryInfo);
					}
					else
						needGet = false;
				}
				else
					needGet = false;
			}
			return result;
		}

		/// <summary>
		/// Get the entity data list based on Edges
		/// </summary>
		/// <typeparam name="TEntityData">The entity data type</typeparam>
		/// <typeparam name="TNodeResponse">The node container of entity data</typeparam>
		/// <typeparam name="TEdgesResponse">The edges container of node</typeparam>
		/// <typeparam name="TEntitiesResponse">The response container of edges list</typeparam>
		/// <param name="queryInfo">The query info object</param>
		/// <param name="cancellationToken"></param>
		/// <returns>The list of entity data</returns>
		public async Task<IEnumerable<TEntityData>> GetAllFromEdgesAsync<TEntityData, TNodeResponse, TEdgesResponse, TEntitiesResponse>(GraphQLQueryInfo queryInfo, CancellationToken cancellationToken = default)
			where TEntityData : BCAPIEntity, new()
			where TNodeResponse : class, INodeResponse<TEntityData>, new()
			where TEdgesResponse : class, IEdgesResponse<TEntityData, TNodeResponse>, new()
			where TEntitiesResponse : class, IEntitiesResponses<TEntityData, TEdgesResponse>, new()
		{
			List<TEntityData> result = new List<TEntityData>();
			bool needGet = true;
			while (needGet)
			{
				cancellationToken.ThrowIfCancellationRequested();
				var response = await GraphQLAPIClient.ExecuteAsync<TEntitiesResponse>(queryInfo.Query, queryInfo.GetCostPoints(), queryInfo.Variables, cancellationToken);
				if (response?.TEntitiesData.Edges != null)
				{
					var entities = response.TEntitiesData.Edges;
					if (entities != null)
					{
						result.AddRange(entities.Select(x => x.Node));
						needGet = HasNextPageData(typeof(TEntityData), response.TEntitiesData.PageInfo, queryInfo);
					}
					else
						needGet = false;
				}
				else
					needGet = false;
			}
			return result;
		}

		/// <summary>
		/// Create/Update/Delete entity data  
		/// </summary>
		/// <typeparam name="TMutationObject">The mutation object type</typeparam>
		/// <param name="queryInfo">The query info object</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<TMutationObject> MutationAsync<TMutationObject>(GraphQLQueryInfo queryInfo, CancellationToken cancellationToken = default)
			where TMutationObject : class, new()
		{
			cancellationToken.ThrowIfCancellationRequested();
			return await GraphQLAPIClient.ExecuteAsync<TMutationObject>(queryInfo.Query, queryInfo.GetCostPoints(), queryInfo.Variables, cancellationToken);
		}

		/// <summary>
		/// Check whether have next page data
		/// </summary>
		/// <param name="pageInfo">The page info from API response</param>
		/// <param name="queryInfo">The query info object</param>
		/// <returns></returns>
		protected bool HasNextPageData(Type dataObjType, PageInfo pageInfo, GraphQLQueryInfo queryInfo)
		{
			if (pageInfo != null && pageInfo.HasNextPage == true && !string.IsNullOrEmpty(pageInfo.EndCursor))
			{
				return queryInfo.SetArgumentVariable(dataObjType, GraphQLConstants.Connection.Arguments.After, pageInfo.EndCursor);
			}
			return false;
		}

		/// <summary>
		/// Check response, if it includes the entity data level errors, throw error 
		/// </summary>
		/// <param name="userErrors"></param>
		protected virtual void CheckIfHaveErrors(MutationUserError[] userErrors)
		{
			if (userErrors?.Length > 0)
				throw new PXException(string.Join(";", userErrors.Select(x => x.Message ?? string.Empty)));
		}
	}
}
