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
using PX.Objects.CR;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.CS.Contracts.Interfaces;
using PX.Objects.CR.MassProcess;
using PX.Objects.AR;


namespace PX.Objects.PM
{
	/// <summary>Represents a billing address that is specified in the <see cref="PMProject">project</see> for billing purposes. These settings are initially populated with the information
	/// specified on the <strong>Billing Settings</strong> tab of the Customers (AR303000) form (as a copy of the default customer location's <see cref="Address">address</see>), but
	/// you can override any of the default settings. The record is independent of changes to the original <see cref="Address">address</see> record. The entities of this type are
	/// created and edited on the Projects (PM301000) form (which corresponds to the <see cref="ProjectEntry" /> graph)</summary>
	[System.SerializableAttribute()]
	[PXCacheName(Messages.PMAddress)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PMAddress : PX.Data.IBqlTable, IAddress, IAddressBase, IAddressLocation
	{
		#region AddressID
		public abstract class addressID : PX.Data.BQL.BqlInt.Field<addressID> { }
		protected Int32? _AddressID;
		/// <summary>
		/// The unique integer identifier of the record.
		/// This field is the key field.
		/// </summary>
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
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		protected Int32? _CustomerID;
		/// <summary>
		/// The identifier of the <see cref="Customer"/> record, 
		/// which is specified in the document to which the address belongs.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Customer.BAccountID"/> field.
		/// </value>
		[PXDBInt()]
		public virtual Int32? CustomerID
		{
			get
			{
				return this._CustomerID;
			}
			set
			{
				this._CustomerID = value;
			}
		}
		#endregion
		#region BAccountID
		public virtual Int32? BAccountID
		{
			get
			{
				return this._CustomerID;
			}
			set
			{
				this._CustomerID = value;
			}
		}
		#endregion
		#region CustomerLocationID
		public abstract class customerAddressID : PX.Data.BQL.BqlInt.Field<customerAddressID> { }
		protected Int32? _CustomerAddressID;
		/// <summary>
		/// The identifier of the <see cref="Address"/> record from which 
		/// the address was originally created.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Address.AddressID"/> field.
		/// </value>
		[PXDBInt()]
		public virtual Int32? CustomerAddressID
		{
			get
			{
				return this._CustomerAddressID;
			}
			set
			{
				this._CustomerAddressID = value;
			}
		}
		/// <summary>
		/// An alias for <see cref="PMAddress.CustomerAddressID"/>,
		/// which exists for the purpose of implementing the 
		/// <see cref="IAddress"/> interface.
		/// </summary>
		public virtual Int32? BAccountAddressID
		{
			get
			{
				return this._CustomerAddressID;
			}
			set
			{
				this._CustomerAddressID = value;
			}
		}
		#endregion
		#region IsDefaultBillAddress
		public abstract class isDefaultBillAddress : PX.Data.BQL.BqlBool.Field<isDefaultBillAddress> { }
		protected Boolean? _IsDefaultBillAddress;
		/// <summary>
		/// If set to <c>true</c>, indicates that the address record 
		/// is identical to the original <see cref="Address"/>
		/// record, which is referenced by <see cref="CustomerAddressID"/>.
		/// </summary>
		[PXDBBool()]
		[PXUIField(DisplayName = "Customer Default", Visibility = PXUIVisibility.Visible)]
		[PXDefault(true)]
		public virtual Boolean? IsDefaultBillAddress
		{
			get
			{
				return this._IsDefaultBillAddress;
			}
			set
			{
				this._IsDefaultBillAddress = value;
			}
		}
		/// <summary>
		/// An alias for <see cref="IsDefaultBillAddress"/>,
		/// which exists for the purpose of implementing the
		/// <see cref="IAddress"/> interface.
		/// </summary>
		public virtual Boolean? IsDefaultAddress
		{
			get
			{
				return this._IsDefaultBillAddress;
			}
			set
			{
				this._IsDefaultBillAddress = value;
			}
		}
		#endregion
		#region OverrideAddress
		public abstract class overrideAddress : PX.Data.BQL.BqlBool.Field<overrideAddress> { }
		/// <summary>
		/// If set to <c>true</c>, indicates that the address
		/// overrides the default <see cref="Address"/> record, which is
		/// referenced by <see cref="CustomerAddressID"/>. This field 
		/// is the inverse of <see cref="IsDefaultBillAddress"/>.
		/// </summary>
		[PXBool()]
		[PXUIField(DisplayName = "Override Address", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? OverrideAddress
		{
			[PXDependsOnFields(typeof(isDefaultBillAddress))]
			get
			{
				return (bool?)(this._IsDefaultBillAddress == null ? this._IsDefaultBillAddress : this._IsDefaultBillAddress == false);
			}
			set
			{
				this._IsDefaultBillAddress = (bool?)(value == null ? value : value == false);
			}
		}
		#endregion
		#region RevisionID
		public abstract class revisionID : PX.Data.BQL.BqlInt.Field<revisionID> { }
		protected Int32? _RevisionID;
		/// <summary>
		/// The revision ID of the original <see cref="Address"/> record
		/// from which the record originates.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Address.RevisionID"/> field.
		/// </value>
		[PXDBInt()]
		[PXDefault()]
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
		/// <summary>
		/// The first address line.
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
		public abstract class addressLine2 : PX.Data.BQL.BqlString.Field<addressLine2> { }
		protected String _AddressLine2;
		/// <summary>
		/// The second address line.
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
		public abstract class addressLine3 : PX.Data.BQL.BqlString.Field<addressLine3> { }
		protected String _AddressLine3;
		/// <summary>
		/// The third address line.
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
		public abstract class city : PX.Data.BQL.BqlString.Field<city> { }
		protected String _City;
		/// <summary>
		/// The name of the city or inhabited locality.
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
		public abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }
		protected String _CountryID;
		/// <summary>
		/// The identifier of the <see cref="Country"/> record.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Country.CountryID"/> field.
		/// </value>
		[PXDefault(typeof(Search<GL.Branch.countryID, Where<GL.Branch.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Country")]
		[PXSelector(typeof(Search<Country.countryID>), DescriptionField = typeof(Country.description), CacheGlobal = true)]
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
		/// The name of the state.
		/// </summary>
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "State")]
		[State(typeof(PMAddress.countryID), DescriptionField = typeof(State.name))]
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
		/// The postal code.
		/// </summary>
		[PXDBString(20)]
		[PXUIField(DisplayName = "Postal Code")]
		[PXZipValidation(typeof(Country.zipCodeRegexp), typeof(Country.zipCodeMask), countryIdField: typeof(PMAddress.countryID))]
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
		/// If set to <c>true</c>, indicates that the address has been
		/// successfully validated by Acumatica.
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

		/// <summary>
		/// The latitude of the address.
		/// </summary>
		[PXDBDecimal(6)]
		[PXUIField(DisplayName = "Latitude", Visible = false)]
		public virtual decimal? Latitude { get; set; }
		#endregion
		#region Longitude
		public abstract class longitude : PX.Data.BQL.BqlDecimal.Field<longitude> { }

		/// <summary>
		/// The longitude of the address.
		/// </summary>
		[PXDBDecimal(6)]
		[PXUIField(DisplayName = "Longitude", Visible = false)]
		public virtual decimal? Longitude { get; set; }
		#endregion
	}

	/// <summary>
	/// Represents a pro forma shipping address. The records of this type are created and edited on the Pro Forma Invoices (PM307000) form
	/// (which corresponds to the <see cref="ProformaEntry" /> graph).
	/// The DAC is based on the <see cref="PMAddress" /> DAC.
	/// </summary>
	[Serializable]
	[PXCacheName(Messages.PMAddress)]
	public class PMShippingAddress : PMAddress
	{
		public new abstract class addressID : PX.Data.BQL.BqlInt.Field<addressID> { }
		public new abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		public new abstract class customerAddressID : PX.Data.BQL.BqlInt.Field<customerAddressID> { }
		public new abstract class isDefaultBillAddress : PX.Data.BQL.BqlBool.Field<isDefaultBillAddress> { }
		public new abstract class overrideAddress : PX.Data.BQL.BqlBool.Field<overrideAddress> { }
		public new abstract class revisionID : PX.Data.BQL.BqlInt.Field<revisionID> { }
		public new abstract class addressLine1 : PX.Data.BQL.BqlString.Field<addressLine1> { }
		public new abstract class addressLine2 : PX.Data.BQL.BqlString.Field<addressLine2> { }
		public new abstract class addressLine3 : PX.Data.BQL.BqlString.Field<addressLine3> { }
		public new abstract class city : PX.Data.BQL.BqlString.Field<city> { }
		public new abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }
		public new abstract class state : PX.Data.BQL.BqlString.Field<state> { }
		public new abstract class postalCode : PX.Data.BQL.BqlString.Field<postalCode> { }
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		public new abstract class isValidated : PX.Data.BQL.BqlBool.Field<isValidated> { }
	}

	/// <summary>Represents a project site address. The records of this type are created and edited on the Projects (PM301000) form (which corresponds to the <see cref="ProjectEntry" />
	/// graph). The DAC is based on the <see cref="PMAddress" /> DAC.</summary>
	[Serializable]
	[PXCacheName(Messages.PMAddress)]
	[PXBreakInheritance]
	public partial class PMSiteAddress : PMAddress
	{
		public new abstract class addressID : PX.Data.BQL.BqlInt.Field<addressID> { }
		public new abstract class overrideAddress : PX.Data.BQL.BqlBool.Field<overrideAddress> { }
		#region RevisionID
		public new abstract class revisionID : PX.Data.BQL.BqlInt.Field<revisionID> { }

		/// <summary>
		/// The revision ID of the original <see cref="Address"/> record
		/// from which the record originates.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Address.RevisionID"/> field.
		/// </value>
		[PXDBInt()]
		[PXDefault(0)]
		public override Int32? RevisionID
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
		public new abstract class addressLine1 : PX.Data.BQL.BqlString.Field<addressLine1> { }

		/// <summary>
		/// The first address line.
		/// </summary>
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Address Line 1")]
		[PXPersonalDataField]
		public override String AddressLine1
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
		public new abstract class addressLine2 : PX.Data.BQL.BqlString.Field<addressLine2> { }

		/// <summary>
		/// The second address line.
		/// </summary>
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Address Line 2")]
		[PXPersonalDataField]
		public override String AddressLine2
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
		#region City
		public new abstract class city : PX.Data.BQL.BqlString.Field<city> { }
		/// <summary>
		/// The name of the city or inhabited locality.
		/// </summary>
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "City")]
		[PXPersonalDataField]
		public override string City
		{
			get;
			set;
		}
		#endregion
		#region CountryID
		public new abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }
		/// <summary>
		/// The identifier of the <see cref="Country"/> record.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Country.CountryID"/> field.
		/// </value>
		[PXDBString(2, IsUnicode = true)]
		[PXUIField(DisplayName = "Country")]
		[Country]
		public override string CountryID
		{
			get;
			set;
		}
		#endregion
		#region State
		public new abstract class state : PX.Data.BQL.BqlString.Field<state> { }
		/// <summary>
		/// The name of the state.
		/// </summary>
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "State")]
		[State(typeof(PMSiteAddress.countryID))]
		public override string State
		{
			get;
			set;
		}
		#endregion
		#region PostalCode
		public new abstract class postalCode : PX.Data.BQL.BqlString.Field<postalCode> { }
		/// <summary>
		/// The postal code.
		/// </summary>
		[PXDBString(20, IsUnicode = false)]
		[PXUIField(DisplayName = "Postal Code")]
		[PXZipValidation(typeof(Country.zipCodeRegexp), typeof(Country.zipCodeMask), typeof(PMSiteAddress.countryID))]
		[PXPersonalDataField]
		public override string PostalCode
		{
			get;
			set;
		}
		#endregion
		#region Latitude
		public new abstract class latitude : PX.Data.BQL.BqlDecimal.Field<latitude> { }

		/// <summary>
		/// The latitude of the address.
		/// </summary>
		[PXDBDecimal(6, MaxValue = 90f, MinValue = -90f)]
		[PXUIField(DisplayName = "Latitude")]
		public override decimal? Latitude
		{
			get;
			set;
		}
		#endregion
		#region Longitude
		public new abstract class longitude : PX.Data.BQL.BqlDecimal.Field<longitude> { }

		/// <summary>
		/// The longitude of the address.
		/// </summary>
		[PXDBDecimal(6, MaxValue = 180f, MinValue = -180f)]
		[PXUIField(DisplayName = "Longitude")]
		public override decimal? Longitude
		{
			get;
			set;
		}
		#endregion
	}
}
