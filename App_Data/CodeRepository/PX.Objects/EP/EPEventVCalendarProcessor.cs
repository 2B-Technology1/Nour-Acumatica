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
using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Export.Imc;
using PX.Objects.EP.Imc;
using PX.Objects.CR;
using PX.SM;

namespace PX.Objects.EP
{
	public class EPEventVCalendarProcessor : IVCalendarProcessor
	{
		private PXSelectBase<CRActivity> _infoSelect;
		private PXSelectBase<EPAttendee> _attendees;

		private PXGraph _graph;

		protected PXGraph Graph
		{
			get { return _graph ?? (_graph = new PXGraph()); }
		}

		public virtual void Process(vEvent card, object item)
		{
			var row = item as CRActivity;
			if (row == null) return;

			FillCommon(card, row);
			FillOrganizer(card, row);
			FillAttendee(card, row);
		}

		private PXSelectBase<CRActivity> InfoSelect
		{
			get
			{
				if (_infoSelect == null)
				{
					_infoSelect = 
						new PXSelectJoin<CRActivity,
							LeftJoin<EPEmployee,
								On<EPEmployee.defContactID, Equal<CRActivity.ownerID>>,
							LeftJoin<Contact,
								On<Contact.contactID, Equal<EPEmployee.defContactID>>,
							LeftJoin<Users,
								On<Users.pKID, Equal<EPEmployee.userID>>>>>,
							Where<
								CRActivity.noteID, Equal<Required<CRActivity.noteID>>>>(Graph);
				}
				return _infoSelect;
			}
		}

		private PXSelectBase<EPAttendee> Attendees
		{
			get
			{
				if (_attendees == null)
				{
					_attendees =
						new PXSelectJoin<EPAttendee,
							LeftJoin<Contact,
								On<Contact.contactID, Equal<EPAttendee.contactID>>,
							LeftJoin<EPEmployee,
								On<EPEmployee.defContactID, Equal<Contact.contactID>>,
							LeftJoin<Users,
								On<Users.pKID, Equal<EPEmployee.userID>>>>>,
							Where<
								EPAttendee.eventNoteID, Equal<Required<EPAttendee.eventNoteID>>>>(Graph);
				}
				return _attendees;
			}
		}

		private void FillAttendee(vEvent card, CRActivity row)
		{
			Attendees.View.Clear();
			foreach (PXResult<EPAttendee, Contact, EPEmployee, Users> item in Attendees.Select(row.NoteID))
			{
				var attendee = (EPAttendee)item[typeof(EPAttendee)];
				var user = (Users)item[typeof(Users)];
				var contact = (Contact)item[typeof(Contact)];
				string fullName;
				string email;
				var status = vEvent.Attendee.Statuses.NeedAction;
				if (contact is null)
					(fullName, email) = (null, attendee.Email);
				else
				{
					ExtractAttendeeInfo(user, contact, out fullName, out email);
					switch (attendee.Invitation)
					{
						case PXInvitationStatusAttribute.ACCEPTED:
							status = vEvent.Attendee.Statuses.Accepted;
							break;
						case PXInvitationStatusAttribute.REJECTED:
							status = vEvent.Attendee.Statuses.Declined;
							break;
					}
				}
				card.Attendees.Add(
					new vEvent.Attendee(
						fullName,
						email,
						status));
			}
		}

		private static void FillCommon(vEvent card, CRActivity row)
		{
			if (row.StartDate == null)
				throw new ArgumentNullException("row", Messages.NullStartDate);

			var timeZone = LocaleInfo.GetTimeZone();
			var startDate = PXTimeZoneInfo.ConvertTimeToUtc((DateTime)row.StartDate, timeZone);
			card.Summary = row.Subject;
			card.IsHtml = true;
			card.Description = row.Body;
			card.StartDate = startDate;
			card.EndDate = row.EndDate.HasValue
								? PXTimeZoneInfo.ConvertTimeToUtc((DateTime)row.EndDate, timeZone)
								: startDate;
			card.Location = row.Location;
			card.IsPrivate = row.IsPrivate ?? false;
			card.UID = "ACUMATICA_" + row.NoteID;
			card.CreateDate = PXTimeZoneInfo.ConvertTimeToUtc((DateTime)row.CreatedDateTime, timeZone);
		}

		private void FillOrganizer(vEvent card, CRActivity row)
		{
			InfoSelect.View.Clear();
			var set = InfoSelect.Select(row.NoteID);
			if (set == null || set.Count == 0) return;

			var owner = (Users)set[0][typeof(Users)];
			var contact = (Contact)set[0][typeof(Contact)];
			string fullName;
			string email;
			ExtractAttendeeInfo(owner, contact, out fullName, out email);

			card.OrganizerName = fullName;
			card.OrganizerEmail = email;

			if (!string.IsNullOrEmpty(fullName))
				card.Attendees.Add(new vEvent.Attendee(
					fullName,
					email,
					vEvent.Attendee.Statuses.Accepted,
					vEvent.Attendee.Rules.Chair));
		}

		private static void ExtractAttendeeInfo(Users user, Contact contact, out string fullName, out string email)
		{
			fullName = user.FullName;
			email = user.Email;

			if (contact.DisplayName != null && contact.DisplayName.Trim().Length > 0)
				fullName = contact.DisplayName;
			if (contact.EMail != null && contact.EMail.Trim().Length > 0)
				email = contact.EMail;
		}
	}
}
