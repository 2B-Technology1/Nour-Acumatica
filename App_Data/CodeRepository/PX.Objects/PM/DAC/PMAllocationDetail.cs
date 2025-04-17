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
using PX.Objects.GL;
using System;
using static PX.Data.BQL.BqlPlaceholder;

namespace PX.Objects.PM
{
	/// <summary>
	/// Represents a step of the <see cref="PMAllocation">allocation rule</see>
	/// that defines the calculation rules and allocation settings.
	/// The records of this type are created and edited through the Allocation Rules (PM207500) form
	/// (which corresponds to the <see cref="AllocationMaint"/> graph).
	/// </summary>
	[System.SerializableAttribute()]
	[PXCacheName(Messages.PMAllocationDetail)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public partial class PMAllocationDetail : PX.Data.IBqlTable
	{
		#region AllocationID
		/// <inheritdoc cref="AllocationID"/>
		public abstract class allocationID : PX.Data.BQL.BqlString.Field<allocationID> { }
		protected String _AllocationID;
		/// <summary>
		/// The identifier of the <see cref="PMAllocation">allocation</see> to which this allocation step belongs.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMAllocation.allocationID" /> field.
		/// </value>
		[PXDBString(PMAllocation.allocationID.Length, IsKey = true, IsUnicode = true)]
		[PXDefault(typeof(PMAllocation.allocationID))]
		[PXParent(typeof(Select<PMAllocation, Where<PMAllocation.allocationID, Equal<Current<PMAllocationDetail.allocationID>>>>))]
		public virtual String AllocationID
		{
			get
			{
				return this._AllocationID;
			}
			set
			{
				this._AllocationID = value;
			}
		}
		#endregion
		#region StepID
		/// <inheritdoc cref="StepID"/>
		public abstract class stepID : PX.Data.BQL.BqlInt.Field<stepID> { }
		protected Int32? _StepID;
		/// <summary>
		/// The unique identifier of the allocation rule.
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Step ID")]
		public virtual Int32? StepID
		{
			get
			{
				return this._StepID;
			}
			set
			{
				this._StepID = value;
			}
		}
		#endregion
		#region Description
		/// <inheritdoc cref="Description"/>
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		/// <summary>
		/// The description of the step.
		/// </summary>
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
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
		#region SelectOption
		/// <inheritdoc cref="SelectOption"/>
		public abstract class selectOption : PX.Data.BQL.BqlString.Field<selectOption> { }
		protected String _SelectOption;
		/// <summary>
		/// The way the system should select the transactions for allocation.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="PMSelectOption.ListAttribute"/>.
		/// </value>
		[PMSelectOption.List]
		[PXDefault(PMSelectOption.Transaction)]
		[PXDBString(1)]
		[PXUIField(DisplayName = "Select Transactions")]
		public virtual String SelectOption
		{
			get
			{
				return this._SelectOption;
			}
			set
			{
				this._SelectOption = value;
			}
		}
		#endregion
		#region Post
		/// <inheritdoc cref="Post"/>
		public abstract class post : PX.Data.BQL.BqlBool.Field<post> { }
		protected Boolean? _Post;
		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the system creates the allocation transactions.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Create Allocation Transaction")]
		public virtual Boolean? Post
		{
			get
			{
				return this._Post;
			}
			set
			{
				this._Post = value;
			}
		}
		#endregion
		#region QtyFormula
		/// <inheritdoc cref="QtyFormula"/>
		public abstract class qtyFormula : PX.Data.BQL.BqlString.Field<qtyFormula> { }
		protected String _QtyFormula;
		/// <summary>
		/// The formula to be used for calculating the quantity for allocation transactions.
		/// </summary>
		[PXDBString(4000, IsUnicode = true)]
		[PXUIField(DisplayName = "Quantity Formula")]
		public virtual String QtyFormula
		{
			get
			{
				return this._QtyFormula;
			}
			set
			{
				this._QtyFormula = value;
			}
		}
		#endregion
		#region BillableQtyFormula
		/// <inheritdoc cref="BillableQtyFormula"/>
		public abstract class billableQtyFormula : PX.Data.BQL.BqlString.Field<billableQtyFormula> { }
		protected String _BillableQtyFormula;
		/// <summary>
		/// The formula for calculating the billable quantity for allocation transactions.
		/// </summary>
		[PXDBString(4000, IsUnicode = true)]
		[PXUIField(DisplayName = "Billable Qty. Formula")]
		public virtual String BillableQtyFormula
		{
			get
			{
				return this._BillableQtyFormula;
			}
			set
			{
				this._BillableQtyFormula = value;
			}
		}
		#endregion
		#region AmountFormula
		/// <inheritdoc cref="AmountFormula"/>
		public abstract class amountFormula : PX.Data.BQL.BqlString.Field<amountFormula> { }
		protected String _AmountFormula;
		/// <summary>
		/// The formula for calculating the amount of allocation transactions.
		/// </summary>
		[PXDBString(4000, IsUnicode = true)]
		[PXUIField(DisplayName = "Amount Formula")]
		public virtual String AmountFormula
		{
			get
			{
				return this._AmountFormula;
			}
			set
			{
				this._AmountFormula = value;
			}
		}
		#endregion
		#region DescriptionFormula
		/// <inheritdoc cref="DescriptionFormula"/>
		public abstract class descriptionFormula : PX.Data.BQL.BqlString.Field<descriptionFormula> { }
		protected String _DescriptionFormula;
		/// <summary>
		/// The formula to be used to generate the descriptions for allocation transactions.
		/// </summary>
		[PXDBString(4000, IsUnicode = true)]
		[PXUIField(DisplayName = "Description Formula")]
		public virtual String DescriptionFormula
		{
			get
			{
				return this._DescriptionFormula;
			}
			set
			{
				this._DescriptionFormula = value;
			}
		}
		#endregion
		#region RangeStart
		/// <inheritdoc cref="RangeStart"/>
		public abstract class rangeStart : PX.Data.BQL.BqlInt.Field<rangeStart> { }
		protected Int32? _RangeStart;
		/// <summary>
		/// The first step of the range.
		/// </summary>
		[PXDBInt()]
		[PXUIField(DisplayName = "Range Start")]
		public virtual Int32? RangeStart
		{
			get
			{
				return this._RangeStart;
			}
			set
			{
				this._RangeStart = value;
			}
		}
		#endregion
		#region RangeEnd
		/// <inheritdoc cref="RangeEnd"/>
		public abstract class rangeEnd : PX.Data.BQL.BqlInt.Field<rangeEnd> { }
		protected Int32? _RangeEnd;
		/// <summary>
		/// The last step of the range.
		/// </summary>
		[PXDBInt()]
		[PXUIField(DisplayName = "Range End")]
		public virtual Int32? RangeEnd
		{
			get
			{
				return this._RangeEnd;
			}
			set
			{
				this._RangeEnd = value;
			}
		}
		#endregion
		#region RateTypeID
		/// <inheritdoc cref="RateTypeID"/>
		public abstract class rateTypeID : PX.Data.BQL.BqlString.Field<rateTypeID> { }
		protected String _RateTypeID;
		/// <summary>
		/// The rate type used in the allocation rule step.
		/// </summary>
		[PXDBString(PMRateType.rateTypeID.Length, IsUnicode = true)]
		[PXSelector(typeof(PMRateType.rateTypeID), DescriptionField = typeof(PMRateType.description))]
		[PXUIField(DisplayName="Rate Type")]
        public virtual String RateTypeID
		{
			get
			{
                return this._RateTypeID;
			}
			set
			{
                this._RateTypeID = value;
			}
		}
		#endregion
		#region AccountGroupFrom
		/// <inheritdoc cref="AccountGroupFrom"/>
		public abstract class accountGroupFrom : PX.Data.BQL.BqlInt.Field<accountGroupFrom> { }
		protected Int32? _AccountGroupFrom;
		/// <summary>
		/// The account group that starts the range of account groups whose transactions are involved in this allocation step.
		/// </summary>
		[AccountGroup(DisplayName="Account Group From")]
		public virtual Int32? AccountGroupFrom
		{
			get
			{
				return this._AccountGroupFrom;
			}
			set
			{
				this._AccountGroupFrom = value;
			}
		}
		#endregion
		#region AccountGroupTo
		/// <inheritdoc cref="AccountGroupTo"/>
		public abstract class accountGroupTo : PX.Data.BQL.BqlInt.Field<accountGroupTo> { }
		protected Int32? _AccountGroupTo;
		/// <summary>
		/// The account group that ends the range of account groups whose transactions are involved in this allocation step.
		/// </summary>
		[AccountGroup(DisplayName = "Account Group To")]
		public virtual Int32? AccountGroupTo
		{
			get
			{
				return this._AccountGroupTo;
			}
			set
			{
				this._AccountGroupTo = value;
			}
		}
		#endregion
		#region Method
		/// <inheritdoc cref="Method"/>
		public abstract class method : PX.Data.BQL.BqlString.Field<method> { }
        protected String _Method;
		/// <summary>
		/// The method of the allocation.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="PMMethod.ListAttribute"/>.
		/// </value>
		[PMMethod.List]
        [PXDefault(PMMethod.Transaction)]
        [PXDBString(1)]
        [PXUIField(DisplayName = "Allocation Method")]
        public virtual String Method
        {
            get
            {
                return this._Method;
            }
            set
            {
                this._Method = value;
            }
        }
		#endregion
		#region UpdateGL
		/// <inheritdoc cref="UpdateGL"/>
		public abstract class updateGL : PX.Data.BQL.BqlBool.Field<updateGL> { }
		protected Boolean? _UpdateGL;
		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the allocation transactions should be posted to the general ledger.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Post Transaction to GL")]
		public virtual Boolean? UpdateGL
		{
			get
			{
				return this._UpdateGL;
			}
			set
			{
				this._UpdateGL = value;
			}
		}
		#endregion

		#region SourceBranchID
		/// <inheritdoc cref="SourceBranchID"/>
		public abstract class sourceBranchID : PX.Data.BQL.BqlInt.Field<sourceBranchID> { }
		protected Int32? _SourceBranchID;
		/// <summary>
		/// The <see cref="Branch">branch</see> of project transactions to be allocated.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Branch.branchID" /> field.
		/// </value>
		[Branch(IsDetail = true, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? SourceBranchID
		{
			get
			{
				return this._SourceBranchID;
			}
			set
			{
				this._SourceBranchID = value;
			}
		}
		#endregion
		#region OffsetBranchOrigin
		/// <inheritdoc cref="OffsetBranchOrigin"/>
		public abstract class offsetBranchOrigin : PX.Data.BQL.BqlString.Field<offsetBranchOrigin> { }
		protected String _OffsetBranchOrigin;
		/// <summary>
		/// The source of the <see cref="Branch">branch</see> associated with the project allocation transactions.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="PMOrigin.ListAttribute"/>.
		/// </value>
		[PMOrigin.List]
		[PXDefault(PMOrigin.Source)]
		[PXDBString(1)]
		[PXUIField(DisplayName = "Branch", FieldClass = "COMPANYBRANCH")]
		public virtual String OffsetBranchOrigin
		{
			get
			{
				return this._OffsetBranchOrigin;
			}
			set
			{
				this._OffsetBranchOrigin = value;
			}
		}
		#endregion
		#region TargetBranchID
		/// <inheritdoc cref="TargetBranchID"/>
		public abstract class targetBranchID : PX.Data.BQL.BqlInt.Field<targetBranchID> { }
		protected Int32? _TargetBranchID;
		/// <summary>
		/// The identifier of the <see cref="Branch">branch</see> to be used in project allocation transactions.
		/// </summary>
		[Branch(DisplayName = "Target Branch ID", IsDetail = true, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? TargetBranchID
		{
			get
			{
				return this._TargetBranchID;
			}
			set
			{
				this._TargetBranchID = value;
			}
		}
		#endregion
		#region ProjectOrigin
		/// <inheritdoc cref="ProjectOrigin"/>
		public abstract class projectOrigin : PX.Data.BQL.BqlString.Field<projectOrigin> { }
		protected String _ProjectOrigin;
		/// <summary>
		/// The source of the <see cref="PMProject">project</see> associated with the allocation's debit transactions.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="PMOrigin.ListAttribute"/>.
		/// </value>
		[PMOrigin.List]
		[PXDefault(PMOrigin.Source)]
		[PXDBString(1)]
		[PXUIField(DisplayName = "Project")]
		public virtual String ProjectOrigin
		{
			get
			{
				return this._ProjectOrigin;
			}
			set
			{
				this._ProjectOrigin = value;
			}
		}
		#endregion
		#region ProjectID
		/// <inheritdoc cref="ProjectID"/>
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;
		/// <summary>
		/// The identifier of the <see cref="PMProject">project</see> associated with the allocation's debit transactions.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMProject.contractID" /> field.
		/// </value>
		[Project]
		public virtual Int32? ProjectID
		{
			get
			{
				return this._ProjectID;
			}
			set
			{
				this._ProjectID = value;
			}
		}
		#endregion
		#region TaskOrigin
		/// <inheritdoc cref="TaskOrigin"/>
		public abstract class taskOrigin : PX.Data.BQL.BqlString.Field<taskOrigin> { }
		protected String _TaskOrigin;
		/// <summary>
		/// The source of the <see cref="PMTask">task</see> associated with the allocation's debit transactions.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="PMOrigin.ListAttribute"/>.
		/// </value>
		[PMOrigin.List]
		[PXDefault(PMOrigin.Source)]
		[PXDBString(1)]
		[PXUIField(DisplayName = "Task")]
		public virtual String TaskOrigin
		{
			get
			{
				return this._TaskOrigin;
			}
			set
			{
				this._TaskOrigin = value;
			}
		}
		#endregion
		#region TaskID
		/// <inheritdoc cref="TaskID"/>
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		protected Int32? _TaskID;
		/// <summary>
		/// The identifier of the <see cref="PMTask">task</see> associated with the allocation's debit transactions.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMTask.taskID" /> field.
		/// </value>
		[ProjectTask(typeof(PMAllocationDetail.projectID), AllowNull=true )]
		public virtual Int32? TaskID
		{
			get
			{
				return this._TaskID;
			}
			set
			{
				this._TaskID = value;
			}
		}
		#endregion
		#region TaskCD
		/// <inheritdoc cref="TaskCD"/>
		public abstract class taskCD : PX.Data.BQL.BqlString.Field<taskCD> { }
		protected String _TaskCD;
		/// <summary>
		/// The <see cref="TaskID">project task identifier</see> displayed on the form.
		/// </summary>
		[PXDimension(ProjectTaskAttribute.DimensionName)]
		[PXDBString(IsUnicode = true, InputMask = "")]
		[PXUIField(Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String TaskCD
		{
			get
			{
				return this._TaskCD;
			}
			set
			{
				this._TaskCD = value;
			}
		}
		#endregion
		#region AccountGroupOrigin
		/// <inheritdoc cref="AccountGroupOrigin"/>
		public abstract class accountGroupOrigin : PX.Data.BQL.BqlString.Field<accountGroupOrigin> { }
		protected String _AccountGroupOrigin;
		/// <summary>
		/// The source of <see cref="PMAccountGroup">account group</see> associated with the allocation's debit transactions.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="PMOrigin.ListAttribute"/>.
		/// </value>
		[PXDefault(PMOrigin.Source)]
		[PXDBString(1)]
		[PXUIField(DisplayName = "Account Group")]
		public virtual String AccountGroupOrigin
		{
			get
			{
				return this._AccountGroupOrigin;
			}
			set
			{
				this._AccountGroupOrigin = value;
			}
		}
		#endregion
		#region AccountGroupID
		/// <inheritdoc cref="AccountGroupID"/>
		public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
		protected Int32? _AccountGroupID;
		/// <summary>
		/// The identifier of the <see cref="PMAccountGroup">account group</see> associated with the allocation's debit transactions.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMAccountGroup.accountID" /> field.
		/// </value>
		[AccountGroup(typeof(Where<Current<PMAllocationDetail.updateGL>, Equal<True>,
		And<PMAccountGroup.type, NotEqual<PMAccountType.offBalance>,
		Or<Current<PMAllocationDetail.updateGL>, Equal<False>>>>), DisplayName = "Account Group")]
		public virtual Int32? AccountGroupID
		{
			get
			{
				return this._AccountGroupID;
			}
			set
			{
				this._AccountGroupID = value;
			}
		}
		#endregion
		#region AccountOrigin
		/// <inheritdoc cref="AccountOrigin"/>
		public abstract class accountOrigin : PX.Data.BQL.BqlString.Field<accountOrigin> { }
		protected String _AccountOrigin;
		/// <summary>
		/// The account of the allocation.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="PMOrigin.DebitAccountListAttribute"/>.
		/// </value>
		[PMOrigin.DebitAccountListAttribute]
		[PXDefault(PMOrigin.Source)]
		[PXDBString(1)]
		[PXUIField(DisplayName = "Account Origin")]
		public virtual String AccountOrigin
		{
			get
			{
				return this._AccountOrigin;
			}
			set
			{
				this._AccountOrigin = value;
			}
		}
		#endregion
		#region AccountID
		/// <inheritdoc cref="AccountID"/>
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;
		/// <summary>
		/// The identifier of the <see cref="Account">account</see> associated with the allocation's debit transactions.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Account.accountID" /> field.
		/// </value>
		[Account(null, typeof(Search<Account.accountID, Where<Account.accountGroupID, IsNotNull>>), AvoidControlAccounts = true)]
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
		#region SubMask
		/// <inheritdoc cref="SubMask"/>
		public abstract class subMask : PX.Data.BQL.BqlString.Field<subMask> { }
		protected String _SubMask;
		/// <summary>
		/// The subaccount associated with the allocation's credit transactions.		
		/// </summary>
		[PMSubAccountMask]
		public virtual String SubMask
		{
			get
			{
				return this._SubMask;
			}
			set
			{
				this._SubMask = value;
			}
		}
		#endregion
		#region SubID
		/// <inheritdoc cref="SubID"/>
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		protected Int32? _SubID;
		/// <summary>
		/// The identifier of the <see cref="Sub">subaccount</see> associated with the allocation's debit transactions.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Sub.subID" /> field.
		/// </value>
		[SubAccount(typeof(PMAllocationDetail.accountID))]
		public virtual Int32? SubID
		{
			get
			{
				return this._SubID;
			}
			set
			{
				this._SubID = value;
			}
		}
		#endregion

		#region OffsetProjectOrigin
		/// <inheritdoc cref="OffsetProjectOrigin"/>
		public abstract class offsetProjectOrigin : PX.Data.BQL.BqlString.Field<offsetProjectOrigin> { }
		protected String _OffsetProjectOrigin;
		/// <summary>
		/// The source of <see cref="PMProject">project</see> associated with the allocation's credit transactions.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="PMOrigin.ListAttribute"/>.
		/// </value>
		[PMOrigin.List]
		[PXDefault(PMOrigin.Source)]
		[PXDBString(1)]
		[PXUIField(DisplayName = "Project")]
		public virtual String OffsetProjectOrigin
		{
			get
			{
				return this._OffsetProjectOrigin;
			}
			set
			{
				this._OffsetProjectOrigin = value;
			}
		}
		#endregion
		#region OffsetProjectID
		/// <inheritdoc cref="OffsetProjectID"/>
		public abstract class offsetProjectID : PX.Data.BQL.BqlInt.Field<offsetProjectID> { }
		protected Int32? _OffsetProjectID;
		/// <summary>		
		/// The identifier of the <see cref="PMProject">project</see> associated with the allocation's credit transactions.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMProject.contractID" /> field.
		/// </value>
		[Project(DisplayName="Project")]
		public virtual Int32? OffsetProjectID
		{
			get
			{
				return this._OffsetProjectID;
			}
			set
			{
				this._OffsetProjectID = value;
			}
		}
		#endregion
		#region OffsetTaskOrigin
		/// <inheritdoc cref="OffsetTaskOrigin"/>
		public abstract class offsetTaskOrigin : PX.Data.BQL.BqlString.Field<offsetTaskOrigin> { }
		protected String _OffsetTaskOrigin;
		/// <summary>.
		/// The source of the <see cref="PMTask">task</see> associated with the allocation's credit transactions.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="PMOrigin.ListAttribute"/>.
		/// </value>
		[PMOrigin.List]
		[PXDefault(PMOrigin.Source)]
		[PXDBString(1)]
		[PXUIField(DisplayName = "Task")]
		public virtual String OffsetTaskOrigin
		{
			get
			{
				return this._OffsetTaskOrigin;
			}
			set
			{
				this._OffsetTaskOrigin = value;
			}
		}
		#endregion
		#region OffsetTaskID
		/// <inheritdoc cref="OffsetTaskID"/>
		public abstract class offsetTaskID : PX.Data.BQL.BqlInt.Field<offsetTaskID> { }
		protected Int32? _OffsetTaskID;
		/// <summary>
		/// The identifier of the <see cref="OffsetTaskOrigin">origin of the offset task</see> associated with the record.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMTask.taskID" /> field.
		/// </value>
		[ProjectTask(typeof(PMAllocationDetail.offsetProjectID), AllowNull=true, DisplayName="Project Task")]
		public virtual Int32? OffsetTaskID
		{
			get
			{
				return this._OffsetTaskID;
			}
			set
			{
				this._OffsetTaskID = value;
			}
		}
		#endregion
		#region OffsetTaskCD
		/// <inheritdoc cref="OffsetTaskCD"/>
		public abstract class offsetTaskCD : PX.Data.BQL.BqlString.Field<offsetTaskCD> { }
		protected String _OffsetTaskCD;
		/// <summary>
		/// The identifier of the <see cref="PMTask">task</see> associated with the allocation's credit transactions.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMTask.taskID" /> field.
		/// </value>
		[PXDimension(ProjectTaskAttribute.DimensionName)]
		[PXDBString(IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Project Task", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String OffsetTaskCD
		{
			get
			{
				return this._OffsetTaskCD;
			}
			set
			{
				this._OffsetTaskCD = value;
			}
		}
		#endregion
		#region OffsetAccountGroupOrigin
		/// <inheritdoc cref="OffsetAccountGroupOrigin"/>
		public abstract class offsetAccountGroupOrigin : PX.Data.BQL.BqlString.Field<offsetAccountGroupOrigin> { }
		protected String _OffsetAccountGroupOrigin;
		/// <summary>
		/// The source of the <see cref="PMAccountGroup">account group</see> associated with the allocation's credit transactions.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="PMOrigin.ListAttribute"/>.
		/// </value> 
		[PXDefault(PMOrigin.Source)]
		[PXDBString(1)]
		[PXUIField(DisplayName = "Account Group")]
		public virtual String OffsetAccountGroupOrigin
		{
			get
			{
				return this._OffsetAccountGroupOrigin;
			}
			set
			{
				this._OffsetAccountGroupOrigin = value;
			}
		}
		#endregion
		#region OffsetAccountGroupID
		/// <inheritdoc cref="OffsetAccountGroupID"/>
		public abstract class offsetAccountGroupID : PX.Data.BQL.BqlInt.Field<offsetAccountGroupID> { }
		protected Int32? _OffsetAccountGroupID;
		/// <summary>
		/// The identifier of the <see cref="PMAccountGroup">account group</see> associated with the allocation's credit transactions.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMAccountGroup.accountID" /> field.
		/// </value>
		[AccountGroup(typeof(Where<Current<PMAllocationDetail.updateGL>, Equal<True>,
		And<PMAccountGroup.type, NotEqual<PMAccountType.offBalance>,
		Or<Current<PMAllocationDetail.updateGL>, Equal<False>>>>), DisplayName = "Account Group")]
		public virtual Int32? OffsetAccountGroupID
		{
			get
			{
				return this._OffsetAccountGroupID;
			}
			set
			{
				this._OffsetAccountGroupID = value;
			}
		}
		#endregion
		#region OffsetAccountOrigin
		/// <inheritdoc cref="OffsetAccountOrigin"/>
		public abstract class offsetAccountOrigin : PX.Data.BQL.BqlString.Field<offsetAccountOrigin> { }
		protected String _OffsetAccountOrigin;
		/// <summary>
		/// The account for the allocation's debit transactions.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="PMOrigin.CreditAccountListAttribute"/>.
		/// </value>
		[PMOrigin.CreditAccountListAttribute]
		[PXDefault(PMOrigin.Source)]
		[PXDBString(1)]
		[PXUIField(DisplayName = "Account Origin")]
		public virtual String OffsetAccountOrigin
		{
			get
			{
				return this._OffsetAccountOrigin;
			}
			set
			{
				this._OffsetAccountOrigin = value;
			}
		}
		#endregion
		#region OffsetAccountID
		/// <inheritdoc cref="OffsetAccountID"/>
		public abstract class offsetAccountID : PX.Data.BQL.BqlInt.Field<offsetAccountID> { }
		protected Int32? _OffsetAccountID;
		/// <summary>
		/// The identifier of the <see cref="Account">account</see> associated with the allocation's credit transactions.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Account.accountID" /> field.
		/// </value>
		[Account(null, typeof(Search<Account.accountID, Where<Account.accountGroupID, IsNotNull>>), DisplayName = "Account", AvoidControlAccounts = true)]
		public virtual Int32? OffsetAccountID
		{
			get
			{
				return this._OffsetAccountID;
			}
			set
			{
				this._OffsetAccountID = value;
			}
		}
		#endregion
		#region OffsetSubMask
		/// <inheritdoc cref="OffsetSubMask"/>
		public abstract class offsetSubMask : PX.Data.BQL.BqlString.Field<offsetSubMask> { }
		protected String _OffsetSubMask;
		/// <summary>
		/// The subaccount for the allocation's credit transactions.
		/// </summary>
		[PMOffsetSubAccountMask]
		public virtual String OffsetSubMask
		{
			get
			{
				return this._OffsetSubMask;
			}
			set
			{
				this._OffsetSubMask = value;
			}
		}
		#endregion
		#region OffsetSubID
		/// <inheritdoc cref="OffsetSubID"/>
		public abstract class offsetSubID : PX.Data.BQL.BqlInt.Field<offsetSubID> { }
		protected Int32? _OffsetSubID;
		/// <summary>
		/// The identifier of the <see cref="Sub">subaccount</see> associated with the allocation's credit transactions.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Sub.SubID" /> field.
		/// </value>
		[SubAccount(typeof(PMAllocationDetail.offsetAccountID), DisplayName="Subaccount")]
		public virtual Int32? OffsetSubID
		{
			get
			{
				return this._OffsetSubID;
			}
			set
			{
				this._OffsetSubID = value;
			}
		}
		#endregion

		#region Reverse
		/// <inheritdoc cref="Reverse"/>
		public abstract class reverse : PX.Data.BQL.BqlString.Field<reverse> { }
		protected String _Reverse;
		/// <summary>
		/// The reverse allocation for the allocation transaction.
		/// </summary>
		/// <value>
		/// The field can have one of the following values:
		/// <list>
		/// <item><description><c>I</c>: On AR Invoice Release</description></item>
		/// <item><description><c>B</c>: On AR Invoice Generation</description></item>
		/// <item><description><c>N</c>: Never</description></item>
		/// </list>
		/// </value>
		[PMReverse.List]
		[PXDefault(PMReverse.OnInvoiceRelease)]
		[PXDBString(1)]
		[PXUIField(DisplayName = "Reverse Allocation")]
		public virtual String Reverse
		{
			get
			{
				return this._Reverse;
			}
			set
			{
				this._Reverse = value;
			}
		}
		#endregion
		#region UseReversalDateFromOriginal
		/// <inheritdoc cref="UseReversalDateFromOriginal"/>
		public abstract class useReversalDateFromOriginal : PX.Data.BQL.BqlBool.Field<useReversalDateFromOriginal> { }
		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the system will specify the date and the financial period of the allocation transaction being reversed.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Reversal Date from Original Transaction")]
		public virtual Boolean? UseReversalDateFromOriginal
		{
			get;
			set;
		}
		#endregion
		#region NoRateOption
		/// <inheritdoc cref="NoRateOption"/>
		public abstract class noRateOption : PX.Data.BQL.BqlString.Field<noRateOption> { }
		protected String _NoRateOption;
		/// <summary>
		/// The action that is performed if the value for @Rate has not been defined.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="PMNoRateOption.AllocationListAttribute"/>.
		/// </value>
		[PMNoRateOption.AllocationList]
		[PXDefault(PMNoRateOption.SetOne)]
		[PXDBString(1)]
		[PXUIField(DisplayName = "If @Rate Is Not Defined")]
		public virtual String NoRateOption
		{
			get
			{
				return this._NoRateOption;
			}
			set
			{
				this._NoRateOption = value;
			}
		}
		#endregion
		#region DateSource
		/// <inheritdoc cref="DateSource"/>
		public abstract class dateSource : PX.Data.BQL.BqlString.Field<dateSource> { }
		protected String _DateSource;
		/// <summary>
		/// The date source, which defines how the date for the allocation transactions is defined.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="PMDateSource.ListAttribute"/>.
		/// </value>
		[PMDateSource.List]
		[PXDefault(PMDateSource.Transaction)]
		[PXDBString(1)]
		[PXUIField(DisplayName = "Date Source")]
		public virtual String DateSource
		{
			get
			{
				return this._DateSource;
			}
			set
			{
				this._DateSource = value;
			}
		}
		#endregion

		#region GroupByItem
		/// <inheritdoc cref="GroupByItem"/>
		public abstract class groupByItem : PX.Data.BQL.BqlBool.Field<groupByItem> { }
		protected Boolean? _GroupByItem;
		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the underlying transactions should be groupped by inventory items.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "By Item")]
		public virtual Boolean? GroupByItem
		{
			get
			{
				return this._GroupByItem;
			}
			set
			{
				this._GroupByItem = value;
			}
		}
		#endregion
		#region GroupByEmployee
		/// <inheritdoc cref="GroupByEmployee"/>
		public abstract class groupByEmployee : PX.Data.BQL.BqlBool.Field<groupByEmployee> { }
		protected Boolean? _GroupByEmployee;
		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the underlying transactions should be groupped by employees.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "By Employee")]
		public virtual Boolean? GroupByEmployee
		{
			get
			{
				return this._GroupByEmployee;
			}
			set
			{
				this._GroupByEmployee = value;
			}
		}
		#endregion
		#region GroupByDate
		/// <inheritdoc cref="GroupByDate"/>
		public abstract class groupByDate : PX.Data.BQL.BqlBool.Field<groupByDate> { }
		protected Boolean? _GroupByDate;
		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the underlying transactions should be groupped by dates.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "By Date")]
		public virtual Boolean? GroupByDate
		{
			get
			{
				return this._GroupByDate;
			}
			set
			{
				this._GroupByDate = value;
			}
		}
		#endregion
		#region GroupByVendor
		/// <inheritdoc cref="GroupByVendor"/>
		public abstract class groupByVendor : PX.Data.BQL.BqlBool.Field<groupByVendor> { }
		protected Boolean? _GroupByVendor;
		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the underlying transactions should be groupped by vendors or customers.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "By Vendor")]
		public virtual Boolean? GroupByVendor
		{
			get
			{
				return this._GroupByVendor;
			}
			set
			{
				this._GroupByVendor = value;
			}
		}
		#endregion
		#region FullDetail
		/// <inheritdoc cref="FullDetail"/>
		public abstract class fullDetail : PX.Data.BQL.BqlBool.Field<fullDetail> { }
		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the underlying transactions should be groupped by all of the following: inventory items, employees, dates, and vendors or customers.
		/// </summary>
		[PXBool()]
		public virtual Boolean? FullDetail
		{
			get
			{
				return GroupByItem != true && GroupByEmployee != true && GroupByDate != true && GroupByVendor != true;
			}
		}
		#endregion

		#region Allocation
		/// <inheritdoc cref="Allocation"/>
		public abstract class allocation : PX.Data.BQL.BqlInt.Field<allocation> { }
		protected int? _Allocation;
		/// <summary>
		/// The step ID of the allocation rule.
		/// </summary>
		[PXInt]
		[PXUIField(DisplayName = "Allocation")]
		public virtual int? Allocation
		{
			get
			{
				return StepID;
			}
			set
			{
			}
		}
		#endregion
		#region AllocationText
		/// <inheritdoc cref="AllocationText"/>
		public abstract class allocationText : PX.Data.BQL.BqlString.Field<allocationText> { }
		protected String _AllocationText;
		/// <summary>
		/// The allocation text of the allocation rule.
		/// </summary>
		[PXString(10)]
		public virtual String AllocationText
		{
			get
			{
				return this._AllocationText;
			}
			set
			{
				this._AllocationText = value;
			}
		}
		#endregion

		#region AllocateZeroAmount
		/// <inheritdoc cref="AllocateZeroAmount"/>
		public abstract class allocateZeroAmount : PX.Data.BQL.BqlBool.Field<allocateZeroAmount> { }
		protected Boolean? _AllocateZeroAmount;
		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the system will create the allocation transaction even if it has an amount of zero.
		/// </summary>
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Create Transaction with Zero Amount")]
		public virtual Boolean? AllocateZeroAmount
		{
			get
			{
				return this._AllocateZeroAmount;
			}
			set
			{
				this._AllocateZeroAmount = value;
			}
		}
		#endregion
		#region AllocateZeroQty
		/// <inheritdoc cref="AllocateZeroQty"/>
		public abstract class allocateZeroQty : PX.Data.BQL.BqlBool.Field<allocateZeroQty> { }
		protected Boolean? _AllocateZeroQty;
		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the system will create the allocation transaction even if it has a quantity of zero.
		/// </summary>
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Create Transaction with Zero Qty.")]
		public virtual Boolean? AllocateZeroQty
		{
			get
			{
				return this._AllocateZeroQty;
			}
			set
			{
				this._AllocateZeroQty = value;
			}
		}
		#endregion
		#region AllocateNonBillable
		/// <inheritdoc cref="AllocateNonBillable"/>
		public abstract class allocateNonBillable : PX.Data.BQL.BqlBool.Field<allocateNonBillable> { }
		protected Boolean? _AllocateNonBillable;
		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the system will create the allocation transaction even if it is non-billable.
		/// </summary>
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Allocate Non-Billable Transactions")]
		public virtual Boolean? AllocateNonBillable
		{
			get
			{
				return this._AllocateNonBillable;
			}
			set
			{
				this._AllocateNonBillable = value;
			}
		}
		#endregion

		#region MarkAsNotAllocated
		/// <inheritdoc cref="MarkAsNotAllocated"/>
		public abstract class markAsNotAllocated : PX.Data.BQL.BqlBool.Field<markAsNotAllocated> { }
		protected Boolean? _MarkAsNotAllocated;
		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the transactions allocated by this allocation rule could be used in subsequent allocations.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Can Be Used as a Source in Another Allocation")]
		public virtual Boolean? MarkAsNotAllocated
		{
			get
			{
				return this._MarkAsNotAllocated;
			}
			set
			{
				this._MarkAsNotAllocated = value;
			}
		}
		#endregion

		#region CopyNotes
		/// <inheritdoc cref="CopyNotes"/>
		public abstract class copyNotes : PX.Data.BQL.BqlBool.Field<copyNotes> { }
		protected Boolean? _CopyNotes;
		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the notes can be copied.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Copy Notes", Visibility=PXUIVisibility.Visible, Visible=false)]
		public virtual Boolean? CopyNotes
		{
			get
			{
				return this._CopyNotes;
			}
			set
			{
				this._CopyNotes = value;
			}
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
	public static class PMOrigin
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Source, Messages.Origin_Source),
					Pair(Change, Messages.Origin_Change),
				}) {}
		}

		public class DebitAccountListAttribute : PXStringListAttribute
		{
			public DebitAccountListAttribute() : base(
				new[]
				{
					Pair(Source, Messages.Origin_Source),
					Pair(Change, Messages.Origin_Change),
					Pair(OtherSource, Messages.Origin_CreditSource),
				}) {}
		}

		public class CreditAccountListAttribute : PXStringListAttribute
		{
			public CreditAccountListAttribute() : base(
				new[]
				{
					Pair(Source, Messages.Origin_Source),
					Pair(Change, Messages.Origin_Change),
					Pair(OtherSource, Messages.Origin_DebitSource),
				}) {}
		}

		/// <summary>
		/// List of available Account Group sources. 
		/// Account Group can be taken either from Source object, from Account or specified directly.
		/// </summary>
		public class AccountGroupListAttribute : PXStringListAttribute
		{
			public AccountGroupListAttribute() : base(
				new[]
				{
					Pair(Source, Messages.Origin_Source),
					Pair(Change, Messages.Origin_Change),
					Pair(FromAccount, Messages.Origin_FromAccount),
				}) {}
		}

        public class BranchFilterListAttribute : PXStringListAttribute
        {
            public BranchFilterListAttribute() : base(
                new[]
                {
                    Pair(Source, Messages.Origin_None),
                    Pair(Change, Messages.Origin_Branch),
                })
            { }
        }

        public const string Source = "S";
		public const string Change = "C";
		public const string FromAccount = "F";
		public const string None = "N";
		public const string OtherSource = "X";
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public static class PMMethod
    {
	    public class ListAttribute : PXStringListAttribute
	    {
		    public ListAttribute() : base(
			    new[]
				{
					Pair(Transaction, Messages.PMMethod_Transaction),
					Pair(Budget, Messages.PMMethod_Budget),
				}) {}
	    }

	    public const string Transaction = "T";
        public const string Budget = "B";

        public class transaction : PX.Data.BQL.BqlString.Constant<transaction>
		{
            public transaction() : base(Transaction) { ;}
        }

        public class budget : PX.Data.BQL.BqlString.Constant<budget>
		{
            public budget() : base(Budget) { ;}
        }

    }


	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public static class PMReverse
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(OnInvoiceRelease, Messages.PMReverse_OnARInvoiceRelease),
					Pair(OnInvoiceGeneration, Messages.PMReverse_OnARInvoiceGeneration),
					Pair(Never, Messages.PMReverse_Never),
				}) {}
		}

		public const string OnInvoiceRelease = "I";
		public const string OnInvoiceGeneration = "B";
		public const string Never = "N";
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public static class PMNoRateOption
	{
		public class AllocationListAttribute : PXStringListAttribute
		{
			public AllocationListAttribute():base(
				new[]
				{
					Pair(SetOne, Messages.PMNoRateOption_SetOne),
					Pair(SetZero, Messages.PMNoRateOption_SetZero),
					Pair(RaiseError, Messages.PMNoRateOption_RaiseError),
					Pair(DontAllocate, Messages.PMNoRateOption_NoAllocate),
				}){}
		}

		public class BillingListAttribute : PXStringListAttribute
		{
			public BillingListAttribute() : base(
				new[]
				{
					Pair(SetOne, Messages.PMNoRateOption_SetOne),
					Pair(SetZero, Messages.PMNoRateOption_SetZero),
					Pair(RaiseError, Messages.PMNoRateOption_RaiseError),
					Pair(DontAllocate, Messages.PMNoRateOption_NoBill),
				})
			{ }
		}


		public const string SetOne = "1";
		public const string SetZero = "0";
		public const string RaiseError = "E";
		public const string DontAllocate = "N";

	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public static class PMDateSource
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Transaction, Messages.PMDateSource_Transaction),
					Pair(Allocation, Messages.PMDateSource_Allocation),
				}) {}
		}

		public const string Transaction = "T";
		public const string Allocation = "A";
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public static class PMSelectOption
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Transaction, Messages.PMSelectOption_Transaction),
					Pair(Step, Messages.PMSelectOption_Step),
				}) {}
		}

		public const string Transaction = "T";
		public const string Step = "S";

		public class transaction : PX.Data.BQL.BqlString.Constant<transaction>
		{
			public transaction() : base(Transaction) { ;}
		}

		public class step : PX.Data.BQL.BqlString.Constant<step>
		{
			public step() : base(Step) { ;}
		}

	}
}
