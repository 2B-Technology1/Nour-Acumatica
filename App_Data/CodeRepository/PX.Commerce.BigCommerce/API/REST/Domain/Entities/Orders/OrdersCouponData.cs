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

using Newtonsoft.Json;
using PX.Commerce.Core;
using System.ComponentModel;

namespace PX.Commerce.BigCommerce.API.REST
{
	[Description(BigCommerceCaptions.OrdersCoupon)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class OrdersCouponData : BCAPIEntity
	{
        /// <summary>
        /// The ID of the order coupon applied to the order.
        /// </summary>
        [JsonProperty("id")]
        public virtual int? Id { get; set; }

        /// <summary>
        /// The ID of the order the coupon was applied to. 
        /// </summary>
        [JsonProperty("order_id")]
        public virtual int? OrderId { get; set; }

        /// <summary>
        /// The ID of the actual Coupon that was applied to the order. 
        /// </summary>
        [JsonProperty("coupon_id")]
        public virtual int? CouponId { get; set; }

        /// <summary>
        /// The code of the coupon.
        /// string(50) 
        /// </summary>
        [JsonProperty("code")]
		[Description(BigCommerceCaptions.CouponCode)]
		public virtual string CouponCode { get; set; }

        /// <summary>
        /// The amount that the coupon is configured to discount. eg. 10 for 10% or $10 discount. 
        /// 
        /// decimal(20,4)
        /// </summary>
        [JsonProperty("amount")]
		[Description(BigCommerceCaptions.CouponAmount)]
		public virtual decimal CouponAmount { get; set; }

        /// <summary>
        /// The type of coupon which is one of the following int values:
        /// 0 - Dollar amount off each item in the order
        /// 1 - Percentage off each item in the order
        /// 2 - Dollar amount off the order total
        /// 3 - Dollar amount off the shipping total
        /// 4 - Free shipping
        /// </summary>
        [JsonProperty("type")]
		[Description(BigCommerceCaptions.OrdersCouponType)]
		public virtual OrdersCouponType CouponType { get; set; }

        [JsonProperty("discount")]
		[Description(BigCommerceCaptions.Discount)]
		public virtual decimal Discount { get; set; }
    }
}
