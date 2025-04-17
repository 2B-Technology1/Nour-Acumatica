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
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.CA.MultipleBaseCurrencies.GraphExtensions
{
	public class CATranEntryMultipleBaseCurrencies : PXGraphExtension<CATranEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();
		}

		protected virtual void _(Events.FieldVerifying<CAAdj.cashAccountID> e)
		{
			if (e.NewValue == null)
				return;

			CashAccount cahsAccount = PXSelectorAttribute.Select<CAAdj.cashAccountID>(e.Cache, e.Row, e.NewValue) as CashAccount;
			Branch branch = PXSelect<Branch, Where<Branch.branchID, Equal<AccessInfo.branchID.FromCurrent>>>.Select(Base);

			if (cahsAccount != null && branch != null && cahsAccount.BaseCuryID != branch.BaseCuryID)
			{
				e.NewValue = cahsAccount.CashAccountCD;
				throw new PXSetPropertyException(Messages.CashAccountBaseCurrencyDiffersCurrentBranch,
					PXAccess.GetBranchCD(cahsAccount.BranchID),
					cahsAccount.CashAccountCD);
			}
		}
	}
}
