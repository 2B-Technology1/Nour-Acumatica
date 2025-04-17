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

using PX.Data;
using PX.Objects.SO;
using PX.Objects.Extensions;

namespace PX.Objects.RQ.GraphExtensions.SOOrderEntryExt
{
	public class AffectedRQRequisitionOrders : ProcessAffectedEntitiesInPrimaryGraphBase<AffectedRQRequisitionOrders, SOOrderEntry, RQRequisition, RQRequisitionEntry>
	{
		private readonly Dictionary<string, (RQRequisitionOrder Link, SOOrder Order)> orders = new Dictionary<string, (RQRequisitionOrder, SOOrder)>();
		private PXCache<RQRequisition> requisitions => Base.Caches<RQRequisition>();

		protected override bool PersistInSameTransaction => false;

		protected override bool EntityIsAffected(RQRequisition entity)
		{
			if (!Equals(requisitions.GetValueOriginal<RQRequisition.quoted>(entity), entity.Quoted))
			{
				var link = Base.Caches<RQRequisitionOrder>().Deleted.Cast<RQRequisitionOrder>().FirstOrDefault(o => o.ReqNbr == entity.ReqNbr && o.OrderCategory == RQOrderCategory.SO);
				if (link != null)
				{
					var order = SOOrder.PK.Find(Base, link.OrderType, link.OrderNbr);
					orders[entity.ReqNbr] = (link, order);
					return true;
				}
			}
			return false;
		}

		protected override void ProcessAffectedEntity(RQRequisitionEntry primaryGraph, RQRequisition entity)
		{
			if (orders.TryGetValue(entity.ReqNbr, out var pair) && pair.Link != null && pair.Order != null)
				RQRequisitionOrder.Events.Select(e => e.SOOrderUnlinked).FireOn(primaryGraph, pair.Link, pair.Order);
		}

		protected override RQRequisition ActualizeEntity(RQRequisitionEntry primaryGraph, RQRequisition entity)
			=> primaryGraph.Document.Search<RQRequisition.reqNbr>(entity.ReqNbr);
	}
}
