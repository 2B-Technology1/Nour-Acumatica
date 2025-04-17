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
using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.EP;

namespace PX.Objects.PR
{
	public static class EmploymentHistoryHelper
	{
		public static EmploymentDates GetEmploymentDates(PXGraph graph, int? employeeID, DateTime? effectiveDate)
		{
			EPEmployeePosition[] employeePositions = SelectFrom<EPEmployeePosition>
				.Where<EPEmployeePosition.employeeID.IsEqual<P.AsInt>>.View
				.Select(graph, employeeID).FirstTableItems.ToArray();

			DateTime? initialHireDate = GetInitialHireDate(employeePositions, effectiveDate);
			DateTime? continuousHireDate = GetContinuousHireDate(employeePositions, effectiveDate);
			DateTime? rehireDateAuf = initialHireDate < continuousHireDate ? continuousHireDate : null;
			DateTime? terminationDate = GetTerminationDate(employeePositions);
			DateTime? terminationDateAuf = terminationDate <= effectiveDate ? terminationDate : null;

			return new EmploymentDates(initialHireDate, continuousHireDate, rehireDateAuf, terminationDate, terminationDateAuf);
		}

		private static DateTime? GetInitialHireDate(EPEmployeePosition[] employeePositions, DateTime? effectiveDate)
		{
			return employeePositions.Where(p => p.StartDate <= effectiveDate).Min(p => p.StartDate);
		}

		private static DateTime? GetContinuousHireDate(EPEmployeePosition[] employeePositions, DateTime? effectiveDate)
		{
			EPEmployeePosition[] sortedEmployeePositions = employeePositions
				.Where(p => p.StartDate <= effectiveDate)
				.OrderByDescending(p => p.StartDate).ToArray();

			EPEmployeePosition currentPosition = sortedEmployeePositions.FirstOrDefault();
			if (currentPosition == null)
				return null;

			while (true)
			{
				DateTime hireDate = currentPosition.StartDate.Value;

				EPEmployeePosition previousPosition = sortedEmployeePositions.FirstOrDefault(p => 
						p.StartDate < hireDate && (p.EndDate == null || (hireDate.Date - p.EndDate.Value.Date).TotalDays <= 1));
				if (previousPosition == null)
					return hireDate;

				currentPosition = previousPosition;
			}
		}

		private static DateTime? GetTerminationDate(EPEmployeePosition[] employeePositions)
		{
			if (employeePositions.Any(p => p.IsActive == true && p.EndDate == null))
				return null;
			
			return employeePositions.Max(p => p.EndDate);
		}
	}
}
