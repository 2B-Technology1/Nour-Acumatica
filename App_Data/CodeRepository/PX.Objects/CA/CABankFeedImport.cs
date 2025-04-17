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
using PX.Objects.CA.Descriptor;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.EP.DAC;
using PX.Objects.GL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PX.Objects.CA
{
	public class CABankFeedImport : PXGraph<CABankFeedImport>
	{
		public PXCancel<CABankFeed> Cancel;
		public PXAction<CABankFeed> ViewBankFeed;

		[InjectDependency]
		internal Func<string, BankFeedManager> BankFeedManagerProvider
		{
			get;
			set;
		}

		[PXButton]
		[PXUIField(DisplayName = "")]
		protected virtual void viewBankFeed()
		{
			CABankFeed bankFeed = BankFeeds.Current;
			CABankFeedMaint graph = CreateInstance<CABankFeedMaint>();

			PXSelectBase<CABankFeed> query = new PXSelect<CABankFeed,
				Where<CABankFeed.bankFeedID, Equal<Required<CABankFeed.bankFeedID>>>>(this);

			graph.BankFeed.Current = query.SelectSingle(bankFeed.BankFeedID);

			if (graph.BankFeed.Current != null)
			{
				throw new PXRedirectRequiredException(graph, false, string.Empty);
			}

		}

		public PXAction<CABankFeed> AddBankFeed;
		[PXUIField(DisplayName = "")]
		[PXInsertButton(CommitChanges = true)]
		public virtual void addBankFeed()
		{
			CABankFeedMaint graph = CreateInstance<CABankFeedMaint>();
			graph.BankFeed.Current = graph.BankFeed.Insert();
			graph.BankFeed.Cache.IsDirty = false;
			throw new PXRedirectRequiredException(graph, false, string.Empty);
		}

		public PXAction<CABankFeed> EditBankFeed;
		[PXUIField(DisplayName = "")]
		[PXEditDetailButton(CommitChanges = true)]
		public virtual void editBankFeed()
		{
			CABankFeed bankFeed = BankFeeds.Current;
			CABankFeedMaint graph = CreateInstance<CABankFeedMaint>();
			graph.BankFeed.Current = graph.BankFeed.Search<CABankFeed.bankFeedID>(bankFeed.BankFeedID);
			throw new PXRedirectRequiredException(graph, false, string.Empty);
		}

		[PXFilterable]
		public PXProcessing<CABankFeed> BankFeeds;

		protected IEnumerable<CABankFeedDetail> BankFeedDetails;
		protected IEnumerable<CABankFeedCorpCard> CorpCardDetails;
		protected IEnumerable<CABankFeedExpense> ExpenseDetails;
		protected BankFeedTransactionsImport ImportGraph;
		protected ExpenseClaimDetailEntry ExpenseGraph;
		protected CABankMatchingProcess MatchingGraph;
		protected CABankTranHeader lastProcessedTranHeader;
		protected int TranRecordProcessed;

		public CABankFeedImport()
		{
			var guid = (Guid)this.UID;
			BankFeeds.SetProcessDelegate((List<CABankFeed> list) =>
			{
				ImportTransactions(list, guid).GetAwaiter().GetResult();
			});
		}

		public virtual async Task<Dictionary<int, string>> DoImportTransactionsAndCreateReceipts(CABankFeed bankFeed, Guid guid)
		{
			ReadDetailsData(bankFeed);
			BankFeedImportException importException = null;
			Dictionary<int, string> lastUpdatedStatements = new Dictionary<int, string>();
			if (BankFeedDetails.Count() == 0) return lastUpdatedStatements;

			foreach (CABankFeedDetail bankFeedDetail in BankFeedDetails.Where(i => i.CashAccountID != null))
			{
				var res = await DoImportTransAndCreateReceiptsForAccount(bankFeed, bankFeedDetail, guid);
				if (res.IsOk)
				{
					var cashAccountId = bankFeedDetail.CashAccountID.Value;
					if (!lastUpdatedStatements.ContainsKey(cashAccountId) && res.LastUpdatedStatementRefNbr != null)
					{
						lastUpdatedStatements.Add(cashAccountId, res.LastUpdatedStatementRefNbr);
					}
				}
				else
				{
					importException = res.ImportException;

				}
			}

			if (importException != null)
			{
				throw importException;
			}

			return lastUpdatedStatements;
		}

		public virtual async Task<Dictionary<int, string>> DoImportTransaAndCreateReceiptsForAccounts(CABankFeed bankFeed, List<CABankFeedDetail> details,  Guid guid)
		{
			ReadDetailsData(bankFeed);
			BankFeedImportException importException = null;
			Dictionary<int, string> lastUpdatedStatements = new Dictionary<int, string>();
			var filteredDetails = details.Where(i => i.CashAccountID != null
				&& BankFeedDetails.Any(ii => ii.LineNbr == i.LineNbr && ii.BankFeedID == i.BankFeedID));

			foreach (var detail in filteredDetails)
			{
				var res = await DoImportTransAndCreateReceiptsForAccount(bankFeed, detail, guid);
				if (res.IsOk)
				{
					var cashAccountId = detail.CashAccountID.Value;
					if (!lastUpdatedStatements.ContainsKey(cashAccountId) && res.LastUpdatedStatementRefNbr != null)
					{
						lastUpdatedStatements.Add(cashAccountId, res.LastUpdatedStatementRefNbr);
					}
				}
				else
				{
					importException = res.ImportException;
				}
			}

			if (importException != null)
			{
				throw importException;
			}
			return lastUpdatedStatements;
		}

		public virtual void UpdateRetrievalStatus(CABankFeed item, string status,
			string message = null, DateTime? time = null)
		{
			item.RetrievalStatus = status;
			item.RetrievalDate = time ?? PXTimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, LocaleInfo.GetTimeZone());
			item.ErrorMessage = ShrinkMessage(message);
			PersistUpdatedData(item);
		}

		public virtual void UpdateRetrievalStatus(CABankFeed item, BankFeedImportException importException)
		{
			if (importException.Reason == BankFeedImportException.ExceptionReason.LoginFailed)
			{
				item.RetrievalStatus = CABankFeedRetrievalStatus.LoginFailed;
			}
			else
			{
				item.RetrievalStatus = CABankFeedRetrievalStatus.Error;
			}
			item.RetrievalDate = importException.ErrorTime;
			item.ErrorMessage = ShrinkMessage(importException.Message);
			PersistUpdatedData(item);
		}

		protected virtual async Task<ImportResult> DoImportTransAndCreateReceiptsForAccount(CABankFeed bankFeed, CABankFeedDetail detail, Guid guid)
		{
			BankFeedImportException importException = null;
			string headerRefNbr = null;
			try
			{
				TranRecordProcessed = 0;
				DateTime startDate = CABankFeedMaint.GetImportStartDate(this, bankFeed, detail);
				var trans = await GetTransactionFromService(bankFeed, startDate, detail.AccountID);
				using (var context = new CAMatchProcessContext(GetMatchingGraph(), detail.CashAccountID, guid))
				{
					foreach (BankFeedTransaction tran in trans)
					{
						DisableSubordinationCheck(GetExpenseGraph());
						using (PXTransactionScope scope = new PXTransactionScope())
						{
							if (detail != null
								&& (detail.ImportStartDate == null || detail.ImportStartDate <= tran.Date))
							{
								CABankTran bankTran = null;
								if (tran.Pending == false)
								{
									bankTran = CreateOrUpdateBankStatement(tran, bankFeed, detail, GetImportGraph());
									if (bankTran != null)
									{
										headerRefNbr = bankTran.HeaderRefNbr;
									}
								}

								if (bankFeed.CreateExpenseReceipt == true && bankFeed.DefaultExpenseItemID != null && tran.Amount > 0
									&& ((tran.Pending == false && bankTran != null) || (tran.Pending == true && bankFeed.CreateReceiptForPendingTran == true))
									&& PXAccess.FeatureInstalled<FeaturesSet.expenseManagement>() == true)
								{
									EPExpenseClaimDetails receipt = CreateOrUpdateExpenseReceipt(tran, bankFeed, detail, GetExpenseGraph());
									if (receipt != null && bankTran != null)
									{
										MatchTransactionAndReceipt(bankTran, receipt, GetMatchingGraph());
									}
								}
							}
							scope.Complete();
						}
					}
				}
				UpdateRetrievalStatusForDetail(detail, CABankFeedRetrievalStatus.Success);
				LogTranRecordProcessedCount(detail);
			}
			catch (CAMatchProcessContext.CashAccountLockedException ex)
			{
				throw ex;
			}
			catch (PXOuterException ex)
			{
				var message = string.Join(" ", ex.InnerMessages.Select(i => PXMessages.LocalizeNoPrefix(i)));
				if (message == string.Empty) message = ex.Message;

				importException = new BankFeedImportException(message);
				UpdateRetrievalStatusForDetail(detail, importException);
			}
			catch (BankFeedImportException ex)
			{
				importException = ex;
				UpdateRetrievalStatusForDetail(detail, importException);
			}
			catch (Exception ex)
			{
				importException = new BankFeedImportException(ex.Message);
				UpdateRetrievalStatusForDetail(detail, importException);
			}

			return  new ImportResult() { ImportException = importException, IsOk = importException == null, LastUpdatedStatementRefNbr = headerRefNbr };
		}

		protected virtual void UpdateRetrievalStatusForDetail(CABankFeedDetail item, string status,
			string message = null, DateTime? time = null)
		{
			item.RetrievalStatus = status;
			item.RetrievalDate = time ?? PXTimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, LocaleInfo.GetTimeZone());
			item.ErrorMessage = message;
			PersistUpdatedData(item);
		}

		protected virtual void UpdateRetrievalStatusForDetail(CABankFeedDetail item, BankFeedImportException importException)
		{
			if (importException.Reason == BankFeedImportException.ExceptionReason.LoginFailed)
			{
				item.RetrievalStatus = CABankFeedRetrievalStatus.LoginFailed;
			}
			else
			{
				item.RetrievalStatus = CABankFeedRetrievalStatus.Error;
			}
			item.RetrievalDate = importException.ErrorTime;
			item.ErrorMessage = ShrinkMessage(importException.Message);
			PersistUpdatedData(item);
		}

		protected virtual void PersistUpdatedData(CABankFeed item)
		{
			this.Caches[typeof(CABankFeed)].Update(item);
			this.Caches[typeof(CABankFeed)].Persist(PXDBOperation.Update);
		}

		protected virtual void PersistUpdatedData(CABankFeedDetail item)
		{
			this.Caches[typeof(CABankFeedDetail)].Update(item);
			this.Caches[typeof(CABankFeedDetail)].Persist(PXDBOperation.Update);
		}

		protected virtual CABankTran CreateOrUpdateBankStatement(BankFeedTransaction tran, CABankFeed bankFeed, CABankFeedDetail bankFeedDetail, CABankTransactionsImport importGraph)
		{
			bool headerFound = false;
			CABankTranHeader bankTranHeader = null;

			if (lastProcessedTranHeader != null && lastProcessedTranHeader.CashAccountID == bankFeedDetail.CashAccountID
				&& lastProcessedTranHeader.StartBalanceDate <= tran.Date && lastProcessedTranHeader.EndBalanceDate >= tran.Date)
			{
				bankTranHeader = lastProcessedTranHeader;
				headerFound = true;
			}

			if (!headerFound)
			{
				bankTranHeader = GetLatestBankTranHeaderWithTransByCashAccount(bankFeedDetail.CashAccountID, importGraph);
				if (bankTranHeader?.EndBalanceDate >= tran.Date)
				{
					if (bankTranHeader?.StartBalanceDate > tran.Date)
					{
						return null;
					}
					importGraph.Header.Current = bankTranHeader;
					headerFound = true;
				}
			}

			if (!headerFound)
			{
				CABankTranHeader bankTranHeaderNewSearch = GetLatestBankTranHeaderByCashAccount(bankFeedDetail.CashAccountID, importGraph);
				if (bankTranHeaderNewSearch != null)
				{
					bankTranHeader = bankTranHeaderNewSearch;
					if (bankTranHeader?.EndBalanceDate >= tran.Date && bankTranHeader.StartBalanceDate <= tran.Date)
					{
						importGraph.Header.Current = bankTranHeader;
						headerFound = true;
					}
				}
			}

			if (!headerFound)
			{
				var statementDates = GetStatementDates(bankFeedDetail, tran.Date.Value);

				if (bankTranHeader != null && statementDates.StartDate <= bankTranHeader.EndBalanceDate
					&& statementDates.EndDate > bankTranHeader.EndBalanceDate)
				{
					statementDates.StartDate = bankTranHeader.EndBalanceDate.Value.AddDays(1);
				}
				else if (bankTranHeader != null && statementDates.StartDate < bankTranHeader.StartBalanceDate
					&& statementDates.EndDate >= bankTranHeader.StartBalanceDate)
				{
					statementDates.EndDate = bankTranHeader.StartBalanceDate.Value.AddDays(-1);
				}

				bankTranHeader = new CABankTranHeader();
				bankTranHeader.CashAccountID = bankFeedDetail.CashAccountID;
				bankTranHeader.DocDate = statementDates.EndDate;
				bankTranHeader.StartBalanceDate = statementDates.StartDate;
				bankTranHeader.EndBalanceDate = statementDates.EndDate;
				importGraph.Header.Insert(bankTranHeader);
			}

			CABankTran targetLine = null;
			foreach (CABankTran line in importGraph.Details.Select())
			{
				if (line.ExtTranID == tran.TransactionID)
				{
					targetLine = line;
					break;
				}
			}

			if (targetLine == null)
			{
				try
				{
					targetLine = importGraph.Details.Insert();
					var cacheExt = PXCache<CABankTran>.GetExtension<CABankTranFeedSource>(targetLine);
					targetLine.ExtRefNbr = tran.TransactionID;
					targetLine.ExtTranID = tran.TransactionID;
					targetLine.TranDate = tran.Date;
					targetLine.TranDesc = tran.Name;
					targetLine.CuryCreditAmt = tran.Amount >= 0 ? tran.Amount : 0;
					targetLine.CuryDebitAmt = tran.Amount < 0 ? -1 * tran.Amount : 0;
					TransferMappedValues(bankFeed.BankFeedID, targetLine, tran);
					cacheExt.Source = bankFeed.Type;
					importGraph.Details.Update(targetLine);
					bankTranHeader = importGraph.Header.Current;
					bankTranHeader.CuryEndBalance = bankTranHeader.CuryDetailsEndBalance;

					importGraph.Header.Update(bankTranHeader);
					importGraph.Persist();
					TranRecordProcessed++;
				}
				catch
				{
					if (targetLine != null)
					{
						importGraph.Details.Cache.Remove(targetLine);
					}
					throw;
				}
			}

			lastProcessedTranHeader = bankTranHeader;
			return targetLine;
		}

		private readonly SyFormulaProcessor _formulaProcessor = new SyFormulaProcessor();

		protected virtual string CalcValue(string formula, BankFeedTransaction bankFeedTransaction)
		{
			string result = string.Empty;
			SyFormulaFinalDelegate fieldValueGetter;
			CABankFeedMappingSourceHelper cABankFeedMappingSourceHelper = new CABankFeedMappingSourceHelper(this.GetPrimaryCache());
			fieldValueGetter = (names) =>
			{
				PXCache cache = null;
				cache = new PXCache<BankFeedTransaction>(this);
				string fieldName = cABankFeedMappingSourceHelper.GetFieldNameByDisplayName(names[0]);
				if (string.IsNullOrEmpty(fieldName))
				{
					throw new PXArgumentException(nameof(names), ErrorMessages.ArgumentOutOfRangeException);
				}
				return cache.GetValue(bankFeedTransaction, fieldName);
			};

			result = _formulaProcessor.Evaluate(formula, fieldValueGetter)?.ToString();

			return result;
		}

		private void TransferMappedValues(string bankFeedId, CABankTran bankTran, BankFeedTransaction bankFeedTransaction)
		{
			IEnumerable<CABankFeedFieldMapping> bankFeedFieldMappings = PXSelectReadonly<CABankFeedFieldMapping,
				Where<CABankFeedFieldMapping.bankFeedID, Equal<Required<CABankFeedFieldMapping.bankFeedID>>,
					And<CABankFeedFieldMapping.active, Equal<True>>>>.Select(this, bankFeedId).RowCast<CABankFeedFieldMapping>();
			foreach (CABankFeedFieldMapping bankFeedFieldMapping in bankFeedFieldMappings)
			{
				string sourceValue = CalcValue(bankFeedFieldMapping.SourceFieldOrValue, bankFeedTransaction);

				switch (bankFeedFieldMapping.TargetField)
				{
					case CABankFeedMappingTarget.CardNumber:
						bankTran.CardNumber = sourceValue;
						break;
					case CABankFeedMappingTarget.ExtRefNbr:
						bankTran.ExtRefNbr = sourceValue;
						break;
					case CABankFeedMappingTarget.InvoiceNbr:
						bankTran.InvoiceInfo = sourceValue;
						break;
					case CABankFeedMappingTarget.PayeeName:
						bankTran.PayeeName = sourceValue;
						break;
					case CABankFeedMappingTarget.PaymentMethod:
						bankTran.PaymentMethodID = sourceValue;
						break;
					case CABankFeedMappingTarget.TranCode:
						bankTran.TranCode = sourceValue;
						break;
					case CABankFeedMappingTarget.TranDesc:
						bankTran.TranDesc = sourceValue;
						break;
					case CABankFeedMappingTarget.UserDesc:
						bankTran.UserDesc = sourceValue;
						break;
					default:
						break;
				}
			}
		}

		protected virtual EPExpenseClaimDetails CreateOrUpdateExpenseReceipt(BankFeedTransaction tran, CABankFeed bankFeed, CABankFeedDetail bankFeedDetail, ExpenseClaimDetailEntry expenseGraph)
		{
			expenseGraph.ClaimDetails.Current = null;
			CABankFeedCorpCard corpCard = GetSuitableCorpCard(tran);
			if (corpCard == null) return null;
			int? expenseItem = GetExpenseItemForExpenseReceipt(tran, bankFeed);
			if (expenseItem == null) return null;

			EPExpenseClaimDetails expense = null;
			if (!string.IsNullOrEmpty(tran.PendingTransactionID))
			{
				expenseGraph.ClaimDetails.Current = GetExpenseClaimDetailsByRefNbr(tran.PendingTransactionID);
				expense = expenseGraph.ClaimDetails.Current;
			}

			if(expense == null)
			{
				expenseGraph.ClaimDetails.Current = GetExpenseClaimDetailsByRefNbr(tran.TransactionID);
				expense = expenseGraph.ClaimDetails.Current;
			}

			CheckEmplIsLinkedToCorpCard(corpCard, bankFeed);

			bool newDoc = false;
			if (expense?.RefNbr != null) return null;

			try
			{
				if (expense == null)
				{
					expense = expenseGraph.ClaimDetails.Insert();
					newDoc = true;
				}

				var bankFeedExt = PXCache<EPExpenseClaimDetails>.GetExtension<ExpenseClaimDetailsBankFeedExt>(expense);

				string prevBankTranStatus = bankFeedExt.BankTranStatus;
				expense.ExpenseDate = tran.Date;
				expense.CuryUnitCost = tran.Amount;
				expense.CuryExtCost = tran.Amount;
				expense.CuryID = tran.IsoCurrencyCode;
				expense.TranDesc = tran.Name;
				bankFeedExt.BankTranStatus = tran.Pending == true ? EPBankTranStatus.Pending
					: EPBankTranStatus.Posted;

				if (newDoc)
				{
					expense.EmployeeID = corpCard.EmployeeID;
					expense.InventoryID = expenseItem;
					expense.Qty = 1;
					expense.ExpenseRefNbr = tran.TransactionID;
					bankFeedExt.Category = tran.Category;
					expense.PaidWith = EPExpenseClaimDetails.paidWith.CardCompanyExpense;
					expense.CorpCardID = corpCard.CorpCardID;
				}

				expense = expenseGraph.ClaimDetails.Update(expense);
				if (tran.Pending == true && expense.Hold == false)
				{
					expenseGraph.hold.Press();
				}
				if (tran.Pending == false && expense.Hold == true && prevBankTranStatus == EPBankTranStatus.Pending)
				{
					expenseGraph.Submit.Press();
				}

				expenseGraph.Persist();
				return expenseGraph.ClaimDetails.Current;
			}
			catch
			{
				if (expense != null)
				{
					expenseGraph.ClaimDetails.Cache.Remove(expense);
				}
				throw;
			}
		}

		protected virtual void MatchTransactionAndReceipt(CABankTran bankTran, EPExpenseClaimDetails expenseReceipt, CABankMatchingProcess matchingGraph)
		{
			if (expenseReceipt.Hold == true) return;
			if (expenseReceipt.CorpCardID == null) return;
			if (expenseReceipt.BankTranDate != null) return;

			CashAccount cashAccount = GetCashAccountById(bankTran.CashAccountID.Value);
			if (cashAccount.UseForCorpCard == false) return;

			matchingGraph.Clear();
			CABankTranMatch match = new CABankTranMatch()
			{
				TranID = bankTran.TranID,
				TranType = bankTran.TranType,
				DocModule = BatchModule.EP,
				DocRefNbr = expenseReceipt.ClaimDetailCD,
				DocType = EPExpenseClaimDetails.DocType,
				ReferenceID = expenseReceipt.EmployeeID,
				CuryApplAmt = expenseReceipt.ClaimCuryTranAmtWithTaxes,
			};

			matchingGraph.TranMatch.Insert(match);
			expenseReceipt.BankTranDate = bankTran.TranDate;
			matchingGraph.ExpenseReceipts.Update(expenseReceipt);
			bankTran.DocumentMatched = true;
			matchingGraph.CABankTran.Update(bankTran);
			matchingGraph.Persist();
		}

		protected virtual int? GetExpenseItemForExpenseReceipt(BankFeedTransaction tran, CABankFeed bankFeed)
		{
			int? ret = bankFeed.DefaultExpenseItemID;
			foreach (CABankFeedExpense item in ExpenseDetails)
			{
				string checkValue = GetValueFromTranByExpenseFilter(tran, item);
				if (checkValue != null && item.MatchValue != null)
				{
					if (CheckValueByRule(checkValue, item.MatchValue, item.MatchRule))
					{
						if (item.DoNotCreate == true || item.InventoryItemID == null)
						{
							ret = null;
						}
						else
						{
							ret = item.InventoryItemID;
						}
						break;
					}
				}
			}
			return ret;
		}

		protected virtual CABankFeedCorpCard GetSuitableCorpCard(BankFeedTransaction tran)
		{
			CABankFeedCorpCard ret = null;
			var corpCards = CorpCardDetails.Where(i => i.AccountID == tran.AccountID);
			foreach (CABankFeedCorpCard item in corpCards)
			{
				if (item.MatchRule == CABankFeedMatchRule.Empty)
				{
					ret = item;
					break;
				}

				string checkValue = GetValueFromTranByCorpCardFilter(tran, item);
						
				if (checkValue != null && item.MatchValue != null)
				{
					if (CheckValueByRule(checkValue, item.MatchValue, item.MatchRule))
					{
						ret = item;
						break;
					}
				}
			}
			return ret;
		}

		protected virtual StatementDates GetStatementDates(CABankFeedDetail bankFeedDet, DateTime date)
		{
			StatementDates ret = null;
			string statementPeriod = bankFeedDet.StatementPeriod;

			switch (statementPeriod)
			{
				case CABankFeedStatementPeriod.Month: ret = GetStatementDatesForMonth(bankFeedDet, date); break;
				case CABankFeedStatementPeriod.Week: ret = GetStatementDatesForWeek(bankFeedDet, date); break;
				case CABankFeedStatementPeriod.Day: ret = new StatementDates() { StartDate = date, EndDate = date }; break;
			}

			return ret;
		}

		protected virtual StatementDates GetStatementDatesForWeek(CABankFeedDetail bankFeedDet, DateTime date)
		{
			int dayOfWeek = (int)date.DayOfWeek + 1;
			DateTime startDate;
			DateTime endDate;
			if (dayOfWeek == bankFeedDet.StatementStartDay)
			{
				startDate = new DateTime(date.Year, date.Month, date.Day);
				endDate = startDate.AddDays(6);
			}
			else if (dayOfWeek > bankFeedDet.StatementStartDay)
			{
				startDate = new DateTime(date.Year, date.Month, date.Day);
				startDate = startDate.AddDays(bankFeedDet.StatementStartDay.Value - dayOfWeek);
				endDate = startDate.AddDays(6);
			}
			else
			{
				startDate = new DateTime(date.Year, date.Month, date.Day);
				startDate = startDate.AddDays(bankFeedDet.StatementStartDay.Value - dayOfWeek - 7);
				endDate = startDate.AddDays(6);
			}
			return new StatementDates() { StartDate = startDate, EndDate = endDate};
		}

		protected virtual StatementDates GetStatementDatesForMonth(CABankFeedDetail bankFeedDet, DateTime date)
		{
			int startDay = bankFeedDet.StatementStartDay.Value;
			DateTime currentStartDate;
			DateTime nextStartDate;

			int lastDayInMonth = DateTime.DaysInMonth(date.Year, date.Month);

			if (date.Date.Day < startDay && date.Date.Day < lastDayInMonth)
			{
				currentStartDate = new DateTime(date.Year, date.Month, 1).AddMonths(-1);
				currentStartDate = GetDateTimeByComponents(currentStartDate.Year, currentStartDate.Month, startDay);
				nextStartDate = GetDateTimeByComponents(date.Year, date.Month, startDay);
			}
			else
			{
				currentStartDate = GetDateTimeByComponents(date.Year, date.Month, startDay);
				nextStartDate = new DateTime(date.Year, date.Month, 1).AddMonths(1);
				nextStartDate = GetDateTimeByComponents(nextStartDate.Year, nextStartDate.Month, startDay);
			}
			DateTime currentEndDate = nextStartDate.AddDays(-1);
			return new StatementDates() { StartDate = currentStartDate, EndDate = currentEndDate };
		}

		protected virtual void CheckEmplIsLinkedToCorpCard(CABankFeedCorpCard corpCardItem, CABankFeed bankFeed)
		{
			EPEmployeeCorpCardLink result = PXSelect<EPEmployeeCorpCardLink,
				Where<EPEmployeeCorpCardLink.corpCardID, Equal<Required<EPEmployeeCorpCardLink.corpCardID>>,
					And<EPEmployeeCorpCardLink.employeeID, Equal<Required<EPEmployeeCorpCardLink.employeeID>>>>>
				.SelectSingleBound(this, null, corpCardItem.CorpCardID, corpCardItem.EmployeeID);

			if (result == null)
			{
				throw new PXException(Messages.EmplIsNotLinkedToCorpCard, corpCardItem.EmployeeName, corpCardItem.CardName, bankFeed.BankFeedID);
			}
		}

		protected virtual void DisableSubordinationCheck(ExpenseClaimDetailEntry expenseGraph)
		{
			var targetAttr = expenseGraph.ClaimDetails.Cache.GetAttributes(null, nameof(EPExpenseClaimDetails.EmployeeID))
				.OfType<PXConfigureSubordinateAndWingmenSelectorAttribute>().FirstOrDefault();

			if (targetAttr != null)
			{
				targetAttr.AllowSubordinationAndWingmenCheck = false;
			}
		}

		public async static Task<Dictionary<int, string>> ImportTransactionsForAccounts(CABankFeed bankFeed, List<CABankFeedDetail> items, Guid guid)
		{
			if (bankFeed == null)
			{
				throw new PXArgumentException(nameof(bankFeed));
			}
			if (items == null)
			{
				throw new PXArgumentException(nameof(items));
			}
			if (items.Any(i => i.BankFeedID != bankFeed.BankFeedID))
			{
				throw new PXArgumentException(nameof(items));
			}

			Exception lastException = null;
			Dictionary<int, string> lastUpdatedStatements = new Dictionary<int, string>();
			var graph = PXGraph.CreateInstance<CABankFeedImport>();
			graph.CheckFeed(bankFeed);
			try
			{
				lastUpdatedStatements = await graph.DoImportTransaAndCreateReceiptsForAccounts(bankFeed, items, guid);
				var lastProcessedTime = graph.GetLastProcessedTime();
				graph.UpdateRetrievalStatus(bankFeed, CABankFeedRetrievalStatus.Success, null, lastProcessedTime);
			}
			catch (BankFeedImportException ex)
			{
				lastException = ex;
				graph.UpdateRetrievalStatus(bankFeed, ex);
			}
			catch (Exception ex)
			{
				lastException = ex;
				graph.UpdateRetrievalStatus(bankFeed, CABankFeedRetrievalStatus.Error, ex.Message);
			}

			if (lastException != null)
			{
				throw lastException;
			}

			return lastUpdatedStatements;
		}

		public async static Task<Dictionary<int, string>> ImportTransactions(List<CABankFeed> items, Guid guid)
		{
			if (items == null)
			{
				throw new PXArgumentException(nameof(items));
			}

			Exception lastException = null; ;
			Dictionary<int, string> lastUpdatedStatements = new Dictionary<int, string>();
			CABankFeedImport graph = PXGraph.CreateInstance<CABankFeedImport>();
			var processingInfo = PXLongOperation.GetCustomInfoForCurrentThread(PXProcessing.ProcessingKey) as PXProcessingInfo;
			for (int i = 0; i < items.Count; i++)
			{
				var item = items[i];
				try
				{
					graph.CheckFeed(item);
					var res = await graph.DoImportTransactionsAndCreateReceipts(item, guid);
					foreach (var kvp in res)
					{
						if (!lastUpdatedStatements.ContainsKey(kvp.Key))
						{
							lastUpdatedStatements.Add(kvp.Key, kvp.Value);
						}
					}
					var lastProcessedTime = graph.GetLastProcessedTime();
					graph.UpdateRetrievalStatus(item, CABankFeedRetrievalStatus.Success, null, lastProcessedTime);
				}
				catch (CAMatchProcessContext.CashAccountLockedException ex)
				{
					lastException = ex;
					graph.UpdateProcessingInfo(i, processingInfo, ex);
				}
				catch (BankFeedImportException ex)
				{
					lastException = ex;
					graph.UpdateRetrievalStatus(item, ex);
					graph.UpdateProcessingInfo(i, processingInfo, ex);
				}
				catch (Exception ex)
				{
					lastException = ex;
					graph.UpdateRetrievalStatus(item, CABankFeedRetrievalStatus.Error, ex.Message);
					graph.UpdateProcessingInfo(i, processingInfo, ex);
				}
			}

			if (processingInfo == null && lastException != null)
			{
				throw lastException;
			}

			return lastUpdatedStatements;
		}

		protected virtual CACorpCard GetCorpCardById(int? corpCardId)
		{
			return PXSelect<CACorpCard, Where<CACorpCard.corpCardID, Equal<Required<CACorpCard.corpCardID>>>>
				.SelectSingleBound(this, null, corpCardId);
		}

		protected virtual CashAccount GetCashAccountById(int cashAccount)
		{
			return PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>
				.SelectSingleBound(this, null, cashAccount);
		}
		protected virtual IEnumerable<CABankFeedDetail> GetBankFeedDetailByBankFeedId(string bankFeedId)
		{
			foreach (CABankFeedDetail item in PXSelectJoin<CABankFeedDetail,
				InnerJoin<CABankFeed, On<CABankFeedDetail.bankFeedID, Equal<CABankFeed.bankFeedID>>,
				InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<CABankFeedDetail.cashAccountID>>>>,
				Where<CABankFeed.status, In3<CABankFeedStatus.active, CABankFeedStatus.setupRequired>,
					And<CABankFeed.bankFeedID, Equal<Required<CABankFeed.bankFeedID>>>>>.Select(this, bankFeedId))
			{
				yield return item;
			}
		}

		protected virtual IEnumerable<CABankFeedCorpCard> GetBankFeedCordCardByBankFeedId(string bankFeedId)
		{
			foreach (CABankFeedCorpCard item in PXSelectJoin<CABankFeedCorpCard,
				InnerJoin<CABankFeed, On<CABankFeedCorpCard.bankFeedID, Equal<CABankFeed.bankFeedID>>,
				InnerJoin<CACorpCard, On<CACorpCard.corpCardID, Equal<CABankFeedCorpCard.corpCardID>>,
				InnerJoin<CABankFeedDetail, On<CABankFeedCorpCard.bankFeedID, Equal<CABankFeedDetail.bankFeedID>,
					And<CABankFeedCorpCard.accountID, Equal<CABankFeedDetail.accountID>>>,
				InnerJoin<EPEmployee, On<CABankFeedCorpCard.employeeID, Equal<EPEmployee.bAccountID>>>>>>,
				Where<CABankFeed.status, In3<CABankFeedStatus.active, CABankFeedStatus.setupRequired>,
					And<CABankFeed.bankFeedID, Equal<Required<CABankFeed.bankFeedID>>>>>.Select(this, bankFeedId))
			{
				yield return item;
			}
		}

		protected virtual IEnumerable<CABankFeedExpense> GetBankFeedExpenseByBankFeedId(string bankFeedId)
		{
			foreach (CABankFeedExpense item in PXSelectJoin<CABankFeedExpense,
				InnerJoin<CABankFeed, On<CABankFeedExpense.bankFeedID, Equal<CABankFeed.bankFeedID>>>,
				Where<CABankFeed.status, In3<CABankFeedStatus.active, CABankFeedStatus.setupRequired>,
					And<CABankFeed.bankFeedID, Equal<Required<CABankFeed.bankFeedID>>>>>.Select(this, bankFeedId))
			{
				yield return item;
			}
		}

		protected virtual CABankTranHeader GetLatestBankTranHeaderWithTransByCashAccount(int? cashAccountId, PXGraph graph)
		{
			return PXSelectJoin<CABankTranHeader, InnerJoin<CABankTran, On<CABankTran.headerRefNbr, Equal<CABankTranHeader.refNbr>>>,
				Where<CABankTranHeader.cashAccountID, Equal<Required<CABankTranHeader.cashAccountID>>>,
				OrderBy<Desc<CABankTranHeader.endBalanceDate>>>.SelectSingleBound(graph, null, cashAccountId);
		}

		protected virtual CABankTranHeader GetLatestBankTranHeaderByCashAccount(int? cashAccountId, PXGraph graph)
		{
			return PXSelect<CABankTranHeader,
				Where<CABankTranHeader.cashAccountID, Equal<Required<CABankTranHeader.cashAccountID>>>,
				OrderBy<Desc<CABankTranHeader.endBalanceDate>>>.SelectSingleBound(graph, null, cashAccountId);
		}

		protected virtual EPExpenseClaimDetails GetExpenseClaimDetailsByRefNbr(string refNbr)
		{
			PXResultset<EPExpenseClaimDetails> expenseClaimDetails = PXSelect<EPExpenseClaimDetails, Where<EPExpenseClaimDetails.expenseRefNbr,
				Equal<Required<EPExpenseClaimDetails.expenseRefNbr>>>>.Select(this, refNbr);
			foreach (EPExpenseClaimDetails expenseClaimDetail in expenseClaimDetails)
			{
				if (expenseClaimDetail.ExpenseRefNbr.Equals(refNbr, StringComparison.Ordinal))
					return expenseClaimDetail;
			}
			return null;
		}

		private async Task<IEnumerable<BankFeedTransaction>> GetTransactionFromService(CABankFeed bankFeed, DateTime startDate, string accountId)
		{
			var input = new LoadTransactionsData();
			input.AccountID = accountId;
			input.StartDate = startDate;
			input.EndDate = PXTimeZoneInfo.Now;

			var manager = GetSpecificManager(bankFeed);

			return await manager.GetTransactionsAsync(input, bankFeed);
		}

		private void CheckFeed(CABankFeed bankFeed)
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.bankFeedIntegration>())
			{
				throw new PXException(Messages.BankFeedIntegrationIsDisabled);
			}

			if (bankFeed.Status == CABankFeedStatus.Disconnected || bankFeed.Status == CABankFeedStatus.Suspended
				|| bankFeed.Status == CABankFeedStatus.MigrationRequired
				|| (bankFeed.Status == CABankFeedStatus.SetupRequired && bankFeed.AccountQty == bankFeed.UnmatchedAccountQty))
			{
				var label = CABankFeedStatus.ListAttribute.GetStatuses.Where(ii => ii.Item1 == bankFeed.Status)
					.Select(ii => ii.Item2).FirstOrDefault();
				throw new PXException(Messages.BankFeedWrongImportStatus, bankFeed.BankFeedID, label);
			}
		}

		private string GetValueFromTranByCorpCardFilter(BankFeedTransaction tran, CABankFeedCorpCard corpCard)
		{
			string ret = null;

			switch (corpCard.MatchField)
			{
				case CABankFeedMatchField.AccountOwner: ret = tran.AccountOwner; break;
				case CABankFeedMatchField.Name: ret = tran.Name; break;
				case CABankFeedMatchField.CheckNumber: ret = tran.CheckNumber; break;
				case CABankFeedMatchField.Memo: ret = tran.Memo; break;
				case CABankFeedMatchField.PartnerAccountId: ret = tran.PartnerAccountId; break;
			}
			return ret;
		}

		private bool CheckValueByRule(string value, string pattern, string mathRule)
		{
			bool found = false;
			value = value.Trim().ToLowerInvariant();
			pattern = pattern.Trim().ToLowerInvariant();
			if (pattern != string.Empty)
			{
				switch (mathRule)
				{
					case CABankFeedMatchRule.StartsWith:
						found = value.StartsWith(pattern);
						break;
					case CABankFeedMatchRule.EndsWith:
						found = value.EndsWith(pattern);
						break;
					case CABankFeedMatchRule.Contains:
						found = value.Contains(pattern);
						break;
				}
			}
			return found;
		}

		private DateTime GetDateTimeByComponents(int year, int month, int day)
		{
			DateTime ret = DateTime.MinValue;
			int lastDayInMonth = DateTime.DaysInMonth(year, month);
			if (lastDayInMonth < day)
			{
				ret = new DateTime(year, month, lastDayInMonth);
			}
			else
			{
				ret = new DateTime(year, month, day);
			}
			return ret;
		}

		private string GetValueFromTranByExpenseFilter(BankFeedTransaction tran, CABankFeedExpense expense)
		{
			string ret = null;
			if (expense.MatchField == CABankFeedMatchField.Category)
			{
				ret = tran.Category;
			}
			if (expense.MatchField == CABankFeedMatchField.Name)
			{
				ret = tran.Name;
			}
			return ret;
		}

		private string ShrinkMessage(string message)
		{
			const int maxMessageLength = 250;
			return message?.Length > maxMessageLength ? message.Substring(0, maxMessageLength) : message;
		}

		private DateTime? GetLastProcessedTime()
		{
			DateTime? lastProcessedTime = null;

			var lastProcessedDetail = this.Caches[typeof(CABankFeedDetail)].Cached.RowCast<CABankFeedDetail>().
				Where(ii => ii.RetrievalStatus == CABankFeedRetrievalStatus.Success)
				.OrderByDescending(ii => ii.RetrievalDate).FirstOrDefault();
			if (lastProcessedDetail != null && lastProcessedDetail.RetrievalDate != null)
			{
				lastProcessedTime = lastProcessedDetail.RetrievalDate.Value;
			}

			return lastProcessedTime;
		}

		private void UpdateProcessingInfo(int index, PXProcessingInfo processingInfo, Exception ex)
		{
			if (processingInfo != null)
			{
				var procMessage = new PXProcessingMessage(PXErrorLevel.RowError, ex.Message);
				processingInfo.Messages[index] = procMessage;
				PXTrace.WriteError(ex);
			}
		}

		private BankFeedManager GetSpecificManager(CABankFeed bankfeed)
		{
			return BankFeedManagerProvider(bankfeed.Type);
		}

		private BankFeedTransactionsImport GetImportGraph()
		{
			if (ImportGraph == null)
			{
				ImportGraph = PXGraph.CreateInstance<BankFeedTransactionsImport>();
			}
			return ImportGraph;
		}

		private ExpenseClaimDetailEntry GetExpenseGraph()
		{
			if (ExpenseGraph == null)
			{
				ExpenseGraph = PXGraph.CreateInstance<ExpenseClaimDetailEntry>();
			}
			return ExpenseGraph;
		}

		private CABankMatchingProcess GetMatchingGraph()
		{
			if (MatchingGraph == null)
			{
				MatchingGraph = PXGraph.CreateInstance<CABankMatchingProcess>();
			}
			return MatchingGraph;
		}

		private void ReadDetailsData(CABankFeed bankFeed)
		{
			BankFeedDetails = GetBankFeedDetailByBankFeedId(bankFeed.BankFeedID);
			CorpCardDetails = GetBankFeedCordCardByBankFeedId(bankFeed.BankFeedID);
			ExpenseDetails = GetBankFeedExpenseByBankFeedId(bankFeed.BankFeedID);
		}


		private void LogTranRecordProcessedCount(CABankFeedDetail detail)
		{
			if (detail.CashAccountID != null)
			{
				CashAccount cashAccount = GetCashAccountById(detail.CashAccountID.Value);
				var cashAccountCd = cashAccount.CashAccountCD.Trim();
				PXTrace.WriteInformation($"Number of created bank transactions for cash account {cashAccountCd}: {TranRecordProcessed}.");
			}
		}

		protected class StatementDates
		{
			public DateTime StartDate { get; set; }
			public DateTime EndDate { get; set; }
		}
	}
}
