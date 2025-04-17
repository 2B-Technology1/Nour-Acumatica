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
using PX.Objects.AP;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.PO;
using System;

namespace PX.Objects.CN.ProjectAccounting.PM.Services
{
	[Obsolete]
    public class ProjectTaskUsageInConstructionValidationService : ProjectTaskUsageValidationServiceBase
    {
        protected override bool IsTaskUsedInCostDocument(int? projectID, int? taskId)
        {
            return IsTaskUsed<POLine, POLine.projectID, POLine.taskID>(projectID, taskId)
                || IsTaskUsed<POReceiptLine, POReceiptLine.projectID, POReceiptLine.taskID>(projectID, taskId)
                || IsTaskUsed<INTran, INTran.projectID, INTran.taskID>(projectID, taskId)
                || IsTaskUsed<APTran, APTran.projectID, APTran.taskID>(projectID, taskId)
                || IsTaskUsed<EPEquipmentSummary, EPEquipmentSummary.projectID, EPEquipmentSummary.projectTaskID>(projectID, taskId)
                || IsTaskUsed<EPEquipmentDetail, EPEquipmentDetail.projectID, EPEquipmentDetail.projectTaskID>(projectID, taskId)
                || IsTaskUsed<EPActivityApprove, EPActivityApprove.projectID, EPActivityApprove.projectTaskID>(projectID, taskId)
                || IsTaskUsed<EPTimeCardSummary, EPTimeCardSummary.projectID, EPTimeCardSummary.projectTaskID>(projectID, taskId)
                || IsTaskUsed<TimeCardMaint.EPTimecardDetail, TimeCardMaint.EPTimecardDetail.projectID, TimeCardMaint.EPTimecardDetail.projectTaskID>(projectID, taskId)
                || IsTaskUsed<EPTimeCardItem, EPTimeCardItem.projectID, EPTimeCardItem.taskID>(projectID, taskId)
                || IsTaskUsed<EPExpenseClaimDetails, EPExpenseClaimDetails.contractID, EPExpenseClaimDetails.taskID>(projectID, taskId)
                || IsTaskUsed<PMChangeOrderBudget, PMChangeOrderBudget.projectID, PMChangeOrderBudget.projectTaskID, PMChangeOrderBudget.type>(projectID, taskId, AccountType.Expense)
                || IsTaskUsed<PMChangeOrderLine, PMChangeOrderLine.projectID, PMChangeOrderLine.taskID>(projectID, taskId);
        }
    }
}
