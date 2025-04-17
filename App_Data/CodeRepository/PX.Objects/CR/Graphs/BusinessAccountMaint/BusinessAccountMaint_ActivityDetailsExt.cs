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
using PX.Data;
using PX.Objects.CR.Extensions;

namespace PX.Objects.CR.BusinessAccountMaint_Extensions
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class BusinessAccountMaint_ActivityDetailsExt_Actions : ActivityDetailsExt_Actions<BusinessAccountMaint_ActivityDetailsExt, BusinessAccountMaint, BAccount> { }

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class BusinessAccountMaint_ActivityDetailsExt : ActivityDetailsExt<BusinessAccountMaint, BAccount>
	{
		public override Type GetLinkConditionClause() => typeof(Where<CRPMTimeActivity.bAccountID, Equal<Current<BAccount.bAccountID>>>);

		public override Type GetBAccountIDCommand() => typeof(BAccount.bAccountID);

		public override string GetCustomMailTo()
		{
			var current = Base.BAccount.Current;
			if (current == null)
				return null;

			var contact = Contact.PK.Find(Base, current.DefContactID);

			return
				!string.IsNullOrWhiteSpace(contact?.EMail)
					? PXDBEmailAttribute.FormatAddressesWithSingleDisplayName(contact.EMail, contact.DisplayName)
					: null;
		}

		[PXSelector(typeof(Contact.contactID), DescriptionField = typeof(Contact.memberName))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CRPMTimeActivity.contactID> e) { }

		[PXDBDefault(typeof(BAccount.bAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBChildIdentity(typeof(BAccount.bAccountID))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CRPMTimeActivity.bAccountID> e) { }

		protected virtual void _(Events.RowSelected<BAccount> e)
		{
			if (e.Row == null)
				return;

			var accountClass = BAccount.FK.Class.FindParent(Base, e.Row);
			if (accountClass != null)
			{
				this.DefaultEmailAccountID = accountClass.DefaultEMailAccountID;
			}
		}
	}
}
