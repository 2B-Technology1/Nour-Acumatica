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

using PX.Api;
using PX.Common;
using PX.Data;
using PX.Objects.CA.BankFeed;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.EP.DAC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.CA
{
	public class CABankFeedMaint : PXGraph<CABankFeedMaint, CABankFeed>
	{
		[PXCacheName("TransactionsFilter")]
		public class TransactionsFilter : IBqlTable
		{
			public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
			[PXInt]
			[PXSelector(typeof(Search<CABankFeedDetail.lineNbr, Where<CABankFeedDetail.bankFeedID, Equal<Current<CABankFeed.bankFeedID>>>,
				OrderBy<Asc<CABankFeedDetail.lineNbr>>>), typeof(CABankFeedDetail.accountName), typeof(CABankFeedDetail.accountMask), typeof(CABankFeedDetail.accountID))]
			[PXUIField(DisplayName = "Account Name", Required = true)]
			public virtual int? LineNbr { get; set; }

			public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }
			[PXDate]
			[PXUIField(DisplayName = "Date From", Required = true)]
			public virtual DateTime? Date { get; set; }

			public abstract class dateTo : PX.Data.BQL.BqlDateTime.Field<dateTo> { }
			[PXDate]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "Date To", Required = true)]
			public virtual DateTime? DateTo { get; set; }

			public abstract class maxTransactions : PX.Data.BQL.BqlInt.Field<maxTransactions> { }
			[PXInt(MinValue = 1, MaxValue = 9999)]
			[PXDefault(30)]
			[PXUIField(DisplayName = "Max. Number of Recent Transactions", Required = true)]
			public virtual int? MaxTransactions { get; set; }
		}

		public PXFilter<TransactionsFilter> Filter;

		public PXSelect<CABankFeed> BankFeed;
		[PXFilterable]
		public PXSelect<CABankFeedDetail, Where<CABankFeedDetail.bankFeedID, Equal<Current<CABankFeed.bankFeedID>>>> BankFeedDetail;
		public PXSelect<CABankFeedCorpCard, Where<CABankFeedCorpCard.bankFeedID, Equal<Current<CABankFeed.bankFeedID>>>> BankFeedCorpCC;
		public PXSelect<CABankFeed, Where<CABankFeed.bankFeedID, Equal<Current<CABankFeed.bankFeedID>>>> CurrentBankFeed;
		public PXSelect<CABankFeedExpense, Where<CABankFeedExpense.bankFeedID, Equal<Current<CABankFeed.bankFeedID>>>> BankFeedExpense;
		public PXSelect<CashAccount> CashAccounts;
		public PXSelect<CABankFeedFieldMapping, Where<CABankFeedFieldMapping.bankFeedID, Equal<Current<CABankFeed.bankFeedID>>>> BankFeedFieldMapping;

		private (string, string)[] CorpCardFilters { get; set; }
		private string[] AvailableTransactionFields { get; set; }

		[InjectDependency]
		internal Func<string, BankFeedManager> BankFeedManagerProvider
		{
			get;
			set;
		}

		[PXVirtualDAC]
		public PXSelect<BankFeedCategory> BankFeedCategories;
		public IEnumerable bankFeedCategories()
		{
			foreach (var item in BankFeedCategories.Cache.Cached)
			{
				yield return item;
			}
		}

		[PXVirtualDAC]
		public PXSelectOrderBy<BankFeedTransaction, OrderBy<Desc<BankFeedTransaction.date>>> BankFeedTransactions;
		public IEnumerable bankFeedTransactions()
		{
			foreach (var item in BankFeedTransactions.Cache.Cached)
			{
				yield return item;
			}
		}

		public PXAction<CABankFeed> activateFeed;
		[PXUIField(DisplayName = "Activate", Visible = true)]
		[PXButton(CommitChanges = true, Category = ActionCategories.Processing)]
		public virtual IEnumerable ActivateFeed(PXAdapter adapter)
		{
			CABankFeed bankfeed = BankFeed.Current;
			if (bankfeed == null) return adapter.Get();

			foreach (CABankFeedDetail row in BankFeedDetail.Select())
			{
				if (row.CashAccountID == null) continue;

				CashAccount cashAccount = GetCashAccount(row.CashAccountID);
				CABankFeedDetail dupl = FindDuplicateCashAccountLink(row);
				if (dupl != null)
				{
					CABankFeed bankfeedWithDuplRow = GetBankFeed(dupl.BankFeedID);
					string nameMask = dupl.AccountName + ":" + dupl.AccountMask;
					var ex = new PXSetPropertyException(Messages.CashAccountAlreadyLinked, PXErrorLevel.Error, cashAccount.CashAccountCD.Trim(),
						nameMask, bankfeedWithDuplRow.BankFeedID);
					BankFeedDetail.Cache.RaiseExceptionHandling<CABankFeedDetail.cashAccountID>(row, cashAccount.CashAccountCD, ex);
					throw ex;
				}
			}
			bankfeed.Status = CABankFeedStatus.Active;
			BankFeed.Update(bankfeed);
			this.Save.Press();
			return adapter.Get();
		}

		public PXAction<CABankFeed> connectFeed;
		[PXUIField(DisplayName = "Connect", Visible = true)]
		[PXProcessButton(Category = ActionCategories.Processing)]
		public virtual IEnumerable ConnectFeed(PXAdapter adapter)
		{
			var bankFeed = BankFeed.Current;
			if (bankFeed == null) return adapter.Get();

			this.Save.Press();

			var manager = GetSpecificManager(bankFeed);
			var copy = CopyBankFeedRecord(bankFeed);
			var type = bankFeed.Type;
			PXLongOperation.StartOperation(this, () =>
			{
				manager.ConnectAsync(copy).GetAwaiter().GetResult();
			});
			return adapter.Get();
		}

		public PXAction<CABankFeed> updateFeed;
		[PXUIField(DisplayName = "Update Credentials", Visible = true)]
		[PXButton(CommitChanges = true, Category = ActionCategories.Processing, PopupCommand = nameof(syncUpdateFeed))]
		public virtual IEnumerable UpdateFeed(PXAdapter adapter)
		{
			var bankFeed = BankFeed.Current;
			if (bankFeed == null) return adapter.Get();

			this.Save.Press();

			var copy = CopyBankFeedRecord(bankFeed);
			var manager = GetSpecificManager(copy);
			var type = bankFeed.Type;

			PXLongOperation.StartOperation(this, () =>
			{
				manager.UpdateAsync(copy).GetAwaiter().GetResult();
			});
			return adapter.Get();
		}


		public PXAction<CABankFeed> syncUpdateFeed;
		[PXUIField(DisplayName = "", Visible = false)]
		[PXButton]
		public virtual IEnumerable SyncUpdateFeed(PXAdapter adapter)
		{
			var formProcRes = adapter.CommandArguments;
			var bankFeed = BankFeed.Current;
			if (string.IsNullOrEmpty(formProcRes) || bankFeed == null) return adapter.Get();

			var copy = CopyBankFeedRecord(bankFeed);
			var manager = GetSpecificManager(bankFeed);
			PXLongOperation.StartOperation(this, () =>
			{
				manager.ProcessUpdateResponseAsync(formProcRes, copy).GetAwaiter().GetResult();
			});

			return adapter.Get();
		}

		public PXAction<CABankFeed> syncConnectFeed;
		[PXUIField(DisplayName = "", Visible = false)]
		[PXButton]
		public virtual IEnumerable SyncConnectFeed(PXAdapter adapter)
		{
			var formProcRes = adapter.CommandArguments;
			var bankFeed = BankFeed.Current;
			if (string.IsNullOrEmpty(formProcRes) || bankFeed == null) return adapter.Get();

			var copy = CopyBankFeedRecord(bankFeed);
			var manager = GetSpecificManager(bankFeed);
			PXLongOperation.StartOperation(this, () =>
			{
				manager.ProcessConnectResponseAsync(formProcRes, copy).GetAwaiter().GetResult();
			});
			return adapter.Get();
		}

		public PXAction<CABankFeed> suspendFeed;
		[PXUIField(DisplayName = "Suspend", Visible = true)]
		[PXButton(CommitChanges = true, Category = ActionCategories.Processing)]
		public virtual IEnumerable SuspendFeed(PXAdapter adapter)
		{
			var bankFeed = BankFeed.Current;
			if (bankFeed == null) return adapter.Get();

			bankFeed.Status = CABankFeedStatus.Suspended;
			this.BankFeed.Update(bankFeed);
			this.Save.Press();
			return adapter.Get();
		}

		public PXAction<CABankFeed> disconnectFeed;
		[PXUIField(DisplayName = "Disconnect", Visible = true)]
		[PXButton(CommitChanges = true, Category = ActionCategories.Processing)]
		public virtual IEnumerable DisconnectFeed(PXAdapter adapter)
		{
			var bankFeed = BankFeed.Current;
			if (bankFeed == null) return adapter.Get();

			var label = CABankFeedType.ListAttribute.GetTypes.Where(ii => ii.Item1 == bankFeed.Type)
					.Select(ii => ii.Item2).FirstOrDefault();

			var confirmation = PXMessages.LocalizeFormatNoPrefix(Messages.DisconnectConfirmation, label);
			if (BankFeed.Ask(confirmation, MessageButtons.YesNo) == WebDialogResult.Yes)
			{
				bankFeed.CreateExpenseReceipt = false;
				bankFeed.CreateReceiptForPendingTran = false;
				bankFeed = BankFeed.Update(bankFeed);
				Save.Press();
				var copy = CopyBankFeedRecord(bankFeed);
				var manager = GetSpecificManager(bankFeed);
				PXLongOperation.StartOperation(this, () =>
				{
					manager.DeleteAsync(copy).GetAwaiter().GetResult();
				});

			}
			return adapter.Get();
		}

		public PXAction<CABankFeed> showCategories;
		[PXUIField(DisplayName = "View Categories")]
		[PXButton]
		public virtual IEnumerable ShowCategories(PXAdapter adapter)
		{
			var ret = adapter.Get();
			var bankFeed = BankFeed.Current;
			if (bankFeed == null) return ret;

			var cnt = BankFeedCategories.Cache.Cached.Count();
			if (cnt > 0)
			{
				BankFeedCategories.AskExt(true);
				return ret;
			}

			var copy = CopyBankFeedRecord(bankFeed);
			var manager = GetSpecificManager(bankFeed);
			PXLongOperation.StartOperation(this, () =>
			{
				var categories = manager.GetCategoriesAsync(copy).GetAwaiter().GetResult();
				PXLongOperation.SetCustomInfo(new BankFeedShowCategoriesCustomInfo(categories));
			});
			return ret;
		}

		public PXAction<CABankFeed> showAccessId;
		[PXUIField(DisplayName = "Show Access ID", Visible = true)]
		[PXButton(Category = ActionCategories.Processing)]
		public virtual void ShowAccessId()
		{
			CurrentBankFeed.AskExt(true);
		}

		public PXAction<CABankFeed> checkConnection;
		[PXUIField(DisplayName = "Load Transactions in Test Mode", Visible = true)]
		[PXButton(Category = ActionCategories.Processing)]
		public virtual void CheckConnection()
		{
			Filter.AskExt((graph, view) => {
				Filter.Cache.Clear();
				BankFeedTransactions.Cache.Clear();
			}, true);
		}

		public PXAction<CABankFeed> migrateFeed;
		[PXUIField(DisplayName = "Migrate", Visible = true)]
		[PXButton(Category = ActionCategories.Processing)]
		public IEnumerable MigrateFeed(PXAdapter adapter)
		{
			var ret = adapter.Get();
			var bankFeed = BankFeed.Current;
			if (bankFeed == null) return ret;

			this.Save.Press();

			if (string.IsNullOrEmpty(bankFeed.InstitutionID))
			{
				string displayName = PXUIFieldAttribute.GetDisplayName(BankFeed.Cache, nameof(CABankFeed.InstitutionID));
				throw new PXException(ErrorMessages.FieldIsEmpty, displayName);
			}

			var copy = CopyBankFeedRecord(bankFeed);
			var manager = GetSpecificManager(bankFeed);

			PXLongOperation.StartOperation(this, () =>
			{
				manager.ConnectAsync(copy).GetAwaiter().GetResult();
			});

			return ret;
		}

		public PXAction<CABankFeed> loadTransactions;
		[PXUIField(DisplayName = "Load Transactions", Visible = true)]
		[PXButton]
		public virtual IEnumerable LoadTransactions(PXAdapter adapter)
		{
			var ret = adapter.Get();
			var filter = Filter.Current;

			if (filter == null) return ret;
			if (!ValidateFields(filter)) return ret;

			var bankFeed = BankFeed.Current;
			var copy = CopyBankFeedRecord(bankFeed);
			var accountId = BankFeedDetail.Select().RowCast<CABankFeedDetail>().Where(i => i.LineNbr == filter.LineNbr)
				.Select(i => i.AccountID).FirstOrDefault();
			var manager = GetSpecificManager(bankFeed);
			var tranFilter = manager.GetTransactionsFilterForTesting(filter.DateTo.Value);
			tranFilter.StartDate = filter.Date.Value;
			tranFilter.AccountID = accountId;
			tranFilter.TransactionsLimit = filter.MaxTransactions;
			tranFilter.TransactionsOrder = LoadTransactionsData.Order.Desc;
			BankFeedTransactions.Cache.Clear();

			PXLongOperation.ClearStatus(this.UID);
			PXLongOperation.StartOperation(this, () =>
			{
				var trans = manager.GetTransactionsAsync(tranFilter, bankFeed).GetAwaiter().GetResult();
				PXLongOperation.SetCustomInfo(trans);
			});
			PXLongOperation.WaitCompletion(this.UID);

			var drLabel = PXMessages.LocalizeNoPrefix(Messages.CADebit);
			var crLabel = PXMessages.LocalizeNoPrefix(Messages.CACredit);
			var result = PXLongOperation.GetCustomInfo(this.UID) as IEnumerable<BankFeedTransaction>;
			if (result != null)
				PXLongOperation.ClearStatus(this.UID);
			int cnt = 0;
			foreach (BankFeedTransaction item in result)
			{
				item.Type = (item.Amount < 0) ? drLabel : crLabel;
				item.Amount = Math.Abs(item.Amount.Value);

				BankFeedTransactions.Cache.Hold(item);
				cnt++;
			}

			if (cnt == 0)
			{
				Filter.Cache.RaiseExceptionHandling<TransactionsFilter.date>(filter, filter.Date, new PXException(Messages.NoBankFeedTransForSelectedDate));
			}
			return ret;
		}

		public PXAction<CABankFeed> CloseBankFeed;
		[PXUIField(DisplayName = "")]
		[PXCancelCloseButton(CommitChanges = true)]
		public virtual void closeBankFeed()
		{
			this.Cancel.Press();
			CABankFeedImport graph = CreateInstance<CABankFeedImport>();
			throw new PXRedirectRequiredException(graph, false, string.Empty);
		}

		public PXAction<CABankFeed> setDefaultMapping;
		[PXButton]
		[PXUIField(DisplayName = "Set Default Mapping", Visible = true)]
		public virtual void SetDefaultMapping()
		{
			if (this.BankFeedFieldMapping.Any())
			{
				if (this.BankFeedFieldMapping.View.Ask(
					null,
					Messages.ReplaceRulesHeader,
					Messages.ReplaceRulesQuestion,
					MessageButtons.OKCancel,
					new Dictionary<WebDialogResult, string>
					{
						[WebDialogResult.OK] = Messages.Replace,
						[WebDialogResult.Cancel] = nameof(WebDialogResult.Cancel)
					},
					MessageIcon.None
					) != WebDialogResult.OK)
				{
					return;
				}
				this.BankFeedFieldMapping.Cache.DeleteAll(this.BankFeedFieldMapping.Select());
			}
			CreateDefaultMappingRules();
		}

		private void CreateDefaultMappingRules()
		{
			InsertBankFeedFieldMapping(CABankFeedMappingTarget.ExtRefNbr, "=ISNULL([Check Number], [Transaction ID])");
			if (this.CurrentBankFeed.Current.Type.IsIn(CABankFeedType.Plaid, CABankFeedType.TestPlaid))
			{
				InsertBankFeedFieldMapping(CABankFeedMappingTarget.CardNumber, "=TRIM(ISNULL([Account Owner], ''))");
			}
		}

		private void InsertBankFeedFieldMapping(string targetField, string sourceFieldOrValue)
		{
			CABankFeedFieldMapping cABankFeedFieldMapping = new CABankFeedFieldMapping()
			{
				BankFeedID = this.CurrentBankFeed.Current.BankFeedID,
				TargetField = targetField,
				SourceFieldOrValue = sourceFieldOrValue,
				Active = true
			};
			this.BankFeedFieldMapping.Insert(cABankFeedFieldMapping);
		}

		public CABankFeedMaint()
		{
			this.Actions.Move(nameof(Save), nameof(CloseBankFeed));
			var categoryCache = BankFeedCategories.Cache;
			categoryCache.AllowInsert = false;
			categoryCache.AllowUpdate = false;
			categoryCache.AllowDelete = false;
			var detailCache = BankFeedDetail.Cache;
			detailCache.AllowInsert = false;
			detailCache.AllowDelete = false;
			var tranCache = BankFeedTransactions.Cache;
			tranCache.AllowInsert = false;
			tranCache.AllowUpdate = false;
		}

		public virtual void StoreBankFeedUser(CABankFeed bankFeed, string externalUser)
		{
			var bFeed = BankFeed.Locate(bankFeed);
			bFeed.ExternalUserID = externalUser;
			BankFeed.Update(bFeed);
			this.Save.Press();
		}

		public virtual void CreateUserRecord(string externalUser, int organizationId)
		{
			var userCache = Caches[typeof(CABankFeedUser)];
			var record = new CABankFeedUser();
			record.ExternalUserID = externalUser;
			record.OrganizationID = organizationId;
			userCache.Insert(record);
			userCache.Persist(PXDBOperation.Insert);
		}

		public virtual void DisconnectBankFeed()
		{
			DisconnectBankFeed(false);
		}

		internal virtual void StoreBankFeed(BankFeedFormResponse formResponse)
		{
			CheckNewItem(formResponse);
			var bankFeed = BankFeed.Current;
			bankFeed.AccessToken = formResponse.AccessToken;
			bankFeed.ExternalItemID = formResponse.ItemID;
			bankFeed.Institution = formResponse.InstitutionName;
			bankFeed.InstitutionID = formResponse.InstitutionID;
			bankFeed.Status = CABankFeedStatus.SetupRequired;
			this.BankFeed.Update(bankFeed);
			AddAccounts(formResponse);
			this.Save.Press();
		}

		internal virtual void MigrateBankFeed(BankFeedFormResponse formResponse)
		{
			var bankFeed = BankFeed.Current;
			if (bankFeed == null) return;

			if (string.IsNullOrEmpty(bankFeed.InstitutionID))
			{
				string displayName = PXUIFieldAttribute.GetDisplayName(BankFeed.Cache, nameof(CABankFeed.InstitutionID));
				throw new PXException(ErrorMessages.FieldIsEmpty, displayName);
			}

			var instID = formResponse.InstitutionID;
			if (instID != bankFeed.InstitutionID)
			{
				throw new PXException(Messages.InstitutionIdDoesNotEqualToStoredValue, instID, bankFeed.InstitutionID);
			}

			var bankFeedCopy = (CABankFeed)this.BankFeed.Cache.CreateCopy(bankFeed);
			using (var scope = new PXTransactionScope())
			{
				var mappedCashAccounts = new Dictionary<int, (string newAccountId, string oldAccountId)>();
				var detailsFromCust = BankFeedDetail.Select().RowCast<CABankFeedDetail>().Where(i => i.CashAccountID != null && i.AccountName != null).ToList();
				var corpCardsFromCust = BankFeedCorpCC.Select().RowCast<CABankFeedCorpCard>().Where(i => detailsFromCust.Any(ii => ii.AccountID == i.AccountID)).ToList();

				DisconnectBankFeed(true);
				StoreBankFeed(formResponse);

				foreach (CABankFeedDetail det in BankFeedDetail.Select())
				{
					var matchByNameAndMask = detailsFromCust.Where(i => i.AccountName == det.AccountName && i.AccountMask == det.AccountMask).FirstOrDefault();
					if (matchByNameAndMask != null && !mappedCashAccounts.ContainsKey(matchByNameAndMask.CashAccountID.Value))
					{
						var detCopy = (CABankFeedDetail)this.BankFeedDetail.Cache.CreateCopy(det);
						var cashAccountId = matchByNameAndMask.CashAccountID.Value;
						var detDescr = matchByNameAndMask.Descr;
						var statementStartDay = matchByNameAndMask.StatementStartDay;
						mappedCashAccounts.Add(cashAccountId, (detCopy.AccountID, matchByNameAndMask.AccountID));
						detCopy.CashAccountID = cashAccountId;
						detCopy.Descr = detDescr;
						detCopy.StatementStartDay = statementStartDay;
						BankFeedDetail.Update(detCopy);

						CABankFeedDetail dupl = FindDuplicateCashAccountLink(detCopy);
						if (dupl != null)
						{
							CashAccount cashAccount = GetCashAccount(detCopy.CashAccountID);
							CABankFeed bankFeedWithDuplRow = GetBankFeed(dupl.BankFeedID);
							string nameMask = dupl.AccountName + ":" + dupl.AccountMask;
							throw new PXException(Messages.CashAccountAlreadyLinked, cashAccount.CashAccountCD.Trim(), nameMask, bankFeedWithDuplRow.BankFeedID);
						}

					}
				}

				if (mappedCashAccounts.Count() > 0)
				{
					this.Save.Press();
					var newDetails = BankFeedDetail.Select().RowCast<CABankFeedDetail>();
					foreach (var kvp in mappedCashAccounts)
					{
						foreach (var corpCard in corpCardsFromCust.Where(i => i.AccountID == kvp.Value.oldAccountId))
						{
							var newLine = this.BankFeedCorpCC.Insert();
							var bankFeedDetail = newDetails.Where(i => i.AccountID == kvp.Value.newAccountId).FirstOrDefault();
							newLine.AccountID = bankFeedDetail.AccountID;
							newLine = this.BankFeedCorpCC.Update(newLine);
							newLine.MatchField = corpCard.MatchField;
							newLine.MatchRule = corpCard.MatchRule;
							newLine.MatchValue = corpCard.MatchValue;
							newLine.CorpCardID = corpCard.CorpCardID;
							newLine.EmployeeID = corpCard.EmployeeID;
							newLine = this.BankFeedCorpCC.Update(newLine);
						}
					}
					this.Save.Press();
				}
				scope.Complete();
			}
		 	CustomizationFeedHelper.DisconnectFeed(bankFeedCopy);
		}

		internal virtual void UpdateAccounts(IEnumerable<BankFeedAccount> accounts)
		{
			bool needPersist = false;
			var storedAccounts = BankFeedDetail.Select().RowCast<CABankFeedDetail>();
			foreach (BankFeedAccount account in accounts)
			{
				if (storedAccounts.Any(x => x.AccountID == account.AccountID)) continue;

				var sameAccount = storedAccounts.FirstOrDefault(i => i.AccountName == account.Name && i.AccountMask == account.Mask);

				if (sameAccount != null)
				{
					sameAccount.AccountID = account.AccountID;
					BankFeedDetail.Update(sameAccount);
					needPersist = true;
					continue;
				}

				var detail = BankFeedDetail.Insert();
				detail.AccountID = account.AccountID;
				detail.AccountName = account.Name;
				detail.AccountMask = account.Mask;
				detail.AccountType = account.Type;
				detail.AccountSubType = account.Subtype;
				detail.Currency = account.Currency;
				this.BankFeedDetail.Update(detail);
				needPersist = true;
			}

			if (needPersist)
			{
				var bankFeed = this.BankFeed.Current;
				bankFeed.Status = CABankFeedStatus.SetupRequired;
				this.Save.Press();
			}
		}

		internal virtual void AddAccounts(BankFeedFormResponse bankFeedResponse)
		{
			var accounts = bankFeedResponse.Accounts;
			if (accounts.Count() > 0)
			{
				var currentAccounts = new List<CABankFeedDetail>();
				foreach (CABankFeedDetail detail in BankFeedDetail.Select())
				{
					currentAccounts.Add(detail);
				}
				foreach (var account in accounts)
				{
					var detail = currentAccounts.FirstOrDefault(x => x.AccountID == account.AccountID);
					if (detail == null) detail = BankFeedDetail.Insert();
					detail.AccountID = account.AccountID;
					detail.AccountName = account.Name;
					detail.AccountMask = account.Mask;
					detail.AccountType = account.Type;
					detail.AccountSubType = account.Subtype;
					detail.Currency = account.Currency;
					this.BankFeedDetail.Update(detail);
				}
			}
		}

		internal virtual CABankFeed GetStoredBankFeedByIds(string externalUserId, string externalItemId)
		{
			return PXSelect<CABankFeed, Where<CABankFeed.externalUserID, Equal<Required<CABankFeed.externalUserID>>,
				And<CABankFeed.externalItemID, Equal<Required<CABankFeed.externalItemID>>>>>
				.SelectSingleBound(this, null, externalUserId, externalItemId);
		}

		internal virtual void CheckNewItem(BankFeedFormResponse formResponse)
		{
			CABankFeedDetail duplDet = null;
			CABankFeed duplBankFeed = null;

			var data = GetBankFeedDetailsByInstitution(formResponse.InstitutionID);

			foreach (Tuple<CABankFeedDetail, CABankFeed> item in data)
			{
				CABankFeedDetail bankFeedDet = item.Item1;
				CABankFeed bankFeedItem = item.Item2;
				var name = bankFeedDet.AccountName;
				var mask = bankFeedDet.AccountMask;
				if (name != null)
				{
					var row = formResponse.Accounts.FirstOrDefault(i => i.Name == name && i.Mask == mask);
					if (row != null)
					{
						duplDet = bankFeedDet;
						duplBankFeed = bankFeedItem;
						break;
					}
				}
			}

			if (duplDet != null)
			{
				throw new PXException(Messages.AccountAlreadyLinked, duplDet.AccountName, duplDet.AccountMask, duplBankFeed.BankFeedID);
			}
		}

		internal void ClearLoginFailedStatus()
		{
			var bankFeed = BankFeed.Current;
			if (bankFeed != null && bankFeed.RetrievalStatus == CABankFeedRetrievalStatus.LoginFailed)
			{
				var dt = PXTimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, LocaleInfo.GetTimeZone());

				bankFeed.RetrievalStatus = CABankFeedRetrievalStatus.Success;
				bankFeed.ErrorMessage = null;
				bankFeed.RetrievalDate = dt;
				BankFeed.Update(bankFeed);

				var details = BankFeedDetail.Select().RowCast<CABankFeedDetail>().Where(i => i.RetrievalStatus == CABankFeedRetrievalStatus.LoginFailed);
				foreach (var detail in details)
				{
					detail.RetrievalStatus = CABankFeedRetrievalStatus.Success;
					detail.ErrorMessage = null;
					bankFeed.RetrievalDate = dt;
					BankFeedDetail.Update(detail);
				}
				this.Save.Press();
			}
		}

		protected virtual void _(Events.RowSelected<CABankFeedCorpCard> e)
		{
			CABankFeedCorpCard corpCard = e.Row;
			PXCache cache = e.Cache;
			CABankFeed bankfeed = BankFeed.Current;
			if (corpCard == null) return;

			bool matchFieldExists = corpCard.MatchField != "N";
			PXUIFieldAttribute.SetEnabled<CABankFeedCorpCard.matchValue>(e.Cache, corpCard, matchFieldExists);
			PXUIFieldAttribute.SetEnabled<CABankFeedCorpCard.matchRule>(e.Cache, corpCard, matchFieldExists);
			PXSetPropertyException ex = null;

			if (corpCard.AccountID != null && corpCard.CorpCardID != null)
			{
				var detail = BankFeedDetail.Select().RowCast<CABankFeedDetail>()
					.Where(i => i.AccountID == corpCard.AccountID).FirstOrDefault();

				if (detail != null)
				{
					var accountMask = detail.AccountMask;
					var result = CABankMatchingProcess.IsCardNumberMatch(accountMask, corpCard.CardNumber);

					if (!result)
					{
						ex = new PXSetPropertyException(Messages.IncorrectCardForMatching, PXErrorLevel.Warning,
							corpCard.CardNumber, detail.AccountName, accountMask);
					}
				}
			}
			cache.RaiseExceptionHandling<CABankFeedCorpCard.cardNumber>(corpCard, corpCard.CardNumber, ex);

			if (bankfeed != null)
			{
				var filters = GetCorpCardFilters(bankfeed);
				PXStringListAttribute.SetList<CABankFeedCorpCard.matchField>(cache, corpCard, filters);
			}
		}

		protected virtual void _(Events.RowSelected<CABankFeed> e)
		{
			CABankFeed bankFeed = e.Row;
			PXCache cache = e.Cache;
			if (bankFeed == null) return;

			bool isDisconnected = bankFeed.Status == CABankFeedStatus.Disconnected;
			bool isActive = bankFeed.Status == CABankFeedStatus.Active;
			bool isSuspended = bankFeed.Status == CABankFeedStatus.Suspended;
			bool isSetupReq = bankFeed.Status == CABankFeedStatus.SetupRequired;
			bool isMigrationReq = bankFeed.Status == CABankFeedStatus.MigrationRequired;
			bool allowDisconnect = isSuspended || isActive || isSetupReq;

			cache.AllowDelete = isDisconnected || isMigrationReq;
			SetEnabledRequired(e.Cache, bankFeed);
			this.connectFeed.SetEnabled(isDisconnected);
			this.disconnectFeed.SetEnabled(allowDisconnect);
			this.showCategories.SetEnabled(allowDisconnect);
			this.updateFeed.SetEnabled(!isSuspended && !isDisconnected && !isMigrationReq);
			this.connectFeed.SetIsLockedOnToolbar(isDisconnected);
			this.updateFeed.SetIsLockedOnToolbar(!isSuspended && !isDisconnected && !isMigrationReq);
			this.activateFeed.SetIsLockedOnToolbar(isSuspended);
			this.suspendFeed.SetEnabled(isActive);
			this.checkConnection.SetEnabled(!isDisconnected && !isMigrationReq);
			this.showAccessId.SetEnabled(!isDisconnected && !isMigrationReq);
			this.migrateFeed.SetEnabled(isMigrationReq);
			this.migrateFeed.SetVisible(isMigrationReq);
			this.migrateFeed.SetIsLockedOnToolbar(isMigrationReq);
			bool enableActivateFeed = isSuspended
				|| (isSetupReq && this.BankFeedDetail.Select().RowCast<CABankFeedDetail>().Any(i => i.CashAccountID != null));
			this.activateFeed.SetEnabled(enableActivateFeed);
			PXUIFieldAttribute.SetEnabled<CABankFeed.defaultExpenseItemID>(cache, bankFeed);
			PXUIFieldAttribute.SetEnabled<CABankFeed.type>(cache, bankFeed, isDisconnected);
			PXUIFieldAttribute.SetVisible<CABankFeed.createExpenseReceipt>(cache, bankFeed, !isDisconnected);
			PXUIFieldAttribute.SetVisible<CABankFeed.createReceiptForPendingTran>(cache, bankFeed, !isDisconnected);
			PXUIFieldAttribute.SetVisible<CABankFeed.externalUserID>(cache, bankFeed, !string.IsNullOrEmpty(bankFeed.ExternalUserID));
		}

		protected virtual void _(Events.RowSelected<CABankFeedDetail> e)
		{
			PXCache cache = e.Cache;
			CABankFeedDetail detail = e.Row;
			if (detail == null) return;

			SetCashAccountWarningIfNeeded(cache, detail);
			PXUIFieldAttribute.SetEnabled<CABankFeedDetail.hidden>(cache, detail, detail.CashAccountID == null);
		}

		protected virtual void _(Events.RowSelected<TransactionsFilter> e)
		{
			PXCache transCache = BankFeedTransactions.Cache;
			TransactionsFilter filter = e.Row;
			CABankFeed bankFeed = BankFeed.Current;
			if (filter == null || bankFeed?.BankFeedID == null) return;

			var fields = GetAvailableTransactionFields(bankFeed);
			if (fields != null)
			{
				foreach (var field in fields)
				{
					PXUIFieldAttribute.SetVisible(transCache, null, field, true);
				}
			}

			var showWarning = filter.MaxTransactions > 1000 ? true : false;
			Filter.Cache.RaiseExceptionHandling<TransactionsFilter.maxTransactions>(filter, filter.MaxTransactions,
				showWarning ? new PXSetPropertyException(Messages.MaxTranNumberWarning, PXErrorLevel.Warning) : null);
		}

		protected virtual void _(Events.RowPersisting<CABankFeed> e)
		{
			var bankFeed = e.Row;
			if (bankFeed.CreateExpenseReceipt == true && bankFeed.DefaultExpenseItemID == null)
			{
				throw new PXRowPersistingException(PXDataUtils.FieldName<CABankFeed.defaultExpenseItemID>(), e.Row.DefaultExpenseItemID,
					Messages.DefaultExpenseRequired);
			}

			if (bankFeed.Status == CABankFeedStatus.Active ||
				bankFeed.Status == CABankFeedStatus.SetupRequired && bankFeed.AccountQty != bankFeed.UnmatchedAccountQty)
			{
				foreach (CABankFeedDetail det in BankFeedDetail.Select())
				{
					var cashAccountId = det.CashAccountID;
					if (cashAccountId != null)
					{
						var process = CAMatchProcess.PK.Find(this, cashAccountId);
						if (process != null)
						{
							throw new PXRowPersistingException(PXDataUtils.FieldName<CABankFeedDetail.cashAccountID>(), cashAccountId,
								Messages.TransactionsAreBeingRetrieved, bankFeed.BankFeedID);
						}
					}
				}
			}
		}

		protected virtual void _(Events.RowPersisting<CABankFeedDetail> e)
		{
			CABankFeedDetail row = e.Row;
			CABankFeed bankFeed = BankFeed.Current;
			if (e.Operation != PXDBOperation.Delete && bankFeed.Status == CABankFeedStatus.Active)
			{
				CABankFeedDetail dupl =  FindDuplicateCashAccountLink(row);
				if (dupl != null)
				{
					CashAccount cashAccount = GetCashAccount(row.CashAccountID);
					CABankFeed bankFeedWithDuplRow = GetBankFeed(dupl.BankFeedID);
					string nameMask = dupl.AccountName + ":" + dupl.AccountMask;
					throw new PXRowPersistingException(nameof(CABankFeedDetail.CashAccountID), cashAccount.CashAccountCD, Messages.CashAccountAlreadyLinked,
						cashAccount.CashAccountCD.Trim(), nameMask, bankFeedWithDuplRow.BankFeedID);
				}
			}
		}

		protected virtual void _(Events.RowPersisting<CABankFeedExpense> e)
		{
			CABankFeedExpense row = e.Row;
			PXCache cache = e.Cache;
			if (e.Operation != PXDBOperation.Delete)
			{
				PXDefaultAttribute.SetPersistingCheck<CABankFeedExpense.inventoryItemID>(cache, row, row.DoNotCreate == true
					? PXPersistingCheck.Nothing : PXPersistingCheck.Null);
			}
		}

		protected virtual void _(Events.RowPersisting<CABankFeedCorpCard> e)
		{
			CABankFeedCorpCard row = e.Row;
			PXCache cache = e.Cache;
			if (e.Operation != PXDBOperation.Delete)
			{
				bool valueReq = row.MatchField != "N" ? true : false;
				PXDefaultAttribute.SetPersistingCheck<CABankFeedCorpCard.matchValue>(cache, row, valueReq ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

				if (row.MatchRule == "N" && valueReq)
				{
					string displayName = PXUIFieldAttribute.GetDisplayName(cache, nameof(CABankFeedCorpCard.MatchRule));
					throw new PXRowPersistingException(nameof(CABankFeedCorpCard.MatchRule), row.MatchValue, PX.Data.ErrorMessages.FieldIsEmpty, displayName);
				}
			}
		}

		public override void Persist()
		{
			var bankFeed = BankFeed.Current;
			if(bankFeed != null && (bankFeed.Status == CABankFeedStatus.Active
				|| bankFeed.Status == CABankFeedStatus.SetupRequired))
			{
				foreach (CABankFeedDetail row in BankFeedDetail.Select())
				{
					if (row.CashAccountID != null && BankFeedDetail.Cache.GetStatus(row) != PXEntryStatus.Deleted)
					{
						CashAccount cashAccount = GetCashAccount(row.CashAccountID);
						UpdateCashAccountEntry(cashAccount);
					}
				}
			}

			base.Persist();
		}

		protected virtual void _(Events.RowUpdated<CABankFeedCorpCard> e)
		{
			CABankFeedCorpCard corpCard = e.Row;
			CABankFeedCorpCard oldCorpCard = e.OldRow;
			if (corpCard == null || oldCorpCard == null) return;

			if (corpCard.AccountID != oldCorpCard.AccountID)
			{
				BankFeedCorpCC.View.RequestRefresh();
			}
		}

		protected virtual void _(Events.RowDeleting<CABankFeed> e)
		{
			CABankFeed bankFeed = e.Row;
			if (bankFeed == null) return;
			if (bankFeed.Status != CABankFeedStatus.Disconnected && bankFeed.Status != CABankFeedStatus.MigrationRequired)
			{
				var label = CABankFeedStatus.ListAttribute.GetStatuses.Where(ii => ii.Item1 == bankFeed.Status)
					.Select(ii => ii.Item2).FirstOrDefault();
				throw new PXException(Messages.BankFeedStatus, bankFeed.BankFeedID, label);
			}
		}

		protected virtual void _(Events.FieldSelecting<CABankFeedDetail, CABankFeedDetail.importStartDate> e)
		{
			if (e?.Row == null) return;
			CABankFeedDetail detail = e.Row;
			CABankFeed bankFeed = BankFeed.Current;
			if (detail.CashAccountID != null && bankFeed != null && detail.ImportStartDate == null)
			{
				e.ReturnValue = GetImportStartDate(this, bankFeed, detail);
			}
		}

		protected virtual void _(Events.FieldSelecting<CABankFeedCorpCard, CABankFeedCorpCard.accountID> e)
		{
			if (e?.Row == null) return;
			CABankFeedCorpCard corpCard = e.Row;
			
			if (corpCard?.AccountID != null)
			{
				var detailRow = BankFeedDetail.Select().RowCast<CABankFeedDetail>()
					.FirstOrDefault(i => i.AccountID == corpCard.AccountID);

				if (detailRow != null)
				{
					e.ReturnValue = detailRow.AccountName;
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<CABankFeed.importStartDate> e)
		{
			if (BankFeed.Current?.ImportStartDate != null)
			{
				foreach (CABankFeedDetail det in BankFeedDetail.Select())
				{
					if (det.CashAccountID != null)
					{
						det.ImportStartDate = GetImportStartDate(this, BankFeed.Current, det);
					}
				}
			}
		}

		protected virtual void _(Events.FieldSelecting<TransactionsFilter, TransactionsFilter.lineNbr> e)
		{
			if (e?.Row == null) return;
			TransactionsFilter tranFilter = e.Row;

			if (tranFilter?.LineNbr != null)
			{
				var detailRow = BankFeedDetail.Select().RowCast<CABankFeedDetail>()
					.FirstOrDefault(i => i.LineNbr == tranFilter.LineNbr);

				if (detailRow != null)
				{
					e.ReturnValue = detailRow.AccountName;
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<CABankFeed.type> e)
		{
			CABankFeed bankFeed = e.Row as CABankFeed;
			if (bankFeed == null) return;

			CorpCardFilters = null;
		}

		protected virtual void _(Events.FieldUpdated<CABankFeedCorpCard.matchField> e)
		{
			CABankFeedCorpCard corpCard = e.Row as CABankFeedCorpCard;
			PXCache cache = e.Cache;
			if (corpCard == null) return;

			string newVal = (string)e.NewValue;
			if (newVal == "N")
			{
				cache.SetValueExt<CABankFeedCorpCard.matchRule>(corpCard, "N");
				cache.SetValueExt<CABankFeedCorpCard.matchValue>(corpCard, null);
			}
		}

		protected virtual void _(Events.FieldUpdated<CABankFeedCorpCard.accountID> e)
		{
			CABankFeedCorpCard corpCard = e.Row as CABankFeedCorpCard;
			PXCache cache = e.Cache;
			if (corpCard == null) return;
			int? newVal = null;

			var bankFeedDet = BankFeedDetail.Select().RowCast<CABankFeedDetail>()
				.Where(i => i.AccountID == corpCard.AccountID && i.BankFeedID == corpCard.BankFeedID).FirstOrDefault();
			if (bankFeedDet != null)
			{
				newVal = bankFeedDet.CashAccountID;
			}
			cache.SetValueExt<CABankFeedCorpCard.cashAccountID>(corpCard, newVal);
		}

		protected virtual void _(Events.FieldUpdated<CABankFeedDetail.cashAccountID> e)
		{
			CABankFeed bankFeed = BankFeed.Current;
			CABankFeedDetail row = e.Row as CABankFeedDetail;
			PXCache cache = e.Cache;
			int? cashAccountId = (int?)e.NewValue;
			if (cashAccountId != null)
			{
				DateTime importStartDate = GetImportStartDate(this, BankFeed.Current, row);
				cache.SetValue<CABankFeedDetail.importStartDate>(row, importStartDate);

				if (bankFeed.Status == CABankFeedStatus.SetupRequired)
				{
					BankFeed.Cache.SetValue<CABankFeed.status>(bankFeed, CABankFeedStatus.Active);
				}

				cache.SetValue<CABankFeedDetail.hidden>(row, false);
			}
			else
			{
				cache.SetValue<CABankFeedDetail.importStartDate>(row, null);

				if (bankFeed.Status != CABankFeedStatus.MigrationRequired
					&& BankFeedDetail.Select().RowCast<CABankFeedDetail>().All(i => i.CashAccountID == null))
				{
					BankFeed.Cache.SetValue<CABankFeed.status>(bankFeed, CABankFeedStatus.SetupRequired);
				}
			}

			foreach (CABankFeedCorpCard item in BankFeedCorpCC.Select())
			{
				if (item.AccountID == row.AccountID)
				{
					if (cashAccountId == null)
					{
						BankFeedCorpCC.Delete(item);
					}
					else
					{
						item.CashAccountID = cashAccountId;
						item.CorpCardID = null;
						BankFeedCorpCC.Update(item);
					}
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<CABankFeedDetail.statementPeriod> e)
		{
			CABankFeedDetail row = e.Row as CABankFeedDetail;
			if (row == null) return;
			const int mondayIndex = 2;
			const int firstDayIndex = 1; 
			string newVal = (string)e.NewValue;
			string oldVal = (string)e.OldValue;
			if (oldVal != newVal)
			{
				int valIndex = newVal == CABankFeedStatementPeriod.Week ? mondayIndex : firstDayIndex; 
				e.Cache.SetValueExt<CABankFeedDetail.statementStartDay>(row, valIndex);
			}
		}

		protected virtual void _(Events.FieldUpdated<CABankFeedCorpCard.corpCardID> e)
		{
			CABankFeedCorpCard row = e.Row as CABankFeedCorpCard;
			if (e.Row == null) return;
		
			foreach (EPEmployeeCorpCardLink link in PXSelect<EPEmployeeCorpCardLink,
											Where<EPEmployeeCorpCardLink.corpCardID, Equal<Required<EPEmployeeCorpCardLink.corpCardID>>>>
											.Select(this, row.CorpCardID))
			{
				e.Cache.SetValueExt<CABankFeedCorpCard.employeeID>(e.Row, link.EmployeeID);
				break;
			}
		}

		protected virtual void _(Events.FieldVerifying<CABankFeedDetail.statementStartDay> e)
		{
			CABankFeedDetail bfDetail = (CABankFeedDetail)e.Row;
			if (bfDetail != null && !e.Cancel)
			{
				int? newValue = (int?)e.NewValue;
				if (newValue == null || newValue <= 0 || newValue > 31)
				{
					throw new PXSetPropertyException<CABankFeedDetail.statementStartDay>(Messages.IncorrectStartStatementDay, PXErrorLevel.Error);
				}
			}
		}

		protected virtual void _(Events.FieldDefaulting<TransactionsFilter.date> e)
		{
			var filter = e.Row as TransactionsFilter;
			if (filter == null) return;
			var startDate = PXTimeZoneInfo.Now.Subtract(new TimeSpan(7, 0, 0, 0)).Date;
			e.NewValue = startDate;
		}

		public static DateTime GetImportStartDate(PXGraph graph, CABankFeed bankFeed, CABankFeedDetail bankFeedDetail)
		{
			DateTime ret = DateTime.MinValue;
			CABankTran targetTran = null;

			var res = PXSelectGroupBy<CABankTran, Where<CABankTran.cashAccountID, Equal<Required<CABankTran.cashAccountID>>,
				And<CABankTran.tranDate, GreaterEqual<Required<CABankTran.tranDate>>>>,
				Aggregate<GroupBy<CABankTranFeedSource.source, Max<CABankTran.tranDate>>>>
				.Select(graph, bankFeedDetail.CashAccountID, bankFeed.ImportStartDate)
				.RowCast<CABankTran>();

			var maxDate = res.Max(i => i.TranDate);

			if (maxDate != null)
			{
				var itemsMaxDate = res.Where(i => i.TranDate == maxDate);
				foreach (var item in itemsMaxDate)
				{
					var cacheExt = PXCache<CABankTran>.GetExtension<CABankTranFeedSource>(item);
					if (cacheExt.Source == null)
					{
						targetTran = item;
						break;
					}
					targetTran = item;
				}
			}

			if (targetTran != null)
			{
				var cacheExt = PXCache<CABankTran>.GetExtension<CABankTranFeedSource>(targetTran);
				if (cacheExt.Source != bankFeed.Type)
				{
					ret = targetTran.TranDate.Value.AddDays(1);
				}
				else
				{
					ret = targetTran.TranDate.Value;
				}
			}

			if (targetTran == null || bankFeed.ImportStartDate > ret)
			{
				ret = bankFeed.ImportStartDate.Value;
			}

			return ret;
		}

		protected virtual void DeleteAccounts()
		{
			foreach (CABankFeedDetail detail in BankFeedDetail.Select())
			{
				BankFeedDetail.Delete(detail);
			}
			foreach (CABankFeedCorpCard corpCard in BankFeedCorpCC.Select())
			{
				BankFeedCorpCC.Delete(corpCard);
			}
		}

		protected virtual CABankFeedDetail FindDuplicateCashAccountLink(CABankFeedDetail row)
		{
			if (row.CashAccountID == null) return null;

			foreach (CABankFeedDetail cachedItem in this.Caches[typeof(CABankFeedDetail)].Cached)
			{
				if (cachedItem.LineNbr != row.LineNbr && cachedItem.CashAccountID == row.CashAccountID
					&& cachedItem.BankFeedID == row.BankFeedID)
				{
					var status = this.BankFeedDetail.Cache.GetStatus(cachedItem);
					if (status != PXEntryStatus.Deleted && status != PXEntryStatus.InsertedDeleted)
					{
						return cachedItem;
					}
				}
			}

			foreach (PXResult<CABankFeed, CABankFeedDetail> item in PXSelectJoin<CABankFeed,
				InnerJoin<CABankFeedDetail, On<CABankFeedDetail.bankFeedID, Equal<CABankFeed.bankFeedID>>>,
					Where<CABankFeedDetail.cashAccountID, Equal<Required<CABankFeedDetail.cashAccountID>>,
						And<CABankFeed.status, In3<CABankFeedStatus.active, CABankFeedStatus.setupRequired>>>>.Select(this, row.CashAccountID))
			{
				CABankFeedDetail detail = item;
				if (row.BankFeedID != detail.BankFeedID)
				{
					return item;
				}
			}
			return null;
		}

		protected virtual void UpdateCashAccountEntry(CashAccount cashAccount)
		{
			if (cashAccount?.StatementImportTypeName != null)
			{
				cashAccount.StatementImportTypeName = null;
				CashAccounts.Update(cashAccount);
			}
		}

		protected virtual CashAccount GetCashAccount(int? cashAccountId)
		{
			return PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>
				.SelectSingleBound(this, null, cashAccountId);
		}

		protected virtual CABankFeed GetBankFeed(string bankFeedId)
		{
			return PXSelect<CABankFeed, Where<CABankFeed.bankFeedID, Equal<Required<CABankFeed.bankFeedID>>>>
				.SelectSingleBound(this, null, bankFeedId);
		}

		protected virtual IEnumerable<Tuple<CABankFeedDetail, CABankFeed>> GetBankFeedDetailsByInstitution(string instId)
		{
			foreach (PXResult<CABankFeedDetail, CABankFeed> item in PXSelectReadonly2<CABankFeedDetail,
				InnerJoin<CABankFeed, On<CABankFeed.bankFeedID, Equal<CABankFeedDetail.bankFeedID>>>,
				Where<CABankFeed.institutionID, Equal<Required<CABankFeed.institutionID>>>>.Select(this, instId))
			{
				yield return new Tuple<CABankFeedDetail, CABankFeed>(item, item);
			}
		}

		private void DisconnectBankFeed(bool keepUserId)
		{
			var bankFeed = BankFeed.Current;
			bankFeed.AccessToken = null;
			bankFeed.Institution = null;
			bankFeed.InstitutionID = null;
			bankFeed.ExternalItemID = null;
			bankFeed.Status = CABankFeedStatus.Disconnected;
			if (keepUserId == false)
			{
				bankFeed.ExternalUserID = null;
			}
			this.BankFeed.Update(bankFeed);
			DeleteAccounts();
			this.Save.Press();
		}

		private bool ValidateFields(TransactionsFilter filter)
		{
			if (filter.Date == null)
			{
				Filter.Cache.RaiseExceptionHandling<TransactionsFilter.date>(filter, filter.Date,
					new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<TransactionsFilter.date>(Filter.Cache)));
				return false;
			}

			if (filter.DateTo == null)
			{
				Filter.Cache.RaiseExceptionHandling<TransactionsFilter.date>(filter, filter.DateTo,
					new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<TransactionsFilter.dateTo>(Filter.Cache)));
				return false;
			}

			if (filter.MaxTransactions == null)
			{
				Filter.Cache.RaiseExceptionHandling<TransactionsFilter.maxTransactions>(filter, filter.MaxTransactions,
					new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<TransactionsFilter.dateTo>(Filter.Cache)));
				return false;
			}

			if (filter.LineNbr == null)
			{
				Filter.Cache.RaiseExceptionHandling<TransactionsFilter.lineNbr>(filter, filter.LineNbr,
					new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<TransactionsFilter.lineNbr>(Filter.Cache)));
				return false;
			}

			if (filter.Date > filter.DateTo)
			{
				Filter.Cache.RaiseExceptionHandling<TransactionsFilter.dateTo>(filter, filter.DateTo,
					new PXException(CS.Messages.Entry_GE, filter.Date /*PXUIFieldAttribute.GetDisplayName<TransactionsFilter.date>(Filter.Cache)*/));
				return false;
			}

			return true;
		}

		private (string, string)[] GetCorpCardFilters(CABankFeed bankFeed)
		{
			if (CorpCardFilters == null)
			{
				var manager = GetSpecificManager(bankFeed);
				CorpCardFilters = manager.AvailableCorpCardFilters;
			}
			return CorpCardFilters;
		}

		private string[] GetAvailableTransactionFields(CABankFeed bankFeed)
		{
			if(AvailableTransactionFields == null && bankFeed?.Type != null)
			{
				var manager = GetSpecificManager(bankFeed);
				AvailableTransactionFields = manager.AvailableTransactionFields;
			}
			return AvailableTransactionFields;
		}

		private CABankFeed CopyBankFeedRecord(CABankFeed bankFeed)
		{
			return (CABankFeed)this.BankFeed.Cache.CreateCopy(bankFeed);
		}

		private BankFeedManager GetSpecificManager(CABankFeed bankfeed)
		{
			return BankFeedManagerProvider(bankfeed.Type);
		}

		private void SetCashAccountWarningIfNeeded(PXCache cache, CABankFeedDetail detail)
		{
			PXSetPropertyException exception = null;
			if (detail.CashAccountID == null)
			{
				exception = new PXSetPropertyException(Messages.SpecifyCashAccount, PXErrorLevel.Warning);
			}
			else
			{
				CashAccount cashAccount = PXSelectorAttribute.Select<CABankFeedDetail.cashAccountID>(cache, detail) as CashAccount;
				if (!string.IsNullOrEmpty(detail.Currency) && cashAccount.CuryID != detail.Currency)
				{
					exception = new PXSetPropertyException(Messages.SpecifyCorrectCurrency, PXErrorLevel.Warning, detail.Currency);
				}
			}

			PXFieldState state = (PXFieldState)cache.GetStateExt<CABankFeedDetail.cashAccountID>(detail);
			if (state.ErrorLevel != PXErrorLevel.Error)
			{
				cache.RaiseExceptionHandling<CABankFeedDetail.cashAccountID>(detail, detail.CashAccountID, exception);
			}
		}

		private void SetEnabledRequired(PXCache cache, CABankFeed bf)
		{
			if (bf == null) return;
			if (!bf.CreateExpenseReceipt.HasValue) return;
			PXUIFieldAttribute.SetEnabled<CABankFeed.defaultExpenseItemID>(cache, bf, bf.CreateExpenseReceipt.Value);
			PXUIFieldAttribute.SetEnabled<CABankFeed.createReceiptForPendingTran>(cache, bf, bf.CreateExpenseReceipt.Value);
			PXUIFieldAttribute.SetRequired<CABankFeed.defaultExpenseItemID>(cache, bf.CreateExpenseReceipt.Value);
		}
	}
}
