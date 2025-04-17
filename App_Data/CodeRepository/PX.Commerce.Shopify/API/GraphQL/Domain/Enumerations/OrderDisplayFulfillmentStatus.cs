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

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// https://shopify.dev/docs/api/admin-graphql/2023-04/enums/OrderDisplayFulfillmentStatus
	/// </summary>
	public class OrderDisplayFulfillmentStatusGQL
	{
		/// <summary>
		/// Displayed as Fulfilled. All the items in the order have been fulfilled.
		/// </summary>
		public const string Fulfilled = "FULFILLED";

		/// <summary>
		/// Displayed as In progress. Some of the items in the order have been fulfilled, 
		/// or a request for fulfillment has been sent to the fulfillment service.
		/// </summary>
		public const string InProgress = "IN_PROGRESS";

		/// <summary>
		/// Displayed as On hold. All of the unfulfilled items in this order are on hold.
		/// </summary>
		public const string OnHold = "ON_HOLD";

		/// <summary>
		/// Displayed as Open. None of the items in the order have been fulfilled. 
		/// Replaced by "UNFULFILLED" status.
		/// </summary>
		public const string Open = "OPEN";

		/// <summary>
		/// Displayed as Partially fulfilled. Some of the items in the order have been fulfilled.
		/// </summary>
		public const string PartiallyFulfilled = "PARTIALLY_FULFILLED";

		/// <summary>
		/// Displayed as Pending fulfillment. A request for fulfillment of some items 
		/// awaits a response from the fulfillment service. Replaced by the "IN_PROGRESS" status.
		/// </summary>
		public const string PendingFulfillment = "PENDING_FULFILLMENT";

		/// <summary>
		/// Displayed as Restocked. All the items in the order have been restocked. 
		/// Replaced by the "UNFULFILLED" status.
		/// </summary>
		public const string Restocked = "RESTOCKED";

		/// <summary>
		/// Displayed as Scheduled. All of the unfulfilled items in this order are 
		/// scheduled for fulfillment at a later time.
		/// </summary>
		public const string Scheduled = "SCHEDULED";

		/// <summary>
		/// Displayed as Unfulfilled. None of the items in the order have been fulfilled.
		/// </summary>
		public const string Unfulfilled = "UNFULFILLED";
	}
}

