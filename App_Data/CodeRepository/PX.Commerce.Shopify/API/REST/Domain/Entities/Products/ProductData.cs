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
using System.ComponentModel;
using Newtonsoft.Json;
using PX.Commerce.Core;

namespace PX.Commerce.Shopify.API.REST
{
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class ProductResponse : IEntityResponse<ProductData>
	{
		[JsonProperty("product")]
		public ProductData Data { get; set; }
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class ProductsResponse : IEntitiesResponse<ProductData>
	{
		[JsonProperty("products")]
		public IEnumerable<ProductData> Data { get; set; }
	}

	[JsonObject(Description = "Product")]
	[CommerceDescription(ShopifyCaptions.Product, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class ProductData : BCAPIEntity
	{
		public ProductData()
		{
			Variants = new List<ProductVariantData>();
		}
		/// <summary>
		/// A description of the product. Supports HTML formatting.
		/// </summary>
		[JsonProperty("body_html", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.BodyHTML, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public string BodyHTML { get; set; }

		/// <summary>
		/// A description of the product. Supports HTML formatting.
		/// </summary>
		[JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Status, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public ProductStatus? Status { get; set; }

		/// <summary>
		/// The date and time (ISO 8601 format) when the product variant was created.
		/// </summary>
		[JsonProperty("created_at")]
		//[CommerceDescription(ShopifyCaptions.DateCreated, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		[ShouldNotSerialize]
		public DateTime? DateCreatedAt { get; set; }

		/// <summary>
		/// A unique human-friendly string for the product. Automatically generated from the product's title. Used by the Liquid templating language to refer to objects.
		/// </summary>
		[JsonProperty("handle")]
		[ShouldNotSerialize]
		public string Handle { get; set; }

		/// <summary>
		/// An unsigned 64-bit integer that's used as a unique identifier for the product. Each id is unique across the Shopify system. No two products will have the same id, even if they're from different shops.
		/// </summary>
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		public long? Id { get; set; }

		/// <summary>
		/// A list of product image objects, each one representing an image associated with the product.
		/// </summary>
		[JsonProperty("images", NullValueHandling = NullValueHandling.Ignore)]
		public List<ProductImageData> Images { get; set; }

		/// <summary>
		/// The custom product property names like Size, Color, and Material. You can add up to 3 options of up to 255 characters each.
		/// </summary>
		[JsonProperty("options")]
		[CommerceDescription(ShopifyCaptions.ProductOptions, FieldFilterStatus.Skipped, FieldMappingStatus.Skipped)]
		public List<ProductOptionData> Options { get; set; }

		/// <summary>
		/// A categorization for the product used for filtering and searching products.
		/// </summary>
		[JsonProperty("product_type", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.ProductType, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public string ProductType { get; set; }

		/// <summary>
		/// The date and time (ISO 8601 format) when the product was published. Can be set to null to unpublish the product from the Online Store channel.
		/// </summary>
		[JsonProperty("published_at")]
		[ShouldNotSerialize]
		public DateTime? DataPublishedAt { get; set; }

		/// <summary>
		/// Set the product will be published or unpublished. Can be set to true to publish or set false to unpublish the product from the Online Store channel.
		/// </summary>
		[JsonProperty("published", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Published, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public bool? Published { get; set; }

		/// <summary>
		/// Whether the product is published to the Point of Sale channel. Valid values:
		/// web: The product is published to the Online Store channel but not published to the Point of Sale channel.
		/// global: The product is published to both the Online Store channel and the Point of Sale channel.
		/// </summary>
		[JsonProperty("published_scope", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.PublishedScope, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public PublishedScope? PublishedScope { get; set; }

		/// <summary>
		/// A string of comma-separated tags that are used for filtering and search. A product can have up to 250 tags. Each tag can have up to 255 characters.
		/// </summary>
		[JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Tags, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public string Tags { get; set; }

		/// <summary>
		/// The suffix of the Liquid template used for the product page. If this property is specified, then the product page uses a template called "product.suffix.liquid", where "suffix" is the value of this property. 
		/// If this property is "" or null, then the product page uses the default template "product.liquid". (default: null)
		/// </summary>
		[JsonProperty("template_suffix", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.TemplateSuffix, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public string TemplateSuffix { get; set; }

		/// <summary>
		/// The name of the product.
		/// </summary>
		[JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Title, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public string Title { get; set; }

		/// <summary>
		/// The date and time (ISO 8601 format) when the product was last modified.
		/// </summary>
		[JsonProperty("updated_at")]
		public String DateModifiedAt { get; set; }

		/// <summary>
		/// A list of product variants, each representing a different version of the product.
		/// </summary>
		[JsonProperty("variants")]
		[CommerceDescription(ShopifyCaptions.ProductVariants, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		[ApiHeaderRequest("X-Shopify-Api-Features", "include-presentment-prices")]
		public List<ProductVariantData> Variants { get; set; }

		/// <summary>
		///The name of the product's vendor.
		/// </summary>
		[JsonProperty("vendor", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Vendor, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public String Vendor { get; set; }

		/// <summary>
		///The product's SEO title
		/// </summary>
		[JsonProperty("metafields_global_title_tag", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.GlobalTitleTage, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public String GlobalTitleTag { get; set; }

		/// <summary>
		///The product's SEO description
		/// </summary>
		[JsonProperty("metafields_global_description_tag", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.GlobalDescriptionTag, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public String GlobalDescriptionTag { get; set; }

		[JsonIgnore]
		public List<String> Categories { get; set; }

		/// <summary>
		/// Attaches additional metadata to a shop's resources:
		///key(required) : An identifier for the metafield(maximum of 30 characters).
		///namespace(required): A container for a set of metadata(maximum of 20 characters). Namespaces help distinguish between metadata that you created and metadata created by another individual with a similar namespace.
		///value (required): Information to be stored as metadata.
		///value_type(required): The value type.Valid values: string and integer.
		///description(optional): Additional information about the metafield.
		/// </summary>
		[JsonProperty("metafields", NullValueHandling = NullValueHandling.Ignore)]
        [CommerceDescription(ShopifyCaptions.Metafields, mappingStatus: FieldMappingStatus.Export)]
		[BCExternCustomField(BCConstants.ShopifyMetaFields)]
		public List<MetafieldData> Metafields { get; set; }

	}

	public enum ProductTypes
	{
		physical,
		digital
	}
}
