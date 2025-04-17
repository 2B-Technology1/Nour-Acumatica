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

namespace PX.Objects.IN.InventoryRelease.Accumulators.Statistics.Item
{
	[PXHidden]
	[ItemStatsAccumulator(
		LastCostDateField = typeof(lastCostDate), LastCostField = typeof(lastCost),
		MinCostField = typeof(minCost), MaxCostField = typeof(maxCost),
		QtyOnHandField = typeof(qtyOnHand), TotalCostField = typeof(totalCost),
		LastPurchasedDateField = typeof(lastPurchaseDate))]
	[PXDisableCloneAttributes]
	public class ItemStats : INItemStats
	{
		#region InventoryID
		[StockItem(IsKey = true, DirtyRead = true)]
		[PXDefault]
		public override int? InventoryID
		{
			get => _InventoryID;
			set => _InventoryID = value;
		}
		public new abstract class inventoryID : BqlInt.Field<inventoryID> { }
		#endregion
		#region SiteID
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public override int? SiteID
		{
			get => _SiteID;
			set => _SiteID = value;
		}
		public new abstract class siteID : BqlInt.Field<siteID> { }
		#endregion
		#region ValMethod
		[PXDBString(1, IsFixed = true)]
		[PXDefault(typeof(SearchFor<InventoryItem.valMethod>.Where<InventoryItem.inventoryID.IsEqual<inventoryID.FromCurrent>>))]
		public override string ValMethod
		{
			get => _ValMethod;
			set => _ValMethod = value;
		}
		public new abstract class valMethod : BqlString.Field<valMethod> { }
		#endregion
	}
}
