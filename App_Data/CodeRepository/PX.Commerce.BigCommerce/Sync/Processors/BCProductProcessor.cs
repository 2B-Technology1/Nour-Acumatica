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

using PX.Api.ContractBased.Models;
using PX.Commerce.BigCommerce.API.REST;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Commerce.Objects;
using PX.Commerce.Objects.Substitutes;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.IN.RelatedItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static PX.Commerce.Core.BCSyncSystemAttribute;

namespace PX.Commerce.BigCommerce
{
	public class BCProductEntityBucket<TPrimaryMapped> : EntityBucketBase, IEntityBucket
		where TPrimaryMapped : IMappedEntity
	{
		public IMappedEntity Primary => Product;
		public IMappedEntity[] Entities => new IMappedEntity[] { Primary };
		public TPrimaryMapped Product;
		public List<MappedCategory> Categories = new List<MappedCategory>();
		public Dictionary<string, long?> VariantMappings = new Dictionary<string, long?>();
	}
	public abstract class BCProductProcessor<TGraph, TEntityBucket, TPrimaryMapped, TExternType, TLocalType> : ProductProcessorBase<TGraph, TEntityBucket, TPrimaryMapped, TExternType, TLocalType>
		where TGraph : PXGraph
		where TEntityBucket : BCProductEntityBucket<TPrimaryMapped>, new()
		where TPrimaryMapped : class, IMappedEntityLocal<TLocalType>, new()
		where TExternType : BCAPIEntity, IExternEntity, new()
		where TLocalType : ProductItem, ILocalEntity, new()

	{
		private IChildRestDataProvider<ProductsImageData> productImageDataProvider;
		private IChildRestDataProvider<ProductsVideo> productVideoDataProvider;
		protected IStockRestDataProvider<ProductData> productDataProvider;
		protected IParentReadOnlyRestDataProvider<ProductsTaxData> taxDataProvider;
		protected IChildRestDataProvider<ProductsCustomFieldData> productsCustomFieldDataProvider;
		protected List<ProductsTaxData> taxClasses;
		protected BCRestClient client;

		#region Factories
		[InjectDependency]
		protected IBCRestDataProviderFactory<IChildRestDataProvider<ProductsImageData>> productImageDataProviderFactory { get; set; }
		[InjectDependency]
		protected IBCRestDataProviderFactory<IChildRestDataProvider<ProductsVideo>> productVideoDataProviderFactory { get; set; }
		[InjectDependency]
		protected IBCRestDataProviderFactory<IChildRestDataProvider<ProductsCustomFieldData>> productsCustomFieldDataProviderFactory { get; set; }
		[InjectDependency]
		protected IBCRestDataProviderFactory<IStockRestDataProvider<ProductData>> productDataProviderFactory { get; set; }
		[InjectDependency]
		protected IBCRestDataProviderFactory<IParentReadOnlyRestDataProvider<ProductsTaxData>> taxDataProviderFactory { get; set; }
		#endregion

		public override async Task Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			await base.Initialise(iconnector, operation);
			client = BCConnector.GetRestClient(GetBindingExt<BCBindingBigCommerce>());
			productDataProvider = productDataProviderFactory.CreateInstance(client);
			productImageDataProvider = productImageDataProviderFactory.CreateInstance(client);
			productVideoDataProvider = productVideoDataProviderFactory.CreateInstance(client);
			productsCustomFieldDataProvider = productsCustomFieldDataProviderFactory.CreateInstance(client);
			taxDataProvider = taxDataProviderFactory.CreateInstance(client);
			taxClasses = taxDataProvider.GetAll().ToList();
		}

		#region Common

		/// <summary>
		/// Initialize a new object of the entity to be used to Get bucket
		/// </summary>
		/// <returns>The initialized entity</returns>
		protected abstract TLocalType CreateEntityForGet();

		public List<ProductData> PullSimilar(string description, string inventoryId, out string uniqueField, CancellationToken cancellationToken = default)
		{
			uniqueField = inventoryId;

			List<ProductData> datas = null;
			if (!string.IsNullOrEmpty(inventoryId))
			{

				datas = productDataProvider.GetAll(new FilterProducts() { SKU = inventoryId }, cancellationToken)?.ToList();
			}
			if ((datas == null || datas.Count == 0) && !string.IsNullOrEmpty(description))
			{
				uniqueField = description;
				datas = productDataProvider.GetAll(new FilterProducts() { Name = description }, cancellationToken)?.ToList();
			}
		
			if (datas == null) return null;
			var id = datas.FirstOrDefault(x => x.Id != null)?.Id;
			if (id != null){
				var statuses = PXSelect<BCSyncStatus,
					Where<BCSyncStatus.connectorType, Equal<Required<BCSyncStatus.connectorType>>,
						And<BCSyncStatus.bindingID, Equal<Required<BCSyncStatus.bindingID>>,
						And<BCSyncStatus.externID, Equal<Required<BCSyncStatus.externID>>>>>>.Select(this, Operation.ConnectorType, Operation.Binding, id);
				if (statuses != null)
				{
					if ((Operation.EntityType == BCEntitiesAttribute.ProductWithVariant && statuses.Any(x => x.GetItem<BCSyncStatus>().EntityType == BCEntitiesAttribute.StockItem || x.GetItem<BCSyncStatus>().EntityType == BCEntitiesAttribute.NonStockItem)) ||
						(Operation.EntityType == BCEntitiesAttribute.StockItem && statuses.Any(x => x.GetItem<BCSyncStatus>().EntityType == BCEntitiesAttribute.ProductWithVariant || x.GetItem<BCSyncStatus>().EntityType == BCEntitiesAttribute.NonStockItem)) ||
						(Operation.EntityType == BCEntitiesAttribute.NonStockItem && statuses.Any(x => x.GetItem<BCSyncStatus>().EntityType == BCEntitiesAttribute.StockItem || x.GetItem<BCSyncStatus>().EntityType == BCEntitiesAttribute.ProductWithVariant)))
					{
						throw new PXException(BigCommerceMessages.MappedToOtherEntity, uniqueField);
					}

				}
			}

			return datas;
		}

		public virtual void MapCustomUrl(IMappedEntity existing, string url, ProductData data)
		{
			ProductData existingProduct = existing?.Extern as ProductData;
			if (existingProduct?.CustomUrl?.Url == null && url != null)// For new Product
				data.CustomUrl = new ProductCustomUrl() { Url = url, IsCustomized = true };
			//Bigcommerce do not allow to update product with same custom Url so skip if url is same.
			if (existingProduct?.CustomUrl?.Url != null && url != null)
				if (!existingProduct.CustomUrl.Url.TrimEnd(new char[] { '/' }).Equals(url.TrimEnd(new char[] { '/' })))
					data.CustomUrl = new ProductCustomUrl() { Url = url, IsCustomized = true };
		}

		
		/// <inheritdoc/>
		protected override void DeleteImageFromExternalSystem(string parentId, string imageId)
		{
			try
			{
				productImageDataProvider.Delete(imageId, parentId);
			}
			catch (RestException ex)
			{
				throw new PXException(ex, BigCommerceMessages.ErrorDuringImageDeletionExceptionMessage, ex.Message);
			}
		}

		public virtual void SaveImages(IMappedEntity obj, List<InventoryFileUrls> urls, CancellationToken cancellationToken = default)
		{
			var fileURLs = urls?.Where(x => x.FileType?.Value == BCCaptions.Image);

			SyncDeletedMediaUrls(obj, fileURLs?.ToList());

			if (fileURLs == null) return;

			List<DetailInfo> existingList = new List<DetailInfo>(obj.Details);

			var existingImages = productImageDataProvider.GetAll(obj.ExternID, cancellationToken);
			// clear all Product Image records from Details 
			obj.ClearDetails(BCEntitiesAttribute.ProductImage);


			foreach (var image in fileURLs)
			{
				if (!string.IsNullOrEmpty(image.FileURL?.Value))
				{
					var productImage = new ProductsImageData();
					productImage.ImageUrl = Uri.EscapeUriString(System.Web.HttpUtility.UrlDecode(image.FileURL?.Value));
					try
					{
						ProductsImageData response;
						if (existingList.Any(x => x.LocalID == image.NoteID?.Value) == false)
						{
							response = productImageDataProvider.Create(productImage, obj.ExternID);
							if (response.Id > 0)
								obj.AddDetail(BCEntitiesAttribute.ProductImage, image.NoteID.Value, response.Id.ToString());
						}
						else
						{
							var detail = existingList.FirstOrDefault(x => x.LocalID == image.NoteID?.Value);

							if (int.TryParse(detail.ExternID, out int id) && id > 0)
							{
								// check if the image still exists in the external store before updating
								if (existingImages.Any(x => x.Id == id))
								{
									// the image still exists => update it
								response = productImageDataProvider.Update(productImage, detail.ExternID, obj.ExternID);
									obj.AddDetail(BCEntitiesAttribute.ProductImage, image.NoteID.Value, detail.ExternID);
						}
								else
								{
									// the image no longer exists => create a new one
									response = productImageDataProvider.Create(productImage, obj.ExternID);
									if (response.Id > 0)
									{
										// add a new detail record with the new ExternId 
										obj.AddDetail(BCEntitiesAttribute.ProductImage, image.NoteID.Value, response.Id.ToString());
									}
								}
							}
						}
					}
					catch (RestException ex)
					{
						if (ex.ResponceStatusCode == HttpStatusCode.BadRequest.ToString())
							throw new PXException(BigCommerceMessages.InvalidImage);
						throw;
					}
				}
			}
		}
		public virtual void SaveVideos(IMappedEntity obj, List<InventoryFileUrls> urls)
		{
			var fileURLs = urls?.Where(x => x.FileType?.Value == BCCaptions.Video);
			if (fileURLs == null) return;

			//map Videos
			foreach (var video in fileURLs)
			{
				if (!string.IsNullOrEmpty(video.FileURL?.Value) && obj.Details?.Any(x => x.LocalID == video.NoteID?.Value) == false)
				{
					var productVideo = new ProductsVideo();
					try
					{
						productVideo.VideoId = Regex.Match(video.FileURL?.Value, @"^.*(?:(?:youtu\.be\/|v\/|vi\/|u\/\w\/|embed\/)|(?:(?:watch)?\?v(?:i)?=|\&v(?:i)?=))([^#\&\?]*).*", RegexOptions.IgnoreCase).Groups[1].Value;
						ProductsVideo response = productVideoDataProvider.Create(productVideo, obj.ExternID);
						obj.AddDetail(BCEntitiesAttribute.ProductVideo, video.NoteID.Value, response.Id.ToString());
					}
					catch (RestException ex)
					{
						if (ex.ResponceStatusCode == HttpStatusCode.Conflict.ToString())
							throw new PXException(BigCommerceMessages.InvalidVideo);
						throw;
					}
				}
			}

		}

		public virtual List<ProductsCustomFieldData> ExportCustomFields(IMappedEntity obj, IList<ProductsCustomField> customFields, ProductData data, CancellationToken cancellationToken = default)
		{
			if (customFields == null || customFields.Count <= 0) return null;

			var cFields = new List<ProductsCustomFieldData>(customFields?.Select(c => c.Data));
			if (obj.ExternID != null && cFields != null)
			{
				var externalcustomFields = productsCustomFieldDataProvider.GetAll(obj.ExternID, cancellationToken)?.ToList();
				foreach (var cdata in cFields)
				{
					var extID = externalcustomFields.Where(x => x.Name == cdata.Name).FirstOrDefault();
					//Update Custom field if value is specified in local system
					if (extID != null && !String.IsNullOrEmpty(cdata.Value))
					{
						cdata.Id = extID.Id;
						//productsCustomFieldDataProvider.Update(cdata.Data, extID.Id.ToString(), data.Id.ToString());
					}
					//Delete Custom field if value is not specified in local system but exists in external
					else if (extID != null && String.IsNullOrEmpty(cdata.Value))
					{
						productsCustomFieldDataProvider.Delete(extID.Id.ToString(), obj.ExternID);
					}
				}
			}
			return cFields.Where(x => !String.IsNullOrEmpty(x.Value))?.ToList();
		}
		public override List<(string fieldName, string fieldValue)> GetExternCustomFieldList(BCEntity entity, ExternCustomFieldInfo customFieldInfo)
		{
			return new List<(string fieldName, string fieldValue)>() { (BCConstants.AutoMapping, BCConstants.AutoMapping) };
		}
		public override void ValidateExternCustomField(BCEntity entity, ExternCustomFieldInfo customFieldInfo, string sourceObject, string sourceField, string targetObject, string targetField, EntityOperationType direction)
		{
			if (!string.IsNullOrEmpty(targetField) && targetField != BCConstants.AutoMapping && direction == EntityOperationType.ExportMapping)
			{
				throw new PXException(BCMessages.InvalidSourceFieldAutoMapping);
			}
			if (!string.IsNullOrEmpty(sourceField) && sourceField.StartsWith("=") && direction == EntityOperationType.ExportMapping)
			{
				throw new PXException(BCMessages.InvalidSourceFieldCustomFields);
			}
		}

		public override void SetExternCustomFieldValue(TEntityBucket entity, ExternCustomFieldInfo customFieldInfo,
			object targetData, string targetObject, string targetField, string sourceObject, object value, IMappedEntity existing)
		{
			if (value != PXCache.NotSetValue)
			{
				var sourceinfo = sourceObject?.Split('.');
				string sFieldName = (sourceinfo?.Length == 2) ? sourceinfo?[1] : sourceinfo?[0];
				if (!String.IsNullOrEmpty(sFieldName))
				{
					ProductData data = (ProductData)entity.Primary.Extern;
					data.CustomFields.Add(new ProductsCustomField() { Data = new ProductsCustomFieldData() { Id = null, Name = sFieldName, Value = value.ToString() } });
				}
			}
		}

		public virtual List<int> MapRelatedItems(IMappedEntity obj)
		{
			BCBinding binding = GetBinding();
			string[] categoriesAllowed = GetBindingExt<BCBindingExt>().RelatedItems?.Split(',');
			Boolean anyRelation = false;
			List<int> ids = new List<int>();
			if (categoriesAllowed != null && categoriesAllowed.Count() > 0 && !String.IsNullOrWhiteSpace(categoriesAllowed[0]))
			{
				PXResultset<PX.Objects.IN.InventoryItem, INRelatedInventory, BCChildrenInventoryItem, BCSyncStatus> relates = PXSelectJoin<PX.Objects.IN.InventoryItem,
					InnerJoin<INRelatedInventory, On<PX.Objects.IN.InventoryItem.inventoryID, Equal<INRelatedInventory.inventoryID>>,
					InnerJoin<BCChildrenInventoryItem, On<INRelatedInventory.relatedInventoryID, Equal<BCChildrenInventoryItem.inventoryID>>,
					LeftJoin<BCSyncStatus, On<BCSyncStatus.localID, Equal<BCChildrenInventoryItem.noteID>,
						And<BCSyncStatus.connectorType, Equal<Required<BCSyncStatus.connectorType>>,
						And<BCSyncStatus.bindingID, Equal<Required<BCSyncStatus.bindingID>>,
						And<Where<BCSyncStatus.entityType, Equal<BCEntitiesAttribute.stockItem>, Or<BCSyncStatus.entityType, Equal<BCEntitiesAttribute.nonStockItem>>>
						>>>>>>>,
					   Where<PX.Objects.IN.InventoryItem.noteID, Equal<Required<PX.Objects.IN.InventoryItem.noteID>>>>
						.Select<PXResultset<PX.Objects.IN.InventoryItem, INRelatedInventory, BCChildrenInventoryItem, BCSyncStatus>>(this,
						binding.ConnectorType,
						binding.BindingID,
						obj.LocalID);

				if (relates?.Count > 0)
				{
					var existing = new List<DetailInfo>(obj.Details);
					obj.ClearDetails();
					existing.RemoveAll(x => x.EntityType == BCEntitiesAttribute.RelatedItem);
					foreach (var detail in existing)
					{
						if (!obj.Details.Any(x => x.EntityType == detail.EntityType && x.LocalID == detail.LocalID))
							obj.AddDetail(detail.EntityType, detail.LocalID, detail.ExternID);
					}
				}

				foreach (var rel in relates)
				{
					anyRelation = true;
					BCChildrenInventoryItem inventoryItem = rel.GetItem<BCChildrenInventoryItem>();
					INRelatedInventory row = rel.GetItem<INRelatedInventory>();
					if (row.IsActive == true
						&& categoriesAllowed.Contains(row.Relation)
						&& (row.ExpirationDate == null || row.ExpirationDate > DateTime.Now))
					{
						string relatedItemExternID = rel.GetItem<BCSyncStatus>().ExternID;
						if (relatedItemExternID != null)
							ids.Add((int)relatedItemExternID.ToInt());
						if (!obj.Details.Any(x => x.EntityType == BCEntitiesAttribute.RelatedItem && x.LocalID == inventoryItem.NoteID))
							obj.AddDetail(BCEntitiesAttribute.RelatedItem, inventoryItem.NoteID, relatedItemExternID);
					}
				}
			}
			return anyRelation ? ids : null;
		}

		public virtual void UpdateRelatedItems(IMappedEntity obj)
		{
			string[] categoriesAllowed = GetBindingExt<BCBindingExt>()?.RelatedItems?.Split(',');
			BCBinding binding = GetBinding();
			if (categoriesAllowed != null && categoriesAllowed.Count() > 0 && !String.IsNullOrWhiteSpace(categoriesAllowed[0]))
			{
				List<IMappedEntity> relatedMappedProducts = new List<IMappedEntity>();
				List<RelatedProductsData> relatedProductsData = new List<RelatedProductsData>();
				foreach (PXResult<BCSyncDetail, BCSyncStatus> relatedItems in PXSelectJoin<BCSyncDetail,
							InnerJoin<BCSyncStatus, On<BCSyncStatus.syncID, Equal<BCSyncDetail.syncID>>>,
							Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
								And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
								And<BCSyncDetail.entityType, Equal<Required<BCSyncDetail.entityType>>,
								And<BCSyncDetail.localID, Equal<Required<BCSyncDetail.localID>>,
								And<BCSyncDetail.externID, IsNull>>>>>>.Select(this, BCEntitiesAttribute.RelatedItem, obj.LocalID))
				{
					var pstatus = relatedItems.GetItem<BCSyncStatus>();
					var detail = relatedItems.GetItem<BCSyncDetail>();
					IMappedEntity item;
					if (pstatus.EntityType.Equals(BCEntitiesAttribute.NonStockItem))
					{
						item = new MappedNonStockItem() { SyncID = pstatus.SyncID }.Set(pstatus);

					}
					else
					{
						item = new MappedStockItem() { SyncID = pstatus.SyncID }.Set(pstatus);
					}
					item.LocalTimeStamp = DateTime.MaxValue;
					EnsureDetails(item);
					relatedMappedProducts.Add(item);
					var relatedIds = new List<int>() { (int)obj.ExternID.ToInt() };
					var existingIds = item.Details?.Where(x => x.ExternID != null)?.Select(x => (int)x.ExternID.ToInt());
					if (existingIds?.Count() > 0)
						relatedIds.AddRange(existingIds);
					relatedProductsData.Add(new RelatedProductsData()
					{
						Id = item.ExternID.ToInt(),
						RelatedProducts = relatedIds,
					});
				}

				bool retryAttempt = true;
				while (retryAttempt && relatedProductsData.Count() > 0)
				{
					bool attemptedToRemoveFailingEntry = false;
					retryAttempt = false;
					productDataProvider.UpdateAllRelations(relatedProductsData, delegate (ItemProcessCallback<RelatedProductsData> callback)
					{
						IMappedEntity item;
						if (callback.IsSuccess)
						{
							UpdateRelatedItemStatus(obj, relatedMappedProducts, callback);
						}
						else
						{
							if (!attemptedToRemoveFailingEntry && string.Equals(callback.Error.ResponceStatusCode, "422"))
							{
								attemptedToRemoveFailingEntry = true;
								string[] messages = callback.Error.Message.Split('\n');
								int failedID;
								string clean = Regex.Replace(messages.First(), "[^0-9]", "");
								if (messages.First().ToLower().Contains("not found") && int.TryParse(clean, out failedID))
								{
									RelatedProductsData failedItem = relatedProductsData.Find(i => i.Id == failedID);
									relatedProductsData.Remove(failedItem);
									Log(failedID, SyncDirection.Export, callback.Error);
									retryAttempt = true;
									return;
								}
							}
							if (attemptedToRemoveFailingEntry && !retryAttempt)
								productDataProvider.UpdateAllRelations(new List<RelatedProductsData>() { relatedProductsData[callback.Index] }, delegate (ItemProcessCallback<RelatedProductsData> retrycallback)
								{
									item = relatedMappedProducts[callback.Index];
									if (retrycallback.IsSuccess)
									{
										UpdateRelatedItemStatus(obj, relatedMappedProducts, callback);
									}
									else
									{
										Log(item.SyncID, SyncDirection.Export, callback.Error);
									}
								});
						}
					});
				}
			}
		}

		public virtual void UpdateRelatedItemStatus(IMappedEntity obj, List<IMappedEntity> relatedMappedProducts, ItemProcessCallback<RelatedProductsData> callback)
		{
			IMappedEntity item = relatedMappedProducts.FirstOrDefault(i => i.ExternID?.ToInt() == callback.Result.Id);
			item.ExternTimeStamp = callback.Result.DateModified;
			var existing = new List<DetailInfo>(item.Details);
			var detailToUpdate = existing?.FirstOrDefault(x => x.LocalID == obj.LocalID);
			detailToUpdate.ExternID = obj.ExternID;
			item.ClearDetails();
			foreach (var detail in existing)
			{
				if (!item.Details.Any(x => x.EntityType == detail.EntityType && x.LocalID == detail.LocalID))
					item.AddDetail(detail.EntityType, detail.LocalID, detail.ExternID);
			}
			UpdateStatus(item, BCSyncOperationAttribute.ExternUpdate);
		}

		protected virtual string GetCrossReferenceValue(ProductItem entity, StringValue UOMUnit, string AlternateTypeValue)
		{
			string result = null;

			if (!string.IsNullOrWhiteSpace(UOMUnit?.Value))
			{
				result = (entity.CrossReferences?.FirstOrDefault(x => x.AlternateType?.Value == AlternateTypeValue && x.UOM?.Value == UOMUnit?.Value) ??
						entity.CrossReferences?.FirstOrDefault(x => x.AlternateType?.Value == AlternateTypeValue && string.IsNullOrWhiteSpace(x.UOM?.Value)))?.AlternateID?.Value;
			}
			return result;
		}

		#endregion

		#region Export

		public override async Task<EntityStatus> GetBucketForExport(TEntityBucket bucket, BCSyncStatus syncstatus, CancellationToken cancellationToken = default)
		{
			TLocalType item = CreateEntityForGet();
			TLocalType impl = cbapi.GetByID(syncstatus.LocalID, item, GetCustomFieldsForExport());
			if (impl == null) return EntityStatus.None;

			TPrimaryMapped obj = bucket.Product = bucket.Product.Set(impl, impl.SyncID, impl.SyncTime);
			EntityStatus status = EnsureStatus(obj, SyncDirection.Export);

			if (GetEntity(BCEntitiesAttribute.SalesCategory)?.IsActive == true)
			{
				var categories = impl.Categories;
				if (categories != null)
				{
					foreach (CategoryStockItem category in categories)
					{
						BCSyncStatus result = PXSelectJoin<BCSyncStatus,
							InnerJoin<PX.Objects.IN.INCategory, On<PX.Objects.IN.INCategory.noteID, Equal<BCSyncStatus.localID>>>,
							Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
								And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
								And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
								And<PX.Objects.IN.INCategory.categoryID, Equal<Required<PX.Objects.IN.INCategory.categoryID>>>>>>>
							.Select(this, BCEntitiesAttribute.SalesCategory, category.CategoryID.Value);
						if (result != null && result.ExternID != null && result.LocalID != null) continue;

						BCItemSalesCategory implCat = cbapi.Get<BCItemSalesCategory>(new BCItemSalesCategory() { CategoryID = new IntSearch() { Value = category.CategoryID.Value } });
						if (implCat == null) continue;

						MappedCategory mappedCategory = new MappedCategory(implCat, implCat.SyncID, implCat.SyncTime);
						EntityStatus mappedCategoryStatus = EnsureStatus(mappedCategory, SyncDirection.Export);
						if (mappedCategoryStatus == EntityStatus.Deleted)
							throw new PXException(BCMessages.CategoryIsDeletedForItem, category.CategoryID.Value, impl.Description);
						if (mappedCategoryStatus == EntityStatus.Pending)
							bucket.Categories.Add(mappedCategory);
					}
				}
			}
			return status;
		}

		#endregion
	}
}
