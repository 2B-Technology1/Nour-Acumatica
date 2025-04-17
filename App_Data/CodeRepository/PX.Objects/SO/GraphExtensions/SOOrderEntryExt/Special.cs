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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated;
using PX.Objects.PM;
using PX.Objects.PO;

namespace PX.Objects.SO.GraphExtensions.SOOrderEntryExt
{
	public class Special : PXGraphExtension<PurchaseToSOLinkDialog, POLinkDialog, PurchaseSupplyBaseExt, SOOrderEntry>
	{
		[PXLocalizable]
		public static class ExtensionMessages
		{
			public const string SpecialLineExistsDeleteOrder = "At least one line with a special-order item in the sales order is linked to a line of a purchase order. Do you want to remove the link and delete the sales order?";
			public const string SpecialCheckboxCannotBeSelectedPOExists = "The Special Order check box cannot be selected because the line has already been linked to a purchase order line.";
		}

		public static bool IsActive()
			=> PXAccess.FeatureInstalled<FeaturesSet.specialOrders>();

		protected HashSet<int?> _deletedCostCenters;
		protected List<(IBqlTable Row, PXEntryStatus Status)> _rowsToRestore;

		#region Events
		#region SOOrder

		protected virtual void _(Events.RowSelected<SOOrder> e)
		{
			if (e.Row == null)
				return;

			if (HasLinkedSpecialLine(e.Row))
			{
				Base.Delete.SetConfirmationMessage(ExtensionMessages.SpecialLineExistsDeleteOrder);
			}
			else
			{
				Base.Delete.SetConfirmationMessage(ActionsMessages.ConfirmDeleteExplicit);
			}
		}

		protected virtual void _(Events.FieldVerifying<SOOrder, SOOrder.projectID> e)
		{
			if (e.Row != null && !object.Equals(e.OldValue, e.NewValue) && HasLinkedSpecialLine(e.Row))
			{
				var project = PMProject.PK.Find(Base, e.Row.ProjectID);
				e.NewValue = project?.ContractCD;
				throw new PXSetPropertyException(Messages.CannotChangeFieldOnSpecialOrder);
			}
		}

		protected virtual void _(Events.FieldVerifying<SOOrder, SOOrder.curyID> e)
		{
			if (e.Row != null && !object.Equals(e.OldValue, e.NewValue) && HasLinkedSpecialLine(e.Row))
			{
				e.NewValue = e.OldValue;
				throw new PXSetPropertyException(Messages.CannotChangeFieldOnSpecialOrder);
			}
		}

		protected virtual void _(Events.FieldVerifying<SOOrder, SOOrder.cancelled> e)
		{
			if (e.Row.Cancelled == false && (bool?)e.NewValue == true && HasLinkedSpecialLine(e.Row))
			{
				if (HasReceivedSpecialLine(e.Row))
				{
					e.NewValue = false;
					e.Cancel = true;
					throw new PXSetPropertyException(Messages.SpecialLineReceivedCancelOrder);
				}

				if (!PXLongOperation.IsLongOperationContext())
				{
					if (Base.Document.Ask(Messages.Warning, Messages.SpecialLineExistsCancelOrder, MessageButtons.YesNo) != WebDialogResult.Yes)
					{
						e.NewValue = false;
						e.Cancel = true;
					}
				}
				else
				{
					throw new PXException(Messages.SpecialLineReceivedCancelOrderMassProcess);
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<SOOrder, SOOrder.destinationSiteID> e)
		{
			if (e.Row?.Behavior == SOBehavior.TR && !object.Equals(e.NewValue, e.OldValue))
			{
				if (HasSpecialLine(e.Row))
				{
					var site = INSite.PK.Find(Base, e.Row.DestinationSiteID);
					e.NewValue = site?.SiteCD;
					throw new PXSetPropertyException(Messages.CannotChangeSiteOnSpecialTransfer, site?.SiteCD?.Trim());
				}
			}
		}

		protected virtual void _(Events.RowUpdated<SOOrder> e, PXRowUpdated baseEventHandler)
		{
			if (e.Row.Cancelled == true && e.OldRow.Cancelled != true && HasSpecialLine(e.Row))
			{
				foreach (SOLine line in GetLinkedSpecialLines(e.Row))
				{
					RemovePOLink(line);
				}
			}

			baseEventHandler?.Invoke(e.Cache, e.Args);
		}

		protected virtual void _(Events.RowDeleting<SOOrder> e)
		{
			if (HasLinkedSpecialLine(e.Row))
			{
				if (HasBilledOrCompletedSpecialLine(e.Row))
					throw new PXException(Messages.SpecialLineCompletedDeleteOrder);

				if (HasReceivedSpecialLine(e.Row))
					throw new PXException(Messages.SpecialLineReceivedDeleteOrder);
			}
		}

		#endregion // SOOrder
		#region SOLine
		#region CacheAttached
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDBCalced(typeof(SOLine.isSpecialOrder), typeof(bool), Persistent = true)]
		protected virtual void _(Events.CacheAttached<SOLine.origIsSpecialOrder> e) { }
		#endregion // CacheAttached

		protected virtual void _(Events.RowSelected<SOLine> e)
		{
			if (e.Row == null)
				return;

			var order = Base.Document.Current;

			e.Cache.AdjustUI(e.Row)
				.For<SOLine.isSpecialOrder>(a => a.Enabled =
					e.Row.Behavior.IsIn(SOBehavior.SO, SOBehavior.RM, SOBehavior.QT) &&
					(order.Hold == true || (order.Cancelled != true && order.Completed != true && order.DontApprove == true)) &&
					e.Row.Operation == SOOperation.Issue &&
					e.Row.LineType == SOLineType.Inventory &&
					e.Row.POSource.IsIn(null, INReplenishmentSource.PurchaseToOrder));

			if (e.Row.IsSpecialOrder == true && e.Row.IsCostUpdatedOnPO == true)
			{
				e.Cache.RaiseExceptionHandling<SOLine.curyUnitCost>(e.Row, e.Row.CuryUnitCost,
					new PXSetPropertyException(Messages.UnitCostUpdatedOnPO, PXErrorLevel.Warning));
			}

			if (e.Row.Behavior == SOBehavior.TR && e.Row.IsSpecialOrder == true)
			{
				e.Cache.AdjustUI(e.Row)
					.For<SOLine.siteID>(a => a.Enabled = false)
					.SameFor<SOLine.pOCreate>();
			}
		}

		protected virtual void _(Events.FieldVerifying<SOLine, SOLine.uOM> e)
		{
			if (e.Row?.IsSpecialOrder == true && e.Row.POCreated == true && !object.Equals(e.OldValue, e.NewValue))
			{
				var poorders = GetPurchaseOrderNumbers(e.Row);
				throw new PXSetPropertyException(Messages.CannotMakeChangeOnSpecialOrder, poorders);
			}
		}

		protected virtual void _(Events.FieldVerifying<SOLine, SOLine.taskID> e)
		{
			if (e.Row?.IsSpecialOrder == true && e.Row.POCreated == true && !object.Equals(e.OldValue, e.NewValue))
			{
				var task = PMTask.PK.Find(Base, e.Row.ProjectID, e.Row.TaskID);
				e.NewValue = task?.TaskCD;

				var poorders = GetPurchaseOrderNumbers(e.Row);
				throw new PXSetPropertyException(Messages.CannotMakeChangeOnSpecialOrder, poorders);
			}
		}

		protected virtual void _(Events.FieldVerifying<SOLine, SOLine.costCodeID> e)
		{
			if (e.Row?.IsSpecialOrder == true && e.Row.POCreated == true && !object.Equals(e.OldValue, e.NewValue))
			{
				var costCode = PMCostCode.PK.Find(Base, e.Row.CostCodeID);
				e.NewValue = costCode?.CostCodeCD;

				var poorders = GetPurchaseOrderNumbers(e.Row);
				throw new PXSetPropertyException(Messages.CannotMakeChangeOnSpecialOrder, poorders);
			}
		}

		protected virtual void _(Events.FieldVerifying<SOLine, SOLine.siteID> e)
		{
			if (e.Row?.IsSpecialOrder == true && e.Row.POCreated == true && !object.Equals(e.OldValue, e.NewValue))
			{
				var site = INSite.PK.Find(Base, e.Row.SiteID);
				e.NewValue = site?.SiteCD;

				var poorders = GetPurchaseOrderNumbers(e.Row);
				throw new PXSetPropertyException(Messages.CannotMakeChangeOnSpecialOrder, poorders);
			}
		}

		protected virtual void _(Events.FieldVerifying<SOLine, SOLine.isSpecialOrder> e)
		{
			if (e.Row == null || object.Equals(e.OldValue, e.NewValue))
				return;

			if (e.Row.POCreated == true)
			{
				if ((bool?)e.NewValue == false)
				{
					foreach (SOLineSplit split in Base.splits.View.SelectMultiBound(new object[] { e.Row }))
					{
						if (!string.IsNullOrEmpty(split.POReceiptNbr))
						{
							e.NewValue = true;
							throw new PXSetPropertyException(Messages.CannotClearSpecialOrderFlag);
						}
					}

					foreach (var poorder in GetPurchaseOrders(e.Row))
					{
						if (poorder.Cancelled != true && poorder.Hold != true)
						{
							e.NewValue = true;
							throw new PXSetPropertyException(Messages.CannotClearSpecialOrderFlag);
						}
					}
				}
				else
				{
					e.NewValue = false;
					throw new PXSetPropertyException(ExtensionMessages.SpecialCheckboxCannotBeSelectedPOExists);
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<SOLine, SOLine.completed> e)
		{
			if (e.Row?.IsSpecialOrder == true && e.Row.POCreated == true && e.Row.Completed != true && (bool?)e.NewValue == true)
			{
				bool shipmentExists = (int?)Base.Document.Cache.GetValueOriginal<SOOrder.openShipmentCntr>(Base.Document.Current) > 0;
				if (!shipmentExists && HasReceivedSpecialLine(e.Row))
				{
					e.NewValue = false;
					throw new PXSetPropertyException(Messages.CannotCompleteReceivedSpecialLine);
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<SOLine, SOLine.isSpecialOrder> e)
		{
			if (e.Row == null || object.Equals(e.OldValue, e.NewValue) ||
				e.Row.Operation == SOOperation.Receipt || e.Row.Behavior == SOBehavior.TR)
			{
				return;
			}

			if (e.Row.IsSpecialOrder == true)
			{
				if (e.Row.POCreate != true)
					e.Cache.SetValueExt<SOLine.pOCreate>(e.Row, true);

				if (e.Row.POSource != INReplenishmentSource.PurchaseToOrder)
					e.Cache.SetValueExt<SOLine.pOSource>(e.Row, INReplenishmentSource.PurchaseToOrder);
			}
			else
			{
				e.Cache.SetDefaultExt<SOLine.unitCost>(e.Row);
			}
		}

		protected virtual void _(Events.RowUpdated<SOLine> e)
		{
			if (e.Row.IsSpecialOrder == false && e.OldRow.IsSpecialOrder == true && e.OldRow.POCreated == true)
			{
				RemovePOLink(e.Row);
			}
		}

		protected virtual void _(Events.RowPersisted<SOLine> e)
		{
			switch (e.TranStatus)
			{
				case PXTranStatus.Open:
					if (e.Row.OrigIsSpecialOrder == true &&
						(e.Row.IsSpecialOrder != true || e.Operation.Command() == PXDBOperation.Delete))
					{
						var costCenters = SelectFrom<INCostCenter>.
							Where<INCostCenter.sOOrderType.IsEqual<SOLine.orderType.FromCurrent>
								.And<INCostCenter.sOOrderNbr.IsEqual<SOLine.orderNbr.FromCurrent>>
								.And<INCostCenter.sOOrderLineNbr.IsEqual<SOLine.lineNbr.FromCurrent>>>
							.View.SelectMultiBound(Base, new object[] { e.Row });

						foreach (INCostCenter costCenter in costCenters)
						{ 
							// Acuminator disable once PX1073 ExceptionsInRowPersisted We use RowPersisted because: 1. In normal cases, there is no exception; 2. If Cancel of RowPersistingEventArgs is set, we don't need to delete the cost cetner.
							// Acuminator disable once PX1043 SavingChangesInEventHandlers This is exception, we need to delete records that the system updates by accumulators.
							DeleteCostCenter(costCenter.CostCenterID);
						}
					}
					break;

				case PXTranStatus.Aborted:
					if (_rowsToRestore != null)
					{
						foreach (var rowToRestore in _rowsToRestore)
						{
							Base.Caches[rowToRestore.Row.GetType()].SetStatus(rowToRestore.Row, rowToRestore.Status);
						}
						_rowsToRestore.Clear();
					}
					_deletedCostCenters?.Clear();
					break;

				case PXTranStatus.Completed:
					_deletedCostCenters?.Clear();
					_rowsToRestore?.Clear();
					break;
			}
		}

		protected virtual void _(Events.RowDeleting<SOLine> e)
		{
			if (e.Row.IsSpecialOrder == true && e.Row.POCreated == true)
			{
				if (HasBilledOrCompletedSpecialLine(e.Row))
					throw new PXException(Messages.SpecialLineCompletedDeleteLine);

				if (HasReceivedSpecialLine(e.Row))
					throw new PXException(Messages.SpecialLineReceivedDeleteLine);

				if (Base.Document.Cache.GetStatus(Base.Document.Current) != PXEntryStatus.Deleted &&
					Base.Document.Ask(Messages.Warning, Messages.SpecialLineExistsDeleteLine, MessageButtons.YesNo) != WebDialogResult.Yes)
				{
					e.Cancel = true;
				}
			}
		}

		protected virtual void _(Events.RowDeleted<SOLine> e)
		{
			if (e.Row.IsSpecialOrder == true && e.Row.POCreated == true)
			{
				RemovePOLink(e.Row);
			}
		}

		#endregion SOLine
		#region INCostCenter
		protected virtual void _(Events.RowPersisting<INCostCenter> e)
		{
			if (e.Operation.Command() == PXDBOperation.Insert && e.Row.CostLayerType == CostLayerType.Special && e.Row.CostCenterID < 0)
			{
				var soline = Base.Transactions.Locate(
					new SOLine { OrderType = e.Row.SOOrderType, OrderNbr = e.Row.SOOrderNbr, LineNbr = e.Row.SOOrderLineNbr });

				if (Base.Transactions.Cache.GetStatus(soline).IsIn(PXEntryStatus.InsertedDeleted, PXEntryStatus.Deleted)
					|| (soline != null && soline.IsSpecialOrder != true)
					|| (soline == null && SOLine.PK.Find(Base, e.Row.SOOrderType, e.Row.SOOrderNbr, e.Row.SOOrderLineNbr) == null))
				{
					e.Cancel = true;
					e.Cache.SetStatus(e.Row, PXEntryStatus.InsertedDeleted);
				}
			}
		}
		#endregion
		#region SiteStatusByCostCenter
		protected virtual void _(Events.RowPersisting<SiteStatusByCostCenter> e)
		{
			if (e.Operation.Command() == PXDBOperation.Insert && e.Row.CostCenterID < 0)
			{
				var costCenterCache = Base.Caches<INCostCenter>();
				var costCenter = (INCostCenter)costCenterCache.Locate(new INCostCenter() { CostCenterID = e.Row.CostCenterID });

				if (costCenter?.CostLayerType == CostLayerType.Special && costCenterCache.GetStatus(costCenter) == PXEntryStatus.InsertedDeleted)
				{
					e.Cancel = true;
					e.Cache.SetStatus(e.Row, PXEntryStatus.InsertedDeleted);

					if (!e.Row.IsZero())
						throw new PXLockViolationException(typeof(INCostCenter), PXDBOperation.Delete, new object[] { e.Row.CostCenterID });
				}
			}
		}
		#endregion
		#region SupplyPOLine
		public virtual void _(Events.FieldVerifying<SupplyPOLine, SupplyPOLine.selected> e)
		{
			if (e.Row == null || object.Equals(e.OldValue, e.NewValue))
				return;

			var soline = Base.Transactions.Current;
			if ((bool?)e.NewValue == true && soline?.IsSpecialOrder == true)
			{
				if (HasPOWithDifferentCost(soline, e.Row))
					throw new PXSetPropertyException(Messages.CannotLinkPOWithDifferentCost);
			}
		}
		#endregion // SupplyPOLine
		#endregion // Events

		#region Methods

		protected virtual POOrder[] GetPurchaseOrders(SOLine line)
		{
			return SelectFrom<SOLineSplit>
				.InnerJoin<POOrder>.On<SOLineSplit.FK.POOrder>
				.Where<SOLineSplit.FK.OrderLine.SameAsCurrent>
				.View.SelectMultiBound(Base, new object[] { line })
				.RowCast<POOrder>()
				.DistinctByKeys(Base)
				.ToArray();
		}

		protected virtual string GetPurchaseOrderNumbers(SOLine line)
		{
			var orders = GetPurchaseOrders(line);
			return string.Join(", ", orders.Select(o => $"{o.OrderType} {o.OrderNbr}"));
		}

		protected virtual bool HasSpecialLine(SOOrder order)
		{
			return order.SpecialLineCntr > 0;
		}

		protected virtual bool HasLinkedSpecialLine(SOOrder order)
		{
			return HasSpecialLine(order) && (SOLine)GetLinkedSpecialLines(order) != null;
		}

		protected virtual PXResultset<SOLine> GetLinkedSpecialLines(SOOrder order)
		{
			return SelectFrom<SOLine>
				.Where<SOLine.FK.Order.SameAsCurrent
					.And<SOLine.isSpecialOrder.IsEqual<True>>
					.And<SOLine.pOCreated.IsEqual<True>>>
				.View.SelectMultiBound(Base, new object[] { order });
		}

		protected virtual bool HasBilledOrCompletedSpecialLine(SOOrder order)
		{
			if (!HasSpecialLine(order)) return false;

			var line = (SOLineSplit)SelectFrom<SOLineSplit>
				.InnerJoin<POLine>.On<SOLineSplit.FK.POLine>
				.Where<SOLineSplit.FK.Order.SameAsCurrent
					.And<POLine.isSpecialOrder.IsEqual<True>>
					.And<POLine.billedQty.IsNotEqual<decimal0>
						.Or<POLine.completed.IsEqual<True>>
						.Or<POLine.cancelled.IsEqual<True>>
						.Or<POLine.closed.IsEqual<True>>>>
				.View.SelectSingleBound(Base, new object[] { order });

			return line != null;
		}

		protected virtual bool HasBilledOrCompletedSpecialLine(SOLine line)
		{
			var split = (SOLineSplit)SelectFrom<SOLineSplit>
				.InnerJoin<POLine>.On<SOLineSplit.FK.POLine>
				.Where<SOLineSplit.FK.OrderLine.SameAsCurrent
					.And<POLine.isSpecialOrder.IsEqual<True>>
					.And<POLine.billedQty.IsNotEqual<decimal0>
						.Or<POLine.completed.IsEqual<True>>
						.Or<POLine.cancelled.IsEqual<True>>
						.Or<POLine.closed.IsEqual<True>>>>
				.View.SelectSingleBound(Base, new object[] { line });

			return split != null;
		}


		protected virtual bool HasReceivedSpecialLine(SOOrder order)
			=> HasSpecialLine(order) && HasReceivedSpecialLine(order.OrderType, order.OrderNbr);

		protected virtual bool HasReceivedSpecialLine(SOLine line)
			=> HasReceivedSpecialLine(line.OrderType, line.OrderNbr, line.LineNbr);

		private bool HasReceivedSpecialLine(string orderType, string orderNbr, int? lineNbr = null)
		{
			var costCenter = (INCostCenter)SelectFrom<INCostCenter>
				.InnerJoin<INSiteStatusByCostCenter>.On<INCostCenter.costCenterID.IsEqual<INSiteStatusByCostCenter.costCenterID>>
				.Where<INCostCenter.costLayerType.IsEqual<CostLayerType.special>
					.And<INCostCenter.sOOrderType.IsEqual<SOLine.orderType.AsOptional>>
					.And<INCostCenter.sOOrderNbr.IsEqual<SOLine.orderNbr.AsOptional>>
					.And<INCostCenter.sOOrderLineNbr.IsEqual<@P.AsInt>
						.Or<@P.AsInt.IsNull>>
					.And<INSiteStatusByCostCenter.qtyActual.IsGreater<decimal0>
						.Or<INSiteStatusByCostCenter.qtyPOFixedReceipts.IsGreater<decimal0>>>>
				.View.Select(Base, orderType, orderNbr, lineNbr, lineNbr);

			return costCenter != null;
		}

		protected virtual void RemovePOLink(SOLine line)
		{
			var oldCurrent = Base.Transactions.Current;

			try
			{
				Base.Transactions.Current = line;

				foreach (SupplyPOLine poline in Base2.SupplyPOLines.Select())
				{
					if (poline.Selected == true)
					{
						poline.Selected = false;
						if (Base.Transactions.Cache.GetStatus(line).IsIn(PXEntryStatus.Deleted, PXEntryStatus.InsertedDeleted))
							poline.SODeleted = true;

						Base2.SupplyPOLines.Update(poline);
					}
				}
				Base2.LinkPOSupply(line);
			}
			finally
			{
				Base.Transactions.Current = oldCurrent;
			}
		}

		protected virtual void DeleteCostCenter(int? costCenterID)
		{
			if (costCenterID == null || costCenterID < 0)
				throw new PXArgumentException(nameof(costCenterID));

			if (_deletedCostCenters == null)
				_deletedCostCenters = new HashSet<int?>();

			if (_rowsToRestore == null)
				_rowsToRestore = new List<(IBqlTable Row, PXEntryStatus Status)>();

			INItemPlan plan = SelectFrom<INItemPlan>
				.Where<INItemPlan.costCenterID.IsEqual<INCostCenter.costCenterID.AsOptional>>.View.Select(Base, costCenterID);

			if (plan != null)
				throw new PXLockViolationException(typeof(INCostCenter), PXDBOperation.Delete, new object[] { costCenterID });

			DeleteStatusRecord<INSiteStatusByCostCenter, SiteStatusByCostCenter>(costCenterID);
			DeleteStatusRecord<INLocationStatusByCostCenter, LocationStatusByCostCenter>(costCenterID);
			DeleteStatusRecord<INLotSerialStatusByCostCenter, LotSerialStatusByCostCenter>(costCenterID);

			if (!PXDatabase.Delete<INCostSite>(
				new PXDataFieldRestrict<INCostSite.costSiteID>(costCenterID),
				new PXDataFieldRestrict<INCostSite.costSiteType>(nameof(INCostCenter))))
			{
				throw new PXLockViolationException(typeof(INCostCenter), PXDBOperation.Delete, new object[] { costCenterID });
			}

			var costCenterCache = Base.Caches<INCostCenter>();
			var costCenter = (INCostCenter)costCenterCache.Locate(new INCostCenter() { CostCenterID = costCenterID });
			if (costCenter != null)
			{
				_rowsToRestore.Add((costCenter, costCenterCache.GetStatus(costCenter)));
				costCenterCache.Remove(costCenter);
			}

			if (!PXDatabase.Delete<INCostCenter>(
				new PXDataFieldRestrict<INCostCenter.costCenterID>(costCenterID)))
			{
				throw new PXLockViolationException(typeof(INCostCenter), PXDBOperation.Delete, new object[] { costCenterID });
			}

			_deletedCostCenters.Add(costCenterID);
		}

		protected virtual void DeleteStatusRecord<TStatus, TStatusAccumulator>(int? costCenterID)
			where TStatus : class, IStatus, IBqlTable, new()
			where TStatusAccumulator : class, IStatus, IBqlTable, new()
		{
			var cache = Base.Caches[typeof(TStatusAccumulator)];

			foreach (TStatusAccumulator accumulator in cache.Inserted)
			{
				if ((int?)cache.GetValue(accumulator, nameof(INCostCenter.CostCenterID)) == costCenterID)
				{
					DeleteStatusRecord<TStatus, TStatusAccumulator>(cache, costCenterID, accumulator);

					_rowsToRestore.Add((accumulator, PXEntryStatus.Inserted));
					cache.Remove(accumulator);
				}
			}

			DeleteStatusRecord<TStatus, TStatusAccumulator>(cache, costCenterID, null);
		}

		protected virtual void DeleteStatusRecord<TStatus, TStatusAccumulator>(PXCache cache, int? costCenterID, TStatusAccumulator accumulator)
			where TStatus : class, IStatus, IBqlTable, new()
			where TStatusAccumulator : class, IStatus, IBqlTable, new()
		{
			const string QtyFieldTemplate = "qty";

			var conditions = new List<PXDataFieldRestrict>(cache.BqlFields.Count);

			foreach (var field in cache.Fields)
			{
				if (field.StartsWith(QtyFieldTemplate, StringComparison.OrdinalIgnoreCase) &&
					cache.GetAttributesReadonly(field).OfType<PXDBDecimalAttribute>().Any())
				{
					decimal? accumulatorValue = (accumulator != null) ? (decimal?)cache.GetValue(accumulator, field) : null;

					conditions.Add(new PXDataFieldRestrict(field, -1m * (accumulatorValue ?? 0m)));
				}
			}

			if (accumulator != null)
			{
				foreach (var key in cache.Keys)
				{
					conditions.Add(new PXDataFieldRestrict(key, cache.GetValue(accumulator, key)));
				}
			}
			else
			{
				conditions.Add(new PXDataFieldRestrict(nameof(INCostCenter.CostCenterID), costCenterID));
			}

			PXDatabase.Delete<TStatus>(conditions.ToArray());
		}

		protected virtual void OnBeforeCommitValidateDeletedCostCenter(int? costCenterID)
		{
			if (FindCostCenterData<INCostCenter>(costCenterID) ||
				FindCostCenterData<INItemPlan>(costCenterID) ||
				FindCostCenterData<INSiteStatusByCostCenter>(costCenterID) ||
				FindCostCenterData<INLocationStatusByCostCenter>(costCenterID) ||
				FindCostCenterData<INLotSerialStatusByCostCenter>(costCenterID))
			{
				throw new PXLockViolationException(typeof(INCostCenter), PXDBOperation.Delete, new object[] { costCenterID });
			}
		}

		protected virtual bool FindCostCenterData<TDac>(int? costCenterID)
			where TDac : class, IBqlTable, new()
		{
			return PXDatabase.SelectSingle<TDac>(
				new PXDataFieldValue(nameof(INCostCenter.CostCenterID), costCenterID),
				new PXDataField(nameof(INCostCenter.CostCenterID))) != null;
		}

		#endregion // Methods

		#region Overrides

		public override void Initialize()
		{
			base.Initialize();

			Base.OnBeforeCommit += (graph) => _deletedCostCenters?.ForEach(OnBeforeCommitValidateDeletedCostCenter);
			Base.Delete.SetDynamicText(true);
		}

		/// <summary>
		/// Overrides <see cref="SOOrderEntry.IsCuryUnitCostEnabled(SOLine, SOOrder)"/>
		/// </summary>
		[PXOverride]
		public virtual bool IsCuryUnitCostEnabled(SOLine line, SOOrder order, Func<SOLine, SOOrder, bool> baseMethod)
		{
			if (line?.IsSpecialOrder != true)
				return baseMethod(line, order);

			return (line.POCreated != true && line.Behavior != SOBehavior.TR && line.Operation == SOOperation.Issue);
		}

		/// <summary>
		/// Overrides <see cref="SOOrderEntry.IsCuryUnitCostVisible(SOOrder)"/>
		/// </summary>
		[PXOverride]
		public virtual bool IsCuryUnitCostVisible(SOOrder order, Func<SOOrder, bool> baseMethod)
		{ 
			return true;
		}

		/// <summary>
		/// Overrides <see cref="PurchaseSupplyBaseExt.SetPOCreateEnabled"/>
		/// </summary>
		[PXOverride]
		public virtual void SetPOCreateEnabled(PXCache cache, SOLine soline, bool poCreateEnabled,
			Action<PXCache, SOLine, bool> baseMethod)
		{
			baseMethod(cache, soline, poCreateEnabled && soline?.IsSpecialOrder != true);
		}

		/// <summary>
		/// Overrides <see cref="PurchaseSupplyBaseExt.ClearPOFieldsOnWarehouseChange"/>
		/// </summary>
		[PXOverride]
		public virtual void ClearPOFieldsOnWarehouseChange(PXCache cache, SOLine line,
			Action<PXCache, SOLine> baseMethod)
		{
			if (line.IsSpecialOrder != true)
				baseMethod(cache, line);
		}

		/// <summary>
		/// Overrides <see cref="PurchaseToSOLinkDialog.UnlinkSupply(SupplyPOLine, SOLine, IList{SOLineSplit}, bool)"/>
		/// </summary>
		[PXOverride]
		public virtual bool UnlinkSupply(SupplyPOLine supply, SOLine currentSOLine, IList<SOLineSplit> splits, bool deleted,
			Func<SupplyPOLine, SOLine, IList<SOLineSplit>, bool, bool> baseMethod)
		{
			bool removedLink = baseMethod(supply, currentSOLine, splits, deleted);

			if (supply.IsSpecialOrder == true)
			{
				supply.IsSpecialOrder = false;
				supply.CostCenterID = CostCenter.FreeStock;
				Base2.SupplyPOLines.Update(supply);

				if (supply.PlanID != null)
				{
					INItemPlan supplyPlan = SelectFrom<INItemPlan>
						.Where<INItemPlan.planID.IsEqual<INItemPlan.planID.AsOptional>>
						.View.Select(Base, supply.PlanID);

					if (supplyPlan == null)
						throw new Common.Exceptions.RowNotFoundException(Base.Caches<INItemPlan>(), supply.PlanID);

					supplyPlan.CostCenterID = CostCenter.FreeStock;
					Base.Caches[typeof(INItemPlan)].Update(supplyPlan);
				}
			}

			return removedLink;
		}

		/// <summary>
		/// Overrides <see cref="PurchaseToSOLinkDialog.LinkSupply(SupplyPOLine, SOLine, IList{SOLineSplit})"/>
		/// </summary>
		[PXOverride]
		public virtual bool LinkSupply(SupplyPOLine supply, SOLine currentSOLine, IList<SOLineSplit> splits,
			Func<SupplyPOLine, SOLine, IList<SOLineSplit>, bool> baseMethod)
		{
			if (currentSOLine.IsSpecialOrder == true)
			{
				supply.IsSpecialOrder = true;
				supply.CostCenterID = currentSOLine.CostCenterID;
				Base2.SupplyPOLines.Update(supply);

				currentSOLine = Base.Transactions.Locate(currentSOLine) ?? currentSOLine;
				if (currentSOLine.CuryUnitCost != supply.CuryUnitCost)
				{
					currentSOLine.CuryUnitCost = supply.CuryUnitCost;
					currentSOLine = Base.Transactions.Update(currentSOLine);
				}
			}

			return baseMethod(supply, currentSOLine, splits);
		}

		protected virtual bool HasPOWithDifferentCost(SOLine soline, SupplyPOLine newPOLine)
		{
			foreach (SupplyPOLine poline in Base2.SupplyPOLines.Select())
			{
				if (poline.Selected == true && poline.CuryUnitCost != newPOLine.CuryUnitCost)
					return true;
			}

			return false;
		}


		#endregion Overrides
	}
}
