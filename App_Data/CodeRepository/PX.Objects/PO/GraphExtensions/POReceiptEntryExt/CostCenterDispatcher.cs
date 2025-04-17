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

namespace PX.Objects.PO.GraphExtensions.POReceiptEntryExt
{
	public class CostCenterDispatcher : CostCenterDispatcher<POReceiptEntry, POReceiptLine, POReceiptLine.costCenterID>
	{
		public static bool IsActive()
			=> PXAccess.FeatureInstalled<CS.FeaturesSet.projectAccounting>() ||
			PXAccess.FeatureInstalled<CS.FeaturesSet.specialOrders>();

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[CostCenterDBDefault]
		protected virtual void _(Events.CacheAttached<POReceiptLine.costCenterID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDBDefault(typeof(INCostCenter.costCenterID), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<POReceiptLine.specialOrderCostCenterID> e) { }
	}
}
