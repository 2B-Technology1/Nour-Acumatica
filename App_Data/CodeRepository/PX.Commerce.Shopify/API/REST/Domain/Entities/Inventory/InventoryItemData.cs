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
	public class InventoryItemResponse : IEntityResponse<InventoryItemData>
	{
		[JsonProperty("inventory_item")]
		public InventoryItemData Data { get; set; }
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class InventoryItemsResponse : IEntitiesResponse<InventoryItemData>
	{
		[JsonProperty("inventory_items")]
		public IEnumerable<InventoryItemData> Data { get; set; }
	}

	[JsonObject(Description = ShopifyCaptions.InventoryLevel)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class InventoryItemData : BCAPIEntity
	{
		/// <summary>
		/// The date and time (ISO 8601 format) when the inventory item  was created.
		/// </summary>
		[JsonProperty("created_at")]
		[ShouldNotSerialize]
		public DateTime? DateCreatedAt { get; set; }

		/// <summary>
		/// The ID of the inventory item.
		/// </summary>
		[JsonProperty("id")]
		public long? Id { get; set; }

		/// <summary>
		/// The unit cost of the inventory item.
		/// </summary>
		[JsonProperty("cost")]
		public decimal? Cost { get; set; }

		/// <summary>
		/// The two-digit code for the country where the inventory item was made.
		/// </summary>
		[JsonProperty("country_code_of_origin")]
		public string CountryCodeOfOrigin { get; set; }

		/// <summary>
		/// The general Harmonized System (HS) code for the inventory item. Used if a country-specific HS code is not available.
		/// </summary>
		[JsonProperty("harmonized_system_code")]
		public string HarmonizedSystemCode { get; set; }

		/// <summary>
		/// The two-digit code for the province where the inventory item was made. Used only if the shipping provider for the inventory item is Canada Post.
		/// </summary>
		[JsonProperty("province_code_of_origin")]
		public string ProvinceCodeOfOrigin { get; set; }

		/// <summary>
		/// The unique SKU (stock keeping unit) of the inventory item.
		/// </summary>
		[JsonProperty("sku")]
		public string Sku { get; set; }

		/// <summary>
		/// Whether the inventory item is tracked. If true, then inventory quantity changes are tracked by Shopify.
		/// </summary>
		[JsonProperty("tracked")]
		public bool? Tracked { get; set; }

		/// <summary>
		/// Whether a customer needs to provide a shipping address when placing an order containing the inventory item.
		/// </summary>
		[JsonProperty("requires_shipping")]
		public bool? RequiresShipping { get; set; }

		/// <summary>
		/// The date and time (ISO 8601 format) when the inventory item was last modified.
		/// </summary>
		[JsonProperty("updated_at")]
		public String DateModifiedAt { get; set; }

	}

}
