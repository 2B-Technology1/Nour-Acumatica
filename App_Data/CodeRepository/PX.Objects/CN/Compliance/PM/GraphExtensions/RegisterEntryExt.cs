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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.CL.Services;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.SM;

namespace PX.Objects.CN.Compliance.PM.GraphExtensions
{
    public class RegisterEntryExt : PXGraphExtension<ComplianceViewEntityExtension<RegisterEntry, PMRegister>, RegisterEntry>
    {
	    public static bool IsActive()
	    {
		    return PXAccess.FeatureInstalled<FeaturesSet.construction>();
	    }

		[PXCopyPasteHiddenView]
        public PXSelectJoin<ComplianceDocument, LeftJoin<ComplianceDocumentReference,
                    On<ComplianceDocumentReference.complianceDocumentReferenceId,
                        Equal<ComplianceDocument.projectTransactionID>>>,
                Where<ComplianceDocumentReference.type, Equal<Current<PMRegister.module>>,
                    And<ComplianceDocumentReference.referenceNumber, Equal<Current<PMRegister.refNbr>>>>>
            ComplianceDocuments;

        public PXSelect<CSAttributeGroup,
            Where<CSAttributeGroup.entityType, Equal<ComplianceDocument.typeName>,
                And<CSAttributeGroup.entityClassID, Equal<ComplianceDocument.complianceClassId>>>> ComplianceAttributeGroups;

        public PXSelect<ComplianceAnswer> ComplianceAnswers;
        public PXSelect<ComplianceDocumentReference> DocumentReference;

		[PXCopyPasteHiddenView]
		public PXSelect<ComplianceDocumentBill> LinkToBills;

		public PXSetup<LienWaiverSetup> LienWaiverSetup;

        private ComplianceDocumentService service;

        public override void Initialize()
        {
            base.Initialize();
            ValidateComplianceSetup();
            service = new ComplianceDocumentService(Base, ComplianceAttributeGroups, ComplianceDocuments,
                nameof(ComplianceDocuments));
            service.GenerateColumns(ComplianceDocuments.Cache, nameof(ComplianceAnswers));
            service.AddExpirationDateEventHandlers();
            ComplianceDocumentFieldVisibilitySetter.HideFieldsForProjectTransactionsForm(ComplianceDocuments.Cache);
        }

        private void ValidateComplianceSetup()
        {
            if (LienWaiverSetup.Current == null)
                throw new PXSetupNotEnteredException<LienWaiverSetup>();
        }


        public IEnumerable complianceDocuments()
        {
            var documents = GetComplianceDocuments().ToList();
            service.ValidateComplianceDocuments(null, documents, ComplianceDocuments.Cache);
            return documents;
        }

        public virtual void _(Events.RowUpdated<ComplianceDocument> args)
        {
            ComplianceDocuments.View.RequestRefresh();
        }

        protected virtual void _(Events.RowPersisting<PMRegister> args)
        {
            if (args.Row?.Released == true)
            {
                args.Cancel = true;
            }
        }

        protected virtual void PmRegister_RowSelected(PXCache cache, PXRowSelectedEventArgs arguments,
            PXRowSelected baseHandler)
        {
            if (!(arguments.Row is PMRegister))
            {
                return;
            }
            baseHandler(cache, arguments);
            Base.Document.Cache.AllowUpdate = true;
            ComplianceDocuments.Select();
            ComplianceDocuments.AllowInsert = !Base.Document.Cache.Inserted.Any_();
        }

        protected virtual void PmRegister_RowSelecting(PXCache cache, PXRowSelectingEventArgs arguments)
        {
            var documents = GetComplianceDocuments();
            service.ValidateComplianceDocuments(cache, documents, ComplianceDocuments.Cache);
        }

        protected virtual void PmRegister_RowDeleted(PXCache cache, PXRowDeletedEventArgs arguments)
        {
            if (!(arguments.Row is PMRegister))
            {
                return;
            }
            var documents = GetComplianceDocuments();
            foreach (var document in documents)
            {
                document.ProjectTransactionID = null;
                ComplianceDocuments.Update(document);
            }
        }

        protected virtual void ComplianceDocument_RowSelected(PXCache cache, PXRowSelectedEventArgs arguments)
        {
            service.UpdateExpirationIndicator(arguments.Row as ComplianceDocument);
        }

        protected virtual void _(Events.RowInserting<ComplianceDocument> args)
        {
            var currentProjectTransaction = Base.Document.Current;
            if (currentProjectTransaction != null)
            {
                var complianceDocument = args.Row;
                if (complianceDocument != null)
                {
                    complianceDocument.ProjectTransactionID =
                        CreateComplianceDocumentReference(currentProjectTransaction).ComplianceDocumentReferenceId;
                }
            }
        }

        private IEnumerable<ComplianceDocument> GetComplianceDocuments()
        {
            if (Base.Document.Current != null)
            {
                using (new PXConnectionScope())
                {
                    return new PXSelectJoin<ComplianceDocument, LeftJoin<ComplianceDocumentReference,
                                On<ComplianceDocumentReference.complianceDocumentReferenceId,
                                    Equal<ComplianceDocument.projectTransactionID>>>,
                            Where<ComplianceDocumentReference.type, Equal<Current<PMRegister.module>>,
                                And<ComplianceDocumentReference.referenceNumber,
                                    Equal<Current<PMRegister.refNbr>>>>>(Base)
                        .Select(Base.Document.Current.Module, Base.Document.Current.RefNbr).FirstTableItems.ToList();
                }
            }
            return new PXResultset<ComplianceDocument>().FirstTableItems.ToList();
        }

        private ComplianceDocumentReference CreateComplianceDocumentReference(PMRegister register)
        {
            var reference = new ComplianceDocumentReference
            {
                ComplianceDocumentReferenceId = Guid.NewGuid(),
                Type = register.Module,
                ReferenceNumber = register.RefNbr,
                RefNoteId = register.NoteID
            };
            return DocumentReference.Insert(reference);
        }
    }
}
