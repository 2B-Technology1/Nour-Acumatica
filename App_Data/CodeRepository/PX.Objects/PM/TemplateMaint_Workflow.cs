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
	public partial class TemplateMaint_Workflow : PXGraphExtension<TemplateMaint>
	{
		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<TemplateMaint, PMProject>());
		protected static void Configure(WorkflowContext<TemplateMaint, PMProject> context)
		{
			var processingCategory = context.Categories.CreateNew(ToolbarCategory.ActionCategoryNames.Processing,
				category => category.DisplayName(ToolbarCategory.ActionCategory.Processing));

			var activate = context.ActionDefinitions
				.CreateExisting<TemplateMaint_Workflow>(g => g.activate, a => a
					.InFolder(processingCategory)
					.WithFieldAssignments(fa =>
					{ 
						fa.Add<PMProject.isActive>(e => e.SetFromValue(true));
						fa.Add<PMProject.hold>(e => e.SetFromValue(false));
					}));

			var hold = context.ActionDefinitions
				.CreateExisting<TemplateMaint_Workflow>(g => g.hold, a => a
					.InFolder(processingCategory)
					.WithFieldAssignments(fa =>
					{
						fa.Add<PMProject.isActive>(e => e.SetFromValue(false));
						fa.Add<PMProject.hold>(e => e.SetFromValue(true));
					}));

			context.AddScreenConfigurationFor(screen =>
				screen
					.StateIdentifierIs<PMProject.status>()
					.AddDefaultFlow(flow => flow
						.WithFlowStates(fss =>
						{
							fss.Add<ProjectStatus.onHold>(flowState =>
							{
								return flowState
									.IsInitial()
									.WithActions(actions =>
									{
										actions.Add(activate, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
										actions.Add(g => g.copyTemplate, c => c.IsDuplicatedInToolbar());
									});
							});
							fss.Add<ProjectStatus.active>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(hold, c => c.IsDuplicatedInToolbar());
										actions.Add(g => g.copyTemplate, c => c.IsDuplicatedInToolbar());
									});
							});
						})
						.WithTransitions(transitions =>
						{
							transitions.AddGroupFrom<ProjectStatus.onHold>(ts =>
							{
								ts.Add(t => t
									.To<ProjectStatus.active>()
									.IsTriggeredOn(activate));
							});
							transitions.AddGroupFrom<ProjectStatus.active>(ts =>
							{
								ts.Add(t => t
									.To<ProjectStatus.onHold>()
									.IsTriggeredOn(hold));
							});
						}))
					.WithActions(actions =>
					{
						actions.Add(g => g.copyTemplate);
						actions.Add(activate);
						actions.Add(hold);
					}));
		}

		public PXAction<PMProject> activate;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Activate")]
		protected virtual IEnumerable Activate(PXAdapter adapter) => adapter.Get();

		public PXAction<PMProject> hold;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Hold")]
		protected virtual IEnumerable Hold(PXAdapter adapter) => adapter.Get();
	}
}
