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
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.IN.GraphExtensions;
using PX.Objects.SO.DAC.Projections;

namespace PX.Objects.SO.GraphExtensions.SOOrderEntryExt
{
	public class BlanketSOLineSplitPlan : ItemPlan<SOOrderEntry, BlanketSOOrder, BlanketSOLineSplit>
	{
		public static bool IsActive()
			=> PXAccess.FeatureInstalled<FeaturesSet.distributionModule>();

		public override INItemPlan DefaultValues(INItemPlan planRow, BlanketSOLineSplit splitRow)
		{
			bool initPlan = string.IsNullOrEmpty(planRow.PlanType);
			if (initPlan)
			{
				if (splitRow.Completed == true
					|| splitRow.POCompleted == true
					|| splitRow.LineType == SOLineType.MiscCharge)
				{
					return null;
				}
				var line = PXParentAttribute.SelectParent<BlanketSOLine>(ItemPlanSourceCache, splitRow);
				var order = PXParentAttribute.SelectParent<BlanketSOOrder>(Base.Caches<BlanketSOLine>(), line);
				if (order == null)
				{
					throw new Common.Exceptions.RowNotFoundException(Base.Caches<BlanketSOOrder>(),
						splitRow.OrderType, splitRow.OrderNbr);
				}
				planRow.PlanType = CalcPlanType(order, splitRow);
				if (string.IsNullOrEmpty(planRow.PlanType)) return null;

				if (splitRow.POCreate == true)
				{
					planRow.FixedSource = INReplenishmentSource.Purchased;
					if (splitRow.POType != PO.POOrderType.Blanket && splitRow.POType != PO.POOrderType.DropShip && splitRow.POSource == INReplenishmentSource.PurchaseToOrder)
						planRow.SourceSiteID = splitRow.SiteID;
					else
						planRow.SourceSiteID = splitRow.SiteID;

					planRow.VendorID = splitRow.VendorID;
					if (planRow.VendorID != null)
					{
						planRow.VendorLocationID = PO.POItemCostManager.FetchLocation(
							Base,
							splitRow.VendorID,
							splitRow.InventoryID,
							splitRow.SubItemID,
							splitRow.SiteID);
					}
				}
				else
				{
					planRow.FixedSource = (splitRow.SiteID != splitRow.ToSiteID ? INReplenishmentSource.Transfer : INReplenishmentSource.None);
					planRow.SourceSiteID = splitRow.SiteID;
				}
				planRow.BAccountID = line.CustomerID;
				planRow.InventoryID = splitRow.InventoryID;
				planRow.SubItemID = splitRow.SubItemID;
				planRow.SiteID = splitRow.SiteID;
				planRow.LocationID = splitRow.LocationID;
				planRow.ProjectID = line.ProjectID;
				planRow.TaskID = line.TaskID;
				planRow.PlanDate = splitRow.ShipDate;
				planRow.UOM = line.UOM;
				planRow.LotSerialNbr = splitRow.LotSerialNbr;

				planRow.Hold = (order.Hold ?? false) || (!order.Approved ?? false);
				planRow.RefNoteID = order.NoteID;
			}

			planRow.PlanQty = (splitRow.POCreate == true ? splitRow.BaseUnreceivedQty : splitRow.BaseQty) - splitRow.BaseQtyOnOrders;

			return planRow;
		}

		protected virtual string CalcPlanType(BlanketSOOrder order, BlanketSOLineSplit split)
		{
			if (split.POCreate == true && split.POSource == INReplenishmentSource.BlanketPurchaseToOrder)
				return order.IsExpired == true && string.IsNullOrEmpty(split.PONbr) ? null : INPlanConstants.Plan6B;

			if (split.POCreate == true && split.POSource == INReplenishmentSource.PurchaseToOrder)
				return order.IsExpired == true && string.IsNullOrEmpty(split.PONbr) ? null : INPlanConstants.Plan66;

			return (split.IsAllocated == true) ? INPlanConstants.Plan61 : null;
		}

		protected override string GetRefEntityType()
		{
			return typeof(SOOrder).FullName;
		}
	}
}
