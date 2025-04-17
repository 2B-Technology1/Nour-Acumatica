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
using PX.Data.BQL.Fluent;
using PX.Objects.Common.DAC;
using PX.Objects.CS;
using PX.Objects.IN;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.SO.GraphExtensions.SOOrderEntryExt
{
	public class PurchaseSupplyBaseExt : PXGraphExtension<SOOrderEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.sOToPOLink>()
				|| PXAccess.FeatureInstalled<FeaturesSet.dropShipments>()
				|| PXAccess.FeatureInstalled<FeaturesSet.purchaseRequisitions>();
		}

		[PXCopyPasteHiddenView()]
		public PXSelect<DropShipLink,
			Where<DropShipLink.sOOrderType, Equal<Required<SOLine.orderType>>,
				And<DropShipLink.sOOrderNbr, Equal<Required<SOLine.orderNbr>>,
				And<DropShipLink.sOLineNbr, Equal<Required<SOLine.lineNbr>>>>>> DropShipLinks;

		[PXCopyPasteHiddenView()]
		public PXSelect<SupplyPOOrder,
			Where<SupplyPOOrder.orderType, Equal<Required<SupplyPOOrder.orderType>>,
				And<SupplyPOOrder.orderNbr, Equal<Required<SupplyPOOrder.orderNbr>>>>> SupplyPOOrders;

		#region Events

		public virtual void _(Events.RowSelected<SOOrder> e)
		{
			if (e.Row == null)
				return;

			bool allowPOLink = Base.soordertype.Current.RequireShipping == true || Base.soordertype.Current.Behavior == SOBehavior.BL;
			PXUIFieldAttribute.SetVisible<SOLine.pOCreate>(Base.Transactions.Cache, null, allowPOLink);
			PXUIFieldAttribute.SetVisible<SOLineSplit.pOCreate>(Base.splits.Cache, null, allowPOLink);
			PXUIFieldAttribute.SetVisible<SOLine.pOSource>(Base.Transactions.Cache, null, allowPOLink);
			PXUIFieldAttribute.SetVisible<SOLineSplit.pOSource>(Base.splits.Cache, null, allowPOLink);
		}

		protected virtual void _(Events.RowSelected<SOLineSplit> e)
		{
			if (e.Row?.PONbr == null || e.Row.Completed == true)
				return;

			if (e.Row.POCancelled == true)
			{
				Base.splits.Cache.RaiseExceptionHandling<SOLineSplit.refNoteID>(e.Row, null,
					new PXSetPropertyException(Messages.LinkedPOLineIsCanceled, PXErrorLevel.RowWarning, e.Row.PONbr));
			}
			else if (e.Row.POCompleted == true)
			{
				Base.splits.Cache.RaiseExceptionHandling<SOLineSplit.refNoteID>(e.Row, null,
					new PXSetPropertyException(Messages.LinkedPOLineIsCompleted, PXErrorLevel.RowWarning, e.Row.PONbr));
			}
		}

		protected SOLinePrefetchedWarnings soLineWarnings;

		public virtual void _(Events.RowSelected<SOLine> e)
		{
			if (e.Row == null)
				return;

			POCreateVerifyValue(e.Cache, e.Row, e.Row.POCreate);
			bool poCreateEnabled = IsPOCreateEnabled(e.Row);

			if (e.Row.POSource == null)
			{
				PXUIFieldAttribute.SetEnabled<SOLine.pOCreate>(e.Cache, e.Row, poCreateEnabled && e.Row.Completed != true);
				PXUIFieldAttribute.SetEnabled<SOLine.pOSource>(e.Cache, e.Row, poCreateEnabled && e.Row.POCreate == true && !IsDropshipReturn(e.Row));
			}
			else if (IsPoToSoOrBlanket(e.Row.POSource) || e.Row.IsLegacyDropShip == true)
			{
				if (poCreateEnabled == false)
				{
					// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs [Leaving legacy approach for POtoSO and legacy drop-ships. Should be reworked on legacy drop-ship code deletion.]
					e.Row.POCreate = false;
					// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs [Leaving legacy approach for POtoSO and legacy drop-ships. Should be reworked on legacy drop-ship code deletion.]
					e.Row.POSource = null;
				}

				SetPOCreateEnabled(e.Cache, e.Row, poCreateEnabled);
				PXUIFieldAttribute.SetEnabled<SOLine.pOSource>(e.Cache, e.Row, poCreateEnabled && e.Row.POCreate == true && e.Row.POCreated != true && (e.Row.ShippedQty == 0m || e.Row.IsLegacyDropShip == true)
					&& Base.soordertype.Current.Behavior != SOBehavior.BL && e.Row.IsSpecialOrder != true);
			}
			else if (e.Row.POSource == INReplenishmentSource.DropShipToOrder) // && e.Row.IsLegacyDropShip != true
			{
				DropShipLink link = GetDropShipLink(e.Row);
				bool anyQtyReceived = link != null && link.BaseReceivedQty > 0m;
				PXUIFieldAttribute.SetEnabled<SOLine.pOCreate>(e.Cache, e.Row, poCreateEnabled && !anyQtyReceived);
				PXUIFieldAttribute.SetEnabled<SOLine.pOSource>(e.Cache, e.Row, poCreateEnabled && e.Row.POCreate == true && link == null && !IsDropshipReturn(e.Row));
			}

			soLineWarnings?.ShowLineWarning(Base.Transactions.Cache, e.Row);
		}

		protected virtual void SetPOCreateEnabled(PXCache cache, SOLine soline, bool poCreateEnabled)
		{
			PXUIFieldAttribute.SetEnabled<SOLine.pOCreate>(cache, soline, poCreateEnabled);
		}

		protected virtual void _(Events.RowPersisting<SOLine> e)
		{
			if (e.Operation.Command().IsNotIn(PXDBOperation.Insert, PXDBOperation.Update))
				return;

			PXDefaultAttribute.SetPersistingCheck<SOLine.pOSource>(e.Cache, e.Row, e.Row.POCreate == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
		}

		public virtual void _(Events.FieldVerifying<SOLine, SOLine.pOCreate> e)
		{
			if (e.Row == null)
				return;

			bool? newVal = (bool?)e.NewValue;
			POCreateVerifyValue(e.Cache, e.Row, newVal);
		}

		public virtual void _(Events.FieldDefaulting<SOLine, SOLine.pOCreate> e)
		{
			if (e.Row == null)
				return;

			if (!IsPOCreateEnabled(e.Row))
			{
				e.NewValue = false;
				e.Cancel = true;
				return;
			}

			if (e.Row.InventoryID != null && e.Row.SiteID != null)
			{
				bool dropShipmentsEnabled = PXAccess.FeatureInstalled<FeaturesSet.dropShipments>() && Base.soordertype.Current.Behavior != SOBehavior.BL;
				bool soToPOLinkEnabled = PXAccess.FeatureInstalled<FeaturesSet.sOToPOLink>();
				INItemSiteSettings itemSettings = Base.initemsettings.SelectSingle(e.Row.InventoryID, e.Row.SiteID);

				if (itemSettings.ReplenishmentSource == INReplenishmentSource.DropShipToOrder && dropShipmentsEnabled && !IsIssueWithARNoUpdate(e.Row)
					|| itemSettings.ReplenishmentSource == INReplenishmentSource.PurchaseToOrder && soToPOLinkEnabled)
				{
					e.NewValue = true;
					e.Cancel = true;
					return;
				}
			}
		}

		public virtual void _(Events.FieldDefaulting<SOLine, SOLine.pOSiteID> e)
		{
			if (e.Row == null || e.Row.POCreate != true)
				return;

			if (e.Row.POSource == INReplenishmentSource.DropShipToOrder && e.Row.IsLegacyDropShip != true
				|| e.Row.Behavior == SOBehavior.BL)
			{
				e.NewValue = e.Row.SiteID;
				e.Cancel = true;
			}
			else
			{
				INItemSiteSettings itemSettings = Base.initemsettings.SelectSingle(e.Row.InventoryID, e.Row.SiteID);

				object newVal = null;
				if (itemSettings != null && itemSettings.ReplenishmentSource.IsIn(INReplenishmentSource.Purchased,
					INReplenishmentSource.PurchaseToOrder, INReplenishmentSource.DropShipToOrder))
				{
					newVal = itemSettings.ReplenishmentSourceSiteID;
				}

				if (newVal == null)
				{
					newVal = e.Row.SiteID;
				}

				e.NewValue = newVal;
				e.Cancel = true;
			}
		}

		protected virtual void _(Events.FieldDefaulting<SOLine, SOLine.vendorID> e)
		{
			if (e.Row != null && e.Row.POCreate == true && Base.soordertype.Current.RequireLocation == false
				&& (e.Row.TranType != INDocType.Undefined && Base.soordertype.Current.RequireShipping == true
				|| Base.soordertype.Current.Behavior == SOBehavior.BL))
			{
				INItemSiteSettings itemSettings = Base.initemsettings.SelectSingle(e.Row.InventoryID, e.Row.SiteID);

				e.NewValue = itemSettings?.PreferredVendorID;
				e.Cancel = true;
			}
		}

		public virtual void _(Events.FieldDefaulting<SOLine, SOLine.pOSource> e)
		{
			if (e.Row == null)
				return;

			bool dropShipmentsEnabled = PXAccess.FeatureInstalled<FeaturesSet.dropShipments>() && Base.soordertype.Current.Behavior != SOBehavior.BL;
			bool soToPOLinkEnabled = PXAccess.FeatureInstalled<FeaturesSet.sOToPOLink>();

			if (e.Row.POCreate != true)
			{
				e.NewValue = null;
				e.Cancel = true;
				return;
			}

			InventoryItem item;
			if (dropShipmentsEnabled && (IsDropshipReturn(e.Row)
				|| !IsIssueWithARNoUpdate(e.Row) && (item = InventoryItem.PK.Find(Base, e.Row.InventoryID)) != null
				&& item.StkItem != true && item.NonStockReceipt == true && item.NonStockShip == true))
			{
				e.NewValue = INReplenishmentSource.DropShipToOrder;
				e.Cancel = true;
				return;
			}

			if (soToPOLinkEnabled && Base.soordertype.Current.Behavior == SOBehavior.BL)
			{
				e.NewValue = INReplenishmentSource.PurchaseToOrder;
				e.Cancel = true;
				return;
			}

			INItemSiteSettings itemSettings = Base.initemsettings.SelectSingle(e.Row.InventoryID, e.Row.SiteID);
			if (itemSettings?.POSource == INReplenishmentSource.PurchaseToOrder && soToPOLinkEnabled
				|| itemSettings?.POSource == INReplenishmentSource.DropShipToOrder && dropShipmentsEnabled && !IsIssueWithARNoUpdate(e.Row))
			{
				e.NewValue = itemSettings.POSource;
				e.Cancel = true;
				return;
			}
		}

		public virtual void _(Events.FieldUpdating<SOLine, SOLine.pOSource> e)
		{
			if (e.Row == null)
				return;

			if (e.Row.POCreate != true && e.Row.POSource != null)
			{
				e.Row.POSource = null;
			}
		}

		public virtual void _(Events.FieldVerifying<SOLine, SOLine.pOSource> e)
		{
			if (e.Row == null || e.NewValue == null)
				return;

			string newValue = (string)e.NewValue;
			InventoryItem item = InventoryItem.PK.Find(Base, e.Row.InventoryID);
			if (item != null && item.StkItem != true && newValue == INReplenishmentSource.DropShipToOrder)
			{
				if (item.NonStockReceipt != true || item.NonStockShip != true || e.Row.LineType == SOLineType.MiscCharge)
				{
					throw new PXSetPropertyException<SOLine.pOSource>(Messages.ReceiptShipmentRequiredForDropshipNonstock);
				}
			}

			if (IsIssueWithARNoUpdate(e.Row) && newValue.IsIn(INReplenishmentSource.DropShipToOrder, INReplenishmentSource.BlanketDropShipToOrder))
			{
				throw new PXSetPropertyException<SOLine.pOSource>(Messages.DropshipmentNotAllowedForOrderType, e.Row.OrderType);
			}

			if (newValue.IsIn(INReplenishmentSource.DropShipToOrder, INReplenishmentSource.BlanketDropShipToOrder)
				&& !Base.LineSplittingAllocatedExt.IsLotSerialsAllowedForDropShipLine(e.Row) && Base.LineSplittingAllocatedExt.HasMultipleSplitsOrAllocation(e.Row))
			{
				throw new PXSetPropertyException<SOLine.pOSource>(Messages.DropShipSOLineCantHaveMultipleSplitsOrAllocation);
			}
		}

		protected virtual void _(Events.FieldUpdated<SOLine, SOLine.tranType> e)
		{
			if (e.Row == null || (string)e.OldValue == (string)e.NewValue)
				return;

			e.Cache.SetDefaultExt<SOLine.pOCreate>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<SOLine, SOLine.operation> e)
		{
			if (e.Row == null)
				return;

			e.Cache.SetDefaultExt<SOLine.pOCreate>(e.Row);
			e.Cache.SetDefaultExt<SOLine.pOSource>(e.Row);
		}

		public virtual void _(Events.FieldUpdated<SOLine, SOLine.siteID> e)
		{
			if (e.Row == null)
				return;

			if (e.Row.POSource == INReplenishmentSource.DropShipToOrder && e.Row.IsLegacyDropShip != true)
			{
				DropShipLink link = GetDropShipLink(e.Row);
				if (link != null)
					return;
			}

			ClearPOFieldsOnWarehouseChange(e.Cache, e.Row);
		}

		protected virtual void ClearPOFieldsOnWarehouseChange(PXCache cache, SOLine line)
		{
			cache.SetDefaultExt<SOLine.pOCreate>(line);
			cache.SetDefaultExt<SOLine.pOSource>(line);
			cache.SetDefaultExt<SOLine.pOCreated>(line);
		}

		public virtual void _(Events.FieldUpdated<SOLine, SOLine.pOCreate> e)
		{
			if (e.Row.POCreated != true)
			{
				if (e.Row.POCreate == true)
				{
					e.Cache.SetDefaultExt<SOLine.pOSource>(e.Row);
					e.Cache.SetDefaultExt<SOLine.pOSiteID>(e.Row);
					e.Cache.SetDefaultExt<SOLine.vendorID>(e.Row);
				}
				else
				{
					e.Cache.SetValueExt<SOLine.pOSource>(e.Row, null);
					e.Cache.SetValueExt<SOLine.pOSiteID>(e.Row, null);
					e.Cache.SetValueExt<SOLine.vendorID>(e.Row, null);
				}
			}

			SOOrderLineSplittingAllocatedExtension.ResetAvailabilityCounters(e.Row);
		}

		public virtual void _(Events.FieldUpdated<SOLineSplit, SOLineSplit.pOCreate> e)
		{
			if (e.Row.POCreate == true)
			{
				e.Cache.SetDefaultExt<SOLineSplit.pOSource>(e.Row);
			}
			else
			{
				e.Cache.SetValueExt<SOLineSplit.pOSource>(e.Row, null);
			}
		}

		public virtual void _(Events.FieldDefaulting<SOLineSplit, SOLineSplit.pOSource> e)
		{
			if (e.Row != null && e.Row.POCreate != true)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		#endregion Events

		public virtual void POCreateVerifyValue(PXCache sender, SOLine row, bool? value)
		{
			if (row == null || row.InventoryID == null || value != true)
				return;

			InventoryItem item = InventoryItem.PK.Find(Base, row.InventoryID);
			if (item != null && item.StkItem != true)
			{
				if (item.KitItem == true)
				{
					sender.RaiseExceptionHandling<SOLine.pOCreate>(row, value, new PXSetPropertyException(Messages.SOPOLinkNotForNonStockKit, PXErrorLevel.Error));
				}
				else if ((item.NonStockShip != true || item.NonStockReceipt != true) && PXAccess.FeatureInstalled<FeaturesSet.sOToPOLink>())
				{
					sender.RaiseExceptionHandling<SOLine.pOCreate>(row, value, new PXSetPropertyException(Messages.NonStockShipReceiptIsOff, PXErrorLevel.Warning));
				}
			}
		}

		public virtual bool IsPOCreateEnabled(SOLine row)
			=> (Base.soordertype.Current.RequireShipping == true && row.TranType != INDocType.Undefined || Base.soordertype.Current.Behavior == SOBehavior.BL)
			&& (Base.soordertype.Current.ARDocType != AR.ARDocType.NoUpdate || PXAccess.FeatureInstalled<FeaturesSet.sOToPOLink>())
			&& (row.Operation == SOOperation.Issue || IsDropshipReturn(row) && PXAccess.FeatureInstalled<FeaturesSet.dropShipments>());

		public virtual bool IsIssueWithARNoUpdate(SOLine row) => row.Operation == SOOperation.Issue && Base.soordertype.Current.ARDocType == AR.ARDocType.NoUpdate;

		public virtual bool IsDropshipReturn(SOLine row) => row.Operation == SOOperation.Receipt && row.Behavior == SOBehavior.RM
			&& row.OrigShipmentType == SOShipmentType.DropShip && Base.soordertype.Current.ARDocType != AR.ARDocType.NoUpdate;

		public virtual bool IsPoToSoOrBlanket(string poSource) => poSource != null && poSource.IsIn(INReplenishmentSource.PurchaseToOrder,
			INReplenishmentSource.BlanketDropShipToOrder, INReplenishmentSource.BlanketPurchaseToOrder);

		public virtual bool IsOriginalDSLinkVisible(SOLine row) => row?.POCreate == true && row.Operation == SOOperation.Receipt
			&& row.OrigShipmentType == SOShipmentType.DropShip;

		#region LSSOLine

		public virtual void FillInsertingSchedule(PXCache sender, SOLineSplit split)
		{
			SOLine soLine = split != null ? PXParentAttribute.SelectParent<SOLine>(sender, split) : null;
			if (split == null || split.POLineNbr != null || soLine == null
				|| soLine.POSource != INReplenishmentSource.DropShipToOrder || soLine.IsLegacyDropShip == true)
				return;

			DropShipLink link = GetDropShipLink(soLine);
			if (link == null)
				return;

			split.POType = link.POOrderType;
			split.PONbr = link.POOrderNbr;
			split.POLineNbr = link.POLineNbr;
		}

		#endregion

		protected HashSet<string> prefetched = new HashSet<string>();

		[PXOverride]
		public virtual void PrefetchWithDetails()
		{
			if (Base.Document.Current == null || DropShipLinks.Cache.IsDirty
				|| prefetched.Contains(Base.Document.Current.OrderType + Base.Document.Current.OrderNbr))
				return;

			PrefetchWarnings();

			SOOrderTypeOperation receiptOperation = PXSelect<SOOrderTypeOperation,
				Where<SOOrderTypeOperation.orderType, Equal<Current<SOOrderType.orderType>>,
					And<SOOrderTypeOperation.operation, Equal<SOOperation.receipt>,
					And<SOOrderTypeOperation.active, Equal<True>>>>>
				.Select(Base);
			if (receiptOperation != null)
			{
				var receiptDetailsWithLinksQuery = new PXSelectReadonly2<SOLine,
					LeftJoin<DropShipLink,
						On<DropShipLink.sOOrderType, Equal<SOLine.origOrderType>,
							And<DropShipLink.sOOrderNbr, Equal<SOLine.origOrderNbr>,
							And<DropShipLink.sOLineNbr, Equal<SOLine.origLineNbr>>>>,
					LeftJoin<SupplyPOOrder, On<DropShipLink.FK.SupplyPOOrder>>>,
					Where<SOLine.orderType, Equal<Current<SOOrder.orderType>>,
						And<SOLine.orderNbr, Equal<Current<SOOrder.orderNbr>>,
						And<SOLine.operation, Equal<SOOperation.receipt>,
						And<SOLine.origShipmentType, Equal<SOShipmentType.dropShip>>>>>>(Base);
				DoPrefetch(receiptDetailsWithLinksQuery);
			}

			var issueDetailsWithLinksQuery = new PXSelectReadonly2<SOLine,
				LeftJoin<DropShipLink, On<DropShipLink.FK.SOLine>,
				LeftJoin<SupplyPOOrder, On<DropShipLink.FK.SupplyPOOrder>>>,
				Where<SOLine.orderType, Equal<Current<SOOrder.orderType>>,
					And<SOLine.orderNbr, Equal<Current<SOOrder.orderNbr>>,
					And<SOLine.operation, Equal<SOOperation.issue>>>>>(Base);
			DoPrefetch(issueDetailsWithLinksQuery);

			prefetched.Add(Base.Document.Current.OrderType + Base.Document.Current.OrderNbr);
		}

		protected virtual void DoPrefetch(PXSelectBase<SOLine> detailsWithLinksQuery)
		{
			var fieldsAndTables = new[]
			{
				typeof(SOLine.orderType), typeof(SOLine.orderNbr), typeof(SOLine.lineNbr), typeof(DropShipLink), typeof(SupplyPOOrder)
			};
			using (new PXFieldScope(detailsWithLinksQuery.View, fieldsAndTables))
			{
				int startRow = PXView.StartRow;
				int totalRows = 0;
				foreach (PXResult<SOLine, DropShipLink, SupplyPOOrder> record in detailsWithLinksQuery.View.Select(
					PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns,
					PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows))
				{
					SOLine line = record;
					DropShipLink link = record;
					SupplyPOOrder supplyOrder = record;

					DropShipLinkStoreCached(link, line);

					if (supplyOrder?.OrderNbr != null)
					{
						SupplyOrderStoreCached(supplyOrder);
					}
				}
			}
		}

		public virtual void DropShipLinkStoreCached(DropShipLink link, SOLine line)
		{
			var list = new List<object>(1);

			if (link?.SOOrderType != null)
				list.Add(link);

			DropShipLinks.StoreResult(list, PXQueryParameters.ExplicitParameters(line.OrderType, line.OrderNbr, line.LineNbr));
		}

		public virtual DropShipLink GetDropShipLink(SOLine line)
		{
			if (line == null || line.POSource != INReplenishmentSource.DropShipToOrder || line.IsLegacyDropShip == true)
				return null;

			return DropShipLinks.SelectWindowed(0, 1, line.OrderType, line.OrderNbr, line.LineNbr);
		}

		public virtual DropShipLink GetOriginalDropShipLink(SOLine line)
		{
			if (line == null)
				return null;

			return DropShipLinks.SelectWindowed(0, 1, line.OrigOrderType, line.OrigOrderNbr, line.OrigLineNbr);
		}

		public virtual void SupplyOrderStoreCached(SupplyPOOrder order)
		{
			SupplyPOOrders.StoreResult(order);
		}

		public virtual SupplyPOOrder GetSupplyOrder(SOLine line)
		{
			if (line == null || line.POSource != INReplenishmentSource.DropShipToOrder || line.IsLegacyDropShip == true)
				return null;

			DropShipLink link = GetDropShipLink(line);
			return GetSupplyOrder(link);
		}

		public virtual SupplyPOOrder GetSupplyOrder(DropShipLink link)
		{
			if (link == null)
				return null;

			return SupplyPOOrders.SelectWindowed(0, 1, link.POOrderType, link.POOrderNbr);
		}

		public virtual SupplyPOOrder GetOriginalSupplyOrder(SOLine line)
		{
			if (line == null || line.POSource != INReplenishmentSource.DropShipToOrder || line.IsLegacyDropShip == true)
				return null;

			DropShipLink link = GetOriginalDropShipLink(line);
			if (link == null)
				return null;

			return SupplyPOOrders.SelectWindowed(0, 1, link.POOrderType, link.POOrderNbr);
		}

		protected virtual void PrefetchWarnings()
		{
			if (soLineWarnings == null)
				soLineWarnings = new SOLinePrefetchedWarnings();

			var splitsWithFlagsView = new SelectFrom<SOLineSplit>
				.Where<SOLineSplit.orderType.IsEqual<SOOrder.orderType.FromCurrent>
					.And<SOLineSplit.orderNbr.IsEqual<SOOrder.orderNbr.FromCurrent>>
					.And<SOLineSplit.pOCreate.IsEqual<True>>
					.And<SOLineSplit.pONbr.IsNotNull>
					.And<SOLineSplit.completed.IsNotEqual<True>>
					.And<SOLineSplit.pOCancelled.IsEqual<True>.Or<SOLineSplit.pOCompleted.IsEqual<True>>>>
				.OrderBy<Asc<SOLineSplit.orderType>, Asc<SOLineSplit.orderNbr>, Asc<SOLineSplit.lineNbr>>
				.View.ReadOnly(Base);

			var requiredFields = new[]
			{
				typeof(SOLineSplit.orderType), typeof(SOLineSplit.orderNbr), typeof(SOLineSplit.lineNbr),
				typeof(SOLineSplit.splitLineNbr), typeof(SOLineSplit.pONbr),
				typeof(SOLineSplit.pOCancelled), typeof(SOLineSplit.pOCompleted)
			};
			using (new PXFieldScope(splitsWithFlagsView.View, requiredFields))
			{
				var splitsWithFlags = splitsWithFlagsView.View.SelectMultiBound(new[] { Base.Document.Current }).RowCast<SOLineSplit>();

				SOLineSplit prevSplit = null;
				foreach (SOLineSplit split in splitsWithFlags)
				{
					if (prevSplit != null && prevSplit.LineNbr == split.LineNbr)
						continue; // Show the first warning only.

					Exception warning = new PXSetPropertyException(
						split.POCancelled == true ? Messages.LinkedPOLineIsCanceled : Messages.LinkedPOLineIsCompleted,
						PXErrorLevel.RowWarning,
						split.PONbr);

					var line = new SOLine { OrderType = split.OrderType, OrderNbr = split.OrderNbr, LineNbr = split.LineNbr };
					soLineWarnings.PrefetchLineWarning(line, warning);

					prevSplit = split;
				}
			}
		}

		public sealed class SOLinePrefetchedWarnings
		{
			private Dictionary<(string, string, int?), Exception> prefetched = new Dictionary<(string, string, int?), Exception>();

			public void PrefetchLineWarning(SOLine line, Exception warning)
			{
				prefetched[GetKey(line)] = warning;
			}

			public void ShowLineWarning(PXCache cache, SOLine line)
			{
				var key = GetKey(line);
				if (!prefetched.TryGetValue(key, out Exception exception))
					return;

				cache.RaiseExceptionHandling<SOLine.lineNbr>(line, null, exception);
			}

			private (string, string, int?)  GetKey(SOLine line) => (line.OrderType, line.OrderNbr, line.LineNbr);
		}
	}
}
