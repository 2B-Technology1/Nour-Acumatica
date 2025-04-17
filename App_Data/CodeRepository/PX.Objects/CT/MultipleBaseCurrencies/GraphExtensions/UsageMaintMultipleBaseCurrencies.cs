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
using PX.Objects.Common;
using System.Linq;
using PX.Objects.CR;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.PM;
using System;


namespace PX.Objects.CT
{
	public class UsageMaintMultipleBaseCurrencies : PXGraphExtension<UsageMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();
		}

		public PXSelect<ContractBillingSchedule, Where<ContractBillingSchedule.contractID, Equal<Current<Contract.contractID>>>> Billing;

		protected virtual void _(Events.RowSelected<Contract> e)
		{
			Billing.Current = Billing.Select();
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[Branch(typeof(Coalesce<
				Search<Branch.branchID, Where<Branch.bAccountID, Equal<Current<ContractBillingScheduleMultipleBaseCurrencies.cOrgBAccountID>>>>,
				Search<Branch.branchID, Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>>), IsDetail = true)]
		[PXRestrictor(typeof(
			Where<Branch.baseCuryID, Equal<Current<ContractBillingScheduleMultipleBaseCurrencies.customerBaseCuryID>>,
				Or<Current<ContractBillingScheduleMultipleBaseCurrencies.customerBaseCuryID>, IsNull>>), "")]
		protected virtual void _(Events.CacheAttached<PMTran.branchID> e) { }

		protected virtual void _(Events.FieldVerifying<PMTran.branchID> e)
		{
			if (e.NewValue == null || Base.CurrentContract.Current == null || Billing.Current == null)
				return;

			BAccountR customer = (BAccountR)PXSelectorAttribute.Select<ContractBillingSchedule.accountID>(Billing.Cache, Billing.Current);
			Branch branch = PXSelectorAttribute.Select<PMTran.branchID>(e.Cache, e.Row, (int)e.NewValue) as Branch;
			if (customer.BaseCuryID != null && branch.BaseCuryID != customer.BaseCuryID)
			{
				e.NewValue = branch.BranchCD;
				throw new PXSetPropertyException(AR.Messages.BranchCustomerDifferentBaseCury, PXOrgAccess.GetCD(customer.COrgBAccountID), customer.AcctCD);
			}
		}

		[PXDecimal(8)]
		protected virtual void _(Events.CacheAttached<CM.Extensions.CurrencyInfo.sampleCuryRate> e) { }
	}
}
