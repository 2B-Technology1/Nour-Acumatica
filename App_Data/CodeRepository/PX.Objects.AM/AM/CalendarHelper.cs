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
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AM
{
    /// <summary>
    /// Defines the direction the calendar should be read - forward (1/1 --> 12/31) or backwards(12/31 --> 1/1)
    /// </summary>
    public enum ReadDirection
    {
        Forward,
        Backward
    }

    public class CalendarHelper : ICalendarHelper
    {
        /// <summary>
        /// Empty date time as it would relate to a calendar day start/end time
        /// </summary>
        public static DateTime EmptyTime
        {
            get
            {
                return new DateTime(1900, 1, 1, 0, 0, 0);
            }
        } 

        private PXGraph _graph;
        private CSCalendar _csCalendar;
        private CSCalendarExceptions _nextCSCalendarException;
        private bool _forceReCache;
        private DateTime _nullDateDefault;
        private ReadDirection _calendarReadDirection;
        private bool _includeBreakTimeDetail;
        private List<AMCalendarBreakTime> _calendarBreakTimeList;

        public static ReadDirection ChangeDirection(ReadDirection readDirection)
        {
            return readDirection == ReadDirection.Forward ? ReadDirection.Backward : ReadDirection.Forward;
        }

        public static DateTime? GreaterDate(ReadDirection readDirection, DateTime? date1, DateTime? date2)
        {
            if (date1 != null && date2 == null)
            {
                return date1;
            }
            if (date1 == null && date2 != null)
            {
                return date2;
            }

            if (date1 == null && date2 == null)
            {
                return null;
            }

            var results = date1.CompareNullDates(date2);

            if (results == 0)
            {
                return date1;
            }

            if (results < 0 && readDirection == ReadDirection.Backward)
            {
                return date1;
            }
            if (results > 0 && readDirection == ReadDirection.Forward)
            {
                return date1;
            }

            return date2;
        }

        public static DateTime? LessDate(ReadDirection readDirection, DateTime? date1, DateTime? date2)
        {
            if (date1 != null && date2 == null)
            {
                return date1;
            }
            if (date1 == null && date2 != null)
            {
                return date2;
            }

            if (date1 == null && date2 == null)
            {
                return null;
            }

            var results = date1.CompareNullDates(date2);

            if (results == 0)
            {
                return date1;
            }

            if (results < 0 && readDirection == ReadDirection.Forward)
            {
                return date1;
            }
            if (results > 0 && readDirection == ReadDirection.Backward)
            {
                return date1;
            }

            return date2;
        }

        public string CurrentCalendarId
        {
            get
            {
                if (_csCalendar != null)
                {
                    return _csCalendar.CalendarID ?? string.Empty;
                }
                return string.Empty;
            }
        }

        public ReadDirection CalendarReadDirection
        {
            get
            {
                return _calendarReadDirection;
            }
            set
            {
                _forceReCache = true;
                _calendarReadDirection = value;

                _nullDateDefault = Common.Dates.BeginOfTimeDate;
                if (_calendarReadDirection == ReadDirection.Backward)
                {
                    _nullDateDefault = Common.Dates.EndOfTimeDate;
                }
            }
        }

        public bool IncludeBreakTimeDetail
        {
            get { return _includeBreakTimeDetail; }
            set { _includeBreakTimeDetail = value; }
        }

        public List<AMCalendarBreakTime> CalendarBreakTimeList
        {
            get
            {
                if (!_includeBreakTimeDetail)
                {
                    return new List<AMCalendarBreakTime>();
                }

                if (_calendarBreakTimeList == null)
                {
                    LoadCalendarBreakTimes();
                }
                
                return _calendarBreakTimeList;
            }
        }

        protected virtual void LoadCalendarBreakTimes()
        {
            _calendarBreakTimeList = PXSelect<AMCalendarBreakTime,
                Where<AMCalendarBreakTime.calendarID, Equal<Required<AMCalendarBreakTime.calendarID>>>
            >.Select(_graph, _csCalendar.CalendarID).FirstTableItems.ToList();
        }

        public static AMDateInfo MakeNonWorkingDateInfo(DateTime date)
        {
            return new AMDateInfo(date, false, CalendarHelper.EmptyTime, CalendarHelper.EmptyTime, 0);
        }

		public CalendarHelper(PXGraph graph)
		{
			_graph = graph;
		}

        public CalendarHelper(PXGraph graph, string calendar)
            : this(graph, SelectCalendar(graph, calendar))
        {
            if (string.IsNullOrWhiteSpace(calendar))
            {
                throw new PXException(Messages.MissingCalendarID);
            }
        }

        public CalendarHelper(PXGraph graph, CSCalendar calendar) 
        {
            _graph = null;
            _csCalendar = null;
            _includeBreakTimeDetail = false;

            if (graph == null)
            {
                throw new ArgumentNullException(nameof(graph));
            }

            if (calendar == null)
            {
                throw new InvalidWorkCalendarException();
            }

            _graph = graph;
            _csCalendar = calendar;

            CalendarReadDirection = ReadDirection.Forward;

            _nextCSCalendarException = new CSCalendarExceptions()
            {
                CalendarID = _csCalendar.CalendarID
            };
        }

        ///// <summary>
        ///// Retrieve the standard day of week work times for the current calendar
        ///// (does not look at exceptions)
        ///// </summary>
        //public virtual AMDateInfo GetCalendarDateInfo(DayOfWeek dayOfWeek)
        //{
        //    var genericDateTime = MakeGeneralDate(dayOfWeek);

        //    switch (dayOfWeek)
        //    {
        //        case DayOfWeek.Monday:
        //            return new AMDateInfo(genericDateTime, _csCalendar.MonWorkDay.GetValueOrDefault(), _csCalendar.MonStartTime, _csCalendar.MonEndTime, _csCalendar.MonUnpaidTime.GetValueOrDefault());
        //        case DayOfWeek.Tuesday:
        //            return new AMDateInfo(genericDateTime, _csCalendar.TueWorkDay.GetValueOrDefault(), _csCalendar.TueStartTime, _csCalendar.TueEndTime, _csCalendar.TueUnpaidTime.GetValueOrDefault());
        //        case DayOfWeek.Wednesday:
        //            return new AMDateInfo(genericDateTime, _csCalendar.WedWorkDay.GetValueOrDefault(), _csCalendar.WedStartTime, _csCalendar.WedEndTime, _csCalendar.WedUnpaidTime.GetValueOrDefault());
        //        case DayOfWeek.Thursday:
        //            return new AMDateInfo(genericDateTime, _csCalendar.ThuWorkDay.GetValueOrDefault(), _csCalendar.ThuStartTime, _csCalendar.ThuEndTime, _csCalendar.ThuUnpaidTime.GetValueOrDefault());
        //        case DayOfWeek.Friday:
        //            return new AMDateInfo(genericDateTime, _csCalendar.FriWorkDay.GetValueOrDefault(), _csCalendar.FriStartTime, _csCalendar.FriEndTime, _csCalendar.FriUnpaidTime.GetValueOrDefault());
        //        case DayOfWeek.Saturday:
        //            return new AMDateInfo(genericDateTime, _csCalendar.SatWorkDay.GetValueOrDefault(), _csCalendar.SatStartTime, _csCalendar.SatEndTime, _csCalendar.SatUnpaidTime.GetValueOrDefault());
        //        default:
        //            //treated as Sunday
        //            return new AMDateInfo(genericDateTime, _csCalendar.SunWorkDay.GetValueOrDefault(), _csCalendar.SunStartTime, _csCalendar.SunEndTime, _csCalendar.SunUnpaidTime.GetValueOrDefault());
        //    }
        //}

        protected virtual DateTime MakeGeneralDate(DayOfWeek dayOfWeek)
        {
            //1/1/1900 = Monday
            int addDays = (int)dayOfWeek - 1;

            if (addDays < 0 || addDays > 7)
            {
                addDays = 7;
            }

            return Common.Dates.BeginOfTimeDate.AddDays(addDays);
        }

        /// <summary>
        /// Get the day of week as an abbreviated 3 character long description.
        /// Example: DayOfWeek.Sunday = "Sun"
        /// </summary>
        public static string DayOfWeekAbbreviation(DayOfWeek dayOfWeek)
        {
            var fullDayOfWeek = DayOfWeekFullName(dayOfWeek);
            return fullDayOfWeek.Substring(0, fullDayOfWeek.Length < 3 ? fullDayOfWeek.Length : 3);
        }

        /// <summary>
        /// Get the day of week as the days normal full description.
        /// Example: DayOfWeek.Sunday = "Sunday"
        /// </summary>
        public static string DayOfWeekFullName(DayOfWeek dayOfWeek)
        {
            return Enum.GetName(typeof(DayOfWeek), dayOfWeek);
        }

        /// <summary>
        /// Find the next exception date record from the given date.
        /// </summary>
        /// <param name="dateTime">the date to check after for exceptions (optional to return first exception)</param>
        protected virtual CSCalendarExceptions GetNextExceptionDate(DateTime? dateTime = null)
        {
            if (_csCalendar == null || string.IsNullOrWhiteSpace(_csCalendar.CalendarID))
            {
                return null;
            }

            DateTime checkDateTime = dateTime ?? _nullDateDefault;

            if(_calendarReadDirection == ReadDirection.Forward)
            {
                _nextCSCalendarException = PXSelect<CSCalendarExceptions, 
                    Where<CSCalendarExceptions.calendarID, Equal<Required<CSCalendarExceptions.calendarID>>,
                        And<CSCalendarExceptions.date, GreaterEqual<Required<CSCalendarExceptions.date>>>>,
                            OrderBy<Asc<CSCalendarExceptions.date>>
                    >.Select(_graph, _csCalendar.CalendarID, checkDateTime);
            }
            else
            {
                _nextCSCalendarException = PXSelect<CSCalendarExceptions,
                    Where<CSCalendarExceptions.calendarID, Equal<Required<CSCalendarExceptions.calendarID>>,
                        And<CSCalendarExceptions.date, LessEqual<Required<CSCalendarExceptions.date>>>>,
                            OrderBy<Desc<CSCalendarExceptions.date>>
                    >.Select(_graph, _csCalendar.CalendarID, checkDateTime);
            }

            _forceReCache = false;

            return _nextCSCalendarException;
        }

        /// <summary>
        /// Is the given date >= current exception date
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        protected virtual bool CheckForNextDateException(DateTime dateTime)
        {
            if (_forceReCache)
            {
                return true;
            }

            if (_nextCSCalendarException == null)
            {
                return false;
            }

            DateTime exDateTime = _nextCSCalendarException.Date ?? _nullDateDefault;

            int result = DateTime.Compare(dateTime, exDateTime);

            if (CalendarReadDirection == ReadDirection.Forward)
            {
                return result > 0;
            }

            return result < 0;
        }

		/// <summary>
		/// When switching forward and backward with dates it is required that the next calendar exception is reset to correctly process exceptions
		/// </summary>
		public virtual void ResetCalendarException()
		{
			_forceReCache = true;
		}

        /// <summary>
        /// Make sure the calendar exception date is cached
        /// </summary>
        /// <param name="dateTime">Current calendar date being reviewed</param>
        public virtual void CacheNextException(DateTime dateTime)
        {
            if (CheckForNextDateException(dateTime))
            {
                GetNextExceptionDate(dateTime);
            }
        }

        /// <summary>
        /// Get the total minutes of break time from calendar break details
        /// </summary>
        /// <returns>Break time in default minutes</returns>
        public static int GetBreakTimeMinutes(DateTime dateTime, List<AMCalendarBreakTime> breakTimeDetail)
        {
            return breakTimeDetail == null || breakTimeDetail.Count == 0
                ? 0
                : Convert.ToInt32(
                    GetTotalBreakTimeForDayOfWeek(dateTime.DayOfWeek, breakTimeDetail).GetValueOrDefault());
        }

        public static AMCalendarBreakTime Copy(AMCalendarBreakTime source)
        {
            return new AMCalendarBreakTime
            {
                CalendarID = source?.CalendarID,
                DayOfWeek = source?.DayOfWeek,
                StartTime = source?.StartTime,
                EndTime = source?.EndTime,
                Description = source?.Description
            };
        }

        /// <summary>
        /// Adjusts/orders the passed in break time detail to only account for the times between the start and end times
        /// </summary>
        public static List<AMCalendarBreakTime> AdjustBreakTimes(List<AMCalendarBreakTime> breakTimeList, DateTime startTime, DateTime endTime)
        {
            var newList = new List<AMCalendarBreakTime>();
			if(breakTimeList == null)
			{
				return newList;
			}

            DateTime dateStart = EmptyTime.Date + startTime.TimeOfDay;
            DateTime dateEnd = EmptyTime.Date + endTime.TimeOfDay;

			var isOvernight = dateEnd.CompareTo(dateStart) < 0;
            if (isOvernight)
            {
                //this should be an overnight shift
                dateEnd = dateEnd.AddDays(1);
            }

			var breakTimeSort = isOvernight
				? BreakTimeOvernightSort(breakTimeList)
				: breakTimeList.OrderBy(x => x.StartTime);
            foreach (var breakTime in breakTimeSort)
            {
                var newBreakTime = Copy(breakTime);

                if (newBreakTime.StartTime == null || newBreakTime.EndTime == null)
                {
                    continue;
                }

				DateTime breakStart = EmptyTime.Date + newBreakTime.StartTime.GetValueOrDefault().TimeOfDay;
				DateTime breakEnd = EmptyTime.Date + newBreakTime.EndTime.GetValueOrDefault().TimeOfDay;

				if(isOvernight && breakStart.TimeOfDay.TotalHours < 12)
				{
					breakStart = breakStart.AddDays(1);
				}
				if(isOvernight && breakEnd.TimeOfDay.TotalHours < 12)
				{
					breakEnd = breakEnd.AddDays(1);
				}

				if (breakEnd.CompareTo(breakStart) < 0)
                {
                    //this should be an overnight shift
                    breakEnd = breakEnd.AddDays(1);
                }

                if (breakEnd.CompareTo(dateStart) <= 0
                    || breakStart.CompareTo(dateEnd) >= 0)
                {
                    continue;
                }
                
                if (breakStart.CompareTo(dateStart) < 0)
                {
					// break time before the start time
					newBreakTime.StartTime = EmptyTime.Date + dateStart.TimeOfDay;
                }

                if (breakEnd.CompareTo(dateEnd) > 0)
                {
					// break time after the start time
					newBreakTime.EndTime = EmptyTime.Date + dateEnd.TimeOfDay;
                }

                newList.Add(newBreakTime);
            }

            return newList;
        }

		/// <summary>
		/// This will sort PM before AM times as work calendars all have the same date.
		/// </summary>
		public static IEnumerable<AMCalendarBreakTime> BreakTimeOvernightSort(List<AMCalendarBreakTime> breakTimes)
        {
            var breakTimeSort = new List<AMCalendarBreakTime>();
            foreach (var breakTime in breakTimes)
            {
				if (breakTime?.StartTime == null || breakTime.EndTime == null)
				{
					continue;
				}

				if (breakTime.StartTime.GetValueOrDefault().TimeOfDay.TotalHours < 12
					&& Common.Dates.IsDefaultDate(breakTime.StartTime.GetValueOrDefault().Date))
				{
					breakTime.StartTime = breakTime.StartTime.GetValueOrDefault().AddDays(1);
				}

				if (breakTime.EndTime.GetValueOrDefault().TimeOfDay.TotalHours < 12
					&& Common.Dates.IsDefaultDate(breakTime.EndTime.GetValueOrDefault().Date))
				{
					breakTime.EndTime = breakTime.EndTime.GetValueOrDefault().AddDays(1);
				}

				breakTimeSort.Add(breakTime);
			}

			breakTimeSort.Sort((x, y) => DateTime.Compare(x.StartTime.GetValueOrDefault(), y.StartTime.GetValueOrDefault()));
			return breakTimeSort;
        }

        public static decimal? GetTotalBreakTimeForDayOfWeek(DayOfWeek dayOfWeek, List<AMCalendarBreakTime> breakTimeList)
        {
            var breakTimes = GetBreakTimesForDayOfWeek(dayOfWeek, breakTimeList);
            if (breakTimes == null)
            {
                return null;
            }

            var sum = 0m;
            foreach (var breakTime in breakTimes)
            {
                sum += breakTime?.BreakTime ?? 0m;
            }

            return sum;
        }

        /// <summary>
        /// Return the break time detail related only to the day of week (includes those marked as any)
        /// </summary>
        public static List<AMCalendarBreakTime> GetBreakTimesForDayOfWeek(DayOfWeek dayOfWeek, List<AMCalendarBreakTime> breakTimeList)
        {
            if (breakTimeList == null)
            {
                return null;
            }

            var dow = (int)dayOfWeek;
            var dowList = breakTimeList.Where(aBreakTime => aBreakTime.DayOfWeek.GetValueOrDefault(-1) == dow).ToList();

            if (dowList.Count != 0)
            {
                return dowList;
            }

            return breakTimeList.Where(aBreakTime => aBreakTime.DayOfWeek.GetValueOrDefault(-1) == AMCalendarBreakTime.AMDayOfWeek.All).ToList();
        }

        /// <summary>
        /// Gets the break time detail for the given day of week and between the start/end times
        /// </summary>
        protected virtual List<AMCalendarBreakTime> GetBreakTimes(DayOfWeek dayOfWeek, DateTime startTime, DateTime endTime)
        {
            return AdjustBreakTimes(GetBreakTimesForDayOfWeek(dayOfWeek, CalendarBreakTimeList), startTime, endTime);
        }

        protected virtual AMDateInfo MakeAMDateInfo(DateTime date, bool? isWorkingDay, DateTime? startTime, DateTime? endTime, int breakTime)
        {
            return MakeAMDateInfo(date, isWorkingDay, startTime, endTime, breakTime, false, string.Empty);
        }

        protected virtual AMDateInfo MakeAMDateInfo(DateTime date, bool? isWorkingDay, DateTime? startTime, DateTime? endTime, int breakTime, bool isExceptionDate, string dateDescription)
        {
            if (_includeBreakTimeDetail && isWorkingDay.GetValueOrDefault())
            {
                //make date info to include break time detail
                return new AMDateInfo(date, isWorkingDay, startTime, endTime, GetBreakTimes(date.DayOfWeek, startTime.GetValueOrDefault(), endTime.GetValueOrDefault()), isExceptionDate, dateDescription);
            }

            //date info with no break time detail
            return new AMDateInfo(date, isWorkingDay, startTime, endTime, breakTime, isExceptionDate, dateDescription);
        }

        /// <summary>
        /// Returns information about a given date related to the current calendar
        /// </summary>
        /// <param name="dateTime">The date to get information</param>
        public virtual AMDateInfo GetDateInfo(DateTime dateTime)
        {
            CacheNextException(dateTime);

            var breakTime = GetBreakTimeMinutes(dateTime, CalendarBreakTimeList);

            if (_nextCSCalendarException != null)
            {
                if (Common.Dates.DatesEqual(dateTime, _nextCSCalendarException.Date))
                {
                    return MakeAMDateInfo(dateTime, _nextCSCalendarException.WorkDay,_nextCSCalendarException.StartTime,
                            _nextCSCalendarException.EndTime, breakTime, true, _nextCSCalendarException.Description);
                }
            }

            switch (dateTime.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    return MakeAMDateInfo(dateTime, _csCalendar.SunWorkDay, _csCalendar.SunStartTime, _csCalendar.SunEndTime, breakTime);
                case DayOfWeek.Monday:
                    return MakeAMDateInfo(dateTime, _csCalendar.MonWorkDay, _csCalendar.MonStartTime, _csCalendar.MonEndTime, breakTime);
                case DayOfWeek.Tuesday:
                    return MakeAMDateInfo(dateTime, _csCalendar.TueWorkDay, _csCalendar.TueStartTime, _csCalendar.TueEndTime, breakTime);
                case DayOfWeek.Wednesday:
                    return MakeAMDateInfo(dateTime, _csCalendar.WedWorkDay, _csCalendar.WedStartTime, _csCalendar.WedEndTime, breakTime);
                case DayOfWeek.Thursday:
                    return MakeAMDateInfo(dateTime, _csCalendar.ThuWorkDay, _csCalendar.ThuStartTime, _csCalendar.ThuEndTime, breakTime);
                case DayOfWeek.Friday:
                    return MakeAMDateInfo(dateTime, _csCalendar.FriWorkDay, _csCalendar.FriStartTime, _csCalendar.FriEndTime, breakTime);
                case DayOfWeek.Saturday:
                    return MakeAMDateInfo(dateTime, _csCalendar.SatWorkDay, _csCalendar.SatStartTime, _csCalendar.SatEndTime, breakTime);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected virtual DateTime AddDateTimeDays(DateTime dateTime, int days)
        {
            try
            {
                return dateTime.AddDays(days);
            }
            catch (ArgumentOutOfRangeException e)
            {
                PXTraceHelper.PxTraceException(e);

                throw new Exception(Messages.GetLocal(Messages.CalendarHelperUnableToAddDays, days, dateTime.ToShortDateString(), CurrentCalendarId) , e);
            }
        }

        public virtual DateTime NextDateTime(DateTime dateTime)
        {
            return AddDateTimeDays(dateTime, _calendarReadDirection == ReadDirection.Forward ? 1 : -1);
        }

        /// <summary>
        /// Returns the next calendar working day
        /// </summary>
        /// <param name="dateTime">current datetime object needing calculated to get the next date</param>
        /// <param name="includeCurrentDate">Should the dateTime current date be validated as a working day and if so returned</param>
        /// <returns>A working day with hours greater than zero</returns>
        public virtual DateTime? GetNextWorkDay(DateTime dateTime, bool includeCurrentDate = false)
        {
            const int maxNbrOfDays = 366;
            DateTime dateCheck = includeCurrentDate ? dateTime : NextDateTime(dateTime);

            for (int i = 0; i < maxNbrOfDays; i++)
            {
                var dateInfo = GetDateInfo(dateCheck);

                //if work day or "invalid date" lets exit
                if ((dateInfo.IsWorkingDay && dateInfo.WorkingHours > 0) 
                    || Common.Dates.IsDefaultDate(dateCheck) 
                    || Common.Dates.IsMinMaxDate(dateCheck))
                {
                    return dateInfo.Date;
                }

                dateCheck = NextDateTime(dateCheck);
            }

            return null;
        }

        public static CSCalendar SelectCalendar(PXGraph graph, string calendarID)
        {
            if (graph == null)
            {
                throw new ArgumentNullException(nameof(graph));
            }

            if (string.IsNullOrWhiteSpace(calendarID))
            {
                throw new PXException(Messages.MissingCalendarID);
            }

            CSCalendar calendar = PXSelect<CSCalendar, 
                Where<CSCalendar.calendarID, Equal<Required<CSCalendar.calendarID>>>>
                            .Select(graph, calendarID);

            if (calendar == null)
            {
                throw new InvalidWorkCalendarException(calendarID);
            }
            
            return calendar;
        }

        public static CSCalendar GetCalendar(PXGraph graph, string scheduleType, string id)
        {
            if (graph == null)
            {
                throw new PXArgumentException(nameof(graph));
            }

            if (string.IsNullOrWhiteSpace(scheduleType))
            {
                throw new PXArgumentException(nameof(scheduleType));
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new PXArgumentException(nameof(id));
            }

            switch (scheduleType)
            {
                case AMWC.BasisForCapacity.Machines:

                    AMMach amMach = PXSelect<AMMach, 
                        Where<AMMach.machID, Equal<Required<AMMach.machID>>>
                        >.Select(graph, id);

                    if (amMach == null)
                    {
                        throw new Exception(Messages.GetLocal(Messages.RecordMissingWithID,
                            Common.Cache.GetCacheName(typeof(AMMach)), id));
                    }

                    return SelectCalendar(graph, amMach.CalendarID);

                case AMWC.BasisForCapacity.CrewSize:

                    AMShift amShift = PXSelect<AMShift, 
                        Where<AMShift.wcID, Equal<Required<AMShift.wcID>>>,
                        OrderBy<Asc<AMShift.shiftCD>>
                            >.Select(graph, id);

                    if (amShift == null)
                    {
                        throw new Exception(Messages.GetLocal(Messages.RecordMissingWithID,
                            Common.Cache.GetCacheName(typeof(AMShift)), id));
                    }

                    return SelectCalendar(graph, amShift.CalendarID);

                default:
                    throw new ArgumentOutOfRangeException(nameof(scheduleType));
            }
        }

		/// <summary>
		/// Given a day of the week and a working calendar return the correct working times of that day.
		/// Not date specific
		/// </summary>
		public static List<DateRange> GetGenericWorkingTimes(DayOfWeek dayOfWeek, CSCalendar calendar, List<AMCalendarBreakTime> breakTimes)
		{
			if(calendar == null)
			{
				return null;
			}

			switch (dayOfWeek)
			{
				case DayOfWeek.Sunday:
					return calendar.SunWorkDay == true ? GetGenericWorkingTimes(dayOfWeek, calendar.SunStartTime.GetValueOrDefault(), calendar.SunEndTime.GetValueOrDefault(), breakTimes) : null;
				case DayOfWeek.Monday:
					return calendar.MonWorkDay == true ? GetGenericWorkingTimes(dayOfWeek, calendar.MonStartTime.GetValueOrDefault(), calendar.MonEndTime.GetValueOrDefault(), breakTimes) : null;
				case DayOfWeek.Tuesday:
					return calendar.TueWorkDay == true ? GetGenericWorkingTimes(dayOfWeek, calendar.TueStartTime.GetValueOrDefault(), calendar.TueEndTime.GetValueOrDefault(), breakTimes) : null;
				case DayOfWeek.Wednesday:
					return calendar.WedWorkDay == true ? GetGenericWorkingTimes(dayOfWeek, calendar.WedStartTime.GetValueOrDefault(), calendar.WedEndTime.GetValueOrDefault(), breakTimes) : null;
				case DayOfWeek.Thursday:
					return calendar.ThuWorkDay == true ? GetGenericWorkingTimes(dayOfWeek, calendar.ThuStartTime.GetValueOrDefault(), calendar.ThuEndTime.GetValueOrDefault(), breakTimes) : null;
				case DayOfWeek.Friday:
					return calendar.FriWorkDay == true ? GetGenericWorkingTimes(dayOfWeek, calendar.FriStartTime.GetValueOrDefault(), calendar.FriEndTime.GetValueOrDefault(), breakTimes) : null;
				case DayOfWeek.Saturday:
					return calendar.SatWorkDay == true ? GetGenericWorkingTimes(dayOfWeek, calendar.SatStartTime.GetValueOrDefault(), calendar.SatEndTime.GetValueOrDefault(), breakTimes) : null;
			}

			return null;
		}

		/// <summary>
		/// Given a day of the week and a working calendar return the correct working times of that day.
		/// Not date specific
		/// </summary>
		public static List<DateRange> GetGenericWorkingTimes(DayOfWeek dayOfWeek, DateTime startTime, DateTime endTime, List<AMCalendarBreakTime> breakTimes)
		{
			var results = new List<DateRange>();
			var sDate = startTime;
			var eDate = endTime;
			var isOvernight = AMDateInfo.IsOvernightTime(sDate, eDate);
			if (isOvernight)
			{
				eDate = eDate.AddDays(1);
			}

			var dayOfWeekBreakTimes = AdjustBreakTimes(GetBreakTimesForDayOfWeek(dayOfWeek, breakTimes), sDate, eDate);
			if (dayOfWeekBreakTimes == null || dayOfWeekBreakTimes.Count == 0)
			{
				results.Add(new DateRange(sDate, eDate));
				return results;
			}

			var lastRange = new DateRange(sDate, eDate);
			foreach (var bt in dayOfWeekBreakTimes.OrderBy(d => d.StartTime))
			{
				if (bt?.StartTime == null || bt.StartTime == bt.EndTime)
				{
					continue;
				}

				var bStart = bt.StartTime.GetValueOrDefault();
				var bEnd = bt.EndTime.GetValueOrDefault();
				
				if (isOvernight && bStart.LessThan(sDate))
				{
					bStart = bStart.AddDays(1);
				}
				if (isOvernight && bEnd.LessThan(sDate))
				{
					bEnd = bEnd.AddDays(1);
				}

				if (bEnd.LessThan(bStart))
				{
					bEnd = bEnd.AddDays(1);
				}

				// chunk of time before the break
				if (bStart.LessThanOrEqualTo(lastRange.EndDate))
				{
					results.Add(new DateRange(lastRange.StartDate, bStart));
					lastRange = new DateRange(bEnd.LessThanOrEqualTo(lastRange.EndDate) ? bEnd : lastRange.EndDate, lastRange.EndDate);
					continue;
				}

				// chunk of time after the break
				if (bEnd.LessThanOrEqualTo(lastRange.EndDate))
				{
					results.Add(new DateRange(bEnd, lastRange.EndDate));
					lastRange = new DateRange(lastRange.EndDate, lastRange.EndDate);
					continue;
				}
			}

			if (lastRange.StartDate.LessThan(lastRange.EndDate))
			{
				results.Add(lastRange);
			}

			return results;
		}

		public void Initialize(string calendarID, ReadDirection readDirection)
		{
			_csCalendar = SelectCalendar(_graph, calendarID);
			_includeBreakTimeDetail = false;
			if (_csCalendar == null)
			{
				throw new InvalidWorkCalendarException();
			}
			CalendarReadDirection = readDirection;
			_nextCSCalendarException = new CSCalendarExceptions()
			{
				CalendarID = _csCalendar.CalendarID
			};
		}
    }

	#if DEBUG
    [System.Diagnostics.DebuggerDisplay("StartTime={StartDate.ToString()}, EndTime={EndDate.ToString()}")]
	//[System.Diagnostics.DebuggerDisplay("StartTime={StartDate.ToShortTimeString()}, EndTime={EndDate.ToShortTimeString()}")]
	#endif
	public struct DateRange
	{
		public DateTime StartDate { get; private set; }
		public DateTime EndDate { get; private set; }

		public DateRange(DateTime startDate, DateTime endDate)
		{
			StartDate = startDate;
			EndDate = endDate;
		}
	}

	public interface ICalendarHelper
	{
		void Initialize(string calendarID, ReadDirection readDirection);
		AMDateInfo GetDateInfo(DateTime dateTime);
		DateTime? GetNextWorkDay(DateTime dateTime, bool includeCurrentDate = false);
		DateTime NextDateTime(DateTime dateTime);
	}
}
