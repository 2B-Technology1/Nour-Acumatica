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
using PX.CarrierService;
using PX.Data;
using PX.Common;

namespace PX.Objects.PM
{
	[PXLocalizable(Messages.Prefix)]
	public static partial class Messages
	{
		#region Validation and Processing Messages
		public const string Prefix = "PM Error";
		public const string Account_FK = "Account Group cannot be deleted. One or more Accounts are mapped to this Account Group.";
		public const string AccountDiactivate_FK = "Account Group cannot be deactivated. One or more Accounts are mapped to this Account Group.";
		public const string ProjectStatus_FK = "Account Group cannot be deleted. Project Status table contains one or more references to the given Account Group.";
		public const string OnlyPlannedCanbeDeleted = "The task cannot be deleted. You can delete a task with only the Planning or Canceled status.";
		public const string StartEndDateInvalid = "Planned Start Date for the given Task should be before the Planned End Date.";
		public const string UncompletedTasksExist = "Project can only be Completed if all Tasks are completed. {0} Task(s) are still incomplete.";
		public const string ProjectIsCanceled = "Project is Canceled and cannot be used for data entry.";
		public const string ProjectIsCompleted = "Project is Completed and cannot be used for data entry.";
		public const string ProjectIsSuspended = "Project is Suspended and cannot be used for data entry.";
		public const string ProjectTaskIsCompleted = "Task is Completed and cannot be used for data entry.";
		public const string ProjectTaskIsCompletedDetailed = "Task is Completed and cannot be used for data entry. Project:'{0}' Task:'{1}'";
		public const string ProjectTaskIsCanceled = "Task is Canceled and cannot be used for data entry.";
		public const string HasTranData = "Cannot delete Task since it already has at least one Transaction associated with it.";
		public const string HasActivityData = "Cannot delete Task since it already has at least on Activity associated with it.";
		public const string HasTimeCardItemData = "Cannot delete Task since it already has at least one Time Card Item Record associated with it.";
		public const string ValidationFailed = "One or more rows failed to validate. Please correct and try again.";
		public const string NoAccountGroup = "Record is associated with Project whereas Account '{0}' is not associated with any Account Group";
		public const string NoAccountGroup2 = "The account specified in the project-related line must be mapped to an account group. Assign an account group to the {0} account or select the non-project code in the line.";
		public const string NoOffsetAccountGroup = "The offset account specified in the project-related line is not mapped to an account group. Assign an account group to the {0} account or select the non-project code in the line.";
		public const string AccountGroupNotFound = "The line is associated with the {0} project but the {1} account specified in the document line is not mapped to any account group. Select the non-project code or change the account in the line.";
		public const string InactiveProjectsCannotBeBilled = "Inactive Project cannot be billed.";
		public const string CancelledProjectsCannotBeBilled = "The project is canceled and cannot be billed.";
		public const string NoNextBillDateProjectCannotBeBilled = "Project can not be Billed if Next Billing Date is empty.";
		public const string NoCustomer = "This Project has no Customer associated with it and thus cannot be billed.";
		public const string FailedToAddLine = "One purchase order line or multiple purchase order lines cannot be added to the bill. See Trace Log for details.";
		public const string FailedToEmulateExpenses = "Failed to emulate Expenses when running Auto Budget. Probably there is no Expense Account Group in the Budget.";
		public const string FailedToCalcDescFormula = "Failed to calculate Description formula in the allocation rule:{0}, Step{1}. Formula:{2} Error:{3}";
		public const string FailedToCalcAmtFormula = "Failed to calculate Amount formula in the allocation rule:{0}, Step{1}. Formula:{2} Error:{3}";
		public const string FailedToCalcQtyFormula = "Failed to calculate Quantity formula in the allocation rule:{0}, Step{1}. Formula:{2} Error:{3}";
		public const string FailedToCalcBillQtyFormula = "Failed to calculate Billable Quantity formula in the allocation rule:{0}, Step{1}. Formula:{2} Error:{3}";
		public const string FailedToCalcDescFormula_Billing = "Failed to calculate the line description using the {3} step of the {0} billing rule. Line Description Formula: {1} Error: {2}";
		public const string FailedToCalcAmtFormula_Billing = "Failed to calculate the amount using the {3} step of the {0} billing rule. Line Amount Formula: {1} Error: {2}";
		public const string FailedToCalcQtyFormula_Billing = "Failed to calculate the quantity using the {3} step of the {0} billing rule. Line Quantity Formula: {1} Error: {2}";
		public const string FailedToCalcInvDescFormula_Billing = "Failed to calculate the invoice description using the {3} step of the {0} billing rule. Invoice Description Formula: {1} Error: {2}";
		public const string ResultNotReturned = "No result has been returned. Please check Trace for details.";
		public const string PeriodsOverlap = "Overlapping time intervals are not allowed";
		public const string Activities = "Activities";
		public const string RangeOverlapItself = "Range for the summary step should not refer to itself.";
		public const string RangeOverlapFuture = "Range for the summary step should not refer future steps.";
		public const string ReversalExists = "Reversal for the given allocation already exist. Allocation can be reversed only once. RefNbr of the reversal document is {0}.";
		public const string TaskAlreadyExists = "Task with this ID already exists in the Project.";
		public const string AllocationStepFailed = "Failed to Process Step: {0} during Allocation for Task: {1}. Check Trace for details.";
		public const string DebitProjectNotFound = "Step '{0}': Debit Project was not found in the system.";
		public const string CreditProjectNotFound = "Step '{0}': Credit Project was not found in the system.";
		public const string DebitTaskNotFound = "The {0} step of the {1} allocation rule cannot assign the debit task. The {2} task has not been found for the {3} project.";
		public const string CreditTaskNotFound = "The {0} step of the {1} allocation rule cannot assign the credit task. The {2} task has not been found for the {3} project.";
		public const string AccountGroupInBillingRuleNotFound = "The {0} billing rule has the {1} account group that has not been found in the system.";
		public const string AccountGroupInAllocationStepFromNotFound = "The {0} account group specified as the From setting of the {1} step of the {2} allocation rule has not been found in the system.";
		public const string AccountGroupInAllocationStepToNotFound = "The {0} account group specified as the To setting of the {1} step of the {2} allocation rule has not been found in the system.";
		public const string ProjectInTaskNotFound = "Task '{0}' has invalid Project associated with it. Project with the ID '{1}' was not found in the system.";
		public const string TaskNotFound = "Task with the given id was not found in the system. ProjectID='{0}' TaskID='{1}'";
		public const string ProjectNotFound = "Project with the given id was not found in the system. ProjectID='{0}'";
		public const string AutoAllocationFailed = "Auto-allocation of Project Transactions failed.";
		public const string AutoAllocationFailedForDocument = "The document ({0}) has been released but the auto-allocation process performed for the related project transactions has failed for a range of the transactions. See Trace for details.";
		public const string AutoReleaseFailed = "Auto-release of allocated Project Transactions failed. Please try to release this document manually.";
		public const string AutoReleaseAROnBillingFailed = "Auto-release of ARInvoice document created during billing failed. Please try to release this document manually.";
		public const string AutoReleaseAROnProrofmaReleaseFailed = "The system has failed to automatically release the AR document created on the release of a pro forma invoice. Try to release the AR document manually.";
		public const string AutoReleaseOfReversalFailed = "During Billing ARInvoice was created successfully. PM Reversal document was created successfully. Auto-release of PM Reversal document failed. Please try to release this document manually.";
		public const string SourceTransactionAccountIsNotConfiguredForBilling = "The {0} billing step of the {1} billing rule has failed to process the {2} {3} project transaction. The billing step is configured to use the sales account from the source transaction, but the debit account has not been specified in at least one line of this project transaction.";
		public const string BillingRuleAccountIsNotConfiguredForBilling = "The {0} billing rule has been configured to use the sales account from the billing rule, but the account has not been specified for the {0} billing rule.";
		public const string BillingRuleAccountIsNotConfiguredForBillingRecurent = "Recurring billing for the {0} task and the {1} item has been configured to use the account from the recurring item, but the account has not been specified for the recurring item.";
		public const string AccountGroupAccountIsNotConfiguredForBilling = "The {0} billing rule has been configured to use the sales account from the account group, but the default account has not been specified for the {1} account group.";
		public const string ProjectAccountIsNotConfiguredForBilling = "The {0} billing rule has been configured to use the sales account from the project, but the default sales account has not been specified for the {1} project.";
		public const string ProjectAccountIsNotConfiguredForBillingRecurent = "The {0} recurrent item has been configured to use the account from the project, but the default account has not been specified for the {1} project.";
		public const string TaskAccountIsNotConfiguredForBilling = "The {0} billing rule has been configured to get its account from the task, but the default sales account has not been configured for the {1} task of the {2} project.";
		public const string TaskAccountIsNotConfiguredForBillingRecurent = "Recurring billing for the {0} task and the {1} item has been configured to use the account from the task, but the default sales account has not been specified for the {0} task of the {2} project.";
		public const string InventoryAccountIsNotConfiguredForBilling = "The {0} billing rule has been configured to use the sales account from the inventory item, but the sales account has not been specified for the {1} inventory item.";
		public const string InventoryAccountIsNotConfiguredForBillingProjectFallback = "The {0} billing rule has been configured to use the sales account from the inventory item. In case the empty item code is specified for a project budget line, the default account of the project is used instead of the sales account of the inventory item, but no default account has been specified for the {1} project.";
		public const string InventoryAccountIsNotConfiguredForBillingRecurent = "Recurring billing for the {0} task and the {1} item has been configured to use the account from the inventory item, but the sales account has not been specified for the item.";
		public const string CustomerAccountIsNotConfiguredForBilling = "The {0} billing rule has been configured to use the sales account from the customer, but the sales account has not been specified for the {1} customer.";
		public const string CustomerAccountIsNotConfiguredForBillingRecurent = "The {0} recurring item has been configured to use the account from the customer, but the sales account has not been specified for the {1} customer.";
		public const string EmployeeAccountIsNotConfiguredForBilling = "The {0} billing rule has been configured to use the sales account from the employee, but the sales account has not been specified for the {1} employee.";
		public const string SubAccountCannotBeComposed = "Billing Rule '{0}' will not be able the compose the subaccount since account was not determined.";
		public const string EmployeeNotInProjectList = "Project is configured to restrict employees that are not in the Project's Employee list. Given Employee is not assigned to the Project.";
		public const string RateTypeNotDefinedForStep = "Rate Type is not defined for step {0}";
		public const string RateTypeNotDefinedForBilling = "The rate type is not defined for the '{1}' step of the '{0}' billing rule.";
		public const string RateNotDefinedForStep = "The @Rate is not defined for the {1} step of the {0} billing rule. Check Trace for details.";
		public const string RateNotDefinedForStepAllocation = "The @Rate is not defined for the {1} step of the {0} allocation rule. Check Trace for details.";
		public const string InactiveWorkCode = "The {0} workers' compensation code is not marked as Active on the Workers' Compensation Codes form.";
		public const string InactiveTask = "Project Task '{0}' is inactive.";
		public const string CompletedTask = "Project Task '{0}' is completed.";
		public const string TaskInvisibleInModule = "Project Task '{0}' is invisible in {1} module.";
		public const string InvisibleTask = "Project Task '{0}' is invisible.";
		public const string InactiveContract = "Given Project/Contract '{0}' is inactive";
		public const string CompleteContract = "Given Project/Contract '{0}' is completed";
		public const string TemplateContract = "Given Project/Contract '{0}' is a template";
		public const string InactiveUnion = "The {0} union local is inactive.";
		public const string ProjectInvisibleInModule = "The '{0}' project is invisible in the module.";
		public const string CancelledContract = "The {0} project or contract is canceled.";
		public const string DebitAccountGroupIsRequired = "Allocation Rule Step {0} is not defined correctly. Debit Account Group is required.";
		public const string AtleastOneAccountGroupIsRequired = "Allocation Rule Step {0} is not defined correctly. At least either Debit or Credit Account Group is required.";
		public const string DebitAccountEqualCreditAccount = "Debit Account matches Credit Account.";
		public const string DebitAccountGroupEqualCreditAccountGroup = "Debit Account Group matches Credit Account Group.";
		public const string AccountGroupIsRequired = "Failed to Release PM Transaction '{0}': Account Group is required.";
		public const string InvalidAllocationRule = "Allocation Step '{0}' is not valid. When applied to transactions in Task '{1}' failed to set Account Group. Please correct your Allocation rules and try again.";
		public const string PostToGLFailed = "Failed to Automatically Post GLBatch created during release of PM document.";
		public const string UnitConversionNotDefinedForItemOnBudgetUpdate = "Failed to Convert from {0} to {1} when updating the Budget for the Project. Unit conversion is not defined for {2}";
		public const string UnitConversionNotDefinedVerbose = "Project balance cannot be recalculated because conversion from {0} to {1} is not defined for the {2} item on the {3} form. Specify the unit conversion rule, and recalculate the project balance again.";
		public const string SourceSubNotSpecified = "Allocation rule is configured to use the source subaccount of transaction that is being allocated but the Subaccount is not set for the original transaction. Please correct your allocation step. Allocation Rule:{0} Step:{1}";
		public const string OffsetSubNotSpecified = "Allocation rule is configured to use the source subaccount of transaction that is being allocated but the Subaccount is not set for the original transaction. Please correct your allocation step. Allocation Rule:{0} Step:{1}";
		public const string StepSubNotSpecified = "Allocation rule is configured to use the subaccount of allocation step but the subaccount is not set up. Please correct your allocation step. Allocation Rule:{0} Step:{1}";
		public const string StepOffsetSubNotSpecified = "Allocation rule is configured to use the subaccount of allocation step but the subaccount is not set up. Please correct your allocation step. Allocation Rule:{0} Step:{1}";
		public const string ProjectCostSubNotSpecified = "The allocation rule is configured to use the cost subaccount from the project, but the cost subaccount is not specified for the {0} project.";
		public const string TaskCostSubNotSpecified = "The allocation rule is configured to use the cost subaccount from the project task, but the cost subaccount is not specified for the {0} task of the {1} project.";
		public const string ProjectSalesSubNotSpecified = "The allocation rule is configured to use the sales subaccount from the project, but the sales subaccount is not specified for the {0} project.";
		public const string TaskSalesSubNotSpecified = "The allocation rule is configured to use the sales subaccount from the project task, but the sales subaccount is not specified for the {0} task of the {1} project.";
		public const string OtherSourceIsEmpty = "Allocation rule is configured to take Debit Account from the source transaction and use it as a Credit Account of allocated transaction but the Debit Account is not set for the source transaction. Rule:{0} Step:{1} Transaction Description:{2}";
		public const string ProjectIsNullAfterAllocation = "In Step {0} Transaction that is processed has a null ProjectID. Please check the allocation rules in the preceding steps.";
		public const string TaskIsNullAfterAllocation = "In Step {0} Transaction that is processed has a null TaskID. Please check the allocation rules in the preceding steps.";
		public const string AccountGroupIsNullAfterAllocation = "In Step {0} Transaction that is processed has a null AllocationID. Please check the allocation rules in the preceding steps.";
		public const string StepSubMaskSpecified = "Subaccount Mask is not set in allocation step. Please correct your allocation step. Allocation Rule:{0} Step:{1}";
		public const string RateTableIsInvalid = "One or more validations failed for the given Rate Table sequence. Combinations of entities within sequence must be unique. The following combinations are not unique:";
		public const string ProjectRefError = "This record cannot be deleted. One or more projects are referencing this document.";
		public const string ValueMustBeGreaterThanZero = "The value must be greater than zero";
		public const string LocationNotFound = "Failed to create an allocation transaction. The location specified for the task is not valid. Check the following error for more details. {0}";
		public const string GenericFieldErrorOnAllocation = "Failed to create an allocation transaction. Check the following error for more details. {0}";
		public const string OtherUomUsedInTransaction = "Cannot set the UOM on the budget line. There already exists one or more transactions with a different UOM.";
		public const string UomNotDefinedForBudget = "The value of the Actual Qty. will not be updated if no UOM is defined.";
		public const string PrepaidAmountDecreased = "The Prepaid Amount can only be decreased from the auto assigned value.";
		public const string PrepaimentLessThanInvoiced = "The Prepaid Amount can not be decreased less than the already invoiced amount.";
		public const string ProjectExpired = "The project is expired.";
		public const string ProjectTaskExpired = "The project task is expired.";
		public const string TaskIsCompleted = "The project task is completed.";
		public const string PrepaymentAmointExceedsRevisedAmount = "The Prepaid Amount exceeds the uninvoiced balance.";
		public const string NoProgressiveRule = "The billing rule of the task contains only Time and Material steps. The Completed (%) and Pending Invoice Amount columns are not used for billing.";
		public const string UnreleasedProforma = "Pro Forma documents should be released in the sequence they were created. You cannot release the document until you release the following documents that precede the current one: {0}.";
		public const string GroupedAllocationsBillLater = "The selected option is not available when a line represents a group of allocated transactions.";
		public const string NonProjectCodeIsInvalid = "Non-Project is not a valid option.";
		public const string Overlimit = "The validation of the Max Limit Amount value has failed. Do one of the following: adjust the amounts of the document, adjust the limits of the budget, or select Ignore in the Validate T&M Revenue Budget Limits box on the Project Preferences (PM101000) form.";
		public const string OverlimitHint = "The validation of the Max Limit Amount has failed.";
		public const string UnreleasedPreviousInvoice = "You cannot release the pro forma invoice until you release the {1} {0} on the Invoices and Memos (AR301000) form.";
		public const string UnreleasedProformaOrInvoice = "All existing pro forma and Accounts Receivable invoices of the project have to be released before changing this setting.";
		public const string RevenueAccountIsNotMappedToAccountGroup = "Revenue Account {0} is not mapped to Account Group.";
		public const string AccountIsNotMappedToAccountGroup = "The {0} account is not mapped to any account group.";
		[Obsolete]
		public const string FailedToDetermineCostAccount = "Failed to get the cost account from the project settings.";
		public const string FailedToDetermineCostAccount2 = "The time card cannot be released because the expense account is not found. For details, see the trace log.";
		public const string FailedToDetermineAccrualAccount = "The time card cannot be released because the expense accrual account is not found. For details, see the trace log.";
		public const string SalesAccountIsNotMappedToAccountGroup = "The {0} billing step of the {1} billing rule failed. The {2} sales account, which is taken from the {3}, is not included in any account group.";
		public const string AccountIsNotAssociatedWithAccountGroup = "In the cost PM transaction emulated during the auto-budget process, the {0} debit account is not associated with the {1} account group.";
		public const string ReservedForProject = "Item reserved for Project Module to represent N/A item.";
		public const string NoBillingRule = "The invoice cannot be created because no billing rule is specified for the task.";
		[Obsolete]
		public const string InclusiveTaxNotSupported = "Inclusive taxes are not supported. ";
		public const string EmptyValuesFromExternalTaxProvider = AP.Messages.EmptyValuesFromExternalTaxProvider;
		public const string CommitmentAmtIsNegative = "The amount cannot be negative because the line has a non-zero quantity and the New Line or New Document status. Specify the zero quantity first.";
		[Obsolete]
		public const string CommitmentTotalIsNegative = "The change order cannot be released because the order total of the {0} {1} will go negative.";
		public const string CommitmentTotalIsNegativeSubcontract = "The change order cannot be released because the order total of the related subcontract ({0}) will become negative.";
		public const string CommitmentTotalIsNegativePurchaseOrder = "The change order cannot be released because the order total of the related purchase order ({0}) will become negative.";
		public const string NewCommitmentTotalIsNegative = "The change order cannot be released because the total amount of the lines with the New Document status on the Commitments tab must be greater than or equal to zero.";
		public const string NewCommitmentQtyIsNegative = "The quantity of the line with the New Line or New Document status cannot be negative.";
		public const string CommitmentQtyCannotbeDecreased = "The negative change cannot be applied because the value of the resulting document line cannot be negative or less than the received or billed value.";
		public const string NegativeCommitmentAmtCannotBeDecreased = "The change cannot be applied because the negative value of the Ext. Cost of the resulting document line must be less than or equal to the value of its Billed Amount.";
		public const string PositiveCommitmentAmtCannotBeDecreased = "The change cannot be applied because the positive value of the Ext. Cost of the resulting document line must be greater than or equal to the value of its Billed Amount.";
		public const string DuplicateChangeOrderNumber = "The project already has the {0} change order with this number.";
		public const string DuplicateProformaNumber = "The project already has the {0} pro forma invoice with this number.";
		public const string FailedGetFromAddress = "The system has failed to obtain the From address from the pro forma invoice.";
		public const string FailedGetToAddress = "The system has failed to obtain the To address from the pro forma invoice.";
		public const string CommitmentExistForThisProject_Enable = "Before you enable the change order workflow for the project, please make sure that all the related purchase order lines of the project have one of the following statuses: Completed, Closed, or Canceled.";
		public const string CommitmentExistForThisProject_Cancel = "Before canceling change order workflow for the project, please make sure that the project has no related non-canceled purchase order lines.";
		public const string ChangeOrderExistsForThisProject = "Before canceling change order workflow for the project, please make sure that the project has no related change orders.";
		public const string ChangeRequestExistsForThisProject = "Before canceling change order workflow for the project, please make sure that the project has no related change requests.";
		public const string ProjectCommintmentsLocked = "To be able to create original purchase order commitments for this project, perform the Unlock Commitments action for the project on the Projects (PM301000) form.";
		public const string ProjectCommintmentsLockedForSubcontracts = "To be able to create a subcontract for the {0} project, open the project on the Projects (PM301000) form and use the Unlock Commitments command on the More menu.";
		public const string ProjectCommintmentsLockedForPurchaseOrders= "To be able to create a purchase order for the {0} project, open the project on the Projects (PM301000) form and use the Unlock Commitments command on the More menu.";
		public const string CostCodeNotInBudget = "The {0} cost code is not present in the project budget with the combination of the {1} project task and the {2} account group.";
		
		public const string CostCodeInactiveWithFormat = "The {0} cost code is inactive.";
		public const string CannotDeleteDefaultCostCode = "This is a system record and cannot be deleted.";
		public const string CannotDeactivateCostCode = "The {0} cost code cannot be deactivated because it is currently in use for the {1} project.";
		public const string CannotDeactivateDefaultCostCode = "The default cost code cannot be deactivated.";
		public const string CannotModifyCostCode = "Cost code number cannot be updated directly. Use the Change ID action.";
		public const string ChangeOrderContainsRevenueBudget = "The change order class you are about to select does not support project revenue budget modification. Before disabling revenue budget modification for the change order, please make sure there are no change order lines affecting project revenue budget.";
		public const string ChangeOrderContainsCostBudget = "The change order class you are about to select does not support project cost budget modification. Before disabling cost budget modification for the change order, please make sure there are no change order lines affecting project cost budget.";
		public const string ChangeOrderContainsDetails = "The change order class you are about to select does not support project commitments modification. Before disabling commitments modification for the change order, please make sure there are no change order lines affecting project commitments.";
		public const string ClassContainsRevenueBudget = "Before disabling revenue budget modification for the change order class, please make sure there are no change orders belonging to this class that affect project revenue budget.";
		public const string ClassContainsCostBudget = "Before disabling cost budget modification for the change order class, please make sure there are no change orders belonging to this class that affect project cost budget.";
		public const string ClassContainsDetails = "Before disabling commitments modification for the change order class, please make sure there are no change orders belonging to this class that affect project commitments.";
		public const string ClassContainsCRs = "The change order class cannot be modified because it is already used in multiple entities.";
		public const string TaskReferencesRequiredAttributes = "The project tasks cannot be activated because required attributes of the tasks have no values. Please, use the Project Tasks (PM302000) form to fill in required attribute values.";
		public const string AtleastOneTaskWasNotActivated = "At least one task could not be activated. Please, review the list of errors.";
		public const string DuplicateProjectCD = "A project with the given Project ID already exists.";
		public const string DuplicateTemplateCD = "A template with the given Template ID already exists.";
		public const string QuoteConversionFailed = "The quote cannot be converted.";
		public const string InactiveAccountGroup = "The {0} account group is inactive. You can activate it on the Account Groups (PM201000) form.";
		public const string OpportunityBAccount = "The opportunity business account is not equal to the quote account of the project.";
		public const string ClosedQuoteCannotBeDeleted = "Closed quote cannot be deleted.";
		public const string CannotDeleteUsedTask = "Cannot delete a project task that is already in use on the Estimation tab.";
		public const string QuoteCannotBeLinkedToNotActiveOpportunity = "The project quote cannot be linked to an opportunity that is not active.";
		public const string QuoteBAccountIsNotACustomer = "You cannot convert the project quote to a project because the type of the business account of the project quote is not Customer. Select a business account of the Customer type to proceed.";
		public const string MissingExpenseAccountGroup = "The extended cost is non-zero and the cost account group is empty. The line cannot be converted to the project budget. You need to either specify the cost account group or set the extended cost to zero to be able to convert the quote to a project.";
		public const string MissingRevenueAccountGroup = "The amount is non-zero and the revenue account group is empty. The line is not printed in the quote and cannot be converted to the project budget. You need to either specify the revenue account group or set the amount to zero to be able to convert the quote to a project.";
		public const string TaskHasCostType = "The estimation line does not correspond to the type of the project task. Remove the revenue-related data in the estimation line or use a task of the cost and revenue type.";
		public const string TaskHasRevenueType = "The estimation line does not correspond to the type of the project task. Remove the cost-related data in the estimation line or use a task of the cost and revenue type.";
		public const string LicenseCostBudgetAndRevenueBudget = "The total number of lines on the Cost Budget and Revenue Budget tabs has exceeded the limit set for the current license. Please reduce the number of lines to be able to save the document.";
		public const string LicenseCostBudget = "The total number of lines on the Cost Budget tab has exceeded the limit set for the current license. To be able to save the project, reduce the number of the cost budget lines.";
		public const string LicenseRevenueBudget = "The total number of lines on the Revenue Budget tab has exceeded the limit set for the current license. To be able to save the project, reduce the number of the revenue budget lines.";
		public const string LicenseCommitments = "The number of lines on the Commitments tab has exceeded the limit set for the current license. Please reduce the number of lines to be able to save the document.";
		public const string LicenseProgressBillingAndTimeAndMaterial = "The total number of lines on the Progress Billing and Time and Material tabs has exceeded the limit set for the current license. Please reduce the number of lines to be able to save the document.";
		public const string LicenseTasks = "The total number of lines on the Tasks tab has exceeded the limit set for the current license. Please reduce the number of lines to be able to save the document.";
		public const string QuoteIsClosed = "The quote cannot be marked as the primary quote of the {0} opportunity because the opportunity is linked to the closed {1} project quote.";
		public const string BudgetLineCannotBeDeleted = "The line cannot be deleted because the project budget is locked. Please unlock the project budget and try again.";
		public const string LockedBudgetLineCannotBeUpdated = "Original budgeted values of the selected project budget line will not be updated because the project budget is locked.";
		public const string ProjectCuryCannotBeChanged = "The project currency cannot be changed because the project already has transactions.";
		public const string BaseCuryCannotBeChanged = "You cannot change the base currency for the project that has project transactions.";
		public const string CannotSelectCuryDiffersFromProjectBaseCury = "You cannot select a branch with the currency different from {0} for the project.";
		public const string CannotSelectCuryDiffersFromBranchBaseCury = "You cannot select a currency different from the currency of branch.";
		public const string BillingCuryCannotBeChanged = "Another billing currency is not supported because the project currency {0} differs from the base currency {1}.";
		public const string FxTranToProjectNotFound = "The conversion rate from the transaction currency {0} to the project currency {1} cannot be found for the {2} rate type and {3:d} date.";
		public const string BillingCurrencyCannotBeChanged = "An invoice can be created for the {0} project only in the billing currency of the project.";
		public const string CurrencyRateIsNotDefined = "Please define a conversion rate from the {0} to {1} currency within the {2} currency rate type and the {3:d} effective date on the Currency Rates (CM301000) form.";
		public const string ConversionRateNotDefinedForCommitment = "The commitment cannot be created because the conversion rate from the purchase order currency {0} to the project currency {1} cannot be found for the {2} rate type and {3:d} date.";
		public const string FinPeriodForDateNotFound = "The financial period that corresponds to the date of at least one activity of a time card does not exist. Please, generate needed financial periods on the Master Financial Calendar (GL201000) form.";
		public const string OrderIsNotApproved = "The purchase order is pending approval. Please, approve the order first.";
		public const string OrderIsOnHold = "The purchase order is on hold. You need to first clear the On Hold check box for the purchase order.";
		public const string ActiveMigrationMode = "The operation is not available because the migration mode is enabled for accounts receivable.";
		public const string NoPermissionForInactiveTasks = "You have no permission to use project tasks with the Completed, Canceled, or In Planning status for data entry.";
		public const string ChnageOrderInvalidDate = "The financial period for the selected {0} date is not defined in the system. The change date is used for balance calculation and must belong to an existing financial period of the master calendar.";
		public const string ProformaDeletingRestriction = "You cannot delete the document with lines associated with the branches to which your user has no sufficient access rights.";
		public const string ProformaLineDeletingRestriction = "You cannot delete the line linked to a project transaction associated with the branch to which your user has no sufficient access rights.";
		public const string ChangeRetainageMode = "To enable the creation of pro forma invoices, the retainage mode must be changed first.";
		public const string CreateProformaRequired = "To select the Contract Cap mode, the creation of pro forma invoices must be enabled first.";
		public const string DuplicateID = "Duplicate ID";
		public const string EffectiveDateShouldBeGreater = "The effective date should be greater than {0}.";
		public const string EffectiveDateShouldBeLess = "The effective date should be less than {0}.";
		public const string NeedUpdate = "Need update instead of insert";
		public const string ExcludedFromBillingAsCreditMemoWrittenOff = "Written-Off with Credit Memo {0}";
		public const string ExcludedFromBillingAsCreditMemoResult = "Result of Credit Memo {0}";
		public const string ExcludedFromBillingAsARInvoiceResult = "Result of AR Invoice {0}";
		public const string ExcludedFromBillingAsReversal = "Reversal of Tran. ID {0}";
		public const string ExcludedFromBillingAsReversed = "Reversed";
		public const string ExcludedFromBillingAsWIPReversed = "WIP Reversed";
		public const string ExcludedFromBillingAsBillableWithCase = "Billable with Case {0}";
		public const string CannotCorrectContainsTM = "You cannot make corrections to the pro forma invoice because it has at least one time and material line.";
		public const string CannotDeleteAccountMappedToAG = "You cannot delete the account associated with an account group. Delete the {0} account from the {1} account group on the Account Groups (PM201000) form first.";
		public const string CannotCorrectContainsUnreleasedRetainage = "You cannot correct the pro forma invoice because the corresponding AR document has at least one unreleased retainage document. Release or delete the following retainage documents first: {0}.";
		public const string CannotCorrectContainsApplications = "You cannot correct the pro forma invoice because the corresponding AR document has at least one applied document. Unapply or void the following applications related to the {0} AR document first: {1}.";
		public const string CannotCorrectContainsRetainageBalance = "You cannot correct the pro forma invoice because the corresponding AR document has at least one released retainage document that has not been reversed. To be able to make corrections, make the Original Retainage and Unreleased Retainage amounts equal in the {1} invoice first, either by reversing the retainage documents ({0}) or by creating the necessary document applications.";
		public const string CorrectRetainageWarning = "The retainage mode of the project is {0}{1}. Making corrections to the pro forma invoice for the project may cause discrepancies in the calculation of the retainage amount in subsequent pro forma invoices and retainage values of the project on the Revenue Budget tab of the Projects (PM301000) form. You may be required to correct the following pro forma invoices to ensure the accurate recalculation of the retainage: {2}. Click OK to proceed.";
		public const string CannotReleaseRetainage = "The system cannot release retainage from the invoice {0} because the related pro forma invoice {1} is under correction. To be able to release the invoice {0}, release the pro forma invoice {1} on the Pro Forma Invoices (PM307000) form first.";
		public const string CannotPreparePayment = "The system cannot prepare payment for the invoice {0} because the related pro forma invoice {1} is under correction. To be able to prepare a payment for the invoice {0}, release the pro forma invoice {1} on the Pro Forma Invoices (PM307000) form first.";
		public const string CannotReverseInvoice = "The system cannot reverse the invoice {0} or apply it to a credit memo because the related pro forma invoice {1} is under correction. To be able to reverse the invoice {0} or apply it to a credit memo, release the pro forma invoice {1} on the Pro Forma Invoices (PM307000) form first.";
		public const string InvoiceWithProformaReverseWarning = "If you reverse the document manually, you will not be able to create another accounts receivable invoice for the {0} pro forma invoice from which the current document originates. To reverse accounts receivable invoice along with the pro forma invoice, use the Correct action on the Pro Forma Invoices (PM307000) form instead.\r\nAre you sure you want to proceed with manual reversal?";
		public const string InvoiceWithProformaCreditMemoApplyWarning = "If you apply the {1} credit memo to the {2} invoice, you will not be able to create another accounts receivable invoice for the {0} pro forma invoice from which the {2} invoice originates. To reverse the accounts receivable invoice along with the pro forma invoice, use the Correct action on the Pro Forma Invoices (PM307000) form instead.\r\nAre you sure you want to proceed with manual reversal?";
		public const string InvoiceWithProformaReverseDialogHeader = "Manual AR Document Reversal";
		public const string AIAIsOutdated = "The AIA report should be reprinted.";
		public const string ArDocShouldBereleasedBeforeCorrection = "To correct the pro forma invoice, delete the associated {0} AR document first.";
		public const string LastApplicationNbrIsInvalid = "The pro forma invoice has not been generated as the application number is not valid. Change the Last Application Nbr. of the project.";
		public const string LastApplicationNbrIncreaseLength = "The pro forma invoice cannot be created because the automatically generated Application Nbr. exceeds the length limit ({0} symbols). Correct the Last Application Nbr. of the {1} project, and run project billing again.";
		public const string RecalculateBalanceTooltip = "Recalculate project balances, including actual, committed, and change order buckets";
		public const string CannotDeleteAccountFromAccountGroup = "The account cannot be deleted from the account group because it is selected as the Debit Account in the Unbilled Remainders section on the Project Preferences (PM101000) form.";
		public const string ReportsNotSupported = "The AIA Report ({0}) and AIA Report with Quantity ({1}) reports cannot be used in mailing settings. These reports are to be generated only by clicking the AIA Report button on the form toolbar of the Projects (PM301000) and Pro Forma Invoices (PM307000) forms.";
		public const string OneOrMoreLinesFailedAddOnTheFlyValidation = "At least one line is not present in the cost budget of the project. Either add the corresponding budget lines to the project or select the Allow Adding New Items on the Fly check box on the Summary tab of the Projects (PM301000) form.";
		public const string EmptyCostCodeRange = "{0} or {1} must have a value.";
		public const string QuantityAvailableforIssue = "The quantity available for issue from the selected project location is {0} {1}.";
		public const string OverlappingCostCode = "One cost code cannot be associated with multiple workers' compensation codes.";
		public const string CostCodeToGreaterThanFrom = "The value for {0} must be equal to or greater than the value for {1}.";
		public const string DuplicateWorkCodeLaborItem = "One labor item cannot be associated with multiple workers' compensation codes.";
		public const string DuplicateWorkCodeProjectTask = "One project/task combination cannot be associated with multiple workers' compensation codes.";
		[Obsolete]
		public const string TheGrossTaxCalcModeForBAccountIsNotSupportedInPQuotes = "The gross tax calculation mode specified for the location of the selected business account is not supported in project quotes. Change the tax calculation mode of the location of the selected business account, or select another location on the Billing Info tab of this form, or select another business account.";
		[Obsolete]
		public const string TheGrossTaxCalcModeForCustomerIsNotSupportedInProFormaInvoices_TraceError = "To avoid the error, you can do any of the following:" +
			"\n- Clear the Create Pro Forma Invoice on Billing check box on the Summary tab of the Projects (PM301000) form before billing the project" +
			"\n- Change the tax calculation mode of the {0} location on the Shipping tab of the Customer Locations (AR303020) form." +
			"\n- Select another location for the project task on the Summary tab of the Project Tasks (PM302000) form." +
			"\n- Select another location for the default location on the Summary tab of the Projects (PM301000) form if no location is defined at the project task level." +
			"\n- Select another location for the default location on the Locations tab of the Customers (AR303000) form if no location is defined at the project task and project level.";
		[Obsolete]
		public const string TheGrossTaxCalcModeForCustomerIsNotSupportedInProFormaInvoices = "The gross tax calculation mode specified for the {0} default location of the {1} customer is not supported in pro forma invoices. Clear the Create Pro Forma on Billing check box on the Summary tab of the Projects (PM301000) form before billing the project or change the tax calculation mode of the {2} location on the Shipping tab of the Customer Locations (AR303020) form. You can also select another location for the project task on the Summary tab of the Project Tasks (PM302000) form, or the default location on the Summary tab of the Projects (PM301000) form if no location is defined at the project task level, or the default location on the Locations tab of the Customers (AR303000) form if no location is defined at the project task and project level.";
		[Obsolete]
		public const string TheGrossTaxCalcModeForCustomerIsNotSupportedInProFormaInvoicesChangeTaxCalModeOrLocation = "The gross tax calculation mode specified for {0} location of the {1} customer selected in the Summary area is not supported in pro forma invoices. Either select another location or change the tax calculation mode of the {2} location on the Shipping tab of the Customer Locations (AR303020) form.";
		public const string ProjectTraceItem = "Project: {0}";
		public const string ProjectTaskTraceItem = "Project: {0} Task: {1}";
		public const string ProjectDropShipPostExpensesUpdateGLInactive = "The On Receipt Release option cannot be selected because the Update GL check box is cleared on the Inventory Preferences (IN101000) form.";
		public const string POReceiptWithProjectIsNotActive = "The purchase receipt cannot be added to the AP bill because at least one receipt line has an inactive project associated. See the Trace for details.";
		public const string LinkedProjectNotValid = "The {0} project cannot be selected because the {1} warehouse location specified in the line is not linked to this project. Select another project, or change the warehouse location.";
		public const string ModeNotValid_Linked = "You cannot change the inventory tracking method because there is at least one warehouse location linked to the project. You must unlink all warehouse locations from the project on the Warehouses (IN204000) form first. See the trace log for details.";
		public const string ModeNotValid_ItemsOnHand = "You cannot change the inventory tracking method because there are items on hand in warehouse locations linked to the project. You must issue or transfer all items from these warehouse locations and unlink all warehouse locations from the project first. See the trace log for details.";
		public const string InvalidProject_AccountingMode = "A warehouse location can be linked only to projects in which Track by Location is selected in the Inventory Tracking box on the Projects (PM301000) form. Update the settings of the {0} project to be able to select it, or select another project.";
		public const string POReceiptWithProjectTaskIsNotActive = "The purchase receipt cannot be added to the AP bill because at least one receipt line has an inactive project task associated. See the Trace for details.";
		public const string POReceiptWithProjectIsNotActiveTraceCaption = "The purchase receipt cannot be added to the AP bill because at least one receipt line has an inactive project associated.";
		public const string POReceiptWithProjectTaskIsNotActiveTraceCaption = "The purchase receipt cannot be added to the AP bill because at least one receipt line has an inactive project task associated.";
		[Obsolete]
		public const string POOrderWithProjectIsNotActive = "The purchase order cannot be added to the AP bill because at least one order line has an inactive project associated. See the Trace for details.";
		[Obsolete]
		public const string POOrderWithProjectTaskIsNotActive = "The purchase order cannot be added to the AP bill because at least one order line has an inactive project task associated. See the Trace for details.";
		[Obsolete]
		public const string POOrderWithProjectIsNotActiveTraceCaption = "The purchase order cannot be added to the AP bill because at least one order line has an inactive project associated.";
		[Obsolete]
		public const string POOrderWithProjectTaskIsNotActiveTraceCaption = "The purchase order cannot be added to the AP bill because at least one order line has an inactive project task associated.";
		public const string ProjectStatusIs = "The {0} project cannot be selected because it has the {1} status.";
		public const string LinesAreHidden = "One or multiple lines are hidden in the document because you do not have permissions to view them. This value is calculated based on the displayed lines.";
		public const string AccountInRestrictedAG = "The account is included in the account group for which you do not have permissions to operate. Select another account.";
		public const string CannotReleaseTran = "The transaction cannot be released because it includes one or multiple lines that are hidden; you do not have permissions to view and edit these lines.";
		public const string CannotReverseAllocation = "The allocation cannot be reversed because it includes one or multiple lines that are hidden; you do not have permissions to view and edit these lines.";
		public const string BranchCanNotBeDeletedBecauseUsedInProjects = "The {0} branch cannot be deleted because it is used in the following projects or project templates: {1}.";
		[Obsolete]
		public const string LaborCostRateIsDuplicated = "A row with the same effective date and similar settings already exists. You may need to change the effective date, review and update the settings specified in this row, or delete the row before saving the changes.";
		public const string CreditMemoCannotReleaseWithOutAppropriateAccount = "The credit memo related to the {0} project cannot be released. Map the {1} account to an appropriate account group and try again.";
		public const string ProgressBillingBaseNotValid = "You must specify Progress Billing Base in the following budget lines on the Revenue Budget tab: {0}";
		public const string CreateCOProjectStatusError = "A change order cannot be created for the project with the {0} status.";
		public const string ReleaseCOProjectStatusError = "A change order cannot be released for the project with the {0} status.";
		public const string ActualQtyUomConversionNotFound = "The quantity cannot be calculated because the unit conversion from {0} to {1} is not defined.";
		public const string InvalidProjectCode = "The {0} identifier is specified as the non-project code on the Projects Preferences (PM101000) form. Specify another identifier.";
		public const string CommitmentRetainagesDiffersFromDefaultWarning = "The retainage percent in the line ({0}) differs from the retainage percent specified in the commitment document ({1}). On release of the change order, the retainage percent and retainage amount in the related commitment line will be changed to the values specified in the change order line.";

		public const string POCommitmentProjectIsNotActive = "The purchase order cannot be added because it contains one or more lines related to the project that has the status different from Active.";
		public const string POCommitmentProjectTaskIsNotActive = "The purchase order cannot be added because it contains one or more lines related to the project task that has the status different from Active.";
		public const string POCommitmentLineProjectIsNotActive = "The purchase order line cannot be added because it is related to the {1} project that has the status different from Active.";
		public const string POCommitmentLineProjectTaskIsNotActive = "The purchase order line cannot be added because it is related to the {1} project task that has the status different from Active.";
		public const string SCCommitmentProjectIsNotActive = "The subcontract cannot be added because it contains one or more lines related to the project that has the status different from Active.";
		public const string SCCommitmentProjectTaskIsNotActive = "The subcontract cannot be added because it contains one or more lines related to the project task that has the status different from Active.";
		public const string SCCommitmentLineProjectIsNotActive = "The subcontract line cannot be added because it is related to the {1} project that has the status different from Active.";
		public const string SCCommitmentLineProjectTaskIsNotActive = "The subcontract line cannot be added because it is related to the {1} project task that has the status different from Active.";

		public const string PWProjectIsNotActive = "The {0} project is not active.";
		public const string PWProjectTaskNotActive = "The {0} project task has the {1} status and cannot be selected. Select another project task.";
		public const string PWAccountGroupNotActive = "The {0} account group is not active.";
		public const string PWDublicateLine = "A line with the same project budget key already exists.";
		public const string PWDateInvalidFinperiod = "The financial period for the selected {0} date is not defined in the system. The date must belong to an existing financial period of the master calendar.";
		public const string PWDateFinperiodInactive = "The {0} financial period is inactive in the {1} company.";
		public const string PWDateFinperiodClosed = "The {0} financial period is closed in the {1} company.";
		public const string PWCostBudgetNotExist = "A line with the specified project budget key has not been found in the cost budget of the {0} project.";
		public const string BranchCannotDeleteReferencedByProforma = "The {0} branch cannot be deleted because it is used in the following pro forma invoices: {1}.";
		public const string BranchCannotDeactiveReferencedByProforma = "The {0} branch cannot be deactivated because it is used in the following pro forma invoices: {1}.";
		public const string ProformaCannotRealeaseWithDeletedBranch = "The pro forma invoice cannot be released because the {0} branch is inactive or does not exist in the system. To be able to process the pro forma invoice, activate or create the {0} branch on the Branches (CS102000) form.";
		public const string ChangeOrderReportEmailingError = "Email cannot be created for the {0} change order because the related printed form {1} that must be attached to the email contains no lines. You can modify the {1} printed form in the Report Designer to include cost and commitment lines to the report as well, or specify another report for the CHANGE ORDER mailing ID on the Mailing & Printing tab of the Project Preferences (PM101000) form.";
		public const string ProjectIsBilledWithProformaARInvoiceWarning = "The selected project is billed using pro forma invoices. Manually created AR document will not be tracked in the project budget on the Projects (PM301000) form and will not be shown in the project-related reports.";
		public const string AdjustmentCannotBeReleasedDueToInactiveTaskInOneProject = "The adjustment cannot be released because at least one inventory transaction related to the {0} project has the inactive {1} project task associated. You can activate the project task on the Projects (PM301000) or Project Tasks (PM302000) forms.";
		public const string AdjustmentCannotBeReleasedDueToInactiveTasksInOneProject = "The adjustment cannot be released because some inventory transactions related to the {0} project have the following inactive project tasks associated: {1}. You can activate the tasks on the Projects (PM301000) or Project Tasks (PM302000) forms.";
		public const string AdjustmentCannotBeReleasedDueToInactiveTasksInManyProjects = "The adjustment cannot be released because some inventory transactions have inactive project tasks associated. You can activate the project tasks on the Projects (PM301000) or Project Tasks (PM302000) forms. See the Trace for details.";

		public const string PMBillingRuleDuplicateAccountGroup = "The billing rule already includes a billing step for the {0} account group. Select another account group or deactivate other steps configured for this account group.";
		public const string ProjectCostLayerAndProjectNotTrackedByCost = "The cost layer of the Project type cannot be used for the {0} project because of the project inventory tracking settings.";
		public const string NotProjectCostLayerAndProjectTrackedByCost = "The cost layer of the Normal or Special type cannot be used for the {0} project because of the project inventory tracking settings.";
		public const string MixedLocationsAreNotAllowed = "The issue transaction cannot be saved because the {0} project selected in the line {1} has the Track by Project Quantity and Cost inventory tracking mode, and different warehouse locations have been selected in the line splits for this line. Select the same warehouse location in all line splits for the line.";
		public const string WarehouseLocationProjectTaskShouldBeFilled = "The Warehouse, Location, Project, and Project Task columns cannot be empty in lines with the cost layer of the Project type.";
		public const string DestWarehouseLocationProjectTaskShouldBeFilled = "The Destination Warehouse, Location, Project, and Project Task columns cannot be empty in lines with the cost layer of the Project type.";
		[Obsolete]
		public const string NothingToShipTraced_Linked = "The {1} sales order cannot be shipped in full. There are not enough stock items available for the {2} project at the {3} warehouse. To be able to create a shipment, transfer or receive the necessary quantity of the items to the warehouse location linked to the {2} project.";
		[Obsolete]
		public const string NothingToShipTraced_NotLinked = "The {1} sales order cannot be shipped in full. There are not enough project-specific stock items available at the {3} warehouse for the {2} project. To be able to create a shipment, transfer or receive the necessary quantity of the items for the {2} project.";
		public const string NothingToShipTraced = "The {0} sales order cannot be shipped in full or does not contain any items planned for shipment on the {3} date. Make sure the shipment date is not later than the Ship On date. If there are not enough available stock items, then transfer or receive the necessary quantity of items for the {2} project to the {1} warehouse.";

		public const string DeactivateMigrationModeToReleaseProforma = "The document cannot be processed because it was created when migration mode was deactivated. To process the document, clear the Activate Migration Mode check box on the Projects Preferences (PM101000) form.";
		public const string ActivateMigrationModeToReleaseProforma = "The document cannot be processed because it was created when migration mode was activated. To process the document, activate migration mode on the Projects Preferences (PM101000) form.";
		public const string CannotActivateProjectTaskWithInactiveCostCode = "The {0} project task of the {1} project cannot be activated because it is used with the inactive cost code.";
		public const string CannotActivateTaskWithInactiveCostCode = "The {0} project task cannot be activated because it is used with the inactive cost code.";
		public const string ConvertToProjectRaisedError = "Inserting 'Project' record raised at least one error. {0} out of {1} records contain errors. Please review the errors.";


		public const string ProjectAlreadyAssigned = "The {0} project is already assigned to the {1} warehouse location of the {2} warehouse.";
		public const string SameProjectTaskCombination = "The {0} project is already assigned to the {1} warehouse location of the {2} warehouse. To be able to use the same project with multiple warehouse locations, assign different project tasks to each of these locations.";
		public const string ProjectTaskAlreadyAssigned = "The {0} project task is already assigned to the {1} warehouse location of the {2} warehouse.";
		public const string ProjectQuoteIncorrectProjectCD = "'{0}' is not a valid value for the {1}-{2} segment of the {3} segmented key. Either add this segment value to the list of segment values on the Segment Values (CS203000) form or change it to the valid one.";
		public const string ProjectQuoteEmptySegmentOfProjectCD = "The {0}-{1} segment of the New Project ID cannot be empty.";
		public const string ProjectCustomerDontMatchThePayment = "The customer of the project differs from the customer of the payment.";
		public const string CannotRenameProjectGroup = "The {0} project group cannot be renamed because at least one project or project template is mapped to this project group.";
		public const string InsufficientAccessToProjectGroup = "You cannot access the {0} project group.";
		public const string ProjectGroupIsInactive = "The {0} project group is inactive.";

		#endregion

		#region Translatable Strings used in the code
		public const string ViewTask = "View Task";
		public const string ViewProject = "View Project";
		public const string ViewExternalCommitment = "View External Commitment";
		public const string OffBalance = "Off-Balance";
		public const string SetupNotConfigured = "Project Management Setup is not configured.";
		public const string ViewCommitments = "View Commitment Details";
		public const string ViewRates = "View Rates";
		public const string NonProjectDescription = "Non-Project Code.";
		public const string ProcAllocate = "Allocate";
		public const string ProcAllocateAll = "Allocate All";
		public const string Release = "Release";
		public const string ReleaseAll = "Release All";
		public const string ViewTransactions = "View Transactions";
		public const string PrjDescription = "Project Description";
		public const string Bill = "Run Project Billing";
		public const string BillTip = "Runs billing for the Next Billing Date";
		public const string ActivateTasks = "Activate Tasks";
		public const string CompleteTasks = "Complete Tasks";
		public const string AddCommonTasks = "Add Common Tasks";
		public const string AddCTasksAndClose = "Add Tasks & Close";
		public const string CreateTemplate = "Create Template";
		public const string AutoBudget = "Auto-Budget Revenue";
		public const string AutoBudgetTip = "Creates projected budget based on the expenses and Allocation Rules";
		public const string Actions = "Actions";
		public const string Reverse = "Reverse";
		public const string ReverseAllocation = "Reverse Allocation";
		public const string ReverseAllocationTip = "Reverses Released Allocation";
		public const string ViewAllocationSource = "View Allocation Source";
		public const string ViewBatch = "View Batch";
		public const string ProjectSearchTitle = "Project: {0} - {2}";
		public const string ChangeOrderSearchTitle = "Change Order: {0}";
		public const string ChangeRequestSearchTitle = "Change Request: {0}";
		public const string ProformaSearchTitle = "Pro Forma Invoice: {0} - {2}";
		public const string ProgressWorksheetSearchTitle = "Progress Worksheet: {0}";
		public const string AccountGroup = "Account Group";
		public const string RateCode = "Rate Code";
		public const string Task = "Task";
		public const string TaskTotal = "Task Total";
		public const string Item = "Item";
		public const string FailedEmulateBilling = "The billing emulation cannot be run because of incorrect configuration. The sales account in the invoice is not mapped to any account group.";
		public const string InvalidScheduleType = "The schedule type is invalid.";
		public const string ArgumentIsNullOrEmpty = CR.Messages.ArgumentIsNullOrEmpty;
		public const string AllocationForProject = "Allocation for {0}";
		public const string AllocationReversalOnARInvoiceGeneration = "Allocation Reversal on AR Invoice Generation";
		public const string AllocationReversalOnARInvoiceRelease = "Allocation Reversal on AR Invoice Release";
		public const string ProjectAttributeNotSupport = "ProjectAttribute does not support the given module.";
		public const string ProjectTaskAttributeNotSupport = "ProjectTaskAttribute does not support the given module.";
		public const string TaskIdEmptyError = "Task ID cannot be empty.";
		public const string TaskCannotBeSaved = "The {0} task cannot be saved. Please save the task with the Planned status first and then change the status to Active.";
		public const string CreateCommitment = "Create External Commitment";
		public const string ViewProforma = "Pro Forma Invoice";
		public const string ViewPMTrans = "Project Transactions";
		public const string Prepayment = "Prepayment";
		public const string Total = "Total:";
		public const string PMTax = "PM Tax Detail";
		public const string PMTaxTran = "PM Tax";
		public const string ViewChangeOrder = "View Change Order";
		public const string ViewReversingChangeOrder = "View Reversing Change Order";
		public const string ChangeOrderPrefix = "Change Order #{0}";
		public const string ViewPurchaseOrder = "View Purchase Order";
		public const string NotAvailable = @"N/A";
		public const string RetaiangeChangedDialogHeader = "Default Retainage (%) Changed";
		public const string RetaiangeChangedDialogQuestion = "Update Retainage (%) in the revenue budget lines?";
		public const string RetaiangeChangedCustomerDialogQuestion = "Changing Customer will update the default project Retainage (%) from {0:f} to {1:f}. Would you also like to update Retainage (%) in the revenue budget lines?";
		public const string ReleaseRetainage = "Release Retainage";
		public const string QuoteNumberingIDIsNull = "The quote numbering sequence is not specified in the preferences of the Projects module.";
		public const string RunAllocation = "Run Allocation";
		public const string ConvertToProject = "Convert to Project";
		public const string ValidateBalance = "Recalculate Project Balance";
		public const string TemplateChangedDialogQuestion = "Replace quote settings with the settings of the project template? The project tasks, attributes, and project manager will be replaced.";
		public const string AssetTotals = "Asset Totals";
		public const string LiabilityTotals = "Liability Totals";
		public const string IncomeTotals = "Income Totals";
		public const string ExpenseTotals = "Expense Totals";
		public const string OffBalanceTotals = "Off-Balance Totals";
		public const string SubmitProjectForApproval = "Submit Project for Approval";
		public const string Aggregated = "Aggregated: ";
		public const string UpdateQuoteByTemplateDialogHeader = "Update Quote by Template";
		public const string ViewBase = "View Base";
		public const string ViewCury = "View Cury";
		public const string CreatePurchaseOrder = "Create Purchase Order";
		public const string NewKey = "<NEW>";
		public const string WithSteps = " with steps";
		public const string CorrectionReversal = "The reversal originating from corrections of the {0} pro forma invoice";
		public const string RevenueBudgetFilter = "Revenue Budget Filter";
		public const string CostBudgetFilter = "Cost Budget Filter";
		public const string ProjectBillingRecord = "Project Billing Record";
		public const string ProjectUnionLocals = "Project Union Locals";
		public const string BudgetForecasting = "Budget Forecasting";
		public const string PrintingAndEmailing = "Printing and Emailing";
		public const string Processing = "Processing";
		public const string ToProject = "To Project";
		public const string ToProjectTask = "To Project Task";
		public const string ToCostCode = "To Cost Code";
		public const string Corrections = "Corrections";
		public const string Other = "Other";
		public const string ProductivityTypeNotAllowed = "Not Allowed";
		public const string ProductivityTypeTemplate = "Template";
		public const string ProductivityTypeOnDemand = "On Demand";
		#endregion

		#region Graph Names
		public const string ProjectTaskEntry = "Project Task Entry";
		public const string CommitmentEntry = "Commitment Entry";
		public const string RateMaint = "Rate Maintenance";
		public const string Process = "Process";
		public const string ProcessAll = "Process All";
		public const string TransactionInquiry = "Transactions Inquiry";
		public const string PMSetup = "Project Management Setup";
		public const string PMSetupMaint = "Project Preferences";
		#endregion

		#region View Names
		public const string Selection = "Selection";
		public const string ProjectAnswers = "Project Answers";
		public const string TaskAnswers = "Task Answers";
		public const string AccountGroupAnswers = "Account Group Answers";
		public const string QuoteAnswers = "Quote Answers";
		public const string PMTasks = "Tasks";
		public const string Approval = "Approval";
		public const string Commitments = "Commitments";
		public const string Budget = "Budget";
		public const string BudgetProduction = "Budget Production";
		public const string UnionLocals = "Union Locals";
		public const string Estimates = "Estimates";
		public const string RecurringItem = "Recurring Items";
		public const string ChangeRequest = "Change Request";
		public const string Markup = "Markup";
		public const string ContractTotal = "Contract Total";
		public const string PMAddress = "PM Address";
		public const string PMContact = "Project Contact";
		public const string SiteAddress = "Site Address";
		public const string ProgressWorksheet = "Progress Worksheet";
		public const string CostBudgetLineFilter = "Cost Budget Line Filter";
		#endregion

		#region DAC Names
		public const string Project = "Project";
		public const string ProjectTask = "Project Task";
		public const string PMBudget = "Project Budget";
		public const string PMRevenueBudget = "Project Revenue Budget";
		public const string PMCostBudget = "Project Cost Budget";
		public const string PMTaskRate = "PM Task Rate";
		public const string PMProjectRate = "PM Project Rate";
		public const string PMItemRate = "PM Item Rate";
		public const string PMEmployeeRate = "PM Item Employee";
		public const string PMRate = "Rate";
		public const string PMRateType = "Rate Type";
		public const string PMRateTable = "Rate Table";
		public const string PMProjectTemplate = "Project Template";
		public const string PMRateDefinition = "Rate Lookup Rule";
		public const string PMRateSequence = "Rate Lookup Rule Sequence";
		public const string PMAccountTask = "PM Account Task";
		public const string PMAccountGroupRate = "PM Account Group Rate";
		public const string PMAllocationDetail = "Allocation Rule Step";
		public const string PMAllocationSourceTran = "PM Allocation Source Transaction";
		public const string PMAllocationAuditTran = "PM Allocation Audit Transaction";
		public const string SelectedTask = "Tasks for Addition";
		public const string PMAccountGroupName = "Account Group";
		public const string PMTran = "Project Transaction";
		public const string PMProjectStatus = "Project Status";
		public const string PMHistory = "Project History";
		public const string CostCode = "Cost Code";
		public const string Proforma = "Pro Forma Invoice";
		public const string ProformaRevision = "Pro Forma Invoice Revision";
		public const string ProformaLine = "Pro Forma Line";
		public const string ProgressWorksheetLine = "Progress Worksheet Line";
		public const string PMRegister = "Project Register";
		public const string BillingRule = "Billing Rule";
		public const string BillingRuleStep = "Billing Rule Step";
		public const string AllocationRule = "Allocation Rule";
		public const string Commitment = "Commitment Record";
		public const string ChangeOrderClass = "Change Order Class";
		public const string ChangeOrder = "Change Order";
		public const string ChangeOrderLine = "Change Order Line";
		public const string ReversingChangeOrder = "Reversing Change Order";
		public const string PMLaborCostRate = "Labor Cost Rates";
		public const string PMForecast = "Budget Forecast";
		public const string PMForecastDetail = "Budget Forecast Detail";
		public const string PMForecastHistory = "Budget Forecast History";
		public const string ProformaInfo = "Proforma Info for AIA Report";
		public const string PMProjectBalance = "Project Balance";
		public const string ProformaLineInfo = "Proforma Line Info for AIA Report";
		public const string PMWorkCodeCostCodeRange = "Workers' Compensation Code Cost Code Range";
		public const string PMReportRowsMultiplier = "Report Rows Multiplier";
		public const string PMWorkCodeProjectTaskSource = "Workers' Compensation Project Task Source";
		public const string PMWorkCodeLaborItemSource = "Workers' Compensation Labor Item Source";
		public const string PurchaseOrder = "Purchase Order";
		public const string Subcontract = "Subcontract";
		public const string ProjectMustBeSpecified = "The Project must be specified.";
		public const string PMSiteStatus = "Project Site Status";
		public const string PMSiteSummaryStatus = "Project Summary Site Status";
		public const string PMLocationStatus = "Project Location Status";
		public const string PMLotSerialStatus = "Project Lot/Serial Status";
		public const string CostCenter = "Project Cost Center";
		public const string DateFromMustBeSpecified = "The date From must be specified.";
		public const string DateToMustBeSpecified = "The date To must be specified.";
		public const string DocumentTypeMustBeSpecified = "The Document Type must be specified.";
		public const string RecalculateProjectBalances = "Recalculate Project Balances";
		public const string PMProjectGroup = "Project Group";

		public const string PMReportProject = "PM Report Project";
		public const string PMWipTotalBudget = "PM WIP Total Budget";
		public const string PMWipCostProjection = "PM WIP Cost Projection";
		public const string PMWipCostProjectionBudget = "PM WIP Cost Projection Budget"; 
		public const string PMWipBudget = "PM WIP Budget";
		public const string PMWipForecastHistory = "PM WIP Forecast History";
		public const string PMWipTotalForecastHistory = "PM WIP Total Forecast History";
		public const string PMWipDetailTotalForecastHistory = "PM WIP Detail Total Forecast History";
		public const string PMWipChangeOrder = "PM WIP Change Order";
		public const string PMWipChangeOrderBudget = "PM WIP Change Order Budget";
		#endregion

		#region Combo Values
		public const string Active = "Active";
		public const string Canceled = "Canceled";
		public const string Completed = "Completed";
		public const string InPlanning = "In Planning";
		public const string OnHold = "On Hold";
		public const string Suspend = "Suspended";
		public const string PendingApproval = "Pending Approval";
		public const string Open = "Open";
		public const string Closed = "Closed";
		public const string Rejected = "Rejected";

		public const string Hold = "Hold";
		public const string Balanced = "Balanced";
		public const string Released = "Released";
		public const string Reversed = "Reversed";

		public const string GroupTypes_Project = "Project";
		public const string GroupTypes_Task = "Task";
		public const string GroupTypes_AccountGroup = "Account Group";
		public const string GroupTypes_Equipment = "Equipment";


		public const string None = "None";
		public const string All = "All";

		public const string Origin_Source = "Use Source";
		public const string Origin_Change = "Replace";
		public const string Origin_FromAccount = "From Account";
		public const string Origin_None = "None";
		public const string Origin_DebitSource = "Debit Source";
		public const string Origin_CreditSource = "Credit Source";
		public const string Origin_Branch = "Specific Branch";

		public const string PMMethod_Transaction = "Allocate Transactions";
		public const string PMMethod_Budget = "Allocate Budget";

		public const string PMRestrict_AllProjects = "All Projects";
		public const string PMRestrict_CustomerProjects = "Customer Projects";

		public const string PMProForma_Select = "<SELECT>";
		public const string PMProForma_Release = "Release";

		public const string PMBillingType_Transaction = "Time and Material";
		public const string PMBillingType_Budget = "Progress Billing";

		public const string PMSelectOption_Transaction = "Non-Allocated Transactions";
		public const string PMSelectOption_Step = "From Previous Allocation Steps";

		public const string MaskSource = "Source";
		public const string AllocationStep = "Allocation Step";
		public const string ProjectSales = "Project Sales";
		public const string TaskSales = "Task Sales";
		public const string ProjectCost = "Project Cost";
		public const string TaskCost = "Task Cost";
		public const string DebitTransaction = "Debit Transaction";
		public const string CreditTransaction = "Credit Transaction";

		public const string OnBilling = "By Billing Period";
		public const string OnTaskCompletion = "On Task Completion";
		public const string OnProjectCompetion = "On Project Completion";

		public const string AccountSource_ARDefault = "AR Default";
		public const string AccountSource_None = "None";
		public const string AccountSource_SourceTransaction = "Source Transaction";
		public const string AccountSource_BillingRule = "Billing Rule";
		public const string AccountSource_Project = "Project";
		public const string AccountSource_ProjectAccrual = "Project Accrual";
		public const string AccountSource_Task = "Task";
		public const string AccountSource_Task_Accrual = "Task Accrual";
		public const string AccountSource_InventoryItem = "Inventory Item";
		public const string AccountSource_LaborItem = "Labor Item";
		public const string AccountSource_LaborItem_Accrual = "Labor Item Accrual";
		public const string AccountSource_Customer = "Customer";
		public const string AccountSource_Resource = "Resource";
		public const string AccountSource_Employee = "Employee";
		public const string AccountSource_Branch = "Branch";
		public const string AccountSource_CurrentBranch = "Current Branch";
		public const string AccountSource_RecurentBillingItem = "Recurring Item";
		public const string AccountSource_AccountGroup = "Account Group";
		public const string AccountSource_PostingClass = "Posting Class";
		public const string AccountSource_PostingClassItem = "Posting Class or Item";
		public const string AccountSource_Item = "Item";

		public const string Allocation = "Allocation";
		public const string Timecard = "Time Card";
		public const string Case = "Case";
		public const string ExpenseClaim = "Expense Claim";
		public const string EquipmentTimecard = "Equipment Time Card";
		public const string AllocationReversal = "Allocation Reversal";
		public const string Reversal = "Reversal";
		public const string Reversing = "Reversing";
		public const string ARInvoice = "Invoice";
		public const string CreditMemo = "Credit Memo";
		public const string DebitMemo = "Debit Memo";
		public const string APBill = "Bill";
		public const string CreditAdjustment = "Credit Adjustment";
		public const string DebitAdjustment = "Debit Adjustment";
		public const string UnbilledRemainder = "Unbilled Remainder";

		public const string UnbilledRemainderReversal = "Unbilled Remainder Reversal";
		public const string ProformaBilling = "Pro Forma Billing";
		public const string WipReversal = "WIP Reversal";
		public const string ServiceOrder = "Service Order";
		public const string Appointment = "Appointment";

		public const string RegularPaycheck = "Regular Paycheck";
		public const string SpecialPaycheck = "Special Paycheck";
		public const string AdjustmentPaycheck = "Adjustment Paycheck";
		public const string VoidPaycheck = "Void Paycheck";

		public const string PMReverse_OnARInvoiceRelease = "On AR Invoice Release";
		public const string PMReverse_OnARInvoiceGeneration = "On AR Invoice Generation";
		public const string PMReverse_Never = "Never";

		public const string PMNoRateOption_SetOne = "Set @Rate to 1";
		public const string PMNoRateOption_SetZero = "Set @Rate to 0";
		public const string PMNoRateOption_RaiseError = "Raise Error";
		public const string PMNoRateOption_NoAllocate = "Do Not Allocate";
		public const string PMNoRateOption_NoBill = "Do Not Bill";

		public const string PMDateSource_Transaction = "Original Transaction";
		public const string PMDateSource_Allocation = "Allocation Date";

		public const string Included = "Include Trans. created on billing date";
		public const string Excluded = "Exclude Trans. created on billing date";

		public const string Manual = "Manual";
		public const string ByQuantity = "Budgeted Quantity";
		public const string ByAmount = "Budgeted Amount";

		public const string CommitmentType_Internal = "Internal";
		public const string CommitmentType_External = "External";

		public const string CommitmentStatus_Open = "Open";
		public const string CommitmentStatus_Closed = "Closed";
		public const string CommitmentStatus_Canceled = "Canceled";

		public const string TaskType_Combined = "Combined";
		public const string TaskType_Expense = "Cost";
		public const string TaskType_Revenue = "Revenue";

		public const string Progressive = "Progressive";
		public const string Transaction = "Transactions";

		public const string Option_BillNow = "Bill";
		public const string Option_WriteOffRemainder = "Write Off Remainder";
		public const string Option_HoldRemainder = "Hold Remainder";
		public const string Option_Writeoff = "Write Off";

		public const string BudgetLevel_Item = "Task and Item";
		public const string BudgetLevel_CostCode = "Task and Cost Code";
		public const string BudgetLevel_Detail = "Task, Item, and Cost Code";


		public const string Validation_Error = "Validate";
		public const string Validation_Warning = "Ignore";

		public const string BudgetUpdate_Detailed = "Detailed";
		public const string BudgetUpdate_Summary = "Summary";

		public const string ChangeOrderLine_Update = "Update";
		public const string ChangeOrderLine_NewDocument = "New Document";
		public const string ChangeOrderLine_NewLine = "New Line";
		public const string ChangeOrderLine_Reopen = "Reopen";

		public const string CostRateType_All = "All";
		public const string CostRateType_Employee = "Employee";
		public const string CostRateType_Union = "Union Wage";
		public const string CostRateType_Certified = "Prevailing Wage";
		public const string CostRateType_Project = "Project";
		public const string CostRateType_Item = "Labor Item";

		public const string BillingFormat_Summary = "Summary";
		public const string BillingFormat_Detail = "Detail";
		public const string BillingFormat_Progress = "Progress Billing";
				
		public const string Percentage = "%";
		public const string FlatFee = "Flat Fee";
		public const string Cumulative = "Cumulative %";

		public const string BudgetControlCalculate = "Calculate";
		public const string BudgetControlCalculateTip = "Calculate budget overruns";

		public const string BudgetControlOption_Nothing = "Do Not Control";
		public const string BudgetControlOption_Warn = "Show a Warning";

		public const string ProgressMethod_Quantity = "Quantity";
		public const string ProgressMethod_Amount = "Amount";

		public const string BudgetControlDocumentType_PurchaseOrder = "Purchase Order";
		public const string BudgetControlDocumentType_Subcontract = "Subcontract";
		public const string BudgetControlDocumentType_APBill = "AP Bill";
		public const string BudgetControlDocumentType_ChangeOrder = "Change Order";

		public const string BudgetControlWarning = "Budgeted: {0:F2}, Consumed: {1:F2}, Available: {2:F2}, Document: {3:F2}, Remaining: {4:F2}";
		public const string BudgetControlNotFoundWarning = "No budget is found. Budgeted: {0:F2}, Document: {1:F2}, Remaining: {2:F2}";
		public const string BudgetControlDocumentWarning = "The project budget is exceeded. For details, check warnings in the document lines.";

		public const string Retainage_Normal = "Standard";
		public const string Retainage_Contract = "Contract Cap";
		public const string Retainage_Line = "Contract Item Cap";

		public const string DropshipGenerateReceipt = "Generate Receipt";
		public const string DropshipSkipReceipt = "Skip Receipt Generation";

		public const string DropshipRecordExpenseOnBillRelease = "On Bill Release";
		public const string DropshipRecordExpenseOnReceiptRelease = "On Receipt Release";
		#region Project Accounting Mode
		public const string ProjectSpecific = "Track by Project Quantity and Cost";
		public const string Valuated = "Track by Project Quantity";
		public const string Linked = "Track by Location";
		#endregion

		#endregion

		#region Field Display Names

		public const string CreditAccountGroup = "Credit Account Group";
		public const string FinPTDAmount = "Financial PTD Amount";
		public const string FinPTDQuantity = "Financial PTD Quantity";
		public const string BudgetPTDAmount = "Budget PTD Amount";
		public const string BudgetPTDQuantity = "Budget PTD Quantity";
		public const string RevisedPTDAmount = "Revised PTD Amount";
		public const string RevisedPTDQuantity = "Revised PTD Quantity";
		public const string AccountedCampaign = "Accounted Campaign";

		public const string RevisedBudgetedAmount = "Revised Budgeted Amount";
		public const string DraftInvoiceAmount = "Draft Invoice Amount";
		public const string ActualAmount = "Actual Amount";
		public const string RevisedBudgetedAmountInProjectCurrency = "Revised Budgeted Amount in Project Currency";
		public const string DraftInvoiceAmountInProjectCurrency = "Draft Invoices Amount in Project Currency";
		public const string ActualAmountInProjectCurrency = "Actual Amount in Project Currency";

		public const string PendingInvoiceQuantity = "Pending Invoice Quantity";
		public const string DraftInvoiceQuantity = "Draft Invoice Quantity"; 
		public const string ProgressBillingBase = "Progress Billing Base";
		public const string PreviouslyInvoicedQuantity = "Previously Invoiced Quantity";

		public const string EmployeeName = "Employee Name";

		public const string LaborRateNameWithPayroll = "Cost Rate";
		public const string LaborRateNameWithoutPayroll = "Rate";
		#endregion

		#region Cost Projection
		public const string CostProjection = "Cost Projection";
		public const string CostProjectionLine = "Cost Projection Line";
		public const string CostProjectionClass = "Cost Projection Class";
		public const string CostProjectionDetail = "Cost Projection Detail";
		public const string CostProjectionHistory = "Cost Projection History";

		public const string CostProjectionPercentage = "Percentage";
		public const string Quantity = "Quantity";
		public const string Amount = "Amount";

		public const string ClassIsInactive = "Class is Inactive";

		public const string ProjectionModeAuto = "Auto";
		public const string ProjectionModeManual = "Manual";
		public const string ProjectionModeManualQuantity = "Manual Quantity";
		public const string ProjectionModeManualCost = "Manual Cost";

		public const string Incompetable = "The projection is not compatible with the structure of the project budget.";
		public const string IncompetableClass = "The class is not compatible with the structure of the project budget.";
		public const string CostProjectionCantBeReleased = "The cost projection cannot be released because the {0} cost projection class is not compatible with the structure of the project budget.";
		public const string CostProjectionDuplicateID = "Duplicate ID";
		public const string ValueIsDisabled = "The value can be changed only when there are no lines on the Details tab.";
		public const string InvalidAccountGroup = "The {0} account group from the source file does not exist in the system.";
		public const string InvalidCostTask = "The {0} cost task from the source file does not exist in the system.";
		public const string InvalidCostCode = "The {0} cost code from the source file does not exist in the system.";
		public const string InvalidInventoryID = "The {0} inventory item from the source file does not exist in the system.";
		public const string MissingAccountGroup = "The required AccountGroupID column is missed in the source file.";
		public const string MissingTaskID = "The required TaskID column is missed in the source file.";
		public const string MissingCostCode = "The required CostCodeID column is missed in the source file.";
		public const string MissingInventoryID = "The required InventoryID column is missed in the source file.";
		#endregion Cost Projection

		#region Dialogs

		public const string UpdateRestrictionsForProjectsCaption = "Update Projects";
		public const string UpdateRestrictionsForProjectsMessage = "The restriction groups will be reset for all projects that belong to the {0} project group. Do you want to update the settings of the projects?";
		public const string ExcludeAllProjectsInProjectGroupMessage = "If you clear the Included check box, the projects included in the {0} project group will be excluded from the current restriction group. Do you want to proceed?";

		#endregion

		public static string GetLocal(string message)
		{
			return PXLocalizer.Localize(message, typeof(Message).FullName);
		}
	}

	[PXLocalizable(Warnings.Prefix)]
	public static class Warnings
	{
		public const string Prefix = "PM Warning";

		public const string AccountIsUsed = "This account is already added to the '{0}' account group. By clicking Save, you will move the account to the currently selected account group.";
		public const string StartDateOverlow = "Start Date for the given Task falls outside the Project Start and End date range.";
		public const string EndDateOverlow = "End Date for the given Task falls outside the Project Start and End date range.";
		public const string ProjectIsCompleted = "Project is Completed. It will not be available for data entry.";
		public const string ProjectIsNotActive = "Project is Not Active. Please Activate Project.";
		public const string NothingToAllocate = "Transactions were not created during the allocation.";
		public const string NothingToBill = "Invoice was not created during the billing. Nothing to bill.";
		public const string ProjectCustomerDontMatchTheDocument = "Customer on the Document doesn't match the Customer on the Project or Contract.";
		public const string SelectedProjectCustomerDontMatchTheDocument = "The customer in the selected project or contract differs from the customer in the current document.";
		public const string ProjectTaxZoneFeatureIsInUse = "This functionality is in use. If you clear this check box, the system will not use project-specific tax zones and addresses in project-related documents.";
		public const string NoPendingValuesToBeBilled = "The operation has been completed. The project has no pending values to be billed on {0}.";
		[Obsolete]
		public const string CannotCreateProformaBecauseOfInclusiveTax = "A pro forma invoice cannot be created because some of the lines to be billed are subject to an inclusive tax. To bill these lines, clear the Create Pro Forma Invoice on Billing on the Summary tab, and run billing procedure to create a direct AR invoice.";
	}

	[PXLocalizable]
	public static class StatusCodeDescriptions
	{
		public const string InclusiveTaxesInRevenueBudgetIntroduced = "The revenue project balances now include the applicable inclusive taxes. To make sure that the project reports include correct tax information, recalculate the project balance by using the Recalculate Project Balance command in the More menu.";
	}

	[Flags]
	public enum StatusCodes : int
	{
		Valid = 0,
		Warning = 1,
		Error = 2,
		InclusiveTaxesInRevenueBudgetIntroduced = 4
	}

	public static class StatusCodeHelper
	{
		public static bool IsValidStatus(int? statusCodeValue, out string statusDescription, out PXErrorLevel errorLevel)
		{
			statusDescription = string.Empty;
			errorLevel = PXErrorLevel.Undefined;

			if (!statusCodeValue.HasValue)
				return true;

			StatusCodes statusCode = (StatusCodes)statusCodeValue.GetValueOrDefault();

			if (statusCode == StatusCodes.Valid)
				return true;

			if (CheckStatus(statusCode, StatusCodes.InclusiveTaxesInRevenueBudgetIntroduced))
				statusDescription = StatusCodeDescriptions.InclusiveTaxesInRevenueBudgetIntroduced;

			if (string.IsNullOrWhiteSpace(statusDescription))
				return true;

			if (CheckStatus(statusCode, StatusCodes.Warning))
				errorLevel = PXErrorLevel.RowWarning;
			else if (CheckStatus(statusCode, StatusCodes.Error))
				errorLevel = PXErrorLevel.RowError;

			if (errorLevel == PXErrorLevel.Undefined)
				return true;

			return false;
		}

		public static bool CheckStatus(int? statusCodeValue, StatusCodes statusCodeToCheck)
		{
			if (!statusCodeValue.HasValue)
				return false;

			return CheckStatus((StatusCodes)statusCodeValue.GetValueOrDefault(), statusCodeToCheck);
		}

		private static bool CheckStatus(StatusCodes statusCode, StatusCodes statusCodeToCheck)
			=> (statusCode & statusCodeToCheck) == statusCodeToCheck;

		public static int? ResetStatus(int? statusCodeValue, StatusCodes statusCodeToReset)
		{
			if (!statusCodeValue.HasValue)
				return null;

			StatusCodes statusCode = (StatusCodes)statusCodeValue.GetValueOrDefault();

			var result = ResetStatus(statusCode, statusCodeToReset);

			if (result == StatusCodes.Error || result == StatusCodes.Warning)
				result = StatusCodes.Valid;

			return (int?)result;
		}

		public static int? AppendStatus(int? statusCodeValue, StatusCodes statusCodeToAppend)
		{
			StatusCodes statusCode = (StatusCodes)statusCodeValue.GetValueOrDefault();

			return (int?)AppendStatus(statusCode, statusCodeToAppend);
		}

		private static StatusCodes AppendStatus(StatusCodes currentStatusCode, StatusCodes statusCodeToAppend)
			=> currentStatusCode | statusCodeToAppend;

		private static StatusCodes ResetStatus(StatusCodes currentStatusCode, StatusCodes statusCodeToReset)
			=> currentStatusCode & ~statusCodeToReset;
	}
}
