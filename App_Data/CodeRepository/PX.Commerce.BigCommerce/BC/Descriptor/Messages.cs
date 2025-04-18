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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Common;

namespace PX.Commerce.BigCommerce
{
	[PXLocalizable()]
	public static class BigCommerceMessages
	{
		public const string CategoryNotSyncronizedForItem = "The item sales category is not synchronized for the {0} item. Please synchronize the Item Sales Category entity.";
		public const string CannotExportOrder = "The {0} order could not be exported to BigCommerce because at least one refund has been imported for it.";
		public const string CustomerNotSyncronized = "The {0} customer has not been synchronized. Please synchronize the Customer entity.";
		public const string CustomerPriceClassNotSyncronized = "Cannot find a mapped customer group in BigCommerce for this customer price class.";
		public const string InternalServerError = "BigCommerce could not process the request at this time due to an internal server error. Please try again later.";
		public const string InvalidImage = "The image URL is not accepted by BigCommerce. Please check the URL in the Media URLs table.";
		public const string InvalidVideo = "The video URL is not accepted by BigCommerce. Please check the URL in the Media URLs table.";
		public const string OptionsNotMatched = "The synchronization could not be completed because variant options do not match the {0} product in BigCommerce.";
		public const string OptionValuesNotMatched = "The synchronization could not be completed because variant values do not match the {0} product in BigCommerce.";
		public const string MappedToOtherEntity = "The {0} item could not be exported to BigCommerce because there is a BigCommerce product with the matching name that has been mapped to another item with the same description. Please change the item description.";
		public const string NoBranch = "The BigCommerce integration requires the default branch to be configured. Please configure the default branch on the BigCommerce Stores (BC201000) form.";
		public const string NoCustomerClass = "The BigCommerce integration requires the default customer class to be configured. Please configure the default customer class on the BigCommerce Stores (BC201000) form.";
		public const string NoCustomerForAddress = "No customer is found for the following address: {0}. Please synchronize the Customer entity first.";
		public const string NoCustomerNumbering = "The BigCommerce integration requires autonumbering of customers. Please configure a numbering sequence in the store settings on the BigCommerce Stores (BC201000) form or on the Segmented Keys (CS202000) form.";
		public const string NoDefaultAddress = "Customer {0} does not have the default address.";
		public const string NoDefaultCashAccount = "The payment cannot be saved because the BigCommerce integration requires the default cash account for the {0} payment method. Please specify the default cash account on the Payment Methods (CA204000) form.";
		public const string NoGiftCertificateItem = "The gift certificate item could not be found. Please specify it on the Order Settings tab of the BigCommerce Stores (BC201000) form.";
		public const string NoGiftWrapItem = "The gift wrapping item could not be found. Configure the mapping of gift wrapping options on the Order Settings tab of the BigCommerce Stores (BC201000) form.";
		public const string NoLocationNumbering = "The BigCommerce integration requires autonumbering of customer locations. Please configure a numbering sequence in the store settings on the BigCommerce Stores (BC201000) form or on the Segmented Keys (CS202000) form.";
		public const string NoNonStockItemClass = "The BigCommerce integration requires the default non-stock item class to be configured. Please configure the default non-stock item class on the BigCommerce Stores (BC201000) form.";
		public const string NoNonStockNumbering = "The BigCommerce integration requires autonumbering of non-stock items. Please configure a numbering sequence in the store settings on the BigCommerce Stores (BC201000) form or on the Segmented Keys (CS202000) form.";
		public const string NoOrderDetails = "The BigCommerce integration requires document details to be added to orders. Please add order lines to the {0} order.";
		public const string NoRefundItem = "The refund amount item could not be found. Please specify it on the Order Settings tab of the BigCommerce Stores (BC201000) form.";
		public const string NoRequiredField = "{0} is a required field in BigCommerce. Please provide {0} in the {1} record.";
		public const string NoReturnOrderType = "The BigCommerce integration requires the default return order type to be configured. Please configure the default return order type on the BigCommerce Stores (BC201000) form.";
		public const string NoStockItemClass = "The BigCommerce integration requires the default stock item class to be configured. Please configure the default stock item class on the BigCommerce Stores (BC201000) form.";
		public const string NoStockNumbering = "The BigCommerce integration requires autonumbering of stock items. Please configure a numbering sequence in the store settings on the BigCommerce Stores (BC201000) form or on the Segmented Keys (CS202000) form.";
		public const string NoSalesOrderType = "The BigCommerce integration requires an active sales order type to be specified as the default. Please configure the default sales order type on the BigCommerce Stores (BC201000) form.";
		public const string NoSubstituteValues = "The BigCommerce integration requires substitute values for the {0} mapping. Please provide substitute values on the Substitute Lists (BC105000) form.";
		public const string NoGuestCustomer = "The customer record to be used for synchronization cannot be found. Make sure that the customer exists and the guest customer account is specified in the store settings on the BigCommerce Stores (BC201000) form.";
		public const string OrderDescription = "{0} | Order: {1} | Status: {2}";
		public const string PaymentDescription = "{0} | Payment Method: {1} | Order: {2} | Payment ID: {3}";
		public const string PaymentDescriptionGC = "{0} | Payment Method: {1} ({4}) | Order: {2} | Payment ID: {3}";
		public const string PaymentRefundDescription = "{0} | Order: {1} | Refund: {2} | Gateway: {3}";
		public const string ProcessingRealtimeStartSuccess = "Webhook synchronization for {0} has been successfully started."; // 0 - entityName
		public const string ProcessingRealtimeStopSuccess = "Webhook synchronization for {0} has been successfully stopped."; // 0 - entityName
		public const string ReasonCodeRequired = "A refund reason code is required for processing this record. Please provide the refund reason code on the Order Settings tab of the BigCommerce Stores (BC201000) form.";
		public const string ShipTermModeNotMatch = "In the {0} shipping terms with the freight price based on shipment, the shipping price must be zero. Please change the shipping price to zero or update the mapping of terms on the Order Settings tab of the BigCommerce Stores (BC201000) form.";
		public const string TestConnectionFolderNotFound = "Cannot retrieve the files found through BigCommerce WebDAV protocol. Please check if the WebDAV URL is correct.";
		public const string TestConnectionStoreNotFound = "Cannot retrieve the store through BigCommerce REST API. Please check if the store URL is correct.";
		public const string VariantsNotMatched = "The synchronization could not be completed because variant items do not match the {0} product in BigCommerce.";
				
		public const string BasePriceNotSyncedNoValidInventoryItemNotSynced = "The {0} item must be synchronized before synchronizing its base price.";
		public const string BasePriceEmptySynced = "The item does not have any base sales prices that can be exported to the BigCommerce store. The current sale price in the BigCommerce store has been deleted. Check the item's sales prices of the Base type on the Sales Prices (AR202000) form, and try again.";
		public const string BasePriceEmptyNotSynced = "The item does not have any base sales prices that can be exported to the BigCommerce store. Check the item's sales prices of the Base type on the Sales Prices (AR202000) form, and try again.";

		public const string PriceListUnexpectedErrorDuplicates = "An error occurred during the processing of the price list. Multiple prices have been found for the same SKU, currency, break quantity, and price.";
		public const string PriceListUnexpectedInvalidInternalKey = "An error occurred during the processing of the price list. Invalid internal key.";

		public const string PriceListProductsNotInBC = "The {0} price list could not be exported because at least one item in it does not exist in the BigCommerce store. Synchronize all the items in the price list, and then process the price list again.";
		public const string PriceListCustomerGroupNotFound = "The {0} price list could not be synchronized because the {0} customer group no longer exists in the BigCommerce store.";

		public const string ErrorDuringImageDeletionExceptionMessage = "An error occurred while deleting an image in BigCommerce. {0}";
		
	}

	[PXLocalizable()]
	public static class BigCommerceCaptions
	{
		//Discount Code
		public const string Percentage = "Percentage";
		public const string Item = "Item";
		public const string Shipping = "Shipping";
		public const string Total = "Total";
		public const string FreeShipping = "Free Shipping";
		public const string Promotion = "Promotion";

		//Actions
		public const string Import = "Import";
		public const string Export = "Export";
		public const string Update = "Update";
		public const string Delete = "Delete";

		//BigCommerce order statuses
		public const string Pending = "Pending";
		public const string AwaitingPayment = "Awaiting Payment";
		public const string AwaitingFulfillment = "Awaiting Fulfillment";
		public const string AwaitingShipment = "Awaiting Shipment";
		public const string AwaitingPickup = "Awaiting Pickup";
		public const string PartiallyShipped = "Partially Shipped";
		public const string Completed = "Completed";
		public const string Shipped = "Shipped";
		public const string Cancelled = "Canceled";
		public const string Declined = "Declined";
		public const string Refunded = "Refunded";
		public const string Disputed = "Disputed";
		public const string VerificationRequired = "Manual Verification Required";
		public const string PartiallyRefunded = "Partially Refunded";
		public const string Incomplete = "Incomplete";

		//BigCommerce API Object Descriptions
		public const string OrderProducts  = "Order Products";
		public const string AddressLine1 = "Address Line 1";
		public const string AddressLine2 = "Address Line 2";
		public const string Amount = "Amount";
		public const string AvailabilityDescription = "Availability Description";
		public const string AvsResult = "AVS Result";
		public const string Availability = "Availability";
		public const string BaseCost = "Base Cost";
		public const string BaseCostPrice = "Base Cost Price";
		public const string BaseHandlingCost = "Base Handling Cost";
		public const string BasePrice = "Base Price";
		public const string BaseShippingCost = "Base Shipping Cost";
		public const string BaseTotal = "Base Total";
		public const string BaseWrappingCost = "Base Wrapping Cost";
		public const string BillingAddress = "Billing Address";
		public const string BinPickingNumber = "Bin Picking Number";
		public const string BrandId = "Brand ID";
		public const string CardExpiryMonth = "Card Expiry Month";
		public const string CardExpiryYear = "Card Expiry Year";
		public const string CardIin = "Card IIN";
		public const string CardLast4 = "Card Last4";
		public const string CardType = "Card Type";
		public const string Categories = "Categories";
		public const string CategoryAccess = "Category Access";
		public const string CategoryDescription = "Category Description";
		public const string CategoryName = "Category Name";
		public const string City = "City";
		public const string Code = "Code";
		public const string CompanyName = "Company Name";
		public const string Condition = "Condition";
		public const string CostExcludingTax = "Cost Excluding Tax";
		public const string CostIncludingTax = "Cost Including Tax";
		public const string CostPrice = "Cost Price";
		public const string CostPriceExcludingTax = "Cost Price Excluding Tax";
		public const string CostPriceIncludingTax = "Cost Price Including Tax";
		public const string CostPriceTax = "Cost Price Tax";
		public const string CostTax = "Cost Tax";
		public const string CostTaxClassId = "Cost Tax Class ID";
		public const string Country = "Country";
		public const string CountryIso2 = "Country ISO2";
		public const string AddressType = "Address Type";
		public const string CountryISOCode = "Country ISO Code";
		public const string CouponAmount = "Coupon Amount";
		public const string CouponCode = "Coupon Code";
		public const string CouponDiscount = "Coupon Discount";
		public const string CreditCard = "Credit Card";
		public const string Currency = "Currency";
		public const string CurrencyCode = "Currency Code";
		public const string CurrencyExchangeRate = "Currency Exchange Rate";
		public const string Customer = "Customer";
		public const string CustomerAddress = "Customer Address";
		public const string CustomerAddressData = "Customer Address Data";
		public const string CustomerGroup = "Customer Group";
		public const string CustomerGroupId = "Customer Group ID";
		public const string CustomerId = "Customer ID";
		public const string CustomerMessage = "Customer Message";
		public const string CustomFields = "Custom Fields";
		public const string CustomPayment = "Custom Payment";
		public const string CustomUrl = "Custom URL";
		public const string CvvResult = "CVV Result";
		public const string DateCreatedUT = "Date Created UT";
		public const string DateModifiedUT = "Date Modified UT";
		public const string DateShipped = "Date Shipped";
		public const string DefaultProductSort = "Default Product Sort";
		public const string Depth = "Depth";
		public const string Discount = "Discount";
		public const string DiscountAmount = "Discount Amount";
		public const string DiscountRule = "Discount Rule";
		public const string DisplayName = "Display Name";
		public const string DisplayStyle = "Display Style";
		public const string DisplayValue = "Display Value";
		public const string Email = "Email";
		public const string EmailAddress = "Email Address";
		public const string EventName = "Event Name";
		public const string FieldType = "Field Type";
		public const string FieldValue = "Field Value";
		public const string FirstName = "First Name";
		public const string FixedCostShippingPrice = "Fixed Cost Shipping Price";
		public const string FixedShippingCost = "Fixed Shipping Cost";
		public const string FormFields = "Form Fields";
		public const string Gateway = "Gateway";
		public const string GatewayTranscationId = "Gateway Transaction ID";
		public const string GiftCertificate = "Gift Certificate";
		public const string GiftCertificateAmount = "Gift Certificate Amount";
		public const string GiftCertificateStatus = "Gift Certificate Status";
		public const string GiftWrappingOptionsType = "Gift Wrapping Options Type";
		public const string GlobalTradeNumber = "Global Trade Number";
		public const string HandlingCostExcludingTax = "Handling Cost Excluding Tax";
		public const string HandlingCostIncludingTax = "Handling Cost Including Tax";
		public const string HandlingCostTax = "Handling Cost Tax";
		public const string HandlingCostTaxClassId = "Handling Cost Tax Class ID";
		public const string Height = "Height";
		public const string ID = "ID";
		public const string ImageUrl = "Image URL";
		public const string InventoryLevel = "Inventory Level";
		public const string InventoryWarningLevel = "Inventory Warning Level";
		public const string InventoryTracking = "Inventory Tracking";
		public const string IsBundledProduct = "Is Bundled Product";
		public const string IsConditionShown = "Is Condition Shown";
		public const string IsCustomized = "Is Customized";
		public const string IsDefault = "Is Default";
		public const string IsFeatured = "Is Featured";
		public const string IsFreeShipping = "Is Free Shipping";
		public const string IsRefunded = "Is Refunded";
		public const string IsVisible = "Is Visible";
		public const string ItemShipped = "Items Shipped";
		public const string ItemsTotal = "Items Total";
		public const string LastName = "Last Name";
		public const string LayoutFile = "Layout File";
		public const string ManufacturerPartNumber = "Manufacturer Part Number";
		public const string MaximumOrderQuantityn = "Maximum Order Quantity";
		public const string Message = "Message";
		public const string MetaDescription = "Meta Description";
		public const string MetaKeywords = "Meta Keywords";
		public const string Metafields = "Metafields";
		public const string MetaNamespace = "Meta Namespace";
		public const string MetaValueType = "Meta Value Type";
		public const string Method = "Method";
		public const string MinimumOrderQuantity = "Minimum Order Quantity";
		public const string Name = "Name";
		public const string NameLabel = "Name Label";
		public const string Notes = "Notes";
		public const string OfflinePayment = "Offline Payment";
		public const string OpenGraphDescription = "Open Graph Description";
		public const string OpenGraphTitle = "Open Graph Title";
		public const string OpenGraphType = "Open Graph Type";
		public const string OpenGraphUseImage = "Open Graph Use Image";
		public const string OpenGraphUseMetaDescription = "Open Graph Use Meta Description";
		public const string OpenGraphUseProductName = "Open Graph Use Product Name";
		public const string OrderData = "Order Data";
		public const string OrderDate = "Order Date";
		public const string OrderId = "Order ID";
		public const string RefundReason = "Reason";
		public const string Payments = "Payments";
		public const string ProductVariants = "Product Variants";
		public const string RefundedItems = "Items";
		public const string ItemId = "Item Id";
		public const string ItemType = "Item Type";
		public const string TotalAmount = "Total Amount";
		public const string OrderPaymentStatus = "Order Payment Status";
		public const string OrdersCoupon = "Orders Coupon";
		public const string OrdersCouponType = "Orders Coupon Type";
		public const string OrdersProduct = "Orders Product";
		public const string OrdersProductsConfigurableField = "Orders Products Configurable Field";
		public const string OrdersProductsOption = "Order Product Options";
		public const string OrdersProductsType = "Orders Products Type";
		public const string OrdersShipment = "Orders Shipment";
		public const string OrdersShippingAddress = "Orders Shipping Address";
		public const string OrdersTax = "Orders Tax";
		public const string OrdersTransaction = "Orders Transaction";
		public const string OrdersTransactionData = "Orders Transaction Data";
		public const string OriginalBalance = "Original Balance";
		public const string OriginalFileName = "Original File Name";
		public const string PackingSlipNotes = "Packing Slip Notes";
		public const string PageTitle = "Page Title";
		public const string ParentID = "Parent ID";
		public const string Password = "Password";
		public const string PaymentMethod = "Payment Method";
		public const string Phone = "Phone";
		public const string PhoneNumber = "Phone Number";
		public const string PostalCode = "Postal Code";
		public const string PostalMatch = "Postal Match";
		public const string StoreCredit = "Store Credit";
		public const string StoreCreditAmount = "Store Credit Amount";
		public const string Price = "Price";
		public const string PriceExcludingTax = "Price Excluding Tax";
		public const string PriceIncludingTax = "Price Including Tax";
		public const string PriceListId = "Price List ID";
		public const string PriceTax = "Price Tax";
		public const string Product = "Product";
		public const string ProductsAvailability = "Product Availability";
		public const string ProductCategoryData = "Product Category Data";
		public const string ProductDescription = "Product Description";
		public const string Productid = "Product ID";
		public const string ProductName = "Product Name";
		public const string ProductQuantityData = "Product Quantity";
		public const string PurchasingDisabled = "Purchasing Disabled";
		public const string PurchasingDisabledMessage = "Purchasing Disabled Message";
		public const string RelatedProductsData = "Related Products";
		public const string ProductTaxCode = "Product Tax Code";
		public const string ProductType = "Product Type";
		public const string Quantity = "Quantity";
		public const string QuantityRefund = "Quantity Refund";
		public const string QuantityShipped = "Quantity Shipped";
		public const string RefundAmount = "Refund Amount";
		public const string RelatedProducts = "Related Products";
		public const string RemainingBalance = "Remaining Balance";
		public const string RetailPrice = "Retail Price";
		public const string SalePrice = "Sale Price";
		public const string SearchKeywords = "Search Keywords";
		public const string Shipment = "Shipment";
		public const string ShipmentData = "Shipment Data";
		public const string ShipmentID = "Shipment ID";
		public const string ShipmentItems = "Shipment Items";
		public const string ShippingCostExcludingTax = "Shipping Cost Excluding Tax";
		public const string ShippingCostIncludingTax = "Shipping Cost Including Tax";
		public const string ShippingCostTax = "Shipping Cost Tax";
		public const string ShippingMethod = "Shipping Method";
		public const string ShippingMethodId = "Shipping Method ID";
		public const string ShippingMethodName = "Shipping Method Name";
		public const string ShippingMethodType = "Shipping Method Type";
		public const string ShippingProvider = "Shipping Provider";
		public const string ShippingTo = "Shipping To";
		public const string ShippingZone = "Shipping Zone";
		public const string ShippingZoneId = "Shipping Zone ID";
		public const string ShippingZoneName = "Shipping Zone Name";
		public const string ShippingZoneType = "Shipping Zone Type";
		public const string SKU = "SKU";
		public const string SortOrder = "Sort Order";
		public const string StaffNotes = "Staff Notes";
		public const string StartingBalance = "Starting Balance";
		public const string State = "State";
		public const string Status = "Status";
		public const string StockKeepingUnit = "Stock Keeping Unit";
		public const string Street1 = "Street 1";
		public const string Street2 = "Street 2";
		public const string StreetMatch = "Street Match";
		public const string SubtotalExcludingTax = "Subtotal Excluding Tax";
		public const string SubtotalIncludingTax = "Subtotal Including Tax";
		public const string SubtotalTax = "Subtotal Tax";
		public const string TaxClassId = "Tax Class ID";
		public const string TaxLineAmount = "Tax Line Amount";
		public const string TaxLineItemType = "Tax Line Item Type";
		public const string TaxName = "Tax Name";
		public const string TaxPriority = "Tax Priority";
		public const string TaxPriorityAmount = "Tax Priority Amount";
		public const string TaxRate = "Tax Rate";
		public const string TotalExcludingTax = "Total Excluding Tax";
		public const string TotalIncludingTax = "Total Including Tax";
		public const string TotalTax = "Total Tax";
		public const string TrackingCarrier = "Tracking Carrier";
		public const string TrackingID = "Tracking ID";
		public const string Type = "Type";
		public const string UPCCode = "UPC Code";
		public const string Value = "Value";
		public const string ViewCount = "View Count";
		public const string PreorderReleaseDate = "Pre-Order Release Date"; 
		public const string PreorderMessage = "Pre-Order Message";
		public const string IsPreorderOnly = "Is Pre-Order Only";
		public const string PriceHiddenLabel = "Price Hidden Label";
		public const string Warranty = "Warranty";
		public const string Weight = "Weight";
		public const string Width = "Width";
		public const string WrappingCostExcludingTax = "Wrapping Cost Excluding Tax";
		public const string WrappingCostIncludingTax = "Wrapping Cost Including Tax";
		public const string WrappingCostTax = "Wrapping Cost Tax";
		public const string WrappingMessage = "Wrapping Message";
		public const string WrappingName = "Wrapping Name";
		public const string Zip = "Zip";
		public const string Zipcode = "Zip Code";
		public const string Authentication = "Authentication";
		public const string TaxExemptCode = "Tax Exempt Code";
		public const string ForcePasswordReset = "Force Password Reset On Next Login";
		public const string ReceiveACSOrReviewEmails = "Receive ACS/Review Emails";
	}

	//BigCommerce allowed card type values
	public static class BigCommerceCardTypes
	{
		public const string Alelo = "alelo";
		public const string Alia = "alia";
		public const string AmericanExpress = "american_express";
		public const string Cabal = "cabal";
		public const string Carnet = "carnet";
		public const string Dankort = "dankort";
		public const string DinersClub = "diners_club";
		public const string Discover = "discover";
		public const string Elo = "elo";
		public const string Forbrugsforeningen = "forbrugsforeningen";
		public const string Jcb = "jcb";
		public const string Maestro = "maestro";
		public const string Master = "master";
		public const string Naranja = "naranja";
		public const string Sodexo = "sodexo";
		public const string Unionpay = "unionpay";
		public const string Visa = "visa";
		public const string Vr = "vr";
	}

	//Bigcommerce product Option Types
	public static class BigCommerceOptionTypes
	{
		public const string Dropdown = "dropdown";
		public const string Swatch = "swatch";
	}
}
