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
    [CommerceDescription(BigCommerceCaptions.OrdersProductsOption)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class OrdersProductsOption : BCAPIEntity
    {
        /// <summary>
        /// The ID of the order product option applied to the order product.
        /// </summary>
        [JsonProperty("id")]
        public virtual int Id { get; set; }

        /// <summary>
        /// The ID of the order product the option is applied to. 
        /// </summary>
        [JsonProperty("order_product_id")]
        public virtual int OrderProductId { get; set; }

        /// <summary>
        /// The ID of the Product Option the order product option refers to. 
        /// </summary>
        [JsonProperty("option_id")]
        public virtual int OptionId { get; set; }

        /// <summary>
        /// The ID of the Product Option that was applied to the actual product. 
        /// </summary>
        [JsonProperty("product_option_id")]
        public virtual int ProductOptionId { get; set; }

        /// <summary>
        /// The type of Product Option. eg. Multiple choice 
        /// 
        /// string(255)
        /// </summary>
        [JsonProperty("type")]
		[Description(BigCommerceCaptions.OrdersProductsType)]
		public virtual string ProductOptionType { get; set; }

        /// <summary>
        /// The display style of the Product Option. eg. Radio 
        /// 
        /// string(255)
        /// </summary>
        [JsonProperty("display_style")]
		[Description(BigCommerceCaptions.DisplayStyle)]
		public virtual string DisplayStyle { get; set; }

        /// <summary>
        /// The unique internal reference name of the Product Option. 
        /// </summary>
        [JsonProperty("name")]
		[Description(BigCommerceCaptions.Name)]
		public virtual string Name { get; set; }

        /// <summary>
        /// The display name of the Product Option (as shown on the product page). 
        /// 
        /// string(255)
        /// </summary>
        [JsonProperty("display_name")]
		[Description(BigCommerceCaptions.DisplayName)]
		public virtual string DisplayName { get; set; }

        /// <summary>
        /// The customer supplied value for this option. The value depends on the type 
        /// of Product Option. See the Product Option supplemental document for more details.  
        /// </summary>
        [JsonProperty("value")]
		[Description(BigCommerceCaptions.Value)]
		public virtual string Value { get; set; }

        /// <summary>
        /// A readable representation of the customer supplied value. 
        /// 
        /// text 
        /// </summary>
        [JsonProperty("display_value")]
		[Description(BigCommerceCaptions.DisplayValue)]
		public virtual string DisplayValue { get; set; }
    }
}
