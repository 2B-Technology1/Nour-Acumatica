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
using PX.Data.WorkflowAPI;
using PX.Objects.CS;
using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Objects.PJ.DrawingLogs.PJ.Graphs;
using PX.Objects.PJ.ProjectManagement.PM.CacheExtensions;
using PX.Objects.PM;
using PX.Objects.PM.ChangeRequest;

namespace PX.Objects.PJ.Common.GraphExtensions
{
	public class ChangeRequestEntrySidePanelsExtension : PXGraphExtension<ChangeRequestEntry_Workflow, ChangeRequestEntry>
	{
		public static bool IsActive()
		{
			return true;
		}

		public override void Configure(PXScreenConfiguration configuration)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>() == true)
			{
				WorkflowContext<ChangeRequestEntry, PMChangeRequest> context =
					configuration.GetScreenConfigurationContext<ChangeRequestEntry, PMChangeRequest>();

				context.UpdateScreenConfigurationFor(screen =>
				{
					return screen
						.WithActions(actions =>
						{
							actions.AddNew("DrawingLogs",
								config => config.IsSidePanelScreen(
									sidePanelAction =>
										sidePanelAction.NavigateToScreen<DrawingLogsMaint>()
											.WithIcon("description")
											.WithAssignments(containerFiller =>
											{
												containerFiller.Add<DrawingLogFilter.projectId>(c => c.SetFromField<PMChangeRequest.projectID>());
												containerFiller.Add<DrawingLogFilter.isCurrentOnly>(c => c.SetFromValue(false));
											}
											))
								.DisplayName("Drawing Logs"));
							actions.AddNew("ProjectIssues",
								config => config.IsSidePanelScreen(
									sidePanelAction =>
										sidePanelAction.NavigateToScreen("PJ3020PL")
											.WithIcon("details")
											.WithAssignments(containerFiller =>
											{
												containerFiller.Add("ProjectIssue_projectIssueCd", c => c.SetFromField<PmChangeRequestExtension.projectIssueID>());
											}
											))
								.DisplayName("Project Issues"));
							actions.AddNew("RequestsForInformation",
								config => config.IsSidePanelScreen(
									sidePanelAction =>
										sidePanelAction.NavigateToScreen("PJ3010PL")
											.WithIcon("import_contacts")
											.WithAssignments(containerFiller =>
											{
												containerFiller.Add("RFI_requestForInformationCd", c => c.SetFromField<PmChangeRequestExtension.rfiID>());
											}
											))
								.DisplayName("Requests For Information"));
						});
				});
			}
		}
	}
}
