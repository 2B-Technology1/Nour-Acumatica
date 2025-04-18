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
using PX.Commerce.BigCommerce.API.REST;
using Newtonsoft.Json;
using PX.Commerce.Core;
using System.ComponentModel;

namespace PX.Commerce.BigCommerce.API.REST
{
	[CommerceDescription(BigCommerceCaptions.Shipment, FieldFilterStatus.Filterable, FieldMappingStatus.Skipped)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class ShipmentData : BCAPIEntity
	{
		public ShipmentData()
	{
			OrdersShipmentDataList = new List<OrdersShipmentData>();
		}

		[CommerceDescription(BigCommerceCaptions.ShipmentData, FieldFilterStatus.Skipped, FieldMappingStatus.Export)]
		[JsonIgnore]
		public List<OrdersShipmentData> OrdersShipmentDataList { get; set; }

		/// <summary>
		/// Existing extern shipments need to remove before creating the same.
		/// </summary>
		[JsonIgnore]
		public Dictionary<string, string> ExternShipmentsToRemove { get; set; } = new Dictionary<string, string>();

		/// <summary>
		/// Existing extern order status need to update if Shipments are created but order in Acumatica has been deleted.
		/// </summary>
		[JsonIgnore]
		public Dictionary<string, string> ExternOrdersToUpdate { get; set; } = new Dictionary<string, string>();
	}

	[CommerceDescription(BigCommerceCaptions.ShipmentData, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class OrdersShipmentData : BCAPIEntity
	{
        public OrdersShipmentData()
        {
			ShipmentItems = new List<OrdersShipmentItem>();
		}

		public OrdersShipmentData Clone(bool? DeepClone = false)
        {
			OrdersShipmentData copyObj = (OrdersShipmentData)this.MemberwiseClone();
			List<OrdersShipmentItem> newItems = new List<OrdersShipmentItem>();
			if (DeepClone == true)
			{
				foreach (OrdersShipmentItem item in this.ShipmentItems)
				{
					newItems.Add(item.Clone());
				}
				copyObj.ShipmentItems = newItems;
			}
			return copyObj;
		}

		/// <summary>
		/// The ID of the shipment.
		/// </summary>
		[JsonProperty("id")]
		[CommerceDescription(BigCommerceCaptions.ShipmentID, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public virtual int? Id { get; set; }
		public bool ShouldSerializeId()
		{
			return  false;
		}
		/// <summary>
		/// The ID of the customer that placed the order.
		/// </summary>
		[JsonProperty("customer_id")]
        public virtual int CustomerId { get; set; }
		public bool ShouldSerializeCustomerId()
		{
			return false;
		}

		[JsonProperty("date_created")]
		[CommerceDescription(BigCommerceCaptions.DateShipped, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public virtual string DateCreatedUT { get; set; }
		public bool ShouldSerializeDateCreatedUT()
		{
			return false;
		}

		/// <summary>
		/// The tracking number for the shipment.
		///  string(50)
		/// </summary>
		[JsonProperty("tracking_number")]
		[CommerceDescription(BigCommerceCaptions.TrackingID, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public virtual string TrackingNumber { get; set; }

		/// <summary>
		/// Extra detail to describe the shipment, with values like: Standard, My Custom Shipping Method Name, etc. 
		/// Can also be used for live quotes from some shipping providers.
		/// string(100)
		/// </summary>
		[JsonProperty("shipping_method")]
		[CommerceDescription(BigCommerceCaptions.ShippingMethod, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public virtual string ShippingMethod { get; set; }

		/// <summary>
		/// Enum of the BigCommerce shipping-carrier integration/module. 
		/// (Note: This property should be included in a POST request to create a shipment object. 
		/// If it is omitted from the request, the property's value will default to custom, and no tracking link will be generated in the email. 
		/// To avoid this behavior, you can pass the property as an empty string.)
		///  string(50)
		/// </summary>
		[JsonProperty("shipping_provider")]
		[CommerceDescription(BigCommerceCaptions.ShippingProvider, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public virtual string ShippingProvider { get; set; }

		/// <summary>
		/// Optional, but if you include it, its value must refer/map to the same carrier service as the shipping_provider
		/// string(100)
		/// </summary>
		[JsonProperty("tracking_carrier")]
		[CommerceDescription(BigCommerceCaptions.TrackingCarrier, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public virtual string TrackingCarrier { get; set; }

		/// <summary>
		/// The ID of the order this shipment is associated with.
		/// </summary>
		[JsonProperty("order_id")]
        public virtual int? OrderId { get; set; }
		public bool ShouldSerializeOrderId()
		{
			return false;
		}

		[JsonProperty("order_date")]
		[CommerceDescription(BigCommerceCaptions.OrderDate, FieldFilterStatus.Skipped, FieldMappingStatus.Skipped)]
		public virtual string OrderDateUT { get; set; }
		public bool ShouldSerializeOrderDateUT()
		{
			return false;
		}

		/// <summary>
		/// Any comments the store owner has added regarding the shipment.
		/// 
		/// text
		/// </summary>
		[JsonProperty("comments")]
		[CommerceDescription(BigCommerceCaptions.PackingSlipNotes, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public virtual string Comments { get; set; }

        /// <summary>
        /// The ID of the order address this shipment is associated with.
        /// </summary>
        [JsonProperty("order_address_id")]
        public virtual int OrderAddressId { get; set; }

        /// <summary>
        /// The billing address of the order. 
        /// </summary>
        [JsonProperty("billing_address")]
		[CommerceDescription(BigCommerceCaptions.BillingAddress, FieldFilterStatus.Skipped, FieldMappingStatus.Skipped)]
		public virtual OrderAddressData BillingAddress { get; set; }
		public bool ShouldSerializeBillingAddress()
		{
			return false;
		}

		/// <summary>
		/// The shipping address of the shipment. 
		/// </summary>
		[JsonProperty("shipping_address")]
		[CommerceDescription(BigCommerceCaptions.ShippingTo, FieldFilterStatus.Skipped, FieldMappingStatus.Skipped)]
		public virtual OrderAddressData ShippingAddress { get; set; }
		public bool ShouldSerializeShippingAddress()
		{
			return false;
		}

		/// <summary>
		/// The items in the shipment. 
		/// </summary>
		[JsonProperty("items")]
		[CommerceDescription(BigCommerceCaptions.ShipmentItems, FieldFilterStatus.Skipped, FieldMappingStatus.Skipped)]
		public virtual IList<OrdersShipmentItem> ShipmentItems { get; set; }

		[JsonIgnore]
		public virtual Guid? OrderLocalID { get; set; }

		[JsonIgnore]
		public virtual String ShipmentType { get; set; }
    }
}
