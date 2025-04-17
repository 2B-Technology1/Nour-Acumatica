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
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Data.Licensing;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;

namespace PX.Objects.PO
{
	/// <summary>
	/// Represents a contact that is used in purchase order management.
	/// </summary>
	/// <remarks>
	/// An contact state is frozen at the moment of creation of a purchase order.
	/// Each modification to the original address leads to the generation of a new <see cref="revisionID">revision</see> of the contact, which is used in the new purchase order or in the overridden contact in an existed purchase order.
	/// If the <see cref="isDefaultContact"/> field is <see langword="false"/>, the contact has been overridden or the original contact has been copied with the <see cref="revisionID">revision</see> related to the moment of creation.
	/// Also this is the base class for the following derived DACs:
	/// <list type="bullet">
	/// <item><description><see cref="POShipContact"/>, which contains the information related to the shipping of the ordered items</description></item>
	/// <item><description>`<see cref="PORemitContact"/>, which contains the information about the vendor to supply the ordered items</description></item>
	/// </list>
	/// The records of this type are created and edited on the <i>Purchase Orders (PO301000)</i> form
	/// (which corresponds to the <see cref="POOrderEntry"/> graph).
	/// </remarks>
	[Serializable()]
    [PXCacheName(Messages.POContact)]
	public partial class POContact : IBqlTable, IContact, CR.IEmailMessageTarget
	{
		#region Keys
		public class PK : PrimaryKeyOf<POContact>.By<contactID>
		{
			public static POContact Find(PXGraph graph, int? contactID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, contactID, options);
		}
		public static class FK
		{ 
			public class BAccount : CR.BAccount.PK.ForeignKeyOf<POContact>.By<bAccountID> { }
			public class BAccountContact : CR.Contact.PK.ForeignKeyOf<POContact>.By<bAccountContactID> { }

		}
		#endregion

		#region ContactID
		/// <inheritdoc cref="ContactID"/>
		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }
		protected Int32? _ContactID;

		/// <summary>
		/// The identifier of the contact.
		/// </summary>
		[PXDBIdentity(IsKey = true)]
		[PXUIField(DisplayName = "Contact ID", Visible = false)]
		public virtual Int32? ContactID
		{
			get
			{
				return this._ContactID;
			}
			set
			{
				this._ContactID = value;
			}
		}
		#endregion
		#region BAccountID
		/// <inheritdoc cref="BAccountID"/>
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		protected Int32? _BAccountID;

		/// <summary>
		/// The identifier of the <see cref="BAccount">business account</see>.
		/// The field is included in <see cref="FK.BAccount"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="BAccount.bAccountID"/> field.
		/// </value>
		[PXDBInt()]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		public virtual Int32? BAccountID
		{
			get
			{
				return this._BAccountID;
			}
			set
			{
				this._BAccountID = value;
			}
		}
		#endregion
		#region BAccountContactID
		/// <inheritdoc cref="BAccountContactID"/>
		public abstract class bAccountContactID : PX.Data.BQL.BqlInt.Field<bAccountContactID> { }
		protected Int32? _BAccountContactID;

		/// <summary>
		/// The identifier of the <see cref="Contact">business contact</see>.
		/// The field is included in <see cref="FK.BAccountContact"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Contact.contactID"/> field.
		/// </value>
		[PXDBInt()]
		public virtual Int32? BAccountContactID
		{
			get
			{
				return this._BAccountContactID;
			}
			set
			{
				this._BAccountContactID = value;
			}
		}
		#endregion
		#region IsDefaultContact
		/// <inheritdoc cref="IsDefaultContact"/>
		public abstract class isDefaultContact : PX.Data.BQL.BqlBool.Field<isDefaultContact> { }
		protected Boolean? _IsDefaultContact;

		/// <summary>
		/// If the value is <see langword="false" />, the contact has been overridden or the original contact has been copied with the revision related to the moment of creation.
		/// </summary>
		[PXDBBool()]
		[PXUIField(DisplayName = "Vendor Default", Visibility = PXUIVisibility.Visible)]
		[PXDefault(true)]
		public virtual Boolean? IsDefaultContact
		{
			get
			{
				return this._IsDefaultContact;
			}
			set
			{
				this._IsDefaultContact = value;
			}
		}
		#endregion
		#region OverrideContact
		/// <inheritdoc cref="OverrideContact"/>
		public abstract class overrideContact : PX.Data.BQL.BqlBool.Field<overrideContact> { }

		/// <summary>
		/// Specifies (if set to <see langword="true" />) that the contact is overriden.
		/// </summary>
		/// <value>
		/// If the value of the <see cref="isDefaultContact"/> field is <see langword="null"/>, the value of this field is <see langword="null"/>.
		/// If the value of the <see cref="isDefaultContact"/> field is <see langword="true"/>, the value of this field is <see langword="false"/>.
		/// If the value of the <see cref="isDefaultContact"/> field is <see langword="false"/>, the value of this field is <see langword="true"/>.
		/// </value>
		[PXBool()]
		[PXUIField(DisplayName = "Override", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? OverrideContact
		{
			[PXDependsOnFields(typeof(isDefaultContact))]
			get
			{
				return (this._IsDefaultContact == null ? this._IsDefaultContact : this._IsDefaultContact == false);
			}
			set
			{
				this._IsDefaultContact = (value == null ? value : value == false);
			}
		}
		#endregion
		#region RevisionID
		/// <inheritdoc cref="RevisionID"/>
		public abstract class revisionID : PX.Data.BQL.BqlInt.Field<revisionID> { }
		protected Int32? _RevisionID;

		/// <summary>
		/// The identifier of the revision contact.
		/// </summary>
		/// <remarks>
		/// Each modification to the original contact leads to the generation of a new revision of the contact, which is used in the new purchase order or in the overridden contact in an existed purchase order.
		/// </remarks>
		[PXDefault]
		[PXDBInt()]
        [PXUIField]
        public virtual Int32? RevisionID
		{
			get
			{
				return this._RevisionID;
			}
			set
			{
				this._RevisionID = value;
			}
		}
		#endregion
		#region Title
		/// <inheritdoc cref="Title"/>
		public abstract class title : PX.Data.BQL.BqlString.Field<title> { }
		protected String _Title;

		/// <summary>
		/// The name title of the contact.
		/// </summary>
		[PXDBString(50, IsUnicode = true)]
		[CR.Titles]
		[PXUIField(DisplayName = "Title")]
		public virtual String Title
		{
			get
			{
				return this._Title;
			}
			set
			{
				this._Title = value;
			}
		}
		#endregion
		#region Salutation
		/// <inheritdoc cref="Salutation"/>
		public abstract class salutation : PX.Data.BQL.BqlString.Field<salutation> { }
		protected String _Salutation;

		/// <summary>
		/// The job title of the contact.
		/// </summary>
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Job Title", Visibility = PXUIVisibility.SelectorVisible)]
		[PXPersonalDataField]
		public virtual String Salutation
		{
			get
			{
				return this._Salutation;
			}
			set
			{
				this._Salutation = value;
			}
		}
		#endregion
		#region Attention
		/// <inheritdoc cref="Attention"/>
		public abstract class attention : PX.Data.BQL.BqlString.Field<attention> { }

		/// <summary>
		/// The field that is used in the contact's business letters to direct the letter to the proper person if the letter is not addressed to any specific person.
		/// </summary>
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Attention", Visibility = PXUIVisibility.SelectorVisible)]
		[PXPersonalDataField]
		public virtual String Attention { get; set; }
		#endregion
		#region FullName
		/// <inheritdoc cref="FullName"/>
		public abstract class fullName : PX.Data.BQL.BqlString.Field<fullName> { }
		protected String _FullName;

		/// <summary>
		/// The account name of the contact.
		/// </summary>
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Account Name")]
		[PXPersonalDataField]
		public virtual String FullName
		{
			get
			{
				return this._FullName;
			}
			set
			{
				this._FullName = value;
			}
		}
		#endregion
		#region Email
		/// <inheritdoc cref="Email"/>
		public abstract class email : PX.Data.BQL.BqlString.Field<email> { }
		protected String _Email;

		/// <summary>
		/// The email of the contact.
		/// </summary>
		[PXDBEmail]
		[PXUIField(DisplayName = "Email", Visibility = PXUIVisibility.SelectorVisible)]
		[PXPersonalDataField]
		public virtual String Email
		{
			get
			{
				return this._Email;
			}
			set
			{
				this._Email = value;
			}
		}
		#endregion
		#region Fax
		/// <inheritdoc cref="Fax"/>
		public abstract class fax : PX.Data.BQL.BqlString.Field<fax> { }
		protected String _Fax;

		/// <summary>
		/// The fax number of the contact.
		/// </summary>
		[PXDBString(50)]
		[PXUIField(DisplayName = "Fax")]
		[CR.PhoneValidation()]
		[PXPersonalDataField]
		public virtual String Fax
		{
			get
			{
				return this._Fax;
			}
			set
			{
				this._Fax = value;
			}
		}
		#endregion
		#region FaxType
		/// <inheritdoc cref="FaxType"/>
		public abstract class faxType : PX.Data.BQL.BqlString.Field<faxType> { }
		protected String _FaxType;

		/// <summary>
		/// The type of the fax number of the contact.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="CR.PhoneTypesAttribute"/>.
		/// </value>
		[PXDBString(3)]
		[PXDefault(CR.PhoneTypesAttribute.BusinessFax, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Fax")]
		[CR.PhoneTypes]
		public virtual String FaxType
		{
			get
			{
				return this._FaxType;
			}
			set
			{
				this._FaxType = value;
			}
		}
		#endregion
		#region Phone1
		/// <inheritdoc cref="Phone1"/>
		public abstract class phone1 : PX.Data.BQL.BqlString.Field<phone1> { }
		protected String _Phone1;

		/// <summary>
		/// The first phone number of the contact.
		/// </summary>
		[PXDBString(50)]
		[PXUIField(DisplayName = "Phone 1", Visibility = PXUIVisibility.SelectorVisible)]
		[CR.PhoneValidation()]
		[PXPersonalDataField]
		public virtual String Phone1
		{
			get
			{
				return this._Phone1;
			}
			set
			{
				this._Phone1 = value;
			}
		}
		#endregion
		#region Phone1Type
		/// <inheritdoc cref="Phone1Type"/>
		public abstract class phone1Type : PX.Data.BQL.BqlString.Field<phone1Type> { }
		protected String _Phone1Type;

		/// <summary>
		/// The type of the first phone number of the contact.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="CR.PhoneTypesAttribute"/>.
		/// </value>
		[PXDBString(3)]
		[PXDefault(CR.PhoneTypesAttribute.Business1, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Phone 1")]
		[CR.PhoneTypes]
		public virtual String Phone1Type
		{
			get
			{
				return this._Phone1Type;
			}
			set
			{
				this._Phone1Type = value;
			}
		}
		#endregion
		#region Phone2
		/// <inheritdoc cref="Phone2"/>
		public abstract class phone2 : PX.Data.BQL.BqlString.Field<phone2> { }
		protected String _Phone2;

		/// <summary>
		/// The second phone number of the contact.
		/// </summary>
		[PXDBString(50)]
		[PXUIField(DisplayName = "Phone 2")]
		[CR.PhoneValidation()]
		[PXPersonalDataField]
		public virtual String Phone2
		{
			get
			{
				return this._Phone2;
			}
			set
			{
				this._Phone2 = value;
			}
		}
		#endregion
		#region Phone2Type
		/// <inheritdoc cref="Phone2Type"/>
		public abstract class phone2Type : PX.Data.BQL.BqlString.Field<phone2Type> { }
		protected String _Phone2Type;

		/// <summary>
		/// The type of the second phone number of the contact.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="CR.PhoneTypesAttribute"/>.
		/// </value>
		[PXDBString(3)]
		[PXDefault(CR.PhoneTypesAttribute.Business2, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Phone 2")]
		[CR.PhoneTypes]
		public virtual String Phone2Type
		{
			get
			{
				return this._Phone2Type;
			}
			set
			{
				this._Phone2Type = value;
			}
		}
		#endregion
		#region Phone3
		/// <inheritdoc cref="Phone3"/>
		public abstract class phone3 : PX.Data.BQL.BqlString.Field<phone3> { }
		protected String _Phone3;

		/// <summary>
		/// The third phone number of the contact.
		/// </summary>
		[PXDBString(50)]
		[PXUIField(DisplayName = "Phone 3")]
		[CR.PhoneValidation()]
		[PXPersonalDataField]
		public virtual String Phone3
		{
			get
			{
				return this._Phone3;
			}
			set
			{
				this._Phone3 = value;
			}
		}
		#endregion
		#region Phone3Type
		/// <inheritdoc cref="Phone3Type"/>
		public abstract class phone3Type : PX.Data.BQL.BqlString.Field<phone3Type> { }
		protected String _Phone3Type;

		/// <summary>
		/// The type of the third phone number of the contact.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="CR.PhoneTypesAttribute"/>.
		/// </value>
		[PXDBString(3)]
		[PXDefault(CR.PhoneTypesAttribute.Home, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Phone 3")]
		[CR.PhoneTypes]
		public virtual String Phone3Type
		{
			get
			{
				return this._Phone3Type;
			}
			set
			{
				this._Phone3Type = value;
			}
		}
		#endregion
		#region NoteID
		/// <inheritdoc cref="NoteID"/>
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXDBGuidNotNull]
		public virtual Guid? NoteID { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID]
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
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
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
		[PXDBLastModifiedByID]
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
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
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

		#region IEmailMessageTarget Members
		public string Address
		{
			get { return Email; }
		}
		public string DisplayName
		{
			get { return FullName; }
		}
		#endregion
	}
}
