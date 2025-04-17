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
	/// The status of the order. Valid values: open, closed,cancelled.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum OrderStatus
	{
        /// <summary>
		/// any: The order status can be one of the following status : open, closed,cancelled.
        /// This status only uses in the filter data.
		/// </summary>
		[EnumMember(Value = "any")]
        Any,

        /// <summary>
        /// open: The order status is open.
        /// </summary>
        [EnumMember(Value = "open")]
		Open,

		/// <summary>
		/// closed: The order status is closed.
		/// </summary>
		[EnumMember(Value = "closed")]
		Closed,

		/// <summary>
		/// cancelled: The order status is cancelled.
		/// </summary>
		[EnumMember(Value = "cancelled")]
		Cancelled

	}
}
