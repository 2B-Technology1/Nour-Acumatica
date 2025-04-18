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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.SM;
using PX.TM;
using PX.Web.UI;

namespace PX.Objects.CR
{
	[CRTimeActivityPrimaryGraph]
	[Serializable]
	[PXCacheName(Messages.TimeActivity)]
	public partial class PMTimeActivity : IBqlTable
	{

		#region Keys

		/// <summary>
		/// Primary Key
		/// </summary>
		public class PK : PrimaryKeyOf<PMTimeActivity>.By<noteID>
		{
			public static PMTimeActivity Find(PXGraph graph, Guid? noteID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, noteID, options);
		}

		/// <summary>
		/// Foreign Keys
		/// </summary>
		public static class FK
		{
			/// <summary>
			/// Time Card
			/// </summary>
			public class Timecard : EPTimeCard.PK.ForeignKeyOf<PMTimeActivity>.By<timeCardCD> { }

			/// <summary>
			/// Earning Type
			/// </summary>
			public class EarningType : EPEarningType.PK.ForeignKeyOf<PMTimeActivity>.By<earningTypeID> { }

			/// <summary>
			/// Owner
			/// </summary>
			public class OwnerContact : Contact.PK.ForeignKeyOf<PMTimeActivity>.By<ownerID> { }

			/// <summary>
			/// Project
			/// </summary>
			public class Project : PMProject.PK.ForeignKeyOf<PMTimeActivity>.By<projectID> { }

			/// <summary>
			/// Project Task
			/// </summary>
			public class ProjectTask : PMTask.PK.ForeignKeyOf<PMTimeActivity>.By<projectID, projectTaskID> { }

			/// <summary>
			/// Cost Code
			/// </summary>
			public class CostCode : PMCostCode.PK.ForeignKeyOf<PMTimeActivity>.By<costCodeID> { }

			/// <summary>
			/// Related Activity.
			/// </summary>
			public class Related : CRActivity.PK.ForeignKeyOf<PMTimeActivity>.By<refNoteID> { }

			/// <summary>
			/// Parent Activity.
			/// </summary>
			public class Parent : CRActivity.PK.ForeignKeyOf<PMTimeActivity>.By<parentTaskNoteID> { }

			/// <summary>
			/// Union
			/// </summary>
			public class Union : PMUnion.PK.ForeignKeyOf<PMTimeActivity>.By<unionID> { }

			/// <summary>
			/// Work Code
			/// </summary>
			public class WorkCode : PMWorkCode.PK.ForeignKeyOf<PMTimeActivity>.By<workCodeID> { }

			/// <summary>
			/// Contract
			/// </summary>
			public class Contract : CT.Contract.PK.ForeignKeyOf<PMTimeActivity>.By<contractID> { }

			/// <summary>
			/// Approver
			/// </summary>
			public class Approver : EPEmployee.PK.ForeignKeyOf<PMTimeActivity>.By<approverID> { }

			/// <summary>
			/// Original/Corrected Acivity
			/// </summary>
			public class OriginalActivity : PMTimeActivity.PK.ForeignKeyOf<PMTimeActivity>.By<origNoteID> { }

			/// <summary>
			/// Labor Item
			/// </summary>
			public class LaborItem : InventoryItem.PK.ForeignKeyOf<PMTimeActivity>.By<labourItemID> { }

			/// <summary>
			/// Overtime Labor Item
			/// </summary>
			public class OvertimeItem : InventoryItem.PK.ForeignKeyOf<PMTimeActivity>.By<overtimeItemID> { }

			/// <summary>
			/// Shift Code
			/// </summary>
			public class ShiftCode : EPShiftCode.PK.ForeignKeyOf<PMTimeActivity>.By<shiftID> { }
		}
		#endregion

		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected { get; set; }
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		
		[PXDBGuid(true, IsKey = true)]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region RefNoteID
		public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }

		/// <summary>
		/// The identifier of the related <see cref="CRActivity"/>.
		/// This field is included in <see cref="FK.Related"/>.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="CRActivity.NoteID"/> field.
		/// </value>
		[PXSequentialSelfRefNote(SuppressActivitiesCount = true, NoteField = typeof(noteID))]
		[PXUIField(Visible = false)]
		[PXParent(typeof(Select<CRActivity, Where<CRActivity.noteID, Equal<Current<refNoteID>>>>), ParentCreate = true)]
		public virtual Guid? RefNoteID { get; set; }
		#endregion

		#region ParentTaskNoteID
		public abstract class parentTaskNoteID : PX.Data.BQL.BqlGuid.Field<parentTaskNoteID> { }

		[PXDBGuid]
		[PXDBDefault(null, PersistingCheck = PXPersistingCheck.Nothing)]		
        [CRTaskSelector]
		[PXRestrictor(typeof(Where<CRActivity.ownerID, Equal<Current<AccessInfo.contactID>>>), null)]
		[PXUIField(DisplayName = "Task")]
		public virtual Guid? ParentTaskNoteID { get; set; }
		#endregion

		#region TrackTime
		public abstract class trackTime : PX.Data.BQL.BqlBool.Field<trackTime> { }
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Track Time and Costs")]
        [PXFormula(typeof(IIf<
            Where<Current2<CRActivity.classID>, Equal<CRActivityClass.activity>, And<FeatureInstalled<FeaturesSet.timeReportingModule>>>,
            IsNull<Selector<Current<CRActivity.type>, EPActivityType.requireTimeByDefault>, False>, False>))]
        public virtual bool? TrackTime { get; set; }
        #endregion

		#region TimeCardCD
		public abstract class timeCardCD : PX.Data.BQL.BqlString.Field<timeCardCD> { }

		[PXDBString(10)]
		[PXUIField(Visible = false)]
		public virtual string TimeCardCD { get; set; }
		#endregion
		
		#region TimeSheetCD
		public abstract class timeSheetCD : PX.Data.BQL.BqlString.Field<timeSheetCD> { }

		[PXDBString(15)]
		[PXUIField(Visible = false)]
		public virtual string TimeSheetCD { get; set; }
		#endregion

		#region Summary
		public abstract class summary : PX.Data.BQL.BqlString.Field<summary> { }

		[PXDBString(Common.Constants.TranDescLength, InputMask = "", IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Summary", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		[PXNavigateSelector(typeof(summary))]
		public virtual string Summary { get; set; }
		#endregion

		#region Date
		public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }

		[PXDBDateAndTime(DisplayNameDate = "Date", DisplayNameTime = "Time", UseTimeZone = true)]
		[PXUIField(DisplayName = "Date")]
		[PXFormula(typeof(IsNull<Current<CRActivity.startDate>, Current<CRSMEmail.startDate>>))]
		public virtual DateTime? Date { get; set; }
		#endregion

		#region DayOfWeek
		public abstract class dayOfWeek: PX.Data.BQL.BqlInt.Field<dayOfWeek> { }

		[PXInt(MaxValue = 6)]
		[PXUIField(DisplayName = "Day")]
		[PXDependsOnFields(typeof(date))]
		public virtual int? DayOfWeek => (int?)Date?.DayOfWeek;
		#endregion

		#region Owner
		public abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }

		[PXChildUpdatable(AutoRefresh = true)]
		[SubordinateOwnerEmployee]
		public virtual int? OwnerID { get; set; }
		#endregion

		#region EarningTypeID
		public abstract class earningTypeID : PX.Data.BQL.BqlString.Field<earningTypeID> { }

		[PXDBString(EPEarningType.typeCD.Length, IsUnicode = true, InputMask = EPEarningType.typeCD.InputMask)]
		[PXDefault("RG", typeof(Search<EPSetup.regularHoursType>), PersistingCheck = PXPersistingCheck.Null)]
		[PXRestrictor(typeof(Where<EPEarningType.isActive, Equal<True>>), EP.Messages.EarningTypeInactive, typeof(EPEarningType.typeCD))]
		[PXSelector(typeof(EPEarningType.typeCD), DescriptionField = typeof(EPEarningType.description))]
		[PXUIField(DisplayName = "Earning Type")]
		public virtual string EarningTypeID { get; set; }
		#endregion

		#region IsBillable
		public abstract class isBillable : PX.Data.BQL.BqlBool.Field<isBillable> { }

		[PXDBBool]
		[PXUIField(DisplayName = "Billable", FieldClass = "BILLABLE")]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Switch<
			Case<Where<IsNull<Current<CRActivity.classID>, Current<CRSMEmail.classID>>, Equal<CRActivityClass.task>, 
				Or<IsNull<Current<CRActivity.classID>, Current<CRSMEmail.classID>>, Equal<CRActivityClass.events>>>, False,
			Case<Where2<FeatureInstalled<FeaturesSet.timeReportingModule>, And<trackTime, Equal<True>, And<earningTypeID, IsNotNull>>>,
				Selector<earningTypeID, EPEarningType.isbillable>>>,
			False>), KeepIdleSelfUpdates = true)]
		public virtual bool? IsBillable { get; set; }
		#endregion

		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

		[EPActivityProjectDefault(typeof(isBillable))]
		[EPProject(typeof(ownerID), FieldClass = ProjectAttribute.DimensionName)]
		[PXFormula(typeof(
			Switch<
				Case<Where<Not<FeatureInstalled<FeaturesSet.projectModule>>>, DefaultValue<projectID>,
				Case<Where<isBillable, Equal<True>, And<Current2<projectID>, Equal<NonProject>>>, Null,
				Case<Where<isBillable, Equal<False>, And<Current2<projectID>, IsNull>>, DefaultValue<projectID>>>>,
			projectID>))]
		public virtual int? ProjectID { get; set; }
		#endregion

		#region ProjectTaskID
		public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
		[PXDefault(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<projectID>>, And<PMTask.isDefault, Equal<True>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[ProjectTask(typeof(projectID), BatchModule.TA, DisplayName = "Project Task")]
		[PXFormula(typeof(Switch<
			Case<Where<Current2<projectID>, Equal<NonProject>>, Null>,
			projectTaskID>))]
		[PXForeignReference(typeof(CompositeKey<Field<projectID>.IsRelatedTo<PMTask.projectID>, Field<projectTaskID>.IsRelatedTo<PMTask.taskID>>))]
		public virtual int? ProjectTaskID { get; set; }
		#endregion

		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		[CostCode(null, typeof(projectTaskID), GL.AccountType.Expense, ReleasedField = typeof(released))]
		public virtual Int32? CostCodeID
		{
			get;
			set;
		}
		#endregion

		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }
		protected String _ExtRefNbr;
		[PXDBString(30, IsUnicode = true)]
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

		#region CertifiedJob
		public abstract class certifiedJob : PX.Data.BQL.BqlBool.Field<certifiedJob> { }
		[PXDBBool()]
		[PXDefault(typeof(Coalesce<Search<PMProject.certifiedJob, Where<PMProject.contractID, Equal<Current<projectID>>>>,
			Search<PMProject.certifiedJob, Where<PMProject.nonProject, Equal<True>>>>))]
		[PXUIField(DisplayName = "Certified Job", FieldClass = nameof(FeaturesSet.Construction))]
		public virtual Boolean? CertifiedJob
		{
			get; set;
		}
		#endregion

		#region UnionID
		public abstract class unionID : PX.Data.BQL.BqlString.Field<unionID> { }
		[PXForeignReference(typeof(Field<unionID>.IsRelatedTo<PMUnion.unionID>))]
		[PMUnion(typeof(projectID), typeof(Select<EPEmployee, Where<EPEmployee.defContactID, Equal<Current<ownerID>>>>))]
		public virtual String UnionID
		{
			get;
			set;
		}
		#endregion

		#region ApproverID
		public abstract class approverID : PX.Data.BQL.BqlInt.Field<approverID> { }

        [PXDBInt]
        [PXEPEmployeeSelector]
        [PXFormula(typeof(
            Switch<
                Case<Where<Current2<projectID>, Equal<NonProject>>, Null, Case<Where<Current2<projectTaskID>, IsNull>, Null>>,
                Selector<projectTaskID, PMTask.approverID>>
            ))]
        [PXUIField(DisplayName = "Approver", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual int? ApproverID { get; set; }
        #endregion

		#region ApprovalStatus
		public abstract class approvalStatus : PX.Data.BQL.BqlString.Field<approvalStatus> { }

		[PXDBString(2, IsFixed = true)]
		[ApprovalStatus]
		[PXUIField(DisplayName = "Approval Status", Enabled = false)]
		[PXFormula(typeof(Switch<
			Case<Where<trackTime, Equal<True>, And<
				Where<Current2<approvalStatus>, IsNull, Or<Current2<approvalStatus>, Equal<ActivityStatusAttribute.open>>>>>, ActivityStatusAttribute.open,
			Case<Where<released, Equal<True>>, ActivityStatusAttribute.released,
			Case<Where<approverID, IsNotNull>, ActivityStatusAttribute.pendingApproval>>>,
			ActivityStatusAttribute.completed>))]
		public virtual string ApprovalStatus { get; set; }

		#endregion

		#region ApprovedDate
		public abstract class approvedDate : PX.Data.BQL.BqlDateTime.Field<approvedDate> { }

		[PXDBDate(DisplayMask = "d", PreserveTime = true)]
		[PXUIField(DisplayName = "Approved Date")]
		public virtual DateTime? ApprovedDate { get; set; }
		#endregion

		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Workgroup")]
		[PXWorkgroupSelector]
		[PXParent(typeof(Select<EPTimeActivitiesSummary, 
			Where<EPTimeActivitiesSummary.workgroupID, Equal<Current<workgroupID>>, 
				And<EPTimeActivitiesSummary.week, Equal<Current<weekID>>, 
				And<EPTimeActivitiesSummary.contactID, Equal<Current<ownerID>>>>>>),
			ParentCreate = true,
			LeaveChildren = true)]
		[PXDefault(typeof(SearchFor<EPEmployee.defaultWorkgroupID>
			.Where<EPEmployee.defContactID.IsEqual<PMTimeActivity.ownerID.FromCurrent>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? WorkgroupID { get; set; }
		#endregion

		#region ContractID
		public abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Contract", Visible = false)]
		[PXSelector(typeof(Search2<Contract.contractID,
			LeftJoin<ContractBillingSchedule, On<Contract.contractID, Equal<ContractBillingSchedule.contractID>>>,
			Where<Contract.baseType, Equal<CTPRType.contract>>,
			OrderBy<Desc<Contract.contractCD>>>),
			DescriptionField = typeof(Contract.description),
			SubstituteKey = typeof(Contract.contractCD), Filterable = true)]
		[PXRestrictor(typeof(Where<Contract.status, Equal<Contract.status.active>>), Messages.ContractIsNotActive)]
		[PXRestrictor(typeof(Where<Current<AccessInfo.businessDate>, LessEqual<Contract.graceDate>, Or<Contract.expireDate, IsNull>>), Messages.ContractExpired)]
		[PXRestrictor(typeof(Where<Current<AccessInfo.businessDate>, GreaterEqual<Contract.startDate>>), Messages.ContractActivationDateInFuture, typeof(Contract.startDate))]
		public virtual int? ContractID { get; set; }
		#endregion

		#region TimeSpent
		public abstract class timeSpent : PX.Data.BQL.BqlInt.Field<timeSpent> { }

		[PXDBInt]
		[PXTimeList]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Time Spent")]
		[PXUnboundFormula(typeof(Switch<Case<Where<Selector<earningTypeID, EPEarningType.isOvertime>, Equal<False>>, timeSpent>, int0>), typeof(SumCalc<EPTimeActivitiesSummary.totalRegularTime>))]
		[PXUnboundFormula(typeof(timeSpent.When<dayOfWeek.IsEqual<int0>>.Else<int0>), typeof(SumCalc<EPTimeActivitiesSummary.sundayTime>))]
		[PXUnboundFormula(typeof(timeSpent.When<dayOfWeek.IsEqual<int1>>.Else<int0>), typeof(SumCalc<EPTimeActivitiesSummary.mondayTime>))]
		[PXUnboundFormula(typeof(timeSpent.When<dayOfWeek.IsEqual<int2>>.Else<int0>), typeof(SumCalc<EPTimeActivitiesSummary.tuesdayTime>))]
		[PXUnboundFormula(typeof(timeSpent.When<dayOfWeek.IsEqual<int3>>.Else<int0>), typeof(SumCalc<EPTimeActivitiesSummary.wednesdayTime>))]
		[PXUnboundFormula(typeof(timeSpent.When<dayOfWeek.IsEqual<int4>>.Else<int0>), typeof(SumCalc<EPTimeActivitiesSummary.thursdayTime>))]
		[PXUnboundFormula(typeof(timeSpent.When<dayOfWeek.IsEqual<int5>>.Else<int0>), typeof(SumCalc<EPTimeActivitiesSummary.fridayTime>))]
		[PXUnboundFormula(typeof(timeSpent.When<dayOfWeek.IsEqual<int6>>.Else<int0>), typeof(SumCalc<EPTimeActivitiesSummary.saturdayTime>))]
		public virtual int? TimeSpent { get; set; }
		#endregion

		#region OvertimeSpent
		public abstract class overtimeSpent : PX.Data.BQL.BqlInt.Field<overtimeSpent> { }

		[PXDBInt]
		[PXTimeList]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Switch<Case<Where<Selector<earningTypeID, EPEarningType.isOvertime>, Equal<True>>, timeSpent>, int0>))]
		[PXUIField(DisplayName = "Overtime", Enabled = false)]
		[PXFormula(null, typeof(SumCalc<EPTimeActivitiesSummary.totalOvertime>))]
		public virtual int? OvertimeSpent { get; set; }
		#endregion

		#region TimeBillable
		public abstract class timeBillable : PX.Data.BQL.BqlInt.Field<timeBillable> { }

		[PXDBInt]
		[PXTimeList]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(
			Switch<Case<Where<isBillable, Equal<True>>, timeSpent,
				Case<Where<isBillable, Equal<False>>, int0>>,
				timeBillable>))]
		[PXUIField(DisplayName = "Billable Time", FieldClass = "BILLABLE")]
		[PXUIVerify(typeof(Where<isBillable,Equal<False>,
			Or<timeSpent, IsNull,
			Or<timeBillable, IsNull, 
				Or<timeSpent, GreaterEqual<timeBillable>>>>>), 
			PXErrorLevel.Error, Messages.BillableTimeCannotBeGreaterThanTimeSpent)]
		[PXUIVerify(typeof(Where<isBillable, NotEqual<True>, 
			Or<timeBillable, NotEqual<int0>>>), PXErrorLevel.Error, Messages.BillableTimeMustBeOtherThanZero,
			CheckOnInserted = false, CheckOnVerify = false)]
		[PXUnboundFormula(typeof(Switch<Case<Where<Selector<earningTypeID, EPEarningType.isOvertime>, Equal<False>>, timeBillable>, int0>), typeof(SumCalc<EPTimeActivitiesSummary.totalBillableTime>))]
		public virtual int? TimeBillable { get; set; }
		#endregion

		#region OvertimeBillable
		public abstract class overtimeBillable : PX.Data.BQL.BqlInt.Field<overtimeBillable> { }

		[PXDBInt]
		[PXTimeList]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIVerify(typeof(Where<overtimeSpent, IsNull, 
			Or<overtimeBillable, IsNull, 
				Or<overtimeSpent, GreaterEqual<overtimeBillable>>>>), PXErrorLevel.Error, Messages.OvertimeBillableCannotBeGreaterThanOvertimeSpent)]
		[PXFormula(typeof(
			Switch<Case<Where<isBillable, Equal<True>, And<overtimeSpent, GreaterEqual<timeBillable>>>, timeBillable,
				Case<Where<isBillable, Equal<True>, And<overtimeSpent, GreaterEqual<Zero>>>, overtimeBillable,
					Case<Where<isBillable, Equal<False>>, int0>>>,
				overtimeBillable>))]
		[PXUIField(DisplayName = "Billable Overtime", FieldClass = "BILLABLE")]
		[PXFormula(null, typeof(SumCalc<EPTimeActivitiesSummary.totalBillableOvertime>))]
		public virtual int? OvertimeBillable { get; set; }
		#endregion

		#region Billed
		public abstract class billed : PX.Data.BQL.BqlBool.Field<billed> { }

		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Billed", FieldClass = "BILLABLE")]
		public virtual bool? Billed { get; set; }
		#endregion

		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }

		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Released", Enabled = false, Visible = false, FieldClass = "BILLABLE")]
		public virtual bool? Released { get; set; }
		#endregion

		#region IsCorrected
		public abstract class isCorrected : PX.Data.BQL.BqlBool.Field<isCorrected> { }

		/// <summary>
		/// If true this Activity has been corrected in the Timecard and is no longer valid. Please hide this activity in all lists displayed in the UI since there is another valid activity.
		/// The valid activity has a refence back to the corrected activity via OrigTaskID field. 
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? IsCorrected { get; set; }
		#endregion

		#region OrigNoteID
		public abstract class origNoteID : PX.Data.BQL.BqlGuid.Field<origNoteID> { }

		/// <summary>
		/// Use for correction. Stores the reference to the original activity.
		/// </summary>
		[PXDBGuid]
		public virtual Guid? OrigNoteID { get; set; }
		#endregion

		#region TranID
		public abstract class tranID : PX.Data.BQL.BqlLong.Field<tranID> { }

		[PXDBLong]
		public virtual long? TranID { get; set; }
		#endregion

		#region WeekID
		public abstract class weekID : PX.Data.BQL.BqlInt.Field<weekID> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Time Card Week", Enabled = false)]
		[PXWeekSelector2]
		[PXFormula(typeof(Default<date>))]
		[EPActivityDefaultWeek(typeof(date))]
		public virtual int? WeekID { get; set; }
		#endregion

		#region LabourItemID
		public abstract class labourItemID : PX.Data.BQL.BqlInt.Field<labourItemID> { }

		[PMLaborItem(typeof(projectID), typeof(earningTypeID), typeof(Select<EPEmployee, Where<EPEmployee.defContactID, Equal<Current<ownerID>>>>))]
		[PXForeignReference(typeof(Field<labourItemID>.IsRelatedTo<InventoryItem.inventoryID>))]
		public virtual int? LabourItemID { get; set; }
		#endregion

		#region WorkCodeID
		public abstract class workCodeID : PX.Data.BQL.BqlString.Field<workCodeID> { }
		[PXForeignReference(typeof(FK.WorkCode))]
		[PMWorkCodeInTimeActivity(typeof(costCodeID), typeof(projectID), typeof(projectTaskID), typeof(labourItemID), typeof(ownerID))]
		public virtual string WorkCodeID { get; set; }
		#endregion

		#region OvertimeItemID
		public abstract class overtimeItemID : PX.Data.BQL.BqlInt.Field<overtimeItemID> { }

		[PXDBInt]
		[PXUIField(Visible = false)]
		public virtual int? OvertimeItemID { get; set; }
		#endregion

		#region JobID
		public abstract class jobID : PX.Data.BQL.BqlInt.Field<jobID> { }

		[PXDBInt]
		public virtual int? JobID { get; set; }
		#endregion

		#region ShiftID
		public abstract class shiftID : PX.Data.BQL.BqlInt.Field<shiftID> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Shift Code", FieldClass = nameof(FeaturesSet.ShiftDifferential))]
		[TimeActivityShiftCodeSelector(typeof(ownerID), typeof(date))]
		[EPShiftCodeActiveRestrictor]
		public virtual int? ShiftID { get; set; }
		#endregion

		#region EmployeeRate
		public abstract class employeeRate : PX.Data.BQL.BqlDecimal.Field<employeeRate> { }

		/// <summary>
		/// Stores Employee's Hourly rate at the time the activity was released to PM
		/// </summary>
		[IN.PXDBPriceCost]
		[PXUIField(DisplayName = "Cost Rate", Enabled = false)]
		public virtual decimal? EmployeeRate { get; set; }
		#endregion

		#region SummaryLineNbr
		public abstract class summaryLineNbr : PX.Data.BQL.BqlInt.Field<summaryLineNbr> { }

		/// <summary>
		/// This is a adjusting activity for the summary line in the Timecard.
		/// </summary>
		[PXDBInt]
		public virtual int? SummaryLineNbr { get; set; }
		#endregion

		#region ARDocType
		public abstract class arDocType : PX.Data.BQL.BqlString.Field<arDocType> { }
		[AR.ARDocType.List()]
		[PXString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual String ARDocType { get; set; }
		#endregion

		#region ARRefNbr
		public abstract class arRefNbr : PX.Data.BQL.BqlString.Field<arRefNbr> { }
		[PXString(15, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<AR.ARRegister.refNbr, Where<AR.ARRegister.docType, Equal<Current<arDocType>>>>), DescriptionField = typeof(AR.ARRegister.docType))]
		public virtual string ARRefNbr { get; set; }
		#endregion

		#region ReportedInTimeZoneID
		public abstract class reportedInTimeZoneID : PX.Data.BQL.BqlString.Field<reportedInTimeZoneID> { }

		[PXUIField(DisplayName = "Reported in Time Zone", Enabled = false, Visible = false)]
		[PXDBString(32)]
		[PXTimeZone]
		public virtual String ReportedInTimeZoneID { get; set; }
		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID(DontOverrideValue = true)]
		[PXUIField(Enabled = false)]
		public virtual Guid? CreatedByID { get; set; }
		#endregion

		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID { get; set; }
		#endregion

		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXUIField(DisplayName = "Created At", Enabled = false)]
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

		[PXDBTimestamp]
		public virtual byte[] tstamp { get; set; }
		#endregion

		#region NeedToBeDeleted
		public abstract class needToBeDeleted : PX.Data.BQL.BqlBool.Field<needToBeDeleted> { }

		[PXBool]
		[PXFormula(typeof(Switch<
				Case<Where<trackTime, NotEqual<True>,
					And<Where<projectID, IsNull, Or<projectID, Equal<NonProject>>>>>, True>,
			False>))]
		public bool? NeedToBeDeleted { get; set; }
		#endregion

		#region IsActivityExists
		public abstract class isActivityExists : PX.Data.BQL.BqlBool.Field<isActivityExists> { }

		[PXBool]
		[PXUIField(DisplayName = "Activity Exists", Enabled = false, Visible = false)]
		[PXFormula(typeof(Switch<
				Case<Where<refNoteID, NotEqual<noteID>>, True>,
			Null>))]
		public bool? IsActivityExists { get; set; }
		#endregion
	}

	[Serializable]
	[PXBreakInheritance]
	[PXProjection(
		typeof(SelectFrom<CRActivity>
			.LeftJoin<PMTimeActivity>
				.On<PMTimeActivity.refNoteID.IsEqual<CRActivity.noteID>
					.And<PMTimeActivity.isCorrected.IsEqual<False>>>
		),
		Persistent = true)]
	public partial class CRPMTimeActivity : CRActivity
	{
		#region Selected
		public new abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		#endregion
		
		#region CRActivity

		#region NoteID
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		#endregion

		#region ParentNoteID
		public new abstract class parentNoteID : PX.Data.BQL.BqlGuid.Field<parentNoteID> { }
		#endregion

		#region RefNoteType
		public new abstract class refNoteIDType : PX.Data.BQL.BqlString.Field<refNoteIDType> { }
		#endregion

		#region RefNoteID
		public new abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }
		#endregion

		#region DocumentNoteID
		public new abstract class documentNoteID : PX.Data.BQL.BqlGuid.Field<documentNoteID> { }
		#endregion

		#region Source
		public new abstract class source : PX.Data.BQL.BqlString.Field<source> { }
		#endregion

		#region ClassID
		public new abstract class classID : PX.Data.BQL.BqlInt.Field<classID> { }
		#endregion

		#region ClassIcon
		public new abstract class classIcon : PX.Data.BQL.BqlString.Field<classIcon> { }
		#endregion

		#region ClassInfo
		public new abstract class classInfo : PX.Data.BQL.BqlString.Field<classInfo> { }
		#endregion

		#region Type
		public new abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		#endregion

		#region Subject
		public new abstract class subject : PX.Data.BQL.BqlString.Field<subject> { }
		#endregion

		#region Location
		public new abstract class location : PX.Data.BQL.BqlString.Field<location> { }
		#endregion

		#region Body
		public new abstract class body : PX.Data.BQL.BqlString.Field<body> { }
		#endregion

		#region Priority
		public new abstract class priority : PX.Data.BQL.BqlInt.Field<priority> { }
		#endregion

		#region PriorityIcon
		public new abstract class priorityIcon : PX.Data.BQL.BqlString.Field<priorityIcon> { }
		#endregion

		#region UIStatus
		public new abstract class uistatus : PX.Data.BQL.BqlString.Field<uistatus> { }

		/// <inheritdoc />
		[PXDBString(2, IsFixed = true, BqlField = typeof(CRActivity.uistatus))]
		[PXFormula(typeof(Switch<
			Case<Where<type, IsNull>, ActivityStatusAttribute.open,
			Case<Where<trackTime, IsNull, Or<trackTime, Equal<False>>>, ActivityStatusAttribute.completed>>,
			ActivityStatusAttribute.open>))]
		[ActivityStatus]
		[PXUIField(DisplayName = "Status")]
		[PXDefault(ActivityStatusAttribute.Open, PersistingCheck = PXPersistingCheck.Nothing)]
		public override string UIStatus { get; set; }
		#endregion

		#region IsOverdue
		public new abstract class isOverdue : PX.Data.BQL.BqlBool.Field<isOverdue> { }
		#endregion

		#region IsCompleteIcon
		public new abstract class isCompleteIcon : PX.Data.BQL.BqlString.Field<isCompleteIcon> { }

		/// <inheritdoc />
		[PXUIField(DisplayName = "Complete Icon", IsReadOnly = true)]
		[PXImage(HeaderImage = (Sprite.AliasControl + "@" + Sprite.Control.CompleteHead))]
		[PXFormula(typeof(Switch<
			Case<Where<uistatus, Equal<ActivityStatusListAttribute.completed>>, CRActivity.isCompleteIcon.completed,
			Case<Where<approvalStatus, Equal<ActivityStatusListAttribute.completed>, Or<approvalStatus, Equal<ActivityStatusListAttribute.released>>>, CRActivity.isCompleteIcon.completed>>>))]
		public override String IsCompleteIcon { get; set; }
		#endregion

		#region CategoryID
		public new abstract class categoryID : PX.Data.BQL.BqlInt.Field<categoryID> { }
		#endregion

		#region AllDay
		public new abstract class allDay : PX.Data.BQL.BqlBool.Field<allDay> { }
		#endregion

		#region StartDate
		public new abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		#endregion

		#region EndDate
		public new abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
		#endregion

		#region CompletedDate
		public new abstract class completedDate : PX.Data.BQL.BqlDateTime.Field<completedDate> { }
		#endregion

		#region DayOfWeek
		public new abstract class dayOfWeek : PX.Data.BQL.BqlInt.Field<dayOfWeek> { }
		#endregion

		#region PercentCompletion
		public new abstract class percentCompletion : PX.Data.BQL.BqlInt.Field<percentCompletion> { }
		#endregion

		#region Owner
		public new abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }
		#endregion

		#region Workgroup
		public new abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }

		/// <inheritdoc />
		[PXDBInt(BqlField = typeof(CRActivity.workgroupID))]
		[PXChildUpdatable(UpdateRequest = true)]
		[PXUIField(DisplayName = "Workgroup")]
		[PXSubordinateGroupSelector]
		public override int? WorkgroupID { get; set; }
		#endregion

		#region IsExternal
		public new abstract class isExternal : PX.Data.BQL.BqlBool.Field<isExternal> { }
		#endregion

		#region IsPrivate
		public new abstract class isPrivate : PX.Data.BQL.BqlBool.Field<isPrivate> { }
		#endregion

		#region Incoming
		public new abstract class incoming : PX.Data.BQL.BqlBool.Field<incoming> { }
		#endregion

		#region Outgoing
		public new abstract class outgoing : PX.Data.BQL.BqlBool.Field<outgoing> { }
		#endregion

		#region Synchronize
		public new abstract class synchronize : PX.Data.BQL.BqlBool.Field<synchronize> { }
		#endregion

		#region BAccountID
		public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		#endregion

		#region ContactID
		public new abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }
		#endregion

		#region EntityDescription

		public new abstract class entityDescription : PX.Data.BQL.BqlString.Field<entityDescription> { }
		#endregion

		#region ShowAsID
		public new abstract class showAsID : PX.Data.BQL.BqlInt.Field<showAsID> { }
		#endregion

		#region IsLocked
		public new abstract class isLocked : PX.Data.BQL.BqlBool.Field<isLocked> { }
		#endregion

		#region DeletedDatabaseRecord
		// Acuminator disable once PX1027 ForbiddenFieldsInDacDeclaration [it is needed for Exchange sync]
		public new abstract class deletedDatabaseRecord : PX.Data.BQL.BqlBool.Field<deletedDatabaseRecord> { }
		#endregion

		#region CreatedDateTime
		public new abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		#endregion

		#endregion

		#region PMTimeActivity

		#region TimeActivityNoteID
		public abstract class timeActivityNoteID : PX.Data.BQL.BqlGuid.Field<timeActivityNoteID> { }

		[PXDBSequentialGuid(BqlField = typeof(PMTimeActivity.noteID))]
		[PXExtraKey]
		public virtual Guid? TimeActivityNoteID { get; set; }
		#endregion

		#region TimeActivityRefNoteID
		public abstract class timeActivityRefNoteID : PX.Data.BQL.BqlGuid.Field<timeActivityRefNoteID> { }

		[PXDBGuid(BqlField = typeof(PMTimeActivity.refNoteID))]
		public virtual Guid? TimeActivityRefNoteID { get; set; }
		#endregion

		#region ParentTaskNoteID
		public abstract class parentTaskNoteID : PX.Data.BQL.BqlGuid.Field<parentTaskNoteID> { }

		[PXDBGuid(BqlField = typeof(PMTimeActivity.parentTaskNoteID))]
		[PXDBDefault(null, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Task")]
		[PXFormula(typeof(
			Switch<
				Case<Where<Current2<classID>, Equal<CRActivityClass.task>>, noteID>,
			parentTaskNoteID>))]
		public virtual Guid? ParentTaskNoteID { get; set; }
		#endregion

		#region TrackTime
		public abstract class trackTime : PX.Data.BQL.BqlBool.Field<trackTime> { }

		[PXDBBool(BqlField = typeof(PMTimeActivity.trackTime))]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Track Time")]
        [PXFormula(typeof(IIf<
            Where<Current2<CRActivity.classID>, Equal<CRActivityClass.activity>, And<FeatureInstalled<FeaturesSet.timeReportingModule>>>,
            IsNull<Selector<Current<CRActivity.type>, EPActivityType.requireTimeByDefault>, False>, False>))]
        public virtual bool? TrackTime { get; set; }
		#endregion

		#region TimeCardCD
		public abstract class timeCardCD : PX.Data.BQL.BqlString.Field<timeCardCD> { }

		[PXDBString(10, BqlField = typeof(PMTimeActivity.timeCardCD))]
		[PXUIField(Visible = false)]
		public virtual string TimeCardCD { get; set; }
		#endregion

		#region TimeSheetCD
		public abstract class timeSheetCD : PX.Data.BQL.BqlString.Field<timeSheetCD> { }

		[PXDBString(15, BqlField = typeof(PMTimeActivity.timeSheetCD))]
		[PXUIField(Visible = false)]
		public virtual string TimeSheetCD { get; set; }
		#endregion

		#region Summary
		public abstract class summary : PX.Data.BQL.BqlString.Field<summary> { }

		[PXDBString(Common.Constants.TranDescLength, InputMask = "", IsUnicode = true, BqlField = typeof(PMTimeActivity.summary))]
		[PXDefault]
		[PXFormula(typeof(subject))]
		[PXUIField(DisplayName = "Summary", Visibility = PXUIVisibility.SelectorVisible)]
		[PXNavigateSelector(typeof(summary))]
		public virtual string Summary { get; set; }
		#endregion

		#region Date
		public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }

		[PXDBDateAndTime(DisplayNameDate = "Date", DisplayNameTime = "Time", UseTimeZone = true, BqlField = typeof(PMTimeActivity.date))]
		[PXUIField(DisplayName = "Date")]
		[PXFormula(typeof(startDate))]
		public virtual DateTime? Date { get; set; }
		#endregion

		#region TimeActivityOwner
		public abstract class timeActivityOwner : PX.Data.BQL.BqlInt.Field<timeActivityOwner> { }

		[PXChildUpdatable(AutoRefresh = true)]
		[Owner(typeof(workgroupID), BqlField = typeof(PMTimeActivity.ownerID))]
		[PXFormula(typeof(ownerID))]
		public virtual int? TimeActivityOwner { get; set; }
		#endregion

		#region ApproverID
		public abstract class approverID : PX.Data.BQL.BqlInt.Field<approverID> { }

		[PXDBInt(BqlField = typeof(PMTimeActivity.approverID))]
		[PXEPEmployeeSelector]
		[PXFormula(typeof(
			Switch<
				Case<Where<Current2<projectID>, Equal<NonProject>>, Null,
				Case<Where<Current2<projectTaskID>, IsNull>, Null>>,
				Selector<projectTaskID, PMTask.approverID>>
			))]
		[PXUIField(DisplayName = "Approver", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? ApproverID { get; set; }
		#endregion

		#region ApprovalStatus
		public abstract class approvalStatus : PX.Data.BQL.BqlString.Field<approvalStatus> { }

		[PXDBString(2, IsFixed = true, BqlField = typeof(PMTimeActivity.approvalStatus))]
		[ActivityStatusList]
		[PXUIField(DisplayName = "Approval Status", Enabled = false)]
		[PXFormula(typeof(Switch<
			Case<Where<trackTime, Equal<True>, And<Current2<approvalStatus>, IsNull>>, ActivityStatusAttribute.open,
			Case<Where<released, Equal<True>>, ActivityStatusAttribute.released,
			Case<Where<approverID, IsNotNull>, ActivityStatusAttribute.pendingApproval>>>,
			ActivityStatusAttribute.completed>))]
		public virtual string ApprovalStatus { get; set; }

		#endregion

		#region ApprovedDate
		public abstract class approvedDate : PX.Data.BQL.BqlDateTime.Field<approvedDate> { }

		[PXDBDate(DisplayMask = "d", PreserveTime = true, BqlField = typeof(PMTimeActivity.approvedDate))]
		[PXUIField(DisplayName = "Approved Date")]
		public virtual DateTime? ApprovedDate { get; set; }
		#endregion

		#region EarningTypeID
		public abstract class earningTypeID : PX.Data.BQL.BqlString.Field<earningTypeID> { }

		[PXDBString(EPEarningType.typeCD.Length, IsUnicode = true, InputMask = EPEarningType.typeCD.InputMask, BqlField = typeof(PMTimeActivity.earningTypeID))]
		[PXDefault("RG", typeof(Search<EPSetup.regularHoursType>), PersistingCheck = PXPersistingCheck.Null)]
		[PXUIRequired(typeof(trackTime))]
		[PXRestrictor(typeof(Where<EPEarningType.isActive, Equal<True>>), EP.Messages.EarningTypeInactive, typeof(EPEarningType.typeCD))]
		[PXSelector(typeof(EPEarningType.typeCD), DescriptionField = typeof(EPEarningType.description))]
		[PXUIField(DisplayName = "Earning Type")]
		public virtual string EarningTypeID { get; set; }
		#endregion

		#region IsBillable
		public abstract class isBillable : PX.Data.BQL.BqlBool.Field<isBillable> { }

		[PXDBBool(BqlField = typeof(PMTimeActivity.isBillable))]
		[PXUIField(DisplayName = "Billable", FieldClass = "BILLABLE")]
		[PXFormula(typeof(Switch<
			Case<Where<classID, Equal<CRActivityClass.task>, Or<classID, Equal<CRActivityClass.events>>>, False,
			Case<Where2<FeatureInstalled<FeaturesSet.timeReportingModule>, And<trackTime, Equal<True>>>,
				IsNull<Selector<earningTypeID, EPEarningType.isbillable>, False>>>,
			False>))]
		public virtual bool? IsBillable { get; set; }
		#endregion

		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

		[EPActivityProjectDefault(typeof(isBillable))]
		[EPProject(typeof(ownerID), FieldClass = ProjectAttribute.DimensionName, BqlField = typeof(PMTimeActivity.projectID))]
		[PXFormula(typeof(Switch<
			Case<Where<Not<FeatureInstalled<FeaturesSet.projectModule>>>, DefaultValue<projectID>,
			Case<Where<parentNoteID, IsNotNull,
					And<Selector<parentNoteID, Selector<projectID, PMProject.contractCD>>, IsNotNull>>,
				Selector<parentNoteID, Selector<projectID, PMProject.contractCD>>,
			Case<Where<isBillable, Equal<True>, And<Current2<projectID>, Equal<NonProject>>>, Null,
			Case<Where<isBillable, Equal<False>, And<Current2<projectID>, IsNull>>, DefaultValue<projectID>>>>>,
			projectID>))]
		public virtual int? ProjectID { get; set; }
		#endregion

		#region ProjectTaskID
		public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
		
		[ProjectTask(typeof(projectID), BatchModule.TA, DisplayName = "Project Task", BqlField = typeof(PMTimeActivity.projectTaskID))]
		[PXFormula(typeof(Switch<
			Case<Where<Current2<projectID>, Equal<NonProject>>, Null,
			Case<Where<parentNoteID, IsNotNull>,
				Selector<parentNoteID, Selector<projectTaskID, PMTask.taskCD>>>>,
			projectTaskID>))]
		public virtual int? ProjectTaskID { get; set; }
		#endregion

		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }

		[CostCode(null, typeof(projectTaskID), GL.AccountType.Expense, BqlField = typeof(PMTimeActivity.costCodeID), ReleasedField = typeof(released))]
		public virtual int? CostCodeID { get; set; }
		#endregion

		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }
		protected String _ExtRefNbr;
		[PXDBString(30, IsUnicode = true, BqlField = typeof(PMTimeActivity.extRefNbr))]
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

		#region ContractID
		public abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }

		[PXDBInt(BqlField = typeof(PMTimeActivity.contractID))]
		[PXUIField(DisplayName = "Contract", Visible = false)]
		[PXSelector(typeof(Search2<Contract.contractID,
				LeftJoin<ContractBillingSchedule, On<Contract.contractID, Equal<ContractBillingSchedule.contractID>>>,
			Where<Contract.baseType, Equal<CTPRType.contract>>,
			OrderBy<Desc<Contract.contractCD>>>),
			DescriptionField = typeof(Contract.description),
			SubstituteKey = typeof(Contract.contractCD), Filterable = true)]
		[PXRestrictor(typeof(Where<Contract.status, Equal<Contract.status.active>>), Messages.ContractIsNotActive)]
		[PXRestrictor(typeof(Where<Current<AccessInfo.businessDate>, LessEqual<Contract.graceDate>, Or<Contract.expireDate, IsNull>>), Messages.ContractExpired)]
		[PXRestrictor(typeof(Where<Current<AccessInfo.businessDate>, GreaterEqual<Contract.startDate>>), Messages.ContractActivationDateInFuture, typeof(Contract.startDate))]
		public virtual int? ContractID { get; set; }
		#endregion

		#region TimeSpent
		public abstract class timeSpent : PX.Data.BQL.BqlInt.Field<timeSpent> { }

		[PXDBInt(BqlField = typeof(PMTimeActivity.timeSpent))]
		[PXTimeList]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Time Spent")]
		[PXFormula(typeof(Switch<Case<Where<trackTime, NotEqual<True>>, int0>, timeSpent>))]
		public virtual int? TimeSpent { get; set; }
		#endregion

		#region OvertimeSpent
		public abstract class overtimeSpent : PX.Data.BQL.BqlInt.Field<overtimeSpent> { }

		[PXDBInt(BqlField = typeof(PMTimeActivity.overtimeSpent))]
		[PXTimeList]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Switch<Case<Where<Selector<earningTypeID, EPEarningType.isOvertime>, Equal<True>>, timeSpent>, int0>))]
		[PXUIField(DisplayName = "Overtime", Enabled = false)]
		public virtual int? OvertimeSpent { get; set; }
		#endregion

		#region TimeBillable
		public abstract class timeBillable : PX.Data.BQL.BqlInt.Field<timeBillable> { }

		[PXDBInt(BqlField = typeof(PMTimeActivity.timeBillable))]
		[PXTimeList]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(
			Switch<Case<Where<isBillable, Equal<True>>, timeSpent,
				Case<Where<isBillable, Equal<False>>, int0>>,
			timeBillable>))]
		[PXUIField(DisplayName = "Billable Time", FieldClass = "BILLABLE")]
		[PXUIVerify(typeof(Where<timeSpent, IsNull,
			Or<timeBillable, IsNull,
			Or<timeSpent, GreaterEqual<timeBillable>>>>), PXErrorLevel.Error, Messages.BillableTimeCannotBeGreaterThanTimeSpent)]
		[PXUIVerify(typeof(Where<isBillable, NotEqual<True>,
			Or<timeBillable, NotEqual<int0>>>), PXErrorLevel.Error, Messages.BillableTimeMustBeOtherThanZero)]
		public virtual int? TimeBillable { get; set; }
		#endregion

		#region OvertimeBillable
		public abstract class overtimeBillable : PX.Data.BQL.BqlInt.Field<overtimeBillable> { }

		[PXDBInt(BqlField = typeof(PMTimeActivity.overtimeBillable))]
		[PXTimeList]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIVerify(typeof(Where<overtimeSpent, IsNull,
			Or<overtimeBillable, IsNull,
				Or<overtimeSpent, GreaterEqual<overtimeBillable>>>>), PXErrorLevel.Error, Messages.OvertimeBillableCannotBeGreaterThanOvertimeSpent)]
		[PXFormula(typeof(
			Switch<Case<Where<isBillable, Equal<True>, And<overtimeSpent, GreaterEqual<timeBillable>>>, timeBillable,
				Case<Where<isBillable, Equal<True>, And<overtimeSpent, GreaterEqual<Zero>>>, overtimeBillable,
				Case<Where<isBillable, Equal<False>>, int0>>>,
			overtimeBillable>))]
		[PXUIField(DisplayName = "Billable Overtime", FieldClass = "BILLABLE")]
		public virtual int? OvertimeBillable { get; set; }
		#endregion

		#region Billed
		public abstract class billed : PX.Data.BQL.BqlBool.Field<billed> { }

		[PXDBBool(BqlField = typeof(PMTimeActivity.billed))]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Billed", FieldClass = "BILLABLE")]
		public virtual bool? Billed { get; set; }
		#endregion

		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }

		[PXDBBool(BqlField = typeof(PMTimeActivity.released))]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Released", Enabled = false, Visible = false, FieldClass = "BILLABLE")]
		public virtual bool? Released { get; set; }
		#endregion

		#region IsCorrected
		public abstract class isCorrected : PX.Data.BQL.BqlBool.Field<isCorrected> { }

		/// <summary>
		/// If true this Activity has been corrected in the Timecard and is no longer valid. Please hide this activity in all lists displayed in the UI since there is another valid activity.
		/// The valid activity has a refence back to the corrected activity via OrigTaskID field. 
		/// </summary>
		[PXDBBool(BqlField = typeof(PMTimeActivity.isCorrected))]
		[PXDefault(false)]
		public virtual bool? IsCorrected { get; set; }
		#endregion

		#region OrigNoteID
		public abstract class origNoteID : PX.Data.BQL.BqlGuid.Field<origNoteID> { }

		/// <summary>
		/// Use for correction. Stores the reference to the original activity.
		/// </summary>
		[PXDBGuid(BqlField = typeof(PMTimeActivity.origNoteID))]
		public virtual Guid? OrigNoteID { get; set; }
		#endregion

		#region TranID
		public abstract class tranID : PX.Data.BQL.BqlLong.Field<tranID> { }

		[PXDBLong(BqlField = typeof(PMTimeActivity.tranID))]
		public virtual long? TranID { get; set; }
		#endregion

		#region WeekID
		public abstract class weekID : PX.Data.BQL.BqlInt.Field<weekID> { }

		[PXDBInt(BqlField = typeof(PMTimeActivity.weekID))]
		[PXUIField(DisplayName = "Time Card Week", Enabled = false)]
		[PXWeekSelector2()]
		[PXFormula(typeof(Default<date, trackTime>))]
		[EPActivityDefaultWeek(typeof(date))]
		public virtual int? WeekID { get; set; }
		#endregion

		#region LabourItemID
		public abstract class labourItemID : PX.Data.BQL.BqlInt.Field<labourItemID> { }

		[PXDBInt(BqlField = typeof(PMTimeActivity.labourItemID))]
		[PXUIField(Visible = false)]
		public virtual int? LabourItemID { get; set; }
		#endregion

		#region OvertimeItemID
		public abstract class overtimeItemID : PX.Data.BQL.BqlInt.Field<overtimeItemID> { }

		[PXDBInt(BqlField = typeof(PMTimeActivity.overtimeItemID))]
		[PXUIField(Visible = false)]
		public virtual int? OvertimeItemID { get; set; }
		#endregion

		#region JobID
		public abstract class jobID : PX.Data.BQL.BqlInt.Field<jobID> { }

		[PXDBInt(BqlField = typeof(PMTimeActivity.jobID))]
		public virtual int? JobID { get; set; }
		#endregion

		#region ShiftID
		public abstract class shiftID : PX.Data.BQL.BqlInt.Field<shiftID> { }

		[PXDBInt(BqlField = typeof(PMTimeActivity.shiftID))]
		[TimeActivityShiftCodeSelector(typeof(ownerID), typeof(date))]
		[EPShiftCodeActiveRestrictor]
		public virtual int? ShiftID { get; set; }
		#endregion

		#region EmployeeRate
		public abstract class employeeRate : PX.Data.BQL.BqlDecimal.Field<employeeRate> { }

		/// <summary>
		/// Stores Employee's Hourly rate at the time the activity was released to PM
		/// </summary>
		[IN.PXDBPriceCost(BqlField = typeof(PMTimeActivity.employeeRate))]
		[PXUIField(Visible = false)]
		public virtual decimal? EmployeeRate { get; set; }
		#endregion

		#region SummaryLineNbr
		public abstract class summaryLineNbr : PX.Data.BQL.BqlInt.Field<summaryLineNbr> { }

		/// <summary>
		/// This is a adjusting activity for the summary line in the Timecard.
		/// </summary>
		[PXDBInt(BqlField = typeof(PMTimeActivity.summaryLineNbr))]
		public virtual int? SummaryLineNbr { get; set; }
		#endregion

		#region ARDocType
		public abstract class arDocType : PX.Data.BQL.BqlString.Field<arDocType> { }
		[AR.ARDocType.List()]
		[PXString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual String ARDocType { get; set; }
		#endregion

		#region ARDocType
		public abstract class arRefNbr : PX.Data.BQL.BqlString.Field<arRefNbr> { }
		[PXString(15, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<AR.ARRegister.refNbr, Where<AR.ARRegister.docType, Equal<Current<arDocType>>>>), DescriptionField = typeof(AR.ARRegister.docType))]
		public virtual string ARRefNbr { get; set; }
		#endregion

		#region TimeActivityCreatedByID
		public abstract class timeActivityCreatedByID : PX.Data.BQL.BqlGuid.Field<timeActivityCreatedByID> { }

		[PXDBCreatedByID(DontOverrideValue = true, BqlField = typeof(PMTimeActivity.createdByID))]
		[PXUIField(Enabled = false)]
		public virtual Guid? TimeActivityCreatedByID { get; set; }
		#endregion

		#region TimeActivityCreatedByScreenID
		public abstract class timeActivityCreatedByScreenID : PX.Data.BQL.BqlString.Field<timeActivityCreatedByScreenID> { }

		[PXDBCreatedByScreenID(BqlField = typeof(PMTimeActivity.createdByScreenID))]
		public virtual string TimeActivityCreatedByScreenID { get; set; }
		#endregion

		#region TimeActivityCreatedDateTime
		public abstract class timeActivityCreatedDateTime : PX.Data.BQL.BqlDateTime.Field<timeActivityCreatedDateTime> { }

		[PXUIField(DisplayName = "Created At", Enabled = false)]
		[PXDBCreatedDateTime(BqlField = typeof(PMTimeActivity.createdDateTime))]
		public virtual DateTime? TimeActivityCreatedDateTime { get; set; }
		#endregion

		#region TimeActivityLastModifiedByID
		public abstract class timeActivityLastModifiedByID : PX.Data.BQL.BqlGuid.Field<timeActivityLastModifiedByID> { }

		[PXDBLastModifiedByID(BqlField = typeof(PMTimeActivity.lastModifiedByID))]
		public virtual Guid? TimeActivityLastModifiedByID { get; set; }
		#endregion

		#region TimeActivityLastModifiedByScreenID
		public abstract class timeActivityLastModifiedByScreenID : PX.Data.BQL.BqlString.Field<timeActivityLastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID(BqlField = typeof(PMTimeActivity.lastModifiedByScreenID))]
		public virtual string TimeActivityLastModifiedByScreenID { get; set; }
		#endregion

		#region TimeActivityLastModifiedDateTime
		public abstract class timeActivityLastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<timeActivityLastModifiedDateTime> { }

		[PXDBLastModifiedDateTime(BqlField = typeof(PMTimeActivity.lastModifiedDateTime))]
		public virtual DateTime? TimeActivityLastModifiedDateTime { get; set; }
		#endregion

		#endregion

		#region ChildKey
		public abstract class childKey : PX.Data.BQL.BqlGuid.Field<childKey> { }

		[PXGuid]
		public virtual Guid? ChildKey
		{
			[PXDependsOnFields(typeof(timeActivityNoteID))]
			get
			{
				return TimeActivityNoteID;
			}
		}
		#endregion
	}

	[Serializable]
	[PXBreakInheritance]
	[PXProjection(typeof(Select2<PMTimeActivity,
		LeftJoin<CRActivity,
			On<CRActivity.noteID, Equal<PMTimeActivity.refNoteID>>>,
		Where<PMTimeActivity.isCorrected, Equal<False>,
			Or<PMTimeActivity.isCorrected, IsNull>>>), Persistent = true)]
	public partial class PMCRActivity : CRPMTimeActivity
	{
		#region PMTimeActivity

		#region TimeActivityNoteID
		public new abstract class timeActivityNoteID : PX.Data.BQL.BqlGuid.Field<timeActivityNoteID> { }

		[PXDBGuid(true, IsKey = true, BqlField = typeof(PMTimeActivity.noteID))]
		public override Guid? TimeActivityNoteID { get; set; }
		#endregion

		#region TimeActivityRefNoteID
		public new abstract class timeActivityRefNoteID : PX.Data.BQL.BqlGuid.Field<timeActivityRefNoteID> { }

		[PXSequentialSelfRefNote(SuppressActivitiesCount = true, NoteField = typeof(timeActivityNoteID), BqlField = typeof(PMTimeActivity.refNoteID))]
		[PXParent(typeof(Select<CRActivity, Where<CRActivity.noteID, Equal<Current<refNoteID>>>>), ParentCreate = true)]
		public override Guid? TimeActivityRefNoteID { get; set; }
		#endregion

		#region Summary
		public abstract class summary : PX.Data.BQL.BqlString.Field<summary> { }

		[PXDBString(Common.Constants.TranDescLength, InputMask = "", IsUnicode = true, BqlField = typeof(PMTimeActivity.summary))]
		[PXDefault]
		[PXUIField(DisplayName = "Summary", Visibility = PXUIVisibility.SelectorVisible)]
		[PXNavigateSelector(typeof(summary))]
		public virtual string Summary { get; set; }
		#endregion

		#region Date
		public new abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }

		[PXDBDateAndTime(DisplayNameDate = "Date", DisplayNameTime = "Time", UseTimeZone = true, BqlField = typeof(PMTimeActivity.date))]
		[PXUIField(DisplayName = "Date")]
		[PXFormula(typeof(IsNull<Current<CRActivity.startDate>, Current<CRSMEmail.startDate>>))]
		public override DateTime? Date { get; set; }
		#endregion

		#region ApprovalStatus
		public new abstract class approvalStatus : PX.Data.BQL.BqlString.Field<approvalStatus> { }

		[PXDBString(2, IsFixed = true, BqlField = typeof(PMTimeActivity.approvalStatus))]
		[ActivityStatusList]
		[PXUIField(DisplayName = "Status", Enabled = false)]
		[PXFormula(typeof(Switch<
			Case<Where<trackTime, Equal<True>, And<Current2<approvalStatus>, IsNull>>, ActivityStatusAttribute.open,
			Case<Where<released, Equal<True>>, ActivityStatusAttribute.released,
			Case<Where<approverID, IsNotNull>, ActivityStatusAttribute.pendingApproval>>>,
			ActivityStatusAttribute.completed>))]
		public override string ApprovalStatus { get; set; }

		#endregion

		#endregion

		#region CRActivity

		#region NoteID
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXDBSequentialGuid(BqlField = typeof(CRActivity.noteID))]
		[PXTimeTag(typeof(noteID))]
		[PXExtraKey]
		public override Guid? NoteID { get; set; }
		#endregion

		#region ClassID
		public new abstract class classID : PX.Data.BQL.BqlInt.Field<classID> { }

		private int? _ClassID;
		[PXDBInt(BqlField = typeof(CRActivity.classID))]
		[PMActivityClass]
		[PXDefault(typeof(PMActivityClass.timeActivity))]
		[PXUIField(DisplayName = "Class", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public override int? ClassID {
			get
			{
				return _ClassID ?? PMActivityClass.TimeActivity;
				
			}
			set
			{
				_ClassID = value;
				
			}
		}
		#endregion

		#region ClassIcon
		public new abstract class classIcon : PX.Data.BQL.BqlString.Field<classIcon> { }
		#endregion

		#region ClassInfo
		[Obsolete("This field is not used anymore")]
		public new abstract class classInfo : PX.Data.BQL.BqlString.Field<classInfo>
		{
			public class emailResponse : PX.Data.BQL.BqlString.Constant<emailResponse>
			{
				public emailResponse() : base(Messages.EmailResponseClassInfo) { }
			}
		}

		[PXString]
		[PXUIField(DisplayName = "Type", Enabled = false)]
		[PXFormula(typeof(Switch<
			Case<Where<Current2<noteID>, IsNull>, PMActivityClass.UI.timeActivity,
			Case<Where<classID, Equal<CRActivityClass.activity>, And<type, IsNotNull>>, Selector<type, EPActivityType.description>,
			Case<Where<classID, Equal<CRActivityClass.email>, And<incoming, Equal<True>>>, classInfo.emailResponse>>>,
			String<classID>>))]
		[Obsolete("This field is not used anymore")]
		public override string ClassInfo { get; set; }
		#endregion

		#region Owner
		public new abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }

		[PXChildUpdatable(AutoRefresh = true)]
		[Owner(typeof(workgroupID), BqlField = typeof(CRActivity.ownerID))]
		[PXDefault(typeof(Coalesce<
			Search<EPCompanyTreeMember.contactID,
				Where<EPCompanyTreeMember.workGroupID, Equal<Current<workgroupID>>,
				And<EPCompanyTreeMember.contactID, Equal<Current<AccessInfo.contactID>>>>>,
			Search<Contact.contactID,
				Where<Contact.contactID, Equal<Current<AccessInfo.contactID>>,
				And<Current<workgroupID>, IsNull>>>>),
				PersistingCheck = PXPersistingCheck.Nothing)]
		public override int? OwnerID
		{
			get
			{
				return base.OwnerID ?? TimeActivityOwner;
			}
			set
			{
				if (NoteID != null)
				{
					base.OwnerID = TimeActivityOwner = value;
				}
			}
		}
		#endregion

		#region Subject
		public new abstract class subject : PX.Data.BQL.BqlString.Field<subject> { }

		[PXDBString(Common.Constants.TranDescLength, InputMask = "", IsUnicode = true, BqlField = typeof(CRActivity.subject))]
		[PXDefault]
		[PXFormula(typeof(summary))]
		[PXUIField(DisplayName = "Summary", Visibility = PXUIVisibility.SelectorVisible)]
		[PXNavigateSelector(typeof(subject))]
		public override string Subject { get; set; }
		#endregion

		#region StartDate
		public new abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		#endregion

		#endregion

		#region Overrides
		public new abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		public new abstract class isPrivate : PX.Data.BQL.BqlBool.Field<isPrivate> { }
		#endregion

		#region ChildKey
		public new abstract class childKey : PX.Data.BQL.BqlGuid.Field<childKey> { }

		[PXGuid]
		public override Guid? ChildKey
		{
			[PXDependsOnFields(typeof(noteID))]
			get
			{
				return NoteID;
			}
		}
		#endregion
	}

	[Serializable]
	[PXHidden]
	[PXBreakInheritance]
	public partial class CRChildActivity : CRPMTimeActivity
	{
		#region Overrides

		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		public new abstract class classID : PX.Data.BQL.BqlInt.Field<classID> { }
		public new abstract class uistatus : PX.Data.BQL.BqlString.Field<uistatus> { }
		public new abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		public new abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }
		public new abstract class isCorrected : PX.Data.BQL.BqlBool.Field<isCorrected> { }
		public new abstract class timeSpent : PX.Data.BQL.BqlInt.Field<timeSpent> { }
		public new abstract class overtimeSpent : PX.Data.BQL.BqlInt.Field<overtimeSpent> { }
		public new abstract class timeBillable : PX.Data.BQL.BqlInt.Field<timeBillable> { }
		public new abstract class overtimeBillable : PX.Data.BQL.BqlInt.Field<overtimeBillable> { }
		public new abstract class isPrivate : PX.Data.BQL.BqlBool.Field<isPrivate> { }

		#endregion

		#region ParentNoteID
		public new abstract class parentNoteID : PX.Data.BQL.BqlGuid.Field<parentNoteID> { }

		[PXUIField(DisplayName = "Parent")]
		[PXDBGuid]
		[PXSelector(typeof(Search<CRPMTimeActivity.noteID>), DirtyRead = true)]
		[PXRestrictor(typeof(Where<CRPMTimeActivity.classID, Equal<CRActivityClass.task>, Or<CRPMTimeActivity.classID, Equal<CRActivityClass.events>>>), null)]
		[PXDBDefault(typeof(CRPMTimeActivity.noteID), PersistingCheck = PXPersistingCheck.Nothing)]
		public override Guid? ParentNoteID { get; set; }
		#endregion

		#region UIStatus
		[PXDBString(2, IsFixed = true)]
		[ActivityStatus]
		[PXUIField(DisplayName = "Status")]
		[PXDefault(ActivityStatusAttribute.Open, PersistingCheck = PXPersistingCheck.Nothing)]
		public override string UIStatus { get; set; }
		#endregion
	}
}
