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

namespace PX.Commerce.Shopify
{
	[PXLocalizable()]
	public static class ShopifyMessages
	{
		public const string CannotExportOrder = "The {0} order could not be exported to Shopify because at least one refund has been imported for it.";
		public const string DataProviderNotSupportMethod = "Data provider {0} does not support the {1} method.";
		public const string DiscountAppliedToShippingItem = "Discount applied to shipping item";
		public const string DiscountAppliedToLineItem = "Discount applied to line item";
		public const string DiscountCombined = "Aggregated order discount";
		public const string ExternalLocationNotFound = "The Product Availability entity cannot be synchronized because the Shopify location could not be found. Please check the warehouse and location mapping on the Inventory Settings tab of the Shopify Stores (BC201010) form.";
		public const string ExternalProductNotFound = "Product Availability could not be synchronized for the following items because their corresponding products or variants have been deleted from the Shopify store: {0}.";
		public const string GiftcardGateway = "{0}; Card Code: {1}";
		public const string InventoryLevelGetAllNoFilter = "You must include inventory_item_ids, location_ids, or both as filter parameters";
		public const string InventoryLocationNotFound = "The inventory location data could not be obtained from Shopify. Please check your network connection and the store settings on the Shopify Stores (BC201010) form.";
		public const string NoGuestCustomer = "The customer record to be used for synchronization cannot be found. Make sure that the customer exists and the guest customer account is specified in the store settings on the Shopify Stores (BC201010) form.";
		public const string NoRequiredField = "{0} is a required field in Shopify. Please provide {0} in the {1} record.";
		public const string NoCustomerNumbering = "The Shopify integration requires autonumbering of customers. Please configure a numbering sequence in the store settings on the Shopify Stores (BC201010) form or on the Segmented Keys (CS202000) form.";
		public const string NoLocationNumbering = "The Shopify integration requires autonumbering of customer locations. Please configure a numbering sequence in the store settings on the Shopify Stores (BC201010) form or on the Segmented Keys (CS202000) form.";
		public const string NoCustomerClass = "The Shopify integration requires the default customer class to be configured. Please configure the default customer class on the Shopify Stores (BC201010) form.";
		public const string NoBranch = "The Shopify integration requires the default branch to be configured. Please configure the default branch on the Shopify Stores (BC201010) form.";
		public const string NoProductVariants = "The {0} template item could not be synchronized with the Shopify store. Make sure that at least one of the matrix items generated from this template item has the Active, No Purchases, or No Request status and the Export to External System check box selected.";
		public const string NoSalesOrderType = "The Shopify integration requires an active sales order type to be specified as the default. Please configure the default sales order type on the Shopify Stores (BC201010) form.";
		public const string NoReturnOrderType = "The Shopify integration requires the default return order type to be configured. Please configure the default return order type on the Shopify Stores (BC201010) form.";
		public const string NoSubstituteValues = "The Shopify integration requires substitute values for the {0} mapping. Please provide substitute values on the Substitute Lists (BC105000) form.";
		public const string NoOrderDetails = "The Shopify integration requires document details to be added to orders. Please add order lines to the {0} order.";
		public const string NoDefaultCashAccount = "The payment cannot be saved because the Shopify integration requires the default cash account for the {0} payment method. Please specify the default cash account on the Payment Methods (CA204000) form.";
		public const string OrderShipmentNotCreated = "The Shopify order {0} has already been fulfilled. Another fulfillment cannot be created.";
		public const string ProductOptionsOutOfScope = "Product options could not be created in Shopify. There are {0} variant types in the {1} template item, but Shopify supports only up to {2} product options.";
		public const string ProductVariantsOutOfScope = "Product variants could not be created in Shopify. There are {0} matrix items in the {1} template item, but Shopify supports only up to {2} product variants.";
		public const string RefundDiscount = "Refund Discount";
		public const string ShipmentQtyNotMatch = "The {0} item quantity in the shipment line is greater than the unfulfilled quantity in the Shopify order {1}.";
		public const string ShipmentNotFulfullable = "The {0} shipment has been filtered because it does not contain any items to be fulfilled in the external system.";
		public const string ShipTermModeNotMatch = "In the {0} shipping terms with the freight price based on shipment, the shipping price must be zero. Please change the shipping price to zero or update the mapping of terms on the Order Settings tab of the Shopify Stores (BC201010) form.";
		public const string InvalidStoreUrl = "Invalid store URL. Please make sure the URL is specified in the following format: https://yourstorename.myshopify.com/admin/";
		public const string TestConnectionStoreNotFound = "Cannot retrieve the store through Shopify REST API. Please check if the store URL is correct.";
		public const string TooManyApiCalls = "The API call failed because too many requests were sent in a short period of time. Please try again later.";
		public const string NoRefundItem= "The refund amount item could not be found. Please specify it on the Order Settings tab of the Shopify Stores (BC201010) form.";
		public const string NoGiftCertificateItem = "The gift certificate item could not be found. Please specify it on the Order Settings tab of the Shopify Stores (BC201010) form.";
		public const string PaymentDescription = "{0} | Order: {1} | Type: {2} | Status: {3} | Gateway: {4}";
		public const string PaymentRefundDescription = "{0} | Order: {1} | Refund: {2} |Type: {3} | Status: {4} | Gateway: {5}";
		public const string OrderDescription = "{0} | Order: {1} | Status: {2}";
		public const string POSOrderNotSupported = "The {0} order has been skipped. Make sure the Shopify POS feature is enabled on the Enable/Disable Features (CS100000) form and the Import POS Orders check box is selected on the Order Settings tab of the Shopify Stores (BC201010) form.";
		public const string ReasonCodeRequired = "A refund reason code is required for processing this record. Please provide the refund reason code on the Order Settings tab of the Shopify Stores (BC201010) form.";
		public const string GiftNote = "Gift Note: {0}";
		public const string ApiTokenRequired = "To establish a connection to your Shopify store, specify either the access token (in public app mode or custom app mode), or the API key and password (in private app mode).";
		public const string ExternalPriceLessThanErpPrice = "The sales order could not be processed. Make sure the extended price of the {0} item is less than or equal to the item's price in the order in Shopify, and process the sales order again.";
		public const string ErrorDuringImageDeletionExceptionMessage = "An error occurred while deleting an image in Shopify. {0}";
		public const string OrderTaxesDiscrepancy = "Taxes are not exported to Shopify. There might be a discrepancy between the tax amounts calculated in Acumatica ERP and the tax amounts calculated in the Shopify store.";
		public const string PriceListNotSupportCurrency = "Price lists have not been created for the {0} customer price class in the following currencies: {1}. Check the currency settings in the {2} store.";
		public const string PriceListExportResultMsg = "The following number of prices has been exported to Shopify price lists: {0}.";
		public const string PriceListNoItemExported = "The {0} price list has not been exported because it does not contain any prices that meet the price export requirements.";

		public const string UnableToLoadMetafieldsDefinitionsFromShopify = "Metafield definitions could not be loaded from Shopify.";
		public const string InvalidValueForADefinedMetafieldInShopify = "{0} is not a valid value for the {1} type, which was used in Shopify for the {2} key. Modify the type in Shopify or specify a valid value for the {1} type.";


		public const string LogMessage_ErrorDuringACallToGraphQLClient = "An error occurred during a call to SPGraphQLAPIClient. {0}";

		public const string PaymentFailedFeesNotLinkedToEntryType = "The {0} payment based on the {1} payment method could not be processed because the following Shopify fees have not been mapped: {2}. Map the fees with entry types in the Shopify Fees section of the Payment Settings tab of the Shopify Stores (BC201010) form, and process the payment again.";
	}

	[PXLocalizable()]
	public static class ShopifyCaptions
	{
		//Shopify API Object Descriptions
		public const string AcceptsMarketing = "Accepts Marketing";
		public const string AddressLine1 = "Address Line 1";
		public const string AddressLine2 = "Address Line 2";
		public const string Amount = "Amount";
		public const string AmountPresentment = "Presentment Amount";
		public const string AmountSet = "Amount Set";
		public const string Authorization = "Authorization";
		public const string AvsResultCode = "AVS Result Code";
		public const string AvalaraTaxCode = "Avalara Tax Code";
		public const string Barcode = "Barcode";
		public const string BillingAddress = "Billing Address";
		public const string BodyHTML = "Body HTML Description";
		public const string CancelReason = "Cancel Reason";
		public const string CarrierIdentifier = "Carrier Identifier";
		public const string City = "City";
		public const string ClientDetails = "Client Details";
		public const string Catalog = "Catalog";
		public const string Code = "Code";
		public const string Company = "Company";
		public const string CompanyName = "Company Name";
		public const string CompanyAddressData = "Company Address Data";
		public const string CompanyLocationData = "Company Location Data";
		public const string CompanyContactData = "Company Contact Data";
		public const string CompanyCustomerData = "Company Customer Data";
		public const string Country = "Country";
		public const string CountryName = "Normalized Country Name";
		public const string CountryISOCode = "Country ISO Code";
		public const string CreditCard = "Credit Card";
		public const string CreditCardBin = "Credit Card BIN";
		public const string CreditCardCompany = "Credit Card Company";
		public const string CreditCardNumber = "Credit Card Number";
		public const string CreditCardName = "Credit Card Name";
		public const string CreditCardWallet = "Credit Card Wallet";
		public const string CreditCardExpYear = "Credit Card Expiration Year";
		public const string CreditCardExpMonth = "Credit Card Expiration Month";
		public const string Currency = "Currency";
		public const string CurrencyCode = "Currency Code";
		public const string CurrencyExchangeRate = "Currency Exchange Rate";
		public const string CurrentSubTotalPrice = "Current Subtotal Price";
		public const string CurrentSubTotalPricePresentment = "Presentment Current Subtotal Price";
		public const string CurrentTotalAdditionalFeesPresentment = "Presentment Current Total Additional Fees";
		public const string CurrentTotalDiscounts = "Current Total Discounts";
		public const string CurrentTotalDiscountsPresentment = "Presentment Current Total Discounts";
		public const string CurrentTotalDutiesPresentment = "Presentment Current Total Duties";
		public const string CurrentTotalPrice = "Current Total Price";
		public const string CurrentTotalPricePresentment = "Presentment Current Total Price";
		public const string CurrentTotalTax = "Current Total Tax";
		public const string CurrentTotalTaxPresentment = "Presentment Current Total Tax";
		public const string Customer = "Customer";
		public const string CustomerAddress = "Customer Address";
		public const string CustomerAddressData = "Customer Address Data";
		public const string CustomerId = "Customer ID";
		public const string CvvResult = "CVV Result";
		public const string CvvResultCode = "CVV Result Code";
		public const string DateCanceled = "Date Canceled";
		public const string DateCreated = "Date Created";
		public const string DateModified = "Date Modified";
		public const string DateShipped = "Date Shipped";
		public const string DefaultAddress = "Default Address";
		public const string DeliveryMethod = "Delivery Method";
		public const string Depth = "Depth";
		public const string DeviceId = "Device ID";
		public const string Discount = "Discount";
		public const string DiscountedPrice = "Discounted Price";
		public const string DiscountedPricePresentment = "Presentment Discounted Price";
		public const string DiscountedPriceSet = "Discounted Price Set";
		public const string DiscountAllocation = "Discount Allocation";
		public const string DiscountAmount = "Discount Amount";
		public const string DiscountAmountPresentment = "Presentment Discount Amount";
		public const string DiscountRule = "Discount Rule";
		public const string Duties = "Duties";
		public const string ErrorCode = "Error Code";
		public const string Email = "Email";
		public const string EmailAddress = "Email Address";
		public const string EventName = "Event Name";
		public const string FinancialStatus = "Financial Status";
		public const string FirstName = "First Name";
		public const string Fulfillment = "Fulfillment";
		public const string FulfillmentAt = "Fulfillment At";
		public const string FulfillmentBy = "Fulfillment By";
		public const string FulfillableQuantity = "Fulfillable Quantity";
		public const string FulfillmentOrder = "Fulfillment Order";
		public const string FulfillmentService = "Fulfillment Service";
		public const string FulfillmentStatus = "Fulfillment Status";
		public const string Gateway = "Gateway";
		public const string GiftCard = "Gift Card";
		public const string GlobalDescriptionTag = "Global Description Tag";
		public const string GlobalTitleTage = "Global Title Tag";
		public const string GlobalTradeNumber = "Global Trade Number";
		public const string Grams = "Grams";
		public const string Height = "Height";
		public const string Id = "ID";
		public const string ImageUrl = "Image Url";
		public const string InventoryBehaviour = "Inventory Behavior";
		public const string InventoryItem = "Inventory Item";
		public const string InventoryLevel = "Inventory Level";
		public const string InventoryLocation = "Inventory Location";
		public const string InventoryManagement = "Inventory Management";
		public const string InventoryPolicy = "Inventory Policy";
		public const string InventoryTracking = "Inventory Tracking";
		public const string IsDefault = "Is Default";
		public const string IsMainContact = "Is Main Contact";
		public const string ItemShipped = "Item Shipped";
		public const string ItemsTotal = "Items Total";
		public const string ItemsTotalPresentment = "Presentment Items Total";
		public const string ItemsTotalSet = "Items Total Set";
		public const string Kind = "Kind";
		public const string LandingSite = "Landing Site";
		public const string LastName = "Last Name";
		public const string Latitude = "Latitude";
		public const string LineItem = "Line Item";
		public const string LineItemId = "Line Item ID";
		public const string LocationId = "Location ID";
		public const string Longitude = "Longitude";
		public const string MaximumDeliveryDate = "Maximum Delivery Date";
		public const string Message = "Message";
		public const string MetaDescription = "Meta Description";
		public const string Metafields = "Metafields";
		public const string MetaKeywords = "Meta Keywords";
		public const string MetaNamespace = "Meta Namespace";
		public const string MetaValueType = "Meta Value Type";
		public const string Method = "Method";
		public const string MinimumOrderQuantity = "Minimum Order Quantity";
		public const string MinimumDeliveryDate = "Minimum Delivery Date";
		public const string Name = "Name";
		public const string NameLabel = "Name Label";
		public const string Note = "Note";
		public const string NoteAttribute = "Note Attribute";
		public const string NotifyCustomer = "Notify Customer";
		public const string Option1 = "Option1";
		public const string Option2 = "Option2";
		public const string Option3 = "Option3";
		public const string OrderAdjustment = "Order Adjustment";
		public const string OrderAddress = "Order Address";
		public const string OrderData = "Order Data";
		public const string OrderDate = "Order Date";
		public const string OrderId = "Order ID";
		public const string OrderItemLocation = "Order Item Location";
		public const string OrderNumber = "Order Number";
		public const string PurchaseOrderNumber = "PO Number";
		public const string OrderPaymentStatus = "Order Payment Status";
		public const string OrderRisk = "Order Risk";
		public const string OrderTotal = "Order Total";
		public const string OrderTotalPresentment = "Presentment Order Total";
		public const string OrderTotalSet = "Order Total Set";
		public const string OrderRefundsTotalPresentment = "Presentment Order Refunds Total";
		public const string OrderShippingsTotalPresentment = "Presentment Order Shipping Fees Total";
		public const string OrdersProduct = "Orders Product";
		public const string OrdersProductsOption = "Orders Products Option";
		public const string OrdersProductsType = "Orders Products Type";
		public const string OrdersShipment = "Orders Shipment";
		public const string OrdersShippingAddress = "Orders Shipping Address";
		public const string OrdersTax = "Orders Tax";
		public const string OrderStatusURL = "Order Status URL";
		public const string OrdersTransaction = "Orders Transaction";
		public const string OrdersTransactionData = "Orders Transaction Data";
		public const string OriginalTotalDutiesPresentment = "Presentment Original Total Duties";
		public const string PageTitle = "Page Title";
		public const string ParentId = "Parent ID";
		public const string Password = "Password";
		public const string PaymentDetail = "Payment Detail";
		public const string PaymentMethod = "Payment Method";
		public const string Phone = "Phone";
		public const string PhoneNumber = "Phone Number";
		public const string PostalCode = "Postal Code";
		public const string PresentmentCurrency = "Presentment Currency";
		public const string PresentmentMoney = "Presentment Money";
		public const string Price = "Price";
		public const string PricePresentment = "Presentment Price";
		public const string PriceExcludingTax = "Price Excluding Tax";
		public const string PriceIncludingTax = "Price Including Tax";
		public const string PriceListId = "Price List Id";
		public const string PriceSet = "Price Set";
		public const string PriceTax = "Price Tax";
		public const string ProcessedAt = "Date Processed";
		public const string ProcessingMethod = "Processing Method";
		public const string Product = "Product";
		public const string ProductDescription = "Product Description";
		public const string ProductExists = "Product Exists";
		public const string ProductId = "Product Id";
		public const string ProductName = "Product Name";
		public const string ProductOptions = "Product Options";
		public const string ProductTaxCode = "Product Tax Code";
		public const string ProductType = "Product Type";
		public const string ProductVariants = "Product Variants";
		public const string Properties = "Properties";
		public const string Province = "Province";
		public const string ProvinceCode = "Province Code";
		
		public const string Published = "Published";
		public const string PublishedScope = "Published Scope";
		public const string Quantity = "Quantity";
		public const string QuantityRefund = "Quantity Refund";
		public const string QuantityShipped = "Quantity Shipped";
		public const string Reason = "Reason";
		public const string Receipt = "Receipt";
		public const string Recommendation = "Recommendation";
		public const string ReferringSite = "Referring Site";
		public const string Refund = "Refund";
		public const string RefundAmount = "Refund Amount";
		public const string RefundId = "Refund ID";
		public const string RefundItem = "Refund Item";
		public const string RemainingBalance = "Remaining Balance";
		public const string RequiresShipping = "Requires Shipping";
		public const string RestockType = "Restock Type";
		public const string RetailPrice = "Retail Price";
		public const string SalePrice = "Sale Price";
		public const string SearchKeywords = "Search Keywords";
		public const string SendReceipt = "Send Receipt";
		public const string SendFulfillmentReceipt = "Send Fulfillment Receipt";
		public const string Service = "Service";
		public const string Score = "Score";
		public const string ShipmentData = "Shipment Data";
		public const string ShipmentID = "Shipment ID";
		public const string ShipmentItems = "Shipment Items";
		public const string ShipmentStatus = "Shipment Status";
		public const string ShippingAddress = "Shipping Address";
		public const string ShippingCostExcludingTax = "Shipping Cost Excluding Tax";
		public const string ShippingCostExcludingTaxPresentment = "Presentment Shipping Cost Excluding Tax";
		public const string ShippingCostIncludingTax = "Shipping Cost Including Tax";
		public const string ShippingCostTax = "Shipping Cost Tax";
		public const string ShippingLine = "Shipping Line";
		public const string ShippingMethod = "Shipping Method";
		public const string ShippingMethodId = "Shipping Method Id";
		public const string ShippingMethodName = "Shipping Method Name";
		public const string ShippingMethodType = "Shipping Method Type";
		public const string ShippingProvider = "Shipping Provider";
		public const string ShippingTo = "Shipping To";
		public const string ShippingZone = "Shipping Zone";
		public const string ShippingZoneId = "Shipping Zone Id";
		public const string ShippingZoneName = "Shipping Zone Name";
		public const string ShippingZoneType = "Shipping Zone Type";
		public const string ShopMoney = "Shop Money";
		public const string SKU = "SKU";
		public const string SortOrder = "Sort Order";
		public const string SourceName = "Source Name";
		public const string State = "State";
		public const string Status = "Status";
		public const string Street1 = "Street 1";
		public const string Street2 = "Street 2";
		public const string StreetMatch = "Street Match";
		public const string Subtotal = "Subtotal";
		public const string SubtotalPresentment = "Presentment Subtotal";
		public const string SubtotalExcludingTax = "Subtotal Excluding Tax";
		public const string SubtotalIncludingTax = "Subtotal Including Tax";
		public const string SubtotalSet = "Subtotal Set";
		public const string SubtotalTax = "Subtotal Tax";
		public const string Tags = "Tags";
		public const string Taxable = "Taxable";
		public const string TaxAmount = "Tax Amount";
		public const string TaxAmountPresentment = "Presentment Tax Amount";
		public const string TaxAmountSet = "Tax Amount Set";
		public const string TaxesIncluded = "Taxes Included";
		public const string TaxExempt = "Tax Exempt";
		public const string TaxExemptions = "Tax Exemptions";
		public const string TaxLine = "Tax Line";
		public const string TaxLineAmount = "Tax Line Amount";
		public const string TaxLineItemType = "Tax Line Item Type";
		public const string TaxName = "Tax Name";
		public const string TaxRate = "Tax Rate";
		public const string TestCase = "Test Case";
		public const string TestTransaction = "Test Transaction";
		public const string TemplateSuffix = "Template Suffix";
		public const string Token = "Token";
		public const string TotalExcludingTax = "Total Excluding Tax";
		public const string TotalIncludingTax = "Total Including Tax";
		public const string TotalDiscount = "Total Discount";
		public const string TotalDiscountPresentment = "Presentment Total Discount";
		public const string TotalDiscountSet = "Total Discount Set";
		public const string TotalTax = "Total Tax";
		public const string TotalTaxPresentment = "Presentment Total Tax";
		public const string TotalTaxSet = "Total Tax Set";
		public const string TotalTips = "Total Tips";
		public const string TotalWeight = "Total Weight";
		public const string TipPaymentGateway = "Tip Payment Gateway";
		public const string TipPaymentMethod = "Tip Payment Method";
		public const string Title = "Title";
		public const string TimeZone = "Time Zone";
		public const string TrackingCarrier = "Tracking Carrier";
		public const string TrackingCompany = "Tracking Company";
		public const string TrackingID = "Tracking ID";
		public const string TrackingInfo = "Tracking Info";
		public const string TrackingNumber = "Tracking Number";
		public const string TrackingNumbers = "Tracking Numbers";
		public const string TrackingUrl = "Tracking URL";
		public const string TrackingUrls = "Tracking URLs";
		public const string Transactions = "Transactions";
		public const string Type = "Type";
		public const string UPCCode = "UPC Code";
		public const string UserId = "User ID";
		public const string Value = "Value";

		public const string VariantId = "Variant ID";
		public const string VariantTitle = "Variant Title";
		public const string Vendor = "Vendor";
		public const string Weight = "Weight";
		public const string WeightUnit = "Weight Unit";
		public const string Zipcode = "Zipcode";
	}

	[PXLocalizable()]
	public static class ShopifyApiStatusCodes
	{
		public const string Code_200 = "Status Code 200/OK : The request was successfully processed by Shopify.";
		public const string Code_201 = "Status Code 201/Created : The request has been fulfilled and a new resource has been created.";
		public const string Code_202 = "Status Code 202/Accepted : The request has been accepted, but not yet processed.";
		public const string Code_303 = "Status Code 303/See Other : The response to the request can be found under a different URL in the Location header and can be retrieved using a GET method on that resource.";
		public const string Code_400 = "Status Code 400/Bad Request : The request was not understood by the server, generally due to bad syntax or because the Content-Type header was not correctly set to application/json. This status is also returned when the request provides an invalid code parameter during the OAuth token exchange process.";
		public const string Code_401 = "Status Code 401/Unauthorized : The necessary authentication credentials are not present in the request or are incorrect.";
		public const string Code_402 = "Status Code 402/Payment Required : The requested shop is currently frozen. The shop owner needs to log in to the shop's admin and pay the outstanding balance to unfreeze the shop.";
		public const string Code_403 = "Status Code 403/Forbidden : The server is refusing to respond to the request. This is generally because you have not requested the appropriate scope for this action.";
		public const string Code_404 = "Status Code 404/Not Found : The requested resource was not found but could be available again in the future.";
		public const string Code_406 = "Status Code 406/Not Acceptable : The requested resource is only capable of generating content not acceptable according to the Accept headers sent in the request.";
		public const string Code_422 = "Status Code 422/Unprocessable Entity : The request body was well-formed but contains semantic errors. The response body will provide more details in the errors or error parameters.";
		public const string Code_423 = "Status Code 423/Locked : The requested shop is currently locked. Shops are locked if they repeatedly exceed their API request limit, or if there is an issue with the account, such as a detected compromise or fraud risk. Contact support if your shop is locked.";
		public const string Code_429 = "Status Code 429/Too Many Requests : The request was not accepted because the application has exceeded the rate limit. See the API Call Limit documentation for a breakdown of Shopify's rate-limiting mechanism.";
		public const string Code_500 = "Status Code 500/Internal Server Error : An internal error occurred in Shopify. Please post to the API & Technology forum so that Shopify staff can investigate.";
		public const string Code_501 = "Status Code 501/Not Implemented : The requested endpoint is not available on that particular shop, e.g. requesting access to a Plus-specific API on a non-Plus shop. This response may also indicate that this endpoint is reserved for future use.";
		public const string Code_503 = "Status Code 503/Service Unavailable : The server is currently unavailable. Check the status page for reported service outages.";
		public const string Code_504 = "Status Code 504/Gateway Timeout : The request could not complete in time. Try breaking it down in multiple smaller requests.";
		public const string Code_Unknown = "Code {0} : Unknown code";

		public static string GetCodeMessage(string code)
		{
			if (string.IsNullOrEmpty(code))
				return string.Empty;
            string codeDesc = int.TryParse(code, out var intCode) ? $"Code_{intCode}" : code;
			var codeItem = typeof(ShopifyApiStatusCodes).GetField(codeDesc, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Static);
            return codeItem != null ? codeItem.GetRawConstantValue()?.ToString() : string.Format(Code_Unknown, code);
		}
	}
}
