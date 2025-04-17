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
using PX.Objects.IN;
using System;

namespace PX.Objects.FS
{
    // This is to avoid trying to calculate the BaseQuantity value for comment and instruction lines.
    public class FSDBQuantityAttribute : PXDBQuantityAttribute
    {
        public FSDBQuantityAttribute(Type keyField, Type resultField) : base(keyField, resultField)
        {
        }

        public FSDBQuantityAttribute(Type keyField, Type resultField, InventoryUnitType decimalVerifyUnits) : base(keyField, resultField, decimalVerifyUnits)
        {
        }

        protected override void CalcBaseQty(PXCache sender, QtyConversionArgs e)
        {
            if (FSDBQuantityAttribute.IsAnItemLine(sender, e.Row) == true)
            {
                base.CalcBaseQty(sender, e);
            }
            else
            {
                if (_ResultField != null)
                {
                    sender.SetValue(e.Row, _ResultField.Name, 0m);
                }
            }
        }

        public static bool IsAnItemLine(PXCache sender, object row)
        {
            string searchName = typeof(FSAppointmentDet.lineType).Name.ToLower();
            string lineTypeFieldName = string.Empty;

            foreach (string field in sender.Fields)
            {
                if (field.ToLower() == searchName)
                {
                    lineTypeFieldName = field;
                    break;
                }
            }

            if (lineTypeFieldName != string.Empty)
            {
                object lineTypeValue = sender.GetValue(row, lineTypeFieldName);

                if (lineTypeValue != null)
                {
                    string strLineType = (string)lineTypeValue;

                    if (strLineType == ID.LineType_ALL.INSTRUCTION || strLineType == ID.LineType_ALL.COMMENT)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }

    // This is to avoid trying to calculate the BaseQuantity value for comment and instruction lines.
    public class FSQuantityAttribute : PXQuantityAttribute
    {
        public FSQuantityAttribute(Type keyField, Type resultField) : base(keyField, resultField)
        {
        }

        protected override void CalcBaseQty(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            if (FSDBQuantityAttribute.IsAnItemLine(sender, e.Row) == true)
            {
                base.CalcBaseQty(sender, e);
            }
            else
            {
                if (_ResultField != null)
                {
                    sender.SetValue(e.Row, _ResultField.Name, 0m);
                }
            }
        }
    }
}
