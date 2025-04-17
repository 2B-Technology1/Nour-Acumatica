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
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.AP;

namespace PX.Objects.CA
{
	public sealed class CASplitExtVisibilityRestiriction : PXCacheExtension<CASplitExt>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region ReferenceID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByBranch(typeof(BAccountR.cOrgBAccountID),
			WhereType: typeof(Or<Current<PaymentReclassifyProcess.Filter.showReclassified>, Equal<True>>),
			branchID: typeof(PaymentReclassifyProcess.Filter.branchID),
			ResetCustomer = true)]
		[RestrictVendorByBranch(typeof(BAccountR.vOrgBAccountID),
			WhereType: typeof(Or<Current<PaymentReclassifyProcess.Filter.showReclassified>, Equal<True>>),
			branchID: typeof(PaymentReclassifyProcess.Filter.branchID),
			ResetVendor = true)]
		public int? ReferenceID { get; set; }
		#endregion
	}
}
