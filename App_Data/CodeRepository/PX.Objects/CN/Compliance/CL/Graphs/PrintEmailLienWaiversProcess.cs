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
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Common.Helpers;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.CL.Descriptor.Attributes;
using PX.Objects.CN.Compliance.CL.Services;
using PX.Objects.CN.Compliance.Descriptor;
using PX.Objects.CS;
using System;
using PX.Concurrency;
using static PX.Objects.CN.Compliance.CL.Descriptor.Constants;

namespace PX.Objects.CN.Compliance.CL.Graphs
{
    public class PrintEmailLienWaiversProcess : PXGraph<PrintEmailLienWaiversProcess>
    {
        public PXCancel<ProcessLienWaiversFilter> Cancel;

        public PXFilter<ProcessLienWaiversFilter> Filter;
                
        [PXFilterable]
        public PXFilteredProcessingJoin<ComplianceDocument, ProcessLienWaiversFilter,
           LeftJoin<ComplianceAttribute,
               On<ComplianceDocument.documentTypeValue.IsEqual<ComplianceAttribute.attributeId>>,
           InnerJoin<ComplianceAttributeType,
              On<ComplianceDocument.documentType.IsEqual<ComplianceAttributeType.complianceAttributeTypeID>>,
           LeftJoin<ComplianceDocumentAPDocumentReference,
            On<ComplianceDocument.billID.IsEqual<ComplianceDocumentAPDocumentReference.complianceDocumentReferenceId>>,
           LeftJoin<ComplianceDocumentAPPaymentReference,
            On<ComplianceDocument.apCheckId.IsEqual<ComplianceDocumentAPPaymentReference.complianceDocumentReferenceId>>>>>>,
           Where<ComplianceAttributeType.type.IsEqual<ComplianceDocumentType.lienWaiver>
               .And<ComplianceDocument.projectID.IsEqual<ProcessLienWaiversFilter.projectId.FromCurrent>
                   .Or<ProcessLienWaiversFilter.projectId.FromCurrent.IsNull>>
               .And<ComplianceDocument.vendorID.IsEqual<ProcessLienWaiversFilter.vendorId.FromCurrent>
                   .Or<ProcessLienWaiversFilter.vendorId.FromCurrent.IsNull>>
               .And<ComplianceAttribute.value.IsEqual<ProcessLienWaiversFilter.lienWaiverType.FromCurrent>
                   .Or<ProcessLienWaiversFilter.lienWaiverType.FromCurrent.IsNull>
				   .Or<ProcessLienWaiversFilter.lienWaiverType.FromCurrent.IsEqual<LienWaiverDocumentTypeValues.all>>>
               .And<ComplianceDocument.creationDate.IsGreaterEqual<ProcessLienWaiversFilter.startDate.FromCurrent>
                   .Or<ProcessLienWaiversFilter.startDate.FromCurrent.IsNull>>
               .And<ComplianceDocument.creationDate.IsLessEqual<ProcessLienWaiversFilter.endDate.FromCurrent>
                   .Or<ProcessLienWaiversFilter.endDate.FromCurrent.IsNull>>
               .And<ComplianceDocument.isProcessed.IsEqual<False>
                   .Or<ComplianceDocument.isProcessed.IsNull>
                   .Or<ProcessLienWaiversFilter.shouldShowProcessed.FromCurrent.IsEqual<True>>>>> LienWaivers;

        public PrintEmailLienWaiversProcess()
        {            
        }

        [InjectDependency]
        internal IPrintLienWaiversService PrintLienWaiversService
        {
            get;
            set;
        }

        [InjectDependency]
        internal IEmailLienWaiverService EmailLienWaiverService
        {
            get;
            set;
        }

        protected virtual void _(Events.RowInserted<ProcessLienWaiversFilter> args)
        {
            var filter = args.Row;
            if (filter != null)
            {
                filter.StartDate = Accessinfo.BusinessDate;
                filter.EndDate = Accessinfo.BusinessDate;
            }
        }

        protected virtual void _(Events.RowSelected<ProcessLienWaiversFilter> args)
        {
            var filter = args.Row;
            if (filter?.Action != null)
            {
                InitializeProcessDelegate(filter.Action);
                SetPrintSettingFieldsVisibility(args.Cache, filter);
            }
        }

		protected virtual void _(Events.RowSelected<ComplianceDocument> args)
        {
            var complianceDocument = args.Row;
            if (complianceDocument != null && !PrintLienWaiversService.IsLienWaiverValid(complianceDocument))
            {
                args.Cache.RaiseException<ComplianceDocument.documentTypeValue>(complianceDocument,
                    ComplianceMessages.LienWaiver.DocumentTypeOptionVendorAndProjectMustBeSpecified, errorLevel:
                    PXErrorLevel.RowWarning);
            }
        }

        private void InitializeProcessDelegate(string action)
        {
            if (action == ProcessLienWaiverActionsAttribute.PrintLienWaiver)
            {
				LienWaivers.SetAsyncProcessDelegate((l, ct) => PrintLienWaiversService.Process(l, ct));
            }
            else
            {
                LienWaivers.SetAsyncProcessDelegate((l, ct)=>EmailLienWaiverService.Process(l, ct));
            }
        }

        private static void SetPrintSettingFieldsVisibility(PXCache cache, ProcessLienWaiversFilter filter)
        {
            var shouldShowPrintSettings = PXAccess.FeatureInstalled<FeaturesSet.deviceHub>() &&
                filter.Action == ProcessLienWaiverActionsAttribute.PrintLienWaiver;
            PXUIFieldAttribute.SetVisible<ProcessLienWaiversFilter
                .printWithDeviceHub>(cache, filter, shouldShowPrintSettings);
            PXUIFieldAttribute.SetVisible<ProcessLienWaiversFilter
                .definePrinterManually>(cache, filter, shouldShowPrintSettings);
            PXUIFieldAttribute.SetVisible<ProcessLienWaiversFilter.printerID>(cache, filter, shouldShowPrintSettings);
            PXUIFieldAttribute.SetVisible<ProcessLienWaiversFilter.numberOfCopies>(cache, filter,
                shouldShowPrintSettings);
        }

        #region Local Types

        [PXCacheName("Compliance AP Document Reference")]
        public class ComplianceDocumentAPDocumentReference : ComplianceDocumentReference
        {
            #region ComplianceDocumentReferenceId
            public new abstract class complianceDocumentReferenceId : PX.Data.BQL.BqlGuid.Field<complianceDocumentReferenceId>
            {
            }
            #endregion

            #region Type
            public new abstract class type : PX.Data.BQL.BqlString.Field<type>
            {
            }

            [PXDBString]
            [Objects.AP.APDocType.List]
            [PXUIField(DisplayName = "AP Document Type")]
            public override string Type
            {
                get;
                set;
            }
            #endregion

            #region ReferenceNumber

            public new abstract class referenceNumber : PX.Data.BQL.BqlString.Field<referenceNumber>
            {
            }

            [PXDBString]
            [PXUIField(DisplayName = "AP Document Ref. Nbr.")]
            public override string ReferenceNumber
            {
                get;
                set;
            }
            #endregion           
        }

        [PXCacheName("Compliance Document AP Payment Reference")]
        public class ComplianceDocumentAPPaymentReference : ComplianceDocumentReference
        {
            #region ComplianceDocumentReferenceId
            public new abstract class complianceDocumentReferenceId : PX.Data.BQL.BqlGuid.Field<complianceDocumentReferenceId>
            {
            }
            #endregion

            #region Type
            public new abstract class type : PX.Data.BQL.BqlString.Field<type>
            {
            }

            [PXDBString]
            [Objects.AP.APDocType.List]
            [PXUIField(DisplayName = "AP Payment Type")]
            public override string Type
            {
                get;
                set;
            }
            #endregion

            #region ReferenceNumber

            public new abstract class referenceNumber : PX.Data.BQL.BqlString.Field<referenceNumber>
            {
            }

            [PXDBString]
            [PXUIField(DisplayName = "AP Payment Ref. Nbr.")]
            public override string ReferenceNumber
            {
                get;
                set;
            }
            #endregion
        }

        #endregion
    }
}
