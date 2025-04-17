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
	/// Creates an order. By default, product inventory is not claimed. The behaviour to use when updating inventory. (default: bypass)
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
    public enum OrderInventoryBehaviour
    {
		/// <summary>
		/// bypass: Do not claim inventory.
		/// </summary>
		[EnumMember(Value = "bypass")]
		Bypass = 0,

		/// <summary>
		/// decrement_ignoring_policy: Ignore the product's inventory policy and claim inventory.
		/// </summary>
		[EnumMember(Value = "decrement_ignoring_policy")]
		DecrementIgnoringPolicy = 1,

		/// <summary>
		/// decrement_obeying_policy: Follow the product's inventory policy and claim inventory, if possible.
		/// </summary>
		[EnumMember(Value = "decrement_obeying_policy")]
		DecrementObeyingPolicy = 2
    }
}
