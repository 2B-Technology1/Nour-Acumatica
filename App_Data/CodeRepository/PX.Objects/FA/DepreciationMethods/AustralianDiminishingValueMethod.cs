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
using PX.Objects.GL.FinPeriods;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FA.DepreciationMethods
{
	public class AustralianDiminishingValueMethod : DepreciationMethodBase
	{
		private const int DaysInYear = 365;
		private readonly bool DepreciateInDisposalPeriod;

		protected override string CalculationMethod => FADepreciationMethod.depreciationMethod.AustralianDiminishingValue;
		protected override string[] ApplicableAveragingConventions { get; } = null;

		public AustralianDiminishingValueMethod(FASetup faSetup)
		{
			DepreciateInDisposalPeriod = faSetup.DepreciateInDisposalPeriod == true;
		}

		protected override void ApplyDispose(SortedDictionary<string, SortedDictionary<string, FADepreciationScheduleItem>> schedules)
		{
			if (IncomingParameters.Details.DisposalDate == null)
				return;

			FABookPeriod disposalPeriod = IncomingParameters.PeriodDepreciationUtils.FindFABookPeriodOfDate(IncomingParameters.Details.DisposalDate);
			if (CalculationParameters.MaxDepreciateToPeriodID != disposalPeriod.FinPeriodID)
				return;

			string fisrtPeriodOfDesopsalFinYear = FinPeriodUtils.GetFirstFinPeriodIDOfYear(disposalPeriod.FinPeriodID);

			foreach (FAAddition addition in CalculationParameters.Additions)
			{
				SortedDictionary<string, FADepreciationScheduleItem> additionSchedule = schedules[addition.PeriodID];
				AustralianDiminishingValueAdditionParameters additionParameters = addition.CalculatedAdditionParameters as AustralianDiminishingValueAdditionParameters;
				FADepreciationScheduleItem disposalItem = additionSchedule[disposalPeriod.FinPeriodID];

				if (!DepreciateInDisposalPeriod)
				{
					disposalItem.DepreciationAmount = 0;
					return;
				}

				decimal accumulatedDepretiation = 0;

				foreach (KeyValuePair<string, FADepreciationScheduleItem> item in additionSchedule)
				{
					if (string.CompareOrdinal(item.Key, fisrtPeriodOfDesopsalFinYear) >= 0) break;
					accumulatedDepretiation += item.Value.DepreciationAmount;
				}

				DateTime lastDayOfDisposalPeriod = disposalPeriod.EndDate.Value.AddDays(-1);
				double totalDaysInDisposalPeriod = (disposalPeriod.EndDate - disposalPeriod.StartDate).Value.TotalDays;
				decimal daysHeldInDisposalPeriod = (decimal)(totalDaysInDisposalPeriod - (lastDayOfDisposalPeriod - IncomingParameters.Details.DisposalDate.Value).TotalDays);

				disposalItem.DepreciationAmount = (additionParameters.DepreciationBasis - accumulatedDepretiation) * CalculationParameters.BookBalance.PercentPerYear.Value
													* daysHeldInDisposalPeriod / DaysInYear / 100;
			}
		}

		protected override SortedDictionary<string, SortedDictionary<string, FADepreciationScheduleItem>> Calculate()
		{
			SortedDictionary<string, SortedDictionary<string, FADepreciationScheduleItem>> depreciationSchedule = new SortedDictionary<string, SortedDictionary<string, FADepreciationScheduleItem>>();

			foreach (FAAddition addition in CalculationParameters.Additions)
			{
				SortedDictionary<string, FADepreciationScheduleItem> depreciationAdditionSchedule = new SortedDictionary<string, FADepreciationScheduleItem>();
				AustralianDiminishingValueAdditionParameters additionParameters = CalculateAdditionParameters(CalculationParameters, addition);
				string lastDeprPeriodWithSuspend = IncomingParameters.PeriodDepreciationUtils
													.AddPeriodsCountToPeriod(additionParameters.DepreciateToPeriodID, IncomingParameters.SuspendedPeriodsIDs.Count);
				List<string> periods = GetAdditionPeriods(additionParameters.DepreciateFromPeriodID, lastDeprPeriodWithSuspend);

				decimal accumulatedDepretiation = 0;
				decimal depriciationBaseAmount = additionParameters.DepreciationBasis;

				foreach (string period in periods)
				{
					if (string.CompareOrdinal(period, CalculationParameters.MaxDepreciateToPeriodID) > 0) break;

					FABookPeriod currentPeriod = IncomingParameters.PeriodDepreciationUtils.BookPeriods[period];
					decimal daysHeldInCurrentPeriod = 0;

					if (IsFirstPeriod(period))
					{
						depriciationBaseAmount = additionParameters.DepreciationBasis - accumulatedDepretiation;
					}

					if (period == additionParameters.DepreciateFromPeriodID)
					{
						daysHeldInCurrentPeriod = (decimal)(additionParameters.DepreciateFromPeriod.EndDate.Value - additionParameters.DepreciateFromDate).TotalDays;
					}
					else if (period == additionParameters.DepreciateToPeriodID)
					{
						DateTime lastDayOfPeriod = additionParameters.DepreciateToPeriod.EndDate.Value.AddDays(-1);
						double totalDaysInPeriod = (additionParameters.DepreciateToPeriod.EndDate - additionParameters.DepreciateToPeriod.StartDate).Value.TotalDays;
						daysHeldInCurrentPeriod = (decimal)(totalDaysInPeriod - (lastDayOfPeriod - additionParameters.DepreciateToDate).TotalDays);
					}
					else
					{
						daysHeldInCurrentPeriod = (decimal)(currentPeriod.EndDate.Value - currentPeriod.StartDate.Value).TotalDays;
					}

					decimal currentPeriodDepreciationAmount = depriciationBaseAmount * CalculationParameters.BookBalance.PercentPerYear.Value
																* daysHeldInCurrentPeriod / DaysInYear / 100;

					if (IncomingParameters.SuspendedPeriodsIDs.Contains(period))
					{
						currentPeriodDepreciationAmount = 0;
					}

					accumulatedDepretiation += currentPeriodDepreciationAmount;

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

		private AustralianDiminishingValueAdditionParameters CalculateAdditionParameters(
			CalculationParameters calculationData,
			FAAddition addition)
		{
			FABookBalance bookBalance = calculationData.BookBalance;

			CheckParametersContracts();
			int assetID = calculationData.AssetID;
			int bookID = calculationData.BookID;

			AustralianDiminishingValueAdditionParameters parameters = new AustralianDiminishingValueAdditionParameters
			{
				DepreciationBasis = addition.Amount * addition.BusinessUse
			};

			addition.CalculatedAdditionParameters = parameters;

			FABookPeriod additionPeriod = IncomingParameters.RepositoryHelper.FindFABookPeriodOfDate(
				addition.IsOriginal
					? bookBalance.DeprFromDate.Value
					: addition.Date,
				bookID,
				assetID);

			parameters.DepreciateFromDate = bookBalance.DeprFromDate.Value;
			parameters.DepreciateFromPeriodID = additionPeriod.FinPeriodID;

			parameters.IsFirstAddition = addition.IsOriginal;
			parameters.DepreciateToDate = DeprCalcParameters.GetDatePlusYears(parameters.DepreciateFromDate, bookBalance.UsefulLife.Value);
			parameters.DepreciateToPeriodID = IncomingParameters.RepositoryHelper.GetFABookPeriodIDOfDate(parameters.DepreciateToDate, bookID, assetID);

			parameters.DepreciateFromPeriod = IncomingParameters.RepositoryHelper.FindByKey(bookID, IncomingParameters.OrganizationID, parameters.DepreciateFromPeriodID);
			parameters.DepreciateToPeriod = IncomingParameters.RepositoryHelper.FindByKey(bookID, IncomingParameters.OrganizationID, parameters.DepreciateToPeriodID);

			if (!parameters.IsFirstAddition
				&& IncomingParameters.RepositoryHelper.GetFABookPeriodIDOfDate(parameters.DepreciateFromDate, bookID, assetID) != parameters.DepreciateFromPeriodID)
			{
				parameters.DepreciateFromDate = parameters.DepreciateFromPeriod.StartDate.Value;
			}

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
	}
}
