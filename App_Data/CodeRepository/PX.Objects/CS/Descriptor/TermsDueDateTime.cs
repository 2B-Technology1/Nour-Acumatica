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
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.CS
{
	public static class TermsDueDateTime
	{
		public static DateTime GetEndOfMonth(this DateTime date) => new DateTime(date.Year, date.Month, 1).AddMonths(1).AddDays(-1);
		public static DateTime SetDayInMonthAfter(this DateTime date, int dayNumber)
		{
			var nextDate = date.AddMonths(1);

			return new DateTime(nextDate.Year, nextDate.Month, Math.Min(DateTime.DaysInMonth(nextDate.Year, nextDate.Month), dayNumber));
		}
		public static DateTime SetDateByEndOfDecade(this DateTime date)
		{
			if (date.Day <= 10)
			{
				return new DateTime(date.Year, date.Month, 10);
			}
			else if (date.Day <= 20)
			{
				return new DateTime(date.Year, date.Month, 20);
			}
			else
			{
				var daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
				return new DateTime(date.Year, date.Month, daysInMonth);
			}
		}
	}
}
