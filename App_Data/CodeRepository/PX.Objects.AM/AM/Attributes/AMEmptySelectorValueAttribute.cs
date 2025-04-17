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

namespace PX.Objects.AM.Attributes
{
    public class AMEmptySelectorValueAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber
    {
        private bool _OverrideDefautValue;
        private string _DisplayValue;

        public AMEmptySelectorValueAttribute(string displayValue)
        {
            _OverrideDefautValue = true;
            _DisplayValue = displayValue;
        }
        public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            if (_OverrideDefautValue && sender.GetValue(e.Row, this.FieldOrdinal) == null)
            {
                e.ReturnState = PXStringState.CreateInstance(e.ReturnState, null, null, this.FieldName, null, null, string.Empty, null, null, null, null);
                e.ReturnValue = _DisplayValue;
            }
        }

        public void OverrideDefaultValue(bool overrideDefaultValue)
        {
            _OverrideDefautValue = overrideDefaultValue;
        }

        public static void OverrideDefaultValue<TField>(PXCache cache, object row, bool overrideDefaultValue = true)
                where TField : IBqlField
        {
            foreach (var attribute in cache.GetAttributes<TField>(row))
            {
                if (attribute is AMEmptySelectorValueAttribute)
                {
                    var emptySelectorValueAttribute = (AMEmptySelectorValueAttribute)attribute;
                    emptySelectorValueAttribute.OverrideDefaultValue(overrideDefaultValue);
                }
            }
        }
    }
}
