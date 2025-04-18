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

using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.PM
{
	public class AccountGroupMaint : PXGraph<AccountGroupMaint, PMAccountGroup>
	{
		#region Views/Selects

		public PXSelect<PMAccountGroup> AccountGroup;
		public PXSelect<PMAccountGroup, Where<PMAccountGroup.groupID, Equal<Current<PMAccountGroup.groupID>>>> AccountGroupProperties;
		[PXCopyPasteHiddenView]
		[PXVirtualDAC]
		public PXSelect<AccountPtr> Accounts;
		[PXCopyPasteHiddenView]
		public PXSelect<Account> GLAccount;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelect<PMBudget, Where<PMBudget.accountGroupID, Equal<Current<PMAccountGroup.groupID>>, And<PMBudget.type, NotEqual<Required<PMBudget.type>>>>> BudgetToFix;

		[PXViewName(Messages.AccountGroupAnswers)]
		public CRAttributeList<PMAccountGroup> Answers;

		public IEnumerable accounts()
		{
			Dictionary<int, AccountPtr> inCache = new Dictionary<int, AccountPtr>();

			foreach (AccountPtr item in Accounts.Cache.Cached)
			{
				PXEntryStatus status = Accounts.Cache.GetStatus(item);
				inCache.Add(item.AccountID.Value, item);
				if (status == PXEntryStatus.Inserted || status == PXEntryStatus.Updated || status == PXEntryStatus.Notchanged)
				{
					yield return item;
				}
			}

			PXSelectBase<Account> select = new PXSelect<Account,
				Where<Account.accountGroupID, Equal<Current<PMAccountGroup.groupID>>>>(this);

			bool isDirty = Accounts.Cache.IsDirty;

			foreach (Account acct in select.Select())
			{
				if (!inCache.ContainsKey(acct.AccountID.Value))
				{
					AccountPtr ptr = new AccountPtr();
					ptr.AccountID = acct.AccountID;
					ptr.AccountClassID = acct.AccountClassID;
					ptr.CuryID = acct.CuryID;
					ptr.Description = acct.Description;
					ptr.Type = acct.Type;
					ptr.IsDefault = AccountGroup.Current.AccountID == acct.AccountID;

					ptr = Accounts.Insert(ptr);
					Accounts.Cache.SetStatus(ptr, PXEntryStatus.Notchanged);

					yield return ptr;
				}
			}


			Accounts.Cache.IsDirty = isDirty;
		}

		public PXSetup<GLSetup> GLSetup;

		#endregion

		public AccountGroupMaint()
		{
			PXUIFieldAttribute.SetVisible<AccountPtr.curyID>(Accounts.Cache, null, IsMultiCurrency);
		}

		#region Event Handlers

		protected virtual void _(Events.RowUpdated<AccountPtr> e)
		{
			AccountPtr row = e.Row as AccountPtr;
			AccountPtr oldRow = e.OldRow as AccountPtr;
			if (row != null)
			{
				Account account = GetAccountByID(row.AccountID);
				if (account != null)
				{
					row.AccountClassID = account.AccountClassID;
					row.CuryID = account.CuryID;
					row.Description = account.Description;
					row.Type = account.Type;
				}

				bool refresh = false;
				if (row.IsDefault != oldRow.IsDefault)
				{
					if (row.IsDefault == true)
					{
						foreach (AccountPtr ptr in Accounts.Select())
						{
							if (ptr.AccountID == row.AccountID) continue;

							if (ptr.IsDefault == true)
							{
								Accounts.Cache.SetValue<AccountPtr.isDefault>(ptr, false);
								Accounts.Cache.SmartSetStatus(ptr, PXEntryStatus.Updated);
								refresh = true;
							}
						}
					}
				}
				else
				{
					bool hasDefault = false;
					foreach (AccountPtr ptr in Accounts.Select())
					{
						if (ptr.AccountID == row.AccountID) continue;

						if (ptr.IsDefault == true)
						{
							hasDefault = true;
						}
					}

					if (!hasDefault)
					{
						row.IsDefault = true;
					}
				}

				if (refresh)
					Accounts.View.RequestRefresh();
			}
		}

		protected virtual void _(Events.RowPersisting<AccountPtr> e)
		{
			AccountAttribute.VerifyAccountIsNotControl<AccountPtr.accountID>(e.Cache, e.Args);

			AccountPtr row = e.Row as AccountPtr;
			if (row != null)
			{
				switch (e.Operation)
				{
					case PXDBOperation.Delete:
						RemoveAccount(row);
						break;
					case PXDBOperation.Insert:
					case PXDBOperation.Update:
						AddAccount(row);
						break;
				}

				e.Cancel = true;
			}
		}

		protected virtual void _(Events.FieldVerifying<AccountPtr, AccountPtr.accountID> e)
		{
			AccountAttribute.VerifyAccountIsNotControl<AccountPtr.accountID>(e.Cache, e.Args);

			AccountPtr row = e.Row as AccountPtr;
			if (row != null)
			{
				Account account = PXSelect<Account>.Search<Account.accountID>(this, e.NewValue);

				if (account != null && account.IsCashAccount == true)
				{
					e.Cache.RaiseExceptionHandling<AccountPtr.accountID>(row, e.NewValue,
						new PXSetPropertyException(GL.Messages.CashAccountIsNotForProjectPurposes, PXErrorLevel.Warning, account.AccountCD));
				}

				if (account != null && AccountGroup.Current != null && account.AccountGroupID != null && account.AccountGroupID != AccountGroup.Current.GroupID)
				{
					PMAccountGroup ag = PMAccountGroup.PK.Find(this, account.AccountGroupID);

					e.Cache.RaiseExceptionHandling<AccountPtr.accountID>(row, e.NewValue,
						new PXSetPropertyException(Warnings.AccountIsUsed, PXErrorLevel.Warning, ag.GroupCD));
				}
			}
		}

		protected virtual void _(Events.RowDeleting<AccountPtr> e)
		{
			AccountPtr row = e.Row as AccountPtr;
			if (e.Row == null) return;

			PMSetup setup = PXSelect<PMSetup>.Select(this);
			if (setup != null && row.AccountID == setup.UnbilledRemainderAccountID)
			{
				e.Cancel = true;
				throw new PXSetPropertyException(Messages.CannotDeleteAccountFromAccountGroup, PXErrorLevel.Error);
			}
		}

		protected virtual void _(Events.RowDeleted<AccountPtr> e)
		{
			AccountGroup.Cache.IsDirty = true;
		}


		protected virtual void _(Events.RowSelected<PMAccountGroup> e)
		{
			PMAccountGroup row = e.Row as PMAccountGroup;
			if (row != null)
			{
				PXUIFieldAttribute.SetVisible<PMAccountGroup.sortOrder>(e.Cache, row, GLSetup.Current.COAOrder == 4); //todo show on custom sort order. ??

				Accounts.Cache.AllowInsert = row.Type != PMAccountType.OffBalance;
				PXUIFieldAttribute.SetEnabled<PMAccountGroup.type>(e.Cache, row, !IsAccountsExist() && !IsBalanceExist());
				PXUIFieldAttribute.SetVisible<PMAccountGroup.isExpense>(e.Cache, row, DisplayIsExpenseToggleForOffBalance() && row.Type == PMAccountType.OffBalance);
				PXUIFieldAttribute.SetEnabled<PMAccountGroup.revenueAccountGroupID>(e.Cache, row, row.IsExpense == true);

				bool isChangeRequestSettingVisible = row.Type == PMAccountType.OffBalance && row.IsExpense == true || row.Type == GL.AccountType.Expense;
				PXUIFieldAttribute.SetEnabled<PMAccountGroup.createsCommitment>(e.Cache, row, isChangeRequestSettingVisible);
				PXUIFieldAttribute.SetEnabled<PMAccountGroup.defaultLineMarkupPct>(e.Cache, row, isChangeRequestSettingVisible);
			}
		}

		public virtual bool DisplayIsExpenseToggleForOffBalance()
		{
			return true;
		}

		protected virtual void _(Events.RowDeleting<PMAccountGroup> e)
		{
			PMAccountGroup row = e.Row as PMAccountGroup;
			if (row != null)
			{
				if (IsAccountsExist())
				{
					e.Cancel = true;
					throw new PXException(Messages.Account_FK);
				}

				PMBudget ps = PXSelect<PMBudget, Where<PMBudget.accountGroupID, Equal<Required<PMAccountGroup.groupID>>>>.SelectWindowed(this, 0, 1, row.GroupID);
				if (ps != null)
				{
					e.Cancel = true;
					throw new PXException(Messages.ProjectStatus_FK);
				}
			}
		}


		protected virtual void _(Events.FieldVerifying<PMAccountGroup, PMAccountGroup.isActive> e)
		{
			if (e.Row != null && (bool?)e.NewValue != true)
			{
				if (IsAccountsExist())
				{
					throw new PXSetPropertyException(Messages.AccountDiactivate_FK, PXErrorLevel.Error);
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMAccountGroup, PMAccountGroup.type> e)
		{
			if (e.Row != null)
			{
				e.Cache.SetDefaultExt<PMAccountGroup.sortOrder>(e.Row);

				if (e.Row.Type != PMAccountType.OffBalance)
				{
					e.Row.IsExpense = e.Row.Type == GL.AccountType.Expense;
				}
			}
		}

		protected virtual void _(Events.RowUpdated<PMAccountGroup> e)
		{
			if (e.Row != null && e.Row.Type == PMAccountType.OffBalance && e.Row.IsExpense != e.OldRow.IsExpense)
			{
				string expectedValue = e.Row.IsExpense == true ? GL.AccountType.Expense : PM.PMAccountType.OffBalance;

				foreach (PMBudget budget in BudgetToFix.Select(expectedValue))
				{
					budget.Type = expectedValue;
					BudgetToFix.Update(budget);
				}
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMAccountGroup, PMAccountGroup.sortOrder> e)
		{
			if (e.Row != null)
			{
				if (e.Row.Type == PMAccountType.OffBalance)
				{
					e.NewValue = (short)5;
				}
				else
				{
					int ordinal = GL.AccountType.Ordinal(e.Row.Type);
					GL.GLSetup setup = PXSelect<GL.GLSetup>.Select(this);

					if (setup != null)
					{
						if (setup.COAOrder.Value.ToString() == "128") //Custom Order - fixed preset order
						{
							if (e.Row.Type == GL.AccountType.Income)
							{
								e.NewValue = (short)1;
							}
							else if (e.Row.Type == GL.AccountType.Expense)
							{
								e.NewValue = (short)2;
							}
							else if (e.Row.Type == GL.AccountType.Asset)
							{
								e.NewValue = (short)3;
							}
							else if (e.Row.Type == GL.AccountType.Liability)
							{
								e.NewValue = (short)4;
							}
						}
						else
						{
							string order = GL.AccountType.COAOrderOptions[setup.COAOrder.Value];
							e.NewValue = short.Parse(order.Substring(ordinal, 1));
						}
					}

				}

			}
		}

		#endregion

		private bool IsAccountsExist()
		{
			Account account = PXSelect<Account, Where<Account.accountGroupID, Equal<Current<PMAccountGroup.groupID>>>>.SelectWindowed(this, 0, 1);

			if (account == null)
			{
				foreach (object x in Accounts.Cache.Inserted)
					return true;
			}

			return account != null;
		}

		private bool IsBalanceExist()
		{
			PMBudget status = PXSelect<PMBudget, Where<PMBudget.accountGroupID, Equal<Current<PMAccountGroup.groupID>>>>.SelectWindowed(this, 0, 1);
			return status != null;
		}

		protected virtual void RemoveAccount(AccountPtr ptr)
		{
			Account account = GetAccountByID(ptr.AccountID);
			if (account != null && account.AccountGroupID != null)
			{
				account.AccountGroupID = null;
				GLAccount.Update(account);
				GLAccount.Cache.PersistUpdated(account);
			}
		}

		protected virtual void AddAccount(AccountPtr ptr)
		{
			Account account = GetAccountByID(ptr.AccountID);
			if (account != null && AccountGroup.Current != null)
			{
				account.AccountGroupID = AccountGroup.Current.GroupID;
				GLAccount.Update(account);
				GLAccount.Cache.PersistUpdated(account);
			}
		}

		private Account GetAccountByID(int? id)
		{
			return PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, id);
		}

		private bool IsMultiCurrency
		{
			get { return PXAccess.FeatureInstalled<FeaturesSet.multicurrency>(); }
		}

		public override void Persist()
		{
			foreach (AccountPtr ptr in Accounts.Select())
			{
				if (ptr.IsDefault == true && AccountGroup.Current.AccountID != ptr.AccountID)
				{
					AccountGroup.Current.AccountID = ptr.AccountID;
					AccountGroup.Update(AccountGroup.Current);
				}
			}

			base.Persist();
		}

		#region Local Types
		[PXHidden]
		[Serializable]
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public partial class AccountPtr : PX.Data.IBqlTable
		{
			#region AccountID
			public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
			protected Int32? _AccountID;
			[PMAccountAttribute(IsKey = true)]
			[AvoidControlAccounts]
			public virtual Int32? AccountID
			{
				get
				{
					return this._AccountID;
				}
				set
				{
					this._AccountID = value;
				}
			}
			#endregion
			#region Type
			public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
			protected string _Type;
			[PXString(1)]
			[AccountType.List()]
			[PXUIField(DisplayName = "Type", Enabled = false)]
			public virtual string Type
			{
				get
				{
					return this._Type;
				}
				set
				{
					this._Type = value;
				}
			}
			#endregion
			#region AccountClassID
			public abstract class accountClassID : PX.Data.BQL.BqlString.Field<accountClassID> { }
			protected string _AccountClassID;
			[PXString(20, IsUnicode = true)]
			[PXUIField(DisplayName = "Account Class", Enabled = false)]
			[PXSelector(typeof(AccountClass.accountClassID))]
			public virtual string AccountClassID
			{
				get
				{
					return this._AccountClassID;
				}
				set
				{
					this._AccountClassID = value;
				}
			}
			#endregion
			#region Description
			public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
			protected String _Description;
			[PXString(60, IsUnicode = true)]
			[PXUIField(DisplayName = "Description", Enabled = false)]
			public virtual String Description
			{
				get
				{
					return this._Description;
				}
				set
				{
					this._Description = value;
				}
			}
			#endregion
			#region CuryID
			public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
			protected string _CuryID;
			[PXString(5, IsUnicode = true)]
			[PXUIField(DisplayName = "Currency", Enabled = false)]
			//[PXSelector(typeof(Currency.curyID))]
			public virtual string CuryID
			{
				get
				{
					return this._CuryID;
				}
				set
				{
					this._CuryID = value;
				}
			}
			#endregion
			#region IsDefault
			public abstract class isDefault : PX.Data.BQL.BqlBool.Field<isDefault> { }
			protected bool? _IsDefault;
			[PXBool()]
			[PXUIField(DisplayName = "Default")]
			public virtual bool? IsDefault
			{
				get
				{
					return this._IsDefault;
				}
				set
				{
					this._IsDefault = value;
				}
			}
			#endregion
		}
		#endregion
	}
}
