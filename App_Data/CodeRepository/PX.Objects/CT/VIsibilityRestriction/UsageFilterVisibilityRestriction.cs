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
using static PX.Objects.CT.UsageMaint;

namespace PX.Objects.CT
{
	public sealed class UsageFilterVisibilityRestriction : PXCacheExtension<UsageFilter>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region ContractID
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXSelector(typeof(Search2<Contract.contractID,
					LeftJoin<BAccountR, On<Contract.customerID, Equal<BAccountR.bAccountID>>>,
					Where<Contract.baseType, Equal<CTPRType.contract>,
						And<Contract.status, NotEqual<Contract.status.draft>,
						And<Contract.status, NotEqual<Contract.status.inApproval>>>>>),
			SubstituteKey = typeof(Contract.contractCD), DescriptionField = typeof(Contract.description))]
		[RestrictCustomerByUserBranches(typeof(BAccountR.cOrgBAccountID))]
		public int? ContractID { get; set; }
		#endregion
	}
}
