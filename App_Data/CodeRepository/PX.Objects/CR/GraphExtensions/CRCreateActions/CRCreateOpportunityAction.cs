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

using PX.Common;
using PX.CS;
using PX.Data;
using PX.Objects.IN;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;

namespace PX.Objects.CR.Extensions.CRCreateActions
{
	/// <exclude/>
	public abstract partial class CRCreateOpportunityAction<TGraph, TMain>
		: CRCreateActionBase<
			TGraph,
			TMain,
			OpportunityMaint,
			CROpportunity,
			OpportunityFilter,
			OpportunityConversionOptions>
		where TGraph : PXGraph, new()
		where TMain : class, IBqlTable, new()
	{
		#region Ctor

		protected override ICRValidationFilter[] AdditionalFilters => new ICRValidationFilter[] { OpportunityInfoAttributes, OpportunityInfoUDF };

		#endregion

		#region Views

		[PXHidden]
		[PXCopyPasteHiddenView]
		public CRValidationFilter<OpportunityFilter> OpportunityInfo;
		protected override CRValidationFilter<OpportunityFilter> FilterInfo => OpportunityInfo;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public CRValidationFilter<PopupAttributes> OpportunityInfoAttributes;
		protected virtual IEnumerable opportunityInfoAttributes()
		{
			return GetFilledAttributes();
		}

		[PXHidden]
		[PXCopyPasteHiddenView]
		public CRValidationFilter<PopupUDFAttributes> OpportunityInfoUDF;
		protected virtual IEnumerable<PopupUDFAttributes> opportunityInfoUDF()
		{
			return GetRequiredUDFFields();
		}

		#endregion

		#region Events

		public virtual void _(Events.FieldDefaulting<OpportunityFilter, OpportunityFilter.subject> e)
		{
			e.NewValue = Documents.Current?.Description?.Replace("\r\n", " ")?.Replace("\n", " ");
		}

		public virtual void _(Events.FieldUpdated<OpportunityFilter, OpportunityFilter.opportunityClass> e)
		{
			Base.Caches<PopupAttributes>().Clear();
		}

		#endregion

		#region Actions
		public PXAction<TMain> ConvertToOpportunity;
		[PXUIField(DisplayName = Messages.CreateOpportunity, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton(DisplayOnMainToolbar = false)]
		public virtual IEnumerable convertToOpportunity(PXAdapter adapter)
		{
			// todo: should create new instance of extension before call
			if (AskExtConvert(out bool redirect))
			{
				if (Base.IsDirty)
					Base.Actions.PressSave();

				var processingGraph = Base.CloneGraphState();
				PXLongOperation.StartOperation(Base, () =>
				{
					var extension = processingGraph.GetProcessingExtension<CRCreateOpportunityAction<TGraph, TMain>>();

					bool? overrideContact = extension.Documents.Cache.GetValueOriginal<Document.overrideRefContact>(extension.Documents.Current) as bool?;

					var result = extension.Convert(new OpportunityConversionOptions
					{
						ForceOverrideContact = overrideContact
					});

					if(redirect)
						extension.Redirect(result);
				});
			}
			return adapter.Get();
		}

		public PXAction<TMain> ConvertToOpportunityRedirect;
		[PXUIField(DisplayName = Messages.CreateOpportunityRedirect, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXButton]
		public virtual IEnumerable convertToOpportunityRedirect(PXAdapter adapter)
		{
			var graph = CreateTargetGraph();
			var entity = CreateMaster(graph, null);

			Redirect(new ConversionResult<CROpportunity>()
				{
					Graph = graph,
					Entity = entity,
					Converted = false
				}
			);

			return adapter.Get();
		}

		internal override void AdjustFilterForContactBasedAPI(OpportunityFilter filter)
		{
			base.AdjustFilterForContactBasedAPI(filter);
			if (filter.Subject == null)
				filter.Subject = (Contacts.Current ?? Contacts.SelectSingle())?.FullName;
		}

		protected override CROpportunity CreateMaster(OpportunityMaint graph, OpportunityConversionOptions options)
		{
			var filter = OpportunityInfo.Current;
			var document = Documents.Current;
			var docContact = Contacts.Current ?? Contacts.SelectSingle();
			var docAddress = Addresses.Current ?? Addresses.SelectSingle();

			var opp = graph.Opportunity.Insert(new CROpportunity
			{
				Subject = filter.Subject,
				CloseDate = filter.CloseDate,
				ClassID = filter.OpportunityClass,
				LeadID = document.NoteID,
				Source = document.Source,
				CampaignSourceID = document.CampaignID,
				OverrideSalesTerritory = document.OverrideSalesTerritory,
			});

			if (opp.OverrideSalesTerritory is true)
			{
				opp.SalesTerritoryID = document.SalesTerritoryID;
			}

			opp.ContactID = document.RefContactID;
			opp.BAccountID = document.BAccountID;
			
			if (graph.OpportunityClass.SelectSingle()?.DefaultOwner == CRDefaultOwnerAttribute.Source)
			{
				opp.OwnerID = document.OwnerID;
				opp.WorkgroupID = document.WorkgroupID;
			}

			opp = graph.Opportunity.Update(opp);

			// should be after contact and baccount changes
			if (options?.ForceOverrideContact is bool foc && foc)
				graph.Opportunity.Cache.SetValueExt<CROpportunity.allowOverrideContactAddress>(opp, true);

			if (opp.AllowOverrideContactAddress == true)
			{
				var address = graph.Opportunity_Address.SelectSingle();
				MapAddress(docAddress, address);
				graph.Opportunity_Address.Update(address);

				var contact = graph.Opportunity_Contact.SelectSingle();
				MapContact(docContact, contact);
				MapConsentable(docContact, contact);
				graph.Opportunity_Contact.Update(contact);

				var shipAddress = graph.Shipping_Address.SelectSingle();
				MapAddress(docAddress, shipAddress);
				graph.Shipping_Address.Update(shipAddress);

				var shipContact = graph.Shipping_Contact.SelectSingle();
				MapContact(docContact, shipContact);
				MapConsentable(docContact, shipContact);
				graph.Shipping_Contact.Update(shipContact);
			}

			FillAttributes(graph.Answers, opp);

			FillUDF(OpportunityInfoUDF.Cache, Documents.Cache.GetMain(document), graph.Opportunity.Cache, opp, opp.ClassID);

			FillRelations(graph, opp);

			FillNotesAndAttachments(graph, Documents.Cache.GetMain(document), graph.Opportunity.Cache, opp);

			return opp;
		}

		protected override void ReverseDocumentUpdate(OpportunityMaint graph, CROpportunity entity)
		{
			var doc = Documents.Current;
			Documents.Cache.SetValue<Document.description>(doc, entity.Subject);
			Documents.Cache.SetValue<Document.qualificationDate>(doc, PXTimeZoneInfo.Now);
			Documents.Cache.SetValue<Document.convertedBy>(doc, PXAccess.GetUserID());
			graph.Caches<TMain>().Update(GetMain(doc));
		}

		#endregion
	}
}
