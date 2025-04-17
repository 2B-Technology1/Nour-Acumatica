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
using PX.Objects.IN;

namespace PX.Objects.SO
{
	[PXHidden]
	[PXProjection(typeof(
		SelectFrom<SOLineSplit>
		.Where<
			SOLineSplit.lineType.IsEqual<SOLineType.inventory>
			.And<SOLineSplit.operation.IsEqual<SOOperation.issue>>
			.And<SOLineSplit.completed.IsNotEqual<True>>
			.And<SOLineSplit.pOCreate.IsNotEqual<True>.And<SOLineSplit.pOCompleted.IsNotEqual<True>>>
			.And<SOLineSplit.childLineCntr.IsEqual<Zero>>
			.And<SOLineSplit.pOReceiptNbr.IsNull>>
		), Persistent = false)]
	public class SOLineSplitAllocation: IBqlTable
	{
		#region OrderType
		[PXDBString(2, IsKey = true, IsFixed = true, BqlField = typeof(SOLineSplit.orderType))]
		public virtual string OrderType { get; set; }
		public abstract class orderType : BqlString.Field<orderType> { }
		#endregion

		#region OrderNbr
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(SOLineSplit.orderNbr))]
		public virtual string OrderNbr { get; set; }
		public abstract class orderNbr : BqlString.Field<orderNbr> { }
		#endregion

		#region LineNbr
		[PXDBInt(IsKey = true, BqlField = typeof(SOLineSplit.lineNbr))]
		public virtual int? LineNbr { get; set; }
		public abstract class lineNbr : BqlInt.Field<lineNbr> { }
		#endregion

		#region SplitLineNbr
		[PXDBInt(IsKey = true, BqlField = typeof(SOLineSplit.splitLineNbr))]
		public virtual int? SplitLineNbr { get; set; }
		public abstract class splitLineNbr : BqlInt.Field<splitLineNbr> { }
		#endregion

		#region SiteID
		[PXDBInt(BqlField = typeof(SOLineSplit.siteID))]
		public virtual int? SiteID { get; set; }
		public abstract class siteID : BqlInt.Field<siteID> { }
		#endregion

		#region QtyAllocated
		[PXQuantity]
		[PXDBCalced(typeof(Switch<Case<Where<SOLineSplit.isAllocated.IsEqual<True>>, SOLineSplit.baseQty>, CS.decimal0>), typeof(decimal))]
		public virtual decimal? QtyAllocated { get; set; }
		public abstract class qtyAllocated : BqlDecimal.Field<qtyAllocated> { }
		#endregion

		#region QtyUnallocated
		[PXQuantity]
		[PXDBCalced(typeof(Switch<Case<Where<SOLineSplit.isAllocated.IsNotEqual<True>>, SOLineSplit.baseQty>, CS.decimal0>), typeof(decimal))]
		public virtual decimal? QtyUnallocated { get; set; }
		public abstract class qtyUnallocated : BqlDecimal.Field<qtyUnallocated> { }
		#endregion

		#region LotSerialQtyAllocated
		[PXQuantity]
		[PXDBCalced(typeof(Switch<Case<Where<SOLineSplit.isAllocated.IsEqual<True>.And<Length<SOLineSplit.lotSerialNbr>.IsGreater<Zero>>>, SOLineSplit.baseQty>, CS.decimal0>), typeof(decimal))]
		public virtual decimal? LotSerialQtyAllocated { get; set; }
		public abstract class lotSerialQtyAllocated : BqlDecimal.Field<lotSerialQtyAllocated> { }
		#endregion
	}
}
