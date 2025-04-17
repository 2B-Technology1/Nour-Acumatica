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
using PX.Data.BQL.Fluent;
using PX.Objects.BQLConstants;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.SO;
using System;
using System.Collections.Generic;
using System.Linq;
using SOLineSplit3 = PX.Objects.PO.POOrderEntry.SOLineSplit3;

namespace PX.Objects.PO.GraphExtensions.POReceiptEntryExt
{
	public class PurchaseSupplyExt : PXGraphExtension<POReceiptEntry>
	{
		public static bool IsActive()
			=> PXAccess.FeatureInstalled<FeaturesSet.sOToPOLink>();

		/// <summary>
		/// Overrides <see cref="POReceiptEntry.ReduceSOAllocationOnReleaseReturn(PXResult{POReceiptLine}, POReceiptLineSplit, POLineUOpen)" />
		/// </summary>
		[PXOverride]
		public virtual void ReduceSOAllocationOnReleaseReturn(PXResult<POReceiptLine> row, POReceiptLineSplit posplit, POLineUOpen poline,
			Action<PXResult<POReceiptLine>, POReceiptLineSplit, POLineUOpen> baseMethod)
		{
			baseMethod?.Invoke(row, posplit, poline);

			POReceiptLine2 origLine = row.GetItem<POReceiptLine2>();
			var cachedPOLine = (POLineUOpen)Base.Caches<POLineUOpen>().Locate(poline) ?? poline;

			if (POOrderType.IsNormalType(origLine?.POType) && origLine.LineType == POLineType.GoodsForSalesOrder && cachedPOLine.Completed != true)
			{
				var splits = SelectFrom<SOLineSplit>
					.InnerJoin<SOLineSplit3>.On<SOLineSplit.orderType.IsEqual<SOLineSplit3.orderType>
						.And<SOLineSplit.orderNbr.IsEqual<SOLineSplit3.orderNbr>>
						.And<SOLineSplit.lineNbr.IsEqual<SOLineSplit3.lineNbr>>
						.And<SOLineSplit.parentSplitLineNbr.IsEqual<SOLineSplit3.splitLineNbr>>>
					.Where<SOLineSplit.pOReceiptType.IsEqual<POReceiptLine.receiptType.FromCurrent>
						.And<SOLineSplit.pOReceiptNbr.IsEqual<POReceiptLine.receiptNbr.FromCurrent>>
						.And<SOLineSplit3.pOType.IsEqual<POReceiptLine.pOType.FromCurrent>>
						.And<SOLineSplit3.pONbr.IsEqual<POReceiptLine.pONbr.FromCurrent>>
						.And<SOLineSplit3.pOLineNbr.IsEqual<POReceiptLine.pOLineNbr.FromCurrent>>
						.And<POReceiptLineSplit.lotSerialNbr.FromCurrent.NoDefault.IsNull.
							Or<POReceiptLineSplit.lotSerialNbr.FromCurrent.NoDefault.IsEqual<EmptyString>>.
							Or<SOLineSplit.lotSerialNbr.IsNull>.
							Or<SOLineSplit.lotSerialNbr.IsEqual<EmptyString>>.
							Or<SOLineSplit.lotSerialNbr.IsEqual<POReceiptLineSplit.lotSerialNbr.FromCurrent.NoDefault>>>>
					.OrderBy<SOLineSplit.lotSerialNbr.Desc>
					.View.ReadOnly.SelectMultiBound(Base, new object[] { origLine, posplit }).RowCast<SOLineSplit>();

				decimal returnQty = posplit.BaseQty ?? 0m;

				foreach (var sosplit in splits)
				{
					returnQty -= UpdateSOAllocation(sosplit, cachedPOLine, returnQty);
					if (returnQty <= 0)
						break;
				}
			}
		}

		protected virtual decimal UpdateSOAllocation(SOLineSplit sosplit, POLineUOpen poline, decimal returnQty)
		{
			decimal currentReturnQty = 0;

			SOLineSplit activeSplit = FindActiveSplit(sosplit);
			if (activeSplit?.IsAllocated != true || activeSplit.ShippedQty != 0)
				return currentReturnQty;

			INItemPlan plan = SelectFrom<INItemPlan>.
				Where<INItemPlan.planID.IsEqual<SOLineSplit.planID.FromCurrent>>.
				View.SelectSingleBound(Base, new object[] { activeSplit });

			if (plan?.PlanType != INPlanConstants.Plan61)
				return currentReturnQty;

			currentReturnQty = SplitAllocation(activeSplit, plan, returnQty);
			SOLineSplit parent = FindParent(sosplit);
			UpdateParent(parent, poline, currentReturnQty);

			return currentReturnQty;
		}

		protected virtual SOLineSplit FindActiveSplit(SOLineSplit split)
		{
			SOLineSplit currentSplit = split;

			while(currentSplit?.Completed == true)
			{
				var childSplit =
					SelectFrom<SOLineSplit>.
					Where<SOLineSplit.orderType.IsEqual<SOLineSplit.orderType.FromCurrent>
						.And<SOLineSplit.orderNbr.IsEqual<SOLineSplit.orderNbr.FromCurrent>>
						.And<SOLineSplit.lineNbr.IsEqual<SOLineSplit.lineNbr.FromCurrent>>
						.And<SOLineSplit.parentSplitLineNbr.IsEqual<SOLineSplit.splitLineNbr.FromCurrent>>
						.And<SOLineSplit.lotSerialNbr.IsNull.Or<SOLineSplit.lotSerialNbr.IsEqual<SOLineSplit.lotSerialNbr.FromCurrent>>>>.
					View.SelectSingleBound(Base, new object[] { currentSplit });

				currentSplit = childSplit;
			}

			return currentSplit;
		}

		protected virtual decimal SplitAllocation(SOLineSplit split, INItemPlan plan, decimal returnQty)
		{
			decimal currentReturnQty = Math.Min(returnQty, split.BaseQty ?? 0m);

			split.BaseQty -= currentReturnQty;
			split.Qty = INUnitAttribute.ConvertFromBase(Base.solinesplitselect.Cache, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY);

			if (split.BaseQty == 0m)
				split.Completed = true;

			Base.solinesplitselect.Update(split);

			
			if (plan.PlanQty != currentReturnQty)
			{
				plan.PlanQty -= currentReturnQty;
				Base.Caches<INItemPlan>().Update(plan);
			}
			else
			{
				Base.Caches<INItemPlan>().Delete(plan);
			}

			return currentReturnQty;
		}

		protected virtual SOLineSplit FindParent(SOLineSplit split)
		{
			return SelectFrom<SOLineSplit>.
				Where<SOLineSplit.orderType.IsEqual<SOLineSplit.orderType.FromCurrent>
					.And<SOLineSplit.orderNbr.IsEqual<SOLineSplit.orderNbr.FromCurrent>>
					.And<SOLineSplit.lineNbr.IsEqual<SOLineSplit.lineNbr.FromCurrent>>
					.And<SOLineSplit.splitLineNbr.IsEqual<SOLineSplit.parentSplitLineNbr.FromCurrent>>>.
				View.SelectSingleBound(Base, new object[] { split });
		}

		protected virtual void UpdateParent(SOLineSplit parent, POLineUOpen poline, decimal returnQty)
		{
			parent.BaseReceivedQty -= returnQty;
			parent.ReceivedQty = INUnitAttribute.ConvertFromBase(Base.solinesplitselect.Cache, parent.InventoryID, parent.UOM, (decimal)parent.BaseReceivedQty, INPrecision.QUANTITY);
			
			if (parent.Completed != true)
			{
				Base.solinesplitselect.Update(parent);

				INItemPlan plan = SelectFrom<INItemPlan>.
					Where<INItemPlan.planID.IsEqual<SOLineSplit.planID.FromCurrent>>.
					View.SelectSingleBound(Base, new object[] { parent });

				if (plan == null)
					throw new Common.Exceptions.RowNotFoundException(Base.Caches<INItemPlan>(), parent.PlanID);

				plan.PlanQty += returnQty;
				Base.Caches<INItemPlan>().Update(plan);
			}
			else
			{
				parent.Completed = false;
				parent.PlanID = InsertParentPlan(parent, poline, returnQty);
				Base.solinesplitselect.Update(parent);
			}
		}

		protected virtual long? InsertParentPlan(SOLineSplit parent, POLineUOpen poline, decimal returnQty)
		{
			INItemPlan plan = SelectFrom<INItemPlan>.
				Where<INItemPlan.planID.IsEqual<POLineUOpen.planID.FromCurrent>>.
				View.SelectSingleBound(Base, new object[] { poline });

			if (plan == null)
				throw new Common.Exceptions.RowNotFoundException(Base.Caches<INItemPlan>(), poline.PlanID);

			var poorder = PXParentAttribute.SelectParent<POOrder>(Base.Caches<POLineUOpen>(), poline);
			if (poorder == null)
				throw new Common.Exceptions.RowNotFoundException(Base.poOrderUPD.Cache, poline.OrderType, poline.OrderNbr);

			var soorder = PXParentAttribute.SelectParent<SOOrder>(Base.solinesplitselect.Cache, parent);
			if (soorder == null)
				throw new Common.Exceptions.RowNotFoundException(Base.soorderselect.Cache, parent.OrderType, parent.OrderNbr);

			var soline = PXParentAttribute.SelectParent<SOLine>(Base.solinesplitselect.Cache, parent);
			if (soline == null)
				throw new Common.Exceptions.RowNotFoundException(Base.solineselect.Cache, parent.OrderType, parent.OrderNbr, parent.LineNbr);

			var newPlan = PXCache<INItemPlan>.CreateCopy(plan);
			newPlan.PlanID = null;
			newPlan.SupplyPlanID = plan.PlanID;
			newPlan.SiteID = soline.SiteID;
			newPlan.SourceSiteID = soline.SiteID;
			newPlan.CostCenterID = soline.CostCenterID;
			newPlan.PlanType = INPlanConstants.Plan66;
			newPlan.VendorID = poorder.VendorID;
			newPlan.VendorLocationID = poorder.VendorLocationID;
			newPlan.UOM = parent.UOM;
			newPlan.BAccountID = soorder.CustomerID;
			newPlan.Hold = soorder.Hold;
			newPlan.RefNoteID = soorder.NoteID;
			newPlan.RefEntityType = typeof(SOOrder).FullName;
			newPlan.PlanQty = returnQty;
			newPlan.PlanDate = soline.ShipDate;

			newPlan = (INItemPlan)Base.Caches<INItemPlan>().Insert(newPlan);

			return newPlan.PlanID;
		}
	}
}
