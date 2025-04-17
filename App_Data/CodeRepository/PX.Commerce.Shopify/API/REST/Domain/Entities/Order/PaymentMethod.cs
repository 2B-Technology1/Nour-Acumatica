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

using Newtonsoft.Json;
using PX.Commerce.Core;
using PX.Commerce.Objects;

namespace PX.Commerce.Shopify.API.REST
{
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PaymentMethod : IPaymentMethod
	{

		[JsonProperty("name")]
		public string Name { get; set; }

		public string Currency { get; set; }

		public bool CreatePaymentfromOrder { get; set; }

		public PaymentMethod(string name, string Currency, bool CreatePaymentfromOrder = false)
		{
			this.Name = name;
			this.Currency = Currency;
			this.CreatePaymentfromOrder = CreatePaymentfromOrder;
		}
	}

	public class AmountToCaptureAttribute
	{
		[JsonProperty("presentment_money", NullValueHandling = NullValueHandling.Ignore)]
		public AmountCurrencyPair PresentmentMoney { get; set; }

		[JsonProperty("shop_money", NullValueHandling = NullValueHandling.Ignore)]
		public AmountCurrencyPair ShopMoney { get; set; }
	}

	public class AmountCurrencyPair
	{
		/// <summary>
		/// The amount of money included in the transaction.
		/// </summary>
		[JsonProperty("amount", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Amount, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public decimal Amount { get; set; }

		/// <summary>
		/// The three-letter code (ISO 4217 format) for the currency used for the payment.
		/// </summary>
		[JsonProperty("currency", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.Currency, FieldFilterStatus.Filterable, FieldMappingStatus.Import)]
		public string CurrencyCode { get; set; }
	}

	public class PaymentsRefundAttribute : BCAPIEntity
	{
		/// <summary>
		/// The current status of the refund. Valid values: pending, failure, success, and error.
		/// </summary>
		[JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
		public string Status { get; set; }

		/// <summary>
		/// A unique number associated with the transaction that can be used to track the refund. 
		/// </summary>
		[JsonProperty("acquirer_reference_number", NullValueHandling = NullValueHandling.Ignore)]
		public string AcquirerReferenceNumber { get; set; }
	}
}
