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
using PX.Objects.CM;
using PX.Objects.CS;

namespace PX.Objects.GL.DAC
{
	[PXProjection(typeof(Select<GLHistorySum>))]
	[PXHidden]
	public partial class GLHistoryCalcRetainedEarnings : IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		[PXDBInt(IsKey = true, BqlField = typeof(GLHistorySum.branchID))]
		public virtual int? BranchID { get; set; }
		#endregion
		#region LedgerID
		public abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }

		[PXDBInt(IsKey = true, BqlField = typeof(GLHistorySum.ledgerID))]
		public virtual int? LedgerID { get; set; }
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }

		[Account(IsKey = true, BqlField = typeof(GLHistorySum.accountID))]
		public virtual int? AccountID { get; set; }
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }

		[SubAccount(IsKey = true, BqlField = typeof(GLHistorySum.subID))]
		public virtual int? SubID { get; set; }
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

		[GL.FinPeriodID(IsKey = true, BqlField = typeof(GLHistorySum.finPeriodID))]
		public virtual string FinPeriodID { get; set; }
		#endregion
		#region FinPtdCredit
		public abstract class finPtdCredit : PX.Data.BQL.BqlDecimal.Field<finPtdCredit> { }

		[PXDBBaseCury(typeof(GLHistorySum.ledgerID), BqlField = typeof(GLHistorySum.finPtdCredit))]
		public virtual decimal? FinPtdCredit { get; set; }
		#endregion
		#region FinPtdDebit
		public abstract class finPtdDebit : PX.Data.BQL.BqlDecimal.Field<finPtdDebit> { }

		[PXDBBaseCury(typeof(GLHistorySum.ledgerID), BqlField = typeof(GLHistorySum.finPtdDebit))]
		public virtual decimal? FinPtdDebit { get; set; }
		#endregion
		#region TranPtdCredit
		public abstract class tranPtdCredit : PX.Data.BQL.BqlDecimal.Field<tranPtdCredit> { }

		[PXDBBaseCury(typeof(GLHistorySum.ledgerID), BqlField = typeof(GLHistorySum.tranPtdCredit))]
		public virtual decimal? TranPtdCredit { get; set; }
		#endregion
		#region TranPtdDebit
		public abstract class tranPtdDebit : PX.Data.BQL.BqlDecimal.Field<tranPtdDebit> { }

		[PXDBBaseCury(typeof(GLHistorySum.ledgerID), BqlField = typeof(GLHistorySum.tranPtdDebit))]
		public virtual decimal? TranPtdDebit { get; set; }
		#endregion
		#region FinBegBalanceNew
		public abstract class finBegBalanceNew : PX.Data.BQL.BqlDecimal.Field<finBegBalanceNew> { }

		[PXBaseCury]
		[PXDBCalced(typeof(Add<IsNull<GLHistorySum.finPtdNetIncomeSum, decimal0>,
							IsNull<GLHistorySum.finPtdRetEarnSum, decimal0>>), typeof(decimal))]
		public virtual decimal? FinBegBalanceNew { get; set; }
		#endregion
		#region FinYtdBalanceNew
		public abstract class finYtdBalanceNew : PX.Data.BQL.BqlDecimal.Field<finYtdBalanceNew> { }

		[PXBaseCury]
		[PXDBCalced(typeof(Add<IsNull<GLHistorySum.finPtdNetIncomeSum, decimal0>,
							Add<IsNull<GLHistorySum.finPtdRetEarnSum, decimal0>,
							Sub<IsNull<GLHistoryCalcRetainedEarnings.finPtdCredit, decimal0>,
							IsNull<GLHistoryCalcRetainedEarnings.finPtdDebit, decimal0>>>>), typeof(decimal))]
		public virtual decimal? FinYtdBalanceNew { get; set; }
		#endregion
		#region TranBegBalanceNew
		public abstract class tranBegBalanceNew : PX.Data.BQL.BqlDecimal.Field<tranBegBalanceNew> { }

		[PXBaseCury]
		[PXDBCalced(typeof(Add<IsNull<GLHistorySum.tranPtdNetIncomeSum, decimal0>,
							IsNull<GLHistorySum.tranPtdRetEarnSum, decimal0>>), typeof(decimal))]
		public virtual decimal? TranBegBalanceNew { get; set; }
		#endregion
		#region TranYtdBalanceNew
		public abstract class tranYtdBalanceNew : PX.Data.BQL.BqlDecimal.Field<tranYtdBalanceNew> { }

		[PXBaseCury]
		[PXDBCalced(typeof(Add<IsNull<GLHistorySum.tranPtdNetIncomeSum, decimal0>,
							Add<IsNull<GLHistorySum.tranPtdRetEarnSum, decimal0>,
							Sub<IsNull<GLHistoryCalcRetainedEarnings.tranPtdCredit, decimal0>,
							IsNull<GLHistoryCalcRetainedEarnings.tranPtdDebit, decimal0>>>>), typeof(decimal))]
		public virtual decimal? TranYtdBalanceNew { get; set; }
		#endregion
		#region CuryFinPtdCredit
		public abstract class curyFinPtdCredit : PX.Data.BQL.BqlDecimal.Field<curyFinPtdCredit> { }

		[PXDBBaseCury(typeof(GLHistorySum.ledgerID), BqlField = typeof(GLHistorySum.curyFinPtdCredit))]
		public virtual decimal? CuryFinPtdCredit { get; set; }
		#endregion
		#region CuryFinPtdDebit
		public abstract class curyFinPtdDebit : PX.Data.BQL.BqlDecimal.Field<curyFinPtdDebit> { }

		[PXDBBaseCury(typeof(GLHistorySum.ledgerID), BqlField = typeof(GLHistorySum.curyFinPtdDebit))]
		public virtual decimal? CuryFinPtdDebit { get; set; }
		#endregion
		#region CuryTranPtdCredit
		public abstract class curyTranPtdCredit : PX.Data.BQL.BqlDecimal.Field<curyTranPtdCredit> { }

		[PXDBBaseCury(typeof(GLHistorySum.ledgerID), BqlField = typeof(GLHistorySum.curyTranPtdCredit))]
		public virtual decimal? CuryTranPtdCredit { get; set; }
		#endregion
		#region CuryTranPtdDebit
		public abstract class curyTranPtdDebit : PX.Data.BQL.BqlDecimal.Field<curyTranPtdDebit> { }

		[PXDBBaseCury(typeof(GLHistorySum.ledgerID), BqlField = typeof(GLHistorySum.curyTranPtdDebit))]
		public virtual decimal? CuryTranPtdDebit { get; set; }
		#endregion
		#region CuryFinBegBalanceNew
		public abstract class curyFinBegBalanceNew : PX.Data.BQL.BqlDecimal.Field<curyFinBegBalanceNew> { }

		[PXBaseCury]
		[PXDBCalced(typeof(Add<IsNull<GLHistorySum.curyFinPtdNetIncomeSum, decimal0>,
							IsNull<GLHistorySum.curyFinPtdRetEarnSum, decimal0>>), typeof(decimal))]
		public virtual decimal? CuryFinBegBalanceNew { get; set; }
		#endregion
		#region CuryFinYtdBalanceNew
		public abstract class curyFinYtdBalanceNew : PX.Data.BQL.BqlDecimal.Field<curyFinYtdBalanceNew> { }

		[PXBaseCury]
		[PXDBCalced(typeof(Add<IsNull<GLHistorySum.curyFinPtdNetIncomeSum, decimal0>,
							Add<IsNull<GLHistorySum.curyFinPtdRetEarnSum, decimal0>,
							Sub<IsNull<GLHistoryCalcRetainedEarnings.curyFinPtdCredit, decimal0>,
							IsNull<GLHistoryCalcRetainedEarnings.curyFinPtdDebit, decimal0>>>>), typeof(decimal))]
		public virtual decimal? CuryFinYtdBalanceNew { get; set; }
		#endregion
		#region CuryTranBegBalanceNew
		public abstract class curyTranBegBalanceNew : PX.Data.BQL.BqlDecimal.Field<curyTranBegBalanceNew> { }

		[PXBaseCury]
		[PXDBCalced(typeof(Add<IsNull<GLHistorySum.curyTranPtdNetIncomeSum, decimal0>,
							IsNull<GLHistorySum.curyTranPtdRetEarnSum, decimal0>>), typeof(decimal))]
		public virtual decimal? CuryTranBegBalanceNew { get; set; }
		#endregion
		#region CuryTranYtdBalanceNew
		public abstract class curyTranYtdBalanceNew : PX.Data.BQL.BqlDecimal.Field<curyTranYtdBalanceNew> { }

		[PXBaseCury]
		[PXDBCalced(typeof(Add<IsNull<GLHistorySum.curyTranPtdNetIncomeSum, decimal0>,
							Add<IsNull<GLHistorySum.curyTranPtdRetEarnSum, decimal0>,
							Sub<IsNull<GLHistoryCalcRetainedEarnings.curyTranPtdCredit, decimal0>,
							IsNull<GLHistoryCalcRetainedEarnings.curyTranPtdDebit, decimal0>>>>), typeof(decimal))]
		public virtual decimal? CuryTranYtdBalanceNew { get; set; }
		#endregion
	}

	[PXHidden]
	public partial class GLHistorySum : GLHistory
	{
		#region FinPtdNetIncomeSum
		public abstract class finPtdNetIncomeSum : PX.Data.BQL.BqlDecimal.Field<finPtdNetIncomeSum> { }

		[PXBaseCury]
		[PXDBScalar(typeof(Search4<
				GLHistoryNet.finPtdNetIncome,
			Where<
				GLHistoryNet.branchID, Equal<GLHistorySum.branchID>,
				And<GLHistoryNet.ledgerID, Equal<GLHistorySum.ledgerID>,
				And<GLHistoryNet.accountID, Equal<CurrentValue<GLSetup.ytdNetIncAccountID>>,
				And<GLHistoryNet.subID, Equal<GLHistorySum.subID>,
				And<GLHistoryNet.finPeriodID, LessEqual<PX.Data.Substring<GLHistorySum.finPeriodID, int1, int4>>>>>>>,
			Aggregate<
				Sum<GLHistoryNet.finPtdNetIncome>>>))]
		public virtual decimal? FinPtdNetIncomeSum { get; set; }
		#endregion
		#region FinPtdRetEarnSum
		public abstract class finPtdRetEarnSum : PX.Data.BQL.BqlDecimal.Field<finPtdRetEarnSum> { }

		[PXBaseCury]
		[PXDBScalar(typeof(Search4<
				GLHistoryNet.finPtdNetIncome,
			Where<
				GLHistoryNet.branchID, Equal<GLHistorySum.branchID>,
				And<GLHistoryNet.ledgerID, Equal<GLHistorySum.ledgerID>,
				And<GLHistoryNet.accountID, Equal<CurrentValue<GLSetup.retEarnAccountID>>,
				And<GLHistoryNet.subID, Equal<GLHistorySum.subID>,
				And<GLHistoryNet.finPeriodID, Less<GLHistorySum.finPeriodID>>>>>>,
			Aggregate<
				Sum<GLHistoryNet.finPtdNetIncome>>>))]
		public virtual decimal? FinPtdRetEarnSum { get; set; }
		#endregion
		#region TranPtdNetIncomeSum
		public abstract class tranPtdNetIncomeSum : PX.Data.BQL.BqlDecimal.Field<tranPtdNetIncomeSum> { }

		[PXBaseCury]
		[PXDBScalar(typeof(Search4<
				GLHistoryNet.tranPtdNetIncome,
			Where<
				GLHistoryNet.branchID, Equal<GLHistorySum.branchID>,
				And<GLHistoryNet.ledgerID, Equal<GLHistorySum.ledgerID>,
				And<GLHistoryNet.accountID, Equal<CurrentValue<GLSetup.ytdNetIncAccountID>>,
				And<GLHistoryNet.subID, Equal<GLHistorySum.subID>,
				And<GLHistoryNet.finPeriodID, LessEqual<PX.Data.Substring<GLHistorySum.finPeriodID, int1, int4>>>>>>>,
			Aggregate<
				Sum<GLHistoryNet.tranPtdNetIncome>>>))]
		public virtual decimal? TranPtdNetIncomeSum { get; set; }
		#endregion
		#region TranPtdRetEarnSum
		public abstract class tranPtdRetEarnSum : PX.Data.BQL.BqlDecimal.Field<tranPtdRetEarnSum> { }

		[PXBaseCury]
		[PXDBScalar(typeof(Search4<
				GLHistoryNet.tranPtdNetIncome,
			Where<
				GLHistoryNet.branchID, Equal<GLHistorySum.branchID>,
				And<GLHistoryNet.ledgerID, Equal<GLHistorySum.ledgerID>,
				And<GLHistoryNet.accountID, Equal<CurrentValue<GLSetup.retEarnAccountID>>,
				And<GLHistoryNet.subID, Equal<GLHistorySum.subID>,
				And<GLHistoryNet.finPeriodID, Less<GLHistorySum.finPeriodID>>>>>>,
			Aggregate<
				Sum<GLHistoryNet.tranPtdNetIncome>>>))]
		public virtual decimal? TranPtdRetEarnSum { get; set; }
		#endregion
		#region CuryFinPtdNetIncomeSum
		public abstract class curyFinPtdNetIncomeSum : PX.Data.BQL.BqlDecimal.Field<curyFinPtdNetIncomeSum> { }

		[PXDBDecimal(4)]
		[PXDBScalar(typeof(Search4<
				GLHistoryNet.curyFinPtdNetIncome,
			Where<
				GLHistoryNet.branchID, Equal<GLHistorySum.branchID>,
				And<GLHistoryNet.ledgerID, Equal<GLHistorySum.ledgerID>,
				And<GLHistoryNet.accountID, Equal<CurrentValue<GLSetup.ytdNetIncAccountID>>,
				And<GLHistoryNet.subID, Equal<GLHistorySum.subID>,
				And<GLHistoryNet.finPeriodID, LessEqual<PX.Data.Substring<GLHistorySum.finPeriodID, int1, int4>>>>>>>,
			Aggregate<
				Sum<GLHistoryNet.curyFinPtdNetIncome>>>))]
		public virtual decimal? CuryFinPtdNetIncomeSum { get; set; }
		#endregion
		#region CuryFinPtdRetEarnSum
		public abstract class curyFinPtdRetEarnSum : PX.Data.BQL.BqlDecimal.Field<curyFinPtdRetEarnSum> { }

		[PXDBDecimal(4)]
		[PXDBScalar(typeof(Search4<
				GLHistoryNet.curyFinPtdNetIncome,
			Where<
				GLHistoryNet.branchID, Equal<GLHistorySum.branchID>,
				And<GLHistoryNet.ledgerID, Equal<GLHistorySum.ledgerID>,
				And<GLHistoryNet.accountID, Equal<CurrentValue<GLSetup.retEarnAccountID>>,
				And<GLHistoryNet.subID, Equal<GLHistorySum.subID>,
				And<GLHistoryNet.finPeriodID, Less<GLHistorySum.finPeriodID>>>>>>,
			Aggregate<
				Sum<GLHistoryNet.curyFinPtdNetIncome>>>))]
		public virtual decimal? CuryFinPtdRetEarnSum { get; set; }
		#endregion
		#region CuryTranPtdNetIncomeSum
		public abstract class curyTranPtdNetIncomeSum : PX.Data.BQL.BqlDecimal.Field<curyTranPtdNetIncomeSum> { }

		[PXDBDecimal(4)]
		[PXDBScalar(typeof(Search4<
				GLHistoryNet.curyTranPtdNetIncome,
			Where<
				GLHistoryNet.branchID, Equal<GLHistorySum.branchID>,
				And<GLHistoryNet.ledgerID, Equal<GLHistorySum.ledgerID>,
				And<GLHistoryNet.accountID, Equal<CurrentValue<GLSetup.ytdNetIncAccountID>>,
				And<GLHistoryNet.subID, Equal<GLHistorySum.subID>,
				And<GLHistoryNet.finPeriodID, LessEqual<PX.Data.Substring<GLHistorySum.finPeriodID, int1, int4>>>>>>>,
			Aggregate<
				Sum<GLHistoryNet.curyTranPtdNetIncome>>>))]
		public virtual decimal? CuryTranPtdNetIncomeSum { get; set; }
		#endregion
		#region CuryTranPtdRetEarnSum
		public abstract class curyTranPtdRetEarnSum : PX.Data.BQL.BqlDecimal.Field<curyTranPtdRetEarnSum> { }

		[PXDBDecimal(4)]
		[PXDBScalar(typeof(Search4<
				GLHistoryNet.curyTranPtdNetIncome,
			Where<
				GLHistoryNet.branchID, Equal<GLHistorySum.branchID>,
				And<GLHistoryNet.ledgerID, Equal<GLHistorySum.ledgerID>,
				And<GLHistoryNet.accountID, Equal<CurrentValue<GLSetup.retEarnAccountID>>,
				And<GLHistoryNet.subID, Equal<GLHistorySum.subID>,
				And<GLHistoryNet.finPeriodID, Less<GLHistorySum.finPeriodID>>>>>>,
			Aggregate<
				Sum<GLHistoryNet.curyTranPtdNetIncome>>>))]
		public virtual decimal? CuryTranPtdRetEarnSum { get; set; }
		#endregion
	}

	[PXProjection(typeof(Select<GLHistory>))]
	[PXHidden]
	public class GLHistoryNet : PX.Data.IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		[PXDBInt(IsKey = true, BqlField = typeof(GLHistory.branchID))]
		public virtual int? BranchID { get; set; }
		#endregion
		#region LedgerID
		public abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }

		[PXDBInt(IsKey = true, BqlField = typeof(GLHistory.ledgerID))]
		public virtual int? LedgerID { get; set; }
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }

		[Account(IsKey = true, BqlField = typeof(GLHistory.accountID))]
		public virtual int? AccountID { get; set; }
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }

		[SubAccount(IsKey = true, BqlField = typeof(GLHistory.subID))]
		public virtual int? SubID { get; set; }
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

		[GL.FinPeriodID(IsKey = true, BqlField = typeof(GLHistory.finPeriodID))]
		public virtual string FinPeriodID { get; set; }
		#endregion
		#region FinPtdCredit
		public abstract class finPtdCredit : PX.Data.BQL.BqlDecimal.Field<finPtdCredit> { }

		[PXDBBaseCury(typeof(GLHistory.ledgerID), BqlField = typeof(GLHistory.finPtdCredit))]
		public virtual decimal? FinPtdCredit { get; set; }
		#endregion
		#region FinPtdDebit
		public abstract class finPtdDebit : PX.Data.BQL.BqlDecimal.Field<finPtdDebit> { }

		[PXDBBaseCury(typeof(GLHistory.ledgerID), BqlField = typeof(GLHistory.finPtdDebit))]
		public virtual decimal? FinPtdDebit { get; set; }
		#endregion

		#region FinPtdNetIncome
		public abstract class finPtdNetIncome : PX.Data.BQL.BqlDecimal.Field<finPtdNetIncome> { }

		[PXBaseCury]
		[PXDBCalced(typeof(GLHistory.finPtdCredit.Subtract<GLHistory.finPtdDebit>), typeof(decimal))]
		public virtual decimal? FinPtdNetIncome { get; set; }

		#endregion
		#region TranPtdNetIncome
		public abstract class tranPtdNetIncome : PX.Data.BQL.BqlDecimal.Field<tranPtdNetIncome> { }

		[PXBaseCury]
		[PXDBCalced(typeof(GLHistory.tranPtdCredit.Subtract<GLHistory.tranPtdDebit>), typeof(decimal))]
		public virtual decimal? TranPtdNetIncome { get; set; }
		#endregion
		#region CuryFinPtdCredit
		public abstract class curyFinPtdCredit : PX.Data.BQL.BqlDecimal.Field<curyFinPtdCredit> { }

		[PXDBBaseCury(typeof(GLHistory.ledgerID), BqlField = typeof(GLHistory.curyFinPtdCredit))]
		public virtual decimal? CuryFinPtdCredit { get; set; }
		#endregion
		#region CuryFinPtdDebit
		public abstract class curyFinPtdDebit : PX.Data.BQL.BqlDecimal.Field<curyFinPtdDebit> { }

		[PXDBBaseCury(typeof(GLHistory.ledgerID), BqlField = typeof(GLHistory.curyFinPtdDebit))]
		public virtual decimal? CuryFinPtdDebit { get; set; }
		#endregion
		#region CuryFinPtdNetIncome
		public abstract class curyFinPtdNetIncome : PX.Data.BQL.BqlDecimal.Field<curyFinPtdNetIncome> { }

		[PXBaseCury]
		[PXDBCalced(typeof(GLHistory.curyFinPtdCredit.Subtract<GLHistory.curyFinPtdDebit>), typeof(decimal))]
		public virtual decimal? CuryFinPtdNetIncome { get; set; }

		#endregion
		#region TranPtdNetIncome
		public abstract class curyTranPtdNetIncome : PX.Data.BQL.BqlDecimal.Field<curyTranPtdNetIncome> { }

		[PXBaseCury]
		[PXDBCalced(typeof(GLHistory.curyTranPtdCredit.Subtract<GLHistory.curyTranPtdDebit>), typeof(decimal))]
		public virtual decimal? CuryTranPtdNetIncome { get; set; }
		#endregion
	}
}
