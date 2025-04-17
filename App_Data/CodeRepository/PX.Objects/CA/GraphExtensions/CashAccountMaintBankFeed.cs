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

namespace PX.Objects.CA.GraphExtensions
{
	public class CashAccountMaintBankFeed : PXGraphExtension<CashAccountMaint>
	{
		public PXSelectJoin<CABankFeed, InnerJoin<CABankFeedDetail, On<CABankFeed.bankFeedID, Equal<CABankFeedDetail.bankFeedID>>>,
				   Where<CABankFeedDetail.cashAccountID, Equal<Current<CashAccount.cashAccountID>>,
					   And<CABankFeed.status, In3<CABankFeedStatus.active, CABankFeedStatus.setupRequired>>>> BankFeed;

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.bankFeedIntegration>();
		}

		protected virtual void _(Events.FieldSelecting<CABankFeed.statementImportSource> e)
		{
			var bankFeed = e.Row as CABankFeed;
			if (bankFeed == null) return;

			e.ReturnValue = PXMessages.LocalizeNoPrefix(Messages.BankFeedLabel) + " : " + bankFeed.BankFeedID;
		}

		protected virtual void _(Events.RowSelected<CABankFeed> e)
		{
			var bankFeed = e.Row;
			if (bankFeed == null) return;

			PXUIFieldAttribute.SetVisible<CABankFeed.statementImportSource>(e.Cache, bankFeed, true);
		}

		protected virtual void _(Events.RowSelected<CashAccount> e)
		{
			var cashAccount = e.Row;
			if (cashAccount == null) return;

			bool withoutBankFeed = BankFeed.SelectSingle() == null;
			PXUIFieldAttribute.SetVisible<CashAccount.statementImportTypeName>(e.Cache, cashAccount, withoutBankFeed);
		}
	}
}
