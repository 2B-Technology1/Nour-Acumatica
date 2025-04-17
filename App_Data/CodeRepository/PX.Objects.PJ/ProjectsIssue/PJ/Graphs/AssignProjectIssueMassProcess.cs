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
using PX.Objects.PJ.ProjectManagement.PJ.Graphs;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.ProjectsIssue.PJ.Descriptor.Attributes;
using PX.Data;
using PX.SM;
using Messages = PX.Objects.CR.Messages;

namespace PX.Objects.PJ.ProjectsIssue.PJ.Graphs
{
    public class AssignProjectIssueMassProcess : AssignBaseMassProcess<AssignProjectIssueMassProcess,
        ProjectIssue, ProjectManagementSetup.projectIssueAssignmentMapId>
    {
        [PXViewName(Messages.MatchingRecords)]
        [PXFilterable]
        [PXViewDetailsButton(typeof(ProjectIssue))]
        public PXProcessing<ProjectIssue,
                Where<ProjectIssue.status, NotEqual<ProjectIssueStatusAttribute.closed>>> ProjectIssues;
    }
}
