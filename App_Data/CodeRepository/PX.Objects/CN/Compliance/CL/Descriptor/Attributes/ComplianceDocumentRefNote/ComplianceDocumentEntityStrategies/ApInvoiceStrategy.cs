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
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Compliance.AP.CacheExtensions;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes.ComplianceDocumentRefNote.ComplianceDocumentEntityStrategies
{
    public class ApInvoiceStrategy : ComplianceDocumentEntityStrategy
    {
        public ApInvoiceStrategy()
        {
            EntityType = typeof(APInvoice);
            FilterExpression = typeof(Where<APInvoice.docType, Equal<APDocType.invoice>,
                Or<APInvoice.docType, Equal<APDocType.creditAdj>,
                    Or<APInvoice.docType, Equal<APDocType.debitAdj>,
                        Or<APInvoice.docType, Equal<APDocType.prepayment>>>>>);
            TypeField = typeof(APInvoice.docType);
        }

        public override Guid? GetNoteId(PXGraph graph, string clDisplayName)
        {
            var key = ComplianceReferenceTypeHelper.ConvertToDocumentKey<APInvoice>(clDisplayName);

            var noteId = new PXSelect<APInvoice,
                Where<APInvoice.docType, Equal<Required<APInvoice.docType>>,
                And<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>>>>(graph)
                .Select(key.DocType, key.RefNbr)
                .FirstTableItems
                .ToList()
                .SingleOrDefault()
                ?.NoteID;

            return noteId;
        }
    }
}