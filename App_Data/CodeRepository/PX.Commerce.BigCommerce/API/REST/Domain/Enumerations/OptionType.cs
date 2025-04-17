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

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OptionType
    {
        [EnumMember(Value = "C")]
        Checkbox = 0,

        [EnumMember(Value = "D")]
        Date = 1,

        [EnumMember(Value = "F")]
        File = 2,

        [EnumMember(Value = "N")]
        NumbersOnlyText = 3,

        [EnumMember(Value = "T")]
        Text = 4,

        [EnumMember(Value = "MT")]
        MultiLineText = 5,

        [EnumMember(Value = "P")]
        ProductList = 6,

        [EnumMember(Value = "PI")]
        ProductListWithImages = 7,

        [EnumMember(Value = "RB")]
        RadioList = 8,

        [EnumMember(Value = "RT")]
        RectangleList = 9,

        [EnumMember(Value = "S")]
        SelectBox = 10,

        [EnumMember(Value = "CS")]
        Swatch = 11,
    }
}
