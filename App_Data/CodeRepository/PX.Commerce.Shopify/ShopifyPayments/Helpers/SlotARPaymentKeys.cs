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

using PX.Common;

namespace PX.Commerce.Shopify.ShopifyPayments.Extensions
{
    public class SlotARPaymentKeys
    {
        protected string _DocType;
        protected string _DocRefNbr;

        public SlotARPaymentKeys(string docType, string docRefNbr)
        {
            _DocType = docType;
            _DocRefNbr = docRefNbr;

            PXContext.SetSlot<SlotARPaymentKeys>(this);
        }

        public static void SaveKeys(string docType, string docRefNbr)
        {
            new SlotARPaymentKeys(docType, docRefNbr);
        }

        public static void GetKeys(out string docType, out string docRefNbr, bool clearSlot)
        {
            SlotARPaymentKeys slot = PXContext.GetSlot<SlotARPaymentKeys>();

            if (slot == null)
            {
                docType = null;
                docRefNbr = null;

                return;
            }

            docType = slot._DocType;
            docRefNbr = slot._DocRefNbr;

            if (clearSlot == true
                    && (docType != null || docRefNbr != null))
            {
                SaveKeys(null, null);
            }
        }
    }
}
