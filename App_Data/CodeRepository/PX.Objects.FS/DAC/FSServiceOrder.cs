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
using PX.Data.WorkflowAPI;
using PX.Objects.AR;
using PX.Objects.CM.Extensions;
using PX.Objects.CR;
using PX.Objects.CR.MassProcess;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.TX;
using System;
using CRLocation = PX.Objects.CR.Standalone.Location;

namespace PX.Objects.FS
{
    [Serializable]
    [PXCacheName(TX.TableName.SERVICE_ORDER)]
    [PXPrimaryGraph(typeof(ServiceOrderEntry))]
    [PXGroupMask(typeof(LeftJoinSingleTable<Customer, On<Customer.bAccountID, Equal<FSServiceOrder.customerID>, And<Match<Customer, Current<AccessInfo.userName>>>>>),
        WhereRestriction = typeof(Where<Customer.bAccountID, IsNotNull, Or<FSServiceOrder.customerID, IsNull>>))]
    public class FSServiceOrder : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<FSServiceOrder>.By<srvOrdType, refNbr>
		{
			public static FSServiceOrder Find(PXGraph graph, string srvOrdType, string refNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, srvOrdType, refNbr, options);
		}

        public class UK : PrimaryKeyOf<FSServiceOrder>.By<sOID>
        {
            public static FSServiceOrder Find(PXGraph graph, int? sOID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, sOID, options);
        }

        public static class FK
        {
            public class Customer : AR.Customer.PK.ForeignKeyOf<FSServiceOrder>.By<customerID> { }
            public class CustomerLocation : Location.PK.ForeignKeyOf<FSServiceOrder>.By<customerID, locationID> { }
            public class BillCustomer : AR.Customer.PK.ForeignKeyOf<FSServiceOrder>.By<billCustomerID> { }
            public class BillCustomerLocation : Location.PK.ForeignKeyOf<FSServiceOrder>.By<billCustomerID, billLocationID> { }
            public class Branch : GL.Branch.PK.ForeignKeyOf<FSServiceOrder>.By<branchID> { }
            public class ServiceOrderType : FSSrvOrdType.PK.ForeignKeyOf<FSServiceOrder>.By<srvOrdType> { }
            public class Address : FSAddress.PK.ForeignKeyOf<FSServiceOrder>.By<serviceOrderAddressID> { }
            public class Contact : FSContact.PK.ForeignKeyOf<FSServiceOrder>.By<serviceOrderContactID> { }
            public class Contract : CT.Contract.PK.ForeignKeyOf<FSServiceOrder>.By<contractID> { }
            public class BranchLocation : FSBranchLocation.PK.ForeignKeyOf<FSServiceOrder>.By<branchLocationID> { }
            public class Project : PMProject.PK.ForeignKeyOf<FSServiceOrder>.By<projectID> { }
            public class ProjectTask : PMTask.PK.ForeignKeyOf<FSServiceOrder>.By<projectID, dfltProjectTaskID> { }
            public class WorkFlowStage : FSWFStage.PK.ForeignKeyOf<FSServiceOrder>.By<wFStageID> { }
            public class ServiceContract : FSServiceContract.PK.ForeignKeyOf<FSServiceOrder>.By<serviceContractID> { }
            public class Schedule : FSSchedule.PK.ForeignKeyOf<FSServiceOrder>.By<scheduleID> { }
            public class BillServiceContract : FSServiceContract.PK.ForeignKeyOf<FSServiceOrder>.By<billServiceContractID> { }
            public class TaxZone : Objects.TX.TaxZone.PK.ForeignKeyOf<FSServiceOrder>.By<taxZoneID> { }
            public class Problem : FSProblem.UK.ForeignKeyOf<FSServiceOrder>.By<problemID> { }
            public class CurrencyInfo : CM.CurrencyInfo.PK.ForeignKeyOf<FSServiceOrder>.By<curyInfoID> { }
            public class Currency : CM.Currency.PK.ForeignKeyOf<FSServiceOrder>.By<curyID> { }
            public class Room : FSRoom.PK.ForeignKeyOf<FSServiceOrder>.By<branchLocationID, roomID> { }
            public class SalesPerson : AR.SalesPerson.PK.ForeignKeyOf<FSServiceOrder>.By<salesPersonID> { }
        }

        #endregion
        #region Events
        public class Events : PXEntityEvent<FSServiceOrder>.Container<Events>
        {
            public PXEntityEvent<FSServiceOrder> ServiceOrderDeleted;
            public PXEntityEvent<FSServiceOrder> ServiceContractCleared;
            public PXEntityEvent<FSServiceOrder> ServiceContractPeriodAssigned;
            // TODO: Delete in the next major release
            public PXEntityEvent<FSServiceOrder> ServiceContractPeriodCleared;
            public PXEntityEvent<FSServiceOrder> RequiredServiceContractPeriodCleared;
            public PXEntityEvent<FSServiceOrder> LastAppointmentCompleted;
            public PXEntityEvent<FSServiceOrder> LastAppointmentCanceled;
            public PXEntityEvent<FSServiceOrder> LastAppointmentClosed;
            public PXEntityEvent<FSServiceOrder> AppointmentReOpened;
            public PXEntityEvent<FSServiceOrder> AppointmentUnclosed;
        }
        #endregion

        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXDBString(4, IsKey = true, IsFixed = true, InputMask = ">AAAA")]
        [PXUIField(DisplayName = "Order Type", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault(typeof(Coalesce<
            Search<
            FSxUserPreferences.dfltSrvOrdType,
            Where<
                PX.SM.UserPreferences.userID.IsEqual<AccessInfo.userID.FromCurrent>>>,
            Search<
            FSSetup.dfltSrvOrdType>>))]
        [FSSelectorSrvOrdType]
		[PXRestrictor(typeof(Where<FSSrvOrdType.active, Equal<True>>), null)]
		[PXUIVerify(typeof(Where<Current<FSSrvOrdType.active>, Equal<True>>),
					PXErrorLevel.Warning, TX.Error.SRVORDTYPE_INACTIVE, CheckOnRowSelected = true)]
		[PX.Data.EP.PXFieldDescription]
        public virtual string SrvOrdType { get; set; }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXDefault]
        [PXUIField(DisplayName = "Order Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
        [FSSelectorSORefNbr]
        [AutoNumber(typeof(Search<
            FSSrvOrdType.srvOrdNumberingID,
            Where<FSSrvOrdType.srvOrdType, Equal<Optional<FSServiceOrder.srvOrdType>>>>),
                    typeof(AccessInfo.businessDate))]
        [PX.Data.EP.PXFieldDescription]
        public virtual string RefNbr { get; set; }
        #endregion
        #region SOID
        public abstract class sOID : PX.Data.BQL.BqlInt.Field<sOID> { }

        [PXDBIdentity]
        public virtual int? SOID { get; set; }
        #endregion

        #region WorkflowTypeID
        [PXDBString(2, IsFixed = true)]
        [PXDefault(typeof(Selector<srvOrdType, FSSrvOrdType.serviceOrderWorkflowTypeID>))]
        [PXFormula(typeof(Default<srvOrdType>))]
        [workflowTypeID.Values.List]
        [PXUIField(DisplayName = "Workflow Type", Enabled = false)]
        public virtual string WorkflowTypeID { get; set; }
        public abstract class workflowTypeID : PX.Data.BQL.BqlString.Field<workflowTypeID> 
        {
            public abstract class Values : ListField.ServiceOrderWorkflowTypes { }
        }
        #endregion
        #region Attributes
        /// <summary>
        /// A service field, which is necessary for the <see cref="CSAnswers">dynamically 
        /// added attributes</see> defined at the <see cref="FSSrvOrdType">customer 
        /// class</see> level to function correctly.
        /// </summary>
        [CRAttributesField(typeof(FSServiceOrder.srvOrdType), typeof(FSServiceOrder.noteID))]
        public virtual string[] Attributes { get; set; }
        #endregion

        #region ServiceOrderAddressID
        public abstract class serviceOrderAddressID : PX.Data.IBqlField
        {
        }
        protected Int32? _SrvOrdAddressID;
        [PXDBInt]
        [FSSrvOrdAddressAttribute(typeof(Select<
            Address,
            Where<True, Equal<False>>>))]
        public virtual Int32? ServiceOrderAddressID
        {
            get
            {
                return this._SrvOrdAddressID;
            }
            set
            {
                this._SrvOrdAddressID = value;
            }
        }
        #endregion
        #region ServiceOrderContactID
        public abstract class serviceOrderContactID : PX.Data.IBqlField
        {
        }
        protected Int32? _SrvOrdContactID;
        [PXDBInt]
        [FSSrvOrdContactAttribute(typeof(Select<
            Contact,
            Where<True, Equal<False>>>))]
        public virtual Int32? ServiceOrderContactID
        {
            get
            {

                return this._SrvOrdContactID;
            }
            set
            {
                this._SrvOrdContactID = value;
            }
        }
        #endregion  
        #region AllowOverrideContactAddress
        public abstract class allowOverrideContactAddress : PX.Data.IBqlField
        {
        }
        protected Boolean? _AllowOverrideContactAddress;

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Override")]
        public virtual Boolean? AllowOverrideContactAddress
        {
            get
            {
                return this._AllowOverrideContactAddress;
            }
            set
            {
                this._AllowOverrideContactAddress = value;
            }
        }
        #endregion

        #region AllowInvoice
        public abstract class allowInvoice : PX.Data.BQL.BqlBool.Field<allowInvoice> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Allow Billing", Enabled = false)]
        public virtual bool? AllowInvoice { get; set; }
        #endregion
        #region AssignedEmpID
        public abstract class assignedEmpID : PX.Data.BQL.BqlInt.Field<assignedEmpID> { }

        [PXDBInt]
        [FSSelector_StaffMember_All]
        [PXUIField(DisplayName = "Supervisor")]
        public virtual int? AssignedEmpID { get; set; }
        #endregion
        #region AutoDocDesc
        public abstract class autoDocDesc : PX.Data.BQL.BqlString.Field<autoDocDesc> { }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Service description", Visible = true, Enabled = false)]
        public virtual string AutoDocDesc { get; set; }
        #endregion
        #region CustomerID
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Customer", Visibility = PXUIVisibility.SelectorVisible)]
        [PXRestrictor(typeof(Where<BAccountSelectorBase.status, IsNull,
                Or<BAccountSelectorBase.status, Equal<CustomerStatus.active>,
                Or<BAccountSelectorBase.status, Equal<CustomerStatus.prospect>,
                Or<BAccountSelectorBase.status, Equal<CustomerStatus.oneTime>>>>>), 
                PX.Objects.AR.Messages.CustomerIsInStatus, typeof(BAccountSelectorBase.status))]
        [FSSelectorBusinessAccount_CU_PR_VC]
        [PXForeignReference(typeof(FK.Customer))]
        public virtual int? CustomerID { get; set; }
        #endregion
        #region LocationID
        public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }

        [FSLocationActive(typeof(
            Where<Location.bAccountID, Equal<Current<FSServiceOrder.customerID>>,
                And<MatchWithBranch<Location.cBranchID>>>),
					DescriptionField = typeof(Location.descr), DisplayName = "Location", DirtyRead = true)]
        [PXDefault(typeof(Coalesce<Search2<
            BAccountR.defLocationID,
            InnerJoin<CRLocation, 
                On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>, 
                And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>>,
            Where<BAccountR.bAccountID, Equal<Current<FSServiceOrder.customerID>>,
                And<CRLocation.isActive, Equal<True>,
                And<MatchWithBranch<CRLocation.cBranchID>>>>>,
            Search<
            CRLocation.locationID,
            Where<CRLocation.bAccountID, Equal<Current<FSServiceOrder.customerID>>,
                And<CRLocation.isActive, Equal<True>, 
                And<MatchWithBranch<CRLocation.cBranchID>>>>>>))]
        [PXForeignReference(typeof(FK.CustomerLocation))]
        public virtual int? LocationID { get; set; }
        #endregion
        #region BillCustomerID
        public abstract class billCustomerID : PX.Data.BQL.BqlInt.Field<billCustomerID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Billing Customer")]
        [FSCustomer]
        [PXForeignReference(typeof(FK.BillCustomer))]
        [PXUIEnabled(typeof(Where<bAccountRequired.FromCurrent.IsEqual<True>.
                            And<billServiceContractID.FromCurrent.IsNull>>))]
        public virtual int? BillCustomerID { get; set; }
        #endregion
        #region BillLocationID
        public abstract class billLocationID : PX.Data.BQL.BqlInt.Field<billLocationID> { }

        [FSLocationActive(typeof(
            Where<Location.bAccountID, Equal<Current<FSServiceOrder.billCustomerID>>,
                And<MatchWithBranch<Location.cBranchID>>>),
						DescriptionField = typeof(Location.descr),
						DisplayName = "Billing Location",
						DirtyRead = true)]
        [PXUIEnabled(typeof(Where<bAccountRequired.FromCurrent.IsEqual<True>.
                            And<billServiceContractID.FromCurrent.IsNull>>))]
        [PXForeignReference(typeof(FK.BillCustomerLocation))]
        public virtual int? BillLocationID { get; set; }
        #endregion

        #region DocDesc
        public abstract class docDesc : PX.Data.BQL.BqlString.Field<docDesc> { }

        [PXDBString(Common.Constants.TranDescLength, IsUnicode = true)]
        [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string DocDesc { get; set; }
        #endregion

        #region ContactID
        public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Contact")]
        [FSSelectorContact(typeof(FSServiceOrder.customerID))]
        public virtual int? ContactID { get; set; }
        #endregion
        #region ContractID
        public abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Contract", Enabled = false)]
        [FSSelectorContract]
        [PXRestrictor(typeof(
            Where<Contract.status, Equal<FSServiceContract.status.Active>>), "Restrictor 1")]
        [PXRestrictor(typeof(
            Where<Current<AccessInfo.businessDate>, LessEqual<Contract.graceDate>, 
                Or<Contract.expireDate, IsNull>>), "Restrictor 2")]
        [PXRestrictor(typeof(
            Where<Current<AccessInfo.businessDate>, GreaterEqual<Contract.startDate>>), "Restrictor 3", typeof(Contract.startDate))]
        public virtual int? ContractID { get; set; }
        #endregion

        #region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

        [PXDBInt]
        [PXDefault(typeof(AccessInfo.branchID))]
        [PXUIField(DisplayName = "Branch")]
        [PXSelector(typeof(Search<Branch.branchID>), SubstituteKey = typeof(Branch.branchCD), DescriptionField = typeof(Branch.acctName))]
        public virtual Int32? BranchID { get; set; }
        #endregion
        #region BranchLocationID
        public abstract class branchLocationID : PX.Data.BQL.BqlInt.Field<branchLocationID> { }

        [PXDBInt]
        [PXDefault(typeof(
            Search<
                FSxUserPreferences.dfltBranchLocationID, 
                Where<
                PX.SM.UserPreferences.userID, Equal<CurrentValue<AccessInfo.userID>>,
                    And<PX.SM.UserPreferences.defBranchID, Equal<Current<FSServiceOrder.branchID>>>>>))]
        [PXUIField(DisplayName = "Branch Location")]
        [PXSelector(typeof(
            Search<
                FSBranchLocation.branchLocationID, 
                Where<
                FSBranchLocation.branchID, Equal<Current<FSServiceOrder.branchID>>>>), 
            SubstituteKey = typeof(FSBranchLocation.branchLocationCD),
            DescriptionField = typeof(FSBranchLocation.descr))]
        [PXFormula(typeof(Default<FSServiceOrder.branchID>))]
        public virtual int? BranchLocationID { get; set; }
        #endregion
        #region RoomID
        public abstract class roomID : PX.Data.BQL.BqlString.Field<roomID> { }

        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Room")]
        [PXSelector(typeof(
            Search<
                FSRoom.roomID, 
                Where<
                FSRoom.branchLocationID, Equal<Current<FSServiceOrder.branchLocationID>>>>), 
            SubstituteKey = typeof(FSRoom.roomID), DescriptionField = typeof(FSRoom.descr))]
        public virtual string RoomID { get; set; }
        #endregion
        #region OrderDate
        public abstract class orderDate : PX.Data.BQL.BqlDateTime.Field<orderDate> { }

        [PXDBDate(DisplayMask = "d")]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? OrderDate { get; set; }
        #endregion

        #region UserConfirmedClosing
        public abstract class userConfirmedClosing : PX.Data.BQL.BqlBool.Field<userConfirmedClosing> { }

        [PXBool]
        public virtual bool? UserConfirmedClosing { get; set; }
        #endregion
        #region UserConfirmedUnclosing
        public abstract class userConfirmedUnclosing : PX.Data.BQL.BqlBool.Field<userConfirmedUnclosing> { }

        [PXBool]
        public virtual bool? UserConfirmedUnclosing { get; set; }
        #endregion

        #region Copied
        public abstract class copied : PX.Data.BQL.BqlBool.Field<copied> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Copied", Enabled = false)]
        public virtual bool? Copied { get; set; }
        #endregion
        #region Confirmed
        public abstract class confirmed : PX.Data.BQL.BqlBool.Field<confirmed> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Confirmed", Enabled = false)]
        public virtual bool? Confirmed { get; set; }
        #endregion

        #region OpenDoc
        public abstract class openDoc : PX.Data.BQL.BqlBool.Field<openDoc> { }

        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Open", Enabled = false)]
        public virtual bool? OpenDoc { get; set; }
        #endregion
        #region ProcessReopenAction
        public abstract class processReopenAction : PX.Data.BQL.BqlBool.Field<processReopenAction> { }

        [PXBool]
        public virtual bool? ProcessReopenAction { get; set; }
        #endregion

        #region Hold
        public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Hold", Enabled = false)]
        public virtual bool? Hold { get; set; }
        #endregion
        #region Awaiting
        public abstract class awaiting : PX.Data.BQL.BqlBool.Field<awaiting> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Awaiting", Enabled = false)]
        public virtual bool? Awaiting { get; set; }
        #endregion

        #region Completed
        public abstract class completed : PX.Data.BQL.BqlBool.Field<completed> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Completed", Enabled = false)]
        public virtual bool? Completed { get; set; }
        #endregion
        #region ProcessCompleteAction
        public abstract class processCompleteAction : PX.Data.BQL.BqlBool.Field<processCompleteAction> { }

        [PXBool]
        public virtual bool? ProcessCompleteAction { get; set; }
        #endregion
        #region CompleteAppointments
        public abstract class completeAppointments : PX.Data.BQL.BqlBool.Field<completeAppointments> { }

        [PXBool()]
        public virtual bool? CompleteAppointments { get; set; }
        #endregion

        #region Closed
        public abstract class closed : PX.Data.BQL.BqlBool.Field<closed> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Closed", Enabled = false)]
        public virtual bool? Closed { get; set; }
        #endregion
        #region ProcessCloseAction
        public abstract class processCloseAction : PX.Data.BQL.BqlBool.Field<processCloseAction> { }

        [PXBool]
        public virtual bool? ProcessCloseAction { get; set; }
        #endregion
        #region CloseAppointments
        public abstract class closeAppointments : PX.Data.BQL.BqlBool.Field<closeAppointments> { }

        [PXBool()]
        public virtual bool? CloseAppointments { get; set; }
        #endregion

        #region Canceled
        public abstract class canceled : PX.Data.BQL.BqlBool.Field<canceled> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Canceled", Enabled = false)]
        public virtual bool? Canceled { get; set; }
        #endregion
        #region ProcessCancelAction
        public abstract class processCancelAction : PX.Data.BQL.BqlBool.Field<processCancelAction> { }

        [PXBool]
        public virtual bool? ProcessCancelAction { get; set; }
        #endregion
        #region CancelAppointments
        public abstract class cancelAppointments : PX.Data.BQL.BqlBool.Field<cancelAppointments> { }

        [PXBool()]
        public virtual bool? CancelAppointments { get; set; }
        #endregion

        #region Billed
        public abstract class billed : PX.Data.BQL.BqlBool.Field<billed> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Billed", Enabled = false)]
        public virtual bool? Billed { get; set; }
        #endregion
        #region BillingBy
        public abstract class billingBy : PX.Data.BQL.BqlString.Field<billingBy>
        {
            public abstract class Values : ListField_Billing_By { }
        }

        [PXDBString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Billing By", Enabled = false)]
        [billingBy.Values.ListAtrribute]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual string BillingBy { get; set; }
		#endregion
		#region BillOnlyCompletedClosed
		public abstract class billOnlyCompletedClosed : PX.Data.BQL.BqlBool.Field<billOnlyCompletedClosed> { }
		/// <summary>
		/// Non-used field
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Bill Only Completed or Closed Service Orders")]
		public virtual bool? BillOnlyCompletedClosed { get; set; }
		#endregion

		#region CompleteActionRunning
		public abstract class completeActionRunning : PX.Data.BQL.BqlBool.Field<completeActionRunning> { }

        [PXBool]
        [PXUnboundDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? CompleteActionRunning { get; set; }
        #endregion
        #region CancelActionRunning
        public abstract class cancelActionRunning : PX.Data.BQL.BqlBool.Field<cancelActionRunning> { }

        [PXBool]
        [PXUnboundDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? CancelActionRunning { get; set; }
        #endregion
        #region ReopenActionRunning
        public abstract class reopenActionRunning : PX.Data.BQL.BqlBool.Field<reopenActionRunning> { }

        [PXBool]
        [PXUnboundDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? ReopenActionRunning { get; set; }
        #endregion
        #region CloseActionRunning
        public abstract class closeActionRunning : PX.Data.BQL.BqlBool.Field<closeActionRunning> { }

        [PXBool]
        [PXUnboundDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? CloseActionRunning { get; set; }
        #endregion
        #region UnCloseActionRunning
        public abstract class unCloseActionRunning : PX.Data.BQL.BqlBool.Field<unCloseActionRunning> { }

        [PXBool]
        [PXUnboundDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? UnCloseActionRunning { get; set; }
        #endregion

        #region Status
        public abstract class status : PX.Data.BQL.BqlString.Field<status>
        {
            public abstract class Values : ListField.ServiceOrderStatus { }
        }

        [PXDBString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [status.Values.List]
        [PXDefault(status.Values.Open)]
        public virtual string Status { get; set; }
        #endregion
        #region WFStageID
        public abstract class wFStageID : PX.Data.BQL.BqlInt.Field<wFStageID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Workflow Stage")]
        [FSSelectorWorkflowStage(typeof(FSServiceOrder.srvOrdType))]
        [PXDefault(typeof(Search2<
            FSWFStage.wFStageID,
            InnerJoin<FSSrvOrdType,
                On<
                    FSSrvOrdType.srvOrdTypeID, Equal<FSWFStage.wFID>>>,
            Where<
                FSSrvOrdType.srvOrdType, Equal<Current<FSServiceOrder.srvOrdType>>>,
            OrderBy<
                Asc<FSWFStage.parentWFStageID,
                Asc<FSWFStage.sortOrder>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIVisible(typeof(FSSetup.showWorkflowStageField.FromCurrent.IsEqual<True>))]
        public virtual int? WFStageID { get; set; }
        #endregion

        #region CuryID
        public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
        [PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
        [PXDefault(typeof(Current<AccessInfo.baseCuryID>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(Currency.curyID))]
        [PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual String CuryID { get; set; }
        #endregion
        #region CuryInfoID
        public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
        [PXDBLong]
        [CurrencyInfo]
        public virtual Int64? CuryInfoID { get; set; }
        #endregion
        #region ProjectID
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

        [ProjectDefault]
        [ProjectBase(typeof(FSServiceOrder.billCustomerID))]
        [PXRestrictor(typeof(
            Where<PMProject.isActive, Equal<True>>), PM.Messages.InactiveContract, typeof(PMProject.contractCD))]
        [PXRestrictor(typeof(
            Where<PMProject.isCancelled, Equal<False>>), PM.Messages.CancelledContract, typeof(PMProject.contractCD))]
        public virtual int? ProjectID { get; set; }
        #endregion
        #region DfltProjectTaskID
        public abstract class dfltProjectTaskID : PX.Data.BQL.BqlInt.Field<dfltProjectTaskID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Default Project Task", Visibility = PXUIVisibility.Visible, FieldClass = ProjectAttribute.DimensionName)]
        [PXFormula(typeof(Default<projectID>))]
        [PXDefault(typeof(Search<
            PMTask.taskID,
            Where<PMTask.projectID, Equal<Current<projectID>>,
                And<PMTask.isDefault, Equal<True>,
                And<Where<PMTask.status,
                            Equal<ProjectTaskStatus.active>, 
                    Or<PMTask.status, Equal<ProjectTaskStatus.planned>>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [FSSelectorActive_AR_SO_ProjectTask(typeof(
            Where<PMTask.projectID, Equal<Current<projectID>>>))]
        public virtual int? DfltProjectTaskID { get; set; }
        #endregion

        #region EstimatedDurationTotal
        public abstract class estimatedDurationTotal : PX.Data.BQL.BqlInt.Field<estimatedDurationTotal> { }

        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
        [PXUIField(DisplayName = "Estimated Duration", Enabled = false)]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? EstimatedDurationTotal { get; set; }
        #endregion

        #region LongDescr
        public abstract class longDescr : PX.Data.BQL.BqlString.Field<longDescr> { }

        [PXDBString(int.MaxValue, IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
        public virtual string LongDescr { get; set; }
        #endregion

        #region EstimatedOrderTotal
        public abstract class estimatedOrderTotal : PX.Data.BQL.BqlDecimal.Field<estimatedOrderTotal> { }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Ext. Price Total", Enabled = false)]
        public virtual Decimal? EstimatedOrderTotal { get; set; }
        #endregion
        #region CuryEstimatedOrderTotal
        public abstract class curyEstimatedOrderTotal : PX.Data.BQL.BqlDecimal.Field<curyEstimatedOrderTotal> { }
        [PXDBCurrency(typeof(curyInfoID), typeof(estimatedOrderTotal))]
        [PXUIField(DisplayName = "Ext. Price Total", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryEstimatedOrderTotal { get; set; }
        #endregion
        #region BillableOrderTotal
        public abstract class billableOrderTotal : PX.Data.BQL.BqlDecimal.Field<billableOrderTotal> { }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Estimated Billable Total", Enabled = false)]
        public virtual Decimal? BillableOrderTotal { get; set; }
        #endregion
        #region CuryBillableOrderTotal
        public abstract class curyBillableOrderTotal : PX.Data.BQL.BqlDecimal.Field<curyBillableOrderTotal> { }
        private Decimal? _CuryBillableOrderTotal;

        [PXDBCurrency(typeof(curyInfoID), typeof(billableOrderTotal))]
        [PXUIField(DisplayName = "Estimated Billable Total", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryBillableOrderTotal
        {
            get
            {
                return _CuryBillableOrderTotal;
            }
            set
            {
                _CuryBillableOrderTotal = value;
            }
        }

        #endregion

        #region Priority
        public abstract class priority : ListField_Priority_ServiceOrder
        {
        }

        [PXDBString(1, IsFixed = true)]
        [PXDefault(ID.Priority_ServiceOrder.MEDIUM)]
        [PXUIField(DisplayName = "Priority", Visibility = PXUIVisibility.SelectorVisible)]
        [priority.ListAtrribute]
        public virtual string Priority { get; set; }
        #endregion
        #region ProblemID
        public abstract class problemID : PX.Data.BQL.BqlInt.Field<problemID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Problem")]
        [PXSelector(typeof(Search2<
            FSProblem.problemID,
            InnerJoin<FSSrvOrdTypeProblem, 
                On<FSProblem.problemID, Equal<FSSrvOrdTypeProblem.problemID>>,
            InnerJoin<FSSrvOrdType, 
                On<FSSrvOrdType.srvOrdType, Equal<FSSrvOrdTypeProblem.srvOrdType>>>>,
            Where<FSSrvOrdType.srvOrdType, Equal<Current<FSServiceOrder.srvOrdType>>>>),
                            SubstituteKey = typeof(FSProblem.problemCD), DescriptionField = typeof(FSProblem.descr))]
        public virtual int? ProblemID { get; set; }
        #endregion
        #region Severity
        public abstract class severity : ListField_Severity_ServiceOrder
        {
        }

        [PXDBString(1, IsFixed = true)]
        [PXDefault(ID.Severity_ServiceOrder.MEDIUM)]
        [PXUIField(DisplayName = "Severity", Visibility = PXUIVisibility.SelectorVisible)]
        [severity.ListAtrribute]
        public virtual string Severity { get; set; }
        #endregion
        #region SLAETA
        public abstract class sLAETA : PX.Data.BQL.BqlDateTime.Field<sLAETA> { }

        protected DateTime? _SLAETA;
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "SLA")]
        [PXUIField(DisplayName = "SLA")]
        public virtual DateTime? SLAETA
        {
            get
            {
                return this._SLAETA;
            }

            set
            {
                this.SLAETAUTC = value;
                this._SLAETA = value;
            }
        }
        #endregion
        #region SourceDocType
        public abstract class sourceDocType : PX.Data.BQL.BqlString.Field<sourceDocType> { }

        [PXDBString(4, IsFixed = true)]
        [PXUIField(DisplayName = "Source Document Type", Enabled = false)]
        public virtual string SourceDocType { get; set; }
        #endregion
        #region SourceID
        public abstract class sourceID : PX.Data.BQL.BqlInt.Field<sourceID> { }

        [PXDBInt]
        public virtual int? SourceID { get; set; }
        #endregion
        #region SourceRefNbr
        public abstract class sourceRefNbr : PX.Data.BQL.BqlString.Field<sourceRefNbr> { }

        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Source Ref. Nbr.", Enabled = false)]
        public virtual string SourceRefNbr { get; set; }
        #endregion
        #region SourceType
        public abstract class sourceType : ListField_SourceType_ServiceOrder
        {
        }

        [PXDBString(2, IsFixed = true)]
        [PXDefault(ID.SourceType_ServiceOrder.SERVICE_DISPATCH)]
        [PXUIField(DisplayName = "Document Type", Enabled = false)]
        [sourceType.ListAtrribute]
        public virtual string SourceType { get; set; }
        #endregion

        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

        [PXNote(ShowInReferenceSelector=true)]
        [PXSearchable(SM.SearchCategory.FS, "SM {0}: {1} - {3}", new Type[] { typeof(FSServiceOrder.srvOrdType), typeof(FSServiceOrder.refNbr), typeof(FSServiceOrder.customerID), typeof(Customer.acctName) },
           new Type[] { typeof(Customer.acctCD), typeof(FSServiceOrder.srvOrdType), typeof(FSServiceOrder.custWorkOrderRefNbr), typeof(FSServiceOrder.docDesc) },
           NumberFields = new Type[] { typeof(FSServiceOrder.refNbr) },
           Line1Format = "{0:d}{1}{2}", Line1Fields = new Type[] { typeof(FSServiceOrder.orderDate), typeof(FSServiceOrder.status), typeof(FSServiceOrder.custWorkOrderRefNbr) },
           Line2Format = "{0}", Line2Fields = new Type[] { typeof(FSServiceOrder.docDesc) },
           MatchWithJoin = typeof(InnerJoin<Customer, On<Customer.bAccountID, Equal<FSServiceOrder.customerID>>>),
           SelectForFastIndexing = typeof(Select2<
               FSServiceOrder, 
               InnerJoin<Customer, 
                   On<FSServiceOrder.customerID, Equal<Customer.bAccountID>>>>)
        )]
        
        public virtual Guid? NoteID { get; set; }
        #endregion
        #region LineCntr
        public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }

        [PXDBInt()]
        [PXDefault(0)]
        public virtual Int32? LineCntr { get; set; }
        #endregion
        #region SplitLineCntr
        public abstract class splitLineCntr : PX.Data.BQL.BqlInt.Field<splitLineCntr> { }

        [PXDBInt()]
        [PXDefault(0)]
        public virtual Int32? SplitLineCntr { get; set; }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        [PXDBCreatedByID]
        [PXUIField(DisplayName = "Created By")]
        public virtual Guid? CreatedByID { get; set; }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        [PXDBCreatedByScreenID]
        [PXUIField(DisplayName = "Created By Screen ID")]
        public virtual string CreatedByScreenID { get; set; }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        [PXDBCreatedDateTime]
        [PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
        public virtual DateTime? CreatedDateTime { get; set; }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        [PXDBLastModifiedByID]
        [PXUIField(DisplayName = "Last Modified By")]
        public virtual Guid? LastModifiedByID { get; set; }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        [PXDBLastModifiedByScreenID]
        [PXUIField(DisplayName = "Last Modified By Screen ID")]
        public virtual string LastModifiedByScreenID { get; set; }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        [PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        [PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
        public virtual byte[] tstamp { get; set; }
        #endregion
        #region BAccountRequired
        public abstract class bAccountRequired : PX.Data.BQL.BqlBool.Field<bAccountRequired> { }

        [PXDBBool]
        [PXDefault(typeof(Selector<srvOrdType, FSSrvOrdType.bAccountRequired>))]
        [PXUIField(DisplayName = "Customer Required", Enabled = false)]
        public virtual bool? BAccountRequired { get; set; }
        #endregion
        #region Quote
        public abstract class quote : PX.Data.BQL.BqlBool.Field<quote> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Quote", Enabled = false)]
        public virtual bool? Quote { get; set; }
        #endregion
        #region ServiceContractID
        public abstract class serviceContractID : PX.Data.BQL.BqlInt.Field<serviceContractID> { }

        [PXDBInt]
        [PXSelector(typeof(Search<
            FSServiceContract.serviceContractID,
            Where<
                                FSServiceContract.customerID, Equal<Current<FSServiceOrder.customerID>>>>),
                           SubstituteKey = typeof(FSServiceContract.refNbr))]
        [PXUIField(DisplayName = "Source Service Contract ID", Enabled = false, FieldClass = "FSCONTRACT")]
        public virtual int? ServiceContractID { get; set; }
        #endregion
        #region ScheduleID
        public abstract class scheduleID : PX.Data.BQL.BqlInt.Field<scheduleID> { }

        [PXDBInt]
        [PXSelector(typeof(Search<
            FSSchedule.scheduleID,
            Where<
                                FSSchedule.entityType, Equal<ListField_Schedule_EntityType.Contract>,
                And<FSSchedule.entityID, Equal<Current<FSServiceOrder.serviceContractID>>>>>),
                           SubstituteKey = typeof(FSSchedule.refNbr))]
        [PXUIField(DisplayName = "Source Schedule ID", Enabled = false, FieldClass = "FSCONTRACT")]
        public virtual int? ScheduleID { get; set; }
        #endregion
        #region FinPeriodID
        public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

        [PXDBString(6, IsFixed = true)]
        [PXUIField(DisplayName = "Post Period")]
        public virtual string FinPeriodID { get; set; }
        #endregion
        #region GenerationID
        public abstract class generationID : PX.Data.BQL.BqlInt.Field<generationID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Generation ID")]
        public virtual int? GenerationID { get; set; }
        #endregion
        #region CustWorkOrderRefNbr
        public abstract class custWorkOrderRefNbr : PX.Data.BQL.BqlString.Field<custWorkOrderRefNbr> { }

        [PXDBString(40, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC")]
        [NormalizeWhiteSpace]
        [PXUIField(DisplayName = "External Reference")]
        public virtual string CustWorkOrderRefNbr { get; set; }
        #endregion
        #region CustPORefNbr
        public abstract class custPORefNbr : PX.Data.BQL.BqlString.Field<custPORefNbr> { }

        [PXDBString(40, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC")]
        [NormalizeWhiteSpace]
        [PXUIField(DisplayName = "Customer Order")]
        public virtual string CustPORefNbr { get; set; }
        #endregion
        #region ServiceCount
        public abstract class serviceCount : PX.Data.BQL.BqlInt.Field<serviceCount> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Service Count", Enabled = false)]
        public virtual int? ServiceCount { get; set; }
        #endregion
        #region ScheduledServiceCount
        public abstract class scheduledServiceCount : PX.Data.BQL.BqlInt.Field<scheduledServiceCount> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Scheduled Service Count", Enabled = false)]
        public virtual int? ScheduledServiceCount { get; set; }
        #endregion
        #region CompleteServiceCount
        public abstract class completeServiceCount : PX.Data.BQL.BqlInt.Field<completeServiceCount> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Complete Service Count", Enabled = false)]
        public virtual int? CompleteServiceCount { get; set; }
        #endregion
        #region PostedBy
        public abstract class postedBy : ListField_Billing_By
        {
        }

        [PXDBString(2, IsFixed = true)]
        public virtual string PostedBy { get; set; }
        #endregion
        #region PendingAPARSOPost
        public abstract class pendingAPARSOPost : PX.Data.BQL.BqlBool.Field<pendingAPARSOPost> { }

        [PXDBBool]
        [PXDefault(false)]
        public virtual bool? PendingAPARSOPost { get; set; }
        #endregion
        #region PendingINPost
        public abstract class pendingINPost : PX.Data.BQL.BqlBool.Field<pendingINPost> { }

        [PXDBBool]
        [PXDefault(false)]
        public virtual bool? PendingINPost { get; set; }
		#endregion
		#region CBID
		public abstract class cBID : PX.Data.BQL.BqlInt.Field<cBID> { }
		/// <summary>
		/// Non-used field
		/// </summary>
		[PXDBInt]
		public virtual int? CBID { get; set; }
		#endregion

		#region SalesPersonID
		public abstract class salesPersonID : PX.Data.BQL.BqlInt.Field<salesPersonID> { }

        [SalesPerson(DisplayName = "Salesperson")]
        [PXUIEnabled(typeof(
            Where<Current<FSSrvOrdType.behavior>, NotEqual<FSSrvOrdType.behavior.Values.internalAppointment>>))]
        [PXDefault(typeof(Search<
            CustDefSalesPeople.salesPersonID, 
            Where<CustDefSalesPeople.bAccountID, Equal<Current<FSServiceOrder.customerID>>, 
                And<CustDefSalesPeople.locationID, Equal<Current<FSServiceOrder.locationID>>, 
                And<CustDefSalesPeople.isDefault, Equal<True>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<FSServiceOrder.customerID>))]
        [PXFormula(typeof(Default<FSServiceOrder.locationID>))]
        [PXForeignReference(typeof(FK.SalesPerson))]
        public virtual int? SalesPersonID { get; set; }
        #endregion
        #region Commissionable
        public abstract class commissionable : PX.Data.BQL.BqlBool.Field<commissionable> { }

        [PXDBBool]
        [PXUIEnabled(typeof(
            Where<Current<FSSrvOrdType.behavior>, NotEqual<FSSrvOrdType.behavior.Values.internalAppointment>>))]
        [PXDefault(typeof(
            Search<
                FSSrvOrdType.commissionable,
                Where<FSSrvOrdType.srvOrdType, Equal<Current<FSServiceOrder.srvOrdType>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Commissionable")]
        public virtual bool? Commissionable { get; set; }
		#endregion

		#region CutOffDate
		public abstract class cutOffDate : PX.Data.BQL.BqlDateTime.Field<cutOffDate> { }
		/// <summary>
		/// Non-used field
		/// </summary>
		[PXDBDate]
		[PXUIField(DisplayName = "Cut-Off Date")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual DateTime? CutOffDate { get; set; }
		#endregion

		#region Appointment Totals
		#region ApptDurationTotal
		public abstract class apptDurationTotal : PX.Data.BQL.BqlInt.Field<apptDurationTotal> { }

        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
        [PXUIField(DisplayName = "Appointment Duration", Enabled = false)]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? ApptDurationTotal { get; set; }
        #endregion

        // TODO: rename it to CuryAppointmentLineTotal
        #region CuryApptOrderTotal
        public abstract class curyApptOrderTotal : PX.Data.BQL.BqlDecimal.Field<curyApptOrderTotal> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(apptOrderTotal))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Actual Billable Total", Enabled = false)]
        public virtual decimal? CuryApptOrderTotal { get; set; }
        #endregion
        // TODO: rename it to AppointmentLineTotal
        #region ApptOrderTotal
        public abstract class apptOrderTotal : PX.Data.BQL.BqlDecimal.Field<apptOrderTotal> { }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Appointment Line Total", Enabled = false)]
        public virtual Decimal? ApptOrderTotal { get; set; }
        #endregion

        #region CuryAppointmentTaxTotal
        public abstract class curyAppointmentTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyAppointmentTaxTotal> { }

        [PXCurrency(typeof(curyInfoID), typeof(appointmentTaxTotal))]
        [PXUIField(DisplayName = "Actual Tax Total", Enabled = false)]
        public virtual Decimal? CuryAppointmentTaxTotal { get; set; }
        #endregion
        #region AppointmentTaxTotal
        public abstract class appointmentTaxTotal : PX.Data.BQL.BqlDecimal.Field<appointmentTaxTotal> { }

        [PXDecimal]
        [PXUIField(DisplayName = "Base Appointment Tax Total", Enabled = false)]
        public virtual Decimal? AppointmentTaxTotal { get; set; }
        #endregion

        #region CuryAppointmentDocTotal
        public abstract class curyAppointmentDocTotal : PX.Data.BQL.BqlDecimal.Field<curyAppointmentDocTotal> { }

        [PXCurrency(typeof(curyInfoID), typeof(appointmentDocTotal))]
		[PXUIField(DisplayName = "Invoice Total", Enabled = false)]
        public virtual Decimal? CuryAppointmentDocTotal { get; set; }
        #endregion
        #region AppointmentDocTotal
        public abstract class appointmentDocTotal : PX.Data.BQL.BqlDecimal.Field<appointmentDocTotal> { }

        [PXDecimal]
        [PXUIField(DisplayName = "Base Invoice Total", Enabled = false)]
        public virtual Decimal? AppointmentDocTotal { get; set; }
        #endregion
        #endregion

        #region BillServiceContractID
        public abstract class billServiceContractID : PX.Data.BQL.BqlInt.Field<billServiceContractID> { }

        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[FSSelectorPPFRServiceContract(typeof(FSServiceOrder.customerID), typeof(FSServiceOrder.locationID))]
		[PXUIField(DisplayName = "Service Contract", FieldClass = "FSCONTRACT")]
		public virtual int? BillServiceContractID { get; set; }
        #endregion
        #region BillContractPeriodID 
        public abstract class billContractPeriodID : PX.Data.BQL.BqlInt.Field<billContractPeriodID> { }

        [PXDBInt]
        [FSSelectorContractBillingPeriod]
        [PXDefault(typeof(Search2<
            FSContractPeriod.contractPeriodID,
                InnerJoin<FSServiceContract,
                    On<FSServiceContract.serviceContractID, Equal<FSContractPeriod.serviceContractID>>>,
                Where<
                    FSContractPeriod.startPeriodDate, LessEqual<Current<FSServiceOrder.orderDate>>,
                    And<FSContractPeriod.endPeriodDate, GreaterEqual<Current<FSServiceOrder.orderDate>>,
                    And<FSContractPeriod.serviceContractID, Equal<Current<FSServiceOrder.billServiceContractID>>,
                    And2<
                        Where2<
                            Where<FSContractPeriod.status, Equal<FSContractPeriod.status.Active>,
                                Or<FSContractPeriod.status, Equal<FSContractPeriod.status.Pending>>>,
                            Or<Where<FSServiceContract.isFixedRateContract, Equal<True>,
                                And<FSContractPeriod.status, Equal<FSContractPeriod.status.Invoiced>>>>>,
                        And<Current<FSBillingCycle.billingBy>, Equal<FSBillingCycle.billingBy.Values.ServiceOrder>>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<FSServiceOrder.billCustomerID>))]
        [PXUIField(DisplayName = "Contract Period", Enabled = false)]
        public virtual int? BillContractPeriodID { get; set; }
        #endregion

        #region Effective Billable Totals
        #region CuryEffectiveBillableLineTotal
        public abstract class curyEffectiveBillableLineTotal : PX.Data.BQL.BqlDecimal.Field<curyEffectiveBillableLineTotal> { }

        [PXCurrency(typeof(curyInfoID), typeof(effectiveBillableLineTotal))]
        [PXUIField(DisplayName = "Line Total", Enabled = false)]
        public virtual Decimal? CuryEffectiveBillableLineTotal { get; set; }
        #endregion
        #region EffectiveBillableLineTotal
        public abstract class effectiveBillableLineTotal : PX.Data.BQL.BqlDecimal.Field<effectiveBillableLineTotal> { }

        [PXDecimal]
        public virtual Decimal? EffectiveBillableLineTotal { get; set; }
        #endregion

        #region CuryEffectiveLogBillableTranAmountTotal
        public abstract class curyEffectiveLogBillableTranAmountTotal : PX.Data.BQL.BqlDecimal.Field<curyEffectiveLogBillableTranAmountTotal> { }
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXCurrency(typeof(curyInfoID), typeof(effectiveLogBillableTranAmountTotal))]
        [PXUIField(DisplayName = "Billable Labor Total", Enabled = false)]
        public virtual Decimal? CuryEffectiveLogBillableTranAmountTotal { get; set; }
        #endregion
        #region EffectiveLogBillableTranAmountTotal
        public abstract class effectiveLogBillableTranAmountTotal : PX.Data.BQL.BqlDecimal.Field<effectiveLogBillableTranAmountTotal> { }

        [PXDecimal]
        public virtual Decimal? EffectiveLogBillableTranAmountTotal { get; set; }
        #endregion

        #region CuryEffectiveBillableTaxTotal
        public abstract class curyEffectiveBillableTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyEffectiveBillableTaxTotal> { }

        [PXCurrency(typeof(curyInfoID), typeof(effectiveBillableTaxTotal))]
        [PXUIField(DisplayName = "Tax Total", Enabled = false)]
        public virtual Decimal? CuryEffectiveBillableTaxTotal { get; set; }
        #endregion
        #region EffectiveBillableTaxTotal
        public abstract class effectiveBillableTaxTotal : PX.Data.BQL.BqlDecimal.Field<effectiveBillableTaxTotal> { }

        [PXDecimal]
        public virtual Decimal? EffectiveBillableTaxTotal { get; set; }
        #endregion

        #region CuryEffectiveBillableDocTotal
        public abstract class curyEffectiveBillableDocTotal : PX.Data.BQL.BqlDecimal.Field<curyEffectiveBillableDocTotal> { }

        [PXCurrency(typeof(curyInfoID), typeof(effectiveBillableDocTotal))]
        [PXUIField(DisplayName = "Invoice Total", Enabled = false)]
		public virtual Decimal? CuryEffectiveBillableDocTotal { get; set; }
        #endregion
        #region EffectiveBillableDocTotal
        public abstract class effectiveBillableDocTotal : PX.Data.BQL.BqlDecimal.Field<effectiveBillableDocTotal> { }

        [PXDecimal]
        public virtual Decimal? EffectiveBillableDocTotal { get; set; }
        #endregion

        #region CuryShortLabelEffectiveBillableDocTotal
        public abstract class curyShortLabelEffectiveBillableDocTotal : PX.Data.BQL.BqlDecimal.Field<curyShortLabelEffectiveBillableDocTotal> { }

        [PXDecimal]
        [PXUIField(DisplayName = "Billable Total", Enabled = false)]
        public virtual Decimal? CuryShortLabelEffectiveBillableDocTotal { get => CuryEffectiveBillableDocTotal; }
        #endregion
        #region ShortLabelEffectiveBillableDocTotal
        public abstract class shortLabelEffectiveBillableDocTotal : PX.Data.BQL.BqlDecimal.Field<shortLabelEffectiveBillableDocTotal> { }

        [PXDecimal]
        public virtual Decimal? ShortLabelEffectiveBillableDocTotal { get => EffectiveBillableDocTotal; }
        #endregion

        #region CuryEffectiveCostTotal
        public abstract class curyEffectiveCostTotal : PX.Data.BQL.BqlDecimal.Field<curyEffectiveCostTotal> { }

        [PXCurrency(typeof(curyInfoID), typeof(effectiveCostTotal))]
        [PXUIField(DisplayName = "Cost Total", Enabled = false)]
        public virtual Decimal? CuryEffectiveCostTotal { get; set; }
        #endregion
        #region EffectiveBillableLineTotal
        public abstract class effectiveCostTotal : PX.Data.BQL.BqlDecimal.Field<effectiveCostTotal> { }

        [PXDecimal]
        public virtual Decimal? EffectiveCostTotal { get; set; }
        #endregion

        #endregion

        // TODO: Fix the name of these fields: Non-base fields should start with Cury
        #region SOCuryUnpaidBalanace
        public abstract class sOCuryUnpaidBalanace : PX.Data.BQL.BqlDecimal.Field<sOCuryUnpaidBalanace> { }

        [PXCurrency(typeof(curyInfoID), typeof(sOUnpaidBalanace))]
        [PXUIField(DisplayName = "Service Order Unpaid Balance", Enabled = false)]
        public virtual Decimal? SOCuryUnpaidBalanace { get; set; }
        #endregion
        #region SOUnpaidBalanace
        public abstract class sOUnpaidBalanace : PX.Data.BQL.BqlDecimal.Field<sOUnpaidBalanace> { }

        [PXDecimal]
        public virtual Decimal? SOUnpaidBalanace { get; set; }
        #endregion

        // TODO: Fix the name of these fields: Non-base fields should start with Cury
        #region SOCuryBillableUnpaidBalanace
        public abstract class sOCuryBillableUnpaidBalanace : PX.Data.BQL.BqlDecimal.Field<sOCuryBillableUnpaidBalanace> { }

        [PXCurrency(typeof(curyInfoID), typeof(sOBillableUnpaidBalanace))]
        [PXUIField(DisplayName = "Service Order Billable Unpaid Balance", Enabled = false)]
        public virtual Decimal? SOCuryBillableUnpaidBalanace { get; set; }
        #endregion
        #region SOBillableUnpaidBalanace
        public abstract class sOBillableUnpaidBalanace : PX.Data.BQL.BqlDecimal.Field<sOBillableUnpaidBalanace> { }

        [PXDecimal]
        public virtual Decimal? SOBillableUnpaidBalanace { get; set; }
        #endregion

        // TODO: Fix the name of these fields: they are used as Cury fields but they don't have the prefix Cury
        #region SOPrepaymentReceived
        public abstract class sOPrepaymentReceived : PX.Data.BQL.BqlDecimal.Field<sOPrepaymentReceived> { }

        [PXDecimal]
        [PXUIField(DisplayName = "Prepayment Received", Enabled = false)]
        public virtual Decimal? SOPrepaymentReceived { get; set; }
        #endregion
        #region SOPrepaymentRemaining
        public abstract class sOPrepaymentRemaining : PX.Data.BQL.BqlDecimal.Field<sOPrepaymentRemaining> { }

        [PXDecimal]
        [PXUIField(DisplayName = "Prepayment Remaining", Enabled = false)]
        public virtual Decimal? SOPrepaymentRemaining { get; set; }
        #endregion
        #region SOPrepaymentApplied
        public abstract class sOPrepaymentApplied : PX.Data.BQL.BqlDecimal.Field<sOPrepaymentApplied> { }

        [PXDecimal]
        [PXUIField(DisplayName = "Prepayment Applied", Enabled = false)]
        public virtual Decimal? SOPrepaymentApplied { get; set; }
        #endregion

		#region WaitingForParts
        public abstract class waitingForParts : PX.Data.BQL.BqlBool.Field<waitingForParts> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXFormula(typeof(IIf<
            Where<pendingPOLineCntr, Greater<int0>>, True, False>))]
        [PXUIVisible(typeof(
            Where<Current<FSSrvOrdType.postTo>, Equal<FSPostTo.Sales_Order_Invoice>,
                Or<Current<FSSrvOrdType.postTo>, Equal<FSPostTo.Sales_Order_Module>,
                Or<Current<FSSrvOrdType.postTo>, Equal<FSPostTo.Projects>>>>))]
        [PXUIField(DisplayName = "Waiting for Purchased Items", Enabled = false, FieldClass = "DISTINV")]
        public virtual bool? WaitingForParts { get; set; }
        #endregion
        #region AppointmentsNeeded
        public abstract class appointmentsNeeded : PX.Data.BQL.BqlBool.Field<appointmentsNeeded> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Appointments Needed", Enabled = false)]
        public virtual bool? AppointmentsNeeded { get; set; }
        #endregion
        #region MaxLineNbr
        public abstract class maxLineNbr : PX.Data.BQL.BqlInt.Field<maxLineNbr> { }

        protected int? _MaxLineNbr;

        [PXDBInt()]
        [PXDefault(0)]
        public virtual int? MaxLineNbr
        {
            get
            {
                return this._MaxLineNbr;
            }

            set
            {
                this._MaxLineNbr = value;
            }
        }
        #endregion
        #region ApptNeededLineCntr
        public abstract class apptNeededLineCntr : PX.Data.BQL.BqlInt.Field<apptNeededLineCntr> { }

        [PXDBInt()]
        [PXDefault(0)]
        public virtual Int32? ApptNeededLineCntr { get; set; }
        #endregion
        #region PendingPOLineCntr
        public abstract class pendingPOLineCntr : PX.Data.BQL.BqlInt.Field<pendingPOLineCntr> { }

        [PXDBInt()]
        [PXDefault(0)]
        public virtual int? PendingPOLineCntr { get; set; }
        #endregion
        #region APBillLineCntr
        public abstract class apBillLineCntr : PX.Data.BQL.BqlInt.Field<apBillLineCntr> { }

        [PXDBInt]
        [PXDefault(0)]
        public virtual int? APBillLineCntr { get; set; }
        #endregion

        #region MemoryHelper
        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

        [PXBool]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected { get; set; }
        #endregion

        #region ReportLocationID
        public abstract class reportLocationID : PX.Data.BQL.BqlInt.Field<reportLocationID> { }

        [PXInt]
        [PXSelector(typeof(Search<
            Location.locationID,
            Where<
                                Location.bAccountID, Equal<Optional<FSServiceOrder.customerID>>>>),
                           SubstituteKey = typeof(Location.locationCD), 
                           DescriptionField = typeof(Location.descr))]
        public virtual int? ReportLocationID { get; set; }
        #endregion
                
        #region Mem_ReturnValueID
        [PXInt]
        public virtual int? Mem_ReturnValueID { get; set; }
        #endregion

        #region InvoicedByContract
        public abstract class invoicedByContract : PX.Data.BQL.BqlBool.Field<invoicedByContract> { }

        [PXBool]
        [PXUIField(Enabled = false)]
        [PXDBScalar(typeof(Search2<FSContractPeriod.invoiced,
                            InnerJoin<FSServiceContract, On<FSServiceContract.serviceContractID, Equal<FSContractPeriod.serviceContractID>>>,
                                Where<FSContractPeriod.serviceContractID, Equal<FSServiceOrder.billServiceContractID>, 
                                And<FSContractPeriod.contractPeriodID, Equal<FSServiceOrder.billContractPeriodID>,
                                And<FSServiceContract.billingType, Equal<FSServiceContract.billingType.Values.standardizedBillings>>>>>))]
        public virtual bool? InvoicedByContract { get; set; }
        #endregion
        #region Mem_Invoiced
        public abstract class mem_Invoiced : PX.Data.BQL.BqlBool.Field<mem_Invoiced> { }

        [PXBool]
        [PXUIField(DisplayName = "Billed", Enabled = false)]
        public virtual bool? Mem_Invoiced
        {
            get
            {
                return (this.PostedBy != null && this.PostedBy == ID.Billing_By.SERVICE_ORDER) 
                            || (this.BillServiceContractID != null && InvoicedByContract == true);
            }
        }
        #endregion

        #region AppointmentsCompleted
        [PXInt]
        public virtual int? AppointmentsCompletedCntr { get; set; }
        #endregion
        #region AppointmentsCompletedOrClosed
        [PXInt]
        public virtual int? AppointmentsCompletedOrClosedCntr { get; set; }
        #endregion
        #region MemRefNbr
        public abstract class memRefNbr : PX.Data.BQL.BqlString.Field<memRefNbr>
		{
        }

        [PXString(17, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCCCC")]
        public virtual string MemRefNbr { get; set; }
        #endregion
        #region MemAcctName
        public abstract class memAcctName : PX.Data.BQL.BqlString.Field<memAcctName>
		{
        }
        [PXString(62, IsUnicode = true)]
        public virtual string MemAcctName { get; set; }
        #endregion

        #region IsPrepaymentEnable
        public abstract class isPrepaymentEnable : PX.Data.BQL.BqlBool.Field<isPrepaymentEnable> { }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(Visible = false)]
        public virtual bool? IsPrepaymentEnable { get; set; }
        #endregion

        #region ShowInvoicesTab
        public abstract class showInvoicesTab : PX.Data.BQL.BqlBool.Field<showInvoicesTab> { }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(Visible = false)]
        public virtual bool? ShowInvoicesTab { get; set; }
        #endregion

        #region SourceReferenceNbr
        public abstract class sourceReferenceNbr : PX.Data.IBqlField { }
        [PXString]
        [PXUIField(DisplayName = "Reference Nbr.")]
        public virtual string SourceReferenceNbr
        {
            get
            {
                return (SourceDocType != null ? SourceDocType.Trim() + ", " : "") +
                       (SourceRefNbr != null ? SourceRefNbr.Trim() : "");
            }
        }
        #endregion

        #region ProjectCD
        public abstract class projectCD : PX.Data.BQL.BqlBool.Field<projectCD> { }

        [PXString]
        [PXUIField(DisplayName = "Project ID", FieldClass = ProjectAttribute.DimensionName)]
        public virtual string ProjectCD { get; set; }
        #endregion
        #region TaskCD
        public abstract class taskCD : PX.Data.BQL.BqlBool.Field<taskCD> { }

        [PXString]
        [PXUIField(DisplayName = "Task ID", FieldClass = ProjectAttribute.DimensionName)]
        public virtual string TaskCD { get; set; }
        #endregion
        #region ProjectDescr
        public abstract class projectDescr : PX.Data.BQL.BqlBool.Field<projectDescr> { }

        [PXString]
        [PXUIField(DisplayName = "Project Description", FieldClass = ProjectAttribute.DimensionName)]
        public virtual string ProjectDescr { get; set; }
        #endregion
        #region ProjectTaskDescr
        public abstract class projectTaskDescr : PX.Data.BQL.BqlBool.Field<projectTaskDescr> { }

        [PXString]
        [PXUIField(DisplayName = "Task Description", FieldClass = ProjectAttribute.DimensionName)]
        public virtual string ProjectTaskDescr { get; set; }
        #endregion

        #region CanCreatePurchaseOrder
        public abstract class canCreatePurchaseOrder : PX.Data.BQL.BqlBool.Field<canCreatePurchaseOrder> { }

        [PXBool]
        public virtual bool? CanCreatePurchaseOrder
        {
            get
            {
                return this.Canceled == false
                            && this.Closed == false
                            && this.WaitingForParts == true;
            }
        }
        #endregion
        #endregion
        #region DispatchBoardHelper
        #region SLARemaining
        public abstract class sLARemaining : PX.Data.BQL.BqlInt.Field<sLARemaining> { }

        [PXInt]
        public virtual int? SLARemaining { get; set; }
        #endregion
        #region CustomerDisplayName
        public abstract class customerDisplayName : PX.Data.BQL.BqlString.Field<customerDisplayName> { }

        [PXString]
        public virtual string CustomerDisplayName { get; set; }
        #endregion
        #region ContactName
        public abstract class contactName : PX.Data.BQL.BqlString.Field<contactName> { }

        [PXString]
        public virtual string ContactName { get; set; }
        #endregion
        #region ContactPhone
        public abstract class contactPhone : PX.Data.BQL.BqlString.Field<contactPhone> { }

        [PXString]
        public virtual string ContactPhone { get; set; }
        #endregion
        #region ContactEmail
        public abstract class contactEmail : PX.Data.BQL.BqlString.Field<contactEmail> { }

        [PXString]
        public virtual string ContactEmail { get; set; }
        #endregion
        #region AssignedEmployeeDisplayName
        public abstract class assignedEmployeeDisplayName : PX.Data.BQL.BqlString.Field<assignedEmployeeDisplayName> { }

        [PXString]
        public virtual string AssignedEmployeeDisplayName { get; set; }
        #endregion
        #region ServicesRemaning
        public abstract class servicesRemaining : PX.Data.BQL.BqlInt.Field<servicesRemaining > { }

        [PXInt]
        public virtual int? ServicesRemaining { get; set; }
        #endregion
        #region ServicesCount
        public abstract class servicesCount : PX.Data.BQL.BqlInt.Field<servicesCount> { }

        [PXInt]
        public virtual int? ServicesCount { get; set; }
        #endregion
        #region ServiceClassIDs
        public abstract class serviceClassIDs : PX.Data.IBqlField { }

        [PXInt]
        public virtual Array ServiceClassIDs { get; set; }
        #endregion
        #region BranchLocationDesc
        public abstract class branchLocationDesc : PX.Data.BQL.BqlString.Field<branchLocationDesc> { }

        [PXString]
        public virtual string BranchLocationDesc { get; set; }
        #endregion
        #region ServiceOrderTreeHelper
        #region TreeID
        public abstract class treeID : PX.Data.BQL.BqlInt.Field<treeID> { }

        [PXInt]
        public virtual int? TreeID { get; set; }
        #endregion
        #region Text
        public abstract class text : PX.Data.BQL.BqlString.Field<text> { }

        [PXString]
        public virtual string Text { get; set; }
        #endregion
        #region Leaf
        public abstract class leaf : PX.Data.BQL.BqlBool.Field<leaf> { }

        [PXBool]
        public virtual bool? Leaf { get; set; }
        #endregion
        #region Rows
        public abstract class rows : PX.Data.IBqlField { }

        public virtual object Rows { get; set; }
        #endregion
        #endregion
        #region CustomOrderDate
        public abstract class customOrderDate : PX.Data.IBqlField
        {
        }

        [PXString]
        public virtual string CustomOrderDate
        {
            get
            {
                //Value cannot be calculated with PXFormula attribute
                if (this.OrderDate != null)
                {
                    return this.OrderDate.ToString();
                }

                return string.Empty;
            }
        }
        #endregion
        #endregion

        #region UTC Fields
        #region SLAETAUTC
        public abstract class sLAETAUTC : PX.Data.BQL.BqlDateTime.Field<sLAETAUTC> { }

        [PXDBDateAndTime(UseTimeZone = false, PreserveTime = true, DisplayNameDate = "Deadline - SLA Date", DisplayNameTime = "Deadline - SLA Time")]
        [PXUIField(DisplayName = "Deadline - SLA")]
        public virtual DateTime? SLAETAUTC { get; set; }
        #endregion
        #endregion

        #region Tax Fields
        #region TaxZoneID
        public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }

        [PXDBString(10, IsUnicode = true)]
        [PXUIEnabled(typeof(
            Where<Current<FSSrvOrdType.behavior>, NotEqual<FSSrvOrdType.behavior.Values.internalAppointment>>))]
        [PXUIField(DisplayName = "Customer Tax Zone")]
        [PXSelector(typeof(TaxZone.taxZoneID), DescriptionField = typeof(TaxZone.descr), Filterable = true)]
        [PXFormula(typeof(Default<FSServiceOrder.branchID>))]
        [PXFormula(typeof(Default<FSServiceOrder.billLocationID>))]
        public virtual String TaxZoneID { get; set; }
        #endregion
        #region TaxCalcMode
        public abstract class taxCalcMode : PX.Data.BQL.BqlString.Field<taxCalcMode> { }
        [PXDBString(1, IsFixed = true)]
        [PXDefault(TaxCalculationMode.TaxSetting,
            typeof(Search<
                Location.cTaxCalcMode,
                Where<Location.bAccountID, Equal<Current<FSServiceOrder.billCustomerID>>,
                    And<Location.locationID, Equal<Current<FSServiceOrder.billLocationID>>>>>))]
        [PXFormula(typeof(Default<FSServiceOrder.billCustomerID>))]
        [PXFormula(typeof(Default<FSServiceOrder.billLocationID>))]
        [TaxCalculationMode.List]
        [PXUIField(DisplayName = "Tax Calculation Mode")]
        public virtual string TaxCalcMode { get; set; }
        #endregion

        #region CuryVatExemptTotal
        public abstract class curyVatExemptTotal : PX.Data.BQL.BqlDecimal.Field<curyVatExemptTotal> { }

        [PXUIVisible(typeof(FeatureInstalled<FeaturesSet.vATReporting>))]
        [PXDBCurrency(typeof(FSServiceOrder.curyInfoID), typeof(FSServiceOrder.vatExemptTotal))]
        [PXUIField(DisplayName = "VAT Exempt Total", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryVatExemptTotal { get; set; }
        #endregion
        #region VatExemptTotal
        public abstract class vatExemptTotal : PX.Data.BQL.BqlDecimal.Field<vatExemptTotal> { }

        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? VatExemptTotal { get; set; }
        #endregion
        #region CuryVatTaxableTotal
        public abstract class curyVatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<curyVatTaxableTotal> { }

        [PXUIVisible(typeof(FeatureInstalled<FeaturesSet.vATReporting>))]
        [PXDBCurrency(typeof(FSServiceOrder.curyInfoID), typeof(FSServiceOrder.vatTaxableTotal))]
        [PXUIField(DisplayName = "VAT Taxable Total", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryVatTaxableTotal { get; set; }
        #endregion
        #region VatTaxableTotal
        public abstract class vatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<vatTaxableTotal> { }

        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? VatTaxableTotal { get; set; }
        #endregion

        #region CuryTaxTotal
        public abstract class curyTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyTaxTotal> { }

        [PXDBCurrency(typeof(FSServiceOrder.curyInfoID), typeof(FSServiceOrder.taxTotal))]
        [PXUIField(DisplayName = "Estimated Tax Total", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryTaxTotal { get; set; }
        #endregion
        #region TaxTotal
        public abstract class taxTotal : PX.Data.BQL.BqlDecimal.Field<taxTotal> { }

        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? TaxTotal { get; set; }
        #endregion

        #region SkipExternalTaxCalculation
        public abstract class skipExternalTaxCalculation : PX.Data.BQL.BqlBool.Field<skipExternalTaxCalculation> { }

        [PXBool]
        [PXUnboundDefault(false)]
        [PXUIField(DisplayName = "Skip External Tax Calculation", Enabled = false)]
        public virtual Boolean? SkipExternalTaxCalculation { get; set; }
        #endregion
        #region IsTaxValid
        public abstract class isTaxValid : PX.Data.BQL.BqlBool.Field<isTaxValid> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Tax Is Up to Date", Enabled = false)]
        public virtual Boolean? IsTaxValid { get; set; }
        #endregion
        #endregion

        #region Discount fields
        #region CuryLineDocDiscountTotal
        public abstract class curyLineDocDiscountTotal : PX.Data.BQL.BqlDecimal.Field<curyLineDocDiscountTotal> { }
        //AC-162992 -> Refactor for adding missing fields required in SalesTax extension 
        [PXCurrency(typeof(curyInfoID))]
        [PXUIField(Enabled = false)]
        [PXUnboundDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryLineDocDiscountTotal { get; set; }
        #endregion

        #region CuryDocDisc
        public abstract class curyDocDisc : PX.Data.BQL.BqlDecimal.Field<curyDocDisc> { }
        protected Decimal? _CuryDocDisc;
        [PXCurrency(typeof(FSServiceOrder.curyInfoID), typeof(FSServiceOrder.docDisc))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Document Discount", Enabled = true)]
        public virtual Decimal? CuryDocDisc
        {
            get
            {
                return this._CuryDocDisc;
            }
            set
            {
                this._CuryDocDisc = value;
            }
        }
        #endregion
        #region DocDisc
        public abstract class docDisc : PX.Data.BQL.BqlDecimal.Field<docDisc> { }
        protected Decimal? _DocDisc;
        [PXBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Decimal? DocDisc
        {
            get
            {
                return this._DocDisc;
            }
            set
            {
                this._DocDisc = value;
            }
        }
        #endregion
        #region CuryDiscTot
        public abstract class curyDiscTot : PX.Data.BQL.BqlDecimal.Field<curyDiscTot> { }
        protected Decimal? _CuryDiscTot;
        [PXDBCurrency(typeof(FSServiceOrder.curyInfoID), typeof(FSServiceOrder.discTot))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Discount Total")]
        public virtual Decimal? CuryDiscTot
        {
            get
            {
                return this._CuryDiscTot;
            }
            set
            {
                this._CuryDiscTot = value;
            }
        }
        #endregion
        #region DiscTot
        public abstract class discTot : PX.Data.BQL.BqlDecimal.Field<discTot> { }
        protected Decimal? _DiscTot;
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Discount Total")]
        public virtual Decimal? DiscTot
        {
            get
            {
                return this._DiscTot;
            }
            set
            {
                this._DiscTot = value;
            }
        }
        #endregion
        #endregion

        #region CuryDocTotal
        public abstract class curyDocTotal : PX.Data.BQL.BqlDecimal.Field<curyDocTotal> { }
        [PXDependsOnFields(typeof(curyBillableOrderTotal), typeof(curyDiscTot), typeof(curyTaxTotal))]
        [PXDBCurrency(typeof(curyInfoID), typeof(docTotal))]
        [PXUIField(DisplayName = "Estimated Total", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryDocTotal { get; set; }
        #endregion
        #region DocTotal
        public abstract class docTotal : PX.Data.BQL.BqlDecimal.Field<docTotal> { }
        [PXDBDecimal(4)]
        [PXUIField(DisplayName = "Base Service Order Total", Enabled = false)]
        public virtual Decimal? DocTotal { get; set; }
        #endregion

        #region Profitability Fields
        #region CuryCostTotal
        public abstract class curyCostTotal : PX.Data.BQL.BqlDecimal.Field<curyCostTotal> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(costTotal))]
        [PXUIField(DisplayName = "Cost Total", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryCostTotal { get; set; }
        #endregion
        #region CostTotal
        public abstract class costTotal : PX.Data.BQL.BqlDecimal.Field<costTotal> { }

        [PXDBPriceCost()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CostTotal { get; set; }
        #endregion
        #region ProfitPercent
        public abstract class profitPercent : PX.Data.BQL.BqlDecimal.Field<profitPercent> { }

		[PXDecimal]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Profit Markup (%)", Enabled = false)]
		public virtual decimal? ProfitPercent { get; set; }
		#endregion
		#region ProfitMarginPercent
		public abstract class profitMarginPercent : PX.Data.BQL.BqlDecimal.Field<profitMarginPercent> { }

		[PXDecimal]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Profit Margin (%)", Enabled = false)]
		public virtual decimal? ProfitMarginPercent { get; set; }
		#endregion
		#endregion

		#region IsCalledFromQuickProcess
		public abstract class isCalledFromQuickProcess : PX.Data.BQL.BqlBool.Field<isCalledFromQuickProcess> { }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? IsCalledFromQuickProcess { get; set; }
        #endregion
        #region IsBilledOrClosed
        public virtual bool IsBilledOrClosed
        {
            get
            {
                return this.Billed == true
                        || this.Closed == true;
            }
        }
        #endregion

        #region FormCaptionDescription
        [PXString]
        public string FormCaptionDescription { get; set; }
        #endregion

        #region IsINReleaseProcess
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool IsINReleaseProcess { get; set; }
		#endregion

		#region CuryEstimatedBillableTotal
		public abstract class curyEstimatedBillableTotal : PX.Data.BQL.BqlDecimal.Field<curyEstimatedBillableTotal> { }

		[PXCurrency(typeof(curyInfoID), typeof(estimatedBillableTotal))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Estimated Billable Total", Enabled = false)]
		public virtual Decimal? CuryEstimatedBillableTotal { get; set; }
		#endregion
		#region EstimatedBillableTotal
		public abstract class estimatedBillableTotal : PX.Data.BQL.BqlDecimal.Field<estimatedBillableTotal> { }

		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Estimated Billable Total", Enabled = false)]
		public virtual Decimal? EstimatedBillableTotal { get; set; }
		#endregion


	}

	#region RelatedHelper
	[Serializable]
    public class RelatedServiceOrder : FSServiceOrder
    {
        public new abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

        public new abstract class sOID : PX.Data.BQL.BqlInt.Field<sOID> { }

        #region CuryInfoID
        public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
        [PXDBLong]
        public override Int64? CuryInfoID { get; set; }
        #endregion
    }
    #endregion
}
