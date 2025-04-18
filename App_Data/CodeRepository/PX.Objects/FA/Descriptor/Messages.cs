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

namespace PX.Objects.FA
{
	[PXLocalizable(Messages.Prefix)]
	public static class Messages
	{
		public const string Prefix = "FA Error";

		#region Graph/Cache Names
		public const string AssetClassMaint = "Asset Class Maintenance";
		public const string AssetMaint = "Asset Maintenance";
		public const string DepreciationMethodMaint = "Depreciation Method Definition Maintenance";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string CalcDepreciation = "Calculation of Depreciation";
		public const string ServiceScheduleMaint = "Service Schedule Maintenance";
		public const string UsageScheduleMaint = "Usage Schedule Maintenance";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string FATransactionEntry = "Fixed Asset Transactions";
		public const string BookMaint = "Book Maintenance";
		public const string SetupMaint = "Setup Maintenance";
		public const string FABookYearSetupMaint = "Book Calendar";
		public const string GenerationPeriods = "Generate FA Calendars";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string FATranRelease = "Release FA Transactions";
		public const string CalcDeprProcess = "Run Depreciation";
		public const string AssetGLTransactions = "Asset GL Transactions";
		public const string DisposalMethodMaint = "Disposal Methods";
		public const string AssetSummary = "Asset Summary";
		public const string BonusMaint = "Bonuses";
		public const string FixedAsset = "Fixed Asset";
		public const string Register = "Fixed Asset Transaction";
		public const string AssetClass = "Asset Class";
		public const string DepreciationMethod = "Depreciation Method";
		public const string BookCalendar = "Book Calendar";
		public const string DisposalProcess = "Run Disposal";
		public const string SplitProcess = "Split Asset";
		public const string TransferProcess = "Run Transfer";
		public const string FixedAssetCostEnq = "Asset Cost Summary";
		public const string FACostDetailsInq = "Asset Cost Details";
		public const string FASplitsInq = "Asset Splits";
		public const string DeleteDocsProcess = "Delete FA Documents";
		public const string DisposeParams = "Disposal Info";
		public const string ReverseDisposalInfo = "Reverse Disposal Info";
		public const string FAComponent = "FA Component";
		public const string FAHistory = "FA History";
		public const string FAHistoryByPeriod = "FA History by Period";
		public const string FATransaction = "Fixed Asset Transaction";
		public const string FALocationHistory = "FA Location History";
		public const string FADetails = "FA Details";
		public const string FABook = "FA Book";
		public const string FABookBalance = "FA Book Balance";
		public const string FABookHistory = "FA Book History";
		public const string FABookHistoryByPeriod = "FA Book History by Period";
		public const string FAAccrualTran = "FA Accrual Transaction";
		public const string FABookHistoryRecon = "FA Book History for Reconciliation";
		public const string FALocationHistoryByPeriod = "FA Location History by Period";
		public const string FABonusDetails = "FA Bonus Details";
		public const string FABookPeriod = "FA Book Period";
		public const string FABookPeriodSetup = "FA Book Period Template";
		public const string FABookSettings = "FA Book Preferences";
		public const string FABookYear = "FA Book Year";
		public const string FADepreciationMethodLines = "FA Depreciation Method Lines";
		public const string FADisposalMethod = "FA Disposal Method";
		public const string FAService = "FA Service";
		public const string FAServiceSchedule = "FA Service Schedule";
		public const string FAType = "FA Type";
		public const string FAUsage = "FA Usage";
		public const string FAUsageSchedule = "FA Usage Schedule";
		#endregion

		#region Combo Values
		public const string ClassType = "Class";
		public const string AssetType = "Asset";
		public const string ElementType = "Component";
		public const string BothType = "Both";

		public const string Building = "Building";
		public const string Ground = "Land";
		public const string Vehicle = "Vehicle";
		public const string Equipment = "Equipment";
		public const string Computers = "Computers";
		public const string Furniture = "Furniture";
		public const string Machinery = "Machinery";

		public const string Active = "Active";
		//public const string Hold = "On Hold";
		public const string Suspend = "Suspend";
		public const string Unsuspend = "Unsuspend";
		public const string FullyDepreciated = "Fully Depreciated";
		public const string Disposed = "Disposed";
		[Obsolete(PX.Objects.Common.Messages.FieldIsObsoleteRemoveInAcumatica8)]
		public const string UnderConstruction = "Under construction";
		[Obsolete(PX.Objects.Common.Messages.FieldIsObsoleteRemoveInAcumatica8)]
		public const string Dekitting = "Dekitting";

		public const string Sale = "Sale";
		public const string Retirement = "Retirement";
		public const string Sectionalize = "Sectionalize";
		public const string TakenApart = "TakenApart";
		public const string Disappear = "Disappear";

		public const string Displacement = "Displacement";
		public const string ChangeResponsible = "Change Responsible";

		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string ComponentDisplacementsWithAssets = "Component displacements with Assets";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string ComponentBecomeAsset = "Component become Asset";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string AssetBecomeComponent = "Asset become Component";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string AssetBecomeInventoryItem = "Asset become Inventory Item";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string BecomeAsset = "Inventory Item become Asset";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string ComponentBecomeInventoryItem = "Component become Inventory Item";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string BecomeComponent = "Inventory Item become Component";

		public const string Mileage = "Mileage";
		public const string Operating = "Operating";

		public const string Day = "Day";
		public const string Week = "Week";
		public const string Month = "Month";
		public const string Year = "Year";

		public const string Property = "Property";
		public const string GrantProperty = "Grant Property";
		public const string Leased = "Leased";
		public const string LeasedtoOthers = "Leased to Others";
		public const string Rented = "Rented";
		public const string RentedtoOthers = "Rented to Others";
		public const string Credit = "To the Credit of";

		public const string Software = "Software";
		public const string Goodwill = "Goodwill";
		public const string Patents = "Patents";
		public const string Copyrights = "Copyrights";

		public const string Good = "Good";
		public const string Avg = "Average";
		public const string Poor = "Poor";

		public const string NotAplicable = "Not Applicable";
		public const string Line10 = "10. Office Furniture & Machines & Library";
		public const string Line11 = "11. EDP Equipment/Computers/Word Processors";
		public const string Line12 = "12. Store Bar & Lounge and Restaurant Furniture & Equipment";
		public const string Line13 = "13. Machinery and Manufacturing Equipment";
		public const string Line14 = "14. Farm Grove and Dairy Equipment";
		public const string Line15 = "15. Professional Medical Dental & Laboratory Equipment";
		public const string Line16 = "16. Hotel Motel & Apartment Complex";
		public const string Line16a = "16a. Rental Units - Stove Refrig. Furniture Drapes & Appliances";
		public const string Line17 = "17. Mobile Home Attachments";
		public const string Line18 = "18. Service Station & Bulk Plant Equipment";
		public const string Line19 = "19. Sings - Billboard Pole Wall Portable Directional Etc.";
		public const string Line20 = "20. Leasehold improvements";
		public const string Line21 = "21. Pollution Control Equipment";
		public const string Line22 = "22. Equipment owned by you but rented leased or held by others";
		public const string Line23 = "23. Supplies - Not Held for Resale";
		public const string Others = "24. Other";

		public const string Acquisition = "Acquisition";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string AccumulatedDepreciation = "Accumulated Depreciation";
		public const string Disposal = "Disposal";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string CostReappraisal = "Cost Reappraisal";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string AccumulatedDepreciationReappraisal = "Accumulated Depreciation Reappraisal";

		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string InvoiceService = "Service Invoice";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string InvoiceInsurance = "Insurance Invoice";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string InvoiceLease = "Lease Invoice";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string InvoiceRent = "Rent Invoice";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string InvoiceCredit = "Credit Invoice";

		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string PaymentService = "Service Payment";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string PaymentInsurance = "Insurance Payment";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string PaymentLease = "Lease Payment";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string PaymentRent = "Rent Payment";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string PaymentCredit = "Credit Payment";

		public const string Balanced = "Balanced";
		public const string Released = "Released";
		public const string Hold = "On Hold";
		public const string Voided = "Voided";
		public const string Calculated = "Calculated";
		public const string Unposted = "Unposted";
		public const string Posted = "Posted";
		public const string Completed = "Completed";


		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string AssetInventory = "Asset Inventory";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string ComponentInventory = "Component Inventory";

		public const string MACRS = "MACRS";
		public const string ACRS = "ACRS";
		public const string StraightLine = "Straight-Line";
		public const string DecliningBalance = "Declining-Balance";
		public const string SumOfTheYearsDigits = "Sum-of-the-Years’-Digits";
		public const string RemainingValue = "Remaining Value";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string CustomTable = "Custom Table";
		public const string Dutch1 = "Dutch Method 1";
		public const string Dutch2 = "Dutch Method 2";
		public const string RemainingValueByPeriodLength = "Remaining Value by Days in Period";
		public const string DecliningBalanceByPeriodLength = "Declining Balance by Days in Period";
		public const string AustralianPrimeCost = "Australian Prime Cost";
		public const string AustralianDiminishingValue = "Australian Diminishing Value";
		public const string NewZealandStraightLine = "New Zealand Straight-Line";
		public const string NewZealandDiminishingValue = "New Zealand Diminishing Value";
		public const string NewZealandStraightLineEvenly = "New Zealand Straight-Line Evenly by Periods";
		public const string NewZealandDiminishingValueEvenly = "New Zealand Diminishing Value Evenly by Periods";

		public const string MethodDescBonus = "BONUS";
		public const string MethodDescTax179 = "TAX179";

		public const string FullPeriod = "Full Period";
		public const string HalfPeriod = "Mid Period";
		public const string ModifiedPeriod = "Modified Half Period";
		public const string ModifiedPeriod2 = "Modified Half Period 2";
		public const string NextPeriod = "Next Period";
		public const string FullQuarter = "Full Quarter";
		public const string HalfQuarter = "Mid Quarter";
		public const string FullYear = "Full Year";
		public const string HalfYear = "Mid Year";
		public const string FullDay = "Full Day";

		public const string PurchasingPlus = "Purchasing+";
		public const string PurchasingMinus = "Purchasing-";
		public const string DepreciationPlus = "Depreciation+";
		public const string DepreciationMinus = "Depreciation-";
		public const string CalculatedPlus = "Calculated+";
		public const string CalculatedMinus = "Calculated-";
		public const string SalePlus = "Sale/Dispose+";
		public const string SaleMinus = "Sale/Dispose-";
		public const string TransferPurchasing = "Transfer Purchasing";
		public const string TransferDepreciation = "Transfer Depreciation";
		public const string ReconciliationPlus = "Reconciliation+";
		public const string ReconciliationMinus = "Reconciliation-";
		public const string PurchasingDisposal = "Purchasing Disposal";
		public const string PurchasingReversal = "Purchasing Reversal";
		public const string AdjustingDeprPlus = "Depreciation Adjusting+";
		public const string AdjustingDeprMinus = "Depreciation Adjusting-";

		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string QuarterConv = "Quarter";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string MonthConv = "Month";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string HalfYearConv = "Half Year";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string YearConv = "Year";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string HalfMonth = "Half Month";

		public const string FixedDay = "Fixed Day";
		public const string NumberOfDays = "Number of Days";
		public const string PeriodDaysHalve = "Half Period";

		public const string Calculate = "Calculate Only";
		public const string Depreciate = "Depreciate";

		public const string OriginAdjustment = "Adjustment";
		public const string OriginPurchasing = "Purchasing";
		public const string OriginDepreciation = "Depreciation";
		public const string OriginDisposal = "Disposal";
		public const string OriginTransfer = "Transfer";
		public const string OriginReconcilliation = "Reconcilliation";
		public const string OriginSplit = "Split";
		public const string OriginReversal = "Reversal";
		public const string OriginDisposalReversal = "Disposal Reversal";

		public const string SideBySide = "Side by Side";
		public const string BookSheet = "By Book";

		public const string MaskAsset = "Fixed Asset";
		public const string MaskLocation = "Fixed Asset Branch";
		public const string MaskDepartment = "Fixed Asset Department";
		public const string MaskClass = "Fixed Asset Class";

		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string NetValue = "Net Value";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string Acquired = "Acquisitions";
		public const string Depreciated = "Accumulated Depreciation";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string DepreciationBase = "Bepreciation Base";
		public const string Bonus = "Bonus";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string BonusRecap = "Bonus Recapture";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string BonusTaken = "Bonus Taken";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string Tax179 = "Tax 179";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string Tax179Recap = "Tax 179 Recapture";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string Tax179Taken = "Tax 179 Taken";
		public const string Revalue = "Revalue";
		public const string RGOL = "RGOL";
		public const string Suspended = "Suspended";
		public const string Reversed = "Reversed";

		public const string Addition = "Addition";
		public const string Deduction = "Deduction";

		public const string Automatic = "Automatic";
		public const string Manual = "Manual";
		#endregion

		#region Error messages
		public const string Document_Status_Invalid = AP.Messages.Document_Status_Invalid;
		public const string FixedAssetClassCannotBeDeactivated = "The fixed asset class cannot be deactivated. There are some fixed assets with the Active status associated with this class.";
		public const string WrongValue = "Value must equal 100%.";
		public const string BookExistsHistory = "Book '{0}' cannot be deleted because it is used by Fixed Asset or Fixed Asset Class.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string LedgerBookExists = "You cannot set defaul Ledger from GLSetup because there is one more Book with default ledger.";
		public const string ScheduleExistsHistory = "You cannot delete Schedule because transactions for this Schedule exist.";
		public const string NoPeriodsDefined = "Book Period cannot be found in the system.";
		public const string NoCalendarDefined = "Book Calendar cannot be found in the system.";

		public const string ValueCanNotBeEmpty = "Value can not be empty.";
		public const string FinPeriodsNotDefined = "Financial Periods are not defined in {1}.";
		public const string FABookPeriodsNotDefinedFrom = "Asset will be depreciated from {0}, book '{2}' does not have financial periods generated from {1}. You need to go to 'Generate FA Calendars' screen and generate financial periods before changing this asset.";
		public const string FABookPeriodsNotDefinedFromTo = "Asset will be depreciated from {0} to {1}, book '{4}' does not have financial periods generated from {2} to {3}. You need to go to 'Generate FA Calendars' screen and generate financial periods before changing this asset.";
		public const string FABookPeriodsNotDefined = "Financial Periods are not defined for the book '{0}' in {1}.";
		public const string FABookPeriodsNotDefinedForDate = "Financial period is not defined for the {0} book for {1}.";
		public const string IncorrectPurchasingPeriod = "The financial period of the {0} purchasing transaction cannot be earlier than the period of {1} ({2}).";
		public const string IncorrectPurchasingDate = "The {0} of the purchasing transaction must fall between or be equal to the {1} and the {2}.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string CanNotUseAveragingConventionWhithDate = "Can not use averaging convention {0} with depreciation start date {1}.";
		public const string CanNotUseAveragingConventionWhithRecoveryPeriods = "Can not use averaging convention {0} with recovery period {1}.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string CanNotCalculateDepreciation = "Can not calculate depreciation!";
		public const string DepreciationMethodDoesNotExist = "Depreciation method does not exist.";
		public const string DepreciationAdjustmentPostedOpenPeriod = "Depreciation adjustments (D+/D-) can be posted only to closed periods.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string DepreciationAdjustmentPostedClosedPeriod = "Depreciation adjustments (A+/A-) can be posted only to closed periods.";
		public const string CalculatedDepreciationPostedFuturePeriod = "Calculated depreciation of the Fixed Asset '{0}' (Book '{1}') tries to post into the period {2}. It can be posted only to current period {3}.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string PurchasePostedClosedPeriod = "Purchasing adjustments can be posted only to open periods.";
		public const string BalanceRecordCannotBeDeleted = "Record cannot be deleted.";
		public const string TranDateOutOfRange = "Calendar is not setup for date in Book '{0}'.";
		public const string AssetDisposedInPastPeriod = "The asset cannot be disposed of in past periods.";
		public const string AssetDisposedInSuspendedPeriod = "The asset cannot be disposed of in suspended periods.";
		public const string TranPostedToSuspendedPeriod = "Transactions cannot be posted to suspended period in Book '{0}'.";
		public const string TranPostedOnHold = "Transactions cannot be posted for asset 'On Hold'.";
		public const string CalendarAlreadyExists = "Calendar for this book already created.";
		public const string AssetIsNotSaved = "Fixed asset must be saved";
		public const string UnholdAssetOutOfBalance = "Asset is out of balance and cannot be removed from 'Hold'. Add Purchasing+ transactions with total amount {0}.";
		public const string GLTranNotSelected = "GL Transaction must be selected.";
		public const string IncorrectDepreciationPeriods = "Incorrect periods beginning and end of depreciation for Book '{0}'";
		public const string CyclicParentRef = "You cannot reference the child asset '{0}' as the parent asset.";
		public const string AssetHasUnreleasedTran = "The {0} fixed asset contains unreleased transactions. Release them to continue processing the asset.";
		public const string AssetShouldBeDeprToPeriod = "Fixed asset should be depreciated to period '{0}' if \"Automatically Release Depreciation Transactions\" is not set.";
		public const string CalendarSetupNotFound = "FA calendar cannot be generated for the book '{0}' because the calendar structure is not configured for this book on the Book Calendars (FA206000) form.";
		public const string FixedAssetNotSaved = "The fixed asset is not saved.";
		public const string FixedAssetHasUnreleasedPurchasing = "The fixed asset has unreleased purchasing transactions.";
		public const string FixedAssetHasUnreleasedRecon = "The fixed asset has unreleased reconciliation transactions.";
		public const string PeriodWillBeAutoChangedToNearestOpenPeriod = "The selected period will be automatically changed to the nearest open period.";
		public const string InvalidReconTran = "Reconcilliation transaction has no reference to the original GL transaction.";
		public const string CanNotDisposeUnreconciledAsset = "Unreconciled asset can not be disposed";
		public const string AcquisitionAfterDisposal = "Disposal date must be greater than acquisition date.";
		public const string SplittedCostGreatherOrigin = "Total cost of splitted assets greater than cost of origin asset.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string SplittedQtyGreatherOrigin = "Total quantity of splitted assets greater than or equal to quantity of origin asset.";
		public const string DeprFromPeriodGreaterLastDeprPeriod = "The new Depr. From period '{0}' is later than the most recent depreciation period '{1}'.";
		public const string DeprToPeriodLessLastDeprPeriod = "The new Depr. To period '{0}' is earlier than the most recent depreciation period '{1}'.";
		public const string CannotChangeUsefulLife = "New Depr. To Period '{0}' less than Last Depreciation Period '{1}'.";
		public const string CannotCreateAsset = "Cannot create the fixed asset. To create a fixed asset, specify the Asset ID or clear the Manual Numbering check box on the Numbering Sequences (CS201010) form for the fixed asset sequence.";
		public const string AssetIDAlreadyExists = "A fixed asset with this ID already exists in the system.";
		public const string FAClassChangeConfirmation = "Warning - Only GL Accounts will be changed, all other parameters remain unchanged. Do you want to continue?";
		public const string DispAmtIsEmpty = "Can not dispose Fixed Assets. Disposal Amount is empty.";
		public const string CannotReleaseInInitializeMode = "Only Purchasing Register can be released in Initialization Mode.";
		public const string AssetNotDepreciatedInPeriod = "Fixed asset '{0}' not depreciated by the book '{1}' in {2} period.";
		public const string NextPeriodNotGenerated = "Next Period after Last Depr. Period isn't generated.";
		public const string CurrentDeprPeriodIsNull = "Fixed asset not acquired or fully depreciated.";
		public const string OperationNotWorkInInitMode = "This operation is not available in initialization mode. To exit the initialization mode, select the '{1}' checkbox on the '{0}' screen.";
		public const string CannotChangeCurrentPeriod = "New Current Period '{0}' less than previous Current Period '{1}'.";
		public const string ReverseDispPeriodNotFound = "Active open period is not found after Disposal Period '{0}'";
		public const string ActiveAssetTransferedPastDeprToPeriod = "The active fixed asset {0} cannot be transferred in the {1} period after the last {2} period of the depreciation schedule in the {3} book.";
		public const string ActiveAssetTransferedBeforePeriod = "The active fixed asset {0} cannot be transferred in the {1} period before the current {2} period in the {3} book.";
		public const string FullyDepreciatedAssetTransferedBeforePeriod = "Fully-depreciated asset cannot be transferred before Period {0}";
		public const string AnotherDeprRunning = "Another depreciation process is already running. Wait for completion.";
		public const string CantReverseDisposedAsset = "Unable Reverse Disposed Fixed Asset '{0}'";
		public const string CantReverseDisposal = "The Status of Fixed Asset '{0}' is '{1}'. Unable Reverse Disposal.";
		public const string TableMethodHasNoLineForYear = "Table depreciation method '{0}' has no line for {1} year.";
		public const string FlagHasValueOnButItMustBeUndefinedForTransactionType = "{0} flag has value on {1}, but it must be undefined for transaction type '{2}'.";
		public const string FlagsOfControlAccountsWereNotDefinedForType = "Flags of control accounts were not defined for type '{0}'.";
		public const string FAClassUsedInAssets = "You cannot delete Fixed Asset Class because this Class used in Fixed Assets.";
		public const string FADeprMethodUsedInAssets = "You cannot delete Depreciation Method because this Method used in Fixed Assets or Classes.";
		public const string FAMethodCDFormulaBasedExists = "The formula-based depreciation method with the same ID exists in the system.";
		public const string FAMethodCDTableBasedExists = "The table-based depreciation method with the same ID exists in the system.";
		public const string FAClassIsParent = "You cannot delete Fixed Asset Class because this Class is parent for another class.";
		public const string FATypeDeleteUsed = "You cannot delete that Asset Type because it is specified for some fixed asset or fixed asset class.";
		public const string FATypeChangeUsed = "Changes will affect only newly created fixed asset or fixed asset class.";
		public const string UsefulLifeNotMatchDeprMethod = "Useful Life does not match the recovery period specified for selected Depreciation Method.";
		public const string PrevLocationRevisionNotFound = "Previous location of Fixed Asset '{0}' is not found for restoring.";
		public const string QuarterIsUndefined = "Quarter is defined only for the 'Month' and 'Quarter' types of period.";
		public const string InactiveFAClass = "Fixed Asset Class '{0}' is inactive.";
		public const string InvalidDeprToDate = "DepreciationToDate cannot be greater than RecoveryEndDate. Please contact support.";
		public const string UpdateGLBookHasFACalendar = "The Update GL check box cannot be selected for the {0} book because the calendar has been configured for the book on Book Calendars (FA206000).";
	    public const string ConfirmDeleteReconcilliationTransaction = "The selected fixed asset has been created by the process on the Convert Purchases to Assets (FA504500) form. The changes made to the asset will cause the system to delete the reconciliation transactions created by the process, and if you need to reconcile the asset, you have to do so manually. Do you want to proceed?";
		public const string DeprFromPeriodUpdatedWhileDepreciationExists = "The Depr. From Period value cannot be changed because at least one depreciation transaction exists.";
		public const string UnexpectedMasterCalendarOfPostingBook = "The master calendar of a posting book cannot be selected directly if multiple calendars are supported";
	    public const string PeriodDoesNotExistForBookAndCompany = "The {0} period does not exist for the {1} book and the {2} company.";
	    public const string PeriodDoesNotExistForBook = "The {0} period does not exist for the {1} book.";
		public const string NotAllowedTransferBetweenOrganizations = "The fixed asset cannot be transferred between different companies. Select a branch from the company of the fixed asset.";
		public const string NonzeroNetBookValueAndDeprToPeriodInThePostingBookIsClosed = "The fixed asset cannot be created because it has nonzero net book value and the asset's Depr. to Period is closed in the posting book for the {0} company.";
		public const string DeprToPeriodInThePostingBookIsClosed = "The fixed asset cannot be created because the asset's Depr. to Period is closed in the posting book for the {0} company.";
		public const string NonzeroAccumDepreciationAndLastDeprPeriodIsEmptyInThePostingBook = "Last Depr. Period cannot be empty for a fixed asset in the book with nonzero accumulated depreciation.";
		public const string SubAccountNotCorrespondToMask = "The entered value does not correspond to the mask specified for the subaccount on the Fixed Asset Classes (FA201000) form.";
        public const string UnreleasedDocumentsOrFixedAssets = "There are no unreleased documents or fixed assets to be depreciated for the selected period or periods.";
		public const string FixedAssetWiilBeSuspended = "The fixed asset has been depreciated only until {0}. The depreciation will be suspended from {0} to {1}.";
		public const string NonDeprFixedAssetWiilBeSuspended = "The fixed asset has not been depreciated. The depreciation will be suspended from {0} to {1}.";
		public const string PlacedInServiceDateIsEarlierThanReceiptDate = "The {0} must be equal to or later than the {1}.";
		public const string InconsistentAdditionFATranType = "The \"{0}\" FA transaction type does not match the addition or deduction of the fixed asset.";
		public const string SeveralCompetingDepreciationMethods = "Several competing depreciation methods are found for the {0} calculation method and the {1} averaging convention: {3}";
		public const string FAAdditionsDontMatchHistory = "The purchasing additions ({0}) of the {2} fixed asset do not match the depreciation basis history ({1}).";
		public const string DeprFromDateNotMatchPeriod = "The start depreciation date {0} of the {2} fixed asset is outside the {1} transaction period.";
		public const string DeprFromDateNotMatchDeprFromPeriod = "The start depreciation date ({0}) of the {2} fixed asset is outside the {1} period specified in the Depr. from Period column. The asset cannot be depreciated. To solve the issue, contact your Acumatica support provider.";
		public const string AcceleratedDepreciationFlagIsIrrelevant = "The Accelerated Depreciation for SL Depr. Method check box selected for the asset class does not affect the {0} depreciation method based on the Straight-Line calculation method. With these settings, the asset’s net value may not become zero at the end of its useful life in some cases. To get zero net value at the end of the asset’s useful life, on the Balance tab of the Fixed Assets (FA202500) form, change the depreciation method to a method based on the Remaining Value calculation method.";
		[Obsolete(PX.Objects.Common.InternalMessages.ObsoleteToBeRemovedIn2019r2)]
		public const string AdditionsDisabledForAustralianPrimeCostDeprMethod = "Additions and deductions cannot be processed for the Australian Prime Cost depreciation method. Dispose of the asset and create a new one with the adjusted acquisition cost.";
		public const string AdditionsDisabledForCalcMethod = "Additions and deductions cannot be processed for the {0} calculation method. Dispose of the asset and create a new asset with the adjusted acquisition cost.";
		public const string WeeklyBooksDisabledForCalcMethod = "The {0} calculation method cannot be selected for a book that uses a week-based calendar.";
		public const string BookCalendarNotExist = "The book calendar does not exist for the {0} book. Use the Book Calendar Setup (FA206000) form to set up calendars for all needed books.";
		public const string DisposalPeriodCannotBeEarlierThanPeriodOfMostRecentTransaction = "The disposal period ({0}) cannot be earlier than the period of the most recent transaction ({1}).";
		public const string MissingFACalendarAtStartUsefullife = "The book calendar corresponding to the start date of the asset's useful life ({0:d}) is missing in the {1} book. Create a calendar on the Generate Book Calendars (FA501000) form.";
		public const string MissingFACalendarAtEndUsefullife = "The book calendar corresponding to the end date of the asset's useful life ({0:d}) is missing in the {1} book. Create a calendar on the Generate Book Calendars (FA501000) form.";
		public const string MissingFACalendarInCurrentFinYear = "There is no calendar generated for the book in the current financial year. Create a calendar on the Generate Book Calendars (FA501000) form.";
		public const string FixedAssetIsOverReconciled = "The current cost of the {0} fixed asset and its unreconciled amount must have the same sign.";
		public const string AssetIsUnderConstructionAndWillNotBeDepreciated = "The fixed asset is under construction and cannot be depreciated.";
		public const string AssetHasDepreciationTransactionsAndCannotBeTransferred = "The fixed asset has depreciation transactions and cannot be transferred to the Under Construction class.";
		public const string ComponentsCannotBeCreatedBecauseManualNumberingIsActivated = "Components cannot be created for the asset because manual numbering is activated for the {0} numbering sequence. Use the Convert Purchases to Assets (FA504500) form to create manually numbered components.";
		public const string UnderConstructionCheckboxCanBeSelectedOnlyForDepreciableAssetClasses = "The Under Construction check box can be selected only for depreciable asset classes.";
		public const string StateOfUnderConstructionCheckboxCannotBeChangedSomeAssetsAssociatedWithClass = "The state of the Under Construction check box cannot be changed because there are assets associated with the {0} class.";
		public const string AssetUnderConstructionWithEmptyPlaceInServiceDateCannotBeTransferred = "The asset under construction with an empty Placed-in-Service date cannot be transferred to another class.";
		public const string AssetWithCccumulatedDepreciationCannotBeTransferredToTheInderConstructionClass = "The asset with accumulated depreciation cannot be transferred to the Under Construction class.";
		public const string UsefulLifeChangeCausedFullDepreciatedStatus = "The fixed asset with the reduced useful life will get the Fully Depreciated status and its useful life will be disabled for editing. Click OK to proceed with the changes.";
		public const string DisposalDateMustBeInDisposalPeriod = "The disposal date cannot be earlier than the date of the most recent transaction ({0}).";
		public const string AcceleratedDepreciationCalculationIssueForSLMethods = "The Accelerated Depreciation for SL Method check box selected for the {0} asset class is not supported for the Full Day and Full Period averaging conventions. With these settings, the accumulated depreciation of the {1} asset will not be calculated correctly. Change the depreciation method to a method based on the Remaining Value calculation method on the Balance tab of the Fixed Assets (FA202500) form.";
		public const string FACalendarMatchGL = "The periods in the posting book match the financial periods in the general ledger.";
		public const string ReleasedTransactionsExistInUnsynchronizedPeriods = "The periods in the posting book cannot be synchronized with the financial periods in the general ledger, because there are released transactions in the periods. Please contact the support service for further assistance.";
		public const string ShortOrganizationFACalendarsDetected = "In fixed assets, the posting book calendars are shorter than the master calendar for the following companies: {0}. On the Generate Book Calendars (FA501000) form, generate years from {1} to {2} in the posting book for these companies.";
		public const string CannotInsertFAOrganizationPeriod = "The {0} period in the posting book cannot be created for the {1} company.";
		public const string CannotInsertFAOrganizationYear = "The {0} year in the posting book cannot be created for the {1} company.";
		public const string ToYearMustNotBeEarlierThanFromYear = "To Year must not be earlier than From Year.";
		public const string UnreleasedTransactionsExistInUnsynchronizedPeriods = "The periods in the posting book cannot be synchronized with the financial periods in the general ledger, because there are unreleased transactions in the periods. Use the Delete Unreleased Transactions (FA508000) form to delete the transactions.";
		public const string CannotUseDisabledOrReversedFixedAsset = "Disposed and reversed fixed assets cannot be used on this form.";
		public const string OperationCanNotBeCompletedInMigrationMode = "This operation cannot be completed in migration mode. To exit migration mode, select the Update GL check box on the Fixed Assets Preferences (FA101000) form.";
		public const string CanBeDepreciatedUsedWrongField = "The FixedAssetCanBeDepreciated operator must use a field from the {0} DAC in which it was used, but it uses a field from the {1} DAC.";
		public const string AcquisitionCostLessThanTax179AndBonusAmounts = "The total of the Tax 179 amount and the bonus amount must not exceed the acquisition cost of the asset.";
		public const string AssetAlreadyDisposedFromImportScenario = "The operation cannot be completed because the fixed asset has already been disposed.";
		public const string AccountLinkToCashAccountHasDifferentBranch = "The {0} account cannot be selected because it is linked to a cash account with a branch that differs from the asset's branch.";
		public const string InvalidAssetStatusInTransferProcess = "A transfer document cannot be created for a fixed asset in the Suspended or On Hold status. To proceed, unsuspend the asset or remove it from hold on the Fixed Assets (FA303000) form.";
		public const string TransferPeriodCannotBeGreaterThanDeprToPeriodInTransferProcess = "A fixed asset in the Active status cannot be transferred in a period later than its Depr. to Period. To make a transfer, use an earlier period or fully depreciate the asset before transferring it.";
		#endregion

		#region Multiple Base Currencies
		public const string BaseCurrencyDifferentFromAssetBranch = "The base currency of the {0} branch differs from the fixed asset's currency.";
		public const string BaseCurrencyDiffersFromTransactionBranch = "The {0} base currency of the {1} branch differs from the {2} base currency of the transaction's {3} branch.";
		public const string AssetBaseCurrencyDiffersFromTransactionBranch = "The fixed asset's currency ({0}) differs from the base currency of the transaction's {2} branch.";
		public const string AssetBaseCurrencyDiffersFromDestinationBranch = "The {1} fixed asset cannot be transferred to the {3} branch because the base currency of the fixed asset's branch ({0}) differs from the base currency of the {3} destination branch ({2}).";
		public const string DestinationBranchBaseCurrencyDiffersFromSourceBranch = "The {1} branch cannot be selected. The {0} base currency of the {1} destination branch differs from the base currency of the {2} branch from which the assets are transferred.";
		public const string SourceBranchBaseCurrencyDiffersFromDestinationBranch = "The {1} branch cannot be selected. The base currency of the {2} destination branch differs from the {0} base currency of the {1} branch from which the assets are transferred.";
		#endregion

		#region Transaction Descriptions
		public const string TranDescPurchase = "Purchase for Asset {0}";
		public const string TranDescSale = "Sale of Asset {0}";
		public const string TranDescDepreciation = "Depreciation for Asset {0}";
		public const string TranDescCostDisposal = "Cost Disposal for Asset {0}";
		public const string TranDescDeprDisposal = "Depreciation Disposal for Asset {0}";
		public const string TranDescDisposalAdj = "Depreciation Adjustment on Disposal for Asset {0}";
		public const string TranDescDepreciationRecap = "Depreciation Recap for Asset {0}";
		public const string TranDescTransferPurchasing = "Transfer Purchasing for Asset {0}";
		public const string TranDescTransferDepreciation = "Transfer Depreciation for Asset {0}";
		public const string ReduceUnreconciledCost = "Deduction Unreconciled Cost for Asset {0}";
		public const string TranDescSplit = "Split of Asset {0}";
		public const string SplitAssetDesc = "split from";
		public const string DocDescReversal = "Full Reversal of Asset {0}";
		public const string TranDescReversal = "{0} - reversed";
		public const string DocDescDispReversal = "Disposal Reversal of Asset {0}";
		public const string TranDescDispReversal = "{0} - disposal reversed";
		#endregion

		public const string Calendar = "Calendar";
		public const string PeriodFieldName = "Period";
		public const string DeprValueFieldName = "Depreciated";
		public const string CalcValueFieldName = "Calculated";
		public const string Dispose = "Dispose";
		public const string CalculateDepreciation = "Calculate Depreciation";
		public const string ProcessAdditions = "Process";
		public const string ViewDocument = "View Register";
		public const string Split = "Split";
		public const string Transfer = "Transfer";
		public const string Reverse = "Reverse";
		public const string DispReverse = "Reverse Disposal";

		public const string ViewBatch = "View Batch";
		public const string ViewAsset = "View Fixed Asset";
		public const string ViewBook = "View Book";
		public const string ViewClass = "View Asset Class";
		public const string ReduceUnreconCost = "Reduce Unreconciled Cost";
		public const string Prepare = "Prepare";
		public const string PrepareAll = "Prepare All";
		public const string ViewDetails = "View Details";
		public const string ShowAssets = "Show Fixed Assets";
		public const string DeleteGeneratedPeriods = "Delete Book Periods";
		public const string FASetup = "Fixed Assets Preferences";
		public const string FixedAssetSearchTitle = "Fixed Asset ID:{0}";
		public const string FADocumentDepreciationDescription = "Fixed Asset Depreciation";
		public const string PostingBookNotMatchFinPeriodInGL = "The document cannot be released, because the {0} period in the posting book does not match the financial period in the general ledger for the {1} company. To amend the periods in the posting book based on the periods in the general ledger, on the Book Calendars (FA304000) form, click Synchronize FA Calendar with GL.";
		public const string FixedAssetCannotBeSplitInSateLessThanPurchaseDate = "The fixed asset cannot be split on a date earlier than the purchase date {0}.";

		#region Field Display Names
		[Obsolete(PX.Objects.Common.InternalMessages.MessageIsObsoleteAndWillBeRemoved2018R2)]
		public const string AcquiredDate = "Acquisition Date";
		public const string FixedAssetsAccountClass = "Fixed Assets Account Class";
		public const string DeprFromDate = "Depr. From";
		public const string PlacedInServiceDate = "Placed-in-Service Date";
		public const string DeprFromPeriod = "Depr. From Period";
		public const string PlacedInServicePeriod = "Placed-in-Service Period";

		#endregion

		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string DisposalPrepared = "Prepared Disposal FA Registers";

		#region Button Displays

		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string CalcDeprProc = "Calc. & Depreciate";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string CalcDeprAllProc = "Calc. & Depreciate All";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string CalcProc = "Calculate";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string CalcAllProc = "Calculate All";
		public const string DeleteProc = "Delete";
		public const string DeleteAllProc = "Delete All";
		public const string Release = PM.Messages.Release;
		public const string ReleaseAll = PM.Messages.ReleaseAll;

		#endregion

		public const string Predefined = "Predefined";
		public const string Custom = "Custom";
	}
}
