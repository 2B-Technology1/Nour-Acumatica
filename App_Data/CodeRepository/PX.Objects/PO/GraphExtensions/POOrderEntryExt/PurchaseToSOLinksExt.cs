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
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.SO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SOLineSplit3 = PX.Objects.PO.POOrderEntry.SOLineSplit3;

namespace PX.Objects.PO.GraphExtensions.POOrderEntryExt
{
	// Code here is shared between legacy Drop-Ship feature & PO to SO.
	// If you want to change something here that may affect legacy Drop-Ship functionality,
	// please move legacy implementation to DropShipLegacyLinksExt (should be created if does not extist).
	public class PurchaseToSOLinksExt : PXGraphExtension<POOrderEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.dropShipments>() || PXAccess.FeatureInstalled<CS.FeaturesSet.sOToPOLink>();
		}

		public PXAction<POOrder> viewDemand;

		[PXUIField(DisplayName = Messages.ViewDemand, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton(VisibleOnDataSource = false)]
		public virtual IEnumerable ViewDemand(PXAdapter adapter)
		{
			Base.FixedDemand.AskExt();
			return adapter.Get();
		}

		protected virtual void POOrder_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			POOrder doc = e.Row as POOrder;
			if (e.Row == null || doc.OrderType == POOrderType.DropShip)
				return;

			PXUIFieldAttribute.SetEnabled<POOrder.sOOrderType>(cache, doc, false);
			PXUIFieldAttribute.SetEnabled<POOrder.sOOrderNbr>(cache, doc, false);
		}

		protected virtual void POLine_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			POLine row = (POLine)e.Row;
			POOrder doc = Base.Document.Current;
			if (doc == null || row == null || (doc.IsLegacyDropShip != true && doc.OrderType == POOrderType.DropShip
				&& row.LineType.IsIn(POLineType.GoodsForDropShip, POLineType.NonStockForDropShip)))
				return;

			if (Base.IsExport && !Base.IsContractBasedAPI) return;//for performance 

			bool isLegacyPOLinkedToSO = doc.IsLegacyDropShip == true && row.Completed == true && Base.IsLinkedToSO(row);
			if (Base.Document.Current.Hold != true || isLegacyPOLinkedToSO)
			{
				PXSetPropertyException exception = null;

				if (doc.Status == POOrderStatus.Hold && isLegacyPOLinkedToSO)
					exception = new PXSetPropertyException(Messages.POLineLinkedToSO, PXErrorLevel.RowWarning);

				Base.Transactions.Cache.RaiseExceptionHandling<POLine.lineType>(row, row.LineType, exception);
			}
		}

		protected virtual void POLine_InventoryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			POOrder doc = Base.Document.Current;
			POLine row = (POLine)e.Row;
			if (doc == null || (doc.OrderType == POOrderType.DropShip && doc.IsLegacyDropShip != true) || row?.InventoryID == null)
				return;

			SOLineSplit link = PXSelect<SOLineSplit,
				Where<SOLineSplit.pOType, Equal<Required<SOLineSplit.pOType>>,
					And<SOLineSplit.pONbr, Equal<Required<SOLineSplit.pONbr>>,
					And<SOLineSplit.pOLineNbr, Equal<Required<SOLineSplit.pOLineNbr>>>>>>
				.SelectWindowed(Base, 0, 1, row.OrderType, row.OrderNbr, row.LineNbr);
			if (link != null && link.InventoryID != (int?)e.NewValue)
			{
				InventoryItem item = InventoryItem.PK.Find(Base, (int?)e.NewValue);

				var ex = new PXSetPropertyException<POLine.inventoryID>(Messages.ChangingInventoryForLinkedRecord);
				ex.ErrorValue = item?.InventoryCD;

				throw ex;
			}
		}

		protected virtual void POOrder_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			POOrder doc = (POOrder)e.Row;
			if (doc.OrderType == POOrderType.DropShip && doc.IsLegacyDropShip != true)
				return;

			Base.Transactions.View.SetAnswer(null, WebDialogResult.OK);
		}

		protected virtual void POLine_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			POLine row = (POLine)e.Row;
			if ((Base.Document.Current.OrderType != POOrderType.DropShip || Base.Document.Current.IsLegacyDropShip == true)
				&& row.LineType.IsIn(POLineType.GoodsForSalesOrder, POLineType.GoodsForDropShip, POLineType.NonStockForSalesOrder,
					POLineType.NonStockForDropShip) && row.IsSpecialOrder != true)
			{
				SOLineSplit first;
				using (new PXFieldScope(Base.RelatedSOLineSplit.View,
					typeof(SOLineSplit.orderType),
					typeof(SOLineSplit.orderNbr),
					typeof(SOLineSplit.lineNbr),
					typeof(SOLineSplit.splitLineNbr)))
				{
					first = (SOLineSplit)Base.RelatedSOLineSplit.View.SelectMultiBound(new object[] { e.Row }).FirstOrDefault();
				}

				if (first == null)
					return;

				string message = PXMessages.LocalizeFormatNoPrefixNLA(Messages.POLineLinkedToSOLine, first.OrderNbr);
				if (Base.Transactions.View.Ask(message, MessageButtons.OKCancel) == WebDialogResult.Cancel)
				{
					e.Cancel = true;
				}
			}
		}

		protected virtual void POLine_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if ((Base.Document.Current.OrderType == POOrderType.DropShip && Base.Document.Current.IsLegacyDropShip != true))
				return;

			POLine row = (POLine)e.Row;
			using (new PXFieldScope(Base.RelatedSOLineSplit.View,
				typeof(SOLineSplit.orderType),
				typeof(SOLineSplit.orderNbr),
				typeof(SOLineSplit.lineNbr),
				typeof(SOLineSplit.splitLineNbr)))
			{
				foreach (SOLineSplit r in Base.RelatedSOLineSplit.View.SelectMultiBound(new object[] { e.Row }))
				{
					var upd = (SOLineSplit3)PXSelect<SOLineSplit3,
						Where<SOLineSplit3.orderType, Equal<Required<SOLineSplit3.orderType>>,
							And<SOLineSplit3.orderNbr, Equal<Required<SOLineSplit3.orderNbr>>,
							And<SOLineSplit3.lineNbr, Equal<Required<SOLineSplit3.lineNbr>>,
							And<SOLineSplit3.splitLineNbr, Equal<Required<SOLineSplit3.splitLineNbr>>>>>>>
						.Select(Base, r.OrderType, r.OrderNbr, r.LineNbr, r.SplitLineNbr);

					upd.POType = null;
					upd.PONbr = null;
					upd.POLineNbr = null;
					upd.POCancelled = false;
					upd.POCompleted = false;
					upd.RefNoteID = null;

					bool poCreated = false;
					if (upd.POCreated == true)
					{
						bool anyLinked = (SOLineSplit3)PXSelect<SOLineSplit3,
							Where<SOLineSplit3.orderType, Equal<Required<SOLineSplit3.orderType>>,
							And<SOLineSplit3.orderNbr, Equal<Required<SOLineSplit3.orderNbr>>,
							And<SOLineSplit3.lineNbr, Equal<Required<SOLineSplit3.lineNbr>>,
							And<SOLineSplit3.pONbr, IsNotNull,
							And<SOLineSplit3.splitLineNbr, NotEqual<Required<SOLineSplit3.splitLineNbr>>>>>>>>
							.SelectWindowed(Base, 0, 1, upd.OrderType, upd.OrderNbr, upd.LineNbr, upd.SplitLineNbr) != null;
						poCreated = anyLinked;
					}
					Base.UpdateSOLine(upd, upd.VendorID, poCreated);

					if (upd.LinePOCreate == false)
						upd.POCreate = false;

					Base.FixedDemand.Update(upd);
					INItemPlan plan = INItemPlan.PK.Find(Base, upd.PlanID);
					if (plan?.PlanType != null && plan.SupplyPlanID == row.PlanID)
					{
						if (upd.POCreate == false)
						{
							var op = SOOrderTypeOperation.PK.Find(Base, upd.OrderType, upd.Operation);
							if (op != null && op.OrderPlanType != null)
								plan.PlanType = op.OrderPlanType;
						}
						plan.SupplyPlanID = null;
						sender.Graph.Caches[typeof(INItemPlan)].Update(plan);
					}
				}
			}
		}

		protected virtual void POLine_SiteID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			POLine row = (POLine)e.Row;
			if (e.NewValue is int newValue
				&& row.LineType.IsIn(POLineType.GoodsForSalesOrder, POLineType.NonStockForSalesOrder))
			{
				SOLineSplit3 linkedBlanketSplit = Base.FixedDemand.Select().AsEnumerable().RowCast<SOLineSplit3>()
					.FirstOrDefault(s => s.Behavior == SOBehavior.BL && s.SiteID != newValue);
				if (linkedBlanketSplit != null)
				{
					INSite site = INSite.PK.Find(Base, newValue);
					throw new PXSetPropertyException(SO.Messages.WarehouseFixedBecauseLinkedToBlanket, linkedBlanketSplit.OrderNbr)
					{
						ErrorValue = site?.SiteCD ?? e.NewValue
					};
				}
			}
		}

		/// <summary>
		/// Overrides <see cref="POOrderEntry.Persist"/>
		/// </summary>
		[PXOverride]
		public virtual void Persist(Action baseMethod)
		{
			UpdateDemandSchedules();
			CleanLinksToDeletedSupplyPlans();
			baseMethod();
		}

		protected virtual void UpdateDemandSchedules()
		{
			var lineCache = Base.Transactions.Cache;

			var changedLines = lineCache.Inserted.Cast<POLine>()
				.Where(l => l.LineType.IsIn(POLineType.GoodsForSalesOrder, POLineType.NonStockForSalesOrder)
					&& (l.Completed == true || l.Cancelled == true))
				.Union(lineCache.Updated.Cast<POLine>())
				.Where(l => l.LineType.IsIn(POLineType.GoodsForSalesOrder, POLineType.NonStockForSalesOrder)
					&& (l.Completed != (bool?)lineCache.GetValueOriginal<POLine.completed>(l)
					|| l.Cancelled != (bool?)lineCache.GetValueOriginal<POLine.cancelled>(l)));

			foreach (POLine line in changedLines)
			{
				var demandSchedules = Base.FixedDemand.View.SelectMultiBound(new[] { line });

				foreach (SOLineSplit3 schedule in demandSchedules)
				{
					if (schedule.POCompleted == line.Completed && schedule.POCancelled == line.Cancelled)
						continue;

					schedule.POCompleted = line.Completed;
					schedule.POCancelled = line.Cancelled;
					Base.FixedDemand.Update(schedule);
				}
			}
		}

		protected virtual void CleanLinksToDeletedSupplyPlans()
		{
			PXCache itemPlanCache = Base.Caches[typeof(INItemPlan)];
			foreach (var deletedDSPlan in itemPlanCache.Deleted.Cast<INItemPlan>()
				.Where(p => p.PlanType.IsIn(INPlanConstants.Plan74, INPlanConstants.Plan79, // DropShip
					INPlanConstants.Plan78, INPlanConstants.Plan76))) // PO to SO
			{
				var demandPlans = SelectFrom<INItemPlan>
					.Where<INItemPlan.supplyPlanID.IsEqual<@P.AsInt>>
					.View.Select(Base, deletedDSPlan.PlanID).RowCast<INItemPlan>();
				foreach (INItemPlan demandPlan in demandPlans)
				{
					demandPlan.SupplyPlanID = null;
					itemPlanCache.MarkUpdated(demandPlan);
				}
			}
		}
	}
}
