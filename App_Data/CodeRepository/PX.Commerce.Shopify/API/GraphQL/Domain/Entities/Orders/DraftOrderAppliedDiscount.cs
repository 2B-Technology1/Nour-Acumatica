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
	/// The order-level discount applied to a draft order.
	/// </summary>
	public class DraftOrderAppliedDiscountGQL: BCAPIEntity
	{
		/// <summary>
		/// The amount of money discounted, with values shown in both shop currency and presentment currency.
		/// </summary>
		[JsonProperty("amountSet")]
		[GraphQLField("amountSet", GraphQLConstants.DataType.Object, typeof(MoneyBag))]
		public MoneyBag AmountSet { get; set; }

		/// <summary>
		/// Amount of money discounted.
		/// </summary>
		[JsonProperty("amountV2")]
		[GraphQLField("amountV2", GraphQLConstants.DataType.Object, typeof(Money))]
		public Money Amount { get; set; }

		/// <summary>
		/// Description of the order-level discount.
		/// </summary>
		[JsonProperty("description")]
		[GraphQLField("description", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Description { get; set; }

		/// <summary>
		/// Name of the order-level discount.
		/// </summary>
		[JsonProperty("title")]
		[GraphQLField("title", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Title { get; set; }

		/// <summary>
		/// The order level discount amount. If valueType is "percentage", then value is the percentage discount.
		/// </summary>
		[JsonProperty("value")]
		[GraphQLField("value", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Float)]
		public decimal? Value { get; set; }

		/// <summary>
		/// Type of the order-level discount.
		/// </summary>
		[JsonProperty("valueType", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("valueType", GraphQLConstants.DataType.Enum, GraphQLConstants.ScalarType.String)]
		public DraftOrderAppliedDiscountType ValueType { get; set; }
	}
}
