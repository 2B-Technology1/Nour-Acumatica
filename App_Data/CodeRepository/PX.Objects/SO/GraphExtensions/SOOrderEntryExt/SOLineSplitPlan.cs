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
using PX.Objects.IN.GraphExtensions;
using PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated;
using PX.Objects.TX;

namespace PX.Objects.SO.GraphExtensions.SOOrderEntryExt
{
	public class SOLineSplitPlan : ItemPlan<SOOrderEntry, SOOrder, SOLineSplit>
	{
		#region State

		protected Dictionary<object[], HashSet<Guid?>> _processingSets;
		bool _initPlan = false;
		bool _initVendor = false;
		bool _resetSupplyPlanID = false;

		#endregion

		#region Initialization

		public override void Initialize()
		{
			base.Initialize();

			_processingSets = new Dictionary<object[], HashSet<Guid?>>();
			Base.FieldDefaulting.AddHandler<SiteStatusByCostCenter.negAvailQty>(NegAvailQtyFieldDefaulting);

			// we need to resubscribe the RowUpdated handler so that it becomes the last handler as we need to execute it after EPApprovalAutomation
			Base.RowUpdated.RemoveHandler<SOOrder>(_);
			Base.RowUpdated.AddHandler<SOOrder>(_);
		}

		#endregion

		#region Event Handlers

		#region SOOrder

		public override void _(Events.RowUpdated<SOOrder> e)
		{
			base._(e);

			WebDialogResult answer = Base.Document.View.Answer;
			bool datesUpdated = !e.Cache.ObjectsEqual<SOOrder.shipDate>(e.Row, e.OldRow) && (answer == WebDialogResult.Yes || e.Row.ShipComplete != SOShipComplete.BackOrderAllowed);
			bool requestOnUpdated = !e.Cache.ObjectsEqual<SOOrder.requestDate>(e.Row, e.OldRow) && (answer == WebDialogResult.Yes || e.Row.ShipComplete != SOShipComplete.BackOrderAllowed);
			bool creditHoldApprovedUpdated = !e.Cache.ObjectsEqual<SOOrder.creditHold, SOOrder.approved>(e.Row, e.OldRow);
			bool customerUpdated = !e.Cache.ObjectsEqual<SOOrder.customerID>(e.Row, e.OldRow);

			if (customerUpdated || datesUpdated || requestOnUpdated || creditHoldApprovedUpdated
				|| !e.Cache.ObjectsEqual<SOOrder.hold,
										SOOrder.cancelled,
										SOOrder.completed,
										SOOrder.backOrdered,
										SOOrder.shipComplete,
										SOOrder.prepaymentReqSatisfied,
										SOOrder.isExpired>(e.Row, e.OldRow))
			{
				datesUpdated |= !e.Cache.ObjectsEqual<SOOrder.shipComplete>(e.Row, e.OldRow) && e.Row.ShipComplete != SOShipComplete.BackOrderAllowed;
				requestOnUpdated |= !e.Cache.ObjectsEqual<SOOrder.shipComplete>(e.Row, e.OldRow) && e.Row.ShipComplete != SOShipComplete.BackOrderAllowed;

				bool cancelled = e.Row.Cancelled == true;
				bool completed = e.Row.Completed == true;
				bool? backOrdered = e.Row.BackOrdered;

				PXCache splitCache = Base.Caches<SOLineSplit>();
				PXCache lineCache = Base.Caches<SOLine>();

				SOOrderType ordertype = PXSetup<SOOrderType>.Select(Base);
				var unlinkedLines = new Dictionary<int?, bool>();
				var splitsByPlan = new Dictionary<long?, SOLineSplit>();
				var plansToDelete = new HashSet<long?>();
				foreach (IEnumerable<SOLineSplit> lineSplits in PXSelect<SOLineSplit,
					Where<SOLineSplit.orderType, Equal<Current<SOOrder.orderType>>,
						And<SOLineSplit.orderNbr, Equal<Current<SOOrder.orderNbr>>>>>
					.SelectMultiBound(Base, new[] { e.Row })
					.RowCast<SOLineSplit>()
					.GroupBy(x => x.LineNbr)
					.ToDictionary(x => x.Key, x => x.ToList()).Values)
				{
					bool clearPOLinks = false;
					bool newDropShipLine = IsDropShipNotLegacy(lineSplits.FirstOrDefault());
					if (!newDropShipLine && cancelled && lineSplits.Any(x => x.Completed != true && !string.IsNullOrEmpty(x.PONbr)))
					{
						clearPOLinks = true;
						unlinkedLines.Add(lineSplits.First().LineNbr, !lineSplits.Any(x => x.Completed == true && !string.IsNullOrEmpty(x.PONbr)));
					}

					foreach (SOLineSplit split in lineSplits)
					{
						if (cancelled || completed)
						{
							if (split.Completed != true)
							{
								PlanCache.Inserted.RowCast<INItemPlan>()
									.Where(_ => _.PlanID == split.PlanID)
									.ForEach(_ => PlanCache.Delete(_));
								if (clearPOLinks)
								{
									split.POCreate = false;
									if (!string.IsNullOrEmpty(split.PONbr))
									{
										split.ClearPOReferences();
										split.RefNoteID = null;
									}
								}
								split.PlanID = null;
								split.Completed = true;

								splitCache.MarkUpdated(split, assertError: true);
							}
						}
						else
						{
							InventoryItem item = InventoryItem.PK.Find(Base, split.InventoryID);
							bool isConverted = item?.IsConverted == true && split.IsStockItem != null && split.IsStockItem != item.StkItem;

							if (!isConverted && e.OldRow.Cancelled == true)
							{
								if (string.IsNullOrEmpty(split.ShipmentNbr) && split.POCompleted == false)
								{
									split.Completed = false;
								}

								INItemPlan plan = DefaultValuesInt(new INItemPlan(), split);
								if (plan != null)
								{
									plan = (INItemPlan)PlanCache.Insert(plan);
									split.PlanID = plan.PlanID;
								}

								splitCache.MarkUpdated(split, assertError: true);
							}

							if (!e.Cache.ObjectsEqual<SOOrder.isExpired>(e.OldRow, e.Row)
								&& split.POCreate == true && string.IsNullOrEmpty(split.PONbr))
							{
								if (e.Row.IsExpired == false && split.PlanID == null)
								{
									INItemPlan plan = DefaultValuesInt(new INItemPlan(), split);
									if (plan != null)
									{
										plan = (INItemPlan)PlanCache.Insert(plan);
										split.PlanID = plan.PlanID;
										splitCache.MarkUpdated(split, assertError: true);
									}
								}
								else if (e.Row.IsExpired == true && split.PlanID != null)
								{
									plansToDelete.Add(split.PlanID);
									split.PlanID = null;
									splitCache.MarkUpdated(split, assertError: true);
								}
							}

							if (datesUpdated && split.Completed != true)
							{
								split.ShipDate = e.Row.ShipDate;
								splitCache.MarkUpdated(split, assertError: true);
							}

							if (split.PlanID != null)
							{
								splitsByPlan[split.PlanID] = split;
							}
						}
					}
				}

				bool IsUnbilledOrOpenQtyRecalculationNeeded = false;

				foreach (SOLine line in PXSelect<SOLine,
					Where<SOLine.orderType, Equal<Current<SOOrder.orderType>>,
						And<SOLine.orderNbr, Equal<Current<SOOrder.orderNbr>>,
						And<SOLine.lineType, NotEqual<SOLineType.miscCharge>>>>>
					.SelectMultiBound(Base, new[] { e.Row }))
				{
					if (cancelled || completed)
					{
						if (line.Completed != true)
						{
							SOLine oldRow = PXCache<SOLine>.CreateCopy(line);
							line.UnbilledQty -= line.OpenQty;
							line.OpenQty = 0m;
							lineCache.RaiseFieldUpdated<SOLine.unbilledQty>(line, 0m);
							lineCache.RaiseFieldUpdated<SOLine.openQty>(line, 0m);

							if (unlinkedLines.TryGetValue(line.LineNbr, out bool clearPOCreated))
							{
								if (clearPOCreated)
									line.POCreated = false;
								if (line.POCreate == true)
								{
									line.POCreate = false;
									lineCache.RaiseFieldUpdated<SOLine.pOCreate>(line, true);
								}
							}
							line.Completed = true;
							SOOrderLineSplittingAllocatedExtension.ResetAvailabilityCounters(line);

							//SOOrderEntry_SOOrder_RowUpdated should execute later to correctly update balances
							TaxAttribute.Calculate<SOLine.taxCategoryID>(lineCache, new PXRowUpdatedEventArgs(line, oldRow, false));

							IsUnbilledOrOpenQtyRecalculationNeeded = true;
							lineCache.MarkUpdated(line, assertError: true);
						}
					}
					else
					{
						if (e.OldRow.Cancelled == true)
						{
							InventoryItem item = InventoryItem.PK.Find(Base, line.InventoryID);
							bool isConverted = item?.IsConverted == true && line.IsStockItem != null && line.IsStockItem != item.StkItem;

							if (!isConverted)
							{
								SOLine oldRow = PXCache<SOLine>.CreateCopy(line);
								line.OpenQty = line.OrderQty - line.ShippedQty;
								line.UnbilledQty += line.OpenQty;
								object value = line.UnbilledQty;
								lineCache.RaiseFieldVerifying<SOLine.unbilledQty>(line, ref value);
								lineCache.RaiseFieldUpdated<SOLine.unbilledQty>(line, value);

								value = line.OpenQty;
								lineCache.RaiseFieldVerifying<SOLine.openQty>(line, ref value);
								lineCache.RaiseFieldUpdated<SOLine.openQty>(line, value);

								lineCache.SetValueExt<SOLine.completed>(line, false);

								TaxAttribute.Calculate<SOLine.taxCategoryID>(lineCache, new PXRowUpdatedEventArgs(line, oldRow, false));
							}
							else
							{
								lineCache.SetValueExt<SOLine.openLine>(line, false);
							}

							IsUnbilledOrOpenQtyRecalculationNeeded = true;

							lineCache.MarkUpdated(line, assertError: true);
						}
						if (line.Completed != true)
						{
							if (datesUpdated)
							{
								line.ShipDate = e.Row.ShipDate;
								lineCache.MarkUpdated(line, assertError: true);
							}
							if (requestOnUpdated)
							{
								line.RequestDate = e.Row.RequestDate;
								lineCache.MarkUpdated(line, assertError: true);
							}
						}
						if (creditHoldApprovedUpdated || !e.Cache.ObjectsEqual<SOOrder.hold>(e.Row, e.OldRow))
						{
							SOOrderLineSplittingAllocatedExtension.ResetAvailabilityCounters(line);
						}
					}
				}

				if (IsUnbilledOrOpenQtyRecalculationNeeded)
				{
					PXFormulaAttribute.CalcAggregate<SOLine.unbilledQty>(lineCache, e.Row);
					PXFormulaAttribute.CalcAggregate<SOLine.openQty>(lineCache, e.Row);
				}

				PXSelectBase<INItemPlan> cmd = new PXSelect<INItemPlan, Where<INItemPlan.refNoteID, Equal<Current<SOOrder.noteID>>>>(Base);

				//BackOrdered is tri-state
				if (backOrdered == true && e.Row.LastSiteID != null && e.Row.LastShipDate != null)
				{
					cmd.WhereAnd<Where<INItemPlan.siteID, Equal<Current<SOOrder.lastSiteID>>,
						And<INItemPlan.planDate, LessEqual<Current<SOOrder.lastShipDate>>>>>();
				}

				if (backOrdered == false)
				{
					e.Row.LastSiteID = null;
					e.Row.LastShipDate = null;
				}

				foreach (var plan in cmd.View.SelectMultiBound(new[] { e.Row }).Cast<INItemPlan>().Where(plan => plan.IsSkippedWhenBackOrdered != true))
				{
					if (cancelled || completed || plansToDelete.Contains(plan.PlanID))
					{
						PlanCache.Delete(plan);
					}
					else
					{
						INItemPlan copy = PXCache<INItemPlan>.CreateCopy(plan);

						if (datesUpdated)
						{
							plan.PlanDate = e.Row.ShipDate;
						}
						if (customerUpdated)
						{
							plan.BAccountID = e.Row.CustomerID;
						}
						plan.Hold = IsOrderOnHold(e.Row);

						// We should skip allocated plans. In general we should process only "normal" plans.
						if (IsPlanRegular(ordertype, plan))
						{
							if (splitsByPlan.TryGetValue(plan.PlanID, out SOLineSplit split))
							{
								plan.PlanType = CalcPlanType(plan, e.Row, split, backOrdered);

								if (!string.Equals(copy.PlanType, plan.PlanType))
								{
									PlanCache.RaiseRowUpdated(plan, copy);
								}
							}
						}

						PlanCache.MarkUpdated(plan, assertError: true);
					}
				}
				// SOOrder.BackOrdered value should be handled only single time and only in this method
				e.Row.BackOrdered = null;
			}

			RecalcOpenLineCounters(e.Row, e.OldRow);
		}

		public virtual bool IsPlanRegular(SOOrderType ordertype, INItemPlan plan) =>
			(ordertype.RequireAllocation != true)
			&& plan.PlanType.IsIn(INPlanConstants.Plan60, INPlanConstants.Plan62, INPlanConstants.Plan68, INPlanConstants.Plan69);

		#endregion

		#region SOLineSplit

		public override void _(Events.RowUpdated<SOLineSplit> e)
		{
			//respond only to GUI operations
			var isLinked = IsLineLinked(e.Row);

			_initPlan = InitPlanRequired(e.Row, e.OldRow) && !isLinked;
			_initVendor = !e.Cache.ObjectsEqual<SOLineSplit.siteID, SOLineSplit.subItemID, SOLineSplit.vendorID, SOLineSplit.pOCreate>(e.Row, e.OldRow)
				&& !isLinked;
			_resetSupplyPlanID = !isLinked;

			try
			{
				base._(e);
			}
			finally
			{
				_initPlan = false;
				_initVendor = false;
				_resetSupplyPlanID = false;
			}
		}

		#endregion

		#region INItemPlan

		public override void _(Events.RowPersisting<INItemPlan> e)
		{
			// crutch for mass processing CreateShipment & PXProcessing<Table>.TryPersistPerRow
			if (e.Operation == PXDBOperation.Update)
			{
				PXCache cache = Base.Caches<SOOrder>();

				if (e.Row is INItemPlan row && cache.Current is SOOrder order
					&& row.RefNoteID != order.NoteID
					&& PXLongOperation.GetCustomInfo(Base.UID, PXProcessing.ProcessingKey, out object[] processingList) != null
					&& processingList != null)
				{
					if (!_processingSets.TryGetValue(processingList, out HashSet<Guid?> processingSet))
					{
						processingSet = processingList.Select(x => ((SOOrder)x).NoteID).ToHashSet();
						_processingSets[processingList] = processingSet;
					}

					if (processingSet.Contains(row.RefNoteID))
						e.Cancel = true;
				}
			}
			base._(e);
		}

		#endregion

		#region Common

		protected virtual void NegAvailQtyFieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SOOrderType orderType = PXSetup<SOOrderType>.Select(sender.Graph);
			if (e.Cancel == false && orderType != null && orderType.RequireAllocation == true)
			{
				e.NewValue = false;
				e.Cancel = true;
			}
		}

		#endregion

		#endregion

		#region DefaultValues

		public override INItemPlan DefaultValues(INItemPlan planRow, SOLineSplit splitRow)
		{
			if (splitRow.Completed == true
				|| splitRow.POCompleted == true
				|| splitRow.LineType == SOLineType.MiscCharge
				|| splitRow.LineType == SOLineType.NonInventory && splitRow.RequireShipping == false && splitRow.Behavior != SOBehavior.BL)
			{
				return null;
			}
			SOLine parent = PXParentAttribute.SelectParent<SOLine>(ItemPlanSourceCache, splitRow);
			SOOrder order = PXParentAttribute.SelectParent<SOOrder>(ItemPlanSourceCache, splitRow) ?? Base.Document.Current;

			if (string.IsNullOrEmpty(planRow.PlanType) || _initPlan || _resetSupplyPlanID && order.IsExpired == true)
			{
				planRow.PlanType = CalcPlanType(planRow, order, splitRow);
				if (splitRow.POCreate == true)
				{
					planRow.FixedSource = INReplenishmentSource.Purchased;
					if (splitRow.POType != PO.POOrderType.Blanket && splitRow.POType != PO.POOrderType.DropShip && splitRow.POSource == INReplenishmentSource.PurchaseToOrder)
						planRow.SourceSiteID = order.DestinationSiteID ?? splitRow.SiteID;
					else
						planRow.SourceSiteID = splitRow.SiteID;
				}
				else
				{
					planRow.Reverse = (splitRow.Operation == SOOperation.Receipt);

					planRow.FixedSource = (splitRow.SiteID != splitRow.ToSiteID ? INReplenishmentSource.Transfer : INReplenishmentSource.None);
					planRow.SourceSiteID = splitRow.SiteID;
				}
			}

			if (_resetSupplyPlanID)
			{
				planRow.SupplyPlanID = null;
			}

			planRow.VendorID = splitRow.VendorID;

			if (_initVendor || splitRow.POCreate == true && planRow.VendorID != null && planRow.VendorLocationID == null)
			{
				planRow.VendorLocationID =
					PO.POItemCostManager.FetchLocation(
					Base,
					splitRow.VendorID,
					splitRow.InventoryID,
					splitRow.SubItemID,
					splitRow.SiteID);
			}

			planRow.BAccountID = parent?.CustomerID;
			planRow.InventoryID = splitRow.InventoryID;
			planRow.SubItemID = splitRow.SubItemID;
			planRow.SiteID = splitRow.SiteID;
			planRow.LocationID = splitRow.LocationID;
			planRow.LotSerialNbr = splitRow.LotSerialNbr;
			planRow.IsTempLotSerial = string.IsNullOrEmpty(splitRow.AssignedNbr) == false &&
				INLotSerialNbrAttribute.StringsEqual(splitRow.AssignedNbr, splitRow.LotSerialNbr);
			planRow.ProjectID = parent?.ProjectID;
			planRow.TaskID = parent?.TaskID;
			planRow.CostCenterID = splitRow.CostCenterID;

			if (planRow.IsTempLotSerial == true)
			{
				planRow.LotSerialNbr = null;
			}
			planRow.PlanDate = splitRow.ShipDate;
			planRow.UOM = parent?.UOM;
			planRow.PlanQty =
				(splitRow.POCreate == true
					? splitRow.BaseUnreceivedQty - ((splitRow.Behavior == SOBehavior.BL) ? 0m : splitRow.BaseShippedQty)
					: splitRow.BaseQty)
				- splitRow.BaseQtyOnOrders;

			planRow.RefNoteID = order.NoteID;
			planRow.Hold = IsOrderOnHold(order);

			if (string.IsNullOrEmpty(planRow.PlanType))
			{
				return null;
			}
			return planRow;
		}

		#endregion

		#region Helper methods

		protected virtual void RecalcOpenLineCounters(SOOrder newRow, SOOrder oldRow)
		{
			var orderCache = Base.Caches[typeof(SOOrder)];

			if (!orderCache.ObjectsEqual<SOOrder.cancelled, SOOrder.completed>(newRow, oldRow))
			{
				PXCache orderSiteCache = Base.Caches[typeof(SOOrderSite)];
				PXCache lineCache = Base.Caches[typeof(SOLine)];

				var orderSites = PXParentAttribute.SelectChildren(orderSiteCache, newRow, typeof(SOOrder));

				foreach (SOOrderSite orderSite in orderSites)
				{
					PXFormulaAttribute.CalcAggregate<SOLine.siteID>(lineCache, orderSite);
					orderSiteCache.MarkUpdated(orderSite, assertError: true);
				}

				PXFormulaAttribute.CalcAggregate<SOOrderSite.openShipmentCntr>(orderSiteCache, newRow);
				PXFormulaAttribute.CalcAggregate<SOLine.openLine>(lineCache, newRow);
			}
		}

		public virtual bool IsDropShipNotLegacy(SOLineSplit split)
		{
			SOLine soLine = split != null ? PXParentAttribute.SelectParent<SOLine>(ItemPlanSourceCache, split) : null;
			return soLine != null && soLine.POCreate == true && soLine.POSource == INReplenishmentSource.DropShipToOrder && soLine.IsLegacyDropShip != true;
		}

		protected virtual bool InitPlanRequired(SOLineSplit row, SOLineSplit oldRow)
		{
			return !Base.Caches<SOLineSplit>()
				.ObjectsEqual<SOLineSplit.isAllocated,
					SOLineSplit.siteID,
					SOLineSplit.pOCreate,
					SOLineSplit.pOSource,
					SOLineSplit.operation>(row, oldRow);
		}

		protected virtual bool IsLineLinked(SOLineSplit soLineSplit)
		{
			return soLineSplit != null && (soLineSplit.PONbr != null || soLineSplit.SOOrderNbr != null && soLineSplit.IsAllocated == true);
		}

		protected virtual bool IsOrderOnHold(SOOrder order)
		{
			return (order != null) && ((order.Hold ?? false) || (order.CreditHold ?? false) || (!order.Approved ?? false) || (!order.PrepaymentReqSatisfied ?? false));
		}

		protected virtual string CalcPlanType(INItemPlan plan, SOOrder order, SOLineSplit split, bool? backOrdered = null)
		{
			if (split.POCreate == true && split.Operation == SOOperation.Receipt)
				return null;

			SOLine soLine = PXParentAttribute.SelectParent<SOLine>(ItemPlanSourceCache, split);
			if (split.POCreate == true && soLine?.IsLegacyDropShip == true)
			{
				if (split.POType == PO.POOrderType.Blanket)
				{
					return (split.POSource == INReplenishmentSource.DropShipToOrder)
						? INPlanConstants.Plan6E
						: INPlanConstants.Plan6B;
				}
				else
				{
					return (split.POSource == INReplenishmentSource.DropShipToOrder)
						? INPlanConstants.Plan6D
						: INPlanConstants.Plan66;
				}
			}

			if (split.POSource == INReplenishmentSource.DropShipToOrder && (split.POCreate == true || split.PONbr != null))
				return INPlanConstants.Plan6D;

			if (split.POCreate == true && split.POSource == INReplenishmentSource.BlanketDropShipToOrder)
				return INPlanConstants.Plan6E;

			if (split.POCreate == true && split.POSource == INReplenishmentSource.BlanketPurchaseToOrder)
				return order.IsExpired == true && string.IsNullOrEmpty(split.PONbr) ? null : INPlanConstants.Plan6B;

			if (split.POCreate == true && split.POSource == INReplenishmentSource.PurchaseToOrder)
				return order.IsExpired == true && string.IsNullOrEmpty(split.PONbr) ? null : INPlanConstants.Plan66;

			SOOrderType ordertype = PXSetup<SOOrderType>.Select(Base);
			bool isAllocation = (split.IsAllocated == true) || INPlanConstants.IsAllocated(plan.PlanType) || INPlanConstants.IsFixed(plan.PlanType);
			bool isOrderOnHold = IsOrderOnHold(order) && ordertype.RequireAllocation != true;

			string calcedPlanType = CalcPlanType(plan, split, ordertype, isOrderOnHold);
			bool putOnSOPrepared = (calcedPlanType == INPlanConstants.Plan69);

			if (!_initPlan && !putOnSOPrepared && !isAllocation)
			{
				if (backOrdered == true || backOrdered == null && plan.PlanType == INPlanConstants.Plan68)
				{
					return INPlanConstants.Plan68;
				}
			}

			return calcedPlanType;
		}

		protected virtual string CalcPlanType(INItemPlan plan, SOLineSplit split, SOOrderType ordertype, bool isOrderOnHold)
		{
			if (split.Behavior == SOBehavior.BL)
			{
				return (split.IsAllocated == true) ? split.AllocatedPlanType : null;
			}
			else if (ordertype == null || ordertype.RequireShipping == true)
			{
				return (split.IsAllocated == true) ? split.AllocatedPlanType
					: isOrderOnHold ? INPlanConstants.Plan69
					: (split.RequireAllocation != true || split.IsStockItem != true) ? split.PlanType : split.BackOrderPlanType;
			}
			else
			{
				return (isOrderOnHold != true || split.IsStockItem != true) ? split.PlanType : INPlanConstants.Plan69;
			}
		}

		#endregion
	}
}
