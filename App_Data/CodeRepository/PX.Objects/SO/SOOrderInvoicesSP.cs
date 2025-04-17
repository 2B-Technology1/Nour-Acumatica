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
using PX.Objects.GL;

namespace PX.Objects.SO
{
	[TableAndChartDashboardType]
	public class SOOrderInvoicesSP : PXGraph<SOOrderInvoicesSP>
	{
		public PXCancel<SOOrderInvoicesSPFilter> Cancel;
		public PXFilter<SOOrderInvoicesSPFilter> Filter;

		[PXFilterable]
		public SelectFrom<SOOrderInvoicesSPInqResult>
			.OrderBy<SOOrderInvoicesSPInqResult.refNbr.Desc, SOOrderInvoicesSPInqResult.orderNbr.Desc>
			.View.ReadOnly Invoices;

		protected virtual IEnumerable invoices()
		{
			var result = new PXDelegateResult
			{
				IsResultFiltered = true,
				IsResultTruncated = false,
				IsResultSorted = true
			};

			if (string.IsNullOrEmpty(Filter.Current.OrderNbr))
				return result;

			var order = SOOrder.PK.Find(this, Filter.Current.OrderType, Filter.Current.OrderNbr);

			var resultList = new HashSet<SOOrderInvoicesSPInqResult>(Invoices.Cache.GetComparer());
			var fields = new Type[]
			{
				typeof(ARInvoice.docType), typeof(ARInvoice.refNbr), typeof(ARInvoice.status), typeof(ARInvoice.docDate),
				typeof(ARInvoice.dueDate), typeof(ARInvoice.finPeriodID), typeof(ARInvoice.curyOrigDocAmt),
				typeof(ARInvoice.curyDocBal), typeof(ARInvoice.curyID), typeof(ARInvoice.customerID),
				typeof(ARTran.sOOrderType), typeof(ARTran.sOOrderNbr)
			};

			bool hasInvoiceRecords = false;
			var invoicesQ = GetInvoicesQuery(order);
			using (var scope = new PXFieldScope(invoicesQ.View, fields))
			{
				int startRow = 0, maxRows = 0, totalRows = 0;
				foreach (var invoiceRec in invoicesQ.View.Select(new[] { order }, null, null,
						PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, maxRows, ref totalRows)
					.Cast<PXResult<ARInvoice>>())
				{
					ARTran tran = invoiceRec.GetItem<ARTran>();
					resultList.Add(CreateUIRecord(invoiceRec,
						new SOOrder { OrderType = tran.SOOrderType, OrderNbr = tran.SOOrderNbr }));

					hasInvoiceRecords = true;
				}
			}

			bool hasReturnRecords = false;
			if (order.Behavior.IsIn(SOBehavior.RM, SOBehavior.CM, SOBehavior.BL))
			{
				var returnInvoicesQ = GetReturnsQuery(order);
				using (var scope = new PXFieldScope(returnInvoicesQ.View, fields))
				{
					int startRow = 0, maxRows = 0, totalRows = 0;
					foreach (var invoiceRec in returnInvoicesQ.View.Select(new[] { order }, null, null,
							PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, maxRows, ref totalRows)
						.Cast<PXResult<ARInvoice>>())
					{
						ARTran tran = invoiceRec.GetItem<ARTran>();
						var returnRecord = CreateUIRecord(invoiceRec,
							new SOOrder { OrderType = tran.SOOrderType, OrderNbr = tran.SOOrderNbr });

						if (!resultList.Contains(returnRecord))
						{
							resultList.Add(returnRecord);
							hasReturnRecords = true;
						}
					}
				}
			}

			result.AddRange(resultList);
			if (hasInvoiceRecords && hasReturnRecords)
			{
				result.IsResultFiltered = false;
				result.IsResultSorted = false;
			}

			return result;
		}

		protected PXSelectBase<ARInvoice> GetInvoicesQuery(SOOrder order)
		{
			var query = new SelectFrom<ARInvoice>
				.InnerJoin<ARTran>.On<ARTran.FK.Invoice>
				.Where<ARInvoice.origModule.IsEqual<BatchModule.moduleSO>
					.And<ARTran.FK.SOOrder.SameAsCurrent>>
				.AggregateTo<GroupBy<ARInvoice.docType, GroupBy<ARInvoice.refNbr>>>
				.OrderBy<ARInvoice.refNbr.Desc, ARTran.sOOrderNbr.Desc>
				.View.ReadOnly(this);

			if (order.Behavior == SOBehavior.BL)
			{
				query.View.Join<InnerJoin<SOBlanketOrderLink,
					On<SOBlanketOrderLink.orderType.IsEqual<ARTran.sOOrderType>
					.And<SOBlanketOrderLink.orderNbr.IsEqual<ARTran.sOOrderNbr>>>>>();

				query.View.WhereNew<Where<ARInvoice.origModule.IsEqual<BatchModule.moduleSO>
					.And<SOBlanketOrderLink.FK.BlanketOrder.SameAsCurrent>>>();
			}

			return query;
		}

		protected PXSelectBase<ARInvoice> GetReturnsQuery(SOOrder order)
		{
			var query = new SelectFrom<ARInvoice>
				.InnerJoin<ARTran>.On<ARTran.FK.Invoice>
				.InnerJoin<SOLine> // Please pay attention that SOLine does not belong to SOOrder joined above.
					.On<SOLine.invoiceType.IsEqual<ARInvoice.docType>.And<SOLine.invoiceNbr.IsEqual<ARInvoice.refNbr>>>
				.Where<ARInvoice.origModule.IsEqual<BatchModule.moduleSO>
					.And<SOLine.FK.Order.SameAsCurrent>>
				.AggregateTo<GroupBy<ARInvoice.docType, GroupBy<ARInvoice.refNbr>>>
				.OrderBy<ARInvoice.refNbr.Desc, ARTran.sOOrderNbr.Desc>
				.View.ReadOnly(this);

			if (order.Behavior == SOBehavior.BL)
			{
				query.View.Join<InnerJoin<SOBlanketOrderLink,
					On<SOBlanketOrderLink.orderType.IsEqual<ARTran.sOOrderType>
					.And<SOBlanketOrderLink.orderNbr.IsEqual<ARTran.sOOrderNbr>>>>>();

				query.View.WhereNew<Where<ARInvoice.origModule.IsEqual<BatchModule.moduleSO>
					.And<SOBlanketOrderLink.FK.BlanketOrder.SameAsCurrent>>>();
			}

			return query;
		}

		protected virtual SOOrderInvoicesSPInqResult CreateUIRecord(ARInvoice invoice, SOOrder order)
		{
			var result = new SOOrderInvoicesSPInqResult();

			result.DocType = invoice.DocType;
			result.RefNbr = invoice.RefNbr;
			result.Status = invoice.Status;
			result.DocDate = invoice.DocDate;
			result.CustomerID = invoice.CustomerID;

			result.DueDate = invoice.DueDate;
			result.FinPeriodID = invoice.FinPeriodID;
			result.CuryOrigDocAmt = invoice.CuryOrigDocAmt;
			result.CuryDocBal = invoice.CuryDocBal;
			result.CuryID = invoice.CuryID;

			result.OrderType = order.OrderType;
			result.OrderNbr = order.OrderNbr;

			return result;
		}
	}
}
