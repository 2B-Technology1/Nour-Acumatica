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
using PX.Objects.AR;
using PX.Objects.AR.MigrationMode;
using PX.Objects.CS;
using PX.Objects.GL;
using System;
using System.Collections;
using PX.Objects.PM.Billing;

namespace PX.Objects.PM
{
	[GL.TableDashboardType]
	public class BillingProcess : PXGraph<BillingProcess>
	{
		public PXCancel<BillingFilter> Cancel;
		public PXFilter<BillingFilter> Filter;
		public PXFilteredProcessing<Contract, BillingFilter> Items;
		public PXSetup<PMSetup> Setup;
		public PXAction<BillingFilter> viewDocumentProject;

		public PXSelectJoinGroupBy<Contract,
			   InnerJoin<PMUnbilledDailySummary, On<PMUnbilledDailySummary.projectID, Equal<Contract.contractID>>,
			   InnerJoin<CT.ContractBillingSchedule, On<Contract.contractID, Equal<CT.ContractBillingSchedule.contractID>>,
			   InnerJoin<Customer, On<Contract.customerID, Equal<Customer.bAccountID>>,
			   InnerJoin<PMTask, On<PMTask.projectID, Equal<PMUnbilledDailySummary.projectID>,
				   And<PMTask.isActive, Equal<True>,
				   And<PMTask.taskID, Equal<PMUnbilledDailySummary.taskID>,
				   And<Where<PMTask.billingOption, Equal<PMBillingOption.onBilling>,
				   Or2<Where<PMTask.billingOption, Equal<PMBillingOption.onTaskCompletion>, And<PMTask.isCompleted, Equal<True>>>,
				   Or<Where<PMTask.billingOption, Equal<PMBillingOption.onProjectCompetion>, And<Contract.isCompleted, Equal<True>>>>>>>>>>,
			   InnerJoin<PMBillingRule, On<PMBillingRule.billingID, Equal<PMTask.billingID>,
				   And<PMBillingRule.accountGroupID, Equal<PMUnbilledDailySummary.accountGroupID>>>>>>>>,
			   Where2<Where<CT.ContractBillingSchedule.nextDate, LessEqual<Current<BillingFilter.invoiceDate>>,
				   Or<CT.ContractBillingSchedule.type, Equal<CT.BillingType.BillingOnDemand>>>,
			   And2<Where<PMBillingRule.includeNonBillable, Equal<False>, And<PMUnbilledDailySummary.billable, Greater<int0>,
				   Or<Where<PMBillingRule.includeNonBillable, Equal<True>, And<Where<PMUnbilledDailySummary.nonBillable, Greater<int0>, Or<PMUnbilledDailySummary.billable, Greater<int0>>>>>>>>,
				And2<Where<PMUnbilledDailySummary.date, LessEqual<Current<BillingFilter.invoiceDate>>>, And<Match<Current<AccessInfo.userName>>>>>>,
				Aggregate<GroupBy<Contract.contractID>>> ProjectsUnbilled;
		public PXSelectJoinGroupBy<Contract,
			   InnerJoin<PMUnbilledDailySummary, On<PMUnbilledDailySummary.projectID, Equal<Contract.contractID>>,
			   InnerJoin<CT.ContractBillingSchedule, On<Contract.contractID, Equal<CT.ContractBillingSchedule.contractID>>,
			   InnerJoin<Customer, On<Contract.customerID, Equal<Customer.bAccountID>>,
			   InnerJoin<PMTask, On<PMTask.projectID, Equal<PMUnbilledDailySummary.projectID>,
				   And<PMTask.isActive, Equal<True>,
				   And<PMTask.taskID, Equal<PMUnbilledDailySummary.taskID>,
				   And<Where<PMTask.billingOption, Equal<PMBillingOption.onBilling>,
				   Or2<Where<PMTask.billingOption, Equal<PMBillingOption.onTaskCompletion>, And<PMTask.isCompleted, Equal<True>>>,
				   Or<Where<PMTask.billingOption, Equal<PMBillingOption.onProjectCompetion>, And<Contract.isCompleted, Equal<True>>>>>>>>>>,
			   InnerJoin<PMBillingRule, On<PMBillingRule.billingID, Equal<PMTask.billingID>,
				   And<PMBillingRule.accountGroupID, Equal<PMUnbilledDailySummary.accountGroupID>>>>>>>>,
			   Where2<Where<CT.ContractBillingSchedule.nextDate, LessEqual<Current<BillingFilter.invoiceDate>>,
				   Or<CT.ContractBillingSchedule.type, Equal<CT.BillingType.BillingOnDemand>>>,
			   And2<Where<PMBillingRule.includeNonBillable, Equal<False>, And<PMUnbilledDailySummary.billable, Greater<int0>,
				   Or<Where<PMBillingRule.includeNonBillable, Equal<True>, And<Where<PMUnbilledDailySummary.nonBillable, Greater<int0>, Or<PMUnbilledDailySummary.billable, Greater<int0>>>>>>>>,
				And2<Where<PMUnbilledDailySummary.date, Less<Current<BillingFilter.invoiceDate>>>, And<Match<Current<AccessInfo.userName>>>>>>,
				Aggregate<GroupBy<Contract.contractID>>> ProjectsUbilledCutOffDateExcluded;
		public PXSelectJoinGroupBy<Contract,
			   InnerJoin<CT.ContractBillingSchedule, On<Contract.contractID, Equal<CT.ContractBillingSchedule.contractID>>,
			   InnerJoin<Customer, On<Contract.customerID, Equal<Customer.bAccountID>>,
			   InnerJoin<PMTask, On<PMTask.projectID, Equal<Contract.contractID>>,
			   InnerJoin<PMBillingRule, On<PMBillingRule.billingID, Equal<PMTask.billingID>>,
			   InnerJoin<PMRecurringItem, On<PMTask.projectID, Equal<PMRecurringItem.projectID>,
					And<PMTask.taskID, Equal<PMRecurringItem.taskID>,
					And<PMTask.isCompleted, Equal<False>>>>>>>>>,
			  Where2<Where<CT.ContractBillingSchedule.nextDate, LessEqual<Current<BillingFilter.invoiceDate>>,
				   Or<CT.ContractBillingSchedule.type, Equal<CT.BillingType.BillingOnDemand>>>,
				And<Match<Current<AccessInfo.userName>>>>,
				Aggregate<GroupBy<Contract.contractID>>> ProjectsRecurring;
		public PXSelectJoinGroupBy<Contract,
			   InnerJoin<CT.ContractBillingSchedule, On<Contract.contractID, Equal<CT.ContractBillingSchedule.contractID>>,
			   InnerJoin<Customer, On<Contract.customerID, Equal<Customer.bAccountID>>,
			   InnerJoin<PMTask, On<PMTask.projectID, Equal<Contract.contractID>>,
			   InnerJoin<PMBillingRule, On<PMBillingRule.billingID, Equal<PMTask.billingID>>,
			   InnerJoin<PMBudget, On<PMTask.projectID, Equal<PMBudget.projectID>,
					And<PMTask.taskID, Equal<PMBudget.projectTaskID>,
					And<PMBudget.type, Equal<GL.AccountType.income>,
					And<PMBudget.curyAmountToInvoice, NotEqual<decimal0>>>>>>>>>>,
			   Where<Match<Current<AccessInfo.userName>>>,
				Aggregate<GroupBy<Contract.contractID>>> ProjectsProgressive;

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXEditDetailButton]
		public virtual IEnumerable ViewDocumentProject(PXAdapter adapter)
		{
			if (Items.Current != null)
			{
				var service = PXGraph.CreateInstance<PM.ProjectAccountingService>();
				service.NavigateToProjectScreen(Items.Current.ContractID, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		protected virtual IEnumerable items()
		{
			BillingFilter filter = Filter.Current;
			if (filter == null)
			{
				yield break;
			}
			bool found = false;
			foreach (Contract item in Items.Cache.Inserted)
			{
				found = true;
				yield return item;
			}
			if (found)
				yield break;

			PXSelectBase<Contract> selectUnbilled = ProjectsUnbilled;

			if (Setup.Current.CutoffDate == PMCutOffDate.Excluded)
			{
				selectUnbilled = ProjectsUbilledCutOffDateExcluded;
			}
			if (filter.StatementCycleId != null)
			{
				selectUnbilled.WhereAnd<Where<Customer.statementCycleId, Equal<Current<BillingFilter.statementCycleId>>>>();
				ProjectsRecurring.WhereAnd<Where<Customer.statementCycleId, Equal<Current<BillingFilter.statementCycleId>>>>();
				ProjectsProgressive.WhereAnd<Where<Customer.statementCycleId, Equal<Current<BillingFilter.statementCycleId>>>>();
			}
			if (filter.CustomerClassID != null)
			{
				selectUnbilled.WhereAnd<Where<Customer.customerClassID, Equal<Current<BillingFilter.customerClassID>>>>();
				ProjectsRecurring.WhereAnd<Where<Customer.customerClassID, Equal<Current<BillingFilter.customerClassID>>>>();
				ProjectsProgressive.WhereAnd<Where<Customer.customerClassID, Equal<Current<BillingFilter.customerClassID>>>>();
			}
			if (filter.CustomerID != null)
			{
				selectUnbilled.WhereAnd<Where<Customer.bAccountID, Equal<Current<BillingFilter.customerID>>>>();
				ProjectsRecurring.WhereAnd<Where<Customer.bAccountID, Equal<Current<BillingFilter.customerID>>>>();
				ProjectsProgressive.WhereAnd<Where<Customer.bAccountID, Equal<Current<BillingFilter.customerID>>>>();
			}
			if (filter.TemplateID != null)
			{
				selectUnbilled.WhereAnd<Where<Contract.templateID, Equal<Current<BillingFilter.templateID>>>>();
				ProjectsRecurring.WhereAnd<Where<Contract.templateID, Equal<Current<BillingFilter.templateID>>>>();
				ProjectsProgressive.WhereAnd<Where<Contract.templateID, Equal<Current<BillingFilter.templateID>>>>();
			}

			foreach (PXResult item in selectUnbilled.Select())
			{
				var result = CreateListItem(item);

				if (Items.Locate(result) == null)
					yield return Items.Insert(result);
			}

			foreach (PXResult item in ProjectsRecurring.Select())
			{
				var result = CreateListItem(item);

				if (Items.Locate(result) == null)
					yield return Items.Insert(result);
			}

			foreach (PXResult item in ProjectsProgressive.Select())
			{
				var result = CreateListItem(item);

				if (Items.Locate(result) == null)
					yield return Items.Insert(result);
			}

			Items.Cache.IsDirty = false;
		}

		protected virtual Contract CreateListItem(PXResult item)
		{
			Contract project = PXResult.Unwrap<Contract>(item);
			CT.ContractBillingSchedule schedule = PXResult.Unwrap<CT.ContractBillingSchedule>(item);
			Customer customer = PXResult.Unwrap<Customer>(item);

			project.LastDate = schedule.LastDate;

			DateTime? fromDate = null;

			if (schedule.NextDate != null)
			{
				switch (schedule.Type)
				{
					case CT.BillingType.Annual:
						fromDate = schedule.NextDate.Value.AddYears(-1);
						break;
					case CT.BillingType.Monthly:
						fromDate = schedule.NextDate.Value.AddMonths(-1);
						break;
					case CT.BillingType.Weekly:
						fromDate = schedule.NextDate.Value.AddDays(-7);
						break;
					case CT.BillingType.Quarterly:
						fromDate = schedule.NextDate.Value.AddMonths(-3);
						break;
				}
			}

			project.FromDate = fromDate;
			project.NextDate = schedule.NextDate;

			return project;
		}

		public BillingProcess()
		{
			ARSetupNoMigrationMode.EnsureMigrationModeDisabled(this);

			Items.SetProcessCaption(PM.Messages.Process);
			Items.SetProcessAllCaption(PM.Messages.ProcessAll);
			PeriodValidation validationValue = this.IsContractBasedAPI ||
				this.IsImport || this.IsExport || this.UnattendedMode ? PeriodValidation.DefaultUpdate : PeriodValidation.DefaultSelectUpdate;
			OpenPeriodAttribute.SetValidatePeriod<BillingFilter.invFinPeriodID>(Filter.Cache, null, validationValue);
		}

		#region EventHandlers
		protected virtual void BillingFilter_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			if (!cache.ObjectsEqual<BillingFilter.invoiceDate, BillingFilter.invFinPeriodID, BillingFilter.statementCycleId, BillingFilter.customerClassID, BillingFilter.customerID, BillingFilter.templateID>(e.Row, e.OldRow))
				Items.Cache.Clear();
		}
		protected virtual void BillingFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			BillingFilter filter = Filter.Current;

			Items.SetProcessDelegate<PMBillEngine>(
					delegate (PMBillEngine engine, Contract item)
						{
							engine.Clear();
							if (engine.Bill(item.ContractID, filter.InvoiceDate, filter.InvFinPeriodID).IsEmpty)
							{
								throw new PXSetPropertyException(Warnings.NothingToBill, PXErrorLevel.RowWarning);
							}
						});
		}
		#endregion

		[PXHidden]
		[Serializable]
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public partial class BillingFilter : IBqlTable
		{
			#region InvoiceDate
			public abstract class invoiceDate : PX.Data.BQL.BqlDateTime.Field<invoiceDate> { }
			protected DateTime? _InvoiceDate;
			[PXDBDate()]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "Invoice Date", Visibility = PXUIVisibility.Visible, Required = false)]
			public virtual DateTime? InvoiceDate
			{
				get
				{
					return this._InvoiceDate;
				}
				set
				{
					this._InvoiceDate = value;
				}
			}
			#endregion

			#region InvFinPeriodID
			public abstract class invFinPeriodID : PX.Data.BQL.BqlString.Field<invFinPeriodID> { }
			protected string _InvFinPeriodID;
			[OpenPeriod(typeof(BillingFilter.invoiceDate),
				ValidatePeriod = PeriodValidation.DefaultSelectUpdate )]
			[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.Visible, Required = false)]
			public virtual String InvFinPeriodID
			{
				get
				{
					return this._InvFinPeriodID;
				}
				set
				{
					this._InvFinPeriodID = value;
				}
			}
			#endregion

			#region StatementCycleId
			public abstract class statementCycleId : PX.Data.BQL.BqlString.Field<statementCycleId> { }
			protected String _StatementCycleId;
			[PXDBString(10, IsUnicode = true)]
			[PXUIField(DisplayName = "Statement Cycle")]
			[PXSelector(typeof(ARStatementCycle.statementCycleId))]
			public virtual String StatementCycleId
			{
				get
				{
					return this._StatementCycleId;
				}
				set
				{
					this._StatementCycleId = value;
				}
			}
			#endregion

			#region CustomerClassID
			public abstract class customerClassID : PX.Data.BQL.BqlString.Field<customerClassID> { }
			protected String _CustomerClassID;
			[PXDBString(10, IsUnicode = true)]
			[PXSelector(typeof(CustomerClass.customerClassID), DescriptionField = typeof(CustomerClass.descr), CacheGlobal = true)]
			[PXUIField(DisplayName = "Customer Class")]
			public virtual String CustomerClassID
			{
				get
				{
					return this._CustomerClassID;
				}
				set
				{
					this._CustomerClassID = value;
				}
			}
			#endregion

			#region CustomerID
			public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
			protected Int32? _CustomerID;
			[PXUIField(DisplayName = "Customer")]
			[Customer(DescriptionField = typeof(Customer.acctName))]
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

			#region TemplateID
			public abstract class templateID : PX.Data.BQL.BqlInt.Field<templateID> { }
			protected Int32? _TemplateID;
			[Project(typeof(Where<PMProject.baseType, Equal<CT.CTPRType.projectTemplate>>), DisplayName = "Project Template")]
			public virtual Int32? TemplateID
			{
				get
				{
					return this._TemplateID;
				}
				set
				{
					this._TemplateID = value;
				}
			}
			#endregion
		}
	}
}

namespace PX.Objects.PM.Billing
{
	[PXHidden]
	[Serializable]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public partial class Contract : IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		[PXBool]
		[PXUnboundDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public bool? Selected
		{
			get;
			set;
		}
		#endregion

		#region ContractID
		public abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }
		[PXDBInt(IsKey = true)]
		public virtual int? ContractID
		{
			get;
			set;
		}


		#endregion

		#region ContractCD
		public abstract class contractCD : PX.Data.BQL.BqlString.Field<contractCD> { }
		[PXDBString]
		[PXUIField(DisplayName = "Project")]
		public virtual string ContractCD
		{
			get;
			set;
		}


		#endregion

		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		[PXDBInt()]
		[PXUIField(DisplayName = "Customer", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Customer.bAccountID), SubstituteKey = typeof(Customer.acctCD), DescriptionField = typeof(Customer.acctName))]
		public virtual Int32? CustomerID
		{
			get;
			set;
		}
		#endregion
		#region TemplateID
		public abstract class templateID : PX.Data.BQL.BqlInt.Field<templateID> { }

		[PXDBInt]
		public virtual Int32? TemplateID
		{
			get;
			set;
		}
		#endregion
		#region DefaultBranchID
		public abstract class defaultBranchID : PX.Data.BQL.BqlInt.Field<defaultBranchID> { }
		
		[Branch(DisplayName = "Branch", IsDetail = true, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? DefaultBranchID
		{
			get;
			set;
		}
		#endregion

		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		[PXDBString(60, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Description
		{
			get;
			set;
		}
		#endregion

		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? IsActive
		{
			get;
			set;
		}
		#endregion
		#region IsCompleted
		public abstract class isCompleted : PX.Data.BQL.BqlBool.Field<isCompleted> { }
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? IsCompleted
		{
			get;
			set;
		}
		#endregion
		#region GroupMask
		public abstract class groupMask : PX.Data.BQL.BqlByteArray.Field<groupMask> { }
		protected Byte[] _GroupMask;
		[PXDBGroupMask()]
		public virtual Byte[] GroupMask
		{
			get
			{
				return this._GroupMask;
			}
			set
			{
				this._GroupMask = value;
			}
		}
		#endregion


		#region LastDate
		public abstract class lastDate : PX.Data.BQL.BqlDateTime.Field<lastDate> { }
		[PXDate()]
		[PXUIField(DisplayName = "Last Billing Date", Enabled = false)]
		public virtual DateTime? LastDate
		{
			get;
			set;
		}
		#endregion
		#region FromDate
		public abstract class fromDate : PX.Data.BQL.BqlDateTime.Field<fromDate> { }
		[PXDate()]
		[PXUIField(DisplayName = "From")]
		public virtual DateTime? FromDate
		{
			get;
			set;
		}
		#endregion
		#region NextDate
		public abstract class nextDate : PX.Data.BQL.BqlDateTime.Field<nextDate> { }
		[PXDate()]
		[PXUIField(DisplayName = "To")]
		public virtual DateTime? NextDate
		{
			get;
			set;
		}
		#endregion		

		#region NoteID
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
				
		[PXNote(DescriptionField = typeof(contractCD))]
		public virtual Guid? NoteID
		{
			get;
			set;
			
		}
		#endregion
	}
}
