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
using PX.BankFeed.MX;
using PX.Common;
using PX.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace PX.Objects.CA.BankFeed
{
	internal class MXBankFeedManager : BankFeedManager
	{
		internal class ErrorStatusMessage
		{
			public string Message {get; private set;}
			public bool IncludeInstitution { get; private set; }

			public ErrorStatusMessage(string message, bool includeInstitution)
			{
				Message = message;
				IncludeInstitution = includeInstitution;
			}
		}

		internal class CategoryChain
		{
			public string Guid { get; set; }
			public string Category { get; set; }
		}

		const string widgetType = "connect_widget";
		private readonly static (string, string)[] availableCorpCardFilters = new[] {
			(CABankFeedMatchField.Empty, CABankFeedMatchField.EmptyLabel),
			(CABankFeedMatchField.PartnerAccountId, Messages.PartnerAccountId),
			(CABankFeedMatchField.CheckNumber, Messages.CheckNumber),
			(CABankFeedMatchField.Memo, Messages.Memo),
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
			nameof(BankFeedTransaction.Type),
			nameof(BankFeedTransaction.AccountID),
			nameof(BankFeedTransaction.CheckNumber),
			nameof(BankFeedTransaction.Memo),
			nameof(BankFeedTransaction.PartnerAccountId)
		};
		private readonly static IMapper _mapper = new MapperConfiguration(c => c.AddProfile(typeof(MXMapperProfile))).CreateMapper();
		private readonly static Dictionary<string, ErrorStatusMessage> errorStatusMessageDict = new Dictionary<string, ErrorStatusMessage>()
			{
				{ "PREVENTED", new ErrorStatusMessage(Messages.MXPreventedStatus, false) },
				{ "DENIED", new ErrorStatusMessage(Messages.MXDeniedStatus, false) },
				{ "CHALLENGED", new ErrorStatusMessage(Messages.MXChallengedStatus, true) },
				{ "REJECTED", new ErrorStatusMessage(Messages.MXRejectedStatus, false) },
				{ "LOCKED", new ErrorStatusMessage(Messages.MXLockedStatus, true) },
				{ "IMPEDED", new ErrorStatusMessage(Messages.MXImpededStatus, true) },
				{ "DEGRADED", new ErrorStatusMessage(Messages.MXDegradedStatus, false) },
				{ "DISCONNECTED", new ErrorStatusMessage(Messages.MXDisconnectedStatus, true) },
				{ "DISCONTINUED", new ErrorStatusMessage(Messages.MXDiscontinuedStatus, false) },
				{ "CLOSED", new ErrorStatusMessage(Messages.MXClosedStatus, false) },
				{ "DELAYED", new ErrorStatusMessage(Messages.MXDelayedStatus, false)},
				{ "FAILED", new ErrorStatusMessage(Messages.MXFailedStatus, true) },
				{ "DISABLED", new ErrorStatusMessage(Messages.MXDisabledStatus, false) },
				{ "IMPORTED", new ErrorStatusMessage(Messages.MXImportedStatus, true) },
				{ "EXPIRED", new ErrorStatusMessage(Messages.MXExpiredStatus, false) },
				{ "IMPAIRED", new ErrorStatusMessage(Messages.MXImpairedStatus, true) },
			};

		private readonly IBankFeedClient _client;
		private readonly BankFeedUserDataProvider _userDataProvider;

		public override (string, string)[] AvailableCorpCardFilters => availableCorpCardFilters;

		public override string[] AvailableTransactionFields => availableTransactionFields;

		public MXBankFeedManager(IBankFeedClient client, BankFeedUserDataProvider userDataProvider)
		{
			_client = client;
			_userDataProvider = userDataProvider;
		}

		public override async Task ConnectAsync(CABankFeed bankFeed)
		{
			var organizationId = bankFeed.OrganizationID.Value;
			var userGuid = bankFeed.ExternalUserID;

			if (userGuid == null)
			{
				var graph = GetBankFeedGraph(bankFeed);
				userGuid = _userDataProvider.GetUserForOrganization(organizationId);

				if (userGuid == null)
				{
					var userResponse = await _client.CreateUserAsync(organizationId);
					userGuid = userResponse.User.Guid;
					graph.CreateUserRecord(userGuid, organizationId);
				}

				graph.StoreBankFeedUser(bankFeed, userGuid);
			}

			var request = new GetFormTokenRequest() { WidgetUrl = new WidgetUrl() { WidgetType = widgetType } };
			var response = await _client.GetFormTokenAsync(organizationId, userGuid, request);
			var url = response.WidgetUrl.Url;

			var mxForm = new MXFormOptions();
			mxForm.Url = url;
			mxForm.Mode = MXFormOptions.ConnectMode;
			throw new PXPluginRedirectException<PXPluginRedirectOptions>(mxForm);
		}

		protected virtual async Task<BankFeedFormResponse> GetBankFeedResponseObjectAsync(int organizationId, ConnectResponse connectResponse)
		{
			var memberInfo = await _client.GetMemberAsync(organizationId, connectResponse.UserGuid, connectResponse.MemberGuid);
			var accounts = await GetAccountsAsync(organizationId, connectResponse.MemberGuid, connectResponse.UserGuid);
			var formResponse = new BankFeedFormResponse() { Accounts = accounts };

			_mapper.Map(connectResponse, formResponse);
			_mapper.Map(memberInfo, formResponse);

			return formResponse;
		}

		public async override Task ProcessConnectResponseAsync(string response, CABankFeed bankFeed)
		{
			var connectResponse = ParseResponse<ConnectResponse>(response);
			var organizationId = bankFeed.OrganizationID.Value;
			var graph = GetBankFeedGraph(bankFeed);

			var storedFeed = graph.GetStoredBankFeedByIds(connectResponse.UserGuid, connectResponse.MemberGuid);
			if (storedFeed != null)
			{
				throw new PXException(Messages.BankFeedWithSameCredAlreadyExists, storedFeed.Institution, storedFeed.BankFeedID);
			}

			try
			{
				var formResponse = await GetBankFeedResponseObjectAsync(organizationId, connectResponse);
				if (bankFeed.Status != CABankFeedStatus.MigrationRequired)
				{
					graph.StoreBankFeed(formResponse);
				}
				else
				{
					graph.MigrateBankFeed(formResponse);
				}
			}
			catch
			{
				await _client.DeleteMemberAsync(organizationId, connectResponse.UserGuid, connectResponse.MemberGuid);
				if (bankFeed.Status != CABankFeedStatus.MigrationRequired)
				{
					graph.DisconnectBankFeed();
				}
				throw;
			}
		}

		public async override Task DeleteAsync(CABankFeed bankFeed)
		{
			var organizationId = bankFeed.OrganizationID.Value;
			var extUserId = bankFeed.ExternalUserID;
			var extItemId = bankFeed.ExternalItemID;
			CheckExternalUserId(extUserId);
			CheckExternalItemId(extItemId);
			
			await _client.DeleteMemberAsync(organizationId, extUserId, extItemId);
			
			CABankFeedMaint graph = GetBankFeedGraph(bankFeed);
			graph.DisconnectBankFeed();
		}

		public async override Task<IEnumerable<BankFeedCategory>> GetCategoriesAsync(CABankFeed bankFeed)
		{
			int totalPages;
			var organizationId = bankFeed.OrganizationID.Value;
			var ret = new List<BankFeedCategory>();
			var filter = new Filter() { Page = 1, Count = 200 };
			var chainStack = new Stack<CategoryChain>();

			do
			{
				var response = await _client.GetCategoriesAsync(organizationId, filter);
				totalPages = response.Pagination.TotalPages;

				ret.AddRange(BoundCategories(response, chainStack));
				
				filter.Page++;
			}
			while (filter.Page <= totalPages);
			return ret;
		}

		public override Task<IEnumerable<BankFeedAccount>> GetAccountsAsync(CABankFeed bankFeed)
		{
			return GetAccountsAsync(bankFeed.OrganizationID.Value, bankFeed.ExternalItemID, bankFeed.ExternalUserID);
		}

		public async override Task<IEnumerable<BankFeedTransaction>> GetTransactionsAsync(LoadTransactionsData input, CABankFeed bankFeed)
		{
			int total;
			int totalTrans = 0;
			var accountId = input.AccountID;
			var filter = new TransactionsFilter();
			filter.StartDate = input.StartDate;
			filter.EndDate = input.EndDate;
			filter.AccountId = accountId;
			filter.Count = 250;
			filter.Page = 1;
			var transLimit = input.TransactionsLimit;
			var organizationId = bankFeed.OrganizationID.Value;
			var transactions = new List<BankFeedTransaction>();
			var extUserId = bankFeed.ExternalUserID;
			await CheckMemberStatus(bankFeed);

			do
			{
				var resp = await _client.GetTranactionsAsync(organizationId, extUserId, filter);
				var transCnt = resp.Transactions.Count;
				total = resp.Pagination.TotalPages;
				filter.Page++;

				if (transLimit != null && totalTrans + transCnt > transLimit)
				{
					var transLimitVal = transLimit.Value;
					var takeTrans = transCnt > transLimit ? transLimitVal : transLimitVal - transCnt;
					transactions.AddRange(_mapper.Map<List<BankFeedTransaction>>(resp.Transactions).Take(takeTrans));
				}
				else
				{
					transactions.AddRange(_mapper.Map<List<BankFeedTransaction>>(resp.Transactions));
				}
				totalTrans += transCnt;
			}
			while (filter.Page <= total && (transLimit == null || totalTrans < transLimit));

			var tranCount = transactions.Count;
			PXTrace.WriteInformation($"Number of fetched transactions for the {accountId} account from MX: {tranCount}.");

			if (input.TransactionsOrder == LoadTransactionsData.Order.Asc)
			{
				return transactions.OrderBy(x => x.Date);
			}
			else
			{
				return transactions;
			}
		}

		public async override Task UpdateAsync(CABankFeed bankFeed)
		{
			var organizationId = bankFeed.OrganizationID.Value;
			var extUserId = bankFeed.ExternalUserID;
			var status = await _client.GetMemberStatusAsync(organizationId, extUserId, bankFeed.ExternalItemID);
			if (status.Member.ConnectionStatus == "CONNECTED")
			{
				var accounts = await GetAccountsAsync(bankFeed);
				var graph = GetBankFeedGraph(bankFeed);
				graph.UpdateAccounts(accounts);
			}

			var request = new GetFormTokenRequest() { WidgetUrl = new WidgetUrl() {
				CurrentMemberGuid = bankFeed.ExternalItemID, WidgetType = widgetType, DisableInstitutionSearch = true }};
			var response = await _client.GetFormTokenAsync(organizationId, extUserId, request);

			var mxForm = new MXFormOptions();
			mxForm.Url = response.WidgetUrl.Url;
			mxForm.Mode = PlaidFormOptions.UpdateMode;
			throw new PXPluginRedirectException<PXPluginRedirectOptions>(mxForm);
		}

		public override async Task ProcessUpdateResponseAsync(string responseStr, CABankFeed bankFeed)
		{
			var response = ParseResponse<UpdateResponse>(responseStr);
			if (!response.Updated)
			{
				throw new PXException(Messages.CredentialsWereNotUpdated, response.ErrorReason);
			}

			var accounts = await GetAccountsAsync(bankFeed);
			var graph = GetBankFeedGraph(bankFeed);
			graph.UpdateAccounts(accounts);
			graph.ClearLoginFailedStatus();
		}

		public override LoadTransactionsData GetTransactionsFilterForTesting(DateTime loadingDate)
		{
			return new LoadTransactionsData() { StartDate = loadingDate, EndDate = loadingDate.AddDays(1) };
		}

		protected async virtual Task<IEnumerable<BankFeedAccount>> GetAccountsAsync(int organizationId, string itemId, string userId)
		{
			CheckExternalUserId(userId);
			CheckExternalItemId(itemId);

			var filter = new Filter() { Page = 1, Count = 200 };
			var ret = new List<BankFeedAccount>();
			int totalPages;
			do
			{
				var resp = await _client.GetAccountsAsync(organizationId, userId, filter);
				totalPages = resp.Pagination.TotalPages;
				ret.AddRange(_mapper.Map<IEnumerable<BankFeedAccount>>(resp.Accounts.Where(i => i.MemberGuid == itemId && !i.IsClosed)));
				filter.Page++;
			}
			while (filter.Page <= totalPages);
			return ret;
		}

		protected async virtual Task CheckMemberStatus(CABankFeed bankFeed)
		{
			var organizationId = bankFeed.OrganizationID.Value;
			var extUserId = bankFeed.ExternalUserID;
			CheckExternalUserId(extUserId);
			var result = await _client.GetMemberAsync(organizationId, extUserId, bankFeed.ExternalItemID);
			var memberInfo = result.Member;

			var updateDate = memberInfo.SuccessfullyAggregatedAt;
			if (updateDate != null)
			{
				var updateDateStr = updateDate.Value.ToString("u");
				PXTrace.WriteInformation($"Last successful update of transactions in MX: {updateDateStr}.");
			}

			if (memberInfo.ConnectionStatus != "CONNECTED")
			{
				PXException ex;
				if (errorStatusMessageDict.ContainsKey(memberInfo.ConnectionStatus))
				{
					var msgInfo = errorStatusMessageDict[memberInfo.ConnectionStatus];
					if (msgInfo.IncludeInstitution)
					{
						ex = new BankFeedImportException(PXMessages.LocalizeFormatNoPrefix(msgInfo.Message, memberInfo.Name),
							BankFeedImportException.ExceptionReason.LoginFailed);
					}
					else
					{
						ex = new BankFeedImportException(msgInfo.Message, BankFeedImportException.ExceptionReason.LoginFailed);
					}
				}
				else
				{
					ex = new BankFeedImportException(PXMessages.LocalizeFormatNoPrefix(Messages.MXIncorrectStatus, memberInfo.ConnectionStatus),
						BankFeedImportException.ExceptionReason.LoginFailed);
				}
				throw ex;
			}
		}

		private IEnumerable<BankFeedCategory> BoundCategories(GetCategoriesCollectionResponse response, Stack<CategoryChain> chainStack)
		{
			string category;
			foreach (var item in response.Categories)
			{
				if (item.ParentGuid == null)
				{
					category = item.Name;
					chainStack.Clear();
				}
				else
				{
					while (chainStack.Count() > 0 && chainStack.Peek().Guid != item.ParentGuid)
					{
						chainStack.Pop();
					}

					if(chainStack.Count() == 0) continue;

					category = chainStack.Peek().Category + ":" + item.Name;
				}

				var bfCategory = new BankFeedCategory() { Category = category };
				chainStack.Push(new CategoryChain() { Category = category, Guid = item.Guid });
				yield return bfCategory;
			}
		}

		private void CheckExternalUserId(string externalUserId)
		{
			if(externalUserId == null)
			{
				throw new PXArgumentException(nameof(externalUserId));
			}
		}

		private void CheckExternalItemId(string externalItemId)
		{
			if (externalItemId == null)
			{
				throw new PXArgumentException(nameof(externalItemId));
			}
		}
	}
}
