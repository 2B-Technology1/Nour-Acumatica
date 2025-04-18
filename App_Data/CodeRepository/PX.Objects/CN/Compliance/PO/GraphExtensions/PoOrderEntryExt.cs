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
using PX.Objects.CN.Compliance.PO.CacheExtensions;
using PX.Objects.CN.Subcontracts.SC.Graphs;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.Objects.PO;
using PX.SM;

namespace PX.Objects.CN.Compliance.PO.GraphExtensions
{
    public class PoOrderEntryExt : PXGraphExtension<ComplianceViewEntityExtension<POOrderEntry, POOrder>, POOrderEntry>
    {
	    public static bool IsActive()
	    {
		    return PXAccess.FeatureInstalled<FeaturesSet.construction>();
	    }

		[PXCopyPasteHiddenView]
        public PXSelectJoin<ComplianceDocument, LeftJoin<ComplianceDocumentReference,
                On<ComplianceDocumentReference.complianceDocumentReferenceId, Equal<ComplianceDocument.purchaseOrder>>>,
            Where<ComplianceDocumentReference.type, Equal<Current<POOrder.orderType>>,
                And<ComplianceDocumentReference.referenceNumber, Equal<Current<POOrder.orderNbr>>,
                    Or<ComplianceDocument.subcontract, Equal<Current<POOrder.orderNbr>>,
                        And<Current<POOrder.orderType>, Equal<POOrderType.regularSubcontract>>>>>> ComplianceDocuments;

        public PXSelect<CSAttributeGroup,
                Where<CSAttributeGroup.entityType, Equal<ComplianceDocument.typeName>,
                    And<CSAttributeGroup.entityClassID, Equal<ComplianceDocument.complianceClassId>>>>
            ComplianceAttributeGroups;

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
            UpdateFieldsVisibilityForComplianceDocuments();
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

        protected virtual void _(Events.RowSelected<POLine> args)
        {
            if (args.Row != null)
            {
                ValidatePoLine(args.Row);
            }
        }

        protected virtual void _(Events.RowSelected<POOrder> args)
        {
            if (args.Row != null)
            {
                service.ValidateRelatedField<POOrder, ComplianceDocument.vendorID, POOrder.vendorID>(args.Row, args.Row.VendorID);
            }
        }

        protected virtual void _(Events.RowSelected<ComplianceDocument> args)
        {
            if (args.Row != null)
            {
                service.UpdateExpirationIndicator(args.Row);
            }
        }

        protected virtual void PoOrder_RowSelected(PXCache cache, PXRowSelectedEventArgs arguments,
            PXRowSelected baseHandler)
        {
            if (!(arguments.Row is POOrder))
            {
                return;
            }
            baseHandler(cache, arguments);
            ComplianceDocuments.Select();
            ComplianceDocuments.AllowInsert = !Base.Document.Cache.Inserted.Any_();
        }

        protected virtual void _(Events.RowSelecting<POOrder> args)
        {
            var documents = GetComplianceDocuments();
            service.ValidateComplianceDocuments(args.Cache, documents, ComplianceDocuments.Cache);
        }

        protected virtual void _(Events.RowInserting<ComplianceDocument> args)
        {
            var poOrder = Base.Document.Current;
            var complianceDocument = args.Row;
            if (poOrder != null && complianceDocument != null)
            {
                var poLine = Base.Transactions.SelectSingle();
                FillPurchaseOrderInfo(complianceDocument, poOrder, poLine);
            }
        }

        protected virtual void _(Events.RowDeleted<POOrder> args)
        {
            var order = args.Row;
            if (order == null)
            {
                return;
            }
            var documents = GetComplianceDocuments();
            foreach (var document in documents)
            {
                RemoveComplianceReference(document);
            }
        }

        private void RemoveComplianceReference(ComplianceDocument document)
        {
            if (IsSubcontractScreen())
            {
                document.Subcontract = null;
            }
            else
            {
                document.PurchaseOrder = null;
            }
            ComplianceDocuments.Update(document);
        }

        private void FillPurchaseOrderInfo(ComplianceDocument complianceDocument, POOrder poOrder, POLine poLine)
        {
            complianceDocument.VendorID = poOrder.VendorID;
            complianceDocument.VendorName = GetVendorName(complianceDocument.VendorID);
            complianceDocument.ProjectID = poLine?.ProjectID;
            complianceDocument.CostTaskID = poLine?.TaskID;
            complianceDocument.AccountID = poLine?.ExpenseAcctID;
            complianceDocument.CostCodeID = poLine?.CostCodeID;
            SetComplianceDocumentReference(complianceDocument, poOrder);
        }

        private void SetComplianceDocumentReference(ComplianceDocument complianceDocument, POOrder poOrder)
        {
            if (IsSubcontractScreen())
            {
                complianceDocument.Subcontract = poOrder.OrderNbr;
            }
            else
            {
                complianceDocument.PurchaseOrder = CreateComplianceDocumentReference(poOrder).ComplianceDocumentReferenceId;
            }
        }

        private void ValidatePoLine(POLine poLine)
        {
            var projectHasExpiredCompliance =
                service.ValidateRelatedProjectField<POLine, POLine.projectID>(poLine, poLine.ProjectID);
            var taskHasExpiredCompliance =
                service.ValidateRelatedField<POLine, ComplianceDocument.costTaskID, POLine.taskID>(poLine, poLine.TaskID);
            service.ValidateRelatedRow<POLine, PoLineExt.hasExpiredComplianceDocuments>(poLine,
                projectHasExpiredCompliance || taskHasExpiredCompliance);
        }

        private IEnumerable<ComplianceDocument> GetComplianceDocuments()
        {
            if (Base.Document.Current == null)
            {
                return new PXResultset<ComplianceDocument>().FirstTableItems.ToList();
            }
            using (new PXConnectionScope())
            {
                return IsSubcontractScreen()
                    ? GetCompliancesForSubcontract()
                    : GetCompliancesForPurchaseOrder();
            }
        }

        private IEnumerable<ComplianceDocument> GetCompliancesForPurchaseOrder()
        {
            return new PXSelectJoin<ComplianceDocument, LeftJoin<ComplianceDocumentReference,
                        On<ComplianceDocumentReference.complianceDocumentReferenceId,
                            Equal<ComplianceDocument.purchaseOrder>>>,
                    Where<ComplianceDocumentReference.type, Equal<Current<POOrder.orderType>>,
                        And<ComplianceDocumentReference.referenceNumber, Equal<Current<POOrder.orderNbr>>>>>(Base)
                .Select(Base.Document.Current.OrderType, Base.Document.Current.OrderNbr).FirstTableItems.ToList();
        }

        private IEnumerable<ComplianceDocument> GetCompliancesForSubcontract()
        {
            return new PXSelect<ComplianceDocument,
                    Where<ComplianceDocument.subcontract,
                        Equal<Required<POOrder.orderNbr>>>>(Base)
                .Select(Base.Document.Current.OrderNbr).FirstTableItems.ToList();
        }

        private string GetVendorName(int? vendorId)
        {
            if (!vendorId.HasValue)
            {
                return null;
            }
            var vendor = new PXSelect<Vendor,
                Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>(Base).SelectSingle(vendorId);
            return vendor?.AcctName;
        }

        private bool IsSubcontractScreen()
        {
            return Base.GetType() == typeof(SubcontractEntry)
                || Base.GetType().BaseType == typeof(SubcontractEntry);
        }

        private void UpdateFieldsVisibilityForComplianceDocuments()
        {
            if (IsSubcontractScreen())
            {
                ComplianceDocumentFieldVisibilitySetter.HideFieldsForSubcontract(ComplianceDocuments.Cache);
            }
            else
            {
                ComplianceDocumentFieldVisibilitySetter.HideFieldsForPurchaseOrder(ComplianceDocuments.Cache);
            }
        }

        private ComplianceDocumentReference CreateComplianceDocumentReference(POOrder pOrder)
        {
            var reference = new ComplianceDocumentReference
            {
                ComplianceDocumentReferenceId = Guid.NewGuid(),
                Type = pOrder.OrderType,
                ReferenceNumber = pOrder.OrderNbr,
                RefNoteId = pOrder.NoteID
            };
            return DocumentReference.Insert(reference);
        }
    }
}
