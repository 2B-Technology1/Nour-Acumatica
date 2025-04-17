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
using PX.Objects.CR.BusinessAccountMaint_Extensions;
using PX.Objects.CR.Extensions;
using State = PX.Objects.AR.CustomerStatus;

namespace PX.Objects.CR.Workflows
{
	using static PX.Data.WorkflowAPI.BoundedTo<BusinessAccountMaint, BAccount>;

	public class BusinessAccountWorkflow : PXGraphExtension<BusinessAccountMaint>
	{
		public static bool IsActive() => false;

		#region Consts

		public static class CategoryNames
		{
			public const string RecordCreation = "RecordCreation";
			public const string Activities = "Activities";
			public const string Validation = "Validation";
			public const string Other = "Other";
		}

		#endregion

		public sealed override void Configure(PXScreenConfiguration configuration) =>
			Configure(configuration.GetScreenConfigurationContext<BusinessAccountMaint, BAccount>());

		protected static void Configure(WorkflowContext<BusinessAccountMaint, BAccount> context)
		{
			#region categories

			var categoryRecordCreation = context.Categories.CreateNew(CategoryNames.RecordCreation,
				category => category.DisplayName("Record Creation"));
			var categoryActivities = context.Categories.CreateNew(CategoryNames.Activities,
				category => category.DisplayName("Activities").PlaceAfter(categoryRecordCreation));
			var categoryValidation = context.Categories.CreateNew(CategoryNames.Validation,
				category => category.DisplayName("Validation").PlaceAfter(categoryActivities));
			var categoryOther = context.Categories.CreateNew(CategoryNames.Other,
				category => category.DisplayName("Other").PlaceAfter(categoryValidation));

			#endregion

			#region Conditions

			var conditions = new
			{
				IsBusinessAccount
					= context.Conditions.FromBql<BAccount.type.IsEqual<BAccountType.prospectType>>(),

				IsCustomer
					= context.Conditions.FromBql<BAccount.type.IsEqual<BAccountType.customerType>.Or<BAccount.type.IsEqual<BAccountType.combinedType>>>(),

				IsNotCustomer
					= context.Conditions.FromBql<BAccount.type.IsNotEqual<BAccountType.customerType>.And<BAccount.type.IsNotEqual<BAccountType.combinedType>>>(),

				IsVendor
					= context.Conditions.FromBql<BAccount.type.IsEqual<BAccountType.vendorType>.Or<BAccount.type.IsEqual<BAccountType.combinedType>>>(),

				IsNotVendor
					= context.Conditions.FromBql<BAccount.type.IsNotEqual<BAccountType.vendorType>.And<BAccount.type.IsNotEqual<BAccountType.combinedType>>>(),

				IsExtendToCustomerHidden
					= context.Conditions.FromBql<BAccount.type.IsEqual<BAccountType.customerType>.Or<BAccount.type.IsEqual<BAccountType.combinedType>>>(),

				IsExtendToVendorHidded
					= context.Conditions.FromBql<BAccount.type.IsEqual<BAccountType.vendorType>.Or<BAccount.type.IsEqual<BAccountType.combinedType>>>(),

			}.AutoNameConditions();

			#endregion

			context.AddScreenConfigurationFor(screen =>
			{
				return screen
					.WithActions(actions =>
					{
						// Record Creation
						actions.Add(g =>
										g.addOpportunity, a => a
												.WithCategory(categoryRecordCreation)
												.WithPersistOptions(ActionPersistOptions.PersistBeforeAction));
						actions.Add<BusinessAccountMaint.CreateContactFromAccountGraphExt>(e =>
										e.CreateContact, a => a
												.WithCategory(categoryRecordCreation)
												.WithPersistOptions(ActionPersistOptions.PersistBeforeAction));
						actions.Add<BusinessAccountMaint.ExtendToCustomer>(e =>
										e.extendToCustomer, a => a
												.WithPersistOptions(ActionPersistOptions.PersistBeforeAction)
												.IsHiddenWhen(conditions.IsExtendToCustomerHidden));
						actions.Add<BusinessAccountMaint.ExtendToVendor>(e =>
										e.extendToVendor, a => a
												.WithCategory(categoryRecordCreation)
												.WithPersistOptions(ActionPersistOptions.PersistBeforeAction)
												.IsHiddenWhen(conditions.IsExtendToVendorHidded));
						actions.Add<BusinessAccountMaint.CreateLeadFromAccountGraphExt>(e => e
										.CreateLead, a => a
												.WithCategory(categoryRecordCreation)
												.WithPersistOptions(ActionPersistOptions.PersistBeforeAction));

						// Activities
						actions.Add(BusinessAccountMaint_ActivityDetailsExt_Actions.ActionNames.NewTask_Workflow,
							a => a.WithCategory(categoryActivities));
						actions.Add(BusinessAccountMaint_ActivityDetailsExt_Actions.ActionNames.NewActivity_Note_Workflow,
							a => a.WithCategory(categoryActivities));

						// Validation
						actions.Add<BusinessAccountMaint.DefContactAddressExt>(e =>
										e.ValidateAddresses, a => a
												.WithCategory(categoryValidation));

						// Other
						actions.Add<BusinessAccountMaint.ExtendToCustomer>(e =>
										e.viewCustomer, a => a
												.WithPersistOptions(ActionPersistOptions.PersistBeforeAction)
												.IsHiddenWhen(conditions.IsNotCustomer));
						actions.Add<BusinessAccountMaint.ExtendToVendor>(e =>
										e.viewVendor, a => a
												.WithCategory(categoryOther)
												.WithPersistOptions(ActionPersistOptions.PersistBeforeAction)
												.IsHiddenWhen(conditions.IsNotVendor));
						actions.Add(g =>
										g.ChangeID, a => a
												.WithCategory(categoryOther, nameof(BusinessAccountMaint.ExtendToVendor.viewVendor))
												.WithPersistOptions(ActionPersistOptions.PersistBeforeAction));

					})
					.WithCategories(categories =>
					{
						categories.Add(categoryRecordCreation);
						categories.Add(categoryValidation);
						categories.Add(categoryActivities);
						categories.Add(categoryOther);
					});
			});
		}
	}
}
