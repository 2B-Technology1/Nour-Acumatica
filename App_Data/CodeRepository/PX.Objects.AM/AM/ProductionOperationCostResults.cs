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
using System.Diagnostics;
using PX.Objects.AM.Attributes;
using PX.Data;

namespace PX.Objects.AM
{
    public sealed class ProductionOperationCostResults
    {
        public readonly AMProdItem ProdItem;
        public readonly string CostType;
        public decimal UnitAmount;
        public decimal TotalAmount;

        private List<OperationCostResult> _operationResults;
        public List<OperationCostResult> OperationResults => _operationResults;

        public ProductionOperationCostResults(AMProdItem amProdItem)
        {
            if (amProdItem == null
                || string.IsNullOrWhiteSpace(amProdItem.OrderType)
                || string.IsNullOrWhiteSpace(amProdItem.ProdOrdID))
            {
                throw new PXArgumentException(nameof(amProdItem));
            }

            ProdItem = amProdItem;
            CostType = AMTranType.OperWIPComplete;

            _operationResults = new List<OperationCostResult>();
        }

        public void AddOperationCost(AMProdOper prodOper, decimal unitAmount, decimal totalAmount)
        {
            AddOperationCost(prodOper, unitAmount, totalAmount, true);
        }

        public void AddOperationCost(AMProdOper prodOper, decimal unitAmount, decimal totalAmount, decimal processQty)
        {
            AddOperationCost(prodOper, unitAmount, totalAmount, processQty, true);
        }

        public void AddOperationCost(AMProdOper prodOper, decimal unitAmount, decimal totalAmount, bool includeInTotal)
        {
            AddOperationCost(prodOper, unitAmount, totalAmount, 0m, includeInTotal);
        }

        public void AddOperationCost(AMProdOper prodOper, decimal unitAmount, decimal totalAmount, decimal processQty, bool includeInTotal)
        {
            AddOperationCost(new OperationCostResult(prodOper, unitAmount, totalAmount, processQty, CostType), includeInTotal);
        }

        public void AddOperationCost(OperationCostResult operationCostResult, bool includeInTotal)
        {
            _operationResults.Add(operationCostResult);
            if (includeInTotal)
            {
                UnitAmount += operationCostResult.UnitAmount;
                TotalAmount += operationCostResult.TotalAmount;
            }
        }

        [DebuggerDisplay("[{ProdOper.OrderType}:{ProdOper.ProdOrdID}:{ProdOper.OperationID}] UnitAmount = {UnitAmount}; TotalAmount = {TotalAmount}")]
        public struct OperationCostResult
        {
            public AMProdOper ProdOper;
            public decimal UnitAmount;
            public decimal TotalAmount;
            public string CostType;
            public decimal ProcessQty;

            public OperationCostResult(AMProdOper prodOper, decimal unitAmount, decimal totalAmount)
            {
                if (string.IsNullOrWhiteSpace(prodOper?.OrderType) || string.IsNullOrWhiteSpace(prodOper.ProdOrdID) || prodOper.OperationID == null)
                {
                    throw new PXArgumentException(nameof(prodOper));
                }

                ProdOper = prodOper;
                UnitAmount = unitAmount;
                TotalAmount = totalAmount;
                CostType = AMTranType.OperWIPComplete;
                ProcessQty = 0m;
            }

            public OperationCostResult(AMProdOper prodOper, decimal unitAmount, decimal totalAmount, decimal processQty)
            {
                if (string.IsNullOrWhiteSpace(prodOper?.OrderType) || string.IsNullOrWhiteSpace(prodOper.ProdOrdID) || prodOper.OperationID == null)
                {
                    throw new PXArgumentException(nameof(prodOper));
                }

                ProdOper = prodOper;
                UnitAmount = unitAmount;
                TotalAmount = totalAmount;
                CostType = AMTranType.OperWIPComplete;
                ProcessQty = processQty;
            }

            public OperationCostResult(AMProdOper prodOper, decimal unitAmount, decimal totalAmount, decimal processQty, string costType)
            {
                if (string.IsNullOrWhiteSpace(prodOper?.OrderType) || string.IsNullOrWhiteSpace(prodOper.ProdOrdID) || prodOper.OperationID == null)
                {
                    throw new PXArgumentException(nameof(prodOper));
                }

                ProdOper = prodOper;
                UnitAmount = unitAmount;
                TotalAmount = totalAmount;
                CostType = costType;
                ProcessQty = processQty;
            }
        }
    }
}