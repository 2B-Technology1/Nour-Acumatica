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

using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Data;

namespace PX.Objects.PJ.ProjectManagement.PJ.Descriptor.Attributes
{
    public class AnswerDaysCalculationTypeAttribute : PXStringListAttribute, IPXFieldUpdatedSubscriber
    {
        public const string SequentialDays = "S";
        public const string BusinessDays = "B";
        public const string SequentialDaysLabel = "Sequential Days (incl. weekends)";
        public const string BusinessDaysLabel = "Business Days";

        public AnswerDaysCalculationTypeAttribute()
            : base(new[]
            {
                SequentialDays,
                BusinessDays
            }, new[]
            {
                SequentialDaysLabel,
                BusinessDaysLabel
            })
        {
        }

        public void FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs args)
        {
            if (args.Row is ProjectManagementSetup projectManagementSetup)
            {
                projectManagementSetup.CalendarId = null;
            }
        }
    }
}
