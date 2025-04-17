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

using System.Collections.Generic;
using System.Linq;
using PX.Data;

namespace PX.Objects.AM
{
    /// <summary>
    /// Container for a transaction and related operations as they would exist for the currently processing transaction
    /// </summary>
    public sealed class MoveOperationQtyTotals
    {
        public readonly string Key;
        public AMMTran Transaction;
        public Dictionary<string, MoveOperationQtyTotal> OperationTotals { get; }
        private readonly Dictionary<string, decimal> _addtionalOperationWip;
		public bool HasControlPoints;

        /// <summary>
        /// OperationTotals in the form of a List
        /// </summary>
        public List<MoveOperationQtyTotal> OperationTotalsList => OperationTotals?.Values.ToList();

        public List<AMProdOper> OperationsList => OperationTotals.Values.Select(totalsValue => totalsValue.ProdOper).ToList();

        public MoveOperationQtyTotals(AMMTran transaction)
        {
            Transaction = transaction ?? throw new PXArgumentException(nameof(transaction));
            OperationTotals = new Dictionary<string, MoveOperationQtyTotal>();
            Key = transaction.JoinDacKeys();
            _addtionalOperationWip = new Dictionary<string, decimal>();
        }

        public decimal GetAdditionalOperationWipCost(AMProdOper prodOper)
        {
            _addtionalOperationWip.TryGetValue(prodOper.JoinKeys(), out var wip);
            return wip;
        }

        public void SetAdditionalOperationWipCost(AMProdOper prodOper, decimal wip)
        {
            _addtionalOperationWip[prodOper.JoinKeys()] = wip;
        }

        public void SetTransactionTotalMoveBaseQty(AMProdOper prodOper, decimal qty)
        {
            var moveOperQtyTotal = GetOperationTotal(prodOper);
            if(moveOperQtyTotal == null)
            {
                return;
            }
            moveOperQtyTotal.TransactionTotalMoveBaseQty = qty;
            UpdateOperation(moveOperQtyTotal);
        }

        public MoveOperationQtyTotal GetLastOperation(AMProdItem prodItem)
        {
            return GetOperationTotal(prodItem?.OrderType, prodItem?.ProdOrdID, prodItem?.LastOperationID);
        }

        public MoveOperationQtyTotal AddUpdateOperation(AMProdOper amProdOper, AMWC workCenter)
        {
            return UpdateOperation(new MoveOperationQtyTotal(amProdOper, workCenter));
        }

        public MoveOperationQtyTotal AddUpdateOperation(MoveOperationQtyTotal projectedOperationQtyTotal, decimal previousMoveTotalBaseQty, decimal currentMoveBaseQty)
        {
            return UpdateOperation(new MoveOperationQtyTotal(projectedOperationQtyTotal, previousMoveTotalBaseQty, currentMoveBaseQty));
        }

        public MoveOperationQtyTotal GetSetOperationTotal(PXResult<AMProdOper, AMWC> operResult)
        {
            return GetOperationTotal((AMProdOper)operResult) ?? UpdateOperation(new MoveOperationQtyTotal(operResult, operResult));
        }

        public MoveOperationQtyTotal GetTransactionOperationTotal()
        {
            return GetOperationTotal(Transaction?.OrderType, Transaction?.ProdOrdID, Transaction?.OperationID);
        }

        public MoveOperationQtyTotal GetOperationTotal(IProdOper prodOper)
        {
            return GetOperationTotal(prodOper?.OrderType, prodOper?.ProdOrdID, prodOper?.OperationID);
        }

        public MoveOperationQtyTotal GetOperationTotal(string orderType, string prodOrdId, int? operationId)
        {
            if (string.IsNullOrWhiteSpace(orderType) || string.IsNullOrWhiteSpace(prodOrdId) ||
                operationId == null)
            {
                return null;
            }

            var key = GetOperKey(orderType, prodOrdId, operationId);
            return OperationTotals.ContainsKey(key) ? OperationTotals[key] : null;
        }

        private string GetOperKey(string orderType, string prodOrdId, int? operationId)
        {
            return string.Join("~", orderType.TrimIfNotNullEmpty(), prodOrdId.TrimIfNotNullEmpty(), operationId.GetValueOrDefault());
        }

        public MoveOperationQtyTotal UpdateOperation(MoveOperationQtyTotal moveOperationQtyTotal)
        {
            OperationTotals[moveOperationQtyTotal.Key] = moveOperationQtyTotal;
            return moveOperationQtyTotal;
        }
    }
}
