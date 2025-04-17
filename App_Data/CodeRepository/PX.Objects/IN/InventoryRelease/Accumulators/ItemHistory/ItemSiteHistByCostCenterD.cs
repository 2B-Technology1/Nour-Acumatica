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
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.IN.InventoryRelease.Accumulators.ItemHistory
{
	[PXHidden]
	[ItemSiteHistByCostCenterD.Accumulator]
	public class ItemSiteHistByCostCenterD : INItemSiteHistByCostCenterD
	{
		#region SiteID
		public new abstract class siteID : Data.BQL.BqlInt.Field<siteID> { }
		[Site(IsKey = true)]
		[PXDefault]
		public override int? SiteID
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public new abstract class inventoryID : Data.BQL.BqlInt.Field<inventoryID> { }
		[StockItem(IsKey = true)]
		[PXDefault]
		public override int? InventoryID
		{
			get;
			set;
		}
		#endregion
		#region SubItemID
		public new abstract class subItemID : Data.BQL.BqlInt.Field<subItemID> { }
		[SubItem(IsKey = true)]
		[PXDefault]
		public override int? SubItemID
		{
			get;
			set;
		}
		#endregion
		#region CostCenterID
		public new abstract class costCenterID : Data.BQL.BqlInt.Field<costCenterID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public override int? CostCenterID
		{
			get;
			set;
		}
		#endregion
		#region SDate
		public new abstract class sDate : Data.BQL.BqlDateTime.Field<sDate> { }
		[PXDBDate(IsKey = true)]
		[PXDefault]
		public override DateTime? SDate
		{
			get;
			set;
		}
		#endregion

		public class AccumulatorAttribute : PXAccumulatorAttribute
		{
			public AccumulatorAttribute()
				: base(
					new[] { typeof(ItemSiteHistByCostCenterD.endQty), typeof(ItemSiteHistByCostCenterD.endQty), typeof(ItemSiteHistByCostCenterD.endCost) },
					new[] { typeof(ItemSiteHistByCostCenterD.begQty), typeof(ItemSiteHistByCostCenterD.endQty), typeof(ItemSiteHistByCostCenterD.endCost) })
			{
			}

			protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
			{
				if (!base.PrepareInsert(sender, row, columns))
				{
					return false;
				}

				var bal = (ItemSiteHistByCostCenterD)row;
				columns.Update<ItemSiteHistByCostCenterD.qtyReceived>(bal.QtyReceived, PXDataFieldAssign.AssignBehavior.Summarize);
				columns.Update<ItemSiteHistByCostCenterD.qtyIssued>(bal.QtyIssued, PXDataFieldAssign.AssignBehavior.Summarize);
				columns.Update<ItemSiteHistByCostCenterD.qtySales>(bal.QtySales, PXDataFieldAssign.AssignBehavior.Summarize);
				columns.Update<ItemSiteHistByCostCenterD.qtyCreditMemos>(bal.QtyCreditMemos, PXDataFieldAssign.AssignBehavior.Summarize);
				columns.Update<ItemSiteHistByCostCenterD.qtyDropShipSales>(bal.QtyDropShipSales, PXDataFieldAssign.AssignBehavior.Summarize);
				columns.Update<ItemSiteHistByCostCenterD.qtyTransferIn>(bal.QtyTransferIn, PXDataFieldAssign.AssignBehavior.Summarize);
				columns.Update<ItemSiteHistByCostCenterD.qtyTransferOut>(bal.QtyTransferOut, PXDataFieldAssign.AssignBehavior.Summarize);
				columns.Update<ItemSiteHistByCostCenterD.qtyAssemblyIn>(bal.QtyAssemblyIn, PXDataFieldAssign.AssignBehavior.Summarize);
				columns.Update<ItemSiteHistByCostCenterD.qtyAssemblyOut>(bal.QtyAssemblyOut, PXDataFieldAssign.AssignBehavior.Summarize);
				columns.Update<ItemSiteHistByCostCenterD.qtyAdjusted>(bal.QtyAdjusted, PXDataFieldAssign.AssignBehavior.Summarize);
				columns.Update<ItemSiteHistByCostCenterD.sDay>(bal.SDay, PXDataFieldAssign.AssignBehavior.Replace);
				columns.Update<ItemSiteHistByCostCenterD.sMonth>(bal.SMonth, PXDataFieldAssign.AssignBehavior.Replace);
				columns.Update<ItemSiteHistByCostCenterD.sYear>(bal.SYear, PXDataFieldAssign.AssignBehavior.Replace);
				columns.Update<ItemSiteHistByCostCenterD.sQuater>(bal.SQuater, PXDataFieldAssign.AssignBehavior.Replace);
				columns.Update<ItemSiteHistByCostCenterD.sDayOfWeek>(bal.SDayOfWeek, PXDataFieldAssign.AssignBehavior.Replace);

				return true;
			}
		}
	}
}
