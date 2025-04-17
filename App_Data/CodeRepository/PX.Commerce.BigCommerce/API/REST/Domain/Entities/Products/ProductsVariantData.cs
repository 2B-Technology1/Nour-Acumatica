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
using Newtonsoft.Json;
using PX.Commerce.Core;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonObject(Description = "Product -> Product Variant")]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class ProductsVariantData: BCAPIEntity
	{
        public ProductsVariantData()
        {
            OptionValues = new List<ProductVariantOptionValueData>();
        }
        /// <summary>
        ///  (optional) number (double float)	The cost price of the variant.
        /// </summary>
        [JsonProperty("cost_price")]
		[CommerceDescription(BigCommerceCaptions.CostPrice, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public decimal? CostPrice { get; set; }

        /// <summary>
        ///  (optional) number (double float)	This variant’s base price on the storefront. If this value is null, the product’s default price (set in the Product resource’s price field) will be used as the base price.
        /// </summary>
        [JsonProperty("price")]
		[CommerceDescription(BigCommerceCaptions.Price, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
        public decimal? Price { get; set; }

        /// <summary>
        ///  (optional) number (double float)	This variant’s base weight on the storefront. If this value is null, the product’s default weight (set in the Product resource’s weight field) will be used as the base weight.
        /// </summary>
        [JsonProperty("sale_price")]
		[CommerceDescription(BigCommerceCaptions.SalePrice, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
        public decimal? SalePrice { get; set; }

        [JsonProperty("retail_price")]
		[CommerceDescription(BigCommerceCaptions.RetailPrice, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public decimal? RetailPrice { get; set; }

        [JsonProperty("weight")]
		[CommerceDescription(BigCommerceCaptions.Weight, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public decimal? Weight { get; set; }

        [JsonProperty("width")]
		[CommerceDescription(BigCommerceCaptions.Width, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public decimal? Width { get; set; }

        [JsonProperty("height")]
		[CommerceDescription(BigCommerceCaptions.Height, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public decimal? Height { get; set; }

        [JsonProperty("depth")]
		[CommerceDescription(BigCommerceCaptions.Depth, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public decimal? Depth { get; set; }

        [JsonProperty("is_free_shipping")]
		[CommerceDescription(BigCommerceCaptions.IsFreeShipping, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public bool? IsFreeShipping { get; set; }

        [JsonProperty("fixed_cost_shipping_price")]
		[CommerceDescription(BigCommerceCaptions.FixedCostShippingPrice, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public decimal? FixedCostShippingPrice { get; set; }

        [JsonProperty("purchasing_disabled")]
		[CommerceDescription(BigCommerceCaptions.PurchasingDisabled, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public bool? PurchasingDisabled { get; set; }

        [JsonProperty("purchasing_disabled_message")]
		[CommerceDescription(BigCommerceCaptions.PurchasingDisabledMessage, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
        public string PurchasingDisabledMessage { get; set; }

        [JsonProperty("image_url")]
		[CommerceDescription(BigCommerceCaptions.ImageUrl, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public string ImageUrl { get; set; }

        [JsonProperty("upc")]
		[CommerceDescription(BigCommerceCaptions.UPCCode, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public string Upc { get; set; }

        /// <summary>
        /// Manufacturer Part Number (MPN) — MPN is a series of letters or numbers assigned to product by its manufacturer. 
        /// There is no MPN standard so its format can vary between different manufacturers/companies.
        /// </summary>
        [JsonProperty("mpn")]
		[CommerceDescription(BigCommerceCaptions.ManufacturerPartNumber, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public string Mpn { get; set; }

        /// <summary>
        /// Global Trade Item Number (GTIN) — An identifier for trade items that is 
        /// incorporated into several product identifying standards like ISBN, UPC, and EAN.
        /// </summary>
        [JsonProperty("gtin")]
		[CommerceDescription(BigCommerceCaptions.GlobalTradeNumber, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public string Gtin { get; set; }

        [JsonProperty("inventory_level")]
		[CommerceDescription(BigCommerceCaptions.InventoryLevel, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public int? InventoryLevel { get; set; }

        [JsonProperty("inventory_warning_level")]
		[CommerceDescription(BigCommerceCaptions.InventoryWarningLevel, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public int? InventoryWarningLevel { get; set; }

        [JsonProperty("bin_picking_number")]
		[CommerceDescription(BigCommerceCaptions.BinPickingNumber, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public string BinPickingNumber { get; set; }

        [JsonProperty("id")]
        public int? Id { get; set; }

        [JsonProperty("product_id")]
		[CommerceDescription(BigCommerceCaptions.Productid, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public int? ProductId { get; set; }

        [JsonProperty("sku")]
		public string Sku { get; set; }

        [JsonProperty("sku_id")]
        public int? SkuId { get; set; }

        /// <summary>
        /// Array of option and option values IDs that make up this variant. Will be empty if the variant is the product’s base variant
        /// </summary>
        [JsonProperty("option_values")]
        public IList<ProductVariantOptionValueData> OptionValues { get; set; }

        [JsonProperty("calculated_price")]
        public decimal? CalculatedPrice { get; set; }

		public Guid? LocalID { get; set; }

	}


}
