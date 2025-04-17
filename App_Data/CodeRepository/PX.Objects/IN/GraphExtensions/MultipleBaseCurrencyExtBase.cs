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

using PX.Common;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.GL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.IN.GraphExtensions
{
	public abstract class MultipleBaseCurrencyExtBase<TGraph, TDocument, TLine,
		TDocumentBranch, TLineBranch, TLineSite> : PXGraphExtension<TGraph>

		where TGraph : PXGraph
		where TDocument : class, IBqlTable, new()
		where TLine : class, IBqlTable, new()
		where TDocumentBranch : class, IBqlField
		where TLineBranch : class, IBqlField
		where TLineSite : class, IBqlField
	{
		protected virtual void _(Events.RowUpdated<TDocument> e)
		{
			if (!e.Cache.ObjectsEqual<TDocumentBranch>(e.OldRow, e.Row))
			{
				var newBranch = (Branch)PXSelectorAttribute.Select<TDocumentBranch>(e.Cache, e.Row);
				var oldBranch = (Branch)PXSelectorAttribute.Select<TDocumentBranch>(e.Cache, e.OldRow);
				if (!string.Equals(newBranch?.BaseCuryID, oldBranch?.BaseCuryID, StringComparison.OrdinalIgnoreCase))
				{
					OnDocumentBaseCuryChanged(e.Cache, e.Row);
				}
			}
		}

		protected virtual void OnDocumentBaseCuryChanged(PXCache cache, TDocument row)
		{
			PXSelectBase<TLine> transactionView = GetTransactionView();
			foreach (TLine tran in transactionView.Select())
			{
				var tranCache = transactionView.Cache;
				tranCache.MarkUpdated(tran, assertError: true);
				tranCache.VerifyFieldAndRaiseException<TLineBranch>(tran);
				tranCache.VerifyFieldAndRaiseException<TLineSite>(tran);
			}
		}

		protected abstract PXSelectBase<TLine> GetTransactionView();

		protected virtual void _(Events.FieldUpdated<TLine, TLineBranch> eventArgs)
		{
			var branchID = (int?)eventArgs.Cache.GetValue<TLineBranch>(eventArgs.Row);
			var newBranch = Branch.PK.Find(Base, branchID);
			var oldBranch = Branch.PK.Find(Base, (int?)eventArgs.OldValue);
			if (!string.Equals(newBranch?.BaseCuryID, oldBranch?.BaseCuryID, StringComparison.OrdinalIgnoreCase))
			{
				OnLineBaseCuryChanged(eventArgs.Cache, eventArgs.Row);
			}
		}

		protected virtual void OnLineBaseCuryChanged(PXCache cache, TLine row)
		{
			cache.SetDefaultExt<TLineSite>(row);
		}

		protected virtual void _(Events.RowPersisting<TLine> e)
		{
			if (e.Operation.Command().IsNotIn(PXDBOperation.Insert, PXDBOperation.Update))
				return;

			e.Cache.VerifyFieldAndRaiseException<TLineBranch>(e.Row);
			e.Cache.VerifyFieldAndRaiseException<TLineSite>(e.Row);
		}

		protected virtual void SetDefaultBaseCurrency<TCuryID, TCuryInfoID, TDocDate>(PXCache cache, TDocument document, bool resetCuryID)
			where TCuryID : class, IBqlField
			where TCuryInfoID : class, IBqlField
			where TDocDate : class, IBqlField
		{
			CurrencyInfo info = CurrencyInfoAttribute.SetDefaults<TCuryInfoID>(cache, document, resetCuryID);

			string message = PXUIFieldAttribute.GetError<CurrencyInfo.curyEffDate>(Base.Caches[typeof(CurrencyInfo)], info);
			if (string.IsNullOrEmpty(message) == false)
			{
				var date = cache.GetValue<TDocDate>(document);
				cache.RaiseExceptionHandling<TDocDate>(document, date, new PXSetPropertyException(message, PXErrorLevel.Warning));
			}

			if (info != null)
				cache.SetValue<TCuryID>(document, info.CuryID);
		}
	}
}
