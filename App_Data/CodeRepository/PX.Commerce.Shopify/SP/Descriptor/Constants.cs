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

namespace PX.Commerce.Shopify
{
	public static class ShopifyConstants
	{
		//Constant Value
		public const int ApiCallLimitDefault = 2;
		public const int ApiCallLimitPlus = 4;
		public const int ProductOptionsLimit = 3;
		public const int ProductVarantsLimit = 100;
		public const string ShopifyConnector = "SPC";
		public const string ShopifyName = "Shopify";

		[Obsolete("The API version is not supported since version 2022R122 or later")]
		public const string ApiVersion_202301 = "2023-01";
		[Obsolete("The API version is not supported since version 2022R214 or later")]
		public const string ApiVersion_202304 = "2023-04";
		[Obsolete("The API version is not supported since version 2022R220 or later")]
		public const string ApiVersion_202307 = "2023-07";
		public const string ApiVersion_202310 = "2023-10";
		public const string InventoryManagement_Shopify = "shopify";

		public const string ValueType_SingleString = "single_line_text_field";
		public const string ValueType_MultiString = "multi_line_text_field";
		public const string Variant = "Variant";
		public const string Order = "Order";
		public const string POSSource = "pos";
		public const string ProductImage = "ProductImage";
		public const string Bogus = "bogus";
		public const string GiftNote = "gift-note";
		public const string ShopifyPayments = "shopify_payments";
		public const string GiftCard = "gift_card";
		public const string GiftCardID = "gift_card_id";
		public const string GiftCardLastCharacters = "gift_card_last_characters";
		public const string ExternalDiscount = "External Discount";
		public const string TikTokPayments = "TikTok_Payments";

		public const string CommerceShopifyMaxAttempts = "CommerceShopifyMaxApiCallAttempts";
		public const string CommerceShopifyDelayTimeIfFailed = "CommerceShopifyApiCallDelayTimeIfFailed";
		public const string XShopifyAccessToken = "X-Shopify-Access-Token";
		public const string ShopifyAPIVersion = "CommerceShopifyApiVersion";

		//Shopify roles names - must not be in the BCCaptions as these strings must not be translated.
		public const string ShopifyLocationAdmin = "Location admin"; //Translated to Admin in the ERP
		public const string ShopifyOrderingOnly = "Ordering only"; // Translated to User in the ERP
	}

	public static class ShopifyGraphQLConstants
	{
		public const string TokenHeaderName = "X-Shopify-Access-Token";
		public const string Endpoint = "{0}/api/{1}/graphql.json";
		public const string Cost = "cost";
		public const string OWNERTYPE_PRODUCT = "PRODUCT";
        public const string OWNERTYPE_PRODUCTVARIANT = "PRODUCTVARIANT";
        public const string OWNERTYPE_CUSTOMER = "CUSTOMER";
        public const string OWNERTYPE_ORDER = "ORDER";

		public static class Objects
		{
			public const string Company = "Company";
			public const string CompanyLocationCatalog = "CompanyLocationCatalog";
			public const string CompanyContact = "CompanyContact";
			public const string CompanyLocation = "CompanyLocation";
			public const string Customer = "Customer";
			public const string DraftOrder = "DraftOrder";
			public const string Order = "Order";
			public const string Refund = "Refund";
			public const string OrderTransaction = "OrderTransaction";
			public const string MailingAddress = "MailingAddress";
			public const string Product = "Product";
			public const string ProductVariant = "ProductVariant";
			public const string PriceList = "PriceList";
		}

		public static class ModuleNames
		{
			public const string CustomerAddress = "CustomerAddress";
		}
	}

}
