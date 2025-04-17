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
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN;
using System;

namespace PX.Objects.PM
{
	/// <summary>An account group that is used to track the budget, expenses, and revenues of projects.</summary>
	[PXPrimaryGraph(typeof(AccountGroupMaint))]
	[Serializable]
	[PXCacheName(Messages.PMAccountGroupName)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public partial class PMAccountGroup : IBqlTable, PX.SM.IIncludable
	{
		#region Keys

		/// <summary>
		/// Primary Key
		/// </summary>
		/// <exclude />
		public class PK : PrimaryKeyOf<PMAccountGroup>.By<PMAccountGroup.groupID>
		{
			public static PMAccountGroup Find(PXGraph graph, int? accountGroupID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, accountGroupID, options);
		}

		/// <summary>
		/// Unique Key
		/// </summary>
		public class UK : PrimaryKeyOf<PMAccountGroup>.By<groupCD>
		{
			public static PMAccountGroup Find(PXGraph graph, string accountGroupCD, PKFindOptions options = PKFindOptions.None) => FindBy(graph, accountGroupCD, options);
		}

		#endregion

		#region GroupID
		public abstract class groupID : PX.Data.BQL.BqlInt.Field<groupID> { }
		protected Int32? _GroupID;

		/// <summary>
		/// Gets or Sets the AccountGroup identifier.
		/// </summary>
		[PXDBIdentity()]
		[PXReferentialIntegrityCheck]
		[PXSelector(typeof(PMAccountGroup.groupID))]
		public virtual Int32? GroupID
		{
			get
			{
				return this._GroupID;
			}
			set
			{
				this._GroupID = value;
			}
		}
		#endregion
		#region GroupCD
		public abstract class groupCD : PX.Data.BQL.BqlString.Field<groupCD> { }
		protected String _GroupCD;
		/// <summary>
		/// Gets or Sets the AccountGroup identifier.
		/// This is a segmented key and format is configured under segmented key maintenance screen in CS module.
		/// </summary>
		[PXDimensionSelector(AccountGroupAttribute.DimensionName,
			typeof(Search<PMAccountGroup.groupCD>),
			typeof(PMAccountGroup.groupCD),
			typeof(PMAccountGroup.groupCD), typeof(PMAccountGroup.description), typeof(PMAccountGroup.type), typeof(PMAccountGroup.isActive), DescriptionField = typeof(PMTask.description))]
		[PXDBString(IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
		[PXUIField(DisplayName = "Account Group ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual String GroupCD
		{
			get
			{
				return this._GroupCD;
			}
			set
			{
				this._GroupCD = value;
			}
		}
		#endregion

		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		/// <summary>
		/// Gets or Sets the AccountGroup description.
		/// </summary>
		[PXDBLocalizableString(250, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual String Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				this._Description = value;
			}
		}
		#endregion
		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		protected Boolean? _IsActive;
		/// <summary>
		/// Gets or sets whether Account group is active or not.
		/// </summary>
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual Boolean? IsActive
		{
			get
			{
				return this._IsActive;
			}
			set
			{
				this._IsActive = value;
			}
		}
		#endregion
		#region IsExpense
		public abstract class isExpense : PX.Data.BQL.BqlBool.Field<isExpense> { }
		protected Boolean? _IsExpense;

		/// <summary>
		/// A Boolean value that indicates whether the account group is an expense account group
		/// and can be selected on the Cost Budget tab of the Projects (PM301000) form.
		/// </summary>
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Expense")]
		public virtual Boolean? IsExpense
		{
			get
			{
				return this._IsExpense;
			}
			set
			{
				this._IsExpense = value;
			}
		}
		#endregion
		#region RevenueAccountGroupID
		public abstract class revenueAccountGroupID : PX.Data.BQL.BqlInt.Field<revenueAccountGroupID> { }

		/// <summary>
		/// The default revenue account group of the expense account group.
		/// </summary>
		[PXRestrictor(typeof(Where<PMAccountGroup.isActive, Equal<True>>), Messages.InactiveAccountGroup, typeof(PMAccountGroup.groupCD))]
		[PXSelector(typeof(Search<PMAccountGroup.groupID, Where<PMAccountGroup.type, Equal<GL.AccountType.income>>>), SubstituteKey = typeof(PMAccountGroup.groupCD))]
		[PXUIField(DisplayName = "Default Revenue Account Group")]
		[PXDBInt]
		public virtual Int32? RevenueAccountGroupID
		{
			get;
			set;
		}
		#endregion
		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		protected string _Type;
		/// <summary>
		/// The type of the account group, which can be one of the following: Asset, Liability, Expense, Income, and Off-Balance.
		/// </summary>
		[PXDBString(1)]
		[PXDefault(GL.AccountType.Expense)]
		[PMAccountType.List()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string Type
		{
			get
			{
				return this._Type;
			}
			set
			{
				this._Type = value;
			}
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;

		/// <summary>
		/// The identifier of the default <see cref="GL.Account">account</see> for the account group.
		/// </summary>
		[PXDBInt]
		public virtual Int32? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
		#endregion
		#region SortOrder
		public abstract class sortOrder : PX.Data.BQL.BqlShort.Field<sortOrder> { }
		protected Int16? _SortOrder;
		/// <summary>
		/// Gets or sets sort order. Sort order is used in displaying the Balances for the Project.
		/// </summary>
		[PXDBShort()]
		[PXUIField(DisplayName = "Sort Order")]
		public virtual Int16? SortOrder
		{
			get
			{
				return this._SortOrder;
			}
			set
			{
				this._SortOrder = value;
			}
		}
		#endregion
		#region DefaultLineMarkupPct
		public abstract class defaultLineMarkupPct : PX.Data.BQL.BqlDecimal.Field<defaultLineMarkupPct> { }

		/// <summary>
		/// The default percentage of the line markup for change requests.
		/// </summary>
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Default Line Markup, %", FieldClass = nameof(FeaturesSet.ChangeRequest))]
		public virtual Decimal? DefaultLineMarkupPct
		{
			get;
			set;
		}
		#endregion

		#region CreatesCommitment
		public abstract class createsCommitment : PX.Data.BQL.BqlBool.Field<createsCommitment> { }

		/// <summary>
		/// A Boolean value that indicates whether the system automatically selects the Create Commitment
		/// check box for a change request line on the Estimation tab of the Change Requests (PM308500) form if the
		/// change request line has this account group selected in the Account Group column.
		/// </summary>
		[PXDBBool()]
		[PXUIField(DisplayName = "Create Commitment", FieldClass = nameof(FeaturesSet.ChangeRequest))]
		[PXDefault(false)]
		public virtual Boolean? CreatesCommitment
		{
			get;
			set;
		}
		#endregion

		#region GroupMask
		public abstract class groupMask : PX.Data.BQL.BqlByteArray.Field<groupMask> { }
		[PXDBGroupMask]
		public virtual Byte[] GroupMask
		{
			get;
			set;
		}
		#endregion

		#region Included
		public abstract class included : PX.Data.BQL.BqlBool.Field<included> { }

		/// <summary>
		/// An unbound field that is used in the user interface to include the account group into a <see cref="PX.SM.RelationGroup">restriction group</see>.
		/// </summary>
		[PXBool]
		[PXUIField(DisplayName = "Included")]
		[PXUnboundDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? Included
		{
			get;
			set;
		}
		#endregion

		#region Attributes
		/// <summary>
		/// Gets or sets entity attributes.
		/// </summary>
		[CRAttributesField(typeof(PMAccountGroup.classID))]
		public virtual string[] Attributes { get; set; }

		public abstract class classID : PX.Data.BQL.BqlString.Field<classID> { }
		/// <summary>
		/// Gets ClassID for the attributes. Always returns <see cref="GroupTypes.AccountGroup"/>
		/// </summary>
		[PXString(20)]
		public virtual string ClassID
		{
			get { return GroupTypes.AccountGroup; }
		}

		#endregion


		#region System Columns
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID]
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
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		[PXDBCreatedDateTime]
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
		[PXDBLastModifiedByID]
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
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		[PXDBLastModifiedDateTime]
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
		#endregion
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public static class PMAccountType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(GL.AccountType.Asset, GL.Messages.Asset),
					Pair(GL.AccountType.Liability, GL.Messages.Liability),
					Pair(GL.AccountType.Income, GL.Messages.Income),
					Pair(GL.AccountType.Expense, GL.Messages.Expense),
					Pair(OffBalance, Messages.OffBalance),
				}) {}
		}

		public class FilterListAttribute : PXStringListAttribute
		{
			public FilterListAttribute() : base(
				new[]
				{
					Pair(All, Messages.All),
					Pair(GL.AccountType.Asset, GL.Messages.Asset),
					Pair(GL.AccountType.Liability, GL.Messages.Liability),
					Pair(GL.AccountType.Income, GL.Messages.Income),
					Pair(GL.AccountType.Expense, GL.Messages.Expense),
					Pair(OffBalance, Messages.OffBalance),
				})
			{ }
		}

		public const string OffBalance = "O"; 
		public class offBalance : PX.Data.BQL.BqlString.Constant<offBalance>
		{
			public offBalance() : base(OffBalance) { ;}
		}

		public const string All = "X";
		public class all : Constant<string>
		{
			public all() : base(All) {; }
		}

	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public static class AccountGroupStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(new[] {Pair(Active, Messages.Active)}) {}
		}
		public const string Active = "A";
	}
}
