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
using PX.Objects.AR;
using PX.Objects.CS;
using System;
using System.Collections.Generic;

namespace PX.Objects.SO.GraphExtensions.ARPaymentEntryExt
{
	public class AffectedSOOrdersWithCreditLimitExt : AffectedSOOrdersWithCreditLimitExtBase<ARPaymentEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.distributionModule>();
		}

		private HashSet<SOOrder> ordersChangedDuringPersist;

		public override void Persist(Action basePersist)
		{
			ordersChangedDuringPersist = new HashSet<SOOrder>(Base.Caches<SOOrder>().GetComparer());
			base.Persist(basePersist);
		}

		protected virtual void _(Events.RowUpdated<SOOrder> args)
		{
			if (ordersChangedDuringPersist != null && args.Row.IsFullyPaid != args.OldRow.IsFullyPaid)
				ordersChangedDuringPersist.Add(args.Row);
		}

		protected override IEnumerable<SOOrder> GetLatelyAffectedEntities() => ordersChangedDuringPersist;
		protected override void OnProcessed(SOOrderEntry foreignGraph) => ordersChangedDuringPersist = null;
	}
}
