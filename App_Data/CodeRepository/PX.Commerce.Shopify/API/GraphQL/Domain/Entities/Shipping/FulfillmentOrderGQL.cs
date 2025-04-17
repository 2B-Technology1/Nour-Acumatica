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
	/// The FulfillmentOrder resource represents either an item or a group of items in an order that are to be fulfilled from <b>the same location</b>.
	/// There can be more than one fulfillment order for an order at a given location.
	/// </summary>
	[JsonObject(Description = "fulfillment_order")]
	[CommerceDescription(ShopifyCaptions.FulfillmentOrder, FieldFilterStatus.Filterable, FieldMappingStatus.Export)]
	public class FulfillmentOrderGQL : BCAPIEntity, INode
	{
		/// <summary>
		/// The ID of the location that has been assigned to do the work.
		/// </summary>
		[JsonProperty("assignedLocation")]
		[GraphQLField("assignedLocation", GraphQLConstants.DataType.Object, typeof(FulfillmentOrderAssingnedLocationGQL))]
		public FulfillmentOrderAssingnedLocationGQL AssignedLocation { get; set; }

		/// <summary>
		/// A globally-unique identifier.
		/// </summary>
		[JsonProperty("id")]
		[GraphQLField("id", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.ID)]
		public string Id { get; set; }

		/// <summary>
		/// Date and time when the fulfillment order was created.
		/// </summary>
		[JsonProperty("createdAt", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("createdAt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.DateTime)]
		public DateTime? CreatedAt { get; set; }

		/// <summary>
		/// The date and time at which the fulfillment order will be fulfillable. When this date and time is reached, the scheduled fulfillment order is automatically transitioned to open.
		/// For example, the fulfill_at date for a subscription order might be the 1st of each month, a pre-order fulfill_at date would be nil, and a standard order fulfill_at date would be the order creation date.
		/// </summary>
		[JsonProperty("fulfillAt", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("fulfillAt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.DateTime)]
		public DateTime? FulfillAt { get; set; }

		/// <summary>
		/// The status of the fulfillment order.
		/// </summary>
		[JsonProperty("status")]
		[GraphQLField("status", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Status { get; set; }

		/// <summary>
		/// The date and time when the fulfillment order was last updated.
		/// </summary>
		[JsonProperty("updatedAt", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("updatedAt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.DateTime)]
		public DateTime? UpdatedAt { get; set; }

		/// <summary>
		/// A list of the fulfillment order's line items.
		/// </summary>
		[JsonProperty("lineItems", NullValueHandling = NullValueHandling.Ignore)]
		[GraphQLField("lineItems", GraphQLConstants.DataType.Connection, typeof(FulfillmentOrderLineItemGQL))]
		public Connection<FulfillmentOrderLineItemGQL> FulfillmentLineItems { get; set; }
	}
}
