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

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// The type of value given to a customer when a discount is applied to an order.
	/// For example, the application of the discount might give the customer a percentage off a specified item. Alternatively, the application of the discount might give the customer a monetary value in a given currency off an order.
	/// </summary>
	public class PricingValueGQL
	{
		#region MoneyV2

		/// <summary>
		/// Decimal money amount.
		/// </summary>
		[JsonProperty("amount")]
		[GraphQLField("amount", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Decimal, "MoneyV2")]
		[CommerceDescription(ShopifyCaptions.Amount, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public decimal? Amount { get; set; }

		/// <summary>
		/// Currency of the money.
		/// </summary>
		[JsonProperty("currencyCode")]
		[GraphQLField("currencyCode", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String, "MoneyV2")]
		[CommerceDescription(ShopifyCaptions.CurrencyCode, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public string CurrencyCode { get; set; }

		#endregion

		#region PricingPercentageValue

		/// <summary>
		/// Decimal money amount.
		/// </summary>
		[JsonProperty("percentage")]
		[GraphQLField("percentage", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Decimal, "PricingPercentageValue")]
		public decimal? Percentage { get; set; }

		#endregion
	}
}
