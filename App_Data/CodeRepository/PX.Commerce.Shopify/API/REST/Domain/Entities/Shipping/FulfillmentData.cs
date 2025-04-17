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
	[CommerceDescription(ShopifyCaptions.ShipmentData, FieldFilterStatus.Filterable, FieldMappingStatus.Skipped)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class ShipmentData : BCAPIEntity
	{
		public ShipmentData()
		{
			FulfillmentDataList = new List<FulfillmentData>();
		}

		[CommerceDescription(ShopifyCaptions.Fulfillment, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		[JsonIgnore]
		public List<FulfillmentData> FulfillmentDataList { get; set; }

		/// <summary>
		/// Existing extern shipments need to remove before creating new shipment.
		/// </summary>
		[JsonIgnore]
		public Dictionary<string, string> ExternShipmentsToRemove { get; set; } = new Dictionary<string, string>();
	}
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class FulfillmentResponse : IEntityResponse<FulfillmentData>
	{
		[JsonProperty("fulfillment")]
		public FulfillmentData Data { get; set; }
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class FulfillmentsResponse : IEntitiesResponse<FulfillmentData>
	{
		[JsonProperty("fulfillments")]
		public IEnumerable<FulfillmentData> Data { get; set; }
	}

	/// <summary>
	/// Fulfillment resource can be used to view and manage fulfillments for an order or a fulfillment order.
	/// A fulfillment order represents a group of one or more items in an order that will be fulfilled from <b>the same location</b>.
	/// A fulfillment represents work that is completed as part of a fulfillment order and can include one or more items.
	/// </summary>
	/// <remarks> The Fulfillment resource will be deprecated. Use the FulfillmentOrder resource instead. </remarks>
	[JsonObject(Description = "Fulfillment")]
	[CommerceDescription(ShopifyCaptions.Fulfillment, FieldFilterStatus.Filterable, FieldMappingStatus.Export)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class FulfillmentData : BCAPIEntity
	{
		public FulfillmentData()
		{

		}
		public FulfillmentData Clone(bool? DeepClone = false)
		{
			FulfillmentData copyObj = (FulfillmentData)this.MemberwiseClone();
			List<OrderLineItem> newItems = new List<OrderLineItem>();
			if (DeepClone == true)
			{
				foreach (OrderLineItem item in this.LineItems)
				{
					newItems.Add(item.Clone());
				}
				copyObj.LineItems = newItems;
			}
			return copyObj;
		}

		/// <summary>
		/// The date and time when the fulfillment was created. The API returns this value in ISO 8601 format.
		/// </summary>
		[JsonProperty("created_at", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.DateCreated)]
		[ShouldNotSerialize]
		public DateTime? DateCreatedAt { get; set; }

		/// <summary>
		/// The ID for the fulfillment.
		/// </summary>
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Id)]
		public long? Id { get; set; }

		/// <summary>
		/// A historical record of each item in the fulfillment
		/// </summary>
		[JsonProperty("line_items", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.LineItem, FieldFilterStatus.Skipped, FieldMappingStatus.Skipped)]
		public List<OrderLineItem> LineItems { get; set; }

		/// <summary>
		/// The unique identifier of the location that the fulfillment should be processed for. 
		/// </summary>
		[JsonProperty("location_id", NullValueHandling = NullValueHandling.Ignore)]
		public long? LocationId { get; set; }

		/// <summary>
		/// The uniquely identifying fulfillment name, consisting of two parts separated by a .. 
		/// The first part represents the order name and the second part represents the fulfillment number. 
		/// The fulfillment number automatically increments depending on how many fulfillments are in an order (e.g. #1001.1, #1001.2).
		/// </summary>
		[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Name, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public string Name { get; set; }

		/// <summary>
		/// Whether the customer should be notified. If set to true, then an email will be sent when the fulfillment is created or updated. 
		/// For orders that were initially created using the API, the default value is false. For all other orders, the default value is true.
		/// </summary>
		[JsonProperty("notify_customer", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.NotifyCustomer, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public bool? NotifyCustomer { get; set; }

		/// <summary>
		/// The unique numeric identifier for the order.
		/// </summary>
		[JsonProperty("order_id", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.OrderId, FieldFilterStatus.Skipped, FieldMappingStatus.Skipped)]
		public long? OrderId { get; set; }


		/// <summary>
		/// A text field that provides information about the receipt
		/// </summary>
		[JsonProperty("origin_address", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.OrderAddress, FieldFilterStatus.Skipped, FieldMappingStatus.Skipped)]
		public FulfillmentOriginAddress OriginAddress { get; set; }

		/// <summary>
		/// A text field that provides information about the receipt
		/// </summary>
		[JsonProperty("receipt", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Receipt, FieldFilterStatus.Skipped, FieldMappingStatus.Skipped)]
		public FulfillmentReceipt Receipt { get; set; }

		/// <summary>
		/// The type of service used.
		/// </summary>
		[JsonProperty("service", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Service, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public string Service { get; set; }

		/// <summary>
		/// The current shipment status of the fulfillment.
		/// Usually a <see cref="REST.ShipmentStatus"/>.
		/// </summary>
		[JsonProperty("shipment_status", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.ShipmentStatus, FieldFilterStatus.Skipped, FieldMappingStatus.Skipped)]
		public string ShipmentStatus { get; set; }

		/// <summary>
		/// The status of the fulfillment.
		/// </summary>
		[JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Status, FieldFilterStatus.Skipped, FieldMappingStatus.Skipped)]
		public FulfillmentStatus? Status { get; set; }

		/// <inheritdoc cref="TrackingInfo"/>
		[JsonProperty("tracking_info", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.TrackingInfo, FieldFilterStatus.Skipped, FieldMappingStatus.Skipped)]
		public TrackingInfo TrackingInfo { get; set; }
		#region TrackingInfo accessors
		//In order to expose these fields to mapping, we should give accessors to Tracking Info.
		/// <summary>
		/// Assessor for <see cref="TrackingInfo.Number"/>.
		/// </summary>
		[CommerceDescription(ShopifyCaptions.TrackingNumber, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		[ShouldNotSerialize]
		public string TrackingInfoNumber { get => TrackingInfo?.Number;
			set
			{
				if (TrackingInfo == null)
					TrackingInfo = new TrackingInfo() { Number = value };
				else
					TrackingInfo.Number = value;
			}
		}

		/// <summary>
		/// Assessor for <see cref="TrackingInfo.Company"/>.
		/// </summary>
		[JsonProperty("tracking_company", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.TrackingCompany, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public string TrackingCompany
		{
			get => TrackingInfo?.Company;
			set
			{
				if (TrackingInfo == null)
					TrackingInfo = new TrackingInfo() { Company = value };
				else
					TrackingInfo.Company = value;
			}
		}

		/// <summary>
		/// Assessor for <see cref="TrackingInfo.URL"/>.
		/// </summary>
		[CommerceDescription(ShopifyCaptions.TrackingUrl, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		[ShouldNotSerialize]
		public string TrackingURL
		{
			get => TrackingInfo?.URL;
			set
			{
				if (TrackingInfo == null)
					TrackingInfo = new TrackingInfo() { URL = value };
				else
					TrackingInfo.URL = value;
			}
		}
		#endregion

		/// <summary>
		/// A list of tracking numbers, provided by the shipping company.
		/// </summary>
		/// <remarks>This field is not used to define the tracking numbers for a new fulfillment.
		/// For that it is used <see cref="TrackingInfo.Number"/>.</remarks>
		[JsonProperty("tracking_numbers", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.TrackingNumbers, FieldFilterStatus.Skipped, FieldMappingStatus.Skipped)]
		public List<string> TrackingNumbers { get; set; }

		/// <summary>
		/// The URLs of tracking pages for the fulfillment.
		/// </summary>
		/// <remarks>This field is not used to define the url for a new fulfillment. For that it is used <see cref="TrackingInfo.URL"/>.</remarks>
		[JsonProperty("tracking_urls", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.TrackingUrls, FieldFilterStatus.Skipped, FieldMappingStatus.Skipped)]
		public List<string> TrackingUrls { get; set; }

		/// <summary>
		/// The date and time (ISO 8601 format) when the fulfillment was last modified..
		/// </summary>
		[JsonProperty("updated_at")]
		[ShouldNotSerialize]
		public DateTime? DateModifiedAt { get; set; }

		/// <summary>
		/// The name of the inventory management service.
		/// </summary>
		[JsonProperty("variant_inventory_management", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.InventoryManagement, FieldFilterStatus.Skipped, FieldMappingStatus.Skipped)]
		[ShouldNotSerialize]
		public string InventoryManagement { get; set; }

		[JsonIgnore]
		public virtual Guid? OrderLocalID { get; set; }

		[JsonIgnore]
		public virtual String ShipmentType { get; set; }

		[JsonProperty("line_items_by_fulfillment_order", NullValueHandling = NullValueHandling.Ignore)]
		public List<LineItemsByFulfillmentOrder> ItemsByFulfillmentOrder { get; set; }

		[JsonIgnore]
		public virtual bool? ShouldBeExported { get; set; }
	}

	public class FulfillmentReceipt
	{
		/// <summary>
		/// Whether the fulfillment was a testcase.
		/// </summary>
		[JsonProperty("testcase")]
		[CommerceDescription(ShopifyCaptions.TestCase, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public bool? TestCase { get; set; }

		/// <summary>
		/// authorization: The authorization code.
		/// </summary>
		[JsonProperty("authorization")]
		[CommerceDescription(ShopifyCaptions.Authorization, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public string Authorization { get; set; }
	}

	/// <summary>
	/// The tracking information used by Shopify to create or update fulfillments.
	/// </summary>
	public class TrackingInfo
	{
		/// <summary>
		/// The tracking number for the fulfillment.
		/// </summary>
		[JsonProperty("number", NullValueHandling = NullValueHandling.Ignore)]		
		public string Number { get; set; }

		/// <summary>
		/// The URL to track the fulfillment.
		/// </summary>
		[JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
		public string URL { get; set; }

		/// <summary>
		/// The name of the tracking company.
		/// </summary>
		[JsonProperty("company", NullValueHandling = NullValueHandling.Ignore)]
		public string Company { get; set; }
	}

	public abstract class FulfillmentBaseAddress
	{
		/// <summary>
		/// The street address of the fulfillment location.
		/// </summary>
		[JsonProperty("address1", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.AddressLine1, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public string Address1 { get; set; }

		/// <summary>
		/// The second line of the address. Typically the number of the apartment, suite, or unit.
		/// </summary>
		[JsonProperty("address2", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.AddressLine2, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public string Address2 { get; set; }

		/// <summary>
		/// The city of the fulfillment location.
		/// </summary>
		[JsonProperty("city", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.City, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public string City { get; set; }

		/// <summary>
		/// The zip code of the fulfillment location.
		/// </summary>
		[JsonProperty("zip", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Zipcode, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public string Zip { get; set; }
	}

	public class FulfillmentOriginAddress : FulfillmentBaseAddress
	{
		/// <summary>
		/// The country of the fulfillment location
		/// </summary>
		[JsonProperty("country_code", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.CountryISOCode, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public string CountryCode { get; set; }

		/// <summary>
		/// The province of the fulfillment location.
		/// </summary>
		[JsonProperty("province_code", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.ProvinceCode, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		public string ProvinceCode { get; set; }
	}

	public class LineItemsByFulfillmentOrder
	{		
		[JsonProperty("fulfillment_order_id", NullValueHandling = NullValueHandling.Ignore)]
		public long? FulfillmentOrderId { get; set; }

		/// <summary>
		/// Represents line items belonging to a fulfillment order.
		/// </summary>
		[JsonProperty("fulfillment_order_line_items", NullValueHandling = NullValueHandling.Ignore)]
		public List<OrderLineItem> FulfillmentOrderLineItems { get; set; }
	}

}
