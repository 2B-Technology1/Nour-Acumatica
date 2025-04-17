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

using System;
using System.Collections.Generic;
using PX.CCProcessingBase;
using PX.Data;
using PX.Objects.CA;

namespace PX.Objects.AR.CCPaymentProcessing.Repositories
{
	public class ProcessingCenterSettingsStorage : IProcessingCenterSettingsStorage
	{
		private readonly PXGraph _graph;
		private readonly string _processingCenterID;

		public ProcessingCenterSettingsStorage(PXGraph graph, string processingCenterID)
		{
			if (graph == null) throw new ArgumentNullException(nameof(graph));
			if (String.IsNullOrEmpty(processingCenterID)) throw new ArgumentNullException(nameof(processingCenterID));

			_graph = graph;
			_processingCenterID = processingCenterID;
		}

		void IProcessingCenterSettingsStorage.ReadSettings(Dictionary<string, string> aSettings)
		{
			PXSelectBase<CCProcessingCenterDetail> settings = new PXSelect<CCProcessingCenterDetail, Where<CCProcessingCenterDetail.processingCenterID,
						Equal<Required<CCProcessingCenterDetail.processingCenterID>>>>(_graph);
			foreach (CCProcessingCenterDetail it in settings.Select(_processingCenterID))
			{
				aSettings[it.DetailID] = it.Value;
			}
		}		
	}
}
