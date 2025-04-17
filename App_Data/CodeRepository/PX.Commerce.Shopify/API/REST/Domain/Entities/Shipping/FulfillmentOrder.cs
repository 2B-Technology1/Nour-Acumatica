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

namespace PX.Commerce.Shopify.API.REST
{
	public class FulfillmentOrderResponse : IEntityResponse<FulfillmentOrder>
	{
		[JsonProperty("fulfillment_order")]
		public FulfillmentOrder Data { get; set; }
	}

	public class FulfillmentOrdersResponse : IEntitiesResponse<FulfillmentOrder>
	{
		[JsonProperty("fulfillment_orders")]
		public IEnumerable<FulfillmentOrder> Data { get; set; }
	}

	/// <summary>
	/// The FulfillmentOrder resource represents either an item or a group of items in an order that are to be fulfilled from <b>the same location</b>.
	/// There can be more than one fulfillment order for an order at a given location.
	/// </summary>
	[JsonObject(Description = "fulfillment_order")]
	[CommerceDescription(ShopifyCaptions.FulfillmentOrder, FieldFilterStatus.Filterable, FieldMappingStatus.Export)]
	public class FulfillmentOrder : BCAPIEntity
	{
		/// <summary>
		/// The ID of the location that has been assigned to do the work.
		/// </summary>
		[JsonProperty("assigned_location_id", NullValueHandling = NullValueHandling.Ignore)]
		public long? AssignedLocationId { get; set; }

		/// <summary>
		/// The destination where the items should be sent. 
		/// </summary>
		[JsonProperty("destination", NullValueHandling = NullValueHandling.Ignore)]
		public FulfillmentDestinationAddress Destination { get; set; }

		/// <summary>
		/// The type of method used to transfer a product or service to a customer.
		/// </summary>
		[JsonProperty("delivery_method", NullValueHandling = NullValueHandling.Ignore)]
		public FulfillmentDeliveryMethod DeliveryMethod { get; set; }

		/// <summary>
		/// The date and time at which the fulfillment order will be fulfillable.
		/// When this date and time is reached, a scheduled fulfillment order is automatically transitioned to open.
		/// For more information about fulfillment statuses, refer to the <b>status</b> property.
		/// </summary>
		[JsonProperty("fulfill_at", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.FulfillmentAt)]
		public DateTime? FulfillAt { get; set; }

		/// <summary>
		/// The latest date and time by which all items in the fulfillment order need to be fulfilled.
		/// </summary>
		[JsonProperty("fulfill_by", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.FulfillmentBy)]
		public DateTime? FulfillBy { get; set; }

		/// <summary>
		/// Represents the fulfillment holds applied on the fulfillment order.
		/// </summary>
		[JsonProperty("fulfillment_holds", NullValueHandling = NullValueHandling.Ignore)]
		public List<FulfillmentHoldsDescription> FulfillmentHolds { get; set; }

		/// <summary>
		/// An ID for the fulfillment order.
		/// </summary>
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Id)]
		public long? Id { get; set; }

		/// <summary>
		/// Represents line items belonging to a fulfillment order.
		/// </summary>
		[JsonProperty("line_items", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Id)]
		public List<FulfillmentLineItem> LineItems { get; set; }

		/// <summary>
		/// An ID for the fulfillment order.
		/// </summary>
		[JsonProperty("order_id", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.OrderId, FieldFilterStatus.Skipped, FieldMappingStatus.Skipped)]
		public long? OrderId { get; set; }

		/// <summary>
		/// The request status of the fulfillment order.
		/// </summary>
		[JsonProperty("request_status", NullValueHandling = NullValueHandling.Ignore)]
		public FulfillmentOrderRequestStatus RequestStatus { get; set; }

		/// <summary>
		/// The ID of the shop that's associated with the fulfillment order.
		/// </summary>
		[JsonProperty("shop_id", NullValueHandling = NullValueHandling.Ignore)]
		public long? ShopId { get; set; }

		/// <summary>
		/// The status of the fulfillment order.
		/// </summary>
		[JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Status)]
		public FulfillmentOrderStatus Status { get; set; }

		/// <summary>
		/// The actions that can be performed on this fulfillment order.
		/// </summary>
		[JsonProperty("supported_actions", NullValueHandling = NullValueHandling.Ignore)]
		public List<string> SupportedActions { get; set; }

		/// <summary>
		/// The fulfillment order's assigned location. This is the location expected to perform fulfillment.
		/// </summary>
		[JsonProperty("assigned_location", NullValueHandling = NullValueHandling.Ignore)]
		public FulfillmentOrderAssingnedLocation AssignedLocation { get; set; }

		[JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
		public string Message { get; set; }

		[JsonProperty("new_location_id", NullValueHandling = NullValueHandling.Ignore)]
		public long? NewLocationId { get; set; }
	}

	public class FulfillmentDestinationAddress : FulfillmentBaseAddress
	{
		/// <summary>
		/// The ID of the fulfillment order destination.
		/// </summary>
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Id)]
		public long? Id { get; set; }

		/// <summary>
		/// The company of the destination.
		/// </summary>
		[JsonProperty("company")]
		[CommerceDescription(ShopifyCaptions.CompanyName, FieldFilterStatus.Skipped, FieldMappingStatus.Skipped)]
		public string Company { get; set; }

		/// <summary>
		/// The country of the destination.
		/// </summary>
		[JsonProperty("country")]
		[CommerceDescription(ShopifyCaptions.CountryName, FieldFilterStatus.Skipped, FieldMappingStatus.Skipped)]
		public string Country { get; set; }

		/// <summary>
		/// The email of the customer at the destination.
		/// </summary>
		[JsonProperty("email")]
		[CommerceDescription(ShopifyCaptions.Email, FieldFilterStatus.Skipped, FieldMappingStatus.Skipped)]
		public string Email { get; set; }

		/// <summary>
		/// The first name of the customer at the destination.
		/// </summary>
		[JsonProperty("first_name")]
		[CommerceDescription(ShopifyCaptions.FirstName, FieldFilterStatus.Skipped, FieldMappingStatus.Skipped)]
		public string FirstName { get; set; }

		/// <summary>
		/// The last name of the customer at the destination.
		/// </summary>
		[JsonProperty("last_name")]
		[CommerceDescription(ShopifyCaptions.LastName, FieldFilterStatus.Skipped, FieldMappingStatus.Skipped)]
		public string LastName { get; set; }

		/// <summary>
		/// The phone number of the customer at the destination.
		/// </summary>
		[JsonProperty("phone")]
		[CommerceDescription(ShopifyCaptions.PhoneNumber, FieldFilterStatus.Skipped, FieldMappingStatus.Skipped)]
		public string PhoneNumber { get; set; }

		/// <summary>
		/// The province of the destination.
		/// </summary>
		[JsonProperty("province")]
		[CommerceDescription(ShopifyCaptions.Province, FieldFilterStatus.Skipped, FieldMappingStatus.Skipped)]
		public string Province { get; set; }
	}

	public class FulfillmentOrderAssingnedLocation : FulfillmentBaseAddress
	{
		/// <summary>
		/// The two-letter code for the country of the assigned location.
		/// </summary>
		[JsonProperty("country_code", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.CountryISOCode, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public string CountryCode { get; set; }

		/// <summary>
		/// The ID of the assigned location.
		/// </summary>
		[JsonProperty("location_id", NullValueHandling = NullValueHandling.Ignore)]
		public long? LocationId { get; set; }

		/// <summary>
		/// The name of the assigned location.
		/// </summary>
		[JsonProperty("name")]
		[CommerceDescription(ShopifyCaptions.Name, FieldFilterStatus.Skipped, FieldMappingStatus.Skipped)]
		public string Name { get; set; }

		/// <summary>
		/// The phone number of the assigned location.
		/// </summary>
		[JsonProperty("phone")]
		[CommerceDescription(ShopifyCaptions.PhoneNumber, FieldFilterStatus.Skipped, FieldMappingStatus.Skipped)]
		public string PhoneNumber { get; set; }

		/// <summary>
		/// The province of the destination.
		/// </summary>
		[JsonProperty("province")]
		[CommerceDescription(ShopifyCaptions.Province, FieldFilterStatus.Skipped, FieldMappingStatus.Skipped)]
		public string Province { get; set; }
	}

	public class FulfillmentDeliveryMethod
	{
		/// <summary>
		/// The ID of the delivery method.
		/// </summary>
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Id)]
		public long? Id { get; set; }

		/// <summary>
		/// The type of delivery method.
		/// </summary>
		[JsonProperty("method_type", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.DeliveryMethod)]
		public FulfillmentOrderDeliveryMethodType MethodType { get; set; }

		/// <summary>
		/// The minimum date and time by which the delivery is expected to be completed. The API returns this value in ISO 8601 format.
		/// </summary>
		[JsonProperty("min_delivery_date_time", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.MinimumDeliveryDate)]
		[ShouldNotSerialize]
		public DateTime? MinDeliveryDate { get; set; }

		/// <summary>
		/// The maximum date and time by which the delivery is expected to be completed. The API returns this value in ISO 8601 format.
		/// </summary>
		[JsonProperty("max_delivery_date_time", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.MaximumDeliveryDate)]
		[ShouldNotSerialize]
		public DateTime? MaxDeliveryDate { get; set; }
	}

	[JsonObject(Description = "fulfillment_hold")]
	public class FulfillmentHoldsDescription
	{
		/// <summary>
		/// The reason for the fulfillment hold. 
		/// </summary>
		[JsonProperty("reason", NullValueHandling = NullValueHandling.Ignore)]
		public FulfillmentOrderReasons Reason { get; set; }

		/// <summary>
		/// Additional information about the fulfillment hold reason.
		/// </summary>
		[JsonProperty("reason_notes", NullValueHandling = NullValueHandling.Ignore)]
		public string ReasonNotes { get; set; }
	}
}
