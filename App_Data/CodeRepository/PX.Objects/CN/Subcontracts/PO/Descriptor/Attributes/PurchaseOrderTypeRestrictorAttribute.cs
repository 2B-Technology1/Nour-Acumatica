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
using PX.Objects.CN.Subcontracts.PO.DAC;
using PX.Objects.PO;

namespace PX.Objects.CN.Subcontracts.PO.Descriptor.Attributes
{
    public class PurchaseOrderTypeRestrictorAttribute : PXRestrictorAttribute
    {
        public PurchaseOrderTypeRestrictorAttribute()
            : base(typeof(
                    Where<POOrder.orderType, Equal<Current<PurchaseOrderTypeFilter.type1>>,
                        Or<POOrder.orderType, Equal<Current<PurchaseOrderTypeFilter.type2>>,
                        Or<POOrder.orderType, Equal<Current<PurchaseOrderTypeFilter.type3>>,
                        Or<POOrder.orderType, Equal<Current<PurchaseOrderTypeFilter.type4>>,
                        Or<POOrder.orderType, Equal<Current<PurchaseOrderTypeFilter.type5>>>>>>>),
                Messages.OnlyPurchaseOrdersAreAllowedMessage)
        {
        }
    }
}