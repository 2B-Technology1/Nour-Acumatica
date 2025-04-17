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
using System.Collections;

namespace PX.Objects.GL
{
	public partial class GLBudgetEntry
	{
		protected struct BudgetKey
		{
			public int? BranchID { get; }
			public int? LedgerID { get; }
			public string FinYear { get; }

			public BudgetKey(GLBudgetLineKey lineKey)
			{
				BranchID = lineKey.BranchID;
				LedgerID = lineKey.LedgerID;
				FinYear = lineKey.FinYear;
			}

			public BudgetKey(BudgetFilter filter)
			{
				BranchID = filter.BranchID;
				LedgerID = filter.LedgerID;
				FinYear = filter.FinYear;
			}

			public PXResultset<GLBudgetLine> SelectLines(PXGraph graph) => PXSelect<GLBudgetLine,
						Where<GLBudgetLine.branchID, Equal<Required<BudgetFilter.branchID>>,
						And<GLBudgetLine.ledgerID, Equal<Required<BudgetFilter.ledgerID>>,
						And<GLBudgetLine.finYear, Equal<Required<BudgetFilter.finYear>>,
						And<GLBudgetLine.isGroup, Equal<False>>>>>>.Select(graph, BranchID, LedgerID, FinYear);
		}

		protected struct GLBudgetLineKey
		{
			public int BranchID;
			public int LedgerID;
			public int AccountID;
			public int SubID;
			public string FinYear;

			public static GLBudgetLineKey Create(GLBudgetLine line) => new GLBudgetLineKey
			{
				BranchID = line.BranchID ?? 0,
				LedgerID = line.LedgerID ?? 0,
				FinYear = line.FinYear,
				AccountID = line.AccountID ?? 0,
				SubID = line.SubID ?? 0
			};

			public static GLBudgetLineKey Create(IDictionary keys, GLBudgetEntry graph) => new GLBudgetLineKey
			{
				BranchID = ExtractBranch(keys[nameof(GLBudgetLine.BranchID)] as string, graph),
				LedgerID = ExtractLedger(keys[nameof(GLBudgetLine.LedgerID)] as string, graph),
				FinYear = ExtractFinYear(keys[nameof(GLBudgetLine.FinYear)] as string, graph),
				AccountID = ExtractAccountID(keys[nameof(GLBudgetLine.AccountID)] as string, graph.accountIndex),
				SubID = ExtractSubID(keys[nameof(GLBudgetLine.SubID)] as string, graph.subIndex)
			};

			private static int ExtractAccountID(string value, AccountIndex accountIndex)
			{
				if (string.IsNullOrEmpty(value)) return 0;
				else return accountIndex.GetID(value) ?? 0;
			}

			private static int ExtractSubID(string value, SubIndex subIndex)=>
				subIndex.GetID(string.IsNullOrEmpty(value) ? subIndex.DefaultCD : value) ?? -1;

			private static int ExtractBranch(string value, GLBudgetEntry graph)
			{
				if (string.IsNullOrEmpty(value)) return graph.Filter.Current.BranchID ?? 0;
				else return PXAccess.GetBranchID(value) ?? 0;
			}

			private static int ExtractLedger(string value, GLBudgetEntry graph)
			{
				if (string.IsNullOrEmpty(value)) return graph.Filter.Current.LedgerID ?? 0;
				else return graph.ledgerIndex.GetID(value) ?? 0;
			}

			private static string ExtractFinYear(string value, GLBudgetEntry graph)
			{
				if (string.IsNullOrEmpty(value)) return graph.Filter.Current.FinYear;
				else return value;
			}

			public bool Match(BudgetKey filter) => BranchID.Equals(filter.BranchID) && LedgerID.Equals(filter.LedgerID) && FinYear.Equals(filter.FinYear);			

			public override bool Equals(object o)
			{
				if (o is GLBudgetLineKey)
				{
					GLBudgetLineKey d = (GLBudgetLineKey)o;
					return
						BranchID.Equals(d.BranchID) &&
						LedgerID.Equals(d.LedgerID) &&
						FinYear.Equals(d.FinYear) &&
						AccountID.Equals(d.AccountID) &&
						SubID.Equals(d.SubID);
				}
				else return false;
			}

			public override int GetHashCode()
			{
				unchecked
				{
					int result = 37;
					result *= 397;
					result += BranchID.GetHashCode();
					result *= 397;
					result += LedgerID.GetHashCode();
					result *= 397;
					result += FinYear.GetHashCode();
					result *= 397;
					result += AccountID.GetHashCode();
					result *= 397;
					result += SubID.GetHashCode();
					return result;
				}
			}
		}
	}
}
