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
using PX.Objects.Common.Extensions;
using PX.Objects.GL.FinPeriods.TableDefinition;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.GL.FinPeriods
{
	class CalendarByYearPrefetchParameters
	{
		public int OrgID { get; set; }
		public PXGraph Graph { get; set; }
	}

	class CalendarByYear : IPrefetchable<CalendarByYearPrefetchParameters>
	{
		private readonly Dictionary<int, List<FinPeriod>> periodsByYear = new Dictionary<int, List<FinPeriod>>();
		private readonly Dictionary<string, FinPeriod> periodsByID = new Dictionary<string, FinPeriod>();
		private readonly Dictionary<string, FinPeriod> periodsByMasterID = new Dictionary<string, FinPeriod>();

		private void AddPeriodForCalendarYear(int year, FinPeriod period) => periodsByYear.GetOrAdd(year, () => new List<FinPeriod>()).Add(period);

		public void Prefetch(CalendarByYearPrefetchParameters parameters)
		{
			IEnumerable<FinPeriod> periods = PXSelect<FinPeriod, Where<FinPeriod.organizationID, Equal<Required<FinPeriod.organizationID>>>>
				.Select(parameters.Graph, parameters.OrgID).RowCast<FinPeriod>();

			foreach (FinPeriod period in periods)
			{
				if (period.StartDate.Value.Year == period.EndDate.Value.Year)
				{
					AddPeriodForCalendarYear(period.StartDate.Value.Year, period);
				}
				else
				{
					AddPeriodForCalendarYear(period.StartDate.Value.Year, period);
					AddPeriodForCalendarYear(period.EndDate.Value.Year, period);
				}
				periodsByID.Add(period.FinPeriodID, period);
				if (period.MasterFinPeriodID != null) periodsByMasterID.Add(period.MasterFinPeriodID, period);
			}
		}

		public FinPeriod FetchFinPeriod(DateTime date)
		{
			if (periodsByYear.TryGetValue(date.Year, out List<FinPeriod> periods))
				return periods.SingleOrDefault(_ => _.StartDate.Value <= date && _.EndDate.Value > date);
			else return null;
		}

		public FinPeriod FetchFinPeriod(string finperiodID)
		{
			if (finperiodID == null) return null;

			if (periodsByID.TryGetValue(finperiodID, out FinPeriod finPeriod)) return finPeriod;
			else return null;
		}

		public FinPeriod FetchFinPeriodByMasterID(string finperiodID)
		{
			if (finperiodID == null) return null;

			if (periodsByMasterID.TryGetValue(finperiodID, out FinPeriod finPeriod)) return finPeriod;
			else return null;
		}
	}
}
