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
using PX.Objects.AR;
using PX.Objects.CS;

namespace PX.Objects.PM
{
	public class ARInvoiceEntryWorkflowExt : PXGraphExtension<ARInvoiceEntry_Workflow, ARInvoiceEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.projectAccounting>();
		}

		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<ARInvoiceEntry, ARInvoice>());
		protected static void Configure(WorkflowContext<ARInvoiceEntry, ARInvoice> context)
		{
			BoundedTo<ARInvoiceEntry, ARInvoice>.Condition Bql<T>() where T : IBqlUnary, new() => context.Conditions.FromBql<T>();

			var conditions = new
			{
				CreateScheduleHidden
					= Bql<ARInvoice.proformaExists.IsEqual<True>>(),
			}.AutoNameConditions();

			context.UpdateScreenConfigurationFor(screen =>
			{
				return screen
					.WithActions(actions =>
					{
						actions.Update(g => g.createSchedule, c => c.IsHiddenWhenElse(conditions.CreateScheduleHidden));
					});
			});
		}
	}
}
