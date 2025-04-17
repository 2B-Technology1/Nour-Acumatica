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

namespace PX.Objects.CR.Workflows
{
	public class CampaignMaintWorkflow : PXGraphExtension<CampaignMaint>
	{
		public static bool IsActive() => false;

		public static class States
		{
			public const string Canceled = "A";
			public const string Completed = "O";
			public const string Planning = "P";
			public const string Execution = "X";
		}

		public static class StateNames
		{
			public const string Canceled = "Canceled";
			public const string Completed = "Completed";
			public const string Planning = "Planning";
			public const string Execution = "Execution";
		}

		public sealed override void Configure(PXScreenConfiguration configuration) =>
			Configure(configuration.GetScreenConfigurationContext<CampaignMaint, CRCampaign>());

		protected static void Configure(WorkflowContext<CampaignMaint, CRCampaign> context)
		{
			context.AddScreenConfigurationFor(screen =>
			{
				return screen.WithFieldStates(fields =>
					fields.Add<CRCampaign.status>(field => field
						.SetComboValues(
							(States.Canceled, StateNames.Canceled),
							(States.Completed, StateNames.Completed),
							(States.Planning, StateNames.Planning),
							(States.Execution, StateNames.Execution))
						.WithDefaultValue(States.Planning)));
			});
		}
	}
}
