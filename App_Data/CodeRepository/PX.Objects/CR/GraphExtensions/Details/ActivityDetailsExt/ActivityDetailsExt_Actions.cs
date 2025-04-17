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
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using PX.Data.EP;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.SM;
using System.Linq;
using System.Collections.Generic;
using PX.Common.Mail;
using PX.Data.SQLTree;
using PX.Objects.PM;
using PX.Reports;
using PX.TM;
using System.Reflection;

namespace PX.Objects.CR.Extensions
{
	public abstract class ActivityDetailsExt_Actions<TActivityDetailsExt, TGraph, TPrimaryEntity, TPrimaryEntity_NoteID>
		: ActivityDetailsExt_Actions<
			TActivityDetailsExt,
			TGraph,
			TPrimaryEntity>
		where TActivityDetailsExt : ActivityDetailsExt<TGraph, TPrimaryEntity, TPrimaryEntity_NoteID>, IActivityDetailsExt
		where TGraph : PXGraph, new()
		where TPrimaryEntity : class, IBqlTable, INotable, new()
		where TPrimaryEntity_NoteID : IBqlField, IImplement<IBqlCastableTo<IBqlGuid>>
	{
	}

	public abstract class ActivityDetailsExt_Actions<TActivityDetailsExt, TGraph, TPrimaryEntity>
		: ActivityDetailsExt_Actions<
			TActivityDetailsExt,
			TGraph,
			TPrimaryEntity,
			CRPMTimeActivity,
			CRPMTimeActivity.noteID>
		where TActivityDetailsExt : ActivityDetailsExt<TGraph, TPrimaryEntity>, IActivityDetailsExt
		where TGraph : PXGraph, new()
		where TPrimaryEntity : class, IBqlTable, new()
	{
	}

	public abstract class ActivityDetailsExt_Actions<TActivityDetailsExt, TGraph, TPrimaryEntity, TActivityEntity, TActivityEntity_NoteID>
		: PXGraphExtension<TGraph>
		where TActivityDetailsExt : PXGraphExtension<TGraph>, IActivityDetailsExt
		where TGraph : PXGraph, new()
		where TPrimaryEntity : class, IBqlTable, new()
		where TActivityEntity : CRPMTimeActivity, new()
		where TActivityEntity_NoteID : IBqlOperand, IImplement<IBqlEquitable>, IImplement<IBqlCastableTo<IBqlGuid>>
	{
		#region State

		public static class ActivityTypes
		{
			public const string Appointment = "E";
			public const string Escalation = "ES";
			public const string Message = "M";
			public const string Note = "N";
			public const string PhoneCall = "P";
			public const string WorkItem = "W";
		}

		public static class ActionNames
		{
			public const string WorflowActionSuffix = "_Workflow";

			public const string NewTask_Workflow = nameof(NewTask);
			public const string NewEvent_Workflow = nameof(NewEvent);
			public const string NewMailActivity_Workflow = nameof(ActivityDetailsExt.newMailActivity);

			public const string NewActivity_Appointment_Workflow = nameof(NewActivity) + ActivityTypes.Appointment + WorflowActionSuffix;
			public const string NewActivity_Escalation_Workflow = nameof(NewActivity) + ActivityTypes.Escalation + WorflowActionSuffix;
			public const string NewActivity_Message_Workflow = nameof(NewActivity) + ActivityTypes.Message + WorflowActionSuffix;
			public const string NewActivity_Note_Workflow = nameof(NewActivity) + ActivityTypes.Note + WorflowActionSuffix;
			public const string NewActivity_Phonecall_Workflow = nameof(NewActivity) + ActivityTypes.PhoneCall + WorflowActionSuffix;
			public const string NewActivity_Workitem_Workflow = nameof(NewActivity) + ActivityTypes.WorkItem + WorflowActionSuffix;
		}

		[InjectDependency]
		protected IActivityService ActivityService { get; private set; }

		private TActivityDetailsExt _activityDetailsExt;
		public TActivityDetailsExt ActivityDetailsExt
		{
			get
			{
				if (_activityDetailsExt == null)
					_activityDetailsExt = Base.GetExtension<TActivityDetailsExt>();

				return _activityDetailsExt;
			}
		}

		#endregion

		#region ctor

		public override void Initialize()
		{
			base.Initialize();

			AddActivityQuickActionsAsMenu();

			AddPinFunctionality();

			PXUIFieldAttribute.SetVisible<CRActivityPinCacheExtension.isPinned>(ActivityDetailsExt.ActivitiesView.Cache, null, IsPinActivityAvailable());
		}

		#endregion

		#region Actions

		public PXAction<TPrimaryEntity> NewTask;
		[PXUIField(DisplayName = Messages.CreateTask)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Task, OnClosingPopup = PXSpecialButtonType.Refresh, DisplayOnMainToolbar = false)]
		[PXShortCut(ctrl: true, shift: false, alt: false, 'K', 'C')]
		public virtual IEnumerable newTask(PXAdapter adapter)
		{
			ActivityDetailsExt.CreateNewActivityAndRedirect(CRActivityClass.Task, null);

			return adapter.Get();
		}

		public PXAction<TPrimaryEntity> NewEvent;
		[PXUIField(DisplayName = Messages.CreateEvent)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Event, OnClosingPopup = PXSpecialButtonType.Refresh, DisplayOnMainToolbar = false)]
		[PXShortCut(ctrl: true, shift: false, alt: false, 'E', 'C')]
		public virtual IEnumerable newEvent(PXAdapter adapter)
		{
			ActivityDetailsExt.CreateNewActivityAndRedirect(CRActivityClass.Event, null);

			return adapter.Get();
		}

		public PXAction<TPrimaryEntity> NewActivity;
		[PXUIField(DisplayName = Messages.CreateActivity)]
		[PXButton(DisplayOnMainToolbar = false, OnClosingPopup = PXSpecialButtonType.Refresh)]
		public virtual IEnumerable newActivity(PXAdapter adapter)
		{
			return NewActivityByType(adapter, adapter.Menu);
		}

		[PXSuppressActionValidation]
		// for dynamically added
		[PXButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
		public virtual IEnumerable NewActivityByType(PXAdapter adapter, string type)
		{
			ActivityDetailsExt.CreateNewActivityAndRedirect(CRActivityClass.Activity, type);

			return adapter.Get();
		}

		public PXAction<TPrimaryEntity> TogglePinActivity;
		[PXUIField(Visible = false, DisplayName = Messages.PinUnpin)]
		[PXButton(DisplayOnMainToolbar = false)]
		public virtual IEnumerable togglePinActivity(PXAdapter adapter)
		{
			PXCache cache = ActivityDetailsExt.ActivitiesView.Cache;

			INotable activity = cache?.Current as INotable;

			if (activity == null)
				return adapter.Get();

			bool wasPinned = (cache.GetStateExt(activity, nameof(CRActivityPinCacheExtension.IsPinned)) as PXFieldState)?.Value as string == CRActivityPinCacheExtension.isPinned.Pinned;
			string screenID = ScreenHelper.UnmaskScreenID(Base?.Accessinfo?.ScreenID);

			if (wasPinned)
			{
				Base.Caches[typeof(CRActivityPin)].Delete(new CRActivityPin()
				{
					NoteID = activity.NoteID,
					CreatedByScreenID = screenID
				});
			}
			else
			{
				Base.Caches[typeof(CRActivityPin)].Insert(new CRActivityPin()
				{
					NoteID = activity.NoteID
				});
			}

			return adapter.Get();
		}

		#endregion

		#region Events

		#region Row-level

		protected virtual void _(Events.RowSelected<TPrimaryEntity> e)
		{
			var row = e.Row as TPrimaryEntity;
			if (row == null)
				return;

			CorrectButtons(row, e.Cache.GetStatus(row));

			this.TogglePinActivity.SetVisible(IsPinActivityAvailable());
		}

		protected virtual void _(Events.RowSelected<TActivityEntity> e)
		{
			var row = e.Row as TActivityEntity;
			if (row == null)
				return;

			if (row.ClassID != CRActivityClass.Task && row.ClassID != CRActivityClass.Event)
				return;

			e.Cache
				.AdjustUI(e.Row)
				.For<CRActivityPinCacheExtension.isPinned>(_ => _.Visible = IsPinActivityAvailable());
		}

		#endregion

		#region Field-level

		protected virtual void IsPinnedFieldSelecting(PXView view, PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (view is PXPredefinedOrderedView<CRActivityPinCacheExtension.isPinned> orderedView)
			{
				if (orderedView.IsCompare)
					return;
			}

			Guid? selectingNoteId = sender.GetValue(e.Row, nameof(INotable.NoteID)) as Guid?;

			if (selectingNoteId == null)
				return;

			foreach (CRActivityPin pin in sender.Graph.Caches[typeof(CRActivityPin)]
						.Dirty
						.Cast<CRActivityPin>()
						.Where(x => x.NoteID == selectingNoteId))
			{
				e.ReturnState =
					sender.Graph.Caches[typeof(CRActivityPin)].GetStatus(pin) == PXEntryStatus.Deleted
						? CRActivityPinCacheExtension.isPinned.Unpinned
						: CRActivityPinCacheExtension.isPinned.Pinned;
			}
		}

		protected virtual void IsPinnedCommandPreparing(Type itemType, PXCache sender, PXCommandPreparingEventArgs e)
		{
			Query q = new Query();

			q.Field(new SQLConst(true))
				.From(new SimpleTable<CRActivityPin>())
				.Where(new Column<CRActivityPin.noteID>()
					.EQ(new Column(nameof(INotable.NoteID), new SimpleTable(itemType.Name)))
					.And(new Column<CRActivityPin.createdByScreenID>())
					.EQ(ScreenHelper.UnmaskScreenID(sender?.Graph?.Accessinfo?.ScreenID)));

			SQLExpression whenExpr = new SubQuery(q).Exists();
			SQLSwitch switchExpr = new SQLSwitch()
				.Case(whenExpr, new SQLConst(CRActivityPinCacheExtension.isPinned.Pinned))
				.Default(new SQLConst(CRActivityPinCacheExtension.isPinned.Unpinned));
			e.Expr = switchExpr;
			e.BqlTable = itemType;

			e.Cancel = true;
			e.DataType = PXDbType.Bit;
			e.DataLength = 1;
			e.DataValue = e.Value;
		}

		#endregion

		#endregion

		#region Implementation

		#region Dynamic Actions

		public virtual void AddActivityQuickActionsAsMenu()
		{
			foreach (var type in GetActivityTypesForMenu())
			{
				var activityByTypeAction =
					AddAction(
						graph: Base,
						actionName: nameof(NewActivity) + type.Type.TrimEnd(),
						displayName: string.Format(Messages.CreateTypedActivityFormat, type.Description),
						visible: true,
						handler: adapter => NewActivityByType(adapter, type.Type),
						defaultAttributes: new PXButtonAttribute()
						{
							CommitChanges = true,
							DisplayOnMainToolbar = false,
							OnClosingPopup = PXSpecialButtonType.Refresh
						});

				NewActivity.AddMenuAction(activityByTypeAction);
			}
		}

		public virtual IEnumerable<PX.Data.EP.ActivityService.IActivityType> GetActivityTypesForMenu()
		{
			List<PX.Data.EP.ActivityService.IActivityType> types = null;

			types = ActivityService?.GetActivityTypes()?.ToList();

			if (types == null || types.Count <= 0)
				return Enumerable.Empty<PX.Data.EP.ActivityService.IActivityType>();

			var sortedTypes = types.Where(t => t.IsDefault == true).Concat(types.Where(t => t.IsDefault != true));
			sortedTypes = types.OrderBy(_ => _.IsDefault);

			return sortedTypes;
		}

		public virtual PXAction AddAction(PXGraph graph, string actionName, string displayName, bool visible, PXButtonDelegate handler, params PXEventSubscriberAttribute[] defaultAttributes)
		{
			var uiAttribute = new PXUIFieldAttribute
			{
				DisplayName = displayName,
				MapEnableRights = PXCacheRights.Select
			};

			if (!visible)
				uiAttribute.Visible = false;

			var attributes = new List<PXEventSubscriberAttribute> { uiAttribute };

			if (defaultAttributes != null)
				attributes.AddRange(defaultAttributes.Where(attr => attr != null));

			var namedAction = new PXNamedAction<TPrimaryEntity>(graph, actionName, handler, attributes.ToArray());

			graph.Actions[actionName] = namedAction;

			string workflowActionName = actionName + ActionNames.WorflowActionSuffix;
			var workflowAction = new PXNamedAction<TPrimaryEntity>(graph, workflowActionName, handler, attributes.ToArray());

			graph.Actions[workflowActionName] = workflowAction;

			return namedAction;
		}

		#endregion

		public virtual void CorrectButtons(object row, PXEntryStatus status)
		{
			row = row ?? ActivityDetailsExt.ActivitiesView.Cache.Current;
			var viewButtonsEnabled = row != null;

			viewButtonsEnabled = viewButtonsEnabled && status.IsNotIn(PXEntryStatus.Inserted, PXEntryStatus.InsertedDeleted);
			var editButtonEnabled = viewButtonsEnabled && ActivityDetailsExt.ActivitiesView.Cache.AllowInsert;
			PXActionCollection actions = Base.Actions;

			actions[nameof(NewTask)].SetEnabled(editButtonEnabled);
			actions[nameof(NewEvent)].SetEnabled(editButtonEnabled);
			actions[nameof(ActivityDetailsExt<TGraph, TPrimaryEntity>.NewMailActivity)].SetEnabled(editButtonEnabled);
			actions[nameof(NewActivity)].SetEnabled(editButtonEnabled);

			PXButtonState state = actions[nameof(NewActivity)].GetState(row) as PXButtonState;

			if (state != null && state.Menus != null)
			{
				foreach (var button in state.Menus)
				{
					actions[button.Command].SetEnabled(editButtonEnabled);
					actions[button.Command + ActionNames.WorflowActionSuffix].SetEnabled(editButtonEnabled);
				}
			}
		}

		public virtual void AddPinFunctionality()
		{
			if (!IsPinActivityAvailable())
				return;

			Base.Views.Caches.Add(typeof(CRActivityPin));

			if (ActivityDetailsExt.ActivitiesView.BqlDelegate == null)
			{
				Base.Views[ActivityDetailsExt.ActivitiesView.Name] =
						new PXPredefinedOrderedView<CRActivityPinCacheExtension.isPinned>(
								Base,
								ActivityDetailsExt.ActivitiesView.IsReadOnly,
								ActivityDetailsExt.ActivitiesView.BqlSelect);

			}
			else
			{
				Base.Views[ActivityDetailsExt.ActivitiesView.Name] =
						new PXPredefinedOrderedView<CRActivityPinCacheExtension.isPinned>(
								Base,
								ActivityDetailsExt.ActivitiesView.IsReadOnly,
								ActivityDetailsExt.ActivitiesView.BqlSelect,
								ActivityDetailsExt.ActivitiesView.BqlDelegate);
			}
			Type itemType = ActivityDetailsExt.ActivitiesView.Cache.GetItemType();

			Base.CommandPreparing.AddHandler(itemType, nameof(CRActivityPinCacheExtension.IsPinned), (PXCache sender, PXCommandPreparingEventArgs args) => IsPinnedCommandPreparing(itemType, sender, args));

			Base.FieldSelecting.AddHandler(itemType, nameof(CRActivityPinCacheExtension.IsPinned), (PXCache sender, PXFieldSelectingEventArgs args) => IsPinnedFieldSelecting(ActivityDetailsExt.ActivitiesView, sender, args));

			Base.OnAfterPersist += pxBase => ActivityDetailsExt.ActivitiesView.Cache.Clear();
		}

		public virtual bool IsPinActivityAvailable() => false;

		#endregion
	}
}
