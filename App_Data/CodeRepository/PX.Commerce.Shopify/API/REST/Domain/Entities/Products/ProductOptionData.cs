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
    [JsonObject(Description = "Product -> Product Option")]
	[CommerceDescription(ShopifyCaptions.ProductOptions, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class ProductOptionData
    {
		/// <summary>
		/// An unsigned 64-bit integer that's used as a unique identifier for the product option
		/// </summary>
		[JsonProperty("id")]
        public long? Id { get; set; }

		/// <summary>
		/// The unique numeric identifier for the product.
		/// </summary>
		[JsonProperty("product_id")]
        public long ProductId { get; set; }

		/// <summary>
		/// The display name of Option
		/// </summary>
        [JsonProperty("name")]
		[CommerceDescription(ShopifyCaptions.Name)]
        public string Name { get; set; }

		/// <summary>
		/// The position of Option
		/// </summary>
        [JsonProperty("position")]
		public int Position { get; set; }

		/// <summary>
		/// All available values in current product option
		/// </summary>
        [JsonProperty("values")]
        public string[] Values { get; set; }
        
	}
}
