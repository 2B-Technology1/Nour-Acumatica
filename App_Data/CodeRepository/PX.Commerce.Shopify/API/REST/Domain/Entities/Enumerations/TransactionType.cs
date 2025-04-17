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

namespace PX.Commerce.Shopify.API.REST
{
	/// <summary>
	/// The transaction's type. Valid values:
	/// authorization: Money that the customer has agreed to pay. The authorization period can be between 7 and 30 days (depending on your payment service) while a store waits for a payment to be captured.
	/// capture: A transfer of money that was reserved during the authorization of a shop.
	/// sale: The authorization and capture of a payment performed in one single step.
	/// void: The cancellation of a pending authorization or capture.
	/// refund: The partial or full return of captured money to the customer.
	/// For more information about Transaction: <br/><see href="https://shopify.dev/api/admin-rest/2022-10/resources/transaction">Click here for Rest API</see>.<br/>
	/// <see href="https://shopify.dev/api/admin-graphql/2023-01/enums/OrderTransactionKind">Click here for GraphQL</see>.
	/// </summary>
	public class TransactionType
    {
		/// <summary>
		/// authorization: Money that the customer has agreed to pay. The authorization period can be between 7 and 30 days (depending on your payment service) while a store waits for a payment to be captured.
		/// </summary>
		public const string Authorization = "authorization";

		/// <summary>
		/// emv_authorization: An authorization for a payment taken with an EMV credit card reader.
		/// </summary>
		public const string EmvAuthorization = "emv_authorization";

		/// <summary>
		/// capture: A transfer of money that was reserved during the authorization of a shop.
		/// </summary>
		public const string Capture = "capture";

		/// <summary>
		/// sale: The authorization and capture of a payment performed in one single step.
		/// </summary>
		public const string Sale = "sale";

		/// <summary>
		/// void: The cancellation of a pending authorization or capture.
		/// </summary>
		public const string Void = "void";

		/// <summary>
		/// refund: The partial or full return of captured money to the customer.
		/// </summary>
		public const string Refund = "refund";

		/// <summary>
		/// refund: The money returned to the customer when they've paid too much during a cash transaction.
		/// </summary>
		public const string Change = "change";

		/// <summary>
		/// suggested_refund: A suggested refund transaction that can be used to create a refund.
		/// </summary>
		public const string SuggestedRefund = "suggested_refund";
    }
}

