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
	/// Whether customers are allowed to place an order for the product variant when it's out of stock. Valid values:
	/// deny: Customers are not allowed to place orders for the product variant if it's out of stock.
	/// continue: Customers are allowed to place orders for the product variant if it's out of stock.
	/// Default value: deny.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
    public enum InventoryPolicy
	{
		/// <summary>
		/// deny: Customers are not allowed to place orders for the product variant if it's out of stock.
		/// </summary>
		[EnumMember(Value = "deny")]
		Deny = 0,

		/// <summary>
		/// continue: Customers are allowed to place orders for the product variant if it's out of stock.
		/// </summary>
		[EnumMember(Value = "continue")]
		Continue = 1
    }
}
