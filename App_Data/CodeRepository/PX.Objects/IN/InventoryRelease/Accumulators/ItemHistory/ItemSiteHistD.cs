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
using PX.Data;
using PX.Data.BQL;

namespace PX.Objects.IN.InventoryRelease.Accumulators.ItemHistory
{
	using static PXDataFieldAssign.AssignBehavior;

	[PXHidden]
	[Accumulator]
	public class ItemSiteHistD : INItemSiteHistD
	{
		#region InventoryID
		[Inventory(IsKey = true)]
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
		[Site(IsKey = true)]
		[PXDefault]
		public override int? SiteID
		{
			get => _SiteID;
			set => _SiteID = value;
		}
		public new abstract class siteID : BqlInt.Field<siteID> { }
		#endregion
		#region SDate
		[PXDBDate(IsKey = true)]
		public override DateTime? SDate
		{
			get => _SDate;
			set => _SDate = value;
		}
		public new abstract class sDate : BqlDateTime.Field<sDate> { }
		#endregion

		public class AccumulatorAttribute : PXAccumulatorAttribute
		{
			public AccumulatorAttribute() : base(
				Run<begQty>().WithValueOf<endQty>(),
				Run<endQty>().WithOwnValue())
			{ }

			protected override bool PrepareInsert(PXCache cache, object row, PXAccumulatorCollection columns)
			{
				if (!base.PrepareInsert(cache, row, columns))
					return false;

				columns.Update<qtyReceived>(Summarize);
				columns.Update<qtyIssued>(Summarize);
				columns.Update<qtySales>(Summarize);
				columns.Update<qtyCreditMemos>(Summarize);
				columns.Update<qtyDropShipSales>(Summarize);
				columns.Update<qtyTransferIn>(Summarize);
				columns.Update<qtyTransferOut>(Summarize);
				columns.Update<qtyAssemblyIn>(Summarize);
				columns.Update<qtyAssemblyOut>(Summarize);
				columns.Update<qtyAdjusted>(Summarize);
				columns.Update<sDay>(Replace);
				columns.Update<sMonth>(Replace);
				columns.Update<sYear>(Replace);
				columns.Update<sQuater>(Replace);
				columns.Update<sDayOfWeek>(Replace);

				return true;
			}
		}
	}
}
