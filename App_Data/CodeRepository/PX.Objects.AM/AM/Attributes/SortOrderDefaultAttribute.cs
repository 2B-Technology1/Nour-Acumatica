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
    /// <summary>
    /// DAC Field level attribute for defaulting the sort order field by a multiplier from the Line Nbr field value
    /// </summary>
    public class SortOrderDefaultAttribute : PXEventSubscriberAttribute, IPXFieldDefaultingSubscriber
    {
        protected readonly Type LineNbrField;
        protected readonly int StepMultiplier;
        public const int DEFAULTSTEPMULT = 10;

        public SortOrderDefaultAttribute(Type lineNbrField) 
            : this(lineNbrField, DEFAULTSTEPMULT)
        {
        }

        public SortOrderDefaultAttribute(Type lineNbrField, int stepMultiplier)
        {
            LineNbrField = lineNbrField;
            StepMultiplier = stepMultiplier;

            if (stepMultiplier <= 0)
            {
                StepMultiplier = 1;
            }
        }

        protected virtual int GetLineNbrValue(PXCache cache, object row)
        {
            return row == null ? 0 : (int?)cache.GetValue(row, LineNbrField.Name) ?? 0;
        }

        public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = GetLineNbrValue(sender, e.Row) * StepMultiplier;
        }
    }
}