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

using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CR;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.AP.InvoiceRecognition.VendorSearch
{
	internal class ContactRepository : IContactRepository
	{
		public Contact GetAccountContact(PXGraph graph, int baccountId, int defContactId)
		{
			return SelectFrom<Contact>.
				Where<Contact.bAccountID.IsEqual<@P.AsInt>.
					And<Contact.contactID.IsEqual<@P.AsInt>>>.
				View.ReadOnly.Select(graph, baccountId, defContactId);
		}

		public Contact GetPrimaryContact(PXGraph graph, int baccountId, int primaryContactID)
		{
			return SelectFrom<Contact>.
				Where<Contact.bAccountID.IsEqual<@P.AsInt>.
					And<Contact.contactType.IsEqual<ContactTypesAttribute.person>>.
					And<Contact.contactID.IsEqual<@P.AsInt>>>.
				View.ReadOnly.Select(graph, baccountId, primaryContactID);
		}

		public List<string> GetOtherContactEmails(PXGraph graph, int baccountId, Contact accountContact, Contact primaryContact)
		{
			return SelectFrom<Contact>.
				Where<Contact.bAccountID.IsEqual<@P.AsInt>.
					And<Contact.contactType.IsEqual<ContactTypesAttribute.person>>>.
				View.ReadOnly.Select(graph, baccountId).FirstTableItems
				.Where(c => c.ContactID != accountContact?.ContactID && c.ContactID != primaryContact?.ContactID && !string.IsNullOrEmpty(c.EMail))
				.Select(c => c.EMail)
				.ToList();
		}
	}
}
