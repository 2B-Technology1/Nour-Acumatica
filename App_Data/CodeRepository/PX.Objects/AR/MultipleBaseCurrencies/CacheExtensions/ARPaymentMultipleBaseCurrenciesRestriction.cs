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
using PX.Data;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.CR;

namespace PX.Objects.AR
{
	public sealed class ARPaymentMultipleBaseCurrenciesRestriction : PXCacheExtension<ARPaymentVisibilityRestriction, ARPayment>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();
		}

		#region BranchID
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[Branch(typeof(AccessInfo.branchID), IsDetail = false, TabOrder = 0)]
		[PXFormula(typeof(Switch<
				Case<Where<PendingValue<ARPayment.branchID>, IsPending>, Null,
				Case<Where<ARPayment.customerLocationID, IsNull, And<UnattendedMode, Equal<True>>>, Null, 
				Case<Where<Selector<ARPayment.customerLocationID, Location.cBranchID>, IsNotNull>,
					Selector<ARPayment.customerLocationID, Location.cBranchID>,
				Case<Where<ARPayment.customerID, IsNotNull,
						And<Not<Selector<ARPayment.customerID, Customer.cOrgBAccountID>, RestrictByBranch<Current2<ARPayment.branchID>>>>>,
					Null,
				Case<Where<Current2<ARPayment.branchID>, IsNotNull>,
					Current2<ARPayment.branchID>,
				Case<Where<ARPayment.customerID, IsNotNull, 
						And<Selector<ARPayment.customerID, Customer.baseCuryID>, IsNotNull,
						And<Selector<ARPayment.customerID, Customer.baseCuryID>, NotEqual<Current<AccessInfo.baseCuryID>>>>>,
					Null>>>>>>, 
				Current<AccessInfo.branchID>>))]
		public Int32? BranchID{get;	set;}
		#endregion

		#region CustomerID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<Current2<branchBaseCuryID>, IsNull,
			Or<Customer.baseCuryID, IsNull,
			Or<Customer.baseCuryID, Equal<Current2<branchBaseCuryID>>>>>), null)]
		public int? CustomerID { get; set; }
		#endregion

		#region BranchBaseCuryID
		public new abstract class branchBaseCuryID : PX.Data.BQL.BqlString.Field<branchBaseCuryID> { }

		[PXString]
		[PXFormula(typeof(Selector<ARPayment.branchID, Branch.baseCuryID>))]
		public string BranchBaseCuryID { get; set; }
		#endregion
		#region CustomerBaseCuryID
		public new abstract class customerBaseCuryID : PX.Data.BQL.BqlString.Field<customerBaseCuryID> { }

		[PXString]
		[PXFormula(typeof(Selector<ARPayment.customerID, Customer.baseCuryID>))]
		public string CustomerBaseCuryID { get; set; }
		#endregion
	}
}
