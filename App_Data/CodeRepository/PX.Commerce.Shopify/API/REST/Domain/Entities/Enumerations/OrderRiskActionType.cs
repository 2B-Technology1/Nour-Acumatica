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
	/// The recommended action given to the merchant. Valid values:
	/// cancel: There is a high level of risk that this order is fraudulent. The merchant should cancel the order.
	/// investigate: There is a medium level of risk that this order is fraudulent. The merchant should investigate the order.
	/// accept: There is a low level of risk that this order is fraudulent. The order risk found no indication of fraud.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum OrderRiskActionType
	{
		/// <summary>
		/// cancel: There is a high level of risk that this order is fraudulent. The merchant should cancel the order.
		/// </summary>
		[EnumMember(Value = "cancel")]
		Cancel = 0,

		/// <summary>
		/// investigate: There is a medium level of risk that this order is fraudulent. The merchant should investigate the order.
		/// </summary>
		[EnumMember(Value = "investigate")]
		Investigate = 1,

		/// <summary>
		/// accept: There is a low level of risk that this order is fraudulent. The order risk found no indication of fraud.
		/// </summary>
		[EnumMember(Value = "accept")]
		Accept = 2
	}
}
