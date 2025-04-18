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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.WorkflowAPI;
using PX.Objects.Common;
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FS
{
    using State = ListField.ServiceOrderStatus;
    using static FSServiceOrder;
    using static BoundedTo<ServiceOrderEntry, FSServiceOrder>;

    public class ServiceOrderEntryWorkflow : PXGraphExtension<ServiceOrderEntry>
    {
        public sealed override void Configure(PXScreenConfiguration config) =>
            Configure(config.GetScreenConfigurationContext<ServiceOrderEntry, FSServiceOrder>());

        protected static void Configure(WorkflowContext<ServiceOrderEntry, FSServiceOrder> context)
        {
            #region Conditions
            BoundedTo<ServiceOrderEntry, FSServiceOrder>.Condition Bql<T>() where T : IBqlUnary, new() => context.Conditions.FromBql<T>();
            var conditions = new
            {
                IsOnHold
                    = Bql<hold.IsEqual<True>>(),

                IsNotOnHold
                    = Bql<hold.IsEqual<False>>(),

                IsQuote
                    = Bql<quote.IsEqual<True>>(),

                IsOpen
                    = Bql<openDoc.IsEqual<True>>(),

                IsCompleted
                    = Bql<completed.IsEqual<True>>(),

                IsCanceled
                    = Bql<canceled.IsEqual<True>>(),

				IsAwaiting
					= Bql<awaiting.IsEqual<True>>(),

                IsClosed
                    = Bql<closed.IsEqual<True>>(),

                IsCopied
                    = Bql<copied.IsEqual<True>>(),

                IsBilled
                    = Bql<billed.IsEqual<True>>(),

                IsAllowedForInvoice
                    = Bql<allowInvoice.IsEqual<True>>(),

                IsNotAllowedForInvoice
                    = Bql<allowInvoice.IsEqual<False>>(),

                UserConfirmedClosing
                    = Bql<userConfirmedClosing.IsEqual<True>>(),

                UserConfirmedUnclosing
                    = Bql<userConfirmedUnclosing.IsEqual<True>>(),

                IsInternalBehavior
                    = Bql<FSSrvOrdType.behavior.FromCurrent.IsEqual<FSSrvOrdType.behavior.Values.internalAppointment>>(),

                PostToSOSIPM
                   = Bql<FSSrvOrdType.postToSOSIPM.FromCurrent.IsEqual<True>>(),

                PostToProjects
                   = Bql<FSSrvOrdType.postTo.FromCurrent.IsEqual<FSPostTo.Projects>>(),

                CustomerIsSpecified
                    = Bql<customerID.IsNotNull>(),

                CustomerIsProspect
                    = Bql<CR.BAccount.type.FromCurrent.IsEqual<CR.BAccountType.prospectType>>(),

                IsFinalState
                    = Bql<canceled.IsEqual<True>.Or<copied.IsEqual<True>.Or<closed.IsEqual<True>
                         .Or<hold.IsEqual<True>>>>>(),

                IsQuickProcessEnabled
                    = Bql<FSSrvOrdType.allowQuickProcess.FromCurrent.IsEqual<True>>(),

				IsBilledByAppointment
					= Bql<Brackets<billingBy.IsNotNull.And<billingBy.FromCurrent.IsEqual<billingBy.Values.Appointment>>>
						.Or<Brackets<billingBy.IsNull.And<FSBillingCycle.billingBy.FromCurrent.IsEqual<billingBy.Values.Appointment>>>>>(),

				IsBilledByContract
                    = Bql<billServiceContractID.IsNotNull
                        .And<FSServiceContract.billingType.FromCurrent.IsEqual<FSServiceContract.billingType.Values.standardizedBillings>>>(),

                HasNoLinesToCreatePurchaseOrder
                   = Bql<pendingPOLineCntr.IsEqual<Zero>>(),

                IsExpenseFeatureEnabled
                   = Bql<FeatureInstalled<FeaturesSet.expenseManagement>>(),

                IsInserted
                  = Bql<sOID.IsLess<Zero>>(),

                RoomFeatureEnabled
                   = Bql<FSSetup.manageRooms.FromCurrent.IsEqual<True>>(),

            }.AutoNameConditions();
            #endregion

            #region Macros
            void DisableScreenByQuote(FieldState.IContainerFillerFields states)
            {
                states.AddTable<FSSOEmployee>(state => state.IsDisabled());
                states.AddTable<FSSOResource>(state => state.IsDisabled());
            }

            void DisableWholeScreen(FieldState.IContainerFillerFields states)
            {
                states.AddTable<FSServiceOrder>(state => state.IsDisabled());

                states.AddTable<FSSODet>(state => state.IsDisabled());
                states.AddTable<FSSODetSplit>(state => state.IsDisabled());
                states.AddTable<FSAddress>(state => state.IsDisabled());
                states.AddTable<FSContact>(state => state.IsDisabled());
                states.AddTable<FSSOEmployee>(state => state.IsDisabled());
                states.AddTable<FSSOResource>(state => state.IsDisabled());
                states.AddTable<FSServiceOrderTax>(state => state.IsDisabled());
                states.AddTable<FSServiceOrderTaxTran>(state => state.IsDisabled());
                // REVIEW AC-178771
                //states.AddTable<*>(state => state.IsDisabled());
            }
            #endregion

            #region Event Handlers

            WorkflowEventHandlerDefinition.IHandlerConfiguredBase OnServiceOrderDeleted(WorkflowEventHandlerDefinition.INeedEventTarget handler)
            {
                return handler
                    .WithTargetOf<FSServiceOrder>()
                    .OfEntityEvent<FSServiceOrder.Events>(e => e.ServiceOrderDeleted)
                    .Is(g => g.OnServiceOrderDeleted)
                    .UsesTargetAsPrimaryEntity()
                    .DisplayName("Service Order Deleted");
            }

            WorkflowEventHandlerDefinition.IHandlerConfiguredBase OnServiceContractCleared(WorkflowEventHandlerDefinition.INeedEventTarget handler)
            {
                return handler
                    .WithTargetOf<FSServiceOrder>()
                    .OfEntityEvent<FSServiceOrder.Events>(e => e.ServiceContractCleared)
                    .Is(g => g.OnServiceContractCleared)
                    .UsesTargetAsPrimaryEntity()
                    .DisplayName("Service Contract Cleared");
            }

            WorkflowEventHandlerDefinition.IHandlerConfiguredBase OnServiceContractPeriodAssigned(WorkflowEventHandlerDefinition.INeedEventTarget handler)
            {
                return handler
                    .WithTargetOf<FSServiceOrder>()
                    .OfEntityEvent<FSServiceOrder.Events>(e => e.ServiceContractPeriodAssigned)
                    .Is(g => g.OnServiceContractPeriodAssigned)
                    .UsesTargetAsPrimaryEntity()
                    .DisplayName("Service Contract Period Assigned");
            }

            WorkflowEventHandlerDefinition.IHandlerConfiguredBase OnRequiredServiceContractPeriodCleared(WorkflowEventHandlerDefinition.INeedEventTarget handler)
            {
                return handler
                    .WithTargetOf<FSServiceOrder>()
                    .OfEntityEvent<FSServiceOrder.Events>(e => e.RequiredServiceContractPeriodCleared)
                    .Is(g => g.OnRequiredServiceContractPeriodCleared)
                    .UsesTargetAsPrimaryEntity()
                    .DisplayName("Required Service Contract Period Cleared");
            }

            WorkflowEventHandlerDefinition.IHandlerConfiguredBase OnLastAppointmentCompleted(WorkflowEventHandlerDefinition.INeedEventTarget handler)
            {
                return handler
                    .WithTargetOf<FSServiceOrder>()
                    .OfEntityEvent<FSServiceOrder.Events>(e => e.LastAppointmentCompleted)
                    .Is(g => g.OnLastAppointmentCompleted)
                    .UsesTargetAsPrimaryEntity()
                    .DisplayName("Last Appointment Completed");
            }

            WorkflowEventHandlerDefinition.IHandlerConfiguredBase OnLastAppointmentCanceled(WorkflowEventHandlerDefinition.INeedEventTarget handler)
            {
                return handler
                    .WithTargetOf<FSServiceOrder>()
                    .OfEntityEvent<FSServiceOrder.Events>(e => e.LastAppointmentCanceled)
                    .Is(g => g.OnLastAppointmentCanceled)
                    .UsesTargetAsPrimaryEntity()
                    .DisplayName("Last Appointment Canceled");
            }

            WorkflowEventHandlerDefinition.IHandlerConfiguredBase OnLastAppointmentClosed(WorkflowEventHandlerDefinition.INeedEventTarget handler)
            {
                return handler
                    .WithTargetOf<FSServiceOrder>()
                    .OfEntityEvent<FSServiceOrder.Events>(e => e.LastAppointmentClosed)
                    .Is(g => g.OnLastAppointmentClosed)
                    .UsesTargetAsPrimaryEntity()
                    .DisplayName("Last Appointment Closed");
            }

            WorkflowEventHandlerDefinition.IHandlerConfiguredBase OnAppointmentReOpened(WorkflowEventHandlerDefinition.INeedEventTarget handler)
            {
                return handler
                    .WithTargetOf<FSServiceOrder>()
                    .OfEntityEvent<FSServiceOrder.Events>(e => e.AppointmentReOpened)
                    .Is(g => g.OnAppointmentReOpened)
                    .UsesTargetAsPrimaryEntity()
                    .DisplayName("Appointment Reopened");
            }

            WorkflowEventHandlerDefinition.IHandlerConfiguredBase OnAppointmentUnclosed(WorkflowEventHandlerDefinition.INeedEventTarget handler)
            {
                return handler
                    .WithTargetOf<FSServiceOrder>()
                    .OfEntityEvent<FSServiceOrder.Events>(e => e.AppointmentUnclosed)
                    .Is(g => g.OnAppointmentUnclosed)
                    .UsesTargetAsPrimaryEntity()
                    .DisplayName("Appointment Unclosed");
            }
            #endregion

            Workflow.IConfigured QuoteServiceOrderFlow(Workflow.INeedStatesFlow flow)
            {
                return flow
                     .WithFlowStates(flowStates =>
                     {
                         flowStates.Add(State.Initial, fs => fs.IsInitial(g => g.initializeState));
                         flowStates.Add<State.open>(flowState =>
                         {
                             return flowState
                                 .WithActions(actions =>
                                 {
                                     actions.Add(g => g.putOnHold);
                                     actions.Add(g => g.confirmQuote, a => a.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
                                     actions.Add(g => g.cancelOrder);
                                 })
                                 .WithFieldStates(DisableScreenByQuote);
                         });
                         flowStates.Add<State.hold>(flowState =>
                         {
                             return flowState
                                 .WithActions(actions =>
                                 {
                                     actions.Add(g => g.releaseFromHold, a => a.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
                                     actions.Add(g => g.cancelOrder);
                                 });
                         });
                         flowStates.Add<State.canceled>(flowState =>
                         {
                             return flowState
                                 .WithActions(actions =>
                                 {
                                     actions.Add(g => g.reopenOrder, a => a.IsDuplicatedInToolbar());
                                 })
                                 .WithFieldStates(DisableWholeScreen);
                         });
                         flowStates.Add<State.confirmed>(flowState =>
                         {
                             return flowState
                                 .WithActions(actions =>
                                 {
                                     actions.Add(g => g.copyToServiceOrder, a => a.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
                                     actions.Add(g => g.reopenOrder);
                                 })
                                 .WithFieldStates(DisableScreenByQuote);
                         });
                         flowStates.Add<State.copied>(flowState =>
                         {
                             return flowState
                                 .WithActions(actions =>
                                 {
                                     actions.Add(g => g.copyToServiceOrder, a => a.IsDuplicatedInToolbar());
                                 })
                                 .WithEventHandlers(handlers =>
                                 {
                                     handlers.Add(g => g.OnServiceOrderDeleted);
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
                                            fas.Add<openDoc>(f => f.SetFromValue(false));
                                            fas.Add<quote>(f => f.SetFromValue(true));
                                        }));
                            ts.Add(t => t
                                         .To<State.open>()
                                         .IsTriggeredOn(g => g.initializeState)
                                         .When(conditions.IsNotOnHold)
                                         .WithFieldAssignments(fas =>
                                         {
                                             fas.Add<openDoc>(f => f.SetFromValue(true));
                                             fas.Add<quote>(f => f.SetFromValue(true));
                                         }));
                         });
                         transitions.AddGroupFrom<State.open>(ts =>
                         {
                             ts.Add(t => t
                                         .To<State.hold>()
                                         .IsTriggeredOn(g => g.putOnHold));
                             ts.Add(t => t
                                         .To<State.confirmed>()
                                         .IsTriggeredOn(g => g.confirmQuote)
                                         .WithFieldAssignments(fas =>
                                         {
                                             fas.Add<skipExternalTaxCalculation>(f => f.SetFromValue(false));
                                             fas.Add<confirmed>(e => e.SetFromValue(true));
                                             fas.Add<openDoc>(f => f.SetFromValue(false));
                                         }));
                             ts.Add(t => t
                                         .To<State.canceled>()
                                         .IsTriggeredOn(g => g.cancelOrder)
                                         .WithFieldAssignments(fas =>
                                         {
                                             fas.Add<skipExternalTaxCalculation>(f => f.SetFromValue(false));
                                             fas.Add<cancelActionRunning>(f => f.SetFromValue(false));
                                             fas.Add<openDoc>(f => f.SetFromValue(false));
                                             fas.Add<canceled>(f => f.SetFromValue(true));
                                         }));
                         });
                         transitions.AddGroupFrom<State.hold>(ts =>
                         {
                             ts.Add(t => t
                                         .To<State.open>()
                                         .IsTriggeredOn(g => g.releaseFromHold));
                             ts.Add(t => t
                                         .To<State.canceled>()
                                         .IsTriggeredOn(g => g.cancelOrder)
                                         .WithFieldAssignments(fas =>
                                         {
                                             fas.Add<skipExternalTaxCalculation>(f => f.SetFromValue(false));
                                             fas.Add<hold>(f => f.SetFromValue(false));
                                             fas.Add<canceled>(f => f.SetFromValue(true));
                                         }));
                         });
                         transitions.AddGroupFrom<State.canceled>(ts =>
                         {
                             ts.Add(t => t
                                         .To<State.open>()
                                         .IsTriggeredOn(g => g.reopenOrder)
                                         .WithFieldAssignments(fas =>
                                         {
                                             fas.Add<skipExternalTaxCalculation>(f => f.SetFromValue(false));
                                             fas.Add<reopenActionRunning>(f => f.SetFromValue(false));
                                             fas.Add<openDoc>(f => f.SetFromValue(true));
                                             fas.Add<canceled>(e => e.SetFromValue(false));
                                         }));
                         });
                         transitions.AddGroupFrom<State.confirmed>(ts =>
                         {
                             ts.Add(t => t
                                         .To<State.open>()
                                         .IsTriggeredOn(g => g.reopenOrder)
                                         .WithFieldAssignments(fas =>
                                         {
                                             fas.Add<skipExternalTaxCalculation>(f => f.SetFromValue(false));
                                             fas.Add<reopenActionRunning>(f => f.SetFromValue(false));
                                             fas.Add<openDoc>(f => f.SetFromValue(true));
                                             fas.Add<confirmed>(e => e.SetFromValue(false));
                                         }));
                             ts.Add(t => t
                                         .To<State.copied>()
                                         .IsTriggeredOn(g => g.copyToServiceOrder));
                         });
                         transitions.AddGroupFrom<State.copied>(ts =>
                         {
                             ts.Add(t => t
                                         .To<State.confirmed>()
                                         .IsTriggeredOn(g => g.OnServiceOrderDeleted)
                                         .WithFieldAssignments(fas =>
                                         {
                                             fas.Add<copied>(e => e.SetFromValue(false));
                                         }));
                         });
                     });
            }

            Workflow.IConfigured SimpleServiceOrderFlow(Workflow.INeedStatesFlow flow)
            {
                return flow
                     .WithFlowStates(flowStates =>
                     {
                         flowStates.Add(State.Initial, fs => fs.IsInitial(g => g.initializeState));
                         flowStates.Add<State.open>(flowState =>
                         {
                             return flowState
                                 .WithActions(actions =>
                                 {
                                     actions.Add(g => g.putOnHold);
                                     actions.Add(g => g.completeOrder, a => a.IsDuplicatedInToolbar());
                                     actions.Add<ServiceOrderEntry.ServiceOrderQuickProcess>(g => g.quickProcess, a => a.IsDuplicatedInToolbar());
                                     actions.Add(g => g.allowBilling);
                                     actions.Add(g => g.scheduleAppointment, a => a.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
                                     actions.Add(g => g.invoiceOrder, a => a.IsDuplicatedInToolbar());
                                     actions.Add(g => g.cancelOrder);
                                 })
                                 .WithEventHandlers(handlers =>
                                 {
                                     handlers.Add(g => g.OnRequiredServiceContractPeriodCleared);
                                     handlers.Add(g => g.OnLastAppointmentCompleted);
                                     handlers.Add(g => g.OnLastAppointmentCanceled);
                                 });
                         });
                         flowStates.Add<State.hold>(flowState =>
                         {
                             return flowState
                                 .WithActions(actions =>
                                 {
                                     actions.Add(g => g.releaseFromHold, a => a.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
                                     actions.Add(g => g.cancelOrder);
                                 });
                         });
                         flowStates.Add<State.awaiting>(flowState =>
                         {
                             return flowState
                                 .WithActions(actions =>
                                 {
                                     actions.Add(g => g.cancelOrder);
                                     actions.Add(g => g.scheduleAppointment);
                                 })
                                 .WithEventHandlers(handlers =>
                                 {
                                     handlers.Add(g => g.OnServiceContractCleared);
                                     handlers.Add(g => g.OnServiceContractPeriodAssigned);
                                 });
                         });
                         flowStates.Add<State.completed>(flowState =>
                         {
                             return flowState
                                 .WithActions(actions =>
                                 {
                                     actions.Add(g => g.closeOrder, a => a.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
                                     actions.Add<ServiceOrderEntry.ServiceOrderQuickProcess>(g => g.quickProcess, a => a.IsDuplicatedInToolbar());
                                     actions.Add(g => g.allowBilling);
                                     actions.Add(g => g.invoiceOrder);
                                     actions.Add(g => g.reopenOrder);
                                 })
                                 .WithEventHandlers(handlers =>
                                 {
                                     handlers.Add(g => g.OnLastAppointmentClosed);
                                     handlers.Add(g => g.OnAppointmentReOpened);
                                 });
                         });
                         flowStates.Add<State.closed>(flowState =>
                         {
                             return flowState
                                 .WithActions(actions =>
                                 {
                                     actions.Add(g => g.uncloseOrder);
                                     actions.Add<ServiceOrderEntry.ServiceOrderQuickProcess>(g => g.quickProcess, a => a.IsDuplicatedInToolbar());
                                     actions.Add(g => g.invoiceOrder, a => a.IsDuplicatedInToolbar());
                                 })
                                 .WithEventHandlers(handlers =>
                                 {
                                     handlers.Add(g => g.OnAppointmentUnclosed);
                                 })
                                 .WithFieldStates(DisableWholeScreen);
                         });
                         flowStates.Add<State.canceled>(flowState =>
                         {
                             return flowState
                                 .WithActions(actions =>
                                 {
                                     actions.Add(g => g.reopenOrder, a => a.IsDuplicatedInToolbar());
                                 })
                                 .WithEventHandlers(handlers =>
                                 {
                                     handlers.Add(g => g.OnAppointmentReOpened);
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
                                            fas.Add<openDoc>(f => f.SetFromValue(false));
                                        }));
                             ts.Add(t => t
                                        .To<State.open>()
                                        .IsTriggeredOn(g => g.initializeState)
                                        .When(conditions.IsNotOnHold)
                                        .WithFieldAssignments(fas =>
                                        {
                                            fas.Add<openDoc>(f => f.SetFromValue(true));
                                        }));
                         });
                         transitions.AddGroupFrom<State.open>(ts =>
                         {
                             ts.Add(t => t
                                         .To<State.hold>()
                                         .IsTriggeredOn(g => g.putOnHold));
                             ts.Add(t => t
                                         .To<State.completed>()
                                         .IsTriggeredOn(g => g.completeOrder)
                                         .WithFieldAssignments(fas =>
                                         {
                                             fas.Add<skipExternalTaxCalculation>(f => f.SetFromValue(false));
                                             fas.Add<completeActionRunning>(f => f.SetFromValue(false));
                                             fas.Add<processCompleteAction>(f => f.SetFromValue(false));
                                             fas.Add<openDoc>(f => f.SetFromValue(false));
                                             fas.Add<completed>(f => f.SetFromValue(true));
                                         }));
                             ts.Add(t => t
                                         .To<State.completed>()
                                         .IsTriggeredOn(g => g.OnLastAppointmentCompleted)
                                         .WithFieldAssignments(fas =>
                                         {
                                             fas.Add<skipExternalTaxCalculation>(f => f.SetFromValue(false));
                                             fas.Add<processCompleteAction>(f => f.SetFromValue(true));
                                             fas.Add<openDoc>(f => f.SetFromValue(false));
                                             fas.Add<completed>(f => f.SetFromValue(true));
                                         }));
                             ts.Add(t => t
                                         .To<State.canceled>()
                                         .IsTriggeredOn(g => g.cancelOrder)
                                         .WithFieldAssignments(fas =>
                                         {
                                             fas.Add<skipExternalTaxCalculation>(f => f.SetFromValue(false));
                                             fas.Add<cancelActionRunning>(f => f.SetFromValue(false));
                                             fas.Add<openDoc>(f => f.SetFromValue(false));
                                             fas.Add<canceled>(f => f.SetFromValue(true));
                                         }));
                             ts.Add(t => t
                                         .To<State.canceled>()
                                         .IsTriggeredOn(g => g.OnLastAppointmentCanceled)
                                         .WithFieldAssignments(fas =>
                                         {
                                             fas.Add<processCancelAction>(f => f.SetFromValue(true));
                                             fas.Add<openDoc>(f => f.SetFromValue(false));
                                             fas.Add<canceled>(f => f.SetFromValue(true));
                                         }));
                             ts.Add(t => t
                                         .To<State.awaiting>()
                                         .IsTriggeredOn(g => g.OnRequiredServiceContractPeriodCleared)
                                         .WithFieldAssignments(fas =>
                                         {
                                             fas.Add<awaiting>(f => f.SetFromValue(true));
                                         }));
                         });
                         transitions.AddGroupFrom<State.hold>(ts =>
                         {
                             ts.Add(t => t
                                         .To<State.open>()
                                         .IsTriggeredOn(g => g.releaseFromHold));
                             ts.Add(t => t
                                         .To<State.canceled>()
                                         .IsTriggeredOn(g => g.cancelOrder)
                                         .WithFieldAssignments(fas =>
                                         {
                                             fas.Add<skipExternalTaxCalculation>(f => f.SetFromValue(false));
                                             fas.Add<hold>(f => f.SetFromValue(false));
                                             fas.Add<canceled>(f => f.SetFromValue(true));
                                         }));
                         });
                         transitions.AddGroupFrom<State.awaiting>(ts =>
                         {
                             ts.Add(t => t
                                         .To<State.canceled>()
                                         .IsTriggeredOn(g => g.cancelOrder)
                                         .WithFieldAssignments(fas =>
                                         {
                                             fas.Add<skipExternalTaxCalculation>(f => f.SetFromValue(false));
                                             fas.Add<awaiting>(f => f.SetFromValue(false));
                                             fas.Add<canceled>(f => f.SetFromValue(true));
                                         }));
                             ts.Add(t => t
                                         .To<State.open>()
                                         .IsTriggeredOn(g => g.OnServiceContractCleared)
                                         .WithFieldAssignments(fas =>
                                         {
                                             fas.Add<openDoc>(f => f.SetFromValue(true));
                                             fas.Add<awaiting>(f => f.SetFromValue(false));
                                         }));
                             ts.Add(t => t
                                         .To<State.open>()
                                         .IsTriggeredOn(g => g.OnServiceContractPeriodAssigned)
                                         .WithFieldAssignments(fas =>
                                         {
                                             fas.Add<openDoc>(f => f.SetFromValue(true));
                                             fas.Add<awaiting>(f => f.SetFromValue(false));
                                         }));
                         });
                         transitions.AddGroupFrom<State.completed>(ts =>
                         {
                             ts.Add(t => t
                                         .To<State.open>()
                                         .IsTriggeredOn(g => g.reopenOrder)
                                         .WithFieldAssignments(fas =>
                                         {
                                             fas.Add<openDoc>(f => f.SetFromValue(true));
                                             fas.Add<completed>(e => e.SetFromValue(false));
                                             fas.Add<skipExternalTaxCalculation>(f => f.SetFromValue(false));
                                             fas.Add<reopenActionRunning>(f => f.SetFromValue(false));
                                         }));
                             ts.Add(t => t
                                         .To<State.open>()
                                         .IsTriggeredOn(g => g.OnAppointmentReOpened)
                                         .WithFieldAssignments(fas =>
                                         {
                                             fas.Add<openDoc>(f => f.SetFromValue(true));
                                             fas.Add<completed>(e => e.SetFromValue(false));
                                         }));
                             ts.Add(t => t
                                         .To<State.closed>()
                                         .IsTriggeredOn(g => g.closeOrder)
                                         .When(conditions.UserConfirmedClosing)
                                         .WithFieldAssignments(fas =>
                                         {
                                             fas.Add<skipExternalTaxCalculation>(f => f.SetFromValue(false));
                                             fas.Add<closeActionRunning>(f => f.SetFromValue(false));
                                             fas.Add<closed>(f => f.SetFromValue(true));
                                         }));
                             ts.Add(t => t
                                         .To<State.closed>()
                                         .IsTriggeredOn(g => g.OnLastAppointmentClosed)
                                         .WithFieldAssignments(fas =>
                                         {
                                             fas.Add<processCloseAction>(f => f.SetFromValue(true));
                                             fas.Add<closed>(f => f.SetFromValue(true));
                                         }));
                         });
                         transitions.AddGroupFrom<State.closed>(ts =>
                         {
                             ts.Add(t => t
                                         .To<State.completed>()
                                         .IsTriggeredOn(g => g.uncloseOrder)
                                         .When(conditions.UserConfirmedUnclosing)
                                         .WithFieldAssignments(fas =>
                                         {
                                             fas.Add<skipExternalTaxCalculation>(f => f.SetFromValue(false));
                                             fas.Add<unCloseActionRunning>(f => f.SetFromValue(false));
                                             fas.Add<closed>(f => f.SetFromValue(false));
                                         }));
                             ts.Add(t => t
                                         .To<State.completed>()
                                         .IsTriggeredOn(g => g.OnAppointmentUnclosed)
                                         .WithFieldAssignments(fas =>
                                         {
                                             fas.Add<closed>(f => f.SetFromValue(false));
                                         }));
                         });
                         transitions.AddGroupFrom<State.canceled>(ts =>
                         {
                             ts.Add(t => t
                                         .To<State.open>()
                                         .IsTriggeredOn(g => g.reopenOrder)
                                         .WithFieldAssignments(fas =>
                                         {
                                             fas.Add<openDoc>(f => f.SetFromValue(true));
                                             fas.Add<canceled>(e => e.SetFromValue(false));
                                             fas.Add<skipExternalTaxCalculation>(f => f.SetFromValue(false));
                                             fas.Add<reopenActionRunning>(f => f.SetFromValue(false));
                                         }));
                             ts.Add(t => t
                                         .To<State.open>()
                                         .IsTriggeredOn(g => g.OnAppointmentReOpened)
                                         .WithFieldAssignments(fas =>
                                         {
                                             fas.Add<openDoc>(f => f.SetFromValue(true));
                                             fas.Add<canceled>(e => e.SetFromValue(false));
                                         }));
                         });
                     });
            }

            #region Categories
            var commonCategories = CommonActionCategories.Get(context);
            var processingCategory = commonCategories.Processing;
            var correctionCategory = context.Categories.CreateNew(FSToolbarCategory.CorrectionCategoryID,
                    category => category.DisplayName(FSToolbarCategory.CategoryNames.Corrections));
            var schedulingCategory = context.Categories.CreateNew(FSToolbarCategory.SchedulingCategoryID,
                    category => category.DisplayName(FSToolbarCategory.CategoryNames.Scheduling));
            var printingEmailingCategory = commonCategories.PrintingAndEmailing;
            var replenishmentCategory = context.Categories.CreateNew(FSToolbarCategory.ReplenishmentCategoryID,
                    category => category.DisplayName(FSToolbarCategory.CategoryNames.Replenishment));
            var otherCategory = commonCategories.Other;
            #endregion

            context.AddScreenConfigurationFor(screen =>
            {
                return screen
                    .StateIdentifierIs<status>()
                    .FlowTypeIdentifierIs<workflowTypeID>()
                    .WithFlows(flows =>
                    {
                        flows.AddDefault(SimpleServiceOrderFlow);
                        flows.Add<ListField.ServiceOrderWorkflowTypes.quote>(QuoteServiceOrderFlow);
                        flows.Add<ListField.ServiceOrderWorkflowTypes.simple>(SimpleServiceOrderFlow);
                    })
                    .WithActions(actions =>
                    {
                        actions.Add(g => g.initializeState,
                                    c => c.IsHiddenAlways());

                        actions.Add(g => g.releaseFromHold,
                                    c => c.WithCategory(processingCategory)
                                          .WithPersistOptions(ActionPersistOptions.NoPersist)
                                          .WithFieldAssignments(fas =>
                                          {
                                              fas.Add<hold>(f => f.SetFromValue(false));
                                              fas.Add<openDoc>(f => f.SetFromValue(true));
                                          }));

						actions.Add(g => g.putOnHold,
                                    c => c.WithCategory(processingCategory)
                                          .WithPersistOptions(ActionPersistOptions.NoPersist)
                                          .WithFieldAssignments(fas =>
                                          {
                                              fas.Add<hold>(f => f.SetFromValue(true));
                                              fas.Add<openDoc>(f => f.SetFromValue(false));
                                          }));

						actions.Add(g => g.completeOrder,
                                    c => c.WithCategory(processingCategory)
                                          .IsDisabledWhen(conditions.IsQuote)
                                          .WithFieldAssignments(fas =>
                                          {
                                              fas.Add<skipExternalTaxCalculation>(f => f.SetFromValue(true));
                                              fas.Add<completeActionRunning>(f => f.SetFromValue(true));
                                          }));

                        actions.Add(g => g.confirmQuote,
                                    c => c.WithCategory(processingCategory)
                                            .WithFieldAssignments(fas =>
                                            {
                                                fas.Add<skipExternalTaxCalculation>(f => f.SetFromValue(true));
                                            }));

                        actions.Add(g => g.copyToServiceOrder,
                                    c => c.WithCategory(processingCategory));

                        actions.Add(g => g.closeOrder,
                                    c => c.WithCategory(processingCategory)
                                          .WithFieldAssignments(fas =>
                                          {
                                              fas.Add<skipExternalTaxCalculation>(f => f.SetFromValue(true));
                                              fas.Add<closeActionRunning>(f => f.SetFromValue(true));
                                          })
                                          .IsDisabledWhen(conditions.IsQuote));

                        actions.Add<ServiceOrderEntry.ServiceOrderQuickProcess>(g => g.quickProcess,
                                    c => c.WithCategory(processingCategory)
                                            .IsDisabledWhen(conditions.IsBilledByAppointment || !conditions.IsQuickProcessEnabled
                                                            || conditions.IsOnHold || conditions.IsCanceled || conditions.IsBilledByContract
                                                            || conditions.IsBilled)
                                            .IsHiddenWhen(conditions.IsBilledByAppointment || !conditions.IsQuickProcessEnabled
                                                            || conditions.IsBilledByContract || !conditions.HasNoLinesToCreatePurchaseOrder));

                        actions.Add(g => g.allowBilling,
                                    c => c.WithCategory(processingCategory)
                                            .IsDisabledWhen(conditions.IsOnHold
                                                            || conditions.IsCanceled || conditions.IsAllowedForInvoice
                                                            || conditions.IsBilled || conditions.IsBilledByAppointment
                                                            || conditions.IsInternalBehavior)
                                            .IsHiddenWhen(conditions.IsBilledByAppointment || conditions.IsAllowedForInvoice || conditions.IsQuote));

                        actions.Add(g => g.invoiceOrder,
                                    c => c.WithCategory(processingCategory)
                                            .IsDisabledWhen(conditions.IsOnHold
                                                            || !conditions.IsAllowedForInvoice || conditions.IsBilled
                                                            || conditions.IsBilledByContract || conditions.IsInternalBehavior)
                                            .IsHiddenWhen(conditions.IsBilledByAppointment || conditions.IsInternalBehavior || conditions.IsQuote));

                        actions.Add(g => g.cancelOrder,
                                    c => c.WithCategory(processingCategory)
                                            .WithFieldAssignments(fas =>
                                            {
                                                fas.Add<skipExternalTaxCalculation>(f => f.SetFromValue(true));
                                                fas.Add<cancelActionRunning>(f => f.SetFromValue(true));
                                            }));

                        actions.Add(g => g.scheduleAppointment,
                                    c => c.WithCategory(schedulingCategory)
                                            .IsDisabledWhen(conditions.IsFinalState
                                                            || conditions.IsCompleted || conditions.CustomerIsProspect)
                                            .IsHiddenWhen(conditions.IsQuote));

                        actions.Add(g => g.openEmployeeBoard,
                                    c => c.WithCategory(schedulingCategory)
                                            .IsDisabledWhen(conditions.IsFinalState
                                                            || conditions.IsCompleted || conditions.CustomerIsProspect
                                                            || conditions.IsInserted)
                                            .IsHiddenWhen(conditions.IsQuote));

                        actions.Add(g => g.openUserCalendar,
                                    c => c.WithCategory(schedulingCategory)
                                            .IsDisabledWhen(conditions.IsFinalState
                                                            || conditions.IsCompleted || conditions.CustomerIsProspect
                                                            || conditions.IsInserted)
                                            .IsHiddenWhen(conditions.IsQuote));

                        actions.Add(g => g.OpenRoomBoard,
                                    c => c.WithCategory(schedulingCategory)
                                          .IsDisabledWhen(conditions.IsFinalState
                                                            || conditions.IsCompleted || conditions.CustomerIsProspect
                                                            || conditions.IsInserted || conditions.IsAwaiting)
                                          .IsHiddenWhen(!conditions.RoomFeatureEnabled || conditions.IsQuote));

                        actions.Add(g => g.reopenOrder,
                                    c => c.WithCategory(correctionCategory)
                                            .WithFieldAssignments(fas =>
                                            {
                                                fas.Add<skipExternalTaxCalculation>(f => f.SetFromValue(true));
                                                fas.Add<reopenActionRunning>(f => f.SetFromValue(true));
                                            })
                                            .IsDisabledWhen(conditions.IsClosed || conditions.IsBilled));

                        actions.Add(g => g.uncloseOrder,
                                    c => c.WithCategory(correctionCategory)
                                            .WithFieldAssignments(fas =>
                                            {
                                                fas.Add<skipExternalTaxCalculation>(f => f.SetFromValue(true));
                                                fas.Add<unCloseActionRunning>(f => f.SetFromValue(true));
                                            })
                                            .IsDisabledWhen(conditions.IsQuote));

                        actions.Add(g => g.billReversal,
                                    c => c.WithCategory(correctionCategory)
                                            .IsDisabledWhen(conditions.IsInserted || conditions.IsBilledByAppointment
                                                            || !conditions.PostToProjects)
                                            .IsHiddenWhen(conditions.IsBilledByAppointment || conditions.IsInternalBehavior || conditions.IsQuote));

						actions.Add(g => g.printServiceOrder,
                                    c => c.WithCategory(printingEmailingCategory));

                        actions.Add(g => g.printServiceTimeActivityReport,
                                    c => c.WithCategory(printingEmailingCategory)
                                          .IsHiddenWhen(conditions.IsQuote));

                        actions.Add(g => g.serviceOrderAppointmentsReport,
                                    c => c.WithCategory(printingEmailingCategory)
                                          .IsHiddenWhen(conditions.IsQuote));

                        actions.Add(g => g.createPurchaseOrder,
                                    c => c.WithCategory(replenishmentCategory)
                                            .IsDisabledWhen(conditions.HasNoLinesToCreatePurchaseOrder
                                                            || conditions.CustomerIsProspect || conditions.IsFinalState
                                                            || conditions.IsCompleted || conditions.IsBilled || conditions.IsAwaiting)
                                            .IsHiddenWhen(conditions.IsQuote));

						actions.Add<ServiceOrderEntryExternalTax>(
                                    g => g.recalcExternalTax,
                                    c => c.WithCategory(otherCategory)
                                            .IsDisabledWhen(conditions.IsCanceled || conditions.IsCopied
                                                            || conditions.IsClosed));

                        actions.Add(g => g.createPrepayment,
                                    c => c.IsDisabledWhen(conditions.IsQuote || conditions.IsFinalState
                                                            || conditions.IsInternalBehavior || conditions.IsBilledByContract
                                                            || !conditions.PostToSOSIPM || conditions.IsAllowedForInvoice
                                                            || conditions.IsCompleted || conditions.IsInserted || conditions.IsAwaiting));

                        actions.Add(g => g.addInvBySite,
                                    c => c.IsDisabledWhen(conditions.IsCanceled || conditions.IsBilled || conditions.IsClosed
                                                            || conditions.IsInternalBehavior || !conditions.CustomerIsSpecified
                                                            || (conditions.IsQuote && conditions.IsCopied) || conditions.IsAllowedForInvoice));

                        actions.Add(g => g.addInvSelBySite,
                                    c => c.IsDisabledWhen(conditions.IsFinalState));

                        actions.Add(g => g.addReceipt,
									c => c.IsDisabledWhen(conditions.IsCanceled || conditions.IsInternalBehavior 
                                                            || conditions.IsQuote || conditions.IsInserted)
                                          .IsHiddenWhen(!conditions.IsExpenseFeatureEnabled));

                        actions.Add(g => g.addBill,
                                    c => c.IsDisabledWhen(conditions.IsCanceled || conditions.IsInternalBehavior
                                                            || conditions.IsQuote || conditions.IsInserted));

                        actions.Add(g => g.openStaffSelectorFromServiceTab,
                                    c => c.IsDisabledWhen(conditions.IsCanceled || conditions.IsBilled || conditions.IsClosed
                                                            || (conditions.IsQuote && conditions.IsCopied)));

                        actions.Add(g => g.openStaffSelectorFromStaffTab,
                                    c => c.IsDisabledWhen(conditions.IsCanceled || conditions.IsBilled || conditions.IsClosed));

                        actions.Add(g => g.OpenScheduleScreen);
                        actions.Add(g => g.openSource);
                        actions.Add(g => g.openServiceOrderScreen);
                        actions.Add(g => g.viewDirectionOnMap);
                        actions.Add(g => g.createNewCustomer);
                        actions.Add(g => g.OpenPostingDocument);
                        actions.Add(g => g.viewPayment);
                        actions.Add(g => g.validateAddress);
                    })
                    .WithCategories(categories =>
                    {
                        categories.Add(processingCategory);
                        categories.Add(schedulingCategory);
                        categories.Add(correctionCategory);
                        categories.Add(printingEmailingCategory);
                        categories.Add(replenishmentCategory);
                        categories.Add(otherCategory);
                    })
                    .WithHandlers(handlers =>
                    {
                        handlers.Add(OnServiceOrderDeleted);
                        handlers.Add(OnServiceContractCleared);
                        handlers.Add(OnServiceContractPeriodAssigned);
                        handlers.Add(OnRequiredServiceContractPeriodCleared);
                        handlers.Add(OnLastAppointmentCompleted);
                        handlers.Add(OnLastAppointmentCanceled);
                        handlers.Add(OnLastAppointmentClosed);
                        handlers.Add(OnAppointmentReOpened);
                        handlers.Add(OnAppointmentUnclosed);
                    });
            });
        }
    }
}
