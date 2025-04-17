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

using PX.Api;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

using PX.Objects.Common;
using PX.Objects.AR;
using PX.Objects.IN;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.SO.GraphExtensions
{
	public abstract class SOBaseItemAvailabilityExtension<TGraph, TLine, TSplit> : IN.GraphExtensions.ItemAvailabilityExtension<TGraph, TLine, TSplit>
		where TGraph : PXGraph
		where TLine : class, IBqlTable, ILSPrimary, new()
		where TSplit : class, IBqlTable, ILSDetail, new()
	{
		protected virtual ReturnedQtyResult MemoCheckQty(
			int? inventoryID,
			string arDocType, string arRefNbr, int? arTranLineNbr,
			string orderType, string orderNbr, int? orderLineNbr)
		{
			var qtyResult = new ReturnedQtyResult(true);

			bool hasRefToOrigSOLine = orderType != null && orderNbr != null && orderLineNbr != null;
			bool hasRefToOrigARTran = arDocType != null && arRefNbr != null && arTranLineNbr != null;
			if (!hasRefToOrigSOLine && !hasRefToOrigARTran)
				return qtyResult;

			SOInvoicedRecords invoiced = SelectInvoicedRecords(arDocType, arRefNbr);

			//return SO lines (including current document, excluding cancelled orders):
			var returnSOLines = SelectReturnSOLines(arDocType, arRefNbr);

			//return direct AR Transactions (including current document):
			var returnARTrans = SelectReturnARTrans(arDocType, arRefNbr);

			if (hasRefToOrigSOLine)
			{
				var invoicedFromSOLine = invoiced.Records
					.Where(r =>
						r.SOLine.OrderType == orderType &&
						r.SOLine.OrderNbr == orderNbr &&
						r.SOLine.LineNbr == orderLineNbr);
				var returnedFromSOLine = returnSOLines
					.Where(l =>
						l.OrigOrderType == orderType &&
						l.OrigOrderNbr == orderNbr
						&& l.OrigLineNbr == orderLineNbr)
					.Select(ReturnRecord.FromSOLine);
				qtyResult = CheckInvoicedAndReturnedQty(inventoryID, invoicedFromSOLine, returnedFromSOLine);
			}

			if (qtyResult.Success == true && hasRefToOrigARTran)
			{
				var invoicedFromOrigARTran = invoiced.Records.Where(r => r.ARTran.LineNbr == arTranLineNbr);
				var returnedFromOrigARTran =
						returnARTrans
						.Where(t => t.OrigInvoiceLineNbr == arTranLineNbr)
						.Select(ReturnRecord.FromARTran)
					.Concat(
						returnSOLines
						.Where(l => l.InvoiceLineNbr == arTranLineNbr)
						.Select(ReturnRecord.FromSOLine));
				qtyResult = CheckInvoicedAndReturnedQty(inventoryID, invoicedFromOrigARTran, returnedFromOrigARTran);
			}

			return qtyResult;
		}

		protected virtual SOLine[] SelectReturnSOLines(string arDocType, string arRefNbr)
		{
			if (string.IsNullOrEmpty(arDocType) || string.IsNullOrEmpty(arRefNbr))
				return Array<SOLine>.Empty;

			var query = new SelectFrom<SOLine>
				.InnerJoin<SOOrder>
					.On<SOLine.FK.Order>
				.Where<
					SOLine.invoiceType.IsEqual<@P.AsString.ASCII>
					.And<SOLine.invoiceNbr.IsEqual<@P.AsString>>
					.And<SOLine.operation.IsEqual<SOOperation.receipt>>
					.And<SOOrder.cancelled.IsEqual<False>>>
				.View.ReadOnly(Base);

			SOLine[] lines;
			using (new PXFieldScope(query.View, typeof(SOLine)))
				lines = query.SelectMain(arDocType, arRefNbr);

			var linesCache = Base.Caches<SOLine>();
			if (linesCache.IsInsertedUpdatedDeleted)
			{
				var GetOrder = Func.Memorize((string orderType, string orderNbr) =>
					PXParentAttribute.SelectParent<SOOrder>(linesCache, new SOLine { OrderType = orderType, OrderNbr = orderNbr })
					?? (SOOrder)Base.Caches<SOOrder>().Current);

				bool IsReturnLine(SOLine line) =>
					line.InvoiceType == arDocType
					&& line.InvoiceNbr == arRefNbr
					&& line.Operation == SOOperation.Receipt
					&& GetOrder(line.OrderType, line.OrderNbr)?.Cancelled == false;

				var linesSet = new HashSet<SOLine>(new KeyValuesComparer<SOLine>(linesCache, linesCache.BqlKeys));

				linesSet.AddRange(linesCache.Inserted
					.Concat_(linesCache.Updated)
					.RowCast<SOLine>()
					.Where(IsReturnLine)
					.ToArray());

				linesSet.UnionWith(lines);

				linesSet.ExceptWith(linesCache.Deleted
					.RowCast<SOLine>()
					.Where(IsReturnLine));

				lines = linesSet.ToArray();
			}
			return lines;
		}

		protected virtual IEnumerable<ARTran> SelectReturnARTrans(string arDocType, string arRefNbr)
		{
			if (string.IsNullOrEmpty(arDocType) || string.IsNullOrEmpty(arRefNbr))
				return Array<ARTran>.Empty;

			PXSelectBase<ARTran> selectReturnARTrans = new SelectFrom<ARTran>.
				Where<
					ARTran.sOOrderNbr.IsNull.
					And<ARTran.origInvoiceType.IsEqual<@P.AsString.ASCII>>.
					And<ARTran.origInvoiceNbr.IsEqual<@P.AsString>>.
					And<ARTran.qty.Multiply<ARTran.invtMult>.IsGreater<decimal0>>>.
				View(Base);

			return selectReturnARTrans.Select(arDocType, arRefNbr).RowCast<ARTran>();
		}

		public virtual SOInvoicedRecords SelectInvoicedRecords(string arDocType, string arRefNbr)
		{
			return SelectInvoicedRecords(arDocType, arRefNbr, includeDirectLines: false);
		}

		protected virtual SOInvoicedRecords SelectInvoicedRecords(string arDocType, string arRefNbr, bool includeDirectLines)
		{
			SOInvoicedRecords splits = new SOInvoicedRecords(Base.Caches<ARTran>().GetComparer());

			if (string.IsNullOrEmpty(arDocType) || string.IsNullOrEmpty(arRefNbr))
				return splits;

			PXSelectBase<ARTran> cmd = new
				SelectFrom<ARTran>.
				InnerJoin<InventoryItem>.On<ARTran.FK.InventoryItem>.
				LeftJoin<SOLine>.On<ARTran.FK.SOOrderLine>.
				LeftJoin<INTran>.On<INTran.FK.ARTran>.
				LeftJoin<INTranSplit>.On<INTranSplit.FK.Tran>.
				LeftJoin<INLotSerialStatusByCostCenter>.On<
					INLotSerialStatusByCostCenter.lotSerTrack.IsEqual<INLotSerTrack.serialNumbered>.
					And<INLotSerialStatusByCostCenter.inventoryID.IsEqual<INTranSplit.inventoryID>>.
					And<INLotSerialStatusByCostCenter.lotSerialNbr.IsEqual<INTranSplit.lotSerialNbr>>.
					And<
						INLotSerialStatusByCostCenter.qtyOnHand.IsGreater<decimal0>.
						Or<INLotSerialStatusByCostCenter.qtyINReceipts.IsGreater<decimal0>>.
						Or<INLotSerialStatusByCostCenter.qtySOShipping.IsLess<decimal0>>.
						Or<INLotSerialStatusByCostCenter.qtySOShipped.IsLess<decimal0>>>>.
				LeftJoin<SOSalesPerTran>.On<
					SOSalesPerTran.orderType.IsEqual<SOLine.orderType>.
					And<SOSalesPerTran.orderNbr.IsEqual<SOLine.orderNbr>>.
					And<SOSalesPerTran.salespersonID.IsEqual<SOLine.salesPersonID>>>.
				Where<
					ARTran.tranType.IsEqual<@P.AsString.ASCII>.
					And<ARTran.refNbr.IsEqual<@P.AsString>>.
					And<
						Brackets<
							INTran.released.IsEqual<True>.
							And<INTran.qty.IsGreater<decimal0>>.
							And<
								INTran.tranType.IsEqual<INTranType.issue>.
								Or<INTran.tranType.IsEqual<INTranType.debitMemo>>.
								Or<INTran.tranType.IsEqual<INTranType.invoice>>>>.
						Or<
							INTran.released.IsNull.
							And<ARTran.lineType.IsIn<SOLineType.miscCharge, SOLineType.nonInventory>>>>.
					And<
						ARTran.qty.IsEqual<decimal0>.
						Or<
							ARTran.qty.IsGreater<decimal0>.
							And<ARTran.tranType.IsIn<ARDocType.debitMemo, ARDocType.cashSale, ARDocType.invoice>>>.
						Or<
							ARTran.qty.IsLess<decimal0>.
							And<ARTran.tranType.IsIn<ARDocType.creditMemo, ARDocType.cashReturn>>>>>.
				OrderBy<
					ARTran.inventoryID.Asc,
					INTranSplit.subItemID.Asc>.
				View(Base);

			if (!includeDirectLines)
				cmd.WhereAnd<Where<ARTran.lineType, Equal<SOLine.lineType>, And<SOLine.orderNbr, IsNotNull>>>();

			foreach (PXResult<ARTran, InventoryItem, SOLine, INTran, INTranSplit, INLotSerialStatusByCostCenter, SOSalesPerTran> res in
				cmd.Select(arDocType, arRefNbr))
			{
				splits.Add(res);
			}

			return splits;
		}

		protected virtual ReturnedQtyResult CheckInvoicedAndReturnedQty(
			int? returnInventoryID,
			IEnumerable<SOInvoicedRecords.Record> invoiced,
			IEnumerable<ReturnRecord> returned)
		{
			if (returnInventoryID == null)
				return new ReturnedQtyResult(true);

			int origInventoryID = 0;
			decimal totalInvoicedQty = 0;
			var totalInvoicedQtyByComponent = new Dictionary<int, decimal>();
			var componentsInAKit = new Dictionary<int, decimal>();

			//invoiced are always either KIT or a regular item
			foreach (SOInvoicedRecords.Record record in invoiced)
			{
				origInventoryID = record.SOLine.InventoryID ?? record.ARTran.InventoryID.Value;
				decimal invoicedQty = (record.ARTran.DrCr == DrCr.Debit ? -1m : 1m) * (decimal)record.ARTran.Qty;
				totalInvoicedQty += INUnitAttribute.ConvertToBase(Base.Caches<ARTran>(), record.ARTran.InventoryID, record.ARTran.UOM, invoicedQty, INPrecision.QUANTITY);

				foreach (SOInvoicedRecords.INTransaction intran in record.Transactions.Values)
				{
					if (!totalInvoicedQtyByComponent.ContainsKey(intran.Transaction.InventoryID.Value))
						totalInvoicedQtyByComponent[intran.Transaction.InventoryID.Value] = 0;

					totalInvoicedQtyByComponent[intran.Transaction.InventoryID.Value] +=
						INUnitAttribute.ConvertToBase(Base.Caches<INTran>(), intran.Transaction.InventoryID, intran.Transaction.UOM, intran.Transaction.Qty.Value, INPrecision.QUANTITY);
				}
			}

			decimal invoiceQtySign = totalInvoicedQty > 0 ? 1m : -1m;

			foreach (KeyValuePair<int, decimal> kv in totalInvoicedQtyByComponent)
				componentsInAKit[kv.Key] = kv.Value / totalInvoicedQty;

			//returned can be a regular item or a kit or a component of a kit. 
			foreach (var ret in returned)
			{
				if (ret.InventoryID == origInventoryID || totalInvoicedQtyByComponent.Count == 0)//regular item or a kit
				{
					decimal returnedQty = INUnitAttribute.ConvertToBase(LineCache, ret.InventoryID, ret.UOM, ret.Qty, INPrecision.QUANTITY);
					totalInvoicedQty -= returnedQty;

					InventoryItem item = InventoryItem.PK.Find(Base, ret.InventoryID);
					if (item.KitItem == true)
					{
						foreach (KeyValuePair<int, decimal> kv in componentsInAKit)
						{
							totalInvoicedQtyByComponent[kv.Key] -= componentsInAKit[kv.Key] * returnedQty;
						}
					}
				}
				else //component of a kit. 
				{
					totalInvoicedQtyByComponent[ret.InventoryID.Value] -= INUnitAttribute.ConvertToBase(LineCache, ret.InventoryID, ret.UOM, ret.Qty, INPrecision.QUANTITY);
				}
			}

			bool success = true;
			if (returnInventoryID == origInventoryID)
			{
				if (invoiceQtySign * totalInvoicedQty < 0m || totalInvoicedQtyByComponent.Values.Any(v => invoiceQtySign * v < 0m))
					success = false;
			}
			else
			{
				if (invoiceQtySign * totalInvoicedQty < 0m)
					success = false;

				if (totalInvoicedQtyByComponent.TryGetValue(returnInventoryID.Value, out decimal qtyByComponent) && invoiceQtySign * qtyByComponent < 0)
					success = false;
			}

			return new ReturnedQtyResult(success, success ? null : returned.ToArray());
		}

		[PXInternalUseOnly]
		public abstract class ReturnRecord
		{
			public abstract int? InventoryID { get; }
			public abstract string UOM { get; }
			public abstract decimal Qty { get; }
			public abstract string DocumentNbr { get; }

			public static ReturnRecord FromSOLine(SOLine l) => new ReturnSOLine(l);

			public static ReturnRecord FromARTran(ARTran t) => new ReturnARTran(t);

			private class ReturnSOLine : ReturnRecord
			{
				public ReturnSOLine(SOLine line) => Line = line;

				public SOLine Line { get; }
				public override string DocumentNbr => Line.OrderNbr;
				public override int? InventoryID => Line.InventoryID;
				public override string UOM => Line.UOM;
				public override decimal Qty => (Line.InvtMult *
					(Line.LineType != SOLineType.MiscCharge && Line.RequireShipping == true && Line.Completed == true
						? Line.ShippedQty : Line.OrderQty)) ?? 0m;
			}

			private class ReturnARTran : ReturnRecord
			{
				public ReturnARTran(ARTran tran) => Tran = tran;

				public ARTran Tran { get; }
				public override string DocumentNbr => Tran.RefNbr;
				public override int? InventoryID => Tran.InventoryID;
				public override string UOM => Tran.UOM;
				public override decimal Qty => Math.Abs(Tran.Qty ?? 0m);
			}
		}

		public class ReturnedQtyResult
		{
			public ReturnedQtyResult(bool success, ReturnRecord[] returnRecords = null)
			{
				Success = success;
				ReturnRecords = returnRecords;
			}

			public bool Success { get; private set; }
			public ReturnRecord[] ReturnRecords { get; private set; }
		}
	}
}
