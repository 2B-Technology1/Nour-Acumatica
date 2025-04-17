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
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.CL.Services;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes
{
    public class ComplianceDocumentBillAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber, IPXRowSelectedSubscriber
    {
        public void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            if (e.Row is ComplianceDocument row)
            {
                var billAmount = GetBillAmount(row.BillID, sender.Graph);
                sender.SetValue<ComplianceDocument.billAmount>(row, billAmount);
            }
        }

        private static decimal? GetBillAmount(Guid? refNoteId, PXGraph senderGraph)
        {
            if (!refNoteId.HasValue)
            {
                return null;
            }
            var reference = ComplianceDocumentReferenceRetriever.GetComplianceDocumentReference(senderGraph, refNoteId);
            var bill = GetBill(senderGraph, reference);
            return bill?.OrigDocAmt;
        }

        private static APInvoice GetBill(PXGraph senderGraph, ComplianceDocumentReference reference)
        {
            return new PXSelect<APInvoice,
                    Where<APInvoice.docType, Equal<Required<APInvoice.docType>>,
                        And<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>>>>(senderGraph)
                .SelectSingle(reference.Type, reference.ReferenceNumber);
        }
		
		public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ComplianceDocument row = e.Row as ComplianceDocument;
			if (row != null)
			{
				PXUIFieldAttribute.SetEnabled<ComplianceDocument.billID>(sender, row, row.IsCreatedAutomatically != true);
			}
		}
	}
}
