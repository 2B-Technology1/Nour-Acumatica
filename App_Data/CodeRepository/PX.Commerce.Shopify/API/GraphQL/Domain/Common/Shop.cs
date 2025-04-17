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
using System;

namespace PX.Commerce.Shopify.API.GraphQL
{
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class ShopResponse : IEntityResponse<ShopGQL>
	{
		[JsonProperty("shop")]
		public ShopGQL TEntityData { get; set; }
	}

	/// <summary>
	/// Represents information about a shop
	/// </summary>
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	[GraphQLObject(NodeName = "shop")]
	public class ShopGQL : BCAPIEntity, INode
	{
		/// <summary>
		/// The three letter code for the currency that the shop sells in.
		/// </summary>
		[JsonProperty("currencyCode")]
		[GraphQLField("currencyCode", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string CurrencyCode { get; set; }

		/// <summary>
		/// The shop owner's email address. Shopify will use this email address to communicate with the shop owner.
		/// </summary>
		[JsonProperty("email")]
		[GraphQLField("email", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Email { get; set; }

		///<inheritdoc/>
		[JsonProperty("id")]
		[GraphQLField("id", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.ID)]
		public string Id { get; set; }

		/// <summary>
		/// The shop's name.
		/// </summary>
		[JsonProperty("name")]
		[GraphQLField("name", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Name { get; set; }

		/// <summary>
		/// The prefix that appears before order numbers.
		/// </summary>
		[JsonProperty("orderNumberFormatPrefix")]
		[GraphQLField("orderNumberFormatPrefix", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string OrderNumberFormatPrefix { get; set; }

		/// <summary>
		/// The suffix that appears before order numbers.
		/// </summary>
		[JsonProperty("orderNumberFormatSuffix")]
		[GraphQLField("orderNumberFormatSuffix", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string OrderNumberFormatSuffix { get; set; }

		/// <summary>
		/// The URL of the shop's online store.
		/// </summary>
		[JsonProperty("url")]
		[GraphQLField("url", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Url { get; set; }

		/// <summary>
		/// The presentment currency settings for the shop excluding the shop's own currency.
		/// </summary>
		[JsonProperty("currencySettings")]
		[GraphQLField("currencySettings", GraphQLConstants.DataType.Connection, typeof(CurrencySettingGQL))]
		public Connection<CurrencySettingGQL> CurrencySettings { get; set; }
	}
}
