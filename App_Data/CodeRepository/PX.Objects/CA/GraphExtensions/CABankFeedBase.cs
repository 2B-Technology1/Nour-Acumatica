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
using PX.Objects.CA.BankFeed;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.CA.GraphExtensions
{
	public abstract class CABankFeedBase<TGraph, TPrimary> : PXGraphExtension<TGraph>
		where TGraph : PXGraph, new()
		where TPrimary : class, IBqlTable, new()
	{
		[InjectDependency]
		internal Func<string, BankFeedManager> BankFeedManagerProvider
		{
			get;
			set;
		}

		public PXSetup<CASetup> CASetup;

		public PXSelectJoin<CABankFeedDetail, InnerJoin<CABankFeed, On<CABankFeedDetail.bankFeedID, Equal<CABankFeed.bankFeedID>>>,
			Where<CABankFeed.status, In3<CABankFeedStatus.active, CABankFeedStatus.setupRequired>,
				And<CABankFeedDetail.cashAccountID, Equal<Required<CABankFeedDetail.cashAccountID>>>>> BankFeedDetail;

		public PXAction<TPrimary> retrieveTransactions;
		[PXUIField(DisplayName = "Retrieve Transactions", Visible = true)]
		[PXButton(CommitChanges = true)]
		public virtual IEnumerable RetrieveTransactions(PXAdapter adapter)
		{
			var ret = adapter.Get();
			var cashAccountId  = GetCashAccountId();
			if (cashAccountId == null) return ret;

			PXResult<CABankFeedDetail, CABankFeed> result = (PXResult<CABankFeedDetail, CABankFeed>)BankFeedDetail.Select(cashAccountId);
			CABankFeed bankFeed = result;
			CABankFeedDetail bankFeedDet = result;
			if (bankFeed == null) return adapter.Get();

			if (bankFeed.RetrievalStatus == CABankFeedRetrievalStatus.LoginFailed)
			{
				UpdateFeed(bankFeed);
				return ret;
			}

			ImportTransactions(bankFeed, bankFeedDet);
			return ret;
		}

		public PXAction<TPrimary> syncUpdateFeed;
		[PXUIField(DisplayName = "", Visible = false)]
		[PXButton]
		public virtual IEnumerable SyncUpdateFeed(PXAdapter adapter)
		{
			var ret = adapter.Get();
			var formProcRes = adapter.CommandArguments;
			var cashAccountId = GetCashAccountId();
			if (string.IsNullOrEmpty(formProcRes) || cashAccountId == null) return ret;

			PXResult<CABankFeedDetail, CABankFeed> result = (PXResult<CABankFeedDetail, CABankFeed>)BankFeedDetail.Select(cashAccountId);
			CABankFeed bankFeed = result;
			CABankFeedDetail bankfeedDetail = result;

			if (bankFeed == null) return ret;

			var guid = (Guid)Base.UID;
			var importToSingle = CASetup.Current.ImportToSingleAccount == true;
			var manager = GetSpecificManager(bankFeed);
			PXLongOperation.StartOperation(this, () =>
			{
				manager.ProcessUpdateResponseAsync(formProcRes, bankFeed).GetAwaiter().GetResult();
				ImportTransactions(bankFeed, bankfeedDetail, importToSingle, guid);
			});

			return ret;
		}

		protected virtual void ImportTransactions(CABankFeed bankFeed, CABankFeedDetail bankFeedDetail)
		{
			var guid = (Guid)Base.UID;
			var importToSingle = CASetup.Current.ImportToSingleAccount == true;
			PXLongOperation.StartOperation(this, () =>
			{
				ImportTransactions(bankFeed, bankFeedDetail, importToSingle, guid);
			});
		}

		protected virtual void UpdateFeed(CABankFeed bankFeed)
		{
			var dialogResult = Base.Views[Base.PrimaryView].Ask(Messages.UpdateBankFeedCredentials, MessageButtons.OKCancel);
			if (dialogResult == WebDialogResult.Cancel) return;
			
			var manager = GetSpecificManager(bankFeed);

			PXLongOperation.StartOperation(this, () =>
			{
				manager.UpdateAsync(bankFeed).GetAwaiter().GetResult();
			});
		}

		private void ImportTransactions(CABankFeed bankFeed, CABankFeedDetail bankFeedDetail, bool importToSingle, Guid guid)
		{
			Dictionary<int, string> lastUpdatedStatements;
			if (importToSingle)
			{
				lastUpdatedStatements = CABankFeedImport.ImportTransactionsForAccounts(bankFeed, new List<CABankFeedDetail>() { bankFeedDetail }, guid)
					.GetAwaiter().GetResult();
			}
			else
			{
				lastUpdatedStatements = CABankFeedImport.ImportTransactions(new List<CABankFeed>() { bankFeed }, guid).GetAwaiter().GetResult();
			}
			PXLongOperation.SetCustomInfo(new BankFeedRedirectToStatementCustomInfo(lastUpdatedStatements));
		}

		private BankFeedManager GetSpecificManager(CABankFeed bankfeed)
		{
			return BankFeedManagerProvider(bankfeed.Type);
		}

		public abstract int? GetCashAccountId();
	}
}
