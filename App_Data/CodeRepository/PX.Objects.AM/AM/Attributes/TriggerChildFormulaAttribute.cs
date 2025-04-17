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

namespace PX.Objects.AM.Attributes
{
    public class TriggerChildFormulaAttribute : UpdateChildOnFieldUpdatedBaseAttribute
    {
        /// <param name="childType">Type of child row</param>
        /// <param name="childUpdateField">Field in child formula used to trigger formula</param>
        public TriggerChildFormulaAttribute(Type childType, Type childUpdateField) : base(childType, childUpdateField)
        {
        }

        protected virtual void RaiseChildFieldUpdated(object childRow, object newValue)
        {
            ChildCache?.RaiseFieldUpdated(_childUpdateField.Name, childRow, newValue);
        }

        public override void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            foreach (var child in SelectChildren( e.Row))
            {
                RaiseChildFieldUpdated(child, GetChildValue(child));
                UpdateChildRow(child);
            }
        }
    }
}