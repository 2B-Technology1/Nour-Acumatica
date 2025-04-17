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
	/// unsubmitted: The initial request status for newly-created fulfillment orders. This is the only valid request status for fulfillment orders that aren't assigned to a fulfillment service.
	/// submitted: The merchant requested fulfillment for this fulfillment order.
	/// accepted: The fulfillment service accepted the merchant's fulfillment request.
	/// rejected: The fulfillment service rejected the merchant's fulfillment request.
	/// cancellation_requested: The merchant requested a cancellation of the fulfillment request for this fulfillment order.
	/// cancellation_accepted: The fulfillment service accepted the merchant's fulfillment cancellation request.
	/// cancellation_rejected: The fulfillment service rejected the merchant's fulfillment cancellation request.
	/// closed: The fulfillment service closed the fulfillment order without completing it.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum FulfillmentOrderRequestStatus
	{
		/// <summary>
		/// unsubmitted: The initial request status for newly-created fulfillment orders.
		/// This is the only valid request status for fulfillment orders that aren't assigned to a fulfillment service.
		/// </summary>
		[EnumMember(Value = "unsubmitted")]
		Unsubmitted = 0,

		/// <summary>
		/// submitted: The merchant requested fulfillment for this fulfillment order.
		/// </summary>
		[EnumMember(Value = "submitted")]
		Submitted = 1,

		/// <summary>
		/// accepted: The fulfillment service accepted the merchant's fulfillment request.
		/// </summary>
		[EnumMember(Value = "accepted")]
		Accepted = 2,

		/// <summary>
		/// rejected: The fulfillment service rejected the merchant's fulfillment request.
		/// </summary>
		[EnumMember(Value = "rejected")]
		Rejected = 3,

		/// <summary>
		/// cancellation_requested: The merchant requested a cancellation of the fulfillment request for this fulfillment order.
		/// </summary>
		[EnumMember(Value = "cancellation_requested")]
		CancellationRequested = 4,

		/// <summary>
		/// cancellation_accepted: The fulfillment service accepted the merchant's fulfillment cancellation request.
		/// </summary>
		[EnumMember(Value = "cancellation_accepted")]
		CancellationAccepted = 5,

		/// <summary>
		/// cancellation_rejected: The fulfillment service rejected the merchant's fulfillment cancellation request.
		/// </summary>
		[EnumMember(Value = "cancellation_rejected")]
		CancellationRejected = 6,

		/// <summary>
		/// closed: The fulfillment service closed the fulfillment order without completing it.
		/// </summary>
		[EnumMember(Value = "closed")]
		Closed = 7,

	}
}
