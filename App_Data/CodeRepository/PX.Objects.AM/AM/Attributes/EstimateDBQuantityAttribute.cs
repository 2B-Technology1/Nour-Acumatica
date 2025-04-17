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
using PX.Objects.IN;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Handle non inventory with UOM conversion issues for the estimate module
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class)]
    public class EstimateDBQuantityAttribute : PXDBQuantityAttribute
    {
        protected Type _InventoryIdField = null;

        public EstimateDBQuantityAttribute(Type keyField, Type resultField, Type inventoryIdField) : base(keyField, resultField)
        {
            _InventoryIdField = inventoryIdField;
        }

        protected override void CalcBaseQty(PXCache sender, QtyConversionArgs e)
        {
            var inventoryId = sender.GetValue(e.Row, _InventoryIdField.Name);

            if (inventoryId == null && e.NewValue != null)
            {
                if (e.ExternalCall)
                {
                    sender.SetValueExt(e.Row, this._ResultField.Name, (decimal)e.NewValue);
                }
                else
                {
                    sender.SetValue(e.Row, this._ResultField.Name, (decimal)e.NewValue);
                }

                return;
            }

            base.CalcBaseQty(sender, e);
        }
    }
}