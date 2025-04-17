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

using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.CL.Models;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Reports.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PX.Objects.CN.Compliance.CL.Services
{
	internal class EmailLienWaiverService : PrintEmailLienWaiverBaseService, IEmailLienWaiverService
    {
        public EmailLienWaiverService(PXGraph graph)
            : base(graph)
        {
            RecipientEmailDataProvider = graph.GetService<IRecipientEmailDataProvider>();
        }

        private IRecipientEmailDataProvider RecipientEmailDataProvider
        {
            get;
        }

        protected override async System.Threading.Tasks.Task ProcessLienWaiver(NotificationSourceModel notificationSourceModel,
            ComplianceDocument complianceDocument, CancellationToken cancellationToken)
        {
            await base.ProcessLienWaiver(notificationSourceModel, complianceDocument, cancellationToken);
            var notificationRecipients = GetNotificationRecipients(
                notificationSourceModel.NotificationSource, notificationSourceModel.VendorId);
            notificationRecipients.ForEach(nr =>
                SendEmail(nr, notificationSourceModel, complianceDocument));

            PXProcessing.SetProcessed();
		}

        private void SendEmail(NotificationRecipient notificationRecipient,
            NotificationSourceModel notificationSourceModel, ComplianceDocument complianceDocument)
        {
            var email = RecipientEmailDataProvider
                .GetRecipientEmail(notificationRecipient, notificationSourceModel.VendorId);
            if (email != null)
            {
                var report = GetReportInAppropriateFormat(notificationRecipient.Format,
                    notificationSourceModel, complianceDocument);
                var sender = TemplateNotificationGenerator.Create(complianceDocument,
                    notificationSourceModel.NotificationSource.NotificationID);
                sender.MailAccountId = notificationSourceModel.NotificationSource.EMailAccountID;
                sender.RefNoteID = complianceDocument.NoteID;
                sender.To = email;
                sender.AddAttachmentLink(report.ReportFileInfo.UID.GetValueOrDefault());
                sender.Send();
                UpdateLienWaiverProcessedStatus(complianceDocument);
            }
        }

        private LienWaiverReportGenerationModel GetReportInAppropriateFormat(
            string format, NotificationSourceModel notificationSourceModel, ComplianceDocument complianceDocument)
        {
            return format == NotificationFormat.Excel
                ? LienWaiverReportCreator.CreateReport(
                    notificationSourceModel.NotificationSource.ReportID, complianceDocument,
                    notificationSourceModel.IsJointCheck, RenderType.FilterExcel, false)
                : LienWaiverReportGenerationModel;
        }

        private IEnumerable<NotificationRecipient> GetNotificationRecipients(
            NotificationSource notificationSource, int? vendorId)
        {
            var notificationRecipientsQuery = GetNotificationRecipientsQuery(notificationSource, vendorId);
            var notificationRecipientGroups =
                notificationRecipientsQuery.FirstTableItems.GroupBy(nr => nr.ContactID);
            return notificationRecipientGroups.Select(nr => GetAppropriateNotificationRecipient(nr.ToList()))
                .Where(nr => nr.Active == true).ToList();
        }

        private static NotificationRecipient GetAppropriateNotificationRecipient(
            IReadOnlyCollection<NotificationRecipient> notificationRecipients)
        {
            return notificationRecipients.HasAtLeastTwoItems()
                ? notificationRecipients.FirstOrAny(nr => nr.ClassID == null)
                : notificationRecipients.Single();
        }

        private PXResultset<NotificationRecipient> GetNotificationRecipientsQuery(
            NotificationSource notificationSource, int? vendorId)
        {
            var classId = VendorDataProvider.GetVendor(PrintEmailLienWaiversProcess, vendorId).ClassID;
            return SelectFrom<NotificationRecipient>
                .Where<NotificationRecipient.setupID.IsEqual<P.AsGuid>
                    .And<NotificationRecipient.sourceID.IsEqual<P.AsInt>
                        .Or<NotificationRecipient.classID.IsEqual<P.AsString>>>>.View
                .Select(PrintEmailLienWaiversProcess, notificationSource.SetupID, notificationSource.SourceID, classId);
        }
    }
}
