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
using PX.Objects.PJ.ProjectManagement.Descriptor;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Objects.PJ.ProjectManagement.PM.CacheExtensions;
using PX.Data;
using PX.Objects.CN.Common.DAC;
using PX.Objects.CR;
using PX.Objects.PM;
using PX.Objects.PM.ChangeRequest;
using Messages = PX.Objects.CR.Messages;

namespace PX.Objects.PJ.ProjectManagement.PJ.Graphs
{
    public abstract class ProjectManagementBaseMaint<TGraph, TProjectManagementEntity>
        : PXGraph<TGraph, TProjectManagementEntity>
        where TGraph : ProjectManagementBaseMaint<TGraph, TProjectManagementEntity>
        where TProjectManagementEntity : class, IBqlTable, IProjectManagementDocumentBase, new()
    {
        public CRAttributeList<TProjectManagementEntity> Attributes;
        public PXSelect<TProjectManagementEntity> ProjectManagementEntity;

		[PXSuppressActionValidation]
        [PXUIField(DisplayName = "Convert to Change Request", MapEnableRights = PXCacheRights.Update,
            MapViewRights = PXCacheRights.Select)]
        [PXButton(CommitChanges = true)]
        public virtual void convertToChangeRequest()
        {
            Persist();
            CheckChangeOrderWorkflow();
            var graph = CreateInstance<ChangeRequestEntry>();
            graph.Document.Current = CreateChangeRequest(graph);
            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.InlineWindow);
        }

        protected virtual PMChangeRequest CreateChangeRequest(ChangeRequestEntry graph)
        {
            var changeRequest = graph.Document.Insert();
			changeRequest.ProjectID = ProjectManagementEntity.Current.ProjectId;

			PMProject project = PMProject.PK.Find(graph, ProjectManagementEntity.Current.ProjectId);
			changeRequest.CustomerID = project.CustomerID;

			var extension = PXCache<PMChangeRequest>.GetExtension<PmChangeRequestExtension>(changeRequest);
            extension.ConvertedFrom = ProjectManagementEntity.Current.GetType().Name;
            return changeRequest;
        }

        private void CheckChangeOrderWorkflow()
        {
            var project = Select<PMProject>().SingleOrDefault(p => p.ContractID == ProjectManagementEntity.Current.ProjectId);
            if (project?.ChangeOrderWorkflow != true)
            {
                throw new Exception(ProjectManagementMessages.EnableChangeOrderWorkflowForTheProject);
            }
        }
    }
}
