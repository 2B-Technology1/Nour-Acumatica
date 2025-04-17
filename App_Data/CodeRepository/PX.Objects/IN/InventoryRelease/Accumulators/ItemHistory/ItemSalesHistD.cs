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
	[PXDisableCloneAttributes]
	public class ItemSalesHistD : INItemSalesHistD
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
		#region SYear
		[PXDBInt(IsKey = true)]
		public override int? SYear
		{
			get => _SYear;
			set => _SYear = value;
		}
		public new abstract class sYear : BqlInt.Field<sYear> { }
		#endregion
		#region SMonth
		[PXDBInt(IsKey = true)]
		public override int? SMonth
		{
			get => _SMonth;
			set => _SMonth = value;
		}
		public new abstract class sMonth : BqlInt.Field<sMonth> { }
		#endregion
		#region SDay
		[PXDBInt(IsKey = true)]
		public override int? SDay
		{
			get => _SDay;
			set => _SDay = value;
		}
		public new abstract class sDay : BqlInt.Field<sDay> { }
		#endregion

		public class AccumulatorAttribute : PXAccumulatorAttribute
		{
			public AccumulatorAttribute()
			{
				SingleRecord = true;
			}

			protected override bool PrepareInsert(PXCache cache, object row, PXAccumulatorCollection columns)
			{
				if (!base.PrepareInsert(cache, row, columns))
					return false;

				columns.Update<qtyExcluded>(Summarize);
				columns.Update<qtyIssues>(Summarize);
				columns.Update<qtyLostSales>(Summarize);
				columns.Update<qtyPlanSales>(Summarize);
				columns.Update<demandType1>(Replace);
				columns.Update<demandType2>(Replace);
				columns.Update<sQuater>(Replace);
				columns.Update<sDayOfWeek>(Replace);

				return true;
			}
		}
	}
}
