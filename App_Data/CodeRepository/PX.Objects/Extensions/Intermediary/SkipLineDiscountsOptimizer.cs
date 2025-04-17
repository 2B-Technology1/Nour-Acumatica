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
using PX.Objects.AR;
using PX.Objects.Extensions.Discount;
using PX.Objects.Extensions.SalesPrice;
using System;

namespace PX.Objects.Extensions.Intermediary
{
	public abstract class SkipLineDiscountsOptimizer<TPriceExt, TDiscountExt, TGraph, TPrimary>
		: PXGraphExtension<TPriceExt, TDiscountExt, TGraph>
		where TPriceExt : SalesPriceGraph<TGraph, TPrimary>
		where TDiscountExt : DiscountGraph<TGraph, TPrimary>
		where TGraph : PXGraph
		where TPrimary : class, IBqlTable, new()
	{
		#region Shared between SalesPrice and Discount

		private ARSalesPriceMaint.SalesPriceItem _salesPriceItem = null;

		#endregion

		#region SalesPriceOverrides

		/// Overrides <see cref="SalesPriceGraph{TGraph, TPrimary}.GetSalesPriceItemAndCalculatedPrice"/>
		[PXOverride]
		public (ARSalesPriceMaint.SalesPriceItem, decimal?) GetSalesPriceItemAndCalculatedPrice(
			PXCache sender,
			string custPriceClass,
			int? customerID,
			int? inventoryID,
			int? siteID,
			CM.CurrencyInfo currencyinfo,
			string UOM,
			decimal? quantity,
			DateTime date,
			decimal? currentUnitPrice,
			string taxCalcMode,
			Func<PXCache, string, int?, int?, int?, CM.CurrencyInfo, string, decimal?, DateTime, decimal?, string,
				(ARSalesPriceMaint.SalesPriceItem, decimal?)> base_GetSalesPriceItemAndCalculatedPrice)
		{
			var ret = base_GetSalesPriceItemAndCalculatedPrice(sender, custPriceClass, customerID, inventoryID,
				siteID, currencyinfo, UOM, quantity, date, currentUnitPrice, taxCalcMode);
			_salesPriceItem = ret.Item1;
			return ret;
		}

		#endregion

		#region DiscountsOverrides

		/// Overrides <see cref="DiscountGraph{TGraph, TPrimary}.ProcessDiscountsOnDetailRowUpdated"/>
		[PXOverride]
		public void ProcessDiscountsOnDetailRowUpdated(
			Discount.Detail row,
			Discount.Detail oldRow,
			PXCache cache,
			Action<Discount.Detail, Discount.Detail, PXCache> base_ProcessDiscountsOnDetailRowUpdated)
		{
			if (_salesPriceItem != null)
			{
				Common.Discount.Mappers.DiscountLineFields.GetMapFor(row, cache).SkipLineDiscounts = _salesPriceItem.SkipLineDiscounts;
			}
			base_ProcessDiscountsOnDetailRowUpdated(row, oldRow, cache);
		}
		#endregion
	}
}
