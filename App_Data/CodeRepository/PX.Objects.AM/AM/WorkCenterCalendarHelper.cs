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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PX.Objects.AM
{
	/// <summary>
	/// Helps to set workcenter shift value
	/// </summary>
    [DebuggerDisplay("WorkCenter={WorkCenter.WcID}, NbrOfShifts={Shifts.Count}")]
    public class WorkCenterCalendarHelper
    {
        public AMWC WorkCenter { get; }
        protected List<AMShift> Shifts { get; set; } 
        protected PXGraph Graph { get; }
        protected Dictionary<string, CalendarHelper> ShiftCalendars { get; set; }
        protected bool IncludeBreakTimeDetail { get; set; }


        public WorkCenterCalendarHelper(PXGraph graph, AMWC workCenter) : this(graph, workCenter, false)
        {
        }

        public WorkCenterCalendarHelper(PXGraph graph, AMWC workCenter, bool includeBreakTimeDetail)
        {
            if (workCenter == null || string.IsNullOrWhiteSpace(workCenter.WcID))
            {
                throw new PXArgumentException(nameof(workCenter));
            }

            Graph = graph ?? throw new PXArgumentException(nameof(graph));
            WorkCenter = workCenter;
            IncludeBreakTimeDetail = includeBreakTimeDetail;
            ShiftCalendars = new Dictionary<string, CalendarHelper>();
            Shifts = new List<AMShift>();
            SetWorkCenterShifts();

            if (Shifts.Count == 0)
            {
                throw new PXException(Messages.WorkCenterMustHaveOneShift, WorkCenter.WcID.TrimIfNotNullEmpty());
            }
        }

        public virtual AMShift GetFirstShift()
        {
            return Shifts.FirstOrDefault();
        }

        public virtual List<AMShift> GetShifts(ReadDirection schdReadDirection)
        {
            //shifts are stored in the list via ascending order (read direction forward)
            if (schdReadDirection == ReadDirection.Forward)
            {
                return Shifts;
            }

            var reverseShifts = new List<AMShift>(Shifts);
            reverseShifts.Reverse();
            return reverseShifts;
        }

        protected void SetWorkCenterShifts()
        {
            foreach (AMShift shift in PXSelect<AMShift, 
                Where<AMShift.wcID, Equal<Required<AMShift.wcID>>>,
                OrderBy<Asc<AMShift.shiftCD>>>.Select(Graph, WorkCenter.WcID))
            {
                if (string.IsNullOrWhiteSpace(shift.ShiftCD) || string.IsNullOrWhiteSpace(shift.CalendarID))
                {
                    continue;
                }

                Shifts.Add(shift);
                ShiftCalendars.Add(shift.ShiftCD.TrimIfNotNull(), new CalendarHelper(Graph, shift.CalendarID) {IncludeBreakTimeDetail = this.IncludeBreakTimeDetail});
            }
        }

        /// <summary>
        /// Calculate the current work center's working hours based on shift calendars
        /// </summary>
        /// <param name="date">Date of work center calendar</param>
        /// <returns>Combined/calculated date info from all shift calendars</returns>
        public virtual WorkCenterShiftDateInfo GetWorkingHours(DateTime date)
        {
            var shiftDateInfos = new Dictionary<string, AMDateInfo>();
            foreach (var amShift in Shifts)
            {
                shiftDateInfos.Add(amShift.ShiftCD, GetShiftDateInfo(amShift, date));
            }

            if (shiftDateInfos.Count == 0)
            {
                //making a non working day wc date info
                var d = new Dictionary<string, AMDateInfo>();
                foreach (var amShift in Shifts)
                {
                    d.Add(amShift.ShiftCD, CalendarHelper.MakeNonWorkingDateInfo(date));
                }
                return new WorkCenterShiftDateInfo(WorkCenter, CalendarHelper.MakeNonWorkingDateInfo(date), d);
            }
            
            var combinedDateInfo = shiftDateInfos[Shifts[0].ShiftCD];
            for (var i = 1; i < shiftDateInfos.Count; i++)
            {
                combinedDateInfo = AMDateInfo.AddDateInfo(combinedDateInfo, shiftDateInfos[Shifts[i].ShiftCD]);
            }

            return new WorkCenterShiftDateInfo(WorkCenter, combinedDateInfo, shiftDateInfos); ;
        }

        protected virtual AMDateInfo GetShiftDateInfo(AMShift shift, DateTime date)
        {
            if (shift?.ShiftCD == null)
            {
                throw new ArgumentNullException(nameof(shift));
            }

            if (ShiftCalendars.ContainsKey(shift.ShiftCD.TrimIfNotNullEmpty()))
            {
                return ShiftCalendars[shift.ShiftCD.TrimIfNotNullEmpty()].GetDateInfo(date);
            }

            throw new PXException($"Shift {shift?.ShiftCD.TrimIfNotNullEmpty()} on work center {shift?.WcID.TrimIfNotNullEmpty()} not found in the dictionary");
        }

        public virtual DateTime? GetNextWorkDay(DateTime dateTime, ReadDirection readDirection)
        {
            return GetNextWorkDay(dateTime, readDirection, false);
        }

        public virtual DateTime? GetNextWorkDay(DateTime dateTime, ReadDirection readDirection, bool includeCurrentDate)
        {
            SetShiftCalendarReadDirection(readDirection);
            var dateCheck = includeCurrentDate ? dateTime : NextDateTime(dateTime, readDirection);
            return GetNextShiftWorkDay(dateCheck, readDirection, true);
        }

        protected virtual DateTime? GetNextShiftWorkDay(DateTime dateTime, ReadDirection readDirection, bool includeCurrentDate)
        {
            DateTime? bestNextDate = null;
            foreach (var calendarHelper in ShiftCalendars)
            {
                bestNextDate = CalendarHelper.GreaterDate(readDirection, calendarHelper.Value.GetNextWorkDay(dateTime, includeCurrentDate), bestNextDate);
            }
            return bestNextDate;
        }

        protected virtual void SetShiftCalendarReadDirection(ReadDirection readDirection)
        {
            var newShiftCalendars = new Dictionary<string, CalendarHelper>();
            foreach (var calendarHelper in ShiftCalendars)
            {
                var ch = calendarHelper.Value;
                //avoids the need to recache the next exception if the direction did not change
                if (ch.CalendarReadDirection != readDirection)
                {
                    ch.CalendarReadDirection = readDirection;
                }
                newShiftCalendars.Add(calendarHelper.Key, ch);
            }
            ShiftCalendars = newShiftCalendars;
        }

        protected virtual DateTime AddDateTimeDays(DateTime dateTime, int days)
        {
            try
            {
                return dateTime.AddDays(days);
            }
            catch (ArgumentOutOfRangeException)
            {
                PXTrace.WriteError($"Unable to add '{days}' days to date {dateTime.ToShortDateString()}");

                throw;
            }
        }

        public virtual DateTime NextDateTime(DateTime dateTime, ReadDirection readDirection)
        {
            return AddDateTimeDays(dateTime, readDirection == ReadDirection.Forward ? 1 : -1);
        }

    }

    [DebuggerDisplay("WorkCenter={WorkCenter.WcID}, Date={WorkCenterDateInfo.Date.ToShortDateString()}, NbrOfShifts={ShiftDateInfos.Count}")]
    public struct WorkCenterShiftDateInfo
    {
        public readonly AMWC WorkCenter;
        public readonly AMDateInfo WorkCenterDateInfo;
        public readonly Dictionary<string, AMDateInfo> ShiftDateInfos;

        public WorkCenterShiftDateInfo(AMWC workCenter, AMDateInfo workCenterDateInfo, Dictionary<string, AMDateInfo> shiftDateInfos)
        {
            if (workCenter == null || string.IsNullOrWhiteSpace(workCenter.WcID))
            {
                throw new PXArgumentException("workCenter");
            }
            WorkCenter = workCenter;
            WorkCenterDateInfo = workCenterDateInfo;

            if (shiftDateInfos == null || shiftDateInfos.Count == 0)
            {
                throw new PXArgumentException("shiftDateInfos");
            }

            ShiftDateInfos = shiftDateInfos;
        }
    }
}
