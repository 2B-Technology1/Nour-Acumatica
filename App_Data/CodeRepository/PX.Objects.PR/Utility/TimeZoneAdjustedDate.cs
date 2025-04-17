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

using PX.Common;
using PX.Data;
using PX.Data.SQLTree;
using System;
using System.Collections.Generic;

namespace PX.Objects.PR
{
	public sealed class TimeZoneAdjustedDate<Date, TimeZoneID> : BqlFunction, IBqlOperand, IBqlCreator
		where Date : IBqlOperand
		where TimeZoneID : IBqlOperand
	{
		private IBqlCreator _DateOperand;
		private IBqlCreator _TimeZoneIDOperand;

		private static SQLExpression _TimeZoneDateCondition;

		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			value = null;

			if (!getValue<Date>(ref _DateOperand, cache, item, pars, ref result, out object date) || date == null || !(date is DateTime dateValue))
			{
				return;
			}
			if (!getValue<TimeZoneID>(ref _TimeZoneIDOperand, cache, item, pars, ref result, out object timeZoneID) || timeZoneID == null || !(timeZoneID is string timeZoneIDValue))
			{
				return;
			}

			PXTimeZoneInfo timeZoneInfo = PXTimeZoneInfo.FindSystemTimeZoneById(timeZoneIDValue);

			if (timeZoneInfo != null)
			{
				value = PXTimeZoneInfo.ConvertTimeToUtc(dateValue, timeZoneInfo);
			}
		}

		
		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
		{
			bool status = true;

			SQLExpression dateSQLExpression = null;
			status &= GetOperandExpression<Date>(ref dateSQLExpression, ref _DateOperand, graph, info, selection);

			SQLExpression timeZoneIDExpression = null;
			status &= GetOperandExpression<TimeZoneID>(ref timeZoneIDExpression, ref _TimeZoneIDOperand, graph, info, selection);

			if (info.BuildExpression)
			{
				if (_TimeZoneDateCondition != null)
				{
					exp = _TimeZoneDateCondition;
					return status;
				}

				SQLSwitch sqlSwitch = new SQLSwitch();

				foreach (PXTimeZoneInfo timeZoneInfo in PXTimeZoneInfo.GetSystemTimeZones())
				{
					sqlSwitch.Case(timeZoneIDExpression.EQ(timeZoneInfo.Id), new SQLDateAdd(new DatePart.minute(), new SQLConst(timeZoneInfo.UtcOffset.TotalMinutes), dateSQLExpression));
				}

				sqlSwitch.Default(dateSQLExpression);
				// The line below is needed to remove the time component from the SQL datetime field.
				// SELECT DATEADD(dd, 0, DATEDIFF(dd, 0, '2021-12-23 12:34:56.789')) returns 2021-12-23 00:00:00.000
				DateTime sqlMinDate = new DateTime(1900, 1, 1);
				exp = new SQLDateAdd(new DatePart.day(), new SQLDateDiff(new DatePart.day(), new SQLConst(sqlMinDate), sqlSwitch), new SQLConst(sqlMinDate));
				_TimeZoneDateCondition = exp;
			}

			return status;
		}
	}
}
