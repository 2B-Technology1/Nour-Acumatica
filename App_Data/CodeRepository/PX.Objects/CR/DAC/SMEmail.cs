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
using PX.Data.EP;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.SM;
using PX.TM;
using PX.Web.UI;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.SM.Email;

namespace PX.Objects.CR
{
	/// <exclude/>
	[Serializable]
	[CRSMEmailPrimaryGraphAttribute]
	[PXCacheName(Messages.SystemEmail)]
	public partial class SMEmail : IBqlTable 
	{
		#region Keys
		public class PK : PrimaryKeyOf<SMEmail>.By<noteID>
		{
			public static SMEmail Find(PXGraph graph, Guid? noteID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, noteID, options);
		}
		public static class FK
		{
			public class EmailAccount : PX.SM.EMailAccount.PK.ForeignKeyOf<SMEmail>.By<mailAccountID> { }
			public class CRActivity : PX.Objects.CR.CRActivity.PK.ForeignKeyOf<SMEmail>.By<refNoteID> { }
		}
		#endregion

		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected { get; set; }
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXDBGuid(true, IsKey = true)]
		public virtual Guid? NoteID { get; set; }
		#endregion
		
		#region RefNoteID
		public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }

		[PXSequentialSelfRefNote(SuppressActivitiesCount = true, NoteField = typeof(noteID))]
		[PXDBDefault(null, PersistingCheck = PXPersistingCheck.Nothing, DefaultForUpdate = false)]
		[PXParent(typeof(Select<CRActivity, Where<CRActivity.noteID, Equal<Current<refNoteID>>>>), ParentCreate = true)]
		[PXReferentialIntegrityCheck]
		public virtual Guid? RefNoteID { get; set; }
		#endregion

		#region ResponseToNoteID
		public new abstract class responseToNoteID : PX.Data.BQL.BqlGuid.Field<responseToNoteID> { }

		/// <summary>
		/// Email <see cref="SMEmail.NoteID"/> in response to which a new incoming (or outgoing) email was created
		/// </summary>
		[PXUIField(DisplayName = "In Response To")]
		[PXDBGuid]
		public virtual Guid? ResponseToNoteID { get; set; }
		#endregion

		#region Subject
		public abstract class subject : PX.Data.BQL.BqlString.Field<subject> { }

		[PXDBString(998, InputMask = "", IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Summary", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		[PXNavigateSelector(typeof(subject))]
		public virtual string Subject { get; set; }
		#endregion
		
		#region Body
		public abstract class body : PX.Data.BQL.BqlString.Field<body> { }

		[PXDBText(IsUnicode = true)]
		[PXUIField(DisplayName = "Activity Details")]
		public virtual string Body { get; set; }
		#endregion

		#region MPStatus
		public abstract class mpstatus : PX.Data.BQL.BqlString.Field<mpstatus> { }

		[PXDBString(2, IsFixed = true, IsUnicode = false)]
		[MailStatusList]
		[PXDefault(ActivityStatusAttribute.Draft)]
		[PXUIField(DisplayName = "Mail Status", Enabled = false)]
		public virtual string MPStatus { get; set; }
		#endregion

		#region IsArchived
		public abstract class isArchived : PX.Data.BQL.BqlBool.Field<isArchived> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = EP.Messages.EmailArchived)]
		public virtual bool? IsArchived { get; set; }
		#endregion

		#region ImcUID
		public abstract class imcUID : PX.Data.BQL.BqlGuid.Field<imcUID> { }

		[PXDBGuid(true)]
		[PXUIField(Visible = false, Enabled = false)]
		public virtual Guid? ImcUID
		{
			get { return _ImcUID ?? (_ImcUID = Guid.NewGuid()); }
			set { _ImcUID = value; }
		}
		protected Guid? _ImcUID;
		#endregion

		#region Pop3UID
		public abstract class pop3UID : PX.Data.BQL.BqlString.Field<pop3UID> { }

		[PXDBString(150)]
		[PXUIField(Visible = false, Enabled = false)]
		public virtual string Pop3UID { get; set; }
		#endregion

		#region ImapUID
		public abstract class imapUID : PX.Data.BQL.BqlInt.Field<imapUID> { }

		[PXDBInt]
		[PXUIField(Visible = false, Enabled = false)]
		public virtual int? ImapUID { get; set; }
		#endregion

		#region MailAccountID
		public abstract class mailAccountID : PX.Data.BQL.BqlInt.Field<mailAccountID> { }

		[PXDBInt]
		[PXUIField(DisplayName = "From")]
		public int? MailAccountID { get; set; }

		#endregion

		#region MailFrom
		public abstract class mailFrom : PX.Data.BQL.BqlString.Field<mailFrom> { }

		[PXDBString(500, IsUnicode = true)]
		[PXUIField(DisplayName = "From")]
		public virtual string MailFrom { get; set; }
		#endregion

		#region MailReply
		public abstract class mailReply : PX.Data.BQL.BqlString.Field<mailReply> { }

		[PXDBString(500, IsUnicode = true)]
		[PXUIField(DisplayName = "Reply")]
		public virtual string MailReply { get; set; }
		#endregion

		#region MailTo
		public abstract class mailTo : PX.Data.BQL.BqlString.Field<mailTo> { }

		[PXDBString(3000, IsUnicode = true)]
		[PXUIField(DisplayName = "To")]
		public virtual string MailTo { get; set; }
		#endregion

		#region MailCc
		public abstract class mailCc : PX.Data.BQL.BqlString.Field<mailCc> { }

		[PXDBString(3000, IsUnicode = true)]
		[PXUIField(DisplayName = "CC")]
		public virtual string MailCc { get; set; }
		#endregion

		#region MailBcc
		public abstract class mailBcc : PX.Data.BQL.BqlString.Field<mailBcc> { }

		[PXDBString(3000, IsUnicode = true)]
		[PXUIField(DisplayName = "BCC")]
		public virtual string MailBcc { get; set; }
		#endregion

		#region RetryCount
		public abstract class retryCount : PX.Data.BQL.BqlInt.Field<retryCount> { }

		[PXDBInt]
		[PXUIField(Visible = false)]
		[PXDefault(0)]
		public virtual int? RetryCount { get; set; }
		#endregion

		#region MessageId
		public abstract class messageId : PX.Data.BQL.BqlString.Field<messageId> { }

		[PXDBString(255)] // TODO: need review length
		[PXUIField(Visible = false)]
		public virtual string MessageId { get; set; }
		#endregion

		#region MessageReference
		public abstract class messageReference : PX.Data.BQL.BqlString.Field<messageReference> { }

		[PXDBString]
		[PXUIField(Visible = false)]
		public virtual string MessageReference { get; set; }
		#endregion

		#region Exception
		public abstract class exception : PX.Data.BQL.BqlString.Field<exception> { }

		[PXDBString(IsUnicode = true)]
		[PXUIField(DisplayName = "Error Message")]
		public virtual string Exception { get; set; }
        #endregion

        #region Exception
        public abstract class redexception : PX.Data.BQL.BqlString.Field<redexception> { }

        [PXString(IsUnicode = true)]
        [PXUIField(DisplayName = "Error Message")]
        public virtual string RedException
        {
            get
            {
                return CacheUtility.GetErrorDescription(this.Exception);
            }
            
        }
        #endregion

        #region Format
        public abstract class format : PX.Data.BQL.BqlString.Field<format> { }

		[PXDefault(EmailFormatListAttribute.Html, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(255)]
		[PXUIField(DisplayName = "Format")]
		[EmailFormatList]
		public virtual string Format { get; set; }
		#endregion

		#region ReportFormat
		public abstract class reportFormat : PX.Data.BQL.BqlString.Field<reportFormat> { }

		[PXDBString(10)]
		[PXUIField(DisplayName = "Format")]
		[PXStringList(
			new[] { "PDF", "HTML", "Excel" }, 
			new[] { "PDF", "HTML", "Excel" })]
		public virtual string ReportFormat { get; set; }
		#endregion

		#region ID
		public abstract class id : PX.Data.BQL.BqlInt.Field<id> { }
		
		[PXDBIdentity]
		[PXUIField(Visible = false)]
		public virtual int? ID { get; set; }
		#endregion

		#region Ticket
		public abstract class ticket : PX.Data.BQL.BqlInt.Field<ticket> { }

		[PXDBInt]
		[PXUIField(Visible = false)]
		public virtual int? Ticket { get; set; }
		#endregion

		#region IsIncome
		public abstract class isIncome : PX.Data.BQL.BqlBool.Field<isIncome> { }
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Is Income")]
		public virtual bool? IsIncome { get; set; }
		#endregion



		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID(DontOverrideValue = true)]
		[PXUIField(Enabled = false)]
		public virtual Guid? CreatedByID { get; set; }
		#endregion

		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID { get; set; }
		#endregion

		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXUIField(DisplayName = "Created At", Enabled = false)]
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion

		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion

		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID { get; set; }
		#endregion

		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp]
		public virtual byte[] tstamp { get; set; }
		#endregion

		#region Source
		public abstract  class source : PX.Data.BQL.BqlString.Field<source> { }

		[PXString(IsUnicode = true)]
		public virtual string Source { get; set; }
		#endregion
	}

	/// <exclude/>
	[PXProjection(typeof(Select<SMEmail>))]
	[PXHidden]	
	public partial class SMEmailBody : IBqlTable
	{
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXDBGuid(IsKey = true, BqlField = typeof(SMEmail.noteID))]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region RefNoteID
		public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }

		[PXDBGuid(BqlField = typeof(SMEmail.refNoteID))]
		[PXDBDefault(null, PersistingCheck = PXPersistingCheck.Nothing, DefaultForUpdate = false)]
		[PXParent(typeof(Select<CRActivity, Where<CRActivity.noteID, Equal<Current<refNoteID>>>>), ParentCreate = true)]
		public virtual Guid? RefNoteID { get; set; }
		#endregion

		#region Body
		public abstract class body : PX.Data.BQL.BqlString.Field<body> { }

		[PXDBText(IsUnicode = true, BqlField = typeof(SMEmail.body))]
		[PXUIField(DisplayName = "Activity Details")]
		public virtual string Body { get; set; }
		#endregion

	}


	/// <exclude/>
	[Serializable]
	[PXBreakInheritance]
	[PXProjection(typeof(Select2<CRActivity,
		InnerJoin<SMEmail,
			On<SMEmail.refNoteID, Equal<CRActivity.noteID>>>,
		Where<CRActivity.classID, Equal<CRActivityClass.email>,
				Or<CRActivity.classID, Equal<CRActivityClass.emailRouting>>>>), Persistent = true)]
	[PXCacheName(Messages.EmailActivity)]
	[CRCacheIndependentPrimaryGraph(
		typeof(CREmailActivityMaint),
		typeof(Select<
				CRSMEmail,
			Where<
				CRSMEmail.noteID, Equal<Current<CRActivity.noteID>>>>))]
	public partial class CRSMEmail : CRActivity
	{
		#region Keys
		public class PK : PrimaryKeyOf<CRSMEmail>.By<noteID>
		{
			public static CRSMEmail Find(PXGraph graph, Guid? noteID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, noteID, options);
		}
		public static class FK
		{
			public class ActivityType : EP.EPActivityType.PK.ForeignKeyOf<CRSMEmail>.By<type> { }
			public class Contact : CR.Contact.PK.ForeignKeyOf<CRSMEmail>.By<contactID> { }
			public class BusinessAccount : CR.BAccount.PK.ForeignKeyOf<CRSMEmail>.By<bAccountID> { }
			public class EmailAccount : PX.SM.EMailAccount.PK.ForeignKeyOf<CRSMEmail>.By<mailAccountID> { }

			public class Owner : CR.Contact.PK.ForeignKeyOf<CRSMEmail>.By<ownerID> { }
			public class Workgroup : TM.EPCompanyTree.PK.ForeignKeyOf<CRSMEmail>.By<workgroupID> { }
		}
		#endregion

		#region Selected
		public new abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		#endregion

		#region CRActivity

		#region NoteID
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		/// <inheritdoc/>
		[PXSequentialNote(SuppressActivitiesCount = true, IsKey = true, BqlField = typeof(CRActivity.noteID))]
		[PXTimeTag(typeof(noteID))]
		[CRSMEmailStatisticFormulas]
		[PXReferentialIntegrityCheck]
		public override Guid? NoteID { get; set; }
		#endregion

		#region ParentNoteID
		public new abstract class parentNoteID : PX.Data.BQL.BqlGuid.Field<parentNoteID> { }

		[PXUIField(DisplayName = "Task")]
		[PXDBGuid(BqlField = typeof(CRActivity.parentNoteID))]
		[PXSelector(typeof(Search<CRParentActivity.noteID>), DescriptionField = typeof(CRParentActivity.subject), DirtyRead = true)]
		[PXRestrictor(typeof(Where<CRParentActivity.classID, Equal<CRActivityClass.task>, Or<CRParentActivity.classID, Equal<CRActivityClass.events>>>), null)]
		public override Guid? ParentNoteID { get; set; }
		#endregion

		#region RefNoteType
		public new abstract class refNoteIDType : PX.Data.BQL.BqlString.Field<refNoteIDType> { }
		#endregion

		#region RefNoteID
		public new abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }
		#endregion

		#region Source
		public new abstract class source : PX.Data.BQL.BqlString.Field<source> { }
		#endregion

		#region DocumentNoteID
		public new abstract class documentNoteID : PX.Data.BQL.BqlGuid.Field<documentNoteID> { }
		#endregion

		#region DocumentSource
		public abstract class documentSource : PX.Data.BQL.BqlString.Field<documentSource> { }

		[PXString(IsUnicode = true)]
		[PXUIField(DisplayName = "Related Document", Enabled = false)]
		public string DocumentSource { get; set; }
		#endregion

		#region ClassID
		public new abstract class classID : PX.Data.BQL.BqlInt.Field<classID> { }

		/// <inheritdoc/>
		[PXDBInt(BqlField = typeof(CRActivity.classID))]
		[CRActivityClass]
		[PXDefault(typeof(CRActivityClass.email))]
		[PXUIField(DisplayName = "Class", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public override int? ClassID { get; set; }
		#endregion

		#region ClassIcon
		public new abstract class classIcon : PX.Data.BQL.BqlString.Field<classIcon> { }
		#endregion

		#region ClassInfo
		public new abstract class classInfo : PX.Data.BQL.BqlString.Field<classInfo> { }
		#endregion

		#region Type
		public new abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		#endregion

		#region Subject
		public new abstract class subject : PX.Data.BQL.BqlString.Field<subject> { }
		#endregion

		#region Location
		public new abstract class location : PX.Data.BQL.BqlString.Field<location> { }
		#endregion

		#region Body
		//Empty for Activity of Email type
		#endregion

		#region Priority
		public new abstract class priority : PX.Data.BQL.BqlInt.Field<priority> { }
		#endregion

		#region PriorityIcon
		public new abstract class priorityIcon : PX.Data.BQL.BqlString.Field<priorityIcon> { }
		#endregion

		#region MPStatus
		public abstract class mpstatus : PX.Data.BQL.BqlString.Field<mpstatus> { }

		[PXDBString(2, IsFixed = true, IsUnicode = false, BqlField = typeof(SMEmail.mpstatus))]
		[MailStatusList]
		[PXDefault(ActivityStatusAttribute.Draft)]
		[PXUIField(DisplayName = "Mail Status", Enabled = false)]
		public virtual string MPStatus { get; set; }
		#endregion

		#region IsArchive 
		public abstract class isArchived : PX.Data.BQL.BqlBool.Field<isArchived> { }

		[PXDBBool(BqlField = typeof(SMEmail.isArchived))]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Archived", Enabled = true)]
		public virtual bool? IsArchived { get; set; }
		#endregion

		#region UIStatus
		public new abstract class uistatus : PX.Data.BQL.BqlString.Field<uistatus> { }

		/// <inheritdoc/>
		[PXDBString(2, IsFixed = true, BqlField = typeof(CRActivity.uistatus))]
		[PXFormula(typeof(Switch<
			Case<Where<isArchived, Equal<True>>, uistatus,
			Case<Where<type, IsNull>, ActivityStatusAttribute.open,
			Case<Where<mpstatus, Equal<DoubleSpace>>, ActivityStatusAttribute.completed,
			Case<Where<mpstatus, Equal<MailStatusListAttribute.processed>>, ActivityStatusAttribute.completed,
			Case<Where<mpstatus, Equal<MailStatusListAttribute.deleted>,
							Or<mpstatus, Equal<MailStatusListAttribute.failed>,
							Or<mpstatus, Equal<MailStatusListAttribute.canceled>>>>, ActivityStatusAttribute.canceled>>>>>,
			ActivityStatusAttribute.open>))]
		[ActivityStatus]
		[PXUIField(DisplayName = "Status")]
		[PXDefault(ActivityStatusAttribute.Open, PersistingCheck = PXPersistingCheck.Nothing)]
		public override string UIStatus { get; set; }
		#endregion

		#region IsOverdue
		public new abstract class isOverdue : PX.Data.BQL.BqlBool.Field<isOverdue> { }
		#endregion

		#region IsCompleteIcon
		public new abstract class isCompleteIcon : PX.Data.BQL.BqlString.Field<isCompleteIcon> { }
		#endregion

		#region CategoryID
		public new abstract class categoryID : PX.Data.BQL.BqlInt.Field<categoryID> { }
		#endregion

		#region AllDay
		public new abstract class allDay : PX.Data.BQL.BqlBool.Field<allDay> { }
		#endregion

		#region StartDate
		public new abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		#endregion

		#region EndDate
		public new abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
		#endregion

		#region CompletedDate
		public new abstract class completedDate : PX.Data.BQL.BqlDateTime.Field<completedDate> { }
		#endregion

		#region DayOfWeek
		public new abstract class dayOfWeek : PX.Data.BQL.BqlInt.Field<dayOfWeek> { }
		#endregion

		#region PercentCompletion
		public new abstract class percentCompletion : PX.Data.BQL.BqlInt.Field<percentCompletion> { }
		#endregion

		#region OwnerID
		public new abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }
		#endregion

		#region WorkgroupID
		public new abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
		#endregion

		#region IsExternal
		public new abstract class isExternal : PX.Data.BQL.BqlBool.Field<isExternal> { }
		#endregion

		#region IsPrivate
		public new abstract class isPrivate : PX.Data.BQL.BqlBool.Field<isPrivate> { }
		#endregion

		#region Incoming
		public new abstract class incoming : PX.Data.BQL.BqlBool.Field<incoming> { }

		/// <inheritdoc/>
		[PXDBBool(BqlField = typeof(CRActivity.incoming))]
		[PXFormula(typeof(Switch<Case<Where<type, IsNotNull>, Selector<type, EPActivityType.incoming>>, False>))]
		[PXUIField(DisplayName = "Incoming")]
		public override bool? Incoming
		{
			[PXDependsOnFields(typeof(isIncome))]
			get
			{
				return IsIncome;
			}
		}
		#endregion

		#region Outgoing
		public new abstract class outgoing : PX.Data.BQL.BqlBool.Field<outgoing> { }

		/// <inheritdoc/>
		[PXDBBool(BqlField = typeof(CRActivity.outgoing))]
		[PXFormula(typeof(Switch<Case<Where<type, IsNotNull>, Selector<type, EPActivityType.outgoing>>, False>))]
		[PXUIField(DisplayName = "Outgoing")]
		public override bool? Outgoing
		{
			[PXDependsOnFields(typeof(isIncome))]
			get
			{
				return !IsIncome;
			}
		}
		#endregion

		#region Synchronize
		public new abstract class synchronize : PX.Data.BQL.BqlBool.Field<synchronize> { }
		#endregion

		#region BAccountID
		public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		#endregion

		#region ContactID
		public new abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }
		#endregion
		
		#region EntityDescription

		public new abstract class entityDescription : PX.Data.BQL.BqlString.Field<entityDescription> { }
		#endregion

		#region ShowAsID
		public new abstract class showAsID : PX.Data.BQL.BqlInt.Field<showAsID> { }
		#endregion

		#region IsLocked
		public new abstract class isLocked : PX.Data.BQL.BqlBool.Field<isLocked> { }
		#endregion



		#region DeletedDatabaseRecord
		// Acuminator disable once PX1027 ForbiddenFieldsInDacDeclaration [it is needed for Exchange sync]
		public new abstract class deletedDatabaseRecord : PX.Data.BQL.BqlBool.Field<deletedDatabaseRecord> { }
		#endregion

		#region CreatedDateTime
		public new abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		#endregion

		#endregion

		#region SMEmail

		#region EmailNoteID
		public abstract class emailNoteID : PX.Data.BQL.BqlGuid.Field<emailNoteID> { }

		[PXDBSequentialGuid(BqlField = typeof(SMEmail.noteID))]
		[PXExtraKey]
		public virtual Guid? EmailNoteID { get; set; }
		#endregion

		#region ResponseToNoteID
		public new abstract class responseToNoteID : PX.Data.BQL.BqlGuid.Field<responseToNoteID> { }

		/// <inheritdoc cref="SMEmail.ResponseToNoteID"/>
		[PXUIField(DisplayName = "In Response To")]
		[PXDBGuid(BqlField = typeof(SMEmail.responseToNoteID))]
		[PXSelector(typeof(Search<SMEmail.noteID>), DirtyRead = true)]
		public virtual Guid? ResponseToNoteID { get; set; }
		#endregion

		#region EmailRefNoteID
		public abstract class emailRefNoteID : PX.Data.BQL.BqlGuid.Field<emailRefNoteID> { }

		[PXDBGuid(BqlField = typeof (SMEmail.refNoteID))]
		[PXDBDefault(null, PersistingCheck = PXPersistingCheck.Nothing, DefaultForUpdate = false)]
		public virtual Guid? EmailRefNoteID
		{
			[PXDependsOnFields(typeof(noteID))]
			get
			{
				return NoteID;
			}
		}
		#endregion

		#region EmailSubject
		public abstract class emailSubject : PX.Data.BQL.BqlString.Field<emailSubject> { }

		[PXDBString(Common.Constants.TranDescLength, InputMask = "", IsUnicode = true, BqlField = typeof (SMEmail.subject))]
		[PXUIField(DisplayName = "Summary", Visibility = PXUIVisibility.SelectorVisible)]
		[PXNavigateSelector(typeof (subject))]
		public virtual string EmailSubject
		{
			[PXDependsOnFields(typeof (subject))]
			get
			{
				return Subject;
			}
		}

		#endregion

		#region Body
		public new abstract class body : PX.Data.BQL.BqlString.Field<body> { }

		[PXDBText(IsUnicode = true, BqlField = typeof(SMEmail.body))]
		[PXUIField(DisplayName = "Activity Details")]
		public override string Body { get; set; }
		#endregion
		
		#region ImcUID
		public abstract class imcUID : PX.Data.BQL.BqlGuid.Field<imcUID> { }

		[PXDBGuid(true, BqlField = typeof(SMEmail.imcUID))]
		[PXUIField(Visible = false, Enabled = false)]
		public virtual Guid? ImcUID
		{
			get { return _ImcUID ?? (_ImcUID = Guid.NewGuid()); }
			set { _ImcUID = value; }
		}
		protected Guid? _ImcUID;
		#endregion

		#region Pop3UID
		public abstract class pop3UID : PX.Data.BQL.BqlString.Field<pop3UID> { }

		[PXDBString(150, BqlField = typeof(SMEmail.pop3UID))]
		[PXUIField(Visible = false, Enabled = false)]
		public virtual string Pop3UID { get; set; }
		#endregion

		#region ImapUID
		public abstract class imapUID : PX.Data.BQL.BqlInt.Field<imapUID> { }

		[PXDBInt(BqlField = typeof(SMEmail.imapUID))]
		[PXUIField(Visible = false, Enabled = false)]
		public virtual int? ImapUID { get; set; }
		#endregion

		#region MailAccountID
		public abstract class mailAccountID : PX.Data.BQL.BqlInt.Field<mailAccountID> { }

		[EmailAccountRaw(emailAccountsToShow: EmailAccountsToShowOptions.MineAndSystem, BqlField = typeof(SMEmail.mailAccountID), DisplayName = "From")]
		public int? MailAccountID { get; set; }

		#endregion

		#region MailFrom
		public abstract class mailFrom : PX.Data.BQL.BqlString.Field<mailFrom> { }

		[PXDBString(500, IsUnicode = true, BqlField = typeof(SMEmail.mailFrom))]
		[PXUIField(DisplayName = "From")]
		public virtual string MailFrom { get; set; }
		#endregion

		#region MailReply
		public abstract class mailReply : PX.Data.BQL.BqlString.Field<mailReply> { }

		[PXDBString(500, IsUnicode = true, BqlField = typeof(SMEmail.mailReply))]
		[PXUIField(DisplayName = "Reply")]
		public virtual string MailReply { get; set; }
		#endregion

		#region MailTo
		public abstract class mailTo : PX.Data.BQL.BqlString.Field<mailTo> { }

		[PXDBString(4000, IsUnicode = true, BqlField = typeof(SMEmail.mailTo))]
		[PXUIField(DisplayName = "To")]
		public virtual string MailTo { get; set; }
		#endregion

		#region MailCc
		public abstract class mailCc : PX.Data.BQL.BqlString.Field<mailCc> { }

		[PXDBString(4000, IsUnicode = true, BqlField = typeof(SMEmail.mailCc))]
		[PXUIField(DisplayName = "CC")]
		public virtual string MailCc { get; set; }
		#endregion

		#region MailBcc
		public abstract class mailBcc : PX.Data.BQL.BqlString.Field<mailBcc> { }

		[PXDBString(4000, IsUnicode = true, BqlField = typeof(SMEmail.mailBcc))]
		[PXUIField(DisplayName = "BCC")]
		public virtual string MailBcc { get; set; }
		#endregion

		#region RetryCount
		public abstract class retryCount : PX.Data.BQL.BqlInt.Field<retryCount> { }

		[PXDBInt(BqlField = typeof(SMEmail.retryCount))]
		[PXUIField(Visible = false)]
		[PXDefault(0)]
		public virtual int? RetryCount { get; set; }
		#endregion

		#region MessageId
		public abstract class messageId : PX.Data.BQL.BqlString.Field<messageId> { }

		[PXDBString(255, BqlField = typeof(SMEmail.messageId))] // TODO: need review length
		[PXUIField(Visible = false)]
		public virtual string MessageId { get; set; }
		#endregion

		#region MessageReference
		public abstract class messageReference : PX.Data.BQL.BqlString.Field<messageReference> { }

		[PXDBString(BqlField = typeof(SMEmail.messageReference))]
		[PXUIField(Visible = false)]
		public virtual string MessageReference { get; set; }
		#endregion

		#region Exception
		public abstract class exception : PX.Data.BQL.BqlString.Field<exception> { }

		[PXDBString(IsUnicode = true, BqlField = typeof(SMEmail.exception))]
		[PXUIField(DisplayName = "Error Message")]
		public virtual string Exception { get; set; }
		#endregion

		#region Format
		public abstract class format : PX.Data.BQL.BqlString.Field<format> { }

		[PXDefault(EmailFormatListAttribute.Html, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(255, BqlField = typeof(SMEmail.format))]
		[PXUIField(DisplayName = "Format")]
		[EmailFormatList]
		public virtual string Format { get; set; }
		#endregion

		#region ReportFormat
		public abstract class reportFormat : PX.Data.BQL.BqlString.Field<reportFormat> { }

		[PXDBString(10, BqlField = typeof(SMEmail.reportFormat))]
		[PXUIField(DisplayName = "Format")]
		[PXStringList(
			new[] { "PDF", "HTML", "Excel" },
			new[] { "PDF", "HTML", "Excel" })]
		public virtual string ReportFormat { get; set; }
		#endregion

		#region ID
		public abstract class id : PX.Data.BQL.BqlInt.Field<id> { }

		[PXDBIdentity(BqlField = typeof(SMEmail.id))]
		[PXUIField(Visible = false)]
		public virtual int? ID { get; set; }
		#endregion

		#region Ticket
		public abstract class ticket : PX.Data.BQL.BqlInt.Field<ticket> { }

		[PXDBInt(BqlField = typeof(SMEmail.ticket))]
		[PXUIField(Visible = false)]
		public virtual int? Ticket { get; set; }
		#endregion

		#region IsIncome
		public abstract class isIncome : PX.Data.BQL.BqlBool.Field<isIncome> { }

		[PXDBBool(BqlField = typeof(SMEmail.isIncome))]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Is Income")]
		public virtual bool? IsIncome { get; set; }
		#endregion



		#region EmailCreatedByID
		public abstract class emailCreatedByID : PX.Data.BQL.BqlGuid.Field<emailCreatedByID> { }

		[PXDBCreatedByID(DontOverrideValue = true, BqlField = typeof(SMEmail.createdByID))]
		[PXUIField(Enabled = false)]
		public virtual Guid? EmailCreatedByID { get; set; }
		#endregion

		#region EmailCreatedByScreenID
		public abstract class emailCreatedByScreenID : PX.Data.BQL.BqlString.Field<emailCreatedByScreenID> { }

		[PXDBCreatedByScreenID(BqlField = typeof(SMEmail.createdByScreenID))]
		public virtual string EmailCreatedByScreenID { get; set; }
		#endregion

		#region EmailCreatedDateTime
		public abstract class emailCreatedDateTime : PX.Data.BQL.BqlDateTime.Field<emailCreatedDateTime> { }

		[PXUIField(DisplayName = "Created At", Enabled = false)]
		[PXDBCreatedDateTime(BqlField = typeof(SMEmail.createdDateTime))]
		public virtual DateTime? EmailCreatedDateTime { get; set; }
		#endregion

		#region EmailLastModifiedByID
		public abstract class emailLastModifiedByID : PX.Data.BQL.BqlGuid.Field<emailLastModifiedByID> { }

		[PXDBLastModifiedByID(BqlField = typeof(SMEmail.lastModifiedByID))]
		public virtual Guid? EmailLastModifiedByID { get; set; }
		#endregion

		#region EmailLastModifiedByScreenID
		public abstract class emailLastModifiedByScreenID : PX.Data.BQL.BqlString.Field<emailLastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID(BqlField = typeof(SMEmail.lastModifiedByScreenID))]
		public virtual string EmailLastModifiedByScreenID { get; set; }
		#endregion

		#region EmailLastModifiedDateTime
		public abstract class emailLastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<emailLastModifiedDateTime> { }

		[PXDBLastModifiedDateTime(BqlField = typeof(SMEmail.lastModifiedDateTime))]
		public virtual DateTime? EmailLastModifiedDateTime { get; set; }
		#endregion

		#endregion
	}
}
