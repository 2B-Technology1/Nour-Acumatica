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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CR.Extensions;

namespace PX.Objects.CR.OpportunityMaint_Extensions
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class OpportunityMaint_ActivityDetailsExt_Actions : ActivityDetailsExt_Actions<OpportunityMaint_ActivityDetailsExt, OpportunityMaint, CROpportunity, CROpportunity.noteID>
	{
		public override bool IsPinActivityAvailable() => true;
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class OpportunityMaint_ActivityDetailsExt : ActivityDetailsExt<OpportunityMaint, CROpportunity, CROpportunity.noteID>
	{
		public override Type GetLinkConditionClause() => typeof(Where<
			CRPMTimeActivity.refNoteID.IsIn<@P.AsGuid>
			.Or<
				Brackets<
					CROpportunityClass.showContactActivities.FromCurrent.IsEqual<True>
					.And<CRPMTimeActivity.refNoteID.IsEqual<CROpportunity.leadID.FromCurrent>>
				>
			>>);

		public virtual IEnumerable activities()
		{
			if (Base?.OpportunityCurrent?.Current?.NoteID == null)
			{
				return Enumerable.Empty<CRPMTimeActivity>();
			}

			List<object> noteIDs = new List<object>()
				{
					Base.OpportunityCurrent.Current.NoteID
				};
			foreach (CRQuote item in Base.Quotes.Select())
			{
				noteIDs.Add(item.QuoteID);
			}
			return Activities.View.QuickSelect(new object[] { noteIDs.ToArray() });
		}

		public override Type GetBAccountIDCommand() => typeof(CROpportunity.bAccountID);
		public override Type GetContactIDCommand() => typeof(Select<
			Contact,
			Where<
				Current<CROpportunity.allowOverrideContactAddress>, NotEqual<True>,
				And<Contact.contactID, Equal<Current<CROpportunity.contactID>>>>>);

		public override string GetCustomMailTo()
		{
			var current = Base.Opportunity.Current;
			if (current == null)
				return null;

			var contact = current.OpportunityContactID.With(_ => CRContact.PK.Find(Base, _.Value));

			return
				!string.IsNullOrWhiteSpace(contact?.Email)
					? PXDBEmailAttribute.FormatAddressesWithSingleDisplayName(contact.Email, contact.DisplayName)
					: null;
		}

		protected virtual void _(Events.RowSelected<CROpportunity> e)
		{
			if (e.Row == null)
				return;

			var oppClass = CROpportunity.FK.Class.FindParent(Base, e.Row);
			if (oppClass != null)
			{
				this.DefaultEmailAccountID = oppClass.DefaultEMailAccountID;
			}
		}
	}
}
