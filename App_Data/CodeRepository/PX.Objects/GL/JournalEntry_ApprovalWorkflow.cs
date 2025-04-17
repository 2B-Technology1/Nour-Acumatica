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
using PX.Objects.AP;
using PX.Objects.EP;

namespace PX.Objects.GL
{
	using State = BatchStatus;
	using static Batch;
	using static BoundedTo<JournalEntry, Batch>;

	public class GlApprovalSettings : EPApprovalSettings<GLSetupApproval, GL.Batch.batchType>
	{
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class JournalEntry_ApprovalWorkflow : PXGraphExtension<JournalEntry_Workflow, JournalEntry>
	{
		[PXWorkflowDependsOnType(typeof(GLSetupApproval))]
		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<JournalEntry, Batch>());

		public class Conditions : Condition.Pack
		{
			public Condition IsApproved => GetOrCreate(b => b.FromBql<
				Batch.approved.IsEqual<True>
			>());

			public Condition IsRejected => GetOrCreate(b => b.FromBql<
				Batch.rejected.IsEqual<True>
			>());

			public Condition IsNotApproved => GetOrCreate(c => c.FromBql<
				approved.IsEqual<False>.And<rejected.IsEqual<False>>
			>());

			public Condition IsNotOnHoldAndIsApproved => GetOrCreate(c => c.FromBql<
				hold.IsEqual<False>.And<approved.IsEqual<True>.And<released.IsEqual<False>>>
			>());

			public Condition IsNotOnHoldAndIsNotApproved => GetOrCreate(c => c.FromBql<
				hold.IsEqual<False>.And<approved.IsEqual<False>>
			>());

			public Condition IsApprovalDisabled => GetOrCreate(b => b.FromBqlType(
				GlApprovalSettings
					.IsApprovalDisabled<batchType, BatchTypeCode,
						Where<status.IsNotIn<BatchStatus.pendingApproval, BatchStatus.rejected>>>()));
			public Condition NonEditable => GetOrCreate(b => b.FromBqlType(
				BqlCommand.Compose(
					typeof(Where<,,>),
					typeof(status), typeof(NotEqual<BatchStatus.hold>), typeof(And<>), typeof(Not<>),
					GlApprovalSettings.IsApprovalDisabled<batchType, BatchTypeCode>()
					)));
		}

		protected static void Configure(WorkflowContext<JournalEntry, Batch> context)
		{
			var conditions = context.Conditions.GetPack<Conditions>();
			var approvalCategory = context.Categories.Get(JournalEntry_Workflow.ActionCategory.Approval);

			var approveAction = context.ActionDefinitions
				.CreateExisting<JournalEntry_ApprovalWorkflow>(g => g.approve, a => a
					.WithCategory(approvalCategory)
					.PlaceAfter(g => g.post)
					.IsHiddenWhen(conditions.IsApprovalDisabled)
					.WithFieldAssignments(fa => fa.Add<Batch.approved>(e => e.SetFromValue(true))));

			var rejectAction = context.ActionDefinitions
				.CreateExisting<JournalEntry_ApprovalWorkflow>(g => g.reject, a => a
					.WithCategory(approvalCategory, approveAction)
					.PlaceAfter(approveAction)
					.IsHiddenWhen(conditions.IsApprovalDisabled)
					.WithFieldAssignments(fa => fa.Add<Batch.rejected>(e => e.SetFromValue(true))));

			var reassignAction = context.ActionDefinitions
				.CreateExisting(nameof(JournalEntry.Approval.ReassignApproval), a => a
					.WithCategory(approvalCategory)
					.PlaceAfter(rejectAction)
					.IsHiddenWhen(conditions.IsApprovalDisabled));

			Workflow.ConfiguratorFlow InjectApprovalWorkflow(Workflow.ConfiguratorFlow flow)
			{
				const string initialState = "_";

				return flow
					.WithFlowStates(states =>
					{
						states.Add<State.pendingApproval>(flowState =>
						{
							return flowState
								.WithActions(actions =>
								{
									actions.Add(approveAction, a => a.IsDuplicatedInToolbar());
									actions.Add(rejectAction, a => a.IsDuplicatedInToolbar());
									actions.Add(reassignAction);
									actions.Add(g => g.putOnHold);
									actions.Add(g => g.createSchedule);
								})
								.WithFieldStates(fields =>
								{
									fields.AddAllFields<Batch>(table => table.IsDisabled());
									fields.AddField<batchNbr>();
									fields.AddField<module>();
									fields.AddTable<GLTran>(table => table.IsDisabled());
								})
								.WithEventHandlers(handlers =>
								 {
									 handlers.Add(g => g.OnConfirmSchedule);
								 });
						});
						states.Add<State.rejected>(flowState =>
						{
							return flowState
								.WithActions(actions =>
								{
									actions.Add(g => g.putOnHold, a => a.IsDuplicatedInToolbar());
								})
								.WithFieldStates(fields =>
								{
									fields.AddAllFields<Batch>(table => table.IsDisabled());
									fields.AddField<batchNbr>();
									fields.AddField<module>();
									fields.AddTable<GLTran>(table => table.IsDisabled());
								});
						});
						//states.Update<State.balanced>(flowState =>
						//{
						//	return flowState
						//		.WithFieldStates(fields =>
						//		{
						//			fields.AddAllFields<Batch>(table => table.IsDisabled());
						//			fields.AddField<batchNbr>();
						//			fields.AddTable<GLTran>(table => table.IsDisabled());
						//		});
						//});
					})
					.WithTransitions(transitions =>
					{
						transitions.UpdateGroupFrom(initialState, ts =>
						{
							ts.Add(t => t // New Pending Approval
								.To<State.pendingApproval>()
								.IsTriggeredOn(g => g.initializeState)
								.When(conditions.IsNotOnHoldAndIsNotApproved)
								);
							ts.Update(t => t
								.To<State.balanced>()
								.IsTriggeredOn(g => g.initializeState), t => t
								.When(conditions.IsNotOnHoldAndIsApproved)); // IsNotOnHold -> IsNotOnHoldAndIsApproved
						});

						transitions.UpdateGroupFrom<State.hold>(ts =>
						{
							ts.Update(
								t => t
								.To<State.balanced>()
								.IsTriggeredOn(g => g.releaseFromHold), t => t
								.When(conditions.IsApproved));
							ts.Update(
								t => t
								.To<State.balanced>()
								.IsTriggeredOn(g => g.OnUpdateStatus), t => t
								.When(conditions.IsApproved));
							ts.Add(t => t
								.To<State.pendingApproval>()
								.IsTriggeredOn(g => g.releaseFromHold)
								.DoesNotPersist()
								.When(conditions.IsNotApproved));
							ts.Add(t => t
								.To<State.rejected>()
								.IsTriggeredOn(g => g.releaseFromHold)
								.DoesNotPersist()
								.When(conditions.IsRejected));
							ts.Add(t => t
								.To<State.pendingApproval>()
								.IsTriggeredOn(g => g.OnUpdateStatus)
								.DoesNotPersist()
								.When(conditions.IsNotApproved));
							ts.Add(t => t
								.To<State.rejected>()
								.IsTriggeredOn(g => g.OnUpdateStatus)
								.DoesNotPersist()
								.When(conditions.IsRejected));
						});


						transitions.AddGroupFrom<State.pendingApproval>(ts =>
						{
							ts.Add(t => t
								.To<State.balanced>()
								.IsTriggeredOn(approveAction)
								.When(conditions.IsApproved));
							ts.Add(t => t
								.To<State.rejected>()
								.IsTriggeredOn(rejectAction)
								.When(conditions.IsRejected));
							ts.Add(t => t
								.To<State.hold>()
								.IsTriggeredOn(g => g.putOnHold)
								.DoesNotPersist());
							ts.Add(t => t.To<State.scheduled>()
								.IsTriggeredOn(g => g.OnConfirmSchedule)
								.WithFieldAssignments(fas =>
								{
									fas.Add<scheduled>(e => e.SetFromValue(true));
									fas.Add<scheduleID>(e => e.SetFromExpression("@ScheduleID"));
								}));
						});

						transitions.AddGroupFrom<State.rejected>(ts =>
						{
							ts.Add(t => t
								.To<State.hold>()
								.IsTriggeredOn(g => g.putOnHold)
								.DoesNotPersist());
						});
					});
			}

			context.UpdateScreenConfigurationFor(screen =>
			{
				return screen
					.UpdateDefaultFlow(InjectApprovalWorkflow)
					.WithActions(actions =>
					{
						actions.Add(approveAction);
						actions.Add(rejectAction);
						actions.Add(reassignAction);
						actions.Update(
							g => g.putOnHold,
							a => a.WithFieldAssignments(fas =>
							{
								fas.Add<Batch.approved>(f => f.SetFromValue(false));
								fas.Add<Batch.rejected>(f => f.SetFromValue(false));
							}));
					});
			});
		}

		public PXAction<Batch> approve;

		[PXButton(CommitChanges = true),
		 PXUIField(DisplayName = "Approve", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable Approve(PXAdapter adapter) => adapter.Get();

		public PXAction<Batch> reject;

		[PXButton(CommitChanges = true),
		 PXUIField(DisplayName = "Reject", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable Reject(PXAdapter adapter) => adapter.Get();
	}
}
