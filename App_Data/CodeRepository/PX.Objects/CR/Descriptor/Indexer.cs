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

using PX.Api;
using PX.Common;
using PX.Common.Collection;
using PX.Common.Mail;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.EP;
using PX.Data.Maintenance.GI;
using PX.Data.SQLTree;
using PX.Metadata;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR.Extensions;
using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.IN.Matrix.Attributes;
using PX.Objects.PM;
using PX.Objects.SO;
using PX.Reports;
using PX.SM;
using PX.TM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web.Compilation;

namespace PX.Objects.CR
{
	#region ActivityContactFilter

	[Serializable]
	[PXHidden]
	[Obsolete]
	public partial class ActivityContactFilter : IBqlTable
	{
		#region ContactID

		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

		[PXInt]
		[PXUIField(DisplayName = "Select Contact")]
		[PXSelector(typeof(Search<Contact.contactID,
			Where<Contact.bAccountID, Equal<Current<BAccount.bAccountID>>,
			And<Contact.contactType, NotIn3<ContactTypesAttribute.bAccountProperty, ContactTypesAttribute.broker>>>>),
			DescriptionField = typeof(Contact.displayName),
			Filterable = true)]
		public virtual Int32? ContactID { get; set; }

		#endregion

		#region NoteID

		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXGuid]
		public virtual Guid? NoteID { get; set; }

		#endregion
	}

	#endregion

	#region PMTimeActivityList

	public class PMTimeActivityList<TMasterActivity> : PXSelectBase<TMasterActivity>
		where TMasterActivity : CRActivity, new()
	{
		#region Constants

		private static readonly EPSetup EmptyEpSetup = new EPSetup();

		private const string _DELETE_ACTION_NAME = "Delete";
		private const string _MARKASCOMPLETED_ACTION_NAME = "MarkAsCompleted";
		#endregion


		#region Ctor

		public PMTimeActivityList(PXGraph graph)
		{
			_Graph = graph;

			graph.RowSelected.AddHandler(typeof(PMTimeActivity), PMTimeActivity_RowSelected);
			graph.RowDeleting.AddHandler(typeof(PMTimeActivity), PMTimeActivity_RowDeleting);
			graph.RowInserted.AddHandler(typeof(PMTimeActivity), PMTimeActivity_RowInserted);
			graph.RowInserting.AddHandler(typeof(PMTimeActivity), PMTimeActivity_RowInserting);
			graph.RowPersisting.AddHandler(typeof(PMTimeActivity), PMTimeActivity_RowPersisting);
			graph.RowUpdated.AddHandler(typeof(PMTimeActivity), PMTimeActivity_RowUpdated);
			graph.RowPersisted.AddHandler(typeof(PMTimeActivity), PMTimeActivity_RowPersisted);

			graph.FieldUpdated.AddHandler(typeof(PMTimeActivity), typeof(PMTimeActivity.timeSpent).Name, PMTimeActivity_TimeSpent_FieldUpdated);
			graph.FieldUpdated.AddHandler(typeof(PMTimeActivity), typeof(PMTimeActivity.trackTime).Name, PMTimeActivity_TrackTime_FieldUpdated);
			graph.FieldUpdated.AddHandler(typeof(PMTimeActivity), typeof(PMTimeActivity.approvalStatus).Name, PMTimeActivity_ApprovalStatus_FieldUpdated);

			graph.RowInserted.AddHandler<TMasterActivity>(Master_RowInserted);
			graph.RowPersisting.AddHandler<TMasterActivity>(Master_RowPersisting);

			graph.FieldUpdated.AddHandler(typeof(TMasterActivity), typeof(CRActivity.type).Name, Master_Type_FieldUpdated);
			graph.FieldUpdated.AddHandler(typeof(TMasterActivity), typeof(CRActivity.ownerID).Name, Master_OwnerID_FieldUpdated);
			graph.FieldUpdated.AddHandler(typeof(TMasterActivity), typeof(CRActivity.startDate).Name, Master_StartDate_FieldUpdated);
			graph.FieldUpdated.AddHandler(typeof(TMasterActivity), typeof(CRActivity.subject).Name, Master_Subject_FieldUpdated);
			graph.FieldUpdated.AddHandler(typeof(TMasterActivity), typeof(CRActivity.parentNoteID).Name, Master_ParentNoteID_FieldUpdated);

			View = new PXView(graph, false, GenerateOriginalCommand());
			ApprovalStatusAttribute.SetRestictedMode<PMTimeActivity.approvalStatus>(View.Cache, true);
		}

		#endregion

		public static BqlCommand GenerateOriginalCommand()
		{
			var createdDateTimeField = typeof(PMTimeActivity).GetNestedType(typeof(PMTimeActivity.createdDateTime).Name);
			var noteIDField = typeof(TMasterActivity).GetNestedType(typeof(CRActivity.noteID).Name);

			return BqlCommand.CreateInstance(
				typeof(Select<,,>),
					typeof(PMTimeActivity),
				typeof(Where<,,>),
					typeof(PMTimeActivity.refNoteID), typeof(Equal<>), typeof(Current<>), noteIDField,
				typeof(And<PMTimeActivity.isCorrected, Equal<False>>),
				typeof(OrderBy<>),
					typeof(Desc<>), createdDateTimeField);
		}
		public virtual object SelectSingle(params object[] parameters)
		{
			using (new PXReadInsertedDeletedScope())
			{
				return View.Cache.Current = View.SelectSingle(parameters);
			}
		}

		#region Event Handlers

		protected virtual void Master_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			using (var s = new ReadOnlyScope(MainCache))
			{
				this.Current = (PMTimeActivity)MainCache.Insert();
				this.Current.ApprovalStatus = ActivityStatusListAttribute.Open;
			}
		}
		protected virtual void Master_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			var row = e.Row as TMasterActivity;
			PMTimeActivity timeActivity = (PMTimeActivity)Current;
			if (row == null || timeActivity == null) return;

			if (timeActivity.TrackTime != true && e.Operation != PXDBOperation.Delete)
			{
				if (row.ClassID != CRActivityClass.Email)
				{
					var status = row.ClassID != CRActivityClass.Event && row.ClassID != CRActivityClass.Task
						? ActivityStatusAttribute.Completed
						: row.UIStatus;

					var originalStatus = cache.GetValueOriginal(row, typeof(CRActivity.uistatus).Name) as string;
					if (status != originalStatus)
					{
						cache.SetValueExt(row, typeof(CRActivity.uistatus).Name, status);
						cache.RaiseRowUpdated(row, row);
					}
				}
			}
		}

		protected virtual void Master_Type_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as TMasterActivity;
			PMTimeActivity timeActivity = (PMTimeActivity)(Current ?? MainCache.Insert());
			if (row == null || timeActivity == null) return;

			timeActivity.TrackTime = (bool?)PXFormulaAttribute.Evaluate<PMTimeActivity.trackTime>(MainCache, timeActivity);

			MainCache.Update(timeActivity);
		}

		protected virtual void Master_OwnerID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as TMasterActivity;
			PMTimeActivity timeActivity = (PMTimeActivity)Current;
			if (row == null || timeActivity == null) return;

			MainCache.SetValue<PMTimeActivity.ownerID>(timeActivity, row.OwnerID);
		}

		protected virtual void Master_StartDate_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as TMasterActivity;
			PMTimeActivity timeActivity = (PMTimeActivity)Current;
			if (row == null || timeActivity == null) return;

			timeActivity.Date = (DateTime?)PXFormulaAttribute.Evaluate<PMTimeActivity.date>(MainCache, timeActivity);
			MainCache.SetDefaultExt<PMTimeActivity.weekID>(timeActivity);
			MainCache.Update(timeActivity);
		}

		protected virtual void Master_Subject_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			PMTimeActivity timeActivity = (PMTimeActivity)Current;
			if (timeActivity == null) return;

			MainCache.MarkUpdated(timeActivity);
		}

		protected virtual void Master_ParentNoteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as TMasterActivity;
			PMTimeActivity timeActivity = (PMTimeActivity)Current;
			if (row == null || timeActivity == null) return;

			var item = (PXResult<CRActivity, PMTimeActivity>)
				PXSelectJoin<CRActivity,
					InnerJoin<PMTimeActivity,
						On<PMTimeActivity.isCorrected, Equal<False>,
						And<CRActivity.noteID, Equal<PMTimeActivity.refNoteID>>>>,
					Where<CRActivity.noteID, Equal<Required<CRActivity.noteID>>>>
					.Select(_Graph, row.ParentNoteID);

			CRActivity parent = item;
			PMTimeActivity timeParent = item;
			if (timeParent != null)
			{
				timeActivity.ProjectID = timeParent.ProjectID;
				timeActivity.ProjectTaskID = timeParent.ProjectTaskID;
			}

			timeActivity.ParentTaskNoteID =
				parent != null && parent.ClassID == CRActivityClass.Task
					? parent.NoteID
					: null;

			MainCache.Update(timeActivity);
		}

		protected virtual void PMTimeActivity_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			PMTimeActivity row = (PMTimeActivity)e.Row;
			TMasterActivity masterAct = (TMasterActivity)MasterCache.Current;
			PXUIFieldAttribute.SetDisplayName<PMTimeActivity.approvalStatus>(cache, Data.EP.Messages.Status);

			if (row == null || masterAct == null) return;

			// TimeActivity
			bool wasUsed = !string.IsNullOrEmpty(row.TimeCardCD) || row.Billed == true;

			string origTimeStatus;

			if (masterAct.ClassID == CRActivityClass.Task || masterAct.ClassID == CRActivityClass.Event)
			{
				origTimeStatus =
					(string)MasterCache.GetValueOriginal<CRActivity.uistatus>(masterAct)
					?? ActivityStatusListAttribute.Open;
			}
			else
			{
				origTimeStatus =
					(string)cache.GetValueOriginal<PMTimeActivity.approvalStatus>(row)
					?? ActivityStatusListAttribute.Open;
			}

			if (origTimeStatus == ActivityStatusListAttribute.Open)
			{
				PXUIFieldAttribute.SetEnabled(cache, row, true);

				PXUIFieldAttribute.SetEnabled<PMTimeActivity.timeBillable>(cache, row, !wasUsed && row?.IsBillable == true);
				PXUIFieldAttribute.SetEnabled<PMTimeActivity.overtimeBillable>(cache, row, !wasUsed && row?.IsBillable == true);
				PXUIFieldAttribute.SetEnabled<PMTimeActivity.projectID>(cache, row, !wasUsed);
				PXUIFieldAttribute.SetEnabled<PMTimeActivity.projectTaskID>(cache, row, !wasUsed);
				PXUIFieldAttribute.SetEnabled<PMTimeActivity.trackTime>(cache, row, !wasUsed);
			}
			else
			{
				PXUIFieldAttribute.SetEnabled(cache, row, false);
			}

			PXUIFieldAttribute.SetEnabled<PMTimeActivity.approvalStatus>(cache, row, row.TrackTime == true && !wasUsed && row.Released != true);
			PXUIFieldAttribute.SetEnabled<PMTimeActivity.released>(cache, row, false);

			if (row.Released == true && row.ARRefNbr == null)
			{
				CRCase crCase = PXSelect<CRCase,
					Where<CRCase.noteID, Equal<Required<CRActivity.refNoteID>>>>.Select(_Graph, masterAct.RefNoteID);

				if (crCase != null && crCase.ARRefNbr != null)
				{
					ARInvoice invoice = (ARInvoice)PXSelectorAttribute.Select<CRCase.aRRefNbr>(_Graph.Caches<CRCase>(), crCase);
					row.ARRefNbr = invoice.RefNbr;
					row.ARDocType = invoice.DocType;
				}
				if (row.ARRefNbr == null)
				{
					PMTran pmTran = PXSelect<PMTran,
						Where<PMTran.origRefID, Equal<Required<CRActivity.noteID>>>>.Select(_Graph, masterAct.NoteID);

					if (pmTran != null)
					{
						row.ARDocType = pmTran.ARTranType;
						row.ARRefNbr = pmTran.ARRefNbr;
					}
				}
			}
		}

		protected virtual void PMTimeActivity_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			var row = (PMTimeActivity)e.Row;
			if (row == null) return;

			if (!string.IsNullOrEmpty(row.TimeCardCD) || row.Billed == true)
				throw new PXException(EP.Messages.ActivityIsBilled);
		}

		protected virtual void PMTimeActivity_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			var row = e.Row as PMTimeActivity;
			TMasterActivity activity = (TMasterActivity)MasterCache.Current;
			if (row == null || activity == null) return;

			row.RefNoteID = activity.NoteID;
			row.OwnerID = activity.OwnerID;
			cache.RaiseFieldUpdated<PMTimeActivity.approvalStatus>(row, null);
			cache.RaiseFieldUpdated<PMTimeActivity.ownerID>(row, null);
			if (activity.ParentNoteID != null)
			{
				var item = (PXResult<CRActivity, PMTimeActivity>)
					PXSelectJoin<CRActivity,
						InnerJoin<PMTimeActivity,
							On<PMTimeActivity.isCorrected, Equal<False>,
							And<CRActivity.noteID, Equal<PMTimeActivity.refNoteID>>>>,
						Where<CRActivity.noteID, Equal<Required<CRActivity.noteID>>>>
						.Select(_Graph, activity.ParentNoteID);

				CRActivity parent = item;
				PMTimeActivity timeParent = item;

				if (timeParent != null && timeParent.RefNoteID != null &&
					(timeParent.ProjectID != null || timeParent.ProjectTaskID != null) &&
					(row.ProjectID == null || ProjectDefaultAttribute.IsNonProject(row.ProjectID)))
				{
					row.ProjectID = timeParent.ProjectID;
					row.ProjectTaskID = timeParent.ProjectTaskID;
					row.CostCodeID = timeParent.CostCodeID;

					cache.RaiseFieldUpdated<PMTimeActivity.projectTaskID>(row, null);
				}

				row.ParentTaskNoteID =
					parent != null && parent.ClassID == CRActivityClass.Task
						? parent.NoteID
						: null;
			}
		}

		protected virtual void PMTimeActivity_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			var row = e.Row as PMTimeActivity;
			if (row == null) return;

			if ((cache.Inserted.GetEnumerator() is IEnumerator insEnum) && insEnum.MoveNext())
			{
				cache.SetStatus(insEnum.Current, PXEntryStatus.InsertedDeleted);
			}
			else if ((cache.Updated.GetEnumerator() is IEnumerator updEnum) && updEnum.MoveNext())
			{
				cache.SetStatus(updEnum.Current, PXEntryStatus.InsertedDeleted);
			}
			else if ((cache.Deleted.GetEnumerator() is IEnumerator delEnum) && delEnum.MoveNext())
			{
				row.NoteID = ((PMTimeActivity)delEnum.Current).NoteID;
			}
		}

		protected virtual void PMTimeActivity_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			var row = e.Row as PMTimeActivity;
			TMasterActivity activity = (TMasterActivity)MasterCache.Current;
			if (row == null || activity == null) return;

			cache.SetValue<PMTimeActivity.summary>(row, activity.Subject);

			if (row.NoteID == row.RefNoteID)
			{
				cache.SetValue<PMTimeActivity.noteID>(row, SequentialGuid.Generate());
				cache.Normalize();
			}

			if (activity.ClassID == CRActivityClass.Task || activity.ClassID == CRActivityClass.Event)
				cache.SetValue<PMTimeActivity.trackTime>(row, false);

			row.NeedToBeDeleted = (bool?)PXFormulaAttribute.Evaluate<PMTimeActivity.needToBeDeleted>(cache, row);

			if (row.NeedToBeDeleted == true && e.Operation != PXDBOperation.Delete)
			{
				e.Cancel = true;
			}
			else
			{
				if (row.TrackTime != true)
				{
					if (activity.UIStatus == ActivityStatusListAttribute.Completed)
						row.ApprovalStatus = ActivityStatusListAttribute.Completed;
					else if (activity.UIStatus == ActivityStatusListAttribute.Canceled)
						row.ApprovalStatus = ActivityStatusListAttribute.Canceled;
					else
						row.ApprovalStatus = ActivityStatusListAttribute.Open;
				}
				else
					if (row.ApprovalStatus == ActivityStatusListAttribute.Completed &&
						row.ApproverID != null)
					row.ApprovalStatus = ActivityStatusListAttribute.PendingApproval;
			}
		}

		protected virtual void PMTimeActivity_RowPersisted(PXCache cache, PXRowPersistedEventArgs e)
		{
			PMTimeActivity row = (PMTimeActivity)e.Row;
			if (row == null) return;

			if (e.TranStatus == PXTranStatus.Open && e.Operation == PXDBOperation.Delete)
			{
				// Acuminator disable once PX1043 SavingChangesInEventHandlers [Justification]
				PXDatabase.Update<PMTimeActivity>(
					new PXDataFieldAssign<PMTimeActivity.projectID>(PXDbType.Int, PM.ProjectDefaultAttribute.NonProject()),
					new PXDataFieldAssign<PMTimeActivity.projectTaskID>(PXDbType.Int, null),
					new PXDataFieldRestrict<PMTimeActivity.noteID>(PXDbType.UniqueIdentifier, row.NoteID));
			}
		}

		protected virtual void PMTimeActivity_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			var row = e.Row as PMTimeActivity;
			if (row == null || _Graph.IsContractBasedAPI) return;

			var isInDB = cache.GetValueOriginal<PMTimeActivity.noteID>(row) != null;

			row.NeedToBeDeleted = (bool?)PXFormulaAttribute.Evaluate<PMTimeActivity.needToBeDeleted>(cache, row);

			if (row.NeedToBeDeleted == true)
			{
				if (!isInDB)
					cache.SetStatus(row, PXEntryStatus.InsertedDeleted);
				else
					cache.SetStatus(row, PXEntryStatus.Deleted);

				this.StoreCached(new PXCommandKey(new object[] { row.RefNoteID }, null, null, null, 0, 1, null, false, null), new List<object> { row });
			}
			else if (cache.GetStatus(row) == PXEntryStatus.Updated && !isInDB)
			{
				// means "is not in DB", so move from updated to inserted
				cache.SetStatus(row, PXEntryStatus.Inserted);
			}
		}

		protected virtual void PMTimeActivity_ApprovalStatus_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as PMTimeActivity;
			TMasterActivity master = (TMasterActivity)MasterCache.Current;
			if (row == null || master == null) return;
			if (row.TrackTime == true && master.IsLocked != true && master.UIStatus != ActivityStatusListAttribute.Draft)
			{
				TMasterActivity activity = (TMasterActivity)MasterCache.CreateCopy(master);
				switch (row.ApprovalStatus)
				{
					case ActivityStatusListAttribute.Open:
						activity.UIStatus = ActivityStatusListAttribute.Open;
						break;
					case ActivityStatusListAttribute.Canceled:
						activity.UIStatus = ActivityStatusListAttribute.Canceled;
						break;
					default:
						activity.UIStatus = ActivityStatusListAttribute.Completed;
						break;
				}
				if (master.UIStatus != activity.UIStatus)
					MasterCache.Update(activity);
			}
		}
		protected virtual void PMTimeActivity_TimeSpent_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as PMTimeActivity;
			TMasterActivity activity = (TMasterActivity)MasterCache.Current;
			if (row == null || activity == null) return;

			if (activity.StartDate != null)
			{
				MasterCache.SetValue(activity, typeof(CRActivity.endDate).Name, row.TimeSpent != null
					? (DateTime?)((DateTime)activity.StartDate).AddMinutes((int)row.TimeSpent)
					: null);
			}
		}

		protected virtual void PMTimeActivity_TrackTime_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as PMTimeActivity;
			TMasterActivity activity = (TMasterActivity)MasterCache.Current;
			if (row == null || activity == null) return;

			if (row.TrackTime != true)
			{
				cache.SetValue<PMTimeActivity.timeSpent>(row, 0);
				cache.SetValue<PMTimeActivity.timeBillable>(row, 0);
				cache.SetValue<PMTimeActivity.overtimeSpent>(row, 0);
				cache.SetValue<PMTimeActivity.overtimeBillable>(row, 0);
				cache.SetValue<PMTimeActivity.approvalStatus>(row, ActivityStatusAttribute.Completed);
				MasterCache.SetValue(activity, typeof(CRActivity.uistatus).Name, ActivityStatusAttribute.Completed);
			}
		}

		#endregion

		private PXCache MainCache
		{
			get { return _Graph.Caches[typeof(PMTimeActivity)]; }
		}

		private PXCache MasterCache
		{
			get { return _Graph.Caches[typeof(TMasterActivity)]; }
		}

		public PMTimeActivity Current
		{
			get { return (PMTimeActivity)View.Cache.Current; }
			set { View.Cache.Current = value; }
		}

		private EPSetup EPSetupCurrent
		{
			get
			{
				var res = (EPSetup)PXSelect<EPSetup>.
					SelectWindowed(_Graph, 0, 1);
				return res ?? EmptyEpSetup;
			}
		}

	}

	#endregion

	#region CRReminderList

	public class CRReminderList<TMasterActivity> : PXSelectBase
		where TMasterActivity : CRActivity, new()
	{
		#region Ctor

		public CRReminderList(PXGraph graph)
		{
			_Graph = graph;

			View = new PXView(graph, false, GenerateOriginalCommand());

			graph.RowInserted.AddHandler<TMasterActivity>(Master_RowInserted);

			graph.FieldUpdated.AddHandler(typeof(TMasterActivity), typeof(CRActivity.ownerID).Name, Master_OwnerID_FieldUpdated);

			graph.RowSelected.AddHandler<CRReminder>(CRReminder_RowSelected);
			graph.RowInserting.AddHandler<CRReminder>(CRReminder_RowInserting);
			graph.RowInserted.AddHandler<CRReminder>(CRReminder_RowInserted);
			graph.RowPersisting.AddHandler<CRReminder>(CRReminder_RowPersisting);
			graph.RowUpdated.AddHandler<CRReminder>(CRReminder_RowUpdated);

			graph.FieldUpdated.AddHandler<CRReminder.isReminderOn>(CRReminder_IsReminderOn_FieldUpdated);

			PXUIFieldAttribute.SetEnabled<CRReminder.reminderDate>(MainCache, null, false);

			graph.EnsureCachePersistence(typeof(CRReminder));
		}

		public virtual object SelectSingle(params object[] parameters)
		{
			return View.SelectSingle(parameters);
		}

		#endregion

		protected virtual void Master_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			using (var r = new ReadOnlyScope(MainCache))
				this.Current = (CRReminder)MainCache.Insert();
		}

		protected virtual void Master_OwnerID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as TMasterActivity;
			CRReminder reminder = (CRReminder)Current;
			if (row == null || reminder == null) return;

			reminder.Owner = row.OwnerID;
			MainCache.Update(reminder);

			if (!cache.Graph.UnattendedMode && MainCache.GetStatus(reminder) != PXEntryStatus.Inserted)
			{
				var value = row.CreatedByID != PXAccess.GetUserID(row.OwnerID);

				MainCache.SetValueExt<CRReminder.isReminderOn>(reminder, value);
			}
		}

		protected virtual void CRReminder_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var row = e.Row as CRReminder;
			TMasterActivity activity = (TMasterActivity)MasterCache.Current;
			if (row == null || activity == null) return;

			if ((string)MasterCache.GetValueOriginal(activity, typeof(CRActivity.uistatus).Name) != ActivityStatusAttribute.Completed)
			{
				PXUIFieldAttribute.SetEnabled<CRReminder.reminderDate>(cache, row, row.IsReminderOn == true);
				PXUIFieldAttribute.SetVisible<CRReminder.remindAt>(cache, row, row.IsReminderOn == true);
				return;
			}

			PXUIFieldAttribute.SetEnabled(cache, row, false);
		}

		protected virtual void CRReminder_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			var row = e.Row as CRReminder;
			TMasterActivity activity = (TMasterActivity)MasterCache.Current;
			if (row == null || activity == null) return;

			var delEnum = cache.Deleted.GetEnumerator();
			if (delEnum.MoveNext())
			{
				row.NoteID = ((CRReminder)delEnum.Current).NoteID;
			}
		}

		protected virtual void CRReminder_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			var row = e.Row as CRReminder;
			TMasterActivity activity = (TMasterActivity)MasterCache.Current;
			if (row == null || activity == null) return;

			row.RefNoteID = activity.NoteID;
			row.Owner = activity.OwnerID;
		}

		protected virtual void CRReminder_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			var row = e.Row as CRReminder;
			if (row == null) return;



			if (row.IsReminderOn != true && e.Operation != PXDBOperation.Delete)
			{
				e.Cancel = true;
			}

			if (row.IsReminderOn == true && row.ReminderDate == null)
			{
				var reminderDateDisplayName = PXUIFieldAttribute.GetDisplayName<CRReminder.reminderDate>(MainCache);
				var exception = new PXSetPropertyException(ErrorMessages.FieldIsEmpty, reminderDateDisplayName);
				if (MainCache.RaiseExceptionHandling<CRReminder.reminderDate>(row, null, exception))
				{
					throw new PXRowPersistingException(typeof(CRReminder.reminderDate).Name, null, ErrorMessages.FieldIsEmpty, reminderDateDisplayName);
				}
			}
		}

		protected virtual void CRReminder_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			var row = e.Row as CRReminder;
			if (row == null) return;

			var isInDB = cache.GetValueOriginal<CRReminder.noteID>(row) != null;

			if (row.IsReminderOn != true)
			{
				if (!isInDB)
					cache.SetStatus(row, PXEntryStatus.InsertedDeleted);
				else
					cache.SetStatus(row, PXEntryStatus.Deleted);
			}
			else if (cache.GetStatus(row) == PXEntryStatus.Updated && !isInDB)
			{
				// means "is not in DB", so move from updated to inserted
				cache.SetStatus(row, PXEntryStatus.Inserted);
			}
		}

		protected virtual void CRReminder_IsReminderOn_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as CRReminder;
			TMasterActivity activity = (TMasterActivity)MasterCache.Current;
			if (row == null || activity == null) return;

			if (activity.ClassID == CRActivityClass.Task)
			{
				if (row.IsReminderOn == true)
				{
					cache.SetValue<CRReminder.reminderDate>(row,
							row.ReminderDate
								?? activity.StartDate?.AddMinutes(15)
								?? row.LastModifiedDateTime?.AddMinutes(15)
								?? PXTimeZoneInfo.Now.AddMinutes(15)
						);
				}
			}

			if (activity.ClassID == CRActivityClass.Event)
			{
				if (row.IsReminderOn == true)
				{
					cache.SetValue<CRReminder.reminderDate>(row, row.ReminderDate ?? activity.StartDate?.AddMinutes(-15) ?? row.LastModifiedDateTime?.AddMinutes(15));
				}
			}
		}

		public static BqlCommand GenerateOriginalCommand()
		{
			var createdDateTimeField = typeof(CRReminder).GetNestedType(typeof(CRReminder.createdDateTime).Name);
			var noteIDField = typeof(TMasterActivity).GetNestedType(typeof(CRActivity.noteID).Name);

			return BqlCommand.CreateInstance(
				typeof(Select<,,>),
					typeof(CRReminder),
				typeof(Where<,>),
					typeof(CRReminder.refNoteID), typeof(Equal<>), typeof(Current<>), noteIDField,
				typeof(OrderBy<>),
					typeof(Desc<>), createdDateTimeField);
		}

		private PXCache MainCache
		{
			get { return _Graph.Caches[typeof(CRReminder)]; }
		}

		private PXCache MasterCache
		{
			get { return _Graph.Caches[typeof(TMasterActivity)]; }
		}

		public CRReminder Current
		{
			get { return (CRReminder)View.Cache.Current; }
			set { View.Cache.Current = value; }
		}
	}

	#endregion

	#region CRNotificationSetupList

	public class CRNotificationSetupList<Table> : PXSelect<Table>
		where Table : NotificationSetup, new()
	{
		public CRNotificationSetupList(PXGraph graph)
			: base(graph)
		{
			graph.Views.Caches.Add(typeof(NotificationSource));
			graph.Views.Caches.Add(typeof(NotificationRecipient));
			graph.RowDeleted.AddHandler(typeof(Table), OnRowDeleted);
			graph.RowPersisting.AddHandler(typeof(Table), OnRowPersisting);
		}

		protected virtual void OnRowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			NotificationSetup row = (NotificationSetup)e.Row;

			PXCache source = cache.Graph.Caches[typeof(NotificationSource)];
			foreach (NotificationSource item in
				PXSelect<NotificationSource,
			Where<NotificationSource.setupID, Equal<Required<NotificationSource.setupID>>>>.Select(cache.Graph, row.SetupID))
			{
				source.Delete(item);
			}

			PXCache recipient = cache.Graph.Caches[typeof(NotificationRecipient)];
			foreach (NotificationRecipient item in
				PXSelect<NotificationRecipient,
			Where<NotificationRecipient.setupID, Equal<Required<NotificationRecipient.setupID>>>>.Select(cache.Graph, row.SetupID))
			{
				recipient.Delete(item);
			}
		}

		protected virtual void OnRowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			NotificationSetup row = (NotificationSetup)e.Row;
			if (row != null && row.NotificationCD == null)
			{
				cache.RaiseExceptionHandling<NotificationSetup.notificationCD>(e.Row, row.NotificationCD,
					new PXSetPropertyException(PXMessages.LocalizeFormatNoPrefix(Messages.EmptyValueErrorFormat, PXUIFieldAttribute.GetDisplayName<NotificationSetup.notificationCD>(cache)),
						PXErrorLevel.RowError, typeof(NotificationSetup.notificationCD).Name));
			}
		}

	}

	#endregion

	#region CRClassNotificationSourceList

	public class CRClassNotificationSourceList<Table, ClassID, SourceCD> : PXSelect<Table>
		where Table : NotificationSource, new()
		where ClassID : IBqlField
		where SourceCD : IConstant<string>, IBqlOperand
	{
		public CRClassNotificationSourceList(PXGraph graph)
			: base(graph)
		{
			this.View = new PXView(graph, false,
				BqlCommand.CreateInstance(
				BqlCommand.Compose(
				typeof(Select2<,,,>), typeof(Table),
				typeof(InnerJoin<,>), typeof(NotificationSetup),
				typeof(On<,,>), typeof(NotificationSetup.setupID), typeof(Equal<>), typeof(Table).GetNestedType(typeof(NotificationSource.setupID).Name),
				typeof(And<,>), typeof(NotificationSetup.sourceCD), typeof(Equal<>), typeof(SourceCD),
				typeof(Where<,>), typeof(Table).GetNestedType(typeof(NotificationSource.classID).Name), typeof(Equal<>), typeof(Optional<>), typeof(ClassID),
				typeof(OrderBy<>),
				typeof(Asc<>),
				typeof(NotificationSetup.notificationCD))));

			this.setupNotifications =
				new PXView(graph, false,
				BqlCommand.CreateInstance(
				BqlCommand.Compose(typeof(Select<,>), typeof(NotificationSetup),
				typeof(Where<,>), typeof(NotificationSetup.sourceCD), typeof(Equal<>), typeof(SourceCD))));

			graph.RowInserted.AddHandler(BqlCommand.GetItemType(typeof(ClassID)), OnClassRowInserted);
			graph.RowUpdated.AddHandler(BqlCommand.GetItemType(typeof(ClassID)), OnClassRowUpdated);
			graph.RowInserted.AddHandler<NotificationSource>(OnSourceRowInseted);
		}
		private PXView setupNotifications;
		protected virtual void OnClassRowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			if (e.Row == null || cache.GetValue(e.Row, typeof(ClassID).Name) == null) return;

			foreach (NotificationSetup n in setupNotifications.SelectMulti())
			{
				Table source = new Table();
				source.SetupID = n.SetupID;
				this.Cache.Insert(source);
			}
		}
		public virtual void OnClassRowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			if (cache.Graph.IsCopyPasteContext)
			{
				foreach (object source in this.Select())
				{
					this.Cache.Delete(source);
				}
			}
		}
		protected virtual void OnSourceRowInseted(PXCache cache, PXRowInsertedEventArgs e)
		{
			if (cache.Graph.IsCopyPasteContext)
				return;
			NotificationSource source = (NotificationSource)e.Row;
			PXCache rCache = cache.Graph.Caches[typeof(NotificationRecipient)];
			foreach (NotificationSetupRecipient setupRecipient in
					PXSelect<NotificationSetupRecipient,
					Where<NotificationSetupRecipient.setupID, Equal<Required<NotificationSetupRecipient.setupID>>>>
					.Select(cache.Graph, source.SetupID))
			{
				try
				{
					NotificationRecipient rec = (NotificationRecipient)rCache.CreateInstance();
					rec.SetupID = source.SetupID;
					rec.ContactType = setupRecipient.ContactType;
					rec.ContactID = setupRecipient.ContactID;
					rec.Active = setupRecipient.Active;
					rec.AddTo = setupRecipient.AddTo;
					rCache.Insert(rec);
				}
				catch (Exception ex)
				{
					PXTrace.WriteError(ex);
				}
			}
		}
	}

	#endregion

	#region CRNotificationSourceList

	public class CRNotificationSourceList<Source, SourceClass, NotificationType> : EPDependNoteList<NotificationSource, NotificationSource.refNoteID, Source>
		where Source : class, IBqlTable
		where SourceClass : class, IBqlField
		where NotificationType : class, IBqlOperand
	{
		protected readonly PXView _SourceView;
		protected readonly PXView _ClassView;

		public CRNotificationSourceList(PXGraph graph)
			: base(graph)
		{
			this.View = new PXView(graph, false,
				BqlCommand.CreateInstance(
				BqlCommand.Compose(
				typeof(Select<,,>), typeof(NotificationSource),
				typeof(Where<boolTrue, Equal<boolTrue>>),
				typeof(OrderBy<>),
				typeof(Asc<>),
				typeof(NotificationSource).GetNestedType(typeof(NotificationSource.setupID_description).Name))),
				new PXSelectDelegate(NotificationSources));

			_SourceView = new PXView(graph, false,
				BqlCommand.CreateInstance(
				BqlCommand.Compose(typeof(Select<,>), typeof(NotificationSource), ComposeWhere)));



			_ClassView = new PXView(graph, true,
									BqlCommand.CreateInstance(BqlCommand.Compose(
										typeof(Select2<,,>),
										typeof(NotificationSource),
										typeof(InnerJoin<NotificationSetup, On<NotificationSetup.setupID, Equal<NotificationSource.setupID>>>),
										typeof(Where<,,>), typeof(NotificationSetup.sourceCD), typeof(Equal<>), typeof(NotificationType),
																typeof(And<,>), typeof(NotificationSource.classID), typeof(Equal<>), typeof(Optional<>), typeof(SourceClass))));

			graph.RowPersisting.AddHandler<NotificationSource>(OnRowPersisting);
			graph.RowDeleting.AddHandler<NotificationSource>(OnRowDeleting);
			graph.RowSelected.AddHandler<NotificationSource>(OnRowSelected);

		}
		protected virtual IEnumerable NotificationSources()
		{
			List<NotificationSource> result = new List<NotificationSource>();
			foreach (NotificationSource item in _SourceView.SelectMulti())
			{
				result.Add(item);
			}
			foreach (NotificationSource classItem in GetClassItems())
			{
				if (result.Find(i => i.SetupID == classItem.SetupID && i.NBranchID == classItem.NBranchID) == null)
					result.Add(classItem);
			}
			return result;
		}

		protected virtual void OnRowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			NotificationSource row = (NotificationSource)e.Row;
			foreach (NotificationSource classItem in GetClassItems())
				if (classItem.SetupID == row.SetupID && classItem.NBranchID == row.NBranchID && row.OverrideSource == false)
				{
					e.Cancel = true;
					throw new PXRowPersistingException(typeof(NotificationSource).Name, null, Messages.DeleteClassNotification);
				}
			if (!e.Cancel)
				this.View.RequestRefresh();
		}
		protected virtual void OnRowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			NotificationSource row = (NotificationSource)e.Row;
			if (row.ClassID != null)
			{
				if (e.Operation == PXDBOperation.Delete)
					e.Cancel = true;

				if (e.Operation == PXDBOperation.Update)
				{
					sender.SetStatus(row, PXEntryStatus.Deleted);
					NotificationSource ins = (NotificationSource)sender.CreateInstance();
					ins.SetupID = row.SetupID;
					ins.NBranchID = row.NBranchID;
					ins = (NotificationSource)sender.Insert(ins);

					NotificationSource clone = PXCache<NotificationSource>.CreateCopy(row);
					clone.NBranchID = ins.NBranchID;
					clone.SourceID = ins.SourceID;
					clone.RefNoteID = ins.RefNoteID;
					clone.ClassID = null;
					clone = (NotificationSource)sender.Update(clone);
					if (clone != null)
					{
						sender.PersistInserted(clone);
						sender.Normalize();
						sender.SetStatus(clone, PXEntryStatus.Notchanged);
						PXCache source = sender.Graph.Caches[BqlCommand.GetItemType(SourceNoteID)];
						Guid? refNoteID = (Guid?)source.GetValue(source.Current, SourceNoteID.Name);
						if (refNoteID != null)
						{
							foreach (NotificationRecipient r in PXSelect<NotificationRecipient,
							Where<NotificationRecipient.sourceID, Equal<Required<NotificationRecipient.sourceID>>,
							  And<NotificationRecipient.refNoteID, Equal<Required<NotificationRecipient.refNoteID>>,
								And<NotificationRecipient.classID, IsNotNull>>>>
							.Select(sender.Graph, row.SourceID, refNoteID))
							{
								PXCache cache = sender.Graph.Caches[typeof(NotificationRecipient)];
								if (cache.GetStatus(r) == PXEntryStatus.Inserted)
								{
									NotificationRecipient u1 = (NotificationRecipient)cache.CreateCopy(r);
									u1.SourceID = clone.SourceID;
									cache.Update(u1);
									cache.PersistInserted(u1);
								}

								if (cache.GetStatus(r) == PXEntryStatus.Updated ||
									cache.GetStatus(r) == PXEntryStatus.Inserted) continue;
								NotificationRecipient u = (NotificationRecipient)cache.CreateCopy(r);
								u.SourceID = clone.SourceID;
								u.ClassID = null;
								cache.Update(u);
							}
						}
					}
					e.Cancel = true;
				}
			}
		}
		public virtual void OnRowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;
			NotificationSource row = (NotificationSource)e.Row;
			bool classitem = GetClassItems().Any(cs => cs.SourceID == row.SourceID);
			PXUIFieldAttribute.SetEnabled<NotificationSource.overrideSource>(sender, row, !classitem);
			PXUIFieldAttribute.SetEnabled<NotificationSource.setupID>(sender, row, !classitem);
		}

		private IEnumerable<NotificationSource> GetClassItems()
		{
			foreach (object rec in _ClassView.SelectMulti())
			{
				NotificationSource classItem =
					(rec is PXResult && ((PXResult)rec)[0] is NotificationSource) ? (NotificationSource)((PXResult)rec)[0] :
					(rec is NotificationSource ? (NotificationSource)rec : null);
				if (classItem == null) continue;
				yield return classItem;
			}
		}
	}

	#endregion

	#region CRNotificationRecipientList

	public class CRNotificationRecipientList<Source, SourceClassID> : EPDependNoteList<NotificationRecipient, NotificationRecipient.refNoteID, Source>
		where Source : class, IBqlTable
		where SourceClassID : class, IBqlField
	{
		protected readonly PXView _SourceView;
		protected readonly PXView _ClassView;

		public CRNotificationRecipientList(PXGraph graph)
			: base(graph)
		{
			Type table = typeof(NotificationRecipient);
			Type where = BqlCommand.Compose(
				typeof(Where<,,>), typeof(NotificationRecipient.sourceID), typeof(Equal<Optional<NotificationSource.sourceID>>),
				typeof(And<,>), typeof(NotificationRecipient.refNoteID), typeof(Equal<>), typeof(Current<>), this.SourceNoteID);

			this.View = new PXView(graph, false,
				BqlCommand.CreateInstance(
				BqlCommand.Compose(
				typeof(Select<,,>), table,
				typeof(Where<boolTrue, Equal<boolTrue>>),
				typeof(OrderBy<>),
				typeof(Asc<>),
				table.GetNestedType(typeof(NotificationRecipient.orderID).Name))),
				new PXSelectDelegate(NotificationRecipients));

			_SourceView = new PXView(graph, false,
				BqlCommand.CreateInstance(
				BqlCommand.Compose(typeof(Select<,>), typeof(NotificationRecipient), where)));



			_ClassView = new PXView(graph, true, BqlTemplate.OfCommand<
					SelectFrom<NotificationRecipient>
					.Where<
						NotificationRecipient.classID.IsEqual<BqlPlaceholder.A.AsField.FromCurrent>
						.And<NotificationRecipient.setupID.IsEqual<NotificationSource.setupID.FromCurrent>>
						.And<NotificationRecipient.refNoteID.IsNull>>>
				.Replace<BqlPlaceholder.A>(typeof(SourceClassID))
				.ToCommand());

			graph.RowPersisting.AddHandler<NotificationRecipient>(OnRowPersisting);
			graph.RowSelected.AddHandler<NotificationRecipient>(OnRowSeleted);
			graph.RowDeleting.AddHandler<NotificationRecipient>(OnRowDeleting);
			graph.RowInserting.AddHandler<NotificationRecipient>(OnRowInserting);
		}


		protected virtual IEnumerable NotificationRecipients()
		{
			var result = new List<NotificationRecipient>();
			foreach (NotificationRecipient item in _SourceView.SelectMulti())
			{
				item.OrderID = item.NotificationID;
				result.Add(item);
			}

			foreach (NotificationRecipient classItem in GetClassItems())
			{
				NotificationRecipient item = result.Find(i =>
					i.ContactType == classItem.ContactType &&
					i.ContactID == classItem.ContactID);
				if (item == null)
				{
					item = classItem;
					result.Add(item);
				}
				item.OrderID = int.MinValue + classItem.NotificationID;
			}
			return result;
		}

		protected override void Source_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			sender.Current = e.Row;
			internalDelete = true;
			try
			{
				foreach (NotificationRecipient item in _SourceView.SelectMulti())
				{
					this._SourceView.Cache.Delete(item);
				}
			}
			finally
			{
				internalDelete = false;
			}
		}
		private bool internalDelete;

		protected virtual void OnRowSeleted(PXCache sender, PXRowSelectedEventArgs e)
		{
			NotificationRecipient row = (NotificationRecipient)e.Row;
			if (row == null) return;
			bool updatableContractID =
				(row.ContactType == NotificationContactType.Contact ||
			   row.ContactType == NotificationContactType.Employee);
			bool updatableContactType = !GetClassItems().Any(classItem => row.ContactType == classItem.ContactType &&
																		  row.ContactID == classItem.ContactID);

			PXUIFieldAttribute.SetEnabled(sender, row, typeof(NotificationRecipient.contactID).Name, updatableContactType && updatableContractID);
			PXUIFieldAttribute.SetEnabled(sender, row, typeof(NotificationRecipient.contactType).Name, updatableContactType);
		}
		protected virtual void OnRowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			NotificationRecipient row = (NotificationRecipient)e.Row;

			if (e.Operation == PXDBOperation.Insert)
			{
				NotificationSource source =
						PXSelectReadonly<NotificationSource,
						Where<NotificationSource.setupID, Equal<Required<NotificationSource.setupID>>,
							And<NotificationSource.refNoteID, Equal<Required<NotificationSource.refNoteID>>>>>.
							Select(sender.Graph, row.SetupID, row.RefNoteID);
				if (source != null)
				{
					if (_Graph.IsImport is false
						&& sender.Graph.Caches[typeof(NotificationSource)].GetStatus(source) == PXEntryStatus.Updated)
					{
						e.Cancel = true;
					}
				}
			}


			if (row.RefNoteID == null)
			{
				if (e.Operation == PXDBOperation.Update)
				{
					sender.Remove(row);
					NotificationRecipient ins = (NotificationRecipient)sender.Insert();
					NotificationRecipient clone = PXCache<NotificationRecipient>.CreateCopy(row);
					clone.NotificationID = ins.NotificationID;
					clone.RefNoteID = ins.RefNoteID;
					clone.ClassID = null;
					NotificationSource source =
						PXSelectReadonly<NotificationSource,
						Where<NotificationSource.setupID, Equal<Required<NotificationSource.setupID>>,
							And<NotificationSource.refNoteID, Equal<Required<NotificationSource.refNoteID>>>>>.
							Select(sender.Graph, row.SetupID, row.RefNoteID);
					if (source != null)
						clone.SourceID = source.SourceID;

					clone = (NotificationRecipient)sender.Update(clone);
					if (clone != null)
					{
						sender.PersistInserted(clone);
						sender.Normalize();
						sender.SetStatus(clone, PXEntryStatus.Notchanged);
					}
					e.Cancel = true;
				}
			}
			else
			{
				var sourceCache = sender.Graph.Caches<NotificationSource>();
				NotificationSource source = SelectFrom<NotificationSource>
								.Where<NotificationSource.sourceID.IsEqual<@P.AsInt>
									.And<NotificationSource.refNoteID.IsNull>>
								.View
								.SelectSingleBound(sender.Graph, null, row.SourceID);
				// changed base cases marked as deleted (but prevented from deletion ?\(�_o)/? )
				// and new related to current document are created
				// if mark as updated - it will have override
				if (sourceCache.GetStatus(source).IsIn(PXEntryStatus.Notchanged, PXEntryStatus.Held))
				{
					sourceCache.SetStatus(source, PXEntryStatus.Updated);
				}
			}
		}
		protected virtual void OnRowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			if (internalDelete) return;
			NotificationRecipient row = (NotificationRecipient)e.Row;
			foreach (NotificationRecipient classItem in GetClassItems())
				if (classItem.SetupID == row.SetupID &&
					  classItem.ContactType == row.ContactType &&
						classItem.ContactID == row.ContactID)
				{
					if (row.RefNoteID == null)
					{
						e.Cancel = true;
						throw new PXRowPersistingException(typeof(NotificationRecipient).Name, null, Messages.DeleteClassNotification);
					}
				}
			if (!e.Cancel)
				this.View.RequestRefresh();
		}

		protected virtual void OnRowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			if (e.Row != null)
			{
				NotificationRecipient r = (NotificationRecipient)e.Row;
				NotificationSource source = (NotificationSource)sender.Graph.Caches[typeof(NotificationSource)].Current;
				r.ClassID = source != null ? source.ClassID : null;
			}
		}

		private IEnumerable<NotificationRecipient> GetClassItems()
		{
			foreach (object rec in _ClassView.SelectMulti())
			{
				NotificationRecipient classItem =
					(rec is PXResult && ((PXResult)rec)[0] is NotificationRecipient) ? (NotificationRecipient)((PXResult)rec)[0] :
					(rec is NotificationRecipient ? (NotificationRecipient)rec : null);
				if (classItem == null) continue;
				yield return classItem;
			}
		}
	}

	#endregion

	#region CRSubscriptionsSelect

	public sealed class CRSubscriptionsSelect
	{
		public static IEnumerable Select(PXGraph graph, int? mailListID)
		{
			var startRow = PXView.StartRow;
			int totalRows = 0;

			var list = Select(graph, mailListID, PXView.Searches, PXView.SortColumns, PXView.Descendings,
								   ref startRow, PXView.MaximumRows, ref totalRows);

			PXView.StartRow = 0;
			return list;
		}

		public static IEnumerable Select(PXGraph graph, int? mailListID, object[] searches, string[] sortColumns, bool[] descendings, ref int startRow, int maxRows, ref int totalRows)
		{
			CRMarketingList list;
			if (mailListID == null ||
				(list = (CRMarketingList)PXSelect<CRMarketingList>.Search<CRMarketingList.marketingListID>(graph, mailListID)) == null)
			{
				return new PXResultset<Contact, BAccount, BAccountParent, Address, State>();
			}

			BqlCommand command = new Select2<Contact,
				LeftJoin<CRMarketingListMember,
					On<CRMarketingListMember.contactID, Equal<Contact.contactID>,
					And<CRMarketingListMember.marketingListID, Equal<Current<CRMarketingList.marketingListID>>>>,
				LeftJoin<BAccount,
					On<BAccount.bAccountID, Equal<Contact.bAccountID>>,
				LeftJoin<BAccountParent,
					On<BAccountParent.bAccountID, Equal<Contact.parentBAccountID>>,
				LeftJoin<Address,
					On<Address.addressID, Equal<Contact.defAddressID>>,
				LeftJoin<State,
					On<State.countryID, Equal<Address.countryID>,
						And<State.stateID, Equal<Address.state>>>,
				LeftJoin<GL.Branch,
					On<GL.Branch.bAccountID, Equal<Contact.bAccountID>,
					And<Contact.contactType, Equal<ContactTypesAttribute.bAccountProperty>>>,
				LeftJoin<CRLead,
					On<CRLead.contactID.IsEqual<Contact.contactID>>>>>>>>>,
				Where<
					GL.Branch.branchID, IsNull>>();

			var view = new PXView(graph, true, command);

			var sorts = new List<string>()
			{
				nameof(CRMarketingListMember) + "__" + nameof(CRMarketingListMember.IsSubscribed),
				nameof(Contact.MemberName),
				nameof(Contact.ContactID)
			};

			var descs = new List<bool>()
			{
				false,
				false,
				false
			};

			var search = new List<object>()
			{
				null,
				null,
				null
			};
			search.AddRange(searches);

			return view.Select(null, null, search.ToArray(), sorts.ToArray(), descs.ToArray(), PXView.Filters, ref startRow, maxRows, ref totalRows);
		}

			}

	#endregion

	#region CRCaseActivityHelper<TableRefNoteID>

	public class CRCaseActivityHelper
	{
		#region Ctor

		public static CRCaseActivityHelper Attach(PXGraph graph)
		{
			var res = new CRCaseActivityHelper();

			graph.RowInserted.AddHandler<PMTimeActivity>(res.ActivityRowInserted);
			graph.RowSelected.AddHandler<PMTimeActivity>(res.ActivityRowSelected);
			graph.RowUpdated.AddHandler<PMTimeActivity>(res.ActivityRowUpdated);

			return res;
		}
		#endregion

		#region Event Handlers

		protected virtual void ActivityRowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			var item = e.Row as PMTimeActivity;
			if (item == null) return;

			var graph = sender.Graph;
			var @case = GetCase(graph, item.RefNoteID);
			if (@case != null && @case.Released == true) item.IsBillable = false;
		}

		protected virtual void ActivityRowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var item = e.Row as PMTimeActivity;
			var oldItem = e.OldRow as PMTimeActivity;
			if (item == null || oldItem == null) return;

			var graph = sender.Graph;
			var @case = GetCase(graph, item.RefNoteID);
			if (@case != null && @case.Released == true) item.IsBillable = false;
		}

		protected virtual void ActivityRowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var item = e.Row as PMTimeActivity;
			if (item == null || !string.IsNullOrEmpty(item.TimeCardCD)) return;

			var graph = sender.Graph;
			var @case = GetCase(graph, item.RefNoteID);
			if (@case != null && @case.Released == true)
				PXUIFieldAttribute.SetEnabled<PMTimeActivity.isBillable>(sender, item, false);
		}

		#endregion

		#region Private Methods

		private CRCase GetCase(PXGraph graph, object refNoteID)
		{
			if (refNoteID == null) return null;
			return (CRCase)PXSelectJoin<CRCase,
				InnerJoin<CRActivityLink,
					On<CRActivityLink.refNoteID, Equal<CRCase.noteID>>>,
				Where<CRActivityLink.noteID, Equal<Required<PMTimeActivity.refNoteID>>>>.
				SelectWindowed(graph, 0, 1, refNoteID);
		}

		#endregion
	}

	#endregion

	#region CRAttribute

	public class CRAttribute
	{
		public class Attribute
		{
			public readonly string ID;
			public readonly string Description;
			public readonly int? ControlType;
			public readonly string EntryMask;
			public readonly string RegExp;
			public readonly string List;
			public readonly bool IsInternal;

			public Attribute(PXDataRecord record)
			{
				ID = record.GetString(0);
				Description = record.GetString(1);
				ControlType = record.GetInt32(2);
				EntryMask = record.GetString(3);
				RegExp = record.GetString(4);
				List = record.GetString(5);
				IsInternal = record.GetBoolean(6) == true;
				Values = new List<AttributeValue>();
			}

			protected Attribute(Attribute clone)
			{
				this.ID = clone.ID;
				this.Description = clone.Description;
				this.ControlType = clone.ControlType;
				this.EntryMask = clone.EntryMask;
				this.RegExp = clone.RegExp;
				this.List = clone.List;
				this.IsInternal = clone.IsInternal;
				this.Values = clone.Values;
			}

			public void AddValue(AttributeValue value)
			{
				Values.Add(value);
			}

			public readonly List<AttributeValue> Values;
		}

		public class AttributeValue
		{
			public readonly string ValueID;
			public readonly string Description;
			public readonly bool Disabled;

			public AttributeValue(PXDataRecord record)
			{
				ValueID = record.GetString(1);
				Description = record.GetString(2);
				Disabled = record.GetBoolean(3) == true;
			}
		}

		[DebuggerDisplay("ID={ID} Description={Description} Required={Required} IsActive={IsActive}")]
		public class AttributeExt : Attribute
		{
			public readonly string DefaultValue;
			public readonly bool Required;
			public readonly bool IsActive;
			public readonly string AttributeCategory;
			public bool NotInClass;

			public AttributeExt(Attribute attr, string defaultValue, bool required, bool isActive)
				: this(attr, defaultValue, required, isActive, null)
			{
			}

			public AttributeExt(Attribute attr, string defaultValue, bool required, bool isActive, string attributeCategory)
				: base(attr)
			{
				DefaultValue = defaultValue;
				Required = required;
				IsActive = isActive;
				AttributeCategory = attributeCategory;
			}
		}

		public class AttributeList : DList<string, Attribute>
		{
			private readonly bool useDescriptionAsKey;
			public AttributeList(bool useDescriptionAsKey = false)
				: base(StringComparer.InvariantCultureIgnoreCase, DefaultCapacity, DefaultDictionaryCreationThreshold, true)
			{
				this.useDescriptionAsKey = useDescriptionAsKey;
			}
			protected override string GetKeyForItem(Attribute item)
			{
				return useDescriptionAsKey && !item.Description.IsNullOrEmpty() ? item.Description : item.ID;
			}

			public override Attribute this[string key]
			{
				get
				{
					Attribute e;
					return TryGetValue(key, out e) ? e : null;

				}
			}
		}

		public class ClassAttributeList : DList<string, AttributeExt>
		{
			public ClassAttributeList()
				: base(StringComparer.InvariantCultureIgnoreCase, DefaultCapacity, DefaultDictionaryCreationThreshold, true)
			{

			}
			protected override string GetKeyForItem(AttributeExt item)
			{
				return item.ID;
			}
			public override AttributeExt this[string key]
			{
				get
				{
					AttributeExt e;
					return TryGetValue(key, out e) ? e : null;
				}
			}
		}

		private class Definition : IPrefetchable
		{
			public readonly AttributeList Attributes;
			public readonly AttributeList AttributesByDescr;
			public readonly Dictionary<string, AttributeList> EntityAttributes;
			public readonly Dictionary<string, Dictionary<string, ClassAttributeList>> ClassAttributes;

			public Definition()
			{
				Attributes = new AttributeList();
				AttributesByDescr = new AttributeList(true);
				ClassAttributes = new Dictionary<string, Dictionary<string, ClassAttributeList>>(StringComparer.InvariantCultureIgnoreCase);
				EntityAttributes = new Dictionary<string, AttributeList>(StringComparer.InvariantCultureIgnoreCase);
			}
			public void Prefetch()
			{
				using (new PXConnectionScope())
				{
					Attributes.Clear();
					AttributesByDescr.Clear();
					foreach (PXDataRecord record in PXDatabase.SelectMulti<CSAttribute>(
						new PXDataField(typeof(CSAttribute.attributeID).Name),
						PXDBLocalizableStringAttribute.GetValueSelect(typeof(CSAttribute).Name, typeof(CSAttribute.description).Name, false),
						new PXDataField(typeof(CSAttribute.controlType).Name),
						new PXDataField(typeof(CSAttribute.entryMask).Name),
						new PXDataField(typeof(CSAttribute.regExp).Name),
						PXDBLocalizableStringAttribute.GetValueSelect(typeof(CSAttribute).Name, typeof(CSAttribute.list).Name, true),
						new PXDataField(typeof(CSAttribute.isInternal).Name)
						))
					{
						Attribute attr = new Attribute(record);
						Attributes.Add(attr);
						AttributesByDescr.Add(attr);
					}

					foreach (PXDataRecord record in PXDatabase.SelectMulti<CSAttributeDetail>(
						new PXDataField(typeof(CSAttributeDetail.attributeID).Name),
						new PXDataField(typeof(CSAttributeDetail.valueID).Name),
						PXDBLocalizableStringAttribute.GetValueSelect(typeof(CSAttributeDetail).Name, typeof(CSAttributeDetail.description).Name, false),
						new PXDataField(typeof(CSAttributeDetail.disabled).Name),
						new PXDataFieldOrder(typeof(CSAttributeDetail.attributeID).Name),
						new PXDataFieldOrder(typeof(CSAttributeDetail.sortOrder).Name)
						))
					{
						string id = record.GetString(0);
						Attribute attr;
						if (Attributes.TryGetValue(id, out attr))
						{
							attr.AddValue(new AttributeValue(record));
						}
					}

					foreach (PXDataRecord record in PXDatabase.SelectMulti<CSAttributeGroup>(
					   new PXDataField(typeof(CSAttributeGroup.entityType).Name),
					   new PXDataField(typeof(CSAttributeGroup.entityClassID).Name),
					   new PXDataField(typeof(CSAttributeGroup.attributeID).Name),
					   new PXDataField(typeof(CSAttributeGroup.defaultValue).Name),
					   new PXDataField(typeof(CSAttributeGroup.required).Name),
					   new PXDataField(typeof(CSAttributeGroup.isActive).Name),
					   new PXDataField(typeof(CSAttributeGroup.attributeCategory).Name),
					   new PXDataFieldOrder(typeof(CSAttributeGroup.entityType).Name),
					   new PXDataFieldOrder(typeof(CSAttributeGroup.entityClassID).Name),
					   new PXDataFieldOrder(typeof(CSAttributeGroup.sortOrder).Name),
					   new PXDataFieldOrder(typeof(CSAttributeGroup.attributeID).Name)))
					{
						string type = record.GetString(0);
						string classID = record.GetString(1);
						string id = record.GetString(2);

						Dictionary<string, ClassAttributeList> dict;
						AttributeList list;

						if (!EntityAttributes.TryGetValue(type, out list))
							EntityAttributes[type] = list = new AttributeList();

						if (!ClassAttributes.TryGetValue(type, out dict))
							ClassAttributes[type] = dict = new Dictionary<string, ClassAttributeList>(StringComparer.InvariantCultureIgnoreCase);

						ClassAttributeList group;
						if (!dict.TryGetValue(classID, out group))
							dict[classID] = group = new ClassAttributeList();

						Attribute attr;
						if (Attributes.TryGetValue(id, out attr))
						{
							list.Add(attr);
							group.Add(new AttributeExt(attr, record.GetString(3), record.GetBoolean(4) ?? false, record.GetBoolean(5) ?? true, record.GetString(6)));
						}
					}
				}
			}
		}

		private static Definition Definitions
		{
			get
			{
				Definition defs = PX.Common.PXContext.GetSlot<Definition>();
				if (defs == null)
				{
					string currentLanguage = System.Threading.Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
					if (!PXDBLocalizableStringAttribute.IsEnabled)
					{
						currentLanguage = "";
					}
					defs = PX.Common.PXContext.SetSlot<Definition>(PXDatabase.GetSlot<Definition>("CSAttributes" + currentLanguage, typeof(CSAttribute), typeof(CSAttributeDetail), typeof(CSAttributeGroup)));
				}
				return defs;
			}
		}

		public static AttributeList Attributes
		{
			get
			{
				return Definitions.Attributes;
			}
		}
		public static AttributeList AttributesByDescr
		{
			get
			{
				return Definitions.AttributesByDescr;
			}
		}

		public static AttributeList EntityAttributes(string type)
		{
			AttributeList list;
			return Definitions.EntityAttributes.TryGetValue(type, out list) ? list : new AttributeList();
		}

		private static ClassAttributeList EntityAttributes(string type, string classID)
		{
			Dictionary<string, ClassAttributeList> typeList;
			ClassAttributeList list;
			if (type != null && classID != null &&
				Definitions.ClassAttributes.TryGetValue(type, out typeList) &&
				typeList.TryGetValue(classID, out list))
				return list;

			return new ClassAttributeList();
		}

		public static ClassAttributeList EntityAttributes(Type entityType, string classID)
		{
			return EntityAttributes(entityType.FullName, classID);
		}
	}

	#endregion

	#region CSAttributeGroupList

	public class CSAttributeGroupList<TClass, TEntity> : PXSelectBase<CSAttributeGroup>
		where TClass : class
	{
		private readonly string _classIdFieldName;
		private readonly Type _class;

		[InjectDependency]
		protected IScreenInfoCacheControl ScreenInfoCacheControl { get; set; }

		public CSAttributeGroupList(PXGraph graph)
		{
			_Graph = graph;

			var command = new Select3<CSAttributeGroup,
				InnerJoin<CSAttribute, On<CSAttributeGroup.attributeID, Equal<CSAttribute.attributeID>>>,
				OrderBy<Asc<CSAttributeGroup.entityClassID,
					Asc<CSAttributeGroup.entityType, Asc<CSAttributeGroup.sortOrder>>>>>();

			View = new PXView(graph, false, command, new PXSelectDelegate(SelectDelegate));

			if (typeof(IBqlTable).IsAssignableFrom(typeof(TClass)))
			{
				_class = typeof(TClass);
				_classIdFieldName = _Graph.Caches[_class].BqlKeys.Single().Name;
			}
			else if (typeof(IBqlField).IsAssignableFrom(typeof(TClass)))
			{
				_classIdFieldName = typeof(TClass).Name;
				_class = typeof(TClass).DeclaringType;
			}
			else
			{
				throw new PXArgumentException(typeof(TClass).Name);
			}

			_Graph.FieldDefaulting.AddHandler<CSAttributeGroup.entityType>((sender, e) =>
			{
				if (e.Row == null)
					return;
				e.NewValue = typeof(TEntity).FullName;
			});
			_Graph.FieldDefaulting.AddHandler<CSAttributeGroup.entityClassID>((sender, e) =>
			{
				if (e.Row == null)
					return;
				var entityClassCache = _Graph.Caches[_class];
				e.NewValue = entityClassCache.GetValue(entityClassCache.Current, _classIdFieldName)?.ToString();
			});
			_Graph.RowDeleted.AddHandler(_class, (sender, e) =>
			{
				foreach (PXResult<CSAttributeGroup> rec in SelectDelegate())
					this.Cache.Delete((CSAttributeGroup)rec);
			});
			_Graph.RowPersisted.AddHandler<CSAttributeGroup>((sender, e) =>
			{
				if (e.TranStatus == PXTranStatus.Completed
					&& PXPrimaryGraphAttribute.FindPrimaryGraph(sender.Graph.Caches[typeof(TEntity)], out Type graphType) != null)
				{
					foreach (string screenId in PXSiteMap.Provider.GetScreenIdsByGraphType(graphType))
					{
						ScreenInfoCacheControl.InvalidateCache(screenId);
					}
				}
			});


			_Graph.FieldSelecting.AddHandler<CSAttributeGroup.defaultValue>(CSAttributeGroup_DefaultValue_FieldSelecting);
			_Graph.RowDeleting.AddHandler<CSAttributeGroup>(OnRowDeleting);


			if (!graph.Views.Caches.Contains(typeof(CSAnswers)))
				graph.Views.Caches.Add(typeof(CSAnswers));
		}

		private void OnRowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			var attributeGroup = (CSAttributeGroup)e.Row;
			if (attributeGroup == null
				|| sender.GetStatus(e.Row) == PXEntryStatus.Inserted
				|| sender.GetStatus(e.Row) == PXEntryStatus.InsertedDeleted
				|| (_Graph.Caches[_class].Current != null && _Graph.Caches[_class].GetStatus(_Graph.Caches[_class].Current) == PXEntryStatus.Deleted))
				return;

			if (attributeGroup.IsActive == true)
				throw new PXSetPropertyException(Messages.AttributeCannotDeleteActive);

			if (!_Graph.IsContractBasedAPI && Ask("Warning", Messages.AttributeDeleteWarning, MessageButtons.OKCancel) != WebDialogResult.OK)
			{
				e.Cancel = true;
				return;
			}
			DeleteAttributesForGroup(_Graph, attributeGroup);
		}

		public static void DeleteAttributesForGroup(PXGraph graph, CSAttributeGroup attributeGroup)
		{
			Type entityType = PXBuildManager.GetType(attributeGroup.EntityType, false);
			if (entityType == null)
				throw new ArgumentNullException(nameof(entityType), $"Could not locate entity type {attributeGroup.EntityType}");
			var noteIdField = EntityHelper.GetNoteType(entityType);
			if (noteIdField == null)
				throw new ArgumentNullException(nameof(noteIdField), $"Could not locate NoteId field for {attributeGroup.EntityType}");
			var classIdField =
				graph.Caches[entityType].GetAttributes(null).OfType<CRAttributesFieldAttribute>().FirstOrDefault()?.ClassIdField;
			if (classIdField == null)
				throw new ArgumentNullException(nameof(classIdField), $"Could not locate ClassId field for {attributeGroup.EntityType}");

			var queryParameterValues = new List<object>
			{
				attributeGroup.EntityClassID,
				attributeGroup.EntityClassID,
				attributeGroup.AttributeID,
				attributeGroup.EntityType,
			};
			if (classIdField != null)
			{
				queryParameterValues[0] = graph.Caches[entityType].ValueFromString(classIdField.Name, attributeGroup.EntityClassID);
			}

			var classIdIsDbField =
				graph.Caches[entityType].GetAttributes(classIdField.Name)
					.Any(x =>
					{
						var type = x.GetType();
						return type.IsSubclassOf(typeof(PXDBFieldAttribute)) ||
							type == typeof(PXDBCalcedAttribute) ||
							type.IsSubclassOf(typeof(PXDBCalcedAttribute));
					});

			//ClassId can be constant
			if (!classIdIsDbField)
				classIdField = typeof(CSAttributeGroup.entityClassID);

			var graph2 = new PXGraph();

			Type requiredClassIdField = BqlCommand.Compose(typeof(Equal<>), typeof(Required<>), classIdField);

			var view = new PXView(graph2, false, BqlCommand.CreateInstance(
				BqlCommand.Compose(
					typeof(Select2<,,>), typeof(CSAttributeGroup),
					typeof(InnerJoin<,,>), typeof(CSAnswers), typeof(On<CSAnswers.attributeID, Equal<CSAttributeGroup.attributeID>>),
					typeof(InnerJoin<,>), entityType, typeof(On<,,>), noteIdField, typeof(Equal<CSAnswers.refNoteID>), typeof(And<,>),
						classIdField, requiredClassIdField,
					typeof(Where<CSAttributeGroup.entityClassID, Equal<Required<CSAttributeGroup.entityClassID>>,
							And<CSAttributeGroup.attributeID, Equal<Required<CSAttributeGroup.attributeID>>,
							And<CSAttributeGroup.entityType, Equal<Required<CSAttributeGroup.entityType>>>>>)
					)));

			var answers = view.SelectMultiBound(null, queryParameterValues.ToArray());

			if (answers.Count == 0)
				return;

			foreach (var resultSet in answers)
			{
				var answer = PXResult.Unwrap<CSAnswers>(resultSet);
				graph.Caches<CSAnswers>().Delete(answer);
			}
		}

		private void CSAttributeGroup_DefaultValue_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			CSAttributeGroup row = e.Row as CSAttributeGroup;

			if (row == null)
				return;

			const string answerValueField = "DefaultValue";
			const int answerValueLength = 60;

			CSAttribute question = new PXSelect<CSAttribute>(_Graph).Search<CSAttribute.attributeID>(row.AttributeID);

			PXResultset<CSAttributeDetail> options = PXSelect<CSAttributeDetail,
				Where<CSAttributeDetail.attributeID, Equal<Required<CSAttributeGroup.attributeID>>>,
				OrderBy<Asc<CSAttributeDetail.sortOrder>>>.Select(_Graph, row.AttributeID);

			int required = row.Required.GetValueOrDefault() ? 1 : -1;

			//if (options.Count > 0)
			if (options.Count > 0 &&
				(question == null || question.ControlType == CSAttribute.Combo ||
				 question.ControlType == CSAttribute.MultiSelectCombo))
			{
				//ComboBox:

				List<string> allowedValues = new List<string>();
				List<string> allowedLabels = new List<string>();

				foreach (CSAttributeDetail option in options)
				{
					allowedValues.Add(option.ValueID);
					allowedLabels.Add(option.Description);
				}

				string mask = question != null ? question.EntryMask : null;

				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, CSAttributeDetail.ParameterIdLength,
					true, answerValueField, false, required, mask, allowedValues.ToArray(), allowedLabels.ToArray(),
					true, null);

				if (question.ControlType == CSAttribute.MultiSelectCombo)
				{
					((PXStringState)e.ReturnState).MultiSelect = true;
				}
			}
			else if (question != null)
			{
				if (question.ControlType.GetValueOrDefault() == CSAttribute.CheckBox)
				{
					e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(bool), false, false, required,
						null, null, false, answerValueField, null, null, null, PXErrorLevel.Undefined, true, true,
						null, PXUIVisibility.Visible, null, null, null);
				}
				else if (question.ControlType.GetValueOrDefault() == CSAttribute.Datetime)
				{
					e.ReturnState = PXDateState.CreateInstance(e.ReturnState, answerValueField, false, required,
						question.EntryMask, question.EntryMask, null, null);
				}
				else
				{
					//TextBox:
					e.ReturnState = PXStringState.CreateInstance(e.ReturnState, answerValueLength, null,
						answerValueField, false, required, question.EntryMask, null, null, true, null);
				}
			}
		}

		protected virtual IEnumerable SelectDelegate()
		{
			var entityClassCache = _Graph.Caches[_class];
			var row = entityClassCache.Current;

			if (row == null)
				yield break;

			var classIdValue = entityClassCache.GetValue(row, _classIdFieldName);

			if (classIdValue == null)
				yield break;

			var resultSet =
				new PXSelectJoin
					<CSAttributeGroup,
						InnerJoin<CSAttribute, On<CSAttributeGroup.attributeID, Equal<CSAttribute.attributeID>>>,
						Where<CSAttributeGroup.entityClassID, Equal<Required<CSAttributeGroup.entityClassID>>,
							 And<CSAttributeGroup.entityType, Equal<Required<CSAttributeGroup.entityType>>>>>(_Graph)
					.Select(classIdValue.ToString(), typeof(TEntity).FullName);

			foreach (var record in resultSet)
			{
				var attributeGroup = PXResult.Unwrap<CSAttributeGroup>(record);

				if (attributeGroup != null)
					yield return record;
			}
		}
	}

	#endregion

	#region CRAttributeList

	public class CRAttributeList<TEntity> : PXSelectBase<CSAnswers>
	{
		#region TypeNameConst
		public class TypeNameConst : PX.Data.BQL.BqlString.Constant<TypeNameConst>
		{
			public TypeNameConst() : base(typeof(TEntity).FullName) { }
		}
		#endregion

		private readonly EntityHelper _helper;
		private const string CbApiValueFieldName = "Value$value";
		private const string CbApiAttributeIDFieldName = "AttributeID$value";

		public virtual bool ForceValidationInUnattendedMode { get; set; }

		public CRAttributeList(PXGraph graph)
		{
			_Graph = graph;
			_helper = new EntityHelper(graph);

			View = graph.IsExport
				? GetExportOptimizedView()
				: new PXView(graph, false,
				new Select3<CSAnswers, OrderBy<Asc<CSAnswers.order>>>(),
				new PXSelectDelegate(SelectDelegate));

			PXDependToCacheAttribute.AddDependencies(View, new[] { typeof(TEntity) });

			_Graph.EnsureCachePersistence(typeof(CSAnswers));
			PXDBAttributeAttribute.Activate(_Graph.Caches[typeof(TEntity)]);
			_Graph.FieldUpdating.AddHandler<CSAnswers.value>(FieldUpdatingHandler);
			_Graph.FieldSelecting.AddHandler<CSAnswers.value>(FieldSelectingHandler);
			_Graph.FieldSelecting.AddHandler<CSAnswers.isRequired>(IsRequiredSelectingHandler);
			_Graph.FieldSelecting.AddHandler<CSAnswers.attributeCategory>(AttributeCategorySelectingHandler);
			_Graph.FieldSelecting.AddHandler<CSAnswers.attributeID>(AttrFieldSelectingHandler);
			_Graph.RowPersisting.AddHandler<CSAnswers>(RowPersistingHandler);
			_Graph.RowPersisting.AddHandler<TEntity>(ReferenceRowPersistingHandler);
			_Graph.RowUpdating.AddHandler<TEntity>(ReferenceRowUpdatingHandler);
			_Graph.RowDeleted.AddHandler<TEntity>(ReferenceRowDeletedHandler);
			_Graph.RowInserted.AddHandler<TEntity>(RowInsertedHandler);

			_Graph.Caches<CSAnswers>().Fields.Add(CbApiValueFieldName);
			_Graph.Caches<CSAnswers>().Fields.Add(CbApiAttributeIDFieldName);
			_Graph.FieldSelecting.AddHandler(typeof(CSAnswers), CbApiValueFieldName, CbApiValueFieldSelectingHandler);
			_Graph.FieldSelecting.AddHandler(typeof(CSAnswers), CbApiAttributeIDFieldName, CbApiAttributeIdFieldSelectingHandler);
		}

		private PXView GetExportOptimizedView()
		{
			var instance = _Graph.Caches[typeof(TEntity)].CreateInstance();
			var classIdField = GetClassIdField(instance);
			var noteIdField = typeof(TEntity).GetNestedType(nameof(CSAttribute.noteID));

			var command = BqlTemplate.OfCommand<
					Select2<CSAnswers,
						InnerJoin<CSAttribute,
							On<CSAnswers.attributeID, Equal<CSAttribute.attributeID>>,
						InnerJoin<CSAttributeGroup,
							On<CSAnswers.attributeID, Equal<CSAttributeGroup.attributeID>>>>,
						Where<CSAttributeGroup.isActive, Equal<True>,
							And<CSAttributeGroup.entityType, Equal<TypeNameConst>,
							And<CSAttributeGroup.entityClassID, Equal<Current<BqlPlaceholder.A>>,
							And<CSAnswers.refNoteID, Equal<Current<BqlPlaceholder.B>>>>>>>>
					.Replace<BqlPlaceholder.A>(classIdField)
					.Replace<BqlPlaceholder.B>(noteIdField)
					.ToCommand();

			return new PXView(_Graph, true, command);
		}

		protected virtual IEnumerable SelectDelegate()
		{
			var currentObject = _Graph.Caches[typeof(TEntity)].Current;
			return SelectInternal(currentObject);
		}

		protected Guid? GetNoteId(object row)
		{
			return _helper.GetEntityNoteID(row);
		}

		protected Type GetClassIdField(object row)
		{
			if (row == null)
				return null;


			var fieldAttribute =
				_Graph.Caches[row.GetType()].GetAttributes(row, null)
					.OfType<CRAttributesFieldAttribute>()
					.FirstOrDefault();

			if (fieldAttribute == null)
				return null;

			return fieldAttribute.ClassIdField;
		}

		protected Type GetEntityTypeFromAttribute(object row)
		{
			var classIdField = GetClassIdField(row);
			if (classIdField == null)
				return null;

			return classIdField.DeclaringType;
		}

		protected string GetClassId(object row)
		{
			var classIdField = GetClassIdField(row);
			if (classIdField == null)
				return null;

			var entityCache = _Graph.Caches[row.GetType()];

			var classIdValue = entityCache.GetValue(row, classIdField.Name);

			return classIdValue?.ToString();
		}

		protected virtual PXCache GetAnswers()
		{
			return _Graph.Caches[typeof(CSAnswers)];
		}

		private (string id, string description) GetClassIdAndDescription(object row)
		{
			var classIdField = GetClassIdField(row);
			if (classIdField == null)
				return default;

			var entityCache = _Graph.Caches[row.GetType()];

			var classIdValue = entityCache.GetValue(row, classIdField.Name)?.ToString();
			var descrValue = (entityCache.GetStateExt(row, classIdField.Name) as PXFieldState)?.Value?.ToString().Trim();

			return (classIdValue, descrValue ?? classIdValue);
		}

		[PXInternalUseOnly]
		public IEnumerable<CSAnswers> SelectInternal(object row)
		{
			return SelectInternal(_Graph, row);
		}

		[PXInternalUseOnly]
		public IEnumerable<CSAnswers> SelectInternal(PXGraph graph, object row)
		{
			if (row == null)
				yield break;

			var noteId = GetNoteId(row);

			if (!noteId.HasValue)
				yield break;

			var answerCache = graph.Caches[typeof(CSAnswers)];
			var entityCache = graph.Caches[row.GetType()];

			List<CSAnswers> answerList;

			var status = entityCache.GetStatus(row);

			if (status == PXEntryStatus.Inserted || status == PXEntryStatus.InsertedDeleted)
			{
				answerList = answerCache.Inserted.Cast<CSAnswers>().Where(x => x.RefNoteID == noteId &&
					 !MatrixAttributeSelectorAttribute.DummyAttributeName.Equals(x.AttributeID, StringComparison.OrdinalIgnoreCase)).ToList();
			}
			else
			{
				answerList = PXSelect<CSAnswers, Where<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>,
					And<CSAnswers.attributeID, NotEqual<MatrixAttributeSelectorAttribute.dummyAttributeName>>>>
					.Select(graph, noteId).FirstTableItems.ToList();
			}

			var classId = GetClassId(row);

			CRAttribute.ClassAttributeList classAttributeList = new CRAttribute.ClassAttributeList();

			if (classId != null)
			{
				classAttributeList = CRAttribute.EntityAttributes(GetEntityTypeFromAttribute(row), classId);
				classAttributeList.Where(_ => _.NotInClass)
					.Select(_ => _.ID)
					.ToList()
					.ForEach(_ => classAttributeList.Remove(_));
			}
			//when coming from Import scenarios there might be attributes which don't belong to entity's current attribute class or the entity might not have any attribute class at all
			if (graph.IsImport && PXView.SortColumns.Any() && PXView.Searches.Any())
			{
				var columnIndex = Array.FindIndex(PXView.SortColumns,
					x => x.Equals(typeof(CSAnswers.attributeID).Name, StringComparison.OrdinalIgnoreCase));

				if (columnIndex >= 0 && columnIndex < PXView.Searches.Length)
				{
					var searchValue = PXView.Searches[columnIndex];

					if (searchValue != null)
					{
						//searchValue can be either AttributeId or Description
						var attributeDefinition = CRAttribute.Attributes[searchValue.ToString()] ??
											 CRAttribute.AttributesByDescr[searchValue.ToString()];

						if (attributeDefinition == null)
						{
							string message = GetSelectInternalExceptionMessage();
							throw new PXSetPropertyException(message, searchValue.ToString());
						}
						else if (classAttributeList[attributeDefinition.ID] == null)
							classAttributeList.Add(new CRAttribute.AttributeExt(attributeDefinition, null, false, true) { NotInClass = true });
					}
				}
			}

			if (answerList.Count == 0 && classAttributeList.Count == 0)
				yield break;

			//attribute identifiers that are contained in class attribute list but not in CSAnswers cache/table
			List<string> attributeIdListClass =
				classAttributeList.Select(x => x.ID)
					.Except(answerList.Select(x => x.AttributeID))
					.ToList();

			//attribute identifiers which belong to both lists
			List<string> attributeIdListIntersection =
				classAttributeList.Select(x => x.ID)
					.Intersect(answerList.Select(x => x.AttributeID))
					.Distinct()
					.ToList();


			var cacheIsDirty = answerCache.IsDirty;

			List<CSAnswers> output = new List<CSAnswers>();

			//attributes contained only in class attribute list should be created and initialized with default value
			foreach (var attributeId in attributeIdListClass)
			{
				var classAttributeDefinition = classAttributeList[attributeId];

				if (PXSiteMap.IsPortal && classAttributeDefinition.IsInternal
						&& string.IsNullOrEmpty(classAttributeDefinition.DefaultValue))
					continue;

				if (!classAttributeDefinition.IsActive)
					continue;

				CSAnswers answer = (CSAnswers)answerCache.CreateInstance();
				answer.AttributeID = classAttributeDefinition.ID;
				answer.RefNoteID = noteId;
				answer.Value = GetDefaultAnswerValue(classAttributeDefinition);
				if (classAttributeDefinition.ControlType == CSAttribute.CheckBox)
				{
					bool value;
					if (bool.TryParse(answer.Value, out value))
						answer.Value = Convert.ToInt32(value).ToString(CultureInfo.InvariantCulture);
					else if (answer.Value == null)
						answer.Value = 0.ToString();
				}

				answer.IsRequired = classAttributeDefinition.Required;
				answer.NotInClass = classAttributeDefinition.NotInClass;


				Dictionary<string, object> keys = new Dictionary<string, object>();
				foreach (string key in answerCache.Keys.ToArray())
				{
					keys[key] = answerCache.GetValue(answer, key);
				}

				answerCache.Locate(keys);

				answer = (CSAnswers)(answerCache.Locate(answer) ?? answerCache.Insert(answer));

				if (PXSiteMap.IsPortal && classAttributeDefinition.IsInternal)
					continue;

				output.Add(answer);
			}

			//attributes belonging to both lists should be selected from CSAnswers cache/table with and additional IsRequired check against class definition
			foreach (CSAnswers answer in answerList.Where(x => attributeIdListIntersection.Contains(x.AttributeID)).ToList())
			{
				var classAttributeDefinition = classAttributeList[answer.AttributeID];

				if (PXSiteMap.IsPortal && classAttributeDefinition.IsInternal)
					continue;

				if (!classAttributeDefinition.IsActive)
					continue;

				if (answer.Value == null && classAttributeDefinition.ControlType == CSAttribute.CheckBox)
					answer.Value = bool.FalseString;

				if (answer.IsRequired == null || classAttributeDefinition.Required != answer.IsRequired)
				{
					answer.IsRequired = classAttributeDefinition.Required;

					var fieldState = View.Cache.GetValueExt<CSAnswers.isRequired>(answer) as PXFieldState;
					var fieldValue = fieldState != null && ((bool?)fieldState.Value).GetValueOrDefault();

					answer.IsRequired = classAttributeDefinition.Required || fieldValue;
				}



				output.Add(answer);
			}

			answerCache.IsDirty = cacheIsDirty;

			output =
				output.OrderBy(
					x =>
						classAttributeList.Contains(x.AttributeID)
							? classAttributeList.IndexOf(x.AttributeID)
							: (x.Order ?? 0))
					.ThenBy(x => x.AttributeID)
					.ToList();

			short attributeOrder = 0;

			foreach (CSAnswers answer in output)
			{
				answer.Order = attributeOrder++;
				yield return answer;
			}
		}

		protected virtual void FieldUpdatingHandler(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			var row = e.Row as CSAnswers;

			if (row == null || row.AttributeID == null)
				return;

			var attr = CRAttribute.Attributes[row.AttributeID];
			if (attr == null)
				return;

			if (e.NewValue is DateTime v && attr.ControlType == CSAttribute.Datetime)
			{
				e.NewValue = v.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
				return;
			}

			if (!(e.NewValue is string newValue))
				return;

			if (row.NotInClass.GetValueOrDefault() && e.NewValue != null)
			{
				var entity = new EntityHelper(_Graph).GetEntityRow(typeof(TEntity), row.RefNoteID);
				var (classId, classDescription) = GetClassIdAndDescription(entity);
				sender.RaiseExceptionHandling<CSAnswers.value>(row, newValue,
					new PXSetPropertyException(MessagesNoPrefix.AttributeNotFoundInClass, row.AttributeID, classDescription));
			}

			switch (attr.ControlType)
			{
				case CSAttribute.CheckBox:
					{
						bool value;
						if (bool.TryParse(newValue, out value))
						{
							e.NewValue = Convert.ToInt32(value).ToString(CultureInfo.InvariantCulture);
						}
						else if (newValue != "0" && newValue != "1")
						{
							sender.RaiseExceptionHandling<CSAnswers.value>(row, newValue,
								new PXSetPropertyException(
									MessagesNoPrefix.CbApi_Attributes_CheckboxAttributeDoesntSupportSpecifiedValue,
									row.AttributeID));
						}
						break;
					}
				case CSAttribute.Datetime:
					{
						DateTime dt;
						if (sender.Graph.IsMobile)
						{
							newValue = newValue.Replace("Z", "");
						}
						if (DateTime.TryParse(newValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
						{
							e.NewValue = dt.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
						}
						else
						{
							sender.RaiseExceptionHandling<CSAnswers.value>(row, newValue,
								new PXSetPropertyException(
									MessagesNoPrefix.CbApi_Attributes_DatetimeAttributeDoesntSupportSpecifiedValue,
									row.AttributeID));
						}
						break;
					}
				case CSAttribute.MultiSelectCombo:
					{
						if (newValue.IsNullOrEmpty())
						{
							break;
						}

						var parts = newValue.Split(new[] { SystemConstants.MultiSelectValuesDelimiterCharacter }, StringSplitOptions.RemoveEmptyEntries);
						foreach (var part in parts)
						{
							if (attr.Values.Find(a => string.Equals(a.ValueID, part, StringComparison.OrdinalIgnoreCase)) == null)
							{
								sender.RaiseExceptionHandling<CSAnswers.value>(row, newValue,
									new PXSetPropertyException(
										MessagesNoPrefix.CbApi_Attributes_MultiComboboxAttributeDoesntSupportSpecifiedValue,
										row.AttributeID));
								break;
							}
						}
						break;
					}
				case CSAttribute.Combo:
					{
						var atValue = attr.Values.Find(a => string.Equals(a.ValueID.TrimEnd(), newValue.TrimEnd(), StringComparison.OrdinalIgnoreCase));
						if (atValue == null)
							atValue = attr.Values.Find(a => string.Equals(a.Description, newValue, StringComparison.OrdinalIgnoreCase));
						if (atValue != null)
							e.NewValue = atValue.ValueID;
						else
							sender.RaiseExceptionHandling<CSAnswers.value>(row, newValue,
								new PXSetPropertyException(
									MessagesNoPrefix.CbApi_Attributes_ComboboxAttributeDoesntSupportSpecifiedValue,
									row.AttributeID));
						break;
					}
			}
		}

		protected virtual void FieldSelectingHandler(PXCache sender, PXFieldSelectingEventArgs e)
		{
			var row = e.Row as CSAnswers;
			if (row == null) return;

			var question = CRAttribute.Attributes[row.AttributeID];

			var options = question != null ? question.Values : null;

			var required = row.IsRequired == true ? 1 : -1;

			if (options != null && options.Count > 0)
			{
				//ComboBox:
				var allowedValues = new List<string>();
				var allowedLabels = new List<string>();

				foreach (var option in options)
				{
					if (option.Disabled && row.Value != option.ValueID) continue;

					allowedValues.Add(option.ValueID.TrimEnd());
					allowedLabels.Add(option.Description);
				}

				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, CSAttributeDetail.ParameterIdLength,
					true, typeof(CSAnswers.value).Name, false, required, question.EntryMask, allowedValues.ToArray(),
					allowedLabels.ToArray(), true, null);
				if (question.ControlType == CSAttribute.MultiSelectCombo)
				{
					((PXStringState)e.ReturnState).MultiSelect = true;
					if (sender.Graph.IsContractBasedAPI && e.ReturnValue is string values)
					{
						// export engige shows multiselect as internal values list,
						// but here it is just representation
						// have to build it explicitly
						e.ReturnValue = string.Join(", ",
							values.Split(',').Select(i =>
							{
								int index = allowedValues.IndexOf(i.Trim());
								if (index >= 0)
									return allowedLabels[index];
								return i;
							}));
					}
				}

			}
			else if (question != null)
			{
				if (question.ControlType == CSAttribute.CheckBox)
				{
					e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(bool), false, false, required,
						null, null, false, typeof(CSAnswers.value).Name, null, null, null, PXErrorLevel.Undefined, true,
						true, null,
						PXUIVisibility.Visible, null, null, null);
					if (e.ReturnValue is string)
					{
						int value;
						if (int.TryParse((string)e.ReturnValue, NumberStyles.Integer, CultureInfo.InvariantCulture,
							out value))
						{
							e.ReturnValue = Convert.ToBoolean(value);
						}
					}
				}
				else if (question.ControlType == CSAttribute.Datetime)
				{
					e.ReturnState = PXDateState.CreateInstance(e.ReturnState, typeof(CSAnswers.value).Name, false,
						required, question.EntryMask, question.EntryMask,
						null, null);
				}
				else
				{
					//TextBox:
					var vstate = sender.GetStateExt<CSAnswers.value>(null) as PXStringState;
					e.ReturnState = PXStringState.CreateInstance(e.ReturnState, vstate.With(_ => _.Length), null,
						typeof(CSAnswers.value).Name,
						false, required, question.EntryMask, null, null, true, null);
				}
			}
			if (e.ReturnState is PXFieldState)
			{
				var state = (PXFieldState)e.ReturnState;
				var errorState = sender.GetAttributes(row, typeof(CSAnswers.value).Name)
					.OfType<IPXInterfaceField>()
					.FirstOrDefault();
				if (errorState != null && errorState.ErrorLevel != PXErrorLevel.Undefined && !string.IsNullOrEmpty(errorState.ErrorText))
				{
					state.Error = errorState.ErrorText;
					state.ErrorLevel = errorState.ErrorLevel;
				}

				string category = (string)(sender.GetValueExt<CSAnswers.attributeCategory>(row) as PXFieldState)?.Value;
				state.Enabled = (category != CSAttributeGroup.attributeCategory.Variant);

				if (_Graph.IsContractBasedAPI)
				{
					// hide error from ValueDescription field
					state.ErrorLevel = PXErrorLevel.Undefined;
					state.Error = null;
					e.Cancel = true;
				}
			}
		}

		internal virtual void CbApiValueFieldSelectingHandler(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row is CSAnswers answer)
			{
				// set in FieldUpdating
				if (sender.GetAttributes<CSAnswers.value>(answer).OfType<PXUIFieldAttribute>().FirstOrDefault() is IPXInterfaceField uiField)
				{
					if (uiField.ErrorText != null)
					{
						e.ReturnState = PXFieldState.CreateInstance(uiField.ErrorValue, typeof(String),
							errorLevel: PXErrorLevel.Error,
							error: uiField.ErrorText);
					}
				}
				//if(e.ReturnValue is PXSetPropertyException ex)
				//{
				//	e.ReturnState = PXFieldState.CreateInstance(answer.Value, typeof(string),
				//		errorLevel: PXErrorLevel.Error,
				//		error: ex.Message);
				//}
				e.ReturnValue = answer.Value;

			}
		}

		internal virtual void CbApiAttributeIdFieldSelectingHandler(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row is CSAnswers answer)
				e.ReturnValue = answer.AttributeID;
		}

		protected virtual void AttrFieldSelectingHandler(PXCache sender, PXFieldSelectingEventArgs e)
		{
			PXUIFieldAttribute.SetEnabled<CSAnswers.attributeID>(sender, e.Row, false);
		}

		protected virtual void IsRequiredSelectingHandler(PXCache sender, PXFieldSelectingEventArgs e)
		{
			var row = e.Row as CSAnswers;
			var current = sender.Graph.Caches[typeof(TEntity)].Current;

			if (row == null || current == null)
				return;
			var currentNoteId = GetNoteId(current);

			if (e.ReturnValue != null || row.RefNoteID != currentNoteId)
				return;

			//when importing data - make all attributes nonrequired (otherwise import might fail)
			if (sender.Graph.IsImport)
			{
				e.ReturnValue = false;
				return;
			}

			var currentClassId = GetClassId(current);

			var attribute = CRAttribute.EntityAttributes(GetEntityTypeFromAttribute(current), currentClassId)[row.AttributeID];

			if (attribute == null)
			{
				e.ReturnValue = false;
			}
			else
			{
				if (PXSiteMap.IsPortal && attribute.IsInternal)
					e.ReturnValue = false;
				else
					e.ReturnValue = attribute.Required;
			}
		}

		protected virtual void AttributeCategorySelectingHandler(PXCache sender, PXFieldSelectingEventArgs e)
		{
			var row = e.Row as CSAnswers;
			var current = sender.Graph.Caches[typeof(TEntity)].Current;

			if (row == null || current == null)
				return;
			var currentNoteId = GetNoteId(current);

			if (e.ReturnValue != null || row.RefNoteID != currentNoteId)
				return;

			var currentClassId = GetClassId(current);

			var attribute = CRAttribute.EntityAttributes(GetEntityTypeFromAttribute(current), currentClassId)[row.AttributeID];

			e.ReturnValue = attribute?.AttributeCategory ?? CSAttributeGroup.attributeCategory.Attribute;
		}

		protected virtual void RowPersistingHandler(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation != PXDBOperation.Insert && e.Operation != PXDBOperation.Update) return;

			var row = e.Row as CSAnswers;
			if (row == null) return;

			if (!row.RefNoteID.HasValue)
			{
				e.Cancel = true;
				RowPersistDeleted(sender, row);
			}
			else if (string.IsNullOrEmpty(row.Value) && row.IsActive == true)
			{
				var mayNotBeEmpty = PXMessages.LocalizeFormatNoPrefix(ErrorMessages.FieldIsEmpty,
					sender.GetStateExt<CSAnswers.value>(null).With(_ => _ as PXFieldState).With(_ => _.DisplayName));
				if (row.IsRequired == true &&
					sender.RaiseExceptionHandling<CSAnswers.value>(e.Row, row.Value,
						new PXSetPropertyException(mayNotBeEmpty, PXErrorLevel.RowError, typeof(CSAnswers.value).Name)))
				{
					throw new PXRowPersistingException(typeof(CSAnswers.value).Name, row.Value, mayNotBeEmpty,
						typeof(CSAnswers.value).Name);
				}
				e.Cancel = true;
				if (sender.GetStatus(row) != PXEntryStatus.Inserted)
					RowPersistDeleted(sender, row);
			}

		}

		protected virtual void RowPersistDeleted(PXCache cache, object row)
		{
			try
			{
				cache.PersistDeleted(row);
				cache.SetStatus(row, PXEntryStatus.InsertedDeleted);
			}
			catch (PXLockViolationException)
			{
			}
			cache.ResetPersisted(row);
		}
		protected virtual void ReferenceRowDeletedHandler(PXCache sender, PXRowDeletedEventArgs e)
		{
			object row = e.Row;
			if (row == null) return;

			var noteId = GetNoteId(row);

			if (!noteId.HasValue) return;

			var answerCache = _Graph.Caches[typeof(CSAnswers)];
			var entityCache = _Graph.Caches[row.GetType()];

			List<CSAnswers> answerList;

			var status = entityCache.GetStatus(row);

			if (status == PXEntryStatus.Inserted || status == PXEntryStatus.InsertedDeleted)
			{
				answerList = answerCache.Inserted.Cast<CSAnswers>().Where(x => x.RefNoteID == noteId).ToList();
			}
			else
			{
				answerList = PXSelect<CSAnswers, Where<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>>>
					.Select(_Graph, noteId).FirstTableItems.ToList();
			}
			foreach (var answer in answerList)
				this.Cache.Delete(answer);
		}

		protected virtual void ReferenceRowPersistingHandler(PXCache sender, PXRowPersistingEventArgs e)
		{
			var row = e.Row;

			if (row == null) return;

			var answersCache = GetAnswers();

			var classId = GetClassId(row);

			CRAttribute.ClassAttributeList classAttributeList = new CRAttribute.ClassAttributeList();

			if (classId != null)
			{
				classAttributeList = CRAttribute.EntityAttributes(GetEntityTypeFromAttribute(row), classId);
				classAttributeList.Where(_ => _.NotInClass)
					.Select(_ => _.ID)
					.ToList()
					.ForEach(_ => classAttributeList.Remove(_));
			}

			var emptyRequired = new List<string>();
			foreach (CSAnswers answer in answersCache.Cached)
			{
				if (answer.IsRequired == null)
				{
					var state = View.Cache.GetValueExt<CSAnswers.isRequired>(answer) as PXFieldState;
					if (state != null)
						answer.IsRequired = state.Value as bool?;
				}

				if (e.Operation == PXDBOperation.Delete)
				{
					answersCache.Delete(answer);
				}
				else if (string.IsNullOrEmpty(answer.Value) && answer.IsRequired == true
					&& (!_Graph.UnattendedMode || ForceValidationInUnattendedMode))
				{
					CRAttribute.AttributeExt classAttributeDefinition;
					if (classAttributeList.TryGetValue(answer.AttributeID, out classAttributeDefinition)
						&& !classAttributeDefinition.IsActive)
					{
						continue;
					}

					var displayName = "";

					var attributeDefinition = CRAttribute.Attributes[answer.AttributeID];
					if (attributeDefinition != null)
						displayName = attributeDefinition.Description;

					emptyRequired.Add(displayName);
					var mayNotBeEmpty = PXMessages.LocalizeFormatNoPrefix(ErrorMessages.FieldIsEmpty, displayName);
					answersCache.RaiseExceptionHandling<CSAnswers.value>(answer, answer.Value,
						new PXSetPropertyException(mayNotBeEmpty, PXErrorLevel.RowError, typeof(CSAnswers.value).Name));
					PXUIFieldAttribute.SetError<CSAnswers.value>(answersCache, answer, mayNotBeEmpty);
				}
			}
			if (emptyRequired.Count > 0)
			{
				var errors = new Dictionary<string, string>();
				errors.Add("1", PXMessages.LocalizeFormatNoPrefix(Messages.RequiredAttributesAreEmpty, string.Join(", ", emptyRequired.Select(s => string.Format("'{0}'", s)))));
				throw new PXOuterException(errors, _Graph.GetType(), row,
					Messages.RequiredAttributesAreEmpty,
					string.Join(", ", emptyRequired.Select(s => string.Format("'{0}'", s))));
			}
		}

		protected virtual void ReferenceRowUpdatingHandler(PXCache sender, PXRowUpdatingEventArgs e)
		{
			var row = e.Row;
			var newRow = e.NewRow;

			if (row == null || newRow == null)
				return;

			var rowNoteId = GetNoteId(row);

			var rowClassId = GetClassId(row);
			var newRowClassId = GetClassId(newRow);

			if (string.Equals(rowClassId, newRowClassId, StringComparison.InvariantCultureIgnoreCase))
			{
				// SelectInternal fills Answers caches...
				// but this line is called more than one time from cb api
				// so need to execute it only ones
				if (_Graph.IsContractBasedAPI
					&& !_Graph.Caches[typeof(CSAnswers)].Inserted.Any_())
					SelectInternal(newRow).ToList();

				return;
			}
			else if (_Graph.IsContractBasedAPI)
			{
				// workaround to clear cache if class wasn't default
				// (reverts SelectInternal above in previous call, because first call happens with default class)
				// otherwise it will not fill required attributes
				// AC-130095
				_Graph.Caches[typeof(CSAnswers)].Clear();
			}

			var newAttrList = new HashSet<string>();

			if (newRowClassId != null)
			{
				foreach (var attr in CRAttribute.EntityAttributes(GetEntityTypeFromAttribute(newRow), newRowClassId))
				{
					newAttrList.Add(attr.ID);
				}
			}
			var relatedEntityTypes =
				sender.GetAttributesOfType<CRAttributesFieldAttribute>(newRow, null).FirstOrDefault()?.RelatedEntityTypes;

			PXGraph entityGraph = new PXGraph();
			var entityHelper = new EntityHelper(entityGraph);

			if (relatedEntityTypes != null)
				foreach (var classField in relatedEntityTypes)
				{
					object entity = entityHelper.GetEntityRow(classField.DeclaringType, rowNoteId);
					if (entity == null) continue;
					string entityClass = (string)entityGraph.Caches[classField.DeclaringType].GetValue(entity, classField.Name);
					if (entityClass == null) continue;
					CRAttribute.EntityAttributes(classField.DeclaringType, entityClass)
						.Where(x => !newAttrList.Contains(x.ID)).Select(x => x.ID)
						.ForEach(x => newAttrList.Add(x));
				}

			foreach (CSAnswers answersRow in
				PXSelect<CSAnswers,
					Where<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>>>
					.SelectMultiBound(sender.Graph, null, rowNoteId))
			{
				var copy = PXCache<CSAnswers>.CreateCopy(answersRow);
				View.Cache.Delete(answersRow);
				if (newAttrList.Contains(copy.AttributeID))
				{
					string rowDefaultValue =
						CRAttribute.EntityAttributes(GetEntityTypeFromAttribute(row), rowClassId)
							.Where(x => x.ID == copy.AttributeID && x.IsActive == true)
							.FirstOrDefault()
							?.DefaultValue;
					if (string.Equals(rowDefaultValue, copy.Value, StringComparison.InvariantCultureIgnoreCase))
					{
						string newDefaultValue =
						CRAttribute.EntityAttributes(GetEntityTypeFromAttribute(newRow), newRowClassId)
							.Where(x => x.ID == copy.AttributeID && x.IsActive == true)
							.FirstOrDefault()
							?.DefaultValue;
						copy.Value = newDefaultValue;
					}
					View.Cache.Insert(copy);
				}
			}

			if (newRowClassId != null)
				SelectInternal(newRow).ToList();

			sender.IsDirty = true;
		}

		protected virtual void RowInsertedHandler(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (sender != null && sender.Graph != null && !sender.Graph.IsImport)
				SelectInternal(e.Row).ToList();
		}

		public void CopyAttributes(PXGraph destGraph, object destination, PXGraph srcGraph, object source, bool copyAll)
		{
			if (destination == null || source == null)
			{
				return;
			}

			var sourceAttributes = SelectInternal(srcGraph, source).RowCast<CSAnswers>().ToList();
			var targetAttributes = SelectInternal(destGraph, destination).RowCast<CSAnswers>().ToList();

			var answerCache = _Graph.Caches<CSAnswers>();

			foreach (var targetAttribute in targetAttributes)
			{
				var sourceAttr = sourceAttributes.SingleOrDefault(x => x.AttributeID == targetAttribute.AttributeID);

				if (sourceAttr == null
					|| string.IsNullOrEmpty(sourceAttr.Value)
					|| sourceAttr.Value == targetAttribute.Value)
				{
					continue;
				}

				if (string.IsNullOrEmpty(targetAttribute.Value) || copyAll)
				{
					var answer = PXCache<CSAnswers>.CreateCopy(targetAttribute);
					answer.Value = sourceAttr.Value;
					answerCache.Update(answer);
				}
			}
		}

		protected virtual void CopyAttributes(object destination, object source, bool copyall)
		{
			if (destination == null || source == null) return;

			var sourceAttributes = SelectInternal(source).RowCast<CSAnswers>().ToList();
			var targetAttributes = SelectInternal(destination).RowCast<CSAnswers>().ToList();

			var answerCache = _Graph.Caches<CSAnswers>();


			foreach (var targetAttribute in targetAttributes)
			{
				var sourceAttr = sourceAttributes.SingleOrDefault(x => x.AttributeID == targetAttribute.AttributeID);

				if (sourceAttr == null || string.IsNullOrEmpty(sourceAttr.Value) ||
					sourceAttr.Value == targetAttribute.Value)
					continue;

				if (string.IsNullOrEmpty(targetAttribute.Value) || copyall)
				{
					var answer = PXCache<CSAnswers>.CreateCopy(targetAttribute);
					answer.Value = sourceAttr.Value;
					answerCache.Update(answer);
				}
			}
		}

		public void CopyAllAttributes(object row, object src)
		{
			CopyAttributes(row, src, true);
		}

		public void CopyAttributes(object row, object src)
		{
			CopyAttributes(row, src, false);
		}

		protected virtual string GetDefaultAnswerValue(CRAttribute.AttributeExt attr)
		{
			return attr.DefaultValue;
		}

		protected virtual string GetSelectInternalExceptionMessage()
		{
			return Messages.AttributeNotValid;
		}
	}

	#endregion

	#region CRAttributeSourceList

	public class CRAttributeSourceList<TReference, TSourceField> : CRAttributeList<TReference>
		where TReference : class, IBqlTable, new()
		where TSourceField : IBqlField
	{
		public CRAttributeSourceList(PXGraph graph)
			: base(graph)
		{
			_Graph.FieldUpdated.AddHandler<TSourceField>(ReferenceSourceFieldUpdated);
		}


		private object _AttributeSource;

		protected object AttributeSource
		{
			get
			{
				var cache = _Graph.Caches<TReference>();

				var noteFieldName = EntityHelper.GetNoteField(typeof(TReference));

				if (_AttributeSource == null ||
					GetNoteId(_AttributeSource) != (Guid?)cache.GetValue(cache.Current, noteFieldName))
				{
					_AttributeSource = PXSelectorAttribute.Select<TSourceField>(cache, cache.Current);
				}
				return _AttributeSource;
			}
		}

		protected override string GetDefaultAnswerValue(CRAttribute.AttributeExt attr)
		{
			if (AttributeSource == null)
				return base.GetDefaultAnswerValue(attr);

			var sourceNoteId = GetNoteId(AttributeSource);

			var answers =
				PXSelect<CSAnswers, Where<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>>>.Select(_Graph, sourceNoteId);

			foreach (CSAnswers answer in answers)
			{
				if (answer.AttributeID == attr.ID && !string.IsNullOrEmpty(answer.Value))
					return answer.Value;
			}

			return base.GetDefaultAnswerValue(attr);
		}

		protected void ReferenceSourceFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CopyAttributes(e.Row, AttributeSource);
		}
	}
	#endregion

	#region AddressSelectBase

	[Obsolete("This class has been deprecated and will be removed in Acumatica ERP 2021R1.")]
	public abstract class AddressSelectBase : PXSelectBase<Address>, ICacheType<Address>
	{
		private const string OBSOLETE_USE_VIEW = " Use " + nameof(GetView) + " instead."
			+ " Construct valid View or add new SelectDelegate there.";

		internal const string _BUTTON_ACTION = "ValidateAddress"; //TODO: need concat with graph selector name;
		private const string _VIEWONMAP_ACTION = "ViewOnMap";

		protected Type _itemType;

		protected int _addressIdFieldOrdinal;
		protected int _asMainFieldOrdinal;
		private int _accountIdFieldOrdinal;

		private PXAction _action;
		private PXAction _mapAction;

		protected AddressSelectBase(PXGraph graph)
		{
			Initialize(graph);
			View = GetView(graph);
			AttacheHandlers(graph);
			AppendButton(graph);
		}

		public bool DoNotCorrectUI { get; set; }

		private void AppendButton(PXGraph graph)
		{
			_action = PXNamedAction.AddAction(graph, _itemType, _BUTTON_ACTION, CS.Messages.ValidateAddress, CS.Messages.ValidateAddress, ValidateAddress);
			_mapAction = PXNamedAction.AddAction(graph, _itemType, _VIEWONMAP_ACTION, Messages.ViewOnMap, ViewOnMap);
		}

		private void Initialize(PXGraph graph)
		{
			_Graph = graph;
			_Graph.EnsureCachePersistence(typeof(Address));
			_Graph.Initialized += sender => sender.Views.Caches.Remove(IncorrectPersistableDAC);

			var addressIdDAC = GetDAC(AddressIdField);
			var asMainDAC = GetDAC(AsMainField);
			var accounDAC = GetDAC(AccountIdField);
			if (addressIdDAC != asMainDAC || asMainDAC != accounDAC)
				throw new Exception(string.Format("Fields '{0}', '{1}' and '{2}' are defined in different DACs",
					addressIdDAC.Name, asMainDAC.Name, accounDAC));
			_itemType = addressIdDAC;

			var cache = _Graph.Caches[_itemType];
			_addressIdFieldOrdinal = cache.GetFieldOrdinal(AddressIdField.Name);
			_asMainFieldOrdinal = cache.GetFieldOrdinal(AsMainField.Name);
			_accountIdFieldOrdinal = cache.GetFieldOrdinal(AccountIdField.Name);
		}

		protected abstract Type AccountIdField { get; }

		protected abstract Type AsMainField { get; }

		protected abstract Type AddressIdField { get; }

		protected abstract Type IncorrectPersistableDAC { get; }

		protected virtual PXView GetView(PXGraph graph)
		{
			return new PXView(graph, false, new Select<Address>(), new PXSelectDelegate(SelectDelegate));
		}

		private static Type GetDAC(Type type)
		{
			var res = type.DeclaringType;
			if (res == null)
				throw new Exception(string.Format("DAC for field '{0}' can not be found", type.Name));
			return res;
		}

		private void AttacheHandlers(PXGraph graph)
		{
			graph.RowInserted.AddHandler(_itemType, RowInsertedHandler);
			graph.RowUpdating.AddHandler(_itemType, RowUpdatingHandler);
			graph.RowUpdated.AddHandler(_itemType, RowUpdatedHandler);
			graph.RowSelected.AddHandler(_itemType, RowSelectedHandler);
			graph.RowDeleted.AddHandler(_itemType, RowDeletedHandler);
		}

		private void RowDeletedHandler(PXCache sender, PXRowDeletedEventArgs e)
		{
			var currentAddressId = sender.GetValue(e.Row, _addressIdFieldOrdinal);
			var isMainAddress = sender.GetValue(e.Row, AsMainField.Name) as bool?;
			if (isMainAddress == true) return;

			var currentAddress = currentAddressId.
				With(_ => (Address)PXSelect<Address,
					Where<Address.addressID, Equal<Required<Address.addressID>>>>.
				Select(sender.Graph, _));
			if (currentAddress != null)
			{
				var addressCache = sender.Graph.Caches[typeof(Address)];
				addressCache.Delete(currentAddress);
			}
		}

		private void RowSelectedHandler(PXCache sender, PXRowSelectedEventArgs e)
		{
			var addressCache = sender.Graph.Caches[typeof(Address)];
			var asMain = false;
			var isValidated = false;

			var accountId = sender.GetValue(e.Row, _accountIdFieldOrdinal);
			var account = accountId.
					With(_ => (BAccount)PXSelect<BAccount,
						Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
					Select(_Graph, _));
			var accountAddressId = account.With(_ => _.DefAddressID);
			var containsAccount = accountAddressId != null;

			var currentAddressId = sender.GetValue(e.Row, _addressIdFieldOrdinal) ?? accountAddressId;
			var currentAddress = currentAddressId.
				With(_ => (Address)PXSelect<Address,
					Where<Address.addressID, Equal<Required<Address.addressID>>>>.
				SelectSingleBound(sender.Graph, null, _));
			if (currentAddress != null)
			{
				isValidated = currentAddress.IsValidated == true;

				if (currentAddressId.Equals(accountAddressId))
					asMain = true;
			}
			else
			{
				PXEntryStatus status = sender.GetStatus(e.Row);
				if (status != PXEntryStatus.Inserted && status != PXEntryStatus.Deleted && status != PXEntryStatus.InsertedDeleted)
				{
					sender.SetValue(e.Row, _addressIdFieldOrdinal, null);
					RowInsertedHandler(sender, new PXRowInsertedEventArgs(e.Row, true));
					sender.SetStatus(e.Row, PXEntryStatus.Updated);
				}
			}

			sender.SetValue(e.Row, _asMainFieldOrdinal, asMain);
			if (!DoNotCorrectUI)
			{
				PXUIFieldAttribute.SetEnabled(addressCache, currentAddress, !asMain);
				PXUIFieldAttribute.SetEnabled(sender, e.Row, AsMainField.Name, containsAccount);
			}
			_action.SetEnabled(!isValidated);
		}

		[Obsolete(InternalMessages.MethodIsObsoleteAndWillBeRemoved2019R2 + OBSOLETE_USE_VIEW)]
		protected virtual IEnumerable SelectDelegate()
		{
			var primaryCache = _Graph.Caches[_itemType];
			var primaryRecord = GetPrimaryRow();
			var currentAddressId = primaryCache.GetValue(primaryRecord, _addressIdFieldOrdinal);

			var result = (Address)PXSelect<Address,
				Where<Address.addressID, Equal<Required<Address.addressID>>>>.
				Select(_Graph, currentAddressId);
			yield return result;
		}

		protected Type ItemType => _itemType;

		[Obsolete(InternalMessages.MethodIsObsoleteAndWillBeRemoved2019R2 + OBSOLETE_USE_VIEW)]
		protected virtual object GetPrimaryRow() => throw new NotImplementedException();

		private void RowInsertedHandler(PXCache sender, PXRowInsertedEventArgs e)
		{
			var row = e.Row;
			var asMain = sender.GetValue(row, _asMainFieldOrdinal);
			var accountId = sender.GetValue(row, _accountIdFieldOrdinal);
			var account = accountId.
				With(_ => (BAccount)PXSelect<BAccount,
					Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
				Select(_Graph, _));
			var accountAddressId = account.With(_ => _.DefAddressID);
			var accountAddress = accountAddressId.
				With<int?, Address>(_ => (Address)PXSelect<Address,
					Where<Address.addressID, Equal<Required<Address.addressID>>>>.
				Select(sender.Graph, _));
			var currentAddressId = sender.GetValue(row, _addressIdFieldOrdinal);

			if (accountAddress == null)
			{
				asMain = false;
				sender.SetValue(row, _asMainFieldOrdinal, false);
			}

			var addressCache = sender.Graph.Caches[typeof(Address)];
			if (accountAddress != null && true.Equals(asMain))
			{
				if (currentAddressId != null && !object.Equals(currentAddressId, accountAddressId))
				{
					var currentAddress = (Address)PXSelect<Address,
						Where<Address.addressID, Equal<Required<Address.addressID>>>>.
						Select(sender.Graph, currentAddressId);
					var oldDirty = addressCache.IsDirty;
					addressCache.Delete(currentAddress);
					addressCache.IsDirty = oldDirty;
				}
				sender.SetValue(row, _addressIdFieldOrdinal, accountAddressId);
			}
			else
			{
				if (currentAddressId == null || object.Equals(currentAddressId, accountAddressId))
				{
					var oldDirty = addressCache.IsDirty;
					Address addr;
					if (accountAddress != null)
					{
						addr = (Address)addressCache.CreateCopy(accountAddress);
					}
					else
					{
						addr = (Address)addressCache.CreateInstance();
					}
					addr.AddressID = null;
					addr.BAccountID = (int?)accountId;
					addr = (Address)addressCache.Insert(addr);

					sender.SetValue(row, _addressIdFieldOrdinal, addr.AddressID);
					addressCache.IsDirty = oldDirty;
				}
			}
		}

		private void RowUpdatingHandler(PXCache sender, PXRowUpdatingEventArgs e)
		{
			var row = e.NewRow;
			var oldRow = e.Row;

			var asMain = sender.GetValue(row, _asMainFieldOrdinal);
			var oldAsMain = sender.GetValue(oldRow, _asMainFieldOrdinal);

			var addressId = sender.GetValue(row, _addressIdFieldOrdinal);

			var accountId = sender.GetValue(row, _accountIdFieldOrdinal);
			var account = accountId.
				With(_ => (BAccount)PXSelect<BAccount,
					Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
				Select(_Graph, _));
			var accountAddressId = account.With(_ => _.DefAddressID);
			var accountAddress = accountAddressId.
				With<int?, Address>(_ => (Address)PXSelect<Address,
					Where<Address.addressID, Equal<Required<Address.addressID>>>>.
				Select(sender.Graph, _));

			var oldAccountId = sender.GetValue(oldRow, _accountIdFieldOrdinal);
			if (!object.Equals(accountId, oldAccountId))
			{
				var oldAddressId = sender.GetValue(row, _addressIdFieldOrdinal);
				var oldAccount = oldAccountId.
					With(_ => (BAccount)PXSelect<BAccount,
						Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
					Select(_Graph, _));
				var oldAccountAddressId = oldAccount.With(_ => _.DefAddressID);
				var oldAccountAddress = oldAccountAddressId.
					With<int?, Address>(_ => (Address)PXSelect<Address,
						Where<Address.addressID, Equal<Required<Address.addressID>>>>.
					Select(sender.Graph, _));
				oldAsMain = oldAccountAddress != null && object.Equals(oldAddressId, oldAccountAddressId);
				if (true.Equals(oldAsMain))
				{
					if (accountAddressId == null)
					{
						var oldDirty = _Graph.Caches[typeof(Address)].IsDirty;

						Address addr = (Address)_Graph.Caches[typeof(Address)].CreateCopy(oldAccountAddress);

						addr.AddressID = null;
						addr.BAccountID = (int?)accountId;
						addr = (Address)_Graph.Caches[typeof(Address)].Insert(addr);

						sender.SetValue(row, _addressIdFieldOrdinal, addr.AddressID);
						sender.SetValue(row, _asMainFieldOrdinal, false);
						_Graph.Caches[typeof(Address)].IsDirty = oldDirty;
						addressId = addr.AddressID;
					}
					else
					{
						asMain = true;
						addressId = accountAddressId;
						sender.SetValue(row, _addressIdFieldOrdinal, accountAddressId);
					}
				}
			}

			if (true.Equals(asMain))
			{

				if (accountAddress == null)
				{
					asMain = false;
					sender.SetValue(row, _asMainFieldOrdinal, false);
				}
			}

			if (!object.Equals(asMain, oldAsMain))
			{
				if (true.Equals(asMain))
				{
					var oldAddressId = sender.GetValue(row, _addressIdFieldOrdinal);
					var oldAddress = ((int?)oldAddressId).
						With<int?, Address>(_ => (Address)PXSelect<Address,
							Where<Address.addressID, Equal<Required<Address.addressID>>>>.
						Select(sender.Graph, _));
					if(sender.Graph.Caches<Address>().GetStatus(oldAddress) == PXEntryStatus.Inserted)
						sender.Graph.Caches<Address>().Delete(oldAddress);

					sender.SetValue(row, _addressIdFieldOrdinal, accountAddressId);
				}
				else
				{
					if (object.Equals(accountAddressId, addressId))
						sender.SetValue(row, _addressIdFieldOrdinal, null);
				}
			}
		}

		private void RowUpdatedHandler(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var row = e.Row;
			var oldRow = e.OldRow;

			var accountId = sender.GetValue(row, _accountIdFieldOrdinal);
			var oldAccountId = sender.GetValue(oldRow, _accountIdFieldOrdinal);

			var addressId = sender.GetValue(row, _addressIdFieldOrdinal);
			var oldAddressId = sender.GetValue(oldRow, _addressIdFieldOrdinal);

			var addressCache = _Graph.Caches[typeof(Address)];
			if (!object.Equals(addressId, oldAddressId))
			{
				var account = accountId.
					With(_ => (BAccount)PXSelect<BAccount,
						Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
					Select(_Graph, _));
				var accountAddressId = account.With(_ => _.DefAddressID);
				var accountWithDefAddress = oldAddressId.
					With(_ => (BAccount)PXSelect<BAccount,
						Where<BAccount.defAddressID, Equal<Required<BAccount.defAddressID>>>>.
					Select(_Graph, _));
				if (accountWithDefAddress == null)
				{
					var oldAddress = oldAddressId.
						With(_ => (Address)PXSelect<Address,
							Where<Address.addressID, Equal<Required<Address.addressID>>>>.
						Select(_Graph, _));
					if (oldAddress != null)
					{
						var oldIsDirty = addressCache.IsDirty;
						addressCache.Delete(oldAddress);
						addressCache.IsDirty = oldIsDirty;
					}
				}

				if (addressId == null)
				{
					var oldDirty = addressCache.IsDirty;
					Address addr;
					var accountAddress = accountAddressId.
						With<int?, Address>(_ => (Address)PXSelect<Address,
							Where<Address.addressID, Equal<Required<Address.addressID>>>>.
						Select(_Graph, _));
					if (accountAddress != null && object.Equals(accountAddressId, oldAddressId))
					{
						addr = (Address)addressCache.CreateCopy(accountAddress);
					}
					else
					{
						addr = (Address)addressCache.CreateInstance();
					}
					if (addr != null)
					{
						addr.AddressID = null;
						addr.BAccountID = (int?)accountId;
						addr = (Address)addressCache.Insert(addr);

						sender.SetValue(row, _addressIdFieldOrdinal, addr.AddressID);
						sender.SetValue(row, _asMainFieldOrdinal, false);
						addressCache.IsDirty = oldDirty;
						addressId = addr.AddressID;
					}
				}
			}
			else if (addressId == null)
			{
				var oldDirty = addressCache.IsDirty;
				var addr = (Address)addressCache.CreateInstance();
				addr.AddressID = null;
				addr.BAccountID = (int?)accountId;
				addr = (Address)addressCache.Insert(addr);

				sender.SetValue(row, _addressIdFieldOrdinal, addr.AddressID);
				sender.SetValue(row, _asMainFieldOrdinal, false);
				addressCache.IsDirty = oldDirty;
			}
			else if (!object.Equals(accountId, oldAccountId))
			{
				bool oldIsDirty = addressCache.IsDirty;

				Address address = addressId.
					With(_ => (Address)PXSelect<Address,
						Where<Address.addressID, Equal<Required<Address.addressID>>>>.
						Select(_Graph, _));

				address.BAccountID = (int?)accountId;
				addressCache.Update(address);

				addressCache.IsDirty = oldIsDirty;
			}
		}

		[PXUIField(DisplayName = CS.Messages.ValidateAddress, FieldClass = CS.Messages.ValidateAddress)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		protected virtual IEnumerable ValidateAddress(PXAdapter adapter)
		{
			var graph = adapter.View.Graph;
			var primaryCache = graph.Caches[_itemType];
			var primaryRecord = primaryCache.Current;
			if (primaryRecord != null)
			{
				var addressId = primaryCache.GetValue(primaryRecord, _addressIdFieldOrdinal);
				var address = addressId.With(_ => (Address)PXSelect<Address,
						Where<Address.addressID, Equal<Required<Address.addressID>>>>.
						Select(graph, _));
				if (address != null && address.IsValidated != true)
					PXAddressValidator.Validate<Address>(graph, address, true);
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.ViewOnMap, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton()]
		protected virtual IEnumerable ViewOnMap(PXAdapter adapter)
		{
			var graph = adapter.View.Graph;
			var primaryCache = graph.Caches[_itemType];
			var primaryRecord = primaryCache.Current;
			if (primaryRecord != null)
			{
				var currentAddressId = primaryCache.GetValue(primaryRecord, _addressIdFieldOrdinal);
				var currentAddress = currentAddressId.With(_ => (Address)PXSelect<Address,
						Where<Address.addressID, Equal<Required<Address.addressID>>>>.
						Select(graph, _));
				if (currentAddress != null)
					BAccountUtility.ViewOnMap(currentAddress);
			}
			return adapter.Get();
		}
	}

	#endregion

	#region AddressSelect

	[Obsolete("This class has been deprecated and will be removed in Acumatica ERP 2021R1.")]
	public class AddressSelect<TAddressIdField, TAsMainField, TAccountIdField> : AddressSelectBase
		where TAddressIdField : IBqlField
		where TAsMainField : IBqlField
		where TAccountIdField : IBqlField
	{
		public AddressSelect(PXGraph graph)
			: base(graph)
		{
		}

		protected override Type AccountIdField => typeof(TAccountIdField);

		protected override Type AsMainField => typeof(TAsMainField);

		protected override Type AddressIdField => typeof(TAddressIdField);

		protected override Type IncorrectPersistableDAC => typeof(TAddressIdField);

		protected override PXView GetView(PXGraph graph)
		{
			return new PXView(graph, false,
				new Select<Address, Where<Address.addressID, Equal<Current<TAddressIdField>>>>());
		}

		[Obsolete(InternalMessages.MethodIsObsoleteAndWillBeRemoved2019R2)]
		protected override object GetPrimaryRow()
		{
			return _Graph.Caches[ItemType].Current;
		}
	}

	#endregion

	#region PXOwnerFilteredSelect

	[Obsolete]
	public class PXOwnerFilteredSelect<TFilter, TSelect, TGroupID, TOwnerID> : PXSelectBase
		where TFilter : OwnedFilter, new()
		where TSelect : IBqlSelect
		where TGroupID : IBqlField
		where TOwnerID : IBqlField
	{
		private BqlCommand _command;
		private Type _selectTarget;
		private Type _newRecordTarget;

		public PXOwnerFilteredSelect(PXGraph graph)
			: this(graph, false)
		{

		}

		protected PXOwnerFilteredSelect(PXGraph graph, bool readOnly)
			: base()
		{
			_Graph = graph;

			InitializeView(readOnly);
			InitializeSelectTarget();
			AppendActions();
			AppendEventHandlers();
		}

		public Type NewRecordTarget
		{
			get { return _newRecordTarget; }
			set
			{
				if (value != null)
				{
					if (!typeof(PXGraph).IsAssignableFrom(value))
						throw new ArgumentException(string.Format("{0} is excpected", typeof(PXGraph).GetLongName()), "value");
					if (value.GetConstructor(new Type[0]) == null)
						throw new ArgumentException("Default constructor is excpected", "value");
				}
				_newRecordTarget = value;
			}
		}

		private void AppendEventHandlers()
		{
			_Graph.RowSelected.AddHandler<TFilter>(RowSelectedHandler);
		}

		private void RowSelectedHandler(PXCache sender, PXRowSelectedEventArgs e)
		{
			var me = true.Equals(sender.GetValue(e.Row, typeof(OwnedFilter.myOwner).Name));
			var myGroup = true.Equals(sender.GetValue(e.Row, typeof(OwnedFilter.myWorkGroup).Name));

			PXUIFieldAttribute.SetEnabled(sender, e.Row, typeof(OwnedFilter.ownerID).Name, !me);
			PXUIFieldAttribute.SetEnabled(sender, e.Row, typeof(OwnedFilter.workGroupID).Name, !myGroup);
		}

		private void AppendActions()
		{
			_Graph.Initialized += sender =>
			{
				var name = _Graph.ViewNames[View] + "_AddNew";
				PXNamedAction.AddAction(_Graph, typeof(TFilter), name, Messages.AddNew, new PXButtonDelegate(AddNewHandler));
			};
		}

		[PXButton(Tooltip = Messages.AddNewRecordToolTip, CommitChanges = true)]
		private IEnumerable AddNewHandler(PXAdapter adapter)
		{
			var filterCache = _Graph.Caches[typeof(TFilter)];
			var currentFilter = filterCache.Current;
			if (NewRecordTarget != null && _selectTarget != null && currentFilter != null)
			{
				var currentOwnerId = filterCache.GetValue(currentFilter, typeof(OwnedFilter.ownerID).Name);
				var currentWorkgroupId = filterCache.GetValue(currentFilter, typeof(OwnedFilter.workGroupID).Name);

				var targetGraph = (PXGraph)PXGraph.CreateInstance(NewRecordTarget);
				var targetCache = targetGraph.Caches[_selectTarget];
				var row = targetCache.Insert();
				var newRow = targetCache.CreateCopy(row);

				EPCompanyTreeMember member = PXSelect<EPCompanyTreeMember,
												Where<EPCompanyTreeMember.contactID, Equal<Required<OwnedFilter.ownerID>>,
												  And<EPCompanyTreeMember.workGroupID, Equal<Required<OwnedFilter.workGroupID>>>>>.
				Select(targetGraph, currentOwnerId, currentWorkgroupId);
				if (member == null) currentOwnerId = null;

				targetCache.SetValue(newRow, typeof(TGroupID).Name, currentWorkgroupId);
				targetCache.SetValue(newRow, typeof(TOwnerID).Name, currentOwnerId);
				targetCache.Update(newRow);
				PXRedirectHelper.TryRedirect(targetGraph, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		private void InitializeSelectTarget()
		{
			var selectTabels = _command.GetTables();
			if (selectTabels == null || selectTabels.Length == 0)
				throw new Exception("Primary table of given select command cannot be found");

			_selectTarget = selectTabels[0];
			_Graph.EnsureCachePersistence(_selectTarget);
		}

		private void InitializeView(bool readOnly)
		{
			_command = CreateCommand();
			View = new PXView(_Graph, readOnly, _command, new PXSelectDelegate(Handler));
		}

		private IEnumerable Handler()
		{
			var filterCache = _Graph.Caches[typeof(TFilter)];
			var currentFilter = filterCache.Current;
			if (filterCache.Current == null) return new object[0];

			var parameters = GetParameters(filterCache, currentFilter);

			return _Graph.QuickSelect(_command, parameters);
		}

		private static object[] GetParameters(PXCache filterCache, object currentFilter)
		{
			var currentOwnerId = filterCache.GetValue(currentFilter, typeof(OwnedFilter.ownerID).Name);
			var currentWorkgroupId = filterCache.GetValue(currentFilter, typeof(OwnedFilter.workGroupID).Name);
			var currentMyWorkgroup = filterCache.GetValue(currentFilter, typeof(OwnedFilter.myWorkGroup).Name);
			var parameters = new object[]
				{
					currentOwnerId, currentOwnerId,
					currentMyWorkgroup, currentMyWorkgroup,
					currentWorkgroupId, currentWorkgroupId, currentMyWorkgroup
				};
			return parameters;
		}

		private static BqlCommand CreateCommand()
		{
			var command = BqlCommand.CreateInstance(typeof(TSelect));
			var additionalCondition = BqlCommand.Compose(
				typeof(Where2<Where<Required<OwnedFilter.ownerID>, IsNull,
								Or<Required<OwnedFilter.ownerID>, Equal<TOwnerID>>>,
							And<
								Where2<
									Where<Required<OwnedFilter.myWorkGroup>, IsNull,
										Or<Required<OwnedFilter.myWorkGroup>, Equal<False>>>,
									And2<
										Where<Required<OwnedFilter.workGroupID>, IsNull,
											Or<TGroupID, Equal<Required<OwnedFilter.workGroupID>>>>,
										Or<Required<OwnedFilter.myWorkGroup>, Equal<True>,
									And<TGroupID, IsWorkgroupOfContact<Current<AccessInfo.contactID>>>>>>>>));
			return command.WhereAnd(additionalCondition);
		}
	}

	#endregion

	#region PXOwnerFilteredSelectReadonly

	[Obsolete]
	public class PXOwnerFilteredSelectReadonly<TFilter, TSelect, TGroupID, TOwnerID>
		: PXOwnerFilteredSelect<TFilter, TSelect, TGroupID, TOwnerID>
		where TFilter : OwnedFilter, new()
		where TSelect : IBqlSelect
		where TGroupID : IBqlField
		where TOwnerID : IBqlField
	{
		public PXOwnerFilteredSelectReadonly(PXGraph graph)
			: base(graph, true)
		{
		}
	}

	#endregion

	#region CRLastNameDefaultAttribute

	internal sealed class CRLastNameDefaultAttribute : PXEventSubscriberAttribute, IPXRowPersistingSubscriber
	{
		public void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var contactType = sender.GetValue(e.Row, typeof(Contact.contactType).Name);
			var val = sender.GetValue(e.Row, _FieldOrdinal) as string;
			if (contactType != null && contactType.Equals(ContactTypesAttribute.Person) && string.IsNullOrWhiteSpace(val))
			{
				if (sender.RaiseExceptionHandling(_FieldName, e.Row, null, new PXSetPropertyKeepPreviousException(PXMessages.LocalizeFormat(ErrorMessages.FieldIsEmpty, $"[{_FieldName}]"))))
				{
					throw new PXRowPersistingException(_FieldName, null, ErrorMessages.FieldIsEmpty, _FieldName);
				}
			}
		}
	}

	#endregion

	#region CRContactBAccountDefaultAttribute

	internal sealed class CRContactBAccountDefaultAttribute : PXEventSubscriberAttribute, IPXRowInsertingSubscriber, IPXRowUpdatingSubscriber, IPXRowPersistingSubscriber, IPXRowPersistedSubscriber
	{
		private Dictionary<object, object> _persistedItems;

		public override void CacheAttached(PXCache sender)
		{
			_persistedItems = new Dictionary<object, object>();
			sender.Graph.RowPersisting.AddHandler(typeof(BAccount), SourceRowPersisting);
		}

		public void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			SetDefaultValue(sender, e.Row);
		}

		public void RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			SetDefaultValue(sender, e.Row);
		}

		private void SetDefaultValue(PXCache sender, object row)
		{
			if (IsLeadOrPerson(sender, row)) return;

			var val = sender.GetValue(row, _FieldOrdinal);
			if (val != null) return;

			PXCache cache = sender.Graph.Caches[typeof(BAccount)];
			if (cache.Current != null)
			{
				var newValue = cache.GetValue(cache.Current, typeof(BAccount.bAccountID).Name);
				sender.SetValue(row, _FieldOrdinal, newValue);
			}
		}

		public void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (IsLeadOrPerson(sender, e.Row)) return;

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && true ||
				(e.Operation & PXDBOperation.Command) == PXDBOperation.Update && true)
			{
				object key = sender.GetValue(e.Row, _FieldOrdinal);
				if (key != null)
				{
					object parent;
					if (_persistedItems.TryGetValue(key, out parent))
					{
						key = sender.Graph.Caches[typeof(BAccount)].GetValue(parent, typeof(BAccount.bAccountID).Name);
						sender.SetValue(e.Row, _FieldOrdinal, key);
						if (key != null)
						{
							_persistedItems[key] = parent;
						}
					}
				}
			}
			if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && true ||
				 (e.Operation & PXDBOperation.Command) == PXDBOperation.Update && true) &&
				sender.GetValue(e.Row, _FieldOrdinal) == null)
			{
				throw new PXRowPersistingException(_FieldName, null, PXMessages.LocalizeFormatNoPrefixNLA(Messages.EmptyValueErrorFormat, _FieldName));
			}
		}

		public void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (IsLeadOrPerson(sender, e.Row)) return;

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Aborted)
			{
				object key = sender.GetValue(e.Row, _FieldOrdinal);
				if (key != null)
				{
					object parent;
					if (_persistedItems.TryGetValue(key, out parent))
					{
						var sourceField = typeof(BAccount.bAccountID).Name;
						sender.SetValue(e.Row, _FieldOrdinal, sender.Graph.Caches[typeof(BAccount)].GetValue(parent, sourceField));
					}
				}
			}
		}

		private void SourceRowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (IsLeadOrPerson(sender, e.Row)) return;

			var sourceField = typeof(BAccount.bAccountID).Name;
			object key = sender.GetValue(e.Row, sourceField);
			if (key != null)
				_persistedItems[key] = e.Row;
		}

		private bool IsLeadOrPerson(PXCache sender, object row)
		{
			var contactType = sender.GetValue(row, typeof(Contact.contactType).Name);
			return contactType != null &&
				(contactType.Equals(ContactTypesAttribute.Lead) ||
					contactType.Equals(ContactTypesAttribute.Person));
		}
	}

	#endregion

	#region BAccountType Attribute
	public class BAccountType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { VendorType, CustomerType, CombinedType, ProspectType, BranchType },
				new string[] { Messages.VendorType, Messages.CustomerType, Messages.CombinedType, Messages.ProspectType, Messages.BranchType })
			{; }

			public override void CacheAttached(PXCache sender)
			{
				if (!sender.Graph.IsContractBasedAPI)
				{
					_AllowedLabels = new[] { Messages.VendorType, Messages.CustomerType, Messages.CombinedType, Messages.BusinessAccountType, Messages.BranchType };
					_NeutralAllowedLabels = _AllowedLabels;
				}
				base.CacheAttached(sender);
			}
		}

		public class SalesPersonTypeListAttribute : PXStringListAttribute
		{
			public SalesPersonTypeListAttribute()
				: base(
				new string[] { VendorType, EmployeeType },
				new string[] { Messages.VendorType, Messages.EmployeeType })
			{; }
		}

		public const string VendorType = "VE";
		public const string CustomerType = "CU";
		public const string CombinedType = "VC";
		public const string EmployeeType = "EP";
		public const string EmpCombinedType = "EC";
		public const string ProspectType = "PR";
		[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica8)]
		public const string CompanyType = "CP";
		public const string BranchType = "CP";
		public const string OrganizationType = "OR";
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2020R2)]
		public const string OrganizationBranchCombinedType = "OB";

		public class vendorType : PX.Data.BQL.BqlString.Constant<vendorType>
		{
			public vendorType() : base(VendorType) {; }
		}
		public class customerType : PX.Data.BQL.BqlString.Constant<customerType>
		{
			public customerType() : base(CustomerType) {; }
		}
		public class combinedType : PX.Data.BQL.BqlString.Constant<combinedType>
		{
			public combinedType() : base(CombinedType) {; }
		}
		public class employeeType : PX.Data.BQL.BqlString.Constant<employeeType>
		{
			public employeeType() : base(EmployeeType) {; }
		}
		public class empCombinedType : PX.Data.BQL.BqlString.Constant<empCombinedType>
		{
			public empCombinedType() : base(EmpCombinedType) {; }
		}
		public class prospectType : PX.Data.BQL.BqlString.Constant<prospectType>
		{
			public prospectType() : base(ProspectType) {; }
		}

		[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica8)]
		public class companyType : PX.Data.BQL.BqlString.Constant<companyType>
		{
			public companyType() : base(CompanyType) { }
		}

		public class branchType : PX.Data.BQL.BqlString.Constant<branchType>
		{
			public branchType() : base(BranchType) {; }
		}
		public class organizationType : PX.Data.BQL.BqlString.Constant<organizationType>
		{
			public organizationType() : base(OrganizationType) {; }
		}
	}
	#endregion

	#region IActivityMaint

	public interface IActivityMaint
	{
		void CancelRow(CRActivity row);
		void CompleteRow(CRActivity row);
	}

	#endregion

	#region SelectContactEmailSync

	public class SelectContactEmailSyncBase<TCommand> : PXSelectBase<Contact, SelectContactEmailSyncBase<TCommand>.Config>
		where TCommand : BqlCommand, new()
	{
		public class Config : IViewConfig
		{
			public BqlCommand GetCommand() => new TCommand();
			public Boolean IsReadOnly => false;
		}

		public SelectContactEmailSyncBase(PXGraph graph, Delegate handler)
			: base(graph, handler)
		{
			AddFieldUpdatedHandlers();
		}

		public SelectContactEmailSyncBase(PXGraph graph) : base(graph)
		{
			AddFieldUpdatedHandlers();
		}

		private void AddFieldUpdatedHandlers()
		{
			_Graph.FieldUpdated.AddHandler(typeof(Contact), typeof(Contact.eMail).Name, FieldUpdated<Contact.eMail, Users.email>);
			_Graph.FieldUpdated.AddHandler(typeof(Contact), typeof(Contact.firstName).Name, FieldUpdated<Contact.firstName, Users.firstName>);
			_Graph.FieldUpdated.AddHandler(typeof(Contact), typeof(Contact.lastName).Name, FieldUpdated<Contact.lastName, Users.lastName>);
		}

		protected virtual void FieldUpdated<TSrcField, TDstField>(PXCache sender, PXFieldUpdatedEventArgs e)
			where TSrcField : IBqlField
			where TDstField : IBqlField
		{
			Contact row = (Contact)e.Row;
			Users user = PXSelect<Users, Where<Users.pKID, Equal<Current<Contact.userID>>>>.SelectSingleBound(_Graph, new object[] { row });
			if (user != null)
			{
				PXCache usercache = _Graph.Caches[typeof(Users)];
				usercache.SetValue<TDstField>(user, sender.GetValue<TSrcField>(row));
				usercache.Update(user);
			}
		}
	}

	public class SelectContactEmailSync<TWhere> : SelectContactEmailSyncBase<Select<Contact, TWhere>>
	where TWhere : IBqlWhere, new()
	{
		public new class Config : IViewConfig
		{
			public BqlCommand GetCommand() => new Select<Contact, TWhere>();
			public Boolean IsReadOnly => false;
		}

		public SelectContactEmailSync(PXGraph graph, Delegate handler)
			: base(graph, handler)
		{
		}

		public SelectContactEmailSync(PXGraph graph) : base(graph)
		{
		}

		protected override void FieldUpdated<TSrcField, TDstField>(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			base.FieldUpdated<TSrcField, TDstField>(sender, e);
		}
	}

	#endregion

	#region CRMMarketingSubscriptions

	/// <exclude/>
	[Obsolete]
	public class CRMMarketingContactSubscriptions<TPrimary, TKey> : CRMMarketingSubscriptions
		where TPrimary : IBqlTable, new()
		where TKey : IBqlField
	{
		public CRMMarketingContactSubscriptions(PXGraph graph)
			: base(graph)
		{
			View = new PXView(_Graph, false,
				new Select2<
						CRMarketingListMember,
					InnerJoin<CRMarketingList,
						On<CRMarketingList.marketingListID, Equal<CRMarketingListMember.marketingListID>>>>(),
			new PXSelectDelegate(subscriptions));
		}

		protected override PXResultset<CRMarketingListMember, CRMarketingList> GetFromStatic()
		{
			PXResultset<CRMarketingListMember, CRMarketingList, Contact> result = new PXResultset<CRMarketingListMember, CRMarketingList, Contact>();

			PXSelectJoin<
					CRMarketingListMember,
				InnerJoin<CRMarketingList,
					On<CRMarketingList.marketingListID, Equal<CRMarketingListMember.marketingListID>,
					And<CRMarketingList.type, Equal<CRMarketingList.type.@static>,
					And<CRMarketingList.status, Equal<CRMarketingList.status.active>>>>,
				InnerJoin<Contact,
					On<Contact.contactID, Equal<CRMarketingListMember.contactID>>>>,
				Where<
					CRMarketingListMember.contactID, Equal<Current<TKey>>>>
				.Select(_Graph)
				.ForEach(_ => result.Add(_));

			return result;
		}

		protected override void PrepareFilter()
		{
			var current = (Contact)_Graph.Caches[typeof(TPrimary)].Current;

			if (current == null)
				return;

			PXView.Filters.Add(new PXFilterRow()
			{
				OrOperator = false,
				OpenBrackets = 1,
				DataField = nameof(Contact) + "__" + nameof(Contact.ContactID),
				Condition = PXCondition.EQ,
				Value = current.ContactID,
				CloseBrackets = 1
			});
		}
	}

	[Obsolete]
	public class CRMMarketingBAccountSubscriptions : CRMMarketingSubscriptions
	{
		public CRMMarketingBAccountSubscriptions(PXGraph graph)
			: base(graph)
		{
			View = new PXView(_Graph, false,
				new Select3<
						CRMarketingListMember,
					InnerJoin<CRMarketingList,
						On<CRMarketingList.marketingListID, Equal<CRMarketingListMember.marketingListID>>,
					InnerJoin<Contact,
						On<Contact.contactID, Equal<CRMarketingListMember.contactID>>>>,
					OrderBy<
						Desc<Contact.contactPriority>>>(),
			new PXSelectDelegate(subscriptions));
		}

		protected override PXResultset<CRMarketingListMember, CRMarketingList> GetFromStatic()
		{
			PXResultset<CRMarketingListMember, CRMarketingList, Contact> result = new PXResultset<CRMarketingListMember, CRMarketingList, Contact>();

			PXSelectJoin<
					CRMarketingListMember,
				InnerJoin<CRMarketingList,
					On<CRMarketingList.marketingListID, Equal<CRMarketingListMember.marketingListID>,
					And<CRMarketingList.type, Equal<CRMarketingList.type.@static>,
					And<CRMarketingList.status, Equal<CRMarketingList.status.active>>>>,
				InnerJoin<Contact,
					On<Contact.contactID, Equal<CRMarketingListMember.contactID>>>>,
				Where<
					Contact.bAccountID, Equal<Current<BAccount.bAccountID>>>>
				.Select(_Graph)
				.ForEach(_ => result.Add(_));

			return result;
		}

		protected override void PrepareFilter()
		{
			var current = (BAccount)_Graph.Caches[typeof(BAccount)].Current;

			if (current == null)
				return;

			PXView.Filters.Add(new PXFilterRow()
			{
				OrOperator = false,
				OpenBrackets = 1,
				DataField = nameof(Contact) + "__" + nameof(Contact.BAccountID),
				Condition = PXCondition.EQ,
				Value = current.BAccountID,
				CloseBrackets = 1
			});
		}
	}

	[Obsolete]
	public abstract class CRMMarketingSubscriptions : PXSelectBase<CRMarketingListMember>
	{
		public CRMMarketingSubscriptions(PXGraph graph)
		{
			_Graph = graph;
		}

		protected virtual IEnumerable subscriptions()
		{
			PXResultset<CRMarketingListMember, CRMarketingList, Contact> subscriptions = new PXResultset<CRMarketingListMember, CRMarketingList, Contact>();

			var primaryCache = _Graph.Views[_Graph.PrimaryView]?.Cache;

			if (primaryCache?.Current != null && primaryCache.GetStatus(primaryCache.Current) != PXEntryStatus.Inserted)
			{
				bool oldDirty = _Graph.Caches[typeof(CRMarketingListMember)].IsDirty;

				GetFromStatic().ForEach(_ => subscriptions.Add(_));
				GetFromDynamic().ForEach(_ => subscriptions.Add(_));

				_Graph.Caches[typeof(CRMarketingListMember)].IsDirty = oldDirty;
			}

			return subscriptions;
		}

		protected abstract PXResultset<CRMarketingListMember, CRMarketingList> GetFromStatic();

		private PXResultset<CRMarketingListMember, CRMarketingList> GetFromDynamic()
		{
			PXResultset<CRMarketingListMember, CRMarketingList, Contact> result = new PXResultset<CRMarketingListMember, CRMarketingList, Contact>();

			List<CRMarketingList> lists = new List<CRMarketingList>();

			PXSelect<
					CRMarketingList,
				Where<
					CRMarketingList.type, Equal<CRMarketingList.type.dynamic>,
					And<CRMarketingList.status, Equal<CRMarketingList.status.active>>>>
				.Select(_Graph)
				.ForEach(_ => lists.Add(_));

			foreach (CRMarketingList list in lists)
			{
				var graph = PXGraph.CreateInstance<CRMarketingListMaint>();
				graph.MailLists.Current = list;

				PrepareFilter();

				int startRow = PXView.StartRow;
				int totalRows = 0;
				foreach (PXResult<CRMarketingListMember, Contact> res in graph.ListMembers.View.Select(PXView.Currents, new object[] { }, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows))
				{
					CRMarketingListMember _CRMarketingListMember = res[typeof(CRMarketingListMember)] as CRMarketingListMember;
					result.Add(new PXResult<CRMarketingListMember, CRMarketingList, Contact>((CRMarketingListMember)res, list, (Contact)res));
					if (_Graph.Caches[typeof(CRMarketingListMember)].Locate(_CRMarketingListMember) == null)
					{
						_Graph.Caches[typeof(CRMarketingListMember)].SetStatus(_CRMarketingListMember, PXEntryStatus.Held);
					}
				}
			}

			return result;
		}

		protected abstract void PrepareFilter();
	}

	#endregion

	#region PXVirtualTableView

	[Obsolete]
	public class PXVirtualTableView<TTable> : PXView
		where TTable : IBqlTable
	{
		public PXVirtualTableView(PXGraph graph)
			: base(graph, false, new Select<TTable>())
		{
			_Delegate = (PXSelectDelegate)Get;
			_Graph.Defaults[_Graph.Caches[typeof(TTable)].GetItemType()] = getFilter;
			_Graph.RowPersisting.AddHandler(typeof(TTable), persisting);
		}
		public IEnumerable Get()
		{
			PXCache cache = _Graph.Caches[typeof(TTable)];
			cache.AllowInsert = true;
			cache.AllowUpdate = true;
			object curr = cache.Current;
			if (curr != null && cache.Locate(curr) == null)
			{
				try
				{
					curr = cache.Insert(curr);
				}
				catch
				{
					cache.SetStatus(curr, PXEntryStatus.Inserted);
				}
			}
			yield return curr;
			cache.IsDirty = false;
		}

		private TTable current;
		private bool _inserting = false;
		private object getFilter()
		{
			PXCache cache = _Graph.Caches[typeof(TTable)];

			if (!_inserting)
			{
				try
				{
					_inserting = true;
					if (current == null)
					{
						current = (TTable)(cache.Insert() ?? cache.Locate(cache.CreateInstance()));
						cache.IsDirty = false;
					}
					else if (cache.Locate(current) == null)
					{
						try
						{
							current = (TTable)cache.Insert(current);
						}
						catch
						{
							cache.SetStatus(current, PXEntryStatus.Inserted);
						}
						cache.IsDirty = false;
					}
				}
				finally
				{
					_inserting = false;
				}
			}
			return current;
		}
		private static void persisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}

		public bool VerifyRequired()
		{
			return VerifyRequired(false);
		}
		public virtual bool VerifyRequired(bool suppressError)
		{
			Cache.RaiseRowSelected(Cache.Current);
			bool result = true;
			PXRowPersistingEventArgs e = new PXRowPersistingEventArgs(PXDBOperation.Insert, Cache.Current);
			foreach (string field in Cache.Fields)
			{
				foreach (PXDefaultAttribute defAttr in Cache.GetAttributes(Cache.Current, field).OfType<PXDefaultAttribute>())
				{
					defAttr.RowPersisting(Cache, e);
					bool error = !string.IsNullOrEmpty(PXUIFieldAttribute.GetError(Cache, Cache.Current, field));
					if (error) result = false;

					if (suppressError && error)
					{
						Cache.RaiseExceptionHandling(field, Cache.Current, null, null);
						return false;
					}
				}
			}
			return result;
		}

	}

	#endregion

	#region WhereEqualNotNull

	public class WhereEqualNotNull<TField, TFieldCurrent> : IBqlWhere
			where TField : IBqlOperand
			where TFieldCurrent : IBqlField
	{
		private IBqlCreator whereEqual = new Where<TField, Equal<Current2<TFieldCurrent>>>();
		private IBqlCreator whereNull = new Where<Current2<TFieldCurrent>, IsNull>();

		private Type cacheType;

		public WhereEqualNotNull()
		{
			cacheType = typeof(TFieldCurrent).DeclaringType;
		}

		private IBqlCreator GetWhereClause(PXCache cache)
		{
			return cache?.GetValue<TFieldCurrent>(cache.Current) == null ? whereNull : whereEqual;
		}

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
		{
			var clause = GetWhereClause(graph?.Caches[cacheType]);
			return clause.AppendExpression(ref exp, graph, info, selection);
		}

		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			var clause = GetWhereClause(cache?.Graph.Caches[cacheType]);
			clause.Verify(cache, item, pars, ref result, ref value);
		}
	}

	#endregion
}
