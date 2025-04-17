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
using PX.Data;
using PX.Objects.Common.GraphExtensions.Abstract.DAC;

namespace PX.Objects.Common.GraphExtensions.Abstract.Mapping
{
    public class InvoiceTranMapping : IBqlMapping
    {
        /// <exclude />
        public Type Extension => typeof(InvoiceTran);
        /// <exclude />
        protected Type _table;
        /// <exclude />
        public Type Table => _table;

        public InvoiceTranMapping(Type table) { _table = table; }

        public Type BranchID = typeof(InvoiceTran.branchID);

        public Type TranDate = typeof(InvoiceTran.tranDate);

        public Type FinPeriodID = typeof(InvoiceTran.finPeriodID);

        public Type TranPeriodID = typeof(InvoiceTran.tranPeriodID);

        public Type LineNbr = typeof(InvoiceTran.lineNbr);

        public Type TaxCategoryID = typeof(InvoiceTran.taxCategoryID);

        public Type InventoryID = typeof(InvoiceTran.inventoryID);

        public Type TranDesc = typeof(InvoiceTran.tranDesc);

        public Type ManualPrice = typeof(InvoiceTran.manualPrice);

        public Type CuryUnitCost = typeof(InvoiceTran.curyUnitCost);

        public Type Qty = typeof(InvoiceTran.qty);

        public Type UOM = typeof(InvoiceTran.uOM);

        public Type NonBillable = typeof(InvoiceTran.nonBillable);

        public Type Date = typeof(InvoiceTran.date);

        public Type ProjectID = typeof(InvoiceTran.projectID);

        public Type TaskID = typeof(InvoiceTran.taskID);

        public Type CostCodeID = typeof(InvoiceTran.costCodeID);

        public Type AccountID = typeof(InvoiceTran.accountID);

        public Type SubID = typeof(InvoiceTran.subID);

        public Type CuryLineAmt = typeof(InvoiceTran.curyLineAmt);

        public Type CuryTaxAmt = typeof(InvoiceTran.curyTaxAmt);

        public Type CuryTaxableAmt = typeof(InvoiceTran.curyTaxableAmt);

        public Type CuryTranAmt = typeof(InvoiceTran.curyTranAmt);

        public Type TaxableAmt = typeof(InvoiceTran.taxableAmt);

        public Type TranAmt = typeof(InvoiceTran.tranAmt);
    }
}
