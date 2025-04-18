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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderStatuses
    {
        [Description(BigCommerceCaptions.Incomplete)]
        Incomplete = 0,

        [Description(BigCommerceCaptions.Pending)]
        Pending = 1,

        [Description(BigCommerceCaptions.Shipped)]
        Shipped = 2,

        [Description(BigCommerceCaptions.PartiallyShipped)]
        PartiallyShipped = 3,

        [Description(BigCommerceCaptions.Refunded)]
        Refunded = 4,

        [Description(BigCommerceCaptions.Cancelled)]
        Cancelled = 5,

        [Description(BigCommerceCaptions.Declined)]
        Declined = 6,

        [Description(BigCommerceCaptions.AwaitingPayment)]
        AwaitingPayment = 7,

        [Description(BigCommerceCaptions.AwaitingPickup)]
        AwaitingPickup = 8,

        [Description(BigCommerceCaptions.AwaitingShipment)]
        AwaitingShipment = 9,

        [Description(BigCommerceCaptions.Completed)]
        Completed = 10,

        [Description(BigCommerceCaptions.AwaitingFulfillment)]
        AwaitingFulfillment = 11,

        [Description(BigCommerceCaptions.VerificationRequired)]
        VerificationRequired = 12,

        [Description(BigCommerceCaptions.Disputed)]
        Disputed = 13,

        [Description(BigCommerceCaptions.PartiallyRefunded)]
        PartiallyRefunded = 14
    }
}
