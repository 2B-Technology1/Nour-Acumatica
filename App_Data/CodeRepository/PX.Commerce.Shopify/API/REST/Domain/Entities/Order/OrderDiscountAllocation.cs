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
	[JsonObject(Description = "Order Discount Allocation")]
	[CommerceDescription(ShopifyCaptions.DiscountAllocation, FieldFilterStatus.Filterable, FieldMappingStatus.ImportAndExport)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class OrderDiscountAllocation : BCAPIEntity
	{
		/// <summary>
		/// The discount amount allocated to the line in the shop currency.
		/// </summary>
		[JsonProperty("amount")]
		[CommerceDescription(ShopifyCaptions.DiscountAmount, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public decimal? DiscountAmount { get; set; }

		[CommerceDescription(ShopifyCaptions.DiscountAmountPresentment, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		[ShouldNotSerialize]
		public decimal? DiscountAmountPresentment { get => DiscountPriceSet?.PresentmentMoney?.Amount; }

		/// <summary>
		/// The index of the associated discount application in the order's discount_applications list.
		/// </summary>
		[JsonProperty("discount_application_index")]
		public int? DiscountApplicationIndex { get; set; }

		/// <summary>
		/// The discount amount allocated to the line item in shop and presentment currencies.
		/// </summary>
		[JsonProperty("amount_set")]
		public PriceSet DiscountPriceSet { get; set; }


		/// <summary>
		/// The discount class:
		/// PRODUCT: The discount applies to specific products
		/// ORDER: The discount applies across all line items.
		/// </summary>
		[JsonProperty("discount_class")]
		public virtual string DiscountClass { get; set; }

	}

}
