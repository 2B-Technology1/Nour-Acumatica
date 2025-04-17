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
	public class PriceListAdjustmentGQL : BCAPIEntity
	{
		/// <summary>
		/// The type of price adjustment, such as percentage increase or decrease.
		/// </summary>
		[JsonProperty("type")]
		[GraphQLField("type", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Type { get; set; }

		/// <summary>
		/// The value of price adjustment, where positive numbers reduce the prices and negative numbers increase them.
		/// </summary>
		[JsonProperty("value")]
		[GraphQLField("value", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Decimal)]
		public decimal? Value { get; set; }
	}
}
