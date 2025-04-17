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

using PX.CCProcessingBase.Attributes;
using PX.CCProcessingBase.Interfaces.V2;
using PX.Commerce.Shopify.API.REST;
using System.Collections.Generic;

namespace PX.Commerce.Shopify.ShopifyPayments
{
    [PXDisplayTypeName(ShopifyPluginMessages.APIPluginDisplayName)]
    public class ShopifyPaymentsProcessingPlugin : ICCProcessingPlugin
    {
		public class Const_PluginName : PX.Data.BQL.BqlString.Constant<Const_PluginName>
		{
			public Const_PluginName() : base(typeof(ShopifyPaymentsProcessingPlugin).FullName) { }
		}

		public IEnumerable<SettingsDetail> ExportSettings()
        {
            return ShopifyPluginHelper.GetDefaultSettings();
        }

		public string ValidateSettings(SettingsValue setting)
		{
			return ShopifyValidator.Validate(setting);
		}

		public void TestCredentials(IEnumerable<SettingsValue> settingValues)
        {
			var restClient = ShopifyPaymentsDataProvider.GetRestClient(settingValues);
			var authenticateTestProcessor = new ShopifyAuthenticateTestProcessor(restClient);
			authenticateTestProcessor.TestCredentials();
		}

		public T CreateProcessor<T>(IEnumerable<SettingsValue> settingValues)
			where T : class
		{
			if (typeof(T) == typeof(ICCProfileProcessor))
			{
				return null;
				//return new AuthnetProfileProcessor(settingValues) as T;
			}
			if (typeof(T) == typeof(ICCHostedFormProcessor))
			{
				return null;
				//return new AuthnetHostedFormProcessor(settingValues) as T;
			}
			if (typeof(T) == typeof(ICCHostedPaymentFormProcessor))
			{
				return new ShopifyHostedPaymentFormProcessor(
					ShopifyPaymentsDataProvider.GetRestClient(settingValues)) as T;
			}
			if (typeof(T) == typeof(ICCTransactionProcessor))
			{
				return new ShopifyTransactionProcessor(
					ShopifyPaymentsDataProvider.GetRestClient(settingValues)) as T;
			}
			if (typeof(T) == typeof(ICCTransactionGetter))
			{
				return new ShopifyTransactionGetter(
					ShopifyPaymentsDataProvider.GetRestClient(settingValues)) as T;
			}
			if (typeof(T) == typeof(ICCProfileCreator))
			{
				return new ShopifyProfileCreator(
					ShopifyPaymentsDataProvider.GetRestClient(settingValues)) as T;
			}
			if (typeof(T) == typeof(ICCWebhookResolver))
			{
				return null;
				//return new AuthnetWebhookResolver() as T;
			}
			if (typeof(T) == typeof(ICCWebhookProcessor))
			{
				return null;
				//return new AuthnetWebhookProcessor(settingValues) as T;
			}
			if (typeof(T) == typeof(ICCTranStatusGetter))
			{
				return null;
				//return new AuthnetTranStatusGetter() as T;
			}
			if (typeof(T) == typeof(ICCHostedPaymentFormResponseParser))
			{
				return new ShopifyHostedPaymentFormResponseParser() as T;
			}
			return null;
		}		
	}
}
