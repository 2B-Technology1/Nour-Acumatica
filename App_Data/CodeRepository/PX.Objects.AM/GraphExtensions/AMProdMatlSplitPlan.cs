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

using PX.Data;
using PX.Objects.AM.Attributes;
using PX.Objects.IN;
using PX.Objects.IN.GraphExtensions;

namespace PX.Objects.AM.GraphExtensions
{
	/// <summary>
	/// Handles the Allocation of Material for Production orders
	/// </summary>
	public class AMProdMatlSplitPlan<TGraph> : ItemPlan<TGraph, AMProdMatl, AMProdMatlSplit>
		where TGraph : PXGraph
	{
		bool _initVendor = false;
		bool _resetSupplyPlanID = false;
		public override void _(Events.RowUpdated<AMProdMatlSplit> e)
		{
			var isLinked = IsLineLinked(e.Row);
			_initVendor = !e.Cache.ObjectsEqual<AMProdMatlSplit.siteID, AMProdMatlSplit.subItemID, AMProdMatlSplit.vendorID, AMProdMatlSplit.pOCreate>(e.Row, e.OldRow) &&
						 !isLinked;
			_resetSupplyPlanID = !isLinked;

			try
			{
				base._(e);
			}
			finally
			{
				_initVendor = false;
				_resetSupplyPlanID = false;
			}
		}

		protected virtual bool IsLineLinked(AMProdMatlSplit split)
		{
			return split != null && (split.POOrderNbr != null || split.AMProdOrdID != null || split.IsAllocated.GetValueOrDefault());
		}

		public override INItemPlan DefaultValues(INItemPlan planRow, AMProdMatlSplit splitRow)
		{
			var parent = GetParentProdMatl(splitRow);

			if ((parent?.MaterialType == AMMaterialType.Subcontract &&
				parent?.SubcontractSource == AMSubcontractSource.VendorSupplied) || parent?.StatusID == ProductionOrderStatus.Completed)
			{
				// No plan record for vendor supplied material. This will hide from allocation details and MRP
				return null;
			}

			var parentItem = GetParentProdItem(splitRow);
			var parentOper = GetParentProdOper(splitRow);

			planRow.PlanType = GetMaterialPlanType(parent, splitRow, parentItem);
			planRow.Hold = parentItem?.Hold == true;
			planRow.BAccountID = parentItem?.CustomerID;
			planRow.InventoryID = splitRow.InventoryID;
			planRow.SubItemID = splitRow.SubItemID;
			planRow.SiteID = splitRow.SiteID;
			planRow.LocationID = splitRow.LocationID;
			planRow.LotSerialNbr = splitRow.LotSerialNbr;
			if (parent.ProdCreate == true)
			{
				planRow.FixedSource = INReplenishmentSource.Manufactured;
			}
			else if(parent.POCreate == true)
			{
				planRow.FixedSource = INReplenishmentSource.Purchased;
			}

			if (_resetSupplyPlanID)
			{
				planRow.SupplyPlanID = null;
			}

			planRow.VendorID = splitRow.VendorID;

			if (_initVendor || splitRow.POCreate == true && splitRow.VendorID != null && planRow.VendorLocationID == null)
			{
				planRow.VendorLocationID =
					PX.Objects.PO.POItemCostManager.FetchLocation(
						Base,
						splitRow.VendorID,
						splitRow.InventoryID,
						splitRow.SubItemID,
						splitRow.SiteID);
			}

			planRow.PlanDate = splitRow.TranDate ?? parentOper?.StartDate ?? parentItem?.StartDate;
			planRow.UOM = parent?.UOM;
			planRow.PlanQty = (splitRow.BaseQty - splitRow.BaseQtyReceived).NotLessZero();
			planRow.RefNoteID = parentOper?.NoteID ?? parent?.NoteID;

			return string.IsNullOrEmpty(planRow.PlanType) || planRow.PlanQty.GetValueOrDefault() == 0 ? null : planRow;
		}

		/// <summary>
		/// Determine the correct material <c>INPlanConstants</c> plan type
		/// </summary>
		/// <param name="prodMatl">Production Material Row</param>
		/// <param name="prodMatlSplit">Production Material Allocation Row</param>
		/// <param name="prodItem">Parent Production Item Row</param>
		/// <returns>A <c>INPlanConstants</c> otherwise null if not applicable</returns>
		protected virtual string GetMaterialPlanType(AMProdMatl prodMatl, AMProdMatlSplit prodMatlSplit, AMProdItem prodItem)
		{
			if (prodMatl == null || prodMatlSplit == null || prodItem == null || ProductionStatus.IsClosedOrCanceled(prodItem))
			{
				return null;
			}

			var isMaterialSupply = prodMatl.IsByproduct.GetValueOrDefault() || prodItem.Function == OrderTypeFunction.Disassemble;
			if (!isMaterialSupply && prodMatlSplit.IsAllocated.GetValueOrDefault())
			{
				return INPlanConstants.PlanM7; /* Production Allocated */
			}

			if (prodMatlSplit.POCreate.GetValueOrDefault())
			{
				return INPlanConstants.PlanM9; /* Production to Purchase */
			}

			if (prodMatlSplit.ProdCreate.GetValueOrDefault())
			{
				return INPlanConstants.PlanMA; /* Production to Production */
			}

			if (!prodItem.Hold.GetValueOrDefault() && prodItem.Released == true)
			{
				return isMaterialSupply
					? INPlanConstants.PlanM2 /* Production Supply */
					: INPlanConstants.PlanM6; /* Production Demand */
			}

			// Prepared types as default
			return isMaterialSupply
				? INPlanConstants.PlanM1 /* Production Supply Prepared */
				: INPlanConstants.PlanM5; /* Production Demand Prepared */
		}

		protected virtual AMProdItem GetParentProdItem(AMProdMatlSplit split)
		{
			return (AMProdItem)PXParentAttribute.SelectParent(ItemPlanSourceCache, split, typeof(AMProdItem));
		}

		protected virtual AMProdMatl GetParentProdMatl(AMProdMatlSplit split)
		{
			return (AMProdMatl)PXParentAttribute.SelectParent(ItemPlanSourceCache, split, typeof(AMProdMatl));
		}

		protected virtual AMProdOper GetParentProdOper(AMProdMatlSplit split)
		{
			return (AMProdOper)PXParentAttribute.SelectParent(ItemPlanSourceCache, split, typeof(AMProdOper));
		}

		protected override string GetRefEntityType()
		{
			return typeof(AMProdOper).FullName;
		}
	}
}
