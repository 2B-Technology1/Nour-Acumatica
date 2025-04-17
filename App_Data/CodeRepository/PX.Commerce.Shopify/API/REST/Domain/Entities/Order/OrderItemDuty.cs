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
using PX.Commerce.Core;

namespace PX.Commerce.Shopify.API.REST
{
	[JsonObject(Description = "Duties")]
	[Description(ShopifyCaptions.Duties)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class OrderItemDuty : BCAPIEntity
	{
		[JsonProperty("id")]
		[Description(ShopifyCaptions.Id)]
		public long? Id { get; set; }

		[JsonProperty("harmonized_system_code")]
		public String HarmonizedSystemCode { get; set; }

		[JsonProperty("country_code_of_origin")]
		public String CountryCodeOfOrigin { get; set; }

		[JsonProperty("shop_money", NullValueHandling = NullValueHandling.Ignore)]
		public PresentmentPrice ShopMoney { get; set; }

		[JsonProperty("presentment_money", NullValueHandling = NullValueHandling.Ignore)]
		public PresentmentPrice PresentmentMoney { get; set; }

		[JsonProperty("tax_lines")]
		[Description(ShopifyCaptions.TaxLine)]
		public List<OrderTaxLine> TaxLines { get; set; }
	}

}
