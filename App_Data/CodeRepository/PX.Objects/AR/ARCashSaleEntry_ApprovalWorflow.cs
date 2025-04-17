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
using PX.Objects.AR.Standalone;

namespace PX.Objects.AR
{
	using State = ARDocStatus;
	using static ARCashSale;
	using static BoundedTo<ARCashSaleEntry, ARCashSale>;

	public class ARCashSaleEntry_ApprovalWorkflow : PXGraphExtension<ARCashSaleEntry_Workflow, ARCashSaleEntry>
	{
		[PXWorkflowDependsOnType(typeof(ARSetupApproval))]
		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<ARCashSaleEntry, ARCashSale>());

		public class Conditions : Condition.Pack
		{
			public Condition IsApproved => GetOrCreate(b => b.FromBql<
				ARRegister.approved.IsEqual<True>
			>());

			public Condition IsRejected => GetOrCreate(b => b.FromBql<
				ARRegister.rejected.IsEqual<True>
			>());

			public Condition IsApprovalDisabled => GetOrCreate(b => b.FromBqlType(
				ARApprovalSettings
					.IsApprovalDisabled<docType, ARDocType,
						Where<status.IsNotIn<ARDocStatus.pendingApproval, ARDocStatus.rejected>>>()));
		}
		protected static void Configure(WorkflowContext<ARCashSaleEntry, ARCashSale> context)
		{
			var approvalCategory = context.Categories.Get(ARCashSaleEntry_Workflow.CategoryID.Approval);
			var conditions = context.Conditions.GetPack<Conditions>();

			var aproveAction = context.ActionDefinitions
				.CreateExisting<ARCashSaleEntry_ApprovalWorkflow>(g => g.approve, a => a
					.WithCategory(approvalCategory, g => g.releaseFromHold)
					.PlaceAfter(g => g.releaseFromHold)
					.IsHiddenWhen(conditions.IsApprovalDisabled)
					.WithFieldAssignments(fa => fa.Add<ARRegister.approved>(e => e.SetFromValue(true))));

			var rejectAction = context.ActionDefinitions
				.CreateExisting<ARCashSaleEntry_ApprovalWorkflow>(g => g.reject, a => a
					.WithCategory(approvalCategory, aproveAction)
					.PlaceAfter(aproveAction)
					.IsHiddenWhen(conditions.IsApprovalDisabled)
					.WithFieldAssignments(fa => fa.Add<ARRegister.rejected>(e => e.SetFromValue(true))));

			var reassignAction = context.ActionDefinitions
				.CreateExisting(nameof(ARCashSaleEntry.Approval.ReassignApproval), a => a
					.WithCategory(approvalCategory)
					.PlaceAfter(rejectAction)
					.IsHiddenWhen(conditions.IsApprovalDisabled));

			Workflow.ConfiguratorFlow InjectApprovalWorkflow(Workflow.ConfiguratorFlow flow)
			{
				const string initialState = "_";

				return flow
					.WithFlowStates(states =>
					{
						states.UpdateSequence<State.HoldToBalance>(seq =>
							seq.WithStates(sss =>
							{
								sss.Add<State.pendingApproval>(flowState =>
								{
									return flowState
										.IsSkippedWhen(conditions.IsApproved || conditions.IsRejected)
										.WithActions(actions =>
										{
											actions.Add(aproveAction, a => a.IsDuplicatedInToolbar());
											actions.Add(rejectAction, a => a.IsDuplicatedInToolbar());
											actions.Add(reassignAction);
											actions.Add(g => g.putOnHold);
											actions.Add(g => g.printAREdit);
											actions.Add(g => g.customerDocuments);
										})
										.PlaceAfter<State.hold>();
								});

								sss.Add<State.rejected>(flowState =>
								{
									return flowState
										.IsSkippedWhen(!conditions.IsRejected)
										.WithActions(actions =>
										{
											actions.Add(g => g.putOnHold, a => a.IsDuplicatedInToolbar());
											actions.Add(g => g.printAREdit);
											actions.Add(g => g.customerDocuments);
										})
										.PlaceAfter<State.pendingApproval>();
								});
							}));
					})
					.WithTransitions(transitions =>
					{
						transitions.AddGroupFrom<State.pendingApproval>(ts =>
						{
							ts.Add(t => t
								.To<State.HoldToBalance>()
								.IsTriggeredOn(g => g.OnUpdateStatus));
							ts.Add(t => t
								.ToNext()
								.IsTriggeredOn(aproveAction)
								.When(conditions.IsApproved));
							ts.Add(t => t
								.To<State.rejected>()
								.IsTriggeredOn(rejectAction)
								.When(conditions.IsRejected));
						});
						transitions.AddGroupFrom<State.rejected>(ts =>
						{
							ts.Add(t => t
								.To<State.hold>()
								.IsTriggeredOn(g => g.putOnHold)
								.DoesNotPersist()
							);
						});
					});
			}

			context.UpdateScreenConfigurationFor(screen =>
			{
				return screen
					.UpdateDefaultFlow(InjectApprovalWorkflow)
					.WithActions(actions =>
					{
						actions.Add(aproveAction);
						actions.Add(rejectAction);
						actions.Add(reassignAction);
						actions.Update(
							g => g.putOnHold,
							a => a.WithFieldAssignments(fas =>
							{
								fas.Add<ARRegister.approved>(f => f.SetFromValue(false));
								fas.Add<ARRegister.rejected>(f => f.SetFromValue(false));
							}));
					});
			});
		}

		public PXAction<ARCashSale> approve;

		[PXButton(CommitChanges = true),
		 PXUIField(DisplayName = "Approve", MapEnableRights = PXCacheRights.Select,
			 MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable Approve(PXAdapter adapter) => adapter.Get();

		public PXAction<ARCashSale> reject;

		[PXButton(CommitChanges = true),
		 PXUIField(DisplayName = "Reject", MapEnableRights = PXCacheRights.Select,
			 MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable Reject(PXAdapter adapter) => adapter.Get();
	}
}
