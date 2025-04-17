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
using System.Security.Permissions;
using PX.Objects.SP.DAC;
using PX.SM;
using PX.Data;
using SP.Objects.SP;


namespace SP.Objects
{
	public class SMAccessPersonalMaintExt : PXGraphExtension<SMAccessPersonalMaint>
	{
		public delegate int? GetDefaultBranchIdDel1(string username, string companyId);

		[SecurityPermission(SecurityAction.Assert, Unrestricted = true)]
		[PXOverride]
		public virtual int? GetDefaultBranchId(string username, string companyId, GetDefaultBranchIdDel1 baseMethod)
		{
			var setup = PortalSetup.Current;
			if (setup != null)
			{
				if (setup.SellingBranchID != null)
				{
					return setup.SellingBranchID;
				}
				else if (setup.DisplayFinancialDocuments == FinancialDocumentsFilterAttribute.BY_BRANCH)
				{
					return setup.RestrictByBranchID;
				}
			}

			return baseMethod(username, companyId);
		}

		public delegate int? GetDefaultBranchIdDel2();

		[PXOverride]
		public virtual int? GetDefaultBranchId(GetDefaultBranchIdDel2 baseMethod)
		{
			var setup = PortalSetup.Current;
			if (setup != null)
			{
				if (setup.SellingBranchID != null)
				{
					return setup.SellingBranchID;
				}
				else if (setup.DisplayFinancialDocuments == FinancialDocumentsFilterAttribute.BY_BRANCH)
				{
					return setup.RestrictByBranchID;
				}
			}

			return baseMethod();
		}
	}
}