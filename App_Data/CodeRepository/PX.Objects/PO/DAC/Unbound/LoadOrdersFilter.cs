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
using PX.Objects.GL;
using System;

namespace PX.Objects.PO
{
	[PXHidden]
	public class LoadOrdersFilter : IBqlTable
	{
		#region  BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[Branch(typeof(branchID), Required = false)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? BranchID
		{
			get;
			set;
		}
		#endregion

		#region FromDate
		public abstract class fromDate : PX.Data.BQL.BqlDateTime.Field<fromDate> { }
		[PXDBDate()]
		[PXUIField(DisplayName = "From Date", Visibility = PXUIVisibility.Visible, Visible = true)]
		public virtual DateTime? FromDate
		{
			get;
			set;
		}
		#endregion

		#region ToDate
		public abstract class toDate : PX.Data.BQL.BqlDateTime.Field<toDate> { }
		[PXDBDate()]
		[PXUIField(DisplayName = "To Date", Visibility = PXUIVisibility.Visible, Visible = true)]
		public virtual DateTime? ToDate
		{
			get;
			set;
		}
		#endregion

		#region MaxNumberOfDocuments
		public abstract class maxNumberOfDocuments : PX.Data.BQL.BqlInt.Field<maxNumberOfDocuments> { }
		[PXDBInt()]
		[PXUIField(DisplayName = "Max. Number of Documents", Visibility = PXUIVisibility.Visible, Visible = true)]
		[PXDefault(999)]
		public virtual int? MaxNumberOfDocuments
		{
			get;
			set;
		}
		#endregion

		#region StartOrderNbr
		public abstract class startOrderNbr : PX.Data.BQL.BqlString.Field<startOrderNbr> { }
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Start Order Nbr.", Required = false)]
		[PXSelector(typeof(Search<POOrder.orderNbr,
			Where<POOrder.status, In3<POOrderStatus.open, POOrderStatus.completed>,
				And<POOrder.orderType, In3<POOrderType.regularOrder, POOrderType.dropShip>>>>),
				typeof(POOrder.orderNbr),
				typeof(POOrder.orderDate),
				typeof(POOrder.status),
				typeof(POOrder.curyUnprepaidTotal),
				typeof(POOrder.curyLineTotal),
				typeof(POOrder.curyID),
				Filterable = true)]
		public virtual String StartOrderNbr
		{
			get;
			set;
		}
		#endregion

		#region EndOrderNbr
		public abstract class endOrderNbr : PX.Data.BQL.BqlString.Field<endOrderNbr> { }
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "End Order Nbr.", Required = false)]
		[PXSelector(typeof(Search<POOrder.orderNbr,
			Where<POOrder.status, In3<POOrderStatus.open, POOrderStatus.completed>,
				And<POOrder.orderType, In3<POOrderType.regularOrder, POOrderType.dropShip>>>>),
				typeof(POOrder.orderNbr),
				typeof(POOrder.orderDate),
				typeof(POOrder.status),
				typeof(POOrder.curyUnprepaidTotal),
				typeof(POOrder.curyLineTotal),
				typeof(POOrder.curyID),
				Filterable = true)]
		public virtual String EndOrderNbr
		{
			get;
			set;
		}
		#endregion

		#region OrderBy
		public abstract class orderBy : PX.Data.BQL.BqlInt.Field<orderBy>
		{
			public const int ByDate = 1;
			public const int ByNbr = 2;

			public class byDate : PX.Data.BQL.BqlInt.Constant<byDate>
			{
				public byDate() : base(ByDate)
				{
				}
			}
			public class byNbr : PX.Data.BQL.BqlInt.Constant<byNbr>
			{
				public byNbr() : base(ByNbr)
				{
				}
			}
		}
		[PXDBInt]
		[PXIntList(new[] { orderBy.ByDate, orderBy.ByNbr }, new[] { "Order Date, Order Nbr.", "Order Nbr." })]
		[PXDefault(orderBy.ByDate)]
		public virtual int? OrderBy
		{
			get;
			set;
		}
		#endregion
	}
}
