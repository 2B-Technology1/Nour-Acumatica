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
using PX.Data.BQL;
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AR;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.TX;
using System;

namespace PX.Objects.PM
{
	/// <summary>The smallest identifiable and essential piece of a job that serves as a unit of work, and as a method of differentiating between the various components of
	/// a project. A task is always defined within the scope of a project. The task budget, profitability and balances are monitored in scope of account groups.</summary>
	[PXCacheName(Messages.ProjectTask)]
	[PXPrimaryGraph(new Type[] {
					typeof(TemplateGlobalTaskMaint),
					typeof(TemplateTaskMaint),
					typeof(ProjectTaskEntry) },
					new Type[] {
					typeof(Select2<PMTask, InnerJoin<PMProject, On<PMTask.projectID, Equal<PMProject.contractID>>>, Where<PMProject.nonProject, Equal<True>, And<PMProject.baseType, Equal<CT.CTPRType.projectTemplate>, And<PMTask.taskID, Equal<Current<PMTask.taskID>>>>>>),
					typeof(Select2<PMTask, InnerJoin<PMProject, On<PMTask.projectID, Equal<PMProject.contractID>>>, Where<PMProject.nonProject, Equal<False>, And<PMProject.baseType, Equal<CT.CTPRType.projectTemplate>, And<PMTask.taskID, Equal<Current<PMTask.taskID>>>>>>),
					typeof(Select2<PMTask, InnerJoin<PMProject, On<PMTask.projectID, Equal<PMProject.contractID>>>, Where<PMProject.nonProject, Equal<False>, And<PMProject.baseType, Equal<CT.CTPRType.project>, And<PMTask.taskID, Equal<Current<PMTask.taskID>>>>>>)
					})]
	[PXGroupMask(typeof(LeftJoin<PMProject, On<PMProject.contractID, Equal<PMTask.projectID>>>),
		WhereRestriction = typeof(Where<PMProject.contractID, IsNull, Or<Match<PMProject, Current<AccessInfo.userName>>>>))]
	[Serializable]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PMTask : PX.Data.IBqlTable, INotable
	{
		#region Keys
		/// <summary>
		/// Primary Key
		/// </summary>
		public class PK : PrimaryKeyOf<PMTask>.By<PMTask.projectID, PMTask.taskID>
		{
			public static PMTask Find(PXGraph graph, int? projectID, int? taskID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, projectID, taskID, options);
			public static PMTask FindDirty(PXGraph graph, int? projectID, int? taskID)
				=> (PMTask)PXSelect<PMTask, Where<projectID, Equal<Required<projectID>>, And<taskID, Equal<Required<taskID>>>>>.SelectWindowed(graph, 0, 1, projectID, taskID);
		}

		/// <summary>
		/// Unique Key
		/// </summary>
		public class UK : PrimaryKeyOf<PMTask>.By<projectID, taskCD>
		{
			public static PMTask Find(PXGraph graph, int? projectID, string taskCD, PKFindOptions options = PKFindOptions.None) => FindBy(graph, projectID, taskCD, options);
		}
		#endregion

		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;

		[PXBool]
		[PXUnboundDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion

		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

		/// <summary>
		/// Gets or sets the parent Project.
		/// </summary>
		protected Int32? _ProjectID;
		[Project(DisplayName = "Project ID", IsKey = true, DirtyRead = true)]
		[PXParent(typeof(Select<PMProject, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>))]
		[PXDBDefault(typeof(PMProject.contractID))]
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
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		protected Int32? _TaskID;
		/// <summary>The unique identifier of the task.</summary>
		[PXDBIdentity()]
		[PXReferentialIntegrityCheck]
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
		public abstract class taskCD : PX.Data.BQL.BqlString.Field<taskCD> { }
		protected String _TaskCD;
		/// <summary>The unique identifier of the task. This is a segmented key, which format is configured on the Segmented Keys (CS202000) form.</summary>
		[PXDimension(ProjectTaskAttribute.DimensionName)]
		[PXDBString(IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
		[PXUIField(DisplayName = "Task ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
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

		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		/// <summary>The description of the task.</summary>
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
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		protected Int32? _CustomerID;
		/// <summary>The customer for the task. The customer is always set at the project level.</summary>
		/// <value>The value is copied from the project.</value>
		[PXDefault(typeof(Search<PMProject.customerID, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[Customer(DescriptionField = typeof(Customer.acctName), Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Int32? CustomerID
		{
			get
			{
				return this._CustomerID;
			}
			set
			{
				this._CustomerID = value;
			}
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;

		/// <summary>The customer location.</summary>
		[LocationActive(typeof(Where<Location.bAccountID, Equal<Current<PMTask.customerID>>>), Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Location", DescriptionField = typeof(Location.descr))]
		[PXDefault(typeof(Search<PMProject.locationID, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion
		#region RateTableID
		public abstract class rateTableID : PX.Data.BQL.BqlString.Field<rateTableID> { }
		protected String _RateTableID;

		/// <summary>The <see cref="PMRateTable">rate table</see>.</summary>
		[PXDBString(PMRateTable.rateTableID.Length, IsUnicode = true)]
		[PXDefault(typeof(Search<PMProject.rateTableID, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>, And<PMProject.nonProject, Equal<False>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Rate Table")]
		[PXSelector(typeof(PMRateTable.rateTableID), DescriptionField = typeof(PMRateTable.description))]
		[PXForeignReference(typeof(Field<rateTableID>.IsRelatedTo<PMRateTable.rateTableID>))]
		public virtual String RateTableID
		{
			get
			{
				return this._RateTableID;
			}
			set
			{
				this._RateTableID = value;
			}
		}
		#endregion
		#region BillingID
		public abstract class billingID : PX.Data.BQL.BqlString.Field<billingID> { }
		protected String _BillingID;

		/// <summary>The <see cref="PMBilling">billing rules</see>.</summary>
		[PXForeignReference(typeof(Field<billingID>.IsRelatedTo<PMBilling.billingID>))]
		[PXDefault(typeof(PMProject.billingID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<PMBilling.billingID, Where<PMBilling.isActive, Equal<True>>>), DescriptionField = typeof(PMBilling.description))]
		[PXUIField(DisplayName = "Billing Rule")]
		[PXDBString(PMBilling.billingID.Length, IsUnicode = true)]
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
		#region AllocationID
		public abstract class allocationID : PX.Data.BQL.BqlString.Field<allocationID> { }
		protected String _AllocationID;
		/// <summary>The <see cref="PMAllocation">allocation rules</see>.</summary>
		[PXForeignReference(typeof(Field<allocationID>.IsRelatedTo<PMAllocation.allocationID>))]
		[PXDefault(typeof(PMProject.allocationID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<PMAllocation.allocationID, Where<PMAllocation.isActive, Equal<True>>>), DescriptionField = typeof(PMAllocation.description))]
		[PXUIField(DisplayName = "Allocation Rule")]
		[PXDBString(PMAllocation.allocationID.Length, IsUnicode = true)]
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
		#region BillingOption
		public abstract class billingOption : PX.Data.BQL.BqlString.Field<billingOption> { }
		protected String _BillingOption;
		/// <summary>The <see cref="PMBillingOption">way</see> the project is billed.</summary>
		[PXDBString(1, IsFixed = true)]
		[PMBillingOption.List()]
		[PXDefault(PMBillingOption.OnBilling)]
		[PXUIField(DisplayName = "Billing Option", Required = true, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String BillingOption
		{
			get
			{
				return this._BillingOption;
			}
			set
			{
				this._BillingOption = value;
			}
		}
		#endregion
		#region CompletedPctMethod
		public abstract class completedPctMethod : PX.Data.BQL.BqlString.Field<completedPctMethod> { }
		protected String _CompletedPctMethod;
		/// <summary>The calculation method of the completion.</summary>
		[PXDBString(1, IsFixed = true)]
		[PMCompletedPctMethod.List()]
		[PXDefault(PMCompletedPctMethod.Manual)]
		[PXUIField(DisplayName = "Completion Method", Required = true, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String CompletedPctMethod
		{
			get
			{
				return this._CompletedPctMethod;
			}
			set
			{
				this._CompletedPctMethod = value;
			}
		}
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		protected String _Status;
		/// <summary>The task <see cref="ProjectTaskStatus">status</see>.</summary>
		[PXDBString(1, IsFixed = true)]
		[ProjectTaskStatus.List()]
		[PXDefault(ProjectTaskStatus.Planned)]
		[PXUIField(DisplayName = "Status", Required = true, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Status
		{
			get
			{
				return this._Status;
			}
			set
			{
				this._Status = value;
			}
		}
		#endregion
		#region PlannedStartDate
		public abstract class plannedStartDate : PX.Data.BQL.BqlDateTime.Field<plannedStartDate> { }
		protected DateTime? _PlannedStartDate;
		/// <summary>The date when the task is supposed to be started.</summary>
		[PXDBDate()]
		[PXUIField(DisplayName = "Planned Start Date")]
		public virtual DateTime? PlannedStartDate
		{
			get
			{
				return this._PlannedStartDate;
			}
			set
			{
				this._PlannedStartDate = value;
			}
		}
		#endregion
		#region PlannedEndDate
		public abstract class plannedEndDate : PX.Data.BQL.BqlDateTime.Field<plannedEndDate> { }
		protected DateTime? _PlannedEndDate;
		/// <summary>The date when the task is supposed to be finished.</summary>
		[PXDBDate()]
		[PXVerifyEndDate(typeof(plannedStartDate), AutoChangeWarning = true)]
		[PXUIField(DisplayName = "Planned End Date")]
		public virtual DateTime? PlannedEndDate
		{
			get
			{
				return this._PlannedEndDate;
			}
			set
			{
				this._PlannedEndDate = value;
			}
		}
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		protected DateTime? _StartDate;
		/// <summary>The actual date when the task is started.</summary>
		[PXDBDate()]
		[PXUIField(DisplayName = "Start Date")]
		public virtual DateTime? StartDate
		{
			get
			{
				return this._StartDate;
			}
			set
			{
				this._StartDate = value;
			}
		}
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
		protected DateTime? _EndDate;
		/// <summary>The actual date when the task is finished.</summary>
		[PXDBDate()]
		[PXVerifyEndDate(typeof(startDate), AutoChangeWarning = true)]
		[PXUIField(DisplayName = "End Date")]
		public virtual DateTime? EndDate
		{
			get
			{
				return this._EndDate;
			}
			set
			{
				this._EndDate = value;
			}
		}
		#endregion
		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }
		protected String _ExtRefNbr;
		[PXDBString(30, IsUnicode = true)]

		/// <summary>
		/// The external reference number.
		/// </summary>
		[PXUIField(DisplayName = "External Ref. Nbr")]
		public virtual String ExtRefNbr
		{
			get
			{
				return this._ExtRefNbr;
			}
			set
			{
				this._ExtRefNbr = value;
			}
		}
		#endregion
		#region ApproverID
		public abstract class approverID : PX.Data.BQL.BqlInt.Field<approverID> { }
		protected Int32? _ApproverID;
		/// <summary>The <see cref="EPEmployee" /> that approves or rejects the activities created under the given task.</summary>
		/// <value>If the value is null, the approval is not required. Otherwise, either <see cref="PMTask.ApproverID" /> or <see cref="PMProject.ApproverID" /> must approve the activity before it can be
		/// released to the project.</value>
		[PXDBInt]
		[PXEPEmployeeSelector]
		[PXUIField(DisplayName = "Approver", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Int32? ApproverID
		{
			get
			{
				return this._ApproverID;
			}
			set
			{
				this._ApproverID = value;
			}
		}
		#endregion
		#region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }
		protected String _TaxCategoryID;
		/// <summary>Obsolete field. Not used anywhere.</summary>
		/// <exclude />
		[PXDBString(TaxCategory.taxCategoryID.Length, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Category")]
		[PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
		[PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
		public virtual String TaxCategoryID
		{
			get
			{
				return this._TaxCategoryID;
			}
			set
			{
				this._TaxCategoryID = value;
			}
		}
		#endregion
		#region DefaultSalesAccountID
		public abstract class defaultSalesAccountID : PX.Data.BQL.BqlInt.Field<defaultSalesAccountID> { }
		/// <summary>The default sales account. The value can be used in an allocation or a billing as a default for the new <see cref="PMTran" /> and <see cref="ARTran" />.</summary>
		[PXDefault(typeof(PMProject.defaultSalesAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(DisplayName = "Default Sales Account", AvoidControlAccounts = true)]
		public virtual Int32? DefaultSalesAccountID
		{
			get;
			set;
		}
		#endregion
		#region DefaultSalesSubID
		public abstract class defaultSalesSubID : PX.Data.BQL.BqlInt.Field<defaultSalesSubID> { }
		/// <summary>The default sales subaccount. The value can be used in an allocation or a billing as a default for the new <see cref="PMTran" /> and <see cref="ARTran" />.</summary>
		[PXDefault(typeof(PMProject.defaultSalesSubID), PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(PMTask.defaultSalesAccountID), typeof(PMTask.defaultBranchID), DisplayName = "Default Sales Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? DefaultSalesSubID
		{
			get;
			set;
		}
		#endregion
		#region DefaultExpenseAccountID
		public abstract class defaultExpenseAccountID : PX.Data.BQL.BqlInt.Field<defaultExpenseAccountID> { }
		/// <summary>The default cost account. The value can be used in an allocation or a cost transaction as a default for the new <see cref="PMTran" />.</summary>
		[PXDefault(typeof(PMProject.defaultExpenseAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(DisplayName = "Default Cost Account", AvoidControlAccounts = true)]
		public virtual Int32? DefaultExpenseAccountID
		{
			get;
			set;
		}
		#endregion
		#region DefaultExpenseSubID
		public abstract class defaultExpenseSubID : PX.Data.BQL.BqlInt.Field<defaultExpenseSubID> { }
		/// <summary>The default cost subaccount. The value can be used in an allocation or a cost transaction as a default for the new <see cref="PMTran" />.</summary>
		[PXDefault(typeof(PMProject.defaultExpenseSubID), PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(PMTask.defaultExpenseAccountID), typeof(PMTask.defaultBranchID), DisplayName = "Default Cost Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? DefaultExpenseSubID
		{
			get;
			set;
		}
		#endregion
		#region DefaultAccrualAccountID
		public abstract class defaultAccrualAccountID : PX.Data.BQL.BqlInt.Field<defaultAccrualAccountID> { }
		protected Int32? _DefaultAccrualAccountID;
		/// <summary>The default accrual account. The field is used depending on the <see cref="PMSetup.ExpenseAccrualSubMask" /> mask setting.</summary>
		[PXDefault(typeof(PMProject.defaultAccrualAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(DisplayName = "Accrual Account", AvoidControlAccounts = true)]
		public virtual Int32? DefaultAccrualAccountID
		{
			get
			{
				return this._DefaultAccrualAccountID;
			}
			set
			{
				this._DefaultAccrualAccountID = value;
			}
		}
		#endregion
		#region DefaultAccrualSubID
		public abstract class defaultAccrualSubID : PX.Data.BQL.BqlInt.Field<defaultAccrualSubID> { }
		protected Int32? _DefaultAccrualSubID;

		/// <summary>The default accrual subaccount. The field is used depending on the <see cref="PMSetup.ExpenseAccrualSubMask" /> mask setting.</summary>
		[PXDefault(typeof(PMProject.defaultAccrualSubID), PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(DisplayName = "Accrual Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? DefaultAccrualSubID
		{
			get
			{
				return this._DefaultAccrualSubID;
			}
			set
			{
				this._DefaultAccrualSubID = value;
			}
		}
		#endregion
		#region DefaultBranchID
		public abstract class defaultBranchID : PX.Data.BQL.BqlInt.Field<defaultBranchID> { }
		protected Int32? _DefaultBranchID;

		/// <summary>
		/// The identifier of the <see cref="Branch"/> associated with the project task.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Branch.BranchID"/> field.
		/// </value>
		[Branch(useDefaulting: false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDefault(typeof(Search<PMProject.defaultBranchID, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? DefaultBranchID
		{
			get
			{
				return this._DefaultBranchID;
			}
			set
			{
				this._DefaultBranchID = value;
			}
		}
		#endregion
		#region WipAccountGroupID
		public abstract class wipAccountGroupID : PX.Data.BQL.BqlInt.Field<wipAccountGroupID> { }
		protected Int32? _WipAccountGroupID;

		/// <summary>
		/// The identifier of the work-in-progress <see cref="PMAccountGroup">account group</see> associated with the task.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMAccountGroup.GroupID"/> field.
		/// </value>
		[PXRestrictor(typeof(Where<PMAccountGroup.isActive, Equal<True>>), PM.Messages.InactiveAccountGroup, typeof(PMAccountGroup.groupCD))]
		[AccountGroup(DisplayName = "Non-Billable WIP Account Group")]
		public virtual Int32? WipAccountGroupID
		{
			get
			{
				return this._WipAccountGroupID;
			}
			set
			{
				this._WipAccountGroupID = value;
			}
		}
		#endregion
		#region CompletedPercent
		public abstract class completedPercent : PX.Data.BQL.BqlDecimal.Field<completedPercent> { }
		/// <summary>The task completion state in percents. Depending on settings, this value either maintained manually or can be auto-calculated based on the budget ratio of
		/// actual or revised values.</summary>
		[PXDBDecimal(2, MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIEnabled(typeof(Where<completedPctMethod, Equal<PMCompletedPctMethod.manual>>))]
		[PXUIField(DisplayName = "Completed (%)")]
		public virtual decimal? CompletedPercent
		{
			get;
			set;
		}
		#endregion
		#region IsDefault
		public abstract class isDefault : PX.Data.BQL.BqlBool.Field<isDefault> { }

		/// <summary>Specifies (if set to <see langword="true"></see>) that the task is default.</summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Default")]
		public virtual Boolean? IsDefault
		{
			get;
			set;
		}
		#endregion
		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		protected Boolean? _IsActive;
		/// <summary>Specifies (if set to <see langword="true"></see>) that the task is active. <see cref="PMTran" /> can be created only for active tasks.</summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Active", Enabled = false, Visibility = PXUIVisibility.Visible, Visible = false)]
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
		#region IsCompleted
		public abstract class isCompleted : PX.Data.BQL.BqlBool.Field<isCompleted> { }
		protected Boolean? _IsCompleted;
		/// <summary>Specifies (if set to <see langword="true"></see>) that the task is completed.</summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Completed", Enabled = false, Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual Boolean? IsCompleted
		{
			get
			{
				return this._IsCompleted;
			}
			set
			{
				this._IsCompleted = value;
			}
		}
		#endregion
		#region IsCancelled
		public abstract class isCancelled : PX.Data.BQL.BqlBool.Field<isCancelled> { }
		protected Boolean? _IsCancelled;
		/// <summary>Specifies (if set to <see langword="true"></see>) that the task is cancelled.</summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Cancelled", Enabled = false, Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual Boolean? IsCancelled
		{
			get
			{
				return this._IsCancelled;
			}
			set
			{
				this._IsCancelled = value;
			}
		}
		#endregion
		#region BillSeparately
		public abstract class billSeparately : PX.Data.BQL.BqlBool.Field<billSeparately> { }
		protected Boolean? _BillSeparately;
		/// <summary>Specifies (if set to <see langword="true"></see>) that the task is billed in a separate invoice.</summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Bill Separately")]
		public virtual Boolean? BillSeparately
		{
			get
			{
				return this._BillSeparately;
			}
			set
			{
				this._BillSeparately = value;
			}
		}
		#endregion
		#region ProgressBillingBase
		public abstract class progressBillingBase : Data.BQL.BqlDecimal.Field<progressBillingBase> { }

		[PXDBString]
		[PXDefault(PM.ProgressBillingBase.Amount)]
		[PXUIField(DisplayName = Messages.ProgressBillingBase)]
		[ProgressBillingBase.List]
		public string ProgressBillingBase { get; set; }
		#endregion
		#region Type
		public abstract class type : BqlString.Field<type> { }
		/// <summary>The task type, which is used in construction functional area.</summary>
		[PXDBString(10)]
		[PXDefault(ProjectTaskType.CostRevenue)]
		[PXUIField(DisplayName = ProjectAccountingLabels.Type, Required = true, FieldClass = nameof(FeaturesSet.Construction))]
		[ProjectTaskType.List]
		public string Type
		{
			get;
			set;
		}
		#endregion

		#region VisibleInGL
		public abstract class visibleInGL : PX.Data.BQL.BqlBool.Field<visibleInGL> { }
		protected Boolean? _VisibleInGL;
		/// <summary>Specifies (if set to <see langword="true"></see>) that the task is visible in the GL module. If the task is invisible, it will not be displayed in the field
		/// selectors in this module.</summary>
		[PXDBBool()]
		[PXDefault(typeof(Search<PMProject.visibleInGL, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>))]
		[PXUIField(DisplayName = "GL")]
		public virtual Boolean? VisibleInGL
		{
			get
			{
				return this._VisibleInGL;
			}
			set
			{
				this._VisibleInGL = value;
			}
		}
		#endregion
		#region VisibleInAP
		public abstract class visibleInAP : PX.Data.BQL.BqlBool.Field<visibleInAP> { }
		protected Boolean? _VisibleInAP;
		/// <summary>Specifies (if set to <see langword="true"></see>) that the task is visible in the AP module. If the task is invisible, it will not be displayed in the field selectors in
		/// this module.</summary>
		[PXDBBool()]
		[PXDefault(typeof(Search<PMProject.visibleInAP, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>))]
		[PXUIField(DisplayName = "AP")]
		public virtual Boolean? VisibleInAP
		{
			get
			{
				return this._VisibleInAP;
			}
			set
			{
				this._VisibleInAP = value;
			}
		}
		#endregion
		#region VisibleInAR
		public abstract class visibleInAR : PX.Data.BQL.BqlBool.Field<visibleInAR> { }
		protected Boolean? _VisibleInAR;
		/// <summary>Specifies (if set to <see langword="true"></see>) that the task is visible in the AR module. If the task is invisible, it will not be displayed in the field selectors in
		/// this module.</summary>
		[PXDBBool()]
		[PXDefault(typeof(Search<PMProject.visibleInAR, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>))]
		[PXUIField(DisplayName = "AR")]
		public virtual Boolean? VisibleInAR
		{
			get
			{
				return this._VisibleInAR;
			}
			set
			{
				this._VisibleInAR = value;
			}
		}
		#endregion
		#region VisibleInSO
		public abstract class visibleInSO : PX.Data.BQL.BqlBool.Field<visibleInSO> { }
		protected Boolean? _VisibleInSO;
		/// <summary>Specifies (if set to <see langword="true"></see>) that the task is visible in the SO module. If the task is invisible, it will not be displayed in the field
		/// selectors in this module.</summary>
		[PXDBBool()]
		[PXDefault(typeof(Search<PMProject.visibleInSO, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>))]
		[PXUIField(DisplayName = "SO")]
		public virtual Boolean? VisibleInSO
		{
			get
			{
				return this._VisibleInSO;
			}
			set
			{
				this._VisibleInSO = value;
			}
		}
		#endregion
		#region VisibleInPO
		public abstract class visibleInPO : PX.Data.BQL.BqlBool.Field<visibleInPO> { }
		protected Boolean? _VisibleInPO;
		/// <summary>Specifies (if set to <see langword="true"></see>) that the task is visible in the PO module. If the task is invisible, it will not be displayed in the field
		/// selectors in this module.</summary>
		[PXDBBool()]
		[PXDefault(typeof(Search<PMProject.visibleInPO, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>))]
		[PXUIField(DisplayName = "PO")]
		public virtual Boolean? VisibleInPO
		{
			get
			{
				return this._VisibleInPO;
			}
			set
			{
				this._VisibleInPO = value;
			}
		}
		#endregion

		#region VisibleInTA
		public abstract class visibleInTA : PX.Data.BQL.BqlBool.Field<visibleInTA> { }
		/// <summary>
		/// Gets or sets whether the Task is visible in the EP Time Module.
		/// If Project Task is set as invisible - it will not show up in the field selectors in the given module.
		/// </summary>
		protected Boolean? _VisibleInTA;
		[PXDBBool()]
		[PXDefault(typeof(Search<PMProject.visibleInTA, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>))]
		[PXUIField(DisplayName = "Time Entries")]
		public virtual Boolean? VisibleInTA
		{
			get
			{
				return this._VisibleInTA;
			}
			set
			{
				this._VisibleInTA = value;
			}
		}
		#endregion
		#region VisibleInEA
		public abstract class visibleInEA : PX.Data.BQL.BqlBool.Field<visibleInEA> { }
		/// <summary>
		/// Gets or sets whether the Task is visible in the EP Expense Module.
		/// If Project Task is set as invisible - it will not show up in the field selectors in the given module.
		/// </summary>
		protected Boolean? _VisibleInEA;
		[PXDBBool()]
		[PXDefault(typeof(Search<PMProject.visibleInEA, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>))]
		[PXUIField(DisplayName = "Expenses")]
		public virtual Boolean? VisibleInEA
		{
			get
			{
				return this._VisibleInEA;
			}
			set
			{
				this._VisibleInEA = value;
			}
		}
		#endregion
		#region VisibleInIN
		public abstract class visibleInIN : PX.Data.BQL.BqlBool.Field<visibleInIN> { }
		protected Boolean? _VisibleInIN;
		/// <summary>Specifies (if set to <see langword="true"></see>) that the task is visible in the IN module. If the task is invisible, it will not be displayed in the field
		/// selectors in this module.</summary>
		[PXDBBool()]
		[PXDefault(typeof(Search<PMProject.visibleInIN, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>))]
		[PXUIField(DisplayName = "IN")]
		public virtual Boolean? VisibleInIN
		{
			get
			{
				return this._VisibleInIN;
			}
			set
			{
				this._VisibleInIN = value;
			}
		}
		#endregion
		#region VisibleInCA
		public abstract class visibleInCA : PX.Data.BQL.BqlBool.Field<visibleInCA> { }
		protected Boolean? _VisibleInCA;
		/// <summary>Specifies (if set to <see langword="true"></see>) that the task is visible in the CA module. If the task is invisible, it will not be displayed in the field
		/// selectors in this module.</summary>
		[PXDBBool()]
		[PXDefault(typeof(Search<PMProject.visibleInCA, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>))]
		[PXUIField(DisplayName = "CA")]
		public virtual Boolean? VisibleInCA
		{
			get
			{
				return this._VisibleInCA;
			}
			set
			{
				this._VisibleInCA = value;
			}
		}
		#endregion
		#region VisibleInCR
		public abstract class visibleInCR : PX.Data.BQL.BqlBool.Field<visibleInCR> { }
		protected Boolean? _VisibleInCR;
		/// <summary>Specifies (if set to <see langword="true"></see>) that the task is visible in the CR module. If the task is invisible, it will not be displayed in the field
		/// selectors in this module.</summary>
		[PXDBBool()]
		[PXDefault(typeof(Search<PMProject.visibleInCR, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>))]
		[PXUIField(DisplayName = "CRM")]
		public virtual Boolean? VisibleInCR
		{
			get
			{
				return this._VisibleInCR;
			}
			set
			{
				this._VisibleInCR = value;
			}
		}
		#endregion
		#region AutoIncludeInPrj
		public abstract class autoIncludeInPrj : PX.Data.BQL.BqlBool.Field<autoIncludeInPrj> { }
		protected bool? _AutoIncludeInPrj;
		/// <summary>Specifies (if set to <see langword="true"></see>) that this task should be automatically created when a template is assigned to the project. This field is used for
		/// project templates.</summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Automatically Include in Project")]
		public virtual bool? AutoIncludeInPrj
		{
			get
			{
				return _AutoIncludeInPrj;
			}
			set
			{
				_AutoIncludeInPrj = value;
			}
		}
		#endregion

		#region FormCaptionDescription
		[PXString()]
		[PXFormula(typeof(SmartJoin<Space, description, Selector<projectID, PMProject.description>>))]
		public string FormCaptionDescription
		{
			get;
			set;
		}
		#endregion

		#region Attributes
		public abstract class attributes : BqlAttributes.Field<attributes> { }

		/// <summary>The entity attributes.</summary>
		[CRAttributesField(typeof(PMTask.classID))]
		public virtual string[] Attributes { get; set; }

		#region ClassID

		public abstract class classID : PX.Data.BQL.BqlString.Field<classID> { }
		/// <summary>The class ID for the attributes.</summary>
		/// <value>Always returns <see cref="GroupTypes.Task" />.</value>
		[PXString(20)]
		public virtual string ClassID
		{
			get { return GroupTypes.Task; }
		}
		#endregion

		#endregion

		#region templateID
		public abstract class templateID : PX.Data.BQL.BqlInt.Field<templateID> { }
		[PXInt]
		public virtual int? TemplateID { get; set; }
		#endregion

		#region System Columns
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote(DescriptionField = typeof(PMTask.taskCD))]
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
	public static class ProjectTaskStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Planned, Messages.InPlanning),
					Pair(Active, Messages.Active),
					Pair(Canceled, Messages.Canceled),
					Pair(Completed, Messages.Completed),
				}) {}
		}

		public const string Planned = "D";
		public const string Active = "A";
		public const string Canceled = "C";
		public const string Completed = "F";
		
	    public class planned : PX.Data.BQL.BqlString.Constant<planned>
		{
		    public planned() : base(Planned){}
		}

		public class active : PX.Data.BQL.BqlString.Constant<active>
		{
			public active() : base(Active) { ;}
		}

		public class completed : PX.Data.BQL.BqlString.Constant<completed>
		{
			public completed() : base(Completed) { ;}
		}

		public class canceled : PX.Data.BQL.BqlString.Constant<canceled>
		{
			public canceled() : base(Canceled) {; }
		}
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public static class PMBillingOption
    {
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(OnBilling, Messages.OnBilling),
					Pair(OnTaskCompletion, Messages.OnTaskCompletion),
					Pair(OnProjectCompetion, Messages.OnProjectCompetion),
				}) {}
		}

		public const string OnBilling = "B";
        public const string OnTaskCompletion = "T";
        public const string OnProjectCompetion = "P";

		public class onBilling : PX.Data.BQL.BqlString.Constant<onBilling>
		{
			public onBilling() : base(OnBilling) {}
		}
		public class onTaskCompletion : PX.Data.BQL.BqlString.Constant<onTaskCompletion>
		{
			public onTaskCompletion() : base(OnTaskCompletion) {}
		}
		public class onProjectCompetion : PX.Data.BQL.BqlString.Constant<onProjectCompetion>
		{
			public onProjectCompetion() : base(OnProjectCompetion) {}
		}
    }

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public static class PMCompletedPctMethod
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Manual, Messages.Manual),
					Pair(ByQuantity, Messages.ByQuantity),
					Pair(ByAmount, Messages.ByAmount),
				}) {}
		}

		public const string Manual = "M";
		public const string ByQuantity = "Q";
		public const string ByAmount = "A";

		public class manual : PX.Data.BQL.BqlString.Constant<manual>
		{
			public manual() : base(Manual) { ;}
		}

		public class byQuantity : PX.Data.BQL.BqlString.Constant<byQuantity>
		{
			public byQuantity() : base(ByQuantity) { ;}
		}

		public class byAmount : PX.Data.BQL.BqlString.Constant<byAmount>
		{
			public byAmount() : base(ByAmount) { ;}
		}
	}
}
