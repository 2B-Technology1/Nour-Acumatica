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

using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Data;

namespace PX.Objects.PJ.ProjectManagement.PJ.Services
{
    public class ProjectManagementClassDataProvider : IProjectManagementClassDataProvider
    {
        private readonly PXGraph graph;

        public ProjectManagementClassDataProvider(PXGraph graph)
        {
            this.graph = graph;
        }

        public bool IsClassInUse(ProjectManagementClass projectManagementClass)
        {
            return DoesAnyRequestForInformationExist(projectManagementClass) ||
                DoesAnyProjectIssueExist(projectManagementClass);
        }

        public bool DoesAnyRequestForInformationExist(ProjectManagementClass projectManagementClass)
        {
            return DoesAnyDocumentExist<RequestForInformation, RequestForInformation.classId>(projectManagementClass);
        }

        public bool DoesAnyProjectIssueExist(ProjectManagementClass projectManagementClass)
        {
            return DoesAnyDocumentExist<ProjectIssue, ProjectIssue.classId>(projectManagementClass);
        }

        public ProjectManagementClass GetProjectManagementClass(string projectManagementClassId)
        {
            return new PXSelect<ProjectManagementClass,
                    Where<ProjectManagementClass.projectManagementClassId,
                        Equal<Required<ProjectManagementClass.projectManagementClassId>>>>(graph)
                .SelectSingle(projectManagementClassId);
        }

        private bool DoesAnyDocumentExist<TCache, TClassId>(ProjectManagementClass projectManagementClass)
            where TCache : class, IBqlTable, new()
            where TClassId : IBqlField
        {
            var query = new PXSelectGroupBy<TCache,
                Where<TClassId, Equal<Required<ProjectManagementClass.projectManagementClassId>>>,
                Aggregate<Count>>(graph);
            return query.Select(projectManagementClass.ProjectManagementClassId).RowCount > 0;
        }
    }
}