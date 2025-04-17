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
using PX.Objects.AR;
using PX.Objects.CR;
using System;

namespace PX.Objects.SO.DAC.ReportParameters
{
	[PXVirtual]
	[PXCacheName(Messages.BlanketSODetailsReportParameters)]
	public partial class BlanketSODetailsReportParameters : IBqlTable
	{
		#region DateFrom
		public abstract class dateFrom : PX.Data.BQL.BqlDateTime.Field<dateFrom> { }

		[PXDate]
		public virtual DateTime? DateFrom
		{
			get;
			set;
		}
		#endregion
		#region DateTo
		public abstract class dateTo : PX.Data.BQL.BqlDateTime.Field<dateTo> { }

		[PXDate]
		public virtual DateTime? DateTo
		{
			get;
			set;
		}
		#endregion
		#region ExpiredByDate
		public abstract class expiredByDate : PX.Data.BQL.BqlDateTime.Field<expiredByDate> { }

		[PXDate]
		public virtual DateTime? ExpiredByDate
		{
			get;
			set;
		}
		#endregion
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

		[CustomerActive]
		public virtual Int32? CustomerID
		{
			get;
			set;
		}
		#endregion
		#region BlanketOrderType
		public abstract class blanketOrderType : PX.Data.BQL.BqlString.Field<blanketOrderType> { }

		[PXString(2, IsKey = true, IsFixed = true)]
		[PXSelector(typeof(Search<SOOrderType.orderType,
			Where<SOOrderType.active.IsEqual<True>.And<SOOrderType.behavior.IsEqual<SOBehavior.bL>>>>), Filterable = true)]
		public virtual String BlanketOrderType
		{
			get;
			set;
		}
		#endregion
		#region NotBlanketOrderType
		public abstract class notBlanketOrderType : PX.Data.BQL.BqlString.Field<notBlanketOrderType> { }

		[PXString(2, IsKey = true, IsFixed = true)]
		[PXSelector(typeof(Search<SOOrderType.orderType,
			Where<SOOrderType.active.IsEqual<True>.And<SOOrderType.behavior.IsNotEqual<SOBehavior.bL>>>>), Filterable = true)]
		public virtual String NotBlanketOrderType
		{
			get;
			set;
		}
		#endregion
		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }

		[PXString(15, IsKey = true, IsUnicode = true)]
		[PXSelector(typeof(Search2<SOOrder.orderNbr,
			LeftJoinSingleTable<Customer, On<SOOrder.FK.Customer.
				And<Where<Match<Customer, Current<AccessInfo.userName>>>>>>,
			Where<SOOrder.behavior.IsEqual<SOBehavior.bL>
				.And<SOOrder.cancelled.IsEqual<False>>
				.And<SOOrder.orderType.IsEqual<blanketOrderType.AsOptional>.Or<blanketOrderType.AsOptional.IsNull>>
				.And<SOOrder.customerID.IsEqual<customerID.AsOptional>.Or<customerID.AsOptional.IsNull>>
				.And<SOOrder.orderDate.IsGreaterEqual<dateFrom.AsOptional>.Or<dateFrom.AsOptional.IsNull>>
				.And<SOOrder.orderDate.IsLessEqual<dateTo.AsOptional>.Or<dateTo.AsOptional.IsNull>>
				.And<SOOrder.expireDate.IsLessEqual<expiredByDate.AsOptional>.Or<expiredByDate.AsOptional.IsNull>>>,
			 OrderBy<Desc<SOOrder.orderNbr>>>), Filterable = true)]
		public virtual String OrderNbr
		{
			get;
			set;
		}
		#endregion
	}
}
