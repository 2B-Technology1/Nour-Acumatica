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
using PX.Objects.CA;

namespace PX.Commerce.Shopify.ShopifyPayments
{
    public static class ExtensionHelper
    {
        public static bool IsShopifyPaymentsPlugin(PXGraph graph, string processingCenterID)
        {
            CCProcessingCenter processingCenter = CCProcessingCenter.PK.Find(graph, processingCenterID);
            return IsShopifyPaymentsPlugin(processingCenter);
        }

        public static bool IsShopifyPaymentsPlugin(CCProcessingCenter row) => IsShopifyPaymentsPlugin(row?.ProcessingTypeName);

        public static bool IsShopifyPaymentsPlugin(string processingTypeName) => (processingTypeName == typeof(ShopifyPaymentsProcessingPlugin).FullName);
    }
}
