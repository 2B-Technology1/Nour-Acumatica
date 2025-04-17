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
using System.Net;
using System.Threading;
using PX.Commerce.Core;
using PX.Common;
using PX.Data;
using RestSharp;
using RestSharp.Serializers;

namespace PX.Commerce.BigCommerce.API.REST
{
    public abstract class BCRestClientBase : RestClient
	{
		public Serilog.ILogger Logger { get; set; } = null;
        protected readonly int commerceRetryCount = WebConfig.GetInt(BCConstants.COMMERCE_RETRY_COUNT, 3);
		private IDeserializer _deserializer;
        protected BCRestClientBase(IRestSerializer serializer, IRestOptions options, Serilog.ILogger logger)
        {
			_deserializer = serializer.Deserializer;
			UseSerializer(() => serializer);
            Authenticator = new Autentificator(options.XAuthClient, options.XAuthTocken);
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

        public RestResponse<T> Execute<T>(RestRequest request)
        {
            int retryCount = 0;
            while (true)
            {
                var response = RestResponse<T>.FromResponse(Execute(request));
                if (response.StatusCode != default(HttpStatusCode))
				{
					try
					{
						response.Data = _deserializer.Deserialize<T>(response);
					}
					catch
					{
						response.Data = default(T);
					}
                    return response;
				}
                else if (retryCount < commerceRetryCount)
                {
                    this.Logger?.ForContext("Scope", new BCLogTypeScope(GetType()))
                        .Error("{CommerceCaption}: Operation '{OperationType}' for failed, RetryCount {RetryCount}, Exception {ExceptionMessage}",
                        BCCaptions.CommerceLogCaption, request.Method, retryCount, response.ErrorException.ToString());

                    retryCount++;
                    Thread.Sleep(1000 * retryCount);
                }
				else throw new PXException(BCMessages.RetryLimitIsExceeded, response.ErrorException);
            }
        }

        public RestResponse Execute(RestRequest request)
        {
            int retryCount = 0;
            while (true)
            {
                var response = base.Execute(request);
                if (response.StatusCode != default(HttpStatusCode))
                    return response;
                else if (retryCount < commerceRetryCount)
                {
                    this.Logger?.ForContext("Scope", new BCLogTypeScope(GetType()))
                        .Error("{CommerceCaption}: Operation '{OperationType}' for failed, RetryCount {RetryCount}, Exception {ExceptionMessage}",
                        BCCaptions.CommerceLogCaption, request.Method, retryCount, response.ErrorException.ToString());

                    retryCount++;
                    Thread.Sleep(1000 * retryCount);
                }
                else throw new PXException(BCMessages.RetryLimitIsExceeded, response.ErrorException);
            }
        }

        public RestRequest MakeRequest(string url, Dictionary<string, string> urlSegments = null)
        {
            var request = new RestRequest(url) { RequestFormat = DataFormat.Json };

            if (urlSegments != null)
            {
                foreach (var urlSegment in urlSegments)
                {
                    request.AddUrlSegment(urlSegment.Key, urlSegment.Value);
                }
            }

            return request;
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
			Logger.ForContext("Scope", new BCLogTypeScope(GetType()))
				.ForContext("Exception", response.ErrorException?.Message)
				.Error("{CommerceCaption}: {ResponseError}, Status Code: {StatusCode}", BCCaptions.CommerceLogCaption, description, response.StatusCode);
        }
    }
}
