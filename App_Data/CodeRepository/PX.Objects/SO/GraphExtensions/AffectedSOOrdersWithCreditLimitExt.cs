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
using PX.Objects.Extensions;

namespace PX.Objects.SO.GraphExtensions
{
	public class AffectedSOOrdersWithCreditLimitExtBase<TGraph> : ProcessAffectedEntitiesInPrimaryGraphBase<AffectedSOOrdersWithCreditLimitExtBase<TGraph>, TGraph, SOOrder, SOOrderEntry>
		where TGraph : PXGraph
	{
		private PXCache<SOOrder> orders => Base.Caches<SOOrder>();
		protected override bool PersistInSameTransaction => false;

		protected override bool EntityIsAffected(SOOrder order)
		{
			bool? originalFullyPaid = (bool?)orders.GetValueOriginal<SOOrder.isFullyPaid>(order);

			return (originalFullyPaid != order.IsFullyPaid);
		}

		protected override void ProcessAffectedEntity(SOOrderEntry orderEntry, SOOrder order)
		{
			if (order.CreditHold == true && order.IsFullyPaid == true)
			{
				order.SatisfyCreditLimitByPayment(orderEntry);
			}
			else if (order.CreditHold != true && order.IsFullyPaid != true)
			{
				RunCreditLimitVerification(orderEntry, order);
			}
		}

		protected virtual void RunCreditLimitVerification(SOOrderEntry orderEntry, SOOrder order)
		{
			orderEntry.Document.Update(order);
		}

		protected override SOOrder ActualizeEntity(SOOrderEntry orderEntry, SOOrder order)
			=> orderEntry.Document.Search<SOOrder.orderNbr>(order.OrderNbr, order.OrderType);
	}
}
