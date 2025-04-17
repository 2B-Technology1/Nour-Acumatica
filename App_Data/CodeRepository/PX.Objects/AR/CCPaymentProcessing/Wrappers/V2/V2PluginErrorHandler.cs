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
using System.Net;
using PX.Objects.AR.CCPaymentProcessing.Common;
using V2 = PX.CCProcessingBase.Interfaces.V2;

namespace PX.Objects.AR.CCPaymentProcessing.Wrappers
{
	public static class V2PluginErrorHandler
	{
		public static void ExecuteAndHandleError(Action pluginAction)
		{
			try
			{
				pluginAction();
			}
			catch (V2.CCProcessingException e)
			{
				throw new PXException(e.InnerException, Messages.CardProcessingError, CCError.CCErrorSource.ProcessingCenter, e.Message);
			}
			catch (WebException e)
			{
				throw new PXException(e, Messages.CardProcessingError, CCError.CCErrorSource.Network, e.Message);
			}
			catch (Exception e)
			{
				throw new PXException(e, Messages.CardProcessingError, CCError.CCErrorSource.Internal, e.Message);
			}
		}

		public static T ExecuteAndHandleError<T>(Func<T> pluginAction)
		{
			T result = default(T);
			try
			{
				result = pluginAction();
			}
			catch (V2.CCProcessingException e)
			{
				throw new PXException(e, Messages.CardProcessingError, CCError.CCErrorSource.ProcessingCenter, e.Message);
			}
			catch (WebException e)
			{
				throw new PXException(e, Messages.CardProcessingError, CCError.CCErrorSource.Network, e.Message);
			}
			catch (Exception e)
			{
				throw new PXException(e, Messages.CardProcessingError, CCError.CCErrorSource.Internal, e.Message);
			}
			return result;
		}
	}
}
