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
using PX.Data.BQL.Fluent;
using PX.Data.WorkflowAPI;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Common.Helpers;
using PX.Objects.Common.Extensions;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.PJ.Common.CacheExtensions;
using PX.Objects.PJ.Common.Descriptor;
using PX.Objects.PJ.ProjectManagement.Descriptor;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Objects.PJ.ProjectManagement.PJ.Graphs;
using PX.Objects.PJ.ProjectManagement.PJ.Services;
using PX.Objects.PJ.ProjectManagement.PM.CacheExtensions;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.ProjectsIssue.PJ.Descriptor.Attributes;
using PX.Objects.PJ.RequestsForInformation.Descriptor;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes;
using PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes.DocumentSelectorProviders;
using PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Lists;
using PX.Objects.PJ.RequestsForInformation.PJ.Extensions;
using PX.Objects.PJ.RequestsForInformation.PJ.Services;
using PX.Objects.PJ.RequestsForInformation.PM.DAC;
using PX.Objects.PM;
using PX.Objects.PM.ChangeRequest;

using Constants = PX.Objects.PJ.RequestsForInformation.PM.Descriptor.Constants;
using Messages = PX.Objects.CR.Messages;
using PmConstants = PX.Objects.PJ.ProjectManagement.PJ.Descriptor.Constants;

namespace PX.Objects.PJ.RequestsForInformation.PJ.Graphs
{
    public class RequestForInformationMaint : ProjectManagementBaseMaint<RequestForInformationMaint,
        RequestForInformation>
    {
        [PXViewName(CacheNames.RequestForInformation)]
        [PXCopyPasteHiddenFields(
            typeof(RequestForInformation.status),
            typeof(RequestForInformation.reason),
            typeof(RequestForInformation.creationDate))]
		public SelectFrom<RequestForInformation>
			.LeftJoin<PMProject>
				.On<PMProject.contractID.IsEqual<RequestForInformation.projectId>>
			.Where<
				PMProject.contractID.IsNull
				.Or<MatchUserFor<PMProject>>>
			.View RequestForInformation;

		[PXHidden]
        public PXSelect<RequestForInformation,
            Where<RequestForInformation.requestForInformationId,
                Equal<Current<RequestForInformation.requestForInformationId>>>> CurrentRequestForInformation;

        [PXHidden]
        public PXSelect<Contact> Contacts;

        [PXHidden]
        [PXCheckCurrent]
        public PXSetup<ProjectManagementSetup> Setup;

        [PXCopyPasteHiddenView]
        [PXViewName(Messages.Relations)]
        [PXFilterable]
        public RequestForInformationRelationList Relations;

        [PXViewName(RequestForInformationLabels.Email)]
        public PXSelect<CRSMEmail> Email;

        public PXAction<RequestForInformation> RequestForInformationEmail;
        public PXAction<RequestForInformation> RequestForInformationPrint;
        public PXAction<RequestForInformation> ConvertToChangeRequest;
        public PXAction<RequestForInformation> ConvertToOutgoingRequestForInformation;

        private readonly RequestForInformationDataProvider requestForInformationDataProvider;
        private readonly EntityHelper entityHelper;

        public RequestForInformationMaint()
        {
            requestForInformationDataProvider = new RequestForInformationDataProvider(this);
            entityHelper = new EntityHelper(this);

            ConvertToChangeRequest.SetVisible(PXAccess.FeatureInstalled<FeaturesSet.changeRequest>());
        }

        [InjectDependency]
        public IProjectManagementClassDataProvider ProjectManagementClassDataProvider
        {
            get;
            set;
        }

        [InjectDependency]
        public IProjectManagementImpactService ProjectManagementImpactService
        {
            get;
            set;
        }

        [PXUIField(DisplayName = "Email",
            MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
        [PXButton(OnClosingPopup = PXSpecialButtonType.Cancel, CommitChanges = true)]
        public virtual void requestForInformationEmail()
        {
            Persist();
            var emailActivityService = new RequestForInformationEmailActivityService(this);
            var graph = emailActivityService.GetEmailActivityGraph();

			if (graph is CREmailActivityMaint emailActivityGraph && emailActivityGraph.Message.Current != null)
			{
				new RequestForInformationEmailFileAttachService(emailActivityGraph)
				.AttachAllFilesLinkedToRelatedEntitiesExceptProject(emailActivityGraph.Message.Cache, emailActivityGraph.Message.Current);
			}

			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.Popup);
        }

        [PXUIField(DisplayName = "Print",
            MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(CommitChanges = true)]
        public virtual void requestForInformationPrint()
        {
            Persist();
            var parameters = GetReportParameters(CurrentRequestForInformation.Current);

			RequestForInformationContext.CurrentRequestForInformationCD
				= CurrentRequestForInformation.Current?.RequestForInformationCd;

			throw new PXReportRequiredException(parameters, ScreenIds.RequestForInformationReport, null);
        }

        [PXUIField(DisplayName = "Convert to Outgoing RFI",
            MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(CommitChanges = true)]
        public virtual void convertToOutgoingRequestForInformation()
        {
            Persist();
            RedirectToOutgoingRequestForInformation();
        }

        public override IEnumerable ExecuteSelect(string viewName, object[] parameters, object[] searches,
            string[] sortColumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows,
            ref int totalRows)
        {
            if (ShouldOverrideBAccountSelect(viewName, filters))
            {
                var projectContactsView = requestForInformationDataProvider.GetProjectContactsView();
                return projectContactsView.Select(null, parameters, searches, sortColumns, descendings, filters,
                    ref startRow, maximumRows, ref totalRows);
            }
            OverridePrioritiesSelectIfRequired(viewName, sortColumns, ref searches);
            searches = ResetSearchParameterIfRequired(viewName, searches);
            return base.ExecuteSelect(viewName, parameters, searches, sortColumns,
                descendings, filters, ref startRow, maximumRows, ref totalRows);
        }

        public PXAction<RequestForInformation> reopen;
        [PXButton(CommitChanges = true), PXUIField(DisplayName = "Reopen")]
        public virtual IEnumerable Reopen(PXAdapter adapter) => adapter.Get();

        public PXAction<RequestForInformation> close;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Close")]
		protected virtual IEnumerable Close(PXAdapter adapter)
        {
            if (RequestForInformation.Current != null)
            {
                ValidateRequestAnswer(RequestForInformation.Cache, RequestForInformation.Current);
            }
            return adapter.Get();
        }

        #region Entity Event Handlers
        public PXWorkflowEventHandler<RequestForInformation> OnConvertToChangeRequest;
        public PXWorkflowEventHandler<RequestForInformation> OnDeleteChangeRequest;
        #endregion

        public virtual void _(Events.RowSelected<RequestForInformation> args)
        {
            var requestForInformation = args.Row;
            if (requestForInformation != null)
            {
                RequestForInformationEmail.SetEnabled(IsEmailActionEnabled(requestForInformation));
                Delete.SetEnabled(requestForInformation.IsNew());
                RequestForInformationPrint.SetEnabled(IsRequestForInformationSaved(requestForInformation));
                ValidateContact(args.Cache, requestForInformation);
                PXUIFieldAttribute.SetVisible<RequestForInformation.incomingRequestForInformationId>(args.Cache,
                    requestForInformation, requestForInformation.IncomingRequestForInformationId.HasValue);
                PXUIFieldAttribute.SetEnabled<RequestForInformation.incoming>(args.Cache, requestForInformation,
                    !requestForInformation.IncomingRequestForInformationId.HasValue);
            }
        }

        public virtual void _(Events.FieldUpdating<RequestForInformation, RequestForInformation.requestDetails> args)
        {
            if (args.Row != null && IsEmptyText(args.NewValue))
            {
                var errorMessage = string.Format(SharedMessages.FieldIsEmpty,
                    RequestForInformationLabels.RequestDetails);
                var exception = new PXSetPropertyException(errorMessage, PXErrorLevel.RowError);
                args.Cache.RaiseExceptionHandling<RequestForInformation.requestDetails>(args.Row, null, exception);
                args.Cancel = true;
            }
        }

        public virtual void _(Events.FieldUpdated<RequestForInformation, RequestForInformation.requestAnswer> args)
        {
            var requestForInformation = args.Row;
            if (requestForInformation == null || requestForInformation.RequestAnswer == (string) args.OldValue)
            {
                return;
            }
            requestForInformation.LastModifiedRequestAnswer = Accessinfo.BusinessDate;
            NullifyEmptyRequestAnswer(requestForInformation);
        }

        public virtual void _(Events.RowPersisting<RequestForInformation> args)
        {
            var requestForInformation = args.Row;
            if (requestForInformation != null)
            {
                if (args.Operation == PXDBOperation.Delete)
                {
                    DeleteProjectIssue(requestForInformation);
                }
                ProjectManagementImpactService.ClearScheduleAndCostImpactIfRequired(requestForInformation);
                ValidateClassId(args.Cache, requestForInformation);
                ProcessOutgoingRequestForInformationIfRequired(requestForInformation);
                ProcessConvertedRequestForInformationWithContactIfRequired(requestForInformation);
            }
        }

        public virtual void _(Events.RowSelected<CRPMTimeActivity> args)
        {
            SetActivitiesVisibility(args.Cache);
        }

        public bool IsRequestForInformationSaved(RequestForInformation requestForInformation)
        {
            return RequestForInformation.Cache.GetStatus(requestForInformation) != PXEntryStatus.Inserted;
        }

        protected override PMChangeRequest CreateChangeRequest(ChangeRequestEntry graph)
        {
            var changeRequest = base.CreateChangeRequest(graph);
            changeRequest.Description = RequestForInformation.Current.Summary;
            changeRequest.Text = RequestForInformation.Current.RequestDetails;

            var changeRequestExt = PXCache<PMChangeRequest>.GetExtension<PmChangeRequestExtension>(changeRequest);
            changeRequestExt.RFIID = RequestForInformation.Current.RequestForInformationId;
            changeRequestExt.ProjectIssueID = Select<ProjectIssue>()
                .SingleOrDefault(pi => pi.NoteID == RequestForInformation.Current.ConvertedFrom)?.ProjectIssueId;

            return changeRequest;
        }

        private void ProcessConvertedRequestForInformationWithContactIfRequired(RequestForInformation requestForInformation)
        {
            if (!this.HasErrors())
            {
                InsertContactIfRequired(requestForInformation);
                ProcessConvertedRequestForInformationIfRequired(requestForInformation);
            }
        }

        private void DeleteProjectIssue(RequestForInformation requestForInformation)
        {
            var projectIssue = (ProjectIssue) entityHelper
                .GetEntityRow(typeof(ProjectIssue), requestForInformation.ConvertedFrom);
            if (projectIssue != null && requestForInformation.NoteID == projectIssue.ConvertedTo)
            {
                UpdateProjectIssue(projectIssue, null, ProjectIssue.Events.Select(ev => ev.Open));
            }
        }

        private void ProcessConvertedRequestForInformationIfRequired(RequestForInformation requestForInformation)
        {
            if (IsNewConvertedRequestForInformation(requestForInformation))
            {
                var projectIssue = (ProjectIssue) entityHelper
                    .GetEntityRow(typeof(ProjectIssue), requestForInformation.ConvertedFrom);
                if (projectIssue != null && projectIssue.Status == ProjectIssueStatusAttribute.Open)
                {
                    UpdateProjectIssue(projectIssue, requestForInformation.NoteID, ProjectIssue.Events.Select(ev => ev.ConvertToRfi));
                    CopyFilesFromProjectIssueToRequestForInformation(projectIssue, requestForInformation);
                }
            }
        }

        private void CopyFilesFromProjectIssueToRequestForInformation(ProjectIssue projectIssue, RequestForInformation requestForInformation)
        {
            PXNoteAttribute.CopyNoteAndFiles(this.Caches<ProjectIssue>(), projectIssue,
                RequestForInformation.Cache, requestForInformation, false, true);
            Caches[typeof(NoteDoc)].Persist(PXDBOperation.Insert);
        }

        private void UpdateProjectIssue(ProjectIssue projectIssue, Guid? noteId, SelectedEntityEvent<ProjectIssue> piEvent)
        {
            projectIssue.ConvertedTo = noteId;
            RaiseProjectIssueEvent(projectIssue, piEvent);
            this.Caches<ProjectIssue>().PersistUpdated(projectIssue);
        }

        protected virtual void RaiseProjectIssueEvent(ProjectIssue projectIssue,
            SelectedEntityEvent<ProjectIssue> piEvent)
        {
            piEvent.FireOn(this, projectIssue);
        }

        private void RedirectToOutgoingRequestForInformation()
        {
            var graph = CreateInstance<RequestForInformationMaint>();
            var outgoingRequestForInformation = CreateOutgoingRequestForInformation(RequestForInformation.Current);
            var insertedOutgoingRequestForInformation =
                graph.Caches<RequestForInformation>().Insert(outgoingRequestForInformation);
            graph.Caches<RequestForInformation>()
                .SetStatus(insertedOutgoingRequestForInformation, PXEntryStatus.Inserted);
            PXRedirectHelper.TryRedirect(graph, insertedOutgoingRequestForInformation,
                PXRedirectHelper.WindowMode.Same);
        }

        private void ProcessOutgoingRequestForInformationIfRequired(RequestForInformation requestForInformation)
        {
            if (IsNewOutgoingRequestForInformation(requestForInformation))
            {
                var incomingRequestForInformation = RequestForInformationDataProvider.GetRequestForInformation(
                    this, requestForInformation.IncomingRequestForInformationId);
                UpdateIncomingRequestForInformation(incomingRequestForInformation);
                InsertRequestForInformationRelation(incomingRequestForInformation.NoteID, requestForInformation.NoteID);
            }
        }

        private bool IsNewOutgoingRequestForInformation(RequestForInformation requestForInformation)
        {
            return !IsRequestForInformationSaved(requestForInformation) &&
                   requestForInformation.IncomingRequestForInformationId.HasValue;
        }

        private bool IsNewConvertedRequestForInformation(RequestForInformation requestForInformation)
        {
            return !IsRequestForInformationSaved(requestForInformation) &&
                   requestForInformation.ConvertedFrom.HasValue;
        }

        private void InsertRequestForInformationRelation(Guid? documentNoteId, Guid? requestForInformationNoteId)
        {
            if (!DoesRelationEntityExistInView(documentNoteId, requestForInformationNoteId))
            {
                var requestForInformationRelation = CreateRequestForInformationRelation(
                    documentNoteId, requestForInformationNoteId);
                this.Caches<RequestForInformationRelation>().Insert(requestForInformationRelation);
            }
        }

        private bool DoesRelationEntityExistInView(Guid? documentNoteId, Guid? requestForInformationNoteId)
        {
            return Relations.Select().FirstTableItems.Any(x => x.DocumentNoteId == documentNoteId
                && x.RequestForInformationNoteId == requestForInformationNoteId
                && x.Role == RequestForInformationRoleListAttribute.RelatedEntity
                && x.Type == RequestForInformationRelationTypeAttribute.RequestForInformation);
        }

        private void UpdateIncomingRequestForInformation(RequestForInformation requestForInformation)
        {
            requestForInformation.Reason = RequestForInformationReasonAttribute.WaitingInformation;
            this.Caches<RequestForInformation>().PersistUpdated(requestForInformation);
        }

        private void InsertContactIfRequired(RequestForInformation requestForInformation)
        {
            if (requestForInformation.ProjectId != null && requestForInformation.ContactId != null)
            {
                var contactType =
                    requestForInformationDataProvider.GetContact(requestForInformation.ContactId).ContactType;
                var projectEntry = CreateInstance<ProjectEntry>();
                if (contactType == ContactTypesAttribute.Person)
                {
                    InsertProjectContactIfRequired(projectEntry, requestForInformation);
                }
                else
                {
                    InsertEmployeeContactIfRequired(projectEntry, requestForInformation);
                }
            }
        }

        private void InsertProjectContactIfRequired(PXGraph graph, RequestForInformation requestForInformation)
        {
            var existingProjectContact =
                requestForInformationDataProvider.GetProjectContact(graph, requestForInformation);
            if (existingProjectContact == null)
            {
                var projectContact = CreateProjectContact(requestForInformation);
                InsertContact(projectContact);
            }
        }

        private void InsertEmployeeContactIfRequired(PXGraph graph, RequestForInformation requestForInformation)
        {
            var businessAccountId =
                requestForInformationDataProvider.GetBusinessAccountId(requestForInformation.ContactId);
            if (businessAccountId != null)
            {
                var existingEmployeeContact = requestForInformationDataProvider
                    .GetEmployeeContact(graph, requestForInformation.ProjectId, businessAccountId);
                if (existingEmployeeContact == null)
                {
                    var projectEmployee = CreateEmployeeContact(requestForInformation.ProjectId, businessAccountId);
                    InsertContact(projectEmployee);
                }
            }
        }

        private static ProjectContact CreateProjectContact(RequestForInformation requestForInformation)
        {
            return new ProjectContact
            {
                ProjectId = requestForInformation.ProjectId,
                BusinessAccountId = requestForInformation.BusinessAccountId,
                ContactId = requestForInformation.ContactId
            };
        }

        private static EPEmployeeContract CreateEmployeeContact(int? projectId, int? businessAccountId)
        {
            return new EPEmployeeContract
            {
                ContractID = projectId,
                EmployeeID = businessAccountId
            };
        }

        private void InsertContact<TContact>(TContact contact)
            where TContact : class, IBqlTable, new()
        {
            var cache = this.Caches<TContact>();
            cache.Insert(contact);
            cache.Persist(PXDBOperation.Insert);
            cache.Clear();
        }

        private static Dictionary<string, string> GetReportParameters(RequestForInformation document)
        {
            return new Dictionary<string, string>
            {
                [RequestForInformationConstants.Print.RequestForInformationId] = document.RequestForInformationCd,
                [RequestForInformationConstants.Print.EmailId] = null
            };
        }

        private bool IsEmailActionEnabled(RequestForInformation requestForInformation)
        {
            return RequestForInformation.Cache.GetStatus(requestForInformation) != PXEntryStatus.Inserted;
        }

        private void ValidateContact(PXCache cache, RequestForInformation requestForInformation)
        {
            if (requestForInformation.ContactId != null && requestForInformation.BusinessAccountId != null)
            {
                var contact = requestForInformationDataProvider.GetContact(requestForInformation.ContactId);
                if (contact?.BAccountID != requestForInformation.BusinessAccountId)
                {
                    RaiseContactDoesNotMatchWarning(cache, requestForInformation);
                }
            }
        }

        private static void RaiseContactDoesNotMatchWarning(PXCache cache, RequestForInformation requestForInformation)
        {
            var exception = new PXSetPropertyException(
                RequestForInformationMessages.ContactBelongsToAnotherBusinessAccount, PXErrorLevel.Warning);
            cache.RaiseExceptionHandling<RequestForInformation.contactID>(requestForInformation,
                requestForInformation.ContactId, exception);
        }

        private static void SetActivitiesVisibility(PXCache cache)
        {
            PXUIFieldAttribute.SetEnabled(cache, null, false);
            PXUIFieldAttribute.SetEnabled<CRActivityExt.isFinalAnswer>(cache, null, true);
        }

        private static bool IsEmptyText(object textEditValue)
        {
            if (textEditValue is string text)
            {
                var innerText = RichTextEditHelper.GetInnerText(text);
                return innerText.Length == 0;
            }
            return false;
        }

        private static void ValidateRequestAnswer(PXCache cache, RequestForInformation requestForInformation)
        {
            if ((IsEmptyText(requestForInformation.RequestAnswer) || requestForInformation.RequestAnswer == null))
            {
                var message = string.Format(SharedMessages.FieldIsEmpty, RequestForInformationLabels.RequestAnswer);
                cache.RaiseException<RequestForInformation.requestAnswer>(requestForInformation, message);
                throw new PXSetPropertyException<RequestForInformation.requestAnswer>(message);
            }
        }

        private void ValidateClassId(PXCache cache, RequestForInformation requestForInformation)
        {
            if (requestForInformation.ClassId == null)
            {
                cache.RaiseException<RequestForInformation.classId>(requestForInformation,
                    SharedMessages.FieldIsEmpty);
                return;
            }
            var projectManagementClass =
                ProjectManagementClassDataProvider.GetProjectManagementClass(requestForInformation.ClassId);
            if (projectManagementClass == null || projectManagementClass.UseForRequestForInformation == false)
            {
                cache.RaiseException<RequestForInformation.classId>(requestForInformation,
                    ProjectManagementMessages.ProjectManagementClassIsNotActive);
            }
        }

        private bool ShouldOverrideBAccountSelect(string viewName, PXFilterRow[] filters)
        {
            var currentFilterRow = (FilterRow) Caches[typeof(FilterRow)].Current;
            var currentFilterHeader = (FilterHeader) Caches[typeof(FilterHeader)].Current;
            return viewName == Constants.ProjectContactViewName
                   && filters != null
                   && currentFilterRow != null
                   && currentFilterHeader != null
                   && currentFilterHeader.FilterID == currentFilterRow.FilterID
                   && currentFilterHeader.FilterName == Constants.ProjectList;
        }

        /// <summary>
        /// This is temporary solution for Acumatica Support Case = 089889
        /// Reset search parameter to null for custom selector based on <see cref="DocumentSelectorProvider"/>.
        /// </summary>
        private static object[] ResetSearchParameterIfRequired(string viewName, object[] searches)
        {
            var selectorProviders = GetDocumentSelectorProviders();
            return selectorProviders.Contains(viewName)
                ? new[]
                {
                    (object) null
                }
                : searches;
        }

        private static List<string> GetDocumentSelectorProviders()
        {
            return typeof(DocumentSelectorProvider)
                .Assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(DocumentSelectorProvider)) && !t.IsAbstract)
                .Select(t => t.FullName)
                .ToList();
        }

        private static void NullifyEmptyRequestAnswer(RequestForInformation requestForInformation)
        {
            if (IsEmptyText(requestForInformation.RequestAnswer))
            {
                requestForInformation.RequestAnswer = null;
            }
        }

        private RequestForInformation CreateOutgoingRequestForInformation(RequestForInformation requestForInformation)
        {
            var outgoingRequestForInformation = PXCache<RequestForInformation>.CreateCopy(requestForInformation);
            outgoingRequestForInformation.RequestForInformationId = null;
            outgoingRequestForInformation.RequestForInformationCd = null;
            outgoingRequestForInformation.NoteID = null;
            outgoingRequestForInformation.CreationDate = null;
            outgoingRequestForInformation.CreatedById = null;
            outgoingRequestForInformation.Status = null;
            outgoingRequestForInformation.Reason = null;
            outgoingRequestForInformation.ContactId = null;
            outgoingRequestForInformation.BusinessAccountId = null;
            outgoingRequestForInformation.RequestAnswer = null;
            outgoingRequestForInformation.Incoming = false;
            outgoingRequestForInformation.IncomingRequestForInformationId =
                requestForInformation.RequestForInformationId;
            return outgoingRequestForInformation;
        }

        private static RequestForInformationRelation CreateRequestForInformationRelation(Guid? documentNoteId,
            Guid? requestForInformationNoteId)
        {
            return new RequestForInformationRelation
            {
                Role = RequestForInformationRoleListAttribute.RelatedEntity,
                Type = RequestForInformationRelationTypeAttribute.RequestForInformation,
                DocumentNoteId = documentNoteId,
                RequestForInformationNoteId = requestForInformationNoteId
            };
        }

        private static void OverridePrioritiesSelectIfRequired(string viewName, string[] sortColumns, ref object[] searches)
        {
            if (viewName == Constants.ProjectManagementClassPriorityViewName
                && sortColumns.Contains(PmConstants.PriorityNameField))
            {
                var index = sortColumns.FindIndex(x => x == PmConstants.PriorityNameField);
                sortColumns[index] = PmConstants.SortOrderField;
                searches = new[]
                {
                    (object) null
                };
            }
        }
    }
}
