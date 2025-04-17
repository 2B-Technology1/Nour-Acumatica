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
using static PX.Objects.CT.ContractBilling;

namespace PX.Objects.CT
{
	public class ContractBillingVisibilityRestriction : PXGraphExtension<ContractBilling>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		//can't use
		//Base.Items.WhereAnd<Where<Customer.cOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>>>();
		//due to _OuterView for PXFilteredProcessing created before applaying extension
		public PXFilteredProcessingJoin<
			Contract, BillingFilter,
			InnerJoin<ContractBillingSchedule,
				On<Contract.contractID, Equal<ContractBillingSchedule.contractID>>,
			LeftJoin<Customer,
				On<Contract.customerID, Equal<Customer.bAccountID>>>>,
			Where2<
				Where<ContractBillingSchedule.nextDate, LessEqual<Current<BillingFilter.invoiceDate>>,
					Or<ContractBillingSchedule.type, Equal<BillingType.BillingOnDemand>>>,
				And<Contract.baseType, Equal<CTPRType.contract>,
				And<Contract.isCancelled, Equal<False>,
				And<Contract.isCompleted, Equal<False>,
				And<Contract.isActive, Equal<True>,
				And<Customer.cOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>,
				And2<
					Where<Current<BillingFilter.templateID>, IsNull,
						Or<Current<BillingFilter.templateID>, Equal<Contract.templateID>>>,
				And2<
					Where<Current<BillingFilter.customerClassID>, IsNull,
						Or<Current<BillingFilter.customerClassID>, Equal<Customer.customerClassID>>>,
					And<Where<Current<BillingFilter.customerID>, IsNull,
						Or<Current<BillingFilter.customerID>, Equal<Contract.customerID>>>>>>>>>>>>> Items;
	}

	public sealed class BillingFilterVisibilityRestriction : PXCacheExtension<ContractBilling.BillingFilter>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerClassByUserBranches]
		public string CustomerClassID { get; set; }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByUserBranches]
		public int? CustomerID { get; set; }
	}
}