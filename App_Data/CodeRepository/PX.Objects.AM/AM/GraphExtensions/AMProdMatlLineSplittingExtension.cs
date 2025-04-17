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
using System.Linq;

using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

using PX.Objects.Common;
using PX.Objects.IN;

using SiteStatusByCostCenter = PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated.SiteStatusByCostCenter;
using SiteLotSerial = PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated.SiteLotSerial;
using LotSerOptions = PX.Objects.IN.LSSelect.LotSerOptions;
using Counters = PX.Objects.IN.LSSelect.Counters;

namespace PX.Objects.AM
{
	public abstract class AMProdMatlLineSplittingExtension<TGraph> : IN.GraphExtensions.LineSplittingExtension<TGraph, AMProdItem, AMProdMatl, AMProdMatlSplit>
		where TGraph : PXGraph
	{
		#region Configuration
		protected override Type SplitsToDocumentCondition => typeof(AMProdMatlSplit.FK.Material.SameAsCurrent);

		protected override Type LineQtyField => typeof(AMProdMatl.qtyRemaining);

		public override AMProdMatlSplit LineToSplit(AMProdMatl line)
		{
			using (new InvtMultScope(line))
			{
				AMProdMatlSplit ret = (AMProdMatlSplit)line;
				ret.BaseQty = line.BaseQty - line.UnassignedQty;
				return ret;
			}
		}
		#endregion

		#region Event Handlers
		#region AMProdMatl
		protected override void EventHandler(ManualEvent.Row<AMProdMatl>.Inserted.Args e)
		{
			if (e.Row == null)
				return;

			if (e.Row.InvtMult != 0)
			{
				base.EventHandler(e);

				// Field defaults on POCreate and ProdCreate not setting the splits... need this
				UpdateNonAllocatedSplits(e.Row, typeof(AMProdMatlSplit.pOCreate), typeof(AMProdMatlSplit.prodCreate));
			}
			else
			{
				e.Cache.SetValue<AMProdMatl.lotSerialNbr>(e.Row, null);
				e.Cache.SetValue<AMProdMatl.expireDate>(e.Row, null);
			}
		}

		protected override void EventHandler(ManualEvent.Row<AMProdMatl>.Updated.Args e)
		{
			if (e.Row == null || e.OldRow == null)
				return;

			if (!e.Cache.ObjectsEqual<
				AMProdMatl.statusID,
				AMProdMatl.vendorID,
				AMProdMatl.pOCreate,
				AMProdMatl.prodCreate,
				AMProdMatl.tranType,
				AMProdMatl.invtMult,
				AMProdMatl.siteID,
				AMProdMatl.locationID>(e.Row, e.OldRow) &&  e.Row.TranType != INTranType.Receipt)
			{
				UpdateNonAllocatedSplits(e.Row,
					typeof(AMProdMatlSplit.pOCreate),
					typeof(AMProdMatlSplit.vendorID),
					typeof(AMProdMatlSplit.prodCreate),
					typeof(AMProdMatlSplit.tranType),
					typeof(AMProdMatlSplit.invtMult),
					typeof(AMProdMatlSplit.siteID),
					typeof(AMProdMatlSplit.locationID));
			}

			base.EventHandler(e);
		}

		protected override void EventHandlerInternal(ManualEvent.Row<AMProdMatl>.Updated.Args e)
		{
			if (e.Row == null)
				return;

			var skipSplitCreating = false;
			var ii = (InventoryItem)PXSelectorAttribute.Select<AMProdMatl.inventoryID>(e.Cache, e.Row);
			var changeInQty = decimal.Compare(e.Row.BaseQtyRemaining?? 0, e.OldRow?.BaseQtyRemaining ?? 0m);
			if (ii != null && ii.StkItem == false)
				skipSplitCreating = true;

			if (!skipSplitCreating && IsLotSerialItem(e.Row) && changeInQty == 0) //check condition
			{
				base.EventHandlerInternal(e);

				if (ii != null && (ii.KitItem == true || ii.StkItem == true))
					Availability.Check(e.Row, CostCenter.FreeStock);
			}
			else
			{
				if (!IsLotSerialItem(e.Row))
				{
				e.Cache.SetValue<AMProdMatl.lotSerialNbr>(e.Row, null);
				e.Cache.SetValue<AMProdMatl.expireDate>(e.Row, null);
				}

				PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(e.Row.InventoryID);

				if (e.OldRow != null && !e.Cache.ObjectsEqual<AMProdMatl.inventoryID, AMProdMatl.subItemID, AMProdMatl.invtMult, AMProdMatl.uOM>(e.Row, e.OldRow))
				{
					RaiseRowDeleted(e.OldRow);
					RaiseRowInserted(e.Row);
				}
				else if (e.OldRow != null && e.OldRow.SiteID != e.Row.SiteID)
				{
					UpdateNonAllocatedSplits(e.Row, typeof(AMProdMatlSplit.siteID), typeof(AMProdMatlSplit.locationID));
				}
				else if (item != null)
				{
					var oldRowBaseQty = e.OldRow?.BaseQtyRemaining ?? 0m;
					var currentBaseQtyRemaining = (decimal?)PXFormulaAttribute.Evaluate<AMProdMatl.baseQtyRemaining>(e.Cache, e.Row) ?? 0m;
					if (currentBaseQtyRemaining > oldRowBaseQty)
					{
						if (oldRowBaseQty == 0m && currentBaseQtyRemaining != 0m && !HasDetail(e.Row))
						{
							AMProdMatlSplit split = (AMProdMatl)e.Row;
							// Let UpdateSplits set qty values
							split.Qty = 0m;
							split.BaseQty = 0m;
							SplitCache.Insert(split);
						}
						UpdateSplits(e.Row, currentBaseQtyRemaining - oldRowBaseQty);
						UpdateParent(e.Row);
					}
					if (currentBaseQtyRemaining < (e.OldRow?.BaseQtyRemaining ?? 0m))
					{
						TruncateSplits(e.Row, e.OldRow.BaseQtyRemaining.Value - currentBaseQtyRemaining);
						UpdateParent(e.Row);
					}
				}

				Availability.Check(e.Row, CostCenter.FreeStock);
			}
		}
		#endregion
		#region AMProdMatlSplit
		protected override void SubscribeForSplitEvents()
		{
			base.SubscribeForSplitEvents();
			ManualEvent.FieldOf<AMProdMatlSplit, AMProdMatlSplit.invtMult>.Defaulting.Subscribe<short?>(Base, EventHandler);
			ManualEvent.FieldOf<AMProdMatlSplit, AMProdMatlSplit.subItemID>.Defaulting.Subscribe<int?>(Base, EventHandler);
			ManualEvent.FieldOf<AMProdMatlSplit, AMProdMatlSplit.locationID>.Defaulting.Subscribe<int?>(Base, EventHandler);
			ManualEvent.Row<AMProdMatlSplit>.Selected.Subscribe(Base, EventHandler);
		}

		protected virtual void EventHandler(ManualEvent.Row<AMProdMatlSplit>.Selected.Args e)
		{
			if (e.Row == null)
				return;

			PXUIFieldAttribute.SetEnabled<AMProdMatlSplit.subItemID>(e.Cache, e.Row, false);
			PXUIFieldAttribute.SetEnabled<AMProdMatlSplit.siteID>(e.Cache, e.Row, false);
			PXUIFieldAttribute.SetEnabled<AMProdMatlSplit.qty>(e.Cache, e.Row, e.Row.IsAllocated == true);
			PXUIFieldAttribute.SetVisible<AMProdMatlSplit.lotSerialNbr>(e.Cache, e.Row, e.Row.IsAllocated == true);

			if (e.Row.Completed == true)
				PXUIFieldAttribute.SetEnabled(e.Cache, e.Row, false);
		}

		protected virtual void EventHandler(ManualEvent.FieldOf<AMProdMatlSplit, AMProdMatlSplit.invtMult>.Defaulting.Args<short?> e)
		{
			if (LineCurrent != null && (e.Row == null || e.Row.ProdOrdID == LineCurrent.ProdOrdID))
			{
				using (new InvtMultScope(LineCurrent))
				{
					e.NewValue = LineCurrent.InvtMult;
					e.Cancel = true;
				}
			}
		}

		protected virtual void EventHandler(ManualEvent.FieldOf<AMProdMatlSplit, AMProdMatlSplit.subItemID>.Defaulting.Args<int?> e)
		{
			if (LineCurrent != null && (e.Row == null || e.Row.ProdOrdID == LineCurrent.ProdOrdID))
			{
				e.NewValue = LineCurrent.SubItemID;
				e.Cancel = true;
			}
		}

		protected virtual void EventHandler(ManualEvent.FieldOf<AMProdMatlSplit, AMProdMatlSplit.locationID>.Defaulting.Args<int?> e)
		{
			if (LineCurrent != null && (e.Row == null || e.Row.ProdOrdID == LineCurrent.ProdOrdID))
			{
				e.NewValue = LineCurrent.LocationID;
				e.Cancel = true;
			}
		}

		protected override void EventHandler(ManualEvent.Row<AMProdMatlSplit>.Inserting.Args e)
		{
			if (e.Row != null && SuppressedMode && CurrentOperation == PXDBOperation.Update)
			{
				foreach (var siblingSplit in SelectSplits(e.Row))
				{
					if (AreSplitsEqual(e.Row, siblingSplit))
					{
						var oldSiblingSplit = PXCache<AMProdMatlSplit>.CreateCopy(siblingSplit);
#if DEBUG
						// PX1048 - Only the DAC instance that is passed in the event arguments can be modified in the RowInserting
						// Ignoring as this is just a copy and paste from the base class to get serial tracked items to also merge as an update (PX1048 exists in base code)
#endif
#pragma warning disable PX1048
						siblingSplit.BaseQty += e.Row.BaseQty.GetValueOrDefault();
						siblingSplit.Qty = INUnitAttribute.ConvertFromBase(e.Cache, siblingSplit.InventoryID, siblingSplit.UOM, siblingSplit.BaseQty.GetValueOrDefault(), INPrecision.QUANTITY);
#pragma warning restore PX1048
						e.Cache.Current = siblingSplit;
						e.Cache.RaiseRowUpdated(siblingSplit, oldSiblingSplit);
						e.Cache.MarkUpdated(siblingSplit);
						e.Cancel = true;
						break;
					}
				}
			}

			if (!e.Cancel)
				base.EventHandler(e);
		}

		protected override void EventHandler(ManualEvent.Row<AMProdMatlSplit>.Inserted.Args e)
		{
			base.EventHandler(e);

			if (SuppressedMode == false || !string.IsNullOrEmpty(e.Row.LotSerialNbr) && e.Row.IsAllocated != true)
			{
				if (e.Row.IsAllocated == true || !string.IsNullOrEmpty(e.Row.LotSerialNbr) && e.Row.IsAllocated != true)
				{
					AllocatedUpdated(e.Row, e.ExternalCall);

					e.Cache.RaiseExceptionHandling<AMProdMatlSplit.qty>(e.Row, null, null);
					Availability.Check(e.Row, CostCenter.FreeStock);
				}
			}

			if (LineCurrent != null && e.Row != null && LineCurrent.ProdCreate == true)
			{
				e.Row.ProdCreate = true;
				SplitCache.Update(e.Row);
			}
		}

		protected override void EventHandler(ManualEvent.Row<AMProdMatlSplit>.Updated.Args e)
		{
			base.EventHandler(e);

			if (SuppressedMode == false)
			{
				if (e.Row.IsAllocated != e.OldRow.IsAllocated)
				{
					if (e.Row.IsAllocated == true)
					{
						AllocatedUpdated(e.Row, e.ExternalCall);

						e.Cache.RaiseExceptionHandling<AMProdMatlSplit.qty>(e.Row, null, null);
						Availability.Check(e.Row, CostCenter.FreeStock);
					}
					else
					{
						foreach (AMProdMatlSplit siblingSplit in SelectSplitsReversed(e.Row))
						{
							if (siblingSplit.SplitLineNbr != e.Row.SplitLineNbr && SchedulesEqual(siblingSplit, e.Row))
							{
								var baseQty = (siblingSplit.BaseQty - siblingSplit.BaseQtyReceived).NotLessZero();
								e.Row.Qty += (siblingSplit.Qty - siblingSplit.QtyReceived).NotLessZero();
								e.Row.BaseQty += baseQty;
								e.Row.RefNoteID = null;
								e.Row.POReceiptNbr = null;
								e.Row.POReceiptType = null;
								e.Row.POOrderType = null;
								e.Row.POOrderNbr = null;
								e.Row.POLineNbr = null;
								e.Row.VendorID = null;

								if (!string.IsNullOrWhiteSpace(e.Row.LotSerialNbr))
								{
									AMProdMatlSplit copy = PXCache<AMProdMatlSplit>.CreateCopy(e.Row);
									e.Row.LotSerialNbr = null;

									e.Cache.RaiseRowUpdated(e.Row, copy);
								}
								e.Cache.SetStatus(siblingSplit, e.Cache.GetStatus(siblingSplit) == PXEntryStatus.Inserted ? PXEntryStatus.InsertedDeleted : PXEntryStatus.Deleted);
								e.Cache.ClearQueryCache();

								PXCache cache = Base.Caches<INItemPlan>();
								INItemPlan newPlan = SelectFrom<INItemPlan>.Where<INItemPlan.planID.IsEqual<@P.AsLong>>.View.Select(Base, e.Row.PlanID);
								if (newPlan != null)
								{
									newPlan.PlanQty += baseQty;
									if (cache.GetStatus(newPlan) != PXEntryStatus.Inserted)
										cache.SetStatus(newPlan, PXEntryStatus.Updated);
								}

								INItemPlan oldPlan = SelectFrom<INItemPlan>.Where<INItemPlan.planID.IsEqual<@P.AsLong>>.View.Select(Base, siblingSplit.PlanID);
								if (oldPlan != null)
								{
									cache.SetStatus(oldPlan, cache.GetStatus(oldPlan) == PXEntryStatus.Inserted ? PXEntryStatus.InsertedDeleted : PXEntryStatus.Deleted);
									cache.ClearQueryCache();

								}

								RefreshViewOf(e.Cache);
							}
							else if (siblingSplit.SplitLineNbr == e.Row.SplitLineNbr && !string.IsNullOrWhiteSpace(e.Row.LotSerialNbr) && SchedulesEqual(siblingSplit, e.Row))
							{
								AMProdMatlSplit copy = PXCache<AMProdMatlSplit>.CreateCopy(e.Row);
								e.Row.LotSerialNbr = null;
								e.Cache.RaiseRowUpdated(e.Row, copy);
							}
						}
					}
				}

				if (!string.Equals(e.Row.LotSerialNbr, e.OldRow.LotSerialNbr, StringComparison.OrdinalIgnoreCase))
				{
					if (e.Row.LotSerialNbr != null)
					{
						LotSerialNbrUpdated(e.Row, e.ExternalCall);

						e.Cache.RaiseExceptionHandling<AMProdMatlSplit.qty>(e.Row, null, null);
						Availability.Check(e.Row, CostCenter.FreeStock);
					}
					else
					{
						foreach (AMProdMatlSplit siblingSplit in SelectSplitsReversed(e.Row))
						{
							if (siblingSplit.SplitLineNbr == e.Row.SplitLineNbr)
							{
								AMProdMatlSplit copy = PXCache<AMProdMatlSplit>.CreateCopy(siblingSplit);
								e.Row.IsAllocated = false;
								e.Cache.RaiseFieldUpdated<AMProdMatlSplit.isAllocated>(e.Row, e.Row.IsAllocated);
								e.Cache.RaiseRowUpdated(siblingSplit, copy);
							}
						}
					}
				}
			}
		}

		public override void EventHandlerQty(ManualEvent.FieldOf<AMProdMatlSplit>.Verifying.Args<decimal?> e)
		{
			e.NewValue = VerifySNQuantity(e.Cache, e.Row, e.NewValue, nameof(AMProdMatlSplit.qty));
		}

		public override void EventHandlerUOM(ManualEvent.FieldOf<AMProdMatlSplit>.Defaulting.Args<string> e)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(e.Row.InventoryID);

			if (item != null && ((INLotSerClass)item).LotSerTrack == INLotSerTrack.SerialNumbered)
			{
				e.NewValue = ((InventoryItem)item).BaseUnit;
				e.Cancel = true;
			}
			else
			{
				base.EventHandlerUOM(e);
			}
		}


		protected virtual bool AllocatedUpdated(AMProdMatlSplit split, bool externalCall)
		{
			if (split == null)
				return false;

			var accum = new SiteStatusByCostCenter
			{
				InventoryID = split.InventoryID,
				SiteID = split.SiteID,
				SubItemID = split.SubItemID,
				CostCenterID = CostCenter.FreeStock,
			};
			accum = PXCache<SiteStatusByCostCenter>.Insert(Base, accum);
			accum = PXCache<SiteStatusByCostCenter>.CreateCopy(accum);

			var stat = INSiteStatusByCostCenter.PK.Find(Base, split.InventoryID, split.SubItemID, split.SiteID, CostCenter.FreeStock);
			if (stat != null)
			{
				accum.QtyAvail += stat.QtyAvail;
				accum.QtyHardAvail += stat.QtyHardAvail;
			}

			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(split.InventoryID);
			if (INLotSerialNbrAttribute.IsTrack(item, split.TranType, split.InvtMult))
			{
				if (!string.IsNullOrEmpty(split.LotSerialNbr))
				{
					LotSerialNbrUpdated(split, externalCall);
					return true;
				}
			}
			else
			{
				if (accum.QtyHardAvail < 0m)
				{
					var copy = PXCache<AMProdMatlSplit>.CreateCopy(split);
					if (split.BaseQty + accum.QtyHardAvail > 0m)
					{
						split.BaseQty += accum.QtyHardAvail;
						split.Qty = INUnitAttribute.ConvertFromBase(SplitCache, split.InventoryID, split.UOM, split.BaseQty.Value, INPrecision.QUANTITY);
						SplitCache.RaiseFieldUpdated<AMProdMatlSplit.qty>(split, split.Qty);
					}
					else
					{
						split.IsAllocated = false;
						SplitCache.RaiseExceptionHandling<AMProdMatlSplit.isAllocated>(split, true, new PXSetPropertyException(IN.Messages.Inventory_Negative2));
					}

					SplitCache.RaiseFieldUpdated<AMProdMatlSplit.isAllocated>(split, copy.IsAllocated);
					SplitCache.RaiseRowUpdated(split, copy);

					if (split.IsAllocated == true)
					{
						copy.SplitLineNbr = null;
						copy.PlanID = null;
						copy.IsAllocated = false;
						copy.BaseQty = -accum.QtyHardAvail;
						copy.Qty = INUnitAttribute.ConvertFromBase(LineCache, copy.InventoryID, copy.UOM, copy.BaseQty.GetValueOrDefault(), INPrecision.QUANTITY);
						copy.QtyComplete = null;
						copy.BaseQtyComplete = null;
						copy.QtyReceived = null;
						copy.BaseQtyReceived = null;

						SplitCache.Insert(copy);
					}

					RefreshViewOf(SplitCache);
					return true;
				}
			}
			return false;
		}

		protected virtual bool LotSerialNbrUpdated(AMProdMatlSplit split, bool externalCall)
		{
			var parent = (AMProdMatl)LSParentAttribute.SelectParent(SplitCache, split, typeof(AMProdMatl));

			if (split == null || parent == null)
				return false;

			var accum = new SiteLotSerial
			{
				InventoryID = split.InventoryID,
				SiteID = split.SiteID,
				LotSerialNbr = split.LotSerialNbr
			};
			accum = PXCache<SiteLotSerial>.Insert(Base, accum);
			accum = PXCache<SiteLotSerial>.CreateCopy(accum);

			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(split.InventoryID);

			INSiteLotSerial siteLotSerial =
				SelectFrom<INSiteLotSerial>.
				Where<
					INSiteLotSerial.inventoryID.IsEqual<@P.AsInt>.
					And<INSiteLotSerial.siteID.IsEqual<@P.AsInt>>.
					And<INSiteLotSerial.lotSerialNbr.IsEqual<@P.AsString>>>.
				View.ReadOnly.Select(Base, split.InventoryID, split.SiteID, split.LotSerialNbr);

			if (siteLotSerial != null)
			{
				accum.QtyAvail += siteLotSerial.QtyAvail;
				accum.QtyHardAvail += siteLotSerial.QtyHardAvail;
			}

			var isIssue = parent.IsByproduct != true;

			//Serial-numbered items
			if (INLotSerialNbrAttribute.IsTrackSerial(item, split.TranType, split.InvtMult) && split.LotSerialNbr != null)
			{
				var copy = PXCache<AMProdMatlSplit>.CreateCopy(split);
				if (siteLotSerial != null && siteLotSerial.QtyAvail > 0 && siteLotSerial.QtyHardAvail > 0)
				{
					if (isIssue)
					{
						split.BaseQty = 1;
						split.Qty = INUnitAttribute.ConvertFromBase(SplitCache, split.InventoryID, split.UOM, split.BaseQty.Value, INPrecision.QUANTITY);
						split.IsAllocated = true;
					}
					else
					{
						split.IsAllocated = false;
						SplitCache.RaiseExceptionHandling<AMProdMatlSplit.lotSerialNbr>(split, null,
							new PXSetPropertyException(PXMessages.LocalizeFormatNoPrefixNLA(IN.Messages.SerialNumberAlreadyReceived,
								((InventoryItem)item).InventoryCD,
								split.LotSerialNbr)));
					}
				}
				else
				{
					if (isIssue)
					{
						if (externalCall)
						{
							split.IsAllocated = false;
							split.LotSerialNbr = null;
							SplitCache.RaiseExceptionHandling<AMProdMatlSplit.lotSerialNbr>(split, null,
								new PXSetPropertyException(IN.Messages.Inventory_Negative2));
							if (split.IsAllocated == true)
								return false;
						}
					}
					else
					{
						split.BaseQty = 1;
						split.Qty = INUnitAttribute.ConvertFromBase(SplitCache, split.InventoryID, split.UOM, split.BaseQty.Value, INPrecision.QUANTITY);
					}
				}

				SplitCache.RaiseFieldUpdated<AMProdMatlSplit.isAllocated>(split, copy.IsAllocated);
				SplitCache.RaiseRowUpdated(split, copy);

				if (copy.BaseQty - 1 > 0m)
				{
					if (split.IsAllocated == true || (split.IsAllocated != true && !isIssue))
					{
						copy.SplitLineNbr = null;
						copy.PlanID = null;
						copy.IsAllocated = false;
						copy.LotSerialNbr = null;
						copy.BaseQty -= 1;
						copy.Qty = INUnitAttribute.ConvertFromBase(LineCache, copy.InventoryID, copy.UOM, copy.BaseQty.GetValueOrDefault(), INPrecision.QUANTITY);
						SplitCache.Insert(copy);
					}

					RefreshViewOf(SplitCache);
					return true;
				}
			}
			//Lot-numbered items
			else if (INLotSerialNbrAttribute.IsTrack(item, split.TranType, split.InvtMult) && split.LotSerialNbr != null && !INLotSerialNbrAttribute.IsTrackSerial(item, split.TranType, split.InvtMult))
			{
				if (split.BaseQty > 0m)
				{
					//Lot/Serial Nbr. selected on non-allocated line. Trying to allocate line first. Verification of Qty. available for allocation will be performed on the next pass-through
					if (split.IsAllocated == false)
					{
						if (siteLotSerial == null || (((siteLotSerial.QtyOnHand > 0 && accum.QtyHardAvail <= 0m) || siteLotSerial.QtyOnHand <= 0m) && isIssue))
						{
							return NegativeInventoryError(split);
						}

						var copy = PXCache<AMProdMatlSplit>.CreateCopy(split);
						split.IsAllocated = true;
						SplitCache.RaiseFieldUpdated<AMProdMatlSplit.isAllocated>(split, copy.IsAllocated);
						SplitCache.RaiseRowUpdated(split, copy);

						return true;
					}

					//Lot/Serial Nbr. selected on allocated line. Available Qty. verification procedure 
					if (split.IsAllocated == true)
					{
						var copy = PXCache<AMProdMatlSplit>.CreateCopy(split);
						if (siteLotSerial != null && siteLotSerial.QtyOnHand > 0 && accum.QtyHardAvail >= 0m && isIssue)
						{
							split.Qty = INUnitAttribute.ConvertFromBase(SplitCache, split.InventoryID, split.UOM, split.BaseQty.Value, INPrecision.QUANTITY);
						}
						else if (siteLotSerial != null && siteLotSerial.QtyOnHand > 0 && accum.QtyHardAvail < 0m && isIssue)
						{
							split.BaseQty += accum.QtyHardAvail;
							if (split.BaseQty <= 0m)
							{
								if (NegativeInventoryError(split))
									return false;
							}
							split.Qty = INUnitAttribute.ConvertFromBase(SplitCache, split.InventoryID, split.UOM, split.BaseQty.Value, INPrecision.QUANTITY);
						}
						else if (siteLotSerial == null || (siteLotSerial.QtyOnHand <= 0m && isIssue))
						{
							if (NegativeInventoryError(split))
								return false;
						}

						Base.FindImplementation<GraphExtensions.AMProdMatlSplitPlan<TGraph>>().RaiseRowUpdated(split);

						if ((copy.BaseQty - split.BaseQty) > 0m && split.IsAllocated == true)
						{
							using (SuppressedModeScope(true))
							{
								copy.SplitLineNbr = null;
								copy.PlanID = null;
								copy.IsAllocated = false;
								copy.LotSerialNbr = null;
								copy.BaseQty -= split.BaseQty;
								copy.Qty = INUnitAttribute.ConvertFromBase(LineCache, copy.InventoryID, copy.UOM, copy.BaseQty.Value, INPrecision.QUANTITY);
								copy = (AMProdMatlSplit)SplitCache.Insert(copy);
								if (copy.LotSerialNbr != null && copy.IsAllocated != true)
								{
									SplitCache.SetValue<AMProdMatlSplit.lotSerialNbr>(copy, null);
								}
							}
						}

						RefreshViewOf(SplitCache);
						return true;
					}
				}
			}
			return false;
		}

		protected virtual bool NegativeInventoryError(AMProdMatlSplit split)
		{
			split.IsAllocated = false;
			split.LotSerialNbr = null;
			SplitCache.RaiseExceptionHandling<AMProdMatlSplit.lotSerialNbr>(split, null, new PXSetPropertyException(IN.Messages.Inventory_Negative2));
			return split.IsAllocated == true;
		}

		protected virtual bool SchedulesEqual(AMProdMatlSplit a, AMProdMatlSplit b)
		{
			if (a != null && b != null)
			{
				return
					a.InventoryID == b.InventoryID &&
					a.SubItemID == b.SubItemID &&
					a.SiteID == b.SiteID &&
					a.LocationID == b.LocationID &&
					a.IsAllocated == b.IsAllocated &&
					a.Completed == b.Completed &&
					a.POCreate == b.POCreate &&
					a.ProdCreate == b.ProdCreate &&
					a.POCompleted == b.POCompleted &&
					a.RefNoteID == b.RefNoteID &&
					a.POReceiptNbr == b.POReceiptNbr && 
					a.POOrderType == b.POOrderType &&
					a.POOrderNbr == b.POOrderNbr &&
					a.POLineNbr == b.POLineNbr &&
					a.VendorID == b.VendorID;
			}
			else
			{
				return a != null;
			}
		}

		#endregion
		#region LotSerOptions
		protected override void EventHandler(ManualEvent.Row<LotSerOptions>.Selected.Args e)
		{
			PXUIFieldAttribute.SetEnabled<LotSerOptions.startNumVal>(e.Cache, e.Row, false);
			PXUIFieldAttribute.SetEnabled<LotSerOptions.qty>(e.Cache, e.Row, false);
			PXDBDecimalAttribute.SetPrecision(e.Cache, e.Row, nameof(LotSerOptions.Qty), e.Row.IsSerial == true ? 0 : CommonSetupDecPl.Qty);
			generateNumbers.SetEnabled(false);
		}
		#endregion
		#endregion

		#region UpdateParent helper
		public override void UpdateParent(AMProdMatl line)
		{
			if (line != null)
				UpdateParent(line, null, null, out _);
			else
				base.UpdateParent(line);
		}

		public override AMProdMatl UpdateParent(AMProdMatlSplit newSplit, AMProdMatlSplit oldSplit)
		{
			var anySplit = newSplit ?? oldSplit;
			var parent = (AMProdMatl)LSParentAttribute.SelectParent(SplitCache, anySplit, typeof(AMProdMatl));

			if (parent != null)
			{
				if (anySplit != null && SameInventoryItem(anySplit, parent))
				{
					var oldParent = PXCache<AMProdMatl>.CreateCopy(parent);

					UpdateParent(
						parent,
						newSplit != null && newSplit.Completed == false ? newSplit : null,
						oldSplit != null && oldSplit.Completed == false ? oldSplit : null,
						out var baseQty);

					using (new InvtMultScope(parent))
					{
						if (newSplit != null) //IsLotSerialRequired in Check
						{

							if (IsLotSerialItem(newSplit))
								parent.UnassignedQty = SelectSplits(newSplit).Where(s => s.LotSerialNbr == null).Sum(s => s.BaseQty);
							else
								parent.UnassignedQty = 0m;
						}

						parent.BaseQty = baseQty; // + parent.BaseClosedQty;
						parent.Qty = INUnitAttribute.ConvertFromBase(LineCache, parent.InventoryID, parent.UOM, parent.BaseQty.Value, INPrecision.QUANTITY);
					}

					LineCache.MarkUpdated(parent);
					LineCache.RaiseFieldUpdated(LineQtyField.Name, parent, oldParent.Qty);

					if (LineCache.RaiseRowUpdating(oldParent, parent))
						LineCache.RaiseRowUpdated(parent, oldParent);
					else
						PXCache<AMProdMatl>.RestoreCopy(parent, oldParent);
				}

				return parent;
			}
			else
			{
				return base.UpdateParent(newSplit, oldSplit);
			}
		}

		public override void UpdateParent(AMProdMatl line, AMProdMatlSplit newSplit, AMProdMatlSplit oldSplit, out decimal baseQty)
		{
			ResetAvailabilityCounters(line);

			bool counted = LineCounters.ContainsKey(line);

			base.UpdateParent(line, newSplit, oldSplit, out baseQty);

			if (!counted && oldSplit != null && LineCounters.TryGetValue(line, out Counters counters))
				baseQty = counters.BaseQty;
		}

		public static void ResetAvailabilityCounters(AMProdMatl line)
		{
			line.LineQtyAvail = null;
			line.LineQtyHardAvail = null;
		}
		#endregion

		#region Select Helpers
		public AMProdMatlSplit[] GetSplits(AMProdMatl line) => SelectSplits(line);

		protected override AMProdMatlSplit[] SelectSplitsReversed(AMProdMatlSplit split) => SelectSplitsReversed(split, true);
		protected virtual AMProdMatlSplit[] SelectSplitsReversed(AMProdMatlSplit split, bool excludeCompleted = true)
		{
			return SelectSplits(split, excludeCompleted)
				.OrderByDescending(s => s.Completed == true ? 0 : s.IsAllocated == true ? 1 : 2)
				.ThenByDescending(s => s.SplitLineNbr)
				.ToArray();
		}

		protected override AMProdMatlSplit[] SelectSplits(AMProdMatlSplit split) => SelectSplits(split, excludeCompleted: true);
		protected virtual AMProdMatlSplit[] SelectSplits(AMProdMatlSplit split, bool excludeCompleted = true)
		{
			if (Availability.IsOptimizationEnabled)
			{
				return PXParentAttribute
					.SelectSiblings(SplitCache, split, typeof(AMProdOper))
					.Cast<AMProdMatlSplit>()
					.Where(s =>
						SameInventoryItem(s, split) &&
						s.LineID == split.LineID &&
						s.Completed == false)
					.ToArray();
			}

			return base.SelectSplits(split).Where(s => s.Completed == false).ToArray();
		}

		protected override AMProdMatlSplit[] SelectSplits(AMProdMatl line, bool compareInventoryID = true) => base.SelectSplits(line, compareInventoryID)
			.Where(s => s.Completed == false)
			.ToArray();
		#endregion

		protected virtual void UpdateNonAllocatedSplits(AMProdMatl line, params Type[] fieldTypes)
		{
			foreach (var split in SelectSplits(line).Where(s => s.IsAllocated != true))
				UpdateSplit(line, split, fieldTypes);
		}

		protected virtual void UpdateSplit(AMProdMatl line, AMProdMatlSplit split, params Type[] fieldTypes)
		{
			if (fieldTypes == null)
				return;

			var newSplit = PXCache<AMProdMatlSplit>.CreateCopy(split);
			foreach (Type fieldType in fieldTypes)
			{
				var val = LineCache.GetValue(line, fieldType.Name);
				SplitCache.SetValue(newSplit, fieldType.Name, val);
			}
			SplitCache.Update(newSplit);
		}

		protected virtual void UpdateSplits(AMProdMatl line, decimal? baseQtyChange)
		{
			if (baseQtyChange.GetValueOrDefault() == 0 || line?.InventoryID == null)
				return;

			LineCounters.Remove(line);
			(var item, var _) = ReadInventoryItem(line.InventoryID);

			decimal? remainingChangeQty = baseQtyChange;
			bool isNewSplit = true;
			foreach (AMProdMatlSplit split in SelectSplits(line)
				.Where(s => s != null && s.IsAllocated != true && s.BaseQty.GetValueOrDefault() >= 0))
			{
				var newBaseQty = split.BaseQty.GetValueOrDefault() + remainingChangeQty;
				if (newBaseQty < 0m)
				{
					remainingChangeQty = newBaseQty;
					newBaseQty = 0m;
				}
				else
				{
					remainingChangeQty = 0m;
				}

				split.BaseQty = newBaseQty;
				split.Qty = item == null || split.UOM == item.BaseUnit
					? split.BaseQty
					: INUnitAttribute.ConvertFromBase(SplitCache, split.InventoryID, split.UOM, split.BaseQty.GetValueOrDefault(), INPrecision.QUANTITY);

				SplitCache.Update(split);

				isNewSplit = split.IsAllocated.GetValueOrDefault();

				if (remainingChangeQty == 0m)
					break;
			}

			if (isNewSplit && remainingChangeQty.GetValueOrDefault() > 0)
			{
				using (OperationModeScope(alterCurrentOperation: PXDBOperation.Insert))
				{
					AMProdMatlSplit split = line;
					split.BaseQty = baseQtyChange;
					split.Qty = INUnitAttribute.ConvertFromBase(SplitCache, split.InventoryID, split.UOM, split.BaseQty.Value, INPrecision.QUANTITY);
					split.POCreate = line.POCreate;
					split.ProdCreate = line.ProdCreate;
					SplitCache.Insert(split);
				}
			}
		}

		protected virtual void IssueAvailable(AMProdMatl line, decimal? baseQty)
		{
			LineCounters.Remove(line);
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(line.InventoryID);

			foreach (INSiteStatusByCostCenter avail in
				SelectFrom<INSiteStatusByCostCenter>.
				Where<
					INSiteStatusByCostCenter.inventoryID.IsEqual<@P.AsInt>.
					And<INSiteStatusByCostCenter.subItemID.IsEqual<@P.AsInt>>.
					And<INSiteStatusByCostCenter.siteID.IsEqual<@P.AsInt>>.
					And<INSiteStatusByCostCenter.costCenterID.IsEqual<CostCenter.freeStock>>>.
				OrderBy<INLocation.pickPriority.Asc>.
				View.ReadOnly.Select(Base, line.InventoryID, line.SubItemID, line.SiteID))
			{
				AMProdMatlSplit split = (AMProdMatlSplit)line;
				if (item != null && ((INLotSerClass)item).LotSerTrack == INLotSerTrack.SerialNumbered)
					split.UOM = ((InventoryItem)item).BaseUnit;

				split.SplitLineNbr = null;
				split.IsAllocated = true; // Does row require allocation => true;
				split.SiteID = line.SiteID;

				var signs = Availability.GetAvailabilitySigns<SiteStatusByCostCenter>(split);
				if (signs.SignQtyHardAvail.IsMinus)
				{
					var accumavail = new SiteStatusByCostCenter();
					PXCache<INSiteStatusByCostCenter>.RestoreCopy(accumavail, avail);
					accumavail = PXCache<SiteStatusByCostCenter>.Insert(Base, accumavail);

					decimal? availableQty = avail.QtyHardAvail + accumavail.QtyHardAvail;

					if (availableQty <= 0m)
						continue;

					if (availableQty < baseQty)
					{
						split.BaseQty = availableQty;
						split.Qty = INUnitAttribute.ConvertFromBase(LineCache, split.InventoryID, split.UOM, availableQty.Value, INPrecision.QUANTITY);
						SplitCache.Insert(split);

						baseQty -= availableQty;
					}
					else
					{
						split.BaseQty = baseQty;
						split.Qty = INUnitAttribute.ConvertFromBase(LineCache, split.InventoryID, split.UOM, baseQty.Value, INPrecision.QUANTITY);
						SplitCache.Insert(split);

						baseQty = 0m;
						break;
					}
				}
			}

			if (baseQty > 0m && line.InventoryID != null && line.SiteID != null && (line.SubItemID != null || (line.SubItemID == null && line.IsStockItem != true)))
			{
				AMProdMatlSplit split = (AMProdMatlSplit)line;
				if (item != null && ((INLotSerClass)item).LotSerTrack == INLotSerTrack.SerialNumbered)
					split.UOM = ((InventoryItem)item).BaseUnit;

				split.SplitLineNbr = null;
				split.IsAllocated = false;
				split.BaseQty = baseQty;
				split.Qty = INUnitAttribute.ConvertFromBase(LineCache, split.InventoryID, split.UOM, split.BaseQty.Value, INPrecision.QUANTITY);

				SplitCache.Insert(PXCache<AMProdMatlSplit>.CreateCopy(split));
			}
		}

		public virtual void TruncateSplits(AMProdMatl line, decimal baseQty)
		{
			LineCounters.Remove(line);

			foreach (var split in SelectSplitsReversed(line))
			{
				if (baseQty >= split.BaseQty)
				{
					baseQty -= split.BaseQty.Value;
					SplitCache.Delete(split);
				}
				else
				{
					AMProdMatlSplit newSplit = PXCache<AMProdMatlSplit>.CreateCopy(split);
					newSplit.BaseQty -= baseQty;
					newSplit.Qty = INUnitAttribute.ConvertFromBase(SplitCache, newSplit.InventoryID, newSplit.UOM, newSplit.BaseQty.Value, INPrecision.QUANTITY);

					SplitCache.Update(newSplit);
					break;
				}
			}
		}

		// Copy logic from 19.105 LSSOLine / Remove previous set of INLotSerClass which was caching the changed value
		protected override PXResult<InventoryItem, INLotSerClass> ReadInventoryItem(int? inventoryID)
		{
			var item = (InventoryItem)PXSelectorAttribute.Select<AMProdMatl.inventoryID>(LineCache, null, inventoryID);

			if (item == null)
				return null;

			var lsclass = INLotSerClass.PK.Find(Base, item.LotSerClassID);
			return new PXResult<InventoryItem, INLotSerClass>(item, lsclass ?? new INLotSerClass());
		}
		public INLotSerClass GetLotSerClass(ILSMaster line) => ReadInventoryItem(line.InventoryID);

		protected override INLotSerTrack.Mode GetTranTrackMode(ILSMaster row, INLotSerClass lotSerClass) => INLotSerTrack.Mode.None;


		public virtual bool IsLotSerialItem(ILSMaster line)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(line.InventoryID);
			return item != null && INLotSerialNbrAttribute.IsTrack(item, line.TranType, line.InvtMult);
		}

		protected virtual bool HasDetail(AMProdMatl line) => line != null && (SelectSplits(line) ?? Array.Empty<AMProdMatlSplit>()).Length != 0;


		private void RefreshViewOf(PXCache cache)
		{
			foreach (var pair in cache.Graph.Views)
			{
				PXView view = pair.Value;
				if (view.IsReadOnly == false && view.GetItemType() == cache.GetItemType())
					view.RequestRefresh();
			}
		}
	}
}
