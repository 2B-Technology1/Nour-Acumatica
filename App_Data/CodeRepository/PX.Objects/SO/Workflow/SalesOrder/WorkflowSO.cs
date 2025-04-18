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

namespace PX.Objects.SO.Workflow.SalesOrder
{
	using State = SOOrderStatus;
	using CreatePaymentExt = GraphExtensions.SOOrderEntryExt.CreatePaymentExt;
	using static SOOrder;
	using static BoundedTo<SOOrderEntry, SOOrder>;

	public class WorkflowSO : WorkflowBase
	{
		public new class Conditions : WorkflowBase.Conditions
		{
			public Condition HasPaymentsInPendingProcessing => GetOrCreate(b => b.FromBql<
				paymentsNeedValidationCntr.IsGreater<Zero>
			>());

			public Condition IsPaymentRequirementsViolated => GetOrCreate(b => b.FromBql<
				prepaymentReqSatisfied.IsEqual<False>
			>());

			public Condition IsShippable => GetOrCreate(b => b.FromBql<
				openShipmentCntr.IsEqual<Zero>.
				And<openLineCntr.IsGreater<Zero>>
			>());

			public Condition IsShippingCompleted => GetOrCreate(b => b.FromBql<
				completed.IsEqual<True>.
				Or<
					openShipmentCntr.IsEqual<Zero>.
					And<openLineCntr.IsEqual<Zero>>>
			>());

			public Condition HasOpenShipments => GetOrCreate(b => b.FromBql<
				openShipmentCntr.IsGreater<Zero>
			>());
		}

		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<SOOrderEntry, SOOrder>());
		protected static void Configure(WorkflowContext<SOOrderEntry, SOOrder> context)
		{
			var conditions = context.Conditions.GetPack<Conditions>();
			context.UpdateScreenConfigurationFor(screen => screen.WithFlows(flows =>
			{
				flows.Add<SOBehavior.sO>(flow => flow
					.WithFlowStates(flowStates =>
					{
						flowStates.Add(State.Initial, fs => fs.IsInitial(g => g.initializeState));
						flowStates.Add<State.hold>(flowState =>
						{
							return flowState
								.WithActions(actions =>
								{
									actions.Add(g => g.releaseFromHold, a => a.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
									actions.Add(g => g.cancelOrder);
									actions.Add(g => g.printSalesOrder);
									actions.Add(g => g.copyOrder);
									actions.Add(g => g.validateAddresses);
									actions.Add(g => g.recalculateDiscountsAction);
									actions.Add<SOOrderEntryExternalTax>(e => e.recalcExternalTax);
								})
								.WithEventHandlers(handlers =>
								{
									handlers.Add(g => g.OnShipmentCorrected);
								});
						});
						flowStates.Add<State.creditHold>(flowState =>
						{
							return flowState
								.WithActions(actions =>
								{
									actions.Add(g => g.releaseFromCreditHold, a => a.IsDuplicatedInToolbar());
									actions.Add(g => g.cancelOrder);
									actions.Add(g => g.printSalesOrder);
									actions.Add(g => g.putOnHold);
									actions.Add(g => g.emailSalesOrder);
									actions.Add(g => g.copyOrder);
									actions.Add(g => g.validateAddresses);
									actions.Add<CreatePaymentExt>(e => e.createAndCapturePayment);
									actions.Add<CreatePaymentExt>(e => e.createAndAuthorizePayment);
								})
								.WithEventHandlers(handlers =>
								{
									handlers.Add(g => g.OnPaymentRequirementsViolated);
									handlers.Add(g => g.OnObtainedPaymentInPendingProcessing);
									handlers.Add(g => g.OnCreditLimitSatisfied);
								});
						});
						flowStates.Add<State.pendingProcessing>(flowState =>
						{
							return flowState
								.WithActions(actions =>
								{
									actions.Add(g => g.putOnHold, a => a.IsDuplicatedInToolbar());
									actions.Add(g => g.cancelOrder);
									actions.Add(g => g.printSalesOrder);
									actions.Add(g => g.emailSalesOrder);
									actions.Add(g => g.copyOrder);
									actions.Add(g => g.validateAddresses);
									actions.Add<CreatePaymentExt>(e => e.createAndCapturePayment);
									actions.Add<CreatePaymentExt>(e => e.createAndAuthorizePayment);
								})
								.WithEventHandlers(handlers =>
								{
									handlers.Add(g => g.OnLostLastPaymentInPendingProcessing);
									handlers.Add(g => g.OnCreditLimitViolated);
								});
						});
						flowStates.Add<State.awaitingPayment>(flowState =>
						{
							return flowState
								.WithActions(actions =>
								{
									actions.Add(g => g.putOnHold, a => a.IsDuplicatedInToolbar());
									actions.Add(g => g.cancelOrder);
									actions.Add(g => g.printSalesOrder);
									actions.Add(g => g.emailSalesOrder);
									actions.Add(g => g.copyOrder);
									actions.Add(g => g.validateAddresses);
									actions.Add<CreatePaymentExt>(e => e.createAndCapturePayment);
									actions.Add<CreatePaymentExt>(e => e.createAndAuthorizePayment);
								})
								.WithEventHandlers(handlers =>
								{
									handlers.Add(g => g.OnPaymentRequirementsSatisfied);
									handlers.Add(g => g.OnObtainedPaymentInPendingProcessing);
								});
						});
						flowStates.Add<State.open>(flowState =>
						{
							return flowState
								.WithActions(actions =>
								{
									actions.Add<SOOrderEntry.SOQuickProcess>(g => g.quickProcess, a => a.WithConnotation(ActionConnotation.Success).IsDuplicatedInToolbar());
									actions.Add(g => g.createShipmentIssue, a => a.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
									actions.Add(g => g.putOnHold, a => a.IsDuplicatedInToolbar());
									actions.Add(g => g.printSalesOrder);
									actions.Add(g => g.prepareInvoice);
									actions.Add(g => g.cancelOrder);
									actions.Add(g => g.completeOrder);
									actions.Add(g => g.placeOnBackOrder);
									actions.Add(g => g.createPurchaseOrder);
									actions.Add(g => g.createTransferOrder);
									actions.Add(g => g.emailSalesOrder);
									actions.Add(g => g.copyOrder);
									actions.Add(g => g.validateAddresses);
									actions.Add(g => g.recalculateDiscountsAction);
									actions.Add<SOOrderEntryExternalTax>(e => e.recalcExternalTax);
									actions.Add<CreatePaymentExt>(e => e.createAndCapturePayment);
									actions.Add<CreatePaymentExt>(e => e.createAndAuthorizePayment);
								})
								.WithEventHandlers(handlers =>
								{
									handlers.Add(g => g.OnShipmentLinked);
									handlers.Add(g => g.OnShipmentCorrected);
									handlers.Add(g => g.OnPaymentRequirementsViolated);
									handlers.Add(g => g.OnObtainedPaymentInPendingProcessing);
									handlers.Add(g => g.OnCreditLimitViolated);
									handlers.Add(g => g.OnShipmentCreationFailed);
								});
						});
						flowStates.Add<State.shipping>(flowState =>
						{
							return flowState
								.WithActions(actions =>
								{
									actions.Add(g => g.createShipmentIssue);
									actions.Add(g => g.emailSalesOrder);
									actions.Add(g => g.createPurchaseOrder);
									actions.Add<CreatePaymentExt>(e => e.createAndCapturePayment);
									actions.Add<CreatePaymentExt>(e => e.createAndAuthorizePayment);
								})
								.WithEventHandlers(handlers =>
								{
									handlers.Add(g => g.OnShipmentUnlinked);
									handlers.Add(g => g.OnShipmentConfirmed);
								})
								.WithFieldStates(DisableWholeScreen);
						});
						flowStates.Add<State.backOrder>(flowState =>
						{
							return flowState
								.WithActions(actions =>
								{
									actions.Add(g => g.openOrder, a => a.IsDuplicatedInToolbar());
									actions.Add(g => g.createShipmentIssue, a => a.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
									actions.Add<SOOrderEntry.SOQuickProcess>(g => g.quickProcess, a => a.IsDuplicatedInToolbar());
									actions.Add(g => g.cancelOrder);
									actions.Add(g => g.completeOrder);
									actions.Add(g => g.putOnHold);
									actions.Add(g => g.prepareInvoice);
									actions.Add(g => g.emailSalesOrder);
									actions.Add(g => g.copyOrder);
									actions.Add(g => g.validateAddresses);
									actions.Add(g => g.createPurchaseOrder);
									actions.Add(g => g.createTransferOrder);
									actions.Add<CreatePaymentExt>(e => e.createAndCapturePayment);
									actions.Add<CreatePaymentExt>(e => e.createAndAuthorizePayment);
								})
								.WithEventHandlers(handlers =>
								{
									handlers.Add(g => g.OnShipmentLinked);
									handlers.Add(g => g.OnShipmentCorrected);
									handlers.Add(g => g.OnPaymentRequirementsViolated);
									handlers.Add(g => g.OnObtainedPaymentInPendingProcessing);
									handlers.Add(g => g.OnCreditLimitViolated);
								});
						});
						flowStates.Add<State.completed>(flowState =>
						{
							return flowState
								.WithActions(actions =>
								{
									actions.Add(g => g.prepareInvoice, a => a.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
									actions.Add(g => g.printSalesOrder);
									actions.Add(g => g.copyOrder);
									actions.Add(g => g.validateAddresses);
									actions.Add(g => g.reopenOrder);
								})
								.WithEventHandlers(handlers =>
								{
									handlers.Add(g => g.OnShipmentCorrected);
								})
								.WithFieldStates(DisableWholeScreen);
						});
						flowStates.Add<State.cancelled>(flowState =>
						{
							return flowState
								.WithActions(actions =>
								{
									actions.Add(g => g.copyOrder, a => a.IsDuplicatedInToolbar());
									actions.Add(g => g.reopenOrder, a => a.IsDuplicatedInToolbar());
									actions.Add(g => g.printSalesOrder);
									actions.Add(g => g.validateAddresses);
								})
								.WithFieldStates(DisableWholeScreen);
						});
					})
					.WithTransitions(transitions =>
					{
						transitions.AddGroupFrom(State.Initial, ts =>
						{
							ts.Add(t => t
								.To<State.hold>() // SO New Hold
								.IsTriggeredOn(g => g.initializeState)
								.When(conditions.IsOnHold)
								.WithFieldAssignments(fas =>
								{
									fas.Add<inclCustOpenOrders>(false);
								}));
							ts.Add(t => t
								.To<State.pendingProcessing>()
								.IsTriggeredOn(g => g.initializeState)
								.When(conditions.HasPaymentsInPendingProcessing)
								.WithFieldAssignments(fas =>
								{
									fas.Add<inclCustOpenOrders>(false);
								}));
							ts.Add(t => t
								.To<State.awaitingPayment>()
								.IsTriggeredOn(g => g.initializeState)
								.When(conditions.IsPaymentRequirementsViolated)
								.WithFieldAssignments(fas =>
								{
									fas.Add<inclCustOpenOrders>(false);
								}));
							ts.Add(t => t
								.To<State.creditHold>() // SO New Credit Hold
								.IsTriggeredOn(g => g.initializeState)
								.When(conditions.IsOnCreditHold)
								.WithFieldAssignments(fas =>
								{
									fas.Add<hold>(false);
									fas.Add<inclCustOpenOrders>(false);
								}));
							ts.Add(t => t
								.To<State.completed>() // SO New Completed
								.IsTriggeredOn(g => g.initializeState)
								.When(conditions.IsCompleted)
								.WithFieldAssignments(fas =>
								{
									fas.Add<completed>(true);
								}));
							ts.Add(t => t
								.To<State.open>() // SO New Open
								.IsTriggeredOn(g => g.initializeState)
								.WithFieldAssignments(fas =>
								{
									fas.Add<inclCustOpenOrders>(true);
								}));
						});
						transitions.AddGroupFrom<State.hold>(ts =>
						{
							ts.Add(t => t
								.To<State.pendingProcessing>()
								.IsTriggeredOn(g => g.releaseFromHold)
								.When(conditions.HasPaymentsInPendingProcessing));
							ts.Add(t => t
								.To<State.awaitingPayment>()
								.IsTriggeredOn(g => g.releaseFromHold)
								.When(conditions.IsPaymentRequirementsViolated));
							ts.Add(t => t
								.To<State.shipping>()
								.IsTriggeredOn(g => g.OnShipmentCorrected)
								.When(conditions.HasOpenShipments));
							ts.Add(t => t
								.To<State.open>()
								.IsTriggeredOn(g => g.releaseFromHold)
								.WithFieldAssignments(fas =>
								{
									fas.Add<inclCustOpenOrders>(true);
								}));
							ts.Add(t => t
								.To<State.cancelled>()
								.IsTriggeredOn(g => g.cancelOrder)
								.When(conditions.IsCancelled));
						});
						transitions.AddGroupFrom<State.creditHold>(ts =>
						{
							ts.Add(t => t
								.To<State.hold>()
								.IsTriggeredOn(g => g.putOnHold)
								.WithFieldAssignments(fas =>
								{
									fas.Add<creditHold>(false);
								}));
							ts.Add(t => t
								.To<State.pendingProcessing>()
								.IsTriggeredOn(g => g.OnObtainedPaymentInPendingProcessing)
								.WithFieldAssignments(fas =>
								{
									fas.Add<creditHold>(false);
								}));
							ts.Add(t => t
								.To<State.awaitingPayment>()
								.IsTriggeredOn(g => g.OnPaymentRequirementsViolated)
								.WithFieldAssignments(fas =>
								{
									fas.Add<creditHold>(false);
								}));
							ts.Add(t => t
								.To<State.open>()
								.IsTriggeredOn(g => g.releaseFromCreditHold)
								.WithFieldAssignments(fas =>
								{
									fas.Add<creditHold>(false);
									fas.Add<inclCustOpenOrders>(true);
								}));
							ts.Add(t => t
								.To<State.open>()
								.IsTriggeredOn(g => g.OnCreditLimitSatisfied)
								.WithFieldAssignments(fas =>
								{
									fas.Add<creditHold>(false);
									fas.Add<inclCustOpenOrders>(true);
								}));
							ts.Add(t => t
								.To<State.cancelled>()
								.IsTriggeredOn(g => g.cancelOrder)
								.When(conditions.IsCancelled));
						});
						transitions.AddGroupFrom<State.pendingProcessing>(ts =>
						{
							ts.Add(t => t
								.To<State.hold>()
								.IsTriggeredOn(g => g.putOnHold));
							ts.Add(t => t
								.To<State.awaitingPayment>()
								.IsTriggeredOn(g => g.OnLostLastPaymentInPendingProcessing)
								.When(conditions.IsPaymentRequirementsViolated));
							ts.Add(t => t
								.To<State.open>()
								.IsTriggeredOn(g => g.OnLostLastPaymentInPendingProcessing)
								.WithFieldAssignments(fas =>
								{
									fas.Add<inclCustOpenOrders>(true);
								}));
							ts.Add(t => t
								.To<State.cancelled>()
								.IsTriggeredOn(g => g.cancelOrder)
								.When(conditions.IsCancelled));
							ts.Add(t => t
								.To<State.creditHold>()
								.IsTriggeredOn(g => g.OnCreditLimitViolated)
								.WithFieldAssignments(fas =>
								{
									fas.Add<creditHold>(e => e.SetFromValue(true));
									fas.Add<hold>(e => e.SetFromValue(false));
									fas.Add<inclCustOpenOrders>(e => e.SetFromValue(false));
								}));
						});
						transitions.AddGroupFrom<State.awaitingPayment>(ts =>
						{
							ts.Add(t => t
								.To<State.hold>()
								.IsTriggeredOn(g => g.putOnHold));
							ts.Add(t => t
								.To<State.pendingProcessing>()
								.IsTriggeredOn(g => g.OnObtainedPaymentInPendingProcessing));
							ts.Add(t => t
								.To<State.open>()
								.IsTriggeredOn(g => g.OnPaymentRequirementsSatisfied)
								.WithFieldAssignments(fas =>
								{
									fas.Add<inclCustOpenOrders>(true);
								}));
							ts.Add(t => t
								.To<State.cancelled>()
								.IsTriggeredOn(g => g.cancelOrder)
								.When(conditions.IsCancelled));
							ts.Add(t => t
								.To<State.creditHold>()
								.IsTriggeredOn(g => g.OnCreditLimitViolated)
								.WithFieldAssignments(fas =>
								{
									fas.Add<creditHold>(e => e.SetFromValue(true));
									fas.Add<hold>(e => e.SetFromValue(false));
									fas.Add<inclCustOpenOrders>(e => e.SetFromValue(false));
								}));
						});
						transitions.AddGroupFrom<State.open>(ts =>
						{
							ts.Add(t => t
								.To<State.hold>()
								.IsTriggeredOn(g => g.putOnHold)
								.WithFieldAssignments(fas =>
								{
									fas.Add<inclCustOpenOrders>(false);
								}));
							ts.Add(t => t
								.To<State.cancelled>()
								.IsTriggeredOn(g => g.cancelOrder)
								.When(conditions.IsCancelled));
							ts.Add(t => t
								.To<State.shipping>()
								.IsTriggeredOn(g => g.OnShipmentLinked)
								.When(conditions.HasOpenShipments));
							ts.Add(t => t
								.To<State.shipping>()
								.IsTriggeredOn(g => g.OnShipmentCorrected)
								.When(conditions.HasOpenShipments));
							ts.Add(t => t
								.To<State.backOrder>()
								.IsTriggeredOn(g => g.placeOnBackOrder)
								.WithFieldAssignments(fas =>
								{
									fas.Add<backOrdered>(true);
								}));
							ts.Add(t => t
								.To<State.backOrder>()
								.IsTriggeredOn(g => g.OnShipmentCreationFailed)
								.WithFieldAssignments(fas =>
								{
									fas.Add<backOrdered>(true);
								}));
							ts.Add(t => t
								.To<State.pendingProcessing>()
								.IsTriggeredOn(g => g.OnObtainedPaymentInPendingProcessing)
								.WithFieldAssignments(fas =>
								{
									fas.Add<inclCustOpenOrders>(false);
								}));
							ts.Add(t => t
								.To<State.awaitingPayment>()
								.IsTriggeredOn(g => g.OnPaymentRequirementsViolated)
								.WithFieldAssignments(fas =>
								{
									fas.Add<inclCustOpenOrders>(false);
								}));
							ts.Add(t => t
								.To<State.creditHold>()
								.IsTriggeredOn(g => g.OnCreditLimitViolated)
								.WithFieldAssignments(fas =>
								{
									fas.Add<creditHold>(true);
									fas.Add<hold>(false);
									fas.Add<inclCustOpenOrders>(false);
								}));
						});
						transitions.AddGroupFrom<State.shipping>(ts =>
						{
							ts.Add(t => t
								.To<State.pendingProcessing>()
								.IsTriggeredOn(g => g.OnShipmentUnlinked)
								.When(conditions.HasPaymentsInPendingProcessing)
								.WithFieldAssignments(fas => fas.Add<inclCustOpenOrders>(false)));
							ts.Add(t => t
								.To<State.awaitingPayment>()
								.IsTriggeredOn(g => g.OnShipmentUnlinked)
								.When(conditions.IsPaymentRequirementsViolated)
								.WithFieldAssignments(fas => fas.Add<inclCustOpenOrders>(false)));
							ts.Add(t => t
								.To<State.open>() // SO Shipping Open
								.IsTriggeredOn(g => g.OnShipmentUnlinked)
								.When(conditions.IsShippable)
								.WithFieldAssignments(fas => fas.Add<inclCustOpenOrders>(true)));
							ts.Add(t => t
								.To<State.completed>() // SO Shipping Completed / SO New Completed
								.IsTriggeredOn(g => g.OnShipmentConfirmed)
								.When(conditions.IsShippingCompleted)
								.WithFieldAssignments(fas => fas.Add<completed>(true)));
							ts.Add(t => t
								.To<State.pendingProcessing>()
								.IsTriggeredOn(g => g.OnShipmentConfirmed)
								.When(conditions.HasPaymentsInPendingProcessing)
								.WithFieldAssignments(fas => fas.Add<inclCustOpenOrders>(false)));
							ts.Add(t => t
								.To<State.awaitingPayment>()
								.IsTriggeredOn(g => g.OnShipmentConfirmed)
								.When(conditions.IsPaymentRequirementsViolated)
								.WithFieldAssignments(fas => fas.Add<inclCustOpenOrders>(false)));
							ts.Add(t => t
								.To<State.backOrder>() // SO Shipping BackOrder
								.IsTriggeredOn(g => g.OnShipmentConfirmed)
								.When(conditions.IsShippable)
								.WithFieldAssignments(fas => fas.Add<backOrdered>(true)));
						});
						transitions.AddGroupFrom<State.backOrder>(ts =>
						{
							ts.Add(t => t
								.To<State.hold>()
								.IsTriggeredOn(g => g.putOnHold)
								.WithFieldAssignments(fas =>
								{
									fas.Add<backOrdered>(false);
									fas.Add<inclCustOpenOrders>(false);
								}));
							ts.Add(t => t
								.To<State.pendingProcessing>()
								.IsTriggeredOn(g => g.OnObtainedPaymentInPendingProcessing)
								.WithFieldAssignments(fas =>
								{
									fas.Add<backOrdered>(false);
									fas.Add<inclCustOpenOrders>(false);
								}));
							ts.Add(t => t
								.To<State.awaitingPayment>()
								.IsTriggeredOn(g => g.OnPaymentRequirementsViolated)
								.WithFieldAssignments(fas =>
								{
									fas.Add<backOrdered>(false);
									fas.Add<inclCustOpenOrders>(false);
								}));
							ts.Add(t => t
								.To<State.creditHold>()
								.IsTriggeredOn(g => g.OnCreditLimitViolated)
								.WithFieldAssignments(fas =>
								{
									fas.Add<creditHold>(true);
									fas.Add<backOrdered>(false);
									fas.Add<inclCustOpenOrders>(false);
								}));
							ts.Add(t => t
								.To<State.open>()
								.IsTriggeredOn(g => g.openOrder)
								.WithFieldAssignments(fas =>
								{
									fas.Add<backOrdered>(false);
									fas.Add<inclCustOpenOrders>(true);
								}));
							ts.Add(t => t
								.To<State.shipping>()
								.IsTriggeredOn(g => g.OnShipmentLinked)
								.When(conditions.HasOpenShipments));
							ts.Add(t => t
								.To<State.shipping>()
								.IsTriggeredOn(g => g.OnShipmentCorrected)
								.When(conditions.HasOpenShipments));
							ts.Add(t => t
								.To<State.cancelled>()
								.IsTriggeredOn(g => g.cancelOrder)
								.When(conditions.IsCancelled));
						});
						transitions.AddGroupFrom<State.completed>(ts =>
						{
							ts.Add(t => t
								.To<State.shipping>()
								.IsTriggeredOn(g => g.OnShipmentCorrected)
								.When(conditions.HasOpenShipments));
							ts.Add(t => t
								.To<State.pendingProcessing>()
								.IsTriggeredOn(g => g.reopenOrder)
								.When(conditions.HasPaymentsInPendingProcessing)
								.WithFieldAssignments(fas => fas.Add<inclCustOpenOrders>(false)));
							ts.Add(t => t
								.To<State.awaitingPayment>()
								.IsTriggeredOn(g => g.reopenOrder)
								.When(conditions.IsPaymentRequirementsViolated)
								.WithFieldAssignments(fas => fas.Add<inclCustOpenOrders>(false)));
							ts.Add(t => t
								.To<State.backOrder>()
								.IsTriggeredOn(g => g.reopenOrder)
								.WithFieldAssignments(fas => fas.Add<backOrdered>(false)));
						});
						transitions.AddGroupFrom<State.cancelled>(ts =>
						{
							ts.Add(t => t
								.To<State.pendingProcessing>()
								.IsTriggeredOn(g => g.reopenOrder)
								.When(conditions.HasPaymentsInPendingProcessing)
								.WithFieldAssignments(fas =>
								{
									fas.Add<inclCustOpenOrders>(false);
									fas.Add<approved>(v => v.SetFromValue(true));
								}));
							ts.Add(t => t
								.To<State.awaitingPayment>()
								.IsTriggeredOn(g => g.reopenOrder)
								.When(conditions.IsPaymentRequirementsViolated)
								.WithFieldAssignments(fas =>
								{
									fas.Add<inclCustOpenOrders>(false);
									fas.Add<approved>(v => v.SetFromValue(true));
								}));
							ts.Add(t => t
								.To<State.hold>()
								.IsTriggeredOn(g => g.reopenOrder)
								.When(conditions.IsHoldEntryOrLSEntryEnabled)
								.WithFieldAssignments(fas =>
								{
									fas.Add<hold>(v => v.SetFromValue(true));
								}));
							ts.Add(t => t
								.To<State.open>()
								.IsTriggeredOn(g => g.reopenOrder)
								.WithFieldAssignments(fas =>
								{
									fas.Add<inclCustOpenOrders>(true);
									fas.Add<approved>(v => v.SetFromValue(true));
								}));
						});
					})
				);
			}));
		}
	}
}
