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
using PX.Objects.AR;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.CL.Services;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes
{
    public class ComplianceDocumentInvoiceAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber
    {
        public void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            if (e.Row is ComplianceDocument row)
            {
                var invoiceAmount = GetInvoiceAmount(row.InvoiceID, sender.Graph);
                sender.SetValue<ComplianceDocument.invoiceAmount>(row, invoiceAmount);
            }
        }

        private static decimal? GetInvoiceAmount(Guid? refNoteId, PXGraph senderGraph)
        {
            if (!refNoteId.HasValue)
            {
                return null;
            }
            var reference = ComplianceDocumentReferenceRetriever.GetComplianceDocumentReference(senderGraph, refNoteId);
            var invoice = GetInvoice(senderGraph, reference);
            return invoice?.OrigDocAmt;
        }

        private static ARInvoice GetInvoice(PXGraph senderGraph, ComplianceDocumentReference reference)
        {
            return new PXSelect<ARInvoice,
                    Where<ARInvoice.docType, Equal<Required<ARInvoice.docType>>,
                        And<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>>>>(senderGraph)
                .SelectSingle(reference.Type, reference.ReferenceNumber);
        }
    }
}