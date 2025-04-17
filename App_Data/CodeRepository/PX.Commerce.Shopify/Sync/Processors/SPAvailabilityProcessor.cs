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
using System.Globalization;
using System.Linq;
using System.Text;
using PX.Commerce.Shopify.API.REST;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Commerce.Objects;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.IN;
using PX.Objects.Common;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using System.Threading.Tasks;
using System.Threading;
using PX.Api;
using PX.Common;

namespace PX.Commerce.Shopify
{
	public class SPAvailabilityEntityBucket : EntityBucketBase, IEntityBucket
	{
		public IMappedEntity Primary => Product;
		public IMappedEntity[] Entities => new IMappedEntity[] { Primary };
		public MappedAvailability Product;
		public Dictionary<string, StorageDetailsResult> InventoryMappings = new Dictionary<string, StorageDetailsResult>();
	}

	[BCProcessor(typeof(SPConnector), BCEntitiesAttribute.ProductAvailability, BCCaptions.ProductAvailability, 70,
		IsInternal = false,
		Direction = SyncDirection.Export,
		PrimaryDirection = SyncDirection.Export,
		PrimarySystem = PrimarySystem.Local,
		PrimaryGraph = typeof(PX.Objects.IN.InventorySummaryEnq),
		ExternTypes = new Type[] { },
		LocalTypes = new Type[] { },
		GIScreenID = BCConstants.GenericInquiryAvailability,
		GIResult = typeof(StorageDetails),
		AcumaticaPrimaryType = typeof(InventoryItem),
		URL = "products/{0}",
		Requires = new string[] { },
		RequiresOneOf = new string[] { BCEntitiesAttribute.StockItem + "." + BCEntitiesAttribute.ProductWithVariant }
	)]
	[BCProcessorRealtime(PushSupported = true, HookSupported = false,
		PushSources = new String[] { "BC-PUSH-AvailabilityStockItem", "BC-PUSH-AvailabilityTemplates" }, PushDestination = BCConstants.PushNotificationDestination)]
	public class SPAvailabilityProcessor : AvailabilityProcessorBase<SPAvailabilityProcessor, SPAvailabilityEntityBucket, MappedAvailability>
	{
		protected IInventoryLevelRestDataProvider<InventoryLevelData> levelProvider;
		protected IChildRestDataProvider<ProductVariantData> productVariantDataProvider;
		protected IEnumerable<InventoryLocationData> inventoryLocations;
		protected BCBinding currentBinding;

		#region Factories
		[InjectDependency]
		protected ISPRestDataProviderFactory<IInventoryLevelRestDataProvider<InventoryLevelData>> levelProviderFactory { get; set; }
		[InjectDependency]
		protected ISPRestDataProviderFactory<IChildRestDataProvider<ProductVariantData>> productVariantDataProviderFactory { get; set; }
		#endregion

		#region Constructor
		public override async Task Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			await base.Initialise(iconnector, operation);
			currentBinding = GetBinding();

			var client = SPConnector.GetRestClient(GetBindingExt<BCBindingShopify>());

			levelProvider = levelProviderFactory.CreateInstance(client);
			productVariantDataProvider = productVariantDataProviderFactory.CreateInstance(client);
			inventoryLocations = ConnectorHelper.GetConnector(currentBinding.ConnectorType)?.GetExternalInfo<InventoryLocationData>(BCObjectsConstants.BCInventoryLocation, currentBinding.BindingID)?.Where(x => x.Active == true);

			if (inventoryLocations == null || inventoryLocations.Count() == 0)
			{
				throw new PXException(ShopifyMessages.InventoryLocationNotFound);
			}
			if (inventoryLocations.Count() > 1)
			{
				StoreData store = new StoreRestDataProvider(client).Get();
				if (!string.IsNullOrEmpty(store?.PrimaryLocationId))
				{
					inventoryLocations.ToList().ForEach(x => x.IsDefault = string.Equals(x.Id?.ToString(), store.PrimaryLocationId) ? true : false);
				}
			}
		}
		#endregion

		#region Common
		public override void NavigateLocal(IConnector connector, ISyncStatus status, ISyncDetail detail = null)
		{
			PX.Objects.IN.InventorySummaryEnq extGraph = PXGraph.CreateInstance<PX.Objects.IN.InventorySummaryEnq>();
			InventorySummaryEnqFilter filter = extGraph.Filter.Current;
			InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.noteID, Equal<Required<InventoryItem.noteID>>>>.Select(this, status.LocalID);
			filter.InventoryID = item.InventoryID;

			if (filter.InventoryID != null)
				throw new PXRedirectRequiredException(extGraph, "Navigation") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

		public override async Task<MappedAvailability> PullEntity(Guid? localID, Dictionary<string, object> fields, CancellationToken cancellationToken = default)
		{
			if (localID == null) return null;
			DateTime? timeStamp = fields.Where(f => f.Key.EndsWith(nameof(BCEntity.LastModifiedDateTime), StringComparison.OrdinalIgnoreCase)).Select(f => f.Value?.ToDate()).Max();
			int? parentID = fields.Where(f => f.Key.EndsWith(nameof(BCSyncStatus.SyncID), StringComparison.InvariantCultureIgnoreCase)).Select(f => f.Value).LastOrDefault()?.ToInt();
			localID = fields.Where(f => f.Key.EndsWith("TemplateItem_noteID", StringComparison.InvariantCultureIgnoreCase)).Select(f => f.Value).LastOrDefault()?.ToGuid() ?? localID;
			return new MappedAvailability(new StorageDetailsResult(), localID, timeStamp, parentID);
		}

		public override void UpdateSyncStatusRemoved(BCSyncStatus status, string operation)
		{
			if (status.EntityType == BCEntitiesAttribute.ProductAvailability && operation == BCSyncOperationAttribute.LocalDelete)
				base.UpdateSyncStatusRemoved(status, operation, BCMessages.ProductAvailabilityMissing);
			else
				base.UpdateSyncStatusRemoved(status, operation);
		}
		#endregion

		#region Import
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public override async Task FetchBucketsImport(CancellationToken cancellationToken = default)
		{

		}
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public override async Task<List<SPAvailabilityEntityBucket>> GetBucketsImport(List<BCSyncStatus> ids, CancellationToken cancellationToken = default)
		{
			return null;
		}
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public override async Task MapBucketImport(SPAvailabilityEntityBucket bucket, IMappedEntity existing, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public override async Task SaveBucketsImport(List<SPAvailabilityEntityBucket> buckets, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region Export

		public override async Task FetchBucketsExport(CancellationToken cancellationToken = default)
		{
			DateTime? startDate = Operation.PrepareMode == PrepareMode.Incremental ? GetEntityStats()?.LastIncrementalExportDateTime : Operation.StartDate;
			IEnumerable<StorageDetailsResult> results = Enumerable.Empty<StorageDetailsResult>();
			if (GetEntity(BCEntitiesAttribute.StockItem)?.IsActive == true)
			{
				results = results.Concat(FetchStorageDetails(GetBindingExt<BCBindingExt>(), startDate, Operation.EndDate, FetchAvailabilityBaseCommandForStockItem));
			}
			if (GetEntity(BCEntitiesAttribute.ProductWithVariant)?.IsActive == true)
			{
				results = results.Concat(FetchStorageDetails(GetBindingExt<BCBindingExt>(), startDate, Operation.EndDate, FetchAvailabilityBaseCommandForTemplateItem));
			}

			foreach (StorageDetailsResult lineItem in results)
			{
				DateTime? lastModified = new DateTime?[] { lineItem.SiteLastModifiedDate?.Value, lineItem.InventoryLastModifiedDate?.Value }.Where(d => d != null).Select(d => d.Value).Max();
				MappedAvailability obj = new MappedAvailability(lineItem, lineItem.InventoryNoteID.Value, lastModified, lineItem.ParentSyncId.Value);
				EntityStatus status = EnsureStatus(obj, SyncDirection.Export);
				if (status == EntityStatus.Deleted) status = EnsureStatus(obj, SyncDirection.Export, resync: true);
			}
		}

		public override async Task<List<SPAvailabilityEntityBucket>> GetBucketsExport(List<BCSyncStatus> syncIDs, CancellationToken cancellationToken = default)
		{
			string defaultExtLocation = null;
			List<SPAvailabilityEntityBucket> buckets = new List<SPAvailabilityEntityBucket>();
			BCEntityStats entityStats = GetEntityStats();
			BCBindingExt bindingExt = GetBindingExt<BCBindingExt>();

			var warehouses = new Dictionary<int, INSite>();
			IEnumerable<BCLocations> locationMappings = Enumerable.Empty<BCLocations>();
			Dictionary<int, Dictionary<int, PX.Objects.IN.INLocation>> siteLocationIDs = BCLocationSlot.GetWarehouseLocations(bindingExt.BindingID);
			string[] mappedLocations = null;

			if (bindingExt.WarehouseMode == BCWarehouseModeAttribute.SpecificWarehouse &&
				(PXAccess.FeatureInstalled<FeaturesSet.warehouse>() == true || PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>() == true))
			{
				warehouses = BCLocationSlot.GetWarehouses(bindingExt.BindingID);
				locationMappings = BCLocationSlot.GetExportBCLocations(bindingExt.BindingID);
				if (locationMappings.Any())
				{
					locationMappings.ForEach(locMapping =>
					{
						if (!string.IsNullOrEmpty(locMapping.ExternalLocationID) && inventoryLocations.Any(x => x.Id?.ToString() == locMapping.ExternalLocationID) == false)
						{
							throw new PXException(ShopifyMessages.ExternalLocationNotFound);
						}
					});
					mappedLocations = locationMappings.Select(x => x.ExternalLocationID).Distinct().ToArray();
					defaultExtLocation = mappedLocations.Length > 1 ? null : (mappedLocations.Length == 1 ? mappedLocations[0] : (inventoryLocations.FirstOrDefault(x => x.IsDefault == true) ?? inventoryLocations.First()).Id?.ToString());
				}
				else
					defaultExtLocation = inventoryLocations.Count() > 1 ? (inventoryLocations.FirstOrDefault(x => x.IsDefault == true) ?? inventoryLocations.First()).Id?.ToString() : inventoryLocations.First().Id?.ToString();
			}
			else
			{
				defaultExtLocation = inventoryLocations.Count() > 1 ? (inventoryLocations.FirstOrDefault(x => x.IsDefault == true) ?? inventoryLocations.First()).Id?.ToString() : inventoryLocations.First().Id?.ToString();
			}
			Boolean anyLocation = locationMappings.Any(x => x.LocationID != null);

			IEnumerable<StorageDetailsResult> response = GetStorageDetailsResults(bindingExt, syncIDs);

			if (response == null || response.Any() == false) return buckets;

			List<StorageDetailsResult> results = new List<StorageDetailsResult>();
			foreach (var detailsGroup in response.GroupBy(r => new { InventoryID = r.InventoryCD?.Value }))
			{
				StorageDetailsResult result = null;
				if (defaultExtLocation == null)
				{
					result = new StorageDetailsResult()
					{
						InventoryDescription = detailsGroup.First().InventoryDescription,
						InventoryID = detailsGroup.First().InventoryID,
						InventoryCD = detailsGroup.First().InventoryCD,
						InventoryNoteID = detailsGroup.First().InventoryNoteID,
						Availability = detailsGroup.First().Availability,
						NotAvailMode = detailsGroup.First().NotAvailMode,
						ItemStatus = detailsGroup.First().ItemStatus,
						TemplateItemID = detailsGroup.First().TemplateItemID,
						TemplateItemCD = detailsGroup.First().TemplateItemCD,
						IsTemplate = detailsGroup.First().IsTemplate,
						InventoryLastModifiedDate = detailsGroup.First().InventoryLastModifiedDate,
						ParentSyncId = detailsGroup.First().ParentSyncId,
						ProductExternID = detailsGroup.First().ProductExternID,
						VariantExternID = detailsGroup.First().VariantExternID,
						BaseToSalesUnitConversionRate = detailsGroup.First().BaseToSalesUnitConversionRate
					};
					//If defaultExtLocation is null, that means there are multiple shopify locations in the mapping, we need to recalculate the Inventory by Location
					result.InventoryDetails = detailsGroup.ToList();
				}
				else
					result = detailsGroup.First();
				result.SiteLastModifiedDate = detailsGroup.Where(d => d.SiteLastModifiedDate?.Value != null).Select(d => d.SiteLastModifiedDate.Value).Max().ValueField();
				result.LocationLastModifiedDate = detailsGroup.Where(d => d.LocationLastModifiedDate?.Value != null).Select(d => d.LocationLastModifiedDate.Value).Max().ValueField();
				result.SiteOnHand = detailsGroup.Sum(k => k.SiteOnHand?.Value ?? 0m).ValueField();
				result.SiteAvailable = detailsGroup.Sum(k => k.SiteAvailable?.Value ?? 0m).ValueField();
				result.SiteAvailableforIssue = detailsGroup.Sum(k => k.SiteAvailableforIssue?.Value ?? 0m).ValueField();
				result.SiteAvailableforShipping = detailsGroup.Sum(k => k.SiteAvailableforShipping?.Value ?? 0m).ValueField();
				if (bindingExt.WarehouseMode == BCWarehouseModeAttribute.SpecificWarehouse && !locationMappings.Any())//if warehouse is specific but nothing is configured in table
				{
					result.LocationOnHand = result.LocationAvailable = result.LocationAvailableforIssue = result.LocationAvailableforShipping = 0m.ValueField();
				}
				else
				{
					if (detailsGroup.Any(i => i.SiteID?.Value != null))
					{
						result.LocationOnHand = anyLocation ? detailsGroup.Where(k => warehouses.Count <= 0 || (siteLocationIDs.ContainsKey(k.SiteID?.Value ?? 0) && (siteLocationIDs[k.SiteID?.Value ?? 0].Count == 0 || (k.LocationID?.Value != null && siteLocationIDs[k.SiteID?.Value ?? 0].ContainsKey(k.LocationID.Value.Value))))).Sum(k => k.LocationOnHand?.Value ?? 0m).ValueField() : null;
						result.LocationAvailable = anyLocation ? detailsGroup.Where(k => warehouses.Count <= 0 || (siteLocationIDs.ContainsKey(k.SiteID?.Value ?? 0) && (siteLocationIDs[k.SiteID?.Value ?? 0].Count == 0 || (k.LocationID?.Value != null && siteLocationIDs[k.SiteID?.Value ?? 0].ContainsKey(k.LocationID.Value.Value))))).Sum(k => k.LocationAvailable?.Value ?? 0m).ValueField() : null;
						result.LocationAvailableforIssue = anyLocation ? detailsGroup.Where(k => warehouses.Count <= 0 || (siteLocationIDs.ContainsKey(k.SiteID?.Value ?? 0) && (siteLocationIDs[k.SiteID?.Value ?? 0].Count == 0 || (k.LocationID?.Value != null && siteLocationIDs[k.SiteID?.Value ?? 0].ContainsKey(k.LocationID.Value.Value))))).Sum(k => k.LocationAvailableforIssue?.Value ?? 0m).ValueField() : null;
						result.LocationAvailableforShipping = anyLocation ? detailsGroup.Where(k => warehouses.Count <= 0 || (siteLocationIDs.ContainsKey(k.SiteID?.Value ?? 0) && (siteLocationIDs[k.SiteID?.Value ?? 0].Count == 0 || (k.LocationID?.Value != null && siteLocationIDs[k.SiteID?.Value ?? 0].ContainsKey(k.LocationID.Value.Value))))).Sum(k => k.LocationAvailableforShipping?.Value ?? 0m).ValueField() : null;
					}
					else
						result.LocationOnHand = result.LocationAvailable = result.LocationAvailableforIssue = result.LocationAvailableforShipping = null;
				}
				results.Add(result);
			}

			var allVariants = results.Where(x => x.TemplateItemID?.Value != null);

			foreach (StorageDetailsResult line in results.Where(x => x.TemplateItemID?.Value == null))
			{
				Guid? noteID = line.InventoryNoteID?.Value;
				DateTime? lastModified = null;
				line.VariantDetails = new List<StorageDetailsResult>();


				if (line.IsTemplate?.Value == true)
				{
					line.VariantDetails.AddRange(allVariants.Where(x => x.TemplateItemID?.Value == line.InventoryID.Value));
					if (line.VariantDetails.Count() == 0) continue;
					lastModified = line.VariantDetails.Select(x => new DateTime?[] { x.LocationLastModifiedDate?.Value, x.SiteLastModifiedDate?.Value, x.InventoryLastModifiedDate.Value }.Where(d => d != null).Select(d => d.Value).Max()).Max();
				}
				else
				{
					line.VariantDetails.Add(line);
				}

				lastModified = new DateTime?[] { lastModified, line.LocationLastModifiedDate?.Value, line.SiteLastModifiedDate?.Value, line.InventoryLastModifiedDate?.Value }.Where(d => d != null).Select(d => d.Value).Max();

				SPAvailabilityEntityBucket bucket = new SPAvailabilityEntityBucket();
				MappedAvailability obj = bucket.Product = new MappedAvailability(line, noteID, lastModified, ((int?)line.ParentSyncId.Value));
				EntityStatus status = EnsureStatus(obj, SyncDirection.Export);
				if (line.Availability?.Value == BCItemAvailabilities.DoNotUpdate)
				{
					//If Default Availability is "Set as Available(Track Qty)", sync to Shopify, otherwise skip the update and set Sync status to Deleted.
					continue;
				}

				if (obj.ExternID == null || obj.ExternID != line.ProductExternID.Value)
					obj.ExternID = line.ProductExternID.Value;

				List<ProductVariantData> variantsList = null;
				foreach (var variantItem in line.VariantDetails)
				{
					var inventoryItemID = obj.Details.FirstOrDefault(x => x.EntityType == BCEntitiesAttribute.ProductInventory && x.LocalID == variantItem.InventoryNoteID?.Value)?.ExternID;
					if (string.IsNullOrEmpty(inventoryItemID))
					{
						if (variantsList == null)
							variantsList = productVariantDataProvider.GetAll(line.ProductExternID.Value, new FilterWithFields() { Fields = "id,product_id,sku,inventory_item_id" }, cancellationToken).ToList();
						ProductVariantData variantData = null;
						variantData = variantsList?.FirstOrDefault(x => x.Id.ToString() == variantItem.VariantExternID.Value);
						if (variantData == null && variantsList?.Count == 1)
						{// if there is only one variant even if it does not not match sync detail  allow updating inventory
							variantData = variantsList.FirstOrDefault();
						}
						if (variantData == null) continue;
						inventoryItemID = variantData.InventoryItemId.ToString();
					}

					variantItem.ExternInventoryItemID = inventoryItemID.ToLong();
					bucket.InventoryMappings[$"{variantItem.ProductExternID.Value};{variantItem.VariantExternID.Value}"] = variantItem;
					variantItem.VariantDetails = new List<StorageDetailsResult>();

					if (defaultExtLocation == null && mappedLocations?.Length > 1)
					{
						//Handle multiple locations mapping
						foreach (var locationId in mappedLocations)
						{
							var mappingsWithLocation = locationMappings.Where(x => x.ExternalLocationID == locationId && x.MappingDirection == BCMappingDirectionAttribute.Export).ToList();
							StorageDetailsResult result = new StorageDetailsResult()
							{
								InventoryDescription = variantItem.InventoryDescription,
								InventoryCD = variantItem.InventoryCD,
								InventoryNoteID = variantItem.InventoryNoteID,
								Availability = variantItem.Availability,
								NotAvailMode = variantItem.NotAvailMode,
								ItemStatus = variantItem.ItemStatus,
								ParentSyncId = variantItem.ParentSyncId,
								ProductExternID = variantItem.ProductExternID,
								VariantExternID = variantItem.VariantExternID,
								ExternLocationID = locationId,
								ExternInventoryItemID = inventoryItemID.ToLong(),
								BaseToSalesUnitConversionRate = variantItem.BaseToSalesUnitConversionRate
							};
							var matchedSiteDetails = variantItem.InventoryDetails.Where(x => x.SiteID?.Value != null && mappingsWithLocation.Any(l => l.SiteID == x.SiteID?.Value));
							result.SiteOnHand = matchedSiteDetails.Sum(k => k.SiteOnHand?.Value ?? 0m).ValueField();
							result.SiteAvailable = matchedSiteDetails.Sum(k => k.SiteAvailable?.Value ?? 0m).ValueField();
							result.SiteAvailableforIssue = matchedSiteDetails.Sum(k => k.SiteAvailableforIssue?.Value ?? 0m).ValueField();
							result.SiteAvailableforShipping = matchedSiteDetails.Sum(k => k.SiteAvailableforShipping?.Value ?? 0m).ValueField();
							if (matchedSiteDetails.Any(i => i.LocationID?.Value != null))
							{
								var matchedLocationDetails = matchedSiteDetails.Where(x => mappingsWithLocation.Any(l => l.SiteID == x.SiteID?.Value && (l.LocationID == null || (l.LocationID != null && l.LocationID == x.LocationID?.Value))));
								result.LocationOnHand = matchedLocationDetails.Sum(k => k.LocationOnHand?.Value ?? 0m).ValueField();
								result.LocationAvailable = matchedLocationDetails.Sum(k => k.LocationAvailable?.Value ?? 0m).ValueField();
								result.LocationAvailableforIssue = matchedLocationDetails.Sum(k => k.LocationAvailableforIssue?.Value ?? 0m).ValueField();
								result.LocationAvailableforShipping = matchedLocationDetails.Sum(k => k.LocationAvailableforShipping?.Value ?? 0m).ValueField();
							}
							else
								result.LocationOnHand = result.LocationAvailable = result.LocationAvailableforIssue = result.LocationAvailableforShipping = null;

							variantItem.VariantDetails.Add(result);
						}
					}
					else
					{
						variantItem.ExternLocationID = defaultExtLocation;
						variantItem.VariantDetails.Add(variantItem);
					}
				}

				if (bucket.InventoryMappings.Count > 0)
					buckets.Add(bucket);
			}

			return buckets;
		}

		public override async Task SaveBucketsExport(List<SPAvailabilityEntityBucket> buckets, CancellationToken cancellationToken = default)
		{
			BCBindingExt bindingExt = GetBindingExt<BCBindingExt>();
			foreach (var bucket in buckets)
			{


				//When syncing availabilities for variants, it can happen that some of the variants have been deleted from
				//Shopify. In that case, we must set the availability to 'Synchronized' with a  message.
				//in the case where none of the Availabilities could be synced, we must set the status to 'Deleted'
			
				var partiallySucceeded = false;


				MappedAvailability obj = bucket.Product;
				StorageDetailsResult impl = obj.Local;
				obj.Extern = new InventoryLevelData();
				InventoryLevelData data = null;
				var exceptionsByItem = new Dictionary<string, List<RestException>>();

				var errorMsg = string.Empty;
				var notFoundIds = new List<string>();
				obj.ClearDetails();

				foreach (var item in bucket.InventoryMappings)
				{
					var listOfVariantExceptions = new List<RestException>();
					if (item.Value.Availability.Value == BCItemAvailabilities.DoNotUpdate)
						continue;
					Boolean isItemActive = !(item.Value.ItemStatus?.Value == InventoryItemStatus.Inactive
						|| item.Value.ItemStatus?.Value == InventoryItemStatus.MarkedForDeletion
						|| item.Value.ItemStatus?.Value == InventoryItemStatus.NoSales);

					if (obj.OriginalStatus?.LocalTS == null
						|| obj.OriginalStatus?.LocalTS < GetBinding().LastModifiedDateTime
						|| obj.OriginalStatus?.LocalTS < impl.InventoryLastModifiedDate?.Value)
					{
						try
						{
							UpdateVariantInfo(item.Key.KeySplit(0), item.Key.KeySplit(1), item.Value.Availability?.Value, item.Value.NotAvailMode?.Value);
						}
						catch (RestException ex)
						{
							listOfVariantExceptions.Add(ex);
						}
					}

					if (item.Value.Availability?.Value == BCItemAvailabilities.AvailableTrack)
					{
						foreach (var locationItem in item.Value?.VariantDetails ?? new List<StorageDetailsResult>())
						{
							data = new InventoryLevelData();
							data.InventoryItemId = locationItem.ExternInventoryItemID;
							data.LocationId = locationItem.ExternLocationID.ToLong();
							data.Available = isItemActive ? GetInventoryLevel(bindingExt, locationItem) : 0;
							data.DisconnectIfNecessary = true;
							try
							{
								data = levelProvider.SetInventory(data);
								partiallySucceeded = true;
							}
							catch (RestException ex)
							{
								listOfVariantExceptions.Add(ex);
							}
						}

						if(listOfVariantExceptions.Any())
							exceptionsByItem.Add(item.Key, listOfVariantExceptions);
					}
					obj.AddDetail(BCEntitiesAttribute.ProductInventory, item.Value.InventoryNoteID?.Value, item.Value.ExternInventoryItemID?.ToString());
				}

				//Checks if there is at least one exception was added because a variant was not found.
				if (exceptionsByItem.Any(listOfException => listOfException.Value.Any(oneExcpt => oneExcpt.ResponceStatusCode == System.Net.HttpStatusCode.NotFound.ToString())))
				{					
					bool moreExceptionsThanVariants = bucket.InventoryMappings.Keys.All(x => exceptionsByItem.ContainsKey(x));
					
					string errorMessage;
					if (moreExceptionsThanVariants)
					{
						errorMessage = BCMessages.RecordDeletedInExternalSystem;
					}
					else
					{
						var notFoundVariantsInvCode = bucket.InventoryMappings.Where(x => exceptionsByItem.ContainsKey(x.Key)).Select(x => x.Value.InventoryCD.Value);
						errorMessage = String.Format(BCMessages.RecordPartialDeletedInExternalSystem,String.Join(",", notFoundVariantsInvCode));
					}

					DeleteStatus(bucket.Product, BCSyncOperationAttribute.LocalDelete, errorMessage);
					Operation.Callback?.Invoke(new SyncInfo(bucket?.Primary?.SyncID ?? 0, SyncDirection.Export, SyncResult.Processed));
				}
				//If we don't have any "Not Found" Http response, but we still have exceptions, it should go here.
				else if (exceptionsByItem.Any())
				{
					Exception exception = exceptionsByItem.First().Value.First();
					obj.ClearDetails();
					UpdateStatus(bucket.Product, BCSyncOperationAttribute.ExternFailed, exception.Message);
					Log(bucket?.Primary?.SyncID, SyncDirection.Export, exception);
					Operation.Callback?.Invoke(new SyncInfo(bucket?.Primary?.SyncID ?? 0, SyncDirection.Export, SyncResult.Error, exception));
				}
				else if(notFoundIds.Count>0)
				{
					var syncResult  = partiallySucceeded ? SyncResult.Processed : SyncResult.Deleted;
					var ids = notFoundIds.Count > 0 ? $"{string.Join(",", notFoundIds)}" : string.Empty;
					var message = String.Format(ShopifyMessages.ExternalProductNotFound,ids);
					var operation = partiallySucceeded ? BCSyncOperationAttribute.ExternUpdate : BCSyncOperationAttribute.LocalChangedWithoutUpdateExtern;

					obj.ClearDetails();
					if (!partiallySucceeded)
						DeleteStatus(bucket.Product, operation, null);
					else
					{
						bucket.Product.ExternID = new object[] { obj.ExternID }.KeyCombine();
						bucket.Product.AddExtern(data, obj.ExternID, null, data?.DateModifiedAt.ToDate(false));
						UpdateStatus(bucket.Product, operation);
					}

					Log(bucket?.Primary?.SyncID, SyncDirection.Export, new Exception(message));

					Operation.Callback?.Invoke(new SyncInfo(bucket?.Primary?.SyncID ?? 0, SyncDirection.Export, syncResult, new PXException(message)));
				}
				else if(impl.Availability?.Value != BCItemAvailabilities.AvailableTrack)
				{
					bucket.Product.AddExtern(data, obj.ExternID, null, data?.DateModifiedAt.ToDate(false));
					DeleteStatus(bucket.Product, BCSyncOperationAttribute.LocalDelete, BCMessages.ProductAvailabilityReconfigured);
					Operation.Callback?.Invoke(new SyncInfo(bucket?.Primary?.SyncID ?? 0, SyncDirection.Export, SyncResult.Processed));
				}
				else
				{
					bucket.Product.ExternID = new object[] { obj.ExternID }.KeyCombine();
					bucket.Product.AddExtern(data, obj.ExternID, null, data?.DateModifiedAt.ToDate(false));
					UpdateStatus(bucket.Product, BCSyncOperationAttribute.ExternUpdate);
					Operation.Callback?.Invoke(new SyncInfo(bucket?.Primary?.SyncID ?? 0, SyncDirection.Export, SyncResult.Processed));
				}
			}
		}

		public virtual void UpdateVariantInfo(string productId, string variantId, string availability, string notAvailable)
		{
			if (string.IsNullOrEmpty(productId) || string.IsNullOrEmpty(variantId))
				return;

			var variantData = new SpecifiedVariantData();
			variantData.Id = variantId.ToLong();
			variantData.ProductId = productId.ToLong();
			variantData.InventoryPolicy = notAvailable == BCItemNotAvailModes.DisableItem ? InventoryPolicy.Deny : (notAvailable == BCItemNotAvailModes.PreOrderItem ? InventoryPolicy.Continue : (InventoryPolicy?)null);
			variantData.InventoryManagement = availability == BCItemAvailabilities.AvailableTrack ? ShopifyConstants.InventoryManagement_Shopify : null;
			productVariantDataProvider.Update(variantData, productId, variantId);
		}

		public virtual string UpdateVariantInfo(ProductVariantData variant, string availability)
		{
			string errorMsg = string.Empty;
			var variantData = new ProductVariantData();
			variantData.Id = variant.Id;
			variantData.ProductId = variant.ProductId;
			variantData.Price = variant.Price;
			variantData.OriginalPrice = variant.OriginalPrice;

			if (availability == BCItemAvailabilities.AvailableTrack)
			{
				variantData.InventoryManagement = ShopifyConstants.InventoryManagement_Shopify;
			}
			else
			{
				variantData.InventoryManagement = null;
			}
			try
			{
				productVariantDataProvider.Update(variantData, variantData.ProductId.ToString(), variantData.Id.ToString());
			}
			catch (Exception ex)
			{
				errorMsg = ex.InnerException?.Message ?? ex.Message;
			}
			return errorMsg;
		}
		#endregion
	}
}
