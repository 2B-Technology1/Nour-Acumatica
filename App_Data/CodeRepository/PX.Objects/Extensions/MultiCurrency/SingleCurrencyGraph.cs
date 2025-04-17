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

using CommonServiceLocator;
using PX.Data;
using PX.Objects.CM.Extensions;
using PX.Objects.GL;
using System;

namespace PX.Objects.Extensions.MultiCurrency
{

	/// <summary>
	/// An implementation of IPXCurrencyHelper for a screen on which the same currency is always expected
	/// and CurrencyInfo is never persisted.
	/// </summary>
	public abstract class SingleCurrencyGraph<TGraph, TPrimary> : PXGraphExtension<TGraph>, IPXCurrencyHelper
			where TGraph : PXGraph
			where TPrimary : class, IBqlTable, new()
	{
		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>> currencyinfobykey;

		public CurrencyInfo GetCurrencyInfo(long? key) => currencyinfobykey.Select(key);

		public CurrencyInfo GetDefaultCurrencyInfo()
		{
			IPXCurrencyService pXCurrencyService = ServiceLocator.Current.GetInstance<Func<PXGraph, IPXCurrencyService>>()(Base);
			string baseCuryID = pXCurrencyService.BaseCuryID();
			short precision = Convert.ToInt16(pXCurrencyService.CuryDecimalPlaces(baseCuryID));
			return new CurrencyInfo
			{
				CuryID = baseCuryID,
				BaseCuryID = baseCuryID,
				CuryRate = 1m,
				RecipRate = 1m,
				CuryPrecision = precision,
				BasePrecision = precision
			};
		}
	}
}
