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
using PX.Objects.CM.Extensions;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.TM;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    [PXCacheName(TX.TableName.FSLOG)]
    public class FSAppointmentLog : FSLog
    {
        #region Keys
        public new class PK : PrimaryKeyOf<FSAppointmentLog>.By<docType, docRefNbr, lineNbr>
        {
            public static FSAppointmentLog Find(PXGraph graph, string docType, string docRefNbr, int? lineNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, docType, docRefNbr, lineNbr, options);
        }
        public new class UK : PrimaryKeyOf<FSAppointmentLog>.By<logID>
        {
            public static FSAppointmentLog Find(PXGraph graph, int? logID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, logID, options);
        }

        public new static class FK
        {
            public class ServiceOrderType : FSSrvOrdType.PK.ForeignKeyOf<FSAppointmentLog>.By<docType> { }
            public class Appointment : FSAppointment.PK.ForeignKeyOf<FSAppointmentLog>.By<docType, docRefNbr> { }
            public class Staff : BAccount.PK.ForeignKeyOf<FSAppointmentLog>.By<bAccountID> { }
            public class EarningType : EP.EPEarningType.PK.ForeignKeyOf<FSAppointmentLog>.By<earningType> { }
            public class CostCode : PMCostCode.PK.ForeignKeyOf<FSAppointmentLog>.By<costCodeID> { }
            public class Project : PMProject.PK.ForeignKeyOf<FSAppointmentLog>.By<projectID> { }
            public class Task : PMTask.PK.ForeignKeyOf<FSAppointmentLog>.By<projectID, projectTaskID> { }
            public class LaborItem : IN.InventoryItem.PK.ForeignKeyOf<FSAppointmentLog>.By<laborItemID> { }
            public class CurrencyInfo : CM.CurrencyInfo.PK.ForeignKeyOf<FSAppointmentLog>.By<curyInfoID> { }
            public class WorkGorupID : EPCompanyTree.PK.ForeignKeyOf<FSAppointmentLog>.By<workgroupID> { }
            public class TimeCard : EPTimeCard.PK.ForeignKeyOf<FSAppointmentLog>.By<timeCardCD> { }
        }
        #endregion

        #region LogID
        public new abstract class logID : PX.Data.BQL.BqlInt.Field<logID> { }

        [PXDBIdentity]
        public override int? LogID { get; set; }
        #endregion
        #region DocType
        public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

        [PXDBString(4, IsKey = true, IsFixed = true)]
        [PXUIField(DisplayName = "Service Order Type", Visible = false, Enabled = false)]
        [PXDefault(typeof(FSAppointment.srvOrdType))]
        [PXSelector(typeof(Search<FSSrvOrdType.srvOrdType>), CacheGlobal = true)]
        public override string DocType { get; set; }
        #endregion
        #region DocRefNbr
        public new abstract class docRefNbr : PX.Data.BQL.BqlString.Field<docRefNbr> { }

        [PXDBString(20, IsKey = true, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Appointment Nbr.", Visible = false, Enabled = false)]
        [PXDBDefault(typeof(FSAppointment.refNbr), DefaultForUpdate = false)]
        [PXParent(typeof(Select<FSAppointment,
                            Where<FSAppointment.srvOrdType, Equal<Current<docType>>,
                                And<FSAppointment.refNbr, Equal<Current<docRefNbr>>>>>))]
        public override string DocRefNbr { get; set; }
        #endregion
        
        #region DocID
        public new abstract class docID : PX.Data.BQL.BqlInt.Field<docID> { }

        [PXDBInt]
        [PXDBDefault(typeof(FSAppointment.appointmentID))]
        [PXUIField(DisplayName = "Appointment Ref. Nbr.", Visible = false, Enabled = false)]
        public override int? DocID { get; set; }
        #endregion
        #region LineNbr
        public new abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        [PXDBInt(IsKey = true)]
        [PXLineNbr(typeof(FSAppointment.logLineCntr))]
        public override int? LineNbr { get; set; }
        #endregion
        #region BAccountID
        public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Staff Member")]
        [FSSelector_StaffMember_ServiceOrderProjectID]
        public override int? BAccountID { get; set; }
        #endregion
        #region BAccountType
        public new abstract class bAccountType : PX.Data.BQL.BqlString.Field<bAccountType> { }

        [PXDBString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Staff Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [CR.BAccountType.ListAttribute]
        public override string BAccountType { get; set; }
        #endregion
        #region DetLineRef
        public new abstract class detLineRef : PX.Data.BQL.BqlString.Field<detLineRef> { }

        [PXDBString(4, IsFixed = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIRequired(typeof(Where<bAccountID, IsNull>))]
        [FSSelectorAppointmentSODetID(typeof(Where2<
                                                Where<Current<FSAppointment.notStarted>, Equal<False>,
                                                        Or<Where<Current<FSAppointment.startActionRunning>, Equal<True>,
                                                            And<Current<FSAppointment.notStarted>, Equal<True>>>>>,
                                                  Or<FSAppointmentDet.isTravelItem, Equal<True>>>))]
        [PXRestrictor(typeof(Where<FSAppointmentDet.status, NotEqual<FSAppointmentDet.status.Canceled>,
                               And<FSAppointmentDet.status, NotEqual<FSAppointmentDet.status.NotFinished>,
                               And<FSAppointmentDet.status, NotEqual<FSAppointmentDet.status.NotPerformed>,
                               And<FSAppointmentDet.status, NotEqual<FSAppointmentDet.status.waitingForPO>,
                               And<FSAppointmentDet.status, NotEqual<FSAppointmentDet.status.requestForPO>>>>>>), 
                     TX.Error.INVALID_DETAIL_LINE_REF_BY_STATUS, 
                     typeof(FSAppointmentDet.lineRef), typeof(FSAppointmentDet.status))]
        [PXUIField(DisplayName = TX.FSAppointmentLogColumns.DetLineRef)]
        public override string DetLineRef { get; set; }
        #endregion
        #region ItemType
        public new abstract class itemType : PX.Data.BQL.BqlString.Field<itemType>
        {
            public abstract class Values : ListField_Log_ItemType { }
        }

        [PXDBString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Log Type", Enabled = false)]
        [itemType.Values.List]
        [PXDefault(typeof(Switch<
                Case<Where<
                        Current<FSAppointment.notStarted>, Equal<True>,
                        And<Current<FSAppointment.startActionRunning>, Equal<False>>>,
                    FSAppointmentLog.itemType.Values.travel>,
                FSAppointmentLog.itemType.Values.staff>),
            PersistingCheck = PXPersistingCheck.NullOrBlank)]
        public override string ItemType { get; set; }
        #endregion
        #region Status
        public new abstract class status : ListField_Status_Log { }

        [PXDBString(1, IsFixed = true)]
        [status.ListAtrribute]
        [PXDefault(typeof(Switch<
                            Case<Where<Current<FSAppointment.completed>, Equal<True>,
                                    And<itemType, NotEqual<itemType.Values.travel>>>,
                                status.Completed>,
                            status.InProcess>),
            PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Log Line Status")]
        public override string Status { get; set; }
        #endregion

        #region Travel
        public abstract class travel : PX.Data.BQL.BqlBool.Field<travel> { }
        [PXBool]
        [PXFormula(typeof(Switch<Case<Where<itemType, Equal<itemType.Values.travel>>, True>, False>))]
        [PXUnboundFormula(typeof(Switch<
                                    Case<
                                        Where<
                                            travel, Equal<True>, 
                                            And<
                                                Where<
                                                    status, Equal<status.InProcess>,
                                                Or<
                                                    status, Equal<status.Paused>>>>>,
                                        int1>,
                                    int0>),
                          typeof(MaxCalc<FSAppointment.intTravelInProcess>))]
        [PXUIField(DisplayName = "Travel")]
        public virtual bool? Travel { get; set; }
        #endregion
        #region DateTimeBegin
        public new abstract class dateTimeBegin : PX.Data.BQL.BqlDateTime.Field<dateTimeBegin> { }

        [PXDefault]
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "Start Date", DisplayNameTime = "Start Time")]
        [PXUIField(DisplayName = "Start Date")]
        public override DateTime? DateTimeBegin { get; set; }
        #endregion
        #region DateTimeEnd
        public new abstract class dateTimeEnd : PX.Data.BQL.BqlDateTime.Field<dateTimeEnd> { }

        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIVerify(typeof(Where<dateTimeBegin, IsNull,
                           Or<dateTimeEnd, IsNull,
                           Or<dateTimeEnd, GreaterEqual<dateTimeBegin>>>>),
                    PXErrorLevel.Error, TX.Error.END_TIME_LESSER_THAN_START_TIME)]
        [PXUIRequired(typeof(Where<timeDuration, GreaterEqual<Zero>,
                             And<
                                 Where<
                                     status, Equal<status.Completed>,
                                     Or<status, Equal<status.Paused>>>>>))]
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "End Date", DisplayNameTime = "End Time")]
        [PXUIField(DisplayName = "End Date")]
        public override DateTime? DateTimeEnd { get; set; }
        #endregion
        #region TimeDuration
        public new abstract class timeDuration : PX.Data.BQL.BqlInt.Field<timeDuration> { }

        [FSDBTimeSpanLongAllowNegative]
        [PXFormula(typeof(Switch<
                                Case<
                                    Where<dateTimeBegin, IsNotNull,
                                        And<dateTimeEnd, IsNotNull,
                                        And<dateTimeEnd, GreaterEqual<dateTimeBegin>>>>,
                                    Sub<dateTimeEnd, dateTimeBegin>>,
                                    Zero>))]
        [PXUIField(DisplayName = "Duration")]
        public override int? TimeDuration { get; set; }
        #endregion
        #region ApprovedTime
        public new abstract class approvedTime : PX.Data.BQL.BqlBool.Field<approvedTime> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Approved", Visible = false, Enabled = false)]
        [PXUIVisible(typeof(Where<Current<FSSrvOrdType.createTimeActivitiesFromAppointment>, Equal<True>,
                                And<Current<FSSetup.enableEmpTimeCardIntegration>, Equal<True>,
                                And<FeatureInstalled<FeaturesSet.timeReportingModule>>>>))]
        public override bool? ApprovedTime { get; set; }
        #endregion
        #region CuryInfoID
        public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }

        [PXDBLong]
        [CurrencyInfo(typeof(FSAppointment.curyInfoID))]
        public override Int64? CuryInfoID { get; set; }
        #endregion
        #region Descr
        public new abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

        [PXDBString(Common.Constants.TranDescLength, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Description")]
        public override string Descr { get; set; }
        #endregion
        
        #region EarningType
        public new abstract class earningType : PX.Data.BQL.BqlString.Field<earningType> { }

        [PXDBString(EPEarningType.typeCD.Length, IsUnicode = true, InputMask = EPEarningType.typeCD.InputMask)]
        [PXDefault(typeof(Coalesce<
                            Search<
                                FSAppointmentEmployee.earningType,
                                Where<FSAppointmentEmployee.appointmentID, Equal<Current<FSAppointmentLog.docID>>,
                                And<FSAppointmentEmployee.employeeID, Equal<Current<FSAppointmentLog.bAccountID>>,
                                And<    
                                    Where2<
                                        Where<FSAppointmentEmployee.serviceLineRef, IsNull, And<Current<FSAppointmentLog.detLineRef>, IsNull>>,
                                        Or<FSAppointmentEmployee.serviceLineRef, Equal<Current<FSAppointmentLog.detLineRef>>>>>>>>,
                            Search2<
                                FSxService.dfltEarningType,
                                InnerJoin<FSAppointmentDet,
                                On<FSAppointmentDet.appointmentID, Equal<Current<FSAppointmentLog.docID>>,
                                    And<FSAppointmentDet.lineRef, Equal<Current<FSAppointmentLog.detLineRef>>>>>,
                                Where<InventoryItem.inventoryID, Equal<FSAppointmentDet.inventoryID>,
                                    And<BAccountType.employeeType, Equal<Current<bAccountType>>>>>,
                            Search<FSSrvOrdType.dfltEarningType,
                                Where<FSSrvOrdType.srvOrdTypeID, Equal<Current<FSSrvOrdType.srvOrdTypeID>>,
                                    And<BAccountType.employeeType, Equal<Current<bAccountType>>>>>>),
             PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(EPEarningType.typeCD))]
        [PXUIField(DisplayName = "Earning Type")]
        [PXUIVisible(typeof(Where<Current<FSSrvOrdType.createTimeActivitiesFromAppointment>, Equal<True>,
                                And<Current<FSSetup.enableEmpTimeCardIntegration>, Equal<True>,
                                And<FeatureInstalled<FeaturesSet.timeReportingModule>>>>))]
        [PXUIEnabled(typeof(Where<Current<FSSrvOrdType.createTimeActivitiesFromAppointment>, Equal<True>,
                                And<Current<FSSetup.enableEmpTimeCardIntegration>, Equal<True>,
                                And<FeatureInstalled<FeaturesSet.timeReportingModule>>>>))]
        [PXRestrictor(typeof(Where<EPEarningType.isActive, Equal<True>>), EP.Messages.EarningTypeInactive, typeof(EPEarningType.typeCD))]
        public override string EarningType { get; set; }
        #endregion
        #region KeepDateTimes
        public new abstract class keepDateTimes : PX.Data.BQL.BqlBool.Field<keepDateTimes> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIEnabled(typeof(Where<Current<FSSrvOrdType.allowManualLogTimeEdition>, Equal<True>>))]
        [PXUIVisible(typeof(Where<Current<FSSrvOrdType.allowManualLogTimeEdition>, Equal<True>>))]
        [PXUIField(DisplayName = "Manage Time Manually")]
        public override bool? KeepDateTimes { get; set; }
        #endregion
        #region LaborItemID
        public new abstract class laborItemID : PX.Data.BQL.BqlInt.Field<laborItemID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Labor Item ID")]
        [PXDimensionSelector(InventoryAttribute.DimensionName,
                             typeof(Search<InventoryItem.inventoryID,
                                        Where<InventoryItem.itemType, Equal<INItemTypes.laborItem>,
                                        And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>,
                                        And<Match<Current<AccessInfo.userName>>>>>>),
                             typeof(InventoryItem.inventoryCD))]
        [PXDefault(typeof(Coalesce<
                            Search<
                                FSAppointmentEmployee.laborItemID,
                                Where<FSAppointmentEmployee.appointmentID, Equal<Current<FSAppointmentLog.docID>>,
                                    And<FSAppointmentEmployee.employeeID, Equal<Current<FSAppointmentLog.bAccountID>>,
                                    And<
                                    Where2<
                                        Where<FSAppointmentEmployee.serviceLineRef, IsNull, And<Current<FSAppointmentLog.detLineRef>, IsNull>>,
                                        Or<FSAppointmentEmployee.serviceLineRef, Equal<Current<FSAppointmentLog.detLineRef>>>>>>>>,
                            Search<
                                EPEmployee.labourItemID, 
                                Where<EPEmployee.bAccountID, Equal<Current<FSAppointmentLog.bAccountID>>>>>),
                            PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<bAccountID, detLineRef>))]
        [PXForeignReference(typeof(FK.LaborItem))]
        public override int? LaborItemID { get; set; }
        #endregion
        #region LineRef
        public new abstract class lineRef : PX.Data.BQL.BqlString.Field<lineRef> { }

        [FSDBLineRef(typeof(lineNbr))]
        [PXDBString(3, IsFixed = true)]
        [PXUIField(DisplayName = "Log Ref. Nbr.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public override string LineRef { get; set; }
        #endregion
        #region ProjectID
        public new abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

        [PXDefault(typeof(Coalesce<
                            Search<
                                FSAppointmentEmployee.dfltProjectID,
                                Where<FSAppointmentEmployee.appointmentID, Equal<Current<FSAppointmentLog.docID>>,
                                    And<FSAppointmentEmployee.employeeID, Equal<Current<FSAppointmentLog.bAccountID>>,
                                    And<Where2<
                                        Where<FSAppointmentEmployee.serviceLineRef, IsNull,
                                            And<Current<FSAppointmentLog.detLineRef>, IsNull>>,
                                        Or<FSAppointmentEmployee.serviceLineRef, Equal<Current<FSAppointmentLog.detLineRef>>>>>>>>,
                            Search<
                                FSAppointmentDet.projectID,
                                Where<FSAppointmentDet.appointmentID, Equal<Current<FSAppointmentLog.docID>>,
                                    And<FSAppointmentDet.lineRef, Equal<Current<FSAppointmentLog.detLineRef>>>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<bAccountID, detLineRef>))]
        [ProjectBase(typeof(FSServiceOrder.billCustomerID), Visible = false)]
        [PXRestrictor(typeof(Where<PMProject.isActive, Equal<True>>), PM.Messages.InactiveContract, typeof(PMProject.contractCD))]
        [PXRestrictor(typeof(Where<PMProject.isCancelled, Equal<False>>), PM.Messages.CancelledContract, typeof(PMProject.contractCD))]
        [PXForeignReference(typeof(FK.Project))]
        public override int? ProjectID { get; set; }
        #endregion
        #region ProjectTaskID
        public new abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Project Task", FieldClass = ProjectAttribute.DimensionName)]
        [FSSelectorActive_AR_SO_ProjectTask(typeof(Where<PMTask.projectID, Equal<Current<projectID>>>))]
        [PXDefault(typeof(Coalesce<
                            Search<
                                FSAppointmentEmployee.dfltProjectTaskID,
                                Where<FSAppointmentEmployee.appointmentID, Equal<Current<FSAppointmentLog.docID>>,
                                    And<FSAppointmentEmployee.employeeID, Equal<Current<FSAppointmentLog.bAccountID>>,
                                    And<
                                    Where2<
                                        Where<FSAppointmentEmployee.serviceLineRef, IsNull, And<Current<FSAppointmentLog.detLineRef>, IsNull>>,
                                        Or<FSAppointmentEmployee.serviceLineRef, Equal<Current<FSAppointmentLog.detLineRef>>>>>>>>,
                            Search<
                                FSAppointmentDet.projectTaskID,
                                Where<FSAppointmentDet.appointmentID, Equal<Current<FSAppointmentLog.docID>>,
                                    And<FSAppointmentDet.lineRef, Equal<Current<FSAppointmentLog.detLineRef>>>>>,
                            Search<
                               FSAppointment.dfltProjectTaskID,
                               Where<FSAppointment.appointmentID, Equal<Current<FSAppointmentLog.docID>>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<bAccountID, detLineRef>))]
        [PXForeignReference(typeof(FK.Task))]
        public override int? ProjectTaskID { get; set; }
        #endregion
        #region CostCodeID
        public new abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }

        [SMCostCode(typeof(skipCostCodeValidation), null, typeof(projectTaskID))]
        [PXForeignReference(typeof(FK.CostCode))]
        [PXDefault(typeof(Coalesce<
                            Search<
                                FSAppointmentEmployee.costCodeID,
                                Where<FSAppointmentEmployee.appointmentID, Equal<Current<FSAppointmentLog.docID>>,
                                    And<FSAppointmentEmployee.employeeID, Equal<Current<FSAppointmentLog.bAccountID>>,
                                    And<
                                    Where2<
                                        Where<FSAppointmentEmployee.serviceLineRef, IsNull, And<Current<FSAppointmentLog.detLineRef>, IsNull>>,
                                        Or<FSAppointmentEmployee.serviceLineRef, Equal<Current<FSAppointmentLog.detLineRef>>>>>>>>,
                            Search<
                                FSAppointmentDet.costCodeID,
                                Where<FSAppointmentDet.appointmentID, Equal<Current<FSAppointmentLog.docID>>,
                                    And<FSAppointmentDet.lineRef, Equal<Current<FSAppointmentLog.detLineRef>>>>>,
                            Search2<
                               FSSrvOrdType.dfltCostCodeID,
                               LeftJoin<PMProject,
                                    On<PMProject.nonProject, Equal<True>,
                                    And<PMProject.contractID, Equal<Current<projectID>>>>>,
                               Where<FSSrvOrdType.srvOrdType, Equal<Current<FSSrvOrdType.srvOrdType>>,
                                   And<PMProject.contractID, IsNull, 
                                   And<Current<FSAppointmentLog.projectID>, IsNotNull>>>>>),
                            PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<bAccountID, detLineRef>))]

        public override int? CostCodeID { get; set; }
        #endregion
        #region TrackTime
        public new abstract class trackTime : PX.Data.BQL.BqlBool.Field<trackTime> { }

        [PXDBBool]
        [PXDefault(false, typeof(Coalesce<
                            Search<
                                FSAppointmentEmployee.trackTime,
                                Where<FSAppointmentEmployee.appointmentID, Equal<Current<FSAppointmentLog.docID>>,
                                    And<FSAppointmentEmployee.employeeID, Equal<Current<FSAppointmentLog.bAccountID>>,
                                    And<
                                    Where2<
                                        Where<FSAppointmentEmployee.serviceLineRef, IsNull, And<Current<FSAppointmentLog.detLineRef>, IsNull>>,
                                        Or<FSAppointmentEmployee.serviceLineRef, Equal<Current<FSAppointmentLog.detLineRef>>>>>>>>,
                            Search<
                                FSSrvOrdType.createTimeActivitiesFromAppointment,
                                Where<
                                    FSSrvOrdType.srvOrdType, Equal<Current<docType>>,
                                    And<Current<FSSetup.enableEmpTimeCardIntegration>, Equal<True>,
                                    And<Current<bAccountID>, IsNotNull,
                                    And<Current<bAccountType>, Equal<BAccountType.employeeType>,
                                    And<FeatureInstalled<FeaturesSet.timeReportingModule>>>>>>>>))]
        [PXUIField(DisplayName = "Track Time")]
        [PXUIVisible(typeof(Where<Current<FSSrvOrdType.createTimeActivitiesFromAppointment>, Equal<True>,
                                And<Current<FSSetup.enableEmpTimeCardIntegration>, Equal<True>,
                                And<FeatureInstalled<FeaturesSet.timeReportingModule>>>>))]
        [PXUIEnabled(typeof(Where<Current<FSSrvOrdType.createTimeActivitiesFromAppointment>, Equal<True>,
                                And<Current<FSSetup.enableEmpTimeCardIntegration>, Equal<True>,
                                And<FeatureInstalled<FeaturesSet.timeReportingModule>>>>))]
        public override bool? TrackTime { get; set; }
        #endregion
        #region TrackOnService
        public new abstract class trackOnService : PX.Data.BQL.BqlBool.Field<trackOnService> { }

        [PXDBBool]
        [PXUIField(DisplayName = "Add to Actual Duration")]
        [PXFormula(typeof(Default<bAccountID, travel, detLineRef>))]
        [PXUIEnabled(typeof(Where<detLineRef, IsNotNull>))]
        public override bool? TrackOnService { get; set; }
        #endregion
        #region TimeCardCD
        public new abstract class timeCardCD : PX.Data.BQL.BqlString.Field<timeCardCD> { }

        [PXDBString(10, IsUnicode = true, InputMask = ">CCCCCCCCCC")]
        [PXUIField(DisplayName = "Time Card Ref. Nbr.", Enabled = false)]
        [PXSelector(typeof(Search<EPTimeCard.timeCardCD>),
            typeof(EPTimeCard.timeCardCD),
            typeof(EPTimeCard.employeeID),
            typeof(EPTimeCard.weekDescription),
            typeof(EPTimeCard.status))]
        [PXUIVisible(typeof(Where<Current<FSSrvOrdType.createTimeActivitiesFromAppointment>, Equal<True>,
                                And<Current<FSSetup.enableEmpTimeCardIntegration>, Equal<True>,
                                And<FeatureInstalled<FeaturesSet.timeReportingModule>>>>))]
        public override string TimeCardCD { get; set; }
        #endregion
        #region CuryUnitCost
        public new abstract class curyUnitCost : PX.Data.BQL.BqlDecimal.Field<curyUnitCost> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(unitCost))]
        [PXUIField(Visible = false, Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Default<laborItemID>))]
        public override Decimal? CuryUnitCost { get; set; }
        #endregion
        #region CuryExtCost
        public new abstract class curyExtCost : PX.Data.BQL.BqlDecimal.Field<curyExtCost> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(extCost))]
        [PXUIField(Enabled = false, Visible = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Div<Mult<curyUnitCost, timeDuration>, SharedClasses.decimal_60>), typeof(SumCalc<FSAppointment.curyCostTotal>))]
        public override Decimal? CuryExtCost { get; set; }
        #endregion
        #region UnitCost
        public new abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }

        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public override Decimal? UnitCost { get; set; }
        #endregion
        #region ExtCost
        public new abstract class extCost : PX.Data.BQL.BqlDecimal.Field<extCost> { }

        [PXDBPriceCost()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Div<Mult<unitCost, timeDuration>, SharedClasses.decimal_60>))]
        public override Decimal? ExtCost { get; set; }
        #endregion

        #region WorkgroupID
        public new abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
        [PXDBInt]
        [PXUIField(DisplayName = "Workgroup")]
        [PXWorkgroupSelector]
        [PXDefault(typeof(Search<EPEmployee.defaultWorkgroupID,
            Where<EPEmployee.bAccountID, Equal<Current<FSAppointmentLog.bAccountID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<FSAppointmentLog.bAccountID>))]
        public override int? WorkgroupID { get; set; }
        #endregion

        #region IsBillable
        public new abstract class isBillable : PX.Data.BQL.BqlBool.Field<isBillable> { }

        [PXDBBool]
        [PXDefault(typeof(Switch<
                            Case<
                                Where2<
                                Where<
                                    Current<FSSrvOrdType.postTo>, Equal<FSPostTo.Projects>,
                                    And<Current<FSSrvOrdType.billingType>, Equal<FSSrvOrdType.billingType.CostAsCost>,
                                    And<Current<FSSrvOrdType.createTimeActivitiesFromAppointment>, Equal<True>,
                                    And<Current<FSSetup.enableEmpTimeCardIntegration>, Equal<True>,
                                    And<trackTime, Equal<True>>>>>>,
                                And<
                                    FeatureInstalled<FeaturesSet.timeReportingModule>>>,
                                True>,
                            False>))]
        [PXUIField(DisplayName = "Billable Labor")]
        [PXFormula(typeof(Default<trackTime>))]
        [PXUIEnabled(typeof(Where2<
                                Where<
                                    Current<FSSrvOrdType.postTo>, Equal<FSPostTo.Projects>,
                                    And<Current<FSSrvOrdType.billingType>, Equal<FSSrvOrdType.billingType.CostAsCost>,
                                    And<Current<FSSrvOrdType.createTimeActivitiesFromAppointment>, Equal<True>,
                                    And<Current<FSSetup.enableEmpTimeCardIntegration>, Equal<True>,
                                    And<trackTime, Equal<True>>>>>>,
                                And<
                                    FeatureInstalled<FeaturesSet.timeReportingModule>>>))]
        [PXUIVisible(typeof(Where2<
                                Where<
                                    Current<FSSrvOrdType.postTo>, Equal<FSPostTo.Projects>,
                                    And<Current<FSSrvOrdType.billingType>, Equal<FSSrvOrdType.billingType.CostAsCost>,
                                    And<Current<FSSrvOrdType.createTimeActivitiesFromAppointment>, Equal<True>,
                                    And<Current<FSSetup.enableEmpTimeCardIntegration>, Equal<True>>>>>,
                                And<
                                    FeatureInstalled<FeaturesSet.timeReportingModule>>>))]
        public override bool? IsBillable { get; set; }
        #endregion
        #region BillableTimeDuration
        public new abstract class billableTimeDuration : PX.Data.BQL.BqlInt.Field<billableTimeDuration> { }

        [FSDBTimeSpanLong]
        [PXUIField(DisplayName = "Billable Time")]
        [PXDefault(typeof(Switch<
                            Case<Where<isBillable, Equal<True>>,
                                timeDuration>,
                            SharedClasses.int_0>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIEnabled(typeof(Where<isBillable, Equal<True>>))]
        [PXFormula(typeof(Default<timeDuration, isBillable>))]
        [PXUIVisible(typeof(Where2<
                                Where<
                                    Current<FSSrvOrdType.postTo>, Equal<FSPostTo.Projects>,
                                    And<Current<FSSrvOrdType.billingType>, Equal<FSSrvOrdType.billingType.CostAsCost>,
                                    And<Current<FSSrvOrdType.createTimeActivitiesFromAppointment>, Equal<True>,
                                    And<Current<FSSetup.enableEmpTimeCardIntegration>, Equal<True>>>>>,
                                And<
                                    FeatureInstalled<FeaturesSet.timeReportingModule>>>))]
        public override int? BillableTimeDuration { get; set; }
        #endregion
        #region BillableQty
        public new abstract class billableQty : PX.Data.BQL.BqlDecimal.Field<billableQty> { }

        [PXDBQuantity]
        [PXUIField(DisplayName = "Billable Quantity", Enabled = false)]
        [PXFormula(typeof(Default<billableTimeDuration>))]
        [PXUIEnabled(typeof(Where<isBillable, Equal<True>>))]
        [PXUIVisible(typeof(Where2<
                                Where<
                                    Current<FSSrvOrdType.postTo>, Equal<FSPostTo.Projects>,
                                    And<Current<FSSrvOrdType.billingType>, Equal<FSSrvOrdType.billingType.CostAsCost>,
                                    And<Current<FSSrvOrdType.createTimeActivitiesFromAppointment>, Equal<True>,
                                    And<Current<FSSetup.enableEmpTimeCardIntegration>, Equal<True>>>>>,
                                And<
                                    FeatureInstalled<FeaturesSet.timeReportingModule>>>))]
        public override decimal? BillableQty { get; set; }
        #endregion

        #region CuryBillableTranAmount
        public new abstract class curyBillableTranAmount : PX.Data.BQL.BqlDecimal.Field<curyBillableTranAmount> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(billableTranAmount))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Mult<curyUnitCost, billableQty>),
                            typeof(SumCalc<FSAppointment.curyLogBillableTranAmountTotal>))]
        [PXUIField(DisplayName = "Billable Amount", Enabled = false)]
        [PXUIVisible(typeof(Where2<
                                Where<
                                    Current<FSSrvOrdType.postTo>, Equal<FSPostTo.Projects>,
                                    And<Current<FSSrvOrdType.billingType>, Equal<FSSrvOrdType.billingType.CostAsCost>,
                                    And<Current<FSSrvOrdType.createTimeActivitiesFromAppointment>, Equal<True>,
                                    And<Current<FSSetup.enableEmpTimeCardIntegration>, Equal<True>>>>>,
                                And<
                                    FeatureInstalled<FeaturesSet.timeReportingModule>>>))]
        public override Decimal? CuryBillableTranAmount { get; set; }
        #endregion
        #region BillableTranAmount
        public new abstract class billableTranAmount : PX.Data.BQL.BqlDecimal.Field<billableTranAmount> { }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Billable Amount", Enabled = false)]
        public override Decimal? BillableTranAmount { get; set; }
        #endregion

        #region CreatedByID
        public new abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        [PXDBCreatedByID]
        [PXUIField(DisplayName = "CreatedByID")]
        public override Guid? CreatedByID { get; set; }
        #endregion
        #region CreatedByScreenID
        public new abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        [PXDBCreatedByScreenID]
        [PXUIField(DisplayName = "CreatedByScreenID")]
        public override string CreatedByScreenID { get; set; }
        #endregion
        #region CreatedDateTime
        public new abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        [PXDBCreatedDateTime]
        [PXUIField(DisplayName = "CreatedDateTime")]
        public override DateTime? CreatedDateTime { get; set; }
        #endregion
        #region LastModifiedByID
        public new abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        [PXDBLastModifiedByID]
        [PXUIField(DisplayName = "LastModifiedByID")]
        public override Guid? LastModifiedByID { get; set; }
        #endregion
        #region LastModifiedByScreenID
        public new abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        [PXDBLastModifiedByScreenID]
        [PXUIField(DisplayName = "LastModifiedByScreenID")]
        public override string LastModifiedByScreenID { get; set; }
        #endregion
        #region LastModifiedDateTime
        public new abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        [PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = "LastModifiedDateTime")]
        public override DateTime? LastModifiedDateTime { get; set; }
        #endregion
        #region NoteID
        public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

        [PXUIField(DisplayName = "NoteID")]
        [PXNote]
        public override Guid? NoteID { get; set; }
        #endregion
        #region tstamp
        public new abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        [PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
        public override byte[] tstamp { get; set; }
        #endregion

        #region SkipCostCodeValidation
        public new abstract class skipCostCodeValidation : PX.Data.BQL.BqlBool.Field<skipCostCodeValidation> { }

        [PXBool]
        [PXFormula(typeof(IIf<Where<Current<FSSrvOrdType.createTimeActivitiesFromAppointment>, Equal<True>,
                                And<Current<FSSetup.enableEmpTimeCardIntegration>, Equal<True>>>, False, True>))]
        public override bool? SkipCostCodeValidation { get; set; }
        #endregion

		#region TimeActivityStatus
		public new abstract class timeActivityStatus : PX.Data.BQL.BqlString.Field<timeActivityStatus> { }
		#endregion

        #region ServiceDateTimeBegin
        public abstract class serviceDateTimeBegin : PX.Data.BQL.BqlBool.Field<serviceDateTimeBegin> { }

        [PXDateAndTime(UseTimeZone = true)]
        [PXUIField(Visibility = PXUIVisibility.Visible)]
        [PXFormula(null, typeof(MinCalc<FSAppointment.minLogTimeBegin>))]
        public virtual DateTime? ServiceDateTimeBegin
        {
            [PXDependsOnFields(typeof(dateTimeBegin))]
            get
            {
                if (this.ItemType != FSAppointmentLog.itemType.Values.Travel) 
                {
                    return this.DateTimeBegin;
                }

                return null;
            }
        }
        #endregion
        #region ServiceDateTimeEnd
        public abstract class serviceDateTimeEnd : PX.Data.BQL.BqlBool.Field<serviceDateTimeEnd> { }

        [PXDateAndTime(UseTimeZone = true)]
        [PXFormula(null, typeof(MaxCalc<FSAppointment.maxLogTimeEnd>))]
        [PXUIField(Visibility = PXUIVisibility.Visible)]
        public virtual DateTime? ServiceDateTimeEnd
        {
            [PXDependsOnFields(typeof(dateTimeEnd))]
            get
            {
                if (this.ItemType != FSAppointmentLog.itemType.Values.Travel)
                {
                    return this.DateTimeEnd;
                }

                return null;
            }
        }
        #endregion
    }
}
