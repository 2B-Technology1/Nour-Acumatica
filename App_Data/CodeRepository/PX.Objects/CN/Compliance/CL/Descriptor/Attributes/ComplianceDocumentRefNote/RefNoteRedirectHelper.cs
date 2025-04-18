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
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CN.Compliance.CL.Services;
using PX.Objects.PM;
using PX.Objects.PO;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes.ComplianceDocumentRefNote
{
    public class RefNoteRedirectHelper
    {
        private readonly IList<RefNoteBasedRedirectionInstruction> redirectionInstructions =
            new List<RefNoteBasedRedirectionInstruction>
            {
                new RefNoteBasedRedirectionInstruction(typeof(APInvoice), typeof(APInvoiceEntry),
                    typeof(APInvoice.docType), typeof(APInvoice.refNbr)),
                new RefNoteBasedRedirectionInstruction(typeof(ARInvoice), typeof(ARInvoiceEntry),
                    typeof(ARInvoice.docType), typeof(ARInvoice.refNbr)),
                new RefNoteBasedRedirectionInstruction(typeof(APPayment), typeof(APPaymentEntry),
                    typeof(APPayment.docType), typeof(APPayment.refNbr)),
                new RefNoteBasedRedirectionInstruction(typeof(ARPayment), typeof(ARPaymentEntry),
                    typeof(ARPayment.docType), typeof(ARPayment.refNbr)),
                new RefNoteBasedRedirectionInstruction(typeof(POOrder), typeof(POOrderEntry),
                    typeof(POOrder.orderType), typeof(POOrder.orderNbr)),
                new RefNoteBasedRedirectionInstruction(typeof(PMRegister), typeof(RegisterEntry),
                    typeof(PMRegister.module), typeof(PMRegister.refNbr))
            };

        public void Redirect(Type itemType, Guid referenceId)
        {
            var redirectionInstruction = redirectionInstructions.Single(x => x.EntityType == itemType);
            var graph = PXGraph.CreateInstance(redirectionInstruction.GraphType);
            if (graph != null)
            {
                var reference = ComplianceDocumentReferenceRetriever.GetComplianceDocumentReference(graph, referenceId);
                var entity = GetEntity(graph, redirectionInstruction, reference.Type, reference.ReferenceNumber);
                graph.Caches[redirectionInstruction.EntityType].Current = entity;
                throw new PXRedirectRequiredException(graph, string.Empty)
                {
                    Mode = PXBaseRedirectException.WindowMode.NewWindow
                };
            }
        }

        /// <summary>
        /// Dynamically builds a query in the following format
        /// PXSelect&lt;TEntity, Where&lt;TRefNoteField, Equal&lt;Required&lt;TRefNoteField&gt;&gt;&gt;&gt; and selects
        /// single record.
        /// </summary>
        private static object GetEntity(PXGraph graph, RefNoteBasedRedirectionInstruction redirectionInstruction,
            params object[] bqlParameters)
        {
            var command = BqlCommand.CreateInstance(
                typeof(Select<,>), redirectionInstruction.EntityType,
                typeof(Where<,,>), redirectionInstruction.ReferenceTypeField,
                typeof(Equal<>), typeof(Required<>), redirectionInstruction.ReferenceTypeField,
                typeof(And<,>), redirectionInstruction.ReferenceNumberField,
                typeof(Equal<>), typeof(Required<>), redirectionInstruction.ReferenceNumberField);
            var view = new PXView(graph, true, command);
            return view.SelectSingle(bqlParameters);
        }
    }
}