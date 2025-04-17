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
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CA;
using System.Collections;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.GL.Consolidation
{
	public class ConsolAccountMaint : PXGraph<ConsolAccountMaint>
	{
		public PXSavePerRow<GLConsolAccount, GLConsolAccount.accountCD> Save;
		public PXCancel<GLConsolAccount> Cancel;
		[PXImport(typeof(GLConsolAccount))]
		[PXFilterable]
		public PXSelect<GLConsolAccount, Where<True, Equal<True>>, OrderBy<Asc<GLConsolAccount.accountCD>>> AccountRecords;

		public PXSelect<Account, Where<Account.gLConsolAccountCD, Equal<Required<Account.gLConsolAccountCD>>>> Accounts;

		public ConsolAccountMaint()
		{
		}

		protected virtual void GLConsolAccount_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			GLConsolAccount consol = (GLConsolAccount)e.Row;
			foreach (Account acct in Accounts.Select(consol.AccountCD))
			{
				acct.GLConsolAccountCD = null;
				Accounts.Update(acct);
			}
		}
	}
}
