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

using PX.CS.Contracts.Interfaces;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CS;
using PX.Objects.GDPR;
using System;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.FS
{
    [Serializable()]
    [PXCacheName(TX.TableName.FSShippingContact)]
    public partial class FSShippingContact : FSContact
    {
        #region Keys
        public new class PK : PrimaryKeyOf<FSShippingContact>.By<contactID>
        {
            public static FSShippingContact Find(PXGraph graph, int? contactID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, contactID, options);
        }

        public new static class FK
        {
            public class BusinessAccount : CR.BAccount.PK.ForeignKeyOf<FSShippingContact>.By<bAccountID> { }
        }
        #endregion

        #region ContactID
        public new abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }
        #endregion
        #region BAccountID
        public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
        #endregion
        #region BAccountContactID
        public new abstract class bAccountContactID : PX.Data.BQL.BqlInt.Field<bAccountContactID> { }
        #endregion
        #region RevisionID
        public new abstract class revisionID : PX.Data.BQL.BqlInt.Field<revisionID> { }
        #endregion
        #region IsDefaultContact
        public new abstract class isDefaultContact : PX.Data.BQL.BqlBool.Field<isDefaultContact> { }

        [PXDBBool()]
        [PXUIField(DisplayName = "", Visibility = PXUIVisibility.Visible)]
        [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
        public override Boolean? IsDefaultContact
        {
            get
            {
                return base._IsDefaultContact;
            }
            set
            {
                base._IsDefaultContact = value;
            }
        }

        #endregion
        #region OverrideContact
        public new abstract class overrideContact : PX.Data.BQL.BqlBool.Field<overrideContact> { }

        [PXBool()]
        [PXUIField(DisplayName = "Override Contact", Visibility = PXUIVisibility.Visible)]
        public override Boolean? OverrideContact
        {
            [PXDependsOnFields(typeof(isDefaultContact))]
            get
            {
                return base.OverrideContact;
            }
            set
            {
                base.OverrideContact = value;
            }
        }
        #endregion
    }


    [Serializable()]
    [PXCacheName(TX.TableName.FSCONTACT)]
    public partial class FSContact : IBqlTable, IPersonalContact, IContactBase, CR.IEmailMessageTarget, IConsentable
    {
        #region Keys
        public class PK : PrimaryKeyOf<FSContact>.By<contactID>
        {
            public static FSContact Find(PXGraph graph, int? contactID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, contactID, options);
        }

        public static class FK
        {
            public class BusinessAccount : CR.BAccount.PK.ForeignKeyOf<FSContact>.By<bAccountID> { }
        }

        #endregion

        #region ContactID
        public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

        protected Int32? _ContactID;
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

        #region EntityType
        public abstract class entityType : ListField.ACEntityType
        {
        }

        [PXDBString(4, IsFixed = true)]
        [PXDefault(ID.ACEntityType.SERVICE_ORDER)]
        [PXUIField(DisplayName = "Entity Type", Visible = false, Enabled = false)]
        public virtual string EntityType { get; set; }
        #endregion

        #region FirstName
        public abstract class firstName : PX.Data.BQL.BqlString.Field<firstName> { }

        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "First Name")]
		[PXPersonalDataField]
        public virtual String FirstName { get; set; }
        #endregion

        #region LastName
        public abstract class lastName : PX.Data.BQL.BqlString.Field<lastName> { }

        [PXDBString(100, IsUnicode = true)]
        [PXUIField(DisplayName = "Last Name")]
		[PXPersonalDataField]
        public virtual String LastName { get; set; }
        #endregion

        #region MidName
        public abstract class midName : PX.Data.BQL.BqlString.Field<midName> { }

        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Middle Name")]
		[PXPersonalDataField]
        public virtual String MidName { get; set; }
        #endregion

        #region DisplayName
        public abstract class displayName : PX.Data.BQL.BqlString.Field<displayName> { }

        [PXUIField(DisplayName = "Display Name", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [PXDependsOnFields(typeof(FSContact.lastName), typeof(FSContact.firstName), typeof(FSContact.title))]
        [CR.ContactDisplayName(typeof(FSContact.lastName), typeof(FSContact.firstName), typeof(FSContact.midName), typeof(FSContact.title), true)]
        [PXFieldDescription]
		[PXPersonalDataField]
        public virtual String DisplayName { get; set; }

        #endregion

        #region WebSite
        public abstract class webSite : PX.Data.BQL.BqlString.Field<webSite> { }

        [PXDBWeblink]
        [PXUIField(DisplayName = "Web")]
        public virtual String WebSite { get; set; }
        #endregion

        #region BAccountID
        public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

        protected Int32? _BAccountID;
        [PXDBInt()]
        [PXDBDefault(typeof(FSServiceOrder.billCustomerID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(CR.BAccount.bAccountID), SubstituteKey = typeof(CR.BAccount.acctCD), DescriptionField = typeof(CR.BAccount.acctName), DirtyRead = true)]
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
        public abstract class bAccountContactID : PX.Data.BQL.BqlInt.Field<bAccountContactID> { }

        protected Int32? _BAccountContactID;
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

        #region BAccountLocationID
        public abstract class bAccountLocationID : PX.Data.BQL.BqlInt.Field<bAccountLocationID> { }

        protected Int32? _BAccountLocationID;
        [PXDBInt()]
        public virtual Int32? BAccountLocationID
        {
            get
            {
                return this._BAccountLocationID;
            }
            set
            {
                this._BAccountLocationID = value;
            }
        }
        #endregion

        #region IsDefaultContact
        public abstract class isDefaultContact : PX.Data.BQL.BqlBool.Field<isDefaultContact> { }

        protected Boolean? _IsDefaultContact;
        [PXDBBool()]
        [PXUIField(DisplayName = "", Visibility = PXUIVisibility.Visible)]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
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
        public abstract class overrideContact : PX.Data.BQL.BqlBool.Field<overrideContact> { }

        [PXBool()]
        [PXUIField(DisplayName = "", Visibility = PXUIVisibility.Visible)]
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
        public abstract class revisionID : PX.Data.BQL.BqlInt.Field<revisionID> { }

        protected Int32? _RevisionID;
        [PXDBInt()]
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
        public abstract class title : PX.Data.BQL.BqlString.Field<title> { }

        protected String _Title;
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
        public abstract class salutation : PX.Data.BQL.BqlString.Field<salutation> { }

        protected String _Salutation;
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
        public abstract class attention : PX.Data.BQL.BqlString.Field<attention> { }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Attention", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual String Attention { get; set; }
        #endregion
        #region FullName
        public abstract class fullName : PX.Data.BQL.BqlString.Field<fullName> { }

        protected String _FullName;
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
        public abstract class email : PX.Data.BQL.BqlString.Field<email> { }

        protected String _Email;
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
        public abstract class fax : PX.Data.BQL.BqlString.Field<fax> { }

        protected String _Fax;
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
        public abstract class faxType : PX.Data.BQL.BqlString.Field<faxType> { }

        protected String _FaxType;
        [PXDBString(3)]
        [PXDefault(CR.PhoneTypesAttribute.BusinessFax, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Fax Type")]
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
        public abstract class phone1 : PX.Data.BQL.BqlString.Field<phone1> { }

        protected String _Phone1;
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
        public abstract class phone1Type : PX.Data.BQL.BqlString.Field<phone1Type> { }

        protected String _Phone1Type;
        [PXDBString(3)]
        [PXDefault(CR.PhoneTypesAttribute.Business1, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Phone 1 Type")]
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
        public abstract class phone2 : PX.Data.BQL.BqlString.Field<phone2> { }

        protected String _Phone2;
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
        public abstract class phone2Type : PX.Data.BQL.BqlString.Field<phone2Type> { }

        protected String _Phone2Type;
        [PXDBString(3)]
        [PXDefault(CR.PhoneTypesAttribute.Business2, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Phone 2 Type")]
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
        public abstract class phone3 : PX.Data.BQL.BqlString.Field<phone3> { }

        protected String _Phone3;
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
        public abstract class phone3Type : PX.Data.BQL.BqlString.Field<phone3Type> { }

        protected String _Phone3Type;
        [PXDBString(3)]
        [PXDefault(CR.PhoneTypesAttribute.Home, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Phone 3 Type")]
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
        #region ConsentAgreement
        public abstract class consentAgreement : PX.Data.BQL.BqlBool.Field<consentAgreement> { }

        [PXConsentAgreementField]
		public virtual bool? ConsentAgreement { get; set; }
        #endregion

        #region ConsentDate
        public abstract class consentDate : PX.Data.BQL.BqlDateTime.Field<consentDate> { }

        [PXConsentDateField]
		public virtual DateTime? ConsentDate { get; set; }
        #endregion

        #region ConsentExpirationDate
        public abstract class consentExpirationDate : PX.Data.BQL.BqlDateTime.Field<consentExpirationDate> { }

        [PXConsentExpirationDateField]
		public virtual DateTime? ConsentExpirationDate { get; set; }
        #endregion
        #region NoteID
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
        [PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
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


        #region IContactBase Members

        string IContactBase.EMail
        {
            get { return _Email; }
        }

		#endregion

		#region IEmailMessageTarget Members
		public string Address
		{
			get { return Email; }
		}
		#endregion
	}
}
