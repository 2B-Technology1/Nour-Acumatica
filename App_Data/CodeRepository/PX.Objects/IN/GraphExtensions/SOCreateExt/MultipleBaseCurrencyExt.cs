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
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.Common.Bql;
using PX.Objects.CS;
using PX.Objects.SO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SOCreateFilter = PX.Objects.SO.SOCreate.SOCreateFilter;
using SOFixedDemand = PX.Objects.SO.SOCreate.SOFixedDemand;

namespace PX.Objects.IN.GraphExtensions.SOCreateExt
{
	public class MultipleBaseCurrencyExt : PXGraphExtension<SOCreate>
	{
		public static bool IsActive()
			=> PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<Current2<SOCreateFilter.sourceSiteID>, IsNull,
			Or<INSite.baseCuryID, EqualSiteBaseCuryID<Current2<SOCreateFilter.sourceSiteID>>>>),
			Messages.ReplenishmentSiteDiffers, typeof(INSite.branchID), typeof(INSite.siteCD))]
		protected virtual void _(Events.CacheAttached<SOCreateFilter.siteID> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<Current2<SOCreateFilter.siteID>, IsNull,
			Or<INSite.baseCuryID, EqualSiteBaseCuryID<Current2<SOCreateFilter.siteID>>>>),
			Messages.ReplenishmentSiteDiffers, typeof(INSite.branchID), typeof(INSite.siteCD))]
		protected virtual void _(Events.CacheAttached<SOCreateFilter.sourceSiteID> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<Current2<SOFixedDemand.siteID>, IsNull,
			Or<INSite.baseCuryID, EqualSiteBaseCuryID<Current2<SOFixedDemand.siteID>>>>),
			Messages.ReplenishmentSiteDiffers, typeof(INSite.branchID), typeof(INSite.siteCD))]
		protected virtual void _(Events.CacheAttached<SOFixedDemand.sourceSiteID> e)
		{
		}

	}
}
