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
using PX.Common;

namespace PX.Objects.CA.BankFeed
{
	[PXHidden]
	[PXInternalUseOnly]
	public class BankFeedTransactionsImport : CABankTransactionsImport
	{
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDefault]
		[CashAccountDisabledBranchRestrictions(branchID: null, search: typeof(Search<CashAccount.cashAccountID,
			Where<CashAccount.active, Equal<True>,
				And<Match<Current<AccessInfo.userName>>>>>), IsKey = true, DisplayName = "Cash Account",
		Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(CashAccount.descr), Required = true)]
		protected override void CABankTranHeader_CashAccountID_CacheAttached(PXCache sender)
		{
		}
	}
}
