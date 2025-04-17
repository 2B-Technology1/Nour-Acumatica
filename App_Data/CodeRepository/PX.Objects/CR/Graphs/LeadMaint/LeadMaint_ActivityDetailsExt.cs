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

namespace PX.Objects.CR.LeadMaint_Extensions
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class LeadMaint_ActivityDetailsExt_Actions : ActivityDetailsExt_Actions<LeadMaint_ActivityDetailsExt, LeadMaint, CRLead, CRLead.noteID>
	{
		public override bool IsPinActivityAvailable() => true;
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class LeadMaint_ActivityDetailsExt : ActivityDetailsExt<LeadMaint, CRLead, CRLead.noteID>
	{
		public override Type GetBAccountIDCommand() => typeof(CRLead.bAccountID);
		public override Type GetContactIDCommand() => typeof(CRLead.refContactID);

		public override string GetCustomMailTo()
		{
			var contact = Base.Lead.Current;

			return
				!string.IsNullOrWhiteSpace(contact?.EMail)
					? PXDBEmailAttribute.FormatAddressesWithSingleDisplayName(contact.EMail, contact.DisplayName)
					: null;
		}

		[PXDBChildIdentity(typeof(CRLead.contactID))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CRPMTimeActivity.contactID> e) { }

		[PopupMessage]
		[PXDBDefault(typeof(CRLead.bAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CRPMTimeActivity.bAccountID> e) { }

		protected virtual void _(Events.RowSelected<CRLead> e)
		{
			if (e.Row == null)
				return;

			var leadClass = CRLead.FK.Class.FindParent(Base, e.Row);
			if (leadClass != null)
			{
				this.DefaultEmailAccountID = leadClass.DefaultEMailAccountID;
			}
		}
	}
}
