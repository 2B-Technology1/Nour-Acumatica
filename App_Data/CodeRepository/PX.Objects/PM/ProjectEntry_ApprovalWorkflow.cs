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

namespace PX.Objects.PM
{
	using static BoundedTo<ProjectEntry, PMProject>;

	public partial class ProjectEntry_ApprovalWorkflow : PXGraphExtension<ProjectEntry_Workflow, ProjectEntry>
	{
		private class ProjectSetupApproval : IPrefetchable
		{
			public static bool IsActive => PXDatabase.GetSlot<ProjectSetupApproval>(nameof(ProjectSetupApproval), typeof(PMSetup)).RequestApproval;

			private bool RequestApproval;

			void IPrefetchable.Prefetch()
			{
				using (PXDataRecord setup = PXDatabase.SelectSingle<PMSetup>(new PXDataField<PMSetup.assignmentMapID>()))
				{
					if (setup != null)
						RequestApproval = setup.GetInt32(0).HasValue;
				}
			}
		}

		protected static bool ApprovalIsActive() => PXAccess.FeatureInstalled<CS.FeaturesSet.approvalWorkflow>() && ProjectSetupApproval.IsActive;

		[PXWorkflowDependsOnType(typeof(PMSetup))]
		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<ProjectEntry, PMProject>());

		protected static void Configure(WorkflowContext<ProjectEntry, PMProject> context)
		{
			Condition Bql<T>() where T : IBqlUnary, new() => context.Conditions.FromBql<T>();
			var conditions = new
			{
				IsApproved
					= Bql<PMProject.approved.IsEqual<True>>(),
				IsNotApproved
					= Bql<PMProject.approved.IsEqual<False>>(),
				IsNotRejected
					= Bql<PMProject.rejected.IsNotEqual<True>>(),
				IsApprovalDisabled
					= ApprovalIsActive()
						? Bql<True.IsEqual<False>>()
						: Bql<PMProject.status.IsNotEqual<ProjectStatus.pendingApproval>>()
			}.AutoNameConditions();

			var approvalCategory = context.Categories.CreateNew(ToolbarCategory.ActionCategoryNames.Approval,
				category => category.DisplayName(ToolbarCategory.ActionCategory.Approval));

			var approve = context.ActionDefinitions
				.CreateExisting<ProjectEntry_ApprovalWorkflow>(g => g.approve, a => a
					.InFolder(approvalCategory, g => g.activate)
					.PlaceAfter(g => g.activate)
					.IsHiddenWhen(conditions.IsApprovalDisabled)
					.WithFieldAssignments(fa =>
					{
						fa.Add<PMProject.approved>(e => e.SetFromValue(true));
						fa.Add<PMProject.isActive>(e => e.SetFromField<PMProject.approved>());
					})
);

			var reject = context.ActionDefinitions
				.CreateExisting<ProjectEntry_ApprovalWorkflow>(g => g.reject, a => a
					.InFolder(approvalCategory, approve)
					.PlaceAfter(approve)
					.IsHiddenWhen(conditions.IsApprovalDisabled)
					.WithFieldAssignments(fa =>
					{
						fa.Add<PMProject.hold>(e => e.SetFromValue(true));
						fa.Add<PMProject.isActive>(e => e.SetFromValue(false));
					}));

			var reassign = context.ActionDefinitions
				.CreateExisting(nameof(ProjectEntry.Approval.ReassignApproval), a => a
					.WithCategory(approvalCategory)
					.PlaceAfter(reject)
					.IsHiddenWhen(conditions.IsApprovalDisabled));

			context.UpdateScreenConfigurationFor(screen =>
				screen
					.UpdateDefaultFlow(flow => flow
						.WithFlowStates(fss =>
						{
							fss.Add<ProjectStatus.pendingApproval>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(approve, c => c.IsDuplicatedInToolbar());
										actions.Add(reject, c => c.IsDuplicatedInToolbar());
										actions.Add(reassign);
										actions.Add(g => g.hold);
										actions.Add(g => g.createChangeOrder);
										actions.Add(g => g.lockBudget);
										actions.Add(g => g.unlockBudget);
										actions.Add(g => g.lockCommitments);
										actions.Add(g => g.unlockCommitments);
										actions.Add(g => g.validateAddresses);
										actions.Add(g => g.validateBalance);
										actions.Add(g => g.createTemplate);
										actions.Add(g => g.runAllocation);
										actions.Add(g => g.autoBudget);
									});
							});
						})
						.WithTransitions(transitions =>
						{
							transitions.UpdateGroupFrom<ProjectStatus.planned>(ts =>
							{
								ts.Update(t => t
									.To<ProjectStatus.active>()
									.IsTriggeredOn(g => g.activate), t => t
									.When(conditions.IsApproved));

								ts.Add(t => t
									.To<ProjectStatus.pendingApproval>()
									.IsTriggeredOn(g => g.activate)
									.When(conditions.IsNotApproved)
									.When(conditions.IsNotRejected)
									.WithFieldAssignments(fa =>
									{
										fa.Add<PMProject.isActive>(e => e.SetFromValue(false));
									}));
							});
							transitions.AddGroupFrom<ProjectStatus.pendingApproval>(ts =>
							{
								ts.Add(t => t
									.To<ProjectStatus.active>()
									.IsTriggeredOn(approve)
									.When(conditions.IsApproved));

								ts.Add(t => t
									.To<ProjectStatus.planned>()
									.IsTriggeredOn(reject));

								ts.Add(t => t
									.To<ProjectStatus.planned>()
									.IsTriggeredOn(g => g.hold));
							});
						}))
					.WithActions(actions =>
					{
						actions.Add(approve);
						actions.Add(reject);
						actions.Add(reassign);
						actions.Update(
							g => g.hold,
							a => a.WithFieldAssignments(fa => fa.Add<PMProject.approved>(f => f.SetFromValue(false))));
					})
					.WithCategories(categories =>
					{
						categories.Add(approvalCategory);
						categories.Update(ToolbarCategory.ActionCategoryNames.Approval, category => category.PlaceAfter(context.Categories.Get(ToolbarCategory.ActionCategoryNames.Processing)));
					}));
		}

		public PXAction<PMProject> approve;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Approve")]
		protected virtual IEnumerable Approve(PXAdapter adapter) => adapter.Get();

		public PXAction<PMProject> reject;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Reject")]
		protected virtual IEnumerable Reject(PXAdapter adapter) => adapter.Get();
	}
}
