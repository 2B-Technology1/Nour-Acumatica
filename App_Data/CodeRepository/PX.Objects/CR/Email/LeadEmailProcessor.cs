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
using PX.Data;
using PX.Objects.EP;



namespace PX.Objects.CR
{
	public class NewLeadEmailProcessor : BasicEmailProcessor
	{
		protected override bool Process(Package package)
		{
			var account = package.Account;
			if (account.IncomingProcessing != true ||
				account.CreateLead != true)
			{
				return false;
			}

			var message = package.Message;
			if (!string.IsNullOrEmpty(message.Exception)
				|| message.IsIncome != true
				|| message.RefNoteID != null
				|| message.ClassID == CRActivityClass.EmailRouting)
				return false;

			var copy = package.Graph.Caches[typeof(CRSMEmail)].CreateCopy(message);
			try
			{
				LeadMaint graph = PXGraph.CreateInstance<LeadMaint>();
				var dedupExt = graph.GetExtension<LeadMaint.CRDuplicateEntitiesForLeadGraphExt>();
				if (dedupExt != null)
				{
					dedupExt.HardBlockOnly = true;
				}
				var leadCache = graph.Lead.Cache;
				var lead = (CRLead)leadCache.Insert();
				lead = PXCache<CRLead>.CreateCopy(graph.Lead.Search<CRLead.contactID>(lead.ContactID));

				lead.EMail = package.Address;
				lead.LastName = package.Description;
				lead.RefContactID = message.ContactID;

				lead.OverrideRefContact = true;

				CREmailActivityMaint.EmailAddress address = CREmailActivityMaint.ParseNames(message.MailFrom);

				lead.FirstName = address.FirstName;
                lead.LastName = string.IsNullOrEmpty(address.LastName) ? address.Email : address.LastName;
				if (account.CreateLeadClassID != null)
					lead.ClassID = account.CreateLeadClassID;

				lead = (CRLead)leadCache.Update(lead);

				if (lead.ClassID != null)
				{
					CRLeadClass cls = PXSelect<
							CRLeadClass,
						Where<
							CRLeadClass.classID, Equal<Required<CRLeadClass.classID>>>>
						.SelectSingleBound(graph, null, lead.ClassID);

					if (cls?.DefaultOwner == CRDefaultOwnerAttribute.Source)
					{
						lead.WorkgroupID = message.WorkgroupID;
						lead.OwnerID = message.OwnerID;
					}
				}

				message.RefNoteID = PXNoteAttribute.GetNoteID<CRLead.noteID>(leadCache, lead);
				graph.Actions.PressSave();
			}
			catch (Exception e)
			{
				package.Graph.Caches[typeof(CRSMEmail)].RestoreCopy(message, copy);
				throw new PXException(Messages.CreateLeadException, e is PXOuterException ? ("\r\n" + String.Join("\r\n", ((PXOuterException)e).InnerMessages)) : e.Message);
			}

			return true;
		}
	}
}
