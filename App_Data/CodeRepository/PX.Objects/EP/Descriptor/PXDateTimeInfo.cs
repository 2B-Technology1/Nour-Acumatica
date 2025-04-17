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
using PX.Common;
using PX.Objects.EP;

namespace PX.Data.EP
{
	public class PXDateTimeInfo
	{
		private class Definition : IPrefetchable
		{
			private DayOfWeek _firstDayOfWeek = DayOfWeek.Sunday;

			public void Prefetch()
			{
				using (new PXConnectionScope())
				{
					using (PXDataRecord prefGeneral = PXDatabase.SelectSingle<EPSetup>(
							new PXDataField(typeof(EPSetup.firstDayOfWeek).Name)))
					{
						if (prefGeneral != null)
						{
							var firstDayValue = prefGeneral.GetInt32(0);
							_firstDayOfWeek = firstDayValue == null ? DayOfWeek.Sunday : (DayOfWeek)firstDayValue;
						}
					}
				}
			}

			public static Definition Get()
			{
				return PXDatabase.GetSlot<Definition>(typeof(PXDateTimeInfo).Name, typeof(EPSetup));
			}

			public DayOfWeek FirstDayOfWeek
			{
				get { return _firstDayOfWeek; }
			}
		}

		public static int GetWeekNumber(DateTime date)
		{
			var def = Definition.Get();
			if (def == null) return Tools.GetWeekNumber(date);
			return Tools.GetWeekNumber(date, def.FirstDayOfWeek);
		}

		public static DateTime GetWeekStart(int year, int weekNumber)
		{
			var def = Definition.Get();
			if (def == null) return Tools.GetWeekStart(year, weekNumber);
			return Tools.GetWeekStart(year, weekNumber, def.FirstDayOfWeek);
		}
	}
}
