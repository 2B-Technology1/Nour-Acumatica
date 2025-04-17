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
using PX.Objects.CS;
using PX.Objects.Extensions;
using PX.Objects.PO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PODropShipLinksExt = PX.Objects.PO.GraphExtensions.POOrderEntryExt.DropShipLinksExt;

namespace PX.Objects.SO.GraphExtensions.SOOrderEntryExt
{
	public class AffectedDropShipOrdersBySOOrderEntry : ProcessAffectedEntitiesInPrimaryGraphBase<AffectedDropShipOrdersBySOOrderEntry, SOOrderEntry, SupplyPOOrder, POOrderEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.dropShipments>();
		}

		protected override bool PersistInSameTransaction => true;

		protected override bool EntityIsAffected(SupplyPOOrder entity)
		{
			if (entity.OrderType != POOrderType.DropShip || entity.IsLegacyDropShip == true)
				return false;

			var cache = Base.Caches<SupplyPOOrder>();
			int? activeLinksCountOldValue = (int?)cache.GetValueOriginal<SupplyPOOrder.dropShipActiveLinksCount>(entity);

			return entity.DropShipActiveLinksCount != activeLinksCountOldValue;
		}

		protected override void ProcessAffectedEntity(POOrderEntry primaryGraph, SupplyPOOrder entity)
		{
			POOrder order = POOrder.PK.Find(primaryGraph, entity.OrderType, entity.OrderNbr);
			PODropShipLinksExt ext = primaryGraph.GetExtension<PODropShipLinksExt>();
			ext.UpdateDocumentState(order);
		}

		protected override void ClearCaches(PXGraph graph)
		{
			graph.Caches<SupplyPOOrder>().Clear();
			graph.Caches<SupplyPOOrder>().ClearQueryCache();
		}
	}
}
