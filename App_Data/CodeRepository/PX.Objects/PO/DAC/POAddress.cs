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

using PX.Objects.CR;

namespace PX.Objects.PO
{
	using System;
	using PX.CS.Contracts.Interfaces;
	using PX.Data;
	using PX.Data.ReferentialIntegrity.Attributes;
	using PX.Objects.CS;

	/// <summary>
	/// Represents an address that is used in purchase order management.
	/// </summary>
	/// <remarks>
	/// An address state is frozen at the moment of creation of a purchase order.
	/// Each modification to the original address leads to the generation of a new <see cref="revisionID">revision</see> of the address, which is used in the new purchase order or in the overridden address in an existed purchase order.
	/// If the <see cref="isDefaultAddress"/> field is <see langword="false"/>, the address has been overridden or the original address has been copied with the <see cref="revisionID">revision</see> related to the moment of creation.
	/// Also this is the base class for the following derived DACs:
	/// <list type="bullet">
	/// <item><description><see cref="POShipAddress"/>, which contains the information related to shipping of the ordered items</description></item>
	/// <item><description><see cref="PORemitAddress"/>, which contains the information about the vendor to supply the ordered items</description></item>
	/// </list>
	/// The records of this type are created and edited on the <i>Purchase Orders (PO301000)</i> form
	/// (which corresponds to the <see cref="POOrderEntry"/> graph).
	/// </remarks>
	[System.SerializableAttribute()]
    [PXCacheName(Messages.POAddress)]
	public partial class POAddress : PX.Data.IBqlTable, IAddress, IAddressLocation
	{
		#region Keys
		public class PK : PrimaryKeyOf<POAddress>.By<addressID>
		{
			public static POAddress Find(PXGraph graph, int? addressID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, addressID, options);
		}
		public static class FK
		{
			public class BAccount : CR.BAccount.PK.ForeignKeyOf<POAddress>.By<bAccountID> { }
			public class BAccountAddress : CR.Address.PK.ForeignKeyOf<POAddress>.By<bAccountAddressID> { }
			public class Country : CS.Country.PK.ForeignKeyOf<POAddress>.By<countryID> { }
			public class State : CS.State.PK.ForeignKeyOf<POAddress>.By<countryID, state> { }
		}
		#endregion

		#region AddressID
		/// <inheritdoc cref="AddressID"/>
		public abstract class addressID : PX.Data.BQL.BqlInt.Field<addressID> { }
		protected Int32? _AddressID;

		/// <summary>
		/// The identifier of the address.
		/// </summary>
		[PXDBIdentity(IsKey = true)]
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
		#region BAccountAddressID
		/// <inheritdoc cref="BAccountAddressID"/>
		public abstract class bAccountAddressID : PX.Data.BQL.BqlInt.Field<bAccountAddressID> { }
		protected Int32? _BAccountAddressID;

		/// <summary>
		/// The identifier of the <see cref="Address">business account address</see>.
		/// The field is included in <see cref="FK.BAccountAddress"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Address.addressID"/> field.
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
		/// <inheritdoc cref="IsDefaultAddress"/>
		public abstract class isDefaultAddress : PX.Data.BQL.BqlBool.Field<isDefaultAddress> { }
		protected Boolean? _IsDefaultAddress;

		/// <summary>
		/// If the value is <see langword="false" />, the address has been overridden or the original address has been copied with the revision related to the moment of creation.
		/// </summary>
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Is Default Address")]
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
		/// <inheritdoc cref="OverrideAddress"/>
		public abstract class overrideAddress : PX.Data.BQL.BqlBool.Field<overrideAddress> { }

		/// <summary>
		/// Specifies (if set to <see langword="true" />) that the address is overriden.
		/// </summary>
		/// <value>
		/// If the value of the <see cref="isDefaultAddress"/> field is <see langword="null"/>, the value of this field is <see langword="null"/>.
		/// If the value of the <see cref="isDefaultAddress"/> field is <see langword="true"/>, the value of this field is <see langword="false"/>.
		/// If the value of the <see cref="isDefaultAddress"/> field is <see langword="false"/>, the value of this field is <see langword="true"/>.
		/// </value>
		[PXBool()]
		[PXUIField(DisplayName = "Override", Visibility = PXUIVisibility.Visible)]
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
		/// <inheritdoc cref="RevisionID"/>
		public abstract class revisionID : PX.Data.BQL.BqlInt.Field<revisionID> { }
		protected Int32? _RevisionID;

		/// <summary>
		/// The identifier of the revision address.
		/// </summary>
		/// <remarks>
		/// Each modification to the original address leads to the generation of a new revision of the address, which is used in the new purchase order or in the overridden address in an existed purchase order.
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

		#region AddressLine1
		/// <inheritdoc cref="AddressLine1"/>
		public abstract class addressLine1 : PX.Data.BQL.BqlString.Field<addressLine1> { }
		protected String _AddressLine1;

		/// <summary>
		/// The first line of the address.
		/// </summary>
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
		/// <inheritdoc cref="AddressLine2"/>
		public abstract class addressLine2 : PX.Data.BQL.BqlString.Field<addressLine2> { }
		protected String _AddressLine2;

		/// <summary>
		/// The second line of the address.
		/// </summary>
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
		/// <inheritdoc cref="AddressLine3"/>
		public abstract class addressLine3 : PX.Data.BQL.BqlString.Field<addressLine3> { }
		protected String _AddressLine3;

		/// <summary>
		/// The third line of the address.
		/// </summary>
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
		/// <inheritdoc cref="City"/>
		public abstract class city : PX.Data.BQL.BqlString.Field<city> { }
		protected String _City;

		/// <summary>
		/// The city of the address.
		/// </summary>
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
		/// <inheritdoc cref="CountryID"/>
		public abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }
		protected String _CountryID;

		/// <summary>
		/// The identifier of the <see cref="Country">country</see>.
		/// The field is included in <see cref="FK.Country"/> and <see cref="FK.State"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Country.countryID"/> field.
		/// </value>
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
		/// <inheritdoc cref="State"/>
		public abstract class state : PX.Data.BQL.BqlString.Field<state> { }
		protected String _State;

		/// <summary>
		/// The identifier of the <see cref="State">state</see>.
		/// The field is included in <see cref="FK.State"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="State.stateID"/> field.
		/// </value>
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "State")]
		[CR.State(typeof(POAddress.countryID))]
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
		/// <inheritdoc cref="PostalCode"/>
		public abstract class postalCode : PX.Data.BQL.BqlString.Field<postalCode> { }
		protected String _PostalCode;

		/// <summary>
		/// The postal code of the address.
		/// </summary>
		[PXDBString(20)]
		[PXUIField(DisplayName = "Postal Code")]
		[PXZipValidation(typeof(Country.zipCodeRegexp), typeof(Country.zipCodeMask), countryIdField: typeof(POAddress.countryID))]
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
		/// <inheritdoc cref="NoteID"/>
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXDBGuidNotNull]
		public virtual Guid? NoteID { get; set; }
		#endregion
		#region IsValidated
		/// <inheritdoc cref="IsValidated"/>
		public abstract class isValidated : PX.Data.BQL.BqlBool.Field<isValidated> { }
		protected Boolean? _IsValidated;

		/// <summary>
		/// Specifies (if set to <see langword="true" />) that the address has been validated with a third-party specialized software or service.
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
		/// <inheritdoc cref="Latitude"/>
		public abstract class latitude : PX.Data.BQL.BqlDecimal.Field<latitude> { }

		/// <summary>
		/// The latitude of the address location.
		/// </summary>
		[PXDBDecimal(6)]
		[PXUIField(DisplayName = "Latitude", Visible = false)]
		public virtual decimal? Latitude { get; set; }
		#endregion
		#region Longitude
		/// <inheritdoc cref="Longitude"/>
		public abstract class longitude : PX.Data.BQL.BqlDecimal.Field<longitude> { }

		/// <summary>
		/// The longitude of the address location.
		/// </summary>
		[PXDBDecimal(6)]
		[PXUIField(DisplayName = "Longitude", Visible = false)]
		public virtual decimal? Longitude { get; set; }
		#endregion
	}
}
