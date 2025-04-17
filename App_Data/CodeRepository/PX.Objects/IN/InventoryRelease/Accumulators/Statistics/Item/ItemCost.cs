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

namespace PX.Objects.IN.InventoryRelease.Accumulators.Statistics.Item
{
	[PXHidden]
	[ItemStatsAccumulator(
		LastCostDateField = typeof(lastCostDate), LastCostField = typeof(lastCost),
		MinCostField = typeof(minCost), MaxCostField = typeof(maxCost),
		QtyOnHandField = typeof(qtyOnHand), TotalCostField = typeof(totalCost))]
	[PXDisableCloneAttributes]
	[PXBreakInheritance]
	public class ItemCost : INItemCost
	{
		#region InventoryID
		[StockItem(IsKey = true, DirtyRead = true)]
		[PXDefault]
		public override int? InventoryID
		{
			get => base.InventoryID;
			set => base.InventoryID = value;
		}
		public new abstract class inventoryID : BqlInt.Field<inventoryID> { }
		#endregion
		#region LastCostSiteID
		[PXInt]
		public virtual int? LastCostSiteID
		{
			get;
			set;
		}
		public new abstract class lastCostSiteID : BqlInt.Field<lastCostSiteID> { }
		#endregion
		#region TranUnitCost
		[PXDecimal]
		public override decimal? TranUnitCost
		{
			get => base.TranUnitCost;
			set => base.TranUnitCost = value;
		}
		public new abstract class tranUnitCost : BqlDecimal.Field<tranUnitCost> { }
		#endregion
	}
}
