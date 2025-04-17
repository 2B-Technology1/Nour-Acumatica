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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.CS;

namespace PX.Objects.SO
{
	public class SOOrderShipmentProcess : PXGraph<SOOrderShipmentProcess, SOOrderShipment>
	{
        public PXSelect<SOOrder> Orders;
        public PXSelect<SOShipment> Shipments;
		public PXSelect<SOMiscLine2,
			Where<SOMiscLine2.orderType, Equal<Required<SOOrder.orderType>>,
				And<SOMiscLine2.orderNbr, Equal<Required<SOOrder.orderNbr>>,
				And<SOMiscLine2.completed, NotEqual<True>>>>>
			MiscLines;
		public PXSelect<ARBalances> Arbalances;

		public PXAction<SOShipment> flow;
		[PXUIField(DisplayName = "Flow")]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntryF)]
		protected virtual IEnumerable Flow(PXAdapter adapter)
		{
			Save.Press();
			return adapter.Get();
		}

		public PXSelectJoin<SOOrderShipment,
			InnerJoin<SOOrder, 
				On<SOOrder.orderType, Equal<SOOrderShipment.orderType>, 
				And<SOOrder.orderNbr, Equal<SOOrderShipment.orderNbr>>>,
			InnerJoin<ARInvoice,
				On<ARInvoice.docType, Equal<SOOrderShipment.invoiceType>,
					And<ARInvoice.refNbr, Equal<SOOrderShipment.invoiceNbr>,
						And<ARInvoice.released, Equal<boolTrue>>>>>>,
			Where<SOOrderShipment.invoiceType, Equal<Current<ARInvoice.docType>>, 
				And<SOOrderShipment.invoiceNbr, Equal<Current<ARInvoice.refNbr>>>>> Items;
		public PXSelect<SOAdjust,
				Where<SOAdjust.adjdOrderType, Equal<Required<SOAdjust.adjdOrderType>>,
					And<SOAdjust.adjdOrderNbr, Equal<Required<SOAdjust.adjdOrderNbr>>>>> Adjustments;

		public PXSelect<SOTaxTran, Where<SOTaxTran.orderType, Equal<Required<SOTaxTran.orderType>>,
			And<SOTaxTran.orderNbr, Equal<Required<SOTaxTran.orderNbr>>>>> Taxes;

		protected virtual void SOOrder_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            SOOrder doc = (SOOrder)e.Row;
            if (e.Operation == PXDBOperation.Update)
            {
                if (doc.ShipmentCntr < 0 || doc.OpenShipmentCntr < 0 || doc.ShipmentCntr < doc.BilledCntr + doc.ReleasedCntr && doc.Behavior == SOBehavior.SO)
                {
                    throw new Exceptions.InvalidShipmentCountersException();
                }
            } 
        }
		protected virtual void _(Events.RowUpdated<SOOrderShipment> e)
		{
			if (e.Row.OrderTaxAllocated != true && e.OldRow.OrderTaxAllocated != true || e.Row.OrderTaxAllocated == e.OldRow.OrderTaxAllocated)
				return;

			var order = (SOOrder)PXParentAttribute.SelectParent(e.Cache, e.Row, typeof(SOOrder));
			if (order != null)
			{
				order.OrderTaxAllocated = (e.Row.OrderTaxAllocated == true);
				e.Cache.Graph.Caches<SOOrder>().Update(order);
			}
		}

		public SOOrderShipmentProcess()
		{
		}

		public virtual void CompleteSOLinesAndSplits(ARRegister ardoc, List<PXResult<SOOrderShipment, SOOrder>> orderShipments)
		{
			if (ardoc.IsCancellation == true || ardoc.IsCorrection == true) return;

			foreach (PXResult<SOOrderShipment, SOOrder> orderShipment in orderShipments)
			{
				SOOrder order = orderShipment;
				SOOrderType orderType = SOOrderType.PK.Find(this, order.OrderType);
				if (orderType.RequireShipping == false)
				{
					PXDatabase.Update<SOLine>(
						new PXDataFieldAssign<SOLine.completed>(true),
						new PXDataFieldRestrict<SOLine.completed>(false),
						new PXDataFieldRestrict<SOLine.orderType>(PXDbType.VarChar, 2, order.OrderType, PXComp.EQ),
						new PXDataFieldRestrict<SOLine.orderNbr>(PXDbType.NVarChar, 15, order.OrderNbr, PXComp.EQ));
					PXDatabase.Update<SOLineSplit>(
						new PXDataFieldAssign<SOLineSplit.completed>(true),
						new PXDataFieldRestrict<SOLineSplit.completed>(false),
						new PXDataFieldRestrict<SOLineSplit.orderType>(PXDbType.VarChar, 2, order.OrderType, PXComp.EQ),
						new PXDataFieldRestrict<SOLineSplit.orderNbr>(PXDbType.NVarChar, 15, order.OrderNbr, PXComp.EQ));
				}
			}
		}

		public virtual List<PXResult<SOOrderShipment, SOOrder>> UpdateOrderShipments(ARRegister arDoc, HashSet<object> processed)
		{
			bool isCancellationInvoice = arDoc.IsCancellation == true;
			bool isCorrectionInvoice = arDoc.IsCorrection == true;
			var boundInvoice = isCancellationInvoice
				? ARInvoice.PK.Find(this, arDoc.OrigDocType, arDoc.OrigRefNbr)
				: arDoc;

			var orderShipments = Items.View.SelectMultiBound(new object[] { boundInvoice })
				.Cast<PXResult<SOOrderShipment, SOOrder>>()
				.ToList();

			var (links, orders) = orderShipments.UnZip(
				r => PXCache<SOOrderShipment>.CreateCopy(r),
				r => r.GetItem<SOOrder>(),
				(ls, rs) => (ls.ToList(), rs.ToList()));

			SOOrderShipment ChangeReleased(SOOrderShipment sosh, bool isReleased)
			{
				sosh.InvoiceReleased = isReleased;
				return Items.Update(sosh);
			}

			if (isCancellationInvoice)
			{
				links =
					links
					.Select(r => ChangeReleased(r, false))
					.ToList();

				var cancelledInvoice = SOInvoice.PK.Find(this, boundInvoice.DocType, boundInvoice.RefNbr);
				SOInvoice.Events
					.Select(e => e.InvoiceCancelled)
					.FireOn(this, cancelledInvoice);

				var linksallocated =
					links
					.Where(r => r.OrderTaxAllocated == true)
					.ToList();

				links =
					links
					.Select(r => r.UnlinkInvoice(this))
					.ToList();

				ARInvoice existingCorrectionInvoice =
					PXSelect<ARInvoice,
					Where<ARInvoice.origDocType.IsEqual<ARInvoice.origDocType.FromCurrent>.
						And<ARInvoice.origRefNbr.IsEqual<ARInvoice.origRefNbr.FromCurrent>>.
						And<ARInvoice.isCorrection.IsEqual<True>>>>
					.SelectSingleBound(this, new[] { arDoc });

				if (existingCorrectionInvoice != null)
				{
					var correctionInvoice = SOInvoice.PK.Find(this, existingCorrectionInvoice.DocType, existingCorrectionInvoice.RefNbr);
					links =
						links
						.Select(r => r.LinkInvoice(correctionInvoice, this))
						.ToList();
				}
				else
				{
					foreach (var group in orders.GroupBy(order => (order.OrderType, order.OrderNbr)))
					{
						SOOrder order = group.First();
						if (linksallocated.Exists(_ => _.OrderType == order.OrderType && _.OrderNbr == order.OrderNbr))
							ResetUnbilledTaxes(order);
					}
				}
			}
			else
			{
				links =
					links
					.Select(r => ChangeReleased(r, true))
					.ToList();
				// note that SOInvoice.Events.InvoiceReleased will be fired outside
			}

			if (orderShipments.Any())
				processed.Add(arDoc);

			return orderShipments.ToList();
		}

		public void UpdateApplications(ARRegister arDoc, IEnumerable<SOOrderShipment> orderShipments)
		{
			bool isCancellationInvoice = arDoc.IsCancellation == true;
			bool isCorrectionInvoice = arDoc.IsCorrection == true;

			if (!isCancellationInvoice && !isCorrectionInvoice)
			{
				foreach (var group in orderShipments.GroupBy(os => (os.OrderType, os.OrderNbr)))
				{
					SOOrderType otype = SOOrderType.PK.Find(this, group.Key.OrderType);
					SOOrder updatedOrder = (SOOrder)Caches[typeof(SOOrder)]
						.Locate(new SOOrder { OrderType = group.Key.OrderType, OrderNbr = group.Key.OrderNbr });
					if (updatedOrder == null)
					{
						updatedOrder = SelectFrom<SOOrder>
							.Where<SOOrder.orderNbr.IsEqual<P.AsString>
								.And<SOOrder.orderType.IsEqual<P.AsString.ASCII>>>
							.View
							.SelectSingleBound(this, null, group.Key.OrderNbr, group.Key.OrderType);
					}

					if ((updatedOrder.Completed == true || otype.RequireShipping == false)
						&& updatedOrder.ShipmentCntr <= updatedOrder.ReleasedCntr
						&& updatedOrder.UnbilledMiscTot == 0m)
					{
						foreach (SOAdjust adj in Adjustments.Select(updatedOrder.OrderType, updatedOrder.OrderNbr))
						{
							SOAdjust adjcopy = PXCache<SOAdjust>.CreateCopy(adj);
							adjcopy.CuryAdjdAmt = 0m;
							adjcopy.CuryAdjgAmt = 0m;
							adjcopy.AdjAmt = 0m;
							Adjustments.Update(adjcopy);
						}
					}
				}
			}
		}

		private void UpdateBalance(PXCache s, PXRowUpdatedEventArgs e)
		{
			if (!(e.Row is SOOrder row))
				return;

			if (e.OldRow is SOOrder oldRow)
			{
				ARReleaseProcess.UpdateARBalances(this, oldRow, -oldRow.UnbilledOrderTotal, -oldRow.OpenOrderTotal);
			}
			ARReleaseProcess.UpdateARBalances(this, row, row.UnbilledOrderTotal, row.OpenOrderTotal);
		}

		protected virtual void ResetUnbilledTaxes(SOOrder order)
		{
			RowUpdated.AddHandler<SOOrder>(UpdateBalance);
			try
			{
				order = (SOOrder)this.Caches<SOOrder>().Locate(order);

				SOOrder copy = PXCache<SOOrder>.CreateCopy(order);

				order.CuryOpenTaxTotal = 0m;
				order.CuryUnbilledTaxTotal = 0m;

				foreach (SOTaxTran tax in Taxes.Select(order.OrderType, order.OrderNbr))
				{
					tax.CuryUnbilledTaxableAmt = order.CuryUnbilledLineTotal == 0 || order.BilledCntr + order.ReleasedCntr > 0 && order.OrderTaxAllocated == true ? 0m : tax.CuryTaxableAmt;
					order.CuryUnbilledTaxTotal +=
						tax.CuryUnbilledTaxAmt = order.CuryUnbilledLineTotal == 0 || order.BilledCntr + order.ReleasedCntr > 0 && order.OrderTaxAllocated == true ? 0m : tax.CuryTaxAmt;

					tax.CuryUnshippedTaxableAmt = order.CuryOpenLineTotal == 0 || order.BilledCntr + order.ReleasedCntr > 0 && order.OrderTaxAllocated == true ? 0m : tax.CuryTaxableAmt;
					order.CuryOpenTaxTotal +=
						tax.CuryUnshippedTaxAmt = order.CuryOpenLineTotal == 0 || order.BilledCntr + order.ReleasedCntr > 0 && order.OrderTaxAllocated == true ? 0m : tax.CuryTaxAmt;

					Taxes.Update(tax);
				}

				order.CuryOpenOrderTotal += order.CuryOpenTaxTotal - copy.CuryOpenTaxTotal;
				order.CuryUnbilledOrderTotal += order.CuryUnbilledTaxTotal - copy.CuryUnbilledTaxTotal;

				this.Caches<SOOrder>().Update(order);
			}
			finally
			{
				RowUpdated.RemoveHandler<SOOrder>(UpdateBalance);
			}
		}

		public virtual void CompleteMiscLines(
			ARRegister ardoc,
			List<SOOrderShipment> directShipmentsToCreate)
		{
			RowUpdated.AddHandler<SOOrder>(UpdateBalance);
			try
			{
				var miscTransLinkedToOrder = SelectFrom<ARTran>
					.Where<ARTran.FK.Invoice.SameAsCurrent
						.And<ARTran.sOOrderNbr.IsNotNull>
						.And<ARTran.lineType.IsEqual<SOLineType.miscCharge>>>
					.View.ReadOnly.SelectMultiBound(this, new[] { ardoc })
					.RowCast<ARTran>()
					.ToList();

				foreach (var transByOrder in miscTransLinkedToOrder.GroupBy(t => (OrderType: t.SOOrderType, OrderNbr: t.SOOrderNbr)))
				{
					var orderKey = transByOrder.Key;
					foreach (SOMiscLine2 line in MiscLines.Select(orderKey.OrderType, orderKey.OrderNbr))
					{
						if (transByOrder.All(tran => tran.SOOrderLineNbr != line.LineNbr))
							continue;

						line.CuryUnbilledAmt = 0m;
						line.UnbilledQty = 0m;
						line.Completed = true;
						MiscLines.Update(line);
					}

					if (transByOrder.Any(tran => tran.InvtMult != 0))
					{
						directShipmentsToCreate.Add(
							SOOrderShipment.FromDirectInvoice(ardoc, orderKey.OrderType, orderKey.OrderNbr));
					}
				}
			}
			finally
			{
				RowUpdated.RemoveHandler<SOOrder>(UpdateBalance);
			}
		}


		public virtual void OnInvoiceReleased(ARRegister ardoc, List<PXResult<SOOrderShipment, SOOrder>> orderShipments)
        {
			CompleteSOLinesAndSplits(ardoc, orderShipments);
		}
	}
}
