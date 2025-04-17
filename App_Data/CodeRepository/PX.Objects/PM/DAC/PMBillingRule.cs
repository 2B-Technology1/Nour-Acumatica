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
using System;

namespace PX.Objects.PM
{
	/// <summary>
	/// A step of the <see cref="PMBilling">billing rule</see> that defines the calculation rules
	/// and other settings that depend on the step <see cref="Type">type</see>.
	/// The records of this type are created and edited through the Billing Rules (PM207000) form
	/// (which corresponds to the <see cref="BillingMaint"/> graph).
	/// </summary>
	[PXCacheName(Messages.BillingRuleStep)]
	[System.SerializableAttribute()]
	[PXPrimaryGraph(typeof(BillingMaint))]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public partial class PMBillingRule : PX.Data.IBqlTable
	{
		#region Keys

		/// <summary>
		/// Primary Key
		/// </summary>
		public class PK : PrimaryKeyOf<PMBillingRule>.By<billingID>
		{
			public static PMBillingRule Find(PXGraph graph, string billingID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, billingID, options);
		}

		#endregion

		#region BillingID
		/// <inheritdoc cref="BillingID"/>
		public abstract class billingID : PX.Data.BQL.BqlString.Field<billingID> { }
		protected String _BillingID;
		/// <summary>
		/// The identifier of the billing rule to which this billing rule step belongs.
		/// </summary>
		[PXDBString(PMBilling.billingID.Length, IsKey = true, IsUnicode = true)]
		[PXDefault(typeof(PMBilling.billingID))]
		[PXParent(typeof(Select<PMBilling, Where<PMBilling.billingID, Equal<Current<PMBillingRule.billingID>>>>))]
		public virtual String BillingID
		{
			get
			{
				return this._BillingID;
			}
			set
			{
				this._BillingID = value;
			}
		}
		#endregion
		#region StepID
		/// <inheritdoc cref="StepID"/>
		public abstract class stepID : PX.Data.BQL.BqlInt.Field<stepID> { }
		protected Int32? _StepID;
		/// <summary>
		/// The identifier of the billing rule step.
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
		/// <inheritdoc cref="StepID"/>
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		/// <summary>
		/// The description of the billing rule.
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
		#region Type
		/// <inheritdoc cref="Type"/>
		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		protected String _Type;
		/// <summary>
		/// The type of the billing rule step.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="PMBillingType.ListAttribute"/>.
		/// </value>
		[PMBillingType.List]
		[PXDefault(PMBillingType.Transaction)]
		[PXDBString(1)]
		[PXUIField(DisplayName = "Billing Type")]
		public virtual String Type
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
		#region AccountGroupID
		/// <inheritdoc cref="AccountGroupID"/>
		public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
		protected Int32? _AccountGroupID;
		/// <summary>
		/// The <see cref="PMAccountGroup">account group</see> whose transactions are involved in this billing step.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMAccountGroup.groupID"/> field.
		/// </value>
		[PXRestrictor(typeof(Where<PMAccountGroup.isActive, Equal<True>>), PM.Messages.InactiveAccountGroup, typeof(PMAccountGroup.groupCD))]
		[AccountGroup]
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
		#region InvoiceGroup
		/// <inheritdoc cref="InvoiceGroup"/>
		public abstract class invoiceGroup : PX.Data.BQL.BqlString.Field<invoiceGroup> { }
		protected String _InvoiceGroup;
		/// <summary>
		/// The identifier for grouping the invoices.
		/// </summary>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Invoice Group")]
		public virtual String InvoiceGroup
		{
			get
			{
				return this._InvoiceGroup;
			}
			set
			{
				this._InvoiceGroup = value;
			}
		}
		#endregion
		#region BranchSource
		/// <inheritdoc cref="BranchSource"/>
		public abstract class branchSource : PX.Data.BQL.BqlString.Field<branchSource> { }
		protected String _BranchSource;
		/// <summary>
		/// The source branch to be used for billing.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="PMBillingType.ListAttribute"/>.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PMAccountSource.List()]
		[PXDefault(PMAccountSource.None)]
		[PXUIField(DisplayName = "Use Destination Branch from", Required = true)]
		public virtual String BranchSource
		{
			get
			{
				return this._BranchSource;
			}
			set
			{
				this._BranchSource = value;
			}
		}
		#endregion
		#region BranchSourceBudget
		/// <inheritdoc cref="BranchSource"/>
		public abstract class branchSourceBudget : PX.Data.BQL.BqlString.Field<branchSourceBudget> { }
		protected String _BranchSourceBudget;
		/// <summary>
		/// The account to be used for billing as a sales account of the billing rule.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="PMAccountSource.ListAttributeBudget"/>.
		/// </value>
		[PXString(1, IsFixed = true)]
		[PMAccountSource.ListAttributeBudget()]
		[PXDefault(PMAccountSource.None, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Use Destination Branch from", Visible = false, Required = true)]
		public virtual String BranchSourceBudget
		{
			get
			{
				return this._BranchSource;
			}
			set
			{
				this._BranchSource = value;
			}
		}
		#endregion
		#region TargetBranchID
		/// <inheritdoc cref="TargetBranchID"/>
		public abstract class targetBranchID : PX.Data.BQL.BqlInt.Field<targetBranchID> { }
		/// <summary>
		/// The identifier of the <see cref="Branch">target branch</see>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Branch.branchID"/> field.
		/// </value>
		[Branch(DisplayName = "Destination Branch", IsDetail = true, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? TargetBranchID
		{
			get;
			set;
		}
		#endregion
		#region AccountSource
		/// <inheritdoc cref="TargetBranchID"/>
		public abstract class accountSource : PX.Data.BQL.BqlString.Field<accountSource> { }
		protected String _AccountSource;
		/// <summary>
		/// The account to be used for billing of the billing rule.
		/// </summary>
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Use Sales Account From", Required = true)]
		public virtual String AccountSource
		{
			get
			{
				return this._AccountSource;
			}
			set
			{
				this._AccountSource = value;
			}
		}
		#endregion
		#region AccountID
		/// <inheritdoc cref="AccountID"/>
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;
		/// <summary>
		/// The <see cref="Account">account</see> to be used for billing.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Account.accountID"/> field.
		/// </value>
		[Account(DisplayName = "Sales Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
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
		/// The sub mask of the billing rule.
		/// </summary>
		[PMBillSubAccountMask]
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
		#region SubMaskBudget
		/// <inheritdoc cref="SubMaskBudget"/>
		public abstract class subMaskBudget : PX.Data.BQL.BqlString.Field<subMaskBudget> { }
		/// <summary>
		/// The sub mask budget of the billing rule.
		/// </summary>
		[PMBillBudgetSubAccountMask]
		public virtual String SubMaskBudget
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
		/// The <see cref="Sub">subaccount</see> to be used for billing.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Sub.SubID"/> field.
		/// </value>
		[SubAccount(typeof(PMBillingRule.accountID), DisplayName = "Sales Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
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
		#region IncludeNonBillable
		/// <inheritdoc cref="IncludeNonBillable"/>
		public abstract class includeNonBillable : PX.Data.BQL.BqlBool.Field<includeNonBillable> { }
		protected Boolean? _IncludeNonBillable;
		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the system will include non-billable transactions in the created invoice.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Include Non-Billable Transactions")]
		public virtual Boolean? IncludeNonBillable
		{
			get
			{
				return this._IncludeNonBillable;
			}
			set
			{
				this._IncludeNonBillable = value;
			}
		}
		#endregion
		#region CopyNotes
		/// <inheritdoc cref="CopyNotes"/>
		public abstract class copyNotes : PX.Data.BQL.BqlBool.Field<copyNotes> { }
		protected Boolean? _CopyNotes;
		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />)that the notes and files are copied to the created invoice.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
        [PXUIField(DisplayName = "Copy Notes and Files", Visibility = PXUIVisibility.Visible)]
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

		#region RateTypeID
		/// <inheritdoc cref="CopyNotes"/>
		public abstract class rateTypeID : PX.Data.BQL.BqlString.Field<rateTypeID> { }
		protected String _RateTypeID;
		/// <summary>
		/// The <see cref="PMRateType">rate type</see> used in the billing rule step.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMRateType.rateTypeID"/> field.
		/// </value>
		[PXDBString(PMRateType.rateTypeID.Length, IsUnicode = true)]
		[PXSelector(typeof(PMRateType.rateTypeID), DescriptionField = typeof(PMRateType.description))]
		[PXUIField(DisplayName = "Rate Type")]
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
		#region NoRateOption
		/// <inheritdoc cref="NoRateOption"/>
		public abstract class noRateOption : PX.Data.BQL.BqlString.Field<noRateOption> { }
		protected String _NoRateOption;
		/// <summary>
		/// The action to be performed if the rate value has not been defined.
		/// </summary>		
		[PMNoRateOption.BillingList]
		[PXDefault(PMNoRateOption.RaiseError)]
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
		#region InvoiceFormula
		/// <inheritdoc cref="InvoiceFormula"/>
		public abstract class invoiceFormula : PX.Data.BQL.BqlString.Field<invoiceFormula> { }
		protected String _InvoiceFormula;
		/// <summary>
		/// The formula to be used to generate the description for the pro forma invoice that is created during billing.
		/// </summary>
		[PXDBString(4000, IsUnicode = true)]
		[PXUIField(DisplayName = "Invoice Description Formula")]
		public virtual String InvoiceFormula
		{
			get
			{ 
				return this._InvoiceFormula;
			}
			set
			{
				this._InvoiceFormula = value;
			}
		}
		#endregion
		#region QtyFormula
		/// <inheritdoc cref="QtyFormula"/>
		public abstract class qtyFormula : PX.Data.BQL.BqlString.Field<qtyFormula> { }
		protected String _QtyFormula;
		/// <summary>
		/// The formula for calculating the quantity of a line of the generated invoice.
		/// </summary>
		[PXDBString(4000, IsUnicode = true)]
		[PXUIField(DisplayName = "Line Quantity Formula")]
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
		#region AmountFormula
		/// <inheritdoc cref="AmountFormula"/>
		public abstract class amountFormula : PX.Data.BQL.BqlString.Field<amountFormula> { }
		protected String _AmountFormula;
		/// <summary>
		/// The formula for calculating the amount of a line of the generated invoice.
		/// </summary>
		[PXDBString(4000, IsUnicode = true)]
		[PXUIField(DisplayName = "Line Amount Formula")]
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
		/// The formula to be used to generate the description for a line of the generated invoice.
		/// </summary>
		[PXDBString(4000, IsUnicode = true)]
		[PXUIField(DisplayName = "Line Description Formula")]
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
		#region GroupByItem
		/// <inheritdoc cref="GroupByItem"/>
		public abstract class groupByItem : PX.Data.BQL.BqlBool.Field<groupByItem> { }
		protected Boolean? _GroupByItem;
		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the transactions with the same inventory item are grouped in a single line of the generated invoice.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Inventory ID")]
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
		/// A Boolean value that indicates (if set to <see langword="true" />) that the transactions with the same employee are grouped in a single line of the generated invoice.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Employee")]
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
		/// <summary>/// A Boolean value that indicates (if set to <see langword="true" />) that the transactions with the same date are groupped in a single line of the generated invoice.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Date")]
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
		/// A Boolean value that indicates (if set to <see langword="true" />) that the transactions with the same date are grouped in a single line of the generated invoice.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Vendor")]
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
		public virtual Boolean FullDetail
		{
			get
			{
				return GroupByItem != true && GroupByEmployee != true && GroupByDate != true && GroupByVendor != true;
			}
		}
		#endregion
		#region IncludeZeroAmountAndQty
		/// <inheritdoc cref="IncludeZeroAmountAndQty"/>
		public abstract class includeZeroAmountAndQty : PX.Data.BQL.BqlBool.Field<includeZeroAmountAndQty> { }
		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the system will add a transaction in the created invoice even if the transaction has a zero quantity or a zero amount.
		/// </summary>
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Create Lines with Zero Amount and Quantity")]
		public virtual Boolean? IncludeZeroAmountAndQty
		{
			get;
			set;
		}
		#endregion
		#region IncludeZeroAmount
		/// <inheritdoc cref="IncludeZeroAmount"/>
		public abstract class includeZeroAmount : PX.Data.BQL.BqlBool.Field<includeZeroAmount> { }
		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the system will add a transaction in the created invoice even if the transaction has an amount of zero.
		/// </summary>
		[PXBool]
		[PXUIField(DisplayName = "Create Lines with Zero Amount and Quantity", Visible = false)]
		public virtual Boolean? IncludeZeroAmount
		{
			get { return IncludeZeroAmountAndQty; }
			set { IncludeZeroAmountAndQty = value; }
		}
		#endregion
		#region IsActive
		/// <inheritdoc cref="IsActive"/>
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the rule is available for use in projects and project tasks.
		/// </summary>
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual Boolean? IsActive
		{
			get;
			set;
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
	public static class PMAccountSource
	{
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(None,  Messages.AccountSource_SourceTransaction),
					Pair(BillingRule, Messages.AccountSource_BillingRule),
					Pair(Project, Messages.AccountSource_Project),
					Pair(Task, Messages.AccountSource_Task),
					Pair(Customer, Messages.AccountSource_Customer),
					Pair(Employee, Messages.AccountSource_Employee),
				}) {}
		}

		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public class ListAttributeBudget : PXStringListAttribute
		{
			public ListAttributeBudget() : base(
				new[]
				{
				
					Pair(None,  Messages.AccountSource_CurrentBranch),
					Pair(BillingRule, Messages.AccountSource_BillingRule),
					Pair(Project, Messages.AccountSource_Project),
					Pair(Task, Messages.AccountSource_Task),
					Pair(Customer, Messages.AccountSource_Customer),
					Pair(Employee, Messages.AccountSource_Employee),
				})
			{ }
		}

		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public class RecurentListAttribute : PXStringListAttribute
		{
			public RecurentListAttribute() : base(
				new[]
				{
					Pair(None, Messages.AccountSource_ARDefault),
					Pair(RecurringBillingItem, Messages.AccountSource_RecurentBillingItem),
					Pair(Project, Messages.AccountSource_Project),
					Pair(Task, Messages.AccountSource_Task),
					Pair(InventoryItem, Messages.AccountSource_InventoryItem),
					Pair(Customer, Messages.AccountSource_Customer),					
				}) {}
		}

		public const string None = "N";
		public const string BillingRule = "B";
		public const string RecurringBillingItem = "B";
		public const string Project = "P";
		public const string Task = "T";
		public const string InventoryItem = "I";
		public const string Customer = "C";
		public const string Employee = "E";
		public const string AccountGroup = "A";
		public const string Branch = "R";
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public static class PMBranchSource
	{
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { None, BillingRule, Project, Task },
				new string[] { Messages.AccountSource_None, Messages.AccountSource_BillingRule, Messages.AccountSource_Project, Messages.AccountSource_Task })
			{; }
		}
			

		public const string None = "N";
		public const string BillingRule = "B";
		public const string Project = "P";
		public const string Task = "T";
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public static class PMBillingType
	{
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Transaction, Budget },
				new string[] { Messages.PMBillingType_Transaction, Messages.PMBillingType_Budget })
			{; }
		}

		public const string Transaction = "T";
		public const string Budget = "B";

		public class transaction : PX.Data.BQL.BqlString.Constant<transaction>
		{
			public transaction() : base(Transaction) {; }
		}

		public class budget : PX.Data.BQL.BqlString.Constant<budget>
		{
			public budget() : base(Budget) {; }
		}

	}
}
