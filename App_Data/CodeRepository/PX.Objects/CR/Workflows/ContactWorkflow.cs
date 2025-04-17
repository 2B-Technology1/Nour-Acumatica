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
using PX.Objects.CR.ContactMaint_Extensions;
using PX.Objects.CR.Extensions;
using static PX.Objects.CR.ContactMaint;

namespace PX.Objects.CR.Workflows
{
	public class ContactWorkflow : PXGraphExtension<ContactMaint>
	{
		public static bool IsActive() => false;

		#region Consts

		public static class CategoryNames
		{
			public const string RecordCreation = "RecordCreation";
			public const string Activities = "Activities";
			public const string Validation = "Validation";
		}

		#endregion

		public sealed override void Configure(PXScreenConfiguration configuration) =>
			Configure(configuration.GetScreenConfigurationContext<ContactMaint, Contact>());
		protected static void Configure(WorkflowContext<ContactMaint, Contact> config)
		{
			#region Conditions
			var conditions = new
			{
				IsContactActive
					= config.Conditions.FromBql<Contact.status.IsNotEqual<ContactStatus.active>>(),

			}.AutoNameConditions();
			#endregion

			#region categories

			var categoryRecordCreation = config.Categories.CreateNew(CategoryNames.RecordCreation,
				category => category.DisplayName("Record Creation"));
			var categoryActivities = config.Categories.CreateNew(CategoryNames.Activities,
				category => category.DisplayName("Activities"));
			var categoryValidation = config.Categories.CreateNew(CategoryNames.Validation,
				category => category.DisplayName("Validation"));

			#endregion

			var actionCreatePhoneCall = config.ActionDefinitions.CreateExisting(
				ContactMaint_ActivityDetailsExt_Actions.ActionNames.NewActivity_Phonecall_Workflow,
				a => a.WithCategory(categoryActivities));
			var actionCreateNote = config.ActionDefinitions.CreateExisting(
				ContactMaint_ActivityDetailsExt_Actions.ActionNames.NewActivity_Note_Workflow,
				a => a.WithCategory(categoryActivities));
			var actionCreateMail = config.ActionDefinitions.CreateExisting(
				ContactMaint_ActivityDetailsExt_Actions.ActionNames.NewMailActivity_Workflow,
				a => a.WithCategory(categoryActivities));
			var actionCreateTask = config.ActionDefinitions.CreateExisting(
				ContactMaint_ActivityDetailsExt_Actions.ActionNames.NewTask_Workflow,
				a => a.WithCategory(categoryActivities));

			config.AddScreenConfigurationFor(screen =>
			{
				return screen
					.WithActions(actions =>
					{
						actions.Add(g => g.addOpportunity, c => c.WithCategory(categoryRecordCreation));
						actions.Add<CreateAccountFromContactGraphExt>(g =>
										g.CreateBAccount, c => c
											.WithCategory(categoryRecordCreation));
						actions.Add(g => g.addCase, c => c.WithCategory(categoryRecordCreation));
						actions.Add<CreateLeadFromContactGraphExt>(g =>
										g.CreateLead, c => c
											.WithCategory(categoryRecordCreation)
											.IsDisabledWhen(conditions.IsContactActive));

						actions.Add(actionCreatePhoneCall);
						actions.Add(actionCreateNote);
						actions.Add(actionCreateMail);
						actions.Add(actionCreateTask);

					})
					.WithCategories(categories =>
					{
						categories.Add(categoryRecordCreation);
						categories.Add(categoryValidation);
						categories.Add(categoryActivities);
					})
					.WithFieldStates(fields => 
						fields.Add<Contact.resolution>(field =>
							field.SetComboValues(
								("AS", "Assign"),
								("CA", "Abandoned"),
								("CD", "Duplicate"),
								("CL", "Unable to contact"),
								("CU", "Unknown"),
								("EX", "External"),
								("IN", "In Process"),
								("OU", "Nurture"),
								("RJ", "Disqualified")
							)
						)
					);
			});

		}
	}
}
