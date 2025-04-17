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
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CR.Extensions;

namespace PX.Objects.CR.CampaignMaint_Extensions
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class CampaignMaint_ActivityDetailsExt_Actions : ActivityDetailsExt_Actions<CampaignMaint_ActivityDetailsExt, CampaignMaint, CRCampaign, CRCampaign.noteID> { }

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class CampaignMaint_ActivityDetailsExt : ActivityDetailsExt<CampaignMaint, CRCampaign, CRCampaign.noteID>
	{
		#region State

		protected internal const string _NEW_CAMPAIGNMEMBER_TASK_COMMAND = "NewCampaignMemberTask";
		protected internal const string _NEW_CAMPAIGNMEMBER_EVENT_COMMAND = "NewCampaignMemberEvent";
		protected internal const string _NEW_CAMPAIGNMEMBER_MAILACTIVITY_COMMAND = "NewCampaignMemberMailActivity";

		public override Type GetLinkConditionClause() => typeof(
			Where<
				CRPMTimeActivity.documentNoteID.IsEqual<CRCampaign.noteID.FromCurrent>
				.Or<CRPMTimeActivity.refNoteID.IsEqual<CRCampaign.noteID.FromCurrent>>>);

		#endregion

		#region ctor

		public override void Initialize()
		{
			base.Initialize();

			Actions_AddCampginMembersActivityQuickActionsAsMenu();
		}

		#endregion

		#region Actions

		public PXAction<CRCampaign> NewCampaignMemberActivity;
		[PXButton(OnClosingPopup = PXSpecialButtonType.Refresh, DisplayOnMainToolbar = false)]
		[PXUIField(DisplayName = Messages.AddActivity)]
		public virtual IEnumerable newCampaignMemberActivity(PXAdapter adapter)
		{
			string type = null;
			int classID = CRActivityClass.Activity;

			switch (adapter.Menu)
			{
				case _NEW_CAMPAIGNMEMBER_TASK_COMMAND:
					classID = CRActivityClass.Task;
					break;
				case _NEW_CAMPAIGNMEMBER_EVENT_COMMAND:
					classID = CRActivityClass.Event;
					break;
				case _NEW_CAMPAIGNMEMBER_MAILACTIVITY_COMMAND:
					classID = CRActivityClass.Email;
					break;
				default:
					type = adapter.Menu;
					break;
			}

			CreateCampaignMemberActivity(classID, type);

			return adapter.Get();
		}

		public virtual void Actions_AddCampginMembersActivityQuickActionsAsMenu()
		{
			List<PX.Data.EP.ActivityService.IActivityType> types = null;

			try
			{
				types = ActivityService.GetActivityTypes().ToList();
			}
			catch (Exception)
			{
				/* #46997 */
			}

			if (types == null || types.Count <= 0)
				return;

			var menuItems = new List<ButtonMenu>(types.Count)
			{
				new ButtonMenu(_NEW_CAMPAIGNMEMBER_TASK_COMMAND, Messages.AddTask, null),
				new ButtonMenu(_NEW_CAMPAIGNMEMBER_EVENT_COMMAND, Messages.AddEvent, null),
				new ButtonMenu(_NEW_CAMPAIGNMEMBER_MAILACTIVITY_COMMAND, Messages.AddEmail, null)
			};

			foreach (PX.Data.EP.ActivityService.IActivityType type in types)
			{
				ButtonMenu menuItem = new ButtonMenu(type.Type, PXMessages.LocalizeFormatNoPrefix(Messages.AddTypedActivityFormat, type.Description), null);

				if (type.IsDefault == true)
					menuItems.Insert(3, menuItem);
				else
					menuItems.Add(menuItem);
			}

			NewCampaignMemberActivity.SetMenu(menuItems.ToArray());
		}

		public virtual void CreateCampaignMemberActivity(int classId, string type)
		{
			var memberCache = Base.Caches<CRCampaignMembers>();
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

				var result = SelectFrom<
						Contact>
					.LeftJoin<CRLead>
						.On<CRLead.contactID.IsEqual<Contact.contactID>>
					.LeftJoin<BAccount>
						.On<BAccount.defContactID.IsEqual<Contact.contactID>>
					.Where<
						Contact.contactID.IsEqual<@P.AsInt>>
					.View
					.Select<PXResultset<Contact, CRLead, BAccount>>(Base, currentMember.ContactID);

				var contact = (Contact)result;
				var lead = (CRLead)result;
				var baccount = (BAccount)result;

				if (lead?.ContactID != null)
				{
					activityCache.SetValue<CRActivity.refNoteIDType>(activityCache.Current, typeof(CRLead).FullName);
					activityCache.SetValue<CRActivity.refNoteID>(activityCache.Current, PXNoteAttribute.GetNoteID(graph.Caches[typeof(CRLead)], lead, EntityHelper.GetNoteField(typeof(CRLead))));
					activityCache.SetValue<CRActivity.contactID>(activityCache.Current, lead.RefContactID);
				}
				else if (contact.ContactType == ContactTypesAttribute.Person)
				{
					activityCache.SetValue<CRActivity.refNoteIDType>(activityCache.Current, typeof(Contact).FullName);
					activityCache.SetValue<CRActivity.refNoteID>(activityCache.Current, PXNoteAttribute.GetNoteID(graph.Caches[typeof(Contact)], contact, EntityHelper.GetNoteField(typeof(Contact))));
					activityCache.SetValue<CRActivity.contactID>(activityCache.Current, contact.ContactID);
				}
				else if (contact.ContactType == ContactTypesAttribute.BAccountProperty)
				{
					activityCache.SetValue<CRActivity.refNoteIDType>(activityCache.Current, typeof(BAccount).FullName);
					activityCache.SetValue<CRActivity.refNoteID>(activityCache.Current, PXNoteAttribute.GetNoteID(graph.Caches[typeof(BAccount)], baccount, EntityHelper.GetNoteField(typeof(BAccount))));
					activityCache.SetValue<CRActivity.contactID>(activityCache.Current, baccount.BAccountID);
				}

				var primaryCurrent = Base.Caches[typeof(CRCampaign)].Current as CRCampaign;

				activityCache.SetValue<CRActivity.documentNoteID>(activityCache.Current, primaryCurrent?.NoteID);

				activityCache.SetValue<CRActivity.bAccountID>(activityCache.Current, contact.BAccountID);
				activityCache.SetValue<CRSMEmail.mailTo>(activityCache.Current, contact.EMail);
				activityCache.SetValue<CRSMEmail.mailReply>(activityCache.Current, contact.EMail);

				memberCache.ClearQueryCacheObsolete();
				memberCache.Clear();

				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
			}
		}

		#endregion
	}
}
