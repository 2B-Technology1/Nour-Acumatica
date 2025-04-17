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
    public class ProjectEntryExt : PXGraphExtension<ComplianceViewEntityExtension<ProjectEntry, PMProject>, ProjectEntry>
    {
        [PXCopyPasteHiddenView]
        public PXSelect<ComplianceDocument,
            Where<ComplianceDocument.projectID, Equal<Current<PMProject.contractID>>>> ComplianceDocuments;

        public PXSelect<CSAttributeGroup,
            Where<CSAttributeGroup.entityType, Equal<ComplianceDocument.typeName>,
                And<CSAttributeGroup.entityClassID, Equal<ComplianceDocument.complianceClassId>>>> ComplianceAttributeGroups;

        public PXSelect<ComplianceAnswer> ComplianceAnswers;

		[PXCopyPasteHiddenView]
		public PXSelect<ComplianceDocumentBill> LinkToBills;

		private ComplianceDocumentService service;

        public override void Initialize()
        {
            base.Initialize();
            service = new ComplianceDocumentService(Base, ComplianceAttributeGroups, ComplianceDocuments,
                nameof(ComplianceDocuments));
            service.GenerateColumns(ComplianceDocuments.Cache, nameof(ComplianceAnswers));
            service.AddExpirationDateEventHandlers();
            ComplianceDocumentFieldVisibilitySetter.HideFieldsForProject(ComplianceDocuments.Cache);
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

        protected virtual void PmProject_RowSelected(PXCache cache, PXRowSelectedEventArgs args,
            PXRowSelected baseHandler)
        {
            if (!(args.Row is PMProject))
            {
                return;
            }
            baseHandler(cache, args);
            ComplianceDocuments.Select();
            ComplianceDocuments.AllowInsert = !Base.Project.Cache.Inserted.Any_();
        }

        protected virtual void _(Events.RowSelecting<PMProject> args)
        {
            var documents = GetComplianceDocuments();
            service?.ValidateComplianceDocuments(args.Cache, documents, ComplianceDocuments.Cache);
        }

        protected virtual void _(Events.RowDeleted<PMProject> args)
        {
            var project = args.Row;
            if (project == null)
            {
                return;
            }
            var documents = GetComplianceDocuments();
            foreach (var document in documents)
            {
                document.ProjectID = null;
                ComplianceDocuments.Update(document);
            }
        }

        protected virtual void _(Events.RowSelected<ComplianceDocument> args)
        {
            service.UpdateExpirationIndicator(args.Row);

			if (args.Row != null)
			{
				ComplianceDocument lw = args.Row;

				ComplianceAttributeType lwType = PXSelect<ComplianceAttributeType,
					Where<ComplianceAttributeType.type, Equal<Required<ComplianceAttributeType.type>>>>.Select(Base, ComplianceDocumentType.LienWaiver);

				if (lw.DocumentType == lwType.ComplianceAttributeTypeID && lw.ThroughDate != null
						&& lw.ThroughDate < Base.Accessinfo.BusinessDate && lw.Received != true)
				{
					Vendor vendor = PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>.Select(Base, lw.VendorID);

					args.Cache.RaiseExceptionHandling<ComplianceDocument.throughDate>(lw, lw.ThroughDate,
						new PXSetPropertyException<ComplianceDocument.throughDate>(Descriptor.ComplianceMessages.LienWaiver.OutstandingLienWaiver, PXErrorLevel.Warning, vendor.AcctName));
				}
			}
		}
        protected virtual void _(Events.RowInserting<ComplianceDocument> args)
        {
            var project = Base.Project.Current;
            var complianceDocument = args.Row;
            if (project != null && complianceDocument != null)
            {
                FillProjectInfo(complianceDocument, project);
            }
        }

        private void FillProjectInfo(ComplianceDocument complianceDocument, PMProject project)
        {
            complianceDocument.ProjectID = project.ContractID;
            complianceDocument.CustomerID = project.CustomerID;
            complianceDocument.CustomerName = GetCustomerName(complianceDocument.CustomerID);
        }

        private IEnumerable<ComplianceDocument> GetComplianceDocuments()
        {
            if (Base.Project.Current != null)
            {
                using (new PXConnectionScope())
                {
                    return new PXSelect<ComplianceDocument,
                        Where<ComplianceDocument.projectID,
                            Equal<Required<PMProject.contractID>>>>(Base)
                        .Select(Base.Project.Current.ContractID).FirstTableItems.ToList();
                }
            }
            return new PXResultset<ComplianceDocument>().FirstTableItems.ToList();
        }

        private string GetCustomerName(int? customerId)
        {
            if (!customerId.HasValue)
            {
                return null;
            }
            Customer customer = PXSelect<Customer,
                Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(Base, customerId);
            return customer?.AcctName;
        }

        public static bool IsActive()
        {
			return PXAccess.FeatureInstalled<FeaturesSet.construction>();
		}
    }
}
