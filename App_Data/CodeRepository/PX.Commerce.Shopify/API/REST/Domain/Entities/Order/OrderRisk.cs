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
	public class OrderRiskResponse : IEntityResponse<OrderRisk>
	{
		[JsonProperty("risk")]
		public OrderRisk Data { get; set; }
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class OrderRisksResponse : IEntitiesResponse<OrderRisk>
	{
		[JsonProperty("risks")]
		public IEnumerable<OrderRisk> Data { get; set; }
	}

	[JsonObject(Description = "Order Risk")]
	[Description(ShopifyCaptions.OrderRisk)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class OrderRisk : BCAPIEntity
	{
		/// <summary>
		/// Whether this order risk is severe enough to force the cancellation of the order. 
		/// If true, then this order risk is included in the Order canceled message that's shown on the details page of the canceled order.
		/// Note: Setting this property to true does not cancel the order. 
		/// Use this property only if your app automatically cancels the order using the Order resource. 
		/// If your app doesn't automatically cancel orders based on order risks, then leave this property set to false.
		/// </summary>
		[JsonProperty("cause_cancel", NullValueHandling = NullValueHandling.Ignore)]
		public bool? CauseCancel { get; set; }

		/// <summary>
		/// The ID of the checkout that the order risk belongs to. 
		/// </summary>
		[JsonProperty("checkout_id", NullValueHandling = NullValueHandling.Ignore)]
		public long? CheckoutId { get; set; }

		/// <summary>
		/// Whether the order risk is displayed on the order details page in the Shopify admin. 
		/// If false, then this order risk is ignored when Shopify determines your app's overall risk level for the order.
		/// </summary>
		[JsonProperty("display", NullValueHandling = NullValueHandling.Ignore)]
		public bool? Display { get; set; }

		/// <summary>
		/// A unique numeric identifier for the order risk.
		/// </summary>
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Id, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public long? Id { get; set; }

		/// <summary>
		/// The ID of the order that the order risk belongs to.
		/// </summary>
		[JsonProperty("order_id", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.OrderId, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public long? OrderId { get; set; }

		/// <summary>
		/// The message that's displayed to the merchant to indicate the results of the fraud check. 
		/// The message is displayed only if display is set totrue.
		/// </summary>
		[JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Message, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public string Message { get; set; }

		/// <summary>
		/// The recommended action given to the merchant. Valid values:
		/// cancel: There is a high level of risk that this order is fraudulent. The merchant should cancel the order.
		/// investigate: There is a medium level of risk that this order is fraudulent. The merchant should investigate the order.
		/// accept: There is a low level of risk that this order is fraudulent. The order risk found no indication of fraud.
		/// </summary>
		[JsonProperty("recommendation")]
		[CommerceDescription(ShopifyCaptions.Recommendation, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public OrderRiskActionType Recommendation { get; set; }

		/// <summary>
		/// For internal use only. A number between 0 and 1 that's assigned to the order. 
		/// The closer the score is to 1, the more likely it is that the order is fraudulent.
		/// </summary>
		[JsonProperty("score", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Score, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public decimal Score { get; set; }

		/// <summary>
		/// The source of the order risk.
		/// </summary>
		[JsonProperty("source", NullValueHandling = NullValueHandling.Ignore)]
		public string Source { get; set; }
	}
}
