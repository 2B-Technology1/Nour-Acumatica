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

using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CN.Common.Helpers;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.CL.Services;
using PX.Objects.CN.Compliance.Descriptor;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.SM;
using System;

namespace PX.Objects.CN.Compliance.CL.Graphs
{
    [DashboardType((int) DashboardTypeAttribute.Type.Default)]
    public class ComplianceDocumentEntry : PXGraph<ComplianceDocumentEntry>
    {
        [PXFilterable]
        public PXSelectOrderBy<ComplianceDocument, OrderBy<Asc<ComplianceDocument.createdDateTime>>> Documents;

        public PXSelect<CSAttributeGroup,
            Where<CSAttributeGroup.entityType, Equal<ComplianceDocument.typeName>,
                And<CSAttributeGroup.entityClassID, Equal<ComplianceDocument.complianceClassId>>>> ComplianceAttributeGroups;

        public PXSelect<ComplianceAnswer> ComplianceAnswers;
        public PXSelect<ComplianceDocumentReference> DocumentReference;

		[PXCopyPasteHiddenView]
		public PXSelect<ComplianceDocumentBill> LinkToBills;

        public PXSave<ComplianceDocument> Save;
        public PXCancel<ComplianceDocument> Cancel;

        public ComplianceDocumentEntry()
        {
            FeaturesSetHelper.CheckConstructionFeature();
            var service = new ComplianceDocumentService(this, ComplianceAttributeGroups, Documents, nameof(Documents));
            service.GenerateColumns(Documents.Cache, nameof(ComplianceAnswers));
        }

        public override void Persist()
        {
            base.Persist();
        }

        protected virtual void ComplianceDocument_DocumentType_FieldVerifying(PXCache cache,
            PXFieldVerifyingEventArgs arguments)
        {
            if (arguments.NewValue == null)
            {
                throw new PXSetPropertyException(ComplianceMessages.RequiredFieldMessage);
            }
        }
    }

    public class ComplianceDocumentEntryExt : ComplianceViewEntityExtension<ComplianceDocumentEntry, ComplianceDocument> { }
}
