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
	/// Represents the location where the physical good resides.
	/// </summary>
	public class LocationGQL: BCAPIEntity, INode
	{
		/// <summary>
		/// Whether this location can be reactivated.
		/// </summary>
		[JsonProperty("activatable")]
		[GraphQLField("activatable", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool Activatable { get; set; }

		/// <summary>
		/// The address of this location.
		/// </summary>
		[JsonProperty("address")]
		[GraphQLField("address", GraphQLConstants.DataType.Object, typeof(LocationAddressGQL))]
		public LocationAddressGQL Address { get; set; }

		/// <summary>
		/// Whether the location address has been verified.
		/// </summary>
		[JsonProperty("addressVerified")]
		[GraphQLField("addressVerified", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool AddressVerified { get; set; }

		/// <summary>
		/// Whether this location can be deactivated.
		/// </summary>
		[JsonProperty("deactivatable")]
		[GraphQLField("deactivatable", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool Deactivatable { get; set; }

		/// <summary>
		/// The date and time (ISO 8601 format) that the location was deactivated at. For example, 3:30 pm on September 7, 2019 in the time zone of UTC (Universal Time Coordinated) is represented as "2019-09-07T15:50:00Z".
		/// </summary>
		[JsonProperty("deactivatedAt")]
		[GraphQLField("deactivatedAt", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public DateTime? DeactivatedAt { get; set; }

		/// <summary>
		/// Whether this location can be deleted.
		/// </summary>
		[JsonProperty("deletable")]
		[GraphQLField("deletable", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool Deletable { get; set; }

		/// <summary>
		/// Whether this location can fulfill online orders.
		/// </summary>
		[JsonProperty("fulfillsOnlineOrders")]
		[GraphQLField("fulfillsOnlineOrders", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool FulfillsOnlineOrders { get; set; }

		/// <summary>
		/// Whether this location has active inventory.
		/// </summary>
		[JsonProperty("hasActiveInventory")]
		[GraphQLField("hasActiveInventory", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool HasActiveInventory { get; set; }

		/// <summary>
		/// Whether this location has orders that need to be fulfilled.
		/// </summary>
		[JsonProperty("hasUnfulfilledOrders")]
		[GraphQLField("hasUnfulfilledOrders", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool HasUnfulfilledOrders { get; set; }

		///<inheritdoc/>
		[JsonProperty("id")]
		[GraphQLField("id", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.ID)]
		public string Id { get; set; }

		/// <summary>
		/// Whether the location is active.
		/// </summary>
		[JsonProperty("isActive")]
		[GraphQLField("isActive", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool IsActive { get; set; }

		/// <summary>
		/// The name of the location.
		/// </summary>
		[JsonProperty("name")]
		[GraphQLField("name", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.String)]
		public string Name { get; set; }

		/// <summary>
		/// The ID of the corresponding resource in the REST Admin API.
		/// </summary>
		[JsonProperty("legacyResourceId")]
		[GraphQLField("legacyResourceId", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Int)]
		public long? LegacyResourceId { get; set; }

		/// <summary>
		/// Whether this location is used for calculating shipping rates. In multi-origin shipping mode, this flag is ignored.
		/// </summary>
		[JsonProperty("shipsInventory")]
		[GraphQLField("shipsInventory", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool ShipsInventory { get; set; }
	}
}
