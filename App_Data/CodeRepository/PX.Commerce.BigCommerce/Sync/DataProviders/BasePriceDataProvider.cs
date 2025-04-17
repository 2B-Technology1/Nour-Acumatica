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

using PX.Commerce.Core;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.Common;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Commerce.BigCommerce
{
	/// <summary>
	/// Data provider for base price processor
	/// </summary>
	public interface IBasePriceDataProvider
	{
		/// <summary>
		/// Returns the list of base prices to be synced to the external system.
		/// </summary>
		/// <param name="graph"></param>
		/// <param name="bindingId"></param>
		/// <param name="connectorType"></param>
		/// <param name="ids"></param>
		/// <returns></returns>
		IEnumerable<BasePriceDetailsDTO> RetrieveBasePricesByIds(List<BCSyncStatus> ids, PXGraph graph, int bindingId, string connectorType);
		/// <summary>
		/// Retrieves all updated base prices that must be fetched.
		/// </summary>
		/// <param name="graph"></param>
		/// <param name="prepareMode"></param>
		/// <param name="startDate"></param>
		/// <param name="bindingId"></param>
		/// <param name="connectorType"></param>
		/// <returns></returns>
		IEnumerable<BasePriceDetailsDTO> RetrieveBasePriceListToFetch(PrepareMode prepareMode, DateTime? startDate, PXGraph graph,  int bindingId, string connectorType);
	}

	/// <summary>
	/// Provides the two methods to retrieve data for both the Fetch and GetBucket methods (Export)	
	/// </summary>
	public class BasePriceDataProvider : IBasePriceDataProvider
	{

		/// <summary>
		/// Returns the list of updated prices from the database.
		/// This list contains all the prices (defined by the price type) and their statuses (BCSyncStatus record) if it exists in the database for both
		/// the price and the inventory item. 
		/// It returns:
		/// Sale Prices that have a corresponding InventoryItem record in the database and of type <paramref name="priceType"/> 
		/// (Inner Join) Inventory Item related to the Sale price (*)
		/// (Outer Join) BCSyncStatus record of the corresponding Sale Price
		/// (Outer Join) BCSyncStatus record of the corresponding to the inventory item
		/// (Outer Join) BCSyncStatus record of the corresponding to the template item (in case the inventory item is a variant)
		/// (Outer Join) BCSyncDetail record of the corresponding to the variant item (in case the inventory item is a variant)
		/// (Inner Join) To all the sale prices that have been defined for the InventoryItem (*)
		/// Conditions:
		/// InventoryItem.Status is not equal Deleted, Unknown or Inactive
		/// InventoryItem.EntityType is one of StockItem, NonStockItem or ProductWithVariant
		/// </summary>
		/// <returns></returns>
		public  IEnumerable<BasePriceDetailsDTO> RetrieveBasePriceListToFetch(PrepareMode prepareMode, DateTime? startDate, PXGraph graph, int bindingId, string connectorType)
		{
			var baseQuery = new SelectFrom<PX.Objects.AR.ARSalesPrice>.
					InnerJoin<InventoryItem>.On<InventoryItem.inventoryID.IsEqual<ARSalesPrice.inventoryID>>
					.LeftJoin<INSite>.On<INSite.siteID.IsEqual<ARSalesPrice.siteID>>
					.LeftJoin<BCSyncStatus>.On<BCSyncStatus.connectorType.IsEqual<@P.AsString>
												.And<BCSyncStatus.bindingID.IsEqual<@P.AsInt>>
												.And<BCSyncStatus.entityType.IsEqual<@P.AsString>>
												.And<BCSyncStatus.localID.IsEqual<InventoryItem.noteID>>>
					.LeftJoin<BCSyncStatusForInventoryItem>.On<BCSyncStatusForInventoryItem.connectorType.IsEqual<@P.AsString>
														   .And<BCSyncStatusForInventoryItem.bindingID.IsEqual<@P.AsInt>
														   .And<BCSyncStatusForInventoryItem.entityType.IsInSequence<InventoryItemsEntityTypeSequence>>
														   .And<BCSyncStatusForInventoryItem.localID.IsEqual<InventoryItem.noteID>>>>
					.LeftJoin<BCSyncDetailForVariantInventoryItem>.On<Brackets<BCSyncDetailForVariantInventoryItem.entityType.IsEqual<@P.AsString>
																	.And<BCSyncDetailForVariantInventoryItem.localID.IsEqual<InventoryItem.noteID>>>>															   
					.LeftJoin<BCSyncStatusForVariantInventoryItem>.On<BCSyncStatusForVariantInventoryItem.connectorType.IsEqual<@P.AsString>
															  .And<BCSyncStatusForVariantInventoryItem.bindingID.IsEqual<@P.AsInt>
															  .And<BCSyncStatusForVariantInventoryItem.entityType.IsInSequence<InventoryItemsEntityTypeSequence>>
															  .And<BCSyncStatusForVariantInventoryItem.syncID.IsEqual<BCSyncDetailForVariantInventoryItem.syncID>>
															  .And<InventoryItem.isTemplate.IsEqual<False>>>>
					.Where<ARSalesPrice.priceType.IsEqual<PriceTypes.basePrice>
					.And<Brackets<BCSyncStatusForInventoryItem.syncID.IsNotNull.Or<BCSyncStatusForVariantInventoryItem.syncID.IsNotNull>>>
					.And<InventoryItem.itemStatus.IsNotInSequence<ExcludedInventoryItemStatusSequence>>>();

			List<object> param = new List<object>() { connectorType, bindingId, BCEntitiesAttribute.SalesPrice,
													  connectorType, bindingId, BCEntitiesAttribute.Variant, connectorType, bindingId};

			BqlCommand cmd = baseQuery;

			if (prepareMode != PrepareMode.Full)
			{
				//Look only for prices that are new or have changed since the last sync
				//Note that in the  GetBucketExport method, we will retrieve all the prices and not only those that have changed.
				//However, if an inventory item has no price that changed since the last sync, or no price that expired or became effective,
				//then no need to update
				cmd = cmd.WhereAnd<Where<BCSyncStatus.syncID.IsNull.Or<Brackets<BCSyncStatus.syncID.IsNotNull.And<ARSalesPrice.lastModifiedDateTime.IsGreater<BCSyncStatus.lastOperationTS>>>>
																				.Or<Brackets<BCSyncStatus.syncID.IsNotNull.And<ARSalesPrice.expirationDate.IsGreater<BCSyncStatus.lastOperationTS>>>>
																				.Or<Brackets<BCSyncStatus.syncID.IsNotNull.And<ARSalesPrice.effectiveDate.IsGreater<BCSyncStatus.lastOperationTS>>>>>>();
			}

			if (prepareMode == PrepareMode.Full && startDate.HasValue)
			{
				//Retrieve only the prices that have changed since the specified date or never been synced yet.
				cmd = cmd.WhereAnd<Where<BCSyncStatus.syncID.IsNull.Or<BCSyncStatus.syncID.IsNotNull.And<ARSalesPrice.lastModifiedDateTime.IsGreater<@P.AsDateTime>>>>>();
				param.Add(startDate);
			}

			var result = cmd.CreateView(graph).SelectMulti(param.ToArray())
										.Cast<PXResult<PX.Objects.AR.ARSalesPrice>>().ToList();

			foreach (var item in result)
			{
				var dto = new BasePriceDetailsDTO();
				dto.Init(salePrice: item.GetItem<ARSalesPrice>(), inventoryItem: item.GetItem<InventoryItem>(), warehouse: item.GetItem<INSite>(),
						 priceSyncStatus: item.GetItem<BCSyncStatus>(), priceSyncDetails: null, inventorySyncStatus: item.GetItem<BCSyncStatusForInventoryItem>(),
						 variantInventorySyncStatus: item.GetItem<BCSyncStatusForVariantInventoryItem>(), variantInventorySyncDetail: item.GetItem<BCSyncDetailForVariantInventoryItem>());
				yield return dto;
			}

			var priceListWithDeletedItems = GetPriceListWithDeletions(BCEntitiesAttribute.SalesPrice, BCEntitiesAttribute.BulkPrice, graph, bindingId, connectorType).ToList();
			//Get the list for prices that have been deleted from the database.
			foreach (var item in priceListWithDeletedItems)
				yield return item;
		}

		/// <summary>
		/// This method returns a list of PriceListDetails based on the list of ids passed as parameter. 
		/// It returns only the prices that have a corresponding InventoryItem record in the database and of type <paramref name="priceType"/> with 
		/// </summary>
		/// <param name="graph"></param>
		/// <param name="bindingId"></param>
		/// <param name="connectorType"></param>
		/// <param name="ids">List of BCSyncStatus Ids to be returned</param>
		/// <returns></returns>
		public IEnumerable<BasePriceDetailsDTO> RetrieveBasePricesByIds(List<BCSyncStatus> ids, PXGraph graph, int bindingId, string connectorType)
		{
			var baseQuery = new SelectFrom<InventoryItem>
					.InnerJoin<BCSyncStatus>.On<BCSyncStatus.localID.IsEqual<InventoryItem.noteID>
								.And<BCSyncStatus.connectorType.IsEqual<@P.AsString>>
								.And<BCSyncStatus.bindingID.IsEqual<@P.AsInt>>
								.And<BCSyncStatus.entityType.IsEqual<@P.AsString>>>

					.LeftJoin<ARSalesPrice>.On<Brackets<InventoryItem.inventoryID.IsEqual<ARSalesPrice.inventoryID>.And<ARSalesPrice.priceType.IsEqual<PriceTypes.basePrice>>>>
					.LeftJoin<INSite>.On<INSite.siteID.IsEqual<ARSalesPrice.siteID>>
					.LeftJoin<BCSyncStatusForInventoryItem>.On<BCSyncStatusForInventoryItem.connectorType.IsEqual<@P.AsString>
														   .And<BCSyncStatusForInventoryItem.bindingID.IsEqual<@P.AsInt>
														   .And<BCSyncStatusForInventoryItem.entityType.IsInSequence<InventoryItemsEntityTypeSequence>>
														   .And<BCSyncStatusForInventoryItem.localID.IsEqual<InventoryItem.noteID>>>>
					.LeftJoin<BCSyncDetailForVariantInventoryItem>.On<Brackets<BCSyncDetailForVariantInventoryItem.entityType.IsEqual<@P.AsString>
																	.And<BCSyncDetailForVariantInventoryItem.localID.IsEqual<InventoryItem.noteID>>>>
					.LeftJoin<BCSyncStatusForVariantInventoryItem>.On<BCSyncStatusForVariantInventoryItem.connectorType.IsEqual<@P.AsString>
															  .And<BCSyncStatusForVariantInventoryItem.bindingID.IsEqual<@P.AsInt>
															  .And<BCSyncStatusForVariantInventoryItem.entityType.IsInSequence<InventoryItemsEntityTypeSequence>>
															  .And<BCSyncStatusForVariantInventoryItem.syncID.IsEqual<BCSyncDetailForVariantInventoryItem.syncID>
															  .And<InventoryItem.isTemplate.IsEqual<False>>>>>
					.Where<Brackets<BCSyncStatusForInventoryItem.syncID.IsNotNull.Or<BCSyncStatusForVariantInventoryItem.syncID.IsNotNull>>
						.And<InventoryItem.itemStatus.IsNotInSequence<ExcludedInventoryItemStatusSequence>>
						.And<BCSyncStatus.syncID.IsIn<@P.AsInt>>>();

			var result = baseQuery.CreateView(graph).SelectMulti(connectorType, bindingId, BCEntitiesAttribute.SalesPrice, connectorType, bindingId, BCEntitiesAttribute.Variant, connectorType, bindingId, ids.Select(x => x.SyncID).ToArray())
										.Cast<PXResult<InventoryItem>>();
			foreach (var item in result)
			{
				var dto = new BasePriceDetailsDTO();
				dto.Init(salePrice: item.GetItem<ARSalesPrice>(), inventoryItem: item.GetItem<InventoryItem>(), warehouse:item.GetItem<INSite>(),
							 priceSyncStatus:item.GetItem<BCSyncStatus>(), priceSyncDetails: null, inventorySyncStatus: item.GetItem<BCSyncStatusForInventoryItem>(),
							 variantInventorySyncStatus: item.GetItem<BCSyncStatusForVariantInventoryItem>(), variantInventorySyncDetail: item.GetItem<BCSyncDetailForVariantInventoryItem>());

				yield return dto;
			}
		}

		protected const int MaxItemsPerQuery = 1000;
		/// <summary>
		/// Checks the BCSyncDetails for records that have no corresponding ARSalesPrice record in the database.
		/// Picks their corresponding BCSyncStatus records and returns them as a list of PriceListDetails because they need to be deleted from the external system
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerable<BasePriceDetailsDTO> GetPriceListWithDeletions(string syncEntityType, string syncDetailEntityType, PXGraph graph, int bindingId, string connectorType)
		{
			var baseQuery = new SelectFrom<BCSyncDetail>
					.InnerJoin<BCSyncStatus>.On<BCSyncStatus.connectorType.IsEqual<@P.AsString>
											.And<BCSyncStatus.bindingID.IsEqual<@P.AsInt>>
											.And<BCSyncStatus.entityType.IsEqual<@P.AsString>>
											.And<BCSyncStatus.syncID.IsEqual<BCSyncDetail.syncID>>>
					.LeftJoin<ARSalesPrice>.On<ARSalesPrice.noteID.IsEqual<BCSyncDetail.localID>>
					.Where<ARSalesPrice.recordID.IsNull
					.And<BCSyncDetail.entityType.IsEqual<@P.AsString>>>();

			var result = baseQuery.CreateView(graph).SelectMulti(connectorType, bindingId, syncEntityType, syncDetailEntityType)
										.Cast<PXResult<BCSyncDetail>>();
			var ids = new List<BCSyncStatus>();
			var itemsThatContainDeletedPrices = new List<BasePriceDetailsDTO>();
			foreach (var item in result)
			{
				var status = item.GetItem<BCSyncStatus>();
				ids.Add(status);
				if (ids.Count == MaxItemsPerQuery)
				{
					var range = RetrieveBasePricesByIds(ids, graph, bindingId, connectorType).ToList();
					itemsThatContainDeletedPrices.AddRange(range);
					ids.Clear();
				}
			}
			if (ids.Count > 0)
			{
				var range = RetrieveBasePricesByIds(ids, graph, bindingId, connectorType).ToList();
				itemsThatContainDeletedPrices.AddRange(range);
				ids.Clear();
			}

			return itemsThatContainDeletedPrices;
		}
	}
}
