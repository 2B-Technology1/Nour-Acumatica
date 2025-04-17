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
using System.Collections;
using PX.Objects.CR;
using PX.Objects.PM;
using PX.Data;
using PX.TM;
using System.Linq;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data.EP;

namespace PX.Objects.EP
{
	public class EmployeeSummaryApprove : PXGraph<EmployeeSummaryApprove>
	{
		#region Selects

		public PXFilter<EPSummaryFilter> Filter;
		public PXSelectJoin<
			EPSummaryApprove,
			LeftJoin<EPEarningType, 
				On<EPEarningType.typeCD, Equal<EPSummaryApprove.earningType>>>,
			Where2<Where<EPSummaryApprove.taskApproverID, Equal<Current<EPSummaryFilter.approverID>>, 
					Or<Where<EPSummaryApprove.taskApproverID, IsNull, 
						And<EPSummaryApprove.approverID, Equal<Current<EPSummaryFilter.approverID>>>>>>,
				And2<Where<EPSummaryApprove.weekId, GreaterEqual<Current<EPSummaryFilter.fromWeek>>, 
					Or<Current<EPSummaryFilter.fromWeek>, IsNull>>,
				And2<Where<EPSummaryApprove.weekId, LessEqual<Current<EPSummaryFilter.tillWeek>>, 
					Or<Current<EPSummaryFilter.tillWeek>, IsNull>>,
				And2<Where<EPSummaryApprove.projectID, Equal<Current<EPSummaryFilter.projectID>>, 
					Or<Current<EPSummaryFilter.projectID>, IsNull>>,
				And2<Where<EPSummaryApprove.projectTaskID, Equal<Current<EPSummaryFilter.projectTaskID>>,
					Or<Current<EPSummaryFilter.projectTaskID>, IsNull>>,
				And<Where<EPSummaryApprove.employeeID, Equal<Current<EPSummaryFilter.employeeID>>, 
					Or<Current<EPSummaryFilter.employeeID>, IsNull>>>>>>>>> 
			Summary;

		public PXSelect<PMTimeActivity> Activity;

		public PXSetup<EPSetup> Setup;

		#endregion

		#region Actions

		public PXSave<EPSummaryFilter> Save;
		public PXCancel<EPSummaryFilter> Cancel;
		public PXAction<EPSummaryFilter> viewDetails;
		public PXAction<EPSummaryFilter> approveAll;
		public PXAction<EPSummaryFilter> rejectAll;

		[PXUIField(DisplayName = "")]
		[PXEditDetailButton()]
		public virtual IEnumerable ViewDetails(PXAdapter adapter)
		{
			var row = Summary.Current;
			if (row != null)
			{
				PXRedirectHelper.TryRedirect(this, PXSelectorAttribute.Select<EPSummaryApprove.timeCardCD>(Summary.Cache, row), PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.ApproveAll)]
		[PXButton()]
		public virtual IEnumerable ApproveAll(PXAdapter adapter)
		{
			if (Summary.Current == null || Filter.View.Ask(Messages.ApproveAllConfirmation, MessageButtons.YesNo) != WebDialogResult.Yes)
			{
				return adapter.Get();
			}

			foreach (EPSummaryApprove item in Summary.Select())
			{
				item.IsApprove = true;
				item.IsReject = false;
				Summary.Cache.Update(item);
			}
			Persist();
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.RejectAll)]
		[PXButton()]
		public virtual IEnumerable RejectAll(PXAdapter adapter)
		{
			if (Summary.Current == null || Filter.View.Ask(Messages.RejectAllConfirmation, MessageButtons.YesNo) != WebDialogResult.Yes)
			{
				return adapter.Get();
			}

			foreach (EPSummaryApprove item in Summary.Select())
			{
				item.IsApprove = false;
				item.IsReject = true;
				Summary.Cache.Update(item);
			}
			Persist();
			return adapter.Get();
		}

		#endregion

		#region Event handlers

		protected virtual void EPSummaryFilter_FromWeek_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			EPSummaryFilter row = (EPSummaryFilter)e.Row;
			if (row != null && e.ExternalCall && row.FromWeek > row.TillWeek)
				row.TillWeek = row.FromWeek;
		}

		protected virtual void EPSummaryFilter_TillWeek_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			EPSummaryFilter row = (EPSummaryFilter)e.Row;
			if (row != null && e.ExternalCall && row.FromWeek != null && row.FromWeek > row.TillWeek)
				row.FromWeek = row.TillWeek;
		}

		protected virtual void EPSummaryFilter_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			if (Summary.Cache.IsDirty && Filter.View.Ask(ActionsMessages.ConfirmationMsg, MessageButtons.YesNo) != WebDialogResult.Yes)
				e.Cancel = true;
			else
				Summary.Cache.Clear();
		}

		protected virtual void EPSummaryApprove_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			EPSummaryApprove row = (EPSummaryApprove)e.Row;
			if (row == null)
				return;

			PXUIFieldAttribute.SetEnabled(sender, row, false);
			if (row.HasComplite != null || row.HasReject != null || row.HasApprove != null)
			{
				PXUIFieldAttribute.SetEnabled<EPSummaryApprove.isApprove>(sender, row, true);
				PXUIFieldAttribute.SetEnabled<EPSummaryApprove.isReject>(sender, row, true);
			}
			if (row.HasOpen != null)
				sender.RaiseExceptionHandling<EPSummaryApprove.weekId>(row, null, new PXSetPropertyException(EP.Messages.HasOpenActivity, PXErrorLevel.RowWarning));
		}

		protected virtual void EPSummaryFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			EPSummaryFilter row = (EPSummaryFilter)e.Row;
			if (row != null)
			{
				row.RegularTime = 0;
				row.RegularOvertime = 0;
				row.RegularTotal = 0;
				foreach (PXResult<EPSummaryApprove, EPEarningType> item in Summary.Select())
				{
					EPSummaryApprove rowActivity = (EPSummaryApprove)item;
					EPEarningType rowEarningType = (EPEarningType)item;

					if (rowEarningType.IsOvertime == true)
					{
						row.RegularOvertime += rowActivity.TimeSpent.GetValueOrDefault(0);
					}
					else
					{
						row.RegularTime += rowActivity.TimeSpent.GetValueOrDefault(0);
					}

					row.RegularTotal = row.RegularTime + row.RegularOvertime;
				}
			}
		}

		protected virtual void EPSummaryApprove_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			EPSummaryApprove row = (EPSummaryApprove)e.Row;
			if (row == null || e.Operation == PXDBOperation.Delete)
				return;
			if (row.IsApprove == true && row.IsReject == true)
				return;

			PXResultset<PMTimeActivity> activityList = PXSelectJoin<
				PMTimeActivity,
				InnerJoin<EPTimeCard, 
					On<Required<EPTimeCardSummary.timeCardCD>, Equal<EPTimeCard.timeCardCD>, 
					And<EPTimeCard.weekId, Equal<PMTimeActivity.weekID>>>,
				InnerJoin<CREmployee, 
					On<EPTimeCard.employeeID, Equal<CREmployee.bAccountID>, 
					And<CREmployee.defContactID, Equal<PMTimeActivity.ownerID>>>>>,
				Where<
					Required<EPTimeCardSummary.earningType>, Equal<PMTimeActivity.earningTypeID>,
					And<Required<EPTimeCardSummary.projectID>, Equal<PMTimeActivity.projectID>,
					And<Required<EPTimeCardSummary.projectTaskID>, Equal<PMTimeActivity.projectTaskID>,
					And<Required<EPTimeCardSummary.isBillable>, Equal<PMTimeActivity.isBillable>,
					And2<Where<Required<EPTimeCardSummary.parentNoteID>, Equal<PMTimeActivity.parentTaskNoteID>,
						Or<Required<EPTimeCardSummary.parentNoteID>, IsNull,
						And<PMTimeActivity.parentTaskNoteID, IsNull>>>,
					And<PMTimeActivity.trackTime, Equal<True>,
					And<PMTimeActivity.released, Equal<False>,
					And<PMTimeActivity.trackTime, Equal<True>,
					And<PMTimeActivity.approverID, IsNotNull,
					And<Where<PMTimeActivity.approvalStatus, NotEqual<ActivityStatusListAttribute.canceled>,
					And<PMTimeActivity.approvalStatus, NotEqual <ActivityStatusListAttribute.open>>>>>>>>>>>>>>
			.Select(this, row.TimeCardCD, row.EarningType, row.ProjectID, row.ProjectTaskID, row.IsBillable, row.ParentNoteID, row.ParentNoteID);

			foreach(PMTimeActivity act in activityList)
			{

				if (row.IsApprove == true)
				{
					if (act.ApprovalStatus != ActivityStatusListAttribute.Approved)
					{
						Activity.Cache.SetValueExt<PMTimeActivity.approvalStatus>(act, ActivityStatusListAttribute.Approved);
						Activity.Cache.SetValueExt<PMTimeActivity.approvedDate>(act, Accessinfo.BusinessDate);
					}
				}
				else if (row.IsReject == true)
				{
					if(act.ApprovalStatus != ActivityStatusListAttribute.Rejected)
						Activity.Cache.SetValueExt<PMTimeActivity.approvalStatus>(act, ActivityStatusListAttribute.Rejected);
				}
				Activity.Cache.Persist(act, PXDBOperation.Update);
			}
		}

		protected virtual void PMTimeActivity_UIStatus_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Week")]
		protected virtual void _(Events.CacheAttached<EPWeekRaw.shortDescription> e) { }

		#endregion

		public EmployeeSummaryApprove()
		{
			if (Setup.Current == null)
				throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(EPSetup), PXMessages.LocalizeNoPrefix(Messages.EPSetup));
		}

		public override void Persist()
		{
			var groups = Summary.Cache.Updated.Cast<EPSummaryApprove>().Where(a => a.IsReject == true).GroupBy(a => a.TimeCardCD).ToList();

			using (var ts = new PXTransactionScope())
			{
				if (groups.Count > 0)
				{
					TimeCardMaint maint = PXGraph.CreateInstance<TimeCardMaint>();
					maint.Document.Join<InnerJoin<EPTimeCardSummary, On<EPTimeCardSummary.timeCardCD, Equal<EPTimeCard.timeCardCD>>>>();
					maint.Document.Join<InnerJoin<PMTask, On<PMTask.taskID, Equal<EPTimeCardSummary.projectTaskID>>>>();
					maint.Document.WhereOr<Where<PMTask.approverID, Equal<Current<EPSummaryFilter.approverID>>>>();

					foreach (var group in groups)
					{
						maint.Clear();
						maint.Document.Current = maint.Document.Search<EPTimeCard.timeCardCD>(group.Key);
						maint.Actions["Reject"].Press();
						maint.Persist();
					}
				}
				base.Persist();
				ts.Complete();
			}
		}
	}

	#region EPSummaryFilter
	[Serializable]
	[PXHidden]
	public class EPSummaryFilter : IBqlTable
	{
		#region ApproverID
		public abstract class approverID : PX.Data.BQL.BqlInt.Field<approverID> { }

		protected int? _ApproverID;
		[PXDBInt]
		[PXSubordinateSelector]
		[PXDefault(typeof(Search<EPEmployee.bAccountID, Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>))]
		[PXUIField(DisplayName = "Approver", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? ApproverID { get; set; }
		#endregion

		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }

		[PXDBInt]
		[PXSubordinateSelector]
		[PXUIField(DisplayName = "Employee")]
		public virtual int? EmployeeID { set; get; }
		#endregion

		#region FromWeek
		public abstract class fromWeek : PX.Data.BQL.BqlInt.Field<fromWeek> { }

		[PXDBInt]
		[PXWeekSelector2(DescriptionField = typeof(EPWeekRaw.shortDescription))]
		[PXUIField(DisplayName = "From Week", Visibility = PXUIVisibility.Visible)]
		public virtual int? FromWeek { set; get; }
		#endregion

		#region TillWeek
		public abstract class tillWeek : PX.Data.BQL.BqlInt.Field<tillWeek> { }

		[PXDBInt]
		[PXWeekSelector2(DescriptionField = typeof(EPWeekRaw.shortDescription))]
		[PXUIField(DisplayName = "Until Week", Visibility = PXUIVisibility.Visible)]
		public virtual int? TillWeek { set; get; }
		#endregion

		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

		[PXRestrictor(typeof(Where<PMProject.isActive, Equal<True>>), PM.Messages.InactiveContract, typeof(PMProject.contractCD))]
		[ProjectBaseAttribute(DisplayName = PM.Messages.Project)]
		public virtual int? ProjectID { set; get; }
		#endregion

		#region ProjectTaskID
		public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }

		[ProjectTask(typeof(EPSummaryFilter.projectID))]
		public virtual int? ProjectTaskID { set; get; }
		#endregion

		#region Total
		#region Regular
		public abstract class regularTime : PX.Data.BQL.BqlInt.Field<regularTime> { }

		[PXInt]
		[PXUIField(DisplayName = "Time Spent", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual int? RegularTime { set; get; }
		#endregion
		#region RegularOvertime
		public abstract class regularOvertime : PX.Data.BQL.BqlInt.Field<regularOvertime> { }

		[PXInt]
		[PXUIField(DisplayName = "Regular Overtime", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual int? RegularOvertime { set; get; }
		#endregion
		#region RegularTotal
		public abstract class regularTotal : PX.Data.BQL.BqlInt.Field<regularTotal> { }

		[PXInt]
		[PXUIField(DisplayName = "Regular Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual int? RegularTotal { set; get; }
		#endregion

		#endregion

	}
	#endregion

	[PXHidden]
	[Serializable]
	public partial class EPActivityReject : PMTimeActivity
	{
		public new abstract class approvalStatus : PX.Data.BQL.BqlString.Field<approvalStatus> { }
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
	}

	[PXHidden]
	[Serializable]
	public partial class EPActivityComplite : PMTimeActivity
	{
		public new abstract class approvalStatus : PX.Data.BQL.BqlString.Field<approvalStatus> { }
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
	}

	[PXHidden]
	[Serializable]
	public partial class EPActivityOpen : PMTimeActivity
	{
		public new abstract class approvalStatus : PX.Data.BQL.BqlString.Field<approvalStatus> { }
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
	}

	#region EPSummaryApprove

	/// <summary>
	/// Provides a mechanism to approve or reject time activities in groups by approving or rejecting summary rows that represent the related time cards.
	/// Only available if the Project Accounting feature is enabled.
	/// </summary>
	[Serializable]
	[PXProjection(typeof(
		Select5<
			EPTimeCardSummary,
		InnerJoin<EPTimeCard, 
			 On<EPTimeCardSummary.timeCardCD, Equal<EPTimeCard.timeCardCD>>,
		LeftJoin<EPTimeCardEx, 
			 On<EPTimeCard.timeCardCD, Equal<EPTimeCardEx.origTimeCardCD>>,
		InnerJoin<CREmployee, 
			 On<EPTimeCard.employeeID, Equal<CREmployee.bAccountID>>,
		InnerJoin<PMTimeActivity,
			On<EPTimeCardSummary.earningType, Equal<PMTimeActivity.earningTypeID>,
			And<EPTimeCardSummary.projectID, Equal<PMTimeActivity.projectID>,
			And<EPTimeCardSummary.projectTaskID, Equal<PMTimeActivity.projectTaskID>,
			And<EPTimeCardSummary.isBillable, Equal<PMTimeActivity.isBillable>,
			And2<Where<EPTimeCardSummary.parentNoteID, Equal<PMTimeActivity.parentTaskNoteID>, 
				Or<EPTimeCardSummary.parentNoteID, IsNull, 
				And<PMTimeActivity.parentTaskNoteID, IsNull>>>,
			And2<Where<EPTimeCardSummary.jobID, Equal<PMTimeActivity.jobID>, 
				Or<EPTimeCardSummary.jobID, IsNull, 
				And<PMTimeActivity.jobID, IsNull>>>,
			And2<Where<EPTimeCardSummary.shiftID, Equal<PMTimeActivity.shiftID>, 
				Or<EPTimeCardSummary.shiftID, IsNull, 
				And<PMTimeActivity.shiftID, IsNull>>>, 
			And<CREmployee.defContactID, Equal<PMTimeActivity.ownerID>, 
			And<EPTimeCard.weekId, Equal<PMTimeActivity.weekID>, 
			And<PMTimeActivity.released, Equal<False>, 
			And<PMTimeActivity.trackTime, Equal<True>, 
			And<PMTimeActivity.approverID, IsNotNull, 
			And<Where<PMTimeActivity.approvalStatus, NotEqual<ActivityStatusListAttribute.canceled>>>>>>>>>>>>>>>,
		LeftJoin<EPActivityApprove, 
			On<EPActivityApprove.noteID, Equal<PMTimeActivity.noteID>, 
			And<EPActivityApprove.approvalStatus, Equal<ActivityStatusListAttribute.approved>>>,
		LeftJoin<EPActivityReject, 
			On<EPActivityReject.noteID, Equal<PMTimeActivity.noteID>, 
			And<EPActivityReject.approvalStatus, Equal<ActivityStatusListAttribute.rejected>>>,
		LeftJoin<EPActivityComplite, 
			On<EPActivityComplite.noteID, Equal<PMTimeActivity.noteID>, 
			And<EPActivityComplite.approvalStatus, Equal<ActivityStatusListAttribute.pendingApproval>>>,
		LeftJoin<EPActivityOpen, 
			On<EPActivityOpen.noteID, Equal<PMTimeActivity.noteID>, 
			And<EPActivityOpen.approvalStatus, Equal<ActivityStatusListAttribute.open>>>,
		LeftJoin<PMProject, 
			On<PMProject.contractID, Equal<EPTimeCardSummary.projectID>>, 
		LeftJoin<PMTask, 
			On<PMTask.projectID, Equal<EPTimeCardSummary.projectID>, And<PMTask.taskID, Equal<EPTimeCardSummary.projectTaskID>>>>>>>>>>>>>,
		Where<EPTimeCardEx.timeCardCD, IsNull>,
		Aggregate<
			GroupBy<EPTimeCardSummary.timeCardCD,
			GroupBy<EPTimeCardSummary.lineNbr,
			GroupBy<EPTimeCardSummary.earningType,
			GroupBy<EPTimeCardSummary.jobID,
			GroupBy<EPTimeCardSummary.shiftID,
			GroupBy<EPTimeCardSummary.parentNoteID,
			GroupBy<EPTimeCardSummary.projectID,
			GroupBy<EPTimeCardSummary.projectTaskID,
			GroupBy<EPTimeCardSummary.mon,
			GroupBy<EPTimeCardSummary.tue,
			GroupBy<EPTimeCardSummary.wed,
			GroupBy<EPTimeCardSummary.thu,
			GroupBy<EPTimeCardSummary.fri,
			GroupBy<EPTimeCardSummary.sat,
			GroupBy<EPTimeCardSummary.sun,
			GroupBy<EPTimeCardSummary.isBillable,
			GroupBy<EPTimeCardSummary.description,
			GroupBy<EPTimeCardSummary.noteID,
			GroupBy<EPTimeCard.weekId,
			GroupBy<PMProject.approverID,
			GroupBy<PMTask.approverID,
			GroupBy<CREmployee.userID,
			GroupBy<CREmployee.noteID,
			GroupBy<CREmployee.bAccountID,
			Max <EPActivityComplite.approvalStatus,
			Max<EPActivityApprove.approvalStatus,
			Max<EPActivityReject.approvalStatus,
			Max<EPActivityOpen.approvalStatus>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
		), Persistent = false)]
	[PXHidden]
	public partial class EPSummaryApprove : IBqlTable
	{
		#region Keys
		/// <summary>
		/// Primary Key
		/// </summary>
		public class PK : PrimaryKeyOf<EPSummaryApprove>.By<timeCardCD, lineNbr>
		{
			public static EPSummaryApprove Find(PXGraph graph, string timeCardCD, int? lineNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, timeCardCD, lineNbr, options);
		}

		/// <summary>
		/// Foreign Keys
		/// </summary>
		public static class FK
		{
			/// <summary>
			/// Time Card
			/// </summary>
			public class Timecard : EPTimeCard.PK.ForeignKeyOf<EPSummaryApprove>.By<timeCardCD> { }

			/// <summary>
			/// Earning Type
			/// </summary>
			public class EarningType : EPEarningType.PK.ForeignKeyOf<EPSummaryApprove>.By<earningType> { }

			/// <summary>
			/// Project
			/// </summary>
			public class Project : PMProject.PK.ForeignKeyOf<EPSummaryApprove>.By<projectID> { }

			/// <summary>
			/// Project Task
			/// </summary>
			public class ProjectTask : PMTask.PK.ForeignKeyOf<EPSummaryApprove>.By<projectID, projectTaskID> { }

			/// <summary>
			/// Shift Code
			/// </summary>
			public class ShiftCode : EPShiftCode.PK.ForeignKeyOf<EPSummaryApprove>.By<shiftID> { }
		}
		#endregion

		#region Approve
		public abstract class isApprove : PX.Data.BQL.BqlBool.Field<isApprove> { }

		protected bool? _IsApprove;

		/// <summary>
		/// Indicates (if set to <see langword="true"/>) that this summary row is approved.
		/// </summary>
		[PXBool]
		[PXUIField(DisplayName = "Approve")]
		public virtual bool? IsApprove
		{
			get
			{
				return _IsApprove ?? HasApprove != null && HasComplite == null && HasOpen == null && HasReject == null;
			}
			set
			{
				_IsApprove = value;

				if (_IsApprove.HasValue && _IsApprove.Value == true)
                {
					_IsReject = false;
                }
			}
		}
		#endregion

		#region Reject
		public abstract class isReject : PX.Data.BQL.BqlBool.Field<isReject> { }

		/// <summary>
		/// Indicates (if set to <see langword="true"/>) that this summary row is rejected.
		/// </summary>
		protected bool? _IsReject;
		[PXBool]
		[PXUIField(DisplayName = "Reject")]
		public virtual bool? IsReject
		{
			get
			{
				return _IsReject ?? HasApprove == null && HasComplite == null && HasOpen == null && HasReject != null;
			}
			set
			{
				_IsReject = value;
				if (_IsReject.HasValue && _IsReject.Value == true)
				{
					_IsApprove = false;
				}
			}
		}
		#endregion

		#region TimeCardCD

		public abstract class timeCardCD : PX.Data.BQL.BqlString.Field<timeCardCD> { }

		[PXDBDefault(typeof(EPTimeCard.timeCardCD))]
		[PXDBString(10, IsKey = true, BqlField = typeof(EPTimeCardSummary.timeCardCD))]
		[PXUIField(DisplayName = "Time Card")]
		[PXSelector(typeof(Search<EPTimeCard.timeCardCD>))]
		public virtual string TimeCardCD { get; set; }

		#endregion

		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

		/// <summary>
		/// Represents the position of this item within this group of time card activities.
		/// </summary>
		[PXDBInt(IsKey = true, BqlField = typeof(EPTimeCardSummary.lineNbr))]
		[PXLineNbr(typeof(EPTimeCard.summaryLineCntr))]
		[PXUIField(Visible = false)]
		public virtual int? LineNbr { get; set; }
		#endregion

		#region EarningType
		public abstract class earningType : PX.Data.BQL.BqlString.Field<earningType> { }

		/// <summary>
		/// The <see cref="EPEarningType">earning type</see> of the work time spent by the employee.<br></br>
		/// Only active earning types can be selected.
		/// </summary>
		[PXDBString(EPEarningType.typeCD.Length, IsUnicode = true, InputMask = EPEarningType.typeCD.InputMask, BqlField = typeof(EPTimeCardSummary.earningType))]
		[PXDefault(typeof(Search<EPSetup.regularHoursType>))]
		[PXRestrictor(typeof(Where<EPEarningType.isActive, Equal<True>>), Messages.EarningTypeInactive, typeof(EPEarningType.typeCD))]
		[PXSelector(typeof(EPEarningType.typeCD))]
		[PXUIField(DisplayName = "Earning Type")]
		public virtual string EarningType { get; set; }
		#endregion

		#region JobID
		public abstract class jobID : PX.Data.BQL.BqlInt.Field<jobID> { }

		/// <summary>
		/// The identifier of the <see cref="PMTimeActivity">job</see> associated with this activity.
		/// <para></para>
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PMTimeActivity.JobID"/> field.
		/// </value>
		[PXDBInt(BqlField = typeof(EPTimeCardSummary.jobID))]
		public virtual int? JobID { get; set; }
		#endregion

		#region ShiftID
		public abstract class shiftID : PX.Data.BQL.BqlInt.Field<shiftID> { }

		/// <summary>
		/// The <see cref="PX.Objects.EP.EPShiftCode">shift code </see> for this time activity.<br></br>
		/// Only available if the Shift Differential feature is enabled.
		/// </summary>
		[PXDBInt(BqlField = typeof(EPTimeCardSummary.shiftID))]
		[PXUIField(DisplayName = "Shift Code", FieldClass = nameof(FeaturesSet.ShiftDifferential))]
		[TimeCardShiftCodeSelector(typeof(employeeID), typeof(EPTimeCard.weekEndDate))]
		[EPShiftCodeActiveRestrictor]
		public virtual int? ShiftID { get; set; }
		#endregion

		#region ParentNoteID
		public abstract class parentNoteID : PX.Data.BQL.BqlGuid.Field<parentNoteID> { }
		
		[PXUIField(DisplayName = "Task ID")]
		[PXDBGuid(BqlField = typeof(EPTimeCardSummary.parentNoteID))]
		[CRTaskSelector]
		public virtual Guid? ParentNoteID { get; set; }
		#endregion

		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

		/// <summary>
		/// The identifier of the <see cref="PMProject">project</see> that the employee has worked on.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PMProject.ContractID"/> field.
		/// </value>
		[ProjectDefault(GL.BatchModule.TA, ForceProjectExplicitly = true)]
		[EPTimeCardProjectAttribute(BqlField = typeof(EPTimeCardSummary.projectID))]
		public virtual int? ProjectID { get; set; }
		#endregion

		#region ProjectDescription
		public abstract class projectDescription : PX.Data.BQL.BqlString.Field<projectDescription> { }
		/// <summary>
		/// Gets sets Project's Description
		/// </summary>
		[PXUIField(DisplayName = "Project Description", IsReadOnly = true, Visible = false)]
		[PXString]
		[PXFieldDescription]
		[PXUnboundDefault(typeof(Search<
			PMProject.description,
			Where<PMProject.contractID, Equal<Current<EPSummaryApprove.projectID>>>>))]
		[PXFormula(typeof(Default<projectID>))]
		public string ProjectDescription { get; set; }
		#endregion

		#region ProjectTaskID
		public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }

		/// <summary>
		/// The identifier of the <see cref="PMTask">project task</see> that the employee has worked on.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PMTask.TaskID"/> field.
		/// </value>
		[EPTimecardProjectTask(typeof(EPTimeCardSummary.projectID), GL.BatchModule.TA, DisplayName = "Project Task", BqlField = typeof(EPTimeCardSummary.projectTaskID))]
		public virtual int? ProjectTaskID { get; set; }
		#endregion

		#region ProjectTaskDescription
		public abstract class projectTaskDescription : PX.Data.BQL.BqlString.Field<projectTaskDescription> { }
		/// <summary>
		/// Gets sets Project Task's Description
		/// </summary>
		[PXUIField(DisplayName = "Project Task Description", IsReadOnly = true, Visible = false)]
		[PXString]
		[PXFieldDescription]
		[PXUnboundDefault(typeof(Search<
			PMTask.description,
			Where<PMTask.projectID, Equal<Current<EPSummaryApprove.projectID>>, And<PMTask.taskID, Equal<Current<EPSummaryApprove.projectTaskID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<projectID, projectTaskID>))]
		public string ProjectTaskDescription { get; set; }
		#endregion

		#region TimeSpent
		public abstract class timeSpent : PX.Data.BQL.BqlInt.Field<timeSpent> { }

		/// <summary>
		/// The sum of the minutes of work time spent for all the days of the week. <para></para>
		/// This is a read-only calculated field.
		/// </summary>
		[PXInt]
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

		/// <summary>
		/// The minutes of work time reported for Sunday, including overtime.
		/// </summary>
		[PXTimeList]
		[PXDBInt(BqlField = typeof(EPTimeCardSummary.sun))]
		[PXUIField(DisplayName = "Sun")]
		public virtual int? Sun { get; set; }
		#endregion
		#region Mon
		public abstract class mon : PX.Data.BQL.BqlInt.Field<mon> { }

		/// <summary>
		/// The minutes of work time reported for Monday, including overtime.
		/// </summary>
		[PXTimeList]
		[PXDBInt(BqlField = typeof(EPTimeCardSummary.mon))]
		[PXUIField(DisplayName = "Mon")]
		public virtual int? Mon { get; set; }
		#endregion
		#region Tue
		public abstract class tue : PX.Data.BQL.BqlInt.Field<tue> { }

		/// <summary>
		/// The minutes of work time reported for Tuesday, including overtime.
		/// </summary>
		[PXTimeList]
		[PXDBInt(BqlField = typeof(EPTimeCardSummary.tue))]
		[PXUIField(DisplayName = "Tue")]
		public virtual int? Tue { get; set; }
		#endregion
		#region Wed
		public abstract class wed : PX.Data.BQL.BqlInt.Field<wed> { }

		/// <summary>
		/// The minutes of work time reported for Wednesday, including overtime.
		/// </summary>
		[PXTimeList]
		[PXDBInt(BqlField = typeof(EPTimeCardSummary.wed))]
		[PXUIField(DisplayName = "Wed")]
		public virtual int? Wed { get; set; }
		#endregion
		#region Thu
		public abstract class thu : PX.Data.BQL.BqlInt.Field<thu> { }

		/// <summary>
		/// The minutes of work time reported for Thursday, including overtime.
		/// </summary>
		[PXTimeList]
		[PXDBInt(BqlField = typeof(EPTimeCardSummary.thu))]
		[PXUIField(DisplayName = "Thu")]
		public virtual int? Thu { get; set; }
		#endregion
		#region Fri
		public abstract class fri : PX.Data.BQL.BqlInt.Field<fri> { }

		/// <summary>
		/// The minutes of work time reported for Friday, including overtime.
		/// </summary>
		[PXTimeList]
		[PXDBInt(BqlField = typeof(EPTimeCardSummary.fri))]
		[PXUIField(DisplayName = "Fri")]
		public virtual int? Fri { get; set; }
		#endregion
		#region Sat
		public abstract class sat : PX.Data.BQL.BqlInt.Field<sat> { }

		/// <summary>
		/// The minutes of work time reported for Saturday, including overtime.
		/// </summary>
		[PXTimeList]
		[PXDBInt(BqlField = typeof(EPTimeCardSummary.sat))]
		[PXUIField(DisplayName = "Sat")]
		public virtual int? Sat { get; set; }
		#endregion

		#region IsBillable
		public abstract class isBillable : PX.Data.BQL.BqlBool.Field<isBillable> { }

		/// <summary>
		/// Indicates (if set to <see langword="true"/>) that the time is billable.
		/// </summary>
		[PXDBBool(BqlField = typeof(EPTimeCardSummary.isBillable))]
		[PXDefault]
		[PXUIField(DisplayName = "Billable")]
		public virtual bool? IsBillable { get; set; }
		#endregion

		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

		/// <summary>
		/// The description of the reported work hours.
		/// </summary>
		[PXDBString(255, IsUnicode = true, BqlField = typeof(EPTimeCardSummary.description))]
		[PXUIField(DisplayName = "Description")]
		public virtual string Description { get; set; }
		#endregion

		#region hasApprove
		public abstract class hasApprove : PX.Data.BQL.BqlString.Field<hasApprove> { }
		[PXDBString(2, IsFixed = true, BqlField = typeof(EPActivityApprove.approvalStatus))]
		public virtual string HasApprove { get; set; }
		#endregion

		#region hasReject
		public abstract class hasReject : PX.Data.BQL.BqlString.Field<hasReject> { }
		[PXDBString(2, IsFixed = true, BqlField = typeof(EPActivityReject.approvalStatus))]
		public virtual string HasReject { get; set; }
		#endregion

		#region HasComplite
		public abstract class hasComplite : PX.Data.BQL.BqlString.Field<hasComplite> { }

		[PXDBString(2, IsFixed = true, BqlField = typeof(EPActivityComplite.approvalStatus))]
		public virtual string HasComplite { get; set; }
		#endregion

		#region HasOpen
		public abstract class hasOpen : PX.Data.BQL.BqlString.Field<hasOpen> { }

		[PXDBString(2, IsFixed = true, BqlField = typeof(EPActivityOpen.approvalStatus))]
		public virtual string HasOpen { get; set; }
		#endregion

		#region WeekID
		public abstract class weekId : PX.Data.BQL.BqlInt.Field<weekId> { }

		/// <summary>
		/// The identifier of the <see cref="EPWeekRaw">week</see> associated with this time activity.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="EPWeekRaw.Description" /> field.
		/// </value>
		[PXDBInt(BqlField = typeof(EPTimeCard.weekId))]
		[PXUIField(DisplayName = "Week")]
		[PXWeekSelector2(DescriptionField = typeof(EPWeekRaw.shortDescription))]
		public virtual int? WeekID { get; set; }
		#endregion

		#region ApproverID
		public abstract class approverID : PX.Data.BQL.BqlInt.Field<approverID> { }

		private int? _ProjectApproverID;

		/// <summary>
		/// The identifier of the <see cref="EPEmployee">person</see> authorized to approve these time activities.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="BAccount.BAccountID" /> field.
		/// </value>
		[PXDBInt(BqlField = typeof(PMProject.approverID))]
		[PXEPEmployeeSelector]
		[PXUIField(DisplayName = "Approver", Visibility = PXUIVisibility.SelectorVisible)]
		public int? ApproverID {
			get { return TaskApproverID ?? _ProjectApproverID; }
			set { _ProjectApproverID = value; }
		}
		#endregion

		#region TaskApproverID
		public abstract class taskApproverID : PX.Data.BQL.BqlInt.Field<taskApproverID> { }

		/// <summary>
		/// The identifier of the <see cref="EPEmployee">person</see> authorized to approve the task associated with these time activities.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="BAccount.BAccountID" /> field.
		/// </value>
		[PXDBInt(BqlField = typeof(PMTask.approverID))]
		[PXEPEmployeeSelector]
		[PXUIField(DisplayName = "Task Project Manager", Visibility = PXUIVisibility.SelectorVisible)]
		public int? TaskApproverID { get; set; }
		#endregion

		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }

		/// <summary>
		/// The identifier of the <see cref="CREmployee">user</see> to whom this time activity's <see cref="EPSummaryApprove">employee</see> is associated with, if any.
		/// <para></para>
		/// See also <see cref="SubordinateOwnerEmployeeAttribute"></see>.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="BAccount.BAccountID" /> field.
		/// </value>
		[Owner(BqlField = typeof(PMTimeActivity.ownerID), DisplayName = "Employee")]
		public virtual int? OwnerID { set; get; }
		#endregion

		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }

		/// <summary>
		/// The identifier of the <see cref="EPEmployee">employee</see> whose summary rows you want to work with.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="BAccount.BAccountID" /> field.
		/// </value>
		[PXDBInt(BqlField = typeof(CREmployee.bAccountID))]
		[PXSelector(typeof(Search<CREmployee.bAccountID>), DescriptionField = typeof(CREmployee.acctName))]
		[PXUIField(DisplayName = "Employee")]
		public virtual int? EmployeeID { set; get; }
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;

		/// <summary>
		/// Identifier of the <see cref="Note">Note</see> object associated with this activity.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Note.NoteID"></see> field. 
		/// </value>
		[PXNote(BqlField=typeof(EPTimeCardSummary.noteID))]
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
	}
	#endregion

	public partial class EPTimeCardEx : EPTimeCard
	{
		public new abstract class timeCardCD : PX.Data.BQL.BqlString.Field<timeCardCD> { }
		public new abstract class origTimeCardCD : PX.Data.BQL.BqlString.Field<origTimeCardCD> { }
	}
}
