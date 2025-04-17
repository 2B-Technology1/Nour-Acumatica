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

namespace PX.Objects.CR.ContactMaint_Extensions
{
	/// <exclude/>
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class ContactMaint_MarketingListDetailsExt : MarketingListDetailsExt<ContactMaint, Contact, Contact.contactID>
	{
		#region Events

		[PXDBDefault(typeof(Contact.contactID))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CRMarketingListMember.contactID> e) { }

		protected virtual void _(Events.RowSelected<CRMarketingListMember> e)
		{
			CRMarketingListMember row = e.Row as CRMarketingListMember;

			if (row == null)
				return;

			CRMarketingList _CRMarketingList = PXSelect<CRMarketingList, Where<CRMarketingList.marketingListID,
				Equal<Required<CRMarketingList.marketingListID>>>>.Select(Base, row.MarketingListID);

			if (_CRMarketingList != null)
			{
				PXUIFieldAttribute.SetEnabled<CRMarketingList.marketingListID>(e.Cache, row, _CRMarketingList.Type == CRMarketingList.type.Static);
			}
		}

		#endregion
	}
}
