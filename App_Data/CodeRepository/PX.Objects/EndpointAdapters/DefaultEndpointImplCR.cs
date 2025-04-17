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

using PX.Api;
using PX.Api.ContractBased;
using PX.Api.ContractBased.Adapters;
using PX.Api.ContractBased.Models;
using PX.Data;
using PX.Objects.CR;
using System;
using System.Linq;

namespace PX.Objects.EndpointAdapters
{
	[PXVersion("17.200.001", "Default")]
	[PXVersion("18.200.001", "Default")]
	internal class DefaultEndpointImplCR : DefaultEndpointImplCRBase
	{
		public DefaultEndpointImplCR(
			CbApiWorkflowApplicator.CaseApplicator caseApplicator,
			CbApiWorkflowApplicator.OpportunityApplicator opportunityApplicator,
			CbApiWorkflowApplicator.LeadApplicator leadApplicator)
			: base(
				caseApplicator,
				opportunityApplicator,
				leadApplicator)
		{
		}

		[FieldsProcessed(new[] { "OpportunityID", "Status", "Subject" })]
		protected virtual void Opportunity_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			if (!(graph is OpportunityMaint))
				return;

			var current = EnsureAndGetCurrentForInsert<CROpportunity>(graph, o =>
			{
				o.OpportunityID = entity.Fields.OfType<EntityValueField>()
					.FirstOrDefault(f => f.Name == "OpportunityID")?.Value;
				o.Subject = entity.Fields.OfType<EntityValueField>()
					.FirstOrDefault(f => f.Name == "Subject")?.Value;
				return o;
			});

			OpportunityApplicator.ApplyStatusChange(graph, MetadataProvider, entity, current);
		}

		[FieldsProcessed(new[] { "Status" })]
		protected virtual void Opportunity_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			if (!(graph is OpportunityMaint))
				return;

			var current = EnsureAndGetCurrentForUpdate<CROpportunity>(graph);
			OpportunityApplicator.ApplyStatusChange(graph, MetadataProvider, entity, current);
		}

		[FieldsProcessed(new[] { "CaseID", "Status" })]
		protected virtual void Case_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			if (!(graph is CRCaseMaint))
				return;

			var current = EnsureAndGetCurrentForInsert<CRCase>(graph, o =>
			{
				o.CaseCD = entity.Fields.OfType<EntityValueField>()
					.FirstOrDefault(f => f.Name == "CaseID")?.Value;
				return o;
			});

			CaseApplicator.ApplyStatusChange(graph, MetadataProvider, entity, current);
		}

		[FieldsProcessed(new[] { "Status" })]
		protected virtual void Case_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			if (!(graph is CRCaseMaint))
				return;

			var current = EnsureAndGetCurrentForUpdate<CRCase>(graph);
			CaseApplicator.ApplyStatusChange(graph, MetadataProvider, entity, current);
		}

		[FieldsProcessed(new[] { "ContactID", "Status" })]
		protected virtual void Lead_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			if (!(graph is LeadMaint))
				return;

			var current = EnsureAndGetCurrentForInsert<CRLead>(graph, o =>
			{
				var contactID = entity.Fields.OfType<EntityValueField>()
					.FirstOrDefault(f => f.Name == "ContactID")?.Value;
				o.ContactID = contactID == null ? (int?)null : int.Parse(contactID);
				return o;
			});

			LeadApplicator.ApplyStatusChange(graph, MetadataProvider, entity, current);
		}

		[FieldsProcessed(new[] { "Status" })]
		protected virtual void Lead_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			if (!(graph is LeadMaint))
				return;

			var current = EnsureAndGetCurrentForUpdate<CRLead>(graph);
			LeadApplicator.ApplyStatusChange(graph, MetadataProvider, entity, current);
		}
	}
}
