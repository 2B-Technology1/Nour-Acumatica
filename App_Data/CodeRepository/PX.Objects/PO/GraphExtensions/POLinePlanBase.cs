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

namespace PX.Objects.PO.GraphExtensions
{
	public abstract class POLinePlanBase<TGraph, TItemPlanSource> : ItemPlan<TGraph, POOrder, TItemPlanSource>
		where TGraph : PXGraph
		where TItemPlanSource : class, IItemPlanPOSource, IBqlTable, new()
	{
		public override INItemPlan DefaultValues(INItemPlan planRow, TItemPlanSource origRow)
		{
			if (origRow.OrderType.IsNotIn(POOrderType.RegularOrder, POOrderType.DropShip, POOrderType.Blanket)
				|| origRow.InventoryID == null || origRow.SiteID == null
				|| origRow.Cancelled == true || origRow.Completed == true)
			{
				return null;
			}
			POOrder currentOrder = (POOrder)Base.Caches<POOrder>().Current;
			POOrder order =
				currentOrder?.OrderType == origRow.OrderType && currentOrder.OrderNbr == origRow.OrderNbr
				? currentOrder
				: PXParentAttribute.SelectParent<POOrder>(ItemPlanSourceCache, origRow);
			bool isOnHold = IsOrderOnHold(order);

			if (!TryCalcPlanType(ItemPlanSourceCache, origRow, isOnHold, out string newPlanType))
			{
				return null;
			}
			planRow.PlanType = newPlanType;
			planRow.BAccountID = origRow.VendorID;
			planRow.InventoryID = origRow.InventoryID;
			planRow.SubItemID = origRow.SubItemID;
			planRow.SiteID = origRow.SiteID;
			planRow.ProjectID = origRow.ProjectID;
			planRow.TaskID = origRow.TaskID;
			planRow.CostCenterID = origRow.CostCenterID;
			planRow.PlanDate = origRow.PromisedDate;
			planRow.UOM = origRow.UOM;
			planRow.PlanQty = origRow.BaseOpenQty;

			planRow.RefNoteID = order.NoteID;
			planRow.Hold = isOnHold;

			if (string.IsNullOrEmpty(planRow.PlanType))
			{
				return null;
			}
			return planRow;
		}

		protected virtual bool IsOrderOnHold(POOrder order)
		{
			return (order != null) && order.Status.IsNotIn(POOrderStatus.AwaitingLink, POOrderStatus.Open, POOrderStatus.Completed, POOrderStatus.Closed);
		}

		protected virtual bool TryCalcPlanType(PXCache sender, TItemPlanSource line, bool isOnHold, out string newPlanType)
		{
			newPlanType = null;
			if (line.OrderType == POOrderType.Blanket)
				newPlanType = INPlanConstants.Plan7B;
			else
				switch (line.LineType)
				{
					case POLineType.GoodsForManufacturing:
					case POLineType.NonStockForManufacturing:
						newPlanType = isOnHold ? INPlanConstants.PlanM3 : INPlanConstants.PlanM4;
						break;
					case POLineType.GoodsForSalesOrder:
					case POLineType.NonStockForSalesOrder:
						newPlanType = isOnHold ? INPlanConstants.Plan78 : INPlanConstants.Plan76;
						break;
					case POLineType.GoodsForDropShip:
					case POLineType.NonStockForDropShip:
						newPlanType = isOnHold ? INPlanConstants.Plan79 : INPlanConstants.Plan74;
						break;
					case POLineType.GoodsForInventory:
					case POLineType.GoodsForReplenishment:
					case POLineType.NonStock:
						newPlanType = isOnHold ? INPlanConstants.Plan73 : INPlanConstants.Plan70;
						break;
					case POLineType.GoodsForServiceOrder:
					case POLineType.NonStockForServiceOrder:
						newPlanType = isOnHold ? INPlanConstants.PlanF8 : INPlanConstants.PlanF7;
						break;
				}
			return newPlanType != null;
		}

		protected override void SetPlanID(TItemPlanSource row, long? planID)
		{
			base.SetPlanID(row, planID);
			row.ClearPlanID = false;
		}

		protected override void ClearPlanID(TItemPlanSource row)
		{
			if (row.PlanID == null || row.PlanID < 0L)
			{
				base.ClearPlanID(row);
			}
			else
			{
				// we need to postpone clearing of the PlanID field till row persisting
				// to make other pieces of code work
				row.ClearPlanID = true;
			}
		}

		public override void _(Events.RowPersisting<TItemPlanSource> e)
		{
			if (e.Row.ClearPlanID == true)
			{
				base.ClearPlanID(e.Row);
				e.Row.ClearPlanID = null;
			}

			base._(e);
		}
	}
}
