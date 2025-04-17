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

namespace PX.Objects.AP
{
	/// <summary>
	/// DAC Cache extension for the <see cref="APPPDVATAdjParameters"/> that manages visibility by Vendor.
	/// </summary>
	public sealed class APPPDVATAdjParametersVisibilityRestriction : PXCacheExtension<APPPDVATAdjParameters>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region VendorID
		/// <summary>
		/// The identifier of the Vendor related to the <see cref="Vendor.bAccountID"/> 
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictVendorByBranch(branchID: typeof(APPPDVATAdjParameters.branchID), ResetVendor = true)]
		public int? VendorID { get; set; }
		#endregion
	}
}
