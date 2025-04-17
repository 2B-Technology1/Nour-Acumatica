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
using PX.Objects.GL.DAC;
using PX.Objects.GL.Standalone;
using PX.Objects.Common;
using PX.Objects.Common.Bql;
using PX.Data.BQL;
using PX.Objects.CM;

namespace PX.Objects.GL
{
	public sealed class PostGraphMultipleBaseCurrencies : PXGraphExtension<PostGraph>
	{

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();
		}

		public delegate void PostBatchProcDelegate(Batch b, bool createintercompany);
		[PXOverride]
		public void PostBatchProc(Batch b, bool createintercompany, PostBatchProcDelegate baseMethod)
		{
			CheckBatchAndLedgerBaseCurrency(Base.Caches<Batch>(), b);
			baseMethod(b, createintercompany);
		}

		public delegate void ReleaseBatchProcDelegate(Batch b, bool unholdBatch = false);
		[PXOverride]
		public void ReleaseBatchProc(Batch b, bool unholdBatch = false, ReleaseBatchProcDelegate baseMethod = null)
		{
			CheckBatchAndLedgerBaseCurrency(Base.Caches<Batch>(), b);
			baseMethod(b, unholdBatch);
		}

		protected void CheckBatchAndLedgerBaseCurrency(PXCache cache, Batch batch)
		{
			CurrencyInfo tranCurrencyInfo = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>
											.Select(cache.Graph, batch.CuryInfoID);
			Ledger ledger = Ledger.PK.Find(cache.Graph, batch.LedgerID) as Ledger;

			if (tranCurrencyInfo != null && ledger != null
				&& !tranCurrencyInfo.BaseCuryID.Equals(ledger.BaseCuryID))
			{
				throw new PXException(Messages.IncorrectBaseCurrency, ledger.LedgerCD);
			}
		}
	}
}
