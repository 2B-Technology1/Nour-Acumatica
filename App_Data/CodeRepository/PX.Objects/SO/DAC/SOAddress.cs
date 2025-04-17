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
using PX.CS.Contracts.Interfaces;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.SO
{
	/// <summary>
	/// Represents an address that contains the information related to the billing of the ordered items.
	/// </summary>
	/// <remarks>
	/// The records of this type are created and edited on the following forms:
	/// <list type="bullet">
	/// <item><description><i>Invoices (SO303000)</i> (which corresponds to the <see cref="SOInvoiceEntry"/> graph)</description></item>
	/// <item><description><i>Sales Orders (SO301000)</i> (which corresponds to the <see cref="SOOrderEntry"/> graph)</description></item>
	/// </list>
	/// </remarks>
	[Serializable()]
	[PXBreakInheritance]
	[PXCacheName(Messages.BillingAddress)]
	public partial class SOBillingAddress : SOAddress
	{
		#region Keys
		public new class PK : PrimaryKeyOf<SOBillingAddress>.By<addressID>
		{
			public static SOBillingAddress Find(PXGraph graph, int? addressID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, addressID, options);
		}
		public new static class FK
		{
			public class Customer : AR.Customer.PK.ForeignKeyOf<SOBillingAddress>.By<customerID> { }
			public class CustomerAddress : AR.ARAddress.PK.ForeignKeyOf<SOBillingAddress>.By<customerAddressID> { }
			public class Country : CS.Country.PK.ForeignKeyOf<SOBillingAddress>.By<countryID> { }
			public class State : CS.State.PK.ForeignKeyOf<SOBillingAddress>.By<countryID, state> { }
		}
		#endregion

		#region AddressID
		/// <inheritdoc cref="SOAddress.addressID"/>
		public new abstract class addressID : PX.Data.BQL.BqlInt.Field<addressID> { }
		#endregion
		#region CustomerID
		/// <inheritdoc cref="SOAddress.customerID"/>
		public new abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		#endregion
		#region CustomerAddressID
		/// <inheritdoc cref="SOAddress.customerAddressID"/>
		public new abstract class customerAddressID : PX.Data.BQL.BqlInt.Field<customerAddressID> { }
		#endregion
		#region IsDefaultAddress
		/// <inheritdoc cref="SOAddress.isDefaultAddress"/>
		public new abstract class isDefaultAddress : PX.Data.BQL.BqlBool.Field<isDefaultAddress> { }
		#endregion
		#region OverrideAddress
		/// <inheritdoc cref="SOAddress.overrideAddress"/>
		public new abstract class overrideAddress : PX.Data.BQL.BqlBool.Field<overrideAddress> { }
		#endregion
		#region RevisionID
		/// <inheritdoc cref="SOAddress.revisionID"/>
		public new abstract class revisionID : PX.Data.BQL.BqlInt.Field<revisionID> { }
		#endregion
		#region CountryID
		/// <inheritdoc cref="CountryID"/>
		public new abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }

		/// <inheritdoc cref="SOAddress.countryID"/>
		[PXDefault(typeof(Search<GL.Branch.countryID, Where<GL.Branch.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXDBString(100)]
		[PXUIField(DisplayName = "Country")]
		[Country]
		public override String CountryID
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
		public new abstract class state : PX.Data.BQL.BqlString.Field<state> { }

		/// <inheritdoc cref="SOAddress.state"/>
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "State")]
		[CR.State(typeof(SOBillingAddress.countryID))]
		public override String State
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
		public new abstract class postalCode : PX.Data.BQL.BqlString.Field<postalCode> { }

		/// <inheritdoc cref="SOAddress.postalCode"/>
		[PXDBString(20)]
		[PXUIField(DisplayName = "Postal Code")]
		[PXPersonalDataField]
		[PXZipValidation(typeof(Country.zipCodeRegexp), typeof(Country.zipCodeMask), countryIdField: typeof(SOBillingAddress.countryID))]
		public override String PostalCode
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
		#region IsValidated
		/// <inheritdoc cref="SOAddress.isValidated"/>
		public new abstract class isValidated : PX.Data.BQL.BqlBool.Field<isValidated> { }
		#endregion
	}

	/// <summary>
	/// Represents an address that contains the information related to the shipping of the ordered items.
	/// </summary>
	/// <remarks>
	/// The records of this type are created and edited on the following forms:
	/// <list type="bullet">
	/// <item><description><i>Invoices (SO303000)</i> (which corresponds to the <see cref="SOInvoiceEntry"/> graph)</description></item>
	/// <item><description><i>Sales Orders (SO301000)</i> (which corresponds to the <see cref="SOOrderEntry"/> graph)</description></item>
	/// </list>
	/// </remarks>
	[Serializable()]
	[PXBreakInheritance]
	[PXCacheName(Messages.ShippingAddress)]
	public partial class SOShippingAddress : SOAddress
	{
		#region Keys
		public new class PK : PrimaryKeyOf<SOShippingAddress>.By<addressID>
		{
			public static SOShippingAddress Find(PXGraph graph, int? addressID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, addressID, options);
		}
		public new static class FK
		{
			public class Customer : AR.Customer.PK.ForeignKeyOf<SOShippingAddress>.By<customerID> { }
			public class CustomerAddress : AR.ARAddress.PK.ForeignKeyOf<SOShippingAddress>.By<customerAddressID> { }
			public class Country : CS.Country.PK.ForeignKeyOf<SOShippingAddress>.By<countryID> { }
			public class State : CS.State.PK.ForeignKeyOf<SOShippingAddress>.By<countryID, state> { }
		}
		#endregion

		#region AddressID
		/// <inheritdoc cref="SOAddress.addressID"/>
		public new abstract class addressID : PX.Data.BQL.BqlInt.Field<addressID> { }
		#endregion
		#region CustomerID
		/// <inheritdoc cref="SOAddress.customerID"/>
		public new abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		#endregion
		#region CustomerAddressID
		/// <inheritdoc cref="SOAddress.customerAddressID"/>
		public new abstract class customerAddressID : PX.Data.BQL.BqlInt.Field<customerAddressID> { }
		#endregion
		#region IsDefaultAddress
		/// <inheritdoc cref="SOAddress.isDefaultAddress"/>
		public new abstract class isDefaultAddress : PX.Data.BQL.BqlBool.Field<isDefaultAddress> { }
		#endregion
		#region OverrideAddress
		/// <inheritdoc cref="SOAddress.overrideAddress"/>
		public new abstract class overrideAddress : PX.Data.BQL.BqlBool.Field<overrideAddress> { }
		#endregion
		#region RevisionID
		/// <inheritdoc cref="SOAddress.revisionID"/>
		public new abstract class revisionID : PX.Data.BQL.BqlInt.Field<revisionID> { }
		#endregion
		#region CountryID
		/// <inheritdoc cref="CountryID"/>
		public new abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }

		/// <inheritdoc cref="SOAddress.countryID"/>
		[PXDefault(typeof(Search<GL.Branch.countryID, Where<GL.Branch.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXDBString(100)]
		[PXUIField(DisplayName = "Country")]
		[Country]
		public override String CountryID
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
		public new abstract class state : PX.Data.BQL.BqlString.Field<state> { }

		/// <inheritdoc cref="SOAddress.state"/>
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "State")]
		[CR.State(typeof(SOShippingAddress.countryID))]
		public override String State
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
		public new abstract class postalCode : PX.Data.BQL.BqlString.Field<postalCode> { }

		/// <inheritdoc cref="SOAddress.postalCode"/>
		[PXDBString(20)]
		[PXUIField(DisplayName = "Postal Code")]
		[PXPersonalDataField]
		[PXZipValidation(typeof(Country.zipCodeRegexp), typeof(Country.zipCodeMask), countryIdField: typeof(SOShippingAddress.countryID))]
		public override String PostalCode
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
		#region IsValidated
		/// <inheritdoc cref="SOAddress.isValidated"/>
		public new abstract class isValidated : PX.Data.BQL.BqlBool.Field<isValidated> { }
		#endregion
		#region Latitude
		/// <inheritdoc cref="SOAddress.latitude"/>
		public new abstract class latitude : PX.Data.BQL.BqlDecimal.Field<latitude> { }
		#endregion
		#region Longitude
		/// <inheritdoc cref="SOAddress.longitude"/>
		public new abstract class longitude : PX.Data.BQL.BqlDecimal.Field<longitude> { }
		#endregion
	}

	/// <summary>
	/// Represents an address that contains the information related to the shipping of the ordered items.
	/// </summary>
	/// <remarks>
	/// The records of this type are created and edited on the <i>Shipments (SO302000)</i> form
	/// (which corresponds to the <see cref="SOShipmentEntry"/> graph).
	/// </remarks>
	[Serializable()]
	[PXBreakInheritance]
	[PXCacheName(Messages.ShipmentAddress)]
	public partial class SOShipmentAddress : SOAddress
	{
		#region Keys
		public new class PK : PrimaryKeyOf<SOShipmentAddress>.By<addressID>
		{
			public static SOShipmentAddress Find(PXGraph graph, int? addressID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, addressID, options);
		}
		public new static class FK
		{
			public class Customer : AR.Customer.PK.ForeignKeyOf<SOShipmentAddress>.By<customerID> { }
			public class CustomerAddress : AR.ARAddress.PK.ForeignKeyOf<SOShipmentAddress>.By<customerAddressID> { }
			public class Country : CS.Country.PK.ForeignKeyOf<SOShipmentAddress>.By<countryID> { }
			public class State : CS.State.PK.ForeignKeyOf<SOShipmentAddress>.By<countryID, state> { }
		}
		#endregion

		#region AddressID
		/// <inheritdoc cref="SOAddress.addressID"/>
		public new abstract class addressID : PX.Data.BQL.BqlInt.Field<addressID> { }
		#endregion
		#region CustomerID
		/// <inheritdoc cref="CustomerID"/>
		public new abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

		/// <inheritdoc cref="SOAddress.customerID"/>
		[PXDBInt()]
		[PXDBDefault(typeof(SOShipment.customerID))]
		public override Int32? CustomerID
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
		#region CustomerAddressID
		/// <inheritdoc cref="SOAddress.customerAddressID"/>
		public new abstract class customerAddressID : PX.Data.BQL.BqlInt.Field<customerAddressID> { }
		#endregion
		#region IsDefaultAddress
		/// <inheritdoc cref="SOAddress.isDefaultAddress"/>
		public new abstract class isDefaultAddress : PX.Data.BQL.BqlBool.Field<isDefaultAddress> { }
		#endregion
		#region OverrideAddress
		/// <inheritdoc cref="SOAddress.overrideAddress"/>
		public new abstract class overrideAddress : PX.Data.BQL.BqlBool.Field<overrideAddress> { }
		#endregion
		#region RevisionID
		/// <inheritdoc cref="SOAddress.revisionID"/>
		public new abstract class revisionID : PX.Data.BQL.BqlInt.Field<revisionID> { }
		#endregion
		#region CountryID
		/// <inheritdoc cref="CountryID"/>
		public new abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }

		/// <inheritdoc cref="SOAddress.countryID"/>
		[PXDefault(typeof(Search<GL.Branch.countryID, Where<GL.Branch.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXDBString(100)]
		[PXUIField(DisplayName = "Country")]
		[Country]
		public override String CountryID
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
		public new abstract class state : PX.Data.BQL.BqlString.Field<state> { }

		/// <inheritdoc cref="SOAddress.state"/>
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "State")]
		[CR.State(typeof(SOShipmentAddress.countryID))]
		public override String State
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
		/// <inheritdoc cref="SOAddress.postalCode"/>
		public new abstract class postalCode : PX.Data.BQL.BqlString.Field<postalCode> { }

		/// <inheritdoc cref="PostalCode"/>
		[PXDBString(20)]
		[PXUIField(DisplayName = "Postal Code")]
		[PXPersonalDataField]
		[PXZipValidation(typeof(Country.zipCodeRegexp), typeof(Country.zipCodeMask), countryIdField: typeof(SOShipmentAddress.countryID))]
		public override String PostalCode
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
		#region IsValidated
		/// <inheritdoc cref="SOAddress.isValidated"/>
		public new abstract class isValidated : PX.Data.BQL.BqlBool.Field<isValidated> { }
		#endregion
		#region Latitude
		/// <inheritdoc cref="SOAddress.latitude"/>
		public new abstract class latitude : PX.Data.BQL.BqlDecimal.Field<latitude> { }
		#endregion
		#region Longitude
		/// <inheritdoc cref="SOAddress.longitude"/>
		public new abstract class longitude : PX.Data.BQL.BqlDecimal.Field<longitude> { }
		#endregion
	}

	/// <summary>
	/// Represents an address that is used in sales order management.
	/// </summary>
	/// <remarks>
	/// An address state is frozen at the moment of creation of a sales order.
	/// Each modification to the original address leads to the generation of a new <see cref="revisionID">revision</see> of the address, which is used in the new sales order or in the overridden address in an existed sales order.
	/// If the <see cref="isDefaultAddress"/> field is <see langword="false"/>, the address has been overridden or the original address has been copied with the <see cref="revisionID">revision</see> related to the moment of creation.
	/// Also this is the base class for the following derived DACs:
	/// <list type="bullet">
	/// <item><description><see cref="SOShipmentAddress"/>, which contains the information related to shipping of the ordered items</description></item>
	/// <item><description><see cref="SOShippingAddress"/>, which contains the information related to shipping of the ordered items</description></item>
	/// <item><description><see cref="SOBillingAddress"/>, which contains the information related to billing of the ordered items</description></item>
	/// </list>
	/// The records of this type are created and edited on the following forms:
	/// <list type="bullet">
	/// <item><description><i>Invoices (SO303000)</i> (which corresponds to the <see cref="SOInvoiceEntry"/> graph)</description></item>
	/// <item><description><i>Shipments (SO302000)</i> (which corresponds to the <see cref="SOShipmentEntry"/> graph)</description></item>
	/// <item><description><i>Sales Orders (SO301000)</i> (which corresponds to the <see cref="SOOrderEntry"/> graph)</description></item>
	/// </list>
	/// </remarks>
	[Serializable()]
	[PXCacheName(Messages.SOAddress)]
	public partial class SOAddress : PX.Data.IBqlTable, IAddress, IAddressBase, IAddressLocation
	{
		#region Keys
		public class PK : PrimaryKeyOf<SOAddress>.By<addressID>
		{
			public static SOAddress Find(PXGraph graph, int? addressID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, addressID, options);
		}
		public static class FK
		{
			public class Customer : AR.Customer.PK.ForeignKeyOf<SOAddress>.By<customerID> { }
			public class CustomerAddress : AR.ARAddress.PK.ForeignKeyOf<SOAddress>.By<customerAddressID> { }
			public class Country : CS.Country.PK.ForeignKeyOf<SOAddress>.By<countryID> { }
			public class State : CS.State.PK.ForeignKeyOf<SOAddress>.By<countryID, state> { }
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
		/// <inheritdoc cref="CustomerID"/>
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		protected Int32? _CustomerID;

		/// <summary>
		/// The identifier of the <see cref="AR.Customer">customer</see>.
		/// The field is included in <see cref="FK.Customer"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="AR.Customer.bAccountID"/> field.
		/// </value>
		[PXDBInt()]
		[PXDBDefault(typeof(SOOrder.customerID))]
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

		/// <inheritdoc cref="CustomerID"/>
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
		#region CustomerAddressID
		/// <inheritdoc cref="CustomerAddressID"/>
		public abstract class customerAddressID : PX.Data.BQL.BqlInt.Field<customerAddressID> { }
		protected Int32? _CustomerAddressID;

		/// <summary>
		/// The identifier of the <see cref="AR.ARAddress">customer address</see>.
		/// The field is included in <see cref="FK.CustomerAddress"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="AR.ARAddress.addressID"/> field.
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

		/// <inheritdoc cref="CustomerAddressID"/>
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
		#region IsDefaultAddress
		/// <inheritdoc cref="IsDefaultAddress"/>
		public abstract class isDefaultAddress : PX.Data.BQL.BqlBool.Field<isDefaultAddress> { }
		protected Boolean? _IsDefaultAddress;

		/// <summary>
		/// Specifies (if set to <see langword="true" />) that the address is default.
		/// If the value is <see langword="false" />, the address has been overridden or the original address has been copied with the revision related to the moment of creation.
		/// </summary>
		[PXDBBool()]
		[PXUIField(DisplayName = "Default Customer Address", Visibility = PXUIVisibility.Visible)]
		[PXDefault(true)]
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
		#region IsEncrypted
		/// <inheritdoc cref="IsEncrypted"/>
		public abstract class isEncrypted : PX.Data.BQL.BqlBool.Field<isEncrypted> { }

		/// <summary>
		/// Indicates if  Personally identifiable Information fields
		/// (example AddressLine1 , AddressLine2 etc.) are encrypted or not.
		/// </summary>
		[PXDBBool]
		public virtual bool? IsEncrypted { get; set; }

		#endregion
		#region RevisionID
		/// <inheritdoc cref="RevisionID"/>
		public abstract class revisionID : PX.Data.BQL.BqlInt.Field<revisionID> { }
		protected Int32? _RevisionID;

		/// <summary>
		/// The identifier of the revision address.
		/// </summary>
		/// <remarks>
		/// Each modification to the original address leads to the generation of a new revision of the address, which is used in the new sales order or in the overridden address in an existed sales order.
		/// </remarks>
		[PXDBInt()]
		[PXDefault()]
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
		[CR.State(typeof(SOAddress.countryID))]
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
		[PXZipValidation(typeof(Country.zipCodeRegexp), typeof(Country.zipCodeMask), countryIdField: typeof(SOAddress.countryID))]
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
