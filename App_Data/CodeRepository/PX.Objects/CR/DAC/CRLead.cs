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
using PX.Objects.CS;
using PX.Objects.CR.Standalone;
using PX.Data.BQL.Fluent;
using PX.Objects.CR.MassProcess;
using PX.Objects.CR.Workflows;
using PX.TM;
using System.Diagnostics;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data.BQL;
using PX.SM;
using PX.Data.EP;

namespace PX.Objects.CR
{
	/// <summary>
	/// Represents a marketing lead or a sales lead.
	/// </summary>
	/// <remarks>
	/// A marketing lead is a person or a company that has potential interest in a product your organization offers.
	/// A sales lead is a person or a company that expresses interest in products your organization offers.
	/// The records of this type are created and edited on the Leads (CR301000) form,
	/// which corresponds to the <see cref="LeadMaint"/> graph.
	/// Note that this class is a projection of the <see cref="Contact"/> and <see cref="Standalone.CRLead"/> classes.
	/// </remarks>
	[Serializable]
	[PXBreakInheritance]
	[PXCacheName(Messages.Lead)]
	[PXTable(typeof(CR.Contact.contactID))]
	[CRCacheIndependentPrimaryGraph(
		typeof(LeadMaint),
		typeof(Select<CRLead,
			Where<CRLead.contactID, Equal<Current<CRLead.contactID>>>>))]
	[PXGroupMask(typeof(LeftJoinSingleTable<BAccount, On<BAccount.bAccountID, Equal<CRLead.bAccountID>>>),
		WhereRestriction = typeof(Where<BAccount.bAccountID, IsNull, Or<Match<BAccount, Current<AccessInfo.userName>>>>))]
	public class CRLead : Contact
	{
		#region Keys
		public new class PK : PrimaryKeyOf<CRLead>.By<contactID>
		{
			public static CRLead Find(PXGraph graph, int? contactID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, contactID, options);
		}
		public new static class FK
		{
			public class Class : CR.CRLeadClass.PK.ForeignKeyOf<CRLead>.By<classID> { }
			public class Contact : CR.Contact.PK.ForeignKeyOf<CRLead>.By<refContactID> { }
			public class BusinessAccount : CR.BAccount.PK.ForeignKeyOf<CRLead>.By<bAccountID> { }
			public class ParentBusinessAccount : CR.BAccount.PK.ForeignKeyOf<CRLead>.By<parentBAccountID> { }

			public class Address : CR.Address.PK.ForeignKeyOf<CRLead>.By<defAddressID> { }

			public class Owner : CR.Contact.PK.ForeignKeyOf<CRLead>.By<ownerID> { }
			public class Workgroup : TM.EPCompanyTree.PK.ForeignKeyOf<CRLead>.By<workgroupID> { }
			public class SalesTerritory : CS.SalesTerritory.PK.ForeignKeyOf<CRLead>.By<salesTerritoryID> { }
		}
		#endregion

		#region CRLead

		#region ClassID
		public new abstract class classID : PX.Data.BQL.BqlString.Field<classID> { }

		[PXDBString(10, IsUnicode = true, BqlTable = typeof(CRLead))]
		[PXUIField(DisplayName = "Lead Class")]
		[PXDefault(typeof(Search<CRSetup.defaultLeadClassID>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(CRLeadClass.classID), DescriptionField = typeof(CRLeadClass.description), CacheGlobal = true)]
		[PXMassMergableField]
		[PXDeduplicationSearchField]
		[PXMassUpdatableField]
		public override String ClassID { get; set; }
		#endregion

		#endregion

		#region Contact override

		#region ContactID
		public new abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

		[CRLeadSelector(typeof(
				SelectFrom<CRLead>
				.LeftJoin<BAccount>
					.On<BAccount.bAccountID.IsEqual<CRLead.bAccountID>>
				.Where<
					BAccount.bAccountID.IsNull
					.Or<Match<BAccount, Current<AccessInfo.userName>>>
				>
				.SearchFor<CRLead.contactID>),
			fieldList: new[]
			{
				typeof(CRLead.memberName),
				typeof(CRLead.fullName),
				typeof(CRLead.salutation),
				typeof(CRLead.eMail),
				typeof(CRLead.phone1),
				typeof(CRLead.status),
				typeof(CRLead.duplicateStatus)
			},
			Headers = new []
			{
				"Contact",
				"Account Name",
				"Job Title",
				"Email",
				"Phone 1",
				"Status",
				"Duplicate"
			},
			DescriptionField = typeof(CRLead.memberName),
			Filterable = true)]
		[PXDBIdentity(IsKey = true)]
		[PXUIField(DisplayName = "Lead ID", Visibility = PXUIVisibility.Invisible)]
		[PXPersonalDataWarning]
		[LeadLastNameOrCompanyNameRequired]
		public override Int32? ContactID { get; set; }
		#endregion

		#region ContactType
		public new abstract class contactType : PX.Data.BQL.BqlString.Field<contactType> { }

		/// <summary>
		/// The type of the lead.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in the <see cref="ContactTypesAttribute"/> class.
		/// The default value is <see cref="ContactTypesAttribute.Lead"/>.
		/// This field must be specified at the initialization stage and not be changed afterwards.
		/// </value>
		[PXDBString(2, IsFixed = true)]
		[PXDefault(ContactTypesAttribute.Lead)]
		[ContactTypes]
		[PXUIField(DisplayName = "Type", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
		public override String ContactType { get; set; }
		#endregion

		#region BAccountID
		public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

		/// <inheritdoc cref="Contact.BAccountID"/>
		[CRContactBAccountDefault]
		[PXFormula(typeof(Default<refContactID>))]
		[PXDefault(typeof(IIf<Where<refContactID, IsNotNull>,
			Selector<refContactID, Contact.bAccountID>,
			bAccountID>), PersistingCheck = PXPersistingCheck.Nothing)]
		[CRMBAccount(bAccountTypes: new[]
		{
			typeof(BAccountType.prospectType),
			typeof(BAccountType.customerType),
			typeof(BAccountType.combinedType),
			typeof(BAccountType.vendorType),
		})]
		[PXMassUpdatableField]
		public override Int32? BAccountID { get; set; }
		#endregion

		#region FullName
		public new abstract class fullName : PX.Data.BQL.BqlString.Field<fullName> { }

		/// <summary>
		/// The name of the company the contact works for.
		/// </summary>
		/// <value>
		/// Either this field or the <see cref="Contact.LastName"/> field must be specified to create the lead.
		/// </value>
		[PXMassMergableField]
		[PXDeduplicationSearchField]
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Account Name", Visibility = PXUIVisibility.SelectorVisible)]
		[PXPersonalDataField]
		[PXContactInfoField]
		public override String FullName { get; set; }
		#endregion

		#region Source
		public new abstract class source : PX.Data.BQL.BqlString.Field<source> { }

		/// <summary>
		/// The source of the lead.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in the <see cref="CRMSourcesAttribute"/> class.
		/// The value of the field is automatically changed when the <see cref="ClassID"/> property is changed.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Source")]
		[CRMSources(BqlTable = typeof(CRLead))]
		[PXMassMergableField]
		[PXDeduplicationSearchField]
		[PXFormula(typeof(
			Use<Selector<CRLead.classID, CRLeadClass.defaultSource>>.AsString
				.When<False.IsEqual<Use<IsImport>.AsBool>
					.And<Brackets<EntryStatus.IsEqual<EntryStatus.inserted>.Or<CRLead.source.FromCurrent.IsNull>>>>
			.Else<CRLead.source>
		))]
		public override string Source { get; set; }
		#endregion

		#region OwnerID
		public new abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }

		/// <inheritdoc/>
		[Owner(typeof(workgroupID))]
		[PXMassUpdatableField]
		[PXMassMergableField]
		[PXDeduplicationSearchField]
		public override int? OwnerID { get; set; }

		#endregion

		#region NoteID
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		/// <inheritdoc/>
		[PXSearchable(
			category: SM.SearchCategory.CR,
			titlePrefix: "{0}: {1}",
			titleFields: new []
			{
				typeof(CRLead.contactType),
				typeof(CRLead.displayName)
			},
			fields: new []
			{
				typeof(CRLead.fullName),
				typeof(CRLead.eMail),
				typeof(CRLead.phone1),
				typeof(CRLead.phone2),
				typeof(CRLead.phone3),
				typeof(CRLead.webSite)
			},
			WhereConstraint = typeof(Where<
				CRLead.contactType
					.IsNotIn<
						ContactTypesAttribute.bAccountProperty,
						ContactTypesAttribute.employee>>),
			MatchWithJoin = typeof(LeftJoin<BAccount, On<BAccount.bAccountID, Equal<CRLead.bAccountID>>>),
			Line1Format = "{0}{1}{2}{3}",
			Line1Fields = new []
			{
				typeof(CRLead.fullName),
				typeof(CRLead.salutation),
				typeof(CRLead.phone1),
				typeof(CRLead.eMail)
			},
			Line2Format = "{1}{2}{3}",
			Line2Fields = new []
			{
				typeof(CRLead.defAddressID),
				typeof(Address.displayName),
				typeof(Address.city),
				typeof(Address.state),
				typeof(Address.countryID)
			})]
		[PXUniqueNote(
			DescriptionField = typeof(CRLead.memberName),
			Selector = typeof(CRLead.contactID),
			ShowInReferenceSelector = true)]
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
		public override Guid? NoteID { get; set; }
		#endregion

		public new abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		public new abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
		public new abstract class lastName : PX.Data.BQL.BqlString.Field<lastName> { }

		#region DisplayName
		public new abstract class displayName : PX.Data.BQL.BqlString.Field<displayName> { }

		/// <inheritdoc/>
		[PXUIField(DisplayName = "Contact", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXDependsOnFields(typeof(Contact.lastName), typeof(Contact.firstName), typeof(Contact.midName), typeof(Contact.title))]
		[PersonDisplayName(typeof(Contact.lastName), typeof(Contact.firstName), typeof(Contact.midName), typeof(Contact.title))]
		[PXDefault]
		[PXNavigateSelector(typeof(Search2<Contact.displayName,
			LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Contact.contactID>>>,
			Where2<
				Where<Contact.contactType, Equal<ContactTypesAttribute.lead>,
					Or<Contact.contactType, Equal<ContactTypesAttribute.person>,
					Or<Contact.contactType, Equal<ContactTypesAttribute.employee>>>>,
				And<Where<BAccount.bAccountID, IsNull, Or<Match<BAccount, Current<AccessInfo.userName>>>>>
			>>))]
		[PXPersonalDataField]
		[PXContactInfoField]
		public override String DisplayName { get; set; }

		#endregion

		#region MemberName
		public new abstract class memberName : PX.Data.BQL.BqlString.Field<memberName> { }

		/// <inheritdoc/>
		[PXDBCalced(typeof(Switch<
				Case<Where<Contact.displayName, Equal<Empty>>, Contact.fullName>,
			Contact.displayName>), typeof(string))]
		[PXUIField(DisplayName = "Member Name", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXString(255, IsUnicode = true)]
		[PXFieldDescription]
		public override string MemberName { get; set; }

		#endregion

		public new abstract class overrideAddress : PX.Data.BQL.BqlBool.Field<overrideAddress> { }
		public new abstract class defAddressID : PX.Data.BQL.BqlInt.Field<defAddressID> { }
		public new abstract class duplicateFound : PX.Data.BQL.BqlBool.Field<duplicateFound> { }
		public new abstract class duplicateStatus : PX.Data.BQL.BqlString.Field<duplicateStatus> { }
		public new abstract class campaignID : PX.Data.BQL.BqlString.Field<campaignID> { }
		public new abstract class parentBAccountID : PX.Data.BQL.BqlInt.Field<parentBAccountID> { }
		public new abstract class salutation : PX.Data.BQL.BqlString.Field<salutation> { }
		public new abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		public new abstract class overrideSalesTerritory : PX.Data.BQL.BqlBool.Field<overrideSalesTerritory> { }
		#endregion



		#region CRLead

		#region Status
		public new abstract class status : PX.Data.BQL.BqlString.Field<status> { }

		/// <inheritdoc cref="Standalone.CRLead.Status"/>
		[PXDBString(1, IsFixed = true, BqlTable = typeof(CRLead))]
		[PXDefault]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible)]
		[LeadWorkflow.States.List(BqlTable = typeof(CRLead))]
		[PXUIEnabled(typeof(Where<EntryStatus, NotEqual<EntryStatus.inserted>, Or<IsImport, Equal<True>>>))]
		public override String Status { get; set; }
		#endregion

		#region Resolution
		public new abstract class resolution : PX.Data.BQL.BqlString.Field<resolution> { }

		/// <inheritdoc cref="Standalone.CRLead.Resolution"/>
		[PXDBString(2, IsFixed = true, BqlTable = typeof(CRLead))]
		[PXUIField(DisplayName = "Reason", Visibility = PXUIVisibility.SelectorVisible)]
		[PXStringList(new string[0], new string[0], BqlTable = typeof(CRLead))]
		public override String Resolution { get; set; }
		#endregion

		#region RefContactID
		public abstract class refContactID : PX.Data.BQL.BqlInt.Field<refContactID> { }

		/// <inheritdoc cref="Standalone.CRLead.RefContactID"/>
		[ContactRaw(typeof(CRLead.bAccountID))]
		[PXDBChildIdentity(typeof(Contact.contactID))]
		public virtual Int32? RefContactID { get; set; }
		#endregion

		#region OverrideRefContact
		public abstract class overrideRefContact : PX.Data.BQL.BqlBool.Field<overrideRefContact> { }

		/// <inheritdoc cref="Standalone.CRLead.OverrideRefContact"/>
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Null)]
		[PXUIField(DisplayName = "Override")]
		public virtual bool? OverrideRefContact { get; set; }
		#endregion

		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

		/// <inheritdoc cref="Standalone.CRLead.Description"/>
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Description { get; set; }
		#endregion

		#region QualificationDate
		public abstract class qualificationDate : PX.Data.BQL.BqlDateTime.Field<qualificationDate> { }

		/// <inheritdoc cref="Standalone.CRLead.QualificationDate"/>
		[PXDBDate(PreserveTime = true)]
		[PXUIField(DisplayName = "Qualification Date")]
		public virtual DateTime? QualificationDate { get; set; }
		#endregion

		#region ConvertedBy
		public abstract class convertedBy : PX.Data.BQL.BqlGuid.Field<convertedBy> { }

		/// <exclude/>
		[PXDBGuid]
		[PXSelector(typeof(Users.pKID), SubstituteKey = typeof(Users.username), DescriptionField = typeof(Users.fullName), CacheGlobal = true, DirtyRead = true, ValidateValue = false)]
		[PXUIField(DisplayName = "Converted By")]
		public virtual Guid? ConvertedBy { get; set; }
		#endregion

		#endregion


		#region Attributes
		public new abstract class attributes : BqlAttributes.Field<attributes> { }

		/// <summary>
		/// The attributes available for the current contact.
		/// The field is preserved for internal use.
		/// </summary>
		[CRAttributesField(typeof(CRLead.classID), typeof(Contact.noteID))]
		public override string[] Attributes { get; set; }
		#endregion
	}
}
