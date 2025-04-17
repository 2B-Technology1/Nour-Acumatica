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
using PX.Objects.EP;
using PX.Objects.PO;

namespace PX.Objects.CN.Subcontracts.PO.Extensions
{
    public static class EpOwnedExtensions
    {
        public static POOrder GetSubcontractEntity(this EPApprovalProcess.EPOwned epOwned, PXGraph graph)
        {
            if (epOwned == null || epOwned.EntityType != typeof(POOrder).FullName)
            {
                return null;
            }

            using (new PXReadBranchRestrictedScope())
            {
                var purchaseOrder = new PXSelect<POOrder,
                Where<POOrder.noteID, Equal<Required<POOrder.noteID>>>>(graph).SelectSingle(epOwned.RefNoteID);
                return purchaseOrder.OrderType == POOrderType.RegularSubcontract
                    ? purchaseOrder
                    : null;
            }
        }
    }
}