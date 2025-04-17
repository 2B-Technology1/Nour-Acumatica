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
    [JsonObject(Description = "Product Image")]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	class ProductsImage : IEntityResponse<ProductsImageData>
    {
        [JsonProperty("data")]
        public ProductsImageData Data { get; set; }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class ProductsImageList : IEntitiesResponse<ProductsImageData>
    {
        private List<ProductsImageData> _data;

        [JsonProperty("data")]
        public List<ProductsImageData> Data
        {
            get => _data ?? (_data = new List<ProductsImageData>());
            set => _data = value;
        }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }
}
