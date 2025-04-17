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

namespace PX.Commerce.BigCommerce.API.REST
{
	public class OrderPaymentEvent
	{
		/// <summary>
		/// The authorization and capture of a payment performed in one single step.
		/// </summary>
		public const string Purchase = "purchase";

		/// <summary>
		/// Money that the customer has agreed to pay.
		/// </summary>
		public const string Authorization = "authorization";

		/// <summary>
		/// A transfer of money that was reserved during the authorization of a shop.
		/// </summary>
		public const string Capture = "capture";

		/// <summary>
		/// The partial or full return of captured money to the customer.
		/// </summary>
		public const string Refund = "refund";

		/// <summary>
		/// The cancellation of a pending authorization or capture.
		/// </summary>
		public const string Void = "void";

		public const string Pending = "pending";
		public const string Finalization = "finalization";
	}
}
