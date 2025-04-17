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
using System;
using System.Collections.Generic;
using PX.Objects.Extensions.PaymentTransaction;
namespace PX.Objects.AR.Repositories
{
	public class CCProcTranRepository
	{
		protected readonly PXGraph _graph;

		public CCProcTranRepository(PXGraph graph)
		{
			if (graph == null) throw new ArgumentNullException(nameof(graph));

			_graph = graph;
		}

		public CCProcTran InsertCCProcTran(CCProcTran transaction)
		{
			return (CCProcTran)_graph.Caches[typeof(CCProcTran)].Insert(transaction);
		}

		public CCProcTran UpdateCCProcTran(CCProcTran transaction)
		{
			return (CCProcTran)_graph.Caches[typeof(CCProcTran)].Update(transaction);
		}

		public CCProcTran FindVerifyingCCProcTran(int? pMInstanceID)
		{
			return PXSelect<CCProcTran, Where<CCProcTran.pMInstanceID, Equal<Required<CCProcTran.pMInstanceID>>,
					And<CCProcTran.procStatus, Equal<CCProcStatus.finalized>,
						And<CCProcTran.tranStatus, Equal<CCTranStatusCode.approved>,
						And<CCProcTran.cVVVerificationStatus, Equal<CVVVerificationStatusCode.match>
							>>>>, OrderBy<Desc<CCProcTran.tranNbr>>>.Select(_graph, pMInstanceID);
		}

		public IEnumerable<CCProcTran> GetCCProcTranByTranID(int? transactionId)
		{
			return PXSelect<CCProcTran, Where<CCProcTran.transactionID, Equal<Required<CCProcTran.transactionID>>>,OrderBy<Asc<CCProcTran.tranNbr>>>
				.Select(_graph, transactionId).RowCast<CCProcTran>();
		}
	}
}
