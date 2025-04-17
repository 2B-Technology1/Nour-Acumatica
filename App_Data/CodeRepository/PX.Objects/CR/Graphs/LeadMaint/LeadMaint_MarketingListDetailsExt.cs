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
using PX.Objects.CR.Extensions;
using System;

namespace PX.Objects.CR.LeadMaint_Extensions
{
	/// <exclude/>
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class LeadMaint_MarketingListDetailsExt : MarketingListDetailsExt<LeadMaint, CRLead, CRLead.contactID>
	{
		#region Events

		[PXDBDefault(typeof(CRLead.contactID))]
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Name")]
		[PXSelector(typeof(Search<CRLead.contactID>),
			typeof(CRLead.fullName),
			typeof(CRLead.displayName),
			typeof(CRLead.eMail),
			typeof(CRLead.phone1),
			typeof(CRLead.bAccountID),
			typeof(CRLead.salutation),
			typeof(CRLead.contactType),
			typeof(CRLead.isActive),
			typeof(CRLead.memberName),
			DescriptionField = typeof(CRLead.memberName),
			Filterable = true,
			DirtyRead = true)]
		[PXParent(typeof(Select<CRLead, Where<CRLead.contactID, Equal<Current<CRMarketingListMember.contactID>>>>))]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		protected virtual void _(Events.CacheAttached<CRMarketingListMember.contactID> e) { }

		#endregion
	}
}
