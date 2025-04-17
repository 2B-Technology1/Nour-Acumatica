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
using PX.Commerce.BigCommerce.API.REST;
using Newtonsoft.Json;
using PX.Commerce.Core;
using System.ComponentModel;

namespace PX.Commerce.BigCommerce.API.REST
{
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	[CommerceDescription(BigCommerceCaptions.OrdersTax, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
	public class OrdersTaxData : BCAPIEntity
	{
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("order_id")]
        public int OrderId { get; set; }

		[JsonProperty("order_address_id")]
        public int OrderAddressId { get; set; }

		[JsonProperty("order_product_id", NullValueHandling = NullValueHandling.Ignore)]
		public decimal OrderProductId { get; set; }

		[JsonProperty("tax_rate_id")]
        public int TaxRateId { get; set; }

		[JsonProperty("tax_class_id")]
		public int TaxClassId { get; set; }

		[JsonProperty("class")]
		public string Class { get; set; }

		[JsonProperty("name")]
		[CommerceDescription(BigCommerceCaptions.TaxName, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public string Name { get; set; }

        [JsonProperty("rate")]
		[CommerceDescription(BigCommerceCaptions.TaxRate, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public decimal Rate { get; set; }

        [JsonProperty("priority")]
		[CommerceDescription(BigCommerceCaptions.TaxPriority, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public int Priority { get; set; }

		[JsonProperty("priority_amount")]
		[CommerceDescription(BigCommerceCaptions.TaxPriorityAmount, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public decimal PriorityAmount { get; set; }

		[JsonProperty("line_amount")]
		[CommerceDescription(BigCommerceCaptions.TaxLineAmount, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public decimal LineAmount { get; set; }

		[JsonProperty("line_item_type", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(BigCommerceCaptions.TaxLineItemType, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public string LineItemType { get; set; }
	}
}
