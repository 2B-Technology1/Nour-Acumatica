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
    public enum ProductsOpenGraphType
    {
        [EnumMember(Value = "product")]
        Product = 0,

        [EnumMember(Value = "album")]
        Album = 1,

        [EnumMember(Value = "book")]
        Book = 2,

        [EnumMember(Value = "drink")]
        Drink = 3,

        [EnumMember(Value = "food")]
        Food = 4,

        [EnumMember(Value = "game")]
        Game = 5,

        [EnumMember(Value = "movie")]
        Movie = 6,

        [EnumMember(Value = "song")]
        Song = 7,

        [EnumMember(Value = "tv_show")]
        TvShow = 8
    }
}
