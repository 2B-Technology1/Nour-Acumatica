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
using System.Diagnostics;
using PX.Data;

namespace PX.Objects.AM
{
    /// <summary>
    /// Production Operation Totals Container
    /// </summary>
    [DebuggerDisplay("Key = {Key}")]
    public sealed class MoveOperationQtyTotal
    {
        private decimal _currentMoveBaseQty;
        private decimal _transactionTotalMoveBaseQty;
        private bool _excludeFixedLabor;
        private decimal _transactionTotalScrapBaseQty;

        public string Key { get; set; }

        public AMProdOper ProdOper { get; set; }

        public AMWC WorkCenter { get; set; }

		/// <summary>
		/// Control point before the current operation
		/// </summary>
		public int? PriorControlPointOperationID { get; set; }

		/// <summary>
		/// Control point after the current operation
		/// </summary>
		public int? NextControlPointOperationID { get; set; }

		internal bool HasLaborBeforeTransaction { get; set; }

        /// <summary>
        /// Calculated total base quantity
        /// (NOT USED)
        /// </summary>
        public decimal ProjectedTotalBaseQty { get; set; }

        /// <summary>
        /// Total transaction base quantity (indirect quantity from previous operations)
        /// Used to drive supporting transactions such as backflush material, overhead, etc.
        /// The value here might be less on previous operations if previous operations have their own transactions previously released.
        /// </summary>
        public decimal TransactionTotalMoveBaseQty
        {
            get { return _transactionTotalMoveBaseQty; }
            set
            {
                _transactionTotalMoveBaseQty = value;
                SetCurrentMoveLaborTime();
            }
        }

        /// <summary>
        /// Total transaction scrapped base quantity 
        /// </summary>
        public decimal TransactionTotalScrapBaseQty
        {
            get { return _transactionTotalScrapBaseQty; }
            set
            {
                _transactionTotalScrapBaseQty = value;
            }
        }

        /// <summary>
        /// Total BEFORE processing the current move transaction
        /// (NOT USED)
        /// </summary>
        public decimal PreTransactionTotalBaseQty { get; set; }

        /// <summary>
        /// Previously processed base quantity
        /// (updated as the transaction is processed)
        /// </summary>
        public decimal PreviousMoveTotalBaseQty { get; set; }

        /// <summary>
        /// Currently move base qty
        /// (updated as the transaction is processed)
        /// </summary>
        public decimal CurrentMoveBaseQty
        {
            get { return _currentMoveBaseQty; }
            set
            {
                _currentMoveBaseQty = value;
                SetCurrentMoveLaborTime();
            }
        }

        public int CurrentMoveVarLaborTime { get; set; }
        public int CurrentMoveFixLaborTime { get; set; }


        public void ExcludeFixedLabor(bool exclude)
        {
            _excludeFixedLabor = exclude;
            SetCurrentMoveLaborTime();
        }

        private void SetCurrentMoveLaborTime()
        {
            if (ProdOper == null || !ProdOper.BFlush.GetValueOrDefault() || CurrentMoveBaseQty == 0)
            {
                return;
            }

            CurrentMoveFixLaborTime = 0;
            if (!_excludeFixedLabor)
            {
                var setupTimeQty = CurrentMoveBaseQty < 0
                    ? Math.Min(CurrentMoveBaseQty, TransactionTotalMoveBaseQty) * -1
                    : Math.Max(CurrentMoveBaseQty, TransactionTotalMoveBaseQty);

                if (setupTimeQty == 0)
                {
                    setupTimeQty = 1;
                }

                CurrentMoveFixLaborTime = (ProdOper.SetupTime.GetValueOrDefault() * (CurrentMoveBaseQty / setupTimeQty)).ToCeilingInt();
            }

            var rate = ProdOper.RunUnits.GetValueOrDefault() == 0
                ? 0m
                : ProdOper.RunUnitTime.GetValueOrDefault() / ProdOper.RunUnits.GetValueOrDefault();

			var laborTimeValuesymbol = CurrentMoveBaseQty < 0 ? -1 : 1;
			CurrentMoveVarLaborTime = Convert.ToInt32(Math.Ceiling(rate * Math.Abs(CurrentMoveBaseQty)) * laborTimeValuesymbol);

		}

        public MoveOperationQtyTotal(MoveOperationQtyTotal moveOperationQtyTotal, decimal previousMoveTotalBaseQty, decimal currentMoveBaseQty)
        {
            ProdOper = moveOperationQtyTotal.ProdOper ?? throw new PXArgumentException(nameof(moveOperationQtyTotal.ProdOper));
            Key = ProdOper == null ? "-1" : ProdOper.JoinKeys();
            WorkCenter = moveOperationQtyTotal.WorkCenter ?? throw new PXArgumentException(nameof(moveOperationQtyTotal.WorkCenter));
            HasLaborBeforeTransaction = ProdOper.ActualLabor.GetValueOrDefault() != 0 || ProdOper.ActualLaborTime.GetValueOrDefault() != 0;
            ProjectedTotalBaseQty = moveOperationQtyTotal.ProjectedTotalBaseQty;
            PreTransactionTotalBaseQty = moveOperationQtyTotal.PreTransactionTotalBaseQty;
            PreviousMoveTotalBaseQty = previousMoveTotalBaseQty;
            CurrentMoveBaseQty = currentMoveBaseQty;
        }

        public MoveOperationQtyTotal(AMProdOper amProdOper, AMWC workCenter)
        {
            ProdOper = amProdOper ?? throw new PXArgumentException(nameof(amProdOper));
            Key = ProdOper == null ? "-1" : ProdOper.JoinKeys();
            WorkCenter = workCenter ?? throw new PXArgumentException(nameof(workCenter));
            HasLaborBeforeTransaction = ProdOper.ActualLabor.GetValueOrDefault() != 0 || ProdOper.ActualLaborTime.GetValueOrDefault() != 0;
        }
    }
}
