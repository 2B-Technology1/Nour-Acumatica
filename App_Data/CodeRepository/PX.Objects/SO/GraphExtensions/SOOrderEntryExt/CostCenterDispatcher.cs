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
using PX.Objects.IN;
using PX.Objects.IN.Attributes;
using PX.Objects.IN.GraphExtensions;

namespace PX.Objects.SO.GraphExtensions.SOOrderEntryExt
{
	public class CostCenterDispatcher : CostCenterDispatcher<SOOrderEntry, SOLine, SOLine.costCenterID>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.projectAccounting>()
				|| PXAccess.FeatureInstalled<CS.FeaturesSet.specialOrders>();
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		[CostCenterDBDefault]
		protected virtual void _(Events.CacheAttached<SOLine.costCenterID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		[CostCenterDBDefault]
		protected virtual void _(Events.CacheAttached<SOLineSplit.costCenterID> e) { }

		protected override void MoveCostCenterViewCacheToTop()
		{
			int costCenterIndex = Base.Views.Caches.IndexOf(typeof(INCostCenter));
			Base.Views.Caches.RemoveAt(costCenterIndex);
			// insert INCostCenter right after SOOrder to be able to sync autonumbered OrderNbr
			int headerIndex = Base.Views.Caches.IndexOf(typeof(SOOrder));
			Base.Views.Caches.Insert(headerIndex + 1, typeof(INCostCenter));
		}

		protected virtual void _(Events.FieldDefaulting<SOLineSplit, SOLineSplit.costCenterID> e)
		{
			e.NewValue = PXParentAttribute.SelectParent<SOLine>(e.Cache, e.Row)?.CostCenterID;
			e.Cancel = true;
		}
	}
}
