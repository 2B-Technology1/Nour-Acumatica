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
using PX.Objects.Extensions.MultiCurrency;
using System;

namespace PX.Objects.CM.TemporaryHelpers
{
	/// <summary>
	/// Povides SOME operations for currency-related calculations. Uses IPXCurrencyHelper (MultiCurrency Extension) if available, otherwise uses PXCurrencyAttribute
	/// </summary>
	static class MultiCurrencyCalculator
	{
		public static void CuryConvBase(PXCache sender, object row, decimal curyval, out decimal baseval)
		{
			IPXCurrencyHelper pXCurrencyHelper = sender.Graph.FindImplementation<IPXCurrencyHelper>();
			if (pXCurrencyHelper == null)
				PXCurrencyAttribute.CuryConvBase(sender, row, curyval, out baseval);
			else
				baseval = pXCurrencyHelper.GetDefaultCurrencyInfo().CuryConvBase(curyval);
		}

		public static void CuryConvBaseSkipRounding(PXCache sender, object row, decimal curyval, out decimal baseval)
		{
			IPXCurrencyHelper pXCurrencyHelper = sender.Graph.FindImplementation<IPXCurrencyHelper>();
			if (pXCurrencyHelper == null)
				PXCurrencyAttribute.CuryConvBase(sender, row, curyval, out baseval, true);
			else
			{
				CM.Extensions.CurrencyInfo currencyInfo = pXCurrencyHelper.GetDefaultCurrencyInfo();
				baseval = currencyInfo?.CuryConvBaseRaw(curyval) ?? curyval;
			}
		}

		public static void CuryConvCury(PXCache sender, object row, decimal baseval, out decimal curyval)
		{
			IPXCurrencyHelper pXCurrencyHelper = sender.Graph.FindImplementation<IPXCurrencyHelper>();
			if (pXCurrencyHelper == null)
				PXCurrencyAttribute.CuryConvCury(sender, row, baseval, out curyval);
			else
			{
				CM.Extensions.CurrencyInfo currencyInfo = pXCurrencyHelper.GetDefaultCurrencyInfo();
				curyval = currencyInfo?.CuryConvCury(baseval) ?? baseval;
			}
		}

		public static void CuryConvCury(PXCache sender, object row, decimal baseval, out decimal curyval, int precision)
		{
			IPXCurrencyHelper pXCurrencyHelper = sender.Graph.FindImplementation<IPXCurrencyHelper>();
			if (pXCurrencyHelper == null)
				PXCurrencyAttribute.CuryConvCury(sender, row, baseval, out curyval, precision);
			else
			{
				CM.Extensions.CurrencyInfo currencyInfo = pXCurrencyHelper.GetDefaultCurrencyInfo();
				curyval = currencyInfo?.CuryConvCury(baseval, precision) ?? baseval;
			}
		}

		public static decimal RoundCury(PXCache sender, object row, decimal val)
		{
			IPXCurrencyHelper pXCurrencyHelper = sender.Graph.FindImplementation<IPXCurrencyHelper>();
			if (pXCurrencyHelper == null) return PXCurrencyAttribute.RoundCury(sender, row, val);
			else return pXCurrencyHelper.RoundCury(val);
		}

		public static decimal RoundCury(PXCache sender, object row, decimal val, int? customPrecision)
		{
			if (customPrecision == null) return RoundCury(sender, row, val);

			IPXCurrencyHelper pXCurrencyHelper = sender.Graph.FindImplementation<IPXCurrencyHelper>();
			if (pXCurrencyHelper == null) return PXDBCurrencyAttribute.RoundCury(sender, row, val, customPrecision);
			else return Math.Round(val, customPrecision.Value, MidpointRounding.AwayFromZero);
		}

		public static string GetCurrencyID(PXGraph graph)
		{
			IPXCurrencyHelper pXCurrencyHelper = graph.FindImplementation<IPXCurrencyHelper>();
			if (pXCurrencyHelper == null)
			{
				CurrencyInfo currencyInfo = graph.Caches<CurrencyInfo>().Current as CurrencyInfo;
				return currencyInfo?.CuryID;
			}
			else return pXCurrencyHelper.GetDefaultCurrencyInfo()?.CuryID;
		}

		public static Currency GetCurrentCurrency(PXGraph graph)
		{
			string CuryID = GetCurrencyID(graph);
			if (CuryID == null) return null;
			else return CurrencyCollection.GetCurrency(CuryID);
		}

		public static Extensions.CurrencyInfo GetCurrencyInfo<CuryInfoIDField>(PXGraph graph, object row)
			where CuryInfoIDField : IBqlField
		{
			PXCache cache = graph.Caches[row.GetType()];

			IPXCurrencyHelper pXCurrencyHelper = graph.FindImplementation<IPXCurrencyHelper>();
			if (pXCurrencyHelper == null)
			{
				CurrencyInfo currencyInfo = CurrencyInfoAttribute.GetCurrencyInfo<CuryInfoIDField>(cache, row)
					?? PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(graph, cache.GetValue<CuryInfoIDField>(row));

				if (currencyInfo == null) return null;
				else return Extensions.CurrencyInfo.GetEX(currencyInfo);
			}
			else
				return pXCurrencyHelper.GetCurrencyInfo(cache.GetValue<CuryInfoIDField>(row) as long?);
		}
	}
}
