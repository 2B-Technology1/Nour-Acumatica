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

namespace PX.Objects.PR
{
	[PXLocalizable(Messages.Prefix)]
	public static class Messages
	{
		public const string Prefix = "PR Error";

		#region GLAccountSource List
		public const string BenefitAndDeductionCode = "Benefit & Deduction Code";
		public const string Branch = "Branch";
		public const string CompanyDefault = "Company Default";
		public const string Department = "Department";
		public const string EarningType = "Earning Type";
		public const string EmployeeClass = "Employee Class";
		public const string JobCode = "Job Code";
		public const string Location = "Location";
		public const string Position = "Position";
		public const string ShiftCode = "Shift Code";
		public const string TaxCode = "Tax Code";
		public const string Union = "Union";
		public const string PTOBank = "PTO Bank";
		#endregion

		#region Jurisdiction List
		public const string Federal = "Federal";
		public const string State = "State";
		public const string Local = "Local";
		public const string Municipal = "Municipal";
		public const string SchoolDistrict = "School District";
		public const string UnknownJurisdiction = "<Unknown>";
		#endregion

		#region Tax Category List
		public const string EmployerTax = "Employer Tax";
		public const string EmployeeWithholding = "Employee Withholding";
		public const string UnknownTaxCategory = "<Unknown>";
		#endregion

		#region Wage Base Inclusion List
		public const string AllWages = "All Wages";
		public const string StateSpecific = "State Specific";
		public const string LocationSpecific = "Location Specific";
		#endregion

		#region Subject To Taxes List
		public const string All = "All";
		public const string AllButList = "All but Listed Below";
		public const string NoneButList = "None but Listed Below";
		#endregion

		#region Contribution Subject To Taxes List
		public const string AllCSTT = "All";
		public const string NoneCSTT = "None";
		public const string PerTaxEngine = "Per Tax Engine";
		public const string FromListCSTT = "From List Below";
		public const string DeductionPerTaxEngine = "Calculated by Tax Engine";
		public const string ReducedByDeduction = "Reduced by Deduction Except Listed Below";
		public const string IncreasedByContribution = "Increased by Contribution Except Listed Below";
		#endregion

		#region Deduction/Contribution Calculation Method List
		public const string FixedAmount = "Fixed Amount";
		public const string PercentOfGross = "Percent of Gross";
		public const string PercentOfNet = "Percent of Net";
		public const string AmountPerHour = "Amount per Hour";
		public const string PercentOfCustom = "Percent of Custom";
		#endregion

		#region Unit Type List
		public const string Hour = "Hour";
		public const string Year = "Year";
		public const string Misc = "Miscellaneous";
		#endregion

		#region Invoice Description Type List
		public const string Code = "Code";
		public const string CodeName = "Code Name";
		public const string CodeAndCodeName = "Code + Code Name";
		public const string EmployeeGarnishDescription = "Employee Garnish Description";
		public const string EmployeeGarnishDescriptionPlusPaymentDate = "Employee Garnish Descr + Payment Date";
		public const string PaymentDate = "Payment Date";
		public const string PaymentDateAndCode = "Payment Date + Code";
		public const string PaymentDateAndCodeName = "Payment Date + Code Name";
		public const string FreeFormatEntry = "<Free Format Entry>";
		#endregion

		#region Deduction Max Frequency Type List
		public const string NoMaximum = "No Maximum";
		public const string PerPayPeriod = "Per Pay Period";
		public const string PerCalendarYear = "Per Calendar Year";
		#endregion

		#region Contribution Type List
		public const string EmployeeDeduction = "Employee Deduction";
		public const string EmployerContribution = "Employer Contribution";
		public const string BothDeductionAndContribution = "Both Deduction & Contribution";
		#endregion

		#region Income Calculation Type List
		public const string SpecificList = "Specific List";
		public const string Formula = "Formula";
		#endregion

		#region Deduction Split Type List
		public const string Even = "Even";
		public const string ProRata = "Pro-Rata";
		#endregion

		#region Employee Type List
		public const string SalariedExempt = "Salaried Exempt";
		public const string SalariedNonExempt = "Salaried Non-Exempt";
		public const string Hourly = "Hourly";
		public const string Other = "Other";
		#endregion

		#region Bank Account Type List
		public const string Checking = "Checking";
		public const string Savings = "Savings";
		#endregion

		#region Bank Account Status Type List
		public const string Active = "Active";
		public const string Inactive = "Inactive";
		#endregion

		#region Batch Status Type List
		public const string Open = "Open";
		public const string Created = "Created";
		public const string Released = "Released";
		#endregion

		#region Pay Periods
		public const string Weekly = "Weekly";
		public const string Biweekly = "Biweekly";
		public const string Semimonthly = "Semimonthly";
		public const string Monthly = "Monthly";
		public const string BiMonthly = "Bimonthly";
		public const string Quarterly = "Quarterly";
		public const string ThirteenPerYear = "13 Pay Periods a Year";

		public const string ShiftedPeriodDescrFormat = "Shifted {0} from {1}";
		#endregion

		#region PRPayment Status List
		public const string NeedCalculation = "Pending Calculation";
		public const string PaymentBatchCreated = "Added to Payment Batch";
		#endregion

		#region Payment Type List
		public const string Regular = "Regular";
		public const string Special = "Special";
		public const string Adjustment = "Adjustment";
		public const string VoidCheck = "Voiding Paycheck";
		public const string Final = "Final";
		#endregion

		#region Payment Status Type List
		public const string OpenPST = "Open";
		public const string CalculatedPST = "Calculated";
		public const string RequiresRecalculationPST = "Requires Recalculation";
		public const string DirectDepositExportedPST = "Direct Deposit Exported";
		public const string ReleasedPST = "Released";
		public const string Void = "Void Paycheck";
		public const string Voided = "Voided";
		public const string ReprintedCheck = "Reprinted Check";
		public const string Hold = "On Hold";
		public const string LiabilityPartiallyPaid = "Liability Partially Paid";
		public const string Closed = "Closed";
		public const string PendingPayment = "Pending Payment";
		#endregion

		#region Record Of Employment Status Type List
		public const string OpenROE = "Open";
		public const string Exported = "Exported";
		public const string Submitted = "Submitted";
		public const string NeedsAmendment = "Needs Amendment";
		public const string Amended = "Amended";
		#endregion

		#region Record Of Employment Reason Type List
		public const string A00 = "A - Shortage of Work/End of contract or season";
		public const string A01 = "A - Employer bankruptcy or receivership";
		public const string B00 = "B - Strike or lockout";
		public const string D00 = "D - Illness or injury";
		public const string E00 = "E - Quit";
		public const string E02 = "E - Quit / Follow spouse";
		public const string E03 = "E - Quit / Return to school";
		public const string E04 = "E - Quit / Health reasons";
		public const string E05 = "E - Quit / Voluntary retirement";
		public const string E06 = "E - Quit / Take another job";
		public const string E09 = "E - Quit / Employer relocation";
		public const string E10 = "E - Quit / Care for a dependant";
		public const string E11 = "E - Quit / To become self-employed";
		public const string F00 = "F - Maternity";
		public const string G00 = "G - Mandatory retirement";
		public const string G07 = "G - Retirement / Approved workforce reduction";
		public const string H00 = "H - Work Sharing";
		public const string J00 = "J - Apprenticeship training";
		public const string K00 = "K - Other";
		public const string K12 = "K - Other / Change of payroll frequency";
		public const string K13 = "K - Other / Change of ownership";
		public const string K14 = "K - Other / Requested by Employment Insurance";
		public const string K15 = "K - Other / Canadian Forces - Queen's Regulations/Orders";
		public const string K16 = "K - Other / At the employee's request";
		public const string K17 = "K - Other / Change of Service Provider";
		public const string M00 = "M - Dismissal";
		public const string M08 = "M - Dismissal / Terminated within a probationary period";
		public const string N00 = "N - Leave of Absence";
		public const string P00 = "P - Parental";
		public const string Z00 = "Z - Compassionate Care";
		#endregion

		#region Record Of Employment block field headers
		public const string EmployerNameB4 = "Employer Name (Block 4)";
		public const string AddressLine1B4 = "Address Line 1 (Block 4)";
		public const string AddressLine2B4 = "Address Line 2 (Block 4)";
		public const string CityB4 = "City (Block 4)";
		public const string CountryB4 = "Country (Block 4)";
		public const string StateB4 = "State (Block 4)";
		public const string PostalCodeB4 = "Postal Code (Block 4)";
		public const string ROERefNbr = "Reference Nbr.";
		#endregion

		#region Batch For Submission Types
		public const string Original = "Original";
		public const string Amendment = "Amendment";
		public const string Cancellation = "Cancellation";
		#endregion

		#region Batch For Submission Statuses
		public const string AllPublished = "All Published";
		public const string SomePublished = "Some Published";
		public const string NonePublished = "None Published";
		#endregion

		#region Check Reprint Type List
		public const string PrinterIssue = "Failed to Print";
		public const string Lost = "Lost";
		public const string Damaged = "Damaged";
		public const string Stolen = "Stolen";
		public const string Corrected = "Corrected";
		public const string AddedPaycheck = "Added paycheck";

		#endregion

		#region Check Void Type List
		public const string PrinterIssueCVT = "Printer Issue";
		public const string LostCVT = "Lost";
		public const string DamagedCVT = "Damaged";
		public const string StolenCVT = "Stolen";
		public const string ErrorCorrection = "Error Correction";
		#endregion

		#region Required Calculation Level List
		public const string CalculateAll = "Calculate All";
		public const string CalculateTaxesAndNetPay = "Calculate Taxes and Net Pay";
		public const string CalculateNetPayOnly = "Calculate Net Pay Only";
		public const string NoCalculationRequired = "No Calculation Required";
		#endregion

		#region Earning Detail Source Type List

		public const string TimeActivity = "Time Activity";
		public const string QuickPay = "Quick Pay";
		public const string SalesCommission = "Sales Commission";

		#endregion

		#region Batch Status Type List
		public const string Balanced = "Balanced";
		#endregion

		#region Carryover Type List
		public const string Partial = "Partial";
		public const string Total = "Total";
		public const string PaidOnTimeLimit = "Paid After a Period of Time";
		#endregion Carryover Type List

		#region Overtime Rule Type List
		public const string DailyRule = "Daily";
		public const string WeeklyRule = "Weekly";
		public const string ConsecutiveRule = "Consecutive";
		#endregion

		#region Earning Type List
		public const string Wage = "Wage";
		public const string Overtime = "Overtime";
		public const string Piecework = "Piecework";
		public const string AmountBased = "Amount-Based";
		public const string TimeOff = "Time Off";
		#endregion

		#region Overtime Rule Warnings
		public const string WeeklyOvertimeRulesApplyToWeeklyPeriods = "Weekly Overtime Rules can be applied to Weekly and Biweekly pay periods only.";
		public const string InconsistentBaseEarningDetailRecord = "Inconsistent Base Earning Detail record (ID: {0}) for current calculated Earning Detail record (ID: {1}).";
		#endregion

		#region Error Messages
		public const string SegmentNotSupportedInMaskForAccountType = "Segment {0} can't be used for masks of this account type. Allowed values are: {1}";
		public const string AddressNotRecognized = "The tax calculation service does not recognize the address specified for {0}. Make sure that the address is complete and valid.";
		public const string EmployeeAddressNotRecognized = "The tax calculation service does not recognize the address information specified for at least one of the selected employees. Make sure that each employee has a complete and valid address specified on the General tab of the Employee Payroll Settings form. See Trace for the list of problematic employee records.";
		public const string ValueBlankAndRequiredAndNotOverridable = "The Value box cannot be empty for a setting that has the Required check box selected and the Allow Employee Override check box cleared.";
		public const string ValueBlankAndRequired = "The Value box cannot be empty for a setting that has the Required check box selected.";
		public const string TaxSettingNotFound = "The {0} setting is not configured for the parent entity.";
		public const string StartDateMustBeLessOrEqualToTheEndDate = "Start date must be less or equal to the end date.";
		public const string NoTaxCodeExistForTaxType = "No tax code is setup for tax type {0}.";
		public const string AtLeastOneRemainderDD = "At least one active Direct Deposit must have 'Gets Remainder' checked";
		public const string AccountNotAPayrollEmployee = "This account is not a payroll employee.";
		public const string DeductionCodeIsInUseAndCantBeDeleted = "This deduction code is in use and can't be deleted.";
		public const string EmployeeClassIsInUseAndCantBeDeleted = "This employee class is in use and can't be deleted.";
		public const string IncomeCodeIsInUseAndCantBeDeleted = "This income code is in use and can't be deleted.";
		public const string PayGroupIsInUseAndCantBeDeleted = "This pay group is in use and can't be deleted.";
		public const string TaxTypeIsNotImplemented = "Tax Type with with ID {0} is not implemented.";
		public const string CantGetPrimaryView = IN.Messages.CantGetPrimaryView;
		public const string Document_Status_Invalid = AP.Messages.Document_Status_Invalid;
		public const string CannotReleaseInInitializationMode = "Cannot release PR transaction to GL in Initialization Mode.";
		public const string PayGroupPeriodsDefined = "You cannot delete the Payroll Year Setup because one or more periods for the pay group exist in the system.";
		public const string DeleteGeneratedPeriods = "Delete Pay Group Periods";
		public const string CalendarMonthlyTransactionsDateOutOfRange = "Transactions Start Date must occur during the first month of the year.";
		public const string CalendarBiMonthlyTransactionsDateOutOfRange = "Transactions Start Date must occur during the two first month of the year.";
		public const string CalendarQuarterTransactionsDateOutOfRange = "Transactions Start Date must occur during the first quarter of the year.";
		public const string CalendarCustomTransactionsDateOutOfRange = "Transactions Start Date must occur before {0:D}.";
		public const string CalendarTransactionsDateOtherYear = "Transactions Start Date must occur during the payroll year.";
		public const string CalendarSemiMonthlySecondPeriodsOutOfRange = "Second Period must start 10 to 20 after the first one.";
		public const string CalendarSemiMonthlySecondTransactionsOutOfRange = "Second Transaction Date must be 10 to 20 after the first one.";
		public const string NonZeroYTDAmounts = "This row contain non-zero YTD amounts and can not be deleted.";
		public const string DuplicateDeductionCode = "You can not add duplicate deduction codes.";
		public const string AccountCantBeEmpty = "Account can not be empty.";
		public const string SubaccountCantBeEmpty = "Subaccount can not be empty.";
		public const string DedCalculationErrorFormat = "Deduction amount must be set for {0} deduction.";
		public const string CntCalculationErrorFormat = "Contribution amount must be set for {0} contribution.";
		public const string DuplicateEmployeeDeduct = "The deduction and benefit code has already been added for the same start date or end date, or for both dates. You can change the start date to have the settings applied later, or delete the record.";
		public const string InconsistentDeductDate = "Deduction End Date can't be before Deduction Start Date (Start Date: {0}, End Date: {1})";
		public const string DuplicateEmployeeEarning = "Duplicate Earning Definition. This line overlaps with another Earning (Start Date: {0}, End Date: {1})";
		public const string InconsistentEarningDate = "Earning End Date can't be before Earning Start Date (Start Date: {0}, End Date: {1})";
		public const string InvalidNegative = "Value cannot be negative.";
		public const string DeductionMaxLimitExceededWarn = "{0} maximum for deduction has been {1}.";
		public const string BenefitMaxLimitExceededWarn = "{0} maximum for contribution has been {1}.";
		public const string Reached = "reached";
		public const string Exceeded = "exceeded";
		public const string DeductionCausesNetPayBelowMin = "Deduction amount causes net pay to go below Net Pay Minimum.";
		public const string DeductionAdjustedForNetPayMin = "Calculated deduction amount has been adjusted to respect Net Pay Minimum.";
		public const string GarnishmentCausesPercentOfNetAboveMax = "Overridden garnishment amount causes total garnishment amount to go above Maximum Percent of Net Pay for All Garnishments.";
		public const string GarnishmentAdjustedForPercentOfNetMax = "Calculated garnishment amount has been adjusted to respect Maximum Percent of Net Pay for All Garnishments.";
		public const string CalculationEngineError = "An error occurred during the calculation of the deductions, benefits, and taxes. Payment {0} has not been found.";
		public const string DeductionAdjustedByTaxEngine = "Deduction amount has been adjusted by tax engine.";
		public const string BenefitAdjustedByTaxEngine = "Benefit amount has been adjusted by tax engine.";
		public const string DuplicateEarningType = "Earning type is already used with another bank.";
		public const string CantUseFieldAsState = "Can't use field {0} as a filter for state.";
		public const string CantBeEmpty = "{0} can not be empty.";
		public const string PostingValueNotFound = "{0} value wasn't found automatically and can not be empty. Verify your posting settings in Payroll Preferences or set it manually.";
		public const string InvalidPayPeriod = "Invalid pay period number";
		public const string PTOUsedExceedsAvailable = "PTO used exceeds available amount.";
		public const string CantFindAatrixEndpoint = "Can't find Aatrix endpoint for requested operation.";
		public const string AatrixOperationTimeout = "Aatrix operation timed out (timeout = {0} ms).";
		public const string AatrixReportProcessingError = "Error while trying to run the Aatrix report.";
		public const string AatrixReportEinMissing = "The EIN is not specified. If you fill taxes by branches, you need to specify the Tax Registration ID setting on the Branches (CS102000) form. Otherwise, you need to specify the Tax Registration ID setting on the Companies (CS101500) form.";
		public const string AatrixReportAatrixVendorIDMissing = "Aatrix Vendor ID not set.";
		public const string AatrixReportFormNameMissing = "Form name not selected.";
		public const string AatrixReportCompanyNameMissing = "Company Name not set.";
		public const string AatrixReportFirstNameMissing = "First Name is not specified for employee {0}. Specify a first name for the employee on the Employees (EP203000) form.";
		public const string AatrixReportLastNameMissing = "Last Name not set for employee {0}.";
		public const string AatrixReportInvalidSsn = "A wrong social security number is specified for employee {0} ({1}). Specify a proper social security number on the Tax Settings tab of the Employee Payroll Settings (PR203000) form.";
		public const string AatrixReportSsnNotSet = "Social security number not set for employee {0}. Go to Employee Payroll Settings form and under the tax settings tab update the Social Security Number.";
		public const string AatrixReportMissingAufInfo = "Can't generate AUF file. Required information in unavailable.";
		public const string UnsupportedTaxJurisdictionLevel = "Tax jurisdiction level {0} is not supported. You need to update tax information by clicking Update Taxes on the Tax Maintenance (PR208000) form.";
		public const string TaxCodeNotSetUp = "The tax {0} should be assigned and active on the Employee Payroll Settings (PR203000) form under the Taxes tab.";
		public const string CantParseYear = "Can't parse year from string \"{0}\"";
		public const string UnknownOfferOfCoverageCode = "Unknown Offer of Coverage \"{0}\"";
		public const string AcaEinMissing = "The tax registration number is missing. It can be specified on the Branches (CS102000) or Companies (CS101500) form depending on whether your company is filing taxes by branches.";
		public const string CannotDeactivatePieceworkEarningType = "Cannot deactivate \"Piecework as an Earning Type\". The \"{0}\" Earning Type is already selected as a Piecework Type.";
		public const string CannotDeactivatePieceworkEarningTypeEmployee = "Cannot deactivate \"Piecework as an Earning Type\". The \"{0}\" Unit of Pay is assigned to employee: {1}.";
		public const string CannotDeactivatePieceworkEarningTypePayment = "Cannot deactivate \"Piecework as an Earning Type\". The \"{0}\" Unit of Pay is assigned to payment: {1}.";
		public const string CannotChangeFieldStateEarningType = "The {0} setting cannot be modified because the {1} earning type is assigned as a regular time type for the {2} earning type.";
		public const string CannotChangeFieldStateEmployee = "The {0} setting cannot be modified because the {1} earning type is assigned to the {2} employee.";
		public const string CannotChangeFieldStatePayment = "The {0} setting cannot be modified because the {1} earning type is used in the {2} payment.";
		public const string CannotChangeFieldStatePayrollBatch = "The {0} setting cannot be modified because the {1} earning type is used in the {2} payroll batch.";
		public const string CannotChangeFieldStateSetup = "The {0} setting cannot be modified because the {1} earning type is used in the {2} payroll setting.";
		public const string CannotChangeFieldStateOvertime = "The {0} setting cannot be modified because the {1} earning type is used in the {2} overtime rule.";
		public const string CannotChangeFieldStatePTOBanks = "The {0} setting cannot be modified because the {1} earning type is used in the {2} PTO bank.";
		public const string EarningTypeIsNotActive = "'{0}' Earning Type is not active. Choose another Earning Type.";
		public const string CannotModifyUpdateGLSetting = "The state of the Update GL check box cannot be changed because there are unreleased paychecks in the system.";
		public const string RequiredCheckNumber = "Paycheck Nbr. cannot be empty. Manually enter the paycheck number or specify a value in the AP/PR Last Reference Number box on the Payment Methods (CA204000) form.";
		public const string CantCalculateNegative = "Cannot calculate a paycheck with negative earnings details, deductions, or benefits. Either release the paycheck without calculating it or delete the negative amount from the paycheck before calculating it.";
		public const string EarningDetailsWithInactiveEarningTypes = "Paycheck contains Earning Details with inactive Earning Type: '{0}'.";
		public const string DuplicateExceptionDate = "Cannot insert duplicate Transaction Exception Date";
		public const string CantCreatePayGroupYear = "Cannot create payroll year {0} for pay group {1}.";
		public const string CantCreatePayGroupPeriodNumber = "Cannot generate payroll period number for pay group {0}.";
		public const string TotalOver100Pct = "Total is over 100%.";
		public const string NoBankAccountForDirectDeposit = "At least one bank account is required if the employee is paid via direct deposit. Enter the bank account information on the Employee Payroll Settings (PR203000) form under the Payment tab in the Direct Deposit grid.";
		public const string BatchContainsVoidedPayments = "This batch contains voided payments and can't be released.";
		public const string OneBasedDayOfWeekIncorrectValue = "One-based numbering is used for the days of the week but the range of the day numbers is wrong, which may indicate data corruption. Contact your system administrator.";
		public const string CantReleasePaymentWithoutGrossPay = "Payment can't be released without a gross pay.";
		public const string ProjectTaskIsNotActive = "The payment cannot be released because the {0} task of the {1} project is not active. Either cancel the payment on the Payment Batches (PR305000) form and specify another task or activate the task on the Project Tasks (PM302000) form.";
		public const string IncorrectPeriodDate = "The entered date should be in the range between {0} and {1}.";
		public const string OutOfPeriodDateError = "This date cannot be outside the specified pay period.";
		public const string IncorrectPeriodDateWithEnteredDate = "The entered date should be in the range between {0} and {1}. Entered date: {2}.";
		public const string YearNotSetUp = "Payroll year {0} has not been set up.";
		public const string TaxDefinitionAssemblyInvalidOrNotFound = "The taxes or tax settings were not properly loaded. You need to update tax information by clicking Update Taxes on the Tax Maintenance (PR208000) form.";
		public const string PayrollCalendarNotSetUp = "Payroll calendar has not been set up for pay group {0}.";
		public const string SameBillRecordError = "A record for the same bill couldn't be processed.";
		public const string VendorRequired = "Vendor is required.";
		public const string VendorRequiredForDedBen = "Specify a vendor for the deduction and benefit code by using the Deduction and Benefit Codes (PR101060) form.";
		public const string VendorRequiredForTax = "Specify a vendor for the tax code in the Vendor column on the Tax Codes tab of the Tax Maintenance (PR208000) form.";
		public const string DeleteEmployeePayrollSettings = "An employee with an existing payroll record cannot be deleted. First you need to delete the record on the Employee Payroll Settings (PR203000) form.";
		public const string AttributeKeysInvalid = "{0} doesn't have valid keys defined. Verify the following attribute parameters : {1}, {2}. Check if both key arrays are not empty and have the same number of keys.";
		public const string InactivePREmployee = "Employee '{0}' is not active in Employee Payroll Settings (PR203000).";
		public const string OlderPaymentIsUnreleased = "A payment with an earlier transaction date exists for employee {0} and is not released. Release or delete payment {1} to continue.";
		public const string CannotCalculateBecauseOlderPaymentIsUnreleased = "Another paycheck {0} with the same or earlier pay period has already been created for the employee. Release it before calculating this paycheck.";
		public const string CannotReleaseBecauseOlderPaymentIsUnreleased = "Another paycheck {0} with the same or earlier pay period has already been created for the employee. Release it before releasing this paycheck.";
		public const string CantFindPostingPeriod = "Cannot find an open posting period for pay period {0}.";
		public const string FieldCantBeNull = "Field cannot be empty.";
		public const string CantAssignCostToLaborItemAndEarningType = "Cannot assign payroll cost to both Labor Item and Earning Type";
		public const string InactiveEPEmployee = "Employee '{0}' is not marked as Active on the Employees (EP203000) form.";
		public const string CantDuplicateDeductionDetail = "Cannot duplicate deduction detail record.";
		public const string CantDuplicateBenefitDetail = "Cannot duplicate benefit detail record.";
		public const string CantDuplicateTaxDetail = "Cannot duplicate tax detail record.";
		public const string CantDuplicatePTODetail = "A PTO detail record cannot be duplicated.";
		public const string DeductionDetailSumDoesntMatchProject = "The sum of deduction details for {0} must equal the sum of the certified project packages of {1:0.00}.";
		public const string DeductionDetailSumDoesntMatchUnion = "The sum of deduction details for {0} must equal the sum of the union packages of {1:0.00}.";
		public const string DeductionDetailSumDoesntMatchWC = "The sum of deduction details for {0} must equal the sum of the workers' compensation packages of {1:0.00}.";
		public const string BenefitDetailSumDoesntMatchProject = "The sum of benefit details for {0} must equal the sum of the certified project packages of {1:0.00} and the fringe rate amount in benefit of {2:0.00}.";
		public const string BenefitDetailSumDoesntMatchUnion = "The sum of benefit details for {0} must equal the sum of the union packages of {1:0.00}.";
		public const string BenefitDetailSumDoesntMatchWC = "The sum of benefit details for {0} must equal the sum of the workers' compensation packages of {1:0.00}.";
		public const string BenefitSummarySumDoesntMatchProject = "The benefit summary for {0} must equal the sum of the certified project packages of {1:0.00} and the fringe rate amount in benefit of {2:0.00}.";
		public const string BenefitSummarySumDoesntMatchUnion = "The benefit summary for {0} must equal the sum of the union packages of {1:0.00}.";
		public const string TaxDetailSumDoesntMatch = "The sum of tax details for {0} must equal the sum of the tax splits of {1:0.00}.";
		public const string EarningTypeAndLaborItemInAcctSub = "Earning Type and Labor Item cannot be both present in the account and subaccount settings for '{0}' and '{1}'.";
		public const string AdjustmentDetailsWillBeDeleted = "Changing this setting will delete {0} Details for one or more adjustment paychecks. Details will need to be recreated manually or paychecks will have to be Calculated. See trace for details.";
		public const string AdjustmentListWithDeletedDetails = "List of Adjustment paychecks with {0} Details deleted:";
		public const string BankNotFound = "Could not get the PTO bank history because the source bank couldn't be found.";
		public const string InvalidBankStartDate = "The system cannot retrieve the PTO bank history because the start date of the PTO bank is missing. Make sure that a valid start date is specified for the PTO bank on the PTO Banks (PR204000) form.";
		public const string InactivePTOBank = "The {0} PTO bank is not marked as Active on the PTO Banks (PR204000) form.";
		public const string CantContactWebservice = "The system cannot connect to the tax calculation service because there is no connection to the Internet or because the service is temporarily unavailable. Try updating taxes again later.";
		public const string PeriodsNotGenerated = "No calendar periods have been found. Click Create Periods to generate calendar periods before saving this calendar.";
		public const string DeductCodeInUse = "This deduction and benefit code cannot be edited because it is already in use.";
		public const string PercentOfNetInCertifiedProject = "The Percent of Net calculation method is not supported for certified projects.";
		public const string PercentOfNetInUnion = "The Percent of Net calculation method is not supported for union locals.";
		public const string LocationNotSetInEmployee = "Work location {0} is not assigned to employee {1}.";
		public const string LocationNotSetInEmployeeClass = "Work location {0} is not assigned to employee class {1}.";
		public const string LocationIsInactive = "The {0} work location is not active.";
		public const string CantAssignTaxesToEmployee = "Because of missing settings, an error occurred while taxes were being assigned to employees. Click Import Taxes on the Taxes tab of the Employee Payroll Settings (PR203000) form to see which settings need to be updated.";
		public const string RequiredTaxSettingNullInCalculate = "Required tax settings are missing for the employee. Make sure that all required settings are specified for the employee on the Tax Settings and Taxes tabs of the Employee Payroll Settings (PR203000) form.";
		public const string TaxUpdateNeeded = "New tax information is available. To update tax definitions, click Update Taxes on the Tax Maintenance form.";
		public const string EmployeeClassHasNoWorkLocation = "No work location is specified for the employee class.";
		public const string EmployeeHasNoTaxes = "Tax settings need to be specified on the Employee Payroll Settings (PR203000) form.";
		public const string CancelPrintedCheckWarning = "This action will put this check back to its pre-print status.";
		public const string RemoveFromDirectDepositWarning = "This action will put this check back to its pre-print status and remove it from its direct deposit batch.";
		public const string CantUseSimultaneously = "The Allow Negative Balance and Can Only Disburse from Carryover check boxes cannot be both selected.";
		public const string NotEnoughLastYearCarryover = "There are not enough hours left from last year's carryover of PTO Bank '{0}'.";
		public const string NotEnoughLastYearCarryoverMoney = "There is not enough money left from the last year's carryover of the {0} PTO bank.";
		public const string NotEnoughPTOAvailable = "A negative balance is not allowed. Hours associated with earning code '{0}' can't exceed available hours in PTO Bank '{1}'.";
		public const string NotEnoughPTOMoneyAvailable = "A PTO bank cannot have a negative balance. The amount associated with the {0} earning code cannot exceed the available amount in the {1} PTO Bank.";
		public const string CarryoverPaidWithThisEarningLine = "This earning line was created to pay remaining carryover hours.";
		public const string NegativePTOBalanceError = "Can't disallow negative balances because the following employees already have a negative balance {0}.";
		public const string DeductsWillBeRemoved = " This action will also delete deductions (summaries and details), certified project packages, union packages and workers' compensation packages for which the associated deduction and benefit code's source doesn't match.";
		public const string PressOK = " Click OK to proceed.";
		public const string PaychecksNeedRecalculationSeeTrace = "Some paychecks will need to be calculated to apply the change. See the trace for details.";
		public const string PaychecksNeedRecalculationFormat = "Some paychecks will need to be calculated to be affected by the change : {0}";
		public const string NoAccessToAllEmployeesInDDBatch = "You are not permitted to see the information related to at least one employee included in this batch.";
		public const string UnspecifiedCalculationEngineError = "An unspecified error occurred during the calculation of deductions, benefits and taxes. Please try calculating the paycheck again.";
		public const string RequiredTaxSettingNullInCalculateTraceFormat = "Required setting '{0}' is missing for tax '{1}'.";
		public const string RequiredEmployeeSettingNullInCalculateTraceFormat = "Required setting '{0}' is missing for state '{1}'.";
		public const string EmployeeTaxSettingValueTraceFormat = "Employee setting value: {0}";
		public const string CompanyTaxSettingValueTraceFormat = "Default company setting value: {0}";
		public const string InvalidBankName = "A bank name cannot consist of only digits; it must contain at least one letter.";
		public const string RoutingNumberRequires9Digits = "A bank routing number must contain 9 digits.";
		public const string CopyPasteIsNotAvailableForVoidPaychecks = "Copy-and-paste options are not available for void paychecks.";
		public const string CannotPasteIntoPaidOrReleasedPaycheck = "Data cannot be pasted into paid or released paychecks.";
		public const string InvalidStartDate = "{0} is not a valid date.";
		public const string DuplicateBanksWithUseClassDefault = "You cannot add duplicate PTO banks if the Use Class Default Values check box is selected.";
		public const string IrregularNbrPayPeriods = "Irregular number of pay periods. It will impact the salary and the PTO hours calculated by the system.";
		public const string AccrualMethodNotSupported = "No PTO accrual method is specified or its calculation has not been implemented.";
		public const string Unknown = "Unknown";
		public const string BulkProcessErrorFormat = "{0}: {1}";
		public const string ProjectCostAssignmentNotNoCostAssigned = "This option can be used only if 'No Cost Assigned' is selected in the Project Cost Assignment box.";
		public const string ProjectCostAssignmentNotWageCostsAssigned = "This option can be used only if 'Wage Costs Assigned' or 'Wage Costs and Labor Burden Assigned' is selected in the Project Cost Assignment box.";
		public const string EPPostingOptionNotDoNotPost = "This option can be used only if the Do not Post PM Transactions or Post PM Transactions from Time Activities Using an Off-Balance Account Group option is selected in the Time Posting Option box.";
		public const string EPPostingOptionNotWageCostsAssigned = "This option can be used only if the Post PM Transactions from Time Activities and Override Them with Payroll Information, Post PM and GL Transactions from Time Activities and Override Them with Payroll Information, or Post PM and GL Transactions from Payroll Only option is selected in the Time Posting Option box.";
		public const string SalariedExemptWithOT = "A salaried-exempt employee cannot have overtime.";
		public const string SalariedExemptWithOTReleaseException = "A payment cannot be released if a salaried-exempt employee has overtime.";
		public const string SalariedExemptWithOTCheckEarnings = "Overtime hours have been specified for at least one salaried-exempt employee. Open the Earning Details tab and remove the overtime hours for the salaried-exempt employees.";
		public const string ErrorOnOtherPayment = "An error in the calculation of the payment '{0}' is blocking the calculation of this payment.";
		public const string OnlySomeSegmentsCanBeAlternate = "Only default segments set as Project, Task, or Labor Item can be changed.";
		public const string EmployeeInDifferentCountry = "The same country should be specified in the employee's home address and in the work location associated with the employee.";
		public const string WorkCodeFromClassNotFound = "The {0} from the class cannot be found in the system.";
		public const string WebServiceDataTypeNotRecognized = "The {0} value is not recognized as a valid web service data type.";
		public const string CannotCalculateCountry = "Cannot calculate a payment with country ID '{0}'.";
		public const string PTOBankInUse = "This setting cannot be edited because the PTO bank is already in use.";
		public const string PTOFieldWillBeOverridden = "The value in the {0} box for this PTO disbursement line will be updated when the paycheck is calculated.";
		public const string PaycheckCantBeReleasedVerifyStatus = "Paycheck {0} cannot be released. You may need to verify its status.";
		public const string DedBenCodeInUse = "You cannot edit this deduction and benefit code because it is already in use.";
		public const string FinalPaycheckCantBeReleased = "Paycheck {0} cannot be released because it has the Final type and is supposed to be the last paycheck of the employee but at least one other paycheck has already been released in a later pay period for the same employee.";
		public const string WorkLocationInUse = "You cannot edit this work location because it is already in use.";
		public const string DuplicateOvertimeRule = "You are trying to add a duplicate overtime rule. In different overtime rules, at least one of the following settings should be different: active, disbursing earning type, type, threshold for overtime, day of the week, number of consecutive days, state, union or project.";
		public const string MandatoryDOB = "The employee's date of birth is required for tax calculation.";
		public const string EffectiveDateCannotBeChanged = "The effective date must be after {0}, which is the end date of the pay period of the last released paycheck.";
		public const string PTOBankCannotBeDeleted = "The PTO bank cannot be deleted because at least one paycheck is associated with it.";
		public const string PayGroupIsAssociatedWithPaycheck = "The select pay group is associated with existing paychecks. If you want to change the pay frequency of a pay group, you will have to create a new one in the Pay Groups (PR205000) form and then create a new Payroll Calendar for it on the Payroll Calendar (PR206000) form. If the paychecks are not released, you can delete all of them and then delete the pay group period.";
		public const string NegativePTOAmount = "This bank has a negative available amount and zero payable hours left.";
		public const string StateAlreadyAssociatedWithDedBenCode = "Another deduction and benefit code is already associated with this state. Different deduction and benefit codes can be associated with only different states.";
		public const string IncorrectTaxSetting = "At least one tax setting has an improper value stored in the database. This could happen because an incorrect setting was recorded during the import of tax settings. For instance, 'False' could be recorded instead of '0' for a check box. Please verify the settings in the import source file, identify the incorrect setting and manually update it on the Tax Settings tab of the Employee Payroll Settings (PR203000) form.";
		public const string PaycheckEarningLocationNotAssignedToEmployee = "At least one earnings record in this paycheck is associated with a location that is not included in the list of the employee's work locations on the Employee Payroll Settings (PR203000) form. Either add the location to the list on the Employee Payroll Settings form or specify a proper location in the earnings record.";
		public const string NoAccountGroup = "The record is associated with the {0} project but account {1} is not included in any account group. Either specify a different account that is already included in an account group or add account {1} to an account group by using the Account Groups (PM201000) form.";
		public const string TaxInvDescrTypeCantBeEmpty = "An invoice description source must be specified for each tax code. Click View Tax Details to open the dialog box where you can specify the missing setting.";
		public const string DeductCodeAssociatedToACertProject = "This deduction and benefit code is associated with a certified project. To be able to clear the Certified Reporting Type box, remove the deduction and benefit code from the Benefits Reducing the Rate table on the Fringe Benefits tab of the Certified Projects (PR209900) form.";
		#endregion Error Messages

		#region Payroll Batches Messages
		public const string PayrollBatchReCreateTitle = "Re-Create Batch";
		public const string PayrollBatchReCreateConfirm = "This will delete all current Transaction Detail records. Do you want to continue?";
		#endregion

		#region Miscellaneous Strings
		public const string Add = "Add";
		public const string None = "None";
		public const string NotSetUp = "<Not Set Up>";
		public const string NotImplemented = "<Not Implemented>";
		public const string Remove = "Remove";
		public const string AddTaxCode = "Add Tax Code";
		public const string SemiMonthlyFirstHalfDescr = "First Half - {0:MMMM}";
		public const string SemiMonthlySecondHalfDescr = "Second Half - {0:MMMM}";
		public const string Benefit = "Benefit";
		public const string Deduction = "Deduction";
		public const string Tax = "Tax";
		public const string PayCheckReport = "Check";
		public const string CreatePREmployeeLabel = "Create Payroll Employee";
		public const string EditPREmployeeLabel = "Edit Payroll Employee";
		public const string LaborItem = "Labor Item";
		public const string Project = "Project";
		public const string ProjectTask = "Project Task";
		public const string Others = "others";
		public const string DirectDepositBatch = "Direct Deposit Batch";
		public const string NewKey = "<NEW>";
		public const string PREmployee = "Employee";
		#endregion

		#region PXCacheName Values
		public const string PRAcaAggregateGroupMember = "ACA Aggregate Group Member";
		public const string PRAcaCompanyYearlyInformation = "ACA Company Yearly Information";
		public const string PRAcaCompanyMonthlyInformation = "ACA Company Monthly Information";
		public const string PRAcaDeductCode = "ACA Deduction Code";
		public const string PRAcaDeductCoverageInfo = "ACA Deduction Code Coverage Information";
		public const string PRAcaEmployeeMonthlyInformation = "ACA Employee Monthly Information";
		public const string PRAcaUpdateEmployeeFilter = "ACA Update Employee Filter";
		public const string PRAcaUpdateCompanyMonthFilter = "ACA Update Company Month Filter";
		public const string PRBatch = "Batch";
		public const string PRBatchDeduct = "Batch Deduction";
		public const string PRBatchEmployee = "Batch Employee";
		public const string PRRecordOfEmployment = "Record Of Employment";
		public const string PRROEInsurableEarningsByPayPeriod = "Insurable Earnings by Pay Period";
		public const string PRROEStatutoryHolidayPay = "Statutory Holiday Pay";
		public const string PRROEOtherMonies = "Other Monies";
		public const string PRGovernmentSlip = "Government Slip";
		public const string PRGovernmentSlipField = "Government Slip Field";
		public const string PRTaxFormBatch = "Tax Form Batch";
		public const string PRTaxReportingAccount = "Tax Reporting Account";
		public const string PREmployeeTaxForm = "Employee Tax Form";
		public const string PREmployeeTaxFormData = "Employee Tax Form Data";
		public const string PRBenefitDetail = "Benefit Detail";
		public const string PRBenefitType = "Benefit Type";
		public const string PRCompanyTaxAttribute = "Company Tax Setting";
		public const string PRDeductCode = "Deduction Code";
		public const string PRDeductCodeDetail = "Deduction Code Taxability";
		public const string PRDeductCodeSubjectToTaxes = "Deduction Code Subject To Taxes";
		public const string PRDeductionDetails = "Deduction Details";
		public const string PRDeductionSummary = "Deduction Summary";
		public const string PREarningType = "Earning Type";
		public const string PREarningTypeDetail = "Earning Type Detail";
		public const string PREmployeeCacheName = "Payroll Employee";
		public const string PREmployeeAttribute = "Employee Setting";
		public const string PREmployeeClass = "Employee Payroll Class";
		public const string PREmployeeClassPTOBank = "Employee Class PTO Bank";
		public const string PREmployeeDirectDeposit = "Employee Direct Deposit";
		public const string PREmployeeDeduct = "Employee Deduct";
		public const string PREmployeeEarning = "Employee Earning";
		public const string PREmployeePTOBank = "Employee PTO Bank";
		public const string PREmployeePTOHistory = "Employee PTO History";
		public const string PREmployeeTax = "Employee Tax";
		public const string PREmployeeTaxAttribute = "Employee Tax Setting";
		public const string PRGovernmentReport = "Government Report";
		public const string PRGovernmentReportingFilter = "Government Reporting Filter";
		public const string PRTaxFormsFilter = "Tax Forms Filter";
		public const string PRLocation = "Location";
		public const string PRPayGroup = "Pay Group";
		public const string PRPayGroupYear = "Pay Group Year";
		public const string PayCheckDetail = "Pay Check Detail";
		public const string PayCheckDetailFilter = "Pay Check Detail Filter";
		public const string PRPayGroupCalendar = "Pay Group Calendar";
		public const string PRPayGroupPeriod = "Pay Periods";
		public const string PRPayGroupPeriodSetup = "Pay Group Period Setup";
		public const string PRPayment = "Payment";
		public const string PRPaymentEarning = "Payment Earning";
		public const string PRPaymentPTOBank = "Payment PTO Bank";
		public const string PRPayPeriodCreationDialog = "Pay Period Creation Dialog";
		public const string PREarningDetail = "Earning Detail";
		public const string PROvertimeRule = "Overtime Rule";
		public const string PRBatchOvertimeRule = "Batch Overtime Rule";
		public const string PRPaymentOvertimeRule = "Payment Overtime Rule";
		public const string PRRegularTypeForOvertime = "Regular Type for Overtime";
		public const string PRPaymentTax = "Payment Tax";
		public const string PRPaymentTaxSplit = "Tax Splits";
		public const string PRPTOBank = "PTO Bank";
		public const string PRPeriodTaxes = "Period Taxes";
		public const string PRSetup = "Payroll Preferences";
		public const string PRTaxCode = "Tax Code";
		public const string PRTaxCodeAttribute = "Tax Code Setting";
		public const string PRTaxDetail = "Tax Detail";
		public const string PRTaxType = "Tax Type";
		public const string PRTransactionDateException = "Transaction Date Exception";
		public const string PRYtdEarnings = "YTD Earnings";
		public const string PRYtdDeductions = "YTD Deductions";
		public const string PRYtdTaxes = "YTD Taxes";
		public const string PTOBanksFilter = "PTO Banks Filter";
		public const string PRDeductionAndBenefitUnionPackage = "Deductions And Benefits Union Package";
		public const string PRDeductionAndBenefitProjectPackage = "Deductions And Benefits Project Package";
		public const string PrintChecksFilter = "Print Checks Filter";
		public const string PayrollDocumentsFilter = "Payroll Documents Filter";
		public const string PRProjectFringeBenefitRate = "Project Fringe Benefit Rate";
		public const string PRProjectFringeBenefitRateReducingDeduct = "Project Fringe Benefit Rate Reducing Deduction";
		public const string PRProjectFringeBenefitRateReducingEarning = "Project Fringe Benefit Rate Reducing Earning";
		public const string PRDirectDepositSplit = "Direct Deposit Split";
		public const string PRBatchDetail = "Batch Detail";
		public const string PRWorkCompensationBenefitRate = "Work Compensation Benefit Rate";
		public const string PRWorkCompensationMaximumInsurableWage = "Workers' Compensation Maximum Insurable Wage";
		public const string PRPaymentWCPremium = "Payment Work Compensation Premium";
		public const string EmploymentHistory = "Employment History";
		public const string CreateEditPREmployeeFilter = "Create/Edit Payroll Employee Filter";
		public const string PRPaymentUnionPackageDeduct = "Payment Union Package Deduction";
		public const string PRPaymentProjectPackageDeduct = "Payment Project Package Deduction";
		public const string PREmployeeClassWorkLocation = "Employee Class Work Location";
		public const string PREmployeeWorkLocation = "Employee Work Location";
		public const string PRPaymentFringeBenefit = "Payment Fringe Benefit";
		public const string PRPaymentFringeBenefitDecreasingRate = "Payment Fringe Benefit Decreasing Rate";
		public const string PRPaymentFringeEarningDecreasingRate = "Payment Fringe Earning Decreasing Rate";
		public const string PRDeductCodeEarningIncreasingWage = "Deduct Code Earning Increasing Wage";
		public const string PRDeductCodeBenefitIncreasingWage = "Deduct Code Benefit Increasing Wage";
		public const string PRDeductCodeTaxIncreasingWage = "Deduct Code Tax Increasing Wage";
		public const string PRDeductCodeDeductionDecreasingWage = "Deduct Code Deduction Decreasing Wage";
		public const string PRDeductCodeTaxDecreasingWage = "Deduct Code Tax Decreasing Wage";
		public const string PRPaymentBatchExportHistory = "Payment Batch Export History";
		public const string PRPaymentBatchExportDetails = "Payment Batch Export Details";
		public const string PRCABatch = "CA Batch for Payroll";
		public const string PRTaxSettingAdditionalInformation = "Tax Setting Additional Information";
		public const string PRTaxWebServiceData = "Tax Web Service Data";
		public const string PRPeriodTaxApplicableAmounts = "Period Tax Applicable Amounts";
		public const string PRPaymentTaxApplicableAmounts = "Payment Tax Applicable Amounts";
		public const string PRPTODetail = "PTO Detail";
		#endregion

		#region PRPayBatchEntry Error Messages
		public const string EmployeeIDCannotBeNull = "EmpoyeeID is not specified";
		public const string BatchStartDateCannotBeNull = "Batch Start Date is not specified";
		public const string BatchEndDateCannotBeNull = "Batch End Date is not specified";
		public const string ActivityOnHold = "Activity on Hold";
		public const string ActivityPendingApproval = "Activity Pending Approval";
		public const string ActivityNotReleased = "Activity Not Released";
		public const string EmployeeWasNotEmployed = "Employee was not employed during the Payroll Batch period";
		public const string PayRateNotFound = "At least one of the selected employees has no compensation settings specified on the Employee Payroll Settings (PR203000) form. Remove such employees from the batch or specify necessary settings for them. See Trace for the list of problematic employee records.";
		public const string ActivityWhenNotEmployed = "Activity when employee was not employed";
		public const string RegularHoursTypeIsNotSetUp = "The Regular Hours earning type is not set up for the quick pay process.";
		public const string HolidaysTypeIsNotSetUp = "The Holiday earning type is not set up for the quick pay process.";
		public const string RegularAndHolidaysTypesAreNotSetUp = "The Regular Hours and Holiday earning types are not set up for the quick pay process.";
		public const string CommissionTypeIsNotSetUp = "Commission Earning Type is not set up";
		public const string IncorrectNumberOfPayPeriods = "Incorrect number of Pay Periods";
		public const string EmployeeAlreadyAddedToThisBatch = "The employee has already been added to this payroll batch.";
		public const string EmployeeAlreadyAddedToBatch = "The employee has already been added to the payroll batch ({0}) with the same pay period. Would you like to view the existing payroll batch or continue editing the current paycheck?";
		public const string EmployeeAlreadyAddedToAnotherBatch = "The payroll batch cannot be processed because a payroll batch for an earlier pay period has not been released yet. First you need to delete the earlier batch or release it and process its paychecks.";
		public const string EmployeeAlreadyAddedToRegularPaycheck = "A regular paycheck ({0}) with the same or earlier pay period has already been created for the employee.";
		public const string EmployeeAlreadyAddedToNonRegularPaycheck = "A paycheck ({0}) with the same or earlier pay period has already been created for the employee. If you need to include this employee in the batch, delete the paycheck first.";
		public const string EmployeeAlreadyAddedToPaycheckBatchRelease = "It is impossible to release the payroll batch since the paycheck ({0}) with the same or earlier pay period has already been created for {1}. To release the payroll batch please delete the document detail record for this employee first.";
		public const string EmployeeAlreadyAddedToAnotherPaycheck = "Another paycheck ({0}) with the same or earlier pay period has already been created for the employee. Would you like to view the existing paycheck or continue editing the current one?";
		public const string EmployeeAlreadyAddedToAnotherPaycheckError = "Another paycheck ({0}) with the same or earlier pay period has already been created for the employee";
		public const string TimeActivityAlreadyAddedToThisBatch = "The time activity has already been added to this payroll batch.";
		public const string TimeActivityAlreadyAddedToThisPaycheck = "The time activity has already been added to this paycheck.";
		public const string TimeActivityAlreadyAddedToBatch = "The time activity has already been added to the {0} payroll batch with the same pay period.";
		public const string TimeActivityAlreadyAddedToPaycheck = "The time activity has already been added to the {0} paycheck with the same pay period.";
		public const string TimeActivityTimeSpentChanged = "The associated time activity has {0} hour(s) specified.";
		public const string EmployeeEarningDetailsCreationFailed = "Creating Earning Details for Employee ({0}) failed with error: {1}";
		public const string EarningDetailsCreationFailedForSomeEmployees = "There were errors during Earning Details creation for some Employees. Please check the trace for more details.";
		public const string EarningDetailsCreationFailedBecauseOfTimeActivity = "At least one of the selected employees has unreleased time activities for the days within the pay period. The system can produce earning details only for employees whose activities between the start and end dates of the pay period have all been released. See Trace for the list of problematic employee records.";
		public const string EarningDetailsCreationFailedBecauseOfDates = "At least one of the selected employees has the employment start date that is later than the end date of the pay period. Remove such employees from the selection because the system cannot produce earning details for them. See Trace for the list of problematic employee records.";
		public const string NoAccessToAllEmployeesInPRBatch = "You are not permitted to see the information related to at least one employee included in this batch.";
		public const string EmployeeCannotBeAddedToPayrollBatch = "Employee {0} cannot be added to the payroll batch.";
		public const string NegativeHoursEarnings = "Employee {0} has at least one earning line with negative hours specified.";
		public const string CantReleaseWithNegativeHours = "Some employees have earning lines with negative hours specified. You need to adjust the earning details before releasing this document.";
		public const string EmployeeCountryNotActive = "The payroll functionality is not enabled for the country associated with the employee ({0}).";
		public const string PaymentBranchCountryNotMatching = "The country associated with the payment ({0}) differs from the country specified in the company address ({1}).";
		#endregion

		#region TimeCards
		public const string TimeActivitiesImportedToPaycheck = "The time activities have been imported to Payroll. If you want to update or delete the time card, delete the corresponding earning detail records of the '{0}' paycheck.";
		public const string TimeActivitiesImportedToPaidOrReleasedPaycheck = "The time activities have been imported to Payroll. If you want to update or delete the time card, void the '{0}' paycheck.";
		public const string TimeActivitiesImportedToPayrollBatch = "The time activities have been imported to Payroll. If you want to update or delete the time card, delete the corresponding earning detail records of the '{0}' payroll batch.";
		#endregion

		#region PRPayRate Warning and Error Messages
		public const string EmptyPayRate = "Pay Rate is empty";
		public const string ZeroPayRate = "Pay Rate is zero";
		public const string NegativePayRate = "Pay Rate is negative";
		public const string EarningTypeNotFound = "'{0}' Earning Type was not found in the {1} table.";
		#endregion

		#region RegularAmount Warning and Error Messages
		public const string IncorrectNumberOfPayPeriodsInPayGroup = "The {0} pay group has an incorrect number of pay periods.";
		public const string RegularHoursTypeIsNotSetUpInPayrollPreferences = "The Regular Hours earning type is not set up on the Payroll Preferences (PR101000) form.";
		public const string SuitableEmployeeEarningNotFound = "No suitable employee earning type has been found.";
		public const string OverlappingEarnings = "This pay rate applies only if the pay period's start date is past the start date of this record (but not past its end date).";
		#endregion

		#region Tax Form Batch Information, Warning and Error Messages
		public const string BatchForSubmission = "Batch for Submission";
		public const string T4 = "T4 Statement of Remuneration Paid";
		public const string ViewDiscrepancies = "View Discrepancies";
		public const string EmployeeSlipAlreadyPublished = "An employee slip has already been published from a different batch ({0}). Confirm if you want to replace the published slip with the slip from the current batch.";
		public const string ConfirmationHeader = "Confirmation";
		public const string ConfirmLabel = "Confirm";
		public const string CancelLabel = "Cancel";
		public const string NoOriginalTaxFormBatchForCorrectAction = "No original has been prepared for some employees in the selected year. Confirm if you still want to proceed.";
		public const string NoOriginalTaxFormBatchForAmendment = "No original has been prepared for this employee in the selected year. Confirm if you still want to proceed with preparing an amendment for submission.";
		public const string NoOriginalTaxFormBatchForCancellation = "No original has been prepared for this employee in the selected year. Confirm if you still want to proceed with preparing a cancellation for submission";
		public const string CurrentBatchWillBeDeleted = "The current Batch for Submission will be deleted.";
		public const string IncludesUnreleasedPaychecks = "Includes Data from Unreleased Paychecks";
		public const string ErrorDuringPdfCreation = "There was an error during the creation of the PDF preview. See trace for details.";
		public const string TaxFormIsNotSupported = "The '{0}' tax form is not supported.";
		public const string TaxFormBatchTypeIsNotSupported = "The '{0}' tax form batch type is not supported.";
		public const string ImpossibleToGenerateTaxForm = "Impossible to generate the '{0}' tax form file in the '{1}' format";
		public const string CheckDiscrepancies = "Do you want the system to automatically verify if there are some differences between the values from the last generated batch and the data from paychecks?";
		public const string TaxFormToBeGenerated = "After the release, you will need to generate a new tax form by using the Correct command on the Prepare Tax Forms form. Do you want to proceed with the release?";
		public const string CreateCancellationT4 = "A cancellation T4 should be created for this employee. Would you like to prepare it now?";
		public const string PrepareCancellationTaxForm = "Prepare cancellation tax form";
		public const string Releve1 = "Releve1 - Revenus d’emploi et revenus divers";
		#endregion

		#region Deduction Warnings
		public const string DeductCodeInactive = "Deduction code is not active.";
		public const string PaymentDeductInactivated = "Deduction has been de-activated in the following pay checks: {0}.";
		public const string CantFindProjectPackageDeduct = "No certified project deduction and benefit package has been found for this combination of Project ID, Labor Item ID, and Deduction and Benefit Code.";
		public const string CantFindUnionPackageDeduct = "No union local deduction and benefit package has been found for this combination of Union Local ID, Labor Item ID, and Deduction and Benefit Code.";
		#endregion Deduction Warnings

		#region PRCreateLiabilitiesAPBill Messages
		public const string PayrollLiabilities = "Payroll Liabilities";
		#endregion PRCreateLiabilitiesAPBill Messages

		#region Government Reporting Period
		public const string Annual = "Annual";
		public const string DateRange = "Date Range";

		public const string Quarter = "Quarter";
		public const string Month = "Month";
		public const string DateFrom = "Date From";
		public const string DateTo = "Date To";
		public const string InconsistentDateRange = "Date To can't be before Date From.";
		#endregion

		#region PaymentMethod

		public const string PrintChecks = "Print Checks";
		public const string CreateBatchPayment = "Create Batch Payments";

		#endregion PaymentMethod
		
		#region ACA Reporting
		public const string QualifyingOfferMethod = "Qualifying Offer Method";
		public const string NinetyEightPctOfferMethod = "98% Offer Method";
		public const string FullTime = "Full Time";
		public const string PartTime = "Part Time";
		public const string Employee = "Employee";
		public const string Spouse = "Spouse";
		public const string Children = "Children";
		public const string MeetsEssentialCoverageAndValue = "Meets Minimum Essential Coverage and provides Minimum Value";
		public const string MeetsEssentialCoverage = "Meets Minimum Essential Coverage but does not provide Minimum Value";
		public const string SelfInsured = "Self-Insured";
		public const string NoneOfTheAbove = "None of the Above";
		#endregion

		#region Payment Deduction Sources
		public const string EmployeeSettingsSource = "Employee Settings";
		public const string CertifiedProjectSource = "Certified Project";
		public const string UnionSource = "Union";
		public const string WorkCodeSource = "Workers' Compensation";
		#endregion
		
		#region Document Release Actions
		public const string PutOnHoldAction = "Put on Hold";
		public const string RemoveFromHoldAction = "Remove from Hold";
		public const string Calculate = "Calculate";
		public const string PrintPayStub = "Print Pay Stub";
		public const string Recalculate = "Recalculate";
		public const string Release = "Release";
		public const string CreateROE = "Create ROE";
		#endregion
		
		#region Prepare Tax Forms Actions
		public const string Prepare = "Prepare";
		public const string Correct = "Correct";
		public const string CorrectAll = "Correct All";
		public const string T4Form = "T4";
		public const string RL1Form = "Releve1";
		#endregion

		#region Transaction Date Exception Behaviors
		public const string TransactionDayBefore = "Paid First Business Day Before Exception";
		public const string TransactionDayAfter = "Paid First Business Day After Exception";
		public const string PeriodWillBeShifted = "Period will be shifted to year {0}.";
		public const string CantChangePeriodTransDate = "Cannot change period transaction date. Pay period is already being used by payment: {0}.";
		#endregion

		#region Direct Deposit Messages

		public const string ViewPRDocument = "View Payment Document";
		public const string DisplayPayStubs = "Display Pay Stubs";
		public const string PrintPayStubs = "Print Paystubs";
		public const string AddPayment = "Add Payment";
		public const string NeedCalculationWarning = "The paycheck must be calculated to pay this batch.";
		public const string Actions = "Actions";
		public const string ExportPrenote = "Export as Prenote";

		#endregion Direct Deposit Messages

		#region Employee Portal

		public const string ViewPayStub = "View Pay Stub";
		public const string ViewTaxForm = "View Tax Form";

		#endregion Employee Portal

		#region Project Cost Assignment Types
		public const string NoCostAssigned = "No Cost Assigned";
		public const string WageCostAssigned = "Wage Costs Assigned";
		public const string WageLaborBurdenAssigned = "Wage Costs and Labor Burden Assigned";
		#endregion

		#region Tax setting messages
		public const string NewTaxSetting = "Review and, if needed, update the new tax setting.";
		#endregion

		#region Tran descriptions
		public const string DefaultPaymentDescriptionFormat = "Paycheck for {0} - {1}";
		public const string DefaultPaymentDescriptionWithHiddenNameFormat = "Paycheck {0} - {1}";
		public const string DefaultRoeDescriptionFormat = "{0} - {1} - Record of Employment";
		public const string EarningDescriptionFormat = "Earning {0} - {1}";
		public const string DeductionLiabilityFormat = "Deduction Liability for {0}";
		public const string BenefitExpenseFormat = "Benefit Expense for {0}";
		public const string BenefitLiabilityFormat = "Benefit Liability for {0}";
		public const string TaxExpenseFormat = "Tax Expense for {0}";
		public const string TaxLiabilityFormat = "Tax Liability for {0}";
		public const string PTOExpenseFormat = "PTO Expense for {0}";
		public const string PTOLiabilityFormat = "PTO Liability for {0}";
		public const string PTOAssetFormat = "PTO Asset Credit for {0}";
		public const string TimeTransactionReverse = "Time Transaction Reverse for '{0}'";
		#endregion

		#region Months
		public const string January = "January";
		public const string February = "February";
		public const string March = "March";
		public const string April = "April";
		public const string May = "May";
		public const string June = "June";
		public const string July = "July";
		public const string August = "August";
		public const string September = "September";
		public const string October = "October";
		public const string November = "November";
		public const string December = "December";
		#endregion Months

		#region Deduction/Benefit Applicable Earnings
		public const string TotalEarnings = "Total Earnings";
		public const string RegularEarnings = "Regular Earnings";
		public const string RegularAndOTEarnings = "Regular and OT Earnings";
		public const string RegularAndOTEarningsWithPTO = "Regular and OT Earnings with Time Off";
		public const string StraightTimeEarnings = "Straight Time Earnings";
		public const string StraightTimeEarningsWithPTO = "Straight Time Earnings and Time Off";
		public const string TotalEarningsWithOTMultiplier = "Total Earnings with Multiplier Applied to Overtime";
		public const string RegularAndOTEarningsWithOTMultiplier = "Regular and Overtime Earnings with Mutiplier Applied to Overtime";
		#endregion

		#region Project/Tasks warnings

		public const string ProjectStatusWarning = "Project status is '{0}'.";
		public const string TaskStatusWarning = "Task status is '{0}'.";

		#endregion

		#region PaymentBatchStatus list attribute

		public const string ReadyForPrint = "Ready to Print";
		public const string WaitingPaycheckCalculation = "Waiting Paycheck Calculation / Payment";
		public const string Paid = "Paid";

		#endregion PaymentBatchStatus list attribute

		#region Payment Batches screen

		public const string ConfirmPayment = "Confirm Payment";
		public const string ConfirmPaymentAndRelease = "Confirm Payment and Release";
		public const string CancelPayment = "Cancel Payment";
		public const string ExportReasonOtherFormat = "Other: {0}";
		public const string ExportDetails = "Export Details";
		public const string PrintDetails = "Printing Details";

		#endregion Payment Batches screen

		#region PaymentBatchExportReason list attribute

		public const string Initial = "Initial Export";
		public const string WrongEmployeeConfiguration = "Wrong Employee Configuration";
		public const string PaycheckError = "Error in paycheck";
		public const string BatchError = "Error in the batch";
		public const string Prenote = "Prenote Export";
		public const string OtherReason = "Other";

		#endregion PaymentBatchExportReason list attribute
		
		#region Searchable Titles
		public const string SearchableTitlePREmployee = "Payroll Employee: {0} {1}";
		public const string SearchableTitlePRPayment = "Paycheck: {0} {1}";
		public const string SearchableTitlePRBatch = "Payroll Batch: {0} {1}";
		public const string SearchableTitlePRCABatch = "Payroll Direct Deposit Batch: {0} {1}";
		public const string SearchableTitlePRROE = "Record of Employment: {0}";
		#endregion

		#region Dashboard Selection Period Names
		public const string LastMonth = "Last Month";
		public const string Last12Months = "Last 12 Months";
		public const string CurrentQuarter = "Current Quarter";
		public const string CurrentCalYear = "Current Calendar Year";
		public const string CurrentFinYear = "Current Financial Year";
		#endregion

		#region PtoAccrualMethod

		public const string Percentage = "Percentage";
		public const string TotalHoursPerYear = "Total Hours per Year";

		#endregion PtoAccrualMethod

		#region Validate totals warnings/errors
		public const string UpdatingFromDetailsWarning = " Updating summary from details.";
		public const string SummaryEarningAmountDoesntMatch = "The summary earnings amount for the type {0} and location {1} ({2}) does not match the earnings details ({3}). Delete the paycheck and recreate it.";
		public const string SummaryEarningHoursDontMatch = "The summary earnings hours for the type {0} and location {1} ({2}) do not match the earnings details ({3}). Delete the paycheck and recreate it.";
		public const string SummaryEarningRateDoesntMatch = "The summary earnings rate for the type {0} and location {1} is not correctly calculated. Delete the paycheck and recreate it.";
		public const string SummaryEarningMissing = "The summary earnings record for the type {0} and location {1} is missing. Delete the paycheck and recreate it.";
		public const string SummaryDeductionDoesntMatch = "The summary deduction amount for {0} ({1}) does not match the deduction details ({2}). Delete the paycheck and recreate it.";
		public const string SummaryBenefitDoesntMatch = "The summary benefit amount for {0} ({1}) does not match the benefit details ({2}). Delete the paycheck and recreate it.";
		public const string SummaryDeductionMissing = "The summary deduction record for the type {0} is missing. Delete the paycheck and recreate it.";
		public const string TaxDetailsDontMatch = "Tax details' amount for {0} ({1}) does not match the tax splits ({2}). Delete the paycheck and recreate it.";
		public const string TaxDetailMissing = "The tax details for {0} is missing. Update the tax details information on the Taxes tab.";
		public const string SummaryTaxAmountDoesntMatch = "The summary tax amount for {0} ({1}) does not match the tax details ({2}). Update the tax details information on the Taxes tab.";
		public const string SummaryTaxHoursDoesntMatch = "The summary tax taxable hours for {0} ({1}) do not match the tax splits ({2}). Update the tax details information on the Taxes tab.";
		public const string SummaryTaxWagesDontMatch = "The summary tax taxable wages for {0} ({1}) do not match the tax splits ({2}). Update the tax details information on the Taxes tab.";
		public const string SummaryTaxGrossDoesntMatch = "The summary tax taxable gross for {0} ({1}) does not match the tax splits ({2}). Update the tax details information on the Taxes tab.";
		public const string SummaryTaxMissing = "Summary tax record for {0} is missing.";
		public const string SummaryPTODoesntMatch = "The paycheck accrual amount for {0} ({1}) differs from the amount in the PTO details ({2}).";
		public const string SummaryPTOMissing = "There is no PTO bank for {0}.";
		public const string CannotFindDeductCode = "The {0} deduction and benefit code cannot be found in the system.";
		public const string HeaderEarningAmountDoesntMatch = "The total earnings amount in the summary area ({0}) does not match the earnings summaries ({1}). Delete the paycheck and recreate it.";
		public const string HeaderEarningHoursDontMatch = "The total earnings hours in the summary area ({0}) do not match the earnings summaries ({1}). Delete the paycheck and recreate it.";
		public const string HeaderDeductionAmountDoesntMatch = "The total deduction amount in the summary area ({0}) does not match the deduction summaries ({1}). Delete the paycheck and recreate it.";
		public const string HeaderBenefitAmountDoesntMatch = "The total benefit amount in the summary area ({0}) does not match the benefit summaries ({1}). Delete the paycheck and recreate it.";
		public const string HeaderPayableBenefitAmountDoesntMatch = "The total payable benefit amount in the summary area ({0}) does not match the benefit summaries ({1}). Delete the paycheck and recreate it.";
		public const string HeaderEmployeeTaxAmountDoesntMatch = "The total employee tax amount in the summary area ({0}) does not match the tax summaries ({1}). Delete the paycheck and recreate it.";
		public const string HeaderEmployerTaxAmountDoesntMatch = "The total employer tax amount in the header ({0}) doesn't match tax summaries ({1}).";
		public const string HeaderGrossAmountDoesntMatch = "The gross amount in the header ({0}) doesn't match earning and benefit summaries ({1}).";
		public const string HeaderNetAmountDoesntMatch = "The net amount in the header ({0}) doesn't match earning, deduction, benefit and tax summaries ({1}).";
		public const string HeaderDetailLinesCountDoesntMatch = "The detail lines count in the header ({0}) doesn't match earning, deduction, benefit and tax summaries ({1}).";
		public const string DirectDepositSplitsDontMatch = "The net amount in the header ({0}) doesn't match the direct deposit splits ({1}).";
		#endregion Validate totals warnings/errors

		#region Time posting options
		public const string DoNotPost = "Do Not Post PM Transactions";
		public const string PostFromTime = "Post PM Transactions from Time Activities Using an Off-Balance Account Group";
		public const string OverridePMInPayroll = "Post PM Transactions from Time Activities and Override Them with Payroll Information";
		public const string OverridePMAndGLInPayroll = "Post PM and GL Transactions from Time Activities and Override Them with Payroll Information";
		public const string PostPMandGLFromPayroll = "Post PM and GL Transactions from Payroll Only";
		#endregion

		#region Earning type taxability values
		public const string CashSubjectTaxable = "Cash Subject Taxable";
		public const string CashSubjectNonTaxable = "Cash Subject Non-Taxable";
		public const string CashNonSubjectNonTaxable = "Cash Non-Subject Non-Taxable";
		public const string NonCashSubjectTaxable = "Non-Cash Subject Taxable";
		public const string NonCashSubjectNonTaxable = "Non-Cash Subject Non-Taxable";
		public const string NonCashNonSubjectNonTaxable = "Non-Cash Non-Subject Non-Taxable";
		#endregion

		#region Tax split wage types
		public const string Tips = "Tips";
		public const string NotTips = "Wages other than tips";
		#endregion

		#region PTO Disbursing Types
		public const string CurrentRate = "Current Rate";
		public const string AverageRate = "Average Rate";
		#endregion
		
		#region  Settlement / Final paycheck

		public const string Pay = "Pay Balance";
		public const string Keep = "Keep Balance";
		public const string Discard = "Discard Balance";

		public const string EarningIsPayingSettlementWarning = "Automatic disbursement for final paycheck.";
		public const string ModifiedPeriodEndNeedsHourlyError = "Only 'Hourly' value can be used when the payment's period end date has been modified.";

		#endregion  Settlement / Final paycheck

		public const string WorkersCompensationFormat = "Workers Compensation - {0}";
		public const string AskSkipCarryoverPayments = "Do you want to skip all carryover payment for this pay check?";
		public const string EarningTypeInactive = "The earning type is not active.";
		public const string PTOBankNotActive = "The PTO bank is not active.";
		public const string EmployeeAndBranchCountriesDifferent = "For correct calculation of taxes, make sure that the same country is specified in the employee's residential address, in the employee branch or company, and in each work location associated with the employee.";
	}
}
