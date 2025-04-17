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
using PX.Objects.SO;
using PX.Objects.CS;

namespace PX.Objects.IN.Attributes
{
	public class AdjustmentSpecialOrderCostCenterSelectorAttribute : SpecialOrderCostCenterSelectorAttribute
	{
		public AdjustmentSpecialOrderCostCenterSelectorAttribute()
			: base(typeof(
							Search2<INCostCenter.costCenterID,
								InnerJoin<SOLine, On<INCostCenter.FK.OrderLine>,
								InnerJoin<INSiteStatusByCostCenter, On<SOLine.inventoryID, Equal<INSiteStatusByCostCenter.inventoryID>,
									And<SOLine.subItemID, Equal<INSiteStatusByCostCenter.subItemID>,
									And<INCostCenter.siteID, Equal<INSiteStatusByCostCenter.siteID>,
									And<INCostCenter.costCenterID, Equal<INSiteStatusByCostCenter.costCenterID>>>>>>>,
							Where<SOLine.inventoryID, Equal<Current2<INTran.inventoryID>>,
								And<INCostCenter.siteID, Equal<Current2<INTran.siteID>>,
								And2<Where<
									Current2<INTran.tranType>, Equal<INTranType.receiptCostAdjustment>,
									Or<INSiteStatusByCostCenter.qtyOnHand, Greater<decimal0>>>,
								And<Where<Current2<INRegister.pIID>, IsNull,
									Or<Current2<INTran.lotSerialNbr>, IsNull,
									Or<Current2<INTran.lotSerialNbr>, Equal<StringEmpty>,
									Or<Exists<
										Select<INLotSerialStatusByCostCenter,
										Where<INLotSerialStatusByCostCenter.inventoryID, Equal<Current2<INTran.inventoryID>>,
											And<INLotSerialStatusByCostCenter.subItemID, Equal<Current2<INTran.subItemID>>,
											And<INLotSerialStatusByCostCenter.siteID, Equal<Current2<INTran.siteID>>,
											And<INLotSerialStatusByCostCenter.locationID, Equal<Current2<INTran.locationID>>,
											And<INLotSerialStatusByCostCenter.lotSerialNbr, Equal<Current2<INTran.lotSerialNbr>>,
											And<INLotSerialStatusByCostCenter.costCenterID, Equal<INCostCenter.costCenterID>,
											And<INLotSerialStatusByCostCenter.qtyOnHand, Greater<decimal0>>>>>>>>>>>>>>>>>>>
							))
		{
			InventoryIDField = typeof(INTran.inventoryID);
			SiteIDField = typeof(INTran.siteID);

			SOOrderTypeField = typeof(INTran.sOOrderType);
			SOOrderNbrField = typeof(INTran.sOOrderNbr);
			SOOrderLineNbrField = typeof(INTran.sOOrderLineNbr);
			IsSpecialOrderField = typeof(INTran.isSpecialOrder);
			CostCenterIDField = typeof(INTran.costCenterID);
			CostLayerTypeField = typeof(INTran.costLayerType);
			OrigModuleField = typeof(INTran.origModule);
			ReleasedField = typeof(INTran.released);
			ProjectIDField = typeof(INTran.projectID);
			TaskIDField = typeof(INTran.taskID);
			CostCodeIDField = typeof(INTran.costCodeID);
		}
	}
}
