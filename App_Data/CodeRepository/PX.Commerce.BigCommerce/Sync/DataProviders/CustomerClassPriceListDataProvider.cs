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
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using PX.Objects.AR;
using PX.Objects.Common;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data.SQLTree;
using Microsoft.Extensions.Logging;

namespace PX.Commerce.BigCommerce
{ 
	/// <summary>
	/// Data Provider interface to implement data provider for the Price List Processor.
	/// </summary>
	public interface ICustomerClassPriceListDataProvider
	{
		/// <summary>
		/// Returns the list of Price Lists that have been created in the database and which CustomerClass has been already synced with BC.
		/// </summary>
		/// <param name="graph"></param>
		/// <param name="bindingId"></param>
		/// <param name="connectorType"></param>
		/// <returns></returns>
		IEnumerable<PriceListDescriptorDTO> RetrieveExistingPriceLists(PXGraph graph, int bindingId, string connectorType);

		/// <summary>
		/// Returns informations on a particular price list.
		/// </summary>
		/// <param name="priceListNoteId"></param>
		/// <param name="graph"></param>
		/// <param name="bindingId"></param>
		/// <param name="connectorType"></param>
		/// <returns></returns>
		PriceListDescriptorDTO RetrievePriceListInfoByNoteId(Guid priceListNoteId, PXGraph graph, int bindingId, string connectorType);

		/// <summary>
		/// Returns all the prices that have been updated since the last sync date for the given price list.
		/// This method is used only to indicate that a particular price list has been updated and needs to be synced.
		/// Used in the FetchBuckExport method.
		/// </summary>
		/// <param name="priceListNoteId"></param>
		/// <param name="graph"></param>
		/// <param name="prepareMode"></param>
		/// <param name="startDate"></param>
		/// <param name="bindingId"></param>
		/// <param name="connectorType"></param>
		/// <returns></returns>
		IEnumerable<PriceListDetailsDTO> RetrieveUpdatedPricesByListId(Guid? priceListNoteId, PrepareMode prepareMode, DateTime? startDate, PXGraph graph, int bindingId, string connectorType);

		///// <summary>
		///// Retrieve all the updated prices with the corresponding statuses and inventory items.
		///// </summary>
		///// <param name="priceClassNoteId"></param>
		///// <param name="startDate"></param>
		///// <param name="prepareMode"></param>
		///// <param name="graph"></param>
		///// <param name="bindingId"></param>
		///// <param name="connectorType"></param>
		///// <returns></returns>
		//IEnumerable<PriceListDetailsDTO> RetrieveInventoryItemsPriceChangesForPriceList(Guid priceClassNoteId, DateTime? startDate, PrepareMode prepareMode, PXGraph graph, int bindingId, string connectorType);

		/// <summary>
		/// Retrieve all Inventory Items that are related to the template item (in case the inventory item is a variant) for which no price has been synced yet.
		/// Must return inventory items only and only if no variant of the same matrix has a price synced.
		/// </summary>
		/// <param name="templateId"></param>
		/// <param name="priceListNoteId"></param>
		/// <param name="graph"></param>
		/// <param name="bindingId"></param>
		/// <param name="connectorType"></param>
		/// <returns></returns>
		IEnumerable<PriceListDetailsDTO> RetrieveInventoryVariantItemsForTempateID(int templateId, Guid priceListNoteId, PXGraph graph, int bindingId, string connectorType);
	}

	///<inheritdoc/>
	public class CustomerClassPriceListDataProvider : ICustomerClassPriceListDataProvider
	{
		/// <inheritdoc/>
		public IEnumerable<PriceListDetailsDTO> RetrieveUpdatedPricesByListId(Guid? priceListNoteId, PrepareMode prepareMode, DateTime? startDate, PXGraph graph, int bindingId, string connectorType)
		{			
			var listOfItemsWithChangedPrices = GetInventoryItemsThatHaveChangedPrices(priceListNoteId.Value, startDate, prepareMode, graph, bindingId, connectorType);
			var listOfItemsWithDeletedPrices = GetInventoryItemsThatHaveDeletedPrices(priceListNoteId.Value, graph, bindingId, connectorType);
			var totalList = listOfItemsWithChangedPrices.Union(listOfItemsWithDeletedPrices).ToList();

			var bacthSize = 100;
			var subList = new List<int>();	
			foreach (var item in totalList)
			{
				subList.Add(item);
				if (subList.Count == bacthSize)
				{
					var prices = GetPricesForInventoriesByBatch(priceListNoteId, subList, graph, bindingId, connectorType);
					foreach (var price in prices)
						yield return price;
					subList.Clear();
				}
			}

			if (subList.Count > 0)
			{
				var prices = GetPricesForInventoriesByBatch(priceListNoteId, subList, graph, bindingId, connectorType);
				foreach (var price in prices)
					yield return price;
				subList.Clear();
			}
		}

		/// <inheritdoc/>
		public IEnumerable<PriceListDescriptorDTO> RetrieveExistingPriceLists(PXGraph graph, int bindingId, string connectorType)
		{
			return RetrievePriceListDescriptors(null, graph, bindingId, connectorType);
		}

		/// <inheritdoc/>
		public PriceListDescriptorDTO RetrievePriceListInfoByNoteId(Guid priceListNoteId, PXGraph graph, int bindingId, string connectorType)
		{
			var list =  RetrievePriceListDescriptors(priceListNoteId, graph, bindingId, connectorType);
			if (list == null)
				return null;
			return list.FirstOrDefault();
		} 
			
		/// <inheritdoc/>
		public IEnumerable<PriceListDetailsDTO> RetrieveInventoryVariantItemsForTempateID(int templateId, Guid priceListNoteId,  PXGraph graph, int bindingId, string connectorType)
		{
			var query = new SelectFrom<InventoryItem>
							.LeftJoin<ARSalesPrice>.On<ARSalesPrice.inventoryID.IsEqual<InventoryItem.inventoryID>>
							.LeftJoin<ARPriceClass>.On<ARPriceClass.priceClassID.IsEqual<ARSalesPrice.custPriceClassID>
												   .And<ARPriceClass.noteID.IsEqual<@P.AsGuid>>>
							.Where<InventoryItem.templateItemID.IsEqual<@P.AsInt>>();
			BqlCommand cmd = query;

			List<object> param = new List<object>() { priceListNoteId, templateId };
			var result = cmd.CreateView(graph).SelectMulti(param.ToArray())
										.Cast<PXResult<InventoryItem>> ();
			foreach( var item in result )
			{
				var dto = new PriceListDetailsDTO();
				dto.InventoryItem = item.GetItem<InventoryItem>();
				dto.SalesPrice = item.GetItem<ARSalesPrice>();

				yield return dto;
			}
		}

		/// <summary>
		/// Return the whole list of prices for a particular a list of inventory items
		/// with the sync status and the corrsponding AR prices records.
		/// Note that if an inventory item has no prices, this method will return anyway the inventory items with no prices
		/// so that we can delete it afterwards in the external system.
		/// </summary>
		/// <param name="priceListNoteId"></param>
		/// <param name="inventoryIds"></param>
		/// <param name="graph"></param>
		/// <param name="bindingId"></param>
		/// <param name="connectorType"></param>
		/// <returns></returns>ConvertToStr
		protected virtual IEnumerable<PriceListDetailsDTO> GetPricesForInventoriesByBatch(Guid? priceListNoteId, List<int> inventoryIds, PXGraph graph, int bindingId, string connectorType)
		{
			var baseQuery = new SelectFrom<InventoryItem>
							.InnerJoin<ARPriceClass>.On<ARPriceClass.noteID.IsEqual<@P.AsGuid>>
							//Status of the price class - must be there otherwise we can not sync the price list.
							.InnerJoin<BCSyncStatusForCustomerPriceClass>.On<BCSyncStatusForCustomerPriceClass.connectorType.IsEqual<@P.AsString>
																 .And<BCSyncStatusForCustomerPriceClass.bindingID.IsEqual<@P.AsInt>>
																 .And<BCSyncStatusForCustomerPriceClass.entityType.IsEqual<@P.AsString>>
																 .And<BCSyncStatusForCustomerPriceClass.externID.IsNotNull>
																 .And<BCSyncStatusForCustomerPriceClass.localID.IsEqual<ARPriceClass.noteID>>
																 .And<BCSyncStatusForCustomerPriceClass.status.IsNotInSequence<InvalidSyncStatusSquence>>>
							//Status of the inventory must be there. Otherwise, it means that it has never been synced yet.
							.LeftJoin<BCSyncStatusForInventoryItem>.On<BCSyncStatusForInventoryItem.connectorType.IsEqual<@P.AsString>
															   .And<BCSyncStatusForInventoryItem.bindingID.IsEqual<@P.AsInt>
															   .And<BCSyncStatusForInventoryItem.entityType.IsInSequence<InventoryItemsEntityTypeSequence>>
															   .And<BCSyncStatusForInventoryItem.localID.IsEqual<InventoryItem.noteID>>>>
							//Left join to ensure that we also get the Inventory Items for which all prices have been deleted.
							.LeftJoin<ARSalesPrice>.On<InventoryItem.inventoryID.IsEqual<ARSalesPrice.inventoryID>
												   .And<ARSalesPrice.priceType.IsEqual<PriceTypes.customerPriceClass>
												   .And<ARSalesPrice.custPriceClassID.IsEqual<ARPriceClass.priceClassID>>>>
							//Get the status of the pricelist if any
							.LeftJoin<BCSyncStatus>.On<BCSyncStatus.connectorType.IsEqual<@P.AsString>
												   .And<BCSyncStatus.bindingID.IsEqual<@P.AsInt>>
												   .And<BCSyncStatus.entityType.IsEqual<@P.AsString>>
												   .And<BCSyncStatus.localID.IsEqual<ARPriceClass.noteID>>>
							.LeftJoin<BCSyncDetail>.On<BCSyncDetail.syncID.IsEqual<BCSyncStatus.syncID>
												   .And<Brackets<Brackets<ARSalesPrice.recordID.IsNull.And<BCSyncDetail.refNoteID.IsEqual<InventoryItem.noteID>>>
												   .Or<Brackets<ARSalesPrice.recordID.IsNotNull.And<BCSyncDetail.localID.IsEqual<ARSalesPrice.noteID>>>>>>>												   
							.LeftJoin<BCSyncDetailForVariantInventoryItem>.On<Brackets<BCSyncDetailForVariantInventoryItem.localID.IsEqual<InventoryItem.noteID>
														   .And<BCSyncDetailForVariantInventoryItem.entityType.IsEqual<@P.AsString>>>>
							.LeftJoin<BCSyncStatusForVariantInventoryItem>.On<BCSyncStatusForVariantInventoryItem.connectorType.IsEqual<@P.AsString>
														  .And<BCSyncStatusForVariantInventoryItem.bindingID.IsEqual<@P.AsInt>
														  .And<BCSyncStatusForVariantInventoryItem.entityType.IsInSequence<InventoryItemsEntityTypeSequence>>
														  .And<BCSyncStatusForVariantInventoryItem.syncID.IsEqual<BCSyncDetailForVariantInventoryItem.syncID>>
														  .And<InventoryItem.isTemplate.IsEqual<False>>>>
							.Where<InventoryItem.itemStatus.IsNotInSequence<ExcludedInventoryItemStatusSequence>
							.And<InventoryItem.inventoryID.IsIn<@P.AsInt>
							.And<Brackets<BCSyncStatusForInventoryItem.syncID.IsNotNull>.Or<BCSyncStatusForVariantInventoryItem.syncID.IsNotNull>>
							//If there is no price but never been synced as well... then no need to pick it up.
							.And<Brackets<ARSalesPrice.recordID.IsNotNull.Or<BCSyncDetail.externID.IsNotNull>>>>>();

			List<object> param = new List<object>() { priceListNoteId, connectorType, bindingId, BCEntitiesAttribute.CustomerPriceClass,
													  connectorType, bindingId,
													  connectorType, bindingId, BCEntitiesAttribute.PriceList,
													  BCEntitiesAttribute.Variant,
													  connectorType,  bindingId, inventoryIds.ToArray() };

			BqlCommand cmd = baseQuery;

			var result = cmd.CreateView(graph).SelectMulti(param.ToArray())
									.Cast<PXResult<InventoryItem>>();

			foreach (var item in result)
			{
				var dto = new PriceListDetailsDTO();
				dto.Init(item.GetItem<InventoryItem>(), item.GetItem<ARSalesPrice>(),
						 item.GetItem<ARPriceClass>(), item.GetItem<BCSyncStatus>(), item.GetItem<BCSyncDetail>(),
						 item.GetItem<BCSyncStatusForInventoryItem>(), item.GetItem<BCSyncStatusForCustomerPriceClass>(),
						 item.GetItem<BCSyncStatusForVariantInventoryItem>(), item.GetItem<BCSyncDetailForVariantInventoryItem>());
				yield return dto;
			}
		}

		/// <summary>
		/// Returns the Ids of all the inventory items that have prices that have changed since the last sync or have never been synced yet.
		/// </summary>
		/// <param name="priceClassNoteId"></param>
		/// <param name="startDate"></param>
		/// <param name="prepareMode"></param>
		/// <param name="graph"></param>
		/// <param name="bindingId"></param>
		/// <param name="connectorType"></param>
		/// <returns></returns>
		protected virtual IEnumerable<int> GetInventoryItemsThatHaveChangedPrices(Guid priceClassNoteId, DateTime? startDate, PrepareMode prepareMode, PXGraph graph, int bindingId, string connectorType)
		{
			var baseQuery = new SelectFrom<InventoryItem>
								.InnerJoin<ARSalesPrice>.On<ARSalesPrice.inventoryID.IsEqual<InventoryItem.inventoryID>
														.And<ARSalesPrice.priceType.IsEqual<PriceTypes.customerPriceClass>>>
								.InnerJoin<ARPriceClass>.On<ARPriceClass.priceClassID.IsEqual<ARSalesPrice.custPriceClassID>>
								//To ensure that the price class has been already synced.
								.InnerJoin<BCSyncStatusForPriceClass>.On<BCSyncStatusForPriceClass.connectorType.IsEqual<@P.AsString>
																	 .And<BCSyncStatusForPriceClass.bindingID.IsEqual<@P.AsInt>>
																	 .And<BCSyncStatusForPriceClass.entityType.IsEqual<@P.AsString>>
																	 .And<BCSyncStatusForPriceClass.externID.IsNotNull>
																	 .And<BCSyncStatusForPriceClass.localID.IsEqual<ARPriceClass.noteID>>
																	 .And<BCSyncStatusForPriceClass.status.IsNotInSequence<InvalidSyncStatusSquence>>>
								.LeftJoin<BCSyncStatus>.On<BCSyncStatus.connectorType.IsEqual<@P.AsString>
													   .And<BCSyncStatus.bindingID.IsEqual<@P.AsInt>>
													   .And<BCSyncStatus.entityType.IsEqual<@P.AsString>>
													   .And<BCSyncStatus.localID.IsEqual<ARPriceClass.noteID>>>
								.Where<InventoryItem.itemStatus.IsNotInSequence<ExcludedInventoryItemStatusSequence>
								.And<ARPriceClass.noteID.IsEqual<@P.AsGuid>>>();

			List<object> param = new List<object>() {connectorType, bindingId, BCEntitiesAttribute.CustomerPriceClass,
													 connectorType, bindingId, BCEntitiesAttribute.PriceList,priceClassNoteId };

			BqlCommand cmd = baseQuery;

			if (prepareMode != PrepareMode.Full && prepareMode != PrepareMode.None)
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
									.Cast<PXResult<InventoryItem>>();

			foreach (var item in result)
			{
				var inventoryItem = item.GetItem<InventoryItem>();
				yield return inventoryItem.InventoryID.Value;
			}
		}

		/// <summary>
		/// Gets the list of all already synced prices via BCSyncDetail (status is Synced already)
		/// Left Outer with the corresponding ARSalesPrice using the LocalID
		/// If there is no ARSalesPrice record then it means it has been deleted
		/// Consider to return the InventoryItem ID.
		/// </summary>
		/// <param name="priceClassNoteId"></param>
		/// <param name="graph"></param>
		/// <param name="bindingId"></param>
		/// <param name="connectorType"></param>
		/// <returns></returns>
		protected virtual IEnumerable<int> GetInventoryItemsThatHaveDeletedPrices(Guid priceClassNoteId, PXGraph graph, int bindingId, string connectorType)
		{
			var baseQuery = new SelectFrom<BCSyncDetail>.InnerJoin<InventoryItem>
														.On<BCSyncDetail.refNoteID.IsEqual<InventoryItem.noteID>>
														.InnerJoin<BCSyncStatus>.On<BCSyncStatus.connectorType.IsEqual<@P.AsString>
																				.And<BCSyncStatus.bindingID.IsEqual<@P.AsInt>>
																				.And<BCSyncStatus.entityType.IsEqual<@P.AsString>>
																				.And<BCSyncStatus.syncID.IsEqual<BCSyncDetail.syncID>>>
														.LeftJoin<ARSalesPrice>.On<ARSalesPrice.noteID.IsEqual<BCSyncDetail.localID>
																			  .And<ARSalesPrice.priceType.IsEqual<PriceTypes.customerPriceClass>>>
														.Where<BCSyncStatus.localID.IsEqual<@P.AsGuid>
															  .And<BCSyncStatus.status.IsEqual<@P.AsString>>
															  .And<ARSalesPrice.recordID.IsNull>
															  .And<InventoryItem.itemStatus.IsNotInSequence<ExcludedInventoryItemStatusSequence>>>();

			List<object> param = new List<object>() { connectorType, bindingId, BCEntitiesAttribute.PriceList,
													  priceClassNoteId, BCSyncStatusAttribute.Synchronized};

			var result = baseQuery.CreateView(graph).SelectMulti(param.ToArray())
										.Cast<PXResult<BCSyncDetail>>();

			foreach (var item in result)
			{
				var inventoryItem = item.GetItem<InventoryItem>();
				yield return inventoryItem.InventoryID.Value;
			}
		}
		/// <summary>
		/// Retrieve one price list if the pricelistnodeid is provided otherwise the list of all lists.
		/// </summary>
		/// <param name="priceListNoteId"></param>
		/// <param name="graph"></param>
		/// <param name="bindingId"></param>
		/// <param name="connectorType"></param>
		/// <returns></returns>
		protected virtual IEnumerable<PriceListDescriptorDTO> RetrievePriceListDescriptors(Guid? priceListNoteId, PXGraph graph, int bindingId, string connectorType)
		{
			var query = new SelectFrom<PX.Objects.AR.ARPriceClass>
							.InnerJoin<BCSyncStatusForPriceClass>
							.On<BCSyncStatusForPriceClass.connectorType.IsEqual<@P.AsString>
							.And<BCSyncStatusForPriceClass.bindingID.IsEqual<@P.AsInt>
							.And<BCSyncStatusForPriceClass.entityType.IsEqual<@P.AsString>>
							.And<BCSyncStatusForPriceClass.localID.IsEqual<ARPriceClass.noteID>>>>
							.LeftJoin<BCSyncStatus>.On<BCSyncStatus.connectorType.IsEqual<@P.AsString>
												   .And<BCSyncStatus.bindingID.IsEqual<@P.AsInt>
												   .And<BCSyncStatus.entityType.IsEqual<@P.AsString>>
												   .And<BCSyncStatus.localID.IsEqual<ARPriceClass.noteID>>>>();
			BqlCommand cmd = query;
			List<object> param = new List<object>() { connectorType, bindingId, BCEntitiesAttribute.CustomerPriceClass,
													  connectorType, bindingId, BCEntitiesAttribute.PriceList };
			if (priceListNoteId.HasValue)
			{
				cmd = cmd.WhereAnd<Where<BCSyncStatusForPriceClass.externID.IsNotNull.And<ARPriceClass.noteID.IsEqual<@P.AsGuid>>>>();
				param.Add(priceListNoteId.Value);
			}

			var result = cmd.CreateView(graph).SelectMulti(param.ToArray())
										.Cast<PXResult<ARPriceClass>>().ToList();
			
			foreach (var item in result)
			{
				var listDescriptor = new PriceListDescriptorDTO()
				{
					CustomerPriceClassStatus = item.GetItem<BCSyncStatusForPriceClass>(),
					Status = item.GetItem<BCSyncStatus>(),
					PriceListID = item.GetItem<ARPriceClass>().PriceClassID,
					PriceClassNoteID = item.GetItem<ARPriceClass>().NoteID,
				};

				var emptyListQuery = new SelectFrom<ARPriceClass>.Where<ARPriceClass.noteID.IsEqual<@P.AsGuid>
														   .And<Exists<SelectFrom<ARSalesPrice>.Where<ARSalesPrice.custPriceClassID.IsEqual<ARPriceClass.priceClassID>>>>>();
				var emptyListResult = emptyListQuery.CreateView(graph).SelectMulti(listDescriptor.PriceClassNoteID).Cast<ARPriceClass>().ToList();

				listDescriptor.IsEmpty = emptyListResult.Count == 0;

				yield return listDescriptor;
			}
		}
	}
}
