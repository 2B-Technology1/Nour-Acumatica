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

using PX.Data;
using PX.SM;
using PX.Objects.CR;
using System;

namespace PX.Objects.EP
{
	public class ConversationEmailProcessor : BasicEmailProcessor
	{
		protected override bool Process(Package package)
		{
			CRSMEmail activity = package.Message;
			EMailAccount account = package.Account;
			PXGraph graph = package.Graph;

		    if (activity.Ticket != null
				|| activity.ResponseToNoteID != null
		        || account.EmailAccountType != EmailAccountTypesAttribute.Exchange
		        || !(account.IncomingProcessing ?? false))
		    {
		        return false;
		    }

			if (String.IsNullOrEmpty(activity.MessageId) || String.IsNullOrEmpty(activity.MessageReference)) return false;

			foreach (String id in activity.MessageReference.Split(new string[] { "> <", ">,<" }, StringSplitOptions.RemoveEmptyEntries))
			{
				string refId = id;
				if (refId.IndexOf('<') != 0) refId = refId.Insert(0, "<");
				if (refId.LastIndexOf('>') != refId.Length - 1) refId = refId.Insert(refId.Length, ">");

				Boolean found = false;
				foreach (CRSMEmail parent in PXSelect<CRSMEmail, Where<CRSMEmail.messageId, Like<Required<CRSMEmail.messageId>>>>.SelectSingleBound(graph, null, refId))
				{
					activity.Ticket = parent.ID;
					found = true;
					break;
				}
				if (found) return true;
			}

			return false;
		}
	}
}
