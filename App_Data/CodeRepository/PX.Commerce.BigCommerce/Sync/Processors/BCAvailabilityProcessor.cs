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
using PX.Objects.IN;
using PX.Objects.Common;
using System.Threading.Tasks;
using System.Threading;
using System.Net;

namespace PX.Commerce.BigCommerce
{
	public class BCAvailabilityEntityBucket : EntityBucketBase, IEntityBucket
	{
		public IMappedEntity Primary => Product;
		public IMappedEntity[] Entities => new IMappedEntity[] { Primary };

		public MappedAvailability Product;
	}

	[BCProcessor(typeof(BCConnector), BCEntitiesAttribute.ProductAvailability, BCCaptions.ProductAvailability, 110,
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
		URL = "products/{0}/edit",
		Requires = new string[] { },
		RequiresOneOf = new string[] { BCEntitiesAttribute.StockItem + "." + BCEntitiesAttribute.ProductWithVariant }
	)]
	[BCProcessorRealtime(PushSupported = true, HookSupported = false,
		PushSources = new String[] { "BC-PUSH-AvailabilityStockItem", "BC-PUSH-AvailabilityTemplates" }, PushDestination = BCConstants.PushNotificationDestination)]
	public class BCAvailabilityProcessor : AvailabilityProcessorBase<BCAvailabilityProcessor, BCAvailabilityEntityBucket, MappedAvailability>
	{
		protected IStockRestDataProvider<ProductData> productDataProvider;
		protected IChildUpdateAllRestDataProvider<ProductsVariantData> variantBatchRestDataProvider;
		protected IChildRestDataProvider<ProductsVariantData> productVariantDataProvider;

		#region Factories
		[InjectDependency]
		protected IBCRestDataProviderFactory<IStockRestDataProvider<ProductData>> productDataProviderFactory { get; set; }
		[InjectDependency]
		protected IBCRestDataProviderFactory<IChildUpdateAllRestDataProvider<ProductsVariantData>> variantBatchRestDataProviderFactory { get; set; }
		[InjectDependency]
		protected IBCRestDataProviderFactory<IChildRestDataProvider<ProductsVariantData>> productVariantDataProviderFactory { get; set; }
		#endregion

		#region Constructor
		public override async Task Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			await base.Initialise(iconnector, operation);

			BCRestClient client = BCConnector.GetRestClient(GetBindingExt<BCBindingBigCommerce>());
			productDataProvider = productDataProviderFactory.CreateInstance(client);
			variantBatchRestDataProvider = variantBatchRestDataProviderFactory.CreateInstance(client);
			productVariantDataProvider = productVariantDataProviderFactory.CreateInstance(client);
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
		public override async Task<List<BCAvailabilityEntityBucket>> GetBucketsImport(List<BCSyncStatus> ids, CancellationToken cancellationToken = default)
		{
			return null;
		}
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public override async Task MapBucketImport(BCAvailabilityEntityBucket bucket, IMappedEntity existing, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public override async Task SaveBucketsImport(List<BCAvailabilityEntityBucket> buckets, CancellationToken cancellationToken = default)
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

		public override async Task<List<BCAvailabilityEntityBucket>> GetBucketsExport(List<BCSyncStatus> syncIDs, CancellationToken cancellationToken = default)
		{
			BCEntityStats entityStats = GetEntityStats();
			BCBinding binding = GetBinding();
			BCBindingExt bindingExt = GetBindingExt<BCBindingExt>();
			List<BCAvailabilityEntityBucket> buckets = new List<BCAvailabilityEntityBucket>();

			var warehouses = new Dictionary<int, INSite>();
			IEnumerable<BCLocations> locationMappings = BCLocationSlot.GetExportBCLocations(bindingExt.BindingID);
			Dictionary<int, Dictionary<int, PX.Objects.IN.INLocation>> siteLocationIDs = BCLocationSlot.GetWarehouseLocations(bindingExt.BindingID);

			if (bindingExt.WarehouseMode == BCWarehouseModeAttribute.SpecificWarehouse)
			{
				warehouses = BCLocationSlot.GetWarehouses(bindingExt.BindingID);
			}
			Boolean anyLocation = locationMappings.Any(x => x.LocationID != null);

			IEnumerable<StorageDetailsResult> response = GetStorageDetailsResults(bindingExt, syncIDs);

			if (response == null || response.Any() == false) return buckets;

			List<StorageDetailsResult> results = new List<StorageDetailsResult>();
			foreach (var detailsGroup in response.GroupBy(r => new { InventoryID = r.InventoryCD?.Value, /*SiteID = r.SiteID?.Value*/ }))
			{
				if (detailsGroup.First().Availability?.Value == BCItemAvailabilities.DoNotUpdate)
					continue;
				StorageDetailsResult result = detailsGroup.First();
				result.SiteLastModifiedDate = detailsGroup.Where(d => d.SiteLastModifiedDate?.Value != null).Select(d => d.SiteLastModifiedDate.Value).Max().ValueField();
				result.LocationLastModifiedDate = detailsGroup.Where(d => d.LocationLastModifiedDate?.Value != null).Select(d => d.LocationLastModifiedDate.Value).Max().ValueField();
				result.SiteOnHand = detailsGroup.Sum(k => k.SiteOnHand?.Value ?? 0m).ValueField();
				result.SiteAvailable = detailsGroup.Sum(k => k.SiteAvailable?.Value ?? 0m).ValueField();
				result.SiteAvailableforIssue = detailsGroup.Sum(k => k.SiteAvailableforIssue?.Value ?? 0m).ValueField();
				result.SiteAvailableforShipping = detailsGroup.Sum(k => k.SiteAvailableforShipping?.Value ?? 0m).ValueField();
				if (bindingExt.WarehouseMode == BCWarehouseModeAttribute.SpecificWarehouse && locationMappings.Any() == false)//if warehouse is specific but nothing is configured in table
				{
					result.LocationOnHand = result.LocationAvailable = result.LocationAvailableforIssue = result.LocationAvailableforShipping = 0m.ValueField();
				}
				else
				{
					if (detailsGroup.Any(i => i.SiteID?.Value != null))
					{
						result.LocationOnHand = anyLocation ? detailsGroup.Where
							(k => warehouses.Count <= 0
							|| (siteLocationIDs.ContainsKey(k.SiteID?.Value ?? 0)
								&& (siteLocationIDs[k.SiteID?.Value ?? 0].Count == 0
								|| (k.LocationID?.Value != null
									&& siteLocationIDs[k.SiteID?.Value ?? 0].ContainsKey(k.LocationID.Value.Value)))))
							.Sum(k => k.LocationOnHand?.Value ?? 0m).ValueField() : null;
						result.LocationAvailable = anyLocation ? detailsGroup.Where(
							k => warehouses.Count <= 0
							|| (siteLocationIDs.ContainsKey(k.SiteID?.Value ?? 0)
								&& (siteLocationIDs[k.SiteID?.Value ?? 0].Count == 0
								|| (k.LocationID?.Value != null
									&& siteLocationIDs[k.SiteID?.Value ?? 0].ContainsKey(k.LocationID.Value.Value)))))
							.Sum(k => k.LocationAvailable?.Value ?? 0m).ValueField() : null;
						result.LocationAvailableforIssue = anyLocation ? detailsGroup.Where(
							k => warehouses.Count <= 0
							|| (siteLocationIDs.ContainsKey(k.SiteID?.Value ?? 0)
								&& (siteLocationIDs[k.SiteID?.Value ?? 0].Count == 0
								|| (k.LocationID?.Value != null
								&& siteLocationIDs[k.SiteID?.Value ?? 0].ContainsKey(k.LocationID.Value.Value)))))
							.Sum(k => k.LocationAvailableforIssue?.Value ?? 0m).ValueField() : null;
						result.LocationAvailableforShipping = anyLocation ? detailsGroup.Where(
							k => warehouses.Count <= 0
							|| (siteLocationIDs.ContainsKey(k.SiteID?.Value ?? 0)
								&& (siteLocationIDs[k.SiteID?.Value ?? 0].Count == 0
								|| (k.LocationID?.Value != null
								&& siteLocationIDs[k.SiteID?.Value ?? 0].ContainsKey(k.LocationID.Value.Value)))))
							.Sum(k => k.LocationAvailableforShipping?.Value ?? 0m).ValueField() : null;
					}
					else
						result.LocationOnHand = result.LocationAvailable = result.LocationAvailableforIssue = result.LocationAvailableforShipping = null;
				}
				results.Add(result);
			}

			var allVariants = results.Where(x => x.TemplateItemID?.Value != null);

			if (results != null)
			{
				var stockItems = results.Where(x => x.TemplateItemID?.Value == null);
				if (stockItems != null)
				{
					foreach (StorageDetailsResult line in stockItems)
					{
						Guid? noteID = line.InventoryNoteID?.Value;
						DateTime? lastModified = null;
						if (line.IsTemplate?.Value == true)
						{
							line.VariantDetails = new List<StorageDetailsResult>();
							line.VariantDetails.AddRange(allVariants.Where(x => x.TemplateItemID?.Value == line.InventoryID.Value));
							if (line.VariantDetails.Count() == 0) continue;
							lastModified = line.VariantDetails.Select(x => new DateTime?[] { x.LocationLastModifiedDate?.Value, x.SiteLastModifiedDate?.Value, x.InventoryLastModifiedDate.Value }.Where(d => d != null).Select(d => d.Value).Max()).Max();
						}

						lastModified = new DateTime?[] { lastModified, line.LocationLastModifiedDate?.Value, line.SiteLastModifiedDate?.Value, line.InventoryLastModifiedDate?.Value }.Where(d => d != null).Select(d => d.Value).Max();
						BCAvailabilityEntityBucket bucket = new BCAvailabilityEntityBucket();
						MappedAvailability obj = bucket.Product = new MappedAvailability(line, noteID, lastModified, line.ParentSyncId.Value);
						EntityStatus status = EnsureStatus(obj, SyncDirection.Export);

						obj.ParentID = line.ParentSyncId.Value;
						if (Operation.PrepareMode != PrepareMode.Reconciliation && Operation.PrepareMode != PrepareMode.Full && status != EntityStatus.Pending && Operation.SyncMethod != SyncMode.Force)
						{
							SynchronizeStatus(bucket.Product, BCSyncOperationAttribute.Reconfiguration);
							Statuses.Cache.Persist(PXDBOperation.Update);
							Statuses.Cache.Persisted(false);
							continue;
						}

						buckets.Add(bucket);
					}
				}
			}

			return buckets;
		}

		public override async Task MapBucketExport(BCAvailabilityEntityBucket bucket, IMappedEntity existing, CancellationToken cancellationToken = default)
		{
			BCBinding binding = GetBinding();
			BCBindingExt bindingExt = GetBindingExt<BCBindingExt>();

			StorageDetailsResult local = bucket.Product.Local;
			ProductQtyData external = bucket.Product.Extern = new ProductQtyData();

			MappedAvailability existingMapped = existing as MappedAvailability;
			ProductQtyData existingData = existing?.Extern as ProductQtyData;

			external.Id = local.ProductExternID.Value.ToInt();

			string availability = BCItemAvailabilities.Convert(local.Availability?.Value);
			if (availability == null || availability == BCCaptions.StoreDefault)
			{
				availability = BCItemAvailabilities.Convert(GetBindingExt<BCBindingExt>().Availability);
			}

			string notAvailable = BCItemAvailabilities.Convert(local.NotAvailMode?.Value);
			if (notAvailable == null || notAvailable == BCCaptions.StoreDefault)
			{
				notAvailable = BCItemNotAvailModes.Convert(GetBindingExt<BCBindingExt>().NotAvailMode);
			}

			if (availability == BCCaptions.AvailableTrack)
			{
				//Template item
				if (local.IsTemplate?.Value == true)
				{
					external.Variants = new List<ProductsVariantData>();
					foreach (var variant in local.VariantDetails)
					{
						ProductsVariantData variantData = new ProductsVariantData();

						string existingVariantID = bucket.Primary.Details.FirstOrDefault(d => d.EntityType == BCEntitiesAttribute.Variant && d.LocalID == local.Id)?.ExternID;
						ProductsVariantData existingVariant = existingData?.Variants.FirstOrDefault(v => v?.Id.ToString() == existingVariantID);

						if (variant.VariantExternID.Value != null)
						{
							variantData.Id = variant.VariantExternID.Value.ToInt();
							variantData.ProductId = external.Id;
							variantData.OptionValues = null;
							//Inventory Level
							variantData.InventoryLevel = GetInventoryLevel(bindingExt, variant);
							if (variantData.InventoryLevel < 0)
								variantData.InventoryLevel = 0;

							string variantAvailability = BCItemNotAvailModes.Convert(variant.Availability.Value);
							if (variantAvailability == null || variantAvailability == BCCaptions.StoreDefault)
							{
								variantAvailability = BCItemAvailabilities.Convert(GetBindingExt<BCBindingExt>().Availability);
							}

							string variantNotAvailable = BCItemNotAvailModes.Convert(variant.NotAvailMode.Value);
							if (variantNotAvailable == null || variantAvailability == BCCaptions.StoreDefault)
							{
								variantNotAvailable = BCItemAvailabilities.Convert(GetBindingExt<BCBindingExt>().NotAvailMode);
							}

							string variantStatus = local.ItemStatus?.Value;
							string parentStatus = local.ItemStatus?.Value;
							if (parentStatus == InventoryItemStatus.Active || parentStatus == InventoryItemStatus.NoPurchases || parentStatus == InventoryItemStatus.NoRequest)
							{
								if (variantStatus == InventoryItemStatus.Active || variantStatus == InventoryItemStatus.NoPurchases || variantStatus == InventoryItemStatus.NoRequest)
								{
									if (variantAvailability == BCCaptions.AvailableTrack)
									{
										if (variantData?.InventoryLevel > 0)
										{
											variantData.PurchasingDisabled = false;
										}
										else
										{
											variantData.PurchasingDisabled = false;
											if (variantNotAvailable == BCCaptions.DisableItem)
											{
												variantData.PurchasingDisabled = true;
											}
											else if (variantNotAvailable == BCCaptions.PreOrderItem || variantNotAvailable == BCCaptions.ContinueSellingItem || variantNotAvailable == BCCaptions.EnableSellingItem)
											{
												variantData.PurchasingDisabled = false;
											}
											else if (variantNotAvailable == BCCaptions.DoNothing || variantNotAvailable == BCCaptions.DoNotUpdate)
											{
												//If there is no existing product default to available
												variantData.PurchasingDisabled = existingVariant?.PurchasingDisabled ?? false;
											}
										}
									}
									else if (variantAvailability == BCCaptions.AvailableSkip)
									{
										variantData.PurchasingDisabled = false;
									}
									else if (variantAvailability == BCCaptions.PreOrder)
									{
										variantData.PurchasingDisabled = false;
									}
									else if (variantAvailability == BCCaptions.DoNotUpdate)
									{
										variantData.PurchasingDisabled = existingVariant?.PurchasingDisabled ?? false;
									}
									else if (variantAvailability == BCCaptions.Disabled)
									{
										variantData.PurchasingDisabled = true;
									}
									else
									{
										variantData.PurchasingDisabled = false;
									}

								}
								else if (variantStatus == InventoryItemStatus.Inactive || variantStatus == InventoryItemStatus.NoSales || variantStatus == InventoryItemStatus.MarkedForDeletion)
								{
									variantData.PurchasingDisabled = true;
								}
							}
							else if (parentStatus == InventoryItemStatus.Inactive || parentStatus == InventoryItemStatus.NoSales || parentStatus == InventoryItemStatus.MarkedForDeletion)
							{
								variantData.PurchasingDisabled = true;
							}
							external.Variants.Add(variantData);
						}
					}
					if (local.ItemStatus?.Value == InventoryItemStatus.Active || local.ItemStatus?.Value == InventoryItemStatus.NoPurchases || local.ItemStatus?.Value == InventoryItemStatus.NoRequest)
					{
						external.Availability = BigCommerceConstants.AvailabilityAvailable;
						external.InventoryTracking = BigCommerceConstants.InventoryTrackingVariant;

						bool? positiveInventoryLevel = external.Variants?.Sum(v => v.InventoryLevel) > 0;
						bool? purchasableVariants = external?.Variants.Any(v => v.PurchasingDisabled == false);

						if (positiveInventoryLevel == true && purchasableVariants == true)
						{
							external.Availability = BigCommerceConstants.AvailabilityAvailable;
							external.InventoryTracking = BigCommerceConstants.InventoryTrackingVariant;
						}
						else if (positiveInventoryLevel == false && purchasableVariants == true)
						{
							external.Availability = BigCommerceConstants.AvailabilityPreOrder;
							external.InventoryTracking = BigCommerceConstants.InventoryTrackingVariant;
							external.PreorderDate = existingData?.PreorderDate;
							external.IsPreorderOnly = existingData?.IsPreorderOnly;
						}
						else if (purchasableVariants == false)
						{
							external.Availability = BigCommerceConstants.AvailabilityDisabled;
							external.InventoryTracking = BigCommerceConstants.InventoryTrackingVariant;
						}
					}
					else if (local.ItemStatus?.Value == InventoryItemStatus.Inactive || local.ItemStatus?.Value == InventoryItemStatus.NoSales || local.ItemStatus?.Value == InventoryItemStatus.MarkedForDeletion)
					{
						external.Availability = BigCommerceConstants.AvailabilityDisabled;
						external.InventoryTracking = BigCommerceConstants.InventoryTrackingNone;
					}
				}
				//Stock Item
				else
				{
					external.InventoryTracking = "product";
					//Inventory Level
					external.InventoryLevel = GetInventoryLevel(bindingExt, local);
					//Not In Stock mode
					if (external.InventoryLevel <= 0)
						external.InventoryLevel = 0;

					external.Availability = BigCommerceConstants.AvailabilityAvailable;
					external.InventoryTracking = BigCommerceConstants.InventoryTrackingProduct;

					//If there is no existing product default to enable.
					if (external?.InventoryLevel <= 0)
					{
						if (notAvailable == BCCaptions.DisableItem)
						{
							external.Availability = BigCommerceConstants.AvailabilityDisabled;
							external.IsPriceHidden = true;
						}
						else if (notAvailable == BCCaptions.PreOrderItem || notAvailable == BCCaptions.ContinueSellingItem || notAvailable == BCCaptions.EnableSellingItem)
						{
							external.Availability = BigCommerceConstants.AvailabilityPreOrder;
						}
						else if (notAvailable == BCCaptions.DoNothing || notAvailable == BCCaptions.DoNotUpdate)
						{
							//If there is no existing product default to available
							external.Availability = existingData?.Availability ?? BigCommerceConstants.AvailabilityAvailable;
						}
					}
				}
			}
			else
			{
				if (availability == BCCaptions.AvailableSkip)
				{
					external.Availability = BigCommerceConstants.AvailabilityAvailable;
					external.InventoryTracking = BigCommerceConstants.InventoryTrackingNone;
				}
				else if (availability == BCCaptions.PreOrder)
				{
					external.Availability = BigCommerceConstants.AvailabilityPreOrder;
					external.InventoryTracking = BigCommerceConstants.InventoryTrackingNone;
					external.PreorderDate = existingData?.PreorderDate;
					external.IsPreorderOnly = existingData?.IsPreorderOnly;
				}
				else if (availability == BCCaptions.DisableItem)
				{
					external.Availability = BigCommerceConstants.AvailabilityDisabled;
					external.InventoryTracking = BigCommerceConstants.InventoryTrackingNone;
					external.IsPriceHidden = true;
				}
				else if (availability == BCCaptions.DoNotUpdate)
				{
					external.Availability = existingData?.Availability ?? BigCommerceConstants.AvailabilityAvailable;
					external.InventoryTracking = existingData?.InventoryTracking ?? BigCommerceConstants.InventoryTrackingNone;
					external.PreorderDate = existingData?.PreorderDate;
					external.IsPreorderOnly = existingData?.IsPreorderOnly;
				}
			}
		}

		public override async Task SaveBucketsExport(List<BCAvailabilityEntityBucket> buckets, CancellationToken cancellationToken = default)
		{
			SyncResult syncResult = SyncResult.Error;
			Exception Error = null;

			productDataProvider.UpdateAllQty(buckets.Select(b => b.Product.Extern).ToList(), delegate (ItemProcessCallback<ProductQtyData> callback)
			{
				BCAvailabilityEntityBucket bucket = buckets[callback.Index];
				if (callback.IsSuccess)
				{
					ProductQtyData data = callback.Result;
					if (bucket.Product.Extern.Variants != null && bucket.Product.Extern.Variants.Count > 0)
					{
						variantBatchRestDataProvider.UpdateAll(bucket.Product.Extern.Variants.ToList(), delegate (ItemProcessCallback<ProductsVariantData> callbackVariant)
						{
							if (!callbackVariant.IsSuccess)
							{
								Error = callbackVariant.Error;
								return;
							}
						});
					}

					if (Error != null)
					{
						List<string> notFoundVariantsInvCode = new List<string>();
						//Trying to re sync individually
						if (Error is RestException restException && restException.ResponceStatusCode == BigCommerceConstants.HttpInvalidData)
						{
							foreach (ProductsVariantData variant in bucket.Product.Extern.Variants)
							{
								try
								{
									productVariantDataProvider.Update(variant, variant.Id.ToString(), variant.ProductId.ToString());
								}
								catch (RestException ex)
								{
									if (ex.ResponceStatusCode == HttpStatusCode.NotFound.ToString())
									{
										string inventoryCode = bucket.Product.Local.VariantDetails.FirstOrDefault(x => x.VariantExternID.Value == variant.Id.ToString())?.InventoryCD.Value;
										notFoundVariantsInvCode.Add(inventoryCode);
									}
								}
							}
						}

						if (notFoundVariantsInvCode.Any())
						{							
							string errorMessage = String.Format(BCMessages.RecordPartialDeletedInExternalSystem, String.Join(",", notFoundVariantsInvCode));
							DeleteStatus(bucket.Product, BCSyncOperationAttribute.LocalDelete, errorMessage);
							syncResult = SyncResult.Processed;
							Error = null;
						}
						else
						{
						Log(bucket.Product?.SyncID, SyncDirection.Export, Error);
						UpdateStatus(bucket.Product, BCSyncOperationAttribute.ExternFailed, Error.ToString());
						}

					}
					else if (bucket.Product.Local.Availability?.Value != BCItemAvailabilities.AvailableTrack)
					{
						bucket.Product.AddExtern(data, data.Id?.ToString(), null, data.DateModified);
						DeleteStatus(bucket.Product, BCSyncOperationAttribute.LocalDelete, BCMessages.ProductAvailabilityReconfigured);
						syncResult = SyncResult.Processed;
					}
					else
					{
						bucket.Product.AddExtern(data, data.Id?.ToString(), null, data.DateModified);
						UpdateStatus(bucket.Product, BCSyncOperationAttribute.ExternUpdate);
						syncResult = SyncResult.Processed;
					}

				}
				else
				{
					productDataProvider.UpdateAllQty(new List<ProductQtyData>() { bucket.Product.Extern }, delegate (ItemProcessCallback<ProductQtyData> retrycallback)
					{
						if (retrycallback.IsSuccess)
						{
							ProductQtyData data = retrycallback.Result;
							bucket.Product.AddExtern(data, data.Id?.ToString(), null, data.DateModified);
							UpdateStatus(bucket.Product, BCSyncOperationAttribute.ExternUpdate);
							syncResult = SyncResult.Processed;
						}
						else if (retrycallback.Error?.ResponceStatusCode == BigCommerceConstants.HttpInvalidData) //id not found
						{
							UpdateSyncStatusRemoved(BCSyncStatus.PK.Find(this, bucket.Product.ParentID), BCSyncOperationAttribute.ExternDelete);
							syncResult = SyncResult.Deleted;
							Error = null;
						}
						else if (retrycallback.Error?.ResponceStatusCode == BigCommerceConstants.HttpPartialSuccess) 
						{
							var externVariants = variantBatchRestDataProvider.GetVariants(bucket.Product?.ExternID).ToList();
							var notFoundVariantsIds = bucket.Product.Extern.Variants.Select(x => x.Id.ToString()).Except(externVariants.Select(x => x.Id.ToString()));
							List<string> notFoundVariantsInvCode = bucket.Product.Local.VariantDetails
																							.Where(detail => notFoundVariantsIds.Contains(detail.VariantExternID.Value))
																							.Select(detail => detail.InventoryCD.Value)
																							.ToList();
							if (notFoundVariantsIds.Any())
							{
								string errorMessage = String.Format(BCMessages.RecordPartialDeletedInExternalSystem, String.Join(",", notFoundVariantsInvCode));
								DeleteStatus(bucket.Product, BCSyncOperationAttribute.LocalDelete, errorMessage);
								syncResult = SyncResult.Processed;
								Error = null;
							}
							}

						if(syncResult == SyncResult.Error)
						{
							Log(bucket.Product?.SyncID, SyncDirection.Export, retrycallback.Error);
							UpdateStatus(bucket.Product, BCSyncOperationAttribute.ExternFailed, retrycallback.Error.ToString());
							Error = retrycallback.Error;
						}
						
					});
				}

				Operation.Callback?.Invoke(new SyncInfo(bucket?.Primary?.SyncID ?? 0, SyncDirection.Export, syncResult, Error));
			});
		}
		#endregion
	}
}
