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
using PX.Objects.CN.ProjectAccounting.PM.GraphExtensions;

namespace PX.Objects.PM
{
	using static BoundedTo<ProgressWorksheetEntry, PMProgressWorksheet>;

	public partial class ProgressWorksheetEntry_ApprovalWorkflow : PXGraphExtension<ProgressWorksheetEntry_Workflow, ProgressWorksheetEntry>
	{
		public static bool IsActive() => true;

		private class ProgressWorksheetSetupApproval : IPrefetchable
		{
			public static bool IsActive => PXDatabase.GetSlot<ProgressWorksheetSetupApproval>(nameof(ProgressWorksheetSetupApproval), typeof(PMSetup)).RequestApproval;

			private bool RequestApproval;

			void IPrefetchable.Prefetch()
			{
				using (PXDataRecord setup = PXDatabase.SelectSingle<PMSetup>(new PXDataField<PMSetup.progressWorksheetApprovalMapID>()))
				{
					if (setup != null)
						RequestApproval = setup.GetInt32(0).HasValue;
				}
			}
		}

		protected static bool ApprovalIsActive() => PXAccess.FeatureInstalled<CS.FeaturesSet.approvalWorkflow>() && ProgressWorksheetSetupApproval.IsActive;

		[PXWorkflowDependsOnType(typeof(PMSetup))]
		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<ProgressWorksheetEntry, PMProgressWorksheet>());

		protected static void Configure(WorkflowContext<ProgressWorksheetEntry, PMProgressWorksheet> context)
		{
			Condition Bql<T>() where T : IBqlUnary, new() => context.Conditions.FromBql<T>();
			var conditions = new
			{
				IsRejected
					= Bql<PMProgressWorksheet.rejected.IsEqual<True>>(),
				IsApproved
					= Bql<PMProgressWorksheet.approved.IsEqual<True>>(),
				IsNotApproved
					= Bql<PMProgressWorksheet.approved.IsEqual<False>>(),
				IsApprovalDisabled
					= ApprovalIsActive()
						? Bql<True.IsEqual<False>>()
						: Bql<PMProgressWorksheet.status.IsNotIn<ProgressWorksheetStatus.pendingApproval, ProgressWorksheetStatus.rejected>>()
			}.AutoNameConditions();

			var approvalCategory = context.Categories.CreateNew(ToolbarCategory.ActionCategoryNames.Approval,
				category => category.DisplayName(ToolbarCategory.ActionCategory.Approval));

			var approve = context.ActionDefinitions
				.CreateExisting<ProgressWorksheetEntry_ApprovalWorkflow>(g => g.approve, a => a
					.InFolder(approvalCategory, g => g.removeHold)
					.PlaceAfter(g => g.removeHold)
					.IsHiddenWhen(conditions.IsApprovalDisabled)
					.WithFieldAssignments(fa => fa.Add<PMProgressWorksheet.approved>(e => e.SetFromValue(true))));

			var reject = context.ActionDefinitions
				.CreateExisting<ProgressWorksheetEntry_ApprovalWorkflow>(g => g.reject, a => a
					.InFolder(approvalCategory, approve)
					.PlaceAfter(approve)
					.IsHiddenWhen(conditions.IsApprovalDisabled)
					.WithFieldAssignments(fa => fa.Add<PMProgressWorksheet.rejected>(e => e.SetFromValue(true))));

			var reassign = context.ActionDefinitions
				.CreateExisting(nameof(ProgressWorksheetEntry.Approval.ReassignApproval), a => a
					.WithCategory(approvalCategory)
					.PlaceAfter(reject)
					.IsHiddenWhen(conditions.IsApprovalDisabled));

			context.UpdateScreenConfigurationFor(screen =>
				screen
					.UpdateDefaultFlow(flow => flow
						.WithFlowStates(fss =>
						{
							fss.Add<ProgressWorksheetStatus.pendingApproval>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(approve, c => c.IsDuplicatedInToolbar());
										actions.Add(reject, c => c.IsDuplicatedInToolbar());
										actions.Add(reassign);
										actions.Add(g => g.hold);
									});
							});
							fss.Add<ProgressWorksheetStatus.rejected>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(g => g.hold, c => c.IsDuplicatedInToolbar());
									});
							});
						})
						.WithTransitions(transitions =>
						{
							transitions.UpdateGroupFrom<ProgressWorksheetStatus.onHold>(ts =>
							{
								ts.Update(t => t
									.To<ProgressWorksheetStatus.open>()
									.IsTriggeredOn(g => g.removeHold), t => t
									.When(conditions.IsApproved));
								ts.Add(t => t
									.To<ProgressWorksheetStatus.pendingApproval>()
									.IsTriggeredOn(g => g.removeHold)
									.When(conditions.IsNotApproved));
							});
							transitions.AddGroupFrom<ProgressWorksheetStatus.pendingApproval>(ts =>
							{
								ts.Add(t => t
									.To<ProgressWorksheetStatus.open>()
									.IsTriggeredOn(approve)
									.When(conditions.IsApproved));
								ts.Add(t => t
									.To<ProgressWorksheetStatus.rejected>()
									.IsTriggeredOn(reject)
									.When(conditions.IsRejected));
								ts.Add(t => t
									.To<ProgressWorksheetStatus.onHold>()
									.IsTriggeredOn(g => g.hold));
							});
							transitions.AddGroupFrom<ProgressWorksheetStatus.rejected>(ts =>
							{
								ts.Add(t => t
									.To<ProgressWorksheetStatus.onHold>()
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
							a => a.WithFieldAssignments(fa =>
							{
								fa.Add<PMProgressWorksheet.approved>(f => f.SetFromValue(false));
								fa.Add<PMProgressWorksheet.rejected>(f => f.SetFromValue(false));
							}));
					})
					.WithCategories(categories =>
					{
						categories.Add(approvalCategory);
						categories.Update(ToolbarCategory.ActionCategoryNames.Approval, category => category.PlaceAfter(context.Categories.Get(ToolbarCategory.ActionCategoryNames.Corrections)));
					}));
		}

		public PXAction<PMProgressWorksheet> approve;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Approve")]
		protected virtual IEnumerable Approve(PXAdapter adapter) => adapter.Get();

		public PXAction<PMProgressWorksheet> reject;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Reject")]
		protected virtual IEnumerable Reject(PXAdapter adapter) => adapter.Get();
	}
}
