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
    public class ClockTranScrapActionDefaultAttribute : PXDefaultAttribute
    {
        public ClockTranScrapActionDefaultAttribute(Type sourceType)
            : base(AM.Attributes.ScrapAction.NoAction, sourceType)
        {
        }

        public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            base.FieldDefaulting(sender, e);

            if ((int?)e.NewValue == AM.Attributes.ScrapAction.Quarantine)
            {
                e.NewValue = AM.Attributes.ScrapAction.WriteOff;
            }
        }

        public class ClockTranListAttribute : PXIntListAttribute
        {
            public ClockTranListAttribute()
                : base(
                new int[] { AM.Attributes.ScrapAction.NoAction, AM.Attributes.ScrapAction.WriteOff },
                new string[] { Messages.NoAction, Messages.WriteOff })
            { }
        }


    }
}