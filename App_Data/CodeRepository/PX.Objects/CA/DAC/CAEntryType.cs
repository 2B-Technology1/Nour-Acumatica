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
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.CA
{
	/// <summary>
	/// An entry type that can be used in cash management.
	/// </summary>
	[Serializable]
	[PXPrimaryGraph(typeof(EntryTypeMaint))]
	[PXCacheName(Messages.CAEntryType)]
	public partial class CAEntryType : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<CAEntryType>.By<entryTypeId>
		{
			public static CAEntryType Find(PXGraph graph, String entryTypeId, PKFindOptions options = PKFindOptions.None) => FindBy(graph, entryTypeId, options);
		}
		public static class FK
		{
			public class DefaultOffsetBranch : GL.Branch.PK.ForeignKeyOf<CAEntryType>.By<branchID> { }
			public class CashAccount : CA.CashAccount.PK.ForeignKeyOf<CAEntryType>.By<cashAccountID> { }
			public class Account : GL.Account.PK.ForeignKeyOf<CAEntryType>.By<accountID> { }
			public class Subaccount : GL.Sub.PK.ForeignKeyOf<CAEntryType>.By<subID> { }
			public class BusinessAccount : CR.BAccount.PK.ForeignKeyOf<CAEntryType>.By<referenceID> { }
		}
		#endregion

		#region EntryTypeId
		public abstract class entryTypeId : PX.Data.BQL.BqlString.Field<entryTypeId> { }

		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Entry Type ID", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string EntryTypeId
			{
			get;
			set;
		}
		#endregion
		#region OrigModule
		public abstract class module : PX.Data.BQL.BqlString.Field<module> { }

		[PXDBString(2, IsFixed = true)]
		[PXDefault(BatchModule.CA)]
		[BatchModule.CashManagerList]
		[PXUIField(DisplayName = "Module", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string Module
			{
			get;
			set;
		}
		#endregion
        #region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }
        
        [CashAccountScalar(DisplayName = "Reclassification Account", Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(CashAccount.descr), Enabled = false)]
        [PXDBScalar(typeof(Search<CashAccount.cashAccountID, Where<CashAccount.accountID, Equal<CAEntryType.accountID>,
                                   And<CashAccount.subID, Equal<CAEntryType.subID>, And<CashAccount.branchID, Equal<CAEntryType.branchID>>>>>))]
		public virtual int? CashAccountID
            {
			get;
			set;
        }
        #endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }

		[Account(DescriptionField = typeof(Account.description), DisplayName = "Default Offset Account", Enabled = false, AvoidControlAccounts = true)]
		public virtual int? AccountID
			{
			get;
			set;
		}
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }

		[SubAccount(typeof(CAEntryType.accountID), DisplayName = "Default Offset Subaccount", Enabled = false)]
		public virtual int? SubID
		{
			get;
			set;
		}
		#endregion
        #region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

        [Branch(DisplayName = "Default Offset Account Branch", PersistingCheck = PXPersistingCheck.Nothing, Enabled = false)]
		public virtual int? BranchID
            {
			get;
			set;
        }
        #endregion
		#region ReferenceID
		public abstract class referenceID : PX.Data.BQL.BqlInt.Field<referenceID> { }

		[PXDBInt]
			[PXUIField(DisplayName = "Business Account", Enabled = false)]
			[PXVendorCustomerSelector(typeof(CAEntryType.module))]
		public virtual int? ReferenceID
				{
			get;
			set;
			}
		#endregion
		#region DrCr
		public abstract class drCr : PX.Data.BQL.BqlString.Field<drCr> { }

			[PXDefault(GL.DrCr.Debit)]
			[PXDBString(1, IsFixed = true)]
		[CADrCr.List]
			[PXUIField(DisplayName = "Disb./Receipt", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string DrCr
				{
			get;
			set;
			}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		[PXDBString(60, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Entry Type Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string Descr
			{
			get;
			set;
		}
		#endregion
		#region UseToReclassifyPayments
		public abstract class useToReclassifyPayments : PX.Data.BQL.BqlBool.Field<useToReclassifyPayments> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use for Payments Reclassification", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual bool? UseToReclassifyPayments
			{
			get;
			set;
		}
		#endregion		
        #region Consolidate
		public abstract class consolidate : PX.Data.BQL.BqlBool.Field<consolidate> { }

		[PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Deduct from Payment")]
		public bool? Consolidate
            {
			get;
			set;
        }
        #endregion
        #region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp]
		public virtual byte[] tstamp
			{
			get;
			set;
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID
			{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID
			{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
	}
}
