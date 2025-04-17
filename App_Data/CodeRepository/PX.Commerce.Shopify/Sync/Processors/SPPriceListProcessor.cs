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
using PX.Commerce.Core.API;
using PX.Data;
using PX.Objects.AR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog.Context;
using PX.Commerce.Objects;
using System.Threading;
using PX.Objects.CS;
using PX.Commerce.Shopify.API.GraphQL;
using PX.Data.BQL.Fluent;
using System.Data;
using PX.Common;

namespace PX.Commerce.Shopify
{
	public class SPPriceListEntityBucket : EntityBucketBase, IEntityBucket
	{
		public IMappedEntity Primary => PriceList;
		public IMappedEntity[] Entities => new IMappedEntity[] { Primary };

		public override IMappedEntity[] PreProcessors { get => Companies.ToArray(); }

		public MappedPriceList PriceList;

		public List<PriceListGQL> ExistingPriceList = new List<PriceListGQL>();

		public List<PriceListMutationPackage> PriceListMutationPackages { get; set; } = new List<PriceListMutationPackage>();

		public List<MappedCompany> Companies { get; set; } = new List<MappedCompany>();

		public string SyncMessage { get; set; }

		public EntityStatus? SyncStatus { get; set; }
		/// <summary>
		/// The list of <see cref="CustomerLocation"/>s belonging to the customer.
		/// </summary>
		public List<CustomerLocation> CustomerLocations = new List<CustomerLocation>();
	}

	/// <summary>
	/// Determines whether the PriceList entity is eligible for synchronization and restricts invalid entities.
	/// </summary>
	public class SPPriceListRestrictor : BCBaseRestrictor, IRestrictor
	{
		/// <summary>
		/// Determines whether <paramref name="mapped"/> should be exported.
		/// </summary>
		/// <param name="processor">The processor performing the synchronization.</param>
		/// <param name="mapped">The entity being synchronized.</param>
		/// <param name="mode">Specifies the filtering mode of the operation.</param>
		/// <returns>Returns <see cref="FilterResult"/> of <see cref="FilterStatus.Invalid"/>, <see cref="FilterStatus.Filtered"/>,
		/// or <see cref="FilterStatus.Ignore"/> if the object is restricted. Otherwise null indicating it should not be filtered.</returns>
		public virtual FilterResult RestrictExport(IProcessor processor, IMappedEntity mapped, FilterMode mode)
		{
			return base.Restrict(mapped, delegate (MappedPriceList obj)
			{
				var customerPriceClass = obj.Local;

				//Ignore the PriceList if no price details,
				if (obj.IsNew && customerPriceClass != null && customerPriceClass.CountForDetails == 0)
				{
					return new FilterResult(FilterStatus.Ignore);
				}

				return null;
			});
		}

		/// <summary>
		/// Determines whether <paramref name="mapped"/> should be imported.
		/// </summary>
		/// <param name="processor">The processor performing the synchronization.</param>
		/// <param name="mapped">The entity being synced.</param>
		/// <param name="mode">Specifies the filtering mode of the operation.</param>
		/// <returns>Returns <see cref="FilterResult"/> of <see cref="FilterStatus.Invalid"/>, <see cref="FilterStatus.Filtered"/>,
		/// or <see cref="FilterStatus.Ignore"/> if the object is restricted. Otherwise null indicating it should not be filtered.</returns>
		public FilterResult RestrictImport(IProcessor processor, IMappedEntity mapped, FilterMode mode)
		{
			return null;
		}
	}

	[BCProcessor(typeof(SPConnector), BCEntitiesAttribute.PriceList, BCCaptions.PriceList, 120,
		IsInternal = false,
		Direction = SyncDirection.Export,
		PrimaryDirection = SyncDirection.Export,
		PrimarySystem = PrimarySystem.Local,
		ExternTypes = new Type[] { typeof(PriceListGQL)},
		LocalTypes = new Type[] { typeof(PriceListSalesPrice) },
		AcumaticaPrimaryType = typeof(ARPriceClass),
		AcumaticaFeaturesSet = typeof(FeaturesSet.commerceB2B),
		EntityMappingMode = EntityMappingMode.ExportFiltering,
		URL = "catalogs/",
		RequiresOneOf = new string[] { BCEntitiesAttribute.StockItem + "." + BCEntitiesAttribute.NonStockItem + "." + BCEntitiesAttribute.ProductWithVariant }
	)]
	public class SPPriceListProcessor : BCProcessorSingleBase<SPPriceListProcessor, SPPriceListEntityBucket, MappedPriceList>
	{
		protected IPriceListDataGQLProvider priceListDataGQLProvider;

		[InjectDependency]
		protected IPriceListInternalDataProvider priceListInternalDataProvider { get; set; }

		#region Factories
		[InjectDependency]
		public ISPGraphQLAPIClientFactory shopifyGraphQLClientFactory { get; set; }
		[InjectDependency]
		protected ISPGraphQLDataProviderFactory<PriceListGQLDataProvider> priceListDataGQLProviderFactory { get; set; }

		#endregion

		#region Constructor

		/// <inheritdoc/>
		public override async Task Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			base.Initialise(iconnector, operation);
			var graphQLClient = shopifyGraphQLClientFactory.GetClient(GetBindingExt<BCBindingShopify>());
			priceListDataGQLProvider = priceListDataGQLProviderFactory.GetProvider(graphQLClient);
		}
		#endregion

		#region Common

		/// <inheritdoc/>
		public override void NavigateLocal(IConnector connector, ISyncStatus status, ISyncDetail detail = null)
		{
			ARSalesPriceMaint extGraph = PXGraph.CreateInstance<ARSalesPriceMaint>();
			ARSalesPriceFilter filter = extGraph.Filter.Current;
			filter.PriceType = PriceTypes.CustomerPriceClass;
			ARPriceClass priceClass = PXSelect<ARPriceClass, Where<ARPriceClass.noteID, Equal<Required<ARPriceClass.noteID>>>>.Select(this, status?.LocalID);
			filter.PriceCode = priceClass?.PriceClassID?.Trim();

			throw new PXRedirectRequiredException(extGraph, "Navigation") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

		/// <inheritdoc/>
		public override async Task<MappedPriceList> PullEntity(string externID, string externalInfo, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override async Task<MappedPriceList> PullEntity(Guid? localID, Dictionary<string, object> externalInfo, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		protected virtual async Task<PriceListGQL> GetPriceListByID(string priceListId, CancellationToken cancellationToken = default)
		{
			return await priceListDataGQLProvider.GetPriceListByID(priceListId, true, true, cancellationToken, nameof(PriceListGQL.FixedPricesCount),
						nameof(PriceListGQL.Id), nameof(PriceListGQL.Currency), nameof(PriceListGQL.Name), nameof(PriceListGQL.Catalog), nameof(PriceListGQL.Prices), nameof(PriceListPriceGQL.Variant));
		}

		protected virtual async Task<List<string>> GetStoreCurrencies()
		{
			List<string> currencies = new List<string>();

			var shopInfo = await priceListDataGQLProvider.GetShop();
			currencies.Add(shopInfo.CurrencyCode);
			if (shopInfo?.CurrencySettings?.Nodes?.Count() > 0)
			{
				currencies.AddRange(shopInfo.CurrencySettings.Nodes.Where(x => x.Enabled).Select(x => x.CurrencyCode));
			}

			return currencies;
		}

		#endregion

		#region Import

		/// <inheritdoc/>
		public override async Task FetchBucketsForImport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override async Task<EntityStatus> GetBucketForImport(SPPriceListEntityBucket bucket, BCSyncStatus status, CancellationToken cancellationToken = default)
		{
			var externIds = status.ExternID.Split(',');
			foreach(var externID in externIds)
			{
				var priceList = await GetPriceListByID(externID.KeySplit(1, externID));
				if (priceList != null)
					bucket.ExistingPriceList.Add(priceList);
			}
			bucket.PriceList = bucket.PriceList.Set(new PriceListGQL(), status.ExternID, null, DateTime.Now);
			return EntityStatus.None;
		}

		/// <inheritdoc/>
		public override async Task SaveBucketImport(SPPriceListEntityBucket bucket, IMappedEntity existing, string operation, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region Export

		/// <inheritdoc/>
		public override async Task FetchBucketsForExport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters, CancellationToken cancellationToken = default)
		{
			var existingSyncRecords = GetBCSyncStatusResult(Operation.EntityType, (int?)null).Select(x => x.GetItem<BCSyncStatus>())?.ToList();

			var priceClassRecords = priceListInternalDataProvider.GetPriceClasses(this);
			foreach (var priceClassItem in priceClassRecords)
			{
				//The data in existingRecord may change after EnsureStatus, we should save the Status first to do the comparison later
				var existingRecord = existingSyncRecords?.FirstOrDefault(x => x.LocalID == priceClassItem.NoteID);
				var existingRecordStatus = existingRecord?.Status;
				MappedPriceList mapped = new MappedPriceList(priceClassItem, priceClassItem.NoteID, (new[] { priceClassItem.LastModifiedDateTime, priceClassItem.LastModifiedDateTimeForDetails }).Max());
				EntityStatus status = EnsureStatus(mapped, SyncDirection.Export);

				//Update the graph timestamp first, ensure the timestamp is up-to-date;
				this.SelectTimeStamp();

				if (status == EntityStatus.Deleted) status = EnsureStatus(mapped, SyncDirection.Export, resync: true);

				if (existingRecord != null)
				{
					//For filtered record, if it has synced before, we should change its status to Prepared and set the Catalog status to Archived in Shopify, and then change its status back to Filtered
					if(status == EntityStatus.Filtered && existingRecordStatus.IsIn(BCSyncStatusAttribute.Pending, BCSyncStatusAttribute.Synchronized) && existingRecord.ExternID != null)
					{
						
						EnsureStatus(mapped, SyncDirection.Export, filter: false, resync: true);
					}
					existingSyncRecords.Remove(existingRecord);
				}
			}

			if (existingSyncRecords?.Count > 0)
			{
				//For the records that don't exist in the above fetching result, we should force re-sync them in case to delete the associated exported records in Shopify
				foreach (var existingStatus in existingSyncRecords.Where(x => x.Status.IsIn(BCSyncStatusAttribute.Pending, BCSyncStatusAttribute.Synchronized) && x.ExternID != null))
				{
					MappedPriceList mapped = new MappedPriceList(null, existingStatus.LocalID, existingStatus.LastModifiedDateTime);
					EntityStatus status = EnsureStatus(mapped, SyncDirection.Export, filter: false, resync: true);
				}
			}
		}

		/// <inheritdoc/>
		public override async Task<EntityStatus> GetBucketForExport(SPPriceListEntityBucket bucket, BCSyncStatus status, CancellationToken cancellationToken = default)
		{
			MappedPriceList mapped = null;
			var existingStatus = status.Status;

			var matchedPriceClass = priceListInternalDataProvider.GetPriceClass(this, status.LocalID.Value);
			//For the price class doesn't exist in the filter list, we should delete the exported data in Shopify
			if (matchedPriceClass == null)
			{
				PriceListSalesPrice deletedPriceList = new PriceListSalesPrice() { NoteID = status.LocalID, Deleted = true };
				mapped = bucket.PriceList = new MappedPriceList(deletedPriceList, status.LocalID, status.LastModifiedDateTime);
				return EnsureStatus(mapped, SyncDirection.Export, filter: false, resync: true);
			}

			var salesPriceList = priceListInternalDataProvider.GetSalesPrices(this, status.LocalID.Value);
			matchedPriceClass.SalesPriceDetails = salesPriceList?.ToList();

			mapped = bucket.PriceList = bucket.PriceList.Set(matchedPriceClass, matchedPriceClass.NoteID, (new[] { matchedPriceClass.LastModifiedDateTime, matchedPriceClass.LastModifiedDateTimeForDetails }).Max());
			var ensureStatus = EnsureStatus(mapped, SyncDirection.Export);

			//Update the graph timestamp first, ensure the timestamp is up-to-date;
			this.SelectTimeStamp();

			if (ensureStatus == EntityStatus.Filtered && Operation.SyncMethod != SyncMode.Force)
			{
				//If record is filtered out but it has synced to Shopify, we should update the PriceList status to Archive in Shopify
				if (status?.ExternID != null && (existingStatus == BCSyncStatusAttribute.Pending || existingStatus == BCSyncStatusAttribute.Synchronized))
				{
					matchedPriceClass.Filtered = true;
					return EnsureStatus(mapped, SyncDirection.Export, filter: false, resync: true);
				}
				else
				{
					//If record has been filtered and not in the force sync mode, skip the CustomerLocation part to improve the performance
					return ensureStatus;
				}
			}

			EnsureAssociatedCompanyStatus(bucket, matchedPriceClass.PriceClassID);

			return ensureStatus;
		}

		/// <summary>
		/// Check the Locations that involves in the PriceClass, If the location doesn't sync before, should add to the list and sync it first
		/// </summary>
		/// <param name="bucket"></param>
		/// <param name="priceClassID"></param>
		protected virtual void EnsureAssociatedCompanyStatus(SPPriceListEntityBucket bucket, string priceClassID)
		{
			var customerLocations = priceListInternalDataProvider.GetCustomerLocationsWithPriceClass(this, priceClassID);
			foreach (var location in customerLocations ?? Enumerable.Empty<CustomerLocation>())
			{
				var companyStatus = BCSyncStatus.LocalIDIndex.Find(this, Operation.ConnectorType, Operation.Binding, BCEntitiesAttribute.Company, location.CustomerNoteID);

				//If the parent Company is not in the correct status, skip the locations in the PriceList.
				if (companyStatus != null && (companyStatus.Deleted == true || companyStatus.Status == BCSyncStatusAttribute.Deleted || companyStatus.Status == BCSyncStatusAttribute.Skipped || companyStatus.Status == BCSyncStatusAttribute.Invalid ||
					companyStatus.Status == BCSyncStatusAttribute.Aborted || companyStatus.Status == BCSyncStatusAttribute.Filtered))
				{
					continue;
				}

				BCSyncDetail locationStatus = null;
				if (companyStatus != null && location.NoteID?.Value != null)
				{
					locationStatus = BCSyncDetail.LocalIndex.Find(this, companyStatus.SyncID, BCEntitiesAttribute.CompanyLocation, location.NoteID.Value.Value);
				}

				if (companyStatus == null || string.IsNullOrEmpty(companyStatus.ExternID) || locationStatus?.ExternID == null)
				{
					MappedCompany company = new MappedCompany(new Core.API.Customer() { NoteID = location.CustomerNoteID.ValueField() }, location.CustomerNoteID, location.LastModifiedDateTime.Value);
					EnsureStatus(company, SyncDirection.Export, filter: true, resync: true);
					bucket.Companies.Add(company);
				}

				bucket.CustomerLocations.Add(location);
			}
		}

		/// <inheritdoc/>
		public override async Task MapBucketExport(SPPriceListEntityBucket bucket, IMappedEntity existing, CancellationToken cancellationToken = default)
		{
			var local = (PriceListSalesPrice)bucket.Primary.Local;
			List<string> currencies = await GetStoreCurrencies();

			//If PriceClass has been removed in the export list, or no SalesPrice in the PriceClass, should delete the previous exported data in Shopify
			if (local.Deleted == true)
			{
				HandleDeletedPriceList(bucket);
			}
			else if (local.Filtered == true)
			{
				HandleFilteredPriceList(bucket);
			}
			else
			{
				var associatedLocations = priceListInternalDataProvider.GetBCSyncDetailsForLocations(this, Operation, bucket.CustomerLocations?.Select(x => x.NoteID.Value)?.ToArray()).ToList();
				var notSupportCurrencies = new List<string>();
				var groupByCurrency = local.SalesPriceDetails.GroupBy(x => x.CurrencyID.Value);
				int totalItems = 0, filterTotal = 0;

				foreach (var groupItem in groupByCurrency)
				{
					var currencyCode = groupItem.Key;
					if (currencies.Any(x => string.Equals(x, currencyCode, StringComparison.OrdinalIgnoreCase)) != true)
					{
						notSupportCurrencies.Add(currencyCode);
						continue;
					}

					var priceListName = $"{local.PriceClassID}-{currencyCode}";
					PriceListGQL matchedExistingItem = bucket.ExistingPriceList?.FirstOrDefault(x => x != null && (string.Equals(x.Name, priceListName, StringComparison.OrdinalIgnoreCase) || string.Equals(x.Currency, currencyCode, StringComparison.OrdinalIgnoreCase)));
					//If no existing record, try to use title to search the record in Shopify side
					if(matchedExistingItem == null)
					{
						var similarCatalog = (await priceListDataGQLProvider.GetCatalogs($"title:{priceListName}", true)).FirstOrDefault();
						if(similarCatalog?.PriceList != null)
						{
							matchedExistingItem = await GetPriceListByID(similarCatalog.PriceList.Id);
						}
					}

					var mutationPackage = new PriceListMutationPackage();
					bool isExistingItem = matchedExistingItem != null;

					//For new PriceList creation
					if(!isExistingItem)
					{
						mutationPackage.CatalogCreation = new CatalogCreateInput()
						{
							Context = new CatalogContextInput() { CompanyLocationIds = associatedLocations?.Select(x => x.ExternID?.ConvertIdToGid(ShopifyGraphQLConstants.Objects.CompanyLocation)).ToList() ?? new List<string>() },
							Status = CatalogStatusGQL.Active,
							Title = priceListName
						};

						mutationPackage.PriceListCreation = new PriceListCreateInput()
						{
							Currency = currencyCode,
							Name = priceListName
						};
					}
					else //For PriceList update
					{
						mutationPackage.CatalogUpdate = new CatalogUpdateInput()
						{
							Context = new CatalogContextInput() { CompanyLocationIds = associatedLocations?.Select(x => x.ExternID?.ConvertIdToGid(ShopifyGraphQLConstants.Objects.CompanyLocation)).ToList() ?? new List<string>() },
							Status = matchedExistingItem.Catalog?.Status == CatalogStatusGQL.Draft ? CatalogStatusGQL.Draft : CatalogStatusGQL.Active,
							PublicationId = matchedExistingItem.Catalog.Publication?.Id,
							Title = priceListName
						};
						mutationPackage.CatalogId = matchedExistingItem.Catalog.Id;
						mutationPackage.PriceListId = matchedExistingItem.Id;
						mutationPackage.ExistingPriceListData = matchedExistingItem;
					}

					HandleSalesPrieDetails(mutationPackage, groupItem, currencyCode, ref totalItems, ref filterTotal);

					HandleExistingItemToDelete(mutationPackage);

					//if no SalesPrice items can be exported, skip it.
					if (!isExistingItem && mutationPackage.PriceListPriceInputs.Count == 0)
						continue;

					bucket.PriceListMutationPackages.Add(mutationPackage);
				}

				if(notSupportCurrencies.Count > 0)
				{
					bucket.SyncMessage = string.Format(ShopifyMessages.PriceListNotSupportCurrency, local.PriceClassID, string.Join(",", notSupportCurrencies), GetBinding().BindingName);
					if (notSupportCurrencies.Count == groupByCurrency.Count())
					{
						bucket.SyncStatus = EntityStatus.Invalid;
					}
				}

				//All SalesPrice records have been filtered out
				if (totalItems != 0 && totalItems == filterTotal
					&& bucket.PriceListMutationPackages.SelectMany(x => x.PriceListPriceInputs).Count() == 0 && bucket.PriceListMutationPackages.All(x => string.IsNullOrEmpty(x.PriceListId)) == true)
				{
					bucket.SyncMessage = string.Format(ShopifyMessages.PriceListNoItemExported, local.PriceClassID);
					bucket.SyncStatus = EntityStatus.Invalid;
				}

				//Handle rest existing records
				HandleRestExistingPriceList(bucket, associatedLocations);
			}
		}

		protected virtual void HandleDeletedPriceList(SPPriceListEntityBucket bucket)
		{
			if (bucket.ExistingPriceList?.Count() > 0)
			{
				bucket.PriceListMutationPackages.AddRange(bucket.ExistingPriceList.Select(x => new PriceListMutationPackage()
				{
					PriceListToDelete = true,
					CatalogId = x.Catalog.Id,
					PriceListId = x.Id,
					ExistingPriceListData = x
				}));
			}
		}

		protected virtual void HandleFilteredPriceList(SPPriceListEntityBucket bucket)
		{
			if (bucket.ExistingPriceList?.Count() > 0)
			{
				//If PriceClass has been filtered out, set the Catalog status to Archived in Shopify.
				bucket.PriceListMutationPackages.AddRange(bucket.ExistingPriceList.Select(x => new PriceListMutationPackage()
				{
					PriceListToArchived = true,
					CatalogId = x.Catalog.Id,
					PriceListId = x.Id,
					CatalogUpdate = new CatalogUpdateInput()
					{
						Status = CatalogStatusGQL.Archived
					},
					ExistingPriceListData = x
				}));
			}
		}

		/// <summary>
		/// Handle the rest existing PriceLists that exported before but no more exist in the current sync
		/// </summary>
		/// <param name="bucket"></param>
		/// <param name="associatedLocations"></param>
		protected virtual void HandleRestExistingPriceList(SPPriceListEntityBucket bucket, IEnumerable<BCSyncDetail> associatedLocations)
		{
			if(bucket.ExistingPriceList.Count > 0)
			{
				foreach(var oneExistingPriceList in bucket.ExistingPriceList)
				{
					if(bucket.PriceListMutationPackages.Any(x => x.ExistingPriceListData?.Id == oneExistingPriceList.Id) != true &&
						oneExistingPriceList.FixedPricesCount > 0)
					{
						var mutationPackage = new PriceListMutationPackage()
						{
							CatalogId = oneExistingPriceList.Catalog?.Id,
							PriceListId = oneExistingPriceList.Id,
							ExistingPriceListData = oneExistingPriceList
						};
						mutationPackage.CatalogUpdate = new CatalogUpdateInput()
						{
							Context = new CatalogContextInput() { CompanyLocationIds = associatedLocations?.Select(x => x.ExternID?.ConvertIdToGid(ShopifyGraphQLConstants.Objects.CompanyLocation)).ToList() ?? new List<string>() },
							Status = oneExistingPriceList.Catalog?.Status ?? CatalogStatusGQL.Active,
							PublicationId = oneExistingPriceList.Catalog.Publication?.Id,
							Title = oneExistingPriceList.Catalog?.Title
						};

						HandleExistingItemToDelete(mutationPackage);

						bucket.PriceListMutationPackages.Add(mutationPackage);
					}
				}
			}
		}

		protected virtual void HandleSalesPrieDetails(PriceListMutationPackage mutationPackage, IGrouping<string, SalesPriceDetail> groupedItems, string currencyCode, ref int totalItems, ref int filterTotal)
		{
			var bcSyncDetailsForItems = priceListInternalDataProvider.GetBCSyncDetailsForInventoryItem(this, Operation, groupedItems.Select(x => x?.InventoryNoteID).Distinct().ToArray());

			var groupByItem = groupedItems.GroupBy(x => x.InventoryNoteID.Value);
			foreach (var sameInventoryItems in groupByItem)
			{
				totalItems++;
				SalesPriceDetail salesPriceItem = GetMatchedSalesPriceItem(sameInventoryItems.ToList());

				var existingExportedItem = bcSyncDetailsForItems.FirstOrDefault(x => x.LocalID == salesPriceItem.InventoryNoteID);
				//Should skip this SalesPriceItem if the associated inventory item doesn't export to Shopify.
				//Should skip this SalesPriceItem if it doesn't match the export conditions
				if (existingExportedItem == null || ShouldFilterSalesPriceItem(salesPriceItem))
				{
					filterTotal++;
					continue;
				}

				string externVariantID = existingExportedItem.ExternID.ConvertIdToGid(ShopifyGraphQLConstants.Objects.ProductVariant);

				mutationPackage.PriceListPriceInputs.Add(new PriceListPriceInput()
				{
					Price = new MoneyInput() { Amount = salesPriceItem.Price.Value.Value, CurrencyCode = currencyCode },
					VariantId = externVariantID
				});

				mutationPackage.SalesPriceMappings[externVariantID] = salesPriceItem;
			}
		}

		protected virtual SalesPriceDetail GetMatchedSalesPriceItem(List<SalesPriceDetail> mainSalesPriceItems)
		{
			if (mainSalesPriceItems?.Count == 0) return null;

			SalesPriceDetail salesPriceItem = mainSalesPriceItems.First();
			//If there are multiple items in the list, we should take the one that satisfies the conditions
			if (mainSalesPriceItems.Count() > 1)
			{
				SalesPriceDetail mostMatchedItem = null;

				//if salesPriceItems have both TemplateItem and matrix items, matrix items should be used first
				var withoutTemplateItems = mainSalesPriceItems.Where(x => x.Isvariant != true).ToList();
				if (withoutTemplateItems.Count > 0)
				{
					mostMatchedItem = GetMostMatchedSalesPriceItem(withoutTemplateItems);
				}

				if(mostMatchedItem == null)
					mostMatchedItem = GetMostMatchedSalesPriceItem(mainSalesPriceItems);

				if (mostMatchedItem != null)
				{
					salesPriceItem = mostMatchedItem;
				}
			}

			return salesPriceItem;
		}

		protected virtual SalesPriceDetail GetMostMatchedSalesPriceItem(List<SalesPriceDetail> mainSalesPriceItems)
		{
			if (mainSalesPriceItems?.Count == 0) return null;

			SalesPriceDetail salesPriceItem = mainSalesPriceItems.FirstOrDefault(x => x.BreakQty.Value == 1 && string.Equals(x.UOM.Value, x.SalesUnit, StringComparison.OrdinalIgnoreCase) && x.Warehouse?.Value == null
									&& (x.EffectiveDate?.Value <= DateTime.Now || x.EffectiveDate?.Value == null) && x.Promotion.Value == true && x.ExpirationDate?.Value >= DateTime.Now) ??
								 mainSalesPriceItems.FirstOrDefault(x => x.BreakQty.Value == 0 && string.Equals(x.UOM.Value, x.SalesUnit, StringComparison.OrdinalIgnoreCase) && x.Warehouse?.Value == null
									&& (x.EffectiveDate?.Value <= DateTime.Now || x.EffectiveDate?.Value == null) && x.Promotion.Value == true && x.ExpirationDate?.Value >= DateTime.Now) ??
								 mainSalesPriceItems.FirstOrDefault(x => x.BreakQty.Value == 1 && string.Equals(x.UOM.Value, x.SalesUnit, StringComparison.OrdinalIgnoreCase) && x.Warehouse?.Value == null
									&& (x.EffectiveDate?.Value <= DateTime.Now || x.EffectiveDate?.Value == null) && (x.ExpirationDate?.Value >= DateTime.Now || x.ExpirationDate?.Value == null)) ??
								 mainSalesPriceItems.FirstOrDefault(x => x.BreakQty.Value == 0 && string.Equals(x.UOM.Value, x.SalesUnit, StringComparison.OrdinalIgnoreCase) && x.Warehouse?.Value == null
									&& (x.EffectiveDate?.Value <= DateTime.Now || x.EffectiveDate?.Value == null) && (x.ExpirationDate?.Value >= DateTime.Now || x.ExpirationDate?.Value == null)) ??
								 mainSalesPriceItems.FirstOrDefault(x => (x.BreakQty.Value == 1 || x.BreakQty.Value == 0) && string.Equals(x.UOM.Value, x.SalesUnit, StringComparison.OrdinalIgnoreCase) && x.Warehouse?.Value == null
									&& (x.EffectiveDate?.Value <= DateTime.Now || x.EffectiveDate?.Value == null) && ((x.ExpirationDate?.Value >= DateTime.Now) || x.ExpirationDate?.Value == null));

			return salesPriceItem;
		}

		/// <summary>
		/// Get the list of variantIDs need to delete in Shopify PriceList
		/// </summary>
		/// <param name="mutationPackage"></param>
		protected virtual void HandleExistingItemToDelete(PriceListMutationPackage mutationPackage)
		{
			var existingData = mutationPackage?.ExistingPriceListData;
			if (mutationPackage.PriceListId != null && existingData?.FixedPricesCount > 0)
			{
				var shouldPublishVariantIDs = mutationPackage.PriceListPriceInputs.Select(x => x.VariantId).ToList();
				var shouldRemoveVariantIDs = existingData.PricesList?.Where(x => x != null && x.OriginType == PriceListPriceOriginTypeGQL.Fixed && shouldPublishVariantIDs.Contains(x.Variant?.Id) == false).Select(x => x.Variant.Id).ToList();
				if(shouldRemoveVariantIDs?.Count > 0)
				{
					mutationPackage.VariantsToDelete = shouldRemoveVariantIDs;
				}
			}
		}

		/// <summary>
		/// Validate the SalesPriceDetail. If the condition doesn't match, system will ignore it; otherwise will export to Shopify
		/// </summary>
		/// <param name="salesPriceItem"></param>
		/// <returns></returns>
		protected virtual bool ShouldFilterSalesPriceItem(SalesPriceDetail salesPriceItem)
		{
			//Only Sales UOM can be exported. If the Sales UOM is not used, these records will be ignored if not already present, if it is already present, then would be removed from the Price list on Shopify store side.
			if (salesPriceItem.UOM?.Value == null || !string.Equals(salesPriceItem.UOM.Value, salesPriceItem.SalesUnit, StringComparison.OrdinalIgnoreCase))
				return true;

			//Break qty should be = 0 OR 1, if others, these records will be ignored if not already present, if it is already present, then would be removed from the Price list on Shopify store side. If both 0 and 1 are exist, then 1 is preferable.
			if (salesPriceItem.BreakQty.Value != 0 && salesPriceItem.BreakQty.Value != 1)
				return true;

			//Warehouse should be null.If the warehouse is specified, these records will be ignored if not already present, if it is already present, then would be removed.
			if(salesPriceItem.Warehouse != null)
				return true;

			//If effective date is in a future, then the record will be ignored if not already present, if it is already present, then would be removed.
			if(salesPriceItem.EffectiveDate?.Value != null && salesPriceItem.EffectiveDate?.Value > DateTime.Now.Date)
				return true;

			//If the expired date is specified, the Connector should process and export these records, when the date becomes expired, ERP will delete such records
			if(salesPriceItem.ExpirationDate?.Value != null && salesPriceItem.ExpirationDate?.Value < DateTime.Now.Date)
				return true;

			return false;
		} 

		/// <inheritdoc/>
		public override async Task SaveBucketExport(SPPriceListEntityBucket bucket, IMappedEntity existing, string operation, CancellationToken cancellationToken = default)
		{
			MappedPriceList obj = bucket.PriceList;
			string exportedSummary = string.Empty;
			int deletedPackages = 0; //If all PriceLists have been deleted, should change the sync record to Deleted status.
			int filteredPackages = 0; //If all PriceLists have been filtered, should change the sync record to Filtered status.
			if (bucket.PriceListMutationPackages?.Count > 0)
			{
				foreach(var oneMutationPackage in bucket.PriceListMutationPackages)
				{
					if (oneMutationPackage.PriceListToDelete)
					{
						//We will delete the catalog
						await priceListDataGQLProvider.DeleteCatalog(oneMutationPackage.CatalogId, true, cancellationToken);
						deletedPackages++;

						continue;
					}

					if(oneMutationPackage.PriceListId == null)
					{
						await CreatePriceList(oneMutationPackage, cancellationToken);
					}
					else
					{
						if(oneMutationPackage.PriceListToArchived)
						{
							filteredPackages++;
						}
						await UpdatePriceList(oneMutationPackage, cancellationToken);
					}
				}

				obj.ClearDetails();
				//Handle externalID and sync status/details
				obj.ExternID = GetCombinedExternID(bucket, out string description);
				obj.AddExtern(null, obj.ExternID, description, DateTime.Now);

				exportedSummary = HandleExportedDetails(bucket);
			}

			if(deletedPackages > 0 && deletedPackages == bucket.PriceListMutationPackages.Count)
			{
				DeleteStatus(obj, BCSyncOperationAttribute.LocalDelete, null);
			}
			else if (filteredPackages > 0 && filteredPackages == bucket.PriceListMutationPackages.Count)
			{
				SynchronizeStatus(obj, BCSyncOperationAttribute.LocalUpdate, BCSyncStatusAttribute.Filtered, bucket.SyncMessage);
			}
			else if(bucket.SyncStatus != null && bucket.SyncStatus == EntityStatus.Invalid)
			{
				SetInvalidStatus(obj, bucket.SyncMessage);
			}
			else
				UpdateSyncStatusSucceeded(obj, operation, !string.IsNullOrEmpty(bucket.SyncMessage) ? bucket.SyncMessage: string.Format(ShopifyMessages.PriceListExportResultMsg, exportedSummary));
		}

		protected virtual async Task CreatePriceList(PriceListMutationPackage mutationPackage, CancellationToken cancellationToken = default)
		{
			var publication = await priceListDataGQLProvider.CreatePublication(new PublicationCreateInput(), cancellationToken);
			if (publication != null)
			{
				mutationPackage.CatalogCreation.PublicationId = publication.Id;
			}
			mutationPackage.CatalogData = await priceListDataGQLProvider.CreateCatalog(mutationPackage.CatalogCreation, cancellationToken);
			if (mutationPackage.CatalogData?.Id != null)
			{
				mutationPackage.CatalogId = mutationPackage.CatalogData.Id;
				mutationPackage.PriceListCreation.CatalogId = mutationPackage.CatalogId;
				mutationPackage.PriceListData = await priceListDataGQLProvider.CreatePriceList(mutationPackage.PriceListCreation, cancellationToken);
				mutationPackage.PriceListId = mutationPackage.PriceListData?.Id;
				if (mutationPackage.PriceListId != null)
				{
					if (mutationPackage.PriceListPriceInputs.Count > 0)
					{
						mutationPackage.PriceListPriceDetails = await priceListDataGQLProvider.AddPriceListFixedPrices(mutationPackage.PriceListId, mutationPackage.PriceListPriceInputs, cancellationToken);
					}
				}
			}
		}

		protected virtual async Task UpdatePriceList(PriceListMutationPackage mutationPackage, CancellationToken cancellationToken = default)
		{
			mutationPackage.PriceListData = mutationPackage.ExistingPriceListData;

			await UpdateCatalog(mutationPackage, cancellationToken);

			//Update the price from SalesPrice 
			if (mutationPackage.PriceListPriceInputs.Count > 0)
			{
				var result = await priceListDataGQLProvider.UpdatePriceListFixedPrices(mutationPackage.PriceListId, mutationPackage.PriceListPriceInputs, mutationPackage.VariantsToDelete.ToArray(), cancellationToken);
				mutationPackage.PriceListPriceDetails = result?.PricesAdded;
				mutationPackage.PriceListData = result?.PriceList;
			}
			else if (mutationPackage.VariantsToDelete.Count > 0)
			{
				//if PriceListPriceInputs is empty, we should delete the items that removed from SalesPrice only
				await priceListDataGQLProvider.DeletePriceListFixedPrices(mutationPackage.PriceListId, mutationPackage.VariantsToDelete.ToArray(), cancellationToken);
			}
		}

		protected virtual async Task UpdateCatalog(PriceListMutationPackage mutationPackage, CancellationToken cancellationToken = default)
		{
			//Update the Catalog
			if (mutationPackage.CatalogUpdate != null)
			{
				mutationPackage.CatalogData = await priceListDataGQLProvider.UpdateCatalog(mutationPackage.CatalogId, mutationPackage.CatalogUpdate, cancellationToken);
			}
		}

		protected virtual string GetCombinedExternID(SPPriceListEntityBucket bucket, out string description)
		{
			description = string.Join(",", bucket.PriceListMutationPackages.Where(x => x.PriceListToDelete != true && x.CatalogId != null && x.PriceListId != null).Select(x => x.PriceListData?.Name).ToArray());
			var externIdList = bucket.PriceListMutationPackages.Where(x => x.PriceListToDelete != true && x.CatalogId != null && x.PriceListId != null).Select(x => $"{x.CatalogId.ConvertGidToId()};{x.PriceListId.ConvertGidToId()}").ToList();

			return externIdList.Count > 0 ? string.Join(",", externIdList) : string.Empty;
		}

		/// <summary>
		/// Check all exported price details, add each price detail to BCSyncDetail and return the exportedSummaries.
		/// </summary>
		/// <param name="bucket"></param>
		/// <returns></returns>
		protected virtual string HandleExportedDetails(SPPriceListEntityBucket bucket)
		{
			MappedPriceList obj = bucket.PriceList;
			List<string> exportedSummaries = new List<string>();
			foreach (var onePackage in bucket.PriceListMutationPackages.Where(x => x.PriceListToDelete != true && x.CatalogId != null && x.PriceListId != null))
			{
				int exportedItems = 0;
				if (onePackage.PriceListPriceDetails?.Count() > 0)
				{
					Dictionary<Guid, List<SalesPriceDetail>> matrixItemsCollection = new Dictionary<Guid, List<SalesPriceDetail>>();
					foreach (var oneDetail in onePackage.PriceListPriceDetails)
					{
						if (oneDetail.Variant?.Id != null && onePackage.SalesPriceMappings.TryGetValue(oneDetail.Variant.Id, out var salesPriceDetail))
						{
							if(salesPriceDetail.Isvariant == true)
							{
								List<SalesPriceDetail> matrixItems = new List<SalesPriceDetail>();
								if (matrixItemsCollection.ContainsKey(salesPriceDetail.NoteID.Value.Value))
									matrixItems = matrixItemsCollection[salesPriceDetail.NoteID.Value.Value] ?? new List<SalesPriceDetail>();

								salesPriceDetail.ExternalInventoryID = oneDetail.Variant?.Id.ConvertGidToId().ValueField();
								matrixItems.Add(salesPriceDetail);
								matrixItemsCollection[salesPriceDetail.NoteID.Value.Value] = matrixItems;
							}
							else 
								obj.AddDetail(BCEntitiesAttribute.BulkPrice, salesPriceDetail.NoteID.Value, oneDetail.Variant.Id.ConvertGidToId(), false, salesPriceDetail.InventoryNoteID);
							exportedItems++;
						}
					}

					foreach(var oneTemplateItemDetail in matrixItemsCollection)
					{
						var externIds = oneTemplateItemDetail.Value.Select(x => x.ExternalInventoryID.Value).ToArray();
						obj.AddDetail(BCEntitiesAttribute.BulkPrice, oneTemplateItemDetail.Key, string.Join(";", externIds).TruncateString(128), false, oneTemplateItemDetail.Value.FirstOrDefault()?.TemplateItemNoteID);
					}
				}

				exportedSummaries.Add($"{onePackage.PriceListData?.Name}: {exportedItems}");
			}

			return exportedSummaries?.Any() == true ? string.Join(", ", exportedSummaries) : "0";
		}

		#endregion
	}
}
