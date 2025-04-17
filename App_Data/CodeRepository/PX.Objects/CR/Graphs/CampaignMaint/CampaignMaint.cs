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
using System.Collections.Generic;
using System.Collections;
using System.Collections.Specialized;
using PX.Objects.CR.Standalone;
using PX.Objects.CS;
using System.Linq;
using PX.Common;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.Maintenance.GI;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.Common.Extensions;
using PX.Objects.SO;
using PX.Objects.GL;
using PX.Objects.PM;
using PX.Objects.CR.CRMarketingListMaint_Extensions;
using PX.Objects.CR.Extensions;
using Serilog;
using PX.SM;

namespace PX.Objects.CR
{
	public class CampaignMaint : PXGraph<CampaignMaint, CRCampaign>
	{
		#region Views

		[PXHidden]
		public PXSetup<CRSetup>
			crSetup;

		[PXHidden]
		public PXSelect<Contact>
			BaseContacts;

		[PXHidden]
		public PXSelect<APInvoice>
			APInvoicies;

		[PXHidden]
		public PXSelect<ARInvoice>
			ARInvoicies;

		public PXSelect<Contact, Where<Contact.contactID, Equal<Required<Contact.contactID>>>>
			ContactByContactId;

		[PXViewName(Messages.Campaign)]
		public PXSelect<CRCampaign>
			Campaign;

		[PXHidden]
		public PXSelect<CROpportunityClass>
			CROpportunityClass;

		[PXViewName(Messages.Opportunities)]
		public PXSelectJoin<CROpportunity,
			LeftJoin<CROpportunityProbability, On<CROpportunity.stageID, Equal<CROpportunityProbability.stageCode>>,
			LeftJoin<CROpportunityClass, On<CROpportunityClass.cROpportunityClassID, Equal<CROpportunity.classID>>>>,
			Where<CROpportunity.campaignSourceID, Equal<Current<CRCampaign.campaignID>>>>
			Opportunities;

		[PXHidden]
		[PXCopyPasteHiddenFields(typeof(CRCampaign.projectID), typeof(CRCampaign.projectTaskID))]
		public PXSelect<CRCampaign,
			Where<CRCampaign.campaignID, Equal<Current<CRCampaign.campaignID>>>>
			CampaignCurrent;

		[PXHidden]
		public PXSelect<DAC.Standalone.CRCampaign,
			Where<DAC.Standalone.CRCampaign.campaignID, Equal<Current<CRCampaign.campaignID>>>>
			CalcCampaignCurrent;

		[PXViewName(Messages.CampaignMembers)]
		[PXImportSubstitute(typeof(CRCampaign), typeof(CRMarketingMemberForImport))]
		[PXFilterable]
		[PXViewDetailsButton(typeof(Contact),
			typeof(SelectFrom<Contact>.Where<Contact.contactID.IsEqual<CRCampaignMembers.contactID.FromCurrent>>),
			WindowMode = PXRedirectHelper.WindowMode.New)]
		[PXViewDetailsButton(typeof(BAccount),
			typeof(SelectFrom<BAccount>
				.InnerJoin<Contact>
					.On<Contact.bAccountID.IsEqual<BAccount.bAccountID>>
				.Where<Contact.contactID.IsEqual<CRCampaignMembers.contactID.FromCurrent>>),
			WindowMode = PXRedirectHelper.WindowMode.New)]
		[PXViewDetailsButton(typeof(CRMarketingList),
			typeof(SelectFrom<CRMarketingList>.Where<CRMarketingList.marketingListID.IsEqual<CRCampaignMembers.marketingListID.FromCurrent>>),
			WindowMode = PXRedirectHelper.WindowMode.New)]
		[PXViewDetailsButton(typeof(CRContactClass),
			typeof(SelectFrom<CRContactClass>
				.InnerJoin<Contact>
					.On<Contact.classID.IsEqual<CRContactClass.classID>>
				.Where<Contact.contactID.IsEqual<CRCampaignMembers.contactID.FromCurrent>>),
			WindowMode = PXRedirectHelper.WindowMode.New)]
		public SelectFrom<CRCampaignMembers>
			.LeftJoin<Contact>
				.On<CRCampaignMembers.contactID.IsEqual<Contact.contactID>>
			.LeftJoin<Address>
				.On<Address.addressID.IsEqual<Contact.defAddressID>>
			.LeftJoin<BAccount>
				.On<BAccount.bAccountID.IsEqual<Contact.bAccountID>
				.And<BAccount.defContactID.IsEqual<Contact.contactID>>>
			.LeftJoin<Address2>
				.On<Address2.addressID.IsEqual<BAccount.defAddressID>
				.And<Address2.bAccountID.IsEqual<BAccount.bAccountID>>>
			.LeftJoin<CRMarketingList>
				.On<CRCampaignMembers.marketingListID.IsEqual<CRMarketingList.marketingListID>>
			.Where<
				Brackets<
					Contact.contactType.IsIn<
						ContactTypesAttribute.lead,
						ContactTypesAttribute.person,
						ContactTypesAttribute.bAccountProperty,
						ContactTypesAttribute.employee
					>
				>
				.And<CRCampaignMembers.campaignID.IsEqual<CRCampaign.campaignID.FromCurrent>>
			>
			.OrderBy<CRCampaignMembers.marketingListID.Asc>
			.View
			CampaignMembers;

		[PXHidden]
		public PXSelect<CRCampaignMembers> CampaignMembersHidden;

		[PXViewName(Messages.Answers)]
		public CRAttributeList<CRCampaign>
			Answers;


		[PXViewName(Messages.Leads)]
		[PXFilterable]
		[PXCopyPasteHiddenView]
		public PXSelectJoin<CRLead,
			LeftJoin<BAccount,
				On<BAccount.bAccountID, Equal<CRLead.bAccountID>>,
			LeftJoin<Address,
				On<Address.addressID, Equal<CRLead.defAddressID>>>>,
			Where<CRLead.campaignID, Equal<Current<CRCampaign.campaignID>>>,
			OrderBy<
				Asc<CRLead.displayName, Asc<CRLead.contactID>>>>
			Leads;

		[PXHidden]
		public SelectFrom<CRCampaignToCRMarketingListLink>
				.View CRCampaignToCRMarketingListLinkDummy;

		[PXViewName(Messages.CampaignMarketingLists)]
		[PXViewDetailsButton(typeof(CRMarketingList),
			ActionName = "CampaignMarketingLists_CRMarketingList_ViewDetails",
			WindowMode = PXRedirectHelper.WindowMode.New)]
		[PXFilterable]
		[PXCopyPasteHiddenView]
		public SelectFrom<CRMarketingListWithLinkToCRCampaign>
			.Where<
				CRCampaign.campaignID.FromCurrent.IsNotNull
				.And<Brackets<
					CRMarketingListWithLinkToCRCampaign.campaignID.IsEqual<CRCampaign.campaignID.FromCurrent>
					.Or<CRMarketingListWithLinkToCRCampaign.campaignID.IsNull>>
				>>
			.OrderBy<
				CRMarketingListWithLinkToCRCampaign.selectedForCampaign.Desc>
			.View CampaignMarketingLists;

		#endregion

		#region ctor

		public CampaignMaint()
		{
			PXDBAttributeAttribute.Activate(this.Caches[typeof(Contact)]);
			PXDBAttributeAttribute.Activate(Opportunities.Cache);
			PXUIFieldAttribute.SetEnabled<DAC.Standalone.CRCampaign.leadsGenerated>(CampaignCurrent.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<DAC.Standalone.CRCampaign.leadsConverted>(CampaignCurrent.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<DAC.Standalone.CRCampaign.contacts>(CampaignCurrent.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<DAC.Standalone.CRCampaign.opportunities>(CampaignCurrent.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<DAC.Standalone.CRCampaign.closedOpportunities>(CampaignCurrent.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<DAC.Standalone.CRCampaign.opportunitiesValue>(CampaignCurrent.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<DAC.Standalone.CRCampaign.closedOpportunitiesValue>(CampaignCurrent.Cache, null, false);

			PXUIFieldAttribute.SetRequired<CRCampaign.startDate>(CampaignCurrent.Cache, true);
			PXUIFieldAttribute.SetRequired<CRCampaign.status>(CampaignCurrent.Cache, true);


			var cache = Caches[typeof(Contact)];
			PXDBAttributeAttribute.Activate(cache);
			PXUIFieldAttribute.SetVisible<Contact.title>(cache, null, false);
			PXUIFieldAttribute.SetVisible<Contact.workgroupID>(cache, null, false);
			PXUIFieldAttribute.SetVisible<Contact.firstName>(cache, null, false);
			PXUIFieldAttribute.SetVisible<Contact.midName>(cache, null, false);
			PXUIFieldAttribute.SetVisible<Contact.lastName>(cache, null, false);
			PXUIFieldAttribute.SetVisible<Contact.phone2>(cache, null, false);
			PXUIFieldAttribute.SetVisible<Contact.phone3>(cache, null, false);
			PXUIFieldAttribute.SetVisible<Contact.fax>(cache, null, false);
			PXUIFieldAttribute.SetVisible<Contact.webSite>(cache, null, false);
			PXUIFieldAttribute.SetVisible<Contact.dateOfBirth>(cache, null, false);
			PXUIFieldAttribute.SetVisible<Contact.createdByID>(cache, null, false);
			PXUIFieldAttribute.SetVisible<Contact.createdDateTime>(cache, null, false);
			PXUIFieldAttribute.SetVisible<Contact.lastModifiedByID>(cache, null, false);
			PXUIFieldAttribute.SetVisible<Contact.lastModifiedDateTime>(cache, null, false);

			PXUIFieldAttribute.SetVisible<Address.addressLine1>(Caches[typeof(Address)], null, false);
			PXUIFieldAttribute.SetVisible<Address.addressLine2>(Caches[typeof(Address)], null, false);

			PXUIFieldAttribute.SetVisible<Contact.classID>(cache, null, false);
			PXUIFieldAttribute.SetVisible<Contact.source>(cache, null, false);
			PXUIFieldAttribute.SetVisible<Contact.status>(cache, null, false);

			PXUIFieldAttribute.SetVisibility<Contact.contactPriority>(cache, null, PXUIVisibility.Invisible);

			PXUIFieldAttribute.SetDisplayName<BAccount.acctName>(Caches[typeof(BAccount)], Messages.CustomerName);
			cache = Caches[typeof(CRLead)];
			PXUIFieldAttribute.SetVisible<CRLead.title>(cache, null, false);
			PXUIFieldAttribute.SetVisible<CRLead.firstName>(cache, null, false);
			PXUIFieldAttribute.SetVisible<CRLead.midName>(cache, null, false);
			PXUIFieldAttribute.SetVisible<CRLead.lastName>(cache, null, false);
			PXUIFieldAttribute.SetVisible<CRLead.phone1>(cache, null, false);
			PXUIFieldAttribute.SetVisible<CRLead.phone2>(cache, null, false);
			PXUIFieldAttribute.SetVisible<CRLead.phone3>(cache, null, false);
			PXUIFieldAttribute.SetVisible<CRLead.fax>(cache, null, false);
			PXUIFieldAttribute.SetVisible<CRLead.eMail>(cache, null, false);
			PXUIFieldAttribute.SetVisible<CRLead.webSite>(cache, null, false);
			PXUIFieldAttribute.SetVisible<CRLead.dateOfBirth>(cache, null, false);
			PXUIFieldAttribute.SetVisible<CRLead.createdByID>(cache, null, false);
			PXUIFieldAttribute.SetVisible<CRLead.createdDateTime>(cache, null, false);
			PXUIFieldAttribute.SetVisible<CRLead.lastModifiedByID>(cache, null, false);
			PXUIFieldAttribute.SetVisible<CRLead.lastModifiedDateTime>(cache, null, false);
			PXUIFieldAttribute.SetEnabled(this.Caches[typeof(Contact)], null, null, false);
			PXUIFieldAttribute.SetEnabled<Contact.selected>(this.Caches[typeof(Contact)], null, true);

			CampaignMarketingLists.AllowInsert = CampaignMarketingLists.AllowInsert = false;

			cache = Caches[typeof(CRMarketingListWithLinkToCRCampaign)];
			PXDBAttributeAttribute.Activate(cache);
			PXUIFieldAttribute.SetEnabled<CRMarketingListWithLinkToCRCampaign.campaignID>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<CRMarketingListWithLinkToCRCampaign.marketingListID>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<CRMarketingListWithLinkToCRCampaign.mailListCode>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<CRMarketingListWithLinkToCRCampaign.name>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<CRMarketingListWithLinkToCRCampaign.description>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<CRMarketingListWithLinkToCRCampaign.status>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<CRMarketingListWithLinkToCRCampaign.workgroupID>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<CRMarketingListWithLinkToCRCampaign.ownerID>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<CRMarketingListWithLinkToCRCampaign.method>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<CRMarketingListWithLinkToCRCampaign.type>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<CRMarketingListWithLinkToCRCampaign.gIDesignID>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<CRMarketingListWithLinkToCRCampaign.sharedGIFilter>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<CRMarketingListWithLinkToCRCampaign.noteID>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<CRMarketingListWithLinkToCRCampaign.createdByScreenID>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<CRMarketingListWithLinkToCRCampaign.createdByID>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<CRMarketingListWithLinkToCRCampaign.createdDateTime>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<CRMarketingListWithLinkToCRCampaign.lastModifiedByID>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<CRMarketingListWithLinkToCRCampaign.lastModifiedByScreenID>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<CRMarketingListWithLinkToCRCampaign.lastModifiedDateTime>(cache, null, false);

			PXUIFieldAttribute.SetDisplayName<Address2.addressLine1>(this.Caches<Address2>(), "Business Account Address Line 1");
			PXUIFieldAttribute.SetDisplayName<Address2.addressLine2>(this.Caches<Address2>(), "Business Account Address Line 2");
			PXUIFieldAttribute.SetDisplayName<Address2.city>(this.Caches<Address2>(), "Business Account City");
			PXUIFieldAttribute.SetDisplayName<Address2.state>(this.Caches<Address2>(), "Business Account State");
			PXUIFieldAttribute.SetDisplayName<Address2.postalCode>(this.Caches<Address2>(), "Business Account Postal Code");
			PXUIFieldAttribute.SetDisplayName<Address2.countryID>(this.Caches<Address2>(), "Business Account Country");
		}

		[InjectDependency]
		public ICRMarketingListMemberRepository MemberRepository { get; private set; }

		public override void Clear(PXClearOption option)
		{
			if (this.Caches.ContainsKey(typeof(CRCampaignMembers)))
			{
				this.Caches[typeof(CRCampaignMembers)].ClearQueryCache();
			}

			base.Clear(option);
		}

		private ILogger _logger;
		[InjectDependency]
		public ILogger Logger
		{
			get => _logger;
			set => _logger = value?.ForContext<CampaignMaint>();
		}

		#endregion

		#region Actions

		public PXAction<CRCampaign> UpdateListMembers;
		[PXUIField(DisplayName = "Update List", MapEnableRights = PXCacheRights.Update)]
		[PXButton(Tooltip = MessagesNoPrefix.CampaignUpdateListMembersButtonTooltip)]
		public virtual IEnumerable updateListMembers(PXAdapter adapter)
		{
			bool isExistsNonMarketingListMembers = false;
			
			using (PXDataRecord exist = ProviderSelectSingle<CRCampaignMembers>(
					new PXDataField<CRCampaignMembers.campaignID>(),
					new PXDataFieldValue<CRCampaignMembers.campaignID>(CampaignCurrent.Current.CampaignID),
					new PXDataFieldValue<CRCampaignMembers.marketingListID>(null, PXComp.ISNOTNULL)))
			{
				isExistsNonMarketingListMembers = exist != null;
			}

			if (isExistsNonMarketingListMembers == false
				||
				CampaignMembers.View
					.Ask(Confirmations.CampaignMemberExistsInCampaign, MessageButtons.OKCancel) == WebDialogResult.OK)
			{
				CampaignMembers.View.Answer = WebDialogResult.No;
				FixLastUpdateDate();
				Actions.PressSave();
				CampaignUpdateListMembersLongOperation();
				Actions.PressCancel();
			}
			return adapter.Get();
		}

		public virtual void CampaignUpdateListMembersLongOperation()
		{
			var selectedItems = CampaignMarketingLists
									.SelectMain()
										.Where(i => i.SelectedForCampaign == true)
									.RowCast<CRMarketingList>()
									.ToList();

			var graph = this.CloneGraphState();
			PXLongOperation.StartOperation(this, () =>
			{
				using (var scope = new PXTransactionScope())
				{
					graph.CampaignUpdateListMembers(graph.CampaignCurrent.Current, selectedItems);
					scope.Complete();
				}
			});
		}

		public virtual void CampaignUpdateListMembers(CRCampaign cRCampaign, IEnumerable<CRMarketingList> maketingLists)
		{
			var userId = Accessinfo.UserID;
			var screenId = Accessinfo.GetNormalizedScreenID();

			ProviderDelete<CRCampaignMembers>(
				new PXDataFieldRestrict<CRCampaignMembers.campaignID>(
					cRCampaign.CampaignID),
				new PXDataFieldRestrict<CRCampaignMembers.marketingListID>(PXDbType.Int, 4, null, PXComp.ISNOTNULL));

			var hashset = new HashSet<int?>();
			foreach (CRMarketingListMember member in MemberRepository.GetMembers(maketingLists))
			{
				if (hashset.Add(member.ContactID) is false)
					continue;

				var datetime = PXTimeZoneInfo.UtcNow;

				try
				{
					ProviderInsert<CRCampaignMembers>(
						new PXDataFieldAssign<CRCampaignMembers.campaignID>(cRCampaign.CampaignID),
						new PXDataFieldAssign<CRCampaignMembers.contactID>(member.ContactID),
						new PXDataFieldAssign<CRCampaignMembers.marketingListID>(member.MarketingListID),
						new PXDataFieldAssign<CRCampaignMembers.createdByID>(userId),
						new PXDataFieldAssign<CRCampaignMembers.lastModifiedByID>(userId),
						new PXDataFieldAssign<CRCampaignMembers.createdByScreenID>(screenId),
						new PXDataFieldAssign<CRCampaignMembers.lastModifiedByScreenID>(screenId),
						new PXDataFieldAssign<CRCampaignMembers.createdDateTime>(datetime),
						new PXDataFieldAssign<CRCampaignMembers.lastModifiedDateTime>(datetime));
				}
				catch (PXDatabaseException ex) when (ex.ErrorCode is PXDbExceptions.PrimaryKeyConstraintViolation)
				{
					Logger.Verbose(ex,
						"Campaign member {ContactID} for Campaign {CampaignID} already exists",
						member.ContactID, cRCampaign.CampaignID);
				}
			}
		}

		public PXAction<CRCampaign> ClearMembers;

		[PXUIField(DisplayName = "Clear All", MapEnableRights = PXCacheRights.Update)]
		[PXButton(Tooltip = MessagesNoPrefix.CampaignClearMembersButtonTooltip)]
		protected virtual IEnumerable clearMembers(PXAdapter adapter)
		{
			if (CampaignCurrent?.Current?.CampaignID != null)
			{
				if (this.CampaignCurrent.View.Ask(
							null,
							Confirmations.CampaignClearMembers,
							MessageButtons.OKCancel) == WebDialogResult.OK)
				{
					ClearCampaignMembersLongOperation();
				}
			}
			return adapter.Get();
		}

		public virtual void ClearCampaignMembersLongOperation()
		{
			var graph = this.CloneGraphState();
			var externalFilter = CampaignMembers.View.GetExternalFilters();

			PXLongOperation.StartOperation(this, () =>
			{
				using (var scope = new PXTransactionScope())
				{
					int startRow = 0;
					int totalRows = 0;
					graph.CampaignMembers.View
							.Select(
								new object[] { graph.CampaignCurrent.Current },
								null, null, null, null,
								externalFilter,
								ref startRow,
								0,
								ref totalRows)
							.RowCast<CRCampaignMembers>()
							.ForEach(x => graph.CampaignMembers.Delete(x));
					graph.Actions.PressSave();
					scope.Complete();
				}
			});
		}

		public PXDBAction<CRCampaign> addOpportunity;
		[PXUIField(DisplayName = Messages.AddNewOpportunity, FieldClass = FeaturesSet.customerModule.FieldClass)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.AddNew, OnClosingPopup = PXSpecialButtonType.Refresh)]
		public virtual void AddOpportunity()
		{
			var row = CampaignCurrent.Current;
			if (row == null || row.CampaignID == null) return;

			var graph = PXGraph.CreateInstance<OpportunityMaint>();

			var newOpportunity = graph.Opportunity.Insert();

			newOpportunity.CampaignSourceID = row.CampaignID;

			if (row.ProjectID != null)
				newOpportunity.ProjectID = row.ProjectID;

			CROpportunityClass ocls = PXSelect<CROpportunityClass, Where<CROpportunityClass.cROpportunityClassID, Equal<Current<CROpportunity.classID>>>>
				.SelectSingleBound(this, new object[] { newOpportunity });
			if (ocls?.DefaultOwner == CRDefaultOwnerAttribute.Source)
			{
				newOpportunity.WorkgroupID = row.WorkgroupID;
				newOpportunity.OwnerID = row.OwnerID;
			}

			graph.Opportunity.Update(newOpportunity);
			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
		}

		public PXDBAction<CRCampaign> addContact;
		[PXUIField(DisplayName = Messages.AddNewLead, FieldClass = FeaturesSet.customerModule.FieldClass)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.AddNew, OnClosingPopup = PXSpecialButtonType.Refresh)]
		public virtual void AddContact()
		{
			var row = CampaignCurrent.Current;
			if (row?.CampaignID == null) return;

			var graph = PXGraph.CreateInstance<LeadMaint>();

			var lead = graph.Lead.Insert();

			lead.CampaignID = row.CampaignID;

			CRLeadClass ocls = PXSelect<CRLeadClass, Where<CRLeadClass.classID, Equal<Current<Contact.classID>>>>
				.SelectSingleBound(this, new object[] { lead });
			if (ocls?.DefaultOwner == CRDefaultOwnerAttribute.Source)
			{
				lead.WorkgroupID = row.WorkgroupID;
				lead.OwnerID = row.OwnerID;
			}

			lead = graph.Lead.Update(lead);

			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
		}

		public PXAction<CROpportunity> addNewProjectTask;
		[PXUIField(Visible = false)]
		[PXButton()]
		public virtual IEnumerable AddNewProjectTask(PXAdapter adapter)
		{
			var campaign = CampaignCurrent.Current;
			if (campaign != null && campaign.ProjectID.HasValue)
			{
				var graph = PXGraph.CreateInstance<ProjectTaskEntry>();
				graph.Clear();
				var task = new PMTask();
				graph.Task.Cache.SetValue<PMTask.projectID>(task, campaign.ProjectID);

				object taskID = campaign.CampaignID;
				graph.Task.Cache.RaiseFieldUpdating<PMTask.taskCD>(task, ref taskID);
				graph.Task.Cache.SetValue<PMTask.taskCD>(task, taskID);


				task = (PMTask)graph.Task.Cache.CreateCopy(graph.Task.Insert(task));

				graph.Task.Cache.SetValue<PMTask.description>(task, campaign.CampaignName);
				graph.Task.Cache.SetValue<PMTask.plannedStartDate>(task, campaign.StartDate);
				graph.Task.Cache.SetValue<PMTask.startDate>(task, campaign.StartDate);
				graph.Task.Cache.SetValue<PMTask.plannedEndDate>(task, campaign.EndDate);
				graph.Task.Cache.SetValue<PMTask.endDate>(task, campaign.EndDate);
				graph.Task.Update(task);

				PXRedirectHelper.TryRedirect(graph, task, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		public PXAction<CRMarketingList> deleteAction;
		[PXButton]
		[PXUIField(DisplayName = Messages.DeleteSelected)]
		protected IEnumerable DeleteAction(PXAdapter adapter)
		{
			var row = Campaign.Current as CRCampaign;
			if (row == null) return adapter.Get();

			if (row == null || row.CampaignID == null || Campaign.Cache.GetStatus(row) == PXEntryStatus.Inserted)
				return adapter.Get();

			List<CRCampaignMembers> membersToDelete = new List<CRCampaignMembers>();

			var cacheMember = Caches[typeof(CRCampaignMembers)];
			foreach (CRCampaignMembers member in ((IEnumerable<CRCampaignMembers>)cacheMember.Updated)
					 .Concat<CRCampaignMembers>((IEnumerable<CRCampaignMembers>)cacheMember.Inserted))
			{
				if (member.Selected == true) membersToDelete.Add(member);
			}

			if (!membersToDelete.Any() && cacheMember.Current != null)
				membersToDelete.Add((CRCampaignMembers)cacheMember.Current);

			foreach (CRCampaignMembers member in membersToDelete)
				cacheMember.Delete(member);

			return adapter.Get();
		}

		#endregion

		#region Events

		#region CacheAttached

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDBDefault(typeof(CRCampaign.campaignID))]
		[PXUIField(DisplayName = Messages.CampaignID)]
		[PXParent(typeof(Select<CRCampaign, Where<CRCampaign.campaignID, Equal<Current<CRCampaignMembers.campaignID>>>>))]
		protected virtual void _(Events.CacheAttached<CRCampaignMembers.campaignID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Member Since", Enabled = false)]
		protected virtual void _(Events.CacheAttached<CRCampaignMembers.createdDateTime> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Contact", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		protected virtual void _(Events.CacheAttached<Contact.displayName> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault("")]
		protected virtual void _(Events.CacheAttached<Contact.contactType> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Account ID", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXSelector(typeof(BAccount.bAccountID), SubstituteKey = typeof(BAccount.acctCD), DescriptionField = typeof(BAccount.acctCD), DirtyRead = true)]
		protected virtual void _(Events.CacheAttached<Contact.bAccountID> e) { }

		[PXUIField(DisplayName = "Class Description")]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CROpportunityClass.description> e) { }

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Business Account Workgroup")]
		protected virtual void _(Events.CacheAttached<BAccount.workgroupID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Business Account Owner")]
		protected virtual void _(Events.CacheAttached<BAccount.ownerID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Business Account Parent Account")]
		protected virtual void _(Events.CacheAttached<BAccount.parentBAccountID> e) { }

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Business Account Source Campaign")]
		protected virtual void _(Events.CacheAttached<BAccount.campaignSourceID> e) { }

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Business Account Class ID")]
		protected virtual void _(Events.CacheAttached<BAccount.classID> e) { }

		#endregion

		protected virtual void _(Events.RowDeleting<CRCampaign> e)
		{
			CRCampaign row = e.Row as CRCampaign;
			DAC.Standalone.CRCampaign dacRow = CalcCampaignCurrent.Select();

			if (row == null || dacRow == null) return;

			if (CanBeDeleted(row, dacRow) == false)
			{
				e.Cancel = true;

				throw new PXException(Messages.CampaignIsReferenced);
			}
		}

		protected virtual void _(Events.FieldDefaulting<CRCampaign, CRCampaign.status> e)
		{
			CRCampaign campaign = (CRCampaign)e.Row;
			var state = (PXStringState)this.Caches<CRCampaign>().GetStateExt<CRCampaign.status>(campaign);
			e.NewValue = state.AllowedValues.FirstOrDefault();
		}

		protected virtual bool CanBeDeleted(CRCampaign campaign, DAC.Standalone.CRCampaign dacCampaign)
		{
			foreach (var f in new string[]
			{
				nameof(CRCampaign.mailsSent)
			})
			{
				var state = CampaignCurrent.Cache.GetStateExt(campaign, f);
				if (((PXIntState)state).Value != null && (int)((PXIntState)state).Value > 0)
					return false;
			}

			foreach (var f in new string[]
			{
				nameof(DAC.Standalone.CRCampaign.closedOpportunities),
				nameof(DAC.Standalone.CRCampaign.contacts),
				nameof(DAC.Standalone.CRCampaign.leadsConverted),
				nameof(DAC.Standalone.CRCampaign.leadsGenerated),
				nameof(DAC.Standalone.CRCampaign.opportunities),
			})
			{
				var state = CalcCampaignCurrent.Cache.GetStateExt(dacCampaign, f);
				if (((PXIntState)state).Value != null && (int)((PXIntState)state).Value > 0)
					return false;
			}

			if (PXSelectGroupBy<PMTask,
				Where<PMTask.projectID, Equal<Current<CRCampaign.projectID>>,
				And<PMTask.taskID, Equal<Current<CRCampaign.projectTaskID>>>>,
				Aggregate<Count>>.Select(this).RowCount > 0)
				return false;

			return true;
		}

		protected virtual void _(Events.RowSelected<CRCampaign> e)
		{
			CRCampaign row = e.Row as CRCampaign;
			if (row == null) return;

			var isNotInserted = e.Cache.GetStatus(row) != PXEntryStatus.Inserted;
			addOpportunity.SetEnabled(isNotInserted);

			PXUIFieldAttribute.SetEnabled<CRCampaign.projectTaskID>(CampaignCurrent.Cache, row, row.ProjectID.HasValue);
			PXUIFieldAttribute.SetRequired<CRCampaign.projectTaskID>(CampaignCurrent.Cache, row.ProjectID.HasValue);

			PXImportAttribute.SetEnabled(this, nameof(CampaignMembers), e.Cache.GetOriginal(e.Row) != null);
		}

		protected virtual void _(Events.FieldUpdated<CRCampaign, CRCampaign.projectID> e)
		{
			CRCampaign row = e.Row as CRCampaign;
			if (row == null) return;
			this.CampaignCurrent.Cache.SetValue<CRCampaign.projectTaskID>(row, null);
		}

		protected virtual void _(Events.RowPersisting<CRCampaign> e)
		{
			CRCampaign row = (CRCampaign)e.Row;
			if (row != null)
			{
				if (row.StartDate.HasValue == false)
				{
					if (e.Cache.RaiseExceptionHandling<CRCampaign.startDate>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(CRCampaign.startDate)}]")))
					{
						throw new PXRowPersistingException(typeof(CRCampaign.startDate).Name, null, ErrorMessages.FieldIsEmpty, nameof(CRCampaign.startDate));
					}
				}

				if (row.ProjectID.HasValue && !row.ProjectTaskID.HasValue)
				{
					if (e.Cache.RaiseExceptionHandling<CRCampaign.projectTaskID>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(CRCampaign.projectTaskID)}]")))
					{
						throw new PXRowPersistingException(typeof(CRCampaign.projectTaskID).Name, null, ErrorMessages.FieldIsEmpty, nameof(CRCampaign.projectTaskID));
					}
				}

				if (row.ProjectTaskID.HasValue)
				{
					if (PXSelectGroupBy<CRCampaign,
						Where<CRCampaign.projectID, Equal<Required<CRCampaign.projectID>>,
							And<CRCampaign.projectTaskID, Equal<Required<CRCampaign.projectTaskID>>,
							And<CRCampaign.campaignID, NotEqual<Required<CRCampaign.campaignID>>>>>,
						Aggregate<Count>>.Select(this, new object[] { row.ProjectID, row.ProjectTaskID, row.CampaignID }).RowCount > 0)
					{
						throw new PXRowPersistingException(typeof(CRCampaign.projectTaskID).Name, row.ProjectTaskID, Messages.TaskIsAlreadyLinkedToCampaign, typeof(CRCampaign.projectTaskID).Name);
					}
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<CRMarketingListWithLinkToCRCampaign, CRMarketingListWithLinkToCRCampaign.selectedForCampaign> e)
		{
			CRMarketingListWithLinkToCRCampaign row = e.Row as CRMarketingListWithLinkToCRCampaign;

			if (row == null) return;

			CRCampaignToCRMarketingListLink linkRow = SelectFrom<
					CRCampaignToCRMarketingListLink>
				.Where<
					CRCampaignToCRMarketingListLink.campaignID.IsEqual<CRCampaign.campaignID.FromCurrent>
					.And<CRCampaignToCRMarketingListLink.marketingListID.IsEqual<@P.AsInt>>>
				.View
				.Select(this, row.MarketingListID)
				.FirstOrDefault()
				?? this.Caches<CRCampaignToCRMarketingListLink>().InitNewRow();

			linkRow.MarketingListID = row.MarketingListID;
			linkRow.SelectedForCampaign = e.NewValue as bool?;

			this.Caches<CRCampaignToCRMarketingListLink>().Update(linkRow);
		}

		public override void Persist()
		{
			if (this.Caches<CRCampaignToCRMarketingListLink>().IsInsertedUpdatedDeleted)
			{
				if (this.CampaignMembers.View.Ask(
							this.CampaignMembers.Current,
							Confirmations.Confirmation,
							Confirmations.CampaignMarketingListsUpdated,
							MessageButtons.YesNoCancel,
							new Dictionary<WebDialogResult, string>
							{
								[WebDialogResult.Yes] = "Update",
								[WebDialogResult.No] = "Keep",
								[WebDialogResult.Cancel] = "Cancel",
							}, MessageIcon.None).IsIn(WebDialogResult.Yes, WebDialogResult.No)
					)
				{
					bool updateMembers = this.CampaignMembers.View.Answer == WebDialogResult.Yes;

					if (updateMembers)
					{
						FixLastUpdateDate();
						CampaignUpdateListMembersLongOperation();
					}
				}
				else
				{
					this.CampaignMembers.View.Answer = WebDialogResult.None;
					return;
				}
			}
			base.Persist();
		}

		protected virtual void FixLastUpdateDate()
		{
			var now = PXTimeZoneInfo.Now;
			var cache = this.Caches<CRCampaignToCRMarketingListLink>();

			var linkRows = SelectFrom<
					CRCampaignToCRMarketingListLink>
				.Where<
					CRCampaignToCRMarketingListLink.campaignID.IsEqual<CRCampaign.campaignID.FromCurrent>>
				.View
				.Select(this);

			foreach (var row in linkRows)
			{
				var linkRow = row.GetItem<CRCampaignToCRMarketingListLink>();

				if (linkRow?.SelectedForCampaign is true)
				{
					linkRow.LastUpdateDate = now;
					cache.Update(linkRow);
				}
				else if (linkRow?.LastUpdateDate != null)
				{
					cache.Delete(linkRow);
				}
			}
		}

		#endregion
	}
}
