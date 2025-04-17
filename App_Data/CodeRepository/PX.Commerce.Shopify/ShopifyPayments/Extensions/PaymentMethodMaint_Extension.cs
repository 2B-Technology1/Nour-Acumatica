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

namespace PX.Commerce.Shopify.ShopifyPayments.Extensions
{
    public class PaymentMethodMaint_Extension : PXGraphExtension<PaymentMethodMaint>
    {
        #region Event Handlers
        public virtual void _(Events.FieldDefaulting<CCProcessingCenterPmntMethod, CCProcessingCenterPmntMethod.fundHoldPeriod> e)
        {
            if (e.Row == null || e.Row.ProcessingCenterID == null)
            {
                return;
            }

            CCProcessingCenter ccProcessingCenter = (CCProcessingCenter)PXSelectorAttribute.Select<CCProcessingCenterPmntMethod.processingCenterID>(e.Cache, e.Row);

            if (ExtensionHelper.IsShopifyPaymentsPlugin(ccProcessingCenter))
            {
                e.NewValue = ShopifyPluginHelper.AuthorizationValidPeriodDays;
                e.Cancel = true;
            }
        }

        public virtual void _(Events.FieldUpdated<CCProcessingCenterPmntMethod, CCProcessingCenterPmntMethod.processingCenterID> e)
        {
            if (e.Row == null || e.Row.ProcessingCenterID == null)
            {
                return;
            }

            CCProcessingCenter ccProcessingCenter = (CCProcessingCenter)PXSelectorAttribute.Select<CCProcessingCenterPmntMethod.processingCenterID>(e.Cache, e.Row);

            if (ExtensionHelper.IsShopifyPaymentsPlugin(ccProcessingCenter))
            {
                e.Cache.SetDefaultExt<CCProcessingCenterPmntMethod.fundHoldPeriod>(e.Row);
                e.Cache.SetValuePending<CCProcessingCenterPmntMethod.fundHoldPeriod>(e.Row, PXCache.NotSetValue);
            }
        }

        public virtual void _(Events.RowUpdated<CCProcessingCenterPmntMethod> e)
        {
            if (e.Row == null || e.Row.ProcessingCenterID == null)
            {
                return;
            }

            CCProcessingCenter ccProcessingCenter = (CCProcessingCenter)PXSelectorAttribute.Select<CCProcessingCenterPmntMethod.processingCenterID>(e.Cache, e.Row);

            if (ExtensionHelper.IsShopifyPaymentsPlugin(ccProcessingCenter))
            {
                e.Cache.SetDefaultExt<CCProcessingCenterPmntMethod.fundHoldPeriod>(e.Row);
            }
        }

        public virtual void _(Events.RowSelected<CCProcessingCenterPmntMethod> e)
        {
            if (e.Row == null)
            {
                return;
            }

            bool enableFundHoldPeriod = true;

            if (e.Row.ProcessingCenterID != null)
            {
                CCProcessingCenter ccProcessingCenter = (CCProcessingCenter)PXSelectorAttribute.Select<CCProcessingCenterPmntMethod.processingCenterID>(e.Cache, e.Row);

                enableFundHoldPeriod = ExtensionHelper.IsShopifyPaymentsPlugin(ccProcessingCenter) == false;
            }

            PXUIFieldAttribute.SetEnabled<CCProcessingCenterPmntMethod.fundHoldPeriod>(e.Cache, e.Row, enableFundHoldPeriod);
        }
        #endregion
    }
}
