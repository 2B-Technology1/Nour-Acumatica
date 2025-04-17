using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nour20220913V1;
using Nour20230314V1.InspectionForm.helpers;
using PX.Data;
using PX.Data.WorkflowAPI;
using static PX.Data.WorkflowAPI.BoundedTo<Nour20220913V1.InspectionFormEntry,
Nour20220913V1.InspectionFormInq>;

namespace Nour20230314V1.WorkFlow
{
    public class InspectionFormWorkFlow : PX.Data.PXGraphExtension<InspectionFormEntry>
    {

        #region constant
        public static class States
        {
            public const string Open = InspectionStatesConstants.open;
            public const string Cancel = InspectionStatesConstants.canceled;
            public const string JobOrder = InspectionStatesConstants.jobOrder;
            public class open : PX.Data.BQL.BqlString.Constant<open>
            {
                public open() : base(Open) { }
            }
            public class cancel : PX.Data.BQL.BqlString.Constant<cancel>
            {
                public cancel() : base(Cancel) { }
            }
            public class jobOrder : PX.Data.BQL.BqlString.Constant<jobOrder>
            {
                public jobOrder() : base(JobOrder) { }
            }
        }
        #endregion



        public class Conditions : Condition.Pack
        {
            public Condition CreateJobOrderCondition => GetOrCreate(b => b.FromBql<
              Where<InspectionFormInq.status.IsNotEqual<States.open>>>());
        }



        public sealed override void Configure(PXScreenConfiguration config)
        {
            Configure(config.GetScreenConfigurationContext<InspectionFormEntry, InspectionFormInq>());
        }



        protected static void Configure(WorkflowContext<InspectionFormEntry, InspectionFormInq> context)
        {

            var conditions = context.Conditions.GetPack<Conditions>();


            context.AddScreenConfigurationFor(screen =>
            screen
            .StateIdentifierIs<InspectionFormInq.status>()
            .AddDefaultFlow(flow => flow
                .WithFlowStates(fss =>
                {
                    fss.Add<States.open>(flowState =>
                    {
                        return flowState
                        .IsInitial()
                        .WithActions(actions =>
                        {
                            actions.Add(g => g.ConvertToJobOrder, a => a
                            .IsDuplicatedInToolbar());

                            actions.Add(g => g.CancelForm, a =>
                            a.IsDuplicatedInToolbar());
                        });
                    });
                    fss.Add<States.jobOrder>(flowState =>
                    {
                        return flowState
                        .WithFieldStates(states =>
                        {
                            states.AddAllFields<InspectionFormInq>(s => s.IsDisabled());
                        });

                    });
                    fss.Add<States.cancel>(flowState =>
                    {
                        return flowState
                        .WithFieldStates(states =>
                        {
                            states.AddAllFields<InspectionFormInq>(s => s.IsDisabled());
                        });
                    });

                })
                .WithTransitions(transitions =>
                {


                    transitions.Add(t => t.From<States.open>()
                            .To<States.cancel>()
                            .IsTriggeredOn(g => g.CancelForm));


                    transitions.Add(t => t.From<States.open>()
                            .To<States.jobOrder>()
                            .IsTriggeredOn(g => g.ConvertToJobOrder)

                            );

                })

                ).WithActions(actions =>
                {
                    actions.Add(g => g.ConvertToJobOrder, c => c.IsDisabledWhen(conditions.CreateJobOrderCondition));
                })
            );
        }
    }
}