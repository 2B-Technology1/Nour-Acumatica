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
using System.Collections.Generic;

namespace PX.Objects.CA.BankFeed
{
	class BankFeedRedirectToStatementCustomInfo : IPXCustomInfo
	{
		private readonly Dictionary<int, string> _lastUpdatedStatements;
		public BankFeedRedirectToStatementCustomInfo(Dictionary<int, string> lastUpdatedStatements)
		{
			_lastUpdatedStatements = lastUpdatedStatements;
		}

		public void Complete(PXLongRunStatus status, PXGraph graph)
		{
			var importGraph = graph as CABankTransactionsImport;
			if (importGraph == null) return;

			var currentHeader = importGraph.Header.Current;
			if (currentHeader == null || currentHeader.CashAccountID == null || status != PXLongRunStatus.Completed) return;

			var cashAccount = currentHeader.CashAccountID.Value;
			if (_lastUpdatedStatements != null && _lastUpdatedStatements.ContainsKey(cashAccount))
			{
				var headerRefNbr = _lastUpdatedStatements[cashAccount];
				if (headerRefNbr != null && currentHeader.RefNbr != headerRefNbr)
				{
					CABankTranHeader header = PXSelect<CABankTranHeader,
						Where<CABankTranHeader.refNbr, Equal<Required<CABankTranHeader.refNbr>>>>.SelectSingleBound(graph, null, headerRefNbr);
		
					importGraph.Header.Current = header;
					throw new PXRedirectRequiredException(importGraph, string.Empty);
				}
			}
		}
	}
}
