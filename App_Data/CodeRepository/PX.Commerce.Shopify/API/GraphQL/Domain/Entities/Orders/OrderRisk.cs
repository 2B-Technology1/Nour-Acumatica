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
	public class OrderRiskGQL: BCAPIEntity
	{

		/// <summary>
		/// Whether the order risk is displayed on the order details page in the Shopify admin. 
		/// If false, then this order risk is ignored when Shopify determines your app's overall risk level for the order.
		/// </summary>
		[JsonProperty("display")]
		[GraphQLField("display", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool? Display { get; set; }

		/// <summary>
		/// The message that's displayed to the merchant to indicate the results of the fraud check. 
		/// The message is displayed only if display is set totrue.
		/// </summary>
		[JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("message", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Message { get; set; }

		/// <summary>
		/// The mapping between restful API field Recommendation:
		/// accept:low, investigate:medium, cancel:high
		/// HIGH: There is a high level of risk that this order is fraudulent.
		/// LOW: There is a low level of risk that this order is fraudulent.
		/// MEDIUM: There is a medium level of risk that this order is fraudulent.
		/// </summary>
		[JsonProperty("level")]
		[GraphQLField("level", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Level { get; set; }

	}
}
