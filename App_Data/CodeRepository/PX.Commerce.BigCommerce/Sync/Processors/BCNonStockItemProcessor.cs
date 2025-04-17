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
using PX.Data;
using PX.Objects.Common;
using Serilog.Context;
using PX.Objects.IN.RelatedItems;
using System;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing.Text;

namespace PX.Commerce.BigCommerce
{
	public class BCNonStockItemEntityBucket : BCProductEntityBucket<MappedNonStockItem>
	{
		public override IMappedEntity[] PreProcessors { get => Categories.ToArray(); }
	}

	public class BCNonStockItemRestrictor : BCBaseRestrictor, IRestrictor
	{
		public virtual FilterResult RestrictExport(IProcessor processor, IMappedEntity mapped, FilterMode mode)
		{
			#region NonStockItems
			return base.Restrict<MappedNonStockItem>(mapped, delegate (MappedNonStockItem obj)
			{
				BCBindingExt bindingExt = processor.GetBindingExt<BCBindingExt>();

				if (obj.Local != null && obj.Local.TemplateItemID?.Value != null)
				{
					return new FilterResult(FilterStatus.Invalid,
						PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogNonStockSkippedVariant, obj.Local.InventoryID?.Value ?? obj.Local.SyncID.ToString()));
				}

				if (obj.Local != null && obj.Local.ExportToExternal?.Value == false)
				{
					return new FilterResult(FilterStatus.Invalid,
						PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogItemNoExport, obj.Local.InventoryID?.Value ?? obj.Local.SyncID.ToString()));
				}

				if (bindingExt.GiftCertificateItemID != null && obj.Local?.InventoryID?.Value != null)
				{
					PX.Objects.IN.InventoryItem giftCertificate = bindingExt.GiftCertificateItemID != null ? PX.Objects.IN.InventoryItem.PK.Find((PXGraph)processor, bindingExt.GiftCertificateItemID) : null;
					if (giftCertificate != null && obj.Local?.InventoryID?.Value.Trim() == giftCertificate?.InventoryCD?.Trim())
					{
						return new FilterResult(FilterStatus.Invalid,
							PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogNonStockSkippedGift, obj.Local.InventoryID?.Value ?? obj.Local.SyncID.ToString()));
					}
				}

				if (bindingExt.GiftWrappingItemID != null && obj.Local?.InventoryID?.Value != null)
				{
					PX.Objects.IN.InventoryItem giftWrapItem = bindingExt.GiftWrappingItemID != null ? PX.Objects.IN.InventoryItem.PK.Find((PXGraph)processor, bindingExt.GiftWrappingItemID) : null;
					if (giftWrapItem != null && obj.Local?.InventoryID?.Value.Trim() == giftWrapItem?.InventoryCD?.Trim())
					{
						return new FilterResult(FilterStatus.Invalid,
							PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogNonStockSkippedGiftWrapItem, obj.Local.InventoryID?.Value ?? obj.Local.SyncID.ToString()));
					}
				}

				if (bindingExt.RefundAmountItemID != null && obj.Local?.InventoryID?.Value != null)
				{
					PX.Objects.IN.InventoryItem refundItem = bindingExt.RefundAmountItemID != null ? PX.Objects.IN.InventoryItem.PK.Find((PXGraph)processor, bindingExt.RefundAmountItemID) : null;
					if (refundItem != null && obj.Local?.InventoryID?.Value.Trim() == refundItem?.InventoryCD?.Trim())
					{
						return new FilterResult(FilterStatus.Invalid,
							PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogNonStockSkippedRefund, obj.Local.InventoryID?.Value ?? obj.Local.SyncID.ToString()));
					}
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

	[BCProcessor(typeof(BCConnector), BCEntitiesAttribute.NonStockItem, BCCaptions.NonStockItem, 60,
		IsInternal = false,
		Direction = SyncDirection.Export,
		PrimaryDirection = SyncDirection.Export,
		PrimarySystem = PrimarySystem.Local,
		PrimaryGraph = typeof(PX.Objects.IN.NonStockItemMaint),
		ExternTypes = new Type[] { typeof(ProductData) },
		LocalTypes = new Type[] { typeof(NonStockItem) },
		AcumaticaPrimaryType = typeof(PX.Objects.IN.InventoryItem),
		AcumaticaPrimarySelect = typeof(Search<PX.Objects.IN.InventoryItem.inventoryCD, Where<PX.Objects.IN.InventoryItem.stkItem, Equal<False>>>),
		URL = "products/{0}/edit",
		Requires = new string[] { }
	)]
	[BCProcessorDetail(EntityType = BCEntitiesAttribute.ProductVideo, EntityName = BCCaptions.ProductVideo, AcumaticaType = typeof(BCInventoryFileUrls))]
	[BCProcessorDetail(EntityType = BCEntitiesAttribute.RelatedItem, EntityName = BCCaptions.RelatedItem, AcumaticaType = typeof(PX.Objects.IN.InventoryItem))]
	[BCProcessorRealtime(PushSupported = true, HookSupported = false,
		PushSources = new String[] { "BC-PUSH-NonStocks" }, PushDestination = BCConstants.PushNotificationDestination,
		WebHookType = typeof(WebHookProduct),
		WebHooks = new String[]
		{
			"store/product/created",
			"store/product/updated",
			"store/product/deleted"
		})]
	[BCProcessorExternCustomField(BCConstants.CustomFields, BigCommerceCaptions.CustomFields, nameof(ProductData.CustomFields), typeof(ProductData))]
	public class BCNonStockItemProcessor : BCProductProcessor<BCNonStockItemProcessor, BCNonStockItemEntityBucket, MappedNonStockItem, ProductData, NonStockItem>
	{
		#region Constructor
		public override async Task Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			await base.Initialise(iconnector, operation);
		}
		#endregion

		#region Common
		public override async Task<MappedNonStockItem> PullEntity(Guid? localID, Dictionary<string, object> externalInfo, CancellationToken cancellationToken = default)
		{
			NonStockItem impl = cbapi.GetByID(localID,
				new NonStockItem()
				{
					ReturnBehavior = ReturnBehavior.OnlySpecified,
					Attributes = new List<AttributeValue>() { new AttributeValue() },
					Categories = new List<CategoryStockItem>() { new CategoryStockItem() },
					CrossReferences = new List<InventoryItemCrossReference>() { new InventoryItemCrossReference() },
					VendorDetails = new List<ProductItemVendorDetail>() { new ProductItemVendorDetail() },
					FileUrls = new List<InventoryFileUrls>() { new InventoryFileUrls() },

				});
			if (impl == null) return null;

			MappedNonStockItem obj = new MappedNonStockItem(impl, impl.SyncID, impl.SyncTime);

			return obj;
		}
		public override async Task<MappedNonStockItem> PullEntity(String externID, String externalInfo, CancellationToken cancellationToken = default)
		{
			ProductData data = productDataProvider.GetByID(externID);
			if (data == null) return null;

			MappedNonStockItem obj = new MappedNonStockItem(data, data.Id?.ToString(), data.Name, data.DateModifiedUT.ToDate());

			return obj;
		}

		/// <summary>
		/// Initialize a new object of the entity to be used to Fetch bucket
		/// </summary>
		/// <returns>The initialized entity</returns>
		protected override NonStockItem CreateEntityForFetch()
		{
			NonStockItem entity = new NonStockItem();

			entity.InventoryID = new StringReturn();
			entity.TemplateItemID = new StringReturn();
			entity.Categories = new List<CategoryStockItem>() { new CategoryStockItem() { CategoryID = new IntReturn() } };
			entity.ExportToExternal = new BooleanReturn();

			return entity;
		}

		/// <summary>
		/// Initialize a new object of the entity to be used to Get bucket
		/// </summary>
		/// <returns>The initialized entity</returns>
		protected override NonStockItem CreateEntityForGet()
		{
			NonStockItem entity = new NonStockItem();

			entity.ReturnBehavior = ReturnBehavior.OnlySpecified;
			entity.Attributes = new List<AttributeValue>() { new AttributeValue() };
			entity.Categories = new List<CategoryStockItem>() { new CategoryStockItem() };
			entity.CrossReferences = new List<InventoryItemCrossReference>() { new InventoryItemCrossReference() };
			entity.VendorDetails = new List<ProductItemVendorDetail>() { new ProductItemVendorDetail() };
			entity.FileUrls = new List<InventoryFileUrls>() { new InventoryFileUrls() };

			return entity;
		}

		/// <summary>
		/// Creates a mapped entity for the passed entity
		/// </summary>
		/// <param name="entity">The entity to create the mapped entity from</param>
		/// <param name="syncId">The sync id of the entity</param>
		/// <param name="syncTime">The timestamp of the last modification</param>
		/// <returns>The mapped entity</returns>
		protected override MappedNonStockItem CreateMappedEntity(NonStockItem entity, Guid? syncId, DateTime? syncTime)
		{
			return new MappedNonStockItem(entity, syncId, syncTime);
		}

		#endregion

		#region Import
		public override async Task FetchBucketsForImport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters, CancellationToken cancellationToken = default)
		{
			FilterProducts filter = new FilterProducts
			{
				Type = ProductTypes.digital.ToString(),
				MinDateModified = minDateTime == null ? null : minDateTime,
				MaxDateModified = maxDateTime == null ? null : maxDateTime
			};

			IEnumerable<ProductData> datas = productDataProvider.GetAll(filter, cancellationToken);

			foreach (ProductData data in datas)
			{
				BCNonStockItemEntityBucket bucket = CreateBucket();

				MappedNonStockItem obj = bucket.Product = bucket.Product.Set(data, data.Id?.ToString(), data.Name, data.DateModifiedUT.ToDate());
				EntityStatus status = EnsureStatus(obj, SyncDirection.Import);
			}
		}
		public override async Task<EntityStatus> GetBucketForImport(BCNonStockItemEntityBucket bucket, BCSyncStatus syncstatus, CancellationToken cancellationToken = default)
		{
			FilterProducts filter = new FilterProducts { Include = "images,modifiers" };
			ProductData data = productDataProvider.GetByID(syncstatus.ExternID, filter);
			if (data == null) return EntityStatus.None;

			MappedNonStockItem obj = bucket.Product = bucket.Product.Set(data, data.Id?.ToString(), data.Name, data.DateModifiedUT.ToDate());
			EntityStatus status = EnsureStatus(obj, SyncDirection.Import);

			return status;
		}

		public override async Task MapBucketImport(BCNonStockItemEntityBucket bucket, IMappedEntity existing, CancellationToken cancellationToken = default)
		{
			MappedNonStockItem obj = bucket.Product;

			ProductData data = obj.Extern;
			NonStockItem impl = obj.Local = new NonStockItem();
			StringValue tax = obj.Extern?.TaxClassId != null ? GetHelper<BCHelper>().GetSubstituteLocalByExtern(
					BCSubstitute.GetValue(Operation.ConnectorType, BCSubstitute.TaxClasses),
					taxClasses?.Find(i => i.Id == obj.Extern?.TaxClassId)?.Name, "").ValueField() :
					obj.Local.TaxCategory;

			impl.Custom = GetCustomFieldsForImport();

			//Product
			impl.InventoryID = GetEntityKey(PX.Objects.IN.InventoryAttribute.DimensionName, data.Name).ValueField();
			impl.Description = data.Name.ValueField();
			impl.ItemClass = obj.LocalID == null || existing?.Local == null ? PX.Objects.IN.INItemClass.PK.Find(this, GetBindingExt<BCBindingExt>().NonStockItemClassID)?.ItemClassCD.ValueField() : null;
			impl.CurySpecificPrice = data.Price.ValueField();
			impl.TaxCategory = tax;

			if (GetEntity(BCEntitiesAttribute.SalesCategory)?.IsActive == true)
			{
				if (data.Categories != null) impl.Categories = new List<CategoryStockItem>();
				foreach (int cat in data.Categories ?? new List<int>())
				{
					PX.Objects.IN.INCategory incategory = PXSelectJoin<PX.Objects.IN.INCategory,
					LeftJoin<BCSyncStatus, On<PX.Objects.IN.INCategory.noteID, Equal<BCSyncStatus.localID>>>,
					Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
						And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
						And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
						And<BCSyncStatus.externID, Equal<Required<BCSyncStatus.externID>>>>>>>.Select(this, BCEntitiesAttribute.SalesCategory, cat);

					if (incategory == null || incategory.CategoryID == null) throw new PXException(BCMessages.CategoryNotSyncronizedForItem, data.Name);

					impl.Categories.Add(new CategoryStockItem() { CategoryID = incategory.CategoryID.ValueField() });
				}
			}
		}
		public override async Task SaveBucketImport(BCNonStockItemEntityBucket bucket, IMappedEntity existing, String operation, CancellationToken cancellationToken = default)
		{
			MappedNonStockItem obj = bucket.Product;

			if (existing?.Local != null) obj.Local.InventoryID = ((NonStockItem)existing.Local).InventoryID.Value.SearchField();

			NonStockItem impl = cbapi.Put<NonStockItem>(obj.Local, obj.LocalID);

			bucket.Product.AddLocal(impl, impl.SyncID, impl.SyncTime);
			UpdateStatus(obj, operation);
		}
		#endregion

		#region Export
		public override async Task<PullSimilarResult<MappedNonStockItem>> PullSimilar(ILocalEntity entity, CancellationToken cancellationToken = default)
		{
			string uniqueField;
			List<ProductData> datas = PullSimilar(((NonStockItem)entity)?.Description?.Value, ((NonStockItem)entity)?.InventoryID?.Value, out uniqueField, cancellationToken);
			return new PullSimilarResult<MappedNonStockItem>() { UniqueField = uniqueField, Entities = datas == null ? null : datas.Select(data => new MappedNonStockItem(data, data.Id.ToString(), data.Name, data.DateModifiedUT.ToDate())) };
		}

		public override async Task MapBucketExport(BCNonStockItemEntityBucket bucket, IMappedEntity existing, CancellationToken cancellationToken = default)
		{
			NonStockItem local = bucket.Product.Local;
			ProductData external = bucket.Product.Extern = new ProductData();

			MappedNonStockItem existingMapped = existing as MappedNonStockItem;
			ProductData existingData = existingMapped?.Extern;

			MapInventoryItem(bucket, local, external, existingData);
			MapCustomFields(local, external);
			external.RelatedProducts = MapRelatedItems(bucket.Product);
			MapVisibility(local, external);
			MapAvailability(local, external, existingData);
			MapCategories(local, external);
		}

		public virtual void MapInventoryItem(BCNonStockItemEntityBucket bucket, NonStockItem local, ProductData external, ProductData existingData)
		{
			external.Name = local.Description?.Value;
			external.Description = GetHelper<BCHelper>().ClearHTMLContent(local.Content?.Value);
			external.Type = local.RequireShipment?.Value == true ? ProductsType.Physical.ToEnumMemberAttrValue() : ProductsType.Digital.ToEnumMemberAttrValue();
			external.Price = GetHelper<BCHelper>().RoundToStoreSetting(local.CurySpecificPrice?.Value);
			external.Weight = local.DimensionWeight.Value;
			external.CostPrice = local.CurrentStdCost.Value;
			external.RetailPrice = GetHelper<BCHelper>().RoundToStoreSetting(local.CurySpecificMSRP?.Value);
			external.Sku = local.InventoryID?.Value;
			external.TaxClassId = taxClasses?.Find(i => string.Equals(i.Name, GetHelper<BCHelper>().GetSubstituteLocalByExtern(GetBindingExt<BCBindingExt>().TaxCategorySubstitutionListID, local.TaxCategory?.Value, String.Empty)))?.Id;
		}

		public virtual void MapCustomFields(NonStockItem local, ProductData external)
		{
			external.PageTitle = local.PageTitle?.Value;
			external.MetaDescription = local.MetaDescription?.Value;
			external.MetaKeywords = local.MetaKeywords?.Value != null ? local.MetaKeywords?.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries) : null;
			external.SearchKeywords = local.SearchKeywords?.Value;
			var vendor = local.VendorDetails?.FirstOrDefault(v => v.Default?.Value == true);
			if (vendor != null)
				external.MPN = local.CrossReferences?.FirstOrDefault(x => x.AlternateType?.Value == BCCaptions.VendorPartNumber && x.VendorOrCustomer?.Value == vendor.VendorID?.Value)?.AlternateID?.Value;
			if (local.CustomURL?.Value != null) external.CustomUrl = new ProductCustomUrl() { Url = local.CustomURL?.Value, IsCustomized = true };

			external.Upc = GetCrossReferenceValue(local, local.SalesUnit, PX.Objects.IN.Messages.Barcode) ?? GetCrossReferenceValue(local, local.BaseUnit, PX.Objects.IN.Messages.Barcode) ?? string.Empty;
			external.GTIN = GetCrossReferenceValue(local, local.SalesUnit, PX.Objects.IN.Messages.GIN) ?? GetCrossReferenceValue(local, local.BaseUnit, PX.Objects.IN.Messages.GIN) ?? string.Empty;
		}

		public virtual void MapVisibility(NonStockItem local, ProductData external)
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

		public virtual void MapAvailability(NonStockItem local, ProductData external, ProductData existingProduct)
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
					external.InventoryTracking = BigCommerceConstants.InventoryTrackingProduct;
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

		private void MapCategories(NonStockItem local, ProductData external)
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
					String categories = GetBindingExt<BCBindingExt>().NonStockSalesCategoriesIDs;
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

		public override object GetAttribute(BCNonStockItemEntityBucket bucket, string attributeID)
		{
			MappedNonStockItem obj = bucket.Product;
			NonStockItem impl = obj.Local;
			return impl.Attributes?.Where(x => string.Equals(x?.AttributeDescription?.Value, attributeID, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

		}
		public override void AddAttributeValue(BCNonStockItemEntityBucket bucket, string attributeID, object attributeValue)
		{
			MappedNonStockItem obj = bucket.Product;
			NonStockItem impl = obj.Local;
			impl.Attributes = impl.Attributes ?? new List<AttributeValue>();
			AttributeValue attribute = new AttributeValue();
			attribute.AttributeID = new StringValue() { Value = attributeID };
			attribute.Value = new StringValue() { Value = attributeValue?.ToString() };
			attribute.ValueDescription = new StringValue() { Value = attributeValue?.ToString() };
			impl.Attributes.Add(attribute);
		}

		public override async Task SaveBucketExport(BCNonStockItemEntityBucket bucket, IMappedEntity existing, String operation, CancellationToken cancellationToken = default)
		{
			MappedNonStockItem obj = bucket.Product;

			ProductData data = null;

			obj.Extern.CustomFieldsData =ExportCustomFields(obj, obj.Extern.CustomFields, data, cancellationToken);

			if (obj.ExternID == null)
				data = productDataProvider.Create(obj.Extern);
			else
				data = productDataProvider.Update(obj.Extern, obj.ExternID);

			obj.AddExtern(data, data.Id?.ToString(), data.Name, data.DateModifiedUT.ToDate());

			SaveImages(obj, obj.Local?.FileUrls, cancellationToken);
			SaveVideos(obj, obj.Local?.FileUrls);

			UpdateStatus(obj, operation);
			if (data != null)
				UpdateRelatedItems(obj);
		}
		#endregion
	}
}
