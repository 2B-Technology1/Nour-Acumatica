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
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.PM;

namespace PX.Objects.FS
 {
	[System.SerializableAttribute]
	[PXCacheName(TX.TableName.SERVICE_CONTRACT)]
	[PXPrimaryGraph(
		   new Type[] {
					typeof(ServiceContractEntry),
					typeof(RouteServiceContractEntry)
		   },
		   new Type[] {
					typeof(Where<FSServiceContract.recordType, Equal<recordType.ServiceContract>>),
					typeof(Where<FSServiceContract.recordType, Equal<recordType.RouteServiceContract>>)
		   })]
	[PXGroupMask(typeof(InnerJoinSingleTable<Customer, On<Customer.bAccountID, Equal<FSServiceContract.customerID>, And<Match<Customer, Current<AccessInfo.userName>>>>>))]
	public class FSServiceContract : PX.Data.IBqlTable, INotable
	{
		#region Keys
		public class PK : PrimaryKeyOf<FSServiceContract>.By<serviceContractID>
		{
			public static FSServiceContract Find(PXGraph graph, int? serviceContractID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, serviceContractID, options);
		}

		public class UK : PrimaryKeyOf<FSServiceContract>.By<refNbr>
		{
			public static FSServiceContract Find(PXGraph graph, string refNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, refNbr, options);
		}

		public static class FK
		{
			public class Branch : GL.Branch.PK.ForeignKeyOf<FSServiceContract>.By<branchID> { }
			public class Customer : AR.Customer.PK.ForeignKeyOf<FSServiceContract>.By<customerID> { }
			public class CustomerLocation : Location.PK.ForeignKeyOf<FSServiceContract>.By<customerID, customerLocationID> { }
			public class BillCustomer : AR.Customer.PK.ForeignKeyOf<FSServiceContract>.By<billCustomerID> { }
			public class BillCustomerLocation : Location.PK.ForeignKeyOf<FSServiceContract>.By<billCustomerID, billLocationID> { }
			public class MasterContract : FSMasterContract.PK.ForeignKeyOf<FSServiceContract>.By<masterContractID> { }
			public class Vendor : AP.Vendor.PK.ForeignKeyOf<FSServiceContract>.By<vendorID> { }
			public class SalesPerson : AR.SalesPerson.PK.ForeignKeyOf<FSServiceContract>.By<salesPersonID> { }
			public class Project : PMProject.PK.ForeignKeyOf<FSServiceContract>.By<projectID> { }
			public class DefaultTask : PMTask.PK.ForeignKeyOf<FSServiceContract>.By<projectID, dfltProjectTaskID> { }
		}
		#endregion

		#region RefNbr
		public abstract class refNbr : PX.Data.IBqlField
		{
		}

		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Service Contract ID", Visibility = PXUIVisibility.SelectorVisible, Visible = true, Enabled = true)]
		[AutoNumber(typeof(Search<FSSetup.serviceContractNumberingID>), typeof(AccessInfo.businessDate))]
		[FSSelectorContractRefNbrAttribute(typeof(ListField_RecordType_ContractSchedule.ServiceContract))]
		[PX.Data.EP.PXFieldDescription]

		public virtual string RefNbr { get; set; }
		#endregion
		#region CustomerContractNbr
		public abstract class customerContractNbr : PX.Data.IBqlField
		{
		}

		//Included in FSRouteContractScheduleFSServiceContract projection
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Customer Contract Nbr.", Visibility = PXUIVisibility.SelectorVisible, Visible = true, Enabled = false)]
		[FSSelectorCustomerContractNbrAttributeAttribute(typeof(ListField_RecordType_ContractSchedule.ServiceContract), typeof(FSServiceContract.customerID))]
		[ServiceContractAutoNumber]
		public virtual string CustomerContractNbr { get; set; }
		#endregion
		#region CustomerID
		public abstract class customerID : PX.Data.IBqlField
		{
		}

		[PXDBInt]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Customer", Visibility = PXUIVisibility.SelectorVisible)]
		[FSSelectorBAccountTypeCustomerOrCombined]
		[PXRestrictor(typeof(Where<Customer.status, IsNull,
			   Or<Customer.status, Equal<CustomerStatus.active>,
			   Or<Customer.status, Equal<CustomerStatus.oneTime>>>>),
			   PX.Objects.AR.Messages.CustomerIsInStatus, typeof(Customer.status))]
		[PX.Data.EP.PXFieldDescription]
		public virtual int? CustomerID { get; set; }
		#endregion
		#region ServiceContractID
		public abstract class serviceContractID : PX.Data.IBqlField
		{
		}

		[PXDBIdentity]
		public virtual int? ServiceContractID { get; set; }
		#endregion
		#region Attributes
		/// <summary>
		/// A service field, which is necessary for the <see cref="CSAnswers">dynamically 
		/// added attributes</see> defined at the <see cref="FSServiceContract">customer 
		/// class</see> level to function correctly.
		/// </summary>
		[CRAttributesField(typeof(FSServiceContract.classID))]
		public virtual string[] Attributes { get; set; }

		public abstract class classID : IBqlField
		{
		}
		[PXString(20)]
		public string ClassID
		{
			get { return GroupTypes.ListAttribute.ServiceContract; }
		}
		#endregion
		#region BillingPeriod
		public abstract class billingPeriod : ListField_Contract_BillingPeriod
		{
		}

        [PXDBString(1, IsUnicode = false)]
		[billingPeriod.ListAtrribute]
		[PXDefault(ID.Contract_BillingPeriod.MONTH)]
		[PXUIField(DisplayName = "Period")]
		public virtual string BillingPeriod { get; set; }
		#endregion
		#region BillingType
		public abstract class billingType : PX.Data.BQL.BqlString.Field<billingType>
		{
			public abstract class Values : ListField.ServiceContractBillingType { }
		}

        [PXDBString(4, IsUnicode = false)]
		[PXDefault(billingType.Values.PerformedBillings)]
		[billingType.Values.List]
		[PXUIField(DisplayName = "Billing Type")]
		public virtual string BillingType { get; set; }
		#endregion
		#region BillTo
		public abstract class billTo : ListField_Contract_BillTo
		{
		}

        [PXDBString(1, IsUnicode = false)]
		[PXDefault(ID.Contract_BillTo.CUSTOMERACCT)]
		[billTo.ListAtrribute]
		[PXUIField(DisplayName = "Bill To")]
		public virtual string BillTo { get; set; }
		#endregion
		#region BillCustomerID
		public abstract class billCustomerID : PX.Data.IBqlField
		{
		}

		[PXDBInt]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Billing Customer", Enabled = false)]
		[FSSelectorBAccountTypeCustomerOrCombined]
		[PXRestrictor(typeof(Where<Customer.status, IsNull,
			   Or<Customer.status, Equal<CustomerStatus.active>,
			   Or<Customer.status, Equal<CustomerStatus.oneTime>>>>),
			   PX.Objects.AR.Messages.CustomerIsInStatus, typeof(Customer.status))]
		public virtual int? BillCustomerID { get; set; }
		#endregion
		#region BillLocationID
		public abstract class billLocationID : PX.Data.IBqlField
		{
		}

		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [FSLocationActive(typeof(Where<Location.bAccountID, Equal<Current<FSServiceContract.billCustomerID>>>),
					DescriptionField = typeof(Location.descr), DisplayName = "Billing Location", DirtyRead = true, Enabled = false)]
		[PXForeignReference(typeof(FK.BillCustomerLocation))]
		public virtual int? BillLocationID { get; set; }
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.IBqlField
		{
		}

		[PXDBInt]
		[PXDefault(typeof(AccessInfo.branchID), PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Branch", TabOrder = 0)]
		[PXSelector(typeof(Branch.branchID), SubstituteKey = typeof(Branch.branchCD), DescriptionField = typeof(Branch.acctName))]
		public virtual int? BranchID { get; set; }
		#endregion
		#region BranchLocationID
		public abstract class branchLocationID : PX.Data.IBqlField
		{
		}

		[PXDBInt]
		[PXDefault(typeof(
			Search<FSxUserPreferences.dfltBranchLocationID,
			Where<
				PX.SM.UserPreferences.userID, Equal<CurrentValue<AccessInfo.userID>>,
				And<PX.SM.UserPreferences.defBranchID, Equal<Current<FSServiceContract.branchID>>>>>))]
		[PXUIField(DisplayName = "Branch Location")]
		[PXSelector(typeof(Search<FSBranchLocation.branchLocationID,
					Where<FSBranchLocation.branchID, Equal<Current<FSServiceContract.branchID>>>>),
					SubstituteKey = typeof(FSBranchLocation.branchLocationCD),
					DescriptionField = typeof(FSBranchLocation.descr))]
		public virtual int? BranchLocationID { get; set; }
		#endregion
		#region CustomerLocationID
		public abstract class customerLocationID : PX.Data.IBqlField
		{
		}

		//Included in FSRouteContractScheduleFSServiceContract projection
		[PXDefault]
        [FSLocationActive(typeof(Where<Location.bAccountID, Equal<Current<FSServiceContract.customerID>>>),
					DescriptionField = typeof(Location.descr), DisplayName = "Location", DirtyRead = true)]
		[PXForeignReference(typeof(FK.CustomerLocation))]
		public virtual int? CustomerLocationID { get; set; }
		#endregion
		#region DocDesc
		public abstract class docDesc : PX.Data.IBqlField
		{
		}

		//Included in FSRouteContractScheduleFSServiceContract projection
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual string DocDesc { get; set; }
		#endregion
		#region ExpirationType
		public abstract class expirationType : ListField_Contract_ExpirationType
		{
		}

        [PXDBString(1, IsUnicode = false)]
		[PXDefault(ID.Contract_ExpirationType.UNLIMITED)]
		[expirationType.ListAtrribute]
		[PXUIField(DisplayName = "Expiration Type")]
		public virtual string ExpirationType { get; set; }
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.IBqlField
		{
		}

		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Start Date")]

		public virtual DateTime? StartDate { get; set; }
		#endregion

		#region DurationType
		public abstract class durationType : SC_Duration_Type { }

		[PXDBString]
		[PXUIField(DisplayName = "Duration Type")]
		[PXDefault(durationType.YEAR)]
		[durationType.ListAtrribute]
		public virtual string DurationType { get; set; }
		#endregion
		#region RenewalDate
		public abstract class renewalDate : PX.Data.IBqlField
		{
		}

		[PXUIField(DisplayName = "Renewal Date", Enabled = false)]
		[PXDBDate]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual DateTime? RenewalDate { get; set; }
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.IBqlField
		{
		}

		[PXUIField(DisplayName = "Expiration Date", Enabled = false)]
		[PXDBDate]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<durationType, expirationType, startDate>))]
		public virtual DateTime? EndDate { get; set; }
		#endregion
		#region Duration
		public abstract class duration : PX.Data.BQL.BqlInt.Field<duration> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Duration")]
		[PXDefault(1, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<durationType, expirationType, startDate>))]
		public virtual int? Duration { get; set; }
		#endregion
		
		#region LastBillingInvoiceDate 
		public abstract class lastBillingInvoiceDate : PX.Data.IBqlField
        {
        }

        [PXDBDate]
        [PXUIField(DisplayName = "Last Billing Date", Enabled = false)]
        public virtual DateTime? LastBillingInvoiceDate { get; set; }
        #endregion
        #region MasterContractID
        public abstract class masterContractID : PX.Data.IBqlField
        {
        }

        [PXDBInt]
        [PXUIField(DisplayName = "Master Contract")]
        [PXSelector(typeof(
                        Search<FSMasterContract.masterContractID, 
                        Where<
                            FSMasterContract.customerID, Equal<Current<FSServiceContract.customerID>>>>),
                    SubstituteKey = typeof(FSMasterContract.masterContractCD), 
                    DescriptionField = typeof(FSMasterContract.descr))]
        public virtual int? MasterContractID { get; set; }
        #endregion
        #region NextBillingInvoiceDate 
        public abstract class nextBillingInvoiceDate : PX.Data.IBqlField
        {
        }

        [PXDBDate]
        [PXUIField(DisplayName = "Next Billing Date", Enabled = false)]
        public virtual DateTime? NextBillingInvoiceDate { get; set; }
        #endregion
        #region RecordType
        public abstract class recordType : ListField_RecordType_ContractSchedule
        {
        }

        [PXDBString(4, IsUnicode = false)]
        [PXDefault(ID.RecordType_ServiceContract.SERVICE_CONTRACT)]
        [recordType.ListAtrribute]
        public virtual string RecordType { get; set; }
        #endregion
        #region Status
        public abstract class status : ListField_Status_ServiceContract
        {
        }

        [PXDBString(1, IsFixed = true)]
        [PXDefault(ID.Status_ServiceContract.DRAFT)]
        [status.ListAtrribute]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual string Status { get; set; }
        #endregion
        #region StatusEffectiveFromDate
        public abstract class statusEffectiveFromDate : PX.Data.IBqlField
        {
        }

        [PXUIField(DisplayName = "Effective From Date", Enabled = false)]
        [PXDBDate]
        public virtual DateTime? StatusEffectiveFromDate { get; set; }
        #endregion
        #region StatusEffectiveUntilDate
        public abstract class statusEffectiveUntilDate : PX.Data.IBqlField
        {
        }

        [PXUIField(DisplayName = "Effective Until Date", Enabled = false)]
        [PXDBDate]
        public virtual DateTime? StatusEffectiveUntilDate { get; set; }
        #endregion
        #region UpcomingStatus
        public abstract class upcomingStatus : ListField_Status_ServiceContract
        {
        }

        [PXDBString(1, IsFixed = true)]
        [upcomingStatus.ListAtrribute]
        [PXUIField(DisplayName = "Upcoming Status", Enabled = false)]
        public virtual string UpcomingStatus { get; set; }
        #endregion
        #region CustomerContactID
        public abstract class customerContactID : PX.Data.BQL.BqlInt.Field<customerContactID> { }

        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Contact")]
        [FSSelectorContact(typeof(customerID))]
        [PXFormula(typeof(Default<customerID>))]
        public virtual int? CustomerContactID { get; set; }
        #endregion
        #region VendorID
        public abstract class vendorID : PX.Data.IBqlField
        {
        }

        [PXDBInt]
        [PXUIField(DisplayName = "Vendor", Visibility = PXUIVisibility.SelectorVisible)]
        [FSSelectorVendor]
        public virtual int? VendorID { get; set; }
        #endregion
        #region SourcePrice
        public abstract class sourcePrice : ListField_Contract_SourcePrice
        {
        }

        [PXDBString(1, IsFixed = true)]
        [PXDefault(ID.SourcePrice.PRICE_LIST)]
        [sourcePrice.ListAtrribute]
        [PXUIField(DisplayName = "Take Prices From", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string SourcePrice { get; set; }
        #endregion
        #region SalesPersonID
        public abstract class salesPersonID : PX.Data.IBqlField
        {
        }

        [SalesPerson(DisplayName = "Salesperson ID")]
        [PXDefault(typeof(Search<CustDefSalesPeople.salesPersonID,
                          Where<CustDefSalesPeople.bAccountID, Equal<Current<FSServiceContract.customerID>>,
                          And<CustDefSalesPeople.locationID, Equal<Current<FSServiceContract.customerLocationID>>,
                          And<CustDefSalesPeople.isDefault, Equal<True>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<FSServiceContract.customerID>))]
        [PXFormula(typeof(Default<FSServiceContract.customerLocationID>))]
        public virtual int? SalesPersonID { get; set; }
        #endregion
        #region Commissionable
        public abstract class commissionable : PX.Data.IBqlField
        {
        }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Commissionable")]
        public virtual bool? Commissionable { get; set; }
        #endregion
        #region ScheduleGenType
        public abstract class scheduleGenType : ListField_ScheduleGenType_ContractSchedule
        {
        }

        [PXDBString(2, IsUnicode = false)]
        [scheduleGenType.ListAtrribute]
        [PXUIField(DisplayName = "Schedule Generation Type")]
        [PXFormula(typeof(Default<billingType>))]
        public virtual string ScheduleGenType { get; set; }
        #endregion

        #region ProjectID
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

        [ProjectDefault]
        [PXRestrictor(typeof(Where<PMProject.isActive, Equal<True>>), PM.Messages.InactiveContract, typeof(PMProject.contractCD))]
        [PXRestrictor(typeof(Where<PMProject.isCancelled, Equal<False>>), PM.Messages.CancelledContract, typeof(PMProject.contractCD))]
        [ProjectBase(typeof(FSServiceContract.customerID), Filterable = true)]
        [PXForeignReference(typeof(FK.Project))]
        public virtual int? ProjectID { get; set; }
        #endregion
        #region DfltProjectTaskID
        public abstract class dfltProjectTaskID : PX.Data.BQL.BqlInt.Field<dfltProjectTaskID> { }

        [PXDefault(typeof(Search<PMTask.taskID,
                        Where<PMTask.projectID, Equal<Current<projectID>>,
                        And<PMTask.isDefault, Equal<True>,
                        And<Where<PMTask.status,
                            Equal<ProjectTaskStatus.active>, Or<PMTask.status, Equal<ProjectTaskStatus.planned>>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [ActiveOrInPlanningProjectTask(typeof(FSServiceContract.projectID), DisplayName = "Default Project Task", DescriptionField = typeof(PMTask.description), Enabled = false)]
        [PXForeignReference(typeof(FK.DefaultTask))]
        public virtual int? DfltProjectTaskID { get; set; }
		#endregion
		#region DfltCostCodeID
		public abstract class dfltCostCodeID : PX.Data.BQL.BqlInt.Field<dfltCostCodeID> { }

		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[CostCode(DisplayName = "Default Cost Code", Filterable = false, SkipVerification = true)]
		[PXForeignReference(typeof(Field<dfltCostCodeID>.IsRelatedTo<PMCostCode.costCodeID>))]
		public virtual int? DfltCostCodeID { get; set; }
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID>
        {
        }

        [PXUIField(DisplayName = "NoteID")]
        [PXNote(ShowInReferenceSelector=true)]
        [PXSearchable(SM.SearchCategory.FS, "SM: {0} - {2}", new Type[] { typeof(FSServiceContract.refNbr), typeof(FSServiceContract.customerID), typeof(Customer.acctName) },
           new Type[] { typeof(Customer.acctCD), typeof(FSServiceContract.docDesc) },
           NumberFields = new Type[] { typeof(FSServiceContract.refNbr) },
           Line1Format = "{0:d}{1}{2}", Line1Fields = new Type[] { typeof(FSServiceContract.startDate), typeof(FSServiceContract.status), typeof(FSServiceContract.branchID) },
           Line2Format = "{0}", Line2Fields = new Type[] { typeof(FSServiceContract.docDesc) },
           MatchWithJoin = typeof(InnerJoin<Customer, On<Customer.bAccountID, Equal<FSServiceContract.customerID>>>),
           SelectForFastIndexing = typeof(Select2<FSServiceContract, InnerJoin<Customer, On<FSServiceContract.customerID, Equal<Customer.bAccountID>>>>)
        )]

        public virtual Guid? NoteID { get; set; }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.IBqlField
        {
        }

        [PXDBCreatedByID]
        [PXUIField(DisplayName = "Created By")]
        public virtual Guid? CreatedByID { get; set; }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.IBqlField
        {
        }

        [PXDBCreatedByScreenID]
        [PXUIField(DisplayName = "CreatedByScreenID")]
        public virtual string CreatedByScreenID { get; set; }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.IBqlField
        {
        }

        [PXDBCreatedDateTime]
        [PXUIField(DisplayName = "Created On")]
        public virtual DateTime? CreatedDateTime { get; set; }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.IBqlField
        {
        }

        [PXDBLastModifiedByID]
        [PXUIField(DisplayName = "Last Modified By")]
        public virtual Guid? LastModifiedByID { get; set; }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.IBqlField
        {
        }

        [PXDBLastModifiedByScreenID]
        [PXUIField(DisplayName = "LastModifiedByScreenID")]
        public virtual string LastModifiedByScreenID { get; set; }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.IBqlField
        {
        }

        [PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = "Last Modified On")]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.IBqlField
        {
        }

        [PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
        [PXUIField(DisplayName = "tstamp")]
        public virtual byte[] tstamp { get; set; }
        #endregion
        #region Selected
        public abstract class selected : IBqlField
        {
        }

        [PXBool]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected { get; set; }
        #endregion
        #region FixedRate
        public abstract class isFixedRateContract : IBqlField
        {
        }

        [PXBool]
        [PXDependsOnFields(typeof(billingType))]
        [PXFormula(typeof(IIf<Where<billingType, Equal<billingType.Values.fixedRateBillings>,
                                    Or<billingType, Equal<billingType.Values.fixedRateAsPerformedBillings>>>, True, False>))]
        [PXDBCalced(typeof(IIf<Where<billingType, Equal<billingType.Values.fixedRateBillings>,
                                    Or<billingType, Equal<billingType.Values.fixedRateAsPerformedBillings>>>, True, False>), typeof(bool))]
        public virtual bool? IsFixedRateContract { get; set; }
        #endregion

        #region ReportServiceContractID
        public abstract class reportServiceContractID : IBqlField
        {
        }

        [PXString]
        [PXSelector(typeof(Search<refNbr,
                           Where<
                                FSServiceContract.recordType, Equal<recordType.ServiceContract>,
                                And<FSServiceContract.customerID, Equal<Optional<FSServiceContract.customerID>>>>>),
                            new Type[]
                            {
                                typeof(FSServiceContract.refNbr),
                                typeof(FSServiceContract.customerContractNbr),
                                typeof(FSServiceContract.customerID),
                                typeof(FSServiceContract.status),
                                typeof(FSServiceContract.customerLocationID)
                            })]
        public virtual string ReportServiceContractID { get; set; }
        #endregion
        #region HasSchedule
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? HasSchedule { get; set; }
        #endregion
		#region HasProcessedSchedule
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? HasProcessedSchedule { get; set; }
		#endregion
		#region HasForecast
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? HasForecast { get; set; }
		#endregion
		#region Mem_ShowPriceTab
		public abstract class mem_ShowPriceTab : IBqlField
        {
        }

        [PXBool]
        public virtual bool? Mem_ShowPriceTab
        {
            get
            {
                return this.BillingType == FSServiceContract.billingType.Values.PerformedBillings;
            }
        }
        #endregion
        #region Mem_ShowScheduleTab
        public abstract class mem_ShowScheduleTab : IBqlField
        {
        }

        [PXBool]
        public virtual bool? Mem_ShowScheduleTab
        {
            get
            {
                return this.ScheduleGenType == ID.ScheduleGenType_ServiceContract.NONE;
            }
        }
        #endregion
        #region ShowInvoicesTab
        public abstract class showInvoicesTab : PX.Data.BQL.BqlBool.Field<showInvoicesTab> { }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(Visible = false)]
        public virtual bool? ShowInvoicesTab { get; set; }
        #endregion
        #region UsageBillingCycleID
        public abstract class usageBillingCycleID : IBqlField
        {
        }

        [PXInt]
        [PXSelector(typeof(FSBillingCycle.billingCycleID), SubstituteKey = typeof(FSBillingCycle.billingCycleCD), DescriptionField = typeof(FSBillingCycle.descr))]
        [PXUIField(DisplayName = "Usage Billing Cycle", Enabled = false)]
        public virtual int? UsageBillingCycleID { get; set; }
        #endregion
        #region ActivePeriodID
        public abstract class activePeriodID : PX.Data.BQL.BqlInt.Field<activePeriodID> { }

        [PXInt]
        [PXDBScalar(typeof(Search<FSContractPeriod.contractPeriodID, Where<FSContractPeriod.serviceContractID, Equal<FSServiceContract.serviceContractID>,
                            And<FSContractPeriod.status, Equal<FSContractPeriod.status.Active>>>>))]
        public virtual int? ActivePeriodID { get; set; }
        #endregion

        #region FormCaptionDescription
        [PXString]
        [PXFormula(typeof(Selector<customerID, Customer.acctName>))]
        public string FormCaptionDescription { get; set; }
		#endregion
		#region OrigServiceContractRefNbr
		public abstract class origServiceContractRefNbr : PX.Data.BQL.BqlString.Field<origServiceContractRefNbr> { }
		[PXString(15)]
		[PXUIField(DisplayName = "Orig Service Contract ID", Enabled = false)]
		public virtual string OrigServiceContractRefNbr { get; set; }
		#endregion

		#region EmailNotificationCD
		public abstract class emailNotificationCD : PX.Data.BQL.BqlString.Field<emailNotificationCD> { }
		[PXString(30)]
		public virtual string EmailNotificationCD { get; set; }
		#endregion

		public static bool TryParse(object row, out FSServiceContract fsServiceContractRow)
        {
            fsServiceContractRow = null;

            if (row is FSServiceContract)
            {
                fsServiceContractRow = (FSServiceContract)row;
                return true;
            }

            return false;
        }
        public bool isEditable()
        {
            return this.Status == ID.Status_ServiceContract.DRAFT || this.Status == ID.Status_ServiceContract.ACTIVE;
        }
    }
}
