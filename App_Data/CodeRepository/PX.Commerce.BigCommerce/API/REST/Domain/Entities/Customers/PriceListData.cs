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
using System.ComponentModel;
using System.Linq;

namespace PX.Commerce.BigCommerce.API.REST
{
	[JsonObject(Description = "PriceList")]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PriceList : BCAPIEntity
	{
		[JsonProperty("id")]
		public int? ID { get; set; }
		public bool ShouldSerializeId()
		{
			return false;
		}

		[JsonProperty("name")]
		public string Name { get; set; }

		public List<PriceListRecord> priceListRecords { get; set; }
		public string ExtrenalPriceClassID { get; set; }

	}

	[JsonObject(Description = "PriceListRecords")]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PriceListRecord : BCAPIEntity
	{
		[JsonProperty("variant_id")]
		public string VariantID { get; set; }

		[JsonProperty("sku")]
		public string SKU { get; set; }

		[JsonProperty("currency")]
		public string Currency { get; set; }

		[JsonProperty("price")]
		public decimal? Price { get; set; }

		[JsonProperty("sale_price")]
		public decimal? SalesPrice { get; set; }

		[JsonProperty("product_id")]
		public int? ProductID { get; set; }
		public bool ShouldSerializeProductID()
		{
			return false;
		}

		[JsonProperty("bulk_pricing_tiers")]
		public List<BulkPricingTier> BulKPricingTier { get; set; }


		/// <summary>
		/// Contains the NoteId of the inventory item that is related directly to the price record in the ERP
		/// If we define prices at the template item level, all variants records will have the note id of the template item
		/// If we define a price at the variant level, this field will contain the noteid of the variant inventory item.
		/// This field is not sent to BC, but used to store the local inventory id
		/// so that we can relate it to the current external record after syncing.
		/// </summary>
		[JsonIgnore]
		public Guid localPriceInventoryNoteId { get; set; }

		public PriceListRecord ShallowCopy()
		{
			return (PriceListRecord)this.MemberwiseClone();
		}
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class BulkPricingTier : BCAPIEntity
	{
		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("quantity_min")]
		public int QuantityMinimum { get; set; }

		[JsonProperty("amount")]
		public decimal? Amount { get; set; }

		public string PriceCode { get; set; }
		
	}

	[JsonObject(Description = "Price list (BigCommerce API v3 response)")]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PriceListResponse : IEntityResponse<PriceList>
	{
		[JsonProperty("data")]
		public PriceList Data { get; set; }

		[JsonProperty("meta")]
		public Meta Meta { get; set; }
	}

	[JsonObject(Description = "List of Pricelist (BigCommerce API v3 response)")]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PriceListsResponse : IEntitiesResponse<PriceList>
	{
		public PriceListsResponse()
		{
			Data = new List<PriceList>();
		}

		[JsonProperty("data")]
		public List<PriceList> Data { get; set; }

		[JsonProperty("meta")]
		public Meta Meta { get; set; }
	}

	[JsonObject(Description = "List of Price list  records(BigCommerce API v3 response)")]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PriceListRecordResponse : IEntitiesResponse<PriceListRecord>
	{
		public PriceListRecordResponse()
		{
			Data = new List<PriceListRecord>();
		}
		[JsonProperty("data")]
		public List<PriceListRecord> Data { get; set; }

		[JsonProperty("meta")]
		public Meta Meta { get; set; }
	}
}
