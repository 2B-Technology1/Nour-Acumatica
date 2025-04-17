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
	/// The status of payments associated with the order. Can only be set when the order is created. Valid values:
	/// pending: The payments are pending.Payment might fail in this state.Check again to confirm whether the payments have been paid successfully.
	/// authorized: The payments have been authorized.
	/// partially_paid: The order have been partially paid.
	/// paid: The payments have been paid.
	/// partially_refunded: The payments have been partially refunded.
	/// refunded: The payments have been refunded.
	/// voided: The payments have been voided.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum OrderFinancialStatus
	{
		/// <summary>
		/// pending: The payments are pending.Payment might fail in this state.Check again to confirm whether the payments have been paid successfully.
		/// </summary>
		[EnumMember(Value = "pending")]
		Pending = 0,

		/// <summary>
		/// authorized: The payments have been authorized.
		/// </summary>
		[EnumMember(Value = "authorized")]
		Authorized = 1,

		/// <summary>
		/// partially_paid: The order have been partially paid.
		/// </summary>
		[EnumMember(Value = "partially_paid")]
		PartiallyPaid = 2,

		/// <summary>
		/// Paid: The payments have been paid.
		/// </summary>
		[EnumMember(Value = "paid")]
		Paid = 3,

		/// <summary>
		/// partially_refunded: The payments have been partially refunded.
		/// </summary>
		[EnumMember(Value = "partially_refunded")]
		PartiallyRefunded = 4,

		/// <summary>
		/// refunded: The payments have been refunded.
		/// </summary>
		[EnumMember(Value = "refunded")]
		Refunded = 5,

		/// <summary>
		/// voided: The payments have been voided.
		/// </summary>
		[EnumMember(Value = "voided")]
		Voided = 6
	}
}
