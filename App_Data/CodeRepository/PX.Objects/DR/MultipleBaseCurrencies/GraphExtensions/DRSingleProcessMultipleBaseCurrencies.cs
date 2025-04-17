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
using PX.Objects.Common;
using System.Collections.Generic;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.CM;


namespace PX.Objects.DR
{
	public class DRSingleProcessMultipleBaseCurrencies : PXGraphExtension<DRSingleProcess>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();
		}

		public delegate void SetFairValueSalesPriceDelegate(DRScheduleDetail scheduleDetail, Location location, CurrencyInfo currencyInfo);
		[PXOverride]
		public void SetFairValueSalesPrice(DRScheduleDetail scheduleDetail, Location location, CurrencyInfo currencyInfo, SetFairValueSalesPriceDelegate baseMethod)
		{
			var takeInBaseCurrency = currencyInfo.CuryID == currencyInfo.BaseCuryID || Base.Setup.Current.UseFairValuePricesInBaseCurrency.Value;
			DRSingleProcess.SetFairValueSalesPrice(Base.Schedule.Current, scheduleDetail, Base.ScheduleDetail, location, currencyInfo, takeInBaseCurrency);
		}
	}
}
