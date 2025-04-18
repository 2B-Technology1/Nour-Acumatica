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
using PX.Objects.PO;
using PX.Objects.SO;
using PX.Objects.PO.DAC.Projections;
using System.Linq;
using System;
using PX.Common;
using PX.Objects.CS;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace PX.Objects.EndpointAdapters
{
	[PXInternalUseOnly]
	[PXVersion("20.200.001", "Default")]
	public class DefaultEndpointImpl20 : DefaultEndpointImpl18
	{
		[FieldsProcessed(new[] {
			"AttributeID",
			"AttributeDescription",
			"RefNoteID",
			"Value",
			"ValueDescription"
		})]
		protected new void AttributeValue_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			var attributeID = targetEntity.Fields.OfType<EntityValueField>().FirstOrDefault(f => f.Name == "AttributeID")?.Value;
			if (attributeID == null)
			{
				Debug.Fail("Cannot get AttributeID");
				return;
			}
			ProcessAttribute(graph, targetEntity, attributeID);
		}

		[FieldsProcessed(new[] {
			"AttributeID",
			"AttributeDescription",
			"RefNoteID",
			"Value",
			"ValueDescription"
		})]
		protected void AttributeValue_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			if (!targetEntity.InternalKeys.TryGetValue(CS.Messages.CSAnswers, out var answersKeys)
				|| !answersKeys.TryGetValue(nameof(CSAnswers.AttributeID), out var attributeID))
			{
				Debug.Fail("Cannot get AttributeID");
				return;
			}
			ProcessAttribute(graph, targetEntity, attributeID);
		}

		private void ProcessAttribute(PXGraph graph, EntityImpl targetEntity, string attributeID)
		{
			var value = targetEntity.Fields.OfType<EntityValueField>().FirstOrDefault(f => f.Name == "Value");
			if (value == null)
				return;

			var view = graph.Views[CS.Messages.CSAnswers];
			var cache = view.Cache;

			var rows = view.SelectMulti().OrderBy(row =>
			{
				var orderState = cache.GetStateExt<CSAnswers.order>(row) as PXFieldState;
				return orderState.Value;
			}).ToArray();

			var updatedAttributes = new OrderedDictionary();

			foreach (CSAnswers row in rows)
			{
				var attributeDescr = (cache.GetStateExt<CSAnswers.attributeID>(row) as PXFieldState)?.Value?.ToString();
				if (attributeID.OrdinalEquals(row.AttributeID) || attributeID.OrdinalEquals(attributeDescr))
				{
					if (cache.GetStateExt<CSAnswers.value>(row) is PXStringState state)
					{
						if (state.Enabled is false)
						{
							continue;
						}
						if (state.ValueLabelDic != null)
						{
							foreach (var rec in state.ValueLabelDic)
							{
								if (rec.Value == value.Value)
								{
									value.Value = rec.Key;
									break;
								}
							}
						}
					}
					cache.SetValueExt<CSAnswers.value>(row, value.Value);
					cache.Update(row);
					updatedAttributes.Add(row.AttributeID, value.Value);
					break;
				}
			}
			graph.ExecuteUpdate(CS.Messages.CSAnswers, updatedAttributes, updatedAttributes);
		}
		
		[FieldsProcessed(new[] {
			"POLineNbr",
			"POOrderType",
			"POOrderNbr",
			"POReceiptLineNbr",
			"POReceiptNbr",
			"TransferOrderType",
			"TransferOrderNbr",
			"TransferShipmentNbr"
		})]
		protected override void PurchaseReceiptDetail_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			var receiptEntry = (POReceiptEntry)graph;

			if (receiptEntry.Document.Current != null)
			{
				var receiptNbr = targetEntity.Fields.SingleOrDefault(f => f.Name == "POReceiptNbr") as EntityValueField;

				var detailsCache = receiptEntry.transactions.Cache;

				if (receiptEntry.Document.Current.ReceiptType == POReceiptType.POReturn && receiptNbr != null)
				{
					var receiptLineNbr = targetEntity.Fields.SingleOrDefault(f => f.Name == "POReceiptLineNbr") as EntityValueField;

					bool insertViaAddPRLine = receiptLineNbr != null && receiptNbr != null;
					bool insertViaAddPR = receiptNbr != null;

					if (insertViaAddPRLine)
					{
						FillInAddPRFilter(receiptEntry, receiptNbr);

						var receiptLines = receiptEntry.poReceiptLineReturn.Select().Select(r => r.GetItem<POReceiptLineReturn>());
						var receiptLine = receiptLines.FirstOrDefault(o => o.LineNbr == int.Parse(receiptLineNbr.Value));
						if (receiptLine == null)
						{
							throw new PXException(PO.Messages.PurchaseReceiptLineNotFound);
						}
						receiptLine.Selected = true;
						receiptEntry.poReceiptLineReturn.Update(receiptLine);
						receiptEntry.Actions["AddPOReceiptLineReturn2"].Press();
						return;
					}
					else if (insertViaAddPR)
					{
						FillInAddPRFilter(receiptEntry, receiptNbr);

						var order = receiptEntry.poReceiptReturn.Select().Select(r => r.GetItem<POReceiptReturn>()).FirstOrDefault();

						order.Selected = true;
						receiptEntry.poReceiptReturn.Update(order);
						receiptEntry.Actions["AddPOReceiptReturn2"].Press();
						return;
					}
				}

				base.PurchaseReceiptDetail_Insert(graph, entity, targetEntity);

				if (receiptEntry.Document.Current.ReceiptType == POReceiptType.POReturn && receiptNbr == null && detailsCache.Current != null
					&& ((POReceiptLine)detailsCache.Current).InventoryID == null)
				{
					SetFieldsNeedToInsertAllocations(targetEntity, receiptEntry, (POReceiptLine)detailsCache.Current);
				}
			}
		}

		protected virtual void FillInAddPRFilter(POReceiptEntry receiptEntry, EntityValueField receiptNbr)
		{
			receiptEntry.returnFilter.Cache.Remove(receiptEntry.returnFilter.Current);
			receiptEntry.returnFilter.Cache.Insert(new POReceiptReturnFilter());
			var filter = receiptEntry.returnFilter.Current;

			receiptEntry.returnFilter.Cache.SetValueExt(filter, nameof(POReceiptReturnFilter.ReceiptNbr), receiptNbr.Value);
			filter = receiptEntry.returnFilter.Update(filter);

			Dictionary<string, string> filterErrors = PXUIFieldAttribute.GetErrors(receiptEntry.returnFilter.Cache, filter);

			if (filterErrors.Count() > 0)
			{
				throw new PXException(string.Join(";", filterErrors.Select(x => x.Key + "=" + x.Value)));
			}
		}

		[FieldsProcessed(new string[0])]
		protected virtual void SalesOrderDetail_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			var orderEntry = (SOOrderEntry)graph;

			if (orderEntry.Document.Current != null)
			{
				var detailsCache = orderEntry.Transactions.Cache;
				detailsCache.Current = detailsCache.Insert();
				var orderLineCurrent = detailsCache.Current as SOLine;

				if (detailsCache.Current == null)
					throw new InvalidOperationException("Cannot insert Sales Order detail.");

				//We need to insert InventoryID here regardless of order Behavior, otherwise there will be issues with discount and taxes calculation
				var InventoryIDField = targetEntity.Fields.SingleOrDefault(f => f.Name == "InventoryID") as EntityValueField;
				if (InventoryIDField != null)
				{
					orderEntry.Transactions.Cache.SetValueExt(orderLineCurrent, "InventoryID", InventoryIDField.Value);
					orderLineCurrent = orderEntry.Transactions.Update(orderLineCurrent);
				}

				//For now this logic is only applicable to Blanket orders to minimize possible issues with regular order types.
				if (orderEntry.Document.Current.Behavior == SOBehavior.BL)
				{
					var allocations = (targetEntity.Fields.SingleOrDefault(f => string.Equals(f.Name, "Allocations")) as EntityListField)?.Value ?? new EntityImpl[0];
					bool hasAllocations = allocations.Any(a => a.Fields != null && a.Fields.Length > 0);

					if (hasAllocations)
					{
						var SiteField = targetEntity.Fields.SingleOrDefault(f => f.Name == "WarehouseID") as EntityValueField;
						var LocationField = targetEntity.Fields.SingleOrDefault(f => f.Name == "Location") as EntityValueField;
						var SubItemField = targetEntity.Fields.FirstOrDefault(f => f.Name == "Subitem") as EntityValueField;

						if (SiteField != null)
							orderEntry.Transactions.Cache.SetValueExt(orderLineCurrent, "SiteID", SiteField.Value);
						if (LocationField != null)
							orderEntry.Transactions.Cache.SetValueExt(orderLineCurrent, "LocationID", LocationField.Value);
						if (SubItemField != null)
							orderEntry.Transactions.Cache.SetValueExt(orderLineCurrent, "SubItemID", SubItemField.Value);

						var QtyField = targetEntity.Fields.FirstOrDefault(f => f.Name == "OrderQty") as EntityValueField;
						if (QtyField != null)
						{
							orderLineCurrent.OrderQty = decimal.Parse(QtyField.Value);
							orderLineCurrent = orderEntry.Transactions.Update(orderLineCurrent);
						}

						//All the created splits will be deleted. New splits will be inserted later.
						if (detailsCache.Current != null)
						{
							var inserted = orderEntry.splits.Cache.Inserted;
							foreach (SOLineSplit split in inserted)
							{
								if (split.LineNbr == (detailsCache.Current as SOLine).LineNbr)
									orderEntry.splits.Delete(split);
							}
						}
					}
				}
			}
		}
	}
}
