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

using PX.Objects.PJ.ProjectManagement.Descriptor;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Objects.PJ.ProjectManagement.PJ.Graphs;
using PX.Data;
using PX.Objects.CN.Common.Extensions;

namespace PX.Objects.PJ.ProjectManagement.PJ.Services
{
    /// <summary>
    /// Provides common logic for "Use For" fields of a <see cref="ProjectManagementClass"/>.
    /// </summary>
    internal class ProjectManagementClassUsageService : IProjectManagementClassUsageService
    {
        private readonly ProjectManagementClassMaint graph;
        private readonly IProjectManagementClassDataProvider projectManagementClassDataProvider;

        public ProjectManagementClassUsageService(PXGraph graph)
        {
            this.graph = (ProjectManagementClassMaint) graph;
            projectManagementClassDataProvider = graph.GetService<IProjectManagementClassDataProvider>();
        }

        public void SetEnabledClassUsageIndicators(ProjectManagementClass projectManagementClass)
        {
            if (projectManagementClassDataProvider.DoesAnyRequestForInformationExist(projectManagementClass))
            {
                projectManagementClass.UseForRequestForInformation = true;
                DisableClass<ProjectManagementClass.useForRequestForInformation>();
            }
            if (projectManagementClassDataProvider.DoesAnyProjectIssueExist(projectManagementClass))
            {
                projectManagementClass.UseForProjectIssue = true;
                DisableClass<ProjectManagementClass.useForProjectIssue>();
            }
        }

        public void ValidateUseForProjectIssue(ProjectManagementClass projectManagementClass)
        {
            if (projectManagementClass?.UseForProjectIssue == false &&
                projectManagementClassDataProvider.DoesAnyProjectIssueExist(projectManagementClass))
            {
                throw new PXException(ProjectManagementMessages.ProjectManagementClassAlreadyInUse);
            }
        }

        public void ValidateUseForRequestForInformation(ProjectManagementClass projectManagementClass)
        {
            if (projectManagementClass?.UseForRequestForInformation == false &&
                projectManagementClassDataProvider.DoesAnyRequestForInformationExist(projectManagementClass))
            {
                throw new PXException(ProjectManagementMessages.ProjectManagementClassAlreadyInUse);
            }
        }

        private void DisableClass<TCache>()
            where TCache : IBqlField
        {
            PXUIFieldAttribute.SetEnabled<TCache>(graph.ProjectManagementClassesCurrent.Cache, null, false);
            graph.Delete.SetEnabled(false);
        }
    }
}