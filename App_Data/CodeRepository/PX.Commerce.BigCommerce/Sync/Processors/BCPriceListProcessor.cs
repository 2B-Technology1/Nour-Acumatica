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

using PX.Commerce.BigCommerce.API.REST;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Commerce.Objects;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PX.Commerce.BigCommerce
{
	public class BCPriceListEntityBucket : EntityBucketBase, IEntityBucket
	{
		public IMappedEntity Primary => Price;
		public IMappedEntity[] Entities => new IMappedEntity[] { Primary };

		public MappedPriceList Price;
	}


	public class PriceDetailsForCustomerClass
	{
		public SalesPriceDetail SalesPriceDetail { get; set; }
		public string ExternalInventoryId { get; set; }
		public BCSyncStatus InventoryParentSyncStatus { get; set; }
		public InventoryItem InventoryItem { get; set; }
		public Guid? PriceNoteId { get; set; }
	}

	[DebuggerDisplay("Name={PriceCode}, Count={Prices?.Count}")]

	public class PriceCodeDetails
	{
		public PriceCodeDetails()
		{
			Prices = new List<PriceDetailsForCustomerClass>();
		}

		public string PriceCode { get; set; }
		public List<PriceDetailsForCustomerClass> Prices { get; set; }

		public BCSyncStatus PriceSyncStatus { get; set; }
		public BCSyncStatus CustomerClassSyncStatus { get; set; }
	}


	[BCProcessor(typeof(BCConnector), BCEntitiesAttribute.PriceList, BCCaptions.PriceList, 90,
		IsInternal = false,
		Direction = SyncDirection.Export,
		PrimaryDirection = SyncDirection.Export,
		PrimarySystem = PrimarySystem.Local,
		ExternTypes = new Type[] { },
		LocalTypes = new Type[] { },
		AcumaticaPrimaryType = typeof(ARPriceClass),
		AcumaticaFeaturesSet = typeof(FeaturesSet.commerceB2B),
		URL = "products/pricelists/{0}/edit",
		Requires = new string[] { BCEntitiesAttribute.CustomerPriceClass },
		RequiresOneOf = new string[] { BCEntitiesAttribute.StockItem + "." + BCEntitiesAttribute.NonStockItem + "." + BCEntitiesAttribute.ProductWithVariant }
	)]
	public class BCPriceListProcessor : BCProcessorBulkBase<BCPriceListProcessor, BCPriceListEntityBucket, MappedPriceList>
	{
		protected IPriceListRestDataProvider priceListRestDataProvider;
		protected IPriceListRecordRestDataProvider priceListrecordRestDataProvider;
		protected IRestDataReader<List<Currency>> storCurrencyDataProvider;
		protected IParentRestDataProvider<CustomerGroupData> customerPriceClassRestDataProvider;
		#region Factories
		[InjectDependency]
		protected IBCRestDataProviderFactory<IPriceListRestDataProvider> priceListRestDataProviderFactory { get; set; }
		[InjectDependency]
		protected IBCRestDataProviderFactory<IPriceListRecordRestDataProvider> priceListrecordRestDataProviderFactory { get; set; }
		[InjectDependency]
		protected IBCRestDataProviderFactory<IRestDataReader<List<Currency>>> storeCurrencyDataProviderFactory { get; set; }
		[InjectDependency]
		protected IBCRestDataProviderFactory<IParentRestDataProvider<CustomerGroupData>> customerPriceClassRestDataProviderFactory { get; set; }

		[InjectDependency]
		protected ICustomerClassPriceListDataProvider customerClassPriceListDataProvider { get; set; }

		#endregion

		#region Constructor
		public override async Task Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			await base.Initialise(iconnector, operation);
			var client = BCConnector.GetRestClient(GetBindingExt<BCBindingBigCommerce>());
			priceListRestDataProvider = priceListRestDataProviderFactory.CreateInstance(client);
			priceListrecordRestDataProvider = priceListrecordRestDataProviderFactory.CreateInstance(client);
			storCurrencyDataProvider = storeCurrencyDataProviderFactory.CreateInstance(client);
			customerPriceClassRestDataProvider = customerPriceClassRestDataProviderFactory.CreateInstance(client);
		}
		#endregion

		#region Common
		protected List<Currency> _currencies = null;

		protected List<Currency> Currencies
		{
			get
			{
				if (_currencies == null )
					_currencies = storCurrencyDataProvider.Get();
				return _currencies;
			}
		}
		public override void NavigateLocal(IConnector connector, ISyncStatus status, ISyncDetail detail = null)
		{
			ARSalesPriceMaint extGraph = PXGraph.CreateInstance<ARSalesPriceMaint>();
			ARSalesPriceFilter filter = extGraph.Filter.Current;
			filter.PriceType = PriceTypes.CustomerPriceClass;
			ARPriceClass priceClass = PXSelect<ARPriceClass, Where<ARPriceClass.noteID, Equal<Required<ARPriceClass.noteID>>>>.Select(this, status?.LocalID);
			filter.PriceCode = priceClass?.PriceClassID?.Trim();

			throw new PXRedirectRequiredException(extGraph, "Navigation") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

		#endregion

		#region Import
		public override async Task FetchBucketsImport(CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}
		public override async Task<List<BCPriceListEntityBucket>> GetBucketsImport(List<BCSyncStatus> ids, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}
		public override async Task SaveBucketsImport(List<BCPriceListEntityBucket> buckets, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region Export
		///<inheritdoc/>
		public override Task FetchBucketsExport(CancellationToken cancellationToken = default)
		{
			var binding = GetBinding();
			var connectorType = binding.ConnectorType;
			var bindingId = binding.BindingID;

			var existingPriceLists = customerClassPriceListDataProvider.RetrieveExistingPriceLists(this, bindingId.Value, connectorType); 
			
			foreach(var priceList in existingPriceLists)
			{
				var isEmptyList = priceList.IsEmpty;

				//List is empty in the database but is completely empty.
				//We must create a Status (Pending) so that it gets deleted later.
				if (isEmptyList && !string.IsNullOrEmpty(priceList.Status?.ExternID))
				{
					MappedPriceList obj = new MappedPriceList(null, priceList.PriceClassNoteID, PX.Common.PXTimeZoneInfo.Now.Date, priceList.Status.SyncID);
					UpdateSyncStatus(obj, PX.Common.PXTimeZoneInfo.Now.Date, true);
					continue;
				}

				if (isEmptyList && string.IsNullOrEmpty(priceList.Status?.ExternID))
					continue;

				//Process the price list
				//At this point we know that the list is not empty.

				//Retrieve all the prices that have been updated/ deleted for that particular list.
				var pricesDTO = customerClassPriceListDataProvider.RetrieveUpdatedPricesByListId(priceList.PriceClassNoteID, Operation.PrepareMode, Operation.StartDate, this, bindingId.Value, connectorType).ToList();

				//No price have been updated or deleted for the current Price List.
				//No need to sync
				if (pricesDTO.Count() == 0)
					continue;

				DateTime? maxDateTime = null;
				var hasAtLeastOneValidPrice = false;

				PriceListSalesPrice productsSalesPrice = new PriceListSalesPrice();
				productsSalesPrice.SalesPriceDetails = new List<SalesPriceDetail>();
				productsSalesPrice.PriceClassID = priceList.PriceListID;

				foreach (var dto in pricesDTO)
				{
					var inventoryExternalId = dto.InventoryItem?.IsTemplate.Value == true ? dto.VariantInventorySyncStatus.ExternID : dto.InventorySyncStatus.ExternID;

					var price = GetPriceDetails(dto, inventoryExternalId);

					var deletedPrice = price.SalesPriceDetail.Delete;
					if (!deletedPrice)
					{
						bool isValidPriceLine = IsValidPriceLine(price);
						if (isValidPriceLine) { hasAtLeastOneValidPrice = true; }
						var customerPrice = price.SalesPriceDetail;
						customerPrice.SyncTime = customerPrice.LastModifiedDateTime?.Value;
						maxDateTime = maxDateTime ?? customerPrice.SyncTime;
						maxDateTime = maxDateTime > customerPrice.SyncTime ? maxDateTime : customerPrice.SyncTime;
					}
					else
					{
						hasAtLeastOneValidPrice = true;
						maxDateTime = PX.Common.PXTimeZoneInfo.Now.Date;
					}
				}

				//If the price list has no valid line and have never been synced yet, there is no point syncing it.
				if ((priceList.Status == null || priceList.Status.SyncID == null) && !hasAtLeastOneValidPrice)
					continue;
				
				MappedPriceList obj1 = new MappedPriceList(productsSalesPrice, priceList.PriceClassNoteID, maxDateTime, priceList.Status.SyncID);
				UpdateSyncStatus(obj1, maxDateTime, true);
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Return the price lists to be updated.
		/// For each id passed in the list, we must check the following:
		/// What are the Inventory Items that have one of the conditions:
		/// - One valid sale price that has been added (not synced yet)
		/// - A sales price that has been modified
		/// - A sales price that became effective (based on the effective / expiration dates)
		/// - A sales price that has been deleted.
		/// </summary>
		/// <param name="ids"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public override async Task<List<BCPriceListEntityBucket>> GetBucketsExport(List<BCSyncStatus> ids, CancellationToken cancellationToken = default)
		{

			if (ids == null || ids.Count() == 0)
				return null;

			BCBinding binding = GetBinding();
			var bindingId = binding.BindingID;
			var connectorType = binding.ConnectorType;
			List<BCPriceListEntityBucket> buckets = new List<BCPriceListEntityBucket>();			
			
			//Create the bucket one list at a time.
			foreach (var status in ids)
			{
				var priceListNoteId = status.LocalID;
				if (priceListNoteId == null)
					continue; //maybe raise an exception?!??

				var priceListInfo = customerClassPriceListDataProvider.RetrievePriceListInfoByNoteId(priceListNoteId.Value, this, bindingId.Value, connectorType);

				var parentId = priceListInfo.CustomerPriceClassStatus?.SyncID;
				var parentLocalId = priceListInfo.CustomerPriceClassStatus?.LocalID;
				var customerClassSyncStatus = priceListInfo.CustomerPriceClassStatus;

				if (priceListInfo.IsEmpty && String.IsNullOrEmpty(status.ExternID))
				{					
					continue;
				}

				PriceListSalesPrice productsSalesPrice = new PriceListSalesPrice();
				productsSalesPrice.SalesPriceDetails = new List<SalesPriceDetail>();
				productsSalesPrice.PriceClassID = priceListInfo.PriceListID;

				//If the list is empty but has been synced before, we must create a status (Pending) so that it gets deleted later.
				if (priceListInfo.IsEmpty)
				{
					var dt = base.SyncTime;
					productsSalesPrice.Delete = true;
					MappedPriceList obj1 = new MappedPriceList(productsSalesPrice, parentLocalId, dt, parentId);
					UpdateSyncStatus(obj1, dt, true);
					buckets.Add(new BCPriceListEntityBucket() { Price = obj1 });
					continue;
				}

				var listOfPriceChanges = customerClassPriceListDataProvider.RetrieveUpdatedPricesByListId(priceListNoteId.Value, Operation.PrepareMode, Operation.StartDate, this, bindingId.Value, connectorType).ToList();

				//If Nothing changed in the list, skip it.
				if (listOfPriceChanges == null || listOfPriceChanges.Count() == 0)
					continue;

				DateTime? maxDateTime = null;
				List<PriceListDetailsDTO> deletedPrices = new List<PriceListDetailsDTO>();

				//parse all prices and add them to the price list.
				foreach (var item in listOfPriceChanges)
				{
					ARSalesPrice salesPrice = item.SalesPrice;
					InventoryItem inventoryItem = item.InventoryItem;
					BCSyncStatus inventoryParentSyncStatus = item.InventoryItem.TemplateItemID == null ? item.InventorySyncStatus : item.VariantInventorySyncStatus;
					BCSyncDetail priceSyncDetails = item.PriceSyncDetails;

					if (item.SalesPrice == null || item.SalesPrice.RecordID == null)
					{
						deletedPrices.Add(item);
						continue;
					}

					if (salesPrice != null && !string.IsNullOrEmpty(salesPrice.CustPriceClassID) && salesPrice.TaxCalcMode != PX.Objects.TX.TaxCalculationMode.Gross)
					{
						//Customer class must have been synced
						if (customerClassSyncStatus?.ExternID == null || customerClassSyncStatus?.Deleted == true)
							continue;

						if (inventoryItem.InventoryID == null || inventoryItem.ExportToExternal == false) continue;
						
						var inventoryStatus =  inventoryParentSyncStatus;

						//Inventory must have been synced
						if (inventoryStatus?.ExternID == null || IgnoreDependentEntityStatus(inventoryStatus))
						{
							LogWarning(Operation.LogScope(item.PriceSyncStatus), BCMessages.LogPricesSkippedItemNotSynce, inventoryItem.InventoryCD);
							continue;
						}

						var inventoryExternalId = inventoryParentSyncStatus.ExternID;
						var price = GetPriceDetails(item, inventoryExternalId);
						var isValidPrice = IsValidPriceLine(price);

						//If not valid and never has been synced, then ignore it
						if (!isValidPrice && String.IsNullOrEmpty(priceSyncDetails?.ExternID))
							continue;
						if (isValidPrice)
						{ 
							var customerPrice = price.SalesPriceDetail;
							customerPrice.SyncTime = customerPrice.LastModifiedDateTime?.Value;
							maxDateTime = maxDateTime ?? customerPrice.SyncTime;
							maxDateTime = maxDateTime > customerPrice.SyncTime ? maxDateTime : customerPrice.SyncTime;
						}
						else
						{
							price.SalesPriceDetail.Delete = true;
						}
						productsSalesPrice.SalesPriceDetails.Add(price.SalesPriceDetail);
					}
				}

				//Deleted prices.
				//We need to add deleted prices in the bucket only if there is no other price for the same item.
				foreach(var item in deletedPrices)
				{
					var hasSimilarInventoryItem = productsSalesPrice.SalesPriceDetails.Any(x => x.InventoryID.Value == item.InventoryItem.InventoryCD);
					if (hasSimilarInventoryItem)
						continue;
					var inventoryExternalId = item.InventoryItem?.IsTemplate.Value == true ? item.VariantInventorySyncStatus.ExternID : item.InventorySyncStatus.ExternID;
					var price = GetPriceDetails(item, inventoryExternalId);
					productsSalesPrice.SalesPriceDetails.Add(price.SalesPriceDetail);
				}

				MappedPriceList obj = new MappedPriceList(productsSalesPrice, parentLocalId.Value, maxDateTime, parentId.Value);
				EntityStatus entityStatus = UpdateSyncStatus(obj, maxDateTime, true);
				if (Operation.PrepareMode != PrepareMode.Reconciliation && entityStatus != EntityStatus.Pending && Operation.SyncMethod != SyncMode.Force) continue;

				buckets.Add(new BCPriceListEntityBucket() { Price = obj });
			}

			return buckets;
		}

		/// <summary>
		/// Check the statuses to ignore when syncing.
		/// </summary>
		/// <param name="status"></param>
		/// <returns></returns>
		protected virtual bool IgnoreDependentEntityStatus(BCSyncStatus status)
		{
			return status?.Deleted == true ||
					status?.Status == BCSyncStatusAttribute.Filtered ||
					status?.Status == BCSyncStatusAttribute.Invalid ||
					status?.Status == BCSyncStatusAttribute.Skipped;
		}

		/// <summary>
		/// Checks whether the price is valid
		/// </summary>
		/// <param name="priceLine"></param>
		/// <returns></returns>
		protected virtual bool IsValidPriceLine(PriceDetailsForCustomerClass priceLine)
		{
			var isValid = true;
			var customerPrice = priceLine.SalesPriceDetail;
			var inventoryItem = priceLine.InventoryItem;

			if (!Currencies.Any(x => x.CurrencyCode == customerPrice.CurrencyID?.Value)) isValid = false;

			if (inventoryItem.SalesUnit != customerPrice.UOM?.Value || customerPrice.Warehouse?.Value != null) isValid = false;

			if (customerPrice.ExpirationDate?.Value != null && ((DateTime)customerPrice.ExpirationDate.Value).Date < PX.Common.PXTimeZoneInfo.Now.Date) isValid = false;

			if (customerPrice.EffectiveDate?.Value != null && ((DateTime)customerPrice.EffectiveDate.Value).Date > PX.Common.PXTimeZoneInfo.Now.Date) isValid = false;

			return isValid;
		}
		/// <summary>
		/// Calls the Ensure Status - Redirection needed in order to be able to mock the method for unit testing.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="maxDateTime"></param>
		/// <param name="forceSync"></param>
		/// <returns></returns>
		public virtual EntityStatus UpdateSyncStatus(MappedPriceList obj, DateTime? maxDateTime, bool forceSync)
		{
			EntityStatus status = EnsureStatus(obj, SyncDirection.Export, conditions: forceSync ? Conditions.Resync : Conditions.Default);
			return status;
		}

		/// <summary>
		/// Returns a PriceDetails for price lists object
		/// </summary>
		/// <param name="dto"></param>
		/// <param name="inventoryExternalId"></param>
		/// <returns></returns>
		protected virtual PriceDetailsForCustomerClass GetPriceDetails(PriceListDetailsDTO dto, string inventoryExternalId)
		{
			SalesPriceDetail salesDetails;

			salesDetails = new SalesPriceDetail()
			{
				NoteID = dto.SalesPrice?.NoteID?.ValueField(),
				PriceCode = dto.SalesPrice?.CustPriceClassID?.ValueField(),
				UOM = dto.SalesPrice?.UOM?.ValueField(),
				TAX = dto.SalesPrice?.TaxID?.ValueField(),
				Warehouse = dto.SalesPrice.SiteID != null ? dto.SalesPrice.SiteID.ToString().ValueField() : null,
				CurrencyID = dto.SalesPrice?.CuryID?.ValueField(),
				Promotion = dto.SalesPrice?.IsPromotionalPrice?.ValueField(),
				PriceType = dto.SalesPrice?.PriceType?.ValueField(),
				InventoryID = dto.InventoryItem.InventoryCD.Trim().ValueField(),
				LastModifiedDateTime = dto.SalesPrice?.LastModifiedDateTime?.ValueField(),
				EffectiveDate = dto.SalesPrice?.EffectiveDate?.ValueField(),
				ExpirationDate = dto.SalesPrice?.ExpirationDate?.ValueField(),
				Description = dto.SalesPrice?.Description?.ValueField(),
				BreakQty = (dto.SalesPrice?.BreakQty ?? 1).ValueField(),
				Price = dto.SalesPrice?.SalesPrice?.ValueField(),
				PriceClassNoteID = dto.PriceClass?.NoteID.ValueField(),
				ExternalInventoryID = inventoryExternalId.ValueField(),
				InventoryNoteID = dto.InventoryItem.NoteID,
				TemplateID = dto.InventoryItem.IsTemplate.HasValue && dto.InventoryItem.IsTemplate.Value ?  dto.InventoryItem.InventoryID : null,
				Isvariant = dto.InventoryItem.TemplateItemID.HasValue ? true : false,
				TemplateItemID = dto.InventoryItem.TemplateItemID
			};

			salesDetails.Delete = dto.SalesPrice == null || !dto.SalesPrice.RecordID.HasValue;
			if (salesDetails.Delete && !string.IsNullOrEmpty(dto.PriceSyncDetails?.ExternID))
			{
				var key = new PriceListExternalKey(dto.PriceSyncDetails?.ExternID);
				salesDetails.CurrencyID = key.Currency?.ValueField();
			}

			return new PriceDetailsForCustomerClass()
			{
				SalesPriceDetail = salesDetails,
				InventoryItem = dto.InventoryItem,
				InventoryParentSyncStatus = dto.InventorySyncStatus,
				ExternalInventoryId = inventoryExternalId,
				PriceNoteId = dto.SalesPrice?.NoteID
			};
		}

		/// <summary>
		/// Redirection needed in order to be able to mock the method for unit testing.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		protected virtual BCSyncStatus GetStatusById(int? id)
		{
			return BCSyncStatus.PK.Find(this, id);
		}

		///<inheritdoc/>
		public override async Task MapBucketExport(BCPriceListEntityBucket bucket, IMappedEntity existing, CancellationToken cancellationToken = default)
		{
			MappedPriceList obj = bucket.Price;
			PriceList priceList = obj.Extern = new PriceList();
			PriceListSalesPrice impl = obj.Local;

			BCBinding binding = GetBinding();
			var bindingId = binding.BindingID;
			var connectorType = binding.ConnectorType;

			BCSyncStatus status = GetStatusById(obj.ParentID);				
			if (status == null)
			{
				throw new PXException(BigCommerceMessages.CustomerPriceClassNotSyncronized);
			}
			priceList.Name = impl.PriceClassID;
			priceList.ExtrenalPriceClassID = status.ExternID;
			priceList.priceListRecords = new List<PriceListRecord>();
			var inventories = impl.SalesPriceDetails?.Where(x=>!x.Delete).GroupBy(x => x.InventoryID.Value.Trim()).ToDictionary(x => x.Key, x => x.ToList());
			if (inventories == null)
				return;

			// extract list of variants that are present per currency 
			var variantInventoryCDsByCurrency = impl.SalesPriceDetails.Where(priceDetail => priceDetail.Isvariant)
				.GroupBy(priceDetail => priceDetail.CurrencyID.Value)
				.ToDictionary(group => group.Key, group => group.GroupBy(priceDetail => priceDetail.InventoryID.Value.Trim()).Select(priceDetail => priceDetail.Key).ToList());
			
			foreach (var inventory in inventories)
			{
				var CurrencyBased = inventory.Value?.GroupBy(x => x.CurrencyID.Value).ToDictionary(x => x.Key, x => x.ToList());
				foreach (var currency in CurrencyBased)
				{
					List<string> variantInventoryCDs = new List<string>();
					// get list variants of current currency
					if (variantInventoryCDsByCurrency.ContainsKey(currency.Key))
						variantInventoryCDs = variantInventoryCDsByCurrency[currency.Key];

					var priceListRecord = new PriceListRecord();
					priceListRecord.SKU = inventory.Key;
					var message = string.Format(BCMessages.SalesPriceWithoutBasePrice, inventory.Key, currency.Key);
					if (!currency.Value.Any(x => x.BreakQty?.Value == 0 || x.BreakQty?.Value == 1)) throw new PXException(message);
					var prices = currency.Value.ToList();
					prices = prices.GroupBy(x => x.BreakQty.Value).Select(x => x.Count() > 1 ? x.FirstOrDefault(y => y.Promotion.Value == true) : x.FirstOrDefault()).ToList();

					if (prices.Any(x => x.BreakQty?.Value == 0) && prices.Any(x => x.BreakQty?.Value == 1))
						prices.Remove(prices.FirstOrDefault(x => x.BreakQty?.Value == 0));
					priceListRecord.BulKPricingTier = new List<BulkPricingTier>();

					prices.ForEach(x =>
					{
						var price = GetHelper<BCHelper>().RoundToStoreSetting(x.Price.Value);
						priceListRecord.ProductID = x.ExternalInventoryID?.Value?.ToInt();

						if (x.BreakQty.Value == 1 || x.BreakQty.Value == 0) // if breakqty is null then set it as New sales price
						{
							priceListRecord.Price = 0;
							priceListRecord.SalesPrice = price;
							priceListRecord.Currency = x.CurrencyID?.Value;
							priceListRecord.localPriceInventoryNoteId = x.InventoryNoteID.Value;
						}
						else if (x.BreakQty.Value > 1)
						{
							BulkPricingTier bulKPricingTier = new BulkPricingTier();
							bulKPricingTier.Amount = price;
							bulKPricingTier.PriceCode = x.PriceCode?.Value;
							bulKPricingTier.Type = BCObjectsConstants.Fixed;
							bulKPricingTier.QuantityMinimum = Convert.ToInt32(x.BreakQty?.Value);
							priceListRecord.BulKPricingTier.Add(bulKPricingTier);
						}
					});
					
					var template = prices.FirstOrDefault(x => x.TemplateID != null);
					if (template?.TemplateID != null)//means inventory is template
					{
						var variantsPrices = GetVariantsPricesForTemplateItems(template.TemplateID, template.InventoryNoteID, priceListNoteId: template.PriceClassNoteID.Value,
																			   currencyId: currency.Key,  priceListRecord, bucket, bindingId.Value, connectorType);
						foreach (var variantPrice in variantsPrices)
						{
							priceList.priceListRecords.Add(variantPrice);
						}
					}
					else
					{
						priceList.priceListRecords.Add(priceListRecord);
					}
				}
			}
		}

		/// <summary>
		/// A user can define a price for a template item and therefore, the price is applied to each variant
		/// However, if a price has been defined for at least one of the variants, then we ignore this case and we do not even create a price details for the template.
		/// </summary>
		public virtual IEnumerable<PriceListRecord> GetVariantsPricesForTemplateItems(int? templateId, Guid? parentInventoryItemNoteId, Guid? priceListNoteId, string currencyId,  PriceListRecord templatePriceListRecord, BCPriceListEntityBucket bucket, int? bindingId, string connectorType)
		{
			if (!templateId.HasValue) yield break;
			//in the current price list, we already have one variant for this template item.
			//skip.
			if (bucket.Price.Local.SalesPriceDetails.Any(x => x.TemplateItemID == templateId && x.Isvariant))
				yield break;

			//Retrieve all the variants for that template id that do not have any price that has been synced.
			var variants = customerClassPriceListDataProvider.RetrieveInventoryVariantItemsForTempateID(templateId.Value, priceListNoteId.Value, this, bindingId.Value, connectorType).ToList();

			//if any of the items in the template variants has a price in this price list for that currency, skip the template.
			if (variants.Any(v => v.SalesPrice.RecordID != null && v.SalesPrice.CuryID == currencyId))
				yield break;

			foreach (var variant in variants)
			{
				var inventoryItem = variant.InventoryItem;
				var salesPrice = variant.SalesPrice;

				PriceListRecord childPriceListRecord = templatePriceListRecord.ShallowCopy();
				childPriceListRecord.SKU = inventoryItem.InventoryCD.Trim();
				childPriceListRecord.localPriceInventoryNoteId = parentInventoryItemNoteId.Value;
				yield return childPriceListRecord;
			}
		}

		///<inheritdoc/>
		public override async Task SaveBucketsExport(List<BCPriceListEntityBucket> buckets, CancellationToken cancellationToken = default)
		{
			string priceListId;

			//Get all price lists
			var existingPriceListsInExternalSystem = priceListRestDataProvider.GetAll(cancellationToken: cancellationToken).ToList();
		
			//iterate by pricecode
			foreach (var priceList in buckets)
			{
				try
				{
					var mappedExternObj = priceList.Price.Extern;

					string externalGroupId = mappedExternObj.ExtrenalPriceClassID;

					//check price code in Price list
					PriceList priceListresponse = existingPriceListsInExternalSystem.Where(x => x.Name.Trim().ToLower() == mappedExternObj.Name.Trim().ToLower()).FirstOrDefault();
					priceListId = priceListresponse?.ID?.ToString();

					if (priceListId == null)
					{
						//create price list
						priceListresponse = priceListRestDataProvider.Create(new PriceList() { Name = mappedExternObj.Name });
						priceListId = priceListresponse.ID.ToString();
					}

					if (priceList.Price.Local.Delete)
					{
						priceListRestDataProvider.DeletePriceList(priceListId);
						//remove from bcsyncstatus
						DeleteStatus(priceList.Price.SyncID);
						continue;
					}

					DeleteRecordsForBucket(priceListId, priceList);

					//The price list contains no elements to be synced for now					
					if (mappedExternObj?.priceListRecords == null || mappedExternObj.priceListRecords.Count == 0)
						continue;

					bool errorOcured = false;
					List<string> errorSkus = new List<string>();
					string callBackOriginalErrorMessage = "";
					//call upsert
					priceListrecordRestDataProvider.Upsert(mappedExternObj.priceListRecords, priceListId, delegate (ItemProcessCallback<PriceListRecord> callback)
					{
						if (!callback.IsSuccess)
						{
							errorOcured = true;
							callBackOriginalErrorMessage = callback.Error.ToString();
							if (callback.Error.Message.Contains(".sku") && callback.OriginalBatch != null)
							{

								foreach (Match match in Regex.Matches(callback.Error.Message, @"\d+.sku"))
								{
									string indexStr = match.Value.Split('.')[0];
									bool intConverted = int.TryParse(indexStr, out int index);

									if (intConverted)
									{
										if (index < callback.OriginalBatch.Count && !errorSkus.Contains(callback.OriginalBatch[index].SKU))
											errorSkus.Add(callback.OriginalBatch[index].SKU);
										else
											continue;
									}
									else
										continue;
								}
							}
						}

					});

					if (errorOcured)
					{
						var errorMessage = callBackOriginalErrorMessage;
						if (errorSkus.Count > 0)
						{
							errorMessage = PXMessages.LocalizeFormat(BigCommerceMessages.PriceListProductsNotInBC, priceListresponse.Name);
							var skus = string.Join(", ", errorSkus);
							if (skus.Length>50)
								skus = skus.Substring(0,47) + "...";
							LogWarning(Operation.LogScope(), BigCommerceMessages.PriceListProductsNotInBC + " " + skus, priceListresponse.Name);
						}
						
						Log(priceList?.Primary?.SyncID, SyncDirection.Export, new PXException(errorMessage));
						UpdateStatus(priceList.Price, BCSyncOperationAttribute.ExternFailed, errorMessage);
						continue;
					}

					priceList.Price.ExternID = null;
					//Add extern and updateStatus for each record
					priceList.Price.AddExtern(priceListresponse, priceListId, priceListresponse.Name, priceListresponse.CalculateHash());
					priceList.Price.ClearDetails();

					//Add status details - Each line of the sync, add a line in the status details
					var index = 1;
					foreach (var record in mappedExternObj.priceListRecords)
					{
						var localPrice = GetLocalPriceFromBucket(priceList, record.localPriceInventoryNoteId, record.SalesPrice, null, record.Currency); ;
						var externalKey =  new PriceListExternalKey() { PriceListId = priceListId, Currency = record.Currency, ProductID = record.ProductID?.ToString(), VariantID = record.VariantID};
						if (!priceList.Price.Details.Any(x=>x.LocalID == localPrice.NoteID.Value && x.RefNoteId == localPrice.InventoryNoteID))
							priceList.Price.AddDetail(BCEntitiesAttribute.BulkPrice, localPrice.NoteID.Value, externalKey.ToString(), false, localPrice.InventoryNoteID);
						foreach(var brk in record.BulKPricingTier)
						{
							localPrice = GetLocalPriceFromBucket(priceList, record.localPriceInventoryNoteId, brk.Amount, brk.QuantityMinimum, record.Currency);							
							externalKey = new PriceListExternalKey() { PriceListId = priceListId, Currency = record.Currency, ProductID = record.ProductID?.ToString(), VariantID = record.VariantID, Index = index++ };
							if (!priceList.Price.Details.Any(x => x.LocalID == localPrice.NoteID.Value && x.RefNoteId == localPrice.InventoryNoteID))
								priceList.Price.AddDetail(BCEntitiesAttribute.BulkPrice, localPrice.NoteID.Value, externalKey.ToString(), hide:false, refNoteId: localPrice.InventoryNoteID);
						}
					}
					
					UpdateStatus(priceList.Price, BCSyncOperationAttribute.ExternUpdate);					

					LinkCustomerGroupToPriceList(priceListId, externalGroupId, priceListresponse.Name);
				}					
				catch (Exception ex)
				{
					Log(priceList?.Primary?.SyncID, SyncDirection.Export, ex);
					UpdateStatus(priceList.Price, BCSyncOperationAttribute.ExternFailed, ex.InnerException?.Message ?? ex.Message);
				}
			}
		}

		/// <summary>
		/// Assign a customer group in Big Commerce to the price list that has been synced
		/// </summary>
		/// <param name="priceListId">The Price List Id to be assigned to the Customer Group</param>
		/// <param name="customerGroupId">Customer Group Id in BigCommerce</param>
		/// <param name="priceListExternalName">The name of the Customer Group in Big Commerce: Used for the error message only</param>
		public virtual void LinkCustomerGroupToPriceList(string priceListId, string customerGroupId, string priceListExternalName)
		{
			try
			{
				customerPriceClassRestDataProvider.Update(new CustomerGroupData()
				{
					DiscountRule = new List<DiscountRule>()
								{
								new DiscountRule()
									{
										Type ="price_list",
										PriceListId = priceListId.ToInt()
									}
								}
				}, customerGroupId);
			}
			catch (PX.Commerce.BigCommerce.API.REST.RestException ex)
			{
				if (ex.ResponceStatusCode == System.Net.HttpStatusCode.NotFound.ToString())
				{
					LogError(Operation.LogScope(), ex);
					throw new PXException(BigCommerceMessages.PriceListCustomerGroupNotFound, priceListExternalName);
				}
				throw;
			}
		}

		/// <summary>
		/// Retrieve the corresponding local price list detail for a particular price we synced.
		/// </summary>
		/// <param name="bucket"></param>
		/// <param name="price"></param>
		/// <param name="brqty"></param>
		/// <param name="currency"></param>
		/// <param name="priceInventoryItemNoteId">
		/// NoteId of the inventory item that is bound to the price
		/// If we define prices at the template item level, all variants records will have the note id of the template item
		/// If we define a price at the variant level, this field will contain the noteid of the variant inventory item.
		/// </param>
		/// <returns></returns>
		protected SalesPriceDetail GetLocalPriceFromBucket(BCPriceListEntityBucket bucket, Guid priceInventoryItemNoteId, decimal? price, decimal? brqty, string currency)
		{
			
			var result = bucket.Price.Local.SalesPriceDetails.Where(x => x.InventoryNoteID.Value == priceInventoryItemNoteId
																	&& x.CurrencyID.Value == currency
																	&& ((brqty == null && x.BreakQty.Value <= 1) || (x.BreakQty.Value == brqty))
																	&& GetHelper<BCHelper>().RoundToStoreSetting(x.Price.Value) == price.Value);
			if (result.Count() > 1)
				throw new Exception(BigCommerceMessages.PriceListUnexpectedErrorDuplicates);

			var record = result.FirstOrDefault();

			if (record!= null)
				return record;

			//record is null. This might be the case of a template item.
			//locally we have only the price of the template item and not the sku itself.

			return result.FirstOrDefault();			
		}

		/// <summary>
		/// Delete all records marked with 'Delete=true'
		/// </summary>
		/// <param name="priceListId"></param>
		/// <param name="bucket"></param>
		protected virtual void DeleteRecordsForBucket(string priceListId, BCPriceListEntityBucket bucket, CancellationToken cancellationToken = default)
		{

			//for now, and because the upsert does not return any information about the inserted records,
			//we don't have the variantID which identify the price record in big commerce.
			//VariantID has nothing to do with variants in Acumatica.
			//Hence, and until they fix the issue, we need to get the list of all the prices for the current list
			//in order to be able to retrieve the variantIDs for the prices and be able to delete them.

			if (bucket.Price.Local.SalesPriceDetails.Where(x => x.Delete).Count() == 0)
				return;

			var existingpPriceListRecords = priceListrecordRestDataProvider.GetAllRecords(priceListId, cancellationToken: cancellationToken)?.ToList();

			if (existingpPriceListRecords?.Count == 0)
				return;

			var listToDelete = existingpPriceListRecords.Where(x => bucket.Price.Local.SalesPriceDetails.Exists(local => local.Delete && local.CurrencyID.Value.ToLower() == x.Currency && local.ExternalInventoryID.Value.ToInt() == x.ProductID)).ToList();

			if (listToDelete == null || listToDelete.Count == 0) return;

			foreach (var rec in listToDelete)
			{
				priceListrecordRestDataProvider.DeleteRecords(priceListId, rec.VariantID, rec.Currency);
			}	
		}
		#endregion

	}
	/// <summary>
	/// This class helps to create and read externalid from BCSyncDetail table that correspond to one price for a particular inventory item.
	/// </summary>
	public class PriceListExternalKey
	{

		/// <summary>
		/// 
		/// </summary>
		public PriceListExternalKey() { }


		/// <summary>
		/// Construct the external key from a string.
		/// </summary>
		/// <param name="key"></param>
		/// <exception cref="Exception"></exception>
		public PriceListExternalKey(string key)
		{
			if (String.IsNullOrEmpty(key))
				return;

			string[] keys = key.Trim().Split(';');

			if (keys.Length < 4)
				throw new Exception(BigCommerceMessages.PriceListUnexpectedInvalidInternalKey);

			PriceListId = keys[0]?.Trim();
			ProductID = keys[1]?.Trim();
			VariantID = keys[2]?.Trim();
			Currency = keys[3]?.Trim();

			if (String.IsNullOrEmpty(ProductID) && String.IsNullOrEmpty(Currency))
				throw new Exception(BigCommerceMessages.PriceListUnexpectedInvalidInternalKey);

			if (keys.Length > 4)
			{
				var i = 0;
				var validInt = int.TryParse(keys[4]?.Trim(), out i);
				if (validInt)
					Index = i;
			}
		}

		/// <summary>
		/// External Price List Id
		/// </summary>
		public string PriceListId { get; set; }

		/// <summary>
		/// External Product Id
		/// </summary>
		public string ProductID { get; set; }
		/// <summary>
		///	External Variant id	
		/// </summary>
		public string VariantID { get; set; }
		/// <summary>
		/// Currency code
		/// </summary>
		public string Currency { get; set; }

		/// <summary>
		/// Index of the price in the price list.
		/// When we do have one price and a list of bulk prices (minimum quantiry > 1) the base sales price does not have an index
		/// All the bulk prices have an index.
		/// </summary>
		public int? Index { get; set; } = null;

		/// <summary>
		/// Format the key and returns it as a string.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			if (Index.HasValue)
				return string.Format("{0};{1};{2};{3};{4}", PriceListId, ProductID, VariantID, Currency, Index.Value);

			return string.Format("{0};{1};{2};{3}", PriceListId, ProductID, VariantID, Currency);
		}
	}
}
