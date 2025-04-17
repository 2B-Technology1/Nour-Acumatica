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
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CR;

namespace PX.Objects.Localizations.CA {
	/// <summary>
	/// Projection DAC for aggregating payments for the purpose of T5018 calculations.
	/// </summary>
	[PXHidden]
	[Serializable]
	[PXProjection(typeof(SelectFrom<BAccountR>.
		InnerJoin<APAdjust>.On<APAdjust.vendorID.IsEqual<BAccountR.bAccountID>>.

		InnerJoin<APTran>.
			On<APTran.tranType.IsEqual<APAdjust.adjdDocType>.
			And<APTran.refNbr.IsEqual<APAdjust.adjdRefNbr>>>.

		InnerJoin<APInvoice>.
			On<APInvoice.docType.IsEqual<APAdjust.adjdDocType>.
			And<APInvoice.refNbr.IsEqual<APAdjust.adjdRefNbr>>>.

		Where<T5018VendorTransaction.adjdDocType.IsNotEqual<APDocType.prepayment>.
			And<T5018VendorTransaction.adjdDocType.IsNotEqual<APDocType.debitAdj>>.
			And<T5018VendorExt.vendorT5018.IsEqual<True>>.
			And<T5018VendorTransaction.released.IsEqual<True>>.
			And<T5018VendorTransaction.voided.IsEqual<False>>>
		))]
	public class T5018VendorTransaction : BAccountR {
		#region AdjdDocType
		public abstract class adjdDocType : BqlString.Field<adjdDocType> { }
		/// <summary>
		/// The type of the adjusted document.
		/// </summary>
		[PXDBString(3, IsKey = true, BqlField = typeof(APAdjust.adjdDocType))]
		public virtual String AdjdDocType
		{
			get; set;
		}
		#endregion

		#region Multiplier
		public abstract class multiplier : BqlDecimal.Field<multiplier> {
			#region Multiplier Constants
			public const decimal Positive = 1m;
			public const decimal Negative = -1m;
			public class positive : BqlDecimal.Constant<positive> {
				public positive() : base(Positive) { }
			}
			public class negative : BqlDecimal.Constant<negative> {
				public negative() : base(Negative) { }
			}
			#endregion
		}

		/// <summary>
		/// Constant defined by adjdDocType used in transaction amount.
		/// </summary>
		[PXDecimal]
		[PXDBCalced(typeof(
			Switch<
				Case<Where<APAdjust.adjdDocType.IsEqual<APDocType.voidQuickCheck>.
					Or<APAdjust.adjdDocType.IsEqual<APDocType.refund>>.
					Or<APAdjust.adjdDocType.IsEqual<APDocType.voidRefund>>.
					Or<APAdjust.adjdDocType.IsEqual<APDocType.debitAdj>>>, multiplier.negative>,
				multiplier.positive
				>
			), typeof(decimal))]
		public virtual decimal? Multiplier { get; set; }
		#endregion

		#region Transaction Amount
		public abstract class transactionAmt : BqlDecimal.Field<transactionAmt> { }
		/// <summary>
		/// The calculated amounts of transactions applicable to T5018.
		/// </summary>
		[PXDecimal]
		[PXDBCalced(typeof(
			APTran.tranAmt.Add<
				APTran.tranAmt.Multiply<APInvoice.taxTotal>.Divide<APInvoice.origDocAmt.Subtract<APInvoice.taxTotal>>>.
			Multiply<APAdjust.adjAmt>.
			Divide<APInvoice.origDocAmt>.
			Multiply<multiplier>), typeof(decimal))]
		public virtual decimal? TransactionAmt
		{
			get; set;
		}
		#endregion

		#region VendorID
		public abstract class vendorID : BqlInt.Field<vendorID> { }
		/// <summary>
		/// The vendor ID of associated payments.
		/// </summary>
		[PXDBInt(BqlField = typeof(APInvoice.vendorID))]
		public virtual int? VendorID { get; set; }
		#endregion

		#region BranchID
		public abstract class branchID : BqlInt.Field<branchID> { }
		/// <summary>
		/// The branch ID of associated payments.
		/// </summary>
		[PXDBInt(BqlField = typeof(APAdjust.adjgBranchID))]
		public virtual int? BranchID { get; set; }
		#endregion

		#region AdjdDocDate
		public abstract class adjdDocDate : BqlDateTime.Field<adjdDocDate> { }
		/// <summary>
		/// Either the date when the adjusted document was created or the date of the original vendor's document.
		/// </summary>
		[PXDBDate(BqlField = typeof(APAdjust.adjdDocDate))]
		public virtual DateTime? AdjdDocDate { get; set; }
		#endregion

		#region Released
		public abstract class released : BqlBool.Field<released> { }
		/// <summary>
		/// When set to <c>true</c> indicates that the adjustment was released.
		/// </summary>
		[PXDBBool(BqlField = typeof(APAdjust.released))]
		public virtual bool? Released { get; set; }
		#endregion

		#region Voided
		public abstract class voided : BqlBool.Field<voided> { }
		/// <summary>
		/// When set to <c>true</c> indicates that the adjustment was voided.
		/// </summary>
		[PXDBBool(BqlField = typeof(APAdjust.voided))]
		public virtual bool? Voided { get; set; }
		#endregion
	}
}
