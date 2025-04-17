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
using PX.Common.Mail;
using PX.Data;
using PX.Objects.CR;
using PX.SM;

namespace PX.Objects.EP
{
	public class DefaultEmailProcessor : BasicEmailProcessor
	{
		protected override bool Process(Package package)
		{
			EMailAccount account = package.Account;
			CRSMEmail message = package.Message;
			if (message.RefNoteID != null) return false;
			if (message.Ticket == null) return false;

			var graph = package.Graph;
			if (message.StartDate == null)
				message.StartDate = PXTimeZoneInfo.Now;

			//Evaludate sender only if standart email processing
			if(account.EmailAccountType == EmailAccountTypesAttribute.Standard)
				message.OwnerID = GetKnownSender(graph, message);

			var parentMessage = GetParentOriginalEmail(graph, (int)message.Ticket);
			if (parentMessage == null) return false;

			message.ResponseToNoteID = parentMessage.EmailNoteID;
			message.ParentNoteID = parentMessage.ParentNoteID;
			message.RefNoteID = parentMessage.RefNoteID;
			message.BAccountID = parentMessage.BAccountID;
			message.ContactID = parentMessage.ContactID;
			message.DocumentNoteID = parentMessage.DocumentNoteID;

			if (parentMessage.ProjectID != null)
			{
				var timeAct = (PMTimeActivity)graph.Caches[typeof(PMTimeActivity)].Insert();

				timeAct.ProjectID = parentMessage.ProjectID;
				timeAct.ProjectTaskID = parentMessage.ProjectTaskID;

				graph.Caches[typeof(PMTimeActivity)].Update(timeAct);
			}
			
			message.IsPrivate = parentMessage.IsPrivate;

			if (message.OwnerID == null && account.EmailAccountType == EmailAccountTypesAttribute.Standard)
			{
				try
				{
					message.WorkgroupID = parentMessage.WorkgroupID;
					graph.Caches[typeof(CRSMEmail)].SetValueExt<CRSMEmail.ownerID>(message, parentMessage.OwnerID);
				}
				catch (PXSetPropertyException)
				{
					message.OwnerID = null;
				}				
			}
			return true;
		}
		
		private int? GetKnownSender(PXGraph graph, CRSMEmail message)
		{
			var @from = Mailbox.Parse(message.MailFrom).With(_ => _.Address).With(_ => _.Trim());

			PXSelect<Contact,
				Where2<
					Where<
						Contact.eMail, Equal<Required<Contact.eMail>>,
						And<Contact.contactType, Equal<ContactTypesAttribute.employee>>>,
					And<Contact.userID, IsNotNull>>>
				.Clear(graph);

				var contactByEmail = (Contact)PXSelect<Contact,
					Where2<
						Where<
							Contact.eMail, Equal<Required<Contact.eMail>>,
							And<Contact.contactType, Equal<ContactTypesAttribute.employee>>>,
						And<Contact.userID, IsNotNull>>>
				.Select(graph, @from);

			if (contactByEmail != null) return contactByEmail.ContactID;

			return null;
		}

		/// <summary>
		/// Find email by <see cref="CRPMSMEmail.id"/> and return <see cref="CRPMSMEmail"/> in response to which this email was created.
		/// </summary>
		/// <param name="graph"></param>
		/// <param name="id"><see cref="CRPMSMEmail.id"/></param>
		/// <returns></returns>
		public static CRPMSMEmail GetParentOriginalEmail(PXGraph graph, int id)
		{
			PXSelectReadonly<CRPMSMEmail,
				Where<CRPMSMEmail.id, Equal<Required<CRPMSMEmail.id>>>>.
				Clear(graph);

			var res = (CRPMSMEmail)PXSelectReadonly<CRPMSMEmail,
				Where<CRPMSMEmail.id, Equal<Required<CRPMSMEmail.id>>>>.
				Select(graph, id);

			while (res != null && res.ClassID == CRActivityClass.EmailRouting)
			{
				if (res.ResponseToNoteID == null) res = null;
				else
					res = (CRPMSMEmail)PXSelectReadonly<CRPMSMEmail,
							Where<CRPMSMEmail.emailNoteID, Equal<Required<CRPMSMEmail.emailNoteID>>>>.
							Select(graph, res.ResponseToNoteID);
			}
			return res;
		}
	}
}
