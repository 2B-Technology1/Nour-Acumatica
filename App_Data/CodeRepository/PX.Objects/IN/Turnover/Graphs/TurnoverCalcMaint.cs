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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.Common.Exceptions;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.IN.Turnover
{
	public class TurnoverCalcMaint: PXGraph<TurnoverCalcMaint>
	{
		public bool MassProcessing { get; set; }

		#region Views

		public SelectFrom<INTurnoverCalc>
			.Where<
				INTurnoverCalc.branchID.IsEqual<@P.AsInt>
				.And<INTurnoverCalc.fromPeriodID.IsEqual<@P.AsString.ASCII>>
				.And<INTurnoverCalc.toPeriodID.IsEqual<@P.AsString.ASCII>>>
			.View Calc;

		public SelectFrom<INTurnoverCalcItem>
			.Where<INTurnoverCalcItem.branchID.IsEqual<INTurnoverCalc.branchID.FromCurrent>
				.And<INTurnoverCalcItem.fromPeriodID.IsEqual<INTurnoverCalc.fromPeriodID.FromCurrent>>
				.And<INTurnoverCalcItem.toPeriodID.IsEqual<INTurnoverCalc.toPeriodID.FromCurrent>>>
			.View CalcItems;

		public PXSetup<INSetup> insetup;

		#endregion

		#region Deletion

		public virtual void Delete(INTurnoverCalc calc)
		{
			using (var scope = new PXTransactionScope())
			{
				INTurnoverCalc origCalc = Calc.Select(calc.BranchID, calc.FromPeriodID, calc.ToPeriodID);
				if (origCalc == null)
					throw new RowNotFoundException(Calc.Cache, calc.BranchID, calc.FromPeriodID, calc.ToPeriodID);

				DeleteImpl(origCalc);

				scope.Complete();
			}
		}

		protected virtual void DeleteImpl(INTurnoverCalc calc)
		{
			PXDatabase.Delete<INTurnoverCalcItem>(
				new PXDataFieldRestrict<INTurnoverCalcItem.branchID>(calc.BranchID),
				new PXDataFieldRestrict<INTurnoverCalcItem.fromPeriodID>(calc.FromPeriodID),
				new PXDataFieldRestrict<INTurnoverCalcItem.toPeriodID>(calc.ToPeriodID));

			PXDatabase.Delete<INTurnoverCalc>(
				new PXDataFieldRestrict<INTurnoverCalc.branchID>(calc.BranchID),
				new PXDataFieldRestrict<INTurnoverCalc.fromPeriodID>(calc.FromPeriodID),
				new PXDataFieldRestrict<INTurnoverCalc.toPeriodID>(calc.ToPeriodID));
		}

		#endregion

		#region Calculation

		public virtual void Calculate(INTurnoverCalc calc)
		{
			var args = new TurnoverCalculationArgs
			{
				NoteID = calc.NoteID,

				BranchID = calc.BranchID,
				FromPeriodID = calc.FromPeriodID,
				ToPeriodID = calc.ToPeriodID,

				SiteID = calc.SiteID,
				ItemClassID = calc.ItemClassID
			};

			if (calc.InventoryID != null)
				args.Inventories = new int?[] { calc.InventoryID };

			Calculate(args);
		}

		public virtual void Calculate(TurnoverCalculationArgs calculationArgs)
		{
			if (PXTransactionScope.IsScoped)
			{
				CalculateImpl(calculationArgs);
			}
			else
			{
				using (var scope = new PXTransactionScope())
				{
					CalculateImpl(calculationArgs);
					scope.Complete();
				}
			}
		}

		protected virtual void CalculateImpl(TurnoverCalculationArgs calculationArgs)
		{
			if (calculationArgs.BranchID == null
				|| calculationArgs.FromPeriodID == null
				|| calculationArgs.ToPeriodID == null)
				return;

			calculationArgs.NumberOfDays = CaclulateNumberOfDays(calculationArgs.FromPeriodID, calculationArgs.ToPeriodID);

			if (calculationArgs.NumberOfDays == 0)
				return;

			INTurnoverCalc existingCalc = Calc.Select(calculationArgs.BranchID, calculationArgs.FromPeriodID, calculationArgs.ToPeriodID);
			if (existingCalc != null)
			{
				DeleteImpl(existingCalc);

				Calc.Cache.Clear();
				Calc.Cache.ClearQueryCache();
			}

			var newCalc = CreateTurnoverCalc(calculationArgs);
			Calc.Insert(newCalc);

			CreateCalcItems(calculationArgs);

			Actions.PressSave();
		}

		protected virtual INTurnoverCalc CreateTurnoverCalc(TurnoverCalculationArgs calculationArgs)
		{
			var calc = new INTurnoverCalc
			{
				NoteID = calculationArgs.NoteID,

				BranchID = calculationArgs.BranchID,
				FromPeriodID = calculationArgs.FromPeriodID,
				ToPeriodID = calculationArgs.ToPeriodID,

				IsFullCalc = calculationArgs.IsFullCalc,
				IsInventoryListCalc = calculationArgs.Inventories?.Length > 1,
				InventoryID = calculationArgs.Inventories?.Length == 1 ? calculationArgs.Inventories[0] : null,
				SiteID = calculationArgs.SiteID,
				ItemClassID = calculationArgs.ItemClassID
			};

			if (!PXAccess.FeatureInstalled<CS.FeaturesSet.kitAssemblies>())
				calc.IncludedAssembly = false;

			if (!PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>())
				calc.IncludedProduction = false;

			return calc;
		}

		protected virtual INTurnoverCalcItem CreateTurnoverCalcItem(TurnoverCalculationArgs calculationArgs, int? inventoryID, int? siteID)
		{
			var calcItem = new INTurnoverCalcItem
			{
				BranchID = calculationArgs.BranchID,
				FromPeriodID = calculationArgs.FromPeriodID,
				ToPeriodID = calculationArgs.ToPeriodID,

				InventoryID = inventoryID,
				SiteID = siteID,
			};
			return calcItem;
		}

		protected virtual void CreateCalcItems(TurnoverCalculationArgs calculationArgs)
		{
			var inventories = LoadStockItems(calculationArgs);

			foreach(var inventoryID in inventories)
			{
				var inventoryHist = LoadTurnoverCalcItemHist(calculationArgs, inventoryID)
					.OrderBy(x => x.CostSubItemID)
					.ThenBy(x => x.CostSiteID)
					.ThenBy(x => x.AccountID)
					.ThenBy(x => x.SubID)
					.ToArray();

				NormalizeInventoryHist(inventoryHist);

				CalcInventoryTurnover(calculationArgs, inventoryID, inventoryHist);
			}
		}

		protected virtual void NormalizeInventoryHist(INTurnoverCalcItemHist[] inventoryHist)
		{
			var actualBalance = new Dictionary<(int? CostSubItemID, int? CostSiteID, int? AccountID, int? SubID),(decimal? YtdQty, decimal? YtdCost)>();

			foreach (var hist in inventoryHist)
			{
				if (hist.CostHistFinPeriodID == null)
				{
					//no actual balance
					if (hist.LastActiveFinPeriodID != null)
					{
						//it is a first period, take balance from last active period
						hist.FinBegQty = hist.FinYtdQty = hist.LastFinYtdQty;
						hist.FinBegCost = hist.FinYtdCost = hist.LastFinYtdCost;
					}
					else
					{
						//find from actualBalance dictionary
						(decimal? YtdQty, decimal? YtdCost) b;
						if (!actualBalance.TryGetValue(hist, out b))
							b = (0, 0);

						hist.FinBegQty = hist.FinYtdQty = b.YtdQty;
						hist.FinBegCost = hist.FinYtdCost = b.YtdCost;
					}
				}
				actualBalance[hist] = (hist.FinYtdQty, hist.FinYtdCost);
			}
		}

		protected virtual void CalcInventoryTurnover(TurnoverCalculationArgs calculationArgs, int? inventoryID, INTurnoverCalcItemHist[] inventoryHist)
		{
			var inventoryHistDict = inventoryHist.GroupBy(b => b.SiteID);

			bool included = false;

			foreach (var balances in inventoryHistDict)
			{
				var siteBalance = new InventorySiteBalance(balances.Key);
				siteBalance.AddRange(balances);

				included |= CalcInventoryTurnover(calculationArgs, inventoryID, siteBalance);
			}

			if(!included && calculationArgs.Inventories?.Length > 1)
			{
				AddVirtualInventoryTurnover(calculationArgs, inventoryID);
			}
		}

		protected virtual bool CalcInventoryTurnover(TurnoverCalculationArgs calculationArgs, int? inventoryID, InventorySiteBalance siteBalance)
		{
			if (!siteBalance.Any())
				return false;

			var calc = Calc.Current;

			var calcItem = CreateTurnoverCalcItem(calculationArgs, inventoryID, siteBalance.SiteID);

			var firstPeriodBalance = siteBalance[calculationArgs.FromPeriodID];
			var lastPeriodBalance = siteBalance[calculationArgs.ToPeriodID];

			calcItem.BegQty = firstPeriodBalance.Hist.FinBegQty;
			calcItem.YtdQty = lastPeriodBalance.Hist.FinYtdQty;

			calcItem.BegCost = firstPeriodBalance.Hist.FinBegCost;
			calcItem.YtdCost = lastPeriodBalance.Hist.FinYtdCost;

			calcItem.SoldQty = CalcSoldQty(calc, siteBalance);
			calcItem.SoldCost = CalcSoldCost(calc, siteBalance);

			CalcItems.Cache.SetValueExt<INTurnoverCalcItem.avgQty>(calcItem, CalculateAverage(siteBalance, b => b.Hist.FinBegQty, b => b.Hist.FinYtdQty));
			CalcItems.Cache.SetValueExt<INTurnoverCalcItem.avgCost>(calcItem, CalculateAverage(siteBalance, b => b.Hist.FinBegCost, b => b.Hist.FinYtdCost));

			if (calcItem.AvgQty == 0 && calcItem.SoldQty == 0
				&& calcItem.AvgCost == 0 && calcItem.SoldCost == 0)
				return false;

			if (calcItem.AvgQty != 0)
				CalcItems.Cache.SetValueExt<INTurnoverCalcItem.qtyRatio>(calcItem, calcItem.SoldQty / calcItem.AvgQty);

			if (calcItem.AvgCost != 0)
				CalcItems.Cache.SetValueExt<INTurnoverCalcItem.costRatio>(calcItem, calcItem.SoldCost / calcItem.AvgCost);

			if (calcItem.SoldQty != 0)
				CalcItems.Cache.SetValueExt<INTurnoverCalcItem.qtySellDays>(calcItem, calcItem.AvgQty * calculationArgs.NumberOfDays / calcItem.SoldQty);

			if (calcItem.SoldCost != 0)
				CalcItems.Cache.SetValueExt<INTurnoverCalcItem.costSellDays>(calcItem, calcItem.AvgCost * calculationArgs.NumberOfDays / calcItem.SoldCost);

			CalcItems.Insert(calcItem);

			return true;
		}

		protected virtual decimal? CalcSoldQty(INTurnoverCalc calc, InventorySiteBalance siteBalance)
		{
			var soldQty = siteBalance.GetSoldQty();

			if (calc.IncludedProduction == true && calc.IncludedAssembly == true)
				soldQty += siteBalance.GetAssemblyQty();
			else if(calc.IncludedProduction == true)
				soldQty += siteBalance.GetProductionQty();
			else if(calc.IncludedAssembly == true)
				soldQty += siteBalance.GetAssemblyQty() - siteBalance.GetProductionQty();

			if (calc.IncludedIssue == true)
				soldQty += siteBalance.GetIssuedAdjustedQty();

			if (calc.IncludedTransfer == true)
				soldQty += siteBalance.GetTransferedQty();

			return soldQty;
		}

		protected virtual decimal? CalcSoldCost(INTurnoverCalc calc, InventorySiteBalance siteBalance)
		{
			decimal? soldCost = siteBalance.GetSoldCost();

			if (calc.IncludedProduction == true && calc.IncludedAssembly == true)
				soldCost += siteBalance.GetAssemblyCost();
			else if (calc.IncludedProduction == true)
				soldCost += siteBalance.GetProductionCost();
			else if (calc.IncludedAssembly == true)
				soldCost += siteBalance.GetAssemblyCost() - siteBalance.GetProductionCost();

			if (calc.IncludedIssue == true)
				soldCost += siteBalance.GetIssuedAdjustedCost();

			if (calc.IncludedTransfer == true)
				soldCost += siteBalance.GetTransferedCost();

			return soldCost;
		}

		protected virtual void AddVirtualInventoryTurnover(TurnoverCalculationArgs calculationArgs, int? inventoryID)
		{
			int? siteID = null;

			if (calculationArgs.SiteID != null)
				siteID = calculationArgs.SiteID;
			else
			{
				var siteQuery = new SelectFrom<INSite>
					.Where<INSite.active.IsEqual<True>
					.And<INSite.branchID.IsEqual<@P.AsInt>>>
					.View.ReadOnly(this);
				using (new PXFieldScope(siteQuery.View, typeof(INSite.siteID)))
					// Acuminator disable once PX1015 IncorrectNumberOfSelectParameters Correct
					siteID = ((INSite)siteQuery.SelectWindowed(0, 1, calculationArgs.BranchID))?.SiteID;
			}

			if (siteID != null)
			{
				var calcItem = CreateTurnoverCalcItem(calculationArgs, inventoryID, siteID);
				calcItem.IsVirtual = true;

				CalcItems.Insert(calcItem);
			}
		}

		#endregion

		#region Hist rows and inventories loading

		protected virtual int?[] LoadStockItems(TurnoverCalculationArgs calculationArgs)
		{
			var query = new SelectFrom<InventoryItem>
								.Where<InventoryItem.stkItem.IsEqual<True>
								.And<InventoryItem.isTemplate.IsEqual<False>>
								.And<InventoryItem.itemStatus.IsNotIn<InventoryItemStatus.unknown, InventoryItemStatus.markedForDeletion>>
								.And<Match<InventoryItem, Current<AccessInfo.userName>>>>
								.View.ReadOnly(this);
			var parameters = new List<object>();

			if (calculationArgs.ItemClassID != null)
			{
				query.WhereAnd<Where<InventoryItem.itemClassID.IsEqual<@P.AsInt>>>();
				parameters.Add(calculationArgs.ItemClassID);
			}

			if (calculationArgs.Inventories?.Length > 0)
			{
				query.WhereAnd<Where<InventoryItem.inventoryID.IsIn<@P.AsInt>>>();
				parameters.Add(calculationArgs.Inventories);
			}

			InventoryItem[] items;
			using (new PXFieldScope(query.View, typeof(InventoryItem.inventoryID)))
				items = query.SelectMain(parameters.ToArray());
			return items.Select(x => x.InventoryID).ToArray();
		}

		protected virtual INTurnoverCalcItemHist[] LoadTurnoverCalcItemHist(TurnoverCalculationArgs calculationArgs, int? inventoryID)
		{
			var query = new SelectFrom<INTurnoverCalcItemHist>
							.Where<
								INTurnoverCalcItemHist.inventoryID.IsEqual<@P.AsInt>
								.And<INTurnoverCalcItemHist.branchID.IsEqual<@P.AsInt>>
								.And<INTurnoverCalcItemHist.finPeriodID.IsGreaterEqual<@P.AsString.ASCII>>
								.And<INTurnoverCalcItemHist.finPeriodID.IsLessEqual<@P.AsString.ASCII>>>
							.View.ReadOnly(this);
			var parameters = new List<object>
			{
				inventoryID,
				calculationArgs.BranchID,
				calculationArgs.FromPeriodID,
				calculationArgs.ToPeriodID
			};

			if (calculationArgs.SiteID != null)
			{
				query.WhereAnd<Where<INTurnoverCalcItemHist.siteID.IsEqual<@P.AsInt>>>();
				parameters.Add(calculationArgs.SiteID);
			}

			var rows = query.SelectMain(parameters.ToArray());

			return rows;
		}

		#endregion

		#region Helpers

		protected virtual decimal? CalculateAverage(IEnumerable<InventorySitePeriodBalance> balances,
			Func<InventorySitePeriodBalance, decimal?> begValue,
			Func<InventorySitePeriodBalance, decimal?> ytdValue)
		{
			decimal? avg = 0;

			var first = balances.First();
			var last = balances.Last();

			foreach (var b in balances)
			{
				if (b == first)
					avg += begValue(b) / 2;
				else
					avg += begValue(b);
			}

			avg += ytdValue(last) / 2;

			avg = avg / balances.Count();

			return avg;
		}

		protected virtual int CaclulateNumberOfDays(string fromPeriodID, string toPeriodID)
		{
			var periods = new SelectFrom<MasterFinPeriod>
				.Where<MasterFinPeriod.finPeriodID.IsGreaterEqual<@P.AsString.ASCII>
					.And<MasterFinPeriod.finPeriodID.IsLessEqual<@P.AsString.ASCII>>>
				.View.ReadOnly(this)
				.SelectMain(fromPeriodID, toPeriodID);
			var days = periods.Sum(p => (int)(p.EndDate.Value - p.StartDate.Value).TotalDays);
			return days;
		}

		#endregion

		[DebuggerDisplay("Inventory {InventoryID}")]
		public class InventoryBalance
		{
			Dictionary<int?, InventorySiteBalance> _siteBalances = new Dictionary<int?, InventorySiteBalance>();

			public int? InventoryID { get; }

			public InventoryBalance(int? inventoryID)
			{
				InventoryID = inventoryID;
			}

			public void AddRange(INTurnoverCalcItemHist[] histRows)
			{
				foreach (var hist in histRows)
					Add(hist);
			}

			public void Add(INTurnoverCalcItemHist hist)
			{
				InventorySiteBalance siteBalance;
				if (!_siteBalances.TryGetValue(hist.SiteID, out siteBalance))
					_siteBalances.Add(hist.SiteID, siteBalance = new InventorySiteBalance(hist.SiteID));

				siteBalance.Add(hist);
			}
		}

		[DebuggerDisplay("Site {SiteID}")]
		public class InventorySiteBalance: IEnumerable<InventorySitePeriodBalance>
		{
			Dictionary<string, InventorySitePeriodBalance> _periodBalances = new Dictionary<string, InventorySitePeriodBalance>();

			public int? SiteID { get; }

			public InventorySiteBalance(int? siteID)
			{
				SiteID = siteID;
			}

			public bool Any() => _periodBalances.Any();

			public void AddRange(IEnumerable<INTurnoverCalcItemHist> histRows)
			{
				foreach (var hist in histRows)
					Add(hist);
			}

			public void Add(INTurnoverCalcItemHist hist)
			{
				InventorySitePeriodBalance periodBalance;
				if (!_periodBalances.TryGetValue(hist.FinPeriodID, out periodBalance))
					_periodBalances.Add(hist.FinPeriodID, periodBalance = new InventorySitePeriodBalance(hist.FinPeriodID));

				periodBalance.Add(hist);
			}

			public decimal? GetSoldQty() => _periodBalances.Values.Sum(b => (b.Hist.FinPtdQtySales ?? 0) - (b.Hist.FinPtdQtyCreditMemos ?? 0));
			public decimal? GetSoldCost() => _periodBalances.Values.Sum(b => (b.Hist.FinPtdCOGS ?? 0) - (b.Hist.FinPtdCOGSCredits ?? 0));

			public decimal? GetProductionQty() => _periodBalances.Values.Sum(b => b.Hist.FinPtdQtyAMAssemblyOut ?? 0);
			public decimal? GetProductionCost() => _periodBalances.Values.Sum(b => b.Hist.FinPtdCostAMAssemblyOut ?? 0);

			public decimal? GetAssemblyQty() => _periodBalances.Values.Sum(b => b.Hist.FinPtdQtyAssemblyOut ?? 0);
			public decimal? GetAssemblyCost() => _periodBalances.Values.Sum(b => b.Hist.FinPtdCostAssemblyOut ?? 0);

			public decimal? GetIssuedAdjustedQty() => _periodBalances.Values.Sum(b => b.Hist.FinPtdQtyIssued ?? 0);
			public decimal? GetIssuedAdjustedCost() => _periodBalances.Values.Sum(b => b.Hist.FinPtdCostIssued ?? 0);

			public decimal? GetTransferedQty() => _periodBalances.Values.Sum(b => b.Hist.FinPtdQtyTransferOut ?? 0);
			public decimal? GetTransferedCost() => _periodBalances.Values.Sum(b => b.Hist.FinPtdCostTransferOut ?? 0);

			public IEnumerator<InventorySitePeriodBalance> GetEnumerator() => _periodBalances.Values.GetEnumerator();

			IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_periodBalances.Values).GetEnumerator();

			public InventorySitePeriodBalance this[string finPeriod]
			{
				get => _periodBalances[finPeriod];
			}
		}

		[DebuggerDisplay("Period {FinPeriodID}: Qty = {_hist.FinBegQty} => {_hist.FinYtdQty}, Cost = {_hist.FinBegCost} => {_hist.FinYtdCost}, Sold = {_hist.FinPtdQtySales} items on {_hist.FinPtdCOGS}")]
		public class InventorySitePeriodBalance
		{
			INTurnoverCalcItemHist _hist;

			public string FinPeriodID { get; }

			public INTurnoverCalcItemHist Hist { get => _hist; }

			public InventorySitePeriodBalance(string finPeriodID)
			{
				FinPeriodID = finPeriodID;
			}

			public void Add(INTurnoverCalcItemHist hist)
			{
				_hist = _hist ?? new INTurnoverCalcItemHist
				{
					InventoryID = hist.InventoryID,
					SiteID = hist.SiteID
				};

				_hist.FinBegQty = (_hist.FinBegQty ?? 0) + (hist.FinBegQty ?? 0);
				_hist.FinYtdQty = (_hist.FinYtdQty ?? 0) + (hist.FinYtdQty ?? 0);

				_hist.FinBegCost = (_hist.FinBegCost ?? 0) + (hist.FinBegCost ?? 0);
				_hist.FinYtdCost = (_hist.FinYtdCost ?? 0) + (hist.FinYtdCost ?? 0);

				_hist.FinPtdQtySales = (_hist.FinPtdQtySales ?? 0) + (hist.FinPtdQtySales ?? 0);
				_hist.FinPtdCOGS = (_hist.FinPtdCOGS ?? 0) + (hist.FinPtdCOGS ?? 0);
				_hist.FinPtdQtyCreditMemos = (_hist.FinPtdQtyCreditMemos ?? 0) + (hist.FinPtdQtyCreditMemos ?? 0);
				_hist.FinPtdCOGSCredits = (_hist.FinPtdCOGSCredits ?? 0) + (hist.FinPtdCOGSCredits ?? 0);

				_hist.FinPtdQtyAMAssemblyOut = (_hist.FinPtdQtyAMAssemblyOut ?? 0) + (hist.FinPtdQtyAMAssemblyOut ?? 0);
				_hist.FinPtdCostAMAssemblyOut = (_hist.FinPtdCostAMAssemblyOut ?? 0) + (hist.FinPtdCostAMAssemblyOut ?? 0);

				_hist.FinPtdQtyAssemblyOut = (_hist.FinPtdQtyAssemblyOut ?? 0) + (hist.FinPtdQtyAssemblyOut ?? 0);
				_hist.FinPtdCostAssemblyOut = (_hist.FinPtdCostAssemblyOut ?? 0) + (hist.FinPtdCostAssemblyOut ?? 0);

				_hist.FinPtdQtyIssued = (_hist.FinPtdQtyIssued ?? 0) + (hist.FinPtdQtyIssued ?? 0);
				_hist.FinPtdCostIssued = (_hist.FinPtdCostIssued ?? 0) + (hist.FinPtdCostIssued ?? 0);

				_hist.FinPtdQtyTransferOut = (_hist.FinPtdQtyTransferOut ?? 0) + (hist.FinPtdQtyTransferOut ?? 0);
				_hist.FinPtdCostTransferOut = (_hist.FinPtdCostTransferOut ?? 0) + (hist.FinPtdCostTransferOut ?? 0);
			}
		}
	}
}
