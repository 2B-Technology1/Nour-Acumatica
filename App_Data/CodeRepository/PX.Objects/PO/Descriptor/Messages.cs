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
using PX.Data;
using PX.Common;

namespace PX.Objects.PO
{
	[PXLocalizable(Messages.Prefix)]
	public static class Messages
	{
		// Add your messages here as follows (see line below):
		// public const string YourMessage = "Your message here.";
		#region Validation and Processing Messages
		public const string Prefix = "PO Error";
		public const string DocumentOutOfBalance = "Document is out of balance.";
        public const string AssignNotSetup_Receipt = "Default Purchase Order Receipt Assignment Map is not entered in PO setup";
		public const string Document_Status_Invalid = "Document Status is invalid for processing.";
        public const string POOrderLineHasReceiptsAndCannotBeDeleted = "The line cannot be deleted because some quantity of an item in this line have been received.";
        public const string POOrderHasReceiptsAndCannotBeDeleted = "The order cannot be deleted because some quantity of items for this purchase order have been received.";
        public const string POOrderHasBillsReleasedAndCannotBeDeleted = "The order cannot be deleted because there is at least one AP bill has been released for this order. For the list of AP bills, refer to Reports > View Purchase Order Receipts History.";
        public const string POOrderLineHasBillsReleasedAndCannotBeDeleted = "The order line cannot be deleted because there is at least one AP bill has been released for this order line. For the list of AP bills, refer to Reports > View Purchase Order Receipts History.";
        public const string POOrderHasBillsGeneratedAndCannotBeDeleted = "The order cannot be deleted because one or multiple AP bills have been generated for this order. To proceed, delete AP bills first. For the list of AP bills, refer to Reports > View Purchase Order Receipts History.";
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R1)]
		public const string POOrderHasPrepaymentRequestAndCannotBeDeleted = "The order cannot be deleted because one or multiple prepayment requests have been generated for this order. To proceed, delete prepayment requests first.";
		public const string POOrderHasPrepaymentAndCannotBeDeleted = "The purchase order cannot be deleted because one or multiple prepayments have been applied to this order. To proceed, delete all prepayments first.";
		public const string POOrderLineHasBillsGeneratedAndCannotBeDeleted = "The order line cannot be deleted because one or multiple AP bills have been generated for this order line. To proceed, delete AP bills first. For the list of AP bills, refer to Reports > View Purchase Order Receipts History.";
		public const string POOrderLineHasPrepaymentRequestAndCannotBeDeleted = "The line cannot be deleted because one or multiple prepayment requests have been generated for this order. To proceed, delete prepayment requests first.";
		public const string ReceiptLineQtyGoNegative = "Receipt quantity will go negative source PO Order line";	
		public const string ReceiptLineQtyDoesNotMatchMinPOQuantityRules = "Receipt quantity is below then minimum quantity defined in PO Order for this item";
		public const string ReceiptLineQtyDoesNotMatchMaxPOQuantityRules = "Receipted quantity is above the maximum quantity defined in PO Order for this item";
		public const string LandedCostReceiptTotalCostIsZero = "This Landed Cost is allocated by Amount , but Receipt total Amount is equal to zero";
		public const string LandedCostReceiptTotalVolumeIsZero = "This Landed Cost is  allocated by Volume, but Receipt total Volume is equal to zero";
		public const string LandedCostReceiptTotalWeightIsZero = "This Landed Cost is allocated by Weight , but Receipt total Weight is equal to zero";
		public const string LandedCostReceiptTotalQuantityIsZero = "This Landed Cost is allocated by Quantity , but Receipt total Quantity is equal to zero";
		public const string LandedCostReceiptNoReceiptLines = "This Receipt does not have detail lines";
		public const string LandedCostUnknownAllocationType = "Unknown Landed Cost alloction type";
		public const string LandedCostProcessingForOneOrMorePOReceiptsFailed = "At least one of the Document selected has failed to process";
		public const string SourcePOOrderExpiresBeforeTheDateOfDocument = "Originating Blanket Order# {1} expires before the date of current Document - on {0:d}";
		public const string POBlanketOrderExpiresEarlierThanTheOrderDate = "The Expires On date is set to a date in the past.";
		public const string PostingClassIsNotDefinedForTheItemInReceiptRow = "Posting class is not defined for the Inventory Item '{0}' in PO Receipt# '{1}' line {2}";
        public const string ShippingContactMayNotBeEmpty = "The document cannot be saved, shipping contact is not specified  for the warehouse selected  on the Shipping Instructions tab.";
        public const string ShippingAddressMayNotBeEmpty = "The document cannot be saved, shipping address is not specified  for the warehouse selected  on the Shipping Instructions tab.";
        public const string LCVarianceAccountCanNotBeFoundForItemInReceiptRow = "Landed Cost Variance Account can't be found for the Inventory Item '{0}' in PO Receipt# '{1}' line {2}. Please, check the settings in the IN Post Class '{3}', Inventory Item and Warehouse '{4}'";
		public const string LCVarianceSubAccountCanNotBeFoundForItemInReceiptRow = "Landed Cost Variance Subaccount can't be found for the Inventory Item '{0}' in PO Receipt# '{1}' line {2}. Please, check the settings in the IN Post Class '{3}', Inventory Item and Warehouse '{4}'";
        public const string COGSAccountCanNotBeFoundForItemInReceiptRow = "GOGS Account can't be found for the Inventory Item '{0}' in PO Receipt# '{1}' line {2}. Please, check the settings in the IN Post Class '{3}', Inventory Item and Warehouse '{4}'";
        public const string COGSSubAccountCanNotBeFoundForItemInReceiptRow = "COGS Subaccount can't be found for the Inventory Item '{0}' in PO Receipt# '{1}' line {2}. Please, check the settings in the IN Post Class '{3}', Inventory Item and Warehouse '{4}'";
		public const string POAccrualSubAccountCanNotBeFoundForItemInReceiptRow = "PO Accrual Subaccount can't be found for the Inventory Item '{0}' in PO Receipt# '{1}' line {2}. Please, check the settings in the IN Post Class '{3}', Inventory Item and Warehouse '{4}'";
		public const string LCInventoryItemInReceiptRowIsNotFound = "Inventory Item '{0}' used in PO Receipt# '{1}' line {2} is not found in the system";
		public const string UnknownLCAllocationMethod = "Unknown Landed Cost Allocation Method for Landed Cost Code '{0}'";
		public const string POReceiptLineDoesNotReferencePOOrder = "This line does not reference PO Order";
		public const string INReceiptMustBeReleasedBeforeLCProcessing = "IN Receipt# '{0}' created from PO Receipt# '{1}' must be released before the Landed Cost may be processed";
		public const string OrderLineQtyExceedsQuantityInBlanketOrder = "The order quantity of the line exceeds the quantity in the Blanket Open Qty. column of the linked line in the {0} blanket purchase order.";
		public const string OrderLineQtyLessThanQuantityInChildOrders = "The quantity in the Order Qty. column cannot be less than the quantity in the Qty. On Orders column.";
		public const string QuantityReceivedForOrderLineExceedsOrdersQuatity = "Quantity received is already above the order's quantity for this line";
		public const string ReleaseOfOneOrMoreReceiptsHasFailed = "Release of one or more PO Receipts  has failed";
		public const string PurchaseOrderHasTypeDifferentFromOthersInPOReceipt = "Purchase Order {0} {1} cannot be added - it has a type different from other orders in this Receipt";
		public const string PurchaseOrderOnHoldWithReceipt = "The purchase order {0} {1} cannot be put on hold. It has unreleased receipt {2}.";
        public const string PurchaseOrderHasShipDestinationDifferentFromOthersInPOReceipt = "Purchase Order {0} {1} cannot be added because it has a different shipping destination than other orders in this receipt.";
		public const string SomeOrdersMayNotBeAddedTypeOrShippingDestIsDifferent = "Selected Purchase Orders cannot be added. Selected Orders must have same Currency, Type and Shipping Destinations.";
		public const string LandedCostCannotBeDistributed = "Landed costs cannot be applied to items that are transferred within one warehouse.";
		
		public const string POReceiptFromOrderCreation_NoApplicableLinesFound = "There are no lines in this document that may be entered in PO Receipt Document";
		public const string APInvoicePOOrderCreation_NoApplicableLinesFound = "There are no lines in this document that may be entered in AP Bill Document directly";
		public const string PrepaymentPOCreation_AllLinesCompleteOnZero = "The prepayment cannot be created because the value in the Complete On (%) column is equal to zero for all the lines of the purchase order on the Purchase Orders (PO301000) form. The prepayment amount cannot be calculated for such lines.";
		public const string ReceiptNumberBelongsToDocumentHavingDifferentType = "Document with number {0} already exists but it has a type {1}. Document Number must be unique for all the PO Receipts regardless of the type";
		public const string UnreasedAPDocumentExistsForPOReceipt = "There is one or more unreleased AP documents created for this document. They must be released before another AP Document may be created";
		public const string UnreleasedAPDocumentExistsForPOLines = "There is at least one unreleased AP document {0} prepared for this purchase receipt. To create new AP document, remove or release the unreleased AP document.";
	    public const string AllTheLinesOfPOReceiptAreAlreadyBilled = "AP documents are already created for all the lines of this document.";
		public const string UnitCostShouldBeNonZeroForStockItems = "Unit Cost should not be 0 for the stock items";
		public const string QuantityReducedToBlanketOpen = "Order Quantity reduced to Blanket Order: '{0}' Open Qty. for this item";
		public const string LCCodeUsesNonLCVendor = "This Code uses a vendor which has 'Landed Cost Vendor' set to 'off'. You should correct vendor or use another one";
		public const string POOrderHasUnreleaseReceiptsAndCantBeCancelled = "This Order can not be cancelled - there is one or more unreleased PO Receipts referencing it";
		public const string POOrderHasUnreleaseReceiptsAndCantBeCompleted = "The {0} purchase order cannot be completed because one or multiple unreleased purchase receipts have been generated for this order. To proceed, delete or release purchase receipts first.";
		public const string POLineHasUnreleaseReceiptsAndCantBeCompletedOrCancelled = "This line can not be completed or cancelled - there is one or more unreleased PO Receipts referencing it";
		public const string POOrderHasReleasedReceiptAndCantBeCancelled = "The {0} purchase order cannot be canceled because some of the ordered items have been received.";
		public const string POLineHasReleaseReceiptsAndCantBeCancelled = "The {0} purchase order line cannot be canceled because a part of the line items has been received.";
		public const string POSourceNotFound = "There are no lines in open purchase orders that match specified criteria.";
        public const string INSourceNotFound = "There are no unreceived lines in transfer orders that match specified criteria.";
		public const string BarCodeAddToItem = "Barcode will be added to inventory item.";
		public const string POOrderPromisedDateChangeConfirmation = "Changing of the purchase order 'Promised on' date will reset 'Promised' dates for all it's details to their default values. Continue?";
        public const string POOrderOrderDateChangeConfirmation = "Changing of the purchase order date will reset the 'Requested' and 'Promised' dates for all order lines to new values. Do you want to continue?";
		public const string POOrderPromisedDateChangedAutomatically = "The promised date has been changed from {0} to {1}.";
		public const string INDocumentFailedToReleaseDuringPOReceiptRelease = "IN Document has been created but failed to release with the following error: '{0}'. Please, validate a created IN Document";
		public const string INDocumentFailedToReleaseDuringPOReceiptReleaseInTransaction = "IN Document failed to release with the following error: '{0}'.";
        public const string APDocumentFailedToReleaseDuringPOReceiptRelease = "Release of AP document failed with the following error: '{0}'. Please validate the AP document";
		public const string APDocumentNotCreatedAllBilled = "AP document was not created because all lines have already been billed.";
        public const string POOrderQtyValidation = "Insufficient quantity requested. Line quantity was changed to match.";		
		public const string POLineQuantityMustBeGreaterThanZero = "Quantity must be greater than 0";
		public const string POLineQuantityMustBeGreaterThanZeroNonStock = "The quantity must be greater than 0 because the inventory item is closed by quantity or requires receipt. To be able to process the line with the zero quantity, clear the Require Receipt check box and select By Amount in the Close PO Line box for the inventory item on the General Settings tab of the Non-Stock Items (IN202000) form.";
		public const string BinLotSerialNotAssigned = IN.Messages.BinLotSerialNotAssigned;
		public const string ReceiptAddedForPO = "Item {0}{1} receipted {2} {3} for Purchase Order {4}";
		public const string ReceiptAdded = "Item {0}{1} receipted {2} {3}";
		public const string Availability_Info = IN.Messages.Availability_Info;
		public const string VendorIsNotLandedCostVendor = "Vendor is not landed cost vendor.";
		public const string UnsupportedLineType = "Selected line type is not allowed for current inventory item.";
		public const string POVendorInventoryDuplicate = "Unable to propagate selected item for all locations there's another one.";
		public const string UnitCostMustBeNonNegativeForStockItem = "A value for the Unit Cost must not be negative for Stock Items";
		public const string ExtCostMustBeNonNegativeForStockItem = "A value for the Ext. Cost must not be negative for Stock Items";
		public const string POOrderTotalAmountMustBeNonNegative = "'{0}' may not be negative";
        public const string LineCannotBeReceiptedTwicePO = "The item in this transfer line is already received according to the '{1}' line of the '{0}' PO receipt.";
        public const string LineCannotBeReceiptedTwiceIN = "The item in this transfer line is already received according to the '{1}' line of the '{0}' IN transaction.";
        public const string DiscountGreaterLineTotal = "Discount Total may not be greater than Line Total";
		public const string DiscountGreaterDetailTotal = "The total amount of line and document discounts cannot exceed the Detail Total amount.";
		public const string ProjectDropShipCostByIssueStrategyInvalid = "The Cost by Issue Strategy option cannot be selected because at least one line on the Details tab has been drop-shipped for a project.";
		public const string ProjectDropShipShipDestTypeInvalid = "Project Site is the only valid Shipping Destination Type for the Project Drop-Ship order.";
		public const string ProjectDropShipPostOnReceiptReleaseUpdateGLNotSet = "The selected project requires posting of drop-ship expenses on release of the purchase receipt. To be able to process the purchase order, either select the Update GL check box on the Inventory Preferences (IN101000) form or select On Bill Release in the Record Drop-Ship Expenses box for the {0} project.";
		public const string ProjectDropShipReceiptReleaseUpdateGLNotSet = "The purchase receipt cannot be released because it contains the lines that require posting of GL transactions while the Update GL check box is cleared on the Inventory Preferences (IN101000) form. To release the purchase receipt, select the Update GL check box.";

		public const string PPVAccountCanNotBeFoundForItemInReceiptRow = "Purchase Price Variance Account can't be found for the Inventory Item '{0}' in PO Receipt# '{1}' line {2}. Please, check the settings in the Reason Code '{3}'";
		public const string PPVSubAccountCanNotBeFoundForItemInReceiptRow = "Purchase Price Variance Subaccount can't be found for the Inventory Item '{0}' in PO Receipt# '{1}' line {2}. Please, check the settings in the Reason Code '{3}', Inventory Item and Warehouse '{4}'";
		public const string PPVInventoryItemInReceiptRowIsNotFound = "Inventory Item '{0}' used in PO Receipt# '{1}' line {2} is not found in the system";
		public const string ProjectTaskIsNotAssociatedWithAnyInLocation = "Given Project Task is not associated with any Warehouse Location and cannot be used with a Stock-Item. Either select another Project Task or map it to the warehouse location.";
		public const string ProjectTaskIsAssociatedWithAnotherWarehouse = "Given Project Task is associated with another Warehouse - '{0}'. Either change the warehouse or select another Project Task.";
		[Obsolete]
		public const string LocationIsNotMappedToTaskWarning = "The specified location is not linked to the project task selected in the purchase order line.";
		[Obsolete]
		public const string LocationIsNotMappedToTaskError = "The warehouse location cannot be changed because the purchase receipt line corresponds to the line of the purchase order that has been approved.";
		public const string LocationIsNotMappedToTask = "The specified warehouse location is linked to the project and project task which differ from those in the corresponding purchase order line. Select a warehouse location that is linked to the {0} project and the {1} project task.";
		public const string LocationIsMappedToOtherProject = "The items cannot be received to the warehouse location {0} because it is linked to the {1} project. To be able to receive the items for the {2} project that is selected in the purchase receipt line, select a warehouse location that is not linked to any project.";
		public const string TaskDiffersWarning = "Given Task differs from the Project Task selected on Purchase Order line.";
		public const string TaskDiffersError = "Given Task differs from the Project Task selected on Purchase Order line. Since Purchase Order document explicitly requires approval and was approved Project Task cannot be changed on the Receipt.";
		public const string TaskDiffersRSWarning = "The selected project and project task differ from those of the specified subcontract.";
		public const string TaskDiffersRSError = "The project and project task cannot be changed because the line is linked to the subcontract.";
		public const string DefaultVendorDeactivated = "Default Vendor cannot be deactivated.";
		public const string DuplicateSerialNumbers = "One or more records with duplicate serial numbers found in the document.";
		public const string ContainsDuplicateSerialNumbers = "Contains duplicate serial numbers.";
		public const string POLineLinkedToSOLine = "Deletion of the purchase order line will unlink sales order '{0}' from this purchase order. Do you want to continue?";
		public const string ChangingInventoryForLinkedRecord = "This line is linked to a sales order line. To replace the item in this line, you have to replace it in the linked sales order first.";
		public const string CustomerCannotBeChangedForOrderWithActiveDemand = "A customer to whom the drop-ship order is shipped must match the customer specified in the linked sales order.";
		public const string POLineLinkedToSO = "The line cannot be reopened because it is linked to a sales order.";
		public const string ProjectIsNotActive = "Project is not Active.";
		public const string ProjectTaskIsNotActive = "Project Task is not Active.";
		public const string LineIsInvalid = "One or more records in the document is invalid.";
		public const string FailedToAddLine = "One purchase order line or multiple purchase order lines cannot be added to the bill. See Trace Log for details.";
		public const string FailedToAddPOOrder = "Failed to add one or more purchase orders. Please check the Trace for details.";
		public const string PayToVendorHasDifferentCury = "The currency '{1}' of the pay-to vendor '{0}' differs from currency '{2}' of the document.";
		public const string VendorHasDifferentCury = "The currency '{1}' of the vendor '{0}' differs from currency '{2}' of the document.";
		public const string PossibleOverbillingAPNormalPO = "Changing this setting could lead to overbilling open service lines in the purchase orders for which you have already created AP bills. Please see the list of the affected purchase orders in the trace log.";
		public const string PossibleOverbillingAPDSPO = "Changing this setting could lead to overbilling open service lines in the drop-ship purchase orders for which you have already created AP bills. Please see the list of the affected drop-ship purchase orders in the trace log.";
		public const string PossibleOverbillingPRNormalPO = "Changing this setting could lead to overbilling open service lines in the purchase orders for which you have already created purchase receipts. Please see the list of the affected purchase orders in the trace log.";
		public const string PossibleOverbillingPRDSPO = "Changing this setting could lead to overbilling open service lines in the drop-ship purchase orders for which you have already created purchase receipts. Please see the list of the affected drop-ship purchase orders in the trace log.";
		public const string PossibleOverbillingTraceList = "List of open service lines that are partially billed or receipted (Top 1000):";
		public const string PossibleOverbillingTraceMessage = "Order Type: {0}, Order Nbr.: {1}, Line Nbr.: {2}, Inventory ID: {3}";
		public const string AutoAppliedRetainageFromOrder = "The Apply Retainage check box is selected automatically because you have added one or more lines with a retainage from the purchase order.";
		public const string LocationNotAssignedToProject = "The selected location is not assigned to the {0} project that is specified in the purchase order. Please specify another location.";
		public const string TransferUsedInUnreleasedReceipt = "The transfer order cannot be selected because it is used in the unreleased transfer receipt {0}";
		public const string CanNotDeleteWithChangeOrderBehavior = "The purchase order cannot be removed: change order workflow has been enabled for the document because it contains lines related to projects with change order workflow enabled.";
		public const string SORequestedDateEarlier = "The Requested On date in the corresponding sales order {0} is {1}.";
		[Obsolete]
		public const string ManualDiscountsWillNotBeCopiedToAP = "Manual discounts will not be transferred to the AP bill.";
		public const string SelectLinesForProcessing = "Select lines for processing.";
		public const string ReturnedQtyMoreThanReceivedQty = "The specified Returned Qty. exceeds the Received Qty. specified in the original PO receipt.";
		public const string ReturnedQtyMoreThanReceivedQtyInvt = "The Returned Qty. exceeds the Received Qty. in the {0} line of the originating PO receipt.";
		public const string UnreleasedReturnExistsForPOReceipt = "There is at least one unreleased return {0} prepared for this purchase receipt. To create new purchase return, remove or release the unreleased purchase return.";
		public const string GoodsLineWithoutOrigLinkCantReturnByOrigCost = "Return by original receipt cost cannot be processed because there is at least one line not linked to original receipt (#{0}).";
		public const string GoodsLineWithoutOrigLinkCantReturnByManualCost = "The return with the manual cost input cannot be processed because there is at least one line that is not linked to the original receipt (#{0}). Select another option or delete the lines that are not linked to the original receipt.";
		public const string OrigINReceiptMustBeReleased = "Original inventory receipt {0} must be released prior to return.";
		public const string ReceiptLineIsCompletelyReturned = "The line is already completely returned.";
		public const string AccrualAcctUsedInAPBill = "Cannot change accrual account because the line is used in the AP Document {0}, {1}.";
		public const string AccrualAcctUsedInPOReceipt = "Cannot change accrual account because the line is used in the PO Receipt {0}, {1}.";
		public const string INIssueMustReleasedPriorBilling = "IN Issue for the Purchase Return must be released prior to billing.";
		public const string SomeLinesSkippedBecauseUom = "A line from the PO receipt related to the same PO line as this line has not been added to the bill because the quantity of items is in different UOM.";
		public const string SomeLinesAggregated = "The quantity of items in this line has been summed with the quantity of items related to the same PO line from the added PO receipt or receipts.";
		public const string LineDelinkedFromBlanket = "This line is no longer linked to the {0} blanket PO because the inventory ID was changed.";
		public const string BinLotSerialEntryDisabled = "The Line Details dialog box cannot be opened because changing line details is not allowed for the selected item.";

		public const string LandedCostsDetailsEmpty = "Landed Costs document details are empty.";
		public const string LandedCostsCantReleaseWoDetails = "Cannot release landed costs without document detail lines.";
		public const string LandedCostsDetailCannotDelete = "Landed costs with a linked AP bill cannot be deleted.";
		public const string LandedCostsFailCreateAdj = "IN Document has not been created with the following error: {0}";
		public const string LandedCostsFailCreateInvoice = "AP Document has not been created with the following error: {0}";
		public const string LandedCostsFailReleaseAdj = "IN Document failed to release with the following error: {0}";
		public const string LandedCostsFailReleaseInvoice = "AP Document failed to release with the following error: {0}";
		public const string LandedCostsManyErrors = "Landed costs release has many errors. Check Trace for details.";
		public const string LandedCostsCannotReleaseWithUnreleasedReceipts = "Cannot release landed costs with unreleased receipts.";
		public const string PostingTaxToInventoryThroughLandedCostsIsNotSupported = "Cannot save the landed cost document because the tax with the {0} tax ID has the Use Tax Expense Account check box cleared and thus cannot be applied to the document. For processing taxable landed costs, configure a separate tax ID with the Use Tax Expense Account check box selected.";
		public const string LandedCostsCannotAddNonStockKit = "A landed cost document cannot be saved with a non-stock kit added.";

		public const string POLineOverbillingNotAllowed = "Cannot release the bill because the billed quantity of the related line in the {0} purchase order cannot exceed {1}. Correct the {2} line of the purchase order or remove the {3} line from the bill.";
		public const string PrepaidQtyCantExceedPOLine = "The prepaid quantity cannot exceed the quantity in the corresponding line of the purchase order.";
		public const string PrepaidAmtCantExceedPOLine = "The prepaid amount cannot exceed the amount in the corresponding line of the purchase order.";
		public const string PrepaidAmtMayExceedPOLine = "The {0} purchase order already has a partial prepayment. Make sure that the amount of the current prepayment does not exceed the purchase order amount.";
		public const string SiteWithoutDropShipLocation = "The selected warehouse has no drop-ship location. Please select another warehouse or define the drop-ship location for the currently selected warehouse.";
		public const string POHasNoItemsToReceive = "The purchase order {0} does not contain any items to be received.";
		public const string BillCannotReverseItHasNotReleasedINAdjustment = "Cannot reverse the bill because the {0} IN adjustment linked to the {1} bill is not released. Release the adjustment first to be able to reverse the bill.";
		public const string DetailSiteDiffersFromShipping = "The warehouse in the line differs from the warehouse on the Shipping Instructions tab. The items will be delivered to the {0} warehouse.";

		public const string IntercompanyPOCannotBeCancelled = "The purchase order cannot be canceled because a shipment has been prepared for the related {0} sales order.";
		public const string IntercompanyPOCannotBeDeleted = "The purchase order cannot be deleted because the {0} sales order has been generated for it.";
		public const string IntercompanySOCancelled = "The related {0} sales order has been canceled.";
		public const string IntercompanySOReturnCancelled = "The related {0} return order has been canceled.";
		public const string BranchIsNotExtendedToCustomer = "The {0} company or branch has not been extended to a customer. To create an intercompany sales order, extend the company or branch to a customer on the Companies (CS101500) or Branches (CS102000) form, respectively.";
		public const string CopyProjectDetailsToSalesOrder = "Do you want to copy the project details to the sales order?";
		public const string CopyProjectDetailsToSalesOrderNonProjectCode = "The sales order will be created with the non-project code and project details will not be copied for the purchase order lines because the lines are assigned to different projects.~Do you want to proceed?~~To copy the project details to the related sales order, create a separate purchase order for each project.";
		public const string RelatedSalesOrderDeleted = "The related sales order has been deleted.";
		public const string IntercompanySOEmptyInventory = "The lines without inventory ID cannot be copied to a sales order. The sales order has been created without these lines.";

		public const string NoLinesForVendorReturn = "There are no lines, for which a vendor return can be created.";
		public const string RMALineModifiedVendorReturnCantRelease = "The corresponding line of the RMA order has been modified, the vendor return cannot be released.";
		public const string OrigReceiptLineNotFound = "The {0} line was not found in the originating PO receipt.";

		public const string DropShipUnlinkErrorReceipt = "The {0} purchase order cannot be unlinked because one or multiple purchase receipts have been generated for it.";
		public const string DropShipUnlinkConfirmation = "The {0} purchase order has a link to the {1} sales order. Do you want to remove the link?";
		public const string DropShipUnlinkErrorNoLink = "There are no links to sales orders in this document.";
		public const string DropShipReceiptLinesNotLinked = "There are one or multiple lines in the {0} drop-ship purchase order that are not linked to a sales order. Link all lines in the {0} drop-ship purchase order first.";
		public const string DropShipReceiptSOStatus = "The purchase receipt cannot be created because the linked sales order has the {0} status.";
		public const string DropShipReceiptReleaseWrongSOStatus = "The {0} purchase receipt cannot be released because the linked {1} sales order has the {2} status.";
		public const string DropShipReceiptAddPOSOStatus = "The {0} purchase order cannot be added because the linked sales order has the {1} status.";
		public const string DropShipPOHasStatus = "The {0} line of the {1} purchase order cannot be added because the purchase order has the {2} status.";
		public const string DropShipSOLineNoLink = "The purchase order line has no active link to a line of a sales order.";
		public const string DropShipLinkedSOLineCompleted = "The linked {0} sales order line has been completed.";
		public const string DropShipPOLineCannotUncompleteLinkedSOLineCompleted = "The line cannot be reopened because the linked sales order line has been completed.";
		public const string DropShipPOLineHasActiveLink = "The line has an active link to a line of the {0} sales order. To make changes, clear the SO Linked check box.";
		public const string DropShipPOLineValidationFailed = "The {0} column value in the line does not match the {0} column value in the linked line of the {1} sales order.";
		public const string DropShipPOLineHasLinkAndCantBeDeleted = "The line cannot be deleted because there is an active link to a line of the {0} sales order. To delete the line, clear the SO Linked check box.";
		public const string DropShipPOOrderDeletionConfirmation = "The {0} purchase order has a link to a sales order. Do you want to remove the link and delete the {0} purchase order?";
		public const string DropShipPOOrderCancelationConfirmation = "The {0} purchase order has a link to a sales order. Do you want to remove the link and cancel the {0} purchase order?";
		public const string DSConvertToNormalPrepaymentExists = "The {0} prepayment request is applied to the {1} drop-ship purchase order. To convert the {1} purchase order to the Normal type, remove all applications.";
		public const string DSConvertToNormalBillExists = "The {0} AP bill is applied to the {1} drop-ship purchase order. To convert the {1} purchase order to the Normal type, remove all applications.";
		public const string DSConvertToNormalReceiptExists = "The {0} purchase receipt is applied to the {1} drop-ship purchase order. To convert the {1} purchase order to the Normal type, remove all applications.";
		public const string DSLinkedConvertToNormalAsk = "The link to the {0} sales order will be removed. The purchase order will be canceled. A new purchase order of the Normal type will be created. The value of the Shipping Destination Type box and the Vendor Tax Zone in the created purchase order will be set to the default value for a purchase order of the Normal type.";
		public const string DSNotLinkedConvertToNormalAsk = "The purchase order will be canceled. A new purchase order of the Normal type will be created. The value of the Shipping Destination Type box and the Vendor Tax Zone in the created purchase order will be set to the default value for a purchase order of the Normal type.";
		public const string DSConvertToNormalRepoen = "The {0} purchase order cannot be reopened. The {1} purchase order of the Normal type is linked to the {0} purchase order.";
		public const string DSCreateSOAlreadyLinked = "A sales order cannot be created because the {0} purchase order has already linked to the {1} sales order.";
		public const string DSLegacyCreateSOAlreadyLinked = "The sales order cannot be created because all lines of the {0} purchase order with stock or non-stock items are linked to another sales order, or the {0} purchase order does not have lines with stock or non-stock items.";
		public const string CreateSalesOrderCompleted = "The {0} sales order has been created successfully.";
		public const string CannotChangeRestricToIfPOReceiptExists = "A branch with the base currency other than {0} cannot be associated with the {1} vendor because {1} is selected in the {2} purchase receipt.";
		public const string CannotChangeRestricToIfLCExists = "A branch with the base currency other than {0} cannot be associated with the {1} vendor because {1} is selected in the {2} landed cost.";

		public const string OrigPOReceiptCannotBeFound = "The original {0} purchase receipt or some of its lines cannot be found. The return cannot be processed with the original cost. Select another option.";
		public const string ReturnByOrigCostCannotBeSelectedDifferentRates = "The Original Cost from Receipt option cannot be selected because this return contains lines from receipts with a different currency, currency rate, rate type, or effective date.";
		public const string ReturnByOrigCostAndByIssueStrategyCannotBeSelected = "The purchase return includes lines that cannot be processed with the receipt cost and lines that cannot be processed with the cost calculated according to the valuation method. To proceed, select the Manual Cost Input option.";
		public const string CannotReturnByOrigCostDropShipFIFO = "A return by original cost cannot be processed for a purchase order with the Drop-Ship type if the return order includes lines with FIFO-valuated items.";
		public const string CannotReturnByManualCostDropShipFIFO = "A return by manual cost input cannot be processed for a purchase order with the Drop-Ship type if the return order includes lines with FIFO-valuated items.";
		public const string ReceiptCannotBeAddedToReturnDifferentCurrencies = "The {0} purchase receipt cannot be added to this return because it has a different currency.";
		public const string ReceiptCannotBeAddedToReturnByOrigCostDifferentRates = "The Original Cost from Receipt option is selected. The {0} purchase receipt cannot be added to this return because it has a different currency, currency rate, rate type, or effective date.";

		public const string IntercompanyReceivedNotIssued = "The {0} inventory receipt has been created but it could not be released because at least one item with a serial number has not yet been issued from a warehouse of the selling company. Wait until the selling company issues the item, and then release the inventory receipt on the Receipts (IN301000) form.";
		public const string AdjustmentHasNotBeenReleased = "The {0} adjustment has not been released. To release the adjustment, use the Release IN Documents or Adjustments form.";
		public const string SubcontractIncludesDifferentProjects = "The subcontract includes lines related to different projects.";

		public const string ActionNotAvailableInCurrentState = SO.Messages.ActionNotAvailableInCurrentState;

		public const string CannotReturnConvertedItem = "The {0} item cannot be returned with a link to the purchase receipt that had been processed before the stock status of the item has changed. To return the item, add a new line with this item to the purchase return.";
		public const string CannotCreateLandedCostConvertedNonStock = "The landed cost amount cannot be allocated for the {0} item because the stock status of the item has changed. To add the landed cost amount to the expense account of the item, create a transaction on the Journal Transactions (GL301000) form.";
		public const string CannotCreateLandedCostConvertedStock = "The landed cost amount cannot be allocated for the {0} item because the stock status of the item has changed. To add the landed cost amount to the inventory account of the item, create an inventory adjustment on the Adjustments (IN303000) form.";
		public const string CanApplyDirectTaxOnlyToAlocationMethodNone = "The {0} tax category containing the {1} direct-entry tax can be selected only for a landed cost code with the None allocation method.";
		public const string ImportTaxCanBeAppliedOnlyToLCNonAlocationMethod = "The {0} direct-entry tax can be applied only to a landed cost line with the None allocation method.";

		public const string CannotCreatePOOrderDueToVendorCurrency = "The vendor with the currency that differs from the currency of the sales order is selected and it is not possible to override the currency in purchase orders. The purchase order for the special-order item cannot be created.";
		public const string CreatedPOOrderWillHaveSOOrderCurrency = "The vendor with the currency that differs from the currency of the sales order is selected. A purchase order with the currency of the sales order will be created.";
		public const string TheUnitCostInLinkedSOOrderWillBeChanged = "The unit cost in the linked line of the sales order will be changed from {0} to {1} when you save the purchase order. To view the affected sales orders, click View SO Demand on the table toolbar.";
		public const string TheUnitCostCannotBeChangedDueToCompletedSOLine = "The unit cost cannot be changed because the corresponding line in the {0} {1} sales order has been completed.";
		public const string TheUnitCostCannotBeChangedDueToPOLines = "The unit cost cannot be changed because the line is linked to a special-order line of a sales order, and this SO line is linked to multiple PO lines.";
		public const string CannotChangeFieldOnSpecialOrder = "The line has been linked to the line of the {0} {1} sales order. To make changes in this column, unlink the purchase order line on the Sales Orders (SO301000) form.";
		public const string SpecialLineExistsCancelOrder = "At least one purchase order line with a special-order item is linked to a line of a sales order. Do you want to cancel the purchase order?";
		public const string SpecialLineExistsCancelLine = "The line with a special-order item is linked to a sales order line. Do you want to cancel this line?";
		public const string SpecialLineExistsDeleteLine = "The line with a special-order item is linked to a sales order line. Do you want to delete this line?";
		public const string SpecialLineCompleted = "The corresponding line in the linked {0} {1} sales order has been completed.";
		public const string SpecialLineDeleted = "The linked sales order line has been deleted.";

		public const string CannotFindINIssueForDropship = "The inventory issue related to the {0} purchase receipt was not found. Prepare and release a sales invoice for the {1} sales order with the {2} type first.";

		#endregion

		#region Translatable Strings used in the code

		public const string Warning = "Warning";
		public const string PurchaseOrder = "Purchase Order";
		public const string Assign = CR.Messages.Assign;
		public const string Release = "Release";
		public const string AddPOOrder = "Add PO";
		public const string AddTransfer = "Add Transfer";
        public const string AddTransferLine = "Add Transfer Line";
		public const string AddBlanketOrder = "Add Blanket PO";
		public const string ViewPOOrder = "View PO";
		public const string ViewParentOrder = "View Parent Order";
		public const string AddPOOrderLine = "Add PO Line";
		public const string AddBlanketLine = "Add Blanket PO Line";
		public const string AddPOReceipt = "Add PO Receipt";
		public const string AddPOReceiptLine = "Add PO Receipt Line";
		public const string AddLine = "Add Line";
		public const string AddReceipt = "Add PR";
		public const string AddReceiptLine = "Add PR Line";
		public const string ViewAPDocument = "View AP Document";
		public const string ViewPODocument = "View PO Document";
		public const string ViewDemand = "View SO Demand";
		public const string ReportPOReceipAllocated = "Purchase Receipt Allocated and Backordered";
		public const string Process = "Process";
		public const string ProcessAll = "Process All";
		public const string Document = "Document";
		public const string CreatePOReceipt = "Enter PO Receipt";
		public const string CreateAPInvoice= "Enter AP Bill";
		public const string CreateLandedCostDocument= "Enter Landed Costs";
		public const string CreateReturn = "Return";
		public const string POReceiptRedirection = "Switch to PO Receipt";
		public const string EmptyValuesFromExternalTaxProvider = AP.Messages.EmptyValuesFromExternalTaxProvider;
		public const string ExternalTaxVendorNotFound = TX.Messages.ExternalTaxVendorNotFound;
		public const string FailedGetFromAddressSO = SO.Messages.FailedGetFromAddressSO;
		public const string FailedGetToAddressSO = SO.Messages.FailedGetToAddressSO;
		public const string POTypeNbrMustBeFilled = "PO type and PO number must be filled in for the system to add the purchase order to the details.";
		public const string PurchaseOrderLineNotFound = "The purchase order line was not found.";
		public const string PurchaseReceiptLineNotFound = "The purchase receipt line was not found.";
		public const string PurchaseOrderNotFound = "The {0} {1} purchase order was not found.";
		public const string TransferOrderNotFound = "The transfer order was not found.";
		public const string TransferOrderLineNotFound = "The transfer order line was not found.";
		public const string Availability_POOrder = "On Hand {1} {0}, Available {2} {0}, Available for Shipping {3} {0}, Purchase Orders {4} {0}";

		public const string TransferOrderType = "Transfer Order Type";
		public const string TransferOrderNbr = "Transfer Order Nbr.";
		public const string TransferLineNbr = "Transfer Line Nbr.";
		public const string SOReturnType = "SO Return Type";
		public const string SOReturn = "SO Return";
		public const string SOReturnLine = "SO Return Line";

		#endregion

		#region Graph and Cache Names
		public const string POSetupMaint = "Purchasing Preferences";

		public const string POOrder = "Purchase Order";
		public const string POReceipt = "Purchase Receipt";
		public const string POReceiptLine = "Purchase Receipt Line";		
		public const string POLine = "Purchase Order Line";
		public const string PORemitAddress = "Remit Address";
		public const string PORemitContact = "Remit Contact";
		public const string POShipAddress = "Ship Address";
		public const string POShipContact = "Ship Contact";
        public const string VendorContact = "Vendor Contact";
        public const string Approval = "Approval";
		public const string LandedCostCode = "Landed Cost Code";
		public const string VendorLocation = "Vendor Location";
        public const string InventoryItemVendorDetails = "Inventory Item Vendor Details";
        public const string PORemitAddressFull = "PO Remittance Address";
        public const string POShipAddressFull = "PO Shipping Address";
        public const string POAddress = "PO Address";
        public const string PORemitContactFull = "PO Remittance Contact";
        public const string POShipContactFull = "PO Shipping Contact";
        public const string POContact = "PO Contact";
        public const string POLineS = "PO Line to Add";
        public const string POLineShort = "PO Line";
        public const string POLineBillingRevision = "PO Line Billing Revision";
		public const string POAccrualStatus = "PO Accrual Status";
		public const string POAccrualDetail = "PO Accrual Detail";
		public const string POAccrualSplit = "PO Accrual Allocation";
		public const string POAccrualBalanceInquiryFilter = "Purchase Accrual Balance Filter";
		public const string POAccrualBalanceInquiryResult = "Purchase Accrual Balance Result";
		public const string POOrderPrepayment = "PO Prepayment";
		public const string POOrderPOReceipt = "Purchase Order to Purchase Receipt Link";
		public const string POReceiptPOOrder = "Purchase Receipt to Purchase Order Link";
		public const string POReceiptPOReturn = "Purchase Receipt to Purchase Return Link";
		public const string POOrderAPDoc = "Purchase Order to Accounts Payable Document Link";
		public const string SOForPurchaseReceiptFilter = "Intercompany Sales Orders Filter";
		public const string DropShipPOLine = "PO Drop-Ship Line";

		public const string POOrderDiscountDetail = "Purchase Order Discount Detail";
		public const string POSetupApproval = "PO Approval";
		public const string POReceiptLineSplit = "Purchase Receipt Line Split";
		public const string POTaxTran = "PO Tax";
		public const string POTax = "PO Tax Detail";
		public const string POReceiptAPDoc = "Purchase Receipt Billing History";

		public const string POLandedCostDoc = "Landed Costs Document";
		public const string POLandedCostDetail = "Landed Costs Detail";
		public const string POLandedCostReceipt = "Landed Costs Receipt";
		public const string POLandedCostReceiptLine = "Landed Costs Receipt Line";
		public const string POLandedCostTaxTran = "Landed Costs Tax";
		public const string POLandedCostTax = "Landed Costs Tax Detail";

		public const string POReceiptSplitToTransferSplitLink = "Receipt Line Split To Transfer Line Split Link";
		public const string POReceiptSplitToCartSplitLink = "Receipt Line Split To Cart Split Link";
		public const string POCartReceipt = "Receipt Cart";
		public const string POReceivePutAwaySetup = "Receive Put Away Setup";
		public const string POReceivePutAwayUserSetup = "Receive Put Away User Setup";
		#endregion

		#region Order Type
		public const string Blanket = "Blanket";
		public const string StandardBlanket = "Standard";
		public const string RegularOrder = "Normal";
		public const string RegularOrder_NPO = "Normal Purchase Order";
		public const string DropShip = "Drop-Ship";
		public const string ProjectDropShip = "Project Drop-Ship";
		#endregion

		#region Receipt Type
		public const string PurchaseReceipt = "Receipt";
		public const string PurchaseReturn = "Return";
		public const string TransferReceipt = "Transfer Receipt";
		#endregion

		public const string POBehavior_Standard = "Standard";
		public const string POBehavior_ChangeOrder = "Change Order";

		#region Shipping Destination
        public const string ShipDestCompanyLocation = "Branch";
        public const string ShipDestCustomer = "Customer";
		public const string ShipDestVendor = "Vendor";
		public const string ShipDestSite = "Warehouse";
		public const string ShipDestProjectSite = "Project Site";
		#endregion

		#region Default Receipt Quantity
		public const string OpenQuantity = "Open Quantity";
        public const string ZeroQuantity = "Zero";
        #endregion

		#region Order Line Type
		public const string GoodsForInventory = "Goods for IN";
		public const string GoodsForSalesOrder = "Goods for SO";
        public const string GoodsForServiceOrder = "Goods for FS";
        public const string GoodsForReplenishment = "Goods for RP";
		public const string GoodsForDropShip = "Goods for Drop-Ship";
		public const string NonStockForDropShip = "Non-Stock for Drop-Ship";
		public const string NonStockForSalesOrder = "Non-Stock for SO";
        public const string NonStockForServiceOrder = "Non-Stock for FS";
		public const string GoodsForProject = "Goods for Project";
		public const string NonStockForProject = "Non-Stock for Project";
		public const string NonStockItem = "Non-Stock";
		public const string Service = "Service";
		public const string Freight = "Freight";
		public const string Description = "Description";

		#endregion

		#region Report Order Type
		public const string PrintBlanket = "Blanket";
		public const string PrintStandardBlanket = "Stadard";
		public const string PrintRegularOrder = "Normal";
		public const string PrintDropShip = "Drop Ship";
		#endregion

		#region Order Status
		public const string Hold = "On Hold";
		public const string Open = "Open";
		public const string AwaitingLink = "Awaiting Link";
		public const string Completed = "Completed";
		public const string Closed = "Closed";
		public const string Cancelled = "Canceled";
		public const string PendingPrint = "Pending Printing";
		public const string PendingEmail = "Pending Email";
		public const string Printed = "Printed";
		public const string Approved = "Approved";
		public const string Rejected = "Rejected";
		#endregion

		#region Receipt Status
		public const string Balanced = "Balanced";
		public const string Released = "Released";
		#endregion

		#region PO Receipt Quantity Action
		public const string Accept = "Accept";
		public const string AcceptButWarn = "Accept but Warn";
		public const string Reject = "Reject";
		#endregion

		#region PO Vendor Catalogue Actions
        public const string ShowVendorPrices = "Vendor Prices";
		#endregion

		#region LandedCostAllocationMethods
		public const string ByQuantity = "By Quantity";
		public const string ByCost = "By Cost";
		public const string ByWeight = "By Weight";
		public const string ByVolume = "By Volume";
		public const string None = "None";

		#endregion

		#region AP Invoice PO Validation
		public const string APInvoicePOUnbilledAmountValidationWarning = "Related purchase order line:~Unbilled Amount: {4} {1}.";
		public const string APInvoicePOValidationWarning = "Related purchase order line:~Unbilled Quantity: {2} {0}, Unit Cost: {3} {1} per {0}, Unbilled Amount: {4} {1}.";
		public const string APInvoiceRSValidationWarning = "Related subcontract line:~Unbilled Quantity: {2} {0}, Unit Cost: {3} {1} per {0}, Unbilled Amount: {4} {1}.";
		public const string APInvoicePOValidationWarningWithReceivedQty = "Related purchase order line:~Unbilled Quantity: {2} {0}, Unit Cost: {3} {1} per {0}, Unbilled Amount: {4} {1}.~Received quantity exceeds the ordered quantity.";
		public const string APInvoiceRSValidationWarningWithReceivedQty = "Related subcontract line:~Unbilled Quantity: {2} {0}, Unit Cost: {3} {1} per {0}, Unbilled Amount: {4} {1}.~Received quantity exceeds the ordered quantity.";
		public const string QuantityBilledIsGreaterThenPOReceiptQuantity = "Quantity billed is greater than the quantity in the original PO Receipt for this row";
		public const string QuantityBilledIsGreaterThenPOQuantity = "The billed quantity is greater than the quantity in the original purchase order for this row.";
		public const string BillLinesDifferFromPOLines = "Some of the bill lines differ from the corresponding lines of the related purchase order. For details, review warnings in the lines.";
		public const string BillLinesDifferFromRSLines = "Some of the bill lines differ from the corresponding lines of the related subcontract. For details, review warnings in the lines.";
		#endregion

		#region LandedCostType
		public const string FreightOriginCharges = "Freight & Misc. Origin Charges";
		public const string CustomDuties = "Customs Duties";
		public const string VATTaxes = "VAT Taxes";
		public const string MiscDestinationCharges = "Misc. Destination Charges";
		public const string Other = "Other";
		#endregion

		#region Landed Costs Action
		public const string EnterAPBill = "Enter AP Bill";
		public const string AddLandedCosts = "Add Landed Costs";
		public const string Actions = "Actions";
		#endregion

		#region POAccrualType

		public const string Receipt = "Receipt";
		public const string Order = "Order";

		#endregion

		#region PPV Allocation Mode
		public const string PPVInventory = "Inventory Account";
		public const string PPVAccount = "Purchase Price Variance Account";
		#endregion

		#region Return Cost Mode
		public const string OriginalReceiptCost = "Original Cost from Receipt";
		public const string CostByIssueStategy = "Cost by Issue Strategy";
		public const string ManualCost = "Manual Cost Input";
		#endregion

		#region AP Invoice Validation Mode
		public const string APInvoiceNoValidation = "No Validation";
		public const string APInvoiceValidateWithWarning = "Validate with Warning";
		#endregion

		#region Vendor changed - validation messages
		public const string VendorChangedOnOrderWithRestrictedCurrency = "The selected vendor does not work with the {0} currency specified in the purchase order. Select another vendor.";
		public const string VendorChangedOnOrderWithRestrictedRateType = "The selected vendor does not work with the {0} currency rate type specified in the purchase order. Select another vendor.";
		public const string VendorChangedOnOrderWithPurchaseReceipt = "You cannot change a vendor for this purchase order because it has purchase receipts linked.";
		public const string VendorChangedOnOrderWithAPDocument = "You cannot change a vendor for this purchase order because it has AP documents linked.";
		public const string VendorChangedOnOrderWithLinesFromBlanketOrder = "You cannot change a vendor for this purchase order because one or more lines were created from a blanked purchase order.";
		public const string VendorChangedOnOrderLinkedWithSO = "You cannot change a vendor for this purchase order because it is linked with a sales order line that is split between multiple purchase orders.";
		#endregion
		
		#region Headings
		public const string InventoryItemDescr = "Item Description";
		public const string SiteDescr = "Warehouse Description";
		public const string VendorAcctName = "Vendor Name";
		public const string CustomerAcctName = "Customer Name";
		public const string CustomerLocationID = "Customer Location";
		public const string PlanTypeDescr = "Plan Type";
		public const string CustomerPrice = "Customer Price";
		public const string CustomerPriceUOM = "Customer UOM";
		public const string POLineOrderNbr = "PO Nbr.";
		#endregion

		public const string PurchaseOrderCreated = "Purchase Order '{0}' created.";
		public const string MissingVendorOrLocation = "Vendor and vendor location should be defined.";
		public const string SameSOExists = "A sales order with this number already exists. Enter another number.";
		public const string SalesOrderRelatedToDropShipReceiptIsNotApproved = "The {0} {1} sales order related to this drop-ship receipt is not approved. Contact {2} for details.";
		public const string SalesOrderLineHasAlreadyBeenCompleted = "In the {0} {1} sales order related to this purchase receipt, the line with the {2} item has already been completed.";
		public const string SalesOrderLineHasAlreadyBeenCompletedContactForDetails = "In the {0} {1} sales order related to this purchase receipt, the line with the {2} item has already been completed. Contact {3} for details.";
		public const string StatusTotalBilled = "Total Billed Qty. {0}, Total Billed Amt. {1}";
		public const string StatusTotalBilledTotalPPV = "Total Billed Qty. {0}, Total Billed Amt. {1}, Total PPV Amt. {2}";
		public const string StatusTotalBilledTotalAccrued = "Total Billed Qty. {0}, Total Billed Amt. {1}, Total Accrued Qty. {2}, Total Accrued Amt. {3}";
		public const string StatusTotalBilledTotalAccruedTotalPPV = "Total Billed Qty. {0}, Total Billed Amt. {1}, Total Accrued Qty. {2}, Total Accrued Amt. {3}, Total PPV Amt. {4}";
		public const string StatusTotalReceived = "Total Received Qty. {0}";
		public const string StatusTotalPrepayments = "Total Applied to Order Amount {0}, Total Balance Amount {1}";
		public const string POAdjust = "Purchase Order Adjust";
		public const string LoadOrders = "Load Orders";
		public const string CanNotRemoveReferenceToReleasedPOOrder = "You cannot remove the reference to the {0} purchase order because it is released.";
		public const string CanNotRemoveReferenceToPOOrderRelatedPrepayment = "You cannot remove the reference to the {0} purchase order because it is related to the {1} prepayment request.";
		public const string PrepaidTotalGreaterUnbilledOrderTotal = "The value of the Unbilled Prepayment Total box of the {0} purchase order cannot exceed its unbilled amount.";
		public const string PrepaidTotalGreaterUnbilledOrderTotalOrOrderTotal = "The value of the Unbilled Prepayment Total box of the {0} purchase order cannot exceed its unbilled or total amount.";
		public const string TotalPrepaymentAmountGreaterDocumentAmount = "The value of the PO Applied Amount box cannot be greater than the value of the Payment Amount box.";
		public const string PrepayPOLineMoreThanAllowed = "Cannot release the prepayment because the sum of amounts in the released prepayments and in the current prepayment for the purchase order line ({0}, {1}, {2}) exceeds the order amount {3}.";
		public const string POOrderDocTypeOnPrepaymentCheck = "PO";

		public const string All = "All";
	}
}
