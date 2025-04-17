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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;

namespace PX.Objects.SO
{
	[PXHidden]
	[PXProjection(typeof(
		SelectFrom<SOLineSplitAllocation>
		.AggregateTo<
			GroupBy<SOLineSplitAllocation.orderType>,
			GroupBy<SOLineSplitAllocation.orderNbr>,
			GroupBy<SOLineSplitAllocation.lineNbr>,
			GroupBy<SOLineSplitAllocation.siteID>,
			Sum<SOLineSplitAllocation.qtyAllocated>,
			Sum<SOLineSplitAllocation.qtyUnallocated>,
			Sum<SOLineSplitAllocation.lotSerialQtyAllocated>>
		), Persistent = false)]
	public class SOLineSiteAllocation: IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<SOLineSiteAllocation>.By<orderType, orderNbr, lineNbr, siteID>
		{
			public static SOLineSiteAllocation Find(PXGraph graph, string orderType, string orderNbr, int? lineNbr, int? siteID)
				=> FindBy(graph, orderType, orderNbr, lineNbr, siteID);
		}
		public static class FK
		{
			public class Order : SOOrder.PK.ForeignKeyOf<SOLineSiteAllocation>.By<orderType, orderNbr> { }
			public class OrderLine : SOLine.PK.ForeignKeyOf<SOLineSiteAllocation>.By<orderType, orderNbr, lineNbr> { }
		}
		#endregion

		#region OrderType
		[PXDBString(2, IsKey = true, IsFixed = true, BqlField = typeof(SOLineSplitAllocation.orderType))]
		public virtual string OrderType { get; set; }
		public abstract class orderType : BqlString.Field<orderType> { }
		#endregion

		#region OrderNbr
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(SOLineSplitAllocation.orderNbr))]
		public virtual string OrderNbr { get; set; }
		public abstract class orderNbr : BqlString.Field<orderNbr> { }
		#endregion

		#region LineNbr
		[PXDBInt(IsKey = true, BqlField = typeof(SOLineSplitAllocation.lineNbr))]
		public virtual int? LineNbr { get; set; }
		public abstract class lineNbr : BqlInt.Field<lineNbr> { }
		#endregion

		#region SiteID
		[PXDBInt(IsKey = true, BqlField = typeof(SOLineSplitAllocation.siteID))]
		public virtual int? SiteID { get; set; }
		public abstract class siteID : BqlInt.Field<siteID> { }
		#endregion

		#region QtyAllocated
		[PXDBQuantity(BqlField = typeof(SOLineSplitAllocation.qtyAllocated))]
		public virtual decimal? QtyAllocated { get; set; }
		public abstract class qtyAllocated : BqlDecimal.Field<qtyAllocated> { }
		#endregion

		#region QtyUnallocated
		[PXDBQuantity(BqlField = typeof(SOLineSplitAllocation.qtyUnallocated))]
		public virtual decimal? QtyUnallocated { get; set; }
		public abstract class qtyUnallocated : BqlDecimal.Field<qtyUnallocated> { }
		#endregion

		#region LotSerialQtyAllocated
		[PXDBQuantity(BqlField = typeof(SOLineSplitAllocation.lotSerialQtyAllocated))]
		public virtual decimal? LotSerialQtyAllocated { get; set; }
		public abstract class lotSerialQtyAllocated : BqlDecimal.Field<lotSerialQtyAllocated> { }
		#endregion
	}
}
