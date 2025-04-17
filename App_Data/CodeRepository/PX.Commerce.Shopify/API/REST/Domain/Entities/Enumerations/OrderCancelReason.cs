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
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderCancelReason
    {
		/// <summary>
		/// Customer - The customer canceled the order.
		/// </summary>
		[EnumMember(Value = "customer")]
        Customer = 0,

		/// <summary>
		/// Fraud - The order was fraudulent.
		/// </summary>
		[EnumMember(Value = "fraud")]
		Fraud = 1,

		/// <summary>
		/// Inventory - Items in the order were not in inventory.
		/// </summary>
		[EnumMember(Value = "inventory")]
		Inventory = 2,

		/// <summary>
		/// Declined - The payment was declined.
		/// </summary>
		[EnumMember(Value = "declined")]
		Declined = 3,

		/// <summary>
		/// other - A reason not in this list.
		/// </summary>
		[EnumMember(Value = "other")]
        Other = 4,

		/// <summary>
		/// staff - Staff made an error..
		/// </summary>
		[EnumMember(Value = "staff")]
		Staff = 5,
	}
}
