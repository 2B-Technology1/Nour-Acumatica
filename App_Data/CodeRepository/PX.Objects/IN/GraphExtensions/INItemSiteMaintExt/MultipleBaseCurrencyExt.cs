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
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.Common.Bql;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.IN.Attributes;

namespace PX.Objects.IN.GraphExtensions.INItemSiteMaintExt
{
	public class MultipleBaseCurrencyExt : PXGraphExtension<INItemSiteMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictorWithParameters(
			typeof(Where<INSite.baseCuryID, EqualSiteBaseCuryID<Current<INItemSite.siteID>>>),
			Messages.ReplenishmentSourceSiteBaseCurrencyDiffers, 
			typeof(Selector<INItemSite.siteID, INSite.branchID>), typeof(INSite.branchID), typeof(INSite.siteCD))]
		protected virtual void _(Events.CacheAttached<INItemSite.replenishmentSourceSiteID> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<Vendor.baseCuryID, EqualSiteBaseCuryID<Current2<INItemSite.siteID>>,
			Or<Vendor.baseCuryID, IsNull>>),
			Messages.ReplenishmentVendorDiffers, typeof(Vendor.acctCD))]
		protected virtual void _(Events.CacheAttached<INItemSite.preferredVendorID> e)
		{
		}

		protected virtual void _(Events.RowPersisting<INItemSite> e)
		{
			if (e.Operation.Command().IsNotIn(PXDBOperation.Insert, PXDBOperation.Update))
				return;

			e.Cache.VerifyFieldAndRaiseException<INItemSite.replenishmentSourceSiteID>(e.Row);
		}
	}
}
