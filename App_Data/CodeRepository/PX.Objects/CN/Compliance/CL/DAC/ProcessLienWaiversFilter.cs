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
using PX.Data.BQL;
using PX.Objects.AP;
using PX.Objects.CN.Compliance.CL.Descriptor.Attributes;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.PM;
using PX.SM;
using static PX.Objects.CN.Compliance.CL.Descriptor.Constants;

namespace PX.Objects.CN.Compliance.CL.DAC
{
    [Serializable]
    [PXCacheName("Process LienWaivers Filter")]
    public class ProcessLienWaiversFilter : IBqlTable, IPrintable
    {
        [PXString]
        [PXUnboundDefault(ProcessLienWaiverActionsAttribute.PrintLienWaiver)]
        [PXUIField(DisplayName = "Action")]
        [ProcessLienWaiverActions]
        public virtual string Action
        {
            get;
            set;
        }

        [Project(typeof(Where<PMProject.nonProject.IsEqual<False>
                .And<PMProject.baseType.IsEqual<CTPRType.project>>
                .And<PMProject.status.IsEqual<ProjectStatus.active>>>),
            DisplayName = "Project")]
        public virtual int? ProjectId
        {
            get;
            set;
        }

        [Vendor(typeof(Search<BAccountR.bAccountID,
                Where<Vendor.type.IsEqual<BAccountType.vendorType>>>),
            DisplayName = "Vendor")]
        public virtual int? VendorId
        {
            get;
            set;
        }

        [PXString]
        [PXUIField(DisplayName = "Category")]
		[PXUnboundDefault(LienWaiverDocumentTypeValues.All)]
        [LienWaiverTypes]
        public virtual string LienWaiverType
        {
            get;
            set;
        }

        [PXDate]
        [PXUIField(DisplayName = "Start Date")]
        public virtual DateTime? StartDate
        {
            get;
            set;
        }

        [PXDate]
        [PXUIField(DisplayName = "End Date")]
        public virtual DateTime? EndDate
        {
            get;
            set;
        }

        [PXBool]
        [PXUIField(DisplayName = "Show Processed")]
        public virtual bool? ShouldShowProcessed
        {
            get;
            set;
        }

        [PXBool]
        [PXUnboundDefault(typeof(FeatureInstalled<FeaturesSet.deviceHub>))]
        [PXUIField(DisplayName = "Print with DeviceHub")]
        public bool? PrintWithDeviceHub
        {
            get;
            set;
        }

        [PXBool]
        [PXUIField(DisplayName = "Define Printer Manually")]
        [PXUIEnabled(typeof(printWithDeviceHub.IsEqual<True>))]
        public bool? DefinePrinterManually
        {
            get;
            set;
        }

        [PXPrinterSelector(DisplayName = "Printer")]
        [PXUIEnabled(typeof(printWithDeviceHub.IsEqual<True>
            .And<definePrinterManually.IsEqual<True>>))]
        public Guid? PrinterID
        {
            get;
            set;
        }

        [PXDBInt(MinValue = 1)]
        [PXDefault(1)]
        [PXFormula(typeof(Selector<printerID, SMPrinter.defaultNumberOfCopies>))]
        [PXUIField(DisplayName = "Number of Copies")]
        [PXUIEnabled(typeof(printWithDeviceHub.IsEqual<True>))]
        public int? NumberOfCopies
        {
            get;
            set;
        }

        public abstract class action : BqlString.Field<action>
        {
        }

        public abstract class projectId : BqlInt.Field<projectId>
        {
        }

        public abstract class vendorId : BqlInt.Field<vendorId>
        {
        }

        public abstract class lienWaiverType : BqlString.Field<lienWaiverType>
        {
        }

        public abstract class startDate : BqlDateTime.Field<startDate>
        {
        }

        public abstract class endDate : BqlDateTime.Field<endDate>
        {
        }

        public abstract class shouldShowProcessed : BqlBool.Field<shouldShowProcessed>
        {
        }

        public abstract class printWithDeviceHub : BqlBool.Field<printWithDeviceHub>
        {
        }

        public abstract class definePrinterManually : BqlBool.Field<definePrinterManually>
        {
        }

        public abstract class printerID : BqlGuid.Field<printerID>
        {
        }

        public abstract class numberOfCopies : BqlInt.Field<numberOfCopies>
        {
        }
    }
}
