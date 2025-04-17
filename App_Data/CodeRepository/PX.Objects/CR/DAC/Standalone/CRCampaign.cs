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
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Data.EP;
using PX.Objects.AR;
using PX.Objects.AP;
using PX.Objects.GL;
using PX.Objects.SO;
using PX.Objects.CR.Workflows;

namespace PX.Objects.CR.DAC.Standalone
{
	/// <exclude/>
	[PXCacheName(Messages.CampaignStats)]
	public partial class CRCampaign : PX.Data.IBqlTable
	{
		#region Selected

		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

		protected bool? _Selected = false;

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected")]
		public bool? Selected
		{
			get { return _Selected; }
			set { _Selected = value; }
		}

		#endregion
		#region CampaignID

		public abstract class campaignID : PX.Data.BQL.BqlString.Field<campaignID> { }

		protected String _CampaignID;

		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = Messages.CampaignID, Visibility = PXUIVisibility.SelectorVisible)]
		[AutoNumber(typeof(CRSetup.campaignNumberingID), typeof(AccessInfo.businessDate))]
		[PXSelector(typeof(CRCampaign.campaignID), DescriptionField = typeof(CRCampaign.campaignName))]
		[PXFieldDescription]
		public virtual String CampaignID
		{
			get { return this._CampaignID; }
			set { this._CampaignID = value; }
		}

		#endregion
		#region CampaignName

		public abstract class campaignName : PX.Data.BQL.BqlString.Field<campaignName> { }

		protected String _CampaignName;

		[PXDBString(60, IsUnicode = true)]
		[PXDefault("", PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Campaign Name", Visibility = PXUIVisibility.SelectorVisible)]
		[PXNavigateSelector(typeof(CRCampaign.campaignName))]
		[PXFieldDescription]
		public virtual String CampaignName
		{
			get { return this._CampaignName; }
			set { this._CampaignName = value; }
		}

		#endregion

		#region LeadsGenerated
		public abstract class leadsGenerated : PX.Data.BQL.BqlInt.Field<leadsGenerated> { }

		[PXInt]
		[PXUIField(DisplayName = "Leads Generated", Enabled = false)]
		[PXDBScalar(typeof(Search4<
				Contact.contactID,
			Where<
				Contact.campaignID, Equal<CRCampaign.campaignID>,
				And<Contact.contactType, Equal<ContactTypesAttribute.lead>>>,
			Aggregate<
				Count<Contact.contactID>>>))]
		public virtual Int32? LeadsGenerated { get; set; }
		#endregion

		#region LeadsConverted
		public abstract class leadsConverted : PX.Data.BQL.BqlInt.Field<leadsConverted> { }

		[PXInt]
		[PXUIField(DisplayName = "Leads Converted", Enabled = false)]
		[PXDBScalar(typeof(Search4<
				CRLead.contactID,
			Where<
				CRLead.campaignID, Equal<CRCampaign.campaignID>,
				And<CRLead.status, Equal<LeadWorkflow.States.converted>>>,
			Aggregate<
				Count<CRLead.contactID>>>))]
		public virtual Int32? LeadsConverted { get; set; }
		#endregion

		#region Contacts
		public abstract class contacts : PX.Data.BQL.BqlInt.Field<contacts> { }

		[PXInt]
		[PXUIField(DisplayName = "Total Members", Enabled = false)]
		[PXDBScalar(typeof(Search5<
				CRCampaignMembers.contactID,
			InnerJoin<Contact, 
				On<Contact.contactID, Equal<CRCampaignMembers.contactID>>>,
			Where<
				CRCampaignMembers.campaignID, Equal<CRCampaign.campaignID>>,
			Aggregate<
				Count<CRCampaignMembers.contactID>>>))]
		public virtual Int32? Contacts { get; set; }
		#endregion

		#region MembersContacted
		public abstract class membersContacted : PX.Data.BQL.BqlInt.Field<membersContacted> { }

		[PXInt]
		[PXUIField(DisplayName = "Members Contacted", Enabled = false)]
		[PXDBScalar(typeof(Search4<
				CRCampaignMembers.contactID,
			Where<
				CRCampaignMembers.campaignID, Equal<CRCampaign.campaignID>,
				And<CRCampaignMembers.outgoingActivitiesLogged, Greater<Zero>>>,
			Aggregate<
				Count<CRCampaignMembers.contactID>>>))]
		public virtual Int32? MembersContacted { get; set; }
		#endregion

		#region MembersResponded
		public abstract class membersResponded : PX.Data.BQL.BqlInt.Field<membersResponded> { }

		[PXInt]
		[PXUIField(DisplayName = "Members Responded", Enabled = false)]
		[PXDBScalar(typeof(Search4<
				CRCampaignMembers.contactID,
			Where<
				CRCampaignMembers.campaignID, Equal<CRCampaign.campaignID>,
				And<CRCampaignMembers.incomingActivitiesLogged, Greater<Zero>>>,
			Aggregate<
				Count<CRCampaignMembers.contactID>>>))]
		public virtual Int32? MembersResponded { get; set; }
		#endregion

		#region Opportunities
		public abstract class opportunities : PX.Data.BQL.BqlInt.Field<opportunities> { }

		[PXInt]
		[PXUIField(DisplayName = "Opportunities", Enabled = false)]
		[PXDBScalar(typeof(Search4<
				CROpportunity.opportunityID,
			Where<
				CROpportunity.campaignSourceID, Equal<CRCampaign.campaignID>>,
			Aggregate<
				Count<CROpportunity.opportunityID>>>))]
		public virtual Int32? Opportunities { get; set; }
		#endregion

		#region ClosedOpportunities
		//todo: should remove reference to CROpportunity.status
		public abstract class closedOpportunities : PX.Data.BQL.BqlInt.Field<closedOpportunities> { }

		[PXInt]
		[PXUIField(DisplayName = "Won Opportunities", Enabled = false)]
		[PXDBScalar(typeof(Search4<
				CROpportunity.opportunityID,
			Where<
				CROpportunity.campaignSourceID, Equal<CRCampaign.campaignID>,
				And<CROpportunity.status, Equal<OpportunityStatus.won>>>,
			Aggregate<
				Count<CROpportunity.opportunityID>>>))]
		public virtual Int32? ClosedOpportunities { get; set; }
		#endregion

		#region OpportunitiesValue
		public abstract class opportunitiesValue : PX.Data.BQL.BqlDecimal.Field<opportunitiesValue> { }

		[PXDecimal]
		[PXUIField(DisplayName = "Opportunities Value", Enabled = false)]
		[PXDBScalar(typeof(Search4<
				CROpportunity.productsAmount,
			Where<
				CROpportunity.campaignSourceID, Equal<CRCampaign.campaignID>>,
			Aggregate<
				Sum<CROpportunity.amount, 
				Sum<CROpportunity.productsAmount, 
				Sum<CROpportunity.discTot>>>>>))]
		public virtual Decimal? OpportunitiesValue { get; set; }
		#endregion

		#region ClosedOpportunitiesValue
		//todo: should remove reference to CROpportunity.status
		public abstract class closedOpportunitiesValue : PX.Data.BQL.BqlDecimal.Field<closedOpportunitiesValue> { }

		[PXDecimal]
		[PXUIField(DisplayName = "Won Opportunities Value", Enabled = false)]
		[PXDBScalar(typeof(Search4<
				CROpportunity.productsAmount,
			Where<
				CROpportunity.campaignSourceID, Equal<CRCampaign.campaignID>,
				And<CROpportunity.status, Equal<OpportunityStatus.won>>>,
			Aggregate<
				Sum<CROpportunity.amount, 
				Sum<CROpportunity.productsAmount, 
				Sum<CROpportunity.discTot>>>>>))]
		public virtual Decimal? ClosedOpportunitiesValue { get; set; }
		#endregion

		#region LeadsGeneratedClosedOpportunities
		//todo: should remove reference to CROpportunity.status
		public abstract class leadsGeneratedClosedOpportunities : PX.Data.BQL.BqlInt.Field<leadsGeneratedClosedOpportunities> { }

		[PXInt]
		[PXUIField(DisplayName = "Generated Leads Converted to Won Opportunities", Enabled = false)]
		[PXDBScalar(typeof(
			Search5<
				Contact.contactID,
				InnerJoin<CROpportunity,
					On<CROpportunity.leadID, Equal<Contact.noteID>,
					And<CROpportunity.status, Equal<OpportunityStatus.won>>>>,
				Where<
					Contact.campaignID, Equal<CRCampaign.campaignID>>,
				Aggregate<
					Count<Contact.contactID>>>))]
		public virtual Int32? LeadsGeneratedClosedOpportunities { get; set; }
		#endregion
	}
}
