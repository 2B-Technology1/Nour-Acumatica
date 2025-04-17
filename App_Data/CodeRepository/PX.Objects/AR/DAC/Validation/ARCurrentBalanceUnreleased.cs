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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CM.Extensions;
using PX.Objects.CS;
using PX.Objects.GL;
using System;

namespace PX.Objects.AR
{
	[PXProjection(typeof(Select2<
		ARBalances,
		LeftJoin<ARRegister,
			On<ARRegister.branchID, Equal<ARBalances.branchID>,
				And<ARRegister.customerID, Equal<ARBalances.customerID>,
				And<ARRegister.customerLocationID, Equal<ARBalances.customerLocationID>,

				And<ARRegister.released, Equal<False>,
				And<ARRegister.hold, Equal<False>,
				And<ARRegister.scheduled, Equal<False>,
				And<ARRegister.voided, Equal<False>,
				And<ARRegister.docBal, NotEqual<Zero>>>>>>>>>,
		LeftJoin<ARInvoice,
			On<ARInvoice.docType, Equal<ARRegister.docType>,
				And<ARInvoice.refNbr, Equal<ARRegister.refNbr>>>>>,
		Where<
			ARInvoice.docType, IsNull,
			Or<
				ARInvoice.creditHold, Equal<False>,
				And<ARInvoice.pendingProcessing, Equal<False>>>>
		>), Persistent = false)]
	[PXHidden]
	public class ARCurrentBalanceUnreleased : IBqlTable
	{
		#region Keys
		public new class PK : PrimaryKeyOf<ARCurrentBalanceUnreleased>.By<branchID, customerID, customerLocationID>
		{
			public static ARCurrentBalanceUnreleased Find(PXGraph graph, int branchID, int? customerID, int? customerLocationID, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, branchID, customerID, customerLocationID, options);
		}
		#endregion

		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		[PXDBInt(BqlTable = typeof(ARBalances))]
		public virtual int? BranchID { get; set; }
		#endregion

		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

		[PXDBInt(BqlTable = typeof(ARBalances))]
		public virtual int? CustomerID { get; set; }
		#endregion

		#region CustomerLocationID
		public abstract class customerLocationID : PX.Data.BQL.BqlInt.Field<customerLocationID> { }

		[PXDBInt(BqlTable = typeof(ARBalances))]
		public virtual int? CustomerLocationID { get; set; }
		#endregion


		#region UnreleasedBal
		public abstract class unreleasedBal : PX.Data.BQL.BqlDecimal.Field<unreleasedBal> { }

		[PXDBDecimal(BqlTable = typeof(ARBalances))]
		public virtual Decimal? UnreleasedBal { get; set; }
		#endregion

		#region BalanceSign
		public abstract class balanceSign : PX.Data.BQL.BqlDecimal.Field<balanceSign> { }

		[PXDecimal]
		[PXDBCalced(typeof(
				Switch<
					Case<Where<ARRegister.docType.IsIn<ARDocType.invoice, ARDocType.debitMemo, ARDocType.refund, ARDocType.voidRefund, ARDocType.finCharge, ARDocType.smallCreditWO>>,
					decimal1,
					Case<Where<ARRegister.docType.IsIn<ARDocType.payment, ARDocType.prepayment, ARDocType.voidPayment, ARDocType.creditMemo, ARDocType.smallBalanceWO>>,
					decimal_1>>,
					decimal0>), typeof(decimal))]
		public virtual decimal? BalanceSign { get; set; }
		#endregion

		#region UnreleasedBalSigned
		public abstract class unreleasedBalSigned : PX.Data.BQL.BqlDecimal.Field<unreleasedBalSigned> { }

		[PXDecimal]
		[PXDBCalced(typeof(ARRegister.origDocAmt.Multiply<balanceSign>), typeof(decimal))]
		public virtual decimal? UnreleasedBalSigned { get; set; }
		#endregion
	}

	[PXProjection(typeof(Select4<ARCurrentBalanceUnreleased,
		Aggregate<
			GroupBy<ARCurrentBalanceUnreleased.branchID,
			GroupBy<ARCurrentBalanceUnreleased.customerID,
			GroupBy<ARCurrentBalanceUnreleased.customerLocationID,
			GroupBy<ARCurrentBalanceUnreleased.unreleasedBal,

			Sum<ARCurrentBalanceUnreleased.unreleasedBalSigned>>>>>>>), Persistent = false)]
	[PXHidden]
	public class ARCurrentBalanceUnreleasedSum : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<ARCurrentBalanceUnreleasedSum>.By<branchID, customerID, customerLocationID>
		{
			public static ARCurrentBalanceUnreleasedSum Find(PXGraph graph, int branchID, int? customerID, int? customerLocationID, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, branchID, customerID, customerLocationID, options);
		}
		#endregion

		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		[PXDBInt(BqlTable = typeof(ARCurrentBalanceUnreleased))]
		public virtual int? BranchID { get; set; }
		#endregion

		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

		[PXDBInt(BqlTable = typeof(ARCurrentBalanceUnreleased))]
		public virtual int? CustomerID { get; set; }
		#endregion

		#region CustomerLocationID
		public abstract class customerLocationID : PX.Data.BQL.BqlInt.Field<customerLocationID> { }

		[PXDBInt(BqlTable = typeof(ARCurrentBalanceUnreleased))]
		public virtual int? CustomerLocationID { get; set; }
		#endregion


		#region UnreleasedBal
		public abstract class unreleasedBal : PX.Data.BQL.BqlDecimal.Field<unreleasedBal> { }

		[PXDBDecimal(BqlTable = typeof(ARCurrentBalanceUnreleased))]
		public virtual Decimal? UnreleasedBal { get; set; }
		#endregion

		#region UnreleasedBalSigned
		public abstract class unreleasedBalSum : PX.Data.BQL.BqlDecimal.Field<unreleasedBalSum> { }

		[PXDBDecimal(4, BqlField = typeof(ARCurrentBalanceUnreleased.unreleasedBalSigned))]
		public virtual decimal? UnreleasedBalSum { get; set; }
		#endregion
	}
}
