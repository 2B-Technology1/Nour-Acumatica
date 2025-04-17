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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Web.Compilation;
using PX.Common;
using PX.Common.Mail;
using PX.Data;
using PX.Data.EP;
using PX.Data.SQLTree;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.IN;
using PX.TM;
using PX.Objects.TX;
using PX.Objects.GL;
using PX.Objects.SO;
using PX.Objects.CS;
using PX.SM;
using PX.Objects.PM;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.Common.Interfaces;
using PX.Objects.EP;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Reports;
using ActivityService = PX.Data.EP.ActivityService;

namespace PX.Objects.CR.BackwardCompatibility
{

	/// <exclude/>
	[Obsolete]
	public abstract class CRActivityListBaseAttribute : PXViewExtensionAttribute
	{
		private readonly BqlCommand _command;

		private PXView _view;

		protected string _hostViewName;
		private PXSelectBase _select;

		protected CRActivityListBaseAttribute() { }

		protected CRActivityListBaseAttribute(Type select)
		{
			if (select == null) throw new ArgumentNullException("select");

			if (typeof(IBqlSelect).IsAssignableFrom(select))
			{
				_command = BqlCommand.CreateInstance(select);
			}
			else
			{
				throw new PXArgumentException("@select", PXMessages.LocalizeFormatNoPrefixNLA(Messages.IncorrectSelectExpression, select.Name));
			}
		}

		public override void ViewCreated(PXGraph graph, string viewName)
		{
			Initialize(graph, viewName);

			AttachHandlers(graph);
		}

		private void Initialize(PXGraph graph, string viewName)
		{
			_hostViewName = viewName;
			_select = GetSelectView(graph);

			if (_command != null)
				_view = new PXView(graph, true, _command);
		}

		protected PXSelectBase GraphSelector
		{
			get { return _select; }
		}

		protected abstract void AttachHandlers(PXGraph graph);

		protected object SelectRecord()
		{
			if (_view == null)
				throw new InvalidOperationException(Messages.CommandNotSpecified);

			var dataRecord = _view.SelectSingle();
			if (dataRecord == null) return null;

			var res = dataRecord as PXResult;
			if (res == null) return dataRecord;

			return res[0];
		}

		protected virtual PXSelectBase GetSelectView(PXGraph graph)
		{
			var selectView = graph.GetType().GetField(_hostViewName).GetValue(graph);
			var selectViewType = selectView.GetType();
			Type typeDefinition = selectViewType;

			while (typeDefinition != typeof(object))
			{
				if (typeDefinition.IsGenericType)
					typeDefinition = typeDefinition.GetGenericTypeDefinition();

				if (typeof(CRActivityList<>).IsAssignableFrom(typeDefinition))
				{
					return (PXSelectBase)selectView;
				}
				else
				{
					typeDefinition = typeDefinition.BaseType;
				}
			}

			var attributeTypeName = GetType().Name;
			throw new PXArgumentException((string)null, PXMessages.LocalizeFormatNoPrefixNLA(Messages.AttributeCanOnlyUsedOnView, attributeTypeName, selectViewType.Name));
		}
	}

	/// <exclude/>
	[Obsolete]
	public class PMDefaultMailToAttribute : CRDefaultMailToAttribute
	{
		public PMDefaultMailToAttribute()
			: base()
		{
		}

		public PMDefaultMailToAttribute(Type select)
			: base(select)
		{
		}

		protected override PXSelectBase GetSelectView(PXGraph graph)
		{
			var selectView = graph.GetType().GetField(_hostViewName).GetValue(graph);

			return (PXSelectBase)selectView;
		}
	}

	/// <exclude/>
	[Obsolete]
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class CRReferenceAttribute : PXViewExtensionAttribute
	{
		private readonly BqlCommand _bAccountCommand;
		private readonly BqlCommand _contactCommand;
		private PXView _bAccountView;
		private PXView _contactView;

		private string BAccountRefFieldName
		{
			get { return BAccountRefField != null ? BAccountRefField.Name : EntityHelper.GetIDField(_bAccountView.Cache); }
		}

		public Type BAccountRefField { get; set; }


		private string ContactRefFieldName
		{
			get { return ContactRefField != null ? ContactRefField.Name : EntityHelper.GetIDField(_contactView.Cache); }
		}

		public Type ContactRefField { get; set; }

		public bool Persistent { get; set; }

		public CRReferenceAttribute(Type bAccountSelect, Type contactSelect = null)
		{
			Persistent = false;

			if (bAccountSelect == null) throw new ArgumentNullException("bAccountSelect");

			if (typeof(IBqlSelect).IsAssignableFrom(bAccountSelect))
			{
				_bAccountCommand = BqlCommand.CreateInstance(bAccountSelect);
			}
			else if (typeof(IBqlField).IsAssignableFrom(bAccountSelect))
			{
				BAccountRefField = bAccountSelect;
			}
			else
			{
				throw new PXArgumentException("sel", PXMessages.LocalizeFormatNoPrefixNLA(Messages.IncorrectSelectExpression, bAccountSelect.Name));
			}

			if (contactSelect != null && typeof(IBqlSelect).IsAssignableFrom(contactSelect))
			{
				_contactCommand = BqlCommand.CreateInstance(contactSelect);
			}
			else if (typeof(IBqlField).IsAssignableFrom(contactSelect))
			{
				ContactRefField = contactSelect;
			}

		}

		public override void ViewCreated(PXGraph graph, string viewName)
		{
			if (_bAccountCommand != null)
				_bAccountView = new PXView(graph, true, _bAccountCommand);

			graph.FieldDefaulting.AddHandler<CRActivity.bAccountID>(BAccountID_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<CRPMTimeActivity.bAccountID>(BAccountID_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<CRSMEmail.bAccountID>(BAccountID_FieldDefaulting);

			if (_contactCommand != null || ContactRefField != null)
			{
				if (_contactCommand != null)
					_contactView = new PXView(graph, true, _contactCommand);

				graph.FieldDefaulting.AddHandler<CRActivity.contactID>(ContactID_FieldDefaulting);
				graph.FieldDefaulting.AddHandler<CRPMTimeActivity.contactID>(ContactID_FieldDefaulting);
				graph.FieldDefaulting.AddHandler<CRSMEmail.contactID>(ContactID_FieldDefaulting);
			}

			if (Persistent)
			{
				graph.Views.Caches.Remove(typeof(CRActivity));
				graph.Views.Caches.Add(typeof(CRActivity));
				graph.RowPersisting.AddHandler(typeof(CRActivity), RowPersisting);

				graph.Views.Caches.Remove(typeof(CRPMTimeActivity));
				graph.Views.Caches.Add(typeof(CRPMTimeActivity));
				graph.RowPersisting.AddHandler(typeof(CRPMTimeActivity), RowPersisting);

				graph.Views.Caches.Remove(typeof(CRSMEmail));
				graph.Views.Caches.Add(typeof(CRSMEmail));
				graph.RowPersisting.AddHandler(typeof(CRSMEmail), RowPersisting);
			}
		}

		private int? GetBAccIDRef(PXCache sender)
		{
			if (_bAccountView != null)
			{
				object record = _bAccountView.SelectSingle();
				return GetRecordValue(sender, record, BAccountRefFieldName);
			}
			else if (BAccountRefField != null)
				return CetCurrentValue(sender, BAccountRefField);
			return null;
		}

		private int? GetContactIDRef(PXCache sender)
		{
			if (_contactView != null)
			{
				object record = _contactView.SelectSingle();
				return GetRecordValue(sender, record, ContactRefFieldName);
			}
			else if (ContactRefFieldName != null)
				return CetCurrentValue(sender, ContactRefField);
			return null;
		}

		private static int? GetRecordValue(PXCache sender, object record, string field)
		{
			return record != null ? (int?)sender.Graph.Caches[record.GetType()].GetValue(record, field) : null;
		}

		private static int? CetCurrentValue(PXCache sender, Type field)
		{
			if (field == null) return null;
			var cache = sender.Graph.Caches[field.DeclaringType];
			return (cache.Current != null)
				? cache.GetValue(cache.Current, field.Name) as int?
				: null;
		}

		private void BAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = GetBAccIDRef(sender);
		}

		private void ContactID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = GetContactIDRef(sender);
		}

		private void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (_bAccountView != null || BAccountRefField != null)
				sender.SetValue(e.Row, typeof(CRActivity.bAccountID).Name, GetBAccIDRef(_bAccountView?.Cache ?? sender.Graph.Caches[BAccountRefField.DeclaringType]));
			if (_contactView != null || ContactRefField != null)
				sender.SetValue(e.Row, typeof(CRActivity.contactID).Name, GetContactIDRef(_contactView?.Cache ?? sender.Graph.Caches[ContactRefField.DeclaringType]));
		}
	}

	/// <exclude/>
	[Obsolete]
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class CRDefaultMailToAttribute : CRActivityListBaseAttribute
	{
		private readonly bool _takeCurrent;

		public CRDefaultMailToAttribute()
			: base()
		{
			_takeCurrent = true;
		}

		public CRDefaultMailToAttribute(Type select)
			: base(select)
		{
		}

		protected override void AttachHandlers(PXGraph graph)
		{
			graph.RowInserting.AddHandler<CRSMEmail>(RowInserting);
		}

		private void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			var row = e.Row as CRSMEmail;
			if (row == null) return;

			string emailAddress = null;

			IEmailMessageTarget record;
			if (_takeCurrent)
			{
				var primaryCache = sender.Graph.GetPrimaryCache();
				record = primaryCache.Current as IEmailMessageTarget;
			}
			else
			{
				record = SelectRecord() as IEmailMessageTarget;
			}

			if (record != null)
			{
				var displayName = record.DisplayName.With(_ => _.Trim());
				var addresses = record.Address.With(_ => _.Trim());
				if (!string.IsNullOrEmpty(addresses))
				{
					emailAddress = PXDBEmailAttribute.FormatAddressesWithSingleDisplayName(addresses, displayName);
				}
			}
			row.MailTo = emailAddress;
		}
	}

	/// <exclude/>
	[Obsolete]
	public sealed class ProjectTaskActivities : PMActivityList<PMTask>
	{
		public ProjectTaskActivities(PXGraph graph)
			: base(graph)
		{
			_Graph.RowSelected.AddHandler<PMTask>(RowSelected);
			_Graph.RowInserting.AddHandler<CRPMTimeActivity>(RowInserting);
		}

		private void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PM.PMTask row = (PM.PMTask)e.Row;
			if (row == null || View == null || View.Cache == null)
				return;

			PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>.Select(_Graph);
			bool userCanAddActivity = true;
			if (project != null && project.RestrictToEmployeeList == true)
			{
				var select = new PXSelectJoin<EPEmployeeContract,
					InnerJoin<EPEmployee, On<EPEmployee.bAccountID, Equal<EPEmployeeContract.employeeID>>>,
					Where<EPEmployeeContract.contractID, Equal<Current<PMTask.projectID>>,
					And<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>>(_Graph);

				EPEmployeeContract record = select.SelectSingle();
				userCanAddActivity = record != null;
			}

			View.Cache.AllowInsert = userCanAddActivity;
		}

		private void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			var row = (CRPMTimeActivity)e.Row;
			if (row == null) return;

			row.ProjectID = ((PMTask)sender.Graph.Caches[typeof(PMTask)].Current).ProjectID;
			row.ProjectTaskID = ((PMTask)sender.Graph.Caches[typeof(PMTask)].Current).TaskID;
		}

		protected override void CreateTimeActivity(PXCache cache, int classId)
		{
			PXCache timeCache = cache.Graph.Caches[typeof(PMTimeActivity)];
			if (timeCache == null) return;

			PMTimeActivity timeActivity = (PMTimeActivity)timeCache.Current;
			if (timeActivity == null) return;

			bool withTimeTracking = classId != CRActivityClass.Task && classId != CRActivityClass.Event;

			timeCache.SetValue<PMTimeActivity.trackTime>(timeActivity, withTimeTracking);
			timeCache.SetValueExt<PMTimeActivity.projectID>(timeActivity, ((PMTask)_Graph.Caches[typeof(PMTask)].Current)?.ProjectID);
			timeCache.SetValueExt<PMTimeActivity.projectTaskID>(timeActivity, ((PMTask)_Graph.Caches[typeof(PMTask)].Current)?.TaskID);
		}

		public new static BqlCommand GenerateOriginalCommand()
		{
			return BqlCommand.CreateInstance(
				typeof(Select2<
					PMCRActivity,
				LeftJoin<CRReminder,
					On<CRReminder.refNoteID, Equal<PMCRActivity.noteID>>>,
				Where<
					PMCRActivity.projectTaskID, Equal<Current<PMTask.taskID>>>,
				OrderBy<
					Desc<PMCRActivity.timeActivityCreatedDateTime>>>));
		}

		protected override void SetCommandCondition(Delegate handler = null)
		{
			var command = ProjectTaskActivities.GenerateOriginalCommand();

			if (handler == null)
				View = new PXView(View.Graph, View.IsReadOnly, command);
			else
				View = new PXView(View.Graph, View.IsReadOnly, command, handler);
		}
	}

	/// <exclude/>
	[Obsolete]
	public sealed class ProjectActivities : PMActivityList<PMProject>
	{
		public ProjectActivities(PXGraph graph)
			: base(graph)
		{
			_Graph.RowSelected.AddHandler<PMProject>(PMProject_RowSelected);

			_Graph.FieldDefaulting.AddHandler<PMTimeActivity.projectID>(ProjectID_FieldDefaulting);
			_Graph.FieldDefaulting.AddHandler<PMTimeActivity.trackTime>(TrackTime_FieldDefaulting);
		}
		private void ProjectID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var primaryCache = sender.Graph.Caches[typeof(PMProject)];

			if (primaryCache.Current != null)
			{
				e.NewValue = ((PMProject)primaryCache.Current).ContractID;
			}

		}

		private void TrackTime_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = true;
		}

		private void PMProject_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PM.PMProject row = (PM.PMProject)e.Row;
			if (row == null || View == null)
				return;

			//if(View.Cache == null)
			if (View.Graph.Caches.TryGetValue(View.GetItemType(), out var v) && v == null)
				return;

			bool userCanAddActivity = row.Status != ProjectStatus.Completed;
			if (row.RestrictToEmployeeList == true && !sender.Graph.IsExport)
			{
				var select = new PXSelectJoin<EPEmployeeContract,
					InnerJoin<EPEmployee,
						On<EPEmployee.bAccountID, Equal<EPEmployeeContract.employeeID>>>,
					Where<EPEmployeeContract.contractID, Equal<Current<PMProject.contractID>>,
					And<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>>(_Graph);

				EPEmployeeContract record = select.SelectSingle();
				userCanAddActivity = userCanAddActivity && record != null;
			}

			View.Graph.Caches.SubscribeCacheCreated(View.GetItemType(), delegate
			{
				View.Cache.AllowInsert = userCanAddActivity;
			});


		}

		protected override void CreateTimeActivity(PXCache cache, int classId)
		{
			PXCache timeCache = cache.Graph.Caches[typeof(PMTimeActivity)];
			if (timeCache == null) return;

			PMTimeActivity timeActivity = (PMTimeActivity)timeCache.Current;
			if (timeActivity == null) return;

			bool withTimeTracking = classId != CRActivityClass.Task && classId != CRActivityClass.Event;

			timeActivity.TrackTime = withTimeTracking;
			timeActivity.ProjectID = ((PMProject)_Graph.Caches[typeof(PMProject)].Current)?.ContractID;

			timeCache.Update(timeActivity);
		}

		public new static BqlCommand GenerateOriginalCommand()
		{
			return BqlCommand.CreateInstance(
				typeof(Select2<
					PMCRActivity,
				LeftJoin<CRReminder,
					On<CRReminder.refNoteID, Equal<PMCRActivity.noteID>>>,
				Where<
					PMCRActivity.projectID, Equal<Current<PMProject.contractID>>>,
				OrderBy<
					Desc<PMCRActivity.timeActivityCreatedDateTime>>>));
		}

		protected override void SetCommandCondition(Delegate handler = null)
		{
			var command = ProjectActivities.GenerateOriginalCommand();

			if (handler == null)
				View = new PXView(View.Graph, View.IsReadOnly, command);
			else
				View = new PXView(View.Graph, View.IsReadOnly, command, handler);
		}
	}

	/// <exclude/>
	[Obsolete]
	public class PMActivityList<TPrimaryView> : CRActivityListBase<TPrimaryView, PMCRActivity>
		where TPrimaryView : class, IBqlTable, new()
	{
		public PMActivityList(PXGraph graph)
			: base(graph)
		{
		}
	}

	/// <exclude/>
	[Obsolete]
	public sealed class OpportunityActivities : CRActivityList<CROpportunity>
	{
		public OpportunityActivities(PXGraph graph)
			: base(graph)
		{
		}


		protected override void SetCommandCondition(Delegate handler = null)
		{
			var command = new SelectFrom<CRPMTimeActivity>
				.LeftJoin<Standalone.CROpportunityRevision>
					.On<Standalone.CROpportunityRevision.noteID.IsEqual<CRPMTimeActivity.refNoteID>
						.And<Standalone.CROpportunityRevision.opportunityID.IsEqual<CROpportunity.opportunityID.FromCurrent>>>
				.LeftJoin<CRReminder>
					.On<CRReminder.refNoteID.IsEqual<CRPMTimeActivity.noteID>>
				.Where<
					CRPMTimeActivity.refNoteID.IsEqual<CROpportunity.noteID.FromCurrent>
					.Or<CRPMTimeActivity.refNoteID.IsEqual<CROpportunity.quoteNoteID.FromCurrent>>
					.Or<
						Brackets<
							CROpportunityClass.showContactActivities.FromCurrent.IsEqual<True>
							.And<CRPMTimeActivity.refNoteID.IsEqual<CROpportunity.leadID.FromCurrent>>
						>
					>
				>
				.OrderBy<CRPMTimeActivity.createdDateTime.Desc>();

			if (handler == null)
				View = new PXView(View.Graph, View.IsReadOnly, command);
			else
				View = new PXView(View.Graph, View.IsReadOnly, command, handler);
		}

	}

	/// <exclude/>
	[Obsolete]
	public sealed class LeadActivities : CRActivityList<CRLead>
	{
		public LeadActivities(PXGraph graph)
			: base(graph) { }

		protected override void SetCommandCondition(Delegate handler = null)
		{
			var command = new SelectFrom<
					CRPMTimeActivity>
				.LeftJoin<CRReminder>
					.On<CRReminder.refNoteID.IsEqual<CRPMTimeActivity.noteID>>
				.Where<
					CRPMTimeActivity.refNoteID.IsEqual<CRLead.noteID.FromCurrent>
				>
				.OrderBy<
					CRPMTimeActivity.createdDateTime.Desc>();

			if (handler == null)
				View = new PXView(View.Graph, View.IsReadOnly, command);
			else
				View = new PXView(View.Graph, View.IsReadOnly, command, handler);
		}

	}

	/// <exclude/>
	[Obsolete]
	public sealed class SOOrderActivities : CRActivityList<SOOrder>
	{
		public SOOrderActivities(PXGraph graph)
			: base(graph) { }

		protected override string GetPrimaryRecipientFromContext(NotificationUtility utility, string type, object row, NotificationSource source)
		{
			var order = _Graph.Caches[typeof(SOOrder)].Current as SOOrder;

			if (order == null)
				return null;

			var contact = SOOrder.FK.Contact.FindParent(_Graph, order);

			if (contact == null || contact.EMail == null)
				return null;

			return contact.EMail;
		}
	}

	/// <exclude/>
	[Obsolete]
	public class CRCampaignMembersActivityList<TPrimaryView> : CRActivityListBase<TPrimaryView, CRPMTimeActivity>
		where TPrimaryView : class, IBqlTable, INotable, new()
	{
		protected internal const string _NEW_CAMPAIGNMEMBER_ACTIVITY_COMMAND = "NewCampaignMemberActivity";
		protected internal const string _NEW_CAMPAIGNMEMBER_TASK_COMMAND = "NewCampaignMemberTask";
		protected internal const string _NEW_CAMPAIGNMEMBER_EVENT_COMMAND = "NewCampaignMemberEvent";
		protected internal const string _NEW_CAMPAIGNMEMBER_MAILACTIVITY_COMMAND = "NewCampaignMemberMailActivity";

		public CRCampaignMembersActivityList(PXGraph graph)
			: base(graph)
		{
			AddCampginMembersActivityQuickActionsAsMenu(graph);
		}

		protected override void SetCommandCondition(Delegate handler = null)
		{
			var command = new SelectFrom<
					CRPMTimeActivity>
				.LeftJoin<CRReminder>
					.On<CRReminder.refNoteID.IsEqual<CRPMTimeActivity.noteID>>
				.Where<
					CRPMTimeActivity.documentNoteID.IsEqual<CRCampaign.noteID.FromCurrent>
					.Or<CRPMTimeActivity.refNoteID.IsEqual<CRCampaign.noteID.FromCurrent>>>
				.OrderBy<
					CRPMTimeActivity.createdDateTime.Desc>();

			if (handler == null)
				View = new PXView(View.Graph, View.IsReadOnly, command);
			else
				View = new PXView(View.Graph, View.IsReadOnly, command, handler);
		}

		private void AddCampginMembersActivityQuickActionsAsMenu(PXGraph graph)
		{
			List<ActivityService.IActivityType> types = null;
			try
			{
				var activityService = CommonServiceLocator.ServiceLocator.Current.GetInstance<IActivityService>();
				types = activityService.GetActivityTypes().ToList();
			}
			catch (Exception) {/* #46997 */}



			PXButtonAttribute btAtt = new PXButtonAttribute { OnClosingPopup = PXSpecialButtonType.Refresh, DisplayOnMainToolbar = false };

			PXAction btn = this.AddAction(graph, _NEW_CAMPAIGNMEMBER_ACTIVITY_COMMAND,
									PXMessages.LocalizeNoPrefix(Messages.AddActivity),
									types != null && types.Count > 0,
									NewCampaignMemberActivity, btAtt);

			if (types != null && types.Count > 0)
			{
				List<ButtonMenu> menuItems = new List<ButtonMenu>(types.Count);
				foreach (ActivityService.IActivityType type in types)
				{
					ButtonMenu menuItem = new ButtonMenu(type.Type,
						PXMessages.LocalizeFormatNoPrefix(Messages.AddTypedActivityFormat, type.Description), null);
					if (type.IsDefault == true)
						menuItems.Insert(0, menuItem);
					else
						menuItems.Add(menuItem);
				}
				var taskCommand = new ButtonMenu(_NEW_CAMPAIGNMEMBER_TASK_COMMAND, Messages.AddTask, null);
				menuItems.Add(taskCommand);
				var eventCommand = new ButtonMenu(_NEW_CAMPAIGNMEMBER_EVENT_COMMAND, Messages.AddEvent, null);
				menuItems.Add(eventCommand);
				var newEmailActivityCommand = new ButtonMenu(_NEW_CAMPAIGNMEMBER_MAILACTIVITY_COMMAND, Messages.AddEmail, null);
				menuItems.Add(newEmailActivityCommand);

				btn.SetMenu(menuItems.ToArray());
			}
		}

		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry, OnClosingPopup = PXSpecialButtonType.Refresh)]
		[PXShortCut(true, false, false, 'A', 'C')]
		public virtual IEnumerable NewCampaignMemberActivity(PXAdapter adapter)
		{
			string type = null;
			int clasId = CRActivityClass.Activity;
			switch (adapter.Menu)
			{
				case _NEW_CAMPAIGNMEMBER_TASK_COMMAND:
					clasId = CRActivityClass.Task;
					break;
				case _NEW_CAMPAIGNMEMBER_EVENT_COMMAND:
					clasId = CRActivityClass.Event;
					break;
				case _NEW_CAMPAIGNMEMBER_MAILACTIVITY_COMMAND:
					clasId = CRActivityClass.Email;
					break;
				default:
					type = adapter.Menu;
					break;
			}
			this.CreateCampaignMemberActivity(clasId, type);
			return adapter.Get();
		}

		private void CreateCampaignMemberActivity(int classId, string type)
		{
			var memberCache = this._Graph.Caches<CRCampaignMembers>();
			if (memberCache.Current is CRCampaignMembers currentMember)
			{
				var graph = CreateNewActivity(classId, type);

				if (graph == null)
					return;

				PXCache activityCache = null;
				if (classId == CRActivityClass.Email)
				{
					activityCache = graph.Caches<CRSMEmail>();
				}
				else
				{
					activityCache = graph.Caches<CRActivity>();
				}

				var result = SelectFrom<Contact>
					.LeftJoin<Standalone.CRLead>
						.On<Standalone.CRLead.contactID.IsEqual<Contact.contactID>>
					.Where<
						Contact.contactID.IsEqual<@P.AsInt>>
					.View
					.Select<PXResultset<Contact, Standalone.CRLead>>(this._Graph, currentMember.ContactID);

				var contact = (Contact)result;
				var lead = (Standalone.CRLead)result;

				if (lead?.ContactID != null)
				{
					activityCache.SetValue<CRActivity.refNoteID>(activityCache.Current, contact.NoteID);
					activityCache.SetValue<CRActivity.contactID>(activityCache.Current, lead.RefContactID);
				}
				else if (contact.ContactType == ContactTypesAttribute.Person)
				{
					activityCache.SetValue<CRActivity.contactID>(activityCache.Current, contact.ContactID);
				}

				var primaryCurrent = this._Graph.Caches[typeof(TPrimaryView)].Current as TPrimaryView;

				activityCache.SetValue<CRActivity.documentNoteID>(activityCache.Current, primaryCurrent?.NoteID);

				activityCache.SetValue<CRActivity.bAccountID>(activityCache.Current, contact.BAccountID);
				activityCache.SetValue<CRSMEmail.mailTo>(activityCache.Current, contact.EMail);
				activityCache.SetValue<CRSMEmail.mailReply>(activityCache.Current, contact.EMail);

				memberCache.ClearQueryCacheObsolete();
				memberCache.Clear();

				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
			}
		}
	}

	/// <exclude/>
	[Obsolete]
	public class CRChildActivityList<TParentActivity> : CRActivityListBase<TParentActivity, CRChildActivity>
		where TParentActivity : CRActivity, new()
	{
		public CRChildActivityList(PXGraph graph)
			: base(graph)
		{
			_Graph.FieldDefaulting.AddHandler<CRActivity.parentNoteID>(ParentNoteID_FieldDefaulting);
			_Graph.FieldDefaulting.AddHandler<CRChildActivity.parentNoteID>(ParentNoteID_FieldDefaulting);
			_Graph.FieldDefaulting.AddHandler<CRSMEmail.parentNoteID>(ParentNoteID_FieldDefaulting);

			_Graph.FieldDefaulting.AddHandler<PMTimeActivity.projectID>(ProjectID_FieldDefaulting);
			_Graph.FieldDefaulting.AddHandler<PMTimeActivity.projectTaskID>(ProjectTaskID_FieldDefaulting);
			_Graph.FieldDefaulting.AddHandler<PMTimeActivity.costCodeID>(ProjectCostCodeID_FieldDefaulting);
		}

		private void ProjectID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var primaryCache = sender.Graph.Caches[typeof(CRPMTimeActivity)];

			if (primaryCache.Current != null)
			{
				e.NewValue = ((CRPMTimeActivity)primaryCache.Current).ProjectID;
			}

		}

		private void ProjectTaskID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var primaryCache = sender.Graph.Caches[typeof(CRPMTimeActivity)];

			if (primaryCache.Current != null)
			{
				e.NewValue = ((CRPMTimeActivity)primaryCache.Current).ProjectTaskID;
			}

		}

		private void ProjectCostCodeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var primaryCache = sender.Graph.Caches[typeof(CRPMTimeActivity)];

			if (primaryCache.Current != null)
			{
				e.NewValue = ((CRPMTimeActivity)primaryCache.Current).CostCodeID;
			}

		}

		private void ParentNoteID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var parentCache = sender.Graph.Caches[typeof(TParentActivity)];

			if (parentCache.Current != null)
			{
				e.NewValue = ((TParentActivity)parentCache.Current).NoteID;
			}
		}

		[Obsolete]
		public IEnumerable SelectByParentNoteID(object parentNoteId)
		{
			return PXSelect<CRChildActivity,
				Where<CRChildActivity.parentNoteID, Equal<Required<CRChildActivity.parentNoteID>>>>.
				Select(_Graph, parentNoteId).RowCast<CRChildActivity>();
		}

		/// <summary>
		/// Check is any billable child time activity exist or not 
		/// </summary>
		/// <param name="parentNoteId"></param>
		/// <returns>true if exists, otherwise false</returns>
		public bool AnyBillableChildExists(object parentNoteId)
		{
			return SelectFrom<CRChildActivity>
					.Where<
						CRChildActivity.isBillable.IsEqual<True>
					.And<
						CRChildActivity.parentNoteID.IsEqual<@P.AsGuid>>>
					.View
					.Select(_Graph, parentNoteId)
					.Any();
		}

		/// <summary>
		/// Return time totals of children activities
		/// </summary>
		public (int timeSpent, int overtimeSpent, int timeBillable, int overtimeBillable)
			GetChildrenTimeTotals(object parentNoteId)
		{
			BqlCommand childActivitiescmd = View.BqlSelect.OrderByNew<BqlNone>();
			Type aggregate = BqlTemplate.FromType(
								typeof(Aggregate<
										Sum<CRChildActivity.timeSpent,
										Sum<CRChildActivity.overtimeSpent,
										Sum<CRChildActivity.timeBillable,
										Sum<CRChildActivity.overtimeBillable,
											GroupBy<CRChildActivity.parentNoteID>>>>>>))
							.ToType();
			childActivitiescmd = childActivitiescmd.AggregateNew(aggregate);

			var timeFields = new List<Type>()
				{
					typeof(CRChildActivity.timeSpent),
					typeof(CRChildActivity.overtimeSpent),
					typeof(CRChildActivity.timeBillable),
					typeof(CRChildActivity.overtimeBillable)
				};

			PXView childActivitiesView = new PXView(_Graph, true, childActivitiescmd);

			using (new PXFieldScope(childActivitiesView, timeFields, false))
			{
				CRChildActivity child = (childActivitiesView.SelectSingle(parentNoteId) as PXResult<CRChildActivity>);

				return (
							timeSpent: (child?.TimeSpent ?? 0),
							overtimeSpent: (child?.OvertimeSpent ?? 0),
							timeBillable: (child?.TimeBillable ?? 0),
							overtimeBillable: (child?.OvertimeBillable ?? 0)
						);
			}
		}

		protected override void ReadNoteIDFieldInfo(out string noteField, out Type noteBqlField)
		{
			noteField = typeof(CRActivity.refNoteID).Name;
			noteBqlField = _Graph.Caches[typeof(TParentActivity)].GetBqlField(noteField);
		}

		protected override void SetCommandCondition(Delegate handler = null)
		{
			var newCmd = OriginalCommand.WhereAnd(
				BqlCommand.Compose(typeof(Where<,,>),
					typeof(CRChildActivity.parentNoteID),
					typeof(Equal<>),
					typeof(Optional<>),
					typeof(TParentActivity).GetNestedType(typeof(CRActivity.noteID).Name),
					typeof(And<>),
					typeof(Where<,,>),
					typeof(CRChildActivity.isCorrected),
					typeof(NotEqual<True>),
					typeof(Or<,>),
					typeof(CRChildActivity.isCorrected),
					typeof(IsNull)));

			if (handler == null)
				View = new PXView(View.Graph, View.IsReadOnly, newCmd);
			else
				View = new PXView(View.Graph, View.IsReadOnly, newCmd, handler);
		}
	}

	/// <exclude/>
	[Obsolete]
	public class CRActivityList<TPrimaryView> : CRActivityListBase<TPrimaryView, CRPMTimeActivity>
		where TPrimaryView : class, IBqlTable, new()
	{
		public CRActivityList(PXGraph graph)
			: base(graph)
		{
		}

		public CRActivityList(PXGraph graph, Delegate handler)
			: base(graph, handler)
		{
		}

		public IEnumerable NewActivity(PXAdapter adapter, string type)
		{
			return NewActivityByType(adapter, type);
		}
	}

	/// <exclude/>
	[Obsolete]
	public interface IActivityList { }

	/// <exclude/>
	[Obsolete]
	public static class ActivityTypes
	{
		public const string Appointment = "E";
		public const string Escalation = "ES";
		public const string Message = "M";
		public const string Note = "N";
		public const string PhoneCall = "P";
		public const string WorkItem = "W";
	}

	/// <exclude/>
	[Obsolete]
	public abstract class CRActivityListBase<TActivity> : PXSelectBase<TActivity>
		where TActivity : CRPMTimeActivity, new()
	{
		protected internal const string _WORKFLOW = "_Workflow";

		public const string _NEWTASK_COMMAND = "NewTask";
		public const string _NEWEVENT_COMMAND = "NewEvent";
		public const string _VIEWACTIVITY_COMMAND = "ViewActivity";
		public const string _VIEWALLACTIVITIES_COMMAND = "ViewAllActivities";
		public const string _NEWACTIVITY_COMMAND = "NewActivity";
		public const string _NEWMAILACTIVITY_COMMAND = "NewMailActivity";
		public const string _REGISTERACTIVITY_COMMAND = "RegisterActivity";
		public const string _OPENACTIVITYOWNER_COMMAND = "OpenActivityOwner";

		public const string _NEWTASK_WORKFLOW_COMMAND = _NEWTASK_COMMAND + _WORKFLOW;
		public const string _NEWEVENT_WORKFLOW_COMMAND = _NEWEVENT_COMMAND + _WORKFLOW;
		public const string _VIEWACTIVITY_WORKFLOW_COMMAND = _VIEWACTIVITY_COMMAND + _WORKFLOW;
		public const string _VIEWALLACTIVITIES_WORKFLOW_COMMAND = _VIEWALLACTIVITIES_COMMAND + _WORKFLOW;
		public const string _NEWACTIVITY_WORKFLOW_COMMAND = _NEWACTIVITY_COMMAND + _WORKFLOW;
		public const string _NEWMAILACTIVITY_WORKFLOW_COMMAND = _NEWMAILACTIVITY_COMMAND + _WORKFLOW;
		public const string _REGISTERACTIVITY_WORKFLOW_COMMAND = _REGISTERACTIVITY_COMMAND + _WORKFLOW;
		public const string _OPENACTIVITYOWNER_WORKFLOW_COMMAND = _OPENACTIVITYOWNER_COMMAND + _WORKFLOW;

		public const string _NEWACTIVITY_APPOINTMENT_WORKFLOW_COMMAND = _NEWACTIVITY_COMMAND + ActivityTypes.Appointment + _WORKFLOW;
		public const string _NEWACTIVITY_ESCALATION_WORKFLOW_COMMAND = _NEWACTIVITY_COMMAND + ActivityTypes.Escalation + _WORKFLOW;
		public const string _NEWACTIVITY_MESSAGE_WORKFLOW_COMMAND = _NEWACTIVITY_COMMAND + ActivityTypes.Message + _WORKFLOW;
		public const string _NEWACTIVITY_NOTE_WORKFLOW_COMMAND = _NEWACTIVITY_COMMAND + ActivityTypes.Note + _WORKFLOW;
		public const string _NEWACTIVITY_PHONECALL_WORKFLOW_COMMAND = _NEWACTIVITY_COMMAND + ActivityTypes.PhoneCall + _WORKFLOW;
		public const string _NEWACTIVITY_WORKITEM_WORKFLOW_COMMAND = _NEWACTIVITY_COMMAND + ActivityTypes.WorkItem + _WORKFLOW;

		public static BqlCommand GenerateOriginalCommand()
		{
			var noteIdField = typeof(TActivity).GetNestedType(typeof(CRActivity.noteID).Name);
			var classIdField = typeof(TActivity).GetNestedType(typeof(CRActivity.classID).Name);
			var createdDateTimeField = typeof(TActivity).GetNestedType(typeof(CRActivity.createdDateTime).Name);

			return BqlCommand.CreateInstance(
				typeof(Select2<,,,>),
					typeof(TActivity),
				typeof(LeftJoin<,>), typeof(CRReminder),
					typeof(On<,>), typeof(CRReminder.refNoteID), typeof(Equal<>), noteIdField,
				typeof(Where<,>),
					classIdField, typeof(GreaterEqual<>), typeof(Zero),
				//typeof(And<,>), typeof(CRActivity.mpstatus), typeof(NotEqual<>), typeof(MailStatusListAttribute.deleted),
				typeof(OrderBy<>),
				typeof(Desc<>), createdDateTimeField);
		}
	}

	/// <exclude/>
	[Obsolete]
	[PXDynamicButton(new string[] { "NewTask", "NewEvent", "ViewActivity", "NewMailActivity", "RegisterActivity", "OpenActivityOwner", "ViewAllActivities", "NewActivity" },
					 new string[] { Messages.AddTask, Messages.AddEvent, Messages.Details, Messages.AddEmail, Messages.RegisterActivity, Messages.OpenActivityOwner, Messages.ViewAllActivities, Messages.AddActivity },
					 TranslationKeyType = typeof(Messages))]
	public class CRActivityListBase<TPrimaryView, TActivity> : CRActivityListBase<TActivity>, IActivityList
		where TPrimaryView : class, IBqlTable, new()
		where TActivity : CRPMTimeActivity, new()
	{
		#region Constants

		protected const string _PRIMARY_WORKGROUP_ID = "WorkgroupID";

		#endregion

		#region Fields

		public delegate string GetEmailHandler();

		private int? _entityDefaultEMailAccountId;

		private readonly BqlCommand _originalCommand;
		private readonly string _refField;
		private readonly Type _refBqlField;
		private EntityHelper _EntityHelper;
		public EntityHelper EntityHelper
		{
			get
			{
				if (_EntityHelper == null)
					_EntityHelper = new EntityHelper(_Graph);

				return _EntityHelper;
			}
		}

		#endregion

		#region Dependency Injection
		[InjectDependency]
		protected IReportLoaderService ReportLoader { get; private set; }

		[InjectDependency]
		protected IReportDataBinder ReportDataBinder { get; private set; }
		#endregion

		#region Ctor

		public CRActivityListBase(PXGraph graph) : this(graph, null)
		{
		}

		public CRActivityListBase(PXGraph graph, Delegate handler)
		{
			_Graph = graph;

			_Graph.EnsureCachePersistence(typeof(TActivity));
			_Graph.EnsureCachePersistence(typeof(CRReminder));

			var cache = _Graph.Caches[typeof(TActivity)];

			ReadNoteIDFieldInfo(out _refField, out _refBqlField);

			if (typeof(CRActivity).IsAssignableFrom(typeof(TPrimaryView)))
				graph.RowPersisted.AddHandler<TPrimaryView>(Table_RowPersisted);
			graph.RowSelected.AddHandler<TActivity>(Activity_RowSelected);

			graph.FieldDefaulting.AddHandler(typeof(CRActivity), typeof(CRActivity.refNoteID).Name, Activity_RefNoteID_FieldDefaulting);
			graph.FieldDefaulting.AddHandler(typeof(CRSMEmail), typeof(CRSMEmail.refNoteID).Name, Activity_RefNoteID_FieldDefaulting);
			graph.FieldDefaulting.AddHandler(typeof(TActivity), typeof(CRActivity.refNoteID).Name, Activity_RefNoteID_FieldDefaulting);

			graph.FieldSelecting.AddHandler(typeof(TActivity), typeof(CRActivity.body).Name, Activity_Body_FieldSelecting);

			AddActions(graph);
			AddPreview(graph);	// TODO: for BC, try deleting

			PXUIFieldAttribute.SetVisible(cache, null, typeof(CRActivity.noteID).Name, false);

			_originalCommand = GenerateOriginalCommand();

			if (handler == null)
				View = new PXView(graph, false, OriginalCommand);
			else
				View = new PXView(graph, false, OriginalCommand, handler);


			SetCommandCondition(handler);
		}

		#endregion

		#region Implementation

		#region Preview

		private void AddPreview(PXGraph graph)
		{
			// TODO: for BC, try deleting
			graph.Initialized += sender =>
			{
				string viewName;
				if (graph.ViewNames.TryGetValue(this.View, out viewName))
				{
					var att = new CRPreviewAttribute(typeof(TPrimaryView), typeof(TActivity));
					att.Attach(graph, viewName, null);
				}
			};
		}

		#endregion

		#region Add Actions
		protected void AddActions(PXGraph graph)
		{
			AddAction(graph, _NEWTASK_COMMAND, Messages.CreateTask, true, NewTask);
			AddAction(graph, _NEWEVENT_COMMAND, Messages.CreateEvent, true, NewEvent);
			AddAction(graph, _VIEWACTIVITY_COMMAND, Messages.Details, ViewActivity);
			AddAction(graph, _NEWMAILACTIVITY_COMMAND, Messages.CreateEmail, true, NewMailActivity);
			AddAction(graph, _OPENACTIVITYOWNER_COMMAND, string.Empty, OpenActivityOwner);
			AddAction(graph, _VIEWALLACTIVITIES_COMMAND, Messages.ViewAllActivities, ViewAllActivities);

			AddActivityQuickActionsAsMenu(graph);
		}

		private void AddActivityQuickActionsAsMenu(PXGraph graph)
		{
			List<ActivityService.IActivityType> types = null;
			try
			{
				var activityService = CommonServiceLocator.ServiceLocator.Current.GetInstance<IActivityService>();
				types = activityService.GetActivityTypes().ToList();
			}
			catch (Exception) {/* #46997 */}

			PXAction btn = AddAction(graph, _NEWACTIVITY_COMMAND,
								PXMessages.LocalizeNoPrefix(Messages.CreateActivity),
								types != null && types.Count > 0,
								NewActivityByType, new PXButtonAttribute() { DisplayOnMainToolbar = false, OnClosingPopup = PXSpecialButtonType.Refresh });

			if (types != null && types.Count > 0)
			{
				var sortedTypes = types.Where(t => t.IsDefault == true).Concat(types.Where(t => t.IsDefault != true));
				foreach (var type in sortedTypes)
				{
					var btn1 = AddAction(graph,
						_NEWACTIVITY_COMMAND + type.Type.TrimEnd(),
						PXMessages.LocalizeFormatNoPrefix(Messages.CreateTypedActivityFormat, type.Description),
						true,
						adapter => NewActivityByType(adapter, type.Type),
						new PXButtonAttribute() { CommitChanges = true, DisplayOnMainToolbar = false, OnClosingPopup = PXSpecialButtonType.Refresh });

					btn.AddMenuAction(btn1);
				}
			}
		}

		internal void AddAction(PXGraph graph, string name, string displayName, PXButtonDelegate handler)
		{
			AddAction(graph, name, displayName, true, handler, null);
		}

		internal PXAction AddAction(PXGraph graph, string name, string displayName, bool visible, PXButtonDelegate handler, params PXEventSubscriberAttribute[] attrs)
		{
			PXUIFieldAttribute uiAtt = new PXUIFieldAttribute
			{
				DisplayName = PXMessages.LocalizeNoPrefix(displayName),
				MapEnableRights = PXCacheRights.Select
			};
			if (!visible) uiAtt.Visible = false;
			List<PXEventSubscriberAttribute> addAttrs = new List<PXEventSubscriberAttribute> { uiAtt };
			if (attrs != null)
				addAttrs.AddRange(attrs.Where(attr => attr != null));

			PXNamedAction<TPrimaryView> res = new PXNamedAction<TPrimaryView>(graph, name, handler, addAttrs.ToArray());
			graph.Actions[name] = res;

			string workflowActionName = name + _WORKFLOW;
			var workflowAction = new PXNamedAction<TPrimaryView>(graph, workflowActionName, handler, addAttrs.ToArray());
			graph.Actions[workflowActionName] = workflowAction;

			return res;
		}
		#endregion

		protected void CreateActivity(int classId, string type)
		{
			var graph = CreateNewActivity(classId, type);

			if (graph != null)
				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
		}

		protected virtual PXGraph CreateNewActivity(int classId, string type)
		{
			// Dummy activity is created to search through the primary graphs.
			// Activity's Primary Graphs should be defined in proper way to handle non-inserted record (like Current<Activity.Class> EQUALS XX)
			object dummyActivityForGraphSearch = new CRActivity
			{
				ClassID = classId,
				Type = type
			};

			Type targetGraphType = EntityHelper.GetPrimaryGraphType(ref dummyActivityForGraphSearch, true);

			// TODO: NEED REFACTOR!

			if (!PXAccess.VerifyRights(targetGraphType))
			{
				_Graph.Views[_Graph.PrimaryView].Ask(null, Messages.AccessDenied, Messages.FormNoAccessRightsMessage(targetGraphType),
					MessageButtons.OK, MessageIcon.Error);
				return null;
			}
			else
			{
				PXCache cache;

				if (classId == CRActivityClass.Email)
				{
					cache = PXGraph.CreateInstance(targetGraphType).Caches[typeof(CRSMEmail)];
					if (cache == null) return null;

					var localActivity = (CRSMEmail)_Graph.Caches[typeof(CRSMEmail)].CreateInstance();

					localActivity.ClassID = classId;
					localActivity.Type = type;

					CRSMEmail email = ((PXCache<CRSMEmail>)_Graph.Caches[typeof(CRSMEmail)]).InitNewRow(localActivity);


					int? owner = EmployeeMaint.GetCurrentOwnerID(_Graph);
					int? workgroup = GetParentGroup();
					email.OwnerID = owner;
					if (email.OwnerID != null && OwnerAttribute.BelongsToWorkGroup(_Graph, workgroup, email.OwnerID))
						email.WorkgroupID = workgroup;


					email.MailAccountID = DefaultEMailAccountId;
					FillMailReply(email);
					FillMailTo(email);
					if (email.RefNoteID != null)
						FillMailCC(email, email.RefNoteID);
					FillMailSubject(email);
					email.Body = GenerateMailBody();

					email.ClassID = classId;
					_Graph.Caches[typeof(CRSMEmail)].SetValueExt(email, typeof(CRActivity.type).Name,
						!string.IsNullOrEmpty(type) ? type : email.Type);

					if (_Graph.IsDirty)
					{
						if (_Graph.IsMobile) // ensure that row will be persisted with Note when call from mobile
						{
							var rowCache = _Graph.Views[_Graph.PrimaryView].Cache;
							if (rowCache.Current != null)
							{
								rowCache.SetStatus(rowCache.Current, PXEntryStatus.Updated);
							}
						}
						_Graph.Actions.PressSave();
					}

					cache.Insert(email);
				}
				else
				{
					CRActivity activity;

					cache = PXGraph.CreateInstance(targetGraphType).Caches[typeof(CRActivity)];
					if (cache == null) return null;

					var localActivity = (CRActivity)_Graph.Caches[typeof(CRActivity)].CreateInstance();

					localActivity.ClassID = classId;
					localActivity.Type = type;

					activity = ((PXCache<CRActivity>)_Graph.Caches[typeof(CRActivity)]).InitNewRow(localActivity);

					int? owner = EmployeeMaint.GetCurrentOwnerID(_Graph);
					int? workgroup = GetParentGroup();
					activity.OwnerID = owner;
					if (activity.OwnerID != null && OwnerAttribute.BelongsToWorkGroup(_Graph, workgroup, activity.OwnerID))
						activity.WorkgroupID = workgroup;


					if (_Graph.IsDirty)
					{
						if (_Graph.IsMobile) // ensure that row will be persisted with Note when call from mobile
						{
							var rowCache = _Graph.Views[_Graph.PrimaryView].Cache;
							if (rowCache.Current != null)
							{
								rowCache.SetStatus(rowCache.Current, PXEntryStatus.Updated);
							}
						}

						_Graph.Actions.PressSave();

					}

					activity = cache.Insert(activity) as CRActivity;

					UDFHelper.CopyAttributes(_Graph.Views[_Graph.PrimaryView].Cache, _Graph.Views[_Graph.PrimaryView].Cache.Current, cache, activity, null);
				}

				CreateTimeActivity(cache, classId);

				foreach (PXCache dirtycache in cache.Graph.Caches.Caches.Where(c => c.IsDirty))
				{
					dirtycache.IsDirty = false;
				}
				return cache.Graph;
			}
		}

		protected virtual void CreateTimeActivity(PXCache graphType, int classId)
		{

		}

		private int? GetParentGroup()
		{
			PXCache cache = _Graph.Caches[typeof(TPrimaryView)];
			return (int?)cache.GetValue(cache.Current, _PRIMARY_WORKGROUP_ID);
		}

		protected PMTimeActivity CurrentTimeActivity
		{
			get
			{
				var tableCache = _Graph.Caches[typeof(TActivity)];
				return tableCache
					.With(_ => (TActivity)_.Current)
					.With(_ => _.TimeActivityNoteID ?? Guid.Empty)
					.With(_ => PXSelect<PMTimeActivity, Where<PMTimeActivity.noteID, Equal<Required<CRPMTimeActivity.timeActivityNoteID>>>>.Select(_Graph, _));
			}
		}

		public void SendNotification(string sourceType, string notifications, int? branchID, IDictionary<string, string> parameters, bool massProcess = false, IList<Guid?> attachments = null)
		{
			var sender = CreateNotificationProvider(sourceType,
				notifications,
				branchID,
				parameters,
				attachments);

			sender.MassProcessMode = massProcess;

			if (sender == null || !sender.Send().Any())
				throw new PXException(Messages.EmailNotificationError);
		}

		public virtual NotificationGenerator CreateNotificationProvider(string sourceType, string notifications, int? branchID,
			IDictionary<string, string> parameters, IList<Guid?> attachments = null)
		{
			if (notifications == null) return null;
			IList<string> list = notifications.Split(',')
				.Select(n => n?.Trim())
				.Where(cd => !string.IsNullOrEmpty(cd)).ToList();
			return CreateNotificationProvider(sourceType, list, branchID, parameters, attachments);
		}


		public virtual NotificationGenerator CreateNotificationProvider(string sourceType, IList<string> notificationCDs,
			int? branchID, IDictionary<string, string> parameters, IList<Guid?> attachments = null)
		{
			PXCache sourceCache = _Graph.Caches[typeof(TPrimaryView)];
			if (sourceCache.Current == null)
				throw new PXException(Messages.EmailNotificationObjectNotFound);

			IList<NotificationSetup> setupIDs = GetSetupNotifications(sourceType, notificationCDs);
			PXCache cache = _Graph.Caches[typeof(TPrimaryView)];
			TPrimaryView row = (TPrimaryView)cache.Current;

			object correctTypeRow = cache.CreateInstance();
			cache.RestoreCopy(correctTypeRow, row);
			row = (TPrimaryView)correctTypeRow;

			TActivity activity = ((PXCache<TActivity>)_Graph.Caches[typeof(TActivity)]).InitNewRow();
			var sourceRow = GetSourceRow(sourceType, activity);

			var utility = new NotificationUtility(_Graph);
			RecipientList recipients = null;
			TemplateNotificationGenerator sender = null;
			for (int i = 0; i < setupIDs.Count; i++)
			{
				NotificationSetup setup = setupIDs[i];
				NotificationSource source =
					sourceRow != null
						? utility.GetSource(sourceType, sourceRow, setup.SetupID.Value, branchID ?? this._Graph.Accessinfo.BranchID)
						: utility.GetSource(setup);

				if (source == null && sourceType == PMNotificationSource.Project)
				{
					source = utility.GetSource(sourceType, row, setup.SetupID.Value, branchID ?? this._Graph.Accessinfo.BranchID);
				}

				if (source == null)
				{
					throw new PXException(PX.SM.Messages.NotificationSourceNotFound);
				}

				if (sender == null)
				{
					var accountId = source.EMailAccountID ?? setup.EMailAccountID ?? DefaultEMailAccountId;
					if (accountId == null)
						throw new PXException(ErrorMessages.EmailNotConfigured);

					if (recipients == null)
						recipients = utility.GetRecipients(sourceType, sourceRow, source);

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

					ReportLoader.InitDefaultReportParameters(_report, parameters ?? KeyParameters(sourceCache));
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
							throw new InvalidOperationException(
								PXMessages.LocalizeFormatNoPrefixNLA(
									EP.Messages.EmailFromReportCannotBeCreated, source.ReportID));
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
							TemplateNotificationGenerator generator =
								TemplateNotificationGenerator.Create(row, message.TemplateID);

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

				var mainDocRecipient = GetPrimaryRecipientFromContext(utility, sourceType, sourceRow, source);
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

		[Obsolete]
		protected virtual RecipientList GetRecipientsFromContext(NotificationUtility utility, string type, object row, NotificationSource source) => null;
		protected virtual string GetPrimaryRecipientFromContext(NotificationUtility utility, string type, object row, NotificationSource source) => null;

		protected virtual object GetSourceRow(string sourceType, TActivity activity)
		{
			var sourceRow = activity.BAccountID.With(_ => new EntityHelper(_Graph).GetEntityRowByID(typeof(BAccountR), _.Value));

			if (sourceRow == null)
				sourceRow = activity.RefNoteID.With(_ => new EntityHelper(_Graph).GetEntityRow(typeof(BAccountR), _.Value));

			return sourceRow;
		}

		protected static IDictionary<string, string> KeyParameters(PXCache sourceCache)
		{
			IDictionary<string, string> parameters = new Dictionary<string, string>();
			foreach (string key in sourceCache.Keys)
			{
				object value = sourceCache.GetValueExt(sourceCache.Current, key);
				parameters[key] = value?.ToString();
			}
			return parameters;
		}

		protected Guid[] GetNotifications(string sourceType, IList<string> notificationCDs)
		{
			Guid[] setupIDs = new Guid[notificationCDs.Count];
			for (int i = 0; i < notificationCDs.Count; i++)
			{
				Guid? SetupID = new NotificationUtility(_Graph).SearchSetupID(sourceType, notificationCDs[i]);
				if (SetupID == null)
					throw new PXException(Messages.EmailNotificationSetupNotFound, notificationCDs[i]);
				setupIDs[i] = SetupID.Value;
			}
			return setupIDs;
		}

		protected List<NotificationSetup> GetSetupNotifications(string sourceType, IList<string> notificationCDs)
		{
			var setups = new List<NotificationSetup>();
			for (int i = 0; i < notificationCDs.Count; i++)
			{
				NotificationSetup setup = new NotificationUtility(_Graph).SearchSetup(sourceType, notificationCDs[i]);
				if (setup == null)
					throw new PXException(Messages.EmailNotificationSetupNotFound, notificationCDs[i]);
				setups.Add(setup);
			}
			return setups;
		}

		private static Guid? GetNoteId(PXGraph graph, object row)
		{
			if (row == null) return null;

			var rowType = row.GetType();
			var noteField = EntityHelper.GetNoteField(rowType);
			var cache = graph.Caches[rowType];
			return PXNoteAttribute.GetNoteID(cache, row, noteField);
		}

		private static bool IsHtml(string text)
		{
			if (!string.IsNullOrEmpty(text))
			{
				var htmlIndex = text.IndexOf("<html", StringComparison.CurrentCultureIgnoreCase);
				var bodyIndex = text.IndexOf("<body", StringComparison.CurrentCultureIgnoreCase);
				return htmlIndex > -1 && bodyIndex > -1 && bodyIndex > htmlIndex;
			}
			return false;
		}

		#endregion

		#region Email Methods

		protected virtual void FillMailReply(CRSMEmail message)
		{
			Mailbox mailAddress = null;

			var isCorrect = message.MailReply != null
				&& Mailbox.TryParse(message.MailReply, out mailAddress)
				&& !string.IsNullOrEmpty(mailAddress.Address);

			if (isCorrect)
			{
				isCorrect = PXSelect<EMailAccount,
					Where<EMailAccount.address, Equal<Required<EMailAccount.address>>>>
					.Select(_Graph, mailAddress.Address)
					.Count > 0;
			}

			var result = message.MailReply;

			if (!isCorrect)
			{
				result = DefaultEMailAccountId
					.With(_ => (EMailAccount)PXSelect<EMailAccount,
						Where<EMailAccount.emailAccountID, Equal<Required<EMailAccount.emailAccountID>>>>
						.Select(_Graph, _.Value))
					.With(_ => _.Address);
			}

			if (string.IsNullOrEmpty(result))
			{
				var firstAcct = (EMailAccount)PXSelect<EMailAccount>.SelectWindowed(_Graph, 0, 1);
				if (firstAcct != null) result = firstAcct.Address;
			}

			message.MailReply = result;
		}

		protected virtual void FillMailTo(CRSMEmail message)
		{
			string customMailTo = GetNewEmailAddress?.Invoke();

			if (!string.IsNullOrEmpty(customMailTo))
				message.MailTo = customMailTo.With(_ => _.Trim());
		}

		protected virtual void FillMailCC(CRSMEmail message, Guid? refNoteId)
		{
			if (refNoteId == null) return;

			var activityService = CommonServiceLocator.ServiceLocator.Current.GetInstance<IActivityService>();

			message.MailCc = PXDBEmailAttribute.AppendAddresses(message.MailCc, activityService?.GetEmailAddressesForCc(_Graph, refNoteId.Value));
		}

		protected virtual void FillMailSubject(CRSMEmail message)
		{
			if (!string.IsNullOrEmpty(DefaultSubject))
				message.Subject = DefaultSubject;
		}

		protected virtual string GenerateMailBody()
		{
			string res = null;

			res = MailAccountManager.AppendSignature(res, _Graph, MailAccountManager.SignatureOptions.Default);

			return PX.Web.UI.PXRichTextConverter.NormalizeHtml(res);
		}

		#endregion

		#region Actions

		[PXButton]
		[PXUIField(Visible = false)]
		public virtual IEnumerable OpenActivityOwner(PXAdapter adapter)
		{
			var act = Cache.Current as CRActivity;
			if (act != null)
			{
				var empl = (EPEmployee)PXSelectReadonly<EPEmployee,
					Where<EPEmployee.defContactID, Equal<Required<CRActivity.ownerID>>>>.
					Select(_Graph, act.OwnerID);
				if (empl != null)
					PXRedirectHelper.TryRedirect(_Graph.Caches[typeof(EPEmployee)], empl, string.Empty, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Task, OnClosingPopup = PXSpecialButtonType.Refresh, DisplayOnMainToolbar = false)]
		[PXShortCut(true, false, false, 'K', 'C')]
		public virtual IEnumerable NewTask(PXAdapter adapter)
		{
			CreateActivity(CRActivityClass.Task, null);
			return adapter.Get();
		}

		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Event, OnClosingPopup = PXSpecialButtonType.Refresh, DisplayOnMainToolbar = false)]
		[PXShortCut(true, false, false, 'E', 'C')]
		public virtual IEnumerable NewEvent(PXAdapter adapter)
		{
			CreateActivity(CRActivityClass.Event, null);
			return adapter.Get();
		}

		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry, OnClosingPopup = PXSpecialButtonType.Refresh)]
		[PXShortCut(true, false, false, 'A', 'C')]
		public virtual IEnumerable NewActivity(PXAdapter adapter)
		{
			return NewActivityByType(adapter);
		}

		[PXButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
		protected virtual IEnumerable NewActivityByType(PXAdapter adapter)
		{
			return NewActivityByType(adapter, adapter.Menu);
		}

		[PXButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
		protected virtual IEnumerable NewActivityByType(PXAdapter adapter, string type)
		{
			CreateActivity(CRActivityClass.Activity, type);
			return adapter.Get();
		}

		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.MailSend, OnClosingPopup = PXSpecialButtonType.Refresh, DisplayOnMainToolbar = false)]
		[PXShortCut(true, false, false, 'A', 'M')]
		public virtual IEnumerable NewMailActivity(PXAdapter adapter)
		{
			CreateActivity(CRActivityClass.Email, null);
			return adapter.Get();
		}

		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select)]
		[PXButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
		public virtual IEnumerable ViewAllActivities(PXAdapter adapter)
		{
			var gr = PXGraph.CreateInstance<ActivitiesMaint>();
			gr.Filter.Current.NoteID = ((PXCache<TActivity>)_Graph.Caches[typeof(TActivity)]).InitNewRow().RefNoteID;
			throw new PXPopupRedirectException(gr, string.Empty, true);
		}

		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select)]
		[PXButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
		public virtual IEnumerable ViewActivity(PXAdapter adapter)
		{
			NavigateToActivity(_Graph.Caches[typeof(TActivity)].Current);

			return adapter.Get();
		}

		public string DefaultActivityType
		{
			get
			{
				string result = null;

				EPSetup setup = PXSelect<EPSetup>.Select(_Graph);
				if (setup != null && !string.IsNullOrEmpty(setup.DefaultActivityType))
				{
					result = setup.DefaultActivityType;
				}

				return result;
			}
		}

		private void NavigateToActivity(object row)
		{
			Type primaryGraph = EntityHelper.GetPrimaryGraphType(row as CRActivity, true);
			PXGraph graph = PXGraph.CreateInstance(primaryGraph);

			PXCache rowCache = graph.Caches[row.GetType()];

			if (rowCache.GetValue(row, typeof(CRPMTimeActivity.timeActivityNoteID).Name) != null
				&& rowCache.GetValue(row, typeof(CRPMTimeActivity.timeActivityNoteID).Name).Equals(rowCache.GetValue(row, typeof(CRPMTimeActivity.timeActivityRefNoteID).Name)))
			{
				var timeAct = CurrentTimeActivity;

				PXCache parentCache = graph.Caches[typeof(CRActivity)];
				PXCache childCache = graph.Caches[typeof(PMTimeActivity)];

				var parentEntity = parentCache.NonDirtyInsert();

				parentCache.SetValue(parentEntity, typeof(CRActivity.noteID).Name, childCache.GetValue(timeAct, typeof(CRPMTimeActivity.refNoteID).Name));
				parentCache.SetValue(parentEntity, typeof(CRActivity.type).Name, DefaultActivityType);
				parentCache.SetValue(parentEntity, typeof(CRActivity.subject).Name, childCache.GetValue(timeAct, typeof(CRPMTimeActivity.summary).Name));
				parentCache.SetValue(parentEntity, typeof(CRActivity.ownerID).Name, childCache.GetValue(timeAct, typeof(CRPMTimeActivity.ownerID).Name));
				parentCache.SetValue(parentEntity, typeof(CRActivity.startDate).Name, childCache.GetValue(timeAct, typeof(CRPMTimeActivity.date).Name));
				parentCache.SetValue(parentEntity, typeof(CRActivity.uistatus).Name, ActivityStatusAttribute.Completed);
				parentCache.Normalize();

				childCache.SetValue(timeAct, typeof(PMTimeActivity.summary).Name, parentCache.GetValue(parentEntity, typeof(CRActivity.subject).Name));
				childCache.Current = timeAct;

				childCache.SetStatus(childCache.Current, PXEntryStatus.Updated);

				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
			}
			else
			{
				PXRedirectHelper.TryRedirect(graph, row, PXRedirectHelper.WindowMode.NewWindow);
			}
		}

		#endregion

		#region Buttons

		public PXAction ButtonViewAllActivities
		{
			get { return _Graph.Actions[_VIEWALLACTIVITIES_COMMAND]; }
		}

		#endregion

		#region Properties

		public virtual GetEmailHandler GetNewEmailAddress { get; set; }

		public string DefaultSubject { get; set; }

		public int? DefaultEMailAccountId
		{
			get
			{
				return
					MailAccountManager.GetUserSettingsEmailAccount(PXAccess.GetUserID(), true)?.EmailAccountID
					?? _entityDefaultEMailAccountId
					?? MailAccountManager.GetSystemSettingsEmailAccount(PXAccess.GetUserID(), true)?.EmailAccountID;
			}
			set { _entityDefaultEMailAccountId = value; }
		}

		#endregion

		#region Event Handlers

		internal void CorrectButtons(PXCache sender, object row, PXEntryStatus status)
		{
			row = row ?? sender.Current;
			var viewButtonsEnabled = row != null;

			viewButtonsEnabled = viewButtonsEnabled && Array.IndexOf(NotEditableStatuses, status) < 0;
			var editButtonEnabled = viewButtonsEnabled && this.View.Cache.AllowInsert;
			PXActionCollection actions = sender.Graph.Actions;

			actions[_NEWTASK_COMMAND].SetEnabled(editButtonEnabled);
			actions[_NEWTASK_WORKFLOW_COMMAND].SetEnabled(editButtonEnabled);

			actions[_NEWEVENT_COMMAND].SetEnabled(editButtonEnabled);
			actions[_NEWEVENT_WORKFLOW_COMMAND].SetEnabled(editButtonEnabled);

			actions[_NEWMAILACTIVITY_COMMAND].SetEnabled(editButtonEnabled);
			actions[_NEWMAILACTIVITY_WORKFLOW_COMMAND].SetEnabled(editButtonEnabled);

			actions[_NEWACTIVITY_COMMAND].SetEnabled(editButtonEnabled);

			PXButtonState state = actions[_NEWACTIVITY_COMMAND].GetState(row) as PXButtonState;
			if (state != null && state.Menus != null)
				foreach (var button in state.Menus)
				{
					actions[button.Command].SetEnabled(editButtonEnabled);
					actions[button.Command + _WORKFLOW].SetEnabled(editButtonEnabled);
				}
		}

		protected virtual void Table_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Completed)
			{
				object row = e.Row;
				CorrectButtons(sender, row, PXEntryStatus.Notchanged);
			}
		}

		protected virtual void Activity_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as TActivity;
			if (row == null) return;

			if (row.ClassID == CRActivityClass.Task || row.ClassID == CRActivityClass.Event)
			{
				int timespent = 0;
				int overtimespent = 0;
				int timebillable = 0;
				int overtimebillable = 0;

				foreach (PXResult<CRChildActivity, PMTimeActivity> child in
					PXSelectJoin<CRChildActivity,
						InnerJoin<PMTimeActivity,
							On<PMTimeActivity.refNoteID, Equal<CRChildActivity.noteID>>>,
						Where<CRChildActivity.parentNoteID, Equal<Required<CRChildActivity.parentNoteID>>,
							And<
								Where<PMTimeActivity.isCorrected, NotEqual<True>, Or<PMTimeActivity.isCorrected, IsNull>>>>>.
						Select(_Graph, row.NoteID))
				{
					var childTime = (PMTimeActivity)child;

					timespent += (childTime.TimeSpent ?? 0);
					overtimespent += (childTime.OvertimeSpent ?? 0);
					timebillable += (childTime.TimeBillable ?? 0);
					overtimebillable += (childTime.OvertimeBillable ?? 0);
				}

				row.TimeSpent = timespent;
				row.OvertimeSpent = overtimespent;
				row.TimeBillable = timebillable;
				row.OvertimeBillable = overtimebillable;
			}
		}

		protected virtual void Activity_RefNoteID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			GetNoteId(sender.Graph, sender.Graph.Caches[typeof(TPrimaryView)].Current);
			PXCache cache = sender.Graph.Caches[_refBqlField.DeclaringType];
			e.NewValue = cache.GetValue(cache.Current, _refBqlField.Name);
		}

		protected virtual void Activity_Body_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			var row = e.Row as TActivity;
			if (row == null) return;

			if (row.ClassID == CRActivityClass.Email)
			{
				var entity = (SMEmailBody)PXSelect<SMEmailBody, Where<SMEmailBody.refNoteID, Equal<Required<CRPMTimeActivity.noteID>>>>.Select(sender.Graph, row.NoteID);

				e.ReturnValue = entity.Body;

				PXContext.Session.SetString(string.Format("{0}+{1}", typeof(Note).FullName, row.NoteID), row.NoteID?.ToString());
			}
		}

		#endregion

		protected virtual void ReadNoteIDFieldInfo(out string noteField, out Type noteBqlField)
		{
			var cache = _Graph.Caches[typeof(TPrimaryView)];
			noteField = EntityHelper.GetNoteField(typeof(TPrimaryView));
			if (string.IsNullOrEmpty(_refField))
				throw new ArgumentException(
					string.Format("Type '{0}' must contain field with PX.Data.NoteIDAttribute on it",
								  typeof(TPrimaryView).GetLongName()));
			noteBqlField = cache.GetBqlField(_refField);
		}

		protected virtual void SetCommandCondition(Delegate handler = null)
		{
			Type refID;
			Type sourceID = _refBqlField;

			if (typeof(BAccount).IsAssignableFrom(typeof(TPrimaryView)))
			{
				refID = typeof(CRPMTimeActivity.bAccountID);
				sourceID = View.Graph.Caches[typeof(TPrimaryView)].GetBqlField(typeof(BAccount.bAccountID).Name);
			}
			else if (typeof(Contact).IsAssignableFrom(typeof(TPrimaryView)))
			{
				refID = typeof(CRPMTimeActivity.contactID);
				sourceID = View.Graph.Caches[typeof(TPrimaryView)].GetBqlField(typeof(Contact.contactID).Name);
			}
			else
			{
				refID = typeof(CRPMTimeActivity.refNoteID);
			}

			var newCmd = OriginalCommand.WhereAnd(
				BqlCommand.Compose(typeof(Where<,>),
					refID,                  // Activity
					typeof(Equal<>),
					typeof(Current<>),
					sourceID));         // Primary

			if (handler == null)
				View = new PXView(View.Graph, View.IsReadOnly, newCmd);
			else
				View = new PXView(View.Graph, View.IsReadOnly, newCmd, handler);
		}

		protected virtual PXEntryStatus[] NotEditableStatuses
		{
			get
			{
				return new[] { PXEntryStatus.Inserted, PXEntryStatus.InsertedDeleted };
			}
		}

		protected BqlCommand OriginalCommand
		{
			get { return _originalCommand; }
		}
	}

}
