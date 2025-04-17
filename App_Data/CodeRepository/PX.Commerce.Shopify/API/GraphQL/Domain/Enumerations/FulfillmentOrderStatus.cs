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
	/// https://shopify.dev/docs/api/admin-graphql/2023-04/enums/FulfillmentOrderStatus
	/// </summary>
	public class FulfillmentOrderStatusGQL
	{
		/// <summary>
		/// The fulfillment order has been cancelled by the merchant.
		/// </summary>
		public const string Cancelled = "CANCELLED";

		/// <summary>
		/// The fulfillment order has been completed and closed.
		/// </summary>
		public const string Closed = "CLOSED";

		/// <summary>
		/// The fulfillment order cannot be completed as requested.
		/// </summary>
		public const string Incomplete = "INCOMPLETE";

		/// <summary>
		/// The fulfillment order is being processed.
		/// </summary>
		public const string InProgress = "IN_PROGRESS";

		/// <summary>
		/// The fulfillment order is on hold. The fulfillment process can't be initiated until the hold on the fulfillment order is released.
		/// </summary>
		public const string OnHold = "ON_HOLD";

		/// <summary>
		/// The fulfillment order is ready for fulfillment.
		/// </summary>
		public const string Open = "OPEN";

		/// <summary>
		/// The fulfillment order is deferred and will be ready for fulfillment after the date and time specified in fulfill_at.
		/// </summary>
		public const string Scheduled = "SCHEDULED";
	}
}

