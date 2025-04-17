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
using System.Diagnostics;
using PX.CS.Contracts.Interfaces;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CR.MassProcess;
using PX.Objects.CS;
using PX.Objects.Common.Bql;

namespace PX.Objects.CR
{

	/// <summary>
	/// Represents an address of the <see cref="BAccount">business account</see>, the <see cref="Contact">contact</see> or the <see cref="CRLead">lead</see>.
	/// </summary>
	/// <remarks>
	/// The records of this type are created and edited on the <i>Business Accounts (CR303000)</i>, <i>Contacts (CR302000)</i>, or <i>Leads (CR301000)</i> form,
	/// which correspond to the <see cref="BusinessAccountMaint"/>, <see cref="ContactMaint"/> or <see cref="LeadMaint"/> graph respectively.
	/// </remarks>
	[Serializable]
	[PXCacheName(Messages.Address)]
	[PXPersonalDataTable(typeof(
		Select<
			Address,
		Where<
			Address.addressID, Equal<Current<Contact.defAddressID>>>>))] // not in CacheExtension for test purposes
	[DebuggerDisplay("Address: AddressID = {AddressID,nq}, DisplayName = {DisplayName}")]
	public partial class Address : PX.Data.IBqlTable, IAddressBase, IValidatedAddress, IPXSelectable, INotable, IAddressLocation
	{
		#region Keys

		/// <summary>
		/// Primary Key
		/// </summary>
		public class PK : PrimaryKeyOf<Address>.By<addressID>
		{
			public static Address Find(PXGraph graph, int? addressID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, addressID, options);
		}

		/// <summary>
		/// Foreign Keys
		/// </summary>
		public static class FK
		{
			/// <summary>
			/// Country
			/// </summary>
			public class Country : CS.Country.PK.ForeignKeyOf<Address>.By<countryID> { }

			/// <summary>
			/// State
			/// </summary>
			public class State : CS.State.PK.ForeignKeyOf<Address>.By<countryID, state> { }
		}
		#endregion

		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected", Visibility = PXUIVisibility.Service)]
		public virtual bool? Selected { get; set; }
		#endregion
		#region AddressID
		public abstract class addressID : PX.Data.BQL.BqlInt.Field<addressID> { }
		protected Int32? _AddressID;

		/// <summary>
		/// The unique integer identifier of the record.
		/// This field is the key field.
		/// </summary>
		[PXDBIdentity(IsKey = true)]
		[PXUIField(DisplayName = "Address ID", Visible = false, Enabled = false, Visibility = PXUIVisibility.Invisible)]
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

		/// <summary>
		/// The identifier of the <see cref="BAccount"/> record that is specified in the business account to which the address belongs.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="BAccount.BAccountID"/> field.
		/// </value>
		[PXDBInt(IsKey = false)]
		[PXDBDefault(typeof(BAccount.bAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Business Account ID", Visible = false, Enabled = false, Visibility = PXUIVisibility.Invisible)]
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
		#region RevisionID
		public abstract class revisionID : PX.Data.BQL.BqlInt.Field<revisionID> { }
		protected Int32? _RevisionID;

		/// <summary>
		/// The revision ID of the original record.
		/// </summary>
		[PXDBInt()]
		[PXDefault(0)]
		[AddressRevisionID()]
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
		#region AddressType
		public abstract class addressType : PX.Data.BQL.BqlString.Field<addressType> { }
		protected String _AddressType;

		/// <summary>
		/// The type of the address.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in the <see cref="AddressTypes"/> class.
		/// The default value is <see cref="AddressTypes.BusinessAddress"/>.
		/// </value>
		[PXDBString(2, IsFixed = true)]
		[PXDefault(AddressTypes.BusinessAddress)]
		[AddressTypes.List()]
		[PXUIField(DisplayName = "AddressType", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String AddressType
		{
			get
			{
				return this._AddressType;
			}
			set
			{
				this._AddressType = value;
			}
		}
		#endregion
		#region DisplayName
		public abstract class displayName : PX.Data.BQL.BqlString.Field<displayName> { }

		/// <summary>
		/// The display name of the address as a concatenation of address lines (<see cref="AddressLine1"/>, <see cref="AddressLine2"/> and <see cref="AddressLine2"/> fields).
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Address", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBCalced(typeof(
			Switch<
				Case<
					Where<addressLine1, IsNotNull, And<addressLine2, IsNotNull, And<addressLine3, IsNotNull>>>,
					addressLine1.Concat<Space>.Concat<addressLine2>.Concat<Space>.Concat<addressLine3>,
				Case<
					Where<addressLine1, IsNotNull, And<addressLine2, IsNotNull, And<addressLine3, IsNull>>>,
					addressLine1.Concat<Space>.Concat<addressLine2>,
				Case<
					Where<addressLine1, IsNotNull, And<addressLine2, IsNull, And<addressLine3, IsNotNull>>>,
					addressLine1.Concat<Space>.Concat<addressLine3>,
				Case<
					Where<addressLine1, IsNull, And<addressLine2, IsNotNull, And<addressLine3, IsNotNull>>>,
					addressLine2.Concat<Space>.Concat<addressLine3>,
				Case<
					Where<addressLine1, IsNull, And<addressLine2, IsNull, And<addressLine3, IsNotNull>>>,
					addressLine3,
				Case<
					Where<addressLine1, IsNull, And<addressLine2, IsNotNull, And<addressLine3, IsNull>>>,
					addressLine2,
				Case<
					Where<addressLine1, IsNotNull, And<addressLine2, IsNull, And<addressLine3, IsNull>>>,
					addressLine1,
				Case<
					Where<addressLine1, IsNull, And<addressLine2, IsNull, And<addressLine3, IsNull>>>,
					Empty>>>>>>>>>
			), typeof(string)
		)]
		public virtual String DisplayName { get; set; }
		
		#endregion
		#region AddressLine1
		public abstract class addressLine1 : PX.Data.BQL.BqlString.Field<addressLine1> { }
		protected String _AddressLine1;

		/// <summary>
		/// The first line of the street address.
		/// </summary>
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Address Line 1", Visibility = PXUIVisibility.SelectorVisible)]
		[PXMassMergableField]
		[PXPersonalDataField]
		[PXContactInfoField]
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

		/// <summary>
		/// The second line of the street address.
		/// </summary>
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Address Line 2")]
		[PXMassMergableField]
		[PXPersonalDataField]
		[PXContactInfoField]
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

		/// <summary>
		/// The third line of the street address.
		/// </summary>
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Address Line 3")]
		[PXMassMergableField]
		[PXPersonalDataField]
		[PXContactInfoField]
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

		/// <summary>
		/// The name of the city or inhabited locality.
		/// </summary>
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "City", Visibility = PXUIVisibility.SelectorVisible)]
		[PXMassMergableField]
		[PXDeduplicationSearchField]
		[PXPersonalDataField]
		[PXContactInfoField]
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

		/// <summary>
		/// The identifier of the <see cref="Country"/> record.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Country.CountryID"/> field.
		/// </value>
		[PXDefault(typeof(Search<GL.Branch.countryID, Where<GL.Branch.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXDBString(100)]
		[PXUIField(DisplayName = "Country")]
		[Country]
		[PXMassMergableField]
		[PXDeduplicationSearchField]
		[PXContactInfoField]
		[PXForeignReference(typeof(FK.Country))]
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

		/// <summary>
		/// The string identifier of the state or province part of the address.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="State.StateID" /> field.
		/// </value>
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "State")]
		[State(typeof(Address.countryID))]
		[PXMassMergableField]
		[PXDeduplicationSearchField]
		[PXContactInfoField]
		[PXForeignReference(typeof(FK.State))]
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

		/// <summary>
		/// The postal code part of the address.
		/// </summary>
		[PXDBString(20)]
		[PXUIField(DisplayName = "Postal Code")]
		[PXZipValidation(typeof(Country.zipCodeRegexp), typeof(Country.zipCodeMask), countryIdField: typeof(Address.countryID))]
		[PXMassMergableField]
		[PXDeduplicationSearchField]
		[PXPersonalDataField]
		[PXContactInfoField]
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
		protected Guid? _NoteID;
		[PXUniqueNote(DescriptionField = typeof(Address.displayName))]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
		#region IsValidated
		public abstract class isValidated : PX.Data.BQL.BqlBool.Field<isValidated> { }
		protected Boolean? _IsValidated;

		/// <summary>
		/// If set to <see langword="true"/>, this field indicates that the address has been successfully validated by Acumatica ERP.
		/// </summary>
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBBool()]
		[CS.ValidatedAddress()]
		[PXUIField(DisplayName = "Validated", Enabled = false, FieldClass = CS.Messages.ValidateAddress)]
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
		#region TaxLocationCode
		public abstract class taxLocationCode : PX.Data.BQL.BqlString.Field<taxLocationCode> { }
        protected String _TaxLocationCode;

		/// <summary>
		/// The code of the location tax that is used in reports.
		/// </summary>
        [PXDBString(30, IsUnicode = true)]
        [PXUIField(DisplayName = "Tax Location Code")]
        public virtual String TaxLocationCode
        {
            get
            {
                return this._TaxLocationCode;
            }
            set
            {
                this._TaxLocationCode = value;
            }
        }
        #endregion
        #region TaxMunicipalCode
        public abstract class taxMunicipalCode : PX.Data.BQL.BqlString.Field<taxMunicipalCode> { }
        protected String _TaxMunicipalCode;

		/// <summary>
		/// The code of the municipal tax that is used in reports.
		/// </summary>
        [PXDBString(30, IsUnicode = true)]
        [PXUIField(DisplayName = "Tax Municipal Code")]
        public virtual String TaxMunicipalCode
        {
            get
            {
                return this._TaxMunicipalCode;
            }
            set
            {
                this._TaxMunicipalCode = value;
            }
        }
        #endregion
        #region TaxSchoolCode
        public abstract class taxSchoolCode : PX.Data.BQL.BqlString.Field<taxSchoolCode> { }
        protected String _TaxSchoolCode;

		/// <summary>
		/// The code of the school tax that is used in reports.
		/// </summary>
        [PXDBString(30, IsUnicode = true)]
        [PXUIField(DisplayName = "Tax School Code")]
        public virtual String TaxSchoolCode
        {
            get
            {
                return this._TaxSchoolCode;
            }
            set
            {
                this._TaxSchoolCode = value;
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

		#region AddressTypes
		public class AddressTypes
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
					 new string[] { BusinessAddress, HomeAddress, OtherAddress },
					 new string[] { Messages.BusinessAddress, Messages.HomeAddress, Messages.OtherAddress }) { ; }
			}

			public const string BusinessAddress = "BS";
			public const string HomeAddress = "HM";
			public const string OtherAddress = "UN";

			public class businessAddress : PX.Data.BQL.BqlString.Constant<businessAddress>
			{
				public businessAddress() : base(BusinessAddress) { ;}
			}
			public class homeAddress : PX.Data.BQL.BqlString.Constant<homeAddress>
			{
				public homeAddress() : base(HomeAddress) { ;}
			}
			public class otherAddress : PX.Data.BQL.BqlString.Constant<otherAddress>
			{
				public otherAddress() : base(OtherAddress) { ;}
			}
		}
		#endregion

		#region Latitude
		public abstract class latitude : PX.Data.BQL.BqlDecimal.Field<latitude> { }

		/// <summary>
		/// The latitude coordinates that are entered for a location if the location does not contain the postal address or the postal address cannot be validated.
		/// The field value is filled using the Google or Bing lookup.
		/// </summary>
		[PXDBDecimal(6)]
		[PXUIField(DisplayName = "Latitude", Visible = false)]
		public virtual decimal? Latitude { get; set; }
		#endregion
		#region Longitude
		public abstract class longitude : PX.Data.BQL.BqlDecimal.Field<longitude> { }

		/// <summary>
		/// The longitude coordinates that are entered for a location if the location does not contain the postal address or the postal address cannot be validated.
		/// The field value is filled using the Google or Bing lookup.
		/// </summary>
		[PXDBDecimal(6)]
		[PXUIField(DisplayName = "Longitude", Visible = false)]
		public virtual decimal? Longitude { get; set; }
		#endregion
	}

	[Serializable]
	[PXHidden]
	public partial class Address2 : Address
	{
		public new abstract class addressID : PX.Data.BQL.BqlInt.Field<addressID> { }
		public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		public new abstract class revisionID : PX.Data.BQL.BqlInt.Field<revisionID> { }
		public new abstract class addressType : PX.Data.BQL.BqlString.Field<addressType> { }
		public new abstract class displayName : PX.Data.BQL.BqlString.Field<displayName> { }
		public new abstract class addressLine1 : PX.Data.BQL.BqlString.Field<addressLine1> { }
		public new abstract class addressLine2 : PX.Data.BQL.BqlString.Field<addressLine2> { }
		public new abstract class addressLine3 : PX.Data.BQL.BqlString.Field<addressLine3> { }
		public new abstract class city : PX.Data.BQL.BqlString.Field<city> { }
		public new abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }
		public new abstract class state : PX.Data.BQL.BqlString.Field<state> { }
		public new abstract class postalCode : PX.Data.BQL.BqlString.Field<postalCode> { }
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		public new abstract class isValidated : PX.Data.BQL.BqlBool.Field<isValidated> { }
		public new abstract class taxLocationCode : PX.Data.BQL.BqlString.Field<taxLocationCode> { }
		public new abstract class taxMunicipalCode : PX.Data.BQL.BqlString.Field<taxMunicipalCode> { }
		public new abstract class taxSchoolCode : PX.Data.BQL.BqlString.Field<taxSchoolCode> { }
		public new abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		public new abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		public new abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		public new abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		public new abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		public new abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		public new abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		public new abstract class latitude : PX.Data.BQL.BqlDecimal.Field<latitude> { }
		public new abstract class longitude : PX.Data.BQL.BqlDecimal.Field<longitude> { }
	}
}
