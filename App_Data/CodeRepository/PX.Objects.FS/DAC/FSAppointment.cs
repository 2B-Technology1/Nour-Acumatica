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

using PX.Common;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data.WorkflowAPI;
using PX.Objects.AR;
using PX.Objects.CM.Extensions;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.TX;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    [PXCacheName(TX.TableName.APPOINTMENT)]
    [PXPrimaryGraph(typeof(AppointmentEntry))]
    [PXGroupMask(typeof(InnerJoinSingleTable<Customer, On<Customer.bAccountID, Equal<FSAppointment.customerID>, And<Match<Customer, Current<AccessInfo.userName>>>>>))]
    public partial class FSAppointment : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<FSAppointment>.By<srvOrdType, refNbr>
		{
			public static FSAppointment Find(PXGraph graph, string srvOrdType, string refNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, srvOrdType, refNbr, options);
		}
        public class UK : PrimaryKeyOf<FSAppointment>.By<appointmentID>
        {
            public static FSAppointment Find(PXGraph graph, int? appointmentID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, appointmentID, options);
        }

        public static class FK
        {
            public class Customer : AR.Customer.PK.ForeignKeyOf<FSAppointment>.By<customerID> { }
            public class Branch : GL.Branch.PK.ForeignKeyOf<FSAppointment>.By<branchID> { }
            public class ServiceOrderType : FSSrvOrdType.PK.ForeignKeyOf<FSAppointment>.By<srvOrdType> { }
            public class ServiceOrder : FSServiceOrder.PK.ForeignKeyOf<FSAppointment>.By<srvOrdType, soRefNbr> { }
            public class Project : PMProject.PK.ForeignKeyOf<FSAppointment>.By<projectID> { }
            public class DefaultTask : PMTask.PK.ForeignKeyOf<FSAppointment>.By<projectID, dfltProjectTaskID> { }
            public class WorkFlowStage : FSWFStage.PK.ForeignKeyOf<FSAppointment>.By<wFStageID> { }
            public class ServiceContract : FSServiceContract.PK.ForeignKeyOf<FSAppointment>.By<serviceContractID> { }
            public class Schedule : FSSchedule.PK.ForeignKeyOf<FSAppointment>.By<scheduleID> { }
            public class BillServiceContract : FSServiceContract.PK.ForeignKeyOf<FSAppointment>.By<billServiceContractID> { }
            public class TaxZone : Objects.TX.TaxZone.PK.ForeignKeyOf<FSAppointment>.By<taxZoneID> { }
            public class Route : FSRoute.PK.ForeignKeyOf<FSAppointment>.By<routeID> { }
            public class RouteDocument : FSRouteDocument.PK.ForeignKeyOf<FSAppointment>.By<routeDocumentID> { }
            public class Vehicle : FSVehicle.PK.ForeignKeyOf<FSAppointment>.By<vehicleID> { }
            public class CurrencyInfo : CM.CurrencyInfo.PK.ForeignKeyOf<FSAppointment>.By<curyInfoID> { }
			public class Currency : CM.Currency.PK.ForeignKeyOf<FSAppointment>.By<curyID> { }
            public class SalesPerson : AR.SalesPerson.PK.ForeignKeyOf<FSAppointment>.By<salesPersonID> { }
        }
        #endregion
        #region Events
        public class Events : PXEntityEvent<FSAppointment>.Container<Events>
        {
            public PXEntityEvent<FSAppointment> ServiceContractCleared;
            public PXEntityEvent<FSAppointment> ServiceContractPeriodAssigned;
            // TODO: Delete in the next major release
            public PXEntityEvent<FSAppointment> ServiceContractPeriodCleared;
            public PXEntityEvent<FSAppointment> RequiredServiceContractPeriodCleared;
            public PXEntityEvent<FSAppointment> AppointmentUnposted;
            public PXEntityEvent<FSAppointment> AppointmentPosted;
        }
        #endregion

        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXDBString(4, IsFixed = true, IsKey = true, InputMask = ">AAAA")]
        [PXDefault(typeof(Coalesce<
            Search<FSxUserPreferences.dfltSrvOrdType,
            Where<
                PX.SM.UserPreferences.userID.IsEqual<AccessInfo.userID.FromCurrent>>>,
            Search<FSSetup.dfltSrvOrdType>>))]
        [PXUIField(DisplayName = "Service Order Type", Visibility = PXUIVisibility.SelectorVisible)]
		[PXRestrictor(typeof(Where<FSSrvOrdType.active, Equal<True>>), null)]
		[FSSelectorSrvOrdTypeNOTQuote]
        [PXUIVerify(typeof(Where<Current<FSSrvOrdType.active>, Equal<True>>),
                    PXErrorLevel.Warning, TX.Error.SRVORDTYPE_INACTIVE, CheckOnRowSelected = true)]
        public virtual string SrvOrdType { get; set; }
        #endregion

		#region SrvOrdTypeCode
		/// <summary>
		/// TODO: AC-233462 Code Refactoring - Removing FSAppointment.SrvOrdTypeCode
		/// </summary>
		public abstract class srvOrdTypeCode : PX.Data.BQL.BqlString.Field<srvOrdTypeCode> { }
		[PXString]
		[PXFormula(typeof(srvOrdType))]
		[PX.Data.EP.PXFieldDescription]
		public virtual string SrvOrdTypeCode { get; set; }
		#endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

        [PXDBString(20, IsKey = true, IsUnicode = true, InputMask = "CCCCCCCCCCCCCCCCCCCC")]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Appointment Nbr.", Visibility = PXUIVisibility.SelectorVisible, Visible = true, Enabled = true)]
        [PXSelector(typeof(
            Search2<FSAppointment.refNbr,
            LeftJoin<FSServiceOrder,
                On<FSServiceOrder.sOID, Equal<FSAppointment.sOID>>,
            LeftJoin<Customer, 
                On<Customer.bAccountID, Equal<FSServiceOrder.customerID>>,
            LeftJoin<Location, 
                On<Location.bAccountID, Equal<FSServiceOrder.customerID>,
                    And<Location.locationID, Equal<FSServiceOrder.locationID>>>>>>,
            Where2<
            Where<
                FSAppointment.srvOrdType, Equal<Optional<FSAppointment.srvOrdType>>>,
                And<Where<
                    Customer.bAccountID, IsNull,
                    Or<Match<Customer, Current<AccessInfo.userName>>>>>>,
            OrderBy<
                Desc<FSAppointment.refNbr>>>),
                    new Type[] {
                                typeof(FSAppointment.refNbr),
                                typeof(Customer.acctCD),
                                typeof(Customer.acctName),
                                typeof(Location.locationCD),
                                typeof(FSAppointment.docDesc),
                                typeof(FSAppointment.status),
                                typeof(FSAppointment.scheduledDateTimeBegin)
                    })]
        [AppointmentAutoNumber(typeof(
            Search<FSSrvOrdType.srvOrdNumberingID,
            Where<
                FSSrvOrdType.srvOrdType, Equal<Optional<FSAppointment.srvOrdType>>>>),
            typeof(AccessInfo.businessDate))]
		[PX.Data.EP.PXFieldDescription]
        public virtual string RefNbr { get; set; }
        #endregion
        #region AppointmentID
        public abstract class appointmentID : PX.Data.BQL.BqlInt.Field<appointmentID> { }

        [PXDBIdentity]
        public virtual int? AppointmentID { get; set; }
        #endregion

        #region WorkflowTypeID
        [PXDBString(2, IsUnicode = false)]
        [PXDefault(typeof(Selector<srvOrdType, FSSrvOrdType.appointmentWorkflowTypeID>))]
        [PXFormula(typeof(Default<srvOrdType>))]
        [workflowTypeID.Values.List]
        [PXUIField(DisplayName = "Workflow Type")]
        public virtual string WorkflowTypeID { get; set; }
        public abstract class workflowTypeID : PX.Data.BQL.BqlString.Field<workflowTypeID>
        {
            public abstract class Values : ListField.ServiceOrderWorkflowTypes { }
        }
        #endregion
        #region SORefNbr
        public abstract class soRefNbr : PX.Data.BQL.BqlString.Field<soRefNbr> { }

        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Service Order Nbr.")]
        [FSSelectorSORefNbr_Appointment]
        public virtual string SORefNbr { get; set; }
        #endregion
        #region SOID
        public abstract class sOID : PX.Data.BQL.BqlInt.Field<sOID> { }

        [PXDBInt]
        public virtual int? SOID { get; set; }
        #endregion
        #region Attributes
        /// <summary>
        /// A service field, which is necessary for the <see cref="CSAnswers">dynamically 
        /// added attributes</see> defined at the <see cref="FSSrvOrdType">customer 
        /// class</see> level to function correctly.
        /// </summary>
        [CRAttributesField(typeof(FSAppointment.srvOrdType), typeof(FSAppointment.noteID))]
        public virtual string[] Attributes { get; set; }
        #endregion

        #region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Branch", Enabled = false, Visible = false, TabOrder = 0)]
        [PXDefault(typeof(AccessInfo.branchID))]
        [PXSelector(typeof(Search<Branch.branchID>), SubstituteKey = typeof(Branch.branchCD), DescriptionField = typeof(Branch.acctName))]
        public virtual Int32? BranchID { get; set; }
        #endregion
        #region CustomerID
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

        [PXDBInt]
        [PopupMessage]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Customer", Visibility = PXUIVisibility.SelectorVisible)]
        [PXRestrictor(typeof(Where<Customer.status, IsNull,
                Or<Customer.status, Equal<CustomerStatus.active>,
                Or<Customer.status, Equal<CustomerStatus.oneTime>>>>),
                PX.Objects.AR.Messages.CustomerIsInStatus, typeof(Customer.status))]
        [FSSelectorBAccountCustomerOrCombined]
        [PXForeignReference(typeof(FK.Customer))]
        public virtual int? CustomerID { get; set; }
        #endregion
        #region BillCustomerID
        public abstract class billCustomerID : PX.Data.BQL.BqlInt.Field<billCustomerID> { }

        [PXInt]
        [PXUIField(DisplayName = "Billing Customer")]
        public virtual int? BillCustomerID { get; set; }
        #endregion
        #region DocDesc
        public abstract class docDesc : PX.Data.BQL.BqlString.Field<docDesc> { }

        [PXDBString(Common.Constants.TranDescLength, IsUnicode = true)]
        [PXUIField(DisplayName = "Description", FieldName = "DocDesc", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string DocDesc { get; set; }
        #endregion
        #region ScheduledDateTimeBegin
        public abstract class scheduledDateTimeBegin : PX.Data.BQL.BqlDateTime.Field<scheduledDateTimeBegin> { }

        protected DateTime? _ScheduledDateTimeBegin;
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "Scheduled Start Date", DisplayNameTime = "Scheduled Start Time")]
        [PXDefault]
        [PXUIField(DisplayName = "Scheduled Start Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? ScheduledDateTimeBegin
        {
            get
            {
                return this._ScheduledDateTimeBegin;
            }

            set
            {
                this.ScheduledDateTimeBeginUTC = value;
                this._ScheduledDateTimeBegin = value;
            }
        }
        #endregion
        #region HandleManuallyScheduleTime
        public abstract class handleManuallyScheduleTime : PX.Data.BQL.BqlBool.Field<handleManuallyScheduleTime> { }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Handle Manually")]
        public virtual bool? HandleManuallyScheduleTime { get; set; }
        #endregion
        #region ScheduledDateTimeEnd
        public abstract class scheduledDateTimeEnd : PX.Data.BQL.BqlDateTime.Field<scheduledDateTimeEnd> { }

        protected DateTime? _ScheduledDateTimeEnd;
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "Scheduled End Date", DisplayNameTime = "Scheduled End Time")]
        [PXDefault]
        [PXUIEnabled(typeof(handleManuallyScheduleTime))]
        [PXUIField(DisplayName = "Scheduled End Date")]
        public virtual DateTime? ScheduledDateTimeEnd
        {
            get
            {
                return this._ScheduledDateTimeEnd;
            }

            set
            {
                this.ScheduledDateTimeEndUTC = value;
                this._ScheduledDateTimeEnd = value;
            }
        }
        #endregion

        #region ExecutionDate
        public abstract class executionDate : PX.Data.BQL.BqlDateTime.Field<executionDate> { }

        [PXDBDate]
        [PXDefault]
        [PXUIField(DisplayName = "Actual Start Date", Enabled = false)]
        public virtual DateTime? ExecutionDate { get; set; }
        #endregion
        #region ActualDateTimeBegin
        public abstract class actualDateTimeBegin : PX.Data.BQL.BqlDateTime.Field<actualDateTimeBegin> { }

        protected DateTime? _ActualDateTimeBegin;
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "Actual Start Date", DisplayNameTime = "Actual Start Time")]
        [PXUIField(DisplayName = "Actual Start Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? ActualDateTimeBegin
        {
            get
            {
                return this._ActualDateTimeBegin;
            }
            set
            {
                this._ActualDateTimeBegin = value;

                // Is there a task to delete these kind of fields??
                this.ActualDateTimeBeginUTC = value;

                if (_ActualDateTimeBegin != null)
                {
                    // This is temporary while deleting ExecutionDate
                    var newExecutionDate = new DateTime(_ActualDateTimeBegin.Value.Year, _ActualDateTimeBegin.Value.Month, _ActualDateTimeBegin.Value.Day);
                    if (ExecutionDate != newExecutionDate)
                    {
                        ExecutionDate = newExecutionDate;
                    }
                }
            }
        }
        #endregion
        #region HandleManuallyActualTime
        public abstract class handleManuallyActualTime : PX.Data.BQL.BqlBool.Field<handleManuallyActualTime> { }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Handle Manually")]
        public virtual bool? HandleManuallyActualTime { get; set; }
        #endregion
        #region ActualDateTimeEnd
        public abstract class actualDateTimeEnd : PX.Data.BQL.BqlDateTime.Field<actualDateTimeEnd> { }

        protected DateTime? _ActualDateTimeEnd;
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "Actual End Date", DisplayNameTime = "Actual End Time")]
        [PXUIEnabled(typeof(handleManuallyActualTime))]
        [PXUIField(DisplayName = "Actual End Date", Visibility = PXUIVisibility.Invisible)]
        public virtual DateTime? ActualDateTimeEnd
        {
            get
            {
                return this._ActualDateTimeEnd;
            }

            set
            {
                this.ActualDateTimeEndUTC = value;
                this._ActualDateTimeEnd = value;
            }
        }
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

        #region AutoDocDesc
        public abstract class autoDocDesc : PX.Data.BQL.BqlString.Field<autoDocDesc> { }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Service Description", Visible = true, Enabled = false)]
        public virtual string AutoDocDesc { get; set; }
        #endregion        
        #region Confirmed
        public abstract class confirmed : PX.Data.BQL.BqlBool.Field<confirmed> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Confirmed")]
        public virtual bool? Confirmed { get; set; }
        #endregion

        #region DeliveryNotes
        public abstract class deliveryNotes : PX.Data.BQL.BqlString.Field<deliveryNotes> { }

        [PXDBString(int.MaxValue, IsUnicode = true)]
        [PXUIField(DisplayName = "Delivery Notes")]
        public virtual string DeliveryNotes { get; set; }
        #endregion
        #region ProjectID
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

        [ProjectDefault]
        [PXUIEnabled(typeof(Where<Current<FSServiceOrder.sOID>, Less<Zero>,
                              And<Current<FSServiceContract.billingType>, 
                                    NotEqual<FSServiceContract.billingType.Values.standardizedBillings>>>))]
        [ProjectBase(typeof(FSServiceOrder.billCustomerID))]
        [PXRestrictor(typeof(Where<PMProject.isActive, Equal<True>>), PM.Messages.InactiveContract, typeof(PMProject.contractCD))]
        [PXRestrictor(typeof(Where<PMProject.isCancelled, Equal<False>>), PM.Messages.CancelledContract, typeof(PMProject.contractCD))]
        [PXForeignReference(typeof(FK.Project))]
        public virtual int? ProjectID { get; set; }
        #endregion
        #region DfltProjectTaskID
        public abstract class dfltProjectTaskID : PX.Data.BQL.BqlInt.Field<dfltProjectTaskID> { }

        [PXDBInt]
        [PXFormula(typeof(Default<projectID>))]
        [PXUIField(DisplayName = "Default Project Task", Visibility = PXUIVisibility.Visible, FieldClass = ProjectAttribute.DimensionName)]
        [PXDefault(typeof(Search<PMTask.taskID,
                            Where<PMTask.projectID, Equal<Current<projectID>>,
                            And<PMTask.isDefault, Equal<True>,
                            And<PMTask.isCompleted, Equal<False>,
                            And<PMTask.isCancelled, Equal<False>>>>>>),
                            PersistingCheck = PXPersistingCheck.Nothing)]
        [FSSelectorActive_AR_SO_ProjectTask(typeof(Where<PMTask.projectID, Equal<Current<projectID>>>))]
        [PXForeignReference(typeof(FK.DefaultTask))]
        public virtual int? DfltProjectTaskID { get; set; }
        #endregion
        #region LongDescr
        public abstract class longDescr : PX.Data.BQL.BqlString.Field<longDescr> { }

        [PXDBString(int.MaxValue, IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
        public virtual string LongDescr { get; set; }
        #endregion

        #region NotStarted
        public abstract class notStarted : PX.Data.BQL.BqlBool.Field<notStarted> { }

        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Not Started", Enabled = false)]
        public virtual bool? NotStarted { get; set; }
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
        #region InProcess
        public abstract class inProcess : PX.Data.BQL.BqlBool.Field<inProcess> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "In Process", Enabled = false)]
        public virtual bool? InProcess { get; set; }
        #endregion
        #region Paused
        public abstract class paused : PX.Data.BQL.BqlBool.Field<paused> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Paused", Enabled = false)]
        public virtual bool? Paused { get; set; }
        #endregion
        #region Completed
        public abstract class completed : PX.Data.BQL.BqlBool.Field<completed> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Completed", Enabled = false)]
        public virtual bool? Completed { get; set; }
        #endregion
        #region Closed
        public abstract class closed : PX.Data.BQL.BqlBool.Field<closed> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Closed", Enabled = false)]
        public virtual bool? Closed { get; set; }
        #endregion
        #region Canceled
        public abstract class canceled : PX.Data.BQL.BqlBool.Field<canceled> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Canceled", Enabled = false)]
        public virtual bool? Canceled { get; set; }
        #endregion
        #region Billed
        public abstract class billed : PX.Data.BQL.BqlBool.Field<billed> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Billed", Enabled = false)]
        public virtual bool? Billed { get; set; }
        #endregion
        #region GeneratedByContract
        public abstract class generatedByContract : PX.Data.BQL.BqlBool.Field<generatedByContract> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Generated by Contract", Enabled = false)]
        public virtual bool? GeneratedByContract { get; set; }
        #endregion
        #region UserConfirmedUnclosing
        public abstract class userConfirmedUnclosing : PX.Data.BQL.BqlBool.Field<userConfirmedUnclosing> { }

        [PXBool]
        public virtual bool? UserConfirmedUnclosing { get; set; }
        #endregion

        #region StartActionRunning
        public abstract class startActionRunning : PX.Data.BQL.BqlBool.Field<startActionRunning> { }

        [PXBool]
        [PXUnboundDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? StartActionRunning { get; set; }
        #endregion
        #region PauseActionRunning
        public abstract class pauseActionRunning : PX.Data.BQL.BqlBool.Field<pauseActionRunning> { }

        [PXBool]
        [PXUnboundDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? PauseActionRunning { get; set; }
        #endregion
        #region ResumeActionRunning
        public abstract class resumeActionRunning : PX.Data.BQL.BqlBool.Field<resumeActionRunning> { }

        [PXBool]
        [PXUnboundDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? ResumeActionRunning { get; set; }
        #endregion
        #region CompleteActionRunning
        public abstract class completeActionRunning : PX.Data.BQL.BqlBool.Field<completeActionRunning> { }

        [PXBool]
        [PXUnboundDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? CompleteActionRunning { get; set; }
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
        #region ReloadServiceOrderRelated
        public abstract class reloadServiceOrderRelated : PX.Data.BQL.BqlBool.Field<reloadServiceOrderRelated> { }

        [PXUnboundDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXBool]
        public virtual bool? ReloadServiceOrderRelated { get; set; }
        #endregion

        #region Status
        public abstract class status : PX.Data.BQL.BqlString.Field<status> 
        {
            public abstract class Values : ListField.AppointmentStatus { }
        }

        [PXDBString(1, IsFixed = true)]
        [PXDefault(status.Values.NotStarted)]
        [status.Values.List]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual string Status { get; set; }
        #endregion

        #region Effective Fields (based on the Document Status)
        #region AreActualFieldsActive
        public abstract class areActualFieldsActive : PX.Data.BQL.BqlBool.Field<areActualFieldsActive> { }

        [PXBool]
        [PXUnboundDefault(typeof(Switch<
                            Case<Where<
                                    reopenActionRunning, Equal<True>>,
                                False,
                            Case<Where<
                                    startActionRunning, Equal<True>,
                                    Or<inProcess, Equal<True>,
                                    Or<paused, Equal<True>,
                                    Or<completed, Equal<True>>>>>,
                                True>>,
                            False>))]
        public virtual bool? AreActualFieldsActive { get; set; }
        #endregion
        #region EffDocDate
        public abstract class effDocDate : PX.Data.BQL.BqlDateTime.Field<effDocDate> { }

        [PXDate(UseTimeZone = true)]
        [PXUIField(DisplayName = "Effective Document Date", Enabled = false)]
        [PXFormula(typeof(IIf<Where<areActualFieldsActive, Equal<False>>, scheduledDateTimeBegin, IsNull<actualDateTimeBegin, scheduledDateTimeBegin>>))]
        public virtual DateTime? EffDocDate { get; set; }
        #endregion
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
        #region LogLineCntr
        public abstract class logLineCntr : PX.Data.BQL.BqlInt.Field<logLineCntr> { }

        [PXDBInt()]
        [PXDefault(0)]
        public virtual Int32? LogLineCntr { get; set; }
        #endregion
        #region EmployeeLineCntr
        public abstract class employeeLineCntr : PX.Data.BQL.BqlInt.Field<employeeLineCntr> { }

        [PXDBInt()]
        [PXDefault(0)]
        public virtual Int32? EmployeeLineCntr { get; set; }
        #endregion
        #region PendingPOLineCntr
        public abstract class pendingPOLineCntr : PX.Data.BQL.BqlInt.Field<pendingPOLineCntr> { }

        [PXDBInt()]
        [PXDefault(0)]
        public virtual int? PendingPOLineCntr { get; set; }
        #endregion
		#region PendingApptPOLineCntr
		public abstract class pendingApptPOLineCntr : PX.Data.BQL.BqlInt.Field<pendingApptPOLineCntr> { }

		[PXDBInt()]
		[PXDefault(0)]
		public virtual int? PendingApptPOLineCntr { get; set; }
		#endregion
        #region APBillLineCntr
        public abstract class apBillLineCntr : PX.Data.BQL.BqlInt.Field<apBillLineCntr> { }

        [PXDBInt]
        [PXDefault(0)]
        public virtual int? APBillLineCntr { get; set; }
        #endregion
        #region StaffCntr
        public abstract class staffCntr : PX.Data.BQL.BqlInt.Field<staffCntr> { }

        [PXDBInt()]
        [PXDefault(0)]
        public virtual Int32? StaffCntr { get; set; }
        #endregion

        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

        [PXDefault]
        [PXUIField(DisplayName = "NoteID")]
        [PXSearchable(SM.SearchCategory.FS, "SM {0}: {1}", new Type[] { typeof(FSAppointment.srvOrdType), typeof(FSAppointment.refNbr) },
           new Type[] { typeof(Customer.acctCD), typeof(FSAppointment.srvOrdType), typeof(FSAppointment.refNbr), typeof(FSAppointment.soRefNbr),  typeof(FSAppointment.docDesc) },
           NumberFields = new Type[] { typeof(FSAppointment.refNbr) },
           Line1Format = "{0:d}{1}{2}", Line1Fields = new Type[] { typeof(FSAppointment.scheduledDateTimeBegin), typeof(FSAppointment.status), typeof(FSAppointment.soRefNbr) },
           Line2Format = "{0}", Line2Fields = new Type[] { typeof(FSAppointment.docDesc) },
           MatchWithJoin = typeof(InnerJoin<FSServiceOrder, On<FSServiceOrder.sOID, Equal<FSAppointment.sOID>>, InnerJoin<Customer, On<Customer.bAccountID, Equal<FSServiceOrder.customerID>>>>),
           SelectForFastIndexing = typeof(Select2<FSAppointment, InnerJoin<FSServiceOrder, On<FSServiceOrder.sOID, Equal<FSAppointment.sOID>>, InnerJoin<Customer, On<Customer.bAccountID, Equal<FSServiceOrder.customerID>>>>>)
        )]
        [PXNote(ShowInReferenceSelector = true)]
        public virtual Guid? NoteID { get; set; }
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
        [PXUIField(DisplayName = "CreatedByScreenID")]
        public virtual string CreatedByScreenID { get; set; }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        [PXDBCreatedDateTime]
        [PXUIField(DisplayName = "Created On")]
        public virtual DateTime? CreatedDateTime { get; set; }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        [PXDBLastModifiedByID]
        [PXUIField(DisplayName = "LastModifiedByID")]
        public virtual Guid? LastModifiedByID { get; set; }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        [PXDBLastModifiedByScreenID]
        [PXUIField(DisplayName = "LastModifiedByScreenID")]
        public virtual string LastModifiedByScreenID { get; set; }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        [PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = "Last Modified On")]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        [PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
        [PXUIField(DisplayName = "tstamp")]
        public virtual byte[] tstamp { get; set; }
        #endregion

        #region EstimatedDurationTotal
        // SetDisplayName in RouteDocumentMaint
        // SetDisplayName in RouteClosingMaint
        public abstract class estimatedDurationTotal : PX.Data.BQL.BqlInt.Field<estimatedDurationTotal> { }

        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
        [PXUIField(DisplayName = "Estimated Duration", Enabled = false)]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? EstimatedDurationTotal { get; set; }
        #endregion
        #region ActualDurationTotal
        public abstract class actualDurationTotal : PX.Data.BQL.BqlInt.Field<actualDurationTotal> { }

        [FSDBTimeSpanLongAllowNegative]
        [PXUIField(DisplayName = "Actual Duration", Enabled = false)]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? ActualDurationTotal { get; set; }
        #endregion

        #region DriveTime
        public abstract class driveTime : PX.Data.BQL.BqlInt.Field<driveTime> { }

        [PXDBInt(MinValue = 0)]
        [PXUIField(DisplayName = "Driving Time")]
        public virtual int? DriveTime { get; set; }
        #endregion
        #region MapLatitude
        public abstract class mapLatitude : PX.Data.BQL.BqlDecimal.Field<mapLatitude> { }

        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Latitude", Enabled = false)]
        public virtual decimal? MapLatitude { get; set; }
        #endregion
        #region MapLongitude
        public abstract class mapLongitude : PX.Data.BQL.BqlDecimal.Field<mapLongitude> { }

        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Longitude", Enabled = false)]
        public virtual decimal? MapLongitude { get; set; }
        #endregion
        #region RoutePosition
        public abstract class routePosition : PX.Data.BQL.BqlInt.Field<routePosition> { }

        [PXDBInt(MinValue = 1)]
        [PXUIField(DisplayName = "Route Position")]
        public virtual int? RoutePosition { get; set; }
        #endregion

        #region TimeLocked
        public abstract class timeLocked : PX.Data.BQL.BqlBool.Field<timeLocked> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Time Locked")]
        public virtual bool? TimeLocked { get; set; }
        #endregion
        #region ServiceContractID
        public abstract class serviceContractID : PX.Data.BQL.BqlInt.Field<serviceContractID> { }

        public int? _ServiceContractID;
        [PXDBInt]
        [PXSelector(typeof(Search<FSServiceContract.serviceContractID,
                           Where<
                                FSServiceContract.customerID, Equal<Current<FSServiceOrder.customerID>>>>), 
                           SubstituteKey = typeof(FSServiceContract.refNbr))]
        [PXUIField(DisplayName = "Source Service Contract ID", Enabled = false, FieldClass = "FSCONTRACT")]
        public virtual int? ServiceContractID 
        {
            get
            {
                return _ServiceContractID;
            }
            set
            {
                _ServiceContractID = value;
                GeneratedByContract = _ServiceContractID != null ? true : false;
            }
        }
        #endregion
        #region ScheduleID
        public abstract class scheduleID : PX.Data.BQL.BqlInt.Field<scheduleID> { }

        [PXDBInt]
        [PXSelector(typeof(Search<FSSchedule.scheduleID,
                           Where<
                                FSSchedule.entityType, Equal<ListField_Schedule_EntityType.Contract>,
                                And< FSSchedule.entityID, Equal<Current<FSServiceOrder.serviceContractID>>>>>),
                           SubstituteKey = typeof(FSSchedule.refNbr))]
        [PXUIField(DisplayName = "Source Schedule ID", Enabled = false, FieldClass = "FSCONTRACT")]
        public virtual int? ScheduleID { get; set; }
        #endregion
        #region OriginalAppointmentID
        public abstract class originalAppointmentID : PX.Data.BQL.BqlInt.Field<originalAppointmentID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Original Appointment ID", Enabled = false)]
        public virtual int? OriginalAppointmentID { get; set; }
        #endregion
        #region UnreachedCustomer
        public abstract class unreachedCustomer : PX.Data.BQL.BqlBool.Field<unreachedCustomer> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Unreached Customer")]
        public virtual bool? UnreachedCustomer { get; set; }
        #endregion
        #region Route ID
        public abstract class routeID : PX.Data.BQL.BqlInt.Field<routeID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Route ID", Enabled = true)]
        [FSSelectorRouteID]
        public virtual int? RouteID { get; set; }
        #endregion
        #region RouteDocumentID
        public abstract class routeDocumentID : PX.Data.BQL.BqlInt.Field<routeDocumentID> { }

        [PXDBInt]
        [PXSelector(typeof(Search<FSRouteDocument.routeDocumentID>), SubstituteKey = typeof(FSRouteDocument.refNbr))]
        [PXUIField(DisplayName = "Route Nbr.")]
        public virtual int? RouteDocumentID { get; set; }
        #endregion
        #region ValidatedByDispatcher
        public abstract class validatedByDispatcher : PX.Data.BQL.BqlBool.Field<validatedByDispatcher> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Validated by Dispatcher")]
        public virtual bool? ValidatedByDispatcher { get; set; }
        #endregion
        #region VehicleID
        public abstract class vehicleID : PX.Data.BQL.BqlInt.Field<vehicleID> { }

        [PXDBInt]
        [FSSelectorVehicle]
        [PXRestrictor(typeof(Where<FSVehicle.status, Equal<EPEquipmentStatus.EquipmentStatusActive>>),
                TX.Messages.VEHICLE_IS_INSTATUS, typeof(FSVehicle.status))]
        [PXUIField(DisplayName = "Vehicle ID", FieldClass = FSRouteSetup.RouteManagementFieldClass)]
        public virtual int? VehicleID { get; set; }
        #endregion
        #region GenerationID
        public abstract class generationID : PX.Data.BQL.BqlInt.Field<generationID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Generation ID")]
        public virtual int? GenerationID { get; set; }
        #endregion
        #region FinPeriodID
        public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

        [PXDBString(6, IsFixed = true)]
        [PXUIField(DisplayName = "Post Period")]
        public virtual string FinPeriodID { get; set; }
        #endregion
        #region WFStageID
        public abstract class wFStageID : PX.Data.BQL.BqlInt.Field<wFStageID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Workflow Stage")]
        [FSSelectorWorkflowStage(typeof(FSAppointment.srvOrdType))]
        [PXDefault(typeof(Search2<FSWFStage.wFStageID,
                    InnerJoin<FSSrvOrdType,
                        On<
                            FSSrvOrdType.srvOrdTypeID, Equal<FSWFStage.wFID>>>,
                    Where<
                        FSSrvOrdType.srvOrdType, Equal<Current<FSAppointment.srvOrdType>>>,
                    OrderBy<
                        Asc<FSWFStage.parentWFStageID,
                        Asc<FSWFStage.sortOrder>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIVisible(typeof(FSSetup.showWorkflowStageField.FromCurrent.IsEqual<True>))]
        public virtual int? WFStageID { get; set; }
        #endregion
        #region TimeRegistered
        public abstract class timeRegistered : PX.Data.BQL.BqlBool.Field<timeRegistered> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Approved Staff Times", Enabled = false, Visible = false)]
        public virtual bool? TimeRegistered { get; set; }
        #endregion        
        #region CustomerSignaturePath
        public abstract class CustomerSignaturePath : PX.Data.BQL.BqlString.Field<CustomerSignaturePath> { }

        [PXDBString(IsUnicode = true, InputMask = "")]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Customer Signature")]
        public virtual string customerSignaturePath { get; set; }
        #endregion
        #region customerSignedReport
        public abstract class customerSignedReport : PX.Data.BQL.BqlGuid.Field<customerSignedReport> { }

        [PXUIField(DisplayName = "Signed Report ID")]
        [PXDBGuid]
        public virtual Guid? CustomerSignedReport { get; set; }
        #endregion

        #region FullNameSignature
        public abstract class fullNameSignature : PX.Data.BQL.BqlString.Field<fullNameSignature> { }

        [PXDBString(255, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Full Name")]
        public virtual string FullNameSignature { get; set; }
        #endregion

        #region SalesPersonID
        public abstract class salesPersonID : PX.Data.BQL.BqlInt.Field<salesPersonID> { }

        [SalesPerson(DisplayName = "Salesperson")]
        [PXDefault(typeof(
            Coalesce<
            Search<FSServiceOrder.salesPersonID,
                Where<FSServiceOrder.sOID, Equal<Current<FSServiceOrder.sOID>>>>,
            Search<FSSrvOrdType.salesPersonID,
            Where<
                FSSrvOrdType.srvOrdType, Equal<Current<FSAppointment.srvOrdType>>>>>), 
            PersistingCheck = PXPersistingCheck.Nothing)]
        [PXForeignReference(typeof(FK.SalesPerson))]
        public virtual int? SalesPersonID { get; set; }
        #endregion
        #region Commissionable
        public abstract class commissionable : PX.Data.BQL.BqlBool.Field<commissionable> { }

        [PXDBBool]
        [PXDefault(typeof(
            Coalesce<
            Search<FSServiceOrder.commissionable,
                Where<FSServiceOrder.sOID, Equal<Current<FSServiceOrder.sOID>>>>,
            Search<FSSrvOrdType.commissionable,
                Where<FSSrvOrdType.srvOrdType, Equal<Current<FSAppointment.srvOrdType>>>>>), 
            PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Commissionable")]
        public virtual bool? Commissionable { get; set; }
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
        #region PostingStatusAPARSO
        public abstract class postingStatusAPARSO : ListField_Status_Posting
        {
        }

        [PXDBString(2, IsFixed = true)]
        [PXDefault(ID.Status_Posting.NOTHING_TO_POST)]
        [PXUIField(Visible = false)]
        public virtual string PostingStatusAPARSO { get; set; }
        #endregion
        #region PostingStatusIN
        public abstract class postingStatusIN : ListField_Status_Posting
        {
        }

        [PXDBString(2, IsFixed = true)]
        [PXDefault(ID.Status_Posting.NOTHING_TO_POST)]
        [PXUIField(Visible = false)]
        public virtual string PostingStatusIN { get; set; }
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

		#region GPSLatitudeStart
		public abstract class gPSLatitudeStart : PX.Data.BQL.BqlDecimal.Field<gPSLatitudeStart> { }

        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Latitude", Enabled = false)]
        public virtual decimal? GPSLatitudeStart { get; set; }
        #endregion
        #region GPSLongitudeStart
        public abstract class gPSLongitudeStart : PX.Data.BQL.BqlDecimal.Field<gPSLongitudeStart> { }

        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Longitude", Enabled = false)]
        public virtual decimal? GPSLongitudeStart { get; set; }
        #endregion

        #region GPSLatitudeComplete
        public abstract class gPSLatitudeComplete : PX.Data.BQL.BqlDecimal.Field<gPSLatitudeComplete> { }

        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Latitude", Enabled = false)]
        public virtual decimal? GPSLatitudeComplete { get; set; }
        #endregion
        #region GPSLongitudeComplete
        public abstract class gPSLongitudeComplete : PX.Data.BQL.BqlDecimal.Field<gPSLongitudeComplete> { }

        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Longitude", Enabled = false)]
        public virtual decimal? GPSLongitudeComplete { get; set; }
        #endregion

        #region EstimatedLineTotal
        public abstract class estimatedLineTotal : PX.Data.BQL.BqlDecimal.Field<estimatedLineTotal> { }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Estimated Total", Enabled = false)]
        public virtual decimal? EstimatedLineTotal { get; set; }
        #endregion
        #region CuryEstimatedLineTotal
        public abstract class curyEstimatedLineTotal : PX.Data.BQL.BqlDecimal.Field<curyEstimatedLineTotal> { }
        [PXDBCurrency(typeof(curyInfoID), typeof(estimatedLineTotal))]
        [PXUIField(DisplayName = "Estimated Total", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? CuryEstimatedLineTotal { get; set; }
        #endregion

        #region EstimatedCostTotal
        public abstract class estimatedCostTotal : PX.Data.BQL.BqlDecimal.Field<estimatedCostTotal> { }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(Visible = false, Enabled = false)]
        public virtual Decimal? EstimatedCostTotal { get; set; }
        #endregion
        #region CuryEstimatedCostTotal
        public abstract class curyEstimatedCostTotal : PX.Data.BQL.BqlDecimal.Field<curyEstimatedCostTotal> { }
        [PXDBCurrency(typeof(curyInfoID), typeof(estimatedCostTotal))]
        [PXUIField(DisplayName = "Estimated Cost Total", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? CuryEstimatedCostTotal { get; set; }
        #endregion

        #region LineTotal
        public abstract class lineTotal : PX.Data.BQL.BqlDecimal.Field<lineTotal> { }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Ext. Price Total", Enabled = false)]
        public virtual decimal? LineTotal { get; set; }
        #endregion
        #region CuryLineTotal
        public abstract class curyLineTotal : PX.Data.BQL.BqlDecimal.Field<curyLineTotal> { }
        [PXDBCurrency(typeof(curyInfoID), typeof(lineTotal))]
        [PXUIField(DisplayName = "Ext. Price Total", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? CuryLineTotal { get; set; }
        #endregion

        #region LogBillableTranAmountTotal
        public abstract class logBillableTranAmountTotal : PX.Data.BQL.BqlDecimal.Field<logBillableTranAmountTotal> { }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Billable Labor Total", Enabled = false)]
        public virtual Decimal? LogBillableTranAmountTotal { get; set; }
        #endregion
        #region CuryLogBillableTranAmountTotal
        public abstract class curyLogBillableTranAmountTotal : PX.Data.BQL.BqlDecimal.Field<curyLogBillableTranAmountTotal> { }
        [PXDBCurrency(typeof(curyInfoID), typeof(logBillableTranAmountTotal))]
        [PXUIField(DisplayName = "Billable Labor Total", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIVisible(typeof(Where2<
                                Where<
                                    Current<FSSrvOrdType.postTo>, Equal<FSPostTo.Projects>,
                                    And<Current<FSSrvOrdType.billingType>, Equal<FSSrvOrdType.billingType.CostAsCost>,
                                    And<Current<FSSrvOrdType.createTimeActivitiesFromAppointment>, Equal<True>,
                                    And<Current<FSSetup.enableEmpTimeCardIntegration>, Equal<True>>>>>,
                                And<
                                    FeatureInstalled<FeaturesSet.timeReportingModule>>>))]
        public virtual Decimal? CuryLogBillableTranAmountTotal { get; set; }
        #endregion

        #region BillableLineTotal
        public abstract class billableLineTotal : PX.Data.BQL.BqlDecimal.Field<billableLineTotal> { }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Billable Total", Enabled = false)]
        public virtual Decimal? BillableLineTotal { get; set; }
        #endregion
        #region CuryBillableLineTotal
        public abstract class curyBillableLineTotal : PX.Data.BQL.BqlDecimal.Field<curyBillableLineTotal> { }
        [PXDBCurrency(typeof(curyInfoID), typeof(billableLineTotal))]
        [PXUIField(DisplayName = "Actual Billable Total", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryBillableLineTotal { get; set; }
        #endregion

        #region BillServiceContractID
        public abstract class billServiceContractID : PX.Data.BQL.BqlInt.Field<billServiceContractID> { }

        [PXDBInt]
		[FSSelectorPPFRServiceContract(typeof(FSServiceOrder.customerID), typeof(FSServiceOrder.locationID))]
		[PXUIField(DisplayName = "Service Contract", FieldClass = "FSCONTRACT", IsReadOnly = true)]
		public virtual int? BillServiceContractID { get; set; }
        #endregion
        #region BillContractPeriodID 
        public abstract class billContractPeriodID : PX.Data.BQL.BqlInt.Field<billContractPeriodID> { }

        [PXDBInt]
        [FSSelectorContractBillingPeriod]
        [PXFormula(typeof(Default<FSServiceOrder.billCustomerID, FSAppointment.scheduledDateTimeEnd>))]
        [PXUIField(DisplayName = "Contract Period", Enabled = false)]
        public virtual int? BillContractPeriodID { get; set; }
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
        [PXUIField(Enabled = false)]
        public virtual Decimal? CostTotal { get; set; }
        #endregion
        #region ProfitPercent
        public abstract class profitPercent : PX.Data.BQL.BqlDecimal.Field<profitPercent> { }

        [PXDecimal]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Profit Markup (%)", Enabled = false)]
        [PXFormula(typeof(
            Switch<
                Case<Where<curyCostTotal, Equal<decimal0>>, Null>,
                Mult<
                    Div<
                        Sub<
							curyActualBillableTotal, curyCostTotal>,
                        curyCostTotal>,
                    decimal100>>))]
        public virtual decimal? ProfitPercent { get; set; }
		#endregion
		#region ProfitMarginPercent
		public abstract class profitMarginPercent : PX.Data.BQL.BqlDecimal.Field<profitMarginPercent> { }

		[PXDecimal]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Profit Margin (%)", Enabled = false)]
		[PXFormula(typeof(
			Switch<
				Case<Where<curyActualBillableTotal, Equal<decimal0>>, Null>,
				Mult<
					Div<
						Sub<
							curyActualBillableTotal, curyCostTotal>,
						curyActualBillableTotal>,
					decimal100>>))]
		public virtual decimal? ProfitMarginPercent { get; set; }
		#endregion
		#endregion

		#region Tax Fields
		#region CuryVatExemptTotal
		public abstract class curyVatExemptTotal : PX.Data.BQL.BqlDecimal.Field<curyVatExemptTotal> { }

        [PXUIVisible(typeof(FeatureInstalled<FeaturesSet.vATReporting>))]
        [PXDBCurrency(typeof(FSAppointment.curyInfoID), typeof(FSAppointment.vatExemptTotal))]
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
        [PXDBCurrency(typeof(FSAppointment.curyInfoID), typeof(FSAppointment.vatTaxableTotal))]
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

        #region TaxZoneID
        public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }

        [PXDBString(10, IsUnicode = true)]
        [PXUIEnabled(typeof(Where<Current<FSSrvOrdType.behavior>, NotEqual<FSSrvOrdType.behavior.Values.internalAppointment>>))]
        [PXUIField(DisplayName = "Customer Tax Zone")]
        [PXSelector(typeof(TaxZone.taxZoneID), DescriptionField = typeof(TaxZone.descr), Filterable = true)]
        [PXFormula(typeof(Default<FSAppointment.branchID>))]
        [PXFormula(typeof(Default<FSServiceOrder.billLocationID>))]
        public virtual String TaxZoneID { get; set; }
        #endregion

        #region TaxCalcMode
        public abstract class taxCalcMode : PX.Data.BQL.BqlString.Field<taxCalcMode> { }
        [PXDBString(1, IsFixed = true)]
        [PXDefault(TaxCalculationMode.TaxSetting,
            typeof(Search<Location.cTaxCalcMode,
                Where<Location.bAccountID, Equal<Current<FSServiceOrder.billCustomerID>>,
                    And<Location.locationID, Equal<Current<FSServiceOrder.billLocationID>>>>>))]
        [PXFormula(typeof(Default<FSServiceOrder.billCustomerID>))]
        [PXFormula(typeof(Default<FSServiceOrder.billLocationID>))]
        [TaxCalculationMode.List]
        [PXUIField(DisplayName = "Tax Calculation Mode")]
        public virtual string TaxCalcMode { get; set; }
        #endregion

        #region TaxTotal
        public abstract class taxTotal : PX.Data.BQL.BqlDecimal.Field<taxTotal> { }

        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? TaxTotal { get; set; }
        #endregion
        #region CuryTaxTotal
        public abstract class curyTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyTaxTotal> { }

        [PXDBCurrency(typeof(FSAppointment.curyInfoID), typeof(FSAppointment.taxTotal))]
        [PXUIField(DisplayName = "Actual Tax Total", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryTaxTotal { get; set; }
        #endregion

        #region DiscTot
        public abstract class discTot : PX.Data.BQL.BqlDecimal.Field<discTot> { }

        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Discount Total")]
        public virtual Decimal? DiscTot { get; set; }
        #endregion
        #region CuryDiscTot
        public abstract class curyDiscTot : PX.Data.BQL.BqlDecimal.Field<curyDiscTot> { }

        protected Decimal? _CuryDiscTot;
        [PXDBCurrency(typeof(FSAppointment.curyInfoID), typeof(FSAppointment.discTot))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Discount Total", Enabled = false)]
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

        #region DocTotal
        public abstract class docTotal : PX.Data.BQL.BqlDecimal.Field<docTotal> { }
        [PXDBDecimal(4)]
        [PXUIField(DisplayName = "Base Order Total", Enabled = false)]
        public virtual Decimal? DocTotal { get; set; }
        #endregion
        #region CuryDocTotal
        public abstract class curyDocTotal : PX.Data.BQL.BqlDecimal.Field<curyDocTotal> { }
        [PXDependsOnFields(typeof(curyBillableLineTotal), typeof(curyDiscTot), typeof(curyTaxTotal))]
        [PXDBCurrency(typeof(curyInfoID), typeof(docTotal))]
        [PXUIField(DisplayName = "Invoice Total", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryDocTotal { get; set; }
        #endregion
        #region CuryLineDocDiscountTotal
        public abstract class curyLineDocDiscountTotal : PX.Data.BQL.BqlDecimal.Field<curyLineDocDiscountTotal> { }
        //AC-162992 -> Refactor for adding missing fields required in SalesTax extension 
        [PXCurrency(typeof(curyInfoID))]
        [PXUIField(Enabled = false)]
        [PXUnboundDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryLineDocDiscountTotal { get; set; }
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
        #region CuryDocDisc
        public abstract class curyDocDisc : PX.Data.BQL.BqlDecimal.Field<curyDocDisc> { }
        protected Decimal? _CuryDocDisc;
        [PXCurrency(typeof(FSAppointment.curyInfoID), typeof(FSAppointment.docDisc))]
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

        #region MinLogTimeBegin
        public abstract class minLogTimeBegin : PX.Data.BQL.BqlDateTime.Field<minLogTimeBegin> { }

        protected DateTime? _MinLogTimeBegin;
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "Min Log Date Begin", DisplayNameTime = "Min Log Time Begin")]
        [PXUIField(DisplayName = "Min Log Time Begin")]
        public virtual DateTime? MinLogTimeBegin
        {
            get
            {
                return this._MinLogTimeBegin;
            }

            set
            {
                this._MinLogTimeBegin = value;
            }
        }
        #endregion

        #region MaxLogTimeEnd
        public abstract class maxLogTimeEnd : PX.Data.BQL.BqlDateTime.Field<maxLogTimeEnd> { }

        protected DateTime? _MaxLogTimeEnd;
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "Max Log Date End", DisplayNameTime = "Max Log Time End")]
        [PXUIField(DisplayName = "Max Log Time End")]
        public virtual DateTime? MaxLogTimeEnd
        {
            get
            {
                return this._MaxLogTimeEnd;
            }

            set
            {
                this._MaxLogTimeEnd = value;
            }
        }
        #endregion

        #region WaitingForParts
        public abstract class waitingForParts : PX.Data.BQL.BqlBool.Field<waitingForParts> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXFormula(typeof(IIf<Where<pendingPOLineCntr, Greater<int0>>, True, False>))]
        [PXUIVisible(typeof(Where<Current<FSSrvOrdType.postTo>, Equal<FSPostTo.Sales_Order_Invoice>,
                               Or<Current<FSSrvOrdType.postTo>, Equal<FSPostTo.Sales_Order_Module>,
                               Or<Current<FSSrvOrdType.postTo>, Equal<FSPostTo.Projects>>>>))]
        [PXUIField(DisplayName = "Waiting for Purchased Items", Enabled = false, FieldClass = "DISTINV")]
        public virtual bool? WaitingForParts { get; set; }
        #endregion
        #region Finished
        public abstract class finished : PX.Data.BQL.BqlBool.Field<finished> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Finished")]
        public virtual bool? Finished { get; set; }
        #endregion


		#region AppCompletedBillableTotal
        public abstract class appCompletedBillableTotal : PX.Data.BQL.BqlDecimal.Field<appCompletedBillableTotal> { }

        [PXDecimal]
        [PXUIField(DisplayName = "Appointment Billable Total", Enabled = false)]
        [PXFormula(typeof(
            Switch<
                Case<Where<inProcess, Equal<True>,
                        Or<completed, Equal<True>, 
                        Or<closed, Equal<True>>>>, curyDocTotal>,
                Null>))]
        public virtual Decimal? AppCompletedBillableTotal { get; set; }
        #endregion

        #region intTravelInProcess
        public abstract class intTravelInProcess : PX.Data.BQL.BqlInt.Field<intTravelInProcess> { }

        [PXInt()]
        public virtual Int32? IntTravelInProcess
        {
            set
            {
                TravelInProcess = value > 0 ? true : false;
            }
        }
        #endregion
        #region TravelInProcess
        public abstract class travelInProcess : PX.Data.BQL.BqlBool.Field<travelInProcess> { }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Travel in Process", Enabled = false)]
        public virtual bool? TravelInProcess { get; set; }
        #endregion

        #region PrimaryDriver
        public abstract class primaryDriver : PX.Data.BQL.BqlInt.Field<primaryDriver> { }

        [PXDBInt]
        [FSSelector_StaffMember_ServiceOrderProjectID]
        [PXUIField(DisplayName = "Primary Driver", FieldClass = "ROUTEOPTIMIZER")]
        public virtual Int32? PrimaryDriver { get; set; }
        #endregion

        #region ROOptimizationStatus
        public abstract class rOOptimizationStatus : ListField_Status_ROOptimization
        {
        }

        [PXDBString(2, IsFixed = true)]
        [rOOptimizationStatus.ListAtrribute]
		[PXFormula(typeof(Default<FSAppointment.primaryDriver, FSAppointment.scheduledDateTimeBegin, FSAppointment.scheduledDateTimeEnd>))]
        [PXDefault(typeof(rOOptimizationStatus.NotOptimized), PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Optimization Result", FieldClass = "ROUTEOPTIMIZER", Enabled = false)]
        public virtual string ROOptimizationStatus { get; set; }
        #endregion

        #region ROOriginalSortOrder
        public abstract class rOOriginalSortOrder : PX.Data.BQL.BqlInt.Field<rOOriginalSortOrder> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Nbr. in Original Sequence", Enabled = false, FieldClass = "ROUTEOPTIMIZER")]
        public virtual Int32? ROOriginalSortOrder { get; set; }
        #endregion

        #region ROSortOrder
        public abstract class rOSortOrder : PX.Data.BQL.BqlInt.Field<rOSortOrder> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Nbr. in Optimized Sequence", Enabled = false, FieldClass = "ROUTEOPTIMIZER")]
        public virtual Int32? ROSortOrder { get; set; }
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

        #region MemoryHelper

        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

            [PXBool]
            [PXUIField(DisplayName = "Selected")]
            public virtual bool? Selected { get; set; }
            #endregion

            #region Mem_InvoiceDate
        public abstract class mem_InvoiceDate : PX.Data.BQL.BqlDateTime.Field<mem_InvoiceDate> { }

            [PXDate]
            public virtual DateTime? Mem_InvoiceDate { get; set; }
            #endregion

            #region Mem_InvoiceDocType
            public abstract class mem_InvoiceDocType : PX.Data.BQL.BqlString.Field<mem_InvoiceDocType> { }

            [PXString]
            [PXUIField(DisplayName = "Invoice Doc Type", Enabled = false)]
            public virtual string Mem_InvoiceDocType { get; set; }
            #endregion

            #region Mem_BatchNbr
            public abstract class mem_BatchNbr : PX.Data.BQL.BqlString.Field<mem_BatchNbr> { }

            [PXString(15, IsFixed = true)]
            [PXUIField(DisplayName = "Batch Nbr.", Enabled = false)]
            public virtual string Mem_BatchNbr { get; set; }
            #endregion

            #region Mem_InvoiceRef    
            public abstract class mem_InvoiceRef : PX.Data.BQL.BqlString.Field<mem_InvoiceRef> { }

            [PXString(15)]
            [PXUIField(DisplayName = "Invoice Ref. Nbr.", Enabled = false)]
            public virtual string Mem_InvoiceRef { get; set; }
            #endregion

            #region Mem_ScheduledHours
            public abstract class mem_ScheduledHours : PX.Data.BQL.BqlDecimal.Field<mem_ScheduledHours> { }

            [PXDecimal(2)]
            [PXUIField(DisplayName = "Scheduled Hours", Enabled = false)]
            public virtual decimal? Mem_ScheduledHours { get; set; }
            #endregion

            #region Mem_AppointmentHours
            public abstract class mem_AppointmentHours : PX.Data.BQL.BqlDecimal.Field<mem_AppointmentHours> { }

            [PXDecimal(2)]
            [PXUIField(DisplayName = "Appointment Hours", Enabled = false)]
            public virtual decimal? Mem_AppointmentHours { get; set; }
            #endregion

            #region Mem_IdleRate
            public abstract class mem_IdleRate : PX.Data.BQL.BqlDecimal.Field<mem_IdleRate> { }

            [PXDecimal(2)]
            [PXUIField(DisplayName = "Idle Rate (%)", Enabled = false)]
            public virtual decimal? Mem_IdleRate { get; set; }
            #endregion

            #region Mem_OccupationalRate
            public abstract class mem_OccupationalRate : PX.Data.BQL.BqlDecimal.Field<mem_OccupationalRate> { }

            [PXDecimal(2)]
            [PXUIField(DisplayName = "Occupational Rate (%)", Enabled = false)]
            public virtual decimal? Mem_OccupationalRate { get; set; }
            #endregion

            #region Mem_isBeingCloned
            // Useful for skipping unwanted appointment logic during the cloning process    
            [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
            public virtual bool? isBeingCloned { get; set; }
            #endregion

            #region Mem_ReturnValueID
            [PXInt]
            public virtual int? Mem_ReturnValueID { get; set; }
            #endregion

            #region Mem_LastRouteDocumentID
            [PXInt]
            public virtual int? Mem_LastRouteDocumentID { get; set; }
            #endregion

            #region Mem_BusinessDateTime
            public abstract class mem_BusinessDateTime : PX.Data.BQL.BqlDateTime.Field<mem_BusinessDateTime> { }

            [PXDateAndTime]
            public virtual DateTime? Mem_BusinessDateTime
            {
                get
                {
                    return PXTimeZoneInfo.Now;
                }
            }
            #endregion

            #region ScheduledDuration
            public abstract class scheduledDuration : PX.Data.BQL.BqlInt.Field<scheduledDuration> { }

            [PXTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
            [PXFormula(typeof(DateDiff<FSAppointment.scheduledDateTimeBegin, FSAppointment.scheduledDateTimeEnd, DateDiff.minute>))]
            [PXUIField(DisplayName = "Scheduled Duration", Enabled = false)]
            public virtual int? ScheduledDuration { get; set; }
            #endregion

            #region ActualDuration
            public abstract class actualDuration : PX.Data.BQL.BqlInt.Field<actualDuration> { }

            [PXTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
            [PXDefault(typeof(IIf<
                                Where<
                                    DateDiff<FSAppointment.actualDateTimeBegin, FSAppointment.actualDateTimeEnd, DateDiff.minute>,
                                    Greater<SharedClasses.int_0>>, 
                                DateDiff<FSAppointment.actualDateTimeBegin, FSAppointment.actualDateTimeEnd, DateDiff.minute>,
                                SharedClasses.int_0>), PersistingCheck = PXPersistingCheck.Nothing)]
            [PXFormula(typeof(IIf<
                                Where<
                                    DateDiff<FSAppointment.actualDateTimeBegin, FSAppointment.actualDateTimeEnd, DateDiff.minute>,
                                    Greater<SharedClasses.int_0>>, 
                                DateDiff<FSAppointment.actualDateTimeBegin, FSAppointment.actualDateTimeEnd, DateDiff.minute>,
                                SharedClasses.int_0>))]
            [PXUIField(DisplayName = "Actual Duration", Enabled = false)]
            public virtual int? ActualDuration { get; set; }
            #endregion

            #region Mem_ActualDateTimeBegin_Time
        public abstract class mem_ActualDateTimeBegin_Time : PX.Data.BQL.BqlInt.Field<mem_ActualDateTimeBegin_Time> { }

            [PXTimeList(1, 1440)]
            [PXInt]
            [PXUIField(DisplayName = "Actual Start Time")]
            public virtual int? Mem_ActualDateTimeBegin_Time
            {
                get
                {
                    //Value cannot be calculated with PXFormula attribute
                    if (ActualDateTimeBegin != null && ActualDateTimeBegin.Value != null)
                    {
                        return (int?)ActualDateTimeBegin.Value.TimeOfDay.TotalMinutes;
                    }

                    return null;
                }
            }
            #endregion

            #region Mem_ActualDateTimeEnd_Time
            public abstract class mem_ActualDateTimeEnd_Time : PX.Data.BQL.BqlInt.Field<mem_ActualDateTimeEnd_Time> { }

            [PXTimeList(1, 1440)]
            [PXInt]
            [PXUIField(DisplayName = "Actual End Time")]
            public virtual int? Mem_ActualDateTimeEnd_Time
            {
                get
                {
                    //Value cannot be calculated with PXFormula attribute
                    if (ActualDateTimeEnd != null && ActualDateTimeEnd.Value != null)
                    {
                        return (int?)ActualDateTimeEnd.Value.TimeOfDay.TotalMinutes;
                    }

                    return null;
                }
            }
            #endregion

            #region WildCard_AssignedEmployeesList
            public abstract class wildCard_AssignedEmployeesList : PX.Data.BQL.BqlString.Field<wildCard_AssignedEmployeesList> { }

            [PXString]
            [PXUIField(DisplayName = "Assigned employees list", Enabled = false)]
            public virtual string WildCard_AssignedEmployeesList { get; set; }
            #endregion
        
            #region WildCard_AssignedEmployeesCellPhoneList
            public abstract class wildCard_AssignedEmployeesCellPhoneList : PX.Data.BQL.BqlString.Field<wildCard_AssignedEmployeesCellPhoneList> { }

            [PXString]
            [PXUIField(DisplayName = "Assigned employees cells list", Enabled = false)]
            public virtual string WildCard_AssignedEmployeesCellPhoneList { get; set; }
            #endregion
        
            #region WildCard_CustomerPrimaryContact
            /// <summary>
            /// This memory field is used to store the names from the contact(s) associated to a given customer.
            /// </summary>
            public abstract class wildCard_CustomerPrimaryContact : PX.Data.BQL.BqlString.Field<wildCard_CustomerPrimaryContact> { }

            [PXString]
            [PXUIField(DisplayName = "Customer primary contact", Enabled = false)]
            public virtual string WildCard_CustomerPrimaryContact { get; set; }
            #endregion

            #region WildCard_CustomerPrimaryContactCell
            /// <summary>
            /// This memory field is used to store the cellphones from the contact(s) associated to a given customer.
            /// </summary>
            public abstract class wildCard_CustomerPrimaryContactCell : PX.Data.BQL.BqlString.Field<wildCard_CustomerPrimaryContactCell> { }

            [PXString]
            [PXUIField(DisplayName = "Customer primary contact cell", Enabled = false)]
            public virtual string WildCard_CustomerPrimaryContactCell { get; set; }
            #endregion

            #region Mem_ScheduledTimeBegin
            public abstract class mem_ScheduledTimeBegin : PX.Data.BQL.BqlDateTime.Field<mem_ScheduledTimeBegin> { }

            [PXDateAndTime(UseTimeZone = true, DisplayMask = "t")]
            public virtual DateTime? Mem_ScheduledTimeBegin 
            {
                get
                {
                    return this.ScheduledDateTimeBegin;
                }
            }
            #endregion

        #region ScheduledDateBegin
        public abstract class scheduledDateBegin : PX.Data.BQL.BqlDateTime.Field<scheduledDateBegin> { }

        [PXDate]
        public virtual DateTime? ScheduledDateBegin 
        {
            get
            {
                return this.ScheduledDateTimeBegin;
            }
        }
        #endregion

                #region Mem_CompanyLogo
                //public abstract class mem_CompanyLogo : PX.Data.BQL.BqlString.Field<mem_CompanyLogo>
                //{
                //}
                //[PXString]
                //[PXUIField(DisplayName = "Logo", Enabled = false)]
                //public virtual string Mem_CompanyLogo
                //{
                //    get
                //    {
                //        StringBuilder names = new StringBuilder();
                //        // SD-7259 what to do in this cases?
                //        names.Append("<img src='http://66.35.42.244/Hoveround_4_20/icons/logo.jpg'>");
                //        return names.ToString();
                //    }
                //}
                #endregion

                #region Mem_ActualDateTime_Month
        public abstract class mem_ActualDateTime_Month : ListField_Month
            {
            }

            [PXString]
            [mem_ActualDateTime_Month.ListAtrribute]
            [PXDefault(ID.Months.JANUARY)]
            [PXUIField(DisplayName = "Month")]
            public virtual string Mem_ActualDateTime_Month
            {
                get
                {
                    //Value cannot be calculated with PXFormula attribute
                    if (ScheduledDateTimeBegin != null && ScheduledDateTimeBegin.Value != null)
                    {
                        return SharedFunctions.GetMonthOfYearInStringByID(ScheduledDateTimeBegin.Value.Month);
                    }

                    return null;
                }
            }
            #endregion

            #region Mem_ActualDateTime_Year
            public abstract class mem_ActualDateTime_Year : PX.Data.BQL.BqlInt.Field<mem_ActualDateTime_Year> { }

            [PXInt]
            [PXUIField(DisplayName = "Year")]
            public virtual int? Mem_ActualDateTime_Year
            {
                get
                {
                    //Value cannot be calculated with PXFormula attribute
                    if (ScheduledDateTimeBegin != null && ScheduledDateTimeBegin.Value != null)
                    {
                        return (int?)ScheduledDateTimeBegin.Value.Year;
                    }

                    return null;
                }
            }
            #endregion

            #region IsRouteAppoinment
            public abstract class isRouteAppoinment : PX.Data.BQL.BqlBool.Field<isRouteAppoinment> { }

            [PXBool]
            [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
            [PXUIField(Visible = false)]
            public virtual bool? IsRouteAppoinment { get; set; }
            #endregion

            #region IsPrepaymentEnable
            public abstract class isPrepaymentEnable : PX.Data.BQL.BqlBool.Field<isPrepaymentEnable> { }

            [PXBool]
            [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
            [PXUIField(Visible = false)]
            public virtual bool? IsPrepaymentEnable { get; set; }
            #endregion

            #region IsReassigned
            public abstract class isReassigned : PX.Data.BQL.BqlBool.Field<isReassigned> { }

            [PXBool]
            [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
            public virtual bool? IsReassigned { get; set; }
            #endregion

            #region Mem_SMequipmentID
            public abstract class mem_SMequipmentID : PX.Data.BQL.BqlInt.Field<mem_SMequipmentID> { }

            [PXInt]
            public virtual int? Mem_SMequipmentID { get; set; }
            #endregion

            #region ActualDurationTotalReport
            public abstract class actualDurationTotalReport : PX.Data.BQL.BqlInt.Field<actualDurationTotalReport> { }

            [PXInt]
            [PXFormula(typeof(FSAppointment.actualDurationTotal))]
            public virtual int? ActualDurationTotalReport { get; set; }
            #endregion

            #region AppointmentRefReport
            public abstract class appointmentRefReport : PX.Data.BQL.BqlInt.Field<appointmentRefReport> { }

            [PXInt]
            [PXSelector(typeof(Search<FSAppointment.refNbr,
                               Where<
                                    FSAppointment.soRefNbr, Equal<Optional<FSAppointment.soRefNbr>>>>),
                               SubstituteKey = typeof(FSAppointment.refNbr),
                               DescriptionField = typeof(FSAppointment.refNbr))]
            public virtual int? AppointmentRefReport { get; set; }
        #endregion

            #region Mem_GPSLatitudeLongitude    
            public abstract class mem_GPSLatitudeLongitude : PX.Data.BQL.BqlString.Field<mem_GPSLatitudeLongitude> { }

            [PXString(255)]
            [PXUIField(DisplayName = "GPS Latitude Longitude", Enabled = false)]
            public virtual string Mem_GPSLatitudeLongitude { get; set; }
            #endregion
        #endregion

        #region UTC Fields
        #region ActualDateTimeBeginUTC
        public abstract class actualDateTimeBeginUTC : PX.Data.BQL.BqlDateTime.Field<actualDateTimeBeginUTC> { }

        [PXDBDateAndTime(UseTimeZone = false, PreserveTime = true, DisplayNameDate = "Actual Date Time Begin", DisplayNameTime = "Actual Start Time")]
        [PXUIField(DisplayName = "Actual Date")]
        public virtual DateTime? ActualDateTimeBeginUTC { get; set; }
        #endregion
        #region ActualDateTimeEndUTC
        public abstract class actualDateTimeEndUTC : PX.Data.BQL.BqlDateTime.Field<actualDateTimeEndUTC> { }

        [PXDBDateAndTime(UseTimeZone = false, PreserveTime = true, DisplayNameDate = "Actual Date Time End", DisplayNameTime = "Actual End Time")]
        [PXUIField(DisplayName = "Actual Date End", Visibility = PXUIVisibility.Invisible)]
        public virtual DateTime? ActualDateTimeEndUTC { get; set; }
        #endregion
        #region ScheduledDateTimeBeginUTC
        public abstract class scheduledDateTimeBeginUTC : PX.Data.BQL.BqlDateTime.Field<scheduledDateTimeBeginUTC> { }

        [PXDBDateAndTime(UseTimeZone = false, PreserveTime = true, DisplayNameDate = "Scheduled Date", DisplayNameTime = "Scheduled Start Time")]
        [PXUIField(DisplayName = "Scheduled Date")]
        public virtual DateTime? ScheduledDateTimeBeginUTC { get; set; }
        #endregion
        #region ScheduledDateTimeEndUTC
        public abstract class scheduledDateTimeEndUTC : PX.Data.BQL.BqlDateTime.Field<scheduledDateTimeEndUTC> { }

        [PXDBDateAndTime(UseTimeZone = false, PreserveTime = true, DisplayNameDate = "Scheduled End Date", DisplayNameTime = "Scheduled End Time")]
        [PXUIField(DisplayName = "Scheduled Date End", Visibility = PXUIVisibility.Invisible)]
        public virtual DateTime? ScheduledDateTimeEndUTC { get; set; }
        #endregion
        #endregion

        #region IsCalledFromQuickProcess
        public abstract class isCalledFromQuickProcess : PX.Data.BQL.BqlBool.Field<isCalledFromQuickProcess> { }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? IsCalledFromQuickProcess { get; set; }
        #endregion
        #region IsPosted
        public abstract class isPosted : PX.Data.BQL.BqlBool.Field<isPosted> { }

        [PXBool]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Switch<
                Case<Where<
                        FSAppointment.postingStatusAPARSO, NotEqual<FSAppointment.postingStatusAPARSO.Posted>,
                        And<FSAppointment.postingStatusIN, NotEqual<FSAppointment.postingStatusIN.Posted>>>,
                False>,
                True>))]
        public virtual bool? IsPosted { get; set; }
        #endregion
        #region Unbound fields to enable/disable buttons
        #region TravelCanBeStarted
        public abstract class travelCanBeStarted : PX.Data.BQL.BqlBool.Field<travelCanBeStarted> { }

        [PXBool]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "TravelCanBeStarted", Enabled = false, Visible = false, Visibility = PXUIVisibility.Invisible)]
        [PXFormula(typeof(Switch<
                Case<Where<
                        FSAppointment.closed, NotEqual<True>,
                        And<FSAppointment.canceled, NotEqual<True>,
                        And<FSAppointment.hold, NotEqual<True>>>>,
                True>,
                False>))]
        public virtual bool? TravelCanBeStarted { get; set; }
        #endregion
        #region TravelCanBeCompleted
        public abstract class travelCanBeCompleted : PX.Data.BQL.BqlBool.Field<travelCanBeCompleted> { }

        [PXBool]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "TravelCanBeCompleted", Enabled = false, Visible = false, Visibility = PXUIVisibility.Invisible)]
        [PXFormula(typeof(Switch<
                Case<Where<
                        FSAppointment.travelCanBeStarted, Equal<True>,
                        And<FSAppointment.travelInProcess, Equal<True>>>,
                True>,
                False>))]
        public virtual bool? TravelCanBeCompleted { get; set; }
        #endregion
        #endregion

        #region MustUpdateServiceOrder
        public abstract class mustUpdateServiceOrder : PX.Data.BQL.BqlBool.Field<mustUpdateServiceOrder> { }

        [PXBool]
        [PXUnboundDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? MustUpdateServiceOrder { get; set; }
        #endregion

        #region IsBilledOrClosed
        public virtual bool IsBilledOrClosed
        {
            get
            {
                return this.IsPosted == true
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

		#region TrackTimeChanged
		public new abstract class trackTimeChanged : PX.Data.BQL.BqlBool.Field<trackTimeChanged> { }

		/// <summary>
		/// Specifies (if set to <see langword="true" />) that there is inconsistency with the trackTime flags in log records.
		/// </summary>
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(Visible = false)]
		public virtual bool? TrackTimeChanged { get; set; }
		#endregion

		#region CuryActualBillableTotal
		public abstract class curyActualBillableTotal : PX.Data.BQL.BqlDecimal.Field<curyActualBillableTotal> { }
		[PXCurrency(typeof(curyInfoID), typeof(actualBillableTotal))]
		[PXUIField(DisplayName = "Actual Billable Total", Enabled = false)]
		public virtual Decimal? CuryActualBillableTotal { get; set; }
		#endregion

		#region ActualBillableTotal
		public abstract class actualBillableTotal : PX.Data.BQL.BqlDecimal.Field<actualBillableTotal> { }
		[PXDecimal(4)]
		[PXUIField(DisplayName = "Actual Billable Total", Enabled = false)]
		public virtual Decimal? ActualBillableTotal { get; set; }
		#endregion

	}
}
