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
using PX.Objects.AR;
using PX.Objects.CR.MassProcess;
using PX.Objects.GL;
using PX.Objects.PO;
using PX.Objects.TX;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.SO;
using PX.Objects.AP;
using PX.Objects.CA;
using PX.Objects.CR.Workflows;

namespace PX.Objects.CR
{
	/// <summary>
	/// The location of a business account, a customer or a vendor with specific settings for entities such as sales orders, purchase orders, and invoices.
	/// </summary>
	/// <remarks>
	/// An user can specify more than one location for a business account, a customer, or a vendor, and each location can have specific settings
	/// such as the following:
	/// <list type="bullet">
	/// <item><description>Accounts payable and expense accounts and the corresponding subaccounts</description></item>
	/// <item><description>Payment settings and remittance information</description></item>
	/// <item><description>Purchasing and shipping-related information</description></item>
	/// <item><description>Tax-related information</description></item>
	/// </list>
	/// The records of this type are created and edited on the <i>Account Locations (CR303010)</i>, <i>Customer Locations (AR303020)</i>, and <i>Vendor Locations (AP303010)</i> forms,
	/// which correspond to the <see cref="AccountLocationMaint"/>, <see cref="CustomerLocationMaint"/>, and <see cref="VendorLocationMaint"/> graphs respectively.
	/// </remarks>
	[System.SerializableAttribute()]
	[PXPrimaryGraph(new Type[] {
					typeof(VendorLocationMaint),
					typeof(CustomerLocationMaint),
					typeof(BranchMaint),
					typeof(EP.EmployeeMaint),
					typeof(AccountLocationMaint)},
				new Type[] {
					typeof(Select<Location, 
						Where<Location.bAccountID, Equal<Current<Location.bAccountID>>, And<Location.locationID, Equal<Current<Location.locationID>>, 
						And<Where<Current<Location.locType>, Equal<LocTypeList.vendorLoc>, Or<Current<Location.locType>, Equal<LocTypeList.combinedLoc>>>>>>>),
                    typeof(Select<Location, 
						Where<Location.bAccountID, Equal<Current<Location.bAccountID>>, And<Location.locationID, Equal<Current<Location.locationID>>, 
							And<Where<Current<Location.locType>, Equal<LocTypeList.customerLoc>, Or<Current<Location.locType>, Equal<LocTypeList.combinedLoc>>>>>>>),
					typeof(Select2<Branch, InnerJoin<BAccount, On<BAccount.bAccountID, Equal<Branch.bAccountID>>, 
						InnerJoin<Location, On<Location.bAccountID, Equal<BAccount.bAccountID>, And<Location.locationID, Equal<BAccount.defLocationID>>>>>, 
						Where<Location.bAccountID, Equal<Current<Location.bAccountID>>, And<Location.locationID, Equal<Current<Location.locationID>>, And<Current<Location.locType>, Equal<LocTypeList.companyLoc>>>>>),
					typeof(Select2<EP.EPEmployee, 
						InnerJoin<Location, On<Location.bAccountID, Equal<EP.EPEmployee.bAccountID>, And<Location.locationID, Equal<EP.EPEmployee.defLocationID>>>>, 
						Where<Location.bAccountID, Equal<Current<Location.bAccountID>>, And<Location.locationID, Equal<Current<Location.locationID>>, And<Current<Location.locType>, Equal<LocTypeList.employeeLoc>>>>>),
					typeof(Select<Location,
						Where<Location.bAccountID, Equal<Current<Location.bAccountID>>, And<Location.locationID, Equal<Current<Location.locationID>>,
						And<Where<Current<Location.locType>, Equal<LocTypeList.companyLoc>, Or<Current<Location.locType>, Equal<LocTypeList.combinedLoc>>>>>>>)
					})]
	[PXCacheName(Messages.Location)]
	[PXProjection(typeof(Select2<Location, 
		LeftJoin<LocationAPAccountSub, On<LocationAPAccountSub.bAccountID, Equal<Location.bAccountID>, And<LocationAPAccountSub.locationID, Equal<Location.vAPAccountLocationID>>>,
		LeftJoin<LocationARAccountSub, On<LocationARAccountSub.bAccountID, Equal<Location.bAccountID>, And<LocationARAccountSub.locationID, Equal<Location.cARAccountLocationID>>>,
		LeftJoin<LocationAPPaymentInfo, On<LocationAPPaymentInfo.bAccountID, Equal<Location.bAccountID>, And<LocationAPPaymentInfo.locationID, Equal<Location.vPaymentInfoLocationID>>>,
		LeftJoin<BAccountR, On<BAccountR.bAccountID, Equal<Location.bAccountID>>>>>>>), Persistent = true)]
	[PXGroupMask(typeof(InnerJoin<BAccount, On<BAccount.bAccountID, Equal<Location.bAccountID>, And<Match<BAccount, Current<AccessInfo.userName>>>>>))]
	public partial class Location : PX.Data.IBqlTable, IPaymentTypeDetailMaster, ILocation
	{
		#region Keys

		/// <summary>
		/// Primary Key
		/// </summary>
		public class PK : PrimaryKeyOf<Location>.By<bAccountID, locationID>
		{
			public static Location Find(PXGraph graph, Int32? bAccountID, Int32? locationID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, bAccountID, locationID, options);
		}
		public class UK : PrimaryKeyOf<Location>.By<bAccountID, locationCD>
		{
			public static Location Find(PXGraph graph, Int32? bAccountID, string locationCD, PKFindOptions options = PKFindOptions.None) => FindBy(graph, bAccountID, locationCD, options);
		}

		/// <summary>
		/// Foreign Keys
		/// </summary>
		public new static class FK
		{
			/// <summary>
			/// Default Address
			/// </summary>
			public class Address : CR.Address.PK.ForeignKeyOf<Location>.By<defAddressID> { }

			/// <summary>
			/// Default Contact
			/// </summary>
			public class ContactInfo : CR.Contact.PK.ForeignKeyOf<Location>.By<defContactID> { }

			/// <summary>
			/// BAccount that locations belongs
			/// </summary>
			public class BAccount : CR.BAccount.PK.ForeignKeyOf<Location>.By<bAccountID> { }

			/// <summary>
			/// Remit contact
			/// </summary>
			public class RemitContact : CR.Contact.PK.ForeignKeyOf<Location>.By<vRemitContactID> { }

			/// <summary>
			/// Remit address
			/// </summary>
			public class RemitAddress : CR.Address.PK.ForeignKeyOf<Location>.By<vRemitAddressID> { }
		}
		#endregion

		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		protected Int32? _BAccountID;

		/// <summary>
		/// The identifier of the <see cref="BAccount"/> record that is specified in the document to which the location belongs.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="BAccount.BAccountID"/> field.
		/// </value>
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(BAccount.bAccountID))]
		[PXUIField(DisplayName = "Account ID", Visible = false, Enabled = false, Visibility = PXUIVisibility.Invisible, TabOrder = 0)]
		[PXParent(typeof(Select<BAccount,
			Where<BAccount.bAccountID,
			Equal<Current<Location.bAccountID>>>>)
			)]
		[PXSelector(typeof(Search<BAccount.bAccountID>), DirtyRead = true)]
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
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;

		/// <summary>
		/// The unique identifier assigned to the location.
		/// This field is the primary key field.
		/// </summary>
		[PXDBIdentity]
		[PXUIField(Visible = false, Enabled=false, Visibility = PXUIVisibility.Invisible)]
		[PXReferentialIntegrityCheck]
		public virtual Int32? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion
		#region LocationCD
		public abstract class locationCD : PX.Data.BQL.BqlString.Field<locationCD> { }
		protected String _LocationCD;

		/// <summary>
		/// The human-readable identifier of the location that is specified by the user when they create a location.
		/// This field is a natural key as opposed to the <see cref="LocationID"/> surrogate key.
		/// </summary>
		[CS.LocationRaw(typeof(Where<Location.bAccountID, Equal<Current<Location.bAccountID>>>), IsKey = true, Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Location ID")]
		[PXDefault(PersistingCheck =PXPersistingCheck.NullOrBlank)]
		public virtual String LocationCD
		{
			get
			{
				return this._LocationCD;
			}
			set
			{
				this._LocationCD = value;
			}
		}
		#endregion
		#region LocType
		public abstract class locType : PX.Data.BQL.BqlString.Field<locType> { }
		protected String _LocType;

		/// <summary>
		/// The type of the location.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in the <see cref="LocTypeList.ListAttribute"/> class.
		/// The default value depends on the form where the location was created.
		/// </value>
		[PXDBString(2,IsFixed = true)]
		[LocTypeList.List()]
		[PXUIField(DisplayName = "Location Type", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		public virtual String LocType
		{
			get
			{
				return this._LocType;
			}
			set
			{
				this._LocType = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;

		/// <summary>
		/// The name of the location.
		/// </summary>
		[PXDBString(60,IsUnicode = true)]
		[PXUIField(DisplayName = "Location Name", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion
		#region TaxRegistrationID
		public abstract class taxRegistrationID : PX.Data.BQL.BqlString.Field<taxRegistrationID> { }
		protected String _TaxRegistrationID;

		/// <summary>
		/// The registration ID of the company in the state tax authority.
		/// </summary>
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Registration ID")]
        [PXMassMergableField]
		[PXPersonalDataField]
		public virtual String TaxRegistrationID
		{
			get
			{
				return this._TaxRegistrationID;
			}
			set
			{
				this._TaxRegistrationID = value;
			}
		}
		#endregion
		#region DefAddressID
		public abstract class defAddressID : PX.Data.BQL.BqlInt.Field<defAddressID> { }
		protected Int32? _DefAddressID;

		/// <summary>
		/// The identifier of the address for this location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Address.AddressID"/> field.
		/// </value>
		[PXDBInt()]
		[PXDBChildIdentity(typeof(Address.addressID))]
		[PXUIField(DisplayName = "Default Address", Visibility = PXUIVisibility.Invisible)]
		[PXSelector(typeof(Search<Address.addressID>),DirtyRead = true)]
		public virtual Int32? DefAddressID
		{
			get
			{
				return this._DefAddressID;
			}
			set
			{
				this._DefAddressID = value;
			}
		}
		#endregion
		#region DefContactID
		public abstract class defContactID : PX.Data.BQL.BqlInt.Field<defContactID> { }
		protected Int32? _DefContactID;

		/// <summary>
		/// The identifier of the bisuness account contact of the location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Contact.ContactID"/> field.
		/// </value>
		[PXDBInt()]
		[PXDBChildIdentity(typeof(Contact.contactID))]
		[PXUIField(DisplayName = "Default Contact")]
		[PXSelector(typeof(Search<Contact.contactID>), ValidateValue = false, DirtyRead = true)]
		public virtual Int32? DefContactID
		{
			get
			{
				return this._DefContactID;
			}
			set
			{
				this._DefContactID = value;
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote()]
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
		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		protected bool? _IsActive;

		/// <summary>
		/// This field indicates whether the location is active.
		/// </summary>
		/// <value>
		/// The default value is <see langword="true"/>.
		/// </value>
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active", Enabled = false)]
		public virtual bool? IsActive
		{
			get
			{
				return this._IsActive;
			}
			set
			{
				this._IsActive = value;
			}
		}
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

		/// <summary>
		/// The current status of the location.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in the <see cref="LocationStatus.ListAttribute"/> class.
		/// The default value is <see cref="LocationStatus.Active"/>.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(LocationStatus.Active)]
		[PXUIField(DisplayName = "Status")]
		[LocationStatus.List]
		public virtual string Status { get; set; }
		#endregion
		#region IsDefault
		public abstract class isDefault : PX.Data.BQL.BqlBool.Field<isDefault> { }

		/// <summary>
		/// This field indicates whether the location is default for the corresponding business account.
		/// </summary>
		/// <value>
		/// The field value is <see langword="true" /> when this location is the default location of the corresponding business account, and <see langword="false"/> otherwise.
		/// </value>
		[PXDBCalced(typeof(IIf<BAccountR.defLocationID.IsEqual<locationID>, True, False>), typeof(bool))]
		[PXBool]
		[PXUIField(DisplayName = "Default")]
		public virtual bool? IsDefault { get; set; }
		#endregion



		//Customer Locaiton Properties
		#region CTaxZoneID
		public abstract class cTaxZoneID : PX.Data.BQL.BqlString.Field<cTaxZoneID> { }
		protected String _CTaxZoneID;

		/// <summary>
		/// The customer's tax zone.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="TaxZone.TaxZoneID" /> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Zone")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<TaxZone.taxZoneID>), DescriptionField = typeof(TaxZone.descr), CacheGlobal = true)]
		public virtual String CTaxZoneID
		{
			get
			{
				return this._CTaxZoneID;
			}
			set
			{
				this._CTaxZoneID = value;
			}
		}
		#endregion
		#region CTaxCalcMode
		public abstract class cTaxCalcMode : PX.Data.BQL.BqlString.Field<cTaxCalcMode> { }

		/// <summary>
		/// The tax calculation mode of the customer.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in the <see cref="TaxCalculationMode.ListAttribute"/> class.
		/// The default value is <see cref="TaxCalculationMode.TaxSetting"/>.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(TaxCalculationMode.TaxSetting, typeof(Search<CustomerClass.taxCalcMode, Where<CustomerClass.customerClassID, Equal<Current<CustomerClass.customerClassID>>>>))]
		[TaxCalculationMode.List]
		[PXUIField(DisplayName = "Tax Calculation Mode")]
		public virtual string CTaxCalcMode { get; set; }
		#endregion
		#region CAvalaraExemptionNumber
		public abstract class cAvalaraExemptionNumber : PX.Data.BQL.BqlString.Field<cAvalaraExemptionNumber> { }
		protected String _CAvalaraExemptionNumber;

		/// <summary>
		/// The Avalara Exemption number of the customer location.
		/// </summary>
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Exemption Number")]
		public virtual String CAvalaraExemptionNumber
		{
			get
			{
				return this._CAvalaraExemptionNumber;
			}
			set
			{
				this._CAvalaraExemptionNumber = value;
			}
		}
		#endregion
		#region CAvalaraCustomerUsageType
		public abstract class cAvalaraCustomerUsageType : PX.Data.BQL.BqlString.Field<cAvalaraCustomerUsageType> { }
		protected String _CAvalaraCustomerUsageType;

		/// <summary>
		/// The customer's entity type for reporting purposes. This field is used if the system is integrated with External Tax Calculation
		/// and the <see cref="FeaturesSet.AvalaraTax">External Tax Calculation Integration</see> feature is enabled.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in the <see cref="TXAvalaraCustomerUsageType.ListAttribute"/> class.
		/// The default value is <see cref="TXAvalaraCustomerUsageType.Default"/>.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Entity Usage Type")]
		[PXDefault(TXAvalaraCustomerUsageType.Default)]
		[TX.TXAvalaraCustomerUsageType.List]
		public virtual String CAvalaraCustomerUsageType
		{
			get
			{
				return this._CAvalaraCustomerUsageType;
			}
			set
			{
				this._CAvalaraCustomerUsageType = value;
			}
		}
		#endregion
		#region CCarrierID
		public abstract class cCarrierID : PX.Data.BQL.BqlString.Field<cCarrierID> { }
		protected String _CCarrierID;

		/// <summary>
		/// The shipping carrier for the vendor location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the <see cref="Carrier.carrierID" /> field.
		/// </value>
		[PXDBString(15, IsUnicode = true, InputMask = ">aaaaaaaaaaaaaaa")]
		[PXUIField(DisplayName = "Ship Via")]
		[PXSelector(typeof(Search<Carrier.carrierID>),
			typeof(Carrier.carrierID), typeof(Carrier.description), typeof(Carrier.isExternal), typeof(Carrier.confirmationRequired),
			CacheGlobal = true,
			DescriptionField = typeof(Carrier.description))]
		public virtual String CCarrierID
		{
			get
			{
				return this._CCarrierID;
			}
			set
			{
				this._CCarrierID = value;
			}
		}
		#endregion
		#region CShipTermsID
		public abstract class cShipTermsID : PX.Data.BQL.BqlString.Field<cShipTermsID> { }
		protected String _CShipTermsID;

		/// <summary>
		/// The customer's shipping terms.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="ShipTerms.ShipTermsID" /> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Shipping Terms")]
		[PXSelector(typeof(Search<ShipTerms.shipTermsID>),CacheGlobal=true,DescriptionField = typeof(ShipTerms.description))]
		public virtual String CShipTermsID
		{
			get
			{
				return this._CShipTermsID;
			}
			set
			{
				this._CShipTermsID = value;
			}
		}
		#endregion
		#region CShipZoneID
		public abstract class cShipZoneID : PX.Data.BQL.BqlString.Field<cShipZoneID> { }
		protected String _CShipZoneID;

		/// <summary>
		/// The customer's shipping zone.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="ShippingZone.ZoneID" /> field.
		/// </value>
		[PXDBString(15, IsUnicode = true, InputMask = ">aaaaaaaaaaaaaaa")]
		[PXUIField(DisplayName = "Shipping Zone")]
		[PXSelector(typeof(ShippingZone.zoneID), CacheGlobal = true, DescriptionField = typeof(ShippingZone.description))]
		public virtual String CShipZoneID
		{
			get
			{
				return this._CShipZoneID;
			}
			set
			{
				this._CShipZoneID = value;
			}
		}
		#endregion
		#region CFOBPointID
		public abstract class cFOBPointID : PX.Data.BQL.BqlString.Field<cFOBPointID> { }
		protected String _CFOBPointID;

		/// <summary>
		/// The customer's FOB (free on board) shipping point.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="FOBPoint.FOBPointID" /> field.
		/// </value>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "FOB Point")]
		[PXSelector(typeof(FOBPoint.fOBPointID), CacheGlobal = true, DescriptionField = typeof(FOBPoint.description))]
		public virtual String CFOBPointID
		{
			get
			{
				return this._CFOBPointID;
			}
			set
			{
				this._CFOBPointID = value;
			}
		}
		#endregion
		#region CResedential
		public abstract class cResedential : PX.Data.BQL.BqlBool.Field<cResedential> { }
		protected Boolean? _CResedential;

		/// <summary>
		/// This field indicates whether the residential delivery is available in this location.
		/// </summary>
		/// <value>
		/// The default value is <see langword="false"/>.
		/// </value>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Residential Delivery")]
		public virtual Boolean? CResedential
		{
			get
			{
				return this._CResedential;
			}
			set
			{
				this._CResedential = value;
			}
		}
		#endregion
		#region CSaturdayDelivery
		public abstract class cSaturdayDelivery : PX.Data.BQL.BqlBool.Field<cSaturdayDelivery> { }
		protected Boolean? _CSaturdayDelivery;

		/// <summary>
		/// This field indicates whether the Saturday delivery is available in this location.
		/// </summary>
		/// <value>
		/// The default value is <see langword="false"/>.
		/// </value>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Saturday Delivery")]
		public virtual Boolean? CSaturdayDelivery
		{
			get
			{
				return this._CSaturdayDelivery;
			}
			set
			{
				this._CSaturdayDelivery = value;
			}
		}
		#endregion
		#region CGroundCollect
		public abstract class cGroundCollect : PX.Data.BQL.BqlBool.Field<cGroundCollect> { }
		protected Boolean? _CGroundCollect;

		/// <summary>
		/// This field indicates whether the FedEx Ground Collect program is available in this location.
		/// </summary>
		/// <value>
		/// The default value is <see langword="false"/>.
		/// </value>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "FedEx Ground Collect")]
		public virtual Boolean? CGroundCollect
		{
			get
			{
				return this._CGroundCollect;
			}
			set
			{
				this._CGroundCollect = value;
			}
		}
		#endregion
		#region CInsurance
		public abstract class cInsurance : PX.Data.BQL.BqlBool.Field<cInsurance> { }
		protected Boolean? _CInsurance;

		/// <summary>
		/// This field indicates whether the delivery insurance is available in this location.
		/// </summary>
		/// <value>
		/// The default value is <see langword="false"/>.
		/// </value>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Insurance")]
		public virtual Boolean? CInsurance
		{
			get
			{
				return this._CInsurance;
			}
			set
			{
				this._CInsurance = value;
			}
		}
		#endregion
		#region CLeadTime
		public abstract class cLeadTime : PX.Data.BQL.BqlShort.Field<cLeadTime> { }
		protected Int16? _CLeadTime;

		/// <summary>
		/// The amount of lead days (the time in days from the moment when the production was finished to the moment when the customer's order was delivered).
		/// </summary>
		[PXDBShort(MinValue=0,MaxValue=100000)]
		[PXUIField(DisplayName = CR.Messages.LeadTimeDays)]
		public virtual Int16? CLeadTime
		{
			get
			{
				return this._CLeadTime;
			}
			set
			{
				this._CLeadTime = value;
			}
		}
		#endregion
		#region CBranchID
		public abstract class cBranchID : PX.Data.BQL.BqlInt.Field<cBranchID> { }
		protected Int32? _CBranchID;

		/// <summary>
		/// The identifier of the default branch of the customer location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Branch.BranchID" /> field.
		/// </value>
		[Branch(useDefaulting: false, IsDetail = false, PersistingCheck = PXPersistingCheck.Nothing, DisplayName = "Shipping Branch", IsEnabledWhenOneBranchIsAccessible = true)]
		public virtual Int32? CBranchID
		{
			get
			{
				return this._CBranchID;
			}
			set
			{
				this._CBranchID = value;
			}
		}
		#endregion
		#region CSalesAcctID
		public abstract class cSalesAcctID : PX.Data.BQL.BqlInt.Field<cSalesAcctID> { }
		protected Int32? _CSalesAcctID;

		/// <summary>
		/// The identifier of the customer's sales account.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Account.AccountID" /> field.
		/// </value>
		[Account(DisplayName = "Sales Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), Required = true, AvoidControlAccounts = true)]
		public virtual Int32? CSalesAcctID
		{
			get
			{
				return this._CSalesAcctID;
			}
			set
			{
				this._CSalesAcctID = value;
			}
		}
			  #endregion
		#region CSalesSubID
		public abstract class cSalesSubID : PX.Data.BQL.BqlInt.Field<cSalesSubID> { }
		protected Int32? _CSalesSubID;

		/// <summary>
		/// The identifier of the customer's sales subaccount.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Sub.SubID" /> field.
		/// </value>
		[SubAccount(typeof(Location.cSalesAcctID), DisplayName = "Sales Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), Required = true)]
		[PXReferentialIntegrityCheck(CheckPoint = CheckPoint.BeforePersisting)]
		[PXForeignReference(typeof(Field<cSalesSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? CSalesSubID
		{
			get
			{
				return this._CSalesSubID;
			}
			set
			{
				this._CSalesSubID = value;
			}
		}
		#endregion
		#region CPriceClassID
		public abstract class cPriceClassID : PX.Data.BQL.BqlString.Field<cPriceClassID> { }
		protected String _CPriceClassID;

		/// <summary>
		/// The price class of the customer.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="ARPriceClass.PriceClassID" /> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(AR.ARPriceClass.priceClassID))]
		[PXUIField(DisplayName = "Price Class", Visibility = PXUIVisibility.Visible)]
		public virtual String CPriceClassID
		{
			get
			{
				return this._CPriceClassID;
			}
			set
			{
				this._CPriceClassID = value;
			}
		}
		#endregion
		#region CSiteID
		public abstract class cSiteID : PX.Data.BQL.BqlInt.Field<cSiteID> { }
		protected Int32? _CSiteID;

		/// <summary>
		/// The warehouse identifier of the customer location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="INSite.SiteID" /> field.
		/// </value>
		[PXDBInt()]
		[PXUIField(DisplayName = "Warehouse", Visibility = PXUIVisibility.Visible)]
		[PXDimensionSelector(SiteAttribute.DimensionName, typeof(INSite.siteID), typeof(INSite.siteCD), DescriptionField=typeof(INSite.descr))]
        [PXRestrictor(typeof(Where<INSite.active, Equal<True>>), IN.Messages.InactiveWarehouse, typeof(INSite.siteCD))]
        [PXRestrictor(typeof(Where<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>>), IN.Messages.TransitSiteIsNotAvailable)]
		[PXForeignReference(typeof(Field<cSiteID>.IsRelatedTo<INSite.siteID>))]
		public virtual Int32? CSiteID
		{
			get
			{
				return this._CSiteID;
			}
			set
			{
				this._CSiteID = value;
			}
		}
		#endregion
		#region CDiscountAcctID
		public abstract class cDiscountAcctID : PX.Data.BQL.BqlInt.Field<cDiscountAcctID> { }
		protected Int32? _CDiscountAcctID;

		/// <summary>
		/// The identifier of the discount account of the customer location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Account.AccountID" /> field.
		/// </value>
		[Account(DisplayName = "Discount Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), Required=false, AvoidControlAccounts = true)]		
		public virtual Int32? CDiscountAcctID
		{
			get
			{
				return this._CDiscountAcctID;
			}
			set
			{
				this._CDiscountAcctID = value;
			}
		}
		#endregion
		#region CDiscountSubID
		public abstract class cDiscountSubID : PX.Data.BQL.BqlInt.Field<cDiscountSubID> { }
		protected Int32? _CDiscountSubID;

		/// <summary>
		/// The identifier of the discount subaccount of the customer's location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Sub.SubID" /> field.
		/// </value>
		[SubAccount(typeof(Location.cDiscountAcctID), DisplayName = "Discount Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), Required = false)]		
		[PXReferentialIntegrityCheck(CheckPoint = CheckPoint.BeforePersisting)]
		[PXForeignReference(typeof(Field<cDiscountSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? CDiscountSubID
		{
			get
			{
				return this._CDiscountSubID;
			}
			set
			{
				this._CDiscountSubID = value;
			}
		}
		#endregion
		#region CRetainageAcctID
		public abstract class cRetainageAcctID : PX.Data.BQL.BqlInt.Field<cRetainageAcctID> { }

		/// <summary>
		/// The identifier of the retainage account of the customer's location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Account.AccountID" /> field.
		/// </value>
		[Account(DisplayName = "Retainage Receivable Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), Required = false)]
		public virtual int? CRetainageAcctID
		{
			get;
			set;
		}
		#endregion
		#region CRetainageSubID
		public abstract class cRetainageSubID : PX.Data.BQL.BqlInt.Field<cRetainageSubID> { }

		/// <summary>
		/// The identifier of the retainage subaccount of the customer's location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Sub.SubID" /> field.
		/// </value>
		[SubAccount(typeof(Location.cRetainageAcctID), DisplayName = "Retainage Receivable Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), Required = false)]
		[PXReferentialIntegrityCheck(CheckPoint = CheckPoint.BeforePersisting)]
		[PXForeignReference(typeof(Field<cRetainageSubID>.IsRelatedTo<Sub.subID>))]
		public virtual int? CRetainageSubID
		{
			get;
			set;
		}
		#endregion
		#region CFreightAcctID
		public abstract class cFreightAcctID : PX.Data.BQL.BqlInt.Field<cFreightAcctID> { }
		protected Int32? _CFreightAcctID;

		/// <summary>
		/// The identifier of the freight account of the customer's location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Account.AccountID" /> field.
		/// </value>
		[Account(DisplayName = "Freight Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), Required = false, AvoidControlAccounts = true)]
		public virtual Int32? CFreightAcctID
		{
			get
			{
				return this._CFreightAcctID;
			}
			set
			{
				this._CFreightAcctID = value;
			}
		}
		#endregion
		#region CFreightSubID
		public abstract class cFreightSubID : PX.Data.BQL.BqlInt.Field<cFreightSubID> { }
		protected Int32? _CFreightSubID;

		/// <summary>
		/// The identifier of the freight subaccount of the customer location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Sub.SubID" /> field.
		/// </value>
		[SubAccount(typeof(Location.cFreightAcctID), DisplayName = "Freight Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), Required = false)]
		[PXReferentialIntegrityCheck(CheckPoint = CheckPoint.BeforePersisting)]
		[PXForeignReference(typeof(Field<cFreightSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? CFreightSubID
		{
			get
			{
				return this._CFreightSubID;
			}
			set
			{
				this._CFreightSubID = value;
			}
		}
		#endregion
		#region CShipComplete
		public abstract class cShipComplete : PX.Data.BQL.BqlString.Field<cShipComplete> { }
		protected String _CShipComplete;

		/// <summary>
		/// The shipping rule of the customer location.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in the <see cref="SOShipComplete.ListAttribute"/> class.
		/// The default value is <see cref="SOShipComplete.CancelRemainder"/>.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(SOShipComplete.CancelRemainder)]
		[SOShipComplete.List()]
		[PXUIField(DisplayName = "Shipping Rule")]
		public virtual String CShipComplete
		{
			get
			{
				return this._CShipComplete;
			}
			set
			{
				this._CShipComplete = value;
			}
		}
		#endregion
		#region COrderPriority
		public abstract class cOrderPriority : PX.Data.BQL.BqlShort.Field<cOrderPriority> { }
		protected Int16? _COrderPriority;

		/// <summary>
		/// The order priority of the customer's location.
		/// </summary>
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Order Priority")]
		public virtual Int16? COrderPriority
		{
			get
			{
				return this._COrderPriority;
			}
			set
			{
				this._COrderPriority = value;
			}
		}
		#endregion
		#region CCalendarID
		public abstract class cCalendarID : PX.Data.BQL.BqlString.Field<cCalendarID> { }
		protected String _CCalendarID;

		/// <summary>
		/// The type of the work calendar in the customer location.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="CSCalendar.CalendarID" /> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Calendar", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<CSCalendar.calendarID>), DescriptionField = typeof(CSCalendar.description))]
		public virtual String CCalendarID
		{
			get
			{
				return this._CCalendarID;
			}
			set
			{
				this._CCalendarID = value;
			}
		}
		#endregion
		#region CDefProject
		public abstract class cDefProjectID : PX.Data.BQL.BqlInt.Field<cDefProjectID> { }
		protected Int32? _CDefProjectID;

		/// <summary>
		/// The identifier of the default project of the customer location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PM.PMProject.ContractID" /> field.
		/// </value>
		[PM.Project(typeof(Where<PM.PMProject.customerID, Equal<Current<Location.bAccountID>>>), DisplayName = "Default Project")]
		public virtual Int32? CDefProjectID
		{
			get
			{
				return this._CDefProjectID;
			}
			set
			{
				this._CDefProjectID = value;
			}
		}
		#endregion
		#region CARAccountLocationID
		public abstract class cARAccountLocationID : PX.Data.BQL.BqlInt.Field<cARAccountLocationID> { }
		protected Int32? _CARAccountLocationID;

		/// <summary>
		/// The identifier of the AR account location of the customer location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the <see cref="Location.LocationID" /> field.
		/// </value>
		[PXDBInt()]
		[PXDefault()]
		public virtual Int32? CARAccountLocationID
		{
			get
			{
				return this._CARAccountLocationID;
			}
			set
			{
				this._CARAccountLocationID = value;
			}
		}
		#endregion
		#region CARAccountID
		public abstract class cARAccountID : PX.Data.BQL.BqlInt.Field<cARAccountID> { }
		protected Int32? _CARAccountID;

		/// <summary>
		/// The identifier of the AR account of the customer location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Location.LocationID" /> field.
		/// </value>
        [Account(null, typeof(Search<Account.accountID,
                    Where2<Match<Current<AccessInfo.userName>>,
                         And<Account.active, Equal<True>,
                         And<Account.isCashAccount, Equal<False>,
                         And<Where<Current<GLSetup.ytdNetIncAccountID>, IsNull,
                          Or<Account.accountID, NotEqual<Current<GLSetup.ytdNetIncAccountID>>>>>>>>>), DisplayName = "AR Account", Required = true)]
        public virtual Int32? CARAccountID
		{
			get
			{
				return this._CARAccountID;
			}
			set
			{
				this._CARAccountID = value;
			}
		}
		#endregion
		#region CARSubID
		public abstract class cARSubID : PX.Data.BQL.BqlInt.Field<cARSubID> { }
		protected Int32? _CARSubID;

		/// <summary>
		/// The identifier of the AR subaccount of the customer location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Sub.SubID" /> field.
		/// </value>
		[SubAccount(typeof(Location.cARAccountID), DisplayName = "AR Sub.", DescriptionField = typeof(Sub.description), Required = true)]
		[PXReferentialIntegrityCheck(CheckPoint = CheckPoint.BeforePersisting)]
		[PXForeignReference(typeof(Field<cARSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? CARSubID
		{
			get
			{
				return this._CARSubID;
			}
			set
			{
				this._CARSubID = value;
			}
		}
		#endregion
		#region IsARAccountSameAsMain
		public abstract class isARAccountSameAsMain : PX.Data.BQL.BqlBool.Field<isARAccountSameAsMain> { }
		protected bool? _IsARAccountSameAsMain;

		/// <summary>
		/// This field indicates that the <see cref="CARAccountLocationID">AR account location</see> is the same in the customer location.
		/// </summary>
		[PXBool()]
		[PXUIField(DisplayName = "Same As Default Location's")]
		[PXFormula(typeof(Switch<Case<Where<locationID, Equal<cARAccountLocationID>>, False>, True>))]
		public virtual bool? IsARAccountSameAsMain
		{
			get
			{
				return this._IsARAccountSameAsMain;
			}
			set
			{
				this._IsARAccountSameAsMain = value;
			}
		}
		#endregion



		// Vendor Location Properties
		#region VTaxZoneID
		public abstract class vTaxZoneID : PX.Data.BQL.BqlString.Field<vTaxZoneID> { }
		protected String _VTaxZoneID;

		/// <summary>
		/// The vendor's tax zone.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="TaxZone.taxZoneID" /> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Zone")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<TaxZone.taxZoneID>), CacheGlobal = true)]
		public virtual String VTaxZoneID
		{
			get
			{
				return this._VTaxZoneID;
			}
			set
			{
				this._VTaxZoneID = value;
			}
		}
		#endregion
		#region VTaxCalcMode
		public abstract class vTaxCalcMode : PX.Data.BQL.BqlString.Field<vTaxCalcMode> { }

		/// <summary>
		/// The vendor's tax calculation mode.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in the <see cref="TaxCalculationMode.ListAttribute"/> class.
		/// The default value is <see cref="TaxCalculationMode.TaxSetting"/>.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(TaxCalculationMode.TaxSetting, typeof(Search<VendorClass.taxCalcMode, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>))]
		[TaxCalculationMode.List]
		[PXUIField(DisplayName = "Tax Calculation Mode")]
		public virtual string VTaxCalcMode { get; set; }
		#endregion
		#region VCarrierID
		public abstract class vCarrierID : PX.Data.BQL.BqlString.Field<vCarrierID> { }
		protected String _VCarrierID;

		/// <summary>
		/// The shipping carrier for the customer location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Carrier.CarrierID" /> field.
		/// </value>
		[PXDBString(15, IsUnicode = true, InputMask = ">aaaaaaaaaaaaaaa")]
		[PXUIField(DisplayName = "Ship Via")]
		[PXSelector(typeof(Search<Carrier.carrierID>),
			typeof(Carrier.carrierID), typeof(Carrier.description), typeof(Carrier.isExternal), typeof(Carrier.confirmationRequired),
			CacheGlobal = true,
			DescriptionField = typeof(Carrier.description))]
		public virtual String VCarrierID
		{
			get
			{
				return this._VCarrierID;
			}
			set
			{
				this._VCarrierID = value;
			}
		}
		#endregion
		#region VShipTermsID
		public abstract class vShipTermsID : PX.Data.BQL.BqlString.Field<vShipTermsID> { }
		protected String _VShipTermsID;

		/// <summary>
		/// The vendor's shipping terms.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="ShipTerms.shipTermsID" /> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Shipping Terms")]
		[PXSelector(typeof(Search<ShipTerms.shipTermsID>), CacheGlobal = true, DescriptionField = typeof(ShipTerms.description))]
		public virtual String VShipTermsID
		{
			get
			{
				return this._VShipTermsID;
			}
			set
			{
				this._VShipTermsID = value;
			}
		}
		#endregion
		#region VFOBPointID
		public abstract class vFOBPointID : PX.Data.BQL.BqlString.Field<vFOBPointID> { }
		protected String _VFOBPointID;

		/// <summary>
		/// The vendor's FOB (free on board) shipping point.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="FOBPoint.fOBPointID" /> field.
		/// </value>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "FOB Point")]
		[PXSelector(typeof(FOBPoint.fOBPointID), CacheGlobal = true, DescriptionField = typeof(FOBPoint.description))]
		public virtual String VFOBPointID
		{
			get
			{
				return this._VFOBPointID;
			}
			set
			{
				this._VFOBPointID = value;
			}
		}
		#endregion
		#region VLeadTime
		public abstract class vLeadTime : PX.Data.BQL.BqlShort.Field<vLeadTime> { }
		protected Int16? _VLeadTime;

		/// <summary>
		/// The amount of lead days (the time in days from the moment when the production was finished to the moment when the vendor's order was delivered).
		/// </summary>
		[PXDBShort(MinValue = 0, MaxValue = 100000)]
		[PXUIField(DisplayName = CR.Messages.LeadTimeDays)]
		public virtual Int16? VLeadTime
		{
			get
			{
				return this._VLeadTime;
			}
			set
			{
				this._VLeadTime = value;
			}
		}
		#endregion
		#region VBranchID
		public abstract class vBranchID : PX.Data.BQL.BqlInt.Field<vBranchID> { }
		protected Int32? _VBranchID;

		/// <summary>
		/// The identifier of the default branch of the vendor location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Branch.branchID" /> field.
		/// </value>
		[Branch(useDefaulting: false, IsDetail = false, PersistingCheck = PXPersistingCheck.Nothing, DisplayName = "Receiving Branch", IsEnabledWhenOneBranchIsAccessible = true)]
		public virtual Int32? VBranchID
		{
			get
			{
				return this._VBranchID;
			}
			set
			{
				this._VBranchID = value;
			}
		}
		#endregion
		#region VExpenseAcctID
		public abstract class vExpenseAcctID : PX.Data.BQL.BqlInt.Field<vExpenseAcctID> { }
		protected Int32? _VExpenseAcctID;

		/// <summary>
		/// The identifier of the expense account of the vendor location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the <see cref="Account.AccountID" /> field.
		/// </value>
		[Account(DisplayName = "Expense Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		public virtual Int32? VExpenseAcctID
		{
			get
			{
				return this._VExpenseAcctID;
			}
			set
			{
				this._VExpenseAcctID = value;
			}
		}
		#endregion
		#region VExpenseSubID
		public abstract class vExpenseSubID : PX.Data.BQL.BqlInt.Field<vExpenseSubID> { }
		protected Int32? _VExpenseSubID;

		/// <summary>
		/// The identifier of the expense subaccount of the vendor location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Sub.SubID" /> field.
		/// </value>
		[SubAccount(typeof(Location.vExpenseAcctID), DisplayName = "Expense Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXReferentialIntegrityCheck(CheckPoint = CheckPoint.BeforePersisting)]
		[PXForeignReference(typeof(Field<vExpenseSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? VExpenseSubID
		{
			get
			{
				return this._VExpenseSubID;
			}
			set
			{
				this._VExpenseSubID = value;
			}
		}
		#endregion
		#region VRetainageAcctID
		public abstract class vRetainageAcctID : PX.Data.BQL.BqlInt.Field<vRetainageAcctID> { }

		/// <summary>
		/// The identifier of the retainage account of the vendor location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Account.AccountID" /> field.
		/// </value>
		[Account(DisplayName = "Retainage Payable Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), Required = false)]
		public virtual int? VRetainageAcctID
		{
			get;
			set;
		}
		#endregion
		#region VRetainageSubID
		public abstract class vRetainageSubID : PX.Data.BQL.BqlInt.Field<vRetainageSubID> { }

		/// <summary>
		/// The identifier of the retainage subaccount of the vendor location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Sub.SubID" /> field.
		/// </value>
		[SubAccount(typeof(Location.vRetainageAcctID), DisplayName = "Retainage Payable Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), Required = false)]
		[PXReferentialIntegrityCheck(CheckPoint = CheckPoint.BeforePersisting)]
		[PXForeignReference(typeof(Field<vRetainageSubID>.IsRelatedTo<Sub.subID>))]
		public virtual int? VRetainageSubID
		{
			get;
			set;
		}
		#endregion
		#region VFreightAcctID
		public abstract class vFreightAcctID : PX.Data.BQL.BqlInt.Field<vFreightAcctID> { }
		protected Int32? _VFreightAcctID;

		/// <summary>
		/// The identidier of the freight account of the vendor location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Account.AccountID" /> field.
		/// </value>
		[Account(DisplayName = "Freight Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), Required = false)]
		public virtual Int32? VFreightAcctID
		{
			get
			{
				return this._VFreightAcctID;
			}
			set
			{
				this._VFreightAcctID = value;
			}
		}
		#endregion
		#region VFreightSubID
		public abstract class vFreightSubID : PX.Data.BQL.BqlInt.Field<vFreightSubID> { }
		protected Int32? _VFreightSubID;

		/// <summary>
		/// The identifier of the freight subaccount of the vendor location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Sub.SubID" /> field.
		/// </value>
		[SubAccount(typeof(Location.vFreightAcctID), DisplayName = "Freight Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), Required = false)]
		public virtual Int32? VFreightSubID
		{
			get
			{
				return this._VFreightSubID;
			}
			set
			{
				this._VFreightSubID = value;
			}
		}
		#endregion		
        #region VDiscountAcctID
		public abstract class vDiscountAcctID : PX.Data.BQL.BqlInt.Field<vDiscountAcctID> { }

		/// <summary>
		/// The identifier of the discount account of the vendor location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Account.AccountID" /> field.
		/// </value>
		[Account(DisplayName = "Discount Account", 
			Visibility = PXUIVisibility.Visible, 
			DescriptionField = typeof(Account.description), 
			Required = false,
			AvoidControlAccounts = true)]
		public virtual int? VDiscountAcctID
            {
			get;
			set;
        }
        #endregion
        #region VDiscountSubID
		public abstract class vDiscountSubID : PX.Data.BQL.BqlInt.Field<vDiscountSubID> { }

		/// <summary>
		/// The identifier of the discount subaccount of the vendor location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Sub.SubID" /> field.
		/// </value>
		[PXReferentialIntegrityCheck(CheckPoint = CheckPoint.BeforePersisting)]
		[PXForeignReference(typeof(Field<vDiscountSubID>.IsRelatedTo<Sub.subID>))]
		[SubAccount(typeof(Location.vDiscountAcctID), 
			DisplayName = "Discount Sub.", 
			Visibility = PXUIVisibility.Visible, 
			DescriptionField = typeof(Sub.description), 
			Required = false)]
		public virtual int? VDiscountSubID
		{
			get;
			set;
        }
        #endregion
		#region VRcptQtyMin
		public abstract class vRcptQtyMin : PX.Data.BQL.BqlDecimal.Field<vRcptQtyMin> { }
		protected Decimal? _VRcptQtyMin;

		/// <summary>
		/// The minimal receipt amount for the vendor location in percentages.
		/// </summary>
		[PXDBDecimal(2, MinValue = 0.0, MaxValue = 999.0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Min. Receipt (%)")]
		public virtual Decimal? VRcptQtyMin
		{
			get
			{
				return this._VRcptQtyMin;
			}
			set
			{
				this._VRcptQtyMin = value;
			}
		}
		#endregion
		#region VRcptQtyMax
		public abstract class vRcptQtyMax : PX.Data.BQL.BqlDecimal.Field<vRcptQtyMax> { }
		protected Decimal? _VRcptQtyMax;

		/// <summary>
		/// The maximum receipt amount for the vendor location in percentages.
		/// </summary>
		[PXDBDecimal(2, MinValue = 0.0, MaxValue = 999.0)]
		[PXDefault(TypeCode.Decimal, "100.0")]
		[PXUIField(DisplayName = "Max. Receipt (%)")]
		public virtual Decimal? VRcptQtyMax
		{
			get
			{
				return this._VRcptQtyMax;
			}
			set
			{
				this._VRcptQtyMax = value;
			}
		}
		#endregion
		#region VRcptQtyThreshold
		public abstract class vRcptQtyThreshold : PX.Data.BQL.BqlDecimal.Field<vRcptQtyThreshold> { }
		protected Decimal? _VRcptQtyThreshold;

		/// <summary>
		/// The threshold receipt amount for the vendor location in percentages.
		/// </summary>
		[PXDBDecimal(2, MinValue = 0.0, MaxValue = 999.0)]
		[PXDefault(TypeCode.Decimal, "100.0")]
		[PXUIField(DisplayName = "Threshold Receipt (%)")]
		public virtual Decimal? VRcptQtyThreshold
		{
			get
			{
				return this._VRcptQtyThreshold;
			}
			set
			{
				this._VRcptQtyThreshold = value;
			}
		}
		#endregion
		#region VRcptQtyAction
		public abstract class vRcptQtyAction : PX.Data.BQL.BqlString.Field<vRcptQtyAction> { }
		protected String _VRcptQtyAction;

		/// <summary>
		/// The type of the receipt action for the vendor location.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in the <see cref="POReceiptQtyAction.ListAttribute"/> class.
		/// The default value is <see cref="POReceiptQtyAction.AcceptButWarn"/>.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(POReceiptQtyAction.AcceptButWarn)]
		[POReceiptQtyAction.List()]
		[PXUIField(DisplayName = "Receipt Action")]
		public virtual String VRcptQtyAction
		{
			get
			{
				return this._VRcptQtyAction;
			}
			set
			{
				this._VRcptQtyAction = value;
			}
		}
		#endregion
		#region VSiteID
		public abstract class vSiteID : PX.Data.BQL.BqlInt.Field<vSiteID> { }
		protected Int32? _VSiteID;

		/// <summary>
		/// The warehouse identifier of the vendor location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="INSite.siteID" /> field.
		/// </value>
		[PXDBInt()]
		[PXUIField(DisplayName = "Warehouse", Visibility = PXUIVisibility.Visible)]
		[PXDimensionSelector(SiteAttribute.DimensionName, typeof(INSite.siteID), typeof(INSite.siteCD), DescriptionField=typeof(INSite.descr))]
        [PXRestrictor(typeof(Where<INSite.active, Equal<True>>), IN.Messages.InactiveWarehouse, typeof(INSite.siteCD))]
        [PXRestrictor(typeof(Where<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>>), IN.Messages.TransitSiteIsNotAvailable)]
		[PXForeignReference(typeof(Field<vSiteID>.IsRelatedTo<INSite.siteID>))]
		public virtual Int32? VSiteID
		{
			get
			{
				return this._VSiteID;
			}
			set
			{
				this._VSiteID = value;
			}
		}
		#endregion
		#region VSiteIDIsNull
		public abstract class vSiteIDIsNull : PX.Data.BQL.BqlShort.Field<vSiteIDIsNull> { }

		/// <exclude />
		[PXShort()]
		[PXDBCalced(typeof(Switch<Case<Where<Location.vSiteID, IsNull>, shortMax>, short0>), typeof(short))]
		public virtual Int16? VSiteIDIsNull { get; set; }
		#endregion
		#region VPrintOrder
		public abstract class vPrintOrder : PX.Data.BQL.BqlBool.Field<vPrintOrder> { }
		protected bool? _VPrintOrder;

		/// <summary>
		/// This field indicates whether the order an order should be printed in the vendor location.
		/// </summary>
		/// <value>
		/// The default value is <see langword="false"/>.
		/// </value>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Print Order")]
		public virtual bool? VPrintOrder
		{
			get
			{
				return this._VPrintOrder;
			}
			set
			{
				this._VPrintOrder = value;
			}
		}
		#endregion
		#region VEmailOrder
		public abstract class vEmailOrder : PX.Data.BQL.BqlBool.Field<vEmailOrder> { }
		protected bool? _VEmailOrder;

		/// <summary>
		/// This field indicates whether the order should be sent by email in the vendor location.
		/// </summary>
		/// <value>
		/// The default value is <see langword="false"/>.
		/// </value>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Email Order")]
		public virtual bool? VEmailOrder
		{
			get
			{
				return this._VEmailOrder;
			}
			set
			{
				this._VEmailOrder = value;
			}
		}
		#endregion
		#region VDefProjectID
		public abstract class vDefProjectID : PX.Data.BQL.BqlInt.Field<vDefProjectID> { }
		protected Int32? _VDefProjectID;

		/// <summary>
		/// The identifier of the default project for the vendor location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PM.PMProject.ContractID" /> field.
		/// </value>
		[PM.Project(DisplayName = "Default Project")]
		public virtual Int32? VDefProjectID
		{
			get
			{
				return this._VDefProjectID;
			}
			set
			{
				this._VDefProjectID = value;
			}
		}
		#endregion
		#region VAPAccountLocationID
		public abstract class vAPAccountLocationID : PX.Data.BQL.BqlInt.Field<vAPAccountLocationID> { }
		protected Int32? _VAPAccountLocationID;

		/// <summary>
		/// The identifier of the AP location of the vendor location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Location.LocationID" /> field.
		/// </value>
		[PXDBInt()]
		[PXDefault()]
		public virtual Int32? VAPAccountLocationID
		{
			get
			{
				return this._VAPAccountLocationID;
			}
			set
			{
				this._VAPAccountLocationID = value;
			}
		}
		#endregion
		#region VAPAccountID
		public abstract class vAPAccountID : PX.Data.BQL.BqlInt.Field<vAPAccountID> { }
		protected Int32? _VAPAccountID;

		/// <summary>
		/// The identifier of the AP account of the vendor location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Account.accountID" /> field.
		/// </value>
        [Account(null, typeof(Search<Account.accountID,
                    Where2<Match<Current<AccessInfo.userName>>,
                         And<Account.active, Equal<True>,
                         And<Where<Current<GLSetup.ytdNetIncAccountID>, IsNull,
                          Or<Account.accountID, NotEqual<Current<GLSetup.ytdNetIncAccountID>>>>>>>>), DisplayName = "AP Account", Required = true,
			ControlAccountForModule = ControlAccountModule.AP)]
        public virtual Int32? VAPAccountID
		{
			get
			{
				return this._VAPAccountID;
			}
			set
			{
				this._VAPAccountID = value;
			}
		}
		#endregion
		#region VAPSubID
		public abstract class vAPSubID : PX.Data.BQL.BqlInt.Field<vAPSubID> { }
		protected Int32? _VAPSubID;

		/// <summary>
		/// The identifier of the AP subaccount of the vendor location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Sub.subID" /> field.
		/// </value>
		[SubAccount(typeof(Location.vAPAccountID), DisplayName = "AP Sub.", DescriptionField = typeof(Sub.description), Required = true)]
		[PXReferentialIntegrityCheck(CheckPoint = CheckPoint.BeforePersisting)]
		[PXForeignReference(typeof(Field<vAPSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? VAPSubID
		{
			get
			{
				return this._VAPSubID;
			}
			set
			{
				this._VAPSubID = value;
			}
		}
		#endregion
		#region VPaymentInfoLocationID
		public abstract class vPaymentInfoLocationID : PX.Data.BQL.BqlInt.Field<vPaymentInfoLocationID> { }
		protected Int32? _VPaymentInfoLocationID;

		/// <summary>
		/// The indentifier of the vendor payment info in the vendor location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Location.LocationID" /> field.
		/// </value>
		[PXDBInt()]
		[PXDefault()]
		public virtual Int32? VPaymentInfoLocationID
		{
			get
			{
				return this._VPaymentInfoLocationID;
			}
			set
			{
				this._VPaymentInfoLocationID = value;
			}
		}
		#endregion
		#region OverrideRemitAddress
		public abstract class overrideRemitAddress : PX.Data.BQL.BqlBool.Field<overrideRemitAddress> { }

		/// <summary>
		/// This field indicates whether the remit address is not the same as the default address for this location.
		/// </summary>
		[PXBool]
		[PXFormula(typeof(Switch<Case<Where<Location.vRemitAddressID, Equal<Location.vDefAddressID>>, False>, True>))]
		[PXUIField(DisplayName = "Override")]
		public virtual bool? OverrideRemitAddress { get; set; }
		#endregion
		#region IsRemitAddressSameAsMain

		/// <exclude />
		[Obsolete("Use OverrideRemitAddress instead")]
		public abstract class isRemitAddressSameAsMain : PX.Data.BQL.BqlBool.Field<isRemitAddressSameAsMain> { }
		protected bool? _IsRemitAddressSameAsMain;

		/// <exclude />
		[Obsolete("Use OverrideRemitAddress instead")]
		[PXBool()]
		[PXUIField(DisplayName = "Same as Main")]
		public virtual bool? IsRemitAddressSameAsMain
		{
			get { return OverrideRemitAddress != null ? !this.OverrideRemitAddress : null; }
		}
		#endregion
		#region VRemitAddressID
		public abstract class vRemitAddressID : PX.Data.BQL.BqlInt.Field<vRemitAddressID> { }
		protected Int32? _VRemitAddressID;

		/// <summary>
		/// The identifier of the remit address of the vendor location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Address.AddressID"/> field.
		/// </value>
		[PXDBInt()]
		[PXDBChildIdentity(typeof(Address.addressID))]
		[PXForeignReference(typeof(FK.RemitAddress))]
		public virtual Int32? VRemitAddressID
		{
			get
			{
				return this._VRemitAddressID;
			}
			set
			{
				this._VRemitAddressID = value;
			}
		}
		#endregion
		#region OverrideRemitContact
		public abstract class overrideRemitContact : PX.Data.BQL.BqlBool.Field<overrideRemitContact> { }

		/// <summary>
		/// This field indicates whether the remit contact is not the same as the default contact for this location.
		/// </summary>
		[PXBool]
		[PXFormula(typeof(Switch<Case<Where<Location.vRemitContactID, Equal<Location.vDefContactID>>, False>, True>))]
		[PXUIField(DisplayName = "Override")]
		public virtual bool? OverrideRemitContact { get; set; }
		#endregion
		#region IsRemitContactSameAsMain

		/// <exclude />
		[Obsolete("Use OverrideRemitContact instead")]
		public abstract class isRemitContactSameAsMain : PX.Data.BQL.BqlBool.Field<isRemitContactSameAsMain> { }
		protected bool? _IsRemitContactSameAsMain;

		/// <exclude />
		[Obsolete("Use OverrideRemitContact instead")]
		[PXBool()]
		[PXUIField(DisplayName = "Same as Main")]
		public virtual bool? IsRemitContactSameAsMain
		{
			get { return OverrideRemitContact != null ? !this.OverrideRemitContact : null; }
		}
		#endregion
		#region VRemitContactID
		public abstract class vRemitContactID : PX.Data.BQL.BqlInt.Field<vRemitContactID> { }
		protected Int32? _VRemitContactID;

		/// <summary>
		/// The identifier of the remit contact identifier of the vendor location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Contact.ContactID"/> field.
		/// </value>
		[PXDBInt()]
		[PXDBChildIdentity(typeof(Contact.contactID))]
		[PXForeignReference(typeof(FK.RemitContact))]
		public virtual Int32? VRemitContactID
		{
			get
			{
				return this._VRemitContactID;
			}
			set
			{
				this._VRemitContactID = value;
			}
		}
		#endregion
		#region VPaymentMethodID
		public abstract class vPaymentMethodID : PX.Data.BQL.BqlString.Field<vPaymentMethodID> { }
		protected String _VPaymentMethodID;

		/// <summary>
		/// The payment method indentifier of the vendor location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PaymentMethod.PaymentMethodID" /> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Method")]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID,
							Where<PaymentMethod.useForAP, Equal<True>,
							And<PaymentMethod.isActive, Equal<True>>>>),					
							DescriptionField = typeof(PaymentMethod.descr))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String VPaymentMethodID
		{
			get
			{
				return this._VPaymentMethodID;
			}
			set
			{
				this._VPaymentMethodID = value;
			}
		}
		#endregion
		#region VCashAccountID
		public abstract class vCashAccountID : PX.Data.BQL.BqlInt.Field<vCashAccountID> { }
		protected Int32? _VCashAccountID;

		/// <summary>
		/// The cash account indentifier of the vendor location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="CashAccount.CashAccountID" /> field.
		/// </value>
		[CashAccount(typeof(Location.vBranchID), typeof(Search2<CashAccount.cashAccountID,
						InnerJoin<PaymentMethodAccount, 
							On<PaymentMethodAccount.cashAccountID,Equal<CashAccount.cashAccountID>>>,
						Where2<Match<Current<AccessInfo.userName>>, 
							And<CashAccount.clearingAccount, Equal<False>,
							And<PaymentMethodAccount.paymentMethodID,Equal<Current<Location.vPaymentMethodID>>,
							And<PaymentMethodAccount.useForAP,Equal<True>>>>>>), 
							Visibility = PXUIVisibility.Visible)]		
		public virtual Int32? VCashAccountID
		{
			get
			{
				return this._VCashAccountID;
			}
			set
			{
				this._VCashAccountID = value;
			}
		}
		#endregion
		#region VPaymentLeadTime
		public abstract class vPaymentLeadTime : PX.Data.BQL.BqlShort.Field<vPaymentLeadTime> { }
		protected Int16? _VPaymentLeadTime;

		/// <summary>
		/// The amount of the payment lead days for the vendor location.
		/// </summary>
		[PXDBShort(MinValue = -3660, MaxValue = 3660)]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Payment Lead Time (Days)")]
		public Int16? VPaymentLeadTime
		{
			get
			{
				return this._VPaymentLeadTime;
			}
			set
			{
				this._VPaymentLeadTime = value;
			}
		}
		#endregion		
		#region VPaymentByType
		public abstract class vPaymentByType : PX.Data.BQL.BqlInt.Field<vPaymentByType> { }
		protected int? _VPaymentByType;

		/// <summary>
		/// An option that defines when a vendor should be paid at this location.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in the <see cref="APPaymentBy.List"/> class.
		/// The default value is <see cref="APPaymentBy.DueDate"/>.
		/// </value>
		[PXDBInt()]
		[PXDefault(APPaymentBy.DueDate)]
		[APPaymentBy.List]
		[PXUIField(DisplayName = "Payment By")]
		public int? VPaymentByType
		{
			get
			{
				return this._VPaymentByType;
			}
			set
			{
				this._VPaymentByType = value;
			}
		}
		#endregion
		#region VSeparateCheck
		public abstract class vSeparateCheck : PX.Data.BQL.BqlBool.Field<vSeparateCheck> { }
		protected Boolean? _VSeparateCheck;

		/// <summary>
		/// This field indicates whether a vendor should pay separately in this location.
		/// </summary>
		/// <value>
		/// The default value is <see langword="false"/>.
		/// </value>
		[PXDBBool()]
		[PXUIField(DisplayName = "Pay Separately")]
		[PXDefault(false)]
		public virtual Boolean? VSeparateCheck
		{
			get
			{
				return this._VSeparateCheck;
			}
			set
			{
				this._VSeparateCheck = value;
			}
		}
		#endregion
		#region VPrepaymentPct
		public abstract class vPrepaymentPct : Data.BQL.BqlDecimal.Field<vPrepaymentPct>
		{
		}

		/// <summary>
		/// The amount of prepayment percentage for the vendor location.
		/// </summary>
		/// <value>
		/// The default value is 100.
		/// </value>
		[PXDBDecimal(6, MinValue = 0, MaxValue = 100)]
		[PXUIField(DisplayName = "Prepayment Percent")]
		[PXDefault(TypeCode.Decimal, "100.0")]
		public virtual decimal? VPrepaymentPct
		{
			get;
			set;
		}
		#endregion
		#region VAllowAPBillBeforeReceipt
		public abstract class vAllowAPBillBeforeReceipt : PX.Data.BQL.BqlBool.Field<vAllowAPBillBeforeReceipt> { }

		/// <summary>
		/// This field indicates that, in this location, a vendor can create a bill before creating a receipt.
		/// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = "Allow AP Bill Before Receipt")]
		[PXDefault(false)]
		public virtual bool? VAllowAPBillBeforeReceipt
		{
			get;
			set;
		}
		#endregion
		#region IsAPAccountSameAsMain
		public abstract class isAPAccountSameAsMain : PX.Data.BQL.BqlBool.Field<isAPAccountSameAsMain> { }
		protected bool? _IsAPAccountSameAsMain;

		/// <summary>
		/// This field indicates that the vendor AP location is not the same as this location.
		/// </summary>
		[PXBool()]
		[PXUIField(DisplayName = "Same As Default Location's")]
		[PXFormula(typeof(Switch<Case<Where<locationID, Equal<vAPAccountLocationID>>, False>, True>))]
		public virtual bool? IsAPAccountSameAsMain
		{
			get
			{
				return this._IsAPAccountSameAsMain;
			}
			set
			{
				this._IsAPAccountSameAsMain = value;
			}
		}
		#endregion
		#region IsAPPaymentInfoSameAsMain
		public abstract class isAPPaymentInfoSameAsMain : PX.Data.BQL.BqlBool.Field<isAPPaymentInfoSameAsMain> { }
		protected bool? _IsAPPaymentInfoSameAsMain;

		/// <summary>
		/// This field indicates tha the AP payment info in the default location is not the same as the one specified in this location.
		/// </summary>
		[PXBool()]
		[PXUIField(DisplayName = "Same As Default Location's")]
		[PXFormula(typeof(Switch<Case<Where<locationID, Equal<vPaymentInfoLocationID>>, False>, True>))]
		public virtual bool? IsAPPaymentInfoSameAsMain
		{
			get
			{
				return this._IsAPPaymentInfoSameAsMain;
			}
			set
			{
				this._IsAPPaymentInfoSameAsMain = value;
			}
		}
		#endregion



		//LocationAPAccountSub fields
		#region LocationAPAccountSubBAccountID
		public abstract class locationAPAccountSubBAccountID : PX.Data.BQL.BqlInt.Field<locationAPAccountSubBAccountID> { }
		protected Int32? _LocationAPAccountSubBAccountID;

		/// <summary>
		/// The identifier of the AP subaccount of the business account.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="LocationARAccountSub.BAccountID" /> field.
		/// </value>
		[PXDBInt(BqlField=typeof(LocationAPAccountSub.bAccountID))]
		[PXExtraKey()]
		public virtual Int32? LocationAPAccountSubBAccountID
		{
			get
			{
				return null;
			}
			set
			{
			}
		}
		#endregion
		#region APAccountID
		public abstract class aPAccountID : PX.Data.BQL.BqlInt.Field<aPAccountID> { }
		protected Int32? _APAccountID;

		/// <summary>
		/// The identifier of the AP account.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Account.AccountID" /> field.
		/// </value>
        [Account(null, typeof(Search<Account.accountID,
                    Where2<Match<Current<AccessInfo.userName>>,
                         And<Account.active, Equal<True>,
                         And<Account.isCashAccount, Equal<False>,
                         And<Where<Current<GLSetup.ytdNetIncAccountID>, IsNull,
                          Or<Account.accountID, NotEqual<Current<GLSetup.ytdNetIncAccountID>>>>>>>>>), DisplayName = "AP Account", BqlField = typeof(LocationAPAccountSub.vAPAccountID))]
		public virtual Int32? APAccountID
		{
			get
			{
				return this._APAccountID;
			}
			set
			{
				this._APAccountID = value;
			}
		}
		#endregion
		#region APSubID
		public abstract class aPSubID : PX.Data.BQL.BqlInt.Field<aPSubID> { }
		protected Int32? _APSubID;

		/// <summary>
		/// The identifier of the AP subaccount.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Sub.subID" /> field.
		/// </value>
		[SubAccount(typeof(Location.aPAccountID), BqlField = typeof(LocationAPAccountSub.vAPSubID), DisplayName = "AP Sub.", DescriptionField = typeof(Sub.description))]
		public virtual Int32? APSubID
		{
			get
			{
				return this._APSubID;
			}
			set
			{
				this._APSubID = value;
			}
		}
		#endregion
		#region APRetainageAcctID
		public abstract class aPRetainageAcctID : PX.Data.BQL.BqlInt.Field<aPRetainageAcctID> { }

		/// <summary>
		/// The identifier of the AP retainage account.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="LocationAPAccountSub.VRetainageAcctID" /> field.
		/// </value>
		[Account(DisplayName = "Retainage Payable Account",
			Visibility = PXUIVisibility.Visible,
			DescriptionField = typeof(Account.description),
			BqlField = typeof(LocationAPAccountSub.vRetainageAcctID))]
		public virtual Int32? APRetainageAcctID
		{
			get;
			set;
		}
		#endregion
		#region APRetainageSubID
		public abstract class aPRetainageSubID : PX.Data.BQL.BqlInt.Field<aPRetainageSubID> { }

		/// <summary>
		/// The identifier of the AP retainage subaccount.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="LocationAPAccountSub.VRetainageSubID" /> field.
		/// </value>
		[SubAccount(typeof(Location.vRetainageAcctID),
			DisplayName = "Retainage Payable Sub.",
			Visibility = PXUIVisibility.Visible,
			DescriptionField = typeof(Sub.description),
			BqlField = typeof(LocationAPAccountSub.vRetainageSubID))]
		public virtual Int32? APRetainageSubID
		{
			get;
			set;
		}
		#endregion



		//LocationARAccountSub fields
		#region LocationARAccountSubBAccountID
		public abstract class locationARAccountSubBAccountID : PX.Data.BQL.BqlInt.Field<locationARAccountSubBAccountID> { }
		protected Int32? _LocationARAccountSubBAccountID;

		/// <summary>
		/// The identifier of the AR subaccount of the business account.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="LocationARAccountSub.BAccountID" /> field.
		/// </value>
		[PXDBInt(BqlField = typeof(LocationARAccountSub.bAccountID))]
		[PXExtraKey()]
		public virtual Int32? LocationARAccountSubBAccountID
		{
			get
			{
				return null;
			}
			set
			{
			}
		}
		#endregion
		#region ARAccountID
		public abstract class aRAccountID : PX.Data.BQL.BqlInt.Field<aRAccountID> { }
		protected Int32? _ARAccountID;

		/// <summary>
		/// The identifier of the AR account.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Account.AccountID" /> field.
		/// </value>
        [Account(null, typeof(Search<Account.accountID,
                    Where2<Match<Current<AccessInfo.userName>>,
                         And<Account.active, Equal<True>,
                         And<Account.isCashAccount, Equal<False>,
                         And<Where<Current<GLSetup.ytdNetIncAccountID>, IsNull,
                          Or<Account.accountID, NotEqual<Current<GLSetup.ytdNetIncAccountID>>>>>>>>>), DisplayName = "AR Account", BqlField = typeof(LocationARAccountSub.cARAccountID))]
		public virtual Int32? ARAccountID
		{
			get
			{
				return this._ARAccountID;
			}
			set
			{
				this._ARAccountID = value;
			}
		}
		#endregion
		#region ARSubID
		public abstract class aRSubID : PX.Data.BQL.BqlInt.Field<aRSubID> { }
		protected Int32? _ARSubID;

		/// <summary>
		/// The identifier of the AR account.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="LocationARAccountSub.CARSubID" /> field.
		/// </value>
		[SubAccount(typeof(Location.aRAccountID), BqlField = typeof(LocationARAccountSub.cARSubID), DisplayName = "AR Sub.", DescriptionField = typeof(Sub.description))]
		public virtual Int32? ARSubID
		{
			get
			{
				return this._ARSubID;
			}
			set
			{
				this._ARSubID = value;
			}
		}
		#endregion
		#region ARRetainageAcctID
		public abstract class aRRetainageAcctID : PX.Data.BQL.BqlInt.Field<aRRetainageAcctID> { }

		/// <summary>
		/// The identifier of the AR retainage account.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="LocationARAccountSub.cRetainageAcctID" /> field.
		/// </value>
		[Account(DisplayName = "Retainage Receivable Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), BqlField = typeof(LocationARAccountSub.cRetainageAcctID))]
		public virtual int? ARRetainageAcctID
		{
			get;
			set;
		}
		#endregion
		#region ARRetainageSubID
		public abstract class aRRetainageSubID : PX.Data.BQL.BqlInt.Field<aRRetainageSubID> { }

		/// <summary>
		/// The identifier of the AR retainage subaccount.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="LocationARAccountSub.CRetainageSubID" /> field.
		/// </value>
		[SubAccount(typeof(Location.aRRetainageAcctID), DisplayName = "Retainage Receivable Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), BqlField = typeof(LocationARAccountSub.cRetainageSubID))]
		public virtual int? ARRetainageSubID
		{
			get;
			set;
		}
		#endregion



		//LocationAPPaymentInfo fields
		#region LocationAPPaymentInfoBAccountID
		public abstract class locationAPPaymentInfoBAccountID : PX.Data.BQL.BqlInt.Field<locationAPPaymentInfoBAccountID> { }
		protected Int32? _LocationAPPaymentInfoBAccountID;

		/// <summary>
		/// The identifier of AP payment info of the business account.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="LocationAPPaymentInfo.bAccountID" /> field.
		/// </value>
		[PXDBInt(BqlField = typeof(LocationAPPaymentInfo.bAccountID))]
		[PXExtraKey()]
		public virtual Int32? LocationAPPaymentInfoBAccountID
		{
			get
			{
				return null;
			}
			set
			{
			}
		}
		#endregion
		#region RemitAddressID
		[Obsolete]
		public abstract class remitAddressID : PX.Data.BQL.BqlInt.Field<remitAddressID> { }
		protected Int32? _RemitAddressID;

		/// <exclude />
		[Obsolete]
		[PXInt]
		public virtual Int32? RemitAddressID
		{
			get
			{
				return this._RemitAddressID;
			}
			set
			{
				this._RemitAddressID = value;
			}
		}
		#endregion
		#region RemitContactID
		[Obsolete]
		public abstract class remitContactID : PX.Data.BQL.BqlInt.Field<remitContactID> { }
		protected Int32? _RemitContactID;

		/// <exclude />
		[Obsolete]
		[PXInt]
		public virtual Int32? RemitContactID
		{
			get
			{
				return this._RemitContactID;
			}
			set
			{
				this._RemitContactID = value;
			}
		}
		#endregion
		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
		protected String _PaymentMethodID;

		/// <summary>
		/// The identifier of the payment method.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="LocationAPPaymentInfo.VPaymentMethodID" /> field.
		/// </value>
		[PXDBString(10, IsUnicode = true, BqlField = typeof(LocationAPPaymentInfo.vPaymentMethodID))]
		[PXUIField(DisplayName = "Payment Method")]
		public virtual String PaymentMethodID
		{
			get
			{
				return this._PaymentMethodID;
			}
			set
			{
				this._PaymentMethodID = value;
			}
		}
		#endregion
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }
		protected Int32? _CashAccountID;

		/// <summary>
		/// The identifier of the cash account.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="LocationAPPaymentInfo.VCashAccountID" /> field.
		/// </value>
		[CashAccount(typeof(Search2<CashAccount.cashAccountID,
						InnerJoin<PaymentMethodAccount,
							On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>>>,
						Where2<Match<Current<AccessInfo.userName>>,
							And<CashAccount.clearingAccount, Equal<False>,
							And<PaymentMethodAccount.paymentMethodID, Equal<Current<Location.vPaymentMethodID>>,
							And<PaymentMethodAccount.useForAP, Equal<True>>>>>>), 
							BqlField = typeof(LocationAPPaymentInfo.vCashAccountID), 
							Visibility = PXUIVisibility.Visible)]
		public virtual Int32? CashAccountID
		{
			get
			{
				return this._CashAccountID;
			}
			set
			{
				this._CashAccountID = value;
			}
		}
		#endregion
		#region PaymentLeadTime
		public abstract class paymentLeadTime : PX.Data.BQL.BqlShort.Field<paymentLeadTime> { }
		protected Int16? _PaymentLeadTime;

		/// <summary>
		/// The amount of payment lead days.
		/// </summary>
		[PXDBShort(BqlField = typeof(LocationAPPaymentInfo.vPaymentLeadTime), MinValue = 0, MaxValue = 3660)]
		[PXUIField(DisplayName = "Payment Lead Time (Days)")]
		public Int16? PaymentLeadTime
		{
			get
			{
				return this._PaymentLeadTime;
			}
			set
			{
				this._PaymentLeadTime = value;
			}
		}
		#endregion
		#region SeparateCheck
		public abstract class separateCheck : PX.Data.BQL.BqlBool.Field<separateCheck> { }
		protected Boolean? _SeparateCheck;

		/// <summary>
		/// This field indicates whether the vendor should pay separately.
		/// </summary>
		/// <value>
		/// The default value is <see langword="false"/>.
		/// </value>
		[PXDBBool(BqlField = typeof(LocationAPPaymentInfo.vSeparateCheck))]
		[PXUIField(DisplayName = "Pay Separately")]
		public virtual Boolean? SeparateCheck
		{
			get
			{
				return this._SeparateCheck;
			}
			set
			{
				this._SeparateCheck = value;
			}
		}
		#endregion
		#region PaymentByType
		public abstract class paymentByType : PX.Data.BQL.BqlInt.Field<paymentByType> { }
		protected int? _PaymentByType;

		/// <summary>
		/// The option that defines when the customer should be paid at this location.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in the <see cref="APPaymentBy.List"/> class.
		/// The default value is <see cref="APPaymentBy.DueDate"/>.
		/// </value>
		[PXDBInt(BqlField = typeof(LocationAPPaymentInfo.vPaymentByType))]
		[PXDefault(APPaymentBy.DueDate)]
		[APPaymentBy.List]
		[PXUIField(DisplayName = "Payment By")]
		public int? PaymentByType
		{
			get
			{
				return this._PaymentByType;
			}
			set
			{
				this._PaymentByType = value;
			}
		}
		#endregion



		//BAccount fields
		#region BAccountBAccountID
		public abstract class bAccountBAccountID : PX.Data.BQL.BqlInt.Field<bAccountBAccountID> { }
		protected Int32? _BAccountBAccountID;
		//should be BAccount not BAccountR
		/// <exclude />
		[PXDBInt(BqlField = typeof(BAccountR.bAccountID))]
		[PXExtraKey()]
		public virtual Int32? BAccountBAccountID
		{
			get
			{
				return null;
			}
			set
			{
			}
		}
		#endregion
		#region VDefAddressID
		public abstract class vDefAddressID : PX.Data.BQL.BqlInt.Field<vDefAddressID> { }
		protected Int32? _VDefAddressID;

		/// <summary>
		/// The identifier of the vendor's default address of the location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Address.AddressID"/> field.
		/// </value>
		[PXDBInt(BqlField = typeof(BAccountR.defAddressID))]
		[PXDefault(typeof(Select<BAccount, Where<BAccount.bAccountID, Equal<Current<Location.bAccountID>>>>), SourceField = typeof(BAccount.defAddressID))]
		public virtual Int32? VDefAddressID
		{
			get
			{
				return this._VDefAddressID;
			}
			set
			{
				this._VDefAddressID = value;
			}
		}
		#endregion
		#region VDefContactID
		public abstract class vDefContactID : PX.Data.BQL.BqlInt.Field<vDefContactID> { }
		protected Int32? _VDefContactID;

		/// <summary>
		/// The identifier of the vendor's default contact of the location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Contact.ContactID"/> field.
		/// </value>
		[PXDBInt(BqlField = typeof(BAccountR.defContactID))]
		[PXDefault(typeof(Select<BAccount, Where<BAccount.bAccountID, Equal<Current<Location.bAccountID>>>>), SourceField = typeof(BAccount.defContactID))]
		public virtual Int32? VDefContactID
		{
			get
			{
				return this._VDefContactID;
			}
			set
			{
				this._VDefContactID = value;
			}
		}
		#endregion



		//Company Location Properties
		#region CMPSalesSubID
		public abstract class cMPSalesSubID : PX.Data.BQL.BqlInt.Field<cMPSalesSubID> { }
		protected Int32? _CMPSalesSubID;

		/// <summary>
		/// The identifier of the sales subacount of the company location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Sub.SubID" /> field.
		/// </value>
		[SubAccount(DisplayName = "Sales Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? CMPSalesSubID
		{
			get
			{
				return this._CMPSalesSubID;
			}
			set
			{
				this._CMPSalesSubID = value;
			}
		}
		#endregion
		#region CMPExpenseSubID
		public abstract class cMPExpenseSubID : PX.Data.BQL.BqlInt.Field<cMPExpenseSubID> { }
		protected Int32? _CMPExpenseSubID;

		/// <summary>
		/// The identifier of the expense subaccount of the company location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Sub.SubID" /> field.
		/// </value>
		[SubAccount(DisplayName = "Expense Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? CMPExpenseSubID
		{
			get
			{
				return this._CMPExpenseSubID;
			}
			set
			{
				this._CMPExpenseSubID = value;
			}
		}
		#endregion
		#region CMPFreightSubID
		public abstract class cMPFreightSubID : PX.Data.BQL.BqlInt.Field<cMPFreightSubID> { }
		protected Int32? _CMPFreightSubID;

		/// <summary>
		/// The identifier of the freight subaccount of the company location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Sub.SubID" /> field.
		/// </value>
		[SubAccount(DisplayName = "Freight Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? CMPFreightSubID
		{
			get
			{
				return this._CMPFreightSubID;
			}
			set
			{
				this._CMPFreightSubID = value;
			}
		}
		#endregion
		#region CMPDiscountSubID
		public abstract class cMPDiscountSubID : PX.Data.BQL.BqlInt.Field<cMPDiscountSubID> { }
		protected Int32? _CMPDiscountSubID;

		/// <summary>
		/// The identifier of the discount subaccount of the company location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Sub.SubID" /> field.
		/// </value>
		[SubAccount(DisplayName = "Discount Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? CMPDiscountSubID
		{
			get
			{
				return this._CMPDiscountSubID;
			}
			set
			{
				this._CMPDiscountSubID = value;
			}
		}
		#endregion
        #region CMPGainLossSubID
        public abstract class cMPGainLossSubID : PX.Data.BQL.BqlInt.Field<cMPGainLossSubID> { }
        protected Int32? _CMPGainLossSubID;

		/// <summary>
		/// The identifier of the currency gain and loss subaccount of the company location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="Sub.SubID" /> field.
		/// </value>
        [SubAccount(DisplayName = "Currency Gain/Loss Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
        public virtual Int32? CMPGainLossSubID
        {
            get
            {
                return this._CMPGainLossSubID;
            }
            set
            {
                this._CMPGainLossSubID = value;
            }
        }
        #endregion
		#region CMPSiteID
		public abstract class cMPSiteID : PX.Data.BQL.BqlInt.Field<cMPSiteID> { }
		protected Int32? _CMPSiteID;

		/// <summary>
		/// The warehouse of the company location.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="INSite.siteID" /> field.
		/// </value>
		[PXDBInt()]
		[PXUIField(DisplayName = "Warehouse", Visibility = PXUIVisibility.Visible)]
		[PXDimensionSelector(SiteAttribute.DimensionName, typeof(INSite.siteID), typeof(INSite.siteCD), DescriptionField = typeof(INSite.descr))]
        [PXRestrictor(typeof(Where<INSite.active, Equal<True>>), IN.Messages.InactiveWarehouse, typeof(INSite.siteCD))]
        public virtual Int32? CMPSiteID
		{
			get
			{
				return this._CMPSiteID;
			}
			set
			{
				this._CMPSiteID = value;
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
		[PXDBCreatedDateTime]
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
		[PXDBLastModifiedDateTime]
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

		#region IsAddressSameAsMain
		[Obsolete("Use OverrideAddress instead")]
		public abstract class isAddressSameAsMain : PX.Data.BQL.BqlBool.Field<isAddressSameAsMain> { }

		[Obsolete("Use OverrideAddress instead")]
		[PXBool()]
		[PXUIField(DisplayName = "Same as Main")]
		public virtual bool? IsAddressSameAsMain
		{
			get { return OverrideAddress != null ? !this.OverrideAddress : null; }
		}
		#endregion
		#region Override Address
		public abstract class overrideAddress : PX.Data.BQL.BqlBool.Field<overrideAddress> { }

		/// <summary>
		/// If set to <c>true</c>, indicates that the address
		/// overrides the default <see cref="Address"/> record, which is
		/// referenced by <see cref="DefAddressID"/>.
		/// </summary>
		[PXBool]
		[PXFormula(typeof(Switch<Case<Where<defAddressID, Equal<vDefAddressID>>, False>, True>))]
		[PXUIField(DisplayName = "Override")]
		public virtual bool? OverrideAddress { get; set; }
		#endregion
		#region IsContactSameAsMain
		[Obsolete("Use OverrideContact instead")]
		public abstract class isContactSameAsMain : PX.Data.BQL.BqlBool.Field<isContactSameAsMain> { }

		[Obsolete("Use OverrideContact instead")]
		[PXBool()]
		[PXUIField(DisplayName = "Same as Main")]
		public virtual bool? IsContactSameAsMain
		{
			get { return OverrideContact != null ? !this.OverrideContact : null; }
		}
		#endregion
		#region Override Contact
		public abstract class overrideContact : PX.Data.BQL.BqlBool.Field<overrideContact> { }

		/// <summary>
		/// If set to <c>true</c>, indicates that the address
		/// overrides the default <see cref="Contact"/> record, which is
		/// referenced by <see cref="DefContactID"/>.
		/// </summary>
		[PXBool]
		[PXFormula(typeof(Switch<Case<Where<defContactID, Equal<vDefContactID>>, False>, True>))]
		[PXUIField(DisplayName = "Override")]
		public virtual bool? OverrideContact { get; set; }
		#endregion

		public static string GetKeyImage(int? baccountID, int? locationID)
		{
			return String.Format("{0}:{1}, {2}:{3}", typeof(Location.bAccountID).Name, baccountID,
																typeof(Location.locationID).Name, locationID);
		}

		public string GetKeyImage()
		{
			return GetKeyImage(BAccountID, LocationID);
		}

		public static string GetImage(int? baccountID, int? locationID)
		{
			return string.Format("{0}[{1}]",
								EntityHelper.GetFriendlyEntityName(typeof(Location)),
								GetKeyImage(baccountID, locationID));
		}

		public override string ToString()
		{
			return GetImage(BAccountID, LocationID);
		}
	}
	#region LocType Attribute
	public class LocTypeList
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { CompanyLoc, VendorLoc, CustomerLoc, CombinedLoc, EmployeeLoc },
				new string[] { Messages.CompanyLoc, Messages.VendorLoc, Messages.CustomerLoc, Messages.CombinedLoc, Messages.EmployeeLoc }) { ; }
		}

		public const string CompanyLoc  = "CP";
		public const string VendorLoc   = "VE";
		public const string CustomerLoc = "CU";
		public const string CombinedLoc = "VC";
		public const string EmployeeLoc = "EP";

		public class companyLoc : PX.Data.BQL.BqlString.Constant<companyLoc>
		{
			public companyLoc() : base(CompanyLoc) { ;}
		}

		public class vendorLoc : PX.Data.BQL.BqlString.Constant<vendorLoc>
		{
			public vendorLoc() : base(VendorLoc) { ;}
		}

		public class customerLoc : PX.Data.BQL.BqlString.Constant<customerLoc>
		{
			public customerLoc() : base(CustomerLoc) { ;}
		}

		public class combinedLoc : PX.Data.BQL.BqlString.Constant<combinedLoc>
		{
			public combinedLoc() : base(CombinedLoc) { ;}
		}

		public class employeeLoc : PX.Data.BQL.BqlString.Constant<employeeLoc>
		{
			public employeeLoc() : base(EmployeeLoc) { ;}
		}
	}
	#endregion

	public interface ILocation
	{
		int? LocationID { get; set; }
		bool? IsActive { get; set; }

		int? CARAccountID { get; set; }
		int? CARSubID { get; set; }
		int? CSalesAcctID { get; set; }
		int? CSalesSubID { get; set; }
		bool? IsARAccountSameAsMain { get; set; }

		int? VAPAccountID { get; set; }
		int? VAPSubID { get; set; }
		int? VExpenseAcctID { get; set; }
		int? VExpenseSubID { get; set; }
		bool? IsAPAccountSameAsMain { get; set; }
		int? CDiscountAcctID { get; set; }
		int? CDiscountSubID { get; set; }
		int? CFreightAcctID { get; set; }
		int? CFreightSubID { get; set; }
	}
}
