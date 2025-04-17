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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.CA
{
	[Serializable]
	[PXProjection(typeof(Select5<CARecon,
		InnerJoin<CashAccount, 
			On<CashAccount.cashAccountID, Equal<CARecon.cashAccountID>>,
		InnerJoin<Branch,
			On<Branch.branchID, Equal<CashAccount.branchID>>,
		InnerJoin<OrganizationFinPeriod,
			On<OrganizationFinPeriod.endDate, Greater<CARecon.reconDate>,
			And<OrganizationFinPeriod.organizationID, Equal<Branch.organizationID>,
			And<CARecon.reconciled, Equal<boolTrue>, 
			And<CARecon.voided, Equal<boolFalse>>>>>>>>,
		Aggregate<GroupBy<CARecon.cashAccountID,
			Max<CARecon.reconDate,
			GroupBy<OrganizationFinPeriod.finPeriodID
		>>>>>))]
	[PXCacheName(Messages.CAReconByPeriod)]
	public partial class CAReconByPeriod : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<CAReconByPeriod>.By<cashAccountID, finPeriodID>
		{
			public static CAReconByPeriod Find(PXGraph graph, int? cashAccountID, string finPeriodID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, cashAccountID, finPeriodID, options);
		}

		public static class FK
		{
			public class CashAccount : CA.CashAccount.PK.ForeignKeyOf<CAReconByPeriod>.By<cashAccountID> { }
		}

		#endregion

		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }
		[CashAccount(IsKey = true, BqlField = typeof(CARecon.cashAccountID))]
		public virtual int? CashAccountID
		{
			get;
			set;
		}
		#endregion
		#region LastReconDate
		public abstract class lastReconDate : PX.Data.BQL.BqlDateTime.Field<lastReconDate> { }
		[PXDBDate(BqlField = typeof(CARecon.reconDate))]
		[PXUIField(DisplayName = "Last Reconciliation Date")]
		public virtual DateTime? LastReconDate
		{
			get;
			set;
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

		[FinPeriodID(IsKey = true, BqlField = typeof(OrganizationFinPeriod.finPeriodID))]
		public virtual string FinPeriodID
		{
			get;
			set;
		}
		#endregion
	}
}
