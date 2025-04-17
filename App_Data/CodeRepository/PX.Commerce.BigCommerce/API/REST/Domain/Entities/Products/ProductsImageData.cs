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
using PX.Commerce.BigCommerce.API.WebDAV;
using PX.Commerce.Core;

namespace PX.Commerce.BigCommerce.API.REST
{
	[JsonObject(Description = "Product-> ProductImage")]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class ProductsImageData : BCAPIEntity, IWebDAVEntity
	{
		[JsonProperty("is_thumbnail")]
		public bool IsThumbnail { get; set; }

		[JsonProperty("sort_order")]
		public int SortOrder { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("id")]
		public int Id { get; set; }

		[JsonProperty("image_file")]
		public string ImageFile { get; set; }

		[JsonProperty("url_zoom")]
		public string UrlZoom { get; set; }

		[JsonProperty("url_standard")]
		public string UrlStandard { get; set; }

		[JsonProperty("url_thumbnail")]
		public string UrlThumbnail { get; set; }

		[JsonProperty("url_tiny")]
		public string UrlTiny { get; set; }

		[JsonProperty("date_modified")]
		public string DateModified { get; set; }

		[JsonProperty("image_url")]

		public string ImageUrl { get; set; }

	}
}
