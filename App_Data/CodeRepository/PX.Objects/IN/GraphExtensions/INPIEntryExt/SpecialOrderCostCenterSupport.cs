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
using CostLayerInfo = PX.Objects.IN.INPIEntry.CostLayerInfo;
using ProjectedTranRec = PX.Objects.IN.INPIEntry.ProjectedTranRec;

namespace PX.Objects.IN.GraphExtensions.INPIEntryExt
{
	public class SpecialOrderCostCenterSupport : PXGraphExtension<INPIEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.specialOrders>();
		}

		/// <summary>
		/// Overrides <see cref="INPIEntry.CreateProjectedTran(CostLayerInfo, INPIDetail, INItemSiteSettings)"/>
		/// </summary>
		[PXOverride]
		public virtual ProjectedTranRec CreateProjectedTran(CostLayerInfo costLayerInfo, INPIDetail line, INItemSiteSettings itemSiteSettings,
			Func<CostLayerInfo, INPIDetail, INItemSiteSettings, ProjectedTranRec> baseMethod)
		{
			ProjectedTranRec tran = baseMethod(costLayerInfo, line, itemSiteSettings);

			if (costLayerInfo.CostLayerType == CostLayerType.Special)
			{
				var costCenter = INCostCenter.PK.Find(Base, costLayerInfo.CostLayer.CostSiteID);

				tran.IsSpecialOrder = true;
				tran.SOOrderType = costCenter?.SOOrderType;
				tran.SOOrderNbr = costCenter?.SOOrderNbr;
				tran.SOOrderLineNbr = costCenter?.SOOrderLineNbr;
			}

			return tran;
		}

		/// <summary>
		/// Overrides <see cref="INPIEntry.ReadCostLayers(INPIDetail)"/>
		/// </summary>
		[PXOverride]
		public virtual IEnumerable<CostLayerInfo> ReadCostLayers(INPIDetail detail, Func<INPIDetail, IEnumerable<CostLayerInfo>> baseMethod)
		{
			List<CostLayerInfo> list = new List<CostLayerInfo>();
			list.AddRange(baseMethod(detail));
			list.AddRange(ReadCostLayersFromSpecialOrderCostCenter(detail));

			return list;
		}

		private IEnumerable<CostLayerInfo> ReadCostLayersFromSpecialOrderCostCenter(INPIDetail detail)
		{
			var select = new PXSelectJoin<INCostStatus,
				InnerJoin<INCostCenter, On<INCostStatus.costSiteID, Equal<INCostCenter.costCenterID>>,
				InnerJoin<INCostSubItemXRef, On<INCostSubItemXRef.costSubItemID, Equal<INCostStatus.costSubItemID>>>>,
				Where<INCostStatus.inventoryID, Equal<Required<INCostStatus.inventoryID>>,
					And<INCostStatus.qtyOnHand, Greater<decimal0>,
					And<INCostSubItemXRef.subItemID, Equal<Required<INCostSubItemXRef.subItemID>>,
					And<INCostCenter.siteID, Equal<Required<INCostCenter.siteID>>,
					And<INCostCenter.costLayerType, Equal<CostLayerType.special>,
					And<Where<INCostStatus.lotSerialNbr, Equal<Required<INCostStatus.lotSerialNbr>>,
						Or<INCostStatus.lotSerialNbr, IsNull,
						Or<INCostStatus.lotSerialNbr, Equal<Empty>>>>>>>>>>>(Base);

			return select
				.Select(detail.InventoryID, detail.SubItemID, detail.SiteID, detail.LotSerialNbr)
				.RowCast<INCostStatus>().AsEnumerable()
				.Select(layer => new CostLayerInfo(layer, CostLayerType.Special))
				.ToList();
		}

		/// <summary>
		/// Overrides <see cref="INPIEntry.GetIntersectionQty(CostLayerInfo, INPIDetail)"/>
		/// </summary>
		[PXOverride]
		public virtual decimal GetIntersectionQty(CostLayerInfo costLayer, INPIDetail line,
			Func<CostLayerInfo, INPIDetail, decimal> base_GetIntersectionQty)
		{
			decimal ret = base_GetIntersectionQty(costLayer, line);

			int layerCostCenterID;
			if (costLayer.CostLayerType == CostLayerType.Special)
			{
				layerCostCenterID = costLayer.CostLayer.CostSiteID.Value;
			}
			// in case Project Specific Inventory is turned on
			// normal cost layers may contain stock for projects tracked by quantity,
			// it should be handled by separate graph extension
			else if (costLayer.CostLayerType == CostLayerType.Normal && !PXAccess.FeatureInstalled<FeaturesSet.materialManagement>())
			{
				layerCostCenterID = CostCenter.FreeStock;
			}
			else
			{
				return ret;
			}

			var locStatus = INLocationStatusByCostCenter.PK.Find(
				Base, line.InventoryID, line.SubItemID, line.SiteID, line.LocationID, layerCostCenterID);
			ret = Math.Min(ret, Math.Min(locStatus?.QtyOnHand ?? 0m, locStatus?.QtyActual ?? 0m));

			if (!string.IsNullOrEmpty(line.LotSerialNbr))
			{
				var lotSerStatus = INLotSerialStatusByCostCenter.PK.Find(
					Base, line.InventoryID, line.SubItemID, line.SiteID, line.LocationID, line.LotSerialNbr, layerCostCenterID);
				ret = Math.Min(ret, Math.Min(lotSerStatus?.QtyOnHand ?? 0m, lotSerStatus?.QtyActual ?? 0m));
			}

			return ret;
		}
	}
}
