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
using System.ComponentModel;

namespace PX.Commerce.Shopify.API.REST
{
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class FilterProducts : FilterWithDateTimeAndLimit, IFilterWithIDs, IFilterWithFields, IFilterWithSinceID
	{
		/// <summary>
		/// Restrict results to customers specified by a comma-separated list of IDs.
		/// </summary>
		[Description("ids")]
		public string IDs { get; set; }

		/// <summary>
		/// Restrict results to those after the specified ID.
		/// </summary>
		[Description("since_id")]
		public string SinceID { get; set; }

		/// <summary>
		/// Show only certain fields, specified by a comma-separated list of field names.
		/// </summary>
		[Description("fields")]
		public string Fields { get; set; }

		/// <summary>
		/// Filter results by product title.
		/// </summary>
		[Description("title")]
		public string Title { get; set; }

		/// <summary>
		/// Filter results by product vendor.
		/// </summary>
		[Description("vendor")]
		public string Vendor { get; set; }

		/// <summary>
		/// Filter results by product handle.
		/// </summary>
		[Description("handle")]
		public string Handle { get; set; }

		/// <summary>
		/// Filter results by product type.
		/// </summary>
		[Description("product_type")]
		public string ProductType { get; set; }

		/// <summary>
		/// Filter results by product collection ID.
		/// </summary>
		[Description("collection_id")]
		public string CollectionId { get; set; }

		/// <summary>
		/// Show products published after date. (format: 2014-04-25T16:15:47-04:00)
		/// </summary>
		[Description("published_at_min")]
		public string PublishedAtMin { get; set; }

		/// <summary>
		/// Show products published before date. (format: 2014-04-25T16:15:47-04:00)
		/// </summary>
		[Description("published_at_max")]
		public string PublishedAtMax { get; set; }

		/// <summary>
		/// Return products by their published status
		/// (default: any)
		/// published: Show only published products.
		/// unpublished: Show only unpublished products.
		/// any: Show all products.
		/// </summary>
		[Description("published_status")]
		public string PublishedStatus { get; set; }

		/// <summary>
		/// Return presentment prices in only certain currencies, specified by a comma-separated list of ISO 4217 currency codes.
		/// </summary>
		[Description("presentment_currencies")]
		public string PresentmentCurrencies { get; set; }
	}
}
