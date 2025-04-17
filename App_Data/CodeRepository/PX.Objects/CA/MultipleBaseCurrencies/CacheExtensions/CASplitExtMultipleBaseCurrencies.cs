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
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using System;

namespace PX.Objects.CA
{
	[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2022R2)]

	public sealed class CASplitExtMultipleBaseCurrencies : PXCacheExtension<CASplitExtVisibilityRestiriction, CASplitExt>
	{
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2022R2)]
		public static bool IsActive()
		{
			return false;
		}

		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2022R2)]
		#region ReferenceID
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[RestrictCustomerByBranch(typeof(BAccountR.cOrgBAccountID), branchID: typeof(PaymentReclassifyProcess.Filter.branchID))]
		[RestrictVendorByBranch(typeof(BAccountR.vOrgBAccountID), branchID: typeof(PaymentReclassifyProcess.Filter.branchID))]
		public int? ReferenceID { get; set; }
		#endregion
	}
}
