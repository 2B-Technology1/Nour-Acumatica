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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;
using CostLayerInfo = PX.Objects.IN.INPIEntry.CostLayerInfo;
using ProjectedTranRec = PX.Objects.IN.INPIEntry.ProjectedTranRec;

namespace PX.Objects.PM.MaterialManagement
{
	public class INPIEntryMaterialExt : PXGraphExtension<INPIEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.materialManagement>();
		}


		/// <summary>
		/// Overrides <see cref="INPIEntry.ReadCostLayers(INPIDetail)"/>
		/// </summary>
		[PXOverride]
		public virtual IEnumerable<CostLayerInfo> ReadCostLayers(INPIDetail detail, Func<INPIDetail, IEnumerable<CostLayerInfo>> baseMethod)
		{
			List<CostLayerInfo> list = new List<CostLayerInfo>();
			list.AddRange(baseMethod(detail));
			list.AddRange(ReadCostLayersFromProjectCostCenter(detail));

			return list;
		}

		/// <summary>
		/// Overrides <see cref="INPIEntry.CreateProjectedTran(CostLayerInfo, INPIDetail, INItemSiteSettings)"/>
		/// </summary>
		[PXOverride]
		public virtual ProjectedTranRec CreateProjectedTran(CostLayerInfo costLayerInfo, INPIDetail line, INItemSiteSettings itemSiteSettings,
			Func<CostLayerInfo, INPIDetail, INItemSiteSettings, ProjectedTranRec> baseMethod)
		{
			ProjectedTranRec tran = baseMethod(costLayerInfo, line, itemSiteSettings);
			if (costLayerInfo.CostLayerType == CostLayerType.Project)
            {
				var costCenter = INCostCenter.PK.Find(Base, costLayerInfo.CostLayer.CostSiteID);
				tran.ProjectID = costCenter.ProjectID;
				tran.TaskID = costCenter.TaskID;
				tran.CostCenterID = costCenter.CostCenterID;
            }

			INLocation location = INLocation.PK.Find(Base, line.LocationID);
			if (location != null && location.TaskID != null)
            {
				tran.ProjectID = location.ProjectID;
				tran.TaskID = location.TaskID;
            }

			return tran;
		}

		private IEnumerable<CostLayerInfo> ReadCostLayersFromProjectCostCenter(INPIDetail detail)
		{
			var select = new PXSelectJoin<INCostStatus,
				InnerJoin<INCostCenter, On<INCostStatus.costSiteID, Equal<INCostCenter.costCenterID>>,
				InnerJoin<INCostSubItemXRef, On<INCostSubItemXRef.costSubItemID, Equal<INCostStatus.costSubItemID>>>>,
				Where<INCostStatus.inventoryID, Equal<Required<INCostStatus.inventoryID>>,
				And<INCostStatus.qtyOnHand, Greater<decimal0>,
				And<INCostSubItemXRef.subItemID, Equal<Required<INCostSubItemXRef.subItemID>>,
				And<INCostCenter.siteID, Equal<Required<INCostCenter.siteID>>,
				And<INCostCenter.costLayerType, Equal<CostLayerType.project>,
				And<Where<INCostStatus.lotSerialNbr, Equal<Required<INCostStatus.lotSerialNbr>>,
					Or<INCostStatus.lotSerialNbr, IsNull,
					Or<INCostStatus.lotSerialNbr, Equal<Empty>>>>>>>>>>>(Base);

			return select
				.Select(detail.InventoryID, detail.SubItemID, detail.SiteID, detail.LotSerialNbr)
				.RowCast<INCostStatus>().AsEnumerable()
				.Select(layer => new CostLayerInfo(layer, CostLayerType.Project))
				.ToList();
		}

		/// Overrides <see cref="INPIEntry.GetIntersectionQty(CostLayerInfo, INPIDetail)"/>
		[PXOverride]
		public virtual decimal GetIntersectionQty(CostLayerInfo costLayer, INPIDetail line,
			Func<CostLayerInfo, INPIDetail, decimal> base_GetIntersectionQty)
		{
			decimal ret = base_GetIntersectionQty(costLayer, line);

			if (costLayer.CostLayerType == CostLayerType.Project)
			{
				int layerCostCenterID = costLayer.CostLayer.CostSiteID.Value;
				var locStatus = INLocationStatusByCostCenter.PK.Find(
					Base, line.InventoryID, line.SubItemID, line.SiteID, line.LocationID, layerCostCenterID);
				ret = Math.Min(ret, Math.Min(locStatus?.QtyOnHand ?? 0m, locStatus?.QtyActual ?? 0m));

				if (!string.IsNullOrEmpty(line.LotSerialNbr))
				{
					var lotSerStatus = INLotSerialStatusByCostCenter.PK.Find(
						Base, line.InventoryID, line.SubItemID, line.SiteID, line.LocationID, line.LotSerialNbr, layerCostCenterID);
					ret = Math.Min(ret, Math.Min(lotSerStatus?.QtyOnHand ?? 0m, lotSerStatus?.QtyActual ?? 0m));
				}
			}
			else if (costLayer.CostLayerType == CostLayerType.Normal)
			{
				// normal cost layers may contain stock for projects tracked by quantity, need to summarize it
				INLocationStatusByCostCenter locStatus = SelectFrom<INLocationStatusByCostCenter>
					.LeftJoin<INCostCenter>.On<INCostCenter.costCenterID.IsEqual<INLocationStatusByCostCenter.costCenterID>>
					.Where<INLocationStatusByCostCenter.inventoryID.IsEqual<@P.AsInt>
						.And<INLocationStatusByCostCenter.subItemID.IsEqual<@P.AsInt>>
						.And<INLocationStatusByCostCenter.siteID.IsEqual<@P.AsInt>>
						.And<INLocationStatusByCostCenter.locationID.IsEqual<@P.AsInt>>
						.And<INLocationStatusByCostCenter.costCenterID.IsEqual<CostCenter.freeStock>
						.Or<INCostCenter.costLayerType.IsEqual<CostLayerType.normal>>>>
					.AggregateTo<
						GroupBy<INLocationStatusByCostCenter.inventoryID>, GroupBy<INLocationStatusByCostCenter.subItemID>,
						GroupBy<INLocationStatusByCostCenter.siteID>, GroupBy<INLocationStatusByCostCenter.locationID>,
						Sum<INLocationStatusByCostCenter.qtyOnHand>, Sum<INLocationStatusByCostCenter.qtyActual>>
					.View.ReadOnly.Select(Base, line.InventoryID, line.SubItemID, line.SiteID, line.LocationID);
				ret = Math.Min(ret, Math.Min(locStatus?.QtyOnHand ?? 0m, locStatus?.QtyActual ?? 0m));

				if (!string.IsNullOrEmpty(line.LotSerialNbr))
				{
					INLotSerialStatusByCostCenter lotSerStatus = SelectFrom<INLotSerialStatusByCostCenter>
						.LeftJoin<INCostCenter>.On<INCostCenter.costCenterID.IsEqual<INLotSerialStatusByCostCenter.costCenterID>>
						.Where<INLotSerialStatusByCostCenter.inventoryID.IsEqual<@P.AsInt>
							.And<INLotSerialStatusByCostCenter.subItemID.IsEqual<@P.AsInt>>
							.And<INLotSerialStatusByCostCenter.siteID.IsEqual<@P.AsInt>>
							.And<INLotSerialStatusByCostCenter.locationID.IsEqual<@P.AsInt>>
							.And<INLotSerialStatusByCostCenter.lotSerialNbr.IsEqual<@P.AsString>>
							.And<INLotSerialStatusByCostCenter.costCenterID.IsEqual<CostCenter.freeStock>
							.Or<INCostCenter.costLayerType.IsEqual<CostLayerType.normal>>>>
						.AggregateTo<
							GroupBy<INLotSerialStatusByCostCenter.inventoryID>, GroupBy<INLotSerialStatusByCostCenter.subItemID>,
							GroupBy<INLotSerialStatusByCostCenter.siteID>, GroupBy<INLotSerialStatusByCostCenter.locationID>,
							GroupBy<INLotSerialStatusByCostCenter.lotSerialNbr>,
							Sum<INLotSerialStatusByCostCenter.qtyOnHand>, Sum<INLotSerialStatusByCostCenter.qtyActual>>
						.View.ReadOnly.Select(Base, line.InventoryID, line.SubItemID, line.SiteID, line.LocationID, line.LotSerialNbr);
					ret = Math.Min(ret, Math.Min(lotSerStatus?.QtyOnHand ?? 0m, lotSerStatus?.QtyActual ?? 0m));
				}
			}

			return ret;
		}
	}
}
