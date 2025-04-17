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
using PX.Objects.EP;
using PX.SM;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data.BQL.Fluent;
using PX.SM.Email;

namespace PX.Objects.CR
{
	/// <summary>
	/// Represents the contact class in CRM.
	/// </summary>
	/// <remarks>
	/// A <i>contact class</i> is a special entity that contains different default sets of additional information about the contacts
	/// and may help the user to easily group contacts into classes.
	/// Form IDs without dots: <i>Contact Classes (CR205000)</i> form
	/// which corresponds to the <see cref="CRContactClassMaint"/> graph.
	/// </remarks>
	[PXCacheName(Messages.LeadClass)]
	[PXPrimaryGraph(typeof(CRContactClassMaint))]
	[Serializable]
	public class CRContactClass : CRBaseClass, IBqlTable, ITargetToLead, ITargetToAccount, ITargetToOpportunity
	{
		#region Keys

		/// <summary>
		/// Primary Key
		/// </summary>
		public class PK : PrimaryKeyOf<CRContactClass>.By<classID>
		{
			public static CRContactClass Find(PXGraph graph, string classID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, classID, options);
		}

		/// <summary>
		/// Foreign Keys
		/// </summary>
		public static class FK
		{
			/// <summary>
			/// Default Assignment Map
			/// </summary>
			public class DefaultAssignmentMap : EP.EPAssignmentMap.PK.ForeignKeyOf<CRContactClass>.By<defaultAssignmentMapID> { }

			/// <summary>
			/// Default Email Account
			/// </summary>
			public class DefaultEmailAccount : PX.SM.EMailAccount.PK.ForeignKeyOf<CRContactClass>.By<defaultEMailAccountID> { }

			/// <summary>
			/// Target Lead Class
			/// </summary>
			public class TargetLeadClass : CR.CRLeadClass.PK.ForeignKeyOf<CRContactClass>.By<targetLeadClassID> { }

			/// <summary>
			/// Target Business Account Class
			/// </summary>
			public class TargetBusinessAccountClass : CR.CRCustomerClass.PK.ForeignKeyOf<CRContactClass>.By<targetBAccountClassID> { }

			/// <summary>
			/// Target Opportunity Class
			/// </summary>
			public class TargetOpportunityClass : CR.CROpportunityClass.PK.ForeignKeyOf<CRContactClass>.By<targetOpportunityClassID> { }
		}
		#endregion

		#region ClassID
		public abstract class classID : PX.Data.BQL.BqlString.Field<classID> { }

		/// <summary>
		/// The user-friendly unique identifier of the contact class.
		/// This field is the primary key field.
		/// </summary>
		/// <value>
		/// The value can be entered only manually.
		/// </value>
		[PXSelector(typeof(CRContactClass.classID))]
		[PXUIField(DisplayName = "Class ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault]
		public virtual string ClassID { get; set; }
		#endregion

		#region IsInternal

		public abstract class isInternal : PX.Data.BQL.BqlBool.Field<isInternal> { }

		/// <summary>
		/// This field indicates that the contacts of the class are hidden from user of the Self-Service Portal
		/// so that only Acumatica ERP users can view the contacts.
		/// </summary>
		/// <value>
		/// The default value is <see langword="true"/>.
		/// </value>
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Internal", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Boolean? IsInternal { get; set; }

		#endregion

		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

		/// <summary>
		/// The brief description of the contact class.
		/// </summary>
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBString(250, IsUnicode = true)]
		public virtual string Description { get; set; }
		#endregion

		#region DefaultOwner
		public abstract class defaultOwner : PX.Data.BQL.BqlInt.Field<defaultOwner> { }

		/// <summary>
		/// The field defines a way that a default owner should be determined for a newly created contact of this class.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in the <see cref="CRDefaultOwnerAttribute.CRDefaultOwnerAttribute"/> class.
		/// The default value is <see cref="CRDefaultOwnerAttribute.DoNotChange"/>.
		/// </value>
		[PXDBString]
		[PXUIField(DisplayName = "Default Owner")]
		[PXDefault(CRDefaultOwnerAttribute.DoNotChange, PersistingCheck = PXPersistingCheck.Nothing)]
		[CRDefaultOwner]
		public override string DefaultOwner { get; set; }
		#endregion

		#region DefaultAssignmentMapID
		public abstract class defaultAssignmentMapID : PX.Data.BQL.BqlInt.Field<defaultAssignmentMapID> { }

		/// <summary>
		/// The identifier of the default assignment map that is used to assign the default owner
		/// and the workgroup to a newly created contact of this class.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="EPAssignmentMap.AssignmentMapID"/> field.
		/// </value>
		[PXDBInt]
		[PXSelector(typeof(Search<
				EPAssignmentMap.assignmentMapID,
			Where<
				EPAssignmentMap.entityType, Equal<AssignmentMapType.AssignmentMapTypeContact>>>),
			typeof(EPAssignmentMap.assignmentMapID),
			typeof(EPAssignmentMap.name),
			DescriptionField = typeof(EPAssignmentMap.name)
			)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIRequired(typeof(Where<defaultOwner, Equal<CRDefaultOwnerAttribute.assignmentMap>>))]
		[PXUIEnabled(typeof(Where<defaultOwner, Equal<CRDefaultOwnerAttribute.assignmentMap>>))]
		[PXUIField(DisplayName = "Assignment Map")]
		public override int? DefaultAssignmentMapID { get; set; }
		#endregion

		#region TargetLeadClassID
		public abstract class targetLeadClassID : PX.Data.BQL.BqlString.Field<targetLeadClassID> { }

		/// <summary>
		/// The identifier of the <see cref="CRLeadClass">lead class</see> that the system inserts by default
		/// if a user creates a lead to be associated with a contact of this class.
		/// The field is included in <see cref="FK.TargetLeadClass"/>.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="CRLeadClass.ClassID"/> field.
		/// </value>
		[PXSelector(typeof(CRLeadClass.classID))]
		[PXUIField(DisplayName = "Lead Class ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBString(10, IsUnicode = true)]
		public virtual string TargetLeadClassID { get; set; }
		#endregion

		#region TargetBAccountClassID
		public abstract class targetBAccountClassID : PX.Data.BQL.BqlString.Field<targetBAccountClassID> { }

		/// <summary>
		/// The identifier of the <see cref="CRCustomerClass">business account class</see> that the system inserts by default
		/// if a user creates a business account to be associated with a contact of this class.
		/// The field is included in <see cref="FK.TargetBusinessAccountClass"/>.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="CRCustomerClass.CRCustomerClassID"/> field.
		/// </value>
		[PXSelector(typeof(CRCustomerClass.cRCustomerClassID))]
		[PXUIField(DisplayName = "Account Class ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBString(10, IsUnicode = true)]
		public virtual string TargetBAccountClassID { get; set; }
		#endregion

		#region TargetOpportunityClassID
		public abstract class targetOpportunityClassID : PX.Data.BQL.BqlString.Field<targetOpportunityClassID> { }

		/// <summary>
		/// The identifier of the <see cref="CROpportunityClass">opportunity class</see> that the system inserts by default
		/// for a new opportunity if a user creates an opportunity based on a contact of this class.
		/// The field is included in <see cref="FK.TargetOpportunityClass"/>.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="CROpportunityClass.CROpportunityClassID"/> field.
		/// </value>
		[PXSelector(typeof(CROpportunityClass.cROpportunityClassID))]
		[PXUIField(DisplayName = "Opportunity Class ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBString(10, IsUnicode = true)]
		public virtual string TargetOpportunityClassID { get; set; }
		#endregion

		#region TargetOpportunityStage
		public abstract class targetOpportunityStage : PX.Data.BQL.BqlString.Field<targetOpportunityStage> { }

		/// <summary>
		/// The initial stage of an opportunity created from the contact of this class.
		/// This option defines the <see cref="CROpportunityStagesAttribute.CROppClassStage">opportunity stage</see>
		/// that should be set as a default one for a new opportunity created from contact qualification.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="CROpportunityStagesAttribute.CROppClassStage.StageID"/> field.
		/// </value>
		[PXDBString(2)]
		[PXUIField(DisplayName = "Opportunity Stage")]
		[CROpportunityStages(typeof(targetOpportunityClassID), OnlyActiveStages = true)]
		[PXFormula(typeof(Switch<Case<Where<CRContactClass.targetOpportunityClassID.IsNull>, Null>, CRContactClass.targetOpportunityStage>))]
		[PXUIEnabled(typeof(Where<CRContactClass.targetOpportunityClassID.IsNotNull>))]
		public virtual string TargetOpportunityStage { get; set; }
		#endregion

		#region DefaultEMailAccount
		public abstract class defaultEMailAccountID : PX.Data.BQL.BqlInt.Field<defaultEMailAccountID>
		{
			// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
			public class EmailAccountRule
				: EMailAccount.userID.PreventMakingPersonalIfUsedAsSystem<
					SelectFrom<CRContactClass>.Where<FK.DefaultEmailAccount.SameAsCurrent>> {}
		}

		/// <summary>
		/// The identifier of the default email account for this contact class.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="EMailAccount.EmailAccountID"/> field.
		/// </value>
		[EmailAccountRaw(emailAccountsToShow: EmailAccountsToShowOptions.OnlySystem, DisplayName = "Default Email Account")]
		[PXForeignReference(typeof(FK.DefaultEmailAccount), Data.ReferentialIntegrity.ReferenceBehavior.SetNull)]
		public virtual int? DefaultEMailAccountID { get; set; }
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote]
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region System Columns
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp()]
		public virtual Byte[] tstamp { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}
}
