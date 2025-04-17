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
using PX.Data.BQL.Fluent;
using PX.Objects.IN;

namespace PX.Objects.SO.GraphExtensions.SOOrderEntryExt
{
	public class SpecialOrderCostCenterSupport : SpecialOrderCostCenterSupport<SOOrderEntry, SOLine>
	{
		public static bool IsActive()
			=> PXAccess.FeatureInstalled<CS.FeaturesSet.specialOrders>();

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDBDefault(typeof(SOOrder.orderNbr))]
		protected virtual void _(Events.CacheAttached<INCostCenter.sOOrderNbr> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[SOCostCenterPersisting]
		protected virtual void _(Events.CacheAttached<INCostCenter.sOOrderLineNbr> e) { }

		public override IEnumerable<Type> GetFieldsDependOn()
		{
			yield return typeof(SOLine.isSpecialOrder);
			yield return typeof(SOLine.siteID);
			yield return typeof(SOLine.operation);
		}

		public override bool IsSpecificCostCenter(SOLine line)
			=> line.IsSpecialOrder == true
			&& line.SiteID != null
			&& line.Behavior != SOBehavior.QT;

		protected override CostCenterKeys GetCostCenterKeys(SOLine line)
		{
			if (line.Operation == SOOperation.Receipt || line.Behavior == SOBehavior.TR)
			{
				if (string.IsNullOrEmpty(line.OrigOrderType) || string.IsNullOrEmpty(line.OrigOrderNbr) || line.OrigLineNbr == null)
				{
					throw new PXInvalidOperationException();
				}
				return new CostCenterKeys
				{
					SiteID = line.SiteID,
					OrderType = line.OrigOrderType,
					OrderNbr = line.OrigOrderNbr,
					LineNbr = line.OrigLineNbr,
				};
			}
			else
			{
				return new CostCenterKeys
				{
					SiteID = line.SiteID,
					OrderType = line.OrderType,
					OrderNbr = line.OrderNbr,
					LineNbr = line.LineNbr,
				};
			}
		}

		protected virtual void _(Events.FieldUpdated<SOLineSplit, SOLineSplit.siteID> e)
		{
			if (e.Row.SiteID != null && e.Row.CostCenterID.IsNotIn(null, CostCenter.FreeStock))
			{
				SOLine line = PXParentAttribute.SelectParent<SOLine>(e.Cache, e.Row);
				if (line?.IsSpecialOrder == true)
				{
					var costCenterKeys = GetCostCenterKeys(line);
					costCenterKeys.SiteID = e.Row.SiteID;
					e.Cache.SetValueExt<SOLineSplit.costCenterID>(e.Row, FindOrCreateCostCenter(costCenterKeys));
				}
			}
		}

		private class SOCostCenterPersistingAttribute : PXEventSubscriberAttribute, IPXRowPersistingSubscriber
		{
			public void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
			{
				if (e.Row is INCostCenter row)
				{
					row.CostCenterCD = string.Format("{0} {1} {2}", row.SOOrderType.Trim(), row.SOOrderNbr.Trim(), row.SOOrderLineNbr);
				}
			}
		}
	}
}
