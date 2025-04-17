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
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CA;
using System.Collections;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Common;
using PX.Objects.PM;

namespace PX.Objects.GL
{
	public class AccountMaint : PXGraph<AccountMaint>, PXImportAttribute.IPXPrepareItems
	{
		public PXSavePerRow<Account, Account.accountID> Save;
		public PXCancel<Account> Cancel;
		[PXImport(typeof(Account))]
		[PXFilterable]
		public PXSelect<Account,Where<Match<Current<AccessInfo.userName>>>, OrderBy<Asc<Account.accountCD>>> AccountRecords;

		public PXSelectReadonly<GLSetup> GLSetup;
		public GLSetup GLSETUP
		{
			get
			{
				GLSetup setup = GLSetup.Select();
				if (setup == null)
				{
					setup = new GLSetup();
					setup.COAOrder = (short)0;
				}
				return setup;
			}
		}
		public PXSetup<Company> Company;
		public CMSetupSelect cmsetup;
		public PXFilter<AccountTypeDialogBoxParameters> AccountTypeChangePrepare;

		[Serializable]
		[PXHidden]
		public class AccountTypeDialogBoxParameters : IBqlTable
		{
			public abstract class message : PX.Data.BQL.BqlBool.Field<message> { }
			[PXString]
			public virtual string Message { get; set; }
		}

		protected bool? IsCOAOrderVisible = null;

		public AccountMaint()
		{
			if (string.IsNullOrEmpty(Company.Current.BaseCuryID))
			{
				throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(Company), PXMessages.LocalizeNoPrefix(CS.Messages.BranchMaint));
			}
			if (IsCOAOrderVisible == null)
			{
				IsCOAOrderVisible = (GLSetup.Current.COAOrder > 3);
				PXUIFieldAttribute.SetVisible<Account.cOAOrder>(AccountRecords.Cache, null, (bool) IsCOAOrderVisible);
				PXUIFieldAttribute.SetEnabled<Account.cOAOrder>(AccountRecords.Cache, null, (bool)IsCOAOrderVisible);
			}

			var mcFeatureInstalled = PXAccess.FeatureInstalled<FeaturesSet.multicurrency>();
			PXUIFieldAttribute.SetVisible<Account.curyID>(AccountRecords.Cache, null, mcFeatureInstalled);
			PXUIFieldAttribute.SetEnabled<Account.curyID>(AccountRecords.Cache, null, mcFeatureInstalled);

			PXUIFieldAttribute.SetVisible<Account.revalCuryRateTypeId>(AccountRecords.Cache, null, mcFeatureInstalled);
			PXUIFieldAttribute.SetEnabled<Account.revalCuryRateTypeId>(AccountRecords.Cache, null, mcFeatureInstalled);
		}

#region Repository methods

[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R1)]
		public static Account FindAccountByCD(PXGraph graph, string accountCD) => Account.UK.Find(graph, accountCD);

		protected bool PostedTransInOtherCuryExists(Account account, string curyID)
		{
			return PXSelectJoin<GLTran,
						InnerJoin<CurrencyInfo,
							On<GLTran.curyInfoID, Equal<CurrencyInfo.curyInfoID>>,
						InnerJoin<Ledger,
							On<GLTran.ledgerID, Equal<Ledger.ledgerID>>>>,
						Where<GLTran.accountID, Equal<Current<Account.accountID>>,
							And<CurrencyInfo.curyID, NotEqual<Required<CurrencyInfo.curyID>>,
							And<GLTran.posted, Equal<True>,
							And<Ledger.balanceType, NotEqual<LedgerBalanceType.report>>>>>>
						.SelectSingleBound(this, new object[] { account }, curyID)
						.Count > 0;
		}

		#endregion

		protected virtual void Account_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Account row = (Account)e.Row;
			if (row == null) return;

			Exception rateTypeWarning = (row != null && row.CuryID != null && row.CuryID != Company.Current.BaseCuryID && row.RevalCuryRateTypeId == null) ?
				new PXSetPropertyException(Messages.RevaluationRateTypeIsNotDefined, PXErrorLevel.Warning) :
				null;

			sender.RaiseExceptionHandling<Account.revalCuryRateTypeId>(row, row.RevalCuryRateTypeId, rateTypeWarning);

			PXUIFieldAttribute.SetEnabled<Account.curyID>(sender, row, row.IsCashAccount != true);
            PXUIFieldAttribute.SetEnabled<Account.postOption>(sender, row, row.IsCashAccount != true);

			PXUIFieldAttribute.SetEnabled<Account.controlAccountModule>(sender, row, row.IsCashAccount != true);
			PXUIFieldAttribute.SetEnabled<Account.allowManualEntry>(sender, row, row.IsCashAccount != true && row.ControlAccountModule != null);
		}

		protected virtual void Account_COAOrder_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (IsCOAOrderVisible == false && e.Row != null && string.IsNullOrEmpty(((Account)e.Row).Type) == false)
			{
				e.NewValue = Convert.ToInt16(AccountType.COAOrderOptions[(int)GLSetup.Current.COAOrder].Substring(AccountType.Ordinal(((Account)e.Row).Type), 1));
				e.Cancel = true;
			}
		}

		protected virtual void Account_COAOrder_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (IsCOAOrderVisible == false && e.Row != null && string.IsNullOrEmpty(((Account)e.Row).Type) == false)
			{
				e.NewValue = Convert.ToInt16(AccountType.COAOrderOptions[(int)GLSetup.Current.COAOrder].Substring(AccountType.Ordinal(((Account)e.Row).Type), 1));
			}
		}

		protected virtual void Account_Type_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (IsCOAOrderVisible == false)
			{
				sender.SetDefaultExt<Account.cOAOrder>(e.Row);
			}
		}

		protected virtual void Account_ControlAccountModule_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (e.Row == null) return;
			var acc = (Account)e.Row;
			if (acc.ControlAccountModule == null)
				acc.AllowManualEntry = false;
		}

		protected virtual void Account_AllowManualEntry_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.Row == null || (bool?)e.NewValue != true) return;
			var acc = (Account)e.Row;
			if (acc.ControlAccountModule == null)
				e.NewValue = false;
		}

		protected virtual void Account_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			if (e.NewRow == null) return;
			var account = (Account)e.NewRow;
			try
			{
				ValidateAccountGroupID(sender, account);
			}
			catch (PXSetPropertyException ex)
			{
				if (ex.ErrorLevel == PXErrorLevel.Error)
				{
					PM.PMAccountGroup item = (PM.PMAccountGroup)PXSelectorAttribute.Select<Account.accountGroupID>(sender, account);
					sender.RaiseExceptionHandling<Account.accountGroupID>(account, item.GroupCD, ex);
				}
				else
				{
					sender.RaiseExceptionHandling<Account.accountGroupID>(account, account.AccountGroupID, ex);
				}
			}
		}

		private void ValidateAccountGroupID(PXCache sender, Account account)
		{
			if (account.AccountGroupID == null) return;

			if (account.IsCashAccount == true)
			{
				throw new PXSetPropertyException(Messages.CashAccountIsNotForProjectPurposes, PXErrorLevel.Warning, account.AccountCD);
			}
			else
			{
				AccountAttribute.VerifyAccountIsNotControl(account);
			}
		}

		protected virtual void Account_Type_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			Account acct = e.Row as Account;
			if (acct.Active != null && acct.Type != (string)e.NewValue && acct.AccountID != null)
			{		
				bool hasHistory = GLUtility.IsAccountHistoryExist(this, acct.AccountID);
				if (hasHistory)
				{
					bool isFinSuperviser = !string.IsNullOrEmpty(PredefinedRoles.FinancialSupervisor)
					&& PXContext.PXIdentity.User.IsInRole(PredefinedRoles.FinancialSupervisor);

					if (!isFinSuperviser)
					{
						throw new PXSetPropertyException(Messages.AccountTypeChangeUserRole);
					}

					if (!(acct.Type.IsIn(AccountType.Asset, AccountType.Liability)
							&& e.NewValue.IsIn(AccountType.Asset, AccountType.Liability)
							|| acct.Type.IsIn(AccountType.Income, AccountType.Expense)
							&& e.NewValue.IsIn(AccountType.Income, AccountType.Expense)))
					{
						if (acct.Type != null)
						{
							var accountTypes = acct.Type switch
							{
								AccountType.Asset => new { OldAccountType = Messages.Asset, NewAccountType = Messages.Liability },
								AccountType.Liability => new { OldAccountType = Messages.Liability, NewAccountType = Messages.Asset },
								AccountType.Income => new { OldAccountType = Messages.Income, NewAccountType = Messages.Expense },
								AccountType.Expense => new { OldAccountType = Messages.Expense, NewAccountType = Messages.Income },
								_ => new { OldAccountType = string.Empty, NewAccountType = string.Empty }
							};
							throw new PXSetPropertyException(Messages.AccountTypeCannotBeChanged, accountTypes.OldAccountType, accountTypes.NewAccountType);
						}
					}
				}

				if (acct.AccountID == GLSetup.Current?.YtdNetIncAccountID)
				{
					throw new PXSetPropertyException(Messages.AccountTypeCannotBeChangedGLYTDOrRE, Messages.YTDNetIncome);
				}

				if (acct.AccountID == GLSetup.Current?.RetEarnAccountID)
				{
					throw new PXSetPropertyException(Messages.AccountTypeCannotBeChangedGLYTDOrRE, Messages.RetainedEarnings);
				}
			}	
		}

		protected virtual void Account_CuryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			Account acct = e.Row as Account;

			if ((acct.CuryID != null) && (acct.CuryID != Company.Current.BaseCuryID))
			{
				acct.RevalCuryRateTypeId = cmsetup.Current.GLRateTypeReval;
			}
			else
			{
				acct.RevalCuryRateTypeId = null;
			}
		}

		protected virtual void Account_RevalCuryRateTypeId_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			Account acct = e.Row as Account;

			if (((string)e.NewValue != null) && ((acct.CuryID == null) || (acct.CuryID == Company.Current.BaseCuryID)) && !PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>())
			{
			  throw new PXSetPropertyException(Messages.AccountRevalRateTypefailed);
			}

		}

		protected virtual void Account_GLConsolAccountCD_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			Account account = e.Row as Account;

			if (account == null)
				return;

			if (account.AccountID == GLSETUP.YtdNetIncAccountID
			    && e.NewValue != null)
			{
				throw new PXSetPropertyException(Messages.ConsolidationAccountCannotBeSpecified);
			}
		}

		protected virtual void Account_CuryID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			Account acct = cache.Locate(e.Row) as Account;
			if (acct == null)
			{
				return;
			}
			string newCuryID = (string)e.NewValue;
			if (string.IsNullOrEmpty(acct.CuryID) && !string.IsNullOrEmpty(newCuryID))
			{
				if(PXSelect<BranchAcctMap,
				Where<BranchAcctMap.mapAccountID, Equal<Required<Account.accountID>>>>.Select(this, acct.AccountID).Any())
				{
					throw new PXSetPropertyException(Messages.CannotSetCurrencyToMappingAccount);
				}
				if (!PostedTransInOtherCuryExists(acct, newCuryID))
				{
					return;
				}
			}

			if (acct.CuryID != newCuryID)
			{
				if (newCuryID != null || acct.IsCashAccount == true)
				{
					bool hasHistory = GLUtility.IsAccountHistoryExist(this, acct.AccountID);
					if (hasHistory)
					{
						throw new PXSetPropertyException(Messages.CannotChangeAccountCurrencyTransactionsExist);
					}
				}
			}	

			if (acct.IsCashAccount == true && string.IsNullOrEmpty(newCuryID))
			{
				throw new PXSetPropertyException(Messages.CannotClearCurrencyInCashAccount);
			}
			
		}

		protected virtual void Account_AccountClassID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<Account.type>(e.Row);
		}

		protected virtual void Account_BranchID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (string.IsNullOrEmpty(((Account)e.Row).CuryID))
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		public override int Persist(Type cacheType, PXDBOperation operation)
		{
			if (operation == PXDBOperation.Update)
			{
				List<Account> accountTypeChangedList = AccountRecords.Cache.Updated.RowCast<Account>()
					.Where(x => ((Account)AccountRecords.Cache.GetOriginal(x)).Type != x.Type).ToList();

				if (accountTypeChangedList.Count > 0)
				{
					PXResultset<Account> updatedAccounts = PXSelectJoinGroupBy<Account,
														InnerJoin<GLHistory, On<Account.accountID, Equal<GLHistory.accountID>>>,
														Where<GLHistory.accountID, In<Required<GLHistory.accountID>>>,
														Aggregate<GroupBy<GLHistory.accountID>>>.Select(this, accountTypeChangedList.Select(x => x.AccountID).ToArray());

					bool glTrans = PXSelect<GLTran,
							Where<GLTran.accountID, In<Required<Account.accountID>>,
							And<GLTran.posted, Equal<True>>>>.Select(this, accountTypeChangedList.Select(x => x.AccountID).ToArray()).Any();

					bool pmTrans = PXSelect<PMTran,
							Where<PMTran.accountID, In<Required<Account.accountID>>,
							Or<PMTran.offsetAccountID, In<Required<Account.accountID>>, And<PMTran.released, Equal<True>>>>>
							.Select(this, accountTypeChangedList.Select(x => x.AccountID).ToArray(), accountTypeChangedList.Select(x => x.AccountID).ToArray()).Any();

					if (updatedAccounts.Count > 0 || pmTrans)
					{
						List<string> accountCDs = updatedAccounts.FirstTableItems.ToList().Select(x => x.AccountCD).ToList();

						if (pmTrans && glTrans)
						{
							AccountTypeChangePrepare.Current.Message = PXLocalizer.LocalizeFormat(Messages.AccountTypeChangeWithPMTranConfirmation, string.Join(", ", accountCDs));
							PXTrace.Logger.Information(PXLocalizer.LocalizeFormat(Messages.AccountTypeChangeWithPMTranConfirmation, string.Join(", ", accountCDs)));
						}
						else if (pmTrans && !glTrans)
						{
							AccountTypeChangePrepare.Current.Message = Messages.AccountTypeChangeOnlyPMTranExistConfirmation;
							PXTrace.Logger.Information(Messages.AccountTypeChangeOnlyPMTranExistConfirmation);
						}
						else
						{
							AccountTypeChangePrepare.Current.Message = PXLocalizer.LocalizeFormat(Messages.AccountTypeChangeConfirmation, string.Join(", ", accountCDs));
							PXTrace.Logger.Information(PXLocalizer.LocalizeFormat(Messages.AccountTypeChangeConfirmation, string.Join(", ", accountCDs)));
						}

						if (AccountTypeChangePrepare.AskExt() == WebDialogResult.OK && pmTrans)
						{
							ProjectBalanceValidationProcess graph = PXGraph.CreateInstance<ProjectBalanceValidationProcess>();
							graph.RebuildBalanceOnAccountTypeChange(accountTypeChangedList);
						}
						else if (AccountTypeChangePrepare.AskExt() == WebDialogResult.No)
						{
							foreach (Account updatedAccount in accountTypeChangedList)
							{
								Account account = (Account)AccountRecords.Cache.Locate(updatedAccount);
								Account originalAccount = (Account)AccountRecords.Cache.GetOriginal(updatedAccount);
								AccountRecords.Cache.SetValueExt<Account.type>(account, originalAccount.Type);
							}
						}
					}
				}
			}

			return base.Persist(cacheType, operation);
		}

		protected virtual void Account_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			Account row = (Account)e.Row;

			try
			{
				ValidateAccountGroupID(sender, row);
			}
			catch (PXSetPropertyException ex)
			{
				if (ex.ErrorLevel == PXErrorLevel.Error)
				{
					PM.PMAccountGroup item = (PM.PMAccountGroup)PXSelectorAttribute.Select<Account.accountGroupID>(sender, row);
					sender.RaiseExceptionHandling<Account.accountGroupID>(row, item.GroupCD, ex);
					throw ex;
				}
			}

			if (!string.IsNullOrEmpty(row.CuryID))
			{
				CASetup casetup = PXSelect<CASetup>.Select(this);
				if (casetup != null && casetup.TransitAcctId != null && casetup.TransitAcctId == row.AccountID)
				{
					PXException exception = new PXException(CA.Messages.CashInTransitAccountCanNotBeDenominated);
					sender.RaiseExceptionHandling<Account.curyID>(row, row.CuryID, exception);
					throw exception;
				}

				string newCuryid;
				if (e.Operation == PXDBOperation.Update)
				{
					newCuryid = row.CuryID;
					byte[] timestamp = PXDatabase.SelectTimeStamp();

					// Acuminator disable once PX1043 SavingChangesInEventHandlers [Justification]
					PXDatabase.Update<GLHistory>(new PXDataFieldAssign("CuryID", newCuryid),
							new PXDataFieldRestrict("AccountID", ((Account)e.Row).AccountID),
							new PXDataFieldRestrict("CuryID", PXDbType.VarChar, 5, null, PXComp.ISNULL),
							new PXDataFieldRestrict("tstamp", PXDbType.Timestamp, 8, timestamp, PXComp.LE));
				}
			}

			string oldAccountType = sender.GetValueOriginal<Account.type>(row) as string;
			bool glTrans = PXSelect<GLTran,
								Where<GLTran.accountID, Equal<Required<GLTran.accountID>>>>.Select(this, row.AccountID).Any();
			if (oldAccountType != null && oldAccountType != row.Type && glTrans)
			{
				UpdateGLHistoryDetails(row.AccountID);
			}
		}

		protected virtual void UpdateGLHistoryDetails(int? accountId)
		{
			PXUpdate<
					Set<GLHistory.finYtdBalance, PX.Data.Minus<GLHistory.finYtdBalance>,
					Set<GLHistory.tranYtdBalance, PX.Data.Minus<GLHistory.tranYtdBalance>,
					Set<GLHistory.curyFinYtdBalance, PX.Data.Minus<GLHistory.curyFinYtdBalance>,
					Set<GLHistory.curyTranYtdBalance, PX.Data.Minus<GLHistory.curyTranYtdBalance>,
					Set<GLHistory.finBegBalance, PX.Data.Minus<GLHistory.finBegBalance>,
					Set<GLHistory.tranBegBalance, PX.Data.Minus<GLHistory.tranBegBalance>,
					Set<GLHistory.curyFinBegBalance, PX.Data.Minus<GLHistory.curyFinBegBalance>,
					Set<GLHistory.curyTranBegBalance, PX.Data.Minus<GLHistory.curyTranBegBalance>>>>>>>>>,
					GLHistory,
				Where<GLHistory.accountID, Equal<Required<GLHistory.accountID>>,
					And<Where<GLHistory.balanceType, Equal<LedgerBalanceType.actual>,
						Or<GLHistory.balanceType, Equal<LedgerBalanceType.report>,
						Or<GLHistory.balanceType, Equal<LedgerBalanceType.statistical>>>>>>>
				.Update(this, accountId);

			PXUpdateJoin<
					Set<GLHistory.finPtdDebit, GLHistory2.finPtdCredit,
					Set<GLHistory.finPtdCredit, GLHistory2.finPtdDebit,
					Set<GLHistory.tranPtdDebit, GLHistory2.tranPtdCredit,
					Set<GLHistory.tranPtdCredit, GLHistory2.tranPtdDebit,
					Set<GLHistory.curyFinPtdDebit, GLHistory2.curyFinPtdCredit,
					Set<GLHistory.curyFinPtdCredit, GLHistory2.curyFinPtdDebit,
					Set<GLHistory.curyTranPtdDebit, GLHistory2.curyTranPtdCredit,
					Set<GLHistory.curyTranPtdCredit, GLHistory2.curyTranPtdDebit
					>>>>>>>>,
					GLHistory,
					LeftJoin<GLHistory2,
						On<GLHistory2.ledgerID, Equal<GLHistory.ledgerID>,
						And<GLHistory2.branchID, Equal<GLHistory.branchID>,
						And<GLHistory2.accountID, Equal<GLHistory.accountID>,
						And<GLHistory2.subID, Equal<GLHistory.subID>,
						And<GLHistory2.finPeriodID, Equal<GLHistory.finPeriodID>>>>>>>,
				Where<GLHistory.accountID, Equal<Required<GLHistory.accountID>>,
					And<GLHistory.balanceType, Equal<LedgerBalanceType.budget>>>>
				.Update(this, accountId);
		}

		public PXAction<Account> viewRestrictionGroups;
		[PXUIField(DisplayName = Messages.ViewRestrictionGroups, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewRestrictionGroups(PXAdapter adapter)
		{
			if (AccountRecords.Current != null)
			{
				GLAccessByAccount graph = CreateInstance<GLAccessByAccount>();
				graph.Account.Current = graph.Account.Search<Account.accountCD>(AccountRecords.Current.AccountCD);
				throw new PXRedirectRequiredException(graph, false, "Restricted Groups");
			}
			return adapter.Get();
		}

		public PXAction<Account> accountByPeriodEnq;
		[PXUIField(DisplayName = Messages.ViewAccountByPeriod, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable AccountByPeriodEnq(PXAdapter adapter)
		{
			if (AccountRecords.Current != null)
			{
				AccountHistoryByYearEnq graph = CreateInstance<AccountHistoryByYearEnq>();
				graph.Filter.Current.AccountID = AccountRecords.Current.AccountID;
				throw new PXRedirectRequiredException(graph, false, Messages.ViewAccountByPeriod);
			}
			return adapter.Get();
		}

		public static Account GetAccountByCD(PXGraph graph, string accountCD)
		{
			Account account = Account.UK.Find(graph, accountCD);

			if (account == null)
			{
				throw new PXException(Messages.CannotFindAccount, accountCD);
			}

			return account;
		}

		#region IPXPrepareItems

		public bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			if (string.Compare(viewName, nameof(AccountRecords), true) == 0)
			{

				// we need to clone cache. Otherwise inserted record doesn't appear in grid.
				AccountMaint cl = this.Clone();
				Account located;
				PXCache ca = cl.Caches[typeof(Account)];
				bool isNewRecord = ca.Locate(keys) == 0;
				located = isNewRecord ? (Account)ca.Insert() : (Account)ca.Current;
				foreach (string inputFieldName in values.Keys.ToArray<string>())
				{
					if (!ca.Fields.Contains(inputFieldName)) continue;

					object rawState = ca.GetStateExt(located, inputFieldName);
					PXFieldState fs = rawState is PXFieldState ? (PXFieldState)rawState : null;
					if (fs != null && (!fs.Enabled || !fs.Visible))
					{
						values[inputFieldName] = ca.GetValue(located, inputFieldName);
					}
					else
					{
						// Lets recalculate disabled columns after each field assignment.
						// It gives ability to use particular field order from the input file to simulate interactive logic.
						object value = values[inputFieldName];
						ca.SetValue(located, inputFieldName, value is string v ? ca.ValueFromString(inputFieldName, v) : value);
						ca.RaiseRowSelected(located);
					}
				}

			}

			return true;

		}

		public bool RowImporting(string viewName, object row)
		{
			return row == null;
		}

		public bool RowImported(string viewName, object row, object oldRow)
		{
			return oldRow == null;
		}

		public void PrepareItems(string viewName, IEnumerable items) { }

		#endregion
	}
}
