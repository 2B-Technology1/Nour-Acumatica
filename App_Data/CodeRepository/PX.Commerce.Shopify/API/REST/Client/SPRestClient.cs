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

using PX.Commerce.Core;
using RestSharp;
using RestSharp.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PX.Commerce.Shopify.API.REST
{
	public class SPRestClient : SPRestClientBase, IShopifyRestClient
	{
		private ISerializer _serializer;
		public SPRestClient(IRestSerializer serializer, IRestOptions options, Serilog.ILogger logger) : base(serializer, options, logger)
		{
			_serializer = serializer.Serializer;
		}

		#region API Request

		public T Post<T, TR>(RestRequest request, T obj, bool usingTRasBodyObj = true)
			where T : class, new() 
			where TR : class, IEntityResponse<T>, new()
		{
			request.Method = Method.Post;
			object _obj = usingTRasBodyObj ? (object)(new TR() { Data = obj }) : (object)obj;
			request.AddJsonBody(_serializer.Serialize(_obj));
			var response = ExecuteRequest<TR>(request);
			if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
			{
				T result = response.Data?.Data;
                if (result == null && response.ErrorException != null)
                {
					LogError(Options.BaseUrl, request, response);
                    throw response.ErrorException;
                }
                if (result != null && result is BCAPIEntity) (result as BCAPIEntity).JSON = response.Content;

				return result;
			}

			LogError(Options.BaseUrl, request, response);

			throw new RestException(response);
		}

		public bool Post(RestRequest request)
		{
			request.Method = Method.Post;
			var response = ExecuteRequest(request);
			if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
			{
				return true;
			}

			LogError(Options.BaseUrl, request, response);

			throw new RestException(response);
		}

		public T Put<T, TR>(RestRequest request, T obj, bool usingTRasBodyObj = true)
			where T : class, new() where TR : class, IEntityResponse<T>, new()
		{
			request.Method = Method.Put;
			object _obj = usingTRasBodyObj ? (object)(new TR() { Data = obj }) : (object)obj;
			request.AddJsonBody(_serializer.Serialize(_obj));
			var response = ExecuteRequest<TR>(request);
			if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
			{
				T result = response.Data?.Data;
                if (result == null && response.ErrorException != null)
                {
					LogError(Options.BaseUrl, request, response);
                    throw response.ErrorException;
                }
                if (result != null && result is BCAPIEntity) (result as BCAPIEntity).JSON = response.Content;

				return result;
			}

			LogError(Options.BaseUrl, request, response);

			throw new RestException(response);
		}

		public bool Delete(RestRequest request)
		{
			request.Method = Method.Delete;
			var response = ExecuteRequest(request);
			if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound)
			{
				return true;
			}

			LogError(Options.BaseUrl, request, response);
			throw new RestException(response);
		}

		public T Get<T, TR>(RestRequest request)
			where T : class, new() where TR : class, IEntityResponse<T>, new()
		{
			return Get<T, TR>(request, out _);
		}

		public T Get<T, TR>(RestRequest request, out IList<HeaderParameter> headers)
			where T : class, new() where TR : class, IEntityResponse<T>, new()
		{
			request.Method = Method.Get;
			var response = ExecuteRequest<TR>(request);
			headers = response.Headers.ToList();
			if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound)
			{
				T result = response.Data?.Data;

				if (result != null && result is BCAPIEntity) (result as BCAPIEntity).JSON = response.Content;
                if(result == null && response.ErrorMessage != null)
                {
					LogError(Options.BaseUrl, request, response);
                    throw response.ErrorException;
                }
				return result;
			}

			LogError(Options.BaseUrl, request, response);
			throw new RestException(response);
		}

		public IEnumerable<T> GetCurrentList<T, TR>(RestRequest request, out string previousList, out string nextList) where T : class, new() where TR : class, IEntitiesResponse<T>, new()
		{
			request.Method = Method.Get;
			var response = ExecuteRequest<TR>(request);
			previousList = nextList = default;
			if (response.StatusCode == HttpStatusCode.OK)
			{
				var responseHeader = response.Headers;
				var entities = response.Data?.Data;
                if (entities == null && response.ErrorException != null)
                {
					LogError(Options.BaseUrl, request, response);
                    throw response.ErrorException;
                }
                if (entities != null && entities.Any())
				{
					if (TryGetNextPageUrl(responseHeader.ToList(), out var previousUrl, out var nextUrl))
					{
						previousList = previousUrl;
						nextList = nextUrl;
					}
				}
				return entities;
			}

			LogError(Options.BaseUrl, request, response);
			throw new RestException(response);
		}

		public IEnumerable<T> GetAll<T, TR>(RestRequest request, CancellationToken cancellationToken = default) where T : class, new() where TR : class, IEntitiesResponse<T>, new()
		{
			request.Method = Method.Get;
            bool needGet = true;
            while(needGet)
            {
				cancellationToken.ThrowIfCancellationRequested();
                var response = ExecuteRequest<TR>(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseHeader = response.Headers;
                    var entities = response.Data?.Data;
                    if (entities == null && response.ErrorException != null)
                    {
						LogError(Options.BaseUrl, request, response);
                        throw response.ErrorException;
                    }
                    if (entities != null && entities.Any())
                    {
                        foreach (T entity in entities)
                        {
                            yield return entity;
                        }
                        if (TryGetNextPageUrl(responseHeader.ToList(), out _, out var nextUrl))
                        {
                            request = MakeRequest(nextUrl);
                            request.Method = Method.Get;
                            needGet = true;
                        }
                        else
                            needGet = false;
                    }
                    else
                        yield break;
                }
                else
                {
                    LogError(Options.BaseUrl, request, response);
                    throw new RestException(response);
                }
            }	
		}

		private bool TryGetNextPageUrl(IList<HeaderParameter> header, out string previousUrl, out string nextUrl)
		{
			previousUrl = nextUrl = default;
			if (header == null || header.Count == 0) return false;
			var paraLink = header.FirstOrDefault(x => string.Equals(x.Name, "Link", StringComparison.InvariantCultureIgnoreCase));
			if (paraLink != null && paraLink.Value != null && !string.IsNullOrWhiteSpace(paraLink.Value.ToString()))
			{
				var linkStr = paraLink.Value.ToString();
				Match previousMatch = Regex.Match(linkStr, $@"<{Options.BaseUrl}([^\s]*)>;\s*rel=""previous""", RegexOptions.IgnoreCase);
				if (previousMatch.Success && !string.IsNullOrWhiteSpace(previousMatch.Groups[1].Value))
				{
					previousUrl = previousMatch.Groups[1].Value;
				}
				Match nextMatch = Regex.Match(linkStr, $@"<{Options.BaseUrl}([^\s]*)>;\s*rel=""next""", RegexOptions.IgnoreCase);
				if (nextMatch.Success && !string.IsNullOrWhiteSpace(nextMatch.Groups[1].Value))
				{
					nextUrl = nextMatch.Groups[1].Value;
					if (Regex.IsMatch(nextUrl, $@"limit=\d+", RegexOptions.IgnoreCase))
						nextUrl = Regex.Replace(nextUrl, $@"limit=\d+", "limit=250", RegexOptions.IgnoreCase);
					else
						nextUrl += "&limit=250";
					return true;
				}
			}
			return false;
		}
		#endregion
	}
}
