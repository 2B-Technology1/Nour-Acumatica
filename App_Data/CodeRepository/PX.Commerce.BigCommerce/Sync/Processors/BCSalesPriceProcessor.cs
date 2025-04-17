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
using System.Collections.Generic;
using System.Linq;
using PX.Commerce.BigCommerce.API.REST;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Commerce.Objects;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.AR;
using PX.Objects.IN;
using System.Threading.Tasks;
using System.Threading;
using PX.Common;
using PX.Objects.Common;
using PX.Api.ContractBased.Models;
using System.Diagnostics;

namespace PX.Commerce.BigCommerce
{
	public class BCSalesPriceEnityBucket : EntityBucketBase, IEntityBucket
	{
		public IMappedEntity Primary => Price;
		public IMappedEntity[] Entities => new IMappedEntity[] { Primary };

		public MappedBaseSalesPrice Price;
	}


	[DebuggerDisplay("Name={InventoryItem.InventoryCD}, NoteID={InventoryItem.NoteID}, TemplateID={InventoryItem.TemplateItemID}, Count={Prices?.Count}")]

	public class InventoryPriceDetails
	{
		public InventoryPriceDetails()
		{
			Prices = new List<SalesPriceDetail>();
		}
		public List<SalesPriceDetail> Prices { get; set; }
		public BCSyncStatus InventoryParentSyncStatus { get; set; }
		public BCSyncStatus PriceSyncStatus { get; set; }
		public InventoryItem InventoryItem { get; set; }
		public string InventoryExternalId { get; set; }
		public DateTime? MaxSyncDateTime { get; set; }
		public bool MustSync { get; set; } = false;
		
	}
	

	[BCProcessor(typeof(BCConnector), BCEntitiesAttribute.SalesPrice, BCCaptions.SalesPrice, 80,
		IsInternal = false,
		Direction = SyncDirection.Export,
		PrimaryDirection = SyncDirection.Export,
		PrimarySystem = PrimarySystem.Local,
		ExternTypes = new Type[] { },
		LocalTypes = new Type[] { },
		AcumaticaPrimaryType = typeof(InventoryItem),
		URL = "products/{0}/edit",
		RequiresOneOf = new string[] { BCEntitiesAttribute.StockItem + "." + BCEntitiesAttribute.NonStockItem + "." + BCEntitiesAttribute.ProductWithVariant }
	)]
	[BCProcessorDetail(EntityType = BCEntitiesAttribute.BulkPrice, EntityName = BCCaptions.SalesPrice, AcumaticaType = typeof(PX.Objects.AR.ARSalesPrice))]
	public class BCSalesPriceProcessor : BCProcessorBulkBase<BCSalesPriceProcessor, BCSalesPriceEnityBucket, MappedBaseSalesPrice>
	{
		protected IChildRestDataProvider<ProductsBulkPricingRules> productBulkPricingRestDataProvider;
		protected IChildUpdateAllRestDataProvider<BulkPricingWithSalesPrice> productBatchBulkRestDataProvider;
		protected IStockRestDataProvider<ProductData> productRestDataProvider;
		protected IRestDataReader<List<Currency>> storCurrencyDataProvider;
		protected IChildUpdateAllRestDataProvider<ProductsVariantData> variantBatchRestDataProvider;

		#region Factories
		[InjectDependency]
		protected IBCRestDataProviderFactory<IChildRestDataProvider<ProductsBulkPricingRules>> productBulkPricingRestDataProviderFactory { get; set; }
		[InjectDependency]
		protected IBCRestDataProviderFactory<IChildUpdateAllRestDataProvider<BulkPricingWithSalesPrice>> productBatchBulkRestDataProviderFactory { get; set; }
		[InjectDependency]
		protected IBCRestDataProviderFactory<IStockRestDataProvider<ProductData>> productRestDataProviderFactory { get; set; }
		[InjectDependency]
		protected IBCRestDataProviderFactory<IRestDataReader<List<Currency>>> storCurrencyDataProviderFactory { get; set; }
		[InjectDependency]
		protected IBCRestDataProviderFactory<IChildUpdateAllRestDataProvider<ProductsVariantData>> variantBatchRestDataProviderFactory { get; set; }

		[InjectDependency]
		protected IBasePriceDataProvider basePriceDataProvider { get; set; }

		#endregion

		#region Constructor
		public override async Task Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			await base.Initialise(iconnector, operation);

			var client = BCConnector.GetRestClient(GetBindingExt<BCBindingBigCommerce>());

			productBulkPricingRestDataProvider = productBulkPricingRestDataProviderFactory.CreateInstance(client);
			productBatchBulkRestDataProvider = productBatchBulkRestDataProviderFactory.CreateInstance(client);
			productRestDataProvider = productRestDataProviderFactory.CreateInstance(client);
			storCurrencyDataProvider = storCurrencyDataProviderFactory.CreateInstance(client);
			variantBatchRestDataProvider = variantBatchRestDataProviderFactory.CreateInstance(client);			
		}
		#endregion
		#region Common
		public override void NavigateLocal(IConnector connector, ISyncStatus status, ISyncDetail detail = null)
		{
			ARSalesPriceMaint extGraph = PXGraph.CreateInstance<ARSalesPriceMaint>();
			ARSalesPriceFilter filter = extGraph.Filter.Current;
			filter.PriceType = PriceTypes.BasePrice;
			InventoryItem inventory = PXSelect<InventoryItem, Where<InventoryItem.noteID, Equal<Required<InventoryItem.noteID>>>>.Select(this, status?.LocalID);
			filter.InventoryID = inventory.InventoryID;

			throw new PXRedirectRequiredException(extGraph, "Navigation") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

		private List<Currency> _currencies = null;
		protected List<Currency> Currencies
		{
			get
			{
				if (_currencies == null)
				{
					_currencies = storCurrencyDataProvider.Get();
				}
				return _currencies;
			}
		}

		#endregion

		#region Import
		public override async Task FetchBucketsImport(CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}
		public override async Task<List<BCSalesPriceEnityBucket>> GetBucketsImport(List<BCSyncStatus> ids, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}
		public override async Task SaveBucketsImport(List<BCSalesPriceEnityBucket> buckets, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Export


		protected virtual IEnumerable<BasePriceDetailsDTO> GetFetchedData(string priceType)
		{
			var binding = GetBinding();
			var connectorType = binding.ConnectorType;
			var bindingId = binding.BindingID;
			return basePriceDataProvider.RetrieveBasePriceListToFetch(Operation.PrepareMode, Operation.StartDate, this, bindingId.Value, connectorType);
		}

		/// <summary>
		/// Create the list of buckets to be exported
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public override Task FetchBucketsExport(CancellationToken cancellationToken = default)
		{
			var updatedSalesList = GetFetchedData(PriceTypes.BasePrice);

			var listOfSalePricePerInventory = GetPriceListGroupedByInvenotryItem(updatedSalesList, false);
			
			if (listOfSalePricePerInventory.Count == 0)
				return Task.CompletedTask;

			List<IMappedEntity> mappedList = new List<IMappedEntity>();
			int countNum = 0;

			foreach (var item in listOfSalePricePerInventory.Keys)
			{
				var inventoryPrice = listOfSalePricePerInventory[item];
				var isVariant = inventoryPrice.InventoryItem.TemplateItemID != null;

				SalesPricesInquiry productsSalesPrice = new SalesPricesInquiry();
				productsSalesPrice.ExternalTemplateID = inventoryPrice.InventoryParentSyncStatus.ExternID.ValueField();
				productsSalesPrice.ExternalInventoryID = inventoryPrice.InventoryExternalId.ValueField();
				productsSalesPrice.SalesPriceDetails = new List<SalesPriceDetail>();
				productsSalesPrice.InventoryID = inventoryPrice.InventoryItem.InventoryID?.ToString();
				productsSalesPrice.Isvariant = isVariant;

				DateTime? maxDateTime = inventoryPrice.MaxSyncDateTime;			

				MappedBaseSalesPrice obj = new MappedBaseSalesPrice(productsSalesPrice, inventoryPrice.InventoryItem.NoteID, maxDateTime, inventoryPrice.InventoryParentSyncStatus.SyncID);
				obj.SyncID = inventoryPrice.PriceSyncStatus?.SyncID;

				if (inventoryPrice.MustSync)
				{
					EnsureStatus(obj, SyncDirection.Export, Conditions.Resync);
				}
				else
					mappedList.Add(obj);

				countNum++;
				if (countNum % BatchFetchCount == 0)
				{
					ProcessMappedListForExport(mappedList);
				}
			}
			
			if (mappedList.Any())
			{
				ProcessMappedListForExport(mappedList);
			}

			return Task.CompletedTask;
		}

		/// <inheritdoc/>
		public override async Task<List<BCSalesPriceEnityBucket>> GetBucketsExport(List<BCSyncStatus> ids, CancellationToken cancellationToken = default)
		{
			if (ids == null || ids.Count() == 0)
				return null;

			BCBinding binding = GetBinding();
			var bindingId = binding.BindingID;
			var connectorType = binding.ConnectorType;

			var baseCurrency = Branch.PK.Find(this, binding.BranchID)?.BaseCuryID.ValueField();
			List<BCSalesPriceEnityBucket> buckets = new List<BCSalesPriceEnityBucket>();			
			
			var listOfUpdatedBasePrice = basePriceDataProvider.RetrieveBasePricesByIds(ids, this, bindingId.Value, connectorType);
			var listOfSalePricePerInventory = GetPriceListGroupedByInvenotryItem(listOfUpdatedBasePrice, true);

			if (listOfSalePricePerInventory.Count == 0) return buckets;

			foreach (var item in listOfSalePricePerInventory.Values)
			{
				bool isVariant = item.InventoryItem.TemplateItemID != null;
				var priceSyncStatus = item.PriceSyncStatus;

				if (priceSyncStatus == null || priceSyncStatus.SyncID == null)
					continue;

				if (isVariant)
				{
					//BigCommerce does not support sales price for breaking quantity greater than one.
					item.Prices.RemoveAll(x => x.BreakQty?.Value > 1);
				}

				DateTime? maxDateTime = null;

				//Check whether we need or not the updateAny/ForceSync.
				bool updatedAny = false;
				bool forceSync = this.Operation.SyncMethod == SyncMode.Force;

				BCSyncStatus parent = item.InventoryParentSyncStatus;
				string variantExternalId = item.InventoryExternalId;

				//Inventory item has not been synced.
				//It has been deleted between prepare and process operations or in the case of force sync
				if (parent == null)
				{
					//Mark the status as for delete
					DeleteStatus(priceSyncStatus, BCSyncOperationAttribute.LocalDelete, String.Format(BigCommerceMessages.BasePriceNotSyncedNoValidInventoryItemNotSynced, item.InventoryItem.InventoryCD));
					continue;
				}

				SalesPricesInquiry productsSalesPrice = new SalesPricesInquiry();
				productsSalesPrice.ExternalTemplateID = parent.ExternID.ValueField();
				productsSalesPrice.ExternalInventoryID = isVariant ? variantExternalId.ValueField() : parent.ExternID.ValueField();
				productsSalesPrice.SalesPriceDetails = new List<SalesPriceDetail>();
				productsSalesPrice.InventoryID = item.InventoryItem.InventoryCD;
				productsSalesPrice.Isvariant = isVariant;

				foreach (SalesPriceDetail basePrice in item.Prices)
				{
					basePrice.Isvariant = isVariant;

					if (basePrice.Delete)
						forceSync = true;

					if (basePrice.Delete || !IsValidBasePrice(basePrice))
						continue;

					basePrice.SyncTime = basePrice.LastModifiedDateTime?.Value;
					maxDateTime = maxDateTime ?? basePrice.SyncTime;
					maxDateTime = maxDateTime > basePrice.SyncTime ? maxDateTime : basePrice.SyncTime;

					productsSalesPrice.SalesPriceDetails.Add(basePrice);
					updatedAny = true;
				}

				MappedBaseSalesPrice obj = new MappedBaseSalesPrice(productsSalesPrice, item.InventoryItem.NoteID, maxDateTime, parent.SyncID);
				obj.AddStatus(priceSyncStatus);

				//get difference
				if (isVariant)
				{
					if (priceSyncStatus.ExternID != null) //meaning sales price was synced for variant before  but is no longer present in ERP/or expired
					{
						if (!productsSalesPrice.SalesPriceDetails.Any(x => x.BreakQty?.Value == 0 || x.BreakQty?.Value == 1))
						{
							forceSync = true;
							obj.Local.Delete = true;
						}
					}
				}
				else
				{
					var breakQtyPrices = productsSalesPrice.SalesPriceDetails.Where(x => x.BreakQty.Value > 1)?.ToList();
					obj.SyncID = priceSyncStatus?.SyncID;
					if (obj.SyncID != null)
						EnsureDetails(obj);
					if (obj.Details?.Where(x => x.ExternID != priceSyncStatus.ExternID)?.Count() > 0 && (breakQtyPrices == null || breakQtyPrices?.Count() == 0)) forceSync = true; // Lines deletd or no longer valid at acumatica 
					var breakqty0Or1 = obj.Details?.FirstOrDefault(x => x.ExternID == priceSyncStatus.ExternID);
					if (priceSyncStatus.ExternID != null && breakqty0Or1 != null)
					{
						if (!productsSalesPrice.SalesPriceDetails.Any(x => x.BreakQty?.Value == 0 || x.BreakQty?.Value == 1))
						{
							forceSync = true; // 0 or 1 breakqty is not in sync
							if (breakQtyPrices == null || breakQtyPrices?.Count() == 0) obj.Local.Delete = true;
						}
						if (productsSalesPrice.SalesPriceDetails.Any(x => (x.BreakQty?.Value == 0 || x.BreakQty?.Value == 1) && x.NoteID.Value != breakqty0Or1.LocalID))
						{ //if price becomes effectve today or on date after last sync need to force sync
							forceSync = true;
						}
					}

					if (obj.Details != null)//lines exist for product but some brekqty line is deleted
					{
						if (breakQtyPrices != null)
						{
							if (!breakQtyPrices.All(c => obj.Details.Where(x => x.ExternID != priceSyncStatus.ExternID).Any(x => x.LocalID == c.NoteID.Value))) forceSync = true;
							if (!obj.Details.Where(x => x.ExternID != priceSyncStatus.ExternID).All(c => breakQtyPrices.Any(x => c.LocalID == x.NoteID.Value))) forceSync = true;
						}
					}
				}

				if (!updatedAny && !forceSync)
				{
					priceSyncStatus.LastOperation = BCSyncOperationAttribute.Skipped;
					//Not sure how to force the skip.
					EnsureStatus(obj, SyncDirection.Export, conditions: Conditions.Default);
					continue;
				}

				EntityStatus status = EnsureStatus(obj, SyncDirection.Export, conditions: forceSync ? Conditions.Resync : Conditions.Default);
				if (Operation.PrepareMode != PrepareMode.Reconciliation && status != EntityStatus.Pending && Operation.SyncMethod != SyncMode.Force) continue;
				buckets.Add(new BCSalesPriceEnityBucket() { Price = obj });
			}
					
			return buckets;
		}

		/// <summary>
		/// Parses the list of prices and group them by Inventory ID. In fact, when we sync to Big Commerce, we sync one price list per inventory item
		/// The price list must contain all applicable prices to the inventory item. Thus, all applicable prices must be sent to Big Commerce even if only
		/// one of them has been updated in the database.
		/// </summary>
		/// <param name="listOfUpdatedBasePrice"></param>
		/// <param name="addPriceDetails"></param>		
		/// <returns></returns>
		protected Dictionary<int, InventoryPriceDetails>  GetPriceListGroupedByInvenotryItem(IEnumerable<BasePriceDetailsDTO> listOfUpdatedBasePrice, bool addPriceDetails)
		{
			BCBinding binding = GetBinding();
			var bindingId = binding.BindingID;
			
			var defaultCurrency = Currencies.Where(x => x.Default == true).FirstOrDefault();
			var baseCurrency = Branch.PK.Find(this, binding.BranchID)?.BaseCuryID.ValueField();
			
			Dictionary<int, InventoryPriceDetails> listOfSalePricePerInventory = new Dictionary<int, InventoryPriceDetails>();

			foreach (var item in listOfUpdatedBasePrice)
			{
				ARSalesPrice salesPrice = item.SalesPrice;
				InventoryItem inventoryItem = item.InventoryItem;
				INSite warehouse = item.Warehouse;
				BCSyncStatus priceSyncStatus = item.PriceSyncStatus;
				BCSyncStatus inventorySyncStatus = item.InventorySyncStatus;
				BCSyncStatus variantInventorySyncStatus = item.VariantInventorySyncStatus;
				BCSyncDetail variantInventorySyncDetail = item.VariantInventorySyncDetail;

				//if the ARSalesPrice is returned without a record ID, it means that the price has been deleted
				//In this case, we must sync the list of prices to Big Commerce to remove the price from Big Commerce.
				//If the list contains 0 prices, we must sync it anyway.
				var deleted = salesPrice == null || salesPrice.RecordID == null; 

				if (Operation.PrepareMode==PrepareMode.None && (inventoryItem == null || inventoryItem.InventoryID == null || priceSyncStatus == null || priceSyncStatus.SyncID == null))
				{
					LogWarning(Operation.LogScope(), BCMessages.LogPricesSkippedItemNotSynce, inventoryItem.InventoryID);
					continue;
				}

				var isVariant = inventoryItem.TemplateItemID != null;
				var parentSyncItem = isVariant ? variantInventorySyncStatus : inventorySyncStatus;

				//Could not find the parent item in the ERP. It has been deleted since the Prepare operation
				if (parentSyncItem?.SyncID == null)
				{
					LogWarning(Operation.LogScope(), BCMessages.LogPricesSkippedItemNotSynce, inventoryItem.InventoryID);
					continue;
				}

				var externalId = isVariant ? variantInventorySyncDetail?.ExternID : inventorySyncStatus?.ExternID;

				if (externalId == null || parentSyncItem == null || parentSyncItem?.SyncID == null || parentSyncItem?.ExternID == null || parentSyncItem.Deleted == true ||
					parentSyncItem?.Status == BCSyncStatusAttribute.Filtered || parentSyncItem?.Status == BCSyncStatusAttribute.Invalid || parentSyncItem?.Status == BCSyncStatusAttribute.Skipped)
				{
					LogWarning(Operation.LogScope(), BCMessages.LogPricesSkippedItemNotSynce, inventoryItem.InventoryID);
					continue; //if Inventory is not found, skip  
				}

				if (!listOfSalePricePerInventory.ContainsKey(inventoryItem.InventoryID.Value))
				{
					listOfSalePricePerInventory.Add(inventoryItem.InventoryID.Value, new InventoryPriceDetails()
					{ InventoryParentSyncStatus = parentSyncItem, PriceSyncStatus = priceSyncStatus, InventoryItem = inventoryItem, InventoryExternalId = externalId });
				}

				if (addPriceDetails)
				{
					if (deleted || (salesPrice != null && (salesPrice.CuryID ?? baseCurrency?.Value) == defaultCurrency.CurrencyCode && salesPrice.TaxCalcMode != PX.Objects.TX.TaxCalculationMode.Gross))
					{
						SalesPriceDetail salePriceDetail = MapPriceDetails(baseCurrency, salesPrice, inventoryItem, warehouse);
						salePriceDetail.Delete = deleted;
						listOfSalePricePerInventory[inventoryItem.InventoryID.Value].Prices.Add(salePriceDetail);
					}
				}
				else
				{
					var dt = listOfSalePricePerInventory[inventoryItem.InventoryID.Value].MaxSyncDateTime;
					if (dt == null || salesPrice.LastModifiedDateTime > dt)
						listOfSalePricePerInventory[inventoryItem.InventoryID.Value].MaxSyncDateTime = salesPrice.LastModifiedDateTime;
				}

				if (deleted)
				{
					//if for that inventory item, we have at least one price that has been deleted, then we must force the synchronization.
					listOfSalePricePerInventory[inventoryItem.InventoryID.Value].MustSync = true;
				}

			}

			return listOfSalePricePerInventory;
		}

		protected virtual SalesPriceDetail MapPriceDetails(StringValue baseCurrency, ARSalesPrice salesPrice, InventoryItem inventoryItem, INSite warehouse)
		{
			if (salesPrice.RecordID == null)
				return new SalesPriceDetail()
				{
					Warehouse = warehouse?.SiteCD?.Trim().ValueField(),
					InventoryID = inventoryItem.InventoryCD.Trim().ValueField(),
					TemplateItemID = inventoryItem.TemplateItemID,
					InventoryNoteID = inventoryItem.NoteID,
					BaseUnit = inventoryItem.SalesUnit,
				};
			
			return new SalesPriceDetail()
			{
				NoteID = salesPrice.NoteID.ValueField(),
				PriceCode = salesPrice.PriceCode.ValueField(),
				UOM = salesPrice.UOM.ValueField(),
				TAX = salesPrice.TaxID.ValueField(),
				CurrencyID = (salesPrice.CuryID ?? baseCurrency?.Value).ValueField(),
				Promotion = salesPrice.IsPromotionalPrice?.ValueField(),
				PriceType = salesPrice.PriceType.ValueField(),
				LastModifiedDateTime = salesPrice.LastModifiedDateTime.ValueField(),
				EffectiveDate = salesPrice.EffectiveDate.ValueField(),
				ExpirationDate = salesPrice.ExpirationDate.ValueField(),
				Description = salesPrice.Description.ValueField(),
				BreakQty = (salesPrice.BreakQty ?? 1).ValueField(),
				Price = salesPrice.SalesPrice.ValueField(),
				Warehouse = warehouse?.SiteCD?.Trim().ValueField(),
				InventoryID = inventoryItem.InventoryCD.Trim().ValueField(),
				TemplateItemID = inventoryItem.TemplateItemID,
				InventoryNoteID = inventoryItem.NoteID,
				BaseUnit = inventoryItem.SalesUnit
			};
		}

		/// <summary>
		/// Validate the base price
		/// Checks for the UOM, Warehouse, Expiration Date and Effective Date
		/// </summary>
		/// <param name="basePrice"></param>
		/// <returns></returns>
		protected bool IsValidBasePrice(SalesPriceDetail basePrice)
		{
			if ((basePrice.BaseUnit != basePrice.UOM?.Value || basePrice.Warehouse?.Value != null) ||
				(basePrice.ExpirationDate?.Value != null && ((DateTime)basePrice.ExpirationDate.Value).Date < PX.Common.PXTimeZoneInfo.Now.Date) ||
				(basePrice.EffectiveDate?.Value != null && ((DateTime)basePrice.EffectiveDate.Value).Date > PX.Common.PXTimeZoneInfo.Now.Date))
			{
				return false;
			}

			return true;
		}
		
		public override async Task MapBucketExport(BCSalesPriceEnityBucket bucket, IMappedEntity existing, CancellationToken cancellationToken = default)
		{
			MappedBaseSalesPrice obj = bucket.Price;

			BulkPricingWithSalesPrice product = obj.Extern = new BulkPricingWithSalesPrice();
			SalesPricesInquiry salesPricesInquiry = obj.Local;
			product.Data = new List<ProductsBulkPricingRules>();
			product.SalePrice = 0;
			product.Id = obj.Local.ExternalInventoryID.Value.ToInt();
			if (salesPricesInquiry.SalesPriceDetails.Any(x => x.BreakQty?.Value == 0) && salesPricesInquiry.SalesPriceDetails.Any(x => x.BreakQty?.Value == 1))
			{
				var basePrice = salesPricesInquiry.SalesPriceDetails.FirstOrDefault(x => x.BreakQty?.Value == 0);
				salesPricesInquiry.SalesPriceDetails.Remove(basePrice);
			}

			if (salesPricesInquiry.Isvariant)
			{
				product.Variant = new ProductsVariantData();
				product.Variant.Id = salesPricesInquiry.ExternalInventoryID?.Value.ToInt();
				product.Variant.ProductId = salesPricesInquiry.ExternalTemplateID?.Value.ToInt();
				product.Variant.OptionValues = null;
				product.Variant.SalePrice = 0;

				foreach (var impl in salesPricesInquiry.SalesPriceDetails)
				{					
					var price = GetHelper<BCHelper>().RoundToStoreSetting(impl.Price.Value);
					product.Variant.SalePrice = price;
				}

				return;
			}

			//non variant stock items prices
			salesPricesInquiry.existingId = productBulkPricingRestDataProvider.GetAll(product.Id.ToString(), cancellationToken).Select(x => x.Id.Value)?.ToList();
			if (salesPricesInquiry.SalesPriceDetails?.Count() > 0)
			{
				var prices = salesPricesInquiry.SalesPriceDetails.OrderBy(x => x.BreakQty?.Value)?.ToList();
				for (int i = 0; i < prices.Count(); i++)
				{
					ProductsBulkPricingRules bulkPricingRules = new ProductsBulkPricingRules();
					var impl = prices[i];
					var price = GetHelper<BCHelper>().RoundToStoreSetting(impl.Price?.Value);
					if (impl.BreakQty?.Value > 1)
					{
						bulkPricingRules.QuantityMax = Convert.ToInt32((i + 1) >= prices.Count() ? 0 : prices[i + 1].BreakQty.Value - 1);
						bulkPricingRules.Type = BCObjectsConstants.Fixed;
						bulkPricingRules.Amount = price;

						bulkPricingRules.QuantityMin = Convert.ToInt32(impl.BreakQty?.Value);
						product.Data.Add(bulkPricingRules);

					}
					else
					{
						product.SalePrice = bulkPricingRules.Amount = price;
					}

				}
			}
		}

		/// <summary>
		/// Validates the bucket right before syncing it.
		/// For now, this method ensures that if there are variants, that the price list contains only breaking quantity 1 or 0
		/// </summary>
		/// <param name="buckets"></param>
		public virtual void ValidateBucketsBeforeSync(List<BCSalesPriceEnityBucket> buckets)
		{
			var variants = buckets.Where(x => x.Price.Local.Isvariant).ToList();
			foreach(var variantPrice in variants)
			{				
				var local = variantPrice.Price.Local;
				if (!local.SalesPriceDetails.Any(x => x.BreakQty?.Value <= 1) && variantPrice.Price.ExternID == null)
				{

					DeleteStatus(variantPrice.Price, BCSyncOperationAttribute.LocalChangedWithoutUpdateExtern, BigCommerceMessages.BasePriceEmptyNotSynced);
					//Remove it from the list. No need to sync it
					buckets.Remove(variantPrice);
				}
			}

			//Check for base prices that do not have any valid base price and have never been synced
			var basePrices = buckets.Where(x => !x.Price.Local.Isvariant && !x.Price.Local.SalesPriceDetails.Any() && x.Price.ExternID == null).ToList();
			foreach(var basePrice in basePrices)
			{
				DeleteStatus(basePrice.Price, BCSyncOperationAttribute.LocalChangedWithoutUpdateExtern, BigCommerceMessages.BasePriceEmptyNotSynced);
				//Remove it from the list. No need to sync it
				buckets.Remove(basePrice);
			}

		}

		public override async Task SaveBucketsExport(List<BCSalesPriceEnityBucket> buckets, CancellationToken cancellationToken = default)
		{

			ValidateBucketsBeforeSync(buckets);

			var bulkPrices = buckets.Where(x => !x.Price.Local.Isvariant).ToList();
			foreach (var price in bulkPrices)
			{
				foreach (var id in price.Price.Local.existingId)
					try
					{
						productBulkPricingRestDataProvider.Delete(id.ToString(), price.Price.Extern.Id.ToString());
					}
					catch { }
			}
			productBatchBulkRestDataProvider.UpdateAll(bulkPrices.Select(x => x.Price.Extern).ToList(), delegate (ItemProcessCallback<BulkPricingWithSalesPrice> callback)
			{
				BCSalesPriceEnityBucket obj = bulkPrices[callback.Index];
				if (callback.IsSuccess)
				{
					BulkPricingWithSalesPrice data = callback.Result;

					obj.Price.ClearDetails();
					obj.Price.ExternID = null;
					if (obj.Price.Local.Delete)
					{
						DeleteStatus(obj.Price, BCSyncOperationAttribute.LocalChangedWithoutUpdateExtern, BigCommerceMessages.BasePriceEmptySynced);
					}
					else
					{
						if (obj.Price.Local.SalesPriceDetails?.Any(x => x.BreakQty?.Value <= 1) == true)
						{
							var localId = obj.Price.Local.SalesPriceDetails?.FirstOrDefault(x => x.BreakQty?.Value <= 1)?.NoteID?.Value;
							if (!obj.Price.Details.Any(x => x.EntityType == BCEntitiesAttribute.BulkPrice && x.LocalID == localId))
								obj.Price.AddDetail(BCEntitiesAttribute.BulkPrice, localId, data.Id.ToString());
						}

						if (data.Data?.Count() > 0)
						{

							foreach (var price in data.Data)
							{
								var localId = obj.Price.Local.SalesPriceDetails?.FirstOrDefault(x => Convert.ToInt32(x.BreakQty?.Value) == price.QuantityMin)?.NoteID?.Value;
								if (!obj.Price.Details.Any(x => x.EntityType == BCEntitiesAttribute.BulkPrice && x.LocalID == localId))
									obj.Price.AddDetail(BCEntitiesAttribute.BulkPrice, localId, price.Id.ToString());
							}
						}
						obj.Price.AddExtern(obj.Price.Extern, new object[] { data.Id }.KeyCombine(), null, data.DateModifiedUT.ToDate());

						if (obj.Price.Extern.Data.Count == 0 && obj.Price.Extern.SalePrice == 0)
						{
							DeleteStatus(obj.Price, BCSyncOperationAttribute.LocalChangedWithoutUpdateExtern, BigCommerceMessages.BasePriceEmptySynced);
						}
						else
							UpdateStatus(obj.Price, BCSyncOperationAttribute.ExternUpdate);
					}
				}
				else
				{
					Log(obj.Price.SyncID, SyncDirection.Export, callback.Error);
					UpdateStatus(obj.Price, BCSyncOperationAttribute.ExternFailed, callback.Error.ToString());
				}
			});

			var variantPrices = buckets.Where(x => x.Price.Local.Isvariant).ToList();
			variantBatchRestDataProvider.UpdateAll(variantPrices.Select(x => x.Price.Extern.Variant).ToList(), delegate (ItemProcessCallback<ProductsVariantData> callbackVariant)
			{
				BCSalesPriceEnityBucket obj = variantPrices[callbackVariant.Index];
				if (callbackVariant.IsSuccess)
				{
					ProductsVariantData data = callbackVariant.Result;
					obj.Price.ExternID = null;
					if (obj.Price.Local.Delete)
						DeleteStatus(obj.Price, BCSyncOperationAttribute.LocalChangedWithoutUpdateExtern, BigCommerceMessages.BasePriceEmptySynced);
					else
					{
						obj.Price.AddExtern(obj.Price.Extern, new object[] { data.ProductId, data.Id }.KeyCombine(), data.Sku, data.CalculateHash());
						UpdateStatus(obj.Price, BCSyncOperationAttribute.ExternUpdate);
					}
				}
				else
				{
					Log(obj.Price.SyncID, SyncDirection.Export, callbackVariant.Error);
					UpdateStatus(obj.Price, BCSyncOperationAttribute.ExternFailed, callbackVariant.Error.ToString());
				}
			});
		}
		#endregion
	}
}
