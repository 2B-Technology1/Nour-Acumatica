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
using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.PM;
using PX.SM;
using PX.TM;
using PX.Web.UI;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CR.Extensions;

namespace PX.Objects.CR
{
	/// <summary>
	/// An alias of <see cref="CRActivity"/>
	/// used to isolate the cache of a selector for the
	/// <see cref="CRActivity.ParentNoteID">CRActivity.ParentNoteID</see> field
	/// from the <see cref="CRActivity"/> class itself.
	/// This class is preserved for internal use only.
	/// </summary>
	[PXHidden]
	[PXBreakInheritance]
	[PXTable(typeof(CRActivity.noteID))]
	public partial class CRParentActivity : CRActivity
	{
		#region Keys
		public new class PK : PrimaryKeyOf<CRParentActivity>.By<noteID>
		{
			public static CRParentActivity Find(PXGraph graph, Guid noteID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, noteID, options);
		}
		#endregion
	}

	/// <summary>
	/// Represents the base entity for activities that are used to log interactions between users and contacts.
	/// The records of this type are created and edited on the Activity (CR306010) form
	/// (which corresponds to the <see cref="CRActivityMaint"/> graph),
	/// the Events (CR306030) form (<see cref="EPEventMaint"/>),
	/// and the Tasks (CR306020) form (<see cref="CRTaskMaint"/>).
	/// The <see cref="SMEmail">emails</see> are created and edited on the Emails (CR306015) form
	/// (<see cref="CREmailActivityMaint"/>).
	/// Also, activities (events, tasks, and emails) can be created on the <b>Activities</b> tab
	/// of any document form that uses activities; the logic on this tab is implemented by the
	/// <see cref="ActivityDetailsExt{TGraph, TPrimaryEntity, TActivityEntity, TActivityEntity_NoteID}"/> generic graph extension.
	/// For instance, activities for leads use: <see cref="LeadMaint_ActivityDetailsExt"/>.
	/// </summary>
	[Serializable]
	[PXCacheName(Messages.ActivityClassInfo)]

	// CREmailActivityMaint
	[CRCacheIndependentPrimaryGraph(
		typeof(CREmailActivityMaint),
		typeof(Select<
				CRSMEmail,
			Where<
				CRSMEmail.noteID, Equal<Current<CRActivity.noteID>>>>))]
	[CRCacheIndependentPrimaryGraph(
		typeof(CREmailActivityMaint),
		typeof(Where<Current<CRActivity.classID>, Equal<CRActivityClass.email>>))]

	// CRTaskMaint
	[CRCacheIndependentPrimaryGraph(
		typeof(CRTaskMaint),
		typeof(Select<
				CRActivity,
			Where<
				CRActivity.noteID, Equal<Current<CRActivity.noteID>>,
					And<CRActivity.classID, Equal<CRActivityClass.task>>>>))]
	[CRCacheIndependentPrimaryGraph(
		typeof(CRTaskMaint),
		typeof(Where<Current<CRActivity.classID>, Equal<CRActivityClass.task>>))]

	// EPEventMaint
	[CRCacheIndependentPrimaryGraph(
		typeof(EPEventMaint),
		typeof(Select<
				CRActivity,
			Where<
				CRActivity.noteID, Equal<Current<CRActivity.noteID>>,
					And<CRActivity.classID, Equal<CRActivityClass.events>>>>))]
	[CRCacheIndependentPrimaryGraph(
		typeof(EPEventMaint),
		typeof(Where<Current<CRActivity.classID>, Equal<CRActivityClass.events>>))]

	// CRActivityMaint
	[CRCacheIndependentPrimaryGraph(
		typeof(CRActivityMaint),
		typeof(Select<
				CRActivity,
			Where<
				CRActivity.noteID, Equal<Current<CRActivity.noteID>>,
					And<CRActivity.classID, Equal<CRActivityClass.activity>>>>))]
	[CRCacheIndependentPrimaryGraph(
		typeof(CRActivityMaint),
		typeof(Where<Current<CRActivity.classID>, Equal<CRActivityClass.activity>>))]

	[CRCacheIndependentPrimaryGraph(
		typeof(CRActivityMaint),
		typeof(Where<Current<CRActivity.classID>, Equal<PMActivityClass.timeActivity>>))]
	public partial class CRActivity : IBqlTable, IAssign, INotable
	{
		#region Keys

		/// <summary>
		/// Primary Key
		/// </summary>
		public class PK : PrimaryKeyOf<CRActivity>.By<noteID>
		{
			public static CRActivity Find(PXGraph graph, Guid? noteID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, noteID, options);
		}

		/// <summary>
		/// Foreign Keys
		/// </summary>
		public static class FK
		{
			/// <summary>
			/// Business Account
			/// </summary>
			public class BusinessAccount : BAccountR.PK.ForeignKeyOf<CRActivity>.By<bAccountID> { }

			/// <summary>
			/// Contact
			/// </summary>
			public class Contact : PX.Objects.CR.Contact.PK.ForeignKeyOf<CRActivity>.By<contactID> { }

			/// <summary>
			/// Parent Task
			/// </summary>
			public class ParentActivity : PX.Objects.CR.Contact.PK.ForeignKeyOf<CRActivity>.By<parentNoteID> { }

			/// <summary>
			/// Type
			/// </summary>
			public class ActivityType : EPActivityType.PK.ForeignKeyOf<CRActivity>.By<type> { }

			/// <summary>
			/// Category
			/// </summary>
			public class Category : EPEventCategory.PK.ForeignKeyOf<CRActivity>.By<categoryID> { }

			/// <summary>
			/// Owner
			/// </summary>
			public class Owner : CR.Contact.PK.ForeignKeyOf<CRActivity>.By<ownerID> { }

			/// <summary>
			/// Workgroup
			/// </summary>
			public class Workgroup : TM.EPCompanyTree.PK.ForeignKeyOf<CRActivity>.By<workgroupID> { }
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

		/// <inheritdoc/>
		[PXSequentialNote(SuppressActivitiesCount = true, IsKey = true)]
		[PXUIField(DisplayName = "ID")]
		[PXTimeTag(typeof(noteID))]
		[CRActivityStatisticFormulas]
		[PXSelector(typeof(noteID),
			new[] { typeof(noteID) })]
		[PXReferentialIntegrityCheck]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region ParentNoteID
		public abstract class parentNoteID : PX.Data.BQL.BqlGuid.Field<parentNoteID> { }

		/// <summary>
		/// The identifier of the parent task or event of the current activity.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="NoteID"/> field.
		/// </value>
		[PXDBGuid]
		[PXUIField(DisplayName = "Task")]
		[PXSelector(typeof(Search<
				CRParentActivity.noteID,
			Where<
				CRParentActivity.classID, Equal<CRActivityClass.task>,
				Or<CRParentActivity.classID, Equal<CRActivityClass.events>>>>),
				 typeof(CRParentActivity.subject),
				 typeof(CRParentActivity.priority),
				 typeof(CRParentActivity.startDate),
				 typeof(CRParentActivity.endDate),
			DescriptionField = typeof(CRParentActivity.subject))]
		public virtual Guid? ParentNoteID { get; set; }
		#endregion

		#region RefNoteType
		public abstract class refNoteIDType : PX.Data.BQL.BqlString.Field<refNoteIDType> { }

		/// <summary>
		/// Contains the type of the related entity, that is specified in <see cref="CRActivity.RefNoteID"/>.
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Related Entity Type")]
		[PXEntityTypeList]
		[PXUIEnabled(typeof(Where<IsMobile.IsEqual<False>>))]
		[PXDBScalar(typeof(Search<Note.entityType, Where<Note.noteID, Equal<refNoteID>>>))]
		public virtual string RefNoteIDType { get; set; }
		#endregion

		#region RefNoteID
		public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }

		/// <summary>
		/// Contains the <see cref="INotable.NoteID"/> value of the related entity.
		/// This activity is displayed on the <b>Activities</b> tab of the entity's form.
		/// </summary>
		/// <remarks>The related document may or may not implement the <see cref="INotable"/> interface,
		/// but it must have a field marked with the <see cref="PXNoteAttribute"/> attribute
		/// with the <see cref="PXNoteAttribute.ShowInReferenceSelector"/> property set to <see langword="true"/>.
		/// </remarks>
		[ActivityEntityIDSelector(typeof(refNoteIDType), typeof(contactID), typeof(bAccountID))]
		[PXDBGuid]
		[PXParent(typeof(Select<CRActivityStatistics, Where<CRActivityStatistics.noteID, Equal<Current<refNoteID>>>>), LeaveChildren = true, ParentCreate = true)]
		[PXUIField(DisplayName = "Related Entity")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIRequired(typeof(Where<refNoteIDType, IsNotNull>))]
		public virtual Guid? RefNoteID { get; set; }
		#endregion

		#region DocumentNoteID
		public abstract class documentNoteID : PX.Data.BQL.BqlGuid.Field<documentNoteID> { }

		/// <summary>
		/// The <see cref="INotable.NoteID"/> value of the related document.
		/// It is similar to <see cref="RefNoteID"/>, but contains an additional reference
		/// to the source of the activity (usually the source of the activity is a document).
		/// For example, the (<see cref="CRMassMailMaint"/>) graph fills this value with a link to the source
		/// <see cref="CRCampaign"/> or <see cref="CRMarketingList"/> object.
		/// </summary>
		[PXDBGuid]
		[PXUIField(DisplayName = "Related Document")]
		public virtual Guid? DocumentNoteID { get; set; }
		#endregion

		#region Source
		public abstract class source : PX.Data.BQL.BqlString.Field<source> { }

		/// <summary>
		/// The description of the entity whose <tt>NoteID</tt> value is specified as <see cref="RefNoteID"/>.
		/// </summary>
		/// <value>
		/// The description is retrieved by the
		/// <see cref="EntityHelper.GetEntityDescription(Guid?, System.Type)"/> method.
		/// </value>
		[PXString(IsUnicode = true)]
		[PXUIField(DisplayName = "Related Entity", Enabled = false)]
		[PXFormula(typeof(EntityDescription<refNoteID>))]
		public virtual string Source { get; set; }
		#endregion

		#region ClassID
		public abstract class classID : PX.Data.BQL.BqlInt.Field<classID> { }

		/// <summary>
		/// The class of the activity.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in the <see cref="CRActivityClass"/> class.
		/// The default values are <see cref="CRActivityClass.Activity"/> for the <see cref="CRActivityMaint"/> graph,
		/// <see cref="CRActivityClass.Task"/> for the <see cref="CRTaskMaint"/> graph,
		/// <see cref="CRActivityClass.Event"/> for the <see cref="EPEventMaint"/> graph,
		/// and <see cref="CRActivityClass.Email"/> for the <see cref="CREmailActivityMaint"/> graph.
		/// This field must be specified at the initialization of an email and not be changed afterwards.
		/// </value>
		[PXDBInt]
		[CRActivityClass]
		[PXDefault(typeof(CRActivityClass.activity))]
		[PXUIField(DisplayName = "Class", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual int? ClassID { get; set; }
		#endregion

		#region ClassIcon

		public abstract class classIcon : PX.Data.BQL.BqlString.Field<classIcon>
		{
			public class task : PX.Data.BQL.BqlString.Constant<task>
			{
				public task() : base(Sprite.Main.GetFullUrl(Sprite.Main.Task)) { }
			}
			public class events : PX.Data.BQL.BqlString.Constant<events>
			{
				public events() : base(Sprite.Main.GetFullUrl(Sprite.Main.Event)) { }
			}
			public class email : PX.Data.BQL.BqlString.Constant<email>
			{
				public email() : base(Sprite.Main.GetFullUrl(Sprite.Main.MailSend)) { }
			}
			public class emailResponse : PX.Data.BQL.BqlString.Constant<emailResponse>
			{
				public emailResponse() : base(Sprite.Main.GetFullUrl(Sprite.Main.MailReceive)) { }
			}
		}

		/// <summary>
		/// The URL of the icon that corresponds to the <see cref="ClassID"/> of the activity.
		/// </summary>
		/// <value>
		/// The icon is displayed in a grid to indicate the activity type.
		/// The available values are listed in the <see cref="classIcon"/> class.
		/// </value>
		[PXUIField(DisplayName = "Class Icon", IsReadOnly = true)]
		[PXImage(HeaderImage = (Sprite.AliasControl + "@" + Sprite.Control.Empty))]
		[PXFormula(typeof(Switch<Case<Where<classID, Equal<CRActivityClass.task>>, CRActivity.classIcon.task,
			Case<Where<classID, Equal<CRActivityClass.events>>, CRActivity.classIcon.events,
			Case<Where<classID, Equal<CRActivityClass.email>, And<incoming, Equal<True>>>, CRActivity.classIcon.email,
			Case<Where<classID, Equal<CRActivityClass.email>, And<outgoing, Equal<True>>>, CRActivity.classIcon.emailResponse
			>>>>,
			Selector<Current2<type>, EPActivityType.imageUrl>>))]
		public virtual string ClassIcon { get; set; }
		#endregion

		#region ClassInfo
		public abstract class classInfo : PX.Data.BQL.BqlString.Field<classInfo>
		{
			public class emailResponse : PX.Data.BQL.BqlString.Constant<emailResponse>
			{
				public emailResponse() : base(Messages.EmailResponseClassInfo) { }
			}
		}
		
		[PXString]
		[PXUIField(DisplayName = "Type", Enabled = false)]
		[PXFormula(typeof(Switch<
			Case<Where<Current2<noteID>, IsNull>, StringEmpty,
			Case<Where<classID, Equal<CRActivityClass.activity>, And<type, IsNotNull>>, Selector<type, EPActivityType.description>,
			Case<Where<classID, Equal<CRActivityClass.email>, And<incoming, Equal<True>>>, classInfo.emailResponse>>>,
			String<classID>>))]
		public virtual string ClassInfo { get; set; }
		#endregion

		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }

		/// <summary>
		/// The type of the activity.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="EPActivityType.Type"/> field.
		/// </value>
		[PXDBString(5, IsFixed = true, IsUnicode = false)]
		[PXUIField(DisplayName = "Type", Required = true)]
		[PXSelector(typeof(EPActivityType.type), DescriptionField = typeof(EPActivityType.description))]
		[PXRestrictor(typeof(Where<EPActivityType.active, Equal<True>>), Messages.InactiveActivityType, typeof(EPActivityType.type))]
		[PXRestrictor(typeof(Where<EPActivityType.isInternal, Equal<True>, Or<EPActivityType.isSystem, Equal<True>>>), Messages.ExternalActivityType, typeof(EPActivityType.type))]
		[PXDefault(typeof(Search<EPActivityType.type,
			Where<Current<classID>, Equal<EPActivityType.classID>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string Type { get; set; }
		#endregion

		#region Subject
		public abstract class subject : PX.Data.BQL.BqlString.Field<subject> { }

		/// <summary>
		/// The summary description of the activity.
		/// </summary>
		[PXDBString(Common.Constants.TranDescLength, InputMask = "", IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Summary", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		[PXNavigateSelector(typeof(subject))]
		public virtual string Subject { get; set; }
		#endregion

		#region Location
		public abstract class location : PX.Data.BQL.BqlString.Field<location> { }

		/// <summary>
		/// The location of the event.
		/// </summary>
		[PXDBString(255, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Location")]
		public virtual string Location { get; set; }
		#endregion

		#region Body
		public abstract class body : PX.Data.BQL.BqlString.Field<body> { }

		/// <summary>
		/// The HTML body of the activity.
		/// </summary>
		[PXDBText(IsUnicode = true)]
		[PXUIField(DisplayName = "Activity Details")]
		public virtual string Body { get; set; }
		#endregion

		#region Priority
		public abstract class priority : PX.Data.BQL.BqlInt.Field<priority> { }

		/// <summary>
		/// The priority of the activity.
		/// </summary>
		[PXDBInt]
		[PXUIField(DisplayName = "Priority")]
		[PXDefault(1, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXIntList(
			new [] { 0, 1, 2 },
			new [] { Messages.Low, Messages.Normal, Messages.High })]
		public virtual int? Priority { get; set; }
		#endregion

		#region PriorityIcon
		public abstract class priorityIcon : PX.Data.BQL.BqlString.Field<priorityIcon>
		{
			public class low : PX.Data.BQL.BqlString.Constant<low>
			{
				public low() : base(Sprite.Control.GetFullUrl(Sprite.Control.PriorityLow)) { }
			}
			public class high : PX.Data.BQL.BqlString.Constant<high>
			{
				public high() : base(Sprite.Control.GetFullUrl(Sprite.Control.PriorityHigh)) { }
			}
		}

		/// <summary>
		/// The URL of the icon to indicate the <see cref="Priority"/> of the activity.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in the <see cref="priorityIcon"/> class.
		/// If the value is <see langword="null"/>, no icon is used.
		/// </value>
		[PXUIField(DisplayName = "Priority Icon", IsReadOnly = true)]
		[PXImage(HeaderImage = (Sprite.AliasControl + "@" + Sprite.Control.PriorityHead))]
		[PXFormula(typeof(Switch<
			Case<Where<priority, Equal<int0>>, CRActivity.priorityIcon.low,
			Case<Where<priority, Equal<int2>>, CRActivity.priorityIcon.high>>>))]
		public virtual string PriorityIcon { get; set; }
		#endregion

		#region UIStatus
		public abstract class uistatus : PX.Data.BQL.BqlString.Field<uistatus> { }

		/// <summary>
		/// The status of the activity.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in the <see cref="ActivityStatusListAttribute"/> class.
		/// The default value is <see cref="ActivityStatusListAttribute.Open"/>.
		/// </value>
		[PXDBString(2, IsFixed = true)]
		[ActivityStatus]
		[PXUIField(DisplayName = "Status")]
		[PXDefault(ActivityStatusAttribute.Open, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string UIStatus { get; set; }
		#endregion

		#region IsOverdue
		public abstract class isOverdue : PX.Data.BQL.BqlBool.Field<isOverdue> { }

		/// <summary>
		/// Indicates whether the activity is overdue
		/// (the <see cref="UIStatus"/> is neither <see cref="ActivityStatusListAttribute.Completed"/> 
		/// nor <see cref="ActivityStatusListAttribute.Canceled"/> and <see cref="EndDate"/> is passed).
		/// </summary>
		[PXBool]
		public virtual bool? IsOverdue
		{
			[PXDependsOnFields(typeof(uistatus), typeof(endDate))]
			get
			{
				return
					UIStatus != ActivityStatusAttribute.Completed &&
					UIStatus != ActivityStatusAttribute.Canceled &&
					EndDate != null &&
					EndDate < PX.Common.PXTimeZoneInfo.Now;
			}
		}
		#endregion

		#region IsCompleteIcon
		public abstract class isCompleteIcon : PX.Data.BQL.BqlString.Field<isCompleteIcon>
		{
			public class completed : PX.Data.BQL.BqlString.Constant<completed>
			{
				public completed() : base(Sprite.Control.GetFullUrl(Sprite.Control.Complete)) { }
			}
		}

		/// <summary>
		/// The URL of the icon for a completed activity (<see cref="UIStatus"/> is
		/// <see cref="ActivityStatusListAttribute.Completed"/>).
		/// </summary>
		[PXUIField(DisplayName = "Complete Icon", IsReadOnly = true)]
		[PXImage(HeaderImage = (Sprite.AliasControl + "@" + Sprite.Control.CompleteHead))]
		[PXFormula(typeof(Switch<
			Case<Where<uistatus, Equal<ActivityStatusListAttribute.completed>>, CRActivity.isCompleteIcon.completed>>))]
		public virtual String IsCompleteIcon { get; set; }
		#endregion

		#region CategoryID
		public abstract class categoryID : PX.Data.BQL.BqlInt.Field<categoryID> { }

		/// <summary>
		/// The identifier of the task or event category.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="EPEventCategory.CategoryID">EPEventCategory.CategoryID</see> field.
		/// </value>
		[PXDBInt]
		[PXSelector(typeof(EPEventCategory.categoryID), SubstituteKey = typeof(EPEventCategory.description))]
		[PXUIField(DisplayName = "Category")]
		public virtual int? CategoryID { get; set; }
		#endregion

		#region AllDay
		public abstract class allDay : PX.Data.BQL.BqlBool.Field<allDay> { }

		/// <summary>
		/// Specifies whether this event is an all-day event.
		/// </summary>
		[EPAllDay(typeof(startDate), typeof(endDate))]
		[PXUIField(DisplayName = "All Day")]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Switch<Case<Where<classID, Equal<CRActivityClass.task>>, True>, False>))]
		public virtual bool? AllDay { get; set; }
		#endregion

		#region TimeZone
		public abstract class timeZone : PX.Data.BQL.BqlString.Field<timeZone> { }

		/// <summary>
		/// The custom time zone of the activity.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="PXTimeZoneAttribute"/>.
		/// If it does not set to <see langword="null"/> then adjusts <see cref="StartDate"/> and <see cref="EndDate"/>
		/// field values to the specified time zone.
		/// </value>
		/// <remarks>
		/// It is currently used only by <see cref="EPEventMaint"/>.
		/// </remarks>
		[PXDBString(32)]
		[PXUIField(DisplayName = "Time Zone")]
		[PXTimeZone(false)]
		[PXUIEnabled(typeof(Where<allDay, Equal<False>>))]
		public virtual String TimeZone { get; set; }
		#endregion

		#region CompletedDate
		public abstract class completedDate : PX.Data.BQL.BqlDateTime.Field<completedDate> { }

		/// <summary>
		/// The date and time when activity was completed
		/// (<see cref="UIStatus"/> was set to <see cref="ActivityStatusListAttribute.Completed"/>).
		/// </summary>
		[PXDBDate(InputMask = "g", PreserveTime = true)]
		[PXUIField(DisplayName = "Completed At", Enabled = false)]
		[PXFormula(typeof(
			Switch<
				Case<Where<uistatus, Equal<ActivityStatusAttribute.completed>>, PXDateAndTimeAttribute.now>,
			completedDate>))]
		public virtual DateTime? CompletedDate { get; set; }
		#endregion

		#region DayOfWeek
		public abstract class dayOfWeek : PX.Data.BQL.BqlInt.Field<dayOfWeek> { }

		/// <summary>
		/// The day of week of <see cref="StartDate"/>.
		/// </summary>
		/// <value>
		/// The field is calculated automatically on the basis of <see cref="StartDate"/>.
		/// The following values are possible: <see langword="null"/> (when <see cref="StartDate"/> is <see langword="null"/>),
		/// and the values defined by the <see cref="System.DayOfWeek"/> enumeration and converted to <see langword="int"/>.
		/// </value>
		[PXInt]
		[PXUIField(DisplayName = PX.Objects.EP.Messages.DayOfWeek)]
		[DayOfWeek]
		public virtual int? DayOfWeek
		{
			[PXDependsOnFields(typeof(startDate))]
			get
			{
				var date = StartDate;
				return date != null ? (int?)((int)date.Value.DayOfWeek) : null;
			}
		}
		#endregion

		#region PercentCompletion
		public abstract class percentCompletion : PX.Data.BQL.BqlInt.Field<percentCompletion> { }

		/// <summary>
		/// The estimation of the task completion expressed as a percentage.
		/// </summary>
		[PXDBInt(MinValue = 0, MaxValue = 100)]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Completion (%)")]
		public virtual int? PercentCompletion { get; set; }
		#endregion

		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }

		/// <inheritdoc/>
		[PXChildUpdatable(AutoRefresh = true)]
		[Owner(typeof(workgroupID))]
		[PXDefault(typeof(Coalesce<
			Search<EPCompanyTreeMember.contactID,
				Where<EPCompanyTreeMember.workGroupID, Equal<Current<workgroupID>>,
				And<EPCompanyTreeMember.contactID, Equal<Current<AccessInfo.contactID>>>>>,
			Search<Contact.contactID,
				Where<Contact.contactID, Equal<Current<AccessInfo.contactID>>,
				And<Current<workgroupID>, IsNull>>>>),
				PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<workgroupID>))]
		public virtual int? OwnerID { get; set; }
		#endregion

		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate>
		{
			public abstract class startDate_date : PX.Data.BQL.BqlDateTime.Field<startDate_date> { }
			public abstract class startDate_time : PX.Data.BQL.BqlDateTime.Field<startDate_time> { }
		}

		/// <summary>
		/// The start date and time of the event.
		/// </summary>
		[EPStartDate(AllDayField = typeof(allDay), DisplayName = "Start Date", DisplayNameDate = "Date", DisplayNameTime = "Start Time")]
		[PXFormula(typeof(TimeZoneNow))]
		[PXUIField(DisplayName = "Start Date")]
		public virtual DateTime? StartDate { get; set; }
		#endregion

		#region EndDate
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate>
		{
			public abstract class endDate_date : PX.Data.BQL.BqlDateTime.Field<endDate_date> { }
			public abstract class endDate_time : PX.Data.BQL.BqlDateTime.Field<endDate_time> { }

		}

		/// <summary>
		/// The end date and time of the event.
		/// </summary>
		[EPEndDate(typeof(classID), typeof(startDate), AllDayField = typeof(allDay))]
		[PXUIField(DisplayName = "End Time")]
		public virtual DateTime? EndDate { get; set; }
		#endregion

		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }

		/// <inheritdoc/>
		[PXDBInt]
		[PXChildUpdatable(UpdateRequest = true)]
		[PXUIField(DisplayName = "Workgroup")]
		[PXCompanyTreeSelector]
		public virtual int? WorkgroupID { get; set; }
		#endregion
		
		#region IsExternal
		public abstract class isExternal : PX.Data.BQL.BqlBool.Field<isExternal> { }

		/// <summary>
		/// Specifies whether this activity was created by
		/// an external user on the portal.
		/// </summary>
		[PXDBBool]
		[PXUIField(Visible = false)]
		public virtual bool? IsExternal { get; set; }
		#endregion

		#region IsPrivate
		public abstract class isPrivate : PX.Data.BQL.BqlBool.Field<isPrivate> { }

		/// <summary>
		/// Specifies whether this activity is hidden from external users
		/// and not visible on the portal site.
		/// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = "Internal")]
		[PXFormula(typeof(IsNull<Selector<type, EPActivityType.privateByDefault>, False>))]
		public virtual bool? IsPrivate { get; set; }
		#endregion

		#region Incoming
		public abstract class incoming : PX.Data.BQL.BqlBool.Field<incoming> { }

		/// <summary>
		/// Specifies whether this activity is incoming.
		/// </summary>
		/// <value>
		/// The value is equal to <see cref="EPActivityType.Incoming">EPActivityType.Incoming</see> of the current <see cref="Type"/>.
		/// </value>
		[PXDBBool]
		[PXFormula(typeof(Switch<Case<Where<type, IsNotNull>, Selector<type, EPActivityType.incoming>>, False>))]
		[PXUIField(DisplayName = "Incoming")]
		public virtual bool? Incoming { get; set; }
		#endregion

		#region Outgoing
		public abstract class outgoing : PX.Data.BQL.BqlBool.Field<outgoing> { }

		/// <summary>
		/// Specifies whether this activity is outgoing.
		/// </summary>
		/// <value>
		/// The value is equal to <see cref="EPActivityType.Outgoing">EPActivityType.Outgoing</see> of the current <see cref="Type"/>.
		/// </value>
		[PXDBBool]
		[PXFormula(typeof(Switch<Case<Where<type, IsNotNull>, Selector<type, EPActivityType.outgoing>>, False>))]
		[PXUIField(DisplayName = "Outgoing")]
		public virtual bool? Outgoing { get; set; }
		#endregion
		
		#region Synchronize
		public abstract class synchronize : PX.Data.BQL.BqlBool.Field<synchronize> { }

		/// <summary>
		/// Specifies whether the activity should be included in the exchange synchronization.
		/// </summary>
		/// <value>
		/// The value is used in the exchange integration (see <see cref="FeaturesSet.ExchangeIntegration"/>).
		/// The default value is <see langword="true"/>.
		/// </value>
		[PXDBBool]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Synchronize")]
		public virtual bool? Synchronize { get; set; }
		#endregion
		
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

		/// <summary>
		/// The identifier of the related <see cref="BAccount">business account</see>.
		/// Along with <see cref="ContactID"/>, this field is used as an additional reference,
		/// but unlike <see cref="RefNoteID"/> and <see cref="DocumentNoteID"/> it is used for specific entities.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="BAccount.BAccountID"/> field.
		/// </value>
		[PXDBInt]
		[PXUIField(DisplayName = "Business Account")]
		[PXSelector(typeof(Search<BAccount.bAccountID>), SubstituteKey = typeof(BAccount.acctCD), DirtyRead = true)]
		public virtual int? BAccountID { get; set; }
		#endregion

		#region ContactID
		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

		/// <summary>
		/// The identifier of the related <see cref="Contact">contact</see>.
		/// Along with <see cref="BAccountID"/>, this field is used as an additional reference,
		/// but unlike <see cref="RefNoteID"/> and <see cref="DocumentNoteID"/> it is used for specific entities.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="Contact.ContactID"/> field.
		/// </value>
		[PXDBInt]
		[PXUIField(DisplayName = "Contact")]
		[PXSelector(typeof(Contact.contactID), DescriptionField = typeof(Contact.displayName), DirtyRead = true)]
		public virtual int? ContactID { get; set; }
		#endregion
		
		#region EntityDescription

		public abstract class entityDescription : PX.Data.BQL.BqlString.Field<entityDescription> { }

		/// <summary>
		/// Returns either additional information about the related entity or the last error message. The property is
		/// used by the <see cref="CREmailActivityMaint"/> graph to show additional infomation about the <see cref="SMEmail"/> status.
		/// </summary>
		[PXString(InputMask = "")]
		[PXUIField(DisplayName = "Entity", Visibility = PXUIVisibility.SelectorVisible, Enabled = false, IsReadOnly = true)]
		public virtual string EntityDescription { get; set; }
		#endregion

		#region ShowAsID
		public abstract class showAsID : PX.Data.BQL.BqlInt.Field<showAsID> { }

		/// <summary>
		/// The event status to be displayed on your schedule if it is public.
		/// </summary>
		/// <value>
		/// </value>
		[PXDBInt]
		[PXUIField(DisplayName = "Show As")]
		[ShowAsList]
		[PXDefault(typeof(ShowAsListAttribute.busy))]
		public virtual int? ShowAsID { get; set; }
		#endregion

		#region IsLocked
		public abstract class isLocked : PX.Data.BQL.BqlBool.Field<isLocked> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Is Locked")]
		public virtual bool? IsLocked { get; set; }
		#endregion



		#region DeletedDatabaseRecord
		// Acuminator disable once PX1027 ForbiddenFieldsInDacDeclaration [it is needed for Exchange sync]
		public abstract class deletedDatabaseRecord : PX.Data.BQL.BqlBool.Field<deletedDatabaseRecord> { }

		/// <exclude/>
		// Acuminator disable once PX1027 ForbiddenFieldsInDacDeclaration [it is needed for Exchange sync]
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? DeletedDatabaseRecord { get; set; }
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
		
		[PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.FromRecord)]
		public virtual byte[] tstamp { get; set; }
		#endregion
	}

	/// <summary>
	/// Lightweight projection of the <see cref="CRActivity"/> class that is used to fetch references to
	/// entities and activities (the <see cref="NoteID"/>, <see cref="RefNoteID"/>, and <see cref="ParentNoteID"/> properties).
	/// For instance, it is used to fetch <see cref="CRCase"/> only if the <see cref="NoteID"/> value of the linked activity is known.
	/// This class is preserved for internal use only.
	/// </summary>
	[PXProjection(typeof(Select<CRActivity>))]
	[PXHidden]
	public partial class CRActivityLink : IBqlTable
	{
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		/// <inheritdoc cref="CRActivity.NoteID"/>
		[PXDBGuid(IsKey = true, BqlField = typeof(CRActivity.noteID))]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region ParentNoteID
		public abstract class parentNoteID : PX.Data.BQL.BqlGuid.Field<parentNoteID> { }

		/// <inheritdoc cref="CRActivity.ParentNoteID"/>
		[PXDBGuid(BqlField = typeof(CRActivity.parentNoteID))]
		public virtual Guid? ParentNoteID { get; set; }
		#endregion

		#region RefNoteID
		public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }

		/// <inheritdoc cref="CRActivity.RefNoteID"/>
		[PXDBGuid(BqlField = typeof(CRActivity.refNoteID))]
		public virtual Guid? RefNoteID { get; set; }
		#endregion
	}

	/// <summary>
	/// The projection of the <see cref="CRActivity"/> class which is a flattened version of the
	/// <see cref="CRActivity"/>, <see cref="SMEmail"/>, and <see cref="PMTimeActivity"/> classes.
	/// It is used only by <see cref="DefaultEmailProcessor"/>.
	/// This class is preserved for internal use only.
	/// </summary>
	[Serializable]
	[PXProjection(typeof(Select2<CRActivity,
		LeftJoin<SMEmail, 
			On<SMEmail.refNoteID, Equal<CRActivity.noteID>>,
		LeftJoin<PMTimeActivity,
			On<PMTimeActivity.refNoteID, Equal<CRActivity.noteID>>>>>), Persistent = true)]
	public partial class CRPMSMEmail : CRActivity
	{
		#region NoteID
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		/// <inheritdoc cref="CRActivity.NoteID"/>
		[PXSequentialNote(SuppressActivitiesCount = true, IsKey = true, BqlField = typeof(CRActivity.noteID))]
		public override Guid? NoteID { get; set; }
		#endregion

		#region TimeActivityNoteID
		public abstract class timeActivityNoteID : PX.Data.BQL.BqlGuid.Field<timeActivityNoteID> { }

		/// <summary>
		/// The <see cref="PMTimeActivity.NoteID">PMTimeActivity.noteID</see> field.
		/// </summary>
		[PXDBSequentialGuid(BqlField = typeof(PMTimeActivity.noteID))]
		[PXExtraKey]
		public virtual Guid? TimeActivityNoteID { get; set; }
		#endregion

		#region EmailNoteID
		public abstract class emailNoteID : PX.Data.BQL.BqlGuid.Field<emailNoteID> { }

		/// <summary>
		/// The <see cref="SMEmail.NoteID">PMTimeActivity.noteID</see> field.
		/// </summary>
		[PXDBSequentialGuid(BqlField = typeof(SMEmail.noteID))]
		[PXExtraKey]
		public virtual Guid? EmailNoteID { get; set; }
		#endregion

		#region ResponseToNoteID
		public abstract class responseToNoteID : PX.Data.BQL.BqlGuid.Field<responseToNoteID> { }

		/// <summary>
		/// Email <see cref="SMEmail.NoteID"/> in response to which a new incoming (or outgoing) email was created
		/// </summary>
		[PXUIField(DisplayName = "In Response To")]
		[PXDBGuid(BqlField = typeof(SMEmail.responseToNoteID))]
		public virtual Guid? ResponseToNoteID { get; set; }
		#endregion

		#region RefNoteID
		public new abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }

		/// <inheritdoc cref="CRActivity.RefNoteID"/>
		[PXDBGuid(BqlField = typeof(CRActivity.refNoteID))]
		public override Guid? RefNoteID { get; set; }
		#endregion

		#region IsBillable
		public abstract class isBillable : PX.Data.BQL.BqlBool.Field<isBillable> { }

		/// <inheritdoc cref="PMTimeActivity.IsBillable"/>
		[PXDBBool(BqlField = typeof(PMTimeActivity.isBillable))]
		public virtual bool? IsBillable { get; set; }
		#endregion

		#region TimeCardCD
		public abstract class timeCardCD : PX.Data.BQL.BqlString.Field<timeCardCD> { }

		/// <inheritdoc cref="PMTimeActivity.TimeCardCD"/>
		[PXDBString(10, BqlField = typeof(PMTimeActivity.timeCardCD))]
		public virtual string TimeCardCD { get; set; }
		#endregion

		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		
		/// <inheritdoc cref="PMTimeActivity.ProjectID"/>
		[EPProject(typeof(ownerID), FieldClass = ProjectAttribute.DimensionName, BqlField = typeof(PMTimeActivity.projectID))]
		public virtual int? ProjectID { get; set; }
		#endregion

		#region ProjectTaskID
		public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
		
		/// <inheritdoc cref="PMTimeActivity.ProjectTaskID"/>
		[EPTimecardProjectTask(typeof(projectID), BatchModule.TA, DisplayName = "Project Task", BqlField = typeof(PMTimeActivity.projectTaskID))]		
		public virtual int? ProjectTaskID { get; set; }
		#endregion

		#region MPStatus
		public abstract class mpstatus : PX.Data.BQL.BqlString.Field<mpstatus> { }

		/// <inheritdoc cref="SMEmail.MPStatus"/>
		[PXDBString(2, IsFixed = true, IsUnicode = false, BqlField = typeof(SMEmail.mpstatus))]
		public virtual string MPStatus { get; set; }
		#endregion

		#region IsArchived
		public abstract class isArchived : PX.Data.BQL.BqlBool.Field<isArchived> { }

		/// <inheritdoc cref="SMEmail.IsArchived"/>
		[PXDBBool(BqlField = typeof(SMEmail.isArchived))]
		[PXDefault(false)]
		public virtual bool? IsArchived { get; set; }
		#endregion

		#region ID
		public abstract class id : PX.Data.BQL.BqlInt.Field<id> { }

		/// <inheritdoc cref="SMEmail.ID"/>
		[PXDBIdentity(BqlField = typeof(SMEmail.id))]
		public virtual int? ID { get; set; }
		#endregion
	}
}
