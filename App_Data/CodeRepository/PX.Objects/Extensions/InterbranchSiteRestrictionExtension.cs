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
using PX.Objects.IN;
using System.Collections.Generic;

namespace PX.Objects.Extensions
{
	public abstract class InterbranchSiteRestrictionExtension<TGraph, THeaderBranchField, TDetail, TDetailSiteField> : PXGraphExtension<TGraph>
		where TGraph : PXGraph
		where THeaderBranchField : class, IBqlField
		where TDetail : class, IBqlTable, new()
		where TDetailSiteField : class, IBqlField
	{
		protected PXCache HeaderCache => Base.Caches[BqlCommand.GetItemType(typeof(THeaderBranchField))];
		protected PXCache DetailsCache => Base.Caches[BqlCommand.GetItemType(typeof(TDetailSiteField))];

		protected abstract IEnumerable<TDetail> GetDetails();

		protected virtual void _(Events.FieldVerifying<THeaderBranchField> e)
		{
			var newBranchId = (int?)e.NewValue;
			if (newBranchId == null || PXAccess.FeatureInstalled<FeaturesSet.interBranch>())
				return;

			bool inappropriateSiteIsFound = false;
			foreach (var detail in GetDetails())
			{
				var detailSite = (INSite)PXSelectorAttribute.Select(DetailsCache, detail, typeof(TDetailSiteField).Name);
				if (detailSite == null || PXAccess.IsSameParentOrganization(newBranchId, detailSite.BranchID))
					continue;

				inappropriateSiteIsFound = true;

				// In order to force validation in RowPersisting method.
				DetailsCache.MarkUpdated(detail);
			}

			if (inappropriateSiteIsFound)
				RaiseBranchFieldWarning((int?)e.NewValue, Common.Messages.InappropriateSiteOnDetailsTab);
		}

		protected void RaiseBranchFieldWarning(int? newBranch, string message)
		{
			HeaderCache.RaiseExceptionHandling<THeaderBranchField>(HeaderCache.Current, newBranch,
					new PXSetPropertyException(message, PXErrorLevel.Warning));
		}

		protected virtual void _(Events.RowPersisting<TDetail> e)
		{
			if (e.Operation.Command() == PXDBOperation.Delete)
				return;

			var headerBranchId = (int?)HeaderCache.GetValue<THeaderBranchField>(HeaderCache.Current);
			if (headerBranchId == null || PXAccess.FeatureInstalled<FeaturesSet.interBranch>())
				return;

			var detailSite = (INSite)PXSelectorAttribute.Select(DetailsCache, e.Row, typeof(TDetailSiteField).Name);
			if (detailSite == null || PXAccess.IsSameParentOrganization(headerBranchId, detailSite.BranchID))
				return;

			DetailsCache.RaiseExceptionHandling<INTran.siteID>(e.Row, detailSite.SiteCD, new PXSetPropertyException(Common.Messages.InterBranchFeatureIsDisabled));
		}
	}
}
