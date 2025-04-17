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
using PX.Data;
using PX.Objects.AM.Attributes;
using PX.Objects.IN;
using PX.Objects.IN.GraphExtensions;

namespace PX.Objects.AM.GraphExtensions
{
	/// <summary>
	/// Handles the Allocation of Production orders - The manufactured item
	/// </summary>
	public class AMProdItemSplitPlan<TGraph> : ItemPlan<TGraph, AMProdItem, AMProdItemSplit>
		where TGraph : PXGraph
	{
		public override INItemPlan DefaultValues(INItemPlan planRow, AMProdItemSplit splitRow)
		{
			if (!string.IsNullOrWhiteSpace(splitRow.LotSerialNbr) && InventoryHelper.IsLotSerialTempAssigned(splitRow))
			{
				// Wait for split persisting to correctly set the defaulting LotSerialNbr
				return null;
			}

			planRow.InventoryID = splitRow.InventoryID;
			planRow.SubItemID = splitRow.SubItemID;
			planRow.SiteID = splitRow.SiteID;
			planRow.LocationID = splitRow.LocationID;
			planRow.LotSerialNbr = splitRow.LotSerialNbr;
			planRow.UOM = splitRow.UOM;
			// Covers late evaluation to QtyRemaining which impacts allocations
			var qtyRemaining = PXFormulaAttribute.Evaluate<AMProdItemSplit.baseQtyRemaining>(ItemPlanSourceCache, splitRow) as decimal?;
			planRow.PlanQty = qtyRemaining;

			if (splitRow.StatusID == ProductionOrderStatus.Closed || splitRow.StatusID == ProductionOrderStatus.Cancel)
			{
				planRow.PlanQty = 0m;
			}

			var parent = GetParentProdItem(splitRow);
			if (parent == null)
			{
				// During insert persist of lot/serial tracked items with preassigned true the AMProdItem will have the ProductionNbr set where the split does not yet have
				// it set so we need to grab in cache the best we can for that correct item
				parent = (AMProdItem)Base.Caches[typeof(AMProdItem)].Current ?? Base.Caches[typeof(AMProdItem)].Cached.RowCast<AMProdItem>()
					.Where(r => r.OrderType == splitRow?.OrderType && r.InventoryID == splitRow?.InventoryID).FirstOrDefault();
			}

			planRow.Hold = parent?.Hold ?? false;
			planRow.PlanDate = parent?.EndDate ?? splitRow?.TranDate;
			if (parent != null)
			{
				planRow.PlanType = GetPlanType(parent, planRow.Hold.GetValueOrDefault());
				planRow.BAccountID = parent.CustomerID;
				planRow.RefNoteID = parent.NoteID;
				planRow.DemandPlanID = parent.DemandPlanID;
			}

			if (planRow.RefNoteID == Guid.Empty)
			{
				planRow.RefNoteID = null;
			}

			return string.IsNullOrEmpty(planRow.PlanType) || planRow.PlanQty.GetValueOrDefault() == 0 ? null : planRow;
		}

		protected virtual string GetPlanType(AMProdItem parent, bool hold)
		{
			// Determine the Plan Type based on Parent Status and the Supply type
			if (parent == null || parent.Canceled == true || parent.Closed == true
				|| parent.Completed == true)
			{
				return null;
			}

			if (parent.Function == OrderTypeFunction.Disassemble)
			{
				return hold || parent.StatusID == ProductionOrderStatus.Planned ? INPlanConstants.PlanM5 : INPlanConstants.PlanM6;
			}

			switch (parent.SupplyType)
			{
				case ProductionSupplyType.Inventory:
					return hold || parent.Released == false ? INPlanConstants.PlanM1 : INPlanConstants.PlanM2;
				case ProductionSupplyType.Production:
					return hold || parent.Released == false ? INPlanConstants.PlanMB : INPlanConstants.PlanMC;
				case ProductionSupplyType.SalesOrder:
					return hold || parent.Released == false ? INPlanConstants.PlanMD : INPlanConstants.PlanME;
			}

			return null;
		}

		protected virtual AMProdItem GetParentProdItem(AMProdItemSplit split)
		{
			return (AMProdItem)PXParentAttribute.SelectParent(ItemPlanSourceCache, split, typeof(AMProdItem));
		}
	}
}
