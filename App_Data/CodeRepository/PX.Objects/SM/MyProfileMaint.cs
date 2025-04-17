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

using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using System;
using PX.Data.BQL.Fluent;
using Microsoft.Extensions.Options;

namespace PX.SM
{
	public class MyProfileMaint : SMAccessPersonalMaint
	{
		[InjectDependency]
		private IAdvancedAuthenticationRestrictor AdvancedAuthenticationRestrictor { get; set; }

		#region Selects

		public PXSelect<Contact, Where<Contact.userID, Equal<Optional<Users.pKID>>>> Contact;

		[PXHidden]
		public PXSelect<EPEmployee, Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>> Employee;

		public PXSelectJoin<EMailSyncAccount,
			InnerJoin<BAccount,
				On<BAccount.bAccountID, Equal<EMailSyncAccount.employeeID>>,
			InnerJoin<Contact,
				On<Contact.contactID, Equal<BAccount.defContactID>,
				And<Contact.userID, Equal<Optional<Users.pKID>>>>>>> SyncAccount;

		public PXSelect<EMailAccount,
			Where<EMailAccount.emailAccountID, Equal<Optional<EMailSyncAccount.emailAccountID>>>> EMailAccountsNew;

		public PXFilter<CustomerManagementFeature> CustomerModule;

		public SelectFrom<NotificationSetupUserOverride>.Where<NotificationSetupUserOverride.userID.IsEqual<Users.pKID.FromCurrent>>.View Notifications;

		#endregion

		#region Event Handlers
		[PXDBString(50)]
		[PXUIField(DisplayName = "Phone", Visibility = PXUIVisibility.SelectorVisible)]
		[PhoneValidation]
		protected virtual void Users_Phone_CacheAttached(PXCache sender) {}

		protected virtual void UserPreferences_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ResetTimeZone.SetVisible(true);
		}

		protected virtual void UserPreferences_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.TranStatus == PXTranStatus.Completed)
			{
				if (System.Web.HttpContext.Current != null &&
				    String.Equals(this.UserProfile.Current.Username, PXAccess.GetUserName()))
					// do not throw an exception to prevent breaking of the event-handling chain
					Redirector.Refresh(System.Web.HttpContext.Current);
			}
		}

		#endregion

		#region Public Methods

		public override string GetUserTimeZoneId(string username)
		{
			var result = base.GetUserTimeZoneId(username);
			if (string.IsNullOrEmpty(result))
			{
				var set = PXSelectJoin<CSCalendar,
					InnerJoin<EPEmployee, On<EPEmployee.calendarID, Equal<CSCalendar.calendarID>>,
						InnerJoin<Users, On<Users.pKID, Equal<EPEmployee.userID>>>>,
					Where<Users.username, Equal<Required<Users.username>>>>.
					Select(this, username);
				if (set != null && set.Count > 0) result = ((CSCalendar)set[0][typeof(CSCalendar)]).TimeZone;
			}
			return result;
		}

		protected override string GetDefaultUserTimeZoneId(string username)
		{
			CSCalendar calendar = PXSelectJoin<CSCalendar,
				InnerJoin<EPEmployee, On<EPEmployee.calendarID, Equal<CSCalendar.calendarID>>,
					InnerJoin<Users, On<Users.pKID, Equal<EPEmployee.userID>>>>,
				Where<Users.username, Equal<Required<Users.username>>>>.
				Select(this, username);

			if (calendar != null && !string.IsNullOrEmpty(calendar.TimeZone))
				return calendar.TimeZone;

			return base.GetDefaultUserTimeZoneId(username);
		}

		[PXUIField(DisplayName = "Change Email", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public override void changeEmail()
		{
			base.changeEmail();
			foreach (Contact copy in Contact.Select(UserProfile.Current.PKID)
				       .RowCast<Contact>()
				       .Select(contact => (Contact) Contact.Cache.CreateCopy(contact)))
			{
				copy.EMail = UserProfile.Current.Email;
				Contact.Update(copy);
			}

			foreach (EMailSyncAccount syncAccount in SyncAccount.Select(UserProfile.Current.PKID)
					   .RowCast<EMailSyncAccount>()
					   .Select(account => (EMailSyncAccount)SyncAccount.Cache.CreateCopy(account)))
			{
				syncAccount.Address = UserProfile.Current.Email;

				syncAccount.ContactsExportDate = null;
				syncAccount.ContactsImportDate = null;
				syncAccount.EmailsExportDate = null;
				syncAccount.EmailsImportDate = null;
				syncAccount.TasksExportDate = null;
				syncAccount.TasksImportDate = null;
				syncAccount.EventsExportDate = null;
				syncAccount.EventsImportDate = null;

				EMailAccount mailAccount = EMailAccountsNew.Select(syncAccount.EmailAccountID);
				mailAccount.Address = syncAccount.Address;

				EMailAccountsNew.Update(mailAccount);

				SyncAccount.Update(syncAccount);
			}

			Actions.PressSave();
		}

		#endregion

		#region EPEmployeeDelegateExtension

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class MyProfileMaint_EPEmployeeDelegateExtension : PX.Objects.Extension.EPEmployeeDelegateExtension<MyProfileMaint>
		{
			public static bool IsActive() => IsExtensionActive();

			#region Override

			public override void Initialize()
			{
				Delegates.Join<InnerJoin<EPEmployee,
					On<EPEmployee.bAccountID, Equal<EPWingman.employeeID>>>>();

				Delegates.WhereNew<Where<
					EPEmployee.userID.IsEqual<AccessInfo.userID.FromCurrent>>>();
			}

			#endregion

			#region Events

			#region CachAttach

			[PXMergeAttributes(Method = MergeMethod.Merge)]
			[PXDBDefault(typeof(Search<EPEmployee.bAccountID, Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>))]
			protected virtual void _(Events.CacheAttached<EPWingman.employeeID> e) { }

			[PXMergeAttributes(Method = MergeMethod.Append)]
			[PXRestrictor(typeof(Where<EPEmployee.userID, NotEqual<Current<AccessInfo.userID>>>), null)]
			protected virtual void _(Events.CacheAttached<EPWingman.wingmanID> e) { }

			#endregion

			protected virtual void _(Events.RowSelected<Users> e)
			{
				if (e.Row == null)
					return;

				// Acuminator disable once PX1049 DatabaseQueriesInRowSelected [query cache is used]
				Delegates.Cache.AllowSelect = Base.Employee.Select().ToList().Any();
			}

			#endregion
		}

		#endregion
	}

	public class CustomerManagementFeature : IBqlTable
	{
		public abstract class isInstalled : PX.Data.BQL.BqlBool.Field<isInstalled> { }
		[PXBool]
		public virtual bool? IsInstalled { get { return PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.customerModule>(); } }

		public abstract class isOutlookIntegrationInstalled : PX.Data.BQL.BqlBool.Field<isOutlookIntegrationInstalled> { }
		[PXBool]
		public virtual bool? IsOutlookIntegrationInstalled { get { return PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.outlookIntegration>(); } }
	}
}
