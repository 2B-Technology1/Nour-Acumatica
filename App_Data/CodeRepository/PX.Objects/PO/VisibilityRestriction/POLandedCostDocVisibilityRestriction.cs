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
using PX.Objects.Common.Formula;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.GL;

namespace PX.Objects.PO
{
	public sealed class POLandedCostDocVisibilityRestriction : PXCacheExtension<POLandedCostDoc>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region BranchID
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[Branch(IsDetail = false)]
		[PXFormula(typeof(Switch<
			Case<Where<IsCopyPasteContext, Equal<True>, And<Current2<POLandedCostDoc.branchID>, IsNotNull>>, Current2<POLandedCostDoc.branchID>,
			Case<Where<POLandedCostDoc.vendorLocationID, IsNotNull,
					And<Selector<POLandedCostDoc.vendorLocationID, Location.vBranchID>, IsNotNull>>,
				Selector<POLandedCostDoc.vendorLocationID, Location.vBranchID>,
				Case<Where<POLandedCostDoc.vendorID, IsNotNull,
						And<Not<Selector<POLandedCostDoc.vendorID, Vendor.vOrgBAccountID>, RestrictByBranch<Current2<POLandedCostDoc.branchID>>>>>,
					Null,
					Case<Where<Current2<POLandedCostDoc.branchID>, IsNotNull>,
						Current2<POLandedCostDoc.branchID>>>>>,
			Current<AccessInfo.branchID>>))]
		public int? BranchID { get; set; }
		#endregion

		#region VendorID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictVendorByBranch(branchID: typeof(POLandedCostDoc.branchID), ResetVendor = false)]
		public int? VendorID { get; set; }
		#endregion
	}
}