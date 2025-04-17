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

namespace PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes.DocumentSelectorProviders
{
    public class ArInvoiceDocumentSelectorProvider : DocumentSelectorProvider
    {
        public ArInvoiceDocumentSelectorProvider(PXGraph graph, string fieldName)
            : base(graph, fieldName)
        {
        }

        public override string DocumentType => RequestForInformationRelationTypeAttribute.ArInvoice;

        protected override Type SelectorType => typeof(ARInvoice);

        protected override Type SelectorQuery =>
			typeof(Select<ARInvoice, Where<ARInvoice.docType, Equal<ARDocType.invoice>>>);

		protected override Type SubstituteKeyType => typeof(ARInvoice.refNbr);

		protected override Type[] SelectorFieldTypes =>
            new[]
            {
                typeof(ARInvoice.docType),
                typeof(ARInvoice.refNbr),
                typeof(ARInvoice.docDate),
                typeof(ARRegister.finPeriodID),
                typeof(ARInvoice.customerID),
                typeof(ARInvoice.curyID),
                typeof(ARInvoice.curyOrigDocAmt),
                typeof(ARInvoice.curyDocBal),
                typeof(ARInvoice.status),
                typeof(ARInvoice.dueDate)
            };

        public override void NavigateToDocument(Guid? noteId)
        {
            var invoiceEntry = PXGraph.CreateInstance<ARInvoiceEntry>();
            invoiceEntry.Document.Current = GetInvoice(noteId);

            throw new PXRedirectRequiredException(invoiceEntry, string.Empty)
            {
                Mode = PXBaseRedirectException.WindowMode.NewWindow
            };
        }

        private ARInvoice GetInvoice(Guid? noteId)
        {
            var query = new PXSelect<ARInvoice,
                Where<ARInvoice.noteID, Equal<Required<ARInvoice.noteID>>>>(Graph);

            return query.SelectSingle(noteId);
        }
    }
}
