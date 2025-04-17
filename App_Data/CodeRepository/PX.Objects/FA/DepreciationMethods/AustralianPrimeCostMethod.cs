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
using PX.Objects.FA.DepreciationMethods.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FA.DepreciationMethods
{
	/// <summary>
	/// Class implements calculation logic for Australian Prime Cost calculation method
	/// as described in https://wiki.acumatica.com/pages/viewpage.action?spaceKey=SPEC&title=AC-230055%3A+Australian+Prime+Cost+method specification
	/// </summary>
	public class AustralianPrimeCostMethod : DepreciationMethodBase
	{
		private const int DaysInYear = 365;

		protected override string CalculationMethod => FADepreciationMethod.depreciationMethod.AustralianPrimeCost;
		protected override string[] ApplicableAveragingConventions { get; } = null;

		protected override SortedDictionary<string, SortedDictionary<string, FADepreciationScheduleItem>> Calculate()
		{
			SortedDictionary<string, SortedDictionary<string, FADepreciationScheduleItem>> depreciationSchedule = new SortedDictionary<string, SortedDictionary<string, FADepreciationScheduleItem>>();

			foreach (FAAddition addition in CalculationParameters.Additions)
			{
				SortedDictionary<string, FADepreciationScheduleItem> depreciationAdditionSchedule = new SortedDictionary<string, FADepreciationScheduleItem>();
				AustralianPrimeCostAdditionParameters additionParameters = CalculateAdditionParameters(CalculationParameters, addition);

				string lastDeprPeriodWithSuspend = IncomingParameters.PeriodDepreciationUtils.AddPeriodsCountToPeriod(additionParameters.DepreciateToPeriodID, IncomingParameters.SuspendedPeriodsIDs.Count);
				List<string> periods = GetAdditionPeriods(additionParameters.DepreciateFromPeriodID, lastDeprPeriodWithSuspend);

				decimal accumulatedDepretiation = 0;
				foreach (string period in periods)
				{
					decimal currentPeriodDepreciationAmount = 0;

					FABookPeriod currentPeriod = IncomingParameters.PeriodDepreciationUtils.BookPeriods[period];
					double daysHeldInCurrentPeriod = 0;

					if (period == additionParameters.DepreciateFromPeriodID)
					{
						daysHeldInCurrentPeriod = (additionParameters.DepreciateFromPeriod.EndDate.Value - additionParameters.DepreciateFromDate).TotalDays;
					}
					else
					{
						daysHeldInCurrentPeriod = (currentPeriod.EndDate.Value - currentPeriod.StartDate.Value).TotalDays;
					}

					//currentPeriodDepreciationAmount = additionParameters.DepreciationBasis * (additionParameters.PercentPerYear / 100) * ((decimal)daysHeldInCurrentPeriod / 365);
					currentPeriodDepreciationAmount = additionParameters.DepreciationBasis * additionParameters.PercentPerYear  * (decimal)daysHeldInCurrentPeriod / DaysInYear / 100;

					if (period == additionParameters.DepreciateToPeriodID)
					{
						currentPeriodDepreciationAmount = additionParameters.DepreciationBasis - accumulatedDepretiation;
					}

					if (IncomingParameters.SuspendedPeriodsIDs.Contains(period))
					{
						currentPeriodDepreciationAmount = 0;
					}

					accumulatedDepretiation += currentPeriodDepreciationAmount;

					if (string.CompareOrdinal(period, CalculationParameters.MaxDepreciateToPeriodID) > 0) break;

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

		private AustralianPrimeCostAdditionParameters CalculateAdditionParameters(
			CalculationParameters calculationData,
			FAAddition addition)
		{
			FABookBalance bookBalance = calculationData.BookBalance;

			CheckParametersContracts();
			int assetID = calculationData.AssetID;
			int bookID = calculationData.BookID;

			AustralianPrimeCostAdditionParameters parameters = new AustralianPrimeCostAdditionParameters
			{
				DepreciationBasis = addition.DepreciationBasis,
				PlacedInServiceDate = (DateTime)bookBalance.DeprFromDate,
				PercentPerYear = bookBalance.PercentPerYear.Value
			};

			addition.CalculatedAdditionParameters = parameters;

			FABookPeriod additionPeriod = IncomingParameters.RepositoryHelper.FindFABookPeriodOfDate(
				addition.IsOriginal
					? parameters.PlacedInServiceDate
					: addition.Date,
				bookID,
				assetID);
			parameters.DepreciateFromDate = parameters.PlacedInServiceDate;

			parameters.DepreciateFromPeriodID = additionPeriod.FinPeriodID;

			parameters.DepreciateToDate = DeprCalcParameters.GetDatePlusYears(parameters.DepreciateFromDate, bookBalance.UsefulLife.Value);
			parameters.DepreciateToPeriodID = IncomingParameters.RepositoryHelper.GetFABookPeriodIDOfDate(parameters.DepreciateToDate, bookID, assetID);

			parameters.DepreciateFromPeriod = IncomingParameters.RepositoryHelper.FindByKey(bookID, IncomingParameters.OrganizationID, parameters.DepreciateFromPeriodID);
			parameters.DepreciateToPeriod = IncomingParameters.RepositoryHelper.FindByKey(bookID, IncomingParameters.OrganizationID, parameters.DepreciateToPeriodID);

			parameters.TotalDaysInFromPeriod = (parameters.DepreciateFromPeriod.EndDate - parameters.DepreciateFromPeriod.StartDate).Value.TotalDays;
			parameters.DaysHeldInFromPeriod = (parameters.DepreciateFromPeriod.EndDate.Value - parameters.DepreciateFromDate).TotalDays;

			DateTime lastDayOfPeriod = parameters.DepreciateToPeriod.EndDate.Value.AddDays(-1);
			parameters.TotalDaysInToPeriod = (parameters.DepreciateToPeriod.EndDate - parameters.DepreciateToPeriod.StartDate).Value.TotalDays;
			parameters.DaysHeldInToPeriod = parameters.TotalDaysInToPeriod - (lastDayOfPeriod - parameters.DepreciateToDate).TotalDays;

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
			if (bookBalance.UsefulLife == null)
			{
				throw new ArgumentNullException(nameof(bookBalance.UsefulLife));
			}
		}
		#endregion

		protected List<string> GetAdditionPeriods(string fromPeriodID, string toPeriodID)
		{
			return IncomingParameters.PeriodDepreciationUtils.GetPeriods(fromPeriodID, toPeriodID).ToList();
		}

		protected override void ApplyDispose(SortedDictionary<string, SortedDictionary<string, FADepreciationScheduleItem>> schedules)
		{
			if (IncomingParameters.Details.DisposalDate == null)
				return;

			FABookPeriod disposalPeriod = IncomingParameters.PeriodDepreciationUtils.FindFABookPeriodOfDate(IncomingParameters.Details.DisposalDate);
			if (CalculationParameters.MaxDepreciateToPeriodID != disposalPeriod.FinPeriodID)
				return;

			foreach (FAAddition addition in CalculationParameters.Additions)
			{
				SortedDictionary<string, FADepreciationScheduleItem> additionSchedule = schedules[addition.PeriodID];
				AustralianPrimeCostAdditionParameters additionParameters = addition.CalculatedAdditionParameters as AustralianPrimeCostAdditionParameters;
				FADepreciationScheduleItem disposalItem = additionSchedule[disposalPeriod.FinPeriodID];

				DateTime lastDayOfDisposalPeriod = disposalPeriod.EndDate.Value.AddDays(-1);
				double totalDaysInDisposalPeriod = (disposalPeriod.EndDate - disposalPeriod.StartDate).Value.TotalDays;
				double daysHeldInDisposalPeriod = totalDaysInDisposalPeriod - (lastDayOfDisposalPeriod - IncomingParameters.Details.DisposalDate.Value).TotalDays;

				disposalItem.DepreciationAmount = additionParameters.DepreciationBasis * (additionParameters.PercentPerYear / 100) * ((decimal)daysHeldInDisposalPeriod / 365);
			}
		}

		protected override void ApplySuspend(SortedDictionary<string, SortedDictionary<string, FADepreciationScheduleItem>> schedules)
		{
		}
	}
}
