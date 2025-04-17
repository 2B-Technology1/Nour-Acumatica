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
using PX.Objects.CA;
using PX.Objects.CA.BankStatementHelpers;
using PX.Objects.CS;
using System;
using IMatchSettings = PX.Objects.CA.BankStatementHelpers.IMatchSettings;

namespace PX.Objects.PR
{
	public class PRxCABankMatchingProcess : PXGraphExtension<CABankMatchingProcess>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.payrollModule>();
		}

		[PXOverride]
		public virtual PXResultset<CATranExt> SearchForMatchingTransactions(CABankTran aDetail, IMatchSettings aSettings, PX.Objects.AP.Pair<DateTime, DateTime> tranDateRange, string curyID)
		{
			return Base.CABankTransactionsRepository.SearchForMatchingTransactions(Base, aDetail, aSettings, tranDateRange, curyID);
		}

		[PXOverride]
		public virtual PXResultset<CABatch> SearchForMatchingCABatches(CABankTran aDetail, IMatchSettings aSettings, PX.Objects.AP.Pair<DateTime, DateTime> tranDateRange, string curyID, bool allowUnreleased)
		{
			return Base.CABankTransactionsRepository.SearchForMatchingCABatches(Base, aDetail, aSettings, tranDateRange, curyID, allowUnreleased);
		}

		[PXOverride]
		public virtual PXResultset<CABatchDetailOrigDocAggregate> SearchForMatchesInCABatches(string tranType, string batchNbr)
		{
			return PXSelectJoin<CABatchDetailOrigDocAggregate,
					InnerJoin<CATran, On<CATran.origModule, Equal<CABatchDetailOrigDocAggregate.origModule>,
						And<CATran.origTranType, Equal<CABatchDetailOrigDocAggregate.origDocType>,
						And<CATran.origRefNbr, Equal<CABatchDetailOrigDocAggregate.origRefNbr>,
						And<CATran.isPaymentChargeTran, Equal<False>>>>>,
					InnerJoin<CABankTranMatch, On<CABankTranMatch.cATranID, Equal<CATran.tranID>,
						And<CABankTranMatch.tranType, Equal<Required<CABankTran.tranType>>>>,
					LeftJoin<PRDirectDepositSplit, On<PRDirectDepositSplit.docType, Equal<CABatchDetailOrigDocAggregate.origDocType>,
							And<PRDirectDepositSplit.refNbr, Equal<CABatchDetailOrigDocAggregate.origRefNbr>,
							And<Where<PRDirectDepositSplit.caTranID, Equal<CATran.tranID>,
											And<PRDirectDepositSplit.lineNbr, Equal<CABatchDetailOrigDocAggregate.origLineNbr>,
											Or<CABatchDetailOrigDocAggregate.origLineNbr, Equal<CABatchDetailOrigDocAggregate.origLineNbr.defaultValue>>>>>>>>>>,
					Where<CABatchDetailOrigDocAggregate.batchNbr, Equal<Required<CABatch.batchNbr>>>>.Select(Base, tranType, batchNbr);
		}

		[PXOverride]
		public virtual PXResultset<CABankTranMatch> SearchForTranMatchForCABatch(string batchNbr)
		{
			return PXSelect<CABankTranMatch,
					Where<CABankTranMatch.docRefNbr, Equal<Required<CABankTranMatch.docRefNbr>>,
					And<CABankTranMatch.docType, Equal<CATranType.cABatch>>>>.Select(Base, batchNbr);
		}
	}
}
