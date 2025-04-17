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

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing configuration option
    /// </summary>
    public interface IConfigOption
    {
        string Label { get; set; }
        int? InventoryID { get; set; }
        int? SubItemID { get; set; }
        string Descr { get; set; }
        bool? FixedInclude { get; set; }
        bool? QtyEnabled { get; set; }
        string QtyRequired { get; set; }
        string UOM { get; set; }
        string MinQty { get; set; }
        string MaxQty { get; set; }
        string LotQty { get; set; }
        string ScrapFactor { get; set; }
        string PriceFactor { get; set; }
        bool? ResultsCopy { get; set; }
    }
}