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
using System.IO;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using PX.Objects.GL;

namespace PX.Objects.GL.Consolidation
{
	internal class ConsolidationClient : IDisposable
	{
		protected const string LOGIN_URL = "/entity/auth/login";
		protected const string LOGOUT_URL = "/entity/auth/logout";
		protected const string ENDPOINT_URL = "/entity/GLConsolidation/22.200.001/";
		protected const string JSON_CONTENT_TYPE = "application/json";
		protected const string BRANCH_URL = ENDPOINT_URL + "Branch";
		protected const string ORGANIZATION_URL = ENDPOINT_URL + "Organization";
		protected const string GLCONSOLACCOUNT_URL = ENDPOINT_URL + "ConsolAccount";
		protected const string LEDGER_URL = ENDPOINT_URL + "Ledger";
		protected const string CONSOL_DATA_URL = ENDPOINT_URL + "ConsolidationData";

		protected bool _loggedIn;
		protected string _host;
		protected string _login;
		protected string _password;
		static readonly HttpClient _client = new HttpClient();

		public ConsolidationClient(string url, string login, string password)
		{
			_host = url;
			_login = login;
			_password = password;
			Login();
		}

		public ConsolidationClient(string url, string login, string password, int? timeout)
		{
			_host = url;
			_login = login;
			_password = password;
			if (timeout.HasValue && _client.Timeout.TotalSeconds != timeout.Value)
				_client.Timeout = TimeSpan.FromSeconds(timeout.Value);
			Login();
		}

		public void Dispose()
		{
			if (_loggedIn)
			{
				Logout();
			}
		}
		protected void Login()
		{
			var request = new { name = _login, password = _password };
			var body = Serialize(request);
			try
			{
				var loginTask = Task.Run(() => Post(LOGIN_URL, body));
				loginTask.Wait();
			}
			catch (Exception ex)
			{
				var error = ConsolidationClient.GetServerError(ex);
				throw new PX.Data.PXException(error);
			}
			_loggedIn = true;
		}
		protected void Logout()
		{
			try
			{
				var logoutTask = Task.Run(() => Post(LOGOUT_URL, string.Empty));
				logoutTask.Wait();
			}
			catch (Exception ex)
			{
				var error = ConsolidationClient.GetServerError(ex);
				throw new PX.Data.PXException(error);
			}
			_loggedIn = false;
		}

		public virtual async Task<IEnumerable<ConsolAccountAPI>> GetConsolAccounts()
		{
			var response = await Get(GLCONSOLACCOUNT_URL);
			var tmp = Deserialize<ConsolAccountAPITmp[]>(response);
			var accounts = tmp.Select(_ =>
				new ConsolAccountAPI()
				{
					AccountCD = (string)_.AccountCD.value,
					Description = (string)_.Description.value
				});
			return accounts;
		}
		public virtual async Task UpdateConsolAccount(ConsolAccountAPI account)
		{
			var request = new ConsolAccountAPITmp(account.AccountCD, account.Description);
			var body = Serialize(request);
			await Put(GLCONSOLACCOUNT_URL, body);
		}
		public virtual async Task InsertConsolAccount(ConsolAccountAPI account)
		{
			var request = new ConsolAccountAPITmp(account.AccountCD, account.Description);
			var body = Serialize(request);
			await Put(GLCONSOLACCOUNT_URL, body);
		}
		public virtual async Task DeleteConsolAccount(ConsolAccountAPI account)
		{
			await Delete(GLCONSOLACCOUNT_URL + "/" + account.AccountCD);
		}
		public virtual async Task<IEnumerable<LedgerAPI>> GetLedgers()
		{
			var response = await Get(LEDGER_URL);
			var tmp = Deserialize<LedgerAPITmp[]>(response);
			var ledgers = tmp.Select(_ =>
				new LedgerAPI()
				{
					LedgerCD = (string)_.LedgerCD.value,
					Descr = (string)_.Descr.value,
					BalanceType = ((string)_.BalanceType.value).Substring(0, 1)
				}); ;
			return ledgers;
		}
		public virtual async Task<IEnumerable<BranchAPI>> GetBranches()
		{
			var response = await Get(BRANCH_URL);
			var tmp = Deserialize<BranchAPITmp[]>(response);
			var branches = tmp.Select(_ =>
				new BranchAPI()
				{
					BranchCD = (string)_.BranchCD.value,
					OrganizationCD = (string)_.OrganizationCD.value,
					AcctName = (string)_.AcctName.value,
					LedgerCD = (string)_.LedgerCD.value
				});
			return branches;
		}
		public virtual async Task<IEnumerable<OrganizationAPI>> GetOrganizations()
		{
			var response = await Get(ORGANIZATION_URL);
			var tmp = Deserialize<OrganizationAPITmp[]>(response);
			var organizations = tmp.Select(_ =>
				new OrganizationAPI()
				{
					OrganizationCD = (string)_.OrganizationCD.value,
					OrganizationName = (string)_.OrganizationName.value,
					LedgerCD = (string)_.LedgerCD.value
				});
			return organizations;
		}
		public virtual async Task<IEnumerable<ConsolidationItemAPI>> GetConsolidationData(string ledgerCD, string branchCD)
		{
			var request = new ConsolidationDataParametersAPITmp(ledgerCD, branchCD);
			var body = Serialize(request);
			string response;
			response = await Put(CONSOL_DATA_URL + "?$expand=Result", body);
			var tmp = Deserialize<ConsolidationDataAPITmp>(response);
			var result = tmp.Result.Select(_ => _.ToApiItem()).ToList();
			return result;
		}

		protected virtual async Task<string> Get(string uri)
		{
			var response = await _client.GetAsync(_host + uri);
			var content = await response.Content.ReadAsStringAsync();
			return GetResult(response, content);
		}
		protected virtual async Task<string> Put(string uri, string body)
		{
			var response = await _client.PutAsync(_host + uri, new StringContent(body, Encoding.UTF8, JSON_CONTENT_TYPE));
			var content = await response.Content.ReadAsStringAsync();
			return GetResult(response, content);
		}
		protected virtual async Task<string> Post(string uri, string body)
		{
			var response = await _client.PostAsync(_host + uri, new StringContent(body, Encoding.UTF8, JSON_CONTENT_TYPE));
			var content = await response.Content.ReadAsStringAsync();
			return GetResult(response, content);
		}
		protected virtual async Task<string> Delete(string uri)
		{
			var response = await _client.DeleteAsync(_host + uri);
			var content = await response.Content.ReadAsStringAsync();
			return GetResult(response, content);
		}
		protected string GetResult(HttpResponseMessage response, string content)
		{
			if (response.IsSuccessStatusCode)
			{
				return content;
			}
			else if (response.StatusCode == HttpStatusCode.InternalServerError || (int)response.StatusCode == 422)
			{
				var error = GetInternalError(content);
				throw new ApiException(error);
			}
			else
			{
				throw new ApiException(PX.Data.PXMessages.LocalizeFormatNoPrefix(Messages.ConsolidationHttpError, response.StatusCode));
			}
		}

		protected string GetInternalError(string errorContent)
		{
			string error = null;

			var branchLedgerError = Deserialize<BranchLedgerApiExceptionAPI>(errorContent);
			if (branchLedgerError.BranchCD?.error != null)
			{
				error = branchLedgerError.BranchCD?.error;
			}
			if (branchLedgerError.LedgerCD?.error != null)
			{
				error = (error == null) ? "" : error + " ";
				error += branchLedgerError.BranchCD?.error;
			}

			if (error != null)
			{
				return error;
			}

			error = Deserialize<CommonApiExceptionAPI>(errorContent).exceptionMessage;
			return error;
		}

		internal static string GetServerError(Exception ex)
		{
			var currentEx = ex;
			while (!(currentEx is ApiException) && currentEx.InnerException != null)
			{
				currentEx = currentEx.InnerException;
			}
			if (currentEx is ApiException)
			{
				return currentEx.Message;
			}
			else
			{
				return ex.Message;
			}
		}

		protected string Serialize(object obj)
		{
			return JsonConvert.SerializeObject(obj);
		}
		protected T Deserialize<T>(string body)
		{
			return JsonConvert.DeserializeObject<T>(body);
		}
	}
}
