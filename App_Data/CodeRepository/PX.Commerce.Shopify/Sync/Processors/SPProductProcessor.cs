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
using PX.Commerce.Shopify.API.GraphQL;
using PX.Commerce.Shopify.API.REST;
using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PX.Objects.GL;
using PX.Objects.AR;
using PX.Objects.IN;

namespace PX.Commerce.Shopify
{
	public class SPProductEntityBucket<TPrimaryMapped> : EntityBucketBase, IEntityBucket
		where TPrimaryMapped : IMappedEntity
	{
		public IMappedEntity Primary => Product;
		public IMappedEntity[] Entities => new IMappedEntity[] { Primary };
		public TPrimaryMapped Product;
		public Dictionary<string, long?> VariantMappings = new Dictionary<string, long?>();
	}

	public abstract class SPProductProcessor<TGraph, TEntityBucket, TPrimaryMapped, TExternType, TLocalType> : ProductProcessorBase<TGraph, TEntityBucket, TPrimaryMapped, TExternType, TLocalType>
		where TGraph : PXGraph
		where TEntityBucket : SPProductEntityBucket<TPrimaryMapped>, new()
		where TPrimaryMapped : class, IMappedEntityLocal<TLocalType>, new()
		where TExternType : BCAPIEntity, IExternEntity, new()
		where TLocalType : ProductItem, ILocalEntity, new()
	{

		protected IProductRestDataProvider<ProductData> productDataProvider;
		protected IChildRestDataProvider<ProductVariantData> productVariantDataProvider;
		protected IChildRestDataProvider<ProductImageData> productImageDataProvider;
		protected IEnumerable<ProductVariantData> ExternProductVariantData = new List<ProductVariantData>();
		protected Dictionary<int, string> SalesCategories;
		protected IMetafieldsGQLDataProvider metafieldDataGQLProvider;
		protected IProductGQLDataProvider ProductGQLDataProvider { get; set; }

		#region Factories

		[InjectDependency]
		protected ISPRestDataProviderFactory<IProductRestDataProvider<ProductData>> productDataProviderFactory { get; set; }

		[InjectDependency]
		protected ISPRestDataProviderFactory<IChildRestDataProvider<ProductVariantData>> productVariantDataProviderFactory { get; set; }

		[InjectDependency]
		protected ISPRestDataProviderFactory<IChildRestDataProvider<ProductImageData>> productImageDataProviderFactory { get; set; }
		[InjectDependency]
		public ISPGraphQLDataProviderFactory<MetaFielsGQLDataProvider> metafieldGrahQLDataProviderFactory { get; set; }

		[InjectDependency]
		public ISPGraphQLAPIClientFactory shopifyGraphQLClientFactory { get; set; }

		[InjectDependency]
		public ISPMetafieldsMappingServiceFactory spMetafieldsMappingServiceFactory { get; set; }

		public SPGraphQLAPIClient shopifyGraphQLClient { get; set; }

		private ISPMetafieldsMappingService metafieldsMappingService;

		[InjectDependency]
		protected ISPGraphQLDataProviderFactory<ProductGQLDataProvider> SPGraphQLDataProviderFactory { get; set; }

		#endregion

		public override async Task Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			await base.Initialise(iconnector, operation);

			var client = SPConnector.GetRestClient(GetBindingExt<BCBindingShopify>());

			productDataProvider = productDataProviderFactory.CreateInstance(client);
			productVariantDataProvider = productVariantDataProviderFactory.CreateInstance(client);
			productImageDataProvider = productImageDataProviderFactory.CreateInstance(client);

			var graphQLClient = shopifyGraphQLClientFactory.GetClient(GetBindingExt<BCBindingShopify>());
			metafieldDataGQLProvider = metafieldGrahQLDataProviderFactory.GetProvider(graphQLClient);
			metafieldsMappingService = spMetafieldsMappingServiceFactory.GetInstance(metafieldDataGQLProvider);
			ProductGQLDataProvider = SPGraphQLDataProviderFactory.GetProvider(graphQLClient);

			SalesCategories = new Dictionary<int, string>();
		}
		#region Common

		/// <summary>
		/// Initialize a new object of the entity to be used to Get bucket
		/// </summary>
		/// <returns>The initialized entity</returns>
		protected abstract TLocalType CreateEntityForGet();

		/// <inheritdoc/>
		protected override void DeleteImageFromExternalSystem(string parentId, string imageId)
		{
			try
			{
				productImageDataProvider.Delete(parentId, imageId);
			}
			catch (RestException ex)
			{
				throw new PXException(ex, ShopifyMessages.ErrorDuringImageDeletionExceptionMessage, ex.Message);
			}
		}

		/// <summary>
		/// Calulates Default and Msrp prices if baseuom and sales UOn are different
		/// </summary>
		/// <param name="inventoryCD"></param>
		/// <param name="msrp"></param>
		/// <param name="defaultPrice"></param>
		public virtual void CalculatePrices(string inventoryCD, ref decimal? msrp, ref decimal? defaultPrice)
		{
			INUnit unit = null;
			var price = GetDefaultPrice(inventoryCD);
			if (price == null)
			{
				//convert based on converion rate
				unit = GetUnit(inventoryCD);
				defaultPrice = INUnitAttribute.ConvertValue(defaultPrice.Value, unit, INPrecision.UNITCOST);
			}
			else
			{
				defaultPrice = price;
			}

			if (msrp > 0)
			{
				if (unit == null)
				{
					unit = GetUnit(inventoryCD);
				}
				msrp = INUnitAttribute.ConvertValue(msrp.Value, unit, INPrecision.UNITCOST);
			}
		}

		public virtual INUnit GetUnit(string inventoryCD)
		{
			return PXSelectJoin<INUnit, InnerJoin<InventoryItem, On<INUnit.inventoryID, Equal<InventoryItem.inventoryID>
							   , And<INUnit.fromUnit, Equal<InventoryItem.salesUnit>>>>,
								   Where<INUnit.unitType, Equal<INUnitType.inventoryItem>,
									   And<InventoryItem.inventoryCD, Equal<Required<InventoryItem.inventoryCD>>>>>.Select(this, inventoryCD);
		}



		/// Return sales price from Sales prices form where
		/// 1.Price base type is base price
		/// 2 price is effective and not expired
		/// 3.Whose warehouse is null
		/// 4. price uom is equal to inventory sales unit
		/// 5.In case of multicurrency  price whose Currency matches with store base currency
		/// </summary>
		/// <param name="inventoryId"></param>
		/// <returns></returns>
		public virtual decimal? GetDefaultPrice(string inventoryCd)
		{
			decimal? price = null;
			var baseCurrency = Branch.PK.Find(this, GetBinding().BranchID)?.BaseCuryID.ValueField();
			var storeDefaultCurrency = GetBindingExt<Objects.BCBindingExt>().DefaultStoreCurrency;
			foreach (PXResult<ARSalesPrice, InventoryItem, INSite> item in PXSelectJoin<PX.Objects.AR.ARSalesPrice,
				InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<ARSalesPrice.inventoryID>, And<ARSalesPrice.uOM, Equal<InventoryItem.salesUnit>>>,
				LeftJoin<INSite, On<INSite.siteID, Equal<ARSalesPrice.siteID>>>>,
				Where<ARSalesPrice.priceType, Equal<PriceTypes.basePrice>,
				And<InventoryItem.inventoryCD, Equal<Required<InventoryItem.inventoryCD>>>>>.Select(this, inventoryCd))
			{
				ARSalesPrice salesPrice = (ARSalesPrice)item;
				InventoryItem inventoryItem = (InventoryItem)item;
				INSite warehouse = (INSite)item;
				if (salesPrice != null && (salesPrice.CuryID ?? baseCurrency?.Value) == storeDefaultCurrency && salesPrice.TaxCalcMode != PX.Objects.TX.TaxCalculationMode.Gross)
				{
					if ((salesPrice.BreakQty ?? 1) > 1 || warehouse?.SiteCD != null ||
					(salesPrice.ExpirationDate != null && ((DateTime)salesPrice.ExpirationDate.Value).Date < PX.Common.PXTimeZoneInfo.Now.Date) ||
					(salesPrice.EffectiveDate != null && ((DateTime)salesPrice.EffectiveDate.Value).Date > PX.Common.PXTimeZoneInfo.Now.Date))
					{
						continue;
					}
					
					if (salesPrice.BreakQty == null || salesPrice.BreakQty == 1)//if brk qty is 1 use it
					{
						return salesPrice.SalesPrice;
					}

					price = salesPrice.SalesPrice; //if brk qty is 0 then  check if brkqty 1 is present if not then use 0
				}
			}
			return price;
		}

		public virtual void SaveImages(IMappedEntity obj, List<InventoryFileUrls> urls, CancellationToken cancellationToken = default)
		{
			var fileURLs = urls?.Where(x => x.FileType?.Value == BCCaptions.Image && !string.IsNullOrEmpty(x.FileURL?.Value))?.ToList();

			SyncDeletedMediaUrls(obj, fileURLs);

			if (fileURLs == null || fileURLs.Count() == 0) return;

			List<ProductImageData> imageList = null;
			obj.ClearDetails(BCEntitiesAttribute.ProductImage);

			foreach (var image in fileURLs)
			{
				ProductImageData productImageData = null;
				try
				{
					if (imageList == null)
						imageList = productImageDataProvider.GetAll(obj.ExternID, new FilterWithFields() { Fields = "id,product_id,src,variant_ids,position" }, cancellationToken: cancellationToken).ToList();
					if (imageList?.Count > 0)
					{
						productImageData = imageList.FirstOrDefault(x => (x.Metafields != null && x.Metafields.Any(m => string.Equals(m.Key, ShopifyConstants.ProductImage, StringComparison.OrdinalIgnoreCase)
							&& string.Equals(m.Value, image.FileURL.Value, StringComparison.OrdinalIgnoreCase))));
						if (productImageData != null)
						{
							if (obj.Details?.Any(x => x.EntityType == BCEntitiesAttribute.ProductImage && x.LocalID == image.NoteID?.Value) == false)
							{
								obj.AddDetail(BCEntitiesAttribute.ProductImage, image.NoteID.Value, productImageData.Id.ToString());
							}
							continue;
						}
					};
					productImageData = new ProductImageData()
					{
						Src = Uri.EscapeUriString(System.Web.HttpUtility.UrlDecode(image.FileURL.Value)),
						Metafields = new List<MetafieldData>() { new MetafieldData() { Key = ShopifyConstants.ProductImage, Value = image.FileURL.Value, Type = ShopifyConstants.ValueType_SingleString, Namespace = BCObjectsConstants.Namespace_Global } },
					};
					var metafields = productImageData.Metafields;
					productImageData = productImageDataProvider.Create(productImageData, obj.ExternID);
					productImageData.Metafields = metafields;
					if (obj.Details?.Any(x => x.EntityType == BCEntitiesAttribute.ProductImage && x.LocalID == image.NoteID?.Value) == false)
					{
						obj.AddDetail(BCEntitiesAttribute.ProductImage, image.NoteID.Value, productImageData.Id.ToString());
					}
					imageList = imageList ?? new List<ProductImageData>();
					imageList.Add(productImageData);
				}
				catch (Exception ex)
				{
					throw new PXException(ex.Message);
				}
			}
		}

		public virtual void SetProductStatus(ProductData data, string status, string availability, string visibility)
		{
			if (availability != BCCaptions.DoNotUpdate)
			{
				if (status.Equals(PX.Objects.IN.Messages.Inactive) || status.Equals(PX.Objects.IN.Messages.NoSales) || status.Equals(PX.Objects.IN.Messages.ToDelete))
				{
					data.Status = ProductStatus.Draft;
					data.Published = false;
				}
				else
				{
					data.Status = ProductStatus.Active;
					if (visibility == BCCaptions.Invisible || availability == BCCaptions.Disabled)
					{
						data.PublishedScope = PublishedScope.Web;
						data.Published = false;
					}
					else
					{
						data.PublishedScope = PublishedScope.Global;
						data.Published = true;
					}
				}
			}
		}

		public override List<(string fieldName, string fieldValue)> GetExternCustomFieldList(BCEntity entity, ExternCustomFieldInfo customFieldInfo)
		{
			List<(string fieldName, string fieldValue)> fieldsList = new List<(string fieldName, string fieldValue)>() { (BCConstants.MetafieldFormat, BCConstants.MetafieldFormat) };

			return fieldsList;
		}
		public override void ValidateExternCustomField(BCEntity entity, ExternCustomFieldInfo customFieldInfo, string sourceObject, string sourceField, string targetObject, string targetField, EntityOperationType direction)
		{
			//Validate the field format
			if (customFieldInfo.Identifier == BCConstants.ShopifyMetaFields)
			{
				var fieldStrGroup = direction == EntityOperationType.ImportMapping ? sourceField.Split('.') : targetField.Split('.');
				if (fieldStrGroup.Length == 2)
				{
					var keyFieldName = fieldStrGroup[0].Replace("[", "").Replace("]", "").Replace(" ", "");
					if (!string.IsNullOrWhiteSpace(keyFieldName) && string.Equals(keyFieldName, BCConstants.MetafieldFormat, StringComparison.OrdinalIgnoreCase) == false)
						return;
				}
				throw new PXException(BCMessages.InvalidFilter, "Target", BCConstants.MetafieldFormat);
			}
		}

		public override object GetExternCustomFieldValue(TEntityBucket entity, ExternCustomFieldInfo customFieldInfo, object sourceData, string sourceObject, string sourceField, out string displayName)
		{
			displayName = null;
			if (customFieldInfo.Identifier == BCConstants.ShopifyMetaFields)
			{
				return new List<object>() { sourceData };
			}
			else if (customFieldInfo.Identifier == BCAPICaptions.Matrix)
			{
				if (string.IsNullOrWhiteSpace(sourceField))
				{
					return ((TemplateItems)sourceData)?.Matrix ?? new List<ProductItem>();
					// we need make sure order of matrix items to be returned will be as same as that of Extern which are to be exported to Shopify
					// so that values to be mapped as specified in the Entities screen (if there's any) for Matrix Items will be correctly mapped 
					var matrixItems = ((ProductData)entity.Primary.Extern)?.Variants?.Select(x => ((TemplateItems)sourceData)?.Matrix?.FirstOrDefault(y => y.InventoryID.Value.Trim() == x.Sku)).ToList();
					return matrixItems ?? new List<ProductItem>();
				}

				var result = GetPropertyValue(sourceData, sourceField, out displayName);
				displayName = sourceData != null && sourceData is ProductItem ? $"{BCAPICaptions.Matrix}{BCConstants.Arrow} {((ProductItem)sourceData)?.InventoryID?.Value}" : displayName;
				return result;
			}
			return null;
		}

		public override void SetExternCustomFieldValue(TEntityBucket entity, ExternCustomFieldInfo customFieldInfo, object targetData, string targetObject, string targetField, string sourceObject, object value, IMappedEntity existing)
		{
			if (value == null || value == PXCache.NotSetValue)
				return;

			if (customFieldInfo.Identifier != BCConstants.ShopifyMetaFields)
				return;

			var targetinfo = targetField?.Split('.');
			if (targetinfo.Length != 2)
				return;

			var nameSpaceField = targetinfo[0].Replace("[", "").Replace("]", "")?.Trim();
			var keyField = targetinfo[1].Replace("[", "").Replace("]", "")?.Trim();

			ProductData data = (ProductData)entity.Primary.Extern;
			ProductData existingProduct = existing?.Extern as ProductData;

			var entityType = customFieldInfo.ExternEntityType == typeof(ProductVariantData) ?
				ShopifyGraphQLConstants.OWNERTYPE_PRODUCTVARIANT : ShopifyGraphQLConstants.OWNERTYPE_PRODUCT;

			//Correct the metafield name - metafield names are case sensitive. But if we submit the same metafield name with different case, Shopify will raise an error
			//thus, we must check whethe the metafield exists or not and correct the name if needed
			var correctedName = metafieldsMappingService.CorrectMetafieldName(entityType, nameSpaceField, keyField, existingProduct?.Metafields);
			nameSpaceField = correctedName.Item1;
			keyField = correctedName.Item2;

			var metafieldValue = metafieldsMappingService.GetFormattedMetafieldValue(entityType, nameSpaceField, keyField, Convert.ToString(value));

			var newMetaField = new MetafieldData()
			{
				Namespace = nameSpaceField,
				Key = keyField,
				Value = metafieldValue.Value,
				Type = metafieldValue.GetShopifyType()
			};

			if (customFieldInfo.ExternEntityType == typeof(ProductData))
			{
				var metaFieldList = data.Metafields = data.Metafields ?? new List<MetafieldData>();
				if (existingProduct != null && existingProduct.Metafields?.Count > 0)
				{
					var existedMetaField = existingProduct.Metafields.FirstOrDefault(x => string.Equals(x.Namespace, nameSpaceField, StringComparison.OrdinalIgnoreCase) && string.Equals(x.Key, keyField, StringComparison.OrdinalIgnoreCase));
					newMetaField.Id = existedMetaField?.Id;
					if (existedMetaField?.Type != null && !String.IsNullOrEmpty(existedMetaField.Type)) //always keep the original type of the metafield
						newMetaField.Type = existedMetaField.Type;
				}
				var matchedData = metaFieldList.FirstOrDefault(x => string.Equals(x.Namespace, nameSpaceField, StringComparison.OrdinalIgnoreCase) && string.Equals(x.Key, keyField, StringComparison.OrdinalIgnoreCase));
				if (matchedData != null)
				{
					matchedData = newMetaField;
				}
				else
					metaFieldList.Add(newMetaField);

				return;
			}

			if (customFieldInfo.ExternEntityType == typeof(ProductVariantData))
			{
				bool anyFound = false;
				string matrixItemFormat = $"{BCAPICaptions.Matrix}{BCConstants.Arrow}";
				foreach (var variantItem in data.Variants)
				{
					var metaFieldList = variantItem.VariantMetafields = variantItem.VariantMetafields ?? new List<MetafieldData>();
					if (sourceObject.StartsWith(matrixItemFormat))
					{
						if (string.Equals(variantItem.Sku, sourceObject.Substring(matrixItemFormat.Length)?.Trim(), StringComparison.OrdinalIgnoreCase))
							anyFound = true;
						else
							continue;
					}
					else
					{
						//If source object is not from Matrix item itself, all variants should have the same metafield data, but the ID should be different in each variant metafield record.
						//We should avoid to reference the same metafield object and cause both of them have the same ID value, so create the new object each time.
						newMetaField = new MetafieldData()
						{
							Namespace = nameSpaceField,
							Key = keyField,
							Value = metafieldValue.Value,
							Type = metafieldValue.GetShopifyType()
						};
					}
					if (existingProduct?.Variants?.Count > 0)
					{
						var existedVariant = existingProduct.Variants.FirstOrDefault(x => string.Equals(x.Sku, variantItem.Sku, StringComparison.OrdinalIgnoreCase));
						if (existedVariant != null && existedVariant.VariantMetafields?.Count > 0)
						{
							var existedMetaField = existedVariant.VariantMetafields.FirstOrDefault(x => string.Equals(x.Namespace, nameSpaceField, StringComparison.OrdinalIgnoreCase) && string.Equals(x.Key, keyField, StringComparison.OrdinalIgnoreCase));
							newMetaField.Id = existedMetaField?.Id;
						}
					}
					var matchedData = metaFieldList.FirstOrDefault(x => string.Equals(x.Namespace, nameSpaceField, StringComparison.OrdinalIgnoreCase) && string.Equals(x.Key, keyField, StringComparison.OrdinalIgnoreCase));
					if (matchedData != null)
					{
						matchedData = newMetaField;
					}
					else
						metaFieldList.Add(newMetaField);
					if (anyFound) break;
				}
			}
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
			TLocalType impl = cbapi.GetByID<TLocalType>(syncstatus.LocalID, item, GetCustomFieldsForExport());
			if (impl == null) return EntityStatus.None;

			TPrimaryMapped obj = bucket.Product = bucket.Product.Set(impl, impl.SyncID, impl.SyncTime);
			EntityStatus status = EnsureStatus(obj, SyncDirection.Export);
			if (!bucket.VariantMappings.ContainsKey(impl.InventoryID.Value))
				bucket.VariantMappings.Add(impl.InventoryID.Value, null);
			var categories = impl.Categories;
			if (categories != null)
			{
				foreach (CategoryStockItem category in categories)
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
		#endregion
	}
}
