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

using System;
using System.ComponentModel;

namespace PX.Commerce.BigCommerce.API.REST
{
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class FilterOrders: Filter
    {
        [Description("min_id")]
        public int? MinimumId { get; set; }

        [Description("max_id")]
        public int? MaximumId { get; set; }

        [Description("min_total")]
        public int? MinTotal { get; set; }

        [Description("max_total")]
        public int? MaxTotal { get; set; }

        [Description("customer_id")]
        public string CustomerId { get; set; }

        [Description("email")]
        public string Email { get; set; }

        [Description("status_id")]
        public int? StatusId { get; set; }

        [Description("cart_id")]
        public string CartId { get; set; }

        /// <summary>
        /// ‘true’ or 'false’	  
        /// </summary>
        [Description("is_deleted")]
        public string IsDeleted { get; set; }

        [Description("payment_method")]
        public string PaymentMethod { get; set; }

        [Description("min_date_created")]
        public DateTime? MinDateCreated { get; set; }

        [Description("max_date_created")]
        public DateTime? MaxDateCreated  { get; set; }

        [Description("min_date_modified")]
        public DateTime?  MinDateModified { get; set; }

        [Description("max_date_modified")]
        public DateTime? MaxDateModified  { get; set; }

        [Description("sort")]
        public string Sort { get; set; }
    }


}
