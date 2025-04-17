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
using System.ComponentModel;
using Newtonsoft.Json;

namespace PX.Commerce.Shopify.API.REST
{
    [JsonObject(Description = "Product Variant -> Presentment Prices")]
	[Description("Presentment Prices")]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class ProductVariantPresentmentPriceData
	{
		/// <summary>
		/// The variant's presentment prices
		/// </summary>
		[JsonProperty("price")]
        public PresentmentPrice Price { get; set; }

		/// <summary>
		/// The variant's presentment compare at price
		/// </summary>
		[JsonProperty("compare_at_price")]
        public PresentmentPrice OriginalPrice { get; set; }

        public PresentmentPrice AddPrice(PriceType priceType, String currencyCode, decimal amount )
		{
			var newPrice = new PresentmentPrice() { CurrencyCode = currencyCode, Amount = amount };
			switch(priceType)
			{
				case PriceType.Price: Price = newPrice; break;
				case PriceType.CompareAtPrice: OriginalPrice = newPrice; break;
				default: break;
			}
			return newPrice;
		}

		public enum PriceType
		{
			Price,
			CompareAtPrice
		}
	}
}
