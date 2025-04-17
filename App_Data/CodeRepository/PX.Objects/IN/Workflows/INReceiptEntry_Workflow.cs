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

using PX.Data.WorkflowAPI;

namespace PX.Objects.IN
{
	using State = INDocStatus;

	public class INReceiptEntry_Workflow : INRegisterEntryBase_Workflow<INReceiptEntry, INDocType.receipt>
	{
		public sealed override void Configure(PXScreenConfiguration configuration) =>
			Configure(configuration.GetScreenConfigurationContext<INReceiptEntry, INRegister>());
		protected static void Configure(WorkflowContext<INReceiptEntry, INRegister> context)
		{
			ConfigureCommon(context);
			context.UpdateScreenConfigurationFor(screen =>
				screen
				.WithActions(actions =>
					actions.Add(g => g.iNItemLabels, a => a.WithCategory(PredefinedCategory.Reports)))
				.UpdateDefaultFlow(flow =>
					flow.WithFlowStates(flowStates =>
						flowStates.Update<State.released>(flowState =>
							flowState.WithActions(actions =>
								actions.Add(g => g.iNItemLabels))))));
		}
	}
}
