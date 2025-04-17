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

using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.Shopify.API.REST
{
	/// <summary>
	/// awaiting_payment: The fulfillment hold is applied because payment is pending.
	/// high_risk_of_fraud: The fulfillment hold is applied because of a high risk of fraud.
	/// incorrect_address: The fulfillment hold is applied because of an incorrect address.
	/// inventory_out_of_stock: The fulfillment hold is applied because inventory is out of stock.
	/// other: The fulfillment hold is applied for any other reason.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum FulfillmentOrderReasons
	{
		/// <summary>
		/// awaiting_payment: The fulfillment hold is applied because payment is pending.
		/// </summary>
		[EnumMember(Value = "awaiting_payment")]
		AwaitingPayment = 0,

		/// <summary>
		/// high_risk_of_fraud: The fulfillment hold is applied because of a high risk of fraud.
		/// </summary>
		[EnumMember(Value = "high_risk_of_fraud")]
		HighRiskOfFraud = 1,

		/// <summary>
		/// incorrect_address: The fulfillment hold is applied because of an incorrect address.
		/// </summary>
		[EnumMember(Value = "incorrect_address")]
		IncorrectAddress = 2,

		/// <summary>
		/// inventory_out_of_stock: The fulfillment hold is applied because inventory is out of stock.
		/// </summary>
		[EnumMember(Value = "inventory_out_of_stock")]
		InventoryOutOfStock = 3,

		/// <summary>
		/// other: The fulfillment hold is applied for any other reason.
		/// </summary>
		[EnumMember(Value = "other")]
		Other = 4,
	}
}
