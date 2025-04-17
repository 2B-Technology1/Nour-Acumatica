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
using System.Threading;
using System.Threading.Tasks;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// Provides functionality for performing CRUD tasks with priceList in an external system.
	/// </summary>
	public interface IPriceListDataGQLProvider
	{
		/// <summary>
		/// Create a new catalog in the store.
		/// </summary>
		/// <param name="catalogInput"></param>
		/// <param name="cancellationToken">Cancellation token for the operation.</param>
		/// <returns>The created catalog.</returns>
		public Task<CompanyLocationCatalogGQL> CreateCatalog(CatalogCreateInput catalogInput, CancellationToken cancellationToken = default);

		/// <summary>
		/// Update a catalog in the store.
		/// </summary>
		/// <param name="catalogId">Catalog ID</param>
		/// <param name="catalogInput"></param>
		/// <param name="cancellationToken"></param>
		/// <returns>The updated catalog</returns>
		public Task<CompanyLocationCatalogGQL> UpdateCatalog(string catalogId, CatalogUpdateInput catalogInput, CancellationToken cancellationToken = default);

		/// <summary>
		/// Delete a catalog in the store
		/// </summary>
		/// <param name="catalogId">Catalog ID</param>
		/// <param name="deleteDependentResources"></param>
		/// <param name="cancellationToken"></param>
		/// <returns>The deleted catalog ID</returns>
		public Task<string> DeleteCatalog(string catalogId, bool deleteDependentResources = true, CancellationToken cancellationToken = default);

		/// <summary>
		/// Create a new Publication
		/// </summary>
		/// <param name="publicationInput"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<PublicationGQL> CreatePublication(PublicationCreateInput publicationInput, CancellationToken cancellationToken = default);

		/// <summary>
		/// Create a new priceList in the store.
		/// </summary>
		/// <param name="priceListInput"></param>
		/// <param name="cancellationToken"></param>
		/// <returns>The created PriceList</returns>
		public Task<PriceListGQL> CreatePriceList(PriceListCreateInput priceListInput, CancellationToken cancellationToken = default);

		/// <summary>
		/// Update a PriceList in the store
		/// </summary>
		/// <param name="priceListId">PriceList ID</param>
		/// <param name="priceListInput"></param>
		/// <param name="cancellationToken"></param>
		/// <returns>The updated PriceList</returns>
		public Task<PriceListGQL> UpdatePriceList(string priceListId, PriceListUpdateInput priceListInput, CancellationToken cancellationToken = default);

		/// <summary>
		/// Delete a PriceList in the store
		/// </summary>
		/// <param name="priceListId"></param>
		/// <param name="cancellationToken"></param>
		/// <returns>The deleted PriceList ID</returns>
		public Task<string> DeletePriceList(string priceListId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Add the Prices to the PriceList
		/// </summary>
		/// <param name="priceListId"></param>
		/// <param name="priceListPriceInputs"></param>
		/// <param name="cancellationToken"></param>
		/// <returns>The added prices in the PriceList</returns>
		public Task<IEnumerable<PriceListPriceGQL>> AddPriceListFixedPrices(string priceListId, List<PriceListPriceInput> priceListPriceInputs, CancellationToken cancellationToken = default);

		/// <summary>
		/// Update the Prices in the PriceList
		/// </summary>
		/// <param name="priceListId"></param>
		/// <param name="priceListPriceInputs"></param>
		/// <param name="variantIdsToDelete"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<PriceListFixedPricesUpdatePayload> UpdatePriceListFixedPrices(string priceListId, List<PriceListPriceInput> priceListPriceInputs, string[] variantIdsToDelete = null, CancellationToken cancellationToken = default);

		/// <summary>
		/// Delete Prices in the PriceList
		/// </summary>
		/// <param name="priceListId"></param>
		/// <param name="variantIdsToDelete"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<IEnumerable<string>> DeletePriceListFixedPrices(string priceListId, string[] variantIdsToDelete, CancellationToken cancellationToken = default);

		/// <summary>
		/// Get Catalogs in the store with/without filter string
		/// </summary>
		/// <param name="filterString"></param>
		/// <param name="includedSubFields"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="specifiedFieldsOnly"></param>
		/// <returns></returns>
		public Task<IEnumerable<CompanyLocationCatalogGQL>> GetCatalogs(string filterString = null, bool includedSubFields = false, CancellationToken cancellationToken = default, params string[] specifiedFieldsOnly);

		/// <summary>
		/// Get PriceLists in the store with/without filter string
		/// </summary>
		/// <param name="filterString"></param>
		/// <param name="includedSubFields"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="specifiedFieldsOnly"></param>
		/// <returns></returns>
		public Task<IEnumerable<PriceListGQL>> GetPriceLists(string filterString = null, bool includedSubFields = false, CancellationToken cancellationToken = default, params string[] specifiedFieldsOnly);

		/// <summary>
		/// Get the Catalog by ID
		/// </summary>
		/// <param name="id"></param>
		/// <param name="withSubFields"></param>
		/// <param name="withSubConnections"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="specifiedFieldsOnly"></param>
		/// <returns></returns>
		public  Task<CompanyLocationCatalogGQL> GetCatalogByID(string id, bool withSubFields = true, bool withSubConnections = false, CancellationToken cancellationToken = default, params string[] specifiedFieldsOnly);


		/// <summary>
		/// Get the PriceList by ID
		/// </summary>
		/// <param name="id"></param>
		/// <param name="withSubFields"></param>
		/// <param name="withSubConnections"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="specifiedFieldsOnly"></param>
		/// <returns></returns>
		public Task<PriceListGQL> GetPriceListByID(string id, bool withSubFields = true, bool withSubConnections = false, CancellationToken cancellationToken = default, params string[] specifiedFieldsOnly);

		/// <summary>
		/// Get the Shop info
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <param name="specifiedFieldsOnly"></param>
		/// <returns></returns>
		public Task<ShopGQL> GetShop(CancellationToken cancellationToken = default, params string[] specifiedFieldsOnly);
	}
}
