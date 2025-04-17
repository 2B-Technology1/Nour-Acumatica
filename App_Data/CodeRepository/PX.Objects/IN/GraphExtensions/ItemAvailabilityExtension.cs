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

using PX.Common;
using PX.Data;

using PX.Objects.Common;
using PX.Objects.Common.Exceptions;
using PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated;

using IQtyAllocatedBase = PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated.Abstraction.IQtyAllocatedBase;
using IQtyAllocated = PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated.Abstraction.IQtyAllocated;

namespace PX.Objects.IN.GraphExtensions
{
	public abstract class ItemAvailabilityExtension<TGraph, TLine, TSplit> : PXGraphExtension<TGraph>
		where TGraph : PXGraph
		where TLine : class, IBqlTable, ILSPrimary, new()
		where TSplit : class, IBqlTable, ILSDetail, new()
	{
		#region Cache Helpers
		#region TLine
		private PXCache<TLine> _lineCache;
		public PXCache<TLine> LineCache => _lineCache ?? (_lineCache = Base.Caches<TLine>());
		#endregion
		#region TSplit
		private PXCache<TSplit> _splitCache;
		public PXCache<TSplit> SplitCache => _splitCache ?? (_splitCache = Base.Caches<TSplit>());
		#endregion
		#endregion

		#region Configuration
		protected abstract TSplit EnsureSplit(ILSMaster row);

		protected abstract string GetStatus(TLine line);

		protected abstract decimal GetUnitRate(TLine line);

		protected abstract void RaiseQtyExceptionHandling(TLine line, PXExceptionInfo ei, decimal? newValue);

		protected abstract void RaiseQtyExceptionHandling(TSplit split, PXExceptionInfo ei, decimal? newValue);
		#endregion

		#region Initialization
		public override void Initialize()
		{
			AddStatusField();
			ItemPlanHelper<TGraph>.AddStatusDACsToCacheMapping(Base);
		}
		#endregion

		#region Status Field
		public (string Name, string DisplayName) StatusField { get; protected set; } = (Messages.Availability_Field, Messages.Availability_Field);

		protected virtual void AddStatusField()
		{
			StatusField = (Messages.Availability_Field, PXMessages.LocalizeNoPrefix(Messages.Availability_Field));
			LineCache.Fields.Add(StatusField.Name);
			ManualEvent.FieldOf<TLine>.Selecting.Subscribe(Base, StatusField.Name, EventHandlerStatusField);
		}

		protected virtual void EventHandlerStatusField(ManualEvent.FieldOf<TLine>.Selecting.Args e)
		{
			if (e.Row != null && e.Row.InventoryID != null && e.Row.SiteID != null && !PXLongOperation.Exists(Base))
				e.ReturnValue = GetStatus(e.Row);
			else
				e.ReturnValue = string.Empty;

			var returnState = PXStringState.CreateInstance(e.ReturnState, 255, null, StatusField.Name, false, null, null, null, null, null, null);
			returnState.Visible = false;
			returnState.Visibility = PXUIVisibility.Invisible;
			returnState.DisplayName = StatusField.DisplayName;
			e.ReturnState = returnState;
		}
		#endregion

		#region Check
		public virtual void Check(ILSMaster row, int? costCenterID)
		{
			if (row != null && row.InvtMult == -1 && row.BaseQty > 0m)
			{
				IStatus availability = FetchWithBaseUOM(row, excludeCurrent: true, costCenterID);
				Check(row, availability);
			}
		}

		protected virtual void Check(ILSMaster row, IStatus availability)
		{
			foreach (var errorInfo in GetCheckErrors(row, availability))
				RaiseQtyExceptionHandling(row, errorInfo, row.Qty);
		}

		protected virtual void RaiseQtyExceptionHandling(ILSMaster row, PXExceptionInfo ei, decimal? newValue)
		{
			if (row is TLine line)
				RaiseQtyExceptionHandling(line, ei, newValue);
			else if (row is TSplit split)
				RaiseQtyExceptionHandling(split, ei, newValue);
		}

		public virtual IEnumerable<PXExceptionInfo> GetCheckErrors(ILSMaster row, int? costCenterID)
		{
			if (row != null && row.InvtMult == -1 && row.BaseQty > 0m)
			{
				IStatus availability = FetchWithBaseUOM(row, excludeCurrent: true, costCenterID);

				return GetCheckErrors(row, availability);
			}
			return Array.Empty<PXExceptionInfo>();
		}

		protected virtual IEnumerable<PXExceptionInfo> GetCheckErrors(ILSMaster row, IStatus availability)
		{
			if (!IsAvailableQty(row, availability))
			{
				string message = GetErrorMessageQtyAvail(GetStatusLevel(availability));

				if (message != null)
					yield return new PXExceptionInfo(PXErrorLevel.Warning, message);
			}
		}

		protected virtual bool IsAvailableQty(ILSMaster row, IStatus availability)
		{
			if (row.InvtMult == -1 && row.BaseQty > 0m && availability != null)
				if (availability.QtyNotAvail < 0m && (availability.QtyAvail + availability.QtyNotAvail) < 0m)
					return false;

			return true;
		}

		protected virtual string GetErrorMessageQtyAvail(StatusLevel level)
		{
			switch (level)
			{
				case StatusLevel.LotSerial: return Messages.StatusCheck_QtyLotSerialNegative;
				case StatusLevel.Location: return Messages.StatusCheck_QtyLocationNegative;
				case StatusLevel.Site: return Messages.StatusCheck_QtyNegative;
				default: throw new ArgumentOutOfRangeException(nameof(level));
			}
		}

		protected virtual string GetErrorMessageQtyOnHand(StatusLevel level)
		{
			switch (level)
			{
				case StatusLevel.LotSerial: return Messages.StatusCheck_QtyLotSerialOnHandNegative;
				case StatusLevel.Location: return Messages.StatusCheck_QtyLocationOnHandNegative;
				case StatusLevel.Site: return Messages.StatusCheck_QtyOnHandNegative;
				default: throw new ArgumentOutOfRangeException(nameof(level));
			}
		}

		protected virtual StatusLevel GetStatusLevel(IStatus availability)
		{
			switch (availability)
			{
				case LotSerialStatusByCostCenter _: return StatusLevel.LotSerial;
				case LocationStatusByCostCenter _: return StatusLevel.Location;
				case SiteStatusByCostCenter _: return StatusLevel.Site;
				default: throw new ArgumentOutOfRangeException(nameof(availability));
			}
		}
		#endregion

		#region Fetch
		public bool IsFetching { get; protected set; }

		public IStatus FetchWithLineUOM(TLine line, bool excludeCurrent, int? costCenterID)
		{
			if (FetchWithBaseUOM(line, excludeCurrent, costCenterID) is IStatus availability)
				return availability.Multiply(GetUnitRate(line));

			return null;
		}

		public virtual IStatus FetchWithBaseUOM(ILSMaster row, bool excludeCurrent, int? costCenterID)
		{
			if (row == null)
				return null;

			try
			{
				IsFetching = true;

				TSplit split = EnsureSplit(row);
				return Fetch(split, excludeCurrent, costCenterID);
			}
			finally
			{
				IsFetching = false;
			}
		}


		protected virtual IStatus Fetch(ILSDetail split, bool excludeCurrent, int? costCenterID)
		{
			if (split == null || split.InventoryID == null || split.SubItemID == null || split.SiteID == null)
				return null;

			INLotSerClass lsClass =
				InventoryItem.PK.Find(Base, split.InventoryID)
				.With(ii => ii.StkItem == true ? INLotSerClass.PK.Find(Base, ii.LotSerClassID) : null);

			if (lsClass?.LotSerTrack == null)
				return null;

			if (_detailsRequested++ == DetailsCountToEnableOptimization)
				Optimize();

			if (split.LocationID != null)
			{
				if (string.IsNullOrEmpty(split.LotSerialNbr) == false &&
					(string.IsNullOrEmpty(split.AssignedNbr) || INLotSerialNbrAttribute.StringsEqual(split.AssignedNbr, split.LotSerialNbr) == false) &&
					lsClass.LotSerAssign == INLotSerAssign.WhenReceived)
				{
					return FetchLotSerial(split, excludeCurrent, costCenterID);
				}

				return FetchLocation(split, excludeCurrent, costCenterID);
			}

			return FetchSite(split, excludeCurrent, costCenterID);
		}

		protected virtual IStatus FetchLotSerial(ILSDetail split, bool excludeCurrent, int? costCenterID)
		{
			using (new DisableSelectorValidationScope(Base.Caches<LotSerialStatusByCostCenter>(), typeof(LotSerialStatusByCostCenter.siteID)))
			{
				var acc = InitializeRecord(new LotSerialStatusByCostCenter
				{
					InventoryID = split.InventoryID,
					SubItemID = split.SubItemID,
					SiteID = split.SiteID,
					LocationID = split.LocationID,
					LotSerialNbr = split.LotSerialNbr,
					CostCenterID = costCenterID
				});

				var status = INLotSerialStatusByCostCenter.PK.Find(Base, split.InventoryID, split.SubItemID, split.SiteID, split.LocationID, split.LotSerialNbr, costCenterID);

				return Fetch<LotSerialStatusByCostCenter>(split, PXCache<LotSerialStatusByCostCenter>.CreateCopy(acc), status, excludeCurrent);
			}
		}

		protected virtual IStatus FetchLocation(ILSDetail split, bool excludeCurrent, int? costCenterID)
		{
			using (new DisableSelectorValidationScope(Base.Caches<LocationStatusByCostCenter>(), typeof(LocationStatusByCostCenter.siteID)))
			{
				var acc = InitializeRecord(new LocationStatusByCostCenter
				{
					InventoryID = split.InventoryID,
					SubItemID = split.SubItemID,
					SiteID = split.SiteID,
					LocationID = split.LocationID,
					CostCenterID = costCenterID
				});

				var status = INLocationStatusByCostCenter.PK.Find(Base, split.InventoryID, split.SubItemID, split.SiteID, split.LocationID, costCenterID);

				return Fetch<LocationStatusByCostCenter>(split, PXCache<LocationStatusByCostCenter>.CreateCopy(acc), status, excludeCurrent);
			}
		}

		protected virtual IStatus FetchSite(ILSDetail split, bool excludeCurrent, int? costCenterID)
		{
			using (new DisableSelectorValidationScope(Base.Caches<SiteStatusByCostCenter>(), typeof(SiteStatusByCostCenter.siteID)))
			{
				var acc = InitializeRecord(new SiteStatusByCostCenter
				{
					InventoryID = split.InventoryID,
					SubItemID = split.SubItemID,
					SiteID = split.SiteID,
					CostCenterID = costCenterID
				});

				var status = INSiteStatusByCostCenter.PK.Find(Base, split.InventoryID, split.SubItemID, split.SiteID, costCenterID);

				return Fetch<SiteStatusByCostCenter>(split, PXCache<SiteStatusByCostCenter>.CreateCopy(acc), status, excludeCurrent);
			}
		}

		protected virtual IStatus Fetch<TQtyAllocated>(ILSDetail split, IStatus allocated, IStatus existing, bool excludeCurrent)
			where TQtyAllocated : class, IQtyAllocated, IBqlTable, new()
		{
			Summarize(allocated, existing);

			if (excludeCurrent)
			{
				var signs = GetAvailabilitySigns<TQtyAllocated>((TSplit)split);
				ExcludeCurrent(split, allocated, signs);
			}

			return allocated;
		}

		public virtual AvailabilitySigns GetAvailabilitySigns<TStatus>(TSplit split)
			where TStatus : class, IQtyAllocatedBase, IBqlTable, new()
		{
			return Base.FindImplementation<IItemPlanHandler<TSplit>>()?
				.GetAvailabilitySigns<TStatus>(split)
				?? new AvailabilitySigns();
		}

		protected virtual void Summarize(IStatus allocated, IStatus existing) => allocated.Add(existing);

		protected virtual void ExcludeCurrent(ILSDetail currentSplit, IStatus allocated, AvailabilitySigns signs)
		{
			if (signs.SignQtyAvail != Sign.Zero)
			{
				allocated.QtyAvail -= signs.SignQtyAvail * (currentSplit.BaseQty ?? 0m);
				allocated.QtyNotAvail += signs.SignQtyAvail * (currentSplit.BaseQty ?? 0m);
			}

			if (signs.SignQtyHardAvail != Sign.Zero)
			{
				allocated.QtyHardAvail -= signs.SignQtyHardAvail * (currentSplit.BaseQty ?? 0m);
			}

			if (signs.SignQtyActual != Sign.Zero)
			{
				allocated.QtyActual -= signs.SignQtyActual * (currentSplit.BaseQty ?? 0m);
			}
		}
		#endregion

		#region Helpers
		protected T InitializeRecord<T>(T row)
			where T : class, IBqlTable, new()
		{
			Base.RowInserted.AddHandler<T>(CleanUpOnInsert);
			try
			{
				return PXCache<T>.Insert(Base, row);
			}
			finally
			{
				Base.RowInserted.RemoveHandler<T>(CleanUpOnInsert);
			}

			void CleanUpOnInsert(PXCache cache, PXRowInsertedEventArgs e)
			{
				cache.SetStatus(e.Row, PXEntryStatus.Notchanged);
				cache.IsDirty = false;
			}
		}

		protected decimal GetUnitRate<TInventoryID, TUOM>(TLine line)
			where TInventoryID : IBqlField
			where TUOM : IBqlField
			=> INUnitAttribute.ConvertFromBase<TInventoryID, TUOM>(LineCache, line, 1m, INPrecision.NOROUND);

		protected virtual string FormatQty(decimal? value)
			=> value?.ToString("N" + CommonSetupDecPl.Qty.ToString(), System.Globalization.NumberFormatInfo.CurrentInfo) ?? string.Empty;
		#endregion

		#region Optimization
		protected int _detailsRequested = 0;

		protected virtual int DetailsCountToEnableOptimization => 5;
		public bool IsOptimizationEnabled => _detailsRequested > DetailsCountToEnableOptimization;

		protected virtual void Optimize() { }
		#endregion

		public enum StatusLevel
		{
			Site,
			Location,
			LotSerial
		}
	}
}
