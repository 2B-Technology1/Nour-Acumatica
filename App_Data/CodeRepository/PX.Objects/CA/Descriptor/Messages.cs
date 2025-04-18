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
using PX.Common;

namespace PX.Objects.CA
{
	[PXLocalizable(Messages.Prefix)]
	public static class Messages
	{
		// Add your messages here as follows (see line below):
		// public const string YourMessage = "Your message here.";
		#region Validation and Processing Messages
		public const string Prefix = "CA Error";
		public const string CATranNotSaved = "An error occurred while saving CATran for the table '{0}'";
		public const string DocumentOutOfBalance = AP.Messages.DocumentOutOfBalance;
		public const string DocumentStatusInvalid = AP.Messages.Document_Status_Invalid;
		public const string DuplicatedPaymentTypeDetail = "Record already exists.";
		public const string DuplicatedPaymentMethodDetail = "Record already exists.";
		public const string CashAccountExists = "This ID is already used for another Cash Account record.";
		public const string ProcessingCenterInactive = "The processing center is deactivated.";
		public const string CashAccountInactive = "The cash account {0} is deactivated on the Cash Accounts (CA202000) form.";
		public const string CashAccount_MayBeCreatedFromDenominatedAccountOnly = "Only a denominated GL account can be linked to a cash account. For the account, specify a currency of denomination in the Currency column on the Chart of Accounts (GL202500) form.";
		public const string CashAccountNotReconcile = "The {0} cash account does not require reconciliation. Verify if the Requires Reconciliation check box is selected on the Cash Accounts (CA202000) form.";
		public const string ReleasedDocCanNotBeDel = "Released document cannot be deleted.";
		public const string TransferDocCanNotBeDel = "This transaction cannot be deleted. Use Cash Transfer Entry screen.";
		public const string TransferOutCAAreEquals = "The source cash account must be different from the destination cash account.";
		public const string TransferInCAAreEquals = "The destination cash account must be different from the source cash account.";
		public const string TransferCAAreEquals = "Destination Cash Account must be different from the source.";
		public const string GLTranExistForThisCashAcct = "One or more transactions recorded on the selected General Ledger account are not tracked in the Cash Management module. To synchronize the balances of the specified accounts, validate the balance of the cash account on the Validate Account Balances (CA.50.30.00) form.";
		public const string TransferCannotBeReleased = "Transfer {0} cannot be released. Please review the errors.";
		public const string HoldExpenses = "The transfer {0} cannot be released. At least one expense associated with the transfer is on hold or pending approval. First clear the On Hold check box for an expense document and then release the transfer.";
		public const string HoldExpense = "The expense is on hold and cannot be released. Review the expense and clear the On Hold check box.";
		public const string CantEditDisbReceipt = "You cannot change the Type for the Entry Type if one or more transactions was entered already.";
		public const string CantEditModule = "You cannot change the Module for the Entry Type if one or more transactions was entered already.";
		public const string DuplicatedKeyForRow = "Record with this ID already exists.";
		public const string ERR_CashAccountHasTransactions_DeleteForbidden = "This Cash Account cannot be deleted as one or more transaction already exists.";
		public const string ERR_IncorrectFormatOfPTInstanceExpiryDate = "Incorrect date format provided.";
		public const string ERR_RequiredValueNotEnterd = "This field is required.";
		public const string RequiredPaymentInstructionCanNotBeEmpty = "The {0} payment instruction is required and cannot be empty.";
		public const string ValueIsNotValid = "Provided value does not pass validation rules defined for this field.";
		public const string ProcessingCenterIsAlreadyAssignedToTheCard = "This Processing Center is already assigned to the Payment Method";
		public const string ProcCenterDoesNotSupportReauth = "The {0} processing center does not support reauthorization.";
		public const string IncompatiblePluginForCardProcessing = "The plug-in you selected may be incompatible. Please consult with the provider of this plug-in.";
		public const string ProcessExpiredCardWarning = "Expired credit card payment methods will be deactivated, the sensitive information stored within them will be deleted from the system.";
		public const string PaymentMethodConverterWarning = "This operation cannot be reverted. Before converting the payment method, ensure that the {1} processing center is configured to connect to the same external processing center with the same credentials as the {0} processing center. Are you sure you want to convert the customer payment profiles from the {0} processing center to the {1} processing center?";
		public const string UseAcceptPaymentFormWarning = "The check box was selected automatically because this processing center allows accepting payments from new cards. Clear this check box if new cards should be registered on the Customer Payment Methods (AR303010) form only.";
		public const string UseAllowUnlinkedRefundWarning = "The processing center may require you to sign an additional agreement to be able to process unlinked refunds.";
		public const string SelectAccountWithTheSameCurrencyAsCashAccount = "Select an account with the same currency as the cash account.";
		public const string AcceptPaymentFromNewCardDisabledWarning = "The Accept Payments from New Cards check box is cleared for the {0} processing center. Payments with the New Card check box selected cannot be processed with this processing center.";
		public const string UseDiscontinuedDirectInputMode = "The processing center uses the direct input mode, which is no longer supported.";
		public const string DiscontinuedDirectInputModeNotAllowed = "The processing center configuration cannot be saved with the Allow Direct Input check box selected because the direct input mode is no longer supported. Please configure processing centers to use processing plug-ins that support hosted forms.";
		public const string GettingTranDetailsByIdNotSupportedWarn = "Customer payment methods cannot be created for imported transactions with saved cards. The processing center plug-in does not support getting a customer payment profile for a transaction.";
		public const string DefaultProcessingCenterConfirmation = "Make this processing center default?";
		public const string PaymentMethodIsAlreadyAssignedToTheProcessingCenter = "This Payment Method is already assigned to the Processing Center";
		public const string RowIsDuplicated = "Row is duplicated";
		public const string RequiresReconNumbering = "Requires Reconciliation Numbering";
		public const string EntryTypeIDDoesNotExist = "This Entry Type ID does not exist";
		public const string TransactionNotComplete = "Transaction Not Complete";
		public const string TransactionNotFound = "Cash Transaction Not Found";
		public const string OneOrMoreItemsAreNotReleased = "One or more items are not released";
		public const string OneOrMoreItemsAreNotPosted = "One or more items are not posted";
		public const string OneOrMoreItemsAreNotReleasedAndStatementCannotBeCompleted = "One or more items are not released and statement cannot be completed";
		public const string DocNotFound = "Document Not Found";
		public const string APDocumentsCanNotBeReleasedFromCAModule = "AP Documents Can Not Be Released from CA Module";
		public const string ARDocumentsCanNotBeReleasedFromCAModule = "AR Documents Can Not Be Released from CA Module";
		public const string NotAllDocumentsAllowedForClearingAccount = "A document of this type cannot be recorded to this account. The account is selected as a clearing account on the Cash Accounts (CA202000) form.";
		public const string ThisDocTypeNotAvailableForRelease = "The document of this type cannot be released in the Cash Management module. Release the document in the module from which the document has originated.";
		public const string OriginalDocAlreadyReleased = "Original document has already been released";
		public const string CanNotVoidStatement = "There are newer non-voided statements.";
		public const string CanNotCreateStatement = "Can not create statement - current statement is not reconciled.";
		public const string CashAccounNotReconcile = "Cash account does not require reconciliation";
		public const string ReconciledDocCanNotBeNotCleared = "The document has to be cleared before it is reconciled";
		public const string ProcessingCenterIDIsRequiredForImport = "Processing CenterID is required for this operation";
		public const string ProcessingObjectTypeIsNotSpecified = "Type of the object for the Credit Card processing is not specified";
		public const string InstanceOfTheProcessingTypeCanNotBeCreated = "Instance of the Type {0} can't be created";
		public const string PaymentMethodAccountIsInUseAndCantBeDeleted = "This Cash Account is used  in one or more Customer Payment Methods and can not be deleted";
		public const string PaymentMethodIsInUseAndCantBeDeleted = "This Payment Method is used in one or more Customer Payment Methods and can not be deleted";
		public const string CashAccountMayNotBeMadeClearingAccount = "A cash account that has one or more clearing accounts cannot be defined as a clearing account.";
		public const string DontHaveAppoveRights = "You don't have access rights to approve document.";
		public const string DontHaveRejectRights = "You don't have access rights to reject document.";
		public const string CABatchExportProviderIsNotConfigured = "The batch cannot be exported. An export scenario is not specified for the payment method.";
		public const string ReleasedDocumentMayNotBeAddedToCABatch = "This document is released and can not be added to the batch";
		public const string ReleasedDocumentMayNotBeDeletedFromCABatch = "This document is released and can not be deleted from the batch";
		public const string CABatchDefaultExportFilenameTemplate = "{0}-{1}-{2:yyyyMMdd}{3:00000}.txt";  //Do not translate this message, only change it if required
		public const string CABatchStatusIsNotValidForProcessing = "Document status is not valid for processing";
		public const string CABatchContainsUnreleasedPaymentsAndCannotBeReleased = "This  batch contains unreleased payments. It can'not be released until all the payments are released successfully";
		public const string DateSeqNumberIsOutOfRange = "Date Sequence Number is out of range";
		public const string DocumentOnHoldCanNotBeReleased = "Statement on Hold can't be released";
		public const string DocumentIsUnbalancedItCanNotBeReleased = "Statement is not balanced";
		public const string StatementCanNotBeReleasedSomeDetailsMatchedDeletedDocument = "Statement can not be released - same of the details matched deleted document";
		public const string StatementCanNotBeReleasedThereAreUnmatchedDetails = "Statement can not be released - same of the details are not matched";
		public const string PaymentMethodIsRequiredToCreateDocument = "Filling the Payment Method box is mandatory for creating a payment.";
		public const string EntryTypeIsRequiredToCreateCADocument = "Filling the Entry Type box is mandatory for creating a payment.";
		public const string PayeeLocationIsRequiredToCreateDocument = "Filling the Location box is mandatory for creating a payment.";
		public const string PayeeIsRequiredToCreateDocument = "Filling the Business Account box is mandatory for creating a payment.";
		public const string ToCreateChargeSpecifyChargeType = "To create a charge, specify a charge type.";
		public const string ChargeTypeRequiresDefinedProjectAP = "The charge cannot be created automatically from this tab, because the {0} charge type requires a specific project. Do the following:~1. On the Checks and Payments(AP302000) form, create an AP payment for the bill.~2. On the Transactions (CA304000) form, create a cash transaction for the entry type of the charge and specify the project on the Transaction Details tab.~3. On the Match to Payments tab of the current form, select the Match to Multiple Payments check box and match the bill and charge to the bank transaction.";
		public const string ChargeTypeRequiresDefinedProjectAR = "The charge cannot be created automatically from this tab, because the {0} charge type requires a specific project. Do the following:~1. On the Payments and Applications(AR302000) form, create a payment for the invoice.~2. On the Transactions (CA304000) form, create a cash transaction for the entry type of the charge and specify the project on the Transaction Details tab.~3. On the Match to Payments tab of the current form, select the Match to Multiple Payments check box and match the invoice and charge to the bank transaction.";
		public const string DocumentIsAlreadyCreatedForThisDetail = "Document is already created";
		public const string StatementEndDateMustBeGreaterThenStartDate = "End Balance Date should be greater then Start Balance Date";
		public const string StatementEndBalanceDateIsRequired = "End Balance Date is required";
		public const string StatementStartBalanceDateIsRequired = "Start Balance Date is required";
		public const string StatementIsOutOfBalanceThereAreUnmatchedDetails = "Statement is out of balance - there are unmatched details";
		public const string StatementIsOutOfBalanceEndBalanceDoesNotMatchDetailsTotal = "Statement is not balanced - end balance does not match details total";
		public const string StatementDetailIsAlreadyMatched = "This detail is already matched with another CA transaction";
		public const string CashAccountWithExtRefNbrIsNotFoundInTheSystem = "Accounts with the following Ext Ref Numbers could not be found in the system: {0}";
		public const string CashAccountHasCurrencyDifferentFromOneInStatement = "Account {0} has currency {1} different from one specified in the statement. Statement can not be imported. Please, check correctness of the cash account's Ext Ref Nbr and other settings";
		public const string CashInTransitAccountCanNotBeDenominated = "Cash-In-Transit Account can not be Cash Account or denominated one.";
		public const string CashAccountCanNotBeTransit = "Cash-in-Transit account cannot be Cash Account.";
		public const string TransactionWithFitIdHasAlreadyBeenImported = "Transaction with FITID {0} is found in the existing Statement: {1} for the Account: {2}-'{3}'. Most likely, this file has already been imported";
		public const string FITIDisEmpty = "The file does not comply with the standard format: FITID is empty. You will be able to upload the file if you select the Allow Empty FITID check box on the CA Preferences form.";
		public const string OFXImportErrorAccountInfoIsIncorrect = "Account information in the file is invalid or has an unsupported format";
		public const string OFXParsingErrorTransactionValueHasInvalidFormat = "The Value {0} for the Field {1} in the transaction {2} has invalid format: {3}";
		public const string OFXParsingErrorValueHasInvalidFormat = "The Field {0} has invalid format: {1}";
		public const string OFXUnsupportedEncodingDetected = "Unsupported Encoding {0} or Charset (1) detected in the header";
		public const string UnsavedDataInThisScreenWillBeLostConfirmation = "Unsaved data in this screen will be lost. Continue?";
		public const string ImportConfirmationTitle = "Confirmation";
		public const string ViewResultingDocument = "View Resulting Document";
		public const string DuplicatedPaymentMethodForCashAccount = "Payment method '{0}' is already added to this Cash Account";
		public const string PaymentMethodCannotBeUsedInAR = "The {0} payment method cannot be used in AR. Use the Payment Methods (CA204000) form to modify the payment method settings.";
		public const string PaymentMethodCannotBeUsedInAP = "The {0} payment method cannot be used in AP. Use the Payment Methods (CA204000) form to modify the payment method settings.";
		public const string DuplicatedCashAccountForPaymentMethod = "Cash Account '{0}' is already added to this Payment method";
		public const string ApplicationInvoiceIsNotReleased = "Invoice with number {0} is not released. Application can be made only to the released invoices";
		public const string APPaymentApplicationInvoiceIsClosed = "Invoice with number {0} is closed.";
		public const string APPaymentApplicationInvoiceDateIsGreaterThenPaymentDate = "Invoice with the number {0} is found, but it's date is greater then date of the transaction. It can not  be used for the payment application";
		public const string APPaymentApplicationInvoiceUnrealeasedApplicationExist = "There are unreleased applications to the Invoice number {0}. It can not be used for this payment application.";
		public const string APPaymentApplicationInvoiceIsPartOfPrepaymentOrDebitAdjustment = "Invoice with the number {0} is found, but there it's used in prepayment or debit adjustment. It can not be used for this payment application.";
		public const string APPaymentApplicationInvoiceIsNotFound = "Invoice number {0} match neither internal nor external Invoice numbers registered in the system. You need to enter this invoice before the application.";
		public const string ARPaymentApplicationInvoiceIsClosed = "Invoice with number {0} is closed.";
		public const string ARPaymentApplicationInvoiceDateIsGreaterThenPaymentDate = "Invoice with the number {0} is found, but it's date is greater then date of the transaction. It can not  be used for the payment application";
		public const string ARPaymentApplicationInvoiceUnrealeasedApplicationExist = "There are unreleased applications to the Invoice number {0}. It can not be used for this payment application.";
		public const string ARPaymentApplicationInvoiceIsNotFound = "Invoice number {0} match neither internal nor external Invoice numbers registered in the system. You need to enter this invoice before the application.";
		public const string CAFinPeriodRequiredForSheduledTransactionIsNotDefinedInTheSystem = "A scheduled document {0} {1} {2} assigned to the Schedule {3} needs a financial period, but it's not defined in the system";
		public const string FinPeriodsAreNotDefinedForDateRangeProvided = "Financial Periods are not defined for the date range provided. Scheduled documents may not be included correctly";
		public const string CurrencyRateIsNotDefined = "The currency rate is not defined. Specify it on the Currency Rates (CM301000) form.";
		public const string CurrencyRateIsRequiredToConvertFromCuryToBaseCuryForAccount = "A currency rate for conversion from Currency {0} to Base Currency {1} is not found for account {2}";
		public const string CurrencyRateTypeInNotDefinedInCashAccountAndDafaultIsNotConfiguredInCMSetup = "A currency rate type is not defined to the Cash ccount {0} and no default is provided for CA Module in CM Setup";
		public const string RowMatchesCATranWithDifferentExtRefNbr = "This row is matched with the document having not exactly matching Ext. Ref. Nbr.";
		public const string MatchingCATransIsNotValid = "Matching Transaction is not valid. Probably, it has been deleted";
		public const string RowsWithSuspiciousMatchingWereDetected = "There were detected {0} rows, having not exact matching";
		public const string CashAccountDifferentBaseCury = "The base currency of the {0} branch associated with the {1} cash account differs from the base currency of the {2} branch associated with the {3} cash account.";
		public const string ClearingAccountDifferentBaseCury = "The base currency of the {0} branch associated with the {1} clearing account differs from the base currency of the {2} branch.";
		public const string CABatchContainsVoidedPaymentsAndConnotBeReleased = "This Batch Payments contains voided documents. You must remove them to be able to release the Batch";
		public const string CAEntryTypeUsedForPaymentReclassificationMustHaveCashAccount = "Entry Type which is used for Payment Reclassification must have a Cash Account as Offset Account";
		public const string EntryTypeCashAccountIsNotConfiguredToUseWithAnyPaymentMethod = "This Cash Account is not configured for usage with any Payment Method. Please, check the configuration of the Payment Methods before using the Payments Reclassifications";
		public const string OffsetAccountForThisEntryTypeMustBeInSameCurrency = "Offset account must be a Cash Account in the same currency as current Cash Account";
		public const string OffsetAccountMayNotBeTheSameAsCurrentAccount = "Offset account may not be the same as current Cash Account";
		public const string BranchAccountSubCannotBeUsed = "The current branch, offset account, and offset subaccount cannot be used because this account is specified in the Cash Account box.";
		public const string NoActivePaymentMethodIsConfigueredForCashAccountInModule = "There is no active Payment Method which may be used with account '{0}' to create documents for Module '{1}'. Please, check the configuration for the Cash Account '{0}'.";
		public const string EntryTypeRequiresCashAccountButNoOneIsConfigured = "This Entry Type requires to set a Cash Account with currency {0} as an Offset Account. Currently, there is no such a Cash Account defined in the system";
		public const string UploadFileHasUnrecognizedBankStatementFormat = "This file format is not supported for the bank statement import. You must create an import scenario for this file extention prior uploading it.";
		public const string SetOffsetAccountInSameCurrency = "Default offset account currency is different from the currency of the current Cash Account. You must override Reclassification account.";
		public const string SetOffsetAccountDifferFromCurrentAccount = "Default offset account can not be the same as current Cash Account. You must override Reclassification account.";
		public const string SpecifyLastRefNbr = "To use the {0} - Suggest Next Number option you must specify the {0} Last Reference Number.";
		public const string StatementServiceReaderCreationError = "A Statement Reader Service of a type {0} is failed to create";
		public const string CashAccountMustBeSelectedToImportStatement = "You need to select a Cash Account, for which a statement will be imported";
		public const string StatementImportServiceMustBeConfiguredForTheCashAccount = "The statement import service has not been specified for the selected cash account. Update the account settings on the Cash Accounts (CA202000) form.";
		public const string StatementImportServiceMustBeConfiguredInTheCASetup = "You have to configure Statement Import Service. Please, check 'Bank Statement Settings' section in the 'Cash Management Preferences'";
		public const string ImportedStatementIsMadeForAnotherAccount = "The selected file contains information about other accounts: {0}.";
		public const string CashAccountWithExtRefNbrIsNotFoundInTheSystemAndImportedStatementIsMadeForAnotherAccount = "Accounts with the following Ext Ref Numbers could not be found in the system: {0}\n\nThe selected file contains information about other accounts: {1}.";
		public const string CashAccountExist = "Cash account for this account, sub account and branch already exist";
		public const string SomeRemittanceSettingsForCashAccountAreNotValid = "Some Remittance Settings for this Payment Method have invalid values. Please Check.";
		public const string WrongSubIdForCashAccount = "Wrong sub account for this account";
		public const string DocumentMustByAppliedInFullBeforeItMayBeCreated = "To be able to create the payment, you need to apply documents whose total amount must be equal to the payment amount.";
		public const string TranCuryNotMatchAcctCury = "Transaction's Currency does not Match CashAccount's Currency";
		public const string CryptoSettingsChanged = "Encryption settings were changed during last system update. To finalize changes please press save button manually.";
		public const string ShouldContainBQLField = "Parameter should cointain a BqlField";
		public const string CouldNotInsertPMDetail = "Converter was not able to setup Payment Method Detail due to ID conflict. Please contact support for help.";
		public const string CouldNotInsertCATran = "Attempt to rewrite existing CATran was detected. Please contact support for help.";
		public const string NoProcCenterSetAsDefault = "No processing center was set as default";
		public const string CCPaymentProfileIDNotSetUp = "Payment Profile ID in 'Settings for Use in AR' has to be set up before tokenized processing centers can be used";
		public const string ProcCenterIsExternalAuthorizationOnly = "The {0} processing center does not support the Authorize action. The Capture action is supported only for payments that were pre-authorized externally.";
		public const string NoCashAccountForBranchAndSub = "There is no Cash Account matching these Branch and Subaccount";
		public const string GLAccountIsInactive = "The {0} GL account used with this cash account is inactive. To use this GL account, activate the account on the Chart of Accounts (GL202500) form.";
		public const string SubaccountIsInactive = "The {0} subaccount used with this cash account is inactive. To use this cash account, activate the subaccount on the Subaccounts (GL203000) form.";
		public const string CashAccountUsesInactiveSub = "The document cannot be released because the {0} cash account uses the inactive {1} subaccount. To release the document with this cash account, activate the subaccount on the Subaccounts (GL203000) form.";
		public const string CashAccountUsesInactiveGLAccount = "The document cannot be released because the {0} cash account uses the inactive {1} GL account. To release the document with this cash account, activate the GL account on the Chart of Accounts (GL202500) form.";
		public const string ExpenseCashAccountUsesInactiveSub = "The document cannot be released because the {0} cash account of an expense uses the inactive {1} subaccount. To release the document with this cash account, activate the subaccount on the Subaccounts (GL203000) form.";
		public const string ExpenseCashAccountUsesInactiveGLAccount = "The document cannot be released because the {0} cash account of the expense uses the inactive {1} GL account. To release the document with this cash account, activate the GL account on the Chart of Accounts (GL202500) form.";
		public const string OffsetInactiveSub = "The document cannot be released because an expense uses the inactive {0} Offset Subaccount. To release the document with this subaccount, activate the subaccount on the Subaccounts (GL203000) form.";
		public const string OffsetInactiveAccount = "The document cannot be released because an expense uses the inactive {0} GL Account. To release the document with this GL Account, activate the account on the Chart of Accounts (GL202500) form.";
		public const string CashInTransitSubIsInactive = "The document cannot be released because the {0} cash-in-transit subaccount is inactive. Activate the subaccount on the Subaccounts (GL203000) form or change the subaccount in the Cash-in-Transit Subaccount box of the Cash Management Preferences (CA101000) form.";
		public const string CashInTransitGLAccountIsInactive = "The document cannot be released because the {0} cash-in-transit account is inactive. Activate the account on the Chart of Accounts (GL202500) form or change the account in the Cash-in-Transit Account box of the Cash Management Preferences (CA101000) form.";
		public const string SameSetOfBranchAccountSubAsCashAccount = "The current branch, offset account, and offset subaccount cannot be used because this account is specified in the Cash Account box.";
		public const string CashAccountNotMatchBranch = "This Cash Account does not match selected Branch";
		public const string CATranNotFound = "The transaction cannot be processed due to absence of the matched document. Clear the transaction match by clicking the Unmatch button and repeat the matching process.";
		public const string MatchNotFound = "Match for transaction '{0}' was not found";
		public const string DeatsilProcess = "Bank transaction was processed";
		public const string ErrorsInProcessing = "Not all records have been processed, please review";
		public const string AmountDiscrepancy = "The payment detail amount ({0:F2}) differs from the bank transaction amount ({1:F2}). To create the payment, you should add details whose total amount is equal to the bank transaction amount.";
		public const string MatchToInvoiceAmountDiscrepancy = "The total amount of the selected invoices ({0}) is not equal to the bank transaction amount ({1}). Select invoices with the total amount equal to the bank transaction amount.";
		public const string MatchToPaymentAmountDiscrepancy = "The total amount of the selected payments ({0}) is not equal to the bank transaction amount ({1}). Select payments with the total amount equal to the bank transaction amount.";

		public const string NoDocumentSelected = "No document selected. Please select one or more documents to process.";

		public const string BankRuleTooLoose = "Tran. Code, Description, Payee/Payer or Amount Criteria must be specified for the rule.";
		public const string BankRuleEntryTypeDoesntSuitCashAccount = "Entry Type does not suit the selected Cash Account.";
		public const string BankRuleOnlyCADocumentsSupported = "Only Cash Management documents can be created according to rules.";
		public const string BankRuleFailedToApply = "Failed to apply a rule due to data validation.";
		public const string BankRuleInUseCantDelete = "Cannot delete the Rule. There are Transactions associated with this Rule.";
		public const string HideTranMsg = "This will undo all changes to this transaction, hide it from feed and mark it as processed. Proceed?";
		public const string UnmatchTranMsg = "The system will roll back all changes to the selected bank transaction and make it available for processing on the Process Bank Transactions (CA306000) form. The link to the matched document will be deleted, but the document will stay marked as cleared if it is included into a reconciliation statement. If the document was created to match the transaction, it should be handled manually, for instance, voided or matched. Proceed?";
		public const string UnmatchAllTranMsg = "The system will roll back all changes to the matched bank transactions and make them available for processing on the Process Bank Transactions (CA306000) form. The links to the matched documents will be deleted, but the documents will stay marked as cleared if they are included into a reconciliation statement. If the documents were created to match the transactions, they should be handled manually, for instance, voided or matched. Proceed?";
		public const string AnotherOptionChosen = "Another option is already chosen";
		public const string TransactionMatchedToExistingDocument = "Transaction is matched to an existing document.";
	    public const string TransactionMatchedToExistingExpenseReceipt = "The transaction is already matched to an expense receipt.";
        public const string TransactionWillPayInvoice = "A new payment will be created for this transaction based on the invoice details.";
		public const string TransactionWillCreateNewDocument = "New payment will be created for this transaction.";
		public const string TransactionWillCreateNewDocumentBasedOnRuleDefined = "New payment will be created for this transaction basing on the defined rule.";
		public const string TRansactionNotMatched = "Transaction is not matched.";
		public const string WrongInvoiceType = "Wrong Invoice Type!";
		public const string UnknownModule = "Unknown module!";
		public const string CouldNotAddApplication = "Could not add application of '{0}' invoice. Possibly it is already used in another application";
		public const string ApplicationremovedByUser = "Application of '{0}' invoice was removed by user.";
		public const string ErrorInMatchTable = "The {0} bank transaction is matched to multiple documents with different types and cannot be processed. Please contact your Acumatica Support provider for assistance.";
		public const string InvoiceNotFound = "Invoice No. '{0}' has not been found.";
		public const string CannotDeleteTranHeader = "This statement cannot be deleted because it contains transactions that has already been matched or processed.";
		public const string CannotDeleteTran = "This transaction cannot be deleted because it has already been matched or processed.";
		public const string CannotDeleteRecon = "The operation cannot be performed because the reconciliation statement has been already released or voided.";
		public const string PaymentAlreadyAppliedToThisDocument = "The payment is already applied to this document.";
		public const string CanApplyPrepaymentToDrAdjOrPrepayment = "Can't apply Prepayment to Debit Adjustment or Prepayment. Please remove the Debit Adjustments and Prepayments from the list of applications or apply the entire payment amount so that a Check is created.";
		public const string UnableToProcessWithoutDetails = "This document has no details and can not be processed.";
		public const string EndBalanceDoesNotMatch = "Ending balance does not match the calculated balance.";
		public const string BalanceDateDoesNotMatch = "Start date does not match the end date of the previous statement ({0:d}).";
		public const string EarlyInDate = "The date in Receipt Date is earlier than in Transfer Date.";
		public const string EarlyExpenseDate = "The date of the expense is earlier than the transfer date.";
		public const string ReversingTransferOfTransferNbr = "The reversing transfer of the {0} transfer";
		public const string BegBalanceDoesNotMatch = "Beginning balance does not match the ending balance of previous statement ({0:F2}).";
		public const string NotExactAmount = "Please note that the application amount is different from the invoice total.";
        public const string CannotProceedFundTransfer = "A fund transfer with {0} amount cannot be processed. To proceed, enter an amount greater than zero.";
        public const string VoidedDepositCannotBeReleasedDueToInvalidStatusOfIncludedPayment = "The voided deposit cannot be released due to an invalid status of at least one included payment.";
		public const string VoidedDepositCannotBeReleasedDueToIncludedPaymentHasUnreleasedVoidingEntry = "The voided deposit cannot be released because at least one included payment has an unreleased voiding entry. Delete the unreleased voided payments before releasing the voided deposit.";
        public const string ExternalTaxCannotBeCalculated = "Taxes for an external tax provider cannot be calculated on the current form. To calculate taxes correctly, create a cash transaction on the Transactions (CA304000) form.";


        public const string ErrorsProcessingEmptyLines = "Cannot release this document because it has no lines.";

		public const string CantTransferNegativeAmount = "Cannot transfer negative amount.";
		public const string CannotEditTaxAmtWOCASetup = "Tax Amount cannot be edited if \"Validate Tax Totals on Entry\" is not selected on the CA Preferences form";
		public const string CannotEditTaxAmtOnBankTran = "The tax amount cannot be edited, because the system cannot process an inclusive tax with the tax amount different from the calculated tax amount. Use the Transactions (CA304000) form to process the cash entry.";
		public const string ConfirmVoid = "Are you sure you want to void the statement?";
		public const string ExpDateRetrievalFailed = "Failed to retrieve the expiration date from the processing center.";
		public const string CannotDeleteClearingAccount = "The cash account cannot be deleted. This cash account is assigned to another cash account as clearing. Remove links to other cash account and then proceed with the cash account deletion.";
		public const string CannotDeletePaymentMethodAccount = "The combination of Payment Method, Cash Account '{0}, {1}' cannot be deleted because it is already used in payments.";
		public const string CATranHasExcessGLTran = "Cannot release documents. Please contact support.";
		public const string GLAccountIsNotCashAccount = "This cash account is mapped to the {0} GL account for which the Cash Account check box is cleared on the Chart of Accounts (GL202500) form.";
		public const string MultipleCABankTransMatchedToSingleCATran = "Multiple bank transactions have been matched to the same cash transaction in the system. Please contact the support service.";
		public const string DocumentIsAlreadyMatched = "This document has been already matched. Refresh the page to update the data.";
		public const string ValueValidationNotValid = "The ValidationField value is not valid.";
		public const string ValueMaskNotValid = "The MaskField value is not valid.";
		public const string OutOfProcessed = "The system processed {0} out of {1}. {2} are searchable. Error: {3}.";
		public const string ProcessCannotBeCompleted = "The process cannot be completed. Please contact support service.";
		public const string CannotReleasePendingApprovalDocument = "A document with the Pending Approval status cannot be released. The document has to be approved by a responsible person before it can be released.";
		public const string CannotReleasePendingprocessingDocument = "A document with the Pending Processing status cannot be released. The document has to be authorized and captured before it can be released.";
		public const string CustomerNotDefined = "Customer not defined!";
		public const string PaymentMethodAccountCannotBeFound = "The payment method for the cash account cannot be found in the system.";
		public const string CardAccountNumberMustBeSelected = "The Require Card/Account Number check box must be selected if the means of payment is Credit Card and the Integrated Processing check box is selected.";
		public const string IntegratedProcessingMustBeSelected = "The Integrated Processing check box must be selected if the means of payment is Credit Card and the Require Card/Account Number check box is selected.";
		public const string PaymentMethodCannotBeFound = "The {0} payment method cannot be found in the system.";
		public const string PaymentMethodNotDefined = "Payment Method not defined!";
		public const string CorpCardCashAccountToBeLinkedToOnePaymentMethod = "The cash account configured for corporate cards should have a single associated payment method.";
		public const string ClearingAccountNotAllowed = "The cash account configured for corporate cards cannot be set up as a clearing account.";
		public const string PaymentAndAdditionalProcessingSettingsHaveWrongValues = "For a cash account configured for corporate cards, you can select only a payment method with the following settings specified on the Settings for Use in AP tab of the Payment Methods (CA204000) form: the Require Unique Payment Ref. check box is cleared and the Not Required option is selected in the Additional Processing section.";
		public const string PaymentAndAdditionalProcessingSettingsHaveWrongValuesPaymentSide = "The payment method has an associated cash account configured for corporate cards and should have the following settings specified on the Settings for Use in AP tab: the Require Unique Payment Ref. check box is cleared and the Not Required option is selected in the Additional Processing section.";
		public const string CashAccountLinkOrMethodCannotBeDeleted = "You cannot delete the associated cash account because it is configured for corporate cards. If you need to change the payment method for this cash account, please use the Cash Accounts (CA202000) form.";
		public const string Completed = "Completed";
		public const string CorpCardIsInactive = "The corporate card is inactive.";
		public const string RecordWithPaymentCCPIDExists = "The customer payment method cannot be created because a record with the specified payment profile ID already exists.";
		public const string RelodCardDataDialogMsg = "Data will be reloaded. Continue loading?";
		public const string LoadCardCompleted = "Loading of the credit card has been completed.";
		public const string FinancialPeriodClosedInCA = "The {0} financial period of the {1} company is closed in Cash Management.";
		public const string FinPeriodCanNotBeSpecifiedCashAccountIsEmpty = "The financial period cannot be specified because the cash account has not been selected in the Cash Account box.";
		public const string OnlyNonControlAccountCanBeCashAccount = "Only a non-control account can be selected as a cash account.";
		public const string CashAccountCanNotBeUsedAsControl = "Cash accounts cannot be used as control accounts.";
		public const string TranDateIsLaterThanTransactionDate = "The payment date must be equal to or earlier than the bank transaction date.";
		public const string TranDateIsMoreThanDaysBeforeBankTransactionDate = "The payment date is more than {0} days earlier than the bank transaction date.";
		public const string ChargeAlreadyExists = "The charge with the same Entry Type and Payment Method already exists.";
		public const string BaseCurrencyDiffers = "The {0} cash account is associated with the branch whose base currency differs from the base currency of the originating branch.";
		public const string MakeSureProcessingCenterIsActiveAndImportSettlementBatches = "Settlement batches cannot be imported for the {0} processing center. On the Processing Center (CA205000) form, make sure the processing center is active and the Import Settlement Batches check box is selected.";
		public const string SetImportStartDateForTheProcessingCenter = "Settlement batches cannot be imported for the {0} processing center. On the Processing Center (CA205000) form, specify the Import Start Date for the processing center.";
		public const string ImportOfSettlementBatchesInProgress = "Import of Settlement batches for this processing center is already in progress. Please wait for the import process to finish.";
		public const string SpecifyActiveDepositAccountInProcessingCenter = "To create a bank deposit, specify an active deposit account for the {0} processing center on the Processing Centers (CA205000) form.";
		public const string NoPaymentsSetBatchStatusToDeposited = "There are no payments that can be included in a deposit. Set the batch status to Deposited?";
		public const string UnreleasedDocumentsCannotBeIncludedInBankDeposit = "Unreleased documents cannot be included in a bank deposit. To create a bank deposit, release the documents with the Balanced status included in the settlement batch.";
		public const string TheFeeTypeIsNotLinkedToEntryType = "The {0} fee type is not linked to an entry type. Add the {0} fee type on the Fees tab of the Processing Centers (CA205000) form.";
		public const string ConsiderSettingImportStartDate = "Settlement batches starting from {0} will be imported during the next import. If you want to import batches from a later date, specify the Import Start Date.";
		public const string ThisTransactionWillBeExcludedProceed = "This transaction will be excluded from further processing. Proceed?";
		public const string ErrorWhileProcessingTransaction = "An error occurred while processing the {0} transaction.";
		public const string UnreleasedDocsCannotBeDeposited = "Unreleased documents cannot be included in a bank deposit. Release the document to create a bank deposit.";
		public const string SelectOnlyOneTransaction = "Please select only one transaction.";
		public const string ReversingTransactionExists = "One or more reversing transactions already exist. Do you want to proceed with reversing the transaction?";
		public const string CashAccountBaseCurrencyDiffersCurrentBranch = "The base currency of the {0} branch associated with the {1} cash account differs from the base currency of the current branch.";
		public const string ForNegativeBalanceWriteOffAmountCreditWriteOffReasonCodeShouldBeUsed = "For a negative write-off amount, specify the Credit write-off reason code.";
		public const string ForPositiveBalanceWriteOffAmountCreditWriteOffReasonCodeShouldBeUsed = "For a positive write-off amount, specify the Balance write-off reason code.";
		public const string FieldCanNotBeFound = "'{0}' cannot be found in the system.";
		public const string TranDateIsIncorrect = "The {0} value of the {1} table is incorrect because it contains a time component. The format must be yyyy-mm-dd.";
		public const string ChargeCantBeReversedEntryTypeNotConfiguredForCashAccount = "The charge cannot be reversed. The {0} entry type is not configured for the {1} cash account. To proceed, add the entry type for the cash account on the Cash Accounts (CA202000) form.";
		public const string Releasing = "Releasing";
		public const string AccountCannotBeUsedAsOpenDocumentsExists = "The {0} account cannot be used as a cash account because there are open documents associated with the account in the following subledgers: {1}.";
		public const string AmountOfOriginalTransactionCannotBeZero = "Amount of the original transaction cannot be 0. Delete or change the amount of the new transaction.";
		public const string TransactionCannotBeSplitBecauseItHasBeenMatched = "The transaction cannot be split because it has been matched.";
		public const string TransactionCannotBeHiddenBecauseItHasBeenSplit = "The transaction cannot be hidden because it has been split.";
		public const string TransactionCanBeDeletedBecauseItHasBeenSplit = "The transaction cannot be deleted because it has been split. To delete the transaction, delete its child transactions first.";
		public const string TransactionCanBeProcessedBecauseItHasRelatedUnmatchedTransactions = "The transaction cannot be processed because it has related unmatched transactions.";
		public const string OnlyChildTransactionsCanBeDeleted = "Only child transactions can be deleted.";
		public const string ChildTransactionCanBeDeletedbecauseItOrParentHasBeenMatched = "The transaction cannot be deleted because it or its parent transaction has been matched.";
		public const string ChildTransactionNeedsToBeSavedBeforeMatching = "The child transaction must be saved before it can be matched.";
		public const string CashAccountPMSettingsIsEmpty = "The {0} remittance detail is empty. Check the remittance settings of the {1} cash account and the {2} payment method on the Cash Accounts (CA202000) form.";
		public const string SomeCashAccountPMSettingsAreEmpty = "Some remittance details are empty. Check the remittance settings of the {0} cash account and the {1} payment method on the Cash Accounts (CA202000) form.";
		public const string VendorPaymentMethodSettingIsEmpty = "The {0} payment instruction is empty. Check the payment instructions of the {1} vendor and the {2} payment method on the Vendors (AP303000) form.";
		public const string SomeVendorPaymentMethodSettingAreEmpty = "Some payment instructions are empty. Check the payment instructions of the vendors that use the {0} payment method on the Vendors (AP303000) form.";
		public const string SomeVendorPaymentMethodSettingAreInvalidOrEmpty = "Some payment instructions are either invalid or empty. Check the payment instructions of the vendors that use the {0} payment method on the Vendors (AP303000) form.";
		public const string CashAccountIsInMatchingProcess = "The {0} cash account is under the matching process.";
		public const string CashAccountIsInMatchingProcessChangesMayBeLost = "The {0} cash account is under the matching process. Your changes may be lost.";
		public const string CashAccountIsInMatchingProcessChangesWillBeLost = "The {0} cash account is under the matching process. Your changes will be lost.";
		public const string CABatchShouldBeExportedBeforeRelease = "The {0} AP batch has an incorrect status and cannot be processed. It should be exported before release.";
		public const string PaymentIsIncludedInExportedCABatch = "The {0} payment is included in a batch with the Exported status. To remove the payment, set the batch status to Balanced.";
		public const string EnterVoidDateOtherwisePaymentsDatesWillBeUsed = "Enter the void date, otherwise the payment's dates will be used for voiding.";
		public const string VoidDateCannotBeEarlierThanDateOfLastPaymentInBatch = "The void date cannot be earlier than the date of the last payment in the batch ({0}).";
		public const string EntryTypeCannotBeDeletedBecauseItsUsedByBankTransactionRules = "The entry type cannot be deleted because it is used on the Bank Transaction Rules (CA204500) form.";
		public const string APLastReferenceNumberCannotBeIncrementedForCashAccount = "To generate a batch automatically, correct AP Last Reference Number for the {0} cash account on the Payment Methods (CA204000) form. AP Last Reference Number cannot be incremented because the last possible value of the sequence was reached.";
		public const string APLastReferenceNumberMustEndWithNumberForCashAccount = "To generate a batch automatically, correct AP Last Reference Number for the {0} cash account on the Payment Methods (CA204000) form. AP Last Reference Number must end with a number.";
		public const string SpecifyAPLastReferenceNumber = "To generate a batch automatically, specify AP Last Reference Number for the {0} cash account on the Payment Methods (CA204000) form.";
		public const string APLastReferenceNumberCannotBeEmptyIfQuickBatchGenerationSelected = "The AP Last Reference Number cannot be empty if the Quick Batch Generation check box is selected.";
		public const string APLastReferenceNumberMustEndWithNumber = "The value in the AP Last Reference Number box must end with a number.";
		public const string APLastReferenceNumberCannotBeIncremented = "AP/PR Last Reference Number cannot be incremented because the last possible value of the sequence was reached.";
		public const string APSuggestNextNumberShouldBeSelected = "The AP/PR Suggest Next Number check box should be selected.";
		public const string APSuggestNextNumberCannotBeClearedIfQuickBatchGenerationIsSelected = "The AP Suggest Next Number check box cannot be cleared if the Quick Batch Generation check box is selected.";
		public const string APSuggestNextNumberShouldBeSelectedForCashAccount = "To generate a batch automatically, select the Suggest Next Number check box for the {0} cash account on the Payment Methods (CA204000) form.";
		public const string PaymentMethodDetailIsMappedWithPlugInSettingAndMissingOnSettingsForUseInAP = "The {0} payment method detail that is mapped with the {1} plug-in setting is missing on the Settings for Use in AP tab.";
		public const string PaymentMethodDetailIsMappedWithPlugInSettingAndMissingOnRemittanceSettings = "The {0} remittance setting that is mapped with the {1} plug-in setting is missing on the Remittance Settings tab.";
		public const string FormatStringMissingClosingBracket = "The format string is missing a closing bracket. Correct the template.";
		#endregion

		#region Translatable Strings used in the code
		public const string CardNumber = "Card Number";
		public const string ExpirationDate = "Expiration Date";
		public const string NameOnCard = "Name on the Card";
		public const string CCVCode = "Card Verification Code";
		public const string CCPID = "Payment Profile ID";
		public const string ReportID = "Report ID";
		public const string ReportName = "Report Name";
		public const string Day = "Day";
		public const string Week = "Week";
		public const string Month = "Month";
		public const string Period = "Financial Period";
		public const string ViewExpense = "View Expense";
		public const string Release = PM.Messages.Release;
		public const string ReleaseAll = PM.Messages.ReleaseAll;
		public const string Void = "Void";
		public const string AddARPayment = "Add Payment";
		public const string AddARPayments = "Add Payments";
		public const string ViewBatch = "View Batch";
		public const string ViewDetails = "View Details";
		public const string Reports = "Reports";
		public const string CashTransactions = "Cash Transactions";
		public const string Approval = "Approval";
		public const string Approved = "Approved";
		public const string Export = "Export";
		public const string MatchSelected = "Match Selected";
		public const string ImportStatement = "Import";
		public const string ViewDocument = "View Document";
		public const string ViewMatchedDocument = "View Matched Document";
		public const string ClearMatch = "Unmatch";
		public const string ClearMatchAll = "Unmatch All";
		public const string CreateAllDocuments = "Create All";
		public const string CreateDocument = "Create Document";
		public const string UploadFile = "Upload File";
		public const string ProcessTransactions = "Process Transactions";
		public const string MatchSettings = "Match Settings";
		public const string CashForecastTran = "Cash Forecast Transactions";
		public const string CashForecastReport = "Cash Flow Forecast Report";
		public const string ViewAsReport = "View As a Report";
		public const string ViewAsTabReport = "View As a Tab Report";
		public const string DateFormat = "yyyy-MM-dd";
		public const string AccountDescription = "Account Description";
		public const string ClearAllIndexes = "Clear All Indexes";
		public const string ClearAllIndexesTip = "Clears all indexes in the system.";
		public const string IndexCustomArticles = "Index Custom Articles";
		public const string RestartFTS = "Restart Full-Text Search";
		public const string HideTran = "Hide Transaction";
		public const string UnhideTran = "Unhide Transaction";
		public const string ProcessMatched = "Process Matched Lines";
		public const string ClearAllMatches = "Unmatch All";
		public const string AutoMatch = "Auto-Match";
		public const string AutoMatchAll = "Auto-Match All";
		public const string Manual = "Matched Manually";
		public const string CreateRule = "Create Rule";
		public const string ClearRule = "Clear Rule";
		public const string ViewPayment = "View Payment";
		public const string ViewInvoice = "View Invoice";
		public const string MatchModeNone = "None";
		public const string MatchModeEqual = "Equal";
		public const string MatchModeBetween = "Between";
		public const string SearchTitleBatchPayment = "Batch Payment: {0} - {2}";
		public const string SearchTitleCATransfer = "CA Transfer: {0}";
		public const string Matched = "Matched to Payment";
		public const string InvoiceMatched = "Matched to Invoice";
		public const string MatchedToEReceipt = "Matched to Expense Receipt";
		public const string Created = "Created";
		public const string Hidden = "Hidden";
		public const string Offset = "Offset";
		public const string ImportTransactions = "Import Transactions";
		public const string ImportAllTransactions = "Import All Transactions";
		public const string FailedGetFrom = "The system failed to get the From address for the document.";
		public const string FailedGetTo = "The system failed to get the To address for the document.";
		public const string NotVoidedUnreleased = "An unreleased document can't be voided.";
		public const string ExternalTaxVendorNotFound = TX.Messages.ExternalTaxVendorNotFound;
		public const string MultiCurDepositNotSupported = "A deposit of multiple currencies is not supported yet.";
		public const string ProcessingCenterNotSelected = "Processing Center is not selected.";
		public const string ProcessingPluginNotSelected = "Processing plug-in is not selected.";
		public const string ProcessingCenterAimPluginNotAllowed = "The Authorize.Net AIM plug-in cannot be used for new processing center configurations. Use another plug-in.";
		public const string DiscontinuedProcCenter = "The processing center uses the discontinued plug-in. Set up the processing center to use the Authorize.Net API plug-in.";
		public const string NotSupportedProcCenter = "The {0} processing center uses an unsupported plug-in.";
		public const string ProcCenterUsesMissingPlugin = "The {0} processing center references a missing plug-in.";
		public const string CredentialsAccepted = "The credentials were accepted by the processing center.";
		public const string Result = "Result";
		public const string ResetDetailsToDefault = "Changing the Plug-In Type will reset the details to default values. Continue?";
		public const string NotSetProcessingCenter = "The new processing center is not set in the Proc. Center ID box.";
		public const string AskConfirmation = AP.Messages.AskConfirmation;
		public const string PaymentMethodDetailsWillReset = "The details for the payment method will be reset. Continue?";
		public const string EmptyValuesFromExternalTaxProvider = AP.Messages.EmptyValuesFromExternalTaxProvider;
		public const string Validate = "Validate";
		public const string ValidateAll = "Validate All";
		public const string ValueMInstanceIdError = @"The value of the parameter ""pMInstanceID"" cannot be represented as an integer.";
		public const string CanNotRedirectToDocumentThisType = "The system cannot redirect the user to a document of this type.";
		public const string UnknownDetailType = "The {0} detail type is unknown.";
		public const string CABankTransactionsRuleName = "CA Bank Transactions Rule";
		public const string PaymentProfileID = "Payment Profile ID";
		public const string NoMatchingDocFound = "No matching documents were found.";
		public const string NoRelevantDocFound = "Found documents are not relevant and cannot be auto-matched.";
		public const string RatioInRelevanceCalculation = "For expense receipts, the weights of the Ref. Nbr., Doc. Date, and Amount in the relevance calculation are {0:F2}%, {1:F2}%, and {2:F2}%, respectively.";
		public const string IncorrectBAccountStatus = "Business account with the {0} status is not allowed.";
		public const string IncorrectBAccountBaseCuryID = "The {0} cash account is associated with the {1} branch whose base currency differs from the base currency of {2} associated with the {3} account.";
		public const string RecalculateBalanceTooltip = "Recalculate account balances based on cash entries";
		public const string DepositOfSettlementBatch = "Deposit of settlement batch from {0}";
		#endregion

		#region Graphs Names
		public const string CABalValidate = "Cash Account Validation Process";
		public const string CAReconEntry = "Reconciliation Statement Entry";
		public const string CAReconEnq = "Reconciliation Statement Summary Entry";
		public const string CASetup = "Cash Management Preferences";
		public const string CASetupMaint = "Setup Cash Management";
		public const string CashAccountMaint = "Cash Account Maintenance";
		public const string CashTransferEntry = "Cash Transfer Entry";
		public const string CATranEnq = "Cash Transactions Summary Entry";
		public const string CATranEntry = "Cash Transaction Details Entry";
		public const string CATrxRelease = "Cash Transactions Release Process";
		public const string EntryTypeMaint = "Entry Types Maintenance";
		public const string PaymentTypeMaint = "Payment Types Maintenance";
		public const string PaymentMethodMaint = "Payment Methods Maintenance";
		public const string CCProcessingCenter = "Processing Center";
		public const string CCSettlementBatch = "Settlement Batch";
		public const string CAReleaseProcess = "Cash Transactions Release";
		public const string CADepositEntry = "Payment Deposits";
		public const string CABatchEntry = "Payment Batch Entry";
		public const string CashForecastEntry = "Cash Flow Forecast Enquiry";
		public const string CashAccountDetails = "Cash Account Details";
		public const string ReconciliationStatementHistory = "Reconciliation Statement History";
		public const string BankTransactionsHistory = "Bank Transactions History";

		#endregion

		#region DAC Names
		public const string CATran = "CA Transaction";
		public const string CATran2 = "CA Transaction (alias)";
		public const string CABatch = "CA Batch";
		public const string CABatchDetail = "CA Batch Details";
		public const string PaymentMethodDetail = "Payment Method Detail";
		public const string CARecon = "Reconciliation Statement";
		public const string PaymentMethod = "Payment Method";
		public const string CashAccount = "Cash Account";
		public const string PaymentType = "Payment Type";
		public const string CashFlowForecast = "Cash Flow Forecast Record";
		public const string CashFlowForecast2 = "Cash Flow Forecast Record";
		public const string BankTranHeader = "Bank Statement";
		public const string BankTransaction = "Bank Transaction";
		public const string BankTranMatch = "Bank Transaction Match";
		public const string BankTranAdjustment = "Bank Transaction Adjustment";
		public const string BankTranByCashAccount = "Bank Transactions by Cash Account";
		public const string ClearingAccount = "Clearing Account";
		public const string CADailySummary = "CA Daily Summary";
		public const string CAReconByPeriod = "Reconciliation by Period";
		public const string CASplit = "CA Transaction Details";
		public const string CADepositDetail = "CA Deposit Detail";
		public const string CABankTranDetail = "CA Bank Transaction Detail";
		public const string CADepositCharge = "CA Deposit Charge";
		public const string CAEntryType = "CA Entry Type";
		public const string CASetupApproval = "CA Approval Preferences";
		public const string CashAccountCheck = "Cash Account Check";
		public const string CashAccountETDetail = "Entry Type for Cash Account";
		public const string CashAccountPaymentMethodDetail = "Details of Payment Method for Cash Account";
		public const string CATax = "CA Tax Detail";
		public const string CABankTax = "CA Bank Tax Detail";
		public const string CATaxTran = "CA Tax Transaction";
		public const string CABankTaxTran = "CA Bank Tax Transaction";
		public const string CCProcessingCenterDetail = "Credit Card Processing Center Detail";
		public const string CCProcessingCenterPmntMethod = "Payment Method for Credit Card Processing Center";
		public const string CCProcessingCenterFeeType = "Fee Type for Credit Card Processing Center";
		public const string PaymentMethodAccount = "Payment Method for Cash Account";
		public const string PMInstance = "Payment Method Instance";
		public const string CorporateCard = "Corporate Card";
		public const string ExtRefNbr = "Ext. Ref. Nbr.";
		public const string TranDesc = "Tran. Desc";
		public const string UserDesc = "Custom Tran. Desc.";
		public const string InvoiceNbr = "Invoice Nbr.";
		public const string PayeeName = "Payee Name";
		public const string TranCode = "Tran. Code";
		#endregion

		#region Tran Type
		public const string CATransferOut = "Transfer Out";
		public const string CATransferIn = "Transfer In";
		public const string CATransferExp = "Expense Entry";
		public const string CATransfer = "Transfer";
		public const string CAAdjustment = "Cash Entry";
		public const string GLEntry = "GL Entry";
		public const string CADeposit = "CA Deposit";
		public const string CAVoidDeposit = "CA Void Deposit";
		public const string Statement = "Bank Statement Import";
		public const string PaymentImport = "Payments Import";

		public const string ARAPInvoice = "AR Invoice or AP Bill";
		public const string ARAPPrepayment = "Prepayment";
		public const string ARAPRefund = "Refund";
        public const string ARAPVoidRefund = "Voided Refund";
		#endregion

		#region Match Type
		public const string Match = "Match";
		public const string Charge = "Charge";
		#endregion

		#region CA Transfer & CA Deposit Status
		public const string Balanced = "Balanced";
		public const string Hold = "On Hold";
		public const string Released = "Released";
		public const string Pending = "Pending Approval";
		public const string Rejected = "Rejected";
		public const string Voided = "Voided";
		public const string Exported = "Exported";
		public const string Canceled = "Canceled";

		#endregion

		#region Dr Cr Type
		public const string CADebit = "Receipt";
		public const string CACredit = "Disbursement";
		#endregion

		#region CA Modules
		public const string ModuleAP = "AP";
		public const string ModuleAR = "AR";
		public const string ModuleCA = "CA";
		#endregion

		#region CA Reconcilation
		public const string ReconDateNotAvailable = "Reconciliation date must be later than the date of the previous released Reconciliation Statement";
		public const string PrevStatementNotReconciled = "Previous Statement Not Reconciled";
		public const string LastDateToLoadNotAvailable = "The date is earlier than the reconciliation date. Note, that the list of documents will be filtered by the specified date.";
		public const string LoadDocumentsUpToSetLessReconciledDateBySystem = "The date in the Load Documents Up To box is earlier than the Reconciliation Date. You can change the date in the Load Documents Up To box, but note that it may affect the system performance.";
		public const string HoldDocCanNotAddToReconciliation = "Document on hold cannot be added to reconciliation.";
		public const string NotReleasedDocCanNotAddToReconciliation = "Unreleased document cannot be added to Reconciliation.";
		public const string HoldDocCanNotBeRelease = "Document on hold cannot be released";
		public const string ClearedDateNotAvailable = "Clear Date NOT Available;";
		public const string OrigDocCanNotBeFound = "Orig. Document Can Not Be Found";
		public const string ThisCATranOrigDocTypeNotDefined = "This CATran Orig. Document Type Not Defined";
		public const string VoidPendingStatus = "Void Pending";
		public const string VoidedTransactionHavingNotReleasedVoidCannotBeAddedToReconciliation = "This transaction has a voiding transaction which is not released. It may not be added to the reconciliation";
		public const string TransactionsWithVoidPendingStatusMayNotBeAddedToReconciliation = "Transactions having a 'Void Pending' status may not be added to the reconciliation";
		public const string AbsenceCashTransitAccount = "The documents cannot be released due to absence of a cash-in-transit account. Specify the account in the Cash-In-Transit box on the Cash Management Preferences (CA101000) form.";
		public const string ReconDateNotSet = "The number of the next reconciliation statement cannot be generated, because the reconciliation date is not set.";
		#endregion

		#region CA Bank Feed
		public const string DefaultExpenseRequired = "Default expense item is required to create expense receipts.";
		public const string DisconnectConfirmation = "The bank connection and all linked accounts will be deleted from the {0} bank feed. Proceed?";
		public const string IncorrectStartStatementDay = "Incorrect Statement Start Day. Select a value between 1 and 31.";
		public const string BankFeedIntegrationIsDisabled = "The Bank Feed Integration feature is disabled on the Enable/Disable Features (CS100000) form.";
		public const string TransactionsAreBeingRetrieved = "Transactions are being retrieved for the {0} bank feed. Your changes will be lost.";
		public const string BankFeedStatus = "The {0} bank feed has the {1} status.";
		public const string BankFeedWrongImportStatus = "Transactions cannot be imported for the {0} bank feed with the {1} status. The bank feed status should be Active.";
		public const string AccountAlreadyLinked = "The {0} account with the {1} account mask is already linked to the {2} bank feed.";
		public const string CashAccountAlreadyLinked = "The {0} cash account is already linked to the {1} bank account in the {2} bank feed.";
		public const string SpecifyCashAccount = "Specify a cash account to import data from the bank feed.";
		public const string SpecifyCorrectCurrency = "Select a cash account in the {0} currency.";
		public const string IncorrectCardForMatching = "The number of the selected corporate card ({0}) differs from the {2} account mask of the {1} account to which the card is matched.";
		public const string EmplIsNotLinkedToCorpCard = "The {0} employee is not linked to the {1} corporate card. Select another employee on the Corporate Cards tab of the Bank Feeds (CA205500) form for the {2} bank feed and the {1} corporate card.";
		public const string CredentialsWereNotUpdated = "Credentials were not updated. Error reason: {0}.";
		public const string CouldNotParseResponseFromForm = "Could not parse the response from the hosted form. Please contact your Acumatica support provider.";
		public const string BankFeedWithSameCredAlreadyExists = "The bank feed to connect to the {0} financial institution with the same credentials already exists ({1}).";
		public const string UpdateBankFeedCredentials = "The credentials for the bank feed connection have been changed. Do you want to update them?";
		public const string NoBankFeedTransForSelectedDate = "There are no transactions for the selected dates.";
		public const string MaxTranNumberWarning = "Loading a large number of transactions might slow down the system.";
		public const string InstitutionIdDoesNotEqualToStoredValue = "Institution ID from the {0} bank feed service is not equal to the {1} ID.";
		public const string Plaid = "Plaid";
		public const string MX = "MX";
		public const string TestPlaid = "Test Plaid";
		public const string Active = "Active";
		public const string Suspended = "Suspended";
		public const string Disconnected = "Disconnected";
		public const string SetupRequired = "Setup Required";
		public const string MigrationRequired = "Migration Required";
		public const string BankFeedLabel = "Bank Feed";
		public const string StartsWith = "Starts With";
		public const string Contains = "Contains";
		public const string EndsWith = "Ends With";
		public const string AccountOwner = "Account Owner";
		public const string Category = "Category";
		public const string Name = "Name";
		public const string CheckNumber = "Check Number";
		public const string Memo = "Memo";
		public const string PartnerAccountId = "Partner Account ID";
		public const string IncorrectPublicToken = "Cannot get the public token from Plaid.";
		public const string IncorrectAccessToken = "Cannot get the access token from Plaid.";
		public const string Error = "Error";
		public const string Success = "Success";
		public const string LoginFailed = "Login Failed";
		public const string Replace = "Replace";
		public const string FirstDefaultMappingRule = "=ISNULL([Check Number], [Transaction ID])";
		public const string SecondDefaultMappingRule = "=TRIM(ISNULL([Account Owner], ''))";

		public const string MXPreventedStatus = "The last 3 attempts to connect have failed. Please re-enter your credentials to continue importing data.";
		public const string MXDeniedStatus = "The credentials entered do not match your credentials at this institution. Please re-enter your credentials to continue importing data.";
		public const string MXChallengedStatus = "To authenticate your connection to the {0} bank, answer the following challenges.";
		public const string MXRejectedStatus = "The answer or answers provided were incorrect. Please try again.";
		public const string MXLockedStatus = "Your account is locked. Log in to the appropriate website for the {0} bank and follow the steps to resolve the issue.";
		public const string MXImpededStatus = "Your attention is needed at this institution's website. Log in to the appropriate website for the {0} bank and follow the steps to resolve the issue.";
		public const string MXDegradedStatus = "We are upgrading this connection. Please try again later.";
		public const string MXDisconnectedStatus = "It looks like your data from the {0} bank cannot be imported. We are working to resolve the issue.";
		public const string MXDiscontinuedStatus = "Connections to this institution are no longer supported.";
		public const string MXClosedStatus = "This connection has been closed. Please contact your Acumatica support provider.";
		public const string MXDelayedStatus = "Importing your data from the {0} bank may take a while. Please run the import process later.";
		public const string MXFailedStatus = "There was a problem validating your credentials with the {0} bank. Please try again later.";
		public const string MXDisabledStatus = "Importing data from this institution has been disabled.";
		public const string MXImportedStatus = "You must reauthenticate before your data can be imported. Enter your credentials for the {0} bank.";
		public const string MXExpiredStatus = "The answer or answers were not provided in time. Please try again.";
		public const string MXImpairedStatus = "You must reauthenticate before your data can be imported. Enter your credentials for the {0} bank.";
		public const string MXIncorrectStatus = "MX connection has the {0} status.";
		public const string ReplaceRulesHeader = "REPLACE CURRENT RULES";
		public const string ReplaceRulesQuestion = "All the existing mapping rules will be removed and replaced with the default ones.";
		#endregion

		#region OFXFileReader
		public const string ContentIsNotAValidOFXFile = "Provided content is not recognized as a valid OFX format";
		public const string UnknownFormatOfTheOFXHeader = "Unrecognized format of the message header";
		public const string OFXDocumentHasUnclosedTag = "Document has invalid format - tag at position {0} is missing closing bracket (>)";


		#endregion
		#region
		public const string PeriodHasUnreleasedDocs = "Period has Unreleased Documents";
		public const string PeriodHasHoldDocs = "Period has Hold Documents";
		#endregion
		#region Void Date Options for Payments in CABatch
		public const string InitialPaymentDates = "Original Payment Dates";
		public const string SetVoidDate = "Specific Date";
		#endregion

		public const string RefNbr = "Reference Nbr.";
		public const string Type = "Type";

		public const string Sunday = "Sunday";
		public const string Monday = "Monday";
		public const string Tuesday = "Tuesday";
		public const string Wednesday = "Wednesday";
		public const string Thursday = "Thursday";
		public const string Friday = "Friday";
		public const string Saturday = "Saturday";

		#region Action names
		public const string LoadCardData = "Load Card Data";
		public const string LoadCardAccountData = "Load Card/Account Data";
		#endregion
		#region ACH.AccountType
		public const string CheckingAccount = "Checking Account";
		public const string SavingAccount = "Savings Account";
		#endregion
	}
}
