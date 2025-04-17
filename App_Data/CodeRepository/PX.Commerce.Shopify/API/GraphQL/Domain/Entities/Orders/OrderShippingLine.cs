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
using PX.Commerce.Shopify.API.REST;

namespace PX.Commerce.Shopify.API.GraphQL
{
	[JsonObject(Description = "Order Shipping Line")]
	[CommerceDescription(ShopifyCaptions.ShippingLine, FieldFilterStatus.Filterable, FieldMappingStatus.Export)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class OrderShippingLineGQL: BCAPIEntity, INode
	{
		/// <summary>
		/// A reference to the carrier service that provided the rate. Present when the rate was computed by a third-party carrier service.
		/// </summary>
		[JsonProperty("carrierIdentifier")]
		[GraphQLField("carrierIdentifier", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.CarrierIdentifier, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public string CarrierIdentifier { get; set; }

		/// <summary>
		/// A reference to the shipping method.
		/// </summary>
		[JsonProperty("code")]
		[GraphQLField("code", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.Code, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public string Code { get; set; }

		/// <summary>
		/// Whether the shipping line is custom or not.
		/// </summary>
		[JsonProperty("custom")]
		[GraphQLField("custom", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool? Custom { get; set; }

		/// <summary>
		/// The general classification of the delivery method.
		/// </summary>
		[JsonProperty("deliveryCategory")]
		[GraphQLField("deliveryCategory", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string DeliveryCategory { get; set; }

		/// <summary>
		/// The discounts that have been allocated to the shipping line.
		/// </summary>
		[JsonProperty("discountAllocations", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("discountAllocations", GraphQLConstants.DataType.Object, typeof(DiscountAllocationGQL))]
		[CommerceDescription(ShopifyCaptions.DiscountAllocation, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public List<DiscountAllocationGQL> DiscountAllocations { get; set; }

		/// <summary>
		/// The pre-tax shipping price with discounts applied.
		/// </summary>
		[JsonProperty("discountedPriceSet", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("discountedPriceSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag DiscountedPriceSet { get; set; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.DiscountedPricePresentment, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public decimal? DiscountedPricePresentment { get => DiscountedPriceSet?.PresentmentMoney?.Amount; set => DiscountedPriceSet = new MoneyBag { PresentmentMoney = new Money { Amount = value } }; }

		///<inheritdoc/>
		[JsonProperty("id")]
		[GraphQLField("id", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.ID)]
		public string Id { get; set; }

		/// <summary>
		/// The pre-tax shipping price without any discounts applied.
		/// </summary>
		[JsonProperty("originalPriceSet", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("originalPriceSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag OriginalPriceSet { get; set; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.ShippingCostExcludingTaxPresentment, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public decimal? OriginalPricePresentment { get => OriginalPriceSet?.PresentmentMoney?.Amount; set => OriginalPriceSet = new MoneyBag { PresentmentMoney = new Money { Amount = value } }; }

		/// <summary>
		/// The phone number at the shipping address.
		/// </summary>
		[JsonProperty("phone")]
		[GraphQLField("phone", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Phone { get; set; }

		/// <summary>
		/// A unique identifier for the shipping rate. The format can change without notice and is not meant to be shown to users.
		/// </summary>
		[JsonProperty("shippingRateHandle")]
		[GraphQLField("shippingRateHandle", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string ShippingRateHandle { get; set; }

		/// <summary>
		/// Returns the rate source for the shipping line.
		/// </summary>
		[JsonProperty("source")]
		[GraphQLField("source", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.SourceName, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public string Source { get; set; }

		/// <summary>
		/// The TaxLine objects connected to this shipping line.
		/// </summary>
		[JsonProperty("taxLines", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("taxLines", GraphQLConstants.DataType.Object, typeof(OrderTaxLineGQL))]
		[CommerceDescription(ShopifyCaptions.TaxLine, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public List<OrderTaxLineGQL> TaxLines { get; set; }

		/// <summary>
		/// Returns the title of the shipping line.
		/// </summary>
		[JsonProperty("title")]
		[GraphQLField("title", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.Title, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public string Title { get; set; }
	}
}
