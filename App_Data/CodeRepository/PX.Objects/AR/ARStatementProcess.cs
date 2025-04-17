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
using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.AR
{
	[TableAndChartDashboardType]
	public class ARStatementProcess : PXGraph<ARStatementProcess>
	{
		#region Internal Types Definition
		public class Parameters : IBqlTable
		{
			public abstract class statementDate : PX.Data.BQL.BqlDateTime.Field<statementDate> { }
			/// <summary>
			/// Indicates the date on which the statements are generated.
			/// Defaults to the current business date.
			/// </summary>
			[PXDate]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = Messages.PrepareFor)]
			public virtual DateTime? StatementDate
			{
				get;
				set;
			}
		}
		#endregion

		public ARStatementProcess()
		{
			ARSetup setup = ARSetup.Current;
			CyclesList.SetProcessDelegate<StatementCycleProcessBO>(StatementCycleProcessBO.ProcessCycles);
		}

		public PXSetup<ARSetup> ARSetup;

		public PXCancel<Parameters> Cancel;
		public PXFilter<Parameters> Filter;

		[PXFilterable]
		public PXFilteredProcessing<ARStatementCycle, Parameters> CyclesList;

		protected virtual IEnumerable cycleslist()
		{
			ARSetup setup = this.ARSetup.Select();

			DateTime statementDate = Filter.Current?.StatementDate ?? Accessinfo.BusinessDate.Value;

			foreach (ARStatementCycle row in PXSelect<ARStatementCycle>.Select(this))
			{
				try
				{
					row.NextStmtDate = CalcStatementDateBefore(
						this,
						statementDate,
						row.PrepareOn,
						row.Day00,
						row.Day01,
						row.DayOfWeek);

					if (row.LastStmtDate != null && row.NextStmtDate <= row.LastStmtDate)
					{
						row.NextStmtDate = CalcNextStatementDate(
							this,
							row.LastStmtDate.Value,
							row.PrepareOn,
							row.Day00,
							row.Day01,
							row.DayOfWeek);
					}
				}
				catch (PXFinPeriodException)
				{
					row.NextStmtDate = null;
				}

				if (row.NextStmtDate > statementDate)
				{
					continue;
				}

				CyclesList.Cache.SetStatus(row, PXEntryStatus.Updated);

				yield return row;
			}
		}

		public static bool CheckForUnprocessedPPD(PXGraph graph, string statementCycleID, DateTime? nextStmtDate, int? customerID)
		{
			PXSelectBase<ARInvoice> select = new PXSelectJoin<ARInvoice, 
				InnerJoin<Customer, On<Customer.bAccountID, Equal<ARInvoice.customerID>>,
				InnerJoin<ARAdjust, On<ARAdjust.adjdDocType, Equal<ARInvoice.docType>,
					And<ARAdjust.adjdRefNbr, Equal<ARInvoice.refNbr>,
					And<ARAdjust.released, Equal<True>, 
					And<ARAdjust.voided, NotEqual<True>,
					And<ARAdjust.pendingPPD, Equal<True>,
					And<ARAdjust.adjgDocDate, LessEqual<Required<ARAdjust.adjgDocDate>>>>>>>>>>,
				Where<ARInvoice.pendingPPD, Equal<True>,
					And<ARInvoice.released, Equal<True>,
					And<ARInvoice.openDoc, Equal<True>,
					And<Customer.statementCycleId, Equal<Required<Customer.statementCycleId>>>>>>>(graph);

			if (customerID != null)
			{
				select.WhereAnd<Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>();
			}

			return select.SelectSingle(nextStmtDate, statementCycleID, customerID) != null;
		}

		/// <summary>
		/// Returns a boolean flag indicating whether the statement contains no details.
		/// For Balance Brought Forward statements, additionally checks that its
		/// forward balance is not zero.
		/// </summary>
		public static bool IsEmptyStatement(PXGraph graph, ARStatement statement)
		{
			if (statement.StatementType == ARStatementType.BalanceBroughtForward)
			{
				bool isForwardBalanceZero =
					!statement.BegBalance.IsNonZero()
					&& !statement.CuryBegBalance.IsNonZero()
					&& !statement.EndBalance.IsNonZero()
					&& !statement.CuryBegBalance.IsNonZero();

				if (!isForwardBalanceZero) return false;
			}

			IEnumerable<ARStatementDetail> details = PXSelect<
				ARStatementDetail,
				Where<
					ARStatementDetail.branchID, Equal<Required<ARStatementDetail.branchID>>,
					And<ARStatementDetail.curyID, Equal<Required<ARStatementDetail.curyID>>,
					And<ARStatementDetail.customerID, Equal<Required<ARStatementDetail.customerID>>,
					And<ARStatementDetail.statementDate, Equal<Required<ARStatementDetail.statementDate>>>>>>>
				.SelectWindowed(graph, 0, 1, statement.BranchID, statement.CuryID, statement.CustomerID, statement.StatementDate)
				.RowCast<ARStatementDetail>();

			return IsEmptyStatement(statement, details);
		}

		public static bool IsEmptyStatement(ARStatement statement, IEnumerable<ARStatementDetail> statementDetails)
		{
			if (statement.StatementType == ARStatementType.BalanceBroughtForward)
			{
				bool isForwardBalanceZero =
					!statement.BegBalance.IsNonZero()
					&& !statement.CuryBegBalance.IsNonZero()
					&& !statement.EndBalance.IsNonZero()
					&& !statement.CuryEndBalance.IsNonZero();

				return isForwardBalanceZero && !statementDetails.Any(detail => detail.RefNbr != string.Empty);
			}
			else
			{
				return !statementDetails.Any(detail => detail.RefNbr != string.Empty);
			}
		}

		private static bool CheckForOpenPayments(PXGraph aGraph, string aStatementCycleID)
		{
			ARRegister doc = PXSelectJoin<ARPayment,
								InnerJoin<Customer, On<ARPayment.customerID, Equal<Customer.bAccountID>>>,
								Where<Customer.statementCycleId, Equal<Required<Customer.statementCycleId>>,
				And<ARPayment.openDoc, Equal<True>>>>.SelectWindowed(aGraph, 0, 1, aStatementCycleID);
			return (doc != null);
		}

		private static bool CheckForOverdueInvoices(PXGraph aGraph, string aStatementCycleID, DateTime aOpDate)
		{
			ARBalances doc = PXSelectJoin<ARBalances,
								InnerJoin<Customer, On<ARBalances.customerID, Equal<Customer.bAccountID>>>,
								Where<Customer.statementCycleId, Equal<Required<Customer.statementCycleId>>,
								And<ARBalances.oldInvoiceDate, LessEqual<Required<ARBalances.oldInvoiceDate>>>>>.SelectWindowed(aGraph, 0, 1, aStatementCycleID, aOpDate);
			return (doc != null);
		}
			   
		public virtual void ARStatementCycle_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;

			ARStatementCycle row = (ARStatementCycle)e.Row;
			ARSetup setup = this.ARSetup.Select();
			PXCache.TryDispose(cache.GetAttributes(e.Row, null));

			if (row.NextStmtDate == null)
			{
				cache.DisplayFieldError<ARStatementCycle.nextStmtDate>(
					row,
					PXErrorLevel.RowWarning,
					Messages.UnableToCalculateNextStatementDateForEndOfPeriodCycle);

				return;
			}

			if (CheckForUnprocessedPPD(this, row.StatementCycleId, row.NextStmtDate, null))
			{
				PXUIFieldAttribute.SetEnabled(cache, row, false);
				cache.RaiseExceptionHandling<ARStatementCycle.selected>(row, false, 
					new PXSetPropertyException(Messages.UnprocessedPPDExists, PXErrorLevel.RowError));

				return;
			}

			bool? hasOverdueInvoices = null;
			bool hasUnAppliedPayments = false;
			bool hasChargeableInvoices = false;
					
			if (row.RequirePaymentApplication == true)
			{
				hasOverdueInvoices = CheckForOverdueInvoices(this, row.StatementCycleId, row.NextStmtDate.Value);

				if (hasOverdueInvoices == true && CheckForOpenPayments(this, row.StatementCycleId))
				{
					hasUnAppliedPayments = true;
				}
			}

			// The third condition below conveys the 'hidden' meaning of 
			// DefFinChargeFromCycle, i.e. 'attaching' overdue charges calculation
			// to statement cycles.
			//
			// If DefFinChargeFromCycle is false, it is assumed that the users take 
			// care of overdue charges themselves and need not be warned.
			// -
			if (row.FinChargeApply == true 
				&& row.RequireFinChargeProcessing == true 
				&& setup.DefFinChargeFromCycle == true)
			{
				if (!hasOverdueInvoices.HasValue)
				{
					hasOverdueInvoices = CheckForOverdueInvoices(this, row.StatementCycleId, row.NextStmtDate.Value);
				}
				if (hasOverdueInvoices.Value && 
					(!row.LastFinChrgDate.HasValue || row.LastFinChrgDate.Value < row.NextStmtDate.Value))
				{
					hasChargeableInvoices = true;						
				}
			}

			if (hasChargeableInvoices && hasUnAppliedPayments)
			{
				CyclesList.Cache.RaiseExceptionHandling<ARStatementCycle.statementCycleId>(
					row, 
					row.StatementCycleId,
					new PXSetPropertyException(Messages.WRN_ProcessStatementDetectsOverdueInvoicesAndUnappliedPayments, PXErrorLevel.RowWarning));
			}
			else if (hasChargeableInvoices) 
			{
				CyclesList.Cache.RaiseExceptionHandling<ARStatementCycle.statementCycleId>(
					row, 
					row.StatementCycleId,
					new PXSetPropertyException(Messages.WRN_ProcessStatementDetectsOverdueInvoices, PXErrorLevel.RowWarning));						
			}
			else if (hasUnAppliedPayments) 
			{
				CyclesList.Cache.RaiseExceptionHandling<ARStatementCycle.statementCycleId>(
					row, 
					row.StatementCycleId,
					new PXSetPropertyException(Messages.WRN_ProcessStatementDetectsUnappliedPayments, PXErrorLevel.RowWarning));
			}
		}

		#region InstanceUtility functions
		public static DateTime? CalcNextStatementDate(
			PXGraph graph,
			DateTime aLastStmtDate, 
			string aPrepareOn, 
			int? aDay00, 
			int? aDay01, 
			int? dayofWeek)
		{
			DateTime? nextDate = null;
			switch (aPrepareOn)
			{
				case ARStatementScheduleType.FixedDayOfMonth:
					DateTime guessDate = new PXDateTime(aLastStmtDate.Year, aLastStmtDate.Month, aDay00 ?? 1);
					nextDate = getNextDate(guessDate, aLastStmtDate, aDay00 ?? 1);
					break;
				case ARStatementScheduleType.EndOfMonth:
					DateTime dateTime = new DateTime(aLastStmtDate.Year, aLastStmtDate.Month , 1);
					dateTime = dateTime.AddMonths(1);
					TimeSpan diff = (dateTime.Subtract(aLastStmtDate));
					int days = diff.Days;
					if (days < 2)
						nextDate = dateTime.AddMonths(1).AddDays(-1);
					else
						nextDate = dateTime.AddDays(-1);
					break;
				case ARStatementScheduleType.TwiceAMonth:

					DateTime dateTime1 = DateTime.MinValue;
					DateTime dateTime2 = DateTime.MinValue;
					bool useBoth = (aDay00 != null) && (aDay01 != null);
					if (aDay00 != null)
						dateTime1 = new PXDateTime(aLastStmtDate.Year, aLastStmtDate.Month, aDay00.Value);
					if (aDay01 != null)
						dateTime2 = new PXDateTime(aLastStmtDate.Year, aLastStmtDate.Month, aDay01.Value);
					if (useBoth)
					{
						Int32 Day00 = (Int32)aDay00;
						Int32 Day01 = (Int32)aDay01;
						Utilities.SwapIfGreater(ref dateTime1, ref dateTime2);
						Utilities.SwapIfGreater(ref Day00, ref Day01);
						if (aLastStmtDate < dateTime1)
							nextDate = dateTime1;
						else
						{
							if (aLastStmtDate < dateTime2)
								nextDate = dateTime2;
							else
								nextDate = PXDateTime.DatePlusMonthSetDay(dateTime1, 1, Day00);
						}
					}
					else
					{
						DateTime dt = (dateTime1 != DateTime.MinValue) ? dateTime1 : dateTime2;
						if (dt != DateTime.MinValue)
						{
							nextDate = getNextDate(dt, aLastStmtDate, aDay00 ?? aDay01 ?? 1);
						}
					}
					break;
				case ARStatementScheduleType.EndOfPeriod:
					try
					{
						IFinPeriodRepository finPeriodRepository = graph.GetService<IFinPeriodRepository>();

						string dateFinancialPeriod = finPeriodRepository.GetPeriodIDFromDate(aLastStmtDate, FinPeriod.organizationID.MasterValue);
						DateTime periodEndDate = finPeriodRepository.PeriodEndDate(dateFinancialPeriod, FinPeriod.organizationID.MasterValue);

						if (periodEndDate.Date > aLastStmtDate.Date)
						{
							nextDate = periodEndDate;
						}
						else
						{
							nextDate = finPeriodRepository.PeriodEndDate(
								finPeriodRepository.GetOffsetPeriodId(dateFinancialPeriod, 1, FinPeriod.organizationID.MasterValue),
								FinPeriod.organizationID.MasterValue);
						}
					}
					catch (PXFinPeriodException exception)
					{
						throw new PXFinPeriodException(
							$"{PXLocalizer.Localize(Messages.UnableToCalculateNextStatementDateForEndOfPeriodCycle)} {exception.MessageNoPrefix}");
					}
					break;
				case ARStatementScheduleType.Weekly:
					DateTime result = aLastStmtDate;

					do
					{
						result = result.AddDays(1);
					}
					while ((int?)result.DayOfWeek != dayofWeek);

					nextDate = result;
					break;
				default:
					throw new PXException(
						Messages.UnsupportedStatementScheduleType, 
						GetLabel.For<ARStatementScheduleType>(aPrepareOn));
			}
			return nextDate;
		}

		public static DateTime CalcStatementDateBefore(
			PXGraph graph,
			DateTime aBeforeDate, 
			string aPrepareOn, 
			int? aDay00, 
			int? aDay01,
			int? dayOfWeek)
		{
			DateTime statementDate = DateTime.MinValue;
			switch (aPrepareOn)
			{
				case ARStatementScheduleType.FixedDayOfMonth:
					statementDate = new PXDateTime(aBeforeDate.Year, aBeforeDate.Month, aDay00 ?? 1);
					if (statementDate.Date == aBeforeDate.Date)
						return statementDate;
					statementDate = getPrevDate(statementDate, aBeforeDate, aDay00 ?? 1);
					break;
				case ARStatementScheduleType.EndOfMonth:
					if (aBeforeDate.AddDays(1).Month != aBeforeDate.Month)
						return aBeforeDate;
					DateTime dateTime = new DateTime(aBeforeDate.Year, aBeforeDate.Month, 1);
					statementDate = dateTime.AddDays(-1);
					break;
				case ARStatementScheduleType.TwiceAMonth:
					DateTime dateTime1 = DateTime.MinValue;
					DateTime dateTime2 = DateTime.MinValue;
					bool useBoth = (aDay00 != null) && (aDay01 != null);
					if (aDay00 != null)
						dateTime1 = new PXDateTime(aBeforeDate.Year, aBeforeDate.Month, aDay00.Value);
					if (aDay01 != null)
						dateTime2 = new PXDateTime(aBeforeDate.Year, aBeforeDate.Month, aDay01.Value);
					if (useBoth)
					{
						Int32 Day00 = (Int32)aDay00;
						Int32 Day01 = (Int32)aDay01;
						Utilities.SwapIfGreater(ref dateTime1, ref dateTime2);
						Utilities.SwapIfGreater(ref Day00, ref Day01);
						if (aBeforeDate >= dateTime2)
							statementDate = dateTime2;
						else
						{
							if(aBeforeDate >= dateTime1)
								statementDate = dateTime1;
							else
								statementDate = PXDateTime.DatePlusMonthSetDay(dateTime2, -1, Day01);
						}
					}
					else
					{
						DateTime dt = (dateTime1 != DateTime.MinValue) ? dateTime1 : dateTime2;
						if (dt != DateTime.MinValue)
						{
							statementDate = getPrevDate(dt, statementDate, aDay00 ?? aDay01 ?? 1);
						}
					}
					break;
				case ARStatementScheduleType.EndOfPeriod:
					try
					{
						IFinPeriodRepository finPeriodRepository = graph.GetService<IFinPeriodRepository>();

						string dateFinancialPeriod = finPeriodRepository.GetPeriodIDFromDate(aBeforeDate, FinPeriod.organizationID.MasterValue);
						DateTime periodEndDate = finPeriodRepository.PeriodEndDate(dateFinancialPeriod, FinPeriod.organizationID.MasterValue);

						if (periodEndDate.Date == aBeforeDate.Date)
						{
							return periodEndDate;
						}
						else
						{
							string previousFinancialPeriod = finPeriodRepository.GetOffsetPeriodId(
								dateFinancialPeriod, -1, FinPeriod.organizationID.MasterValue);

							return finPeriodRepository.PeriodEndDate(previousFinancialPeriod, FinPeriod.organizationID.MasterValue);
						}
					}
					catch (PXFinPeriodException exception)
					{
						throw new PXFinPeriodException(
							$"{PXLocalizer.Localize(Messages.UnableToCalculateNextStatementDateForEndOfPeriodCycle)} {exception.MessageNoPrefix}");
					}
				case ARStatementScheduleType.Weekly:
					if ((int)aBeforeDate.DayOfWeek == dayOfWeek)
					{
						return aBeforeDate;
					}
					else
					{
						DateTime resultDate = aBeforeDate;

						while ((int)resultDate.DayOfWeek != dayOfWeek)
						{
							resultDate = resultDate.AddDays(-1);
						}

						return resultDate;
					}
				default:
					throw new PXException(
						Messages.UnsupportedStatementScheduleType,
						GetLabel.For<ARStatementScheduleType>(aPrepareOn));
			}
			return statementDate;
		}

		public static DateTime FindNextStatementDate(PXGraph graph, DateTime aBusinessDate, ARStatementCycle aCycle) 
		{
			DateTime? result = CalcStatementDateBefore(
				graph,
				aBusinessDate, 
				aCycle.PrepareOn, 
				aCycle.Day00, 
				aCycle.Day01,
				aCycle.DayOfWeek);

			if (aCycle.LastStmtDate.HasValue && result <= aCycle.LastStmtDate)
			{
				result = CalcNextStatementDate(
					graph,
					aCycle.LastStmtDate.Value, 
					aCycle.PrepareOn, 
					aCycle.Day00, 
					aCycle.Day01,
					aCycle.DayOfWeek);
			}

			return result.HasValue ? result.Value : aBusinessDate;
		}

		public static DateTime FindNextStatementDateAfter(PXGraph graph, DateTime aBusinessDate, ARStatementCycle aCycle)
		{
			DateTime? result = null;
			if (aCycle.LastStmtDate.HasValue)
			{
				result = CalcNextStatementDate(
					graph,
					aCycle.LastStmtDate.Value, 
					aCycle.PrepareOn, 
					aCycle.Day00, 
					aCycle.Day01,
					aCycle.DayOfWeek);

				if (result >= aBusinessDate)
				{
					return result.Value;
				}
			}

			result = CalcStatementDateBefore(
				graph,
				aBusinessDate, 
				aCycle.PrepareOn, 
				aCycle.Day00, 
				aCycle.Day01,
				aCycle.DayOfWeek);

			do
			{
				result = CalcNextStatementDate(
					graph,
					result.Value, 
					aCycle.PrepareOn, 
					aCycle.Day00, 
					aCycle.Day01,
					aCycle.DayOfWeek);
			}
			while (result != null && result < aBusinessDate);

			return result.Value;
		}

		protected static DateTime getNextDate(DateTime aGuessDate, DateTime aLastStatementDate, int Day)
		{
			return (aLastStatementDate < aGuessDate) ? aGuessDate : PXDateTime.DatePlusMonthSetDay(aGuessDate, 1, Day);
		}

		protected static DateTime getPrevDate(DateTime aGuessDate, DateTime aBeforeDate, int Day)
		{
			return (aGuessDate < aBeforeDate) ? aGuessDate : PXDateTime.DatePlusMonthSetDay(aGuessDate, -1, Day);
		}

		#endregion
	}

	public static class Utilities
	{
		public static void SwapIfGreater<T>(ref T lhs, ref T rhs) where T : System.IComparable<T>
		{
			T temp;
			if (lhs.CompareTo(rhs) > 0)
			{
				temp = lhs;
				lhs = rhs;
				rhs = temp;
			}
		}
	}
}
