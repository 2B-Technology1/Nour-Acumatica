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

using PX.Data;

namespace PX.Objects.AR.Repositories
{
	public class ExternalTransactionRepository
	{
		protected readonly PXGraph graph;
		public ExternalTransactionRepository(PXGraph graph)
		{
			this.graph = graph ?? throw new ArgumentNullException(nameof(graph));
		}

		public ExternalTransaction FindCapturedExternalTransaction(int? pMInstanceID, string tranNbr)
		{
			if (pMInstanceID == null)
			{
				throw new ArgumentNullException(nameof(pMInstanceID));
			}

			if (string.IsNullOrEmpty(tranNbr))
			{
				throw new ArgumentException(nameof(tranNbr));
			}

			var query = new PXSelect<ExternalTransaction,
				Where<ExternalTransaction.pMInstanceID, Equal<Required<ExternalTransaction.pMInstanceID>>,
					And<ExternalTransaction.tranNumber, Equal<Required<ExternalTransaction.tranNumber>>,
					And<Where<ExternalTransaction.procStatus, Equal<ExtTransactionProcStatusCode.captureSuccess>,
						Or<ExternalTransaction.procStatus, Equal<ExtTransactionProcStatusCode.captureHeldForReview>>>>>>,
				OrderBy<Desc<ExternalTransaction.transactionID>>>(graph);
			return query.SelectSingle(pMInstanceID, tranNbr);
		}

		public Tuple<ExternalTransaction, ARPayment> GetExternalTransactionWithPayment(string tranNbr, string procCenterId)
		{
			Tuple<ExternalTransaction, ARPayment> ret = null;
			var query = new PXSelectJoin<ExternalTransaction,
					InnerJoin<ARPayment, On<ExternalTransaction.docType, Equal<ARPayment.docType>,
						And<ExternalTransaction.refNbr, Equal<ARPayment.refNbr>>>>,
					Where<ExternalTransaction.tranNumber, Equal<Required<ExternalTransaction.tranNumber>>,
					And<ExternalTransaction.processingCenterID, Equal<Required<ExternalTransaction.processingCenterID>>,
					And<Not<ExternalTransaction.syncStatus, Equal<CCSyncStatusCode.error>,
						And<ExternalTransaction.active, Equal<False>>>>>>,
					OrderBy<Desc<ExternalTransaction.transactionID>>>(graph);
			var result = (PXResult<ExternalTransaction, ARPayment>)query.Select(tranNbr, procCenterId);
			if (result != null)
			{
				ExternalTransaction extTran = (ExternalTransaction)result;
				ARPayment payment = (ARPayment)result;
				ret = new Tuple<ExternalTransaction, ARPayment>(extTran, payment);
			}
			return ret;
		}

		public ExternalTransaction FindCapturedExternalTransaction(string procCenterId, string tranNbr)
		{
			if (string.IsNullOrEmpty(procCenterId))
			{
				throw new ArgumentException(nameof(procCenterId));
			}

			if (string.IsNullOrEmpty(tranNbr))
			{
				throw new ArgumentException(nameof(tranNbr));
			}

			var query = new PXSelect<ExternalTransaction,
				Where<ExternalTransaction.tranNumber, Equal<Required<ExternalTransaction.tranNumber>>,
					And<ExternalTransaction.processingCenterID, Equal<Required<ExternalTransaction.processingCenterID>>,
					And<Where<ExternalTransaction.procStatus, Equal<ExtTransactionProcStatusCode.captureSuccess>, 
						Or<ExternalTransaction.procStatus, Equal<ExtTransactionProcStatusCode.captureHeldForReview>>>>>>,
				OrderBy<Desc<ExternalTransaction.transactionID>>>(graph);
			return query.SelectSingle(tranNbr, procCenterId);
		}

		public IEnumerable<ExternalTransaction> GetExternalTransactionsByPayLinkID(int? payLinkId)
		{
			var res = PXSelect<ExternalTransaction,
				Where<ExternalTransaction.payLinkID, Equal<Required<ExternalTransaction.payLinkID>>>>
					.Select(graph, payLinkId).RowCast<ExternalTransaction>();
			return res;
		}

		public IEnumerable<ExternalTransaction> GetExternalTransaction(string cCProcessingCenterID, string tranNumber)
		{
			return PXSelectJoin<ExternalTransaction,
				LeftJoin<CustomerPaymentMethod, On<CustomerPaymentMethod.pMInstanceID, Equal<ExternalTransaction.pMInstanceID>>>,
				Where<ExternalTransaction.tranNumber, Equal<Required<ExternalTransaction.tranNumber>>,
					And<Where<ExternalTransaction.processingCenterID, Equal<Required<ExternalTransaction.processingCenterID>>,
						Or<CustomerPaymentMethod.cCProcessingCenterID, Equal<Required<CustomerPaymentMethod.cCProcessingCenterID>>>>>>>
				.Select(graph, tranNumber, cCProcessingCenterID, cCProcessingCenterID)
				.RowCast<ExternalTransaction>();
		}

		public ExternalTransaction GetExternalTransactionByNoteID(Guid? noteID)
		{
			return PXSelect<ExternalTransaction,
				Where<ExternalTransaction.noteID, Equal<Required<ExternalTransaction.noteID>>>>
				.Select(graph, noteID);
		}

		public ExternalTransaction InsertExternalTransaction(ExternalTransaction extTran)
		{
			return graph.Caches[typeof(ExternalTransaction)].Insert(extTran) as ExternalTransaction;
		}

		public ExternalTransaction UpdateExternalTransaction(ExternalTransaction extTran)
		{
			return graph.Caches[typeof(ExternalTransaction)].Update(extTran) as ExternalTransaction;
		}
	}
}
