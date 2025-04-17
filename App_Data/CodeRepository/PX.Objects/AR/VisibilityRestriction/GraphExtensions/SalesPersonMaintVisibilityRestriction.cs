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

namespace PX.Objects.AR
{
	public class SalesPersonMaintVisibilityRestriction : PXGraphExtension<SalesPersonMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		public PXSelectJoin<CustSalesPeople,
			InnerJoin<Customer, On<Customer.bAccountID, Equal<CustSalesPeople.bAccountID>>>,
			Where<CustSalesPeople.salesPersonID, Equal<Current<SalesPerson.salesPersonID>>,
				And<Customer.cOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>,
				And<Match<Customer, Current<AccessInfo.userName>>>>>> SPCustomers;
	}

	public sealed class CustSalesPeopleVisibilityRestriction : PXCacheExtension<CustSalesPeople>
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
	}
}
