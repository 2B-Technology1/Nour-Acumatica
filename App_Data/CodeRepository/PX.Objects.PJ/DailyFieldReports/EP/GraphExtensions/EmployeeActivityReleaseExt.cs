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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes;
using System.Collections.Generic;

namespace PX.Objects.PJ.DailyFieldReports.EP.GraphExtensions
{
	public class EmployeeActivityReleaseExt : PXGraphExtension<EmployeeActivitiesRelease>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.timeReportingModule>() && PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
		}

		public override void Initialize()
		{
			Base.Activity.SetProcessDelegate(ReleaseActivities);
		}

		private static void ReleaseActivities(List<EPActivityApprove> activities)
		{
			EmployeeActivitiesRelease releaseGraph = (EmployeeActivitiesRelease)PXGraph.CreateInstance(typeof(EmployeeActivitiesRelease));
			List<EPActivityApprove> errorLessActivities = new List<EPActivityApprove>();

			foreach (var activity in activities)
			{
				errorLessActivities.Add(activity);

				var query = new SelectFrom<DailyFieldReportEmployeeActivity>
					.LeftJoin<DailyFieldReport>
					.On<DailyFieldReport.dailyFieldReportId.IsEqual<DailyFieldReportEmployeeActivity.dailyFieldReportId>>
					.Where<DailyFieldReportEmployeeActivity.employeeActivityId
					.IsEqual<@P.AsGuid>.And<DailyFieldReport.status.IsNotEqual<DailyFieldReportStatus.completed>>>.View(releaseGraph);

				var result = query.Select(activity.NoteID);
				foreach (PXResult<DailyFieldReportEmployeeActivity, DailyFieldReport> item in result)
				{
					var dailyFieldReport = (DailyFieldReport)item;
					if (dailyFieldReport != null)
					{
						errorLessActivities.Remove(activity);

						PXProcessing.SetError(activities.IndexOf(activity), new PXException(DailyFieldReportMessages.CompleteDFRBeforeReleasingTimeActivity,
							dailyFieldReport.Status, dailyFieldReport.DailyFieldReportCd));
					}
				}
			}

			releaseGraph.ReleaseAllActivities(errorLessActivities);
		}
	}
}
