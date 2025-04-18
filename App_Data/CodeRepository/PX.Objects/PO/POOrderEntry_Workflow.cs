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
using PX.Objects.Common;
using PX.Objects.CM.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.PO.GraphExtensions.POOrderEntryExt;
using PX.Common;

namespace PX.Objects.PO
{
    using static BoundedTo<POOrderEntry, POOrder>;
    using static POOrder;
    using Prepayments = GraphExtensions.POOrderEntryExt.Prepayments;
    using State = POOrderStatus;
    using DropShipLinksExt = GraphExtensions.POOrderEntryExt.DropShipLinksExt;
    using PurchaseToSOLinksExt = GraphExtensions.POOrderEntryExt.PurchaseToSOLinksExt;

    public class POOrderEntry_Workflow : PXGraphExtension<POOrderEntry>
    {
        public sealed override void Configure(PXScreenConfiguration config) =>
            Configure(config.GetScreenConfigurationContext<POOrderEntry, POOrder>());

        protected static void Configure(WorkflowContext<POOrderEntry, POOrder> context)
        {
            #region Conditions
            Condition Bql<T>() where T : IBqlUnary, new() => context.Conditions.FromBql<T>();
            var conditions = new
            {
                IsOnHold
                    = Bql<hold.IsEqual<True>>(),
                IsCancelled
                    = Bql<cancelled.IsEqual<True>>(),

                IsPrinted
                    = Bql<printedExt.IsEqual<True>>(),
                IsEmailed
                    = Bql<emailedExt.IsEqual<True>>(),

                IsChangeOrder
                    = Bql<behavior.IsEqual<POBehavior.changeOrder>>(),

                HasAllLinesClosed
                    = Bql<linesToCloseCntr.IsEqual<Zero>>(),
                HasAllLinesCompleted
                    = Bql<linesToCloseCntr.IsNotEqual<Zero>
                        .And<linesToCompleteCntr.IsEqual<Zero>>>(),

				IsNotIntercompany
					= Bql<isIntercompany.IsEqual<False>>(),

				IsIntercompanyOrderGenerated
					= Bql<intercompanySONbr.IsNotNull>(),

                HasAllDropShipLinesLinked
                    = Bql<dropShipOpenLinesCntr.IsEqual<Zero>
                        .Or<dropShipNotLinkedLinesCntr.IsEqual<Zero>>>(),

                IsNewDropShipOrder
                    = Bql<orderType.IsEqual<POOrderType.dropShip>
                        .And<isLegacyDropShip.IsNotEqual<True>>>(),
                IsLinkedToSalesOrder
                    = Bql<orderType.IsEqual<POOrderType.dropShip>
                        .And<sOOrderNbr.IsNotNull>>(),

                ProjectDropShipReceiptsNotAllowed
                    = Bql<orderType.IsEqual<POOrderType.projectDropShip>
                        .And<dropshipReceiptProcessing.IsEqual<PX.Objects.PM.DropshipReceiptProcessingOption.skipReceipt>>>(),
            }
            .AutoNameConditions();
            #endregion

            #region Categories
            var commonCategories = CommonActionCategories.Get(context);
            var processingCategory = commonCategories.Processing;
            var dropShipCategory = context.Categories.CreateNew(ActionCategories.DropShipCategoryID,
                    category => category.DisplayName(ActionCategories.DisplayNames.DropShip));
            var intercompanyCategory = commonCategories.Intercompany;
            var printingEmailingCategory = commonCategories.PrintingAndEmailing;
            var otherCategory = commonCategories.Other;
            #endregion

            context.AddScreenConfigurationFor(screen =>
            {
                return screen
                    .StateIdentifierIs<status>()
                    .FlowTypeIdentifierIs<orderType>(true)
                    .WithFlows(flows =>
                    {
                        flows.Add<POOrderType.regularOrder>(flow =>
                        {
                            return flow
                                .WithFlowStates(states =>
                                {
                                    states.Add(State.Initial, state => state.IsInitial(g => g.initializeState));
                                    states.Add<State.hold>(state =>
                                    {
                                        return state
                                            .WithActions(actions =>
                                            {
                                                actions.Add(g => g.releaseFromHold, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
                                                actions.Add(g => g.validateAddresses);
                                                actions.Add(g => g.recalculateDiscountsAction);

                                                actions.Add(g => g.vendorDetails);
                                                actions.Add(g => g.printPurchaseOrder);
                                                actions.Add(g => g.viewPurchaseOrderReceipt);

                                                actions.Add(g => g.addPOOrder);
                                                actions.Add(g => g.addPOOrderLine);
                                                actions.Add<PurchaseToSOLinksExt>(g => g.viewDemand);
                                                actions.Add(g => g.addInvBySite);
                                                actions.Add(g => g.addInvSelBySite);
                                            })
                                            .WithFieldStates(fields =>
                                            {
                                                fields.AddAllFields<POOrder>();
                                                fields.AddTable<POLine>();
												fields.AddFields<DropShipOrderLineFields>(x => x.IsHidden());
												fields.AddFields<BlanketOrderLineFields>(c => c.IsHidden());
												fields.AddTable<POTaxTran>();
												fields.AddTable<POOrderDiscountDetail>();
                                                fields.AddAllFields<POShipAddress>();
                                                fields.AddAllFields<POShipContact>();
                                                fields.AddField<POOrder.approved>(c => c.IsDisabled());
                                                fields.AddField<POOrder.ownerID>();
                                                fields.AddField<POOrder.workgroupID>(c => c.IsDisabled());
                                            });
                                    });
                                    states.Add<State.pendingPrint>(state =>
                                    {
                                        return state
                                            .WithActions(actions =>
                                            {
                                                actions.Add(g => g.putOnHold);
                                                actions.Add(g => g.markAsDontPrint, c => c.IsDuplicatedInToolbar());
                                                actions.Add(g => g.cancelOrder);
                                                actions.Add(g => g.validateAddresses);

                                                actions.Add(g => g.printPurchaseOrder, c => c.IsDuplicatedInToolbar());
                                                actions.Add(g => g.vendorDetails);

                                                actions.Add<PurchaseToSOLinksExt>(g => g.viewDemand);
                                            })
                                            .WithFieldStates(fields =>
                                            {
                                                fields.AddAllFields<POOrder>(c => c.IsDisabled());
                                                fields.AddField<POOrder.orderType>();
                                                fields.AddField<POOrder.orderNbr>();
                                                fields.AddField<POOrder.controlTotal>();
                                                fields.AddField<POOrder.dontPrint>();
												fields.AddField<POOrder.excludeFromIntercompanyProc>();

                                                fields.AddTable<POLine>(c => c.IsDisabled());
                                                fields.AddField<POLine.cancelled>();
                                                fields.AddField<POLine.completed>();
                                                fields.AddField<POLine.promisedDate>();
                                                fields.AddField<POLine.closed>();
												fields.AddFields<DropShipOrderLineFields>(x => x.IsHidden());
												fields.AddFields<BlanketOrderLineFields>(c => c.IsHidden());

												fields.AddTable<POTaxTran>(c => c.IsDisabled());
												fields.AddTable<POOrderDiscountDetail>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipContact>(c => c.IsDisabled());
                                                fields.AddTable<CurrencyInfo>(c => c.IsDisabled());
                                            })
                                            .WithEventHandlers(handlers =>
                                            {
                                                handlers.Add(g => g.OnLinesCompleted);
                                                handlers.Add(g => g.OnLinesClosed);
                                                handlers.Add(g => g.OnPrinted);
                                                handlers.Add(g => g.OnDoNotPrintChecked);
                                            });
                                    });
                                    states.Add<State.pendingEmail>(state =>
                                    {
                                        return state
                                            .WithActions(actions =>
                                            {
                                                actions.Add(g => g.putOnHold);
                                                actions.Add(g => g.cancelOrder);
                                                actions.Add(g => g.validateAddresses);
                                                actions.Add(g => g.emailPurchaseOrder, c => c.IsDuplicatedInToolbar());
                                                actions.Add(g => g.markAsDontEmail, c => c.IsDuplicatedInToolbar());

                                                actions.Add(g => g.printPurchaseOrder);
                                                actions.Add(g => g.vendorDetails);

                                                actions.Add<PurchaseToSOLinksExt>(g => g.viewDemand);
                                            })
                                            .WithFieldStates(fields =>
                                            {
                                                fields.AddAllFields<POOrder>(c => c.IsDisabled());
                                                fields.AddField<POOrder.orderType>();
                                                fields.AddField<POOrder.orderNbr>();
                                                fields.AddField<POOrder.controlTotal>();
                                                fields.AddField<POOrder.dontEmail>();
												fields.AddField<POOrder.excludeFromIntercompanyProc>();

                                                fields.AddTable<POLine>(c => c.IsDisabled());
                                                fields.AddField<POLine.cancelled>();
                                                fields.AddField<POLine.completed>();
                                                fields.AddField<POLine.promisedDate>();
                                                fields.AddField<POLine.closed>();
												fields.AddFields<DropShipOrderLineFields>(x => x.IsHidden());
												fields.AddFields<BlanketOrderLineFields>(c => c.IsHidden());

												fields.AddTable<POTaxTran>(c => c.IsDisabled());
												fields.AddTable<POOrderDiscountDetail>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipContact>(c => c.IsDisabled());
                                                fields.AddTable<CurrencyInfo>(c => c.IsDisabled());
                                            })
                                            .WithEventHandlers(handlers =>
                                            {
                                                handlers.Add(g => g.OnLinesCompleted);
                                                handlers.Add(g => g.OnLinesClosed);
                                                handlers.Add(g => g.OnDoNotEmailChecked);
                                            });
                                    });
                                    states.Add<State.open>(state =>
                                    {
                                        return state
                                            .WithActions(actions =>
                                            {
                                                actions.Add(g => g.putOnHold);

                                                actions.Add(g => g.complete);
                                                actions.Add(g => g.cancelOrder);

                                                actions.Add(g => g.emailPurchaseOrder);
                                                actions.Add(g => g.createPOReceipt, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
                                                actions.Add(g => g.createAPInvoice, c => c.WithConnotation(ActionConnotation.Success));

                                                actions.Add(g => g.validateAddresses);
                                                actions.Add<Prepayments>(g => g.createPrepayment);

                                                actions.Add(g => g.vendorDetails);
                                                actions.Add(g => g.printPurchaseOrder);
                                                actions.Add(g => g.viewPurchaseOrderReceipt);

												actions.Add<Intercompany>(e => e.generateSalesOrder);
                                                actions.Add<PurchaseToSOLinksExt>(g => g.viewDemand);
                                            })
                                            .WithFieldStates(fields =>
                                            {
                                                fields.AddAllFields<POOrder>(c => c.IsDisabled());
                                                fields.AddField<POOrder.orderType>();
                                                fields.AddField<POOrder.orderNbr>();
                                                fields.AddField<POOrder.controlTotal>();
												fields.AddField<POOrder.excludeFromIntercompanyProc>();

                                                fields.AddTable<POLine>(c => c.IsDisabled());
                                                fields.AddField<POLine.cancelled>();
                                                fields.AddField<POLine.completed>();
                                                fields.AddField<POLine.promisedDate>();
                                                fields.AddField<POLine.closed>();
												fields.AddFields<DropShipOrderLineFields>(x => x.IsHidden());
												fields.AddFields<BlanketOrderLineFields>(c => c.IsHidden());

												fields.AddTable<POTaxTran>(c => c.IsDisabled());
												fields.AddTable<POOrderDiscountDetail>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipContact>(c => c.IsDisabled());
                                                fields.AddTable<CurrencyInfo>(c => c.IsDisabled());
                                            })
                                            .WithEventHandlers(handlers =>
                                            {
                                                handlers.Add(g => g.OnLinesCompleted);
                                                handlers.Add(g => g.OnLinesClosed);
                                                handlers.Add(g => g.OnReleaseChangeOrder);
                                            });
                                    });
                                    states.Add<State.completed>(state =>
                                    {
                                        return state
                                            .WithActions(actions =>
                                            {
                                                actions.Add(g => g.reopenOrder);
                                                actions.Add(g => g.validateAddresses);
                                                actions.Add(g => g.createAPInvoice, c => c.WithConnotation(ActionConnotation.Success));
                                                actions.Add<Prepayments>(g => g.createPrepayment);

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
												fields.AddFields<DropShipOrderLineFields>(x => x.IsHidden());
												fields.AddFields<BlanketOrderLineFields>(c => c.IsHidden());

												fields.AddTable<POTaxTran>(c => c.IsDisabled());
												fields.AddTable<POOrderDiscountDetail>(c => c.IsDisabled());
                                                fields.AddAllFields<PORemitAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<PORemitContact>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipContact>(c => c.IsDisabled());

                                                fields.AddTable<CurrencyInfo>(c => c.IsDisabled());
                                            })
                                            .WithEventHandlers(handlers =>
                                            {
                                                handlers.Add(g => g.OnLinesClosed);
                                                handlers.Add(g => g.OnLinesReopened);
                                                handlers.Add(g => g.OnReleaseChangeOrder);
                                            });
                                    });
                                    states.Add<State.cancelled>(state =>
                                    {
                                        return state
                                            .WithActions(actions =>
                                            {
                                                actions.Add(g => g.reopenOrder);
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
												fields.AddFields<DropShipOrderLineFields>(x => x.IsHidden());
												fields.AddFields<BlanketOrderLineFields>(c => c.IsHidden());

												fields.AddAllFields<PORemitAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<PORemitContact>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipContact>(c => c.IsDisabled());
                                                fields.AddTable<POTaxTran>(c => c.IsDisabled());
												fields.AddTable<POOrderDiscountDetail>(c => c.IsDisabled());
                                                fields.AddTable<CurrencyInfo>(c => c.IsDisabled());
                                            })
                                            .WithEventHandlers(handlers =>
                                            {
                                                handlers.Add(g => g.OnLinesReopened);
                                                handlers.Add(g => g.OnReleaseChangeOrder);
                                            });
                                    });
                                    states.Add<State.closed>(state =>
                                    {
                                        return state
                                            .WithActions(actions =>
                                            {
                                                actions.Add(g => g.reopenOrder);
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
												fields.AddFields<DropShipOrderLineFields>(x => x.IsHidden());
												fields.AddFields<BlanketOrderLineFields>(c => c.IsHidden());

												fields.AddAllFields<PORemitAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<PORemitContact>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipContact>(c => c.IsDisabled());
                                                fields.AddTable<POTaxTran>(c => c.IsDisabled());
												fields.AddTable<POOrderDiscountDetail>(c => c.IsDisabled());
                                                fields.AddTable<CurrencyInfo>(c => c.IsDisabled());
                                            })
                                            .WithEventHandlers(handlers =>
                                            {
                                                handlers.Add(g => g.OnLinesReopened);
                                                handlers.Add(g => g.OnLinesCompleted);
                                                handlers.Add(g => g.OnReleaseChangeOrder);
                                            });
                                    });
                                })
                                .WithTransitions(transitions =>
                                {
                                    transitions.AddGroupFrom(State.Initial, ts =>
                                    {
                                        ts.Add(t => t
                                            .To<State.hold>()
                                            .IsTriggeredOn(g => g.initializeState)
                                            .When(conditions.IsOnHold));
                                        ts.Add(t => t
                                            .To<State.pendingPrint>()
                                            .IsTriggeredOn(g => g.initializeState)
                                            .When(!conditions.IsPrinted));
                                        ts.Add(t => t
                                            .To<State.pendingEmail>()
                                            .IsTriggeredOn(g => g.initializeState)
                                            .When(!conditions.IsEmailed));
                                        ts.Add(t => t
                                            .To<State.cancelled>()
                                            .IsTriggeredOn(g => g.initializeState)
                                            .When(conditions.IsCancelled));
                                        ts.Add(t => t
                                            .To<State.closed>()
                                            .IsTriggeredOn(g => g.initializeState)
                                            .When(conditions.HasAllLinesClosed));
                                        ts.Add(t => t
                                            .To<State.completed>()
                                            .IsTriggeredOn(g => g.initializeState)
                                            .When(conditions.HasAllLinesCompleted));
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.initializeState));
                                    });
                                    transitions.AddGroupFrom<State.hold>(ts =>
                                    {
                                        ts.Add(t => t
                                            .To<State.pendingPrint>()
                                            .IsTriggeredOn(g => g.releaseFromHold)
                                            .When(!conditions.IsOnHold && !conditions.IsPrinted));
                                        ts.Add(t => t
                                            .To<State.pendingEmail>()
                                            .IsTriggeredOn(g => g.releaseFromHold)
                                            .When(!conditions.IsOnHold && !conditions.IsEmailed));
                                        ts.Add(t => t
                                            .To<State.closed>()
                                            .IsTriggeredOn(g => g.releaseFromHold)
                                            .When(!conditions.IsOnHold && conditions.HasAllLinesClosed));
                                        ts.Add(t => t
                                            .To<State.completed>()
                                            .IsTriggeredOn(g => g.releaseFromHold)
                                            .When(!conditions.IsOnHold && conditions.HasAllLinesCompleted));
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.releaseFromHold)
                                            .When(!conditions.IsOnHold));
                                    });
                                    transitions.AddGroupFrom<State.pendingPrint>(ts =>
                                    {
                                        ts.Add(t => t
                                            .To<State.hold>()
                                            .IsTriggeredOn(g => g.putOnHold)
                                            .When(conditions.IsOnHold));
                                        ts.Add(t => t
                                            .To<State.cancelled>()
                                            .IsTriggeredOn(g => g.cancelOrder)
                                            .When(conditions.IsCancelled));

                                        ts.Add(t => t
                                            .To<State.pendingEmail>()
                                            .IsTriggeredOn(g => g.OnPrinted)
                                            .When(!conditions.IsEmailed)
                                            .WithFieldAssignments(fas => fas.Add<printed>(true)));
                                        ts.Add(t => t
                                            .To<State.closed>()
                                            .IsTriggeredOn(g => g.OnPrinted)
                                            .When(conditions.HasAllLinesClosed)
                                            .WithFieldAssignments(fas => fas.Add<printed>(true)));
                                        ts.Add(t => t
                                            .To<State.completed>()
                                            .IsTriggeredOn(g => g.OnPrinted)
                                            .When(conditions.HasAllLinesCompleted)
                                            .WithFieldAssignments(fas => fas.Add<printed>(true)));
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.OnPrinted)
                                            .WithFieldAssignments(fas => fas.Add<printed>(true)));

                                        ts.Add(t => t
                                            .To<State.pendingEmail>()
                                            .IsTriggeredOn(g => g.OnDoNotPrintChecked)
                                            .When(!conditions.IsEmailed)
                                            .DoesNotPersist());
                                        ts.Add(t => t
                                           .To<State.closed>()
                                           .IsTriggeredOn(g => g.OnDoNotPrintChecked)
                                           .When(conditions.HasAllLinesClosed)
                                            .DoesNotPersist());
                                        ts.Add(t => t
                                            .To<State.completed>()
                                            .IsTriggeredOn(g => g.OnDoNotPrintChecked)
                                            .When(conditions.HasAllLinesCompleted)
                                            .DoesNotPersist());
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.OnDoNotPrintChecked)
                                            .DoesNotPersist());
                                    });
                                    transitions.AddGroupFrom<State.pendingEmail>(ts =>
                                    {
                                        ts.Add(t => t
                                            .To<State.hold>()
                                            .IsTriggeredOn(g => g.putOnHold)
                                            .When(conditions.IsOnHold));

                                        ts.Add(t => t
                                            .To<State.cancelled>()
                                            .IsTriggeredOn(g => g.cancelOrder)
                                            .When(conditions.IsCancelled));

                                        ts.Add(t => t
                                            .To<State.closed>()
                                            .IsTriggeredOn(g => g.emailPurchaseOrder)
                                            .When(conditions.HasAllLinesClosed));
                                        ts.Add(t => t
                                            .To<State.completed>()
                                            .IsTriggeredOn(g => g.emailPurchaseOrder)
                                            .When(conditions.HasAllLinesCompleted));
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.emailPurchaseOrder));

                                        ts.Add(t => t
                                            .To<State.closed>()
                                            .IsTriggeredOn(g => g.OnDoNotEmailChecked)
                                             .When(conditions.HasAllLinesClosed));
                                        ts.Add(t => t
                                            .To<State.completed>()
                                            .IsTriggeredOn(g => g.OnDoNotEmailChecked)
                                            .When(conditions.HasAllLinesCompleted));
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.OnDoNotEmailChecked));
                                    });
                                    transitions.AddGroupFrom<State.open>(ts =>
                                    {
                                        ts.Add(t => t
                                            .To<State.hold>()
                                            .IsTriggeredOn(g => g.putOnHold)
                                            .When(conditions.IsOnHold));
                                        ts.Add(t => t
                                            .To<State.cancelled>()
                                            .IsTriggeredOn(g => g.cancelOrder)
                                            .When(conditions.IsCancelled));
                                        ts.Add(t => t
                                            .To<State.completed>()
                                            .IsTriggeredOn(g => g.OnLinesCompleted)
                                            .DoesNotPersist());
                                        ts.Add(t => t
                                            .To<State.completed>()
                                            .IsTriggeredOn(g => g.complete)
                                            .When(conditions.HasAllLinesCompleted));
                                        ts.Add(t => t
                                            .To<State.closed>()
                                            .IsTriggeredOn(g => g.OnLinesClosed));
                                        ts.Add(t => t
                                            .To<State.pendingPrint>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsPrinted));
                                        ts.Add(t => t
                                            .To<State.pendingEmail>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsEmailed));
                                    });
                                    transitions.AddGroupFrom<State.cancelled>(ts =>
                                    {
                                        ts.Add(t => t
                                            .To<State.hold>()
                                            .IsTriggeredOn(g => g.reopenOrder)
                                            .When(conditions.IsOnHold));
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.OnLinesReopened));
                                        ts.Add(t => t
                                            .To<State.pendingPrint>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsPrinted));
                                        ts.Add(t => t
                                            .To<State.pendingEmail>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsEmailed));
                                    });
                                    transitions.AddGroupFrom<State.completed>(ts =>
                                    {
                                        ts.Add(t => t
                                            .To<State.hold>()
                                            .IsTriggeredOn(g => g.reopenOrder)
                                            .When(conditions.IsOnHold));
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.OnLinesReopened));
                                        ts.Add(t => t
                                            .To<State.closed>()
                                            .IsTriggeredOn(g => g.OnLinesClosed));
                                        ts.Add(t => t
                                            .To<State.pendingPrint>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsPrinted));
                                        ts.Add(t => t
                                            .To<State.pendingEmail>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsEmailed));
                                    });
                                    transitions.AddGroupFrom<State.closed>(ts =>
                                    {
                                        ts.Add(t => t
                                            .To<State.completed>()
                                            .IsTriggeredOn(g => g.OnLinesCompleted));
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.OnLinesReopened));
                                        ts.Add(t => t
                                            .To<State.hold>()
                                            .IsTriggeredOn(g => g.reopenOrder)
                                            .When(conditions.IsOnHold));
                                        ts.Add(t => t
                                            .To<State.pendingPrint>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsPrinted));
                                        ts.Add(t => t
                                            .To<State.pendingEmail>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsEmailed));
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder));
                                    });
                                });
                        });
                        flows.Add<POOrderType.dropShip>(flow =>
                        {
                            return flow
                                .WithFlowStates(states =>
                                {
                                    states.Add(State.Initial, state => state.IsInitial(g => g.initializeState));
                                    states.Add<State.hold>(state =>
                                    {
                                        return state
                                            .WithActions(actions =>
                                            {
                                                actions.Add(g => g.releaseFromHold, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
                                                actions.Add(g => g.cancelOrder);
                                                actions.Add(g => g.validateAddresses);
                                                actions.Add(g => g.recalculateDiscountsAction);

                                                actions.Add(g => g.vendorDetails);
                                                actions.Add(g => g.printPurchaseOrder);
                                                actions.Add(g => g.viewPurchaseOrderReceipt);

                                                actions.Add(g => g.addPOOrder);
                                                actions.Add(g => g.addPOOrderLine);
                                                actions.Add(g => g.addInvBySite);
                                                actions.Add(g => g.addInvSelBySite);

                                                actions.Add<DropShipLinksExt>(g => g.unlinkFromSO);
                                                actions.Add<DropShipLinksExt>(g => g.convertToNormal);
                                                actions.Add<PurchaseToSOLinksExt>(g => g.viewDemand);
                                            })
                                            .WithFieldStates(fields =>
                                            {
                                                fields.AddAllFields<POOrder>();
                                                fields.AddField<POOrder.approved>(c => c.IsDisabled());
                                                fields.AddField<POOrder.ownerID>();
                                                fields.AddField<POOrder.workgroupID>(c => c.IsDisabled());

                                                fields.AddTable<POLine>();
												fields.AddFields<BlanketOrderLineFields>(c => c.IsHidden());
												fields.AddTable<POTaxTran>();
												fields.AddTable<POOrderDiscountDetail>();
                                                fields.AddAllFields<POShipAddress>();
                                                fields.AddAllFields<POShipContact>();
                                                
                                            });
                                    });
                                    states.Add<State.pendingPrint>(state =>
                                    {
                                        return state
                                            .WithActions(actions =>
                                            {
                                                actions.Add(g => g.putOnHold);
                                                actions.Add(g => g.markAsDontPrint, c => c.IsDuplicatedInToolbar());
                                                actions.Add(g => g.cancelOrder);
                                                actions.Add(g => g.validateAddresses);

                                                actions.Add(g => g.printPurchaseOrder, c => c.IsDuplicatedInToolbar());
                                                actions.Add(g => g.vendorDetails);

                                                actions.Add<DropShipLinksExt>(g => g.unlinkFromSO);
                                                actions.Add<PurchaseToSOLinksExt>(g => g.viewDemand);
                                            })
                                            .WithFieldStates(fields =>
                                            {
                                                fields.AddAllFields<POOrder>(c => c.IsDisabled());
                                                fields.AddField<POOrder.orderType>();
                                                fields.AddField<POOrder.orderNbr>();
                                                fields.AddField<POOrder.controlTotal>();
                                                fields.AddField<POOrder.dontPrint>();

                                                fields.AddTable<POLine>(c => c.IsDisabled());
                                                fields.AddField<POLine.cancelled>();
                                                fields.AddField<POLine.completed>();
                                                fields.AddField<POLine.promisedDate>();
                                                fields.AddField<POLine.closed>();
												fields.AddFields<BlanketOrderLineFields>(c => c.IsHidden());

												fields.AddTable<POTaxTran>(c => c.IsDisabled());
												fields.AddTable<POOrderDiscountDetail>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipContact>(c => c.IsDisabled());
                                                fields.AddTable<CurrencyInfo>(c => c.IsDisabled());
                                            })
                                            .WithEventHandlers(handlers =>
                                            {
                                                handlers.Add(g => g.OnLinesCompleted);
                                                handlers.Add(g => g.OnLinesClosed);
                                                handlers.Add(g => g.OnPrinted);
                                                handlers.Add(g => g.OnDoNotPrintChecked);
                                            });
                                    });
                                    states.Add<State.pendingEmail>(state =>
                                    {
                                        return state
                                            .WithActions(actions =>
                                            {
                                                actions.Add(g => g.putOnHold);
                                                actions.Add(g => g.cancelOrder);
                                                actions.Add(g => g.validateAddresses);
                                                actions.Add(g => g.emailPurchaseOrder, c => c.IsDuplicatedInToolbar());
                                                actions.Add(g => g.markAsDontEmail, c => c.IsDuplicatedInToolbar());

                                                actions.Add(g => g.printPurchaseOrder);
                                                actions.Add(g => g.vendorDetails);

                                                actions.Add<DropShipLinksExt>(g => g.unlinkFromSO);
                                                actions.Add<PurchaseToSOLinksExt>(g => g.viewDemand);
                                            })
                                            .WithFieldStates(fields =>
                                            {
                                                fields.AddAllFields<POOrder>(c => c.IsDisabled());
                                                fields.AddField<POOrder.orderType>();
                                                fields.AddField<POOrder.orderNbr>();
                                                fields.AddField<POOrder.controlTotal>();
                                                fields.AddField<POOrder.dontEmail>();

                                                fields.AddTable<POLine>(c => c.IsDisabled());
                                                fields.AddField<POLine.cancelled>();
                                                fields.AddField<POLine.completed>();
                                                fields.AddField<POLine.promisedDate>();
                                                fields.AddField<POLine.closed>();
												fields.AddFields<BlanketOrderLineFields>(c => c.IsHidden());

												fields.AddTable<POTaxTran>(c => c.IsDisabled());
												fields.AddTable<POOrderDiscountDetail>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipContact>(c => c.IsDisabled());
                                                fields.AddTable<CurrencyInfo>(c => c.IsDisabled());
                                            })
                                            .WithEventHandlers(handlers =>
                                            {
                                                handlers.Add(g => g.OnLinesCompleted);
                                                handlers.Add(g => g.OnLinesClosed);
                                                handlers.Add(g => g.OnDoNotEmailChecked);
                                            });
                                    });
                                    states.Add<State.awaitingLink>(state =>
                                    {
                                        return state
                                            .WithActions(actions =>
                                            {
                                                actions.Add(g => g.putOnHold);

                                                actions.Add(g => g.complete);
                                                actions.Add(g => g.cancelOrder);

                                                actions.Add(g => g.emailPurchaseOrder);
                                                actions.Add(g => g.createAPInvoice);

                                                actions.Add(g => g.validateAddresses);
                                                actions.Add<Prepayments>(g => g.createPrepayment);
                                                actions.Add<DropShipLinksExt>(g => g.unlinkFromSO);
                                                actions.Add<DropShipLinksExt>(g => g.convertToNormal);
                                                actions.Add<DropShipLinksExt>(g => g.createSalesOrder);

                                                actions.Add(g => g.vendorDetails);
                                                actions.Add(g => g.printPurchaseOrder);
                                                actions.Add(g => g.viewPurchaseOrderReceipt);
                                            })
                                            .WithFieldStates(fields =>
                                            {
                                                fields.AddAllFields<POOrder>(c => c.IsDisabled());
                                                fields.AddField<POOrder.orderType>();
                                                fields.AddField<POOrder.orderNbr>();
                                                fields.AddField<POOrder.controlTotal>();

                                                fields.AddField<POOrder.ownerID>(c => c.IsHidden());
                                                fields.AddField<POOrder.workgroupID>(c => c.IsHidden());

                                                fields.AddTable<POLine>(c => c.IsDisabled());
                                                fields.AddField<POLine.cancelled>();
                                                fields.AddField<POLine.completed>();
                                                fields.AddField<POLine.promisedDate>();
                                                fields.AddField<POLine.closed>();
												fields.AddFields<BlanketOrderLineFields>(c => c.IsHidden());

												fields.AddTable<POTaxTran>(c => c.IsDisabled());
												fields.AddTable<POOrderDiscountDetail>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipContact>(c => c.IsDisabled());
                                                fields.AddTable<CurrencyInfo>(c => c.IsDisabled());
                                            })
                                            .WithEventHandlers(handlers =>
                                            {
                                                handlers.Add(g => g.OnLinesCompleted);
                                                handlers.Add(g => g.OnLinesClosed);
                                                handlers.Add(g => g.OnLinesLinked);
                                            });
                                    });
                                    states.Add<State.open>(state =>
                                    {
                                        return state
                                            .WithActions(actions =>
                                            {
                                                actions.Add(g => g.putOnHold);

                                                actions.Add(g => g.complete);
                                                actions.Add(g => g.cancelOrder);

                                                actions.Add(g => g.emailPurchaseOrder);
                                                actions.Add(g => g.createPOReceipt, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
                                                actions.Add(g => g.createAPInvoice);

                                                actions.Add(g => g.validateAddresses);
                                                actions.Add<Prepayments>(g => g.createPrepayment);

                                                actions.Add(g => g.vendorDetails);
                                                actions.Add(g => g.printPurchaseOrder);
                                                actions.Add(g => g.viewPurchaseOrderReceipt);

                                                actions.Add<DropShipLinksExt>(g => g.unlinkFromSO);
                                                actions.Add<DropShipLinksExt>(g => g.convertToNormal);
                                                actions.Add<DropShipLinksExt>(g => g.createSalesOrder);
                                                actions.Add<PurchaseToSOLinksExt>(g => g.viewDemand);
                                            })
                                            .WithFieldStates(fields =>
                                            {
                                                fields.AddAllFields<POOrder>(c => c.IsDisabled());
                                                fields.AddField<POOrder.orderType>();
                                                fields.AddField<POOrder.orderNbr>();
                                                fields.AddField<POOrder.controlTotal>();

                                                fields.AddField<POOrder.ownerID>(c => c.IsHidden());
                                                fields.AddField<POOrder.workgroupID>(c => c.IsHidden());

                                                fields.AddTable<POLine>(c => c.IsDisabled());
                                                fields.AddField<POLine.cancelled>();
                                                fields.AddField<POLine.completed>();
                                                fields.AddField<POLine.promisedDate>();
                                                fields.AddField<POLine.closed>();
												fields.AddFields<BlanketOrderLineFields>(c => c.IsHidden());

												fields.AddTable<POTaxTran>(c => c.IsDisabled());
												fields.AddTable<POOrderDiscountDetail>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipContact>(c => c.IsDisabled());
                                                fields.AddTable<CurrencyInfo>(c => c.IsDisabled());
                                            })
                                            .WithEventHandlers(handlers =>
                                            {
                                                handlers.Add(g => g.OnLinesCompleted);
                                                handlers.Add(g => g.OnLinesClosed);
                                                handlers.Add(g => g.OnLinesUnlinked);
                                                handlers.Add(g => g.OnReleaseChangeOrder);
                                            });
                                    });
                                    states.Add<State.completed>(state =>
                                    {
                                        return state
                                            .WithActions(actions =>
                                            {
                                                actions.Add(g => g.reopenOrder);
                                                actions.Add(g => g.createAPInvoice);
                                                actions.Add<Prepayments>(g => g.createPrepayment);
                                                actions.Add<DropShipLinksExt>(g => g.createSalesOrder);
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
												fields.AddFields<BlanketOrderLineFields>(c => c.IsHidden());

												fields.AddTable<POTaxTran>(c => c.IsDisabled());
												fields.AddTable<POOrderDiscountDetail>(c => c.IsDisabled());
                                                fields.AddAllFields<PORemitAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<PORemitContact>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipContact>(c => c.IsDisabled());

                                                fields.AddTable<CurrencyInfo>(c => c.IsDisabled());
                                            })
                                            .WithEventHandlers(handlers =>
                                            {
                                                handlers.Add(g => g.OnLinesClosed);
                                                handlers.Add(g => g.OnLinesReopened);
                                                handlers.Add(g => g.OnReleaseChangeOrder);
                                            });
                                    });
                                    states.Add<State.cancelled>(state =>
                                    {
                                        return state
                                            .WithActions(actions =>
                                            {
                                                actions.Add(g => g.reopenOrder);
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
												fields.AddFields<BlanketOrderLineFields>(c => c.IsHidden());

												fields.AddAllFields<PORemitAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<PORemitContact>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipContact>(c => c.IsDisabled());
                                                fields.AddTable<POTaxTran>(c => c.IsDisabled());
												fields.AddTable<POOrderDiscountDetail>(c => c.IsDisabled());
                                                fields.AddTable<CurrencyInfo>(c => c.IsDisabled());
                                            })
                                            .WithEventHandlers(handlers =>
                                            {
                                                handlers.Add(g => g.OnLinesReopened);
                                                handlers.Add(g => g.OnReleaseChangeOrder);
                                            }); ;
                                    });
                                    states.Add<State.closed>(state =>
                                    {
                                        return state
                                            .WithActions(actions =>
                                            {
                                                actions.Add(g => g.reopenOrder);
                                                actions.Add(g => g.validateAddresses);

                                                actions.Add(g => g.vendorDetails);
                                                actions.Add(g => g.printPurchaseOrder);
                                                actions.Add(g => g.viewPurchaseOrderReceipt);

                                                actions.Add<DropShipLinksExt>(g => g.createSalesOrder);
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
												fields.AddFields<BlanketOrderLineFields>(c => c.IsHidden());

												fields.AddAllFields<PORemitAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<PORemitContact>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipContact>(c => c.IsDisabled());
                                                fields.AddTable<POTaxTran>(c => c.IsDisabled());
												fields.AddTable<POOrderDiscountDetail>(c => c.IsDisabled());
                                                fields.AddTable<CurrencyInfo>(c => c.IsDisabled());
                                            })
                                            .WithEventHandlers(handlers =>
                                            {
                                                handlers.Add(g => g.OnLinesReopened);
                                                handlers.Add(g => g.OnLinesCompleted);
                                                handlers.Add(g => g.OnReleaseChangeOrder);
                                            });
                                    });
                                })
                                .WithTransitions(transitions =>
                                {
                                    transitions.AddGroupFrom(State.Initial, ts =>
                                    {
                                        ts.Add(t => t
                                            .To<State.hold>()
                                            .IsTriggeredOn(g => g.initializeState)
                                            .When(conditions.IsOnHold));
                                        ts.Add(t => t
                                            .To<State.pendingPrint>()
                                            .IsTriggeredOn(g => g.initializeState)
                                            .When(!conditions.IsPrinted));
                                        ts.Add(t => t
                                            .To<State.pendingEmail>()
                                            .IsTriggeredOn(g => g.initializeState)
                                            .When(!conditions.IsEmailed));
                                        ts.Add(t => t
                                            .To<State.cancelled>()
                                            .IsTriggeredOn(g => g.initializeState)
                                            .When(conditions.IsCancelled));
                                        ts.Add(t => t
                                            .To<State.closed>()
                                            .IsTriggeredOn(g => g.initializeState)
                                            .When(conditions.HasAllLinesClosed));
                                        ts.Add(t => t
                                            .To<State.completed>()
                                            .IsTriggeredOn(g => g.initializeState)
                                            .When(conditions.HasAllLinesCompleted));
                                        ts.Add(t => t
                                            .To<State.awaitingLink>()
                                            .IsTriggeredOn(g => g.initializeState)
                                            .When(!conditions.HasAllDropShipLinesLinked && conditions.IsNewDropShipOrder));
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.initializeState));
                                    });
                                    transitions.AddGroupFrom<State.hold>(ts =>
                                    {
                                        ts.Add(t => t
                                            .To<State.pendingPrint>()
                                            .IsTriggeredOn(g => g.releaseFromHold)
                                            .When(!conditions.IsOnHold && !conditions.IsPrinted));
                                        ts.Add(t => t
                                            .To<State.pendingEmail>()
                                            .IsTriggeredOn(g => g.releaseFromHold)
                                            .When(!conditions.IsOnHold && !conditions.IsEmailed));
                                        ts.Add(t => t
                                            .To<State.closed>()
                                            .IsTriggeredOn(g => g.releaseFromHold)
                                            .When(!conditions.IsOnHold && conditions.HasAllLinesClosed));
                                        ts.Add(t => t
                                            .To<State.completed>()
                                            .IsTriggeredOn(g => g.releaseFromHold)
                                            .When(!conditions.IsOnHold && conditions.HasAllLinesCompleted));
                                        ts.Add(t => t
                                            .To<State.awaitingLink>()
                                            .IsTriggeredOn(g => g.releaseFromHold)
                                            .When(!conditions.HasAllDropShipLinesLinked && conditions.IsNewDropShipOrder));
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.releaseFromHold)
                                            .When(!conditions.IsOnHold));
                                        ts.Add(t => t
                                            .To<State.cancelled>()
                                            .IsTriggeredOn(g => g.cancelOrder)
                                            .When(conditions.IsCancelled));
                                    });
                                    transitions.AddGroupFrom<State.pendingPrint>(ts =>
                                    {
                                        ts.Add(t => t
                                            .To<State.hold>()
                                            .IsTriggeredOn(g => g.putOnHold)
                                            .When(conditions.IsOnHold));
                                        ts.Add(t => t
                                            .To<State.cancelled>()
                                            .IsTriggeredOn(g => g.cancelOrder)
                                            .When(conditions.IsCancelled));
                                        ts.Add(t => t
                                            .To<State.pendingEmail>()
                                            .IsTriggeredOn(g => g.OnPrinted)
                                            .When(!conditions.IsEmailed)
                                            .WithFieldAssignments(fas => fas.Add<printed>(true)));
                                        ts.Add(t => t
                                            .To<State.closed>()
                                            .IsTriggeredOn(g => g.OnPrinted)
                                            .When(conditions.HasAllLinesClosed)
                                            .WithFieldAssignments(fas => fas.Add<printed>(true)));
                                        ts.Add(t => t
                                            .To<State.completed>()
                                            .IsTriggeredOn(g => g.OnPrinted)
                                            .When(conditions.HasAllLinesCompleted)
                                            .WithFieldAssignments(fas => fas.Add<printed>(true)));
                                        ts.Add(t => t
                                            .To<State.awaitingLink>()
                                            .IsTriggeredOn(g => g.OnPrinted)
                                            .When(!conditions.HasAllDropShipLinesLinked && conditions.IsNewDropShipOrder)
                                            .WithFieldAssignments(fas => fas.Add<printed>(true)));
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.OnPrinted)
                                            .WithFieldAssignments(fas => fas.Add<printed>(true)));
                                        ts.Add(t => t
                                            .To<State.pendingEmail>()
                                            .IsTriggeredOn(g => g.OnDoNotPrintChecked)
                                            .When(!conditions.IsEmailed)
                                            .DoesNotPersist());
                                        ts.Add(t => t
                                           .To<State.closed>()
                                           .IsTriggeredOn(g => g.OnDoNotPrintChecked)
                                           .When(conditions.HasAllLinesClosed)
                                            .DoesNotPersist());
                                        ts.Add(t => t
                                            .To<State.completed>()
                                            .IsTriggeredOn(g => g.OnDoNotPrintChecked)
                                            .When(conditions.HasAllLinesCompleted)
                                            .DoesNotPersist());
                                        ts.Add(t => t
                                            .To<State.awaitingLink>()
                                            .IsTriggeredOn(g => g.OnDoNotPrintChecked)
                                            .When(!conditions.HasAllDropShipLinesLinked && conditions.IsNewDropShipOrder)
                                            .DoesNotPersist());
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.OnDoNotPrintChecked)
                                            .DoesNotPersist());
                                    });
                                    transitions.AddGroupFrom<State.pendingEmail>(ts =>
                                    {
                                        ts.Add(t => t
                                            .To<State.hold>()
                                            .IsTriggeredOn(g => g.putOnHold)
                                            .When(conditions.IsOnHold));
                                        ts.Add(t => t
                                            .To<State.cancelled>()
                                            .IsTriggeredOn(g => g.cancelOrder)
                                            .When(conditions.IsCancelled));
                                        ts.Add(t => t
                                            .To<State.closed>()
                                            .IsTriggeredOn(g => g.emailPurchaseOrder)
                                            .When(conditions.HasAllLinesClosed));
                                        ts.Add(t => t
                                            .To<State.completed>()
                                            .IsTriggeredOn(g => g.emailPurchaseOrder)
                                            .When(conditions.HasAllLinesCompleted));
                                        ts.Add(t => t
                                            .To<State.awaitingLink>()
                                            .IsTriggeredOn(g => g.emailPurchaseOrder)
                                            .When(!conditions.HasAllDropShipLinesLinked && conditions.IsNewDropShipOrder));
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.emailPurchaseOrder));
                                        ts.Add(t => t
                                            .To<State.closed>()
                                            .IsTriggeredOn(g => g.OnDoNotEmailChecked)
                                            .When(conditions.HasAllLinesClosed));
                                        ts.Add(t => t
                                            .To<State.completed>()
                                            .IsTriggeredOn(g => g.OnDoNotEmailChecked)
                                            .When(conditions.HasAllLinesCompleted));
                                        ts.Add(t => t
                                            .To<State.awaitingLink>()
                                            .IsTriggeredOn(g => g.OnDoNotEmailChecked)
                                            .When(!conditions.HasAllDropShipLinesLinked && conditions.IsNewDropShipOrder));
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.OnDoNotEmailChecked));
                                    });
                                    transitions.AddGroupFrom<State.awaitingLink>(ts =>
                                    {
                                        ts.Add(t => t
                                            .To<State.hold>()
                                            .IsTriggeredOn(g => g.putOnHold));
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.OnLinesLinked));
                                        ts.Add(t => t
                                            .To<State.cancelled>()
                                            .IsTriggeredOn(g => g.cancelOrder));
                                        ts.Add(t => t
                                           .To<State.completed>()
                                           .IsTriggeredOn(g => g.OnLinesCompleted)
                                           .DoesNotPersist());
                                        ts.Add(t => t
                                            .To<State.completed>()
                                            .IsTriggeredOn(g => g.complete)
                                            .When(conditions.HasAllLinesCompleted));
                                        ts.Add(t => t
                                            .To<State.closed>()
                                            .IsTriggeredOn(g => g.OnLinesClosed));
                                    });
                                    transitions.AddGroupFrom<State.open>(ts =>
                                    {
                                        ts.Add(t => t
                                            .To<State.hold>()
                                            .IsTriggeredOn(g => g.putOnHold)
                                            .When(conditions.IsOnHold));
                                        ts.Add(t => t
                                            .To<State.awaitingLink>()
                                            .IsTriggeredOn(g => g.OnLinesUnlinked)
                                            .When(!conditions.HasAllDropShipLinesLinked && conditions.IsNewDropShipOrder));
                                        ts.Add(t => t
                                            .To<State.cancelled>()
                                            .IsTriggeredOn(g => g.cancelOrder)
                                            .When(conditions.IsCancelled));
                                        ts.Add(t => t
                                           .To<State.completed>()
                                           .IsTriggeredOn(g => g.OnLinesCompleted)
                                           .DoesNotPersist());
                                        ts.Add(t => t
                                            .To<State.completed>()
                                            .IsTriggeredOn(g => g.complete)
                                            .When(conditions.HasAllLinesCompleted));
                                        ts.Add(t => t
                                            .To<State.closed>()
                                            .IsTriggeredOn(g => g.OnLinesClosed));
                                        ts.Add(t => t
                                            .To<State.pendingPrint>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsPrinted));
                                        ts.Add(t => t
                                            .To<State.pendingEmail>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsEmailed));
                                    });
                                    transitions.AddGroupFrom<State.cancelled>(ts =>
                                    {
                                        ts.Add(t => t
                                            .To<State.hold>()
                                            .IsTriggeredOn(g => g.reopenOrder)
                                            .When(conditions.IsOnHold));
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.OnLinesReopened));
                                        ts.Add(t => t
                                            .To<State.pendingPrint>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsPrinted));
                                        ts.Add(t => t
                                            .To<State.pendingEmail>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsEmailed));
                                    });
                                    transitions.AddGroupFrom<State.completed>(ts =>
                                    {
                                        ts.Add(t => t
                                            .To<State.hold>()
                                            .IsTriggeredOn(g => g.reopenOrder)
                                            .When(conditions.IsOnHold));
                                        ts.Add(t => t
                                            .To<State.awaitingLink>()
                                            .IsTriggeredOn(g => g.OnLinesReopened)
                                            .When(!conditions.HasAllDropShipLinesLinked && conditions.IsNewDropShipOrder));
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.OnLinesReopened));
                                        ts.Add(t => t
                                            .To<State.closed>()
                                            .IsTriggeredOn(g => g.OnLinesClosed));
                                        ts.Add(t => t
                                            .To<State.pendingPrint>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsPrinted));
                                        ts.Add(t => t
                                            .To<State.pendingEmail>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsEmailed));
                                    });
                                    transitions.AddGroupFrom<State.closed>(ts =>
                                    {
                                        ts.Add(t => t
                                            .To<State.completed>()
                                            .IsTriggeredOn(g => g.OnLinesCompleted));
                                        ts.Add(t => t
                                            .To<State.awaitingLink>()
                                            .IsTriggeredOn(g => g.OnLinesReopened)
                                            .When(!conditions.HasAllDropShipLinesLinked && conditions.IsNewDropShipOrder));
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.OnLinesReopened));
                                        ts.Add(t => t
                                            .To<State.hold>()
                                            .IsTriggeredOn(g => g.reopenOrder)
                                            .When(conditions.IsOnHold));
                                        ts.Add(t => t
                                            .To<State.pendingPrint>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsPrinted));
                                        ts.Add(t => t
                                            .To<State.pendingEmail>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsEmailed));
                                    });
                                });
                        });
                        flows.Add<POOrderType.projectDropShip>(flow =>
                        {
                            return flow
                                .WithFlowStates(states =>
                                {
                                    states.Add(State.Initial, state => state.IsInitial(g => g.initializeState));
                                    states.Add<State.hold>(state =>
                                    {
                                        return state
                                            .WithActions(actions =>
                                            {
                                                actions.Add(g => g.releaseFromHold, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
                                                actions.Add(g => g.validateAddresses);
                                                actions.Add(g => g.recalculateDiscountsAction);

                                                actions.Add(g => g.vendorDetails);
                                                actions.Add(g => g.printPurchaseOrder);
                                                actions.Add(g => g.viewPurchaseOrderReceipt);

												actions.Add(g => g.addInvBySite);
                                                actions.Add(g => g.addInvSelBySite);
                                            })
                                            .WithFieldStates(fields =>
                                            {
                                                fields.AddAllFields<POOrder>();
                                                fields.AddTable<POLine>();
												fields.AddFields<DropShipOrderLineFields>(x => x.IsHidden());
												fields.AddFields<BlanketOrderLineFields>(c => c.IsHidden());
												fields.AddTable<POTaxTran>();
												fields.AddTable<POOrderDiscountDetail>();
                                                fields.AddAllFields<POShipAddress>();
                                                fields.AddAllFields<POShipContact>();
                                                fields.AddField<POOrder.approved>(c => c.IsDisabled());
                                                fields.AddField<POOrder.ownerID>();
                                                fields.AddField<POOrder.workgroupID>(c => c.IsDisabled());
                                            });
                                    });
                                    states.Add<State.pendingPrint>(state =>
                                    {
                                        return state
                                            .WithActions(actions =>
                                            {
                                                actions.Add(g => g.putOnHold);
                                                actions.Add(g => g.markAsDontPrint, c => c.IsDuplicatedInToolbar());
                                                actions.Add(g => g.cancelOrder);
                                                actions.Add(g => g.validateAddresses);

                                                actions.Add(g => g.printPurchaseOrder, c => c.IsDuplicatedInToolbar());
                                                actions.Add(g => g.vendorDetails);
                                            })
                                            .WithFieldStates(fields =>
                                            {
                                                fields.AddAllFields<POOrder>(c => c.IsDisabled());
                                                fields.AddField<POOrder.orderType>();
                                                fields.AddField<POOrder.orderNbr>();
                                                fields.AddField<POOrder.controlTotal>();
                                                fields.AddField<POOrder.dontPrint>();
                                                fields.AddField<POOrder.excludeFromIntercompanyProc>();

                                                fields.AddTable<POLine>(c => c.IsDisabled());
                                                fields.AddField<POLine.cancelled>();
                                                fields.AddField<POLine.completed>();
                                                fields.AddField<POLine.promisedDate>();
                                                fields.AddField<POLine.closed>();
												fields.AddFields<DropShipOrderLineFields>(x => x.IsHidden());
												fields.AddFields<BlanketOrderLineFields>(c => c.IsHidden());

												fields.AddTable<POTaxTran>(c => c.IsDisabled());
												fields.AddTable<POOrderDiscountDetail>(c => c.IsDisabled());
												fields.AddAllFields<POShipAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipContact>(c => c.IsDisabled());
                                                fields.AddTable<CurrencyInfo>(c => c.IsDisabled());
                                            })
                                            .WithEventHandlers(handlers =>
                                            {
                                                handlers.Add(g => g.OnLinesCompleted);
                                                handlers.Add(g => g.OnLinesClosed);
                                                handlers.Add(g => g.OnPrinted);
                                                handlers.Add(g => g.OnDoNotPrintChecked);
                                            });
                                    });
                                    states.Add<State.pendingEmail>(state =>
                                    {
                                        return state
                                            .WithActions(actions =>
                                            {
                                                actions.Add(g => g.putOnHold);
                                                actions.Add(g => g.cancelOrder);
                                                actions.Add(g => g.validateAddresses);
                                                actions.Add(g => g.emailPurchaseOrder, c => c.IsDuplicatedInToolbar());
                                                actions.Add(g => g.markAsDontEmail, c => c.IsDuplicatedInToolbar());

                                                actions.Add(g => g.printPurchaseOrder);
                                                actions.Add(g => g.vendorDetails);
                                            })
                                            .WithFieldStates(fields =>
                                            {
                                                fields.AddAllFields<POOrder>(c => c.IsDisabled());
                                                fields.AddField<POOrder.orderType>();
                                                fields.AddField<POOrder.orderNbr>();
                                                fields.AddField<POOrder.controlTotal>();
                                                fields.AddField<POOrder.dontEmail>();
                                                fields.AddField<POOrder.excludeFromIntercompanyProc>();

                                                fields.AddTable<POLine>(c => c.IsDisabled());
                                                fields.AddField<POLine.cancelled>();
                                                fields.AddField<POLine.completed>();
                                                fields.AddField<POLine.promisedDate>();
                                                fields.AddField<POLine.closed>();
												fields.AddFields<DropShipOrderLineFields>(x => x.IsHidden());
												fields.AddFields<BlanketOrderLineFields>(c => c.IsHidden());

												fields.AddTable<POTaxTran>(c => c.IsDisabled());
												fields.AddTable<POOrderDiscountDetail>(c => c.IsDisabled());
												fields.AddAllFields<POShipAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipContact>(c => c.IsDisabled());
                                                fields.AddTable<CurrencyInfo>(c => c.IsDisabled());
                                            })
                                            .WithEventHandlers(handlers =>
                                            {
                                                handlers.Add(g => g.OnLinesCompleted);
                                                handlers.Add(g => g.OnLinesClosed);
                                                handlers.Add(g => g.OnDoNotEmailChecked);
                                            });
                                    });
                                    states.Add<State.open>(state =>
                                    {
                                        return state
                                            .WithActions(actions =>
                                            {
                                                actions.Add(g => g.putOnHold);

                                                actions.Add(g => g.complete);
                                                actions.Add(g => g.cancelOrder);

                                                actions.Add(g => g.emailPurchaseOrder);
                                                actions.Add(g => g.createPOReceipt, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
                                                actions.Add(g => g.createAPInvoice, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));

                                                actions.Add(g => g.validateAddresses);
                                                actions.Add<Prepayments>(g => g.createPrepayment);

                                                actions.Add(g => g.vendorDetails);
                                                actions.Add(g => g.printPurchaseOrder);
                                                actions.Add(g => g.viewPurchaseOrderReceipt);
                                            })
                                            .WithFieldStates(fields =>
                                            {
                                                fields.AddAllFields<POOrder>(c => c.IsDisabled());
                                                fields.AddField<POOrder.orderType>();
                                                fields.AddField<POOrder.orderNbr>();
                                                fields.AddField<POOrder.controlTotal>();
                                                fields.AddField<POOrder.excludeFromIntercompanyProc>();

                                                fields.AddTable<POLine>(c => c.IsDisabled());
                                                fields.AddField<POLine.cancelled>();
                                                fields.AddField<POLine.completed>();
                                                fields.AddField<POLine.promisedDate>();
                                                fields.AddField<POLine.closed>();
												fields.AddFields<DropShipOrderLineFields>(x => x.IsHidden());
												fields.AddFields<BlanketOrderLineFields>(c => c.IsHidden());

												fields.AddTable<POTaxTran>(c => c.IsDisabled());
												fields.AddTable<POOrderDiscountDetail>(c => c.IsDisabled());
												fields.AddAllFields<POShipAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipContact>(c => c.IsDisabled());
                                                fields.AddTable<CurrencyInfo>(c => c.IsDisabled());
                                            })
                                            .WithEventHandlers(handlers =>
                                            {
                                                handlers.Add(g => g.OnLinesCompleted);
                                                handlers.Add(g => g.OnLinesClosed);
                                                handlers.Add(g => g.OnReleaseChangeOrder);
                                            });
                                    });
                                    states.Add<State.completed>(state =>
                                    {
                                        return state
                                            .WithActions(actions =>
                                            {
                                                actions.Add(g => g.reopenOrder);
                                                actions.Add(g => g.validateAddresses);
                                                actions.Add(g => g.createAPInvoice, c => c.WithConnotation(ActionConnotation.Success));
                                                actions.Add<Prepayments>(g => g.createPrepayment);

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
												fields.AddFields<DropShipOrderLineFields>(x => x.IsHidden());
												fields.AddFields<BlanketOrderLineFields>(c => c.IsHidden());

												fields.AddTable<POTaxTran>(c => c.IsDisabled());
												fields.AddTable<POOrderDiscountDetail>(c => c.IsDisabled());

												fields.AddAllFields<PORemitAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<PORemitContact>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipContact>(c => c.IsDisabled());

                                                fields.AddTable<CurrencyInfo>(c => c.IsDisabled());
                                            })
                                            .WithEventHandlers(handlers =>
                                            {
                                                handlers.Add(g => g.OnLinesClosed);
                                                handlers.Add(g => g.OnLinesReopened);
                                                handlers.Add(g => g.OnReleaseChangeOrder);
                                            });
                                    });
                                    states.Add<State.cancelled>(state =>
                                    {
                                        return state
                                            .WithActions(actions =>
                                            {
                                                actions.Add(g => g.reopenOrder);
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
												fields.AddFields<DropShipOrderLineFields>(x => x.IsHidden());
												fields.AddFields<BlanketOrderLineFields>(c => c.IsHidden());

												fields.AddAllFields<PORemitAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<PORemitContact>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipContact>(c => c.IsDisabled());
												fields.AddTable<POTaxTran>(c => c.IsDisabled());
												fields.AddTable<POOrderDiscountDetail>(c => c.IsDisabled());

												fields.AddTable<CurrencyInfo>(c => c.IsDisabled());
                                            })
                                            .WithEventHandlers(handlers =>
                                            {
                                                handlers.Add(g => g.OnLinesReopened);
                                                handlers.Add(g => g.OnReleaseChangeOrder);
                                            });
                                    });
                                    states.Add<State.closed>(state =>
                                    {
                                        return state
                                            .WithActions(actions =>
                                            {
                                                actions.Add(g => g.reopenOrder);
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
												fields.AddFields<DropShipOrderLineFields>(x => x.IsHidden());
												fields.AddFields<BlanketOrderLineFields>(c => c.IsHidden());

												fields.AddAllFields<PORemitAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<PORemitContact>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipContact>(c => c.IsDisabled());
												fields.AddTable<POTaxTran>(c => c.IsDisabled());
												fields.AddTable<POOrderDiscountDetail>(c => c.IsDisabled());

												fields.AddTable<CurrencyInfo>(c => c.IsDisabled());
                                            })
                                            .WithEventHandlers(handlers =>
                                            {
                                                handlers.Add(g => g.OnLinesReopened);
                                                handlers.Add(g => g.OnLinesCompleted);
                                                handlers.Add(g => g.OnReleaseChangeOrder);
                                            });
                                    });
                                })
                                .WithTransitions(transitions =>
                                {
                                    transitions.AddGroupFrom(State.Initial, ts =>
                                    {
                                        ts.Add(t => t
                                            .To<State.hold>()
                                            .IsTriggeredOn(g => g.initializeState)
                                            .When(conditions.IsOnHold));
                                        ts.Add(t => t
                                            .To<State.pendingPrint>()
                                            .IsTriggeredOn(g => g.initializeState)
                                            .When(!conditions.IsPrinted));
                                        ts.Add(t => t
                                            .To<State.pendingEmail>()
                                            .IsTriggeredOn(g => g.initializeState)
                                            .When(!conditions.IsEmailed));
                                        ts.Add(t => t
                                            .To<State.cancelled>()
                                            .IsTriggeredOn(g => g.initializeState)
                                            .When(conditions.IsCancelled));
                                        ts.Add(t => t
                                            .To<State.closed>()
                                            .IsTriggeredOn(g => g.initializeState)
                                            .When(conditions.HasAllLinesClosed));
                                        ts.Add(t => t
                                            .To<State.completed>()
                                            .IsTriggeredOn(g => g.initializeState)
                                            .When(conditions.HasAllLinesCompleted));
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.initializeState));
                                    });
                                    transitions.AddGroupFrom<State.hold>(ts =>
                                    {
                                        ts.Add(t => t
                                            .To<State.pendingPrint>()
                                            .IsTriggeredOn(g => g.releaseFromHold)
                                            .When(!conditions.IsOnHold && !conditions.IsPrinted));
                                        ts.Add(t => t
                                            .To<State.pendingEmail>()
                                            .IsTriggeredOn(g => g.releaseFromHold)
                                            .When(!conditions.IsOnHold && !conditions.IsEmailed));
                                        ts.Add(t => t
                                            .To<State.closed>()
                                            .IsTriggeredOn(g => g.releaseFromHold)
                                            .When(!conditions.IsOnHold && conditions.HasAllLinesClosed));
                                        ts.Add(t => t
                                            .To<State.completed>()
                                            .IsTriggeredOn(g => g.releaseFromHold)
                                            .When(!conditions.IsOnHold && conditions.HasAllLinesCompleted));
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.releaseFromHold)
                                            .When(!conditions.IsOnHold));
                                    });
                                    transitions.AddGroupFrom<State.pendingPrint>(ts =>
                                    {
                                        ts.Add(t => t
                                            .To<State.hold>()
                                            .IsTriggeredOn(g => g.putOnHold)
                                            .When(conditions.IsOnHold));
                                        ts.Add(t => t
                                            .To<State.cancelled>()
                                            .IsTriggeredOn(g => g.cancelOrder)
                                            .When(conditions.IsCancelled));

                                        ts.Add(t => t
                                            .To<State.pendingEmail>()
                                            .IsTriggeredOn(g => g.OnPrinted)
                                            .When(!conditions.IsEmailed)
                                            .WithFieldAssignments(fas => fas.Add<printed>(true)));
                                        ts.Add(t => t
                                            .To<State.closed>()
                                            .IsTriggeredOn(g => g.OnPrinted)
                                            .When(conditions.HasAllLinesClosed)
                                            .WithFieldAssignments(fas => fas.Add<printed>(true)));
                                        ts.Add(t => t
                                            .To<State.completed>()
                                            .IsTriggeredOn(g => g.OnPrinted)
                                            .When(conditions.HasAllLinesCompleted)
                                            .WithFieldAssignments(fas => fas.Add<printed>(true)));
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.OnPrinted)
                                            .WithFieldAssignments(fas => fas.Add<printed>(true)));

                                        ts.Add(t => t
                                            .To<State.pendingEmail>()
                                            .IsTriggeredOn(g => g.OnDoNotPrintChecked)
                                            .When(!conditions.IsEmailed)
                                            .DoesNotPersist());
                                        ts.Add(t => t
                                           .To<State.closed>()
                                           .IsTriggeredOn(g => g.OnDoNotPrintChecked)
                                           .When(conditions.HasAllLinesClosed)
                                            .DoesNotPersist());
                                        ts.Add(t => t
                                            .To<State.completed>()
                                            .IsTriggeredOn(g => g.OnDoNotPrintChecked)
                                            .When(conditions.HasAllLinesCompleted)
                                            .DoesNotPersist());
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.OnDoNotPrintChecked)
                                            .DoesNotPersist());
                                    });
                                    transitions.AddGroupFrom<State.pendingEmail>(ts =>
                                    {
                                        ts.Add(t => t
                                            .To<State.hold>()
                                            .IsTriggeredOn(g => g.putOnHold)
                                            .When(conditions.IsOnHold));

                                        ts.Add(t => t
                                            .To<State.cancelled>()
                                            .IsTriggeredOn(g => g.cancelOrder)
                                            .When(conditions.IsCancelled));

                                        ts.Add(t => t
                                            .To<State.closed>()
                                            .IsTriggeredOn(g => g.emailPurchaseOrder)
                                            .When(conditions.HasAllLinesClosed));
                                        ts.Add(t => t
                                            .To<State.completed>()
                                            .IsTriggeredOn(g => g.emailPurchaseOrder)
                                            .When(conditions.HasAllLinesCompleted));
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.emailPurchaseOrder));

                                        ts.Add(t => t
                                            .To<State.closed>()
                                            .IsTriggeredOn(g => g.OnDoNotEmailChecked)
                                             .When(conditions.HasAllLinesClosed));
                                        ts.Add(t => t
                                            .To<State.completed>()
                                            .IsTriggeredOn(g => g.OnDoNotEmailChecked)
                                            .When(conditions.HasAllLinesCompleted));
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.OnDoNotEmailChecked));
                                    });
                                    transitions.AddGroupFrom<State.open>(ts =>
                                    {
                                        ts.Add(t => t
                                            .To<State.hold>()
                                            .IsTriggeredOn(g => g.putOnHold)
                                            .When(conditions.IsOnHold));
                                        ts.Add(t => t
                                            .To<State.cancelled>()
                                            .IsTriggeredOn(g => g.cancelOrder)
                                            .When(conditions.IsCancelled));
                                        ts.Add(t => t
                                            .To<State.completed>()
                                            .IsTriggeredOn(g => g.OnLinesCompleted)
                                            .DoesNotPersist());
                                        ts.Add(t => t
                                            .To<State.completed>()
                                            .IsTriggeredOn(g => g.complete)
                                            .When(conditions.HasAllLinesCompleted));
                                        ts.Add(t => t
                                            .To<State.closed>()
                                            .IsTriggeredOn(g => g.OnLinesClosed));
                                        ts.Add(t => t
                                            .To<State.pendingPrint>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsPrinted));
                                        ts.Add(t => t
                                            .To<State.pendingEmail>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsEmailed));
                                    });
                                    transitions.AddGroupFrom<State.cancelled>(ts =>
                                    {
                                        ts.Add(t => t
                                            .To<State.hold>()
                                            .IsTriggeredOn(g => g.reopenOrder)
                                            .When(conditions.IsOnHold));
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.OnLinesReopened));
                                        ts.Add(t => t
                                            .To<State.pendingPrint>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsPrinted));
                                        ts.Add(t => t
                                            .To<State.pendingEmail>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsEmailed));
                                    });
                                    transitions.AddGroupFrom<State.completed>(ts =>
                                    {
                                        ts.Add(t => t
                                            .To<State.hold>()
                                            .IsTriggeredOn(g => g.reopenOrder)
                                            .When(conditions.IsOnHold));
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.OnLinesReopened));
                                        ts.Add(t => t
                                            .To<State.closed>()
                                            .IsTriggeredOn(g => g.OnLinesClosed));
                                        ts.Add(t => t
                                            .To<State.pendingPrint>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsPrinted));
                                        ts.Add(t => t
                                            .To<State.pendingEmail>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsEmailed));
                                    });
                                    transitions.AddGroupFrom<State.closed>(ts =>
                                    {
                                        ts.Add(t => t
                                            .To<State.completed>()
                                            .IsTriggeredOn(g => g.OnLinesCompleted));
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.OnLinesReopened));
                                        ts.Add(t => t
                                            .To<State.hold>()
                                            .IsTriggeredOn(g => g.reopenOrder)
                                            .When(conditions.IsOnHold));
                                        ts.Add(t => t
                                            .To<State.pendingPrint>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsPrinted));
                                        ts.Add(t => t
                                            .To<State.pendingEmail>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsEmailed));
                                    });
                                });
                        });
                        flows.Add<POOrderType.standardBlanket>(flow =>
                        {
                            return flow
                                .WithFlowStates(states =>
                                {
                                    states.Add(State.Initial, state => state.IsInitial(g => g.initializeState));
                                    states.Add<State.hold>(state =>
                                    {
                                        return state
                                            .WithActions(actions =>
                                            {
                                                actions.Add(g => g.releaseFromHold, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
                                                actions.Add(g => g.validateAddresses);
                                                actions.Add(g => g.recalculateDiscountsAction);

                                                actions.Add(g => g.vendorDetails);
                                                actions.Add(g => g.printPurchaseOrder);
                                                actions.Add(g => g.viewPurchaseOrderReceipt);

                                                actions.Add(g => g.addInvBySite);
                                                actions.Add(g => g.addInvSelBySite);
                                            })
                                            .WithFieldStates(fields =>
                                            {
                                                fields.AddTable<POLine>();
                                                fields.AddField<POLine.completed>(x => x.IsDisabled().IsHidden());
                                                fields.AddField<POLine.closed>(x => x.IsDisabled().IsHidden());
												fields.AddFields<DropShipOrderLineFields>(x => x.IsHidden());
												fields.AddFields<BlanketOrderLineFields>(c => c.IsHidden());

												fields.AddTable<POTaxTran>();
												fields.AddTable<POOrderDiscountDetail>();
                                                fields.AddAllFields<POShipAddress>();
                                                fields.AddAllFields<POShipContact>();
                                                fields.AddTable<POOrderPrepayment>(c => c.IsHidden());

                                                fields.AddAllFields<POOrder>();
                                                fields.AddField<POOrder.approved>(c => c.IsHidden());
                                                fields.AddField<POOrder.printed>(c => c.IsHidden());
                                                fields.AddField<POOrder.emailed>(c => c.IsHidden());
                                                fields.AddField<POOrder.dontPrint>(c => c.IsHidden());
                                                fields.AddField<POOrder.dontEmail>(c => c.IsHidden());
                                                fields.AddField<POOrder.ownerID>(c => c.IsDisabled().IsHidden());
                                                fields.AddField<POOrder.workgroupID>(c => c.IsDisabled().IsHidden());
                                            });

                                    });
                                    states.Add<State.open>(state =>
                                    {
                                        return state
                                            .WithActions(actions =>
                                            {
                                                actions.Add(g => g.putOnHold);
                                                actions.Add(g => g.cancelOrder);

                                                actions.Add(g => g.emailPurchaseOrder, c => c.IsDuplicatedInToolbar());
                                                actions.Add(g => g.validateAddresses);

                                                actions.Add(g => g.vendorDetails);
                                                actions.Add(g => g.printPurchaseOrder);
                                                actions.Add(g => g.viewPurchaseOrderReceipt);
                                            })
                                            .WithFieldStates(fields =>
                                            {
                                                fields.AddAllFields<POOrder>(c => c.IsDisabled());
                                                fields.AddField<POOrder.orderType>();
                                                fields.AddField<POOrder.orderNbr>();
                                                fields.AddField<POOrder.controlTotal>();
                                                fields.AddField<POOrder.approved>(c => c.IsHidden());
                                                fields.AddField<POOrder.printed>(c => c.IsHidden());
                                                fields.AddField<POOrder.emailed>(c => c.IsHidden());
                                                fields.AddField<POOrder.dontPrint>(c => c.IsHidden());
                                                fields.AddField<POOrder.dontEmail>(c => c.IsHidden());
                                                fields.AddField<POOrder.ownerID>(c => c.IsHidden());
                                                fields.AddField<POOrder.workgroupID>(c => c.IsHidden());

                                                fields.AddTable<POLine>(c => c.IsDisabled());
                                                fields.AddField<POLine.cancelled>();
                                                fields.AddField<POLine.promisedDate>();
                                                fields.AddField<POLine.completed>(x => x.IsDisabled().IsHidden());
                                                fields.AddField<POLine.closed>(x => x.IsDisabled().IsHidden());
												fields.AddFields<DropShipOrderLineFields>(x => x.IsHidden());
												fields.AddFields<BlanketOrderLineFields>(c => c.IsHidden());

												fields.AddTable<POTaxTran>(c => c.IsDisabled());
												fields.AddTable<POOrderDiscountDetail>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipContact>(c => c.IsDisabled());
                                                fields.AddTable<CurrencyInfo>(c => c.IsDisabled());
                                                fields.AddTable<POOrderPrepayment>(c => c.IsHidden());
                                            })
                                            .WithEventHandlers(handlers =>
                                            {
                                                handlers.Add(g => g.OnLinesCompleted);
                                                handlers.Add(g => g.OnLinesClosed);
                                                handlers.Add(g => g.OnReleaseChangeOrder);
                                            });
                                    });
                                    states.Add<State.cancelled>(state =>
                                    {
                                        return state
                                            .WithActions(actions =>
                                            {
                                                actions.Add(g => g.reopenOrder);
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
                                                fields.AddField<POOrder.approved>(c => c.IsHidden());
                                                fields.AddField<POOrder.printed>(c => c.IsHidden());
                                                fields.AddField<POOrder.emailed>(c => c.IsHidden());
                                                fields.AddField<POOrder.dontPrint>(c => c.IsHidden());
                                                fields.AddField<POOrder.dontEmail>(c => c.IsHidden());
                                                fields.AddField<POOrder.ownerID>(c => c.IsHidden());
                                                fields.AddField<POOrder.workgroupID>(c => c.IsHidden());

                                                fields.AddTable<POLine>(c => c.IsDisabled());
                                                fields.AddField<POLine.cancelled>();
                                                fields.AddField<POLine.promisedDate>();
                                                fields.AddField<POLine.completed>(x => x.IsDisabled().IsHidden());
                                                fields.AddField<POLine.closed>(x => x.IsDisabled().IsHidden());
												fields.AddFields<DropShipOrderLineFields>(x => x.IsHidden());
												fields.AddFields<BlanketOrderLineFields>(c => c.IsHidden());

												fields.AddAllFields<PORemitAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<PORemitContact>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipContact>(c => c.IsDisabled());
                                                fields.AddTable<POTaxTran>(c => c.IsDisabled());
												fields.AddTable<POOrderDiscountDetail>(c => c.IsDisabled());
                                                fields.AddTable<CurrencyInfo>(c => c.IsDisabled());
                                                fields.AddTable<POOrderPrepayment>(c => c.IsHidden());
                                            })
                                            .WithEventHandlers(handlers =>
                                            {
                                                handlers.Add(g => g.OnLinesReopened);
                                                handlers.Add(g => g.OnReleaseChangeOrder);
                                            });
                                    });
                                    states.Add<State.pendingPrint>();
                                    states.Add<State.pendingEmail>();
                                })
                                .WithTransitions(transitions =>
                                {
                                    transitions.AddGroupFrom(State.Initial, ts =>
                                    {
                                        ts.Add(t => t
                                            .To<State.hold>()
                                            .IsTriggeredOn(g => g.initializeState)
                                            .When(conditions.IsOnHold));
                                        ts.Add(t => t
                                            .To<State.cancelled>()
                                            .IsTriggeredOn(g => g.initializeState)
                                            .When(conditions.IsCancelled));
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.initializeState));
                                    });
                                    transitions.AddGroupFrom<State.hold>(ts =>
                                    {
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.releaseFromHold)
                                            .When(!conditions.IsOnHold));
                                    });
                                    transitions.AddGroupFrom<State.open>(ts =>
                                    {
                                        ts.Add(t => t
                                            .To<State.hold>()
                                            .IsTriggeredOn(g => g.putOnHold)
                                            .When(conditions.IsOnHold));
                                        ts.Add(t => t
                                            .To<State.cancelled>()
                                            .IsTriggeredOn(g => g.cancelOrder)
                                            .When(conditions.IsCancelled));
                                        ts.Add(t => t
                                            .To<State.pendingPrint>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsPrinted));
                                        ts.Add(t => t
                                            .To<State.pendingEmail>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsEmailed));
                                    });
                                    transitions.AddGroupFrom<State.cancelled>(ts =>
                                    {
                                        ts.Add(t => t
                                            .To<State.hold>()
                                            .IsTriggeredOn(g => g.reopenOrder)
                                            .When(!conditions.IsOnHold));
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.OnLinesReopened));
                                        ts.Add(t => t
                                            .To<State.pendingPrint>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsPrinted));
                                        ts.Add(t => t
                                            .To<State.pendingEmail>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsEmailed));
                                    });
                                });
                        });
                        flows.Add<POOrderType.blanket>(flow =>
                        {
                            return flow
                                .WithFlowStates(states =>
                                {
                                    states.Add(State.Initial, state => state.IsInitial(g => g.initializeState));
                                    states.Add<State.hold>(state =>
                                    {
                                        return state
                                            .WithActions(actions =>
                                            {
                                                actions.Add(g => g.releaseFromHold, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
                                                actions.Add(g => g.validateAddresses);
                                                actions.Add(g => g.recalculateDiscountsAction);

                                                actions.Add(g => g.vendorDetails);
                                                actions.Add(g => g.printPurchaseOrder);

                                                actions.Add(g => g.addInvBySite);
                                                actions.Add(g => g.addInvSelBySite);
                                            })
                                            .WithFieldStates(fields =>
                                            {
                                                fields.AddAllFields<POOrder>();
                                                fields.AddTable<POLine>();
												fields.AddFields<DropShipOrderLineFields>(x => x.IsHidden());
												fields.AddFields<BlanketOrderLineFields>(x => x.IsDisabled());
												fields.AddTable<POTaxTran>();
												fields.AddTable<POOrderDiscountDetail>();
                                                fields.AddAllFields<POShipAddress>();
                                                fields.AddAllFields<POShipContact>();
                                                fields.AddField<POOrder.approved>(c => c.IsDisabled());
                                                fields.AddField<POOrder.ownerID>();
                                                fields.AddField<POOrder.workgroupID>(c => c.IsDisabled());
                                                fields.AddField<POOrder.printed>(c => c.IsHidden());
                                                fields.AddField<POOrder.emailed>(c => c.IsHidden());
                                                fields.AddField<POOrder.dontPrint>(c => c.IsHidden());
                                                fields.AddField<POOrder.dontEmail>(c => c.IsHidden());
                                            });
                                    });
                                    states.Add<State.open>(state =>
                                    {
                                        return state
                                            .WithActions(actions =>
                                            {
                                                actions.Add(g => g.putOnHold);

                                                actions.Add(g => g.complete);
                                                actions.Add(g => g.cancelOrder);
                                                actions.Add(g => g.emailPurchaseOrder);

                                                actions.Add(g => g.vendorDetails);
                                                actions.Add(g => g.printPurchaseOrder);
                                            })
                                            .WithFieldStates(fields =>
                                            {
                                                fields.AddAllFields<POOrder>(c => c.IsDisabled());
                                                fields.AddField<POOrder.orderType>();
                                                fields.AddField<POOrder.orderNbr>();
                                                fields.AddField<POOrder.controlTotal>();
                                                fields.AddField<POOrder.approved>(c => c.IsDisabled());
                                                fields.AddField<POOrder.printed>(c => c.IsHidden());
                                                fields.AddField<POOrder.emailed>(c => c.IsHidden());
                                                fields.AddField<POOrder.dontPrint>(c => c.IsHidden());
                                                fields.AddField<POOrder.dontEmail>(c => c.IsHidden());

                                                fields.AddTable<POLine>(c => c.IsDisabled());
                                                fields.AddField<POLine.cancelled>();
                                                fields.AddField<POLine.completed>();
                                                fields.AddField<POLine.promisedDate>();
                                                fields.AddField<POLine.closed>();
												fields.AddFields<DropShipOrderLineFields>(x => x.IsHidden());

												fields.AddTable<POTaxTran>(c => c.IsDisabled());
												fields.AddTable<POOrderDiscountDetail>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipContact>(c => c.IsDisabled());
                                                fields.AddTable<CurrencyInfo>(c => c.IsDisabled());
                                            })
                                            .WithEventHandlers(handlers =>
                                            {
                                                handlers.Add(g => g.OnLinesCompleted);
                                                handlers.Add(g => g.OnLinesClosed);
                                                handlers.Add(g => g.OnReleaseChangeOrder);
                                            });
                                    });
                                    states.Add<State.completed>(state =>
                                    {
                                        return state
                                            .WithActions(actions =>
                                            {
                                                actions.Add(g => g.reopenOrder);

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

                                                fields.AddTable<POLine>(c => c.IsDisabled());
                                                fields.AddField<POLine.cancelled>();
                                                fields.AddField<POLine.completed>();
                                                fields.AddField<POLine.promisedDate>();
                                                fields.AddField<POLine.closed>();
												fields.AddFields<DropShipOrderLineFields>(x => x.IsHidden());

												fields.AddAllFields<PORemitAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<PORemitContact>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipContact>(c => c.IsDisabled());
                                                fields.AddTable<POTaxTran>(c => c.IsDisabled());
												fields.AddTable<POOrderDiscountDetail>(c => c.IsDisabled());
                                                fields.AddTable<CurrencyInfo>(c => c.IsDisabled());
                                            })
                                            .WithEventHandlers(handlers =>
                                            {
                                                handlers.Add(g => g.OnLinesClosed);
                                                handlers.Add(g => g.OnLinesReopened);
                                                handlers.Add(g => g.OnReleaseChangeOrder);
                                            });
                                    });
                                    states.Add<State.cancelled>(state =>
                                    {
                                        return state
                                            .WithActions(actions =>
                                            {
                                                actions.Add(g => g.reopenOrder);

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

                                                fields.AddTable<POLine>(c => c.IsDisabled());
                                                fields.AddField<POLine.cancelled>();
                                                fields.AddField<POLine.completed>();
                                                fields.AddField<POLine.promisedDate>();
                                                fields.AddField<POLine.closed>();
												fields.AddFields<DropShipOrderLineFields>(x => x.IsHidden());

												fields.AddAllFields<PORemitAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<PORemitContact>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipContact>(c => c.IsDisabled());
                                                fields.AddTable<POTaxTran>(c => c.IsDisabled());
												fields.AddTable<POOrderDiscountDetail>(c => c.IsDisabled());
                                                fields.AddTable<CurrencyInfo>(c => c.IsDisabled());
                                            })
                                            .WithEventHandlers(handlers =>
                                            {
                                                handlers.Add(g => g.OnLinesReopened);
                                                handlers.Add(g => g.OnReleaseChangeOrder);
                                            });
                                    });
                                    states.Add<State.closed>(state =>
                                    {
                                        return state
                                            .WithActions(actions =>
                                            {
                                                actions.Add(g => g.reopenOrder, c => c.IsDuplicatedInToolbar());

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

                                                fields.AddTable<POLine>(c => c.IsDisabled());
                                                fields.AddField<POLine.cancelled>();
                                                fields.AddField<POLine.completed>();
                                                fields.AddField<POLine.promisedDate>();
                                                fields.AddField<POLine.closed>();
												fields.AddFields<DropShipOrderLineFields>(x => x.IsHidden());

												fields.AddAllFields<PORemitAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<PORemitContact>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipAddress>(c => c.IsDisabled());
                                                fields.AddAllFields<POShipContact>(c => c.IsDisabled());
                                                fields.AddTable<POTaxTran>(c => c.IsDisabled());
												fields.AddTable<POOrderDiscountDetail>(c => c.IsDisabled());
                                                fields.AddTable<CurrencyInfo>(c => c.IsDisabled());
                                            })
                                            .WithEventHandlers(handlers =>
                                            {
                                                handlers.Add(g => g.OnLinesReopened);
                                                handlers.Add(g => g.OnLinesCompleted);
                                                handlers.Add(g => g.OnReleaseChangeOrder);
                                            });
                                    });
                                    states.Add<State.pendingPrint>();
                                    states.Add<State.pendingEmail>();
                                })
                                .WithTransitions(transitions =>
                                {
                                    transitions.AddGroupFrom(State.Initial, ts =>
                                    {
                                        ts.Add(t => t
                                            .To<State.hold>()
                                            .IsTriggeredOn(g => g.initializeState)
                                            .When(conditions.IsOnHold));
                                        ts.Add(t => t
                                            .To<State.cancelled>()
                                            .IsTriggeredOn(g => g.initializeState)
                                            .When(conditions.IsCancelled));
                                        ts.Add(t => t
                                            .To<State.closed>()
                                            .IsTriggeredOn(g => g.initializeState)
                                            .When(conditions.HasAllLinesClosed));
                                        ts.Add(t => t
                                            .To<State.completed>()
                                            .IsTriggeredOn(g => g.initializeState)
                                            .When(conditions.HasAllLinesCompleted));
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.initializeState));
                                    });
                                    transitions.AddGroupFrom<State.hold>(ts =>
                                    {
                                        ts.Add(t => t
                                            .To<State.closed>()
                                            .IsTriggeredOn(g => g.releaseFromHold)
                                            .When(!conditions.IsOnHold && conditions.HasAllLinesClosed));
                                        ts.Add(t => t
                                            .To<State.completed>()
                                            .IsTriggeredOn(g => g.releaseFromHold)
                                            .When(!conditions.IsOnHold && conditions.HasAllLinesCompleted));
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.releaseFromHold)
                                            .When(!conditions.IsOnHold));
                                    });
                                    transitions.AddGroupFrom<State.open>(ts =>
                                    {
                                        ts.Add(t => t
                                            .To<State.hold>()
                                            .IsTriggeredOn(g => g.putOnHold)
                                            .When(conditions.IsOnHold));
                                        ts.Add(t => t
                                            .To<State.cancelled>()
                                            .IsTriggeredOn(g => g.cancelOrder)
                                            .When(conditions.IsCancelled));
                                        ts.Add(t => t
                                            .To<State.completed>()
                                            .IsTriggeredOn(g => g.OnLinesCompleted)
                                            .DoesNotPersist());
                                        ts.Add(t => t
                                            .To<State.completed>()
                                            .IsTriggeredOn(g => g.complete)
                                            .When(conditions.HasAllLinesCompleted));
                                        ts.Add(t => t
                                            .To<State.closed>()
                                            .IsTriggeredOn(g => g.OnLinesClosed));
                                        ts.Add(t => t
                                            .To<State.pendingPrint>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsPrinted));
                                        ts.Add(t => t
                                            .To<State.pendingEmail>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsEmailed));
                                    });
                                    transitions.AddGroupFrom<State.cancelled>(ts =>
                                    {
                                        ts.Add(t => t
                                            .To<State.hold>()
                                            .IsTriggeredOn(g => g.reopenOrder)
                                            .When(conditions.IsOnHold));
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.OnLinesReopened));
                                        ts.Add(t => t
                                            .To<State.pendingPrint>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsPrinted));
                                        ts.Add(t => t
                                            .To<State.pendingEmail>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsEmailed));
                                    });
                                    transitions.AddGroupFrom<State.completed>(ts =>
                                    {
                                        ts.Add(t => t
                                            .To<State.hold>()
                                            .IsTriggeredOn(g => g.reopenOrder)
                                            .When(conditions.IsOnHold));
                                        ts.Add(t => t
                                            .To<State.closed>()
                                            .IsTriggeredOn(g => g.OnLinesClosed));
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.OnLinesReopened));
                                        ts.Add(t => t
                                            .To<State.pendingPrint>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsPrinted));
                                        ts.Add(t => t
                                            .To<State.pendingEmail>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsEmailed));
                                    });
                                    transitions.AddGroupFrom<State.closed>(ts =>
                                    {
                                        ts.Add(t => t
                                            .To<State.open>()
                                            .IsTriggeredOn(g => g.OnLinesReopened));
                                        ts.Add(t => t
                                            .To<State.completed>()
                                            .IsTriggeredOn(g => g.OnLinesCompleted));
                                        ts.Add(t => t
                                            .To<State.hold>()
                                            .IsTriggeredOn(g => g.reopenOrder)
                                            .When(conditions.IsOnHold));
                                        ts.Add(t => t
                                            .To<State.pendingPrint>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsPrinted));
                                        ts.Add(t => t
                                            .To<State.pendingEmail>()
                                            .IsTriggeredOn(g => g.OnReleaseChangeOrder)
                                            .When(!conditions.IsEmailed));
                                    });
                                });
                        });
                    })
                    .WithActions(actions =>
                    {
                        actions.Add(g => g.initializeState);

                        #region Processing
                        actions.Add(g => g.releaseFromHold, c => c
                            .WithCategory(processingCategory)
                            .WithPersistOptions(ActionPersistOptions.NoPersist)
                            .WithFieldAssignments(fas =>
                            {
                                fas.Add<hold>(false);
                            }));
                        actions.Add(g => g.putOnHold, c => c
                            .WithCategory(processingCategory)
                            .WithPersistOptions(ActionPersistOptions.NoPersist)
                            .WithFieldAssignments(fas =>
                            {
                                fas.Add<hold>(true);
                                fas.Add<printed>(false);
                                fas.Add<emailed>(false);
                                fas.Add<cancelled>(false);
                            })
                            .IsDisabledWhen(conditions.IsChangeOrder));
                        actions.Add<Prepayments>(g => g.createPrepayment, c => c
                            .WithCategory(processingCategory));
                        actions.Add(g => g.createPOReceipt, c => c
                            .WithCategory(processingCategory)
                            .IsHiddenWhen(conditions.ProjectDropShipReceiptsNotAllowed));
                        actions.Add(g => g.createAPInvoice, c => c
                            .WithCategory(processingCategory));
                        actions.Add(g => g.complete, c => c
                            .WithCategory(processingCategory));
                        actions.Add(g => g.cancelOrder, c => c
                            .WithCategory(processingCategory)
                            .WithFieldAssignments(fas => fas.Add<cancelled>(true)));
                        actions.Add(g => g.reopenOrder, c => c
                            .WithCategory(processingCategory)
                            .WithFieldAssignments(fas =>
                            {
                                fas.Add<hold>(true);
                                fas.Add<printed>(false);
                                fas.Add<emailed>(false);
                                fas.Add<cancelled>(false);
                            })
                            .IsDisabledWhen(conditions.IsChangeOrder));
                        #endregion

                        #region Drop-Ship
                        actions.Add<DropShipLinksExt>(g => g.createSalesOrder, c => c
                            .WithCategory(dropShipCategory));
                        actions.Add<DropShipLinksExt>(g => g.unlinkFromSO, c => c
                            .WithCategory(dropShipCategory)
                            .IsHiddenWhen(!conditions.IsNewDropShipOrder)
                            .IsDisabledWhen(!conditions.IsLinkedToSalesOrder));
                        actions.Add<DropShipLinksExt>(g => g.convertToNormal, c => c
                            .WithCategory(dropShipCategory)
                            .IsHiddenWhen(!conditions.IsNewDropShipOrder));
                        #endregion

                        #region Intercompany
                        actions.Add<Intercompany>(e => e.generateSalesOrder, a => a
                            .WithCategory(intercompanyCategory)
                            .IsHiddenWhen(conditions.IsNotIntercompany)
                            .IsDisabledWhen(conditions.IsIntercompanyOrderGenerated));
                        #endregion

                        #region Printing and Emailing
                        actions.Add(g => g.printPurchaseOrder, c => c
                            .WithCategory(printingEmailingCategory)
							.WithPersistOptions(ActionPersistOptions.PersistBeforeAction)
                            .MassProcessingScreen<POPrintOrder>()
                            .InBatchMode());
                        actions.Add(g => g.markAsDontPrint, c => c
                            .WithCategory(printingEmailingCategory)
                            .MassProcessingScreen<POPrintOrder>()
                            .InBatchMode()
                            .WithFieldAssignments(fas => fas.Add<dontPrint>(true)));
                        actions.Add(g => g.emailPurchaseOrder, c => c.
                            WithCategory(printingEmailingCategory)
							.WithPersistOptions(ActionPersistOptions.PersistBeforeAction)
                            .MassProcessingScreen<POPrintOrder>()
                            .InBatchMode()
                            .WithFieldAssignments(fas => fas.Add<emailed>(true)));
                        actions.Add(g => g.markAsDontEmail, c => c
                            .WithCategory(printingEmailingCategory)
                            .MassProcessingScreen<POPrintOrder>()
                            .InBatchMode()
                            .WithFieldAssignments(fas => fas.Add<dontEmail>(true)));
                        #endregion

                        #region Other
                        actions.Add(g => g.recalculateDiscountsAction, c => c
                            .WithCategory(otherCategory));
                        actions.Add(g => g.validateAddresses, c => c
                            .WithCategory(otherCategory));
                        #endregion

                        #region Reports
                        actions.Add(g => g.vendorDetails, c => c
                            .WithCategory(PredefinedCategory.Reports)
							.WithPersistOptions(ActionPersistOptions.PersistBeforeAction));
                        actions.Add(g => g.viewPurchaseOrderReceipt, c => c
                            .WithCategory(PredefinedCategory.Reports)
							.WithPersistOptions(ActionPersistOptions.PersistBeforeAction));
                        #endregion

                        actions.Add(g => g.addPOOrder);
                        actions.Add(g => g.addPOOrderLine);
                        actions.Add<PurchaseToSOLinksExt>(g => g.viewDemand, c => c.IsHiddenWhen(conditions.IsNewDropShipOrder));
                        actions.Add(g => g.addInvBySite);
                        actions.Add(g => g.addInvSelBySite);

                        #region Side Panels
                        actions.AddNew("ShowPreferredVendorItemsGI", a => a
                            .DisplayName("Preferred Vendor Items")
                            .IsSidePanelScreen(sp => sp
                                .NavigateToScreen("IN2025SP")
                                .WithIcon("badge")
                                .WithAssignments(ass =>
                                {
                                    ass.Add(nameof(POOrder.VendorID) ,e => e.SetFromField<vendorID>());
                                })));
                        actions.AddNew("ShowVendorDetails", a => a
                            .DisplayName("Vendor Details")
                            .IsSidePanelScreen(sp => sp
                                .NavigateToScreen<AP.APDocumentEnq>()
                                .WithIcon("details")
                                .WithAssignments(ass =>
                                {
                                    ass.Add<AP.APDocumentEnq.APDocumentFilter.vendorID>(e => e.SetFromField<vendorID>());
                                })));
                        #endregion
                    })
                    .WithCategories(categories =>
                    {
                        categories.Add(processingCategory);
                        categories.Add(dropShipCategory);
                        categories.Add(intercompanyCategory);
                        categories.Add(printingEmailingCategory);
                        categories.Add(otherCategory);
                        categories.Update(FolderType.ReportsFolder, category => category.PlaceAfter(otherCategory));
                    })
                    .WithHandlers(handlers =>
                    {
                        handlers.Add(handler => handler
                            .WithTargetOf<POOrder>()
                            .OfEntityEvent<Events>(e => e.LinesCompleted)
                            .Is(g => g.OnLinesCompleted)
                            .UsesTargetAsPrimaryEntity()
                            .DisplayName("PO Lines Completed"));

                        handlers.Add(handler => handler
                            .WithTargetOf<POOrder>()
                            .OfEntityEvent<Events>(e => e.LinesClosed)
                            .Is(g => g.OnLinesClosed)
                            .UsesTargetAsPrimaryEntity()
                            .DisplayName("PO Lines Closed"));

                        handlers.Add(handler => handler
                            .WithTargetOf<POOrder>()
                            .OfEntityEvent<Events>(e => e.LinesReopened)
                            .Is(g => g.OnLinesReopened)
                            .UsesTargetAsPrimaryEntity()
                            .DisplayName("PO Lines Reopened"));

                        handlers.Add(handler => handler
                            .WithTargetOf<POOrder>()
                            .OfEntityEvent<Events>(e => e.Printed)
                            .Is(g => g.OnPrinted)
                            .UsesTargetAsPrimaryEntity()
                            .DisplayName("Printed"));

                        handlers.Add(handler => handler
                            .WithTargetOf<POOrder>()
                            .OfEntityEvent<Events>(e => e.DoNotPrintChecked)
                            .Is(g => g.OnDoNotPrintChecked)
                            .UsesTargetAsPrimaryEntity()
                            .DisplayName("Do Not Print Selected"));

                        handlers.Add(handler => handler
                            .WithTargetOf<POOrder>()
                            .OfEntityEvent<Events>(e => e.DoNotEmailChecked)
                            .Is(g => g.OnDoNotEmailChecked)
                            .UsesTargetAsPrimaryEntity()
                            .DisplayName("Do Not Email Selected"));

                        handlers.Add(handler => handler
                            .WithTargetOf<POOrder>()
                            .OfEntityEvent<Events>(e => e.LinesLinked)
                            .Is(g => g.OnLinesLinked)
                            .UsesTargetAsPrimaryEntity()
                            .DisplayName("PO Lines Linked"));

                        handlers.Add(handler => handler
                            .WithTargetOf<POOrder>()
                            .OfEntityEvent<Events>(e => e.LinesUnlinked)
                            .Is(g => g.OnLinesUnlinked)
                            .UsesTargetAsPrimaryEntity()
                            .DisplayName("PO Lines Unlinked"));

                        handlers.Add(handler => handler
                            .WithTargetOf<POOrder>()
                            .OfEntityEvent<Events>(e => e.ReleaseChangeOrder)
                            .Is(g => g.OnReleaseChangeOrder)
                            .UsesTargetAsPrimaryEntity()
                            .WithFieldAssignments(fas =>
                            {
                                fas.Add<printed>(false);
                                fas.Add<emailed>(false);
								fas.Add<cancelled>(false);
							})
                            .DisplayName("Change Order Released"));
                    });
            });
        }

        public static class ActionCategories
        {
            public const string DropShipCategoryID = "Drop-Ship Category";

            [PXLocalizable]
            public static class DisplayNames
            {
                public const string DropShip = "Drop-Ship";
            }
        }

		public class BlanketOrderLineFields : TypeArrayOf<IBqlField>
			.FilledWith<POLine.orderedQty, POLine.nonOrderedQty>
		{
		}

		public class DropShipOrderLineFields : TypeArrayOf<IBqlField>
			.FilledWith<POLine.sOOrderNbr, POLine.sOLineNbr, POLine.sOOrderStatus, POLine.sOLinkActive>
		{
		}
	}
}
