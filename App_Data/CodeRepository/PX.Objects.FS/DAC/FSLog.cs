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
    public class FSLog : IBqlTable
    {
        #region Keys
        public class PK : PrimaryKeyOf<FSLog>.By<logID>
        {
            public static FSLog Find(PXGraph graph, int? logID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, logID, options);
        }

        public static class FK
        {
            public class Staff : BAccount.PK.ForeignKeyOf<FSLog>.By<bAccountID> { }
            public class EarningType : EP.EPEarningType.PK.ForeignKeyOf<FSLog>.By<earningType> { }
            public class CostCode : PMCostCode.PK.ForeignKeyOf<FSLog>.By<costCodeID> { }
            public class Project : PMProject.PK.ForeignKeyOf<FSLog>.By<projectID> { }
            public class Task : PMTask.PK.ForeignKeyOf<FSLog>.By<projectID, projectTaskID> { }
            public class LaborItem : IN.InventoryItem.PK.ForeignKeyOf<FSLog>.By<laborItemID> { }
            public class CurrencyInfo : CM.CurrencyInfo.PK.ForeignKeyOf<FSLog>.By<curyInfoID> { }
            public class WorkGorupID : EPCompanyTree.PK.ForeignKeyOf<FSLog>.By<workgroupID> { }
            public class TimeCard : EPTimeCard.PK.ForeignKeyOf<FSLog>.By<timeCardCD> { }
        }
        #endregion

        #region LogID
        public abstract class logID : PX.Data.BQL.BqlInt.Field<logID> { }

        [PXDBIdentity(IsKey = true)]
        public virtual int? LogID { get; set; }
        #endregion
        #region DocType
        public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

        [PXDBString(4, IsFixed = true)]
        public virtual string DocType { get; set; }
        #endregion
        #region DocRefNbr
        public abstract class docRefNbr : PX.Data.BQL.BqlString.Field<docRefNbr> { }

        [PXDBString(20, IsUnicode = true)]
        public virtual string DocRefNbr { get; set; }
        #endregion
        #region DocID
        public abstract class docID : PX.Data.BQL.BqlInt.Field<docID> { }

        [PXDBInt]
        [PXUIField(Visible = false, Enabled = false)]
        public virtual int? DocID { get; set; }
        #endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Line Nbr.", Visible = false, Enabled = false)]
        public virtual int? LineNbr { get; set; }
        #endregion
        #region ItemType
        public abstract class itemType : PX.Data.BQL.BqlString.Field<itemType>
        {
            public abstract class Values : ListField_Log_ItemType { }
        }

        [PXDBString(2, IsFixed = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Log Type")]
        [itemType.Values.List]
        public virtual string ItemType { get; set; }
        #endregion
        #region Status
        public abstract class status : ListField_Status_Log { }

        [PXDBString(1, IsFixed = true)]
        [status.ListAtrribute]
        [PXDefault(ID.Status_Log.IN_PROCESS)]
        [PXUIField(DisplayName = "Log Line Status")]
        public virtual string Status { get; set; }
        #endregion
        #region DateTimeBegin
        public abstract class dateTimeBegin : PX.Data.BQL.BqlDateTime.Field<dateTimeBegin> { }

        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "Date", DisplayNameTime = "Start Time")]
        [PXUIField(DisplayName = "Start Time")]
        public virtual DateTime? DateTimeBegin { get; set; }
        #endregion
        #region DateTimeEnd
        public abstract class dateTimeEnd : PX.Data.BQL.BqlDateTime.Field<dateTimeEnd> { }

        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "Date", DisplayNameTime = "End Time")]
        [PXUIField(DisplayName = "End Time")]
        public virtual DateTime? DateTimeEnd { get; set; }
        #endregion
        #region TimeDuration
        public abstract class timeDuration : PX.Data.BQL.BqlInt.Field<timeDuration> { }

        [FSDBTimeSpanLong]
        [PXFormula(typeof(IsNull<Sub<dateTimeEnd, dateTimeBegin>, SharedClasses.int_0>))]
        [PXUIField(DisplayName = "Duration")]
        public virtual int? TimeDuration { get; set; }
        #endregion
        #region ApprovedTime
        public abstract class approvedTime : PX.Data.BQL.BqlBool.Field<approvedTime> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Approved", Enabled = false)]
        [PXUIVisible(typeof(Where<Current<FSSrvOrdType.createTimeActivitiesFromAppointment>, Equal<True>,
            And<FeatureInstalled<FeaturesSet.timeReportingModule>>>))]
        public virtual bool? ApprovedTime { get; set; }
        #endregion
        #region CostCodeID
        public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }

        [SMCostCode(typeof(skipCostCodeValidation), null, typeof(projectTaskID))]
        [PXForeignReference(typeof(FK.CostCode))]
        public virtual int? CostCodeID { get; set; }
        #endregion
        #region CuryExtCost
        public abstract class curyExtCost : PX.Data.BQL.BqlDecimal.Field<curyExtCost> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(extCost))]
        [PXUIField(Enabled = false, Visible = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryExtCost { get; set; }
        #endregion
        #region CuryInfoID
        public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }

        [PXDBLong]
        [CurrencyInfo]
        public virtual Int64? CuryInfoID { get; set; }
        #endregion
        #region CuryUnitCost
        public abstract class curyUnitCost : PX.Data.BQL.BqlDecimal.Field<curyUnitCost> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(unitCost))]
        [PXUIField(Visible = false, Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryUnitCost { get; set; }
        #endregion
        #region Descr
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

        [PXDBString(Common.Constants.TranDescLength, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Description")]
        public virtual string Descr { get; set; }
        #endregion
        #region DetLineRef
        public abstract class detLineRef : PX.Data.BQL.BqlString.Field<detLineRef> { }

        [PXDBString(4, IsFixed = true)]
        [PXUIField(DisplayName = "Detail Ref. Nbr.")]
        public virtual string DetLineRef { get; set; }
        #endregion
        #region EarningType
        public abstract class earningType : PX.Data.BQL.BqlString.Field<earningType> { }

        [PXDBString(EPEarningType.typeCD.Length, IsUnicode = true, InputMask = EPEarningType.typeCD.InputMask)]
        [PXSelector(typeof(EPEarningType.typeCD))]
        [PXUIField(DisplayName = "Earning Type")]
        [PXUIVisible(typeof(Where<Current<FSSrvOrdType.createTimeActivitiesFromAppointment>, Equal<True>,
            And<FeatureInstalled<FeaturesSet.timeReportingModule>>>))]
        public virtual string EarningType { get; set; }
        #endregion
        #region BAccountID
        public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Staff Member")]
        [FSSelector_StaffMember_ServiceOrderProjectID]
        public virtual int? BAccountID { get; set; }
        #endregion
        #region BAccountType
        public abstract class bAccountType : PX.Data.BQL.BqlString.Field<bAccountType> { }

        [PXDBString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Staff Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [CR.BAccountType.ListAttribute]
        public virtual string BAccountType { get; set; }
        #endregion
        #region ExtCost
        public abstract class extCost : PX.Data.BQL.BqlDecimal.Field<extCost> { }

        [PXDBPriceCost()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? ExtCost { get; set; }
        #endregion
        #region KeepDateTimes
        public abstract class keepDateTimes : PX.Data.BQL.BqlBool.Field<keepDateTimes> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Manage Time Manually", Visible = false)]
        public virtual bool? KeepDateTimes { get; set; }
        #endregion
        #region LaborItemID
        public abstract class laborItemID : PX.Data.BQL.BqlInt.Field<laborItemID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Labor Item ID")]
        [PXDimensionSelector(InventoryAttribute.DimensionName, 
                             typeof(Search<InventoryItem.inventoryID, 
                                        Where<InventoryItem.itemType, Equal<INItemTypes.laborItem>, 
                                        And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>, 
                                        And<Match<Current<AccessInfo.userName>>>>>>), 
                             typeof(InventoryItem.inventoryCD))]
        [PXForeignReference(typeof(FK.LaborItem))]
        public virtual int? LaborItemID { get; set; }
        #endregion
        #region LineRef
        public abstract class lineRef : PX.Data.BQL.BqlString.Field<lineRef> { }

        [FSDBLineRef(typeof(lineNbr))]
        [PXDBString(3, IsFixed = true)]
        [PXUIField(DisplayName = "Log Ref. Nbr.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual string LineRef { get; set; }
        #endregion
        #region ProjectID
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

        [PXDBInt]
        [PXUIField(Visible = false, FieldClass = ProjectAttribute.DimensionName)]
        [PXForeignReference(typeof(FK.Project))]
        public virtual int? ProjectID { get; set; }
        #endregion
        #region ProjectTaskID
        public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Project Task", FieldClass = ProjectAttribute.DimensionName)]
        [FSSelectorActive_AR_SO_ProjectTask(typeof(Where<PMTask.projectID, Equal<Current<projectID>>>))]
        [PXForeignReference(typeof(FK.Task))]
        public virtual int? ProjectTaskID { get; set; }
        #endregion
        #region TrackTime
        public abstract class trackTime : PX.Data.BQL.BqlBool.Field<trackTime> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Track Time")]
        [PXUIVisible(typeof(Where<Current<FSSrvOrdType.createTimeActivitiesFromAppointment>, Equal<True>,
            And<FeatureInstalled<FeaturesSet.timeReportingModule>>>))]
        [PXUIEnabled(typeof(Where<Current<FSSrvOrdType.createTimeActivitiesFromAppointment>, Equal<True>,
            And<FeatureInstalled<FeaturesSet.timeReportingModule>>>))]
        public virtual bool? TrackTime { get; set; }
        #endregion
        #region TrackOnService
        public abstract class trackOnService : PX.Data.BQL.BqlBool.Field<trackOnService> { }

        [PXDBBool]
        [PXUIField(DisplayName = "Add to Actual Duration")]
        public virtual bool? TrackOnService { get; set; }
        #endregion
        #region TimeCardCD
        public abstract class timeCardCD : PX.Data.BQL.BqlString.Field<timeCardCD> { }

        [PXDBString(10, IsUnicode = true, InputMask = ">CCCCCCCCCC")]
        [PXUIField(DisplayName = "Time Card Ref. Nbr.", Enabled = false)]
        [PXUIVisible(typeof(Where<Current<FSSrvOrdType.createTimeActivitiesFromAppointment>, Equal<True>,
            And<FeatureInstalled<FeaturesSet.timeReportingModule>>>))]
        [PXSelector(typeof(Search<EPTimeCard.timeCardCD>),
            typeof(EPTimeCard.timeCardCD),
            typeof(EPTimeCard.employeeID),
            typeof(EPTimeCard.weekDescription),
            typeof(EPTimeCard.status))]
        public virtual string TimeCardCD { get; set; }
        #endregion
        #region UnitCost
        public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }

        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? UnitCost { get; set; }
		#endregion
		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Workgroup")]
		[PXWorkgroupSelector]
		public virtual int? WorkgroupID { get; set; }
		#endregion

		#region IsBillable
		public abstract class isBillable : PX.Data.BQL.BqlBool.Field<isBillable> { }

        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Billable")]
        public virtual bool? IsBillable { get; set; }
        #endregion
        #region BillableTimeDuration
        public abstract class billableTimeDuration : PX.Data.BQL.BqlInt.Field<billableTimeDuration> { }

        [FSDBTimeSpanLong]
        [PXUIField(DisplayName = "Billable Time")]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? BillableTimeDuration { get; set; }
        #endregion
        #region BillableQty
        public abstract class billableQty : PX.Data.BQL.BqlDecimal.Field<billableQty> { }

        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Billable Quantity", Enabled = false)]
        public virtual decimal? BillableQty { get; set; }
        #endregion
        #region CuryBillableTranAmount
        public abstract class curyBillableTranAmount : PX.Data.BQL.BqlDecimal.Field<curyBillableTranAmount> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(billableTranAmount))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Billable Amount", Enabled = false)]
        public virtual Decimal? CuryBillableTranAmount { get; set; }
        #endregion
        #region BillableTranAmount
        public abstract class billableTranAmount : PX.Data.BQL.BqlDecimal.Field<billableTranAmount> { }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Billable Amount", Enabled = false)]
        public virtual Decimal? BillableTranAmount { get; set; }
        #endregion

		#region TimeActivityStatus
		public abstract class timeActivityStatus : PX.Data.BQL.BqlString.Field<timeActivityStatus> { }

		/// <summary>
		/// Approval status of the related PMTimeActivity.
		/// </summary>
		[PXString(2)]
		[PXDBScalar(typeof(Search<CR.PMTimeActivity.approvalStatus,
				Where<FSxPMTimeActivity.appointmentID, Equal<docID>
					, And<FSxPMTimeActivity.logLineNbr, Equal<lineNbr>>>>))]
		public virtual string TimeActivityStatus { get; set; }
		#endregion

        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        [PXDBCreatedByID]
        [PXUIField(DisplayName = "CreatedByID")]
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
        [PXUIField(DisplayName = "CreatedDateTime")]
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
        [PXUIField(DisplayName = "LastModifiedDateTime")]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        #endregion
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

        [PXUIField(DisplayName = "NoteID")]
        [PXNote]
        public virtual Guid? NoteID { get; set; }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        [PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
        public virtual byte[] tstamp { get; set; }
        #endregion

        #region MemoryHelper
        #region SkipCostCodeValidation
        public abstract class skipCostCodeValidation : PX.Data.BQL.BqlBool.Field<skipCostCodeValidation> { }

        [PXBool]
        public virtual bool? SkipCostCodeValidation { get; set; }
        #endregion
        #region TimeDurationReport
        public abstract class timeDurationReport : Data.BQL.BqlInt.Field<timeDurationReport> { }

        [PXInt]
        [PXFormula(typeof(FSLog.timeDuration))]
        public virtual int? TimeDurationReport { get; set; }
        #endregion
        #endregion
    }
}
