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

namespace PX.Objects.SO
{
	/// <summary>
	/// The DAC that is used as a filter in invoices for sales orders side panel inquiry of the sales orders form.
	/// </summary>
	[PXCacheName(Messages.SOOrderInvoicesSPFilter)]
	public class SOOrderInvoicesSPFilter : IBqlTable
	{
		#region OrderType
		/// <summary>
		/// The type of the sales order.
		/// </summary>
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Order Type")]
		[PXSelector(typeof(Search<SOOrderType.orderType, Where<SOOrderType.behavior.IsNotIn<SOBehavior.tR, SOBehavior.qT>>>))]
		public virtual String OrderType { get; set; }
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		#endregion
		#region OrderNbr
		/// <summary>
		/// The number of the sales order.
		/// </summary>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Order Nbr.")]
		[PXSelector(typeof(Search<SOOrder.orderNbr, Where<SOOrder.orderType, Equal<Current<orderType>>>>))]
		public virtual String OrderNbr { get; set; }
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		#endregion
	}
}
