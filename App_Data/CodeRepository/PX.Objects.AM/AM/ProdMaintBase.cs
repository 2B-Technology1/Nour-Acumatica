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
using PX.Data.WorkflowAPI;
using PX.Objects.AM.Attributes;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.Objects.AM.CacheExtensions;
using System.Collections;
using System;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using System.Linq;

namespace PX.Objects.AM
{
	public abstract class ProdMaintBase<TGraph> : PXGraph<TGraph, AMProdItem>
		where TGraph : ProdMaintBase<TGraph>, new()
	{
		[PXHidden]
		public PXSetup<AMPSetup> ampsetup;

		public virtual string GetMaterialStatus(AMProdItem prodItem, AMProdMatl prodMatl, bool forceComplete = false)
		{
			if (prodItem == null || prodMatl == null) return null;

			if (prodItem.Canceled == true) return ProductionOrderStatus.Cancel;

			if (prodItem.Released == false) return ProductionOrderStatus.Planned;

			if (prodItem.Closed == true) return ProductionOrderStatus.Closed;

			if ((forceComplete && prodItem.Completed == true) || prodItem.Locked == true) return ProductionOrderStatus.Completed;

			if (prodMatl.QtyActual == 0 && prodItem.Completed == true) return ProductionOrderStatus.Released;

			if (prodMatl.QtyRemaining == 0 && prodItem.Completed == true) return ProductionOrderStatus.Completed;

			if (prodItem.Completed == true) return ProductionOrderStatus.InProcess;

			if (prodMatl.QtyActual == 0) return ProductionOrderStatus.Released;

			if (prodMatl.QtyRemaining == 0) return ProductionOrderStatus.Completed;

			return ProductionOrderStatus.InProcess;
		}

		public virtual void SetAllMaterialStatus(AMProdItem prodItem, bool forceComplete = false)
		{
			if (prodItem?.ProdOrdID == null) return;

			foreach (AMProdMatl productionMaterial in SelectFrom<AMProdMatl>
				.Where<AMProdMatl.orderType.IsEqual<@P.AsString>
					.And<AMProdMatl.prodOrdID.IsEqual<@P.AsString>>>
				.View.Select(this, prodItem.OrderType, prodItem.ProdOrdID))
			{
				SetMaterialStatus(prodItem, productionMaterial, forceComplete);
			}
		}

		public virtual void SetMaterialStatus(AMProdItem prodItem, AMProdMatl productionMaterial, bool forceComplete = false)
		{
			if (prodItem?.ProdOrdID == null) return;

			var cachedMatl = Caches[typeof(AMProdMatl)].LocateElse(productionMaterial);
			if (cachedMatl == null) return;

			var matlStatus = GetMaterialStatus(prodItem, productionMaterial, forceComplete);
			if (matlStatus != null && matlStatus != cachedMatl.StatusID)
			{
				cachedMatl.StatusID = matlStatus;

				if (!string.IsNullOrEmpty(cachedMatl.LotSerialNbr))
				{
					cachedMatl.LotSerialNbr = null;
				}

				Caches[typeof(AMProdMatl)].Update(cachedMatl);
			}
		}

		public virtual void SetProductionOrderStatus(AMProdItem prodItem, string newStatusID)
		{
			if (prodItem == null
			|| string.IsNullOrWhiteSpace(prodItem.OrderType))
			{
				throw new PXArgumentException(nameof(prodItem));
			}
			if (string.IsNullOrWhiteSpace(prodItem.StatusID))
			{
				throw new PXArgumentException(nameof(prodItem.StatusID));
			}

			var origStatusID = prodItem.StatusID;

			try
			{
				var newProdEvent = new AMProdEvnt();
				AMProdTotal amProdTotal = SelectFrom<AMProdTotal>.
					Where<AMProdTotal.orderType.IsEqual<@P.AsString>.
						And<AMProdTotal.prodOrdID.IsEqual<@P.AsString>>>.
					View.Select(this, prodItem.OrderType, prodItem.ProdOrdID);

				switch (newStatusID)
				{
					case ProductionOrderStatus.Planned:
						prodItem.RelDate = null;
						newProdEvent = ProductionEventHelper.BuildStatusEvent(prodItem, origStatusID, newStatusID);
						ProductionTransactionHelper.UpdatePlannedProductionTotals(this, prodItem, amProdTotal);
						break;
					case ProductionOrderStatus.Released:

						if (prodItem.DetailSource == ProductionDetailSource.Configuration)
						{
							// If order is marked as a configuration - the configuration must be complete before releasing
							AMConfigurationResults config = SelectFrom<AMConfigurationResults>.
								Where<AMConfigurationResults.prodOrderType.IsEqual<@P.AsString>.
									And<AMConfigurationResults.prodOrderNbr.IsEqual<@P.AsString>>>
								.View.Select(this, prodItem.OrderType, prodItem.ProdOrdID);
							if (config == null || !config.Completed.GetValueOrDefault())
							{
								throw new PXException(Messages.ProdConfigNotFinish, prodItem.OrderType.TrimIfNotNullEmpty(), prodItem.ProdOrdID.TrimIfNotNullEmpty());
							}
						}

						prodItem.RelDate = Accessinfo.BusinessDate;
						newProdEvent = ProductionEventHelper.BuildStatusEvent(prodItem, origStatusID, newStatusID);
						BOMCostRoll.UpdatePlannedMaterialCosts(this, prodItem);
						ProductionTransactionHelper.UpdatePlannedProductionTotals(this, prodItem, amProdTotal);
						break;
					case ProductionOrderStatus.Completed:
						prodItem.Completed = true;
						newProdEvent = ProductionEventHelper.BuildStatusEvent(prodItem, origStatusID, newStatusID);
						break;
					case ProductionOrderStatus.Hold:
						newProdEvent = ProductionEventHelper.BuildStatusEvent(prodItem, origStatusID, newStatusID);
						break;
					case ProductionOrderStatus.Cancel:
						prodItem.Canceled = true;
						ProductionTransactionHelper.UpdateProdOperActualCostTotals(this, prodItem);
						newProdEvent = ProductionEventHelper.BuildStatusEvent(prodItem, origStatusID, newStatusID);
						break;
					case ProductionOrderStatus.Closed:
						prodItem.Closed = true;
						ProductionTransactionHelper.UpdateProdOperActualCostTotals(this, prodItem);
						newProdEvent = ProductionEventHelper.BuildStatusEvent(prodItem, origStatusID, newStatusID);						
						break;
				}

				prodItem.TranDate = this.Accessinfo.BusinessDate;

				if (prodItem.DemandPlanID != null)
				{
					var itemPlan = IN.INItemPlan.PK.Find(this, prodItem.DemandPlanID);
					if (itemPlan == null)
					{
						prodItem.DemandPlanID = null;
					}
				}
				prodItem = (AMProdItem)this.Caches[typeof(AMProdItem)].Update(prodItem);

				foreach (AMProdItemSplit prodItemSplit in PXSelect<AMProdItemSplit,
				Where<AMProdItemSplit.orderType, Equal<Required<AMProdItemSplit.orderType>>,
				And<AMProdItemSplit.prodOrdID, Equal<Required<AMProdItemSplit.prodOrdID>>>>>
				.Select(this, prodItem.OrderType, prodItem.ProdOrdID))
				{
					prodItemSplit.StatusID = prodItem.StatusID;
					this.Caches[typeof(AMProdItemSplit)].Update(prodItemSplit);
				}

				SetAllOperationStatus(prodItem, newStatusID);
				PXTrace.WriteInformation(Messages.GetLocal(Messages.ProductionOrderStatusUpdatedTo,
				prodItem.OrderType.TrimIfNotNullEmpty(),
				prodItem.ProdOrdID.TrimIfNotNullEmpty(),
				ProductionOrderStatus.GetStatusDescription(prodItem.StatusID)));

				if (newProdEvent != null && !string.IsNullOrWhiteSpace(newProdEvent.ProdOrdID))
				{
					if (this.Caches[typeof(AMProdItem)].Current == null)
					{
						this.Caches[typeof(AMProdItem)].Current = prodItem;
					}
					this.Caches[typeof(AMProdEvnt)].Insert(newProdEvent);
				}
			}
			catch (Exception e)
			{
				PXTraceHelper.PxTraceException(e);
				throw new PXException(Messages.GetLocal(Messages.ProductionOrderStatusChangeError,
											prodItem.OrderType.TrimIfNotNullEmpty(),
											prodItem.ProdOrdID.TrimIfNotNullEmpty(),
											ProductionOrderStatus.GetStatusDescription(origStatusID),
											ProductionOrderStatus.GetStatusDescription(newStatusID),
											e.Message));
			}
		}

		protected virtual AMProdOper SetOperationStatus(AMProdItem prodItem, AMProdOper operation, string newStatusID)
		{
			if (prodItem?.ProdOrdID == null || operation?.OperationCD == null || string.IsNullOrWhiteSpace(newStatusID))
			{
				return operation;
			}

			operation.StatusID = newStatusID;
			switch (newStatusID)
			{
				case ProductionOrderStatus.Planned:
					break;
				case ProductionOrderStatus.Released:
					if (prodItem.FirstOperationID == operation.OperationID)
					{
						operation.QtytoProd = prodItem.QtytoProd;
						operation.BaseQtytoProd = prodItem.BaseQtytoProd;
					}
					break;
				case ProductionOrderStatus.InProcess:
					break;
				case ProductionOrderStatus.Completed:
					operation.ActEndDate = Common.Dates.Now;
					break;
			}

			return (AMProdOper)Caches[typeof(AMProdOper)].Update(operation);
		}

		protected virtual void SetAllOperationStatus(AMProdItem prodItem, string newStatusID)
		{
			if (prodItem?.ProdOrdID == null || string.IsNullOrWhiteSpace(newStatusID))
			{
				return;
			}

			foreach (AMProdOper operation in PXSelectReadonly<AMProdOper,
			Where<AMProdOper.orderType, Equal<Required<AMProdOper.orderType>>,
			And<AMProdOper.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>>>
			>.Select(this, prodItem.OrderType, prodItem.ProdOrdID))
			{
				Caches[typeof(AMProdOper)].Current = SetOperationStatus(prodItem, Caches[typeof(AMProdOper)].LocateElse(operation), newStatusID);
			}
		}

		public virtual void EndProductionOrder(AMProdItem prodItem, FinancialPeriod financialPeriod, WIPAdjustmentEntry wipAdjustmentEntry)
		{
			if (prodItem?.ProdOrdID == null)
			{
				return;
			}

			if (ProductionTransactionHelper.ProductionOrderHasUnreleasedTransactions(this, prodItem, out var unreleasedMsg))
			{
				throw new PXException(unreleasedMsg);
			}

			ProductionScheduleEngine.DeleteOrderSchedule(prodItem);

			foreach (AMProdMatlSplit prodMatlSplit in PXSelect<AMProdMatlSplit,
			Where<AMProdMatlSplit.orderType, Equal<Required<AMProdMatlSplit.orderType>>,
			And<AMProdMatlSplit.prodOrdID, Equal<Required<AMProdMatlSplit.prodOrdID>>
			>>>.Select(this, prodItem?.OrderType, prodItem?.ProdOrdID))
			{
				if (prodMatlSplit == null)
				{
					continue;
				}

				Caches[typeof(AMProdMatlSplit)].Delete(prodMatlSplit);
			}

			var needsWipAdjustment = prodItem.WIPBalance.GetValueOrDefault() != 0;
			if (!needsWipAdjustment)
			{
				return;
			}

			if (financialPeriod == null || string.IsNullOrWhiteSpace(financialPeriod.FinancialPeriodID))
			{
				DateTime financialDate = wipAdjustmentEntry == null
				? this.Accessinfo.BusinessDate.GetValueOrDefault()
				: wipAdjustmentEntry.Accessinfo.BusinessDate.GetValueOrDefault();
				financialPeriod = new FinancialPeriod
				{
					FinancialPeriodID = ProductionTransactionHelper.PeriodFromDate(this, financialDate)
				};
			}

			if (wipAdjustmentEntry == null)
			{
				wipAdjustmentEntry = CreateInstance<WIPAdjustmentEntry>();
			}

			wipAdjustmentEntry.ampsetup.Current.RequireControlTotal = false;

			if (wipAdjustmentEntry.batch.Current == null)
			{
				var aAMBatch = wipAdjustmentEntry.batch.Insert();
				aAMBatch.Hold = false;
				aAMBatch.FinPeriodID = financialPeriod.FinancialPeriodID;
				aAMBatch.TranDesc = AMTranType.GetTranDescription(AMTranType.WIPvariance);
				wipAdjustmentEntry.batch.Update(aAMBatch);
			}

			var ammTranWipVar = wipAdjustmentEntry.transactions.Insert();
			ammTranWipVar.OrderType = prodItem.OrderType;
			ammTranWipVar.ProdOrdID = prodItem.ProdOrdID;

			ammTranWipVar.OperationID = prodItem.LastOperationID;
			ammTranWipVar.TranAmt = prodItem.WIPBalance * -1;
			ammTranWipVar.TranType = AMTranType.WIPvariance;

			ammTranWipVar.TranDesc = $"{AMTranType.GetTranDescription(AMTranType.WIPvariance)} - {prodItem.OrderType} {prodItem.ProdOrdID}";
			if (prodItem.Canceled == true
			&& !string.IsNullOrWhiteSpace(ProductionOrderStatus.GetStatusDescription(prodItem.StatusID)))
			{
				ammTranWipVar.TranDesc = Messages.GetLocal(Messages.StatusChangedTo, ammTranWipVar.TranDesc, ProductionOrderStatus.GetStatusDescription(prodItem.StatusID));
			}
			ammTranWipVar.FinPeriodID = financialPeriod.FinancialPeriodID;
			ammTranWipVar.WIPAcctID = prodItem.WIPAcctID;
			ammTranWipVar.WIPSubID = prodItem.WIPSubID;
			ammTranWipVar.AcctID = prodItem.WIPVarianceAcctID;
			ammTranWipVar.SubID = prodItem.WIPVarianceSubID;
			wipAdjustmentEntry.transactions.Update(ammTranWipVar);
		}

		public static AMProdItem RaiseProdItemEvent(PXGraph graph, AMProdItem doc, SelectedEntityEvent<AMProdItem> prodEvent)
		{
			prodEvent.FireOn(graph, doc);
			var productionItem = graph.Caches[typeof(AMProdItem)];
			return (AMProdItem)productionItem.Update(doc);
		}
		public override void Persist()
		{
			PXCache cache = Caches[typeof(AMProdItem)];
			var current = (AMProdItem)cache.Current;
			if (current != null && cache.GetStatus(current) == PXEntryStatus.Updated)
			{
				var original = (AMProdItem)cache.GetOriginal(current);
				if (original != null && current.Hold.GetValueOrDefault() != original.Hold.GetValueOrDefault())
				{
					var putOnHold = current.Hold.GetValueOrDefault() && !original.Hold.GetValueOrDefault();
					var prodEvent = ProductionEventHelper.BuildStatusEvent(current, original.StatusID, current.StatusID,
					putOnHold ? ProductionEventType.OrderPlaceOnHold : ProductionEventType.OrderRemoveFromHold);
					if (prodEvent?.Description != null)
					{
						this.Caches[typeof(AMProdEvnt)].Insert(prodEvent);
					}
				}
			}
			base.Persist();
		}

		#region Actions
		public PXInitializeState<AMProdItem> initializeState;

		public PXAction<AMProdItem> putOnHold;
		[PXUIField(DisplayName = "Hold", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable PutOnHold(PXAdapter adapter) => adapter.Get();

		public PXAction<AMProdItem> releaseFromHold;
		[PXUIField(DisplayName = "Remove Hold", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ReleaseFromHold(PXAdapter adapter)
		{
			PXCache cache = Caches[typeof(AMProdItem)];
			var current = (AMProdItem)cache.Current;
			if (current != null)
			{				
				if (!ValidatePreassignLotSerQty(current))
					throw new PXException(
						Messages.GetLocal(
							Messages.PreassignedLotSerialQuantityOutOfSync,
							UomHelper.FormatQty(GetPreassignLotSerQty(current)),
							current.OrderType, current.ProdOrdID,
							UomHelper.FormatQty(current.BaseQtytoProd)),
							PXErrorLevel.Error
							);
			}
			return adapter.Get();
		}

		protected virtual bool ValidatePreassignLotSerQty(AMProdItem item)
		{
			var preassignLotSerQty = GetPreassignLotSerQty(item);
			return preassignLotSerQty == null || item.BaseQtytoProd == preassignLotSerQty;
		}

		protected virtual decimal? GetPreassignLotSerQty(AMProdItem item)
		{
			if (InventoryHelper.LotSerialTrackingFeatureEnabled && item.PreassignLotSerial == true)
			{
				PXResultset<AMProdItemSplit> splits = PXSelect<AMProdItemSplit,
				Where<AMProdItemSplit.orderType, Equal<Current<AMProdItem.orderType>>,
					And<AMProdItemSplit.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>>.Select(this);
				if (splits != null)
				{
					return splits.RowCast<AMProdItemSplit>().Where(x => !string.IsNullOrEmpty(x.LotSerialNbr))?.Select(x => x.BaseQty)?.Sum() ?? 0m;
				}
			}
			return null;
		}

		public PXAction<AMProdItem> CriticalMatl;
		[PXUIField(DisplayName = Messages.CriticalMaterial, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable criticalMatl(PXAdapter adapter)
		{
			PXCache cache = Caches[typeof(AMProdItem)];
			var current = (AMProdItem)cache.Current;
			if (current != null)
			{
				CriticalMaterialsInq cm = PXGraph.CreateInstance<CriticalMaterialsInq>();
				cm.ProdItemRecs.Current.OrderType = current.OrderType;
				cm.ProdItemRecs.Current.ProdOrdID = current.ProdOrdID;
				cm.ProdItemRecs.Current.ShowAll = false;
				throw new PXRedirectRequiredException(cm, Messages.CriticalMaterial);
			}
			return adapter.Get();
		}

		public PXAction<AMProdItem> InventoryAllocationDetailInq;

		[PXUIField(DisplayName = "Allocation Details", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable inventoryAllocationDetailInq(PXAdapter adapter)
		{
			PXCache cache = Caches[typeof(AMProdItem)];
			var current = (AMProdItem)cache.Current;
			if (((AMProdItem)cache?.Current)?.InventoryID != null)
			{
				var allocGraph = CreateInstance<InventoryAllocDetEnq>();
				allocGraph.Filter.Current.InventoryID = current.InventoryID;
				if (InventoryHelper.SubItemFeatureEnabled)
				{
					var subItem = (INSubItem)PXSelectorAttribute.Select<AMProdItem.subItemID>(cache, current);
					if (!string.IsNullOrWhiteSpace(subItem?.SubItemCD))
					{
						allocGraph.Filter.Current.SubItemCD = subItem.SubItemCD;
					}
				}
				allocGraph.Filter.Current.SiteID = current.SiteID;
				allocGraph.RefreshTotal.Press();
				PXRedirectHelper.TryRedirect(allocGraph, PXRedirectHelper.WindowMode.New);
			}

			return adapter.Get();
		}

		public PXAction<AMProdItem> AttributesInq;
		[PXUIField(DisplayName = Messages.Attributes, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable attributesInq(PXAdapter adapter)
		{
			PXCache cache = Caches[typeof(AMProdItem)];
			var current = (AMProdItem)cache.Current;
			if (current != null)
			{
				ProductionAttributesInq.RedirectGraph(current);
			}
			return adapter.Get();
		}

		public PXAction<AMProdItem> ProductionScheduleBoardRedirect;
		[PXUIField(DisplayName = "Production Schedule Board", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable productionScheduleBoardRedirect(PXAdapter adapter)
		{
			PXCache cache = Caches[typeof(AMProdItem)];
			var current = (AMProdItem)cache.Current;
			if (current?.OrderType == null)
			{
				return adapter.Get();
			}

			ManufacturingDiagram.RedirectTo(new ManufacturingDiagram.ManufacturingDiagramFilter
			{
				OrderType = current.OrderType,
				ProdOrdId = current.ProdOrdID,
				IncludeOnHold = true
			});

			return adapter.Get();
		}

		public PXAction<AMProdItem> LateAssignmentEntry;
		[PXUIField(DisplayName = "Late Assignment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable lateAssignmentEntry(PXAdapter adapter)
		{
			PXCache cache = Caches[typeof(AMProdItem)];
			var currentOrder = (AMProdItem)cache?.Current;
			if (currentOrder == null || cache.GetStatus(currentOrder) == PXEntryStatus.Inserted)
			{
				return adapter.Get();
			}

			LateAssignmentMaint.Redirect(currentOrder.OrderType, currentOrder.ProdOrdID, null);

			return adapter.Get();
		}

		public PXAction<AMProdItem> CreatePurchaseOrderInq;
		[PXUIField(DisplayName = Messages.CreatePurchaseOrdersInq, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable createPurchaseOrderInq(PXAdapter adapter)
		{
			PXCache cache = Caches[typeof(AMProdItem)];
			var prodItem = (AMProdItem)cache?.Current;
			if (prodItem?.InventoryID == null || cache.GetStatus(prodItem) == PXEntryStatus.Inserted)
			{
				return adapter.Get();
			}

			var poGraph = CreateInstance<POCreate>();
			var filterExt = PXCache<POCreate.POCreateFilter>.GetExtension<POCreateFilterExt>(poGraph.Filter?.Current);
			if (filterExt == null)
			{
				return adapter.Get();
			}

			filterExt.AMOrderType = prodItem.OrderType;
			filterExt.ProdOrdID = prodItem.ProdOrdID;

			PXRedirectHelper.TryRedirect(poGraph, PXRedirectHelper.WindowMode.New);

			return adapter.Get();
		}

		public PXAction<AMProdItem> CreateProductionOrderInq;
		[PXUIField(DisplayName = Messages.CreateProductionOrdersInq, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable createProductionOrderInq(PXAdapter adapter)
		{
			PXCache cache = Caches[typeof(AMProdItem)];
			var prodItem = (AMProdItem)cache?.Current;
			if (prodItem?.InventoryID == null || cache.GetStatus(prodItem) == PXEntryStatus.Inserted)
			{
				return adapter.Get();
			}

			var cpGraph = CreateInstance<CreateProductionOrdersProcess>();
			cpGraph.Filter.Current.OrderType = prodItem.OrderType;
			cpGraph.Filter.Current.ProdOrdID = prodItem.ProdOrdID;

			PXRedirectHelper.TryRedirect(cpGraph, PXRedirectHelper.WindowMode.New);

			return adapter.Get();
		}

		public PXWorkflowEventHandler<AMProdItem> OnUpdateStatus;

		public PXAction<AMProdItem> SetMaterialsOpen;
		[PXUIField(DisplayName = Messages.SetMaterialsOpen, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable setMaterialsOpen(PXAdapter adapter)
		{
			PXCache cache = Caches[typeof(AMProdItem)];
			var prodItem = (AMProdItem)cache?.Current;

			SetAllMaterialStatus(prodItem);
			Actions.PressSave();
			return adapter.Get();
		}

		public PXAction<AMProdItem> SetMaterialsCompleted;
		[PXUIField(DisplayName = Messages.SetMaterialsCompleted, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable setMaterialsCompleted(PXAdapter adapter)
		{
			PXCache cache = Caches[typeof(AMProdItem)];
			var prodItem = (AMProdItem)cache?.Current;

			SetAllMaterialStatus(prodItem, true);
			Actions.PressSave();
			return adapter.Get();
		}

		public PXAction<AMProdItem> CloseOrderWorkflow;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXButton]
		public virtual IEnumerable closeOrderWorkflow(PXAdapter adapter)
		{
			return adapter.Get();
		}
		#endregion
	}
}
