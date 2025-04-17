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

namespace PX.Commerce.Shopify.API.GraphQL
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderCancelReason
    {
		[EnumMember(Value = null)]
		Null = 0,

		/// <summary>
		/// Customer - The customer canceled the order.
		/// </summary>
		[EnumMember(Value = "CUSTOMER")]
        Customer = 1,

		/// <summary>
		/// Fraud - The order was fraudulent.
		/// </summary>
		[EnumMember(Value = "FRAUD")]
		Fraud = 2,

		/// <summary>
		/// Inventory - Items in the order were not in inventory.
		/// </summary>
		[EnumMember(Value = "INVENTORY")]
		Inventory = 3,

		/// <summary>
		/// Declined - The payment was declined.
		/// </summary>
		[EnumMember(Value = "DECLINED")]
		Declined = 4,

		/// <summary>
		/// other - A reason not in this list.
		/// </summary>
		[EnumMember(Value = "OTHER")]
        Other = 5
    }
}
