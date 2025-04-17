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
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using System;
using System.Collections;

namespace PX.Objects.CA.Descriptor
{
	class CCProcessingCenterPaymentMethodFilterAttribute : PXCustomSelectorAttribute
	{
		Type search;
		public CCProcessingCenterPaymentMethodFilterAttribute(Type type) : base(typeof(CCProcessingCenter.processingCenterID), typeof(CCProcessingCenter.processingCenterID))
		{
			search = type;
		}

		public IEnumerable GetRecords()
		{
			var paymentMethod = ((PaymentMethod)_Graph.Caches<PaymentMethod>().Current).PaymentType;
			BqlCommand command = BqlCommand.CreateInstance(search);
			PXView view = new PXView(_Graph, false, command);

			foreach (var procCenterObj in view.SelectMulti())
			{
				var procCenter = PXResult.Unwrap<CCProcessingCenter>(procCenterObj);
				if (paymentMethod != PaymentMethodType.EFT)
				{
					yield return procCenter;
				}
				else if (CCProcessingFeatureHelper.IsFeatureSupported(procCenter, CCProcessingFeature.EFTSupport, false))
				{ 
					yield return procCenter;
				}
			}
		}
	}
}
