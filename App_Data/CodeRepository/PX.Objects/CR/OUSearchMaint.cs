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
using PX.Common.Extensions;
using PX.Concurrency;
using PX.CloudServices.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.EP;
using PX.Objects.AP.InvoiceRecognition;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.Common.GraphExtensions.Abstract;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.SM;
using PX.Web.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using System.Xml.Linq;
using Branch = PX.Objects.GL.Branch;
using System.Reactive.Disposables;
using PX.Objects.AP.InvoiceRecognition.DAC;

namespace PX.Objects.CR
{
	#region OUOperation
	public class OUOperation
	{
		public const string Case = "Case";
		public const string Opportunity = "Opp";
		public const string Activity = "Msg";
		public const string CreateContact = "CreateContact";
		public const string CreateLead = "CreateLead";
		public const string CreateAPDocument = "CreateAPDocument";
		public const string ViewAPDocument = "ViewAPDocument";
	}
	#endregion
	#region OUSearchEntity
	/// <exclude/>
	[Serializable]
	[PXHidden]
	public class OUSearchEntity : IBqlTable
	{
		#region EMail
		public abstract class eMail : PX.Data.BQL.BqlString.Field<eMail> { }
		private string _eMail;
		[PXUIField(DisplayName = "Email", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBString]
		[PXDBDefault(typeof(OUSearchEntity.outgoingEmail))]
		public virtual string EMail
		{
			get { return _eMail; }
			set { _eMail = value != null ? value.Trim() : null; }
		}
		#endregion
		#region DisplayName
		public abstract class displayName : PX.Data.BQL.BqlString.Field<displayName> { }
		private string _DisplayName;

		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Display Name", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual String DisplayName
		{
			get { return _DisplayName; }
			set { _DisplayName = value; }
		}
		#endregion
		#region NewContactDisplayName
		public abstract class newContactDisplayName : PX.Data.BQL.BqlString.Field<newContactDisplayName> { }
		[PXDependsOnFields(typeof(newContactLastName), typeof(newContactFirstName))]
		[PersonDisplayName(typeof(newContactLastName), typeof(newContactFirstName))]
		[PXUIField(DisplayName = "Display Name", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual String NewContactDisplayName { get; set; }
		#endregion
		#region NewContactFirstName
		public abstract class newContactFirstName : PX.Data.BQL.BqlString.Field<newContactFirstName> { }
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "First Name", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string NewContactFirstName { get; set; }
		#endregion
		#region NewContactLastName
		public abstract class newContactLastName : PX.Data.BQL.BqlString.Field<newContactLastName> { }
		[PXDBString(100, IsUnicode = true)]
		[PXUIField(DisplayName = "Last Name", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string NewContactLastName { get; set; }
		#endregion
		#region NewContactEmail
		public abstract class newContactEmail : PX.Data.BQL.BqlString.Field<newContactEmail> { }
		[PXUIField(DisplayName = "Email", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBString]
		[PXDBDefault(typeof(OUSearchEntity.outgoingEmail))]
		public virtual string NewContactEmail { get; set; }
		#endregion
		#region OutgoingEmail
		public abstract class outgoingEmail : PX.Data.BQL.BqlString.Field<outgoingEmail> { }

		[PXDBString(255, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = Messages.OutlookSelectEmailRecipient, Visibility = PXUIVisibility.SelectorVisible, Visible = false, Enabled = true)]
		[PXStringList()]
		public virtual String OutgoingEmail { get; set; }
		#endregion
		#region ErrorMessage
		public abstract class errorMessage : PX.Data.BQL.BqlString.Field<errorMessage> { }
		private string _ErrorMessage;

		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = " ", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual String ErrorMessage
		{
			get { return _ErrorMessage; }
			set { _ErrorMessage = value; }
		}
		#endregion
		#region Operation
		public abstract class operation : PX.Data.BQL.BqlString.Field<operation> { }

		[PXDBString(IsFixed = true)]
		[PXUIField(Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Operation { get; set; }
		#endregion

		#region ContactID
		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }
		[PXDBInt]
		[PXSelector(typeof(Search2<Contact.contactID,
			LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Contact.bAccountID>>>,
			Where<Contact.contactType, Equal<ContactTypesAttribute.person>,
				Or<Contact.contactType, Equal<ContactTypesAttribute.employee>,
					Or<Contact.contactType, Equal<ContactTypesAttribute.lead>>>>>),
			DescriptionField = typeof(Contact.displayName), Filterable = true, SelectorMode = PXSelectorMode.DisplayModeText)]
		public virtual Int32? ContactID { get; set; }
		#endregion
		#region ContactBaccountID
		public abstract class contactBaccountID : PX.Data.BQL.BqlInt.Field<contactBaccountID> { }
		[PXDBInt]
		public virtual Int32? ContactBaccountID { get; set; }
		#endregion
		#region ContactType
		public abstract class contactType : PX.Data.BQL.BqlString.Field<contactType> { }

		[PXDBString(2, IsFixed = true)]
		[ContactTypes(BqlTable = typeof(Contact))]
		[PXUIField(Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String ContactType { get; set; }
		#endregion
		#region LeadClassID
		public abstract class leadClassID : PX.Data.BQL.BqlString.Field<leadClassID> { }

		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Class ID")]
		[PXSelector(typeof(CRLeadClass.classID), DescriptionField = typeof(CRLeadClass.description), CacheGlobal = true)]
		public virtual String LeadClassID { get; set; }
		#endregion
		#region LeadSource
		public abstract class leadSource : PX.Data.BQL.BqlString.Field<leadSource> { }

		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Source")]
		[CRMSources(BqlTable = typeof(CRLead), BqlField = typeof(CRLead.source))]
		[PXFormula(typeof(Selector<OUSearchEntity.leadClassID, CRLeadClass.defaultSource>))]
		public virtual String LeadSource { get; set; }
		#endregion
		#region ContactClassID
		public abstract class contactClassID : PX.Data.BQL.BqlString.Field<contactClassID> { }

		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Class ID")]
		[PXSelector(typeof(CRContactClass.classID), DescriptionField = typeof(CRContactClass.description), CacheGlobal = true)]
		public virtual String ContactClassID { get; set; }
		#endregion
		#region ContactSource
		public abstract class contactSource : PX.Data.BQL.BqlString.Field<contactSource> { }

		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Source")]
		[CRMSources(BqlTable = typeof(Contact), BqlField = typeof(CRLead.source))]
		public virtual String ContactSource { get; set; }
		#endregion
		#region Salutation
		public abstract class salutation : PX.Data.BQL.BqlString.Field<salutation> { }
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Position", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual String Salutation { get; set; }
		#endregion
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Account")]
		[CustomerProspectVendor(DisplayName = "Account", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Int32? BAccountID { get; set; }
		#endregion
		#region FullName
		public abstract class fullName : PX.Data.BQL.BqlString.Field<fullName> { }
		private string _fullName;

		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Account Name", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXFormula(typeof(Selector<bAccountID, BAccount.acctName>))]
		public virtual String FullName
		{
			get { return _fullName; }
			set { _fullName = value; }
		}
		#endregion
		#region CountryID
		public abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }
		protected String _CountryID;
		[PXDefault(typeof(Search<GL.Branch.countryID, Where<GL.Branch.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXDBString(100)]
		[PXUIField(DisplayName = "Country")]
		[Country]
		public virtual String CountryID
		{
			get
			{
				return this._CountryID;
			}
			set
			{
				this._CountryID = value;
			}
		}
		#endregion
		#region EntityName
		public abstract class entityName : PX.Data.BQL.BqlString.Field<entityName> { }
		private string _EntityName;

		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Entity Name", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual String EntityName
		{
			get { return _EntityName; }
			set { _EntityName = value; }
		}
		#endregion
		#region EntityID
		public abstract class entityID : PX.Data.BQL.BqlString.Field<entityID> { }
		private string _EntityID;

		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Entity ID", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual String EntityID
		{
			get { return _EntityID; }
			set { _EntityID = value; }
		}
		#endregion
		#region PrevItemId
		public abstract class prevItemId : PX.Data.BQL.BqlString.Field<prevItemId> { }
		[PXString]
		public virtual string PrevItemId { get; set; }
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote]
		public virtual Guid? NoteID
		{
			get;
			set;
		}
		#endregion

		[PXDBString(IsUnicode = true)]
		[PXUIField(DisplayName = "Attachment File Names")]
		public virtual string AttachmentNames { get; set; }
		public abstract class attachmentNames : BqlString.Field<attachmentNames> { }

		[PXDBInt]
		[PXDefault(0)]
		public virtual int? AttachmentsCount { get; set; }
		public abstract class attachmentsCount : BqlInt.Field<attachmentsCount> { }

		[PXDBBool]
		[PXUIField(DisplayName = "Recognition In Progress", Visible = false)]
		public virtual bool? IsRecognitionInProgress { get; set; }
		public abstract class isRecognitionInProgress : BqlBool.Field<isRecognitionInProgress> { }

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "", Visible = false)]
		public virtual bool? RecognitionIsNotStarted { get; set; }
		public abstract class recognitionIsNotStarted : BqlBool.Field<recognitionIsNotStarted> { }

		[PXDBString(IsUnicode = true)]
		[PXUIField(DisplayName = "", Visible = false, Enabled = false)]
		public virtual string NumOfRecognizedDocuments { get; set; }
		public abstract class numOfRecognizedDocuments : BqlString.Field<numOfRecognizedDocuments> { }

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "", Visible = false, Enabled = false)]
		public virtual bool? NumOfRecognizedDocumentsCheck { get; set; }
		public abstract class numOfRecognizedDocumentsCheck : BqlBool.Field<numOfRecognizedDocumentsCheck> { }

		[PXDBString(IsUnicode = true)]
		[PXUIField(DisplayName = "", Visible = false, Enabled = false)]
		public virtual string DuplicateFilesMsg { get; set; }
		public abstract class duplicateFilesMsg : BqlString.Field<duplicateFilesMsg> { }

		[PXDefault(false)]
		[PXDBBool]
		[PXUIField(DisplayName = "", Visible = false, Enabled = false)]
		public virtual bool? IsDuplicateDelected { get; set; }
		public abstract class isDuplicateDelected : BqlBool.Field<isDuplicateDelected> { }
	}
	#endregion
	#region ExchangeMessage
	public class ExchangeMessage
	{
		public ExchangeMessage(string ewsUrl, string token)
		{
			this.EwsUrl = ewsUrl;
			this.Token = token;
		}

		public string Body { get; set; }

		public DateTime? StartDate { get; set; }
		public string Token { get; set; }
		public string EwsUrl { get; set; }
		//      public string service { get; set; }
		public AttachmentDetails[] Attachments { get; set; }
	}
	#endregion
	#region AttachmentDetails
	public class AttachmentDetails
	{
		//public string attachmentType { get; set; }
		public string ContentType { get; set; }
		public string Id { get; set; }
		//public bool isInline { get; set; }
		public string Name { get; set; }
		public string ContentId { get; set; }
	}
	#endregion
	#region OUMessage
	/// <exclude/>
	[Serializable]
	[PXHidden]
	public class OUMessage : IBqlTable
	{
		#region MessageId
		public abstract class messageId : PX.Data.BQL.BqlString.Field<messageId> { }
		[PXString]
		public virtual string MessageId { get; set; }
		#endregion

		#region ItemId
		public abstract class itemId : PX.Data.BQL.BqlString.Field<itemId> { }
		[PXString]
		public virtual string ItemId { get; set; }
		#endregion

		#region EwsUrl
		public abstract class ewsUrl : PX.Data.BQL.BqlString.Field<ewsUrl> { }
		[PXString]
		public virtual string EwsUrl { get; set; }
		#endregion

		#region Token
		public abstract class token : PX.Data.BQL.BqlString.Field<token> { }
		[PXString]
		public virtual string Token { get; set; }
		#endregion

		#region From
		public abstract class from : PX.Data.BQL.BqlString.Field<from> { }
		protected string _From;
		[PXDBString(IsUnicode = true)]
		public virtual string From
		{
			get
			{
				return this._From;
			}
			set
			{
				this._From = value;
			}
		}
		#endregion
		#region To
		public abstract class to : PX.Data.BQL.BqlString.Field<to> { }
		protected string _To;
		[PXDBString(IsUnicode = true)]
		public virtual string To
		{
			get
			{
				return this._To;
			}
			set
			{
				this._To = value;
			}
		}
		#endregion
		#region CC
		public abstract class cC : PX.Data.BQL.BqlString.Field<cC> { }
		protected string _CC;
		[PXDBString(IsUnicode = true)]
		public virtual string CC
		{
			get
			{
				return this._CC;
			}
			set
			{
				this._CC = value;
			}
		}
		#endregion
		#region Subject
		public abstract class subject : PX.Data.BQL.BqlString.Field<subject> { }
		protected string _Subject;
		[PXDBString(IsUnicode = true)]
		public virtual string Subject
		{
			get
			{
				return this._Subject;
			}
			set
			{
				this._Subject = value;
			}
		}
		#endregion
		#region IsIncome
		public abstract class isIncome : PX.Data.BQL.BqlBool.Field<isIncome> { }

		protected bool? _IsIncome;
		[PXDBBool]
		public virtual bool? IsIncome { get; set; }
		#endregion

		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		protected DateTime? _StartDate;
		[PXDateAndTime()]
		public virtual DateTime? StartDate
		{
			get
			{
				return this._StartDate;
			}
			set
			{
				this._StartDate = value;
			}
		}
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote]
		public virtual Guid? NoteID
		{
			get;
			set;
		}
		#endregion
	}
	#endregion
	#region OUCase
	/// <exclude/>
	[Serializable]
	[PXHidden]
	public class OUCase : IBqlTable
	{
		#region Subject
		public abstract class subject : PX.Data.BQL.BqlString.Field<subject> { }

		[PXDBString(255, IsUnicode = true)]
		[PXDefault(typeof(OUMessage.subject))]
		[PXUIField(DisplayName = "Subject", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Subject { get; set; }
		#endregion

		#region CaseClassID
		public abstract class caseClassID : PX.Data.BQL.BqlString.Field<caseClassID> { }

		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXDefault(typeof(Search<CRSetup.defaultCaseClassID>))]
		[PXUIField(DisplayName = "Class ID")]
		[PXSelector(typeof(CRCaseClass.caseClassID),
			DescriptionField = typeof(CRCaseClass.description),
			CacheGlobal = true)]
		public virtual String CaseClassID { get; set; }
		#endregion

		#region Source
		public abstract class source : PX.Data.BQL.BqlString.Field<source> { }

		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Source")]
		[PXDefault(CaseSourcesAttribute._EMAIL)]
		[CaseSources]
		public virtual String Source { get; set; }
		#endregion

		#region Severity
		public abstract class severity : PX.Data.BQL.BqlString.Field<severity> { }

		[PXDBString(1, IsFixed = true)]
		[PXDefault(CRCaseSeverityAttribute._MEDIUM)]
		[PXUIField(DisplayName = "Severity")]
		[CRCaseSeverity()]
		public virtual String Severity { get; set; }
		#endregion

		#region ContractID
		public abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }
		[PXDBInt]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Contract", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search2<Contract.contractID,
				LeftJoin<ContractBillingSchedule, On<Contract.contractID, Equal<ContractBillingSchedule.contractID>>>,
			Where<Contract.baseType, Equal<CTPRType.contract>,
				And<Where<Current<Contact.bAccountID>, IsNull,
						Or2<Where<Contract.customerID, Equal<Current<Contact.bAccountID>>,
							And<Current<CRCase.locationID>, IsNull>>,
						Or2<Where<ContractBillingSchedule.accountID, Equal<Current<Contact.bAccountID>>,
							And<Current<CRCase.locationID>, IsNull>>,
						Or2<Where<Contract.customerID, Equal<Current<Contact.bAccountID>>,
							And<Contract.locationID, Equal<Current<CRCase.locationID>>>>,
						Or<Where<ContractBillingSchedule.accountID, Equal<Current<Contact.bAccountID>>,
							And<ContractBillingSchedule.locationID, Equal<Current<CRCase.locationID>>>>>>>>>>>,
			OrderBy<Desc<Contract.contractCD>>>),
			DescriptionField = typeof(Contract.description),
			SubstituteKey = typeof(Contract.contractCD))]
		[PXRestrictor(typeof(Where<Contract.status, Equal<Contract.status.active>>), Messages.ContractIsNotActive)]
		[PXRestrictor(typeof(Where<Current<AccessInfo.businessDate>, LessEqual<Contract.graceDate>, Or<Contract.expireDate, IsNull>>), Messages.ContractExpired)]
		[PXRestrictor(typeof(Where<Current<AccessInfo.businessDate>, GreaterEqual<Contract.startDate>>), Messages.ContractActivationDateInFuture, typeof(Contract.startDate))]
		[PXFormula(typeof(Default<Contact.bAccountID>))]
		public virtual int? ContractID { get; set; }

		#endregion
	}
	#endregion
	#region OUOpportunity
	/// <exclude/>
	[Serializable]
	[PXHidden]
	public class OUOpportunity : IBqlTable
	{
		#region CROpportunityClassID
		public abstract class classID : PX.Data.BQL.BqlString.Field<classID> { }

		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Class ID")]
		[PXSelector(typeof(CROpportunityClass.cROpportunityClassID),
			DescriptionField = typeof(CROpportunityClass.description), CacheGlobal = true)]
		public virtual String ClassID { get; set; }
		#endregion

		#region Subject
		public abstract class subject : PX.Data.BQL.BqlString.Field<subject> { }

		[PXDBString(255, IsUnicode = true)]
		[PXDefault(typeof(OUMessage.subject))]
		[PXUIField(DisplayName = "Subject", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Subject { get; set; }
		#endregion

		#region StageID
		public abstract class stageID : PX.Data.BQL.BqlString.Field<stageID> { }

		[PXDBString(2)]
		[PXUIField(DisplayName = "Stage")]
		[CROpportunityStages(typeof(classID), OnlyActiveStages = true)]
		[PXDefault]
		public virtual String StageID { get; set; }
		#endregion

		#region CloseDate
		public abstract class closeDate : PX.Data.BQL.BqlDateTime.Field<closeDate> { }
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Est. Close Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? CloseDate { get; set; }
		#endregion

		#region CuryID
		public abstract class currencyID : PX.Data.BQL.BqlString.Field<currencyID> { }

		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXDefault(typeof(Current<AccessInfo.baseCuryID>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Currency.curyID))]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String CurrencyID { get; set; }
		#endregion

		#region Amount
		public abstract class manualAmount : PX.Data.BQL.BqlDecimal.Field<manualAmount> { }

		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBBaseCury()]
		[PXUIField(DisplayName = "Amount")]
		public virtual Decimal? ManualAmount
		{
			get; set;
		}

		#endregion

		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[Branch]
		[PXUIField(Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Branch")]
		public virtual Int32? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion
	}
	#endregion
	#region OUActivity
	/// <exclude/>
	[Serializable]
	[PXHidden]
	public class OUActivity : IBqlTable
	{
		#region Subject
		public abstract class subject : PX.Data.BQL.BqlString.Field<subject> { }
		[PXString(255, IsUnicode = true)]
		[PXDefault(typeof(OUMessage.subject))]
		[PXUIField(DisplayName = "Subject", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Subject { get; set; }

		#endregion

		#region IsLinkContact
		public abstract class isLinkContact : PX.Data.BQL.BqlBool.Field<isLinkContact> { }
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Contact")]
		public virtual Boolean? IsLinkContact
		{
			get { return Type == typeof(Contact).FullName; }
			set
			{
				if (value == true)
					Type = typeof(Contact).FullName;
			}
		}
		#endregion

		#region IsLinkCase
		public abstract class isLinkCase : PX.Data.BQL.BqlBool.Field<isLinkCase> { }
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Case")]
		public virtual Boolean? IsLinkCase
		{
			get { return Type == typeof(CRCase).FullName; }
			set
			{
				if (value == true)
					Type = typeof(CRCase).FullName;
				else if (IsLinkCase == true)
					IsLinkContact = true;
			}
		}
		#endregion

		#region IsLinkOpportunity
		public abstract class isLinkOpportunity : PX.Data.BQL.BqlBool.Field<isLinkOpportunity> { }

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Opportunity")]
		public virtual Boolean? IsLinkOpportunity
		{
			get { return Type == typeof(CROpportunity).FullName; }
			set
			{
				if (value == true)
					Type = typeof(CROpportunity).FullName;
				else if (IsLinkOpportunity == true)
					IsLinkContact = true;
			}
		}
		#endregion

		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }

		[PXString]
		[PXUIField(DisplayName = "Type", Required = true)]
		[PXEntityTypeList]
		[PXDefault]
		public virtual String Type { get; set; }
		#endregion

		#region CaseCD
		public abstract class caseCD : PX.Data.BQL.BqlString.Field<caseCD> { }
		[PXString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Entity", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<CRCase.caseCD,
			Where<CRCase.contactID, Equal<Current<OUSearchEntity.contactID>>,
				Or<CRCase.customerID, Equal<Current<OUSearchEntity.contactBaccountID>>>>,
			OrderBy<Desc<CRCase.caseCD>>>),
			typeof(CRCase.caseCD),
			typeof(CRCase.subject),
			typeof(CRCase.status),
			typeof(CRCase.priority),
			typeof(CRCase.severity),
			typeof(CRCase.caseClassID),
			typeof(BAccount.acctName),
			Filterable = true,
			DescriptionField = typeof(CRCase.subject))]
		public virtual String CaseCD { get; set; }
		#endregion

		#region OpportunityID
		public abstract class opportunityID : PX.Data.BQL.BqlString.Field<opportunityID> { }

		public const int OpportunityIDLength = 15;

		[PXString(OpportunityIDLength, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Entity", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<CROpportunity.opportunityID,
			Where<CROpportunity.contactID, Equal<Current<OUSearchEntity.contactID>>,
				Or<CROpportunity.bAccountID, Equal<Current<OUSearchEntity.contactBaccountID>>>>,
			OrderBy<Desc<CROpportunity.opportunityID>>>),
			new[] { typeof(CROpportunity.opportunityID),
				typeof(CROpportunity.subject),
				typeof(CROpportunity.status),
				typeof(CROpportunity.curyAmount),
				typeof(CROpportunity.curyID),
				typeof(CROpportunity.closeDate),
				typeof(CROpportunity.stageID),
				typeof(CROpportunity.classID)},
				Filterable = true,
				DescriptionField = typeof(CROpportunity.subject))]
		[PXFieldDescription]
		public virtual String OpportunityID { get; set; }
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote]
		public virtual Guid? NoteID
		{
			get;
			set;
		}
		#endregion
	}
	#endregion

	[PXInternalUseOnly]
	[PXHidden]
	public class OUAPBillAttachment : IBqlTable
	{
		[PXString(IsKey = true, IsUnicode = true)]
		public virtual string ItemId { get; set; }
		public abstract class itemId : BqlString.Field<itemId> { }

		[PXString(IsKey = true, IsUnicode = true)]
		public virtual string Id { get; set; }
		public abstract class id : BqlString.Field<id> { }

		[PXUIField(DisplayName = "File Name", Enabled = false)]
		[PXString(IsUnicode = true)]
        public virtual string Name { get; set; }
		public abstract class name : BqlString.Field<name> { }

		[PXString(IsUnicode = true)]
		public virtual string ContentType { get; set; }
		public abstract class contentType : BqlString.Field<contentType> { }

		[PXUIField(DisplayName = "Selected")]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXBool]
		public virtual bool? Selected { get; set; }
		public abstract class selected : BqlBool.Field<selected> { }

		[PXUIField(DisplayName = "File Hash")]
		[PXDBBinary(16, IsFixed = true)]
		public virtual byte[] FileHash { get; set; }
		public abstract class fileHash : BqlByteArray.Field<fileHash> { }

		[PXUIField(DisplayName = "File Data")]
		[PXDBBinary]
		public byte[] FileData { get; set; }
		public abstract class fileData : BqlByte.Field<fileData> { }

		[PXUIField(DisplayName = "Recognition Status")]
		[APRecognizedInvoiceRecognitionStatusList]
		[PXString(1, IsFixed = true)]
		public string RecognitionStatus { get; set; }
		public abstract class recognitionStatus : BqlString.Field<recognitionStatus> { }

		[PXUIField(DisplayName = "Duplicate Link", Visible = false)]
		[PXGuid]
		public virtual Guid? DuplicateLink { get; set; }
		public abstract class duplicateLink : BqlGuid.Field<duplicateLink> { }
	}

	public class OUSearchMaint : PXGraph<OUSearchMaint>
	{
		// ReSharper disable InconsistentNaming
		[InjectDependency]
		private ILoginUiService _loginUiService { get; set; }
		// ReSharper restore InconsistentNaming

		private const string BillsAndAdjustmentsScreenId = "AP301000";

		[InjectDependency]
		private IInvoiceRecognitionService InvoiceRecognitionClient { get; set; }

		public PXSelect<BAccount> _baccount;
		public PXSelect<Customer> _customer;

		public PXSave<OUSearchEntity> Save;
		public PXCancel<OUSearchEntity> Cancel;

		public PXFilter<OUMessage> SourceMessage;
		public PXFilter<OUSearchEntity> Filter;
		public PXFilter<OUCase> NewCase;
		public PXFilter<OUOpportunity> NewOpportunity;
		public PXFilter<OUActivity> NewActivity;

		public PXSelectOrderBy<OUAPBillAttachment, OrderBy<Asc<OUAPBillAttachment.name>>> APBillAttachments;
		public virtual IEnumerable apBillAttachments()
		{
			var outlookMessage = SourceMessage.Current;

			var isItemIdChanged = string.IsNullOrWhiteSpace(Filter.Current.PrevItemId)
				|| !string.IsNullOrWhiteSpace(outlookMessage.ItemId) &&
				!outlookMessage.ItemId.Equals(Filter.Current.PrevItemId, StringComparison.Ordinal);

			if (!isItemIdChanged)
			{
				var cached = APBillAttachments.Cache.Cached;
				foreach (OUAPBillAttachment cachedObject in cached)
				{
					if (!string.IsNullOrEmpty(cachedObject.RecognitionStatus) &&
					    cachedObject.DuplicateLink.HasValue) continue;

					var (refNbr, status) = GetFileRecognitionInfo(cachedObject.Name);
					cachedObject.RecognitionStatus = status ?? cachedObject.RecognitionStatus;
					if (cachedObject.DuplicateLink.HasValue)
						continue;
					cachedObject.DuplicateLink = GetFileDuplicateLink(refNbr, cachedObject.FileHash);
				}

				return cached;
			}
			else
			{
				APBillAttachments.Cache.Clear();
			}

			if (outlookMessage == null || string.IsNullOrWhiteSpace(outlookMessage.EwsUrl) ||
				string.IsNullOrWhiteSpace(outlookMessage.Token) || string.IsNullOrWhiteSpace(outlookMessage.ItemId) ||
				outlookMessage.Token == "none")
			{
				return Enumerable.Empty<OUAPBillAttachment>();
			}

			var exchangeMessage = TryDoAndLogIfExchangeReceiveFailed(() => GetExchangeMessage(outlookMessage));

			Filter.Current.PrevItemId = outlookMessage.ItemId;

			if (exchangeMessage == null || exchangeMessage.Attachments == null || exchangeMessage.Attachments.Length == 0)
			{
				return Enumerable.Empty<OUAPBillAttachment>();
			}

			var attachments = exchangeMessage.Attachments
				.Where(a => a.ContentType == MediaTypeNames.Application.Pdf ||
				            a.ContentType == MediaTypeNames.Application.Octet &&
				            APInvoiceRecognitionEntry.IsAllowedFile(a.Name))
				.Select(a => new OUAPBillAttachment
				{
					ItemId = outlookMessage.ItemId,
					Id = a.Id,
					Name = a.Name,
					ContentType = a.ContentType
				})
				.ToArray();

			foreach (var a in attachments)
			{
				var attachmentElement = TryDoAndLogIfExchangeReceiveFailed(() =>
						GetAttachmentsFromExchangeServerUsingEWS(outlookMessage.EwsUrl, outlookMessage.Token, a.Id)
						.FirstOrDefault());

				if (attachmentElement == null)
				{
					Filter.Current.ErrorMessage = PXLocalizer.Localize(Messages.AttachmentIsMissing, typeof(Messages).FullName);
				}
				else
				{
					a.FileData = GetFileData(attachmentElement);
					a.FileHash = APInvoiceRecognitionEntry.ComputeFileHash(a.FileData);

					var (refNbr, status) = GetFileRecognitionInfo(a.Name);
					a.RecognitionStatus = status;
					a.DuplicateLink = GetFileDuplicateLink(refNbr, a.FileHash);
				}

				APBillAttachments.Cache.Insert(a);
				APBillAttachments.Cache.SetStatus(a, PXEntryStatus.Held);
			}

			APBillAttachments.Cache.IsDirty = false;

			return attachments;
		}

		public PXSelect<Contact,
			Where<Contact.eMail, Equal<Current<OUSearchEntity.outgoingEmail>>,
			  And<Contact.isActive, Equal<True>,
			  And<Contact.contactType, NotEqual<ContactTypesAttribute.bAccountProperty>>>>,
			OrderBy<Asc<CR.Contact.contactPriority, Desc<Contact.bAccountID, Asc<Contact.contactID>>>>> DefaultContact;

		public PXSelect<Contact, Where<Contact.contactID, Equal<Current<OUSearchEntity.contactID>>>> Contact;


		public PXSelect<CRSMEmail, Where<CRSMEmail.messageId, Equal<Current<OUMessage.messageId>>>> Message;
		public PXSetup<CRSetup> setup;

		public PXSelect<CRCase> _case;
		public PXSelect<CROpportunity> _opportunity;

		[PXHidden]
		public PXSetup<CRSetup>
			Setup;

		[PXHidden]
		public PXSetup<Customer, Where<Customer.bAccountID, Equal<Optional<Contact.bAccountID>>>> customer;

		public PXAction<OUSearchEntity> LogOut;
		public PXAction<OUSearchEntity> CreateAPDoc;
		public PXAction<OUSearchEntity> CreateAPDocContinue;
		public PXAction<OUSearchEntity> ViewAPDoc;
		public PXAction<OUSearchEntity> ViewAPDocContinue;
		public PXAction<OUSearchEntity> ViewContact;
		public PXAction<OUSearchEntity> ViewBAccount;
		public PXAction<OUSearchEntity> ViewEntity;
		public PXAction<OUSearchEntity> GoCreateLead;
		public PXAction<OUSearchEntity> CreateLead;
		public PXAction<OUSearchEntity> GoCreateContact;
		public PXAction<OUSearchEntity> CreateContact;
		public PXAction<OUSearchEntity> GoCreateCase;
		public PXAction<OUSearchEntity> CreateCase;
		public PXAction<OUSearchEntity> GoCreateOpportunity;
		public PXAction<OUSearchEntity> CreateOpportunity;
		public PXAction<OUSearchEntity> GoCreateActivity;
		public PXAction<OUSearchEntity> CreateActivity;
		public PXAction<OUSearchEntity> Back;
		public PXAction<OUSearchEntity> Reply;

		#region SOAP Requests
		private const string GetAttachmentSoapRequest =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/""
xmlns:t=""http://schemas.microsoft.com/exchange/services/2006/types"">
<soap:Header>
<t:RequestServerVersion Version=""Exchange2013"" />
</soap:Header>
  <soap:Body>
    <GetAttachment xmlns=""http://schemas.microsoft.com/exchange/services/2006/messages""
    xmlns:t=""http://schemas.microsoft.com/exchange/services/2006/types"">
      <AttachmentShape/>
      <AttachmentIds>
        <t:AttachmentId Id=""{0}""/>
      </AttachmentIds>
    </GetAttachment>
  </soap:Body>
</soap:Envelope>";

		private const string GetMessageSoapRequest =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope
  xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
  xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
  xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/""
  xmlns:t=""http://schemas.microsoft.com/exchange/services/2006/types"">
<soap:Header>
<t:RequestServerVersion Version=""Exchange2007_SP1"" />
</soap:Header>
  <soap:Body>
    <GetItem
	  xmlns = ""http://schemas.microsoft.com/exchange/services/2006/messages""
	  xmlns:t=""http://schemas.microsoft.com/exchange/services/2006/types"">
      <ItemShape>
        <t:BaseShape>Default</t:BaseShape>
        <t:IncludeMimeContent>false</t:IncludeMimeContent>
		<t:BodyType>HTML</t:BodyType>
      </ItemShape>
      <ItemIds>
        <t:ItemId Id = ""{0}"" />
      </ItemIds>
    </GetItem>
  </soap:Body>
</soap:Envelope>";
		#endregion

		public OUSearchMaint()
		{
			this.Contact.Cache.AllowUpdate = false;
			this.Message.Cache.AllowUpdate = false;

			Save.SetVisible(false);
			Cancel.SetVisible(false);

		}

		#region Actions
		[PXUIField(DisplayName = "Back", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXButton]
		protected virtual IEnumerable back(PXAdapter adapter)
		{
			this.Filter.Current.ContactType = null;
			this.Filter.Current.Operation = null;
			this.Filter.Current.ErrorMessage = null;

			if (adapter.ExternalCall)
			{
				Filter.Current.DuplicateFilesMsg = null;
			}

			ClearAttachmentsSelection();

			return adapter.Get();
		}

		private void ClearAttachmentsSelection()
		{
			foreach (OUAPBillAttachment a in APBillAttachments.Cache.Cached)
			{
				a.Selected = false;
			}
		}

		[PXUIField(DisplayName = "View", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXButton]
		protected virtual void viewContact()
		{
			Contact contact = Contact.SelectSingle();
			try
			{
				if (contact != null)
					PXRedirectHelper.TryRedirect(Contact.Cache, contact, string.Empty, PXRedirectHelper.WindowMode.New);
			}
			catch (PXRedirectRequiredException ex)
			{
				ExternalRedirect(ex);
			}
		}

		[PXUIField(DisplayName = "View Account", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXButton]
		protected virtual void viewBAccount()
		{
			Contact contact = Contact.SelectSingle();
			try
			{
				if (contact != null && contact.BAccountID != null)
				{
					BAccount account = PXSelect<BAccount, Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.SelectSingleBound(this, null,
						contact.BAccountID);
					if (account != null)
						PXRedirectHelper.TryRedirect(this.Caches[typeof(BAccount)], account, string.Empty, PXRedirectHelper.WindowMode.New);
				}
			}
			catch (PXRedirectRequiredException ex)
			{
				ExternalRedirect(ex);
			}
		}

		[PXUIField(DisplayName = "View Entity", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXButton]
		protected virtual void viewEntity()
		{
			CRSMEmail message = Message.SelectSingle();
			if (message != null)
			{
				try
				{
					EntityHelper helper = new EntityHelper(this);
					helper.NavigateToRow(message.RefNoteID, PXRedirectHelper.WindowMode.New);
				}
				catch (PXRedirectRequiredException ex)
				{
					ExternalRedirect(ex);
				}
			}
		}

		[PXUIField(DisplayName = "Create Lead", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Visible = false, FieldClass = FeaturesSet.customerModule.FieldClass)]
		[PXButton]
		protected virtual void goCreateLead()
		{
			Filter.Current.Operation = OUOperation.CreateLead;
			Filter.Current.ContactType = ContactTypesAttribute.Lead;
			Filter.Current.LeadClassID = setup.Current.DefaultLeadClassID;
			Filter.Cache.SetDefaultExt<OUSearchEntity.leadSource>(this.Filter.Current);
		}

		[PXUIField(DisplayName = "Create Contact", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXButton]
		protected virtual void goCreateContact()
		{
			Filter.Current.Operation = OUOperation.CreateContact;
			Filter.Current.ContactType = ContactTypesAttribute.Person;
			Filter.Current.ContactClassID =
				PXAccess.FeatureInstalled<FeaturesSet.customerModule>() ? setup.Current.DefaultContactClassID :
																			string.Empty;
		}

		[PXUIField(DisplayName = "Create Case", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Visible = false, FieldClass = FeaturesSet.caseManagement.FieldClass)]
		[PXButton]
		protected virtual void goCreateCase()
		{
			this.Filter.Current.Operation = OUOperation.Case;
			Contact.Current = Contact.SelectSingle();

			this.NewCase.Cache.SetDefaultExt<OUCase.subject>(this.NewCase.Current);
		}

		[PXUIField(DisplayName = "Create Opportunity", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Visible = false, FieldClass = FeaturesSet.customerModule.FieldClass)]
		[PXButton]
		protected virtual void goCreateOpportunity()
		{
			this.Filter.Current.Operation = OUOperation.Opportunity;
			Contact.Current = Contact.SelectSingle();

			this.NewOpportunity.Cache.SetDefaultExt<OUOpportunity.subject>(this.NewOpportunity.Current);
			this.NewOpportunity.Cache.SetDefaultExt<OUOpportunity.classID>(this.NewOpportunity.Current);
		}

		[PXUIField(DisplayName = "Log Activity", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXButton]
		protected virtual void goCreateActivity()
		{
			this.Filter.Current.Operation = OUOperation.Activity;
			this.NewActivity.Cache.SetDefaultExt<OUActivity.subject>(this.NewActivity.Current);
			this.NewActivity.Current.IsLinkContact = true;
			this.Contact.View.Clear();
		}

		[PXUIField(DisplayName = "Create AP Document", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		protected virtual void createAPDoc()
		{
			Filter.Current.Operation = OUOperation.CreateAPDocument;
			Filter.Current.DuplicateFilesMsg = null;

			if (Filter.Current.AttachmentsCount == 1)
			{
				CreateAPDocContinue.PressImpl();
			}
		}

		[PXUIField(DisplayName = "View Document", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		protected virtual void viewAPDoc()
		{
			Filter.Current.Operation = OUOperation.ViewAPDocument;
			Filter.Current.DuplicateFilesMsg = null;

			if (Filter.Current.AttachmentsCount == 1)
			{
				ViewAPDocContinue.PressImpl();
				Back.PressImpl();
			}
		}

		[PXUIField(DisplayName = "Continue")]
		[PXButton]
		protected virtual IEnumerable createAPDocContinue(PXAdapter adapter)
		{
			Filter.Current.ErrorMessage = null;
			Filter.Current.RecognitionIsNotStarted = true;

			var message = SourceMessage.Current;
			var selectedAttachments = GetSelectedAttachments();

			try
			{
				var duplicateCount = 0;
				var filesToRecognize = new List<(string fileName, byte[] fileData, Guid fileId)>();

				foreach (var attachment in selectedAttachments)
				{
					if (attachment.DuplicateLink != null)
					{
						duplicateCount++;

						attachment.RecognitionStatus = RecognizedRecordStatusListAttribute.Recognized;
						APBillAttachments.Cache.Update(attachment);
						APBillAttachments.Cache.SetStatus(attachment, PXEntryStatus.Held);
					}
					else
					{
						filesToRecognize.Add((attachment.Name, attachment.FileData, Guid.NewGuid()));
						Filter.Current.RecognitionIsNotStarted = false;
					}
				}

				if (duplicateCount == 1)
				{
					Filter.Current.IsDuplicateDelected = true;
					Filter.Current.DuplicateFilesMsg = PXMessages.LocalizeNoPrefix(Messages.SingleDuplicateDocument);
				}
				else if (duplicateCount > 1)
				{
					Filter.Current.IsDuplicateDelected = true;
					Filter.Current.DuplicateFilesMsg = PXMessages.LocalizeNoPrefix(Messages.MultipleDuplicateDocuments);
				}

				ClearAttachmentsSelection();

				if (filesToRecognize.Count > 0)
				{
					LongOperationManager.StartAsyncOperation(cancellationToken =>
					{
						var batch = filesToRecognize.Select(f => new RecognizedRecordFileInfo(Guid.NewGuid() + "\\" + f.fileName, f.fileData, f.fileId));
						return APInvoiceRecognitionEntry.RecognizeRecordsBatch(batch, message.Subject, message.From, message.MessageId, newFiles: true, externalCancellationToken: cancellationToken);
					});
				}
			}
			catch (PXException e)
			{
				Filter.Current.ErrorMessage = e.MessageNoPrefix;
			}

			Back.PressImpl();

			return adapter.Get();
		}

		[PXUIField(DisplayName = "Continue")]
		[PXButton]
		protected virtual void viewAPDocContinue()
		{
			Filter.Current.ErrorMessage = null;

			if (!APBillAttachments.Cache.Cached.Any_())
			{
				Filter.Current.PrevItemId = null;
			}

			var selectedAttachments = GetSelectedAttachments();

			try
			{
				foreach (var attachment in selectedAttachments)
				{
					NavigateToAPDocument(attachment.FileHash, attachment.DuplicateLink);
				}
			}
			catch (PXBaseRedirectException)
			{
				throw;
			}
			catch (PXException e)
			{
				Filter.Current.ErrorMessage = e.MessageNoPrefix;
			}
		}

        private IEnumerable<OUAPBillAttachment> GetSelectedAttachments()
		{
			var selectedAttachments = Enumerable.Empty<OUAPBillAttachment>();
			var attachments = APBillAttachments
				.Select()
				.AsEnumerable()
				.Select(a => (OUAPBillAttachment)a);

			if (Filter.Current.AttachmentsCount == 1)
			{
				selectedAttachments = attachments.Take(1);
			}
			else
			{
				selectedAttachments = attachments.Where(a => a.Selected == true);
			}

			return selectedAttachments;
		}

		private (Guid? RefNbr, string Status) GetFileRecognitionInfo(string fileName)
		{
			if (string.IsNullOrWhiteSpace(fileName))
			{
				return (null, null);
			}

			var subject = SourceMessage.Current.Subject;
			if (string.IsNullOrWhiteSpace(subject))
			{
				return (null, null);
			}

			var messageId = SourceMessage.Current.MessageId;
			if (string.IsNullOrWhiteSpace(messageId))
			{
				return (null, null);
			}

			messageId = APInvoiceRecognitionEntry.NormalizeMessageId(messageId);
			subject = APInvoiceRecognitionEntry.GetRecognizedSubject(subject, fileName);

			using (var record = PXDatabase.SelectSingle<RecognizedRecord>(
				new PXDataField<RecognizedRecord.refNbr>(),
				new PXDataField<RecognizedRecord.status>(),
				new PXDataFieldValue<RecognizedRecord.messageID>(messageId),
				new PXDataFieldValue<RecognizedRecord.subject>(subject)))
			{
				if (record == null)
				{
					return (null, null);
				}

				return (record.GetGuid(0), record.GetString(1));
			}
		}

		private Guid? GetFileDuplicateLink(Guid? refNbr, byte[] fileHash)
		{
			var selecParams = new List<object> { fileHash };
			BqlCommand duplicateRecordSelect = new SelectFrom<RecognizedRecord>
				.Where<RecognizedRecord.fileHash.IsEqual<@P.AsByteArray>.And<
					   RecognizedRecord.duplicateLink.IsNull>>();

			if (refNbr != null)
			{
				duplicateRecordSelect = duplicateRecordSelect.WhereAnd<Where<RecognizedRecord.refNbr.IsNotEqual<@P.AsGuid>>>();
				selecParams.Add(refNbr);
			}

			var selectView = new PXView(this, true, duplicateRecordSelect);
			var duplicateRecord = (RecognizedRecord) selectView.SelectSingle(selecParams.ToArray());

			return duplicateRecord?.RefNbr;
		}

        private void NavigateToAPDocument(byte[] fileHash, Guid? duplicateLink)
		{
			var normalizedMessageId = APInvoiceRecognitionEntry.NormalizeMessageId(SourceMessage.Current.MessageId);
			var recognizedRecord = duplicateLink == null ? (RecognizedRecord)
				SelectFrom<RecognizedRecord>
				.Where<RecognizedRecord.messageID.IsEqual<@P.AsString>.And<
					   RecognizedRecord.fileHash.IsEqual<@P.AsByteArray>>>
				.View.ReadOnly.Select(this, normalizedMessageId, fileHash) :
				SelectFrom<RecognizedRecord>
				.Where<RecognizedRecord.refNbr.IsEqual<@P.AsGuid>>
				.View.ReadOnly.Select(this, duplicateLink);

			if (recognizedRecord == null)
			{
				return;
			}

			var graph = CreateInstance<APInvoiceRecognitionEntry>();
			var urlParamsForRedirect = new StringBuilder();
			var refNbrParam = HttpUtility.UrlEncode(recognizedRecord.RefNbr.ToString());

			urlParamsForRedirect.Append($"&{APInvoiceRecognitionEntry.RefNbrNavigationParam}={refNbrParam}");

			try
			{
				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.New);
			}
			catch (PXRedirectRequiredException e)
			{
				ExternalRedirect(e, append: urlParamsForRedirect.ToString());
			}
		}

		[PXUIField(DisplayName = "Create Lead", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Visible = false, FieldClass = FeaturesSet.customerModule.FieldClass)]
		[PXButton]
		protected virtual void createLead()
		{
			if (this.Filter.VerifyRequired())
			{
				LeadMaint graph = PXGraph.CreateInstance<LeadMaint>();
				CRLead lead = graph.Lead.Insert();
				graph.Lead.Search<CRLead.contactID>(lead.ContactID);
				lead.FirstName = this.Filter.Current.NewContactFirstName;
				lead.LastName = this.Filter.Current.NewContactLastName;
				lead.EMail = this.Filter.Current.NewContactEmail;
				lead.BAccountID = this.Filter.Current.BAccountID;
				lead.DisplayName = this.Filter.Current.NewContactDisplayName;
				lead.Salutation = this.Filter.Current.Salutation;
				lead.FullName = this.Filter.Current.FullName;
				lead.Source = this.Filter.Current.LeadSource;

				Address address = graph.AddressCurrent.View.SelectSingle() as Address;
				if (address != null)
				{
					address.CountryID = this.Filter.Current.CountryID;
					graph.AddressCurrent.Cache.Update(address);
				}

				graph.Lead.Update(lead);

				graph.Save.PressImpl(false, true);
				this.Filter.Current.ContactType = null;
				this.Filter.SetValueExt<OUSearchEntity.contactID>(this.Filter.Current, lead.ContactID);
				this.Contact.View.Clear();
			}
		}

		[PXUIField(DisplayName = "Create Contact", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXButton]
		protected virtual void createContact()
		{
			if (this.Filter.VerifyRequired())
			{
				ContactMaint graph = PXGraph.CreateInstance<ContactMaint>();

				Contact contact = graph.Contact.Insert();

				graph.Contact.Search<Contact.contactID>(contact.ContactID);
				contact.FirstName = this.Filter.Current.NewContactFirstName;
				contact.LastName = this.Filter.Current.NewContactLastName;
				contact.EMail = this.Filter.Current.NewContactEmail;
				contact.BAccountID = this.Filter.Current.BAccountID;
				contact.DisplayName = this.Filter.Current.NewContactDisplayName;
				contact.Salutation = this.Filter.Current.Salutation;
				contact.FullName = this.Filter.Current.FullName;
				contact.Source = this.Filter.Current.ContactSource;

				Address address = graph.AddressCurrent.View.SelectSingle() as Address;
				if (address != null)
				{
					address.CountryID = this.Filter.Current.CountryID;
					graph.AddressCurrent.Cache.Update(address);
				}

				graph.Contact.Update(contact);

				graph.Save.PressImpl(false, true);
				this.Filter.Current.ContactType = null;
				this.Filter.SetValueExt<OUSearchEntity.contactID>(this.Filter.Current, contact.ContactID);
				this.Contact.View.Clear();
			}
		}

		[PXUIField(DisplayName = "Create Case", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Visible = false, FieldClass = FeaturesSet.caseManagement.FieldClass)]
		[PXButton]
		protected virtual void createCase()
		{
			try
			{
				this.Filter.Current.ErrorMessage = null;
				CRCaseMaint graph = PXGraph.CreateInstance<CRCaseMaint>();
				CRCase newCase = graph.Case.Insert();
				graph.Case.Search<CRCase.caseCD>(newCase.CaseCD);
				foreach (var field in this.NewCase.Cache.Fields)
				{
					graph.Case.Cache.SetValue(newCase, field, this.NewCase.Cache.GetValue(this.NewCase.Current, field));
				}
				Contact contact = Contact.SelectSingle();

				newCase.CustomerID = contact.BAccountID;
				newCase.ContactID = contact.ContactID;
				graph.Case.Update(newCase);
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					BAccount baccount = PXSelectorAttribute.Select<CRCase.customerID>(graph.Case.Cache, newCase) as BAccount;
					if (PersistMessageDefault(PXNoteAttribute.GetNoteID<CRCase.noteID>(graph.Case.Cache, newCase),
							baccount?.BAccountID,
							newCase.ContactID))
					{
						graph.Save.PressImpl(false, true);
					}
					ts.Complete();
				}
				this.Filter.Current.Operation = null;

			}
			catch (PXFieldValueProcessingException e)
			{
				this.Filter.Current.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
			}
			catch (PXOuterException e)
			{
				this.Filter.Current.ErrorMessage = e.InnerMessages[0];
			}
			catch (Exception e)
			{
				this.Filter.Current.ErrorMessage = e.Message;
			}
		}

		[PXUIField(DisplayName = "Create Opportunity", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Visible = false, FieldClass = FeaturesSet.customerModule.FieldClass)]
		[PXButton]
		protected virtual void createOpportunity()
		{
			try
			{
				this.Filter.Current.ErrorMessage = null;
				OpportunityMaint graph = PXGraph.CreateInstance<OpportunityMaint>();
				CROpportunity newOpportunity = graph.Opportunity.Insert();
				graph.Opportunity.Search<CROpportunity.opportunityID>(newOpportunity.OpportunityID);
				foreach (var field in this.NewOpportunity.Cache.Fields)
				{
					graph.Opportunity.Cache.SetValue(newOpportunity, field,
						this.NewOpportunity.Cache.GetValue(this.NewOpportunity.Current, field));
				}
				Contact contact = Contact.SelectSingle();

				newOpportunity.BAccountID = contact.BAccountID;
				newOpportunity.ContactID = contact.ContactID;
				graph.Opportunity.Update(newOpportunity);
				newOpportunity.CuryID = this.NewOpportunity.Current.CurrencyID;
				graph.Opportunity.Update(newOpportunity);
				if (this.NewOpportunity.Current.ManualAmount > 0)
				{
					newOpportunity.ManualTotalEntry = true;
					newOpportunity.CuryAmount = this.NewOpportunity.Current.ManualAmount;
				}
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					BAccount baccount =
						PXSelectorAttribute.Select<CROpportunity.bAccountID>(graph.Opportunity.Cache, newOpportunity) as BAccount;
					if (PersistMessageDefault(PXNoteAttribute.GetNoteID<CROpportunity.noteID>(graph.Opportunity.Cache, newOpportunity),
						baccount?.BAccountID,
						newOpportunity.ContactID))
					{
						this.SetIgnorePersistFields(graph, typeof(CRContact), null);
						this.SetIgnorePersistFields(graph, typeof(CROpportunity), this.NewOpportunity.Cache.Fields);

						graph.Save.PressImpl(false, true);
					}
					ts.Complete();
				}
				this.Filter.Current.Operation = null;
			}
			catch (PXFieldValueProcessingException e)
			{
				this.Filter.Current.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
			}
			catch (PXOuterException e)
			{
				this.Filter.Current.ErrorMessage = e.InnerMessages[0];
			}
			catch (Exception e)
			{
				this.Filter.Current.ErrorMessage = e.Message;
			}
		}

		[PXUIField(DisplayName = "Sign Out", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXButton]
		protected virtual void logOut()
		{
			if (HttpContext.Current != null)
			{
				_loginUiService.LogoutCurrentUser();
				PXDatabase.Delete<UserIdentity>(
							   new PXDataFieldRestrict<UserIdentity.providerName>(PXDbType.VarChar, "ExchangeIdentityToken"),
							   new PXDataFieldRestrict<UserIdentity.userID>(PXDbType.UniqueIdentifier, PXAccess.GetUserID())
							   );

				throw new PXRedirectToUrlException("../../Frames/Outlook/FirstRun.html", String.Empty);
			}
		}

		[PXUIField(DisplayName = "Create", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXButton]
		protected virtual void createActivity()
		{
			if (!this.NewActivity.VerifyRequired()) return;

			try
			{
				this.SelectTimeStamp();
				this.SourceMessage.Current.Subject = this.NewActivity.Current.Subject;

				var a = this.NewActivity.Current;
				Guid? noteID = null;
				int? contactID = null;
				int? bAccountID = null;

				if (a.IsLinkOpportunity == true)
				{
					PXResult<CROpportunity, BAccount> rec = (PXResult<CROpportunity, BAccount>)PXSelectJoin<CROpportunity,
							LeftJoin<BAccount, On<CROpportunity.bAccountID, Equal<BAccount.bAccountID>>>,
						Where<CROpportunity.opportunityID, Equal<Required<CROpportunity.opportunityID>>>>
						.SelectSingleBound(this, null, a.OpportunityID);
					if (rec != null)
					{
						noteID = PXNoteAttribute.GetNoteID<CROpportunity.noteID>(_opportunity.Cache, (CROpportunity)rec);
						bAccountID = ((BAccount)rec).BAccountID;
						contactID = ((CROpportunity)rec).ContactID;
					}
				}
				else if (a.IsLinkCase == true)
				{
					PXResult<CRCase, BAccount> rec = (PXResult<CRCase, BAccount>)PXSelectJoin<CRCase,
						LeftJoin<BAccount, On<CRCase.customerID, Equal<BAccount.bAccountID>>>,
						Where<CRCase.caseCD, Equal<Required<CRCase.caseCD>>>>
						.SelectSingleBound(this, null, a.CaseCD);
					if (rec != null)
					{
						noteID = PXNoteAttribute.GetNoteID<CRCase.noteID>(_case.Cache, (CRCase)rec);
						bAccountID = ((BAccount)rec).BAccountID;
						contactID = ((CRCase)rec).ContactID;
					}
				}
				else
				{
					PXResult<Contact, BAccount> acc =
						(PXResult<Contact, BAccount>)PXSelectJoin<Contact,
							LeftJoin<BAccount, On<BAccount.bAccountID, Equal<CR.Contact.bAccountID>>>,
							Where<CR.Contact.contactID, Equal<Required<CR.Contact.contactID>>,
							And<CR.Contact.isActive, Equal<True>,
							And<CR.Contact.contactType, NotEqual<ContactTypesAttribute.bAccountProperty>>>>,
							OrderBy<Asc<CR.Contact.contactPriority, Desc<Contact.bAccountID, Asc<Contact.contactID>>>>>
						.SelectSingleBound(this, null, this.Filter.Current.ContactID);
					if (acc != null)
					{
						if(((Contact)acc).ContactType == ContactTypesAttribute.Employee)
						{
							BAccount employee = PXSelectReadonly<BAccount,
									   Where<BAccount.bAccountID, Equal<Required<Contact.contactID>>>>
									   .SelectSingleBound(this, null, ((Contact)acc).ContactID);
							noteID = employee.NoteID;
							bAccountID = ((Contact)acc).ContactID;
							contactID = null;
						}
						else
					{
						noteID = PXNoteAttribute.GetNoteID<Contact.noteID>(Contact.Cache, (Contact)acc);
						bAccountID = ((BAccount)acc).BAccountID;
						contactID = ((Contact)acc).ContactID;
						}
					}
				}
				this.Caches[typeof(Note)].ClearQueryCacheObsolete();
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					PersistMessageDefault(noteID, bAccountID, contactID);
					this.Save.Press();
					ts.Complete();
				}
				this.Filter.Current.Operation = null;
			}
			catch (PXFieldValueProcessingException e)
			{
				this.Filter.Current.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
			}
			catch (PXOuterException e)
			{
				this.Filter.Current.ErrorMessage = e.InnerMessages[0];
			}
			catch (Exception e)
			{
				this.Filter.Current.ErrorMessage = e.Message;
			}
		}

		[PXUIField(DisplayName = "Reply", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXButton]
		protected virtual void reply()
		{
			CRSMEmail message = this.Message.SelectSingle();
			if (message != null)
			{
				try
				{

					PXRedirectHelper.TryRedirect(Message.Cache, message, string.Empty, PXRedirectHelper.WindowMode.New);
				}
				catch (PXRedirectRequiredException ex)
				{
					ExternalRedirect(ex, true, "&Run=ReplyInline");
				}
			}
		}
		#endregion

		protected virtual void _(Events.FieldSelecting<OUSearchEntity.isRecognitionInProgress> e)
		{
			var messageId = SourceMessage.Current.MessageId;

			e.ReturnValue = string.IsNullOrWhiteSpace(messageId) ?
				false :
				APInvoiceRecognitionEntry.IsRecognitionInProgress(messageId);
		}

		protected virtual void _(Events.RowSelected<OUSearchEntity> e)
		{
			OUSearchEntity row = e.Row;
			var isIncome = this.SourceMessage.Current.IsIncome == true;
			int recipientCount = 0;

			if (this.SourceMessage.Current.IsIncome != true && this.SourceMessage.Current.To != null)
			{
				var items = this.SourceMessage.Current.To.Replace("\" <", "\"|<").Split(';');
				var labels = items.Take(items.Length - 1).Select(x => x.Segment('|', 0).Replace("\"", String.Empty).Replace("'", String.Empty)).ToArray();
				var values = items.Take(items.Length - 1).Select(x => x.Segment('|', 1).Replace("\"", String.Empty).Replace("'", String.Empty).Replace("<", String.Empty).Replace(">", String.Empty)).ToArray();
				PXStringListAttribute.SetList<OUSearchEntity.outgoingEmail>(e.Cache, row, values, labels);
				row.OutgoingEmail = row.OutgoingEmail ?? values.FirstOrDefault();
				recipientCount = values.Count();
			}

			if (this.SourceMessage.Current.IsIncome == false && !String.IsNullOrEmpty(row.OutgoingEmail))
			{
				var displayName = this.GetDisplayName(row.OutgoingEmail);
				string firstName, lastLame;
				ParseDisplayName(displayName, out firstName, out lastLame);
				row.EMail = row.OutgoingEmail;
				row.NewContactEmail = row.OutgoingEmail;
				row.DisplayName = displayName;
				row.NewContactFirstName = firstName;
				row.NewContactLastName = lastLame;
			}

			Contact contact = Contact.SelectSingle();
			CRSMEmail message = String.IsNullOrEmpty(SourceMessage.Current.MessageId) ? null : Message.SelectSingle();

			if (row.ContactType == null && row.Operation == null)
				row.ErrorMessage = String.Empty;

			row.ContactID = contact.With(_ => _.ContactID);
			row.ContactBaccountID = contact.With(_ => _.BAccountID);

			if (row.ContactType == null)
			{
				row.Salutation = contact.With(_ => _.Salutation);
				row.FullName = contact.With(_ => _.FullName);
			}

			var isCreateContactOrLeadOrNoOperation = row.Operation == OUOperation.CreateLead || row.Operation == OUOperation.CreateContact || row.Operation == null;

			if (contact != null)
			{
				string view = EntityHelper.GetFieldString(Contact.Cache.GetStateExt<Contact.contactType>(contact) as PXFieldState);
				ViewContact.SetCaption(PXMessages.LocalizeNoPrefix(Messages.View) + ' ' + PXMessages.LocalizeNoPrefix(view));

				var defaultContact = DefaultContact.SelectSingle();
				if (defaultContact != null && contact.BAccountID != defaultContact.BAccountID)
				{
					row.ErrorMessage = PXMessages.LocalizeNoPrefix(Messages.DifferentBAccountID);
				}
			}
			else if (row.ContactType != null)
			{
				if (
					!isCreateContactOrLeadOrNoOperation
					&& PXSelect<CRCaseClass, Where<CRCaseClass.requireCustomer, Equal<True>>>.SelectSingleBound(this, null).Count > 0
				)
				{
					row.ErrorMessage = PXMessages.LocalizeNoPrefix(Messages.SomeCaseRequireCustomer);
					if (row.BAccountID != null)
					{
						var b = PXSelect<BAccount, Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>, And<Where<BAccount.type, Equal<BAccountType.customerType>,
							Or<BAccount.type, Equal<BAccountType.combinedType>>>>>>.SelectSingleBound(this, null, row.BAccountID);
						if (b.Count > 0)
							row.ErrorMessage = String.Empty;
					}
				}
			}

			var isContactIDVisible = row.Operation != OUOperation.CreateAPDocument && row.Operation != OUOperation.ViewAPDocument;
			PXUIFieldAttribute.SetVisible<OUSearchEntity.contactID>(e.Cache, row, isContactIDVisible);

			PXUIFieldAttribute.SetVisible<OUSearchEntity.errorMessage>(e.Cache, row, !string.IsNullOrWhiteSpace(row.ErrorMessage));

			var isOutgoingEmailVisible = !isIncome && row.OutgoingEmail != null && recipientCount > 1 && row.Operation != OUOperation.CreateAPDocument &&
				row.Operation != OUOperation.ViewAPDocument;
			PXUIFieldAttribute.SetVisible<OUSearchEntity.outgoingEmail>(e.Cache, row, isOutgoingEmailVisible);

			var customerModuleOn = PXAccess.FeatureInstalled<FeaturesSet.customerModule>();
			var caseModuleOn = PXAccess.FeatureInstalled<FeaturesSet.caseManagement>();

			PXUIFieldAttribute.SetVisible(this.NewCase.Cache, this.NewCase.Current, null, row.Operation == OUOperation.Case);
			PXUIFieldAttribute.SetVisible(this.NewOpportunity.Cache, this.NewOpportunity.Current, null, customerModuleOn && (row.Operation == OUOperation.Opportunity));
			PXUIFieldAttribute.SetVisible(this.NewActivity.Cache, this.NewActivity.Current, null, row.Operation == OUOperation.Activity);

			if (row.Operation == OUOperation.Activity)
			{
				var opportunityAny = (CROpportunity)PXSelect<CROpportunity, Where<CROpportunity.contactID, Equal<Current<OUSearchEntity.contactID>>,
					Or<CROpportunity.bAccountID, Equal<Current<OUSearchEntity.contactBaccountID>>>>>.SelectSingleBound(this, null);
				PXUIFieldAttribute.SetEnabled<OUActivity.isLinkOpportunity>(this.NewActivity.Cache, this.NewActivity.Current, opportunityAny != null);

				var caseAny = (CRCase)PXSelect<CRCase, Where<CRCase.contactID, Equal<Current<OUSearchEntity.contactID>>,
					Or<CRCase.customerID, Equal<Current<OUSearchEntity.contactBaccountID>>>>>.SelectSingleBound(this, null);
				PXUIFieldAttribute.SetEnabled<OUActivity.isLinkCase>(this.NewActivity.Cache, this.NewActivity.Current, caseAny != null);

				PXUIFieldAttribute.SetEnabled<OUActivity.caseCD>(this.NewActivity.Cache, this.NewActivity.Current, this.NewActivity.Current.IsLinkCase == true);
				PXUIFieldAttribute.SetEnabled<OUActivity.opportunityID>(this.NewActivity.Cache, this.NewActivity.Current, this.NewActivity.Current.IsLinkOpportunity == true);
			}
			PXUIFieldAttribute.SetVisible<OUActivity.isLinkCase>(this.NewActivity.Cache, this.NewActivity.Current, row.Operation == OUOperation.Activity && caseModuleOn);
			PXUIFieldAttribute.SetVisible<OUActivity.caseCD>(this.NewActivity.Cache, this.NewActivity.Current, row.Operation == OUOperation.Activity && this.NewActivity.Current.IsLinkOpportunity != true && caseModuleOn);
			PXUIFieldAttribute.SetVisible<OUActivity.isLinkOpportunity>(this.NewActivity.Cache, this.NewActivity.Current, row.Operation == OUOperation.Activity && customerModuleOn);
			PXUIFieldAttribute.SetVisible<OUActivity.opportunityID>(this.NewActivity.Cache, this.NewActivity.Current, row.Operation == OUOperation.Activity && this.NewActivity.Current.IsLinkOpportunity == true && customerModuleOn);
			PXDefaultAttribute.SetPersistingCheck<OUActivity.caseCD>(this.NewActivity.Cache, this.NewActivity.Cache.Current,
			  row.Operation == OUOperation.Activity && this.NewActivity.Current.IsLinkCase == true
					? PXPersistingCheck.NullOrBlank
					: PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<OUActivity.opportunityID>(this.NewActivity.Cache, this.NewActivity.Cache.Current,
				row.Operation == OUOperation.Activity && this.NewActivity.Current.IsLinkOpportunity == true
					? PXPersistingCheck.NullOrBlank
					: PXPersistingCheck.Nothing);

			bool haveAccessRights = false;
			if (message != null)
			{
				EntityHelper helper = new EntityHelper(this);
				if (message.RefNoteID != null)
				{
					var entityrow = helper.GetEntityRow(message.RefNoteID);
					if(entityrow != null)
					{
						var primaryGraphForEntity = helper.GetPrimaryGraphType(entityrow, true);
						if (primaryGraphForEntity != null)
						{
							haveAccessRights = IsRights(
								primaryGraphForEntity,
								helper.GetEntityRowType(message.RefNoteID),
								PXCacheRights.Select);
						}
					}
				}
				string name = helper.GetFriendlyEntityName(message.RefNoteID) ?? Messages.Entity;
				ViewEntity.SetCaption(PXMessages.LocalizeNoPrefix(Messages.View) + ' ' + PXMessages.LocalizeNoPrefix(name));
				row.EntityName = (name ?? Messages.Entity) + ':';
				row.EntityID = helper.GetEntityDescription(message.RefNoteID, message.GetType());
				if (row.EntityID == null)
					message = null;
			}

			PXUIFieldAttribute.SetVisible<OUSearchEntity.salutation>(e.Cache, row, isCreateContactOrLeadOrNoOperation && (contact != null || row.ContactType != null));
			PXUIFieldAttribute.SetVisible<OUSearchEntity.fullName>(e.Cache, row, isCreateContactOrLeadOrNoOperation && (contact != null || row.ContactType != null));
			PXUIFieldAttribute.SetVisible<OUSearchEntity.entityName>(e.Cache, row, isCreateContactOrLeadOrNoOperation && contact != null && message != null);
			PXUIFieldAttribute.SetVisible<OUSearchEntity.entityID>(e.Cache, row, isCreateContactOrLeadOrNoOperation && contact != null && message != null);

			//New contact/lead
			PXUIFieldAttribute.SetVisible<OUSearchEntity.newContactFirstName>(e.Cache, row, row.ContactType != null);
			PXUIFieldAttribute.SetVisible<OUSearchEntity.newContactLastName>(e.Cache, row, row.ContactType != null);
			PXUIFieldAttribute.SetVisible<OUSearchEntity.newContactEmail>(e.Cache, row, row.ContactType != null);

			PXUIFieldAttribute.SetVisible<OUSearchEntity.bAccountID>(e.Cache, row, row.ContactType != null);
			PXUIFieldAttribute.SetVisible<OUSearchEntity.leadSource>(e.Cache, row, row.ContactType == ContactTypesAttribute.Lead);
			PXUIFieldAttribute.SetVisible<OUSearchEntity.contactSource>(e.Cache, row, row.ContactType == ContactTypesAttribute.Person);
			PXUIFieldAttribute.SetVisible<OUSearchEntity.countryID>(e.Cache, row, row.ContactType != null);
			PXUIFieldAttribute.SetEnabled<OUSearchEntity.salutation>(e.Cache, row, row.ContactType != null);
			PXUIFieldAttribute.SetEnabled<OUSearchEntity.fullName>(e.Cache, row, row.ContactType != null && row.BAccountID == null);
			PXUIFieldAttribute.SetEnabled<OUSearchEntity.leadSource>(e.Cache, row, row.ContactType == ContactTypesAttribute.Lead);
			PXUIFieldAttribute.SetEnabled<OUSearchEntity.contactSource>(e.Cache, row, row.ContactType == ContactTypesAttribute.Person);
			PXUIFieldAttribute.SetEnabled<OUSearchEntity.countryID>(e.Cache, row, row.ContactType != null);

			var contactGraph = contact != null && contact.ContactType == ContactTypesAttribute.Employee
				? typeof(EmployeeMaint) : typeof(ContactMaint);
			ViewContact.SetVisible(contact != null && isCreateContactOrLeadOrNoOperation && IsRights(contactGraph, typeof(Contact), PXCacheRights.Select));
			ViewBAccount.SetVisible(contact != null && isCreateContactOrLeadOrNoOperation && contact.ContactType != ContactTypesAttribute.Employee && contact.BAccountID != null && IsRights(typeof(BusinessAccountMaint), typeof(BAccount), PXCacheRights.Select));
			ViewEntity.SetVisible(
					isIncome
					&& contact != null
					&& isCreateContactOrLeadOrNoOperation
					&& message != null
					&& message.RefNoteID != contact.NoteID
					&& (IsRights(typeof(CRCaseMaint), typeof(CRCase), PXCacheRights.Select) || haveAccessRights)
				);

			var createContactIsVisible = contact == null && row.OutgoingEmail != null && row.ContactType == null && isCreateContactOrLeadOrNoOperation;
			GoCreateLead.SetVisible(createContactIsVisible && IsRights(typeof(LeadMaint), typeof(CRLead), PXCacheRights.Insert));
			GoCreateContact.SetVisible(createContactIsVisible && IsRights(typeof(ContactMaint), typeof(Contact), PXCacheRights.Insert));

			GoCreateCase.SetVisible(isIncome && contact != null && contact.ContactType == ContactTypesAttribute.Person && SourceMessage.Current.MessageId != null && isCreateContactOrLeadOrNoOperation
									&& IsRights(typeof(CRCaseMaint), typeof(CRCase), PXCacheRights.Insert));
			GoCreateOpportunity.SetVisible(isIncome && (contact != null && contact.ContactType != ContactTypesAttribute.Employee
				&& SourceMessage.Current.MessageId != null && isCreateContactOrLeadOrNoOperation
									&& IsRights(typeof(OpportunityMaint), typeof(CROpportunity), PXCacheRights.Insert)));
			GoCreateActivity.SetVisible(contact != null && row.ContactType == null && isCreateContactOrLeadOrNoOperation && IsRights(typeof(EP.CRActivityMaint), typeof(CRActivity), PXCacheRights.Insert));
			Back.SetVisible(row.ContactType != null || row.Operation != null);
			Reply.SetVisible(isIncome && (message != null && isCreateContactOrLeadOrNoOperation && row.ContactType == null && IsRights(typeof(CREmailActivityMaint), typeof(CRSMEmail), PXCacheRights.Insert)));

			CreateLead.SetVisible(row.ContactType == ContactTypesAttribute.Lead);
			CreateContact.SetVisible(row.ContactType == ContactTypesAttribute.Person);
			CreateCase.SetVisible(row.Operation == OUOperation.Case);
			CreateOpportunity.SetVisible(row.Operation == OUOperation.Opportunity);
			CreateActivity.SetVisible(row.Operation == OUOperation.Activity);

			var logoutVisible = row.Operation != OUOperation.CreateAPDocument && row.Operation != OUOperation.ViewAPDocument;
			LogOut.SetVisible(logoutVisible);

			SetAPDocumentButtonsState(row, e.Cache);
		}

		private void SetAPDocumentButtonsState(OUSearchEntity row, PXCache cache)
		{
			var isBillAdjScreenAvailable = PXSiteMap.Provider.FindSiteMapNodeByScreenID(BillsAndAdjustmentsScreenId) != null;
			var isNoOperationAndContact = row.Operation == null && row.ContactType == null;
			var isRecognitionActive = isNoOperationAndContact && isBillAdjScreenAvailable &&
				PXAccess.FeatureInstalled<FeaturesSet.apDocumentRecognition>() && InvoiceRecognitionClient.IsConfigured();
			var attachmentStatusesList = DeserializeAttachmentNames(row)
				.Select(n => new
				{
					GetFileRecognitionInfo(n).Status
				})
				.ToList();

			if (row.ContactType != null || row.Operation != null && row.Operation != OUOperation.CreateAPDocument &&
				row.Operation != OUOperation.ViewAPDocument)
			{
				row.DuplicateFilesMsg = null;
			}

			PXUIFieldAttribute.SetVisible<OUSearchEntity.duplicateFilesMsg>(cache, row, !string.IsNullOrEmpty(row.DuplicateFilesMsg));

			var isDuplicateDetected = row.IsDuplicateDelected == true;
			var processedAttachmentCount = attachmentStatusesList
				.Count(a => !string.IsNullOrEmpty(a.Status) &&
						  a.Status != RecognizedRecordStatusListAttribute.InProgress &&
						  a.Status != RecognizedRecordStatusListAttribute.PendingRecognition);
			var isAnyProcessedAttachment = processedAttachmentCount > 0;
			var isViewAPDocVisible = isRecognitionActive && (isAnyProcessedAttachment || isDuplicateDetected);
			ViewAPDoc.SetVisible(isViewAPDocVisible);

			var attachmentsList = APBillAttachments
				.Select()
				.Select(a => (OUAPBillAttachment)a)
				.ToList();
			var isAnyAttachmentSelected = attachmentsList.Any(a => a.Selected == true);
			var isCreateAPDocContinueVisible = row.Operation == OUOperation.CreateAPDocument && row.IsRecognitionInProgress != true &&
				isAnyAttachmentSelected;
			CreateAPDocContinue.SetVisible(isCreateAPDocContinueVisible);

			var isViewAPDocContinueVisible = row.Operation == OUOperation.ViewAPDocument && isAnyAttachmentSelected;
			ViewAPDocContinue.SetVisible(isViewAPDocContinueVisible);

			var allAttachmentsProcessed = attachmentsList.All(a => !string.IsNullOrEmpty(a.RecognitionStatus));
			var isAnyUnprocessedAttachment = attachmentStatusesList.Any(a => string.IsNullOrEmpty(a.Status));
			var isCreateAPDocVisible = isRecognitionActive && isAnyUnprocessedAttachment &&
				(attachmentsList.Count == 0 || !isDuplicateDetected || !allAttachmentsProcessed);
			CreateAPDoc.SetVisible(isCreateAPDocVisible);

			var isNumOfRecognizedDocumentsVisible = isRecognitionActive && isAnyProcessedAttachment;
			PXUIFieldAttribute.SetVisible<OUSearchEntity.numOfRecognizedDocumentsCheck>(cache, row, isNumOfRecognizedDocumentsVisible);
			PXUIFieldAttribute.SetVisible<OUSearchEntity.numOfRecognizedDocuments>(cache, row, isNumOfRecognizedDocumentsVisible);
			if (processedAttachmentCount == 1)
			{
				row.NumOfRecognizedDocuments = PXMessages.LocalizeNoPrefix(Messages.SingleDocumentRecognized);
			}
			else if (processedAttachmentCount > 1)
			{
				row.NumOfRecognizedDocuments = PXMessages.LocalizeFormatNoPrefixNLA(Messages.MultipleDocumentsRecognized, processedAttachmentCount);
			}
			else
			{
				row.NumOfRecognizedDocuments = null;
			}
		}

		private static IEnumerable<string> DeserializeAttachmentNames(OUSearchEntity row)
		{
			var names = row?.AttachmentNames?.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

			if (names == null || names.Length == 0)
			{
				return Enumerable.Empty<string>();
			}

			return names;
		}

		protected virtual void OUSearchEntity_OutgoingEmail_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			this.Filter.Current.Operation = null;
			this.Filter.Cache.SetValueExt<OUSearchEntity.newContactEmail>(this.Filter.Current, this.Filter.Current.OutgoingEmail);
			this.Filter.Cache.SetValueExt<OUSearchEntity.contactID>(this.Filter.Current, null);
		}

		protected virtual void OUSearchEntity_ContactID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			Contact contact = DefaultContact.SelectSingle();
			e.NewValue = contact.With(_ => _.ContactID);
			e.Cancel = true;
		}
		protected virtual void OUSearchEntity_ContactID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as OUSearchEntity;
			if (row == null) return;

			if (row.ContactID == null)
			{
				sender.SetValueExt<OUSearchEntity.eMail>(row, row.OutgoingEmail);
				object newContactID;
				sender.RaiseFieldDefaulting<OUSearchEntity.contactID>(row, out newContactID);
			}
			else
			{
				Contact contact = Contact.SelectSingle();
				row.EMail = contact.EMail;
			}
		}
		protected virtual void OUSearchEntity_ContactBaccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			Contact contact = DefaultContact.SelectSingle();
			e.NewValue = contact.With(_ => _.BAccountID);
			e.Cancel = true;
		}

		protected virtual void OUCase_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			OUCase row = e.Row as OUCase;
			if (row == null) return;

			if (row.CaseClassID == null)
				return;

			if (PXSelect<CRCaseClass, Where<CRCaseClass.requireContract, Equal<True>, And<CRCaseClass.caseClassID, Equal<Current<OUCase.caseClassID>>>>>.SelectSingleBound(this, null).Count == 0)
				PXUIFieldAttribute.SetVisible<OUCase.contractID>(sender, row, false);
			else
			{
				var tmp = PXSelectorAttribute.SelectFirst<OUCase.contractID>(sender, e.Row);
				if (tmp == null) return;
				var contract1 = PXResult.Unwrap<Contract>(tmp).ContractID;

				tmp = PXSelectorAttribute.SelectLast<OUCase.contractID>(sender, e.Row);
				if (tmp == null) return;
				var contract2 = PXResult.Unwrap<Contract>(tmp).ContractID;

				if (contract1 != null && contract1 == contract2)
				{
					NewCase.SetValueExt<OUCase.contractID>(row, contract1);
				}
			}
		}

		private OUAPBillAttachment FindAttachmentByKeys(string itemId, string id)
		{
			return APBillAttachments
				.Select()
				.AsEnumerable()
				.Select(a => (OUAPBillAttachment)a)
				.Where(a => a.ItemId == itemId && a.Id == id)
				.FirstOrDefault();
		}

		public virtual void OUAPBillAttachmentSelectFileFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e, string itemId, string id)
		{
			var attachmentRow = FindAttachmentByKeys(itemId, id);
			if (attachmentRow == null)
			{
				return;
			}

			if (attachmentRow.DuplicateLink == null)
			{
				attachmentRow.RecognitionStatus = GetFileRecognitionInfo(attachmentRow.Name).Status;
			}

			object returnState = attachmentRow.Selected;
			var enabled = false;
			var operation = Filter.Current.Operation;
			var error = default(string);
			var errorLevel = PXErrorLevel.Undefined;

			if (operation == OUOperation.CreateAPDocument)
			{
				enabled = string.IsNullOrEmpty(attachmentRow.RecognitionStatus);

				if (!enabled)
				{
					error = PXMessages.LocalizeNoPrefix(Messages.FileAlreadyRecognized);
					errorLevel = PXErrorLevel.RowInfo;
				}
			}
			else if (operation == OUOperation.ViewAPDocument)
			{
				enabled = (!string.IsNullOrEmpty(attachmentRow.RecognitionStatus) && attachmentRow.RecognitionStatus != RecognizedRecordStatusListAttribute.InProgress);

				if (!enabled)
				{
					error = PXMessages.LocalizeNoPrefix(Messages.FileNotRecognizedYet);
					errorLevel = PXErrorLevel.RowInfo;
				}
			}

			APBillAttachments.Cache.RaiseFieldSelecting<OUAPBillAttachment.selected>(attachmentRow, ref returnState, true);
			PXFieldState returnStateTyped = (PXFieldState)returnState;
			var stateWithDisplayName = PXFieldState.CreateInstance(returnStateTyped, returnStateTyped.DataType, displayName: attachmentRow.Name, visible: true,
				enabled: enabled,
				error: error,
				errorLevel: errorLevel);
			e.ReturnState = stateWithDisplayName;
		}

		protected virtual void _(Events.RowSelected<OUAPBillAttachment> e)
		{
			if (!(e.Row is OUAPBillAttachment attachmentRow))
				return;

			var operation = Filter.Current.Operation;
			var enabled = false;
			switch (operation)
			{
				case OUOperation.CreateAPDocument:
					enabled = string.IsNullOrEmpty(attachmentRow.RecognitionStatus);
					break;
				case OUOperation.ViewAPDocument:
					enabled = (!string.IsNullOrEmpty(attachmentRow.RecognitionStatus) && attachmentRow.RecognitionStatus != RecognizedRecordStatusListAttribute.InProgress);
					break;
			}

			PXUIFieldAttribute.SetEnabled<OUAPBillAttachment.selected>(e.Cache, attachmentRow, enabled);
		}

		public virtual void _(Events.FieldUpdating<OUAPBillAttachment, OUAPBillAttachment.selected> e)
		{
			var attachmentRow = e.Row;
			if (attachmentRow == null)
			{
				return;
			}

			bool? result = PXBoolAttribute.ConvertValue(e.NewValue) as bool?;

			if (result != true)
			{
				return;
			}

			if (Filter.Current.Operation == OUOperation.CreateAPDocument)
			{
				return;
			}

			var otherAttachments = APBillAttachments
				.Select()
				.AsEnumerable()
				.Select(a => (OUAPBillAttachment)a)
				.Where(a => !attachmentRow.Equals(a));
			foreach (var o in otherAttachments)
			{
				APBillAttachments.Cache.SetValue<OUAPBillAttachment.selected>(o, false);
			}
		}

		public virtual void OUAPBillAttachmentSelectFileFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e, string itemId, string id)
		{
			var attachmentRow = FindAttachmentByKeys(itemId, id);
			if (attachmentRow == null)
			{
				return;
			}

			bool? result = PXBoolAttribute.ConvertValue(e.NewValue) as bool?;

			APBillAttachments.Cache.SetValue<OUAPBillAttachment.selected>(attachmentRow, result);

			if (result != true)
			{
				return;
			}

            if (Filter.Current.Operation == OUOperation.CreateAPDocument)
            {
				return;
			}

            var otherAttachments = APBillAttachments
				.Select()
				.AsEnumerable()
				.Select(a => (OUAPBillAttachment)a)
				.Where(a => !attachmentRow.Equals(a));
			foreach (var o in otherAttachments)
			{
				APBillAttachments.Cache.SetValue<OUAPBillAttachment.selected>(o, false);
			}
		}

		public bool IsRights(Type graphType, Type cacheType, PXCacheRights checkRights)
		{
			PXCacheRights rights;
			List<string> invisible = null;
			List<string> disabled = null;
			var node = PXSiteMap.Provider.FindSiteMapNode(graphType);
			if (node == null)
				return false;

			PXAccess.GetRights(node.ScreenID, graphType.Name, cacheType, out rights, out invisible, out disabled);
			return rights >= checkRights;
		}

		public string GetDisplayName(string email)
		{
            if (this.SourceMessage.Current.To == null)
            {
                return string.Empty;
            }
			var item = this.SourceMessage.Current.To.Replace("\" <", "\"|<").Split(';').FirstOrDefault(x => x.Contains(email));
			if (item == null)
				return String.Empty;
			return item.FirstSegment('|').Replace("\"", String.Empty).Replace("'", String.Empty);
		}

		private void SetIgnorePersistFields(PXGraph graph, Type cacheType, PXFieldCollection ignoredFields)
		{
			foreach (var item in graph.Caches[cacheType].Cached)
			{
				foreach (var field in graph.Caches[cacheType].Fields.Where(x => ignoredFields == null || !ignoredFields.Contains(x)))
				{
					if (graph.Caches[cacheType].GetValue(item, field) == null)
						PXDefaultAttribute.SetPersistingCheck(graph.Caches[cacheType], field, item, PXPersistingCheck.Nothing);
				}
			}
		}

		protected bool PersistMessageDefault(Guid? refNoteID, int? bAccountID, int? contactID)
		{
			var result = TryDoAndLogIfExchangeReceiveFailed(() =>
			{
				SaveCRSMEmail(SourceMessage.Current,
					new DefaultCRSMEmailPreparer(refNoteID, bAccountID, contactID, Filter.Current.OutgoingEmail));
				return true;
			});
			Message.View.Clear();
			Message.Cache.Clear();
			return result;
		}

		protected virtual void SaveCRSMEmail(OUMessage message, CRSMEmailPreparer emailPreparer = null)
		{
			if (message is null)
				throw new ArgumentNullException(nameof(message));

			if (message.MessageId is null)
				throw new ArgumentException("message.MessageId is null.", nameof(message));

			CREmailActivityMaint graph = PXGraph.CreateInstance<CREmailActivityMaint>();

			CRSMEmail email = SelectFrom<CRSMEmail>
					.Where<CRSMEmail.messageId.IsEqual<P.AsString>>
					.View
					.Select(graph, message.MessageId);

			if (email is null)
			{
				email = CreateNewCRSMEmail(message, graph);
				emailPreparer?.PrepareNewEmail(email);
			}
			else
			{
				emailPreparer?.PrepareExistingEmail(email);
			}

			email.Subject = string.IsNullOrEmpty(message.Subject)
				? PXMessages.LocalizeNoPrefix(Messages.NoSubject)
				: message.Subject;

			graph.Message.Update(email);
			graph.Save.PressImpl(false, true);
		}

		protected virtual CRSMEmail CreateNewCRSMEmail(OUMessage message, CREmailActivityMaint graph)
		{
			ExchangeMessage exchangeMessage;
			exchangeMessage = GetExchangeMessage(message);

			var imageExtractor = new ImageExtractor();
			CRSMEmail email = graph.Message.Insert();
			List<FileDto> files = null;

			if (email.MailAccountID == null)
			{
				throw new ExchangeReceiveEmailFailedException(Messages.OutlookNoDefaultEmailAccount);
			}

			email.MessageId = message.MessageId;
			email.MailTo = message.To;
			email.MailReply = null;
			email.MailCc = message.CC;
			email.IsIncome = message.IsIncome;
			email.MPStatus = MailStatusListAttribute.Processed;
			email.StartDate = exchangeMessage.StartDate;
			PXNoteAttribute.GetNoteID<CRSMEmail.noteID>(graph.Message.Cache, email);
			if (exchangeMessage.Attachments != null)
			{
				files = new List<FileDto>();
				PX.SM.UploadFileMaintenance upload = PXGraph.CreateInstance<PX.SM.UploadFileMaintenance>();
				foreach (var attachDetail in exchangeMessage.Attachments)
				{
					XElement attachment;
					attachment = GetAttachmentsFromExchangeServerUsingEWS(
						message.EwsUrl, message.Token, attachDetail).FirstOrDefault();


					if (attachment == null)
						continue;

					if (!TryParseAttachment(email, attachment, out var fileDto))
						continue;

					var extension = Path.GetExtension(fileDto.Name).ToLower();
					if (!upload.IgnoreFileRestrictions && !upload.AllowedFileTypes.Contains(extension))
						continue;

					PXBlobStorage.SaveContext = new PXBlobStorageContext
					{
						ViewName = graph.Message.Name,
						Graph = graph,
						DataRow = email,
						NoteID = email.NoteID,
					};
					using (Disposable.Create(() => PXBlobStorage.SaveContext = null))
						graph.InsertFile(fileDto);
					files.Add(fileDto);
				}
			}

			string newBody;
			if (files == null)
				imageExtractor.Extract(exchangeMessage.Body, out newBody, out var __);
			else
				imageExtractor.Extract(exchangeMessage.Body, out newBody, out var __,
					(img) => { return (ImageExtractor.PREFIX_IMG_BY_FILEID + img.ID, img.Name); },
					(item) =>
					{
						var img = files.Find(i => string.Equals(i.ContentId, item.cid, StringComparison.OrdinalIgnoreCase));
						if (img == null)
							return null;
						return new ImageExtractor.ImageInfo(img.FileId, img.Content, img.FullName, img.ContentId);
					});
			email.Body = newBody;
			return email;
		}

		[Obsolete("Don't use this method directly from application code.")]
		public virtual bool PersisMessage(Guid? refNoteID, int? bAccountID, int? contactID)
		{
			return PersistMessageDefault(refNoteID, bAccountID, contactID);
		}

		public virtual void ExternalRedirect(PXRedirectRequiredException ex, bool popup = false, string append = null)
		{
			System.Text.StringBuilder bld = new System.Text.StringBuilder();
			string externalLink = null;


			String company = PXAccess.GetCompanyName();
			PXSiteMapNode node = PXSiteMap.Provider.FindSiteMapNode(ex.Graph.GetType());
			if (node == null || node.ScreenID == null || HttpContext.Current == null ||
				HttpContext.Current.Request == null)
			{
				return;
			}


			externalLink = HttpContext.Current.Request.GetWebsiteUrl().TrimEnd('/');
			externalLink += PX.Common.PXUrl.ToAbsoluteUrl(popup ? node.Url : PXUrl.MainPagePath);

			bld.Append("?"); // may be we should create an event and set this in application code?
			if (!String.IsNullOrEmpty(company))
			{
				bld.Append("CompanyID=" + HttpUtility.UrlEncode(company));
				bld.Append("&");
			}
			bld.Append("ScreenId=" + node.ScreenID);

			PXGraph graph = ex.Graph;
			PXView view = graph.Views[graph.PrimaryView];
			object dataItem = view.Cache.Current;


			List<string> pars = new List<string>();
			foreach (string key in view.Cache.Keys)
			{
				object v = view.Cache.GetValue(dataItem, key);
				if (v != null)
					pars.Add(key + "=" + HttpUtility.UrlEncode(v.ToString()));
			}
			if (pars.Count > 0)
			{
				bld.Append("&");
				bld.Append(string.Join("&", pars.ToArray()));
			}
			externalLink += bld.ToString();
			if (append != null)
				externalLink += append;
			throw new PXRedirectToUrlException(externalLink, PXBaseRedirectException.WindowMode.New, string.Empty);
		}

		public static void ParseDisplayName(string displayName, out string firstName, out string lastName)
		{
			firstName = null;
			lastName = null;

			displayName = displayName.Trim();
			while (displayName.IndexOf("  ") > -1)
				displayName = displayName.Replace("  ", " ");

			string[] name = displayName.Split(' ');
			firstName = name.Length > 1 ? name[0] : null;
			lastName = name.Length > 1 ? name[name.Length - 1] : name[0];
		}

		#region Work with Exchange
		private static IEnumerable<XElement> GetAttachmentsFromExchangeServerUsingEWS(string ewsUrl, string token, AttachmentDetails detail)
		{
			if (detail == null)
			{
				return null;
			}

			return GetAttachmentsFromExchangeServerUsingEWS(ewsUrl, token, detail.Id);
		}

		private static IEnumerable<XElement> GetAttachmentsFromExchangeServerUsingEWS(string ewsUrl, string token, string attachmentDetailId)
		{
			if (ewsUrl == null || token == null)
			{
				return null;
			}

			var responseEnvelope = SendRequestToExchange(ewsUrl, token, string.Format(GetAttachmentSoapRequest, attachmentDetailId));
			if (responseEnvelope == null)
			{
				return null;
			}

			return GetAttachments(responseEnvelope);
		}

		private static ExchangeMessage GetExchangeMessage(OUMessage ouMessage)
		{
			var responseEnvelope = SendRequestToExchange(ouMessage.EwsUrl, ouMessage.Token, string.Format(GetMessageSoapRequest, ouMessage.ItemId));

            if (responseEnvelope == null)
            {
                return null;
            }

			var errorCodes = from errorCode in responseEnvelope.Descendants
							 ("{http://schemas.microsoft.com/exchange/services/2006/messages}ResponseCode")
							 select errorCode;
			// Return the first error code found.
			foreach (var errorCode in errorCodes)
			{
				if (errorCode.Value != "NoError")
				{
					throw new ExchangeReceiveEmailFailedException(MessagesNoPrefix.ErrorOccurred, errorCode.Value);
				}
			}

			var message = new ExchangeMessage(ouMessage.EwsUrl, ouMessage.Token);

			var items = from item in responseEnvelope.Descendants
							  ("{http://schemas.microsoft.com/exchange/services/2006/types}Message")
						select item;
			foreach (var item in items)
			{
				//var mimeContent = item.Element("{http://schemas.microsoft.com/exchange/services/2006/types}MimeContent");
				message.Body = item.Element("{http://schemas.microsoft.com/exchange/services/2006/types}Body").Value;
				var dateTimeCreated = item.Element("{http://schemas.microsoft.com/exchange/services/2006/types}DateTimeCreated").Value;
				message.StartDate = PXTimeZoneInfo.ConvertTimeFromUtc(
										DateTime.Parse(dateTimeCreated).ToUniversalTime()
										, LocaleInfo.GetTimeZone());
				//var dateTimeSent = item.Element("{http://schemas.microsoft.com/exchange/services/2006/types}DateTimeSent").Value;
				//var toRecipients = from toRecipient in item.Descendants("{http://schemas.microsoft.com/exchange/services/2006/types}ToRecipients")
				//				   select toRecipient;
				//var ccRecipients = from ccRecipient in item.Descendants("{http://schemas.microsoft.com/exchange/services/2006/types}CcRecipients")
				//				   select ccRecipient;
				break;
			}



			var fileAttachments = from fileAttachment in responseEnvelope.Descendants
							  ("{http://schemas.microsoft.com/exchange/services/2006/types}FileAttachment")
								  select fileAttachment;
			if (fileAttachments == null || fileAttachments.Count() == 0)
				return message;

			var attachments = new List<AttachmentDetails>();
			foreach (var fileAttachment in fileAttachments)
			{
				var attache = new AttachmentDetails();
				attache.Id = fileAttachment.Element("{http://schemas.microsoft.com/exchange/services/2006/types}AttachmentId").Attribute("Id").Value;
				var name = fileAttachment.Element("{http://schemas.microsoft.com/exchange/services/2006/types}Name");
				attache.Name = name == null ? null : name.Value;
				var contentType = fileAttachment.Element("{http://schemas.microsoft.com/exchange/services/2006/types}ContentType");
				attache.ContentType = contentType == null ? null : contentType.Value;
				var contentId = fileAttachment.Element("{http://schemas.microsoft.com/exchange/services/2006/types}ContentId");
				attache.ContentId = contentId == null ? null : contentId.Value;
				attachments.Add(attache);
			}
			message.Attachments = attachments.ToArray();
			return message;
		}

		private static IEnumerable<XElement> GetAttachments(XElement responseEnvelope)
		{
			// First, check the response for web service errors.
			var errorCodes = from errorCode in responseEnvelope.Descendants
							 ("{http://schemas.microsoft.com/exchange/services/2006/messages}ResponseCode")
							 select errorCode;
			// Return the first error code found.
			foreach (var errorCode in errorCodes)
			{
				if (errorCode.Value != "NoError")
				{
					return null;
				}
			}

			var fileAttachments = from fileAttachment in responseEnvelope.Descendants
							  ("{http://schemas.microsoft.com/exchange/services/2006/types}FileAttachment")
								  select fileAttachment;
			return fileAttachments;
		}

		private static byte[] GetFileData(XElement attachment)
		{
			var fileContent = attachment.Element("{http://schemas.microsoft.com/exchange/services/2006/types}Content");

			return Convert.FromBase64String(fileContent.Value);
		}

		private static bool TryParseAttachment(CRSMEmail msg, XElement attachment, out FileDto file)
		{
			var fileData = GetFileData(attachment);
			if (fileData != null)
			{
				var fileName = attachment.Element("{http://schemas.microsoft.com/exchange/services/2006/types}Name");
				var contentId = attachment.Element("{http://schemas.microsoft.com/exchange/services/2006/types}ContentId");
				file = new FileDto(msg.NoteID.Value, fileName.Value, fileData, contentId: contentId?.Value);
				return true;
			}
			file = null;
			return false;
		}

		private static void ThrowOnNullOrWhiteSpace(string value, string paramName)
		{
			if (value == null)
				throw new ArgumentNullException(paramName);

			if (value == string.Empty)
				throw new ArgumentException(paramName);
		}

		private static XElement SendRequestToExchange(string url, string token, string soapRequest)
		{
			ThrowOnNullOrWhiteSpace(url, nameof(url));
			ThrowOnNullOrWhiteSpace(token, nameof(token));
			ThrowOnNullOrWhiteSpace(soapRequest, nameof(soapRequest));		

			try
            {
	            HttpWebRequest webRequest = WebRequest.CreateHttp(url);
                webRequest.Headers.Add("Authorization",
                  string.Format("Bearer {0}", token));
                webRequest.PreAuthenticate = true;
                webRequest.UseDefaultCredentials = true;
                webRequest.AllowAutoRedirect = false;
                webRequest.Method = "POST";
                webRequest.ContentType = "text/xml; charset=utf-8";

                byte[] bodyBytes = Encoding.UTF8.GetBytes(soapRequest);
                webRequest.ContentLength = bodyBytes.Length;

                using (Stream requestStream = webRequest.GetRequestStream())
                {
                    requestStream.Write(bodyBytes, 0, bodyBytes.Length);
                }

                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    if (webResponse.StatusCode == HttpStatusCode.OK)
                    {
                        using (var responseStream = webResponse.GetResponseStream())
                        {
                            return XElement.Load(responseStream);
                        }
                    }
                    else
                    {
                        throw new ExchangeReceiveEmailFailedException(Messages.ErrorOccurred, webResponse.StatusDescription);
                    }
                }
            }
            catch (WebException e) 
            {
                return null;
            }
            // catch (WebException ex)
            // {
            //     using (WebResponse response = ex.Response)
            //     {
            //         HttpWebResponse httpResponse = (HttpWebResponse)response;
            //         using (Stream data = response.GetResponseStream())
            //         {
            //             using (StreamReader reader = new StreamReader(data))
            //             {
            //                 string text = reader.ReadToEnd();
            //                 throw PXException.PreserveStack(new PXException(PXMessages.LocalizeFormatNoPrefix(Messages.SystemCannotExecuteWebRequest) + Environment.NewLine + text));
            //             }
            //         }
            //     }
            // }
		}
		#endregion

		protected T TryDoAndLogIfExchangeReceiveFailed<T>(Func<T> action)
		{
			try
			{
				return action();
			}
			catch (ExchangeReceiveEmailFailedException e)
			{
				this.Filter.Current.ErrorMessage = e.Message;
				return default;
			}
		}



		[Serializable]
		public class ExchangeReceiveEmailFailedException : PXException
		{
			public ExchangeReceiveEmailFailedException() { }
			public ExchangeReceiveEmailFailedException(string message) : base(message) { }
			public ExchangeReceiveEmailFailedException(string format, params object[] args) : base(format, args) { }
			public ExchangeReceiveEmailFailedException(string message, Exception innerException) : base(message, innerException) { }
			public ExchangeReceiveEmailFailedException(Exception innerException, string format, params object[] args) : base(innerException, format, args) { }
			public ExchangeReceiveEmailFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
		}
		public abstract class CRSMEmailPreparer
		{
			public abstract void PrepareNewEmail(CRSMEmail email);
			public abstract void PrepareExistingEmail(CRSMEmail email);
		}

		public class DefaultCRSMEmailPreparer : CRSMEmailPreparer
		{
			private readonly Guid? _refNoteID;
			private readonly int? _bAccountID;
			private readonly int? _contactID;
			private readonly string _outgoingEmail;

			public DefaultCRSMEmailPreparer(Guid? refNoteID, int? bAccountID, int? contactID, string outgoingEmail)
			{
				_refNoteID = refNoteID;
				_bAccountID = bAccountID;
				_contactID = contactID;
				_outgoingEmail = outgoingEmail;
			}

			public override void PrepareExistingEmail(CRSMEmail email)
			{
				email.RefNoteID = _refNoteID;
				email.BAccountID = _bAccountID;
				email.ContactID = _contactID;
			}

			public override void PrepareNewEmail(CRSMEmail email)
			{
				email.MailFrom = _outgoingEmail;
				email.RefNoteID = _refNoteID;
				email.BAccountID = _bAccountID;
				email.ContactID = _contactID;
			}
		}

		#region Customer Management Extensions
		public sealed class CustomerModuleOUOpportunityExtension : PXCacheExtension<OUOpportunity>
		{
			public static bool IsActive()
			{
				return PXAccess.FeatureInstalled<FeaturesSet.customerModule>();
			}

			#region CROpportunityClassID
			public abstract class classID : PX.Data.BQL.BqlString.Field<classID> { }

			[PXDefault(
				typeof(Coalesce<
					Search<CRContactClass.targetOpportunityClassID, Where<CRContactClass.classID, Equal<Current<Contact.classID>>>>,
					Search<CRSetup.defaultOpportunityClassID>>),
				PersistingCheck = PXPersistingCheck.Nothing)]
			[PXMergeAttributes( Method = MergeMethod.Merge)]
			public String ClassID { get; set; }
			#endregion
		}

		public class CustomerModuleOUSearchMaintExtension : PXGraphExtension<OUSearchMaint>
		{
			public static bool IsActive()
			{
				return PXAccess.FeatureInstalled<FeaturesSet.customerModule>();
			}

			protected virtual void OUOpportunity_CurrencyID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
			{
				if (Base.customer.Current != null && !string.IsNullOrEmpty(Base.customer.Current.CuryID))
				{
					e.NewValue = Base.customer.Current.CuryID;
					e.Cancel = true;
				}
			}
		}
		#endregion
	}
}
