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
using PX.Data;
using PX.Objects.Extensions;

namespace PX.Objects.SO.GraphExtensions
{
	public abstract class AffectedSOOrdersWithPrepaymentRequirementsBase<TSelf, TGraph> : ProcessAffectedEntitiesInPrimaryGraphBase<TSelf, TGraph, SOOrder, SOOrderEntry>
		where TSelf : ProcessAffectedEntitiesInPrimaryGraphBase<TSelf, TGraph, SOOrder, SOOrderEntry>
		where TGraph : PXGraph
	{
		private PXCache<SOOrder> orders => Base.Caches<SOOrder>();
		protected HashSet<SOOrder> ordersChangedDuringPersist;

		protected override bool PersistInSameTransaction => false;

		public override void Persist(Action basePersist)
		{
			ordersChangedDuringPersist = new HashSet<SOOrder>(Base.Caches<SOOrder>().GetComparer());
			base.Persist(basePersist);
		}

		protected virtual void _(Events.RowUpdated<SOOrder> args)
		{
			if (ordersChangedDuringPersist != null && !args.Cache.ObjectsEqual<SOOrder.curyPrepaymentReqAmt, SOOrder.curyPaymentOverall>(args.Row, args.OldRow))
				ordersChangedDuringPersist.Add(args.Row);
		}

		protected override bool EntityIsAffected(SOOrder order)
		{
			return
				!Equals(orders.GetValueOriginal<SOOrder.curyPrepaymentReqAmt>(order), order.CuryPrepaymentReqAmt) ||
				!Equals(orders.GetValueOriginal<SOOrder.curyPaymentOverall>(order), order.CuryPaymentOverall);
		}

		protected override void ProcessAffectedEntity(SOOrderEntry orderEntry, SOOrder order)
		{
			if (order.CuryPaymentOverall >= order.CuryPrepaymentReqAmt)
				order.SatisfyPrepaymentRequirements(orderEntry);
			else
				order.ViolatePrepaymentRequirements(orderEntry);
		}

		protected override IEnumerable<SOOrder> GetLatelyAffectedEntities() => ordersChangedDuringPersist;
		protected override void OnProcessed(SOOrderEntry foreignGraph) => ordersChangedDuringPersist = null;

		protected override SOOrder ActualizeEntity(SOOrderEntry orderEntry, SOOrder order)
			=> orderEntry.Document.Search<SOOrder.orderNbr>(order.OrderNbr, order.OrderType);
	}
}
