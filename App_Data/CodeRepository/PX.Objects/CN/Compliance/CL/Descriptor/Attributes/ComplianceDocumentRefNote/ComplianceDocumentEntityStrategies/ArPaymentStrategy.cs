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
using PX.Objects.AR;
using PX.Objects.CN.Compliance.AR.CacheExtensions;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes.ComplianceDocumentRefNote.ComplianceDocumentEntityStrategies
{
    public class ArPaymentStrategy : ComplianceDocumentEntityStrategy
    {
        public ArPaymentStrategy()
        {
            EntityType = typeof(ARPayment);
            FilterExpression = typeof(Where<ARPayment.docType, Equal<ARDocType.payment>,
                Or<ARPayment.docType, Equal<ARDocType.creditMemo>,
                    Or<ARPayment.docType, Equal<ARDocType.prepayment>,
                        Or<ARPayment.docType, Equal<ARDocType.refund>,
                            Or<ARPayment.docType, Equal<ARDocType.voidPayment>,
                                Or<ARPayment.docType, Equal<ARDocType.smallBalanceWO>,
                                    Or<ARPayment.docType, Equal<ARDocType.voidRefund>
                                    >>>>>>>);
            TypeField = typeof(ARPayment.docType);
        }

        public override Guid? GetNoteId(PXGraph graph, string clDisplayName)
        {
            var key = ComplianceReferenceTypeHelper.ConvertToDocumentKey<ARPayment>(clDisplayName);

            var noteId = new PXSelect<ARPayment,
                Where<ARPayment.docType, Equal<Required<ARPayment.docType>>,
                And<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>>>>(graph)
                .Select(key.DocType, key.RefNbr)
                .FirstTableItems
                .ToList()
                .SingleOrDefault()
                ?.NoteID;

            return noteId;
        }
    }
}
