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
using System;
using System.ComponentModel;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonObject(Description = "Order -> Shipment -> ShipmentItem")]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class OrdersShipmentItem
    {
        public OrdersShipmentItem Clone()
        {
            return (OrdersShipmentItem)this.MemberwiseClone();
        }
        /// <summary>
        /// The ID of the Order Product the item is associated with.
        /// </summary>
        [JsonProperty("order_product_id")]
        public virtual int? OrderProductId { get; set; }

        /// <summary>
        /// The ID of the Product the item is associated with.
        /// </summary>
        [JsonProperty("product_id")]
        public virtual int ProductId { get; set; }

        /// <summary>
        /// The quantity of the item in the shipment.
        /// </summary>
        [JsonProperty("quantity")]
		[CommerceDescription(BigCommerceCaptions.Quantity, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public virtual int Quantity { get; set; }

		[JsonIgnore]
		public virtual string OrderID { get; set; }
    }
}
