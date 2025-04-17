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
using PX.Common;

namespace PX.Objects.CR.Workflows
{
	public static class LocationWorkflow
	{
		public static void Configure(PXScreenConfiguration configuration) =>
			Configure(configuration.GetScreenConfigurationContext<LocationMaint, Location>());
		public static void Configure(WorkflowContext<LocationMaint, Location> context)
		{
			#region Categories
			var customOtherCategory = context.Categories.CreateNew(ActionCategoryNames.CustomOther,
				category => category.DisplayName(ActionCategory.Other));
			#endregion

			var isDefaultCondition = context
				.Conditions
				.FromBql<Location.isDefault.IsEqual<True>>()
				.WithSharedName("IsDefault");

			context.AddScreenConfigurationFor(screen => screen
				.WithActions(actions =>
				{
					actions.Add(g => g.validateAddresses, a => a.InFolder(customOtherCategory));
				})
				.WithCategories(categories =>
				{
					categories.Add(customOtherCategory);
				}));
		}

		public static class ActionCategoryNames
		{
			public const string CustomOther = "CustomOther";
		}

		public static class ActionCategory
		{
			public const string Other = "Other";
		}
	}
}
