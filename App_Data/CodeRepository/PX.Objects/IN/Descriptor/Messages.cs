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

namespace PX.Objects.IN
{
	[PXLocalizable(Messages.Prefix)]
	public static class Messages
	{
		#region Validation and Processing Messages
		public const string Prefix = "IN Error";
		public const string InventoryItemIsInStatus = "The inventory item is {0}.";
		public const string InventoryItemIsNotAStock = "The inventory item is not a stock item.";
		public const string InventoryItemIsAStock = "The inventory item is a stock item.";
		public const string InventoryItemIsNotaKit = "The inventory item is not a kit.";
		public const string InventoryItemIsATemplate = "The inventory item is a template item.";
		public const string InventoryItemIsNotATemplate = "The inventory item is not a template item.";
		public const string Document_Status_Invalid = "Document Status is invalid for processing.";
		public const string DocumentOutOfBalance = "Document is out of balance.";
		public const string TransitSiteIsNotAvailable = "The warehouse cannot be selected; it is used for transit.";
		public const string Document_OnHold_CannotRelease = "Document is On Hold and cannot be released.";
		public const string Inventory_Negative = "Inventory quantity for {0} in warehouse '{1} {2}' will go negative.";
		public const string Inventory_Negative2 = "Inventory quantity will go negative.";
		public const string SubItemSeg_Missing_ConsolidatedVal = "Subitem Segmented Key missing one or more Consolidated values.";
		public const string SubItemIsDisabled = "The {0} value of the {1} segment of the {2} inventory item is inactive. Activate the value on the Stock Items (IN202500) form.";
		public const string TranType_Invalid = "Invalid Transaction Type.";
		public const string InternalError = "Internal Error: {0}.";
		public const string NotPrimaryLocation = "Selected item is not allowed in this location.";
		public const string LocationReceiptsInvalid = "Selected Location is not valid for receipts.";
		public const string LocationSalesInvalid = "Selected Location is not valid for sales.";
		public const string LocationTransfersInvalid = "Selected Location is not valid for transfers.";
		public const string LocationAssemblyInvalid = "Assemblies are not allowed in the {0} location. Change the location, or select the Assembly Allowed check box on the Warehouses (IN204000) form for the current location.";
		public const string Location = "Location";
		public const string StandardCostNoCostOnlyAdjust = "Cost only adjustments are not allowed for Standard Cost items.";
		public const string StatusCheck_QtyNegative = "Updating item '{0} {1}' in warehouse '{2}' quantity available will go negative.";
		public const string StatusCheck_QtyNegativeSPC = "Adjustment cannot be released because the adjustment quantity exceeds the on-hand quantity of the '{0} {1}' item with the {2} lot or serial number.";
		public const string StatusCheck_QtyNegativeFifo = "Adjustment cannot be released because the adjustment quantity exceeds the on-hand quantity of the '{0} {1}' item received by the {2} receipt.";
		public const string StatusCheck_QtyNegativeFifoExactCost = "The document cannot be released because the return quantity exceeds the on-hand quantity of the {0} {1} item received by the {2} receipt.";
		public const string StatusCheck_QtyNegative1 = "Updating item '{0} {1}' in location '{2}' of warehouse '{3}' quantity available will go negative.";
		public const string StatusCheck_QtyNegative2 = "Updating item '{0} {1}' in warehouse '{2}' in cost layer '{3}' quantity  will go negative.";
		public const string StatusCheck_QtyAvailNegative = "Updating item '{0} {1}' in warehouse '{2}' quantity available for shipment will go negative.";
		public const string StatusCheck_QtyAvailNegative_Special = "The available quantity of the {0} special-order item in the {2} warehouse is not sufficient to process the document.";
		public const string StatusCheck_QtyLocationNegative = "Updating data for item '{0} {1}' on warehouse '{2} {3}' will result in negative available quantity.";
		public const string StatusCheck_QtyLotSerialNegative = "Updating item '{0} {1}' in warehouse '{2} {3}' lot/serial number '{4}' quantity available will go negative.";
		public const string StatusCheck_QtyOnHandNegative = "Updating item '{0} {1}' in warehouse '{2}' quantity on hand will go negative.";
		public const string StatusCheck_QtyOnHandNegative_Special = "The on-hand quantity of the {0} special-order item in the {2} warehouse is not sufficient to process the document.";
		public const string StatusCheck_QtyActualNegative = "The document cannot be released because the available-for-issue quantity would become negative for the {0} {1} item. This item is located in the {2} warehouse.";
		public const string StatusCheck_QtyActualNegative_Special = "The available-for-issue quantity of the {0} special-order item in the {2} warehouse is not sufficient to release the document.";
		public const string StatusCheck_QtyLotSerialActualNegative = "The document cannot be released because the available-for-issue quantity would become negative for the {0} item. This item has the {2} lot/serial number and is located in the {1} warehouse.";
		public const string StatusCheck_QtyLocationActualNegative = "The document cannot be released because the available-for-issue quantity would become negative for the {0} {1} item. This item is located in the {3} location of the {2} warehouse.";
		public const string StatusCheck_QtyLocationActualNegative_Special = "The available-for-issue quantity of the {0} special-order item in the {3} location of the {2} warehouse is not sufficient to release the document.";
		public const string StatusCheck_QtyLocationLotSerialActualNegative = "The document cannot be released because the available-for-issue quantity would become negative for the {0} {1} item. This item has the {4} lot/serial number and is located in the {3} location of the {2} warehouse.";
		public const string StatusCheck_QtyLocationLotSerialActualNegative_Special = "The available-for-issue quantity of the {0} special-order item with the {4} lot or serial number in the {3} location of the {2} warehouse is not sufficient to release the document.";
		public const string StatusCheck_QtyLotNbrHardAvailNegative = "The document cannot be saved because the quantity of the {0} item with the {2} lot number is not sufficient for shipping in the {1} warehouse.";
		public const string StatusCheck_QtyLotNbrHardAvailNegativeINScreens = "The document cannot be saved because the quantity of the {0} item with the {2} lot number is not sufficient for shipping in the {1} warehouse. To be able to save the document with the On Hold status, clear the Allocate Items in Documents on Hold check box on the General tab of the Inventory Preferences (IN101000) form.";
		public const string StatusCheck_QtyTransitOnHandNegative = "The document cannot be released. The quantity in transit for the '{0} {1}' item will become negative. To proceed, adjust the quantity of the item in the document.";
		public const string StatusCheck_QtyLocationOnHandNegative = "Updating item '{0} {1}' in warehouse '{2} {3}' quantity on hand will go negative.";
		public const string StatusCheck_QtyLocationOnHandNegative_Special = "The on-hand quantity of the {0} special-order item in the {3} location of the {2} warehouse is not sufficient to process the document.";
		public const string StatusCheck_QtyLotSerialOnHandNegative = "Updating item '{0} {1}' in warehouse '{2} {3}' lot/serial number '{4}' quantity on hand will go negative.";
		public const string StatusCheck_QtyLotSerialOnHandNegative_Special = "The on-hand quantity of the {0} special-order item with the {4} lot or serial number in the {3} location of the {2} warehouse is not sufficient to process the document.";
		public const string StatusCheck_QtyTransitLotSerialOnHandNegative = "The document cannot be released. The quantity in transit for the '{0} {1}' item with the '{2}' lot/serial number will become negative. To proceed, adjust the quantity of the item in the document.";
		public const string StatusCheck_QtyCostImblance = "Updating item '{0} {1}' in warehouse '{2}' caused cost to quantity imbalance.";
		public const string StatusCheck_QtyTransitLotSerialNegative = "The quantity in transit for the {0} item with the {1} lot/serial number will become negative.";
		public const string ItemAllocationsAvailabilityAffected = "Due to inventory item allocations, the available-for-shipping quantity of the {0} item will become negative. Reduce the allocated quantity by {1} {2} before releasing the adjustment. For details, see the Allocations Affected by Inventory Adjustments report.";
		public const string ItemLSAllocationsAvailabilityAffected = "Due to inventory item allocations, the available-for-shipping quantity of the {0} lot/serial number of the {1} item will become negative. Reduce the allocated quantity by {2} {3} before releasing the adjustment. For details, see the Allocations Affected by Inventory Adjustments report.";
		public const string EmptyAutoIncValue = "Auto-Incremental value is not set in {0}.";
		public const string LotSerTrackExpirationInvalid = "Only classes with enabled Track Expiration can use Expiration Issue Method.";
		public const string LotSerAssignCannotBeChanged = "The assignment method cannot be changed for the lot/serial class because it is assigned to at least one item on the Stock Items (IN202500) form.";
		public const string LotSerIssueMethodCannotBeChangedShipment = "The issue method cannot be changed because the {0} shipment contains the {1} item with the serial number of this class. Process the shipment with the current settings or delete the shipment, and then change the settings";
		public const string LotSerAutoNextNbrCannotBeChangedShipment = "The value of the 'Auto-Generate Next Number' checkbox cannot be changed because the {0} shipment contains the {1} item with the serial number of this class. Process the shipment with the current settings or delete the shipment, and then change the settings";
		public const string LotSerClass = "Lot/Serial Class";
		public const string InventoryItem = "Inventory Item";
		public const string InventoryItemCurySetting = "Inventory Item Currency Settings";
		public const string EmptyKitNotAllowed = "Selected kit cannot be added. The kit has no components specified.";
		public const string StartDateMustBeLessOrEqualToTheEndDate = "Start date must be less or equal to the end date.";
		public const string ValMethodCannotBeChanged = "Valuation method cannot be changed from '{0}' to '{1}' while stock is not zero.";
		public const string ValMethodCannotBeChangedTransit = "The valuation method cannot be changed from {0} to {1} because the item is in transit.";
		public const string ThisItemClassCanNotBeDeletedBecauseItIsUsedInInventorySetup = "This Item Class can not be deleted because it is used in Inventory Setup.";
		public const string ThisItemClassCanNotBeDeletedBecauseItIsUsedInInventoryItem = "This Item Class cannot be deleted because it is used for inventory item: {0}.";
		public const string StkItemValueCanNotBeChangedBecauseItIsUsedInInventoryItem = "The value of the Stock Item check box cannot be changed because the item class is assigned to the {0} item. Select another item class for this item first.";
		public const string ChildStkItemValueCanNotBeChangedBecauseItIsUsedInInventoryItem = "The value of the Stock Item check box cannot be changed for the {0} child item class because the item class is assigned to the {1} item. Select another item class for this item first.";
		public const string ThisItemClassCanNotBeDeletedBecauseItIsUsedInWarehouseLocation = "This Item Class can not be deleted because it is used in Warehouse/Location: {0}/{1}.";
		public const string TotalPctShouldBe100 = "Total % should be 100%";
		public const string ThisValueShouldBeBetweenP0AndP1 = "This value should be between {0} and {1}";
		public const string PercentageValueShouldBeBetween0And100 = "Percentage value should be between 0 and 100";
		public const string SpecificOnlyNumbered = "Specific valuated items should be lot or serial numbered during receipt.";
		public const string InsuffQty_LineQtyUpdated = "Insufficient quantity available. Line quantity was changed to match.";
		public const string SerialItem_LineQtyUpdated = "Invalid quantity specified for serial item. Line quantity was changed to match.";
		public const string SerialItemAdjustment_LineQtyUpdated = "Serialized item adjustment can be made for zero or one '{0}' items. Line quantity was changed to match.";
		public const string SerialItemAdjustment_UOMUpdated = "Serialized item adjustment can be made for zero or one '{0}' items. UOM was changed to match.";
		public const string SiteLocationOverride = "Update default location for all items on this site by selected location?";

		public const string Availability_Field = "Availability";
		public const string Availability_Info = "On Hand {1} {0}, Available {2} {0}, Available for Shipping {3} {0}";
		public const string Availability_Info_Project = "On Hand {1}/{4} {0}, Available {2}/{5} {0}, Available for Shipping {3}/{6} {0}";
		public const string Availability_ActualInfo = "On Hand {1} {0}, Available {2} {0}, Available for Shipping {3} {0}, Available for Issue {4} {0}";
		public const string Availability_ActualInfo_Project = "On Hand {1}/{5} {0}, Available {2}/{6} {0}, Available for Shipping {3}/{7} {0}, Available for Issue {4}/{8} {0}";
		public const string PIDBInconsistency = "Inconsistent DB data: The system cannot find the pair record (lock) in one of the following tables: INPIStatusItem, INPIStatusLoc.";
		public const string PICollision = "The system cannot run the PI because it has intersecting entities with PI {0}, which is in progress in warehouse {1}. See Trace for details.";
		public const string PICollisionDetails = "The system cannot run the current PI in warehouse {1} because the PI has intersecting entities with PI {0}, which is in progress. Intersecting inventory items: {2}; intersecting locations: {3}.";
		public const string PIAllInventoryCollisionDetails = "The system cannot run the current PI in warehouse {1} because the PI has intersecting entities with PI {0}, which is in progress. Intersecting locations: {2}.";
		public const string PIAllLocationsCollisionDetails = "The system cannot run the current PI in warehouse {1} because the PI has intersecting entities with PI {0}, which is in progress. Intersecting inventory items: {2}.";
		public const string PIFullCollisionDetails = "The system cannot run the current PI in the {1} warehouse because the full PI {0} is in progress.";
		[Obsolete]
		public const string PICountInProgressDuringRelease = "Physical count in progress for {0} in warehouse '{1} {2}'";
		public const string InventoryItemIsLockedInPIDocument = "You cannot change the quantity of the {0} item in the {1} location of the {2} warehouse because this item and location are used in the {3} physical inventory document. To review all locked items, see the Physical Inventory Locked Items (IN409000) inquiry.";
		public const string InventoryShouldBeUsedInCurrentPI = "Combination of selected Inventory Item and Warehouse Location is not allowed for this Physical Count.";
		public const string ThisCombinationIsUsedAlready = "This Combination Is Used Already in Line Nbr. {0}";
		public const string ThisSerialNumberIsUsedAlready = "This  Serial Number Is Used Already in Line Nbr. {0}";
		public const string ThisSerialNumberIsUsedInItem = "This Serial Number Is Used Already for the item";
		public const string PINotEnoughQtyInWarehouse = "Unable to create adjustment for line '{0}'. Insufficient Qty. On Hand for item '{1} {2}' in warehouse '{3}'.";
		public const string PINotEnoughQtyOnLocation = "Unable to create adjustment for line '{0}'. Insufficient Qty. On Hand for item '{1} {2}' in warehouse '{3} {4}'.";
		public const string ConfirmationXRefUpdate = "Substitute previous cross references information?";
		public const string AlternatieIDNotUnique = "Value '{0}' for Alternate ID is already used for another inventory item.";
		public const string FractionalUnitConversion = "Fractional unit conversions not supported for serial numbered items";
		public const string SiteUsageDeleted = "Unable to delete warehouse, item '{0}' has non-zero Quantity On Hand.";
		public const string ItemLotSerClassVerifying = "Lot/serial class cannot be changed when its tracking method is not compatible with the previous class and the item is in use.";
		public const string SerialNumberAlreadyIssued = "Serial Number '{1}' for item '{0}' already issued.";
		public const string SerialNumberAlreadyIssuedIn = "Serial Number '{1}' for item '{0}' already issued in '{2}'.";
		public const string SerialNumberAlreadyReceived = "Serial Number '{1}' for item '{0}' is already received.";
		public const string SerialNumberAlreadyReceivedIn = "Serial Number '{1}' for item '{0}' is already received in '{2}'.";
		public const string SerialNumberDuplicated = "Duplicate serial number '{1}' for item '{0}' is found in document.";
		public const string ItemWithSerialNumberAlreadyReceivedInWarehouse = "The {0} item with the {1} serial number has already been received in {2}.";
		public const string OneSerialNumberHaveAlreadyBeenReceived = "One item with a serial number has already been received. For details, see the trace log: Click Tools > Trace on the form title bar.";
		public const string SomeSerialNumbersHaveAlreadyBeenReceived = "{0} items with serial numbers have already been received. For details, see the trace log: Click Tools > Trace on the form title bar.";
		public const string NumericLotSerSegmentNotExists = "'{0}' segment must be defined for lot/serial class.";
		public const string NumericLotSerSegmentMultiple = "Multiple '{0}' segments defined for lot/serial class.";
		public const string SumOfAllComponentsMustBeHundred = "Total Percentage for Components must be 100. Please correct the percentage split for the components.";
		public const string SumOfAllComponentsMustBeLessHundredWithResiduals = "Total Percentage for Components must be less than 100 when there is a component with 'Residual' allocation method. Please correct the percentage split for the components.";
		public const string OnlyOneResidualComponentAllowed = "There must be only one component with 'Residual' allocation method for an item.";
		public const string ItemClassChangeWarning = "Please confirm if you want to update current Item settings with the Inventory Class defaults. Original settings will be preserved otherwise.";
		public const string ItemClassAndInventoryItemStkItemShouldBeSameSingleItem = "Inventory item {0} has not been moved to the {1} item class because moved item and the target item class should both be configured either as stock or as non-stock entities.";
		public const string ItemClassAndInventoryItemStkItemShouldBeSameManyItems = "Inventory items have not been moved to the {0} item class because all moved items and the target item class should both be configured either as stock or as non-stock entities. See trace for details.";
		public const string CouldNotBeMovedToItemClassItemsList = "Inventory items that cannot be moved to the {0} item class:";
		public const string DifferentItemsCouldNotBeMovedToItemClass = "You have selected both stock and non-stock items. They could not be moved to one item class.";
		public const string MissingUnitConversion = "Unit conversion is missing.";
		public const string MissingUnitConversionVerbose = "Unit conversion {0} is missing.";
		public const string MissingGlobalUnitConversion = "The conversion rule of the {0} unit of measure to the {1} unit of measure is not found on the Units of Measure (CS203100) form.";
		public const string DfltQtyShouldBeBetweenMinAndMaxQty = "Component Qty should be between Min. and Max. Qty.";
		public const string KitMayNotIncludeItselfAsComponentPart = "Kit May Not Include Itself As Component Part";
		public const string IssuesAreNotAllowedFromThisLocationContinue = "Issues are not allowed from this Location. Continue ?";
		public const string NonStockKitAssemblyNotAllowed = "Non-Stock Kit Assembly is not allowed.";
		public const string LSCannotAutoNumberItem = "Cannot generate the next lot/serial number for item {0}.";
		public const string LocationCostedWarning = "There is non zero Quantity on Hand for this item on selected Warehouse Location. You can only change Cost Separately option when the Qty on Hand is equal to zero";
		public const string LocationCostedSetWarning = "Last Inventory cost on warehouse will not be updated if the item has been received on this Warehouse Location.";
		public const string PeriofNbrCanNotBeGreaterThenInSetup = "Period Number can not be greater than Turnover Periods per Year on the InSetup.";
		public const string PossibleValuesAre = "Possible Values are: 1,2,3,4,6,12.";
		public const string TemplateItemExists = "This ID is already used for another template item. Specify another ID.";
		public const string NonStockItemExists = "This ID is already used for another Non-Stock Item.";
		public const string StockItemExists = "This ID is already used for another Stock Item.";
		public const string QtyOnHandExists = "There is non zero Quantity on Hand for this item. You can only change Cost when the Qty on Hand is equal to zero";
		public const string DecPlQtyChandedWhileQtyOnHandExists = "Some stock items in the system have a nonzero quantity on hand. Changing the decimal places of quantity may lead to unexpected consequences in the processing of inventory transactions with these items.";
		public const string PILineDeleted = "Unable to delete line, just manually added line can be deleted.";
		public const string PIEmpty = "Cannot generate the physical inventory count. List of details is empty.";
		public const string PIPhysicalQty = "Serial-numbered items should have physical quantity only 1 or 0.";
		public const string BinLotSerialNotAssigned = "One or more lines have unassigned Location and/or Lot/Serial Number";
		public const string BinLotSerialNotAssignedWithItemCode = "One or more lines for item '{0}' have unassigned Location and/or Lot/Serial Number";
		public const string AdjustmentsCreated = "The following adjustments have been created: {0}.";
		public const string SingleRevisionForNS = "Non-Stock kit can contain only one revision.";
		public const string RestictedSubItem = "Subitem status restricts using it for selected site.";
		public const string CantGetPrimaryView = "Can't get the primary view type for the graph {0}";
		public const string UnknownSegmentType = "Unknown segment type";
		public const string TooShortNum = "Lot/Serial Number must be {0} characters long";
		public const string UnableNavigateDocument = "Unable to navigate on document.";
		public const string ReplenihmentPlanDeleted = "Processing of replenishment with 0 quantity will delete previous plan.";
        public const string ReplenihmentSourceIsNotSelected = "No replenishment source has been specified for the item. Specify a replenishment source.";
		public const string PILineUpdated = "Item {0}{1} updated physical quantity {2} {3} line {4}.";
		public const string ConversionNotFound = "Unit Conversion is not setup on 'Units Of Measure' screen. Please setup Unit Conversion FROM {0} TO {1}.";
		public const string BoxesRequired = "At least one box must be specified in the Boxes grid for the given packaging option.";
		public const string ReplenishmentSourceSiteMustBeDifferentFromCurrenSite = "Replenishment Source Warehouse must be different from current Warehouse";
		public const string InactiveWarehouse = "Warehouse '{0}' is inactive";
		public const string InactiveLocation = "Location '{0}' is inactive";
		public const string SubitemDeleteError = "You cannot delete Subitem because it is already in use.";
		public const string CantDeactivateSite = "Can't deactivate warehouse. It has unreleased transactions.";
		public const string BranchCannotBeChanged = "The branch cannot be changed because the warehouse has items in stock.";
		public const string PeriodsOverlap = "Periods overlap.";
		public const string ItemCannotPurchase = "Item cannot be purchased";
		public const string ItemCannotSale = "Item cannot be sold";
		public const string ValueIsRequiredForAutoPackage = "Value is required for Auto packaging to work correctly.";
		public const string MaxWeightIsNotDefined = "Box Max. Weight must be defined for Auto Packaging to work correctly.";
		public const string MaxVolumeIsNotDefined = "Box Max. Volume must be defined for Auto Packaging to work correctly.";
		public const string ItemDontFitInTheBox = "The item can't fit the given Box.";
		public const string NonStockKitInKit = "It is not allowed to add non-stock kits as components to a stock kit or to a  non-stock kit.";
		public const string UOMRequiredForAccount = "{0} may not be empty for Account '{1}'";
		public const string CollumnIsMandatory = "Incorrect head in the file. Column \"{0}\" is mandatory";
		public const string ImportHasError = "Import has some error. The list of incorrect records is recorded in the Trace.";
		public const string RowError = "Row number {0}. Error message \"{1}\"";
		public const string ItemClassIsStock = "The class you have selected can not be assigned to a non-stock item, because the Stock Item check box is selected for this class on the Item Classes (IN201000) form. Select another item class which is designated to group non-stock items.";
		public const string ItemClassIsNonStock = "The class you have selected can not be assigned to a stock item, because the Stock Item check box is cleared for this class on the Item Classes(IN201000) form.Select another item class which is designated to group stock items.";
		public const string ItemWasDeleted = "The item was deleted";
		public const string ItemHasStockRemainder = "There is a non-zero quantity of the '{0}' item at the '{1}' warehouse.";
		public const string DiscountAccountIsNotSetupLocation = "Discount Account is not set up. See Location \"{0}\" for Customer \"{1}\" ";
		public const string DiscountAccountIsNotSetupCustomer = "Discount Account is not set up. See Customer \"{0}\" ";
		public const string WarehouseNotAllowed = "Selected Warehouse is not allowed in {0} transfer";
		public const string ProjectUsedInPO = "Project cannot be changed. Atleast one Unrelased PO Receipt exists for the given Project.";
		public const string TaskUsedInPO = "Project Task cannot be changed. Atleast one Unrelased PO Receipt exists for the given Project Task.";
		public const string ProjectUsedInSO = "Project cannot be changed. Atleast one Unrelased SO Shipment exists for the given Project.";
		public const string TaskUsedInSO = "Project Task cannot be changed. Atleast one Unrelased SO Shipment exists for the given Project Task.";
		public const string ProjectUsedInIN = "Project cannot be changed. Available Quantity on this location is not zero.";
		public const string TaskUsedInIN = "Project Task cannot be changed. Available Quantity on this location is not zero.";
		public const string LocationIsMappedToAnotherTask = "The Project Task specified for the given Location do not match the selected Project Task.";
		public const string MixedProjectsInSplits = "Splits cannot mix locations with different Project/Tasks. Please enter them as seperate lines.";
		public const string RequireSingleLocation = "When posting to Project Location must be the same for all splits.";
		public const string StandardCostItemOnProjectLocation = "Inventory revaluation cannot be performed because some of the stock items with pending standard costs are located in the {0} warehouse location which is linked to the {1} project. To perform the inventory revaluation, transfer these stock items to a warehouse location that is not linked to any project.";
		public const string AlternateIDDoesNotCorrelateWithCurrentSegmentRules = "The specified alternate ID does not comply with the INVENTORY segmented key settings. It might be not possible to use this alternate ID directly in entry forms.";
		public const string TransferLineIsCorrupted = "The warehouse in the document differs from the warehouse in the line. Remove the line and add it again to update the warehouse.";
		public const string TransferDocumentIsCorrupted = "The document is corrupted because the warehouse in the document differs from the warehouse in the {0} line. Remove the line and add it again to update the warehouse.";
		public const string UOMIsNotSpecifiedForTheItem = "The {0} UOM is not specified for the {1} item. Select another UOM in the line with this item, or specify the {0} UOM in the settings of the item.";
		public const string LineStockItemIsDifferFromSplitStockItem = "The inventory ID of the {0} item on the Details tab does not match the inventory ID in the Line Details dialog box. The document cannot be released. Please contact your Acumatica support provider.";

		public const string BaseUnitNotSmallest = "The base unit is not the smallest unit of measure available for this item. Ensure that the quantity precision configured in the system is large enough. See the Quantity Decimal Places setting on the Branches form.";
		public const string BaseUnitCouldNotBeChanged = "Base UOM cannot be changed for the item in use.";
		public const string FromUnitCouldNotBeEqualBaseUnit = "The entered unit is the base unit and cannot be used to convert from. Enter a different unit.";
		public const string NotDecimalBaseUnit = "The {0} base UOM is not divisible for the {1} item. Check conversion rules.";
		public const string NotDecimalSalesUnit = "The {0} sales UOM is not divisible for the {1} item.";
		public const string NotDecimalPurchaseUnit = "The {0} purchase UOM is not divisible for the {1} item.";
		public const string DecimalBaseUnitCouldNotUnchecked = "The {0} UOM cannot be changed to not divisible because the quantity of the item allocated for {1} is fractional.";
		public const string LocationIsNotActive = "Location is not Active.";
		public const string ZeroQtyWhenNonZeroCost = "Quantity cannot be zero when Ext. Cost is nonzero.";
		[Obsolete]
		public const string ProjectWildcardLocationIsUsedIn = "Project wildcard (without Task) is already setup for the Warehouse '{0}' Location '{1}'.";
		public const string DoesNotMatchWithAlternateType = "The alternate type for '{0}' is '{1}', which does not match the selected alternate type.";
		public const string VendorIsInactive = "The Vendor is inactive.";
		public const string CustomerIsInactive = "The Customer is inactive.";
		public const string FailedToProcessComponent = "Failed to process Component '{0}' when processing kit '{1}'. {2}";
		public const string MultipleAggregateChecksEncountred = "The '{0}' segment of the '{1}' segmented key has more than one value with the Aggregation check box selected  on the Segment Values (CS203000) form.";
		public const string LocationInUseInItemWarehouseDetails = "Location '{0}' is selected as default location in Item Warehouse Details for Item '{1}' and cannot be deleted.";
		public const string LocationInUseInPIType = "Location '{0}' is added to Physical Inventory Type '{1}' and cannot be deleted.";
		public const string InvalidPlan = "A transaction is missing allocation details. Please, delete current document and create a new one.";
		public const string CannotAddNonStockKit = "A non-stock kit cannot be added to a cash transaction.";
		public const string NotPossibleDeleteINAvailScheme = "This availability calculation rule cannot be deleted because it is assigned to at least one item class.";
		public const string EnteredItemClassIsNotStock = "The entered item class is not a stock item class.";
		public const string EnteredItemClassIsNotNonStock = "The entered item class is not a non-stock item class.";
		public const string ManyAltIDsForSingleInventoryID = "The specified alternate ID is assigned to multiple inventory items. Please select the appropriate inventory ID in the row.";
		public const string AltIDIsNotDefinedAndWillBeAddedOnRelease = "The specified alternate ID has not been defined for the selected inventory item on the Cross-Reference tab of the Stock Items (IN202500) or Non-Stock Items (IN202000) form. Upon release of the worksheet, the system assigns this alternate ID to the inventory item.";
		public const string AltIDIsNotDefinedAndWillNotBeAddedOnRelease = "The specified alternate ID is already defined for another inventory item and thus cannot be assigned to the inventory ID selected in this row.";
		public const string NoSpecifiedAltID = "The specified alternate ID cannot be found in the system.";
		public const string UOMAssignedToAltIDIsNotDefined = "The specified unit of measure is not defined for this inventory item.";
		public const string UnpostedDocsExist = "There are documents pending posting of inventory transactions to the closed period. Review the Unposted IN report (IN656500) for details.";
		public const string WrongUnitConversion = "The changes cannot be saved because the conversion factor for converting unit '{0}' to unit '{0}' differs from 1.";
        public const string INTranCostOverReceipted = "The document has not been released because the cost layer of the '{0} {1}' item was not updated. Try to release the document again.";
		public const string InactiveKitRevision = "Revision '{0}' is inactive";
		public const string LocationWithProjectLowestPickPriority = "There is a location without a project association with the same or lower pick priority. Consider specifying lower pick priority for the current location to ensure correct selection of a location for sales orders unrelated to projects.";
	    public const string ReplenishmentSourceSiteRequiredInTransfer = "Replenishment Warehouse cannot be empty.";
		public const string InactiveSegmentValues = "At least one value in each segment that requires validation should be selected on the SUBITEMS tab.";
		public const string ReasonCodeDoesNotMatch = "The usage type of the reason code does not match the document type.";
		public const string CannotReleaseAllocationsMissing = "The system cannot release the document because allocation of the {0} item in the {1} warehouse is not found. Reallocate the item in the document line #{2}.";
		public const string PITypeEarlyInventoryUnfreezeWarning = "Unfreezing stock when a PI process is not completed may cause discrepancy in cost or quantity of stock items and inability to release PI adjustments.";
		public const string PIGenerationEarlyInventoryUnfreezeWarning = "The Unfreeze Stock When Counting Is Finished check box is selected on the Physical Inventory Types (IN208900) form. This may cause discrepancy in cost or quantity of stock items and inability to release PI adjustments.";
		public const string PIBookQtyDecreasedGeneral = "The book quantity was decreased for several items. For details, see the trace log: Click Tools > Trace on the form title bar.";
		public const string PIBookQtyDecreased = "The book quantity of the {0} item in the {1} location will become negative with the current physical quantity due to inventory transactions generated after the finished counting. Increase the physical quantity of the item by {2} {3}. For details, see the Inventory Transactions History (IN405000) report.";
		public const string PIBookQtyDecreasedLS = "The book quantity of the {0} item with the {4} lot/serial number in the {1} location will become negative with the current physical quantity due to inventory transactions generated after the finished counting. Increase the physical quantity of the item by {2} {3}. For details, see the Inventory Transactions History (IN405000) report.";
		public const string BaseCompanyUomIsNotDefined = "Default values for weight UOM and volume UOM are not specified on the Companies (CS101500) form.";
		public const string TransferIsCorrupted = "The database record that corresponds to the {0} transfer is corrupted. Please contact your Acumatica support provider.";
		public const string WrongInventoryItemToUnitValue = "The {0} value specified in the To Unit box differs from the {2} base unit specified for the {1} item. To resolve the issue, please contact your Acumatica support provider.";
		public const string BaseConversionNotFound = "The conversion rule of the {0} unit of measure to the {1} unit of measure is not found for the {2} item. To resolve the issue, please contact your Acumatica support provider.";
		public const string WrongItemClassToUnitValue = "The {0} value specified in the To Unit box differs from the {2} base unit specified for the {1} item class. To resolve the issue, please contact your Acumatica support provider.";
		public const string TransferShouldBeProcessedThroughPO = "The {0} transfer receipt must be processed by using the Purchase Receipts (PO302000) form.";
		public const string KitSpecificationExists = "The check box cannot be cleared because a kit specification exists for this item.";
		public const string KitComponentEmptyReasonCode = "The Reason Code column cannot be empty for stock components with insufficient quantity on hand. Specify the reason code for the {0} component.";
		public const string CustomerGOGSAccountIsEmpty = "The inventory issue cannot be created because the COGS account is not specified for the customer. Specify the COGS account on the GL Accounts tab of the Customers (AR303000) form.";
		public const string ConversionCantModifyPlanExists = "The unit conversion cannot be modified because it is currently in use in the following document: {0}, {1}.";
		public const string ConversionCantModifyNotFullyCompletedTransactionExists = "The item's conversion rule for the purchase unit cannot be changed because the item has not been fully received, billed, or invoiced in the {0} document of the {1} type: {2}. Use the Purchase Accrual Balance by Period (PO402000) inquiry to view the complete list of such documents.";
		public const string MultiplierEqualsTo = "The multiplier must be equal to {0}.";
		public const string MultiplierEqualsTo2 = "The multiplier must be equal to {0} or {1}.";
		public const string ToLocationIsEmptyForAtLeastOneTransferLine = "The document cannot be released because the To Location ID column on the Details tab is empty for at least one transfer line.";

		public const string IntercompanyReceivedNotIssued = "At least one item with a serial number has not yet been issued from a warehouse of the selling company: the serial number {0} for the item {1}. Wait until the selling company issues the item, and then release the inventory receipt.";
		public const string IndivisiblePurchaseUOMRounded = "The value in the Qty. column has been rounded to an integer number because the purchase UOM of the {0} item is not divisible.";
		public const string IndivisibleBaseUOMRounded = "The value in the Qty. To Process column has been rounded to an integer number because the base UOM of the {0} item is not divisible.";
		public const string IndivisibleSalesUOMRounded = "The value in the Qty. column has been rounded to an integer number because the sales UOM of the {0} item is not divisible.";
		public const string QtyVariance = "The total quantity of the inventory adjustment line must be equal the variance quantity of the line {0} of the physical inventory document {1}.";
		public const string SiteBaseCurrencyDiffers = "The {0} branch specified for the {1} warehouse has other base currency than the {2} branch that is currently selected.";
		public const string AdjustmentError = "Inserting 'IN adjustment line' record raised at least one error. Please review the errors.";
		public const string ItemDefaultSiteBaseCurrencyDiffers = "The {0} branch specified for the {1} warehouse has other base currency than the {2} branch that is currently selected.";
		public const string ReplenishmentSiteDiffers = "The {0} branch specified for the {1} warehouse has other base currency than the branch of the currently selected warehouse.";
		public const string ReplenishmentVendorDiffers = "The {0} vendor has other base currency than the branch of the currently selected warehouse.";
		public const string POVendorBaseCurrencyDiffers = "The {0} vendor has other base currency than the current branch.";
		public const string ReplenishmentSourceSiteBaseCurrencyDiffers = "The base currency of the {0} branch differs from the base currency of the {1} branch specified for the {2} warehouse.";
		public const string CannotChangeTheBranchIfHistoryExists = "Cannot change the branch as the transactions history of the {0} warehouse has transactions in the {1} currency.";
		public const string CustomerOrVendorHasDifferentBaseCurrency = "The branch base currency differs from the base currency of the {0} entity associated with the {1} business account.";
		public const string CarrierSiteBaseCurrencyDiffers = "The {0} branch specified for the {1} warehouse is restricted in the {2} account.";
		public const string CannotChangeRestricToIfPOVendorInventoryExists = "A branch with the base currency other than {0} cannot be associated with the {1} vendor because {1} is added to the vendor's list of the {2} item.";
		public const string CannotChangeRestricToIfPOVendorInventoryExistsNull = "This box must remain blank because {0} is added to the list of vendors in the settings of the {1} item.";
		public const string CannotChangeRestricToIfINItemSiteExists = "A branch with the base currency other than {0} cannot be associated with the {1} vendor because {1} is specified as the preferred vendor in the {3} warehouse details of the {2} item.";
		public const string SpecialCostLayerPositiveAdjustment = "A positive adjustment cannot be made for a line with the cost layer of the Special type.";
		public const string SpecialCostLayerIssueType = "The lines with the cost layer of the Special type can have only the Issue value in the Tran. Type column.";
		public const string SpecaiCostLayerOrderNbr = "The Special Order Nbr. column cannot be empty.";
		public const string SpecaiCostLayerToOrderNbr = "The To Special Order Nbr. column cannot be empty.";
		public const string SpecialCostLayerNegativeQty = "The quantity of the {0} item allocated for the {1} sales order is not sufficient to process the document. The quantity will become negative.";
		public const string SpecialCostLayerOneStepTransfer = "You can select the cost layer of the Special type only for one-step transfers.";
		public const string SpecialCostLayerTransferToDifferentWarehouse = "Special-order items cannot be transferred between warehouses on this form. To transfer the special-order items, use the Create Transfer Orders (SO509000) form.";
		public const string SpecialCostLayerPIAdjustmentShouldBeReviewed = "The PI adjustment has been created with the Balanced status. Review and release the adjustment.";
		public const string DeletedAttributesOnConvert = "The attributes of the current item class do not match the attributes of the previous item class. Review the attributes on the Attributes tab before you save the item.";
		public const string ActionNotAvailableInCurrentState = SO.Messages.ActionNotAvailableInCurrentState;
		public const string InvalidStatusSiteID = "The warehouse of the status table does not coincide with the warehouse of the cost center. Please contact your Acumatica support provider.";
		public const string InvalidLocationSiteID = "The warehouse of the status table does not coincide with the warehouse of the location. Please contact your Acumatica support service.";
		public const string ImportTaxCanNotBeApplied = "The {0} tax category containing the {1} direct-entry tax cannot be selected for a stock item.";
		public const string WarehouseCannotBeActivated = "The {0} warehouse cannot be activated because the related {1} branch is inactive.";
		public const string LotSerialLeadingSpaceIsNotAllowed = "The value in the Lot/Serial Number column cannot have leading spaces.";
		public const string CharacterCannotBeUsedAsPartOfSeparator = "The {0} character cannot be used as a separator because it is used as a prompt character for the INVENTORY segmented key on the Segmented Keys (CS202000) form.";
		public const string AlternateIDExistsUOMIsDifferent = "The {0} alternate ID already exists for the {1} item, but with the {2} UOM. Specify another alternate ID in the document, or remove the UOM specified for the current alternate ID on the Cross-Reference tab of the Stock Items (IN202500) or Non-Stock Items (IN202000) forms.";
		public const string BaseQtyIncorrect = "The {0} document with the {1} number cannot be released due to incorrect base quantity in the line {2}. You can try to delete the line and create a new one, or you can contact your Acumatica support provider.";
		public const string QuantityDecimalPlacesCanNotBeChangedUnreleasedDocument = "The decimal places of quantity cannot be changed if unreleased inventory documents exist in the system. Make sure that there are no inventory documents with the On Hold status, and use the Release IN Documents (IN501000) form to process the documents with the Balanced status.";
		public const string EnterAllLinesToCompletePI = "The PI count cannot be completed because one or more lines have not been entered.";
		#endregion

		#region Translatable Strings used in the code
		public const string LS = "Lot/Serial";
		public const string EmptyLS = "<SPLIT>";
		public const string Unassigned = "<UNASSIGNED>";
		public const string ExceptLocationNotAvailable = "[*]  Except Location Not Available";
		public const string ExceptExpiredNotAvailable = "[**] Except Expired and  Loc. Not Available";
		public const string EstimatedCosts = "[*]  Estimated Costs";
		public const string CustomerID = "Customer ID";
		public const string Customer = "Customer";
		public const string CustomerName = "Customer Name";
		public const string Contact = "Contact";
		public const string ReceiptType = "Receipt Type";
		public const string ReceiptNbr = "Receipt Nbr.";
		public const string ExpireDateLessThanStartDate = "Expire Date must be greater than Start Date";
		public const string ProductionVarianceTranDesc = "Production Variance";
		public const string SeasonalSettingsAreOverlaped = "Seasonal settings are not defined correctly (overlap detected)";
		public const string AttemptToComparePeriodsOfDifferentType = "Period of different types can not be compared";
		public const string ThisTypeOfForecastModelIsNotImplemetedYet = "The model type {0} is not implemented yet";
		public const string InternalErrorSequenceIsNotSortedCorrectly = "InternalError: Sequence's  sorting order is wrong or it's not sorted";

		public const string OverrideInventoryAcctSub = "Override Inventory Account/Sub.";
		public const string OverrideInventoryAcct = "Override Inventory Account.";
		public const string SearchableTitleKit = "Kit: {0} {1}";
		public const string NoDefaultTermSpecified = "For items with no Default Term, the system cannot calculate Term End Date.";

		public const string UnknownDocumentType = "The Document Type is unknown.";
		public const string NotEnteredLineDataError = "Line data should be entered.";
		public const string UnknownPiTagSortOrder = "Unknown PI Tag # sort order";

		public const string Confirmation = "Confirmation";
		public const string ConfirmItemClassApplyToChildren = "The settings of this item class will be assigned to its child item classes, which might override the custom settings. Please confirm your action.";
		public const string ConfirmItemClassDeleteKeepChildren = "The item class that you want to delete has child item classes. Would you like to keep the child item classes? (If you keep the child item classes, they will become children of the item class at the level immediately above the deleted class.)";
		public const string DuplicateItemClassID = "The {0} item class ID already exists. Specify another item class ID.";
		public const string CopyingSettingsFailed = "Copying settings from the selected item class has completed with errors; some settings have not been copied. Try to select the item class again and save the changes.";
		public const string FinancialPeriodClosedInIN = "The {0} financial period of the {1} company is closed in Inventory.";
		public const string TypeMustImplementInterface = "The specified type {0} must implement the {1} interface.";

		public const string NewKey = "<NEW>";
		public const string AlternateIDUnit = "Alt. ID Unit";

		public const string SellingCompanyBranch = "Selling Company/Branch";
		public const string PurchasingCompanyBranch = "Purchasing Company/Branch";

		public const string ListPlaceholder = "<LIST>";

		#endregion

		#region Graph Names
		public const string INItemSiteMaint = "Item Warehouse Details";
		public const string INSetup = "Inventory Preferences";
		public const string INSetupMaint = "IN Setup";
		public const string InventoryAllocDetEnq = "Inventory Allocation Details";
		public const string InventoryTranDetEnq = "Inventory Transaction Details";
		public const string INPIReview = "Physical Inventory Review";
		public const string KitSubstitutionIsRestricted = "Manual Component substitution is not allowed by the Kit specification.";
		public const string KitQtyVarianceIsRestricted = "Quantity is dictated by the Kit specification and cannot be changed manualy for the given component.";
		public const string KitQtyOutOfBounds = "Quantity is out of bounds. Specification dictates that it should be within [{0}-{1}] {2}.";
		public const string KitQtyNotEvenDistributed = "Quantity of Components is not valid. Quantity must be such that it can be uniformly distributed among the kits produced.";
		public const string KitItemMustBeUniqueAccrosSubItems = "Component Item must be unique for the given Kit accross Component ID and Subitem combinations.";
		public const string KitItemMustBeUnique = "Component Item must be unique for the given Kit.";
		public const string UsingKitAsItsComponent = "Non-stock kit can't using as its own component";
		public const string SNComponentInSNKit = "Serial-numbered components are allowed only in serial-numbered kits";
		public const string WhenUsedComponentInKit = "Components with the 'When-Used' assignment method and 'User-Enterable' issue method are not allowed in non-stock kits";
		public const string SerialNumberedComponentMustBeInBaseUnitOnly = "You can add serial tracked components with only a base UOM ('{0}') to the kit specification.";
		#endregion

		#region Cache Names
		public const string Warehouse = "Warehouse";
		public const string WarehouseBuilding = "Warehouse Building";
		public const string ItemClass = "Item Class";
		public const string ItemClassCurySettings = "Item Class Currency Settings";
		public const string INItemClassRep = "Item Class Replenishment";
		public const string Equipment = "Equipment";
		public const string Register = "Receipt";
		public const string INSite = "Warehouse";
		public const string ItemWarehouseSettings = "Item/Warehouse Settings";
		public const string PostingClass = "Posting Class";
		public const string KitSpecification = "Kit Specification";
		public const string ReplenishmentPolicy = "Replenishment Policy";
		public const string INSubItem = "IN Sub Item";
		public const string INItemSiteReplenishment = "SubItem Replenishment Info";
		public const string InventoryUnitConversions = "Inventory Unit Conversions";
		public const string DeferredRevenueComponents = "Deferred Revenue Components";
		public const string ItemCostStatistics = "Item Cost Statistics";
		public const string ItemReplenishmentSettings = "Item Replenishment Settings";
		public const string SubitemReplenishmentSettings = "Subitem Replenishment Settings";
		public const string INReplenishmentClass = "Replenishment Class";
		public const string INReplenishmentOrder = "Replenishment Order";
		public const string INReplenishmentLine = "Replenishment Line";
		public const string INReplenishmentSeason = "Replenishment Seasonality";
		public const string XReferences = "Cross-Reference";
		public const string INComponentTran = "IN Component";
		public const string INOverheadTran = "IN Overhead";
		public const string INTran = "IN Transaction";
		public const string INComponentTranSplit = "IN Component Split";
		public const string INKitTranSplit = "IN Kit Split";
		public const string INTranSplit = "IN Transaction Split";
		public const string INKit = "IN Kit";
		public const string INKitSpecNonStkDet = "Non-Stock Component of Kit Specification";
		public const string INKitSpecStkDet = "Stock Component of Kit Specification";
		public const string INLocationStatus = "IN Location Status";
		public const string INLocationStatusByCostCenter = "IN Location Status by Cost Center";
		public const string INLocationStatusByCostLayerType = "IN Location Status by Cost Layer Type";
		public const string INLotSerialStatus = "IN Lot/Serial Status";
		public const string INLotSerialStatusByCostCenter = "IN Lot/Serial Status by Cost Center";
		public const string INLotSerialStatusByCostLayerType = "IN Lot/Serial Status by Cost Layer Type";
		public const string INLotSerialCostStatusByCostLayerType = "IN Lot/Serial Cost Status by Cost Layer Type";
		public const string INItemLotSerial = "Lot/Serial by Item";
		public const string INSiteLotSerial = "Lot/Serial by Warehouse";
		public const string INLotSerSegment = "Lot/Serial Segment";
		public const string INSiteStatus = "IN Site Status";
		public const string INSiteStatusByCostCenter = "IN Site Status by Cost Center";
		public const string INSiteStatusByCostCenterShort = "IN Site Status by Cost Center Short";
		public const string INSiteStatusByCostLayerType = "IN Site Status by Cost Layer Type";
		public const string INSiteCostStatusByCostLayerType = "IN Site Cost Status by Cost Layer Type";
		public const string INItemSiteHistByPeriod = "IN Item Site History by Period";
		public const string INItemSiteHistDay = "IN Item Site History Day";
		public const string INItemSiteHistByDay = "IN Item Site History by Day";
		public const string INItemSiteHistByLastDayInPeriod = "IN Item Site History by Last Day In Period";
		public const string INTranDetail = "IN Transaction Detail";
		public const string INCostStatusSummary = "IN Cost Status Summary";
		public const string INLocation = "IN Location";
		public const string INTranCost = "IN Transaction Cost";
		public const string INCostStatus = "IN Cost Status";
		public const string INItemCostHistByPeriod = "IN Item Cost History by Period";
		public const string INPIDetail = "IN Physical count Detail";
		public const string INPIClass = "Physical Inventory Type";
		public const string INPIClassItem = "Physical Inventory Type by Item";
		public const string INPIClassLocation = "Physical Inventory Type by Location";
		public const string INPICycle = "Physical Inventory Cycle";
		public const string INPIStatus = "Physical Inventory Status";
		public const string INItemSiteHist = "IN Item Site History";
		public const string INItemCostHist = "IN Item Cost History";
		public const string INItemSalesHist = "Item Sales History";
		public const string INCategory = "Item Sales Category";
		public const string INItemCategory = "Item Sales Category by Item";
		public const string INSiteStatusSummary = "IN Warehouse Status";
		public const string INAvailabilityScheme = "Availability Calculation Rule";
		public const string INSubItemSegmentValue = "IN Subitem Segment Value";
		public const string INPriceClass = "IN Item Price Class";
		public const string INItemPlan = "IN Item Plan";
		public const string INItemPlanType = "IN Item Plan Type";
		public const string INItemStats = "IN Item Statistics";
		public const string INMovementClass = "IN Movement Class";
		public const string INABCCode = "IN ABC Code";
		public const string INTote = "IN Tote";
		public const string INCart = "IN Cart";
		public const string INCartSplit = "IN Cart Split";
		public const string INCartContentByLocation = "IN Cart Content by Location";
		public const string INCartContentByLotSerial = "IN Cart Content by Lot/Serial Nbr.";
		public const string INStoragePlace = "IN Storage Place";
		public const string INStoragePlaceSplit = "IN Storage Place Split";
		public const string INStoragePlaceStatus = "IN Storage Place Status";
		public const string INStoragePlaceStatusExpanded = "IN Storage Place Detailed Status";
		public const string INScanSetup = "IN Scan Setup";
		public const string INScanUserSetup = "IN Scan User Setup";
		public const string WMSJob = "IN WMS Job";
		public const string StockItemAutoIncrementalValue = "Auto-Incremental Value of a Stock Item";
		public const string LotSerClassAutoIncrementalValue = "Auto-Incremental Value of a Lot/Serial Class";
		public const string GS1UOMSetup = "GS1 Unit Setup";
		public const string INItemBox = "IN Item Box";
		public const string INDeadStockEnqFilter = "IN Dead Stock Enquiry Filter";
		public const string INDeadStockEnqResult = "IN Dead Stock Enquiry Result";
		public const string IntercompanyGoodsInTransitFilter = "Intercompany Goods in Transit Filter";
		public const string IntercompanyGoodsInTransitResult = "Intercompany Goods in Transit Result";
		public const string IntercompanyReturnedGoodsInTransitResult = "Intercompany Returned Goods in Transit Result";
		public const string INCostCenter = "IN Cost Center";
		public const string MassConvertStockNonStockFilter = "Change Stock Status of Items Filter";
		public const string InventoryLinkFilter = "Inventory List Filter";
		public const string LocationLinkFilter = "Location List Filter";
		public const string INConversionHistory = "Inventory Conversion History";
		public const string AdjustmentTranBySiteLotSerial = "Adjustment Transactions grouped by SiteLotSerial";
		public const string AdjustmentTranBySiteStatus = "Adjustment Transactions grouped by SiteStatus";
		public const string TotalAdjustmentLotSerialBySite = "Total LotSerials from Adjustment affected on Site";
		public const string Allocation = "Allocations";
		public const string INRecalculateInventoryFilter = "Recalculate Inventory Filter";
		public const string InventoryItemCommon = "Inventory Item Common Fields Only";
		#endregion

		#region Combo Values

		public const string ModulePI = "PI";

		#region Inventory Mask Codes
		public const string MaskItem = "Inventory Item";
		public const string MaskSite = "Warehouse";
		public const string MaskClass = "Posting Class";
		public const string MaskReasonCode = "Reason Code";
		public const string MaskVendor = "Vendor";
		#endregion

		#region Item Types
		public const string NonStockItem = "Non-Stock Item";
		public const string LaborItem = "Labor";
		public const string ServiceItem = "Service";
		public const string ChargeItem = "Charge";
		public const string ExpenseItem = "Expense";

		public const string FinishedGood = "Finished Good";
		public const string Component = "Component Part";
		public const string SubAssembly = "Subassembly";
		#endregion

		#region Valuation Methods
		public const string Standard = "Standard";
		public const string Average = "Average";
		public const string FIFO = "FIFO";
		public const string Specific = "Specific";
		#endregion

		#region Lot Serial Assignment
		public const string WhenReceived = "When Received";
		public const string WhenUsed = "When Used";
		#endregion

		#region Lot Serial Tracking
		public const string NotNumbered = "Not Tracked";
		public const string LotNumbered = "Track Lot Numbers";
		public const string SerialNumbered = "Track Serial Numbers";
		#endregion

		#region Lot Serial Issue Method
		public const string LIFO = "LIFO";
		public const string Sequential = "Sequential";
		public const string Expiration = "Expiration";
		public const string UserEnterable = "User-Enterable";
		#endregion

		#region Lot Serial Segment Type
		public const string NumericVal = "Auto-Incremental Value";
		public const string FixedConst = "Constant";
		public const string DayConst = "Day";
		public const string MonthConst = "Month";
		public const string MonthLongConst = "Month Long";
		public const string YearConst = "Year";
		public const string YearLongConst = "Year Long";
		public const string DateConst = "Custom Date Format";
		#endregion

		#region Transaction / Journal Types
		public const string Assembly = "Assembly";
		public const string Receipt = "Receipt";
		public const string Issue = "Issue";
		public const string Return = "Return";
		public const string Invoice = "Invoice";
		public const string DebitMemo = "Debit Memo";
		public const string CreditMemo = "Credit Memo";
		public const string Transfer = "Transfer";
		public const string Adjustment = "Adjustment";
		public const string Undefined = "Not Used in Inventory";
		public const string StandardCostAdjustment = "Standard Cost Adjustment";
		public const string NegativeCostAdjustment = "Negative Cost Adjustment";
		public const string ReceiptCostAdjustment = "Receipt Cost Adjustment";
		public const string NoUpdate = "No Update";
		public const string Production = "Production";
		public const string Disassembly = "Disassembly";
		public const string AssemblyDisassembly = "Assembly/Disassembly";
		public const string DropShip = "Drop-Shipment";

		#endregion

		#region Transfer Types
		public const string OneStep = "1-Step";
		public const string TwoStep = "2-Step";
		#endregion

		#region Item Status
		public const string Active = "Active";
		public const string NoSales = "No Sales";
		public const string NoPurchases = "No Purchases";
		public const string NoRequest = "No Request";
		public const string Inactive = "Inactive";
		public const string ToDelete = "Marked for Deletion";
		#endregion

		#region Qty Allocation Doc Type
		public const string qadSOOrder = "SO Order";
        public const string qadFSServiceOrder = "FS Order";
        #endregion

        #region Document Status
        public const string Hold = "On Hold";
		public const string Balanced = "Balanced";
		public const string Released = "Released";

		// some additional statuses for PIHeader
		public const string Counting = "Counting In Progress";
		public const string DataEntering = "Data Entering";
		public const string InReview = "In Review";
		public const string Completed = "Completed";
		public const string Cancelled = "Canceled";

		// some additional statuses for PIDetail
		public const string NotEntered = "Not Entered";
		public const string Entered = "Entered";
		public const string Skipped = "Skipped";

		// LineType for PIDetail
		public const string Blank = "Blank";
		public const string UserEntered = "UserEntered";

		#endregion

		#region Primary Item Validation
		public const string PrimaryNothing = "No Validation";
		public const string PrimaryItemError = "Primary Item Error";
		public const string PrimaryItemClassError = "Primary Item Class Error";
		public const string PrimaryItemWarning = "Primary Item Warning";
		public const string PrimaryItemClassWarning = "Primary Item Class Warning";
		#endregion

		#region Location Validation Types
		public const string LocValidate = "Do Not Allow On-the-Fly Entry";
		public const string LocNoValidate = "Allow On-the-Fly Entry";
		public const string LocWarn = "Warn But Allow On-the-Fly Entry";
		#endregion

		#region Alternate Types
		public const string CPN = "Customer Part Number";
		public const string VPN = "Vendor Part Number";
		public const string Global = "Global";
		public const string Barcode = "Barcode";
		public const string GIN = "GTIN/EAN/UPC/ISBN";
		#endregion

		#region Layer Types
		public const string Normal = "Normal";
		public const string Oversold = "Oversold";
        public const string Unmanaged = "Unmanaged";
        #endregion

        #region Physical Inventory Types
        public const string ByInventory = "By Inventory";
		#endregion

		#region PrimaryItemValidationType

		public const string Warning = "Warning";
		public const string Error = "Error";

		#endregion


		#region INPriceOption

		public const string Percentage = "Percentage";
		public const string FixedAmt = "Fixed Amount";
		public const string Residual = "Residual";
		public const string FairValue = "Fair Value";

		#endregion

		#region INReplenishmentType
		public const string None = "None";
		public const string MinMax = "Min./Max.";
		public const string FixedReorder = "Fixed Reorder Qty";
		#endregion

		#region Planning Method
		public const string DRP = "DRP";
		public const string MRP = "MRP";
		public const string InventoryReplenishment = "Inventory Replenishment";
		#endregion

		#region INReplenishmentSource
		public const string Purchased = "Purchase";
		public const string Manufactured = "Manufacturing";
		public const string PurchaseToOrder = "Purchase to Order";
		public const string DropShipToOrder = "Drop-Ship";
		public const string BlanketForDropShip = "Blanket for Drop-Ship";
		public const string BlanketForPurchaseToOrder = "Blanket for Normal";
		#endregion

		#region Cost Source
		public const string AverageCost = "Average";
		public const string LastCost = "Last";
		#endregion

		#region PackageOption
		public const string Weight = "By Weight";
		public const string Quantity = "By Quantity";
		public const string WeightAndVolume = "By Weight & Volume";
		public const string Manual = "Manual";
		#endregion
		
		#region CostBasisOption
		public const string StandardCost = "Standard Cost";
		public const string PriceMarkupPercent = "Markup %";
		public const string PercentOfSalesPrice = "Percentage of Sales Price";
		#endregion

		#region Demand Period Type
		public const string Month = "Month";
		public const string Week = "Week";
		public const string Day = "Day";
		public const string Quarter = "Quarter";

		#endregion

		#region DemandForecastModelType
		public const string DFM_None = "None";
		public const string DFM_MovingAverage = "Moving Average";

		#endregion

		#region Demand Calculation
		public const string ItemClassSettings = "Item Class Settings";
		public const string HardDemand = "Hard Demand Only";
		#endregion
		#region CompletePOLine
		public const string ByAmount = "By Amount";
		public const string ByQuantity = "By Quantity";
		#endregion

		#region Cost Center Types
		public const string Project = "Project";
		public const string Special = "Special";
		public const string ProductionOrder = "Production Order";
		#endregion
		#endregion

		#region Custom Actions
		public const string Release = PM.Messages.Release;
		public const string ReleaseAll = PM.Messages.ReleaseAll;
		public const string Process = "Process";
		public const string ProcessAll = "Process All";
		public const string ViewInventoryTranDet = "Inventory Transaction Details";
		public const string INEditDetails = "Inventory Edit Details";
		public const string INRegisterDetails = "Inventory Register Detailed";
		public const string INItemLabels = "Inventory Item Labels";
		public const string INLocationLabels = "Location Labels";
		public const string GeneratePI = "Generate PI";
		public const string FinishCounting = "Finish Counting";
		public const string CancelPI = "Cancel PI";
		public const string CompletePI = "Complete PI";
		public const string InventorySummary = "Inventory Summary";
		public const string InventoryAllocDet = "Allocation Details";
		public const string InventoryTranSum = "Transaction Summmary";
		public const string InventoryTranHist = "Transaction History";
		public const string InventoryTranDet = "Transaction Details";
		public const string SetNotEnteredToZero = "Set Not Entered To Zero";
		public const string SetNotEnteredToSkipped = "Set Not Entered To Skipped";
		public const string UpdateCost = "Update Actual Cost";
		public const string BinLotSerial = "Line Details";
		public const string Generate = "Generate";
		public const string Add = "Add";
		public const string AddNewLine = "Add New Line";
		public const string ViewRestrictionGroup = "Group Details";
		public const string ApplyRestrictionSettings = "Apply Restriction Settings to All Inventory Items";
		public const string Calculate = "Calculate";
		public const string Clear = "Clear";
		public const string ttipRefresh = "Refresh";
		public const string ApplyToChildren = "Apply to Children";
		public const string ttipCutSelectedRecords = "Cut Selected Records";
		public const string ttipPasteRecords = "Paste Records";

		#endregion

		#region PI Generation Sort Order Combos
		public const string ByLocationID = "By Location";
		public const string ByInventoryID = "By Inventory ID";
		public const string BySubItem = "By Subitem";
		public const string ByLotSerial = "By Lot/Serial Number";
		public const string ByInventoryDescription = "By Inventory Description";
		#endregion

		#region PI Generation Methods
		public const string FullPhysicalInventory = "Full Physical Inventory";
		public const string ByCycleCountFrequency = "By Cycle Count Frequency";
		public const string ByMovementClassCountFrequency = "By Movement Class Count Frequency";
		public const string ByABCClassCountFrequency = "By ABC Code Count Frequency";
		public const string ByCycleID = "By Cycle";
		public const string LastCountDate = "Last Count On Or Before";
		public const string ByPreviousPIID = "By Previous Physical Count";
		public const string ByItemClassID = "By Item Class";
		public const string ListOfItems = "List Of Items";
		public const string RandomlySelectedItems = "Random Items (up to)";
		public const string ItemsHavingNegativeBookQty = "Items Having Negative Book Qty.";
		public const string ByMovementClass = "By Movement Class";
		public const string ByABCClass = "By ABC Code";

		#endregion

		public const string InTransit = "In-Transit";
		public const string InTransitLine = "Transfer Line";
		public const string InTransitS = "In-Transit [*]";
		public const string InTransit2S = "In-Transit [**]";
		public const string SOBooked = "SO Booked";
		public const string SOBookedS = "SO Booked [*]";
		public const string SOBooked2S = "SO Booked [**]";
		public const string SOAllocated = "SO Allocated";
		public const string SOAllocatedS = "SO Allocated [*]";
		public const string SOAllocated2S = "SO Allocated [**]";
		public const string SOShipped = "SO Shipped";
		public const string SOShippedS = "SO Shipped [*]";
		public const string SOShipped2S = "SO Shipped [**]";
		public const string INIssues = "IN Issues";
		public const string INIssuesS = "IN Issues [*]";
		public const string INIssues2S = "IN Issues [**]";
		public const string INReceipts = "IN Receipts";
		public const string INReceiptsS = "IN Receipts [*]";
		public const string Expired = "Expired";
		public const string ExpiredS = "Expired [*]";
		public const string ExceptExpiredS = "[*] Except Expired";
		public const string ExceptExpired2S = "[**] Except Expired and  Loc. Not Available";
		public const string TotalLocation = "Total:";

		public const string RegisterCart = "Receipt Cart";
		public const string RegisterCartLine = "Receipt Cart Line";
		public const string ReceiptDate = "Receipt Date";
		
		#region Matrix
		public const string AdditionalAttributesDAC = "Additional Attributes";
		public const string INMatrixGenerationRuleDAC = "Matrix Generation Rule";
		public const string IDGenerationRuleDAC = "ID Generation Rule";
		public const string DescriptionGenerationRuleDAC = "Description Generation Rule";
		public const string EntityHeaderDAC = "Entity Header";
		public const string EntityMatrixDAC = "Entity Matrix";
		public const string InventoryItemWithAttributeValuesDAC = "Inventory Item with Attribute Values";
		public const string TemplateAttributesDAC = "Template Attributes";
		public const string AttributeDescriptionGroupDAC = "Attribute Description Group";
		public const string AttributeDescriptionItemDAC = "Attribute Description Item";
		public const string LinesWithSameInventoryHaveDifferentUOM = "Specify the same UOM in all lines with the selected inventory item before using inventory matrix.";
		public const string ItIsNotAllowedToChangeStkItemFlagIfChildExists = "You cannot change the value of the Stock Item check box if the template item has at least one matrix item. Remove all matrix items of the template item first.";
		public const string ItIsNotAllowedToChangeMainFieldsIfChildExists = "You cannot change the values of the Item Class, Base Unit, and Sales Categories boxes if the template item has at least one matrix item. Remove all matrix items of the template item first.";
		public const string InventoryIDExists = "The item with the same inventory ID already exists. Change segment settings of the inventory ID.";
		public const string InventoryIDDuplicates = "The inventory ID is duplicated. Change segment settings of the inventory ID.";
		public const string SelectRow = "Select Row";
		public const string SelectColumn = "Select Column";
		public const string TotalQty = "Total Qty.";
		public const string StkItemSettingMustCoincide = "The item class specified for the stock item must be the same as in the template item.";
		public const string CantChangeAttributeCategoryForMatrixItem = "The value in the Category column cannot be changed if at least one matrix item exists with this item class assigned. Remove all matrix items of the template item first.";
		public const string CantChangeAttributeCategoryForMatrixTemplate = "The attribute category cannot be changed because this attribute is specified as the default column or row attribute for the following templates on the Template Items (IN203000) form: {0}. Select another attribute as the default column or row attribute for the templates first.";
		public const string CantChangeAttributeIsActiveFlagForMatrixItem = "The value of the Active check box for the variant attribute cannot be changed if at least one matrix item exists with this item class assigned. Remove all matrix items of the template item first.";
		public const string CantChangeAttributeIsActiveFlagForMatrixTemplate = "The Active check box cannot be cleared for this attribute because it is specified as the default column or row attribute for the following templates on the Template Items (IN203000) form: {0}. Select another attribute as the default column or row attribute for the templates first.";
		public const string CantDeleteVariantAttributeForMatrixItem = "The {0} attribute cannot be deleted because it is a variant attribute and at least one matrix item exists with this item class assigned. Remove all matrix items of the template item first.";
		public const string CantDeleteVariantAttributeForMatrixTemplate = "The {0} attribute cannot be deleted because it is specified as the default column or row attribute for the following templates on the Template Items (IN203000) form: {1}. Select another attribute as the default column or row attribute for all the templates first.";
		public const string CantAddVariantAttributeForMatrixItem = "The {0} variant attribute cannot be added if at least one matrix item exists with this item class assigned. Remove all matrix items of the template item first.";
		public const string AttributeIsInactive = "The {0} attribute is inactive. Specify an active attribute.";
		public const string SampleInventoryID = "Inventory ID Example: {0}";
		public const string SampleInventoryDescription = "Description Example: {0}";
		public const string INMatrixExcludedData = "Data Excluded From Update of Matrix Items";
		public const string INMatrixExcludedField = "Field Excluded From Update of Matrix Items";
		public const string INMatrixExcludedAttribute = "Attribute Excluded From Update of Matrix Items";
		public const string INMatrixExcludedFieldName = "Field Name Excluded From Update of Matrix Items";
		public const string TemplateHasDuplicatesByAttributes = "The {0} item has the same attribute values as the {1} item. {0} cannot be shown as a matrix item based on the {2} template.";
		#endregion

		#region Convert Items
		public const string ConvertToStockItem = "Convert to Stock Item";
		public const string ConvertToNonStockItem = "Convert to Non-Stock Item";
		public const string NotProcessedDocumentsSeeTrace = "The item cannot be converted because it is included in documents that are not processed completely. For details, see the trace log: Click Tools > Trace on the form title bar.";
		public const string CannotConvertKit = "The {0} item cannot be converted because it is a kit. To convert the item, remove the specification related to this item on the Kit Specifications (IN209500) form, and clear the Is a Kit check box on the General tab of the current form.";
		public const string CannotConvertMatrix = "Cannot convert the {0} item because it is a matrix item. Matrix item conversion is not supported.";
		public const string CannotConvertInventoryReceipts = "The {0} item cannot be converted because it is included in the following receipts that are not released: {1}";
		public const string CannotConvertInventoryIssues = "The {0} item cannot be converted because it is included in the following issues that are not released: {1}";
		public const string CannotConvertInventoryTrasfers = "The {0} item cannot be converted because it is included in the following transfers that are not released: {1}";
		public const string CannotConvertInventoryAdjustments = "The {0} item cannot be converted because it is included in the following adjustments that are not released: {1}";
		public const string CannotConvertInventoryKits = "The {0} item cannot be converted because it is included in the following kit assembly documents that are not released: {1}";
		public const string CannotConvertPOOrders = "The {0} item cannot be converted because it is included in the following incomplete purchase orders: {1}";
		public const string CannotConvertNotReleasedPOReceipts = "The {0} item cannot be converted because it is included in the following purchase receipts that are not released: {1}";
		public const string CannotConvertNotBilledPOReceipts = "The {0} item cannot be converted because it is included in the following unbilled purchase receipts or purchase returns: {1}";
		public const string CannotConvertLandedCosts = "The {0} item cannot be converted because it is included in the following landed cost documents that are not released: {1}";
		public const string CannotConvertAPBills = "The {0} item cannot be converted because it is included in the following AP bills that are not released: {1}";
		public const string CannotConvertAPDebitAdjustments = "The {0} item cannot be converted because it is included in the following debit adjustments that are not released: {1}";
		public const string CannotConvertAPCreditAdjustments = "The {0} item cannot be converted because it is included in the following credit adjustments that are not released: {1}";
		public const string CannotConvertAPPrepaymentRequests = "The {0} item cannot be converted because it is included in the following prepayment requests that are not released: {1}";
		public const string CannotConvertSOOrders = "The {0} item cannot be converted because it is included in the following incomplete sales orders: {1}";
		public const string CannotConvertSOShipments = "The {0} item cannot be converted because inventory has not been updated for it in the following shipments: {1}";
		public const string CannotConvertInvoices = "The {0} item cannot be converted because it is included in the following invoices that are not released: {1}";
		public const string CannotConvertCashSale = "The {0} item cannot be converted because it is included in the following cash sale documents that are not released: {1} ";
		public const string CannotConvertItemTypeIsNotNonStock = "The {0} item cannot be converted. To convert a non-stock item to a stock item, the non-stock item must have the Non-Stock Item type and the Require Receipt and Require Shipment check boxes selected.";
		public const string CannotConvertKitSpecList = "The {0} item cannot be converted because it is included as a component in the following revisions of kit specifications: {1}";
		public const string CannotConvertKitSpecListItem = "the {0} kit, the {1} specification revision";
		public const string CannotConvertKitSpecSeeTrace = "The {0} item cannot be converted because it is included as a component in at least one revision of kit specifications. For details, see the trace log: Click Tools > Trace on the form title bar.";
		public const string CannotConvertSiteStatuses = "The {0} item cannot be converted because its quantity on hand is less or more than zero in the following warehouses: {1}";
		public const string CannotConvertPIClasses = "The {0} item cannot be converted because it is selected on the Inventory Item Selection tab of the Physical Inventory Types (IN208900) form for the following inventory types: {1}";
		public const string CannotConvertPI = "The {0} item cannot be converted because it is included in an incomplete physical inventory count: {1}";
		public const string CannotConvertCashEntries = "The {0} item cannot be converted because it is included in the following cash entry documents that are not released: {1}";
		public const string CannotConvertINTransit = "The {0} item cannot be converted because it is in transit. For details, see the Goods In Transit (616500) report.";
		public const string CannotConvertDunningFeeItem = "The {0} item cannot be converted because it is specified in the Dunning Fee Item box on the Dunning tab of the Accounts Receivable Preferences (AR101000) form.";
		public const string CannotConvertTransferRequests = "The {0} item cannot be converted because it is included in at least one transfer request. To process the transfer requests, open the Create Transfer Orders (SO509000) form.";
		public const string CannotConvertPurchaseRequests = "The {0} item cannot be converted because it is included in at least one purchase request. To process the purchase requests, open the Create Purchase Orders (PO505000) form.";
		public const string CannotConvertINItemPlans = "The {0} item cannot be converted because it has the following item plans: {1}";
		public const string CannotConvertQuickChecks = "The {0} item cannot be converted because it is included in the following quick checks that are not released: {1}";
		public const string CannotConvertContracts = "The {0} item cannot be converted because it is specified in the Inventory ID box on the Unbilled tab of the Contract Usage (CT303000) form for the following contracts: {1}";
		public const string CannotConvertContractTemplates = "The {0} item cannot be converted because it is specified in the Case Count Item box on the Summary tab of the Contract Templates (CT202000) form for the following active templates: {1}";
		public const string CannotConvertContractItems = "The {0} item cannot be converted because the {0} item is specified in the Recurring Item box, Setup Item box, or Renewal Item box on the Price Options tab of the Contract Items (CT201000) form for the following contract items: {1}";
		public const string ItemHasBeenConverted = "The document with the {0} item cannot be processed because the stock status of the item has changed.";
		public const string ItemHasBeenConvertedToNonStock = "The {0} item has been converted to a non-stock item.";
		#endregion // Convert Items
	}
}
