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

using PX.Data;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace PX.Objects.CA.BankFeed
{
	internal class BankFeedUserDataProvider
	{
		public virtual string GetUserForOrganization(int organizationId)
		{
			var collection = Definition.GetOrganizationToUserMapFromSlot();
			collection.TryGetValue(organizationId, out string userId);
			return userId;
		}

		private class Definition : IPrefetchable
		{
			const string key = "BankFeedUserCollection";
			public IReadOnlyDictionary<int, string> OrganizationToUserMap { get; private set; }

			public void Prefetch()
			{
				OrganizationToUserMap = PXDatabase.Select<CABankFeedUser>().RowCast<CABankFeedUser>()
					.ToDictionary(i => i.OrganizationID.Value, i => i.ExternalUserID);
			}

			public static IReadOnlyDictionary<int, string> GetOrganizationToUserMapFromSlot()
			{
				var def = PXDatabase.GetSlot<Definition>(key, typeof(CABankFeedUser));
				return def.OrganizationToUserMap;
			}
		}
	}
}
