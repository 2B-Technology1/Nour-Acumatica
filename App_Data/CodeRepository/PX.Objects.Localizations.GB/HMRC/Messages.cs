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

namespace PX.Objects.Localizations.GB.HMRC
{
	[PXLocalizable(Messages.Prefix)]
	public static class Messages
	{
		public const string Prefix = "HMRC";

		public const string Fulfilled = "Fulfilled";
		public const string Open = "Open";

		//public const string VatReturnWillBeSentToHMRC = "The VAT return will be sent to HMRC. Is the VAT return finalized?";
		public const string VatReturnWillBeSentToHMRC = "When you submit this VAT information, you are making a legal declaration that the information is true and complete. A false declaration can result in prosecution.";
		public const string PleaseAuthorize = "Please authorize.";

		public const string FromDate = "From Date";
		public const string ToDate = "To Date";
		public const string DueDate = "Due Date";
		public const string SubmitVATReturn = "Submit VAT Return";
		public const string RetrieveVATreturn = "Retrieve VAT return";
		public const string PeriodCode = "Period Code";
		public const string ReceivedDate = "Received Date";
		public const string Status = "Status";
		public const string Description = "Description";
		public const string Amount = "Amount";
		public const string TaxBoxCode = "Tax Box Code";
		public const string TaxBoxNumber = "Tax Box Number";
		public const string VATreturnIsAccepted = "The VAT return is accepted.";
		public const string TestFraudHeaders = "Test Fraud Prevention Headers";
		public const string HeadersValidated = "Headers have been validated, please check trace to see the details.";

		public const string vatDueSales = "VAT due on sales and other outputs";
		public const string vatDueAcquisitions = "VAT due on acquisitions from other EC Member States.";
		public const string totalVatDue = "Total VAT Due";
		public const string vatReclaimedCurrPeriod = "VAT reclaimed on purchases and other inputs";
		public const string netVatDue = "The difference between Box 3 and Box 4";
		public const string totalValueSalesExVAT = "Total value of sales and all other outputs excluding any VAT";
		public const string totalValuePurchasesExVAT = "Total value of purchases and all other inputs excluding any VAT";
		public const string totalValueGoodsSuppliedExVAT = "Total value of all supplies of goods and related costs, excluding any VAT, to other EC member states.";
		public const string totalAcquisitionsExVAT = "Total value of acquisitions of goods and related costs excluding any VAT, from other EC member states.";

		public const string ImpossibleToRefreshToken = "Cannot refresh the token";
		public const string RefreshTokenIsInvalid = "Refresh token is invalid";
		public const string RefreshTokenIsMissing = "Refresh token is missing";

		public const string NoReportLinesToSend = "There are no report lines to send.";

		public const string CompanyFieldEmpty = "The box ({1}) is empty for the company ({0}) on the Companies (CS101500) form.";
		public const string BranchFieldEmpty = "The box ({1}) is empty for the branch ({0}) on the Branches (CS102000) form.";
		public const string TaxRegistrationIDInvalid = "The value ({0}) is not a valid HMRC Tax Registration ID; it must be a number of nine or 12 digits long.";
		public const string UnretrievableRedirectURL = "Could not retrieve redirect URL. Please see trace for details.";
		public const string NonJSONContentType = "When trying to retrieve the API URLs, a response was received with the content type that is not JSON.";
		public const string CouldNotInitApi = "Could not initialize the API provider. External application is not specified.";
		public const string TaxPeriodNotFoundHMRC = "Tax Period is not found on the HMRC side.";
		public const string CannotSubmitReturn = "Cannot submit VAT return.";

		public const string UnretrievableToken = "Could not receive the access token";
		public const string UnretrievableApplicationProcessor = "Could not retrieve the application processor";
		public const string ErrorGettingHttpRequest = "An error occurred when executing the HTTP request";

		public const string HowToEnableService = "To activate the service, enable the {0} feature.";
	}
}
