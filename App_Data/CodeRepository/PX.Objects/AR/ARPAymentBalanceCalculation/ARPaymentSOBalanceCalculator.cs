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
using PX.Objects.CM.Extensions;
using PX.Objects.CS;
using PX.Objects.SO;
using System;
using OrdersToApplyTab = PX.Objects.SO.GraphExtensions.ARPaymentEntryExt.OrdersToApplyTab;

namespace PX.Objects.AR
{
	partial class ARPaymentEntry
	{
		public class ARPaymentSOBalanceCalculator : PXGraphExtension<OrdersToApplyTab, ARPaymentEntry.MultiCurrency, ARPaymentEntry>
		{
			public void CalcBalances(SOAdjust adj, SOOrder invoice, bool isCalcRGOL, bool DiscOnDiscDate)
			{
				new PaymentBalanceCalculator(Base1).CalcBalances(adj.AdjgCuryInfoID, adj.AdjdCuryInfoID, invoice, adj);
				
				if (DiscOnDiscDate)
				{
					CM.PaymentEntry.CalcDiscount(adj.AdjgDocDate, invoice, adj);
				}
				CM.PaymentEntry.WarnDiscount(Base, adj.AdjgDocDate, invoice, adj);

				new PaymentBalanceAjuster(Base1).AdjustBalance(adj);

				if (isCalcRGOL && (adj.Voided != true))
				{
					new PaymentRGOLCalculator(Base1, adj, adj.ReverseGainLoss).Calculate(invoice);
				}
			}

			[PXMergeAttributes(Method = MergeMethod.Append)]
			[PXRemoveBaseAttribute(typeof(CM.PXCurrencyAttribute))]
			[PXCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.docBal))]
			protected virtual void SOOrder_CuryDocBal_CacheAttached(PXCache sender) { }

			public void CalcBalances(SOAdjust adj, bool isCalcRGOL, bool DiscOnDiscDate)
			{
				if (PXTransactionScope.IsConnectionOutOfScope) return;

				foreach (PXResult<SOOrder> res in Base2.SOOrder_CustomerID_OrderType_RefNbr.Select(adj.CustomerID, adj.AdjdOrderType, adj.AdjdOrderNbr))
				{
					SOOrder invoice = PXCache<SOOrder>.CreateCopy(res);

					Base.internalCall = true;

					SOAdjust other = PXSelectGroupBy<SOAdjust,
					Where<SOAdjust.voided, Equal<False>,
					And<SOAdjust.adjdOrderType, Equal<Required<SOAdjust.adjdOrderType>>,
					And<SOAdjust.adjdOrderNbr, Equal<Required<SOAdjust.adjdOrderNbr>>,
					And<Where<SOAdjust.adjgDocType, NotEqual<Required<SOAdjust.adjgDocType>>, Or<SOAdjust.adjgRefNbr, NotEqual<Required<SOAdjust.adjgRefNbr>>>>>>>>,
					Aggregate<GroupBy<SOAdjust.adjdOrderType,
					GroupBy<SOAdjust.adjdOrderNbr, Sum<SOAdjust.curyAdjdAmt, Sum<SOAdjust.adjAmt>>>>>>.Select(Base, adj.AdjdOrderType, adj.AdjdOrderNbr, adj.AdjgDocType, adj.AdjgRefNbr);

					if (other != null && other.AdjdOrderNbr != null)
					{
						invoice.CuryDocBal -= other.CuryAdjdAmt;
						invoice.DocBal -= other.AdjAmt;
					}
					Base.internalCall = false;

					CalcBalances(adj, invoice, isCalcRGOL, DiscOnDiscDate);

					if (Base2.IsApplicationToBlanketOrderWithChild(adj))
					{
						adj.CuryDocBal = 0m;
						adj.DocBal = 0m;
					}
				}
			}

			public virtual void _(Events.FieldUpdating<SOAdjust, SOAdjust.curyDocBal> e)
			{
				if (!Base.internalCall && e.Row != null)
				{
					if (e.Row.AdjdCuryInfoID != null && e.Row.CuryDocBal == null && e.Cache.GetStatus(e.Row) != PXEntryStatus.Deleted)
					{
						CalcBalances(e.Row, false, false);

						// If this event handler is executing in a transaction scope, we read outdated sales orders
						// and this data is automatically put into PXCache. We need to remove sales orders from the cache.
						RemoveNotchangedSalesOrdersFromCache();
					}
					e.NewValue = e.Row.CuryDocBal;
				}
				e.Cancel = true;
			}

			protected virtual void RemoveNotchangedSalesOrdersFromCache()
			{
				if (PXTransactionScope.IsScoped)
				{
					var orderCache = Base2.SOOrder_CustomerID_OrderType_RefNbr.Cache;
					foreach (var order in orderCache.Cached)
					{
						if (orderCache.GetStatus(order) == PXEntryStatus.Notchanged)
							orderCache.Remove(order);
					}
					orderCache.ClearQueryCacheObsolete();
				}
			}

			public virtual void _(Events.FieldUpdated<SOAdjust, SOAdjust.curyAdjgAmt> e)
			{
				CalcBalances(e.Row, true, false);
			}

			public virtual void _(Events.FieldVerifying<SOAdjust, SOAdjust.curyAdjgAmt> e)
			{
				if (e.Row.CuryDocBal == null || e.Row.CuryDiscBal == null)
				{
					CalcBalances(e.Row, false, false);
				}

				if (e.Cancel) return;

				if (e.Row.CuryDocBal == null)
				{
					throw new PXSetPropertyException<SOAdjust.adjdOrderNbr>(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<SOAdjust.adjdOrderNbr>(e.Cache));
				}

				if ((decimal)e.NewValue < 0m)
				{
					throw new PXSetPropertyException(CS.Messages.Entry_GE, ((int)0).ToString());
				}

				if ((decimal)e.Row.CuryDocBal + (decimal)e.Row.CuryAdjgAmt - (decimal)e.NewValue < 0
					&& !Base2.IsApplicationToBlanketOrderWithChild(e.Row))
				{
					throw new PXSetPropertyException(CS.Messages.Entry_LE, ((decimal)e.Row.CuryDocBal + (decimal)e.Row.CuryAdjgAmt).ToString());
				}
			}

			public virtual void _(Events.FieldUpdated<SOAdjust, SOAdjust.curyAdjgDiscAmt> e)
			{
				CalcBalances(e.Row, true, false);
			}

			public virtual void _(Events.FieldVerifying<SOAdjust, SOAdjust.curyAdjgDiscAmt> e)
			{
				SOAdjust adj = e.Row;

				if (adj.CuryDocBal == null || adj.CuryDiscBal == null)
				{
					CalcBalances(e.Row, false, false);
				}

				if (adj.CuryDocBal == null || adj.CuryDiscBal == null)
				{
					throw new PXSetPropertyException<SOAdjust.adjdOrderNbr>(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<SOAdjust.adjdOrderNbr>(e.Cache));
				}

				if ((decimal)e.NewValue < 0m)
				{
					throw new PXSetPropertyException(CS.Messages.Entry_GE, ((int)0).ToString());
				}

				if ((decimal)adj.CuryDiscBal + (decimal)adj.CuryAdjgDiscAmt - (decimal)e.NewValue < 0)
				{
					throw new PXSetPropertyException(CS.Messages.Entry_LE, ((decimal)adj.CuryDiscBal + (decimal)adj.CuryAdjgDiscAmt).ToString());
				}

				if (adj.CuryAdjgAmt != null && (e.Cache.GetValuePending<SOAdjust.curyAdjgAmt>(e.Row) == PXCache.NotSetValue || (decimal?)e.Cache.GetValuePending<SOAdjust.curyAdjgAmt>(e.Row) == adj.CuryAdjgAmt))
				{
					if ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgDiscAmt - (decimal)e.NewValue < 0)
					{
						throw new PXSetPropertyException(CS.Messages.Entry_LE, ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgDiscAmt).ToString());
					}
				}
			}
		}
	}
}
