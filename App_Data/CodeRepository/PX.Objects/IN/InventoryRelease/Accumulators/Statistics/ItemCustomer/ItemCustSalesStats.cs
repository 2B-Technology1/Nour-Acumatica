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

namespace PX.Objects.IN.InventoryRelease.Accumulators.Statistics.ItemCustomer
{
	using static PXDataFieldAssign.AssignBehavior;

	[PXHidden]
	[Accumulator]
	[PXDisableCloneAttributes]
	public class ItemCustSalesStats : INItemCustSalesStats
	{
		#region InventoryID
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public override int? InventoryID
		{
			get => _InventoryID;
			set => _InventoryID = value;
		}
		public new abstract class inventoryID : BqlInt.Field<inventoryID> { }
		#endregion
		#region SubItemID
		[SubItem(IsKey = true)]
		[PXDefault]
		public override int? SubItemID
		{
			get => _SubItemID;
			set => _SubItemID = value;
		}
		public new abstract class subItemID : BqlInt.Field<subItemID> { }
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
		#region BAccountID
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public override int? BAccountID
		{
			get => _BAccountID;
			set => _BAccountID = value;
		}
		public new abstract class bAccountID : BqlInt.Field<bAccountID> { }
		#endregion

		public class AccumulatorAttribute : INItemCustSalesStatsAccumAttribute
		{
			protected override void PrepareInsertImpl(INItemCustSalesStats stats, PXAccumulatorCollection columns)
			{
				columns.Update<lastDate>(Maximize);
				columns.Update<lastUnitPrice>(Replace);
				columns.Update<lastQty>(Replace);
				columns.Restrict<lastDate>(PXComp.LEorISNULL, stats.LastDate);
			}
		}
	}
}
