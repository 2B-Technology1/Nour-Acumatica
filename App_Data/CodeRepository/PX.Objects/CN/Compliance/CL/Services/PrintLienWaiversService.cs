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

using System.Collections.Generic;
using System.Threading;
using PX.Data;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.CL.Descriptor;
using PX.Objects.CN.Compliance.CL.Models;
using PX.Objects.CR;
using PX.SM;

namespace PX.Objects.CN.Compliance.CL.Services
{
    internal class PrintLienWaiversService : PrintEmailLienWaiverBaseService, IPrintLienWaiversService
    {
        private PXReportRequiredException reportRequiredException;

        public PrintLienWaiversService(PXGraph graph)
            : base(graph)
        {
        }

        public override async System.Threading.Tasks.Task Process(List<ComplianceDocument> complianceDocuments, CancellationToken cancellationToken)
        {
            await base.Process(complianceDocuments, cancellationToken);
            if (reportRequiredException != null)
            {
                reportRequiredException.Mode = PXBaseRedirectException.WindowMode.New;
                throw reportRequiredException;
            }
        }

        protected override async System.Threading.Tasks.Task ProcessLienWaiver(NotificationSourceModel notificationSourceModel,
            ComplianceDocument complianceDocument, CancellationToken cancellationToken)
        {
            await base.ProcessLienWaiver(notificationSourceModel, complianceDocument, cancellationToken);
            await ConfigurePrintActionParameters(notificationSourceModel.NotificationSource.ReportID,
                notificationSourceModel.NotificationSource.NBranchID, cancellationToken);
            UpdateLienWaiverProcessedStatus(complianceDocument);

            PXProcessing.SetProcessed();
		}

        private async System.Threading.Tasks.Task ConfigurePrintActionParameters(string reportId, int? branchId, CancellationToken cancellationToken)
        {
            reportRequiredException = PXReportRequiredException.CombineReport(
                reportRequiredException, reportId,
                LienWaiverReportGenerationModel.Parameters, false);
            var reportParametersForDeviceHub = GetReportParametersForDeviceHub();
            var reportToPrint = new Dictionary<PrintSettings, PXReportRequiredException>();
            reportToPrint = SMPrintJobMaint.AssignPrintJobToPrinter(
                reportToPrint, reportParametersForDeviceHub,
                PrintEmailLienWaiversProcess.Filter.Current,
                new NotificationUtility(PrintEmailLienWaiversProcess).SearchPrinter,
                Constants.ComplianceNotification.LienWaiverNotificationSourceCd,
                reportId, reportId, branchId);
            await SMPrintJobMaint.CreatePrintJobGroups(reportToPrint, cancellationToken);
        }

        private Dictionary<string, string> GetReportParametersForDeviceHub()
        {
            return new Dictionary<string, string>
            {
                [Constants.LienWaiverReportParameters.DeviceHubComplianceDocumentId] = LienWaiverReportGenerationModel
                    .Parameters[Constants.LienWaiverReportParameters.ComplianceDocumentId],
                [Constants.LienWaiverReportParameters.IsJointCheck] = LienWaiverReportGenerationModel
                    .Parameters[Constants.LienWaiverReportParameters.IsJointCheck]
            };
        }
    }
}
