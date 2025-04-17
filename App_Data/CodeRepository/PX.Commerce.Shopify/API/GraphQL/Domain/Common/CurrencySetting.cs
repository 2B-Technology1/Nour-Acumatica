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
using System;
using System.Collections.Generic;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// A setting for a presentment currency.
	/// </summary>
	public class CurrencySettingGQL : BCAPIEntity
	{
		/// <summary>
		/// The currency's ISO code.
		/// </summary>
		[JsonProperty("currencyCode")]
		[GraphQLField("currencyCode", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string CurrencyCode { get; set; }

		/// <summary>
		/// The full name of the currency.
		/// </summary>
		[JsonProperty("currencyName")]
		[GraphQLField("currencyName", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string CurrencyName { get; set; }

		/// <summary>
		/// Whether the currency is enabled or not. An enabled currency setting is visible to buyers and allows orders to be generated with that currency as presentment.
		/// </summary>
		[JsonProperty("enabled")]
		[GraphQLField("enabled", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool Enabled { get; set; }

		/// <summary>
		/// The date and time when the active exchange rate for the currency was last modified. It can be the automatic rate's creation date, or the manual rate's last updated at date if active.
		/// </summary>
		[JsonProperty("rateUpdatedAt")]
		[GraphQLField("rateUpdatedAt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.DateTime)]
		public DateTime? RateUpdatedAt { get; set; }
	}
}
