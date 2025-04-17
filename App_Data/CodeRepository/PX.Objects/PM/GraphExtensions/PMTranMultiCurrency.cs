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
using PX.Objects.CM.Extensions;
using PX.Objects.Extensions.MultiCurrency;
using PX.Objects.GL;

namespace PX.Objects.PM
{
	public abstract class PMTranMultiCurrency<TGraph> : MultiCurrencyGraph<TGraph, PMTran>
		where TGraph : PXGraph
	{
		public bool UseDocumentRowInsertingFromBase { get; set; } = false;

		protected override CurySourceMapping GetCurySourceMapping()
		{
			return new CurySourceMapping(typeof(Company))
			{
				CuryID = typeof(Company.baseCuryID)
			};
		}

		protected override bool AllowOverrideCury() => true;

		protected override DocumentMapping GetDocumentMapping()
		{
			return new DocumentMapping(typeof(PMTran))
			{
				BAccountID = typeof(PMTran.bAccountID),
				BranchID = typeof(PMTran.branchID),
				CuryInfoID = typeof(PMTran.baseCuryInfoID),
				CuryID = typeof(PMTran.tranCuryID),
				DocumentDate = typeof(PMTran.date),
			};
		}

		protected override void DocumentRowInserting<CuryInfoID, CuryID>(PXCache sender, object row)
		{
			if (UseDocumentRowInsertingFromBase)
				base.DocumentRowInserting<CuryInfoID, CuryID>(sender, row);
			else
			{
				CurrencyInfo info = currencyinfo.Insert(new CurrencyInfo());
				currencyinfo.Cache.IsDirty = false;
				if (info == null) return;
				sender.SetValue<CuryInfoID>(row, info.CuryInfoID);
				defaultCurrencyRate(currencyinfo.Cache, info, true, true);
				sender.SetValue<CuryID>(row, info.CuryID);
			}
		}

		/// <summary>
		/// CurrencyInfo should not be synced if it is not linked to PMTran
		/// </summary>
		/// <param name="e"></param>
		protected override void _(Events.FieldVerifying<Document, Document.curyID> e)
		{
			long? curyInfoIDInExtension = e.Row.CuryInfoID;
			PMTran pMTran = (PMTran) e.Row.Base;
			if (curyInfoIDInExtension == pMTran.BaseCuryInfoID || curyInfoIDInExtension == pMTran.ProjectCuryInfoID)
			// Acuminator disable once PX1044 ChangesInPXCacheInEventHandlers [Insert a new CuryInfo]
				base._(e);
		}
	}
}
