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
	/// Transaction fee related to an order transaction.
	/// </summary>
	public class OrderTransactionFeeGQL: BCAPIEntity, INode
	{
		/// <summary>
		/// A globally-unique identifier.
		/// </summary>
		[JsonProperty("id")]
		[GraphQLField("id", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Id { get; set; }

		/// <summary>
		/// Name of the credit card flat fee.
		/// </summary>
		[JsonProperty("flatFeeName", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("flatFeeName", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string FlatFeeName { get; set; }

		/// <summary>
		/// Name of the credit card rate.
		/// </summary>
		[JsonProperty("rateName", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("rateName", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string RateName { get; set; }

		/// <summary>
		/// Amount of the fee.
		/// </summary>
		[JsonProperty("amount")]
		[GraphQLField("amount", GraphQLConstants.DataType.Object, typeof(Money))]
		public Money Amount { get; set; }

		/// <summary>
		/// Flat rate charge for a transaction.
		/// </summary>
		[JsonProperty("flatFee")]
		[GraphQLField("flatFee", GraphQLConstants.DataType.Object, typeof(Money))]
		public Money FlatFee { get; set; }

		/// <summary>
		/// Percentage charge.
		/// </summary>
		[JsonProperty("rate")]
		[GraphQLField("rate", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Decimal)]
		public decimal Rate { get; set; }

		/// <summary>
		/// Tax amount charged on the fee.
		/// </summary>
		[JsonProperty("taxAmount")]
		[GraphQLField("taxAmount", GraphQLConstants.DataType.Object, typeof(Money))]
		public Money TaxAmount { get; set; }

		/// <summary>
		/// Name of the type of fee.
		/// </summary>
		[JsonProperty("type")]
		[GraphQLField("type", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Type { get; set; }
	}
}
