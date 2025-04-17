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
using System.Collections;

namespace PX.Objects.AR
{
	public class ARIntegrityCheckVisibilityRestriction : PXGraphExtension<ARIntegrityCheck>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerClassByUserBranches]
		protected virtual void ARIntegrityCheckFilter_CustomerClassID_CacheAttached(PXCache sender) { }

		public PXFilteredProcessing<Customer, ARIntegrityCheckFilter,
			Where<Customer.cOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>,
			And<Match<Current<AccessInfo.userName>>>>> ARCustomerList;

		public PXSelect<Customer,
			Where<Customer.customerClassID, Equal<Current<ARIntegrityCheckFilter.customerClassID>>,
			And<Customer.cOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>,
			And<Match<Current<AccessInfo.userName>>>
			>>> Customer_ClassID;

		protected IEnumerable arcustomerlist()
		{
			if (Base.Filter.Current != null && Base.Filter.Current.CustomerClassID != null)
			{
				return Customer_ClassID.Select();
			}
			return null;
		}
	}
}