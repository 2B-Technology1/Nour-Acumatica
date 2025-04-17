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
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.AP
{
	public sealed class VendorRVisibilityRestriction : PXCacheExtension<VendorR>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region AcctCD
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[VendorRaw(typeof(
				Where2<
					Where<Vendor.type, Equal<BAccountType.vendorType>,
						Or<Vendor.type, Equal<BAccountType.combinedType>>>,
					And<Where<Vendor.vOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>>>>),
				DescriptionField = typeof(Vendor.acctName),
				IsKey = true, DisplayName = "Vendor ID")]
		public string AcctCD { get; set; }
		#endregion
	}
}
