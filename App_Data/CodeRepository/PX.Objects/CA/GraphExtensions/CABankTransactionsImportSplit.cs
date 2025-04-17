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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.CA
{
	public class CABankTransactionsImportSplit : PXGraphExtension<CABankTransactionsImport>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.bankTransactionSplits>();
		}

		public PXSelect<CABankTran,
			Where<CABankTran.headerRefNbr, Equal<Current<CABankTranHeader.refNbr>>,
			And<CABankTran.tranType, Equal<Current<CABankTranHeader.tranType>>,
				And<CABankTranSplit.parentTranID, IsNull>>>> Details;

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Disbursement", Visibility = PXUIVisibility.Invisible, Visible = false)]
		public virtual void _(Events.CacheAttached<CABankTran.curyCreditAmt> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Receipt", Visibility = PXUIVisibility.Invisible, Visible = false)]
		public virtual void _(Events.CacheAttached<CABankTran.curyDebitAmt> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Processed", Visibility = PXUIVisibility.Invisible, Visible = false)]
		public virtual void _(Events.CacheAttached<CABankTran.processed> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Matched", Visibility = PXUIVisibility.Invisible, Visible = false)]
		public virtual void _(Events.CacheAttached<CABankTran.documentMatched> e) { }

		[PXOverride]
		public virtual void CABankTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = (CABankTran)e.Row;

			CABankTranSplit currentExt = Base.Details.Current?.GetExtension<CABankTranSplit>();
			bool isSplitted = currentExt?.Splitted == true;
			if (isSplitted)
			{
				PXUIFieldAttribute.SetEnabled(sender, row, false);
			}
		}

		[PXOverride]
		public virtual void CABankTran_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			CABankTran row = e.Row as CABankTran;
			CABankTranSplit currentExt = Base.Details.Current?.GetExtension<CABankTranSplit>();

			if (currentExt?.Splitted == true)
				throw new PXSetPropertyException(Messages.TransactionCanBeDeletedBecauseItHasBeenSplit);
		}
	}
}
