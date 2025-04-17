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
using System.Collections;

namespace PX.Objects.PM
{
	using static BoundedTo<ChangeOrderEntry, PMChangeOrder>;

	public partial class ChangeOrderEntry_ApprovalWorkflow : PXGraphExtension<ChangeOrderEntry_Workflow, ChangeOrderEntry>
	{
		private class PMChangeOrderSetupApproval : IPrefetchable
		{
			public static bool IsActive => PXDatabase.GetSlot<PMChangeOrderSetupApproval>(nameof(PMChangeOrderSetupApproval), typeof(PMSetup)).RequestApproval;

			private bool RequestApproval;

			void IPrefetchable.Prefetch()
			{
				using (PXDataRecord setup = PXDatabase.SelectSingle<PMSetup>(new PXDataField<PMSetup.changeOrderApprovalMapID>()))
				{
					if (setup != null)
						RequestApproval = setup.GetInt32(0).HasValue;
				}
			}
		}

		protected static bool ApprovalIsActive() => PXAccess.FeatureInstalled<CS.FeaturesSet.approvalWorkflow>() && PMChangeOrderSetupApproval.IsActive;

		[PXWorkflowDependsOnType(typeof(PMSetup))]
		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<ChangeOrderEntry, PMChangeOrder>());

		protected static void Configure(WorkflowContext<ChangeOrderEntry, PMChangeOrder> context)
		{
			Condition Bql<T>() where T : IBqlUnary, new() => context.Conditions.FromBql<T>();
			var conditions = new
			{
				IsRejected
					= Bql<PMChangeOrder.rejected.IsEqual<True>>(),
				IsNotRejected
					= Bql<PMChangeOrder.rejected.IsNotEqual<True>>(),
				IsApproved
					= Bql<PMChangeOrder.approved.IsEqual<True>>(),
				IsNotApproved
					= Bql<PMChangeOrder.approved.IsEqual<False>>(),
				IsApprovalDisabled
					= ApprovalIsActive()
						? Bql<True.IsEqual<False>>()
						: Bql<PMChangeOrder.status.IsNotIn<ChangeOrderStatus.pendingApproval, ChangeOrderStatus.rejected>>()
			}.AutoNameConditions();

			var approvalCategory = context.Categories.CreateNew(ToolbarCategory.ActionCategoryNames.Approval,
				category => category.DisplayName(ToolbarCategory.ActionCategory.Approval));

			var approve = context.ActionDefinitions
				.CreateExisting<ChangeOrderEntry_ApprovalWorkflow>(g => g.approve, a => a
					.InFolder(approvalCategory, g => g.removeHold)
					.PlaceAfter(g => g.removeHold)
					.IsHiddenWhen(conditions.IsApprovalDisabled)
					.WithFieldAssignments(fa => fa.Add<PMChangeOrder.approved>(e => e.SetFromValue(true))));

			var reassign = context.ActionDefinitions
				.CreateExisting(nameof(ChangeOrderEntry.Approval.ReassignApproval), a => a
					.WithCategory(approvalCategory)
					.PlaceAfter(g => g.reject)
					.IsHiddenWhen(conditions.IsApprovalDisabled));

			context.UpdateScreenConfigurationFor(screen =>
				screen
					.UpdateDefaultFlow(flow => flow
						.WithFlowStates(fss =>
						{
							fss.Add<ChangeOrderStatus.pendingApproval>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(approve, c => c.IsDuplicatedInToolbar());
										actions.Add(g => g.reject, c => c.IsDuplicatedInToolbar());
										actions.Add(reassign);
										actions.Add(g => g.hold);
										actions.Add(g => g.send);
										actions.Add(g => g.coCancel);
									});
							});
							fss.Add<ChangeOrderStatus.rejected>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(g => g.hold, c => c.IsDuplicatedInToolbar());
										actions.Add(g => g.send);
										actions.Add(g => g.coCancel);
									});
							});
						})
						.WithTransitions(transitions =>
						{
							transitions.UpdateGroupFrom<ChangeOrderStatus.onHold>(ts =>
							{
								ts.Update(t => t
									.To<ChangeOrderStatus.open>()
									.IsTriggeredOn(g => g.removeHold), t => t
									.When(conditions.IsApproved));
								ts.Add(t => t
									.To<ChangeOrderStatus.rejected>()
									.IsTriggeredOn(g => g.removeHold)
									.When(conditions.IsRejected));
								ts.Add(t => t
									.To<ChangeOrderStatus.pendingApproval>()
									.IsTriggeredOn(g => g.removeHold)
									.When(conditions.IsNotRejected)
									.When(conditions.IsNotApproved));
							});
							transitions.AddGroupFrom<ChangeOrderStatus.pendingApproval>(ts =>
							{
								ts.Add(t => t
									.To<ChangeOrderStatus.open>()
									.IsTriggeredOn(approve)
									.When(conditions.IsApproved));
								ts.Add(t => t
									.To<ChangeOrderStatus.rejected>()
									.IsTriggeredOn(g => g.reject)
									.When(conditions.IsRejected));
								ts.Add(t => t
									.To<ChangeOrderStatus.onHold>()
									.IsTriggeredOn(g => g.hold));
								ts.Add(t => t
									.To<ChangeOrderStatus.canceled>()
									.IsTriggeredOn(g => g.coCancel));
							});
							transitions.AddGroupFrom<ChangeOrderStatus.rejected>(ts =>
							{
								ts.Add(t => t
									.To<ChangeOrderStatus.onHold>()
									.IsTriggeredOn(g => g.hold));
								ts.Add(t => t
									.To<ChangeOrderStatus.canceled>()
									.IsTriggeredOn(g => g.coCancel));
							});
						}))
					.WithActions(actions =>
					{
						actions.Add(approve);
						actions.Add(
							g => g.reject, c => c.InFolder(approvalCategory, approve)
							.PlaceAfter(approve)
							.IsHiddenWhen(conditions.IsApprovalDisabled)
							.WithFieldAssignments(fa => fa.Add<PMChangeOrder.rejected>(e => e.SetFromValue(true)))
							);
						actions.Add(reassign);
						actions.Update(
							g => g.hold,
							a => a.WithFieldAssignments(fa =>
							{
								fa.Add<PMChangeOrder.approved>(f => f.SetFromValue(false));
								fa.Add<PMChangeOrder.rejected>(f => f.SetFromValue(false));
							}));
					})
					.WithCategories(categories =>
					{
						categories.Add(approvalCategory);
						categories.Update(ToolbarCategory.ActionCategoryNames.Approval, category => category.PlaceAfter(context.Categories.Get(ToolbarCategory.ActionCategoryNames.Corrections)));
					}));
		}

		public PXAction<PMChangeOrder> approve;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Approve", MapEnableRights = PXCacheRights.Select)]
		protected virtual IEnumerable Approve(PXAdapter adapter) => adapter.Get();
	}
}
