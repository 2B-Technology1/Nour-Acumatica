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
using PX.Data.BQL.Fluent;
using PX.Objects.IN;
using PX.Objects.IN.GraphExtensions;

namespace PX.Objects.PO.GraphExtensions.POReceiptEntryExt
{
	public class POReceiptLineSplitPlan : POReceiptLineSplitPlanBase<POReceiptLineSplit>
	{
		public override void _(Events.RowUpdated<POReceipt> e)
		{
			base._(e);

			if (!e.Cache.ObjectsEqual<POReceipt.receiptDate, POReceipt.hold>(e.Row, e.OldRow))
			{
				foreach (INItemPlan plan in SelectFrom<INItemPlan>
					.Where<INItemPlan.refNoteID.IsEqual<POReceipt.noteID.FromCurrent>>
					.View.SelectMultiBound(Base, new[] { e.Row }))
				{
					DefaultValuesFromReceipt(plan, e.Row);

					PlanCache.MarkUpdated(plan, assertError: true);
				}
			}
		}

		protected override HashSet<long?> CollectReceiptPlans(POReceipt receipt)
		{
			return new HashSet<long?>(
				SelectFrom<POReceiptLineSplit>
					.Where<POReceiptLineSplit.FK.Receipt.SameAsCurrent>
				.View.SelectMultiBound(Base, new[] { receipt })
				.AsEnumerable()
				.Select(s => ((POReceiptLineSplit)s).PlanID));
		}
	}

	public class POReceiptLineSplitPlanUnassigned : POReceiptLineSplitPlanBase<Unassigned.POReceiptLineSplit>
	{
		protected override HashSet<long?> CollectReceiptPlans(POReceipt receipt)
		{
			return new HashSet<long?>(
				SelectFrom<Unassigned.POReceiptLineSplit>
					.Where<Unassigned.POReceiptLineSplit.FK.Receipt.SameAsCurrent>
				.View.SelectMultiBound(Base, new[] { receipt })
				.AsEnumerable()
				.Select(s => ((Unassigned.POReceiptLineSplit)s).PlanID));
		}
	}

	public abstract class POReceiptLineSplitPlanBase<TItemPlanSource> : ItemPlan<POReceiptEntry, POReceipt, TItemPlanSource>
		where TItemPlanSource : class, IItemPlanPOReceiptSource, IBqlTable, new()
	{
		public override void _(Events.RowUpdated<POReceipt> e)
		{
			base._(e);

			if (!e.Cache.ObjectsEqual<POReceipt.receiptDate, POReceipt.hold>(e.Row, e.OldRow))
			{
				if (PlanCache.Inserted.Any_())
				{
					HashSet<long?> receiptPlans = CollectReceiptPlans(e.Row);

					foreach (INItemPlan plan in PlanCache.Inserted)
					{
						if (receiptPlans.Contains(plan.PlanID))
						{
							DefaultValuesFromReceipt(plan, e.Row);
						}
					}
				}
			}
		}

		protected abstract HashSet<long?> CollectReceiptPlans(POReceipt receipt);

		protected virtual void DefaultValuesFromReceipt(INItemPlan plan, POReceipt receipt)
		{
			plan.Hold = receipt.Hold;
			plan.PlanDate = receipt.ReceiptDate;
			plan.RefNoteID = receipt.NoteID;
		}

		public override INItemPlan DefaultValues(INItemPlan planRow, TItemPlanSource splitRow)
		{
			//split is using as a container of assigned and unassigned split which provides properties for plan
			POReceiptLine line = null;

			if (planRow.IsTemporary != true)
			{
				line = PXParentAttribute.SelectParent<POReceiptLine>(ItemPlanSourceCache, splitRow);
				if (line?.Released == true)
					return null;
			}

			planRow.BAccountID = line?.VendorID;
			if (splitRow.PONbr != null)
			{
				planRow.OrigPlanLevel = INPlanLevel.Site;

				if (line != null && planRow.OrigPlanID == null)
				{
					POLine poLine = POLine.PK.Find(Base, line.POType, line.PONbr, line.POLineNbr);
					planRow.OrigPlanID = poLine.PlanID;
				}
			}

			switch (splitRow.LineType)
			{
				case POLineType.GoodsForInventory:
				case POLineType.GoodsForReplenishment:
				case POLineType.GoodsForManufacturing:
					if (line != null && line.OrigTranType == INTranType.Transfer)
					{
						if (planRow.OrigNoteID == null)
							planRow.OrigNoteID = line.OrigNoteID;

						planRow.OrigPlanLevel =
							(line.OrigToLocationID != null ? INPlanLevel.Location : INPlanLevel.Site)
							| (line.OrigIsLotSerial == true ? INPlanLevel.LotSerial : INPlanLevel.Site);

						planRow.PlanType = line.OrigIsFixedInTransit == true ? INPlanConstants.Plan45 : INPlanConstants.Plan43;
					}
					else
					{
						planRow.PlanType = INPlanConstants.Plan71;
						planRow.Reverse = splitRow.ReceiptType == POReceiptType.POReturn;
					}
					break;
				case POLineType.GoodsForSalesOrder:
					if (splitRow.ReceiptType == POReceiptType.POReceipt)
					{
						planRow.PlanType = INPlanConstants.Plan77;
					}
					else
					{
						throw new PXException();
					}
					break;
				case POLineType.GoodsForServiceOrder:
					if (splitRow.ReceiptType == POReceiptType.POReceipt)
					{
						planRow.PlanType = INPlanConstants.PlanF9;
					}
					else
					{
						throw new PXException();
					}
					break;
				case POLineType.GoodsForDropShip:
					if (splitRow.ReceiptType == POReceiptType.POReceipt)
					{
						planRow.PlanType = INPlanConstants.Plan75;
					}
					else if (splitRow.ReceiptType == POReceiptType.POReturn)
					{
						planRow.PlanType = null;
					}
					else
					{
						throw new PXException();
					}
					break;
				default:
					return null;
			}

			planRow.OrigPlanType = splitRow.OrigPlanType;
			planRow.InventoryID = splitRow.InventoryID;
			planRow.SubItemID = splitRow.SubItemID;
			planRow.SiteID = splitRow.SiteID;
			planRow.LocationID = splitRow.LocationID;
			planRow.LotSerialNbr = splitRow.LotSerialNbr;
			planRow.IsTempLotSerial =
				string.IsNullOrEmpty(splitRow.AssignedNbr) == false
				&& INLotSerialNbrAttribute.StringsEqual(splitRow.AssignedNbr, splitRow.LotSerialNbr);
			planRow.ProjectID = line?.ProjectID;
			planRow.TaskID = line?.TaskID;
			planRow.CostCenterID = (planRow.IsTemporary != true) ? line?.CostCenterID : CostCenter.FreeStock;

			if (planRow.IsTempLotSerial == true)
			{
				planRow.LotSerialNbr = null;
			}
			planRow.UOM = line?.UOM;
			planRow.PlanQty = splitRow.BaseQty;

			if (string.IsNullOrEmpty(planRow.PlanType))
			{
				return null;
			}

			DefaultValuesFromReceipt(planRow, Base.Document.Current);

			return planRow;
		}
	}
}
