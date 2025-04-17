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

using PX.Data;
using System;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Manufacturing configuration option - fixed include attribute.
    /// (Links Qty Required and Fixed Include fields based on defaults and needing a formula field)
    /// </summary>
    public class ConfigOptionFixedIncludeAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber
    {
        protected Type QtyRequiredField;

        public ConfigOptionFixedIncludeAttribute(Type qtyRequiredField)
        {
            QtyRequiredField = qtyRequiredField;
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);

            sender.Graph.FieldUpdating.AddHandler(sender.GetItemType(), QtyRequiredField.Name, QtyRequiredField_FieldUpdating);
        }

        public void QtyRequiredField_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            var row = (IConfigOption)e.Row;
            if (row == null || e.NewValue == null)
            {
                return;
            }

            if (row.FixedInclude.GetValueOrDefault() && row.InventoryID != null &&
                TryGetQtyRequiredAsFormulaValue(sender, (string)e.NewValue, out var formulaValue))
            {
                e.NewValue = formulaValue;
            }
        }

        public static bool TryGetQtyRequiredAsFormulaValue(PXCache cache, string qtyRequiredValue, out string formulaValue)
        {
            formulaValue = null;
            if (cache == null || string.IsNullOrWhiteSpace(qtyRequiredValue)
                || qtyRequiredValue.StartsWith("="))
            {
                return false;
            }
            formulaValue = $"={qtyRequiredValue.TrimStart()}";
            return true;
        }

        protected virtual void SetQtyRequiredAsFormulaValue(PXCache cache, IConfigOption row)
        {
            if (TryGetQtyRequiredAsFormulaValue(cache, row.QtyRequired, out var formulaValue))
            {
                cache.SetValueExt(row, QtyRequiredField.Name, formulaValue);
            }
        }

        public void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (IConfigOption)e.Row;
            if (row == null || row.InventoryID == null || !row.FixedInclude.GetValueOrDefault())
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(row.QtyRequired))
            {
                // Default value of 1
                sender.SetValueExt(row, QtyRequiredField.Name, "1");
                return;
            }

            SetQtyRequiredAsFormulaValue(sender, row);
        }
    }
}