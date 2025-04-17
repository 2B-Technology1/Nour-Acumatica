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
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;
using System;

namespace PX.Objects.CS
{
	/// <summary>
	/// The DAC used to simplify selection and aggregation of proper <see cref="GLHistory"/> records
	/// on various inquiry and processing screens of the General Ledger module. The main purpose of this DAC is
	/// to close the gaps in GL history records, which appear in case GL history records do not exist for 
	/// every financial period defined in the system. To close these gaps, this projection DAC
	/// calculates the <see cref="LastActivityPeriod">last activity period</see> for every existing
	/// <see cref="FinPeriod">financial period</see>, so that inquiries and reports that produce information
	/// for a given financial period can look at the latest available <see cref="GLHistory"/> record.
	/// </summary>
	[PXProjection(typeof(Select5<GLHistory,
		LeftJoin<Account,
						On<GLHistory.accountID, Equal<Account.accountID>>,
		LeftJoin<MasterFinPeriod,
			On<MasterFinPeriod.finPeriodID, GreaterEqual<GLHistory.finPeriodID>>>>,
		Aggregate<
			Max<GLHistory.finPeriodID,
			Max<Account.accountClassID,
			GroupBy<GLHistory.branchID,
			GroupBy<GLHistory.ledgerID,
			GroupBy<GLHistory.accountID,
			GroupBy<GLHistory.subID,
			GroupBy<MasterFinPeriod.finPeriodID
		>>>>>>>>>))]
	[GLHistoryPrimaryGraph]
	[PXCacheName(GL.Messages.GLHistoryByPeriod)]
	public class ArmGLHistoryByPeriod : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<ArmGLHistoryByPeriod>.By<ledgerID, branchID, accountID, subID, finPeriodID>
		{
			public static ArmGLHistoryByPeriod Find(PXGraph graph, Int32? ledgerID, Int32? branchID, Int32? accountID, Int32? subID, String finPeriodID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, ledgerID, branchID, accountID, subID, finPeriodID, options);
		}
		public static class FK
		{
			public class Branch : GL.Branch.PK.ForeignKeyOf<ArmGLHistoryByPeriod>.By<branchID> { }
			public class Ledger : GL.Ledger.PK.ForeignKeyOf<ArmGLHistoryByPeriod>.By<ledgerID> { }
			public class Account : GL.Account.PK.ForeignKeyOf<ArmGLHistoryByPeriod>.By<accountID> { }
			public class Subaccount : GL.Sub.PK.ForeignKeyOf<ArmGLHistoryByPeriod>.By<subID> { }
		}
		#endregion

		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		/// <summary>
		/// Identifier of the <see cref="Branch"/>, which the history record belongs.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Branch.BAccountID"/> field.
		/// </value>
		[PXDBInt(IsKey = true, BqlField = typeof(GLHistory.branchID))]
		[PXUIField(DisplayName = "Branch")]
		[PXSelector(typeof(Branch.branchID), SubstituteKey = typeof(Branch.branchCD))]
		public virtual Int32? BranchID { get; set; }
		#endregion
		#region LedgerID
		public abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }

		/// <summary>
		/// Identifier of the <see cref="Ledger"/>, which the history record belongs.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Ledger.LedgerID"/> field.
		/// </value>
		[PXDBInt(IsKey = true, BqlField = typeof(GLHistory.ledgerID))]
		[PXUIField(DisplayName = "Ledger")]
		[PXSelector(typeof(Ledger.ledgerID), SubstituteKey = typeof(Ledger.ledgerCD))]
		public virtual Int32? LedgerID { get; set; }
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }

		/// <summary>
		/// Identifier of the <see cref="Account"/> associated with the history record.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Account.AccountID"/> field.
		/// </value>
		[Account(IsKey = true, BqlField = typeof(GLHistory.accountID))]
		public virtual Int32? AccountID { get; set; }
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }

		/// <summary>
		/// Identifier of the <see cref="Sub">Subaccount</see> associated with the history record.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Sub.SubID"/> field.
		/// </value>
		[SubAccount(IsKey = true, BqlField = typeof(GLHistory.subID))]
		public virtual Int32? SubID { get; set; }
		#endregion
		#region LastActivityPeriod
		public abstract class lastActivityPeriod : PX.Data.BQL.BqlString.Field<lastActivityPeriod> { }

		/// <summary>
		/// Identifier of the <see cref="PX.Objects.GL.Obsolete.FinPeriod">Financial Period</see> of the last activity on the Account and Subaccount associated with the history record,
		/// with regards to Ledger and Branch.
		/// </summary>
		[GL.FinPeriodID(BqlField = typeof(GLHistory.finPeriodID))]
		[PXUIField(DisplayName = "Last Activity Period", Visibility = PXUIVisibility.Invisible)]
		public virtual String LastActivityPeriod { get; set; }
		#endregion
		#region AccountClassID
		public abstract class accountClassID : PX.Data.BQL.BqlInt.Field<accountClassID> { }

		[PXDBString(20, IsUnicode = true, BqlField = typeof(Account.accountClassID))]
		public virtual string AccountClassID { get; set; }
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

		/// <summary>
		/// Identifier of the <see cref="PX.Objects.GL.Obsolete.FinPeriod">Financial Period</see>, for which the history data is given.
		/// </summary>
		[PXUIField(DisplayName = "Financial Period", Visibility = PXUIVisibility.Invisible)]
		[GL.FinPeriodID(IsKey = true, BqlField = typeof(MasterFinPeriod.finPeriodID))]
		public virtual String FinPeriodID { get; set; }
		#endregion
		#region FinYear
		public abstract class finYear : PX.Data.IBqlField { }

		/// <summary>
		/// Financial year, to which history data belongs
		/// </summary>
		[PXString]
		public virtual String FinYear
		{
			get => FinPeriodID?.Substring(0, 4);
			set { }
		}
		#endregion
	}

}
