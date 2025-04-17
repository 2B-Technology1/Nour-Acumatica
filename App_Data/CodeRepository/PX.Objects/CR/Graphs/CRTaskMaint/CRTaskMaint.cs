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
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Objects.Common.GraphExtensions.Abstract;
using PX.Objects.EP;
using PX.SM;

namespace PX.Objects.CR
{
	public class CRTaskMaint : CRBaseActivityMaint<CRTaskMaint, CRActivity>, ICaptionable
	{
		#region Extensions

		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class EmbeddedImagesExtractor : EmbeddedImagesExtractorExtension<CRTaskMaint, CRActivity, CRActivity.body>
		{
		}

		#endregion

		#region Selects

		[PXHidden]
		public PXSelect<CT.Contract>
				BaseContract;

		[PXCopyPasteHiddenFields(
			typeof(CRActivity.completedDate),
			typeof(CRActivity.percentCompletion),
			typeof(CRActivity.endDate.endDate_date),
			typeof(CRActivity.startDate.startDate_date),
			typeof(CRActivity.uistatus))]
		public PXSelect<CRActivity,
			Where<CRActivity.classID, Equal<CRActivityClass.task>>>
			Tasks;

		[PXHidden]
		public PXSelect<PMTimeActivity>
			TimeActivitiesOld;

		[PXCopyPasteHiddenFields(
			typeof(PMTimeActivity.timeSpent),
			typeof(PMTimeActivity.overtimeSpent),
			typeof(PMTimeActivity.timeBillable),
			typeof(PMTimeActivity.overtimeBillable))]
		public PMTimeActivityList<CRActivity>
			TimeActivity;

		[PXCopyPasteHiddenView]
		public CRReminderList<CRActivity>
			Reminder;

		#endregion

		#region Ctors

		public CRTaskMaint()
			: base()
		{
			ActivityStatusAttribute.SetRestictedMode<CRActivity.uistatus>(Tasks.Cache, true);
		}

		public string Caption()
		{
			CRActivity currentItem = this.Tasks.Current;
			if (currentItem == null) return "";

			if (currentItem.Subject != null)
				return $"{currentItem.Subject}";
			return "";
		}

		#endregion

		#region Actions

		public PXCopyPasteAction<CRActivity> CopyPaste;
		public PXDelete<CRActivity> Delete;

		public PXAction<CRActivity> Complete;
		[PXUIField(DisplayName = TM.Messages.CompleteTask, MapEnableRights = PXCacheRights.Select)]
		[PXButton(Tooltip = Messages.CompleteTaskTooltip,
			ShortcutCtrl = true, ShortcutChar = (char)75, //Ctrl + K
			Connotation = Data.WorkflowAPI.ActionConnotation.Success)]
		protected virtual void complete()
		{
			var row = Tasks.Current;
			if (row == null) return;

			CompleteTask(row);
		}

		public PXAction<CRActivity> CompleteAndFollowUp;
		[PXUIField(DisplayName = Messages.CompleteTaskAndFollowUp, MapEnableRights = PXCacheRights.Select)]
		[PXButton(Tooltip = Messages.CompleteTaskAndFollowUpTooltip,
			ShortcutCtrl = true, ShortcutShift = true, ShortcutChar = (char)75)] //Ctrl + Shift + K
		protected virtual void completeAndFollowUp()
		{
			CRActivity row = Tasks.Current;
			if (row == null) return;

			CompleteTask(row);

			CRTaskMaint graph = CreateInstance<CRTaskMaint>();

			CRActivity followUpTask = (CRActivity)graph.Tasks.Cache.CreateCopy(row);
			followUpTask.NoteID = null;
			followUpTask.ParentNoteID = row.NoteID;
			followUpTask.UIStatus = null;
			followUpTask.PercentCompletion = null;

			followUpTask = (CRActivity)graph.Tasks.Cache.Insert(followUpTask);
			UDFHelper.CopyAttributes(this.Caches<CRActivity>(), row, graph.Tasks.Cache, followUpTask, null);

			CRReminder oldReminder = Reminder.Current;
			CRReminder newReminder = graph.Reminder.Current;
			if (oldReminder != null && newReminder != null)
			{
				newReminder.ReminderDate = oldReminder.ReminderDate;
				newReminder.RefNoteID = followUpTask.NoteID;

				graph.Reminder.Cache.Update(newReminder);
			}

			if (!this.IsContractBasedAPI)
				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);

			graph.Save.Press();
		}

		public PXAction<CRActivity> CancelActivity;
		[PXUIField(DisplayName = TM.Messages.CancelTask, MapEnableRights = PXCacheRights.Select)]
		[PXButton(Tooltip = TM.Messages.CancelTask)]
		protected virtual void cancelActivity()
		{
			var row = Tasks.Current;
			if (row == null) return;

			CancelTask(row);
		}

		#endregion

		#region Event Handlers

		[TaskStatus]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void CRActivity_UIStatus_CacheAttached(PXCache cache) { }

		[PXUIField(DisplayName = "Start Date")]
		[EPStartDate(DisplayName = "Start Date", DisplayNameDate = "Start Date", DisplayNameTime = "Start Time", PreserveTime = false, BqlField = typeof(CRActivity.startDate), AllDayField = typeof(CRActivity.allDay))]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		protected virtual void CRActivity_StartDate_CacheAttached(PXCache sender) { }

		[PXUIField(DisplayName = Messages.DueDate)]
		[EPEndDate(typeof(CRActivity.classID), typeof(CRActivity.startDate), DisplayNameDate = "Due Date", DisplayNameTime = "Due Time", PreserveTime = false, BqlField = typeof(CRActivity.endDate), AllDayField = typeof(CRActivity.allDay))]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		protected virtual void CRActivity_EndDate_CacheAttached(PXCache sender) { }

		[PXUIField(DisplayName = "Track Time", Visible = false)]
		[PXDefault(false)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void PMTimeActivity_TrackTime_CacheAttached(PXCache cache) { }

		[PXUIField(DisplayName = "Billable", Visible = false)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void PMTimeActivity_IsBillable_CacheAttached(PXCache cache) { }


		[PXFormula(typeof(False))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void PMTimeActivity_NeedToBeDeleted_CacheAttached(PXCache cache) { }

		[PXDefault(CRActivityClass.Task)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void CRActivity_ClassID_CacheAttached(PXCache cache) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PM.ProjectTask(typeof(PMTimeActivity.projectID), GL.BatchModule.TA, DisplayName = "Project Task", DefaultActiveTask = true)]
		protected virtual void _(Events.CacheAttached<PMTimeActivity.projectTaskID> e) { }

		protected virtual void _(Events.FieldUpdated<CRActivity, CRActivity.uistatus> e)
		{
			PMTimeActivity timeActivity = (PMTimeActivity)TimeActivity.SelectSingle();
			if (timeActivity != null)
			{
				Caches[typeof(PMTimeActivity)].MarkUpdated(timeActivity);//For Persisting event handler to sync Status.
			}
		}

		protected virtual void CRActivity_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			var row = (CRActivity)e.Row;
			if (row == null) return;
			
			if (row.UIStatus == ActivityStatusListAttribute.Completed)
			{
				row.PercentCompletion = 100;
			}		
		}
		
		protected virtual void CRActivity_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var row = e.Row as CRActivity;
			var oldRow = (CRActivity)e.OldRow;
			if (row == null || oldRow == null) return;
			
			if (row.UIStatus == ActivityStatusListAttribute.Completed)
			{
				row.PercentCompletion = 100;
				if (!object.Equals(sender.GetValueOriginal<CRActivity.uistatus>(row), ActivityStatusListAttribute.Completed))
					row.CompletedDate = PXTimeZoneInfo.Now;
			}			
		}

		protected virtual void CRActivity_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var row = e.Row as CRActivity;
			if (row == null) return;

			string status = ((string) cache.GetValueOriginal<CRActivity.uistatus>(row) ?? ActivityStatusListAttribute.Open);
			bool editable = status == ActivityStatusListAttribute.Open || status == ActivityStatusListAttribute.Draft || status == ActivityStatusListAttribute.InProcess;

			PXUIFieldAttribute.SetEnabled(cache, row, editable);
			Complete.SetEnabled(row.UIStatus == ActivityStatusListAttribute.Open || row.UIStatus == ActivityStatusListAttribute.InProcess);
			if (row.UIStatus == ActivityStatusListAttribute.Open || row.UIStatus == ActivityStatusListAttribute.InProcess)
			{
				Complete.SetConnotation(Data.WorkflowAPI.ActionConnotation.Success);
			}
			else
			{
				Complete.SetConnotation(Data.WorkflowAPI.ActionConnotation.None);
			}
			CompleteAndFollowUp.SetEnabled(row.UIStatus == ActivityStatusListAttribute.Open || row.UIStatus == ActivityStatusListAttribute.InProcess);
			CancelActivity.SetEnabled(row.UIStatus == ActivityStatusListAttribute.Open || row.UIStatus == ActivityStatusListAttribute.Draft || row.UIStatus == ActivityStatusListAttribute.InProcess);

			PXUIFieldAttribute.SetEnabled<CRActivity.noteID>(cache, row);
			PXUIFieldAttribute.SetEnabled<CRActivity.uistatus>(cache, row);
			PXUIFieldAttribute.SetEnabled<CRActivity.createdByID>(cache, row, false);
			PXUIFieldAttribute.SetEnabled<CRActivity.completedDate>(cache, row, false);

			var tAct = (PMTimeActivity)TimeActivity.SelectSingle();
			var tActCache = TimeActivity.Cache;

			PXUIFieldAttribute.SetEnabled<PMTimeActivity.projectID>(tActCache, tAct, editable);
			PXUIFieldAttribute.SetEnabled<PMTimeActivity.projectTaskID>(tActCache, tAct, editable);
			
			PXUIFieldAttribute.SetEnabled<CRReminder.isReminderOn>(Reminder.Cache, Reminder.SelectSingle(), editable);
			PXUIFieldAttribute.SetEnabled<CRActivity.parentNoteID>(cache, row, false);

			// Acuminator disable once PX1043 SavingChangesInEventHandlers [Persist is called only once on the entity opening by the user]
			// Acuminator disable once PX1044 ChangesInPXCacheInEventHandlers [EPView is placed in cache to Hold the entity]
			MarkAs(cache, row, Accessinfo.ContactID, EPViewStatusAttribute.VIEWED);

			PXUIFieldAttribute.SetEnabled<CRActivity.refNoteID>(cache, row, cache.GetValue<CRActivity.refNoteIDType>(row) != null || IsContractBasedAPI);
		}
		
		protected virtual void CRActivity_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var row = e.Row as CRActivity;
			if (row == null) return;
			
			if ((e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update) && row.OwnerID == null && row.WorkgroupID == null)
			{
				var displayName = PXUIFieldAttribute.GetDisplayName<CRActivity.ownerID>(Tasks.Cache);
				var exception = new PXSetPropertyException(ErrorMessages.FieldIsEmpty, displayName);
				if (Tasks.Cache.RaiseExceptionHandling<CRActivity.ownerID>(row, null, exception))
				{
					throw new PXRowPersistingException(typeof(CRActivity.ownerID).Name, null, ErrorMessages.FieldIsEmpty, displayName);
				}
			}
		}

		[PXUIField(DisplayName = "Parent Activity", Enabled = false)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void CRActivity_ParentNoteID_CacheAttached(PXCache cache) { }

		protected virtual void _(Events.FieldDefaulting<PMTimeActivity, PMTimeActivity.costCodeID> e)
		{
			if (PM.CostCodeAttribute.UseCostCode())
			{
				e.NewValue = PM.CostCodeAttribute.DefaultCostCode;
			}
		}

		protected virtual void _(Events.RowSelected<CRReminder> e)
		{
			if (e.Row == null) return;

			bool bIsReminderOn = true.Equals(e.Row.IsReminderOn);
			PXUIFieldAttribute.SetVisible<CRReminder.reminderDate>(e.Cache, e.Row, bIsReminderOn);
			PXUIFieldAttribute.SetEnabled<CRReminder.reminderDate>(e.Cache, e.Row, bIsReminderOn);
			PXUIFieldAttribute.SetRequired<CRReminder.reminderDate>(e.Cache, bIsReminderOn);
		}

		#endregion

		#region Private Methods

		private void CompleteTask(CRActivity row)
		{
			string origStatus = (string)Tasks.Cache.GetValueOriginal<CRActivity.uistatus>(row) ?? ActivityStatusListAttribute.Open;			
			if (origStatus == ActivityStatusListAttribute.Completed ||
					origStatus == ActivityStatusListAttribute.Canceled)
			{
				return;
			}
						
			CRActivity activityCopy = (CRActivity)Tasks.Cache.CreateCopy(row);
			activityCopy.UIStatus = ActivityStatusListAttribute.Completed;			
			Tasks.Cache.Update(activityCopy);
			Actions.PressSave();
		}

		private void CancelTask(CRActivity row)
		{
			string origStatus = (string)Tasks.Cache.GetValueOriginal<CRActivity.uistatus>((CRActivity)row) ?? ActivityStatusListAttribute.Open;
			if (origStatus == ActivityStatusListAttribute.Completed ||
					origStatus == ActivityStatusListAttribute.Canceled)
			{
				return;
			}


			CRActivity activityCopy = (CRActivity)Tasks.Cache.CreateCopy((CRActivity)row);			
			activityCopy.UIStatus = ActivityStatusListAttribute.Canceled;
			Tasks.Cache.Update(activityCopy);
			Actions.PressSave();
		}
		
		#endregion

		#region Public Methods

		public override void CompleteRow(CRActivity row)
		{
			if (row != null) CompleteTask(row);
		}

		public override void CancelRow(CRActivity row)
		{
			if (row != null) CancelTask(row);
		}

		#endregion
	}
}
