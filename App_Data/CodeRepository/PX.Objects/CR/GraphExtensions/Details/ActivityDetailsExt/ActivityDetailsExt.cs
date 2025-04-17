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
using PX.Objects.CR.Descriptor.Exceptions;

namespace PX.Objects.CR.Extensions
{
	public abstract class ActivityDetailsExt<TGraph, TPrimaryEntity, TPrimaryEntity_NoteID>
		: ActivityDetailsExt<
			TGraph,
			TPrimaryEntity>
		where TGraph : PXGraph, new()
		where TPrimaryEntity : class, IBqlTable, INotable, new()
		where TPrimaryEntity_NoteID : IBqlField, IImplement<IBqlCastableTo<IBqlGuid>>
	{
		public override Type GetLinkConditionClause() => typeof(Where<CRPMTimeActivity.refNoteID, Equal<Current<TPrimaryEntity_NoteID>>>);
	}

	public abstract class ActivityDetailsExt<TGraph, TPrimaryEntity>
		: ActivityDetailsExt<
			TGraph,
			TPrimaryEntity,
			CRPMTimeActivity,
			CRPMTimeActivity.noteID>
		where TGraph : PXGraph, new()
		where TPrimaryEntity : class, IBqlTable, new()
	{
		public override Type GetOrderByClause() => typeof(OrderBy<Desc<CRPMTimeActivity.createdDateTime>>);

		public override Type GetClassConditionClause() => typeof(Where<CRPMTimeActivity.classID, GreaterEqual<Zero>>);

		public override Type GetPrivateConditionClause() => PXSiteMap.IsPortal
			? typeof(Where<CRPMTimeActivity.isPrivate.IsNull.Or<CRPMTimeActivity.isPrivate.IsEqual<False>>>)
			: null;
	}

	public abstract class ActivityDetailsExt<TGraph, TPrimaryEntity, TActivityEntity, TActivityEntity_NoteID> : PXGraphExtension<TGraph>, IActivityDetailsExt
		where TGraph : PXGraph, new()
		where TPrimaryEntity : class, IBqlTable, new()
		where TActivityEntity : CRPMTimeActivity, new()
		where TActivityEntity_NoteID : IBqlOperand, IImplement<IBqlEquitable>, IImplement<IBqlCastableTo<IBqlGuid>>
	{
		#region State

		[InjectDependency]
		protected IReportLoaderService ReportLoader { get; private set; }

		[InjectDependency]
		protected IReportDataBinder ReportDataBinder { get; private set; }

		[InjectDependency]
		protected IActivityService ActivityService { get; private set; }

		[InjectDependency]
		internal ICurrentUserInformationProvider CurrentUserInformationProvider { get; private set; }

		private EntityHelper _EntityHelper;
		public EntityHelper EntityHelper
		{
			get
			{
				if (_EntityHelper == null)
					_EntityHelper = new EntityHelper(Base);

				return _EntityHelper;
			}
		}

		private NotificationUtility _NotificationUtility;
		public NotificationUtility NotificationUtility
		{
			get
			{
				if (_NotificationUtility == null)
					_NotificationUtility = new NotificationUtility(Base);

				return _NotificationUtility;
			}
		}

		private int? _entityDefaultEMailAccountId;
		public int? DefaultEmailAccountID
		{
			get
			{
				return
					MailAccountManager.GetUserSettingsEmailAccount(CurrentUserInformationProvider.GetUserId(), true)?.EmailAccountID
					?? _entityDefaultEMailAccountId
					?? MailAccountManager.GetSystemSettingsEmailAccount(CurrentUserInformationProvider.GetUserId(), true)?.EmailAccountID;
			}
			set { _entityDefaultEMailAccountId = value; }
		}

		public string DefaultActivityType
		{
			get
			{
				EPSetup setup = PXSetup<EPSetup>.Select(Base);

				if (setup != null && !string.IsNullOrEmpty(setup.DefaultActivityType))
				{
					return setup.DefaultActivityType;
				}

				return null;
			}
		}

		public string DefaultSubject { get; set; }

		Type IActivityDetailsExt.GetActivityType() => typeof(TActivityEntity);

		#endregion

		#region ctor

		public override void Initialize()
		{
			base.Initialize();

			AdjustActivitiesView();

			AttachPreview();

			AttachEvents();
		}

		#endregion

		#region Views

		[PXCopyPasteHiddenView]
		[PXViewName(Messages.Activities)]
		[PXFilterable]
		public SelectFrom<
				TActivityEntity>
			.LeftJoin<CRReminder>
				.On<CRReminder.refNoteID.IsEqual<TActivityEntity_NoteID>>
			.View
			Activities;

		public PXView ActivitiesView => Activities.View;

		// hack to help ProjectTaskIDAttribute work correctly for CRPMTimeActivity
		[PXHidden]
		public PXSelect<CT.Contract> ContractsDummy;

		#endregion

		#region Actions

		public PXAction<TPrimaryEntity> NewMailActivity;
		[PXUIField(DisplayName = Messages.CreateEmail)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.MailSend, OnClosingPopup = PXSpecialButtonType.Refresh, DisplayOnMainToolbar = false)]
		[PXShortCut(ctrl: true, shift: false, alt: false, 'A', 'M')]
		public virtual IEnumerable newMailActivity(PXAdapter adapter)
		{
			CreateNewActivityAndRedirect(CRActivityClass.Email, null);

			return adapter.Get();
		}

		public PXAction<TPrimaryEntity> ViewActivity;
		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select)]
		[PXButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
		public virtual IEnumerable viewActivity(PXAdapter adapter)
		{
			NavigateToActivity(Activities.Current);

			return adapter.Get();
		}

		public PXAction<TPrimaryEntity> OpenActivityOwner;
		[PXButton]
		[PXUIField(Visible = false)]
		public virtual IEnumerable openActivityOwner(PXAdapter adapter)
		{
			var act = Activities.Current as CRActivity;
			if (act != null)
			{
				var empl = (EPEmployee)PXSelectReadonly<EPEmployee,
					Where<EPEmployee.defContactID, Equal<Required<CRActivity.ownerID>>>>.
					Select(Base, act.OwnerID);
				if (empl != null)
					PXRedirectHelper.TryRedirect(Base.Caches[typeof(EPEmployee)], empl, string.Empty, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		public PXAction<TPrimaryEntity> ViewAllActivities;
		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select)]
		[PXButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
		public virtual IEnumerable viewAllActivities(PXAdapter adapter)
		{
			var gr = PXGraph.CreateInstance<ActivitiesMaint>();

			gr.Filter.Current.NoteID = ((PXCache<TActivityEntity>)Base.Caches[typeof(TActivityEntity)]).InitNewRow()?.RefNoteID;

			throw new PXPopupRedirectException(gr, string.Empty, true);
		}

		#endregion

		#region Events

		#region Row-level

		protected virtual void _(Events.RowSelected<TActivityEntity> e)
		{
			var row = e.Row as TActivityEntity;
			if (row == null)
				return;

			if (row.ClassID != CRActivityClass.Task && row.ClassID != CRActivityClass.Event)
				return;

			int timespent = 0;
			int overtimespent = 0;
			int timebillable = 0;
			int overtimebillable = 0;

			using (new PXConnectionScope())
			{
				foreach (PXResult<CRChildActivity, PMTimeActivity> child in
						PXSelectJoin<
								CRChildActivity,
							InnerJoin<PMTimeActivity,
								On<PMTimeActivity.refNoteID, Equal<CRChildActivity.noteID>>>,
							Where<
								CRChildActivity.parentNoteID, Equal<Required<CRChildActivity.parentNoteID>>,
								And<Where<PMTimeActivity.isCorrected, NotEqual<True>,
									Or<PMTimeActivity.isCorrected, IsNull>>>>>
							.Select(Base, row.NoteID))
				{
					var childTime = (PMTimeActivity)child;

					timespent += (childTime.TimeSpent ?? 0);
					overtimespent += (childTime.OvertimeSpent ?? 0);
					timebillable += (childTime.TimeBillable ?? 0);
					overtimebillable += (childTime.OvertimeBillable ?? 0);
				}
			}

			row.TimeSpent = timespent;
			row.OvertimeSpent = overtimespent;
			row.TimeBillable = timebillable;
			row.OvertimeBillable = overtimebillable;
		}

		protected virtual void _(Events.RowInserting<CRSMEmail> e)
		{
			if (e.Row == null)
				return;

			InitializeEmail(e.Row);
		}

		protected virtual void ActivityRowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			var row = e.Row as CRActivity;

			if (row == null)
				return;

			InitializeActivity(row);
		}

		protected virtual void _(Events.RowPersisting<TActivityEntity> e)
		{
			if ((e.Operation & PXDBOperation.Delete) == PXDBOperation.Delete)
			{
				e.Cancel = true;
			}

			if (e.Row == null)
				return;

			if (e.Row.ChildKey != null)
				return;

			// means no Child

			var tables = e.Cache.BqlSelect.GetTables();

			foreach (var field in Base.Caches[tables.Length > 1 ? tables[1] : typeof(CRActivity)].Fields)
			{
				PXDefaultAttribute.SetPersistingCheck(e.Cache, field, e.Row, PXPersistingCheck.Nothing);
			}

			PXProjectionAttribute projection = (PXProjectionAttribute)(e.Row.GetType()).GetCustomAttributes(typeof(PXProjectionAttribute), true)[0];

			projection.Persistent = false;
		}

		protected virtual void _(Events.RowPersisted<TActivityEntity> e)
		{
			if (e.Row == null) return;

			PXProjectionAttribute projection = (PXProjectionAttribute)(e.Row.GetType()).GetCustomAttributes(typeof(PXProjectionAttribute), true)[0];

			projection.Persistent = true;
		}

		#endregion

		#region Field-level

		protected virtual void BAccountIDFieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (GetBAccountIDCommand() is Type accountReference)
			{
				e.NewValue = GetIDByReference(accountReference);
			}
		}

		protected virtual void ContactIDFieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (GetContactIDCommand() is Type contactReference)
			{
				e.NewValue = GetIDByReference(contactReference);
			}
		}

		protected virtual void _(Events.FieldSelecting<TActivityEntity, CRActivity.body> e)
		{
			if (e.Row == null) return;

			(bool hasAccess, Type graphType) = CheckAccessRightsOfTargetGraph(e.Row.ClassID, e.Row.Type);

			if (!hasAccess && !Base.UnattendedMode)
			{
				e.ReturnValue = Messages.FormNoAccessRightsMessage(graphType);
			}
			else
			{
				if (e.Row.ClassID == CRActivityClass.Email)
				{
					var entity = (SMEmailBody)PXSelect<SMEmailBody, Where<SMEmailBody.refNoteID, Equal<Required<CRActivity.noteID>>>>.SelectSingleBound(Base, null, e.Row.NoteID);

					if (entity == null) return;
					e.ReturnValue = entity.Body;

					PXContext.Session.SetString(string.Format("{0}+{1}", typeof(Note).FullName, e.Row.NoteID), e.Row.NoteID?.ToString());
				}
			}
		}

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Enabled), false)]
		protected virtual void _(Events.CacheAttached<CRActivity.body> e) { }

		#endregion

		#endregion

		#region Implementation

		#region Activity View adjustments

		public virtual void AdjustActivitiesView()
		{
			if (GetLinkConditionClause() is Type linkCondition)
				Activities.View.WhereAnd(linkCondition);

			if (GetClassConditionClause() is Type classCondition)
				Activities.View.WhereAnd(classCondition);

			if (GetPrivateConditionClause() is Type privateCondition)
				Activities.View.WhereAnd(privateCondition);

			if (GetOrderByClause() is Type orderBy)
				Activities.View.OrderByNew(orderBy);
		}

		public abstract Type GetLinkConditionClause();
		public abstract Type GetClassConditionClause();
		public abstract Type GetPrivateConditionClause();
		public abstract Type GetOrderByClause();

		#endregion

		public virtual void AttachEvents()
		{
			foreach (var activityType in GetAllActivityTypes())
			{
				Base.FieldDefaulting.AddHandler(activityType, nameof(CRActivity.BAccountID), BAccountIDFieldDefaulting);
				Base.FieldDefaulting.AddHandler(activityType, nameof(CRActivity.ContactID), ContactIDFieldDefaulting);

				Base.RowInserting.AddHandler(activityType, ActivityRowInserting);
			}
		}

		public virtual IList<Type> GetAllActivityTypes()
		{
			return new []
			{
				typeof(CRActivity),
				typeof(CRSMEmail),

				typeof(TActivityEntity),
			};
		}

		public virtual void AttachPreview()
		{
			var att = new CRPreviewAttribute(typeof(TPrimaryEntity), typeof(TActivityEntity));
			att.Attach(Base, nameof(Activities), null);
		}

		public virtual void NavigateToActivity(TActivityEntity row)
		{
			object resultRow = row;
			Type targetGraphType = EntityHelper.GetPrimaryGraphType(ref resultRow, checkRights: false);

			if (targetGraphType == null || !PXAccess.VerifyRights(targetGraphType))
			{
				Base
					.Views[Base.PrimaryView]
					.Ask(null, Messages.AccessDenied, Messages.FormNoAccessRightsMessage(targetGraphType), MessageButtons.OK, MessageIcon.Error);

				return;
			}

			var graph = PXGraph.CreateInstance(targetGraphType);

			if (row.TimeActivityNoteID != null && row.TimeActivityNoteID.Equals(row.TimeActivityRefNoteID))
			{
				// only Time Activity exists => create an ordinary Activity

				var timeAct = PMTimeActivity.PK.Find(Base, row.TimeActivityNoteID);

				PXCache parentCache = graph.Caches[typeof(CRActivity)];
				PXCache childCache = graph.Caches[typeof(PMTimeActivity)];

				var parentEntity = parentCache.NonDirtyInsert();

				parentCache.SetValue(parentEntity, nameof(CRActivity.noteID), childCache.GetValue(timeAct, nameof(CRPMTimeActivity.refNoteID)));
				parentCache.SetValue(parentEntity, nameof(CRActivity.type), DefaultActivityType);
				parentCache.SetValue(parentEntity, nameof(CRActivity.subject), childCache.GetValue(timeAct, nameof(CRPMTimeActivity.summary)));
				parentCache.SetValue(parentEntity, nameof(CRActivity.ownerID), childCache.GetValue(timeAct, nameof(CRPMTimeActivity.ownerID)));
				parentCache.SetValue(parentEntity, nameof(CRActivity.startDate), childCache.GetValue(timeAct, nameof(CRPMTimeActivity.date)));
				parentCache.SetValue(parentEntity, nameof(CRActivity.uistatus), ActivityStatusAttribute.Completed);
				parentCache.Normalize();

				childCache.SetValue(timeAct, nameof(PMTimeActivity.summary), parentCache.GetValue(parentEntity, nameof(CRActivity.subject)));
				childCache.Current = timeAct;

				childCache.MarkUpdated(childCache.Current);

				bool wasUsed = !string.IsNullOrEmpty(timeAct?.TimeCardCD) || timeAct?.Billed == true;
				if (wasUsed)
					parentCache.IsDirty = false;

				this.Activities.Cache.Clear();

				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
			}
			else
			{
				graph.GetPrimaryCache().Current = resultRow;

				this.Activities.Cache.Clear();

				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
			}
		}

		public virtual Guid? EnsureNoteID(object row)
		{
			if (row == null)
				return null;

			var rowType = row.GetType();
			var noteField = EntityHelper.GetNoteField(rowType);
			var cache = Base.Caches[rowType];

			return PXNoteAttribute.GetNoteID(cache, row, noteField);
		}

		public virtual (bool, Type) CheckAccessRightsOfTargetGraph(int? classID, string activityType)
		{
			// Dummy activity is created to search through the primary graphs.
			// Activity's Primary Graphs should be defined in proper way to handle non-inserted record (like Current<Activity.Class> EQUALS XX)
			object dummyActivityForGraphSearch = new CRActivity
			{
				ClassID = classID,
				Type = activityType
			};

			Type targetGraphType = EntityHelper.GetPrimaryGraphType(ref dummyActivityForGraphSearch, checkRights: false);

			return (PXAccess.VerifyRights(targetGraphType), targetGraphType);
			
		}

		public virtual void CreateNewActivityAndRedirect(int classID, string activityType)
		{
			var graph = CreateNewActivity(classID, activityType);

			if (graph != null)
			{
				throw new PXRedirectRequiredException(graph: graph, message: string.Empty, repaintControls: true)
				{
					Mode = PXBaseRedirectException.WindowMode.NewWindow
				};
			}
		}

		public virtual PXGraph CreateNewActivity(int classID, string activityType)
		{
			(bool hasAccess, Type graphType) = CheckAccessRightsOfTargetGraph(classID, activityType);

			if (!hasAccess)
			{
				Base
					.Views[Base.PrimaryView]
					.Ask(null, Messages.AccessDenied, Messages.FormNoAccessRightsMessage(graphType), MessageButtons.OK, MessageIcon.Error);

				return null;
			}

			var targetGraph = PXGraph.CreateInstance(graphType);

			CreatePrimaryActivity(targetGraph, classID, activityType);

			CreateTimeActivity(targetGraph, classID, activityType);

			foreach (PXCache dirtycache in targetGraph.Caches.Caches.Where(c => c.IsDirty))
			{
				dirtycache.IsDirty = false;
			}

			return targetGraph;
		}

		public virtual void CreatePrimaryActivity(PXGraph targetGraph, int classID, string activityType)
		{
			// Insert the Activity into the current context, where the "Create XX Activity" is pressed
			var localActivity = Base.Caches[targetGraph.PrimaryItemType].CreateInstance() as CRActivity;

			if (localActivity == null)
				return;

			localActivity.ClassID = classID;
			localActivity.Type = activityType;

			// Defaultings and other stuff is handled in current context, not in destination
			// Please subcribe for RowInserted or FieldDefaulting events
			// InitNewRow will invoke Insert without dirtying
			var activity = InitNewRow(Base.Caches[targetGraph.PrimaryItemType], localActivity);

			if (Base.IsDirty)
			{
				if (Base.IsMobile)
				{
					// ensure that row will be persisted with Note when call from mobile

					var rowCache = Base.Views[Base.PrimaryView].Cache;
					if (rowCache.Current != null)
					{
						rowCache.SetStatus(rowCache.Current, PXEntryStatus.Updated);
					}
				}

				Base.Actions.PressSave();
			}

			var destinationCache = targetGraph.GetPrimaryCache();

			InsertActivityIntoTargetGraph(targetGraph, activity);

			foreach (CRActivity destinationActivity in destinationCache.Inserted)
			{
				UDFHelper.CopyAttributes(Base.Views[Base.PrimaryView].Cache, Base.Views[Base.PrimaryView].Cache.Current, destinationCache, destinationActivity, null);
			}
		}

		public virtual void CreateTimeActivity(PXGraph targetGraph, int classID, string activityType)
		{
		}

		public virtual void InsertActivityIntoTargetGraph(PXGraph targetGraph, CRActivity activity)
		{
			var destinationCache = targetGraph.GetPrimaryCache();

			var dict = destinationCache.ToDictionary(activity);

			// Set NotSetValue so the value can be taken into account when making a defaulting on a target graph.
			// Need to understand, if the field is already set with empty value or hasn't been set yet
			foreach (var key in dict.Keys.ToList())
			{
				if (dict[key] == null)
					dict[key] = PXCache.NotSetValue;
			}

			// Insert the predefined Activity into the target XX Activity graph
			destinationCache.Insert(dict);
		}

		public virtual TNode InitNewRow<TNode>(PXCache cache, TNode node)
			where TNode : class, IBqlTable, new()
		{
			TNode result = null;

			if (node == null)
				return result;

			void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
			{
				result = (TNode)e.Row;
				e.Cancel = true;
			}

			cache.Graph.RowInserting.AddHandler(node.GetType(), RowInserting);

			try
			{
				cache.Insert(node);
			}
			finally
			{
				cache.Graph.RowInserting.RemoveHandler(node.GetType(), RowInserting);
			}

			return result;
		}

		public virtual void InitializeActivity(CRActivity row)
		{
			row.OwnerID ??= EmployeeMaint.GetCurrentOwnerID(Base);
			int? workgroup = GetPrimaryEntityWorkgroupID();

			if (row.OwnerID != null && OwnerAttribute.BelongsToWorkGroup(Base, workgroup, row.OwnerID))
				row.WorkgroupID ??= workgroup;

			row.RefNoteID ??= GetRefNoteID();

			if (GetBAccountIDCommand() is Type accountReference)
			{
				row.BAccountID ??= GetIDByReference(accountReference);
			}

			if (GetContactIDCommand() is Type contactReference)
			{
				row.ContactID ??= GetIDByReference(contactReference);
			}
		}

		public virtual int? GetPrimaryEntityWorkgroupID()
		{
			PXCache cache = Base.Caches[typeof(TPrimaryEntity)];
			var iAssignEntity = cache.Current as IAssign;

			if (iAssignEntity == null)
				return null;

			return iAssignEntity.WorkgroupID;
		}

		public virtual Type GetContactIDCommand() => null;
		public virtual Type GetBAccountIDCommand() => null;

		public virtual Guid? GetRefNoteID()
		{
			var primaryCache = Base.Caches[typeof(TPrimaryEntity)];
			var primaryEntity = primaryCache.Current;

			if (primaryEntity == null)
				return null;

			if (primaryEntity is INotable notableEntity)
			{
				EnsureNoteID(notableEntity);

				return notableEntity.NoteID;
			}
			else if (primaryCache.GetValue(primaryEntity, nameof(INotable.NoteID)) is Guid noteID)
			{
				// fallback: item is not INotable, but let's try to do smth at least

				EnsureNoteID(primaryEntity);

				return noteID;
			}

			return null;
		}

		public virtual int? GetIDByReference(Type reference)
		{
			if (typeof(IBqlSelect).IsAssignableFrom(reference))
			{
				var view = new PXView(Base, true, BqlCommand.CreateInstance(reference));
				var record = view.SelectSingle();

				return
					record != null
						? Base.Caches[record.GetType()].GetValue(record, EntityHelper.GetIDField(view.Cache)) as int?
						: null;
			}
			else if (typeof(IBqlField).IsAssignableFrom(reference))
			{
				var cache = Base.Caches[reference.DeclaringType];

				return
					cache.Current != null
						? cache.GetValue(cache.Current, reference.Name) as int?
						: null;
			}

			return null;
		}

		#region Email Methods

		public virtual void InitializeEmail(CRSMEmail row)
		{
			row.MailAccountID ??= DefaultEmailAccountID;

			row.MailReply ??= GetMailReply(row, row.MailReply);

			row.MailTo ??= GetMailTo(row);

			if (row.RefNoteID != null)
				row.MailCc ??= GetMailCc(row, row.RefNoteID);

			row.Subject ??= GetSubject(row);

			row.Body ??= GetBody();
		}

		public virtual string GetMailReply(CRSMEmail message, string currentMailReply)
		{
			Mailbox mailAddress = null;

			var isCorrect =
				currentMailReply != null
				&& Mailbox.TryParse(currentMailReply, out mailAddress)
				&& !string.IsNullOrEmpty(mailAddress.Address);

			if (isCorrect)
			{
				isCorrect = PXSelect<EMailAccount,
					Where<EMailAccount.address, Equal<Required<EMailAccount.address>>>>
					.Select(Base, mailAddress.Address)
					.Count > 0;
			}

			var result = currentMailReply;

			if (!isCorrect)
			{
				result = EMailAccount.PK.Find(Base, DefaultEmailAccountID)?.Address;
			}

			if (!string.IsNullOrEmpty(result))
				return result;

			var firstAcct = (EMailAccount)PXSelect<EMailAccount>.SelectWindowed(Base, 0, 1);

			if (firstAcct != null)
				result = firstAcct.Address;

			return result;
		}

		public virtual string GetMailTo(CRSMEmail message)
		{
			string customMailTo = GetCustomMailTo() ?? GetDefaultMailTo();

			return
				!string.IsNullOrWhiteSpace(customMailTo)
					? customMailTo.Trim()
					: null;
		}

		public virtual string GetMailCc(CRSMEmail message, Guid? refNoteId)
		{
			return
				refNoteId != null
					? PXDBEmailAttribute.AppendAddresses(message.MailCc, ActivityService?.GetEmailAddressesForCc(Base, refNoteId.Value))
					: null;
		}

		public virtual string GetSubject(CRSMEmail message)
		{
			return
				!string.IsNullOrEmpty(DefaultSubject)
					? DefaultSubject
					: null;
		}

		public virtual string GetBody()
		{
			string res = null;

			res = MailAccountManager.AppendSignature(res, Base, MailAccountManager.SignatureOptions.Default);

			return PX.Web.UI.PXRichTextConverter.NormalizeHtml(res);
		}

		public virtual string GetDefaultMailTo()
		{
			string emailAddress = null;

			IEmailMessageTarget record;

			if (GetEmailMessageTarget() is Type select && typeof(IBqlSelect).IsAssignableFrom(select))
			{
				record = new PXView(Base, true, BqlCommand.CreateInstance(select)).SelectSingle() as IEmailMessageTarget;
			}
			else
			{
				var primaryCache = Base.GetPrimaryCache();
				record = primaryCache.Current as IEmailMessageTarget;
			}

			if (record == null)
				return null;

			var displayName = record.DisplayName?.Trim();
			var addresses = record.Address?.Trim();
			if (!string.IsNullOrEmpty(addresses))
			{
				emailAddress = PXDBEmailAttribute.FormatAddressesWithSingleDisplayName(addresses, displayName);
			}

			return emailAddress;
		}

		public virtual Type GetEmailMessageTarget() => null;

		public virtual string GetCustomMailTo()
		{
			return null;
		}

		#endregion

		#region Notifications

		public virtual void SendNotification(string sourceType, string notifications, int? branchID, IDictionary<string, string> parameters, bool massProcess = false, IList<Guid?> attachments = null)
		{
			var sender = CreateNotificationProvider(sourceType, notifications, branchID, parameters, attachments);

			sender.MassProcessMode = massProcess;

			if (!sender.Send().Any())
				throw new PXException(Messages.EmailNotificationError);
		}

		public virtual NotificationGenerator CreateNotificationProvider(
			string sourceType, string notifications, int? branchID, IDictionary<string, string> parameters, IList<Guid?> attachments = null)
		{
			if (notifications == null)
				return null;

			IList<string> list = notifications.Split(',')
				.Select(n => n?.Trim())
				.Where(cd => !string.IsNullOrEmpty(cd)).ToList();

			return CreateNotificationProvider(sourceType, list, branchID, parameters, attachments);
		}

		public virtual NotificationGenerator CreateNotificationProvider(string sourceType, IList<string> notificationCDs, int? branchID, IDictionary<string, string> parameters, IList<Guid?> attachments = null)
		{
			PXCache sourceCache = Base.Caches[typeof(TPrimaryEntity)];

			if (sourceCache.Current == null)
				throw new PXException(Messages.EmailNotificationObjectNotFound);

			IList<NotificationSetup> setupIDs = GetSetupNotifications(sourceType, notificationCDs);
			PXCache primaryCache = Base.Caches[typeof(TPrimaryEntity)];
			TPrimaryEntity row = (TPrimaryEntity)primaryCache.Current;

			object correctTypeRow = primaryCache.CreateInstance();
			primaryCache.RestoreCopy(correctTypeRow, row);
			row = (TPrimaryEntity)correctTypeRow;

			TActivityEntity activity = ((PXCache<TActivityEntity>)Base.Caches[typeof(TActivityEntity)]).InitNewRow();
			var bAccount = GetBAccountRow(sourceType, activity);

			RecipientList recipients = null;
			TemplateNotificationGenerator sender = null;
			for (int i = 0; i < setupIDs.Count; i++)
			{
				NotificationSetup setup = setupIDs[i];
				NotificationSource source =
					bAccount != null
						? NotificationUtility.GetSource(sourceType, bAccount, setup.SetupID.Value, branchID ?? this.Base.Accessinfo.BranchID)
						: NotificationUtility.GetSource(setup);

				if (source == null && sourceType == PMNotificationSource.Project)
				{
					source = NotificationUtility.GetSource(sourceType, row, setup.SetupID.Value, branchID ?? this.Base.Accessinfo.BranchID);
				}

				if (source == null)
					throw new PXException(PX.SM.Messages.NotificationSourceNotFound);

				if (sender == null)
				{
					var accountId = source.EMailAccountID ?? setup.EMailAccountID ?? DefaultEmailAccountID;

					if (accountId == null)
						throw new PXException(ErrorMessages.EmailNotConfigured);

					recipients ??= NotificationUtility.GetRecipients(sourceType, bAccount, source);

					sender = TemplateNotificationGenerator.Create(row, source.NotificationID);

					sender.MailAccountId = accountId;
					sender.RefNoteID = activity.RefNoteID;
					sender.DocumentNoteID = activity.DocumentNoteID;
					sender.BAccountID = activity.BAccountID;
					sender.ContactID = activity.ContactID;
					sender.Watchers = recipients;
				}

				if (source.ReportID != null)
				{
					var _report = ReportLoader.LoadReport(source.ReportID, incoming: null);

					if (_report == null)
						throw new ArgumentException(PXMessages.LocalizeFormatNoPrefixNLA(EP.Messages.ReportCannotBeFound, source.ReportID), "reportId");

					ReportLoader.InitDefaultReportParameters(_report, parameters ?? GetKeyParameters(sourceCache));
					_report.MailSettings.Format = ReportNotificationGenerator.ConvertFormat(source.Format);
					var reportNode = ReportDataBinder.ProcessReportDataBinding(_report);

					reportNode.SendMailMode = true;

					PX.Reports.Mail.Message message =
						(from msg in reportNode.Groups.Select(g => g.MailSettings)
						 where msg != null && msg.ShouldSerialize()
						 select new PX.Reports.Mail.Message(msg, reportNode, msg)).FirstOrDefault();

					if (message == null)
					{
						if (i == 0)
							throw new EmailFromReportCannotBeCreatedException(
								PXMessages.LocalizeFormatNoPrefixNLA(EP.Messages.EmailFromReportCannotBeCreated, source.ReportID),
								source.ReportID);

						continue;
					}

					if (i == 0)
					{
						bool bodyWasNull = false;

						if (sender.Body == null)
						{
							string body = message.Content.Body;
							bodyWasNull = body == null;
							if (bodyWasNull || !IsHtml(body))
							{
								body = Tools.ConvertSimpleTextToHtml(message.Content.Body);
							}
							sender.Body = body;
							sender.BodyFormat = NotificationFormat.Html;
						}

						sender.Subject = string.IsNullOrEmpty(sender.Subject) ? message.Content.Subject : sender.Subject;
						sender.To = string.IsNullOrEmpty(sender.To) ? message.Addressee.To : sender.To;
						sender.Cc = string.IsNullOrEmpty(sender.Cc) ? message.Addressee.Cc : sender.Cc;
						sender.Bcc = string.IsNullOrEmpty(sender.Bcc) ? message.Addressee.Bcc : sender.Bcc;

						if (!string.IsNullOrEmpty(message.TemplateID))
						{
							var generator = TemplateNotificationGenerator.Create(row, message.TemplateID);

							var template = generator.ParseNotification();

							if (string.IsNullOrEmpty(sender.Body) || bodyWasNull)
								sender.Body = template.Body;

							if (string.IsNullOrEmpty(sender.Subject))
								sender.Subject = template.Subject;

							if (string.IsNullOrEmpty(sender.To))
								sender.To = template.To;

							if (string.IsNullOrEmpty(sender.Cc))
								sender.Cc = template.Cc;

							if (string.IsNullOrEmpty(sender.Bcc))
								sender.Bcc = template.Bcc;
						}

						if (string.IsNullOrEmpty(sender.Subject))
							sender.Subject = reportNode.Report.Name;
					}

					foreach (var attachment in message.Attachments)
					{
						if (sender.Body == null && sender.BodyFormat == NotificationFormat.Html && attachment.MimeType == "text/html")
						{
							sender.Body = attachment.Encoding.GetString(attachment.GetBytes());
						}
						else
							sender.AddAttachment(attachment.Name, attachment.GetBytes(), attachment.CID);
					}

					if (attachments != null)
						foreach (var attachment in attachments)
							if (attachment != null)
								sender.AddAttachmentLink(attachment.Value);
				}

				var mainDocRecipient = GetPrimaryRecipientFromContext(NotificationUtility, sourceType, bAccount, source);

				if (!String.IsNullOrEmpty(mainDocRecipient))
				{
					sender.To = mainDocRecipient;
				}

				switch (source.RecipientsBehavior)
				{
					case RecipientsBehaviorAttribute.Add:
					default:
						// do not clear TO, CC and BCC as they are needed for sending
						break;

					case RecipientsBehaviorAttribute.Override:
						// clear TO, CC and BCC as they shouldn't recieve any emails
						// sender.Watchers contains all the recipients
						sender.To = null;
						sender.Cc = null;
						sender.Bcc = null;
						break;
				}
			}
			return sender;
		}

		public virtual string GetPrimaryRecipientFromContext(NotificationUtility utility, string type, object row, NotificationSource source) => null;

		public virtual object GetBAccountRow(string sourceType, TActivityEntity activity)
		{
			var sourceRow =
				EntityHelper.GetEntityRowByID(typeof(BAccountR), activity.BAccountID)
				?? EntityHelper.GetEntityRow(typeof(BAccountR), activity.RefNoteID);

			return sourceRow;
		}

		public virtual IDictionary<string, string> GetKeyParameters(PXCache sourceCache)
		{
			IDictionary<string, string> parameters = new Dictionary<string, string>();
			foreach (string key in sourceCache.Keys)
			{
				object value = sourceCache.GetValueExt(sourceCache.Current, key);
				parameters[key] = value?.ToString();
			}
			return parameters;
		}

		public virtual Guid[] GetNotifications(string sourceType, IList<string> notificationCDs)
		{
			var setupIDs = new Guid[notificationCDs.Count];

			for (int i = 0; i < notificationCDs.Count; i++)
			{
				Guid? SetupID = NotificationUtility.SearchSetupID(sourceType, notificationCDs[i]);

				if (SetupID == null)
					throw new PXException(Messages.EmailNotificationSetupNotFound, notificationCDs[i]);

				setupIDs[i] = SetupID.Value;
			}

			return setupIDs;
		}

		public virtual List<NotificationSetup> GetSetupNotifications(string sourceType, IList<string> notificationCDs)
		{
			var setups = new List<NotificationSetup>();

			for (int i = 0; i < notificationCDs.Count; i++)
			{
				NotificationSetup setup = NotificationUtility.SearchSetup(sourceType, notificationCDs[i]);

				if (setup == null)
					throw new PXException(Messages.EmailNotificationSetupNotFound, notificationCDs[i]);

				setups.Add(setup);
			}

			return setups;
		}

		public virtual bool IsHtml(string text)
		{
			if (string.IsNullOrEmpty(text))
				return false;

			var htmlIndex = text.IndexOf("<html", StringComparison.CurrentCultureIgnoreCase);
			var bodyIndex = text.IndexOf("<body", StringComparison.CurrentCultureIgnoreCase);

			return
				htmlIndex > -1
				&& bodyIndex > -1
				&& bodyIndex > htmlIndex;
		}

		#endregion

		#endregion
	}
}
