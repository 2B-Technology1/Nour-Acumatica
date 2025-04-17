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
	/// <exclude/>
	public class StraightLineFullPeriodMethod : StraightLineMethodBase
	{
		protected override string[] ApplicableAveragingConventions { get; } = { FAAveragingConvention.FullPeriod };

		protected override SortedDictionary<string, SortedDictionary<string, FADepreciationScheduleItem>> Calculate()
		{
			SortedDictionary<string, SortedDictionary<string, FADepreciationScheduleItem>> depreciationSchedule = new SortedDictionary<string, SortedDictionary<string, FADepreciationScheduleItem>>();

			foreach (FAAddition addition in CalculationParameters.Additions)
			{
				SortedDictionary<string, FADepreciationScheduleItem> depreciationAdditionSchedule = new SortedDictionary<string, FADepreciationScheduleItem>();
				SLMethodAdditionParameters additionParameters = CalculateAdditionParameters(CalculationParameters, addition);
				List<string> periods = GetAdditionPeriods(additionParameters.DepreciateFromPeriodID, additionParameters.DepreciateToPeriodID);

				decimal periodDepreciationAmount = additionParameters.DepreciationBasis / periods.Count;
				foreach (string period in periods)
				{
					if (string.CompareOrdinal(period, CalculationParameters.MaxDepreciateToPeriodID) > 0) break;

					if (depreciationAdditionSchedule.TryGetValue(period, out FADepreciationScheduleItem scheduleItem))
					{
						scheduleItem.DepreciationAmount += periodDepreciationAmount;
					}
					else
					{
						depreciationAdditionSchedule[period] = new FADepreciationScheduleItem
						{
							FinPeriodID = period,
							DepreciationAmount = periodDepreciationAmount
						};
					}
				}

				depreciationSchedule[addition.PeriodID] = depreciationAdditionSchedule;
			}

			return depreciationSchedule;
		}

		public static DateTime AddUsefulLifeToDate(DateTime date, decimal usefulLife)
		{
			return DeprCalcParameters.GetDatePlusYears(date, usefulLife);
		}

		private SLMethodAdditionParameters CalculateAdditionParameters(
			CalculationParameters calculationData,
			FAAddition addition)
		{
			FABookBalance bookBalance = calculationData.BookBalance;

			CheckParametersContracts();
			int assetID = calculationData.AssetID;
			int bookID = calculationData.BookID;

			SLMethodAdditionParameters parameters = new SLMethodAdditionParameters
			{
				DepreciationBasis = addition.DepreciationBasis,
				PlacedInServiceDate = (DateTime)bookBalance.DeprFromDate
			};

			addition.CalculatedAdditionParameters = parameters;

			FABookPeriod additionPeriod = IncomingParameters.RepositoryHelper.FindFABookPeriodOfDate(
				addition.IsOriginal
					? parameters.PlacedInServiceDate
					: addition.Date,
				bookID,
				assetID);
			parameters.DepreciateFromDate = (DateTime)additionPeriod?.StartDate.Value;
			parameters.DepreciateFromPeriodID = additionPeriod.FinPeriodID;

			DateTime depreciateToDate = AddUsefulLifeToDate(parameters.DepreciateFromDate, bookBalance.UsefulLife.Value);
			parameters.DepreciateToPeriodID = IncomingParameters.RepositoryHelper.GetFABookPeriodIDOfDate(depreciateToDate, bookID, assetID);

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
		}
	}
}
