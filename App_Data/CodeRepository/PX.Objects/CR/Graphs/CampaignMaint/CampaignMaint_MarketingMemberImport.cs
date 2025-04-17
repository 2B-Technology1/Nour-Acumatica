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
using PX.Objects.CR.Extensions;

namespace PX.Objects.CR
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class CampaignMaint_MarketingMemberImport : MarketingMemberImport<CampaignMaint, CRCampaign, CRCampaignMembers>
	{
		[PXOverride]
		public virtual int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, object[] parameters, ExecuteUpdateDelegate del)
		{
			if (viewName == nameof(Base.CampaignMembers) && Base.IsImportFromExcel)
			{
				return del(nameof(this.ImportEntity), keys, values, parameters);
			}
			return del(viewName, keys, values, parameters);
		}

		public override void GenerateDummyRowsForProcessingResultsGrid(List<CRCampaign> list, List<CRMarketingMemberForImport> membersToCreate)
		{
			list.Add(Base.Campaign.Current);

			for (int i = 0; i < membersToCreate.Count - 1; i++)
			{
				list.Add(new CRCampaign()
				{
					Selected = true,
					CampaignID = i.ToString("D12").Replace('0', 'Y')
				});
			}
		}

		public override void InsertContactMarketingInfoIfNotExists(ContactMaint graph, Contact contact, CRMarketingMemberForImport newMember)
		{
			if (CRCampaignMembers.PK.Find(graph, newMember.LinkCampaignID, contact.ContactID) != null)
				return;

			graph.Members.Insert(new CRCampaignMembers()
			{
				CampaignID = newMember.LinkCampaignID,
				ContactID = contact.ContactID
			});
		}

		public override void InsertLeadMarketingInfoIfNotExtsis(LeadMaint graph, CRLead lead, CRMarketingMemberForImport newMember)
		{
			if (CRCampaignMembers.PK.Find(graph, newMember.LinkCampaignID, lead.ContactID) != null)
				return;

			graph.Members.Insert(new CRCampaignMembers()
			{
				CampaignID = newMember.LinkCampaignID,
				ContactID = lead.ContactID
			});
		}

		public override void InsertBAMarketingInfoIfNotExists(BusinessAccountMaint graph, int? contactID, CRMarketingMemberForImport newMember)
		{
			if (CRCampaignMembers.PK.Find(graph, newMember.LinkCampaignID, contactID) != null)
				return;

			var result = graph.Members.Insert(new CRCampaignMembers()
			{
				CampaignID = newMember.LinkCampaignID,
				ContactID = contactID
			});

			PXDBDefaultAttribute.SetDefaultForInsert<CRCampaignMembers.contactID>(graph.Members.Cache, result, false);
		}

		public override bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			if (String.Compare(viewName, nameof(Base.CampaignMembers), StringComparison.OrdinalIgnoreCase) != 0)
				return true;

			values[nameof(CRMarketingMemberForImport.LinkCampaignID)] = Base.Campaign.Current?.CampaignID;

			return base.PrepareImportRow(viewName, keys, values);
		}
	}
}
