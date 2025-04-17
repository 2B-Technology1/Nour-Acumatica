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

using PX.Api;
using PX.Api.ContractBased;
using PX.Api.ContractBased.Models;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.IN;
using PX.Objects.SO;
using PX.Objects.PO.DAC.Projections;
using System.Linq;
using System;
using PX.Common;
using PX.Objects.CS;
using System.Diagnostics;
using System.Collections.Generic;
using PX.Objects.SO.DAC.Projections;
using PX.Objects.SO.GraphExtensions.SOOrderEntryExt;

namespace PX.Objects.EndpointAdapters
{
	[PXInternalUseOnly]
	[PXVersion("22.200.001", "Default")]
	public class DefaultEndpointImpl22 : DefaultEndpointImpl20
	{
		[FieldsProcessed(new string[0])]
		protected virtual void InventoryIssueDetail_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			var issueEntry = (INIssueEntry)graph;
			if (issueEntry.issue.Current != null)
			{
				var detailsCache = issueEntry.transactions.Cache;
				detailsCache.Current = detailsCache.Insert();
				var detailCurrent = detailsCache.Current as INTran;

				if (detailsCache.Current == null)
					throw new InvalidOperationException("Cannot insert Inventory Issue detail.");

				var allocations = (targetEntity.Fields.SingleOrDefault(f => string.Equals(f.Name, "Allocations")) as EntityListField)?.Value ?? new EntityImpl[0];
				bool hasAllocations = allocations.Any(a => a.Fields != null && a.Fields.Length > 0);

				if (hasAllocations)
				{
					var InventoryIDField = targetEntity.Fields.SingleOrDefault(f => f.Name == "InventoryID") as EntityValueField;
					var SiteField = targetEntity.Fields.SingleOrDefault(f => f.Name == "WarehouseID") as EntityValueField;
					var LocationField = targetEntity.Fields.SingleOrDefault(f => f.Name == "Location") as EntityValueField;
					var SubItemField = targetEntity.Fields.FirstOrDefault(f => f.Name == "Subitem") as EntityValueField;

					if (InventoryIDField != null)
						issueEntry.transactions.Cache.SetValueExt(detailCurrent, "InventoryID", InventoryIDField.Value);
					if (SiteField != null)
						issueEntry.transactions.Cache.SetValueExt(detailCurrent, "SiteID", SiteField.Value);
					if (LocationField != null)
						issueEntry.transactions.Cache.SetValueExt(detailCurrent, "LocationID", LocationField.Value);
					if (SubItemField != null)
						issueEntry.transactions.Cache.SetValueExt(detailCurrent, "SubItemID", SubItemField.Value);

					var QtyField = targetEntity.Fields.FirstOrDefault(f => f.Name == "Qty") as EntityValueField;
					if (QtyField != null)
					{
						detailCurrent.Qty = decimal.Parse(QtyField.Value);
						detailCurrent = issueEntry.transactions.Update(detailCurrent);
					}

					//All the created splits will be deleted. New splits will be inserted later.
					if (detailsCache.Current != null)
					{
						var inserted = issueEntry.splits.Cache.Inserted;
						foreach (INTranSplit split in inserted)
						{
							if (split.LineNbr == (detailsCache.Current as INTran).LineNbr)
								issueEntry.splits.Delete(split);
						}
					}
				}
			}
		}

		[FieldsProcessed(new[] {
			"InvoiceType",
			"InvoiceNbr",
			"InvoiceLineNbr"
		})]
		protected override void SalesOrderDetail_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			var orderEntry = (SOOrderEntry)graph;

			var InvoiceTypeField = targetEntity.Fields.SingleOrDefault(f => f.Name == "InvoiceType") as EntityValueField;
			var InvoiceNbrField = targetEntity.Fields.SingleOrDefault(f => f.Name == "InvoiceNbr") as EntityValueField;

			var purchasingDetailsEntities = (targetEntity.Fields.SingleOrDefault(f => string.Equals(f.Name, "PurchasingDetails")) as EntityListField)?.Value ?? new EntityImpl[0];
			bool hasPurchasingDetails = purchasingDetailsEntities.Any(a => a.Fields != null && a.Fields.Length > 0);

			if (InvoiceTypeField != null && InvoiceNbrField != null) //Create return orders only
			{
				var filterCache = orderEntry.addinvoicefilter.Cache;
				var filter = (AddInvoiceFilter)filterCache.Current;

				string oldRefNbr = filter.RefNbr;
				filterCache.SetValueExt<AddInvoiceFilter.docType>(filter, InvoiceTypeField.Value);
				filter.RefNbr = InvoiceNbrField.Value;
				filterCache.RaiseFieldUpdated<AddSOFilter.orderNbr>(filter, oldRefNbr);
				filterCache.Update(filter);

				var InvoiceLineNbrField = targetEntity.Fields.SingleOrDefault(f => f.Name == "InvoiceLineNbr") as EntityValueField;
				int? invoiceLineNbr = !string.IsNullOrEmpty(InvoiceLineNbrField?.Value) ? (int?)int.Parse(InvoiceLineNbrField.Value) : null;

				foreach (InvoiceSplits invoiceSplit in orderEntry.invoicesplits.Select())
				{
					if (invoiceLineNbr != null)
					{
						if (invoiceSplit.LineNbrARTran == invoiceLineNbr)
						{
							invoiceSplit.Selected = true;
							orderEntry.invoicesplits.Update(invoiceSplit);
							break;
						}
						else
							continue;
					}
					else
					{
						invoiceSplit.Selected = true;
						orderEntry.invoicesplits.Update(invoiceSplit);
					}
				}
				orderEntry.addInvoiceOK.Press();
			}
			else if (hasPurchasingDetails) //Create SO line and link SO and PO
			{
				var detailsCache = orderEntry.Transactions.Cache;
				detailsCache.Current = detailsCache.Insert();
				var transaction = detailsCache.Current as SOLine;

				LinkSOandPO(orderEntry, transaction, targetEntity, purchasingDetailsEntities);
			}
			else
			{
				base.SalesOrderDetail_Insert(graph, entity, targetEntity);
			}
		}

		[FieldsProcessed(new string[0])]
		protected virtual void SalesOrderDetail_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			var purchasingDetailsEntities = (targetEntity.Fields.SingleOrDefault(f => string.Equals(f.Name, "PurchasingDetails")) as EntityListField)?.Value ?? new EntityImpl[0];
			bool hasPurchasingDetails = purchasingDetailsEntities.Any(a => a.Fields != null && a.Fields.Length > 0);

			if (hasPurchasingDetails)
			{
				var soOrderEntry = (SOOrderEntry)graph;
				var transaction = soOrderEntry.Transactions.Current;

				LinkSOandPO(soOrderEntry, transaction, targetEntity, purchasingDetailsEntities);
			}
		}

		protected virtual void LinkSOandPO(SOOrderEntry soOrderEntry, SOLine transaction, EntityImpl targetEntity, EntityImpl[] purchasingDetailsEntities)
		{
			if (transaction == null)
				throw new InvalidOperationException("Cannot insert Sales Order detail.");

			SetFieldsNeedToInsertLinkPO(targetEntity, soOrderEntry, transaction);

			POLinkDialog pOLinkDialog = soOrderEntry.GetExtension<POLinkDialog>();

			foreach (var purchasingDetailsEntity in purchasingDetailsEntities)
			{
				string orderType = (purchasingDetailsEntity.Fields.SingleOrDefault(f => f.Name == "POOrderType") as EntityValueField)?.Value;
				string orderNbr = (purchasingDetailsEntity.Fields.SingleOrDefault(f => f.Name == "POOrderNbr") as EntityValueField)?.Value;
				var orderLineNbrField = purchasingDetailsEntity.Fields.SingleOrDefault(f => f.Name == "POOrderLineNbr") as EntityValueField;
				int? orderLineNbr = !string.IsNullOrEmpty(orderLineNbrField?.Value) ? (int?)int.Parse(orderLineNbrField.Value) : null;
				var selectedField = purchasingDetailsEntity.Fields.SingleOrDefault(f => f.Name == "Selected") as EntityValueField;
				bool? selected = !string.IsNullOrEmpty(selectedField?.Value) ? (bool?)bool.Parse(selectedField.Value) : null;

				var isPOLineFound = false;

				pOLinkDialog.SOLineDemand.View.Cache.ClearQueryCache();

				foreach (SupplyPOLine supply in pOLinkDialog.SupplyPOLines.Select())
				{
					bool orderTypesEqual = supply.OrderType == orderType;
					if (!orderTypesEqual)
					{
						var state = pOLinkDialog.SupplyPOLines.Cache.GetStateExt<SupplyPOLine.orderType>(supply) as PXStringState;
						if (state != null && state.ValueLabelDic != null)
						{
							foreach (var rec in state.ValueLabelDic)
							{
								if (rec.Key == supply.OrderType && rec.Value == orderType)
								{
									orderTypesEqual = true;
									break;
								}
							}
						}
					}

					if (orderTypesEqual && supply.OrderNbr == orderNbr && supply.LineNbr == orderLineNbr)
					{
						supply.Selected = selected;
						pOLinkDialog.SupplyPOLines.Update(supply);
						isPOLineFound = true;
					}
				}

				if (!isPOLineFound)
				{
					throw new Exception($"Purchase Order Line (Order Type = {orderType}, OrderNbr = {orderNbr}, LineNbr = {orderLineNbr}) cannot be found in Purchasing Details for Sales Order line (LineNbr = {transaction.LineNbr}).");
				}
			}
			pOLinkDialog.SOLineDemand.View.Answer = WebDialogResult.OK;
			pOLinkDialog.pOSupplyOK.Press();
		}

		protected virtual void SetFieldsNeedToInsertLinkPO(EntityImpl targetEntity, SOOrderEntry soOrderEntry, SOLine soLine)
		{
			var markForPOOrig = soLine.POCreate;
			var pOSourceOrig = soLine.POSource;

			var InventoryIDField = targetEntity.Fields.SingleOrDefault(f => f.Name == "InventoryID") as EntityValueField;
			var SubItemField = targetEntity.Fields.FirstOrDefault(f => f.Name == "Subitem") as EntityValueField;
			var WarehouseIDField = targetEntity.Fields.SingleOrDefault(f => f.Name == "WarehouseID") as EntityValueField;
			var OrderQtyField = targetEntity.Fields.SingleOrDefault(f => f.Name == "OrderQty") as EntityValueField;
			decimal? orderQty = !string.IsNullOrEmpty(OrderQtyField?.Value) ? (decimal?)decimal.Parse(OrderQtyField.Value) : null;
			var UOMField = targetEntity.Fields.SingleOrDefault(f => f.Name == "UOM") as EntityValueField;

			var MarkForPOField = targetEntity.Fields.SingleOrDefault(f => f.Name == "MarkForPO") as EntityValueField;
			bool? markForPONew = !string.IsNullOrEmpty(MarkForPOField?.Value) ? (bool?)bool.Parse(MarkForPOField.Value) : null;
			bool? markForPOValue = (markForPONew == null) ? markForPOOrig : markForPONew;

			var POSourceParam = targetEntity.Fields.SingleOrDefault(f => f.Name == "POSource") as EntityValueField;

			var PurchaseWarehouseField = targetEntity.Fields.SingleOrDefault(f => f.Name == "PurchaseWarehouse") as EntityValueField;
			var VendorIDField = targetEntity.Fields.SingleOrDefault(f => f.Name == "VendorID") as EntityValueField;

			string pOSourceValue;
			if (POSourceParam == null)
			{
				pOSourceValue = pOSourceOrig;
			}
			else
			{
				var state = soOrderEntry.Transactions.Cache.GetStateExt(soLine, "POSource") as PXStringState;
				if (state == null)
					throw new Exception($"Cannot get labels for PO Source for Sales Order line (LineNbr = {soLine.LineNbr}).");

				if (state.AllowedLabels.Contains(POSourceParam.Value))
					pOSourceValue = state.ValueLabelDic.Single(p => p.Value == POSourceParam.Value).Key;
				else
					throw new Exception($"PO Source = {POSourceParam.Value} is not found in the system for Sales Order line (LineNbr = {soLine.LineNbr}).");

			}

			var isDropShip = IsDropShip(pOSourceValue);

			if (InventoryIDField != null)
				soOrderEntry.Transactions.Cache.SetValueExt(soLine, "InventoryID", InventoryIDField.Value);
			if (SubItemField != null)
				soOrderEntry.Transactions.Cache.SetValueExt(soLine, "SubItemID", SubItemField.Value);
			if (WarehouseIDField != null)
				soOrderEntry.Transactions.Cache.SetValueExt(soLine, "SiteID", WarehouseIDField.Value);
			if (OrderQtyField != null)
				soOrderEntry.Transactions.Cache.SetValueExt(soLine, "OrderQty", orderQty);
			if (UOMField != null)
				soOrderEntry.Transactions.Cache.SetValueExt(soLine, "UOM", UOMField.Value);
			if (MarkForPOField != null)
				soOrderEntry.Transactions.Cache.SetValueExt(soLine, "POCreate", markForPOValue);
			if (pOSourceValue != null)
				soOrderEntry.Transactions.Cache.SetValueExt(soLine, "POSource", pOSourceValue);
			if (!isDropShip && PurchaseWarehouseField != null)
				soOrderEntry.Transactions.Cache.SetValueExt(soLine, "POSiteID", PurchaseWarehouseField.Value);

			soLine = soOrderEntry.Transactions.Update(soLine);

			if (VendorIDField != null)
				soOrderEntry.Transactions.Cache.SetValueExt(soLine, "VendorID", VendorIDField?.Value);
		}

		protected virtual bool IsDropShip(string POSourceValue)
		{
			if (POSourceValue == null) return false;

			return POSourceValue == PX.Objects.IN.INReplenishmentSource.DropShipToOrder;
		}

		[FieldsProcessed(new[] {
			"ShipmentNbr",
			"OrderNbr",
			"OrderType",
			"OrigInvType",
			"OrigInvNbr",
			"OrigInvLineNbr"
		})]
		protected override void SalesInvoiceDetail_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			var invoiceEntry = (SOInvoiceEntry)graph;

			var OrigInvoiceTypeField = targetEntity.Fields.SingleOrDefault(f => f.Name == "OrigInvType") as EntityValueField;
			var OrigInvoiceNbrField = targetEntity.Fields.SingleOrDefault(f => f.Name == "OrigInvNbr") as EntityValueField;
			var OrigInvoiceLineNbrField = targetEntity.Fields.SingleOrDefault(f => f.Name == "OrigInvLineNbr") as EntityValueField;

			if (OrigInvoiceTypeField != null && OrigInvoiceNbrField != null)
			{
				int? origInvoiceLineNbr = !string.IsNullOrEmpty(OrigInvoiceLineNbrField?.Value) ? (int?)int.Parse(OrigInvoiceLineNbrField.Value) : null;

				SO.GraphExtensions.SOInvoiceEntryExt.AddReturnLineToDirectInvoice returnLineExtension = invoiceEntry.GetExtension<SO.GraphExtensions.SOInvoiceEntryExt.AddReturnLineToDirectInvoice>();
				if (returnLineExtension != null)
				{
					string invoiceType = OrigInvoiceTypeField.Value;
					var state = returnLineExtension.arTranList.Cache.GetStateExt<ARTranForDirectInvoice.tranType>(new ARTranForDirectInvoice { }) as PXStringState;

					if (state != null && state.AllowedLabels.Contains(OrigInvoiceTypeField.Value))
					{
						invoiceType = state.ValueLabelDic.Single(p => p.Value == OrigInvoiceTypeField.Value).Key;
					}

					foreach (ARTranForDirectInvoice invoiceLine in returnLineExtension.arTranList.Select())
					{
						if (invoiceLine.TranType == invoiceType && invoiceLine.RefNbr == OrigInvoiceNbrField.Value)
						{
							if (origInvoiceLineNbr != null)
							{
								if (invoiceLine.LineNbr == origInvoiceLineNbr)
								{
									invoiceLine.Selected = true;
									returnLineExtension.arTranList.Update(invoiceLine);
									break;
								}
								else
									continue;
							}
							else
							{
								invoiceLine.Selected = true;
								returnLineExtension.arTranList.Update(invoiceLine);
							}
						}
					}
					returnLineExtension.addARTran.Press();
				}
			}
			else
			{
				base.SalesInvoiceDetail_Insert(graph, entity, targetEntity);
			}
		}

		[FieldsProcessed(new[] {
			"ShipmentSplitLineNbr",
			"OrigOrderType",
			"OrigOrderNbr",
			"InventoryID",
			"Subitem",
			"LotSerialNbr"
		})]
		protected virtual void ShipmentPackageDetail_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			var shipmentEntry = (SOShipmentEntry)graph;

			SOShipmentEntry.PackageDetail packageDetailExtension = shipmentEntry.GetExtension<SOShipmentEntry.PackageDetail>();
			if (packageDetailExtension != null)
			{
				SOShipLineSplitPackage packageSplit = packageDetailExtension.PackageDetailSplit.Insert(new SOShipLineSplitPackage());

				var ShipmentSplitLineNbrField = targetEntity.Fields.SingleOrDefault(f => f.Name == "ShipmentSplitLineNbr") as EntityValueField;
				int? shipmentSplitLineNbr = ConvertToNullableInt(ShipmentSplitLineNbrField);

				var OrigOrderTypeField = targetEntity.Fields.SingleOrDefault(f => f.Name == "OrigOrderType") as EntityValueField;
				var OrigOrderNbrField = targetEntity.Fields.SingleOrDefault(f => f.Name == "OrigOrderNbr") as EntityValueField;
				var InventoryIDField = targetEntity.Fields.SingleOrDefault(f => f.Name == "InventoryID") as EntityValueField;
				var SubitemField = targetEntity.Fields.SingleOrDefault(f => f.Name == "Subitem") as EntityValueField;
				var LotSerialNbrField = targetEntity.Fields.SingleOrDefault(f => f.Name == "LotSerialNbr") as EntityValueField;
				var QuantityField = targetEntity.Fields.SingleOrDefault(f => f.Name == "Quantity") as EntityValueField;
				decimal? quantity = ConvertToNullableDecimal(QuantityField);

				if (!string.IsNullOrWhiteSpace(InventoryIDField?.Value) || shipmentSplitLineNbr != null)
				{
					List<SOShipLineSplit> applicableShipLineSplits = new List<SOShipLineSplit>();

					var shipmentSplits = PXSelect<SOShipLineSplit,
						Where<SOShipLineSplit.shipmentNbr, Equal<Required<SOShipLineSplit.shipmentNbr>>>>.Select(graph, shipmentEntry.Document.Current?.ShipmentNbr);
					foreach (SOShipLineSplit shipLineSplit in shipmentSplits)
					{
						if (shipmentEntry.splits.Cache.GetStatus(shipLineSplit).IsNotIn(PXEntryStatus.Deleted, PXEntryStatus.InsertedDeleted))
						{
							if (shipmentSplitLineNbr != null)
							{
								if (shipmentSplitLineNbr == shipLineSplit.SplitLineNbr)
								{
									applicableShipLineSplits.Add(shipLineSplit);
									break;
								}
							}
							else
							{
								var itemValueExt = shipmentEntry.splits.Cache.GetValueExt<SOShipLineSplit.inventoryID>(shipLineSplit);
								var inventoryCD = PXFieldState.UnwrapValue(itemValueExt);

								if (inventoryCD == null)
								{
									continue;
								}

								if (inventoryCD.ToString().Trim() == InventoryIDField.Value.Trim())
								{
									applicableShipLineSplits.Add(shipLineSplit);
								}
								else
								{
									continue;
								}

								if (!string.IsNullOrWhiteSpace(SubitemField?.Value))
								{
									var subitemValueExt = shipmentEntry.splits.Cache.GetValueExt<SOShipLineSplit.subItemID>(shipLineSplit);
									var subitemCD = PXFieldState.UnwrapValue(subitemValueExt);

									if (subitemCD != null && subitemCD.ToString().Trim() != SubitemField.Value.Trim())
									{
										applicableShipLineSplits.Remove(shipLineSplit);
										continue;
									}
								}

								if (!string.Equals(LotSerialNbrField?.Value, shipLineSplit.LotSerialNbr, StringComparison.OrdinalIgnoreCase))
								{
									applicableShipLineSplits.Remove(shipLineSplit);
									continue;
								}

								if (OrigOrderTypeField?.Value != shipLineSplit.OrigOrderType)
								{
									applicableShipLineSplits.Remove(shipLineSplit);
									continue;
								}

								if (OrigOrderNbrField?.Value != shipLineSplit.OrigOrderNbr)
								{
									applicableShipLineSplits.Remove(shipLineSplit);
									continue;
								}

								if (quantity != null && quantity > shipLineSplit.Qty)
								{
									applicableShipLineSplits.Remove(shipLineSplit);
									continue;
								}
							}
						}
					}

					if (applicableShipLineSplits.Count > 0)
					{
						SOShipLineSplit splitToSelect = applicableShipLineSplits.First();
						SOShipLineSplitPackage.SOShipLineSplitNbrVisible sOShipLineSplitNbrVisible = new SOShipLineSplitPackage.SOShipLineSplitNbrVisible();
						sOShipLineSplitNbrVisible.ShipmentNbr = splitToSelect.ShipmentNbr;
						sOShipLineSplitNbrVisible.OrigOrderType = splitToSelect.OrigOrderType;
						sOShipLineSplitNbrVisible.OrigOrderNbr = splitToSelect.OrigOrderNbr;
						sOShipLineSplitNbrVisible.InventoryID = splitToSelect.InventoryID;
						sOShipLineSplitNbrVisible.UOM = splitToSelect.UOM;
						sOShipLineSplitNbrVisible.Qty = splitToSelect.Qty;
						sOShipLineSplitNbrVisible.SplitLineNbr = splitToSelect.SplitLineNbr;
						sOShipLineSplitNbrVisible.LineNbr = splitToSelect.LineNbr;
						sOShipLineSplitNbrVisible.LotSerialNbr = splitToSelect.LotSerialNbr;

						sOShipLineSplitNbrVisible = (SOShipLineSplitPackage.SOShipLineSplitNbrVisible)shipmentEntry.Caches[typeof(SOShipLineSplitPackage.SOShipLineSplitNbrVisible)].Insert(sOShipLineSplitNbrVisible);
						packageSplit.ShipmentSplitLineNbr = splitToSelect.SplitLineNbr;

						packageDetailExtension.PackageDetailSplit.Cache.Update(packageSplit);
					}
					else
					{
						throw new PXException($"No suitable items found to pack (ShipmentSplitLineNbr: {ShipmentSplitLineNbrField?.Value} InventoryID: {InventoryIDField?.Value} Subitem: {SubitemField?.Value} LotSerialNbr: {LotSerialNbrField?.Value} OrigOrderType: {OrigOrderTypeField?.Value} OrigOrderNbr: {OrigOrderNbrField?.Value}).");
					}
				}
			}
		}
	}
}

