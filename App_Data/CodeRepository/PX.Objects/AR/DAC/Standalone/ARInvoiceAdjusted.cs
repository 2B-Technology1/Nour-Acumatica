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
using PX.Data.BQL.Fluent;
using PX.Objects.CM;
using PX.Objects.GL;
using System;

namespace PX.Objects.AR.Standalone
{
	[PXHidden]
	[PXProjection(typeof(
		SelectFrom<ARInvoice>.
			InnerJoin<ARRegister>.
			On<ARInvoice.refNbr.IsEqual<ARRegister.refNbr>.And<ARInvoice.docType.IsEqual<ARRegister.docType>>>)
		, Persistent = true)]
	[PXBreakInheritance]
	public class ARInvoiceAdjusted : ARRegister
	{
		#region DocType
		public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		[PXDBString(3, IsKey = true, IsFixed = true, BqlField = typeof(ARInvoice.docType))]
		[PXDefault()]
		public override string DocType
		{
			get;
			set;
		}
		#endregion

		#region RefNbr
		public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(ARInvoice.refNbr))]
		[PXDefault()]
		public override string RefNbr
		{
			get;
			set;
		}
		#endregion

		#region DocType
		public abstract class aRRegisterDocType : PX.Data.BQL.BqlString.Field<docType> { }
		[PXDBString(3, IsKey = true, IsFixed = true, BqlField = typeof(ARRegister.docType))]
		[PXDefault()]
		public virtual string ARRegisterDocType
		{
			get;
			set;
		}
		#endregion

		#region RefNbr
		public abstract class aRRegisterRefNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(ARRegister.refNbr))]
		[PXDefault()]
		public virtual string ARRegisterRefNbr
		{
			get;
			set;
		}
		#endregion


		#region NoteID
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote(BqlField = typeof(ARRegister.noteID))]
		public override Guid? NoteID
		{
			get;
			set;
		}
		#endregion

		#region BranchID
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[Branch(BqlField = typeof(ARRegister.branchID))]
		public override int? BranchID
		{
			get;
			set;
		}
		#endregion

		#region CuryPaymentTotal
		public abstract class curyPaymentTotal : PX.Data.BQL.BqlDecimal.Field<curyPaymentTotal> { }
		[PXDBCurrency(typeof(curyInfoID), typeof(paymentTotal), BqlField = typeof(ARInvoice.curyPaymentTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryPaymentTotal
		{
			get;
			set;
		}
		#endregion

		#region PaymentTotal
		public abstract class paymentTotal : PX.Data.BQL.BqlDecimal.Field<paymentTotal> { }
		[PXDBBaseCury(BqlField = typeof(ARInvoice.paymentTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? PaymentTotal
		{
			get;
			set;
		}
		#endregion

		#region CuryBalanceWOTotal
		public abstract class curyBalanceWOTotal : PX.Data.BQL.BqlDecimal.Field<curyBalanceWOTotal> { }
		[PXDBCurrency(typeof(curyInfoID), typeof(balanceWOTotal), BqlField = typeof(ARInvoice.curyBalanceWOTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryBalanceWOTotal
		{
			get;
			set;
		}
		#endregion

		#region BalanceWOTotal
		public abstract class balanceWOTotal : PX.Data.BQL.BqlDecimal.Field<balanceWOTotal> { }
		[PXDBDecimal(4, BqlField = typeof(ARInvoice.balanceWOTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? BalanceWOTotal
		{
			get;
			set;
		}
		#endregion

		#region CuryInfoID
		public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		[PXDBLong(BqlField = typeof(ARRegister.curyInfoID))]
		[CurrencyInfo]
		public override long? CuryInfoID
		{
			get;
			set;
		}
		#endregion

		#region CuryUnpaidBalance
		public abstract class curyUnpaidBalance : Data.BQL.BqlDecimal.Field<curyUnpaidBalance> { }
		[PXDBCurrency(typeof(curyInfoID), typeof(unpaidBalance), BqlField = typeof(ARInvoice.curyUnpaidBalance))]
		[PXFormula(typeof(Sub<curyOrigDocAmt, Add<curyPaymentTotal, Add<curyDiscAppliedAmt, curyBalanceWOTotal>>>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public decimal? CuryUnpaidBalance
		{
			get;
			set;
		}
		#endregion

		#region UnpaidBalance
		public abstract class unpaidBalance : Data.BQL.BqlDecimal.Field<unpaidBalance> { }
		[PXDBBaseCury(typeof(branchID), BqlField = typeof(ARInvoice.unpaidBalance))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public decimal? UnpaidBalance
		{
			get;
			set;
		}
		#endregion

		#region CuryDiscAppliedAmt
		public abstract class curyDiscAppliedAmt : PX.Data.BQL.BqlDecimal.Field<curyDiscAppliedAmt> { }
		[PXDBCurrency(typeof(curyInfoID), typeof(discAppliedAmt), BqlField = typeof(ARInvoice.curyDiscAppliedAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryDiscAppliedAmt
		{
			get;
			set;
		}
		#endregion

		#region DiscAppliedAmt
		public abstract class discAppliedAmt : PX.Data.BQL.BqlDecimal.Field<discAppliedAmt> { }
		[PXDBBaseCury(BqlField = typeof(ARInvoice.discAppliedAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? DiscAppliedAmt
		{
			get;
			set;
		}
		#endregion

		#region CuryOrigDocAmt
		public new abstract class curyOrigDocAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDocAmt> { }
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.origDocAmt), BqlField = typeof(ARRegister.curyOrigDocAmt))]
		public override decimal? CuryOrigDocAmt
		{
			get;
			set;
		}
		#endregion

		#region OrigDocAmt
		public new abstract class origDocAmt : PX.Data.BQL.BqlDecimal.Field<origDocAmt> { }
		[PXDBBaseCury(typeof(branchID), BqlField = typeof(ARRegister.origDocAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override decimal? OrigDocAmt
		{
			get;
			set;

		}
		#endregion

		#region IsUnderCorrection
		public new abstract class isUnderCorrection : Data.BQL.BqlBool.Field<isUnderCorrection> { }
		[PXDBBool(BqlField = typeof(ARRegister.isUnderCorrection))]
		[PXDefault(false)]
		public override bool? IsUnderCorrection
		{
			get;
			set;
		}
		#endregion

		#region CreatedByID
		public new abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID(BqlField = typeof(ARRegister.createdByID))]
		public override Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public new abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID(BqlField = typeof(ARRegister.createdByScreenID))]
		public override String CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public new abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime(BqlField = typeof(ARRegister.createdDateTime))]
		public override DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public new abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID(BqlField = typeof(ARRegister.lastModifiedByID))]
		public override Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public new abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID(BqlField = typeof(ARRegister.lastModifiedByScreenID))]
		public override String LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public new abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime(BqlField = typeof(ARRegister.lastModifiedDateTime))]
		public override DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public new abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp(BqlField = typeof(ARRegister.Tstamp))]
		public override Byte[] tstamp
		{
			get;
			set;
		}
		#endregion
	}
}
