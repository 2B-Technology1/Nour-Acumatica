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

namespace PX.Commerce.Shopify.API.GraphQL
{
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PriceListPriceGQL : BCAPIEntity
	{
		public class Arguments
		{
			[GraphQLArgument("originType", "PriceListPriceOriginType")]
			public abstract class OriginType { }
		}

		/// <summary>
		/// The currency for fixed prices associated with this price list.
		/// </summary>
		[JsonProperty("compareAtPrice")]
		[GraphQLField("compareAtPrice", GraphQLConstants.DataType.Object, typeof(Money))]
		public Money CompareAtPrice { get; set; }

		/// <summary>
		/// The origin of a price, either fixed (defined on the price list) or relative (calculated using a price list adjustment configuration).
		/// </summary>
		[JsonProperty("originType")]
		[GraphQLField("originType", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string OriginType { get; set; }

		/// <summary>
		/// The price of the product variant on this price list.
		/// </summary>
		[JsonProperty("price")]
		[GraphQLField("price", GraphQLConstants.DataType.Object, typeof(Money))]
		public Money Price { get; set; }

		/// <summary>
		/// The product variant associated with this price.
		/// </summary>
		[JsonProperty("variant")]
		[GraphQLField("variant", GraphQLConstants.DataType.Object, typeof(ProductVariantGQL))]
		public ProductVariantGQL Variant { get; set; }
	}
}
