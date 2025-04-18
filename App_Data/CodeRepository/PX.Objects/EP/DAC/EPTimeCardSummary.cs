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
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;

namespace PX.Objects.EP
{
	/// <summary>
	/// Aggregates the information on the activities performed by an employee during a week according to the project, labor item, and day. The information will be displayed on the Employee Time Cards (EP406000) form.
	/// </summary>
	[PXCacheName(Messages.TimeCardSummary)]
	[Serializable]
	public partial class EPTimeCardSummary : IBqlTable
	{
		#region Keys

		/// <summary>
		/// Primary Key
		/// </summary>
		public class PK : PrimaryKeyOf<EPTimeCardSummary>.By<timeCardCD, lineNbr>
		{
			public static EPTimeCardSummary Find(PXGraph graph, string timeCardCD, int? lineNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, timeCardCD, lineNbr, options);
		}

		/// <summary>
		/// Foreign Keys
		/// </summary>
		public static class FK
		{
			/// <summary>
			/// Time Card
			/// </summary>
			public class Timecard : EPTimeCard.PK.ForeignKeyOf<EPTimeCardSummary>.By<timeCardCD> { }

			/// <summary>
			/// Earning Type
			/// </summary>
			public class EarningType : EPEarningType.PK.ForeignKeyOf<EPTimeCardSummary>.By<earningType> { }
						

			/// <summary>
			/// Project
			/// </summary>
			public class Project : PMProject.PK.ForeignKeyOf<EPTimeCardSummary>.By<projectID> { }

			/// <summary>
			/// Project Task
			/// </summary>
			public class ProjectTask : PMTask.PK.ForeignKeyOf<EPTimeCardSummary>.By<projectID, projectTaskID> { }

			/// <summary>
			/// Cost Code
			/// </summary>
			public class CostCode : PMCostCode.PK.ForeignKeyOf<EPTimeCardSummary>.By<costCodeID> { }
						

			/// <summary>
			/// Union
			/// </summary>
			public class Union : PMUnion.PK.ForeignKeyOf<EPTimeCardSummary>.By<unionID> { }

			/// <summary>
			/// Work Code
			/// </summary>
			public class WorkCode : PMWorkCode.PK.ForeignKeyOf<EPTimeCardSummary>.By<workCodeID> { }

			/// <summary>
			/// Labor Item
			/// </summary>
			public class LaborItem : InventoryItem.PK.ForeignKeyOf<EPTimeCardSummary>.By<labourItemID> { }

			/// <summary>
			/// Shift Code
			/// </summary>
			public class ShiftCode : EPShiftCode.PK.ForeignKeyOf<EPTimeCardSummary>.By<shiftID> { }
		}
		#endregion

		#region TimeCardCD

		public abstract class timeCardCD : PX.Data.BQL.BqlString.Field<timeCardCD> { }

        [PXDBDefault(typeof(EPTimeCard.timeCardCD))]
		[PXDBString(10, IsKey = true)]
		[PXUIField(Visible = false)]
        [PXParent(typeof(Select<EPTimeCard, Where<EPTimeCard.timeCardCD, Equal<Current<EPTimeCardSummary.timeCardCD>>>>))]
		public virtual string TimeCardCD { get; set; }

		#endregion

		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(EPTimeCard.summaryLineCntr))]
		[PXUIField(Visible = false)]
		public virtual int? LineNbr { get; set; }
		#endregion

		#region EarningType
		public abstract class earningType : PX.Data.BQL.BqlString.Field<earningType> { }

		[PXDBString(EPEarningType.typeCD.Length, IsUnicode = true, InputMask= EPEarningType.typeCD.InputMask)]
		[PXDefault(typeof(Search<EPSetup.regularHoursType>))]
		[PXRestrictor(typeof(Where<EPEarningType.isActive, Equal<True>>), Messages.EarningTypeInactive, typeof(EPEarningType.typeCD))]
		[PXSelector(typeof(EPEarningType.typeCD))]
		[PXUIField(DisplayName = "Earning Type")]
		public virtual string EarningType { get; set; }
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
		[TimeCardShiftCodeSelector(typeof(employeeID), typeof(EPTimeCard.weekEndDate))]
		[EPShiftCodeActiveRestrictor]
		public virtual int? ShiftID { get; set; }
		#endregion

		#region ParentNoteID
		public abstract class parentNoteID : PX.Data.BQL.BqlGuid.Field<parentNoteID> { }
		
		[PXUIField(DisplayName = "Task ID")]
		[PXDBGuid]
        [CRTaskSelector]
		[PXForeignReference(typeof(Field<parentNoteID>.IsRelatedTo<CRActivity.noteID>))]
        [PXRestrictor(typeof(Where<CRActivity.ownerID, Equal<Current<AccessInfo.contactID>>>), null)]
        public virtual Guid? ParentNoteID { get; set; }
		#endregion

		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[ProjectDefault(BatchModule.TA, ForceProjectExplicitly = true)]
		[EPTimeCardProject]
		public virtual int? ProjectID { get; set; }
		#endregion

		#region ProjectTaskID
		public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
		[PXDefault(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<projectID>>, And<PMTask.isDefault, Equal<True>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[EPTimecardProjectTask(typeof(projectID), BatchModule.TA, DisplayName = "Project Task")]
		[PXForeignReference(typeof(CompositeKey<Field<projectID>.IsRelatedTo<PMTask.projectID>, Field<projectTaskID>.IsRelatedTo<PMTask.taskID>>))]
		public virtual int? ProjectTaskID { get; set; }
		#endregion

		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		[CostCode(null, typeof(projectTaskID), GL.AccountType.Expense)]
		public virtual Int32? CostCodeID
		{
			get;
			set;
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
		[PMUnion(typeof(projectID), typeof(Select<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<EPTimeCard.employeeID>>>>))]
		public virtual String UnionID
		{
			get;
			set;
		}
		#endregion
		#region LabourItemID
		public abstract class labourItemID : PX.Data.BQL.BqlInt.Field<labourItemID> { }

		[PMLaborItem(typeof(projectID), typeof(earningType), typeof(Select<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<EPTimeCard.employeeID>>>>))]
		[PXForeignReference(typeof(Field<labourItemID>.IsRelatedTo<InventoryItem.inventoryID>))]
		public virtual int? LabourItemID { get; set; }
		#endregion
		#region WorkCodeID
		public abstract class workCodeID : PX.Data.BQL.BqlString.Field<workCodeID> { }
		[PXForeignReference(typeof(Field<workCodeID>.IsRelatedTo<PMWorkCode.workCodeID>))]
		[PMWorkCode(typeof(costCodeID), typeof(projectID), typeof(projectTaskID), typeof(labourItemID), typeof(employeeID))]
		public virtual String WorkCodeID
		{
			get; set;
		}
		#endregion

		
		#region TimeSpent
		public abstract class timeSpent : PX.Data.BQL.BqlInt.Field<timeSpent> { }

		[PXInt]
		[PXTimeList]
		[PXDependsOnFields(typeof(mon), typeof(tue), typeof(wed), typeof(thu), typeof(fri), typeof(sat), typeof(sun))]
		[PXUIField(DisplayName = "Time Spent", Enabled = false)]
		public virtual int? TimeSpent
		{
			get
			{
				return Mon.GetValueOrDefault() +
					   Tue.GetValueOrDefault() +
					   Wed.GetValueOrDefault() +
					   Thu.GetValueOrDefault() +
					   Fri.GetValueOrDefault() +
					   Sat.GetValueOrDefault() +
					   Sun.GetValueOrDefault();
			}
		}
		#endregion

		#region Sun
		public abstract class sun : PX.Data.BQL.BqlInt.Field<sun> { }

        [PXTimeList]
        [PXDBInt]
		[PXUIField(DisplayName = "Sun")]
		public virtual int? Sun { get; set; }
		#endregion
		#region Mon
		public abstract class mon : PX.Data.BQL.BqlInt.Field<mon> { }
		
        [PXTimeList]
        [PXDBInt]
		[PXUIField(DisplayName = "Mon")]
		public virtual int? Mon { get; set; }
		#endregion
		#region Tue
		public abstract class tue : PX.Data.BQL.BqlInt.Field<tue> { }

        [PXTimeList]
        [PXDBInt]
		[PXUIField(DisplayName = "Tue")]
		public virtual int? Tue { get; set; }
		#endregion
		#region Wed
		public abstract class wed : PX.Data.BQL.BqlInt.Field<wed> { }

        [PXTimeList]
        [PXDBInt]
		[PXUIField(DisplayName = "Wed")]
		public virtual int? Wed { get; set; }
		#endregion
		#region Thu
		public abstract class thu : PX.Data.BQL.BqlInt.Field<thu> { }

        [PXTimeList]
        [PXDBInt]
		[PXUIField(DisplayName = "Thu")]
		public virtual int? Thu { get; set; }
		#endregion
		#region Fri
		public abstract class fri : PX.Data.BQL.BqlInt.Field<fri> { }

        [PXTimeList]
        [PXDBInt]
		[PXUIField(DisplayName = "Fri")]
		public virtual int? Fri { get; set; }
		#endregion
		#region Sat
		public abstract class sat : PX.Data.BQL.BqlInt.Field<sat> { }

        [PXTimeList]
        [PXDBInt]
		[PXUIField(DisplayName = "Sat")]
		public virtual int? Sat { get; set; }
		#endregion
		
		#region IsBillable
		public abstract class isBillable : PX.Data.BQL.BqlBool.Field<isBillable> { }
		
		[PXDBBool()]
		[PXDefault()]
		[PXUIField(DisplayName = "Billable")]
		public virtual bool? IsBillable { get; set; }
		#endregion

		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual string Description { get; set; }
		#endregion

		#region EmployeeID
		[PXInt]
		[PXUnboundDefault(typeof(Parent<EPTimeCard.employeeID>))]
		public int? EmployeeID { get; set; }
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		#endregion

		#region System Columns
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		
		[PXNote()]
		public virtual Guid? NoteID { get; set; }
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		
		[PXDBTimestamp()]
		public virtual byte[] tstamp { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		
		[PXDBCreatedByScreenID()]
		public virtual string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		
		[PXDBLastModifiedByScreenID()]
		public virtual string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion

		#region EmployeeRate
		public abstract class employeeRate : PX.Data.BQL.BqlDecimal.Field<employeeRate> { }

		/// <summary>
		/// Stores Employee's Hourly rate at the time the activity was released to PM
		/// </summary>
		[IN.PXPriceCost]
		[PXUIField(DisplayName = "Cost Rate", Enabled = false)]
		public virtual decimal? EmployeeRate { get; set; }
		#endregion

		public int? GetTimeTotal(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Monday: return Mon;
                case DayOfWeek.Tuesday: return Tue;
                case DayOfWeek.Wednesday: return Wed;
                case DayOfWeek.Thursday: return Thu;
                case DayOfWeek.Friday: return Fri;
                case DayOfWeek.Saturday: return Sat;
                case DayOfWeek.Sunday: return Sun;

                default:
                    return null;
            }
        }

        public int? GetTimeTotal()
        {
            return Mon.GetValueOrDefault() + Tue.GetValueOrDefault() + Wed.GetValueOrDefault() + Thu.GetValueOrDefault() +
                   Fri.GetValueOrDefault() + Sat.GetValueOrDefault() + Sun.GetValueOrDefault();

        }

		public void SetDayTime(DayOfWeek day, int minutes)
		{
			switch (day)
			{
				case DayOfWeek.Monday: Mon = minutes; break;
				case DayOfWeek.Tuesday: Tue = minutes; break;
				case DayOfWeek.Wednesday: Wed = minutes; break;
				case DayOfWeek.Thursday: Thu = minutes; break;
				case DayOfWeek.Friday: Fri = minutes; break;
				case DayOfWeek.Saturday: Sat = minutes; break;
				case DayOfWeek.Sunday: Sun = minutes; break;
			}
		}

        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3} {4} {5} {6} {7}", EarningType, Mon, Tue, Wed, Thu, Fri, Sat, Sun);
        }

	}
}
