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

using GraphQL;
using PX.Common;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Client.Http;
using Newtonsoft.Json;
using PX.Data;
using System.Collections.Generic;
using System;
using System.Linq;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// A GraphQL client for performing GraphQL queries to Shopify's GraphQL API.
	/// </summary>
	public class SPGraphQLAPIClient : GraphQLAPIClientBase
	{
		private readonly int _maxAttemptRecallAPI = WebConfig.GetInt(ShopifyConstants.CommerceShopifyMaxAttempts, 5); //How many times should we retry....???
		private readonly int _delayApiCallTime = WebConfig.GetInt(ShopifyConstants.CommerceShopifyDelayTimeIfFailed, 500); //500ms
		private readonly string apiIdentifyId;
		protected int retryCount;

		/// <summary>
		/// Initializes a new instance of the client for the specified endpoint that uses the
		/// provided HttpClient and optional JsonSerialize settings.
		/// </summary>
		/// <param name="endpoint">The end point of the GraphQL API.</param>
		/// <param name="serializerSettings">Optional Json serializer settings.</param>
		public SPGraphQLAPIClient(string endpoint, string token, JsonSerializerSettings serializerSettings = null) :
			base(endpoint, serializerSettings)
		{
			HttpClient.DefaultRequestHeaders.Add(ShopifyGraphQLConstants.TokenHeaderName, token);
			apiIdentifyId = token;
		}

		/// <summary>
		/// Prepares and sends the query to the GraphQL API and returns the results or an exception.
		/// </summary>
		/// <param name="query">The query to send.</param>
		/// <param name="queryCost">The cost point of the query</param>
		/// <param name="variables">Variables to include in the request</param>
		/// <param name="cancellationToken">Cancellation token for the task.</param>
		/// <returns>A  containing the response as an instance of <typeparamref name="TQueryRoot"/>.</returns>
		/// <exception cref="GraphQLAggregateException">Thrown if there are any errors in the query.
		/// An aggregate exception that contains a list of <see cref="GraphQLException"/>s describing
		/// errors from the GraphQL API.</exception>
		public override async Task<TQueryRoot> ExecuteAsync<TQueryRoot>(string query, int queryCost, Dictionary<string, object> variables = null, CancellationToken cancellationToken = default)
		{
			var request = PrepareRequest(query, variables);
			var requestRateController = RequestRateControllers.GetController(apiIdentifyId);
			GraphQLResponse<TQueryRoot> response = null;

			if (requestRateController != null)
			{
				int attemptRecallAPI = 1;
				while (attemptRecallAPI <= _maxAttemptRecallAPI)
				{
					await requestRateController.GrantAccessAsync(queryCost);
					try
					{
						response = await SendQueryAsync<TQueryRoot>(request, cancellationToken);
						var cost = GetCostExtension(response);
						await requestRateController.UpdateControllerAsync(cost);

						if (response?.Errors?.Any() == true)
						{
							throw new GraphQLAggregateException(response.Errors
								.Select(e => new GraphQLException(e)).ToArray());
						}
						return HandleResponse(response);
					}
					catch (GraphQLHttpRequestException ex)
					{
						Logger.Error("Error during a call to SPGraphQLAPIService; Exception: {EMessage}", ex.Message);
						if (!string.IsNullOrEmpty(ex?.StatusCode.ToString()) && int.TryParse(ex?.StatusCode.ToString(), out var intCode) && intCode == 429)
						{
							attemptRecallAPI++;
							await Task.Delay(_delayApiCallTime);
						}
						else
							throw;
					}
				}
			}
			throw new PXException(ShopifyMessages.TooManyApiCalls);
		}

		protected override TQueryRoot HandleResponse<TQueryRoot>(GraphQLResponse<TQueryRoot> response)
		{
			return base.HandleResponse(response);
		}

		protected virtual CostExtension GetCostExtension<TQueryRoot>(GraphQLResponse<TQueryRoot> response)
		{
			CostExtension cost = null;
			object costExtension = null;
			if (response?.Extensions?.TryGetValue(ShopifyGraphQLConstants.Cost, out costExtension) ?? false)
			{
				if(costExtension != null)
					cost = JsonConvert.DeserializeObject<CostExtension>(JsonConvert.SerializeObject(costExtension));
			}

			return cost;
		}
	}
}
