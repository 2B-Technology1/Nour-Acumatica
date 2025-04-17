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
using PX.Objects.FS;

namespace PX.Objects.FS.Scheduler
{
    /// <summary>
    /// This class specifies the structure for a Monthly Schedule in a specific weekday of the month.
    /// </summary>
    public class AnnualScheduleWeekDay : AnnualSchedule
    {
        /// <summary>
        /// Gets or sets attribute to specify the number of the week in the month.
        /// </summary>
        public short MonthlyOnWeek { get; set; }

        /// <summary>
        /// Gets or sets attribute to specify the day of the week in which applies the Schedule.
        /// </summary>
        public DayOfWeek MonthlyOnDayOfWeek { get; set; }

        /// <summary>
        /// Validates if the [date] matches with the [MonthlyOnWeek] and [MonthlyOnDayOfWeek] specified in the Schedule.
        /// </summary>
        public override bool IsOnCorrectDate(DateTime date)
        {
            if (months.Contains(SharedFunctions.getMonthOfYearByID(date.Month)))
            {
                DateTime checkDate = new DateTime(date.Year, date.Month, 1);

                // find the first <DayOfWeek>
                while (checkDate.DayOfWeek != MonthlyOnDayOfWeek)
                {
                    checkDate = checkDate.AddDays(1);
                }

                // add the corresponding weeks specified in <MonthlyOnWeek>
                int dayOfMonth = checkDate.Day + ((MonthlyOnWeek - 1) * 7);
                if (dayOfMonth > DateTime.DaysInMonth(checkDate.Year, checkDate.Month))
                {
                    dayOfMonth -= 7;
                }

                if (date.Day == dayOfMonth)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }
    }
}
