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

using Newtonsoft.Json;
using PX.Commerce.Core;
using System;

namespace PX.Commerce.BigCommerce.API.REST
{
	[JsonObject(Description = "Product -> Product Option -> Product Option Value")]
	public class ProductOptionValueData : BCAPIEntity
	{
		public ProductOptionValueData()
		{
		}

		[JsonProperty("id")]
		public int? Id { get; set; }

		[JsonProperty("label")]
		public string Label { get; set; }

		[JsonProperty("sort_order")]
		public int? SortOrder { get; set; }

		[JsonProperty("value_data")]
		public ValueData ValueData { get; set; }

		[JsonProperty("is_default")]
		public bool IsDefault { get; set; }

		[JsonIgnore]
		public Guid? LocalID { get; set; }
	}


	[JsonObject(Description = "Product -> Product Option -> Product Option Value ->ValueData")]
	public class ValueData : BCAPIEntity
	{
		/*The swatch type option can accept an array of colors, with up to three hexidecimal color keys;
		 or an image_url, which is a full image URL path including protocol.*/
		[JsonProperty("colors")]
		public string[] Colors { get; set; }

		/*The swatch type option can accept an array of colors, with up to three hexidecimal color keys;
		or an image_url, which is a full image URL path including protocol.*/
		[JsonProperty("image_url")]
		public string ImageUrl { get; set; }

		//The product list type option requires a product_id.

		[JsonProperty("list")]
		public string List { get; set; }

		/*The checkbox type option requires a boolean flag, called checked_value, 
		 * to determine which value is considered to be the checked state*/
		[JsonProperty("checked_value")]
		public bool CheckedValue { get; set; }

	}
}
