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
using System.Reflection;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.CL.Descriptor.Attributes.ComplianceDocumentRefNote;
using PX.Objects.CN.Compliance.CL.Services;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.SM;

namespace PX.Objects.CN.Compliance.AP.GraphExtensions
{
    public class ApPaymentEntryExt : PXGraphExtension<ComplianceViewEntityExtension<APPaymentEntry, APPayment>, APPaymentEntry>
    {
        [PXCopyPasteHiddenView]
        public PXSelectJoin<ComplianceDocument, LeftJoin<ComplianceDocumentReference,
                On<ComplianceDocumentReference.complianceDocumentReferenceId, Equal<ComplianceDocument.apCheckId>>>,
            Where<ComplianceDocumentReference.type, Equal<Current<APPayment.docType>>,
                And<ComplianceDocumentReference.referenceNumber, Equal<Current<APPayment.refNbr>>>>> ComplianceDocuments;

        public PXSelect<CSAttributeGroup,
            Where<CSAttributeGroup.entityType, Equal<ComplianceDocument.typeName>,
                And<CSAttributeGroup.entityClassID, Equal<ComplianceDocument.complianceClassId>>>> ComplianceAttributeGroups;

        public PXSelect<ComplianceAnswer> ComplianceAnswers;
        public PXSelect<ComplianceDocumentReference> DocumentReference;

		public PXSelectJoin<ComplianceDocumentBill,
			InnerJoin<APInvoice, On<ComplianceDocumentBill.docType, Equal<APInvoice.docType>,
				And<ComplianceDocumentBill.refNbr, Equal<APInvoice.refNbr>>>>,
			Where<ComplianceDocumentBill.complianceDocumentID, Equal<Current<ComplianceDocument.complianceDocumentID>>>> ComplianceDetails;

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
            ComplianceDocumentFieldVisibilitySetter.HideFieldsForApPayment(ComplianceDocuments.Cache);
        }

		public PXAction<APPayment> complianceViewDetails;
		[PXUIField(DisplayName = "Line Details", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(DisplayOnMainToolbar = false, VisibleOnProcessingResults = false, PopupVisible = false)]
		public virtual IEnumerable ComplianceViewDetails(PXAdapter adapter)
		{
			if (ComplianceDocuments.Current != null &&
				ComplianceDocuments.Current.IsCreatedAutomatically == true)
			{
				ComplianceDetails.AskExt();
			}
			return adapter.Get();
		}

		public IEnumerable adjustments_History()
        {
            foreach (PXResult result in GetAdjustmentHistory())
            {
                service.ValidateApAdjustment<APAdjust.displayRefNbr>(result.GetItem<APAdjust>());
                yield return result;
            }
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

        protected virtual void _(Events.RowSelected<APAdjust> args)
        {
            if (args.Row != null)
            {
                service.ValidateApAdjustment<APAdjust.adjdRefNbr>(args.Row);
            }
        }

        protected virtual void ApPayment_RowSelected(PXCache cache, PXRowSelectedEventArgs arguments,
            PXRowSelected baseHandler)
        {
            if (!(arguments.Row is APPayment))
            {
                return;
            }
            baseHandler(cache, arguments);
            Base.Document.Cache.AllowUpdate = true;
            ComplianceDocuments.Select();
            ComplianceDocuments.AllowInsert = !Base.Document.Cache.Inserted.Any_();
        }

        protected virtual void _(Events.RowSelected<APPayment> args)
        {
            if (args.Row is APPayment payment)
            {
                service.ValidateRelatedField<APPayment, ComplianceDocument.vendorID, APPayment.vendorID>(payment,
                    payment.VendorID);
            }
            var documents = GetComplianceDocuments();
            service.ValidateComplianceDocuments(args.Cache, documents, ComplianceDocuments.Cache);
        }

        protected virtual void _(Events.RowDeleted<APPayment> args)
        {
            var payment = args.Row;
            if (payment == null)
            {
                return;
            }

            var documents = GetComplianceDocuments();

			ComplianceAttributeType lwDocType = PXSelect<ComplianceAttributeType,
				Where<ComplianceAttributeType.type, Equal<Required<ComplianceAttributeType.type>>>>.Select(Base, ComplianceDocumentType.LienWaiver);

            foreach (var document in documents)
            {
				if (document.DocumentType != lwDocType.ComplianceAttributeTypeID)
				{
					document.ApCheckID = null;
					ComplianceDocuments.Update(document);
				}
            }
        }

        protected virtual void _(Events.RowSelected<ComplianceDocument> args)
        {
            service.UpdateExpirationIndicator(args.Row);
        }

        protected virtual void _(Events.RowInserting<ComplianceDocument> args)
        {
            var currentApPayment = Base.Document.Current;
            if (currentApPayment != null)
            {
                var complianceDocument = args.Row;
                if (complianceDocument != null && complianceDocument.SkipInit != true)
                {
                    complianceDocument.VendorID = currentApPayment.VendorID;
                    complianceDocument.VendorName = GetVendorName(complianceDocument.VendorID);
					ComplianceDocumentRefNoteAttribute.SetComplianceDocumentReference<ComplianceDocument.apCheckId>(
						args.Cache, args.Row, currentApPayment.DocType, currentApPayment.RefNbr, currentApPayment.NoteID);
					complianceDocument.CheckNumber = currentApPayment.ExtRefNbr;
                    complianceDocument.ApPaymentMethodID = currentApPayment.PaymentMethodID;
                }
            }
        }

        private IEnumerable GetAdjustmentHistory()
        {
            return (IEnumerable) Base.GetType()
                .GetMethod("adjustments_history", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.Invoke(Base, null);
        }

        private string GetVendorName(int? vendorId)
        {
            if (!vendorId.HasValue)
            {
                return null;
            }
            Vendor vendor = PXSelect<Vendor,
                Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>.Select(Base, vendorId);
            return vendor?.AcctName;
        }

        private IEnumerable<ComplianceDocument> GetComplianceDocuments()
        {
            if (Base.Document.Current != null)
            {
				return new PXSelectJoin<ComplianceDocument, LeftJoin<ComplianceDocumentReference,
							On<ComplianceDocumentReference.complianceDocumentReferenceId, Equal<ComplianceDocument.apCheckId>>>,
						Where<ComplianceDocumentReference.type, Equal<Current<APPayment.docType>>,
							And<ComplianceDocumentReference.referenceNumber, Equal<Current<APPayment.refNbr>>>>>(Base)
					.Select(Base.Document.Current.DocType, Base.Document.Current.RefNbr).FirstTableItems.ToList();
			}
            return new PXResultset<ComplianceDocument>().FirstTableItems.ToList();
        }

        public static bool IsActive()
        {
	        return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }
    }
}
