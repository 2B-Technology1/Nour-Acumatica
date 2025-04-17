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
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods.TableDefinition;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FA.DepreciationMethods.Parameters
{
	/// <exclude/>
	public class IncomingCalculationParameters
	{
		public PXGraph Graph;

		public FixedAsset FixedAsset;
		public FADetails Details;
		public FABookBalance BookBalance;
		public FADepreciationMethod Method;
		public List<FAAddition> Additions;
		public SortedSet<string> SuspendedPeriodsIDs;
		public List<(string, string)> SuspendSections;


		public int Precision { get; set; }

		public string CalculationMethod => Method?.DepreciationMethod;
		public string AveragingConvention => BookBalance?.AveragingConvention;

		public int? BookID => BookBalance?.BookID;
		public int? AssetID => BookBalance?.AssetID;
		public int? OrganizationID => RepositoryHelper.IsPostingFABook(BookID)
			? (int)PXAccess.GetParentOrganizationID(FixedAsset.BranchID)
			: FinPeriod.organizationID.MasterValue;

		private IFABookPeriodRepository repositoryHelper;
		public IFABookPeriodRepository RepositoryHelper
		{
			get
			{
				repositoryHelper = repositoryHelper ?? Graph.GetService<IFABookPeriodRepository>();
				return repositoryHelper;
			}
		}

		private IFABookPeriodUtils utilsHelper;

		public IFABookPeriodUtils UtilsHelper
		{
			get
			{
				utilsHelper = utilsHelper ?? Graph.GetService<IFABookPeriodUtils>();
				return utilsHelper;
			}
		}

		public FABookPeriodDepreciationUtils PeriodDepreciationUtils { get; }

		public IncomingCalculationParameters(PXGraph graph, FABookBalance bookBalance)
		{
			Graph = graph;
			BookBalance = bookBalance;

			FixedAsset = PXSelect<FixedAsset, 
					Where<FixedAsset.assetID, Equal<Required<FixedAsset.assetID>>>>
				.Select(Graph, AssetID);

			Details = PXSelect<FADetails, 
				Where<FADetails.assetID, Equal<Required<FADetails.assetID>>>>
				.Select(Graph, AssetID);

			Method = PXSelect<FADepreciationMethod,
				Where<FADepreciationMethod.methodID, Equal<Required<FADepreciationMethod.methodID>>>>
				.Select(Graph, BookBalance.DepreciationMethodID);

			Precision = (int)SelectFrom<Currency>
				.InnerJoin<Company>
					.On<Company.baseCuryID.IsEqual<Currency.curyID>>
				.View
				.Select(Graph)
				.RowCast<Currency>()
				.FirstOrDefault()
				.DecimalPlaces;

			PeriodDepreciationUtils = new FABookPeriodDepreciationUtils(this);

			SuspendedPeriodsIDs = new SortedSet<string>(SelectFrom<FABookHistory>
				.Where<FABookHistory.assetID.IsEqual<@P.AsInt>
					.And<FABookHistory.bookID.IsEqual<@P.AsInt>>
					.And<FABookHistory.suspended.IsEqual<True>>>
				.OrderBy<FABookHistory.finPeriodID.Asc>
				.View
				.Select(
					Graph,
					AssetID,
					BookID)
				.RowCast<FABookHistory>()
				.Select(history => history.FinPeriodID).ToHashSet());

			SuspendSections = CalculateSuspendSections();

			// TODO: CollectAdditions() must be invoked after AC-156072 implementation
			Additions = CollectAdditionsFromHistory();
		}

		// TODO: Must be removed in scope of AC-156072
		// Will be used CollectAdditions() instead
		private List<FAAddition> CollectAdditionsFromHistory()
		{
			List<FAAddition> additions = SelectFrom<FABookHistory>
				.Where<FABookHistory.assetID.IsEqual<@P.AsInt>
					.And<FABookHistory.bookID.IsEqual<@P.AsInt>>
					.And<FABookHistory.ptdDeprBase.IsNotEqual<decimal0>>>
				.OrderBy<FABookHistory.finPeriodID.Asc>
				.View
				.Select(Graph, AssetID, BookID)
				.RowCast<FABookHistory>()
				.Select(history => new FAAddition(
					(decimal)history.PtdDeprBase,
					history.FinPeriodID,
					CalculateAdditionDate(history.FinPeriodID, BookBalance.DeprFromDate),
					Precision,
					BookBalance.BusinessUse.Value))
				.ToList();

			if(additions.IsEmpty()) // Zero cost fixed asset
			{
				additions.Add(new FAAddition(
					0m,
					BookBalance.DeprFromPeriod,
					CalculateAdditionDate(BookBalance.DeprFromPeriod, BookBalance.DeprFromDate),
					Precision,
					BookBalance.BusinessUse.Value));
			}

			additions.FirstOrDefault()?.MarkOriginal(BookBalance);

			return additions;
		}



		private List<FAAddition> CollectAdditions()
		{
			List<FAAddition> additions = SelectFrom<FATran>
				.Where<FATran.assetID.IsEqual<@P.AsInt>
					.And<FATran.bookID.IsEqual<@P.AsInt>>
					.And<FATran.released.IsEqual<True>>
					.And<FATran.tranType.IsEqual<FATran.tranType.purchasingPlus>
					.Or<FATran.tranType.IsEqual<FATran.tranType.purchasingMinus>
						.And<FATran.origin.IsNotEqual<FARegister.origin.split>>>>>
				.View
				.Select(Graph, AssetID, BookID)
				.RowCast<FATran>()
				.Select(transaction => new FAAddition(
					(transaction.TranType == FATran.tranType.PurchasingMinus ? -1 : 1) * (transaction.TranAmt ?? 0m),
					transaction.TranPeriodID,
					CalculateAdditionDate(transaction.TranPeriodID, transaction.TranDate, BookBalance.DeprFromDate),
					Precision,
					BookBalance.BusinessUse.Value))
				.GroupBy(addition => (addition.Date, addition.PeriodID), (key, group) => new FAAddition(
					group.Sum(addition => addition.Amount),
					key.PeriodID,
					key.Date,
					Precision,
					BookBalance.BusinessUse.Value))
				.ToList();

			additions.Sort((x, y) => x.Date.CompareTo(y.Date));
			additions.FirstOrDefault()?.MarkOriginal(BookBalance);

			foreach(FATran splitReduceTransaction in SelectFrom<FATran>
				.Where<FATran.assetID.IsEqual<@P.AsInt>
					.And<FATran.bookID.IsEqual<@P.AsInt>>
					.And<FATran.released.IsEqual<True>>
					.And<FATran.tranType.IsEqual<FATran.tranType.purchasingMinus>>
					.And<FATran.origin.IsEqual<FARegister.origin.split>>>
				// DO NOT DELETE
				// Temporary commented. Uncomment after AC-149047 fix.
				//
				.AggregateTo<
					GroupBy<FATran.tranPeriodID>,
					Sum<FATran.tranAmt>>
				.View
				.Select(Graph, AssetID, BookID))
			{
				ReduceProportionally(additions, splitReduceTransaction);
			}

			CheckCollectedAdditionsInHistory(additions);

			return additions;
		}

		private DateTime MaxDate(DateTime date1, DateTime date2) => new DateTime(Math.Max(date1.Ticks, date2.Ticks));

		private DateTime CalculateAdditionDate(string tranPeriodID, DateTime? tranDate, DateTime? deprFromDate)
		{
			DateTime additionDate = MaxDate(
				RepositoryHelper.FindFABookPeriodOfDate(tranDate, BookID, AssetID).FinPeriodID == tranPeriodID
					? tranDate.Value
					: RepositoryHelper.FindOrganizationFABookPeriodByID(tranPeriodID, BookID, AssetID).StartDate.Value,
				deprFromDate.Value);

			if (RepositoryHelper.GetFABookPeriodIDOfDate(additionDate, BookID, AssetID) != tranPeriodID)
			{
				throw new PXException(Messages.DeprFromDateNotMatchPeriod, additionDate, FinPeriodIDFormattingAttribute.FormatForError(tranPeriodID), FixedAsset.AssetCD);
			}

			return additionDate;
		}

		// TODO: Must be removed in scope of AC-156072
		private DateTime CalculateAdditionDate(string periodID, DateTime? deprFromDate)
		{
			DateTime additionDate = MaxDate(
				RepositoryHelper.FindOrganizationFABookPeriodByID(periodID, BookID, AssetID).StartDate.Value,
				deprFromDate.Value);

			if (RepositoryHelper.GetFABookPeriodIDOfDate(additionDate, BookID, AssetID) != periodID)
			{
				throw new PXException(Messages.DeprFromDateNotMatchPeriod, additionDate, FinPeriodIDFormattingAttribute.FormatForError(periodID), FixedAsset.AssetCD);
			}

			return additionDate;
		}

		private void CheckCollectedAdditionsInHistory(List<FAAddition> additions)
		{
			HashSet<(string FinPeriodID, decimal Amount)> additionsByPeriod = additions.GroupBy(
				addition => addition.PeriodID,
				(finPeriodID, group) => (finPeriodID, group.Sum(a => a.Amount)))
				.ToHashSet();

			HashSet<(string FinPeriodID, decimal Amount)> additionsFromHistory = SelectFrom<FABookHistory>
				.Where<FABookHistory.assetID.IsEqual<@P.AsInt>
					.And<FABookHistory.bookID.IsEqual<@P.AsInt>>
					.And<FABookHistory.ptdDeprBase.IsNotEqual<decimal0>>>
				.View
				.Select(Graph, AssetID, BookID)
				.RowCast<FABookHistory>()
				.Select(history => (history.FinPeriodID, (decimal)history.PtdDeprBase))
				.ToHashSet();

			if (!additionsByPeriod.SetEquals(additionsFromHistory))
			{
				throw new PXException(
					Messages.FAAdditionsDontMatchHistory,
					FormatAdditions(additionsByPeriod),
					FormatAdditions(additionsFromHistory),
					FixedAsset.AssetCD);
			}
		}

		private string FormatAdditions(IEnumerable<(string FinPeriodID, decimal Amount)> additions)
		{
			return string.Join("; ", additions.Select(addition => $"{addition.FinPeriodID}: {addition.Amount}"));
		}

		private void ReduceProportionally(List<FAAddition> additions, FATran splitReduceTransaction)
		{
			List<FAAddition> affectedAdditions = additions.Where(addition => addition.PeriodID == splitReduceTransaction.TranPeriodID).ToList();
			decimal amountToReduce = affectedAdditions.Sum(addition => addition.Amount);
			decimal reducedSum = 0m;

			foreach(FAAddition addition in affectedAdditions)
			{
				decimal reducedAmount = PXRounder.Round((addition.Amount * splitReduceTransaction.TranAmt / amountToReduce) ?? 0m, Precision);
				reducedSum += reducedAmount;
				addition.Amount -= reducedAmount;
			}
			affectedAdditions.Last().Amount -= (splitReduceTransaction.TranAmt - reducedSum) ?? 0;
		}

		private List<(string, string)> CalculateSuspendSections()
		{
			List<(string, string)> suspendSections = new List<(string, string)>();

			if (SuspendedPeriodsIDs.Count > 0)
			{
				string startSectionPeriodID = SuspendedPeriodsIDs.First();
				string currentPeriodID = SuspendedPeriodsIDs.First();
				string previousPeriodID = SuspendedPeriodsIDs.First();

				foreach (string suspendedPeriodID in SuspendedPeriodsIDs)
				{
					currentPeriodID = suspendedPeriodID;
					int differenceBetweenPeriods = PeriodDepreciationUtils.GetNumberOfPeriodsBetweenPeriods(currentPeriodID, previousPeriodID);
					if (differenceBetweenPeriods > 1)
					{
						suspendSections.Add((startSectionPeriodID, previousPeriodID));
						startSectionPeriodID = currentPeriodID;
					}
					previousPeriodID = suspendedPeriodID;
				}
				suspendSections.Add((startSectionPeriodID, currentPeriodID));
			}
			suspendSections.Sort();
			return suspendSections;
		}
	}

	/// <exclude/>
	public class CalculationParameters
	{
		public int AssetID { get; set; }
		public int BookID { get; set; }

		public FABookBalance BookBalance { get; set; }

		public List<FAAddition> Additions
		{
			get;
			set;
		}
		public FAAddition OriginalAddition { get; set; }
		public string MaxDepreciateToPeriodID { get; set; }

		public CalculationParameters(IncomingCalculationParameters incomingData, string maxPeriodID = null)
		{
			if (incomingData.AssetID == null)
			{
				throw new ArgumentNullException(nameof(AssetID));
			}
			if (incomingData.BookID == null)
			{
				throw new ArgumentNullException(nameof(BookID));
			}

			AssetID = incomingData.AssetID.Value;
			BookID = incomingData.BookID.Value;

			BookBalance = incomingData.BookBalance;

			Additions = incomingData.Additions;
			OriginalAddition = Additions.First();

			MaxDepreciateToPeriodID = string.IsNullOrEmpty(maxPeriodID) || string.CompareOrdinal(BookBalance.DeprToPeriod, maxPeriodID) <= 0
				? BookBalance.DeprToPeriod
				: maxPeriodID; 
		}
	}
	/// <exclude/>
	public class FABookPeriodDepreciationUtils
	{
		IncomingCalculationParameters IncomingParameters { get; }
		public SortedDictionary<int, string> BookPeriodsIDByIndex;
		public SortedDictionary<string, int> BookPeriodsIndexByID;
		public SortedDictionary<string, FABookPeriod> BookPeriods;
		public IYearSetup YearSetup;

		public FABookPeriodDepreciationUtils(IncomingCalculationParameters incomingParameters)
		{
			IncomingParameters = incomingParameters;
			FillBookPeriodsCollection();
			YearSetup = IncomingParameters.RepositoryHelper.FindFABookYearSetup(IncomingParameters.BookID);
		}

		private void FillBookPeriodsCollection()
		{
			BookPeriodsIDByIndex = new SortedDictionary<int, string>();
			BookPeriodsIndexByID = new SortedDictionary<string, int>();
			BookPeriods = new SortedDictionary<string, FABookPeriod>();

			IEnumerable<FABookPeriod> periods = SelectFrom<FABookPeriod>
				.Where<FABookPeriod.organizationID.IsEqual<@P.AsInt>
					.And<FABookPeriod.bookID.IsEqual<@P.AsInt>>
					.And<FABookPeriod.startDate.IsNotEqual<FABookPeriod.endDate>>>
				.OrderBy<FABookPeriod.finPeriodID.Asc>
					.View
					.Select(
						IncomingParameters.Graph,
						IncomingParameters.OrganizationID,
						IncomingParameters.BookID
						)
					.RowCast<FABookPeriod>();

			int i = 0;
			foreach (FABookPeriod period in periods)
			{
				BookPeriods.Add(period.FinPeriodID, period);
				BookPeriodsIDByIndex.Add(i, period.FinPeriodID);
				BookPeriodsIndexByID.Add(period.FinPeriodID, i);
				i++;
			}
		}

		public int GetNumberOfPeriodsBetweenPeriods(string periodID1, string periodID2)
		{
			return BookPeriodsIndexByID[periodID1] - BookPeriodsIndexByID[periodID2];
		}
		public string AddPeriodsCountToPeriod(string periodID, int offset)
		{
			return BookPeriodsIDByIndex[BookPeriodsIndexByID[periodID] + offset];
		}

		public IEnumerable<string> GetPeriods(string firstPeriodID, string lastPeriodID)
		{
			return BookPeriodsIDByIndex
				.Where(period => string.CompareOrdinal(period.Value, firstPeriodID) >= 0 && string.CompareOrdinal(period.Value, lastPeriodID) <= 0)
				.Select(period => period.Value);
		}

		public FABookPeriod FindFABookPeriodOfDate(DateTime? date)
		{
			if (date == null)
				throw new ArgumentNullException(nameof(date));

			return BookPeriods.Values.Where(period => period.StartDate <= date && period.EndDate > date).FirstOrDefault();
		}

		public string GetLastFinPeriodIdOfFinYear(string finYear)
		{
			KeyValuePair<string, FABookPeriod>  max = BookPeriods.LastOrDefault(a => a.Value.FinYear == finYear);

			return  max.Key;
		}
	}
}
