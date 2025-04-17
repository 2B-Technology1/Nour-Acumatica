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
using System.Collections.Generic;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// The input fields required to update a catalog.
	/// </summary>
	public class CatalogUpdateInput
	{
		/// <summary>
		/// The context associated with the catalog.
		/// </summary>
		[JsonProperty("context", NullValueHandling = NullValueHandling.Ignore)]
		public CatalogContextInput Context { get; set; }

		/// <summary>
		/// The ID of the price list to associate to the catalog.
		/// </summary>
		[JsonProperty("priceListId", NullValueHandling = NullValueHandling.Ignore)]
		public string PriceListId { get; set; }

		/// <summary>
		/// The ID of the publication to associate to the catalog.
		/// </summary>
		[JsonProperty("publicationId", NullValueHandling = NullValueHandling.Ignore)]
		public string PublicationId { get; set; }

		/// <summary>
		/// The status of the catalog.
		/// </summary>
		[JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
		public string Status { get; set; }

		/// <summary>
		/// The name of the catalog.
		/// </summary>
		[JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
		public string Title { get; set; }

	}
}
