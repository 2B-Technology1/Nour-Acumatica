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
using System.Text.RegularExpressions;

namespace PX.Objects.FS
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter)]

    // PXEventSubscriberAttribute: Extends the BLC events associated to a DAC attribute which have the [NormalizeWhiteSpace] attribute.
    // IPXFieldUpdatingSubscriber: Allows to overwrite the FieldUpdating event inside the [NormalizeWhiteSpace] class.
    public class NormalizeWhiteSpace : PXEventSubscriberAttribute, IPXFieldUpdatingSubscriber
    {
        public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            if (e.NewValue != null && e.NewValue is string && !e.Cancel)
            {
                string strValue = (string)e.NewValue;
                int fieldLengthBeforeNormalize = strValue.Length;
                e.NewValue = Regex.Replace(strValue.Trim(), @"\s+", " ").PadRight(fieldLengthBeforeNormalize, ' ');
            }
        }
    }
}
