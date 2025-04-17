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
using PX.Objects.AM.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.AM
{
    using static BoundedTo<ECRMaint, AMECRItem>;
    using static AMECRItem;
    using State = AMECRStatus;
    using This = ECRMaint_ApprovalWorkflow;

    public class ECRMaint_ApprovalWorkflow : PXGraphExtension<ECRMaint_Workflow, ECRMaint>
    {
        public static bool IsActive() => true;

        public class AMBSetupDefinition : IPrefetchable
        {
            public bool ECRRequestApproval { get; private set; }
            public bool ECORequestApproval { get; private set; }
            public static bool ECRActive => PXDatabase.GetSlot<AMBSetupDefinition>(nameof(AMBSetupDefinition), typeof(AMBSetup)).ECRRequestApproval;
            public static bool ECOActive => PXDatabase.GetSlot<AMBSetupDefinition>(nameof(AMBSetupDefinition), typeof(AMBSetup)).ECORequestApproval;

            void IPrefetchable.Prefetch()
            {
                using (PXDataRecord rec =
                    PXDatabase.SelectSingle<AMBSetup>(
                        new PXDataField("ECRRequestApproval"),
                        new PXDataField("ECORequestApproval")))
                {
                    if (rec != null)
                    {
                        ECRRequestApproval = (bool)rec.GetBoolean(0);
                        ECORequestApproval = (bool)rec.GetBoolean(1);
                    }

                }
            }
            public static AMBSetupDefinition GetSlot()
            {
                return PXDatabase.GetSlot<AMBSetupDefinition>(typeof(AMBSetup).FullName, typeof(AMBSetup));
            }
        }

        public const string ApproveActionName = "Approve";
        public const string RejectActionName = "Reject";

        private static bool ApprovalIsActive => PXAccess.FeatureInstalled<CS.FeaturesSet.approvalWorkflow>() && AMBSetupDefinition.ECRActive;


        [PXWorkflowDependsOnType(typeof(AMECRSetupApproval))]
        public sealed override void Configure(PXScreenConfiguration config)
        {
            if (ApprovalIsActive)
                Configure(config.GetScreenConfigurationContext<ECRMaint, AMECRItem>());
            else
                HideApproveAndRejectActions(config.GetScreenConfigurationContext<ECRMaint, AMECRItem>());
        }

        protected static void HideApproveAndRejectActions(WorkflowContext<ECRMaint, AMECRItem> context)
        {
            var approvalCategory = context.Categories.Get(categoryName: ECRMaint_Workflow.ActionCategory.Approval);
            var approve = context.ActionDefinitions
                .CreateNew(ApproveActionName, a => a
                    .WithCategory(approvalCategory)
                .PlaceAfter(g => g.hold)
                .IsHiddenAlways());
            var reject = context.ActionDefinitions
                .CreateNew(RejectActionName, a => a
                    .WithCategory(approvalCategory)
                .PlaceAfter(approve)
                .IsHiddenAlways());
            var reassign = context.ActionDefinitions
                .CreateExisting(nameof(ECRMaint.Approval.ReassignApproval), a => a
                    .WithCategory(approvalCategory)
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

        protected static void Configure(WorkflowContext<ECRMaint, AMECRItem> context)
        {
            var _Definition = AMBSetupDefinition.GetSlot();

            #region Conditions
            Condition Bql<T>() where T : IBqlUnary, new() => context.Conditions.FromBql<T>();
            Condition Existing(string name) => (Condition)context.Conditions.Get(name);
            var conditions = new
            {
                IsPendingApproval
                    = Bql<hold.IsEqual<False>.And<approved.IsEqual<False>>>(),
                IsNotApprovedOrCompleted
                    = Bql<approved.IsEqual<False>.Or<status.IsEqual<AMECRStatus.completed>>>(),
                IsNotOnHold
                    = Bql<hold.IsEqual<False>>(),
                IsApproved
                    = Existing("IsApproved"),
                IsOnHold
                    = Existing("IsOnHold"),
                IsApprovalRequired
                    = _Definition.ECRRequestApproval == true
                        ? Bql<True.IsEqual<True>>()
                        : Bql<True.IsEqual<False>>(),
            }.AutoNameConditions();
            #endregion
            var approvalCategory = context.Categories.Get(categoryName: ECRMaint_Workflow.ActionCategory.Approval);
            var approve = context.ActionDefinitions
                .CreateNew(ApproveActionName, a => a
                .WithCategory(approvalCategory)
                .PlaceAfter(g => g.hold)
                .WithFieldAssignments(fas =>
                {
                    fas.Add<approved>(true);
                }));
            var reject = context.ActionDefinitions
                .CreateNew(RejectActionName, a => a
                    .WithCategory(approvalCategory)
                .PlaceAfter(approve)
                .WithFieldAssignments(fas =>
                {
                    fas.Add<hold>(true);
                }));
            var reassign = context.ActionDefinitions
                .CreateExisting(nameof(ECRMaint.Approval.ReassignApproval), a => a
                .WithCategory(approvalCategory)
                .PlaceAfter(reject));

            context.UpdateScreenConfigurationFor(screen =>
            {
                return screen
                    .UpdateDefaultFlow(flow => flow
                        .WithFlowStates(fss => {
                            fss.Add<State.pendingApproval>(flowState =>
                            {
                                return flowState
                                    .WithActions(actions =>
                                    {
                                        actions.Add(approve, a => a.IsDuplicatedInToolbar());
                                        actions.Add(reject, a => a.IsDuplicatedInToolbar());
                                        actions.Add(reassign);
                                    });
                            });
                        })
                        .WithTransitions(transitions =>
                        {
                            transitions.UpdateGroupFrom<State.hold>(ts =>
                            {
                                ts.Remove(t => t.To<State.approved>().IsTriggeredOn(g => g.submit));
                                ts.Add(t => t
                                    .To<State.pendingApproval>()
                                    .IsTriggeredOn(g => g.submit).When(conditions.IsApprovalRequired));
                            });
                            transitions.AddGroupFrom<State.pendingApproval>(ts =>
                            {
                                ts.Add(t => t
                                    .To<State.approved>()
                                    .IsTriggeredOn(approve)
                                    .When(conditions.IsApproved));
                                ts.Add(t => t
                                    .To<State.hold>()
                                        .IsTriggeredOn(reject)
                                        .When(conditions.IsOnHold));
                            });
                        }))
                    .WithActions(actions =>
                    {
                        actions.Add(approve);
                        actions.Add(reject);
                        actions.Add(reassign);
                    });
            });
        }
    }
}
