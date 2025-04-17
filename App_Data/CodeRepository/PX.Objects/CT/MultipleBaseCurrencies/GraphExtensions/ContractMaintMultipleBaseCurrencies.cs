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
using PX.Objects.GL;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.PM;
using System;


namespace PX.Objects.CT
{
	public class ContractMaintMultipleBaseCurrencies : PXGraphExtension<ContractMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();
		}

		protected virtual void _(Events.FieldVerifying<Contract.curyID> e)
		{
			if (e.Row == null || e.NewValue == null)
				return;

			Contract contract = (Contract)e.Row;
			string curyID = (string)e.NewValue;
			if (contract.CuryID == curyID)
				return;

			foreach (ContractDetail det in Base.ContractDetails.Select())
			{
				ContractItem item = ContractItem.PK.Find(Base, det.ContractItemID);
				if (item.CuryID != curyID)
				{
					throw new PXSetPropertyException(Messages.ContractCuryCannotBeChangedDueToDetails, item.ContractItemCD, item.CuryID);
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<ContractBillingSchedule.accountID> e)
		{
			int? oldCustomerID = (int?)e.Cache.GetValueOriginal<ContractBillingSchedule.accountID>(e.Row);
			if (oldCustomerID == null || e.NewValue == null || (int?)e.NewValue == oldCustomerID)
			{
				return;
			}

			BAccountR oldCustomer = (BAccountR)PXSelect<BAccountR, Where<BAccountR.bAccountID, Equal<Required<BAccountR.bAccountID>>>>.Select(Base, oldCustomerID);
			BAccountR newCustomer = (BAccountR)PXSelect<BAccountR, Where<BAccountR.bAccountID, Equal<Required<BAccountR.bAccountID>>>>.Select(Base, e.NewValue);

			if (newCustomer.BaseCuryID == null || oldCustomer.BaseCuryID == newCustomer.BaseCuryID)
			{
				return;
			}

			PMTran usage = PXSelectJoin<PMTran,
				InnerJoin<Branch,
					On<Branch.branchID, Equal<PMTran.branchID>,
					And<Branch.baseCuryID, NotEqual<Required<Branch.baseCuryID>>>>>,
				Where<PMTran.projectID, Equal<Required<PMTran.projectID>>,
					And<PMTran.billable, Equal<True>,
					And<PMTran.billed, Equal<False>>>>>.SelectSingleBound(Base, null, newCustomer.BaseCuryID, ((ContractBillingSchedule)e.Row).ContractID);

			if (usage != null)
			{
				var branch = PXAccess.GetBranch(usage.BranchID);
				e.NewValue = newCustomer.AcctCD;
				throw new PXSetPropertyException(Messages.ContractUsageWithDiffBaseCurrencyExists, branch.BranchCD, newCustomer.AcctCD);
			}
		}
	}
}
