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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

namespace PX.Objects.CA.Descriptor
{
	public class CAOptimizeLoadDateAttribute : PXFormulaAttribute
	{
		private int _transactionsLimit;

		public int TransactionsLimit
		{
			get => _transactionsLimit;
			set
			{
				_transactionsLimit = value;
				((OptimizeLoadDateEvaluator)_Formula).TransactionsLimit = value;
			}
		}

		public CAOptimizeLoadDateAttribute() : base(typeof(OptimizeLoadDateEvaluator))
		{
		}

		public class OptimizeLoadDateEvaluator : BqlFormulaEvaluator<CARecon.cashAccountID>
		{
			public int TransactionsLimit;

			public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> pars)
			{
				try
				{
					return GetLoadDocumentsTill(cache, (CARecon)item);
				}
				catch (PXException ex)
				{
					throw new PXSetPropertyException(ex.Message, PXErrorLevel.Error);
				}
				catch
				{
					return null;
				}
			}

			public virtual DateTime? GetLoadDocumentsTill(PXCache cache, CARecon row)
			{
				var graph = cache.Graph;

				if (row.Reconciled == true
					|| row.ShowBatchPayments == null
					|| graph == null) return null;

				IEnumerable<DateTime?> datesToReconcile;

				if (row.ShowBatchPayments == true)
				{
					var tranDatesToReconcile = GetNotBatchedTransactions(graph, row).Select(t => ((CATran)t).TranDate);

					var batchDatesToReconcile = GetBatchedTransactions(graph, row).Select(t => ((CABatch)t).TranDate);

					datesToReconcile = tranDatesToReconcile.Concat(batchDatesToReconcile).OrderBy(_=>_);
				}
				else
				{
					datesToReconcile = GetTransactions(graph, row).Select(t => ((CATran)t).TranDate);
				}

				datesToReconcile = datesToReconcile.Take(TransactionsLimit + 1).ToList();

				var edgeDate = datesToReconcile.LastOrDefault();
				if (datesToReconcile.Count() <= TransactionsLimit)
				{
					return row.ReconDate;
				}

				DateTime? recommendedDate;
				var firstDate = datesToReconcile.FirstOrDefault();
				var limitDate = datesToReconcile.ElementAt(datesToReconcile.Count() - 2);

				if (edgeDate != limitDate)
				{
					recommendedDate = limitDate;
				}
				else if (edgeDate != firstDate)
				{
					recommendedDate = edgeDate?.AddDays(-1);
				}
				else
				{
					recommendedDate = firstDate;
				}

				return recommendedDate > row.ReconDate ? row.ReconDate : recommendedDate;
			}

			private IEnumerable<PXResult<CATran>> GetTransactions(PXGraph graph, CARecon row)
			{
				return SelectFrom<CATran>
						.Where<CATran.cashAccountID.IsEqual<@P.AsInt>.
							And<CATran.reconNbr.IsEqual<@P.AsString>.
								Or<@P.AsBool.IsEqual<False>>.
								And<CATran.reconNbr.IsNull>>>
						.OrderBy<CATran.tranDate.Asc>.View.ReadOnly
						.SelectWindowed(graph, 0, TransactionsLimit + 1, row.CashAccountID, row.ReconNbr, row.Reconciled)
						.AsEnumerable();
			}

			private IEnumerable<PXResult<CABatch>> GetBatchedTransactions(PXGraph graph, CARecon row)
			{
				return SelectFrom<CABatch>
						.InnerJoin<CABatchDetail>.On<CABatch.batchNbr.IsEqual<CABatchDetail.batchNbr>>
						.InnerJoin<CATran>
						.On<CABatchDetail.origModule.IsEqual<CATran.origModule>
							.And<CABatchDetail.origDocType.IsEqual<CATran.origTranType>>
							.And<CABatchDetail.origRefNbr.IsEqual<CATran.origRefNbr>>
							.And<CATran.isPaymentChargeTran.IsEqual<False>>>
						.Where<CABatch.cashAccountID.IsEqual<@P.AsInt>
							.And<CABatch.reconNbr.IsEqual<@P.AsString>.
								Or<@P.AsBool.IsEqual<False>>.
								And<CABatch.reconNbr.IsNull>>>
						.OrderBy<CABatch.tranDate.Asc>.View.ReadOnly
						.SelectWindowed(graph, 0, TransactionsLimit + 1, row.CashAccountID, row.ReconNbr, row.Reconciled)
						.AsEnumerable();
			}

			private IEnumerable<PXResult<CATran>> GetNotBatchedTransactions(PXGraph graph, CARecon row)
			{
				return SelectFrom<CATran>
						.LeftJoin<CABatchDetail>
						.On<CABatchDetail.origModule.IsEqual<CATran.origModule>
							.And<CABatchDetail.origDocType.IsEqual<CATran.origTranType>>
							.And<CABatchDetail.origRefNbr.IsEqual<CATran.origRefNbr>>
							.And<CATran.isPaymentChargeTran.IsEqual<False>>>
						.Where<CABatchDetail.batchNbr.IsNull.
							And<CATran.cashAccountID.IsEqual<@P.AsInt>.
							And<CATran.reconNbr.IsEqual<@P.AsString>.
								Or<@P.AsBool.IsEqual<False>>.
								And<CATran.reconNbr.IsNull>>>>
						.OrderBy<CATran.tranDate.Asc>.View.ReadOnly
						.SelectWindowed(graph, 0, TransactionsLimit + 1, row.CashAccountID, row.ReconNbr, row.Reconciled)
						.AsEnumerable();
			}
		}
	}
}
