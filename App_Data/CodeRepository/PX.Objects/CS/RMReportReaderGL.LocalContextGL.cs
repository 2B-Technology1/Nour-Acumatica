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
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;

using PX.Common;
using PX.CS;
using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.CS
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public partial class RMReportReaderGL : PXGraphExtension<RMReportReader>
	{
		// Local context are structs to reduce memory allocations count when the code is executed sequentially

		/// <summary>
		/// A local GL reports' context used during iteration over sub accounts.
		/// </summary>
		private readonly struct SubIterationContextGL
		{
			public SharedContextGL SharedContext { get; }

			public Account CurrentAccount { get; }

			public int AccountIndex { get; }

			public NestedDictionary<int, (int BranchID, int LedgerID), Dictionary<string, GLHistory>> AccountDict { get; }

			public SubIterationContextGL(SharedContextGL sharedContext, Account currentAccount, int accountIndex,
										NestedDictionary<int, (int BranchID, int LedgerID), Dictionary<string, GLHistory>> accountDict)
			{
				SharedContext = sharedContext;
				CurrentAccount = currentAccount;
				AccountIndex = accountIndex;
				AccountDict = accountDict;
			}

			public void SubIterationNoClosures(int subIndex) => RMReportReaderGL.SubIteration(this, subIndex);
		}


		/// <summary>
		/// A local GL reports' context used during iteration over branches.
		/// </summary>
		private readonly struct BranchIterationContextGL
		{
			public SharedContextGL SharedContext { get; }

			public Account CurrentAccount { get; }

			public int AccountIndex { get; }

			public Sub CurrentSub { get; }

			public int SubIndex { get; }

			public Dictionary<(int BranchID, int LedgerID), Dictionary<string, GLHistory>> SubDict { get; }

			public BranchIterationContextGL(in SubIterationContextGL subIterationContext, Sub currentSub, int subIndex,
											Dictionary<(int BranchID, int LedgerID), Dictionary<string, GLHistory>> subDict)
			{
				SharedContext = subIterationContext.SharedContext;

				CurrentAccount = subIterationContext.CurrentAccount;
				AccountIndex = subIterationContext.AccountIndex;
				CurrentSub = currentSub;
				SubIndex = subIndex;

				SubDict = subDict;
			}

			public void BranchIterationNoClosures(Branch currentBranch) => RMReportReaderGL.BranchIteration(this, currentBranch);
		}
	}
}
