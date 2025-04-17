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
using PX.Objects.CS;
using PX.Objects.GL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM.GraphExtensions
{
	public class AccountMaintExt : PXGraphExtension<AccountMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.projectAccounting>();
		}

		protected virtual void _(Events.RowDeleting<Account> e)
		{
			if (e.Row.AccountGroupID != null)
			{
				PMAccountGroup accountGroup = PXSelect<PMAccountGroup, Where<PMAccountGroup.groupID, Equal<Required<PMAccountGroup.groupID>>>>.Select(Base, e.Row.AccountGroupID);
				if (accountGroup != null)
				{
					throw new PXException(Messages.CannotDeleteAccountMappedToAG, e.Row.AccountCD, accountGroup.GroupCD);
				}
			}
		}

		protected virtual void _(Events.RowUpdating<Account> e)
		{
			if (e.NewRow == null) return;
			var account = e.NewRow;
			var oldAccount = e.Row;
			if (oldAccount != null && oldAccount.AccountGroupID.HasValue && account.AccountGroupID == null)
			{
				PMSetup setup = PXSelect<PMSetup>.Select(Base);
				if (setup != null && account.AccountID == setup.UnbilledRemainderAccountID)
				{
					throw new PXSetPropertyException(Messages.CannotDeleteAccountFromAccountGroup, PXErrorLevel.Error);
				}
			}
		}
	}
}
