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

using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace PX.Commerce.BigCommerce.API.REST
{
	[Description(BigCommerceCaptions.OrdersProductsConfigurableField)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class OrdersProductsConfigurableField
    {
        /// <summary>
        /// The ID of the order configurable field applied to the order product.
        /// </summary>
        [JsonProperty("id")]
        public virtual int Id { get; set; }

        /// <summary>
        /// The ID of the configurable field originally applied to the Product.
        /// </summary>
        [JsonProperty("configurable_field_id")]
        public virtual int ConfigurableFieldId { get; set; }

        /// <summary>
        /// The ID of the order the configurable field is associated with. 
        /// </summary>
        [JsonProperty("order_id")]
        public virtual int OrderId { get; set; }

        /// <summary>
        /// The ID of the order product the configurable field is associated with.
        /// </summary>
        [JsonProperty("order_product_id")]
        public virtual int OrderProductId { get; set; }

        /// <summary>
        /// The ID of the actual product the field was applied to.
        /// </summary>
        [JsonProperty("product_id")]
        public virtual int ProductId { get; set; }

        /// <summary>
        /// The value that was configured/chosen by the customer. The data type of the value depends on the type of field, as described below:
        ///
        /// Short Text - text
        /// Textarea - text
        /// Select Box - text
        /// Checkbox - boolean
        /// File - string(255) - A URI to the file supplied by the customer
        /// </summary>
        [JsonProperty("value")]
		[Description(BigCommerceCaptions.FieldValue)]
		public virtual string FieldValue { get; set; }

        /// <summary>
        /// The name of the file as originally uploaded by the customer.
        /// 
        /// string(255)
        /// </summary>
        [JsonProperty("original_filename")]
		[Description(BigCommerceCaptions.OriginalFileName)]
		public virtual string OriginalFilename { get; set; }

        /// <summary>
        /// The name/label of the field. 
        /// 
        /// string(255)
        /// </summary>
        [JsonProperty("name")]
		[Description(BigCommerceCaptions.NameLabel)]
		public virtual string NameLabel { get; set; }

        /// <summary>
        /// The type of configurable field, one of the following string values:
        ///
        ///text (Short Text)
        ///textarea
        ///select
        ///checkbox
        ///file
        ///
        /// ?string(50)
        /// </summary>
        [JsonProperty("type")]
		[Description(BigCommerceCaptions.FieldType)]
		public virtual string FieldType { get; set; }

        /// <summary>
        /// The list of valid options for a Select Box field as defined on the Product. 
        /// 
        /// ?string(255)
        /// </summary>
        [JsonProperty("select_box_options")]
        public virtual List<string> SelectBoxOptions { get; set; }
    }
}
