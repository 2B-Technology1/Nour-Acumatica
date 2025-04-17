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
	[JsonObject(Description = "Order Tax Line")]
	[Description(ShopifyCaptions.TaxLine)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class OrderTaxLineGQL: BCAPIEntity
	{

		/// <summary>
		/// Whether the channel that submitted the tax line is liable for remitting. A value of null indicates unknown liability for this tax line.
		/// </summary>
		[JsonProperty("channelLiable")]
		[GraphQLField("channelLiable", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool? ChannelLiable { get; set; }

		/// <summary>
		/// The amount of tax, in shop and presentment currencies, after discounts and before returns.
		/// </summary>
		[JsonProperty("priceSet", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("priceSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag PriceSet { get; set; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.TaxAmountPresentment, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public decimal? PricePresentment { get => PriceSet?.PresentmentMoney?.Amount; set => PriceSet = new MoneyBag { PresentmentMoney = new Money { Amount = value } }; }

		/// <summary>
		/// The proportion of the line item price that the tax represents as a decimal.
		/// </summary>
		[JsonProperty("rate")]
		[GraphQLField("rate", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Decimal)]
		[CommerceDescription(ShopifyCaptions.TaxRate, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public decimal? Rate { get; set; }

		/// <summary>
		/// The proportion of the line item price that the tax represents as a percentage.
		/// </summary>
		[JsonProperty("ratePercentage")]
		[GraphQLField("ratePercentage", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Decimal)]
		public decimal? RatePercentage { get; set; }

		/// <summary>
		/// The name of the tax.
		/// </summary>
		[JsonProperty("title")]
		[GraphQLField("title", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.TaxName, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public string Title { get; set; }
	}
}
