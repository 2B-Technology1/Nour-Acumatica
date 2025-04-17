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

namespace PX.Objects.GL
{
	/// <summary>
	/// The rules for balancing inter-branch transactions.
	/// </summary>
	[PXCacheName(Messages.BranchAcctMap)]
	[Serializable]
	[PXHidden]
	public partial class BranchAcctMap : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<BranchAcctMap>.By<branchID, lineNbr>
		{
			public static BranchAcctMap Find(PXGraph graph, int? branchID, int? lineNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, branchID, lineNbr, options);
		}
		public static class FK
		{
			public class Branch : GL.Branch.PK.ForeignKeyOf<BranchAcctMap>.By<branchID> { }
			public class FromBranch : GL.Branch.PK.ForeignKeyOf<BranchAcctMap>.By<fromBranchID> { }
			public class ToBranch : GL.Branch.PK.ForeignKeyOf<BranchAcctMap>.By<toBranchID> { }
			public class FromAccount : GL.Account.UK.ForeignKeyOf<BranchAcctMap>.By<fromAccountCD> { }
			public class ToAccount : GL.Account.UK.ForeignKeyOf<BranchAcctMap>.By<toAccountCD> { }
			public class MapAccount : GL.Account.PK.ForeignKeyOf<BranchAcctMap>.By<mapAccountID> { }
			public class MapSubaccount : GL.Sub.PK.ForeignKeyOf<BranchAcctMap>.By<mapSubID> { }
		}
		#endregion

		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(Branch.branchID))]
		public virtual int? BranchID
		{
			get;
			set;
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(Branch.acctMapNbr))]
		public virtual int? LineNbr
		{
			get;
			set;
		}
		#endregion
		#region FromBranchID
		public abstract class fromBranchID : PX.Data.BQL.BqlInt.Field<fromBranchID> { }
		[PXDBInt]
		public virtual int? FromBranchID
		{
			get;
			set;
		}
		#endregion
		#region ToBranchID
		public abstract class toBranchID : PX.Data.BQL.BqlInt.Field<toBranchID> { }
		[PXDBInt]
		public virtual int? ToBranchID
		{
			get;
			set;
		}
		#endregion
		#region FromAccountCD
		public abstract class fromAccountCD : PX.Data.BQL.BqlString.Field<fromAccountCD> { }
		[PXDefault]
		[AccountRaw]
		public virtual string FromAccountCD
		{
			get;
			set;
		}
		#endregion
		#region ToAccountCD
		public abstract class toAccountCD : PX.Data.BQL.BqlString.Field<toAccountCD> { }
		[PXDefault]
		[AccountRaw]
		public virtual string ToAccountCD
		{
			get;
			set;
		}
		#endregion
		#region MapAccountID
		public abstract class mapAccountID : PX.Data.BQL.BqlInt.Field<mapAccountID> { }
		[Account(DescriptionField = typeof(Account.description))]
		[PXDefault]
		[PXForeignReference(typeof(Field<BranchAcctMap.mapAccountID>.IsRelatedTo<Account.accountID>))]
        public virtual int? MapAccountID
		{
			get;
			set;
		}
		#endregion
		#region MapSubID
		public abstract class mapSubID : PX.Data.BQL.BqlInt.Field<mapSubID> { }
		[SubAccount(typeof(BranchAcctMap.mapAccountID), DescriptionField = typeof(Account.description))]
		[PXDefault]
		[PXForeignReference(typeof(Field<BranchAcctMap.mapSubID>.IsRelatedTo<Sub.subID>))]
        public virtual int? MapSubID
		{
			get;
			set;
		}
		#endregion
	}

	[PXProjection(typeof(Select<BranchAcctMap, Where<BranchAcctMap.branchID, Equal<BranchAcctMap.fromBranchID>>>), Persistent = true)]
	[PXCacheName(Messages.BranchAcctMapFrom)]
	[Serializable]
	public partial class BranchAcctMapFrom : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<BranchAcctMapFrom>.By<branchID, lineNbr>
		{
			public static BranchAcctMapFrom Find(PXGraph graph, int? branchID, int? lineNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, branchID, lineNbr, options);
		}
		public static class FK
		{
			public class Branch : GL.Branch.PK.ForeignKeyOf<BranchAcctMapFrom>.By<branchID> { }
			public class FromBranch : GL.Branch.PK.ForeignKeyOf<BranchAcctMapFrom>.By<fromBranchID> { }
			public class ToBranch : GL.Branch.PK.ForeignKeyOf<BranchAcctMapFrom>.By<toBranchID> { }
			public class FromAccount : GL.Account.UK.ForeignKeyOf<BranchAcctMapFrom>.By<fromAccountCD> { }
			public class ToAccount : GL.Account.UK.ForeignKeyOf<BranchAcctMapFrom>.By<toAccountCD> { }
			public class MapAccount : GL.Account.PK.ForeignKeyOf<BranchAcctMapFrom>.By<mapAccountID> { }
			public class MapSubaccount : GL.Sub.PK.ForeignKeyOf<BranchAcctMapFrom>.By<mapSubID> { }
		}
		#endregion

		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[PXDBInt(IsKey = true, BqlTable = typeof(BranchAcctMap))]
		[PXDBDefault(typeof(Branch.branchID))]
		public virtual int? BranchID
		{
			get;
			set;
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		[PXDBInt(IsKey = true, BqlTable = typeof(BranchAcctMap))]
		[PXLineNbr(typeof(Branch.acctMapNbr))]
		public virtual int? LineNbr
		{
			get;
			set;
		}
		#endregion
		#region FromBranchID
		public abstract class fromBranchID : PX.Data.BQL.BqlInt.Field<fromBranchID> { }
		[PXDBInt(BqlTable = typeof(BranchAcctMap))]
		[PXDBDefault(typeof(Branch.branchID))]
		public virtual int? FromBranchID
		{
			get;
			set;
		}
		#endregion
		#region ToBranchID
		public abstract class toBranchID : PX.Data.BQL.BqlInt.Field<toBranchID> { }
		[PXDBInt(BqlTable = typeof(BranchAcctMap))]
		[PXSelector(typeof(Search<Branch.branchID, Where<Branch.branchID, NotEqual<Current<BranchAcctMapFrom.branchID>>>>), SubstituteKey = typeof(Branch.branchCD))]
		[PXUIField(DisplayName = "Destination Branch")]
		[PXRestrictor(typeof(Where<Branch.active, Equal<True>>), GL.Messages.BranchInactive)]
		public virtual int? ToBranchID
		{
			get;
			set;
		}
		#endregion
		#region FromAccountCD
		public abstract class fromAccountCD : PX.Data.BQL.BqlString.Field<fromAccountCD> { }
		[PXDBString(10, IsUnicode = true, InputMask = "", BqlTable = typeof(BranchAcctMap))]
		[PXDefault]
		[PXDimensionSelector("ACCOUNT", typeof(Search<Account.accountCD,
				Where<Account.accountingType, Equal<AccountEntityType.gLAccount>>>), DescriptionField = typeof(Account.description))]
		[PXUIField(DisplayName = "Account From")]
		public virtual string FromAccountCD
		{
			get;
			set;
		}
		#endregion
		#region ToAccountCD
		public abstract class toAccountCD : PX.Data.BQL.BqlString.Field<toAccountCD> { }
		[PXDBString(10, IsUnicode = true, InputMask = "", BqlTable = typeof(BranchAcctMap))]
		[PXDefault]
		[PXDimensionSelector("ACCOUNT", typeof(Search<Account.accountCD,
				Where<Account.accountingType, Equal<AccountEntityType.gLAccount>>>), DescriptionField = typeof(Account.description))]
		[PXUIField(DisplayName = "Account To")]
		public virtual string ToAccountCD
		{
			get;
			set;
		}
		#endregion
		#region MapAccountID
		public abstract class mapAccountID : PX.Data.BQL.BqlInt.Field<mapAccountID> { }
		[Account(null, typeof(Search<Account.accountID,
			Where<Account.isCashAccount, Equal<False>, And<Account.curyID, IsNull>>>),
			DescriptionField = typeof(Account.description), BqlTable = typeof(BranchAcctMap), DisplayName = "Offset Account", AvoidControlAccounts = true)]
		[PXDefault]
		[PXForeignReference(typeof(Field<BranchAcctMapFrom.mapAccountID>.IsRelatedTo<Account.accountID>))]
		public virtual int? MapAccountID
		{
			get;
			set;
		}
		#endregion
		#region MapSubID
		public abstract class mapSubID : PX.Data.BQL.BqlInt.Field<mapSubID> { }
		[SubAccount(typeof(BranchAcctMapFrom.mapAccountID), DisplayName = "Offset Subaccount", DescriptionField = typeof(Account.description), BqlTable = typeof(BranchAcctMap))]
		[PXDefault]
		[PXForeignReference(typeof(Field<BranchAcctMapFrom.mapSubID>.IsRelatedTo<Sub.subID>))]
		public virtual int? MapSubID
		{
			get;
			set;
		}
		#endregion
	}

	[PXProjection(typeof(Select<BranchAcctMap, Where<BranchAcctMap.branchID, Equal<BranchAcctMap.toBranchID>>>), Persistent = true)]
	[PXCacheName(Messages.BranchAcctMapTo)]
	[Serializable]
	public partial class BranchAcctMapTo : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<BranchAcctMapTo>.By<branchID, lineNbr>
		{
			public static BranchAcctMapTo Find(PXGraph graph, int? branchID, int? lineNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, branchID, lineNbr, options);
		}
		public static class FK
		{
			public class Branch : GL.Branch.PK.ForeignKeyOf<BranchAcctMapTo>.By<branchID> { }
			public class FromBranch : GL.Branch.PK.ForeignKeyOf<BranchAcctMapTo>.By<fromBranchID> { }
			public class ToBranch : GL.Branch.PK.ForeignKeyOf<BranchAcctMapTo>.By<toBranchID> { }
			public class FromAccount : GL.Account.UK.ForeignKeyOf<BranchAcctMapTo>.By<fromAccountCD> { }
			public class ToAccount : GL.Account.UK.ForeignKeyOf<BranchAcctMapTo>.By<toAccountCD> { }
			public class MapAccount : GL.Account.PK.ForeignKeyOf<BranchAcctMapTo>.By<mapAccountID> { }
			public class MapSubaccount : GL.Sub.PK.ForeignKeyOf<BranchAcctMapTo>.By<mapSubID> { }
		}
		#endregion

		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[PXDBInt(IsKey = true, BqlTable = typeof(BranchAcctMap))]
		[PXDBDefault(typeof(Branch.branchID))]
		public virtual int? BranchID
		{
			get;
			set;
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		[PXDBInt(IsKey = true, BqlTable = typeof(BranchAcctMap))]
		[PXLineNbr(typeof(Branch.acctMapNbr))]
		public virtual int? LineNbr
		{
			get;
			set;
		}
		#endregion
		#region FromBranchID
		public abstract class fromBranchID : PX.Data.BQL.BqlInt.Field<fromBranchID> { }
		[PXDBInt(BqlTable = typeof(BranchAcctMap))]
		[PXSelector(typeof(Search<Branch.branchID, Where<Branch.branchID, NotEqual<Current<BranchAcctMapFrom.branchID>>>>), SubstituteKey = typeof(Branch.branchCD))]
		[PXUIField(DisplayName = "Destination Branch")]
		[PXRestrictor(typeof(Where<Branch.active, Equal<True>>), GL.Messages.BranchInactive)]
		public virtual int? FromBranchID
		{
			get;
			set;
		}
		#endregion
		#region ToBranchID
		public abstract class toBranchID : PX.Data.BQL.BqlInt.Field<toBranchID> { }
		[PXDBInt(BqlTable = typeof(BranchAcctMap))]
		[PXDBDefault(typeof(Branch.branchID))]
		public virtual int? ToBranchID
		{
			get;
			set;
		}
		#endregion
		#region FromAccountCD
		public abstract class fromAccountCD : PX.Data.BQL.BqlString.Field<fromAccountCD> { }
		[PXDBString(10, IsUnicode = true, InputMask = "", BqlTable = typeof(BranchAcctMap))]
		[PXDefault]
		[PXDimensionSelector("ACCOUNT", typeof(Search<Account.accountCD,
				Where<Account.accountingType, Equal<AccountEntityType.gLAccount>>>), DescriptionField = typeof(Account.description))]
		[PXUIField(DisplayName = "Account From")]
		public virtual string FromAccountCD
		{
			get;
			set;
		}
		#endregion
		#region ToAccountCD
		public abstract class toAccountCD : PX.Data.BQL.BqlString.Field<toAccountCD> { }
		[PXDBString(10, IsUnicode = true, InputMask = "", BqlTable = typeof(BranchAcctMap))]
		[PXDefault]
		[PXDimensionSelector("ACCOUNT", typeof(Search<Account.accountCD,
				Where<Account.accountingType, Equal<AccountEntityType.gLAccount>>>), DescriptionField = typeof(Account.description))]
		[PXUIField(DisplayName = "Account To")]
		public virtual string ToAccountCD
		{
			get;
			set;
		}
		#endregion
		#region MapAccountID
		public abstract class mapAccountID : PX.Data.BQL.BqlInt.Field<mapAccountID> { }
		[Account(null, typeof(Search<Account.accountID,
			Where<Account.isCashAccount, Equal<False>, And<Account.curyID, IsNull>>>),
			DescriptionField = typeof(Account.description), BqlTable = typeof(BranchAcctMap), DisplayName = "Offset Account", AvoidControlAccounts = true)]
		[PXDefault]
		[PXForeignReference(typeof(Field<BranchAcctMapTo.mapAccountID>.IsRelatedTo<Account.accountID>))]
		public virtual int? MapAccountID
		{
			get;
			set;
		}
		#endregion
		#region MapSubID
		public abstract class mapSubID : PX.Data.BQL.BqlInt.Field<mapSubID> { }
		[SubAccount(typeof(BranchAcctMapTo.mapAccountID), DisplayName = "Offset Subaccount", DescriptionField = typeof(Account.description), BqlTable = typeof(BranchAcctMap))]
		[PXDefault]
		[PXForeignReference(typeof(Field<BranchAcctMapTo.mapSubID>.IsRelatedTo<Sub.subID>))]
		public virtual int? MapSubID
		{
			get;
			set;
		}
		#endregion
	}
}
