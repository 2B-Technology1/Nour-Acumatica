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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.CS;
using PX.Data.BQL;
using PX.Objects.CR;

namespace PX.Objects.CA
{
	public sealed class CashFlowEnqVisibilityRestriction : PXCacheExtension<CashFlowEnq.CashFlowFilter>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.branch>();
		}

		#region AccountID
		[PXRestrictor(
			typeof(Where2<Where<CashAccount.restrictVisibilityWithBranch, Equal<True>, 
				And<CashAccount.branchID, InsideBranchesOf<Current<CashFlowEnq.CashFlowFilter.orgBAccountID>>>>, 
				Or<Where<CashAccount.restrictVisibilityWithBranch, Equal<False>, 
					And<CashAccount.baseCuryID, Equal<Current<CashFlowEnq.CashFlowFilter.organizationBaseCuryID>>>>>
				>),
			"",
			SuppressVerify = false
		)]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		public int? CashAccountID
		{
			get;
			set;
		}
		#endregion
		#region DefaultAccountID
		[PXRestrictor(
			typeof(Where2<Where<CashAccount.restrictVisibilityWithBranch, Equal<True>,
				And<CashAccount.branchID, InsideBranchesOf<Current<CashFlowEnq.CashFlowFilter.orgBAccountID>>>>,
				Or<Where<CashAccount.restrictVisibilityWithBranch, Equal<False>,
					And<CashAccount.baseCuryID, Equal<Current<CashFlowEnq.CashFlowFilter.organizationBaseCuryID>>>>>
				>),
			"",
			SuppressVerify = false
		)]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		public int? DefaultAccountID
		{
			get;
			set;
		}
		#endregion
	}
}
