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
using System.Net.Http;
using System.Threading.Tasks;
using Autofac;
using IdentityModel;
using IdentityModel.Client;
using Newtonsoft.Json;
using PX.CloudServices;
using PX.CloudServices.Diagnostic;
using PX.Data;
using PX.OAuthClient.DAC;
using PX.OAuthClient.Handlers;
using PX.OAuthClient.Processors;
using PX.Objects.CS;
using PX.Objects.Localizations.GB.HMRC.Model;
using Module = Autofac.Module;

namespace PX.Objects.Localizations.GB.HMRC
{
	public class MTDCloudApplicationProcessor : IExternalApplicationProcessor, ICloudServiceClientWithDiagnostics
	{
		private readonly ICloudServiceClient _cloudServiceClient;
		private const string DiscoveryServiceName = "hmrc-making-tax-digital";

		public MTDCloudApplicationProcessor(ICloudServiceClientFactory cloudServiceClientFactory)
		{
			_cloudServiceClient = cloudServiceClientFactory.CreateClient(typeof(MTDCloudApplicationProcessor), DiscoveryServiceName);
		}

		public string ServiceName => DiscoveryServiceName;
		public string Description => TypeName;
		public bool IsActive => PXAccess.FeatureInstalled<FeaturesSet.uKLocalization>();
		public string HowToActivate => PXMessages.LocalizeFormatNoPrefix(Messages.HowToEnableService,
			CS.Messages.UKLocalizationFeatureName);
		public Task<IEnumerable<(bool Success, string Message)>> TestAsync() =>	Task.FromResult(Enumerable.Empty<(bool, string)>());

		public void SignIn(OAuthApplication oAuthApplication, ref OAuthToken token)
		{
			var redirectUri = AuthenticationHandler.ReturnUrl;
			object key = Guid.NewGuid();

			Uri url;
			try
			{
				url = GetServiceUrlAsync(oAuthApplication.ApplicationID.ToString(), redirectUri).Result;
			}
			catch (Exception e)
			{
				string UrlError = e.ToString();
				PXTrace.WriteError(UrlError);
				throw new PXException(Messages.UnretrievableRedirectURL);
			}

			var redirectUrl = url.ToString();

			if (!string.IsNullOrEmpty(redirectUrl))
			{
				throw new PXRedirectToUrlException(redirectUrl, PXBaseRedirectException.WindowMode.InlineWindow,
					"Authenticate");
			}
		}

		public void ProcessAuthorizationCode(string code, OAuthApplication application, OAuthToken token)
		{
			var redirectUri = AuthenticationHandler.ReturnUrl;
			object key = Guid.NewGuid();
			PXLongOperation.StartOperation(key, () =>
				{
					ProcessAuthorizationCodeImpl(code, application, token, redirectUri).Wait();
				}
			);
			PXLongOperation.WaitCompletion(key);
		}

		public void RefreshAccessToken(OAuthToken token, OAuthApplication oAuthApplication)
		{
			object key = Guid.NewGuid();
			PXLongOperation.StartOperation(key, () =>
				{
					RefreshAccessTokenImpl(token, oAuthApplication).Wait();
				}
			);
			PXLongOperation.WaitCompletion(key);
		}

		private async Task ProcessAuthorizationCodeImpl(string code, OAuthApplication application, OAuthToken token,
			string redirectUri)
		{
			var baseUrl = await _cloudServiceClient.GetServiceUrlAsync();
			var authorization = await _cloudServiceClient.GetAuthorizationHeaderAsync();
			using (var httpClient = _cloudServiceClient.CreateHttpClient())
			{
				var tokenResponse = await httpClient.RequestAuthorizationCodeTokenAsync(
					new AuthorizationCodeTokenRequest
					{
						Address = new Uri(baseUrl, "oauth2/token").ToString(),
						Code = code,
						RedirectUri = redirectUri,
						Headers = { Authorization = authorization },
					});
				FillTokenFromTokenResponse(tokenResponse, token);
			}
		}

		private async Task RefreshAccessTokenImpl(OAuthToken token, OAuthApplication oAuthApplication)
		{
			var baseUrl = await _cloudServiceClient.GetServiceUrlAsync();
			var authorization = await _cloudServiceClient.GetAuthorizationHeaderAsync();
			using (var httpClient = _cloudServiceClient.CreateHttpClient())
			{
				var refreshToken = token.RefreshToken;
				var tokenResult = await httpClient.RequestRefreshTokenAsync(new RefreshTokenRequest
				{
					Address = new Uri(baseUrl, "oauth2/token").ToString(),
					RefreshToken = refreshToken,
					Headers = { Authorization = authorization },
				});
				FillTokenFromTokenResponse(tokenResult, token);
			}
		}

		public Dictionary<string, string> GetAPIUrls()
		{
			Guid key = Guid.NewGuid();
			PXLongOperation.StartOperation(key, () =>
			{
				try
				{
					var urls = GetAPIUrlsImpl().GetAwaiter().GetResult();
					PXLongOperation.SetCustomInfo(urls, $"Urls");
				}
				catch (Exception e)
				{
					PXLongOperation.SetCustomInfo(e, $"Error");
				}
			}
			);
			PXLongOperation.WaitCompletion(key);

			object customInfo = PXLongOperation.GetCustomInfo(key, $"Urls");
			if (customInfo is Dictionary<string, string>)
			{
				return customInfo as Dictionary<string, string>;
			}

			object error = PXLongOperation.GetCustomInfo(key, $"Error");
			if (error is Exception)
			{
				PXTrace.WriteError((Exception)error);
			}

			return new Dictionary<string, string>();
		}

		private async Task<Dictionary<string, string>> GetAPIUrlsImpl()
		{
			var baseUrl = await _cloudServiceClient.GetServiceUrlAsync();
			var authorization = await _cloudServiceClient.GetAuthorizationHeaderAsync();
			Dictionary<string, string> result = new Dictionary<string, string>();
			using (var httpClient = _cloudServiceClient.CreateHttpClient())
			{
				httpClient.DefaultRequestHeaders.Authorization = authorization;
				var response = await httpClient.GetAsync(baseUrl);
				if (response.IsSuccessStatusCode
					&& response.Content.Headers.ContentType.ToString().ToLower().Contains("application/json"))
				{
					string urlsStr = await response.Content.ReadAsStringAsync();
					try
					{
						result = JsonConvert.DeserializeObject<Dictionary<string, string>>(urlsStr);
					}
					catch (JsonException e)
					{
						PXTrace.WriteError(e);
					}
				}
				else if (!response.Content.Headers.ContentType.ToString().ToLower().Contains("application/json"))
				{
					throw new PXException(Messages.NonJSONContentType);
				}
			}
			return result;
		}

		public async Task<string> GetApplicationRestrictedToken()
		{
			string token;
			try
			{
				var baseUrl = await _cloudServiceClient.GetServiceUrlAsync();
				var authorization = await _cloudServiceClient.GetAuthorizationHeaderAsync();
				using (var httpClient = _cloudServiceClient.CreateHttpClient())
				{
					var tokenResult = await httpClient.RequestClientCredentialsTokenAsync(
						new ClientCredentialsTokenRequest
						{
							Address = new Uri(baseUrl, "oauth2/token").ToString(),
							Headers = { Authorization = authorization },
						}
					);
					if (tokenResult.IsError) throw new PXException(tokenResult.Error);
					token = tokenResult.AccessToken;
				}
			}
			catch
			{
				throw new PXException(Messages.UnretrievableToken);
			}

			return token;
		}

		private void FillTokenFromTokenResponse(TokenResponse tokenResponse, OAuthToken token)
		{
			token.AccessToken = tokenResponse.AccessToken;
			token.RefreshToken = tokenResponse.RefreshToken;
			token.UtcExpiredOn = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
			token.Bearer = string.Empty;
		}

		public string ResourceHtml(OAuthToken token, OAuthResource resource, OAuthApplication application)
		{
			return string.Empty;
		}

		public async Task<IEnumerable<Resource>> GetResources(OAuthToken token, OAuthApplication application)
		{
			return await Task.FromResult(Enumerable.Empty<Resource>());
		}

		public bool IsSignedIn(OAuthApplication application, OAuthToken Token)
		{
			return !string.IsNullOrEmpty(Token?.AccessToken);
		}

		public const string Type = "RCMTD";
		public string TypeCode => Type;
		public string TypeName => "HMRC Making Tax Digital";
		public bool HasRefreshToken => true;
		public string SignInFailedMessage => "Authentication failed. Sign in to the selected application.";

		private const string Scope = "read:vat write:vat";

		private async Task<Uri> GetServiceUrlAsync(string state, string redirectUri)
		{
			using (var httpClient = _cloudServiceClient.CreateHttpClient())
			{
				var baseUrl = await _cloudServiceClient.GetServiceUrlAsync();
				var authorization = await _cloudServiceClient.GetAuthorizationHeaderAsync();

				var serviceResponse = await httpClient.SendAsync(new HttpRequestMessage
				{
					Method = HttpMethod.Get,
					RequestUri = new Uri(new RequestUrl(new Uri(baseUrl, "oauth2/authorize").ToString())
						.CreateAuthorizeUrl(
							clientId: null, //doesn't matter
							responseType: OidcConstants.ResponseTypes.Code,
							scope: Scope,
							redirectUri: redirectUri,
							state: state
						)),
					Headers = { Authorization = authorization },
				});

				if (serviceResponse.StatusCode != HttpStatusCode.Redirect)
				{
					var response = await serviceResponse.Content.ReadAsStringAsync();

					throw new InvalidOperationException($"Unexpected status code {serviceResponse.StatusCode}, Content: {response}");
				}

				return new Uri(serviceResponse.Headers.Location.ToString());
			}
		}
	}

	public class RegisterModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.BindFromConfiguration<MtdOptions>("hmrc-mtd");
		}
	}
}
