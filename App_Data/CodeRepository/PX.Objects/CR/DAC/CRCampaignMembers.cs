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
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.CR
{
	/// <exclude/>
	[Serializable]
    [PXCacheName(Messages.CampaignMember)]
    public partial class CRCampaignMembers : IBqlTable
    {
        #region Keys
        public class PK : PrimaryKeyOf<CRCampaignMembers>.By<campaignID, contactID>
        {
            public static CRCampaignMembers Find(PXGraph graph, string campaignID, int? contactID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, campaignID, contactID, options);
        }
        public static class FK
        {
            public class Campaign : CR.CRCampaign.PK.ForeignKeyOf<CRCampaignMembers>.By<campaignID> { }
            public class MarketingList : CR.CRMarketingList.PK.ForeignKeyOf<CRCampaignMembers>.By<marketingListID> { }
        }
        #endregion

        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
        protected bool? _Selected = false;
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Selected")]
        public bool? Selected
        {
            get
            {
                return _Selected;
            }
            set
            {
                _Selected = value;
            }
        }
        #endregion

        #region CampaignID
        public abstract class campaignID : PX.Data.BQL.BqlString.Field<campaignID> { }

        [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXDBDefault(typeof(CRCampaign.campaignID))]
        [PXUIField(DisplayName = Messages.CampaignID)]
        [PXParent(typeof(Select<CRCampaign, Where<CRCampaign.campaignID, Equal<Current<CRCampaignMembers.campaignID>>>>))]
        [PXSelector(typeof(Search<CRCampaign.campaignID,
            Where<CRCampaign.isActive, Equal<True>>>))]
        public virtual String CampaignID { get; set; }
        #endregion

        #region ContactID
        public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

        [PXDBInt(IsKey = true)]
        [PXDefault]        
        [PXUIField(DisplayName = "Member Name")]
		[PXSelector(typeof(Search2<Contact.contactID,
				LeftJoin<GL.Branch,
					On<GL.Branch.bAccountID, Equal<Contact.bAccountID>,
						And<Contact.contactType, Equal<ContactTypesAttribute.bAccountProperty>>>,
				LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Contact.bAccountID>>>>,
				Where<GL.Branch.bAccountID, IsNull,
					And<Where<BAccount.bAccountID, IsNull, Or<Match<BAccount, Current<AccessInfo.userName>>>>>>>),
			typeof(Contact.contactType),
			typeof(Contact.memberName),
			typeof(Contact.salutation),
			typeof(Contact.bAccountID),
			typeof(Contact.eMail),
			typeof(Contact.phone1),
			typeof(Contact.isActive),
			DescriptionField = typeof(Contact.memberName),
			Filterable = true,
			DirtyRead = true)]
		[PXParent(typeof(Select<Contact, Where<Contact.contactID, Equal<Current<CRCampaignMembers.contactID>>>>))]
        public virtual Int32? ContactID { get; set; }
        #endregion

        #region OpportunityCreatedCount
        public abstract class opportunityCreatedCount : PX.Data.BQL.BqlInt.Field<opportunityCreatedCount> { }

        [PXInt]
        [PXUIField(DisplayName = "Opportunities Created", Enabled = false)]
        [PXDBScalar(typeof(Search5<CROpportunity.noteID,
			InnerJoin<Contact,
				On<True, Equal<True>>,
			LeftJoin<BAccount,
				On<BAccount.defContactID, Equal<Contact.contactID>>,
			InnerJoin<CRCampaign, 
				On<CROpportunity.campaignSourceID, Equal<CRCampaign.campaignID>>>>>,
			Where<
				CRCampaign.campaignID, Equal<CRCampaignMembers.campaignID>,
				And<Contact.contactID, Equal<CRCampaignMembers.contactID>,
				And<Where<CROpportunity.contactID, Equal<CRCampaignMembers.contactID>, And<Contact.contactType, NotEqual<ContactTypesAttribute.lead>,
				Or<CROpportunity.leadID, Equal<Contact.noteID>, And<Contact.contactType, Equal<ContactTypesAttribute.lead>,
				Or<CROpportunity.bAccountID, Equal<BAccount.bAccountID>, And<Contact.contactType, Equal<ContactTypesAttribute.bAccountProperty>>>>>>>>>>,
			Aggregate<Count>>))]
        public virtual int? OpportunityCreatedCount { get; set; }
		#endregion

		#region IncomingActivitiesLogged
		public abstract class incomingActivitiesLogged : PX.Data.BQL.BqlInt.Field<incomingActivitiesLogged> { }

		[PXInt]
		[PXUIField(DisplayName = "Incoming Activities Logged", Enabled = false)]
		[PXDBScalar(typeof(Search5<CRActivity.noteID,
			InnerJoin<CRCampaign,
				On<CRActivity.documentNoteID, Equal<CRCampaign.noteID>>,
			InnerJoin<Contact,
				On<True.IsEqual<True>>,
			LeftJoin<BAccount,
				On<BAccount.bAccountID, Equal<CRActivity.bAccountID>>,
			LeftJoin<Contact2,
				On<Contact2.contactID, Equal<BAccount.defContactID>>>>>>,
			Where<
				CRCampaign.campaignID, Equal<CRCampaignMembers.campaignID>,
				And2<
					Where<CRActivity.contactID, Equal<CRCampaignMembers.contactID>,
							And<Contact.contactType, Equal<ContactTypesAttribute.person>,
						Or<Contact2.contactID, Equal<CRCampaignMembers.contactID>,
							And<Contact2.contactType, Equal<ContactTypesAttribute.bAccountProperty>,
						Or<CRActivity.refNoteID, Equal<Contact.noteID>,
							And<Contact.contactType, Equal<ContactTypesAttribute.lead>
					>>>>>>,
				And<Contact.contactID, Equal<CRCampaignMembers.contactID>,
				And<CRActivity.incoming, Equal<True>,
				And<Where<Not<CRActivity.type, IsNull, And<CRActivity.classID, Equal<CRActivityClass.task>>>>>>>>>,
			Aggregate<Count>>))]
		public virtual int? IncomingActivitiesLogged { get; set; }
		#endregion

		#region OutgoingActivitiesLogged
		public abstract class outgoingActivitiesLogged : PX.Data.BQL.BqlInt.Field<outgoingActivitiesLogged> { }

		[PXInt]
		[PXUIField(DisplayName = "Outgoing Activities Logged", Enabled = false)]
		[PXDBScalar(typeof(Search5<CRActivity.noteID,
			InnerJoin<CRCampaign,
				On<CRActivity.documentNoteID, Equal<CRCampaign.noteID>>,
			InnerJoin<Contact,
				On<True.IsEqual<True>>,
			LeftJoin<BAccount,
				On<BAccount.bAccountID, Equal<CRActivity.bAccountID>>,
			LeftJoin<Contact2,
				On<Contact2.contactID, Equal<BAccount.defContactID>>>>>>,
			Where<
				CRCampaign.campaignID, Equal<CRCampaignMembers.campaignID>,
				And2<
					Where<CRActivity.contactID, Equal<CRCampaignMembers.contactID>,
							And<Contact.contactType, Equal<ContactTypesAttribute.person>,
						Or<Contact2.contactID, Equal<CRCampaignMembers.contactID>,
							And<Contact2.contactType, Equal<ContactTypesAttribute.bAccountProperty>,
						Or<CRActivity.refNoteID, Equal<Contact.noteID>,
							And<Contact.contactType, Equal<ContactTypesAttribute.lead>
					>>>>>>,
				And<Contact.contactID, Equal<CRCampaignMembers.contactID>,
				And<CRActivity.outgoing, Equal<True>,
				And<Where<Not<CRActivity.type, IsNull, And<CRActivity.classID, Equal<CRActivityClass.task>>>>>>>>>,
			Aggregate<Count>>))]
		public virtual int? OutgoingActivitiesLogged { get; set; }
		#endregion

		#region ActivitiesLogged
		public abstract class activitiesLogged : PX.Data.BQL.BqlInt.Field<activitiesLogged> { }

        [PXInt]
        [PXUIField(DisplayName = "Activities Logged", Enabled = false)]
		[PXDBScalar(typeof(Search5<CRActivity.noteID,
			InnerJoin<Contact,
				On<True, Equal<True>>,
			InnerJoin<CRCampaign,
				On<True, Equal<True>>>>,
			Where<
				CRCampaign.campaignID, Equal<CRCampaignMembers.campaignID>,
				And<Contact.contactID, Equal<CRCampaignMembers.contactID>,
				And<CRActivity.documentNoteID, Equal<CRCampaign.noteID>,
				And2<
					Where<CRActivity.contactID, Equal<CRCampaignMembers.contactID>, And<Contact.contactType, NotEqual<ContactTypesAttribute.lead>,
						Or<CRActivity.refNoteID, Equal<Contact.noteID>, And<Contact.contactType, Equal<ContactTypesAttribute.lead>>>>>,
				And<CRActivity.classID, NotEqual<CRActivityClass.email>,
				And<CRActivity.classID, NotEqual<CRActivityClass.task>>>>>>>,
			Aggregate<Count>>))]
		public virtual int? ActivitiesLogged { get; set; }
        #endregion

        #region EmailSendCount
        public abstract class emailSendCount : PX.Data.BQL.BqlInt.Field<emailSendCount> { }     

        [PXDBScalar(typeof(Search5<CRActivity.noteID,
            InnerJoin<Contact,
				On<True, Equal<True>>,
            InnerJoin<CRCampaign,
				On<True, Equal<True>>>>,
            Where<
				CRCampaign.campaignID, Equal<CRCampaignMembers.campaignID>,
				And<Contact.contactID, Equal<CRCampaignMembers.contactID>,
                And<CRActivity.documentNoteID, Equal<CRCampaign.noteID>,
                And2<
                    Where<CRActivity.contactID, Equal<CRCampaignMembers.contactID>, And<Contact.contactType, NotEqual<ContactTypesAttribute.lead>,
                        Or<CRActivity.refNoteID, Equal<Contact.noteID>, And<Contact.contactType, Equal<ContactTypesAttribute.lead>>>>>,
                And<CRActivity.classID, Equal<CRActivityClass.email>>>>>>,
            Aggregate<Count>>))]
        [PXInt]
        [PXUIField(DisplayName = "Emails Sent", Enabled = false)]    
        public virtual int? EmailSendCount { get; set; }
        #endregion

        #region MarketingListID
        public abstract class marketingListID : PX.Data.BQL.BqlInt.Field<marketingListID> { }
		/// <summary>
		/// A reference to the <see cref="CRMarketingList.marketingListID"/> from which the member has been added.
		/// </summary>
        [PXDBInt]
        [PXUIField(Visible = false)]
        public virtual Int32? MarketingListID { get; set; }
        #endregion

        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
        protected Byte[] _tstamp;
        [PXDBTimestamp()]
        public virtual Byte[] tstamp
        {
            get
            {
                return this._tstamp;
            }
            set
            {
                this._tstamp = value;
            }
        }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
        protected String _CreatedByScreenID;
        [PXDBCreatedByScreenID()]
        public virtual String CreatedByScreenID
        {
            get
            {
                return this._CreatedByScreenID;
            }
            set
            {
                this._CreatedByScreenID = value;
            }
        }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
        protected Guid? _CreatedByID;
        [PXDBCreatedByID()]
        public virtual Guid? CreatedByID
        {
            get
            {
                return this._CreatedByID;
            }
            set
            {
                this._CreatedByID = value;
            }
        }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
        protected DateTime? _CreatedDateTime;
        [PXDBCreatedDateTime()]
        public virtual DateTime? CreatedDateTime
        {
            get
            {
                return this._CreatedDateTime;
            }
            set
            {
                this._CreatedDateTime = value;
            }
        }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
        protected Guid? _LastModifiedByID;
        [PXDBLastModifiedByID()]
        public virtual Guid? LastModifiedByID
        {
            get
            {
                return this._LastModifiedByID;
            }
            set
            {
                this._LastModifiedByID = value;
            }
        }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
        protected String _LastModifiedByScreenID;
        [PXDBLastModifiedByScreenID()]
        public virtual String LastModifiedByScreenID
        {
            get
            {
                return this._LastModifiedByScreenID;
            }
            set
            {
                this._LastModifiedByScreenID = value;
            }
        }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
        protected DateTime? _LastModifiedDateTime;
        [PXDBLastModifiedDateTime()]
        public virtual DateTime? LastModifiedDateTime
        {
            get
            {
                return this._LastModifiedDateTime;
            }
            set
            {
                this._LastModifiedDateTime = value;
            }
        }
        #endregion
    }

	[Serializable]
	[PXHidden]
	public partial class CRCampaignMembers2 : CRCampaignMembers
	{
		public new abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }
		public new abstract class campaignID : PX.Data.BQL.BqlString.Field<campaignID> { }
	}

	[Serializable]
	public partial class OperationParam : IBqlTable
	{
		#region CampaignID

		public abstract class campaignID : PX.Data.BQL.BqlString.Field<campaignID> { }
		protected String _CampaignID;
		[PXString]
		[PXDefault]
		[PXUIField(DisplayName = "Campaign ID")]
		[PXSelector(typeof(CRCampaign.campaignID), DescriptionField = typeof(CRCampaign.campaignName))]
		public virtual String CampaignID
		{
			get
			{
				return this._CampaignID;
			}
			set
			{
				this._CampaignID = value;
			}
		}

		#endregion

		#region DataSource

		public abstract class dataSource : PX.Data.BQL.BqlString.Field<dataSource> { }
		[PXString(20)]
		[PXUIField(DisplayName = "Data Source", Visibility = PXUIVisibility.SelectorVisible)]
		[DataSourceList]
		[PXDefault(DataSourceList.Inquiry)]
		public string DataSource { get; set; }

		public class DataSourceList : PXStringListAttribute
		{
			public DataSourceList()
				: base(new[] { Inquiry, MarketingLists }, new[] { Messages.Inquiry, Messages.MailList })
			{

			}
			public const string Inquiry = "Inquiry";
			public const string MarketingLists = "MarketingLists";

			public class contacts : PX.Data.BQL.BqlString.Constant<contacts>
			{
				public contacts() : base(Inquiry) { }
			}

			public class marketingLists : PX.Data.BQL.BqlString.Constant<marketingLists>
			{
				public marketingLists() : base(MarketingLists) { }
			}
		}

		#endregion

		#region ContactGI

		public abstract class contactGI : PX.Data.BQL.BqlGuid.Field<contactGI> { }

		[PXGuid]
		[PXUIField(DisplayName = "Generic Inquiry")]
		[CM.ContactGISelector]
		public virtual Guid? ContactGI { get; set; }

		#endregion

		#region SourceMarketingListID

		public abstract class sourceMarketingListID : PX.Data.BQL.BqlInt.Field<sourceMarketingListID> { }

		[PXInt]
		[PXDBDefault(typeof(CRMarketingList.marketingListID))]
		[PXUIField(DisplayName = "Marketing List")]        
		[PXSelector(typeof(Search<CRMarketingList.marketingListID>),
            SubstituteKey = typeof(CRMarketingList.mailListCode),
			DescriptionField = typeof(CRMarketingList.name))]        
		public virtual Int32? SourceMarketingListID { get; set; }

		#endregion

		#region MarketingListID

		public abstract class marketingListID : PX.Data.BQL.BqlInt.Field<marketingListID> { }

		[PXInt]
		[PXDBDefault(typeof(CRMarketingList.marketingListID))]
		[PXUIField(DisplayName = "Marketing List")]
		[PXSelector(typeof(Search<CRMarketingList.marketingListID>),
			DescriptionField = typeof(CRMarketingList.mailListCode))]
		public virtual Int32? MarketingListID { get; set; }

		#endregion

		#region SharedGIFilter

		public abstract class sharedGIFilter : PX.Data.BQL.BqlGuid.Field<sharedGIFilter> { }
        [PXGuid]		
		[PXUIField(DisplayName = "Shared Filter to Apply", Visibility = PXUIVisibility.SelectorVisible)]
        [FilterList(typeof(contactGI), IsSiteMapIdentityScreenID = false, IsSiteMapIdentityGIDesignID = true)]
        [PXFormula(typeof(Default<contactGI>))]
        public virtual Guid? SharedGIFilter { get; set; }

		#endregion

		#region Action
		public abstract class action : PX.Data.BQL.BqlString.Field<action> { }

		[PXString(6, IsFixed = true)]
		[ActionList]
		public virtual string Action { get; set; }

		[PXString(6, IsFixed = true)]
		[ActionList]
		public virtual string MarketingListMemberAction { get; set; }

		public class ActionList : PXStringListAttribute
		{
			public ActionList()
				: base(new[] { Add, Delete }, new[] { Messages.AddMembers, Messages.Remove })
			{

			}
			public const string Add = "Add";
			public const string Delete = "Delete";

			public class add : PX.Data.BQL.BqlString.Constant<add>
			{
				public add() : base(Add) { }
			}

			public class delete : PX.Data.BQL.BqlString.Constant<delete>
			{
				public delete() : base(Delete) { }
			}
		}

		#endregion
	}

}
