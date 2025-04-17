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
using PX.Objects.CS;
using PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated;
using PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated.Abstraction;
using ItemLotSerial = PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated.ItemLotSerial;
using SiteLotSerial = PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated.SiteLotSerial;

namespace PX.Objects.IN.GraphExtensions
{
	public abstract class ItemPlan<TGraph, TRefEntity, TItemPlanSource> : ItemPlanBase<TGraph, TItemPlanSource>, IItemPlanHandler<TItemPlanSource>
		where TGraph : PXGraph
		where TRefEntity : class, IBqlTable, new()
		where TItemPlanSource : class, IItemPlanSource, IBqlTable, new()
	{
		#region ReleaseModeScope
		private class ReleasingScope : IDisposable
		{
			private readonly bool _initReleaseMode;
			private readonly ItemPlan<TGraph, TRefEntity, TItemPlanSource> _itemPlanExt;

			public ReleasingScope(ItemPlan<TGraph, TRefEntity, TItemPlanSource> itemPlanExt)
			{
				_itemPlanExt = itemPlanExt;
				_initReleaseMode = _itemPlanExt.ReleaseMode;
				_itemPlanExt.ReleaseMode = true;
			}

			void IDisposable.Dispose()
			{
				_itemPlanExt.ReleaseMode = _initReleaseMode;
			}
		}

		public IDisposable ReleaseModeScope() => new ReleasingScope(this);
		#endregion

		#region State

		public bool ReleaseMode { get; private set; }
		protected Dictionary<Type, List<PXView>> _viewsToClear;

		#endregion

		#region Initialization

		public override void Initialize()
		{
			base.Initialize();

			if (!Base.Views.Caches.Contains(typeof(INItemPlan)))
			{
				Base.RowInserting.AddHandler<INItemPlan>(PlanRowInserting);
				Base.RowInserted.AddHandler<INItemPlan>(PlanRowInserted);
				Base.RowUpdated.AddHandler<INItemPlan>(PlanRowUpdated);
				Base.RowDeleted.AddHandler<INItemPlan>(PlanRowDeleted);

				Base.CommandPreparing.AddHandler<INItemPlan.planID>(ParameterCommandPreparing);

				if (PXAccess.FeatureInstalled<FeaturesSet.inventory>())
				{
					if (!PXAccess.FeatureInstalled<FeaturesSet.warehouse>())
					{
						Base.FieldDefaulting.AddHandler<INItemPlan.siteID>(FeatureFieldDefaulting);
						Base.FieldDefaulting.AddHandler<LocationStatusByCostCenter.siteID>(FeatureFieldDefaulting);
						Base.FieldDefaulting.AddHandler<LotSerialStatusByCostCenter.siteID>(FeatureFieldDefaulting);
					}

					if (!PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>())
					{
						Base.FieldDefaulting.AddHandler<INItemPlan.locationID>(FeatureFieldDefaulting);
						Base.FieldDefaulting.AddHandler<LocationStatusByCostCenter.locationID>(FeatureFieldDefaulting);
						Base.FieldDefaulting.AddHandler<LotSerialStatusByCostCenter.locationID>(FeatureFieldDefaulting);
					}
				}
			}

			AddStatusDACsToCacheMapping(Base);

			if (!Base.Views.Caches.Contains(typeof(TItemPlanSource)))
				Base.Views.Caches.Add(typeof(TItemPlanSource));
			if (Base.IsImport || Base.UnattendedMode)
			{
				if (!Base.Views.Caches.Contains(typeof(INItemPlan)))
				{
					int index = Base.Views.Caches.IndexOf(typeof(TItemPlanSource));
					Base.Views.Caches.Insert(index, typeof(INItemPlan));
				}
			}
			else
			{
				//plan source should go before plan
				//to bind errors from INItemPlan.SubItemID -> SOLine.SubItemID or SOLineSplit.SubItemID
				if (!Base.Views.Caches.Contains(typeof(INItemPlan)))
					Base.Views.Caches.Add(typeof(INItemPlan));
			}

			if (!Base.Views.Caches.Contains(typeof(LotSerialStatusByCostCenter)))
				Base.Views.Caches.Add(typeof(LotSerialStatusByCostCenter));
			if (!Base.Views.Caches.Contains(typeof(ItemLotSerial)))
				Base.Views.Caches.Add(typeof(ItemLotSerial));
			if (!Base.Views.Caches.Contains(typeof(SiteLotSerial)))
				Base.Views.Caches.Add(typeof(SiteLotSerial));
			if (!Base.Views.Caches.Contains(typeof(LocationStatusByCostCenter)))
				Base.Views.Caches.Add(typeof(LocationStatusByCostCenter));
			if (!Base.Views.Caches.Contains(typeof(SiteStatusByCostCenter)))
				Base.Views.Caches.Add(typeof(SiteStatusByCostCenter));

			Base.FieldVerifying.AddHandler<SiteStatusByCostCenter.subItemID>(SurrogateIDFieldVerifying);
			Base.FieldVerifying.AddHandler<LocationStatusByCostCenter.subItemID>(SurrogateIDFieldVerifying);
			Base.FieldVerifying.AddHandler<LotSerialStatusByCostCenter.subItemID>(SurrogateIDFieldVerifying);
			Base.FieldVerifying.AddHandler<LocationStatusByCostCenter.locationID>(SurrogateIDFieldVerifying);
			Base.FieldVerifying.AddHandler<LotSerialStatusByCostCenter.locationID>(SurrogateIDFieldVerifying);

			Base.RowPersisted.AddHandler<SiteStatusByCostCenter>(AccumulatorRowPersisted);
			Base.RowPersisted.AddHandler<LocationStatusByCostCenter>(AccumulatorRowPersisted);
			Base.RowPersisted.AddHandler<LotSerialStatusByCostCenter>(AccumulatorRowPersisted);
			Base.RowPersisted.AddHandler<ItemLotSerial>(AccumulatorRowPersisted);
			Base.RowPersisted.AddHandler<SiteLotSerial>(AccumulatorRowPersisted);

			Base.CommandPreparing.AddHandler(typeof(TItemPlanSource), nameof(IItemPlanSource.PlanID), ParameterCommandPreparing);
		}

		#endregion

		#region Event Handlers

		#region INItemPlan

		protected virtual void PlanRowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			if (e.Row != null && ((INItemPlan)e.Row).InventoryID == null)
			{
				e.Cancel = true;
			}
		}

		protected virtual void PlanRowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			UpdateAllocatedQuantitiesWithPlan((INItemPlan)e.Row, revert: false);
		}

		protected virtual void PlanRowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			UpdateAllocatedQuantitiesWithPlan((INItemPlan)e.Row, revert: true);
		}

		protected virtual void PlanRowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			UpdateAllocatedQuantitiesWithPlan((INItemPlan)e.Row, revert: false);
			UpdateAllocatedQuantitiesWithPlan((INItemPlan)e.OldRow, revert: true);
		}

		#endregion

		#region TItemPlanSource

		public virtual void _(Events.RowInserted<TItemPlanSource> e)
		{
			if (ReleaseMode) return;

			INItemPlan info = FetchPlan(e.Row);

			if (info == null)
			{
				info = DefaultValuesInt(new INItemPlan(), e.Row);
				if (info == null)
				{
					return;
				}

				info = (INItemPlan)PlanCache.Insert(info);
				SetPlanID(e.Row, info.PlanID);
			}
			else
			{
				INItemPlan oldInfo = PXCache<INItemPlan>.CreateCopy(info);
				info = DefaultValuesInt(info, e.Row);
				if (info != null)
				{
					if (!PlanCache.ObjectsEqual(info, oldInfo))
					{
						info.PlanID = null;
						PlanCache.Delete(oldInfo);
						info = (INItemPlan)PlanCache.Insert(info);
						SetPlanID(e.Row, info.PlanID);
					}
					else
					{
						info = (INItemPlan)PlanCache.Update(info);
					}
				}
			}
		}

		public virtual void _(Events.RowUpdated<TItemPlanSource> e) => RowUpdatedImpl(e.Row);

		public virtual void _(Events.RowDeleted<TItemPlanSource> e)
		{
			if (ReleaseMode) return;

			INItemPlan info = FetchPlan(e.Row);

			if (info != null)
			{
				PlanCache.Delete(info);
			}
		}

		#endregion

		#region TRefEntity

		public virtual void _(Events.RowInserted<TRefEntity> e)
		{
			PXNoteAttribute.GetNoteID(e.Cache, e.Row, nameof(Note.noteID));
			Base.Caches<Note>().IsDirty = false;
		}

		public virtual void _(Events.RowUpdated<TRefEntity> e)
		{
			PXNoteAttribute.GetNoteID(e.Cache, e.Row, nameof(Note.noteID));
		}

		#endregion

		#region Common

		protected virtual void ParameterCommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && (e.Operation & PXDBOperation.Option) != PXDBOperation.External &&
				(e.Operation & PXDBOperation.Option) != PXDBOperation.ReadOnly && e.Row == null && (e.Value is long key))
			{
				if (key < 0L)
				{
					e.DataValue = null;
					e.Cancel = true;
				}
			}
		}

		protected virtual void AccumulatorRowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.Operation != PXDBOperation.Delete && e.TranStatus == PXTranStatus.Completed)
			{
				Type dacType = sender.GetItemType();
				if (dacType == typeof(ItemLotSerial))
				{
					Clear<ItemLotSerial>();
				}
				else if (dacType == typeof(SiteLotSerial))
				{
					Clear<SiteLotSerial>();
				}
				else if (dacType == typeof(SiteStatusByCostCenter))
				{
					Clear<SiteStatusByCostCenter>();
				}
				else if (dacType == typeof(LocationStatusByCostCenter))
				{
					Clear<LocationStatusByCostCenter>();
				}
				else if (dacType == typeof(LotSerialStatusByCostCenter))
				{
					Clear<LotSerialStatusByCostCenter>();
				}
			}
		}

		protected virtual void FeatureFieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = null;
			e.Cancel = true;
		}

		protected virtual void SurrogateIDFieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.NewValue is Int32)
			{
				e.Cancel = true;
			}
		}

		#endregion

		#endregion

		#region RowUpdatedImpl

		public void RaiseRowUpdated(TItemPlanSource row) => RowUpdatedImpl(row);

		private void RowUpdatedImpl(TItemPlanSource row)
		{
			if (ReleaseMode) return;

			INItemPlan info = FetchPlan(row);

			if (info == null)
			{
				info = DefaultValuesInt(new INItemPlan(), row);
				if (info == null)
				{
					return;
				}

				info = (INItemPlan)PlanCache.Insert(info);
				SetPlanID(row, info.PlanID);
			}
			else
			{
				INItemPlan oldInfo = PXCache<INItemPlan>.CreateCopy(info);
				info = DefaultValuesInt(info, row);
				if (info != null)
				{
					if (IsPlanInfoChanged(info, oldInfo))
					{
						info.PlanID = null;
						PlanCache.Delete(oldInfo);
						info = (INItemPlan)PlanCache.Insert(info);
						SetPlanID(row, info.PlanID);

						foreach (INItemPlan demandInfo in PXSelect<INItemPlan,
							Where<INItemPlan.supplyPlanID, Equal<Required<INItemPlan.supplyPlanID>>>>
							.Select(Base, oldInfo.PlanID))
						{
							demandInfo.SupplyPlanID = info.PlanID;
							PlanCache.MarkUpdated(demandInfo, assertError: true);
						}
					}
					else
					{
						info = (INItemPlan)PlanCache.Update(info);
					}
				}
				else
				{
					PlanCache.Delete(oldInfo);
					ClearPlanID(row);
				}
			}
		}

		#endregion

		#region DefaultValues

		public abstract INItemPlan DefaultValues(INItemPlan planRow, TItemPlanSource origRow);

		protected INItemPlan DefaultValuesInt(INItemPlan planRow, TItemPlanSource origRow)
		{
			INItemPlan info = DefaultValues(planRow, origRow);
			if (info != null && info.InventoryID != null && info.SiteID != null)
			{
				info.RefEntityType = GetRefEntityType();
				return info;
			}
			return null;
		}

		protected virtual string GetRefEntityType()
		{
			return typeof(TRefEntity).FullName;
		}

		#endregion

		#region UpdateAllocatedQuantities

		protected virtual bool CanUpdateAllocatedQuantitiesWithPlan(INItemPlan plan)
		{
			return (plan.InventoryID != null &&
					plan.SubItemID != null &&
					plan.SiteID != null &&
					InventoryItem.PK.Find(Base, plan.InventoryID) is InventoryItem stkitem &&
					stkitem.StkItem == true);
		}

		protected virtual void UpdateAllocatedQuantitiesWithPlan(INItemPlan plan, bool revert)
		{
			INPlanType plantype = INPlanType.PK.Find(Base, plan.PlanType);
			plantype = revert ? -plantype : plantype;

			if (CanUpdateAllocatedQuantitiesWithPlan(plan))
			{
				if (plan.LocationID != null)
				{
					LocationStatusByCostCenter itemByCostCenter = UpdateAllocatedQuantities<LocationStatusByCostCenter>(plan, plantype, true);
					UpdateAllocatedQuantities<SiteStatusByCostCenter>(plan, plantype, (bool)itemByCostCenter.InclQtyAvail);
					if (!string.IsNullOrEmpty(plan.LotSerialNbr))
					{
						UpdateAllocatedQuantities<LotSerialStatusByCostCenter>(plan, plantype, true);
						UpdateAllocatedQuantities<ItemLotSerial>(plan, plantype, true);
						UpdateAllocatedQuantities<SiteLotSerial>(plan, plantype, (bool)itemByCostCenter.InclQtyAvail);
					}
				}
				else
				{
					UpdateAllocatedQuantities<SiteStatusByCostCenter>(plan, plantype, true);
					if (!string.IsNullOrEmpty(plan.LotSerialNbr))
					{
						//TODO: check if LotSerialNbr was allocated on OrigPlanType
						UpdateAllocatedQuantities<ItemLotSerial>(plan, plantype, true);
						UpdateAllocatedQuantities<SiteLotSerial>(plan, plantype, true);
					}
				}
			}
		}

		protected TNode UpdateAllocatedQuantities<TNode>(INItemPlan plan, INPlanType plantype, bool inclQtyAvail)
			where TNode : class, IQtyAllocatedBase
		{
			INPlanType targettype = GetTargetPlanType<TNode>(plan, plantype);
			return UpdateAllocatedQuantitiesBase<TNode>(plan, targettype, inclQtyAvail);
		}

		#endregion

		#region AvailabilitySigns

		public virtual AvailabilitySigns GetAvailabilitySigns<TNode>(TItemPlanSource data)
			where TNode : class, IQtyAllocatedBase, IBqlTable, new()
		{
			INItemPlan plan = DefaultValuesInt(new INItemPlan() { IsTemporary = true }, data);
			if (plan != null)
			{
				INPlanType plantype = INPlanType.PK.Find(Base, plan.PlanType);
				plantype = GetTargetPlanType<TNode>(plan, plantype);

				return GetAvailabilitySigns<TNode>(plan, plantype);
			}
			return new AvailabilitySigns();
		}

		protected AvailabilitySigns GetAvailabilitySigns<TNode>(INItemPlan plan, INPlanType plantype)
			where TNode : class, IQtyAllocatedBase, IBqlTable, new()
		{
			TNode target = InsertWith<TNode>(ConvertPlan<TNode>(plan),
				(cache, e) =>
				{
					cache.SetStatus(e.Row, PXEntryStatus.Notchanged);
					cache.IsDirty = false;
				});

			decimal signQtyAvail = 0m;

			if (plan.Reverse != true || target.InclQtySOReverse == true || !IsSORelated(plantype))
			{
				signQtyAvail -= target.InclQtyINIssues == true ? (decimal)plantype.InclQtyINIssues : 0m;
				signQtyAvail += target.InclQtyINReceipts == true ? (decimal)plantype.InclQtyINReceipts : 0m;
				signQtyAvail += target.InclQtyInTransit == true ? (decimal)plantype.InclQtyInTransit : 0m;
				signQtyAvail += target.InclQtyPOPrepared == true ? (decimal)plantype.InclQtyPOPrepared : 0m;
				signQtyAvail += target.InclQtyPOOrders == true ? (decimal)plantype.InclQtyPOOrders : 0m;
				signQtyAvail += target.InclQtyPOReceipts == true ? (decimal)plantype.InclQtyPOReceipts : 0m;
				signQtyAvail += target.InclQtyINAssemblySupply == true ? (decimal)plantype.InclQtyINAssemblySupply : 0m;
				signQtyAvail += target.InclQtyProductionSupplyPrepared == true ? (decimal)plantype.InclQtyProductionSupplyPrepared : 0m;
				signQtyAvail += target.InclQtyProductionSupply == true ? (decimal)plantype.InclQtyProductionSupply : 0m;
				signQtyAvail -= target.InclQtySOBackOrdered == true ? (decimal)plantype.InclQtySOBackOrdered : 0m;
				signQtyAvail -= target.InclQtySOPrepared == true ? (decimal)plantype.InclQtySOPrepared : 0m;
				signQtyAvail -= target.InclQtySOBooked == true ? (decimal)plantype.InclQtySOBooked : 0m;
				signQtyAvail -= target.InclQtySOShipped == true ? (decimal)plantype.InclQtySOShipped : 0m;
				signQtyAvail -= target.InclQtySOShipping == true ? (decimal)plantype.InclQtySOShipping : 0m;
				signQtyAvail -= target.InclQtyINAssemblyDemand == true ? (decimal)plantype.InclQtyINAssemblyDemand : 0m;
				signQtyAvail -= target.InclQtyProductionDemandPrepared == true ? (decimal)plantype.InclQtyProductionDemandPrepared : 0m;
				signQtyAvail -= target.InclQtyProductionDemand == true ? (decimal)plantype.InclQtyProductionDemand : 0m;
				signQtyAvail -= target.InclQtyProductionAllocated == true ? (decimal)plantype.InclQtyProductionAllocated : 0m;

				signQtyAvail -= target.InclQtyFSSrvOrdPrepared == true ? (decimal)plantype.InclQtyFSSrvOrdPrepared : 0m;
				signQtyAvail -= target.InclQtyFSSrvOrdBooked == true ? (decimal)plantype.InclQtyFSSrvOrdBooked : 0m;
				signQtyAvail -= target.InclQtyFSSrvOrdAllocated == true ? (decimal)plantype.InclQtyFSSrvOrdAllocated : 0m;

				signQtyAvail += target.InclQtyPOFixedReceipt == true ? (decimal)plantype.InclQtyPOFixedReceipts : 0m;

				if (target.InclQtyFixedSOPO == true)
				{
					signQtyAvail += target.InclQtyPOOrders == true ? (decimal)plantype.InclQtyPOFixedOrders : 0m;
					signQtyAvail += target.InclQtyPOPrepared == true ? (decimal)plantype.InclQtyPOFixedPrepared : 0m;
					signQtyAvail += target.InclQtyPOReceipts == true ? (decimal)plantype.InclQtyPOFixedReceipts : 0m;
					signQtyAvail -= target.InclQtySOBooked == true ? (decimal)plantype.InclQtySOFixed : 0m;
				}

				if (plan.Reverse == true)
				{
					signQtyAvail = -signQtyAvail;
				}
			}

			decimal signQtyHardAvail = 0m;

			if (plan.Reverse != true)
			{
				signQtyHardAvail -= (decimal)plantype.InclQtySOShipped;
				signQtyHardAvail -= (decimal)plantype.InclQtySOShipping;
				signQtyHardAvail -= (decimal)plantype.InclQtyINIssues;
				signQtyHardAvail -= (decimal)plantype.InclQtyProductionAllocated;

				signQtyHardAvail -= (decimal)plantype.InclQtyFSSrvOrdAllocated;
				signQtyHardAvail -= (decimal)plantype.InclQtyINAssemblyDemand;
			}

			decimal signQtyActual = (plan.Reverse != true) ? -(decimal)plantype.InclQtySOShipped : 0m;

			return new AvailabilitySigns(signQtyAvail, signQtyHardAvail, signQtyActual);
		}

		#endregion

		#region Helper methods

		protected virtual bool IsPlanInfoChanged(INItemPlan info, INItemPlan oldInfo)
		{
			return !PlanCache.ObjectsEqual(info, oldInfo) ||
				!string.Equals(info.LotSerialNbr, oldInfo.LotSerialNbr, StringComparison.OrdinalIgnoreCase) &&
				(!string.IsNullOrEmpty(info.LotSerialNbr) || !string.IsNullOrEmpty(oldInfo.LotSerialNbr)) ||
				info.ProjectID != oldInfo.ProjectID ||
				info.TaskID != oldInfo.TaskID;
		}

		protected virtual void SetPlanID(TItemPlanSource row, long? planID)
		{
			row.PlanID = planID;
		}

		protected virtual void ClearPlanID(TItemPlanSource row)
		{
			row.PlanID = null;
		}

		protected virtual INItemPlan FetchPlan(TItemPlanSource origRow)
		{
			INItemPlan info = null;

			if (origRow.PlanID != null)
			{
				if (origRow.PlanID < 0)
				{
					info = new INItemPlan
					{
						PlanID = origRow.PlanID,
						InventoryID = origRow.InventoryID,
						SiteID = origRow.SiteID
					};
					if (info != null)
					{
						info = (INItemPlan)PlanCache.Locate(info);
					}
				}
				if (info == null)
				{
					if (origRow.PlanID < 0)
					{
						foreach (INItemPlan plan in PlanCache.Inserted)
						{
							if (plan.PlanID == origRow.PlanID)
							{
								info = plan;
								break;
							}
						}
					}
					else
					{
						info = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(Base, origRow.PlanID);
					}
				}
				if (info == null)
				{
					origRow.PlanID = null;
				}
				else
				{
					return PXCache<INItemPlan>.CreateCopy(info);
				}
			}

			return info;
		}

		protected void Clear<TNode>()
			where TNode : class, IBqlTable
		{
			if (_viewsToClear == null)
			{
				_viewsToClear = new Dictionary<Type, List<PXView>>();
			}

			if (!_viewsToClear.TryGetValue(typeof(TNode), out List<PXView> views))
			{
				views = _viewsToClear[typeof(TNode)] = new List<PXView>();

				List<PXView> namedviews = new List<PXView>(Base.Views.Values);
				foreach (PXView view in namedviews)
				{
					if (typeof(TNode).IsAssignableFrom(view.GetItemType()))
					{
						views.Add(view);
					}
				}

				List<PXView> typedviews = new List<PXView>(Base.TypedViews.Values);
				foreach (PXView view in typedviews)
				{
					if (typeof(TNode).IsAssignableFrom(view.GetItemType()))
					{
						views.Add(view);
					}
				}

				List<PXView> readonlyviews = new List<PXView>(Base.TypedViews.ReadOnlyValues);
				foreach (PXView view in readonlyviews)
				{
					if (typeof(TNode).IsAssignableFrom(view.GetItemType()))
					{
						views.Add(view);
					}
				}
			}

			foreach (PXView view in views)
			{
				view.Clear();
				view.Cache.Clear();
			}
		}

		protected T InsertWith<T>(T row, PXRowInserted handler)
			where T : class, IBqlTable, new()
		{
			Base.RowInserted.AddHandler<T>(handler);
			try
			{
				return PXCache<T>.Insert(Base, row);
			}
			finally
			{
				Base.RowInserted.RemoveHandler<T>(handler);
			}
		}

		#endregion
	}
}
