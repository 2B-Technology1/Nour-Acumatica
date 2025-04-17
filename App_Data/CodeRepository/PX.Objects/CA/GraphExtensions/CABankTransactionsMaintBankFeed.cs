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
using System;
using System.Collections;

namespace PX.Objects.CA.GraphExtensions
{
	public class CABankTransactionsMaintBankFeed : CABankFeedBase<CABankTransactionsMaint, CABankTransactionsMaint.Filter>
	{ 
		public delegate IEnumerable UploadFileDelegate(PXAdapter adapter);

		public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.bankFeedIntegration>();

		[PXOverride]
		public virtual IEnumerable UploadFile(PXAdapter adapter, UploadFileDelegate baseMethod)
		{
			var cashAccountId = GetCashAccountId();
			if (cashAccountId != null && Base.CASetup.Current.ImportToSingleAccount == true)
			{
				var bankFeed = BankFeedDetail.SelectSingle(cashAccountId);
				if (bankFeed != null)
				{
					CashAccount acct = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>
						.Select(Base, cashAccountId);
					throw new PXException(Messages.CashAccountAlreadyLinked, acct?.CashAccountCD, bankFeed.AccountName + ":" + bankFeed.AccountMask, bankFeed.BankFeedID);
				}
			}
			return baseMethod(adapter);
		}

		protected virtual void RowSelected(Events.RowSelected<CABankTransactionsMaint.Filter> e)
		{
			var row = e?.Row;

			var status = PXLongOperation.GetStatus(Base.UID, out TimeSpan timespan, out Exception ex);
			var enableButton = (status == PXLongRunStatus.NotExists) && row != null && row.CashAccountID.HasValue;
			retrieveTransactions.SetEnabled(enableButton);

			if (row == null) return; 


			var hasBankFeed = BankFeedDetail.SelectSingle(row.CashAccountID) != null;
			Base.uploadFile.SetVisible(!hasBankFeed);
			retrieveTransactions.SetVisible(hasBankFeed);
		}

		public override int? GetCashAccountId() => Base.TranFilter.Current?.CashAccountID;
	}
}
