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
using PX.Objects.CM;
using PX.Objects.GL;
using System;

namespace PX.Objects.SO
{
	/// <summary>
	/// The DAC that represent the result line of the invoices for the sales orders side panel inquiry of the sales orders form.
	/// </summary>
	[PXCacheName(Messages.SOOrderInvoicesSPInqResult)]
	public class SOOrderInvoicesSPInqResult : IBqlTable
	{
		#region DocType
		/// <inheritdoc cref="SOInvoice.DocType"/>
		[PXString(3, IsKey = true, IsFixed = true)]
		[AR.ARDocType.List()]
		[PXUIField(DisplayName = "Type")]
		public virtual String DocType { get; set; }
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		#endregion

		#region RefNbr
		/// <inheritdoc cref="SOInvoice.RefNbr"/>
		[PXString(15, IsUnicode = true, IsKey = true)]
		[PXSelector(typeof(Search<SOInvoice.refNbr, Where<SOInvoice.docType, Equal<Current<docType>>>>))]
		[PXUIField(DisplayName = "Reference Nbr.")]
		public virtual String RefNbr { get; set; }
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		#endregion

		#region Status
		/// <inheritdoc cref="ARInvoice.Status"/>
		[PXString(1, IsFixed = true)]
		[ARDocStatus.List()]
		[PXUIField(DisplayName = "Status")]
		public virtual String Status { get; set; }
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		#endregion

		#region DocDate
		/// <inheritdoc cref="ARRegister.DocDate"/>
		[PXDate]
		[PXUIField(DisplayName = "Date")]
		public virtual DateTime? DocDate { get; set; }
		public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
		#endregion

		#region CustomerID
		/// <inheritdoc cref="ARInvoice.CustomerID"/>
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2024R1)]
		[CustomerActive(DescriptionField = typeof(Customer.acctName), Filterable = true)]
		public virtual Int32? CustomerID { get; set; }
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2024R1)]
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		#endregion

		#region DueDate
		/// <inheritdoc cref="ARInvoice.DueDate"/>
		[PXDate]
		[PXUIField(DisplayName = "Due Date")]
		public virtual DateTime? DueDate { get; set; }
		public abstract class dueDate : PX.Data.BQL.BqlDateTime.Field<dueDate> { }
		#endregion

		#region FinPeriodID
		/// <inheritdoc cref="ARRegister.FinPeriodID"/>
		[PeriodID]
		[PXUIField(DisplayName = "Post Period")]
		public virtual String FinPeriodID { get; set; }
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		#endregion

		#region CuryOrigDocAmt
		/// <inheritdoc cref="ARRegister.CuryOrigDocAmt"/>
		[PXCury(typeof(curyID))]
		[PXUIField(DisplayName = "Amount")]
		public virtual Decimal? CuryOrigDocAmt { get; set; }
		public abstract class curyOrigDocAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDocAmt> { }
		#endregion

		#region CuryDocBal
		/// <inheritdoc cref="ARRegister.CuryDocBal"/>
		[PXCury(typeof(curyID))]
		[PXUIField(DisplayName = "Balance")]
		public virtual Decimal? CuryDocBal { get; set; }
		public abstract class curyDocBal : PX.Data.BQL.BqlDecimal.Field<curyDocBal> { }
		#endregion

		#region CuryID
		/// <inheritdoc cref="ARRegister.CuryID"/>
		[PXString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "Currency")]
		public virtual String CuryID { get; set; }
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		#endregion

		#region OrderType
		/// <inheritdoc cref="SOOrder.OrderType"/>
		[PXString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Order Type")]
		public virtual String OrderType { get; set; }
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		#endregion

		#region OrderNbr
		/// <inheritdoc cref="SOOrder.OrderNbr"/>
		[PXString(15, IsUnicode = true)]
		[PXSelector(typeof(Search<SOOrder.orderNbr, Where<SOOrder.orderType, Equal<Current<orderType>>>>))]
		[PXUIField(DisplayName = "Order Nbr.")]
		public virtual String OrderNbr { get; set; }
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		#endregion
	}
}
