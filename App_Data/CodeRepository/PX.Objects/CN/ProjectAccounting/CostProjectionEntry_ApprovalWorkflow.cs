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
using PX.Objects.PM;

namespace PX.Objects.CN.ProjectAccounting
{
	using static BoundedTo<CostProjectionEntry, PMCostProjection>;

	public partial class CostProjectionEntry_ApprovalWorkflow : PXGraphExtension<CostProjectionEntry_Workflow, CostProjectionEntry>
	{
		private class CostProjectionSetupApproval : IPrefetchable
		{
			public static bool IsActive => PXDatabase.GetSlot<CostProjectionSetupApproval>(nameof(CostProjectionSetupApproval), typeof(PMSetup)).RequestApproval;

			private bool RequestApproval;

			void IPrefetchable.Prefetch()
			{
				using (PXDataRecord setup = PXDatabase.SelectSingle<PMSetup>(new PXDataField<PMSetup.costProjectionApprovalMapID>()))
				{
					if (setup != null)
						RequestApproval = setup.GetInt32(0).HasValue;
				}
			}
		}

		protected static bool ApprovalIsActive() => PXAccess.FeatureInstalled<CS.FeaturesSet.approvalWorkflow>() && CostProjectionSetupApproval.IsActive;

		[PXWorkflowDependsOnType(typeof(PMSetup))]
		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<CostProjectionEntry, PMCostProjection>());

		public class Conditions : Condition.Pack
		{
			public Condition IsApproved => GetOrCreate(b => b.FromBql<
				PMCostProjection.approved.IsEqual<True>
			>());

			public Condition IsRejected => GetOrCreate(b => b.FromBql<
				PMCostProjection.rejected.IsEqual<True>
			>());
			
			public Condition IsNotApproved => GetOrCreate(b => b.FromBql<
				PMCostProjection.approved.IsEqual<False>
			>());

			public Condition IsNotRejected => GetOrCreate(b => b.FromBql<
				PMCostProjection.rejected.IsNotEqual<True>
			>());

			public Condition IsApprovalDisabled => GetOrCreate(b =>
				ApprovalIsActive()
					? b.FromBql<True.IsEqual<False>>()
					: b.FromBql<PMCostProjection.status.IsNotIn<CostProjectionStatus.pendingApproval,
						CostProjectionStatus.rejected>>()
			);
		}
		protected static void Configure(WorkflowContext<CostProjectionEntry, PMCostProjection> context)
		{
			var conditions = context.Conditions.GetPack<Conditions>();

			var approvalCategory = context.Categories.CreateNew(ToolbarCategory.ActionCategoryNames.Approval,
				category => category.DisplayName(ToolbarCategory.ActionCategory.Approval));

			var approve = context.ActionDefinitions
				.CreateExisting<CostProjectionEntry_ApprovalWorkflow>(g => g.approve, a => a
					.InFolder(approvalCategory, g => g.removeHold)
					.PlaceAfter(g => g.removeHold)
					.IsHiddenWhen(conditions.IsApprovalDisabled)
					.WithFieldAssignments(fa => fa.Add<PMCostProjection.approved>(e => e.SetFromValue(true))));

			var reject = context.ActionDefinitions
				.CreateExisting<CostProjectionEntry_ApprovalWorkflow>(g => g.reject, a => a
					.InFolder(approvalCategory, approve)
					.PlaceAfter(approve)
					.IsHiddenWhen(conditions.IsApprovalDisabled)
					.WithFieldAssignments(fa => fa.Add<PMCostProjection.rejected>(e => e.SetFromValue(true))));

			var reassign = context.ActionDefinitions
				.CreateExisting(nameof(CostProjectionEntry.Approval.ReassignApproval), a => a
					.WithCategory(approvalCategory)
					.PlaceAfter(reject)
					.IsHiddenWhen(conditions.IsApprovalDisabled));

			context.UpdateScreenConfigurationFor(screen =>
				screen
					.UpdateDefaultFlow(flow => flow
						.WithFlowStates(fss =>
						{
							fss.Add<CostProjectionStatus.pendingApproval>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(approve, c => c.IsDuplicatedInToolbar());
										actions.Add(reject, c => c.IsDuplicatedInToolbar());
										actions.Add(reassign);
										actions.Add(g => g.hold);
										actions.Add(g => g.createRevision, c => c.IsDuplicatedInToolbar());
									});
							});
							fss.Add<CostProjectionStatus.rejected>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(g => g.hold, c => c.IsDuplicatedInToolbar());
										actions.Add(g => g.createRevision, c => c.IsDuplicatedInToolbar());
									});
							});
						})
						.WithTransitions(transitions =>
						{
							transitions.UpdateGroupFrom<CostProjectionStatus.onHold>(ts =>
							{
								ts.Update(t => t
									.To<CostProjectionStatus.open>()
									.IsTriggeredOn(g => g.removeHold), t => t
									.When(conditions.IsApproved));
								ts.Add(t => t
									.To<CostProjectionStatus.rejected>()
									.IsTriggeredOn(g => g.removeHold)
									.When(conditions.IsRejected));
								ts.Add(t => t
									.To<CostProjectionStatus.pendingApproval>()
									.IsTriggeredOn(g => g.removeHold)
									.When(conditions.IsNotRejected)
									.When(conditions.IsNotApproved));
							});
							transitions.AddGroupFrom<CostProjectionStatus.pendingApproval>(ts =>
							{
								ts.Add(t => t
									.To<CostProjectionStatus.open>()
									.IsTriggeredOn(approve)
									.When(conditions.IsApproved));
								ts.Add(t => t
									.To<CostProjectionStatus.rejected>()
									.IsTriggeredOn(reject)
									.When(conditions.IsRejected));
								ts.Add(t => t
									.To<CostProjectionStatus.onHold>()
									.IsTriggeredOn(g => g.hold));
							});
							transitions.AddGroupFrom<CostProjectionStatus.rejected>(ts =>
							{
								ts.Add(t => t
									.To<CostProjectionStatus.onHold>()
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
								fa.Add<PMCostProjection.approved>(f => f.SetFromValue(false));
								fa.Add<PMCostProjection.rejected>(f => f.SetFromValue(false));
							}));
					})
					.WithCategories(categories =>
					{
						categories.Add(approvalCategory);
						categories.Update(ToolbarCategory.ActionCategoryNames.Approval, category => category.PlaceAfter(context.Categories.Get(ToolbarCategory.ActionCategoryNames.Processing)));
					}));
		}

		public PXAction<PMCostProjection> approve;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Approve")]
		protected virtual IEnumerable Approve(PXAdapter adapter) => adapter.Get();

		public PXAction<PMCostProjection> reject;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Reject")]
		protected virtual IEnumerable Reject(PXAdapter adapter) => adapter.Get();
	}
}
