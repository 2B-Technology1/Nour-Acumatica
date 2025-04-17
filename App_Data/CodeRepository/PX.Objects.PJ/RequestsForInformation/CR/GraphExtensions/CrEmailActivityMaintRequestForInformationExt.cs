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
using PX.Objects.PJ.Common.CacheExtensions;
using PX.Objects.PJ.Common.GraphExtensions;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.CR.CacheExtensions;
using PX.Objects.PJ.RequestsForInformation.CR.Services;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes;
using PX.Objects.PJ.RequestsForInformation.PJ.Graphs;
using PX.Objects.PJ.RequestsForInformation.PJ.Services;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.PJ.RequestsForInformation.Descriptor;
using PX.SM;
using PX.Objects.CR.Extensions;

namespace PX.Objects.PJ.RequestsForInformation.CR.GraphExtensions
{
    public class CrEmailActivityMaintRequestForInformationExt_LoadEmailSource : PXGraphExtension<CREmailActivityMaint_CRSelectNotificatonTemplateExt, CrEmailActivityMaintExt,
        CREmailActivityMaint>
    {
        private EmailTemplateService emailTemplateService;
        private EmailActivityDataProvider emailActivityDataProvider;

        private int? DefaultNotificationId =>
            new PXSelect<ProjectManagementSetup>(Base).SelectSingle()?.DefaultEmailNotification;

        public override void Initialize()
        {
            emailActivityDataProvider = new EmailActivityDataProvider(Base);
            emailTemplateService = new EmailTemplateService(Base);
            base.Initialize();
        }

        [PXOverride]
        public virtual IEnumerable loadEmailSource(PXAdapter adapter, Func<PXAdapter, IEnumerable> del)
        {
            del(adapter);

            if (Base2.NotificationInfo.Current.NotificationName == RequestForInformationConstants.EmailTemplate.NotificationName &&
                Base1.IsRequestsForInformationEmail())
            {
                UpdateEmailMessage();
            }

            return adapter.Get();
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        private void UpdateEmailMessage()
        {
            var requestForInformation = GetParentRequestForInformation();
            var notificationId = emailActivityDataProvider
                .GetNotification(RequestForInformationConstants.EmailTemplate.NotificationName).NotificationID;
            Base.Message.Current.Body = emailTemplateService.GenerateEmailBodyWithLogo(Base.Message.Current,
                notificationId, requestForInformation.RequestForInformationCd);
        }

        private RequestForInformation GetParentRequestForInformation(PXGraph graph = null)
        {
            var refNoteId = Base.Message.Current.RefNoteID;
            RequestForInformation requestForInformation =
                (RequestForInformation)new EntityHelper(graph ?? Base).GetEntityRow(refNoteId);
            return requestForInformation;
        }

        protected virtual void _(Events.RowSelected<CRSMEmail> args)
        {
            var email = args.Row;
            if (email != null)
            {
                UpdateLayout(args.Cache, email);
            }
        }

        private void UpdateLayout(PXCache cache, CRSMEmail email)
        {
            var isRequestsForInformationEmail = Base1.IsRequestsForInformationEmail();
            var shouldUseNotificationTemplate = DefaultNotificationId != null && isRequestsForInformationEmail;
            Base2.LoadEmailSource.SetEnabled(
                Base2.LoadEmailSource.GetEnabled()
                && !shouldUseNotificationTemplate);
        }
    }

    public class CrEmailActivityMaintRequestForInformationExt : PXGraphExtension<CrEmailActivityMaintExt,
        CREmailActivityMaint>
    {
        private EmailTemplateService emailTemplateService;

        private int? DefaultNotificationId =>
            new PXSelect<ProjectManagementSetup>(Base).SelectSingle()?.DefaultEmailNotification;

        public override void Initialize()
        {
            emailTemplateService = new EmailTemplateService(Base);
            base.Initialize();
        }

        [PXOverride]
        public IEnumerable send(PXAdapter adapter, CrEmailActivityMaintExt.SendDelegate baseMethod)
        {
            if (Base1.IsRequestsForInformationEmail())
            {
	            Base.Persist();
				
				var requestForInformation = GetParentRequestForInformation();

				var graph = PXGraph.CreateInstance<UploadFileMaintenance>();
                ((RequestForInformationEmailFileAttachService) Base1.EmailFileAttachService).GenerateAndAttachReport(requestForInformation, graph);
				Base1.EmailFileAttachService.AttachDrawingLogArchive(graph);
                OpenRequestForInformationIfRequired();
            }
            return baseMethod(adapter);
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        protected virtual void _(Events.RowInserted<CRSMEmail> args)
        {
            var email = args.Row;
            if (email != null)
            {
                UpdateEmailBodyIfRequired(email);
            }
        }

        protected virtual void _(Events.FieldUpdated<CRSMEmail, CrSmEmailExt.recipientNotes> args)
        {
            var email = args.Row;
            if (email != null && Base1.IsRequestsForInformationEmail() && DefaultNotificationId != null)
            {
				var requestForInformation = GetParentRequestForInformation();
				var emailExtension = PXCache<CRSMEmail>.GetExtension<CrSmEmailExt>(email);
                email.Body = emailTemplateService.InsertRecipientNote(email, DefaultNotificationId,
                    requestForInformation.RequestForInformationCd, emailExtension.RecipientNotes);
            }
        }

        private RequestForInformation GetParentRequestForInformation(PXGraph graph = null)
        {
	        var refNoteId = Base.Message.Current.RefNoteID;
	        RequestForInformation requestForInformation =
		        (RequestForInformation) new EntityHelper(graph ?? Base).GetEntityRow(refNoteId);
	        return requestForInformation;
        }

        protected virtual void _(Events.RowSelected<CRSMEmail> args)
        {
            var email = args.Row;
            if (email != null)
            {
                UpdateLayout(args.Cache, email);
            }
        }

        private void UpdateLayout(PXCache cache, CRSMEmail email)
        {
            var isRequestsForInformationEmail = Base1.IsRequestsForInformationEmail();
            var shouldUseNotificationTemplate = DefaultNotificationId != null && isRequestsForInformationEmail;
            PXUIFieldAttribute.SetEnabled<CRSMEmail.body>(cache, email, !shouldUseNotificationTemplate);
            PXUIFieldAttribute.SetVisible<CrSmEmailExt.recipientNotes>(cache, null, shouldUseNotificationTemplate);
        }

        private void UpdateEmailBodyIfRequired(CRSMEmail email)
        {
            if (Base1.IsRequestsForInformationEmail() && DefaultNotificationId != null)
            {
	            var requestForInformation = GetParentRequestForInformation();
				email.Body = emailTemplateService.GenerateEmailBodyWithLogo(email, DefaultNotificationId,
                    requestForInformation.RequestForInformationCd);
            }
        }

        private void OpenRequestForInformationIfRequired()
        {
	        var requestForInformationMaint = PXGraph.CreateInstance<RequestForInformationMaint>();

			var requestForInformation = GetParentRequestForInformation(requestForInformationMaint);

			if (requestForInformation.Incoming == false)
            {
                requestForInformation.Status = RequestForInformationStatusAttribute.OpenStatus;
                requestForInformation.Reason = RequestForInformationReasonAttribute.Unanswered;

				requestForInformationMaint.RequestForInformation.Update(requestForInformation);
				requestForInformationMaint.Persist();
			}
        }
    }
}
