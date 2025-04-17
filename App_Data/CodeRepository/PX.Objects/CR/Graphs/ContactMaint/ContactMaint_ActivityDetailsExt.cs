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

namespace PX.Objects.CR.ContactMaint_Extensions
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class ContactMaint_ActivityDetailsExt_Actions : ActivityDetailsExt_Actions<ContactMaint_ActivityDetailsExt, ContactMaint, Contact> { }

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class ContactMaint_ActivityDetailsExt : ActivityDetailsExt<ContactMaint, Contact>
	{
		public override Type GetLinkConditionClause() => typeof(Where<CRPMTimeActivity.contactID, Equal<Current<Contact.contactID>>>);

		public override Type GetBAccountIDCommand() => typeof(Contact.bAccountID);
		public override Type GetContactIDCommand() => typeof(Contact.contactID);

		public override string GetCustomMailTo()
		{
			var contact = Base.Contact.Current;

			return
				!string.IsNullOrWhiteSpace(contact?.EMail)
					? PXDBEmailAttribute.FormatAddressesWithSingleDisplayName(contact.EMail, contact.DisplayName)
					: null;
		}

		[PXDBChildIdentity(typeof(Contact.contactID))]
		[PXSelector(typeof(Contact.contactID), DescriptionField = typeof(Contact.memberName), DirtyRead = true)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CRPMTimeActivity.contactID> e) { }

		[PXDBDefault(typeof(Contact.bAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CRPMTimeActivity.bAccountID> e) { }

		protected virtual void _(Events.RowSelected<Contact> e)
		{
			if (e.Row == null)
				return;

			var contactClass = Contact.FK.Class.FindParent(Base, e.Row);
			if (contactClass != null)
			{
				this.DefaultEmailAccountID = contactClass.DefaultEMailAccountID;
			}
		}
	}
}
