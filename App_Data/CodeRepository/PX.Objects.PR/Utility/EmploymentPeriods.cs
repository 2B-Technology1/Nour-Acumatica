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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.EP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PR
{
	public class EmploymentPeriods
	{
		public EmploymentPeriods(HashSet<DateTime> employmentDates, bool employedForEntireBatchPeriod)
		{
			EmploymentDates = employmentDates;
			EmployedForEntireBatchPeriod = employedForEntireBatchPeriod;
		}

		public HashSet<DateTime> EmploymentDates { get; }
		public bool EmployedForEntireBatchPeriod { get; }

		public bool IsEmployedOnDate(DateTime date)
		{
			return EmployedForEntireBatchPeriod || EmploymentDates.Contains(date.Date);
		}

		public static EmploymentPeriods GetEmploymentPeriods(PXGraph graph, int? currentEmployeeID, DateTime batchStartDate, DateTime batchEndDate)
		{
			HashSet<DateTime> employmentDates = new HashSet<DateTime>();

			PXResultset<EPEmployeePosition> employeePositionsWithinBatchPeriod =
				SelectFrom<EPEmployeePosition>.
				Where<EPEmployeePosition.employeeID.IsEqual<P.AsInt>.
					And<EPEmployeePosition.startDate.IsLessEqual<P.AsDateTime.UTC>>.
					And<EPEmployeePosition.endDate.IsNull.
						Or<EPEmployeePosition.endDate.IsGreaterEqual<P.AsDateTime.UTC>>>>.
				OrderBy<EPEmployeePosition.startDate.Desc>.View.
				Select(graph, currentEmployeeID, batchEndDate, batchStartDate);

			foreach (EPEmployeePosition position in employeePositionsWithinBatchPeriod)
			{
				if (position.StartDate <= batchStartDate &&
					(position.EndDate == null || position.EndDate >= batchEndDate))
				{
					return new EmploymentPeriods(new HashSet<DateTime>(), true);
				}

				DateTime startDate = position.StartDate == null || position.StartDate <= batchStartDate
					? batchStartDate
					: position.StartDate.Value;
				DateTime endDate = position.EndDate == null || position.EndDate >= batchEndDate
					? batchEndDate
					: position.EndDate.Value;
				
				for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
				{
					employmentDates.Add(date.Date);
				}
			}

			for (DateTime date = batchStartDate; date <= batchEndDate; date = date.AddDays(1))
			{
				if (!employmentDates.Contains(date))
				{
					return new EmploymentPeriods(employmentDates, false);
				}
			}

			return new EmploymentPeriods(new HashSet<DateTime>(), true);
		}
	}
}
