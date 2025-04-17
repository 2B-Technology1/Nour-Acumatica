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
	/// The order's status in terms of fulfilled line items. Valid values:
	/// fulfilled: Every line item in the order has been fulfilled.
	/// null: None of the line items in the order have been fulfilled.
	/// partial: At least one line item in the order has been fulfilled.
	/// restocked: Every line item in the order has been restocked and the order canceled.
	/// not_eligible
	/// </summary>
	[JsonConverter(typeof(StringEnumConverterWtihNullHandler))]
	public enum OrderFulfillmentStatus
	{
        /// <summary>
		/// null: None of the line items in the order have been fulfilled.
		/// </summary>
		[EnumMember(Value = null)]
        Null = 0,

        /// <summary>
        /// fulfilled: Every line item in the order has been fulfilled.
        /// </summary>
        [EnumMember(Value = "fulfilled")]
		Fulfilled = 1,

		/// <summary>
		/// partial: At least one line item in the order has been fulfilled.
		/// </summary>
		[EnumMember(Value = "partial")]
		Partial = 2,

		/// <summary>
		/// restocked: Every line item in the order has been restocked and the order canceled.
		/// </summary>
		[EnumMember(Value = "restocked")]
		Restocked = 3,

		/// <summary>
		/// not_eligible
		/// </summary>
		[EnumMember(Value = "not_eligible")]
		NotEligible = 4
	}

    public class StringEnumConverterWtihNullHandler : Newtonsoft.Json.Converters.StringEnumConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value.Equals(OrderFulfillmentStatus.Null)) value = null;
            base.WriteJson(writer, value, serializer);
        }
    }
}
