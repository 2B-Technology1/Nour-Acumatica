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
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.CR;
using System;


namespace PX.Objects.CT
{
	public abstract class UsageCurrencyValidationBase<TGraph> : PXGraphExtension<TGraph>
		where TGraph : PXGraph
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();
		}

		protected virtual void ValidateContractUsageBaseCurrency(int? contractID)
		{
			if (contractID != null)
			{
				var schedule = (ContractBillingSchedule)PXSelect<ContractBillingSchedule,
					Where<ContractBillingSchedule.contractID, Equal<Required<Contract.contractID>>>>
					.Select(Base, contractID);

				BAccount customer = (BAccount)PXSelectorAttribute.Select<ContractBillingSchedule.accountID>(Base.Caches[typeof(ContractBillingSchedule)], schedule);
				if (customer.BaseCuryID != null && customer.BaseCuryID != Base.Accessinfo.BaseCuryID)
				{
					throw new PXException(Messages.CurrentBaseCurrenceDiffersFromCustomer, customer.AcctCD, PXAccess.GetBranchCD(Base.Accessinfo.BranchID), customer.AcctCD);
				}
			}
		}
	}
}
