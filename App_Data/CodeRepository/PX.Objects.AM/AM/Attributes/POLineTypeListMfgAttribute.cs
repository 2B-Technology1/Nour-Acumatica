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
using PX.Data;
using PX.Objects.PO;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Extension to add new PO Lines types for manufacturing
    /// </summary>
    public class POLineTypeListMfgAttribute : POLineTypeList2Attribute
    {
        public POLineTypeListMfgAttribute(Type docType, Type inventoryID) : base(docType, inventoryID)
        {
        }

        public override void CacheAttached(PXCache sender)
        {
            // Add 2 new options to the base list of POLineTypes
            base.CacheAttached(sender);
            string[] values = new string[_AllowedValues.Length + 2];
            string[] labels = new string[values.Length];
            _AllowedValues.CopyTo(values, 0);
            _AllowedLabels.CopyTo(labels, 0);
            values[values.Length - 1] = POLineType.GoodsForManufacturing; /* Goods for MFG */
            labels[labels.Length - 1] = AM.Messages.GoodsForManufacturing;
            values[values.Length - 2] = POLineType.NonStockForManufacturing; /* Non-Stock for MFG */
            labels[labels.Length - 2] = AM.Messages.NonStockForManufacturing;

            _AllowedValues = values;
            _AllowedLabels = labels;
        }
    }
}
