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

using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.Shopify.API.REST
{
	/// <summary>
	/// open: The fulfillment order is ready for fulfillment.
	/// in_progress: The fulfillment order is being processed.
	/// scheduled: The fulfillment order is deferred and will be ready for fulfillment after the datetime specified in fulfill_at.
	/// canceled: The fulfillment order has been canceled by the merchant.
	/// on_hold: The fulfillment order is on hold. The fulfillment process can't be initiated until the hold on the fulfillment order is released.
	/// incomplete: The fulfillment order cannot be completed as requested.
	/// closed: The fulfillment order has been completed and closed.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum FulfillmentOrderStatus
	{
		/// <summary>
		/// open: The fulfillment order is ready for fulfillment.
		/// </summary>
		[EnumMember(Value = "open")]
		Open = 0,

		/// <summary>
		/// in_progress: The fulfillment order is being processed.
		/// </summary>
		[EnumMember(Value = "in_progress")]
		InProgress = 1,

		/// <summary>
		/// scheduled: The fulfillment order is deferred and will be ready for fulfillment after the datetime specified in fulfill_at.
		/// </summary>
		[EnumMember(Value = "scheduled")]
		Scheduled = 2,

		/// <summary>
		/// canceled: The fulfillment order has been canceled by the merchant.
		/// </summary>
		[EnumMember(Value = "canceled")]
		Canceled = 3,

		/// <summary>
		/// on_hold: The fulfillment order is on hold. The fulfillment process can't be initiated until the hold on the fulfillment order is released.
		/// </summary>
		[EnumMember(Value = "on_hold")]
		OnHold = 4,

		/// <summary>
		/// incomplete: The fulfillment order cannot be completed as requested.
		/// </summary>
		[EnumMember(Value = "incomplete")]
		Incomplete = 5,

		/// <summary>
		/// The fulfillment order has been completed and closed.
		/// </summary>
		[EnumMember(Value = "closed")]
		Closed = 6,
	}
}
