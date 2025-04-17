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
    /// This class specifies the structure for a Monthly Schedule in a specific day of the month.
    /// </summary>
    public class MonthlyScheduleSpecificDay : MonthlySchedule
    {
        /// <summary>
        /// Gets or sets the number of the specific day of the month.
        /// </summary>
        public int DayOfMonth { get; set; }

        /// <summary>
        /// Handles if the rule applies in the specific [date] using the [DayOfMonth]. It will return the last day if the [DayOfMonth] is incorrect for that month.
        /// </summary>
        public override bool IsOnCorrectDate(DateTime date)
        {
            if (date.Day == DayOfMonth)
            {
                return true;
            }
            else if (date.Day == DateTime.DaysInMonth(date.Year, date.Month)
                              && DayOfMonth > date.Day)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
