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
	public class FilterProducts : Filter
    {
        [Description("min:id")]
        public int? MinimumId { get; set; }

        [Description("max:id")]
        public int? MaximumId { get; set; }

        [Description("name")]
        public string Name { get; set; }

        /// <summary>
        /// Names to include, in a comma-separated list
        /// </summary>
        [Description("name:in")]
        public string NameIn { get; set; }

        [Description("sku")]
        public string SKU { get; set; }

        /// <summary>
        /// Filter items by categories. 
        /// NOTE: To ensure that your request will retrieve products that are also cross-listed in 
        /// additional categories beyond the categories  you’ve specified. 
        /// use the syntax: CategoriesIn = "295,296")
        /// </summary>
        [Description("categories:in")]
        public string CategoriesIn { get; set; }

        [Description("date_modified:min")]
        public DateTime? MinDateModified { get; set; }

        [Description("date_modified:max")]
        public DateTime? MaxDateModified { get; set; }


        [Description("date_last_imported:min")]
        public DateTime? MinDateLastImported { get; set; }

        [Description("date_last_imported:max")]
        public DateTime? MaxDateLastImported { get; set; }

        [Description("is_visible")]
        public DateTime? IsVisible { get; set; }

		[Description("type")]
		public String Type { get; set; }

        [Description("include")]
        public string Include { get; set; }
    }
}
