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
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;

using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.IN;

using SiteLotSerial = PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated.SiteLotSerial;
using LotSerOptions = PX.Objects.IN.LSSelect.LotSerOptions;
using Counters = PX.Objects.IN.LSSelect.Counters;


namespace PX.Objects.SO.GraphExtensions.SOOrderEntryExt
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class SOOrderLineSplittingExtension : IN.GraphExtensions.LineSplittingExtension<SOOrderEntry, SOOrder, SOLine, SOLineSplit>
	{
		#region State
		public bool IsLocationEnabled
		{
			get
			{
				SOOrderType ordertype = PXSetup<SOOrderType>.Select(Base);
				return ordertype == null || (ordertype.RequireShipping == false && ordertype.RequireLocation == true && ordertype.INDocType != INTranType.NoUpdate);
			}
		}

		public bool IsLotSerialRequired
		{
			get
			{
				SOOrderType ordertype = PXSetup<SOOrderType>.Select(Base);
				return ordertype == null || ordertype.RequireLotSerial == true;
			}
		}

		public bool IsLSEntryEnabled
		{
			get
			{
				SOOrderType ordertype = PXSetup<SOOrderType>.Select(Base);
				return ordertype == null || ordertype.RequireLocation == true || ordertype.RequireLotSerial == true;
			}
		}

		public bool IsBlanketOrder
		{
			get
			{
				SOOrderType ordertype = PXSetup<SOOrderType>.Select(Base);
				return ordertype?.Behavior == SOBehavior.BL;
			}
		}
		#endregion

		#region Configuration
		protected override Type SplitsToDocumentCondition => typeof(SOLineSplit.FK.Order.SameAsCurrent);

		protected override Type LineQtyField => typeof(SOLine.orderQty);

		public override SOLineSplit LineToSplit(SOLine line)
		{
			using (InvtMultModeScope(line))
			{
				SOLineSplit ret = (SOLineSplit)line;
				//baseqty will be overriden in all cases but AvailabilityFetch
				ret.BaseQty = line.BaseQty - line.UnassignedQty;

				return ret;
			}
		}
		#endregion

		#region Initialization
		public override void Initialize()
		{
			base.Initialize();
			ManualEvent.Row<SOOrder>.Selected.Subscribe(Base, EventHandler);
			ManualEvent.Row<SOOrder>.Updated.Subscribe(Base, EventHandler);
		}
		#endregion

		#region Actions
		public override IEnumerable ShowSplits(PXAdapter adapter)
		{
			SOLine currentSOLine = LineCurrent;

			if (currentSOLine?.LineType == SOLineType.MiscCharge && !IsBlanketOrder)
				throw new PXSetPropertyException(Messages.BinLotSerialInvalid);

			if (IsLSEntryEnabled)
				if (currentSOLine != null && currentSOLine.LineType != SOLineType.Inventory)
					throw new PXSetPropertyException(Messages.BinLotSerialInvalid);

			return base.ShowSplits(adapter);
		}

		#endregion

		#region Event Handlers
		#region SOOrder
		protected SOOrder _LastSelected;

		protected virtual void EventHandler(ManualEvent.Row<SOOrder>.Selected.Args e)
		{
			if (_LastSelected == null || !ReferenceEquals(_LastSelected, e.Row))
			{
				PXUIFieldAttribute.SetRequired<SOLine.locationID>(LineCache, IsLocationEnabled);
				PXUIFieldAttribute.SetVisible<SOLine.locationID>(LineCache, null, IsLocationEnabled);
				PXUIFieldAttribute.SetEnabled<SOLine.locationID>(LineCache, null, IsLocationEnabled);
				PXUIFieldAttribute.SetVisible<SOLine.lotSerialNbr>(LineCache, null, IsLSEntryEnabled);
				LineCache.Adjust<INLotSerialNbrAttribute>().For<SOLine.lotSerialNbr>(a => a.ForceDisable = !IsLSEntryEnabled);
				PXUIFieldAttribute.SetVisible<SOLine.expireDate>(LineCache, null, IsLSEntryEnabled);
				LineCache.Adjust<INExpireDateAttribute>().For<SOLine.expireDate>(a => a.ForceDisable = !IsLSEntryEnabled);

				PXUIFieldAttribute.SetVisible<SOLineSplit.inventoryID>(SplitCache, null, IsLSEntryEnabled);
				PXUIFieldAttribute.SetEnabled<SOLineSplit.inventoryID>(SplitCache, null, IsLSEntryEnabled);
				PXUIFieldAttribute.SetVisible<SOLineSplit.locationID>(SplitCache, null, IsLocationEnabled);
				PXUIFieldAttribute.SetEnabled<SOLineSplit.locationID>(SplitCache, null, IsLocationEnabled);
				PXUIFieldAttribute.SetVisible<SOLineSplit.lotSerialNbr>(SplitCache, null, !IsBlanketOrder);
				SplitCache.Adjust<INLotSerialNbrAttribute>().For<SOLineSplit.lotSerialNbr>(a => a.ForceDisable = IsBlanketOrder);
				PXUIFieldAttribute.SetVisible<SOLineSplit.expireDate>(SplitCache, null, IsLSEntryEnabled);
				SplitCache.Adjust<INExpireDateAttribute>().For<SOLineSplit.expireDate>(a => a.ForceDisable = !IsLSEntryEnabled);

				if (Base.Views.TryGetValue(TypePrefixed(nameof(LotSerOptions)), out PXView view))
					view.AllowSelect = IsLSEntryEnabled;

				if (e.Row != null)
					_LastSelected = e.Row;
			}

			showSplits.SetEnabled(false);

			if (IsLSEntryEnabled)
				showSplits.SetEnabled(true);
		}

		protected virtual void EventHandler(ManualEvent.Row<SOOrder>.Updated.Args e)
		{
			if ((IsLSEntryEnabled || IsBlanketOrder) && e.Row.Hold != e.OldRow.Hold && e.Row.Hold == false)
			{
				foreach (SOLine line in PXParentAttribute.SelectSiblings(LineCache, null, typeof(SOOrder)))
				{
					if (Math.Abs(line.BaseQty.Value) >= 0.0000005m && (line.UnassignedQty >= 0.0000005m || line.UnassignedQty <= -0.0000005m))
					{
						string errorMsg = IsBlanketOrder ? Messages.BlanketSplitTotalQtyNotEqualLineQty : Messages.BinLotSerialNotAssigned;
						LineCache.RaiseExceptionHandling<SOLine.orderQty>(line, line.Qty, new PXSetPropertyException(errorMsg));
						LineCache.MarkUpdated(line, assertError: true);
					}
				}
			}
		}
		#endregion
		#region SOLine
		protected override void EventHandler(ManualEvent.Row<SOLine>.Updated.Args e)
		{
			try
			{
				using (ResolveNotDecimalUnitErrorRedirectorScope<SOLineSplit.qty>(e.Row))
					base.EventHandler(e);
			}
			catch (PXUnitConversionException ex)
			{
				bool isUomField(string f) => string.Equals(f, nameof(SOLine.uOM), StringComparison.InvariantCultureIgnoreCase);
				if (!PXUIFieldAttribute.GetErrors(e.Cache, e.Row, PXErrorLevel.Error).Keys.Any(isUomField))
					e.Cache.RaiseExceptionHandling<SOLine.uOM>(e.Row, null, ex);
			}
		}

		protected override void EventHandler(ManualEvent.Row<SOLine>.Persisting.Args e)
		{
			if (e.Operation.Command().IsIn(PXDBOperation.Insert, PXDBOperation.Update))
			{
				if (IsLSEntryEnabled || IsBlanketOrder)
				{
					SOOrder doc = PXParentAttribute.SelectParent<SOOrder>(e.Cache, e.Row) ?? Base.Document.Current;

					if (doc.Hold == false && Math.Abs(e.Row.BaseQty.Value) >= 0.0000005m && (e.Row.UnassignedQty >= 0.0000005m || e.Row.UnassignedQty <= -0.0000005m))
					{
						string errorMsg = IsBlanketOrder ? Messages.BlanketSplitTotalQtyNotEqualLineQty : Messages.BinLotSerialNotAssigned;
						if (e.Cache.RaiseExceptionHandling<SOLine.orderQty>(e.Row, e.Row.Qty, new PXSetPropertyException(errorMsg)))
							throw new PXRowPersistingException(nameof(SOLine.orderQty), e.Row.Qty, errorMsg);
					}
				}
			}

			base.EventHandler(e);
		}
		#endregion
		#region SOLineSplit
		protected override void SubscribeForSplitEvents()
		{
			base.SubscribeForSplitEvents();

			ManualEvent.Row<SOLineSplit>.Selected.Subscribe(Base, EventHandler);
			ManualEvent.FieldOf<SOLineSplit, SOLineSplit.invtMult>.Defaulting.Subscribe<short?>(Base, EventHandler);
			ManualEvent.FieldOf<SOLineSplit, SOLineSplit.subItemID>.Defaulting.Subscribe<int?>(Base, EventHandler);
			ManualEvent.FieldOf<SOLineSplit, SOLineSplit.locationID>.Defaulting.Subscribe<int?>(Base, EventHandler);
		}

		protected virtual void EventHandler(ManualEvent.Row<SOLineSplit>.Selected.Args e)
		{
			if (e.Row != null)
			{
				SOLine parent = PXParentAttribute.SelectParent<SOLine>(e.Cache, e.Row);
				bool isLineTypeInventory = e.Row.LineType == SOLineType.Inventory;
				object val = e.Cache.GetValueExt<SOLineSplit.isAllocated>(e.Row);
				bool isAllocated = e.Row.IsAllocated == true || (bool?)PXFieldState.UnwrapValue(val) == true;
				bool isCompleted = e.Row.Completed == true;
				bool IsLinked = e.Row.PONbr != null || e.Row.SOOrderNbr != null && e.Row.IsAllocated == true;
				bool isPOSchedule = (e.Row.POCreate == true && parent?.IsSpecialOrder != true) || e.Row.POCompleted == true;

				PXUIFieldAttribute.SetEnabled<SOLineSplit.subItemID>(e.Cache, e.Row, false);
				PXUIFieldAttribute.SetEnabled<SOLineSplit.completed>(e.Cache, e.Row, false);
				PXUIFieldAttribute.SetEnabled<SOLineSplit.shippedQty>(e.Cache, e.Row, false);
				PXUIFieldAttribute.SetEnabled<SOLineSplit.shipmentNbr>(e.Cache, e.Row, false);
				PXUIFieldAttribute.SetEnabled<SOLineSplit.siteID>(e.Cache, e.Row, !isCompleted && isLineTypeInventory && isAllocated && !IsLinked && !IsBlanketOrder);
				PXUIFieldAttribute.SetEnabled<SOLineSplit.qty>(e.Cache, e.Row, !isCompleted && !IsLinked);
				PXUIFieldAttribute.SetEnabled<SOLineSplit.shipDate>(e.Cache, e.Row, !isCompleted && parent?.ShipComplete == SOShipComplete.BackOrderAllowed);
				PXUIFieldAttribute.SetEnabled<SOLineSplit.pOCreate>(e.Cache, e.Row, IsBlanketOrder && !isCompleted && !isAllocated && parent?.POCreate == true && e.Row.PONbr == null);
				PXUIFieldAttribute.SetEnabled<SOLineSplit.pONbr>(e.Cache, e.Row, false);
				PXUIFieldAttribute.SetEnabled<SOLineSplit.pOReceiptNbr>(e.Cache, e.Row, false);
				e.Cache.Adjust<INLotSerialNbrAttribute>(e.Row).For<SOLineSplit.lotSerialNbr>(a => a.ForceDisable = isCompleted || isPOSchedule);
			}
		}

		protected virtual void EventHandler(ManualEvent.FieldOf<SOLineSplit, SOLineSplit.invtMult>.Defaulting.Args<short?> e)
		{
			if (LineCurrent != null && (e.Row == null || LineCurrent.LineNbr == e.Row.LineNbr))
			{
				using (InvtMultModeScope(LineCurrent))
				{
					e.NewValue = LineCurrent.InvtMult;
					e.Cancel = true;
				}
			}
		}

		protected virtual void EventHandler(ManualEvent.FieldOf<SOLineSplit, SOLineSplit.subItemID>.Defaulting.Args<int?> e)
		{
			if (LineCurrent != null && (e.Row == null || LineCurrent.LineNbr == e.Row.LineNbr && e.Row.IsStockItem == true))
			{
				e.NewValue = LineCurrent.SubItemID;
				e.Cancel = true;
			}
		}

		protected virtual void EventHandler(ManualEvent.FieldOf<SOLineSplit, SOLineSplit.locationID>.Defaulting.Args<int?> e)
		{
			if (LineCurrent != null && (e.Row == null || LineCurrent.LineNbr == e.Row.LineNbr && e.Row.IsStockItem == true))
			{
				e.NewValue = LineCurrent.LocationID;
				e.Cancel = SuppressedMode == true || e.NewValue != null || !IsLocationEnabled;
			}
		}

		protected override void EventHandler(ManualEvent.Row<SOLineSplit>.Inserting.Args e)
		{
			if (IsLSEntryEnabled)
			{
				if (e.ExternalCall && e.Row.LineType != SOLineType.Inventory)
					throw new PXSetPropertyException(ErrorMessages.CantInsertRecord);

				if (e.Row != null && !IsLocationEnabled && e.Row.LocationID != null)
					e.Row.LocationID = null;

				base.EventHandler(e);
			}
		}

		public override void EventHandler(ManualEvent.Row<SOLineSplit>.Persisting.Args e)
		{
			base.EventHandler(e);
			if (e.Row != null && e.Operation.Command().IsIn(PXDBOperation.Insert, PXDBOperation.Update))
			{
				bool requireLocationAndSubItem = e.Row.RequireLocation == true && e.Row.IsStockItem == true && e.Row.BaseQty != 0m;

				PXDefaultAttribute.SetPersistingCheck<SOLineSplit.subItemID>(e.Cache, e.Row, requireLocationAndSubItem ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
				PXDefaultAttribute.SetPersistingCheck<SOLineSplit.locationID>(e.Cache, e.Row, requireLocationAndSubItem ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			}
		}

		public override void EventHandlerUOM(ManualEvent.FieldOf<SOLineSplit>.Defaulting.Args<string> e)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(e.Row.InventoryID);

			if (UseBaseUnitInSplit(e.Row, Base.Transactions.Current, item))
			{
				e.NewValue = ((InventoryItem)item).BaseUnit;
				e.Cancel = true;
			}
			else
			{
				base.EventHandlerUOM(e);
			}
		}
		#endregion
		#endregion

		#region Select Helpers
		internal SOLineSplit[] GetSplits(SOLine line) => SelectSplits(line);

		protected override SOLineSplit[] SelectSplits(SOLineSplit split) => SelectSplits(split, excludeCompleted: true);
		protected virtual SOLineSplit[] SelectSplits(SOLineSplit split, bool excludeCompleted = true)
		{
			bool NotCompleted(SOLineSplit a) => a.Completed == false || a.RequireShipping == false && !IsBlanketOrder
				|| excludeCompleted == false && a.PONbr == null && a.SOOrderNbr == null;
			if (Availability.IsOptimizationEnabled)
				return SelectAllSplits(split).Where(NotCompleted).ToArray();

			return base.SelectSplits(split).Where(NotCompleted).ToArray();
		}

		protected override SOLineSplit[] SelectSplits(SOLine line, bool compareInventoryID = true) => SelectAllSplits(line, compareInventoryID)
					.Where(s => s.Completed == false || s.RequireShipping == false && !IsBlanketOrder)
					.ToArray();

		protected virtual SOLineSplit[] SelectAllSplits(SOLine line, bool compareInventoryID = true)
		{
			if (Availability.IsOptimizationEnabled)
				return SelectAllSplits(LineToSplit(line), compareInventoryID);

			return base.SelectSplits(line, compareInventoryID);
		}

		private SOLineSplit[] SelectAllSplits(SOLineSplit split, bool compareInventoryID)
		{
			return PXParentAttribute
				.SelectSiblings(SplitCache, split, typeof(SOOrder))
				.Cast<SOLineSplit>()
				.Where(a =>
					(!compareInventoryID || SameInventoryItem(a, split)) &&
					a.LineNbr == split.LineNbr)
				.ToArray();
		}

		protected override SOLineSplit[] SelectSplitsOrdered(SOLineSplit split) => SelectSplitsOrdered(split, excludeCompleted: true);
		protected virtual SOLineSplit[] SelectSplitsOrdered(SOLineSplit split, bool excludeCompleted = true)
		{
			return SelectSplits(split, excludeCompleted)
				.OrderBy(s => s.Completed == true ? 0 : s.POCreate == true ? 1 : s.IsAllocated == true ? 2 : 3)
				.ThenBy(s => s.SplitLineNbr)
				.ToArray();
		}

		protected override SOLineSplit[] SelectSplitsReversed(SOLineSplit split) => SelectSplitsReversed(split, excludeCompleted: true);
		protected virtual SOLineSplit[] SelectSplitsReversed(SOLineSplit split, bool excludeCompleted = true)
		{
			return SelectSplits(split, excludeCompleted)
				.OrderByDescending(s => s.Completed == true ? 0 : s.POCreate == true ? 1 : s.IsAllocated == true ? 2 : 3)
				.ThenByDescending(s => s.SplitLineNbr)
				.ToArray();
		}

		protected virtual SOLineSplit[] SelectSplitsReversedforTruncate(SOLineSplit split, bool excludeCompleted = true)
		{
			return SelectSplits(split, excludeCompleted)
				.OrderByDescending(s => s.Completed == true ? 0 : s.IsAllocated == true ? 1 : s.POCreate == true ? 2 : 3)
				.ThenByDescending(s => s.SplitLineNbr)
				.ToArray();
		}

		#endregion

		#region Select LotSerial Status
		protected override PXSelectBase<INLotSerialStatusByCostCenter> GetSerialStatusCmdBase(SOLine line, PXResult<InventoryItem, INLotSerClass> item)
		{
			return new
				SelectFrom<INLotSerialStatusByCostCenter>.
				InnerJoin<INLocation>.On<INLotSerialStatusByCostCenter.FK.Location>.
				InnerJoin<INSiteLotSerial>.On<
					INSiteLotSerial.inventoryID.IsEqual<INLotSerialStatusByCostCenter.inventoryID>.
					And<INSiteLotSerial.siteID.IsEqual<INLotSerialStatusByCostCenter.siteID>>.
					And<INSiteLotSerial.lotSerialNbr.IsEqual<INLotSerialStatusByCostCenter.lotSerialNbr>>>.
				Where<
					INLotSerialStatusByCostCenter.inventoryID.IsEqual<INLotSerialStatusByCostCenter.inventoryID.FromCurrent>.
					And<INLotSerialStatusByCostCenter.siteID.IsEqual<INLotSerialStatusByCostCenter.siteID.FromCurrent>>.
					And<INLotSerialStatusByCostCenter.costCenterID.IsEqual<INLotSerialStatusByCostCenter.costCenterID.FromCurrent>>.
					And<INLotSerialStatusByCostCenter.qtyOnHand.IsGreater<decimal0>>>.
				View(Base);
		}

		protected override void AppendSerialStatusCmdWhere(PXSelectBase<INLotSerialStatusByCostCenter> cmd, SOLine line, INLotSerClass lotSerClass)
		{
			if (line.SubItemID != null)
				cmd.WhereAnd<Where<INLotSerialStatusByCostCenter.subItemID.IsEqual<INLotSerialStatusByCostCenter.subItemID.FromCurrent>>>();

			if (line.LocationID != null)
			{
				cmd.WhereAnd<Where<INLotSerialStatusByCostCenter.locationID.IsEqual<INLotSerialStatusByCostCenter.locationID.FromCurrent>>>();
			}
			else
			{
				switch (line.TranType)
				{
					case INTranType.Transfer:
						cmd.WhereAnd<Where<INLocation.transfersValid.IsEqual<True>>>();
						break;
					default:
						cmd.WhereAnd<Where<INLocation.salesValid.IsEqual<True>>>();
						break;
				}
			}

			if (lotSerClass.IsManualAssignRequired == true)
			{
				if (string.IsNullOrEmpty(line.LotSerialNbr))
					cmd.WhereAnd<Where<True.IsEqual<False>>>();
				else
					cmd.WhereAnd<Where<INLotSerialStatusByCostCenter.lotSerialNbr.IsEqual<INLotSerialStatusByCostCenter.lotSerialNbr.FromCurrent>>>();
			}
		}
		#endregion

		protected override void UpdateCounters(Counters counters, SOLineSplit split)
		{
			base.UpdateCounters(counters, split);

			if (split.POCreate == true || split.AMProdCreate == true)
			{
				//base shipped qty in context of purchase for so is meaningless and equals zero, so it's appended for dropship context
				counters.BaseQty -= split.BaseReceivedQty.Value + split.BaseShippedQty.Value;
			}
		}

		public virtual bool UseBaseUnitInSplit(SOLineSplit split, SOLine line, PXResult<InventoryItem, INLotSerClass> item)
			=> !IsBlanketOrder && item != null && ((INLotSerClass)item).LotSerTrack == INLotSerTrack.SerialNumbered && line?.IsSpecialOrder != true;

		protected override bool GenerateLotSerialNumberOnPersist(SOLine line) => base.GenerateLotSerialNumberOnPersist(line) && IsLSEntryEnabled;
		
		protected override decimal? GetSerialStatusAvailableQty(ILotSerial lsmaster, IStatus accumavail)
		{
			decimal? availableQty = base.GetSerialStatusAvailableQty(lsmaster, accumavail);

			var siteLotSerial = INSiteLotSerial.PK.Find(Base, lsmaster.InventoryID, lsmaster.SiteID, lsmaster.LotSerialNbr);
			var accumSiteLotSerial = (SiteLotSerial)Base.Caches[typeof(SiteLotSerial)].Locate(new SiteLotSerial()
			{
				InventoryID = lsmaster.InventoryID,
				SiteID = lsmaster.SiteID,
				LotSerialNbr = lsmaster.LotSerialNbr,
			});

			decimal siteAvailableQty = (siteLotSerial?.QtyAvail ?? 0m) + (accumSiteLotSerial?.QtyAvail ?? 0m);
			return Math.Min(availableQty ?? 0m, siteAvailableQty);
		}

		protected override decimal? GetSerialStatusQtyOnHand(ILotSerial lsmaster)
		{
			decimal? qtyOnHand = base.GetSerialStatusQtyOnHand(lsmaster);

			var siteLotSerial = INSiteLotSerial.PK.Find(Base, lsmaster.InventoryID, lsmaster.SiteID, lsmaster.LotSerialNbr);
			var accumSiteLotSerial = (SiteLotSerial)Base.Caches[typeof(SiteLotSerial)].Locate(new SiteLotSerial()
			{
				InventoryID = lsmaster.InventoryID,
				SiteID = lsmaster.SiteID,
				LotSerialNbr = lsmaster.LotSerialNbr,
			});

			decimal siteHardAvailQty = (siteLotSerial?.QtyHardAvail ?? 0m) + (accumSiteLotSerial?.QtyHardAvail ?? 0m);
			return Math.Min(qtyOnHand ?? 0m, siteHardAvailQty);
		}

		internal override List<ILotSerial> PerformSelectSerial<TLotSerialStatus>(PXSelectBase cmd, object[] pars)
		{
			// the objective of this method is only to cache INSiteLotSerial records
			var src = cmd.View.SelectMultiBound(pars);
			var dest = new List<ILotSerial>(src.Count);
			foreach (object row in src)
			{
				TLotSerialStatus status;
				if (row is PXResult res)
				{
					status = res.GetItem<TLotSerialStatus>();
					var siteStatus = res.GetItem<INSiteLotSerial>();
					if (siteStatus?.SiteID != null)
					{
						INSiteLotSerial.PK.StoreResult(Base, siteStatus);
					}
				}
				else
				{
					status = (TLotSerialStatus)row;
				}
				dest.Add(status);
			}
			return dest;
		}
		
		#region InvtMultScope

		public class SOLineInvtMultScope : InvtMultScope
		{
			public SOLineInvtMultScope(SOLine line)
				: base(line)
			{
				if (_reverse == true)
				{
					_line.OpenQty = -_line.OpenQty;
					_line.BaseOpenQty = -_line.BaseOpenQty;
					_line.ClosedQty = -_line.ClosedQty;
					_line.BaseClosedQty = -_line.BaseClosedQty;
				}
			}

			public SOLineInvtMultScope(SOLine line, SOLine oldLine)
				: base(line, oldLine)
			{
				if (_reverse == true)
				{
					_line.OpenQty = -_line.OpenQty;
					_line.BaseOpenQty = -_line.BaseOpenQty;
					_line.ClosedQty = -_line.ClosedQty;
					_line.BaseClosedQty = -_line.BaseClosedQty;
				}
				if (_reverseOld == true)
				{
					_oldLine.OpenQty = -_oldLine.OpenQty;
					_oldLine.BaseOpenQty = -_oldLine.BaseOpenQty;
					_oldLine.ClosedQty = -_oldLine.ClosedQty;
					_oldLine.BaseClosedQty = -_oldLine.BaseClosedQty;
				}
			}

			protected override bool IsReverse(SOLine line) => line.LineSign < 0;

			public override void Dispose()
			{
				base.Dispose();
				if (_reverse == true)
				{
					_line.OpenQty = -_line.OpenQty;
					_line.BaseOpenQty = -_line.BaseOpenQty;
					_line.ClosedQty = -_line.ClosedQty;
					_line.BaseClosedQty = -_line.BaseClosedQty;
				}
				if (_reverseOld == true)
				{
					_oldLine.OpenQty = -_oldLine.OpenQty;
					_oldLine.BaseOpenQty = -_oldLine.BaseOpenQty;
					_oldLine.ClosedQty = -_oldLine.ClosedQty;
					_oldLine.BaseClosedQty = -_oldLine.BaseClosedQty;
				}
			}
		}

		protected override IDisposable InvtMultModeScope(SOLine line) => new SOLineInvtMultScope(line);
		protected override IDisposable InvtMultModeScope(SOLine line, SOLine oldLine) => new SOLineInvtMultScope(line, oldLine);

		#endregion
	}
}
