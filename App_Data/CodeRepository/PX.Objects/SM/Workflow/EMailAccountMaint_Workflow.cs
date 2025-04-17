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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Data.WorkflowAPI;
using PX.Objects.Common;
using PX.SM;
using PX.SM.Email;

namespace PX.Objects.SM
{
	public class EMailAccountMaint_Workflow : PXGraphExtension<EMailAccountMaint>
	{
		// only workflow
		public static bool IsActive() => false;

		public sealed override void Configure(PXScreenConfiguration configuration) =>
			Configure(configuration.GetScreenConfigurationContext<EMailAccountMaint, EMailAccount>());
		protected static void Configure(WorkflowContext<EMailAccountMaint, EMailAccount> context)
		{
			var otherCategory = CommonActionCategories.Get(context).Other;

			context.AddScreenConfigurationFor(screen =>
			{
				return screen
					.WithActions(actions =>
					{
						actions.AddNew("EmailSummarySidePanel", a => a
							.DisplayName("Summary")
							.IsSidePanelScreen(sp =>
								sp
									.NavigateToScreen("SM2040DB")
									.WithIcon("pie_chart")
									.WithAssignments(fields =>
									{
										fields.Add("EmailAccount", f => f.SetFromField<EMailAccount.emailAccountID>());
									})
							));

						actions.AddNew("EmailTrackingSidePanel", a => a
							.DisplayName("Account Emails")
							.IsSidePanelScreen(sp =>
								sp
									.NavigateToScreen("SM2040SP")
									.WithIcon("email_outline")
									.WithAssignments(fields =>
									{
										fields.Add("EMailAccount_emailAccountID", f => f.SetFromField<EMailAccount.emailAccountID>());
									})
							));

						actions.Add(graph => graph.SendAll,
						            c => c.WithCategory(PredefinedCategory.Actions)
						                  .MassProcessingScreen<EmailSendReceiveMaint>());

						actions.Add(graph => graph.ReceiveAll,
						            c => c.WithCategory(PredefinedCategory.Actions)
						                  .MassProcessingScreen<EmailSendReceiveMaint>());

						actions.Add(graph => graph.SendReceiveAll,
						            c => c.WithCategory(PredefinedCategory.Actions)
						                  .MassProcessingScreen<EmailSendReceiveMaint>());

						actions.Add<EMailAccountMaint_CheckEmailAccountGraphExt>(graph => graph.CheckEmailAccount,
						            c => c.WithCategory(otherCategory)
						                  .WithDisplayOnToolbar(DisplayOnToolBar.Always));
					})
					.WithCategories(categories =>
					{
						categories.Add(otherCategory);
					});
			});
		}
	}
}
