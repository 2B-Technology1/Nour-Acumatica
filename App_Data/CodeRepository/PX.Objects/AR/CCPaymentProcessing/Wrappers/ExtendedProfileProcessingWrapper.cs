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
using System;
using System.Collections.Generic;
using PX.Objects.AR.CCPaymentProcessing.Common;
using V1 = PX.CCProcessingBase;
using V2 = PX.CCProcessingBase.Interfaces.V2;
using System.Web;
using PX.Objects.AR.CCPaymentProcessing.Repositories;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
namespace PX.Objects.AR.CCPaymentProcessing.Wrappers
{
	public class ExtendedProfileProcessingWrapper
	{
		public static IExtendedProfileProcessingWrapper GetExtendedProfileProcessingWrapper(object pluginObject, ICardProcessingReadersProvider provider)
		{
			IExtendedProfileProcessingWrapper wrapper = GetExtendedProfileProcessingWrapper(pluginObject);
			ISetCardProcessingReadersProvider setProviderBehaviour = wrapper as ISetCardProcessingReadersProvider;
			if(setProviderBehaviour == null)
			{
				throw new PXException(NotLocalizableMessages.ERR_CardProcessingReadersProviderSetting);
			}
			setProviderBehaviour.SetProvider(provider);
			return wrapper;
		}

		private static IExtendedProfileProcessingWrapper GetExtendedProfileProcessingWrapper(object pluginObject)
		{
			CCProcessingHelper.CheckHttpsConnection();
			bool isV1Interface = CCProcessingHelper.IsV1ProcessingInterface(pluginObject.GetType());
			if(isV1Interface)
			{
				throw new PXException(Messages.TryingToUseNotSupportedPlugin);
			}
			var v2ProcessingInterface = CCProcessingHelper.IsV2ProcessingInterface(pluginObject);
			if(v2ProcessingInterface != null)
			{
				return new V2ExtendedProfileProcessor(v2ProcessingInterface);
			}
			throw new PXException(V1.Messages.UnknownPluginType, pluginObject.GetType().Name);
		}

		class V2ExtendedProfileProcessor : ISetCardProcessingReadersProvider, IExtendedProfileProcessingWrapper
		{
			private V2.ICCProcessingPlugin _plugin;
			private ICardProcessingReadersProvider _provider;

			public V2ExtendedProfileProcessor(V2.ICCProcessingPlugin v2Plugin)
			{
				_plugin = v2Plugin;
			}

			private T GetProcessor<T>() where T : class
			{
				V2SettingsGenerator seetingsGen = new V2SettingsGenerator(_provider);
				T processor = _plugin.CreateProcessor<T>(seetingsGen.GetSettings());
				if (processor == null)
				{
					string errorMessage = PXMessages.LocalizeFormatNoPrefixNLA(
						Messages.FeatureNotSupportedByProcessing,
						CCProcessingFeature.ExtendedProfileManagement);
					throw new PXException(errorMessage);
				}
				return processor;
			}
			public IEnumerable<V2.CustomerData> GetAllCustomerProfiles()
			{
				throw new NotImplementedException();
			}

			public IEnumerable<V2.CreditCardData> GetAllPaymentProfiles()
			{
				V2.ICCProfileProcessor processor = GetProcessor<V2.ICCProfileProcessor>();
				string customerProfileId = V2ProcessingInputGenerator.GetCustomerData(_provider.GetCustomerDataReader()).CustomerProfileID;
				IEnumerable<V2.CreditCardData> result = V2PluginErrorHandler.ExecuteAndHandleError(() => processor.GetAllPaymentProfiles(customerProfileId));
				return result;
			}

			public V2.TranProfile GetOrCreatePaymentProfileFromTransaction(string transactionId, V2.CreateTranPaymentProfileParams cParams)
			{
				V2.ICCProfileCreator processor = GetProcessor<V2.ICCProfileCreator>();
				cParams.CustomerData = V2ProcessingInputGenerator.GetCustomerData(_provider.GetCustomerDataReader());
				V2.TranProfile result = V2PluginErrorHandler.ExecuteAndHandleError(()=>processor.GetOrCreatePaymentProfileFromTransaction(transactionId, cParams));
				return result;
			}

			public V2.CustomerData GetCustomerProfile()
			{
				throw new NotImplementedException();
			}

			public void UpdateCustomerProfile()
			{
				throw new NotImplementedException();
			}

			public void UpdatePaymentProfile()
			{
				throw new NotImplementedException();
			}

			public void SetProvider( ICardProcessingReadersProvider provider )
			{
				_provider = provider;
			}

			protected ICardProcessingReadersProvider GetProvider()
			{
				if(_provider == null)
				{
					throw new PXInvalidOperationException("Could not set CardProcessingReaderProvider");
				}
				return _provider;
			}
		}
	}
}
