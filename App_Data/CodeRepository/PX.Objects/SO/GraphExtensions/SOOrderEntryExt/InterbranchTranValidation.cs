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

namespace PX.Objects.SO.GraphExtensions.SOOrderEntryExt
{
	public class InterbranchTranValidation
		: Extensions.InterbranchSiteRestrictionExtension<SOOrderEntry, SOOrder.branchID, SOLine, SOLine.siteID>
	{
		protected override void _(Events.FieldVerifying<SOOrder.branchID> e)
		{
			SOOrder order = (SOOrder)e.Row;

			if (order?.Behavior == SOBehavior.QT)
				return;

			if (order?.IsTransferOrder == true && !IsDestinationSiteValid(order, (int?)e.NewValue))
			{
				RaiseBranchFieldWarning((int?)e.NewValue, Messages.InappropriateDestSite);
				return; // To avoid warning override. Let Destination Warehouse warning go first.
			}

			base._(e);
		}

		protected override void _(Events.RowPersisting<SOLine> e)
		{
			var headerRow = Base.CurrentDocument.Current;
			if (headerRow?.Behavior == SOBehavior.QT || e.Row.LineType == SOLineType.MiscCharge)
				return;

			base._(e);
		}

		protected override IEnumerable<SOLine> GetDetails()
		{
			return Base.Transactions.Select().RowCast<SOLine>();
		}
		
		protected virtual bool IsDestinationSiteValid(SOOrder row, int? newBranchId)
		{
			if (newBranchId == null || PXAccess.FeatureInstalled<FeaturesSet.interBranch>())
				return true;

			var destSite = INSite.PK.Find(Base, row.DestinationSiteID);
			if (destSite == null || PXAccess.IsSameParentOrganization(newBranchId, destSite.BranchID))
				return true;

			return false;
		}

		protected virtual void SOOrder_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			if (e.Operation.Command() == PXDBOperation.Delete)
				return;

			var order = (SOOrder)e.Row;
			if (order?.IsTransferOrder != true || order.BranchID == null || PXAccess.FeatureInstalled<FeaturesSet.interBranch>())
				return;

			var destSite = INSite.PK.Find(Base, order.DestinationSiteID);
			if (destSite == null || PXAccess.IsSameParentOrganization(order.BranchID, destSite.BranchID))
				return;

			cache.RaiseExceptionHandling<SOOrder.destinationSiteID>(e.Row, destSite.SiteCD, new PXSetPropertyException(Common.Messages.InterBranchFeatureIsDisabled));
		}
	}
}
