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

namespace PX.Objects.EP
{
	[PXLocalizable(Prefix)]
	public static class Messages
	{
		// Add your messages here as follows (see line below):
		// public const string YourMessage = "Your message here.";
		#region Validation and Processing Messages
		public const string YouAreNotAlowedToViewTheDetails = "You are not allowed to view the details of this record.";
		public const string EarningTypeDeactivate = "Earning type {0} cannot be deactivated because it is used in the {1} setting on the Time & Expenses Preferences form EP101000";
        public const string InventoryItemIsType = "Inventory Item type is {0}.";
        public const string InventoryItemIsNotAnExpenseType = "Only inventory items of the Expense type can be selected.";
		public const string TipItemIsNotDefined = "To be able to specify a nonzero tip amount in the expense receipt, specify an appropriate tip item in the Non-Taxable Tip Item box on the Time & Expenses Preferences (EP101000) form.";
		public const string LegacyReceipt = "This receipt was created in an old version of Acumatica ERP. Taxes for this receipt will be calculated upon an update of the Tax Zone, Tax Category, Tax Calculation Mode, or Amount field.";
		public const string LegacyClaim = "This expense receipt, which is a legacy expense receipt created in an earlier version of Acumatica ERP, cannot be edited when included in an expense claim. To be able to modify data in this expense receipt, delete the expense claim {0}.";
        public const string LegacyClaimHeader = "The expense claim cannot be edited due to an incompatible tax calculation model. Create the expense claim again from scratch to process it further.";
        public const string TaxZoneEmpty = "Because the employee who is claiming the expenses has a tax zone specified, you may need to also specify a tax zone for the receipt.";
		public const string TaxZoneNotMatch = "Cannot add the expense receipt {0} to the expense claim {1} because they have different tax zones specified.";
        public const string TaxZoneCalcModeChangeOnReceiptsForm = "To change the tax zone and the tax calculation mode in an expense receipt, use the Expense Receipt (EP301010) form.";
        public const string CantChangeTaxCalcMode = "Cannot change the tax calculation mode in the expense claim because expense receipts and the expense claim must have the same tax calculation mode.";
        public const string CantChangeTaxZone = "Cannot change the tax zone in the expense claim because expense receipts and the expense claim must have the same tax zone.";
        public const string TaxCalcModeNotMatch = "Cannot add the expense receipt {0} to the expense claim {1} because they have different tax calculation modes specified.";
		public const string TipItemAssociatedWithTaxes = "The selected item is taxable and cannot be used as a non-taxable tip item. Select a tax-exempt item instead.";
		public const string Prefix = "EP Error";
        public const string Warning = "Warning";
		public const string RecordExists = "Record already exists.";
		public const string BAccountExists = "The {0} identifier is already used for another business account record (vendor, customer, employee, branch, company, or company group).";
		public const string VendorClassExists = "This ID is already used for the Vendor Class.";
		public const string EmployeeLoginExists = "This Login ID is assigned to Employee {0}: {1}. It cannot be associated with another Employee.";
		public const string CantDeactivateYourself = "You cannot change the status of your own employee record.";
		public const string DocumentOutOfBalance = "Document is out of balance.";
		public const string CustomerRequired = "Customer ID must be specified for billable items.";
		public const string Document_Status_Invalid = "Document Status is invalid for processing.";
		public const string EarningTypeInactive = "The earning type {0} selected on the Time & Expenses Preferences (EP101000) form is inactive. Inactive earning types are not available for data entry in new activities and time entries.";
		public const string EmployeeTermsCannotHaveCashDiscounts = "You cannot use Terms with configured Cash Discount for Employees.";
		public const string EmployeeTermsCannotHaveMultiplyInstallments = "You cannot use Terms with configured Multiple Installments for Employees.";
		public const string TimePeriodEnteredCanNotBeGreaterThen24Hours = "You cannot specify a time interval that is larger than 24 hours. Please split the record.";
		public const string CompleteAndCancelNotAvailableForTask = "Task in draft status could not be canceled or completed";
		public const string ConfirmDeleteAttendeeHeader = "Delete Attendee";
		public const string ConfirmDeleteAttendeeText = "Are you sure you want to delete";
		public const string ConfirmRescheduleNotificationHeader = "Event date was changed";
		public const string ConfirmRescheduleNotificationText = "Would you like to notify invited attendees about new event date?";
		public const string EMailWasChanged = "Email was changed";
		public const string SendInvitationToNewEMail = "Send the invitation to new e-mail";
		public const string InvalidEmail = "Incorrect e-mail";
		public const string MailFromUndefined = "Create message failed. Email account should be defined.";
		public const string MailToUndefined = "Create message failed. Email recipient should be defined.";
		public const string EventIsNotSaved = "Event must be saved";
		public const string EventIsThePast = "Incorrect Event Date";
		public const string EventIsNotEditable = "Incorrect Event Status";
		public const string DontHaveAppoveRights = "You don't have access rights to approve document.";
		public const string DontHaveRejectRights = "You don't have access rights to reject document.";
		public const string RequireCommentForApproveReject = "The document cannot be processed because a comment must be entered to complete the action. Please use the corresponding entry form to perform this action upon the document and enter a comment.";
		public const string ReleaseClaimWithoutFinPeriod = "Fin Period should be specified to release claim.";
		public const string EmptyEmailAccountAddress = "Email account address is empty";
		public const string EmailAccountNotActive = "The email account {0} ({1}) is inactive.";
		public const string MailAccountNotSpecified = "Email account is not specified";
		public const string InvalidEmailForOperation = "Invalid email message for this operation";
		public const string ValueMustBeGreaterThanZero = "The value must be greater than zero";
		public const string InventoryItemIsEmpty = "Inventory Item is not entered";
		public const string InventoryItemIsEmptyInfo = "No labor item has been specified for the time activity with the '{0}' description in the time card with the {1} references number and {2} date.";
		public const string DateNotInWeek = "The selected date does not belong to the week selected in the Summary area.";
		public const string AccountGroupIsNotAssignedForAccount = "Expense Account '{0}' is not included in any Account Group. Please assign an Account Group given Account and try again.";
		public const string ReportCannotBeFound = "Report '{0}' cannot be found";
		public const string EmailFromReportCannotBeCreated = "Email cannot be created for the specified report '{0}' because the report has not been generated or the email settings are not specified.";
		public const string AssigmentMapEntityType = "Only types can be used as entity type for assignment map.";
		public const string ProcessRouteSequence = "Sequence {0} process successfully.";
		public const string DocumentPreApproved = "No approver has been assigned to the document. The document is automatically approved.";
		public const string CannotDeleteCorrectionActivity = "In the correction Time Card if you want to delete/eliminate previosly released Activity just set the Time to zero.";
		public const string CannotDeleteCorrectionRecord = "In the correction Time Card if you want to delete/eliminate previosly released time record just set the Time to zero.";
		public const string LaborClassNotSpecified = "Labor Item is not specified for the Employee.";
		public const string OvertimeLaborClassNotSpecified = "Overtime Labor Item is not specified for the Employee.";
		public const string DepartmentInUse = "This Department is assigned to the Employee and cannot be changed.";
		public const string PositionInUse = "This Position is assigned to the Employee and cannot be changed.";
		public const string PositionInUseForDelete = "This Position is assigned to the Employee '{0}' and cannot be deleted.";

		public const string NoDefualtAccountOnProject = "Project preferences have been configured to get the expense account from projects, but the default cost account is not specified for the {0} project.";
		public const string NoDefualtAccountOnProject2 = "The time card cannot be released because the default cost account is not specified in the settings of the {0} project on the Defaults tab of the Projects (PM301000) form.";
		public const string NoDefualtAccrualAccountOnProject = "Project preferences have been configured to get the expense accrual account from projects, but the default accrual account is not specified for the {0} project.";
		public const string NoDefualtAccrualAccountOnProject2 = "The time card cannot be released because the accrual account is not specified in the settings of the {0} project on the Defaults tab of the Projects (PM301000) form.";
		public const string NoAccountGroupOnProject = "Project preferences have been configured to get the expense account from project tasks, but the {0} default cost account specified for the {1} task of the {2} project is not mapped to any account group.";
		public const string NoDefualtAccountOnTask = "Project preferences have been configured to get the expense account from project tasks, but the default cost account is not specified for the {0} task of the {1} project.";
		public const string NoDefualtAccountOnTask2 = "The time card cannot be released because the default cost account is not specified in the settings of the {0} project task of the {1} project on the Summary tab of the Project Tasks (PM302000) form.";
		[Obsolete]
		public const string NoDefualtAccrualAccountOnTask = "Project preferences have been configured to get the expense accrual account from project tasks, but the default accrual account is not specified for the {1} task of the {0} project.";
		public const string NoDefualtAccrualAccountOnTask2 = "The time card cannot be released because the accrual account is not specified in the settings of the {0} project task of the {1} project on the Summary tab of the Project Tasks (PM302000) form.";
		public const string NoAccountGroupOnTask = "Project preferences have been configured to get the expense account from project tasks, but the {0} default cost account of the {2} task of the {1} project is not mapped to any account group.";
		public const string NoExpenseAccountOnEmployee = "Project preferences have been configured to get the expense account from employees, but the expense account is not specified for the {0} employee.";
		public const string NoExpenseAccountOnEmployee2 = "The time card cannot be released because the expense account is not specified in the settings of the {0} employee on the Financial Settings tab of the Employees (EP203000) form.";
		public const string NoDefaultAccountOnEquipment = "Project preferences have been configured to get the expense account from equipment, but the default account is not specified for the {0} equipment.";
		public const string NoAccountGroupOnEmployee = "Project preferences have been configured to get the expense account from resources, but the {0} expense account of the {1} employee is not mapped to any account group.";
		public const string NoAccountGroupOnEquipment = "Project preferences have been configured to get the expense account from resources, but the {0} expense account for the {1} equipment is not mapped to any account group.";
		public const string NoExpenseAccountOnInventory = "Project preferences have been configured to get the expense account from inventory items, but the expense account is not specified for the {0} inventory item.";
		public const string NoAccrualExpenseAccountOnInventory = "Project preferences have been configured to get the expense accrual account from inventory items, but the expense accrual account is not specified for the {0} inventory item.";
		public const string NoAccountGroupOnInventory = "Project preferences have been configured to get the expense account from inventory items, but the {0} expense account of the {1} inventory item is not mapped to any account group.";
		public const string NoExpenseSubOnInventory = "Project preferences have been configured to combine the expense subaccount from inventory items, but the expense subaccount is not specified for the {0} inventory item.";
		public const string NoExpenseAccrualSubOnInventory = "Project preferences have been configured to combine the expense accrual subaccount from inventory items, but the expense accrual subaccount is not specified for the {0} inventory item.";
		public const string NoExpenseSubOnProject = "Project preferences have been configured to combine the expense subaccount from projects, but the expense subaccount is not specified for the {0} project.";
		public const string NoExpenseAccrualSubOnProject = "Project preferences have been configured to combine the expense accrual subaccount from projects, but the expense accrual subaccount is not specified for the {0} project.";
		public const string NoExpenseSubOnTask = "Project preferences have been configured to combine the expense subaccount from project tasks, but the expense subaccount is not specified for the {1} task of the {0} project.";
		public const string NoExpenseAccrualSubOnTask = "Project preferences have been configured to combine the expense accrual subaccount from project tasks, but the expense accrual subaccount is not specified for the {1} task of the {0} project.";
		public const string NoExpenseSubOnEmployee = "Project preferences have been configured to combine the expense subaccount from resources, but the expense subaccount is not specified for the {0} employee.";
		public const string NoDefaultSubOnEquipment = "Project preferences have been configured to combine the expense subaccount from resources, but the default subaccount is not specified for the {0} equipment.";
		public const string ExpenseAccrualIsRequired = "Expense Accrual Account is Required but is not configured for Non-Stock Item '{0}'. Please setup the account and try again.";
		public const string ExpenseAccrualSubIsRequired = "Expense Accrual Subaccount is Required but is not configured for Non-Stock Item '{0}'. Please setup the subaccount and try again.";
		public const string TimeCardInFutureExists = "Since there exists a Time Card for the future week you cannot change the Employee in the given week.";
		public const string EquipmentTimeCardInFutureExists = "Since there exists a Time Card for the future week you cannot change the Equipment in the given week.";
		public const string AlreadyReleased = "This document is already released.";
		public const string UnapprovedWhenRelease = "The expense claim has to be approved by a responsible person before it can be released.";
		public const string ApprovalRefNoteIDNull = "Record for approving not found, RefNoteId is undefined.";
		public const string ApprovalRecordNotFound = "Record for approving not found.";
		public const string TimeCardNoDelete = "Since there exists a timecard for the future week you cannot delete this Time Card.";
		public const string ProjectIsNotAvailableForEquipment = "This project is not available for current equipment";
		public const string NotificationTemplateNotFound = "The email template cannot be found.";
		public const string NotificationTemplateCDNotFound = "The {0} email template cannot be found.";
		public const string InvalidProjectTaskInActivity = "Activity is referencing Project Task that was not found in the system. Please correct the activity.";
		public const string OneOrMoreActivitiesAreNotApproved = "At least one activity of the time card is not approved. The time card can be approved only when all its activities are approved.";
        public const string ActivityIsNotApproved = "The activity is not approved.";
		
		public const string ActivityAssignedToTimeCard = "This Activity assigned to the Time Card. You may do changes only in a Time Card screen.";
		public const string UserWithoutEmployee = "The user '{0}' is not associated with an employee.";
        

        public const string UserIsInactive = "The user is not active.";
        public const string UserDeleted = "The record has been deleted";
        public const string EmployeeIsInactive = "The employee is not active.";
        public const string EmployeeDeleted = "The record has been deleted.";
        public const string Employeedeattach = "The user is not associated with an employee record.";
		
        public const string EquipmentSetupRateIsNotDefined = "The Setup Rate Class is not defined for the given Equipment. Please set the Setup Rate Class on the Equipment screen and try agaain.";
        public const string EquipmentRunRateIsNotDefined = "The Run-Rate Item is not specified for the {0} equipment on the Equipment (EP208000) form.";
        public const string EquipmentSuspendRateIsNotDefined = "The Suspend Rate Item is not defined for the given Equipment. Please set the Suspend Rate Class on the Equipment screen and try agaain.";
		public const string defaultActivityTypeNoTracTime = "Default Activity Type must have track time is enabled.";
		public const string OvertimeNotAllowed = "Overtime cannot be specified untill all the available regular time is utilised. Regular time for week = {0} hrs";
		public const string TimecradIsNotNormalized = "The time card must be filled out completely. The norm of regular hours this employee should spend during a week is {0} hours. You can click Normalize Time Card on the table toolbar of the Summary tab to automatically fill up the remaining hours.";
		public const string DisableEmployeeBeforeDeleting = "Make this employee inactive before deleting";
		public const string UserParticipateInAssignmentMap = "User '{0}' participate in the Assignment and Approval Map '{1}'";
		public const string UserCannotBeFound = "User cannot be found";
		public const string CustomWeekNotFound = "Custom Week cannot be found";
		public const string CustomWeekNotFoundByDate = "Custom Week cannot be found. Custom Week's must be generated with date greater than {0:d}";
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R1)]
        public const string NoSalesAccountOnInventory = "Sales Account is not setup for the Inventory Item but is required. Inventory Item '{0}'.";
		public const string TotalTimeForWeekCannotExceedHours = "Regular Time for the week cannot exceed {0} hrs.";
		public const string TimecardIsNotValid = "Time Card is not valid. Please correct and try again.";
		public const string InactiveEpmloyee = "The status of employee '{0}' is '{1}'.";
		public const string ExistsActivitiesLessThanDate = "The {0} cannot be earlier than '{1:d}' because some activities or time cards were created after the specified period.";
		public const string ExistsActivitiesGreateThanDate = "The {0} cannot be later than '{1:d}' because some activities or time cards were created before the specified period.";
		public const string ActivityIsNotCompleted = "The activity is not completed.";
		public const string CustomWeekNotCreated = "You must configure the weeks in the Custom Week Settings tab for the period spanning between {0:d} and {1:d} because there are existing Activities in this period";
		public const string CustomWeeksUpdated = "This time card is no longer valid because the custom week settings have been changed. Please re-create the time card.";
		public const string TimecardMustBeSaved = "Time Card must be saved before you can view the Activities.";
		public const string CostInUse = "You cannot delete this record of \"Employee Cost\" because it is already in use.";
		public const string CannotDeleteInUse = "Cannot delete because it is already in use.";
		public const string TransactionNotExists = "Transaction does not exist";
		public const string MustBeEmployee = "User must be an Employee to use current screen.";
		public const string CurrentUserTimeZoneDiffersFromEmployeeTimeZone = "Time is displayed in the time zone of the current user ({0}), which differs from the time zone of the employee of the time card ({1}).";
		public const string CurrentUserTimeZoneDiffersFromAtLeastOneActivitiesWereReportedIn = "Time is displayed in the time zone of the current user ({0}), which differs from at least one of the time zones the time card activities were reported in.";
		public const string CurrentUserTimeZoneDiffersFromActivitiesWereReportedIn = "Time is displayed in the time zone of the current user ({0}), which differs from the time zone the time card activities were reported in ({1}).";
		public const string ProjectIsNotActive = "Project is not active. Cannot record cost transaction against inactive project. Project: {0}";
		public const string ProjectIsCompleted = "Project is completed. Cannot record cost transaction against completed project. Project: {0}";
		public const string ProjectTaskIsNotActive = "Project Task is not active. Cannot record cost transaction against inactive task. ProjectID: {0} TaskID:{1}";
		public const string ProjectTaskIsCompleted = "Project Task is completed. Cannot record cost transaction against completed task. ProjectID: {0} TaskID:{1}";
		public const string ProjectTaskIsCancelled = "Project Task is cancelled. Cannot record cost transaction against cancelled task. ProjectID: {0} TaskID:{1}";
		public const string ProjectsDefaultAccountNotFound = "Failed to find the account with the given AccountID:{0}. This Account is specified as Project's Default Account";
		public const string ProjectTasksDefaultAccountNotFound = "Failed to find the account with the given AccountID:{0}. This Account is specified as Project Task's Default Account";
		public const string EmployeeExpenseAccountNotFound = "Failed to find the account with the given AccountID:{0}. This Account is specified as Employees Expense Account";
		public const string ItemCogsAccountNotFound = "Failed to find the account with the given AccountID:{0}. This Account is specified as InventoryItem's COGS Account";
		public const string EmployeePartExceed = "The employee part must not exceed line total amount.";
		public const string EmployeePartSign = "The employee part must have the same sign as total amount.";
		public const string TipSign = "The tip amount must have the same sign as the total amount.";
		public const string TaxSign = "The tax amount must have the same sign as the total amount.";
		public const string TaxableSign = "The taxable amount must have the same sign as the total amount.";
		public const string ApproveAllConfirmation = "Are you sure you want to approve all the listed records?";
		public const string RejectAllConfirmation = "Are you sure you want to reject all the listed records?";
		public const string ActivityIsOpenAndCannotbeApproved = "Activity is Open and is not visible to the Approver. Please complete the activity so that it can be approved.";
		public const string CustomerDoesNotMatch = "The customer doesn't match the default customer on the claim.";
		public const string EndDateMustBeGreaterOrEqualToTheStartDate = "End Date must be greater than or equal to Start Date.";
		public const string CannotFindLabor = "Cannot find the labor item for the {0} employee.";
		public const string StartDateWrongYear = "Start Date must have a Year at {0}.";
		public const string Step = "Step";
		public const string Rule = "Rule";
        public const string MapID = "Map ID";
		public const string NoName = "<No Name>";
		public const string TraceAssign = "Assign: {0} - {1}";
		public const string TraceReassignedToDelegate = "Reassigned: {0} - {1}";
		public const string TraceReassignToDelegateCantFindAvailableDelagete = "The delegate or their delegates are not available: {0}. The approval has not been delegated.";
		public const string TraceRuleResult = "{0} - {1} :: {2} - {3}";
		public const string TraceCondition = "{0} - {1} - {2} - {3} | {4}\n";
		public const string TraceConditions = "Conditions:\n{0}";
		public const string FieldCannotBeFound = "The specified field cannot be found.";
		public const string EmployeeFormulaIsInvalid_Wrapper = "The following formula in the Employee box is invalid: {0}.";
		public const string EmployeeFormulaIsInvalid_Field = "The Employee box is empty.";
		public const string EmployeeFormulaIsInvalid = "At least one element in the formula is incorrect or missing.";
		public const string AllEmployeesByFilterRequired = "Approval will be required from all employees specified in the following list.";
		public const string ViewMustBeDeclared = "The view with type '{0}' must be declared inside the graph for using EPApprovalAutomation.";

		public const string EventNonOwned = "Cannot perform operation. You are not an owner of event '{0}'.";
		public const string EventInStatus = "Event '{0}' already is {1}.";
		public const string ValueShouldBeNonZero = "Value should not be 0.";

        public const string WorkgroupIsInUse = "Workgroup is in use and cannot be deleted.";
        public const string WorkgroupIsInUseAtAssignmentMap = "The workgroup '{0}' is used in the '{1}' map '{2}'.";
        public const string WorkgroupIsInUseInApproval = "The {0} workgroup is assigned to approve a record of the {1} type. {1} reference number: {2}. {1} description: {3}.";
        public const string EmployeeIsInUseAtAssignmentMapPart1 = "Employee '{0}' and this workgroup are used in the '{1}' map '{2}'.";
        public const string EmployeeIsInUseAtAssignmentMapPart2 = "Deactivation of this employee may cause delays";
        
        public const string Assignment = "Assignment";
        public const string ApprovalNotificationError = "Unable to process approval notification. {0}";
        public const string ApprovalProcessError = "Unable to process the approval.\n{0}";

		public const string EmployeeClassChangeWarning = "Please confirm if you want to update current Employee settings with the Employee Class defaults. Original settings will be preserved otherwise.";
		public const string DateMustBeGreaterOrLess = "{0} must be greater than or equal to '{1:d}' or less than or equal to '{2:d}'";

		public const string TimecardCannotBeReleased_NoRights = "Timecard can not be released. Most probably current user has no right to view/release given timecard.";
		public const string IncorrectUsingAttribute = "Attribute '{0}' can be used only with DAC '{1}' or its inheritors";
		public const string WrongDates = "Wrong dates have been specified.";
		public const string UntilGreaterFrom = "The Until date cannot be earlier than the From date.";
		public const string UntilGreaterThanNow = "The Until date is later than the current business date.";
		public const string ClaimCannotBeCreatedForReceiptBecauseCuryCannotBeOverriden = "A claim cannot be created for the expense receipt because the employee currency differs from the corporate card currency of the expense receipt and cannot be overridden. Enable currency override for the employee on the Employees (EP203000) form first.";
		public const string ReceiptCannotBeAddedToClaimBecauseClaimCuryDiffersFromEmployeeCury = "The expense receipt cannot be added to the claim because the employee currency differs from the receipt currency and cannot be overridden. Enable currency override for the employee on the Employees (EP203000) form first.";
		public const string ThereAreNoTimeCardsToGenerate = "There are no time cards to generate.";
		public const string FailedToCreateCorrectionTC = "Failed to create correction timecard. Please check the errors on the Details.";
		public const string CardCurrencyAndTheClaimCurrencyMustBeEqual = "The corporate card currency and the claim currency must be equal.";
		public const string SummarySyncFailed = "Activity cannot be synced with the Summary records.";
		public const string AmountOfTheExpenseReceiptToBePaidWithTheCorporateCardCannotBeNegative = "The amount of the expense receipt to be paid with the corporate card cannot be negative.";
		public const string SomeBoxesAndActionsOnTheFormAreUnavailableExpenseReceiptIsMatchedToBankStatement = "Some boxes and actions on the form are unavailable because the expense receipt is matched to a bank statement record with the date of {0}.";
		public const string ExpenseReceiptCannotBeDeletedBecauseItIsMatchedToBankStatement = "The expense receipt cannot be deleted because it is matched to a bank statement record with the date of {0}.";
		public const string EmployeePartMustBeZero = "The employee part of the expense receipt must be zero. You can submit a separate expense receipt for personal expenses paid with a corporate card.";
		public const string SummaryTaskNotFound = "Activity cannot be synced with the Summary records. Task not found, please clear Task field to continue.";
        public const string AssignmentNotApprovalMap = "The type of the selected map is incorrect. Only an approval map can be used for assigning documents for approval.";
		public const string CanNotProcessMessage = "The system cannot process the '{0}' message. The expected type is {1}.";
		public const string DuplicateTimecard = "A time card for this week already exists in the system. Most probably, it has just been created by another user.";
		public const string EmployeeCostRateNotFound = "The Employee Labor Cost Rate has not been found.";
		public const string ContainsMixedRates = "The Cost Rate contains mixed rates. You can find the exact rate by date on the Details tab.";
		public const string ExpenseReceiptCannotBeSummarized = "The following expense receipts cannot be summarized because they have been already matched to bank statement transactions: {0}. Please unmatch the expense receipts on the Import Bank Transactions (CA306500) form first.";
		public const string ExpenseClaimCannotBeReleasedSummary = "The expense claim cannot be released. Please review the details of the expense claim.";
		public const string ExpenseClaimCannotBeReleased = "The expense claim cannot be released because the {0} cash account of the {1} corporate card is associated with the {2} payment method that requires a unique payment reference number. Either clear the Require Unique Payment Ref. check box on the Settings for Use in AP tab of the Payment Methods (CA204000) form for the payment method or review the configuration of the cash account.";
		public const string ExpenseClaimCannotBeReleasedAccountHasBeenDeleted = "The {0} expense claim cannot be released because the {1} account specified in the employee details has been deleted. To be able to release the document, select another account for the {2} employee in the AP Account box on the Financial Settings tab of the Employees (EP203000) form.";
		public const string BranchCannotBeAssociated = "A branch with the base currency other than {0} cannot be associated with the {1} account.";
		public const string BranchCannotBeAssociatedDueToDocumentsInvolved = "You cannot select a branch with the currency different from {0} for the employee until there is at least one unreleased document with this base currency. Remove or release the following documents: {1}";

		public const string CogsNotDefinedForInventoryItem = "The COGS account is not defined for the {0} inventory item.";
		public const string CogsNotDefinedForInventoryItem2 = "The time card cannot be released because the expense account is not specified in the settings of the {0} labor item on the GL Accounts tab of the Non-Stock Items (IN202000) form.";
		public const string InventoryAccountNotDefinedForInventoryItem = "The inventory account is not defined for the {0} inventory item.";
		public const string InventoryAccountNotDefinedForInventoryItem2 = "The time card cannot be released because the expense accrual account is not specified in the settings of the {0} labor item on the GL Accounts tab of the Non-Stock Items (IN202000) form.";

		public const string BankTranIsPending = "This receipt cannot be taken off hold because the bank transaction has the Pending status.";
		public const string CancelledTask = "Task is Canceled and cannot be used for data entry.";
		public const string ProbationEndDateUpdated = "The probation end date was updated based on the start date and the probation period specified in the Employee Classes (EP202000) form. You can manually adjust it if you want.";
		public const string ProbationEndDateEarlierThanStartDate = "The Probation End Date cannot be earlier than the Start Date.";
		public const string ProbationPeriodMonthsChanged = "The new probation period will be used only for newly hired employees of the Employee Class. For existing employees, probation period end dates should be adjusted individually, if needed.";

		#endregion //Validation and Processing Messages

		#region Translatable Strings used in the code
		public const string Errors = "Errors";
		public static string Message = "Message";
		public const string ARInvoiceDesc = "Reimbursable Personal Expenses";
		public const string Release = "Release";
		public const string ReleaseAll = "Release All";
		public const string Approve = "Approve";
		public const string ApproveEntireDoc = "Approve Document";
		public const string ApproveDoc = "Approve Document";
		public const string ApproveAll = "Approve All";
		public const string ClaimDetails = "Claim Details";
		public const string GenerateTimeCards = "Generate Time Cards";
		public const string Sent = CR.Messages.Sent_MassMailStatus;
		public const string Send = "Send";
		public const string SendMessage = "Send Message";
		public const string Forward = "Forward";
		public const string ForwardMessage = "Forward Message";
		public const string Reply = "Reply";
		public const string ReplyMessage = "Reply Message";
		public const string View = "View";
		public const string AddAttendee = "Add Attandee";
		public const string RemoveAttendee = "Delete Attandee";
		public const string Inbox = "Inbox";
		public const string Received = "Received";
		public const string Draft = CT.Messages.Draft;
		public const string Open = "Open";
		public const string OnHold = "On Hold";
		public const string Deferred = "Deferred";
		public const string Outbox = "Outbox";
		public const string Failed = "Failed";
		public const string Completed = GL.Messages.Completed;
		public const string Canceled = "Canceled";
		public const string Cancel = "Cancel";
		public const string CancelTooltipS = "Marks current record as canceled";
		public const string Receive = "Receive";
		public const string ProcessAll = IN.Messages.ProcessAll;
		public const string Process = IN.Messages.Process;
		public const string AcceptInvitation = "Accept";
		public const string AcceptInvitationTooltip = "Accept invitation to event";
		public const string RejectInvitation = "Reject";
		public const string RejectInvitationTooltip = "Reject invitation to event";
		public const string SendCard = "Send Card";
		public const string SendCardTooltip = "Send i-Calendar format information about event by e-mail";
		[Obsolete]
		public const string SendInvitations = "Invite All";
		[Obsolete]
		public const string SendInvitationsTooltip = "Send e-mail invitations for all";
		[Obsolete]
		public const string SendPersonalInvitation = "Invite";
		[Obsolete]
		public const string SendPersonalInvitationTooltip = "Send e-mail invitation for current record";
		[Obsolete]
		public const string ResendPersonalInvitation = "Invitation was already sent. Are you sure you want send repeat invitation?";
		[Obsolete]
		public const string Invitation = "Invitation";
		[Obsolete]
		public const string CancelInvitation = "Cancel Invitation";
		[Obsolete]
		public const string NotifyNotInvitedAttendees = "Would you like send the event invitation to only not invited attendees?";
		[Obsolete]
		public const string NotifyAllInvitedAttendees = "Would you like send the event invitation to all previously invited attendees?";
		[Obsolete]
		public const string NotifyAttendees = "Would you like to send the event invitation to the selected attendees?";
		[Obsolete]
		public const string ConfirmCancelAttendeeInvitations = "Would you like send cancel invitations?";
		public const string NoteID = "Note ID";
		public const string TaskType = "Type";
		public const string Assigned = "Assigned";
		public const string AssignedTo = "Assigned To";
		public const string WorkGroup = "Workgroup";
		public const string Owner = "Owner";
		public const string Escalated = "Escalated";
		public const string FollowUp = "Follow Up";
		public const string DayOfWeek = "Day Of Week";
		public const string MarkAsCompleted = "Mark As Completed";
		public const string MarkAsCompletedTooltip = "Marks current record as completed (Ctrl + K)";
		public const string Dismiss = "Dismiss";
		public const string DismissAll = "Dismiss All";
		public const string Snooze = "Snooze";
		public const string Complete = "Complete";
		public const string CompleteStep = "Complete Step";
		public const string CompleteTooltipS = "Marks current record as completed";
		public const string CompleteAndFollowUp = "Complete & Follow-Up";
		public const string CompleteAndFollowUpTooltip = "Marks current record as completed and creates new its copy (Ctrl + Shift + K)";
		public const string CancelSending = "Cancel Sending";
		public const string CancelSendingTooltip = "Cancels sending of the email message";
		public const string AssignmentMap = "Assignment Map";
		public const string ViewDetails = "View Details";
		public const string ViewEntity = "View Entity";
		public const string ViewOwner = "View Owner";
		public const string ttipViewEntity = "View Reference Entity";
		public const string ttipViewOwner = "Shows current owner";
		public const string ttipViewAccount = "Shows current customer";
		public const string ttipViewParentAccount = "Shows current parent customer";
		public const string ttipViewParentActivity = "View Parent Activity";
		public const string Periods = "Periods";
		public const string CompleteEventTooltip = "Complete Event (Ctrl + K)";
		public const string CompleteAndFollowUpEvent = "Complete & Follow-Up";
		public const string CompleteAndFollowUpEventTooltip = "Complete & Follow-Up (Ctrl + Shift + K)";
		public const string Min5 = "5 minutes";
		public const string Min10 = "10 minutes";
		public const string Min15 = "15 minutes";
		public const string Min20 = "20 minutes";
		public const string Min25 = "25 minutes";
		public const string Min30 = "30 minutes";
		public const string Min35 = "35 minutes";
		public const string Min40 = "40 minutes";
		public const string Min45 = "45 minutes";
		public const string Min50 = "50 minutes";
		public const string Min55 = "55 minutes";
		public const string Min60 = "1 hour";
		public const string Min120 = "2 hours";
		public const string Min240 = "4 hours";
		public const string Min720 = "0.5 days";
		public const string Min1440 = "1 day";
		public const string Date = "Date";
		public const string Delete = "Delete";
		public const string ttipDelete = "Delete selected messages";
		public const string CreatedBy = "Created By";
		public const string StartTime = "Start Time";
		public const string Company = "Company";
		public const string AddEvent = "Add Event";
		public const string ExportCalendar = "Export Calendar";
		public const string ExportCalendarTooltip = "Export iCalendar-format information about calendar";
		public const string ImportCalendar = "Import Calendar";
		public const string ImportCalendarTooltip = "Import iCalendar-format information about calendar";
		public const string ExportCard = "Export Card";
		public const string ExportCardTooltip = "Export iCalendar-format information about event";
		public const string ImportCard = "Import Card";
		public const string ImportCardTooltip = "Import iCalendar-format information about event";
		public const string CompleteEvent = "Complete";
		public const string CancelEvent = "Cancel";
		public const string OneOfRecordsIsNull = "One of records is null";
		public const string NullStartDate = "Start Date cannot be null";
		public const string EntityIsNotViewed = "Unread";
		public const string EntityIsViewed = "Read";
		public const string Number = "Number";
		public const string Name = "Name";
		public const string OldValue = "Old Value";
		public const string NewValue = "New Value";
		public const string CompletedAt = "Completed At";
		public const string CannotApproveRejectedItem = "Cannot approve rejected document.";
		public const string AddTask = "Add Task";
		public const string AddActivity = "Add Activity";
		public const string AddActivityTooltip = "Add New Activity";
		public const string Actions = "Actions";
		public const string Reject = "Reject";
		public const string ReassignApproval = "Reassign";
		public const string RejectDoc = "Reject Document";
		public const string Always = "Always";
		public const string IfNoApproversFoundatPreviousSteps = "If No Approvers Found at Previous Steps";
		public const string Assign = "Assign";
		public const string Edit = "Edit";
		public const string Hold = "Hold";
		public const string ApprovalDate = "Approval Date";
		public const string EmptyEmployeeID = "Employee must be set";
		public const string EmployeeID = "Employee ID";
		public const string Active = "Active";
		public const string Inactive = "Inactive";
		public const string Copy = "Copy";
		public const string Correct = "Correct";
		public const string CreateNew = "New";
		public const string Accounts = "Mail Boxes";
		public const string Group = "Group";
		public const string Workgroup = "Workgroup";
		public const string Router = "Router";
		public const string Jump = "Jump";
		public const string Wait = "Wait";
		public const string WaitForApproval = "Collect All Approvals from This Step";
		public const string DelegationOfApprovals = "Approvals";
		public const string DelegationOfExpenses = "Expense Receipts and Expense Claims";
		public const string Next = "Next";
		public const string NextStep = "Go to Next Step";
		public const string DownloadEmlFile = "Download .eml file";
		public const string DownloadEmlFileTooltip = "Export as .eml file";
		public const string Automatically = "Automatically";
		public const string Resend = "Resend";
		public const string ActivityIsBilled = "Activity cannot be deleted because it has already been billed";
		public const string ActivityIsReleased = "Activity cannot be deleted because it has already been released";
		public const string ActivityIs = "Activity in status \"{0}\" cannot be deleted.";
		public const string EmailTemplateIsNotConfigured = "Email message template is not configured";
		public const string RejectAll = "Reject All";
		public const string PreloadFromTasks = "Preload from Tasks";
		public const string PreloadFromTasksTooltip = "Preload Activities from Tasks";
		public const string PreloadFromPreviousTimecard = "Preload from Previous Time Card";
		public const string PreloadFromPreviousTimecardTooltip = "Preload Time from Previous Time Card";
		public const string PreloadHolidays = "Preload Holidays";
		public const string NormalizeTimecard = "Normalize Time Card";
		
		public const string SetupNotEntered = "Required configuration data is not entered. Default Time Activity on the Time & Expense Preference screen must be set to \"Track Time\" Activity. Please check the settings on the {0}";
		public const string StartDateOutOfRange = "Date is out of range. It can only be within \"From Date\" and \"Till Date\".";
		public const string FixedDayOfMonth = "Fixed Day of Month";
		public const string EndOfMonth = "End of Month";
		public const string EndOfYear = "End Of Year";
		public const string WeekInUse = "Week in use, cannot delete record";
		public const string WeekNotLast = "Week is not last, cannot delete record";
		public const string StartDateGreaterThanEndDate = "{0} cannot be later than '{1:d}'";
		public const string IncrorrectPrevWeek = "Incorrect \"End Date\" of previous week";
		public const string EmployeeContact = "Employee Contact";
		public const string PrintExpenseClaim = "Print Expense Claim";
		public const string HasOpenActivity = "The time card includes one or multiple time activities with the Open status. All time activities must be completed before the time card may be submitted for approval.";
		public const string HasPendingOrRejectedActivity = "The time card includes one or multiple time activities that require approval by project manager. All time activities must be approved before the time card may be submitted for approval.";
		public const string HasPendingOrRejectedActivityOnRelease = "The time card includes one or multiple time activities that require approval by the project manager. The time card can be released after the time activities are approved.";
		public const string HasOpenAndPendingOrRejectedActivity = "The time card includes one or multiple time activities that have the Open status or require approval by project manager. All time activities must be completed and approved before the time card may be submitted for approval.";
		public const string HasInactiveProject = "There is one or more open activities referencing Inactive project. Please Activate Project to proceed with approval.";
		public const string Submit = "Submit";
		public const string Claim = "Claim";
		public const string ClaimAll = "Claim All";
		public const string SubmittedReceipt = "Submitted Receipt(s)";
		public const string AddNewReceipt = "Add New Receipt";
		public const string AddReceipts = "Add Receipts";
		public const string AddReceiptToolTip = "Add New Receipt";
		public const string ReleasedDocumentMayNotBeDeleted = "This document is released and can not be deleted.";
		public const string ReceiptMayNotBeDeleted = "This receipt is submitted and can not be deleted.";
		public const string ReceiptIsClaimed = "This receipt has already been claimed.";
		public const string ReceiptNotApproved = "This receipt must be approved.";
		public const string RemovedRejectedReceipt = "This receipt must be removed from the claim.";
		public const string ReceiptTakenOffHold = "This receipt must be taken off hold.";
		public const string ErrorProcessingReceipts = "One or multiple errors occurred during the processing of receipts.";
		public const string NotAllReceiptsOpenStatus = "All receipts included in the claim must be in the Open status.";
		public const string ApprovalMapCouldNotAssign = "No approver has been assigned to the document. The document is considered approved.";
		public const string NavigateToTheSelectedMap = "Navigate to the selected map";
		public const string AddNewApprovalMap = "Add New Approval Map";
		public const string AddNewAssignmentMap = "Add New Assignment Map";
		public const string Add = "Add";
		public const string Close = "Close";
		public const string SearchableTitleEmployee = "Employee: {0} {1}";
		public const string SearchableTitleExpenseClaim = "Expense Claim: {0} - {2}";
		public const string SearchableTitleExpenseReceipt = "Expense Receipt: {0} by {2}";
		public const string Employee = "Employee";
		public const string DirectEmployee = "Employee";
		public const string DocumentEmployee = "Employee from Document";
		public const string FilterEmployee = "Employees by Filter";
        public const string EmployeeInAssigmentAndApprovalMap = "This employee is used in at least one assignment map, approval map, or workgroup. (View the Assignment and Approval Maps tab for the list of the relevant maps or the Company Tree tab for the list of the workgroups.) Detaching the login from the employee may affect the assignment process and cause delays in document processing. Are you sure you want to detach the login?";
        public const string EmployeeInCompanyTree = "This employee is used in at least one workgroup in the company tree. (View the list of relevant workgroups on the Company Tree Info tab.) Detaching the login from the employee may affect the assignment process and cause delays in document processing. Are you sure you want to detach the login?";
		public const string FailedCreateContractUsageTransactions = "The system failed to create contract-usage transactions.";
		public const string FailedCreateCostTransactions = "The system failed to create cost transactions.";
		public const string InfiniteLoop = "An infinite loop occurred due to an incorrect week generation option.";
		public const string FailedSelectEquipment = "Equipment was not selected.";
		public const string RecordCannotDeleted = "The summary record cannot be deleted.";
		public const string NotPossibleProcessMessage = "The system cannot process this message.";
		public const string AutomationNotConfigured = "Automation for screen/graph {0} exists but is not configured properly. Failed to find action - 'Action'";
		public const string TypeCannotBeFound = "{0} type cannot be found.";
		public const string IsNotGraphSubclass = "{0} is not a graph subclass.";
		public const string Correction = "{0} - {1} correction";
		public const string SummaryRecord = "Summary {0} Record";
		public const string SummaryActivities = "Summary {0} Activities";
		public const string ViewProject = PM.Messages.ViewProject;
		public const string Subject = "Subject";
		public const string EventNumber = "Event Number";
		public const string ContactPerson = "Contact person";
		public const string Email = "Email";
		public const string Phone = "Phone";
		public const string CancelInvitationTo = "Cancel invitation to ";
		public const string RescheduleOf = "Reschedule of ";
		public const string InvitationTo = "Invitation to ";
		public const string EventWasCanceled = "Event was canceled.";
		public const string EventWasRescheduled = "Event was rescheduled.";
		public const string InvitedYouToAnEvent = "invited you to an event. ";
		public const string YouAreInvitedToAnEvent = "You are invited to an event.";
		public const string Location = "Location";
		public const string StartDate = "Start Date";
		public const string EndDate = "End Date";
		public const string Duration = "Duration";
		public const string HolidayDesc = "Holiday";
		public const string NormalizationDesc = "Normalization";
		public const string ChangeTaxZoneAsk = "Do you want to use the specified tax zone for expense receipts by default?";
		public const string NotPossibleChangeTaxSetting = "The {0} cannot be changed because some receipts were created on the Expense Receipt (EP301010) form. To change the {0} in those receipts, use the Expense Receipt form.";
		public const string TaxZoneText = "Tax Zone";
		public const string TaxCalcModeText = "Tax Calculation Mode";
		public const string ViewInvoice = CA.Messages.ViewInvoice;
		public const string TimeZoneCannotBeEmpty = "Time Zone cannot be empty.";

        public const string PersonalAccount = "Personal Account";
        public const string CorpCardCompanyExpense = "Corporate Card, Company Expense";
        public const string CorpCardPersonalExpense = "Corporate Card, Personal Expense";
	    public const string Receipt = "Receipt";

		public const string EmailFromFieldDoesntMatchAccountFromAddress = "The sender's email address does not match the email address specified in the From box. The SMTP server requires that the specified email address is similar to the email address of the authenticated user.";
		public const string ApprovalCategory = "Approval";
		public const string ManagementCategory = "Management";
		public const string OtherCategory = "Other";
		public const string WeekDateRangeException = "The specified date is outside the selected week.";
		[Obsolete]
		public const string EmployeeIsNotLinkedToUser = "The employee record is not associated with a user.";
		#endregion

		#region Not Traslatable Strings used in the code

		public const string CRActivityIsExpected = "Invalid cache type. CRActivity is expected.";

		#endregion

		#region DAC Names
		public const string EPContractRate = "Contract Rates";
		public const string Task = "Task";
		public const string Event = "Event";
		public const string EmployeeClass = "Employee Class";
		public const string CustomerVendorClass = "Customer/Vendor Class";
		public const string EmailRouting = "Route Email";
		public const string EPEmployeeRate = "Employee Rate";
		public const string EPEmployeeRateByProject = "Employee Rate by Projects";
		public const string TimeCard = "Time Card";
		public const string EquipmentTimeCard = "Equipment Time Card";
		public const string TimeCardSimple = "Simple";
		public const string TimeCardDefault = "Default";
		public const string TimeCardDetail = "Time Card Detail";
		public const string AddNewTimecardToolTip = "Add New Timecard";
		public const string EditTimecardToolTip = "Edit Timecard";
		public const string DeleteTimecardToolTip = "Delete Timecard";
		public const string EquipmentSummary = "Equipment Time Card Summary";
		public const string EquipmentDetail = "Equipment Time Card Detail";
		public const string DoNotSplit = "Do Not Split";
		public const string WeekEmployee = "Week, Employee";
		public const string ProjectEmployee = "Project, Employee";
		public const string WeekProject = "Week, Employee";
		public const string WeekProjectEmployee = "Week, Project, Employee";
		public const string Equipment = "Equipment";
		public const string EPSetup = "Time & Expenses Preferences";
		public const string ExpenseClaim = "Expense Claim";
		public const string ExpenseReceipt = "Expense Receipt";
		public const string TimeCardDocument = "Document";
		public const string TimeCardDetails = "Details";
		public const string TimeCardSummary = "Time Card Summary";
		public const string Department = "Department";
		public const string Wingman = "Delegate";
		public const string ActivityViewStatus = "Activity View Status";
		public const string TimeCardItem = "Time Card Item";
		public const string Position = "Position";
		
		
		
		public const string EventShowAs = "Event ShowAs";
		public const string EventCategory = "Event Category";
		public const string EmployeePosition = "Employee Position";
		public const string EmployeeContract = "Employee Contract";
		public const string EmployeeClassLabor = "Employee Class Labor";
		public const string CustomWeek = "Custom Week";
		public const string AttendeeMessage = "Attendee Message";
		public const string Attendee = "Attendee";
		public const string LegacyAssignmentRule = "Legacy Assignmnent Rule";
		public const string LegacyAssignmentRoute = "Legacy Assignment Route";
		public const string EPTax = "EP Tax Detail";
		public const string EPEquipmentRate = "Equipment Rate";
		public const string EPOtherAttendeeMessage = "Other Attendee Message";
	    public const string EmployeeCorpCardReference = "Employee Corporate Card Reference";
		public const string EPWeeklyCrewTimeActivity = "Weekly Crew Time Activity";
		public const string EPWeeklyCrewTimeActivityFilter = "Weekly Crew Time Activity Filter";
		public const string EPTimeActivitiesSummary = "Time Activities Summary";
		public const string EPShiftCode = "Shift Code";
		public const string EPShiftCodeRate = "Shift Code Rate";

		#endregion

		#region Field Names
		public const string ReportsTo = "Reports to";
		#endregion

		#region View Names

		public const string Events = "Events";
		public const string Folders = "Email Accounts";
		public const string Filter = "Filter";
		public const string Changeset = "Changeset";
		public const string ChangesetDetails = "Fields";
		public const string Activities = "Activities";
		public const string Timecards = "Time Cards";
		public const string ActivityType = "Activity Type";
		public const string ActivityTypes = "Activity Types";
		public const string Approval = "Approval";
		public const string Emails = "Emails";
		public const string EquipmentAnswers = "Equipment Answers";
		public const string Selection = "Selection";
		public const string EarningType = "Earning Type";
		public const string ClaimDetailsView = "ClaimDetails";
		public const string ExpenseClaimDetails = "Expense Claim Details";
		public const string EPRuleEmployeeCondition = "Assignment/Approval Rule Employee Condition";
		public const string EPRuleCondition = "Assignment/Approval Rule Condition";
		public const string EPRule = "Assignment/Approval Rule";
		public const string EmployeeCannotBeEmpty = "'Employee' cannot be empty";
        public const string EPEmployee = "Employee";
        public const string FinancialSettings = "Financial Settings";
        public const string Address = "Address";
        public const string Contact = "Contact";
        #endregion

        #region Statuses

        #region EP claim statuses
		public const string Balanced = "Pending Approval";
		public const string Voided = "Rejected";
		public const string Pending = "Pending";
		public const string Approved = "Approved";
		public const string Rejected = "Rejected";
		public const string Released = "Released";
		public const string Closed = "Closed";
		public const string NotRequired = "Not Required";
		public const string PartiallyApprove = "Partially";
		public const string PendingApproval = "Pending Approval";
		#endregion

		#region Invitations

		public const string InvitationNotInvited = "Not invited";
		public const string InvitationInvited = "Invited";
		public const string InvitationAccepted = "Accepted";
		public const string InvitationRejected = "Declined";
		public const string InvitationRescheduled = "Rescheduled";
		public const string InvitationCanceled = "Canceled";

		#endregion

		#endregion //Statuses

		#region EP Mask Codes
		public const string MaskItem = "Non-Stock Item";
		public const string MaskEmployee = "Employee";
		public const string MaskCompany = "Branch";
		#endregion

		#region Combo Values

		public const string Day = "Day";
		public const string Week = "Week";
		public const string Month = "Month";
		public const string Year = "Year";

		public const string Sunday = "Sunday";
		public const string Monday = "Monday";
		public const string Tuesday = "Tuesday";
		public const string Wednesday = "Wednesday";
		public const string Thursday = "Thursday";
		public const string Friday = "Friday";
		public const string Saturday = "Saturday";

		public const string CompleteTask = "Complete";
		public const string CancelTask = "Cancel";

		public const string PreProcess = "Pending Processing";
		public const string InProcess = "Processing";
		public const string Scheduled = "Scheduled";
		public const string Processed = "Processed";
		public const string Waiting = "Waiting";
		public const string EmailDeleted = "Deleted";
		public const string EmailArchived = "Archived";

		public const string Success = "Success";
		public const string Error = "Error";
		public const string Deleted = "Deleted";

		public const string Biweekly = "Semiweekly";
		public const string Weekly = "Weekly";
		public const string Semimonthly = "Semimonthly";
		public const string Monthly = "Monthly";

		public const string Hourly = "Hourly";
		public const string Salary = "Annual Non-Exempt";
		public const string SalaryWithExemption = "Annual Exempt";

		
		public const string Validate = "Validate";
		public const string WarningOnly = "Warning Only";
		public const string None = "None";

		public const string SelectedCustomer = "Lines with selected customer";
		public const string AllLines = "All lines";
		public const string Nothing = "Nothing";

		//Start Reason
		public const string New = "New Hire";
		public const string Rehire = "Rehire";
		public const string Promotion = "Promotion";
		public const string Demotion = "Demotion";
		public const string NewSkills = "New Skills";
		public const string Reorganization = "Reorganization";
		public const string Other = "Other";

		//TermReason
		public const string Retirement = "Retirement";
		public const string Layoff = "Layoff";
		public const string TerminatedForCause = "Terminated for Cause";
		public const string Resignation = "Resignation";
		public const string Deceased = "Deceased";
		public const string Disabled = "Disabled";
		public const string MedicalIssues = "Medical Issues";

		//DefaultDateInActivity
		public const string LastDay = "Last Day Entered";
		public const string NextWorkDay = "Next Work Day";

		public const string Post_PostingOption = "Post PM and GL Transactions";
		public const string DoNotPost_PostingOption = "Do not Post";
		public const string PostToOffBalance_PostingOption = "Post PM to Off-Balance Account Group";

		public const string ApprovalReasonIsRequired = "Is Required";
		public const string ApprovalReasonIsOptional = "Is Optional";
		public const string ApprovalReasonIsNotPrompted = "Is Not Prompted";

		public const string TimecardTypeNormal = "Normal";
		public const string TimecardTypeCorrection = "Correction";
		public const string TimecardTypeNormalCorrected = "Normal Corrected";

		#endregion

		#region Filter Name

		public const string Today = "Today";
		public const string ThisWeek = "This Week";
		public const string ThisMonth = "This Month";
		public const string NextWeek = "Next Week";
		public const string NextMonth = "Next Month";
		#endregion

		#region EP Expense Receipts Status
		public const string ApprovedStatus = "Open";
		public const string HoldStatus = "On Hold";
		public const string ReleasedStatus = "Released";
		public const string OpenStatus = "Pending Approval";
		public const string RejectedStatus = "Rejected";
		#endregion

		#region WorkgroupMemberStatusAttribute labels

		public const string PermanentActive = "Permanent";
		public const string PermanentInactive = "Permanent - Inactive";
		public const string TemporaryActive = "Temporary";
		public const string TemporaryInactive = "Temporary - Inactive";
		public const string Adhoc = "Ad Hoc";

		#endregion WorkgroupMemberStatusAttribute labels

		#region Crew Time Entry

		public const string DateTimeField = "Date/Time";
		public const string CantDeleteWithActivities = "This employee has time activities reported for the selected week and cannot be removed from the list.";
		public const string InactiveMemberTimeEntry = "This employee is inactive.";
		public const string WithoutActivities = "At least one of the listed employees has no time activities reported for the selected period.";


		#endregion Crew Time Entry

		#region EPShiftCodes
		public const string Amount = "Amount";
		public const string Percent = "Percent";
		public const string CostingTooLow = "The costing amount cannot be less than the wage amount.";
		public const string WageTooHigh = "The wage amount cannot be greater than the costing amount.";
		public const string DateMustBeLatest = "The shift code is already in use and no effective date can be added before {0}.";
		public const string ShiftCodeNotActive = "The shift code is not active.";
		public const string ShiftCodeNotEffective = "The shift code is not active or doesn't have any effective rates.";
		public const string ShiftAmountNameWithPayroll = "Costing Amount";
		public const string ShiftAmountNameWithoutPayroll = "Amount";
		#endregion EPShiftCodes

		#region EmploymentHistory

		public const string HistoryHasFinalPayment = "This record can't be deleted because it is associated with a final payment.";

		#endregion EmploymentHistory

		#region BankFeed
		public const string PostedBankTran = "Posted";
		public const string PendingBankTran = "Pending";
		#endregion
	}


	[PXLocalizable]
	public static class MessagesNoPrefix
	{
		public const string ApprovalRecordNotFound = "Record for approving not found.";

		public const string ReassignmentOfApprovalsNotSupported = "Reassignment of approvals is supported only for maps of the Approval Map type.";
		public const string ReassignmentNotAllowed = "The approval request cannot be reassigned because reassignment of approvals is not allowed in the approval map rule.";
		public const string ReassignmentApproverNotAvailable = "The selected approver or their delegates are not available for the specified period. Select another approver.";
		public const string ReassignmentDelegateNotAvailable = "The delegate or their delegates are not available. The approval was not delegated.";

		public const string DelegateStartsOnDateInThePast = "The Starts On date cannot be in the past.";
		public const string DelegateStartsExpiresOnDatesInsideExistingPeriod = "The date is within the period specified for one of the existing delegations. The delegation periods cannot intersect.";
		public const string DelegateExpiresOnDateBeforeStartsOn = "The Expires On date cannot be before the Starts On date.";
	}

}
