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

namespace PX.Objects.IN.InventoryRelease.Accumulators.ItemHistory
{
	[PXHidden]
	[Accumulator]
	[PXDisableCloneAttributes]
	public class ItemSiteHist : INItemSiteHist
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
		[PXDBInt(IsKey = true)]
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
		#region LocationID
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public override int? LocationID
		{
			get => _LocationID;
			set => _LocationID = value;
		}
		public new abstract class locationID : BqlInt.Field<locationID> { }
		#endregion
		#region FinPeriodID
		[PXDBString(6, IsKey = true, IsFixed = true)]
		[PXDefault]
		public override string FinPeriodID
		{
			get => _FinPeriodID;
			set => _FinPeriodID = value;
		}
		public new abstract class finPeriodID : BqlString.Field<finPeriodID> { }
		#endregion

		public class AccumulatorAttribute : PXAccumulatorAttribute
		{
			public AccumulatorAttribute() : base(
				Run<finBegQty>().WithValueOf<finYtdQty>(),
				Run<finYtdQty>().WithOwnValue(),

				Run<tranBegQty>().WithValueOf<tranYtdQty>(),
				Run<tranYtdQty>().WithOwnValue())
			{ }
		}
	}
}
