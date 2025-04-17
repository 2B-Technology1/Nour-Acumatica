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

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes
{
    public class ComplianceDocumentCheckAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber
    {
        public void FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs args)
        {
            if (args.Row is ComplianceDocument document)
            {
                UpdateCheckNumber(cache, document);
            }
        }

        private void UpdateCheckNumber(PXCache cache, ComplianceDocument document)
        {
            var checkNumber = document.ApCheckID.HasValue
                ? GetPayment(document.ApCheckID, cache.Graph)?.ExtRefNbr
                : null;
            cache.SetValue<ComplianceDocument.checkNumber>(document, checkNumber);
        }

        private APPayment GetPayment(Guid? checkId, PXGraph graph)
        {
            var reference = GetComplianceDocumentReference(checkId, graph);
			if (reference != null)
			{
				return new PXSelect<APPayment, Where<APPayment.refNbr, Equal<Required<APPayment.refNbr>>,
						And<APPayment.docType, Equal<Required<APPayment.docType>>>>>(graph)
					.SelectSingle(reference.ReferenceNumber, reference.Type);
			}

			return null;
        }

        private ComplianceDocumentReference GetComplianceDocumentReference(Guid? checkId, PXGraph graph)
        {
            return new PXSelect<ComplianceDocumentReference, Where<
                ComplianceDocumentReference.complianceDocumentReferenceId,
                Equal<Required<ComplianceDocumentReference.complianceDocumentReferenceId>>>>(
                graph).SelectSingle(checkId);
        }
    }
}
