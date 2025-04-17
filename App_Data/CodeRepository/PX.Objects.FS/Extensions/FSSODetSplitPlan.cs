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
using PX.Objects.IN.GraphExtensions;
using PX.Objects.SO;

namespace PX.Objects.FS
{
	public class FSSODetSplitPlan : ItemPlan<ServiceOrderEntry, FSServiceOrder, FSSODetSplit>
	{
		public override void Initialize()
		{
			base.Initialize();

			Base.FieldDefaulting.AddHandler<SiteStatusByCostCenter.negAvailQty>(NegAvailQtyFieldDefaulting);

			// we need to resubscribe the RowUpdated handler so that it becomes the last handler as we need to execute it after EPApprovalAutomation
			Base.RowUpdated.RemoveHandler<FSServiceOrder>(_);
			Base.RowUpdated.AddHandler<FSServiceOrder>(_);
		}

		protected virtual void NegAvailQtyFieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SOOrderType ordertype = PXSetup<SOOrderType>.Select(sender.Graph);

			if (e.Cancel == false && ordertype != null && ordertype.RequireAllocation == true)
			{
				e.NewValue = false;
				e.Cancel = true;
			}
		}

		public override void _(Events.RowUpdated<FSServiceOrder> e)
		{
			base._(e);

			WebDialogResult answer = WebDialogResult.Yes;

			bool DatesUpdated = !e.Cache.ObjectsEqual<FSServiceOrder.orderDate>(e.Row, e.OldRow) && (answer == WebDialogResult.Yes /*|| ((FSServiceOrder)e.Row).ShipComplete != SOShipComplete.BackOrderAllowed*/);
			bool RequestOnUpdated = !e.Cache.ObjectsEqual<FSServiceOrder.orderDate>(e.Row, e.OldRow) && (answer == WebDialogResult.Yes /*|| ((FSServiceOrder)e.Row).ShipComplete != SOShipComplete.BackOrderAllowed*/);
			//bool CreditHoldApprovedUpdated = !sender.ObjectsEqual<FSServiceOrder.creditHold>(e.Row, e.OldRow) || !sender.ObjectsEqual<FSServiceOrder.approved>(e.Row, e.OldRow);
			bool CustomerUpdated = !e.Cache.ObjectsEqual<FSServiceOrder.billCustomerID>(e.Row, e.OldRow);

			if (CustomerUpdated || DatesUpdated || RequestOnUpdated
				|| !e.Cache.ObjectsEqual<FSServiceOrder.hold, FSServiceOrder.status>(e.Row, e.OldRow))
			{
				//DatesUpdated |= !sender.ObjectsEqual<FSServiceOrder.shipComplete>(e.Row, e.OldRow) && ((FSServiceOrder)e.Row).ShipComplete != SOShipComplete.BackOrderAllowed;
				//RequestOnUpdated |= !sender.ObjectsEqual<FSServiceOrder.shipComplete>(e.Row, e.OldRow) && ((FSServiceOrder)e.Row).ShipComplete != SOShipComplete.BackOrderAllowed;

				bool cancelled = e.Row.Canceled == true;
				//bool? BackOrdered = (bool?)sender.GetValue<FSServiceOrder.backOrdered>(e.Row);

				PXCache fsSODetCache = Base.Caches<FSSODet>();
				PXCache splitcache = Base.Caches<FSSODetSplit>();

				SOOrderType ordertype = PXSetup<SOOrderType>.Select(Base);
				FSSrvOrdType srvOrdType = PXSetup<FSSrvOrdType>.Select(Base);
				FSBillingCycle billingCycle = PXSelect<FSBillingCycle>.Select(Base);
				string billingBy = ServiceOrderEntry.GetBillingMode(Base, billingCycle, srvOrdType, e.Row);

				var splitsByPlan = new Dictionary<long?, FSSODetSplit>();

				foreach (FSSODetSplit split in PXSelect<FSSODetSplit,
											   Where<
												   FSSODetSplit.srvOrdType, Equal<Current<FSServiceOrder.srvOrdType>>,
											   And<
												   FSSODetSplit.refNbr, Equal<Current<FSServiceOrder.refNbr>>>>>
											   .SelectMultiBound(Base, new[] { e.Row }))
				{
					FSSODet soDet = PXSelect<FSSODet,
											   Where<
												   FSSODet.srvOrdType, Equal<Required<FSSODet.srvOrdType>>,
											   And<
												   FSSODet.refNbr, Equal<Required<FSSODet.refNbr>>,
												And<
													FSSODet.lineNbr, Equal<Required<FSSODet.refNbr>>>>>>
											   .Select(Base, split.SrvOrdType, split.RefNbr, split.LineNbr);

					if (cancelled)
					{
						PlanCache.Inserted.RowCast<INItemPlan>()
										  .Where(_ => _.PlanID == split.PlanID)
										  .ForEach(_ => PlanCache.Delete(_));

						split.PlanID = null;
						split.Completed = true;

						splitcache.MarkUpdated(split);
					}
					else
					{
						if (e.OldRow.Canceled == true)
						{
							if (string.IsNullOrEmpty(split.ShipmentNbr)
									&& split.POCompleted == false)
							{
								split.Completed = false;
							}

							INItemPlan planl = DefaultValuesInt(new INItemPlan(), split);

							if (planl != null)
							{
								planl = (INItemPlan)PlanCache.Insert(planl);
								split.PlanID = planl.PlanID;
							}

							splitcache.MarkUpdated(split);
						}

						if (DatesUpdated)
						{
							split.ShipDate = e.Row.OrderDate;
							splitcache.MarkUpdated(split);
						}

						if (split.PlanID != null)
						{
							splitsByPlan[split.PlanID] = split;
						}

						if (e.OldRow.Closed == true)
						{
							if (string.IsNullOrEmpty(split.ShipmentNbr)
									&& split.POCompleted == false
									&& split.Completed == true
									&& split.LastModifiedByScreenID == ID.ScreenID.SERVICE_ORDER
									&& billingBy == ID.Billing_By.APPOINTMENT)
							{
								soDet.BaseShippedQty -= split.BaseShippedQty;
								soDet.ShippedQty -= split.ShippedQty;
								soDet.OpenQty = soDet.OrderQty - soDet.ShippedQty;
								soDet.BaseOpenQty = soDet.BaseOrderQty - soDet.BaseShippedQty;
								soDet.ClosedQty = soDet.ShippedQty;
								soDet.BaseClosedQty = soDet.BaseShippedQty;

								fsSODetCache.MarkUpdated(soDet);

								split.Completed = false;
								split.ShippedQty = 0;

								INItemPlan plan = DefaultValuesInt(new INItemPlan(), split);

								if (plan != null)
								{
									plan = (INItemPlan)PlanCache.Insert(plan);
									split.PlanID = plan.PlanID;
								}

								splitcache.MarkUpdated(split);
							}
						}
					}
				}

				foreach (FSSODet line in PXSelect<FSSODet,
										 Where<
											 FSSODet.srvOrdType, Equal<Current<FSServiceOrder.srvOrdType>>,
										 And<
											 FSSODet.refNbr, Equal<Current<FSServiceOrder.refNbr>>>>>
										 .SelectMultiBound(Base, new[] { e.Row }))
				{
					if (cancelled)
					{
						FSSODet old_row = PXCache<FSSODet>.CreateCopy(line);
						//line.UnbilledQty -= line.OpenQty;
						line.OpenQty = 0m;
						//linecache.RaiseFieldUpdated<FSSODet.unbilledQty>(line, 0m);
						fsSODetCache.RaiseFieldUpdated<FSSODet.openQty>(line, 0m);

						line.Completed = true;
						this.ResetAvailabilityCounters(line);

						//SOOrderEntry_SOOrder_RowUpdated should execute later to correctly update balances
						//+++//TaxAttribute.Calculate<FSSODet.taxCategoryID>(linecache, new PXRowUpdatedEventArgs(line, old_row, false));

						fsSODetCache.MarkUpdated(line);
					}
					else
					{
						if (e.OldRow.Canceled == true)
						{
							FSSODet old_row = PXCache<FSSODet>.CreateCopy(line);
							line.OpenQty = line.OrderQty;
							/*line.UnbilledQty += line.OpenQty;
                            object value = line.UnbilledQty;
                            linecache.RaiseFieldVerifying<FSSODet.unbilledQty>(line, ref value);
                            linecache.RaiseFieldUpdated<FSSODet.unbilledQty>(line, value);*/

							object value = line.OpenQty;
							fsSODetCache.RaiseFieldVerifying<FSSODet.openQty>(line, ref value);
							fsSODetCache.RaiseFieldUpdated<FSSODet.openQty>(line, value);

							line.Completed = false;

							//+++++//
							//TaxAttribute.Calculate<FSSODet.taxCategoryID>(linecache, new PXRowUpdatedEventArgs(line, old_row, false));

							fsSODetCache.MarkUpdated(line);
						}
						if (DatesUpdated)
						{
							line.ShipDate = e.Row.OrderDate;
							fsSODetCache.MarkUpdated(line);
						}
						/*if (RequestOnUpdated)
                        {
                            line.RequestDate = (DateTime?)sender.GetValue<FSServiceOrder.requestDate>(e.Row);
                            linecache.MarkUpdated(line);
                        }*/
						if (/*CreditHoldApprovedUpdated ||*/ !e.Cache.ObjectsEqual<FSServiceOrder.hold>(e.Row, e.OldRow))
						{
							this.ResetAvailabilityCounters(line);
						}
					}
				}

				if (cancelled)
				{
					//PXFormulaAttribute.CalcAggregate<FSSODet.unbilledQty>(linecache, e.Row);
					PXFormulaAttribute.CalcAggregate<FSSODet.openQty>(fsSODetCache, e.Row);
				}

				PXSelectBase<INItemPlan> cmd = new PXSelect<INItemPlan, Where<INItemPlan.refNoteID, Equal<Current<FSServiceOrder.noteID>>>>(Base);

				//BackOrdered is tri-state
				/*if (BackOrdered == true && sender.GetValue<FSServiceOrder.lastSiteID>(e.Row) != null && sender.GetValue<FSServiceOrder.lastShipDate>(e.Row) != null)
                {
                    cmd.WhereAnd<Where<INItemPlan.siteID, Equal<Current<FSServiceOrder.lastSiteID>>, And<INItemPlan.planDate, LessEqual<Current<FSServiceOrder.lastShipDate>>>>>();
                }

                if (BackOrdered == false)
                {
                    sender.SetValue<FSServiceOrder.lastSiteID>(e.Row, null);
                    sender.SetValue<FSServiceOrder.lastShipDate>(e.Row, null);
                }*/

				foreach (INItemPlan plan in cmd.View.SelectMultiBound(new[] { e.Row }))
				{
					if (cancelled)
					{
						PlanCache.Delete(plan);
					}
					else
					{
						INItemPlan copy = PXCache<INItemPlan>.CreateCopy(plan);

						if (DatesUpdated)
						{
							plan.PlanDate = e.Row.OrderDate;
						}
						if (CustomerUpdated)
						{
							plan.BAccountID = e.Row.CustomerID;
						}
						plan.Hold = IsOrderOnHold(e.Row);

						if (splitsByPlan.TryGetValue(plan.PlanID, out FSSODetSplit split))
						{
							plan.PlanType = CalcPlanType(plan, e.Row, split/*, BackOrdered*/);

							if (!string.Equals(copy.PlanType, plan.PlanType))
							{
								PlanCache.RaiseRowUpdated(plan, copy);
							}
						}

						if (PlanCache.GetStatus(plan).IsIn(PXEntryStatus.Notchanged, PXEntryStatus.Held))
						{
							PlanCache.SetStatus(plan, PXEntryStatus.Updated);
						}
					}
				}
				// FSServiceOrder.BackOrdered value should be handled only single time and only in this method
				// sender.SetValue<FSServiceOrder.backOrdered>(e.Row, null);
			}
		}

		protected virtual bool IsOrderOnHold(FSServiceOrder order)
		{
			return (order != null) && ((order.Hold ?? false)) /*|| (order.CreditHold ?? false) || (!order.Approved ?? false))*/;
		}

		public virtual void ResetAvailabilityCounters(FSSODet row)
		{
			row.LineQtyAvail = null;
			row.LineQtyHardAvail = null;
		}

		bool _initPlan = false;
		bool _initVendor = false;
		bool _resetSupplyPlanID = false;
		public override void _(Events.RowUpdated<FSSODetSplit> e)
		{
			//respond only to GUI operations
			var isLinked = IsLineLinked(e.Row);

			_initPlan = InitPlanRequired(e.Row, e.OldRow) && !isLinked;

			FSSODet parent = (FSSODet)PXParentAttribute.SelectParent(e.Cache, e.Row, typeof(FSSODet));

			_initVendor = !e.Cache.ObjectsEqual<FSSODetSplit.siteID, FSSODetSplit.subItemID, FSSODetSplit.vendorID, FSSODetSplit.pOCreate>(e.Row, e.OldRow) && !isLinked;

			_initVendor = _initVendor || parent.POVendorLocationID != null;

			_resetSupplyPlanID = !isLinked;

			try
			{
				base._(e);
			}
			finally
			{
				_initPlan = false;
				_resetSupplyPlanID = false;
			}
		}

		protected virtual bool InitPlanRequired(FSSODetSplit row, FSSODetSplit oldRow)
		{
			return !Base.Caches<FSSODetSplit>()
				.ObjectsEqual<FSSODetSplit.isAllocated,
					FSSODetSplit.siteID,
					FSSODetSplit.pOCreate,
					FSSODetSplit.pOSource,
					FSSODetSplit.operation>(row, oldRow);
		}

		protected virtual string CalcPlanType(INItemPlan plan, FSServiceOrder order, FSSODetSplit split, bool? backOrdered = null)
		{
			if (split.POCreate == true)
			{
				return INPlanConstants.PlanF6;
			}

			SOOrderType ordertype = PXSetup<SOOrderType>.Select(Base);
			bool isAllocation = (split.IsAllocated == true) || INPlanConstants.IsAllocated(plan.PlanType) || INPlanConstants.IsFixed(plan.PlanType);
			bool isOrderOnHold = IsOrderOnHold(order) && ordertype.RequireAllocation != true;

			string calcedPlanType = CalcPlanType(plan, split, ordertype, isOrderOnHold);
			bool putOnSOPrepared = (calcedPlanType == INPlanConstants.PlanF0);

			if (!_initPlan && !putOnSOPrepared && !isAllocation)
			{
				if (backOrdered == true || backOrdered == null && plan.PlanType == INPlanConstants.Plan68)
				{
					return INPlanConstants.Plan68;
				}
			}

			return calcedPlanType;
		}

		protected virtual string CalcPlanType(INItemPlan plan, FSSODetSplit split, SOOrderType ordertype, bool isOrderOnHold)
		{
			if (ordertype == null || ordertype.RequireShipping == true)
			{
				return (split.IsAllocated == true) ? split.AllocatedPlanType
					: isOrderOnHold ? INPlanConstants.PlanF0
					: (split.RequireAllocation != true || split.IsStockItem != true) ? split.PlanType : split.BackOrderPlanType;
			}
			else
			{
				return (isOrderOnHold != true || split.IsStockItem != true) ? split.PlanType : INPlanConstants.PlanF0;
			}
		}

		protected virtual bool IsLineLinked(FSSODetSplit soLineSplit)
		{
			return soLineSplit != null && (soLineSplit.PONbr != null || soLineSplit.SOOrderNbr != null && soLineSplit.IsAllocated == true);
		}

		public override INItemPlan DefaultValues(INItemPlan planRow, FSSODetSplit splitRow)
		{
			if (splitRow.Completed == true || splitRow.POCompleted == true || splitRow.LineType == SOLineType.MiscCharge || splitRow.LineType == SOLineType.NonInventory && splitRow.RequireShipping == false)
			{
				return null;
			}

			FSSODet parent = (FSSODet)PXParentAttribute.SelectParent(ItemPlanSourceCache, splitRow, typeof(FSSODet));
			FSServiceOrder order = (FSServiceOrder)PXParentAttribute.SelectParent(ItemPlanSourceCache, splitRow, typeof(FSServiceOrder));

			if (string.IsNullOrEmpty(planRow.PlanType) || _initPlan)
			{
				planRow.PlanType = CalcPlanType(planRow, order, splitRow);

				if (splitRow.POCreate == true)
				{
					planRow.FixedSource = INReplenishmentSource.Purchased;

					if (splitRow.POType != PO.POOrderType.Blanket && splitRow.POType != PO.POOrderType.DropShip && splitRow.POSource == INReplenishmentSource.PurchaseToOrder)
						planRow.SourceSiteID = splitRow.SiteID;
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
				planRow.VendorLocationID = parent?.POVendorLocationID;

				if (planRow.VendorLocationID == null)
				{
					planRow.VendorLocationID = PO.POItemCostManager.FetchLocation(Base,
																				  splitRow.VendorID,
																				  splitRow.InventoryID,
																				  splitRow.SubItemID,
																				  splitRow.SiteID);
				}
			}

			planRow.BAccountID = parent == null ? null : parent.BillCustomerID;
			planRow.InventoryID = splitRow.InventoryID;
			planRow.SubItemID = splitRow.SubItemID;
			planRow.SiteID = splitRow.SiteID;
			planRow.LocationID = splitRow.LocationID;
			planRow.LotSerialNbr = splitRow.LotSerialNbr;

			if (string.IsNullOrEmpty(splitRow.AssignedNbr) == false && INLotSerialNbrAttribute.StringsEqual(splitRow.AssignedNbr, splitRow.LotSerialNbr))
			{
				planRow.LotSerialNbr = null;
			}

			planRow.PlanDate = splitRow.ShipDate;
			planRow.UOM = parent?.UOM;
			planRow.PlanQty = (splitRow.POCreate == true ? splitRow.BaseUnreceivedQty - splitRow.BaseShippedQty : splitRow.BaseQty);

			planRow.RefNoteID = order.NoteID;
			planRow.Hold = IsOrderOnHold(order);

			if (string.IsNullOrEmpty(planRow.PlanType))
			{
				return null;
			}

			return planRow;
		}

		public INItemPlan DefaultValues(FSSODetSplit split) => DefaultValuesInt(new INItemPlan(), split);
	}
}
