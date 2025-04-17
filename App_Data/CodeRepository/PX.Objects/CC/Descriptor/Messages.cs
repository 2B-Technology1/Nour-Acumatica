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

using PX.Common;

namespace PX.Objects.CC
{
	[PXLocalizable]
	public static class Messages
	{
		#region PayLink
		public const string NoSuitablePMAndCAForMeansOfPayment = "There is no suitable payment method and cash account for the {0} means of payment.";
		public const string NoSuitablePMForPayment = "There is no suitable payment method for the payment being created.";
		public const string ProcCenterBranchMappingNotFound = "The mapping row for the {0} branch and the {1} processing center has not been found.";
		public const string DocumentHasActivePayLink = "The {0} document with the {1} reference number already has an active payment link.";
		public const string CannotCreateLinkForDocument = "Cannot create a link for the {0} document with the {1} reference number because the document has the invalid status.";
		public const string PayLinkProcessingError = "Payment Link processing error: {0}";

		public const string PayLinkCategory = "Payment Links";
		public const string PayLinkPaymentDescr = "Payment Link {0} {1}, External ID {2}";
		public const string PayLinkStatusOpen = "Open";
		public const string PayLinkStatusClosed = "Closed";
		public const string PayLinkStatusNone = "None";
		public const string PayLinkPaymentStatusUnpaid = "Unpaid";
		public const string PayLinkPaymentStatusPaid = "Paid";
		public const string PayLinkPaymentStatusIncomplete = "Incomplete";
		public const string PayLinkPaymentStatusNone = "None";
		public const string UseInvoiceToProcessPayLink = "Use the {0} invoice that is related to the {1} sales order to process the payment link.";
		public const string UseRelatedInvoiceToCreateLink = "Use the invoices related to the {0} sales order to create a payment link.";
		public const string DocHasPaymentLink = "The {0} document with the {1} reference number has an active payment link. Close the payment link to change the currency.";
		public const string DeactivatePayLinkToChangeCustomer = "The {0} document with the {1} reference number has an active payment link. Close the payment link to change the customer.";
		public const string ClosePayLinkBeforeCancelOrder = "The {0} document with the {1} reference number has an active payment link. Close the payment link to cancel the order.";
		public const string CreateLink = "Create Payment Link";
		public const string SyncLink = "Sync Payment Link";
		public const string ProcCenterWasSelectedAsDefaultForBranch = "The {0} processing center has been selected as default for the {1} branch. Only one processing center can be defined as default for a single branch-currency combination.";
		public const string SpecifyPaymentMethodCashAccForBranch = "Specify at least one payment method and cash account for the {0} branch.";
		public const string PaymentLinkMappingNotDefined = "No settings for the payment link and for payment creation have been defined.";
		#endregion
	}
}
