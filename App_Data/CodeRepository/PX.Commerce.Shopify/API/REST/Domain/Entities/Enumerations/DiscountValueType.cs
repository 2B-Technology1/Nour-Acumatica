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
	/// The type of the value. Valid values:
	/// fixed_amount: A fixed amount discount value in the currency of the order.
	/// percentage: A percentage discount value.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum DiscountValueType
	{
		/// <summary>
		/// fixed_amount: A fixed amount discount value in the currency of the order.
		/// </summary>
		[EnumMember(Value = "fixed_amount")]
		FixedAmount = 0,

		/// <summary>
		/// percentage: A percentage discount value.
		/// </summary>
		[EnumMember(Value = "percentage")]
		Percentage = 1
	}
}
