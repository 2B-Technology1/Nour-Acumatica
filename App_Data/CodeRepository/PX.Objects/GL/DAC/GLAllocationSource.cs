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

namespace PX.Objects.GL
{
	using System;
	using PX.Data;
	using PX.Data.ReferentialIntegrity.Attributes;
	using PX.Objects.GL.DAC;

	[System.SerializableAttribute()]
	[PXCacheName(Messages.GLAllocationSource)]
	public partial class GLAllocationSource : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<GLAllocationSource>.By<gLAllocationID, lineID>
		{
			public static GLAllocationSource Find(PXGraph graph, String gLAllocationID, Int32? lineID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, gLAllocationID, lineID, options);
		}
		public static class FK
		{
			public class Branch : GL.Branch.PK.ForeignKeyOf<GLAllocationSource>.By<branchID> { }
			public class Allocation : GL.GLAllocation.PK.ForeignKeyOf<GLAllocationSource>.By<gLAllocationID> { }
			public class Account : GL.Account.UK.ForeignKeyOf<GLAllocationSource>.By<accountCD> { }
			public class Subaccount : GL.Sub.UK.ForeignKeyOf<GLAllocationSource>.By<subCD> { }
			public class ContraAccount : GL.Account.PK.ForeignKeyOf<GLAllocationSource>.By<contrAccountID> { }
			public class ContraSubaccount : GL.Sub.PK.ForeignKeyOf<GLAllocationSource>.By<contrSubID> { }
		}
		#endregion

		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[PXDefault(typeof(Search2<Branch.branchID,
					InnerJoin<OrganizationLedgerLink,
						On<OrganizationLedgerLink.organizationID, Equal<Branch.organizationID>,
							And<OrganizationLedgerLink.ledgerID, Equal<Current<GLAllocation.sourceLedgerID>>,
							And<Branch.branchID, Equal<Current<GLAllocation.branchID>>>>>>,
					Where<Match<Current<AccessInfo.userName>>>>))]
		[Branch(null, typeof(Search2<Branch.branchID,
					InnerJoin<Organization,
							On<Organization.organizationID, Equal<Branch.organizationID>>,
					 InnerJoin<OrganizationLedgerLink,
						  On<Branch.organizationID, Equal<OrganizationLedgerLink.organizationID>, And<OrganizationLedgerLink.ledgerID, Equal<Current<GLAllocation.sourceLedgerID>>>>>>,
					 Where2<Match<Current<AccessInfo.userName>>, And<MatchWithBranch<Branch.branchID>>>>),
			useDefaulting: false)]
		public virtual Int32? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion
		#region GLAllocationID
		public abstract class gLAllocationID : PX.Data.BQL.BqlString.Field<gLAllocationID> { }
		protected String _GLAllocationID;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(GLAllocation.gLAllocationID))]
		[PXUIField(DisplayName = "Allocation ID", Visible = false)]
		[PXParent(typeof(Select<GLAllocation,Where<GLAllocation.gLAllocationID,Equal<Current<GLAllocationSource.gLAllocationID>>>>))]
		public virtual String GLAllocationID
		{
			get
			{
				return this._GLAllocationID;
			}
			set
			{
				this._GLAllocationID = value;
			}
		}
		#endregion
		#region LineID
		public abstract class lineID : PX.Data.BQL.BqlInt.Field<lineID> { }
		protected Int32? _LineID;
		[PXDBIdentity(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.", Visible =false)]
		public virtual Int32? LineID
		{
			get
			{
				return this._LineID;
			}
			set
			{
				this._LineID = value;
			}
		}
		#endregion
		#region AccountCD
		public abstract class accountCD : PX.Data.BQL.BqlString.Field<accountCD> { }
		protected String _AccountCD;
		[AccountCDWildcard(typeof(Search<Account.accountCD, Where<Account.active, Equal<True>,
								And<Account.accountingType, Equal<AccountEntityType.gLAccount>>>>), DescriptionField = typeof(Account.description))]
		[PXDefault()]
		public virtual String AccountCD
		{
			get
			{
				return this._AccountCD;
			}
			set
			{
				this._AccountCD = value;
			}
		}
		#endregion
		#region SubCD
		public abstract class subCD : PX.Data.BQL.BqlString.Field<subCD> { }
		protected String _SubCD;
		[SubCDWildcard()]
		[PXDefault()]
		public virtual String SubCD
		{
			get
			{
				return this._SubCD;
			}
			set
			{
				this._SubCD = value;
			}
		}
		#endregion
		#region ContrAccountID
		public abstract class contrAccountID : PX.Data.BQL.BqlInt.Field<contrAccountID> { }
		protected Int32? _ContrAccountID;
		[Account(null,typeof(Search2<Account.accountID, LeftJoin<GLSetup, On<GLSetup.ytdNetIncAccountID, Equal<Account.accountID>>,
							LeftJoin<Ledger,On<Ledger.ledgerID,Equal<Current<GLAllocation.allocLedgerID>>>>>,
						 Where2<Match<Current<AccessInfo.userName>>,
						 And<Account.active, Equal<True>,
						And<Where<Account.curyID, IsNull, Or<Account.curyID, Equal<Ledger.baseCuryID>,                        
					   And<GLSetup.ytdNetIncAccountID, IsNull>>>>>>>), DisplayName = "Contra Account", Visibility = PXUIVisibility.Visible)]
		public virtual Int32? ContrAccountID
		{
			get
			{
				return this._ContrAccountID;
			}
			set
			{
				this._ContrAccountID = value;
			}
		}
		#endregion
		#region ContrSubID
		public abstract class contrSubID : PX.Data.BQL.BqlInt.Field<contrSubID> { }
		protected Int32? _ContrSubID;
		[SubAccount(typeof(GLAllocationSource.contrAccountID), DisplayName = "Contra Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? ContrSubID
		{
			get
			{
				return this._ContrSubID;
			}
			set
			{
				this._ContrSubID = value;
			}
		}
		#endregion
		#region PercentLimitType
		public abstract class percentLimitType : PX.Data.BQL.BqlString.Field<percentLimitType> { }
		protected String _PercentLimitType;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Percent Limit Type", Visible = false)]
		[Constants.PercentLimitType.List]
		public virtual String PercentLimitType
		{
			get
			{
				return this._PercentLimitType;
			}
			set
			{
				this._PercentLimitType = value;
			}
		}
		#endregion
		#region LimitAmount
		public abstract class limitAmount : PX.Data.BQL.BqlDecimal.Field<limitAmount> { }
		protected Decimal? _LimitAmount;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[CM.PXDBBaseCury(typeof(GLAllocation.sourceLedgerID), MinValue = 0)]
		[PXUIField(DisplayName = "Amount Limit")]
		public virtual Decimal? LimitAmount
		{
			get
			{
				return this._LimitAmount;
			}
			set
			{
				this._LimitAmount = value;
			}
		}
		#endregion
		#region LimitPercent
		public abstract class limitPercent : PX.Data.BQL.BqlDecimal.Field<limitPercent> { }
		protected Decimal? _LimitPercent;
		[PXDBDecimal(2, MinValue = 0, MaxValue = 100)]
		[PXDefault(TypeCode.Decimal,"100.0")]
		[PXUIField(DisplayName = "Percentage Limit")]
		public virtual Decimal? LimitPercent
		{
			get
			{
				return this._LimitPercent;
			}
			set
			{
				this._LimitPercent = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
	}
}
