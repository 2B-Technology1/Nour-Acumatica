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
using System;

namespace PX.Objects.CM.Extensions
{
	public static class CurrencyServiceHelper
	{
		public static IPXCurrencyRate SearchForNewRate(this CurrencyInfo info, PXGraph graph) => ServiceLocator.Current.GetInstance<Func<PXGraph, IPXCurrencyService>>()(graph)
				.GetRate(info.CuryID, info.BaseCuryID, info.CuryRateTypeID, info.CuryEffDate);

		public static void Populate(this IPXCurrencyRate rate, CurrencyInfo info)
		{
			info.CuryEffDate = rate.CuryEffDate;
			info.CuryRate = Math.Round((decimal)rate.CuryRate, 8);
			info.CuryMultDiv = rate.CuryMultDiv;
			info.RecipRate = Math.Round((decimal)rate.RateReciprocal, 8);
		}
	}
}
