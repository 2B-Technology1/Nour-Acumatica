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
using System.ComponentModel;
using Newtonsoft.Json;
using PX.Commerce.Core;

namespace PX.Commerce.Shopify.API.REST
{
	/// <summary>
	/// Information about the credit card used for the transaction.
	/// </summary>
	[JsonObject(Description = "Payment Detail")]
	[CommerceDescription(ShopifyCaptions.PaymentDetail, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PaymentDetail : BCAPIEntity
	{
		/// <summary>
		/// avs_result_code: The response code from the address verification system. The code is a single letter; see this chart for the codes and their definitions.
		/// </summary>
		[JsonProperty("avs_result_code", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.AvsResultCode, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public string AvsResultCode { get; set; }

		/// <summary>
		/// credit_card_bin: The issuer identification number (IIN), formerly known as bank identification number (BIN) of the customer's credit card. 
		/// This is made up of the first few digits of the credit card number.
		/// </summary>
		[JsonProperty("credit_card_bin", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.CreditCardBin, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public String CreditCardBin { get; set; }

		/// <summary>
		/// credit_card_company: The name of the company that issued the customer's credit card.
		/// </summary>
		[JsonProperty("credit_card_company")]
		[CommerceDescription(ShopifyCaptions.CreditCardCompany, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public string CreditCardCompany { get; set; }

		/// <summary>
		/// credit_card_number: The customer's credit card number, with most of the leading digits redacted.
		/// </summary>
		[JsonProperty("credit_card_number", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.CreditCardNumber, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public string CreditCardNumber { get; set; }

		/// <summary>
		///  cvv_result_code: The response code from the credit card company indicating whether the customer entered the card security code, or card verification value, correctly. The code is a single letter or empty string; 
		/// </summary>
		[JsonProperty("cvv_result_code", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.CvvResultCode, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public string CvvResultCode { get; set; }

		/// <summary>
		/// The holder of the credit card.
		/// </summary>
		[JsonProperty("credit_card_name", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.CreditCardName, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public string CreditCardName { get; set; }

		/// <summary>
		///	The wallet type where this credit card was retrieved from.
		/// </summary>
		[JsonProperty("credit_card_wallet", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.CreditCardWallet, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public string CreditCardWallet { get; set; }

		/// <summary>
		/// The year in which the credit card expires.
		/// </summary>
		[JsonProperty("credit_card_expiration_year", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.CreditCardExpYear, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public int? CreditCardExpYear { get; set; }

		/// <summary>
		///	The month in which the credit card expires.
		/// </summary>
		[JsonProperty("credit_card_expiration_month", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.CreditCardExpMonth, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public int? CreditCardExpMonth { get; set; }
	}

}
