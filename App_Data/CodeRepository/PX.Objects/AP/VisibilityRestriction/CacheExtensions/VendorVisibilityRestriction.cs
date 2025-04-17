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
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;

namespace PX.Objects.AP
{
	public sealed class VendorVisibilityRestriction: PXCacheExtension<Vendor>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region VendorClassID
		// Acuminator disable once PX1030 PXDefaultIncorrectUse [DBField definded in the base DAC]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(typeof(Search2<APSetup.dfltVendorClassID,
			InnerJoin<VendorClass, On<VendorClass.vendorClassID, Equal<APSetup.dfltVendorClassID>>>,
			Where<VendorClass.orgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>>>))]
		[PXSelector(typeof(Search2<VendorClass.vendorClassID,
			LeftJoin<EPEmployeeClass, On<EPEmployeeClass.vendorClassID, Equal<VendorClass.vendorClassID>>>,
			Where<VendorClass.orgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>,
				And<Where<EPEmployeeClass.vendorClassID, IsNull, And<MatchUser>>>>>),
			DescriptionField = typeof(VendorClass.descr), CacheGlobal = true)]
		public string VendorClassID { get; set; }
		#endregion

		#region VOrgBAccountID
		public abstract class vOrgBAccountID : PX.Data.BQL.BqlInt.Field<vOrgBAccountID> { }
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(0, typeof(Search<VendorClass.orgBAccountID, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>))]
		public int? VOrgBAccountID { get; set; }
		#endregion

		#region ParentBAccountID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByUserBranches(typeof(BAccountR.cOrgBAccountID))]
		[RestrictVendorByUserBranches(typeof(BAccountR.vOrgBAccountID))]
		public int? ParentBAccountID { get; set; }
		#endregion

		#region PayToVendorID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictVendorByUserBranches(typeof(BAccountR.vOrgBAccountID))]
		public int? PayToVendorID { get; set; }
		#endregion
	}
}
