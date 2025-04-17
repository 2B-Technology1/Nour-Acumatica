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
using PX.Common;
using PX.Data;
using PX.Objects.CR.Extensions;

namespace PX.Objects.CR.CRCaseMaint_Extensions
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class CRCaseMaint_ActivityDetailsExt_Actions : ActivityDetailsExt_Actions<CRCaseMaint_ActivityDetailsExt, CRCaseMaint, CRCase, CRCase.noteID>
	{
		public override bool IsPinActivityAvailable() => true;
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class CRCaseMaint_ActivityDetailsExt : ActivityDetailsExt<CRCaseMaint, CRCase, CRCase.noteID>
	{
		public override Type GetBAccountIDCommand() => typeof(CRCase.customerID);
		public override Type GetContactIDCommand() => typeof(CRCase.contactID);

		public override string GetCustomMailTo()
		{
			var current = Base.Case.Current;
			if (current == null)
				return null;

			var contact = current.ContactID.With(_ => Contact.PK.Find(Base, _.Value));

			if (!string.IsNullOrWhiteSpace(contact?.EMail))
				return PXDBEmailAttribute.FormatAddressesWithSingleDisplayName(contact.EMail, contact.DisplayName);

			var customerContact = current.CustomerID.With(_ => (PXResult<Contact, BAccount>)
					PXSelectJoin<
							Contact,
							InnerJoin<BAccount,
								On<BAccount.defContactID, Equal<Contact.contactID>>>,
							Where<
								BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>
						.Select(Base, _.Value))
				.With(_ => (Contact)_);

			return
				!string.IsNullOrWhiteSpace(customerContact?.EMail)
					? PXDBEmailAttribute.FormatAddressesWithSingleDisplayName(customerContact.EMail, customerContact.DisplayName)
					: null;
		}

		protected virtual void _(Events.RowSelected<CRCase> e)
		{
			if (e.Row == null)
				return;

			this.DefaultSubject = PXMessages.LocalizeFormatNoPrefixNLA(Messages.CaseEmailDefaultSubject, e.Row.CaseCD, e.Row.Subject);

			var caseClass = CRCase.FK.Class.FindParent(Base, e.Row);
			if (caseClass != null)
			{
				this.DefaultEmailAccountID = caseClass.DefaultEMailAccountID;
			}

			Activities.Cache.AllowInsert = e.Row.Released != true;
		}
	}
}
