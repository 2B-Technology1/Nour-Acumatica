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

namespace PX.Objects.CM
{
	public class CuryHelper : IPXCurrencyHelper
	{
		private PXSelectBase<CurrencyInfo> Currencyinfoselect { get; }

		public CuryHelper(PXSelectBase<CurrencyInfo> currencyinfoselect)
		{
			Currencyinfoselect = currencyinfoselect;
		}

		public CurrencyInfo GetCurrencyInfo(long? currencyInfoID) => CurrencyInfoCache.GetInfo(Currencyinfoselect, currencyInfoID);

		Extensions.CurrencyInfo IPXCurrencyHelper.GetCurrencyInfo(long? key) => Extensions.CurrencyInfo.GetEX(GetCurrencyInfo(key));

		public Extensions.CurrencyInfo GetDefaultCurrencyInfo() => Extensions.CurrencyInfo.GetEX(Currencyinfoselect.SelectSingle());
	}
}
