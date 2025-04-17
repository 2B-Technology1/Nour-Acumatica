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
using PX.Objects.PO;

namespace PX.Objects.IN.GraphExtensions.InventoryItemMaintBaseExt
{
	public class MultipleBaseCurrencyExt : PXGraphExtension<InventoryItemMaintBase>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictorWithParameters(typeof(Where<INSite.baseCuryID, Equal<Current2<InventoryItemCurySettings.curyID>>,
			Or<INSite.baseCuryID, Equal<Current<AccessInfo.baseCuryID>>>>),
			Messages.ItemDefaultSiteBaseCurrencyDiffers,
				typeof(INSite.branchID), typeof(INSite.siteCD), typeof(Current<AccessInfo.branchID>))]
		protected virtual void _(Events.CacheAttached<InventoryItemCurySettings.dfltSiteID> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<Vendor.baseCuryID, Equal<Current<AccessInfo.baseCuryID>>,
			Or<Vendor.baseCuryID, IsNull>>),
			Messages.POVendorBaseCurrencyDiffers, typeof(Vendor.acctCD))]
		protected virtual void _(Events.CacheAttached<POVendorInventory.vendorID> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictorWithParameters(typeof(Where<INSite.baseCuryID, Equal<Current2<INItemRep.curyID>>,
			Or<INSite.baseCuryID, Equal<Current<AccessInfo.baseCuryID>>>>),
			Messages.ItemDefaultSiteBaseCurrencyDiffers,
				typeof(INSite.branchID), typeof(INSite.siteCD), typeof(Current<AccessInfo.branchID>))]
		protected virtual void _(Events.CacheAttached<INItemRep.replenishmentSourceSiteID> e)
		{
		}

		protected virtual void _(Events.RowPersisting<InventoryItemCurySettings> e)
		{
			if (e.Operation.Command().IsNotIn(PXDBOperation.Insert, PXDBOperation.Update))
				return;

			e.Cache.VerifyFieldAndRaiseException<InventoryItemCurySettings.dfltSiteID>(e.Row);
		}

		protected virtual void _(Events.RowPersisting<INItemRep> e)
		{
			if (e.Operation.Command().IsNotIn(PXDBOperation.Insert, PXDBOperation.Update))
				return;

			e.Cache.VerifyFieldAndRaiseException<INItemRep.replenishmentSourceSiteID>(e.Row);
		}

	}
}
