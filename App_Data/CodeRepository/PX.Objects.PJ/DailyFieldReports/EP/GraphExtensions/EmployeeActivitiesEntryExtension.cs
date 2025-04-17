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
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.EP;

namespace PX.Objects.PJ.DailyFieldReports.EP.GraphExtensions
{
    public class EmployeeActivitiesEntryExtension : PXGraphExtension<EmployeeActivitiesEntry>
    {
        [PXCopyPasteHiddenView]
        public SelectFrom<DailyFieldReportEmployeeActivity>.View DailyFieldReportEmployeeActivities;

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        public virtual void _(Events.RowSelected<EPActivityApprove> args)
        {
            if (args.Row is EPActivityApprove activity && args.Cache.GetEnabled<EPActivityApprove.projectID>(activity))
            {
                var hasRelatedDailyFieldReport = HasRelatedDailyFieldReport(activity.NoteID);
                PXUIFieldAttribute.SetEnabled<EPActivityApprove.projectID>(args.Cache, activity,
                    !hasRelatedDailyFieldReport);
            }
        }

        public virtual void _(Events.RowDeleting<EPActivityApprove> args)
        {
            if (args.Row is EPActivityApprove activityApprove &&
                activityApprove.TimeCardCD == null && HasRelatedDailyFieldReport(activityApprove.NoteID))
            {
                var message = string.Format(DailyFieldReportMessages.EntityCannotBeDeletedBecauseItIsLinked,
                    DailyFieldReportEntityNames.EmployeeTimeActivity.Capitalize());
                Base.Activity.View.Ask(message, MessageButtons.OK);
                args.Cancel = true;
            }
        }

        private bool HasRelatedDailyFieldReport(Guid? employeeActivityId)
        {
            return SelectFrom<DailyFieldReportEmployeeActivity>
                .Where<DailyFieldReportEmployeeActivity.employeeActivityId.IsEqual<P.AsGuid>>.View
                .Select(Base, employeeActivityId).Any();
        }
    }
}