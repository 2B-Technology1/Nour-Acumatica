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
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.IN;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PX.Commerce.BigCommerce
{
	public class BCProductWithVariantEntityBucket : BCProductEntityBucket<MappedTemplateItem>
	{
		public override IMappedEntity[] PreProcessors { get => Categories.ToArray(); }
	}

	public class BCTemplateItem : BCBaseRestrictor, IRestrictor
	{
		public virtual FilterResult RestrictExport(IProcessor processor, IMappedEntity mapped, FilterMode mode)
		{
			return base.Restrict<MappedTemplateItem>(mapped, delegate (MappedTemplateItem obj)
			{
				if (obj.Local != null && obj.Local.ExportToExternal?.Value == false)
				{
					return new FilterResult(FilterStatus.Invalid,
						PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogItemNoExport, obj.Local.InventoryID?.Value ?? obj.Local.SyncID.ToString()));
				}

				if (mode != FilterMode.Merge
					&& obj.Local != null && (obj.Local.Matrix == null || obj.Local.Matrix?.Count == 0))
				{
					return new FilterResult(FilterStatus.Invalid,
						PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogTemplateSkippedNoMatrix, obj.Local.InventoryID?.Value ?? obj.Local.SyncID.ToString()));
				}

				return null;
			});
		}

		public virtual FilterResult RestrictImport(IProcessor processor, IMappedEntity mapped, FilterMode mode)
		{
			return null;
		}
	}


	[BCProcessor(typeof(BCConnector), BCEntitiesAttribute.ProductWithVariant, BCCaptions.TemplateItem, 70,
		IsInternal = false,
		Direction = SyncDirection.Export,
		PrimaryDirection = SyncDirection.Export,
		PrimarySystem = PrimarySystem.Local,
		PrimaryGraph = typeof(PX.Objects.IN.InventoryItemMaint),
		ExternTypes = new Type[] { typeof(ProductData) },
		LocalTypes = new Type[] { typeof(TemplateItems) },
		AcumaticaPrimaryType = typeof(PX.Objects.IN.InventoryItem),
		AcumaticaPrimarySelect = typeof(Search<PX.Objects.IN.InventoryItem.inventoryCD, Where<PX.Objects.IN.InventoryItem.isTemplate, Equal<True>>>),
		AcumaticaFeaturesSet = typeof(FeaturesSet.matrixItem),
		URL = "products/{0}/edit",
		Requires = new string[] { }
	)]
	[BCProcessorDetail(EntityType = BCEntitiesAttribute.ProductVideo, EntityName = BCCaptions.ProductVideo, AcumaticaType = typeof(BCInventoryFileUrls))]
	[BCProcessorDetail(EntityType = BCEntitiesAttribute.RelatedItem, EntityName = BCCaptions.RelatedItem, AcumaticaType = typeof(PX.Objects.IN.InventoryItem))]
	[BCProcessorDetail(EntityType = BCEntitiesAttribute.ProductOption, EntityName = BCCaptions.ProductOption, AcumaticaType = typeof(PX.Objects.CS.CSAttribute))]
	[BCProcessorDetail(EntityType = BCEntitiesAttribute.ProductOptionValue, EntityName = BCCaptions.ProductOption, AcumaticaType = typeof(PX.Objects.CS.CSAttributeDetail))]
	[BCProcessorDetail(EntityType = BCEntitiesAttribute.Variant, EntityName = BCCaptions.Variant, AcumaticaType = typeof(PX.Objects.IN.InventoryItem))]
	[BCProcessorRealtime(PushSupported = true, HookSupported = false,
		PushSources = new String[] { "BC-PUSH-Variants" }, PushDestination = BCConstants.PushNotificationDestination)]
	[BCProcessorExternCustomField(BCConstants.CustomFields, BigCommerceCaptions.CustomFields, nameof(ProductData.CustomFields), typeof(ProductData))]
	public class BCTemplateItemProcessor : BCProductProcessor<BCTemplateItemProcessor, BCProductWithVariantEntityBucket, MappedTemplateItem, ProductData, TemplateItems>
	{
		private IChildRestDataProvider<ProductsOptionData> productsOptionRestDataProvider;
		private ISubChildRestDataProvider<ProductOptionValueData> productsOptionValueRestDataProvider;
		private IChildRestDataProvider<ProductsVariantData> productVariantRestDataProvider;
		protected IChildUpdateAllRestDataProvider<ProductsVariantData> productvariantBatchProvider;

		#region Factories
		[InjectDependency]
		private IBCRestDataProviderFactory<IChildRestDataProvider<ProductsOptionData>> productsOptionRestDataProviderFactory { get; set; }
		[InjectDependency]
		private IBCRestDataProviderFactory<ISubChildRestDataProvider<ProductOptionValueData>> productsOptionValueRestDataProviderFactory { get; set; }
		[InjectDependency]
		protected IBCRestDataProviderFactory<IChildUpdateAllRestDataProvider<ProductsVariantData>> productvariantBatchProviderFactory { get; set; }
		[InjectDependency]
		protected IBCRestDataProviderFactory<IChildRestDataProvider<ProductsVariantData>> productVariantRestDataProviderFactory { get; set; }
		#endregion

		#region Constructor
		public override async Task Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			await base.Initialise(iconnector, operation);
			productsOptionRestDataProvider = productsOptionRestDataProviderFactory.CreateInstance(client);
			productsOptionValueRestDataProvider = productsOptionValueRestDataProviderFactory.CreateInstance(client);
			productvariantBatchProvider = productvariantBatchProviderFactory.CreateInstance(client);
			productVariantRestDataProvider = productVariantRestDataProviderFactory.CreateInstance(client);
		}
		#endregion

		#region Common
		public override async Task<MappedTemplateItem> PullEntity(Guid? localID, Dictionary<string, object> fields, CancellationToken cancellationToken = default)
		{
			TemplateItems impl = cbapi.GetByID(localID,
				new TemplateItems()
				{
					ReturnBehavior = ReturnBehavior.OnlySpecified,
					Attributes = new List<AttributeValue>() { new AttributeValue() },
					Categories = new List<CategoryStockItem>() { new CategoryStockItem() },
					FileURLs = new List<InventoryFileUrls>() { new InventoryFileUrls() },
					Matrix = new List<ProductItem>() { new ProductItem() }
				});
			if (impl == null) return null;

			MappedTemplateItem obj = new MappedTemplateItem(impl, impl.SyncID, impl.SyncTime);

			return obj;
		}

		public override async Task<MappedTemplateItem> PullEntity(String externID, String jsonObject, CancellationToken cancellationToken = default)
		{
			ProductData data = productDataProvider.GetByID(externID);
			if (data == null) return null;

			MappedTemplateItem obj = new MappedTemplateItem(data, data.Id?.ToString(), data.Name, data.DateModifiedUT.ToDate());

			return obj;
		}

		/// <summary>
		/// Initialize a new object of the entity to be used to Fetch bucket
		/// </summary>
		/// <returns>The initialized entity</returns>
		protected override TemplateItems CreateEntityForFetch()
		{
			TemplateItems entity = new TemplateItems();

			entity.InventoryID = new StringReturn();
			entity.IsStockItem = new BooleanReturn();
			entity.Matrix = new List<ProductItem>() { new ProductItem() { InventoryID = new StringReturn() } };
			entity.Categories = new List<CategoryStockItem>() { new CategoryStockItem() { CategoryID = new IntReturn() } };
			entity.ExportToExternal = new BooleanReturn();
			entity.VendorDetails = new List<ProductItemVendorDetail>() { new ProductItemVendorDetail() };
			entity.ItemType = new StringReturn();

			return entity;
		}

		/// <summary>
		/// Initialize a new object of the entity to be used to Get bucket
		/// </summary>
		/// <returns>The initialized entity</returns>
		protected override TemplateItems CreateEntityForGet()
		{
			TemplateItems entity = new TemplateItems();

			entity.ReturnBehavior = ReturnBehavior.OnlySpecified;
			entity.Attributes = new List<AttributeValue>() { new AttributeValue() };
			entity.Categories = new List<CategoryStockItem>() { new CategoryStockItem() };
			entity.FileURLs = new List<InventoryFileUrls>() { new InventoryFileUrls() };
			entity.Matrix = new List<ProductItem>() { new ProductItem() };
			entity.ItemType = new StringReturn();

			return entity;
		}

		/// <summary>
		/// Creates a mapped entity for the passed entity
		/// </summary>
		/// <param name="entity">The entity to create the mapped entity from</param>
		/// <param name="syncId">The sync id of the entity</param>
		/// <param name="syncTime">The timestamp of the last modification</param>
		/// <returns>The mapped entity</returns>
		protected override MappedTemplateItem CreateMappedEntity(TemplateItems entity, Guid? syncId, DateTime? syncTime)
		{
			return new MappedTemplateItem(entity, syncId, syncTime);
		}

		/// <inheritdoc/>
		public override object GetTargetObjectExport(object currentSourceObject, IExternEntity data, BCEntityExportMapping mapping)
		{
			ProductData productData = data as ProductData;
			ProductItem productItem = currentSourceObject as ProductItem;
			//Flag that indicates if we are trying to find a matching matrix to a variant.
			bool isMatrixToVariants = mapping.SourceObject.Contains(BCConstants.Matrix) && mapping.TargetObject.Contains(BCConstants.Variants);

			return (isMatrixToVariants)?
				productData?.Variants?.FirstOrDefault(variant => variant.Sku == productItem.InventoryID.Value) :
				base.GetTargetObjectExport(currentSourceObject, data, mapping);
		}
		#endregion

		#region Import
		public override async Task FetchBucketsForImport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters, CancellationToken cancellationToken = default)
		{
			FilterProducts filter = new FilterProducts
			{
				MinDateModified = minDateTime == null ? null : minDateTime,
				MaxDateModified = maxDateTime == null ? null : maxDateTime,
			};

			IEnumerable<ProductData> datas = productDataProvider.GetAll(filter, cancellationToken);

			foreach (ProductData data in datas)
			{
				BCProductWithVariantEntityBucket bucket = CreateBucket();

				MappedTemplateItem obj = bucket.Product = bucket.Product.Set(data, data.Id?.ToString(), data.Name, data.DateModifiedUT.ToDate());
				EntityStatus status = EnsureStatus(obj, SyncDirection.Import);
			}
		}
		public override async Task<EntityStatus> GetBucketForImport(BCProductWithVariantEntityBucket bucket, BCSyncStatus syncstatus, CancellationToken cancellationToken = default)
		{
			FilterProducts filter = new FilterProducts { Include = "variants,options,images,modifiers" };
			ProductData data = productDataProvider.GetByID(syncstatus.ExternID, filter);
			if (data == null) return EntityStatus.None;
			RemoveInvalidVariant(data);

			MappedTemplateItem obj = bucket.Product = bucket.Product.Set(data, data.Id?.ToString(), data.Name, data.DateModifiedUT.ToDate());
			EntityStatus status = EnsureStatus(obj, SyncDirection.Import);

			return status;
		}

		/// <summary>
		/// Big Commerce creates by default one variant with the same SKU of the product depending of the method that the product was created.
		/// RemoveInvalidVariant objective is to remove this invalid method from the fetched data, so it can be treated and synced properly.
		/// </summary>
		/// <param name="externData"></param>
		public virtual void RemoveInvalidVariant(ProductData externData)
			=> externData.Variants?.Remove(externData.Variants?.FirstOrDefault(variant => variant.Sku == externData.Sku));

		public override async Task MapBucketImport(BCProductWithVariantEntityBucket bucket, IMappedEntity existing, CancellationToken cancellationToken = default)
		{
			MappedTemplateItem obj = bucket.Product;

			ProductData data = obj.Extern;
			// Following lines added because a stock items and non-stock item processors also have this tax category resolution, 
			// but currently there are not importing processes being used to test this code. We might still need this in future.
			//StringValue tax = obj.Extern?.TaxClassId != null ? GetSubstituteLocalByExtern(
			//		BCSubstitute.TaxClasses,
			//		taxClasses?.Find(i => i.Id == obj.Extern?.TaxClassId)?.Name, "").ValueField() :
			//		obj.Local.TaxCategory;

			TemplateItems impl = obj.Local = new TemplateItems();
			impl.Custom = GetCustomFieldsForImport();

			//Product
			impl.InventoryID = GetEntityKey(PX.Objects.IN.InventoryAttribute.DimensionName, data.Name).ValueField();
			impl.Description = data.Name.ValueField();
			impl.ItemClass = obj.LocalID == null || existing?.Local == null ? PX.Objects.IN.INItemClass.PK.Find(this, GetBindingExt<BCBindingExt>().StockItemClassID)?.ItemClassCD.ValueField() : null;

			if (GetEntity(BCEntitiesAttribute.SalesCategory)?.IsActive == true)
			{
				if (data.Categories != null) impl.Categories = new List<CategoryStockItem>();
				foreach (int cat in data.Categories)
				{
					PX.Objects.IN.INCategory incategory = PXSelectJoin<PX.Objects.IN.INCategory,
					LeftJoin<BCSyncStatus, On<PX.Objects.IN.INCategory.noteID, Equal<BCSyncStatus.localID>>>,
					Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
						And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
						And<BCSyncStatus.entityType, Equal<Current<BCEntity.entityType>>,
						And<BCSyncStatus.externID, Equal<Required<BCSyncStatus.externID>>>>>>>.Select(this, cat);

					if (incategory == null || incategory.CategoryID == null) throw new PXException(BCMessages.CategoryNotSyncronizedForItem, data.Name);

					impl.Categories.Add(new CategoryStockItem() { CategoryID = incategory.CategoryID.ValueField() });
				}
			}
		}
		public override async Task SaveBucketImport(BCProductWithVariantEntityBucket bucket, IMappedEntity existing, string operation, CancellationToken cancellationToken = default)
		{
			MappedTemplateItem obj = bucket.Product;

			if (existing?.Local != null) obj.Local.InventoryID = ((TemplateItems)existing.Local).InventoryID.Value.SearchField();

			TemplateItems impl = cbapi.Put<TemplateItems>(obj.Local, obj.LocalID);

			bucket.Product.AddLocal(impl, impl.SyncID, impl.SyncTime);
			UpdateStatus(obj, operation);
		}

		#endregion

		#region Export
		public override async Task<PullSimilarResult<MappedTemplateItem>> PullSimilar(ILocalEntity entity, CancellationToken cancellationToken = default)
		{
			List<ProductData> datas = PullSimilar(((TemplateItems)entity)?.Description?.Value, ((TemplateItems)entity)?.InventoryID?.Value, out string uniqueField, cancellationToken);
			return new PullSimilarResult<MappedTemplateItem>() { UniqueField = uniqueField, Entities = datas == null ? null : datas.Select(data => new MappedTemplateItem(data, data.Id.ToString(), data.Name, data.DateModifiedUT.ToDate())) };
		}

		public override async Task<EntityStatus> GetBucketForExport(BCProductWithVariantEntityBucket bucket, BCSyncStatus syncstatus, CancellationToken cancellationToken = default)
		{
			TemplateItems impl = GetTemplateItem(syncstatus.LocalID);
			if (impl == null) return EntityStatus.None;

			int? inventoryID = null;

			impl.AttributesDef = new List<AttributeDefinition>();
			impl.AttributesValues = new List<AttributeValue>();

			foreach (PXResult<CSAttribute, CSAttributeGroup, INItemClass, InventoryItem> attributeDef in PXSelectJoin<CSAttribute,
			   InnerJoin<CSAttributeGroup, On<CSAttributeGroup.attributeID, Equal<CSAttribute.attributeID>>,
			   InnerJoin<INItemClass, On<INItemClass.itemClassID, Equal<CSAttributeGroup.entityClassID>>,
			   InnerJoin<InventoryItem, On<InventoryItem.itemClassID, Equal<INItemClass.itemClassID>>>>>,
			  Where<InventoryItem.isTemplate, Equal<True>,
			  And<InventoryItem.noteID, Equal<Required<InventoryItem.noteID>>,
			  And<CSAttribute.controlType, Equal<Required<CSAttribute.controlType>>,
			  And<CSAttributeGroup.isActive, Equal<True>,
			  And<CSAttributeGroup.attributeCategory, Equal<CSAttributeGroup.attributeCategory.variant>
			  >>>>>>.Select(this, impl.Id, 2))
			{
				AttributeDefinition def = new AttributeDefinition();
				var inventory = (InventoryItem)attributeDef;
				inventoryID = inventory.InventoryID;
				var attribute = (CSAttribute)attributeDef;
				def.AttributeID = attribute.AttributeID.ValueField();
				def.Description = attribute.Description.ValueField();
				def.NoteID = attribute.NoteID.ValueField();
				def.Values = new List<AttributeDefinitionValue>();
				var group = (CSAttributeGroup)attributeDef;
				def.Order = group.SortOrder.ValueField();
				var attributedetails = PXSelect<CSAttributeDetail, Where<CSAttributeDetail.attributeID, Equal<Required<CSAttributeDetail.attributeID>>>>.Select(this, def.AttributeID.Value);
				foreach (CSAttributeDetail value in attributedetails)
				{
					AttributeDefinitionValue defValue = new AttributeDefinitionValue();
					defValue.NoteID = value.NoteID.ValueField();
					defValue.ValueID = value.ValueID.ValueField();
					defValue.Description = value.Description.ValueField();
					defValue.SortOrder = value.SortOrder.ToInt().ValueField();
					def.Values.Add(defValue);
				}

				if (def != null)
					impl.AttributesDef.Add(def);
			}


			foreach (PXResult<InventoryItem, CSAnswers> attributeDef in PXSelectJoin<InventoryItem,
			   InnerJoin<CSAnswers, On<InventoryItem.noteID, Equal<CSAnswers.refNoteID>>>,
			  Where<InventoryItem.templateItemID, Equal<Required<InventoryItem.templateItemID>>
			  >>.Select(this, inventoryID))
			{
				var inventory = (InventoryItem)attributeDef;
				var attribute = (CSAnswers)attributeDef;
				AttributeValue def = new AttributeValue();
				def.AttributeID = attribute.AttributeID.ValueField();
				def.NoteID = inventory.NoteID.ValueField();
				def.InventoryID = inventory.InventoryCD.ValueField();
				def.Value = attribute.Value.ValueField();
				impl.AttributesValues.Add(def);
			}
			impl.InventoryItemID = inventoryID;

			MappedTemplateItem obj = bucket.Product = bucket.Product.Set(impl, impl.SyncID, impl.SyncTime);
			EntityStatus status = EnsureStatus(obj, SyncDirection.Export);

			if (GetEntity(BCEntitiesAttribute.SalesCategory)?.IsActive == true)
			{
				if (obj.Local.Categories != null)
				{
					foreach (CategoryStockItem category in obj.Local.Categories)
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
							throw new PXException(BCMessages.CategoryIsDeletedForItem, category.CategoryID.Value, impl.Description.Value);
						if (mappedCategoryStatus == EntityStatus.Pending)
							bucket.Categories.Add(mappedCategory);

					}
				}
			}
			return status;
		}
		public override async Task MapBucketExport(BCProductWithVariantEntityBucket bucket, IMappedEntity existing, CancellationToken cancellationToken = default)
		{
			TemplateItems local = bucket.Product.Local;
			ProductData externDataToBeExported = bucket.Product.Extern = new ProductData();

			MappedTemplateItem existingMapped = existing as MappedTemplateItem;
			ProductData existingExternData = existing?.Extern as ProductData;

			if (local.Matrix == null || local.Matrix?.Count == 0)
			{
				throw new PXException(BCMessages.NoMatrixCreated);
			}

			MapInventoryItem(bucket, local, externDataToBeExported, existingExternData);
			MapCustomFields(local, externDataToBeExported);
			if (local.CustomURL?.Value != null)
				MapCustomUrl(existing, local.CustomURL?.Value, externDataToBeExported);
			MapVisibility(local, externDataToBeExported);
			MapProductOptions(local, externDataToBeExported, existingExternData);
			MapMetadata(local, externDataToBeExported, existingExternData);

			MapProductVariants(bucket, existingMapped);

			MapAvailability(bucket, local, externDataToBeExported, existingExternData);
		}


		private void MapInventoryItem(BCProductWithVariantEntityBucket bucket, TemplateItems local, ProductData external, ProductData existingData)
		{
			external.Name = local.Description?.Value;
			external.Description = GetHelper<BCHelper>().ClearHTMLContent(local.Content?.Value);
			if (local.IsStockItem?.Value == false)
				external.Type = local.RequireShipment?.Value == true ? ProductsType.Physical.ToEnumMemberAttrValue() : ProductsType.Digital.ToEnumMemberAttrValue();
			else
			{
				external.Type = ProductsType.Physical.ToEnumMemberAttrValue();
				external.BinPickingNumber = local.DefaultIssueLocationID?.Value;

			}
			external.Price = GetHelper<BCHelper>().RoundToStoreSetting(local.CurySpecificPrice?.Value);
			external.Weight = local.DimensionWeight.Value;
			external.CostPrice = local.CurrentStdCost.Value;
			external.RetailPrice = GetHelper<BCHelper>().RoundToStoreSetting(local.CurySpecificMSRP?.Value);
			external.Sku = local.InventoryID?.Value;
			external.TaxClassId = taxClasses?.Find(i => i.Name.Equals(GetHelper<BCHelper>().GetSubstituteLocalByExtern(GetBindingExt<BCBindingExt>().TaxCategorySubstitutionListID, local.TaxCategory?.Value, String.Empty)))?.Id;
		}

		public virtual void MapCustomFields(TemplateItems local, ProductData external)
		{
			external.PageTitle = local.PageTitle?.Value;
			external.MetaDescription = local.MetaDescription?.Value;
			external.MetaKeywords = local.MetaKeywords?.Value != null ? local.MetaKeywords?.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries) : null;
			external.SearchKeywords = local.SearchKeywords?.Value;
			//var vendor = impl.VendorDetails?.FirstOrDefault(v => v.Default?.Value == true);
			//if (vendor != null)
			//	data.GTIN = impl.CrossReferences?.FirstOrDefault(x => x.AlternateType?.Value == BCCaptions.VendorPartNumber && x.VendorOrCustomer?.Value == vendor.VendorID?.Value)?.AlternateID?.Value;
			//if (!string.IsNullOrWhiteSpace(impl.BaseUOM?.Value))
			//	data.MPN = impl.CrossReferences?.FirstOrDefault(x => x.AlternateType?.Value == BCCaptions.Barcode && x.UOM?.Value == impl.BaseUOM?.Value)?.AlternateID?.Value;
		}

		public virtual void MapVisibility(TemplateItems local, ProductData external)
		{
			string visibility = local?.Visibility?.Value;
			if (visibility == null || visibility == BCCaptions.StoreDefault) visibility = BCItemVisibility.Convert(GetBindingExt<BCBindingExt>().Visibility);
			switch (visibility)
			{
				case BCCaptions.Visible:
					{
						external.IsVisible = true;
						external.IsFeatured = false;
						break;
					}
				case BCCaptions.Featured:
					{
						external.IsVisible = true;
						external.IsFeatured = true;
						break;
					}
				case BCCaptions.Invisible:
				default:
					{
						external.IsFeatured = false;
						external.IsVisible = false;
						break;
					}
			}
		}

		public virtual void MapAvailability(BCProductWithVariantEntityBucket bucket, TemplateItems local, ProductData external, ProductData existingProduct)
		{
			string availability = local.Availability?.Value;

			if (availability == null || availability == BCCaptions.StoreDefault)
			{
				availability = BCItemAvailabilities.Convert(GetBindingExt<BCBindingExt>().Availability);
			}

			if (local.ItemStatus?.Value == PX.Objects.IN.Messages.Active || local.ItemStatus?.Value == PX.Objects.IN.Messages.NoPurchases || local.ItemStatus?.Value == PX.Objects.IN.Messages.NoRequest)
			{
				if (availability == BCCaptions.AvailableTrack)
				{
					external.Availability = BigCommerceConstants.AvailabilityAvailable;
					external.InventoryTracking = BigCommerceConstants.InventoryTrackingVariant;

					bool? positiveInventoryLevel = existingProduct?.InventoryLevel > 0;
					bool? purchasableVariants = external?.Variants.Any(v => v.PurchasingDisabled == false);

					if (existingProduct == null)
					{
						external.Availability = BigCommerceConstants.AvailabilityAvailable;
						external.InventoryTracking = BigCommerceConstants.InventoryTrackingVariant;
					}
					else if (positiveInventoryLevel == true && purchasableVariants == true)
					{
						external.Availability = BigCommerceConstants.AvailabilityAvailable;
						external.InventoryTracking = BigCommerceConstants.InventoryTrackingVariant;
					}
					else if (positiveInventoryLevel == false && purchasableVariants == true)
					{
						external.Availability = BigCommerceConstants.AvailabilityPreOrder;
						external.InventoryTracking = BigCommerceConstants.InventoryTrackingVariant;
					}
					else if (purchasableVariants == false)
					{
						external.Availability = BigCommerceConstants.AvailabilityDisabled;
						external.InventoryTracking = BigCommerceConstants.InventoryTrackingVariant;
					}
				}
				else if (availability == BCCaptions.AvailableSkip)
				{
					external.Availability = BigCommerceConstants.AvailabilityAvailable;
					external.InventoryTracking = BigCommerceConstants.InventoryTrackingNone;
				}
				else if (availability == BCCaptions.PreOrder)
				{
					external.Availability = BigCommerceConstants.AvailabilityPreOrder;
					external.InventoryTracking = BigCommerceConstants.InventoryTrackingNone;
				}
				else if (availability == BCCaptions.DisableItem)
				{
					external.Availability = BigCommerceConstants.AvailabilityDisabled;
					external.InventoryTracking = BigCommerceConstants.InventoryTrackingNone;
				}
				else if (availability == BCCaptions.DoNotUpdate)
				{
					external.Availability = existingProduct?.Availability ?? BigCommerceConstants.AvailabilityAvailable;
					external.InventoryTracking = existingProduct?.InventoryTracking ?? BigCommerceConstants.InventoryTrackingNone;
				}
			}
			else if (local.ItemStatus?.Value == PX.Objects.IN.Messages.Inactive || local.ItemStatus?.Value == PX.Objects.IN.Messages.NoSales || local.ItemStatus?.Value == PX.Objects.IN.Messages.ToDelete)
			{
				external.Availability = BigCommerceConstants.AvailabilityDisabled;
				external.InventoryTracking = BigCommerceConstants.InventoryTrackingNone;
			}
		}

		public virtual void MapProductOptions(TemplateItems local, ProductData externDataToBeExported, ProductData existingExternData)
		{
			if (local.AttributesDef?.Count > 0)
			{
                foreach (var def in local.AttributesDef)
                {
                    if (def.Values.Count > 0)
                    {
                        var descriptionDuplicated = def.Values.GroupBy(x => x.Description?.Value?.Trim()).Any(x => !string.IsNullOrEmpty(x.Key) && x.Count() > 1);

                        if (descriptionDuplicated) throw new PXException(BCMessages.AttributeDuplicateOptionDescription, def.AttributeID.Value);
                    }
                }

				foreach (var item in local.Matrix)
				{
					var def = local.AttributesValues.Where(x => x.NoteID.Value == item.Id).ToList();

					foreach (var attrValue in def)
					{
						if (attrValue.AttributeID.Value == null || attrValue.Value.Value == null) continue;

						var attribute = local.AttributesDef.FirstOrDefault(x => string.Equals(x.AttributeID.Value?.Trim(), attrValue.AttributeID.Value.Trim(), StringComparison.InvariantCultureIgnoreCase));
						if (attribute == null) continue;
						var value = attribute.Values.FirstOrDefault(y => string.Equals(y.ValueID.Value?.Trim(), attrValue.Value.Value.Trim(), StringComparison.InvariantCultureIgnoreCase));
						if (value == null) continue;

						ProductsOptionData productsOptionData = externDataToBeExported.ProductsOptionData.FirstOrDefault(x => x.LocalID == attribute.NoteID.Value);
						ProductsOptionData existingProductsOptionData = existingExternData?.ProductsOptionData.FirstOrDefault(x => x.DisplayName == attribute.Description?.Value);
						if (productsOptionData == null)
						{
							productsOptionData = new ProductsOptionData();
							productsOptionData.Name = attribute.AttributeID?.Value;
							productsOptionData.DisplayName = attribute.Description?.Value;
							productsOptionData.LocalID = attribute.NoteID?.Value;
							productsOptionData.Type = string.IsNullOrEmpty(existingProductsOptionData?.Type) ? BigCommerceOptionTypes.Dropdown : existingProductsOptionData?.Type;

							productsOptionData.Id = existingProductsOptionData?.Id;
							productsOptionData.SortOrder = attribute.Order?.Value.ToInt() ?? 0;
                            externDataToBeExported.ProductsOptionData.Add(productsOptionData);
						}
						if (!productsOptionData.OptionValues.Any(x => x.LocalID == value.NoteID.Value))
						{
							ProductOptionValueData productOptionValueData = new ProductOptionValueData();
							productOptionValueData.Label = value.Description?.Value ?? value.ValueID?.Value;
							productOptionValueData.LocalID = value.NoteID?.Value;
							productOptionValueData.SortOrder = value.SortOrder?.Value ?? 0;
							var existingOptionValue = existingProductsOptionData?.OptionValues.FirstOrDefault(x => x.Label == productOptionValueData.Label);
							productOptionValueData.Id = existingOptionValue?.Id;
							if (string.Equals(productsOptionData.Type, BigCommerceOptionTypes.Swatch, StringComparison.InvariantCultureIgnoreCase))
							{
								if (existingOptionValue != null)// if existing then copy the value data as is
								{
									productOptionValueData.ValueData = existingOptionValue.ValueData;
								}
								else
								{ //if new option value is added for example in case of color option where
								  //type swatch then we reset type to drop down as value data is manadatory
									productsOptionData.Type = BigCommerceOptionTypes.Dropdown;
								}
							}
							productsOptionData.OptionValues.Add(productOptionValueData);
						}

					}
				}
			}
		}

		public virtual void MapMetadata(TemplateItems local, ProductData external, ProductData existingData)
		{
			if (GetEntity(BCEntitiesAttribute.SalesCategory)?.IsActive == true)
			{
				if (external.Categories == null) external.Categories = new List<int>();

				foreach (PXResult<PX.Objects.IN.INCategory, PX.Objects.IN.INItemCategory, PX.Objects.IN.InventoryItem, BCSyncStatus> result in PXSelectJoin<PX.Objects.IN.INCategory,
					InnerJoin<PX.Objects.IN.INItemCategory, On<PX.Objects.IN.INItemCategory.categoryID, Equal<PX.Objects.IN.INCategory.categoryID>>,
					InnerJoin<PX.Objects.IN.InventoryItem, On<PX.Objects.IN.InventoryItem.inventoryID, Equal<PX.Objects.IN.INItemCategory.inventoryID>>,
					LeftJoin<BCSyncStatus, On<PX.Objects.IN.INCategory.noteID, Equal<BCSyncStatus.localID>>>>>,
					Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
						And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
						And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
						And<PX.Objects.IN.InventoryItem.noteID, Equal<Required<PX.Objects.IN.InventoryItem.noteID>>>>>>>
							.Select(this, BCEntitiesAttribute.SalesCategory, local.SyncID))
				{
					BCSyncStatus status = result.GetItem<BCSyncStatus>();

					if (status?.ExternID == null)
					{
						continue;
					}

					external.Categories.Add(status.ExternID.ToInt().Value);
				}
				if ((external.Categories ?? Enumerable.Empty<int>()).Empty_())
				{
					String categories = null;
					if (local.IsStockItem?.Value == false)
						categories = GetBindingExt<BCBindingExt>().NonStockSalesCategoriesIDs;
					else
						categories = GetBindingExt<BCBindingExt>().StockSalesCategoriesIDs;

					if (!String.IsNullOrEmpty(categories))
					{
						Int32?[] categoriesArray = categories.Split(',').Select(c => { return Int32.TryParse(c, out Int32 i) ? (int?)i : null; }).Where(i => i != null).ToArray();

						foreach (BCSyncStatus status in PXSelectJoin<BCSyncStatus,
							LeftJoin<PX.Objects.IN.INCategory, On<PX.Objects.IN.INCategory.noteID, Equal<BCSyncStatus.localID>>>,
							Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
								And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
								And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
								And<PX.Objects.IN.INCategory.categoryID, In<Required<PX.Objects.IN.INCategory.categoryID>>>>>>>
								.Select(this, BCEntitiesAttribute.SalesCategory, categoriesArray))
						{
							if (status?.ExternID == null)
							{
								continue;
							}

							external.Categories.Add(status.ExternID.ToInt().Value);
						}
					}
				}
			}
		}

		public virtual void MapProductVariants(BCProductWithVariantEntityBucket bucket, MappedTemplateItem existing)
		{
			var externDataToBeExported = bucket.Product.Extern;
			var local = bucket.Product.Local;
			var obj = bucket.Product;
			var existingExternData = existing?.Extern;
			var existingSyncDetails = bucket.Product.Details.ToList();

			var existingExternProductVariants = existing?.Extern?.Variants ?? new List<ProductsVariantData>();
			//delete inactive variants
			existingSyncDetails.RemoveAll(x => obj.Local.Matrix.All(y => x.EntityType == BCEntitiesAttribute.Variant && (x.LocalID != y.Id || !IsVariantActive(y))));

			var inventoryItems = SelectFrom<InventoryItem>
				.LeftJoin<InventoryItemCurySettings>
				.On<InventoryItemCurySettings.inventoryID.IsEqual<InventoryItem.inventoryID>>
				.LeftJoin<INItemXRef>
				.On<InventoryItem.inventoryID.IsEqual<INItemXRef.inventoryID>
					.And<INItemXRef.alternateType.IsEqual<INAlternateType.vPN>>
					.And<INItemXRef.bAccountID.IsEqual<InventoryItemCurySettings.preferredVendorID>
						.Or<INItemXRef.alternateType.IsEqual<INAlternateType.barcode>>>>
				.Where<InventoryItem.templateItemID.IsEqual<@P.AsInt>>
				.View.Select(this, local.InventoryItemID).AsEnumerable()
				.Cast<PXResult<InventoryItem>>()?.ToList();

			foreach (var localVariant in obj.Local.Matrix.Where(x => IsVariantActive(x)))
			{
				var existingId = existingSyncDetails?.FirstOrDefault(x => x.LocalID == localVariant.Id)?.ExternID?.ToInt();
				ProductsVariantData existingVariant = existingExternProductVariants?.FirstOrDefault(x => (existingId != null && string.Equals(existingId, x.Id?.ToString())) ||
					string.Equals(x.Sku?.Trim(), localVariant.InventoryID.Value?.Trim(), StringComparison.OrdinalIgnoreCase));
				existingId = existingVariant?.Id;

				List<PXResult<InventoryItem>> matchedInventoryItems = inventoryItems?.Where(x => x.GetItem<InventoryItem>().InventoryCD.Trim() == localVariant.InventoryID?.Value?.Trim()).ToList();
				InventoryItem matchedItem = matchedInventoryItems.FirstOrDefault()?.GetItem<InventoryItem>();

				ProductsVariantData externalVariant = new ProductsVariantData();

				MapVariantInventoryItem(local, externDataToBeExported, localVariant, externalVariant, existingId, matchedItem, matchedInventoryItems);
				MapVariantAvailability(local, localVariant, externalVariant, existingVariant, matchedItem);
				externDataToBeExported.Variants.Add(externalVariant);
			}

			// for all other variants, we need to determine whether we need to delete them in BigCommerce
			foreach (var variant in existingExternProductVariants.Where(existingExtern => !externDataToBeExported.Variants.Any(variantsToExport => variantsToExport.Id == existingExtern.Id)
																							&& existingExtern.Id != existingExternData?.BaseVariantId))
			{
				var matchMatrixItem = obj.Local.Matrix.FirstOrDefault(x => string.Equals(variant.Sku?.Trim(), x.InventoryID.Value?.Trim(), StringComparison.OrdinalIgnoreCase));

				// if there's no matrix item with a matching sku/inventoryID present in ERP then it means either the item has been deleted OR linked to another template item
				// in such case we need to delete such variants in BigCommerce
				if (matchMatrixItem == null)
					externDataToBeExported.VariantsToBeDeleted.Add(variant);
				// if there's a match matrix item and it is not an active variant, we export them but mark as not purchasable
				else if (!IsVariantActive(matchMatrixItem))
			{
				variant.PurchasingDisabled = true;
					externDataToBeExported.Variants.Add(variant);
				}
			}
		}

		public virtual void MapVariantInventoryItem(TemplateItems local, ProductData external, ProductItem localVariant, ProductsVariantData externalVariant, int? existingId, InventoryItem matchedItem, List<PXResult<InventoryItem>> matchedInventoryItems)
		{
			externalVariant.LocalID = localVariant.Id;
			externalVariant.ProductId = external.Id;
			if (existingId != null) externalVariant.Id = existingId;
			externalVariant.Sku = localVariant.InventoryID.Value;
			externalVariant.Price = GetHelper<BCHelper>().RoundToStoreSetting(localVariant.DefaultPrice?.Value);
			externalVariant.RetailPrice = GetHelper<BCHelper>().RoundToStoreSetting(localVariant.MSRP?.Value);
			externalVariant.Mpn = matchedInventoryItems?.FirstOrDefault(x => x.GetItem<INItemXRef>().AlternateType == INAlternateType.VPN)?.GetItem<INItemXRef>()?.AlternateID;
			if (!string.IsNullOrWhiteSpace(local.BaseUOM?.Value))
				externalVariant.Upc = (matchedInventoryItems?.FirstOrDefault(x => x.GetItem<INItemXRef>().AlternateType == INAlternateType.Barcode
							&& x.GetItem<INItemXRef>().UOM == local.BaseUOM.Value) ??
							matchedInventoryItems?.FirstOrDefault(x => x.GetItem<INItemXRef>().AlternateType == INAlternateType.Barcode
							&& string.IsNullOrEmpty(x.GetItem<INItemXRef>().UOM)))?.GetItem<INItemXRef>().AlternateID;
			externalVariant.Weight = (matchedItem.BaseItemWeight ?? 0) != 0 ? matchedItem.BaseItemWeight : local.DimensionWeight?.Value;
		}

		public virtual void MapVariantAvailability(TemplateItems parent, ProductItem local, ProductsVariantData external, ProductsVariantData existing, InventoryItem matchedInventoryItem)
		{
			string variantAvailability = BCItemAvailabilities.Convert(matchedInventoryItem.Availability);
			if (variantAvailability == null || variantAvailability == BCCaptions.StoreDefault)
			{
				variantAvailability = BCItemAvailabilities.Convert(GetBindingExt<BCBindingExt>().Availability);
			}

			string variantNotAvailable = BCItemAvailabilities.Convert(matchedInventoryItem.NotAvailMode);
			if (variantNotAvailable == null || variantAvailability == BCCaptions.StoreDefault)
			{
				variantNotAvailable = BCItemAvailabilities.Convert(GetBindingExt<BCBindingExt>().NotAvailMode);
			}

			string parentAvailability = parent?.Availability?.Value;
			if (parentAvailability == null || parentAvailability == BCCaptions.StoreDefault)
			{
				parentAvailability = BCItemAvailabilities.Convert(GetBindingExt<BCBindingExt>().Availability);
			}

			string variantStatus = local.ItemStatus?.Value;
			string parentStatus = parent.ItemStatus?.Value;
			if (parentStatus == PX.Objects.IN.Messages.Active || parentStatus == PX.Objects.IN.Messages.NoPurchases || parentStatus == PX.Objects.IN.Messages.NoRequest)
			{
				if (variantStatus == PX.Objects.IN.Messages.Active || variantStatus == PX.Objects.IN.Messages.NoPurchases || variantStatus == PX.Objects.IN.Messages.NoRequest)
				{
					if (variantAvailability == BCCaptions.AvailableTrack && parentAvailability == BCCaptions.AvailableTrack)
					{
						if (existing?.InventoryLevel > 0)
						{
							external.PurchasingDisabled = false;
						}
						else
						{
							external.PurchasingDisabled = false;
							if (variantNotAvailable == BCCaptions.DisableItem)
							{
								external.PurchasingDisabled = true;
							}
							else if (variantNotAvailable == BCCaptions.PreOrderItem || variantNotAvailable == BCCaptions.ContinueSellingItem || variantNotAvailable == BCCaptions.EnableSellingItem)
							{
								external.PurchasingDisabled = false;
							}
							else if (variantNotAvailable == BCCaptions.DoNothing || variantNotAvailable == BCCaptions.DoNotUpdate)
							{
								//If there is no existing product default to available
								external.PurchasingDisabled = existing?.PurchasingDisabled ?? false;
							}
						}
					}
					else if (variantAvailability == BCCaptions.AvailableSkip)
					{
						external.PurchasingDisabled = false;
					}
					else if (variantAvailability == BCCaptions.PreOrder)
					{
						external.PurchasingDisabled = false;
					}
					else if (variantAvailability == BCCaptions.DoNotUpdate)
					{
						external.PurchasingDisabled = existing?.PurchasingDisabled ?? false;
					}
					else if (variantAvailability == BCCaptions.Disabled)
					{
						external.PurchasingDisabled = true;
					}
					else
					{
						external.PurchasingDisabled = false;
					}
				}
				else if (variantStatus == PX.Objects.IN.Messages.Inactive || variantStatus == PX.Objects.IN.Messages.NoSales || variantStatus == PX.Objects.IN.Messages.ToDelete)
				{
					external.PurchasingDisabled = true;
				}
			}
			else if (parentStatus == PX.Objects.IN.Messages.Inactive || parentStatus == PX.Objects.IN.Messages.NoSales || parentStatus == PX.Objects.IN.Messages.ToDelete)
			{
				external.PurchasingDisabled = true;
			}
		}

		public virtual void MapVariantOptions(MappedTemplateItem obj, ProductItem item, ProductsVariantData variant)
		{
			variant.OptionValues = new List<ProductVariantOptionValueData>();
			var def = obj.Local.AttributesValues.Where(x => x.NoteID.Value == item.Id).ToList();
			foreach (var value in def)
			{
				ProductVariantOptionValueData optionValueData = new ProductVariantOptionValueData();
				var optionObj = obj.Local.AttributesDef.FirstOrDefault(x => x.AttributeID.Value == value.AttributeID.Value);
				if (optionObj == null) continue;

				var optionValueObj = optionObj.Values.FirstOrDefault(y => y.ValueID.Value == value.Value.Value);
				var detailObj = obj.Details.FirstOrDefault(x => x.LocalID == optionValueObj?.NoteID?.Value);
				if (detailObj == null) continue;

				optionValueData.OptionId = detailObj.ExternID.KeySplit(0).ToInt();
				optionValueData.Id = detailObj.ExternID.KeySplit(1).ToInt();
				variant.OptionValues.Add(optionValueData);
			}
		}

		public override object GetAttribute(BCProductWithVariantEntityBucket bucket, string attributeID)
		{
			MappedTemplateItem obj = bucket.Product;
			TemplateItems impl = obj.Local;
			return impl.Attributes?.Where(x => string.Equals(x?.AttributeDescription?.Value, attributeID, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

		}

		public override async Task SaveBucketExport(BCProductWithVariantEntityBucket bucket, IMappedEntity existing, string operation, CancellationToken cancellationToken = default)
		{
			MappedTemplateItem obj = bucket.Product;
			MappedTemplateItem existingMapped = existing as MappedTemplateItem;
			ProductData existingExternData = existing?.Extern as ProductData;
			ProductData externDataToBeExported = null;
			List<DetailInfo> existingSyncDetails = null;
			try
			{
				ValidateLinks(existing, obj);

				obj.Extern.CustomFieldsData = ExportCustomFields(obj, obj.Extern.CustomFields, externDataToBeExported, cancellationToken);

				//Save the variants to be synced separately after the parent product has been synced.
				var externVariantsToBeExported = obj.Extern.Variants;
				obj.Extern.Variants = null;

				if (obj.ExternID == null)
					externDataToBeExported = productDataProvider.Create(obj.Extern);
				else
					externDataToBeExported = productDataProvider.Update(obj.Extern, obj.ExternID);

				externDataToBeExported.Variants = externVariantsToBeExported;
				externDataToBeExported.VariantsToBeDeleted = obj.Extern.VariantsToBeDeleted;

				existingSyncDetails = new List<DetailInfo>(obj.Details);
				obj.ClearDetails();
				existingSyncDetails.Where(x => x.EntityType != BCEntitiesAttribute.ProductOptionValue && x.EntityType != BCEntitiesAttribute.ProductOption && x.EntityType != BCEntitiesAttribute.Variant)?
					.ToList()
					.ForEach(x => obj.AddDetail(x.EntityType, x.LocalID, x.ExternID));

				UpdateProductVariantOptions(obj, externDataToBeExported, existingSyncDetails, existingExternData);
				UpdateProductVariant(obj, externDataToBeExported, existingExternData);
			}
			catch
			{
				existingSyncDetails?.ForEach(x =>
				{
					if (!obj.Details.Any(y => y.LocalID == x.LocalID))
						obj.AddDetail(x.EntityType, x.LocalID, x.ExternID);
				});
				throw;
			}


			obj.AddExtern(externDataToBeExported, externDataToBeExported.Id?.ToString(), externDataToBeExported.Name, externDataToBeExported.DateModifiedUT.ToDate());

			SaveImages(obj, obj.Local.FileURLs, cancellationToken);
			SaveVideos(obj, obj.Local.FileURLs);
			UpdateStatus(obj, operation);
		}

		public virtual void ValidateLinks(IMappedEntity existing, MappedTemplateItem obj)
		{
			if (existing != null && (obj.Details == null || obj.Details?.Count() == 0))//only while linking to existing 
			{
				var existingProduct = existing.Extern as ProductData;
				if (existingProduct.ProductsOptionData.Count() != obj.Extern.ProductsOptionData.Count() || existingProduct.ProductsOptionData.Any(x => obj.Extern.ProductsOptionData.All(y => !string.Equals(y.DisplayName.Trim(), x.DisplayName?.Trim(), StringComparison.InvariantCultureIgnoreCase))))
				{
					throw new PXException(BigCommerceMessages.OptionsNotMatched, obj.ExternID);

				}
			}
		}

		public virtual void UpdateProductVariantOptions(MappedTemplateItem obj, ProductData data, List<DetailInfo> existingList, ProductData existing)
		{
			var existedProductOptionData = existing?.ProductsOptionData;
			//remove deleted attributes and values from BC
			var deletedOption = existingList.Where(x => obj.Extern.ProductsOptionData.All(y => x.LocalID != y.LocalID && x.EntityType == BCEntitiesAttribute.ProductOption)).ToList();
			if (deletedOption?.Count > 0)
			{
				foreach (var option in deletedOption)
				{
					//Check external ProductOptionData whether has data first
					if (existedProductOptionData?.Any(x => string.Equals(x.Id?.ToString(), option?.ExternID)) ?? false)
					{
						productsOptionRestDataProvider.Delete(option?.ExternID, data.Id.ToString());
					}
					existingList.RemoveAll(x => x.LocalID == option.LocalID);
				}
			}

			var allOptionValues = obj.Extern.ProductsOptionData.SelectMany(y => y.OptionValues);
			var deletedValues = existingList.Where(x => allOptionValues.All(y => x.LocalID != y.LocalID && x.EntityType == BCEntitiesAttribute.ProductOptionValue)).ToList();
			//Check external Option values, find all values are not in the push list
			var shouldDelExternalValues = existedProductOptionData?.Count > 0 ? existedProductOptionData.SelectMany(x => x.OptionValues).
				Where(o => allOptionValues.Any(v => (v.Id != null && v.Id == o.Id) || (v.Id == null && string.Equals(v.Label, o.Label, StringComparison.OrdinalIgnoreCase))) == false).ToList() : null;
			if (deletedValues?.Count > 0)
			{
				foreach (var value in deletedValues)
				{
					if (existedProductOptionData?.Any(x => string.Equals(x.Id?.ToString(), value?.ExternID?.KeySplit(0))
					&& x.OptionValues.Any(v => string.Equals(v.Id?.ToString(), value?.ExternID?.KeySplit(1)))) ?? false)
					{
						existingList.RemoveAll(x => x.LocalID == value.LocalID);
					}
				}
			}

			foreach (var option in obj.Extern.ProductsOptionData)
			{
				var localObj = obj.Local.AttributesDef.FirstOrDefault(x => x.NoteID?.Value == option.LocalID);
				var detailObj = existingList?.Where(x => x.LocalID == localObj?.NoteID?.Value)?.ToList();
				ProductsOptionData existingOption = null;
				var savedOptionID = detailObj?.FirstOrDefault()?.ExternID;
				if (existedProductOptionData != null)
				{
					existingOption = existedProductOptionData.FirstOrDefault(x => (savedOptionID != null && string.Equals(savedOptionID, x.Id?.ToString())) || string.Equals(x.DisplayName?.Trim(), option.DisplayName?.Trim(), StringComparison.OrdinalIgnoreCase));
				}

				var optionID = existingOption?.Id?.ToString();
				ProductsOptionData response = null;
				if (optionID != null)
				{
					response = productsOptionRestDataProvider.Update(option, optionID, data.Id.ToString());
					obj.AddDetail(BCEntitiesAttribute.ProductOption, localObj?.NoteID?.Value, optionID);
					foreach (var value in localObj.Values)
					{
						option.Id = optionID.ToInt();
						var optionValue = option.OptionValues.FirstOrDefault(x => x.LocalID == value.NoteID?.Value);
						if (optionValue == null) continue;
						var existingDetail = existingList.FirstOrDefault(x => x.LocalID == value.NoteID.Value);
						string optionValueID = existingDetail?.ExternID?.KeySplit(1);
						if (optionValueID == null)//check if there is existing non synced optionvalue at BC
							optionValueID = response?.OptionValues?.FirstOrDefault(x => string.Equals(x.Label?.Trim(), optionValue.Label?.Trim(), StringComparison.InvariantCultureIgnoreCase))?.Id?.ToString();
						if (optionValueID != null)
						{
							optionValue.Id = optionValueID.ToInt();
							productsOptionValueRestDataProvider.Update(optionValue, data.Id.ToString(), optionID, optionValueID);
						}
						else
						{
							// if option value not present try to create it one by one as update Option api does not add new option values
							var optionValueResponse = productsOptionValueRestDataProvider.Create(optionValue, data.Id.ToString(), optionID);
							if (optionValueResponse != null)
								obj.AddDetail(BCEntitiesAttribute.ProductOptionValue, value.NoteID?.Value, new object[] { optionID, optionValueResponse.Id.ToString() }.KeyCombine());
						}
					}
				}
				else
				{
					response = productsOptionRestDataProvider.Create(option, data.Id.ToString());
					if (response != null)
						obj.AddDetail(BCEntitiesAttribute.ProductOption, localObj?.NoteID?.Value, response.Id.ToString());

				}
				if (response != null)
				{
					foreach (var value in response.OptionValues)
					{
						var localId = localObj.Values.FirstOrDefault(x => string.Equals(x.Description?.Value, value.Label, StringComparison.InvariantCultureIgnoreCase) || string.Equals(x.ValueID?.Value, value.Label, StringComparison.InvariantCultureIgnoreCase))?.NoteID?.Value;
						obj.AddDetail(BCEntitiesAttribute.ProductOptionValue, localId, new object[] { response.Id.ToString(), value.Id.ToString() }.KeyCombine());
					}
				}
			}
		}

		public virtual void UpdateProductVariant(MappedTemplateItem obj, ProductData externDataToBeExported, ProductData existingExternData)
		{
			List<ProductsVariantData> variantsToBeExported = externDataToBeExported.Variants.ToList();
			List<ProductsVariantData> variantsToBeDeleted = externDataToBeExported.VariantsToBeDeleted.ToList();

			// delete variants that do not exist in ERP
			if (variantsToBeDeleted.Any())
			{
				foreach (var variant in variantsToBeDeleted)
				{
					if (variant.Id != null)
						productVariantRestDataProvider.Delete(variant.Id.ToString(), externDataToBeExported.Id.ToString());
				}
			}

			foreach (var item in variantsToBeExported)
			{
				item.ProductId = externDataToBeExported.Id;
				var localVariant = obj.Local.Matrix.FirstOrDefault(m => m.Id == item.LocalID);
				if (localVariant != null) MapVariantOptions(obj, localVariant, item);
			}

			productvariantBatchProvider.UpdateAll(variantsToBeExported, delegate (ItemProcessCallback<ProductsVariantData> callback)
			{
				ProductsVariantData request = variantsToBeExported[callback.Index];
				if (callback.IsSuccess)
				{
					ProductsVariantData productsVariantData = callback.Result;
					obj.AddDetail(BCEntitiesAttribute.Variant, request.LocalID, productsVariantData.Id.ToString());
				}
				else
				{
					throw callback.Error;
				}
			});
		}

		public virtual bool IsVariantActive(ProductItem item)
		{
			return !(item.ItemStatus?.Value == PX.Objects.IN.Messages.Inactive || item.ItemStatus?.Value == PX.Objects.IN.Messages.ToDelete || item.ItemStatus?.Value == PX.Objects.IN.Messages.NoSales)
				&& item.ExportToExternal?.Value == true;
		}

		/// <summary>
		/// Retrieves the Template item using the <see cref="Core.Model.ICBAPIService"/>.
		/// </summary>
		/// <param name="localID"></param>
		/// <returns>The fetched <see cref="TemplateItems"/>.</returns>
		public virtual TemplateItems GetTemplateItem(Guid? localID)
		{
			TemplateItems templateItem = cbapi.GetByID(localID, CreateEntityForGet(), GetCustomFieldsForExport());
			if (templateItem == null) return null;

			templateItem.Matrix = new List<ProductItem>();
			List<StockItem> stockMatrixItems = cbapi.GetAll(CreateProductItemFilter<StockItem>(templateItem.InventoryID.Value)).ToList();
			templateItem.Matrix.AddRange(stockMatrixItems);
			List<NonStockItem> nonStockMatrixItems = cbapi.GetAll(CreateProductItemFilter<NonStockItem>(templateItem.InventoryID.Value)).ToList();
			templateItem.Matrix.AddRange(nonStockMatrixItems);

			return templateItem;
		}

		/// <summary>
		/// Creates a <typeparamref name="ProductItemType"/> using <paramref name="inventoryID"/> as filter for <see cref="ProductItem.TemplateItemID"/>.
		/// </summary>
		/// <typeparam name="ProductItemType">Type of the filter to be created.</typeparam>
		/// <param name="inventoryID"></param>
		/// <returns>A new <typeparamref name="ProductItemType"/>.</returns>
		public virtual ProductItemType CreateProductItemFilter<ProductItemType>(string inventoryID) where ProductItemType : ProductItem, new()
		{
			return new ProductItemType()
			{
				TemplateItemID = new StringSearch() { Value = inventoryID },
				Attributes = new List<AttributeValue>(),
				Custom = GetCustomFieldsForExport(),
				ReturnBehavior = ReturnBehavior.All
			};
		}

		public virtual bool IsVariantPurchasable(ProductItem item, InventoryItem matchedItem)
		{
			return BCItemAvailabilities.Resolve(BCItemAvailabilities.Convert(matchedItem.Availability), GetBindingExt<BCBindingExt>().Availability) != BCCaptions.Disabled;
		}

		#endregion
	}
}
