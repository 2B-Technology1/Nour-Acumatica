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
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.Common;

namespace PX.Objects.AP.Standalone
{
	public sealed class APQuickCheckVisibilityRestriction : PXCacheExtension<APQuickCheck>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region BranchID
		[Branch(typeof(AccessInfo.branchID), IsDetail = false, TabOrder = 0)]
		[PXFormula(typeof(Switch<
				Case<Where<PendingValue<APQuickCheck.branchID>, IsPending>, Null,
				Case<Where<APQuickCheck.vendorLocationID, IsNotNull,
						And<Selector<APQuickCheck.vendorLocationID, Location.vBranchID>, IsNotNull>>,
					Selector<APQuickCheck.vendorLocationID, Location.vBranchID>,
				Case<Where<APQuickCheck.vendorID, IsNotNull,
						And<Not<Selector<APQuickCheck.vendorID, Vendor.vOrgBAccountID>, RestrictByBranch<Current2<APQuickCheck.branchID>>>>>,
					Null,
				Case<Where<Current2<APQuickCheck.branchID>, IsNotNull>,
					Current2<APQuickCheck.branchID>>>>>,
				Current<AccessInfo.branchID>>))]
		public int? BranchID { get; set; }
		#endregion

		#region VendorID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictVendorByBranch(branchID: typeof(APQuickCheck.branchID))]
		public int? VendorID { get; set; }
		#endregion
	}
}
