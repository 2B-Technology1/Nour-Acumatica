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
	/// The input fields required to create a publication.
	/// </summary>
	public class PublicationCreateInput
	{
		/// <summary>
		/// Whether to automatically add newly created products to this publication.
		/// </summary>
		[JsonProperty("autoPublish")]
		public bool AutoPublish { get; set; } = true;
		/// <summary>
		/// The ID of the catalog.
		/// </summary>
		[JsonProperty("catalogId", NullValueHandling = NullValueHandling.Ignore)]
		public string CatalogId { get; set; }

		/// <summary>
		/// Whether to create an empty publication or prepopulate it with all products.
		/// EMPTY
		/// ALL_PRODUCTS
		/// </summary>
		[JsonProperty("defaultState")]
		public string DefaultState { get; set; } = "ALL_PRODUCTS";

	}
}
