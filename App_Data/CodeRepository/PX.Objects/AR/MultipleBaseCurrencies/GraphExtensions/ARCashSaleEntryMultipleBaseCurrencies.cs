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
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using ARCashSale = PX.Objects.AR.Standalone.ARCashSale;

namespace PX.Objects.AR
{
	public sealed class ARCashSaleEntryMultipleBaseCurrencies : PXGraphExtension<ARCashSaleEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();
		}

		[PXRestrictor(typeof(Where<Customer.baseCuryID, IsNull, 
			Or<Customer.baseCuryID, EqualBaseCuryID<ARCashSale.branchID.FromCurrent>>>),
			"",
		SuppressVerify = false
		)]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		public void _(Events.CacheAttached<ARCashSale.customerID> e) { }

		protected void _(Events.FieldVerifying<ARCashSale.branchID> e)
		{
			if (e.NewValue == null)
				return;

			Branch branch = PXSelectorAttribute.Select<ARCashSale.branchID>(e.Cache, e.Row, (int)e.NewValue) as Branch;
			BAccountR customer = PXSelectorAttribute.Select<ARCashSale.customerID>(e.Cache, e.Row) as BAccountR;

			if (branch != null && customer != null
				&& customer.BaseCuryID != null && branch.BaseCuryID != customer.BaseCuryID)
			{
				e.NewValue = branch.BranchCD;
				throw new PXSetPropertyException(Messages.BranchCustomerDifferentBaseCury, PXOrgAccess.GetCD(customer.COrgBAccountID), customer.AcctCD);
			}
		}
	}
}
