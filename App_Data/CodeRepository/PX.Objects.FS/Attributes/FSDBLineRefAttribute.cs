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
using PX.Objects.PM;
using PX.Data;

namespace PX.Objects.FS
{
    public class FSDBLineRefAttribute : PXEventSubscriberAttribute, IPXRowInsertingSubscriber
    {
        private Type _lineNbr;

        public FSDBLineRefAttribute(Type lineNbr)
        {
            _lineNbr = lineNbr;
        }

        public void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            object lineNbr = sender.GetValue(e.Row, _lineNbr.Name);
            object lineRef = sender.GetValue(e.Row, _FieldName);
            int length = -1;

            if (lineRef == null)
            {
                foreach (PXEventSubscriberAttribute attribute in sender.GetAttributes(_FieldName))
                {
                    if (attribute is PXDBStringAttribute)
                    {
                        length = ((PXDBStringAttribute)attribute).Length;
                        break;
                    }
                }

                if(length > 0 && (int?)lineNbr > 0)
                    sender.SetValue(e.Row, _FieldName, ((int?)lineNbr).Value.ToString().PadLeft(length, '0'));
            } 
        }
    }
}
