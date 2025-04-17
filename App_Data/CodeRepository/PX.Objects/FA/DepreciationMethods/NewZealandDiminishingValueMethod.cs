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

using PX.Objects.FA.DepreciationMethods.Parameters;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FA.DepreciationMethods
{
	public class NewZealandDiminishingValueMethod : DepreciationMethodBase
	{
		protected override string CalculationMethod => FADepreciationMethod.depreciationMethod.NewZealandDiminishingValue;
		protected override string[] ApplicableAveragingConventions { get; } = null;

		protected override void ApplyDispose(SortedDictionary<string, SortedDictionary<string, FADepreciationScheduleItem>> schedules) {}

		protected override SortedDictionary<string, SortedDictionary<string, FADepreciationScheduleItem>> Calculate()
		{
			SortedDictionary<string, SortedDictionary<string, FADepreciationScheduleItem>> depreciationSchedule = new SortedDictionary<string, SortedDictionary<string, FADepreciationScheduleItem>>();

			foreach (FAAddition addition in CalculationParameters.Additions)
			{
				SortedDictionary<string, FADepreciationScheduleItem> depreciationAdditionSchedule = new SortedDictionary<string, FADepreciationScheduleItem>();
				NewZealandDiminishingValueAdditionParameters additionParameters = CalculateAdditionParameters(CalculationParameters, addition);
				string lastDeprPeriodWithSuspend = IncomingParameters.PeriodDepreciationUtils
													.AddPeriodsCountToPeriod(additionParameters.DepreciateToPeriodID, IncomingParameters.SuspendedPeriodsIDs.Count);
				List<string> periods = GetAdditionPeriods(additionParameters.DepreciateFromPeriodID, lastDeprPeriodWithSuspend);

				decimal accumulatedDepretiation = 0;
				decimal accumulatedDepretiationOfFinYear = 0;
				decimal depriciationBaseAmount = additionParameters.DepreciationBasis;

				decimal daysHeldInFinancialYear = 0;
				decimal periodsInFinancialYear = 0;
				decimal periodsHeldInFinancialYear = 0;
				FinYearInfo info = null;

				IYearSetup yearSetup = IncomingParameters.PeriodDepreciationUtils.YearSetup;

				Dictionary<string, FinYearInfo> map = GetFinYearInfo(periods, yearSetup);
				RecalculateYearInfoForDisposalPeriod(map, periods.Last(), yearSetup);

				foreach (string period in periods)
				{
					if (string.CompareOrdinal(period, CalculationParameters.MaxDepreciateToPeriodID) > 0) break;

					FABookPeriod currentPeriod = IncomingParameters.PeriodDepreciationUtils.BookPeriods[period];

					if ((info == null || info.FinYear != currentPeriod.FinYear) && map.TryGetValue(currentPeriod.FinYear, out info))
					{
						daysHeldInFinancialYear = info.NumberOfDaysHeldInFinYear;
						periodsInFinancialYear = info.NumberOfPeriodsInFinYear;
						periodsHeldInFinancialYear = info.NumberOfPeriodsHeldInFinYear;
					}

					if (IsFirstPeriod(period))
					{
						accumulatedDepretiationOfFinYear = 0;
						depriciationBaseAmount = additionParameters.DepreciationBasis - accumulatedDepretiation;
					}

					decimal daysHeldInCurrentPeriod = 0;
					daysHeldInCurrentPeriod = GetDaysHeldInPeriod(currentPeriod, yearSetup);

					decimal currentPeriodDepreciationAmount = 0;
					if (period == info.LastFinPeriodOfFinYear)
					{
						currentPeriodDepreciationAmount = depriciationBaseAmount * CalculationParameters.BookBalance.PercentPerYear.Value / 100
															* (periodsHeldInFinancialYear / periodsInFinancialYear)
															- accumulatedDepretiationOfFinYear;
					}
					else
					{
						currentPeriodDepreciationAmount = depriciationBaseAmount * CalculationParameters.BookBalance.PercentPerYear.Value / 100
																* (periodsHeldInFinancialYear / periodsInFinancialYear)
																* (daysHeldInCurrentPeriod / daysHeldInFinancialYear);
					}

					if (IncomingParameters.SuspendedPeriodsIDs.Contains(period))
					{
						accumulatedDepretiationOfFinYear += currentPeriodDepreciationAmount;
						currentPeriodDepreciationAmount = 0;
					}

					accumulatedDepretiation += currentPeriodDepreciationAmount;
					accumulatedDepretiationOfFinYear += currentPeriodDepreciationAmount;

					if (depreciationAdditionSchedule.TryGetValue(period, out FADepreciationScheduleItem scheduleItem))
					{
						scheduleItem.DepreciationAmount += currentPeriodDepreciationAmount;
					}
					else
					{
						depreciationAdditionSchedule[period] = new FADepreciationScheduleItem
						{
							FinPeriodID = period,
							DepreciationAmount = currentPeriodDepreciationAmount
						};
					}
				}

				depreciationSchedule[addition.PeriodID] = depreciationAdditionSchedule;
			}

			return depreciationSchedule;
		}

		private Dictionary<string, FinYearInfo> GetFinYearInfo(List<string> periods, IYearSetup yearSetup)
		{
			Dictionary<string, FinYearInfo> map = new Dictionary<string, FinYearInfo>();

			foreach (string period in periods)
			{
				FABookPeriod currentPeriod = IncomingParameters.PeriodDepreciationUtils.BookPeriods[period];
				if (map.TryGetValue(currentPeriod.FinYear, out FinYearInfo info))
				{
					info.NumberOfDaysHeldInFinYear += GetDaysHeldInPeriod(currentPeriod, yearSetup);
					info.NumberOfPeriodsHeldInFinYear += 1;
				}
				else
				{
					map[currentPeriod.FinYear] = new FinYearInfo
					{
						FinYear = currentPeriod.FinYear,
						LastFinPeriodOfFinYear = IncomingParameters.PeriodDepreciationUtils.GetLastFinPeriodIdOfFinYear(currentPeriod.FinYear),
						NumberOfPeriodsInFinYear = GetNumberOfPeriods(currentPeriod, yearSetup),
						NumberOfPeriodsHeldInFinYear = 1,
						NumberOfDaysHeldInFinYear = GetDaysHeldInPeriod(currentPeriod, yearSetup),
					};
				}
			}

			return map;
		}

		protected virtual int GetNumberOfPeriods(FABookPeriod fABookPeriod, IYearSetup yearSetup)
		{
			return yearSetup.HasAdjustmentPeriod == true ? (int)(yearSetup.FinPeriods - 1) : (int)yearSetup.FinPeriods;
		}

		private void RecalculateYearInfoForDisposalPeriod(Dictionary<string, FinYearInfo> map, string depriciateToPeriod, IYearSetup yearSetup)
		{
			if (IncomingParameters.Details.DisposalDate != null)
			{
				FABookPeriod disposalPeriod = IncomingParameters.PeriodDepreciationUtils.FindFABookPeriodOfDate(IncomingParameters.Details.DisposalDate);
				string lastFinPeriodOfDisposalFinYear = IncomingParameters.PeriodDepreciationUtils.GetLastFinPeriodIdOfFinYear(disposalPeriod.FinYear);
				string fisrtPeriodOfDesopsalFinYear = FinPeriodUtils.GetFirstFinPeriodIDOfYear(disposalPeriod.FinPeriodID);

				string maximumBoundary = string.CompareOrdinal(lastFinPeriodOfDisposalFinYear, depriciateToPeriod) < 0 ? lastFinPeriodOfDisposalFinYear : depriciateToPeriod;

				List<string> disposalFinYearPeriods = GetAdditionPeriods(fisrtPeriodOfDesopsalFinYear, maximumBoundary);

				if (map.TryGetValue(disposalPeriod.FinYear, out FinYearInfo info))
				{
					info.NumberOfDaysHeldInFinYear = 0;
					info.NumberOfPeriodsHeldInFinYear = 0;

					foreach (string period in disposalFinYearPeriods)
					{
						FABookPeriod currentPeriod = IncomingParameters.PeriodDepreciationUtils.BookPeriods[period];
						info.NumberOfDaysHeldInFinYear += GetDaysHeldInPeriod(currentPeriod, yearSetup);
						info.NumberOfPeriodsHeldInFinYear += 1;
					}
				}
			}
		}

		private decimal GetDaysHeldInPeriod(FABookPeriod fABookPeriod, IYearSetup yearSetup)
		{

			if (fABookPeriod.StartDate.Value.DayOfYear == 32 && yearSetup.PeriodType == FinPeriodType.Month)
			{
				return 28;
			}
			
			return (decimal)(fABookPeriod.EndDate.Value - fABookPeriod.StartDate.Value).TotalDays;
		}

		protected override void ApplySuspend(SortedDictionary<string, SortedDictionary<string, FADepreciationScheduleItem>> schedules)
		{
		}

		private bool IsFirstPeriod(string period)
		{
			return period.EndsWith(FinPeriodUtils.FirstPeriodOfYear);
		}

		protected List<string> GetAdditionPeriods(string fromPeriodID, string toPeriodID)
		{
			return IncomingParameters.PeriodDepreciationUtils.GetPeriods(fromPeriodID, toPeriodID).ToList();
		}

		private NewZealandDiminishingValueAdditionParameters CalculateAdditionParameters(
			CalculationParameters calculationData,
			FAAddition addition)
		{
			FABookBalance bookBalance = calculationData.BookBalance;

			CheckParametersContracts();
			int assetID = calculationData.AssetID;
			int bookID = calculationData.BookID;

			NewZealandDiminishingValueAdditionParameters parameters = new NewZealandDiminishingValueAdditionParameters
			{
				DepreciationBasis = addition.Amount * addition.BusinessUse
			};

			addition.CalculatedAdditionParameters = parameters;

			FABookPeriod additionPeriod = IncomingParameters.RepositoryHelper.FindFABookPeriodOfDate(addition.Date, bookID,assetID);
			FABookPeriod originalDepriciateFromPeriod =  IncomingParameters.RepositoryHelper.FindFABookPeriodOfDate(bookBalance.DeprFromDate.Value, bookID, assetID);

			parameters.DepreciateFromDate = additionPeriod.StartDate.Value;
			parameters.DepreciateFromPeriodID = additionPeriod.FinPeriodID;

			parameters.DepreciateToDate = DeprCalcParameters.GetDatePlusYears(originalDepriciateFromPeriod.StartDate.Value, bookBalance.UsefulLife.Value);
			parameters.DepreciateToPeriodID = IncomingParameters.RepositoryHelper.GetFABookPeriodIDOfDate(parameters.DepreciateToDate, bookID, assetID);

			return parameters;
		}

		#region Parameters Contracts
		protected void CheckParametersContracts()
		{
			FABookBalance bookBalance = CalculationParameters.BookBalance;

			if (bookBalance == null)
			{
				throw new ArgumentNullException(nameof(CalculationParameters.BookBalance));
			}
			if (bookBalance.DeprFromDate == null)
			{
				throw new ArgumentNullException(nameof(bookBalance.DeprFromDate));
			}
			if (bookBalance.BusinessUse == null)
			{
				throw new ArgumentNullException(nameof(bookBalance.BusinessUse));
			}
			if (bookBalance.SalvageAmount == null)
			{
				throw new ArgumentNullException(nameof(bookBalance.SalvageAmount));
			}
			if (bookBalance.PercentPerYear == null)
			{
				throw new ArgumentNullException(nameof(bookBalance.PercentPerYear));
			}
		}
		#endregion

		public class FinYearInfo
		{
			public string FinYear { get; set; }
			public string LastFinPeriodOfFinYear { get; set; }
			public decimal NumberOfPeriodsInFinYear { get; set; }
			public decimal NumberOfPeriodsHeldInFinYear { get; set; }
			public decimal NumberOfDaysHeldInFinYear { get; set; }
		}
	}
}
