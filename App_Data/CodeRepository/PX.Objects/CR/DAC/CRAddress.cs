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

using PX.Data;
using PX.Objects.CS;
using System;
using System.Diagnostics;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.CS.Contracts.Interfaces;
using PX.Data.BQL;

namespace PX.Objects.CR
{
	/// <summary>
	/// Represents a shipping address that a user specifies in a document such as
	/// <see cref="CROpportunity.OpportunityAddressID"> an opportunity's shipping address</see>.
	/// A <see cref="CRShippingAddress"/> record is a copy of the default location <see cref="Address"/> of the business account
	/// and can be used to override the address specified in the document.
	/// A <see cref="CRShippingAddress"/> record is independent from changes to the original <see cref="Address"/> record.
	/// </summary>
    [Serializable]
    [PXCacheName(Messages.ShippingAddress)]
    public partial class CRShippingAddress : CRAddress
    {
        #region Keys

		/// <summary>
		/// Primary key
		/// </summary>
        public new class PK : PrimaryKeyOf<CRShippingAddress>.By<addressID>
        {
            public static CRShippingAddress Find(PXGraph graph, int? addressID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, addressID, options);
        }

		/// <summary>
		/// Foreign Keys
		/// </summary>
        public new static class FK
        {
			/// <summary>
			/// Business Account
			/// </summary>
            public class BusinessAccount : CR.BAccount.PK.ForeignKeyOf<CRShippingAddress>.By<bAccountID> { }

			/// <summary>
			/// Business Account Address
			/// </summary>
            public class BusinessAccountAddress : CR.Address.PK.ForeignKeyOf<CRShippingAddress>.By<bAccountAddressID> { }

			/// <summary>
			/// Country
			/// </summary>
            public class Country : CS.Country.PK.ForeignKeyOf<CRShippingAddress>.By<countryID> { }

			/// <summary>
			/// State
			/// </summary>
            public class State : CS.State.PK.ForeignKeyOf<CRShippingAddress>.By<countryID, state> { }
        }
        #endregion

        #region AddressID

		/// <inheritdoc cref="Address.AddressID" />
        public new abstract class addressID : BqlInt.Field<addressID> { }
        #endregion

        #region BAccountID

		/// <inheritdoc cref="Address.BAccountID" />
        public new abstract class bAccountID : BqlInt.Field<bAccountID> { }
        #endregion

        #region BAccountAddressID

		/// <inheritdoc cref="CRAddress.BAccountID" />
        public new abstract class bAccountAddressID : BqlInt.Field<bAccountAddressID> { }
        #endregion

        #region IsDefaultAddress
        public new abstract class isDefaultAddress : BqlBool.Field<isDefaultAddress> { }

		/// <inheritdoc cref="CRAddress.IsDefaultAddress" />
        [PXDBBool()]
        [PXUIField(DisplayName = "Default Customer Address", Visibility = PXUIVisibility.Visible)]
        [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
        public override Boolean? IsDefaultAddress
        {
            get
            {
                return base._IsDefaultAddress;
            }
            set
            {
                base._IsDefaultAddress = value;
            }
        }

		#endregion

		#region IsValidated
		public new abstract class isValidated : PX.Data.IBqlField { }
		#endregion

		#region OverrideAddress
		public new abstract class overrideAddress : PX.Data.IBqlField
        {
        }

		/// <inheritdoc cref="CRAddress.OverrideAddress" />
		[PXBool()]
		[PXUIField(DisplayName = "Override", Visibility = PXUIVisibility.Visible)]
		public override Boolean? OverrideAddress
		{
			[PXDependsOnFields(typeof(isDefaultAddress))]
			get
			{
				return base.OverrideAddress;
			}
			set
			{
				base.OverrideAddress = value;
			}
		}
		#endregion

		#region RevisionID

		/// <inheritdoc cref="CRAddress.RevisionID" />
		public new abstract class revisionID : BqlInt.Field<revisionID> { }
        #endregion

        #region CountryID

        public new abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		[PXDefault(typeof(Search<GL.Branch.countryID, Where<GL.Branch.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXDBString(100)]
		[PXUIField(DisplayName = "Country")]
		[Country]
		public override String CountryID { get; set; }
        #endregion

        #region State

        public new abstract class state : PX.Data.BQL.BqlString.Field<state> { }

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "State")]
		[State(typeof(countryID))]
		public override String State { get; set; }
        #endregion

        #region PostalCode
        public new abstract class postalCode : PX.Data.BQL.BqlString.Field<postalCode> { }

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
        [PXDBString(20)]
        [PXUIField(DisplayName = "Postal Code")]
        [PXZipValidation(typeof(Country.zipCodeRegexp), typeof(Country.zipCodeMask), countryIdField: typeof(countryID))]
		[PXPersonalDataField]
		public override String PostalCode { get; set; }
        #endregion

        #region Latitude

		/// <inheritdoc cref="Address.Latitude" />
        public new abstract class latitude : PX.Data.BQL.BqlDecimal.Field<latitude> { }
        #endregion

        #region Longitude

		/// <inheritdoc cref="Address.Longitude" />
        public new abstract class longitude : PX.Data.BQL.BqlDecimal.Field<longitude> { }
        #endregion
    }

	/// <summary>
	/// Represents a billing address that is specified in the document, such as
	/// <see cref="CROpportunity.OpportunityAddressID"> an opportunity's billing address</see>.
	/// A <see cref="CRBillingAddress"/> record is as a copy of the default location <see cref="Address"/> of the business account
	/// and can be used to override the address specified in the document.
	/// A <see cref="CRBillingAddress"/> record is independent from changes to the original <see cref="Address"/> record.
	/// </summary>
    [Serializable]
    [PXCacheName(Messages.BillToAddress)]
    public partial class CRBillingAddress : CRAddress
    {
        #region Keys

		/// <summary>
		/// Primary key
		/// </summary>
        public new class PK : PrimaryKeyOf<CRBillingAddress>.By<addressID>
        {
            public static CRBillingAddress Find(PXGraph graph, int? addressID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, addressID, options);
        }

		/// <summary>
		/// Foreign Keys
		/// </summary>
        public new static class FK
        {
			/// <summary>
			/// Business Account
			/// </summary>
            public class BusinessAccount : CR.BAccount.PK.ForeignKeyOf<CRBillingAddress>.By<bAccountID> { }

			/// <summary>
			/// Business Account Address
			/// </summary>
            public class BusinessAccountAddress : CR.Address.PK.ForeignKeyOf<CRBillingAddress>.By<bAccountAddressID> { }

			/// <summary>
			/// Country
			/// </summary>
            public class Country : CS.Country.PK.ForeignKeyOf<CRBillingAddress>.By<countryID> { }

			/// <summary>
			/// State
			/// </summary>
            public class State : CS.State.PK.ForeignKeyOf<CRBillingAddress>.By<countryID, state> { }
        }
        #endregion

        #region AddressID

		/// <inheritdoc cref="Address.AddressID" />
        public new abstract class addressID : BqlInt.Field<addressID> { }
        #endregion

        #region BAccountID

		/// <inheritdoc cref="Address.BAccountID" />
        public new abstract class bAccountID : BqlInt.Field<bAccountID> { }
        #endregion

        #region BAccountAddressID

		/// <inheritdoc cref="CRAddress.BAccountID" />
        public new abstract class bAccountAddressID : BqlInt.Field<bAccountAddressID> { }
        #endregion

        #region IsDefaultAddress
        public new abstract class isDefaultAddress : BqlBool.Field<isDefaultAddress> { }

		/// <inheritdoc cref="CRAddress.IsDefaultAddress" />
        [PXDBBool]
        [PXUIField(DisplayName = "Default Customer Address", Visibility = PXUIVisibility.Visible)]
        [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
        public override Boolean? IsDefaultAddress
        {
            get
            {
                return base._IsDefaultAddress;
            }
            set
            {
                base._IsDefaultAddress = value;
            }
        }

		#endregion

		#region IsValidated
		public new abstract class isValidated : PX.Data.IBqlField { }
		#endregion

		#region OverrideAddress
		public new abstract class overrideAddress : BqlBool.Field<overrideAddress> { }

		/// <inheritdoc cref="CRAddress.OverrideAddress" />
        [PXBool()]
        [PXUIField(DisplayName = "Override", Visibility = PXUIVisibility.Visible)]
        public override Boolean? OverrideAddress
        {
            [PXDependsOnFields(typeof(isDefaultAddress))]
            get
            {
                return base.OverrideAddress;
            }
            set
            {
                base.OverrideAddress = value;
            }
        }
        #endregion

        #region RevisionID

		/// <inheritdoc cref="CRAddress.RevisionID" />
        public new abstract class revisionID : BqlInt.Field<revisionID> { }
        #endregion

        #region CountryID

        public new abstract class countryID : BqlString.Field<countryID> { }

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		[PXDefault(typeof(Search<GL.Branch.countryID, Where<GL.Branch.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXDBString(100)]
		[PXUIField(DisplayName = "Country")]
		[Country]
		public override String CountryID { get; set; }

        #endregion

        #region State

        public new abstract class state : BqlString.Field<state> { }

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "State")]
		[State(typeof(countryID))]
		public override String State { get; set; }
        #endregion

        #region PostalCode
        public new abstract class postalCode : PX.Data.BQL.BqlString.Field<postalCode> { }

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
        [PXDBString(20)]
        [PXUIField(DisplayName = "Postal Code")]
        [PXZipValidation(typeof(Country.zipCodeRegexp), typeof(Country.zipCodeMask), countryIdField: typeof(countryID))]
		[PXPersonalDataField]
		public override String PostalCode { get; set; }
        #endregion
    }

	/// <summary>
	/// Represents an address that is specified in the document, such as
	/// <see cref="CROpportunity.OpportunityAddressID"> an opportunity's contact address</see>.
	/// A <see cref="CRAddress"/> record is as a copy of the default location <see cref="Address"/> of the business account
	/// and can be used to override the address specified in the document.
	/// A <see cref="CRAddress"/> record is independent from changes to the original <see cref="Address"/> record.
	/// </summary>
    [Serializable]
    [PXCacheName(Messages.CRAddress)]
    [DebuggerDisplay("{GetType().Name,nq} ({IsDefaultAddress}): AddressID = {AddressID,nq}, AddressLine1 = {AddressLine1}")]
    public partial class CRAddress : PX.Data.IBqlTable, IAddress, IAddressBase, IAddressLocation
    {
        #region Keys

		/// <summary>
		/// Primary Key
		/// </summary>
        public class PK : PrimaryKeyOf<CRAddress>.By<addressID>
        {
            public static CRAddress Find(PXGraph graph, int? addressID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, addressID, options);
        }

		/// <summary>
		/// Foreign Keys
		/// </summary>
        public static class FK
        {
			/// <summary>
			/// Business Account
			/// </summary>
            public class BusinessAccount : CR.BAccount.PK.ForeignKeyOf<CRAddress>.By<bAccountID> { }

			/// <summary>
			/// Business Account Address
			/// </summary>
            public class BusinessAccountAddress : CR.Address.PK.ForeignKeyOf<CRAddress>.By<bAccountAddressID> { }

			/// <summary>
			/// Country
			/// </summary>
            public class Country : CS.Country.PK.ForeignKeyOf<CRAddress>.By<countryID> { }

			/// <summary>
			/// State
			/// </summary>
            public class State : CS.State.PK.ForeignKeyOf<CRAddress>.By<countryID, state> { }
        }
        #endregion
        #region AddressID
        public abstract class addressID : PX.Data.BQL.BqlInt.Field<addressID> { }
        protected Int32? _AddressID;

		/// <inheritdoc cref="Address.AddressID" />
        [PXDBIdentity(IsKey = true)]
        [PXUIField(DisplayName = "Address ID", Visible = false)]
        public virtual Int32? AddressID
        {
            get
            {
                return this._AddressID;
            }
            set
            {
                this._AddressID = value;
            }
        }
        #endregion
        #region BAccountID
        public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
        protected Int32? _BAccountID;

		/// <inheritdoc cref="Address.BAccountID" />
        [PXDBInt()]
        [PXDBDefault(typeof(CROpportunity.bAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
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
        #region BAccountAddressID
        public abstract class bAccountAddressID : PX.Data.BQL.BqlInt.Field<bAccountAddressID> { }
        protected Int32? _BAccountAddressID;

		/// <summary>
		/// The identifier of the <see cref="Address"/> record from the referenced <see cref="BAccountID"/> record.
		/// </summary>
		/// <value>
		/// This field corresponds to the <see cref="BAccount.DefAddressID"/> field.
		/// </value>
        [PXDBInt()]
        public virtual Int32? BAccountAddressID
        {
            get
            {
                return this._BAccountAddressID;
            }
            set
            {
                this._BAccountAddressID = value;
            }
        }
        
        #endregion
        #region IsDefaultAddress
        public abstract class isDefaultAddress : PX.Data.BQL.BqlBool.Field<isDefaultAddress> { }
        protected Boolean? _IsDefaultAddress;

		/// <summary>
		/// If set to <see langword="true"/>, the field indicates that the address record is identical to
		/// the original <see cref="Address"/> record that is referenced by <see cref="BAccountAddressID"/>.
		/// </summary>
		/// <value>
		/// The default value is <see langword="false"/>.
		/// </value>
        [PXDBBool()]
        [PXUIField(DisplayName = "Default Customer Address", Visibility = PXUIVisibility.Visible)]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? IsDefaultAddress
        {
            get
            {
                return this._IsDefaultAddress;
            }
            set
            {
                this._IsDefaultAddress = value;
            }
        }
        #endregion
        #region OverrideAddress
        public abstract class overrideAddress : PX.Data.BQL.BqlBool.Field<overrideAddress> { }

		/// <summary>
		/// If set to <see langword="true"/>, the field indicates that the address overrides the default <see cref="Address"/> record,
		/// that is referenced by <see cref="BAccountAddressID"/>.
		/// </summary>
		/// <value>
		/// This field is the inverse of <see cref="IsDefaultAddress"/>.
		/// </value>
        [PXBool()]
        [PXUIField(DisplayName = "Override Address", Visibility = PXUIVisibility.Visible)]
        public virtual Boolean? OverrideAddress
        {
            [PXDependsOnFields(typeof(isDefaultAddress))]
            get
            {
                return (bool?)(this._IsDefaultAddress == null ? this._IsDefaultAddress : this._IsDefaultAddress == false);
            }
            set
            {
                this._IsDefaultAddress = (bool?)(value == null ? value : value == false);
            }
        }
        #endregion
        #region RevisionID
        public abstract class revisionID : PX.Data.BQL.BqlInt.Field<revisionID> { }
        protected Int32? _RevisionID;

		/// <inheritdoc cref="Address.RevisionID" />
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
        #region AddressLine1
        public abstract class addressLine1 : PX.Data.BQL.BqlString.Field<addressLine1> { }
        protected String _AddressLine1;

		/// <inheritdoc cref="Address.AddressLine1" />
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Address Line 1", Visibility = PXUIVisibility.SelectorVisible)]
		[PXPersonalDataField]
		public virtual String AddressLine1
        {
            get
            {
                return this._AddressLine1;
            }
            set
            {
                this._AddressLine1 = value;
            }
        }
        #endregion
        #region AddressLine2
        public abstract class addressLine2 : PX.Data.BQL.BqlString.Field<addressLine2> { }
        protected String _AddressLine2;

		/// <inheritdoc cref="Address.AddressLine2" />
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Address Line 2")]
		[PXPersonalDataField]
		public virtual String AddressLine2
        {
            get
            {
                return this._AddressLine2;
            }
            set
            {
                this._AddressLine2 = value;
            }
        }
        #endregion
        #region AddressLine3
        public abstract class addressLine3 : PX.Data.BQL.BqlString.Field<addressLine3> { }
        protected String _AddressLine3;

		/// <inheritdoc cref="Address.AddressLine3" />
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Address Line 3")]
		[PXPersonalDataField]
		public virtual String AddressLine3
        {
            get
            {
                return this._AddressLine3;
            }
            set
            {
                this._AddressLine3 = value;
            }
        }
        #endregion
        #region City
        public abstract class city : PX.Data.BQL.BqlString.Field<city> { }
        protected String _City;

		/// <inheritdoc cref="Address.City" />
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "City", Visibility = PXUIVisibility.SelectorVisible)]
		[PXPersonalDataField]
		public virtual String City
        {
            get
            {
                return this._City;
            }
            set
            {
                this._City = value;
            }
        }
        #endregion
        #region CountryID
        public abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }
        protected String _CountryID;

		/// <inheritdoc cref="Address.CountryID" />
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
        #region State
        public abstract class state : PX.Data.BQL.BqlString.Field<state> { }
        protected String _State;

		/// <inheritdoc cref="Address.State" />
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "State")]
        [State(typeof(countryID))]
        public virtual String State
        {
            get
            {
                return this._State;
            }
            set
            {
                this._State = value;
            }
        }
        #endregion
        #region PostalCode
        public abstract class postalCode : PX.Data.BQL.BqlString.Field<postalCode> { }
        protected String _PostalCode;

		/// <inheritdoc cref="Address.PostalCode" />
        [PXDBString(20)]
        [PXUIField(DisplayName = "Postal Code")]
        [PXZipValidation(typeof(Country.zipCodeRegexp), typeof(Country.zipCodeMask), countryIdField: typeof(countryID))]
		[PXPersonalDataField]
		public virtual String PostalCode
        {
            get
            {
                return this._PostalCode;
            }
            set
            {
                this._PostalCode = value;
            }
        }
        #endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXDBGuidNotNull]
		public virtual Guid? NoteID { get; set; }
		#endregion
        
        #region IsValidated
        public abstract class isValidated : PX.Data.BQL.BqlBool.Field<isValidated> { }
        protected Boolean? _IsValidated;

		/// <summary>
		/// If set to <see langword="true" />, the field indicates that the address has been successfully validated by Acumatica ERP.
		/// </summary>
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBBool()]
        [CS.ValidatedAddress()]
        [PXUIField(DisplayName = "Validated", FieldClass = CS.Messages.ValidateAddress)]
        public virtual Boolean? IsValidated
        {
            get
            {
                return this._IsValidated;
            }
            set
            {
                this._IsValidated = value;
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
        #region Latitude
        public abstract class latitude : PX.Data.BQL.BqlDecimal.Field<latitude> { }

		/// <inheritdoc cref="Address.Latitude" />
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Latitude", Visible = false)]
        public virtual decimal? Latitude { get; set; }
        #endregion
        #region Longitude
        public abstract class longitude : PX.Data.BQL.BqlDecimal.Field<longitude> { }

		/// <inheritdoc cref="Address.Longitude" />
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Longitude", Visible = false)]
        public virtual decimal? Longitude { get; set; }
        #endregion
    }

}
