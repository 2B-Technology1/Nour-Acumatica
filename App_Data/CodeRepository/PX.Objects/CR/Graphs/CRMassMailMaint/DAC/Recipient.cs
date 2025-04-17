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
using PX.Objects.CS;
using PX.Objects.EP;

namespace PX.Objects.CR
{
	[PXInternalUseOnly]
	public class Recipient
	{
		public Recipient(Contact contact, string format, object entity = null, int? bAccountID = null, int? contactID = null, Guid? refNoteID = null, Guid? documentNoteID = null)
		{
			Contact = contact;
			Format = format;
			Entity = entity;
			BAccountID = bAccountID;
			ContactID = contactID;
			RefNoteID = refNoteID;
			DocumentNoteID = documentNoteID;
		}

		public static Recipient SmartCreate(EntityHelper helper, Contact contact, object entity = null, CRLead lead = null, string format = NotificationFormat.Html)
		{
			entity = entity ?? contact;

			return lead != null && lead.ContactID != null
				? new Recipient(
					contact: contact,
					format: format,
					entity: entity,
					bAccountID: contact.BAccountID,
					contactID: lead.RefContactID,
					refNoteID: helper.GetNoteIDAndEnsureNoteExists(contact),
					documentNoteID: helper.GetNoteIDAndEnsureNoteExists(entity)
				)
				: new Recipient(
					contact: contact,
					format: format,
					entity: entity,
					bAccountID: contact.BAccountID,
					contactID: contact.ContactID,
					documentNoteID: helper.GetNoteIDAndEnsureNoteExists(entity)
				);
		}

		public Contact Contact { get; }
		public object Entity { get; }
		public string Format { get; }
		public Guid? DocumentNoteID { get; }
		public Guid? RefNoteID { get; }
		public int? BAccountID { get; }
		public int? ContactID { get; }

		public TemplateNotificationGenerator GetSender(PXGraph graph, CRMassMail massMail)
		{
			var sender = TemplateNotificationGenerator.Create(graph, Contact);

			sender.MailAccountId = massMail.MailAccountID ?? MailAccountManager.DefaultMailAccountID;
			sender.To = massMail.MailTo;
			sender.Cc = massMail.MailCc;
			sender.Bcc = massMail.MailBcc;
			sender.Body = massMail.MailContent ?? string.Empty;
			sender.Subject = massMail.MailSubject;
			sender.AttachmentsID = massMail.NoteID;

			sender.BAccountID = BAccountID;
			sender.BodyFormat = Format;
			sender.RefNoteID = RefNoteID;
			sender.ContactID = ContactID;
			sender.DocumentNoteID = DocumentNoteID;

			return sender;
		}
	}
}
