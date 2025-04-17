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
			On<ARRegister.released, Equal<True>,
				And<ARRegister.branchID, Equal<ARBalances.branchID>,
				And<ARRegister.customerID, Equal<ARBalances.customerID>,
				And<ARRegister.customerLocationID, Equal<ARBalances.customerLocationID>,
				And<ARRegister.docBal, NotEqual<Zero>>>>>>>>), Persistent = false)]
	[PXHidden]
	public class ARCurrentBalance : IBqlTable
	{
		#region Keys
		public new class PK : PrimaryKeyOf<ARCurrentBalance>.By<branchID, customerID, customerLocationID>
		{
			public static ARCurrentBalance Find(PXGraph graph, int branchID, int? customerID, int? customerLocationID, PKFindOptions options = PKFindOptions.None) =>
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


		#region CurrentBal
		public abstract class currentBal : PX.Data.BQL.BqlDecimal.Field<currentBal> { }

		[PXDBDecimal(BqlTable = typeof(ARBalances))]
		public virtual Decimal? CurrentBal { get; set; }
		#endregion

		#region BalanceSign
		public abstract class balanceSign : PX.Data.BQL.BqlDecimal.Field<balanceSign> { }

		[PXDecimal]
		[PXDBCalced(typeof(
				Switch<Case<Where<ARRegister.docType.IsIn<ARDocType.refund, ARDocType.voidRefund, ARDocType.invoice, ARDocType.debitMemo, ARDocType.finCharge, ARDocType.smallCreditWO, ARDocType.cashSale>>,
					decimal1>,
					decimal_1>), typeof(decimal))]
		public virtual decimal? BalanceSign { get; set; }
		#endregion

		#region CurrentBalSigned
		public abstract class currentBalSigned : PX.Data.BQL.BqlDecimal.Field<currentBalSigned> { }

		[PXDecimal]
		[PXDBCalced(typeof(ARRegister.docBal.Multiply<balanceSign>), typeof(decimal))]
		public virtual decimal? CurrentBalSigned { get; set; }
		#endregion
	}

	[PXProjection(typeof(Select4<ARCurrentBalance,
		Aggregate<
			GroupBy<ARCurrentBalance.branchID,
			GroupBy<ARCurrentBalance.customerID,
			GroupBy<ARCurrentBalance.customerLocationID,
			GroupBy<ARCurrentBalance.currentBal,

			Sum<ARCurrentBalance.currentBalSigned>>>>>>>), Persistent = false)]
	[PXHidden]
	public class ARCurrentBalanceSum : IBqlTable
	{
		#region Keys
		public new class PK : PrimaryKeyOf<ARCurrentBalanceSum>.By<branchID, customerID, customerLocationID>
		{
			public static ARCurrentBalanceSum Find(PXGraph graph, int branchID, int? customerID, int? customerLocationID, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, branchID, customerID, customerLocationID, options);
		}
		#endregion

		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		[PXDBInt(BqlTable = typeof(ARCurrentBalance))]
		public virtual int? BranchID { get; set; }
		#endregion

		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

		[PXDBInt(BqlTable = typeof(ARCurrentBalance))]
		public virtual int? CustomerID { get; set; }
		#endregion

		#region CustomerLocationID
		public abstract class customerLocationID : PX.Data.BQL.BqlInt.Field<customerLocationID> { }

		[PXDBInt(BqlTable = typeof(ARCurrentBalance))]
		public virtual int? CustomerLocationID { get; set; }
		#endregion


		#region CurrentBal
		public abstract class currentBal : PX.Data.BQL.BqlDecimal.Field<currentBal> { }

		[PXDBDecimal(BqlTable = typeof(ARCurrentBalance))]
		public virtual Decimal? CurrentBal { get; set; }
		#endregion

		#region CurrentBalSigned
		public abstract class currentBalSum : PX.Data.BQL.BqlDecimal.Field<currentBalSum> { }

		[PXDBDecimal(4, BqlField = typeof(ARCurrentBalance.currentBalSigned))]
		public virtual decimal? CurrentBalSum { get; set; }
		#endregion
	}
}
