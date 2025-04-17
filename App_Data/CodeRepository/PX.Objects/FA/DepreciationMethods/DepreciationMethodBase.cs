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
using PX.Objects.GL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FA.DepreciationMethods
{
	/// <exclude/>
	public abstract class DepreciationMethodBase
	{
		public class FADepreciationScheduleItem
		{
			public string FinPeriodID { get; set; }
			public decimal DepreciationAmount { get; set; }
		}

		protected abstract string CalculationMethod { get; }

		protected abstract string[] ApplicableAveragingConventions { get; }

		public bool IsStraightLine => CalculationMethod == FADepreciationMethod.depreciationMethod.StraightLine;
		public ICollection<FADepreciationScheduleItem> CalculateDepreciation(string maxPeriodID = null)
		{
			if(IncomingParameters.RepositoryHelper.FindFABookPeriodOfDate(
				IncomingParameters.BookBalance.DeprFromDate,
				IncomingParameters.BookID,
				IncomingParameters.AssetID).FinPeriodID != IncomingParameters.BookBalance.DeprFromPeriod)
			{
				throw new PXException(Messages.DeprFromDateNotMatchDeprFromPeriod,
					IncomingParameters.BookBalance.DeprFromDate,
					FinPeriodIDFormattingAttribute.FormatForError(IncomingParameters.BookBalance.DeprFromPeriod),
					IncomingParameters.FixedAsset.AssetCD);
			}			

			return CalculateDepreciation(new CalculationParameters(IncomingParameters, maxPeriodID));
		}

		protected abstract SortedDictionary<string, SortedDictionary<string, FADepreciationScheduleItem>> Calculate();
		public ICollection<FADepreciationScheduleItem> CalculateDepreciation(CalculationParameters parameters)
		{
			CalculationParameters = parameters;
			SortedDictionary<string, SortedDictionary<string, FADepreciationScheduleItem>> schedules = Calculate();

			ApplySuspend(schedules);
			ApplyDispose(schedules);

			return Summarize(schedules);
		}

		/// <summary>
		/// Apply suspension to whole asset schedule which consists all additions calculated depreciation schedules
		/// Method gets calculated depreciation schedules and modify each of them
		/// If you need to apply another suspension algorithm after calculation - you should override this method.
		/// </summary>
		/// <param name="schedules">Calculated depreciation schedules</param>
		protected virtual void ApplySuspend(SortedDictionary<string, SortedDictionary<string, FADepreciationScheduleItem>> schedules)
		{
			foreach (var scheduleAddition in schedules.Values)
			{
				SortedDictionary<string, FADepreciationScheduleItem> scheduleWithSuspend = ApplySuspendedPeriodsToSchedule(scheduleAddition);
			}
		}

		/// <summary>
		/// Apply method-specific disposing rule to calculated depreciation schedules
		/// Method gets calculated depreciation data and modifies it as required by the disposal logic
		/// If you need to apply disposing after suspension - you should override this method.
		/// </summary>
		/// <param name="schedules">Calculated depreciation schedules</param>
		protected abstract void ApplyDispose(SortedDictionary<string, SortedDictionary<string, FADepreciationScheduleItem>> schedules);

		private ICollection<FADepreciationScheduleItem> Summarize(SortedDictionary<string, SortedDictionary<string, FADepreciationScheduleItem>> schedules)
		{
			SortedDictionary<string, FADepreciationScheduleItem> resultSchedule = new SortedDictionary<string, FADepreciationScheduleItem>();

			foreach (SortedDictionary<string, FADepreciationScheduleItem> schedule in schedules.Values)
			{
				foreach (FADepreciationScheduleItem item in schedule.Values)
				{
					if (resultSchedule.TryGetValue(item.FinPeriodID, out FADepreciationScheduleItem scheduleItem))
					{
						scheduleItem.DepreciationAmount += item.DepreciationAmount;
					}
					else
					{
						resultSchedule[item.FinPeriodID] = new FADepreciationScheduleItem
						{
							FinPeriodID = item.FinPeriodID,
							DepreciationAmount = item.DepreciationAmount
						};
					}
				}
			}

			return resultSchedule.Values;
		}

		public IncomingCalculationParameters IncomingParameters { get; private set; }
		public CalculationParameters CalculationParameters { get; set; }

		public bool IsSuitable(IncomingCalculationParameters incomingParameters)
		{
			IncomingParameters = incomingParameters;
			return CalculationMethod == incomingParameters.CalculationMethod
				&& (ApplicableAveragingConventions == null || ApplicableAveragingConventions.Contains(incomingParameters.AveragingConvention));
		}

		/// <summary>
		/// Apply suspension to single addition in calculated schedules
		/// Method gets depreciation schedule items and modify each of them as described in https://wiki.acumatica.com/display/SPEC/Asset+Suspension
		/// If you need to apply another suspension method after calculation - you should override this method.
		/// </summary>
		/// <param name="depreciationScheduleItems">Calculated depreciation items</param>
		protected virtual SortedDictionary<string, FADepreciationScheduleItem> ApplySuspendedPeriodsToSchedule(SortedDictionary<string, FADepreciationScheduleItem> depreciationScheduleItems)
		{
			if (depreciationScheduleItems.Count == 0)
			{
				return depreciationScheduleItems;
			}

			List<FADepreciationScheduleItem> resultScheduleItems = depreciationScheduleItems.Values.ToList();
			FADepreciationScheduleItem firstPeriod = resultScheduleItems.First();

			if (IncomingParameters.SuspendedPeriodsIDs.Count > 0)
			{
				List<(string, string)> suspendSections = GetSuspendSectionsForAddition(firstPeriod.FinPeriodID);
				if (suspendSections.Count == 0)
					return depreciationScheduleItems;

				resultScheduleItems.Reverse();
				List<FADepreciationScheduleItem> scheduleItemsToRemove = new List<FADepreciationScheduleItem>();

				foreach (FADepreciationScheduleItem scheduleItem in resultScheduleItems)
				{
					foreach ((string, string) suspendSection in suspendSections)
					{
						if (string.CompareOrdinal(scheduleItem.FinPeriodID, suspendSection.Item1) < 0)
							break;

						int sectionLength = IncomingParameters.PeriodDepreciationUtils.GetNumberOfPeriodsBetweenPeriods(suspendSection.Item2, suspendSection.Item1) + 1;
						string newFinPeriodID = IncomingParameters.PeriodDepreciationUtils.AddPeriodsCountToPeriod(scheduleItem.FinPeriodID, sectionLength);
						scheduleItem.FinPeriodID = newFinPeriodID;

						if (string.CompareOrdinal(newFinPeriodID, CalculationParameters.MaxDepreciateToPeriodID) > 0)
							scheduleItemsToRemove.Add(scheduleItem);
					}
				}

				foreach (FADepreciationScheduleItem scheduleItem in scheduleItemsToRemove)
				{
					resultScheduleItems.Remove(scheduleItem);
				}

				if (!resultScheduleItems.Where(scheduleItem => scheduleItem.FinPeriodID == IncomingParameters.BookBalance.DeprFromPeriod).Any())
					resultScheduleItems.Add(new FADepreciationScheduleItem()
					{
						FinPeriodID = IncomingParameters.BookBalance.DeprFromPeriod
					});
			}

			depreciationScheduleItems.Clear();
			foreach (FADepreciationScheduleItem item in resultScheduleItems)
				depreciationScheduleItems.Add(item.FinPeriodID, item);

			return depreciationScheduleItems;
		}


		private List<(string, string)> GetSuspendSectionsForAddition(string startPeriodID)
		{
			return IncomingParameters.SuspendSections.Where(section => string.CompareOrdinal(section.Item2, startPeriodID) >= 0)
			.Select(section =>
			(string.CompareOrdinal(startPeriodID, section.Item1) > 0 ? startPeriodID : section.Item1, section.Item2))
			.ToList();
		}
	}
}
