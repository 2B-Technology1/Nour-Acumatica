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
using PX.Objects.CR;

namespace PX.Objects.CN.Common.Helpers
{
	public static class DateTimeHelper
	{
		public static DateTime CalculateBusinessDate(DateTime originalBusinessDate, int timeSpan,
			string calendarId)
		{
			var businessDaysDifference = GetBusinessDaysDifference(originalBusinessDate, timeSpan, calendarId);
			return originalBusinessDate.AddDays(businessDaysDifference);
		}

		private static int GetBusinessDaysDifference(DateTime originalBusinessDate, int timeFrame,
			string calendarId)
		{
			var daysDifference = 1;
			var graph = PXGraph.CreateInstance<PXGraph>();
			while (true)
			{
				var newBusinessDate = originalBusinessDate.AddDays(daysDifference);
				if (CalendarHelper.IsWorkDay(graph, calendarId, newBusinessDate) &&
				    --timeFrame < 1)
				{
					break;
				}
				daysDifference++;
			}
			return daysDifference;
		}
	}
}
