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
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class InventoryLevelResponse : IEntityResponse<InventoryLevelData>
	{
		[JsonProperty("inventory_level")]
		public InventoryLevelData Data { get; set; }
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class InventoryLevelsResponse : IEntitiesResponse<InventoryLevelData>
	{
		[JsonProperty("inventory_levels")]
		public IEnumerable<InventoryLevelData> Data { get; set; }
	}

	[JsonObject(Description = ShopifyCaptions.InventoryLevel)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class InventoryLevelData : BCAPIEntity
	{
		/// <summary>
		/// The quantity of inventory items available for sale. Returns null if the inventory item is not tracked.
		/// </summary>
		[JsonProperty("available", NullValueHandling = NullValueHandling.Ignore)]
		public int? Available { get; set; }

		/// <summary>
		/// The amount to adjust the available inventory quantity. Send negative values to subtract from the current available quantity. 
		/// For example, "available_adjustment": 2 increases the current available quantity by 2, and "available_adjustment": -3 decreases the current available quantity by 3.
		/// </summary>
		[JsonProperty("available_adjustment", NullValueHandling = NullValueHandling.Ignore)]
		public int? AvailableAdjustment { get; set; }

		/// <summary>
		/// The ID of the inventory item that the inventory level belongs to.
		/// </summary>
		[JsonProperty("inventory_item_id")]
		public long? InventoryItemId { get; set; }

		/// <summary>
		/// The ID for the location.
		/// </summary>
		[JsonProperty("location_id")]
		public long? LocationId { get; set; }

		/// <summary>
		/// Whether inventory for any previously connected locations will be relocated. This property is ignored when no fulfillment service location is involved. 
		///(default: false)
		/// </summary>
		[JsonProperty("relocate_if_necessary", NullValueHandling = NullValueHandling.Ignore)]
		public bool? RelocateIfNecessary { get; set; }

		/// <summary>
		/// Whether inventory for any previously connected locations will be set to 0 and the locations disconnected. This property is ignored when no fulfillment service is involved.
		///(default: false)
		/// </summary>
		[JsonProperty("disconnect_if_necessary", NullValueHandling = NullValueHandling.Ignore)]
		public bool? DisconnectIfNecessary { get; set; }

		/// <summary>
		/// The date and time (ISO 8601 format) when the inventory level was last modified.
		/// </summary>
		[JsonProperty("updated_at")]
		[ShouldNotSerialize]
		public String DateModifiedAt { get; set; }

	}

}
