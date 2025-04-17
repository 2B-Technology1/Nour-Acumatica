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

namespace PX.Objects.AR.CCPaymentProcessing
{
    public class ExpirationDateAttribute : PXEventSubscriberAttribute, IPXFieldUpdatingSubscriber
    {
        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);

            sender.Graph.FieldSelecting.AddHandler(sender.GetItemType(),
                _FieldName, FieldSelectingHandler);
        }

        public void FieldSelectingHandler(PXCache sender, PXFieldSelectingEventArgs e)
        {
            DateTime? dt = e.ReturnValue as DateTime?;
            if (dt != null)
            {
                e.ReturnValue = dt.Value.AddMonths(-1);
            }
        }

		public void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
            DateTime? dt = e.NewValue as DateTime?;
            if (dt == null && e.NewValue != null)
            {
				if (DateTime.TryParseExact(e.NewValue.ToString(), "MM/yy", null, System.Globalization.DateTimeStyles.None, out DateTime result))
					dt = result;
            }
			if (dt != null)
			{
				e.NewValue = dt.Value.AddMonths(1);
			}
        }
	}
}
