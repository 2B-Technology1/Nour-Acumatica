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
using PX.Objects.CR;
using PX.Objects.CR.CRCaseMaint_Extensions;
using PX.Objects.CR.Extensions;
using PX.Objects.CR.Workflows;

namespace SP.Objects.CR
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class CaseWorkflowExt : PXGraphExtension<CaseWorkflow, CRCaseMaint>
	{
		public override void Configure(PXScreenConfiguration configuration)
		{
			var context = configuration.GetScreenConfigurationContext<CRCaseMaint, CRCase>();

			context.UpdateScreenConfigurationFor(screen =>
			{
				return screen
					.WithActions(actions =>
					{
						HideAction(actions, CaseWorkflow.ActionNames.Open);
						HideAction(actions, nameof(CRCaseMaint.TakeCase));
						HideAction(actions, CaseWorkflow.ActionNames.Close);
						HideAction(actions, CaseWorkflow.ActionNames.PendingCustomer);
						HideAction(actions, nameof(CRCaseMaint.Release));
						HideAction(actions, nameof(CRCaseMaint.ViewInvoice));
						HideAction(actions, nameof(CRCaseMaint.Assign));

						HideAction(actions, CRCaseMaint_ActivityDetailsExt_Actions.ActionNames.NewMailActivity_Workflow);
						HideAction(actions, CRCaseMaint_ActivityDetailsExt_Actions.ActionNames.NewActivity_Workitem_Workflow);
						HideAction(actions, CRCaseMaint_ActivityDetailsExt_Actions.ActionNames.NewActivity_Note_Workflow);
						HideAction(actions, CRCaseMaint_ActivityDetailsExt_Actions.ActionNames.NewTask_Workflow);
						HideAction(actions, CRCaseMaint_ActivityDetailsExt_Actions.ActionNames.NewActivity_Phonecall_Workflow);

						HideAction(actions, nameof(CRCaseMaint_CRCreateReturnOrder.CreateReturnOrder));
					});
			});
		}

		private static void HideAction(BoundedTo<CRCaseMaint, CRCase>.ActionDefinition.ContainerAdjusterActions actions, string actionName)
		{
			actions.Update(actionName, action => action.IsHiddenAlways());
		}
	}
}
