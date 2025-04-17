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

namespace PX.Objects.FS.Scheduler
{
    /// <summary>
    /// This class specifies the structure for a Monthly Schedule.
    /// </summary>
    public abstract class MonthlySchedule : RepeatingSchedule
    {
        /// <summary>
        /// Handles if the rule applies in the [date] using the Frequency of the Schedule.
        /// </summary>
        public override bool OccursOnDate(DateTime date)
        {
            if (DateIsInPeriodAndIsANewDate(date) && IsOnCorrectDate(date))
            {
                int monthsBetweenLastAndCheckDate = ((date.Year - StartOrLastDate.Year) * 12) + date.Month - StartOrLastDate.Month;

                return monthsBetweenLastAndCheckDate % Frequency == 0;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Method to be implemented in child classes. Handles if the rule applies in the [date] depending of the monthly Schedule type.
        /// </summary>
        public abstract bool IsOnCorrectDate(DateTime date);
    }
}
