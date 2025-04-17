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
using PX.Data;
using PX.Data.WorkflowAPI;
using System.Collections;

namespace PX.Objects.PM
{
	public partial class ProjectTaskEntry_Workflow : PXGraphExtension<ProjectTaskEntry>
	{
		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<ProjectTaskEntry, PMTask>());
		protected static void Configure(WorkflowContext<ProjectTaskEntry, PMTask> context)
		{
			var processingCategory = context.Categories.CreateNew(ToolbarCategory.ActionCategoryNames.Processing,
				category => category.DisplayName(ToolbarCategory.ActionCategory.Processing));

			context.AddScreenConfigurationFor(screen =>
				screen
					.StateIdentifierIs<PMTask.status>()
					.AddDefaultFlow(flow => flow
						.WithFlowStates(fss =>
						{
							fss.Add<ProjectTaskStatus.planned>(flowState =>
							{
								return flowState
									.IsInitial()
									.WithActions(actions =>
									{
										actions.Add(g => g.activate, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
										actions.Add(g => g.cancelTask, c => c.IsDuplicatedInToolbar());
									});
							});
							fss.Add<ProjectTaskStatus.active>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(g => g.complete, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
										actions.Add(g => g.cancelTask, c => c.IsDuplicatedInToolbar());
										actions.Add(g => g.hold);
									});
							});
							fss.Add<ProjectTaskStatus.completed>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(g => g.activate, c => c.IsDuplicatedInToolbar());
									});
							});
							fss.Add<ProjectTaskStatus.canceled>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(g => g.activate, c => c.IsDuplicatedInToolbar());
									});
							});
						})
						.WithTransitions(transitions =>
						{
							transitions.AddGroupFrom<ProjectTaskStatus.planned>(ts =>
							{
								ts.Add(t => t
									.To<ProjectTaskStatus.active>()
									.IsTriggeredOn(g => g.activate));
								ts.Add(t => t
									.To<ProjectTaskStatus.canceled>()
									.IsTriggeredOn(g => g.cancelTask));
							});
							transitions.AddGroupFrom<ProjectTaskStatus.active>(ts =>
							{
								ts.Add(t => t
									.To<ProjectTaskStatus.completed>()
									.IsTriggeredOn(g => g.complete));
								ts.Add(t => t
									.To<ProjectTaskStatus.canceled>()
									.IsTriggeredOn(g => g.cancelTask));
								ts.Add(t => t
									.To<ProjectTaskStatus.planned>()
									.IsTriggeredOn(g => g.hold));
							});
							transitions.AddGroupFrom<ProjectTaskStatus.completed>(ts =>
							{
								ts.Add(t => t
									.To<ProjectTaskStatus.active>()
									.IsTriggeredOn(g => g.activate));
							});
							transitions.AddGroupFrom<ProjectTaskStatus.canceled>(ts =>
							{
								ts.Add(t => t
									.To<ProjectTaskStatus.active>()
									.IsTriggeredOn(g => g.activate));
							});
						}))
					.WithActions(actions =>
					{
						actions.Add(g => g.activate, c => c
							.WithCategory(processingCategory)
							.WithFieldAssignments(fa =>
							{
								fa.Add<PMTask.isActive>(e => e.SetFromValue(true));
								fa.Add<PMTask.isCompleted>(e => e.SetFromValue(false));
								fa.Add<PMTask.isCancelled>(e => e.SetFromValue(false));

							}));
						actions.Add(g => g.hold, a => a
							.WithCategory(processingCategory)
							.WithFieldAssignments(fa =>
							{
								fa.Add<PMTask.isActive>(e => e.SetFromValue(false));
								fa.Add<PMTask.isCompleted>(e => e.SetFromValue(false));
								fa.Add<PMTask.isCancelled>(e => e.SetFromValue(false));
							}));
						actions.Add(g => g.complete, a => a
							.WithCategory(processingCategory)
							.WithFieldAssignments(fa =>
							{
								fa.Add<PMTask.isActive>(e => e.SetFromValue(true));
								fa.Add<PMTask.isCompleted>(e => e.SetFromValue(true));
								fa.Add<PMTask.isCancelled>(e => e.SetFromValue(false));
							}));
						actions.Add(g => g.cancelTask, a => a
							.WithCategory(processingCategory)
							.PlaceAfter(g => g.complete)
							.WithFieldAssignments(fa =>
							{
								fa.Add<PMTask.isActive>(e => e.SetFromValue(false));
								fa.Add<PMTask.isCompleted>(e => e.SetFromValue(false));
								fa.Add<PMTask.isCancelled>(e => e.SetFromValue(true));
							}));
					})
					.WithCategories(categories =>
					{
						categories.Add(processingCategory);
					}));
		}
	}
}
