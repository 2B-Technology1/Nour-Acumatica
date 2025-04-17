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
using System.Linq;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.ProjectsIssue.PJ.Descriptor.Attributes;
using PX.Data;
using PX.Objects.PM;
using PX.Objects.PM.ChangeRequest;
using PX.Objects.PJ.ProjectManagement.PM.CacheExtensions;
using PX.Data.WorkflowAPI;

namespace PX.Objects.PJ.ProjectManagement.PM.Services
{
    public class ProjectIssueConversionService : ConversionServiceBase
    {
        public ProjectIssueConversionService(ChangeRequestEntry graph)
            : base(graph)
        {
        }

        public override void UpdateConvertedEntity(PMChangeRequest changeRequest)
        {
            var projectIssue = GetProjectIssue(changeRequest);
            if (projectIssue != null)
            {
                UpdateProjectIssue(projectIssue, null, ProjectIssue.Events.Select(ev => ev.Open));
            }
        }

        public override void SetFieldReadonly(PMChangeRequest changeRequest)
        {
            SetFieldReadOnly<PmChangeRequestExtension.projectIssueID>(changeRequest);
        }

        protected override void ProcessConvertedChangeRequest(PMChangeRequest changeRequest)
        {
            var projectIssue = GetProjectIssue(changeRequest);
            UpdateProjectIssue(projectIssue, changeRequest.NoteID, ProjectIssue.Events.Select(ev => ev.ConvertToChangeRequest));
            CopyFilesToChangeRequest<ProjectIssue>(projectIssue, changeRequest);
        }

        private void UpdateProjectIssue(ProjectIssue projectIssue, Guid? noteId, SelectedEntityEvent<ProjectIssue> piEvent)
        {
            projectIssue.ConvertedTo = noteId;
            RaiseProjectIssueEvent(projectIssue, piEvent);
            Graph.Caches<ProjectIssue>().PersistUpdated(projectIssue);
        }

        private ProjectIssue GetProjectIssue(PMChangeRequest changeRequest)
        {
            var changeRequestExt = PXCache<PMChangeRequest>.GetExtension<PmChangeRequestExtension>(changeRequest);
            return Graph.Select<ProjectIssue>()
                .SingleOrDefault(projectIssue => projectIssue.ProjectIssueId == changeRequestExt.ProjectIssueID);
        }

        protected virtual void RaiseProjectIssueEvent(ProjectIssue projectIssue, 
            SelectedEntityEvent<ProjectIssue> piEvent)
        {
            piEvent.FireOn(Graph, projectIssue);
        }
    }
}
