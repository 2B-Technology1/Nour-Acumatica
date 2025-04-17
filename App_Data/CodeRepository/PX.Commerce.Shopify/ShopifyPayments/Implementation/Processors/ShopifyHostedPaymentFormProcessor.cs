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
using PX.CCProcessingBase.Interfaces.V2;
using PX.Commerce.Shopify.API.REST;
using PX.Data;
using ProcessingInput = PX.CCProcessingBase.Interfaces.V2.ProcessingInput;

namespace PX.Commerce.Shopify.ShopifyPayments
{
	public class ShopifyHostedPaymentFormProcessor : ShopifyPaymentsDataProvider, ICCHostedPaymentFormProcessor
	{
		public ShopifyHostedPaymentFormProcessor(IShopifyRestClient restClient) : base(restClient)
		{
		}

		public HostedFormData GetDataForPaymentForm(ProcessingInput inputData)
        {
            throw new PXException(ShopifyPluginMessages.TheMethodXIsNotImplementedInTheX, nameof(ICCHostedPaymentFormProcessor) + "." + nameof(GetDataForPaymentForm), ShopifyPluginMessages.APIPluginDisplayName);
        }
    }
}
