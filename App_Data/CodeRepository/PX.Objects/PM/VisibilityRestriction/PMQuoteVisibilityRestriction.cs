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
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CR.Standalone;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.PM
{
	public sealed class PMQuoteVisibilityRestriction : PXCacheExtension<PMQuote>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region BranchID
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[Branch(IsDetail = false, TabOrder = 0, BqlField = typeof(CROpportunityRevision.branchID))]
		[PXFormula(typeof(Switch<
				Case<Where<PendingValue<CROpportunityRevision.branchID>, IsNotNull>, Null,
				Case<Where<CROpportunityRevision.locationID, IsNotNull,
					   And<Selector<CROpportunityRevision.locationID, CR.Location.cBranchID>, IsNotNull>>,
					Selector<CROpportunityRevision.locationID, CR.Location.cBranchID>,
				Case<Where<CROpportunityRevision.bAccountID, IsNotNull,
					   And<Not<Selector<CROpportunityRevision.bAccountID, BAccount.cOrgBAccountID>, RestrictByBranch<Current2<CROpportunityRevision.branchID>>>>>,
					Null,
				Case<Where<Current2<CROpportunityRevision.branchID>, IsNotNull>,
					Current2<CROpportunityRevision.branchID>>>>>,
				Current<AccessInfo.branchID>>))]
		public int? BranchID { get; set; }

		#endregion
		#region BAccountID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByBranch(typeof(BAccountR.cOrgBAccountID), typeof(PMQuote.branchID))]
		public int? BAccountID { get; set; }
		#endregion
	}
}
