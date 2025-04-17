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
using PX.Objects.AP;
using PX.Objects.PO;

namespace PX.Objects.CN.Subcontracts.AP.Descriptor
{
    public sealed class SubcontractNumberSelectorAttribute : PXSelectorAttribute
    {
        private static readonly Type[] Fields =
        {
            typeof(POOrder.orderNbr),
            typeof(POOrder.orderDate),
            typeof(POOrder.vendorID),
            typeof(POOrder.vendorID_Vendor_acctName),
            typeof(POOrder.vendorLocationID),
            typeof(POOrder.curyID),
            typeof(POOrder.curyOrderTotal)
        };

        public SubcontractNumberSelectorAttribute()
           : base(typeof(Search5<POOrder.orderNbr,
               InnerJoin<POLine, On<POLine.orderType, Equal<POOrder.orderType>,
                   And<POLine.orderNbr, Equal<POOrder.orderNbr>,
                   And<POLine.pOAccrualType, Equal<POAccrualType.order>,
                   And<POLine.cancelled, NotEqual<True>,
                   And<POLine.closed, NotEqual<True>>>>>>,
               LeftJoin<APTran, On<APTran.pOOrderType, Equal<POLine.orderType>,
                   And<APTran.pONbr, Equal<POLine.orderNbr>,
                   And<APTran.pOLineNbr, Equal<POLine.lineNbr>,
                   And<APTran.receiptNbr, IsNull,
                   And<APTran.receiptLineNbr, IsNull,
                   And<APTran.released, Equal<False>>>>>>>>>,
               Where<POOrder.orderType, Equal<POOrderType.regularSubcontract>,
                   And<POOrder.curyID, Equal<Current<APInvoice.curyID>>,
                   And<POOrder.status, In3<POOrderStatus.open, POOrderStatus.completed>,
                   And<Where<APTran.refNbr, IsNull, Or<APTran.refNbr, Equal<Current<APInvoice.refNbr>>>>>>>>,
               Aggregate<GroupBy<POOrder.orderType, GroupBy<POOrder.orderNbr>>>>), Fields)
        {
            Filterable = true;
        }
    }
}
