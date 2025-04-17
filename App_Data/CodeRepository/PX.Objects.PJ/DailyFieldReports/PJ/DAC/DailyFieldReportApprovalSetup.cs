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
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CS;
using PX.Objects.EP;

namespace PX.Objects.PJ.DailyFieldReports.PJ.DAC
{
    [PXCacheName("Daily Field Report Approval Setup")]
    public class DailyFieldReportApprovalSetup : ProjectManagementSetup, IAssignedMap
    {
        [PXDBInt(BqlField = typeof(dailyFieldReportApprovalMapId))]
        public virtual int? AssignmentMapID
        {
            get;
            set;
        }

        [PXDBInt(BqlField = typeof(pendingApprovalNotification))]
        public virtual int? AssignmentNotificationID
        {
            get;
            set;
        }

        [PXBool]
        public virtual bool? IsActive => PXAccess.FeatureInstalled<FeaturesSet.approvalWorkflow>();

        [PXNote]
        public override Guid? NoteID
        {
            get;
            set;
        }

        public abstract class assignmentMapID : BqlInt.Field<assignmentMapID>
        {
        }

        public abstract class assignmentNotificationID : BqlInt.Field<assignmentNotificationID>
        {
        }

        public abstract class isActive : BqlBool.Field<isActive>
        {
        }

        public abstract class noteId : BqlGuid.Field<noteId>
        {
        }
    }
}