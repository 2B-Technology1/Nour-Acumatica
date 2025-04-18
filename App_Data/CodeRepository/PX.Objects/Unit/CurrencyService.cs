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
using System.Collections.Generic;
using PX.Data;
using PX.Objects.CM.Extensions;

namespace PX.Objects.Unit
{
	public class CurrencyServiceMock : IPXCurrencyService
	{
		public const string CurrencyUSD = "USD";
		public const string CurrencyEUR = "EUR";
		public const string CurrencyJPY = "JPY";

		public const decimal DefaultCuryRate = 1m;

		public const short DefaultCuryPrecision = 2;
		public const short JPYPrecision = 0;

		public int BaseDecimalPlaces()
		{
			return DefaultCuryPrecision;
		}
		public int CuryDecimalPlaces(string curyID)
		{
			switch (curyID)
			{
				case CurrencyJPY:
					return JPYPrecision;
				default:
					return BaseDecimalPlaces();
			}
		}
		public int PriceCostDecimalPlaces()
		{
			return 4;
		}
		public int QuantityDecimalPlaces()
		{
			return 6;
		}
		public string DefaultRateTypeID(string moduleCode)
		{
			return "SPOT";
		}
		public IPXCurrencyRate GetRate(string fromCuryID, string toCuryID, string rateTypeID, DateTime? curyEffDate)
		{
			if (fromCuryID != toCuryID)
			{
				return new CurrencyRate { FromCuryID = fromCuryID, CuryEffDate = DateTime.Today, CuryMultDiv = "M", CuryRate = 1.28m, RateReciprocal = 0.78125m, ToCuryID = toCuryID };
			}
			return new CurrencyRate { FromCuryID = fromCuryID, CuryEffDate = DateTime.Today, CuryMultDiv = "M", CuryRate = DefaultCuryRate, RateReciprocal = DefaultCuryRate, ToCuryID = toCuryID };
		}
		public int GetRateEffDays(string rateTypeID)
		{
			return 3;
		}
		public string BaseCuryID()
		{
			return CurrencyUSD;
		}
		public string BaseCuryID(int? branchID)
		{
			return CurrencyUSD;
		}
		public IEnumerable<IPXCurrency> Currencies()
		{
			return new IPXCurrency[]
			{
				new Currency { CuryID = CurrencyUSD, Description = "Dollar" },
				new Currency { CuryID = CurrencyEUR, Description = "Euro" },
				new Currency { CuryID = CurrencyJPY, Description = "Yen" }
			};
		}
		public IEnumerable<IPXCurrencyRateType> CurrencyRateTypes()
		{
			return new IPXCurrencyRateType[]
			{
				new CurrencyRateType { CuryRateTypeID = "SPOT", Descr = "Spot" },
				new CurrencyRateType { CuryRateTypeID = "BANK", Descr = "Bank" }
			};
		}

		public void PopulatePrecision(PXCache cache, CurrencyInfo info)
		{
			if (info != null && (info.CuryPrecision == null || info.BasePrecision == null))
			{
				if (info.CuryPrecision == null)
				{
					info.CuryPrecision = Convert.ToInt16(CuryDecimalPlaces(info.CuryID));
				}

				if (info.BasePrecision == null)
				{
					info.BasePrecision = Convert.ToInt16(CuryDecimalPlaces(info.BaseCuryID));
				}

				if (cache.GetStatus(info) == PXEntryStatus.Notchanged)
				{
					cache.SetStatus(info, PXEntryStatus.Held);
				}
			}
		}
	}
}
