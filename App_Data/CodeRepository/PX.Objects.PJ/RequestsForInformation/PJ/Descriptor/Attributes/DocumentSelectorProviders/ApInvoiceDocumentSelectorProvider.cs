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

namespace PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes.DocumentSelectorProviders
{
    public class ApInvoiceDocumentSelectorProvider : DocumentSelectorProvider
    {
        public ApInvoiceDocumentSelectorProvider(PXGraph graph, string fieldName)
            : base(graph, fieldName)
        {
        }

        public override string DocumentType => RequestForInformationRelationTypeAttribute.ApInvoice;

        protected override Type SelectorType => typeof(APInvoice);

        protected override Type SelectorQuery =>
			typeof(Select<APInvoice, Where<APInvoice.docType, Equal<APDocType.invoice>>>);

		protected override Type SubstituteKeyType => typeof(APInvoice.refNbr);

		protected override Type[] SelectorFieldTypes =>
            new[]
            {
                typeof(APInvoice.docType),
                typeof(APInvoice.refNbr),
                typeof(APInvoice.docDate),
                typeof(APRegister.finPeriodID),
                typeof(APInvoice.vendorID),
                typeof(APInvoice.invoiceNbr),
                typeof(APInvoice.curyID),
                typeof(APInvoice.curyOrigDocAmt),
                typeof(APInvoice.curyDocBal),
                typeof(APInvoice.status),
                typeof(APInvoice.dueDate)
            };

        public override void NavigateToDocument(Guid? noteId)
        {
            var invoiceEntry = PXGraph.CreateInstance<APInvoiceEntry>();
            invoiceEntry.Document.Current = GetInvoice(noteId);

            throw new PXRedirectRequiredException(invoiceEntry, string.Empty)
            {
                Mode = PXBaseRedirectException.WindowMode.NewWindow
            };
        }

        private APInvoice GetInvoice(Guid? noteId)
        {
            var query = new PXSelect<APInvoice,
                Where<APInvoice.noteID, Equal<Required<APInvoice.noteID>>>>(Graph);

            return query.SelectSingle(noteId);
        }
    }
}
