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
using System.Collections;
using PX.Data;
using PX.Data.WorkflowAPI;
using PX.Objects.CR;
using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Objects.PJ.DrawingLogs.PJ.Graphs;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.ProjectsIssue.PJ.Descriptor.Attributes;
using PX.Objects.PJ.Submittals.PJ.DAC;

namespace PX.Objects.PJ.ProjectsIssue.PJ.Graphs
{
	public partial class ProjectIssueMaint_Workflow : PXGraphExtension<ProjectIssueMaint>
	{
		protected static bool ChangeRequestIsActive() => PXAccess.FeatureInstalled<CS.FeaturesSet.changeRequest>();

		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<ProjectIssueMaint, ProjectIssue>());

		protected static void Configure(WorkflowContext<ProjectIssueMaint, ProjectIssue> context)
		{
			var processingCategory = context.Categories.CreateNew(PX.Objects.PM.ToolbarCategory.ActionCategoryNames.Processing,
				category => category.DisplayName(PX.Objects.PM.ToolbarCategory.ActionCategory.Processing));
			var printingAndEmailingCategory = context.Categories.CreateNew(PX.Objects.PM.ToolbarCategory.ActionCategoryNames.PrintingAndEmailing,
				category => category.DisplayName(PX.Objects.PM.ToolbarCategory.ActionCategory.PrintingAndEmailing));

			var reopen = context.ActionDefinitions
				.CreateExisting<ProjectIssueMaint_Workflow>(g => g.reopen, a => a
					.InFolder(processingCategory));

			var close = context.ActionDefinitions
				.CreateExisting<ProjectIssueMaint_Workflow>(g => g.close, a => a
					.InFolder(processingCategory)
					.PlaceAfter(reopen));

			context.AddScreenConfigurationFor(screen =>
				screen
					.StateIdentifierIs<ProjectIssue.status>()
					.AddDefaultFlow(flow => flow
						.WithFlowStates(fss =>
						{
							fss.Add<ProjectIssueStatusAttribute.open>(flowState =>
							{
								return flowState
									.IsInitial()
									.WithActions(actions =>
									{
										actions.Add(close, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
										actions.Add(g => g.ConvertToRfi, c => c.IsDuplicatedInToolbar());
										actions.Add(g => g.ConvertToChangeRequest, c => c.IsDuplicatedInToolbar());
										actions.Add(g => g.Print, c => c.IsDuplicatedInToolbar());
									})
									.WithEventHandlers(handlers =>
									{
										handlers.Add(g => g.OnConvertToChangeRequest);
										handlers.Add(g => g.OnConvertToRfi);
									});
							});
							fss.Add<ProjectIssueStatusAttribute.closed>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(reopen, c => c.IsDuplicatedInToolbar());
										actions.Add(g => g.Print, c => c.IsDuplicatedInToolbar());
									})
									.WithFieldStates(fields => DisableFields(fields));
							});
							fss.Add<ProjectIssueStatusAttribute.convertedToRfi>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(reopen, c => c.IsDuplicatedInToolbar());
										actions.Add(g => g.Print, c => c.IsDuplicatedInToolbar());
									})
									.WithEventHandlers(handlers =>
										handlers.Add(g => g.OnOpen))
									.WithFieldStates(fields => DisableFields(fields));
							});
							fss.Add<ProjectIssueStatusAttribute.convertedToChangeRequest>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(reopen, c => c.IsDuplicatedInToolbar());
										actions.Add(g => g.Print, c => c.IsDuplicatedInToolbar());
									})
									.WithEventHandlers(handlers =>
										handlers.Add(g => g.OnOpen))
									.WithFieldStates(fields => DisableFields(fields));
							});
						})
						.WithTransitions(transitions =>
						{
							transitions.AddGroupFrom<ProjectIssueStatusAttribute.open>(ts =>
							{
								ts.Add(t => t
									.To<ProjectIssueStatusAttribute.closed>()
									.IsTriggeredOn(close));
								ts.Add(t => t
									.To<ProjectIssueStatusAttribute.convertedToChangeRequest>()
									.IsTriggeredOn(g => g.OnConvertToChangeRequest));
								ts.Add(t => t
									.To<ProjectIssueStatusAttribute.convertedToRfi>()
									.IsTriggeredOn(g => g.OnConvertToRfi));
							});
							transitions.AddGroupFrom<ProjectIssueStatusAttribute.closed>(ts =>
							{
								ts.Add(t => t
									.To<ProjectIssueStatusAttribute.open>()
									.IsTriggeredOn(reopen));
							});
							transitions.AddGroupFrom<ProjectIssueStatusAttribute.convertedToChangeRequest>(ts =>
							{
								ts.Add(t => t
									.To<ProjectIssueStatusAttribute.open>()
									.IsTriggeredOn(reopen));
								ts.Add(t => t
									.To<ProjectIssueStatusAttribute.open>()
									.IsTriggeredOn(g => g.OnOpen));
							});
							transitions.AddGroupFrom<ProjectIssueStatusAttribute.convertedToRfi>(ts =>
							{
								ts.Add(t => t
									.To<ProjectIssueStatusAttribute.open>()
									.IsTriggeredOn(reopen));
								ts.Add(t => t
									.To<ProjectIssueStatusAttribute.open>()
									.IsTriggeredOn(g => g.OnOpen));
							});
						}))
					.WithActions(actions =>
					{
						actions.Add(reopen);
						actions.Add(close);
						actions.Add(g => g.ConvertToRfi, c => c
							.InFolder(processingCategory));
						actions.Add(g => g.ConvertToChangeRequest, c => c
							.InFolder(processingCategory));
						actions.Add(g => g.Print, c => c
							.InFolder(printingAndEmailingCategory));

						actions.AddNew("DrawingLogs",
							config => config.IsSidePanelScreen(
								sidePanelAction =>
									sidePanelAction.NavigateToScreen<DrawingLogsMaint>()
										.WithIcon("description")
										.WithAssignments(containerFiller =>
										{
											containerFiller.Add<DrawingLogFilter.projectId>(c => c.SetFromField<ProjectIssue.projectId>());
											containerFiller.Add<DrawingLogFilter.projectIssueID>(c => c.SetFromField<ProjectIssue.projectIssueId>());
											containerFiller.Add<DrawingLogFilter.isCurrentOnly>(c => c.SetFromValue(false));
										}
										))
							.DisplayName("Drawing Logs"));
					})
					.WithHandlers(handlers =>
					{
						handlers.Add(handler => handler
							.WithTargetOf<ProjectIssue>()
							.OfEntityEvent<ProjectIssue.Events>(e => e.ConvertToChangeRequest)
							.Is(g => g.OnConvertToChangeRequest)
							.UsesTargetAsPrimaryEntity());
						handlers.Add(handler => handler
							.WithTargetOf<ProjectIssue>()
							.OfEntityEvent<ProjectIssue.Events>(e => e.ConvertToRfi)
							.Is(g => g.OnConvertToRfi)
							.UsesTargetAsPrimaryEntity());
						handlers.Add(handler => handler
							.WithTargetOf<ProjectIssue>()
							.OfEntityEvent<ProjectIssue.Events>(e => e.Open)
							.Is(g => g.OnOpen)
							.UsesTargetAsPrimaryEntity());
					})
					.WithCategories(categories =>
					{
						categories.Add(processingCategory);
						categories.Add(printingAndEmailingCategory);
					}));
		}

		private static void DisableFields(BoundedTo<ProjectIssueMaint, ProjectIssue>.FieldState.IContainerFillerFields fields)
		{
			fields.AddTable<CRPMTimeActivity>(table => table.IsDisabled());
			fields.AddField<ProjectIssue.summary>(field => field.IsDisabled());
			fields.AddField<ProjectIssue.projectId>(field => field.IsDisabled());
			fields.AddField<ProjectIssue.projectTaskId>(field => field.IsDisabled());
			fields.AddField<ProjectIssue.classId>(field => field.IsDisabled());
			fields.AddField<ProjectIssue.ownerID>(field => field.IsDisabled());
			fields.AddField<ProjectIssue.workgroupID>(field => field.IsDisabled());
			fields.AddField<ProjectIssue.description>(field => field.IsDisabled());
			fields.AddField<ProjectIssue.dueDate>(field => field.IsDisabled());
			fields.AddField<ProjectIssue.resolvedOn>(field => field.IsDisabled());
			fields.AddField<ProjectIssue.creationDate>(field => field.IsDisabled());
			fields.AddField<ProjectIssue.createdById>(field => field.IsDisabled());
			fields.AddField<ProjectIssue.priorityId>(field => field.IsDisabled());
			fields.AddField<ProjectIssue.isScheduleImpact>(field => field.IsDisabled());
			fields.AddField<ProjectIssue.scheduleImpact>(field => field.IsDisabled());
			fields.AddField<ProjectIssue.isCostImpact>(field => field.IsDisabled());
			fields.AddField<ProjectIssue.costImpact>(field => field.IsDisabled());
			fields.AddField<ProjectIssue.projectIssueTypeId>(field => field.IsDisabled());
		}

		public PXAction<ProjectIssue> reopen;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Reopen")]
		protected virtual IEnumerable Reopen(PXAdapter adapter) => adapter.Get();

		public PXAction<ProjectIssue> close;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Close")]
		protected virtual IEnumerable Close(PXAdapter adapter) => adapter.Get();
	}
}
