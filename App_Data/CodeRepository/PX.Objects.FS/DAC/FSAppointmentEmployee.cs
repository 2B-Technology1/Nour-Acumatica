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
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.IN;
using PX.Objects.PM;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    [PXCacheName(TX.TableName.FSAppointmentEmployee)]
    public class FSAppointmentEmployee : PX.Data.IBqlTable
    {
        #region Keys
        public class PK : PrimaryKeyOf<FSAppointmentEmployee>.By<srvOrdType, refNbr, lineNbr>
        {
            public static FSAppointmentEmployee Find(PXGraph graph, string srvOrdType, string refNbr, int? lineNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, srvOrdType, refNbr, lineNbr, options);
        }

        public static class FK
        {
            public class ServiceOrderType : FSSrvOrdType.PK.ForeignKeyOf<FSAppointmentEmployee>.By<srvOrdType> { }
            public class Appointment : FSAppointment.PK.ForeignKeyOf<FSAppointmentEmployee>.By<srvOrdType, refNbr> { }
            public class Staff : BAccount.PK.ForeignKeyOf<FSAppointmentEmployee>.By<employeeID> { }
            public class EarningType : EP.EPEarningType.PK.ForeignKeyOf<FSAppointmentEmployee>.By<earningType> { }
            public class CostCode : PMCostCode.PK.ForeignKeyOf<FSAppointmentEmployee>.By<costCodeID> { }
            public class DefaultProject : PMProject.PK.ForeignKeyOf<FSAppointmentEmployee>.By<dfltProjectID> { }
            public class DefaultTask : PMTask.PK.ForeignKeyOf<FSAppointmentEmployee>.By<dfltProjectID, dfltProjectTaskID> { }
            public class LaborItem : IN.InventoryItem.PK.ForeignKeyOf<FSAppointmentEmployee>.By<laborItemID> { }
        }

        #endregion

        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXDBString(4, IsKey = true, IsFixed = true)]
        [PXUIField(DisplayName = "Service Order Type", Visible = false, Enabled = false)]
        [PXDefault(typeof(FSAppointment.srvOrdType))]
        [PXSelector(typeof(Search<FSSrvOrdType.srvOrdType>), CacheGlobal = true)]
        public virtual string SrvOrdType { get; set; }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

        [PXDBString(20, IsKey = true, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Appointment Nbr.", Visible = false, Enabled = false)]
        [PXDBDefault(typeof(FSAppointment.refNbr), DefaultForUpdate = false)]
        [PXParent(typeof(Select<
            FSAppointment,
            Where<FSAppointment.srvOrdType, Equal<Current<FSAppointmentEmployee.srvOrdType>>,
                And<FSAppointment.refNbr, Equal<Current<FSAppointmentEmployee.refNbr>>>>>))]
        public virtual string RefNbr { get; set; }
        #endregion
        #region AppointmentID
        public abstract class appointmentID : PX.Data.BQL.BqlInt.Field<appointmentID> { }

        [PXDBInt]
        [PXDBDefault(typeof(FSAppointment.appointmentID))]
        [PXUIField(DisplayName = "Appointment Ref. Nbr.")]
        public virtual int? AppointmentID { get; set; }
        #endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        [PXDBInt(IsKey = true)]
        [PXLineNbr(typeof(FSAppointment.employeeLineCntr))]
        [PXUIField(DisplayName = "Line Nbr.", Visible = false, Enabled = false)]
        [PXUnboundFormula(typeof(int1),
                            typeof(SumCalc<FSAppointment.staffCntr>))]
        public virtual int? LineNbr { get; set; }
        #endregion
        #region LineRef
        public abstract class lineRef : PX.Data.BQL.BqlString.Field<lineRef> { }

        [PXDBString(3, IsFixed = true)]
        [PXUIField(DisplayName = "Ref. Nbr.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual string LineRef { get; set; }
        #endregion
        #region ServiceLineRef
        public abstract class serviceLineRef : PX.Data.BQL.BqlString.Field<serviceLineRef> { }

        [PXDBString(4, IsFixed = true)]
        [PXParent(typeof(Select<
            FSAppointmentDet,
            Where<
                             FSAppointmentDet.lineRef, Equal<Current<FSAppointmentEmployee.serviceLineRef>>,
                And<
                             FSAppointmentDet.appointmentID, Equal<Current<FSAppointmentEmployee.appointmentID>>>>>))]
        [PXUIField(DisplayName = "Detail Ref. Nbr.")]
        [FSSelectorAppointmentSODetID]
        [PXCheckUnique(Where = typeof(
            Where<FSAppointmentEmployee.appointmentID, Equal<Current<FSAppointment.appointmentID>>,
                And<FSAppointmentEmployee.employeeID, Equal<Current<FSAppointmentEmployee.employeeID>>>>),
                       IgnoreNulls = false, ClearOnDuplicate = false)]
        public virtual string ServiceLineRef { get; set; }
        #endregion
        #region EmployeeID
        public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }

        [PXDBInt]
        [PXDefault]
        [FSSelector_StaffMember_ServiceOrderProjectID]
        [PXUIField(DisplayName = "Staff Member", TabOrder = 0)]
        public virtual int? EmployeeID { get; set; }
        #endregion

        #region PrimaryDriver
        public abstract class primaryDriver : PX.Data.BQL.BqlBool.Field<primaryDriver> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Primary Driver")]
        public virtual bool? PrimaryDriver { get; set; }
        #endregion

        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

        [PXNote]
        [PXUIField(DisplayName = "NoteID", Enabled = false)]
        public virtual Guid? NoteID { get; set; }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        [PXDBCreatedByID]
        public virtual Guid? CreatedByID { get; set; }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        [PXDBCreatedByScreenID]
        public virtual string CreatedByScreenID { get; set; }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        [PXDBCreatedDateTime]
        public virtual DateTime? CreatedDateTime { get; set; }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        [PXDBLastModifiedByID]
        public virtual Guid? LastModifiedByID { get; set; }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        [PXDBLastModifiedByScreenID]
        public virtual string LastModifiedByScreenID { get; set; }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        [PXDBLastModifiedDateTime]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        [PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
        public virtual byte[] tstamp { get; set; }
        #endregion
        #region IsDriver
        public abstract class isDriver : PX.Data.BQL.BqlBool.Field<isDriver> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Route Driver", Enabled = false, Visible = false)]
        public virtual bool? IsDriver { get; set; }
        #endregion
        #region Type
        public abstract class type : PX.Data.BQL.BqlString.Field<type> { }

        [PXDBString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Staff Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = false, Visible = false)]
        [EmployeeType.List]
        public virtual string Type { get; set; }
        #endregion

        #region EarningType
        public abstract class earningType : PX.Data.BQL.BqlString.Field<earningType> { }

        [PXDBString(EPEarningType.typeCD.Length, IsUnicode = true, InputMask = EPEarningType.typeCD.InputMask)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<FSAppointmentEmployee.serviceLineRef>))]
        [PXSelector(typeof(EPEarningType.typeCD))]
        [PXUIField(DisplayName = "Earning Type")]
        [PXUIVisible(typeof(
            Where<Current<FSSrvOrdType.createTimeActivitiesFromAppointment>, Equal<True>,
                And<Current<FSSetup.enableEmpTimeCardIntegration>, Equal<True>,
                And<FeatureInstalled<FeaturesSet.timeReportingModule>>>>))]
        [PXRestrictor(typeof(
            Where<EPEarningType.isActive, Equal<True>>), EP.Messages.EarningTypeInactive, typeof(EPEarningType.typeCD))]
        public virtual string EarningType { get; set; }
        #endregion
        #region TrackTime
        public abstract class trackTime : PX.Data.BQL.BqlBool.Field<trackTime> { }

        [PXDBBool]
        [PXDefault(typeof(Switch<
                            Case<
            Where<Current<type>, Equal<BAccountType.employeeType>, 
                And<Current<FSSetup.enableEmpTimeCardIntegration>, Equal<True>,
                And<FeatureInstalled<FeaturesSet.timeReportingModule>>>>,
                                    Current<FSSrvOrdType.createTimeActivitiesFromAppointment>>,
                            False>))]
        [PXUIField(DisplayName = "Track Time")]
        [PXUIVisible(typeof(
            Where<Current<FSSrvOrdType.createTimeActivitiesFromAppointment>, Equal<True>,
                And<Current<FSSetup.enableEmpTimeCardIntegration>, Equal<True>,
                And<FeatureInstalled<FeaturesSet.timeReportingModule>>>>))]
        [PXUIEnabled(typeof(
            Where<Current<FSSrvOrdType.createTimeActivitiesFromAppointment>, Equal<True>,
                And<Current<FSSetup.enableEmpTimeCardIntegration>, Equal<True>,
                And<FeatureInstalled<FeaturesSet.timeReportingModule>>>>))]
        public virtual bool? TrackTime { get; set; }
        #endregion

        #region CostCodeID
        public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
        [PXDefault(typeof(Switch<Case<
            Where<
                            Selector<dfltProjectID, 
                                PMProject.nonProject>, Equal<False>>,
                          Current<FSSrvOrdType.dfltCostCodeID>>>), 
            PersistingCheck = PXPersistingCheck.Nothing)]
        [SMCostCode(typeof(skipCostCodeValidation), null, typeof(dfltProjectTaskID))]
        public virtual int? CostCodeID { get; set; }
        #endregion
        #region SkipCostCodeValidation
        public abstract class skipCostCodeValidation : PX.Data.BQL.BqlBool.Field<skipCostCodeValidation> { }

        [PXBool]
        [PXFormula(typeof(IIf<
            Where<Current<FSSrvOrdType.createTimeActivitiesFromAppointment>, Equal<True>,
                And<Current<FSSetup.enableEmpTimeCardIntegration>, Equal<True>>>, False, True>))]
        public virtual bool? SkipCostCodeValidation { get; set; }
        #endregion

        #region DfltProjectID
        public abstract class dfltProjectID : PX.Data.BQL.BqlInt.Field<dfltProjectID> { }

        [PXDefault(typeof(FSAppointment.projectID), PersistingCheck = PXPersistingCheck.Nothing)]
        [ProjectBase(typeof(FSServiceOrder.billCustomerID), Visible = false)]
        [PXRestrictor(typeof(
            Where<PMProject.isActive, Equal<True>>), PM.Messages.InactiveContract, typeof(PMProject.contractCD))]
        [PXRestrictor(typeof(
            Where<PMProject.isCancelled, Equal<False>>), PM.Messages.CancelledContract, typeof(PMProject.contractCD))]
        [PXForeignReference(typeof(FK.DefaultProject))]
        public virtual int? DfltProjectID { get; set; }
        #endregion
        #region DfltProjectTaskID
        public abstract class dfltProjectTaskID : PX.Data.BQL.BqlInt.Field<dfltProjectTaskID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Project Task", FieldClass = ProjectAttribute.DimensionName)]
        [FSSelectorActive_AR_SO_ProjectTask(typeof(
            Where<PMTask.projectID, Equal<Current<dfltProjectID>>>))]
        [PXForeignReference(typeof(FK.DefaultTask))]
        public virtual int? DfltProjectTaskID { get; set; }
        #endregion

        #region LaborItem
        public abstract class laborItemID : PX.Data.BQL.BqlInt.Field<laborItemID> { }

        [PXDBInt]
        [PXSelector(typeof(Search<
            InventoryItem.inventoryID,
            Where<
                               InventoryItem.itemType, Equal<INItemTypes.laborItem>>>)
            ,SubstituteKey = typeof(InventoryItem.inventoryCD)
            ,DescriptionField = typeof(InventoryItem.descr))]
        [PXUIField(DisplayName = "Labor Item")]
        [PXDefault(typeof(Search<
            EPEmployee.labourItemID, 
            Where<EPEmployee.bAccountID, Equal<Current<FSAppointmentEmployee.employeeID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<FSAppointmentEmployee.employeeID>))]
        public virtual int? LaborItemID { get; set; }
        #endregion

        #region Mem_Selected
        public abstract class mem_Selected : PX.Data.BQL.BqlBool.Field<mem_Selected> { }

        [PXBool]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Mem_Selected { get; set; }
        #endregion

        #region IsStaffCalendar
        public abstract class isStaffCalendar : Data.BQL.BqlBool.Field<isStaffCalendar> { }

        [PXBool]
        [PXUIField(Enabled = false, Visible = false)]
        public virtual bool? IsStaffCalendar { get; set; }
        #endregion
    }
}
