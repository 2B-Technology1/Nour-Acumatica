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
using PX.Objects.AR.CCPaymentProcessing.Repositories;
using PX.Objects.AR.CCPaymentProcessing.Helpers;

namespace PX.Objects.AR.CCPaymentProcessing.Factories
{
	public class ProcessingCardsPluginFactory
	{
		string processingCenterId;
		ICCPaymentProcessingRepository paymentProcessingRepository;
		public ProcessingCardsPluginFactory(string processingCenterId)
		{
			this.processingCenterId = processingCenterId;
		}

		public ICCPaymentProcessingRepository GetPaymentProcessingRepository()
		{
			if(paymentProcessingRepository == null)
			{ 
				paymentProcessingRepository = CCPaymentProcessingRepository.GetCCPaymentProcessingRepository();
			}
			return paymentProcessingRepository;
		}

		public CCProcessingCenter GetProcessingCenter()
		{
			ICCPaymentProcessingRepository repo = GetPaymentProcessingRepository();
			CCProcessingCenter processingCenter = CCProcessingCenter.PK.Find(paymentProcessingRepository.Graph, processingCenterId);
			return processingCenter;
		}

		public object GetPlugin()
		{
			CCProcessingCenter processingCenter = GetProcessingCenter();
			object plugin = GetProcessorPlugin(processingCenter);
			return plugin;
		}

		private object GetProcessorPlugin(CCProcessingCenter processingCenter)
		{
			object plugin = CCPluginTypeHelper.CreatePluginInstance(processingCenter);
			return plugin;
		}
	}
}
