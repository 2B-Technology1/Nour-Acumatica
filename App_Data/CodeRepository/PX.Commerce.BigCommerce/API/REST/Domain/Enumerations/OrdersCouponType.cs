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

using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrdersCouponType
    {
        /// <summary>
        /// 0 - Dollar amount off each item in the order
        /// </summary>
        [EnumMember(Value = "0")]
        [Description(BigCommerceCaptions.Item)]
        DollarAmountOffEachItemInOrder = 0,

        /// <summary>
        /// 1 - Percentage off each item in the order
        /// </summary>
        [EnumMember(Value = "1")]
        [Description(BigCommerceCaptions.Percentage)]
        PercentageOffEachItemInOrder = 1,

        /// <summary>
        /// 2 - Dollar amount off the order total
        /// </summary>
        [EnumMember(Value = "2")]
        [Description(BigCommerceCaptions.Total)]
        DollarAmountAffOrderTotal = 2,

        /// <summary>
        /// 3 - Dollar amount off the shipping total
        /// </summary>
        [EnumMember(Value = "3")]
        [Description(BigCommerceCaptions.Shipping)]
        DollarAmountOffShippingTotal = 3,

        /// <summary>
        /// 4 - Free shipping
        /// </summary>
        [EnumMember(Value = "4")]
        [Description(BigCommerceCaptions.FreeShipping)]
        FreeShipping = 4,

        /// <summary>
        /// 5 - promotion
        /// </summary>
        [EnumMember(Value = "5")]
        [Description(BigCommerceCaptions.Promotion)]
        Promotion = 5
    }
}
