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

using System.Collections.Generic;
using Newtonsoft.Json;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonObject(Description = "Product (total  BigCommerce API v3 response)")]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class Product : IEntityResponse<ProductData>
    {
        [JsonProperty("data")]
        public ProductData Data { get; set; }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }

    [JsonObject(Description = "Product list (total  BigCommerce API v3 response)")]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class ProductList : IEntitiesResponse<ProductData>
    {
        private List<ProductData> _data;

        [JsonProperty("data")]
        public List<ProductData> Data
        {
            get => _data ?? (_data = new List<ProductData>());
            set => _data = value;
        }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }

	[JsonObject(Description = "Product list (total  BigCommerce API v3 response)")]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class ProductQtyList : IEntitiesResponse<ProductQtyData>
	{
		private List<ProductQtyData> _data;

		[JsonProperty("data")]
		public List<ProductQtyData> Data
		{
			get => _data ?? (_data = new List<ProductQtyData>());
			set => _data = value;
		}

		[JsonProperty("meta")]
		public Meta Meta { get; set; }
	}

	[JsonObject(Description = "Related Products List (total  BigCommerce API v3 response)")]
	public class RelatedProductsList : IEntitiesResponse<RelatedProductsData>
	{
		private List<RelatedProductsData> _data;

		[JsonProperty("data")]
		public List<RelatedProductsData> Data
		{
			get => _data ?? (_data = new List<RelatedProductsData>());
			set => _data = value;
		}

		[JsonProperty("meta")]
		public Meta Meta { get; set; }
	}
}




