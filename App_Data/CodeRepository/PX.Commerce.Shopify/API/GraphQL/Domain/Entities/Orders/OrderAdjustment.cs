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
	/// An order adjustment accounts for refunded shipping costs or the difference between calculated and actual refund amount.
	/// </summary>
	[JsonObject(Description = "Order Adjustment")]
	[CommerceDescription(ShopifyCaptions.OrderAdjustment, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class OrderAdjustmentGQL : BCAPIEntity, INode
	{
		/// <summary>
		/// The amount of the order adjustment in shop and presentment currencies.
		/// </summary>
		[JsonProperty("amountSet")]
		[GraphQLField("amountSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag AmountSet { get; set; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.AmountPresentment, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public decimal? AmountPresentment { get => AmountSet?.PresentmentMoney?.Amount; }

		/// <summary>
		/// A globally-unique identifier.
		/// </summary>
		[JsonProperty("id")]
		[GraphQLField("id", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.ID)]
		[CommerceDescription(ShopifyCaptions.Id, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public string Id { get; set; }

		/// <summary>
		/// The order adjustment type.
		/// </summary>
		[JsonProperty("kind")]
		[GraphQLField("kind", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		[CommerceDescription(ShopifyCaptions.Kind, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public string Kind { get; set; }

		/// <summary>
		/// The tax amount of the order adjustment in shop and presentment currencies.
		/// </summary>
		[JsonProperty("taxAmountSet")]
		[GraphQLField("taxAmountSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag TaxAmountSet { get; set; }

		[JsonIgnore]
		[CommerceDescription(ShopifyCaptions.TaxAmountPresentment, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public decimal? TaxAmountPresentment { get => TaxAmountSet?.PresentmentMoney?.Amount; }
	}
}
