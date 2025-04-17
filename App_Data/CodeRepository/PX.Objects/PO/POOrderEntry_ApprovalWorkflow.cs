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
using PX.Objects.CM.Extensions;
using PX.Common;
using PX.Objects.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PO
{
    using static BoundedTo<POOrderEntry, POOrder>;
    using static POOrder;
    using State = POOrderStatus;
    using This = POOrderEntry_ApprovalWorkflow;
    using DropShipLinksExt = GraphExtensions.POOrderEntryExt.DropShipLinksExt;
    using PurchaseToSOLinksExt = GraphExtensions.POOrderEntryExt.PurchaseToSOLinksExt;

    public class POOrderEntry_ApprovalWorkflow : PXGraphExtension<POOrderEntry_Workflow, POOrderEntry>
    {
        private class POApprovalSettings : IPrefetchable
        {
            private static POApprovalSettings Current => PXDatabase.GetSlot<POApprovalSettings>(nameof(POApprovalSettings), typeof(POSetup), typeof(POSetupApproval));

            public static bool IsActive => Current.OrderRequestApproval;
            public static bool IsActiveForBlanket => Current.BlanketOrderRequestApproval;
            public static bool IsActiveForDropShip => Current.DropShipOrderRequestApproval;
            public static bool IsActiveForNormal => Current.NormalOrderRequestApproval;
            public static bool IsActiveForStandart => Current.StandartOrderRequestApproval;
			public static bool IsActiveForProjectDropShip => Current.ProjectDropShipOrderRequestApproval;

			private bool OrderRequestApproval;
            private bool BlanketOrderRequestApproval;
            private bool DropShipOrderRequestApproval;
            private bool NormalOrderRequestApproval;
            private bool StandartOrderRequestApproval;
			private bool ProjectDropShipOrderRequestApproval;

			void IPrefetchable.Prefetch()
            {
                using (PXDataRecord poSetup = PXDatabase.SelectSingle<POSetup>(new PXDataField<POSetup.orderRequestApproval>()))
                {
                    if (poSetup != null)
                        OrderRequestApproval = (bool)poSetup.GetBoolean(0);
                }
                if (OrderRequestApproval)
                {
                    BlanketOrderRequestApproval = RequestApproval(POOrderType.Blanket);
                    DropShipOrderRequestApproval = RequestApproval(POOrderType.DropShip);
                    NormalOrderRequestApproval = RequestApproval(POOrderType.RegularOrder);
                    StandartOrderRequestApproval = RequestApproval(POOrderType.StandardBlanket);
					ProjectDropShipOrderRequestApproval = RequestApproval(POOrderType.ProjectDropShip);
				}
            }

            private static bool RequestApproval(string orderType)
            {
                using (PXDataRecord poApproval = PXDatabase.SelectSingle<POSetupApproval>(
                    new PXDataField<POSetupApproval.approvalID>(),
                    new PXDataFieldValue<POSetupApproval.orderType>(orderType)))
                {
                    if (poApproval != null)
                        return (int?)poApproval.GetInt32(0) != null;
                }
                return false;
            }
        }

        public const string ApproveActionName = "Approve";
        public const string RejectActionName = "Reject";

        private static bool ApprovalIsActive => PXAccess.FeatureInstalled<CS.FeaturesSet.approvalWorkflow>() && POApprovalSettings.IsActive;

        protected static bool IsAnyApprovalActive() => IsBlanketApprovalActive() || IsDropShipApprovalActive() || IsNormalApprovalActive() || IsStandartApprovalActive() || IsProjectDropShipApprovalActive();
        protected static bool IsBlanketApprovalActive() => ApprovalIsActive && POApprovalSettings.IsActiveForBlanket;
        protected static bool IsDropShipApprovalActive() => ApprovalIsActive && POApprovalSettings.IsActiveForDropShip;
        protected static bool IsNormalApprovalActive() => ApprovalIsActive && POApprovalSettings.IsActiveForNormal;
        protected static bool IsStandartApprovalActive() => ApprovalIsActive && POApprovalSettings.IsActiveForStandart;
        protected static bool IsProjectDropShipApprovalActive() => ApprovalIsActive && POApprovalSettings.IsActiveForProjectDropShip;

        [PXWorkflowDependsOnType(typeof(POSetup), typeof(POSetupApproval))]
        public sealed override void Configure(PXScreenConfiguration config)
        {
            if (IsAnyApprovalActive())
                Configure(config.GetScreenConfigurationContext<POOrderEntry, POOrder>());
            else
                HideApproveAndRejectActions(config.GetScreenConfigurationContext<POOrderEntry, POOrder>());
        }

        protected static void HideApproveAndRejectActions(WorkflowContext<POOrderEntry, POOrder> context)
        {
            var approve = context.ActionDefinitions
                .CreateNew(ApproveActionName, a => a
                .WithCategory(PredefinedCategory.Actions)
                .PlaceAfter(g => g.putOnHold)
                .IsHiddenAlways());
            var reject = context.ActionDefinitions
                .CreateNew(RejectActionName, a => a
                .WithCategory(PredefinedCategory.Actions)
                .PlaceAfter(approve)
                .IsHiddenAlways());
            var reassign = context.ActionDefinitions
                .CreateExisting(nameof(POOrderEntry.Approval.ReassignApproval), a => a
                .WithCategory(PredefinedCategory.Actions)
                .PlaceAfter(reject)
                .IsHiddenAlways());

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

        protected static void Configure(WorkflowContext<POOrderEntry, POOrder> context)
        {
            #region Conditions
            Condition Bql<T>() where T : IBqlUnary, new() => context.Conditions.FromBql<T>();
            Condition Existing(string name) => (Condition)context.Conditions.Get(name);
            var conditions = new
            {
                IsApproved
                    = Bql<approved.IsEqual<True>>(),
                IsRejected
                    = Bql<rejected.IsEqual<True>>(),

                IsOnHold
                    = Existing("IsOnHold"),
                IsPrinted
                    = Existing("IsPrinted"),
                IsEmailed
                    = Existing("IsEmailed"),
                HasAllLinesClosed
                    = Existing("HasAllLinesClosed"),
                HasAllLinesCompleted
                    = Existing("HasAllLinesCompleted"),
                HasAllDropShipLinesLinked
                    = Existing("HasAllDropShipLinesLinked")
            }.AutoNameConditions();
            #endregion

            #region Categories
            var approvalCategory = context.Categories.CreateNew(CommonActionCategories.ApprovalCategoryID,
                    category => category.DisplayName(CommonActionCategories.DisplayNames.Approval)
                    .PlaceAfter(CommonActionCategories.IntercompanyCategoryID));
            #endregion

            #region Actions
            var approve = context.ActionDefinitions
                .CreateNew(ApproveActionName, a => a
                .WithCategory(approvalCategory)
                .PlaceAfter(g => g.putOnHold)
				.MapEnableToSelect()
                .WithFieldAssignments(fas =>
                {
                    fas.Add<approved>(true);
                }));
            var reject = context.ActionDefinitions
	            .CreateNew(RejectActionName, a => a
                .WithCategory(approvalCategory)
                .PlaceAfter(approve)
				.MapEnableToSelect()
                .WithFieldAssignments(fas =>
                {
                    fas.Add<rejected>(true);
                }));
            var reassign = context.ActionDefinitions
                .CreateExisting(nameof(POOrderEntry.Approval.ReassignApproval), a => a
                .WithCategory(approvalCategory)
                .PlaceAfter(reject)
                .MapEnableToSelect());
            #endregion

            context.UpdateScreenConfigurationFor(screen =>
            {
                return screen
                    .WithFlows(flows =>
                    {
                        if (IsBlanketApprovalActive())
                            flows.Update<POOrderType.blanket>(flow =>
                            {
                                return flow
                                    .WithFlowStates(states =>
                                    {
                                        states.Add<State.pendingApproval>(state =>
                                        {
                                            return state
                                                .WithActions(actions =>
                                                {
                                                    actions.Add(g => g.putOnHold);
                                                    actions.Add(approve, c => c.IsDuplicatedInToolbar());
                                                    actions.Add(reject, c => c.IsDuplicatedInToolbar());
                                                    actions.Add(reassign);
                                                    actions.Add(g => g.validateAddresses);

                                                    actions.Add(g => g.vendorDetails);
                                                    actions.Add(g => g.printPurchaseOrder);
                                                })
                                                .WithFieldStates(fields =>
                                                {
                                                    fields.AddAllFields<POOrder>(c => c.IsDisabled());
                                                    fields.AddField<POOrder.orderType>();
                                                    fields.AddField<POOrder.orderNbr>();
                                                    fields.AddField<POOrder.controlTotal>();
                                                    fields.AddField<POOrder.workgroupID>();
                                                    fields.AddField<POOrder.ownerID>();
                                                    fields.AddField<POOrder.printed>(c => c.IsHidden());
                                                    fields.AddField<POOrder.emailed>(c => c.IsHidden());
                                                    fields.AddField<POOrder.dontPrint>(c => c.IsHidden());
                                                    fields.AddField<POOrder.dontEmail>(c => c.IsHidden());

                                                    fields.AddTable<POLine>(c => c.IsDisabled());
                                                    fields.AddField<POLine.cancelled>();
                                                    fields.AddField<POLine.completed>();
                                                    fields.AddField<POLine.promisedDate>();
                                                    fields.AddField<POLine.closed>();
													fields.AddFields<POOrderEntry_Workflow.DropShipOrderLineFields>(c => c.IsHidden());

                                                    fields.AddTable<POTax>(c => c.IsDisabled());
                                                    fields.AddAllFields<POShipAddress>(c => c.IsDisabled());
                                                    fields.AddAllFields<POShipContact>(c => c.IsDisabled());
                                                    fields.AddTable<CurrencyInfo>(c => c.IsDisabled());
                                                });
                                        });
                                        states.Add<State.rejected>(state =>
                                        {
                                            return state
                                                .WithActions(actions =>
                                                {
                                                    actions.Add(g => g.putOnHold, c => c.IsDuplicatedInToolbar());

                                                    actions.Add(g => g.vendorDetails);
                                                    actions.Add(g => g.printPurchaseOrder);
                                                    actions.Add(g => g.validateAddresses);
                                                })
                                                .WithFieldStates(fields =>
                                                {
                                                    fields.AddTable<POOrder>(c => c.IsDisabled());
                                                    fields.AddField<POOrder.orderType>();
                                                    fields.AddField<POOrder.orderNbr>();
                                                    fields.AddField<POOrder.printed>(c => c.IsHidden());
                                                    fields.AddField<POOrder.emailed>(c => c.IsHidden());
                                                    fields.AddField<POOrder.dontPrint>(c => c.IsHidden());
                                                    fields.AddField<POOrder.dontEmail>(c => c.IsHidden());

                                                    //fields.AddTable<POLine>();
                                                    fields.AddField<POLine.cancelled>();
                                                    fields.AddField<POLine.completed>();
                                                    fields.AddField<POLine.promisedDate>();
                                                    fields.AddField<POLine.closed>();
													fields.AddFields<POOrderEntry_Workflow.DropShipOrderLineFields>(c => c.IsHidden());

                                                    fields.AddAllFields<PORemitAddress>(c => c.IsDisabled());
                                                    fields.AddAllFields<PORemitContact>(c => c.IsDisabled());
                                                    fields.AddAllFields<POShipAddress>(c => c.IsDisabled());
                                                    fields.AddAllFields<POShipContact>(c => c.IsDisabled());
                                                    fields.AddTable<POTax>(c => c.IsDisabled());

                                                    fields.AddTable<CurrencyInfo>(c => c.IsDisabled());
                                                });
                                        });
                                    })
                                    .WithTransitions(transitions =>
                                    {
                                        transitions.UpdateGroupFrom(State.Initial, ts =>
                                        {
                                            ts.Add(t => t
                                                .To<State.rejected>()
                                                .IsTriggeredOn(g => g.initializeState)
                                                .When(conditions.IsRejected)
                                                .PlaceBefore(tr => tr.To<State.closed>()));
                                            ts.Add(t => t
                                                .To<State.pendingApproval>()
                                                .IsTriggeredOn(g => g.initializeState)
                                                .When(!conditions.IsApproved)
                                                .PlaceAfter(tr => tr.To<State.rejected>()));
                                        });

                                        transitions.UpdateGroupFrom<State.hold>(ts =>
                                        {
                                            ts.Add(t => t
                                                .To<State.pendingApproval>()
                                                .IsTriggeredOn(g => g.releaseFromHold)
                                                .When(!conditions.IsOnHold && !conditions.IsApproved)
                                                .PlaceBefore(tr => tr.To<State.closed>()));
                                        });

                                        transitions.AddGroupFrom<State.pendingApproval>(ts =>
                                        {
                                            ts.Add(t => t
                                                .To<State.closed>()
                                                .IsTriggeredOn(approve)
                                                .When(conditions.IsApproved && conditions.HasAllLinesClosed));
                                            ts.Add(t => t
                                                .To<State.completed>()
                                                .IsTriggeredOn(approve)
                                                .When(conditions.IsApproved && conditions.HasAllLinesCompleted));
                                            ts.Add(t => t
                                                .To<State.open>()
                                                .IsTriggeredOn(approve)
                                                .When(conditions.IsApproved));

                                            ts.Add(t => t
                                                .To<State.rejected>()
                                                .IsTriggeredOn(reject)
                                                .When(conditions.IsRejected));

                                            ts.Add(t => t
                                                .To<State.hold>()
                                                .IsTriggeredOn(g => g.putOnHold)
                                                .When(conditions.IsOnHold));
                                        });

                                        transitions.AddGroupFrom<State.rejected>(ts =>
                                        {
                                            ts.Add(t => t
                                                .To<State.hold>()
                                                .IsTriggeredOn(g => g.putOnHold)
                                                .When(conditions.IsOnHold));
                                        });
                                    });
                            });

                        if (IsStandartApprovalActive())
                            flows.Update<POOrderType.standardBlanket>(flow =>
                            {
                                return flow
                                    .WithFlowStates(states =>
                                    {
                                        states.Add<State.pendingApproval>(state =>
                                        {
                                            return state
                                                .WithActions(actions =>
                                                {
                                                    actions.Add(g => g.putOnHold);
                                                    actions.Add(approve, c => c.IsDuplicatedInToolbar());
                                                    actions.Add(reject, c => c.IsDuplicatedInToolbar());
                                                    actions.Add(reassign);
                                                    actions.Add(g => g.validateAddresses);

                                                    actions.Add(g => g.vendorDetails);
                                                    actions.Add(g => g.printPurchaseOrder);

                                                    actions.Add<PurchaseToSOLinksExt>(g => g.viewDemand);
                                                })
                                                .WithFieldStates(fields =>
                                                {
                                                    fields.AddAllFields<POOrder>(c => c.IsDisabled());
                                                    fields.AddField<POOrder.orderType>();
                                                    fields.AddField<POOrder.orderNbr>();
                                                    fields.AddField<POOrder.controlTotal>();
                                                    fields.AddField<POOrder.workgroupID>();
                                                    fields.AddField<POOrder.ownerID>();
                                                    fields.AddField<POOrder.printed>(c => c.IsHidden());
                                                    fields.AddField<POOrder.emailed>(c => c.IsHidden());
                                                    fields.AddField<POOrder.dontPrint>(c => c.IsHidden());
                                                    fields.AddField<POOrder.dontEmail>(c => c.IsHidden());

                                                    fields.AddTable<POLine>(c => c.IsDisabled());
                                                    fields.AddField<POLine.completed>(x => x.IsDisabled().IsHidden());
                                                    fields.AddField<POLine.closed>(x => x.IsDisabled().IsHidden());
                                                    fields.AddField<POLine.promisedDate>();
													fields.AddFields<POOrderEntry_Workflow.DropShipOrderLineFields>(c => c.IsHidden());
													fields.AddFields<POOrderEntry_Workflow.BlanketOrderLineFields>(c => c.IsHidden());

                                                    fields.AddTable<POTax>(c => c.IsDisabled());
                                                    fields.AddAllFields<POShipAddress>(c => c.IsDisabled());
                                                    fields.AddAllFields<POShipContact>(c => c.IsDisabled());
                                                    fields.AddTable<CurrencyInfo>(c => c.IsDisabled());
                                                    fields.AddTable<POOrderPrepayment>(c => c.IsHidden());
                                                });
                                        });
                                        states.Add<State.rejected>(state =>
                                        {
                                            return state
                                                .WithActions(actions =>
                                                {
                                                    actions.Add(g => g.putOnHold, c => c.IsDuplicatedInToolbar());

                                                    actions.Add(g => g.vendorDetails);
                                                    actions.Add(g => g.printPurchaseOrder);
                                                    actions.Add(g => g.validateAddresses);

                                                    actions.Add<PurchaseToSOLinksExt>(g => g.viewDemand);
                                                })
                                                .WithFieldStates(fields =>
                                                {
                                                    fields.AddTable<POOrder>(c => c.IsDisabled());
                                                    fields.AddField<POOrder.orderType>();
                                                    fields.AddField<POOrder.orderNbr>();
                                                    fields.AddField<POOrder.printed>(c => c.IsHidden());
                                                    fields.AddField<POOrder.emailed>(c => c.IsHidden());
                                                    fields.AddField<POOrder.dontPrint>(c => c.IsHidden());
                                                    fields.AddField<POOrder.dontEmail>(c => c.IsHidden());

                                                    fields.AddTable<POLine>(c => c.IsDisabled());
                                                    fields.AddField<POLine.completed>(x => x.IsDisabled().IsHidden());
                                                    fields.AddField<POLine.closed>(x => x.IsDisabled().IsHidden());
                                                    fields.AddField<POLine.promisedDate>();
													fields.AddFields<POOrderEntry_Workflow.DropShipOrderLineFields>(c => c.IsHidden());
													fields.AddFields<POOrderEntry_Workflow.BlanketOrderLineFields>(c => c.IsHidden());

                                                    fields.AddAllFields<PORemitAddress>(c => c.IsDisabled());
                                                    fields.AddAllFields<PORemitContact>(c => c.IsDisabled());
                                                    fields.AddAllFields<POShipAddress>(c => c.IsDisabled());
                                                    fields.AddAllFields<POShipContact>(c => c.IsDisabled());
                                                    fields.AddTable<POTax>(c => c.IsDisabled());

                                                    fields.AddTable<CurrencyInfo>(c => c.IsDisabled());
                                                    fields.AddTable<POOrderPrepayment>(c => c.IsHidden());
                                                });
                                        });
                                    })
                                    .WithTransitions(transitions =>
                                    {
                                        transitions.UpdateGroupFrom(State.Initial, ts =>
                                        {
                                            ts.Add(t => t
                                                .To<State.rejected>()
                                                .IsTriggeredOn(g => g.initializeState)
                                                .When(conditions.IsRejected)
                                                .PlaceBefore(tr => tr.To<State.open>()));
                                            ts.Add(t => t
                                                .To<State.pendingApproval>()
                                                .IsTriggeredOn(g => g.initializeState)
                                                .When(!conditions.IsApproved)
                                                .PlaceAfter(tr => tr.To<State.rejected>()));
                                        });

                                        transitions.UpdateGroupFrom<State.hold>(ts =>
                                        {
                                            ts.Add(t => t
                                                .To<State.pendingApproval>()
                                                .IsTriggeredOn(g => g.releaseFromHold)
                                                .When(!conditions.IsOnHold && !conditions.IsApproved)
                                                .PlaceBefore(tr => tr.To<State.open>()));
                                        });

                                        transitions.AddGroupFrom<State.pendingApproval>(ts =>
                                        {
                                            ts.Add(t => t
                                                .To<State.open>()
                                                .IsTriggeredOn(approve)
                                                .When(conditions.IsApproved));

                                            ts.Add(t => t
                                                .To<State.rejected>()
                                                .IsTriggeredOn(reject)
                                                .When(conditions.IsRejected));

                                            ts.Add(t => t
                                                .To<State.hold>()
                                                .IsTriggeredOn(g => g.putOnHold)
                                                .When(conditions.IsOnHold));
                                        });

                                        transitions.AddGroupFrom<State.rejected>(ts =>
                                        {
                                            ts.Add(t => t
                                                .To<State.hold>()
                                                .IsTriggeredOn(g => g.putOnHold)
                                                .When(conditions.IsOnHold));
                                        });
                                    });
                            });

                        if (IsDropShipApprovalActive())
                            flows.Update<POOrderType.dropShip>(flow =>
                            {
                                return flow
                                    .WithFlowStates(states =>
                                    {
                                        states.Add<State.pendingApproval>(state =>
                                        {
                                            return state
                                                .WithActions(actions =>
                                                {
                                                    actions.Add(g => g.putOnHold);
                                                    actions.Add(approve, c => c.IsDuplicatedInToolbar());
                                                    actions.Add(reject, c => c.IsDuplicatedInToolbar());
                                                    actions.Add(reassign);
                                                    actions.Add(g => g.validateAddresses);

                                                    actions.Add(g => g.vendorDetails);
                                                    actions.Add(g => g.printPurchaseOrder);

                                                    actions.Add<DropShipLinksExt>(g => g.unlinkFromSO);
                                                    actions.Add<PurchaseToSOLinksExt>(g => g.viewDemand);
                                                })
                                                .WithFieldStates(fields =>
                                                {
                                                    fields.AddAllFields<POOrder>(c => c.IsDisabled());
                                                    fields.AddField<POOrder.orderType>();
                                                    fields.AddField<POOrder.orderNbr>();
                                                    fields.AddField<POOrder.controlTotal>();
                                                    fields.AddField<POOrder.workgroupID>();
                                                    fields.AddField<POOrder.ownerID>();
                                                    fields.AddField<POOrder.printed>();
                                                    fields.AddField<POOrder.emailed>();
                                                    fields.AddField<POOrder.dontPrint>();
                                                    fields.AddField<POOrder.dontEmail>();

                                                    fields.AddTable<POLine>(c => c.IsDisabled());
                                                    fields.AddField<POLine.cancelled>();
                                                    fields.AddField<POLine.completed>();
                                                    fields.AddField<POLine.promisedDate>();
                                                    fields.AddField<POLine.closed>();
													fields.AddFields<POOrderEntry_Workflow.BlanketOrderLineFields>(c => c.IsHidden());

                                                    fields.AddTable<POTax>(c => c.IsDisabled());
                                                    fields.AddAllFields<POShipAddress>(c => c.IsDisabled());
                                                    fields.AddAllFields<POShipContact>(c => c.IsDisabled());
                                                    fields.AddTable<CurrencyInfo>(c => c.IsDisabled());
                                                });
                                        });
                                        states.Add<State.rejected>(state =>
                                        {
                                            return state
                                                .WithActions(actions =>
                                                {
                                                    actions.Add(g => g.putOnHold, c => c.IsDuplicatedInToolbar());
                                                    actions.Add(g => g.validateAddresses);

                                                    actions.Add(g => g.vendorDetails);
                                                    actions.Add(g => g.printPurchaseOrder);
                                                    actions.Add(g => g.viewPurchaseOrderReceipt);

                                                    actions.Add<DropShipLinksExt>(g => g.unlinkFromSO);
                                                    actions.Add<PurchaseToSOLinksExt>(g => g.viewDemand);
                                                })
                                                .WithFieldStates(fields =>
                                                {
                                                    fields.AddTable<POOrder>(c => c.IsDisabled());
                                                    fields.AddField<POOrder.orderType>();
                                                    fields.AddField<POOrder.orderNbr>();

                                                    fields.AddTable<POLine>(c => c.IsDisabled());
                                                    fields.AddField<POLine.cancelled>();
                                                    fields.AddField<POLine.completed>();
                                                    fields.AddField<POLine.promisedDate>();
                                                    fields.AddField<POLine.closed>();
													fields.AddFields<POOrderEntry_Workflow.BlanketOrderLineFields>(c => c.IsHidden());

                                                    fields.AddAllFields<PORemitAddress>(c => c.IsDisabled());
                                                    fields.AddAllFields<PORemitContact>(c => c.IsDisabled());
                                                    fields.AddAllFields<POShipAddress>(c => c.IsDisabled());
                                                    fields.AddAllFields<POShipContact>(c => c.IsDisabled());
                                                    fields.AddTable<POTax>(c => c.IsDisabled());

                                                    fields.AddTable<CurrencyInfo>(c => c.IsDisabled());
                                                });
                                        });
                                    })
                                    .WithTransitions(transitions =>
                                    {
                                        transitions.UpdateGroupFrom(State.Initial, ts =>
                                        {                                            
                                            ts.Add(t => t
                                                .To<State.rejected>()
                                                .IsTriggeredOn(g => g.initializeState)
                                                .When(conditions.IsRejected)
                                                .PlaceBefore(tr => tr.To<State.pendingPrint>()));
                                            ts.Add(t => t
                                                .To<State.pendingApproval>()
                                                .IsTriggeredOn(g => g.initializeState)
                                                .When(!conditions.IsApproved)
                                                .PlaceAfter(tr => tr.To<State.rejected>()));
                                        });

                                        transitions.UpdateGroupFrom<State.hold>(ts =>
                                        {
                                            ts.Add(t => t
                                                .To<State.pendingApproval>()
                                                .IsTriggeredOn(g => g.releaseFromHold)
                                                .When(!conditions.IsOnHold && !conditions.IsApproved)
                                                .PlaceBefore(tr => tr.To<State.pendingPrint>()));
                                        });

                                        transitions.AddGroupFrom<State.pendingApproval>(ts =>
                                        {
                                            ts.Add(t => t
                                                .To<State.pendingPrint>()
                                                .IsTriggeredOn(approve)
                                                .When(conditions.IsApproved && !conditions.IsPrinted));
                                            ts.Add(t => t
                                                .To<State.pendingEmail>()
                                                .IsTriggeredOn(approve)
                                                .When(conditions.IsApproved && !conditions.IsEmailed));
                                            ts.Add(t => t
                                                .To<State.closed>()
                                                .IsTriggeredOn(approve)
                                                .When(conditions.IsApproved && conditions.HasAllLinesClosed));
                                            ts.Add(t => t
                                                .To<State.completed>()
                                                .IsTriggeredOn(approve)
                                                .When(conditions.IsApproved && conditions.HasAllLinesCompleted));
                                            ts.Add(t => t
                                                .To<State.awaitingLink>()
                                                .IsTriggeredOn(approve)
                                                .When(conditions.IsApproved && !conditions.HasAllDropShipLinesLinked));
                                            ts.Add(t => t
                                                .To<State.open>()
                                                .IsTriggeredOn(approve)
                                                .When(conditions.IsApproved));

                                            ts.Add(t => t
                                                .To<State.rejected>()
                                                .IsTriggeredOn(reject)
                                                .When(conditions.IsRejected));

                                            ts.Add(t => t
                                                .To<State.hold>()
                                                .IsTriggeredOn(g => g.putOnHold)
                                                .When(conditions.IsOnHold));
                                        });

                                        transitions.AddGroupFrom<State.rejected>(ts =>
                                        {
                                            ts.Add(t => t
                                                .To<State.hold>()
                                                .IsTriggeredOn(g => g.putOnHold)
                                                .When(conditions.IsOnHold));
                                        });
                                    });
                            });

                        if (IsNormalApprovalActive())
                            flows.Update<POOrderType.regularOrder>(flow =>
                            {
                                return flow
                                    .WithFlowStates(states =>
                                    {
                                        states.Add<State.pendingApproval>(state =>
                                        {
                                            return state
                                                .WithActions(actions =>
                                                {
                                                    actions.Add(g => g.putOnHold);
                                                    actions.Add(approve, c => c.IsDuplicatedInToolbar());
                                                    actions.Add(reject, c => c.IsDuplicatedInToolbar());
                                                    actions.Add(reassign);
                                                    actions.Add(g => g.validateAddresses);

                                                    actions.Add(g => g.vendorDetails);
                                                    actions.Add(g => g.printPurchaseOrder);

                                                    actions.Add<PurchaseToSOLinksExt>(g => g.viewDemand);
                                                })
                                                .WithFieldStates(fields =>
                                                {
                                                    fields.AddAllFields<POOrder>(c => c.IsDisabled());
                                                    fields.AddField<POOrder.orderType>();
                                                    fields.AddField<POOrder.orderNbr>();
                                                    fields.AddField<POOrder.controlTotal>();
                                                    fields.AddField<POOrder.workgroupID>();
                                                    fields.AddField<POOrder.ownerID>();
                                                    fields.AddField<POOrder.printed>();
                                                    fields.AddField<POOrder.emailed>();
                                                    fields.AddField<POOrder.dontPrint>();
                                                    fields.AddField<POOrder.dontEmail>();
													fields.AddField<POOrder.excludeFromIntercompanyProc>();

                                                    fields.AddTable<POLine>(c => c.IsDisabled());
                                                    fields.AddField<POLine.cancelled>();
                                                    fields.AddField<POLine.completed>();
                                                    fields.AddField<POLine.promisedDate>();
                                                    fields.AddField<POLine.closed>();
													fields.AddFields<POOrderEntry_Workflow.DropShipOrderLineFields>(c => c.IsHidden());
													fields.AddFields<POOrderEntry_Workflow.BlanketOrderLineFields>(c => c.IsHidden());

                                                    fields.AddTable<POTax>(c => c.IsDisabled());
                                                    fields.AddAllFields<POShipAddress>(c => c.IsDisabled());
                                                    fields.AddAllFields<POShipContact>(c => c.IsDisabled());
                                                    fields.AddTable<CurrencyInfo>(c => c.IsDisabled());
                                                });
                                        });
                                        states.Add<State.rejected>(state =>
                                        {
                                            return state
                                                .WithActions(actions =>
                                                {
                                                    actions.Add(g => g.putOnHold, c => c.IsDuplicatedInToolbar());
                                                    actions.Add(g => g.validateAddresses);

                                                    actions.Add(g => g.vendorDetails);
                                                    actions.Add(g => g.printPurchaseOrder);
                                                    actions.Add(g => g.viewPurchaseOrderReceipt);

                                                    actions.Add<PurchaseToSOLinksExt>(g => g.viewDemand);
                                                })
                                                .WithFieldStates(fields =>
                                                {
                                                    fields.AddTable<POOrder>(c => c.IsDisabled());
                                                    fields.AddField<POOrder.orderType>();
                                                    fields.AddField<POOrder.orderNbr>();

                                                    fields.AddTable<POLine>(c => c.IsDisabled());
                                                    fields.AddField<POLine.cancelled>();
                                                    fields.AddField<POLine.completed>();
                                                    fields.AddField<POLine.promisedDate>();
                                                    fields.AddField<POLine.closed>();
													fields.AddFields<POOrderEntry_Workflow.DropShipOrderLineFields>(c => c.IsHidden());
													fields.AddFields<POOrderEntry_Workflow.BlanketOrderLineFields>(c => c.IsHidden());

                                                    fields.AddAllFields<PORemitAddress>(c => c.IsDisabled());
                                                    fields.AddAllFields<PORemitContact>(c => c.IsDisabled());
                                                    fields.AddAllFields<POShipAddress>(c => c.IsDisabled());
                                                    fields.AddAllFields<POShipContact>(c => c.IsDisabled());
                                                    fields.AddTable<POTax>(c => c.IsDisabled());

                                                    fields.AddTable<CurrencyInfo>(c => c.IsDisabled());
                                                });
                                        });
                                    })
                                    .WithTransitions(transitions =>
                                    {
                                        transitions.UpdateGroupFrom(State.Initial, ts =>
                                        {
                                            ts.Add(t => t
                                                .To<State.rejected>()
                                                .IsTriggeredOn(g => g.initializeState)
                                                .When(conditions.IsRejected)
                                                .PlaceBefore(tr => tr.To<State.pendingPrint>()));
                                            ts.Add(t => t
                                                .To<State.pendingApproval>()
                                                .IsTriggeredOn(g => g.initializeState)
                                                .When(!conditions.IsApproved)
                                                .PlaceAfter(tr => tr.To<State.rejected>()));
                                        });

                                        transitions.UpdateGroupFrom<State.hold>(ts =>
                                        {
                                            ts.Add(t => t
                                                .To<State.pendingApproval>()
                                                .IsTriggeredOn(g => g.releaseFromHold)
                                                .When(!conditions.IsOnHold && !conditions.IsApproved)
                                                .PlaceBefore(tr => tr.To<State.pendingPrint>()));
                                        });

                                        transitions.AddGroupFrom<State.pendingApproval>(ts =>
                                        {
                                            ts.Add(t => t
                                                .To<State.pendingPrint>()
                                                .IsTriggeredOn(approve)
                                                .When(conditions.IsApproved && !conditions.IsPrinted));
                                            ts.Add(t => t
                                                .To<State.pendingEmail>()
                                                .IsTriggeredOn(approve)
                                                .When(conditions.IsApproved && !conditions.IsEmailed));
                                            ts.Add(t => t
                                                .To<State.closed>()
                                                .IsTriggeredOn(approve)
                                                .When(conditions.IsApproved && conditions.HasAllLinesClosed));
                                            ts.Add(t => t
                                                .To<State.completed>()
                                                .IsTriggeredOn(approve)
                                                .When(conditions.IsApproved && conditions.HasAllLinesCompleted));
                                            ts.Add(t => t
                                                .To<State.open>()
                                                .IsTriggeredOn(approve)
                                                .When(conditions.IsApproved));

                                            ts.Add(t => t
                                                .To<State.rejected>()
                                                .IsTriggeredOn(reject)
                                                .When(conditions.IsRejected));

                                            ts.Add(t => t
                                                .To<State.hold>()
                                                .IsTriggeredOn(g => g.putOnHold)
                                                .When(conditions.IsOnHold));
                                        });

                                        transitions.AddGroupFrom<State.rejected>(ts =>
                                        {
                                            ts.Add(t => t
                                                .To<State.hold>()
                                                .IsTriggeredOn(g => g.putOnHold)
                                                .When(conditions.IsOnHold));
                                        });
                                    });
                            });

                        if (IsProjectDropShipApprovalActive())
                            flows.Update<POOrderType.projectDropShip>(flow =>
                            {
                                return flow
                                    .WithFlowStates(states =>
                                    {
                                        states.Add<State.pendingApproval>(state =>
                                        {
                                            return state
                                                .WithActions(actions =>
                                                {
                                                    actions.Add(g => g.putOnHold);
                                                    actions.Add(approve, c => c.IsDuplicatedInToolbar());
                                                    actions.Add(reject, c => c.IsDuplicatedInToolbar());
                                                    actions.Add(reassign);
                                                    actions.Add(g => g.validateAddresses);

                                                    actions.Add(g => g.vendorDetails);
                                                    actions.Add(g => g.printPurchaseOrder);
                                                })
                                                .WithFieldStates(fields =>
                                                {
                                                    fields.AddAllFields<POOrder>(c => c.IsDisabled());
                                                    fields.AddField<POOrder.orderType>();
                                                    fields.AddField<POOrder.orderNbr>();
                                                    fields.AddField<POOrder.controlTotal>();
                                                    fields.AddField<POOrder.workgroupID>();
                                                    fields.AddField<POOrder.ownerID>();
                                                    fields.AddField<POOrder.printed>();
                                                    fields.AddField<POOrder.emailed>();
                                                    fields.AddField<POOrder.dontPrint>();
                                                    fields.AddField<POOrder.dontEmail>();
                                                    fields.AddField<POOrder.excludeFromIntercompanyProc>();

                                                    fields.AddTable<POLine>(c => c.IsDisabled());
                                                    fields.AddField<POLine.cancelled>();
                                                    fields.AddField<POLine.completed>();
                                                    fields.AddField<POLine.promisedDate>();
                                                    fields.AddField<POLine.closed>();
													fields.AddFields<POOrderEntry_Workflow.DropShipOrderLineFields>(c => c.IsHidden());
													fields.AddFields<POOrderEntry_Workflow.BlanketOrderLineFields>(c => c.IsHidden());

                                                    fields.AddTable<POTax>(c => c.IsDisabled());
                                                    fields.AddAllFields<POShipAddress>(c => c.IsDisabled());
                                                    fields.AddAllFields<POShipContact>(c => c.IsDisabled());
                                                    fields.AddTable<CurrencyInfo>(c => c.IsDisabled());
                                                });
                                        });
                                        states.Add<State.rejected>(state =>
                                        {
                                            return state
                                                .WithActions(actions =>
                                                {
                                                    actions.Add(g => g.putOnHold, c => c.IsDuplicatedInToolbar());
                                                    actions.Add(g => g.validateAddresses);

                                                    actions.Add(g => g.vendorDetails);
                                                    actions.Add(g => g.printPurchaseOrder);
                                                    actions.Add(g => g.viewPurchaseOrderReceipt);
                                                })
                                                .WithFieldStates(fields =>
                                                {
                                                    fields.AddTable<POOrder>(c => c.IsDisabled());
                                                    fields.AddField<POOrder.orderType>();
                                                    fields.AddField<POOrder.orderNbr>();

                                                    fields.AddTable<POLine>(c => c.IsDisabled());
                                                    fields.AddField<POLine.cancelled>();
                                                    fields.AddField<POLine.completed>();
                                                    fields.AddField<POLine.promisedDate>();
                                                    fields.AddField<POLine.closed>();
													fields.AddFields<POOrderEntry_Workflow.DropShipOrderLineFields>(c => c.IsHidden());
													fields.AddFields<POOrderEntry_Workflow.BlanketOrderLineFields>(c => c.IsHidden());

                                                    fields.AddAllFields<PORemitAddress>(c => c.IsDisabled());
                                                    fields.AddAllFields<PORemitContact>(c => c.IsDisabled());
                                                    fields.AddAllFields<POShipAddress>(c => c.IsDisabled());
                                                    fields.AddAllFields<POShipContact>(c => c.IsDisabled());
                                                    fields.AddTable<POTax>(c => c.IsDisabled());

                                                    fields.AddTable<CurrencyInfo>(c => c.IsDisabled());
                                                });
                                        });
                                    })
                                    .WithTransitions(transitions =>
                                    {
                                        transitions.UpdateGroupFrom(State.Initial, ts =>
                                        {
                                            ts.Add(t => t
                                                .To<State.rejected>()
                                                .IsTriggeredOn(g => g.initializeState)
                                                .When(conditions.IsRejected)
                                                .PlaceBefore(tr => tr.To<State.pendingPrint>()));
                                            ts.Add(t => t
                                                .To<State.pendingApproval>()
                                                .IsTriggeredOn(g => g.initializeState)
                                                .When(!conditions.IsApproved)
                                                .PlaceAfter(tr => tr.To<State.rejected>()));
                                        });

                                        transitions.UpdateGroupFrom<State.hold>(ts =>
                                        {
                                            ts.Add(t => t
                                                .To<State.pendingApproval>()
                                                .IsTriggeredOn(g => g.releaseFromHold)
                                                .When(!conditions.IsOnHold && !conditions.IsApproved)
                                                .PlaceBefore(tr => tr.To<State.pendingPrint>()));
                                        });

                                        transitions.AddGroupFrom<State.pendingApproval>(ts =>
                                        {
                                            ts.Add(t => t
                                                .To<State.pendingPrint>()
                                                .IsTriggeredOn(approve)
                                                .When(conditions.IsApproved && !conditions.IsPrinted));
                                            ts.Add(t => t
                                                .To<State.pendingEmail>()
                                                .IsTriggeredOn(approve)
                                                .When(conditions.IsApproved && !conditions.IsEmailed));
                                            ts.Add(t => t
                                                .To<State.closed>()
                                                .IsTriggeredOn(approve)
                                                .When(conditions.IsApproved && conditions.HasAllLinesClosed));
                                            ts.Add(t => t
                                                .To<State.completed>()
                                                .IsTriggeredOn(approve)
                                                .When(conditions.IsApproved && conditions.HasAllLinesCompleted));
                                            ts.Add(t => t
                                                .To<State.open>()
                                                .IsTriggeredOn(approve)
                                                .When(conditions.IsApproved));

                                            ts.Add(t => t
                                                .To<State.rejected>()
                                                .IsTriggeredOn(reject)
                                                .When(conditions.IsRejected));

                                            ts.Add(t => t
                                                .To<State.hold>()
                                                .IsTriggeredOn(g => g.putOnHold)
                                                .When(conditions.IsOnHold));
                                        });

                                        transitions.AddGroupFrom<State.rejected>(ts =>
                                        {
                                            ts.Add(t => t
                                                .To<State.hold>()
                                                .IsTriggeredOn(g => g.putOnHold)
                                                .When(conditions.IsOnHold));
                                        });
                                    });
                            });
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
                    })
                    .WithCategories(categories =>
                    {
                        categories.Add(approvalCategory);
                    });
            });
        }
    }
}
