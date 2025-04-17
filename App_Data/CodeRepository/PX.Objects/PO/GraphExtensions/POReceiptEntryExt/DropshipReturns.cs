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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.CM.Extensions;
using PX.Objects.CS;
using PX.Objects.Extensions.MultiCurrency;
using PX.Objects.IN;
using PX.Objects.SO;

namespace PX.Objects.PO.GraphExtensions.POReceiptEntryExt
{
	public class DropshipReturn : PXGraphExtension<POReceiptEntry>
	{
		public static bool IsActive()
			=> PXAccess.FeatureInstalled<FeaturesSet.dropShipments>();

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDBDefaultAttribute))]
		[PXDBDefault(typeof(POReceipt.receiptNbr), DefaultForUpdate = false)]
		[PXParent(typeof(Select<POReceipt,
			Where<POReceipt.receiptType, Equal<Switch<Case<Where<Current<SOOrderShipment.operation>, Equal<SOOperation.receipt>>, POReceiptType.poreturn>, POReceiptType.poreceipt>>,
				And<POReceipt.receiptNbr, Equal<Current<SOOrderShipment.shipmentNbr>>,
				And<Current<SOOrderShipment.shipmentType>, Equal<SOShipmentType.dropShip>>>>>))]
		public virtual void _(Events.CacheAttached<SOOrderShipment.shipmentNbr> e)
		{
		}

		public virtual void _(Events.RowSelected<POReceipt> e)
		{
			PXUIFieldAttribute.SetDisplayName<POReceiptLine.sOOrderType>(Base.transactions.Cache,
				(e.Row?.ReceiptType == POReceiptType.POReturn) ? Messages.SOReturnType : Messages.TransferOrderType);
			PXUIFieldAttribute.SetDisplayName<POReceiptLine.sOOrderNbr>(Base.transactions.Cache,
				(e.Row?.ReceiptType == POReceiptType.POReturn) ? Messages.SOReturn : Messages.TransferOrderNbr);
			PXUIFieldAttribute.SetDisplayName<POReceiptLine.sOOrderLineNbr>(Base.transactions.Cache,
				(e.Row?.ReceiptType == POReceiptType.POReturn) ? Messages.SOReturnLine : Messages.TransferLineNbr);

			if (e.Row == null) return;

			bool dropshipReturn = !string.IsNullOrEmpty(e.Row.SOOrderNbr);
			PXUIFieldAttribute.SetVisible<POReceipt.sOOrderNbr>(e.Cache, e.Row, dropshipReturn);
			if (dropshipReturn)
			{
				Base.transactions.Cache.AllowInsert =
					Base.transactions.Cache.AllowUpdate =
					Base.transactions.Cache.AllowDelete = false;
				Base.addPOReceiptReturn.SetEnabled(false);
				Base.addPOReceiptLineReturn.SetEnabled(false);
				PXUIFieldAttribute.SetEnabled<POReceipt.returnInventoryCostMode>(e.Cache, e.Row, false);
			}
		}

		/// <summary>
		/// Overrides <see cref="POReceiptEntry.UpdateReturnOrdersMarkedForDropship(POReceiptLine, POLineUOpen)"/>
		/// </summary>
		[PXOverride]
		public virtual void UpdateReturnOrdersMarkedForDropship(POReceiptLine line, POLineUOpen poLine,
			Action<POReceiptLine, POLineUOpen> baseMethod)
		{
			baseMethod?.Invoke(line, poLine);

			if (!POLineType.IsDropShip(line.LineType))
				return;

			SOLine4 soLine = SelectFrom<SOLine4>
				.Where<SOLine4.orderType.IsEqual<POReceiptLine.sOOrderType.FromCurrent>
				.And<SOLine4.orderNbr.IsEqual<POReceiptLine.sOOrderNbr.FromCurrent>>
				.And<SOLine4.lineNbr.IsEqual<POReceiptLine.sOOrderLineNbr.FromCurrent>>>
				.View.SelectMultiBound(Base, new object[] { line });

			if (soLine == null
				|| soLine.UOM != line.UOM || soLine.LineSign * soLine.OrderQty != line.ReceiptQty || soLine.LineSign * soLine.OpenQty != line.ReceiptQty
				|| soLine.Operation != SOOperation.Receipt
				|| soLine.POCreate != true || soLine.POSource != INReplenishmentSource.DropShipToOrder
				|| soLine.OpenLine != true || soLine.Completed == true)
			{
				throw new PXException(Messages.RMALineModifiedVendorReturnCantRelease);
			}

			soLine = PXCache<SOLine4>.CreateCopy(soLine);
			soLine.ShippedQty = soLine.OrderQty;
			soLine.BaseShippedQty = soLine.BaseOrderQty;
			soLine.OpenQty = 0m;
			soLine.CuryOpenAmt = 0m;
			soLine.Completed = true;
			soLine.OpenLine = false;
			soLine = Base.solineselect.Update(soLine);

			foreach (var soSplitRes in SelectFrom<SOLineSplit>
				.Where<SOLineSplit.orderType.IsEqual<POReceiptLine.sOOrderType.FromCurrent>
				.And<SOLineSplit.orderNbr.IsEqual<POReceiptLine.sOOrderNbr.FromCurrent>>
				.And<SOLineSplit.lineNbr.IsEqual<POReceiptLine.sOOrderLineNbr.FromCurrent>>>
				.View.SelectMultiBound(Base, new object[] { line }))
			{
				SOLineSplit soSplit = PXCache<SOLineSplit>.CreateCopy(soSplitRes);
				soSplit.ReceivedQty = soSplit.Qty;
				soSplit.ShippedQty = soSplit.Qty;
				soSplit.Completed = true;
				soSplit.POReceiptType = line.ReceiptType;
				soSplit.POReceiptNbr = line.ReceiptNbr;
				soSplit.POCompleted = true;
				soSplit.PlanID = null;
				soSplit = Base.solinesplitselect.Update(soSplit);
			}

			var order = PXParentAttribute.SelectParent<SOOrder>(Base.solineselect.Cache, soLine);
			if (order.Approved != true)
			{
				var ownerName = Base.soorderselect.Cache.GetValueExt<SOOrder.ownerID>(order);

				throw new PXException(Messages.SalesOrderRelatedToDropShipReceiptIsNotApproved,
					order.OrderType, order.OrderNbr, ownerName);
			}
			Base.PopulateDropshipFields(order);
			order.OpenLineCntr--;

			CurrencyInfo soCurrencyInfo = Base.MultiCurrencyExt.GetCurrencyInfo(order.CuryInfoID);
			decimal? actualDiscUnitPrice = Base.GetActualDiscUnitPrice(soLine);
			decimal? lineAmt = soCurrencyInfo.RoundBase((decimal)(soLine.LineSign * soLine.OrderQty * actualDiscUnitPrice));
			var oshipment = Base.CreateUpdateOrderShipment(order, line, null, true, soLine.LineSign * soLine.BaseOrderQty, lineAmt);

			if (order.OpenShipmentCntr == 0 && order.OpenLineCntr == 0)
			{
				order.MarkCompleted();
			}
		}

		public virtual void CreatePOReturn(SOOrder currentDoc, DocumentList<POReceipt> receiptList)
		{
			var linesToReturn = SelectFrom<SOLine>
				.InnerJoin<SOOrder>.On<SOLine.FK.Order>
				.InnerJoin<ARTran>.On<SOLine.FK.InvoiceLine>
				.InnerJoin<POReceipt>
					.On<POReceipt.receiptType.IsEqual<POReceiptType.poreceipt>
					.And<POReceipt.receiptNbr.IsEqual<ARTran.sOShipmentNbr>>>
				.LeftJoin<POReceiptLineReturn>
					.On<POReceiptLineReturn.receiptNbr.IsEqual<ARTran.sOShipmentNbr>
					.And<POReceiptLineReturn.lineNbr.IsEqual<ARTran.sOShipmentLineNbr>>>
				.LeftJoin<POReceiptLine>.On<POReceiptLine.FK.SOLine>
				.Where<SOLine.FK.Order.SameAsCurrent
					.And<SOLine.operation.IsEqual<SOOperation.receipt>>
					.And<SOLine.origShipmentType.IsEqual<SOShipmentType.dropShip>>
					.And<SOLine.pOCreate.IsEqual<True>>
					.And<SOLine.pOSource.IsEqual<INReplenishmentSource.dropShipToOrder>>
					.And<SOLine.completed.IsEqual<False>>
					.And<POReceiptLine.receiptNbr.IsNull>>
				.OrderBy<Asc<POReceipt.receiptNbr>>
				.View.SelectMultiBound(Base, new[] { currentDoc }).AsEnumerable()
				.Cast<PXResult<SOLine, SOOrder, ARTran, POReceipt, POReceiptLineReturn>>()
				.ToList();

			for (int i = 0; i < linesToReturn.Count; i++)
			{
				PXResult<SOLine, SOOrder, ARTran, POReceipt, POReceiptLineReturn> res = linesToReturn[i];

				SOLine line = res;
				SOOrder order = res;
				POReceipt receipt = res;
				POReceiptLineReturn receiptLine = res;
				if (string.IsNullOrEmpty(receiptLine.ReceiptNbr))
				{
					throw new PXInvalidOperationException(Messages.OrigReceiptLineNotFound,
						PXForeignSelectorAttribute.GetValueExt<SOLine.inventoryID>(Base.Caches[typeof(SOLine)], line));
				}

				if (Base.Document.Current == null)
				{
					POReceipt poReturn = Base.Document.Insert(
						new POReceipt() { ReceiptType = POReceiptType.POReturn });

					poReturn.ReceiptType = POReceiptType.POReturn;
					poReturn.BranchID = receipt.BranchID;
					poReturn.VendorID = receipt.VendorID;
					poReturn.VendorLocationID = receipt.VendorLocationID;
					poReturn.ProjectID = receiptLine.ProjectID;
					poReturn.CuryID = receipt.CuryID;
					poReturn.AutoCreateInvoice = false;
					poReturn.ReturnInventoryCostMode = ReturnCostMode.OriginalCost;

					poReturn.SOOrderType = line.OrderType;
					poReturn.SOOrderNbr = line.OrderNbr;

					Base.CopyReceiptCurrencyInfoToReturn(receipt.CuryInfoID, Base.Document.Current?.CuryInfoID,
						Base.Document.Current?.ReturnInventoryCostMode == ReturnCostMode.OriginalCost);
				}

				POReceiptLine returnLine = AddReturnLine(receiptLine, line);
				CurrencyInfo currencyInfo = Base.FindImplementation<IPXCurrencyHelper>().GetDefaultCurrencyInfo();

				SOLine4 lineAlias = PropertyTransfer.Transfer(line, new SOLine4());
				decimal? actualDiscUnitPrice = Base.GetActualDiscUnitPrice(lineAlias);
				decimal? lineAmt = currencyInfo.RoundBase((decimal)(line.LineSign * line.OrderQty * actualDiscUnitPrice));
				var oshipment = Base.CreateUpdateOrderShipment(order, returnLine, null, false, line.LineSign * line.BaseOrderQty, lineAmt);

				if (i + 1 >= linesToReturn.Count
					|| !Base.Document.Cache.ObjectsEqual<POReceipt.receiptNbr>(receipt, (POReceipt)linesToReturn[i + 1]))
				{
					Base.Save.Press();
					receiptList.Add(Base.Document.Current);
					Base.Clear();
				}
			}
		}

		protected virtual POReceiptLine AddReturnLine(POReceiptLineReturn receiptLine, SOLine line)
		{
			bool requireLotSerial = IsLotSerialRequired(receiptLine.InventoryID);
			var newLine = Base.transactions.Insert(new POReceiptLine
			{
				IsLSEntryBlocked = requireLotSerial,
			});

			Base.CopyFromOrigReceiptLine(newLine, receiptLine, preserveLineType: true, returnOrigCost: Base.Document.Current?.ReturnInventoryCostMode == ReturnCostMode.OriginalCost);

			InventoryItem item = InventoryItem.PK.Find(Base, receiptLine.InventoryID);
			bool isConverted = item?.IsConverted == true && newLine.IsStockItem != item.StkItem;
			if (isConverted && newLine.LineType.IsIn(POLineType.GoodsForDropShip, POLineType.NonStockForDropShip))
			{
				newLine.LineType = (item.StkItem == true) ? POLineType.GoodsForDropShip : POLineType.NonStockForDropShip;
			}

			newLine = Base.transactions.Update(newLine);

			newLine.SOOrderType = line.OrderType;
			newLine.SOOrderNbr = line.OrderNbr;
			newLine.SOOrderLineNbr = line.LineNbr;
			newLine.UOM = line.UOM;
			newLine.ReceiptQty = line.LineSign * line.OrderQty;
			newLine.BaseReceiptQty = line.LineSign * line.BaseOrderQty;
			newLine = Base.transactions.Update(newLine);

			if (requireLotSerial)
			{
				newLine.IsLSEntryBlocked = false;
				newLine = Base.transactions.Update(newLine);
				if (receiptLine.LocationID != null)
					Base.transactions.Cache.SetValueExt<POReceiptLine.locationID>(newLine, receiptLine.LocationID);
				else
					Base.transactions.Cache.SetDefaultExt<POReceiptLine.locationID>(newLine);

				List<SOLineSplit> lineSplits = SelectFrom<SOLineSplit>
					.Where<SOLineSplit.FK.OrderLine.SameAsCurrent>
					.View.SelectMultiBound(Base, new[] { line })
					.RowCast<SOLineSplit>().ToList();
				foreach (SOLineSplit lineSplit in lineSplits)
				{
					var split = new POReceiptLineSplit
					{
						InventoryID = lineSplit.InventoryID,
						SubItemID = lineSplit.SubItemID,
						LotSerialNbr = lineSplit.LotSerialNbr,
						ExpireDate = lineSplit.ExpireDate,
					};
					split = PXCache<POReceiptLineSplit>.CreateCopy(Base.splits.Insert(split));
					split.Qty = lineSplit.Qty;
					split = Base.splits.Update(split);
				}
			}

			if (!string.IsNullOrEmpty(newLine.POType) && !string.IsNullOrEmpty(newLine.PONbr))
			{
				Base.AddPOOrderReceipt(newLine.POType, newLine.PONbr);
			}
			return newLine;
		}

		protected virtual bool IsLotSerialRequired(int? inventoryID)
		{
			var item = InventoryItem.PK.Find(Base, inventoryID);
			if (item?.StkItem != true)
				return false;
			var lotSerClass = INLotSerClass.PK.Find(Base, item.LotSerClassID);
			return lotSerClass?.RequiredForDropship == true;
		}
	}
}
