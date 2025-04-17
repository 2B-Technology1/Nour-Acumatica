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
using PX.Objects.GL;
using PX.Objects.AP;
using PX.Objects.CM;

namespace ReconciliationTools
{
	#region Internal Types

	[Serializable]
	public partial class APGLDiscrepancyEnqFilter : DiscrepancyEnqFilter
	{
		#region AccountID
		public new abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }

		[Account(null, typeof(Search5<Account.accountID,
			InnerJoin<APHistory, On<Account.accountID, Equal<APHistory.accountID>>>,
			Where<Match<Current<AccessInfo.userName>>>,
			Aggregate<GroupBy<Account.accountID>>>),
			DisplayName = "Account", DescriptionField = typeof(Account.description))]
		public override int? AccountID
		{
			get;
			set;
		}
		#endregion
		#region TotalXXAmount
		public new abstract class totalXXAmount : PX.Data.BQL.BqlDecimal.Field<totalXXAmount> { }

		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total AP Amount", Enabled = false)]
		public override decimal? TotalXXAmount
		{
			get;
			set;
		}
		#endregion
	}

	#endregion

	[TableAndChartDashboardType]
	public class APGLDiscrepancyEnqGraphBase<TGraph, TEnqFilter, TEnqResult> : DiscrepancyEnqGraphBase<TGraph, TEnqFilter, TEnqResult>
		where TGraph : PXGraph
		where TEnqFilter : DiscrepancyEnqFilter, new()
		where TEnqResult : class, IBqlTable, IDiscrepancyEnqResult, new()
	{
		protected override decimal CalcGLTurnover(GLTran tran)
		{
			return (tran.CreditAmt ?? 0m) - (tran.DebitAmt ?? 0m);
		}
	}
}