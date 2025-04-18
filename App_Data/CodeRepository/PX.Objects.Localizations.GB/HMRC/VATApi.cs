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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using PX.OAuthClient.DAC;
using Newtonsoft.Json;
using System.Text;
using PX.Data;
using PX.OAuthClient;
using PX.Objects.Localizations.GB.HMRC.Exceptions;
using PX.Objects.Localizations.GB.HMRC.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PX.Objects.Localizations.GB.HMRC
{
	/// <summary>
	/// VAT (MTD) API
	/// https://developer.service.hmrc.gov.uk/api-documentation/docs/api/service/vat-api/1.0
	/// </summary>
	internal class VATApi
	{
		private const string _acceptHeader = "application/vnd.hmrc.1.0+json";
		private const string _contentTypeHeader = "application/json";

		private const string _aPIUrlType = "vat-api/1.0";
		private const string _fPHAPIUrlType = "txm-fph-validator-api/1.0";

		private readonly OAuthApplication _application;
		private readonly OAuthToken _token;
		private string _vrn;
		private readonly string _urlSite;
		private readonly string _urlFPHSite;

		#region Endpoints
		private const string _urlObligations = "/organisations/vat/{vrn}/obligations";
		private const string _urlReturns = "/organisations/vat/{vrn}/returns/{periodKey}";
		private const string _urlSubmitVATreturn = "/organisations/vat/{vrn}/returns";
		private const string _urlLiabilities = "/organisations/vat/{vrn}/liabilities";
		private const string _urlPayments = "/organisations/vat/{vrn}/payments";
		#endregion

		public delegate void UpdateOAuthTokenDelegate(OAuthToken o);

		private readonly ApplicationMaint _graph;

		private IHttpClientFactory _httpClientFactory;

		public static VATApi GetVatApi(VATMaint vatMaint)
		{
			ConnectionInfo info = vatMaint.GetConnectionInfo();
			if (info?.Application == null)
			{
				return null;
			}

			return new VATApi(vatMaint.IsTestEnvironment, info, vatMaint.HttpClientFactory);
		}

		protected VATApi(bool isTestEnvironment, ConnectionInfo info, IHttpClientFactory httpClientFactory)
		{
			_graph = PXGraph.CreateInstance<ApplicationMaint>();
			_application = info.Application;
			_token = info.Token;
			_httpClientFactory = httpClientFactory;
			Dictionary<string, string> apiUrls = null;
			if (_graph._processorFactory.GetProcessorForApplicationType(_application.Type) is MTDCloudApplicationProcessor processor)
			{
				apiUrls = processor.GetAPIUrls();
			}

			string url = null;
			_urlSite = !String.IsNullOrEmpty(apiUrls?.TryGetValue(_aPIUrlType, out url) ?? false ? url : null) ? url : Helper.GetSiteUrl(isTestEnvironment);
			_urlFPHSite = !String.IsNullOrEmpty(apiUrls?.TryGetValue(_fPHAPIUrlType, out url) ?? false ? url : null) ? url : Helper.GetSiteUrl(isTestEnvironment);
			_vrn = info.Vrn;
		}

		/// <summary>
		/// Refresh Access Token if it is need
		/// </summary>
		private void RefreshAccessToken()
		{
			if (_token == null)
			{
				throw new Exceptions.VATAPIInvalidToken(Model.Error.IMPOSSIBLE_TO_REFRESH_TOKEN);
			}
			if (_token.UtcExpiredOn == null || _token.UtcExpiredOn?.AddMinutes(-15) < _token.UtcNow)
			{
				_graph.RefreshAccessToken(_token, _application);
				_graph.Clear();
			}
		}

		/// <summary>
		/// https://developer.service.hmrc.gov.uk/api-documentation/docs/fraud-prevention
		/// https://github.com/hmrc/vat-api/issues/565
		/// https://doc.uzerp.com/modules/accounts/vat/
		/// </summary>
		/// <param name="httpRequest"></param>
		private void addMandatoryHeader(HttpRequestMessage httpRequest, HMRCHeaderData row)
		{
			httpRequest.Headers.Add("Gov-Client-Connection-Method", row.GovClientConnectionMethod);
			httpRequest.Headers.Add("Gov-Client-Public-IP", row.GovClientPublicIP);
			httpRequest.Headers.Add("Gov-Client-Public-IP-Timestamp", row.GovClientPublicIPTimestamp);
			httpRequest.Headers.Add("Gov-Client-Public-Port", row.GovClientPublicPort);
			httpRequest.Headers.Add("Gov-Client-Device-ID", row.GovClientDeviceID);
			httpRequest.Headers.Add("Gov-Client-User-IDs", row.GovClientUserIDs);
			httpRequest.Headers.Add("Gov-Client-Timezone", row.GovClientTimezone);
			httpRequest.Headers.Add("Gov-Client-Local-IPs", row.GovClientLocalIPs);
			httpRequest.Headers.Add("Gov-Client-Local-IPs-Timestamp", row.GovClientLocalIPsTimestamp);
			httpRequest.Headers.Add("Gov-Client-Screens", row.GovClientScreens);
			httpRequest.Headers.Add("Gov-Client-Window-Size", row.GovClientWindowSize);
			httpRequest.Headers.Add("Gov-Client-Browser-Plugins", row.GovClientBrowserPlugins);
			httpRequest.Headers.Add("Gov-Client-Browser-JS-User-Agent", row.GovClientBrowserJSUserAgent);
			httpRequest.Headers.Add("Gov-Client-Browser-Do-Not-Track", row.GovClientBrowserDoNotTrack);
			httpRequest.Headers.Add("Gov-Client-Multi-Factor", row.GovClientMultiFactor);
			httpRequest.Headers.Add("Gov-Vendor-Product-Name", row.GovVendorProductName);
			httpRequest.Headers.Add("Gov-Vendor-Version", row.GovVendorVersion);
			httpRequest.Headers.Add("Gov-Vendor-License-IDs", row.GovVendorLicenseIDs);
			httpRequest.Headers.Add("Gov-Vendor-Public-IP", row.GovVendorPublicIP);
			httpRequest.Headers.Add("Gov-Vendor-Forwarded", row.GovVendorForwarded);
		}

		/// <summary>
		/// Check Api Response
		/// </summary>
		/// <param name="response"></param>
		private async Task CheckApiResponse(HttpResponseMessage response, VATApiType? api = null)
		{
			if (response.IsSuccessStatusCode) return;

			string errJson = await response.Content.ReadAsStringAsync();
			Error err = JsonConvert.DeserializeObject<Error>(errJson);

			if (err == null)
			{
				err = new Error();
				switch (response.StatusCode)
				{
					case HttpStatusCode.NotFound:
						err.code = Error.NOT_FOUND;
						err.message = Error.NOT_FOUND_MSG;
						break;
					default:
						err.code = "HTTP_" + response.StatusCode.ToString();
						break;
				}
			}
			string message = err.message;
			if (err.errors != null)
				message = String.Join("; ", err.errors.Select(o => o.path + " " + o.message));

			Exception exception = null;

			switch (response.StatusCode)
			{
				case HttpStatusCode.Unauthorized: exception = new VATAPIInvalidToken(err.code, message); break;
				default:
					exception = new VATAPIException(err.code, message);
					break;
			}

			if (exception != null)
			{
				exception.Data.Add("errorJson", errJson);
				throw exception;
			}
		}

		/// <summary>
		/// Retrieve VAT obligations
		/// </summary>
		/// <returns></returns>
		public async Task<ObligationResponse> Obligations(ObligationsRequest request, HMRCHeaderData row, string testScenario = null)
		{
			RefreshAccessToken();
			string url = _urlSite + _urlObligations.Replace("{vrn}", _vrn) + "?from=" + request.from?.ToString("yyyy-MM-dd") + "&to=" + request.to?.ToString("yyyy-MM-dd");
			HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
			httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(_acceptHeader));
			httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token.AccessToken);
			addMandatoryHeader(httpRequest, row);

			if (!String.IsNullOrEmpty(testScenario))
			{
				httpRequest.Headers.Add("Gov-Test-Scenario", testScenario);
				//httpRequest.Headers.Add("status", "F");
			}

			ObligationResponse obligationResponse = null;
			string retContent = "";

			using (var httpClient = _httpClientFactory.CreateClient())
			{
				HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
				await CheckApiResponse(response, VATApiType.RetrieveVATobligations);
				retContent = await response.Content.ReadAsStringAsync();
				obligationResponse = JsonConvert.DeserializeObject<ObligationResponse>(retContent);
			}

			PXTrace.WriteInformation(url);
			PXTrace.WriteInformation(retContent);

			return obligationResponse;
		}

		/// <summary>
		/// Submit VAT return for period
		/// </summary>
		/// <param name="periodkey"></param>
		/// <returns></returns>
		public async Task<VATreturn> Returns(string periodkey, HMRCHeaderData row)
		{
			RefreshAccessToken();
			string url = _urlSite + _urlReturns.Replace("{vrn}", _vrn).Replace("{periodKey}", periodkey);
			HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
			httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(_acceptHeader));
			httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token.AccessToken);

			addMandatoryHeader(httpRequest, row);

			VATreturn vATreturn = null;
			string retContent = "";

			using (var httpClient = _httpClientFactory.CreateClient())
			{
				HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
				await CheckApiResponse(response, VATApiType.ViewVATReturn);
				retContent = await response.Content.ReadAsStringAsync();
				vATreturn = JsonConvert.DeserializeObject<VATreturn>(retContent);
			}

			PXTrace.WriteInformation(url);
			PXTrace.WriteInformation(retContent);
			return vATreturn;
		}

		public async Task<VaTreturnResponse> SendReturn(VATreturn vatReturn, HMRCHeaderData row)
		{
			RefreshAccessToken();
			string url = _urlSite + _urlSubmitVATreturn.Replace("{vrn}", _vrn);
			HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
			httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(_acceptHeader));
			httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token.AccessToken);

			addMandatoryHeader(httpRequest, row);

			string json = JsonConvert.SerializeObject(vatReturn, new DecimalJsonConverter());
			httpRequest.Content = new StringContent(json, Encoding.UTF8, _contentTypeHeader);
			string retContent = "";
			VaTreturnResponse ret;

			using (var httpClient = _httpClientFactory.CreateClient())
			{
				HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
				await CheckApiResponse(response, VATApiType.SubmitVATreturnForPeriod);
				retContent = await response.Content.ReadAsStringAsync();
				ret = JsonConvert.DeserializeObject<VaTreturnResponse>(retContent);
			}

			PXTrace.WriteInformation(json);
			PXTrace.WriteInformation(retContent);
			return ret;
		}

		public async Task<string> GetApplicationRestrictedToken()
		{
			if (_graph._processorFactory.GetProcessorForApplicationType(_application.Type) is MTDCloudApplicationProcessor processor)
			{
				string authToken = await processor.GetApplicationRestrictedToken();
				return authToken;
			}

			throw new PXException(Messages.UnretrievableApplicationProcessor);
		}

		public async Task<(HttpRequestMessage req, HttpResponseMessage res)> TestFraudPrevention(HMRCHeaderData row)
		{
			string AccessToken = await GetApplicationRestrictedToken();
			string url = _urlFPHSite + "/test/fraud-prevention-headers/validate";
			HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
			httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(_acceptHeader));
			httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

			addMandatoryHeader(httpRequest, row);

			HttpResponseMessage response;

			using (var httpClient = _httpClientFactory.CreateClient())
			{
				response = await httpClient.SendAsync(httpRequest);
			}

			PXTrace.WriteInformation(httpRequest.ToString());
			PXTrace.WriteInformation(response.ToString());
			PXTrace.WriteInformation(await response.Content.ReadAsStringAsync());

			return (httpRequest, response);
		}
	}
}
