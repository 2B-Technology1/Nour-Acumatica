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
using PX.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.AM
{
	using State = AMECRStatus;
	using static AMECRItem;
	using static BoundedTo<ECRMaint, AMECRItem>;

    public class ECRMaint_Workflow : PXGraphExtension<ECRMaint>
    {
        public static bool IsActive() => true;

        public sealed override void Configure(PXScreenConfiguration config) =>
            Configure(config.GetScreenConfigurationContext<ECRMaint, AMECRItem>());

        protected static void Configure(WorkflowContext<ECRMaint, AMECRItem> context)
        {
            var processingCategory = context.Categories.CreateNew(ActionCategoryNames.Processing,
                category => category.DisplayName(ActionCategory.Processing));

            var approvalCategory = context.Categories.CreateNew(ActionCategoryNames.Approval,
                category => category.DisplayName(ActionCategory.Approval));

            #region Conditions
            Condition Bql<T>() where T : IBqlUnary, new() => context.Conditions.FromBql<T>();
            var conditions = new
            {
                IsOnHold
                    = Bql<hold.IsEqual<True>>(),
                IsApproved
                    = Bql<hold.IsEqual<False>.And<approved.IsEqual<True>>>(),
                IsCompleted
                    = Bql<status.IsEqual<State.completed>>(),
            }.AutoNameConditions();
            #endregion

            const string initialState = "_";

            context.AddScreenConfigurationFor(screen =>
            {
                return screen
                    .StateIdentifierIs<status>()
                    .AddDefaultFlow(flow => flow
                        .WithFlowStates(fss =>
                        {
                            fss.Add(initialState, flowState => flowState.IsInitial(g => g.initializeState));
                            fss.Add<State.hold>(flowState =>
                            {
                                return flowState
                                    .WithActions(actions =>
                                    {
                                        actions.Add(g => g.submit, a => a.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
                                    });
                            });
                            fss.Add<State.approved>(flowState =>
                            {
                                return flowState
                                    .WithActions(actions =>
                                    {
                                        actions.Add(g => g.hold, a => a.IsDuplicatedInToolbar());
                                        actions.Add(g => g.CreateECO, a => a.IsDuplicatedInToolbar());
                                    });
                            });
                            fss.Add<State.completed>(flowState =>
                            {
                                return flowState
                                    .WithActions(actions => { });
                            });
                        })
                        .WithTransitions(transitions =>
                        {
                            transitions.AddGroupFrom(initialState, ts =>
                            {
                                ts.Add(t => t.To<State.hold>().IsTriggeredOn(g => g.initializeState).When(conditions.IsOnHold));
                                ts.Add(t => t.To<State.approved>().IsTriggeredOn(g => g.initializeState).When(conditions.IsApproved));
                            });
                            transitions.AddGroupFrom<State.hold>(ts =>
                            {
                                ts.Add(t => t
                                    .To<State.approved>()
                                    .IsTriggeredOn(g => g.submit)
                                    .When(conditions.IsApproved));
                            });
                            transitions.AddGroupFrom<State.approved>(ts =>
                            {
                                ts.Add(t => t
                                    .To<State.hold>()
                                    .IsTriggeredOn(g => g.hold)
                                    .When(conditions.IsOnHold));
                            });
                        })
                    )
                    .WithCategories(categories =>
                    {
                        categories.Add(processingCategory);
                        categories.Add(approvalCategory);
                        categories.Update(FolderType.InquiriesFolder, category => category.PlaceAfter(approvalCategory));
                    })
                    .WithActions(actions => {
                        actions.Add(g => g.initializeState, a => a.IsHiddenAlways());
                        actions.Add(g => g.submit, a => a.WithCategory(processingCategory)
                            .WithFieldAssignments(fas =>
                            {
                                fas.Add<hold>(f => f.SetFromValue(false));
                                fas.Add<approved>(f => f.SetFromValue(true));
                                fas.Add<rejected>(f => f.SetFromValue(false));
                            }));
                        actions.Add(g => g.hold, a => a.WithCategory(processingCategory)
                            .WithFieldAssignments(fas =>
                            {
                                fas.Add<hold>(f => f.SetFromValue(true));
                                fas.Add<approved>(f => f.SetFromValue(false));
                            }));
                        actions.Add(g => g.CreateECO, a => a.WithCategory(processingCategory));
                    });
            });

        }

        public static class ActionCategoryNames
        {
            public const string Processing = "Processing";
            public const string Approval = "Approval";
        }

        public static class ActionCategory
        {
            public const string Processing = "Processing";
            public const string Approval = "Approval";
        }
    }
}
