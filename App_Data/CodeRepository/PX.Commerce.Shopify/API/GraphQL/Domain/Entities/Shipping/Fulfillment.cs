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
	/// <summary>
	/// Represents a fulfillment. In Shopify, a fulfillment represents a shipment of one or more items in an order.
	/// When an order has been completely fulfilled, it means that all the items that are included in the order have been sent to the customer. There can be more than one fulfillment for an order.
	/// </summary>
	public class FulfillmentGQL : BCAPIEntity, INode
	{
		/// <summary>
		/// A globally-unique identifier.
		/// </summary>
		[JsonProperty("id")]
		[GraphQLField("id", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.ID)]
		public string Id { get; set; }

		/// <summary>
		/// Date and time when the fulfillment was created.
		/// </summary>
		[JsonProperty("createdAt", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("createdAt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.DateTime)]
		public DateTime? DateCreatedAt { get; set; }

		/// <summary>
		/// The status of the fulfillment.
		/// </summary>
		[JsonProperty("status")]
		[GraphQLField("status", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Status { get; set; }

		/// <summary>
		/// The date and time when the fulfillment was last updated.
		/// </summary>
		[JsonProperty("updatedAt", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("updatedAt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.DateTime)]
		public DateTime? UpdatedAt { get; set; }
	}
}
