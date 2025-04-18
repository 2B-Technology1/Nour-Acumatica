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

using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using PX.Common;
using PX.CS;
using PX.Data.EP;
using PX.Objects.CR;
using PX.Data;
using System;
using System.Collections;
using PX.Objects.GL;
using PX.Objects.PM;
using PX.Objects.CS;
using PX.Objects.Common.GraphExtensions.Abstract;
using System.Web.Compilation;

namespace PX.Objects.EP
{
	public class CRActivityMaint : CRBaseActivityMaint<CRActivityMaint, CRActivity>
	{
		#region Extensions

		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class EmbeddedImagesExtractor : EmbeddedImagesExtractorExtension<CRActivityMaint, CRActivity, CRActivity.body>
		{
		}

		#endregion

		#region Constants

		private static readonly EPSetup EmptyEpSetup = new EPSetup();

		#endregion

		#region Selects

		[PXHidden]
		public PXSelect<CT.Contract>
			BaseContract;

		[PXCopyPasteHiddenFields(typeof(CRActivity.body))]
		public PXSelect<CRActivity,
			Where<CRActivity.classID, Equal<CRActivityClass.activity>>>
			Activities;
                
        [PXHidden]
        [PXCopyPasteHiddenView]
        public PXSelect<CRActivity,
            Where<CRActivity.noteID, Equal<Current<CRActivity.noteID>>>>
            CurrentActivity;

        public PXSelect<PMTimeActivity,
			Where<PMTimeActivity.refNoteID, Equal<Current<CRActivity.noteID>>>>
			TimeActivitiesOld;

		public PMTimeActivityList<CRActivity>
			TimeActivity;

		#endregion

		#region Ctors
		public CRActivityMaint()
		{
			CRCaseActivityHelper.Attach(this);
		}

		#endregion

		#region Actions

		public PXDelete<CRActivity> Delete;

		public PXAction<CRActivity> MarkAsCompleted;
		[PXUIField(DisplayName = Messages.Complete)]
		[PXButton(Tooltip = Messages.MarkAsCompletedTooltip,
			ShortcutCtrl = true, ShortcutChar = (char)75)] //Ctrl + K
		public virtual void markAsCompleted()
		{
			CRActivity row = Activities.Current;
			if (row == null) return;

			Actions.PressSave();

			CompleteActivity(row);
		}

		public PXAction<CRActivity> MarkAsCompletedAndFollowUp;

		[PXUIField(DisplayName = Messages.CompleteAndFollowUp)]
		[PXButton(Tooltip = Messages.CompleteAndFollowUpTooltip,
			ShortcutCtrl = true, ShortcutShift = true, ShortcutChar = (char)75)] //Ctrl + Shift + K
		public virtual void markAsCompletedAndFollowUp()
		{
			CRActivity row = Activities.Current;
			if (row == null) return;

			Actions.PressSave();

			CompleteActivity(row);

			CRActivityMaint graph = CreateInstance<CRActivityMaint>();

			CRActivity followUpActivity = (CRActivity)graph.Activities.Cache.CreateCopy(row);
			followUpActivity.NoteID = null;
			followUpActivity.ParentNoteID = row.ParentNoteID;
			followUpActivity.UIStatus = null;
			followUpActivity.PercentCompletion = null;

			if (followUpActivity.StartDate != null)
			{
				followUpActivity.StartDate = ((DateTime)followUpActivity.StartDate).AddDays(1D);
			}
			if (followUpActivity.EndDate != null)
				followUpActivity.EndDate = ((DateTime)followUpActivity.EndDate).AddDays(1D);

			graph.Activities.Insert(followUpActivity);

			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
		}

		#endregion

		#region Event Handlers

		[PXUIField(DisplayName = "Task")]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void CRActivity_ParentNoteID_CacheAttached(PXCache cache) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[ProjectTask(typeof(PMTimeActivity.projectID), BatchModule.TA, DisplayName = "Project Task", DefaultActiveTask = true)]
		protected virtual void _(Events.CacheAttached<PMTimeActivity.projectTaskID> e) { }

		protected virtual void _(Events.FieldUpdated<PMTimeActivity, PMTimeActivity.trackTime> e)
		{
			PMTimeActivity row = e.Row;
			if (row == null) return;
			if (ProjectDefaultAttribute.IsNonProject(row.ProjectID)) return;

			bool isPmNotVisible = !ProjectAttribute.IsPMVisible(BatchModule.TA);
			if (row.TrackTime == false || isPmNotVisible)
			{
				e.Cache.SetValueExt<PMTimeActivity.projectID>(row, PM.ProjectDefaultAttribute.NonProject());
			}
		}

		protected virtual void CRActivity_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			//TODO redesign by task #32833
			var row = (CRActivity)e.Row;
			if (row == null) return;
			row.ClassID = CRActivityClass.Activity;
		}

		protected virtual void CRActivity_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			var row = (CRActivity)e.Row;
			var oldRow = (CRActivity)e.OldRow;
			if (row == null || oldRow == null) return;
			row.ClassID = CRActivityClass.Activity;
		}

		protected virtual void CRActivity_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			CRActivity row = (CRActivity)e.Row;
			if (row == null) return;

			var tAct = (PMTimeActivity)TimeActivity.SelectSingle();
			var tActCache = TimeActivity.Cache;

			PXUIFieldAttribute.SetEnabled<CRActivity.endDate>(cache, row, false);

			bool wasUsed = !string.IsNullOrEmpty(tAct?.TimeCardCD) || tAct?.Billed == true;
			if (wasUsed)
				PXUIFieldAttribute.SetEnabled(cache, row, false);

			bool isPmVisible = ProjectAttribute.IsPMVisible(BatchModule.TA);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.timeSpent>(tActCache, null, tAct?.TrackTime == true);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.approvalStatus>(tActCache, null, tAct?.TrackTime == true);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.earningTypeID>(tActCache, null, tAct?.TrackTime == true);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.isBillable>(tActCache, null, tAct?.TrackTime == true);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.released>(tActCache, null, tAct?.TrackTime == true);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.timeBillable>(tActCache, null, tAct?.IsBillable == true && tAct?.TrackTime == true);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.arRefNbr>(tActCache, null, tAct?.IsBillable == true && tAct?.TrackTime == true);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.approverID>(tActCache, null, tAct?.TrackTime == true);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.projectID>(tActCache, null, tAct?.TrackTime == true && isPmVisible);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.certifiedJob>(tActCache, null, tAct?.TrackTime == true && isPmVisible);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.projectTaskID>(tActCache, null, tAct?.TrackTime == true && isPmVisible);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.costCodeID>(tActCache, null, tAct?.TrackTime == true && isPmVisible);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.labourItemID>(tActCache, null, tAct?.TrackTime == true && isPmVisible);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.unionID>(tActCache, null, tAct?.TrackTime == true);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.workCodeID>(tActCache, null, tAct?.TrackTime == true);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.shiftID>(tActCache, null, tAct?.TrackTime == true);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.overtimeSpent>(tActCache, null, false);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.overtimeSpent>(tActCache, null, false);
			PXUIFieldAttribute.SetVisible<PMTimeActivity.overtimeBillable>(tActCache, null, false);

			if (tAct != null)
			{
				PXUIFieldAttribute.SetRequired<PMTimeActivity.projectTaskID>(tActCache, tAct.ProjectID != null && tAct.ProjectID != PM.ProjectDefaultAttribute.NonProject());
			}

			bool showMinutes = EPSetupCurrent.RequireTimes == true;
			PXDBDateAndTimeAttribute.SetTimeEnabled<CRActivity.startDate>(cache, row, showMinutes && tAct?.TrackTime == true);
			PXDBDateAndTimeAttribute.SetTimeVisible<CRActivity.startDate>(cache, row, showMinutes && tAct?.TrackTime == true);
			PXDBDateAndTimeAttribute.SetTimeVisible<CRActivity.endDate>(cache, row, showMinutes && tAct?.TrackTime == true);			

			string origTimeStatus =
				(string)this.TimeActivity.Cache.GetValueOriginal<PMTimeActivity.approvalStatus>(tAct)
				?? ActivityStatusListAttribute.Open;

			string origStatus =
				(string)this.Activities.Cache.GetValueOriginal<CRActivity.uistatus>(row)
				?? origTimeStatus;

			bool? origTrackTime =
				(bool?)this.TimeActivity.Cache.GetValueOriginal<PMTimeActivity.trackTime>(tAct) 
				?? false;

			if (origStatus == ActivityStatusListAttribute.Completed && origTrackTime != true)
				origStatus = ActivityStatusListAttribute.Open;

			if (row.IsLocked == true)
				origStatus = ActivityStatusListAttribute.Completed;

			if (origStatus == ActivityStatusListAttribute.Open)
			{
				PXUIFieldAttribute.SetEnabled(cache, row, true);
				Delete.SetEnabled(!wasUsed);
			}
			else
			{
				PXUIFieldAttribute.SetEnabled(cache, row, false);								
				Delete.SetEnabled(false);
			}

			PXUIFieldAttribute.SetEnabled<CRActivity.noteID>(cache, row);

			MarkAsCompleted.SetEnabled(origStatus == ActivityStatusListAttribute.Open);
			MarkAsCompleted.SetVisible(origStatus == ActivityStatusListAttribute.Open && tAct?.TrackTime == true);
			MarkAsCompletedAndFollowUp.SetVisible(false);

			// TimeActivity

			if (tAct?.Released == true)
				origTimeStatus = ActivityStatusAttribute.Completed;

			if (origTimeStatus == ActivityStatusListAttribute.Open)
			{
				PXUIFieldAttribute.SetEnabled(tActCache, tAct, true);

				PXUIFieldAttribute.SetEnabled<PMTimeActivity.timeBillable>(tActCache, tAct, !wasUsed && tAct?.IsBillable == true);
				PXUIFieldAttribute.SetEnabled<PMTimeActivity.overtimeBillable>(tActCache, tAct, !wasUsed && tAct?.IsBillable == true);
			}
			else
			{
				PXUIFieldAttribute.SetEnabled(tActCache, tAct, false);
			}
			PXUIFieldAttribute.SetEnabled<PMTimeActivity.approvalStatus>(tActCache, tAct, tAct != null && tAct.TrackTime == true && !wasUsed);

			PXUIFieldAttribute.SetEnabled<PMTimeActivity.released>(tActCache, tAct, false);

			PXDefaultAttribute.SetPersistingCheck<CRActivity.ownerID>(cache, row, tAct?.TrackTime == true ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<CRActivity.type>(cache, row, PXPersistingCheck.Null);

			PXUIFieldAttribute.SetEnabled<CRActivity.refNoteID>(cache, row, cache.GetValue<CRActivity.refNoteIDType>(row) != null || IsContractBasedAPI);
		}

		protected virtual void PMTimeActivity_ProjectID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PMTimeActivity row = (PMTimeActivity)e.Row;
			if (row == null) return;

			if (e.NewValue != null && e.NewValue is int)
			{
				PMProject proj = PXSelect<PMProject>.Search<PMProject.contractID>(sender.Graph, e.NewValue);
				if (proj != null)
				{
					if (proj.IsCompleted == true)
					{
						var ex = new PXSetPropertyException(PM.Messages.ProjectIsCompleted);
						ex.ErrorValue = proj.ContractCD;
						throw ex;
					}
					if (proj.IsCancelled == true)
					{
						var ex = new PXSetPropertyException(PM.Messages.ProjectIsCanceled);
						ex.ErrorValue = proj.ContractCD; ;
						throw ex;
					}
					if (proj.Status == CT.Contract.status.Expired)
					{
						var ex = new PXSetPropertyException(PM.Messages.ProjectIsSuspended);
						ex.ErrorValue = proj.ContractCD; ;
						throw ex;
					}
				}
			}
		}

		protected virtual void PMTimeActivity_ProjectTaskID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PMTimeActivity row = (PMTimeActivity)e.Row;
			if (row == null) return;

			if (e.NewValue != null && e.NewValue is int)
			{
				PMTask task = PXSelect<PMTask>.Search<PMTask.projectID, PMTask.taskID>(sender.Graph, row.ProjectID, e.NewValue);
				if (task != null)
				{
					if (task.IsCompleted == true)
					{
						var ex = new PXSetPropertyException(PM.Messages.ProjectTaskIsCompleted);
						ex.ErrorValue = task.TaskCD;
						throw ex;
					}
					if (task.IsCancelled == true)
					{
						var ex = new PXSetPropertyException(PM.Messages.ProjectTaskIsCanceled);
						ex.ErrorValue = task.TaskCD;
						throw ex;
					}
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMTimeActivity, PMTimeActivity.ownerID> e)
		{
			e.Cache.SetDefaultExt<PMTimeActivity.labourItemID>(e.Row);
		}
		protected virtual void _(Events.FieldUpdated<PMTimeActivity, PMTimeActivity.costCodeID> e)
		{
			e.Cache.SetDefaultExt<PMTimeActivity.workCodeID>(e.Row);
		}
		protected virtual void _(Events.FieldUpdated<PMTimeActivity, PMTimeActivity.projectID> e)
		{
			e.Cache.SetDefaultExt<PMTimeActivity.unionID>(e.Row);
			e.Cache.SetDefaultExt<PMTimeActivity.certifiedJob>(e.Row);
			e.Cache.SetDefaultExt<PMTimeActivity.labourItemID>(e.Row);
		}

		protected virtual void _(Events.FieldDefaulting<PMTimeActivity, PMTimeActivity.costCodeID> e)
		{
			if (CostCodeAttribute.UseCostCode())
			{
				e.NewValue = CostCodeAttribute.DefaultCostCode;
			}
		}

		#endregion

		#region Public Methods

		public virtual int? GetDefaultLaborItem(int? employeeID, string earningType, int? projectID)
		{
			if (employeeID == null)
				return null;

			int? result = null;

			if (ProjectDefaultAttribute.IsProject(this, projectID))
			{
				result = EPContractRate.GetProjectLaborClassID(this, projectID.Value, employeeID.Value, earningType);
			}

			if (result == null)
			{
				result = EPEmployeeClassLaborMatrix.GetLaborClassID(this, employeeID, earningType);
			}

			if (result == null)
			{
				EPEmployee employee = PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<EPTimeCard.employeeID>>>>.Select(this);
				if (employee != null)
				{
					result = employee.LabourItemID;
				}
			}

			return result;
		}

		public override void CompleteRow(CRActivity row)
		{
			CompleteActivity(row);
		}

		public static TimeSpan CalculateOvertime(PXGraph graph, PMTimeActivity act, DateTime start, DateTime end)
		{
			var calendarId = GetCalendarID(graph, act);
			return calendarId == null ? new TimeSpan() : CalendarHelper.CalculateOvertime(graph, start, end, calendarId);
		}

		public static string GetCalendarID(PXGraph graph, PMTimeActivity act)
		{
			var projectCalendar = act.ProjectID.
				With(_ => (CT.Contract)PXSelect<CT.Contract,
					Where<CT.Contract.contractID, Equal<Required<CT.Contract.contractID>>>>.
				Select(graph, _.Value)).
				With(_ => _.CalendarID);
			if (projectCalendar != null) return projectCalendar;

			var projectTaskCalendar = act.ProjectTaskID.
				With(_ => (PXResult<Location, PM.PMTask>)PXSelectJoin<Location,
					InnerJoin<PM.PMTask, On<PM.PMTask.customerID, Equal<Location.bAccountID>, And<PM.PMTask.locationID, Equal<Location.locationID>>>>,
					Where<PM.PMTask.taskID, Equal<Required<PM.PMTask.taskID>>>>.
				Select(graph, _.Value)).
				With(_ => ((Location)_).CCalendarID);
			if (projectTaskCalendar != null) return projectTaskCalendar;

			var caseLocationCalendar = act.RefNoteID.
				With(_ => (PXResult<Location, CRCase>)PXSelectJoin<Location,
					InnerJoin<CRCase, 
						On<CRCase.customerID, Equal<Location.bAccountID>, 
						And<CRCase.locationID, Equal<Location.locationID>>>, 
					InnerJoin<CRActivityLink,
						On<CRActivityLink.refNoteID, Equal<CRCase.noteID>>>>,
					Where<CRActivityLink.noteID, Equal<Required<PMTimeActivity.refNoteID>>>>.
				Select(graph, _.Value)).
				With(_ => ((Location)_).CCalendarID);
			if (caseLocationCalendar != null) return caseLocationCalendar;

			var employeeCalendar = act.OwnerID.
				With(_ => (EPEmployee)PXSelect<EPEmployee,
					Where<EPEmployee.defContactID, Equal<Required<PMTimeActivity.ownerID>>>>.
				Select(graph, _.Value)).
				With(_ => _.CalendarID);
			if (employeeCalendar != null) return employeeCalendar;

			return null;
		}

		public static DateTime? GetNextActivityStartDate<Activity>(PXGraph graph, PXResultset<Activity> res, PMTimeActivity row, int? fromWeekId, int? tillWeekId, PXCache tempDataCache, Type tempDataField)
			where Activity : PMTimeActivity, new()
		{
			DateTime? date;
			if (fromWeekId != null || tillWeekId != null)
				date = PXWeekSelector2Attribute.GetWeekStartDate(graph, (int)(fromWeekId ?? tillWeekId));
			else
				date = graph.Accessinfo.BusinessDate.GetValueOrDefault(DateTime.Now).Date;

			EPEmployee employee = PXSelect<EPEmployee, Where<EPEmployee.defContactID, Equal<Required<PMTimeActivity.ownerID>>>>.Select(graph, row.OwnerID);
			EPEmployeeClass employeeClass = PXSelect<EPEmployeeClass, Where<EPEmployeeClass.vendorClassID, Equal<Required<EPEmployee.vendorClassID>>>>.Select(graph, employee != null ? employee.VendorClassID : null);
			var calendarId = CRActivityMaint.GetCalendarID(graph, row);

			if (employeeClass != null && EPEmployeeClass.defaultDateInActivity.LastDay == employeeClass.DefaultDateInActivity)
			{
				DateTime? val = tempDataCache.GetValue(tempDataCache.Current, tempDataField.Name) as DateTime?;
				if (val != null)
				{
					int week = PXWeekSelector2Attribute.GetWeekID(graph, (DateTime)val);
					if ((fromWeekId == null || week >= fromWeekId) && (tillWeekId == null || tillWeekId >= week))
						date = val;
				}
			}
			else
			{
				DateTime weekDate = (DateTime)date;
				DateTime? newDate = null;
				var resList = res?.ToList();
				date = resList != null && resList.Count > 0 ? resList.Max(_ => ((Activity)_).Date) : null ?? date;
				for (int curentWeek = PXWeekSelector2Attribute.GetWeekID(graph, weekDate); tillWeekId == null || curentWeek <= tillWeekId; curentWeek = PXWeekSelector2Attribute.GetWeekID(graph, weekDate))
				{
					PXWeekSelector2Attribute.WeekInfo week1 = PXWeekSelector2Attribute.GetWeekInfo(graph,
						PXWeekSelector2Attribute.GetWeekID(graph, weekDate));
					foreach (KeyValuePair<DayOfWeek, PXWeekSelector2Attribute.DayInfo> pair in week1.Days.OrderBy(_ => _.Value.Date))
					{
						if (pair.Value.Date >= date &&
							(CalendarHelper.IsWorkDay(graph, calendarId, (DateTime)pair.Value.Date) ||
							 string.IsNullOrEmpty(calendarId) && pair.Key != DayOfWeek.Saturday && pair.Key != DayOfWeek.Sunday))
						{
							newDate = (DateTime)pair.Value.Date;
							break;
						}
						weekDate = weekDate.AddDays(1D);
					}
					if (newDate != null)
					{
						date = ((DateTime)newDate).Date;
						break;
					}
				}
			}

			if (!string.IsNullOrEmpty(calendarId) && date != null)
			{
				DateTime startDate;
				DateTime endDate;
				CalendarHelper.CalculateStartEndTime(graph, calendarId, (DateTime)date, out startDate, out endDate);
				date = startDate;
			}

			return date;
		}

		[Serializable]
		[PXHidden]
		public class EPTempData : IBqlTable
		{
			#region LastEnteredDate
			public abstract class lastEnteredDate : PX.Data.BQL.BqlDateTime.Field<lastEnteredDate> { }
			protected DateTime? _LastEnteredDate;
			[PXDate]
			public virtual DateTime? LastEnteredDate
			{
				get
				{
					return this._LastEnteredDate;
				}
				set
				{
					this._LastEnteredDate = value;
				}
			}
			#endregion

		}

		#endregion

		#region Private Methods

		private void CompleteActivity(CRActivity activity)
		{
			string origStatus = (string)this.Activities.Cache.GetValueOriginal<CRActivity.uistatus>(activity) ?? ActivityStatusListAttribute.Open;

			if (activity == null ||
				origStatus == ActivityStatusListAttribute.Completed ||
				origStatus == ActivityStatusListAttribute.Canceled)
			{
				return;
			}

			activity.UIStatus = ActivityStatusListAttribute.Completed;
			Activities.Cache.Update(activity);

			var timeAct = TimeActivity.Current;

			if (timeAct != null)
			{
				timeAct.ApprovalStatus = ActivityStatusListAttribute.Completed;
				TimeActivity.Cache.Update(timeAct);
			}

			Actions.PressSave();
		}

		private EPSetup EPSetupCurrent
		{
			get
			{
				var res = (EPSetup)PXSelect<EPSetup>.
					SelectWindowed(this, 0, 1);
				return res ?? EmptyEpSetup;
			}
		}


		#endregion
	}
}
