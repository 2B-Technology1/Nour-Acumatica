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
using System.Linq;
using System.Text;
using System.Web.Compilation;
using PX.Common;
using PX.Common.Mail;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CR;
using PX.Objects.CR.Extensions;
using PX.SM;

namespace PX.Objects.EP
{
	public class ActivityService : IActivityService
	{
		private PXView _viewShared;
		private PXView _viewCountShared;
		private PXView _view
		{
			get
			{
				return PXContext.GetSlot<PXView>(this.GetType().Name + "_view");
			}
			set
			{
				PXContext.SetSlot<PXView>(this.GetType().Name + "_view", value);
			}
		}
		private PXView _viewCount
		{
			get
			{
				return PXContext.GetSlot<PXView>(this.GetType().Name + "_viewCount");
			}
			set
			{
				PXContext.SetSlot<PXView>(this.GetType().Name + "_viewCount", value);
			}
		}

		private EntityHelper _EntityHelper;
		public EntityHelper EntityHelper
		{
			get
			{
				if (_EntityHelper == null)
					_EntityHelper = new EntityHelper(Graph);

				return _EntityHelper;
			}
		}

		public int GetCount(object refNoteID)
        {
            ViewCount.Clear();
            int startRow = 0;
            int totalRows = 0;
            var filters = InitFilters(refNoteID, null);

            var list = ViewCount.Select(null,
                new [] { refNoteID },
                null, 
				new [] { typeof(CRActivity.createdDateTime).Name },
                new[] { true }, filters.ToArray(), ref startRow, 0, ref totalRows);
            PXResult pxRes = (PXResult)list[0];
            return pxRes.RowCount.GetValueOrDefault(0);
        }

		public IEnumerable Select(object refNoteID, int? filterId = null)
		{
			View.Clear();
			int startRow = 0;
			int totalRows = 50;
            var filters = InitFilters(refNoteID, filterId);
            
			var list = View.Select(null, null, null,
                new [] { typeof(CRActivity.createdDateTime).Name }, 
                new [] { true }, 
                filters.ToArray(), ref startRow, 0, ref totalRows);		   
			return list.Select(row => row is PXResult ? ((PXResult)row)[0] : row);
		}

        private List<PXFilterRow> InitFilters(object refNoteID, int? filterId)
        {
            var filters = new List<PXFilterRow>();
            if (filterId != null)
            {

                var filterCache = View.Graph.Caches[typeof (FilterRow)];
                foreach (FilterRow row in PXSelect<FilterRow,
                    Where<FilterRow.filterID, Equal<Required<FilterRow.filterID>>>>.
                    Select(View.Graph, filterId))
                {
                    var item = new PXFilterRow(row.DataField,
                        (PXCondition) row.Condition,
                        filterCache.GetValueExt<FilterRow.valueSt>(row.ValueSt),
                        filterCache.GetValueExt<FilterRow.valueSt2>(row.ValueSt2))
                    {
                        OpenBrackets = row.OpenBrackets ?? 0,
                        CloseBrackets = row.CloseBrackets ?? 0,
                        OrOperator = row.Operator == 1
                    };
                    filters.Add(item);
                }
                if (filters.Count > 0)
                {
                    filters[0].OpenBrackets += 1;
                    filters[filters.Count - 1].CloseBrackets += 1;
                    filters[filters.Count - 1].OrOperator = false;
                }
            }

	        filters.Add(new PXFilterRow()
	        {
		        DataField = typeof (CRActivity.refNoteID).Name,
		        Condition = PXCondition.EQ,
		        Value = refNoteID
	        });
            
			return filters;
		}

		public virtual string GetKeys(object item)
		{
			var cache = View.Cache;
			var sb = new StringBuilder();
			var notFirst = false;
			foreach (string key in cache.Keys)
			{
				if (notFirst) sb.Append(',');
				sb.Append(cache.GetValue(item, key));
				notFirst = true;
			}
			return sb.ToString();
		}

		public virtual bool ShowTime(object item)
		{
			var act = (CRActivity)item;
			return Array.IndexOf(ActivitiesWithTime, act.ClassID) > -1;
		}

		public virtual DateTime? GetStartDate(object item)
		{
			var act = (CRActivity)item;
			return act.StartDate;
		}

		public virtual DateTime? GetEndDate(object item)
		{
			var act = (CRActivity)item;
			return act.EndDate;
		}

		public virtual void Cancel(string keys)
		{
			var act = SearchItem(keys) as CRActivity;
			if (act == null) return;

			var graphType = act.With(_ => EntityHelper.GetPrimaryGraphType(_, true));
			if (graphType != null)
			{
				var graph = Activator.CreateInstance(graphType) as IActivityMaint;
				if (graph != null) graph.CancelRow(act);
			}
		}

		public virtual void Complete(string keys)
		{
			var act = SearchItem(keys) as CRActivity;
			if (act == null) return;

			var graphType = act.With(_ => EntityHelper.GetPrimaryGraphType(_, true));
			if (graphType != null)
			{
				var graph = Activator.CreateInstance(graphType) as IActivityMaint;
				if (graph != null) graph.CompleteRow(act);
			}
		}

		public virtual void Defer(string keys, int minuts)
		{
		}

		public virtual void Dismiss(string keys)
		{
		}

		public virtual void Open(string keys)
		{
			var act = SearchItem(keys) as CRActivity;
			((TasksAndEventsReminder)Graph).NavigateToItem(act);
		}

		public virtual bool IsViewed(object item)
		{
			return true;
		}

		public virtual string GetImage(object item)
		{
			var act = (CRActivity)item;
			return act.ClassIcon;
		}

		public virtual string GetTitle(object item)
		{
			var act = (CRActivity)item;
			return act.Subject;
		}

		

		protected virtual object SearchItem(string keys)
		{
			return (CRActivity)PXSelect<CRActivity>.
				Search<CRActivity.noteID>(View.Graph, keys);
		}

		protected PXGraph Graph
		{
			get
			{
				return View.Graph;
			}
		}

		private PXView View
		{
			get
			{
				if (_view == null || _viewCount == null)
				{
					CreateView();					
				}
				return _view;
			}
		}

		private PXView ViewCount
		{
			get
			{
				if (_view == null || _viewCount == null)
				{
					CreateView();					
				}
				return _viewCount;
			}
		}

		protected virtual void CreateView()
		{
			var graph = PXGraph.CreateInstance<TasksAndEventsReminder>();
			_view = graph.ActivityList.View;
			_viewCount = graph.ActivityCount.View;
		}

		public virtual void ShowAll(object refNoteID)
		{
			if (refNoteID is Guid)
				((TasksAndEventsReminder)Graph).OpenInquiryScreen((Guid)refNoteID);
		}

		public virtual int[] ActivitiesWithTime
		{
			get
			{
				return new[] 
				{ 
					CRActivityClass.Event, CRActivityClass.Activity, CRActivityClass.Email 
				};
			}
		}

	    public virtual IEnumerable<PX.Data.EP.ActivityService.Total> GetCounts()
	    {
			TasksAndEventsReminder Reminder;
			if (_viewShared == null || _viewCountShared == null)
			{
				Reminder = (TasksAndEventsReminder)View.Graph;
			}
			else
			{
				Reminder = (TasksAndEventsReminder)_viewShared.Graph;
			}
			List<PX.Data.EP.ActivityService.Total> ret = new List<Data.EP.ActivityService.Total>();
			foreach (PX.Data.EP.ActivityService.Total total in Reminder.Counters.Select())
				ret.Add(total);
			if (_viewShared == null || _viewCountShared == null)
			{
				_viewShared = _view;
				_viewCountShared = _viewCount;
				_view = null;
				_viewCount = null;
			}
			return ret;
		}

		public class ActivityTypeDeinition : IPrefetchable
		{
			public readonly List<EPActivityType> List = new List<EPActivityType>();

			void IPrefetchable.Prefetch()
			{
				List.Clear();
				List.AddRange(
					PXSelect<EPActivityType,
					Where<EPActivityType.active, Equal<True>, And<EPActivityType.isInternal, Equal<True>, And<EPActivityType.isSystem, Equal<False>>>>>.Select(new PXGraph()).RowCast<EPActivityType>());
			}
		}

		public virtual IEnumerable<Data.EP.ActivityService.IActivityType> GetActivityTypes()
		{
			ActivityTypeDeinition def = PXDatabase.GetSlot<ActivityTypeDeinition>(typeof(EPActivityType).Name, typeof(EPActivityType));
			return def?.List;
		}
		
		public virtual void CreateTask(object refNoteID)
		{
			CreateActivity(CRActivityClass.Task, (Guid?)refNoteID, null, null);
		}
		
		public virtual void CreateEvent(object refNoteID)
		{
			CreateActivity(CRActivityClass.Event, (Guid?)refNoteID, null, null);
		}

		public virtual void CreateActivity(object refNoteID, string typeCode)
		{
			CreateActivity(refNoteID, typeCode, (int?)null);
		}

		public virtual void CreateActivity(object refNoteID, string typeCode, PXRedirectHelper.WindowMode windowMode = PXRedirectHelper.WindowMode.NewWindow)
		{
			CreateActivity(refNoteID, typeCode, (int?)null, windowMode);
		}

		[Obsolete]
		public virtual void CreateActivity(object refNoteID, string typeCode, Guid? obsoleteOwnerID, PXRedirectHelper.WindowMode windowMode = PXRedirectHelper.WindowMode.NewWindow)
		{
			CreateActivity(refNoteID, typeCode, obsoleteOwnerID != null ? PXAccess.GetContactID(obsoleteOwnerID) : (int?)null, windowMode);
		}
		
		public virtual void CreateActivity(object refNoteID, string typeCode, int? owner, PXRedirectHelper.WindowMode windowMode = PXRedirectHelper.WindowMode.NewWindow)
		{
			CreateActivity(CRActivityClass.Activity, (Guid?)refNoteID, typeCode, owner, windowMode);
		}

		public virtual void OpenMailPopup(string link)
		{
			PXGraph graph = PXGraph.CreateInstance<CREmailActivityMaint>();
			var activityCache = graph.Caches[typeof(CRSMEmail)];
			var newEmail = (CRSMEmail)activityCache.CreateInstance();
			FillMailAccount(newEmail);
			newEmail.Type = null;
			newEmail.IsIncome = false;
			var body = link;
			newEmail.Body = string.IsNullOrEmpty(body) ? PX.Web.UI.PXRichTextConverter.NormalizeHtml(link + newEmail.Body) : body;
			activityCache.Insert(newEmail);
			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
		}

		public virtual void CreateEmailActivity(object refNoteID, int EmailAccountID)
		{
			CreateEmailActivity(refNoteID, EmailAccountID, null);
		}

		public virtual void CreateEmailActivity(object refNoteID, int EmailAccountID, Action<object> initializeHandler)
		{
			// Dummy activity is created to search through the primary graphs.
			// Activity's Primary Graphs should be defined in proper way to handle non-inserted record (like Current<Activity.Class> EQUALS XX)
			object dummyActivityForGraphSearch = new CRActivity
			{
				ClassID = CRActivityClass.Email,
				Type = null
			};

			var graphType = dummyActivityForGraphSearch.With(_ => EntityHelper.GetPrimaryGraphType(_, true));

			if (!PXAccess.VerifyRights(graphType))
				throw new AccessViolationException(CR.Messages.FormNoAccessRightsMessage(graphType));

			var targetGraph = PXGraph.CreateInstance(graphType);

			DoCreateEmailActivity(targetGraph, refNoteID, EmailAccountID, initializeHandler);
		}

		public virtual void DoCreateEmailActivity(PXGraph targetGraph, object refNoteID, int EmailAccountID, Action<object> initializeHandler)
		{
			var cache = targetGraph.GetPrimaryCache();

			var newEmail = (CRSMEmail)cache.CreateInstance();
			newEmail.Type = null;
			newEmail.IsIncome = false;
			if (EmailAccountID != 0)
				FillMailAccount(newEmail, EmailAccountID);
			else
				FillMailAccount(newEmail);

			FillMailCC(targetGraph, newEmail, (Guid?)refNoteID);
			newEmail.RefNoteID = (Guid?)refNoteID;
			newEmail.Body = GenerateMailBody(targetGraph);

			if (initializeHandler != null)
				initializeHandler(newEmail);
			cache.Insert(newEmail);

			PXRedirectHelper.TryRedirect(targetGraph, PXRedirectHelper.WindowMode.NewWindow);
		}

		public virtual void CreateActivity(int classId, Guid? refNoteID, string typeCode, int? owner, PXRedirectHelper.WindowMode windowMode = PXRedirectHelper.WindowMode.NewWindow)
		{
			// Dummy activity is created to search through the primary graphs.
			// Activity's Primary Graphs should be defined in proper way to handle non-inserted record (like Current<Activity.Class> EQUALS XX)
			object dummyActivityForGraphSearch = new CRActivity
			{
				ClassID = classId,
				Type = typeCode
			};

			Type graphType = EntityHelper.GetPrimaryGraphType(ref dummyActivityForGraphSearch, true);

			if (!PXAccess.VerifyRights(graphType))
				throw new AccessViolationException(CR.Messages.FormNoAccessRightsMessage(graphType));

			var targetGraph = PXGraph.CreateInstance(graphType);

			DoCreateActivity(targetGraph, classId, refNoteID, typeCode, owner);

			PXRedirectHelper.TryRedirect(targetGraph, windowMode);
		}

		public virtual void DoCreateActivity(PXGraph targetGraph, int classId, Guid? refNoteID, string typeCode, int? owner)
		{
			CRActivity activity = null;

			var cache = targetGraph.GetPrimaryCache();

			if (owner == null)
				owner = EmployeeMaint.GetCurrentOwnerID(targetGraph);

			Action<object> initializeHandler = delegate (object act1)
			{
				var act = act1 as CRActivity;
				if (act == null) return;

				act.ClassID = classId;
				act.RefNoteID = refNoteID;
				if (!string.IsNullOrEmpty(typeCode))
					act.Type = typeCode;
				act.OwnerID = owner;
			};

			EntityHelper helper = new EntityHelper(targetGraph);
			var type = helper.GetEntityRowType(refNoteID);
			var entity = helper.GetEntityRow(type, refNoteID);

			Type entityGraphType = null;
			if (type != null)
				PXPrimaryGraphAttribute.FindPrimaryGraph(targetGraph.Caches[type], ref entity, out entityGraphType);
			if (entityGraphType != null)
			{
				PXGraph entry = PXGraph.CreateInstance(entityGraphType);
				PXCache<CRActivity> activityCache = entry.Caches[typeof(CRActivity)] as PXCache<CRActivity>;
				if (activityCache != null)
				{
					entry.Views[entry.PrimaryView].Cache.Current = entity;
					activity = (CRActivity)activityCache.CreateInstance();
					if (initializeHandler != null)
						initializeHandler(activity);
					activity = activityCache.InitNewRow(activity);
				}
			}

			if (activity == null)
			{
				activity = (CRActivity)cache.CreateInstance();

				initializeHandler(activity);

				activity = ((PXCache<CRActivity>)cache).InitNewRow(activity);
			}

			activity = cache.Update(activity) as CRActivity;

			if (type != null)
			{
				UDFHelper.CopyAttributes(targetGraph.Caches[type], entity, cache, activity, activity?.Type);
			}
		}

		public virtual string GenerateMailBody(PXGraph graph)
		{
			var res = MailAccountManager.AppendSignature(null, graph, MailAccountManager.SignatureOptions.Default);

			return PX.Web.UI.PXRichTextConverter.NormalizeHtml(res);
		}

		public virtual void FillMailAccount(CRSMEmail message)
		{
			message.MailAccountID = MailAccountManager.DefaultMailAccountID;
		}

		public virtual void FillMailAccount(CRSMEmail message,int MailAccountID)
		{
			message.MailAccountID = MailAccountID;
		}

		public virtual void FillMailCC(PXGraph graph, CRSMEmail message, Guid? refNoteId)
		{
			if (refNoteId == null) return;

			message.MailCc = PXDBEmailAttribute.AppendAddresses(message.MailCc, GetEmailAddressesForCc(graph, refNoteId.Value));
		}

		public virtual string GetEmailAddressesForCc(PXGraph graph, Guid? refNoteID)
		{
			string result = String.Empty;

			var command = new Select2<
					CRRelation,
				LeftJoin<Contact,
					On<Contact.contactID, Equal<CRRelation.contactID>>,
				LeftJoin<BAccount,
					On<BAccount.bAccountID, Equal<CRRelation.entityID>>,
				LeftJoin<Users,
					On<Users.pKID, Equal<Contact.userID>>>>>,
				Where<
					CRRelation.refNoteID, Equal<Required<CRRelation.refNoteID>>>>();

			var list = new PXView(graph, false, command).SelectMulti(refNoteID);

			foreach (PXResult<CRRelation, Contact, BAccount, Users> row in list)
			{
				var relation = row.GetItem<CRRelation>();

				if (relation?.AddToCC != true)
					continue;

				var contact = row.GetItem<Contact>();
				var businessAccount = row.GetItem<BAccount>();
				var user = row.GetItem<Users>();

				CRRelation.FillUnboundData(relation, contact, businessAccount, user);

				if (relation.Email != null && (relation.Email = relation.Email.Trim()) != string.Empty)
					result = PXDBEmailAttribute.AppendAddresses(result, PXDBEmailAttribute.FormatAddressesWithSingleDisplayName(relation.Email, relation.ContactName ?? relation.Name));
			}

			return result;
		}
	}
}
