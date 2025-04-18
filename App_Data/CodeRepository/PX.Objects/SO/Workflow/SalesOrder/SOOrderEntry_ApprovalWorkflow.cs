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

using PX.Common;
using PX.Data;
using PX.Data.WorkflowAPI;
using PX.Objects.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.SO.Workflow.SalesOrder
{
	using State = SOOrderStatus;
	using static SOOrder;
	using static BoundedTo<SOOrderEntry, SOOrder>;
	using Blanket = GraphExtensions.SOOrderEntryExt.Blanket;

	public class SOOrderEntry_ApprovalWorkflow : PXGraphExtension<SOOrderEntry_Workflow, SOOrderEntry>
	{
		private class SOSetupApproval : IPrefetchable
		{
			public static bool IsActive => PXDatabase.GetSlot<SOSetupApproval>(nameof(SOSetupApproval), typeof(SOSetup)).OrderRequestApproval;

			private bool OrderRequestApproval;

			void IPrefetchable.Prefetch()
			{
				using (PXDataRecord soSetup = PXDatabase.SelectSingle<SOSetup>(new PXDataField<SOSetup.orderRequestApproval>()))
				{
					if (soSetup != null)
						OrderRequestApproval = (bool)soSetup.GetBoolean(0);
				}
			}
		}

		public static bool ApprovalIsActive() => PXAccess.FeatureInstalled<CS.FeaturesSet.approvalWorkflow>() && SOSetupApproval.IsActive;

		[PXWorkflowDependsOnType(typeof(SOSetup))]
		public sealed override void Configure(PXScreenConfiguration config)
		{
			if (ApprovalIsActive())
				Configure(config.GetScreenConfigurationContext<SOOrderEntry, SOOrder>());
			else
				HideApproveAndRejectActions(config.GetScreenConfigurationContext<SOOrderEntry, SOOrder>());
		}

		public class Conditions : Condition.Pack
		{
			public Condition IsApproved => GetOrCreate(b => b.FromBql<
				approved.IsEqual<True>
			>());

			public Condition IsRejected => GetOrCreate(b => b.FromBql<
				rejected.IsEqual<True>
			>());

			public Condition IsNotHoldEntryAndNotLSEntryEnabled => GetOrCreate(b => b.FromBql <
				Where<SOOrderType.holdEntry.FromCurrent, Equal<False>,
				  And<SOOrderType.requireLocation.FromCurrent, Equal<False>,
				  And<SOOrderType.requireLotSerial.FromCurrent, Equal<False>>>>
			>());
		}

		protected static void Configure(WorkflowContext<SOOrderEntry, SOOrder> context)
		{
			var conditions = context.Conditions.GetPack<Conditions>();
			var soConditions = context.Conditions.GetPack<WorkflowSO.Conditions>();
			var approvalCategory = CommonActionCategories.Get(context).Approval;

			var approve = context.ActionDefinitions
				.CreateExisting<SOOrderEntry_ApprovalWorkflow>(g => g.approve, a => a
				.WithCategory(approvalCategory)
				.PlaceInCategory(Placement.First)
				.WithFieldAssignments(fa => fa.Add<approved>(true)));
			var reject = context.ActionDefinitions
				.CreateExisting<SOOrderEntry_ApprovalWorkflow>(g => g.reject, a => a
				.WithCategory(approvalCategory)
				.PlaceInCategory(Placement.First)
				.PlaceAfter(approve)
				.WithFieldAssignments(fa => fa.Add<rejected>(true)));
			var reassign = context.ActionDefinitions
				.CreateExisting(nameof(SOOrderEntry.Approval.ReassignApproval), a => a
				.WithCategory(approvalCategory)
				.PlaceInCategory(Placement.First)
				.PlaceAfter(reject));

			Workflow.ConfiguratorFlow InjectApprovalWorkflow(Workflow.ConfiguratorFlow flow, string behavior)
			{
				bool includeCreditHold = behavior.IsIn(SOBehavior.SO, SOBehavior.IN, SOBehavior.RM, SOBehavior.MO);
				bool inclCustOpenOrders = behavior.IsIn(SOBehavior.SO, SOBehavior.IN, SOBehavior.RM, SOBehavior.MO, SOBehavior.CM);

				const string initialState = "_";

				return flow
					.WithFlowStates(states =>
					{
						states.Add<State.pendingApproval>(flowState =>
						{
							return flowState
								.WithActions(actions =>
								{
									actions.Add(g => g.putOnHold, a => a.IsDuplicatedInToolbar());
									actions.Add(approve, a => a.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
									actions.Add(reject, a => a.IsDuplicatedInToolbar());
									actions.Add(reassign);
								});
						});
						states.Add<State.voided>(flowState =>
						{
							return flowState
								.WithActions(actions =>
								{
									actions.Add(g => g.putOnHold, a => a.IsDuplicatedInToolbar());

									if (behavior == SOBehavior.QT)
									{
										actions.Add(g => g.copyOrderQT);
										actions.Add(g => g.printQuote);
									}
									else
									{
										actions.Add(g => g.copyOrder);

										if (behavior == SOBehavior.BL)
										{
											actions.Add<Blanket>(g => g.printBlanket);
										}
										else
										{
											actions.Add(g => g.printSalesOrder);
										}
									}
								});
						});
					})
					.WithTransitions(transitions =>
					{
						transitions.UpdateGroupFrom(initialState, ts =>
						{
							ts.Add(t => t // New Pending Approval
								.To<State.pendingApproval>()
								.IsTriggeredOn(g => g.initializeState)
								.When(!conditions.IsApproved)
								.PlaceAfter(tr => tr.To<State.hold>())
								.WithFieldAssignments(fas =>
								{
									if (inclCustOpenOrders)
										fas.Add<inclCustOpenOrders>(false);
								}));
						});

						transitions.UpdateGroupFrom<State.hold>(ts =>
						{
							ts.Add(t => t
								.To<State.pendingApproval>()
								.IsTriggeredOn(g => g.releaseFromHold)
								.When(!conditions.IsApproved)
								.PlaceBefore(tr => behavior == SOBehavior.SO
									? tr.To<State.pendingProcessing>()
									: tr.To<State.open>()));
						});

						transitions.UpdateGroupFrom<State.cancelled>(ts =>
						{
							if (behavior == SOBehavior.SO)
							{
								ts.Update(
									t => t.To<State.pendingProcessing>().IsTriggeredOn(g => g.reopenOrder),
									t => t.When(conditions.IsApproved && soConditions.HasPaymentsInPendingProcessing));

								ts.Update(
									t => t.To<State.awaitingPayment>().IsTriggeredOn(g => g.reopenOrder),
									t => t.When(conditions.IsApproved && soConditions.IsPaymentRequirementsViolated));
							}

							ts.Update(
								t => t.To<State.open>().IsTriggeredOn(g => g.reopenOrder),
								t => t.When(conditions.IsApproved && conditions.IsNotHoldEntryAndNotLSEntryEnabled));

							ts.Add(t => t
								.To<State.pendingApproval>()
								.IsTriggeredOn(g => g.reopenOrder)
								.When(!conditions.IsApproved && conditions.IsNotHoldEntryAndNotLSEntryEnabled));
						});

						if (includeCreditHold)
						{
							transitions.UpdateGroupFrom<State.creditHold>(ts =>
							{
								ts.Add(t => t
									.To<State.pendingApproval>()
									.IsTriggeredOn(g => g.releaseFromCreditHold)
									.When(!conditions.IsApproved)
									.PlaceBefore(tr => tr.To<State.open>())
									.WithFieldAssignments(fas =>
									{
										fas.Add<creditHold>(false);
									}));
								ts.Add(t => t
									.To<State.pendingApproval>()
									.IsTriggeredOn(g => g.OnCreditLimitSatisfied)
									.When(!conditions.IsApproved)
									.PlaceBefore(tr => tr.To<State.open>())
									.WithFieldAssignments(fas =>
									{
										fas.Add<creditHold>(false);
									}));
							});
						}

						transitions.AddGroupFrom<State.pendingApproval>(ts =>
						{
							if (behavior == SOBehavior.SO)
							{
								ts.Add(t => t
									.To<State.pendingProcessing>()
									.IsTriggeredOn(approve)
									.When(conditions.IsApproved && soConditions.HasPaymentsInPendingProcessing));
								ts.Add(t => t
									.To<State.awaitingPayment>()
									.IsTriggeredOn(approve)
									.When(conditions.IsApproved && soConditions.IsPaymentRequirementsViolated));
							}

							ts.Add(t => t
								.To<State.open>()
								.IsTriggeredOn(approve)
								.When(conditions.IsApproved)
								.WithFieldAssignments(fas =>
								{
									if (inclCustOpenOrders)
										fas.Add<inclCustOpenOrders>(true);
								}));
							ts.Add(t => t
								.To<State.voided>()
								.IsTriggeredOn(reject)
								.When(conditions.IsRejected));
							ts.Add(t => t
								.To<State.hold>()
								.IsTriggeredOn(g => g.putOnHold));
						});

						transitions.AddGroupFrom<State.voided>(ts =>
						{
							ts.Add(t => t
								.To<State.hold>()
								.IsTriggeredOn(g => g.putOnHold));
						});
					});
			}

			context.UpdateScreenConfigurationFor(screen =>
			{
				return screen
					.WithFlows(flows =>
					{
						flows.Update<SOBehavior.sO>(f => InjectApprovalWorkflow(f, SOBehavior.SO));
						flows.Update<SOBehavior.tR>(f => InjectApprovalWorkflow(f, SOBehavior.TR));
						flows.Update<SOBehavior.qT>(f => InjectApprovalWorkflow(f, SOBehavior.QT));
						flows.Update<SOBehavior.rM>(f => InjectApprovalWorkflow(f, SOBehavior.RM));
						flows.Update<SOBehavior.iN>(f => InjectApprovalWorkflow(f, SOBehavior.IN));
						flows.Update<SOBehavior.cM>(f => InjectApprovalWorkflow(f, SOBehavior.CM));
						flows.Update<SOBehavior.bL>(f => InjectApprovalWorkflow(f, SOBehavior.BL));
						flows.Update<SOBehavior.mO>(f => InjectApprovalWorkflow(f, SOBehavior.MO));
					})
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

					});
			});
		}

		protected static void HideApproveAndRejectActions(WorkflowContext<SOOrderEntry, SOOrder> context)
		{
			var approveHidden = context.ActionDefinitions
				.CreateExisting<SOOrderEntry_ApprovalWorkflow>(g => g.approve, a => a
				.WithCategory(PredefinedCategory.Actions)
				.IsHiddenAlways());
			var rejectHidden = context.ActionDefinitions
				.CreateExisting<SOOrderEntry_ApprovalWorkflow>(g => g.reject, a => a
				.WithCategory(PredefinedCategory.Actions)
				.IsHiddenAlways());
			var reassignHidden = context.ActionDefinitions
				.CreateExisting(nameof(SOOrderEntry.Approval.ReassignApproval), a => a
				.WithCategory(PredefinedCategory.Actions)
				.IsHiddenAlways());

			context.UpdateScreenConfigurationFor(screen =>
			{
				return screen
					.WithActions(actions =>
					{
						actions.Add(approveHidden);
						actions.Add(rejectHidden);
						actions.Add(reassignHidden);
					});
			});
		}

		public PXAction<SOOrder> approve;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Approve", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable Approve(PXAdapter adapter) => adapter.Get();

		public PXAction<SOOrder> reject;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Reject", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable Reject(PXAdapter adapter) => adapter.Get();
	}
}
