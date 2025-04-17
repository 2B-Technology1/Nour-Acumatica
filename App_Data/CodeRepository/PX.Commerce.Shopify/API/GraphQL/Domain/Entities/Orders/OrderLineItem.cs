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

using Newtonsoft.Json;
using PX.Commerce.Core;
using System.Collections.Generic;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// Represents individual products and quantities purchased in the associated order.
	/// </summary>
	[JsonObject(Description = "Order Line Item")]
	[CommerceDescription(ShopifyCaptions.LineItem, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class OrderLineItemGQL : BCAPIEntity, INode
	{
		/// <summary>
		/// The line item's quantity, minus the removed quantity.
		/// </summary>
		[JsonProperty("currentQuantity")]
		[GraphQLField("currentQuantity", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Int)]
		public int CurrentQuantity { get; set; }

		/// <summary>
		/// The line item's quantity, minus the removed quantity.
		/// </summary>
		[JsonProperty("customAttributes")]
		[GraphQLField("customAttributes", GraphQLConstants.DataType.Object, typeof(Attribute))]
		[CommerceDescription(ShopifyCaptions.Properties, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		[BCExternCustomField(BCConstants.OrderItemProperties)]
		public List<Attribute> CustomAttributes { get; set; }

		/// <summary>
		/// The discounts that have been allocated onto the line item by discount applications, not including order edits and refunds.
		/// </summary>
		[JsonProperty("discountAllocations")]
		[GraphQLField("discountAllocations", GraphQLConstants.DataType.Object, typeof(DiscountAllocationGQL))]
		[CommerceDescription(ShopifyCaptions.DiscountAllocation, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public List<DiscountAllocationGQL> DiscountAllocations { get; set; }

		/// <summary>
		/// The total line price after discounts are applied, in shop and presentment currencies.
		/// </summary>
		[JsonProperty("discountedTotalSet", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("discountedTotalSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag DiscountedTotalSet { get; set; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.TotalDiscountPresentment, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public decimal? DiscountedTotalPresentment { get => DiscountedTotalSet?.PresentmentMoney?.Amount; set => DiscountedTotalSet = new MoneyBag { PresentmentMoney = new Money { Amount = value } }; }

		/// <summary>
		/// The approximate split price of a line item unit, in shop and presentment currencies. This value doesn't include discounts applied to the entire order.
		/// </summary>
		[JsonProperty("discountedUnitPriceSet", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("discountedUnitPriceSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag DiscountedUnitPriceSet { get; set; }

		[JsonIgnore]
		public decimal? DiscountedUnitPricePresentment { get => DiscountedUnitPriceSet?.PresentmentMoney?.Amount; set => DiscountedUnitPriceSet = new MoneyBag { PresentmentMoney = new Money { Amount = value } }; }

		/// <summary>
		/// A globally-unique identifier.
		/// </summary>
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("id", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.Id, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public string Id { get; set; }

		/// <summary>
		/// The name of the product.
		/// </summary>
		[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("name", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.Name, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public string Name { get; set; }

		/// <summary>
		/// The total number of units that can't be fulfilled. For example, if items have been refunded, or the item is not something that can be fulfilled, like a tip.
		/// Please see the FulfillmentOrder object for more fulfillment details.
		/// </summary>
		[JsonProperty("nonFulfillableQuantity")]
		[GraphQLField("nonFulfillableQuantity", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Int)]
		public int NonFulfillableQuantity { get; set; }

		/// <summary>
		/// The total price in shop and presentment currencies, without discounts applied. This value is based on the unit price of the variant x quantity.
		/// </summary>
		[JsonProperty("originalTotalSet", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("originalTotalSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag OriginalTotalSet { get; set; }

		[JsonIgnore]
		public decimal? OriginalTotalPresentment { get => OriginalTotalSet?.PresentmentMoney?.Amount; set => OriginalTotalSet = new MoneyBag { PresentmentMoney = new Money { Amount = value } };  }

		/// <summary>
		/// The variant unit price without discounts applied, in shop and presentment currencies.
		/// </summary>
		[JsonProperty("originalUnitPriceSet", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("originalUnitPriceSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag OriginalUnitPriceSet { get; set; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.PricePresentment, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public decimal? OriginalUnitPricePresentment { get => OriginalUnitPriceSet?.PresentmentMoney?.Amount; set => OriginalUnitPriceSet = new MoneyBag { PresentmentMoney = new Money { Amount = value } }; }

		/// <summary>
		/// The number of products that were purchased.
		/// </summary>
		[JsonProperty("quantity")]
		[GraphQLField("quantity", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Int)]
		[CommerceDescription(ShopifyCaptions.Quantity, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public int Quantity { get; set; }

		/// <summary>
		/// The line item's quantity, minus the removed quantity.
		/// </summary>
		[JsonProperty("refundableQuantity")]
		[GraphQLField("refundableQuantity", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Int)]
		public int RefundableQuantity { get; set; }

		/// <summary>
		/// Whether physical shipping is required for the variant.
		/// </summary>
		[JsonProperty("requiresShipping")]
		[GraphQLField("requiresShipping", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		[CommerceDescription(ShopifyCaptions.RequiresShipping, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public bool RequiresShipping { get; set; }

		/// <summary>
		/// The variant SKU number.
		/// </summary>
		[JsonProperty("sku", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("sku", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.SKU, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public string Sku { get; set; }

		/// <summary>
		/// The taxes charged for this line item.
		/// </summary>
		[JsonProperty("taxLines", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("taxLines", GraphQLConstants.DataType.Object, typeof(OrderTaxLineGQL))]
		[CommerceDescription(ShopifyCaptions.TaxLine, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public List<OrderTaxLineGQL> TaxLines { get; set; }

		/// <summary>
		/// Whether the variant is taxable.
		/// </summary>
		[JsonProperty("taxable")]
		[GraphQLField("taxable", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		[CommerceDescription(ShopifyCaptions.Taxable, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public bool Taxable { get; set; }

		/// <summary>
		/// Title of the item. Ignored when variantId is provided.
		/// </summary>
		[JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("title", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.Title, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public string Title { get; set; }

		/// <summary>
		/// The total amount of the discount that's allocated to the line item, in the shop and presentment currencies. This field must be explicitly set using draft orders, Shopify scripts, or the API.
		/// </summary>
		[JsonProperty("totalDiscountSet", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("totalDiscountSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag TotalDiscountSet { get; set; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.TotalDiscountPresentment, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public decimal? TotalDiscountPresentment { get => TotalDiscountSet?.PresentmentMoney?.Amount; set => TotalDiscountSet = new MoneyBag { PresentmentMoney = new Money { Amount = value } }; }

		/// <summary>
		/// The number of units not yet fulfilled.
		/// </summary>
		[JsonProperty("unfulfilledQuantity")]
		[GraphQLField("unfulfilledQuantity", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Int)]
		public int UnfulfilledQuantity { get; set; }

		/// <summary>
		/// Thethe product variant
		/// </summary>
		[JsonProperty("variant", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("variant", GraphQLConstants.DataType.Object, typeof(ProductVariantGQL))]
		public ProductVariantGQL Variant { get; set; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.VariantId, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public string VariantId { get => Variant?.Id; set => Variant = new ProductVariantGQL { Id = value }; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.ProductId, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public string ProductId { get => Product?.Id; set => Product = new ProductDataGQL { Id = value }; }

		/// <summary>
		/// The Product object associated with this line item's variant.
		/// </summary>
		[JsonProperty("product", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("product", GraphQLConstants.DataType.Object, typeof(ProductDataGQL))]
		public ProductDataGQL Product { get; set; }

		[JsonIgnore]
		public string LocationId { get;set; }
	}
}
