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

namespace PX.Objects.SO.GraphExtensions.SOShipmentEntryExt
{
	public class SOShipLineSplitPlan : SOShipLineSplitPlanBase<SOShipLineSplit>
	{
		protected Dictionary<object[], HashSet<Guid?>> _processingSets;

		public override void Initialize()
		{
			base.Initialize();

			_processingSets = new Dictionary<object[], HashSet<Guid?>>();
		}

		protected override void UpdatePlansOnParentUpdated(SOShipment parent)
		{
			foreach (INItemPlan plan in PXSelect<INItemPlan,
				Where<INItemPlan.refNoteID, Equal<Current<SOShipment.noteID>>>>
				.Select(Base))
			{
				plan.Hold = parent.Hold;
				plan.PlanDate = parent.ShipDate;

				PlanCache.MarkUpdated(plan, assertError: true);
			}
		}

		protected override HashSet<long?> CollectShipmentPlans(string shipmentNbr = null)
		{
			object[] pars = (shipmentNbr == null) ? new object[] { } : new object[] { shipmentNbr };
			return new HashSet<long?>(
				PXSelect<SOShipLineSplit, Where<SOShipLineSplit.shipmentNbr, Equal<Optional<SOShipment.shipmentNbr>>>>
				.Select(Base, pars)
				.Select(s => ((SOShipLineSplit)s).PlanID));
		}

		public override void _(Events.RowPersisting<INItemPlan> e)
		{
			// crutch for mass processing CreateShipment & PXProcessing<Table>.TryPersistPerRow
			if (e.Operation == PXDBOperation.Update)
			{
				PXCache cache = Base.Caches<SOShipment>();

				if (e.Row is INItemPlan row && cache.Current is SOShipment shipment
					&& row.RefNoteID != shipment.NoteID
					&& PXLongOperation.GetCustomInfo(Base.UID, PXProcessing.ProcessingKey, out object[] processingList) != null
					&& processingList != null)
				{
					if (!_processingSets.TryGetValue(processingList, out HashSet<Guid?> processingSet))
					{
						processingSet = processingList.Select(x => ((SOShipment)x).NoteID).ToHashSet();
						_processingSets[processingList] = processingSet;
					}

					if (processingSet.Contains(row.RefNoteID))
						e.Cancel = true;
				}
			}
			base._(e);
		}
	}

	public class SOShipLineSplitPlanUnassigned : SOShipLineSplitPlanBase<Unassigned.SOShipLineSplit>
	{
		protected override HashSet<long?> CollectShipmentPlans(string shipmentNbr = null)
		{
			object[] pars = (shipmentNbr == null) ? new object[] { } : new object[] { shipmentNbr };
			return new HashSet<long?>(
				PXSelect<Unassigned.SOShipLineSplit, Where<Unassigned.SOShipLineSplit.shipmentNbr, Equal<Optional<SOShipment.shipmentNbr>>>>
				.Select(Base, pars)
				.Select(s => ((Unassigned.SOShipLineSplit)s).PlanID));
		}

		protected override void UpdatePlansOnParentUpdated(SOShipment parent)
		{
		}
	}

	public abstract class SOShipLineSplitPlanBase<TItemPlanSource> : ItemPlan<SOShipmentEntry, SOShipment, TItemPlanSource>
		where TItemPlanSource : class, IItemPlanSOShipSource, IBqlTable, new()
	{
		public override void _(Events.RowUpdated<SOShipment> e)
		{
			base._(e);

			if (!e.Cache.ObjectsEqual<SOShipment.shipDate, SOShipment.hold>(e.Row, e.OldRow))
			{
				HashSet<long?> shipmentPlans = CollectShipmentPlans();
				foreach (INItemPlan plan in PlanCache.Inserted)
				{
					if (shipmentPlans.Contains(plan.PlanID))
					{
						plan.Hold = e.Row.Hold;
						plan.PlanDate = e.Row.ShipDate;
					}
				}

				UpdatePlansOnParentUpdated(e.Row);
			}
		}

		protected abstract void UpdatePlansOnParentUpdated(SOShipment parent);

		protected abstract HashSet<long?> CollectShipmentPlans(string shipmentNbr = null);

		public override INItemPlan DefaultValues(INItemPlan planRow, TItemPlanSource splitRow)
		{
			if (splitRow.Released == true || splitRow.IsStockItem == false)
			{
				return null;
			}
			else if (planRow.IsTemporary != true && splitRow.Confirmed == true)
			{
				return planRow;
			}
			SOShipLine parent = planRow.IsTemporary == true ? null : PXParentAttribute.SelectParent<SOShipLine>(ItemPlanSourceCache, splitRow);

			if (planRow.IsTemporary != true && parent == null) return null;

			planRow.BAccountID = parent?.CustomerID;
			planRow.PlanType = splitRow.PlanType;
			planRow.OrigPlanType = splitRow.OrigPlanType;
			planRow.IgnoreOrigPlan = splitRow.IsComponentItem;
			planRow.InventoryID = splitRow.InventoryID;
			planRow.Reverse = (splitRow.Operation == SOOperation.Receipt);
			planRow.SubItemID = splitRow.SubItemID;
			planRow.SiteID = splitRow.SiteID;
			planRow.LocationID = splitRow.LocationID;
			planRow.LotSerialNbr = splitRow.LotSerialNbr;
			planRow.ProjectID = parent?.ProjectID;
			planRow.TaskID = parent?.TaskID;
			planRow.CostCenterID = parent?.CostCenterID;

			planRow.IsTempLotSerial = string.IsNullOrEmpty(splitRow.AssignedNbr) == false &&
				INLotSerialNbrAttribute.StringsEqual(splitRow.AssignedNbr, planRow.LotSerialNbr);

			if (planRow.IsTempLotSerial == true)
			{
				planRow.LotSerialNbr = null;
			}
			planRow.PlanDate = splitRow.ShipDate;
			planRow.OrigUOM = parent?.OrderUOM;
			planRow.UOM = parent?.UOM;
			planRow.PlanQty = splitRow.BaseQty;

			SOShipment header = Base.Document.Current;
			planRow.RefNoteID = header.NoteID;
			planRow.Hold = header.Hold;

			if (string.IsNullOrEmpty(planRow.PlanType))
			{
				return null;
			}

			if (parent != null)
			{
				if (planRow.OrigNoteID == null)
				{
					SOOrder doc = SOOrder.PK.Find(Base, parent.OrigOrderType, parent.OrigOrderNbr);
					planRow.OrigNoteID = doc?.NoteID;
				}

				if (parent.OrigSplitLineNbr != null)
				{
					SOLineSplit split = SOLineSplit.PK.Find(Base, parent.OrigOrderType, parent.OrigOrderNbr, parent.OrigLineNbr, parent.OrigSplitLineNbr);
					planRow.OrigPlanLevel = (!string.IsNullOrEmpty(split.LotSerialNbr) ? INPlanLevel.LotSerial : INPlanLevel.Site);
					planRow.OrigPlanID = split.PlanID;
					planRow.IgnoreOrigPlan |= !string.IsNullOrEmpty(split.LotSerialNbr) &&
						!string.Equals(planRow.LotSerialNbr, split.LotSerialNbr, StringComparison.InvariantCultureIgnoreCase);
				}
			}

			return planRow;
		}
	}
}
