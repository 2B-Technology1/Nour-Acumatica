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

using AutoMapper;
using PX.BankFeed.Common;
using PX.BankFeed.Plaid;
using PX.Common;
using PX.Data;
using PX.Objects.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PX.Objects.CA.BankFeed
{
	internal class PlaidBankFeedManager : BankFeedManager
	{
		private readonly IBankFeedClient _client;
		private readonly static (string, string)[] availableCorpCardFilters = new[] { (CABankFeedMatchField.Empty, CABankFeedMatchField.EmptyLabel),
			(CABankFeedMatchField.AccountOwner, Messages.AccountOwner),
			(CABankFeedMatchField.Name, Messages.Name)
		};

		private readonly static string[] availableTransactionFields = new[] {
			nameof(BankFeedTransaction.TransactionID),
			nameof(BankFeedTransaction.Date),
			nameof(BankFeedTransaction.Amount),
			nameof(BankFeedTransaction.IsoCurrencyCode),
			nameof(BankFeedTransaction.Name),
			nameof(BankFeedTransaction.Category),
			nameof(BankFeedTransaction.Pending),
			nameof(BankFeedTransaction.PendingTransactionID),
			nameof(BankFeedTransaction.Type),
			nameof(BankFeedTransaction.AccountID),
			nameof(BankFeedTransaction.AccountOwner)
		};

		private readonly static IMapper _mapper = new MapperConfiguration(c => c.AddProfile(typeof(PlaidMapperProfile))).CreateMapper();

		public override (string, string)[] AvailableCorpCardFilters => availableCorpCardFilters;

		public override string[] AvailableTransactionFields => availableTransactionFields;

		public PlaidBankFeedManager(IBankFeedClient client)
		{
			_client = client;
		}

		public async override Task ConnectAsync(CABankFeed bankFeed)
		{
			var organizationId = bankFeed.OrganizationID.Value;
			var response = await _client.GetConnectFormTokenAsync(organizationId, bankFeed.IsTestFeed.IsTrue());
			var plaidForm = new PlaidFormOptions();
			plaidForm.Token = response.Token;
			plaidForm.Mode = PlaidFormOptions.ConnectMode;
			throw new PXPluginRedirectException<PXPluginRedirectOptions>(plaidForm);
		}

		public async override Task UpdateAsync(CABankFeed bankFeed)
		{
			var request = new GetFormTokenRequest();
			var accessToken = bankFeed.AccessToken;
			CheckAccessToken(accessToken);
			request.AccessToken = accessToken;
			var organizationId = bankFeed.OrganizationID.Value;
			var response = await _client.GetUpdateFormTokenAsync(organizationId, request, bankFeed.IsTestFeed.IsTrue());

			var plaidForm = new PlaidFormOptions();
			plaidForm.Token = response.Token;
			plaidForm.Mode = PlaidFormOptions.UpdateMode;
			throw new PXPluginRedirectException<PXPluginRedirectOptions>(plaidForm);
		}

		public async override Task<IEnumerable<BankFeedCategory>> GetCategoriesAsync(CABankFeed bankFeed)
		{
			var organizationId = bankFeed.OrganizationID.Value;
			var response = await _client.GetCategoriesAsync(organizationId, bankFeed.IsTestFeed.IsTrue());
			return response.Categories.Select(i => new BankFeedCategory() { Category = string.Join(":", i.Hierarchy) }).ToList();
		}

		public async override Task ProcessConnectResponseAsync(string response, CABankFeed bankFeed)
		{
			var organizationId = bankFeed.OrganizationID.Value;
			var connectResponse = ParseResponse<ConnectResponse>(response);
			bool isSandbox = bankFeed.IsTestFeed.IsTrue();
			var accessTokenInfo = await ExchangeTokenAsync(connectResponse, organizationId, isSandbox);
			try
			{
				var bankFeedData = await GetBankFeedResponseObjectAsync(connectResponse, accessTokenInfo, organizationId, isSandbox);
				var graph = GetBankFeedGraph(bankFeed);
				if (bankFeed.Status == CABankFeedStatus.MigrationRequired)
				{
					graph.MigrateBankFeed(bankFeedData);
				}
				else
				{
					graph.StoreBankFeed(bankFeedData);
				}
			}
			catch
			{
				var req = new DeleteItemRequest();
				req.AccessToken = accessTokenInfo.AccessToken;
				await _client.DeleteItemAsync(organizationId, req, isSandbox);
				throw;
			}
		}

		public async override Task<IEnumerable<BankFeedTransaction>> GetTransactionsAsync(LoadTransactionsData input, CABankFeed bankFeed)
		{
			const int defaultTransPerRequestCnt = 250;
			try
			{
				var offset = 0;
				var totalTransCnt = 0;
				GetTransactionsCollectionResponse resp = null;
				var organizationId = bankFeed.OrganizationID.Value;
				var accessToken = bankFeed.AccessToken;
				var accountId = input.AccountID;
				CheckAccessToken(accessToken);
				var transLimit = input.TransactionsLimit;
				var transactions = new List<BankFeedTransaction>();
				var tranRequest = new GetTransactionsRequest();
				tranRequest.Options = new TransactionOptions() { Accounts = new string[] { input.AccountID } };
				tranRequest.EndDate = input.EndDate;
				tranRequest.StartDate = input.StartDate;
				tranRequest.AccessToken = accessToken;

				do
				{
					var count = defaultTransPerRequestCnt;
					if (transLimit != null)
					{
						if (totalTransCnt != 0 && totalTransCnt - offset < transLimit)
						{
							count = totalTransCnt - offset;
						}
						else if (transLimit < defaultTransPerRequestCnt)
						{
							count = transLimit.Value;
						}
					}
					tranRequest.Options.Count = count;
					tranRequest.Options.Offset = offset;
					resp = await _client.GetTranactionsAsync(organizationId, tranRequest, bankFeed.IsTestFeed.IsTrue());
					offset += resp.Transactions.Count;
					transactions.AddRange(_mapper.Map<List<BankFeedTransaction>>(resp.Transactions));
					totalTransCnt = resp.TotalTransactions;
				}
				while (resp != null && totalTransCnt > offset && (transLimit == null || transLimit > offset));

				int tranCount = transactions.Count;
				PXTrace.WriteInformation($"Number of fetched transactions for the {accountId} account from Plaid: {tranCount}.");

				if (input.TransactionsOrder == LoadTransactionsData.Order.Asc)
					return transactions.OrderBy(x => x.Date);
				else
					return transactions;
			}
			catch (BankFeedException ex) when (ex.Reason == BankFeedException.ExceptionReason.LoginFailed)
			{
				throw new BankFeedImportException(ex.Message, ex);
			}
		}

		public async override Task DeleteAsync(CABankFeed bankFeed)
		{
			var organizationId = bankFeed.OrganizationID.Value;
			var accessToken = bankFeed.AccessToken;
			CheckAccessToken(accessToken);
			var req = new DeleteItemRequest();
			req.AccessToken = accessToken;
			await _client.DeleteItemAsync(organizationId, req, bankFeed.IsTestFeed.IsTrue());
			var graph = GetBankFeedGraph(bankFeed);
			graph.DisconnectBankFeed();
		}

		public async override Task<IEnumerable<BankFeedAccount>> GetAccountsAsync(CABankFeed bankFeed)
		{
			var organizationId = bankFeed.OrganizationID.Value;
			var accessToken = bankFeed.AccessToken;
			CheckAccessToken(accessToken);
			var accounts = await _client.GetAccountsAsync(organizationId, new GetAccountsRequest() { AccessToken = accessToken }, bankFeed.IsTestFeed.IsTrue());
			var ret = new List<BankFeedAccount>();
			_mapper.Map(accounts.Accounts, ret);
			return ret;
		}

		public override Task ProcessUpdateResponseAsync(string responseStr, CABankFeed bankFeed)
		{
			var response = ParseResponse<UpdateResponse>(responseStr);
			if (!response.Updated)
			{
				throw new PXException(Messages.CredentialsWereNotUpdated, response.ErrorReason);
			}

			var graph = GetBankFeedGraph(bankFeed);
			graph.ClearLoginFailedStatus();
			return Task.CompletedTask;
		}

		public override LoadTransactionsData GetTransactionsFilterForTesting(DateTime loadingDate)
		{
			return new LoadTransactionsData() { StartDate = loadingDate, EndDate = loadingDate };
		}

		protected virtual async Task<ExchangeTokenResponse> ExchangeTokenAsync(ConnectResponse response, int organizationId, bool isSandbox)
		{
			var publicToken = response.PublicToken;
			if (string.IsNullOrEmpty(publicToken))
			{
				throw new PXException(Messages.IncorrectPublicToken);
			}

			var exchTokenReq = new ExchangeTokenRequest();
			exchTokenReq.PublicToken = publicToken;
			var exchTokenResp = await _client.ExchangeTokenAsync(organizationId, exchTokenReq, isSandbox);

			var accessToken = exchTokenResp?.AccessToken;
			if (string.IsNullOrEmpty(accessToken))
			{
				throw new PXException(Messages.IncorrectAccessToken);
			}
			return exchTokenResp;
		}
		protected virtual async Task<BankFeedFormResponse> GetBankFeedResponseObjectAsync(ConnectResponse response, ExchangeTokenResponse accessTokenInfo, int organizationId, bool isSandbox)
		{
			var metadata = response.Metadata;
			var accountIds = metadata.Accounts.Select(i => i.Id).ToList();

			var accReq = new GetAccountsRequest();
			accReq.Options = new AccountOptions() { Accounts = accountIds };
			accReq.AccessToken = accessTokenInfo.AccessToken;
			var accountsRes = await _client.GetAccountsAsync(organizationId, accReq, isSandbox);

			var ret = new BankFeedFormResponse() { Accounts = new List<BankFeedAccount>() };
			_mapper.Map(accessTokenInfo, ret);
			_mapper.Map(response, ret);
			_mapper.Map(accountsRes.Accounts, ret.Accounts);

			return ret;
		}

		private void CheckAccessToken(string accessToken)
		{
			if (accessToken == null)
			{
				throw new PXArgumentException(nameof(accessToken));
			}
		}
	}
}
