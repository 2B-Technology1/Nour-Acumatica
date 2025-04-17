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
using PX.Objects.Common;
using PX.Objects.FA.DepreciationMethods.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FA.DepreciationMethods
{
	/// <summary>
	/// Class implements calculation logic for Strainght Line calculation method with Full Day averaging convention
	/// as described in https://wiki.acumatica.com/display/SPEC/AC-143586:+Straight+Line+method%2C+Full+Day+averaging+convention specification
	/// </summary>
	public class StraightLineFullDayMethod : StraightLineFullPeriodMethod
	{
		protected override string[] ApplicableAveragingConventions { get; } = { FAAveragingConvention.FullDay };

		protected override SortedDictionary<string, SortedDictionary<string, FADepreciationScheduleItem>> Calculate()
		{
			SortedDictionary<string, SortedDictionary<string, FADepreciationScheduleItem>> depreciationSchedule = new SortedDictionary<string, SortedDictionary<string, FADepreciationScheduleItem>>();
			double TotalNumberOfPeriods = 0;

			foreach (FAAddition addition in CalculationParameters.Additions)
			{
				SortedDictionary<string, FADepreciationScheduleItem> depreciationAdditionSchedule = new SortedDictionary<string, FADepreciationScheduleItem>();
				SLMethodFullDayAdditionParameters additionParameters = CalculateAdditionParameters(CalculationParameters, addition);
				List<string> periods = GetAdditionPeriods(additionParameters.DepreciateFromPeriodID, additionParameters.DepreciateToPeriodID);

				if (additionParameters.IsFirstAddition)
					TotalNumberOfPeriods = periods.Count
						- (additionParameters.TotalDaysInFromPeriod - additionParameters.DaysHeldInFromPeriod) / additionParameters.TotalDaysInFromPeriod
						- (additionParameters.TotalDaysInToPeriod - additionParameters.DaysHeldInToPeriod) / additionParameters.TotalDaysInToPeriod;

				decimal accumulatedDepreciation = 0;
				decimal defaultPeriodDepreciationAmount = additionParameters.DepreciationBasis / (decimal)TotalNumberOfPeriods;

				foreach (string period in periods)
				{
					decimal currentPeriodDepreciationAmount = defaultPeriodDepreciationAmount;
					if (period == additionParameters.DepreciateFromPeriodID && !additionParameters.IsFirstPeriodFull)
					{
						currentPeriodDepreciationAmount =
							additionParameters.DepreciationBasis / (decimal)TotalNumberOfPeriods
								* (decimal)(additionParameters.DaysHeldInFromPeriod / additionParameters.TotalDaysInFromPeriod);
					}
					if (period == additionParameters.DepreciateToPeriodID)
					{
						currentPeriodDepreciationAmount = additionParameters.DepreciationBasis - accumulatedDepreciation;
					}

					accumulatedDepreciation += currentPeriodDepreciationAmount;

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

		private SLMethodFullDayAdditionParameters CalculateAdditionParameters(
			CalculationParameters calculationData,
			FAAddition addition)
		{
			FABookBalance bookBalance = calculationData.BookBalance;

			CheckParametersContracts();

			int assetID = calculationData.AssetID;
			int bookID = calculationData.BookID;
			SLMethodFullDayAdditionParameters parameters = new SLMethodFullDayAdditionParameters
			{
				DepreciationBasis = addition.DepreciationBasis,
				PlacedInServiceDate = (DateTime)bookBalance.DeprFromDate
			};

			addition.CalculatedAdditionParameters = parameters;
			parameters.IsFirstAddition = addition.IsOriginal;

			FABookPeriod additionPeriod = IncomingParameters.RepositoryHelper.FindFABookPeriodOfDate(
				addition.IsOriginal
					? parameters.PlacedInServiceDate
					: addition.Date,
				bookID,
				assetID);
			parameters.DepreciateFromDate = parameters.PlacedInServiceDate;
			if (!parameters.IsFirstAddition)
				parameters.DepreciateFromDate = addition.Date;

			parameters.DepreciateFromPeriodID = additionPeriod.FinPeriodID;

			parameters.DepreciateToDate = AddUsefulLifeToDate(parameters.DepreciateFromDate, bookBalance.UsefulLife.Value);
			parameters.DepreciateToPeriodID = IncomingParameters.RepositoryHelper.GetFABookPeriodIDOfDate(parameters.DepreciateToDate, bookID, assetID);

			parameters.DepreciateFromPeriod = IncomingParameters.RepositoryHelper.FindByKey(bookID, IncomingParameters.OrganizationID, parameters.DepreciateFromPeriodID);
			parameters.DepreciateToPeriod = IncomingParameters.RepositoryHelper.FindByKey(bookID, IncomingParameters.OrganizationID, parameters.DepreciateToPeriodID);

			parameters.TotalDaysInFromPeriod = (parameters.DepreciateFromPeriod.EndDate - parameters.DepreciateFromPeriod.StartDate).Value.TotalDays;
			parameters.DaysHeldInFromPeriod = (parameters.DepreciateFromPeriod.EndDate.Value - parameters.DepreciateFromDate).TotalDays;

			DateTime lastDayOfPeriod = parameters.DepreciateToPeriod.EndDate.Value.AddDays(-1);
			parameters.TotalDaysInToPeriod = (parameters.DepreciateToPeriod.EndDate - parameters.DepreciateToPeriod.StartDate).Value.TotalDays;
			parameters.DaysHeldInToPeriod = parameters.TotalDaysInToPeriod - (lastDayOfPeriod - parameters.DepreciateToDate).TotalDays;

			parameters.IsFirstPeriodFull = parameters.DepreciateFromDate == parameters.DepreciateFromPeriod.StartDate;

			return parameters;
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
				FAAddition originalAddition = CalculationParameters.OriginalAddition;

				SLMethodFullDayAdditionParameters additionParameters = addition.CalculatedAdditionParameters as SLMethodFullDayAdditionParameters;
				SLMethodFullDayAdditionParameters originalAdditionParameters = originalAddition.CalculatedAdditionParameters as SLMethodFullDayAdditionParameters;

				double totalNumberOfPeriods = IncomingParameters.PeriodDepreciationUtils.GetNumberOfPeriodsBetweenPeriods(originalAdditionParameters.DepreciateToPeriodID, originalAdditionParameters.DepreciateFromPeriodID) + 1
					- (originalAdditionParameters.TotalDaysInFromPeriod - originalAdditionParameters.DaysHeldInFromPeriod) / originalAdditionParameters.TotalDaysInFromPeriod
					- (originalAdditionParameters.TotalDaysInToPeriod - originalAdditionParameters.DaysHeldInToPeriod) / originalAdditionParameters.TotalDaysInToPeriod;

				FADepreciationScheduleItem disposalItem = additionSchedule[disposalPeriod.FinPeriodID];

				DateTime lastDayOfDisposalPeriod = disposalPeriod.EndDate.Value.AddDays(-1);
				double totalDaysInDisposalPeriod = (disposalPeriod.EndDate - disposalPeriod.StartDate).Value.TotalDays;
				double daysHeldInDisposalPeriod = totalDaysInDisposalPeriod - (lastDayOfDisposalPeriod - IncomingParameters.Details.DisposalDate.Value).TotalDays;

				disposalItem.DepreciationAmount = additionParameters.DepreciationBasis / (decimal)totalNumberOfPeriods * (decimal)(daysHeldInDisposalPeriod / totalDaysInDisposalPeriod);
			}
		}
	}
}
