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
	/// The input fields to create a price list.
	/// </summary>
	public class PriceListCreateInput
	{
		public PriceListCreateInput()
		{
			Parent = new PriceListParentCreateInput();
		}
		/// <summary>
		/// The ID of the catalog to associate with this price list.If the catalog was already associated with another price list then it will be unlinked.
		/// </summary>
		[JsonProperty("catalogId", NullValueHandling = NullValueHandling.Ignore)]
		public string CatalogId { get; set; }

		/// <summary>
		/// Three letter currency code for fixed prices associated with this price list.
		/// </summary>
		[JsonProperty("currency")]
		public string Currency { get; set; }

		/// <summary>
		/// Relative adjustments to other prices.
		/// </summary>
		[JsonProperty("parent")]
		public PriceListParentCreateInput Parent { get; set; }

		/// <summary>
		/// The unique name of the price list, used as a human-readable identifier.
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }

	}
}
