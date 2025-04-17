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

using System.Collections;
using PX.Data;
using PX.Objects.CA;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.AR.CCPaymentProcessing.Repositories;

namespace PX.Objects.AR
{
	public class CCProcessingCenterSelectorAttribute : PXCustomSelectorAttribute
	{
		CCProcessingFeature feature;
		public CCProcessingCenterSelectorAttribute(CCProcessingFeature feature) : base( typeof(CCProcessingCenter.processingCenterID), typeof(CCProcessingCenter.processingCenterID) )
		{
			this.feature = feature;
		}

		public IEnumerable GetRecords()
		{
			PXSelectBase<CCProcessingCenter> query = new PXSelectReadonly<CCProcessingCenter>(_Graph);

			foreach (CCProcessingCenter item in query.Select())
			{
				string processingCenterId = item.ProcessingCenterID;
				CCProcessingCenter processingCenter = CCProcessingCenter.PK.Find(_Graph, processingCenterId);

				if (CCProcessingFeatureHelper.IsFeatureSupported(processingCenter, feature, false) && processingCenter.AllowSaveProfile == true)
				{
					yield return item;
				}
			}
		}
	}
}
