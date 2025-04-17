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
	/// method_type: The type of delivery method. Valid values:
	/// local: A delivery to a customer's doorstep.
	/// none: No delivery method.
	/// pick_up: A delivery that a customer picks up at your retail store, curbside, or any location that you choose.
	/// retail: Items delivered immediately in a retail store.
	/// shipping: A delivery to a customer using a shipping carrier.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum FulfillmentOrderDeliveryMethodType
	{
		/// <summary>
		/// A delivery to a customer's doorstep.
		/// </summary>
		[EnumMember(Value = "local")]
		Local = 0,

		/// <summary>
		/// No delivery method.
		/// </summary>
		[EnumMember(Value = "none")]
		None = 1,

		/// <summary>
		/// pick_up: A delivery that a customer picks up at your retail store, curbside, or any location that you choose.
		/// </summary>
		[EnumMember(Value = "pick-up")]
		PickUp = 2,

		/// <summary>
		/// retail: Items delivered immediately in a retail store.
		/// </summary>
		[EnumMember(Value = "retail")]
		Retail = 3,

		/// <summary>
		/// shipping: A delivery to a customer using a shipping carrier.
		/// </summary>
		[EnumMember(Value = "shipping")]
		Shipping = 4,
	}
}
