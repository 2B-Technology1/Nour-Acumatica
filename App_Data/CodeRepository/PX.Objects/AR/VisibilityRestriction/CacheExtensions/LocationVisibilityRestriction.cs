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
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.DAC;

namespace PX.Objects.CR
{
	public sealed class LocationVisibilityRestriction : PXCacheExtension<Location>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region BAccountID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByUserBranches]
		public int? BAccountID { get; set; }
		#endregion

		#region CBranchID
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		//have to join BAccountR table for restrictor message parameter
		[Branch(searchType: typeof(Search2<Branch.branchID,
					InnerJoin<Organization,
						On<Branch.organizationID, Equal<Organization.organizationID>>,
					InnerJoin<BAccountR,
						On<BAccountR.bAccountID, Equal<Current<BAccountR.bAccountID>>>>>,
					Where<MatchWithBranch<Branch.branchID>>>),
				useDefaulting: false,
				IsDetail = false,
				PersistingCheck = PXPersistingCheck.Nothing,
				DisplayName = "Shipping Branch",
				IsEnabledWhenOneBranchIsAccessible = true)]
		[PXRestrictor(typeof(Where<Branch.branchID, Inside<Current<BAccountR.cOrgBAccountID>>>),
			AR.Messages.BranchRestrictedByCustomer, new[] { typeof(BAccountR.acctCD), typeof(Branch.branchCD) })]
		[PXDefault(typeof(Search2<Branch.branchID,
				InnerJoin<BAccountR, On<BAccountR.bAccountID, Equal<Current<Location.bAccountID>>,
					And<Branch.bAccountID, Equal<BAccountR.cOrgBAccountID>>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		public int? CBranchID { get; set; }
		#endregion

		#region VBranchID
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		//have to join BAccountR table for restrictor message parameter
		[Branch(searchType: typeof(Search2<Branch.branchID,
					InnerJoin<Organization,
						On<Branch.organizationID, Equal<Organization.organizationID>>,
					InnerJoin<BAccountR,
						On<BAccountR.bAccountID, Equal<Current<BAccountR.bAccountID>>>>>,
					Where<MatchWithBranch<Branch.branchID>>>),
				useDefaulting: false,
				IsDetail = false,
				PersistingCheck = PXPersistingCheck.Nothing,
				DisplayName = "Receiving Branch",
				IsEnabledWhenOneBranchIsAccessible = true)]
		[PXRestrictor(typeof(Where<Branch.branchID, Inside<Current<BAccountR.vOrgBAccountID>>>),
			AP.Messages.BranchRestrictedByVendor, new[] { typeof(BAccountR.acctCD), typeof(Branch.branchCD) })]
		[PXDefault(typeof(Search2<Branch.branchID,
				InnerJoin<BAccountR, On<BAccountR.bAccountID, Equal<Current<Location.bAccountID>>,
					And<Branch.bAccountID, Equal<BAccountR.vOrgBAccountID>>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		public int? VBranchID { get; set; }
		#endregion
	}
}
