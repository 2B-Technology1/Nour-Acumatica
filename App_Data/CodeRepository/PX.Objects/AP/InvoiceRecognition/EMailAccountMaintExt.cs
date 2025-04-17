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

using PX.Common;
using PX.Data;
using PX.Objects.AP.InvoiceRecognition.DAC;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.AP.InvoiceRecognition
{
	[PXInternalUseOnly]
	public class EMailAccountMaintExt : PXGraphExtension<EMailAccountMaint>
	{
		[InjectDependency]
		internal IInvoiceRecognitionService InvoiceRecognitionClient { get; set; }

		protected void _(Events.RowSelected<EMailAccount> e, PXRowSelected baseEvent)
		{
			baseEvent(e.Cache, e.Args);

			if (!(e.Args.Row is EMailAccount row))
			{
				return;
			}

			PXUIFieldAttribute.SetVisible<EMailAccountExt.submitToIncomingAPDocuments>(e.Cache, row,
				PXAccess.FeatureInstalled<FeaturesSet.apDocumentRecognition>() && InvoiceRecognitionClient.IsConfigured());

			PXUIFieldAttribute.SetEnabled<EMailAccountExt.submitToIncomingAPDocuments>(e.Cache, row, row.IncomingProcessing == true);
		}
	}
}
