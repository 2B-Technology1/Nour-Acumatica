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
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using PX.SM;
using PX.Web.UI;
using PX.Data;
using PX.Common;

namespace PX.Objects.AR.CCPaymentProcessing.CardsSynchronization
{
	///<summary>
	///Class allows to add schedule history row during credit cards synchronization. 
	///These rows are inserted into AUScheduleHistory table. This class adds hook methods on button "Add Schedule"
	///</summary>
	public class CCSyncFilteredProcessing<Table, FilterTable, Where, OrderBy> : PXFilteredProcessing<Table, FilterTable, Where, OrderBy>
		where FilterTable : class, IBqlTable, new()
		where Table : class, IBqlTable, new()
		where Where : IBqlWhere, new()
		where OrderBy : IBqlOrderBy, new()
	{
		Action beforeScheduleAdd;
		Action afterScheduleAdd;
		Action beforeScheduleProcessAll;

		public CCSyncFilteredProcessing(PXGraph graph) : base(graph) { }
		public CCSyncFilteredProcessing(PXGraph graph, Delegate handler) : base(graph,handler) { }

		[PXButton(ImageKey = Sprite.Main.AddNew, DisplayOnMainToolbar = false)]
		[PXUIField(DisplayName = "Add", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		protected override IEnumerable _ScheduleAdd_(PXAdapter adapter)
		{
			beforeScheduleAdd?.Invoke();
			IEnumerable ret = base._ScheduleAdd_(adapter);
			afterScheduleAdd?.Invoke();
			return ret;
		}

		public void SetBeforeScheduleProcessAllAction(Action beforeAction)
		{
			this.beforeScheduleProcessAll = beforeAction;
		}

		public void SetBeforeScheduleAddAction(Action beforeScheduleAdd)
		{
			this.beforeScheduleAdd = beforeScheduleAdd;
		}

		public void SetAfterScheduleAddAction(Action afterScheduleAdd)
		{
			this.afterScheduleAdd = afterScheduleAdd;
		}

		protected override bool startPendingProcess(List<Table> items)
		{
			AUSchedule schedule = PX.Common.PXContext.GetSlot<AUSchedule>();

			if (schedule == null)
			{
				return base.startPendingProcess(items);
			}

			_OuterView.Cache.IsDirty = false;
			List<Table> list = GetSelectedItems(_OuterView.Cache, items);

			PX.Common.PXContext.SetSlot<AUSchedule>(null);
			AUSchedule scheduleparam = schedule;

			if (_IsInstance)
			{
				//TODO: try finding better cancellation token
				ProcessSyncCC(_ProcessDelegate, list, scheduleparam, CancellationToken.None);
			}
			else
			{
				_Graph.LongOperationManager.StartOperation(cancellationToken => ProcessSyncCC(_ProcessDelegate, list, scheduleparam, cancellationToken));
			}

			schedule = null;
			return true;
		}

		protected void ProcessSyncCC(Action<List<Table>, CancellationToken> processor, List<Table> list, AUSchedule schedule, CancellationToken cancellationToken)
		{
			beforeScheduleProcessAll?.Invoke();
			PXLongOperation.SetCustomInfo(new List<SyncCCProcessingInfoEntry>(), ProcessingInfo.processingKey);
			list.Clear();
			_InProc = new PXResultset<Table>();
			base._ProcessScheduled(processor,list,schedule,cancellationToken);
			var histCache = _Graph.Caches[typeof(AUScheduleHistory)];
			List<SyncCCProcessingInfoEntry> infoList = PXLongOperation.GetCustomInfoForCurrentThread(ProcessingInfo.processingKey) as List<SyncCCProcessingInfoEntry>;

			if (infoList != null)
			{
				foreach (SyncCCProcessingInfoEntry infoEntry in infoList)
				{
					AUScheduleHistory hist = new PX.SM.AUScheduleHistory();
					hist.ExecutionResult = infoEntry.ProcessingMessage.Message;
					hist.ErrorLevel = (short)infoEntry.ProcessingMessage.ErrorLevel;
					hist.ScheduleID = schedule.ScheduleID;
					hist.ScreenID = schedule.ScreenID;
					var timeZone = PXTimeZoneInfo.FindSystemTimeZoneById(schedule.TimeZoneID);
					DateTime startUtc = PXTimeZoneInfo.UtcNow;
					DateTime start = PXTimeZoneInfo.ConvertTimeFromUtc(startUtc, timeZone);
					hist.ExecutionDate = start;
					hist.RefNoteID = infoEntry.NoteId;
					histCache.Insert(hist);
				}
			}

			histCache.Persist(PXDBOperation.Insert);
		}
	}
}
