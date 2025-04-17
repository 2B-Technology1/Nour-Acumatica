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
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN.DAC.Unbound;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.IN
{
	[TableAndChartDashboardType]
	public class INDeadStockEnq : PXGraph<INDeadStockEnq>
	{
		public PXCancel<INDeadStockEnqFilter> Cancel;
		
		public PXFilter<INDeadStockEnqFilter> Filter;

		[PXFilterable]
		public PXSelect<INDeadStockEnqResult> Result;

		public PXSetup<Company> Company;

		public INDeadStockEnq()
		{
			Result.AllowDelete = false;
			Result.AllowInsert = false;
			Result.AllowUpdate = false;
		}

		protected virtual IEnumerable result()
		{
			var filter = Filter.Current;

			if (!ValidateFilter(filter))
				return new INDeadStockEnqResult[0];

			GetStartDates(filter, out DateTime? inStockSince, out DateTime? noSalesSince);
			PXSelectBase<INSiteStatusByCostCenter> command = CreateCommand();
			var parameters = AddFilters(filter, command, inStockSince, noSalesSince);

			var singleRow = GetRowByPrimaryKeys(command, filter, inStockSince, noSalesSince);
			if (singleRow != null)
				return new INDeadStockEnqResult[] { singleRow };

			bool userSortsFilters = ValidateViewSortsFilters();

			var result = new PXDelegateResult();
			result.IsResultFiltered = !userSortsFilters;
			result.IsResultSorted = !userSortsFilters;
			int resultCounter = 0;

			foreach (PXResult<INSiteStatusByCostCenter> row in command.Select(parameters.ToArray()))
			{
				INDeadStockEnqResult newResult = MakeResult(row, inStockSince, noSalesSince);
				if (newResult == null)
					continue;

				result.Add(new PXResult<INDeadStockEnqResult, InventoryItem>(newResult, row.GetItem<InventoryItem>()));
				resultCounter++;

				if (PXView.MaximumRows > 0 && !userSortsFilters && (PXView.StartRow + PXView.MaximumRows) <= resultCounter)
					break;
			}

			return result;
		}

		protected virtual bool ValidateFilter(INDeadStockEnqFilter filter)
		{
			if (filter == null || filter.SiteID == null || filter.SelectBy == null)
				return false;

			if (filter.SelectBy == INDeadStockEnqFilter.selectBy.Days &&
				filter.InStockDays == null &&
				filter.NoSalesDays == null)
				return false;

			if (filter.SelectBy == INDeadStockEnqFilter.selectBy.Date &&
				filter.InStockSince == null &&
				filter.NoSalesSince == null)
				return false;

			return true;
		}

		protected virtual void GetStartDates(INDeadStockEnqFilter filter, out DateTime? inStockSince, out DateTime? noSalesSince)
		{
			switch (filter.SelectBy)
			{
				case INDeadStockEnqFilter.selectBy.Days:
					inStockSince = filter.InStockDays == null ? (DateTime?)null :
						GetCurrentDate().AddDays(-1 * (int)filter.InStockDays);

					noSalesSince = filter.NoSalesDays == null ? (DateTime?)null :
						GetCurrentDate().AddDays(-1 * (int)filter.NoSalesDays);
					break;
				case INDeadStockEnqFilter.selectBy.Date:
					inStockSince = filter.InStockSince;
					noSalesSince = filter.NoSalesSince;
					break;
				default:
					throw new NotImplementedException();
			}
		}

		protected virtual DateTime GetCurrentDate()
			=> Accessinfo.BusinessDate.Value.Date;

		protected virtual bool ValidateViewSortsFilters()
		{
			if ((PXView.Filters?.Length ?? 0) != 0)
				return true;

			if ((PXView.SortColumns?.Length ?? 0) != 0 &&
					(PXView.SortColumns.Length != Result.Cache.Keys.Count ||
						!PXView.SortColumns.SequenceEqual(Result.Cache.Keys, StringComparer.OrdinalIgnoreCase) ||
						PXView.Descendings?.Any(d => d != false) == true))
				return true;

			if (PXView.ReverseOrder)
				return true;

			if (PXView.Searches?.Any(v => v != null) == true)
				return true;

			return false;
		}

		protected virtual PXSelectBase<INSiteStatusByCostCenter> CreateCommand()
		{
			return new SelectFrom<INSiteStatusByCostCenter>
				.InnerJoin<InventoryItem>.On<INSiteStatusByCostCenter.FK.InventoryItem>
				.Where<INSiteStatusByCostCenter.costCenterID.IsEqual<CostCenter.freeStock>>
				.OrderBy<InventoryItem.inventoryCD.Asc>.View.ReadOnly(this);
		}

		protected virtual List<object> AddFilters(INDeadStockEnqFilter filter, PXSelectBase<INSiteStatusByCostCenter> command,
			DateTime? inStockSince, DateTime? noSalesSince)
		{
			var parameters = new List<object>();

			AddQtyOnHandFilter(command);
			AddSiteFilter(command, filter);
			AddInventoryFilter(command, filter);
			AddItemClassFilter(command, filter);
			AddNoSalesSinceFilter(command, parameters, noSalesSince);

			return parameters;
		}

		protected virtual void AddQtyOnHandFilter(PXSelectBase<INSiteStatusByCostCenter> command)
		{
			command.WhereAnd<Where<INSiteStatusByCostCenter.qtyOnHand.IsGreater<decimal0>>>();

			var fields = GetNegativePlanFields();

			// QtySOBackOrdered + QtyPOPrepared + QtySOBooked + ... < qtyOnHand
			var lastField = fields.Last();
			var whereTypes = new List<Type>() { typeof(Where<,>) };
			whereTypes.AddRange(
				fields.Where(field => field != lastField)
				.SelectMany(field => new[] { typeof(Add<,>), field }));
			whereTypes.Add(lastField);
			whereTypes.Add(typeof(Less<INSiteStatusByCostCenter.qtyOnHand>));

			var whereNegativePlansLessOnHand = BqlCommand.Compose(whereTypes.ToArray());
			command.WhereAnd(whereNegativePlansLessOnHand);
		}

		protected virtual void AddSiteFilter(PXSelectBase<INSiteStatusByCostCenter> command, INDeadStockEnqFilter filter)
		{
			if (filter.SiteID != null)
			{
				command.WhereAnd<Where<INSiteStatusByCostCenter.siteID.IsEqual<INDeadStockEnqFilter.siteID.FromCurrent>>>();
			}
		}

		protected virtual void AddInventoryFilter(PXSelectBase<INSiteStatusByCostCenter> command, INDeadStockEnqFilter filter)
		{
			if (filter.InventoryID != null)
			{
				command.WhereAnd<Where<INSiteStatusByCostCenter.inventoryID
					.IsEqual<INDeadStockEnqFilter.inventoryID.FromCurrent>>>();
			}

			command.WhereAnd<Where<InventoryItem.itemStatus.IsNotIn<
				InventoryItemStatus.markedForDeletion, InventoryItemStatus.inactive>>>();
		}

		protected virtual void AddItemClassFilter(PXSelectBase<INSiteStatusByCostCenter> command, INDeadStockEnqFilter filter)
		{
			if (filter.ItemClassID != null)
			{
				command.WhereAnd<Where<InventoryItem.itemClassID.
					IsEqual<INDeadStockEnqFilter.itemClassID.FromCurrent>>>();
			}
		}

		protected virtual void AddNoSalesSinceFilter(PXSelectBase<INSiteStatusByCostCenter> command,
			List<object> parameters, DateTime? noSalesSince)
		{
			if (noSalesSince != null)
			{
				command.WhereAnd<Where<NotExists<SelectFrom<INItemSiteHistByCostCenterD>
					.Where<INItemSiteHistByCostCenterD.siteID.IsEqual<INSiteStatusByCostCenter.siteID>
						.And<INItemSiteHistByCostCenterD.inventoryID.IsEqual<INSiteStatusByCostCenter.inventoryID>>
						.And<INItemSiteHistByCostCenterD.subItemID.IsEqual<INSiteStatusByCostCenter.subItemID>>
						.And<INItemSiteHistByCostCenterD.costCenterID.IsEqual<INSiteStatusByCostCenter.costCenterID>>
						.And<INItemSiteHistByCostCenterD.sDate.IsGreaterEqual<@P.AsDateTime>>
						.And<INItemSiteHistByCostCenterD.qtySales.IsGreater<decimal0>>>>>>();

				parameters.Add(noSalesSince);
			}
		}

		protected virtual INDeadStockEnqResult GetRowByPrimaryKeys(PXSelectBase<INSiteStatusByCostCenter> command,
			INDeadStockEnqFilter filter, DateTime? inStockSince, DateTime? noSalesSince)
		{
			if (PXView.MaximumRows == 1 && PXView.StartRow == 0 &&
				PXView.Searches?.Length == Result.Cache.Keys.Count &&
				PXView.SearchColumns.Select(sc => sc.Column)
					.SequenceEqual(Result.Cache.Keys, StringComparer.OrdinalIgnoreCase) &&
				PXView.Searches.All(k => k != null))
			{
				int startRow = 0;
				int totalRows = 0;
				var rows = command.View.Select(new object[] { filter },
					PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings,
					PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows);

				foreach (var row in rows)
				{
					if (row is PXResult)
						return MakeResult(row as PXResult<INSiteStatusByCostCenter>, inStockSince, noSalesSince);

					return MakeResult(new PXResult<INSiteStatusByCostCenter>(row as INSiteStatusByCostCenter), inStockSince, noSalesSince);
				}
			}

			return null;
		}

		protected virtual INDeadStockEnqResult MakeResult(PXResult<INSiteStatusByCostCenter> selectResult,
			DateTime? inStockSince, DateTime? noSalesSince)
		{
			INSiteStatusByCostCenter siteStatus = selectResult;
			INItemSiteHistByCostCenterD currentRow = GetCurrentHistoryRecord(siteStatus, inStockSince, noSalesSince);

			decimal deadStockQty = currentRow?.EndQty ?? 0m;
			if (deadStockQty <= 0m)
				return null;

			decimal? negativeQty = GetNegativeQty(siteStatus, inStockSince, noSalesSince);
			deadStockQty -= negativeQty ?? 0m;
			if (deadStockQty <= 0m)
				return null;

			INItemStats stats = INItemStats.PK.Find(this, siteStatus.InventoryID, siteStatus.SiteID);

			var result = new INDeadStockEnqResult()
			{
				BaseCuryID = Company.Current.BaseCuryID,
				DeadStockQty = deadStockQty,
				InStockQty = siteStatus.QtyOnHand,
				SiteID = siteStatus.SiteID,
				LastCost = stats?.LastCost,
				LastSaleDate = GetLastSaleDate(siteStatus),
				LastPurchaseDate = stats?.LastPurchaseDate,
				InventoryID = siteStatus.InventoryID,
				SubItemID = siteStatus.SubItemID
			};

			CalculateDeadStockValues(result, siteStatus, currentRow, deadStockQty);

			return result;
		}

		protected virtual INItemSiteHistByCostCenterD GetCurrentHistoryRecord(INSiteStatusByCostCenter siteStatus,
			DateTime? inStockSince, DateTime? noSalesSince)
		{
			return SelectFrom<INItemSiteHistByCostCenterD>
				.Where<INItemSiteHistByCostCenterD.siteID.IsEqual<INSiteStatusByCostCenter.siteID.FromCurrent>
					.And<INItemSiteHistByCostCenterD.inventoryID.IsEqual<INSiteStatusByCostCenter.inventoryID.FromCurrent>>
					.And<INItemSiteHistByCostCenterD.subItemID.IsEqual<INSiteStatusByCostCenter.subItemID.FromCurrent>>
					.And<INItemSiteHistByCostCenterD.costCenterID.IsEqual<INSiteStatusByCostCenter.costCenterID.FromCurrent>>
					.And<INItemSiteHistByCostCenterD.sDate.IsLessEqual<@P.AsDateTime>>>
				.OrderBy<INItemSiteHistByCostCenterD.sDate.Desc>
				.View.ReadOnly.SelectSingleBound(this, new object[] { siteStatus }, inStockSince ?? noSalesSince);
		}

		protected virtual decimal? GetNegativeQty(INSiteStatusByCostCenter siteStatus, DateTime? inStockSince, DateTime? noSalesSince)
		{
			var siteStatusCache = Caches[typeof(INSiteStatusByCostCenter)];

			decimal negativeQty = 
				GetNegativePlanFields()
				.Sum(field => 
					(decimal?)siteStatusCache.GetValue(siteStatus, field.Name)) ?? 0m;

			INItemSiteHistByCostCenterD aggregatedLastRows = SelectFrom<INItemSiteHistByCostCenterD>
				.Where<INItemSiteHistByCostCenterD.siteID.IsEqual<INSiteStatusByCostCenter.siteID.FromCurrent>
					.And<INItemSiteHistByCostCenterD.inventoryID.IsEqual<INSiteStatusByCostCenter.inventoryID.FromCurrent>>
					.And<INItemSiteHistByCostCenterD.subItemID.IsEqual<INSiteStatusByCostCenter.subItemID.FromCurrent>>
					.And<INItemSiteHistByCostCenterD.costCenterID.IsEqual<INSiteStatusByCostCenter.costCenterID.FromCurrent>>
					.And<INItemSiteHistByCostCenterD.sDate.IsGreater<@P.AsDateTime>>>
				.AggregateTo<Sum<INItemSiteHistByCostCenterD.qtyCredit>>
				.View.ReadOnly.SelectSingleBound(this, new object[] { siteStatus }, inStockSince ?? noSalesSince);

			negativeQty += aggregatedLastRows?.QtyCredit ?? 0m;

			return negativeQty;
		}

		protected virtual decimal GetPlannedNegativeQty(INSiteStatusByCostCenter siteStatus)
		{
			var siteStatusCache = Caches[typeof(INSiteStatusByCostCenter)];

			return GetNegativePlanFields()
				.Sum(field => (decimal?)siteStatusCache.GetValue(siteStatus, field.Name)) ?? 0m;
		}

		protected virtual Type[] GetNegativePlanFields()
		{
			return new Type[]
			{
				typeof(INSiteStatusByCostCenter.qtySOBackOrdered),
				typeof(INSiteStatusByCostCenter.qtySOPrepared),
				typeof(INSiteStatusByCostCenter.qtySOBooked),
				typeof(INSiteStatusByCostCenter.qtySOShipping),
				typeof(INSiteStatusByCostCenter.qtySOShipped),
				typeof(INSiteStatusByCostCenter.qtyINIssues),
				typeof(INSiteStatusByCostCenter.qtyFSSrvOrdPrepared),
				typeof(INSiteStatusByCostCenter.qtyFSSrvOrdBooked),
				typeof(INSiteStatusByCostCenter.qtyFSSrvOrdAllocated),
				typeof(INSiteStatusByCostCenter.qtyINAssemblyDemand),
				typeof(INSiteStatusByCostCenter.qtyProductionDemand)
			};
		}

		protected virtual INItemSiteHistByCostCenterD GetHistoryRecordByQuantity(INSiteStatusByCostCenter siteStatus,
			DateTime? date, decimal begQty)
		{
			return SelectFrom<INItemSiteHistByCostCenterD>
				.Where<INItemSiteHistByCostCenterD.siteID.IsEqual<INSiteStatusByCostCenter.siteID.FromCurrent>
					.And<INItemSiteHistByCostCenterD.inventoryID.IsEqual<INSiteStatusByCostCenter.inventoryID.FromCurrent>>
					.And<INItemSiteHistByCostCenterD.subItemID.IsEqual<INSiteStatusByCostCenter.subItemID.FromCurrent>>
					.And<INItemSiteHistByCostCenterD.costCenterID.IsEqual<INSiteStatusByCostCenter.costCenterID.FromCurrent>>
					.And<INItemSiteHistByCostCenterD.begQty.IsLessEqual<@P.AsDecimal>>
					.And<INItemSiteHistByCostCenterD.endQty.IsGreater<@P.AsDecimal>>
					.And<INItemSiteHistByCostCenterD.sDate.IsLessEqual<@P.AsDateTime>>>
				.OrderBy<INItemSiteHistByCostCenterD.sDate.Desc>
				.View.ReadOnly.SelectSingleBound(this, new object[] { siteStatus }, begQty, begQty, date);
		}

		protected virtual DateTime? GetLastSaleDate(INSiteStatusByCostCenter siteStatus)
		{
			INItemSiteHistByCostCenterD lastSaleRow = SelectFrom<INItemSiteHistByCostCenterD>
				.Where<INItemSiteHistByCostCenterD.siteID.IsEqual<INSiteStatusByCostCenter.siteID.FromCurrent>
					.And<INItemSiteHistByCostCenterD.inventoryID.IsEqual<INSiteStatusByCostCenter.inventoryID.FromCurrent>>
					.And<INItemSiteHistByCostCenterD.subItemID.IsEqual<INSiteStatusByCostCenter.subItemID.FromCurrent>>
					.And<INItemSiteHistByCostCenterD.costCenterID.IsEqual<INSiteStatusByCostCenter.costCenterID.FromCurrent>>
					.And<INItemSiteHistByCostCenterD.qtySales.IsGreater<decimal0>>>
				.AggregateTo<Max<INItemSiteHistByCostCenterD.sDate>>
				.View.ReadOnly.SelectSingleBound(this, new object[] { siteStatus });

			return lastSaleRow?.SDate;
		}

		protected virtual void CalculateDeadStockValues(INDeadStockEnqResult result,
			INSiteStatusByCostCenter siteStatus, INItemSiteHistByCostCenterD currentRow, decimal deadStockQty)
		{
			decimal skipQty = currentRow.EndQty.Value - deadStockQty;
			var rowByQty = GetHistoryRecordByQuantity(siteStatus, currentRow.SDate, skipQty);
			if (rowByQty == null)
			{
				OnNotEnoughHistoryRecords(siteStatus, currentRow, deadStockQty, -1m);
				return;
			}

			decimal begCost = Math.Max((rowByQty.EndCost - rowByQty.CostDebit + rowByQty.CostCredit) ?? 0m, 0m);
			decimal begQty = Math.Max(rowByQty.BegQty ?? 0m, 0m);

			decimal unitCost = ((rowByQty.EndQty ?? 0m) - begQty) == 0m ? 0m :
				((rowByQty.EndCost ?? 0m) - begCost) / ((rowByQty.EndQty ?? 0m) - begQty);

			begCost += unitCost * (skipQty - begQty);
			result.TotalDeadStockCost = currentRow.EndCost - begCost;

			decimal deadStockQtyCounter = deadStockQty;
			result.InDeadStockDays = 0m;

			IEnumerable<INItemSiteHistByCostCenterD> lastRows = GetLastHistoryRecords(siteStatus, deadStockQty, currentRow);
			foreach (INItemSiteHistByCostCenterD lastRow in lastRows)
			{
				if ((lastRow.QtyDebit ?? 0m) == 0m)
					continue;

				if (CalculateDeadStockValues(ref deadStockQtyCounter, result, lastRow))
					return;
			}

			OnNotEnoughHistoryRecords(siteStatus, currentRow, deadStockQty, deadStockQtyCounter);
		}

		protected virtual IEnumerable<INItemSiteHistByCostCenterD> GetLastHistoryRecords(INSiteStatusByCostCenter siteStatus, decimal deadStockQty,
			INItemSiteHistByCostCenterD currentRow)
		{
			const int MaxRows = 1000;

			var getRows = new SelectFrom<INItemSiteHistByCostCenterD>
				.Where<INItemSiteHistByCostCenterD.siteID.IsEqual<@P.AsInt>
					.And<INItemSiteHistByCostCenterD.inventoryID.IsEqual<@P.AsInt>>
					.And<INItemSiteHistByCostCenterD.subItemID.IsEqual<@P.AsInt>>
					.And<INItemSiteHistByCostCenterD.costCenterID.IsEqual<@P.AsInt>>
					.And<INItemSiteHistByCostCenterD.sDate.IsLess<@P.AsDateTime>>
					.And<INItemSiteHistByCostCenterD.qtyDebit.IsGreater<decimal0>>>
				.OrderBy<INItemSiteHistByCostCenterD.sDate.Desc>.View.ReadOnly(this);

			DateTime? lastDate = currentRow.SDate;
			decimal deadStockQtyCounter = deadStockQty;

			yield return currentRow;

			while (lastDate != null && deadStockQtyCounter > 0m)
			{
				// Acuminator disable once PX1015 IncorrectNumberOfSelectParameters It's acuminator issue: see jira ATR-600
				PXResultset<INItemSiteHistByCostCenterD> rows = getRows.SelectWindowed(0, MaxRows,
					siteStatus.SiteID, siteStatus.InventoryID, siteStatus.SubItemID, siteStatus.CostCenterID, lastDate);

				lastDate = null;

				foreach (var row in rows)
				{
					INItemSiteHistByCostCenterD newRow = row;
					yield return newRow;

					lastDate = newRow.SDate;

					deadStockQtyCounter -= newRow.QtyDebit ?? 0m;
					if (deadStockQtyCounter <= 0m)
						break;
				}
			}
		}

		protected virtual bool CalculateDeadStockValues(ref decimal deadStockQtyCounter,
			INDeadStockEnqResult result, INItemSiteHistByCostCenterD lastRow)
		{
			decimal qtyDebit = (decimal)lastRow.QtyDebit;
			decimal mult = (deadStockQtyCounter >= qtyDebit) ? 1m : (deadStockQtyCounter / qtyDebit);

			decimal days = (decimal)GetCurrentDate().Subtract(lastRow.SDate.Value.Date).TotalDays;
			result.InDeadStockDays += days * qtyDebit * mult;

			deadStockQtyCounter -= qtyDebit;

			if (deadStockQtyCounter <= 0m)
			{
				result.AverageItemCost = result.TotalDeadStockCost / result.DeadStockQty;
				result.InDeadStockDays /= result.DeadStockQty;
				return true;
			}

			return false;
		}

		protected virtual void OnNotEnoughHistoryRecords(INSiteStatusByCostCenter siteStatus, INItemSiteHistByCostCenterD currentRow, decimal deadStockQty, decimal deadStockQtyCounter)
		{
			PXTrace.WriteError(
				new Common.Exceptions.RowNotFoundException(Caches[typeof(INItemSiteHist)],
					siteStatus.SiteID,
					siteStatus.InventoryID,
					siteStatus.SubItemID,
					currentRow.SDate,
					deadStockQty,
					deadStockQtyCounter));
		}
	}
}
