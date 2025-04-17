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
	using static SOOrder;
	using static BoundedTo<SOOrderEntry, SOOrder>;

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class WorkflowMO : WorkflowBase
	{
		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<SOOrderEntry, SOOrder>());
		protected static void Configure(WorkflowContext<SOOrderEntry, SOOrder> context)
		{
			var conditions = context.Conditions.GetPack<WorkflowIN.Conditions>();
			context.UpdateScreenConfigurationFor(screen => screen.WithFlows(flows =>
			{
				flows.Add<SOBehavior.mO>(flow => flow
					.WithFlowStates(flowStates =>
					{
						flowStates.Add(State.Initial, fs => fs.IsInitial(g => g.initializeState));
						flowStates.Add<State.hold>(flowState =>
						{
							return flowState
								.WithActions(actions =>
								{
									actions.Add(g => g.releaseFromHold, a => a.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
									actions.Add(g => g.printSalesOrder);
									actions.Add(g => g.cancelOrder);
									actions.Add(g => g.copyOrder);
									actions.Add(g => g.validateAddresses);
									actions.Add(g => g.recalculateDiscountsAction);
								});
						});
						flowStates.Add<State.creditHold>(flowState =>
						{
							return flowState
								.WithActions(actions =>
								{
									actions.Add(g => g.releaseFromCreditHold, a => a.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
									actions.Add(g => g.printSalesOrder);
									actions.Add(g => g.cancelOrder);
									actions.Add(g => g.putOnHold);
									actions.Add(g => g.copyOrder);
									actions.Add(g => g.validateAddresses);
								})
								.WithEventHandlers(handlers =>
								{
									handlers.Add(g => g.OnCreditLimitSatisfied);
								});
						});
						flowStates.Add<State.open>(flowState =>
						{
							return flowState
								.WithActions(actions =>
								{
									actions.Add(g => g.putOnHold, a => a.IsDuplicatedInToolbar());
									actions.Add<SOOrderEntry.SOQuickProcess>(g => g.quickProcess, a => a.IsDuplicatedInToolbar());
									actions.Add(g => g.prepareInvoice, a => a.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
									actions.Add(g => g.printSalesOrder);
									actions.Add(g => g.cancelOrder);
									actions.Add(g => g.copyOrder);
									actions.Add(g => g.validateAddresses);
									actions.Add(g => g.recalculateDiscountsAction);
									actions.Add(g => g.emailSalesOrder);
								})
								.WithEventHandlers(handlers =>
								{
									handlers.Add(g => g.OnInvoiceLinked);
									handlers.Add(g => g.OnCreditLimitViolated);
								});
						});
						flowStates.Add<State.invoiced>(flowState =>
						{
							return flowState
								.WithActions(actions =>
								{
									actions.Add(g => g.printSalesOrder);
									actions.Add(g => g.copyOrder);
									actions.Add(g => g.validateAddresses);
								})
								.WithEventHandlers(handlers =>
								{
									handlers.Add(g => g.OnInvoiceReleased);
									handlers.Add(g => g.OnInvoiceUnlinked);
								})
								.WithFieldStates(DisableWholeScreen);
						});
						flowStates.Add<State.completed>(flowState =>
						{
							return flowState
								.WithActions(actions =>
								{
									actions.Add(g => g.printSalesOrder);
									actions.Add(g => g.copyOrder);
									actions.Add(g => g.validateAddresses);
								})
								.WithFieldStates(DisableWholeScreen);
						});
						flowStates.Add<State.cancelled>(flowState =>
						{
							return flowState
								.WithActions(actions =>
								{
									actions.Add(g => g.reopenOrder, a => a.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
									actions.Add(g => g.copyOrder, a => a.IsDuplicatedInToolbar());
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
								.To<State.hold>()
								.IsTriggeredOn(g => g.initializeState)
								.When(conditions.IsOnHold)
								.WithFieldAssignments(fas =>
								{
									fas.Add<inclCustOpenOrders>(false);
								}));
							ts.Add(t => t
								.To<State.creditHold>()
								.IsTriggeredOn(g => g.initializeState)
								.When(conditions.IsOnCreditHold));
							ts.Add(t => t
								.To<State.open>()
								.IsTriggeredOn(g => g.initializeState)
								.WithFieldAssignments(fas =>
								{
									fas.Add<inclCustOpenOrders>(true);
								}));
						});
						transitions.AddGroupFrom<State.hold>(ts =>
						{
							ts.Add(t => t
								.To<State.open>()
								.IsTriggeredOn(g => g.releaseFromHold)
								.WithFieldAssignments(fas =>
								{
									fas.Add<inclCustOpenOrders>(true);
								}));
							ts.Add(t => t.To<State.cancelled>().IsTriggeredOn(g => g.cancelOrder));
						});
						transitions.AddGroupFrom<State.creditHold>(ts =>
						{
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
								.To<State.hold>()
								.IsTriggeredOn(g => g.putOnHold)
								.WithFieldAssignments(fas =>
								{
									fas.Add<creditHold>(false);
									fas.Add<inclCustOpenOrders>(false);
								}));
							ts.Add(t => t.To<State.cancelled>().IsTriggeredOn(g => g.cancelOrder));
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
							ts.Add(t => t.To<State.invoiced>().IsTriggeredOn(g => g.OnInvoiceLinked));
							ts.Add(t => t.To<State.cancelled>().IsTriggeredOn(g => g.cancelOrder));
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
						transitions.AddGroupFrom<State.invoiced>(ts =>
						{
							ts.Add(t => t
								.To<State.completed>()
								.IsTriggeredOn(g => g.OnInvoiceReleased)
								.When(conditions.HasAllBillsReleased)
								.WithFieldAssignments(fas =>
								{
									fas.Add<completed>(true);
								}));
							ts.Add(t => t
								.To<State.open>()
								.IsTriggeredOn(g => g.OnInvoiceUnlinked)
								.When(conditions.IsUnbilled));
						});
						transitions.AddGroupFrom<State.cancelled>(ts =>
						{
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
					}));
			}));
		}
	}
}
