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
using PX.Objects.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.GL;
using PX.Objects.IN.Attributes;
using PX.Objects.SP.DAC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data.BQL.Fluent;
using PX.Objects.IN.DAC;
using System.Collections;

namespace SP.Objects.SP.GraphExtensions.PortalSetupMaintExt
{
	public class MultipleBaseCurrenciesExt : PXGraphExtension<PortalSetupMaint>
	{
		public static bool IsActive()
			=> PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictorWithParameters(
			typeof(Where<INSite.baseCuryID, EqualBaseCuryID<Current2<PortalSetup.sellingBranchID>>>),
			PX.Objects.IN.Messages.SiteBaseCurrencyDiffers,
			typeof(INSite.branchID), typeof(INSite.siteCD), typeof(Current2<PortalSetup.sellingBranchID>))]
		protected virtual void _(Events.CacheAttached<PortalSetup.defaultStockItemWareHouse> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictorWithParameters(
			typeof(Where<INSite.baseCuryID, EqualBaseCuryID<Current2<PortalSetup.sellingBranchID>>>),
			PX.Objects.IN.Messages.SiteBaseCurrencyDiffers,
			typeof(INSite.branchID), typeof(INSite.siteCD), typeof(Current2<PortalSetup.sellingBranchID>))]
		protected virtual void _(Events.CacheAttached<PortalSetup.defaultnonStockItemWareHouse> e)
		{
		}

		protected virtual void _(Events.FieldUpdated<PortalSetup, PortalSetup.sellingBranchID> e)
		{
			var newBranch = Branch.PK.Find(Base, e.Row.SellingBranchID);
			var oldBranch = Branch.PK.Find(Base, (int?)e.OldValue);
			if (!string.Equals(newBranch?.BaseCuryID, oldBranch?.BaseCuryID, StringComparison.OrdinalIgnoreCase))
			{
				OnBaseCuryChanged(e.Cache, e.Row);
			}
		}

		protected virtual void OnBaseCuryChanged(PXCache cache, PortalSetup row)
		{
			cache.SetValueExt<PortalSetup.defaultStockItemWareHouse>(row, null);
			cache.SetValueExt<PortalSetup.defaultnonStockItemWareHouse>(row, null);

			foreach (WarehouseReference reference in Base.WarehouseReference.Select())
				Base.Caches[typeof(WarehouseReference)].Delete(reference);

			Base.CRSetupINSite.Cache.Clear();
			Base.CRSetupINSite.Cache.ClearQueryCache();
		}

		protected virtual void _(Events.RowPersisting<PortalSetup> e)
		{
			e.Cache.VerifyFieldAndRaiseException<PortalSetup.defaultStockItemWareHouse>(e.Row);
			e.Cache.VerifyFieldAndRaiseException<PortalSetup.defaultnonStockItemWareHouse>(e.Row);
		}

		[PXOverride]
		public virtual IEnumerable cRSetupINSite(Func<IEnumerable> baseMethod)
		{
			var branch = Branch.PK.Find(Base, Base.CRSetupRecord.Current?.SellingBranchID);

			foreach (var record in baseMethod())
			{
				var site = PXResult.Unwrap<INSite>(record);
				if (site.BaseCuryID.Equals(branch?.BaseCuryID, StringComparison.OrdinalIgnoreCase))
					yield return record;
			}
		}
	}
}
