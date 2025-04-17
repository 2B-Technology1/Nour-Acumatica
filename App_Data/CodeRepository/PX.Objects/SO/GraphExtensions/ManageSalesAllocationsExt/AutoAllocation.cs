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

namespace PX.Objects.SO.GraphExtensions.ManageSalesAllocationsExt
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	[PXProtectedAccess(typeof(ManageSalesAllocations))]
	public abstract class AutoAllocation : PXGraphExtension<ManageSalesAllocations>
	{
		[PXReadOnlyView]
		public PXSelect<SalesAllocationStatus> AllocationStatuses;

		#region Protected Access
		/// Uses <see cref="ManageSalesAllocations.GetAllocationError(SalesAllocation, decimal?)"/>
		[PXProtectedAccess]
		protected abstract PXSetPropertyException GetAllocationError(SalesAllocation row, decimal? qtyToAllocate);

		/// Uses <see cref="ManageSalesAllocations.ShowAllocationError(SalesAllocation)"/>
		[PXProtectedAccess]
		protected abstract bool ShowAllocationError(SalesAllocation row);
		#endregion

		#region Overrides

		/// Overrides <see cref="ManageSalesAllocations.ResetAllocations"/>
		public void ResetAllocations(Action baseImpl)
		{
			baseImpl();

			AllocationStatuses.Cache.Clear();
		}

		/// Overrides <see cref="ManageSalesAllocations.CalculateQtyToAllocate(List{PXResult{SalesAllocation}})"/>
		[PXOverride]
		public void CalculateQtyToAllocate(List<PXResult<SalesAllocation>> rows, Action<List<PXResult<SalesAllocation>>> baseImpl)
		{
			baseImpl(rows);

			var allocations = rows.RowCast<SalesAllocation>().ToList();

			ResetAllocationStatuses(allocations);

			var cache = Base.Caches<SalesAllocation>();
			var selectedRows = cache.Inserted
				.Concat_(cache.Updated)
				.Cast<SalesAllocation>()
				.Where(x => x.Selected == true)
				.ToList();
			var hiddenSelectedRows = selectedRows.Except(allocations, cache.GetComparer()).ToList();
			foreach (var row in hiddenSelectedRows)
			{
				row.IsExtraAllocation = false;
				Allocate(row);
			}

			var visibleSelectedRows = allocations.Intersect(selectedRows, cache.GetComparer()).ToList();//keep rows sequence
			foreach (var row in visibleSelectedRows)
			{
				row.IsExtraAllocation = false;
				var status = Allocate(row);
				if (row.QtyToAllocate > 0 && status.AllocatedQty > status.AvailableQty)
					row.IsExtraAllocation = true;
			}

			foreach (SalesAllocation row in allocations)
			{
				if (row.Selected != true)
				{
					row.IsExtraAllocation = false;
					Allocate(row);
				}
			}
		}

		/// Overrides <see cref="ManageSalesAllocations.GetAllocationError(SalesAllocation)"/>
		[PXOverride]
		public PXSetPropertyException GetAllocationError(SalesAllocation row, decimal? qtyToAllocate, Func<SalesAllocation, decimal?, PXSetPropertyException> baseImpl)
		{
			var ex = baseImpl(row, qtyToAllocate);

			if (ex == null
				&& qtyToAllocate > 0
				&& TryGetAllocationStatus(row, out SalesAllocationStatus status))
			{
				bool isNewValue = row.QtyToAllocate != qtyToAllocate;
				if (isNewValue)
				{
					var diff = (qtyToAllocate ?? 0) - (row.QtyToAllocate ?? 0);
					if (diff > 0 && diff > status.UnallocatedQty + status.BufferedQty)
						ex = new PXSetPropertyException(Messages.QtyToAllocateGreaterQtyAvailable);
				}
				else if (status.AllocatedQty > status.AvailableQty)
				{
					if (row.IsExtraAllocation == true)
						ex = new PXSetPropertyException(Messages.QtyToAllocateGreaterQtyAvailable);
				}
			}
			return ex;
		}

		#endregion


		protected virtual void ResetAllocationStatuses(IEnumerable<SalesAllocation> rows)
		{
			var statuses = AllocationStatuses.Cache.Inserted
				.Concat_(AllocationStatuses.Cache.Updated)
				.OfType<SalesAllocationStatus>()
				.ToList();

			foreach (SalesAllocationStatus st in statuses)
			{
				st.AvailableQty = 0m;
				st.AllocatedQty = 0m;
				st.AllocatedSelectedQty = 0m;
				st.UnallocatedQty = 0m;
				st.IsDirty = true;
			}

			foreach(var row in rows.Where(x => x.Selected != true))
				PrepareAllocationStatus(row);

			foreach (var row in rows.Where(x => x.Selected == true))
				PrepareAllocationStatus(row);
		}

		protected virtual bool TryGetAllocationStatus(SalesAllocation row, out SalesAllocationStatus status)
		{
			var s = new SalesAllocationStatus
			{
				SiteID = row.LineSiteID,
				InventoryID = row.InventoryID
			};

			s = (SalesAllocationStatus)AllocationStatuses.Cache.Locate(s);
			if (s != null)
			{
				status = s;
				return true;
			}
			status = null;
			return false;
		}

		protected virtual SalesAllocationStatus PrepareAllocationStatus(SalesAllocation row)
		{
			SalesAllocationStatus status;

			if (TryGetAllocationStatus(row, out status))
			{
				if(status.IsDirty == true)
				{
					decimal? qtyHardAvail = row.QtyHardAvail ?? 0;
					status.AvailableQty = qtyHardAvail;
					status.UnallocatedQty = qtyHardAvail;

					if (status.BufferedQty > 0)
						status.UnallocatedQty -= status.BufferedQty ?? 0;

					status.IsDirty = false;
				}
			}
			else
			{
				status = new SalesAllocationStatus
				{
					SiteID = row.LineSiteID,
					InventoryID = row.InventoryID,

					AvailableQty = row.QtyHardAvail,
					UnallocatedQty = row.QtyHardAvail
				};
				status = (SalesAllocationStatus)AllocationStatuses.Cache.Insert(status);
			}
			return status;
		}

		protected virtual SalesAllocationStatus Allocate(SalesAllocation row)
		{
			var status = PrepareAllocationStatus(row);

			decimal allocateQty;
			if (row.Selected == true)
			{
				allocateQty = row.QtyToAllocate ?? 0;
				if (row.QtyHardAvail != status.AvailableQty)
					row.QtyHardAvail = status.AvailableQty;
			}
			else if (status.UnallocatedQty <= 0)
				allocateQty = 0m;
			else if (status.UnallocatedQty >= (row.QtyUnallocated ?? 0))
				allocateQty = row.QtyUnallocated ?? 0;
			else
				allocateQty = status.UnallocatedQty ?? 0;

			if (row.Selected != true)
				row.QtyToAllocate = allocateQty;

			status.AllocatedQty += allocateQty;
			status.UnallocatedQty -= allocateQty;

			if (row.Selected == true)
				status.AllocatedSelectedQty += allocateQty;

			return status;
		}

		protected virtual SalesAllocationStatus SyncAllocationStatus(SalesAllocation oldRow, SalesAllocation row)
		{
			var selectedChanged = AllocationStatuses.Cache.ObjectsEqual<SalesAllocation.selected>(oldRow, row);
			var qtyToAllocateChanged = AllocationStatuses.Cache.ObjectsEqual<SalesAllocation.qtyToAllocate>(oldRow, row);
			if (!selectedChanged && !qtyToAllocateChanged)
				return null;

			SalesAllocationStatus status;
			if (!TryGetAllocationStatus(row, out status))
				return null;

			var diff = row.QtyToAllocate  - oldRow.QtyToAllocate;

			if (qtyToAllocateChanged)
			{
				if (diff > 0)
				{
					var qtyRemain = diff;

					if (status.BufferedQty > 0)
						qtyRemain -= TakeFromBuffer(row, status, diff);

					status.UnallocatedQty -= qtyRemain;
				}
				else
				{
					PutToBuffer(row, status, -diff);
				}

				status.AllocatedQty += diff;
			}

			if (selectedChanged)
			{
				if (row.Selected == true)
					status.AllocatedSelectedQty += diff;
				else
				{
					status.AllocatedSelectedQty -= diff;
					if(row.BufferedQty > 0)
					{
						status.BufferedQty -= row.BufferedQty;
						row.BufferedQty = 0;
						row.BufferedTime = null;
					}
				}
			}

			return status;
		}

		protected virtual decimal? PutToBuffer(SalesAllocation allocation, SalesAllocationStatus status, decimal? qty)
		{
			allocation.BufferedQty += qty;
			allocation.BufferedTime = DateTime.UtcNow;

			status.BufferedQty += qty;
			return qty;
		}

		protected virtual decimal? TakeFromBuffer(SalesAllocation allocation, SalesAllocationStatus status, decimal? qty)
		{
			qty = status.BufferedQty >= qty
				? qty
				: status.BufferedQty;

			status.BufferedQty -= qty;

			var bufferedAllocations = Base.Allocations.Cache.Inserted
				.Concat_(Base.Allocations.Cache.Updated)
				.OfType<SalesAllocation>()
				.Where(x => x.BufferedQty > 0 && x.InventoryID == allocation.InventoryID)
				.OrderBy(x => x.BufferedTime)
				.ToQueue();

			var remainQty = qty;
			while(remainQty > 0 && bufferedAllocations.Any())
			{
				var alloc = bufferedAllocations.Dequeue();
				var reduceQty = alloc.BufferedQty >= remainQty
					? remainQty
					: alloc.BufferedQty;

				alloc.BufferedQty -= reduceQty;
				if (alloc.BufferedQty == 0)
					alloc.BufferedTime = null;

				remainQty -= reduceQty;
			}

			return qty;
		}

		protected virtual void _(Events.FieldVerifying<SalesAllocation, SalesAllocation.qtyToAllocate> e)
		{
			if (e.ExternalCall)
			{
				var newValue = (decimal?)e.NewValue;
				var ex = GetAllocationError(e.Row, newValue);
				if(ex != null)
				{
					// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs ErrorValue is not DAC field
					ex.ErrorValue = e.NewValue;
					throw ex;
				}
				if (decimal.Remainder(newValue ?? 0, 1m) > 0m)
				{
					var inventoryItem = InventoryItem.PK.Find(Base, e.Row.InventoryID);
					var lotSerClass = INLotSerClass.PK.Find(Base, inventoryItem?.LotSerClassID);
					if (lotSerClass?.LotSerTrack == INLotSerTrack.SerialNumbered)
						e.NewValue = Math.Truncate(newValue ?? 0);
				}
			}
		}

		protected virtual void _(Events.RowUpdated<SalesAllocation> e)
		{
			if (e.ExternalCall && Base.Filter.Current.Action == SalesAllocationsFilter.action.AllocateSalesOrders)
			{
				var status = SyncAllocationStatus(e.OldRow, e.Row);
				if (status != null && e.Row.IsExtraAllocation == true)
				{
					e.Row.IsExtraAllocation =
						e.Row.Selected == true
						&& e.Row.QtyToAllocate > 0
						&& status.AllocatedQty > status.AvailableQty;

					if (e.Row.IsExtraAllocation == false)
						ShowAllocationError(e.Row);
				}
			}
		}
	}
}
