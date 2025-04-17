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
	public class CABankTransactionsImportBankFeed : CABankFeedBase<CABankTransactionsImport, CABankTranHeader>
	{
		public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.bankFeedIntegration>();

		protected virtual void RowSelected(Events.RowSelected<CABankTranHeader> e)
		{
			var row = e?.Row;
			if (row == null) return;

			var hasBankFeed = BankFeedDetail.SelectSingle(row.CashAccountID) != null;
			var processWithBankFeed = hasBankFeed && row.CashAccountID.HasValue;

			Base.uploadFile.SetVisible(!processWithBankFeed);
			retrieveTransactions.SetVisible(processWithBankFeed);
		}

		public override int? GetCashAccountId() => Base.Header.Current?.CashAccountID;
	}
}
