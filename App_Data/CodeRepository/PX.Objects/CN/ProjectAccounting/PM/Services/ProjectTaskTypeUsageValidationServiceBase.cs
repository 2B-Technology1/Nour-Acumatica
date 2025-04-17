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

using PX.Data;
using PX.Objects.AR;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.CacheExtensions;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor;
using PX.Objects.GL;
using PX.Objects.PM;
using PmMessages = PX.Objects.PM.Messages;

namespace PX.Objects.CN.ProjectAccounting.PM.Services
{
    public abstract class ProjectTaskTypeUsageValidationServiceBase : ProjectTaskValidationServiceBase
    {
        public void ValidateProjectTaskType(PXCache cache, PMTask projectTask)
        {
            var status = cache.GetStatus(projectTask);
            if (status == PXEntryStatus.Updated)
            {
                if (projectTask.Type == ProjectTaskType.Cost
                    && IsTaskUsedInRevenueDocument(projectTask.ProjectID, projectTask.TaskID))
                {
                    cache.RaiseException<PMTask.type>(projectTask,
                        string.Format(ProjectAccountingMessages.TaskTypeCannotBeChanged, PmMessages.TaskType_Revenue),
                        projectTask.Type);
                }
                if (projectTask.Type == ProjectTaskType.Revenue
                    && IsTaskUsedInCostDocument(projectTask.ProjectID, projectTask.TaskID))
                {
                    cache.RaiseException<PMTask.type>(projectTask,
                        string.Format(ProjectAccountingMessages.TaskTypeCannotBeChanged, PmMessages.TaskType_Expense),
                        projectTask.Type);
                }
            }
        }

        protected override bool IsTaskUsedInRevenueDocument(int? projectID, int? taskId)
        {
            return IsTaskUsed<PMBudget, PMBudget.projectID, PMBudget.projectTaskID, PMBudget.type>(projectID, taskId, AccountType.Income)
                || IsTaskUsed<PMChangeOrderBudget, PMChangeOrderBudget.projectID, PMChangeOrderBudget.projectTaskID,
                    PMChangeOrderBudget.type>(projectID, taskId, AccountType.Income)
                || IsTaskUsed<ARTran, ARTran.projectID, ARTran.taskID>(projectID, taskId)
                || IsTaskUsed<PMProformaProgressLine, PMProformaLine.projectID, PMProformaLine.taskID>(projectID, taskId)
                || IsTaskUsed<PMProformaTransactLine, PMProformaLine.projectID, PMProformaLine.taskID>(projectID, taskId);
        }
    }
}
