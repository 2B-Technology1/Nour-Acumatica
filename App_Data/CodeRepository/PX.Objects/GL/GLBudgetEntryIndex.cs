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
using PX.Data.BQL.Fluent;
using System.Collections.Generic;

namespace PX.Objects.GL
{
	public partial class GLBudgetEntry
	{
		private abstract class Index
		{
			protected readonly Dictionary<int, string> CDs = new Dictionary<int, string>();
			protected readonly Dictionary<string, int> IDs = new Dictionary<string, int>();
			protected readonly Dictionary<int, bool> isActiveFlags = new Dictionary<int, bool>();

			public int? GetID(string cd)
			{
				return IDs.TryGetValue(cd, out int id) ? (int?)id : null;
			}

			public string GetCD(int id)
			{
				return CDs.TryGetValue(id, out string cd) ? cd : null;
			}

			public bool IsActive(int id)
			{
				return isActiveFlags.TryGetValue(id, out bool isActive) ? isActive : false;
			}
		}

		private class AccountIndex : Index
		{
			public AccountIndex(PXGraph graph)
			{
				PXView accountsView = new PXView(graph, true, SelectFrom<Account>.View.ReadOnly.GetCommand());
				using (new PXFieldScope(accountsView, typeof(Account.accountID), typeof(Account.accountCD), typeof(Account.active)))
				{
					foreach (Account account in accountsView.SelectMulti())
					{
						CDs.Add((int)account.AccountID, account.AccountCD);
						IDs.Add(account.AccountCD, (int)account.AccountID);
						isActiveFlags.Add((int)account.AccountID, account.Active ?? false);
					}
				}
			}
		}

		private class SubIndex : Index
		{
			public readonly int? DefaultID;
			public readonly string DefaultCD;

			public SubIndex(PXGraph graph)
			{
				PXView subsView = new PXView(graph, true, SelectFrom<Sub>.View.ReadOnly.GetCommand());
				using (new PXFieldScope(subsView, typeof(Sub.subID), typeof(Sub.subCD), typeof(Sub.active)))
				{
					foreach (Sub sub in subsView.SelectMulti())
					{
						CDs.Add((int)sub.SubID, sub.SubCD);
						IDs.Add(sub.SubCD, (int)sub.SubID);
						isActiveFlags.Add((int)sub.SubID, sub.Active ?? false);
					}
					DefaultID = SubAccountAttribute.TryGetDefaultSubID();
					CDs.TryGetValue(DefaultID ?? 0, out DefaultCD);
				}
			}
		}

		private class LedgerIndex : Index
		{
			public LedgerIndex(PXGraph graph)
			{
				PXView accountsView = new PXView(graph, true, SelectFrom<Ledger>.View.ReadOnly.GetCommand());
				using (new PXFieldScope(accountsView, typeof(Ledger.ledgerID), typeof(Ledger.ledgerID), typeof(Ledger.balanceType)))
				{
					foreach (Ledger ledger in accountsView.SelectMulti())
					{
						CDs.Add((int)ledger.LedgerID, ledger.LedgerCD);
						IDs.Add(ledger.LedgerCD, (int)ledger.LedgerID);
						isActiveFlags.Add((int)ledger.LedgerID, ledger.BalanceType == LedgerBalanceType.Budget);
					}
				}

			}
		}
	}
}
