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
using PX.Objects.CR.Workflows;

namespace PX.Objects.AR.Workflows
{

	public class CustomerLocationMaint_Workflow : PXGraphExtension<CustomerLocationMaint>
	{
		public static bool IsActive() => false;


		public sealed override void Configure(PXScreenConfiguration configuration) =>
			Configure(configuration.GetScreenConfigurationContext<LocationMaint, Location>());
		protected static void Configure(WorkflowContext<LocationMaint, Location> context)
		{
			LocationWorkflow.Configure(context);

			var otherCategory = context.Categories.Get(LocationWorkflow.ActionCategoryNames.CustomOther);
			var viewAccountLocationAction = context.ActionDefinitions.CreateExisting(g => ((CustomerLocationMaint)g).ViewAccountLocation, a => a.InFolder(otherCategory));
			context.UpdateScreenConfigurationFor(screen => screen.WithActions(a => a.Add(viewAccountLocationAction)));
		}
	}
}
