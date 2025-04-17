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
	/// <summary>
	/// Represents line items belonging to a fulfillment order
	/// </summary>
	public class FulfillmentLineItem : BCAPIEntity
	{
		// <summary>
		/// The ID of the fulfillment order line item.
		/// </summary>
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Id)]
		public long? Id { get; set; }

		/// <summary>
		/// The ID of the shop that's associated with the fulfillment order.
		/// </summary>
		[JsonProperty("shop_id", NullValueHandling = NullValueHandling.Ignore)]
		public long? ShopId { get; set; }

		/// <summary>
		/// The ID of the fulfillment order associated with this line item.
		/// </summary>
		[JsonProperty("fulfillment_order_id", NullValueHandling = NullValueHandling.Ignore)]
		public long? FulfillmentOrderId { get; set; }

		/// <summary>
		/// The ID of the line item associated with this fulfillment order line item.
		/// </summary>
		[JsonProperty("line_item_id", NullValueHandling = NullValueHandling.Ignore)]
		public long? LineItemId { get; set; }

		/// <summary>
		///  The ID of the inventory item associated with this fulfillment order line item.
		/// </summary>
		[JsonProperty("inventory_item_id", NullValueHandling = NullValueHandling.Ignore)]
		public long? InventoryItemId { get; set; }

		/// <summary>
		/// The total number of units to be fulfilled.
		/// </summary>
		[JsonProperty("quantity")]
		[CommerceDescription(ShopifyCaptions.Quantity)]
		public int? Quantity { get; set; }

		/// <summary>
		/// The number of units remaining to be fulfilled.
		/// </summary>
		[JsonProperty("fulfillable_quantity")]
		[CommerceDescription(ShopifyCaptions.FulfillableQuantity)]
		public int? FulfillableQuantity { get; set; }

		// <summary>
		/// The ID of the variant associated with this fulfillment order line item.
		/// </summary>
		[JsonProperty("variant_id", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.VariantId)]
		public long? VariantId { get; set; }
	}
}
