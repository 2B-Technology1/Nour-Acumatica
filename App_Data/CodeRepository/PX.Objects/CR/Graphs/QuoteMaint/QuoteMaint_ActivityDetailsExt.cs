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

namespace PX.Objects.CR.QuoteMaint_Extensions
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class QuoteMaint_ActivityDetailsExt_Actions : ActivityDetailsExt_Actions<QuoteMaint_ActivityDetailsExt, QuoteMaint, CRQuote, CRQuote.noteID> { }

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class QuoteMaint_ActivityDetailsExt : ActivityDetailsExt<QuoteMaint, CRQuote, CRQuote.noteID>
	{
		public override Type GetBAccountIDCommand() => typeof(CRQuote.bAccountID);
		public override Type GetContactIDCommand() => typeof(CRQuote.contactID);

		public override Type GetEmailMessageTarget() => typeof(Select<CRContact, Where<CRContact.contactID, Equal<Current<CRQuote.opportunityContactID>>>>);

		public override string GetCustomMailTo()
		{
			var current = Base.Quote.Current;
			if (current == null)
				return null;

			var contact = current.OpportunityContactID.With(_ => CRContact.PK.Find(Base, _.Value));

			return
				!string.IsNullOrWhiteSpace(contact?.Email)
					? PXDBEmailAttribute.FormatAddressesWithSingleDisplayName(contact.Email, contact.DisplayName)
					: null;
		}
	}
}
