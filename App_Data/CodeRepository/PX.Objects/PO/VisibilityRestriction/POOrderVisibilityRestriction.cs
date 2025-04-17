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
using PX.Objects.Common.Formula;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.PO
{
	public sealed class POOrderVisibilityRestriction : PXCacheExtension<POOrder>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region BranchID
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[Branch(typeof(AccessInfo.branchID), IsDetail = false, TabOrder = 0)]
		[PXFormula(typeof(Switch<
			Case<Where<IsCopyPasteContext, Equal<True>, And<Current2<POOrder.branchID>, IsNotNull>>, Current2<POOrder.branchID>,
			Case<Where<POOrder.vendorLocationID, IsNotNull,
					And<Selector<POOrder.vendorLocationID, Location.vBranchID>, IsNotNull>>,
				Selector<POOrder.vendorLocationID, Location.vBranchID>,
				Case<Where<POOrder.vendorID, IsNotNull, 
						And<Not<Selector<POOrder.vendorID, Vendor.vOrgBAccountID>, RestrictByBranch<Current2<POOrder.branchID>>>>>,
					Null,
					Case<Where<Current2<POOrder.branchID>, IsNotNull>,
						Current2<POOrder.branchID>>>>>,
			Current<AccessInfo.branchID>>))]
		public int? BranchID{get; set;}
		#endregion

		#region ShipToBAccountID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByUserBranches(typeof(BAccount2.cOrgBAccountID))]
		public int? ShipToBAccountID { get; set; }
		#endregion

		#region VendorID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictVendorByBranch(branchID: typeof(POOrder.branchID), ResetVendor = false)]
		public int? VendorID { get; set; }
		#endregion

		#region PayToVendorID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictVendorByBranch(branchID: typeof(POOrder.branchID), ResetVendor = false)]
		public int? PayToVendorID { get; set; }
		#endregion
	}
}