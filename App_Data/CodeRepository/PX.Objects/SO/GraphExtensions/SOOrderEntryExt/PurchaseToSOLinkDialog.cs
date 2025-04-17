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

using PX.Common;
using PX.Common.Collection;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.IN;
using PX.Objects.PO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.SO.GraphExtensions.SOOrderEntryExt
{
	public class PurchaseToSOLinkDialog : PXGraphExtension<POLinkDialog, PurchaseSupplyBaseExt, SOOrderEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.sOToPOLink>()
				|| PXAccess.FeatureInstalled<CS.FeaturesSet.dropShipments>()
				|| PXAccess.FeatureInstalled<CS.FeaturesSet.purchaseRequisitions>();
		}

		public virtual void _(Events.FieldUpdated<SupplyPOLine.selected> e)
		{
			SupplyPOLine updatedSupplyLine = (SupplyPOLine)e.Row;
			SOLine currentSOLine = (SOLine)Base2.SOLineDemand.Select() ?? Base.Transactions.Current;
			if (updatedSupplyLine == null || (bool?)e.OldValue == updatedSupplyLine.Selected
				|| !Base1.IsPoToSoOrBlanket(currentSOLine?.POSource))
				return;

			if (updatedSupplyLine.Selected == true)
				updatedSupplyLine.SelectedSOLines = updatedSupplyLine.SelectedSOLines.SparseArrayAddDistinct(currentSOLine.LineNbr.Value);
			else
				updatedSupplyLine.SelectedSOLines.SparseArrayRemove(currentSOLine.LineNbr.Value);
		}

		public virtual void _(Events.RowSelected<SupplyPOLine> e)
		{
			SupplyPOLine supply = e.Row;
			SOLine currentSOLine = Base2.SOLineDemand.Select();
			if (supply == null || !Base1.IsPoToSoOrBlanket(currentSOLine?.POSource))
				return;

			bool sOLineAllowedUpdate = Base2.SOLineDemand.Cache.AllowUpdate;
			bool enoughQtyForPurchase = supply.BaseOpenQty - supply.BaseDemandQty > 0m && currentSOLine.ShipComplete != SOShipComplete.ShipComplete
						|| supply.BaseOpenQty - supply.BaseDemandQty >= currentSOLine.BaseOrderQty * currentSOLine.CompleteQtyMin / 100m;
			bool enoughQtyForDropShip = supply.BaseOpenQty - supply.BaseDemandQty >= currentSOLine.BaseOrderQty;
			bool enoughOpenQty = enoughQtyForPurchase && currentSOLine.POSource != INReplenishmentSource.BlanketDropShipToOrder || enoughQtyForDropShip;

			bool linkedToCurrentSOLine = supply.LinkedSOLines?.Contains(currentSOLine.LineNbr.Value) ?? false;
			bool partiallyReceipted = supply.BaseReceivedQty > 0 && supply.OrderType != POOrderType.Blanket && linkedToCurrentSOLine;

			PXUIFieldAttribute.SetEnabled<SupplyPOLine.selected>(e.Cache, e.Row, (enoughOpenQty || linkedToCurrentSOLine) && !partiallyReceipted && sOLineAllowedUpdate);

			if (partiallyReceipted && sOLineAllowedUpdate)
				PXUIFieldAttribute.SetWarning<SupplyPOLine.selected>(e.Cache, e.Row, Messages.PurchaseOrderCannotBeDeselected);
		}

		/// <summary>
		/// Overrides <see cref="POLinkDialog.CollectSupplyPOLines(SOLine, ICollection{SupplyPOLine})"/>
		/// </summary>
		[PXOverride]
		public virtual void CollectSupplyPOLines(SOLine currentSOLine, ICollection<SupplyPOLine> supplyPOLines)
		{
			if (!Base1.IsPoToSoOrBlanket(currentSOLine.POSource))
				return;

			var suppliesQuery = new PXSelectJoin<SupplyPOLine,
				LeftJoin<SOLineSplit, On<SOLineSplit.FK.SupplyLine>>,
				Where<SupplyPOLine.lineType,
						In3<POLineType.goodsForInventory,
							POLineType.nonStock,
							POLineType.goodsForSalesOrder,
							POLineType.goodsForServiceOrder,
							POLineType.nonStockForSalesOrder,
							POLineType.nonStockForServiceOrder,
							POLineType.goodsForReplenishment>,
					And2<Where<SupplyPOLine.orderType, Equal<POOrderType.blanket>,
						And<Current<SOLine.pOSource>, In3<INReplenishmentSource.blanketDropShipToOrder, INReplenishmentSource.blanketPurchaseToOrder>,
						Or<SupplyPOLine.orderType, Equal<POOrderType.regularOrder>,
						And<Current<SOLine.pOSource>, Equal<INReplenishmentSource.purchaseToOrder>>>>>,
					And2<Where<SOLineSplit.pOLineNbr, IsNotNull,
						Or<SupplyPOLine.completed, Equal<False>,
							And<SupplyPOLine.cancelled, Equal<False>>>>,
					And<SupplyPOLine.inventoryID, Equal<Current<SOLine.inventoryID>>,
					And2<Where<SupplyPOLine.subItemID, Equal<Current<SOLine.subItemID>>,
						// Remove if we absolutely sure that line with non-stock item will never have null SubItemID.
						Or<Current<SOLine.subItemID>, IsNull>>,
					And<SupplyPOLine.siteID, Equal<Current<SOLine.pOSiteID>>,
					And2<Where<SupplyPOLine.vendorID, Equal<Current<SOLine.vendorID>>,
						Or<Current<SOLine.vendorID>, IsNull>>,
					And<Where2<
						Where<Current<SOLine.isSpecialOrder>, NotEqual<True>,
							And<SupplyPOLine.isSpecialOrder, NotEqual<True>>>,
						Or<Where<
							Current<SOLine.isSpecialOrder>, Equal<True>,
							And<SupplyPOLine.curyID, Equal<Current<SOOrder.curyID>>,
							And<SupplyPOLine.uOM, Equal<Current<SOLine.uOM>>,
							And<SupplyPOLine.projectID, Equal<Current<SOLine.projectID>>,
							And2<Where<SupplyPOLine.taskID, Equal<Current<SOLine.taskID>>,
								Or<SupplyPOLine.taskID, IsNull, And<Current<SOLine.taskID>, IsNull>>>,
							And<Where<SupplyPOLine.costCodeID, Equal<Current<SOLine.costCodeID>>,
								Or<SupplyPOLine.costCodeID, IsNull, And<Current<SOLine.costCodeID>, IsNull>>>>>>>>>>>>>>>>>>>>(Base);

			Dictionary<SupplyPOLine, HashSet<SOLineSplit>> supplyLineDemand = new Dictionary<SupplyPOLine, HashSet<SOLineSplit>>(
				new KeyValuesComparer<SupplyPOLine>(Base2.SupplyPOLines.Cache, Base2.SupplyPOLines.Cache.BqlKeys));

			foreach (PXResult<SupplyPOLine, SOLineSplit> row in suppliesQuery.Select())
			{
				SupplyPOLine supplyLine = row;
				SOLineSplit split = row;

				if (!supplyLineDemand.ContainsKey(supplyLine))
				{
					supplyLineDemand.Add(
						supplyLine,
						new HashSet<SOLineSplit>(
							new KeyValuesComparer<SOLineSplit>(
								Base.splits.Cache,
								Base.splits.Cache.BqlKeys)));
				}

				GatherSODemandFromDB(supplyLine, split, supplyLineDemand);
			}

			GatherSODemandFromCache(supplyLineDemand);
			bool sOLineAllowedUpdate = Base2.SOLineDemand.Cache.AllowUpdate;

			foreach (SupplyPOLine supplyLine in supplyLineDemand.Keys)
			{
				HashSet<SOLineSplit> demandSplits = supplyLineDemand[supplyLine];
				PXEntryStatus supplyLineStatus = Base2.SupplyPOLines.Cache.GetStatus(supplyLine);

				if (currentSOLine.IsSpecialOrder == true &&
					demandSplits.Any(s =>
						s.OrderType != currentSOLine.OrderType ||
						s.OrderNbr != currentSOLine.OrderNbr ||
						s.LineNbr != currentSOLine.LineNbr))
				{
					continue;
				}

				if (supplyLineStatus == PXEntryStatus.Notchanged)
				{
					supplyLine.SelectedSOLines = new int?[0];
					supplyLine.LinkedSOLines = new int?[0];
				}

				supplyLine.BaseDemandQty = 0m;
				supplyLine.LinkedSOLines.SparseArrayClear();

				decimal demandQty = 0m;
				bool linkedToCurrentSOLine = false;
				foreach (SOLineSplit split in demandSplits)
				{
					if (split.OrderType == currentSOLine.OrderType && split.OrderNbr == currentSOLine.OrderNbr)
					{
						supplyLine.LinkedSOLines = supplyLine.LinkedSOLines.SparseArrayAddDistinct(split.LineNbr.Value);

						if (supplyLineStatus == PXEntryStatus.Notchanged)
							supplyLine.SelectedSOLines = supplyLine.SelectedSOLines.SparseArrayAddDistinct(split.LineNbr.Value);

						if (split.LineNbr == currentSOLine.LineNbr)
							linkedToCurrentSOLine = true;
					}

					if (split.PlanID != null && split.POCompleted != true)
					{
						demandQty += split.BaseQty ?? 0m;
					}
				}

				supplyLine.BaseDemandQty = demandQty;
				supplyLine.Selected = supplyLine.SelectedSOLines.Contains(currentSOLine.LineNbr.Value);
				Base2.SupplyPOLines.Cache.Hold(supplyLine);

				if ((currentSOLine.POSource != INReplenishmentSource.PurchaseToOrder || supplyLine.OrderType != POOrderType.RegularOrder)
					&& (currentSOLine.POSource.IsNotIn(INReplenishmentSource.BlanketDropShipToOrder, INReplenishmentSource.BlanketPurchaseToOrder)
					|| supplyLine.OrderType != POOrderType.Blanket))
					return;

				if (currentSOLine.InvtMult * currentSOLine.OpenQty < 0m
					&& supplyLine.BaseOpenQty - supplyLine.BaseDemandQty > 0m
					&& (supplyLine.Hold != true || Base.SOPOLinkShowDocumentsOnHold)
					&& sOLineAllowedUpdate
					|| linkedToCurrentSOLine == true)
				{
					supplyPOLines.Add(supplyLine);
				}
			}
		}

		protected virtual void GatherSODemandFromDB(SupplyPOLine supplyLine, SOLineSplit split, Dictionary<SupplyPOLine, HashSet<SOLineSplit>> supplyLineDemand)
		{
			// Skip SOLineSplits that were unlinked, removed or relinked to another SupplyPOLine.
			split = Base.splits.Locate(split) ?? split;
			if (split?.SplitLineNbr != null && split.POLineNbr != null && Base.splits.Cache.GetStatus(split) != PXEntryStatus.Deleted
				&& split.POType == supplyLine.OrderType && split.PONbr == supplyLine.OrderNbr && split.POLineNbr == supplyLine.LineNbr)
			{
				supplyLineDemand[supplyLine].Add(split);
			}
		}

		protected virtual void GatherSODemandFromCache(Dictionary<SupplyPOLine, HashSet<SOLineSplit>> supplyLineDemand)
		{
			// SOLineSplits relinked to another SupplyPOLine or existing splits that were just linked.
			IEnumerable<SOLineSplit> updatedSplits = Base.splits.Cache.Updated.Cast<SOLineSplit>().Where(s => s.POLineNbr != null);
			foreach (SOLineSplit updatedSplit in updatedSplits)
			{
				SupplyPOLine origSupplyLine = new SupplyPOLine
				{
					OrderType = (string)Base.splits.Cache.GetValueOriginal<SOLineSplit.pOType>(updatedSplit),
					OrderNbr = (string)Base.splits.Cache.GetValueOriginal<SOLineSplit.pONbr>(updatedSplit),
					LineNbr = (int?)Base.splits.Cache.GetValueOriginal<SOLineSplit.pOLineNbr>(updatedSplit)
				};

				SupplyPOLine supplyLine = new SupplyPOLine
				{
					OrderType = updatedSplit.POType,
					OrderNbr = updatedSplit.PONbr,
					LineNbr = updatedSplit.POLineNbr
				};

				if (!supplyLineDemand.Comparer.Equals(origSupplyLine, supplyLine))
				{
					if (supplyLineDemand.ContainsKey(origSupplyLine))
						supplyLineDemand[origSupplyLine].Remove(updatedSplit);

					if (supplyLineDemand.ContainsKey(supplyLine))
						supplyLineDemand[supplyLine].Add(updatedSplit);
				}
			}

			// New SOLineSplits that were linked.
			IEnumerable<SOLineSplit> newLinkedSplits = Base.splits.Cache.Inserted.Cast<SOLineSplit>().Where(s => s.POLineNbr != null);
			foreach (SOLineSplit newLinkedSplit in newLinkedSplits)
			{
				SupplyPOLine supplyLine = new SupplyPOLine
				{
					OrderType = newLinkedSplit.POType,
					OrderNbr = newLinkedSplit.PONbr,
					LineNbr = newLinkedSplit.POLineNbr
				};

				if (supplyLineDemand.ContainsKey(supplyLine))
					supplyLineDemand[supplyLine].Add(newLinkedSplit);
			}
		}

		/// <summary>
		/// Overrides <see cref="POLinkDialog.LinkPOSupply(SOLine)" />
		/// </summary>
		[PXOverride]
		public virtual void LinkPOSupply(SOLine currentSOLine)
		{
			if (!Base1.IsPoToSoOrBlanket(currentSOLine.POSource))
				return;

			bool deleted = Base.Transactions.Cache.GetStatus(currentSOLine).IsIn(PXEntryStatus.Deleted, PXEntryStatus.InsertedDeleted);
			List<SOLineSplit> splits = Base.LineSplittingAllocatedExt.SelectSplitsForDropShip(currentSOLine).ToList();
			SOLineSplit firstSplit = splits.FirstOrDefault();
			if (firstSplit == null)
				return;

			if (currentSOLine.POSource == INReplenishmentSource.BlanketDropShipToOrder && Base.LineSplittingAllocatedExt.HasMultipleSplitsOrAllocation(currentSOLine))
				throw new PXException(Messages.DropShipSOLineCantHaveMultipleSplitsOrAllocation);

			// Unlink first.
			bool removedLink = false;
			foreach (SupplyPOLine supply in Base2.SupplyPOLines.Cache.Updated)
			{
				bool selected = supply.SelectedSOLines.Contains(currentSOLine.LineNbr.Value);
				bool linked = supply.LinkedSOLines.Contains(currentSOLine.LineNbr.Value);

				if (!selected && linked)
				{
					removedLink |= UnlinkSupply(supply, currentSOLine, splits, deleted);
				} 
			}

			// Then link.
			bool addedLink = false;
			foreach (SupplyPOLine supply in Base2.SupplyPOLines.Cache.Updated)
			{
				bool selected = supply.SelectedSOLines.Contains(currentSOLine.LineNbr.Value);
				bool linked = supply.LinkedSOLines.Contains(currentSOLine.LineNbr.Value);

				if (selected && !linked)
				{
					addedLink |= LinkSupply(supply, currentSOLine, splits);
				}
			}

			var soLine = Base.Transactions.Current;
			if (addedLink)
			{
				if (soLine.POCreated != true)
					Base.Transactions.Cache.SetValue<SOLine.pOCreated>(soLine, true);
			}
			else if (removedLink)
			{
				if (soLine.POCreated == true)
				{
					var linked = splits.Any(x => x.POCreate == true && x.PONbr != null);
					if (!linked)
						Base.Transactions.Cache.SetValue<SOLine.pOCreated>(soLine, linked);
				}
			}
		}

		protected virtual bool UnlinkSupply(SupplyPOLine supply, SOLine currentSOLine, IList<SOLineSplit> splits, bool deleted)
		{
			bool removedLink = false;

			foreach (SOLineSplit split in splits)
			{
				if (Base.splits.Cache.GetStatus(split) == PXEntryStatus.Deleted || Base.splits.Cache.GetStatus(split) == PXEntryStatus.InsertedDeleted)
					continue;

				if (split.POType == supply.OrderType && split.PONbr == supply.OrderNbr && split.POLineNbr == supply.LineNbr &&
					split.POCompleted == false && split.Completed == false)
				{
					if (split.POType != null && split.PONbr != null && split.POType == supply.OrderType && split.PONbr == supply.OrderNbr)
					{
						SupplyPOOrder supplyOrder = SupplyPOLine.FK.SupplyOrder.FindParent(Base, supply);

						if (supplyOrder != null)
						{
							if (split.RefNoteID == supplyOrder.NoteID)
								split.RefNoteID = null;

							if (supplyOrder.SOOrderType == split.OrderType && supplyOrder.SOOrderNbr == split.OrderNbr)
							{
								supplyOrder.SOOrderType = null;
								supplyOrder.SOOrderNbr = null;
								Base1.SupplyPOOrders.Update(supplyOrder);
							}
						}
					}

					if (!deleted)
					{
						if (split.PONbr != null)
						{
							split.ClearPOReferences();
							split.POCompleted = false;
							removedLink = true;
						}

						split.ReceivedQty = 0m;
						split.ShippedQty = 0m;
						split.Completed = false;

						Base.splits.Update(split);
					}
				}
			}

			supply.LinkedSOLines.SparseArrayRemove(currentSOLine.LineNbr.Value);

			return removedLink;
		}

		protected virtual bool LinkSupply(SupplyPOLine supply, SOLine currentSOLine, IList<SOLineSplit> splits)
		{
			bool addedLink = false;

			decimal? baseOpenQty = supply.BaseOpenQty - supply.BaseDemandQty;

			for (int i = 0; i < splits.Count; i++)
			{
				SOLineSplit split = PXCache<SOLineSplit>.CreateCopy(splits[i]);

				//TODO: it should not be possible to unallocate TR schedules
				if (string.IsNullOrEmpty(split.SOOrderNbr) && string.IsNullOrEmpty(split.PONbr) && split.IsAllocated == false && split.Completed == false
					&& (split.BaseQty > 0m && currentSOLine.POSource != INReplenishmentSource.BlanketDropShipToOrder || baseOpenQty >= currentSOLine.BaseQty))
				{
					if (supply.OrderType != POOrderType.Blanket)
					{
						supply.LineType = currentSOLine.LineType == SOLineType.Inventory ? POLineType.GoodsForSalesOrder : POLineType.NonStockForSalesOrder;
					}

					supply.LinkedSOLines = supply.LinkedSOLines.SparseArrayAddDistinct(currentSOLine.LineNbr.Value);

					INItemPlan supplyPlan = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(Base, supply.PlanID);
					if (supplyPlan == null)
						continue;

					if (supply.OrderType != POOrderType.Blanket)
					{
						supplyPlan.PlanType = INPlanConstants.Plan76;

						if (currentSOLine.IsSpecialOrder == true)
						{
							supplyPlan.CostCenterID = currentSOLine.CostCenterID;
						}

						Base.Caches[typeof(INItemPlan)].Update(supplyPlan);
					}

					INItemPlan demandPlan = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(Base, split.PlanID);

					demandPlan.PlanType = currentSOLine.POSource == INReplenishmentSource.BlanketPurchaseToOrder ? INPlanConstants.Plan6B
						: currentSOLine.POSource == INReplenishmentSource.BlanketDropShipToOrder ? INPlanConstants.Plan6E
						: currentSOLine.POSource == INReplenishmentSource.DropShipToOrder ? INPlanConstants.Plan6D
						: INPlanConstants.Plan66;

					demandPlan.FixedSource = INReplenishmentSource.Purchased;
					demandPlan.SupplyPlanID = supply.PlanID;

					SupplyPOOrder supplyOrder = SupplyPOLine.FK.SupplyOrder.FindParent(Base, supply);
					if (supplyOrder != null)
					{
						demandPlan.VendorID = supplyOrder.VendorID;
						demandPlan.VendorLocationID = supplyOrder.VendorLocationID;
					}

					Base.Caches[typeof(INItemPlan)].Update(demandPlan);

					split.POCreate = true;
					split.VendorID = supply.VendorID;
					split.POType = supply.OrderType;
					split.PONbr = supply.OrderNbr;
					split.POLineNbr = supply.LineNbr;
					addedLink = true;

					if (split.BaseQty <= baseOpenQty)
					{
						baseOpenQty -= split.BaseQty;
						split = Base.splits.Update(split);
					}
					else
					{
						SOLineSplit copy = PXCache<SOLineSplit>.CreateCopy(split);

						copy.SplitLineNbr = null;
						copy.IsAllocated = false;

						copy.ClearPOFlags();
						copy.ClearPOReferences();
						copy.VendorID = null;
						copy.POCreate = true;

						copy.BaseQty = copy.BaseQty - baseOpenQty;
						copy.Qty = INUnitAttribute.ConvertFromBase(Base.splits.Cache, copy.InventoryID, copy.UOM, (decimal)copy.BaseQty, INPrecision.QUANTITY);
						copy.ShippedQty = 0m;
						copy.ReceivedQty = 0m;
						copy.UnreceivedQty = copy.BaseQty;
						copy.PlanID = null;
						copy.Completed = false;

						split.BaseQty = baseOpenQty;
						split.Qty = INUnitAttribute.ConvertFromBase(Base.splits.Cache, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY);
						baseOpenQty = 0m;
						split = Base.splits.Update(split);

						if ((copy = Base.splits.Insert(copy)) != null)
						{
							splits.Insert(i + 1, copy);
						}
					}
					splits[i] = split;
				}

				if (baseOpenQty <= 0m)
					break;
			}

			return addedLink;
		}
	}
}
