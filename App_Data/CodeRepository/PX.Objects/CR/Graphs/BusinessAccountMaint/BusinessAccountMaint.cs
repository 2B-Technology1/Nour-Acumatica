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

using Autofac;
using System;
using System.Collections;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using PX.Data.EP;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.SO;
using PX.SM;
using PX.Objects.CR.DAC;
using PX.Objects.CA;
using PX.Objects.CR.MassProcess;
using PX.Data.MassProcess;
using PX.Objects.CR.Extensions;
using PX.Objects.CR.Extensions.Cache;
using PX.Objects.CR.Extensions.CRDuplicateEntities;
using PX.Objects.CR.Extensions.CRCreateActions;
using System.Linq;
using System.Collections.Generic;
using PX.Objects.CR.Extensions.Relational;
using PX.CS.Contracts.Interfaces;
using PX.Objects.GDPR;
using PX.Objects.GraphExtensions.ExtendBAccount;
using PX.Objects.AR;
using System.Web.Compilation;
using PX.Data.DependencyInjection;
using PX.Data.WorkflowAPI;
using PX.Objects.CR.BusinessAccountMaint_Extensions;
using PX.Objects.CR.Workflows;
using CRLocation = PX.Objects.CR.Standalone.Location;

namespace PX.Objects.CR
{
	using static PX.Data.WorkflowAPI.BoundedTo<BusinessAccountMaint, BAccount>;

	public partial class BusinessAccountMaint : PXGraph<BusinessAccountMaint, BAccount>
	{
		#region Selects

		[PXHidden]
		public PXSelect<BAccount>
			BaseBAccounts;

        [PXHidden]
        public PXSelect<BAccountCRM>
            BaseBAccountsCRM;

		[PXHidden]
		public PXSetup<GL.Branch>
			Branches;

		[PXHidden]
		[PXCheckCurrent]
		public CM.CMSetupSelect 
			CMSetup;

		[PXHidden]
		[PXCheckCurrent]
		public PXSetup<GL.Company>
			cmpany;

		[PXHidden]
		[PXCheckCurrent]
		public PXSetup<CRSetup> 
			Setup;

		[PXHidden]
		public PXSelect<CRLocation>
			BaseLocations;

		[PXViewName(Messages.BAccount)]
		public PXSelectJoin<BAccount, 
			LeftJoin<Contact, 
				On<Contact.contactID, Equal<BAccount.defContactID>>>,
			Where2<Match<Current<AccessInfo.userName>>, 
			And<Where<BAccount.type, Equal<BAccountType.customerType>,
				Or<BAccount.type, Equal<BAccountType.prospectType>,
				Or<BAccount.type, Equal<BAccountType.combinedType>,
				Or<BAccount.type, Equal<BAccountType.vendorType>>>>>>>>
			BAccount;

		[PXHidden]
		[PXCopyPasteHiddenFields(typeof(BAccount.primaryContactID))]
		public SelectFrom<BAccount>
			.LeftJoin<Contact>
				.On<Contact.contactID.IsEqual<BAccount.defContactID>>
			.Where<BAccount.bAccountID.IsEqual<BAccount.bAccountID.FromCurrent>>
			.View
			CurrentBAccount;

		[PXCopyPasteHiddenView]
		public PXSelect<CRActivityStatistics,
				Where<CRActivityStatistics.noteID, Equal<Current<BAccount.noteID>>>>
			BAccountActivityStatistics;

		[PXHidden]
		public PXSelect<Address>
			AddressDummy;

		[PXHidden]
		public PXSelect<Contact>
			ContactDummy;

		[PXViewName(Messages.Leads)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(Contact))]
		public PXSelectJoin<
				CRLead,
			InnerJoin<Address,
				On<Address.addressID, Equal<Contact.defAddressID>>,
			LeftJoin<CRActivityStatistics,
				On<CRActivityStatistics.noteID, Equal<CRLead.noteID>>>>,
			Where<
				CRLead.bAccountID, Equal<Current<BAccount.bAccountID>>>,
			OrderBy<
				Desc<CRLead.createdDateTime>>>
			Leads;

		[PXViewName(Messages.Answers)]
		public CRAttributeList<BAccount>
			Answers;

		[PXHidden]
		public PXSelect<CROpportunityClass>
			CROpportunityClass;

		[PXViewName(Messages.Opportunities)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(BAccount))]
		[PXViewDetailsButton(typeof(BAccount),
			typeof(Select<BAccount,
				Where<BAccount.bAccountID, Equal<Current<CROpportunity.bAccountID>>>>))]
		[PXViewDetailsButton(typeof(BAccount),
			typeof(Select<Contact,
				Where<Contact.contactID, Equal<Current<CROpportunity.contactID>>>>))]
		public PXSelectJoin<CROpportunity,			
			LeftJoin<Contact, On<Contact.contactID, Equal<CROpportunity.contactID>>, 
			LeftJoin<CROpportunityProbability, On<CROpportunityProbability.stageCode, Equal<CROpportunity.stageID>>,
			LeftJoin<BAccount, On<BAccount.bAccountID, Equal<CROpportunity.bAccountID>>,
			LeftJoin<CROpportunityClass, On<CROpportunityClass.cROpportunityClassID, Equal<CROpportunity.classID>>,
			LeftJoin<CRLead, On<CRLead.noteID, Equal<CROpportunity.leadID>>>>>>>,
			Where<BAccount.bAccountID, Equal<Current<BAccount.bAccountID>>,
					Or<BAccount.parentBAccountID, Equal<Current<BAccount.bAccountID>>>>> 
			Opportunities;

		[PXHidden]
		public PXSelect<CROpportunity> OpportunityLink;

		[PXHidden]
		public PXSelect<CRQuote> SalesQuoteLink;

		[PXViewName(Messages.Cases)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(BAccount))]
		[PXViewDetailsButton(typeof(BAccount),
			typeof(Select<Contact,
				Where<Contact.contactID, Equal<Current<CRCase.contactID>>>>))]
		public PXSelectReadonly2<CRCase,
			LeftJoin<Contact, On<Contact.contactID, Equal<CRCase.contactID>>>, 
			Where<CRCase.customerID, Equal<Current<BAccount.bAccountID>>>> 
			Cases;

		[PXViewName(Messages.Contracts)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(BAccount))]
		[PXViewDetailsButton(typeof(BAccount),
			typeof(Select<Location,
				Where<Location.bAccountID, Equal<Current<Contract.customerID>>, And<Location.locationID, Equal<Current<Contract.locationID>>>>>))]
		[PXViewDetailsButton(typeof(BAccount),
			typeof(Select<BAccount,
				Where<BAccount.bAccountID, Equal<Current<Contract.customerID>>>>))]
		public PXSelectReadonly2<Contract,
			LeftJoin<ContractBillingSchedule, On<ContractBillingSchedule.contractID, Equal<Contract.contractID>>, 
			LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Contract.customerID>>>>,
			Where<Contract.baseType, Equal<CTPRType.contract>, 
			  And<Where<BAccount.bAccountID, Equal<Current<BAccount.bAccountID>>,
							 Or<ContractBillingSchedule.accountID, Equal<Current<BAccount.bAccountID>>>>>>>
			Contracts;

		[PXViewName(Messages.Orders)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(BAccount))]
		public PXSelectReadonly<SOOrder,
			Where<SOOrder.customerID, Equal<Current<BAccount.bAccountID>>>>
			Orders;

        [PXCopyPasteHiddenView]
		[PXViewName(Messages.CampaignMember)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(BAccount),
			typeof(Select<CRCampaign,
				Where<CRCampaign.campaignID, Equal<Current<CRCampaignMembers.campaignID>>>>))]
		[PXViewDetailsButton(typeof(CRCampaign),
			typeof(Select<CRCampaign,
				Where<CRCampaign.campaignID, Equal<Current<CRCampaignMembers.campaignID>>>>),
			WindowMode = PXRedirectHelper.WindowMode.New)]
		public SelectFrom<CRCampaignMembers>
					.InnerJoin<CRCampaign>
						.On<CRCampaign.campaignID.IsEqual<CRCampaignMembers.campaignID>>
					.InnerJoin<Contact>
						.On<Contact.contactID.IsEqual<CRCampaignMembers.contactID>>
					.LeftJoin<CRMarketingList>
						.On<CRMarketingList.marketingListID.IsEqual<CRCampaignMembers.marketingListID>>
					.Where<Contact.bAccountID.IsEqual<BAccount.bAccountID.FromCurrent>>
				.View Members;

		[PXHidden]
		public PXSelect<CRMarketingListMember>
			Subscriptions_stub;

        public CRNotificationSourceList<BAccount, BAccount.classID, CRNotificationSource.bAccount> NotificationSources;

        public CRNotificationRecipientList<BAccount, BAccount.classID> NotificationRecipients;

        #endregion

        #region Ctors
        public BusinessAccountMaint()
		{
			if (Branches.Current.BAccountID.HasValue == false) //TODO: need review
                throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(GL.Branch), PXMessages.LocalizeNoPrefix(CS.Messages.BranchMaint));

			PXUIFieldAttribute.SetDisplayName<BAccount.acctCD>(BAccount.Cache, Messages.BAccountCD);
			PXUIFieldAttribute.SetDisplayName<Carrier.description>(Caches[typeof(Carrier)], Messages.CarrierDescription);

			PXUIFieldAttribute.SetRequired<BAccount.status>(BAccount.Cache, true);
			PXUIFieldAttribute.SetRequired<Contact.lastName>(Caches[typeof(Contact)], true);
		}

		#endregion

		#region Actions

		public PXMenuAction<BAccount> Action;

		public PXDBAction<BAccount> addOpportunity;
		[PXUIField(DisplayName = Messages.CreateNewOpportunity)]
		[PXButton]
		public virtual void AddOpportunity()
		{
			var row = CurrentBAccount.Current;
			if (row == null || row.BAccountID == null) return;

			var graph = PXGraph.CreateInstance<OpportunityMaint>();
            var newOpportunity = graph.Opportunity.Insert();
			newOpportunity.BAccountID = row.BAccountID;
			newOpportunity.LocationID = row.DefLocationID;

			newOpportunity.OverrideSalesTerritory = row.OverrideSalesTerritory;
			if (newOpportunity.OverrideSalesTerritory is true)
			{
				newOpportunity.SalesTerritoryID = row.SalesTerritoryID;
			}

			CROpportunityClass ocls = PXSelect<CROpportunityClass, Where<CROpportunityClass.cROpportunityClassID, Equal<Current<CROpportunity.classID>>>>
				.SelectSingleBound(this, new object[] { newOpportunity });
			if (ocls?.DefaultOwner == CRDefaultOwnerAttribute.Source)
			{
				newOpportunity.WorkgroupID = row.WorkgroupID;
				newOpportunity.OwnerID = row.OwnerID;
			}
			UDFHelper.CopyAttributes(CurrentBAccount.Cache, row, graph.Opportunity.Cache, newOpportunity, newOpportunity.ClassID);

			//TODO: need calculate default contact
			newOpportunity = graph.Opportunity.Update(newOpportunity);
            graph.Opportunity.SetValueExt<CROpportunity.bAccountID>(newOpportunity, row.BAccountID);
            graph.Answers.CopyAllAttributes(newOpportunity, row);

			if (!this.IsContractBasedAPI)
				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);

			graph.Save.Press();
		}

		public PXDBAction<BAccount> addCase;
		[PXUIField(DisplayName = Messages.AddNewCase)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.AddNew)]
		public virtual void AddCase()
		{
			var row = CurrentBAccount.Current;
			if (row == null || row.BAccountID == null) return;

            var graph = PXGraph.CreateInstance<CRCaseMaint>();

            var newCase = (CRCase)graph.Case.Cache.Insert();
            newCase.CustomerID = row.BAccountID;
            newCase.LocationID = row.DefLocationID;
			UDFHelper.CopyAttributes(CurrentBAccount.Cache, row, graph.Case.Cache, graph.Case.Cache.Current, newCase.CaseClassID);

			//TODO: need calculate default contact
			newCase = graph.Case.Update(newCase);
            graph.Answers.CopyAllAttributes(newCase, row);
			
			if (!this.IsContractBasedAPI)
				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);

			graph.Save.Press();
		}

		public PXChangeID<BAccount, BAccount.acctCD> ChangeID;
		#endregion

		#region Cache Attached
		#region NotificationSource
		[PXSelector(typeof(Search<NotificationSetup.setupID,
            Where<NotificationSetup.sourceCD, Equal<CRNotificationSource.bAccount>>>),
			DescriptionField = typeof(NotificationSetup.notificationCD),
			SelectorMode = PXSelectorMode.DisplayModeText | PXSelectorMode.NoAutocomplete)]
        [PXUIField(DisplayName = "Mailing ID")]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIEnabled(typeof(Where<NotificationSource.setupID.IsNull>))]
        protected virtual void NotificationSource_SetupID_CacheAttached(PXCache sender)
        {
        }
        [PXDBString(10, IsUnicode = true)]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
        protected virtual void NotificationSource_ClassID_CacheAttached(PXCache sender)
        {
        }
        [PXCheckUnique(typeof(NotificationSource.setupID), IgnoreNulls = false,
            Where = typeof(Where<NotificationSource.refNoteID, Equal<Current<NotificationSource.refNoteID>>>))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void NotificationSource_NBranchID_CacheAttached(PXCache sender)
        {

        }
        [PXUIField(DisplayName = "Report")]
        [PXSelector(typeof(Search<SiteMap.screenID,
            Where<SiteMap.url, Like<urlReports>,
                And<SiteMap.screenID, Like<PXModule.cr_>>>,
            OrderBy<Asc<SiteMap.screenID>>>), typeof(SiteMap.screenID), typeof(SiteMap.title),
            Headers = new string[] { CA.Messages.ReportID, CA.Messages.ReportName },
            DescriptionField = typeof(SiteMap.title))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void NotificationSource_ReportID_CacheAttached(PXCache sender)
        {
        }
        #endregion

        #region NotificationRecipient
        [PXDBDefault(typeof(NotificationSource.sourceID))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void NotificationRecipient_SourceID_CacheAttached(PXCache sender)
        {
        }
        [PXDefault]
        [CRMContactType.List]
        [PXCheckDistinct(typeof(NotificationRecipient.contactID),
            Where = typeof(Where<NotificationRecipient.sourceID, Equal<Current<NotificationRecipient.sourceID>>,
            And<NotificationRecipient.refNoteID, Equal<Current<BAccount.noteID>>>>))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void NotificationRecipient_ContactType_CacheAttached(PXCache sender)
        {
        }
        [PXUIField(DisplayName = "Contact ID")]
        [PXNotificationContactSelector(typeof(NotificationRecipient.contactType), DirtyRead = true)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void NotificationRecipient_ContactID_CacheAttached(PXCache sender)
        {
        }
        [PXDBString(10, IsUnicode = true)]
		[PXMergeAttributes(Method = MergeMethod.Replace)]	// remove PXDefault from DAC
        protected virtual void NotificationRecipient_ClassID_CacheAttached(PXCache sender)
        {
        }
        [PXUIField(DisplayName = "Email", Enabled = false)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void NotificationRecipient_Email_CacheAttached(PXCache sender)
        {
        }
        #endregion

        #region CROpportunityClass
        [PXUIField(DisplayName = "Class Description")]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void CROpportunityClass_Description_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

        #region Event Handlers
        #region NotificationRecipient Events
        protected virtual void NotificationRecipient_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            NotificationRecipient row = (NotificationRecipient)e.Row;
            if (row == null) return;
            Contact contact = PXSelectorAttribute.Select<NotificationRecipient.contactID>(cache, row) as Contact;
            if (contact == null)
            {
                switch (row.ContactType)
                {
                    case CRMContactType.Primary:
                        var defContactAddress = this.GetExtension<DefContactAddressExt>();
                        contact = defContactAddress.DefContact.SelectWindowed(0, 1);
                        break;
                    case CRMContactType.Shipping:
                        var defLocationExt = this.GetExtension<DefLocationExt>();
                        contact = defLocationExt.DefLocationContact.View.SelectSingle(new object[] { BAccount.Cache.Current }) as Contact;
                        break;
                }
            }
            if (contact != null)
                row.Email = contact.EMail;
        }
        #endregion  

        #region SOOrder

		[SOOrderStatus.ListWithoutOrders()]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void SOOrder_Status_CacheAttached(PXCache sender)
		{

		}

		#endregion

		#region BAccount

		[PXDimensionSelector("BIZACCT", 
			typeof(Search2<BAccount.acctCD,
					LeftJoin<Contact, On<Contact.bAccountID, Equal<BAccount.bAccountID>, And<Contact.contactID, Equal<BAccount.defContactID>>>,
					LeftJoin<Address, On<Address.bAccountID, Equal<BAccount.bAccountID>, And<Address.addressID, Equal<BAccount.defAddressID>>>>>,
				Where2<Where<BAccount.type, Equal<BAccountType.customerType>,
					Or<BAccount.type, Equal<BAccountType.prospectType>,
					Or<BAccount.type, Equal<BAccountType.combinedType>,
					Or<BAccount.type, Equal<BAccountType.vendorType>>>>>,
					And<Match<Current<AccessInfo.userName>>>>>),
			typeof(BAccount.acctCD),
			typeof(BAccount.acctCD), typeof(BAccount.acctName), typeof(BAccount.type), typeof(BAccount.classID), typeof(BAccount.status), typeof(Contact.phone1), 
			typeof(Address.city), typeof(Address.countryID), typeof(Contact.eMail),
			DescriptionField = typeof(BAccount.acctName))]
		[PXUIField(DisplayName = "Business Account ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void BAccount_AcctCD_CacheAttached(PXCache cache)
		{
			
		}

		[CRMParentBAccount(typeof(BAccount.bAccountID))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		public virtual void _(Events.CacheAttached<BAccount.parentBAccountID> e) { }

		protected virtual void BAccount_ClassID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = Setup.Current.DefaultCustomerClassID;
		}

		protected virtual void BAccount_Type_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = BAccountType.ProspectType;
		}

	    protected virtual void BAccount_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var row = (BAccount)e.Row;
			if (row == null) return;

			var isNotInserted = cache.GetStatus(row) != PXEntryStatus.Inserted;
			Opportunities.Cache.AllowInsert = isNotInserted;
			Cases.Cache.AllowInsert = isNotInserted;
			Members.Cache.AllowInsert = isNotInserted;

			var isCustomerOrCombined = row.Type == BAccountType.CustomerType || row.Type == BAccountType.CombinedType;
			var isVendorOrCombined = row.Type == BAccountType.VendorType || row.Type == BAccountType.CombinedType;
			var isCustomerOrProspect = row.Type == BAccountType.CustomerType || row.Type == BAccountType.ProspectType;
			var isCustomerOrProspectOrCombined = isCustomerOrProspect || row.Type == BAccountType.CombinedType;

			addOpportunity.SetEnabled(isNotInserted && isCustomerOrProspectOrCombined);
			addCase.SetEnabled(isNotInserted && isCustomerOrProspectOrCombined);

			ChangeID.SetEnabled(row.IsBranch != true);

			PXUIFieldAttribute.SetEnabled<BAccount.parentBAccountID>(cache, row, isCustomerOrCombined == false || PXAccess.FeatureInstalled<FeaturesSet.parentChildAccount>() == false);

			PXStringListAttribute.SetList<BAccount.status>(cache, row,
				isCustomerOrCombined 
					? new CustomerStatus.ListAttribute() as PXStringListAttribute
					: new CustomerStatus.BusinessAccountListAttribute() as PXStringListAttribute);
		}

		protected virtual void BAccount_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			BAccount row = e.Row as BAccount;

			if (row != null && (row.Type == BAccountType.CustomerType || row.Type == BAccountType.CombinedType))
			{
				AR.Customer customer =
					SelectFrom<AR.Customer>
					.Where<AR.Customer.acctCD.IsEqual<@P.AsString>>
					.View
					.Select(this, row.AcctCD);
				AR.CustomerMaint.VerifyParentBAccountID<BAccount.parentBAccountID>(this, cache, customer, row);
			}
		}
		#endregion

		#region Contact

		[PXDefault(ContactTypesAttribute.BAccountProperty)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<Contact.contactType> e) { }

		[PXUIField(DisplayName = "Business Account", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBDefault(typeof(BAccount.bAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(BAccount.bAccountID), SubstituteKey = typeof(BAccount.acctCD), DirtyRead = true)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		public virtual void _(Events.CacheAttached<CRLead.bAccountID> e) { }

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Source Lead Contact ID")]
		public virtual void _(Events.CacheAttached<CRLead.contactID> e) { }

		[PXDBInt(BqlField = typeof(Standalone.CROpportunityRevision.contactID))]
		[PXUIField(DisplayName = "Main Contact ID", Visible = false)]
		public virtual void _(Events.CacheAttached<CROpportunity.contactID> e) { }

		[PXUIField(DisplayName = "Business Account", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBDefault(typeof(BAccount.bAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(BAccount.bAccountID), SubstituteKey = typeof(BAccount.acctCD), DirtyRead = true)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		public virtual void _(Events.CacheAttached<Contact.bAccountID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Member Name", Visibility = PXUIVisibility.SelectorVisible, Visible = true, Enabled = false)]
		public virtual void _(Events.CacheAttached<Contact.memberName> e) { }

		[PXDBInt(BqlField = typeof(Standalone.CROpportunityRevision.bAccountID))]
		[PXUIField(DisplayName = "Business Account", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBDefault(typeof(BAccount.bAccountID))]
		[PXSelector(typeof(BAccount.bAccountID), SubstituteKey = typeof(BAccount.acctCD), DirtyRead = true)]
		[PXMergeAttributes(Method = MergeMethod.Replace)]   // replace single attribute from DAC
		public virtual void _(Events.CacheAttached<CROpportunity.bAccountID> e) { }

        [PXUIField(DisplayName = "Location")]                
        [PXDBDefault(typeof(CRLocation.locationID))]
        [LocationActive(typeof(Where<Location.bAccountID, Equal<Current<CROpportunity.bAccountID>>>),
            DisplayName = "Location", 
            DescriptionField = typeof(Location.descr),
            BqlField = typeof(Standalone.CROpportunityRevision.locationID),
            ValidateValue = false)]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		public virtual void _(Events.CacheAttached<CROpportunity.locationID> e) { }

		[PXDBInt(BqlField = typeof(Standalone.CROpportunityRevision.bAccountID))]
		[PXUIField(DisplayName = "Business Account", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBDefault(typeof(BAccount.bAccountID))]
		[PXSelector(typeof(BAccount.bAccountID), SubstituteKey = typeof(BAccount.acctCD), DirtyRead = true)]
		[PXMergeAttributes(Method = MergeMethod.Replace)]   // replace single attribute from DAC
		public virtual void _(Events.CacheAttached<CRQuote.bAccountID> e) { }


		[PXUIField(DisplayName = "Location")]
		[PXDBDefault(typeof(CRLocation.locationID))]
		[LocationActive(typeof(Where<Location.bAccountID, Equal<Current<CRQuote.bAccountID>>>),
			DisplayName = "Location",
			DescriptionField = typeof(Location.descr),
			BqlField = typeof(Standalone.CROpportunityRevision.locationID),
			ValidateValue = false)]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		public virtual void _(Events.CacheAttached<CRQuote.locationID> e) { }

		protected virtual void _(Events.RowUpdated<Contact> e)
		{
			var row = e.Row;
			if (row == null || row.IsActive == e?.OldRow.IsActive) return;
			
			BAccount acct = this.BAccount.Current;
			if (acct == null) return;

			if (acct.DefContactID == row.ContactID && row.IsActive == false)
			{
				acct.Status = CustomerStatus.Inactive;
				this.Caches<BAccount>().Update(acct);
			}
		}
		#endregion

		#region Lead

		[PXUIField(DisplayName = "Display Name", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CRLead.memberName> e) { }

		#endregion

		#region Address

		[PXDBDefault(typeof(BAccount.bAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXParent(typeof(Select<BAccount, Where<BAccount.bAccountID, Equal<Current<Address.bAccountID>>>>))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void Address_BAccountID_CacheAttached(PXCache sender) { }

		#endregion

		#region CRCampaignMembers


		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(BAccount.defContactID))]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		protected virtual void CRCampaignMembers_ContactID_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXCheckUnique(typeof(CRCampaignMembers.contactID))]
		protected virtual void CRCampaignMembers_CampaignID_CacheAttached(PXCache sender)
		{
		}

		#endregion

        #endregion

		#region Extensions

		#region Details

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class DefContactAddressExt : DefContactAddressExt<BusinessAccountMaint, BAccount, BAccount.acctName> { }

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class DefLocationExt : DefLocationExt<BusinessAccountMaint, DefContactAddressExt, LocationDetailsExt, BAccount, BAccount.bAccountID, BAccount.defLocationID>
			.WithUIExtension
		{
			#region State

			public override void Initialize()
			{
				base.Initialize();

			}

			public override List<Type> InitLocationFields => new List<Type>
			{
				typeof(CRLocation.vTaxCalcMode),
				typeof(CRLocation.vRetainageAcctID),
				typeof(CRLocation.vRetainageSubID),
			};

			#endregion

			#region Views

			[PXHidden]
			public PXSelectJoin<
				Location,
				InnerJoin<BAccount,
					On<BAccount.defLocationID, Equal<Location.locationID>>,
				InnerJoin<GL.Branch,
					On<GL.Branch.bAccountID, Equal<BAccount.bAccountID>>>>,
				Where<
					GL.Branch.branchID, Equal<Required<GL.Branch.branchID>>>>
				BranchLocation;

			#endregion

			#region Events

			#region Field-level

			protected virtual void _(Events.FieldDefaulting<CRLocation, CRLocation.cShipComplete> e)
			{
				BAccount acct = Base.BAccount.Current;
				if (e.Row == null || acct == null) return;

				if (acct.IsBranch == true)
				{
					e.NewValue = e.Row.CShipComplete;
					e.Cancel = true;
				}
				else
				{
					var currentLocation = BranchLocation.SelectSingle(Base.Accessinfo.BranchID);
					DefaultFrom<Location.cShipComplete>(e.Args, BranchLocation.Cache, currentLocation, true, SOShipComplete.CancelRemainder);
				}
			}

			#endregion

			#region Address Cache Attached

			[PXMergeAttributes(Method = MergeMethod.Append)]
			[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Visible), true)]
			protected virtual void _(Events.CacheAttached<Address.latitude> e) { }

			[PXMergeAttributes(Method = MergeMethod.Append)]
			[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Visible), true)]
			protected virtual void _(Events.CacheAttached<Address.longitude> e) { }

			#endregion

			#endregion
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class ContactDetailsExt : BusinessAccountContactDetailsExt<BusinessAccountMaint, CreateContactFromAccountGraphExt, BAccount, BAccount.bAccountID> { }

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class LocationDetailsExt : LocationDetailsExt<BusinessAccountMaint, BAccount, BAccount.bAccountID> { }

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class PrimaryContactGraphExt : CRPrimaryContactGraphExt<
			BusinessAccountMaint, ContactDetailsExt,
			BAccount, BAccount.bAccountID, BAccount.primaryContactID>
		{
			protected override PXView ContactsView => this.ContactDetailsExtension.Contacts.View;

			#region Events

			[PXVerifySelector(typeof(
				SelectFrom<Contact>
				.Where<
					Contact.bAccountID.IsEqual<BAccount.bAccountID.FromCurrent>
					.And<Contact.contactType.IsEqual<ContactTypesAttribute.person>>
					.And<Contact.isActive.IsEqual<True>>
				>
				.SearchFor<Contact.contactID>),
				fieldList: new[]
				{
					typeof(Contact.displayName),
					typeof(Contact.salutation),
					typeof(Contact.phone1),
					typeof(Contact.eMail)
				},
				VerifyField = false,
				DescriptionField = typeof(Contact.displayName)
			)]
			[PXUIField(DisplayName = "Name")]
			[PXMergeAttributes(Method = MergeMethod.Merge)]
			protected virtual void _(Events.CacheAttached<BAccount.primaryContactID> e) { }

			protected virtual void _(Events.FieldUpdated<BAccount, BAccount.acctName> e)
			{
				BAccount row = e.Row;

				if (row.PrimaryContactID != null &&
					this.PrimaryContactCurrent.SelectSingle() is Contact primaryContact &&
					(row.AcctName != null && !row.AcctName.Equals(primaryContact.FullName)))
				{
					primaryContact.FullName = row.AcctName;

					this.PrimaryContactCurrent.Update(primaryContact);
				}
			}

			#endregion
		}

		#endregion

		#region Address Lookup Extension

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class BusinessAccountMaintAddressLookupExtension : CR.Extensions.AddressLookupExtension<BusinessAccountMaint, BAccount, Address>
		{
			protected override string AddressView => nameof(DefContactAddressExt.DefAddress);
			protected override string ViewOnMap => nameof(DefContactAddressExt.ViewMainOnMap);
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class BusinessAccountMaintDefLocationAddressLookupExtension : CR.Extensions.AddressLookupExtension<BusinessAccountMaint, BAccount, Address>
		{
			protected override string AddressView => nameof(DefLocationExt.DefLocationAddress);
			protected override string ViewOnMap => nameof(DefLocationExt.ViewDefLocationAddressOnMap);
		}

		#endregion

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class ExtendToCustomer : ExtendToCustomerGraph<BusinessAccountMaint, BAccount>
		{
			#region Initialization 


			protected override SourceAccountMapping GetSourceAccountMapping()
			{
				return new SourceAccountMapping(typeof(BAccount));
			}

			#endregion

			#region Overrides

			protected override void _(Events.RowSelected<BAccount> e)
			{
				base._(e);

				viewCustomer.SetVisible(viewCustomer.GetEnabled());
				extendToCustomer.SetVisible(extendToCustomer.GetEnabled());
			}

			#endregion
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class ExtendToVendor : ExtendToVendorGraph<BusinessAccountMaint, BAccount>
		{
			#region Initialization

			protected override SourceAccountMapping GetSourceAccountMapping()
			{
				return new SourceAccountMapping(typeof(BAccount));
			}

			#endregion

			#region Overrides

			protected override void _(Events.RowSelected<BAccount> e)
			{
				base._(e);

				var enabled = viewVendor.GetEnabled();
				viewVendor.SetVisible(enabled);

				enabled = extendToVendor.GetEnabled();
				extendToVendor.SetVisible(enabled);
			}

			#endregion
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class DefaultAccountOwner : CRDefaultDocumentOwner<
			BusinessAccountMaint, BAccount,
			BAccount.classID, BAccount.ownerID, BAccount.workgroupID>
		{ }

		/// <exclude/>
		public class CRDuplicateEntitiesForBAccountGraphExt : CRDuplicateEntities<BusinessAccountMaint, BAccount>
		{
			#region Workflow

			// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
			public class Workflow : PXGraphExtension<CRDuplicateEntitiesForBAccountGraphExt, BusinessAccountWorkflow, BusinessAccountMaint>
			{
				public static bool IsActive()
				{
					return IsExtensionActive();
				}

				public sealed override void Configure(PXScreenConfiguration configuration) =>
					Configure(configuration.GetScreenConfigurationContext<BusinessAccountMaint, BAccount>());

				protected static void Configure(WorkflowContext<BusinessAccountMaint, BAccount> context)
				{
					#region Conditions
					Condition Bql<T>() where T : IBqlUnary, new() => context.Conditions.FromBql<T>();
					var conditions = new
					{
						IsCloseAsDuplicateDisabled
							= Bql<Customer.status.IsNotIn<CustomerStatus.prospect, CustomerStatus.active, CustomerStatus.hold, CustomerStatus.creditHold, CustomerStatus.oneTime>>(),
					}.AutoNameConditions();
					#endregion

					context
						.UpdateScreenConfigurationFor(screen =>
						{
							return screen
								.WithActions(actions =>
								{
									// "Validation" folder
									actions.Add<CRDuplicateEntitiesForBAccountGraphExt>(e => 
													e.CheckForDuplicates, a => a
														.WithCategory(context.Categories.Get(BusinessAccountWorkflow.CategoryNames.Validation)));
									actions.Add<CRDuplicateEntitiesForBAccountGraphExt>(e => 
													e.MarkAsValidated, a => a
														.WithCategory(context.Categories.Get(BusinessAccountWorkflow.CategoryNames.Validation)));
									actions.Add<CRDuplicateEntitiesForBAccountGraphExt>(e => 
													e.CloseAsDuplicate, a => a
														.WithCategory(context.Categories.Get(BusinessAccountWorkflow.CategoryNames.Validation))
														.IsDisabledWhen(conditions.IsCloseAsDuplicateDisabled));
								});
						});
				}
			}

			#endregion

			#region Initialization 

			public override Type AdditionalConditions => typeof(
				
				DuplicateContact.contactType.IsEqual<ContactTypesAttribute.bAccountProperty>
				.And<BAccountR.status.IsNotEqual<CustomerStatus.inactive>>
			);

			public override string WarningMessage => Messages.BAccountHavePossibleDuplicates;

			public static bool IsActive()
			{
				return IsExtensionActive();
			}

			public override void Initialize()
			{
				base.Initialize();

				DuplicateDocuments = new PXSelectExtension<DuplicateDocument>(Base.GetExtension<DefContactAddressExt>().DefContact);
			}

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(BAccount)) { Key = typeof(BAccount.bAccountID) };
			}

			protected override DuplicateDocumentMapping GetDuplicateDocumentMapping()
			{
				return new DuplicateDocumentMapping(typeof(Contact)) { Email = typeof(Contact.eMail) };
			}

			#endregion

			#region Events

			protected virtual void _(Events.FieldUpdated<BAccount, BAccount.status> e)
			{
				BAccount row = e.Row as BAccount;
				if (e.Row == null)
					return;

				if (row.Status != CustomerStatus.Inactive && row.Status != (string)e.OldValue)
				{
					var dupDoc = base.DuplicateDocuments.Current;

					base.DuplicateDocuments.SetValueExt<DuplicateDocument.duplicateStatus>(dupDoc, DuplicateStatusAttribute.NotValidated);
				}
			}

			protected virtual void _(Events.RowSelected<Extensions.CRDuplicateEntities.Document> e)
			{
				if (e.Row == null) return;

				DuplicateAttach.SetVisible(false);
			}

			protected virtual void _(Events.RowSelected<Extensions.CRDuplicateEntities.CRDuplicateRecord> e)
			{
				if (e.Row != null && e.Row.CanBeMerged != true)
				{
					DuplicatesForMerging.Cache.RaiseExceptionHandling<CRDuplicateRecord.canBeMerged>(e.Row, e.Row.CanBeMerged,
						new PXSetPropertyException(Messages.CannotMergeTwoCustomers, PXErrorLevel.RowWarning));
				}
			}

			#endregion

			#region Actions

			[PXUIField(DisplayName = Messages.MarkAsValidated)]
			[PXButton]
			public override IEnumerable markAsValidated(PXAdapter adapter)
			{
				base.markAsValidated(adapter);

				foreach (PXResult<BAccount, Contact> pxresult in adapter.Get())
				{
					var account = (BAccount)pxresult;

					var defContactAddress = Base.GetExtension<DefContactAddressExt>();
					Contact defContact = defContactAddress.DefContact.View.SelectSingleBound(new object[] { account }) as Contact;

					if (defContact != null)
					{
						defContact = (Contact)defContactAddress.DefContact.Cache.CreateCopy(defContact);

						defContact.DuplicateStatus = DuplicateStatusAttribute.Validated;
						defContact.DuplicateFound = false;

						defContactAddress.DefContact.Update(defContact);

						if (Base.IsContractBasedAPI)
							Base.Save.Press();
					}
				}

				return adapter.Get();
			}

			#endregion

			#region Overrides

			public override BAccount GetTargetEntity(int targetID)
			{
				return PXSelect<BAccount, Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.Select(Base, targetID);
			}

			public override Contact GetTargetContact(BAccount targetEntity)
			{
				return PXSelect<Contact, Where<Contact.contactID, Equal<Required<BAccount.defContactID>>>>.Select(Base, targetEntity.DefContactID);
			}

			public override Address GetTargetAddress(BAccount targetEntity)
			{
				return PXSelect<Address, Where<Address.addressID, Equal<Required<BAccount.defAddressID>>>>.Select(Base, targetEntity.DefAddressID);
			}

			public override bool CheckIsActive()
			{
				BAccount account = Base.BAccount.Current;

				if (account == null)
					return false;

				return account.Status != CustomerStatus.Inactive;
			}

			protected override bool WhereMergingMet(CRDuplicateResult result)
			{
				return true; // dummy
			}

			protected override bool CanBeMerged(CRDuplicateResult result)
			{
				return Base.BAccount.Current.Type == BAccountType.ProspectType
				       || result.GetItem<BAccountR>()?.Type == BAccountType.ProspectType;
			}

			public override void DoDuplicateAttach(DuplicateDocument duplicateDocument)
			{
				return;
			}

			public override PXResult<Contact> GetGramContext(DuplicateDocument duplicateDocument)
			{
				return new PXResult<Contact, Address, CRLocation, BAccount>(
					duplicateDocument.Base as Contact,
					Base.GetExtension<DefContactAddressExt>().DefAddress.SelectSingle(),
					Base.GetExtension<DefLocationExt>().DefLocation.SelectSingle(),
					Base.BAccount.Current
				);
			}

			#endregion
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class BAccountLocationSharedContactOverrideGraphExt : SharedChildOverrideGraphExt<BusinessAccountMaint, BAccountLocationSharedContactOverrideGraphExt>
		{
			#region Initialization 

			public override bool ViewHasADelegate => true;

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(CRLocation))
				{
					RelatedID = typeof(CRLocation.bAccountID),
					ChildID = typeof(CRLocation.defContactID),
					IsOverrideRelated = typeof(CRLocation.overrideContact)
				};
			}

			protected override RelatedMapping GetRelatedMapping()
			{
				return new RelatedMapping(typeof(BAccount))
				{
					RelatedID = typeof(BAccount.bAccountID),
					ChildID = typeof(BAccount.defContactID)
				};
			}

			protected override ChildMapping GetChildMapping()
			{
				return new ChildMapping(typeof(Contact))
				{
					ChildID = typeof(Contact.contactID),
					RelatedID = typeof(Contact.bAccountID),
				};
			}

			#endregion
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class BAccountLocationSharedAddressOverrideGraphExt : SharedChildOverrideGraphExt<BusinessAccountMaint, BAccountLocationSharedAddressOverrideGraphExt>
		{
			#region Initialization 

			public override bool ViewHasADelegate => true;

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(CRLocation))
				{
					RelatedID = typeof(CRLocation.bAccountID),
					ChildID = typeof(CRLocation.defAddressID),
					IsOverrideRelated = typeof(CRLocation.overrideAddress)
				};
			}

			protected override RelatedMapping GetRelatedMapping()
			{
				return new RelatedMapping(typeof(BAccount))
				{
					RelatedID = typeof(BAccount.bAccountID),
					ChildID = typeof(BAccount.defAddressID)
				};
			}

			protected override ChildMapping GetChildMapping()
			{
				return new ChildMapping(typeof(Address))
				{
					ChildID = typeof(Address.addressID),
					RelatedID = typeof(Address.bAccountID),
				};
			}

			#endregion
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class CreateLeadFromAccountGraphExt : CRCreateLeadAction<BusinessAccountMaint, BAccount>
		{
			#region Initialization

			public override void Initialize()
			{
				base.Initialize();

				Addresses = new PXSelectExtension<DocumentAddress>(Base.GetExtension<DefContactAddressExt>().DefAddress);
				Contacts = new PXSelectExtension<DocumentContact>(Base.GetExtension<PrimaryContactGraphExt>().PrimaryContactCurrent);
			}

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(BAccount)) { ContactID = typeof(BAccount.primaryContactID) };
			}
			protected override DocumentContactMapping GetDocumentContactMapping()
			{
				return new DocumentContactMapping(typeof(Contact)) { Email = typeof(Contact.eMail) };
			}
			protected override DocumentAddressMapping GetDocumentAddressMapping()
			{
				return new DocumentAddressMapping(typeof(Address));
			}

			#endregion

			#region Actions

			[PXUIField(DisplayName = Messages.AddNewLead, FieldClass = FeaturesSet.customerModule.FieldClass, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
			[PXButton]
			public override void createLead()
			{
				if (Base.IsDirty)
					Base.Actions.PressSave();

				var document = Documents.Current;

				if (document == null) return;

				var graph = PXGraph.CreateInstance<LeadMaint>();

				var newLead = graph.Lead.Insert();

				newLead.BAccountID = document.BAccountID;
				newLead.OverrideRefContact = newLead.OverrideAddress = true;
				newLead.FullName = Base.CurrentBAccount.Current.AcctName;

				newLead.OverrideSalesTerritory = document.OverrideSalesTerritory;
				if (newLead.OverrideSalesTerritory is true)
				{
					newLead.SalesTerritoryID = document.SalesTerritoryID;
				}

				CRLeadClass cls = PXSelect<
						CRLeadClass,
					Where<
						CRLeadClass.classID, Equal<Current<CRLead.classID>>>>
					.SelectSingleBound(Base, new object[] { newLead });
				if (cls?.DefaultOwner == CRDefaultOwnerAttribute.Source)
				{
					newLead.WorkgroupID = document.WorkgroupID;
					newLead.OwnerID = document.OwnerID;
				}

				UDFHelper.CopyAttributes(Base.CurrentBAccount.Cache, Base.CurrentBAccount.Current, graph.Lead.Cache, newLead, newLead.ClassID);

				graph.Lead.Update(newLead);

				if (!Base.IsContractBasedAPI)
					PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);

				graph.Save.Press();
			}
			#endregion

			#region Events

			public virtual void _(Events.RowSelected<BAccount> e)
			{
				CreateLead.SetEnabled(e.Row?.Type != null && e.Row.Type.IsIn(BAccountType.ProspectType, BAccountType.CustomerType, BAccountType.CombinedType));
			}

			#endregion
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class CreateContactFromAccountGraphExt : CRCreateContactActionBase<BusinessAccountMaint, BAccount>
		{
			#region Initialization

			protected override PXSelectBase<CRPMTimeActivity> Activities => Base.GetExtension<BusinessAccountMaint_ActivityDetailsExt>().Activities;

			public override void Initialize()
			{
				base.Initialize();

				Addresses = new PXSelectExtension<DocumentAddress>(Base.GetExtension<DefContactAddressExt>().DefAddress);
				Contacts = new PXSelectExtension<DocumentContact>(Base.GetExtension<DefContactAddressExt>().DefContact);
			}

			protected override DocumentContactMapping GetDocumentContactMapping()
			{
				return new DocumentContactMapping(typeof(Contact)) { Email = typeof(Contact.eMail) };
			}

			protected override DocumentAddressMapping GetDocumentAddressMapping()
			{
				return new DocumentAddressMapping(typeof(Address));
			}

			#endregion

			#region Events

			public virtual void _(Events.RowSelected<ContactFilter> e)
			{
				PXUIFieldAttribute.SetReadOnly<ContactFilter.fullName>(e.Cache, e.Row, true);
			}

			public virtual void _(Events.FieldDefaulting<ContactFilter, ContactFilter.fullName> e)
			{
				e.NewValue = Contacts.SelectSingle()?.FullName;
			}

			#endregion

			#region Overrides

			protected override void FillRelations(PXGraph graph, Contact target)
			{
			}

			protected override void FillNotesAndAttachments(PXGraph graph, object src_row, PXCache dst_cache, Contact dst_row)
			{
			}

			protected override IConsentable MapConsentable(DocumentContact source, IConsentable target)
			{
				return target;
			}

			#endregion
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class UpdateRelatedContactInfoFromAccountGraphExt : CRUpdateRelatedContactInfoGraphExt<BusinessAccountMaint>
		{
			#region Events 

			protected virtual void _(Events.RowPersisting<Contact> e)
			{
				if (e.Row == null)
					return;

				SetUpdateRelatedInfo(e, GetFields_ContactInfoExt(e.Cache, e.Row).Union(new[] { nameof(CR.Contact.DefAddressID) }));
			}

			protected virtual void _(Events.RowPersisting<Address> e)
			{
				if (e.Row == null)
					return;

				SetUpdateRelatedInfo(e, GetFields_ContactInfoExt(e.Cache));
			}

			protected virtual void _(Events.RowPersisting<CRLead> e)
			{
				if (e.Row == null)
					return;

				SetUpdateRelatedInfo<CRLead, CRLead.refContactID>(e);
			}

			protected virtual void _(Events.RowPersisted<Contact> e)
			{
				var row = e.Row;
				if (row == null
					|| UpdateRelatedInfo != true
					|| e.TranStatus != PXTranStatus.Open
					|| e.Operation.Command() != PXDBOperation.Update)
					return;

				BAccount account = Base.BAccount.Current ?? PXSelect<
						BAccount,
						Where<
							BAccount.bAccountID.IsEqual<@P.AsInt>
							.And<BAccount.defContactID.IsEqual<@P.AsInt>>>>
					.Select(Base, row.BAccountID, row.ContactID);

				if (account == null)
					return;

				// Acuminator disable once PX1043 SavingChangesInEventHandlers [ISV]
				UpdateContact(e.Cache, row,
					new SelectFrom<Contact>
						.LeftJoin<Standalone.CRLead>
							.On<Standalone.CRLead.contactID.IsEqual<Contact.contactID>>
						.Where<
						// Leads that are linked to the same Account
							Contact.bAccountID.IsEqual<@P.AsInt>
							.And<Contact.contactType.IsNotEqual<ContactTypesAttribute.lead>>
							.And<Standalone.CRLead.overrideRefContact.IsEqual<False>>>
						.View(Base), // Where
						account.BAccountID);
			}

			public SelectFrom<Address>
						.InnerJoin<Contact>
							.On<Contact.defAddressID.IsEqual<Address.addressID>>
						.LeftJoin<Standalone.CRLead>
						.On<Standalone.CRLead.contactID.IsEqual<Contact.contactID>>
						.Where<
							// Leads that are linked to the same Account
							Contact.bAccountID.IsEqual<@P.AsInt>
							.And<Contact.contactType.IsNotEqual<ContactTypesAttribute.lead>>
							.And<Standalone.CRLead.overrideRefContact.IsEqual<False>>>
					.View BAccountRelatedAddresses;

			protected virtual void _(Events.RowPersisted<Address> e)
			{
				var row = e.Row;
				if (row == null
					|| UpdateRelatedInfo != true
					|| e.TranStatus != PXTranStatus.Open
					|| e.Operation.Command() != PXDBOperation.Update)
					return;

				BAccount account = Base.BAccount.Current ?? PXSelect<
						BAccount,
						Where<
							BAccount.bAccountID.IsEqual<@P.AsInt>
							.And<BAccount.defAddressID.IsEqual<@P.AsInt>>>>
					.Select(Base, row.BAccountID, row.AddressID);

				if (account == null || row.AddressID != account.DefAddressID)
					return;

				// Acuminator disable once PX1043 SavingChangesInEventHandlers [ISV]
				UpdateAddress(e.Cache, row, BAccountRelatedAddresses, account.BAccountID);
			}

			#endregion
		}

		/// <exclude/>
		public class ExtensionSort
			: SortExtensionsBy<ExtensionOrderFor<BusinessAccountMaint>
				.FilledWith<
					DefContactAddressExt,
					CreateLeadFromAccountGraphExt,
					CreateContactFromAccountGraphExt
				>>
		{ }

		#endregion
	}
}
