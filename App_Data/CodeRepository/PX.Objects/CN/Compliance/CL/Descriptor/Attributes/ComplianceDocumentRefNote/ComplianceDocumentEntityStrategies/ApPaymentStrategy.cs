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
    public class ApPaymentStrategy : ComplianceDocumentEntityStrategy
    {
        public ApPaymentStrategy()
        {
            EntityType = typeof(APPayment);
            FilterExpression = typeof(Where<APPayment.docType, Equal<APDocType.check>,
                Or<APPayment.docType, Equal<APDocType.debitAdj>,
                    Or<APPayment.docType, Equal<APDocType.prepayment>,
                        Or<APPayment.docType, Equal<APDocType.refund>,
                            Or<APPayment.docType, Equal<APDocType.voidCheck>,
                                Or<APPayment.docType, Equal<APDocType.voidRefund>>>>>>>);
            TypeField = typeof(APPayment.docType);
        }

        public override Guid? GetNoteId(PXGraph graph, string clDisplayName)
        {
            var key = ComplianceReferenceTypeHelper.ConvertToDocumentKey<APPayment>(clDisplayName);

            var noteId = new PXSelect<APPayment,
                Where<APPayment.docType, Equal<Required<APPayment.docType>>,
                And<APPayment.refNbr, Equal<Required<APPayment.refNbr>>>>>(graph)
                .Select(key.DocType, key.RefNbr)
                .FirstTableItems
                .ToList()
                .SingleOrDefault()
                ?.NoteID;

            return noteId;
        }
    }
}
