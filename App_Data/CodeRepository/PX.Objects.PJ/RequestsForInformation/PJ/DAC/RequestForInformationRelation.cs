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
using PX.Objects.PJ.Common.Descriptor;
using PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.PJ.RequestsForInformation.PJ.Graphs;

namespace PX.Objects.PJ.RequestsForInformation.PJ.DAC
{
	/// <summary>
	/// Represents the relations between a request for information and a customer, an employee, or another related entity.
	/// The records of this type are created and edited through the <b>Relations</b> tab of the Request for Information (PJ301000) form
	/// (which corresponds to the <see cref="RequestForInformationMaint"/> graph).
	/// </summary>
	[Serializable]
	[PXPrimaryGraph(typeof(RequestForInformationMaint))]
	[PXCacheName(CacheNames.RequestForInformationRelation)]
    public class RequestForInformationRelation : IBqlTable
    {
		/// <summary>
		/// The unique identifier of the relation between this request for information and another entity.
		/// </summary>
		[PXDBIdentity(IsKey = true, DatabaseFieldName = "RelationId")]
        [PXUIField(Visible = false)]
        public virtual int? RequestForInformationRelationId
        {
            get;
            set;
        }

		/// <summary>
		/// The unique identifier of the <see cref="Note">note</see> associated with the relation of the request for information.
		/// </summary>
		[PXParent(typeof(Select<Contact, Where<Contact.noteID, Equal<Current<requestForInformationNoteId>>>>))]
        [PXParent(typeof(Select<BAccount, Where<BAccount.noteID, Equal<Current<requestForInformationNoteId>>>>))]
        [PXDBGuid(DatabaseFieldName = "RefNoteId")]
        [PXDBDefault(null)]
        public virtual Guid? RequestForInformationNoteId
        {
            get;
            set;
        }

		/// <summary>
		/// The role of the relation of the request for information.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="RequestForInformationRoleListAttribute"/>.
		/// </value>
		[PXDBString(2)]
        [PXUIField(DisplayName = "Role")]
        [PXDefault]
        [RequestForInformationRoleList]
        public virtual string Role
        {
            get;
            set;
        }


		/// <summary>
		/// A Boolean value that indicates (if the value is <see langword="true" />) that the current row is considered the primary relation of the selected role.
		/// </summary>
		[PXDBBool]
        [PXUIField(DisplayName = "Primary")]
        [PXDefault(false)]
        public virtual bool? IsPrimary
        {
            get;
            set;
        }

		/// <summary>
		/// The type of the entity that is related to the request for information.
		/// </summary>
		[PXDBString(40, DatabaseFieldName = "TargetType")]
        [PXUIField(DisplayName = "Type")]
        [RequestForInformationRelationType]
        [PXFormula(typeof(Default<role>))]
        public virtual string Type
        {
            get;
            set;
        }

		/// <summary>
		/// The unique identifier of the document that is relevant for the request for information.
		/// </summary>
		[PXDBGuid(DatabaseFieldName = "TargetNoteId")]
        [PXUIField(DisplayName = "Document")]
        [RequestForInformationRelationDocumentSelector]
        [PXFormula(typeof(Default<type>))]
        [PXUIEnabled(typeof(Where<role, Equal<RequestForInformationRoleListAttribute.relatedEntity>>))]
        public virtual Guid? DocumentNoteId
        {
            get;
            set;
        }

		/// <summary>
		/// The unique identifier of the <see cref="PX.Objects.CR.BAccount">business account</see> in the database.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the <see cref="PX.Objects.CR.BAccount.BAccountID"/> field.
		/// </value>
		[PXDBInt(DatabaseFieldName = "EntityId")]
        [PXUIField(DisplayName = "Account")]
        [PXFormula(typeof(Default<documentNoteId>))]
        [RequestForInformationRelationAccountSelector]
        public virtual int? BusinessAccountId
        {
            get;
            set;
        }

		/// <summary>
		/// The identifier of the <see cref="PX.Objects.CR.BAccount">business account</see> displayed on the form.
		/// </summary>
		[PXString]
        [PXUIField(DisplayName = "Account/Employee", Enabled = false)]
        public virtual string BusinessAccountCd
        {
            get;
            set;
        }

		/// <summary>
		/// The <see cref="Contact">contact</see> associated with the relation of the request for information. 
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the <see cref="Contact.ContactID"/> field.
		/// </value>
		[PXDBInt(DatabaseFieldName = "ContactId")]
        [PXUIField(DisplayName = "Contact")]
        [PXFormula(typeof(Default<documentNoteId>))]
        [PXFormula(typeof(Default<businessAccountId>))]
        [RequestForInformationRelationContactSelector]
        [PXUIEnabled(typeof(Where<type, NotEqual<RequestForInformationRelationTypeAttribute.apInvoice>,
            Or<type, NotEqual<RequestForInformationRelationTypeAttribute.arInvoice>,
            Or<type, NotEqual<RequestForInformationRelationTypeAttribute.requestForInformation>>>>))]
        public virtual int? ContactId
        {
            get;
            set;
        }

		/// <summary>
		/// A Boolean value that indicates (if the value is <see langword="true" />) that the contact should receive a copy of emails.
		/// </summary>
		[PXDBBool]
        [PXUIField(DisplayName = "Add to CC")]
        public virtual bool? AddToCc
        {
            get;
            set;
        }

		/// <summary>
		/// The name of the <see cref="PX.Objects.CR.BAccount">business account</see> associated with the relation of the request for information.
		/// </summary>
		[PXString]
        [PXUIField(DisplayName = "Name", Enabled = false)]
        public virtual string BusinessAccountName
        {
            get;
            set;
        }

		/// <summary>
		/// The name of the <see cref="Contact">contact</see> associated with the relation of the request for information.
		/// </summary>
		[PXString]
        [PXUIField(DisplayName = "Contact", Enabled = false)]
        public virtual string ContactName
        {
            get;
            set;
        }

		/// <summary>
		/// The email address of the contact person.
		/// </summary>
		[PXString]
        [PXUIField(DisplayName = "Email", Enabled = false)]
        public virtual string ContactEmail
        {
            get;
            set;
        }

        public abstract class contactEmail : IBqlField
        {
        }

        public abstract class contactName : IBqlField
        {
        }

        public abstract class businessAccountName : IBqlField
        {
        }

        public abstract class businessAccountId : IBqlField
        {
        }

        public abstract class addToCc : IBqlField
        {
        }

        public abstract class businessAccountCd : IBqlField
        {
        }

        public abstract class contactId : IBqlField
        {
        }

        public abstract class type : IBqlField
        {
        }

        public abstract class documentNoteId : IBqlField
        {
        }

        public abstract class isPrimary : IBqlField
        {
        }

        public abstract class requestForInformationNoteId : IBqlField
        {
        }

        public abstract class role : IBqlField
        {
        }

        public abstract class requestForInformationRelationId : IBqlField
        {
        }
    }
}
