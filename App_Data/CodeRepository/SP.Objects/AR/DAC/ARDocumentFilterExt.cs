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

namespace SP.Objects.AR
{
	public class ARDocumentFilterExt : PXCacheExtension<ARDocumentEnq.ARDocumentFilter>
	{
		#region ShowAllDocs
		public abstract class showAllDocs : PX.Data.IBqlField
		{
		}

		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Show All Documents")]
		public virtual bool? ShowAllDocs { get; set; }
		#endregion
		#region CustomerBalance
		public abstract class spCustomerBalance : PX.Data.IBqlField
		{
		}

		[PXUIField(DisplayName = "Current Balance", Enabled = false)]
		public virtual string SPCustomerBalance { get; set; }
		#endregion
		#region CustomerDepositsBalance
		public abstract class spcustomerDepositsBalance : PX.Data.IBqlField
		{
		}

		[PXUIField(DisplayName = "Prepayments Balance", Enabled = false)]
		public virtual decimal? SPCustomerDepositsBalance { get; set; }
		#endregion
		#region OpenInvoiceAndCharge
		public abstract class openInvoiceAndCharge : PX.Data.IBqlField
		{
		}

		[PXBaseCury()]
		[PXUIField(DisplayName = "Outstanding Invoices and Memos", Enabled = false)]
		public virtual decimal? OpenInvoiceAndCharge { get; set; }
		#endregion
		#region CreditMemosandUnappliedPayment
		public abstract class creditMemosandUnappliedPayment : PX.Data.IBqlField
		{
		}

		[PXBaseCury()]
		[PXUIField(DisplayName = "Unapplied Payments", Enabled = false)]
		public virtual decimal? CreditMemosandUnappliedPayment { get; set; }
		#endregion
		#region NetBalance
		public abstract class netBalance : PX.Data.IBqlField
		{
		}

		[PXBaseCury()]
		[PXUIField(DisplayName = "Net Balance", Enabled = false)]
		public virtual decimal? NetBalance { get; set; }
		#endregion
		#region CuryOpenInvoiceAndCharge
		public abstract class curyOpenInvoiceAndCharge : PX.Data.IBqlField
		{
		}

		[PXBaseCury()]
		[PXUIField(DisplayName = "Outstanding Invoices and Memos (Currency)", Enabled = false)]
		public virtual decimal? CuryOpenInvoiceAndCharge { get; set; }
		#endregion
		#region CuryCreditMemosandUnappliedPayment
		public abstract class curyCreditMemosandUnappliedPayment : PX.Data.IBqlField
		{
		}

		[PXBaseCury()]
		[PXUIField(DisplayName = "Unapplied Payments (Currency)", Enabled = false)]
		public virtual decimal? CuryCreditMemosandUnappliedPayment { get; set; }
		#endregion
		#region CuryNetBalance
		public abstract class curyNetBalance : PX.Data.IBqlField
		{
		}

		[PXBaseCury()]
		[PXUIField(DisplayName = "Net Balance (Currency)", Enabled = false)]
		public virtual decimal? CuryNetBalance { get; set; }
		#endregion
		#region CreditLimit
		public abstract class creditLimit : PX.Data.IBqlField
		{
		}

		[PXBaseCury()]
		[PXUIField(DisplayName = "Credit Limit", Enabled = false)]
		public virtual decimal? CreditLimit { get; set; }
		#endregion
		#region AvailableCredit
		public abstract class availableCredit : PX.Data.IBqlField
		{
		}

		[PXBaseCury()]
		[PXUIField(DisplayName = "Available Credit", Enabled = false)]
		public virtual decimal? AvailableCredit { get; set; }
		#endregion
	}
}
