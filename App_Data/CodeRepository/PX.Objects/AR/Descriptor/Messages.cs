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
using System;

namespace PX.Objects.AR
{
	[PXLocalizable(Messages.Prefix)]
	public static class Messages
	{
		// Add your messages here as follows (see line below):
		#region Validation and Processing Messages
		public const string Prefix = "AR Error";
		public const string Document_Status_Invalid = AP.Messages.Document_Status_Invalid;
		public const string Entry_LE = AP.Messages.Entry_LE;
		public const string Entry_GE = AP.Messages.Entry_GE;
		public const string ExternalRetainedTaxesNotSupported = "Retained taxes calculated by an external tax provider are not supported.";
		public const string IncorrectExtPriceAndCuryRetainageAmt = "The line retainage amount must have the same sign as the line amount.";
		public const string IncorrectRetainagePositiveAmount = "A retainage amount cannot be greater than the line amount ({0}).";
		public const string IncorrectRetainageNegativeAmount = "A retainage amount cannot be less than the line amount ({0}).";
		public const string UnknownDocumentType = AP.Messages.UnknownDocumentType;
		public const string Document_OnHold_CannotRelease = AP.Messages.Document_OnHold_CannotRelease;
		public const string ApplDate_Less_DocDate = AP.Messages.ApplDate_Less_DocDate;
		public const string WriteOff_ApplDate_Less_DocDate = "Write-Off {0} cannot be less than Document Date.";
		public const string ApplPeriod_Less_DocPeriod = AP.Messages.ApplPeriod_Less_DocPeriod;
		public const string DocumentBalanceNegative = AP.Messages.DocumentBalanceNegative;
		public const string DocumentOutOfBalanceNegative = "The balance of {0}: {1} will go negative. The document will not be released.";
		public const string DocumentOutOfBalanceHigher = "The balance of {0}: {1} will exceed the document's total amount. The document will not be released.";
		public const string LineBalanceNegative = "The line balance will go negative. The document will not be released.";
		public const string LineBalancePositive = "The line balance will go positive. The document will not be released.";
		public const string DocumentApplicationAlreadyVoided = AP.Messages.DocumentApplicationAlreadyVoided;
		public const string DocumentOutOfBalance = AP.Messages.DocumentOutOfBalance;
		public const string CashSaleOutOfBalance = AP.Messages.QuickCheckOutOfBalance;
		public const string SheduleNextExecutionDateExceeded = GL.Messages.SheduleNextExecutionDateExceeded;
		public const string SheduleExecutionLimitExceeded = GL.Messages.SheduleExecutionLimitExceeded;
		public const string SheduleHasExpired = GL.Messages.SheduleHasExpired;
		public const string MultipleApplicationError = AP.Messages.MultipleApplicationError;
		public const string VoidAppl_CheckNbr_NotMatchOrigPayment = "Void Payment must have the same Reference Number as the voided payment.";
		public const string FinChargeCanNotBeDeleted = "Financial charges cannot be entered directly. Please use Overdue Charges calculation process.";
		public const string CreditLimitWasExceeded = "The customer's credit limit has been exceeded.";
		public const string CreditDaysPastDueWereExceeded = "The customer's Days Past Due number of days has been exceeded!";
		public const string CustomerIsOnCreditHold = "The customer status is 'Credit Hold'.";
		public const string CustomerIsOnHold = "The customer status is 'On Hold'.";
		public const string CustomerIsInactive = "The customer status is 'Inactive'.";
		public const string SalesPersonIsInactive = "The sales person status is 'Inactive'.";
		public const string CustomerSmallBalanceAllowOff = "Write-Offs are not allowed for the customer.";
		public const string CustomerWithChildAccountsForWhichBalanceConsolidationEnabled = "{0} cannot be selected as the parent account for {1}, because {1} is associated with child accounts for which balance consolidation is enabled in the system.";
		public const string CustomerWithParentAccountsForWhichBalanceConsolidationEnabled = "{0} cannot be selected as the parent account for {1}, because {0} already has its parent account and balance consolidation is enabled for {0} in the system.";
		public const string CreditHoldEntry = "Document status is 'On Credit Hold'.";
		public const string AdminHoldEntry = "Document status is 'On Hold'.";
		public const string SPCommissionCalcFailure = "Commission calculation process failed with one or more error.";
		public const string SPFuturePeriodIsInvalidToProcess = "Processing date is less than the start date for the selected commission period.";
		public const string SPOpenPeriodProcessingConfirmation = "If new documents for this period arrive, you will need to repeat the process of calculating commission or they will be included into the next commission period. Do you want to continue?";
		public const string SalesPersonAddedForAllLocations = "This Sales Person is added for all the Customer locations already.";
		public const string SalesPersonWithHistoryMayNotBeDeleted = "One or more AR transactions exists for the selected Sales Person. This record cannot be deleted.";
		public const string AllCustomerLocationsAreAdded = "All Customer locations has been added already.";
		public const string CustomerLocationContainErrors = "The record cannot be saved because customer location records contain errors. Correct the errors on the Customer Locations (AR303020) form.";
		public const string CustomerClassChangeWarning = "The customer class will be changed. Click Yes if you want to replace the customer settings with the default settings provided by the selected customer class. Click No if you want to keep the original customer settings.";
		public const string TempCrLimitInvalidDate = "Start date must be less or equal to the end date.";
		public const string TempCrLimitPeriodsCrossed = "Credit limit for this customer has already been exceeded.";
		public const string DuplicateCustomerPayment = "Payment with Payment Ref. '{0}' dated '{1}' already exists for this Customer and have the same Payment Method. It's Reference Number - {2} {3}.";
		public const string PaymentProfileDiscontinuedProcCenter = "The processing center set up for the selected customer payment method uses the discontinued plug-in. Set up the processing center to use the Authorize.Net API plug-in.";
		public const string PaymentProfileProcCenterNotSupported = "The {0} processing center configured for the selected customer payment method uses an unsupported plug-in.";
		public const string PaymentProfileProcCenterMissing = "The {0} processing center configured for the selected customer payment method references a missing plug-in.";
		public const string SavingCardsNotAllowedForProcCenter = "Saving payment profiles is not allowed for the {0} processing center.";
		public const string SavingCardsNotAllowedForCustomerClass = "Saving payment profiles is not allowed for the {0} customer class that is selected for the {1} customer.";
		public const string TryingToUseNotSupportedPlugin = "The operation cannot be completed. The {0} processing center uses an unsupported plug-in.";
		public const string TryingToUseMissingPlugin = "The operation cannot be completed. The {0} processing center references a missing plug-in.";
		public const string PaymentProfileInactiveProcCenter = "The customer payment method uses a deactivated processing center.";
		public const string PaymentMethodIsAlreadyDefined = "You cannot add more than one Payment Method of this type for the Customer.";
		public const string RequiredCPMDetailIsEmpty = "The {0} detail is marked as required for customer payment methods of the {1} payment method but cannot be filled in automatically. Review the settings of the {1} payment method.";
		public const string PaymentIsNotReleased = "The {0} payment is not released.";
		public const string PaymentIsVoided = "The {0} payment has already been voided.";
		public const string CreditCardProcessingIsDisabled = "The operation cannot be performed because the financial period of the current date is either closed or does not exist for the {0} company. To process credit cards or EFT, create or reopen the financial period.";
		public const string TranCanNotBeImportedPaymentIsReleased = "The transaction cannot be imported for the {0} {1}. The {1} has been released.";
		public const string ERR_UnreleasedFinChargesForDocument = "At least one unreleased overdue charge document has been found for this document. Processing has been aborted.";
		public const string WRN_FinChargeCustomerHasOpenPayments = "One or more unapplied or unreleased payments has been found for this Customer. Calculation of Overdue Charges can be affected by these documents. It is recommended to release and apply these documents prior to the processing.";
		public const string ERR_EmailIsRequiredForSendByEmailOptions = "Email address must be specified if any of the following options is activated: {0}.";
		public const string ERR_EmailIsRequiredForOption = "Email address must be specified if '{0}' option is activated.";
		public const string WRN_ProcessStatementDetectsUnappliedPayments = "One or more Customers with unapplied payment documents has been found. It is recommended to run Auto Apply Payments process prior to this Statement Cycle closure.";
		public const string WRN_ProcessStatementDetectsOverdueInvoices = "One or more Customers with overdue documents has been found. It is recommended to run Calculate Overdue Charges process prior to this Statement Cycle closure.";
		public const string WRN_ProcessStatementDetectsOverdueInvoicesAndUnappliedPayments = "One or more Customers with unapplied payments and overdue documents has been found. It's recommened to run Auto Apply Payments and Calculate Overdue Charges process prior to this Statement Cycle closure.";
		public const string DeleteStatementCaption = "Delete Customer Statements";
		public const string DeleteStatementMsg = "The customer statements generated on the {0:d} date for the {1} statement cycle will be deleted. To proceed, click Delete.";
		public const string DeleteButton = "Delete";
		public const string CancelButton = "Cancel";
		public const string Invoice_NotPrinted_CannotRelease = "Invoice/Memo document was not printed and cannot be released.";
		public const string Invoice_NotEmailed_CannotRelease = "Invoice/Memo document was not emailed and cannot be released.";
		public const string ERR_ProcessingCenterForCardNotConfigured = "Processing center for this card type is not configured properly.";
		public const string ERR_PluginTypeIsNotSelectedForProcessingCenter = "Plug-in type is not selected for the {0} processing center.";
		public const string ERR_IncorrectProcessingCenter = "The {0} processing center cannot be used for the processing of the credit card transaction.";
		public const string ERR_IncorrectProcessingCenterForPmInstance = "The {0} processing center is not associated with the {1} customer payment method.";
		public const string ERR_PaymentMethodDoesNotSupportCrediCards = "The {0} payment method does not support storing credit card transaction information.";
		public const string ERR_ProcessingCenterNotSupportAcceptPaymentForm = "The selected processing center does not support acceptance of payments directly from new cards.";
		public const string ERR_ProcessingCenterTypeIsInvalid = "Type {0} defined for the processing center {1} cannot is not located within processing object.";
		public const string ERR_ProcessingCenterTypeInstanceCreationFailed = "Cannot instantiate processing object of {0} type for the processing center {1}.";
		public const string ERR_CCPaymentProcessingInternalError = "Error during request processing. Transaction ID:{0}, Error:{1}";
		public const string ERR_CCProcessingReferensedTransactionNotAuthorized = "Transaction {0} failed authorization";
		public const string ERR_CCProcessingTransactionMayNotBeVoided = "Transaction of {0} type cannot be voided";
		public const string ERR_CCProcessingTransactionMayNotBeCredited = "Transaction {0} type cannot not be credited";
		public const string ERR_CCTransactionCurrentlyInProgress = "This document has one or more transaction under processing.";
		public const string ERR_AcceptHostedFormResponseNotFound = "Could not get a response from the hosted form.";
		public const string ERR_CouldNotGetTransactionIdFromResponse = "Could not get Transaction ID from the hosted form response.";
		public const string ERR_CCAuthorizedPaymentAlreadyCaptured = "This payment has been captured already.";
		public const string ERR_CCPaymentAlreadyAuthorized = "This payment has been pre-authorized already.";
		public const string ERR_CCPaymentIsAlreadyRefunded = "This payment has been refunded already.";
		public const string ERR_CCNoTransactionToVoid = "There is no successful transaction to void.";
		public const string ERR_CCNoTransactionToRefund = "The {0} refund transaction cannot be imported for the {1} {2}. The {2} does not have a successful transaction to refund.";
		public const string ERR_CCNoSuccessfulTransactionToVoid = "The {0} refund transaction cannot be imported for the {1} {2}. The {2} does not have a successful transaction to void.";
		public const string ERR_CCNoSuccessfulTransaction = "The {0} transaction cannot be imported for the {1} {2}. The {2} does not have a successful transaction.";
		public const string ERR_CCTransactionOfThisTypeInvalidToVoid = "This type of transaction cannot be voided";
		public const string ERR_CCTheAmountDoesntMatchOriginalTransaction = "The amount of the {0} {1} does not match the amount of the {2} original transaction. The Void action can be used if these amounts are equal.";
		public const string ERR_CCTransactionProcessingFailed = "Processing of the {0} transaction failed. See the transaction description for details.";
		public const string ERR_DuplicatedSalesPersonAdded = "This Sales Person is already added";
		public const string ERR_TransactionIsRecordedForOtherDoc = "The {0} transaction that was processed with the {1} processing center has already been recorded for the {2} {3}.";
		public const string ERR_TransactionWithCpmIsRecordedForOtherDoc = "The {0} transaction that was processed with the {1} customer payment method has already been recorded for the {2} {3}.";
		public const string ERR_CCTransactionCanNotBeFoundInProcessingCenter = "The {0} transaction cannot be found in the {1} processing center.";
		public const string ERR_TransactionProcessedOnAnotherInstance = "The {0} transaction has been processed on another Acumatica ERP instance and cannot be recorded.";
		public const string ERR_TransactionProcessedOnAnotherInstanceOrTenant = "The {0} transaction has been processed on another Acumatica ERP instance or tenant and cannot be recorded.";
		public const string ERR_IncorrectVoidTranType = "The {0} transaction is voided.";
		public const string ERR_IncorrectRefundTranType = "The {0} transaction is refunded.";
		public const string ERR_IncorrectExpiredTranStatus = "The {0} transaction has expired.";
		public const string ERR_IncorrectDeclinedTranStatus = "The {0} transaction is declined.";
		public const string ERR_IncorrectTranStatus = "The {0} transaction has an invalid status.";
		public const string ERR_IncorrectTranType = "The {0} transaction has an invalid transaction type.";
		public const string ERR_IncorrectTranAmount = "The {0} transaction amount is not the same as the payment amount.";
		public const string ERR_TranAmtIsGreaterPaymentAmt = "The {0} transaction amount is greater than the payment amount.";
		public const string ERR_IncorrectPmInstanceId = "The {0} transaction was processed with a different payment profile.";
		public const string ERR_IncorrectProcCenterID = "The {0} transaction was processed with a different processing center.";
		public const string ERR_IncorrectPaymentMethod = "The {0} transaction was processed with a different payment method.";
		public const string ERR_IncorrectPaymentMethodType = "The payment method with the {0} means of payment is not correct for this transaction. Select a payment method with the {1} means of payment.";
		public const string ERR_IncorrectCashAccount = "The {0} transaction was processed for a document associated with a different cash account.";
		public const string ERR_TranIsUsedForAnotherCustomer = "The {0} transaction has been processed for a different customer.";
		public const string ERR_RefundTranNotLinkedWithOrigTran = "The {0} transaction does not refund the transaction with the {1} transaction number that is entered in the Orig. Transaction box.";
		public const string ERR_InvalidTransactionTypeForCustomerRefund = "Refund documents allow recording only voided and refunded transactions.";
		public const string ERR_RefundTranNotLinkedOrigTran = "The {0} transaction does not refund the transaction with the {1} transaction number.";
		public const string ERR_VoidTranNotLinkedOrigTran = "Validation of the {0} transaction has failed. The {0} transaction cannot void the {1} transaction. To refund the original transaction, use the Refund Card Payment command on the More menu. To void the original transaction, delete the Refund document and void the original payment.";
		public const string ERR_TranNotLinked = "The {0} {1} transaction is not related to the {2} {3} transaction.";
		public const string ERR_RecordTranNotSupportedWithoutTranGetterFeature = "The transaction cannot be recorded. The {0} processing center does not support pulling transaction information from the processing center.";
		public const string ERR_GettingInformationByTranIdNotSupported = "The {0} processing center does not support pulling transaction information from the processing center.";
		public const string ERR_ReauthorizationIsNotSetUp = "The {0} payment has not been processed. Reauthorization is not set up for the {1} processing center.";
		public const string ERR_NoActiveCardForReauth = "The payment is not associated with an active customer payment method and cannot be reauthorized. The pre-authorization has not been voided.";
		public const string ERR_TransactionIsNotValidated = "The {0} transaction requires validation.";
		public const string ERR_ProcCenterNotSupportedUnlinkedRefunds = "The {0} card is associated with the {1} processing center that does not allow processing unlinked refunds. Select another card to process the unlinked refund.";
		public const string ERR_DocumentNotSupportedLinkedRefunds = "The document does not support refunds of credit card transactions.";
		public const string ERR_NeedValidationModeIsNotSupported = "The {0} transaction has not been imported. Set the Need Validation parameter of the API call to False and re-import the transaction.";
		public const string ERR_VoidedTranRecordOtherDoc = "The {0} transaction has not been imported. The transaction is recorded for the {1} {2} that has already been voided.";
		public const string OverdueChargeDateAndFinPeriodAreRequired = "Overdue Charge Date and Fin. Period are required";
		public const string RecordAlreadyExists = "Record already exists";
		public const string UnsupportedStatementScheduleType = "The '{0}' statement schedule type is not supported.";
		public const string UnknownPrepareOnType = "Unknown PrepareOn type";
		public const string CreditCardWithID_0_IsNotDefined = "Credit Card with ID {0} is not defined";
		public const string ProcessingCenterDoesNotAllowNewCards = "The {0} processing center does not support payments from new cards. On the Customer Payment Methods (AR303000) form, add this card for the {1} customer and then on the Payments and Applications (AR302000) form, clear the New Card check box and select the card in the Card/Account Nbr. box for the payment.";
		public const string CreditCardIsNotDefined = "The credit card is not defined.";
		public const string Cash_Sale_Cannot_Have_Multiply_Installments = "Multiple Installments are not allowed for Cash Sale.";
		public const string Multiply_Installments_Cannot_be_Reversed = "Multiple installments invoice cannot be reversed, Please reverse original invoice '{0}'.";
		public const string Application_Amount_Cannot_Exceed_Document_Amount = "Total application amount cannot exceed document amount.";
		public const string ApplicationIsAlreadyApplied = "{0} {1} is already applied to {2} {3}.";
		public const string RegularApplicationTotalAmountNegative = "The total application amount, including the cash discount and the write-off amounts, cannot be negative.";
		public const string RegularApplicationTotalAmountPositive = "The total application amount, including cash discounts and write-offs, cannot be positive.";
		public const string ReversedApplicationTotalAmountPositive = "For reversed applications, the total application amount, including the cash discount and the write-off amounts, cannot be positive.";
		public const string ReversedApplicationTotalAmountNegative = "For reversed applications, the total application amount including cash discounts and write-offs cannot be negative.";
		public const string ApplicationWOLimitExceeded = "The customer's write-off limit {0} has been exceeded.";
		public const string CustomerPMInstanceHasDuplicatedDescription = "A card with this card number is already registered for the customer.";
		public const string ERR_OrigTranNbrIsRequired = "Enter a valid transaction number which was provided by the processing center for the original payment. You can find it in the Proc. Center Tran. Nbr column on the Credit Card Processing Information tab of the Payments and Applications (AR302000) form.";
		public const string ERR_CCTransactionMustBeAuthorizedBeforeCapturing = "Transaction must be authorized before it may be captured";
		public const string ERR_CCOriginalTransactionNumberIsRequiredForVoiding = "Original transaction is required to may be voided";
		public const string ERR_CCOriginalTransactionNumberIsRequiredForVoidingOrCrediting = "Original transaction is required to may be voided/credited";
		public const string ERR_CCOriginalTransactionNumberIsRequiredForCrediting = "Original transaction is required to may be credited";
		public const string ERR_CCUnknownOperationType = "This operation is not implemented yet";
		public const string ERR_CCCreditCardHasExpired = "The credit card for the {1} customer expired on {0}.";
		public const string ERR_CCAuthorizationTransactionIsNotFound = "Priorly Authorized Transaction {0} is not found";
		public const string ERR_CCAuthorizationTransactionHasExpired = "Authorizing Transaction {0} has already expired. Authorization must be redone";
		public const string ERR_CCProcessingCenterUsedForAuthIsNotValid = "Processing center {0}, specified in authorizing transaction {1} can't be found";
		public const string ERR_CCProcessingCenterIsNotSpecified = "Processing center for payment method {0} is not specified";
		public const string ERR_CCProcessingCenterUsedForAuthIsNotActive = "Processing center {0}, specified in authorizing transaction {1} is inactive";
		public const string ERR_CCProcessingIsInactive = "Processing center {0} is inactive";
		public const string ERR_CCProcessingDoesNotSupportAuthorizeAction = "The {0} processing center does not support the Authorize action. The Capture action is supported only for payments that were pre-authorized externally.";
		public const string ERR_CCProcessingCenterIsNotActive = "Processing center {0} is inactive";
		public const string ERR_CCTransactionToVoidIsNotFound = "Transaction to be Void {0} is not found";
		public const string ERR_CCProcessingCenterUsedInReferencedTranNotFound = "Processing center {0}, specified in referenced transaction {1} can't be found";
		public const string ERR_CCProcessingCenterNotFound = "Processing center can't be found";
		public const string ERR_CCProcessingCenterUsedInReferencedTranNotActive = "Processing center {0}, specified in referenced transaction {1} is inactive";
		public const string ERR_CCProcessingCenterPluginNotFound = "Could not get the processing plug-in by the {0} processing center.";
		public const string ERR_PluginNotSupportCreateEditCardViaForm = "The processing plug-in does not support the creation or editing of payment profiles by using hosted forms. Please configure processing centers to use processing plug-ins that support hosted forms.";
		public const string ERR_CCTransactionToCreditIsNotFound = "Transaction to be Credited {0} is not found";
		public const string ERR_CCMultiplyPreauthCombined = "Multiple preauthorized orders are combined in one invoice.";
		public const string ERR_CCTransactionMustBeVoided = "CC Payment must be voided.";
		public const string ERR_CCExternalAuthorizationNumberIsRequiredForCaptureOnlyTrans = "Authorization Number, received from Processing Center is required for this type of transaction.";
		public const string ERR_CCAmountMustBePositive = "The amount must be greater than zero.";
		public const string ERR_CCProcessingCouldNotGenerateRedirectUrl = "Could not generate redirect URL";
		public const string ERR_CCProcessingEndpointCouldNotParse = "Could not parse {0}";
		public const string ERR_CCProcessingCenterNotContainsTranWithID = "The transaction is not found in the processing center.";
		public const string ERR_CCProcessingCenterUserClickedCancel = "Operation was canceled by user.";
		public const string ERR_CCProcessingCenterPCResponseReasonNotExists = "Does not exist.";
		public const string ERR_CCProcessingCenterAdminHasNoAccess = "The {0} document does not exist, or the Administrator role does not have rights to access it.";
		public const string ERR_TranIsNotValidatedDocCanNotBeVoided = "The {0} {1} cannot be voided. Use the Validate Card Payment action to validate the credit card transaction and void the document.";
		public const string ERR_DocCannotBeVoidedWithSharedTran = "The {0} {1} cannot be voided. The void transaction ({2}) has been recorded for the {3} {4}.";
		public const string ERR_TranWasAlreadyImported = "The {0} transaction has already been imported for the {1} {2}.";
		public const string ERR_NotValidImportedTranDate = "The transaction has not been imported. The transaction date specified in the TranDate parameter is earlier than the date of the original transaction.";
		public const string ERR_CannotVoidForReauth = "The pre-authorization was not voided for reauthorization. The scheduled reauthorization date is later than the expiration date of the credit card.";
		public const string ERR_IncorrectExpirationDate = "The date specified in the ExpirationDate parameter should be later than the date specified in the TranDate parameter.";
		public const string ERR_IntegratedProcessingIsDisabled = "The card processing action cannot be performed. The Integrated Card Processing feature is disabled on the Enable/Disable Features (CS100000) form.";
		public const string ERR_DocHasNeedSyncSharedTran = "The void or refund operation for the {0} transaction has been imported for the {1} {2}. Open the {1} {2} and use the Validate Card Payment command on the More menu.";
		public const string CCProcessingTranHeldWarning = "The transaction is held for review by the processing center. Use the processing center interface to approve or reject the transaction.";
		public const string CCProcessingARPaymentTranHeldWarning = "The transaction is held for review by the processing center. Use the processing center interface to approve or reject the transaction. After the transaction is processed by the processing center, use the Validate Card Payment action to update the processing status.";
		public const string CCProcessingAuthTranDeclined = "Cannot capture the pre-authorized credit card payment. The authorization transaction is declined.";
		public const string CCProcessingApprovalHoldingTranNotSupported = "The transaction is held for review. Approval is not supported by the {0} processing center integration plug-in. Please void the transaction.";
		public const string CCProcessingARPaymentMultipleActiveTranWarning = "Multiple active credit card transactions have been recorded for this payment. Use the processing center site to void or refund the duplicates.";
		public const string CCProcessingTranAmountHasChanged = "The {0} transaction amount has changed.";
		public const string CCProcessingMultipleActiveTran = "Multiple active transactions have been recorded for the {0} document.";
		public const string CCProcessingDetailsWereDeleted = "The customer payment method details were deleted. Integrated processing is not supported.";
		public const string CCProcessingARPaymentAlreadyProcessed = "The payment is already being processed. Proceeding can cause a duplicate credit card transaction.";
		public const string ERR_CCProcessingCouldNotFindTransaction = "The {0} transaction for the {1} document cannot be found in the system.";
		public const string ERR_CCProcessingARPaymentSyncLock = "The payment is locked for editing. Please wait for the external transaction result.\r\nIf the payment does not get unlocked, click Actions > Validate Card Payment to request the transaction result from the processing center.";
		public const string CCProcessingARPaymentSyncWarning = "The payment is locked for editing. Please wait for the external transaction result. If the payment does not get unlocked, click Actions > Validate Card Payment to request the transaction result from the processing center. Continue editing?";
		public const string CCProcessingOperationCancelled = "Operation cancelled.";
		public const string ARPaymentIsCreatedProcessingINProgress = "Payment {0} has been created";
		public const string CreationOfARPaymentFailedForSomeInvoices = "Creation of the Payment document has failed for one or more selected documents. Please, check specific error in each row";
		public const string ReservedWord = "'{0}' is a reserved word and cannot be used here.";
		public const string PrepaymentAppliedToMultiplyInstallments = "No applications can be created for documents with multiple installment credit terms specified.";
		public const string InvalidCashReceiptDeferredCode = "On Cash Receipt Deferred Code is not valid for the given document.";
		public const string SPCommissionPeriodMayNotBeProcessedThereArePeriodsOpenBeforeIt = "This Commission Period cannot be processed - all the previous commission periods must be closed first";
		public const string SPCommissionPeriodMayNotBeClosedThereArePeriodsOpenBeforeIt = "This Commission Period cannot be closed - all the previous commission periods must be closed first";
		public const string SPCommissionPeriodMayNotBeReopendThereAreClosedPeriodsAfterIt = "This Commission Period cannot be reopened - there are closed commission periods after it";
		public const string DuplicateInvoiceNbr = "Document with this Invoice Nbr. already exists.";
		public const string EntityDuplicateInvoiceNbr = "This vendor ref. number \"{0}\" has already been used for the document \"{1}\".";
		public const string SubEntityDuplicateInvoiceNbr = "This vendor ref. number \"{0}\" has already been used for the landed cost document (line number \"{1}\") linked to the purchase receipt \"{2}\".";
		public const string CannotSaveNotes = "Cannot save notes.";
		public const string CreditCardExpirationNotificationException = "Notification by E-mail failed: {0}";
		public const string ARPaymentIsIncludedIntoCADepositAndCannotBeVoided = "This payment is included into Payment Deposit document. It can't be voided until deposit is released or the payment is excluded from it.";
		public const string PaymentRefersToInvalidDeposit = "This payment refers to the invalid document {0} {1}.";
		public const string AccountIsSameAsDeferred = "Transaction Account is same as Deferral Account specified in Deferred Code.";
		public const string DocumentNotFound = "Document {0} {1} cannot be found in the system.";
		public const string DocumentNotFoundGenericErr = "The document cannot be found in the system.";
		public const string OriginalDocumentIsNotSet = "Original document is not set.";
		public const string DiscountOutOfDate = "Discount is out of date {0}.";
		public const string ERR_PCTransactionNumberOfTheOriginalPaymentIsRequired = "A valid PC Transaction number of the original payment is required";
		public const string DocsDepositAsBatchSettingDoesNotMatchClearingAccountFlag = "'Batch deposit' setting does not match 'Clearing Account' flag of the Cash Account";
		public const string CashAccountIsNotClearingPaymentWontBeIncludedInDeposit = "This cash account is not a clearing account for the {0} cash account, this payment will not be included in a bank deposit on import of the settlement batch.";
		public const string PMDeltaOptionRequired = "Billing Option is required for ARTran when Amount is less than original PM Transaction";
		public const string PMDeltaOptionNotValid = "Bill Later Option is not valid under current settings. The Amount of the given transaction is set to zero. Delete this line from the invoice so that it can be billed next time.";
		public const string CreditCardHasUncapturedTransactions = "This card holds authorized transactions that were not captured.";
		public const string ConfirmDeleteCustomerPaymentMethod = "This customer payment method is used in at least one document: {0} {1}. Are you sure you want to delete it?";
		public const string EntityCannotBeDeletedBecauseOfOneRefRecord = "{0} cannot be deleted because it is referenced in the following record: {1}.";
		public const string CustomerIsInStatus = "The customer status is '{0}'.";
		public const string CustomerOrOrganization = "Only a customer or company business account can be specified.";
		public const string CustomerOrOrganizationDependingOnShipmentType = "Incorrect value specified. Specify a customer (for shipments of the Shipment type) or a company business account (for shipments of the Transfer type).";
		public const string CustomerClassRestrictedByOrganization = "The usage of the {0} customer class is restricted in the current organization or branch.";
		public const string BranchRestrictedByCustomer = "The usage of the {0} customer is restricted in the {1} branch.";
		public const string BranchCustomerDifferentBaseCury = "The branch base currency differs from the base currency of the {0} entity associated with the {1} account.";
		public const string BranchCustomerDifferentBaseCuryReleased = "The document cannot be released, because the document's base currency differs from the base currency of the {0} entity associated with the {1} account.";
		public const string BranchCustomerDifferentBaseCuryCreated = "The document cannot be created because the base currency of the originating branch ({0}) differs from the base currency of the customer. To be able to process the document, select a current branch with the {1} base currency.";
		public const string ProcCenterCuryIDDifferentFromCashAccountCuryID = "The currency of the {0} processing center ({1}) differs from the currency of the {2} cash account ({3}). Select a cash account denominated in {1} to process transactions with the {0} processing center.";
		public const string CardCuryIDDifferentFromCashAccountCuryID = "The currency of the {0} cash account ({1}) differs from the currency of the {2} card transactions ({3}). Select a cash account denominated in {3} to process transactions with the {2} card.";
		public const string CashAccountIsNotConfiguredForPaymentMethodInAR = "The specified cash account is not configured for use in AR for the {0} payment method.";
		public const string CustomerClassCanNotBeDeletedBecauseItIsUsed = "This Customer Class can not be deleted because it is used in Accounts Receivable Preferences.";
		public const string InactiveCreditCardMayNotBeProcessed = "The credit card with ID {0} is inactive and may not be processed";
		public const string InactiveCustomerPaymentMethodIsUsedInTheScheduledInvoices = "This Customer Payment method is inactive, but there are scheduled invoices using it. You need to correct them in order to avoid invoice processing interruptions.";
		public const string OnlyLastRowCanBeDeleted = "Only last row can be deleted";
		public const string ThisValueMUSTExceed = "This value MUST exceed {0}";
		public const string ThisValueCanNotExceed = "This value can not exceed {0}";
		public const string TaxIsNotUptodate = "Tax is not up-to-date.";
		public const string NoPaymentInstance = "There is no Customer Payment Method associated with the given record. This Payment method does not require specific information for the given customer.";
		public const string AskConfirmation = "Confirmation";
		public const string AskUpdateLastRefNbr = "Do you want to update Last Reference Number with entered number?";
		public const string GroupUpdateConfirm = "All customers of the class will be included in the group specified in the Default Restriction Group box and excluded from the group to which they currently belong. Do you want to proceed?";
		public const string CardProcessingError = "Credit card processing error. {0} : {1}";
		public const string FeatureNotSupportedByProcessing = "{0} feature is not supported by processing";
		public const string AuthorizeNetFeatureIsDisabled = "The Authorize.Net Integration feature is disabled on the Enable/Disable Features (CS100000) form.";
		public const string AcumaticaPaymentsFeatureIsDisabled = "The Acumatica Payments feature is disabled on the Enable/Disable Features (CS100000) form.";
		public const string StripeFeatureIsDisabled = "The Stripe Integration feature is disabled on the Enable/Disable Features (CS100000) form.";
		public const string CustomCCIntegrationFeatureIsDisabled = "The Custom Card Processing Integration feature is disabled on the Enable/Disable Features (CS100000) form.";
		public const string NOCCPID = "No Payment Profile ID in detials for payment method {0}!";
		public const string CouldntGetPMIDetails = "Couldn't get details from processing center for payment method instance {0}";
		public const string DocumentAmountBelowMin = "The overdue charge document cannot be generated. The amount of overdue charges does not exceed the threshold amount required for generating the document.";
		public const string FixedAmountBelowMin = "With 0.00 amount specified, the system will not calculate charges for overdue documents. To initiate calculation of overdue charges, specify the fixed amount greater than 0.00.";
		public const string LineAmountBelowMin = "This line will not be added to the Overdue Charge document because calculated charge amount is less than the threshold amount specified in the overdue charge code on the Overdue Charges (AR204500) form.";
		public const string PercentListEmpty = "For the selected charging method, at least one percent rate must be specified in the table below.";
				public const string PercentForDateNotFound = "Effective percent rate is not found.";
		public const string DateToSettleCrossDunningLetterOfNextLevel = "'{0}'+'{1}' should not exceed the '{0}' of the next level Dunning Letter.";
		public const string NoStatementToRegenerate = "No statements to regenerate. You can prepare a statement according to a statement cycle by using the Prepare Statements (AR503000) form.";
		public const string StatementCycleNotSpecified = "Statement Cycle not specified for the Customer.";
		public const string StatementCycleDayEmpty = "Day Of Month must be number between 1 and 31.";
		public const string StatementCycleDayIncorrect = "If the day of a month is set to {0}, statements will be generated on the last day of a month for the months that are shorter than {0} days.";
		public const string NoStatementsForCustomer = "There is no Statement available for the Customer.  Go to Prepare Statement to create a Statement.";
		public const string StatementCannotBeGeneratedBecauseLaterDateIsExists = "A statement cannot be regenerated for the {0} customer on the {1:d} date because there is a statement prepared for the customer on a later date. Use the Regenerate Last Statement action on the Customers (AR303000) form to regenerate the latest statement.";
		public const string ReasonCodeIsRequired = "Reason Code must be specified before running the process.";
		public const string GroupDiscountExceedLimit = "Total group discount exceeds limit configured for this customer class. Document Discount was not calculated.";
		public const string OnlyGroupDiscountExceedLimit = "Total group discount exceeds limit configured for this customer class.";
		public const string DocDiscountExceedLimit = "The total amount of group and document discounts cannot exceed the limit specified for the customer class ({0:F2}%) on the Customer Classes (AR201000) form.";
		public const string OnlyOneDocumentDiscountAllowed = "Only one Document Discount allowed.";
		public const string OneOrMoreItemsAreNotReleased = "One or more items are not released";
		public const string OneOrMoreItemsAreNotProcessed = "One or more items are not processed";
		public const string DuplicateGroupDiscount = "Duplicate Group Discount.";
		public const string AccountMappingNotConfigured = "Account Task Mapping is not configured for the following Project: {0}, Account: {1}";
		public const string LineDiscountAmtMayNotBeGreaterExtPrice = "The discount amount cannot be greater than the amount in the {0} column.";
		public const string DiscountPercentShouldBeBetweenMinus100And100 = "The discount percent should not be less than -100 and greater than 100.";
		public const string WriteOffIsDisabled = "Write-Off is disabled for the given customer. Set non zero write-off limit on the Customer screen and try again.";
		public const string WriteOffIsOutOfLimit = "Document balance exceeds the configured write-off limit for the given customer (Limit = {0}). Change the write-off limit on the Customer screen and try again.";
		public const string EffectiveDateExpirationDate = "The Expiration Date should not be earlier than the Effective Date.";
		public const string FreeItemMayNotBeEmpty = "Free Item may not be empty. Please select Free Item before activating discount.";
		public const string FreeItemMayNotBeEmptyPending = "Free Item may not be empty. Please select Pending Free Item and update discount before activating it.";
		public const string CarriersCannotBeMixed = "Common carrier and Local carrier cannot be mixed in one invoice. Tax calculation will be invalid.";
		public const string MultipleShipAddressOnInvoice = "Invoice references multiple shipments that were shipped to different locations. Tax calculation will be invalid.";
		public const string PaymentMethodNotConfigured = "To create tokenized payment methods you must first configure 'Payment Profile ID' in Payment Method's 'Settings for Use in AR'";
		public const string PostingToExternalTaxProviderFailed = "Document was released successfully but the tax was not posted to the external tax provider with the following message: {0}";
		public const string NotAllCardsShown = "Some cards for {0} payment method(s) are not shown here because their data is stored at processing center";
		public const string ApplicationDateChanged = "Application date was changed to date of card transaction";
		public const string PaymentAndCaptureDatesDifferent = "Payment date is different than date of capture transaction";
		public const string ApplicationAndCaptureDatesDifferent = "The date in the Application Date box must be the same as the date when the credit card transaction was captured.";
		public const string ApplicationAndVoidRefundDatesDifferent = "The date in the Application Date box must be the same as the date when the credit card transaction was voided or refunded.";
		public const string ExpirationLessThanEffective = "Expiration Date may not be less than Effective Date.";
		public const string DuplicateSalesPrice = "Duplicate Sales Price. This line overlaps with another Sales Price (Price: {0}, Effective Date: {1}, Expiration Date: {2})";
		public const string DuplicateSalesPriceWS = "Duplicate Sales Price.";
		public const string ProcessingCenterCurrencyDoesNotMatch = "The currency of the transaction ({0}) differs from the currency of the processing center ({1}).";
		public const string ProcessingCenterIsExternalAuthorizationOnly = "The {0} processing center does not support the Authorize action. The Capture action is supported only for payments that were pre-authorized externally.";
		public const string CardAssociatedWithExternalAuthorizationOnlyProcessingCenter = "The {0} card is associated with the {1} processing center that does not support the Authorize action. The Capture action is supported only for payments that were pre-authorized externally.";
		public const string CannotDeletedBecauseOfTransactions = "The document cannot be deleted because there are card transactions associated with it. Use the Void action to cancel the document.";
		public const string CardProcessingActionsNotAvailable = "The document can be processed as a document with the Balanced status. Card processing actions are no longer available because the Integrated Card Processing feature has been disabled.";
		public const string LastPriceWarning = "The system retains the last price and the current price for each item.";
		public const string HistoricalPricesWarning = "The system retains changes of the price records during {0} months.";
		public const string HistoricalPricesUnlimitedWarning = "The system retains changes of the price records for an unlimited period.";
		public const string AccounTaskMappingNotFound = "AR Account is mapped to an AccountGroup however there is no Account-DefaultTask mapping setup in the Project. Please correct and try again.";
		public const string UniqueItemConstraint = "Same Item cannot be listed more than once. Same item cannot belong to two or more active discount sequences of the same discount code";
		public const string UniqueBranchConstraint = "Same Branch cannot be listed more than once. Same branch cannot belong to two or more active discount sequences of the same discount code";
		public const string UniqueWarehouseConstraint = "Same Warehouse cannot be listed more than once. Same warehouse cannot belong to two or more active discount sequences of the same discount code";
		public const string UnconditionalDiscUniqueConstraint = "Unconditional discounts cannot have active overlapping sequences.";
		public const string NoDiscountFound = "The Discount Code {0} has no matching Discount Sequence to apply.";
		public const string DiscountGreaterLineTotal = "The total amount of line and document discounts cannot exceed the Detail Total amount.";
		public const string DiscountGreaterLineMiscTotal = "Discount Total may not be greater than Line Total + Misc. Total.";
		public const string DiscountGreaterDetailTotal = "The total amount of line and document discounts cannot exceed the Detail Total amount.";
		public const string NoApplicableSequenceFound = "No applicable discount sequence found.";
		public const string UnapplicableSequence = "Discount Sequence {0} cannot be applied to this document.";
		public const string DocumentDicountCanNotBeAdded = "Skip Document Discounts option is set for one or more group discounts. Document discount can not be added.";
		public const string SequenceExists = "Cannot delete a Discount if there already exist one or more Discount Sequences associated with the given Discount";
		public const string SequenceExistsApplicableTo = "Cannot change Applicable To for the Discount if there already exist one or more Discount Sequences associated with the given Discount";
		public const string NoLineSelected = "No Document Details line selected. Please select Document Details line.";
		public const string DiscountTypeCannotChanged = "Discount Type can not be changed if Discount Code has Discount Sequence";
		public const string DiscountsNotvalid = "One or more validations failed for the given discount sequence. Please fix the errors and try again.";
		public const string MultiplePriceRecords = "There are multiple price records (regular and promotional) that are effective on the same date. Use the Sales Price Worksheets (AR202010) form to create a worksheet by using the Copy Prices action.";
		public const string ToCreateWorksheetSpecifyTaxCalcMode = "To create a worksheet, select a tax calculation mode.";
		public const string RequiredField = "This field is required!";
		public const string ShippedNotInvoicedINtranNotReleased = "Please release inventory Issue {0} before releasing Invoice.";
		public const string ReversingDocumentExists = "A reversing document {0} {1} already exists. Do you want to continue?";
		public const string UnreleasedCreditMemoExistsForPrepaymentInvoice = "The prepayment invoice has already been applied to the {0} credit memo, but the credit memo is not released. Do you want to navigate to the credit memo?";
		public const string CannotReversePrepaymentInvoiceWithUnreleasedApplications = "The prepayment invoice has an unreleased application to the {0} document with the {1} type and cannot be reversed.";
		public const string CannotReverseApplicationOfCreditMemoToPrepaymentInvoice = "Application of a credit memo created on reverse of a prepayment invoice cannot be reversed.";
		public const string UnreleasedCreditMemoExists = "The invoice has already been applied to the {0} credit memo, but the credit memo is not released. Do you want to navigate to the credit memo?";
		public const string ScheduledDocumentAlreadyReleased = "One of the scheduled documents is already released. Cannot generate new documents.";
		public const string TaxAccountTaskMappingNotFound = "Tax account {0} is included in an account group but not mapped to a default task. Use the Account Task Mapping tab on the Projects (PM301000) form to associate the account with a project task and then try releasing the document again.";
		public const string ARAccountTaskMappingNotFound = "AR account {0} is included in an account group but not mapped to a default task. Use the Account Task Mapping tab on the Projects (PM301000) form to associate the account with a project task and then try releasing the document again.";
		public const string ChildCustomerShouldConsolidateBBFStatements = "Child accounts that consolidate balance to parent and use Balance Brought Forward statement type must consolidate statements as well.";
		public const string RelatedFieldChangeOnParentWarning = "Do you wish to update the {0} setting for all child accounts of this customer?";
		public const string StatementCycleShouldBeTheSameOnParentAndChildCustomer = "We recommend setting the same statement cycle as for the parent account.";
		public const string StatementTypeShouldBeOpenItemForParentChildAfterSeparation = "We recommend switching to Open Item statement type for both parent and child accounts.";
		public const string CustomerCantHaveSeparateStatement = "This customer can't have separate statement. Please view the statement for the parent customer {0}";
		public const string OnDemandStatementsAvailableOnlyForOpenItemType = "The system cannot generate statements of the Balance Brought Forward type on demand.";
		public const string ConsolidatingCustomersParentMustBeCustomer = "If either or both of the Consolidate Balance and Consolidate Statements check boxes are selected, only an account of the Customer or Customer & Vendor type can be used as a parent account.";
		public const string ConsolidatingCustomersParentMustNotBeChild = "If either or both of the Consolidate Balance and Consolidate Statements check boxes are selected, only a customer account that has no parent account assigned can be specified as a parent account.";
		public const string CannotCaptureInInvalidPeriod = "Cannot record CC transaction on {0} : {1}";
		public const string CustomerRelationshipCannotBeBroken = "Unreleased parent-child applications exist for this customer. Neither Parent Account nor the Consolidate Balance option can be changed until these are released or deleted. Check the following documents: {0}";
		public const string ShouldSpecifyRoundingLimit = "To use this rounding rule, specify Rounding Limit for the base currency on the Currencies (CM202000) form.";
		public const string UnprocessedPPDExists = "The report cannot be generated. There are documents with unprocessed cash discounts. To proceed, make sure that all documents are processed on the Generate VAT Credit Memos (AR504500) form and appropriate VAT credit memos are released on the Release AR Documents (AR501000) form.";
		public const string UnprocessedPPDExistsBeforeClosing = "There are documents with unprocessed cash discounts. Before you proceed, process these documents by generating and releasing credit memos on the Generate VAT Credit Memos (AR.50.45.00) form.";
		public const string PartialPPD = "Cash discount can be applied only on final payment.";
		public const string PaidPPD = "This document has been paid in full. To close the document, apply the cash discount by generating a credit memo on the Generate VAT Credit Memos (AR.50.45.00) form.";
		public const string PPDApplicationExists = "To proceed, you have to reverse application of the final payment {0} with cash discount given.";
		public const string SelfVoidingDocPartialReverse = "The document should be voided in full. The reversing applications cannot be deleted partially.";		
		public const string SharedChildCreditHoldChange = "The status can not be changed. This is a child account that shares the credit policy with its parent account. Change the status of a parent account and the value will propagate to all child accounts.";
		public const string AdjustRefersNonExistentDocument = "Failed to process an application between documents {0} {1} and {2} {3} -	one of these documents cannot be found. Check whether both documents exist in the system.";
		public const string DuplicateCCProcessingID = "The Token ID {0} cannot be added to the selected payment method because it is already used in another customer payment method ({1})";
		public const string CustomerProfileIDAlreadyInUse = "The {0} customer profile ID cannot be added to the selected payment method because it is already used for the {1} customer.";
		public const string CreditCardTokenIDNotFound = "Card token ID cannot be found.";
		public const string CreditCardNotFoundInProcCenter = "No card with token ID {0} is found in the payment processing center {1}.";
		public const string TransactionHasExpired = "Transaction has already expired.";
		public const string DocumentAlreadyExistsWithTheSameReferenceNumber = "A {0} with this Reference Nbr. already exists in the system. Please, specify another reference number.";
		public const string ApplicationStateInvalid = "The application cannot be processed and your changes are not saved. Cancel the changes and start over. If the error persists, please contact support.";
		public const string AnotherChargeInvoiceRunning = "Another 'Generate Payments' process is already running. Please wait until it is finished.";
		public const string UnknownStatementType = "Unknown customer statement type.";
		public const string UnableToApplyDocumentApplicationDateEarlierThanDocumentDate = "Unable to apply the document because the application date is earlier than the document date.";
		public const string UnableToApplyDocumentApplicationPeriodPrecedesDocumentPeriod = "Unable to apply the document because the application period precedes the financial period of the document.";
		public const string AmountEnteredExceedsRemainingCashDiscountBalance = "The amount entered exceeds the remaining cash discount balance {0}.";
		public const string FailedToSyncCC = "Credit card data cannot be synchronized. Please process the synchronization manually.";
		public const string CustomerProfileBelongsToAnotherCustomer = "The {0} customer profile belongs to the {1} customer.";
		public const string DocumentCannotBeScheduled = "The document cannot be added to a schedule. Only balanced documents originated in the Accounts Receivable module can be added to a schedule.";
		public const string EndDayOfAgingPeriodShouldNotBeEarlierThanStartDay = "The end day of the aging period should not be earlier than its start day.";
		public const string StatementCoveringDateAlreadyExistsForCustomer = "The statement that covers the selected date has already been generated for the customer.";
		public const string ImpossibleToAgeDocumentUnexpectedBucketNumber = "The system could not age one of the documents included in the statement because an unexpected aging period number was produced by the aging engine. Please contact support service.";
		public const string UnexpectedRoundingForApplication = "The document cannot be released because unexpected rounding difference has appeared.";
		public const string UnableToCalculateNextStatementDateForEndOfPeriodCycle = "The next statement date cannot be determined for a statement cycle with the End of Period schedule type.";
		public const string UnableToCalculateBucketNamesPeriodsPrecedingNotDefined = "The aging period names cannot be determined for a statement cycle with the End of Period schedule type. The financial period should be defined for the aging date on the Financial Periods (GL201000) form, as well as the four preceding financial periods.";
		public const string UnableToCalculateBucketNamesPeriodsAfterwardsNotDefined = "The aging period names cannot be determined for a statement cycle with the End of Period schedule type. The financial period should be defined for the aging date on the Financial Periods (GL201000) form, as well as the four subsequent financial periods.";
		public const string ReturnReason = IN.Messages.Return;
        public const string DiscountCodeAlreadyExistAP = "The discount code already exists in AP. Specify another discount code.";
		public const string MigrationModeIsActivated = "Migration mode is activated in the Accounts Receivable module.";
		public const string ReverseRetainage = "The Reverse action cannot be used for invoices with retainage and for retainage documents. To reverse this document, use the Reverse and Apply to Memo action.";
		public const string ReversePaymentsByLines = "The Reverse and Apply to Memo action cannot be used when the Pay by Line check box is selected for a document. To reverse this document, use the Reverse action.";
		public const string ReverseRetainageReversingDocument = "The Reverse action cannot be used for credit and debit memos that reverse original documents with retainage and retainage documents.";
		public const string ReverseApplyRetainageReversingDocument = "The Reverse and Apply to Memo action cannot be used for credit and debit memos that reverse original documents with retainage and retainage documents.";
		public const string PaymentsByLinesCanBePaidOnlyByLines = "The document was created when the Payment Application by Line feature was enabled. Paying this document can cause inconsistency in balances. To pay the document, on the Invoices and Memos (AR301000) form, select Actions > Enter Payment/Apply Memo.";
		public const string NotDistributedApplicationCannotBeReleased = "The application cannot be released because it is not distributed between document lines. On the Payments and Applications (AR302000) form, delete the application, and apply the payment or credit memo to the document lines.";
		public const string InvoicePaymentsByLinesCanBePaidOnlyByLines = "The document has the Pay by Line check box selected and cannot be applied on this form, because Amount Paid is not distributed between document lines. To proceed, release the credit memo, open it on the Payments and Applications (AR302000) form, and apply to the document lines.";
		public const string PayByLineCreditMemoCannotBeUsedAsPayment = "The {0} credit memo is paid by line and cannot be applied to documents directly. To apply the credit memo, on the Payments and Applications (AR302000) form, create a payment and apply the credit memo lines and the needed invoice to it.";
		public const string PayByLineCreditMemoCannotBeWrittenOff = "The {0} credit memo is paid by line; write-offs are not supported for such credit memos. To write off the credit memo, create a debit memo and apply this credit memo to it on the Invoices and Memos (AR301000) form.";
		public const string MigrationModeIsActivatedForRegularDocument = "The document cannot be processed because it was created when migration mode was deactivated. To process the document, clear the Activate Migration Mode check box on the Accounts Receivable Preferences (AR101000) form.";
		public const string MigrationModeIsDeactivatedForMigratedDocument = "The document cannot be processed because it was created when migration mode was activated. To process the document, activate migration mode on the Accounts Receivable Preferences (AR101000) form.";
		public const string CannotReleaseMigratedDocumentInNormalMode = "The document cannot be released because it has been created in migration mode but now migration mode is deactivated. Delete the document or activate migration mode on the Accounts Receivable Preferences (AR101000) form.";
		public const string CannotReleaseNormalDocumentInMigrationMode = "The document cannot be released because it was created when migration mode was deactivated. To release the document, clear the Activate Migration Mode check box on the Accounts Receivable Preferences (AR101000) form.";
		public const string CannotReleasePrepaymentInvoiceWithMultipleInstallmentCreditTerms = "The prepayment invoice cannot be released, because a prepayment invoice cannot have terms with the Multiple installment type.";
		public const string CannotReleasePrepaymentInvoiceWithEmptyPrepaymentAccount = "The document cannot be released because the Prepayment Account box is empty on the Financial tab of the Invoices and Memos (AR301000) form. To proceed, fill in this box.";
		public const string CannotReleasePrepaymentInvoiceWithEmptyPrepaymentSubaccount = "The document cannot be released because the Prepayment Subaccount box is empty on the Financial tab of the Invoices and Memos (AR301000) form. To proceed, fill in this box.";
		public const string CannotReleasePrepaymentInvoiceWithEmptyPendingTaxPayableAccount = "The document cannot be released because the Pending Tax Payable Account box is empty for the {0} tax. To proceed, fill in this box on the Taxes (TX205000) form.";
		public const string CannotReleasePrepaymentInvoiceWithEmptyPendingTaxPayableSubaccount = "The document cannot be released because the Pending Tax Payable Subaccount box is empty for the {0} tax. To proceed, fill in this box on the Taxes (TX205000) form.";
		public const string CannotVoidMigratedPaymentWithInitialApplication = "The payment cannot be voided because it has been created in migration mode and contains an initial application. To proceed, void the payment in migration mode and manually post a CA disbursement to update the cash account.";
		public const string CannotReverseRegularApplicationInMigrationMode = "The application cannot be reversed because it was created when migration mode was deactivated. To process the application, clear the Activate Migration Mode check box on the Accounts Receivable Preferences (AR101000) form.";
		public const string EnterInitialBalanceForUnreleasedMigratedDocument = "Enter the document open balance to this box.";
		public const string ExistingOnDemandStatementsForCustomersOverwritten = "On-demand statements have been overwritten for the following customers: {0}.";
		public const string CustomersExcludedBecauseStatementsAlreadyExistForDate = "The following customers have been excluded from the processing because statements that cover the selected date already exist: {0}.";
		public const string CustomersExcludedFromStatementGeneration = "Some customers have been excluded from statement generation. Check Trace for details.";
		public const string OnDemandStatementsOnlyCannotRegenerate = "The customer has on-demand statements only. They cannot be regenerated. You can generate a new on-demand statement, or prepare a statement according to a statement cycle by using the Prepare Statements (AR503000) form.";
		public const string CannotPerformActionOnDocumentUnreleasedVoidPaymentExists = "{0} {1} cannot be {2} because an unreleased {3} exists for the document. To proceed, delete {4} {5} or complete the voiding process by releasing it.";
		public const string ReleasedProforma = "You cannot delete the document that refers to a pro forma invoice because pro forma invoices can be reopened starting from the last one. To delete the document, at first delete the following documents: {0}.";
		public const string FinancialPeriodClosedInAR = "The {0} financial period of the {1} company is closed in Accounts Receivable.";
		public const string ContinueValidatingBalancesForMultipleCustomers = "Validation of balances for multiple customers may take a significant amount of time. We recommend that you select a particular customer for balance validation to reduce time of processing. To proceed with the current settings, click OK. To select a particular customer, click Cancel.";
		public const string InvoiceCreditHoldCannotRelease = "The {0} {1} is on credit hold and cannot be released.";
		public const string PeriodEarlierThanPeriodOfOriginalDocument = "The retainage document cannot be posted to the period earlier than the post period of the related original document with retainage. The value must be greater than or equal to {0}.";
		public const string WrongOrderNbr = "The order cannot be applied, the specified combination of the order type and order number cannot be found in the system.";
		public const string NoUnitPriceFound = "Unit price has been set to zero because no effective unit price was found.";
		public const string AvalaraAddressSourceError = "Taxes cannot be calculated via the external tax provider because the address is missing for {0} {1}.";
		public const string GLTransExist = "The document cannot be deleted because GL batches have been released for the document. To resolve the issue, please contact your Acumatica support provider.";
		public const string ARTransExist = "The document cannot be deleted as it has been partially released. To resolve the issue, please contact your Acumatica support provider.";
		public const string TaxAccountNotFound = "The document cannot be released because the {0} is not specified for the {1} tax. To proceed, specify the account on the Taxes (TX205000) form.";
		public const string DefaultCostCodeIsExpected = "You can select only the Default Cost Code as the Cost Code because the Revenue Budget Level of the project is Task.";
		public const string InvoiceWasNotReleasedBecauseCreditCardPaymentIsNotCaptured = "Invoice was not released because the credit card payment for the invoice is not captured.";
		public const string PromotionalCannotBeFairValue = "The price cannot be both promotional and fair value.";
		public const string SkipLineDiscAndFairValueCannotBeSelectedBoth = "The Ignore Automatic Line Discounts and Fair Value check boxes cannot be selected simultaneously.";
		public const string UnreleasedDocsWithDRCodes = "There are {0} unreleased AR or SO documents with DR codes. Switching to this mode before releasing them may lead to unwanted results. Do you want to continue?";
		public const string MDAInventoriesWithoutAllocationMethod = "There are one or more stock items marked as Multi-Deliverable Arrangement, which have no information about the allocation method in the Revenue Components table on the Deferral Settings tab of the Stock Items (IN202500) form. You must enter this information in order to create AP, AR, or SO documents with such stock items.";
		public const string InventoryIDCouldNotBeEmpty = "Inventory ID cannot be empty.";
		public const string ContractInvoiceDeletionConfirmation = "Deletion of the document will not affect the data in the related {0} contract, such as the billed usage or the next billing date. To cancel the last action performed on this contract, on the Customer Contracts (CT301000) form, click Actions > Undo Last Action. To proceed with deletion, click OK.";
		public const string DocumentsNotValidated = "At least one document could not be validated.";
		public const string KeepLocationBranchConfirmation ="Do you want to keep the value in the Shipping Branch box?";
		public const string CopyProjectInformationToAPDocument = "Do you want to copy the project information to the AP document?";
		public const string ProjectCannotBeCopiedBecauseProjectVisibilitySettings = "The {0} project cannot be copied because the AP subledger is not added in its visibility settings.";
		public const string BranchIsNotExtendedToVendor = "The {0} branch has not been extended to a vendor.";
		public const string RecalculateBalanceTooltip = "Recalculate balances of customers and customer documents";
		public const string EntityCannotBeAssociated = "An entity with the base currency other than {0} cannot be associated with {1}, because there are AR documents for the customer.";
		public const string EntityCannotBeAssociatedBecauseOfAPHistory = "An entity with the base currency other than {0} cannot be associated with {1}, because the customer has been extended as a vendor whose visibility is limited to the {2} entity with the {3} base currency and there are AP documents for the vendor.";
		public const string EntityCannotBeAssociatedBecauseOfARHistoryNoRestrictForVendor = "The entity with the base currency other than {0} cannot be associated with {1}, because the customer has been extended as a vendor and there are AP documents for the vendor.";
		public const string ChangeVisibilityForVendor = "The {0} customer has been extended as a vendor whose visibility is limited to the {1} entity with the {2} base currency. Do you want to change the visibility of both the customer and vendor accounts? To proceed, click Yes.";
		public const string SetVisibilityForVendor = " The {0} customer has been extended as a vendor whose visibility is not limited to any entity. Do you want to change the visibility of both the customer and vendor accounts? To proceed, click Yes.";
		public const string PendingReviewCCDocCannotBeProcessed = "The document cannot be processed automatically. Please review the document.";
		public const string SelectBalancesToBeRecalculated = "Specify which balances should be recalculated by selecting the needed check boxes in the Selection area.";
		public const string CannotBeUsedAsParent = "The {0} account cannot be used as a parent account, because its usage is limited to the entity whose base currency differs from the base currency of {1} associated with the {2} customer.";
		public const string CannotBeUsedAsParentRestrictor = "Cannot be used as a parent account.";
		public const string InvoiceIsLinkedToShipments = "The invoice is linked to one or multiple shipments. If you need only to reverse the invoice, click OK to proceed. If you need to correct the current invoice, or cancel it and prepare another invoice for the shipped goods, click Cancel to cancel reversing, and then use the Correct or Cancel action on the Invoices (SO303000) form.";
		public const string ChangesInTheDunningFee = "The changes in the dunning fee will be lost. Continue?";
		public const string EntityWithBaseCurencyCannotBeAssociatedWithCustomer = "An entity with the base currency other than {0} cannot be associated with the customer, because it is the parent account for the {1} customer whose usage is limited to {2} with the {3} base currency.";
		public const string DRScheduleNotExist = "Deferral schedule does not exist for the document.";
		public const string DunningFeeItem = "To charge a dunning letter fee, select an item in the Dunning Fee Item box (Dunning tab) of the Accounts Receivable Preferences (AR101000) form.";
		public const string DRScheduleNotExistForDetailsLine = "Deferral schedule does not exist for the selected line.";
		public const string NotReleasedSOInvoiceCannotBeSelectedForPayment = "The {0} document with the {1} number cannot be selected for payment because it is not released yet.";
		public const string DocumentNotApproved = "The document cannot be released because it has not been approved yet.";
		public const string ARSetupOverdueChargeNumberingIDCannotBeManual = "Manual numbering cannot be used for documents of the Overdue Charge type. Clear the Manual Numbering check box for the {0} numbering sequence on the Numbering Sequences (CS201010) form.";
		public const string DoesNotSupportIntegratedProcessingWarning = "The system does not support integrated processing for voided refunds with the Credit Card and EFT payment methods. You need to void a refund transaction manually in the processing center, and then release the voided refund in the system.";
		public const string ApplicationCannotBeReleasedBecauseBranchIsInactive = AP.Messages.ApplicationCannotBeReleasedBecauseBranchIsInactive;
		public const string ExternalTaxPostFailUsePostTaxForm = "Use the Post Taxes (TX501500) form to post the taxes for the document to the external tax provider.";
		public const string CannotSelectMultipleInstallmentCreditTermsForPrepaymentInvoice = "A prepayment invoice cannot have terms with the Multiple installment type.";
		public const string CannotSelectCreditTermsWithDiscountForPrepaymentInvoice = "A prepayment invoice cannot have terms with a cash discount.";
		#endregion

		#region Translatable Strings used in the code
		public const string ViewLastCharge = "View Last Charge";
		public const string NewSchedule = "New Schedule";
		public const string ViewSchedule = "View Schedule";
		public const string MultiplyInstallmentsTranDesc = AP.Messages.MultiplyInstallmentsTranDesc;
		public const string NewInvoice = "Create Invoice";
		public const string NewPayment = "Create Payment";
		public const string CustomerPriceClass = "Customer Price Class";
		public const string AllPrices = "All Prices";
		public const string AllModes = "All Modes";
		public const string BasePrice = "Base";
		public const string Undefined = "Not Set";
		public const string TaxNet = AP.Messages.TaxNet;
		public const string TaxGross = AP.Messages.TaxGross;
		public const string ARAccess = "Customer Access";
		public const string ARAccessDetail = "Customer Access Detail";
		public const string Warning = "Warning";
		public const string SalesPerson = "Sales Person";
		public const string VoidCommissions = "Void Commissions";
		public const string ClosePeriod = "Close Period";
		public const string ReopenPeriod = "Reopen Period";
		public const string Days = "Days";
		public const string DocumentDateSelection = "Document Date Selection";
		public const string Shipping = "Shipping";
		public const string Billing = "Billing";
		public const string ReviewSPComissionPeriod = "Review Commission Period";
		public const string Attention = "Attention!";
                public const string Process = "Process";
		public const string ProcessAll = "Process All";
		public const string CustomerPaymentMethodView = "Customer Payment Method";
		public const string CustomerView = "Customer";
		public const string BillingContactView = "Billing Contact";
		public const string CashSaleInvoice = "Cash Sale Invoice";
		public const string CashReturnInvoice = "Cash Return Invoice";
		public const string Calculate = "Calculate";
		public const string AllTransactions = "All Transactions";
		public const string FailedOnlyTransactions =  "Failed Only";
		public const string CreditCardIsExpired = "CC Expired";
		public const string ViewVendor = "View Vendor";
		public const string ViewBusnessAccount = "View Account";
		public const string ExtendToVendor = "Extend as Vendor";
		public const string ImportedExternalCCTransaction = "Imported External Transaction";
		public const string LostExpiredTranVoided = "Lost or Expired Transaction was Voided";
		public const string ARBalanceByCustomerReport = "AR Balance by Customer";
		public const string CustomerHistoryReport = "Customer History";
		public const string ARAgedPastDueReport = "AR Aging";
		public const string ARAgedPastDueReportMC = "AR Aging MC";
		public const string ARAgedOutstandingReport = "AR Coming Due";
		public const string ARRegisterReport = "AR Register";
		public const string DocDiscDescr = "Group and Document Discount";
		public const string BasePriceClassDescription = "Base Price Class";
		public const string ViewARDiscountSequence = "View Discount Sequence";
		public const string SearchableTitleCustomer = "Customer: {0}";
		public const string SearchableTitleDocument = "AR {0}: {1} - {3}";
		public const string SearchableTitleDocumentWithDynamicPrefix = "{0} {1}: {2} - {4}";
		public const string TokenInputMode = "Profile ID";
		public const string DetailsInputMode = "Card Details";
		public const string PriceCode = "Price Code";
		public const string Description = "Description";
		public const string CreatePriceWorksheet = "Create Price Worksheet";
        public const string ReversingRGOLTanDescription = "Reverse Deposit RGOL";
		public const string DocType = "Doc. Type";
		public const string DocumentType = "Document Type";
		public const string DocRefNbr = "Doc Ref. Nbr";
		public const string Type = "Type";
		public const string CashDiscountTaken = "Cash Discount Taken";
		public const string AmountPaid = "Amount Paid";
		public const string BalanceWriteOff = "Write-Off Amount";
		public const string CalculateOnOverdueChargeDocuments = "Calculate on Overdue Charge Documents";
		public const string ApplyOverdueCharges = "Apply Overdue Charges";
		public const string ScheduleType = "Schedule Type";
		public const string AgeBasedOn = "Age Based On";
		public const string UseFinPeriodForAging = "Use Financial Periods for Aging";
		public const string PrintEmptyStatements = "Print Empty Statements";
		public const string StatementDate = "Statement Date";
		public const string AdjustmentNbr = "Adjustment Nbr.";
		public const string DayOfWeek = "Day of Week";
		public const string DayOfMonth = "Day of Month";
		public const string DayOfMonth1 = "Day of Month 1";
		public const string DayOfMonth2 = "Day of Month 2";
		public const string OnDemandStatement = "On-Demand Statement";
		public const string Current = "Current";
		public const string OpenItem = "Open Item";
		public const string BalanceBroughtForward = "Balance Brought Forward";
		public const string SendInvoicesWithStatement = "Send Invoices with Statement";
		public const string PrintInvoicesWithStatement = "Print Invoices with Statement";
		public const string StatementCycleID = "Statement Cycle ID";
		public const string StartDate = "Start Date";
		public const string EndDate = "End Date";
		public const string IncludeOnDemandStatements = "Include On-Demand Statements";
		public const string PreparedOn = "Prepared On";
		public const string FailedGetFrom = CA.Messages.FailedGetFrom;
		public const string FailedGetTo = CA.Messages.FailedGetTo;
		public const string DocTypeNotSupported = AP.Messages.DocTypeNotSupported;
		public const string EmptyValuesFromExternalTaxProvider = AP.Messages.EmptyValuesFromExternalTaxProvider;
		public const string InvalidReasonCode = "Invalid Reason Code Usage. Only Balance Write-Off or Credit Write-Off codes are expected.";
		public const string VoidingCommissionsFailed = "Voiding commissions for the selected period has failed";
		public const string Release = PM.Messages.Release;
		public const string ReleaseAll = PM.Messages.ReleaseAll;
		public const string Validate = CA.Messages.Validate;
		public const string ValidateAll = CA.Messages.ValidateAll;
		public const string Locale = "Locale";
		public const string PrepareFor = "Prepare For";
		public const string RequirePaymentApplicationBeforeStatement = "Require Payment Application Before Statement";
		public const string Message = "Message";
		public const string ReasonCodeNotFound = "No reason code with the given id was found in the system. Code: {0}.";
		public const string PaymentOfInvoice = "Payment of invoice {0}{1} - {2}";
		public const string CommissionPeriodNotClosed = "The commission period is not closed.";
		public const string ApprovalWorkGroupID = AP.Messages.ApprovalWorkGroupID;
		public const string DocumentsToApply = "Documents to Apply";
		public const string ApplicationHistory = "Application History";
		public const string OrdersToApply = "Orders to Apply";
		public const string CreditCardProcessingInfo = "Credit Card Processing Info";
		public const string CashAccount = "Cash Account";
		public const string CustomerLocation = "Customer Location";
		public const string FuturePayments = "The following payments have not been processed: {0}.";
		public const string FuturePaymentWarning = "Some payments in the system have not been processed because their payment dates are later than the application date selected for processing. See the trace log for more details.";
		public const string WriteOffDiscountGainLossAmountFor = "Write-off, cash discount, and RGOL amount for";
		public const string WriteOffGainLossAmountFor = "Write-off and RGOL amount for";
		public const string DiscountGainLossAmountFor = "Cash discount and RGOL amount for";
		public const string WriteOffDiscountAmountFor = "Cash discount and write-off amount for";
		public const string ConsolidatedRetainageDescription = "Retainage for project: {0} - {1}";
		public const string GainLossAmountFor = "RGOL amount for";
		public const string CashDiscountAmountFor = "Cash discount amount for";
		public const string WriteOffAmountFor = "Write-off amount for";
		public const string AppliedTo = "applied to";
		public const string ActionReleased = "released";
		public const string ActionWrittenOff = "written off";
		public const string ActionRefunded = "refunded";
		public const string ActionAdjusted = "adjusted";
		public const string Capture = "capture";
		public const string Void = "void";
		public const string Authorization = "authorization";
		public const string PrepaymentInvoiceWriteoff = "Write-off of prepayment invoice {0}";
		public const string PrepaymentInvoiceVoiding = "Voiding of prepayment invoice {0}";
		public const string TaxableAmount = "Taxable Amount";
		public const string TaxAmount = "Tax Amount";
		public const string OrigTaxableAmount = "Orig. Taxable Amount";
		public const string OrigTaxAmount = "Orig. Tax Amount";
		#endregion

		#region Graphs Names
		public const string ARAutoApplyPayments = "Payment Application Process";
		public const string ARCreateWriteOff = "Balance Write Off Process";
		public const string CustomerClassMaint = "Customer Classes Maintenance";
		public const string CustomerMaint = "Customer Maintenance";
		public const string CustomerPaymentMethodMaint = "Customer Payment Methods Maintenance";
		public const string ARInvoiceEntry = "AR Invoice Entry";		
		public const string ARPaymentEntry = "AR Payment Entry";
		public const string ARDocumentRelease = "AR Documents Release Process";
		public const string ARPrintInvoices = "AR Invoice Printing Process";
		public const string ARReleaseProcess = "AR Release Process";
		public const string ARCustomerBalanceEnq = "Customers Balance - Summary Inquiry";
		public const string ARDocumentEnq = "Customer Balance - Detail Inquiry";
		public const string ARStatementProcess = "Customer Statement Preparation Process";
		public const string ARStatementDetails = "Statements History - Details by Date Inquiry";
		public const string ARStatementPrint = "Customer Statement Printing Process";
		public const string ARStatementForCustomer = "Statements History - Details by Customer Inquiry";
		public const string ARStatementHistory = "Statements History - Summary Inquiry";
		public const string ARStatementMaint = "Statement Cycle Maintenance";
		public const string SalesPersonMaint = "Sales Person Maintenance";
		public const string ARSPCommissionProcess = "Sales Person Commission Preparation Process";
		public const string ARSPCommissionDocEnq = "Sales Person Commission - Details Inquiry";
		public const string ARSPCommissionReview = "Sales Person Commission Period Closing Process";
		public const string ARFinChargesApplyMaint = "Overdue Charges Calculation Process";
		public const string ARFinChargesMaint = "Overdue Charge Codes Maintenance";
		public const string ARIntegrityCheck = "Customer Balances Validation Process";
		public const string ARScheduleMaint = "AR Scheduled Tasks Maintenance";
		public const string ARScheduleProcess = "AR Scheduled Tasks Process";
		public const string ARScheduleRun = "AR Sheduled Tasks Processing List";
		public const string ARSetupMaint = "Accounts Receivables Setup";
		public const string ARSmallCreditWriteOffEntry = "Small Credit Write-Off Creation";
		public const string ARSmallBalanceWriteOffEntry = "Small Balance Write-Off Creation";
		public const string StatementCreateBO = "Statement Creation";
		public const string ARSPCommissionUpdate = "Commission History Creation";
		public const string ARTempCrLimitMaint = "Temporary Credit Limit Maintenance";
		public const string CCTransactionsHistoryEnq = "Credit Card Transactions History";
		public const string ARPriceClassMaint = "Customer Price Class Maintenance";
		public const string ARInvoice = "AR Invoice/Memo";
		public const string ARTran = "AR Transactions";
		public const string ARAddress = "AR Address";
		public const string ARShippingAddress = "AR Ship-To Address";
		public const string ARBillingAddress = "AR Bill-To Address";
		public const string ARContact = "AR Contact";
		public const string ARShippingContact = "AR Ship-To Contact";
		public const string ARBillingContact = "AR Bill-To Contact";
		public const string CCExpirationNotifyAll = "Notify All";
		public const string CCExpirationNotify = "Notify";
		public const string CCDeactivateAll = "Deactivate all";
		public const string CCDeactivate = "Deactivate";
                public const string DunningLetter = "Dunning Letter";
                public const string DunningLetterLevel = "Dunning Letter Level";
                public const string IncludeNonOverdue = "Add Coming-Due Invoices";
				public const string AddOpenPaymentsAndCreditMemos = "Add Open Payments and Credit Memos";
		public const string DunningLetterNotCreated = "One or more dunning letters was not created";
		public const string DunningLetterNotReleased = "A dunning letter was created, but couldn't be released because of the following error: {0}";
                public const string DunningLetterZeroLevel = "The Dunning Letter does not list any overdue documents, therefore it cannot be released.";
                public const string DunningLetterHavePaidFee = "The invoice for the Dunning Letter Fee has already been paid. To void the invoice you should void the payment first.";
                public const string DunningLetterHigherLevelExists = "The Dunning Letter cannot be voided. A Dunning Letter of a higher level exists for one or more documents. You should void the letters of a higher levels first.";
                public const string ViewDunningLetter = "View Dunning Letter";
                public const string DunningLetterStatus = "Dunning Letter Status";
                public const string DunningLetterFee = "Dunning Letter Fee";
		public const string DunningLetterEmptyInventory = "The invoice for Dunning Letter Fee cannot be generated as a Dunning Fee Item is not specified in Accounts Receivable Preferences.";
		public const string DunningLetterFeeEmptySalesAccount = "The non-stock item has no sales account specified, thus it cannot be used for recording the dunning fee. Select another item or specify a sales account for this item on Non-Stock Items (IN202000).";
		public const string DunningLetterExcludedCustomer = "The Customer will be excluded from Dunning Letter Process if both Print and Send by Email check boxes are cleared.";
		public const string DunningLetterProcessSwithcedToCustomer = "If you switch the mode to the By Customer option, the system will assign the highest dunning level found among customer documents to a customer account. The dunning letters will be prepared starting at this level, and the documents that have lower levels will be included in the first prepared letter.";
                public const string DunningProcessTypeDocument = "By Document";
                public const string DunningProcessTypeCustomer = "By Customer";
		public const string DunningProcessFeeEmptySalesAccount = "The invoice for the dunning fee cannot be generated. The non-stock item specified in the Dunning Fee Item box on Accounts Receivable Preferences (AR101000) has no sales account. Select another item or specify a sales account for this item on Non-Stock Items (IN202000).";
                public const string IncludeAllToDL = "All Overdue Documents";
                public const string IncludeLevelsToDL = "Dunning Letter Level";
		public const string ARExternalTaxCalc = "AR External Tax Posting";
		public const string ARSetup = "Account Receivable Preferences";

		#endregion 

                #region View Names
                #endregion

		#region DAC Names
		public const string CustomerPaymentMethodInfo = "Customer Payment Method";
		public const string CustomerPaymentMethod = "Customer Payment Method";
		public const string CustomerPaymentMethodDetail = "Customer Payment Method Detail";
		public const string CustomerPaymentMethodInputMode = "Customer Payment Method Input Mode";
		public const string ARSalesPerTran = "AR Salesperson Commission";
		public const string ARTaxTran = "AR Tax";
		public const string ARAdjust = "Applications";
		public const string ARPayment = "AR Payment";
		public const string ARPaymentTotals = "AR Payment Totals";
		public const string CustSalesPeople = "Customer Salespersons";		
		public const string CustomerBalanceSummary = "Balance Summary";
		public const string ARCashSale = "Cash Sale";
		public const string CustomerClass = "Customer Class";
		public const string StatementCycle = "Statement Cycle";
		public const string Statement = "AR Statement";

		public const string ARBalances = "AR Balance";
		public const string ARBalanceByCustomer = "AR Balance by Customer";
		public const string ARBalancesByBaseCurrency = "AR Balance by Base Currency";
		public const string ARBalancesSharedCredit = "AR Balance Shared Credit";
		public const string CustomerHistory = "Customer History";
		[Obsolete(PX.Objects.Common.InternalMessages.FieldIsObsoleteAndWillBeRemoved2020R2)]
		public const string ARAgedPastDue = "AR Aged Past Due";
		[Obsolete(PX.Objects.Common.InternalMessages.FieldIsObsoleteAndWillBeRemoved2020R2)]
		public const string ARAgedOutstanding = "AR Aged Outstanding";
		public const string ARRegister = "AR Register";
		public const string CustomerDetails = "Customer Profile";
		public const string DocumentSelection = "AR Document to Process";
		public const string ARDocument = "AR Document";
		public const string ARHistory = "AR History";
		public const string ARHistoryForReport = "AR History for Report";
		public const string ARHistoryByPeriod = "AR History by Period";
		public const string BaseARHistoryByPeriod = "Base AR History by Period";

		public const string ARInvoiceDiscountDetail = "AR Invoice Discount Detail";
                public const string DiscountSequence = "Discount Sequence";
		public const string ARDunningLetterDetail = "Dunning Letter Detail";

		public const string ARLatestHistory = "AR Latest History";
		public const string ARDiscount = "AR Discount";
		public const string ARDunningSetup = "AR Dunning Setup";
		public const string ARFinCharge = "AR Financial Charge";
		public const string ARFinChargePercent = "AR Financial Charge Percent";
		public const string ARFinChargeTran = "AR Financial Charge Transaction";
		public const string ARInvoiceNbr = "AR Invoice Nbr";
		public const string ARNotification = "AR Notification";
		public const string ARPaymentChargeTran = "AR Payment Charge Transaction";
		public const string ARPriceClass = "AR Price Class";
		public const string ARPriceWorksheet = "AR Price Worksheet";
		public const string ARPriceWorksheetDetail = "AR Price Worksheet Detail";
		public const string ARSalesPrice = "AR Sales Price";
		public const string ARSPCommissionPeriod = "AR Salesperson Commission Period";
		public const string ARSPCommissionYear = "AR Salesperson Commission Year";
		public const string ARSPCommnHistory = "AR Salesperson Commission History";
		public const string ARStatementDetail = "AR Statement Detail";
		public const string ARStatementDetailInfo = "AR Statement Detail Info";
		public const string ARStatementAdjust = "AR Statement Application Detail";
		public const string ARTax = "AR Tax Detail";
		public const string CCProcTran = "Credit Card Processing Transaction";
		public const string CuryARHistory = "Currency AR History";
		public const string DiscountBranch = "Discount for Branch";
		public const string DiscountCustomer = "Discount for Customer";
		public const string DiscountCustomerPriceClass = "Discount for Customer and Price Class";
		public const string DiscountInventoryPriceClass = "Discount for Inventory and Price Class";
		public const string DiscountItem = "Discount Item";
		public const string DiscountSequenceDetail = "Discount Sequence Detail";
		public const string DiscountSequenceBreakpoint = "Discount Breakpoint";
		public const string DiscountSite = "Discount for Warehouse";
		#endregion

		#region Document Type
		public const string Register = "Register";		
		public const string Invoice = "Invoice";
		public const string DebitMemo = "Debit Memo";
		public const string CreditMemo = "Credit Memo";
		public const string Payment = "Payment";
		public const string Prepayment = "Prepayment";
		public const string Refund = "Refund";
		public const string VoidPayment = "Voided Payment";
        public const string VoidRefund = "Voided Refund";
        public const string FinCharge = "Overdue Charge";
		public const string SmallBalanceWO = "Balance WO";
		public const string SmallCreditWO = "Credit WO";
		public const string CashSale = "Cash Sale";
		public const string CashReturn = "Cash Return";
		public const string NoUpdate = "No Update";
		public const string InvoiceOrCreditMemo = "Invoice/Credit Memo";
		public const string CashSaleOrReturn = "Cash Sale/Cash Return";
		public const string PrepaymentInvoice = "Prepayment Invoice";

		#endregion

		#region ChargingMethod
		public const string FixedAmount = "Fixed Amount";
                public const string PercentWithThreshold = "Percent with Threshold";
                public const string PercentWithMinAmount = "Percent with Min. Amount";
                #endregion

                #region CalculationMethod
                public const string InterestOnBalance = "Interest on Balance";
                public const string InterestOnProratedBalance = "Interest on Prorated Balance";
                public const string InterestOnArrears = "Interest on Arrears";
                #endregion

                #region Recalculate Discounts Options
                public const string CurrentLine = "Current Line";
		public const string AllLines = "All Lines";
		#endregion

		#region Report Document Type
		public const string PrintInvoice = Invoice;
		public const string PrintDebitMemo = DebitMemo;
		public const string PrintCreditMemo = CreditMemo;
		public const string PrintPayment = Payment;
		public const string PrintPrepayment = Prepayment;
		public const string PrintRefund = Refund;
		public const string PrintVoidRefund = VoidRefund;
		public const string PrintVoidPayment = VoidPayment;
		public const string PrintFinCharge = FinCharge;
		public const string PrintSmallBalanceWO = SmallBalanceWO;
		public const string PrintSmallCreditWO = SmallCreditWO;
		public const string PrintCashSale = CashSale;
		public const string PrintCashReturn = CashReturn;
		public const string PrintPrepaymentInvoice = Invoice;
		#endregion

		#region Document Status
		public const string Hold = "On Hold";
		public const string Balanced = "Balanced";
		public const string Voided = "Voided";
		public const string Scheduled = "Scheduled";
                public const string Open = "Open";
                public const string Draft = "Draft";
		public const string Closed = "Closed";
		public const string PendingPrint = "Pending Print";
		public const string PendingEmail = "Pending Email";
		public const string CCHold = "Pending Processing";
		public const string CreditHold = "Credit Hold";
		public const string PendingApproval = "Pending Approval";
		public const string Released = "Released";
		public const string Reserved = "Reserved";
		public const string Rejected = "Rejected";
		public const string Canceled = "Canceled";
		public const string PendingPayment = "Pending Payment";
		#endregion

		#region Tran post type
		public const string Origin = "Original document";
		public const string Application = "Application";
		public const string Adjustment = "Adjustment";
		public const string Retainage = "Release Retainage";
		public const string RetainageReverse = "Reverse Retainage";
		public const string Installment = "Installment";
		public const string RGOL = "Realized and Rounding GOL";
		public const string Rounding = "Realized and Rounding GOL";
		public const string PrepaymentInv = "Prepayment Invoice";
		#endregion

		#region AR Mask Codes
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R1)]
		public const string MaskItem = "Non-Stock Item";
		public const string MaskNonStockItem = AP.Messages.MaskNonStockItem;
		public const string MaskInventoryItem = AP.Messages.MaskInventoryItem;
		public const string MaskCustomer = "Customer";
                public const string MaskLocation = "Customer Location";
                public const string MaskEmployee = "Employee";
		public const string MaskCompany = "Branch";
		public const string MaskSalesPerson = "Salesperson";
		#endregion

		#region Prepare Statements
		public const string ForEachBranch = "For Each Branch";
		public const string ConsolidatedForCompany = "Consolidated for Company";
		public const string ConsolidatedForAllCompanies = "Consolidated for All Companies";
		public const string StatementsCannotBePrinted = "The statements cannot be printed because it is not allowed to consolidate statements for all companies if the Multiple Base Currency feature is enabled. Select a different option in the Prepare Statements box on the Accounts Receivable Preferences (AR101000) form.";
		#endregion

		#region Commission Period Type
		public const string Monthly = "Monthly";
		public const string Quarterly = "Quarterly";
		public const string Yearly = "Yearly";
		public const string FiscalPeriod = "By Financial Period";
		#endregion

		#region PMInstanceSearchType

		public const string PMInstanceSearchByPartialNumber = "Search by Partial Number";
		public const string PMInstanceSearchByFullNumber = "Search by Full Number";
		
		#endregion

		#region Commission Period Status
		public const string PeriodPrepared = "Prepared";
		public const string PeriodOpen = "Open";
		public const string PeriodClosed = "Closed";
		#endregion

		#region SPCommnCalcTypes
		public const string ByInvoice = "Invoice";
		public const string ByPayment = "Payment";
		#endregion

		#region CCProcessingState
		public const string CCNone = "None";
		public const string CCUnknown = "Unknown";
		public const string CCPreAuthorized ="Pre-Authorized";
		public const string CCPreAuthorizedShort = "Pre-Auth.";
		public const string CCPreAuthorizationFailed = "Pre-Authorization Failed";
		public const string CCCaptured = "Captured";
		public const string CCCaptureFailed = "Capture Failed";
		public const string CCVoided = "Voided";
		public const string CCVoidFailed = "Voiding failed";
		public const string CCRefunded = "Refunded";
		public const string CCRefundFailed = "Refund Failed";
		public const string CCPreAuthorizationExpired = "Pre-Authorization Expired";
		public const string CCCapturedHoldingReviewExpired = "Held for Review (Capture) Expired";
		public const string CCAuthorizedHoldingReview = "Held for Review (Authorization)";
		public const string CCCapturedHoldingReview = "Held for Review (Capture)";
		public const string CCVoidedHoldingReview = "Held for Review (Void)";
		public const string CCRefundedHoldingReview = "Held for Review (Refund)";
		public const string CCPreAuthorizationDeclined = "Pre-Authorization Declined";
		public const string CCCaptureDeclined = "Capture Declined";
		public const string CCVoidDeclined = "Voiding Declined";
		public const string CCRefundDeclined = "Refund Declined";
		#endregion

		#region Save Cards
		public const string CCUponConfirmationSave = "Upon Confirmation";
		public const string CCNeverSave = "Never";
		public const string CCAlwaysSave = "Always";
		#endregion

		#region SyncStatus
		public const string CCSyncStatusNone = "None";
		public const string CCSyncStatusWarning = "Warning";
		public const string CCSyncStatusError = "Error";
		public const string CCSyncStatusSuccess = "Success";
		#endregion

		#region SyncErrors
		public const string CCAuthorizationSyncFailed = "Validation Failed (Authorization)";
		public const string CCCaptureSyncFailed = "Validation Failed (Capture)";
		public const string CCRefundSyncFailed = "Validation Failed (Refund)";
		public const string CCVoidSyncFailed = "Validation Failed (Voiding)";
		public const string CCSyncFailed = "Validation Failed";
		public const string CCVoidRefundSyncFailed = "Void/Refund Failed Validation";
		public const string CCAuthCaptureSyncFailed = "Pre-Auth./Capture Failed Validation";
		#endregion

		#region NeedSync
		public const string CCVoidRefundNeedSync = "Void/Refund Pending Validation";
		public const string CCAuthCaptureNeedSync = "Pre-Auth./Capture Pending Validation";
		#endregion

		#region Load Child Documents Options
		public const string None = "None";
		public const string ExcludeCRM = "Except Credit Memos";
		public const string IncludeCRM = "All Types";
		#endregion

		#region Custom Actions
		public const string ViewCustomer = "View Customer";
		public const string ViewPaymentMethod = "View Payment Method";
		public const string ViewCustomerPaymentMethod = "View Customer Payment Method";
		public const string ViewProcessingCenter = "View Processing Center";
		public const string ViewExternalTransaction = "View External Transaction";
		public const string ViewPayment= "View Payment";
		public const string ViewDocument = "View Document";
		public const string ViewOrigDocument = "View Orig. Document";
		public const string ProcessPrintStatement = "Print Statement";
		public const string ProcessEmailStatement = "Email Statement";
		public const string ProcessMarkDontEmail = "Mark as Do not Email";
		public const string ProcessMarkDontPrint = "Mark as Do not Print";
                public const string ProcessReleaseDunningLetter = "Release Dunning Letter";
		public const string RegenerateStatement = "Regenerate Statement";
		public const string CustomerStatementHistory = "Statement History";

		public const string ProcessPrintDL = "Print Dunning Letter";
		public const string ProcessEmailDL = "Email Dunning Letter";

		public const string RegenerateLastStatement = "Regenerate Last Statements";
		public const string DeleteLastStatement = "Delete Last Statements";
		public const string GenerateStatementOnDemand = "Generate on Demand";

		public const string Add = "Add";
		public const string AddItem = "Add Item";
		public const string AddAll = "Add All";
		public const string CopyPrices = "Copy Prices";
		public const string CalcPendingPrices = "Calculate Pending Prices";

		public const string EnterPayment = "Pay/Apply Document";
		public const string Pay = "Pay";
		public const string Apply = "Apply";

		public const string WriteOffPPIBalance = "Write Off Unpaid Balance";

		public const string VoidCardPayment = "Void Card Payment";
		public const string VoidEftPayment = "Void EFT Payment";
		public const string RefundCardPayment = "Refund Card Payment";
		public const string RefundEftPayment = "Refund EFT Payment";
		public const string ValidateCardPayment = "Validate Card Payment";
		public const string ValidateEftPayment = "Validate EFT Payment";
		public const string RecordCardPayment = "Record Card Payment";
		public const string RecordEftPayment = "Record EFT Payment";
		#endregion

		#region Report Names
		public const string CustomerStatement = "Print Statement";
		#endregion

		#region DiscountAppliedTo
		public const string ExtendedPrice = "Ext. Price";
		public const string SalesPrice = "Unit Price";
		#endregion

		#region DiscountAppliedTo
		public const string DocumentLineUOM = "Document Line UOM";
		public const string BaseUOM = "Base UOM";
		#endregion

                #region Discount Type

                public const string Line = "Line";
                public const string Group = "Group";
                public const string Document = "Document";
				public const string ExternalDocument = "External Document";
                public const string Flat = "Flat-Price";
                public const string Unconditional = "Unconditional";

                #endregion

                #region Discount Target
                public const string Customer = "Customer";
		public const string CustomerMaster = "Customer (alias)";
		public const string CustomerSharedCredit = "Customer Shared Credit";
                public const string Discount_Inventory = "Item";
                public const string CustomerPrice = "Customer Price Class";
                public const string InventoryPrice = "Item Price Class";
                public const string CustomerAndInventory = "Customer and Item";
                public const string CustomerPriceAndInventory = "Customer Price Class and Item";
                public const string CustomerAndInventoryPrice = "Customer and Item Price Class";
                public const string CustomerPriceAndInventoryPrice = "Customer Price Class and Item Price Class";

                public const string CustomerAndBranch = "Customer and Branch";
                public const string CustomerPriceAndBranch = "Customer Price Class and Branch";
                public const string Warehouse = "Warehouse";
                public const string WarehouseAndInventory = "Warehouse and Item";
                public const string WarehouseAndCustomer = "Warehouse and Customer";
                public const string WarehouseAndInventoryPrice = "Warehouse and Item Price Class";
                public const string WarehouseAndCustomerPrice = "Warehouse and Customer Price Class";
                public const string Branch = "Branch";
                #endregion

                #region Discount Option
                public const string Percent = "Percent";
                public const string Amount = "Amount";
                public const string FreeItem = "Free Item";
                #endregion

                #region BreakdownType
                public const string Quantity = "Quantity";
                #endregion

		#region Price Option
		public const string PriceClass = "Price Class";
		#endregion

		#region Retention Types
		public const string LastPrice = "Last Price";
		public const string FixedNumberOfMonths = "Fixed Number of Months";
		#endregion

		#region Price Basis
		public const string LastCost = "Last Cost + Markup %";
		public const string StdCost = "Avg./Std. Cost + Markup %";
		public const string CurrentPrice = "Source Price";
		public const string PendingPrice = "Pending Price";
		public const string RecommendedPrice = "MSRP";
		#endregion

		#region Adjustment Type
		public const string Adjusted = "Adjusted";
		public const string Adjusting = "Adjusting";
		#endregion

        #region Credithold Actions
                public const string ApplyCreditHoldMsg = "Credit Hold";
                public const string ReleaseCreditHoldMsg = "Remove Credit Hold";
                #endregion

		#region AR Statement Prepare On Type
		public const string Weekly = "Weekly";
		public const string TwiceAMonth = "Twice a Month";
		public const string FixedDayOfMonth = "Fixed Day of Month";
		public const string EndOfMonth = "End of Month";
		public const string EndOfPeriod = "End of Financial Period";
		public const string Custom = "Custom";
		#endregion

		#region AR Statement Age Based On Type
		public const string DueDate = "Due Date";
		public const string DocDate = "Document Date";
		#endregion

		public const string DueDateRefNbr = "Due Date, Reference Nbr.";
		public const string DocDateRefNbr = "Doc. Date, Reference Nbr.";
		public const string RefNbr = "Reference Nbr.";
		public const string OrderNbr = "Order Nbr.";
		public const string OrderDateOrderNbr = "Order Date, Order Nbr.";
		public const string Revoked = "Revoked";
		public const string Load = "Load";
		public const string Reload = "Reload";
		public const string AutomaticallyApplyAmountPaid = "Automatically Apply Amount Paid";
		public const string CustomerHasXOpenDocumentsToApply = "The {0} customer has {1} open documents to apply. To add documents, click Load Documents or Add Row.";
		public const string CustomerHasXOpenRowsToApply = "The {0} customer has {1} rows to apply. To add documents and document lines, click Load Documents or Add Row.";

		public const string CompleteLine = "Complete";
		public const string IncompleteLine = "Bill Later";
		public const string MayNeedToUseAdvancedView = "Some settings required for creating document on this screen are missing from the Customer. You may need to use Advanced View to create a document for this Customer.";

		public const string WorkGroupID = AP.Messages.WorkgroupID;
		public const string PendingPPD = "VAT Adjustment";

		public const string DocumentsOfInactiveBranchHaveBeenExcludedFromPreparedStatements =  "Documents of the {0} inactive branch or branches have been excluded from the prepared statements.";
		public const string DocumentsOfFollowingCustomersForInactiveBranchesHaveBeenExcludedFromPreparedStatements = "The documents of the following customers for the {1} inactive branches have been excluded from the prepared statements: {0}.";

		public const string CurrentDocumentIsCorrection = "The current document is the correction invoice for the {0} invoice.";
		public const string CurrentDocumentHasBeenCorrected = "The current document has been corrected with the {0} correction invoice.";

		public const string WarningEftAllPayments = "By continuing this operation, you confirm that the customer's bank account details were obtained legally and with the permission of the account holder. All payments that will be made using this Customer Payment Method must be authorized by the account holder. If this is not the case, this operation must be terminated.";
		public const string WarningEftThisPayment = "By continuing this operation, you confirm that the customer's bank account details were obtained legally and with the permission of the account holder. This payment is authorized by the account holder. If this is not the case, this operation must be terminated.";
	}

	public static class NotLocalizableMessages
	{
		public const string ERR_CardProcessingReadersProviderSetting = "Could not set CardProcessingReadersProvider";
		public const string ERR_CardProcessingReadersProviderGetting = "Could not get CardProcessingReadersProvider";
		public const string ERR_CCProcessingNotImplementedICCPayment = "Could not get object which implemented ICCPayment interface.";
	}
}
