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
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated;

namespace PX.Objects.SO
{
	public class SalesAllocationProcess: PXGraph<SalesAllocationProcess>
	{
		#region Fetch IStatus

		private INSiteStatusByCostCenter FetchSiteStatus(PXGraph graph, INSiteStatusByCostCenter available)
			=> FetchStatus<INSiteStatusByCostCenter, SiteStatusByCostCenter>(graph, available);

		private TStatus FetchStatus<TStatus, TStatusAccum>(PXGraph graph, TStatus status)
			where TStatus : IStatus, IBqlTable, new()
			where TStatusAccum : IStatus, IBqlTable, new()
		{
			var statusCache = graph.Caches[typeof(TStatus)];

			status = (TStatus)statusCache.CreateCopy(status);

			var statusAccum = new TStatusAccum();
			statusCache.RestoreCopy(statusAccum, status);
			statusAccum = (TStatusAccum)graph.Caches[typeof(TStatusAccum)].Insert(statusAccum);

			status = status.Add(statusAccum);

			return status;
		}

		#endregion

		public virtual void AllocateOrders(List<SalesAllocation> allocations)
		{
			ProcessAllocations(allocations.Where(x => x.QtyToAllocate > 0).ToList(),
				ValidateOrderToAllocate, ValidateLineToAllocate, Allocate);
		}

		public virtual void DeallocateOrders(List<SalesAllocation> allocations)
		{
			ProcessAllocations(allocations.Where(x => x.QtyToDeallocate > 0).ToList(),
				ValidateOrderToDeallocate, ValidateLineToDeallocate, Deallocate);
		}

		#region Processing

		protected Dictionary<SalesAllocation, Exception> ProcessingErrors;

		protected void SetProcessingError(SalesAllocation allocation, Exception error)
		{
			if (ProcessingErrors != null)
			{
				if (ProcessingErrors.ContainsKey(allocation))
					ProcessingErrors.Remove(allocation);
				ProcessingErrors.Add(allocation, error);
			}
		}

		protected virtual void OnBeforeProcess()
		{
			ProcessingErrors = new Dictionary<SalesAllocation, Exception>();
		}

		protected virtual void OnAfterProcess()
		{
			ProcessingErrors = null;
		}

		protected delegate bool ValidateOrder(SOOrderEntry graph, SOOrder order, SalesAllocation allocation, out Exception error);
		protected delegate bool ValidateLine(SOOrderEntry graph, SOLine line, SalesAllocation allocation, out Exception error);
		protected delegate decimal? ProcessLine(SOOrderEntry graph, SOLine line, SalesAllocation allocation);

		protected virtual void ProcessAllocations(List<SalesAllocation> allocations,
			ValidateOrder validateOrder, ValidateLine validateLine, ProcessLine processLine)
		{
			OnBeforeProcess();

			var groups = allocations.GroupBy(x => (x.OrderType, x.OrderNbr));

			var graph = CreateInstance<SOOrderEntry>();
			try
			{
				foreach (var group in groups)
				{
					var order = new SOOrder
					{
						OrderType = group.Key.OrderType,
						OrderNbr = group.Key.OrderNbr
					};

					ProcessAllocations(graph, order, group.ToList(), validateOrder, validateLine, processLine);
				}

				if (ProcessingErrors?.Any() == true)
					throw new PXOperationCompletedWithErrorException();
			}
			finally
			{
				OnAfterProcess();
			}
		}

		protected virtual bool ProcessAllocations(SOOrderEntry graph, SOOrder order, List<SalesAllocation> allocations,
			ValidateOrder validateOrder, ValidateLine validateLine, ProcessLine processLine)
		{
			graph.Clear();

			Exception error = null;
			bool anyProcessed = false;

			using (var ts = new PXTransactionScope())
			{
				graph.Document.Current = graph.Document.Search<SOOrder.orderNbr>(order.OrderNbr, order.OrderType);

				if (validateOrder(graph, graph.Document.Current, allocations[0], out error))
				{
					foreach (var allocation in allocations)
					{
						if (ProcessAllocation(graph, allocation, validateLine, processLine))
							anyProcessed |= true;
					}

					if (anyProcessed)
					{
						try
						{
							graph.Save.Press();
						}
						catch (Exception ex)
						{
							error = ex;
							anyProcessed = false;
						}
					}
				}

				if (anyProcessed)
					ts.Complete();
			}

			if (error != null)
			{
				foreach (var allocation in allocations)
				{
					PXProcessing<SalesAllocation>.SetCurrentItem(allocation);
					PXProcessing<SalesAllocation>.SetError(error);
					SetProcessingError(allocation, error);
				}
				return false;
			}
			return anyProcessed;
		}

		protected virtual bool ProcessAllocation(SOOrderEntry graph, SalesAllocation allocation, ValidateLine validateLine, ProcessLine processLine)
		{
			PXProcessing<SalesAllocation>.SetCurrentItem(allocation);
			Exception error;
			try
			{
				graph.Transactions.Current = graph.Transactions.Search<SOLine.lineNbr>(allocation.LineNbr);

				if (!validateLine(graph, graph.Transactions.Current, allocation, out error))
					throw error;

				processLine(graph, graph.Transactions.Current, allocation);
			}
			catch (Exception ex)
			{
				PXProcessing<SalesAllocation>.SetError(ex);
				SetProcessingError(allocation, ex);
				return false;
			}

			PXProcessing<SalesAllocation>.SetProcessed();
			return true;
		}

		#endregion

		#region Allocation

		protected virtual bool ValidateOrderToAllocate(SOOrderEntry graph, SOOrder order, SalesAllocation allocation, out Exception error)
		{
			error = null;
			if (order == null)
				error = new PXException(ErrorMessages.RecordDeletedByAnotherProcess, graph.Document.GetItemType().Name);
			else if (!SalesAllocationsFilter.orderStatus.list.ContainsValue(order.Status)
				|| order.Hold != true && order.DontApprove != true)
			{
				error = new PXException(ErrorMessages.RecordUpdatedByAnotherProcess, graph.Document.GetItemType().Name);
			}

			return error == null;
		}

		protected virtual bool ValidateLineToAllocate(SOOrderEntry graph, SOLine line, SalesAllocation allocation, out Exception error)
		{
			error = null;
			if (line == null)
				error = new PXException(ErrorMessages.RecordDeletedByAnotherProcess, graph.Transactions.GetItemType().Name);
			else if (line.SiteID != allocation.LineSiteID || line.InventoryID != allocation.InventoryID)
				error = new PXException(ErrorMessages.RecordUpdatedByAnotherProcess, graph.Transactions.GetItemType().Name);
			else if ((line.InvtMult == 0 ? 1 : -line.InvtMult) * line.BaseOrderQty < allocation.QtyToAllocate)
				error = new PXException(Messages.QtyToAllocateGreaterQtyUnallocated);
			return error == null;
		}

		public virtual decimal? Allocate(SOOrderEntry graph, SOLine line, SalesAllocation allocation)
		{
			var lotSerClass = ReadLotSerClass(graph, line.InventoryID);
			if (lotSerClass == null || graph.LineSplittingAllocatedExt.IsAllocationEntryEnabled == false)
				return 0;

			var qtyToAllocate = allocation.QtyToAllocate;

			decimal? unallocatedQty;
			{
				var unallocatedSplits = SelectUnallocatedSplits(graph, line);
				unallocatedQty = unallocatedSplits.Sum(x => x.BaseQty);
			}
			if (unallocatedQty < qtyToAllocate)
				throw new PXException(Messages.QtyToAllocateGreaterQtyUnallocated);

			var status = INSiteStatusByCostCenter.PK.Find(graph, line.InventoryID, line.SubItemID, line.SiteID, line.CostCenterID);
			status = FetchSiteStatus(graph, status);
			if (status.QtyHardAvail < qtyToAllocate)
				throw new PXException(Messages.CantAllocateQtyToAllocateGreaterQtyAvailable, graph.Transactions.Cache.GetValueExt<SOLine.inventoryID>(line));

			if (!graph.LineSplittingExt.UseBaseUnitInSplit(line, line,
				new PXResult<InventoryItem, INLotSerClass>(InventoryItem.PK.Find(graph, line.InventoryID), lotSerClass)))
			{
				qtyToAllocate = INUnitAttribute.ConvertFromBase(graph.Transactions.Cache, line.InventoryID, line.UOM, qtyToAllocate ?? 0, INPrecision.QUANTITY);
			}

			using (graph.LineSplittingExt.SuppressedModeScope(true))
			{
				return AllocateNonLots(graph, line, qtyToAllocate);
			}
		}

		protected virtual decimal? AllocateNonLots(SOOrderEntry graph, SOLine line, decimal? qtyToAllocate)
		{
			var remainQty = qtyToAllocate;

			var unallocatedSplits = SelectUnallocatedSplits(graph, line).ToQueue();

			decimal? allocatedQty = 0;
			var splitCache = graph.splits.Cache;
			while (remainQty > 0 && unallocatedSplits.Any())
			{
				var split = unallocatedSplits.Dequeue();
				var qty = AllocateNonLots(splitCache, split, remainQty);
				allocatedQty += qty;
				remainQty -= qty;
			}

			return allocatedQty;
		}

		protected virtual decimal? AllocateNonLots(PXCache splitCache, SOLineSplit splitToAllocate, decimal? qtyToAllocate)
		{
			var splitCopy = (SOLineSplit)splitCache.CreateCopy(splitToAllocate);

			if (splitCopy.Qty <= qtyToAllocate)
			{
				splitCopy.IsAllocated = true;
				splitCache.Update(splitCopy);

				return splitCopy.Qty;
			}
			var extraQty = splitCopy.Qty - qtyToAllocate;

			splitCopy.Qty = qtyToAllocate;
			splitCopy.IsAllocated = true;
			splitCache.Update(splitCopy);

			splitCopy = (SOLineSplit)splitCache.Insert();
			splitCopy.Qty = extraQty;
			splitCopy.IsAllocated = false;
			if(splitCopy.Behavior == SOBehavior.BL)
				splitCopy.POCreate = false;
			splitCache.Update(splitCopy);

			return qtyToAllocate;
		}

		protected virtual SOLineSplit[] SelectUnallocatedSplits(SOOrderEntry graph, SOLine line)
		{
			var splittingAllocatedExt = graph.LineSplittingAllocatedExt;
			return PXParentAttribute
				.SelectChildren(graph.splits.Cache, line, graph.Transactions.GetItemType())
				.Cast<SOLineSplit>()
				.Where(s => s.IsAllocated == false && s.POReceiptNbr == null && splittingAllocatedExt.AllowToManualAllocate(line, s))
				.ToArray();
		}

		#endregion

		#region Deallocation

		protected virtual bool ValidateOrderToDeallocate(SOOrderEntry graph, SOOrder order, SalesAllocation allocation, out Exception error)
		{
			error = null;
			if (order == null)
				error = new PXException(ErrorMessages.RecordDeletedByAnotherProcess, graph.Document.GetItemType().Name);
			else if (!SalesAllocationsFilter.orderStatus.list.withExpired.ContainsValue(order.Status)
				|| order.Hold != true && order.DontApprove != true)
			{
				error = new PXException(ErrorMessages.RecordUpdatedByAnotherProcess, graph.Document.GetItemType().Name);
			}

			return error == null;
		}

		protected virtual bool ValidateLineToDeallocate(SOOrderEntry graph, SOLine line, SalesAllocation allocation, out Exception error)
		{
			error = null;
			if (line == null)
				error = new PXException(ErrorMessages.RecordDeletedByAnotherProcess, graph.Transactions.GetItemType().Name);
			else if (line.SiteID != allocation.LineSiteID || line.InventoryID != allocation.InventoryID)
				error = new PXException(ErrorMessages.RecordUpdatedByAnotherProcess, graph.Transactions.GetItemType().Name);
			else if ((line.InvtMult == 0 ? 1 : -line.InvtMult) * line.BaseOrderQty < allocation.QtyToDeallocate)
				error = new PXException(Messages.QtyToDeallocateGreaterQtyAllocated);
			return error == null;
		}

		public virtual decimal? Deallocate(SOOrderEntry graph, SOLine line, SalesAllocation allocation)
		{
			var lotSerClass = ReadLotSerClass(graph, line.InventoryID);
			if (lotSerClass == null || graph.LineSplittingAllocatedExt.IsAllocationEntryEnabled == false)
				return 0;

			var siteID = allocation.SplitSiteID;
			var qtyToDeallocate = allocation.QtyToDeallocate;

			var allocatedSplits = SelectAllocatedSplits(graph, line, siteID);
			var allocatedQty = allocatedSplits.Sum(x => x.BaseQty);
			
			if (qtyToDeallocate > allocatedQty)
				throw new PXException(Messages.QtyToDeallocateGreaterQtyAllocated);
			if (graph.Document.Current.IsExpired == true && qtyToDeallocate < allocatedQty)
				throw new PXException(Messages.CantEditQtyToDeallocateForExpiredOrderLine);

			if (!graph.LineSplittingExt.UseBaseUnitInSplit(line, line,
				new PXResult<InventoryItem, INLotSerClass>(InventoryItem.PK.Find(graph, line.InventoryID), lotSerClass)))
			{
				qtyToDeallocate = INUnitAttribute.ConvertFromBase(graph.Transactions.Cache, line.InventoryID, line.UOM, qtyToDeallocate ?? 0, INPrecision.QUANTITY);
			}

			using (graph.LineSplittingExt.SuppressedModeScope(true))
			{
				if (lotSerClass.LotSerTrack == INLotSerTrack.NotNumbered
					|| lotSerClass.LotSerAssign == INLotSerAssign.WhenUsed)
					return DeallocateNonLots(graph, line, siteID, qtyToDeallocate);

				return DeallocateLots(graph, line, siteID, qtyToDeallocate);
			}
		}

		protected virtual decimal? DeallocateNonLots(SOOrderEntry graph, SOLine line, int? siteID, decimal? qtyToDeallocate)
		{
			var remainQty = qtyToDeallocate;

			var allocatedSplits = SelectAllocatedSplits(graph, line, siteID)
				.OrderByDescending(s => s.SplitLineNbr)
				.ToQueue();

			decimal? deallocatedQty = 0;
			while (remainQty > 0 && allocatedSplits.Any())
			{
				var split = allocatedSplits.Dequeue();
				var qty = DeallocateNonLots(graph, split, remainQty);
				deallocatedQty += qty;
				remainQty -= qty;
			}

			return deallocatedQty;
		}

		protected virtual decimal? DeallocateNonLots(SOOrderEntry graph, SOLineSplit splitToDeallocate, decimal? qtyToDeallocate)
		{
			decimal? deallocatedQty;
			var splitCache = graph.splits.Cache;
			var splitCopy = (SOLineSplit)splitCache.CreateCopy(splitToDeallocate);
			var destSplit = FindUnallocatedSplitToMerge(graph, splitCopy);

			bool reduced = false;
			if (splitCopy.Qty > qtyToDeallocate)
			{
				splitCopy.Qty = splitCopy.Qty - qtyToDeallocate;
				splitCopy = (SOLineSplit)splitCache.Update(splitCopy);
				reduced = true;
			}

			if(destSplit != null)
			{
				if (!reduced)
				{
					qtyToDeallocate = splitCopy.Qty;
					splitCache.Delete(splitCopy);
				}

				splitCopy = (SOLineSplit)splitCache.CreateCopy(destSplit);
				splitCopy.Qty += qtyToDeallocate;
				splitCache.Update(splitCopy);

				deallocatedQty = qtyToDeallocate;
			}
			else
			{
				if (reduced)
				{
					splitCopy = (SOLineSplit)splitCache.Insert();
					splitCopy.Qty = qtyToDeallocate;
					splitCopy.IsAllocated = false;
					if (splitCopy.Behavior == SOBehavior.BL)
						splitCopy.POCreate = false;
					splitCache.Update(splitCopy);

					deallocatedQty = qtyToDeallocate;
				}
				else
				{
					splitCopy.IsAllocated = false;
					splitCopy.LotSerialNbr = null;
					splitCopy = (SOLineSplit)splitCache.Update(splitCopy);
					if (splitCopy.Behavior == SOBehavior.BL && splitCopy.POCreate == true)
					{
						splitCopy = (SOLineSplit)splitCache.CreateCopy(splitCopy);
						splitCopy.POCreate = false;
						splitCache.Update(splitCopy);
					}

					deallocatedQty = splitCopy.Qty;
				}
			}
			return deallocatedQty;
		}

		protected virtual decimal? DeallocateLots(SOOrderEntry graph, SOLine line, int? siteID, decimal? qtyToDeallocate)
		{
			var allocatedSplits = SelectAllocatedSplits(graph, line, siteID);
			var allocatedQty = allocatedSplits.Sum(x => x.Qty);

			if (allocatedQty > qtyToDeallocate)
			{
				var unassignedLotSplits = allocatedSplits.Where(x => string.IsNullOrEmpty(x.LotSerialNbr)).ToList();
				var unassignedLotQty = unassignedLotSplits.Sum(x => x.Qty);

				if (unassignedLotQty < qtyToDeallocate)
					throw new PXException(Messages.CantDeallocateLotSerialNumber);

				allocatedSplits = unassignedLotSplits.ToArray();
			}

			var remainQty = qtyToDeallocate;

			var splitsToDeallocate = allocatedSplits
				.OrderByDescending(s => s.SplitLineNbr)
				.ToQueue();

			decimal? deallocatedQty = 0;
			while (remainQty > 0 && splitsToDeallocate.Any())
			{
				var split = splitsToDeallocate.Dequeue();
				var qty = DeallocateNonLots(graph, split, remainQty);
				deallocatedQty += qty;
				remainQty -= qty;
			}

			return deallocatedQty;
		}

		protected virtual SOLineSplit FindUnallocatedSplitToMerge(SOOrderEntry graph, SOLineSplit split)
		{
			var splitCache = graph.splits.Cache;
			var splitCopy = (SOLineSplit)splitCache.CreateCopy(split);
			splitCopy.IsAllocated = false;
			splitCopy.LotSerialNbr = null;

			var splitToMerge = SelectUnallocatedSplits(graph, graph.Transactions.Current)
				.OrderByDescending(x => x.SplitLineNbr)
				.FirstOrDefault(s =>
					s.SplitLineNbr != splitCopy.SplitLineNbr
					&& graph.LineSplittingAllocatedExt.SchedulesEqual(s, splitCopy, PXDBOperation.Update));
			return splitToMerge;
		}

		protected virtual SOLineSplit[] SelectAllocatedSplits(SOOrderEntry graph, SOLine line, int? siteID)
		{
			var splittingAllocatedExt = graph.LineSplittingAllocatedExt;
			return PXParentAttribute
				.SelectChildren(graph.splits.Cache, line, graph.Transactions.GetItemType())
				.Cast<SOLineSplit>()
				.Where(s => s.IsAllocated == true && s.SiteID == siteID && s.POReceiptNbr == null && splittingAllocatedExt.AllowToManualAllocate(line, s))
				.ToArray();
		}

		#endregion

		protected virtual INLotSerClass ReadLotSerClass(PXGraph graph, int? inventoryID)
		{
			if (inventoryID == null)
				return null;

			var inventory = InventoryItem.PK.Find(graph, inventoryID);
			if (inventory == null)
				throw new PXException(ErrorMessages.ValueDoesntExistOrNoRights, IN.Messages.InventoryItem, inventoryID);

			INLotSerClass lotSerClass;
			if (inventory.StkItem == true)
			{
				lotSerClass = INLotSerClass.PK.Find(graph, inventory.LotSerClassID);
				if (lotSerClass == null)
					throw new PXException(ErrorMessages.ValueDoesntExistOrNoRights, IN.Messages.LotSerClass, inventory.LotSerClassID);
				return lotSerClass;
			}
			return null;
		}
	}
}
