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

namespace PX.Objects.GL
{
	[PXLocalizable(Messages.Prefix)]
	public static class Messages
	{
		// Add your messages here as follows (see line below):
		// public const string YourMessage = "Your message here.";
		//Messages
		#region Validation and Processing Messages

		public const string Prefix = "GL Error";

		public const string ReverseBatchConfirmationMessage = "The reversal of a batch that originated in the {0} module can lead to inconsistency between data in the modules. " +
															  "Are you sure you want to reverse the batch that originated in the {0} module?";
		public const string BatchFromCMCantBeReversed = "Batch can't be reversed, because batches from CM module are prohibited for reversing.";
		public const string BatchOfTranHasBeenReversed = "Batch of transaction has been reversed.";
		public const string TheTransactionCannotBeReclassifiedBecauseItIsNotReleased = "The transaction cannot be reclassified because it is not released.";
		public const string TransHasBeenReclassified = "The transaction has been reclassified.";
		public const string UnreleasedReclassificationBatchExists = "Unreleased reclassification batch exists for this transaction.";
		public const string ReclassificationBatchHasNotBeenCreatedForTheTransaction = "Reclassification batch has not been created for the transaction.";
		public const string AutoReleaseOnReclassificationApprovalSelected = "The Automatically Release Reclassification Batches check box cannot be selected if an active approval map is set up for the Reclassification batch type.";
		public const string ReclassificationBatchGeneratedForThisTransactionHasNotBeenReleasedOrPosted = "Reclassification batch generated for this transaction has not been released or posted.";
		public const string TheDateIsOutsideOfTheFinancialPeriodOfTheTransaction = "The date is outside of the financial period of the transaction. The reclassification date must be within the same financial period.";
		public const string TheDateIsOutsideOfTheFinancialPeriodOfTheBatchTransactions = "The date is outside of the financial period of the batch transactions. The reclassification date must be within the same financial period.";
		public const string InterBranchFeatureIsDisabled = "Inter-Branch Transactions feature is disabled.";
		public const string SourceAccountNotSpecified = "You have to specify one or more source GL Accounts.";
		public const string DestAccountNotSpecified = "You have to specify one or more destination GL Accounts.";
		public const string AccountsNotInBaseCury = "One or more source GL accounts are denominated in foreign currencies. These GL accounts cannot be used for GL allocation.";
		public const string SumOfDestsMustBe100 = "The total percentage for destination GL Accounts must equal 100%.";
		public const string PrevOperationNotCompleteYet = "The previous operation has not been completed.";
		public const string UnpostedBatches = "There are un-posted GL Batches for this GL Allocation for the selected Financial Period. You must post these batches before executing this GL Allocation.";
		public const string NoUnpostedDocumentsForPeriods = "There are no unposted documents for the selected period or periods.";
		public const string AllocCantBeProcessed = "This GL Allocation cannot be processed. There are GL Allocations with higher priority that must be executed first.";
		public const string AmountCantBeDistributed = "GL Allocation cannot be completed. Allocated amount cannot be fully distributed to destination GL Accounts. Please verify the GL Allocation settings.";
		public const string AmountCannotBeDistributed = "GL allocation cannot be completed. The following destination subaccounts cannot be found in the system: ({0}).";
		public const string AmountCannotBeDistributedTrace = "GL allocation cannot be completed. Destination subaccounts cannot be found in the system. Check Trace for details.";
		public const string _0_WithID_1_DoesNotExists = "{0} with ID '{1}' does not exists";
		public const string NoPeriodsForNextYear = "Financial Year cannot be closed. The next Financial Year contains no open Financial Periods.";
		public const string PeriodOpenInAPModule = "Financial Period is open in AP Module.";
		public const string PeriodOpenInARModule = "Financial Period is open in AR Module.";
		public const string PeriodOpenInINModule = "Financial Period is open in IN Module.";
		public const string PeriodOpenInCAModule = "Financial Period is open in CA Module.";
		public const string PeriodOpenInFAModule = "Financial Period is open in FA Module.";
		public const string PeriodClosedInAPModule = "Financial Period '{0}' is closed in AP Module.";
		public const string PeriodClosedInINModule = "Financial Period '{0}' is closed in IN Module.";
		public const string BatchCannotBeReversedBecauseItContainsTransactionsForMoreThanOneDocument = "Batch cannot be reversed because it contains transactions for more than one document.";
		public const string PeriodHasUnpostedBatches = "The {0} financial period cannot be closed because at least one unposted batch exists for this period.";
        public const string PeriodAlreadyUsed = "You cannot delete a Financial Period which has already been used.";
		public const string FinancialPeriodCanNotBeDeleted = "The financial period \"{0}\" cannot be deleted because at least one General Ledger batch exists for this period.";
		public const string FinancialYearCanNotBeDeleted = "The {0} financial year cannot be deleted because at least one General Ledger batch exists for the periods of the year.";
		public const string FinancialPeriodCanNotBeDeletedFA = "The {0} financial period cannot be deleted because at least one fixed asset transaction exists for this period in the posting book.";
		public const string CalendarCanNotBeAdded = "A calendar for the {0} year cannot be added because the calendar template has been changed.";
		public const string DataInconsistent = "Row cannot be inserted. Data is not inconsistent.";
		public const string DeleteSubseqPeriods = "Financial Period cannot be deleted. You must delete all Financial Periods after this Financial Period first.";
		public const string ConfigDataNotEntered = "You must configure the Financial Year Settings first.";
		public const string FiscalYearsHaveUnclosedPeriods = "One or more open Financial Periods exist. Financial Year Settings can only be changed if all the Financial Periods in the system are closed.";
		public const string FiscalPeriodsDefined = "You cannot delete Financial Year Setup because one or more financial periods exist in the system.";
		public const string FABookPeriodsDefined = "You cannot delete Financial Year Setup because one or more fixed asset book periods exist in the system.";
		public const string ModifyPeriodsInUserDefinedMode = "Modification of Financial Periods settings is only allowed when the Custom Number of Periods option is selected.";
		public const string NotAllFiscalPeriodsDefined = "Please configure all the Financial Periods for the Year to commit your changes.";
		public const string NoDateAdjustment = "Date adjustment is not implemented for the period type: {0}.";
		public const string NoOpenPeriod = "There are no open Financial Periods defined in the system.";
		public const string NoOpenPeriodInOrganization = "No open financial periods exist in the {0} company.";
		public const string NoOpenPeriodAfter = "There are no open Financial Periods after {0} defined in the system.";
		public const string NoActivePeriodAfter = "Operation cannot be performed. No active or open periods are available in the system starting from {0}.";
		public const string BatchStatusInvalid = "Batch Status invalid for processing.";
		public const string IncorrectBaseCurrency = "The transaction cannot be released because the currency of the {0} ledger differs from the transaction base currency.";
		public const string ImportStatusInvalid = "Status invalid for processing.";
		public const string AccountMissing = "One or more GL Accounts cannot be found.";
		public const string AccountCanNotBeDenominated = "Cash account or denominated account cannot be specified here.";
		public const string BrachAcctMapMissing = "Account mapping is missing for the {0} and {1} branches. Specify account mapping on the Inter-Branch Account Mapping (GL101010) form.";
		public const string LedgerMissingForBranchInSingleBranch = "The document cannot be released. A posting ledger of the Actual type does not exist in the system. Use the Ledgers (GL201500) form to create a ledger.";
		public const string LedgerMissingForBranchInMultiBranch = "The document cannot be released. No posting ledger of the Actual type is specified for the branch. Use the Ledgers (GL201500) form to assign a posting ledger for the company.";
		public const string LedgerMissing = "One or more Ledgers cannot be found.";
		public const string BranchMissing = "One or more Branches cannot be found.";
		public const string BatchOutOfBalance = "Batch is out of balance, please review.";
		public const string ScheduleTypeCantBeChanged = "Schedule Type cannot be changed for the processed Schedule.";
		public const string NoPeriodsDefined = "Financial Period cannot be found in the system.";
		public const string BudgetTreeNodeNotFound = "Budget Tree node cannot be found in the system.";
		public const string InvalidField = "Invalid field: '{0}'.";
		public const string AccountCuryNotTransactionCury = "Denominated GL Account currency is different from transaction currency.";
		public const string AccountInactive = "Account is inactive.";
		public const string BranchInactive = "Branch is inactive.";
		public const string TheCompanyIsInactive = "The company is inactive.";
		public const string SubaccountInactive = "Subaccount {0} is inactive.";
		public const string YearOrPeriodFormatIncorrect = "Invalid Year or Period format.";
		public const string AccountTypeCannotBeChangedGLYTDOrRE = "The account type cannot be changed because the account is specified as {0} account on the General Ledger Preferences (GL102000) form.";
		public const string AccountGroupChanged = "The project transaction cannot be released because the account group has been changed for an account selected in a project transaction line. To be able to release the transaction, assign the {1} account group to the {0} account.";
		public const string AccountTypeChangeConfirmation = "Changing the type of accounts may affect financial statements because general ledger transactions were posted to these accounts: {0}.";
		public const string AccountTypeChangeWithPMTranConfirmation = "Changing the type of accounts may affect financial statements because general ledger transactions were posted to these accounts: {0}.<br><br>After you save the changes, the system will run the process of recalculating balances for projects that include transactions with these accounts. This operation may take a significant amount of time.";
		public const string AccountTypeChangeOnlyPMTranExistConfirmation = "After you save the changes, the system will run the process of recalculating balances for projects that include transactions with these accounts. This operation may take a significant amount of time.";
		public const string AccountTypeChangeUserRole = "To change the type of an account to which GL transactions were posted, the user must have the Financial Supervisor role assigned.";
		public const string AccountTypeCannotBeChanged = "The type of this account is {0}, and it can be changed to the {1} type only.";
		public const string CannotChangeAccountCurrencyTransactionsExist = "You cannot change denomination currency for the selected GL Account because transactions for the existing currency exist for this GL Account.";
		public const string CannotClearCurrencyInCashAccount = "You cannot delete denomination currency for selected Cash Account.";
		public const string CannotSetCurrencyToMappingAccount = "A currency cannot be assigned to the selected account because this account is specified as an offset one on the Inter-branch Account Mapping (GL101010) form.";
		public const string AccountExistsHistory2 = "You cannot select this GL account, because it is used in transactions. Select another account.";
		public const string ValueForWeight = "Weight value must be in the range of {0}-{1}.";
		public const string EndDateGreaterEqualStartDate = "Financial Period End Date must be greater or equal then the Start Date.";
		public const string EndDateMustNotBeEarlierStartDateOfRegularPeriod = "The end date must not be earlier than the start date.";
		public const string EndDateMustNotBeEarlierStartDateOfAdjustmentPeriod = "The end date must not be earlier than the start date. To create an adjustment period, enter {0} in the End Date column.";
		public const string NoPostNetIncome = "Cannot post transactions directly to Year to Date Net Income.";
		public const string TranDateOutOfRange = "The financial period that corresponds to the {0} date does not exist in the {1} company.";
        public const string FiscalPeriodInactive = "The {0} financial period is inactive.";
		public const string FiscalPeriodClosed = "The {0} financial period is closed.";
		public const string FiscalPeriodClosedInOrganization = "The {0} financial period is closed in the {1} company.";
		public const string FiscalPeriodNotCurrent = "The date is outside the range of the selected financial period.";
		public const string FiscalPeriodNoPeriods = "At least one period should be defined.";
		public const string FiscalPeriodEndDateNotEqualFinYearEndDate = "End Date of the last Financial Period ({0}) is not equal to the end date of the Financial Year ({1}). Financial Year or Financial Periods configuration should be updated. Please select update method.";
		public const string FiscalPeriodEndDateLessThanStartDate = "End Date may not be less than Start Date";
		public const string FiscalPeriodAdjustmentPeriodError = "Adjustment period should be the last period of the financial year.";
		public const string FiscalPeriodMethodModifyNextYear = "Start Date of the next Financial Year will be moved to match end date of the last period of the current year ({0}).";
		public const string FiscalPeriodMethodModifyNextYearSetupBack = "Financial Year settings will be modified. Start date of Financial Year will be moved {0} day(s) back.";
		public const string FiscalPeriodMethodModifyNextYearSetupForward = "Financial Year settings will be modified. Start date of Financial Year will be moved {0} day(s) forward.";
		public const string FiscalPeriodMethodModifyNextYearSetupDate = "Financial Year settings will be modified. The next Financial Year will start on {0}.";
		public const string FiscalPeriodMethodModifyNextYearNoChange = "Financial Year settings will not be modified.";
		public const string FiscalPeriodMethodExtendLastPeriod = "Last period end date will be moved to {0}";
		public const string FiscalPeriodMethodWeekStartWarning = "<html><head></head><body><br><br><b>Warning: Financial Period start day of week will be moved from {0} to {1}</b></body></html>";
		public const string FiscalPeriodMethodEndDateMoveWarning = "<html><head></head><body><br><br><b>Warning: End Date of the last financial period will be moved to {0} to meet Financial Year settings (Financial Period start day of week: {1})</b></body></html>";
		public const string FiscalYearCustomPeriodsMessageTitle = "Modify Financial Periods";
		public const string FiscalYearCustomPeriodsMessage = "Are you sure you want to modify financial periods for this year? This action could affect statistics, budgets and data on reports.";
		public const string FinPeriodsChange = "Financial periods for the selected year exist in the fixed asset posting book and do not match the periods in the general ledger. To amend the periods in the posting book based on the periods in the general ledger, on the Book Calendars (FA304000) form, click Synchronize FA Calendar with GL.";
		public const string CorrespondingFinPeriodsDoseNotExist = "The financial period that corresponds to the {0} date does not exist in the master financial calendar.";
	    public const string InvalidReferenceNumberClicked = "The selected reference number is assigned to a journal entry. Click the link in the Batch Number column to open the batch that includes the entry.";
		public const string LastBatchCanNotBeFound = "Last batch for the selected allocation cannot be found.";
		public const string AccountRevalRateTypefailed = "Revaluation Rate Type can only be entered for a GL Account that is denominated in foreign currency.";
		public const string AllocationSrcEmptyAccMask = "Source Account mask cannot be empty.";
		public const string AllocationDestEmptyAccMask = "Destination Account cannot be empty.";
		public const string AllocationDestEmptySubMask = "Destination Subaccount cannot be empty.";
		public const string ConsolidationBatchOutOfBalance = "Consolidation GL Batch number {0} is out of balance, please review.";
		public const string ConsolidationFailedToAssembleDestinationSub = "Failed to assemble destination Subaccount for Mapped Value {0}. Please check Subaccounts configuration in the source company.";
		public const string ConsolidationHttpError = "HTTP Status Code = {0}";
		public const string DocumentNotReleased = "The document cannot be released. You can try to delete it and create a new one, or you can contact your Acumatica support provider.";
		public const string DocumentsNotReleased = "One or more documents could not be released.";
		public const string DocumentsNotReleasedSeeTrace = "One or more documents could not be released. See trace for details.";
		public const string DocumentsNotPosted = "One or more documents was released but could not be posted.";
		public const string PeriodEndDateIsAfterYearEnd = "The End Date of the financial period must belong to the same financial year as its Start Date.";
		public const string InvalidCashAccountSub = "Specified Subaccount cannot be used with this Cash Account.";
		public const string AllocationBatchDateIsBeforeTheExistingBatchForPeriod = "This GL Allocation was last executed on {0:d}. You cannot run this GL Allocation with an earlier date.";
		public const string AllocationIsNotApplicableForThePeriod = "This GL Allocation is not configured to be executed for the selected Financial Period.";
		public const string AllocationProcessingRequireFinPeriodIDAndDate = "You must provide the Date and Financial Period for processing.";
		public const string SheduleNextExecutionDateExceeded = "The next generation date for this task is greater than the current business date.";
		public const string SheduleExecutionLimitExceeded = "The task reached the configured limit and will not be processed. Please change the task limit or deactivate it.";
		public const string SheduleHasExpired = "The task will not be processed. The expiration date is less than the current business date. Please change the business date or deactivate the task.";
		public const string TranAmountsDenormalized = "Both Debit and Credit parts of transaction are not zero. Exiting with error.";
		public const string DetailsReportIsNotAllowedForYTDNetIncome = "Year to Date Net Income account cannot be selected for inquiry.";
		public const string DuplicateAccountSubEntry = "Duplicate GL Account/Sub Entry";
		public const string AlreadyContainsDataForComparison = "Budget articles cannot be preloaded because the table already contains data for comparison. To proceed, clear the Compare to Year box.";
		public const string AcctSubMayNotBeEmptyForNonGroup = "Account/Subaccount may not be empty for non-node lines.";
		public const string AcctSubMaskNotBeEmptyForNonGroup = "Account/Subaccount Mask may not be empty for non-node lines.";
		public const string AcctMaskMayNotBeEmptyForGroup = "Account Mask may not be empty if Subaccount Mask is entered.";
		public const string SubMaskMayNotBeEmptyForGroup = "Subaccount Mask may not be empty if Account Mask is entered.";
		public const string FutureAllocationApplicationDetected = "GL Allocation {0} was already executed after the selected Financial Period.";
		public const string CantChangeField = "{0} cannot be changed.";
		public const string AllocationStartPeriodHasIncorrectFormat = "Start Period has incorrect format";
		public const string AllocationEndPeriodHasIncorrectFormat = "End Period has incorrect format";
		public const string AllocationEndPeriodIsBeforeStartPeriod = "End Period should be later then Starting Period";
		public const string BudgetArticleIsNotAllocatedProperly = "The Budget Article is not allocated properly.";
		public const string BudgetApproveUnexpectedError = "Unexpected error occurred.";
		public const string BudgetItemsApprovalFailure = "Several items failed to be approved.";
		public const string BudgetArticlesPreloadFromConfigurationTitle = "Preload from Budget Configuration";
		public const string BudgetArticlesPreloadFromConfiguration = "Do you want to create budget for this year? Budget tree will be preloaded from budget configuration.";
		public const string BudgetLineAmountNotEqualAllocated = "Distributed Amount is not equal to Amount. The budget article cannot be released.";
		public const string BudgetTreeOverlappingMask = "Account-Subaccount mask pair overlaps with another Account-Subaccount mask pair: {0} - {1}";
		public const string BudgetTreeIncorrectAccountMask = "Account Mask should not extend beyond the boundaries of the parent node's Account Mask ({0})";
		public const string BudgetTreeIncorrectSubMask = "Subaccount Mask should not extend beyond the boundaries of the parent node's Subaccount Mask ({0})";
		public const string BudgetTreeDeleteGroupTitle = "Delete Group";
		public const string BudgetTreeDeleteGroupMessage = "The child records will be deleted. Are you sure you want to delete the group?";
		public const string BudgetTreePreloadArticlesTitle = "Can not preload articles";
		public const string BudgetTreePreloadArticlesMessage = "No lines can be preloaded using Account mask provided: {0}";
		public const string BudgetTreePreloadArticlesTooManyMessage = "{0} subarticles will be created. Are you sure you want to continue?";
		public const string BudgetTreePreloadArticlesNothingToPreload = "No lines can be preloaded using Account/Subaccount mask provided.";
		public const string BudgetTreeCannotMoveGroupTitle = "Cannot move group";
		public const string BudgetTreeCannotMoveGroupAggregatingArticle = "Group cannot be moved into the aggregating article.";
		public const string BudgetAccountNotAllowed = "Selected account is not allowed in this group (Account mask: {0})";
		public const string BudgetSubaccountNotAllowed = "Selected subaccount is not allowed in this group or does not exist (Subaccount mask: {0})";
		public const string BudgetUpdateTitle = "Convert Budget";
		public const string BudgetUpdateMessage = "Warning: This action is irreversible. Are you sure you want to convert budget?";
		public const string BudgetUpdateConflictMessage = "One or more existing Budget articles are conflicting with the current Budget Configuration. First conflicting line: {0} - {1}. Budget cannot be converted.";
		public const string BudgetRollback = "Roll Back to Released Values";
		public const string BudgetConvert = "Convert Budget Using Current Budget Configuration";
		public const string BudgetRollbackMessage = "All budget articles will be rolled back to the last released values. All changes will be lost.";
		public const string BudgetConvertMessage = "Current budget will be converted using the budget tree from the Budget Configuration form.";
		public const string BudgetManageAction = "Select Action";
		public const string BudgetDifferentCurrency = "The ledger currency is different from the budget currency.";
		public const string BudgetPendingChangesTitle = "Pending Changes";
		public const string BudgetPendingChangesMessage = "The budget has pending changes. Review the budget and then save or discard your changes before selecting another budget.";
		public const string BudgetDeleteTitle = "Delete Budget";
		public const string BudgetDeleteMessage = "The budget cannot be deleted because at least one of its budget articles has been released.";
		public const string BudgetNodeDeleteMessage = "The article cannot be deleted because it has at least one line released.";
		public const string Confirmation = "Confirmation";
		public const string BudgetArticleDescrCompared = "Compared";
		public const string ReleasedBudgetArticleCanNotBeDeleted = "Budget Articles with non-zero Released amount cannot be deleted.";
		public const string PreconfiguredArticlesCanNotBeDeleted = "Preconfigured Articles cannot be deleted.";
		public const string ComparisonLinesCanNotBeDeleted = "Comparison line cannot be deleted.";
		public const string UnknownAllocationPercentLimitType = "Algorithm for the Percent Limit Type {0} is not implemented";
		public const string UnknownAccountTypeDetected = "Unknown account type {0}";
		public const string AllocationSourceAccountSubInterlacingDetected = "Account-Sub combination can not be included into several source lines.";
		public const string AllocationDistributionTargetOverflowDetected = "Allocation cannot be completed - Distribution algorithm produced too large number for Account{0} Sub {1}. Most probable reason - total weight of the Destination Accounts is too small, giving exception 0/0";
		public const string CantFindConsolidationLedger = "Cannot find the source ledger '{0}'.";
		public const string CantFindConsolidationBranch = "Cannot find the source branch '{0}'.";
		public const string NumberRecodsProcessed = "{0} records processed successfully.";
		public const string NoRecordsToProcess = "There are no records to process.";
		public const string ConsolidationBatch = "Consolidation created from '{0}'.";
		public const string AccountOrSubNotFound = "Either Account ID '{0}' or Sub. ID '{1}' specified is invalid.";
		public const string FiscalPeriodInvalid = "Financial Period '{0}' is invalid.";
		public const string CompanyOrBranchIsInactive = "The entity selected in the Company/Branch box is inactive.";
		public const string BranchOfCompanyWithBranchesNotRequiringBalancingTypeCannotBeSelected = "A branch of a company with the With Branches Not Requiring Balancing type cannot be selected.";
		public const string CompanyWithBranchesRequiringBalancingTypeCannotBeSelected = "A company with the With Branches Requiring Balancing type cannot be selected.";
		public const string TheEntitySpecifiedInTheCompanyBranchBoxCannotBeFoundInTheSystem= "The entity specified in the Company/Branch box cannot be found in the system.";
		public const string ImportBranchCDIsEmpty = "The branch cannot be empty.";
		public const string ImportBranchIDIsEmpty = "The branch is not mapped.";
		public const string ImportAccountCDIsEmpty = "The account cannot be empty.";
		public const string ImportAccountIDIsEmpty = "The account is not mapped.";
		public const string ImportSubAccountCDIsEmpty = "The subaccount cannot be empty.";
		public const string ImportSubAccountIDIsEmpty = "The subaccount is not mapped.";
		public const string ImportBranchCDNotFound = "The branch cannot be mapped.";
		public const string ImportAccountCDNotFound = "The account cannot be mapped.";
		public const string ImportYtdBalanceIsEmpty = "Balance is incorrect";
		public const string DocumentIsOutOfBalancePleaseReview = "Document is out of balance, please review.";
		public const string PreviousBatchesNotPosted = "The trial balance cannot be released until there are unposted General Ledger batches.";
		public const string InvalidNetIncome = "Year to Date Net Income account is not configured properly in General Ledger Preferences.";
		public const string InvalidRetEarnings = "Retained Earnings account is not configured properly in General Ledger Preferences.";
		public const string ImportantConfirmation = "Important";
		public const string FirstFinYearDecrementConfirmation = "Warning - This operation will shift the company's first financial year one year earlier. Shifting the first year to an earlier date can affect statistics and data on reports. Do you want to continue?";
		public const string FirstFinYearDecrementConfirmationGeneric = "Warning - This operation will shift the first year one year earlier. Do you want to continue?";
		public const string InsertFinYearBeforeFirstConfirmation = "Warning - The system detected a financial year that is earlier than the first financial year defined for the company. This will create a new first financial year in the database. Do you want to continue?";
		public const string AccountClassIsUsedIn = "Account Class {0} is used in {1}.";
		public const string MultipleCurrencyInfo = "Original batch consolidates multiple documents. The currency information is taken from the first document.";
		public const string PeriodsStartingDateIsTooFarFromYearStartingDate = "Difference between Periods starting date and Year start date must not exceed {0} days";
		public const string ERR_InfiniteLoopDetected = "System error - Infinit Loop detected";
		public const string AllTheFinPeriodsAreAlreadyInserted = "All the periods are already inserted";
		public const string PeriodTemplatesCanotBeGeneratedForThisPeriodType = "Period templates can't be generated for this Period Type";
		public const string MapAccountError = "Account From should be less that Account To.";
		public const string MapAccountDuplicate = "Entered account range overlaps with another one.";
		public const string ERR_AllocationDestinationAccountMustNotBeDuplicated = "Destination accounts may not be duplicated for this allocation type";
		public const string AllocationDestinationAccountAreIdentical = "These Allocation destinations have identical accounts settings. Probably, they should be merged.";
		public const string ERR_DocumentForThisRowIsNotCreatedYet = "A Document for this row is not created yet";
		public const string RowMayNotBeDeletedItReferesExistsingDocument = "This row may not be deleted - a it refers an existing document";
		public const string PaymentMethodWithARIntegratedProcessingMayNotBeEntered = "Payment Methods which have 'Integrated Processing' setting set may not be used in this interface";
		public const string PaymentMethodWithAPPrintCheckOrAPCreateBatchMayNotBeEntered = "Payment Methods which have 'Print Check' or 'Create Batch Payment' setting set may not be used in this interface";
		public const string DocLineHasUnvalidOrUnsupportedType = "Row {0} has invalid on unsupported type {1} {2}";
		public const string CreationOfSomeOfTheIncludedDocumentsFailed = "{0} of {1} documents were not created";
		public const string ReleasingOfSomeOfTheIncludedDocumentsFailed = "{0} of {1} documents were not released";
		public const string PostingOfSomeOfTheIncludedDocumentsFailed = "Documents were successfully created, but {0} of {1} were not posted";
		public const string PostingOfSomeDocumentsFailed = "Documents were successfully released, but {0} of {1} were not posted";
		public const string UnsupportedTypeOfTheDocumentIsDetected = "Unsupported type of the document detected on line {0} - {1} {2} {3}";
		public const string DocumentWasNotCreatedForTheLine = "Document is not created for the line {0} {1}";
		public const string ERR_BatchContainRowsReferencingExistingDocumentAndCanNotBeDeleted = "This batch contains rows referencing created documents. It may not be deleted";
		public const string TransactionCodeMayNotBeChangedForTheLineWithAssinedReferencedNumber = "The reference number has been assigned - it's not possible to change Transaction Code.";
		public const string ReenterRequired = "Reference number generated for this document is invalid. Try to re-enter the line. If this error appears again, please contact support team.";
		public const string TranMasterPeriodDoesNotMatchBatchMasterPeriod = "The period in one of the lines ({0}) differs from the post period specified for the batch ({1}) and the batch cannot be processed further. If this error appears again, please contact the support team.";
		public const string ProjectIsRequired = "Project is Required but was not specified. Account '{0}' used in the GL Transaction is mapped to Project Account Group.";
		public const string DocumentIsOutOfBalance = "This document is not balanced. The difference is {0}.";
		public const string TaxDetailCanNotBeInsertedManully = "Tax detail can not be inserted manually";
		public const string GLHistoryValidationRunning = "The batch has been released but not posted because GL history validation is in progress. It will be posted automatically after validation is completed.";
		public const string DateMustBeSetWithingPeriodDatesRange = "To have an effect Date must be set between Period Start Date and Period End Date.";
		public const string TheDateIsOutsideOfTheSpecifiedPeriod = "The date is outside of the specified period.";
		public const string ActualLedgerInBaseCurrency = "Actual ledger '{0}' must be defined in base currency {1} only.";
		public const string DocumentTypeIsNotSupportedYet = "This Tran. type is not supported yet. It may not be set 'Active'";
		public const string DocumentMustHaveBalanceToMayBeSplitted = "You must enter a document's Total Amount before you may add splits";
		public const string DocumentMustByAppliedInFullBeforeItMayBeReleased = "You have to apply full amount of this document before it may be released";
		public const string AllocationSourceAccountsContainInactiveContraAccount = "One or more of allocation source contains inactive contra accounts";
		public const string AllocationDestinationAccountsContainInactiveAccount = "One or more of destination accounts is inactive";
		public const string AllocationDestinationAccountsContainInactiveBasisAccount = "All basis accounts are inactive for one or more destination accounts";
		public const string CannotFindAccount = "Cannot find account {0} in the source company. Verify account mapping on the Chart of Accounts (GL202500) form.";
		public const string CannotFindSubAccount = "Cannot find the {0} subaccount in the source company. Verify subaccount mapping on the Subaccounts (GL203000) form.";

		public const string BranchHasNotBeenAssociatedWithLedger = "The {0} branch or branches have not been associated with the {1} ledger on the Ledgers (GL201500) form.";
		public const string NoActualLedgerHasBeenAssociatedWithBranches = "No actual ledger has been associated with the following branches: {0}. Use the Ledgers (GL201500) form to associate ledgers with branches.";

		public const string DocumentIsNotAppliedInFull = "This document is not fully applied";

		public const string DocumentAlreadyExists = "Document with these Reference Number and Document type already exists";
		public const string DayOfWeekNotSelected = "You must select at least one day of week";
		public const string CashAccountDoesNotExist = "Cash account doesn't exist for this branch, account and sub account ({0}, {1}, {2})";
		public const string CashAccountDoesNotExistForBranchAndAccount = "The cash account does not exist for the following branch and account: {0}, {1}.";
		public const string BatchNbrNotValid = "Batch Number is not valid!";
		public const string YTDNetIncomeMayBeLiability = "The selected account must have the Liability type.";
		public const string YTDNetIncomeSelected = "YTD Net Income Account cannot be used in this context.";

		public const string UnrecognizedTaxFoundUsedInDocument = "Tax {0} used in Document with Reference Nbr. {1} is not found in the system";
		public const string DocumentWithTaxIsNotBalanced = "Document with Reference Nbr. {0} is not balanced. Tax information may not be created for it.";
		public const string DocumentWithTaxContainsBothSalesAndPurchaseTransactions = "Document with Reference Nbr. {0} contains both sales and purhase Tax transactions. Tax information may not be created for it.";
		public const string DocumentContainsSalesTaxTransactionsButNoIncomeAccounts = "Document with Reference Nbr. {0} contains sales tax transactions, but there are no transactions to income or asset accounts which may be considered as taxable. Tax information may not be created for it.";
		public const string DocumentContainsPurchaseTaxTransactionsButNoExpenseAccounts = "Document with Reference Nbr. {0} contains purchase tax transactions, but there are no transactions to expense or asset accounts which may be considered as taxable. Tax information may not be created for it.";
		public const string SeveralTaxRevisionFoundForTheTax = "There are several records for the Tax Rate for tax {0} and date {1}. Tax inforamtion may not be created";
		public const string TaxableAndTaxAmountsHaveDifferentSignsForDocument = "Taxable and Tax Amount have different signs for tax {0} in the document {1}";
		public const string TaxAmountIsNegativeForTheDocument = "Tax Amount is negative for tax {0} in the document {1}";
		public const string TaxAmountEnteredDoesNotMatchToAmountCalculatedFromTaxableForTheDocument = "Tax Amount {0} does not match the amount {1} calculated from taxable amount {2} and tax Rate {3}% for Tax {4} in the document {5}";
		public const string DeductedTaxAmountEnteredDoesNotMatchToAmountCalculatedFromTaxableForTheDocument = "Tax Amount {0} does not match the amount {1} calculated from taxable amount {2} and tax Rate {3}% and Non-deductible tax rate {4}% for Tax {5} in the document {6}";
		public const string TypeForTheTaxTransactionIsNoRegonized = "Type of tax transaction is not recognized from Tax Type{0} and sign {1}";
		public const string NoTaxableLinesForTaxID = "Document Reference Nbr. {0} doesn't contain any taxable lines that can be applied to {1} Tax";
		public const string TaxIDMissingForAccountAssociatedWithTaxes = "Account is associated with one or more taxes, but Tax ID is not specified";
		public const string RevaluationRateTypeIsNotDefined = "Revaluation Rate Type is not Defined";
        public const string UnpostedBatchesExist = "There are unposted batches in the consolidating branch/ledger generated by the previous consolidation. To proceed, release and post the batches.";
        public const string CurrencyForLedgerCannotBeFound = "Currency for ledger with ID '{0}' cannot be found in the system";
		public const string DocumentMayNotIncludeWithholdindAndSalesOrPurchanseTaxes = "Tax information cannot be created for document with Reference Nbr. {0} because it contains both withholding tax transaction and sales (or purchase) tax transaction.";
		public const string ConsolidationSegmentValueMayNotBeEmpty = "Consolidation Segment Value may not be empty if Paste Segment Value is selected.";
		public const string DocumentContainsWithHoldingTaxTransactionsButNoExpenseAccountsOrLiabilityAccount = "Document with Reference Nbr. {0} contains withholding tax transactions but tax information cannot be created for it because no expense or liability accounts, which may be considered as taxable, are specified in the transactions.";
		public const string DocumentContainsIncomeOrAssetAccountdAsTaxableForWithHoldingTax = "Document with Reference Nbr. {0} contains withholding tax transactions, in which the income or asset accounts are specified as taxable. Tax information cannot be created for this document because only expense or liablility accounts can be specified as taxable for withholding tax.";
		public const string CantShowBalancesInMultipleYears = "The beginning and ending balances cannot be displayed for the selected account because the specified period spans multiple years.";

		public const string DocumentIsInvalidForVoucher = "Document  is invalid or was not created for the Voucher {0} in Voucher Batch {1}";
		public const string ModuleIsNotSupported = "Module {0} specified in the voucher is not supported by the system";
		public const string ModuleDocTypeIsNotSupported = "Module {0} adn Type {1} specified in the voucher is not supported by the system";
		public const string GLVoucherIsLinkedIncorrectly = "The document linked to the voucher {0} may not edited using the screen specified in Workbook {1}";
		public const string GLVoucherEditGraphMayNotBeAssignedToTheBasedDocumentGraph ="The Business Object specified in the Voucher Edit screen {0} is not matching to the Document base Business Object{1}";
		public const string GLWorkBookIsInvalidOrVoucherEditScreenIsNotConfiguredForIt = "Workbook {0} is invalid or Voucher edit screen is not configured for it";
		public const string GLWorkBookHaveUnreleasedBatches = "The workbook cannot be deactivated because it contains at least one unreleased voucher batch. To deactivate a workbook, release or delete all unreleased voucher batches.";
		public const string NoBatchesIsSelectedForRelease = "Please, select one or more batches for release.";
		public const string NoBatchesForDelete = "There are no batches ready for delete.";
		public const string BatchDeleteConfirmation = "Voucher batch {0} and all included documents will be deleted. Do you want to continue?";
		public const string BatchDeleteReleased = "Released voucher batch cannot be deleted.";
		public const string ReleasedBatchCannotBeDeleted = "A released batch cannot be deleted.";
		public const string BatchDeleteWithDocuments = "The voucher batch cannot be deleted because it contains documents.";
		public const string BatchReleaseFiled = "Some voucher batches are failed to release.";
		public const string OnlyOneUnreleasedBatchAllowed = "New voucher batch cannot be created because only one unreleased voucher batch is allowed for this workbook. Release the previous voucher batch first to create a new one.";
		public const string ViewEditVoucherBatch = "View & Edit";
		public const string ManualNumberingDisabled = "It is not allowed to set the manual numbering of sequences for vouchers.";
		public const string NumberingSequenceOtherWorkBook = "This numbering sequence is already used in the workbook {0}.";
		public const string NumberingSequencesIntersection = "We do not recommend that you use selected numbering sequence because it can intersect with the numbering {0} assigned to the same document type.";
        public const string SameNumberingForVoucherAndBatch = "We do not recommend that you use the same numbering sequence for vouchers and voucher batches.";
		public const string NumberingSequencePreferences = "This numbering sequence is already used in {1} in the {0}.";
		public const string CannotFindSitemapNode = "Voucher Entry Form is not found. Possibly the form is not configured correctly for this workbook or you do not have enough rights to access the form.";
		public const string ViewEditVoucherBatchToolTip = "View/edit voucher batch documents (Ctrl+E)";
		public const string ViewVoucherBatch = "View Voucher Batch";
		public const string ViewWorkBook = "View Workbook";
		public const string ConsolidationAccountCannotBeSpecified = "A consolidation account cannot be specified for the YTD Net Income account because the consolidation of the YTD Net Income account data is prohibited.";
		public const string ImportingYTDNetIncomeAccountDataIsProhibited = "Importing the data of the YTD Net Income account is prohibited. Make sure that this account is not specified as a consolidation account on the Chart of Accounts (GL202500) form in the consolidation unit.";
		public const string AccountCannotBeSpecifiedAsTheYTDNetIncome = "The account cannot be specified as the YTD Net Income account because a consolidation account is specified for this account on the Chart of Accounts (GL202500) form.";
		public const string CannotReleaseScheduled = "It is not allowed to release scheduled documents.";
		public const string WBViewEditVoucher = "View and Edit Voucher Batch";
		public const string VoucherBatchNbr = "Voucher Batch Nbr.";
		public const string NoFinancialPeriodForDate = "The financial period for the {0} date is not defined in the system. To proceed, generate the necessary periods on the Financial Periods (GL201000) form.";
		public const string NoFinancialPeriodWithId = "The {0} financial period is not defined in the system. To proceed, generate the necessary periods on the Financial Periods (GL201000) form.";
		public const string BeforeLowercase = "before";
		public const string AfterLowercase = "after";
		public const string NoFinancialPeriodForOffset = "The financial period that is {0} periods {1} {2} is not defined in the system. To proceed, generate the necessary periods on the Financial Periods (GL201000) form.";
		public const string NoFinancialPeriodBefore = "The financial period preceding {0} is not defined in the system. To proceed, generate the necessary periods on the Financial Periods (GL201000) form.";
		public const string NoFinancialPeriodAfter = "The financial period after {0} is not defined in the system. To proceed, generate the necessary periods on the Financial Periods (GL201000) form.";
		public const string NoFinancialPeriodForNextExecutionDate = "There is no financial period to determine the next execution date. To proceed, add or activate the necessary number of financial periods on the Financial Periods (GL201000) form.";
		public const string ReversingWorkbookIDisnotDefined = "The Reversing Workbook ID cannot be found. Define a reversing workbook on the Workbooks (GL107500) form.";
		public const string ReversingWorkbookIDisnotFound = "The Reversing Workbook ID cannot be found. The reversing workbook might be disabled or hidden.";
		public const string ReversingVoucherBatchDesc = "Reversed vouchers";
		public const string VoucherEdit = "Voucher Edit";
		public const string WorkbookIsUsedAsReversingWorkbook = "The Active status cannot be changed because the workbook is used as the reversing workbook.";
		public const string DeletingWorkbookIsUsedAsReversingWorkbook = "The workbook cannot be deleted because it is used as the reversing workbook.";
        public const string ReplaceActionWillAffectSplitTransacionsThatMatchTheSelectedCriteria = "The Replace action will affect any split transactions that match the selected criteria.";
		public const string ProcessingRequireFinPeriodID = "You must fill in the Fin. Period box to perform validation.";
        public const string CannotIncreaseAmtOfSplittedGreatestThanRestOfOriginalTran = "The total amount of split lines cannot be more than the original amount.";
        public const string NotAllMembersOfSplittingGroupWereSelected = "Because not all lines of the splitting group have been selected, processing is impossible.";
        public const string CurrentRecordAndItsChildRecordsWillBeDeleted = "The current record and its child records will be deleted.";
        public const string CurrentRecordWillBeDeleted = "The current record will be deleted.";
        public const string SplitRecordHasNotBeenAdded = "The splitting record has not been added because the unique key was duplicated. Please contact support service for assistance.";
        public const string ReversingEntryHasNotBeenFound = "The reversing entry has not been found.";
        public const string ReclassifiedRecordCannotBeFoundOrNoReclassifableAmount = "The original transaction has been already reclassified, or the reclassifying amount is greater than the original amount. See the original batch: {0}";
		public const string CustomerVendor = "A vendor, customer, or employee business account can be specified.";

		public const string TheRelationBetweenTheLedgerAndTheCompanyCannotBeRemovedBecauseAtLeastOneGeneralLedgerTransactionHasBeenPosted = "The relation between the {0} ledger and the {1} company cannot be removed because at least one General Ledger transaction has been posted to the related branch and ledger.";
		public const string TheLedgerCannotBeDeletedBecauseAtLeastOneGeneralLedgerTransactionHasBeenReleased = "The {0} ledger cannot be deleted because at least one General Ledger transaction has been released for this ledger.";
		public const string TheLedgerCannotBeDeletedBecauseAtLeastOneGeneralLedgerBatchExists = "The {0} ledger cannot be deleted because at least one General Ledger batch exists for the ledger.";
		public const string TheTypeOfTheLedgerCannotBeChangedBecauseAtLeastOneReleasedGLTransactionExists = "The type of the {0} ledger cannot be changed because at least one released GL transaction exists for the ledger.";
		public const string AtLeastOneGeneralLedgerTransactionHasBeenPosted = "At least one General Ledger transaction has been posted to the {0} ledger. Are you sure you want to detach the ledger from the {1} company?";
		public const string TheDefaultBranchNameCannotBeAssigned = "The default branch name cannot be assigned.";
		public const string TheCompanyTypeCannotBeChangedToBecauseMoreThanOneBranchExistsForTheCompany = "The company type cannot be changed to {0} because more than one branch exists for the company.";
		public const string TheCompanyTypeCannotBeChangedToBecauseDataHasBeenPostedForTheCompany = "The company type cannot be changed to {0} because more than one branch exists for the company and data has been posted for the company.";
		public const string TheCompanyTypeCannotBeChangedToBecauseTheCompanyIsExtendedToCustomerOrVendor = "The company type cannot be changed to {0} because the company has been extended to customer or vendor.";
		public const string TheOfTheBranchCannotBeChangedBecauseAtLeastOneReleasedGLTranExistsForTheBranch = "The {0} of the {1} branch cannot be changed because at least one released General Ledger transaction exists for the branch.";
		public const string OnlyACompanyWithBranchesCanBeSpecified = "Only a company with branches can be specified.";

		public const string BranchCannotBeSetAsInactiveBecauseRelatedWarehousesExist = "The {0} branch of the company cannot be set as inactive because the following related warehouses exist: {1}.";
		public const string BranchCannotBeSetAsInactiveBecauseRelatedFixedAssetsExist = "The {0} branch of the company cannot be set as inactive because the following related fixed assets exist: {1}.";
		public const string BranchCannotBeSetAsInactiveBecauseRelatedBillingRulesExist = "The {0} branch cannot be deactivated because it is used in the following billing rules: {1}.";
		public const string BranchCannotDeletedBecauseRelatedWarehousesExist = "The {0} branch cannot be deleted because the following related warehouses exist: {1}.";
		public const string BranchCannotDeletedBecauseRelatedFixedAssetsExist = "The {0} branch cannot be deleted because the following related fixed assets exist: {1}.";
		public const string BranchCannotDeletedBecauseRelatedBillingRulesExist = "The {0} branch cannot be deleted because it is used in the following billing rules: {1}.";
		public const string CompanyCannotBeSetAsInactiveBecauseRelatedWarehousesExist = "The {0} company cannot be set as inactive because the following related warehouses exist: {1}.";
		public const string CompanyCannotBeSetAsInactiveBecauseRelatedFixedAssetsExist = "The {0} company cannot be set as inactive because the following related fixed assets exist: {1}.";
		public const string CompanyCannotDeletedBecauseRelatedWarehousesExist = "The company cannot be deleted because its branch has been specified as an in-transit branch. To delete the company, on the Inventory Preferences (IN101000) form, specify a branch of another company in the In-Transit Branch box.";
		public const string CompanyCannotDeletedBecauseRelatedFixedAssetsExist = "The {0} company cannot be deleted because the following related fixed assets exist: {1}.";

		public const string TheSpecifiedBranchDoesNotBelongToTheSelectedCompany = "The specified branch does not belong to the selected company. Specify the branch that is associated with the company on the Branches (CS102000) form.";
		public const string TheSelectedLedgerDoesNotBelongToTheSelectedCompanyOrBranch = "The selected ledger does not belong to the selected company or branch.";
		public const string CompanyCannotBeDeletedBecauseBranchOrBranchesExistForThisCompany = "The {0} company cannot be deleted because the following branch or branches exist for this company: {1}.";
		public const string BranchCannotBeActivatedBecauseItsParentCompanyIsInactive = "The branch cannot be activated because its parent company is inactive.";
		public const string LedgerCannotBeAssociatedWithTheCompanyBecauseTheActualLedgerHasBeenAlreadyAssociated = "The {0} ledger cannot be associated with the {1} company because the {2} actual ledger has been already associated with this company.";
		public const string TransactionCannotBePostedBecauseTheBranchAndLedgerAreNotAssociatedWithOneAnother = "The {0} transaction cannot be posted because the {1} branch and {2} ledger are not associated with one another on the Ledgers (GL201500) form.";
		public const string CompanyCannotBeDeletedBecauseLedgerOrLedgersAreAssociated = "The {0} company cannot be deleted because the following ledger or ledgers are associated with this company: {1}.";
		public const string LedgerCannotBeDeletedBecauseCompanyOrCompaniesAreAssociated = "The {0} ledger cannot be deleted because the following company or companies are associated with the ledger: {1}.";
		public const string LedgerAlreadyExists = "The {0} ledger already exists.";
		public const string BranchCannotBeConsolidated = "The {0} branch cannot be consolidated separately because its company has type \"With Branches Not Requiring Balancing\". Consolidate data for the whole ledger.";
		public const string SubaccountSegmentValuesWereDeactivatedWontLoad = "Because some segment values of subaccounts have been deactivated or deleted on the Segment Values (CS203000) form, the subaccounts with these values will not be loaded.";

		public const string InterBranchTransAreNotAllowed = "The application cannot be released, because the documents are related to different branches and the Inter-Branch Transactions feature is disabled.";
		public const string NotValidAccount = "Specify a correct {0} account. It must not be a cash account.";
		public const string TheLedgerIsNotAssociatedWithBranch = "The {0} ledger is not associated with the branch.";
		public const string FinPeriodForBranchOrCompanyDoesNotExist = "The financial period does not exist for the related branch or company.";
	    public const string FinPeriodDoesNotExistForCompany = "The {0} financial period does not exist for the {1} company.";
	    public const string RelatedFinPeriodForMasterDoesNotExistForCompany = "The related financial period for the {0} master period does not exist for the {1} company.";
	    public const string RelatedFinPeriodsForMasterDoesNotExistForCompanies = "The related financial periods for the {0} master period do not exist in the {1} companies.";
	    public const string RelatedFinPeriodsForMasterDoesNotExistForCompany = "The related financial periods for the {0} master period do not exist in the {1} company.";
		public const string MasterFinPeriodDoesNotExist = "The {0} master financial period does not exist on the Master Financial Calendar (GL201000) form.";
		public const string FinPeriodCanNotBeSpecified = "The financial period cannot be specified because the branch has not been specified in the Branch box.";
        public const string FinPeriodIsInactiveInCompany = "The {0} financial period is inactive in the {1} company.";
		public const string FinPeriodIsClosedInCompany = "The {0} financial period is closed in the {1} company.";
	    public const string FinPeriodIsLockedInCompany = "The {0} financial period is locked in the {1} company.";
		public const string FinPeriodLocked = "The {0} financial period is locked in at least one company.";
		public const string FinPeriodCorrespondingToPeriodInCompanyCalendarDoesNotExistForCompanies = "The financial period corresponding to the {0} period in the calendar of the {1} company does not exist for the following companies: {2}.";
        public const string FinPeriodDoesNotExistForCompanies = "The {0} financial period does not exist for the following companies: {1}.";
        public const string InconsistentFinPeriodID = "The {0} financial period ID does not match the {1} financial year and {2} period number.";
        public const string InconsistentCentralizedOrganizationYear = "The {0} financial year is inconsistent in the {1} company.";
        public const string CannotInsertOrganizationYear = "The {0} financial year cannot be created for the {1} company.";
        public const string InconsistentCentralizedOrganizationPeriod = "The {0} financial period is inconsistent in the {1} company.";
        public const string CannotInsertOrganizationPeriod = "The {0} financial period cannot be created for the {1} company.";
        public const string MasterYearNotFound = "The {0} master financial year cannot be found.";
        public const string MasterCalendarDoesNotExist = "The master calendar does not exist.";
        public const string OnlyLastYearCanBeDeleted = "Only last financial year can be deleted.";
		public const string ConfirmMasterFinYearDeletion = "The selected financial year will be deleted.";
		public const string FinPeriodCanNotBeClosedInSubledgers = "The {0} financial period cannot be closed in {1}.";
        public const string FinPeriodCanNotBeLockedInSubledgers = "The {0} financial period cannot be locked in {1}.";
		public const string DiscrepancyPeriod = "A discrepancy exists between the master calendar and the calendar of the {0} company in the {1} period on the Master Financial Calendar (GL201000) and Company Financial Calendar (GL201100) forms, respectively.";
		public const string DiscrepancyPeriodError = "A discrepancy exists between the master calendar and the calendar of the {0} company on the Master Financial Calendar (GL201000) and Company Financial Calendar (GL201100) forms, respectively. Please see Trace.";
		public const string DiscrepancyField = "A discrepancy exists between the calendars of the {0} and {1} companies in the {3} period in the following functional areas: {2}.";
		public const string DiscrepancyFieldError = "A discrepancy exists between the calendars of companies on the Company Financial Calendar (GL201100) form. Please see Trace.";
		public const string FromYearGreaterThanLastExistingYear = "The periods cannot be generated because the value in the From Year box is greater than {0}.";
		public const string ToYearLessThanFirstExistingYear = "The periods cannot be generated because the value in the To Year box is less than {0}.";
		public const string CannotGenerateOver99Years = "You cannot generate periods for more than 99 years at a time.";
		public const string ConfirmClosingInSubledgersCaption = "Close Periods";
		public const string ConfirmClosingInSubledgers = "The selected period or periods will be closed in {0} and {1}. To proceed, click OK.";
		public const string FinPeriodCannotBeUsedForProcessing = "The {0} financial period cannot be used for processing.";
		public const string FinPeriodCannotBeUsedForProcessingForAtLeastOneCompany = "The {0} financial period cannot be used for processing for at least one company.";
		public const string FinPeriodIsClosedInAtLeastOneCompany = "The {0} financial period is closed in at least one company.";
		public const string PossibleMixedRates = "Journal entries may have individual currency conversion rates. Review the project transaction to view the conversion rate.";
		public const string DocumentGraphExtensionIsNotDefined = "The DocumentGraphExtension extension is not defined for the {0} graph.";
		public const string ShiftedOrganizationCalendarsDetected = "The following companies have calendars shifted relative to the master calendar: {0}.";
		public const string ShortOrganizationCalendarsDetected = "The following companies have calendars shorter than the master calendar: {0}.";
		public const string OrganizationCalendarDoesNotExist = "The calendar for the {0} company does not exist. Generate it on the Company Financial Calendar (GL201100) form.";
		public const string NeedToCreateFirstCalendarYear = "Select a company and create its first calendar year if needed.";
	    public const string OnlyFirstOrLastYearCanBeDeleted = "Only first or last financial year can be deleted.";
		public const string CashAccountIsNotForProjectPurposes = "The {0} cash account is not recommended to be used for project purposes.";
		public const string AccountIsControlPostedAllow = "Account {0} is a control account for {1}. Although posting to this account is allowed, we recommend that you select a non-control account.";
		public const string AccountIsControlPostedDisable = "Account {0} is a control account for {1}, and posting to it is prohibited. Please select a non-control account.";
		public const string AccountIsNotControlForModule = "Account {0} is not a control account. Please select an account configured as a control account for {1}.";
		public const string AccountIsControlForAnotherModule = "Account {0} is a control account for {1}. Please select an account configured as a control account for {2}.";
        public const string ReversingBatchExists = "One or more reversing batches already exist. To review the reversing entries, click the link in the Reversing Batches box. Do you want to proceed with reversing the batch?";
        public const string AutoReversingBatchExists = "The batch has the Auto Reversing check box selected and will be reversed by the system on posting or on period closing depending on the configuration on the General Ledger Preferences (GL102000) form. Do you want to continue?";
		public const string CannotReleaseRetainageDocumentIfFeatureOff = "A document with nonzero retainage amount cannot be processed if the Retainage Support feature is disabled on the Enable/Disable Features (CS100000) form.";
		public const string BaseCurrencyDiffers = "The base currency of the {0} branch differs from the base currency of the originating branch.";
		public const string LedgerBaseCurrencyDifferFromCompany = "The base currency of the {0} company does not match the {1} ledger currency.";
		public const string SaveOrCancelDataModificationsBeforeImport= "Save or cancel data modifications before import.";
		public const string CashAccountBaseCurrencyDiffers = "The {0} cash account is associated with the {1} branch whose base currency differs from the base currency of the originating branch.";
		public const string CantReleaseTransactionInterbranchDisabled = "The document cannot be released because the Inter-Branch Transactions feature is disabled on the Enable/Disable Features (CS100000) form.";
		public const string ExtRefNumberLengthCannotExceed15Chars = "The length of Ext. Ref. Number cannot exceed 15 characters for a GL transaction.";
		public const string IncorrectCountryCodeinFeature = "Incorrect country code ({0}) for the Localization attribute. Please check the correctness of the Features.xml file.";
		public const string FinPeriodExistsinBudget = "The {0} financial period cannot be deleted because a budget has been created for this period in the {1} ledger for the {2} branch.";
		#endregion

		#region Translatable Strings used in the code
		public const string AdjustmentPeriod = "Adjustment Period";
		public const string PostPeriodApprovalDetails = "Post period: {0}";
		public const string QuarterDescr = "Quarter# {0}";
		public const string PeriodDescr = "Period# {0}";
		public const string SummaryTranDesc = "";
		public const string RoundingDiff = "Rounding difference";
		public const string AllocTranSourceDescr = "Src: {0}";
		public const string AllocTranDestDescr = "Dst: {0}";
		public const string ConsolidationDetail = "Consolidation detail";
		public const string PeriodFormatted = "Period {0:00}";
		public const string Times = "Times";
		public const string Period = "Period(s)";
		public const string Week = "Week(s)";
		public const string Month = "Month(s)";
		public const string Day = "Day(s)";
		public const string COAOrderOp0 = "1:Assets 2:Liabilities 3:Income and Expenses";
		public const string COAOrderOp1 = "1:Assets 2:Liabilities 3:Income 4:Expenses";
		public const string COAOrderOp2 = "1:Income 2:Expenses 3:Assets 4:Liabilities";
		public const string COAOrderOp3 = "1:Income and Expenses 2:Assets 3:Liabilities";
		public const string COAOrderOp128 = "Custom Chart of Accounts Order";
		public const string Periods = "Periods";
		public const string DecremenFirstYear = "Shift First Year";
		public const string BatchDetails = "Batch Details";
		public const string InterCoTranDesc = "Balancing entry for: {0}";
		public const string Draft = "Draft";

		public const string ScheduleDays = "Day(s)";
		public const string ScheduleWeeks = "Week(s)";
		public const string ScheduleMonths = "Month(s)";
		public const string SchedulePeriods = "Period(s)";
		public const string COAOrder = "COA Order";

		#region PeriodTypes
		public const string PT_Month = "Month";
		public const string PT_BiMonth = "Two Months";
		public const string PT_Quarter = "Quarter";

		public const string PT_Week = "Week";
		public const string PT_BiWeek = "Two Weeks";
		public const string PT_FourWeeks = "Four Weeks";
		public const string PT_FourFourFive = "4-4-5 Week";
		public const string PT_FourFiveFour = "4-5-4 Week";
		public const string PT_FiveFourFour = "5-4-4 Week";
		public const string PT_CustomPeriodsNumber = "Custom Number of Periods";
		#endregion
		public const string Stop = "Stop";
		public const string Every = "Every";
		public const string ExecutionDate = "Execution Date";
		public const string StopOnExecutionDate = "Stop on Execution Date";
		public const string StopAfterNumberOfExecutions = "Stop After Number of Executions";
		public const string ExecutedTimes = "Executed (Times)";
		public const string ExecutionLimitTimes = "Execution Limit (Times)";

		public const string DeletingFailed = "Deletion has failed.";
		public const string TaxableAmountCalculatedError = "The taxable amount cannot be calculated because the Exclude from Tax-on-Tax Calculation check box is selected for some of the taxes on the Taxes form.";
		public const string InterfaceMethodNotSupported = "This interface method is not supported for this document type.";
		public const string RowCanNotInserted = "The row cannot be inserted.";
		public const string UnknownFinPeriodType = "The financial period type is unknown.";
		public const string ManualNumberingError = "A reference number cannot be assigned to this document because the Manual Numbering check box is selected on the Numbering Sequences (CS201010) form for the numbering sequence specified for documents of this type. Only automatic numbering is supported on this form.";

		public const string TheAccessRoleWillBeAssignedToAllBranchesOfTheCompany = "The {0} access role will be assigned to all branches of the {1} company.";
		public const string RemoveTheAccessRoleFromTheSettingsOfAllBranchesOfTheCompany = "Remove the access role from the settings of all branches of the {0} company?";
		#endregion

		#region Graph Names
		public const string JournalEntry = "Journal Transactions Entry";
		public const string JournalTransaction = "Journal Transaction";
		public const string AccountByPeriodEnq = "GL Account History - Detailed Inquiry";
		public const string AccountClassMaint = "GL Account Class Maintenance";
		public const string AccountHistoryBySubEnq = "GL Account History - Inquiry by Sub ";
		public const string AccountHistoryByYearEnq = "GL Account History - Inquiry by Period";
		public const string AccountHistoryEnq = "GL Account History - Summary Inquiry";
		public const string AccountMaint = "Accounts Maintenance";
		public const string AllocationMaint = "Allocations Maintenance";
		public const string AllocationProcess = "Allocation Processing";
		public const string FiscalYearSetupMaint = "Financial Year";
		public const string PostProcess = "Process of Posting GL Transactions";
		public const string ScheduleProcess = "Repeating Tasks Processing";
		public const string ScheduleMaint = "Repeating Tasks Maintenance";
		public const string GLConsolReadMaint = "Consolidation Maintenance";
		public const string GLConsolSetupMaint = "Consolidation Setup";
		public const string GLSetupMaint = "General Ledger Preferences";
		public const string GLSetupApproval = "GL Approval Preferences";
		public const string SubAccountMaint = "Subaccount Maintenance";
		public const string GeneralLedgerMaint = "General Ledger Maintenance";
		public const string FiscalPeriodMaint = "Financial Period Maintenance";
		#endregion

		#region Cache Names
		public const string SubAccountSegment = "Subaccount Segment";
		public const string BudgetArticle = "Budget Article";
		public const string Batch = "GL Batch";
		public const string BatchNew = "GL Batch New";
		public const string Transaction = "GL Transaction";
		public const string Account = "GL Account";
		public const string AccountClass = "GL Account Class";
		public const string Sub = "Subaccount";
		public const string GLTrialBalanceImportMap = "Trial Balance Import";
		public const string GLTrialBalanceImportDetails = "Trial Balance Import Details";
		public const string GLHistoryEnquiryResult = "GL History Enquiry Results";
		public const string Schedule = "Schedule";
		public const string Allocation = "Allocation";
		public const string FinancialYear = "Financial Year";
		public const string GLTranDoc = "GL Tran Document";
		public const string GLDocBatch = "GL Document Batch";
		public const string TransactionDoc = "Journal Voucher";
		public const string BranchAcctMap = "Branch Account Map";
		public const string BranchAcctMapTo = "Branch Account Map To";
		public const string BranchAcctMapFrom = "Branch Account Map From";
		public const string BatchSelection = "Batch to Process";
		public const string GLHistoryByPeriod = "GL History by Period";
		public const string RMDataSource = "Analitycal Report Manager";
		public const string FinancialPeriod = "Financial Period";
		public const string FinPeriod = "Fin. Period";
        public const string Ledger = "Ledger";
		public const string Company = "Company";
		public const string FinPeriodSetup = "Financial Period Template";
		public const string GLAllocationAccountHistory = "GL Allocation History for Account";
		public const string GLAllocationDestination = "GL Allocation Destination";
		public const string GLAllocationHistory = "GL Allocation History";
		public const string GLAllocationSource = "GL Allocation Source";
		public const string GLBudgetLineDetail = "GL Budget Line Detail";
		public const string GLBudgetTree = "GL Budget Tree";
		public const string GLConsolAccount = "GL Consolidation Account";
		public const string GLConsolBranch = "GL Consolidation Branch";
		public const string GLConsolOrganization = "GL Consolidation Organization";
		public const string GLConsolData = "GL Consolidation Data";
		public const string GLConsolLedger = "GL Consolidation Ledger";
		public const string GLConsolSetup = "GL Consolidation Setup";
		public const string GLHistory = "GL History";
		public const string GLNumberCode = "GL Number Code";
		public const string GLTax = "GL Tax Detail";
		public const string GLTaxTran = "GL Tax Transaction";
		public const string GLTranCode = "GL Transaction Code";
		public const string GLVoucher = "GL Voucher";
		public const string GLVoucherBatch = "GL Voucher Batch";
		public const string GLWorkBook = "GL Workbook";
		public const string WorkBookID = "Workbook ID";
        public const string ProjectContract = "Project/Contract";
		#endregion

		#region Combo Values
		public const string Actual = "Actual";
		public const string Report = "Reporting";
		public const string Statistical = "Statistical";
		public const string Budget = "Budget";

		public const string ModuleGL = "GL";
		public const string ModuleAP = "AP";
		public const string ModuleAR = "AR";
		public const string ModuleCM = "CM";
		public const string ModuleCA = "CA";
		public const string ModuleIN = "IN";
		public const string ModuleDR = "DR";
		public const string ModulePO = "PO";
		public const string ModuleSO = "SO";
		public const string ModeleFA = "FA";
		public const string ModulePM = "PM";
		public const string ModuleTX = "TX";
		public const string ModuleEP = "EP";
        public const string ModulePR = "PR";

        //-Batch status
        public const string Hold = "On Hold";
		public const string Released = "Released";
		public const string Balanced = "Balanced";
		public const string Unposted = "Unposted";
		public const string Posted = "Posted";
		public const string Completed = "Completed";
		public const string Voided = "Voided";
		public const string PartiallyReleased = "Partially Released";
		public const string Scheduled = "Scheduled";
		public const string PendingApproval = "Pending Approval";
		public const string Rejected = "Rejected";

		//Batch Type
		public const string BTNormal = "Normal";
		public const string BTConsolidation = "Consolidation";
		public const string BTTrialBalance = "Trial Balance";
		public const string BTReclassification = "Reclassification";
		public const string BTAllocation = "Allocation";

        //Type of reclassification
        public const string CommonReclassType = "Common";
        public const string Split = "Split";
        public const string Reclassification = "Reclassification";

		//AccountType
		public const string Asset = "Asset";
		public const string Liability = "Liability";
		public const string Income = "Income";
		public const string Expense = "Expense";
		public const string Totals = "Totals";
		public const string RetainedEarnings = "Retained Earnings";
		public const string YTDNetIncome = "YTD Net Income";

		//AccountPost Options
		public const string PostSummary = "Summary";
		public const string PostDetail = "Detail";

		public const string GLAccess = "GL Access";
		public const string GLAccessBudget = "GL Access Budget";
		public const string GLAccessDetail = "GL Access Detail";
		public const string GLAccessByAccount = "GL Access by Account";
		public const string GLAccessByBranch = "GL Access by Branch";
		public const string GLAccessBySub = "GL Access by Subaccount";
		public const string GLAccessByArticle = "GL Access by Budget Article";

		public const string Daily = "Daily";
		public const string Weekly = "Weekly";
		public const string Monthly = "Monthly";
		public const string Periodically = "By Financial Period";
		public const string OnDay = "On Day";
		public const string OnThe = "On the";
		//Allocation Distribution
		public const string ByPercent = "By Percent";
		public const string ByWeight = "By Weight";
		public const string ByAccountPTD = "By Dest. Account PTD";
		public const string ByAccountYTD = "By Dest. Account YTD";
		public const string ByExternalRule = "By External Rule";
		//Allocation Collection Method
		public const string CollectByAccountPTD = "By Account PTD";
		public const string CollectFromPreviousAllocation = "From Prev. GL Allocation";
		//Allocation SourceLimit
		public const string PercentLimitTypeByPeriod = "Period";
		public const string PercentLimitTypeByAllocation = "GL Allocation";

		//Auto Rev Options
		public const string AutoRevOnPost = "On Post";
		public const string AutoRevOnPeriodClosing = "On Period Closing";

		//Trial Balances Sign Type
		public const string Normal = "Normal";
		public const string Reversed = "Reversed";

		public const string Undefined = "Undefined";
		//Schedule MonthlyOnWeek options
		public const string OnFirstWeekOfMonth = "1st";
		public const string OnSecondWeekOfMonth = "2nd";
		public const string OnThirdWeekOfMonth = "3rd";
		public const string OnFourthWeekOfMonth = "4th";
		public const string OnLastWeekOfMonth = "Last";

		//Schedule MonthlyOnDayOfWeek options
		public const string MonthlyOnSunday = "Sunday";
		public const string MonthlyOnMonday = "Monday";
		public const string MonthlyOnTuesday = "Tuesday";
		public const string MonthlyOnWednesday = "Wednesday";
		public const string MonthlyOnThursday = "Thursday";
		public const string MonthlyOnFriday = "Friday";
		public const string MonthlyOnSaturday = "Saturday";
		public const string MonthlyOnWeekday = "Weekday";
		public const string MonthlyOnWeekend = "Weekend";

		public const string PeriodStartDate = "Start of Financial Period";
		public const string PeriodEndDate = "End of Financial Period";
		public const string PeriodFixedDate = "Fixed Day of the Period";

		//Trial Balance Import Statuses
		public const string New = "New";
		public const string Valid = "Validated";
		public const string Duplicate = "Duplicate";
		public const string Error = "Error";

		//Budget Distribution Methods
		public const string Evenly = "Evenly";
		public const string PreviousYear = "Proportionally to the Previous Year";
		public const string ComparedValues = "Proportionally to Compared Values";

		//Budget Preload Actions
		public const string ReloadAll = "Delete and Reload All Articles";
		public const string UpdateExisting = "Update Existing Articles Only";
		public const string UpdateAndLoad = "Update Existing Articles and Load Nonexistent Articles";
		public const string LoadNotExisting = "Load Nonexistent Articles Only";

		//Financial Periods Save Actions
		public const string FinPeriodUpdateNextYearStart = "Modify start date of the next year";
		public const string FinPeriodUpdateFinYearSetup = "Modify financial year settings";
		public const string FinPeriodExtendLastPeriod = "Extend last period";

		//Financial Periods Save Actions
		public const string EndYearCalculation_Default = "Last Day of the Financial Year";
		public const string EndYearCalculation_LastDay = "Include Last <Day of Week> of the Financial Year";
		public const string EndYearCalculation_ClosestDay = "Include <Day of Week> Nearest to the End of the Financial Year";

        //Organization types
	    public const string WithoutBranches = "Without Branches";
        public const string WithBranchesNotRequiringBalancing = "With Branches Not Requiring Balancing";
        public const string WithBranchesRequiringBalancing = "With Branches Requiring Balancing";
        public const string Group = "Organization Group";
        public const string Organization = "Organization";
        public const string Branch = "Branch";

		//Financial Perios Statuses
		public const string Inactive = "Inactive";
		public const string Open = "Open";
		public const string Closed = "Closed";
		public const string Locked = "Locked";

		//Financial Perios Status Actions
		public const string ActionOpen = "Open";
		public const string ActionClose = "Close";
		public const string ActionLock = "Lock";
		public const string ActionDeactivate = "Deactivate";
		public const string ActionReopen = "Reopen";
		public const string ActionUnlock = "Unlock";
		#endregion

		#region Custom Actions
		public const string Refresh = "Refresh";
		public const string Save = "Save";
		public const string ttipRefresh = "Refresh";
		public const string ttipSave = "Commit Changes";
		public const string Release = "Release";

		public const string ProcPost = "Post";
		public const string ProcPostAll = "Post All";

		public const string ProcRelease = "Release";
		public const string ProcReleaseAll = "Release All";


		public const string ProcSynchronize = "Synchronize";
		public const string ProcSynchronizeAll = "Synchronize All";

		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R2)]
		public const string ProcValidate = "Validate";
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R2)]
		public const string ProcValidateAll = "Validate All";

		public const string Process = IN.Messages.Process;
		public const string ProcessAll = IN.Messages.ProcessAll;
		public const string RecalculateBalanceTooltip = "Recalculate account balances based on posted transactions";

		public const string ProcRunSelected = "Run";
		public const string ProcRunAll = "Run All";

		public const string ProcRunNow = "Run Now";

		public const string ShowDocuments = "Unposted Documents";
		public const string ShowDocumentsNonGL = "Unreleased Documents";
		public const string GeneratePeriods = "Create Periods";
		public const string GeneratePeriodsToolTip = "Auto fill periods";
		public const string AutoFillFiscalYearSetupToolTip = "Auto fill periods";

		public const string GLEditDetails = "GL Edit Details";

		public const string AddToRepeatingTasks = "Add to Schedule";
		public const string ReverseBatch = "Reverse";
		public const string Edit = "Edit";
		public const string Reclassify = "Reclassify";
		public const string ReclassifyAll = "Reclassify All";
		public const string ReclassificationHistory = "Reclassification History";
		public const string BatchRegisterDetails = "Batch Register Details";


		public const string ViewBatch = "View Batch";
		public const string ttipViewBatch = "Shows selected batch.";
		public const string ViewSourceDocument = "View Source Document";

		public const string ViewAccountDetails = "Account Details";
		public const string ViewAccountBySub = "Account by Subaccount";
		public const string ViewAccountByPeriod = "Account by Period";

		public const string Membership = "Membership";
		public const string ttipMembership = "View user membership.";
		public const string ViewRestrictionGroups = "Manage Restriction Groups";

		public const string ShowDocumentTaxes = "Show Taxes";
		public const string ViewResultDocument = "View Document";

		public const string PreloadArticles = "Preload Articles";
		public const string Distribute = "Distribute";
		public const string PreloadArticlesTree = "Preload Accounts";
		public const string ConfigureSecurity = "Configure Security";
		public const string SaveAndAdd = "Save & Add";
		public const string SaveAndAddToolTip = "Save current record and add a new one (Ctrl+A)";

		public const string Load = "Load";
		public const string ViewSchedule = "View Schedule";
		public const string AddtoSchedule = "Add to Schedule";

		#endregion

		#region Field Names

		public const string PostPeriod = "Post Period";
		public const string FieldNameAccount = "Account";
        public const string AllowOnlyOneUnreleasedVoucherBatchPerWorkbook = "Allow Only One Unreleased Voucher Batch per Workbook";

        #endregion

		#region NameTemplates

		public const string NewBranchNameTemplate = "MAIN{0}";

		#endregion

		#region Module Names
		public const string ModuleNameAP = "Accounts Payable";
		public const string ModuleNameAR = "Accounts Receivable";
		public const string ModuleNameCA = "Cash Management";
		public const string ModuleNameCM = "Currency Management";
		public const string ModuleNameCR = "Customer Management";
		public const string ModuleNameDR = "Deferred Revenue";
		public const string ModuleNameFA = "Fixed Assets";
		public const string ModuleNameGL = "General Ledger";
		public const string ModuleNameIN = "Inventory";
		public const string ModuleNamePM = "Projects";
		public const string ModuleNamePO = "Purchase Orders";
		public const string ModuleNameSO = "Sales Orders";
		public const string ModuleNameTX = "Taxes";
		public const string ModuleNameWZ = "Implementation Scenarios";
		#endregion

	}
}

