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
using System.Globalization;
using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    public class ScheduleProjection : IBqlTable
    {
        #region Date
        public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }

        [PXDate(IsKey = true)]
        [PXUIField(DisplayName = "Date")]
        public virtual DateTime? Date { get; set; }
        #endregion
        #region BeginDateOfWeek
        public abstract class beginDateOfWeek : PX.Data.BQL.BqlDateTime.Field<beginDateOfWeek> { }

        [PXDate]
        [PXUIField(DisplayName = "Start Date of Week")]
        public virtual DateTime? BeginDateOfWeek { get; set; }
        #endregion
        #region DayOfWeek
        public abstract class dayOfWeek : PX.Data.BQL.BqlString.Field<dayOfWeek> { }

        [PXString]
        [PXUIField(DisplayName = "Day of Week")]
        public virtual string DayOfWeek
        {
            get
            {
                //Value cannot be calculated with PXFormula attribute
                if (this.Date != null && this.Date.Value != null)
                {
                    //Adding 1 day to reuse getDayOfWeekByID function.
                    return PXMessages.LocalizeFormatNoPrefix(TX.RecurrencyFrecuency.daysOfWeek[(int)this.Date.Value.DayOfWeek]);
                }

                return null;
            }
        }
        #endregion
        #region WeekOfYear
        public abstract class weekOfYear : PX.Data.BQL.BqlInt.Field<weekOfYear> { }

        [PXInt]
        [PXUIField(DisplayName = "Week of Year")]
        public virtual int? WeekOfYear
        {
            get
            {
                //Value cannot be calculated with PXFormula attribute
                if (this.Date != null && this.Date.Value != null)
                {
                    DateTime auxDateTime = this.Date.Value;
                    /* This presumes that weeks start with Monday.
                     Week 1 is the 1st week of the year with a Thursday in it.
                     If its Monday, Tuesday or Wednesday, then it'll 
                     be the same week# as whatever Thursday, Friday or Saturday are,
                     and we always get those right */
                    DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(auxDateTime);
                    if (day >= System.DayOfWeek.Monday && day <= System.DayOfWeek.Wednesday)
                    {
                        auxDateTime = auxDateTime.AddDays(3);
                    }

                    // Return the week of our adjusted day
                    return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(auxDateTime, CalendarWeekRule.FirstFourDayWeek, System.DayOfWeek.Monday);
                }

                return null;
            }
        }
        #endregion
    }
}