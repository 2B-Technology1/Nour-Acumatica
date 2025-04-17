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

using PX.Objects.PJ.Common.Services;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.PJ.Submittals.PJ.DAC;
using PX.Objects.PJ.Submittals.PJ.Graphs;
using PX.Objects.CR;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PJ.Submittals.PJ.Services
{
    public class SubmittalEmailService : EmailActivityService<PJSubmittal>
    {
        private readonly SubmittalEntry SubmittalGraph;

        public SubmittalEmailService(SubmittalEntry graph)
            : base(graph, graph.Submittals.Current.OwnerID)
        {
            SubmittalGraph = graph;
            Entity = graph.Submittals.Current;
        }

        public string EmailSubject => $"SU #[{Entity.SubmittalID}-{Entity.RevisionID} {GetProjectNumber()}] {Entity.Summary}";

        public PXGraph GetEmailActivityGraph()
        {
            return GetEmailActivityGraph<PJSubmittal.noteID>();
        }

        public override string GetRecipientEmails()
        {
            var cache = SubmittalGraph.Caches<PJSubmittalWorkflowItem>();

			var selectedContacts = SubmittalGraph.SubmittalWorkflowItems.Select()
				.RowCast<PJSubmittalWorkflowItem>()
				.Where(it => it.EmailTo == true && it.ContactID != null)
				.Select(it => it.ContactID)
				.ToArray();

			var emailList = SelectFrom<Contact>
                .Where<Contact.contactID.IsIn<P.AsInt>>
                .View
                .Select(SubmittalGraph, selectedContacts)
                .FirstTableItems
                .Select(contact => contact.EMail)
                .Where(email => !string.IsNullOrWhiteSpace(email))
                .ToList();

			foreach (PJSubmittalWorkflowItem item in SubmittalGraph.SubmittalWorkflowItems.Select())
			{
				item.EmailTo = false;
				SubmittalGraph.SubmittalWorkflowItems.Cache.Update(item);
			}

            return string.Join(";", emailList);
        }

        protected override string GetSubject() => EmailSubject;
    }
}
