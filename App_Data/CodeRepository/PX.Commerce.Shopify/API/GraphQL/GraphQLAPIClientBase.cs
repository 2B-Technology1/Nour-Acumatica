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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Newtonsoft.Json;
using PX.Data;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// A service which performs GraphQL queries to an external GraphQL API and returns the results.
	/// </summary>
	/// <typeparam name="TQueryRoot">A type representing the Query tree of the GraphQL API
	/// that responses will be deserialized into.</typeparam>
	public abstract class GraphQLAPIClientBase : GraphQLHttpClient, IGraphQLAPIClient
	{
		/// <summary>
		/// Logger used by the client.
		/// </summary>
		[InjectDependency]
		public Serilog.ILogger Logger { get; set; }

		/// <summary>
		/// Constructs a new instance of the GraphQLAPIService using the specified HttpClient.
		/// </summary>
		/// <param name="endpoint">The endpoint of the GraphQL API.</param>
		/// <param name="serializerSettings">Optional json serializer settings to use instead of defaults.</param>
		public GraphQLAPIClientBase(string endpoint, JsonSerializerSettings serializerSettings = null) :
			base(endpoint, new NewtonsoftJsonSerializer(serializerSettings ?? NewtonsoftJsonSerializer.DefaultJsonSerializerSettings))
		{
			if (string.IsNullOrEmpty(endpoint))
				throw new PXException(GraphQLMessages.GraphQLClientEmptyEndpoint);
			Uri endpointUri = null;
			try
			{
				endpointUri = new Uri(endpoint);
			}
			catch (UriFormatException e)
			{
				throw new UriFormatException(GraphQLMessages.GraphQLClientInvalidEndpointFormat, e);
			}
		}

		public GraphQLAPIClientBase(GraphQLHttpClientOptions options, IGraphQLWebsocketJsonSerializer serializer, HttpClient httpClient) : base(options, serializer, httpClient) { }

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
		public virtual async Task<TQueryRoot> ExecuteAsync<TQueryRoot>(string query, int queryCost, Dictionary<string, object> variables = null, 
			CancellationToken cancellationToken = default)
		{
			var request = PrepareRequest(query, variables);

			GraphQLResponse<TQueryRoot> response = await SendQueryAsync<TQueryRoot>(request, cancellationToken);

			if (response?.Errors?.Any() ?? false)
			{
				throw new GraphQLAggregateException(response.Errors
					.Select(e => new GraphQLException(e)).ToArray());
			}

			return HandleResponse(response);
		}

		/// <summary>
		/// Prepares a GraphQLRequest object from the passed query and variables.
		/// </summary>
		/// <param name="query">The query to use in the request.</param>
		/// <param name="variables">Any variables to use in the request.</param>
		/// <returns>A GraphQLRequest object.</returns>
		protected virtual GraphQLRequest PrepareRequest(string query, Dictionary<string, object> variables)
		{
			return new GraphQLRequest(query, variables);
		}

		/// <summary>
		/// Takes a GraphQLResponse object and returns the data.
		/// </summary>
		/// <param name="response">The GraphQL response to handle.</param>
		/// <returns>The data inside the response.</returns>
		protected virtual TQueryRoot HandleResponse<TQueryRoot>(GraphQLResponse<TQueryRoot> response)
		{
			return response.Data;
		}
	}
}
