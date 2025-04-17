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
using PX.Objects.AP;
using PX.Objects.Common.Bql;
using PX.Objects.CS;

namespace PX.Objects.IN.GraphExtensions.INReplenishmentCreateExt
{
	public class MultipleBaseCurrencyExt : PXGraphExtension<INReplenishmentCreate>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<INSite.baseCuryID, EqualSiteBaseCuryID<Current2<INReplenishmentFilter.replenishmentSiteID>>>),
			Messages.ReplenishmentSiteDiffers, typeof(INSite.branchID), typeof(INSite.siteCD))]
		protected virtual void _(Events.CacheAttached<INReplenishmentItem.replenishmentSourceSiteID> eventArgs)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<Vendor.baseCuryID, EqualSiteBaseCuryID<Current2<INReplenishmentFilter.replenishmentSiteID>>,
			Or<Vendor.baseCuryID, IsNull>>),
			Messages.ReplenishmentVendorDiffers, typeof(Vendor.acctCD))]
		protected virtual void _(Events.CacheAttached<INReplenishmentItem.preferredVendorID> eventArgs)
		{
		}

	}
}
