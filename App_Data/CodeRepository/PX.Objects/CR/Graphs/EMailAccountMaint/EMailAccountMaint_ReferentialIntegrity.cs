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
using PX.Data.BQL.Fluent;
using PX.SM;

namespace PX.Objects.CR.EMailAccountMaint_Extensions
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class EMailAccountMaint_ReferentialIntegrity : PXGraphExtension<PX.SM.EMailAccountMaint>
	{
		#region Views

		[PXHidden]
		[PXCopyPasteHiddenView]
		public SelectFrom<CRContactClass>.View ContactClass;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public SelectFrom<CRCaseClass>.View CaseClass;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public SelectFrom<CRCustomerClass>.View CustomerClass;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public SelectFrom<CRLeadClass>.View LeadClass;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public SelectFrom<CROpportunityClass>.View OpportunityClass;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public SelectFrom<UserPreferences>
			.Where<UserPreferences.defaultEMailAccountID.IsEqual<EMailAccount.emailAccountID.FromCurrent>>
			.View UserPreference;

		#endregion

		#region Events

		protected virtual void _(Events.RowDeleted<EMailAccount> e)
		{
			if (e.Row == null)
				return;

			PreferencesEmail preferences = Base.Preferences.SelectSingle();

			if (preferences != null && e.Row.EmailAccountID == preferences.DefaultEMailAccountID)
			{
				preferences.DefaultEMailAccountID = null;
				Base.Preferences.Update(preferences);
			}

			foreach (UserPreferences userPreferences in this.UserPreference.Select())
			{
				userPreferences.DefaultEMailAccountID = null;
				this.UserPreference.Update(userPreferences);
			}
		}

		#endregion
	}
}
