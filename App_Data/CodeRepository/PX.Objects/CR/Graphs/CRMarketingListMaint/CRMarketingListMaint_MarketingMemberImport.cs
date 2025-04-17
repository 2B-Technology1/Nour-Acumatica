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
using PX.Data;
using PX.Objects.CR.BusinessAccountMaint_Extensions;
using PX.Objects.CR.ContactMaint_Extensions;
using PX.Objects.CR.Extensions;
using PX.Objects.CR.LeadMaint_Extensions;

namespace PX.Objects.CR
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class CRMarketingListMaint_MarketingMemberImport : MarketingMemberImport<CRMarketingListMaint, CRMarketingList, CRMarketingListMember>
	{
		[PXOverride]
		public virtual int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, object[] parameters, ExecuteUpdateDelegate del)
		{
			if (viewName == nameof(Base.ListMembers) && Base.IsImportFromExcel)
			{
				return del(nameof(this.ImportEntity), keys, values, parameters);
			}
			return del(viewName, keys, values, parameters);
		}

		public override void GenerateDummyRowsForProcessingResultsGrid(List<CRMarketingList> list, List<CRMarketingMemberForImport> membersToCreate)
		{
			list.Add(Base.MailLists.Current);

			for (int i = 0; i < membersToCreate.Count - 1; i++)
			{
				list.Add(new CRMarketingList()
				{
					Selected = true,
					MarketingListID = i,
					MailListCode = i.ToString("D12").Replace('0', 'Y')
				});
			}
		}

		public override void InsertContactMarketingInfoIfNotExists(ContactMaint graph, Contact contact, CRMarketingMemberForImport newMember)
		{
			if (CRMarketingListMember.PK.Find(graph, newMember.LinkMarketingListID, contact.ContactID) != null)
				return;

			var marketingExt = graph.GetExtension<ContactMaint_MarketingListDetailsExt>();

			marketingExt.Subscriptions.Insert(new CRMarketingListMember()
			{
				MarketingListID = newMember.LinkMarketingListID,
				ContactID = contact.ContactID,
				IsSubscribed = true
			});
		}

		public override void InsertLeadMarketingInfoIfNotExtsis(LeadMaint graph, CRLead lead, CRMarketingMemberForImport newMember)
		{
			if (CRMarketingListMember.PK.Find(graph, newMember.LinkMarketingListID, lead.ContactID) != null)
				return;

			var marketingExt = graph.GetExtension<LeadMaint_MarketingListDetailsExt>();

			marketingExt.Subscriptions.Insert(new CRMarketingListMember()
			{
				MarketingListID = newMember.LinkMarketingListID,
				ContactID = lead.ContactID,
				IsSubscribed = true
			});
		}

		public override void InsertBAMarketingInfoIfNotExists(BusinessAccountMaint graph, int? contactID, CRMarketingMemberForImport newMember)
		{
			if (CRMarketingListMember.PK.Find(graph, newMember.LinkMarketingListID, contactID) != null)
				return;

			var marketingExt = graph.GetExtension<BusinessAccountMaint_MarketingListDetailsExt>();

			var result = marketingExt.Subscriptions.Insert(new CRMarketingListMember()
			{
				MarketingListID = newMember.LinkMarketingListID,
				ContactID = contactID,
				IsSubscribed = true
			});

			PXDBDefaultAttribute.SetDefaultForInsert<CRMarketingListMember.contactID>(marketingExt.Subscriptions.Cache, result, false);
		}

		public override bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			if (String.Compare(viewName, nameof(Base.ListMembers), StringComparison.OrdinalIgnoreCase) != 0)
				return true;

			values[nameof(CRMarketingMemberForImport.LinkMarketingListID)] = Base.MailLists.Current?.MarketingListID;

			return base.PrepareImportRow(viewName, keys, values);
		}
	}
}
