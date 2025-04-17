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

using PX.Common;
using PX.Data;
using PX.Data.WorkflowAPI;
using PX.Objects.Common;

namespace PX.Objects.RQ
{
	using State = RQRequisitionStatus;
	using Self = RQRequisitionEntry_ApprovalWorkflow;
	using Context = WorkflowContext<RQRequisitionEntry, RQRequisition>;
	using static RQRequisition;
	using static BoundedTo<RQRequisitionEntry, RQRequisition>;

	public class RQRequisitionEntry_ApprovalWorkflow : PXGraphExtension<RQRequisitionEntry_Workflow, RQRequisitionEntry>
	{
		private class RQRequisitionApproval : IPrefetchable
		{
			public static bool IsActive => PXDatabase.GetSlot<RQRequisitionApproval>(nameof(RQRequisitionApproval), typeof(RQSetup)).RequireApproval;

			private bool RequireApproval;

			void IPrefetchable.Prefetch()
			{
				using (PXDataRecord rqSetup = PXDatabase.SelectSingle<RQSetup>(new PXDataField<RQSetup.requisitionApproval>()))
				{
					if (rqSetup != null)
						RequireApproval = rqSetup.GetBoolean(0) ?? false;
				}
			}
		}

		public class Conditions : Condition.Pack
		{
			public Condition IsApproved => GetOrCreate(b => b.FromBql<
				approved.IsEqual<True>
			>());

			public Condition IsRejected => GetOrCreate(b => b.FromBql<
				rejected.IsEqual<True>
			>());
		}

		[PXWorkflowDependsOnType(typeof(RQSetup))]
		public sealed override void Configure(PXScreenConfiguration config)
		{
			if (RQRequisitionApproval.IsActive)
				Configure(config.GetScreenConfigurationContext<RQRequisitionEntry, RQRequisition>());
			else
				HideApprovalActions(config.GetScreenConfigurationContext<RQRequisitionEntry, RQRequisition>());
		}

		protected static void Configure(Context context)
		{
			var conditions = context.Conditions.GetPack<Conditions>();
			var baseConditions = context.Conditions.GetPack<RQRequisitionEntry_Workflow.Conditions>();

			(var approve, var reject, var reassign, var approvalCategory) = GetApprovalActions(context, hidden: false);

			const string initialState = "_";

			context.UpdateScreenConfigurationFor(screen =>
			{
				return screen
					.UpdateDefaultFlow(flow =>
						flow
						.WithFlowStates(states =>
						{
							states.Add<State.pendingApproval>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(g => g.putOnHold);
										actions.Add(approve, a => a.IsDuplicatedInToolbar());
										actions.Add(reject, a => a.IsDuplicatedInToolbar());
										actions.Add(reassign);
									})
									.WithFieldStates(RQRequisitionEntry_Workflow.DisableWholeScreen);
							});
							states.Add<State.rejected>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(g => g.putOnHold, a => a.IsDuplicatedInToolbar());
									})
									.WithFieldStates(RQRequisitionEntry_Workflow.DisableWholeScreen);
							});
						})
						.WithTransitions(transitions =>
						{
							transitions.UpdateGroupFrom(initialState, ts =>
							{
								ts.Add(t => t
									.To<State.pendingApproval>()
									.IsTriggeredOn(g => g.initializeState)
									.When(!conditions.IsApproved)
									.PlaceAfter(rt => rt.To<State.hold>().IsTriggeredOn(g => g.initializeState)));
							});
							transitions.UpdateGroupFrom<State.hold>(ts =>
							{
								ts.Add(t => t
									.To<State.pendingApproval>()
									.IsTriggeredOn(g => g.releaseFromHold)
									.When(!conditions.IsApproved)
									.PlaceBefore(rt => rt.To<State.open>().IsTriggeredOn(g => g.releaseFromHold)));
							});
							transitions.AddGroupFrom<State.pendingApproval>(ts =>
							{
								ts.Add(t => t
									.To<State.open>()
									.IsTriggeredOn(approve)
									.When(conditions.IsApproved && baseConditions.IsQuoted));
								ts.Add(t => t
									.To<State.pendingQuotation>()
									.IsTriggeredOn(approve)
									.When(conditions.IsApproved && baseConditions.IsBiddingCompleted));
								ts.Add(t => t
									.To<State.bidding>()
									.IsTriggeredOn(approve)
									.When(conditions.IsApproved));
								ts.Add(t => t
									.To<State.rejected>()
									.IsTriggeredOn(reject)
									.When(conditions.IsRejected));
								ts.Add(t => t
									.To<State.hold>()
									.IsTriggeredOn(g => g.putOnHold));
							});
							transitions.AddGroupFrom<State.rejected>(ts =>
							{
								ts.Add(t => t
									.To<State.hold>()
									.IsTriggeredOn(g => g.putOnHold));
							});
						}))
					.WithActions(actions =>
					{
						actions.Add(approve);
						actions.Add(reject);
						actions.Add(reassign);
						actions.Update(
							g => g.putOnHold,
							a => a.WithFieldAssignments(fas =>
							{
								fas.Add<approved>(false);
								fas.Add<rejected>(false);
							}));
					})
					.WithCategories(categories =>
					{
						categories.Add(approvalCategory);
					});
			});
		}

		protected static void HideApprovalActions(Context context)
		{
			(var approve, var reject, var reassign, _) = GetApprovalActions(context, hidden: true);

			context.UpdateScreenConfigurationFor(screen =>
			{
				return screen
					.WithActions(actions =>
					{
						actions.Add(approve);
						actions.Add(reject);
						actions.Add(reassign);
					});
			});
		}

		protected static (ActionDefinition.IConfigured approve, ActionDefinition.IConfigured reject, ActionDefinition.IConfigured reassign, ActionCategory.IConfigured approvalCategory) GetApprovalActions(Context context, bool hidden)
		{
			#region Categories
			ActionCategory.IConfigured approvalCategory = context.Categories.CreateNew(CommonActionCategories.ApprovalCategoryID,
					category => category.DisplayName(CommonActionCategories.DisplayNames.Approval)
					.PlaceAfter(CommonActionCategories.ProcessingCategoryID));
			#endregion

			var approve = context.ActionDefinitions
				.CreateExisting<Self>(g => g.approve, a => a
				.WithCategory(approvalCategory)
				.PlaceAfter(g => g.putOnHold)
				.With(it => hidden ? it.IsHiddenAlways() : it)
				.WithFieldAssignments(fa => fa.Add<approved>(true)));
			var reject = context.ActionDefinitions
				.CreateExisting<Self>(g => g.reject, a => a
				.WithCategory(approvalCategory)
				.PlaceAfter(approve)
				.With(it => hidden ? it.IsHiddenAlways() : it)
				.WithFieldAssignments(fa => fa.Add<rejected>(true)));
			var reassign = context.ActionDefinitions
				.CreateExisting(nameof(RQRequisitionEntry.Approval.ReassignApproval), a => a
				.WithCategory(approvalCategory)
				.PlaceAfter(reject)
				.With(it => hidden ? it.IsHiddenAlways() : it));

			return (approve, reject, reassign, approvalCategory);
		}

		public PXAction<RQRequisition> approve;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Approve", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable Approve(PXAdapter adapter) => adapter.Get();

		public PXAction<RQRequisition> reject;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Reject", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable Reject(PXAdapter adapter) => adapter.Get();
	}
}
