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

using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Objects.CN.ProjectAccounting.PM.Services;

namespace PX.Objects.PJ.ProjectAccounting.PM.Services
{
    public class ProjectTaskTypeUsageInProjectManagementValidationService : ProjectTaskTypeUsageValidationServiceBase
    {
        protected override bool IsTaskUsedInCostDocument(int? projectID, int? taskId)
        {
            return IsTaskUsed<RequestForInformation, RequestForInformation.projectId, RequestForInformation.projectTaskId>(projectID, taskId)
                || IsTaskUsed<ProjectIssue, ProjectIssue.projectId, ProjectIssue.projectTaskId>(projectID, taskId)
                || IsTaskUsed<DrawingLog, DrawingLog.projectId, DrawingLog.projectTaskId>(projectID, taskId);
        }
    }
}
