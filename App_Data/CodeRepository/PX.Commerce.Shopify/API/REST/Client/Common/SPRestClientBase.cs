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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PX.Commerce.Core;
using PX.Common;
using PX.Data;
using RestSharp;
using RestSharp.Serializers;

namespace PX.Commerce.Shopify.API.REST
{
	public abstract class SPRestClientBase : RestClient
	{
		private const string HEADER_LIMIT = "X-Shopify-Shop-Api-Call-Limit";
		private readonly string apiIdentifyId;
		private readonly int maxAttemptRecallAPI = WebConfig.GetInt(ShopifyConstants.CommerceShopifyMaxAttempts, 1000);
		private readonly int delayApiCallTime = WebConfig.GetInt(ShopifyConstants.CommerceShopifyDelayTimeIfFailed, 500); //500ms
		protected readonly int commerceRetryCount = WebConfig.GetInt(BCConstants.COMMERCE_RETRY_COUNT, 3);
		protected int retryCount;
		public Serilog.ILogger Logger { get; set; } = null;
		private IDeserializer _deserializer;
		protected SPRestClientBase(IRestSerializer serializer, IRestOptions options, Serilog.ILogger logger)
		{
			_deserializer = serializer.Deserializer;
			UseSerializer(() => serializer);

			this.AddDefaultHeader(ShopifyConstants.XShopifyAccessToken, options.ApiToken);
			apiIdentifyId = options.ApiToken;

			try
			{
				Options.BaseUrl = new Uri(options.BaseUri);
			}
			catch (UriFormatException e)
			{
				throw new UriFormatException("Invalid URL: The format of the URL could not be determined.", e);
			}
			Logger = logger;
		}

		public RestRequest MakeRequest(string url, Dictionary<string, string> urlSegments = null)
		{
			var request = new RestRequest(url) { RequestFormat = DataFormat.Json, Timeout = 300000 };

			if (urlSegments != null)
			{
				foreach (var urlSegment in urlSegments)
				{
					request.AddUrlSegment(urlSegment.Key, urlSegment.Value);
				}
			}

			return request;
		}

		protected RestResponse ExecuteRequest(RestRequest request)
		{
			var requestRateController = RequestRateControllers.GetController(apiIdentifyId);
			RestResponse response = null;
			if (requestRateController != null)
			{
				int attemptRecallAPI = 1;
				while (attemptRecallAPI <= maxAttemptRecallAPI)
				{

					requestRateController.GrantAccess();
					try
					{
						response = Execute(request);
						CheckResponse(response);
						requestRateController.UpdateController(response.Headers?.FirstOrDefault(x => string.Equals(x.Name, HEADER_LIMIT, StringComparison.OrdinalIgnoreCase))?.Value);
						return response;
					}
					catch (RestShopifyApiCallLimitException ex)
					{
						attemptRecallAPI++;
						Task.Delay(delayApiCallTime);
					}
				}
			}
			throw new Exception(ShopifyMessages.TooManyApiCalls);
		}

		protected RestResponse<TR> ExecuteRequest<TR>(RestRequest request) where TR : class, new()
		{
			var requestRateController = RequestRateControllers.GetController(apiIdentifyId);
			RestResponse<TR> response = null;
			retryCount = 0;
			if (requestRateController != null)
			{
				int attemptRecallAPI = 1;
				while (attemptRecallAPI <= maxAttemptRecallAPI)
				{

					requestRateController.GrantAccess();
					try
					{
						response = RestResponse<TR>.FromResponse(ExecuteRequest(request));
						CheckResponse(response);
						requestRateController.UpdateController(response.Headers?.FirstOrDefault(x => string.Equals(x.Name, HEADER_LIMIT, StringComparison.OrdinalIgnoreCase))?.Value);
						try
						{
							response.Data = _deserializer.Deserialize<TR>(response);
						}
						catch
						{
							response.Data = default(TR);
						}
						return response;
					}
					catch (RestShopifyApiCallLimitException ex)
					{
						attemptRecallAPI++;
						Task.Delay(delayApiCallTime);
					}
				}
			}
			throw new PXException(ShopifyMessages.TooManyApiCalls);
		}

		protected void LogError(Uri baseUrl, RestRequest request, RestResponse response)
		{
			//Get the values of the parameters passed to the API
			var parameters = string.Join(", ", request.Parameters.Select(x => x.Name.ToString() + "=" + (x.Value ?? "NULL")).ToArray());

			//Set up the information message with the URL, the status code, and the parameters.
			var info = "Request to " + baseUrl.AbsoluteUri + request.Resource + " failed with status code " + response.StatusCode + ", parameters: " + parameters;
			var description = "Response content: " + response.Content;

			//Acquire the actual exception
			var ex = (response.ErrorException?.Message) ?? info;

			//Log the exception and info message
			Logger?.ForContext("Scope", new BCLogTypeScope(GetType()))
				.Error(response.ErrorException, "{CommerceCaption}: {ResponseError}", BCCaptions.CommerceLogCaption, description);
		}

		protected void CheckResponse(RestResponse response)
		{
			if (!string.IsNullOrEmpty(response?.StatusCode.ToString()) && int.TryParse(response?.StatusCode.ToString(), out var intCode) && intCode == 429)
			{
				throw new RestShopifyApiCallLimitException(response);
			}
			else if (response?.StatusCode == default(System.Net.HttpStatusCode))
			{
				if (retryCount < commerceRetryCount)
				{
					retryCount++;
					Thread.Sleep(1000 * retryCount);
				}
				else throw new PXException(BCMessages.RetryLimitIsExceeded, response.ErrorException);
			}
		}
	}
}
