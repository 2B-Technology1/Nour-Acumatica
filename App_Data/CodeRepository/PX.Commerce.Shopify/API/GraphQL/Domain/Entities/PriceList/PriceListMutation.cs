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

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// Contains queries for mutations
	/// </summary>
	public class PriceListMutation
	{
		/// <summary>
		/// Creates the Publication.
		/// </summary>
		[JsonProperty("publicationCreate")]
		public PublicationCreatePayload PublicationCreate { get; set; }

		/// <summary>
		/// Creates a new catalog.
		/// </summary>
		[JsonProperty("catalogCreate")]
		public CatalogCreatePayload CatalogCreate { get; set; }

		/// <summary>
		/// Updates the catalog.
		/// </summary>
		[JsonProperty("catalogUpdate")]
		public CatalogUpdatePayload CatalogUpdate { get; set; }

		/// <summary>
		/// The payload for the catalogDelete mutation.
		/// </summary>
		[JsonProperty("catalogDelete")]
		public CatalogDeletePayload CatalogDelete { get; set; }

		/// <summary>
		/// Creates a new priceList.
		/// </summary>
		[JsonProperty("priceListCreate")]
		public PriceListCreatePayload PriceListCreate { get; set; }

		/// <summary>
		/// Updates the priceList.
		/// </summary>
		[JsonProperty("priceListUpdate")]
		public PriceListUpdatePayload PriceListUpdate { get; set; }

		/// <summary>
		/// The payload for the priceListDelete mutation.
		/// </summary>
		[JsonProperty("priceListDelete")]
		public PriceListDeletePayload PriceListDelete { get; set; }

		/// <summary>
		/// Creates or updates fixed prices on a price list. You can use the priceListFixedPricesAdd mutation to set a fixed price for specific product variants.
		/// This lets you change product variant pricing on a per country basis. Any existing fixed price list prices for these variants will be overwritten.
		/// </summary>
		[JsonProperty("priceListFixedPricesAdd")]
		public PriceListFixedPricesAddPayload PriceListFixedPricesAdd { get; set; }

		/// <summary>
		/// Updates fixed prices on a price list. You can use the priceListFixedPricesUpdate mutation to set a fixed price for specific product variants or to delete prices for variants associated with the price list.
		/// </summary>
		[JsonProperty("priceListFixedPricesUpdate")]
		public PriceListFixedPricesUpdatePayload PriceListFixedPricesUpdate { get; set; }

		/// <summary>
		/// Deletes specific fixed prices from a price list using a product variant ID. You can use the priceListFixedPricesDelete mutation to delete a set of fixed prices from a price list.
		/// After deleting the set of fixed prices from the price list, the price of each product variant reverts to the original price that was determined by the price list adjustment.
		/// </summary>
		[JsonProperty("priceListFixedPricesDelete")]
		public PriceListFixedPricesDeletePayload PriceListFixedPricesDelete { get; set; }
	}
}
