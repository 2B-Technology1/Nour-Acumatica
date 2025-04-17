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
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Commerce.Objects;
using PX.Commerce.Shopify.API.REST;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PX.Commerce.Shopify
{
	public class SPTemplateItemEntityBucket : SPProductEntityBucket<MappedTemplateItem>
	{
		public new Dictionary<string, Tuple<long?, string, InventoryPolicy?>> VariantMappings = new Dictionary<string, Tuple<long?, string, InventoryPolicy?>>();
	}

	public class SPTemplateItemRestrictor : BCBaseRestrictor, IRestrictor
	{
		public virtual FilterResult RestrictExport(IProcessor processor, IMappedEntity mapped, FilterMode mode)
		{
			#region TemplateItemss
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
			#endregion
		}

		public virtual FilterResult RestrictImport(IProcessor processor, IMappedEntity mapped, FilterMode mode)
		{
			return null;
		}
	}

	[BCProcessor(typeof(SPConnector), BCEntitiesAttribute.ProductWithVariant, BCCaptions.TemplateItem, 50,
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
		URL = "products/{0}",
		Requires = new string[] { }
	)]
	[BCProcessorDetail(EntityType = BCEntitiesAttribute.ProductVideo, EntityName = BCCaptions.ProductVideo, AcumaticaType = typeof(BCInventoryFileUrls))]
	[BCProcessorDetail(EntityType = BCEntitiesAttribute.RelatedItem, EntityName = BCCaptions.RelatedItem, AcumaticaType = typeof(PX.Objects.IN.InventoryItem))]
	[BCProcessorDetail(EntityType = BCEntitiesAttribute.ProductOption, EntityName = BCCaptions.ProductOption, AcumaticaType = typeof(PX.Objects.CS.CSAttribute))]
	[BCProcessorDetail(EntityType = BCEntitiesAttribute.ProductOptionValue, EntityName = BCCaptions.ProductOption, AcumaticaType = typeof(PX.Objects.CS.CSAttributeDetail))]
	[BCProcessorDetail(EntityType = BCEntitiesAttribute.Variant, EntityName = BCCaptions.Variant, AcumaticaType = typeof(PX.Objects.IN.InventoryItem))]
	[BCProcessorRealtime(PushSupported = true, HookSupported = false,
		PushSources = new String[] { "BC-PUSH-Variants" }, PushDestination = BCConstants.PushNotificationDestination)]
	[BCProcessorExternCustomField(BCConstants.ShopifyMetaFields, ShopifyCaptions.Metafields, nameof(ProductData.Metafields), typeof(ProductData), writeAsCollection: false)]
	[BCProcessorExternCustomField(BCConstants.ShopifyMetaFields, ShopifyCaptions.Metafields, nameof(ProductVariantData.VariantMetafields), typeof(ProductVariantData), writeAsCollection: true)]
	[BCProcessorExternCustomField(BCAPICaptions.Matrix, BCAPICaptions.Matrix, nameof(TemplateItems.Matrix), typeof(TemplateItems), readAsCollection: true)]
	public class SPTemplateItemProcessor : SPProductProcessor<SPTemplateItemProcessor, SPTemplateItemEntityBucket, MappedTemplateItem, ProductData, TemplateItems>
	{
		#region Constructor
		public override async Task Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			await base.Initialise(iconnector, operation);
		}
		#endregion

		#region Common
		public override async Task<MappedTemplateItem> PullEntity(Guid? localID, Dictionary<string, object> externalInfo, CancellationToken cancellationToken = default)
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
		public override async Task<MappedTemplateItem> PullEntity(String externID, String externalInfo, CancellationToken cancellationToken = default)
		{
			ProductData data = productDataProvider.GetByID(externID);
			if (data == null) return null;

			MappedTemplateItem obj = new MappedTemplateItem(data, data.Id?.ToString(), data.Title, data.DateModifiedAt.ToDate(false));

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
		///<remarks><see cref="TemplateItems.Matrix"/> field are fetched separately.</remarks>
		/// <returns>The initialized entity</returns>
		protected override TemplateItems CreateEntityForGet()
		{
			TemplateItems entity = new TemplateItems();

			entity.ReturnBehavior = ReturnBehavior.OnlySpecified;
			entity.Attributes = new List<AttributeValue>() { new AttributeValue() };
			entity.Categories = new List<CategoryStockItem>() { new CategoryStockItem() };
			entity.FileURLs = new List<InventoryFileUrls>() { new InventoryFileUrls() };
			entity.VendorDetails = new List<ProductItemVendorDetail>() { new ProductItemVendorDetail() };
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
			return new MappedTemplateItem(entity, entity.SyncID, entity.SyncTime);
		}

		/// <inheritdoc/>
		public override object GetTargetObjectExport(object currentSourceObject, IExternEntity data, BCEntityExportMapping mapping)
		{
			ProductData productData = data as ProductData;
			ProductItem productItem = currentSourceObject as ProductItem;
			//Flag that indicates if we are trying to find a matching matrix to a variant.
			bool isMatrixToVariants = mapping.SourceObject.Contains(BCConstants.Matrix) && mapping.TargetObject.Contains(BCConstants.Variants);

			return (isMatrixToVariants) ?
				productData?.Variants?.FirstOrDefault(variant => variant.Sku == productItem.InventoryID.Value) :
				base.GetTargetObjectExport(currentSourceObject, data, mapping);
		}
		#endregion

		#region Import
		public override async Task FetchBucketsForImport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters, CancellationToken cancellationToken = default)
		{
			//No DateTime filtering for Category
			FilterProducts filter = new FilterProducts
			{
				UpdatedAtMin = minDateTime == null ? (DateTime?)null : minDateTime.Value.ToLocalTime().AddSeconds(-GetBindingExt<BCBindingShopify>().ApiDelaySeconds ?? 0),
				UpdatedAtMax = maxDateTime == null ? (DateTime?)null : maxDateTime.Value.ToLocalTime()
			};

			IEnumerable<ProductData> datas = productDataProvider.GetAll(filter, cancellationToken);
			if (datas?.Count() > 0)
			{
				foreach (ProductData data in datas)
				{
					SPTemplateItemEntityBucket bucket = CreateBucket();

					MappedTemplateItem obj = bucket.Product = bucket.Product.Set(data, data.Id?.ToString(), data.Title, data.DateModifiedAt.ToDate(false));
					EntityStatus status = EnsureStatus(obj, SyncDirection.Import);
					if (data.Variants?.Count > 0)
					{
						data.Variants.ForEach(x => { bucket.VariantMappings[x.Sku] = Tuple.Create(x.Id, x.InventoryManagement, x.InventoryPolicy); });
					}
				}
			}
		}
		public override async Task<EntityStatus> GetBucketForImport(SPTemplateItemEntityBucket bucket, BCSyncStatus syncstatus, CancellationToken cancellationToken = default)
		{
			ProductData data = productDataProvider.GetByID(syncstatus.ExternID, includedMetafields: true, cancellationToken);
			if (data == null) return EntityStatus.None;

			if (data.Variants?.Count > 0)
			{
				data.Variants.ForEach(x =>
				{
					if (bucket.VariantMappings.ContainsKey(x.Sku))
						bucket.VariantMappings[x.Sku] = Tuple.Create(x.Id, x.InventoryManagement, x.InventoryPolicy);
					else
						bucket.VariantMappings.Add(x.Sku, Tuple.Create(x.Id, x.InventoryManagement, x.InventoryPolicy));
				});
			}
			MappedTemplateItem obj = bucket.Product = bucket.Product.Set(data, data.Id?.ToString(), data.Title, data.DateModifiedAt.ToDate(false));
			EntityStatus status = EnsureStatus(obj, SyncDirection.Import);

			return status;
		}

		public override async Task MapBucketImport(SPTemplateItemEntityBucket bucket, IMappedEntity existing, CancellationToken cancellationToken = default)
		{

		}
		public override async Task SaveBucketImport(SPTemplateItemEntityBucket bucket, IMappedEntity existing, String operation, CancellationToken cancellationToken = default)
		{

		}
		#endregion

		#region Export

		public override async Task<PullSimilarResult<MappedTemplateItem>> PullSimilar(ILocalEntity entity, CancellationToken cancellationToken = default)
		{
			TemplateItems localEnity = (TemplateItems)entity;
			string uniqueField = localEnity?.InventoryID?.Value;
			IEnumerable<ProductData> datas = null;
			List<string> matrixIds = new List<string>();
			List<string> duplicateIds = new List<string>();
			if (localEnity?.Matrix?.Count > 0)
			{
				matrixIds = localEnity.Matrix.Select(x => x?.InventoryID?.Value).ToList();
				var filterQuery = string.Join(" OR ", matrixIds.Select(matrixId => $"sku:{matrixId}"));
				var existingItems = (await ProductGQLDataProvider.GetProductVariantsAsync(filterQuery, cancellationToken)).ToList();
				if (existingItems.Count > 0)
				{
					var matchedVariants = existingItems.Select(x => x.Product.IdNumber).Distinct().ToList();
					if (matchedVariants.Count > 0)
					{
						datas = productDataProvider.GetAll(new FilterProducts() { IDs = string.Join(",", matchedVariants) });

						// collect duplicate SKUs when there's more than one product share same SKU(s)
						if (matchedVariants.Count > 1)
							duplicateIds = existingItems.GroupBy(x => x.Sku).Where(x => x.Count() > 1).Select(x => x.Key).ToList();

						// further filtering using product's name (Title in Shopify and Description in ERP)
						// Idea: if there's at least one product in SP whose name matches with template item's name in ERP
						// then we only need to care about those SP products and ignore other products
						// If there is ONLY one product in SP that has same name => map that product with the template item
						// Otherwise, if there is NONE or more than one product in SP having same name => throw an error
						if (matchedVariants.Count > 1 && datas.Count() > 1 && datas.Any(x => string.Equals(x.Title.Trim(), localEnity.Description?.Value?.Trim(), StringComparison.OrdinalIgnoreCase)))
						{
							datas = datas.Where(x => string.Equals(x.Title.Trim(), localEnity.Description?.Value?.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

							// if there's more than one product having same name => include product name in the uniqueField variable
							// so that it can be displayed in the error message later
							if (datas.Count() > 1 && localEnity.Description?.Value != null)
								duplicateIds.Add(localEnity.Description?.Value);
						}
					}
				}
			}

			// if there's anything in duplicateIds then it will be more useful then the original inventory Id
			if (duplicateIds.Count > 0 && datas.Count() > 1)
				uniqueField = string.Join(", ", duplicateIds);

			return new PullSimilarResult<MappedTemplateItem>() { UniqueField = uniqueField, Entities = datas == null ? null : datas.Select(data => new MappedTemplateItem(data, data.Id.ToString(), data.Title, data.DateModifiedAt.ToDate(false))) };
		}

		public override async Task<EntityStatus> GetBucketForExport(SPTemplateItemEntityBucket bucket, BCSyncStatus syncstatus, CancellationToken cancellationToken = default)
		{
			TemplateItems impl = GetTemplateItem(syncstatus.LocalID);
			if (impl == null || impl.Matrix?.Count == 0) return EntityStatus.None;

			impl.AttributesDef = new List<AttributeDefinition>();
			impl.AttributesValues = new List<AttributeValue>();
			int? inventoryID = null;
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
				var attributeGroup = (CSAttributeGroup)attributeDef;
				def.AttributeID = attribute.AttributeID.ValueField();
				def.Description = attribute.Description.ValueField();
				def.NoteID = attribute.NoteID.ValueField();
				def.Order = attributeGroup.SortOrder.ValueField();

				def.Values = new List<AttributeDefinitionValue>();
				var attributedetails = PXSelect<CSAttributeDetail, Where<CSAttributeDetail.attributeID, Equal<Required<CSAttributeDetail.attributeID>>>>.Select(this, def.AttributeID.Value);
				foreach (CSAttributeDetail value in attributedetails)
				{
					AttributeDefinitionValue defValue = new AttributeDefinitionValue();
					defValue.NoteID = value.NoteID.ValueField();
					defValue.ValueID = value.ValueID.ValueField();
					defValue.Description = value.Description.ValueField();
					defValue.SortOrder = (value.SortOrder ?? 0).ToInt().ValueField();
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
				AttributeValue def = new AttributeValue
				{
					AttributeID = attribute.AttributeID.ValueField(),
					NoteID = inventory.NoteID.ValueField(),
					InventoryID = inventory.InventoryCD.ValueField(),
					Value = attribute.Value.ValueField(),
					IsActive = attribute.IsActive.ValueField()
				};
				impl.AttributesValues.Add(def);
			}
			impl.InventoryItemID = inventoryID;

			MappedTemplateItem obj = bucket.Product = bucket.Product.Set(impl, impl.SyncID, impl.SyncTime);
			EntityStatus status = EnsureStatus(obj, SyncDirection.Export);

			//Only calculates the active values and belongs to the variant category definition
			var activeVariantAttrQty = impl.AttributesValues.Where(a => (a.IsActive?.Value ?? false) && impl.AttributesDef.Any(ad => ad.AttributeID?.Value == a.AttributeID?.Value)).Select(a => a.AttributeID?.Value).Distinct().Count();
			if (activeVariantAttrQty > ShopifyConstants.ProductOptionsLimit)
			{
				throw new PXException(ShopifyMessages.ProductOptionsOutOfScope, activeVariantAttrQty, impl.InventoryID.Value, ShopifyConstants.ProductOptionsLimit);
			}
			if (impl.Matrix?.Count > 0)
			{
				var activeMatrixItems = impl.Matrix.Where(x => x?.ItemStatus?.Value == PX.Objects.IN.Messages.Active);
				if (activeMatrixItems.Count() == 0)
				{
					throw new PXException(BCMessages.NoMatrixCreated);
				}
				if (activeMatrixItems.Count() > ShopifyConstants.ProductVarantsLimit)
				{
					throw new PXException(ShopifyMessages.ProductVariantsOutOfScope, activeMatrixItems.Count(), impl.InventoryID.Value, ShopifyConstants.ProductVarantsLimit);
				}
				foreach (var category in activeMatrixItems)
				{
					if (!bucket.VariantMappings.ContainsKey(category.InventoryID?.Value))
						bucket.VariantMappings.Add(category.InventoryID?.Value, null);
				}
			}
			if (obj.Local.Categories != null)
			{
				foreach (CategoryStockItem category in obj.Local.Categories)
				{
					if (!SalesCategories.ContainsKey(category.CategoryID.Value.Value))
					{
						BCItemSalesCategory implCat = cbapi.Get<BCItemSalesCategory>(new BCItemSalesCategory() { CategoryID = new IntSearch() { Value = category.CategoryID.Value } });
						if (implCat == null) continue;
						if (category.CategoryID.Value != null)
						{
							SalesCategories[category.CategoryID.Value.Value] = implCat.Description.Value;
						}
					}
				}
			}
			return status;
		}

		public override async Task MapBucketExport(SPTemplateItemEntityBucket bucket, IMappedEntity existing, CancellationToken cancellationToken = default)
		{
			TemplateItems local = bucket.Product.Local;
			ProductData external = bucket.Product.Extern = new ProductData();

			MappedTemplateItem existingMapped = existing as MappedTemplateItem;
			ProductData existingData = existing?.Extern as ProductData;

			if (local.Matrix == null || local.Matrix?.Count == 0)
			{
				throw new PXException(BCMessages.NoMatrixCreated);
			}

			MapInventoryItem(local, external, existingData);

			MapMetadata(bucket, local, external, existingData);

			string visibility = local?.Visibility?.Value;
			if (visibility == null || visibility == BCCaptions.StoreDefault) visibility = BCItemVisibility.Convert(GetBindingExt<BCBindingExt>().Visibility);

			SetProductStatus(external, local.ItemStatus?.Value, local.Availability?.Value, visibility);

			MapProductOptions(local, external);

			MapProductVariants(bucket, existingMapped);
		}

		public virtual void MapInventoryItem(TemplateItems local, ProductData external, ProductData existingData)
		{
			external.Title = local.Description?.Value;
			external.BodyHTML = GetHelper<SPHelper>().ClearHTMLContent(local.Content?.Value);
			external.ProductType = SelectFrom<INItemClass>
				.Where<INItemClass.itemClassCD.IsEqual<@P.AsString>>
				.View.Select(this, local.ItemClass.Value).FirstOrDefault()?.GetItem<INItemClass>().Descr;
			external.Vendor = GetDefaultVendorName(local.VendorDetails);
			//Put all categories to the Tags later if CombineCategoriesToTags setting is true
			var categories = local.Categories?.Select(x => { if (SalesCategories.TryGetValue(x.CategoryID.Value.Value, out var desc)) return desc; else return string.Empty; }).Where(x => !string.IsNullOrEmpty(x)).ToList();
			if (categories != null && categories.Count > 0)
				external.Categories = categories;
			if (!string.IsNullOrEmpty(local.SearchKeywords?.Value))
				external.Tags = local.SearchKeywords?.Value;
			if (!string.IsNullOrEmpty(local.PageTitle?.Value))
				external.GlobalTitleTag = local.PageTitle?.Value;
			if (!string.IsNullOrEmpty(local.MetaDescription?.Value))
				external.GlobalDescriptionTag = local.MetaDescription?.Value;
		}

		public virtual void MapMetadata(SPTemplateItemEntityBucket bucket, TemplateItems local, ProductData external, ProductData existingData)
		{
			var categories = local.Categories?.Select(x => { if (SalesCategories.TryGetValue(x.CategoryID.Value.Value, out var desc)) return desc; else return string.Empty; }).Where(x => !string.IsNullOrEmpty(x)).ToList();
			if (categories != null && categories.Count > 0)
				external.Categories = categories;
			if (!string.IsNullOrEmpty(local.SearchKeywords?.Value))
				external.Tags = local.SearchKeywords?.Value;
			if (!string.IsNullOrEmpty(local.PageTitle?.Value))
				external.GlobalTitleTag = local.PageTitle?.Value;
			if (!string.IsNullOrEmpty(local.MetaDescription?.Value))
				external.GlobalDescriptionTag = local.MetaDescription?.Value;

			if (!string.IsNullOrEmpty(bucket.Product.ExternID))
			{
				external.Id = bucket.Product.ExternID.ToLong();
			}
			else
			{
				external.Metafields = new List<MetafieldData>() { new MetafieldData() { Key = ShopifyCaptions.Product, Value = local.Id.Value.ToString(), Type = ShopifyConstants.ValueType_SingleString, Namespace = BCObjectsConstants.Namespace_Global } };
				external.Metafields.Add(new MetafieldData() { Key = ShopifyCaptions.ProductId, Value = local.InventoryID.Value, Type = ShopifyConstants.ValueType_SingleString, Namespace = BCObjectsConstants.Namespace_Global });
				external.Metafields.Add(new MetafieldData() { Key = nameof(ProductTypes), Value = BCCaptions.TemplateItem, Type = ShopifyConstants.ValueType_SingleString, Namespace = BCObjectsConstants.Namespace_Global });
			}
		}

		public virtual void MapProductOptions(TemplateItems local, ProductData external)
		{
			if (local.AttributesDef?.Count > 0)
			{
				external.Options = new List<ProductOptionData>();
				int optionSortOrder = 1;
				//We only want attribute defs that have active matrix items.
				var activeAttributeValues = local.AttributesValues.Where(a => a.IsActive?.Value ?? false)
					.Select(a => a.AttributeID?.Value)
					.Distinct();
				var activeAttributeDefinitions = local.AttributesDef.Where(a => a.AttributeID?.Value.IsIn(activeAttributeValues) ?? false);
				//Shopify only allows maximum 3 options
				foreach (var attribute in activeAttributeDefinitions.OrderBy(x => x.Order?.Value ?? short.MaxValue).Take(ShopifyConstants.ProductOptionsLimit))
				{
					external.Options.Add(new ProductOptionData() { Name = attribute.Description?.Value, Position = optionSortOrder });
					optionSortOrder++;
				}
			}
		}

		public virtual void MapProductVariants(SPTemplateItemEntityBucket bucket, MappedTemplateItem existing)
		{
			TemplateItems local = bucket.Product.Local;
			ProductData external = bucket.Product.Extern;

			var existingData = existing?.Extern;

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
				.Cast<PXResult<InventoryItem, InventoryItemCurySettings, INItemXRef>>()?.ToList();

			external.Variants = new List<ProductVariantData>();
			foreach (var item in bucket.Product.Local.Matrix.Where(x => IsVariantActive(x)).Take(ShopifyConstants.ProductVarantsLimit))
			{
				var matchedInventoryItems = inventoryItems?
					.Where(x => x.GetItem<InventoryItem>().InventoryCD.Trim() == item.InventoryID?.Value?.Trim())
					.ToList();
				var matchedInventoryItem = matchedInventoryItems.FirstOrDefault()?.GetItem<InventoryItem>();

				ProductVariantData variant = new ProductVariantData();
				MapVariantInventoryItem(bucket, item, matchedInventoryItem, variant);
				MapVariantTaxability(local, variant);

				if (!string.IsNullOrWhiteSpace(bucket.Product.Local.BaseUOM?.Value))
				{
					variant.Barcode = (matchedInventoryItems?.FirstOrDefault(x => x.GetItem<INItemXRef>()?.AlternateType == INAlternateType.Barcode
								&& x.GetItem<INItemXRef>()?.UOM == bucket.Product.Local.BaseUOM.Value) ??
								matchedInventoryItems?.FirstOrDefault(x => x.GetItem<INItemXRef>()?.AlternateType == INAlternateType.Barcode
								&& string.IsNullOrEmpty(x.GetItem<INItemXRef>().UOM)))?.GetItem<INItemXRef>().AlternateID;
				}

				if (local.IsStockItem?.Value == true)
				{
					MapVariantAvailability(bucket, item, matchedInventoryItem, variant, existingData);
				}
				else
				{
					variant.InventoryManagement = null;
					variant.InventoryPolicy = InventoryPolicy.Deny;
					variant.RequiresShipping = local.RequireShipment.Value ?? false;
				}

				MapVariantMetadata(bucket, item, variant);

				external.Variants.Add(variant);
			}
			MapVariantPositions(local, external);
		}

		public virtual void MapVariantInventoryItem(SPTemplateItemEntityBucket bucket, ProductItem local, InventoryItem localInventoryItem, ProductVariantData external)
		{
			TemplateItems parent = bucket.Product.Local;
			decimal? price = local.DefaultPrice.Value;
			decimal? msrp = local.MSRP?.Value != null && local.MSRP?.Value > local.DefaultPrice.Value ? local.MSRP?.Value : 0;
			if (localInventoryItem.BaseUnit != localInventoryItem.SalesUnit)
			{
				CalculatePrices(localInventoryItem.InventoryCD.Trim(), ref msrp, ref price);
			}
			bool hasExternalVariantWithItemID = bucket.VariantMappings.ContainsKey(local.InventoryID?.Value) && bucket.VariantMappings[local.InventoryID?.Value] != null;
			external.LocalID = local.Id.Value;
			external.Id = hasExternalVariantWithItemID ? bucket.VariantMappings[local.InventoryID?.Value]?.Item1 : null;
			external.Title = local.Description?.Value ?? parent.Description?.Value;
			external.Price = price;
			external.Sku = local.InventoryID?.Value;
			external.OriginalPrice = msrp;
			external.Weight = (localInventoryItem?.BaseItemWeight ?? 0) != 0 ? localInventoryItem?.BaseItemWeight : parent.DimensionWeight?.Value;
			external.WeightUnit = (localInventoryItem?.WeightUOM ?? parent.WeightUOM?.Value)?.ToLower();
		}

		public virtual void MapVariantTaxability(TemplateItems local, ProductVariantData external)
		{
			bool isTaxable;
			bool.TryParse(GetHelper<SPHelper>().GetSubstituteLocalByExtern(GetBindingExt<BCBindingExt>().TaxCategorySubstitutionListID, local.TaxCategory?.Value, String.Empty), out isTaxable);
			external.Taxable = isTaxable;
		}

		public virtual void MapVariantAvailability(SPTemplateItemEntityBucket bucket, ProductItem local, InventoryItem localInventoryItem, ProductVariantData external, ProductData existingProduct)
		{
			string existingVariantID = bucket.Primary.Details.FirstOrDefault(d => d.EntityType == BCEntitiesAttribute.Variant && d.LocalID == local.Id)?.ExternID;
			ProductVariantData existingVariant = existingProduct?.Variants.FirstOrDefault(v => v?.Id.ToString() == existingVariantID);

			string availability = BCItemAvailabilities.Convert(localInventoryItem.Availability);

			if (availability == null || availability == BCCaptions.StoreDefault)
			{
				availability = BCItemAvailabilities.Convert(GetBindingExt<BCBindingExt>().Availability);
			}
			string notAvailable = BCItemAvailabilities.Convert(localInventoryItem.NotAvailMode);

			if (notAvailable == null || notAvailable == BCCaptions.StoreDefault)
			{
				notAvailable = BCItemNotAvailModes.Convert(GetBindingExt<BCBindingExt>().NotAvailMode);
			}

			if (local.ItemStatus?.Value == PX.Objects.IN.Messages.Active || local.ItemStatus?.Value == PX.Objects.IN.Messages.NoPurchases || local.ItemStatus?.Value == PX.Objects.IN.Messages.NoRequest)
			{
				if (availability == BCCaptions.AvailableTrack)
				{
					external.InventoryManagement = ShopifyConstants.InventoryManagement_Shopify;

					if (notAvailable == BCCaptions.DoNothing || notAvailable == BCCaptions.DoNotUpdate)
					{
						external.InventoryPolicy = existingVariant?.InventoryPolicy ?? InventoryPolicy.Continue;
					}
					else if (notAvailable == BCCaptions.DisableItem)
					{
						external.InventoryPolicy = InventoryPolicy.Deny;
					}
					else if (notAvailable == BCCaptions.PreOrderItem || notAvailable == BCCaptions.ContinueSellingItem || notAvailable == BCCaptions.EnableSellingItem)
					{
						external.InventoryPolicy = InventoryPolicy.Continue;
					}
				}
				else if (availability == BCCaptions.DoNotUpdate)
				{
					// Value can be null or 'shopify' but if the external item does not exist, we need to go along with the SP standard behaviour - set 'shopify'
					external.InventoryManagement = existingProduct is null ? ShopifyConstants.InventoryManagement_Shopify : existingProduct.Variants[0]?.InventoryManagement;
					external.InventoryPolicy = existingVariant?.InventoryPolicy ?? InventoryPolicy.Deny;
				}
			}
			else if (local.ItemStatus?.Value == PX.Objects.IN.Messages.Inactive || local.ItemStatus?.Value == PX.Objects.IN.Messages.NoSales || local.ItemStatus?.Value == PX.Objects.IN.Messages.ToDelete)
			{
				external.InventoryManagement = null;
				external.InventoryPolicy = InventoryPolicy.Deny;
			}
		}

		public virtual void MapVariantMetadata(SPTemplateItemEntityBucket bucket, ProductItem local, ProductVariantData external)
		{
			ProductData externalParent = bucket.Product.Extern;
			var def = bucket.Product.Local.AttributesValues.Where(x => x.NoteID.Value == local.Id).ToList();
			foreach (var attrItem in def)
			{
				var optionObj = bucket.Product.Local.AttributesDef.FirstOrDefault(x => x.AttributeID.Value == attrItem.AttributeID.Value);
				if (optionObj != null)
				{

					var option = externalParent.Options.FirstOrDefault(x => optionObj != null && x.Name == optionObj.Description?.Value);
					if (option == null) continue;
					var attrValue = optionObj.Values.FirstOrDefault(x => x.ValueID?.Value == attrItem?.Value.Value);

					switch (option.Position)
					{
						case 1:
							{
								external.Option1 = attrValue?.Description?.Value;
								external.OptionSortOrder1 = attrValue.SortOrder.Value.Value;
								break;
							}
						case 2:
							{
								external.Option2 = attrValue?.Description?.Value;
								external.OptionSortOrder2 = attrValue.SortOrder.Value.Value;
								break;
							}
						case 3:
							{
								external.Option3 = attrValue?.Description?.Value;
								external.OptionSortOrder3 = attrValue.SortOrder.Value.Value;
								break;
							}
						default:
							break;
					}
				}
			}
			if (external.Id == null || external.Id == 0)
				external.VariantMetafields = new List<MetafieldData>() { new MetafieldData() { Key = ShopifyConstants.Variant, Value = local.Id.Value.ToString(), Type = ShopifyConstants.ValueType_SingleString, Namespace = BCObjectsConstants.Namespace_Global } };
		}

		public virtual void MapVariantPositions(TemplateItems local, ProductData external)
		{
			if (external.Variants?.Count > 0)
			{
				external.Variants = external.Variants.OrderBy(x => x.OptionSortOrder1).ThenBy(x => x.OptionSortOrder2).ThenBy(x => x.OptionSortOrder3).ToList();
			}
			else throw new PXException(ShopifyMessages.NoProductVariants, local.InventoryID.Value);
		}

		public override object GetAttribute(SPTemplateItemEntityBucket bucket, string attributeID)
		{
			MappedTemplateItem obj = bucket.Product;
			TemplateItems impl = obj.Local;
			return impl.Attributes?.Where(x => string.Equals(x?.AttributeDescription?.Value, attributeID, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

		}

		public override async Task SaveBucketExport(SPTemplateItemEntityBucket bucket, IMappedEntity existing, String operation, CancellationToken cancellationToken = default)
		{
			MappedTemplateItem obj = bucket.Product;
			ProductData data = null;
			if (obj.Extern.Categories?.Count > 0 && GetBindingExt<BCBindingShopify>()?.CombineCategoriesToTags == BCSalesCategoriesExportAttribute.SyncToProductTags)
			{
				obj.Extern.Tags = string.Join(",", obj.Extern.Categories) + (string.IsNullOrEmpty(obj.Extern.Tags) ? "" : $",{obj.Extern.Tags}");
			}
			try
			{
				if (obj.ExternID == null)
					data = productDataProvider.Create(obj.Extern);
				else
				{
					var skus = obj.Extern.Variants.Select(x => x.Sku).ToList();
					var notExistedVariantIds = bucket.VariantMappings.Where(x => !skus.Contains(x.Key)).Select(x => x.Value?.Item1)?.ToList();
					if (notExistedVariantIds?.Count > 0)
					{
						notExistedVariantIds.ForEach(x =>
						{
							if (x != null) productVariantDataProvider.Delete(obj.ExternID, x.ToString());
						});
					}
					data = productDataProvider.Update(obj.Extern, obj.ExternID);
				}
			}
			catch (Exception ex)
			{
				throw new PXException(ex.Message);
			}

			obj.ClearDetails(BCEntitiesAttribute.Variant);
			obj.AddExtern(data, data.Id?.ToString(), data.Title, data.DateModifiedAt.ToDate(false));
			if (data.Variants?.Count > 0)
			{
				var localVariants = obj.Local.Matrix;
				foreach (var externVariant in data.Variants)
				{
					var matchItem = localVariants.FirstOrDefault(x => x.InventoryID?.Value == externVariant.Sku);
					if (matchItem != null)
					{
						obj.AddDetail(BCEntitiesAttribute.Variant, matchItem.Id.Value, externVariant.Id.ToString());
					}
				}
			}

			SaveImages(obj, obj.Local?.FileURLs, cancellationToken);

			UpdateStatus(obj, operation);
		}
		#endregion

		public virtual bool IsVariantActive(ProductItem item)
		{
			return !(item.ItemStatus?.Value == PX.Objects.IN.Messages.Inactive || item.ItemStatus?.Value == PX.Objects.IN.Messages.ToDelete || item.ItemStatus?.Value == PX.Objects.IN.Messages.NoSales)
				&& item.ExportToExternal.Value == true;
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
		
		/// <summary>
		/// Iterates through <paramref name="listOfVendors"/> and returns the default vendor name or the first one. 
		/// </summary>
		/// <param name="listOfVendors"></param>
		/// <returns>Return the default vendor name or the first one, if exists.</returns>
		public virtual string GetDefaultVendorName(IEnumerable<ProductItemVendorDetail> listOfVendors)
		{
			if (listOfVendors == null) return null;
			ProductItemVendorDetail vendorDetail = listOfVendors.FirstOrDefault(vendor => vendor.Default?.Value == true) ?? listOfVendors.FirstOrDefault();
			return vendorDetail?.VendorName?.Value;
		}
	}
}
