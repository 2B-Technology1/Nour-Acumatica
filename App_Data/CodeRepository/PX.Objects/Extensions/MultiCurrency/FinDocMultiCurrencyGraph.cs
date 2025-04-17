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
using PX.Objects.AR;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.Extensions.MultiCurrency
{
	/// <summary>The generic graph extension that defines the multi-currency functionality extended for AP/AR entities.</summary>
	/// <typeparam name="TGraph">A <see cref="PX.Data.PXGraph" /> type.</typeparam>
	/// <typeparam name="TPrimary">A DAC (a <see cref="PX.Data.IBqlTable" /> type).</typeparam>
	public abstract class FinDocMultiCurrencyGraph<TGraph, TPrimary> : MultiCurrencyGraph<TGraph, TPrimary>, IPXCurrencyHelper
			where TGraph : PXGraph
			where TPrimary : class, IBqlTable, new()
	{
		/// <summary>
		/// Override to specify the way current document status should be obtained. Override with returning Balanced status code if it is processing screen
		/// </summary>
		protected abstract string DocumentStatus { get; }

		/// <summary>
		/// Some base values should be recalculated even their Cury fields are marked as BaseCalc = false: DocBal and DiscBal, for example.
		/// Override this property to list them. This emulates behavior of old PXDBCurrencyAttribute.SetBaseCalc&lt;ARInvoice.curyDiscBal>(cache, doc, state.BalanceBaseCalc);
		/// </summary>
		protected abstract IEnumerable<Type> FieldWhichShouldBeRecalculatedAnyway { get; }

		protected abstract bool ShouldBeDisabledDueToDocStatus();

		protected override bool AllowOverrideCury()
		{
			return base.AllowOverrideCury() && !ShouldBeDisabledDueToDocStatus();
		}

		protected override void DateFieldUpdated<CuryInfoID, DocumentDate>(PXCache sender, IBqlTable row)
		{
			if (ShouldBeDisabledDueToDocStatus()) return;
			else base.DateFieldUpdated<CuryInfoID, DocumentDate>(sender, row);
		}

		protected override bool ShouldMainCurrencyInfoBeReadonly()
		{
			if (Base.IsContractBasedAPI) //If it is API, random entity appears in Current, so  ShouldBeDisabledDueToDocStatus cannot make sense
				return base.ShouldMainCurrencyInfoBeReadonly();
			else
				return base.ShouldMainCurrencyInfoBeReadonly() || ShouldBeDisabledDueToDocStatus();
		}

		protected virtual void _(Events.RowSelected<TPrimary> e)
		{
			foreach (Type dacType in TrackedItems.Keys)
			{
				if (!typeof(TPrimary).IsAssignableFrom(dacType))
					continue;

				var curyFields = TrackedItems[dacType];
				foreach (var fieldToRecalculate in FieldWhichShouldBeRecalculatedAnyway)
				{
					var curyField = curyFields.SingleOrDefault(f => f.CuryName.Equals(fieldToRecalculate.Name, StringComparison.OrdinalIgnoreCase));
					if (curyField != null)
					{
						curyField.BaseCalc = !ShouldBeDisabledDueToDocStatus();
					}
				}
			}
		}

		internal void SetDetailCuryInfoID<T>(PXSelectBase<T> detailView, long? CuryInfoID)
			where T : class,  IBqlTable, new()
		{
			foreach (T detail in detailView.Select())
			{
				if (detailView.Cache.GetValue<ARTran.curyInfoID>(detail) as long? != CuryInfoID)
				{
					detailView.Cache.SetValue<ARTran.curyInfoID>(detail, CuryInfoID);
					recalculateRowBaseValues(detailView.Cache, detail, TrackedItems[typeof(T)]);
					detailView.Cache.Update(detail);
				}
			}
		}

		private void RestoreDifferentCurrencyInfoIDs(IAdjustment aRAdjust)
		{
			if (aRAdjust.AdjdCuryInfoID == aRAdjust.AdjgCuryInfoID)
				aRAdjust.AdjdCuryInfoID = CloneCurrencyInfo(GetCurrencyInfo(aRAdjust.AdjgCuryInfoID)).CuryInfoID;
			if (aRAdjust.AdjdOrigCuryInfoID == aRAdjust.AdjgCuryInfoID)
				aRAdjust.AdjdOrigCuryInfoID = CloneCurrencyInfo(GetCurrencyInfo(aRAdjust.AdjgCuryInfoID)).CuryInfoID;
		}

		/// <summary>
		/// On saving CurrencyInfo entities identical to some CurrencyInfo record with base currency, acumatica reuses same CurrencyInfoID instead,
		/// so Adjustment can have the same AdjgCuryInfoID, AdjdCuryInfoID, AdjdOrigCuryInfoID.
		/// This method clones CurrencyInfo entity to link different ones to this fields
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="adjustmentsView"></param>
		internal void RestoreDifferentCurrencyInfoIDs<T>(PXSelectBase<T> adjustmentsView)
			where T: class, IAdjustment, IBqlTable, new()
		{
			adjustmentsView.Cache.Cached.OfType<T>().ForEach(RestoreDifferentCurrencyInfoIDs);
		}

		internal void SetAdjgCuryInfoID<T>(PXSelectBase<T> adjustmentsView, long? AdjgCuryInfoID)
			where T : class, IAdjustment, IBqlTable, new()
		{
			foreach (T adjust in adjustmentsView.Select())
			{
				if (adjust.AdjgCuryInfoID != AdjgCuryInfoID)
				{
					adjust.AdjgCuryInfoID = AdjgCuryInfoID;
					recalculateRowBaseValues(adjustmentsView.Cache, adjust, TrackedItems[typeof(T)]);
					adjustmentsView.Cache.Update(adjust);
				}
			}
		}
	}
}
